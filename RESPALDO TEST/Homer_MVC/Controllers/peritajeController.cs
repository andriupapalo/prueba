using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class peritajeController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: peritaje
        public ActionResult Agendar(int? menu)
        {
            icb_sysparameter buscarParametroRolPerito = context.icb_sysparameter.Where(x => x.syspar_cod == "P6").FirstOrDefault();
            string usuarioPeritoParametro = buscarParametroRolPerito != null ? buscarParametroRolPerito.syspar_value : "3";
            int rolPerito = Convert.ToInt32(usuarioPeritoParametro);

            List<SelectListItem> peritos = new List<SelectListItem>();
            List<users> buscarPeritos = context.users.Where(x => x.rol_id == rolPerito).ToList();
            foreach (users perito in buscarPeritos)
            {
                peritos.Add(new SelectListItem
                { Value = perito.user_id.ToString(), Text = perito.user_nombre + " " + perito.user_apellido });
            }

            ViewBag.peritos = peritos;
            ViewBag.rol = Session["user_rolid"];
            BuscarFavoritos(menu);
            return View();
        }

        public void DatosListasDesplegables()
        {
            icb_sysparameter buscarParametroRolPerito = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P6");
            string usuarioPeritoParametro = buscarParametroRolPerito != null ? buscarParametroRolPerito.syspar_value : "3";
            int rolPerito = Convert.ToInt32(usuarioPeritoParametro);

            ViewBag.tpper_id = new SelectList(context.icb_tpperitaje, "tpper_id", "tpper_nombre");
            ViewBag.tpdoc_id = new SelectList(context.tp_documento, "tpdoc_id", "tpdoc_nombre");
            ViewBag.gentercero_id = new SelectList(context.gen_tercero, "gentercero_id", "gentercero_nombre");
            ViewBag.marcvh_id = new SelectList(context.marca_vehiculo, "marcvh_id", "marcvh_nombre");
            ViewBag.modvh_codigo = new SelectList(context.modelo_vehiculo, "modvh_codigo", "modvh_nombre");
            ViewBag.colvh_id = new SelectList(context.color_vehiculo, "colvh_id", "colvh_nombre");
            ViewBag.tpvh_id = new SelectList(context.tipo_vehiculo, "tpvh_id", "tpvh_nombre");
            ViewBag.tpserv_id = new SelectList(context.tpservicio_vehiculo, "tpserv_id", "tpserv_nombre");
            ViewBag.tpcaj_id = new SelectList(context.tpcaja_vehiculo, "tpcaj_id", "tpcaj_nombre");
            ViewBag.tpmot_id = new SelectList(context.tpmotor_vehiculo, "tpmot_id", "tpmot_nombre");
            IOrderedQueryable<nom_ciudad> ciudades = context.nom_ciudad.OrderBy(x => x.ciu_nombre);
            ViewBag.ciu_id = new SelectList(ciudades, "ciu_id", "ciu_nombre");
            ViewBag.ciudad_placa = new SelectList(ciudades, "ciu_id", "ciu_nombre");
            List<users> buscarPeritos = context.users.Where(x => x.rol_id == rolPerito).ToList();
            List<SelectListItem> peritos = new List<SelectListItem>();
            foreach (users perito in buscarPeritos)
            {
                peritos.Add(new SelectListItem
                { Value = perito.user_id.ToString(), Text = perito.user_nombre + " " + perito.user_apellido });
            }

            ViewBag.peritos = peritos;

            List<icb_horas_peritaje> buscarHoras = context.icb_horas_peritaje.OrderBy(x => x.hora).ToList();
            List<SelectListItem> horas = new List<SelectListItem>();
            foreach (icb_horas_peritaje hora in buscarHoras)
            {
                horas.Add(new SelectListItem
                {
                    Value = hora.horas_id.ToString(),
                    Text = hora.hora.Value.Hours.ToString("00") + ":" + hora.hora.Value.Minutes.ToString("00")
                });
            }

            ViewBag.horas = horas;
        }

        public JsonResult ValidarCitaAtendida(int id)
        {
            var buscarCita = (from citaPerito in context.icb_cita_perito
                              join solicitud in context.icb_solicitud_peritaje
                                  on citaPerito.solicitud_peritaje_id equals solicitud.id_peritaje
                              where solicitud.id_peritaje == id
                              select new
                              {
                                  citaPerito.cita_fecha_inicio,
                                  citaPerito.cita_fecha_fin,
                                  solicitud.atendida,
                                  solicitud.cancelada,
                                  solicitud.reprogramada
                              }).FirstOrDefault();
            if (buscarCita != null)
            {
                if (buscarCita.atendida)
                {
                    return Json(new { estado = 1, mensaje = "La cita ya fue atendida" }, JsonRequestBehavior.AllowGet);
                }

                if (buscarCita.cancelada)
                {
                    return Json(new { estado = 2, mensaje = "La cita fue cancelada" }, JsonRequestBehavior.AllowGet);
                }

                if (buscarCita.cita_fecha_inicio >= DateTime.Now && buscarCita.cita_fecha_fin >= DateTime.Now)
                {
                    return Json(new { estado = 3, mensaje = "" /*No ha llegado a la hora de inicio*/},
                        JsonRequestBehavior.AllowGet);
                }

                if (buscarCita.cita_fecha_inicio <= DateTime.Now && buscarCita.cita_fecha_fin >= DateTime.Now)
                {
                    return Json(new { estado = 4, mensaje = "" /*No se ha vencido y no he atendido*/},
                        JsonRequestBehavior.AllowGet);
                }

                return Json(
                    new
                    {
                        estado = 4,
                        mensaje = "" /*Ya se vencio la hora y no he atendido | La fecha y hora de la solicitud ya estan vencidas */
                    }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { estado = 6, mensaje = "No se encontro la solicitud" /*No se encontro al solicitud*/},
                JsonRequestBehavior.AllowGet);

            //var buscarPeritaje = (from encabezadoPeritaje in context.icb_encabezado_insp_peritaje
            //                     join solicitud in context.icb_solicitud_peritaje
            //                     on encabezadoPeritaje.encab_insper_idsolicitud equals solicitud.id_peritaje
            //                     select encabezadoPeritaje).FirstOrDefault();
            //if (buscarPeritaje != null)
            //{
            //    return Json(true, JsonRequestBehavior.AllowGet);
            //}
            //return Json(false,JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarHorariosPeritajes()
        {
            List<icb_horas_peritaje> buscarHoras = context.icb_horas_peritaje.OrderBy(x => x.hora).ToList();
            List<SelectListItem> horas = new List<SelectListItem>();
            foreach (icb_horas_peritaje hora in buscarHoras)
            {
                horas.Add(new SelectListItem
                {
                    Value = hora.horas_id.ToString(),
                    Text = hora.hora.Value.Hours.ToString("00") + ":" + hora.hora.Value.Minutes.ToString("00")
                });
            }

            return Json(horas, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarPlaca(string placa)
        {
            var buscarPlaca = (from vehiculo in context.icb_vehiculo
                               join marca in context.marca_vehiculo
                                   on vehiculo.marcvh_id equals marca.marcvh_id
                               join modelo in context.modelo_vehiculo
                                   on vehiculo.modvh_id equals modelo.modvh_codigo
                               join tipoVehiculo in context.tp_vehiculo
                                   on vehiculo.tpvh_id equals tipoVehiculo.id
                               join color in context.color_vehiculo
                                   on vehiculo.colvh_id equals color.colvh_id
                               join tpServicio in context.tpservicio_vehiculo
                                   on vehiculo.tiposervicio equals tpServicio.tpserv_id into serv
                               from tpServicio in serv.DefaultIfEmpty()
                               join ciudadPlaca in context.nom_ciudad
                                   on vehiculo.ciudadplaca equals ciudadPlaca.ciu_id into ps
                               from ciudadPlaca in ps.DefaultIfEmpty()
                               join retoma in context.icb_tercero_vhretoma
                                   on placa equals retoma.placa into temp
                               from retoma in temp.DefaultIfEmpty()
                               join solPeritaje in context.icb_solicitud_peritaje
                                   on retoma.veh_id equals solPeritaje.id_tercero_vhretoma into temp1
                               from solPeritaje in temp1.DefaultIfEmpty()
                               where vehiculo.plac_vh == placa
                               select new
                               {
                                   marca.marcvh_id,
                                   modelo.modvh_codigo,
                                   vehiculo.anio_vh,
                                   color.colvh_id,
                                   tpserv_id = tpServicio != null ? tpServicio.tpserv_id : 0,
                                   tpVehiculo_id = tipoVehiculo.id,
                                   vehiculo.vin,
                                   motor = vehiculo.nummot_vh,
                                   tipocaja = modelo != null ? modelo.tipocaja : 0,
                                   cilindraje = modelo.cilindraje != null ? modelo.cilindraje.ToString() : "",
                                   ciu_id = ciudadPlaca != null ? ciudadPlaca.ciu_id.ToString() : "",
                                   combustible = modelo.combustible != null ? modelo.combustible : 0,
                                   vehiculo.kilometraje,
                                   previo = (from retoma in temp.DefaultIfEmpty()
                                             join solPeritaje in context.icb_solicitud_peritaje
                                                 on retoma.veh_id equals solPeritaje.id_tercero_vhretoma into temp1
                                             from solPeritaje in temp1.DefaultIfEmpty()
                                             orderby solPeritaje.fecha_agenda_peritaje descending
                                             select solPeritaje.fecha_agenda_peritaje
                                       ).FirstOrDefault()
                               }).FirstOrDefault();
            if (buscarPlaca != null)
            {
                var modelos = (from modelo in context.modelo_vehiculo
                               where modelo.mar_vh_id == buscarPlaca.marcvh_id
                               select new
                               {
                                   modelo.modvh_codigo,
                                   modelo.modvh_nombre
                               }).ToList();
                var anios = (from anio in context.anio_modelo
                             where anio.codigo_modelo == buscarPlaca.modvh_codigo
                             select new
                             {
                                 anio.anio
                             }).ToList();
                decimal parametros = context.vvencimientoperitaje.FirstOrDefault().valorTiempo;
                if (buscarPlaca.previo != null)
                {
                    DateTime fechaActual = DateTime.Now;
                    DateTime fechaPeritaje = buscarPlaca.previo;
                    bool conver = int.TryParse(parametros.ToString(), out int mesex);
                    DateTime fechavencimientoPeritaje = fechaPeritaje.AddMonths(mesex);
                    if (fechavencimientoPeritaje < fechaActual)
                    {
                        return Json(
                            new { encontrado = true, buscarPlaca, modelos, anios, vigente = true, peritaje = true },
                            JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { encontrado = true, buscarPlaca, modelos, anios, vigente = false, peritaje = true },
                        JsonRequestBehavior.AllowGet);
                }

                return Json(new { encontrado = true, buscarPlaca, modelos, anios, vigente = true, peritaje = false },
                    JsonRequestBehavior.AllowGet);
            }

            return Json(new { encontrado = false }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Solicitud(int? cotizacion, int? menu)
        {
            if (cotizacion != null)
            {
                var buscarCliente = (from cotizaciones in context.icb_cotizacion
                                     join tercero in context.icb_terceros
                                         on cotizaciones.id_tercero equals tercero.tercero_id
                                     where cotizaciones.cot_numcotizacion == cotizacion
                                     select new
                                     {
                                         tercero.doc_tercero
                                     }).FirstOrDefault();
                ViewBag.docTercero = buscarCliente != null ? buscarCliente.doc_tercero : "";
            }
            else
            {
                ViewBag.docTercero = "";
            }

            DatosListasDesplegables();
            ViewBag.rol = Session["user_rolid"];
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Solicitud(SolicitudPeritajeModel modelo, int? menu)
        {
            //***************************
            string fechaInicio1 = modelo.fecha_peritaje.ToShortDateString() + " " + modelo.horaInicio;
            DateTime fechaYHoraInicio1 = DateTime.Parse(fechaInicio1);
            string fechaFin1 = modelo.fecha_peritaje.ToShortDateString() + " " + modelo.horaFin;
            DateTime fechaYHoraFin1 = DateTime.Parse(fechaFin1);
            bool horarioDisponible1 = CalcularDisponible(modelo.perito_id, modelo.fecha_peritaje, fechaYHoraInicio1,
                fechaYHoraFin1);
            //*************

            if (ModelState.IsValid)
            {
                //var horaModeloInicio = context.icb_horas_peritaje.FirstOrDefault(x => x.horas_id == modelo.horaInicio).hora;
                string fechaInicio = modelo.fecha_peritaje.ToShortDateString() + " " + modelo.horaInicio;
                DateTime fechaYHoraInicio = DateTime.Parse(fechaInicio);

                //var horaModeloFin = context.icb_horas_peritaje.FirstOrDefault(x => x.horas_id == modelo.horaFin).hora;
                string fechaFin = modelo.fecha_peritaje.ToShortDateString() + " " + modelo.horaFin;
                DateTime fechaYHoraFin = DateTime.Parse(fechaFin);

                if (DateTime.Compare(DateTime.Now, fechaYHoraInicio) > 0)
                {
                    TempData["mensajeError"] = "Debe asignar una fecha valida mayor a la fecha y hora actual";
                    DatosListasDesplegables();
                    BuscarFavoritos(menu);
                    return View(modelo);
                }

                if (DateTime.Compare(fechaYHoraInicio, fechaYHoraFin) >= 0)
                {
                    TempData["mensajeError"] =
                        "La hora de inicio de la solicitud no puede ser menor o igual a la hora de fin";
                    DatosListasDesplegables();
                    BuscarFavoritos(menu);
                    return View(modelo);
                }

                bool horarioDisponible = CalcularDisponible(modelo.perito_id, modelo.fecha_peritaje, fechaYHoraInicio,
                    fechaYHoraFin);

                // Si horario disponible es verdadero significa que la fecha y hora estblecida para asignar el peritaje si se encuentra libre
                if (horarioDisponible)
                {
                    //Primero hay que revisar que el tercero exista o crear el tercero en caso de que no
                    icb_terceros buscaDocumentoTercero =
                        context.icb_terceros.FirstOrDefault(x => x.doc_tercero == modelo.doc_tercero);
                    int idTercero = 0;
                    if (buscaDocumentoTercero != null)
                    {
                        idTercero = buscaDocumentoTercero.tercero_id;
                        buscaDocumentoTercero.tpdoc_id = modelo.tpdoc_id;
                        buscaDocumentoTercero.prinom_tercero = modelo.prinom_tercero;
                        buscaDocumentoTercero.segnom_tercero = modelo.segnom_tercero;
                        buscaDocumentoTercero.apellido_tercero = modelo.apellido_tercero;
                        buscaDocumentoTercero.segapellido_tercero = modelo.segapellido_tercero;
                        buscaDocumentoTercero.genero_tercero = modelo.gentercero_id;
                        buscaDocumentoTercero.telf_tercero = modelo.telf_tercero;
                        buscaDocumentoTercero.celular_tercero = modelo.celular_tercero;
                        buscaDocumentoTercero.email_tercero = modelo.email_tercero;
                        buscaDocumentoTercero.ciu_id = modelo.ciu_id;
                        context.Entry(buscaDocumentoTercero).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                    else
                    {
                        // Si el tercero no existe entonces se crea
                        context.icb_terceros.Add(new icb_terceros
                        {
                            tercerofec_creacion = DateTime.Now,
                            tercerouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            prinom_tercero = modelo.prinom_tercero,
                            segnom_tercero = modelo.segnom_tercero,
                            apellido_tercero = modelo.apellido_tercero,
                            segapellido_tercero = modelo.segapellido_tercero,
                            doc_tercero = modelo.doc_tercero,
                            telf_tercero = modelo.telf_tercero,
                            celular_tercero = modelo.celular_tercero,
                            email_tercero = modelo.email_tercero,
                            ciu_id = modelo.ciu_id,
                            tpdoc_id = modelo.tpdoc_id,
                            genero_tercero = modelo.gentercero_id
                        });
                        bool guardarTerceroNuevo = context.SaveChanges() > 0;
                        if (guardarTerceroNuevo)
                        {
                            icb_terceros nuevoTercero = context.icb_terceros.OrderByDescending(x => x.tercero_id)
                                .FirstOrDefault();
                            idTercero = nuevoTercero.tercero_id;
                        }
                    }

                    context.icb_tercero_vhretoma.Add(new icb_tercero_vhretoma
                    {
                        marca_id = modelo.marcvh_id,
                        modelo_codigo = modelo.modvh_codigo,
                        tp_vehiculo = modelo.tpvh_id,
                        placa = modelo.plac_vh,
                        ciudad_placa = modelo.ciudad_placa,
                        servicio = modelo.tpserv_id,
                        color = modelo.colvh_id,
                        cilindraje = modelo.cilindraje,
                        tp_caja = modelo.tpcaj_id,
                        serie = modelo.serie,
                        numero_motor = modelo.num_motor,
                        tp_motor = modelo.tpmot_id,
                        tercero_id = idTercero,
                        anio = modelo.anioTxt
                    });

                    icb_vehiculo buscarVehiculoPorPlaca = context.icb_vehiculo.FirstOrDefault(x => x.plac_vh == modelo.plac_vh);
                    if (buscarVehiculoPorPlaca != null)
                    {
                        buscarVehiculoPorPlaca.marcvh_id = modelo.marcvh_id;
                        buscarVehiculoPorPlaca.modvh_id = modelo.modvh_codigo;
                        buscarVehiculoPorPlaca.tpvh_id = modelo.tpvh_id;
                        buscarVehiculoPorPlaca.plac_vh = modelo.plac_vh;
                        buscarVehiculoPorPlaca.ciudadplaca = modelo.ciudad_placa;
                        buscarVehiculoPorPlaca.colvh_id = modelo.colvh_id;
                        buscarVehiculoPorPlaca.vin = modelo.serie;
                        buscarVehiculoPorPlaca.nummot_vh = modelo.num_motor;
                        buscarVehiculoPorPlaca.icbvhfec_actualizacion = DateTime.Now;
                        buscarVehiculoPorPlaca.icbvhuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarVehiculoPorPlaca.anio_vh = modelo.anioTxt;
                        buscarVehiculoPorPlaca.propietario = idTercero;
                        context.Entry(buscarVehiculoPorPlaca).State = EntityState.Modified;
                    }
                    else
                    {
                        // Tener en cuenta que hay que modificar la tabla icb_tercero_vhretoma para que los datos de guarden en la tabla icb_vehiculo
                        context.icb_vehiculo.Add(new icb_vehiculo
                        {
                            marcvh_id = modelo.marcvh_id,
                            modvh_id = modelo.modvh_codigo,
                            tpvh_id = modelo.tpvh_id,
                            plac_vh = modelo.plac_vh,
                            ciudadplaca = modelo.ciudad_placa,
                            colvh_id = modelo.colvh_id,
                            vin = modelo.serie,
                            nummot_vh = modelo.num_motor,
                            plan_mayor = modelo.plac_vh,
                            icbvhfec_creacion = DateTime.Now,
                            icbvhuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            anio_vh = modelo.anioTxt,
                            propietario = idTercero,
                            id_bod = Convert.ToInt32(Session["user_bodega"])
                        });
                    }

                    bool guardarVhRetoma = context.SaveChanges() > 0;

                    if (guardarVhRetoma)
                    {
                        icb_tercero_vhretoma vhRetomaActual = context.icb_tercero_vhretoma.OrderByDescending(x => x.veh_id)
                            .FirstOrDefault();

                        var buscarPlaca = (from solicitud in context.icb_solicitud_peritaje
                                           join vehiculo in context.icb_tercero_vhretoma
                                               on solicitud.id_tercero_vhretoma equals vehiculo.veh_id
                                           where vehiculo.placa == modelo.plac_vh
                                           orderby solicitud.fecha_agenda_peritaje descending
                                           select new
                                           {
                                               solicitud.id_peritaje,
                                               solicitud.atendida,
                                               solicitud.cancelada,
                                               solicitud.fecha_agenda_peritaje
                                           }).FirstOrDefault();
                        if (buscarPlaca != null)
                        {
                            if (buscarPlaca.atendida != true && buscarPlaca.cancelada != true &&
                                buscarPlaca.fecha_agenda_peritaje > DateTime.Now)
                            {
                                TempData["mensajeError"] =
                                    "La placa ya se encuentra registrada con un peritaje para la fecha " +
                                    buscarPlaca.fecha_agenda_peritaje.ToShortDateString();
                                DatosListasDesplegables();
                                BuscarFavoritos(menu);
                                return View(modelo);
                            }
                        }

                        // Aqui se guarda el nuevo registro de icb_solicitud peritaje, ya se guardo los datos del vehiculo en retoma, ahora se guarda la solicitud
                        context.icb_solicitud_peritaje.Add(new icb_solicitud_peritaje
                        {
                            id_tercero_vhretoma = vhRetomaActual.veh_id,
                            fecha_agenda_peritaje = fechaYHoraInicio,
                            tp_peritaje = modelo.tpper_id
                        });
                        bool guardarSolicitud = context.SaveChanges() > 0;

                        // Significa que se guardo bien la solicitud y solo falta agregar las horas al perito en la tabla icb_cita_perito
                        if (guardarSolicitud)
                        {
                            icb_solicitud_peritaje solicitudActual = context.icb_solicitud_peritaje.OrderByDescending(x => x.id_peritaje)
                                .FirstOrDefault();

                            context.icb_cita_perito.Add(new icb_cita_perito
                            {
                                perito_id = modelo.perito_id,
                                solicitud_peritaje_id = solicitudActual.id_peritaje,
                                cita_fecha_inicio = fechaYHoraInicio,
                                cita_fecha_fin = fechaYHoraFin
                            });
                            bool guardarCita = context.SaveChanges() > 0;
                            // Guardar cita significa que la hora incio y final con el respectivo perito ya estan guardados correctamente
                            if (guardarCita)
                            {
                                DatosListasDesplegables();
                                BuscarFavoritos(menu);
                                TempData["mensajeCorrecto"] = "La cita se asigno correctamente";
                                return RedirectToAction("Solicitud", new { menu });
                                //return View(modelo);
                            }
                        }
                    }
                }
                else
                {
                    TempData["mensajeError"] = "La hora asignada no se encuentra disponible. Verifique la hora de descanso y/o la hora de atención.";
                    DatosListasDesplegables();
                    BuscarFavoritos(menu);
                    return RedirectToAction("Solicitud", new { menu });
                }
            }

            DatosListasDesplegables();
            BuscarFavoritos(menu);
            return View(modelo);
        }

        [HttpPost]
        public JsonResult BuscarHorario(int perito_id)
        {
            List<parametrizacion_horario> horarioPerito = context.parametrizacion_horario.Where(x => x.usuario_id == perito_id).ToList();

            var data = horarioPerito.Select(x => new
            {
                lunes = x.lunes_disponible,
                lunes1 = x.lunes_desde != null ? x.lunes_desde.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) + " - " + x.lunes_hasta.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) : "",
                lunes2 = x.lunes_desde2 != null ? x.lunes_desde2.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) + " - " + x.lunes_hasta2.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) : "",
                martes = x.martes_disponible,
                martes1 = x.martes_desde != null ? x.martes_desde.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) + " - " + x.martes_hasta.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) : "",
                martes2 = x.martes_desde2 != null ? x.martes_desde2.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) + " - " + x.martes_hasta2.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) : "",
                miercoles = x.miercoles_disponible,
                miercoles1 = x.miercoles_desde != null ? x.miercoles_desde.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) + " - " + x.miercoles_hasta.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) : "",
                miercoles2 = x.miercoles_desde2 != null ? x.miercoles_desde2.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) + " - " + x.miercoles_hasta2.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) : "",
                jueves = x.jueves_disponible,
                jueves1 = x.jueves_desde != null ? x.jueves_desde.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) + " - " + x.jueves_hasta.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) : "",
                jueves2 = x.jueves_desde2 != null ? x.jueves_desde2.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) + " - " + x.jueves_hasta2.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) : "",
                viernes = x.viernes_disponible,
                viernes1 = x.viernes_desde != null ? x.viernes_desde.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) + " - " + x.viernes_hasta.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) : "",
                viernes2 = x.viernes_desde2 != null ? x.viernes_desde2.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) + " - " + x.viernes_hasta2.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) : "",
                sabado = x.sabado_disponible,
                sabado1 = x.sabado_desde != null ? x.sabado_desde.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) + " - " + x.sabado_hasta.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) : "",
                sabado2 = x.sabado_desde2 != null ? x.sabado_desde2.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) + " - " + x.sabado_hasta2.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) : "",
                domingo = x.domingo_disponible,
                domingo1 = x.domingo_desde != null ? x.domingo_desde.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) + " - " + x.domingo_hasta.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) : "",
                domingo2 = x.domingo_desde2 != null ? x.domingo_desde2.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) + " - " + x.domingo_hasta2.Value.ToString(@"hh\:mm", new CultureInfo("en-US")) : "",
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ReprogramarCita(string placa, DateTime desde, DateTime hasta)
        {
            string placaFinal = placa.Substring(1, 6);
            var buscarPlaca = (from solicitud in context.icb_solicitud_peritaje
                               join vehiculo in context.icb_tercero_vhretoma
                                   on solicitud.id_tercero_vhretoma equals vehiculo.veh_id
                               join vhRetoma in context.icb_tercero_vhretoma
                                   on solicitud.id_tercero_vhretoma equals vhRetoma.veh_id
                               where vhRetoma.placa == placaFinal
                               orderby solicitud.fecha_agenda_peritaje descending
                               select new
                               {
                                   solicitud.id_peritaje
                               }).FirstOrDefault();
            if (buscarPlaca != null)
            {
                DateTime fechaYHoraInicio = desde;
                DateTime fechaYHoraFin = hasta;
                DateTime fecha = desde.Date;


                if (DateTime.Compare(DateTime.Now, fechaYHoraInicio) > 0)
                {
                    var respuesta = new
                    { estado = false, mensaje = "Debe asignar una fecha valida mayor a la fecha y hora actual" };
                    return Json(respuesta, JsonRequestBehavior.AllowGet);
                }

                if (DateTime.Compare(fechaYHoraInicio, fechaYHoraFin) >= 0)
                {
                    var respuesta = new
                    {
                        estado = false,
                        mensaje = "La hora de inicio de la solicitud no puede ser menor o igual a la hora de fin"
                    };
                    return Json(respuesta, JsonRequestBehavior.AllowGet);
                }

                int peritoId = Convert.ToInt32(Session["user_usuarioid"]);
                bool horarioDisponible = CalcularDisponible(peritoId, fecha, fechaYHoraInicio, fechaYHoraFin);
                if (horarioDisponible)
                {
                    icb_solicitud_peritaje buscarSolicitud =
                        context.icb_solicitud_peritaje.FirstOrDefault(x => x.id_peritaje == buscarPlaca.id_peritaje);
                    buscarSolicitud.fec_cita_anterior = buscarSolicitud.fecha_agenda_peritaje;
                    buscarSolicitud.fecha_agenda_peritaje = fechaYHoraInicio;
                    buscarSolicitud.reprogramada = true;
                    context.Entry(buscarSolicitud).State = EntityState.Modified;

                    icb_cita_perito buscarcitaPerito =
                        context.icb_cita_perito.FirstOrDefault(x => x.solicitud_peritaje_id == buscarPlaca.id_peritaje);
                    buscarcitaPerito.cita_fecha_inicio = fechaYHoraInicio;
                    buscarcitaPerito.cita_fecha_fin = fechaYHoraFin;
                    context.Entry(buscarcitaPerito).State = EntityState.Modified;
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        var respuesta = new { estado = true, mensaje = "La cita se ha modificado exitosamente" };
                        return Json(respuesta, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var respuesta = new { estado = false, mensaje = "Error al guardar, verifique su conexion..." };
                        return Json(respuesta, JsonRequestBehavior.AllowGet);
                    }
                }

                {
                    var respuesta = new
                    { estado = false, mensaje = "La fecha y hora seleccionadas no se encuentran disponibles" };
                    return Json(respuesta, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CancelarCita(string placa)
        {
            var buscarPlaca = (from solicitud in context.icb_solicitud_peritaje
                               join vehiculo in context.icb_tercero_vhretoma
                                   on solicitud.id_tercero_vhretoma equals vehiculo.veh_id
                               join vhRetoma in context.icb_tercero_vhretoma
                                   on solicitud.id_tercero_vhretoma equals vhRetoma.veh_id
                               where vhRetoma.placa == placa
                               orderby solicitud.fecha_agenda_peritaje descending
                               select new
                               {
                                   solicitud.id_peritaje
                               }).FirstOrDefault();
            if (buscarPlaca != null)
            {
                icb_solicitud_peritaje buscarSolicitud =
                    context.icb_solicitud_peritaje.FirstOrDefault(x => x.id_peritaje == buscarPlaca.id_peritaje);
                buscarSolicitud.cancelada = true;
                context.Entry(buscarSolicitud).State = EntityState.Modified;
                int guardar = context.SaveChanges();
                if (guardar > 0)
                {
                    return Json("La cita ha sido cancelada", JsonRequestBehavior.AllowGet);
                }

                return Json("Error de conexion...", JsonRequestBehavior.AllowGet);
            }

            return Json("Placa no encontrada", JsonRequestBehavior.AllowGet);
        }

        public JsonResult AgregarClasifNegocio(int idEncabezado, int idClasificacion)
        {
            icb_encabezado_insp_peritaje buscarEncabezado =
                context.icb_encabezado_insp_peritaje.FirstOrDefault(x => x.encab_insper_id == idEncabezado);
            if (buscarEncabezado != null)
            {
                buscarEncabezado.encab_insper_clasificacion = idClasificacion;
                context.Entry(buscarEncabezado).State = EntityState.Modified;
                bool guardar = context.SaveChanges() > 0;
                if (guardar)
                {
                    icb_solicitud_peritaje buscarSolicitud = context.icb_solicitud_peritaje.FirstOrDefault(x =>
                        x.id_peritaje == buscarEncabezado.encab_insper_idsolicitud);
                    icb_tercero_vhretoma buscarVehiculoRetoma =
                        context.icb_tercero_vhretoma.FirstOrDefault(
                            x => x.veh_id == buscarSolicitud.id_tercero_vhretoma);

                    icb_vehiculo buscarPlanMayor =
                        context.icb_vehiculo.FirstOrDefault(x => x.plan_mayor == buscarVehiculoRetoma.placa);
                    if (buscarPlanMayor == null)
                    {
                        string eventoCompraUsado = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P20")
                            .syspar_value;

                        icb_vehiculo nuevoVehiculo = new icb_vehiculo
                        {
                            icbvhfec_creacion = DateTime.Now,
                            icbvhuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            icbvh_estado = true,
                            marcvh_id = buscarVehiculoRetoma.marca_id,
                            modvh_id = buscarVehiculoRetoma.modelo_codigo,
                            tpvh_id = buscarVehiculoRetoma.tp_vehiculo,
                            colvh_id = buscarVehiculoRetoma.color,
                            plac_vh = buscarVehiculoRetoma.placa,
                            vin = buscarVehiculoRetoma.serie,
                            nummot_vh = buscarVehiculoRetoma.numero_motor,
                            icbvh_estatus = "0",
                            id_bod = 1,
                            anio_vh = buscarVehiculoRetoma.anio,
                            plan_mayor = buscarVehiculoRetoma.placa,
                            id_evento = Convert.ToInt32(eventoCompraUsado)
                        };
                        context.icb_vehiculo.Add(nuevoVehiculo);
                        bool guardarVehiculo = context.SaveChanges() > 0;
                        if (guardarVehiculo)
                        {
                            return Json("1", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        icb_terceros buscaPropietario =
                            context.icb_terceros.FirstOrDefault(x => x.tercero_id == buscarVehiculoRetoma.tercero_id);
                        int idPropietario = buscaPropietario != null ? buscaPropietario.tercero_id : 0;
                        if (buscarPlanMayor != null && buscarPlanMayor.propietario != idPropietario)
                        {
                            buscarPlanMayor.propietario = idPropietario;
                            context.Entry(buscarPlanMayor).State = EntityState.Modified;
                            bool actualizar = context.SaveChanges() > 0;
                            if (actualizar)
                            {
                                return Json("2", JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            return Json("-1", JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }

            return Json("-2", JsonRequestBehavior.AllowGet);
        }

        public ActionResult CompraRetoma(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult BuscarRegistrosCompraRetoma()
        {
            var buscarVehiculosUsados = from vehiculo in context.icb_vehiculo
                                        join marca in context.marca_vehiculo
                                            on vehiculo.marcvh_id equals marca.marcvh_id
                                        join modelo in context.modelo_vehiculo
                                            on vehiculo.modvh_id equals modelo.modvh_codigo
                                        join color in context.color_vehiculo
                                            on vehiculo.colvh_id equals color.colvh_id
                                        where vehiculo.tpvh_id == 5
                                        select new
                                        {
                                            marca.marcvh_nombre,
                                            modelo.modvh_nombre,
                                            color.colvh_nombre,
                                            vehiculo.plac_vh,
                                            vehiculo.nummot_vh
                                        };
            return Json(buscarVehiculosUsados, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Peritajes(int? menu)
        {
            ViewBag.clasifNegocio = context.icb_clanegocio;
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult BuscarCitasPerito(int peritoId)
        {
            //var citas = context.icb_cita_perito.Where(x=>x.perito_id==peritoId).ToList();
            var citas = (from cita in context.icb_cita_perito
                         join solicitud in context.icb_solicitud_peritaje
                             on cita.solicitud_peritaje_id equals solicitud.id_peritaje
                         join vehiculo in context.icb_tercero_vhretoma
                             on solicitud.id_tercero_vhretoma equals vehiculo.veh_id
                         join t in context.icb_terceros
                             on vehiculo.tercero_id equals t.tercero_id
                         where cita.perito_id == peritoId && solicitud.cancelada == false
                         select new
                         {
                             cita.cita_fecha_inicio,
                             cita.cita_fecha_fin,
                             vehiculo.placa,
                             solicitud.id_peritaje,
                             nombre_cliente = "(" + vehiculo.placa + ") " + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                              t.apellido_tercero + " " + t.segapellido_tercero
                         }).ToList();

            var events = citas.Select(x => new
            {
                title = x.nombre_cliente,
                start = x.cita_fecha_inicio.ToString("MM/dd/yyyy") + " " + x.cita_fecha_inicio.Hour.ToString("00") +
                        ":" + x.cita_fecha_inicio.Minute.ToString("00") + ":00",
                end = x.cita_fecha_fin.ToString("MM/dd/yyyy") + " " + x.cita_fecha_fin.Hour.ToString("00") + ":" +
                      x.cita_fecha_fin.Minute.ToString("00") + ":00",
                id = x.id_peritaje
            });

            return Json(events, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDetallesPeritaje(int id)
        {
            var detalles = (from inspEnca in context.icb_insp_peritaje
                            join zona in context.icb_zonaperitaje
                                on inspEnca.insp_zona_id equals zona.zonaper_id
                            join pieza in context.icb_piezaperitaje
                                on inspEnca.insp_pieza_id equals pieza.pieza_id
                            join convencion in context.icb_conveperitaje
                                on inspEnca.insp_conve_id equals convencion.conve_id
                            where inspEnca.insp_encabezado_id == id
                            select new
                            {
                                zona.zonaper_nombre,
                                pieza.pieza_nombre,
                                convencion.conve_nombre,
                                inspEnca.insp_comentario
                            }).ToList();
            var datosPeritaje = (from encabezado in context.icb_encabezado_insp_peritaje
                                 join solicitud in context.icb_solicitud_peritaje
                                     on encabezado.encab_insper_idsolicitud equals solicitud.id_peritaje
                                 join terVhRet in context.icb_tercero_vhretoma
                                     on solicitud.id_tercero_vhretoma equals terVhRet.veh_id
                                 join cita in context.icb_cita_perito
                                     on encabezado.encab_insper_idsolicitud equals cita.solicitud_peritaje_id
                                 join modelo in context.modelo_vehiculo
                                     on terVhRet.modelo_codigo equals modelo.modvh_codigo
                                 join perito in context.users
                                     on cita.perito_id equals perito.user_id
                                 where encabezado.encab_insper_id == id
                                 select new
                                 {
                                     modelo.modvh_nombre,
                                     terVhRet.placa,
                                     perito.user_nombre,
                                     perito.user_apellido,
                                     encabezado.encab_insper_fecha
                                 }).FirstOrDefault();
            var datosPeritajeFormato = new
            {
                datosPeritaje.modvh_nombre,
                datosPeritaje.placa,
                datosPeritaje.user_nombre,
                datosPeritaje.user_apellido,
                encab_insper_fecha = datosPeritaje.encab_insper_fecha.ToLongDateString() + " " +
                                     datosPeritaje.encab_insper_fecha.ToShortTimeString()
            };
            var data = new
            {
                detalles,
                datosPeritajeFormato
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPeritajes()
        {
            // 3 es rol perito
            if (Session["user_rolid"].ToString() == "3")
            {
                int peritoActual = Convert.ToInt32(Session["user_usuarioid"]);
                var peritajes = (from encabezado in context.icb_encabezado_insp_peritaje
                                 join solicitud in context.icb_solicitud_peritaje
                                     on encabezado.encab_insper_idsolicitud equals solicitud.id_peritaje
                                 join cita in context.icb_cita_perito
                                     on solicitud.id_peritaje equals cita.solicitud_peritaje_id
                                 join terVhRetoma in context.icb_tercero_vhretoma
                                     on solicitud.id_tercero_vhretoma equals terVhRetoma.veh_id
                                 join tercero in context.icb_terceros
                                     on terVhRetoma.tercero_id equals tercero.tercero_id
                                 join modelo in context.modelo_vehiculo
                                     on terVhRetoma.modelo_codigo equals modelo.modvh_codigo
                                 join clasificacion in context.icb_clas_peritaje
                                     on encabezado.encab_insper_idclasper equals clasificacion.claper_id
                                 where cita.perito_id == peritoActual
                                 select new
                                 {
                                     encabezado.encab_insper_id,
                                     tercero.prinom_tercero,
                                     tercero.segnom_tercero,
                                     tercero.apellido_tercero,
                                     tercero.segapellido_tercero,
                                     modelo.modvh_nombre,
                                     terVhRetoma.placa,
                                     clasificacion.claper_nombre
                                 }).ToList();

                return Json(peritajes, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var peritajes = (from encabezado in context.icb_encabezado_insp_peritaje
                                 join solicitud in context.icb_solicitud_peritaje
                                     on encabezado.encab_insper_idsolicitud equals solicitud.id_peritaje
                                 join terVhRetoma in context.icb_tercero_vhretoma
                                     on solicitud.id_tercero_vhretoma equals terVhRetoma.veh_id
                                 join tercero in context.icb_terceros
                                     on terVhRetoma.tercero_id equals tercero.tercero_id
                                 join modelo in context.modelo_vehiculo
                                     on terVhRetoma.modelo_codigo equals modelo.modvh_codigo
                                 join clasificacion in context.icb_clas_peritaje
                                     on encabezado.encab_insper_idclasper equals clasificacion.claper_id
                                 select new
                                 {
                                     encabezado.encab_insper_id,
                                     tercero.prinom_tercero,
                                     tercero.segnom_tercero,
                                     tercero.apellido_tercero,
                                     tercero.segapellido_tercero,
                                     modelo.modvh_nombre,
                                     terVhRetoma.placa,
                                     clasificacion.claper_nombre
                                 }).ToList();

                return Json(peritajes, JsonRequestBehavior.AllowGet);
            }
        }

        //public bool buscarHorario(DateTime fechaInicio,DateTime fechaFin) {
        //    var agendaPeritoFechaActual = context.icb_cita_perito.Where(x=>x.cita_fecha_inicio.Year==fechaInicio.Year&&x.cita_fecha_inicio.Month==fechaInicio.Month&&x.cita_fecha_inicio.Day==fechaInicio.Day).ToList();
        //    foreach (var horas in agendaPeritoFechaActual) {
        //        if (DateTime.Compare(fechaInicio, horas.cita_fecha_inicio) == 0) {
        //            return false;
        //        } else if (DateTime.Compare(fechaInicio, horas.cita_fecha_inicio) >= 0 && DateTime.Compare(fechaFin, horas.cita_fecha_fin) <= 0) {
        //            return false;
        //        } else if (DateTime.Compare(fechaInicio, horas.cita_fecha_inicio) < 0 && DateTime.Compare(fechaFin, horas.cita_fecha_inicio) > 0) {
        //            return false;
        //        }
        //    }
        //    return true;
        //}
        //var buscaCita = (from citasPerito in context.icb_cita_perito
        //                      where citasPerito.perito_id == modelo.perito_id
        //                      && (DbFunctions.CreateTime(citasPerito.cita_fecha_inicio.Hour, citasPerito.cita_fecha_inicio.Minute, citasPerito.cita_fecha_inicio.Second) >
        //                      DbFunctions.CreateTime(fechaYHoraInicio.Hour, fechaYHoraInicio.Minute, fechaYHoraInicio.Second)                                
        //                      && DbFunctions.CreateTime(citasPerito.cita_fecha_inicio.Hour, citasPerito.cita_fecha_inicio.Minute, citasPerito.cita_fecha_inicio.Second) >=
        //                      DbFunctions.CreateTime(fechaYHoraFin.Hour, fechaYHoraFin.Minute, fechaYHoraFin.Second))

        //                      || (DbFunctions.CreateTime(citasPerito.cita_fecha_fin.Hour, citasPerito.cita_fecha_fin.Minute, citasPerito.cita_fecha_fin.Second) <=
        //                      DbFunctions.CreateTime(fechaYHoraInicio.Hour, fechaYHoraInicio.Minute, fechaYHoraInicio.Second)
        //                      && DbFunctions.CreateTime(citasPerito.cita_fecha_fin.Hour, citasPerito.cita_fecha_fin.Minute, citasPerito.cita_fecha_fin.Second) >
        //                      DbFunctions.CreateTime(fechaYHoraFin.Hour, fechaYHoraFin.Minute, fechaYHoraFin.Second))


        //                      select citasPerito.cita_id).ToList();

        public bool CalcularDisponible(int peritoId, DateTime fecha, DateTime inicio, DateTime fin)
        {
            List<icb_cita_perito> citasDelDia = context.icb_cita_perito.Where(x =>
                x.perito_id == peritoId && x.cita_fecha_inicio.Year == fecha.Year &&
                x.cita_fecha_inicio.Month == fecha.Month && x.cita_fecha_inicio.Day == fecha.Day).ToList();

            foreach (icb_cita_perito cita in citasDelDia)
            {
                if (DateTime.Compare(inicio, cita.cita_fecha_inicio) == 0)
                {
                    return false;
                }
                else if (DateTime.Compare(inicio, cita.cita_fecha_inicio) < 0 &&
                         DateTime.Compare(fin, cita.cita_fecha_inicio) > 0)
                {
                    return false;
                }
                else if (DateTime.Compare(inicio, cita.cita_fecha_inicio) > 0 &&
                         DateTime.Compare(inicio, cita.cita_fecha_fin) < 0)
                {
                    return false;
                }
            }

            // Se busca que el horario se encuentre disponible segun el horario que el ha elegido
            parametrizacion_horario buscarHorarioPerito = context.parametrizacion_horario.FirstOrDefault(x => x.usuario_id == peritoId);
            if (buscarHorarioPerito != null)
            {
                TimeSpan horaInicio = new TimeSpan(inicio.Hour, inicio.Minute, inicio.Second);
                TimeSpan horaFin = new TimeSpan(fin.Hour, fin.Minute, fin.Second);

                // Si la cita es para el dia "LUNES" se valida si el horario funciona por la mañana o por la tarde
                if (fecha.DayOfWeek == DayOfWeek.Monday)
                {
                    if (buscarHorarioPerito.lunes_desde != null && buscarHorarioPerito.lunes_hasta != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.lunes_desde ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.lunes_hasta ?? new TimeSpan()) <= 0)
                        {
                            return true;
                        }
                    }

                    if (buscarHorarioPerito.lunes_desde2 != null && buscarHorarioPerito.lunes_hasta2 != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.lunes_desde2 ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.lunes_hasta2 ?? new TimeSpan()) <= 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                // Si la cita es para el dia "MARTES" se valida si el horario funciona por la mañana o por la tarde
                if (fecha.DayOfWeek == DayOfWeek.Tuesday)
                {
                    if (buscarHorarioPerito.martes_desde != null && buscarHorarioPerito.martes_hasta != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.martes_desde ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.martes_hasta ?? new TimeSpan()) <= 0)
                        {
                            return true;
                        }
                    }

                    if (buscarHorarioPerito.martes_desde2 != null && buscarHorarioPerito.martes_hasta2 != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.martes_desde2 ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.martes_hasta2 ?? new TimeSpan()) <= 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                // Si la cita es para el dia "MIERCOLES" se valida si el horario funciona por la mañana o por la tarde
                if (fecha.DayOfWeek == DayOfWeek.Wednesday)
                {
                    if (buscarHorarioPerito.miercoles_desde != null && buscarHorarioPerito.miercoles_hasta != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.miercoles_desde ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.miercoles_hasta ?? new TimeSpan()) <= 0)
                        {
                            return true;
                        }
                    }

                    if (buscarHorarioPerito.miercoles_desde2 != null && buscarHorarioPerito.miercoles_hasta2 != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.miercoles_desde2 ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.miercoles_hasta2 ?? new TimeSpan()) <= 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                // Si la cita es para el dia "JUEVES" se valida si el horario funciona por la mañana o por la tarde
                if (fecha.DayOfWeek == DayOfWeek.Thursday)
                {
                    if (buscarHorarioPerito.jueves_desde != null && buscarHorarioPerito.jueves_hasta != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.jueves_desde ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.jueves_hasta ?? new TimeSpan()) <= 0)
                        {
                            return true;
                        }
                    }

                    if (buscarHorarioPerito.jueves_desde2 != null && buscarHorarioPerito.jueves_hasta2 != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.jueves_desde2 ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.jueves_hasta2 ?? new TimeSpan()) <= 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                // Si la cita es para el dia "VIERNES" se valida si el horario funciona por la mañana o por la tarde
                if (fecha.DayOfWeek == DayOfWeek.Friday)
                {
                    if (buscarHorarioPerito.viernes_desde != null && buscarHorarioPerito.viernes_hasta != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.viernes_desde ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.viernes_hasta ?? new TimeSpan()) <= 0)
                        {
                            return true;
                        }
                    }

                    if (buscarHorarioPerito.viernes_desde2 != null && buscarHorarioPerito.viernes_hasta2 != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.viernes_desde2 ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.viernes_hasta2 ?? new TimeSpan()) <= 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                // Si la cita es para el dia "SABADO" se valida si el horario funciona por la mañana o por la tarde
                if (fecha.DayOfWeek == DayOfWeek.Saturday)
                {
                    if (buscarHorarioPerito.sabado_desde != null && buscarHorarioPerito.sabado_hasta != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.sabado_desde ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.sabado_hasta ?? new TimeSpan()) <= 0)
                        {
                            return true;
                        }
                    }

                    if (buscarHorarioPerito.sabado_desde2 != null && buscarHorarioPerito.sabado_hasta2 != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.sabado_desde2 ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.sabado_hasta2 ?? new TimeSpan()) <= 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                // Si la cita es para el dia "DOMINGO" se valida si el horario funciona por la mañana o por la tarde
                if (fecha.DayOfWeek == DayOfWeek.Sunday)
                {
                    if (buscarHorarioPerito.domingo_desde != null && buscarHorarioPerito.domingo_hasta != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.domingo_desde ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.domingo_hasta ?? new TimeSpan()) <= 0)
                        {
                            return true;
                        }
                    }

                    if (buscarHorarioPerito.domingo_desde2 != null && buscarHorarioPerito.domingo_hasta2 != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.domingo_desde2 ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.domingo_hasta2 ?? new TimeSpan()) <= 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
            else
            {
                // Significa que el perito no tiene horario guardado en base de datos
                return false;
            }

            return false;
        }

        public JsonResult BuscarClientePorDocumento(string documento)
        {
            icb_terceros buscarCliente = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == documento);
            bool encontrado = false;
            if (buscarCliente != null)
            {
                encontrado = true;
            }

            var cliente = new
            {
                tpdoc_id = buscarCliente != null ? buscarCliente.tpdoc_id : 0,
                doc_tercero = buscarCliente != null ? buscarCliente.doc_tercero : "",
                prinom_tercero = buscarCliente != null ? buscarCliente.prinom_tercero : "",
                segnom_tercero = buscarCliente != null ? buscarCliente.segnom_tercero : "",
                apellido_tercero = buscarCliente != null ? buscarCliente.apellido_tercero : "",
                segapellido_tercero = buscarCliente != null ? buscarCliente.segapellido_tercero : "",
                genero_tercero = buscarCliente != null ? buscarCliente.genero_tercero : 0,
                telf_tercero = buscarCliente != null ? buscarCliente.telf_tercero : "",
                celular_tercero = buscarCliente != null ? buscarCliente.celular_tercero : "",
                email_tercero = buscarCliente != null ? buscarCliente.email_tercero : "",
                ciu_id = buscarCliente != null ? buscarCliente.ciu_id : 0,
                encontrado
            };
            return Json(cliente, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarModelosPorMarca(int? marcaId)
        {
            marcaId = marcaId ?? 0;
            List<modelo_vehiculo> buscarModelos = context.modelo_vehiculo.Where(x => x.mar_vh_id == marcaId).ToList();
            var modelosJson = buscarModelos.Select(x => new
            {
                x.modvh_codigo,
                x.modvh_nombre
            });
            return Json(modelosJson, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarAniosPorModelo(string modeloCodigo)
        {
            List<anio_modelo> buscarAnios = context.anio_modelo.Where(x => x.codigo_modelo == modeloCodigo).ToList();
            var aniosJson = buscarAnios.Select(x => new
            {
                x.anio,
                x.codigo_modelo
            });
            var data = new
            {
                aniosJson
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult BuscarTerceroPorDocumento(int docTercero) {
        //    var buscarTercero = (from tercero in context.icb_terceros
        //                        join cliente in context.tercero_cliente
        //                        on tercero.tercero_id equals cliente.tercero_id
        //                        join tpDoc in context.tp_documento
        //                        on tercero.tpdoc_id equals tpDoc.tpdoc_id
        //                        join ciudad in context.nom_ciudad
        //                        on tercero.ciu_id equals ciudad.ciu_id
        //                        where tercero.doc_tercero==docTercero
        //                        select new {
        //                            tercero.tercero_id,
        //                            tpDoc.tpdoc_nombre,
        //                            tercero.doc_tercero,
        //                            tercero.prinom_tercero,
        //                            tercero.segnom_tercero,
        //                            tercero.apellido_tercero,
        //                            tercero.telf_tercero,
        //                            tercero.celular_tercero,
        //                            tercero.email_tercero,
        //                            ciudad.ciu_nombre,
        //                            tercero.direc_tercero
        //                        });
        //    return Json(buscarTercero,JsonRequestBehavior.AllowGet);
        //}

        public JsonResult BuscarModelos(int marca)
        {
            List<modelo_vehiculo> marcas = context.modelo_vehiculo.Where(x => x.mar_vh_id == marca).ToList();
            var result = marcas.Select(x => new
            {
                x.modvh_codigo,
                x.modvh_nombre
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AgregarMarca(string nombreMarca)
        {
            marca_vehiculo nuevaMarca = new marca_vehiculo
            {
                marcvh_estado = true,
                marcvh_nombre = nombreMarca,
                marcvhrazoninactivo = "No aplica",
                marcvhuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                marcvhfec_creacion = DateTime.Now
            };
            context.marca_vehiculo.Add(nuevaMarca);
            bool result = context.SaveChanges() > 0;
            int lastId = -1;
            if (result)
            {
                lastId = context.marca_vehiculo.OrderByDescending(x => x.marcvh_id).First().marcvh_id;
            }

            return Json(lastId, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AgregarModelo(string nombreModelo, int marcaModelo)
        {
            modelo_vehiculo nuevoModelo = new modelo_vehiculo
            {
                modvh_codigo = Guid.NewGuid().ToString().Substring(0, 8),
                modvh_estado = true,
                modvh_nombre = nombreModelo,
                modvhrazoninactivo = "No aplica",
                modvhuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                modvhfec_creacion = DateTime.Now,
                mar_vh_id = marcaModelo
            };
            context.modelo_vehiculo.Add(nuevoModelo);
            bool result = context.SaveChanges() > 0;
            string lastId = "-1";
            if (result)
            {
                lastId = context.modelo_vehiculo.OrderByDescending(x => x.modvh_codigo).First().modvh_codigo;
            }

            return Json(lastId, JsonRequestBehavior.AllowGet);
        }
        //public JsonResult buscarPeritajePrevio(string placa, string kilometraje) {

        //    var buscarLimite = context.vvencimientoperitaje.Select(x => new { x.valorTiempo, x.valorKilometraje }).FirstOrDefault(); 

        //}

        public void BuscarFavoritos(int? menu)
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

            var buscarFavoritosSeleccionados = (from favoritos in context.favoritos
                                                join menu2 in context.Menus
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