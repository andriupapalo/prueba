using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class checkListVehiculosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        public ActionResult Create(int? menu, string idPlan, string idTraslado, string id_select)
        {
            SelectList data2 = new SelectList(context.tipo_Checklist, "id", "descripcion", id_select != null ? id_select : "");

            if (idPlan != null)
            {
                ViewBag.planMayor = idPlan;
            }
            else
            {
                ViewBag.planMayor = "0";
            }

            if (idTraslado != null)
            {
                ViewBag.idTraslado = idTraslado;
            }
            else
            {
                ViewBag.idTraslado = "0";
            }

            if (id_select != null)
            {
                ViewBag.id_select = id_select;
            }
            else
            {
                ViewBag.id_select = "";
            }

            ViewBag.checklist = data2;
            ViewBag.valueParametro = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P68").syspar_value;

            vencabingresovehiculo modelo = new vencabingresovehiculo { nuevo = true, estado = true, recepcion = true };
            //BuscarFavoritos(menu);
            return View(modelo);
        }


        [HttpPost]
        public ActionResult Create(vencabingresovehiculo modelo, int? menu)
        {
            string sysPar = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P68").syspar_value;
            string selected = Request["id_select"];

            int traslado_id = Convert.ToInt32(Request["traslado"]);

            if (traslado_id != 0)
            {
                int idEncabezado = context.recibidotraslados.FirstOrDefault(x => x.id == traslado_id).idtraslado;
                if (sysPar == selected)
                {
                    modelo.idencabtraslado = idEncabezado;
                }
            }


            int preguntas = int.Parse(Request["numeroParametros"]);
            modelo.fecha = DateTime.Now;
            modelo.fec_creacion = DateTime.Now;
            modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
            if (modelo.tercero == null || modelo.tercero == 0)
            {
                int sinDoc = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0").tercero_id;
                modelo.tercero = sinDoc;
            }

            context.vencabingresovehiculo.Add(modelo);
            int guardar = context.SaveChanges();
            if (guardar > 0)
            {
                //var ultimoEncabezado = context.vencabingresovehiculo.OrderByDescending(x => x.id).FirstOrDefault();
                int ultimoEncabezado = modelo.id;
                //if (modelo.nuevo)
                //{
                for (int i = 0; i <= preguntas; i++)
                {
                    string parametros = Request["parametros" + i];
                    string respuestas = Request["respuestas" + i];
                    respuestas = respuestas != null ? respuestas : "";
                    if (parametros != null && respuestas != null)
                    {
                        if (respuestas == "on")
                        {
                            context.vdetalleingresovehiculo.Add(new vdetalleingresovehiculo
                            {
                                idencabezado = ultimoEncabezado,
                                iditeminspeccion = Convert.ToInt32(parametros),
                                respuesta = "true"
                            });
                        }
                        else
                        {
                            context.vdetalleingresovehiculo.Add(new vdetalleingresovehiculo
                            {
                                idencabezado = ultimoEncabezado,
                                iditeminspeccion = Convert.ToInt32(parametros),
                                respuesta = respuestas
                            });
                        }
                    }
                }

                int guardarRespuestas = context.SaveChanges();

                int idTraslado = Convert.ToInt32(Request["traslado"]);


                if (idTraslado != 0)
                {
                    recibidotraslados buscar = context.recibidotraslados.FirstOrDefault(x => x.id == idTraslado);
                    if (buscar != null)
                    {
                        buscar.recibido = true;
                        buscar.fecharecibido = DateTime.Now;
                        buscar.userrecibido = Convert.ToInt32(Session["user_usuarioid"]);
                        context.Entry(buscar).State = EntityState.Modified;
                    }

                    vw_promedio buscarPromedio = context.vw_promedio.FirstOrDefault(x =>
                        x.codigo == buscar.refcodigo && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month);
                    decimal promedio = buscarPromedio != null ? buscarPromedio.Promedio ?? 0 : 0;
                    referencias_inven buscarReferenciasInvenDestino = context.referencias_inven.FirstOrDefault(x =>
                        x.codigo == buscar.refcodigo && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month &&
                        x.bodega == buscar.iddestino);
                    if (buscarReferenciasInvenDestino != null)
                    {
                        buscarReferenciasInvenDestino.can_ent = buscarReferenciasInvenDestino.can_ent + 1;
                        buscarReferenciasInvenDestino.cos_ent = promedio;
                        buscarReferenciasInvenDestino.can_tra = buscarReferenciasInvenDestino.can_tra + 1;
                        buscarReferenciasInvenDestino.cos_tra = promedio;
                        context.Entry(buscarReferenciasInvenDestino).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                    else
                    {
                        referencias_inven crearReferencia = new referencias_inven
                        {
                            bodega = Convert.ToInt32(Session["user_bodega"]),
                            codigo = buscar.refcodigo,
                            ano = (short)DateTime.Now.Year,
                            mes = (short)DateTime.Now.Month,
                            can_ini = 0,
                            can_ent = 1,
                            can_sal = 0,
                            cos_ent = promedio,
                            can_tra = 1,
                            cos_tra = promedio,
                            modulo = "V"
                        };
                        context.referencias_inven.Add(crearReferencia);
                        context.SaveChanges();
                    }
                }

                int guardarTraslado = context.SaveChanges();


                if (guardarRespuestas > 0)
                {
                    TempData["mensaje"] = "La inspeccion se ha realizado exitosamente";
                }
                else
                {
                    TempData["mensaje_error"] = "Error de conexion con la base de datos";
                }
            }

            var data = from t in context.tipo_Checklist
                       select new
                       {
                           t.id,
                           nombre = t.descripcion
                       };
            List<SelectListItem> lcheck = new List<SelectListItem>();
            foreach (var item in data)
            {
                lcheck.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }

            ViewBag.checklist = lcheck;
            ViewBag.valueParametro = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P68").syspar_value;
            //BuscarFavoritos(menu);
            return View();
        }


        public JsonResult BuscarCheckNuevos(int esNuevo)
        {
            var buscarCheck = (from checks in context.vingresovehiculo
                               where checks.tipoCheckid == esNuevo
                               select new
                               {
                                   id_descripcion = checks.id,
                                   checks.descripcion,
                                   checks.tiporespuesta,
                                   opciones = context.vingresovehiculoopcion.Where(x => x.id_ingreso == checks.id)
                                       .Select(x => new { x.id, x.descripcion }).ToList()
                               }).ToList();

            return Json(buscarCheck, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarVehiculoEnNuevos(string cedula, string placa)
        {
            // Tener en cuenta que en nuevos no hay peritaje
            if (!string.IsNullOrEmpty(cedula) && string.IsNullOrEmpty(placa))
            {
                var buscarVehiculo = (from vehiculo in context.icb_vehiculo
                                      join modelo in context.modelo_vehiculo
                                          on vehiculo.modvh_id equals modelo.modvh_codigo
                                      join color in context.color_vehiculo
                                          on vehiculo.colvh_id equals color.colvh_id
                                      join propietario in context.icb_terceros
                                          on vehiculo.propietario equals propietario.tercero_id into prop
                                      from propietario in prop.DefaultIfEmpty()
                                      where propietario.doc_tercero == cedula
                                      select new
                                      {
                                          vehiculo.plan_mayor,
                                          modelo.modvh_nombre,
                                          color.colvh_nombre,
                                          vehiculo.anio_vh,
                                          vehiculo.fecllegadaccs_vh,
                                          vehiculo.vin,
                                          vehiculo.kmgarantia,
                                          propietario.tercero_id,
                                          prinom_tercero = propietario.prinom_tercero != null ? propietario.prinom_tercero : "",
                                          segnom_tercero = propietario.segnom_tercero != null ? propietario.segnom_tercero : "",
                                          apellido_tercero = propietario.apellido_tercero != null ? propietario.apellido_tercero : "",
                                          segapellido_tercero = propietario.segapellido_tercero != null
                                              ? propietario.segapellido_tercero
                                              : ""
                                      }).ToList();
                if (buscarVehiculo.Count() == 1)
                {
                    return Json(new { encontrado = true, buscarVehiculo }, JsonRequestBehavior.AllowGet);
                }

                if (buscarVehiculo.Count() > 1)
                {
                    string vehiculosAsociados = "";
                    foreach (var item in buscarVehiculo)
                    {
                        vehiculosAsociados += item.plan_mayor + ", ";
                    }

                    return Json(
                        new
                        {
                            encontrado = false,
                            mensajeError = "El numero de cedula tiene registrado los siguientes vehiculos vehiculos (" +
                                           vehiculosAsociados + ") , verifique a cual se le dara ingreso"
                        }, JsonRequestBehavior.AllowGet);
                }

                if (buscarVehiculo.Count() == 0)
                {
                    return Json(
                        new
                        {
                            encontrado = false,
                            mensajeError = "El numero de cedula ingresado no tiene registrado vehiculos"
                        }, JsonRequestBehavior.AllowGet);
                }
            }


            if (string.IsNullOrEmpty(cedula) && !string.IsNullOrEmpty(placa))
            {
                var buscarVehiculo = (from vehiculo in context.icb_vehiculo
                                      join modelo in context.modelo_vehiculo
                                          on vehiculo.modvh_id equals modelo.modvh_codigo
                                      join color in context.color_vehiculo
                                          on vehiculo.colvh_id equals color.colvh_id
                                      join propietario in context.icb_terceros
                                          on vehiculo.propietario equals propietario.tercero_id into prop
                                      from propietario in prop.DefaultIfEmpty()
                                      where vehiculo.plan_mayor == placa
                                      select new
                                      {
                                          modelo.modvh_nombre,
                                          color.colvh_nombre,
                                          vehiculo.anio_vh,
                                          vehiculo.fecllegadaccs_vh,
                                          vehiculo.vin,
                                          vehiculo.kmgarantia,
                                          //tercero_id = propietario.tercero_id != null ? propietario.tercero_id : 0,
                                          tercero_id = propietario!= null ? propietario.tercero_id : 0,
                                          prinom_tercero = propietario.prinom_tercero != null ? propietario.prinom_tercero : "",
                                          segnom_tercero = propietario.segnom_tercero != null ? propietario.segnom_tercero : "",
                                          apellido_tercero = propietario.apellido_tercero != null ? propietario.apellido_tercero : "",
                                          segapellido_tercero = propietario.segapellido_tercero != null
                                              ? propietario.segapellido_tercero
                                              : ""
                                      }).FirstOrDefault();
                if (buscarVehiculo != null)
                {
                    return Json(new { encontrado = true, buscarVehiculo }, JsonRequestBehavior.AllowGet);
                }

                return Json(
                    new
                    {
                        encontrado = false,
                        mensajeError =
                            "El numero placa no no fue encontrado, verifique que los datos ingresados esten bien escritos"
                    }, JsonRequestBehavior.AllowGet);
            }


            if (!string.IsNullOrEmpty(cedula) && !string.IsNullOrEmpty(placa))
            {
                var buscarVehiculo = (from vehiculo in context.icb_vehiculo
                                      join modelo in context.modelo_vehiculo
                                          on vehiculo.modvh_id equals modelo.modvh_codigo
                                      join color in context.color_vehiculo
                                          on vehiculo.colvh_id equals color.colvh_id
                                      join propietario in context.icb_terceros
                                          on vehiculo.propietario equals propietario.tercero_id into prop
                                      from propietario in prop.DefaultIfEmpty()
                                      where vehiculo.plan_mayor == placa && propietario.doc_tercero == cedula
                                      select new
                                      {
                                          modelo.modvh_nombre,
                                          color.colvh_nombre,
                                          vehiculo.anio_vh,
                                          vehiculo.fecllegadaccs_vh,
                                          vehiculo.vin,
                                          vehiculo.kmgarantia,
                                          propietario.tercero_id,
                                          prinom_tercero = propietario.prinom_tercero != null ? propietario.prinom_tercero : "",
                                          segnom_tercero = propietario.segnom_tercero != null ? propietario.segnom_tercero : "",
                                          apellido_tercero = propietario.apellido_tercero != null ? propietario.apellido_tercero : "",
                                          segapellido_tercero = propietario.segapellido_tercero != null
                                              ? propietario.segapellido_tercero
                                              : ""
                                      }).FirstOrDefault();
                if (buscarVehiculo != null)
                {
                    return Json(new { encontrado = true, buscarVehiculo }, JsonRequestBehavior.AllowGet);
                }

                return Json(
                    new
                    {
                        encontrado = false,
                        mensajeError = "El numero de cedula con la placa no coinciden, verifique los datos ingresados"
                    }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { encontrado = false }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarIngresosRealizados()
        {
            var buscarIngresos = (from encabezado in context.vencabingresovehiculo
                                  join tercero in context.icb_terceros
                                      on encabezado.tercero equals tercero.tercero_id
                                  select new
                                  {
                                      encabezado.id,
                                      tercero.doc_tercero,
                                      prinom_tercero = tercero.prinom_tercero != null ? tercero.prinom_tercero : "",
                                      segnom_tercero = tercero.segnom_tercero != null ? tercero.segnom_tercero : "",
                                      apellido_tercero = tercero.apellido_tercero != null ? tercero.apellido_tercero : "",
                                      segapellido_tercero = tercero.segapellido_tercero != null ? tercero.segapellido_tercero : "",
                                      encabezado.placa,
                                      encabezado.fecha
                                  }).ToList();

            var ingresos = buscarIngresos.Select(x => new
            {
                x.id,
                x.doc_tercero,
                x.prinom_tercero,
                x.segnom_tercero,
                x.apellido_tercero,
                x.segapellido_tercero,
                x.placa,
                fecha = x.fecha.ToShortDateString()
            });

            return Json(ingresos, JsonRequestBehavior.AllowGet);
        }


        public ActionResult IngresoPDF(int? id)
        {
            string root = Server.MapPath("~/Pdf/");
            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
            string path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);

            var buscarIngreso = (from encabezado in context.vencabingresovehiculo
                                 join tercero in context.icb_terceros
                                     on encabezado.tercero equals tercero.tercero_id
                                 join vehiculo in context.icb_vehiculo
                                     on encabezado.placa equals vehiculo.plan_mayor
                                 join modeloVh in context.modelo_vehiculo
                                     on vehiculo.modvh_id equals modeloVh.modvh_codigo
                                 join color in context.color_vehiculo
                                     on vehiculo.colvh_id equals color.colvh_id
                                 where encabezado.id == id
                                 select new
                                 {
                                     encabezado.id,
                                     encabezado.nuevo,
                                     tercero.doc_tercero,
                                     encabezado.recepcion,
                                     encabezado.entrega,
                                     encabezado.Observacion,
                                     prinom_tercero = tercero.prinom_tercero != null ? tercero.prinom_tercero : "",
                                     segnom_tercero = tercero.segnom_tercero != null ? tercero.segnom_tercero : "",
                                     apellido_tercero = tercero.apellido_tercero != null ? tercero.apellido_tercero : "",
                                     segapellido_tercero = tercero.segapellido_tercero != null ? tercero.segapellido_tercero : "",
                                     encabezado.placa,
                                     encabezado.fecha,
                                     modeloVh.modvh_nombre,
                                     vehiculo.anio_vh,
                                     vehiculo.vin,
                                     vehiculo.plac_vh,
                                     vehiculo.kilometraje,
                                     color.colvh_nombre
                                     //peritaje = from encabezado in context.icb_encabezado_insp_peritaje
                                     //           join solicitud in context.icb_solicitud_peritaje
                                     //           on encabezado.encab_insper_idsolicitud equals solicitud.id_peritaje
                                     //           join vehiculoRetoma in 
                                 }).FirstOrDefault();

            //bool esNuevo = buscarIngreso != null ? buscarIngreso.nuevo : false;
            var buscarCheck = (from checks in context.vingresovehiculo
                               join respuestas in context.vdetalleingresovehiculo
                                   on checks.id equals respuestas.iditeminspeccion into ps
                               from respuestas in ps.DefaultIfEmpty()
                               where respuestas.idencabezado == id
                               select new
                               {
                                   id_descripcion = checks.id,
                                   checks.descripcion,
                                   checks.tiporespuesta,
                                   respuestas.respuesta,
                                   opciones = context.vingresovehiculoopcion.Where(x => x.id_ingreso == checks.id)
                                       .Select(x => new { x.id, x.descripcion }).ToList()
                               }).ToList();

            CheckingIngresoModel modelo = new CheckingIngresoModel
            {
                numeroConsecutivo = buscarIngreso != null ? buscarIngreso.id : 0,
                cedula = buscarIngreso != null ? buscarIngreso.doc_tercero : "",
                recepcion = buscarIngreso != null ? buscarIngreso.recepcion : false,
                entrega = buscarIngreso != null ? buscarIngreso.entrega : false,
                cliente = buscarIngreso != null
                ? buscarIngreso.prinom_tercero + " " + buscarIngreso.segnom_tercero + " " +
                  buscarIngreso.apellido_tercero + " " + buscarIngreso.segapellido_tercero
                : "",
                vehiculo = buscarIngreso != null ? buscarIngreso.modvh_nombre : "",
                color = buscarIngreso != null ? buscarIngreso.colvh_nombre : "",
                modelo = buscarIngreso != null ? buscarIngreso.anio_vh : 0,
                fecha = buscarIngreso != null ? buscarIngreso.fecha : DateTime.Now,
                serie = buscarIngreso != null ? buscarIngreso.vin : "",
                placa = buscarIngreso != null ? buscarIngreso.placa : "",
                observaciones = buscarIngreso != null ? buscarIngreso.Observacion : ""
            };
            //modelo.km = buscarIngreso != null ? buscarIngreso.kilometraje != null ? (int)buscarIngreso.kilometraje : 0 : 0;
            int mitad = buscarCheck.Count / 2;
            int mitadActual = 1;
            foreach (var item in buscarCheck)
            {
                if (mitadActual <= mitad)
                {
                    modelo.preguntas1.Add(new PreguntasIngresoModel
                    {
                        descripcion = item.descripcion,
                        tiporespuesta = item.tiporespuesta,
                        respuesta = item.respuesta
                    });
                    mitadActual++;
                }
                else
                {
                    modelo.preguntas2.Add(new PreguntasIngresoModel
                    {
                        descripcion = item.descripcion,
                        tiporespuesta = item.tiporespuesta,
                        respuesta = item.respuesta
                    });
                }
            }

            ViewAsPdf something = new ViewAsPdf("IngresoPDF", modelo);

            return something;
            //return View();
        }


        //public void BuscarFavoritos(int? menu)
        //{
        //    var usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

        //    var buscarFavoritosSeleccionados = (from favoritos in context.favoritos
        //                                        join menu2 in context.Menus
        //                                        on favoritos.idmenu equals menu2.idMenu
        //                                        where favoritos.idusuario == usuarioActual && favoritos.seleccionado == true
        //                                        select new
        //                                        {
        //                                            favoritos.seleccionado,
        //                                            favoritos.cantidad,
        //                                            menu2.idMenu,
        //                                            menu2.nombreMenu,
        //                                            menu2.url
        //                                        }).OrderByDescending(x => x.cantidad).ToList();

        //    bool esFavorito = false;

        //    foreach (var favoritosSeleccionados in buscarFavoritosSeleccionados)
        //    {
        //        if (favoritosSeleccionados.idMenu == menu)
        //        {
        //            esFavorito = true;
        //            break;
        //        }
        //    }
        //    if (esFavorito)
        //    {
        //        ViewBag.Favoritos = "<div id='areaFavoritos'><i class='fa fa-close'></i>&nbsp;&nbsp;<a href='#' onclick='AgregarQuitarFavorito();return false;'>Quitar de Favoritos</a><div>";
        //    }
        //    else
        //    {
        //        ViewBag.Favoritos = "<div id='areaFavoritos'><i class='fa fa-check'></i>&nbsp;&nbsp;<a href='#' onclick='AgregarQuitarFavorito();return false;'>Agregar a Favoritos</a></div>";
        //    }
        //    ViewBag.id_menu = menu != null ? menu : 0;
        //}
    }
}