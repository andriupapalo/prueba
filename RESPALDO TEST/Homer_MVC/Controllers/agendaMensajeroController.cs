using Homer_MVC.ViewModels.medios;
using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class agendaMensajeroController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();
        private readonly CultureInfo miCultura = new CultureInfo("is-IS");
        /*mensajero*/
        public class listaAgenda
        {
            public string descripcion { get; set; }
            public string colorhx { get; set; }
        }
        public ActionResult Index()
        {
            int mensajeroActual = Convert.ToInt32(Session["user_usuarioid"]);
            ViewBag.mensajeroActual = mensajeroActual;

            var listMensajeros = (from u in db.users
                                  where u.user_estado && u.rol_id == 2038
                                  select new
                                  {
                                      id = u.user_id,
                                      nombre = u.user_nombre + " " + u.user_apellido
                                  }).ToList();

            var listTerceros = (from t in db.icb_terceros
                                where t.tercero_estado
                                select new
                                {
                                    id = t.tercero_id,
                                    nombre = "(" + t.doc_tercero + ") " + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                             t.apellido_tercero + " " + t.segapellido_tercero + " " + t.razon_social
                                }).ToList();

            //ViewBag.asesor_actual = Session["user_rolid"].ToString();

            ViewBag.mensajeroID = new SelectList(listMensajeros, "id", "nombre", mensajeroActual);
            ViewBag.cliente = new SelectList(listTerceros, "id", "nombre");
            //ViewBag.demo_id = new SelectList(db.vdemos, "id", "placa");
            ViewBag.ruta = new SelectList(db.vrutasdemo, "id", "ruta");
            ViewBag.ncliente = new SelectList(db.icb_terceros, "tercero_id", "doc_tercero");
            ViewBag.id_encuesta = new SelectList(db.crm_encuestas, "id", "Descripcion");


            var listM = (from m in db.icb_terceros
                         select new
                         {
                             id = m.tercero_id,
                             nombre = m.prinom_tercero + " " + m.apellido_tercero
                         }).ToList();

            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in listM)
            {
                lista.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }
            ViewBag.Mensajero = lista;



            return View();
        }

        public bool CalcularDisponible(int idmensajero, DateTime fecha, DateTime hasta, DateTime desde)
        {
            System.Collections.Generic.List<agenda_mensajero> citasDelMes = db.agenda_mensajero.Where(x =>
                x.idmensajero == idmensajero && x.desde.Year == fecha.Year && x.desde.Month == fecha.Month).ToList();

            foreach (agenda_mensajero cita in citasDelMes)
            {
                if (DateTime.Compare(hasta, cita.desde) == 0)
                {
                    return false;
                }
                else if (DateTime.Compare(hasta, cita.desde) > 0 && DateTime.Compare(hasta, cita.hasta) < 0)
                {
                    return false;
                }
                else if (DateTime.Compare(hasta, cita.desde) > 0 && DateTime.Compare(desde, cita.hasta) < 0)
                {
                    return false;
                }
                else if (DateTime.Compare(desde, cita.desde) > 0 && DateTime.Compare(desde, cita.hasta) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        public JsonResult validarCreadorCita(int id)
        {
            var data = (from aa in db.agenda_mensajero
                        where aa.id == id
                        select new
                        {
                            aa.idmensajero,
                            aa.userid_creacion
                        }).FirstOrDefault();

            if (data.idmensajero == data.userid_creacion)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarCitas(int idmensajero)
        {
            var citas = (from cita in db.agenda_mensajero
                         join m in db.tercero_empleado
                            on cita.idmensajero equals m.emp_tercero_id
                         join mensajero in db.icb_terceros
                            on m.tercero_id equals mensajero.tercero_id
                         join e in db.Estado
                           on cita.idestado equals e.id
                         where cita.idmensajero == idmensajero
                         select new
                         {
                             cita.desde,
                             cita.hasta,
                             cita.idmensajero,
                             cita.idestado,
                             mensajero.prinom_tercero,
                             mensajero.apellido_tercero,
                             agenda=cita.id,
                             e.colorhx
                         }).ToList();

            var events = citas.Select(x => new
            {
                title = x.prinom_tercero + "  " + x.apellido_tercero,
                start = x.desde.ToString("MM/dd/yyyy") + " " + x.desde.Hour.ToString("00") +
                        ":" + x.desde.Minute.ToString("00") + ":00",
                end = x.hasta.ToString("MM/dd/yyyy") + " " + x.hasta.Hour.ToString("00") + ":" +
                      x.hasta.Minute.ToString("00") + ":00",
                id = x.idmensajero,
                x.agenda,
                colorCita=x.colorhx,
                estado=x.idestado,
            });

            return Json(events, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CrearMensajeria(agenda_mensajero agendaMensajero, int[] idEncabezados)
        {
                   if (Session["user_usuarioid"] != null)
            {
                var dataResult = 0;
                var id = idEncabezados[0];

                if (ModelState.IsValid)
                {
                    using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            bool citas = CalcularDisponible(agendaMensajero.idmensajero, DateTime.Now, agendaMensajero.desde,
                        agendaMensajero.hasta);
                            if (citas)
                            {
                                if (agendaMensajero.hasta > agendaMensajero.desde)
                                {

                                    agendaMensajero.desde = agendaMensajero.desde;
                                    agendaMensajero.hasta = agendaMensajero.hasta;
                                    //agendaMensajero.idestado = agendaMensajero.idestado;/*1*/
                                    agendaMensajero.idestado = 3;
                                    agendaMensajero.idPrioridad = agendaMensajero.idPrioridad;/*10*/
                                    agendaMensajero.idTransporte = agendaMensajero.idTransporte;/*11*/
                                    agendaMensajero.idmensajero = agendaMensajero.idmensajero;/*2*/
                                    agendaMensajero.descripcion = agendaMensajero.descripcion;
                                    agendaMensajero.destino_bodega = agendaMensajero.destino_bodega;/*4*/
                                    agendaMensajero.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);/*6*/
                                    agendaMensajero.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);/*7*/
                                    agendaMensajero.fec_creacion = DateTime.Now;
                                    agendaMensajero.fec_actualizacion = DateTime.Now;

                                    //agendaMensajero.titulo = agendaMensajero.titulo;
                                    //agendaMensajero.motivo = agendaMensajero.motivo;
                                    //agendaMensajero.origen_bodega = agendaMensajero.origen_bodega;
                                    agendaMensajero.origen_bodega = Convert.ToInt32(Session["user_bodega"]); /*3*/

                                    /*
                                    agendaMensajero.idTraslado = agendaMensajero.idTraslado;
                                    agendaMensajero.idEncabezado = Convert.ToInt32(arreglo1[3]);
                                    */
                                    db.agenda_mensajero.Add(agendaMensajero);
                                    int result = db.SaveChanges();

                                    var buscar = agendaMensajero.id;

                                    for (int i = 0; i < idEncabezados.Length; i++)
                                    {
                                        var tmp = idEncabezados[i];

                                        var documento = db.recibidotraslados.Where(d => d.id == tmp && d.lineas_documento.mensajeria_atendido == false && d.encab_documento.requiere_mensajeria == true).FirstOrDefault();

                                        detalle_agendamiento detalle = new detalle_agendamiento();

                                        detalle.fk_agendamiento = buscar;
                                        detalle.fk_encab_documento = documento.idtraslado;
                                        detalle.fk_recibidotraslado = documento.id;

                                        db.detalle_agendamiento.Add(detalle);
                                        int resultado1 = db.SaveChanges();
                                        //al detalle del traslado le tengo que cambiar el estado
                                        documento.estado_traslado = 3;
                                        db.Entry(documento).State = EntityState.Modified;
                                        //marco la linea documento como que ya se atendió
                                        var linea = db.lineas_documento.Where(d => d.id == documento.idlinea).FirstOrDefault();
                                        linea.mensajeria_atendido = true;
                                        db.Entry(linea).State = EntityState.Modified;
                                        int resultado = db.SaveChanges();

                                        //busco el encabezado si tiene otras lineas que no sean estas y que tengan requiere_mensajeria
                                        var otras_lineas = db.lineas_documento.Where(d => d.id_encabezado == documento.idtraslado && d.mensajeria_atendido == false && d.encab_documento.requiere_mensajeria == true).Count();
                                        if (otras_lineas == 0)
                                        {
                                            var encab = db.encab_documento.Where(d => d.idencabezado == documento.idtraslado).FirstOrDefault();
                                            encab.mensajeria_atendido = true;
                                            db.Entry(linea).State = EntityState.Modified;
                                            //busco la solicitud de traslado asociada
                                            var soli = db.Solicitud_traslado.Where(d => d.Id == encab.id_solicitud_traslado).FirstOrDefault();
                                            if (soli != null)
                                            {
                                                soli.Estado_atendido = 3;
                                            }
                                            int resultado2 = db.SaveChanges();
                                        }

                                    }


                                    if (result == 1)
                                    {
                                        dbTran.Commit();
                                        //dataResult = "Agenda creada correctamente";
                                        dataResult = 1;
                                    }
                                    else
                                    {
                                        dbTran.Rollback();
                                        //dataResult = "Error al crear la Agenda, por favor valide los datos";
                                        dataResult = 0;
                                    }
                                }
                                else
                                {
                                    dbTran.Rollback();
                                    //dataResult = "La fecha final de la agenda debe ser mayor a la inicial, por favor valide";
                                    dataResult = 2;

                                }
                            }
                            else
                            {
                                dbTran.Rollback();
                                //dataResult = "Ya tiene una agenda para el día y hora ingresados, por favor valide";
                                dataResult = 3;

                            }
                        }
                        catch (Exception ex)
                        {
                            var error = ex.InnerException.Message;
                            dbTran.Rollback();
                            dataResult = 0;
                        }
                    }
                        
                }
                return Json(dataResult, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
            

        }

        [HttpPost]
        public ActionResult EditarMensajeria(agenda_mensajero agendaMensajero, int? menu)
        {
            if (ModelState.IsValid)
            {

                bool citas = true;

                citas = CalcularDisponible(agendaMensajero.idmensajero, DateTime.Now, agendaMensajero.desde,
                    agendaMensajero.hasta);

                if (citas)
                {
                    agenda_mensajero agenda = db.agenda_mensajero.Find(agendaMensajero.id);

                    agenda.desde = agendaMensajero.desde;
                    agenda.hasta = agendaMensajero.hasta;
                    agenda.descripcion = agendaMensajero.descripcion;
                    agenda.fec_actualizacion = DateTime.Now;
                    agenda.idestado = agendaMensajero.idestado;
                    agenda.idmensajero = agendaMensajero.idmensajero;
                    agenda.bodega_destino = agendaMensajero.bodega_destino;
                    agenda.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    agenda.idPrioridad = agendaMensajero.idPrioridad;
                    agenda.idTransporte = agendaMensajero.idTransporte;


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
            else
            {
                //TempData["mensaje_error"] = "Errores en la creación del pedido, por favor valide";
                System.Collections.Generic.List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();
            }

            return RedirectToAction("Index", new { menu });
        }

        public ActionResult parametroEstado()
        {
            var listE = (from e in db.Estado
                         select new
                         {
                             id = e.id,
                             nombre = e.Tipo
                         }).ToList();

            List<SelectListItem> listaEstado = new List<SelectListItem>();
            foreach (var item in listE)
            {
                listaEstado.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }
            ViewBag.Estado = listaEstado;

            return View();
        }

        public JsonResult tablaEstado()
        {

            var data = (from e in db.Estado
                        select new
                        {
                            id = e.id,
                            nombre = e.Tipo,
                            habilitado = e.habilitado
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult parametroPrioridad()
        {

            var listP = (from p in db.Prioridad
                         select new
                         {
                             id = p.id,
                             nombre = p.tipo_prioridad
                         }).ToList();

            List<SelectListItem> lista3 = new List<SelectListItem>();
            foreach (var item in listP)
            {
                lista3.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }
            ViewBag.Prioridad = lista3;

            return View();

        }

        public JsonResult tablaPrioridad()
        {
            var data = (from e in db.Prioridad
                        select new
                        {
                            id = e.id,
                            nombre = e.tipo_prioridad,
                            habilitado = e.habilitado
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult parametroTipoTransporte()
        {
            var listT = (from t in db.Transporte
                         select new
                         {
                             id = t.id,
                             tipo = t.tipo_transporte
                         }).ToList();

            List<SelectListItem> lista2 = new List<SelectListItem>();
            foreach (var item in listT)
            {
                lista2.Add(new SelectListItem
                {
                    Text = item.tipo,
                    Value = item.id.ToString()
                });
            }
            ViewBag.Transporte = lista2;


            return View();
        }

        public JsonResult tablaTransporte()
        {

            var data = (from e in db.Transporte
                        select new
                        {
                            id = e.id,
                            nombre = e.tipo_transporte,
                            habilitado = e.habilitado
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult agregarEstado(int id, bool habilitado)
        {
            var result = 0;
            Estado e = new Estado();

            var state = db.Estado.Find(id);

            state.habilitado = habilitado;

            db.Entry(state).State = EntityState.Modified;
            int resultado = db.SaveChanges();

            if (resultado > 0)
            {
                result = 1;
            }
            else
            {
                result = 0;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult agregarPrioridad(int id, bool habilitado)
        {

            var result = 0;
            Prioridad p = new Prioridad();

            var state = db.Prioridad.Find(id);

            state.habilitado = habilitado;

            db.Entry(state).State = EntityState.Modified;
            int resultado = db.SaveChanges();

            if (resultado > 0)
            {
                result = 1;
            }
            else
            {
                result = 0;
            }

            return Json(result, JsonRequestBehavior.AllowGet);

        }
        public JsonResult agregarTipoTransporte(int id, bool habilitado)
        {

            var result = 0;
            Transporte t = new Transporte();

            var state = db.Transporte.Find(id);

            state.habilitado = habilitado;

            db.Entry(state).State = EntityState.Modified;
            int resultado = db.SaveChanges();

            if (resultado > 0)
            {
                result = 1;
            }
            else
            {
                result = 0;
            }

            return Json(result, JsonRequestBehavior.AllowGet);

        }
        public JsonResult parametrizarEstado(string parametro)
        {
            var result = 0;

            Estado e = new Estado();

            e.Tipo = parametro;

            db.Estado.Add(e);
            int resultado = db.SaveChanges();

            if (resultado > 0)
            {
                result = 1;
            }
            else
            {
                result = 0;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult parametrizarPrioridad(string parametro)
        {
            var result = 0;
            Prioridad p = new Prioridad();

            p.tipo_prioridad = parametro;

            db.Prioridad.Add(p);
            int resultado = db.SaveChanges();

            if (resultado > 0)
            {
                result = 1;
            }
            else
            {
                result = 0;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult parametrizarTransporte(string parametro)
        {
            var result = 0;
            Transporte t = new Transporte();

            t.tipo_transporte = parametro;

            db.Transporte.Add(t);
            int resultado = db.SaveChanges();

            if (resultado > 0)
            {
                result = 1;
            }
            else
            {
                result = 0;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult BrowserMensajeria()
        {
            int rol = Convert.ToInt32(Session["user_rolid"]);

            var permisos = (from acceso in db.rolacceso
                            join rolPerm in db.rolpermisos
                            on acceso.idpermiso equals rolPerm.id
                            where acceso.idrol == rol //&& rolPerm.codigo == "P40" 
                            select new { rolPerm.codigo }).ToList();

            var resultado1 = permisos.Where(x => x.codigo == "P38").Count() > 0 ? "Si" : "No";
            var resultado2 = permisos.Where(x => x.codigo == "P41").Count() > 0 ? "Si" : "No";

            ViewBag.Permiso = resultado1;
            ViewBag.Permiso1 = resultado2;

            ViewBag.ttiposcitas = (from e in db.Estado where e.habilitado select new listaAgenda { descripcion = e.Tipo, colorhx = e.colorhx }).ToList();

            var listM = (from m in db.tercero_empleado
                         join e in db.icb_terceros
                         on m.tercero_id equals e.tercero_id
                         where m.teremp_cargo == 4
                         select new
                         {
                             id = m.emp_tercero_id,
                             nombre = e.prinom_tercero + " " + e.apellido_tercero
                         }).ToList();

            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in listM)
            {
                lista.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }
            ViewBag.Mensajero = lista;
            /*Transporte*/


            var listT = (from t in db.Transporte
                         where t.habilitado == true
                         select new
                         {
                             id = t.id,
                             tipo = t.tipo_transporte
                         }).ToList();

            List<SelectListItem> lista2 = new List<SelectListItem>();
            foreach (var item in listT)
            {
                lista2.Add(new SelectListItem
                {
                    Text = item.tipo,
                    Value = item.id.ToString()
                });
            }
            ViewBag.Transporte = lista2;

            /*Prioridad*/

            var listP = (from p in db.Prioridad
                         where p.habilitado == true
                         select new
                         {
                             id = p.id,
                             nombre = p.tipo_prioridad
                         }).ToList();

            List<SelectListItem> lista3 = new List<SelectListItem>();
            foreach (var item in listP)
            {
                lista3.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }
            ViewBag.Prioridad = lista3;

            /*Estado*/
            var listE = (from e in db.Estado
                         //where e.id == 2
                         select new
                         {
                             id = e.id,
                             nombre = e.Tipo
                         }).ToList();

            List<SelectListItem> listaEstado = new List<SelectListItem>();
            foreach (var item in listE)
            {
                listaEstado.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }
            ViewBag.Estado = listaEstado;

            /*Bodega origen*/
            List<bodega_concesionario> origen_bodega = db.bodega_concesionario.ToList();
            List<SelectListItem> itemsB1 = new List<SelectListItem>();
            foreach (bodega_concesionario item in origen_bodega)
            {
                string nombre = item.bodccs_nombre;
                int id = item.id;
                itemsB1.Add(new SelectListItem
                {
                    Text = nombre,
                    Value = id.ToString(),
                });

            }
            ViewBag.BodegaOrigen = new SelectList(itemsB1, "Value", "Text");
            /*Bodega Destino*/
            List<bodega_concesionario> destino_bodega = db.bodega_concesionario.ToList();
            List<SelectListItem> itemsB2 = new List<SelectListItem>();
            foreach (bodega_concesionario item in origen_bodega)
            {
                string nombre = item.bodccs_nombre;
                int id = item.id;
                itemsB2.Add(new SelectListItem
                {
                    Text = nombre,
                    Value = id.ToString(),
                });

            }
            ViewBag.BodegaDestino = new SelectList(itemsB2, "Value", "Text");
            return View();
        }

        public JsonResult cargarTrasladosDestino()
        {
            //int bodegaActual = Convert.ToInt32(Session["user_bodega"]);

            var buscar = (from a in db.recibidotraslados
                          join b in db.encab_documento
                              on a.idtraslado equals b.idencabezado
                          join c in db.icb_referencia
                              on a.refcodigo equals c.ref_codigo
                          join d in db.bodega_concesionario
                              on a.idorigen equals d.id
                          join dd in db.bodega_concesionario
                              on a.iddestino equals dd.id
                          join e in db.users
                              on a.usertraslado equals e.user_id
                          where a.recibo_completo == false && a.tipo == "R" && b.requiere_mensajeria == true && b.mensajeria_atendido == false //&& a.idorigen == bodegaActual //|| a.iddestino == bodegaActual
                          select new
                          {
                              a.id,
                              b.numero,
                              a.refcodigo,
                              c.ref_descripcion,
                              a.cantidad,
                              a.recibido,
                              a.fechatraslado,
                              a.costo,
                              origen = d.bodccs_nombre,
                              destino = dd.bodccs_nombre,
                              cantidad_pendiente = a.recibido == true ? a.cant_recibida != null ? a.cantidad - a.cant_recibida : a.cantidad : a.cantidad,
                              esfaltante = a.recibido && a.recibo_completo == false ? 1 : 0,
                              destinatario = b.destinatario != null ? b.users2.user_nombre.ToString() + " " + b.users2.user_apellido.ToString() : "",
                          }).ToList();

            var data = buscar.Select(x => new
            {
                x.id,
                x.numero,
                cod_referencia = x.refcodigo,
                referencia = x.refcodigo + " - " + x.ref_descripcion,
                x.cantidad,
                x.cantidad_pendiente,
                x.esfaltante,
                x.recibido,
                fechatraslado = x.fechatraslado.ToString("yyyy/MM/dd HH:mm"),
                costo = x.costo.ToString("0,0", miCultura),
                x.destino,
                x.origen,
                x.destinatario
            });


            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonResult cargarReferencias(int mensajeria)
        {

            var data1 = (from a in db.recibidotraslados
                        join b in db.encab_documento
                        on a.idtraslado equals b.idencabezado
                        join c in db.icb_referencia
                        on a.refcodigo equals c.ref_codigo
                        join d in db.agenda_mensajero
                        on b.idencabezado equals d.idEncabezado
                        join e in db.bodega_concesionario 
                        on a.idorigen equals e.id
                        join f in db.bodega_concesionario
                        on a.iddestino equals f.id
                        where d.id == mensajeria //&& b.numero == 104
                        select new
                        {
                            a.id,
                            b.numero,
                            a.refcodigo,
                            c.ref_descripcion,
                            d.hasta,
                            agenda =d.id,
                            f.bodccs_nombre
                        }).ToList();

            var data = data1.Select(x => new
            {
                x.id,
                x.numero,
                referencia = x.refcodigo + "-" + x.ref_descripcion,
                fecha_agendamiento = x.hasta.ToString("yyyy/MM/dd HH:mm"),
                x.agenda,
                destino = x.bodccs_nombre
            });

            return Json(data, JsonRequestBehavior.AllowGet);    

        }

        public JsonResult AgregarEstadoNota(int mensajeria,string nota,int estado) {

            var result = 0;

            var agenda = db.agenda_mensajero.Find(mensajeria);

            agenda.notas_inconformidad = nota;
            agenda.idestado = estado;

            db.Entry(agenda).State = EntityState.Modified;

            int resultado = db.SaveChanges();

            if (resultado > 0)
            {
                result = 1;
            }
            else
            {
                result = 0;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /*tabla mensajeria*/
        public JsonResult cargarMensajeria()
        {

            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            //
            var buscar = db.vw_cargarMensajeria.Where(x => x.tipo_estado >2).ToList();

            var data = buscar.GroupBy(d=>d.traslado).Select(x => new
            {
                numero=x.Select(d=>d.numero).FirstOrDefault(),
                desde=x.Select(d=>d.desde).FirstOrDefault(),
                hasta=x.Select(d=>d.hasta).FirstOrDefault(),
                estado=x.Select(d=>d.estado).FirstOrDefault(),
                tipo_prioridad=x.Select(d=>d.tipo_prioridad).FirstOrDefault(),
                tipo_transporte= x.Select(d => d.tipo_transporte).FirstOrDefault(),
                prinom_tercero = x.Select(d => d.prinom_tercero).FirstOrDefault(),
                apellido_tercero = x.Select(d => d.apellido_tercero).FirstOrDefault(),
                descripcion = x.Select(d => d.descripcion).FirstOrDefault(),
                origen_bodega = x.Select(d => d.origen_bodega).FirstOrDefault(),
                destino_bodega = x.Select(d => d.destino_bodega).FirstOrDefault(),
                fec_creacion = x.Select(d=>d.fec_creacion).FirstOrDefault().Value.ToString("yyyy/MM/dd HH:mm"),
                fec_actualizacion = x.Select(d => d.fec_actualizacion).FirstOrDefault(),
                idEncabezado =x.Key,
                id = x.Select(d => d.id).FirstOrDefault(),
                bodccs_nombre = x.Select(d => d.bodccs_nombre).FirstOrDefault(),
                userid_creacion = x.Select(d => d.userid_creacion).FirstOrDefault(),
                user_nombre = x.Select(d => d.user_nombre).FirstOrDefault(),
                user_apellido = x.Select(d => d.user_apellido).FirstOrDefault(),
                recibotraslado = x.Select(d => d.recibotraslado).FirstOrDefault(),
                tipo_estado = x.Select(d => d.tipo_estado).FirstOrDefault(),
                mensajero = x.Select(d => d.prinom_tercero).FirstOrDefault() + " " + x.Select(d => d.apellido_tercero).FirstOrDefault(),
                usuario = x.Select(d => d.user_nombre).FirstOrDefault() + " " + x.Select(d => d.user_apellido).FirstOrDefault(),
                fecha_agendamiento= x.Select(d => d.hasta).FirstOrDefault().ToString("yyyy/MM/dd HH:mm"),

            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult detalleMensajeria(int id)
        {
            var buscar = (from d in db.detalle_agendamiento
                          join a in db.agenda_mensajero
                         on d.fk_agendamiento equals a.id
                          join e in db.encab_documento
                         on d.fk_encab_documento equals e.idencabezado
                          join r in db.recibidotraslados
                         on e.idencabezado equals r.idtraslado
                          join c in db.icb_referencia
                         on r.refcodigo equals c.ref_codigo
                          where a.id == id
                          select new
                          {
                              a.id,
                              e.numero,
                              r.refcodigo,
                              c.ref_descripcion,
                              a.notas_inconformidad
                              
                          }).ToList();

            var data = buscar.Select(x => new
            {
                x.id,
                x.numero,
                referencia = x.refcodigo + x.ref_descripcion,
                x.notas_inconformidad,
            });


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult cargarMensajeriaFiltro(DateTime? desde, DateTime? hasta, string estado)
        {

            CultureInfo myCultura = new CultureInfo("is-IS");

            var predicado = PredicateBuilder.True<vw_cargarMensajeria>();

            if (desde != null)
            {
                predicado = predicado.And(d => d.desde >= desde);
            }
            if (hasta != null)
            {
                predicado = predicado.And(d => d.hasta <= hasta);
            }
            if (!string.IsNullOrWhiteSpace(estado))
            {
                int estado_id = Convert.ToInt32(estado);
                predicado = predicado.And(d => d.tipo_estado == estado_id);
            }

            var data = db.vw_cargarMensajeria.Where(predicado).ToList();
            var data1 = data.GroupBy(d => d.traslado).Select(x => new
            {
                numero = x.Select(d => d.numero).FirstOrDefault(),
                desde = x.Select(d => d.desde).FirstOrDefault(),
                hasta = x.Select(d => d.hasta).FirstOrDefault(),
                estado = x.Select(d => d.estado).FirstOrDefault(),
                tipo_prioridad = x.Select(d => d.tipo_prioridad).FirstOrDefault(),
                tipo_transporte = x.Select(d => d.tipo_transporte).FirstOrDefault(),
                prinom_tercero = x.Select(d => d.prinom_tercero).FirstOrDefault(),
                apellido_tercero = x.Select(d => d.apellido_tercero).FirstOrDefault(),
                descripcion = x.Select(d => d.descripcion).FirstOrDefault(),
                origen_bodega = x.Select(d => d.origen_bodega).FirstOrDefault(),
                destino_bodega = x.Select(d => d.destino_bodega).FirstOrDefault(),
                fec_creacion = x.Select(d => d.fec_creacion).FirstOrDefault().Value.ToString("yyyy/MM/dd HH:mm"),
                fec_actualizacion = x.Select(d => d.fec_actualizacion).FirstOrDefault(),
                idEncabezado = x.Key,
                id = x.Select(d => d.id).FirstOrDefault(),
                bodccs_nombre = x.Select(d => d.bodccs_nombre).FirstOrDefault(),
                userid_creacion = x.Select(d => d.userid_creacion).FirstOrDefault(),
                user_nombre = x.Select(d => d.user_nombre).FirstOrDefault(),
                user_apellido = x.Select(d => d.user_apellido).FirstOrDefault(),
                recibotraslado = x.Select(d => d.recibotraslado).FirstOrDefault(),
                tipo_estado = x.Select(d => d.tipo_estado).FirstOrDefault(),
                mensajero = x.Select(d => d.prinom_tercero).FirstOrDefault() + " " + x.Select(d => d.apellido_tercero).FirstOrDefault(),
                usuario = x.Select(d => d.user_nombre).FirstOrDefault() + " " + x.Select(d => d.user_apellido).FirstOrDefault(),
                fecha_agendamiento = x.Select(d => d.hasta).FirstOrDefault().ToString("yyyy/MM/dd HH:mm"),

            }).ToList();
            return Json(data1, JsonRequestBehavior.AllowGet);
        }
        public JsonResult cargarMensajeros()
        {

            var buscar = (from t in db.tercero_empleado
                          join it in db.icb_terceros
                            on t.tercero_id equals it.tercero_id
                          select new
                          {
                              it.prinom_tercero,
                              it.apellido_tercero,
                              t.emp_tercero_id
                              //r.idtraslado,
                          }).ToList();
            var data = buscar.Select(x => new
            {
                x.emp_tercero_id,
                mensajero = x.prinom_tercero + " " + x.apellido_tercero
            });


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult cargarMensajerosDisponibles(int emp_tercero_id)
        {
            var buscar = (from a in db.agenda_mensajero
                          join t in db.tercero_empleado
                            on a.idmensajero equals t.emp_tercero_id
                          join it in db.icb_terceros
                            on t.tercero_id equals it.tercero_id
                          where a.hasta != null && t.emp_tercero_id == emp_tercero_id
                          select new
                          {
                              it.prinom_tercero,
                              it.apellido_tercero,
                              a.hasta
                              //r.idtraslado,
                          }).ToList();
            var data = buscar.Select(x => new
            {
                mensajero = x.prinom_tercero + " " + x.apellido_tercero,
                cita = x.hasta.ToString("yyyy/MM/dd HH:mm:ss")
            });


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AgregarNota(agenda_mensajero agendaMensajero, int? menu)
        {
            agenda_mensajero agenda = db.agenda_mensajero.Find(agendaMensajero.id);
            agenda.notas_inconformidad = agendaMensajero.notas_inconformidad;
            db.Entry(agenda).State = EntityState.Modified;
            int resultado = db.SaveChanges();


            if (resultado == 1)
            {
                TempData["mensaje"] = " Cita creada correctamente";

            }
            else
            {
                TempData["mensaje_error"] = "Error al crear la Cita, por favor valide los datos";
            }
            return RedirectToAction("Index", new { menu });

        }

        public ActionResult cambiarEstado(agenda_mensajero agendaMensajero, int? menu, int[] mensajerias)
        {

            var dataResult = 0;

            for (int i = 0; i < mensajerias.Length; i++)
            {
                var tmp = mensajerias[i];

                agenda_mensajero agenda = db.agenda_mensajero.Find(tmp);
                var error = 0;
                if (agenda != null)
                {
                    agenda.idestado = 4;

                    db.Entry(agenda).State = EntityState.Modified;

                    //busco los recibidotraslados asociados a los detalles (si los hay) y le cambio el estado a 4

                    var detalles = db.detalle_agendamiento.Where(d => d.fk_agendamiento == tmp).ToList();
                    if (detalles.Count > 0)
                    {
                        foreach (var item2 in detalles)
                        {
                            if (item2.recibidotraslados != null)
                            {
                                //busco el recibidotraslado y le pongo el estado 4 (despachado)
                                var reci = db.recibidotraslados.Where(d => d.id == item2.fk_recibidotraslado).FirstOrDefault();
                                if (reci != null)
                                {
                                    reci.estado_traslado = 4;
                                    db.Entry(reci).State = EntityState.Modified;
                                }
                                int resultado = db.SaveChanges();
                                if (resultado == 0)
                                {
                                    error = 1;
                                    break;
                                }
                                //busco si hay otros recibidotraslado para este encabezado que tengan estado menor a 4
                                var existenpendientes = db.recibidotraslados.Where(d => d.id != item2.fk_recibidotraslado && d.estado_traslado < 4).Count();
                                if (existenpendientes == 0)
                                {
                                    //actualizo la solicitud de traslado asociada
                                    var enca = db.encab_documento.Where(d => d.idencabezado == item2.fk_encab_documento).FirstOrDefault();
                                    if (enca != null)
                                    {
                                        if (enca.id_solicitud_traslado != null)
                                        {
                                            var soli = db.Solicitud_traslado.Where(d => d.Id == enca.id_solicitud_traslado).FirstOrDefault();
                                            if (soli != null)
                                            {
                                                soli.Estado_atendido = 4;
                                                db.Entry(soli).State = EntityState.Modified;
                                                var guar = db.SaveChanges();
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }

                    if (error == 0)
                    {
                        TempData["mensaje"] = "Cambio de Estado correctamente";
                        dataResult = 1;
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error al cambiar el estado, por favor valide los datos";
                        dataResult = 0;
                       // break;
                    }
                }
                else
                {

                    TempData["mensaje_error"] = "Error al cambiar el estado, por favor valide los datos (numero de cita no válido)";
                    dataResult = 0;
                    //break;
                }
            }


            return Json(dataResult, JsonRequestBehavior.AllowGet);
        }
    }
}