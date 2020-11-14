using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class agenda_asesorController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        CultureInfo miCulturaTime = new CultureInfo("en-US");

        public class listaAgenda
        {
            public string descripcion { get; set; }
            public string colorhx { get; set; }
        }
        // GET: agenda_asesor
        public ActionResult Index(int? menu, int? idAsesor, int? idped)
        {
            //Haciendo una validación de vista para no saltar directamente a la vista de Index, sino que regrese nuevamente a la vista de inicio
            if (Session["user_usuarioid"] != null)
            {
                var placa = "";
                int asesor_actual = 0;
                string sAsesorComercial = db.icb_sysparameter.Where(x => x.syspar_cod == "P88").Select(x => x.syspar_value)
                    .FirstOrDefault();
                int iAsesorComercial = Convert.ToInt32(sAsesorComercial);
                if (idped != null)
                {//buscamos el pedido
                    var pedidox = db.vpedido.Where(x => x.id == idped).FirstOrDefault();
                    //buscamos el vehiculo
                    var vehi = pedidox.planmayor;
                    if (!string.IsNullOrWhiteSpace(vehi))
                    {
                        //verificamos si el vehiculo TIENE placa
                        if (!string.IsNullOrWhiteSpace(pedidox.icb_vehiculo.plac_vh))
                        {
                            placa = pedidox.icb_vehiculo.plac_vh;
                        }
                    }
                    asesor_actual = Convert.ToInt32(db.vpedido.Where(x => x.id == idped).Select(x => x.vendedor)
                        .FirstOrDefault());
                    ViewBag.asesor_actual_id = asesor_actual;
                    ViewBag.asesor_id =
                        new SelectList(db.users.Where(x => x.user_id == asesor_actual && x.rol_id == iAsesorComercial),
                            "user_id", "user_nombre", asesor_actual);
                    string sEntregaVh = db.icb_sysparameter.Where(x => x.syspar_cod == "P91" || x.syspar_cod == "P123")
                        .Select(x => x.syspar_value).FirstOrDefault();
                    int iEntregaVh = Convert.ToInt32(sEntregaVh);
                    ViewBag.tipoorigen =
                        new SelectList(db.vmotagenda.Where(x => x.id == iEntregaVh), "id", "descripcion");
                }
                else if (Session["user_rolid"] != null && Session["user_rolid"].ToString() != sAsesorComercial)
                {
                    ViewBag.asesor_id = new SelectList(db.users.Where(x => x.rol_id == iAsesorComercial), "user_id",
                        "user_nombre", idAsesor);
                    ViewBag.tipoorigen = new SelectList(db.vmotagenda, "id", "descripcion");
                }
                else
                {
                    asesor_actual = Convert.ToInt32(Session["user_usuarioid"]);
                    ViewBag.asesor_actual = Session["user_rolid"].ToString();
                    ViewBag.asesor_id = new SelectList(db.users.Where(x => x.rol_id == iAsesorComercial), "user_id",
                        "user_nombre", asesor_actual);
                    ViewBag.tipoorigen = new SelectList(db.vmotagenda, "id", "descripcion");
                }
                ViewBag.placa = placa;

                ViewBag.origenes = db.vmotagenda.ToList();

                //Buscando Tipos Citas Agenda
                ViewBag.ttiposcitas = (from tp in db.ttipocita_agenda where tp.estado select new listaAgenda { descripcion = tp.descripcion, colorhx = tp.colorhx }).ToList();

                BuscarFavoritos(menu);
                return View();
            }

            return RedirectToAction("Login", "Login");
            //Leo, se hace una parametrización de código
        }

        // POST: agenda_asesor/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(agenda_asesor agenda_asesor, int? menu)
        {
            //Se crea una variable para llamar la función de mensaje de texto
            MensajesDeTexto mensajeTexto = new MensajesDeTexto();
            // si este campo trae un id que es alistamiento.
            string sIdPed = Request["idpedido_txt"];
            string celular = Request["celular"];
            string placa = Request["placa"];
            string fecha = Request["desde"];
            int iIped = 0;
            string planmayor = "";
            if (!string.IsNullOrWhiteSpace(sIdPed))
            {
                bool convertir = int.TryParse(sIdPed, out iIped);
            }

            if (ModelState.IsValid)
            {
                bool citas = CalcularDisponible(agenda_asesor.asesor_id, DateTime.Now, agenda_asesor.desde,
                    agenda_asesor.hasta);
                if (citas)
                {
                    if (agenda_asesor.hasta > agenda_asesor.desde)
                    {
                        int result1 = 0;
                        int result2 = 0;
                        using (DbContextTransaction dbTrn = db.Database.BeginTransaction())
                        {
                            try
                            {
                                if (iIped > 0)
                                {
                                    // Registrar evento de Programacion Entrega
                                    string sEventoEntregaVh = db.icb_sysparameter.Where(x => x.syspar_cod == "P35")
                                        .Select(x => x.syspar_value).FirstOrDefault();
                                    int iEventoEntregaVh = Convert.ToInt32(sEventoEntregaVh);
                                    icb_tpeventos buscarEvento =
                                        db.icb_tpeventos.FirstOrDefault(x => x.tpevento_id == iEventoEntregaVh);


                                    //valido si el campo que me trae de placa pertenece a algún vehículo
                                    if (!string.IsNullOrWhiteSpace(placa))
                                    {
                                        icb_vehiculo existeplaca = db.icb_vehiculo.Where(d => d.plac_vh == placa || d.plan_mayor == placa).FirstOrDefault();
                                        if (existeplaca != null)
                                        {
                                            planmayor = existeplaca.plan_mayor;
                                        }
                                        else
                                        {
                                            planmayor = agenda_asesor.placa;
                                        }

                                    }
                                    else
                                    {
                                        planmayor = agenda_asesor.placa;
                                    }
                                    // Validar que no exista fecha o actualizarla
                                    icb_vehiculo_eventos buscarEventoRealizado = db.icb_vehiculo_eventos.FirstOrDefault(x =>
                                        (x.planmayor == planmayor || x.vin == planmayor || x.placa == planmayor) &&
                                        x.id_tpevento == iEventoEntregaVh);
                                    // Validar que no exista fecha o actualizarla
                                    icb_terceros buscarTercero =
                                        db.icb_terceros.FirstOrDefault(x => x.doc_tercero == agenda_asesor.cliente);
                                    // Si llega aqui significa que los datos estan completos

                                    vw_pendientesEntrega pendAlis = db.vw_pendientesEntrega.Where(x => x.id == iIped).FirstOrDefault();
                                    if (buscarEventoRealizado == null)
                                    {
                                        icb_vehiculo_eventos crearEvento = new icb_vehiculo_eventos
                                        {
                                            eventofec_creacion = DateTime.Now,
                                            eventouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            evento_nombre = buscarEvento.tpevento_nombre,
                                            evento_estado = true,
                                            bodega_id = Convert.ToInt32(Session["user_bodega"]),
                                            planmayor = pendAlis.planmayor,
                                            id_tpevento = iEventoEntregaVh,
                                            fechaevento = agenda_asesor.desde,
                                            placa = agenda_asesor.placa
                                        };
                                        if (buscarTercero != null)
                                        {
                                            crearEvento.terceroid = buscarTercero.tercero_id;
                                        }
                                        else
                                        {
                                            crearEvento.terceroid = null;
                                        }

                                        crearEvento.ubicacion = pendAlis.ubivh_id;
                                        db.icb_vehiculo_eventos.Add(crearEvento);
                                        result1 = db.SaveChanges();
                                    }
                                    else
                                    {
                                        buscarEventoRealizado.eventouserid_actualizacion =
                                            Convert.ToInt32(Session["user_usuarioid"]);
                                        buscarEventoRealizado.eventofec_actualizacion = DateTime.Now;
                                        buscarEventoRealizado.bodega_id = Convert.ToInt32(Session["user_bodega"]);
                                        buscarEventoRealizado.fechaevento = agenda_asesor.desde;
                                        if (buscarTercero != null)
                                        {
                                            buscarEventoRealizado.terceroid = buscarTercero.tercero_id;
                                        }
                                        else
                                        {
                                            buscarEventoRealizado.terceroid = null;
                                        }

                                        buscarEventoRealizado.ubicacion = pendAlis.ubivh_id;
                                        db.Entry(buscarEventoRealizado).State = EntityState.Modified;
                                        result1 = db.SaveChanges();
                                    }
                                }

                                // Traer el evento de entrega de vehículo, validar si existe y en caso tal actualizarlo.
                                string sEntregaVh = db.icb_sysparameter.Where(x => x.syspar_cod == "P91")
                                    .Select(x => x.syspar_value).FirstOrDefault();
                                int iEntregaVh = Convert.ToInt32(sEntregaVh);
                                // validar si ya existe 
                                agenda_asesor buscarCita = db.agenda_asesor.FirstOrDefault(x =>
                                    x.placa == planmayor && x.tipoorigen == iEntregaVh);

                                string titulotipoorigen = "";
                                titulotipoorigen = db.vmotagenda.Where(d => d.id == agenda_asesor.tipoorigen).Select(d => d.descripcion).FirstOrDefault();
                                if (buscarCita == null)
                                {
                                    agenda_asesor.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                    agenda_asesor.fec_creacion = DateTime.Now;
                                    agenda_asesor.titulo = titulotipoorigen;
                                    agenda_asesor.estado = "Activa";
                                    if (!string.IsNullOrWhiteSpace(planmayor))
                                    {
                                        agenda_asesor.placa = planmayor;
                                    }

                                    db.agenda_asesor.Add(agenda_asesor);
                                }
                                else
                                {
                                    buscarCita.desde = agenda_asesor.desde;
                                    buscarCita.hasta = agenda_asesor.hasta;
                                    buscarCita.titulo = titulotipoorigen;

                                    if (!string.IsNullOrWhiteSpace(planmayor))
                                    {
                                        buscarCita.placa = planmayor;
                                    }

                                    buscarCita.asesor_id = agenda_asesor.asesor_id;
                                    buscarCita.descripcion = agenda_asesor.descripcion;
                                    buscarCita.cliente = agenda_asesor.cliente;
                                    buscarCita.fec_actualizacion = DateTime.Now;
                                    buscarCita.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                                    db.Entry(buscarCita).State = EntityState.Modified;
                                }
                                result2 = db.SaveChanges();
                                dbTrn.Commit();
                            }
                            catch (Exception ex)
                            {
                                Exception exp = ex;
                                dbTrn.Rollback();
                            }
                        }

                        if (result2 > 0)
                        {
                            string textomensaje = "";
                            switch (agenda_asesor.tipoorigen)
                            {
                                case 1:
                                    textomensaje = "Se ha creado correctamente una cita de prospección para el día: " + fecha;
                                    break;
                                case 2:
                                    textomensaje = "Se ha agendado una cita de Test Drive para el día: " + fecha;
                                    break;
                                case 3:
                                    textomensaje = "Se ha agendado una visita para el día: " + fecha;
                                    break;
                                case 4:
                                    textomensaje = "El vehículo se encuentra matriculado y está asociado con la placa: " + placa +
                                " y está listo para ser entregado el día: " + fecha;
                                    break;
                                case 5:
                                    textomensaje = "Se ha agendado una cita para trabajo externo para el día: " + fecha;
                                    break;
                                case 6:
                                    textomensaje = "Se ha agendado una visita al concesionario para el día: " + fecha;
                                    break;
                                default:
                                    break;
                            }
                            TempData["mensaje"] = " Cita creada correctamente";
                            mensajeTexto.enviarmensaje(celular,
                                textomensaje);
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error al crear la Cita, por favor valide los datos";
                        }
                    }
                    else
                    {
                        TempData["mensaje_error"] =
                            "La fecha final de la cita debe ser mayor a la inicial, por favor valide";
                    }
                }
                else
                {
                    TempData["mensaje_error"] =
                        "Ya tiene una cita agendada para el día y hora ingresados, por favor valide";
                }
            }

            if (Request["idpedido_txt"] != null && Request["idpedido_txt"].Trim() != "")
            {
                return RedirectToAction("Index", new { menu, idped = Request["idpedido_txt"].Trim() });
            }

            return RedirectToAction("Index", new { menu });
        }

        public JsonResult ValidarCreacionCita(int? asesor_id, DateTime? desde, DateTime? hasta, int? documento_cliente)
        {
            if (asesor_id == null || desde == null || hasta == null)
            {
                return Json(
                    new
                    {
                        done = false,
                        error_message = "Debe digitar un asesor con fechas para consultar su disponibilidad"
                    }, JsonRequestBehavior.AllowGet);
            }

            icb_terceros usuariocreado = (from user in db.icb_terceros where user.doc_tercero == documento_cliente.ToString() select user).FirstOrDefault();
            if (usuariocreado == null)
            {
                return Json(
                   new
                   {
                       done = false,
                       error_message = "El usuario a agendar debe estar creado como tercero prospecto"
                   }, JsonRequestBehavior.AllowGet);
            }

            bool citas = CalcularDisponible(asesor_id ?? 0, DateTime.Now, desde ?? DateTime.Now, hasta ?? DateTime.Now);
            if (citas)
            {
                if (hasta > desde)
                {
                    return Json(new { done = true }, JsonRequestBehavior.AllowGet);
                }

                return Json(
                    new
                    {
                        done = false,
                        error_message = "La fecha final de la cita debe ser mayor a la inicial, por favor valide"
                    }, JsonRequestBehavior.AllowGet);
            }
            else
            {

                return Json(
                    new
                    {
                        done = false,
                        error_message = "Ya tiene una cita agendada para el día y hora ingresados, por favor valide"
                    }, JsonRequestBehavior.AllowGet);
            }
                  
        }

        public JsonResult validarCreadorCita(int id)
        {
            //permiso de cambiar estado
            int permiso = 0;
            rolpermisos permiso2 = db.rolpermisos.Where(d => d.codigo == "P34").FirstOrDefault();
            if (permiso2 != null)
            {
                permiso = permiso2.id;
            }
            var data = (from aa in db.agenda_asesor
                        where aa.id == id
                        select new
                        {
                            aa.asesor_id,
                            aa.userid_creacion,
                            aa.estado,
                            aa.desde,
                            aa.hasta

                        }).FirstOrDefault();

            var fechades = data.desde.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));

            var fechahas = data.hasta.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
            if (data.asesor_id == data.userid_creacion)
            {
                return Json(new { valor = true, estate = data.estado,desde=fechades,hasta=fechahas }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (Session["user_usuarioid"] != null)
                {
                    int usuario = Convert.ToInt32(Session["user_usuarioid"]);
                    //busqueda de persona
                    users usuarii = db.users.Where(d => d.user_id == usuario).FirstOrDefault();
                    if (usuarii != null)
                    {
                        int rol = usuarii.rols.rolacceso.Where(d => d.idrol == usuarii.rol_id && d.idpermiso == permiso).Count();
                        if (rol > 0)
                        {
                            return Json(new { valor = true, estate = data.estado, desde = fechades, hasta = fechahas }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                //veo si el rol de la persona tiene el permiso para poder modificar la cita

                return Json(new { valor = false, estate = data.estado, desde = fechades, hasta = fechahas }, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult datosPedido(int idp)
        {
            vw_pendientesEntrega x = db.vw_pendientesEntrega.FirstOrDefault(t => t.id == idp);
            if (x == null)
            {
                x= db.vw_pendientesEntrega.FirstOrDefault(t => t.numero == idp);
            }
            agendaAlistamientoModel alist = new agendaAlistamientoModel
            {
                idpedido = x.id,
                planMayorVh = x.planmayor,
                cedulaVh = x.doc_tercero,
                placaVh = x.plac_vh
                
            };
            return Json(alist, JsonRequestBehavior.AllowGet);
        }

        // POST: agenda_asesor/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(agenda_asesor agenda_asesor, int? menu)
        {
            if (ModelState.IsValid)
            {
                //var citas = db.agenda_asesor.FirstOrDefault(
                //    x => x.asesor_id == agenda_asesor.asesor_id
                //    && (agenda_asesor.desde >= x.desde && x.hasta >= agenda_asesor.desde)
                //    && (agenda_asesor.hasta >= x.desde && x.hasta >= agenda_asesor.hasta)
                //    );

                bool citas = true;
                if (agenda_asesor.estado == "Reprogramada")
                {
                    citas = CalcularDisponible(agenda_asesor.asesor_id, DateTime.Now, agenda_asesor.desde,
                        agenda_asesor.hasta);
                }

                if (citas)
                {
                    agenda_asesor agenda = db.agenda_asesor.Find(agenda_asesor.id);
                    agenda.estado = agenda_asesor.estado;
                    agenda.asesor_id = agenda_asesor.asesor_id;
                    agenda.descripcion = agenda_asesor.descripcion;
                    agenda.desde = agenda_asesor.desde;
                    agenda.hasta = agenda_asesor.hasta;
                    agenda.motivo = agenda_asesor.motivo;
                    agenda.titulo = agenda_asesor.titulo;
                    agenda.fec_actualizacion = DateTime.Now;
                    agenda.bodega = Convert.ToInt32(Session["user_bodega"]);
                    agenda.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    db.Entry(agenda).State = EntityState.Modified;
                    try
                    {
                        int result = db.SaveChanges();
                        if (result == 1)
                        {
                            TempData["mensaje"] = " Cita editada correctamente";
                        }
                        else
                        {
                            TempData["mensaje_error"] = " Error al editar la Cita, por favor valide los datos";
                        }
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        Exception raise = dbEx;
                        foreach (DbEntityValidationResult validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (DbValidationError validationError in validationErrors.ValidationErrors)
                            {
                                string message = string.Format("{0}:{1}",
                                validationErrors.Entry.Entity,
                                validationError.ErrorMessage);
                                // raise a new exception nesting
                                // the current instance as InnerException
                                raise = new InvalidOperationException(message, raise);
                                TempData["mensaje_error"] = raise;
                                return RedirectToAction("Index", new { menu });
                            }
                        }

                        throw raise;
                    }
                }
                else
                {
                    TempData["mensaje_error"] =
                        "Ya tiene una cita agendada para el rango de fecha y hora ingresados, por favor valide";
                }
            }

            return RedirectToAction("Index", new { menu });
        }

        public ActionResult Posponer(agenda_asesor agenda_asesor, int? menu)
        {
            DateTime fecha = DateTime.Now;
            string todas = Request["todas"];
            string tiempo = Request["tiempo"];

            if (!string.IsNullOrEmpty(todas))
            {
                System.Collections.Generic.List<agenda_asesor> citasDelMes = db.agenda_asesor.Where(x =>
                    x.asesor_id == agenda_asesor.asesor_id && x.hasta < fecha && x.estado == "Activa").ToList();
                foreach (agenda_asesor cita in citasDelMes)
                {
                    cita.desde = sumarFecha(tiempo, cita.desde);
                    cita.hasta = sumarFecha(tiempo, cita.hasta);
                    cita.estado = "Pospuesta";
                    cita.motivo = agenda_asesor.motivo;
                    db.Entry(cita).State = EntityState.Modified;
                }

                int result = db.SaveChanges();
                if (result > 0)
                {
                    TempData["mensaje"] = "Todas las citas se pospusieron Correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "Error al posponer las Citas, por favor valide";
                }
            }
            else
            {
                string[] citasSeleccionadas = Request["citasSeleccionadas"].Split(',');

                foreach (string item in citasSeleccionadas)
                {
                    int id = Convert.ToInt32(item);
                    agenda_asesor cita = db.agenda_asesor.FirstOrDefault(x => x.id == id);
                    cita.desde = sumarFecha(tiempo, cita.desde);
                    cita.hasta = sumarFecha(tiempo, cita.hasta);
                    cita.estado = "Pospuesta";
                    cita.motivo = agenda_asesor.motivo;
                    bool disponible = CalcularDisponible(agenda_asesor.asesor_id, fecha, cita.desde, cita.hasta);
                    if (!disponible)
                    {
                        db.Entry(cita).State = EntityState.Modified;
                        int result = db.SaveChanges();
                        if (result > 0)
                        {
                            TempData["mensaje"] = "Citas pospuestas Correctamente";
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error al posponer las Citas, por favor valide";
                        }
                    }
                    else
                    {
                        TempData["mensaje_error"] = "No se puede posponer la cita " + cita.titulo +
                                                    " por que ya tiene una cita agendada en el rango de tiempo: " +
                                                    cita.desde + " a " + cita.hasta;
                    }
                }
            }
            return RedirectToAction("Index", new { menu });
        }

        public JsonResult BuscarCitas(int asesor_id)
        {
            var citas = (from c in db.agenda_asesor
                         join t in db.icb_terceros
                             on c.cliente equals t.doc_tercero
                         //join m in db.icb_vehiculo
                         //on c.placa equals m.plan_mayor 
                         //join mv in db.modelo_vehiculo 
                         //on m.modvh_id equals mv.modvh_codigo
                         where c.asesor_id == asesor_id
                         select new
                         {
                             c.desde,
                             c.hasta,
                             c.titulo,
                             c.id,
                             c.motivo,
                             c.descripcion,
                             c.asesor_id,
                             c.estado,
                             c.fec_creacion,
                             placa = (c.placa != null) & (c.placa != string.Empty) ? c.placa : "",
                             c.tipoorigen,
                             c.cliente,
                             nombre_cliente = t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero + " " +
                                              t.segapellido_tercero,
                             modvh_nombre = c.placa != null && c.placa != string.Empty
                                 ? c.icb_vehiculo.modelo_vehiculo.modvh_nombre
                                 : "",
                             color = c.ttipocita_agenda.colorhx
                         }).ToList();

            var events = citas.GroupBy(x => x.id).Select(x => new
            {
                //title = x.Select(e => e.titulo).FirstOrDefault() + " " + x.Select(e => e.hasta.Hour.ToString("00")).FirstOrDefault() + ":" + x.Select(e => e.hasta.Minute.ToString("00")).FirstOrDefault() + ":00" + " " + x.Select(e => e.nombre_cliente).FirstOrDefault(),
                title = x.Select(e => e.nombre_cliente).FirstOrDefault() + "||" +
                        x.Select(e => e.modvh_nombre).FirstOrDefault(),
                start = x.Select(e => e.desde.ToString("yyyy/MM/dd")).FirstOrDefault() + " " +
                        x.Select(e => e.desde.Hour.ToString("00")).FirstOrDefault() + ":" +
                        x.Select(e => e.desde.Minute.ToString("00")).FirstOrDefault(),
                end = x.Select(e => e.hasta.ToString("yyyy/MM/dd")).FirstOrDefault() + " " +
                      x.Select(e => e.hasta.Hour.ToString("00")).FirstOrDefault() + ":" +
                      x.Select(e => e.hasta.Minute.ToString("00")).FirstOrDefault() + ":00",
                id = x.Key,
                colorCita = x.Select(e => e.color).FirstOrDefault(),
                descripcion = x.Select(e => e.descripcion).FirstOrDefault(),
                asesor_id = x.Select(e => e.asesor_id).FirstOrDefault(),
                motivo = x.Select(e => e.motivo).FirstOrDefault(),
                estado = x.Select(e => e.estado).FirstOrDefault(),
                fec_creacion = x.Select(e => e.fec_creacion).FirstOrDefault(),
                placa = x.Select(e => e.placa).FirstOrDefault(),
                tipoorigen = x.Select(e => e.tipoorigen).FirstOrDefault(),
                cliente = x.Select(e => e.cliente).FirstOrDefault(),
                nombre_cliente = x.Select(e => e.nombre_cliente).FirstOrDefault()
                //modelname = x.Select(e => e.modvh_nombre).FirstOrDefault()                              
            });

            return Json(events, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarCitasDisponibles(int asesor_id)
        {
            DateTime hoy = DateTime.Now;
            var citas = (from c in db.agenda_asesor
                         join t in db.icb_terceros
                             on c.cliente equals t.doc_tercero
                         //join m in db.icb_vehiculo
                         //on c.placa equals m.plan_mayor 
                         //join mv in db.modelo_vehiculo 
                         //on m.modvh_id equals mv.modvh_codigo
                         where c.asesor_id == asesor_id
                         select new
                         {
                             c.desde,
                             c.hasta,
                             c.titulo,
                             c.id,
                             c.motivo,
                             c.descripcion,
                             c.asesor_id,
                             c.estado,
                             c.fec_creacion,
                             placa = (c.placa != null) & (c.placa != string.Empty) ? c.placa : "",
                             c.tipoorigen,
                             c.cliente,
                             nombre_cliente = t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero + " " +
                                              t.segapellido_tercero,
                             modvh_nombre = c.placa != null && c.placa != string.Empty
                                 ? c.icb_vehiculo.modelo_vehiculo.modvh_nombre
                                 : ""
                         }).ToList();

            var events = citas.GroupBy(x => x.id).Select(x => new
            {
                title = x.Select(e => e.titulo).FirstOrDefault() + "||" + x.Select(e => e.nombre_cliente).FirstOrDefault() + "||" +
                        x.Select(e => e.modvh_nombre).FirstOrDefault(),
                start = x.Select(e => e.desde.ToString("yyyy/MM/dd")).FirstOrDefault() + " " +
                        x.Select(e => e.desde.Hour.ToString("00")).FirstOrDefault() + ":" +
                        x.Select(e => e.desde.Minute.ToString("00")).FirstOrDefault(),
                end = x.Select(e => e.hasta.ToString("yyyy/MM/dd")).FirstOrDefault() + " " +
                      x.Select(e => e.hasta.Hour.ToString("00")).FirstOrDefault() + ":" +
                      x.Select(e => e.hasta.Minute.ToString("00")).FirstOrDefault() + ":00",
                id = x.Key,
                descripcion = x.Select(e => e.descripcion).FirstOrDefault(),
                asesor_id = x.Select(e => e.asesor_id).FirstOrDefault(),
                motivo = x.Select(e => e.motivo).FirstOrDefault(),
                estado = x.Select(e => e.estado).FirstOrDefault(),
                fec_creacion = x.Select(e => e.fec_creacion).FirstOrDefault(),
                placa = x.Select(e => e.placa).FirstOrDefault(),
                tipoorigen = x.Select(e => e.tipoorigen).FirstOrDefault(),
                cliente = x.Select(e => e.cliente).FirstOrDefault(),
                nombre_cliente = x.Select(e => e.nombre_cliente).FirstOrDefault()
            });
            return Json(events, JsonRequestBehavior.AllowGet);
        }

        public bool CalcularDisponible(int asesor_id, DateTime fecha, DateTime inicio, DateTime fin)
        {
            System.Collections.Generic.List<agenda_asesor> citasDelMes = db.agenda_asesor.Where(x =>
                x.asesor_id == asesor_id && x.desde.Year == fecha.Year && x.desde.Month == fecha.Month).ToList();

            foreach (agenda_asesor cita in citasDelMes)
            {
                if (DateTime.Compare(inicio, cita.desde) == 0)
                {
                    return false;
                }
                else if (DateTime.Compare(inicio, cita.desde) > 0 && DateTime.Compare(inicio, cita.hasta) < 0)
                {
                    return false;
                }
                else if (DateTime.Compare(inicio, cita.desde) > 0 && DateTime.Compare(fin, cita.hasta) < 0)
                {
                    return false;
                }
                else if (DateTime.Compare(fin, cita.desde) > 0 && DateTime.Compare(fin, cita.hasta) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        public JsonResult BuscarHorario(int asesor_id)
        {
            var horario2 = db.parametrizacion_horario.Where(h => h.usuario_id == asesor_id).FirstOrDefault();
            var lunes = "";
            var martes = "";
            var miercoles = "";
            var jueves = "";
            var viernes = "";
            var sabado = "";
            var domingo = "";

            if (horario2.lunes_desde != null)
            {
                lunes = lunes + "" + horario2.lunes_desde.Value.ToString().Substring(0, 5) + " - ";
            }
            if (horario2.lunes_hasta != null)
            {
                lunes = lunes + "" + horario2.lunes_hasta.Value.ToString().Substring(0, 5) + " - ";
            }
            if (horario2.lunes_desde2 != null)
            {
                lunes = lunes + "" + horario2.lunes_desde2.Value.ToString().Substring(0, 5) + " - ";
            }
            if (horario2.lunes_hasta2 != null)
            {
                lunes = lunes + "" + horario2.lunes_hasta2.Value.ToString().Substring(0, 5);
            }
            //////
            if (horario2.martes_desde != null)
            {
                martes = lunes + "" + horario2.martes_desde.Value.ToString().Substring(0, 5) + " - ";
            }
            if (horario2.martes_hasta != null)
            {
                martes = lunes + "" + horario2.martes_hasta.Value.ToString().Substring(0, 5) + " - ";
            }
            if (horario2.martes_desde2 != null)
            {
                martes = lunes + "" + horario2.martes_desde2.Value.ToString().Substring(0, 5) + " - ";
            }
            if (horario2.martes_hasta2 != null)
            {
                martes = lunes + "" + horario2.martes_hasta2.Value.ToString().Substring(0, 5);
            }
            //////
            if (horario2.miercoles_desde != null)
            {
                miercoles = lunes + "" + horario2.miercoles_desde.Value.ToString().Substring(0, 5) + " - ";
            }
            if (horario2.miercoles_hasta != null)
            {
                miercoles = lunes + "" + horario2.miercoles_hasta.Value.ToString().Substring(0, 5) + " - ";
            }
            if (horario2.miercoles_desde2 != null)
            {
                miercoles = lunes + "" + horario2.miercoles_desde2.Value.ToString().Substring(0, 5) + " - ";
            }
            if (horario2.miercoles_hasta2 != null)
            {
                miercoles = lunes + "" + horario2.miercoles_hasta2.Value.ToString().Substring(0, 5);
            }
            /////
            if (horario2.jueves_desde != null)
            {
                jueves = lunes + "" + horario2.jueves_desde.Value.ToString().Substring(0, 5) + " - ";
            }
            if (horario2.jueves_hasta != null)
            {
                jueves = lunes + "" + horario2.jueves_hasta.Value.ToString().Substring(0, 5) + " - ";
            }
            if (horario2.jueves_desde2 != null)
            {
                jueves = lunes + "" + horario2.jueves_desde2.Value.ToString().Substring(0, 5) + " - ";
            }
            if (horario2.jueves_hasta2 != null)
            {
                jueves = lunes + "" + horario2.jueves_hasta2.Value.ToString().Substring(0, 5);
            }
            ////
            if (horario2.viernes_desde != null)
            {
                viernes = lunes + "" + horario2.viernes_desde.Value.ToString().Substring(0, 5) + " - ";
            }
            if (horario2.viernes_hasta != null)
            {
                viernes = lunes + "" + horario2.viernes_hasta.Value.ToString().Substring(0, 5) + " - ";
            }
            if (horario2.viernes_desde2 != null)
            {
                viernes = lunes + "" + horario2.viernes_desde2.Value.ToString().Substring(0, 5) + " - ";
            }
            if (horario2.viernes_hasta2 != null)
            {
                viernes = lunes + "" + horario2.viernes_hasta2.Value.ToString().Substring(0, 5);
            }
            ////
            if (horario2.sabado_desde != null)
            {
                sabado = lunes + "" + horario2.sabado_desde.Value.ToString().Substring(0, 5) + " - ";
            }
            if (horario2.sabado_hasta != null)
            {
                sabado = lunes + "" + horario2.sabado_hasta.Value.ToString().Substring(0, 5) + " - ";
            }
            if (horario2.sabado_desde2 != null)
            {
                sabado = lunes + "" + horario2.sabado_desde2.Value.ToString().Substring(0, 5) + " - ";
            }
            if (horario2.sabado_hasta2 != null)
            {
                sabado = lunes + "" + horario2.sabado_hasta2.Value.ToString().Substring(0, 5);
            }
            ///
            if (horario2.domingo_desde != null)
            {
                domingo = lunes + "" + horario2.domingo_desde.Value.ToString().Substring(0, 5) + " - ";
            }
            if (horario2.domingo_hasta != null)
            {
                domingo = lunes + "" + horario2.domingo_hasta.Value.ToString().Substring(0, 5) + " - ";
            }
            if (horario2.domingo_desde2 != null)
            {
                domingo = lunes + "" + horario2.domingo_desde2.Value.ToString().Substring(0, 5) + " - ";
            }
            if (horario2.domingo_hasta2 != null)
            {
                domingo = lunes + "" + horario2.domingo_hasta2.Value.ToString().Substring(0, 5);
            }
            var horario = new
            {
                lunes,
                martes,
                miercoles,
                jueves,
                viernes,
                sabado,
                domingo,
                no_disponible = horario2.ndispo_fecha_inicio != null ? horario2.ndispo_fecha_inicio + " a " + horario2.ndispo_fecha_fin :
                                  horario2.no_disponible ? "No disponible en toda la semana" : " ",
                fecha_inicio = horario2.ndispo_fecha_inicio,
                fecha_fin = horario2.ndispo_fecha_fin,
                motivo = horario2.observaciones != null ? horario2.observaciones : " ",
                nd = horario2.no_disponible,
                d_lunes = horario2.lunes_disponible,
                d_martes = horario2.martes_disponible,
                d_miercoles = horario2.miercoles_disponible,
                d_jueves = horario2.jueves_disponible,
                d_viernes = horario2.viernes_disponible,
                d_sabado = horario2.sabado_disponible,
                d_domingo = horario2.domingo_disponible
            };

            var novedades = (from n in db.vhorarionovedad
                             where n.parametrizacion_horario.usuario_id == asesor_id
                             select new
                             {
                                 n.fechaini,
                                 n.fechafin
                             }).ToList();

            var data = new
            {
                horario,
                novedades
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDisponibilidadHorario(int asesor_id, string fecha)
        {
            DateTimeFormatInfo usDtfi = new CultureInfo("en-US").DateTimeFormat;
            DateTime fec = DateTime.Parse(fecha, usDtfi);
            string fechaSolicitada = Convert.ToDateTime(fec, usDtfi).ToShortDateString();
            DayOfWeek dia = fec.DayOfWeek;
            string hour = Convert.ToString(fec.Hour + ":" + fec.Minute);
            TimeSpan hora = TimeSpan.Parse(hour);
            int data = 0;
            parametrizacion_horario horario = db.parametrizacion_horario.FirstOrDefault(x => x.usuario_id == asesor_id);

            #region buscarNovedades

            System.Collections.Generic.List<vhorarionovedad> novedad = db.vhorarionovedad.Where(x => x.horarioid == horario.horario_id).ToList();

            if (novedad != null)
            {
                foreach (vhorarionovedad item in novedad)
                {
                    DateTime fIni = Convert.ToDateTime(item.fechaini, usDtfi);
                    string fechaIni = Convert.ToDateTime(item.fechaini, usDtfi).ToShortDateString();
                    DateTime fFin = Convert.ToDateTime(item.fechafin, usDtfi);
                    string fechaFin = Convert.ToDateTime(item.fechafin, usDtfi).ToShortDateString();

                    if (fechaIni == fechaSolicitada || fechaFin == fechaSolicitada)
                    {
                        if (fIni >= fec || fFin >= fec)
                        {
                            string a = Convert.ToString(fIni);
                            string b = Convert.ToString(fFin);
                            //esta en una novedad, no permitir agendar
                            data = 2;
                            return Json(new { data, novedad = item.motivo, inicio = a, final = b },
                                JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }

            #endregion


            if (horario.no_disponible)
            {
                data = 0;
            }
            else
            {
                #region lunes

                if (dia == DayOfWeek.Monday)
                {
                    if (horario.lunes_disponible)
                    {
                        if (horario.lunes_desde != null)
                        {
                            if (horario.lunes_desde < hora && horario.lunes_hasta > hora)
                            {
                                data = 1;
                            }
                        }

                        if (horario.lunes_desde2 != null)
                        {
                            if (horario.lunes_desde2 < hora && horario.lunes_hasta2 > hora)
                            {
                                data = 1;
                            }
                        }
                    }
                }

                #endregion

                #region martes

                if (dia == DayOfWeek.Tuesday)
                {
                    if (horario.martes_disponible)
                    {
                        if (horario.martes_desde != null)
                        {
                            if (horario.martes_desde < hora && horario.martes_hasta > hora)
                            {
                                data = 1;
                            }
                        }

                        if (horario.martes_desde2 != null)
                        {
                            if (horario.martes_desde2 < hora && horario.martes_hasta2 > hora)
                            {
                                data = 1;
                            }
                        }
                    }
                }

                #endregion

                #region miercoles

                if (dia == DayOfWeek.Wednesday)
                {
                    if (horario.miercoles_disponible)
                    {
                        if (horario.miercoles_desde != null)
                        {
                            if (horario.miercoles_desde < hora && horario.miercoles_hasta > hora)
                            {
                                data = 1;
                            }
                        }

                        if (horario.miercoles_desde2 != null)
                        {
                            if (horario.miercoles_desde2 < hora && horario.miercoles_hasta2 > hora)
                            {
                                data = 1;
                            }
                        }
                    }
                }

                #endregion

                #region jueves

                if (dia == DayOfWeek.Thursday)
                {
                    if (horario.jueves_disponible)
                    {
                        if (horario.jueves_desde != null)
                        {
                            if (horario.jueves_desde < hora && horario.jueves_hasta > hora)
                            {
                                data = 1;
                            }
                        }

                        if (horario.jueves_desde2 != null)
                        {
                            if (horario.jueves_desde2 < hora && horario.jueves_hasta2 > hora)
                            {
                                data = 1;
                            }
                        }
                    }
                }

                #endregion

                #region viernes

                if (dia == DayOfWeek.Friday)
                {
                    if (horario.viernes_disponible)
                    {
                        if (horario.viernes_desde != null)
                        {
                            if (horario.viernes_desde < hora && horario.viernes_hasta > hora)
                            {
                                data = 1;
                            }
                        }

                        if (horario.viernes_desde2 != null)
                        {
                            if (horario.viernes_desde2 < hora && horario.viernes_hasta2 > hora)
                            {
                                data = 1;
                            }
                        }
                    }
                }

                #endregion

                #region sabado

                if (dia == DayOfWeek.Saturday)
                {
                    if (horario.sabado_disponible)
                    {
                        if (horario.sabado_desde != null)
                        {
                            if (horario.sabado_desde < hora && horario.sabado_hasta > hora)
                            {
                                data = 1;
                            }
                        }

                        if (horario.sabado_desde2 != null)
                        {
                            if (horario.sabado_desde2 < hora && horario.sabado_hasta2 > hora)
                            {
                                data = 1;
                            }
                        }
                    }
                }

                #endregion

                #region domingo

                if (dia == DayOfWeek.Sunday)
                {
                    if (horario.domingo_disponible)
                    {
                        if (horario.domingo_desde != null)
                        {
                            if (horario.domingo_desde < hora && horario.domingo_hasta > hora)
                            {
                                data = 1;
                            }
                        }

                        if (horario.domingo_desde2 != null)
                        {
                            if (horario.domingo_desde2 < hora && horario.domingo_hasta2 > hora)
                            {
                                data = 1;
                            }
                        }
                    }
                }

                #endregion
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public DateTime sumarFecha(string tiempo, DateTime fecha)
        {
            if (tiempo == "hora")
            {
                fecha = fecha.AddHours(1);
            }
            else if (tiempo == "dia")
            {
                fecha = fecha.AddDays(1);
            }
            else if (tiempo == "semana")
            {
                fecha = fecha.AddDays(7);
            }
            else if (tiempo == "mes")
            {
                fecha = fecha.AddMonths(1);
            }

            return fecha;
        }

        public JsonResult validarHoraInicio(string horaInicio)
        {
            string data = "";

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult validarHoraFinal(string horaInicio, string horaFinal)
        {
            string data = "";

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }

        public void BuscarFavoritos(int? menu)
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

            var buscarFavoritosSeleccionados = (from favoritos in db.favoritos
                                                join menu2 in db.Menus
                                                    on favoritos.idmenu equals menu2.idMenu
                                                where favoritos.idusuario == usuarioActual && favoritos.seleccionado
                                                select new
                                                {
                                                    favoritos.seleccionado,
                                                    favoritos.cantidad,
                                                    menu2.idMenu,
                                                    menu2.nombreMenu,
                                                    menu2.url
                                                }).OrderByDescending(x => x.cantidad).ToList();

            bool esFavorito = false;

            foreach (var favoritosSeleccionados in buscarFavoritosSeleccionados)
            {
                if (favoritosSeleccionados.idMenu == menu)
                {
                    esFavorito = true;
                    break;
                }
            }

            if (esFavorito)
            {
                ViewBag.Favoritos =
                    "<div id='areaFavoritos'><i class='fa fa-close'></i>&nbsp;&nbsp;<a href='#' onclick='AgregarQuitarFavorito();return false;'>Quitar de Favoritos</a><div>";
            }
            else
            {
                ViewBag.Favoritos =
                    "<div id='areaFavoritos'><i class='fa fa-check'></i>&nbsp;&nbsp;<a href='#' onclick='AgregarQuitarFavorito();return false;'>Agregar a Favoritos</a></div>";
            }

            ViewBag.id_menu = menu != null ? menu : 0;
        }
    }
}