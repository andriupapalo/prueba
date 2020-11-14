using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    //laura 2260
    public class EventoVehiculoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public JsonResult MostrarDatosVehiculo(string id)
        {
            var data = (from vehiculo in context.icb_vehiculo
                        join modelovh in context.modelo_vehiculo
                            on vehiculo.modvh_id equals modelovh.modvh_codigo
                        join colorvh in context.color_vehiculo
                            on vehiculo.colvh_id equals colorvh.colvh_id
                        where vehiculo.plan_mayor == id || vehiculo.vin.EndsWith(id) || vehiculo.plac_vh == id
                        select new
                        {
                            pla = vehiculo.plac_vh,
                            cod = vehiculo.modvh_id,
                            mod = modelovh.modvh_nombre,
                            ani = vehiculo.anio_vh,
                            col = colorvh.colvh_nombre,
                            vehiculo.vin,
                            planmayor = vehiculo.plan_mayor
                        }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult validarParaEncuesta(string id_tpevento, string plan_mayor)
        {
            if (id_tpevento != null)
            {
                string[] array = id_tpevento.Split(',');
                string parametro = array[2];
                if (parametro == "P30")
                {
                    int buscarValue = Convert.ToInt32(context.icb_sysparameter
                        .FirstOrDefault(x => x.syspar_cod == "P69").syspar_value);
                    int? tercero = context.icb_vehiculo
                        .FirstOrDefault(x => x.plan_mayor == plan_mayor || x.vin == plan_mayor).propietario;

                    crm_encuesta_respuestas buscar = context.crm_encuesta_respuestas.FirstOrDefault(x =>
                        x.id_encuesta == buscarValue && x.tercero == tercero && x.plan_mayor == plan_mayor);
                    if (buscar != null)
                    {
                        //1= Ya se realizó la encuesta
                        return Json(1, JsonRequestBehavior.AllowGet);
                    }

                    //return Json(2, JsonRequestBehavior.AllowGet);
                    //PROVISIONAL MIENTRAS SE CONFIGURA LA ENCUESTA
                    return Json(1, JsonRequestBehavior.AllowGet);

                }
            }

            //3 = Equivale a otro parametro 
            return Json(3, JsonRequestBehavior.AllowGet);
        }

        public JsonResult encuestaExistente(string id_tpevento, string plan_mayor)
        {
            if (id_tpevento != null)
            {
                string[] array = id_tpevento.Split(',');
                string parametro = array[2];
                if (parametro == "P30")
                {
                    int buscarValue = Convert.ToInt32(context.icb_sysparameter
                        .FirstOrDefault(x => x.syspar_cod == "P69").syspar_value);
                    int? tercero = context.icb_vehiculo.FirstOrDefault(x => x.plan_mayor == plan_mayor).propietario;

                    crm_encuesta_respuestas buscar = context.crm_encuesta_respuestas.FirstOrDefault(x =>
                        x.id_encuesta == buscarValue && x.tercero == tercero && x.plan_mayor == plan_mayor);
                    if (buscar != null)
                    {
                        //1= Ya se realizó la encuesta
                        return Json(1, JsonRequestBehavior.AllowGet);
                    }

                    return Json(2, JsonRequestBehavior.AllowGet);
                }
            }

            //3 = Equivale a otro parametro 
            return Json(3, JsonRequestBehavior.AllowGet);
        }

        public JsonResult traerSeries(string serie /*cadena de la referencia a buscar*/)
        {
            var series = (from r in context.icb_vehiculo
                          where r.vin.Contains(serie) || r.modelo_vehiculo.modvh_nombre.Contains(serie)
                          select new
                          {
                              series = r.vin + " | " + r.modelo_vehiculo.modvh_nombre
                          }).ToList();
            List<string> series_data = series.Select(d => d.series).ToList();

            return Json(series_data);
        }

        //se hace una función para traer la placa aquellos vehículos que estan matriculados
        public JsonResult traerPlaca(string placa)
        {
            var placas = (from p in context.icb_vehiculo
                          where p.plac_vh.Contains(placa)
                          select new
                          {
                              placa = p.plac_vh
                          }).FirstOrDefault();
            return Json(placas);
        }

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

        #region Seccion para crear y modificar eventos agregados a un vehiculo

        public ActionResult Registrar(int? menu, string planmayor)
        {
            if (planmayor != null || planmayor != "")
            {
                icb_vehiculo_eventosModel model = new icb_vehiculo_eventosModel
                {
                    planmayor = planmayor
                };
                ViewBag.planmayor = planmayor;
                CamposListasDesplegables();
                BuscarFavoritos(menu);
                return View(model);
            }

            CamposListasDesplegables();
            BuscarFavoritos(menu);
            return View();
        }

        public void CamposListasDesplegables()
        {
            int bodega = Convert.ToInt32(Session["user_bodega"]);

            List<vw_eventosParametros> eventos2 = context.vw_eventosParametros.ToList();
            var eventos = (from v in eventos2
                           select new
                           {
                               value = v.tpevento_id + "," + v.codigoevento + "," +
                                       (!string.IsNullOrWhiteSpace(v.syspar_cod) ? v.syspar_cod : "0"),
                               text = v.tpevento_nombre
                           }).ToList();
            ViewBag.id_tpevento = new SelectList(eventos, "value", "text");
            ViewBag.ave_id = new SelectList(context.icb_averias.Where(x => x.ave_estado), "ave_id", "ave_descripcion");
            ViewBag.grave_id = new SelectList(context.icb_gravedad_averias.Where(x => x.grave_estado), "grave_id", "grave_descripcion");
            ViewBag.taller_averia_id = new SelectList(context.tallerAveria.Where(x => x.estado == true && !(x.nombre.Contains("solucion"))), "id", "nombre");
            ViewBag.comb_id = new SelectList(context.icb_combustible_averia.Where(x => x.comb_estado), "comb_id", "comb_descripcion");
            ViewBag.medida_id = new SelectList(context.icb_medida_averia.Where(x => x.medida_estado), "medida_id", "medida_descripcion");
            ViewBag.zona_id = new SelectList(context.icb_zona_averia.Where(x => x.zona_estado), "zona_id", "zona_descripcion");
            ViewBag.impacto_id = new SelectList(context.icb_impacto_averia.Where(x => x.impacto_estado), "impacto_id", "impacto_descripcion");
            ViewBag.eventoEntrega = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P30").syspar_value;

            var tramitador = (from t in context.tramitador_vh
                              select new
                              {
                                  value = t.tramitador_documento,
                                  text = "(" + t.tramitador_documento + ") " + t.tramitadorpri_nombre + " " + t.tramitadorseg_nombre +
                                         " " + t.tramitador_apellidos + " " + t.tramitador_apellido2
                              }).ToList();
            ViewBag.nit = new SelectList(tramitador, "value", "text");

            var tecnico = (from t in context.ttecnicos
                           join u in context.users
                               on t.idusuario equals u.user_id
                           select new
                           {
                               value = t.id,
                               text = "(" + u.user_numIdent + ") " + u.user_nombre + " " + u.user_apellido
                           }).ToList();
            ViewBag.idtecnico = new SelectList(tecnico, "value", "text");
            ViewBag.ubicacion =
                new SelectList(
                    context.ubicacion_bodega.Where(x => x.estado && x.tipo == 2 && x.idbodega == bodega)
                        .OrderBy(x => x.descripcion), "id", "descripcion");
        }
        //Validar evento anterior al guardar averias
        public JsonResult ValidarEventoAnterior(string vin)
        {

            bool eventoAnteriorBien = true;
            string nombreEventoAnterior = "";
            icb_tpeventos buscarEvento = context.icb_tpeventos.FirstOrDefault(x => x.tpevento_nombre.Contains("averia"));
            if (buscarEvento.evento_anterior != null)
            {
                icb_vehiculo_eventos buscarEventoAnterior = context.icb_vehiculo_eventos.FirstOrDefault(x => x.planmayor == vin && x.id_tpevento == buscarEvento.evento_anterior);
                if (buscarEventoAnterior == null)
                {
                    icb_tpeventos eventoRequerido =
                        context.icb_tpeventos.FirstOrDefault(x =>
                            x.tpevento_id == buscarEvento.evento_anterior);
                    nombreEventoAnterior = eventoRequerido.tpevento_nombre;
                    eventoAnteriorBien = false;
                }
            }
            var data = new
            {
                resultado = 0,
                mensaje = ""
            };
            if (eventoAnteriorBien)
            {
                data = new
                {
                    resultado = 1,
                    mensaje = "Correcto"
                };
            }
            else
            {
                data = new
                {
                    resultado = 0,
                    mensaje = "El evento debe pasar primero por el evento " + nombreEventoAnterior
                };
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Registrar(icb_vehiculo_eventosModel modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                string[] valores = modelo.id_tpevento.Split(',');
                int eventoID = Convert.ToInt32(valores[0]);
                int eventoCodigo = Convert.ToInt32(valores[1]);

                string buscarParametro = context.icb_sysparameter.Where(x => x.syspar_cod == "P55")
                    .Select(x => x.syspar_value).FirstOrDefault();
                string buscarParametro2 = context.icb_sysparameter.Where(x => x.syspar_cod == "P15")
                    .Select(x => x.syspar_value).FirstOrDefault();
                string buscarParametro3 = context.icb_sysparameter.Where(x => x.syspar_cod == "P14")
                    .Select(x => x.syspar_value).FirstOrDefault();
                string buscarParametro4 = context.icb_sysparameter.Where(x => x.syspar_cod == "P8")
                    .Select(x => x.syspar_value).FirstOrDefault();
                string buscarParametro5 = context.icb_sysparameter.Where(x => x.syspar_cod == "P30")
                    .Select(x => x.syspar_value).FirstOrDefault();
                string buscarParametro6 = context.icb_sysparameter.Where(x => x.syspar_cod == "P94")
                    .Select(x => x.syspar_value).FirstOrDefault();
                int parametroCodigo = Convert.ToInt32(buscarParametro);
                int buscarParametroRimpronta = Convert.ToInt32(buscarParametro2);
                int buscarParametroEimpronta = Convert.ToInt32(buscarParametro3);
                int buscarParametroRBodega = Convert.ToInt32(buscarParametro4);
                int buscarParametroEntrega = Convert.ToInt32(buscarParametro5);
                icb_tpeventos buscarEvento = context.icb_tpeventos.FirstOrDefault(x => x.tpevento_id == eventoID);
                if (buscarEvento != null)
                {
                    icb_vehiculo_eventos buscarEventoRealizado = context.icb_vehiculo_eventos.FirstOrDefault(x =>
                        (x.planmayor == modelo.planmayor || x.vin == modelo.planmayor) && x.id_tpevento == eventoID);
                    if (buscarEventoRealizado != null)
                    {
                        TempData["mensaje_error"] = "El evento ya se encuentra registrado";
                        CamposListasDesplegables();
                        BuscarFavoritos(menu);
                        return View(modelo);
                    }

                    bool eventoAnteriorBien = true;
                    string nombreEventoAnterior = "";
                    if (buscarEvento.evento_anterior != null)
                    {
                        icb_vehiculo_eventos buscarEventoAnterior = context.icb_vehiculo_eventos.FirstOrDefault(x =>
                            (x.planmayor == modelo.planmayor || x.vin == modelo.planmayor) &&
                            x.id_tpevento == buscarEvento.evento_anterior);
                        if (buscarEventoAnterior == null)
                        {
                            icb_tpeventos eventoRequerido =
                                context.icb_tpeventos.FirstOrDefault(x =>
                                    x.tpevento_id == buscarEvento.evento_anterior);
                            nombreEventoAnterior = eventoRequerido.tpevento_nombre;
                            eventoAnteriorBien = false;
                        }
                    }

                    if (eventoAnteriorBien)
                    {
                        icb_terceros buscarTercero = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == modelo.nit);

                        // Si llega aqui significa que los datos estan completos

                        string planMayor = context.icb_vehiculo
                            .FirstOrDefault(x => x.plan_mayor == modelo.planmayor || x.vin == modelo.planmayor)
                            .plan_mayor;


                        icb_vehiculo_eventos crearEvento = new icb_vehiculo_eventos
                        {
                            eventofec_creacion = DateTime.Now,
                            eventouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            evento_nombre = buscarEvento.tpevento_nombre,
                            evento_estado = true,
                            bodega_id = Convert.ToInt32(Session["user_bodega"]),
                            planmayor = planMayor,
                            id_tpevento = eventoID,
                            fechaevento = modelo.fechaevento,
                            placa = modelo.placa,
                            ubicacion = modelo.ubicacion,
                            idtecnico = modelo.idtecnico,
                            poliza = modelo.poliza != null && modelo.poliza != "" ? modelo.poliza : null
                        };

                        if (parametroCodigo == buscarEvento.codigoevento)
                        {
                            icb_vehiculo vehiculo = context.icb_vehiculo.FirstOrDefault(x =>
                                x.plan_mayor == modelo.planmayor || x.vin == modelo.planmayor);
                            vehiculo.fecmatricula = modelo.fechaevento;
                            context.Entry(vehiculo).State = EntityState.Modified;
                        }

                        if (buscarParametroRimpronta == buscarEvento.codigoevento)
                        {
                            icb_vehiculo vehiculo = context.icb_vehiculo.FirstOrDefault(x =>
                                x.plan_mayor == modelo.planmayor || x.vin == modelo.planmayor);
                            vehiculo.icbvh_fec_envioimpronta = modelo.fechaevento;
                            context.Entry(vehiculo).State = EntityState.Modified;
                        }

                        if (buscarParametroEimpronta == buscarEvento.codigoevento)
                        {
                            icb_vehiculo vehiculo = context.icb_vehiculo.FirstOrDefault(x =>
                                x.plan_mayor == modelo.planmayor || x.vin == modelo.planmayor);
                            vehiculo.icbvh_fec_recepcionimpronta = modelo.fechaevento;
                            context.Entry(vehiculo).State = EntityState.Modified;
                        }

                        if (buscarParametroRBodega == buscarEvento.codigoevento)
                        {
                            icb_vehiculo vehiculo = context.icb_vehiculo.FirstOrDefault(x =>
                                x.plan_mayor == modelo.planmayor || x.vin == modelo.planmayor);
                            vehiculo.fecllegadaccs_vh = modelo.fechaevento;
                            context.Entry(vehiculo).State = EntityState.Modified;
                        }

                        if (buscarParametroEntrega == buscarEvento.codigoevento)
                        {
                            icb_vehiculo vehiculo = context.icb_vehiculo.FirstOrDefault(x =>
                                x.plan_mayor == modelo.planmayor || x.vin == modelo.planmayor);
                            vehiculo.fecha_entrega = modelo.fechaevento;
                            context.Entry(vehiculo).State = EntityState.Modified;
                        }

                        icb_vehiculo buscarVh = context.icb_vehiculo
                            .Where(x => modelo.planmayor == x.plan_mayor || x.vin == modelo.planmayor).FirstOrDefault();
                        buscarVh.ubicacionactual = modelo.ubicacion;
                        context.Entry(buscarVh).State = EntityState.Modified;

                        // para registrar los eventos de programacion entrega
                        //syparemter P35
                        string parametro = valores[2];
                        if (parametro == "P35")
                        {
                            icb_sysparameter parametros = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == parametro);
                            int? propietario_vh = context.icb_vehiculo.Find(modelo.planmayor).propietario;
                            string documentoPropietario = context.icb_terceros
                                .FirstOrDefault(x => x.tercero_id == propietario_vh).doc_tercero;

                            if (eventoCodigo == Convert.ToInt32(parametros.syspar_value))
                            {
                                crearEvento.terceroid = propietario_vh;
                            }
                            //Registro la cita al asesor

                            agenda_asesor crearCita = new agenda_asesor
                            {
                                desde = modelo.fechaevento,
                                hasta = modelo.fechaevento.AddMinutes(30),
                                estado = "Activa"
                            };
                            string asesor = Request["pedidoVendedor"];
                            if (asesor != null)
                            {
                                crearCita.asesor_id = Convert.ToInt32(Request["pedidoVendedor"]);
                            }

                            crearCita.descripcion =
                                "Cita asignada por el back office, programación de entrega de vehículo";
                            crearCita.motivo = modelo.evento_observacion;
                            crearCita.cliente = documentoPropietario;
                            crearCita.placa = modelo.planmayor;
                            crearCita.tipoorigen = 4;
                            crearCita.fec_creacion = DateTime.Now;
                            crearCita.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                            context.agenda_asesor.Add(crearCita);
                        }
                        else //para los demas eventos
                        {
                            if (buscarTercero != null)
                            {
                                crearEvento.terceroid = buscarTercero.tercero_id;
                            }
                            else
                            {
                                crearEvento.terceroid = null;
                            }
                        }

                        crearEvento.evento_observacion = modelo.evento_observacion;
                        context.icb_vehiculo_eventos.Add(crearEvento);
                        int guardar = 0;
                        try
                        {
                            guardar = context.SaveChanges();
                        }
                        catch (DbEntityValidationException e)
                        {
                            foreach (DbEntityValidationResult eve in e.EntityValidationErrors)
                            {
                                Console.WriteLine(
                                    "Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                    eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                foreach (DbValidationError ve in eve.ValidationErrors)
                                {
                                    Console.WriteLine("- Property: \"{0}\", Value: \"{1}\", Error: \"{2}\"",
                                        ve.PropertyName,
                                        eve.Entry.CurrentValues.GetValue<object>(ve.PropertyName),
                                        ve.ErrorMessage);
                                }
                            }
                        }

                        if (guardar > 0)
                        {
                            // Se actualiza el campo placa en la tabla vehiculo, el campo placa quedaria insertado en las dos tablas (icv_vehiculo eventos y icb_vehiculo)
                            icb_vehiculo buscarVehiculo = context.icb_vehiculo.FirstOrDefault(x =>
                                x.plan_mayor == modelo.planmayor || x.vin == modelo.planmayor);
                            if (buscarVehiculo != null && !string.IsNullOrEmpty(modelo.placa))
                            {
                                buscarVehiculo.plac_vh = modelo.placa;
                                context.Entry(buscarVehiculo).State = EntityState.Modified;
                                context.SaveChanges();
                            }

                            TempData["mensaje"] = "El evento fue registrado exitosamente";
                            CamposListasDesplegables();
                            BuscarFavoritos(menu);
                            return View(modelo);
                        }
                    }
                    else
                    {
                        TempData["mensaje_error"] =
                            "El evento debe pasar primero por el evento " + nombreEventoAnterior;
                        CamposListasDesplegables();
                        BuscarFavoritos(menu);
                        return View(modelo);
                    }
                }
            }

            CamposListasDesplegables();
            BuscarFavoritos(menu);
            return View(modelo);
        }

        public JsonResult EventosPorVehiculo(string planMayor)
        {
            var buscaEventos = (from evento in context.icb_vehiculo_eventos
                                join tpEvento in context.icb_tpeventos
                                    on evento.id_tpevento equals tpEvento.tpevento_id
                                join usuario in context.users
                                    on evento.eventouserid_creacion equals usuario.user_id
                                where evento.planmayor == planMayor
                                select new
                                {
                                    tpEvento.codigoevento,
                                    tpEvento.tpevento_nombre,
                                    evento.fechaevento,
                                    responsable = usuario.user_nombre + " " + usuario.user_apellido
                                }).ToList();
            var data = buscaEventos.Select(x => new
            {
                x.tpevento_nombre,
                x.codigoevento,
                fechaevento = x.fechaevento.ToShortDateString(),
                x.responsable
            }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BrowserAnfitriona(int? id, int? menu)
        {
            //syparemter P36
            icb_sysparameter parametro = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P36");
            if (parametro != null)
            {
                int valor = Convert.ToInt32(parametro.syspar_value);
                if (id != null)
                {
                    vpedido buscarPed = context.vpedido.Find(id);
                    vw_pendientesEntrega buscarDatos = context.vw_pendientesEntrega.Where(x => x.id == buscarPed.id).FirstOrDefault();
                    icb_terceros consultaTercero = context.icb_terceros.Where(x => x.tercero_id == buscarDatos.idCliente)
                        .FirstOrDefault();
                    if (buscarDatos != null)
                    {
                 

                        icb_vehiculo_eventos crearEvento = new icb_vehiculo_eventos
                        {
                            eventofec_creacion = DateTime.Now,
                            eventouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            evento_nombre = "Llegada Cliente",
                            evento_estado = true,
                            bodega_id = Convert.ToInt32(Session["user_bodega"]),
                            planmayor = buscarDatos.planmayor,
                            id_tpevento = valor,
                            fechaevento = DateTime.Now,
                            terceroid = buscarDatos.idCliente,
                            evento_observacion = "Cliente llega al consecionario para entrega del vehículo",
                            placa = buscarDatos.plac_vh
                        };
                        context.icb_vehiculo_eventos.Add(crearEvento);

                        notificacion_llegadacliente crearNotificacion = new notificacion_llegadacliente
                        {
                            asesor_id = buscarDatos.idAsesor.Value,
                            cliente_id = buscarDatos.idCliente.Value,
                            leido = false,
                            descripcion =
                            "Notificación de llegada del cliente para la entrega del vehículo.",
                            planmayor = buscarDatos.planmayor
                        };
                        context.notificacion_llegadacliente.Add(crearNotificacion);

                        context.SaveChanges();

                        string msmEntregado = "";
                        try
                        {
                            users buscaAsesor = context.users.FirstOrDefault(x => x.user_id == buscarDatos.idAsesor);
                            string mensajeAlAsesor = "El cliente " + consultaTercero.prinom_tercero + " " +
                                                  consultaTercero.segnom_tercero + " "
                                                  + consultaTercero.apellido_tercero + " " +
                                                  consultaTercero.segapellido_tercero +
                                                  " ha llegado para la entrega del vehículo.";
                            string telefonoEnviarMsm = buscaAsesor.user_telefono;

                            MensajesDeTexto enviar2 = new MensajesDeTexto();
                            string status = enviar2.enviarmensaje(telefonoEnviarMsm, mensajeAlAsesor);

                            if (status != "OK")
                            {
                                msmEntregado = ", no se ha enviado notificacion en sms, telefono no disponible";
                            }
                            else
                            {
                                msmEntregado = ", mensaje de texto enviado correctamente al asesor";
                            }

                            TempData["mensaje"] = "Llegada del cliente registrada correctamente" + msmEntregado;
                        }
                        catch (Exception)
                        {
                            TempData["mensaje"] =
                                "Llegada del cliente registrada correctamente pero el mesaje de texto al asesor no ha salido, verfique el servicio de mensajeria... ";
                            BuscarFavoritos(menu);
                        }
                    }
                }
            }

            BuscarFavoritos(menu);

            return View();
        }


        //registro eventos exportación vehiculo 
        public ActionResult EventExportVeh(string[] vehiculos)
        {
            string vehiculos2 = vehiculos[0];
            List<string> vehiculos3 = vehiculos2.Split(',').ToList();
            vehiculos3 = vehiculos3.Distinct().ToList();
            int cantVehiculos = vehiculos3 != null ? vehiculos3.Count : 0;

            //syparemter P133
            List<string> listavehiculos = new List<string>();
            icb_sysparameter parametro = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P133");
            if (parametro != null && !string.IsNullOrWhiteSpace(vehiculos3[0]))
            {
                int valor = Convert.ToInt32(parametro.syspar_value);
                int idevento = context.icb_tpeventos.Where(x => x.codigoevento == valor).FirstOrDefault().tpevento_id;
                string pm = "";
                for (int i = 0; i < cantVehiculos; i++)
                {
                    pm = vehiculos3[i] != null ? vehiculos3[i] : "";
                    if (pm != null || pm != "")
                    {
                        icb_vehiculo_eventos buscarVehiculo =
                            context.icb_vehiculo_eventos.FirstOrDefault(x => x.planmayor == pm || x.vin == pm);
                        if (buscarVehiculo != null)
                        {
                            icb_vehiculo_eventos crearEvento = new icb_vehiculo_eventos
                            {
                                eventofec_creacion = DateTime.Now,
                                eventouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                evento_nombre = "Exportar Vehículo",
                                evento_estado = true,
                                bodega_id = Convert.ToInt32(Session["user_bodega"]),
                                planmayor = buscarVehiculo.planmayor,
                                id_tpevento = idevento,
                                fechaevento = DateTime.Now,
                                terceroid = buscarVehiculo.terceroid,
                                evento_observacion = "Exportación del vehículo"
                            };
                            context.icb_vehiculo_eventos.Add(crearEvento);
                            context.SaveChanges();
                            //lo añadi a la lista de planes mayores a los que les cree el evento
                            listavehiculos.Add(pm);
                        }
                    }
                }
                //listo los vehiculos que se les creó el evento
                List<vw_cargacrmgm> listadoexcel = context.vw_cargacrmgm.Where(d => 1 == 1 && listavehiculos.Contains(d.planmayor))
                    .ToList();
                var datosexcel = listadoexcel.Select(d => new
                {
                    concesionario = d.concesionario != null ? d.concesionario : "",
                    puntoventa = d.puntoventa != null ? d.puntoventa : "",
                    serie = d.serie != null ? d.serie : "",
                    chevystar = d.chevystar != null ? d.chevystar : "",
                    nit_flota = d.nit_flota != null ? d.nit_flota : "",
                    nomflota = d.nomflota != null ? d.nomflota : "",
                    dirflota = d.dirflota != null ? d.dirflota : "",
                    ciuflota = d.ciuflota != null ? d.ciuflota : "",
                    depflota = d.depflota != null ? d.depflota : "",
                    paisflota = d.paisflota != null ? d.paisflota : "",
                    apartflota = d.apartflota != null ? d.apartflota : "",
                    telflota = d.telflota != null ? d.telflota : "",
                    nitleasing = d.nitleasing != null ? d.nitleasing : "",
                    nomleasing = d.nomleasing != null ? d.nomleasing : "",
                    dirleasing = d.dirleasing != null ? d.dirleasing : "",
                    ciuleasinf = d.ciuleasing != null ? d.ciuleasing : "",
                    depleasing = d.depleasing != null ? d.depleasing : "",
                    paisleasing = d.paisleasing != null ? d.paisleasing : "",
                    aparleasing = d.aparleasing != null ? d.aparleasing : "",
                    telleasing = d.telleasing != null ? d.telleasing : "",
                    nit_cliente = d.nit_cliente != null ? d.nit_cliente : "",
                    tipo = d.tipo != null ? d.tipo : "",
                    nomcliente = d.nomcliente != null ? d.nomcliente : "",
                    apellidocliente = d.apellidocliente != null ? d.apellidocliente : "",
                    gentercero_nombre = d.gentercero_nombre != null ? d.gentercero_nombre : "",
                    email_tercero = d.email_tercero != null ? d.email_tercero : "",
                    dircliente = d.dircliente != null ? d.dircliente : "",
                    ciucliente = d.ciucliente != null ? d.ciucliente : "",
                    depcliente = d.depcliente != null ? d.depcliente : "",
                    paiscliente = d.paiscliente != null ? d.paiscliente : "",
                    telcliente = d.telcliente != null ? d.telcliente : "",
                    aparcliente = d.aparcliente != null ? d.aparcliente : "",
                    celular = d.celular != null ? d.celular : "",
                    fec_nacimiento = d.fec_nacimiento != null
                        ? d.fec_nacimiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    vendedor = d.vendedor != null ? d.vendedor : 0,
                    tipoventa = d.tipoventa != null ? d.tipoventa : "",
                    formapago = d.formapago != null ? d.formapago : "",
                    d.numero,
                    fecha = d.fecha != null ? d.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                    d.valor_mercancia,
                    d.valor_total,
                    tpserv_nombre = d.tpserv_nombre != null ? d.tpserv_nombre : "",
                    d.fec_entrega /*!= null ? (Convert.ToDateTime(d.fec_entrega)).ToString("yyyy/MM/dd", new CultureInfo("en-US")) : ""*/,
                    planmayor = d.planmayor != null ? d.planmayor : "",
                    fec_alistamiento = d.fec_alistamiento != null
                        ? d.fec_alistamiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    marca_anterior = d.marca_anterior != null ? d.marca_anterior : "",
                    modeloaterior = d.modeloaterior != null ? d.modeloaterior : "",
                    anoanterior = d.anoanterior != null ? d.anoanterior : "",
                    companiaseguros = d.companiaseguros != null ? d.companiaseguros : ""
                }).ToList();
                //inicio un elemento de tipo ExcelPackage que es mi libreria de excel.
                ExcelPackage excel = new ExcelPackage();
                //Añado una hoja de calculo al archivo
                ExcelWorksheet WorkSheet = excel.Workbook.Worksheets.Add("Vehiculos");
                //ahora comienzo a añadir valores CELDA POR CELDA manualmente para encabezados y esas cosas
                WorkSheet.Cells[1, 1].Value = "ICEBERG - ERP. Informe de Vehículos Exportados";
                WorkSheet.Cells[1, 2].Value = DateTime.Now.ToString("yyyy/MM/dd", new CultureInfo("is-IS"));

                //en esta fila agrego los nombres de las columnas que voy a añadir
                WorkSheet.Cells[4, 1].Value = "Concesionario";
                WorkSheet.Cells[4, 2].Value = "Punto Venta";
                WorkSheet.Cells[4, 3].Value = "Serie";
                WorkSheet.Cells[4, 4].Value = "Chevistar";
                WorkSheet.Cells[4, 5].Value = "Nit Flota";
                WorkSheet.Cells[4, 6].Value = "Nombre Flota";
                WorkSheet.Cells[4, 7].Value = "Direccion Flota";
                WorkSheet.Cells[4, 8].Value = "Ciudad Flota";
                WorkSheet.Cells[4, 9].Value = "Departamento Flota";
                WorkSheet.Cells[4, 10].Value = "Pais Flota";
                WorkSheet.Cells[4, 11].Value = "Apart Flota";
                WorkSheet.Cells[4, 12].Value = "Telefono Flota";
                WorkSheet.Cells[4, 13].Value = "Nit Leasing";
                WorkSheet.Cells[4, 14].Value = "Nombre Leasing";
                WorkSheet.Cells[4, 15].Value = "Direccion Leasing";
                WorkSheet.Cells[4, 16].Value = "Ciudad Leasing";
                WorkSheet.Cells[4, 17].Value = "Departamento Leasing";
                WorkSheet.Cells[4, 18].Value = "Pais Leasing";
                WorkSheet.Cells[4, 19].Value = "Apart Leasing";
                WorkSheet.Cells[4, 20].Value = "Telefono Leasing";
                WorkSheet.Cells[4, 21].Value = "Nit Cliente";
                WorkSheet.Cells[4, 22].Value = "Tipo Cliente";
                WorkSheet.Cells[4, 23].Value = "Nombre Cliente";
                WorkSheet.Cells[4, 24].Value = "Apellido cliente";
                WorkSheet.Cells[4, 25].Value = "Genero";
                WorkSheet.Cells[4, 26].Value = "Email";
                WorkSheet.Cells[4, 27].Value = "Direccion";
                WorkSheet.Cells[4, 28].Value = "Ciudad";
                WorkSheet.Cells[4, 29].Value = "Departamento";
                WorkSheet.Cells[4, 30].Value = "Pais";
                WorkSheet.Cells[4, 31].Value = "Telefono";
                WorkSheet.Cells[4, 32].Value = "Apar Cliente";
                WorkSheet.Cells[4, 33].Value = "Celular";
                WorkSheet.Cells[4, 34].Value = "Fecha Nacimiento";
                WorkSheet.Cells[4, 35].Value = "Numero Identificacion";
                WorkSheet.Cells[4, 36].Value = "Tipo Venta";
                WorkSheet.Cells[4, 37].Value = "Forma Pago";
                WorkSheet.Cells[4, 38].Value = "Numero";
                WorkSheet.Cells[4, 39].Value = "Fecha";
                WorkSheet.Cells[4, 40].Value = "Valor Unitario";
                WorkSheet.Cells[4, 41].Value = "Valor Total";
                WorkSheet.Cells[4, 42].Value = "Tipo Servicio";
                WorkSheet.Cells[4, 43].Value = "Fecha Entrega";
                WorkSheet.Cells[4, 44].Value = "Plan Mayor";
                WorkSheet.Cells[4, 45].Value = "Fecha Alistamiento";
                WorkSheet.Cells[4, 46].Value = "Marca Anterior";
                WorkSheet.Cells[4, 47].Value = "Modelo Anterior";
                WorkSheet.Cells[4, 48].Value = "Año Anterior";
                WorkSheet.Cells[4, 49].Value = "Compañia Seguros";


                //ahora añado el RESULTADO de la consulta
                WorkSheet.Cells[5, 1].LoadFromCollection(datosexcel, false);
                string nombreExcel = "Expor_vehiculos_" + DateTime.Now;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment;  filename=" + nombreExcel + ".xlsx");
                    // salvamos el archivo en memoria para poderlo exportar
                    excel.SaveAs(memoryStream);
                    //lo añadimos a la respuesta
                    memoryStream.WriteTo(Response.OutputStream);
                    //Enviamos la respuesta al front
                    Response.Flush();
                    Response.End();
                }

                return View("informe");
            }

            TempData["mensaje_error"] = "No se han seleccionado vehículos";
            return RedirectToAction("Index", "cargaCRM");
        }


        public JsonResult BuscarVehiculosPorEntregar()
        {
            //syparemter P35
            icb_sysparameter parametro = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P35");
            if (parametro != null)
            {
                int valor = Convert.ToInt32(parametro.syspar_value);

                int bodega_actual = Convert.ToInt32(Session["user_bodega"]);

                //List<vw_pendientesEntrega> buscarVH = context.vw_pendientesEntrega.Where(x =>
                //        x.bodega == bodega_actual && x.FechaProgramacioEntrega != null &&
                //        x.FechaFinAlistamiento != null)
                //    .ToList();
                var predicado = PredicateBuilder.True<vw_pendientesEntrega>();
               

                    predicado = predicado.And(d => d.bodega == bodega_actual);
                predicado = predicado.And(d => d.FechaProgramacioEntrega != null);
                predicado = predicado.And(d=> d.FechaFinAlistamiento != null);



                var buscarVH = context.vw_pendientesEntrega.Where(predicado).ToList();
                //var informacion = context.notificacion_llegadacliente.Where(y => y.planmayor==).FirstOrDefault();

                var data = buscarVH.Select(x => new
                {
                    x.plac_vh,
                    fechaevento =
                        x.FechaProgramacioEntrega.Value.ToString("yyyy/MM/dd hh:mm tt", new CultureInfo("en-US")),
                    anio_modelo = x.anio_vh,
                    x.modelo,
                    x.cliente,
                    documento = x.doc_tercero,
                    x.planmayor,
                    x.id,
                    x.asesor,
                    verificarAlistamientoAcc = !string.IsNullOrWhiteSpace(x.planmayor)
                        ? verificarAlistamientoAcc(x.planmayor)
                        : "",
                        
                    citaasesor = !string.IsNullOrWhiteSpace(x.doc_tercero)
                        ? citaAsesor(x.doc_tercero)
                        : "El pedido no tiene cliente asignado",
                    notificacionLeida= !string.IsNullOrWhiteSpace(x.planmayor)
                        ? Notificacion(x.planmayor,x.idCliente,x.idAsesor)
                        : "Ya se envio notificacion a ese pedido",


                });

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(JsonRequestBehavior.AllowGet);
        }

        public string citaAsesor(string documento)
        {
            string asesor = "";
            agenda_asesor cita = context.agenda_asesor
                .Where(x => x.cliente == documento && x.tipoorigen == 4 && x.hasta >= DateTime.Now).FirstOrDefault();

            if (cita != null)
            {
                var buscarCita = (from a in context.agenda_asesor
                                  join u in context.users
                                      on a.asesor_id equals u.user_id
                                      //join c in context.notificacion_llegadacliente
                                      //on a.placa equals c.planmayor
                                  where a.cliente == documento && a.tipoorigen == 4 
                                  select new
                                  {
                                      u.user_nombre,
                                      u.user_apellido
                                  }).FirstOrDefault();
                asesor = buscarCita.user_nombre + " " + buscarCita.user_apellido;
            }
            else
            {
                asesor = "No hay cita agendada";
            }

            return asesor;
        }
        public string Notificacion(string planmayor, int? cliente, int? idasesor)
        {
            string asesor = "";

            notificacion_llegadacliente notificar = context.notificacion_llegadacliente
                .Where(x => x.planmayor == planmayor && x.cliente_id==cliente && x.asesor_id==idasesor).FirstOrDefault();

            if (notificar != null)
            {
                asesor = "Ya se envio notificacion";
            }
           

            return asesor;
        }

        public string verificarAlistamientoAcc(string planmayor)
        {
            int pedido = context.vpedido.Where(x => x.planmayor == planmayor).Select(z => z.id).FirstOrDefault();
            List<vpedrepuestos> insaccesorios = context.vpedrepuestos.Where(x => x.pedido_id == pedido).ToList();
            int instalado = 0;
            //parametro tipo de orden de taller accesorios (razon de ingreso accesorios)
            icb_sysparameter acce1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P116").FirstOrDefault();
            int accesori = acce1 != null ? Convert.ToInt32(acce1.syspar_value) : 6;

            //veo si el plan_mayor tiene orden de taller en accesorios
            tencabezaorden ordenaccesorios = context.tencabezaorden
                .Where(d => d.razoningreso == accesori && d.estado && d.placa == planmayor)
                .OrderByDescending(x => x.fecha).FirstOrDefault();


            int resultado = 0;
            if (insaccesorios.Count == 0)
            {
                resultado = 1;
            }
            else if (insaccesorios.Count > 0)
            {
                for (int i = 0; i < insaccesorios.Count; i++)
                {
                    if (insaccesorios[i].instalado == false)
                    {
                        instalado = 0;
                        break;
                    }
                    else
                    {
                        instalado = 1;
                    }
                }

                if (instalado == 0)
                {
                    resultado = 0;
                }
                else if (ordenaccesorios != null)
                {
                    if (instalado == 1 && ordenaccesorios.fecha_fin_operacion != null)
                    {
                        resultado = 1;
                    }
                }
            }

            return resultado.ToString();
        }

        public JsonResult GetEventosVehiculoJson()
        {
            var buscarEventos = (from evento in context.icb_vehiculo_eventos
                                 join vehiculo in context.icb_vehiculo
                                     on evento.planmayor equals vehiculo.plan_mayor
                                 join modelo in context.modelo_vehiculo
                                     on vehiculo.modvh_id equals modelo.modvh_codigo
                                 select new
                                 {
                                     evento.planmayor,
                                     modelo.modvh_nombre,
                                     vehiculo.anio_vh,
                                     vehiculo.vin
                                 }).Distinct().ToList();
            return Json(buscarEventos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPlanMayor(string planMayor)
        {
            icb_vehiculo buscarPlanMayor =
                context.icb_vehiculo.FirstOrDefault(x => x.plan_mayor == planMayor || x.vin == planMayor);
            if (buscarPlanMayor != null)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ValidarTipoEvento(string idEvento)
        {
            string[] valores = idEvento.Split(',');
            int eventoID = Convert.ToInt32(valores[0]);

            icb_tpeventos buscarEvento = context.icb_tpeventos.FirstOrDefault(x => x.tpevento_id == eventoID);
            if (buscarEvento != null)
            {
                var data = new
                {
                    buscarEvento.pplaca,
                    buscarEvento.ptercero,
                    buscarEvento.pubicacion,
                    buscarEvento.ptecnico
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult traerDatosPedido(string planMayor)
        {
            var data = (from p in context.vpedido
                        where p.planmayor == planMayor
                        select new
                        {
                            p.numero,
                            p.id,
                            p.vendedor
                        }).FirstOrDefault();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Seccion para crear y modificar tipo de eventos

        public ActionResult Crear(int? menu)
        {
            ViewBag.evento_anterior = new SelectList(context.icb_tpeventos.OrderBy(x => x.tpevento_nombre),
                "tpevento_id", "tpevento_nombre");
            ViewBag.iddocasociado =
                new SelectList(context.documento_facturacion.OrderBy(x => x.docfac_nombre).Where(x => x.docfac_estado),
                    "docfac_id", "docfac_nombre");

            icb_tpeventos crearEvento = new icb_tpeventos
            {
                tpevento_estado = true,
                tpeventorazoninactivo = "No aplica"
            };

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 204);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(crearEvento);
        }

        [HttpPost]
        public ActionResult Crear(icb_tpeventos modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.icb_tpeventos
                           where a.tpevento_nombre == modelo.tpevento_nombre || a.codigoevento == modelo.codigoevento
                           select a.tpevento_nombre).Count();

                if (nom == 0)
                {
                    modelo.tpeventofec_creacion = DateTime.Now;
                    modelo.tpeventouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.icb_tpeventos.Add(modelo);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro del nuevo tipo de evento de vehiculo fue exitoso!";
                    return RedirectToAction("Crear", new { menu });
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 204);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            ViewBag.evento_anterior = new SelectList(context.icb_tpeventos.OrderBy(x => x.tpevento_nombre),
                "tpevento_id", "tpevento_nombre");
            ViewBag.iddocasociado =
                new SelectList(context.documento_facturacion.OrderBy(x => x.docfac_nombre).Where(x => x.docfac_estado),
                    "docfac_id", "docfac_nombre");
            BuscarFavoritos(menu);
            return View(modelo);
        }

        public ActionResult Actualizar(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_tpeventos evento = context.icb_tpeventos.Find(id);
            if (evento == null)
            {
                return HttpNotFound();
            }

            ConsultarDatosCreacion(evento);

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 204);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            ViewBag.evento_anterior = new SelectList(context.icb_tpeventos.OrderBy(x => x.tpevento_nombre),
                "tpevento_id", "tpevento_nombre");
            ViewBag.iddocasociado =
                new SelectList(context.documento_facturacion.OrderBy(x => x.docfac_nombre).Where(x => x.docfac_estado),
                    "docfac_id", "docfac_nombre", evento.iddocasociado);
            BuscarFavoritos(menu);
            return View(evento);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Actualizar(icb_tpeventos evento, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.icb_tpeventos
                           where a.tpevento_nombre == evento.tpevento_nombre && a.codigoevento == evento.codigoevento &&
                                 a.tpevento_id == evento.tpevento_id
                           select a.tpevento_nombre).Count();

                if (nom == 1)
                {
                    evento.tpeventofec_actualizacion = DateTime.Now;
                    evento.tpeventouserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(evento).State = EntityState.Modified;
                    context.SaveChanges();
                    ConsultarDatosCreacion(evento);
                    TempData["mensaje"] = "La actualización del tipo de evento fue exitoso!";
                    ViewBag.evento_anterior = new SelectList(context.icb_tpeventos.OrderBy(x => x.tpevento_nombre),
                        "tpevento_id", "tpevento_nombre");
                    ViewBag.iddocasociado =
                        new SelectList(
                            context.documento_facturacion.OrderBy(x => x.docfac_nombre).Where(x => x.docfac_estado),
                            "docfac_id", "docfac_nombre");
                    BuscarFavoritos(menu);
                    return View(evento);
                }

                {
                    int nom2 = (from a in context.icb_tpeventos
                                where a.tpevento_nombre == evento.tpevento_nombre && a.tpevento_id != evento.tpevento_id ||
                                      a.codigoevento == evento.codigoevento && a.tpevento_id != evento.tpevento_id
                                select a.tpevento_nombre).Count();
                    if (nom2 == 0)
                    {
                        evento.tpeventofec_actualizacion = DateTime.Now;
                        evento.tpeventouserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        context.Entry(evento).State = EntityState.Modified;
                        context.SaveChanges();

                        TempData["mensaje"] = "La actualización del tipo de evento fue exitoso!";
                        ConsultarDatosCreacion(evento);
                        ViewBag.evento_anterior = new SelectList(context.icb_tpeventos.OrderBy(x => x.tpevento_nombre),
                            "tpevento_id", "tpevento_nombre");
                        ViewBag.iddocasociado =
                            new SelectList(
                                context.documento_facturacion.OrderBy(x => x.docfac_nombre).Where(x => x.docfac_estado),
                                "docfac_id", "docfac_nombre");
                        BuscarFavoritos(menu);
                        return View(evento);
                    }

                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                }
            }

            ConsultarDatosCreacion(evento);
            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 204);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            ViewBag.evento_anterior = new SelectList(context.icb_tpeventos, "tpevento_id", "tpevento_nombre");
            ViewBag.iddocasociado =
                new SelectList(context.documento_facturacion.OrderBy(x => x.docfac_nombre).Where(x => x.docfac_estado),
                    "docfac_id", "docfac_nombre");
            BuscarFavoritos(menu);
            return View(evento);
        }

        public void ConsultarDatosCreacion(icb_tpeventos evento)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(evento.tpeventouserid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(evento.tpeventouserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult GetEventosJson()
        {
            var buscarEventos = context.icb_tpeventos.ToList().Select(x => new
            {
                x.tpevento_id,
                x.tpevento_nombre,
                codigoevento = x.codigoevento != null ? x.codigoevento.ToString() : "",
                tpevento_estado = x.tpevento_estado ? "Activo" : "Inactivo"
            });
            return Json(buscarEventos, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}