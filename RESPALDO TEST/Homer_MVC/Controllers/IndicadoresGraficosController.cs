using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;

namespace Homer_MVC.Controllers
    {
    public class IndicadoresGraficosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");
        private CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

        public class listabodegas
            {
            public int id { get; set; }
            public string bodccs_nombre { get; set; }
            }
        // GET: IndicadoresGraficos

        public class ventasbodegas
            {
            public int id { get; set; }
            public string bodccs_nombre { get; set; }
            public decimal? total_ventas { get; set; }
            public int numero_ventas { get; set; }
            public decimal? participacion { get; set; }
            }

        public ActionResult indicadores()
            {
            return View();
            }

        public ActionResult graficoClasificacionABC()
            {
            return View();
            }

        public JsonResult BuscarBodegas()
            {
            var usuarioLog = Convert.ToInt32(Session["user_usuarioid"]);

            var rol = Convert.ToInt32(Session["user_rolid"]);

            var rolasesor = context.rolpermisos.Where(d => d.codigo == "P35").Select(d => d.id).FirstOrDefault();
            //veo si el rol seleccionado tiene permiso de ver todas las bodegas y todos los asesores
            var permiso = context.rolacceso.Where(d => d.idpermiso == rolasesor && d.idrol == rol).Count();
            var listarBodegas2 = (from v in context.vw_usuarios_rol
                                  where v.user_id == usuarioLog && v.es_vehiculos
                                  select new
                                      {
                                      v.id_bodega,
                                      v.bodccs_nombre
                                      }).Distinct().ToList();
            var listarBodegas = listarBodegas2.Select(d => new listabodegas
                {
                id = d.id_bodega,
                bodccs_nombre = d.bodccs_nombre
                }).ToList();
            if (permiso > 0 || rol == 1)
                {
                listarBodegas = (from v in context.bodega_concesionario
                                 where v.es_vehiculos == true && v.bodccs_estado == true
                                 select new listabodegas
                                     {
                                     id = v.id,
                                     bodccs_nombre = v.bodccs_nombre
                                     }).Distinct().ToList();
                }
            else
                {
                //veo además si tiene otras bodegas para visualizar
                var otrasbodegas = context.bodega_usuario_visualizacion.Where(d => d.id_usuario == usuarioLog).ToList();
                foreach (var item in otrasbodegas)
                    {
                    if (listarBodegas.Where(d => d.id == item.id_bodega).Count() == 0)
                        {
                        listarBodegas.Add(new listabodegas
                            {
                            id = item.bodega_concesionario.id,
                            bodccs_nombre = item.bodega_concesionario.bodccs_nombre
                            });
                        }
                    }
                }

            return Json(listarBodegas, JsonRequestBehavior.AllowGet);

            }

        public JsonResult BuscarBodegasRanking()
            {
            var data = (from a in context.bodega_concesionario
                        where a.es_vehiculos
                        select new
                            {
                            id_bodega = a.id,
                            a.bodccs_nombre
                            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
            }

        public JsonResult listarAsesorPorBodega(int [] param)
            {
            var predicate = PredicateBuilder.True<vw_usuarios_rol>();
            var bodega_predicate = PredicateBuilder.False<vw_usuarios_rol>();
            var tramite_predicate = PredicateBuilder.False<vw_usuarios_rol>();

            var usuario = Convert.ToInt32(Session["user_usuarioid"]);
            var rol = Convert.ToInt32(Session["user_rolid"]);
            //parametro de sistema rol asesor de ventas
            var asesorventas1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P29").FirstOrDefault();
            var asesorventas = asesorventas1 != null ? Convert.ToInt32(asesorventas1.syspar_value) : 4;
            //parametro de sistema rol asesor usados
            var asesorusados1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P99").FirstOrDefault();
            var asesorusados = asesorusados1 != null ? Convert.ToInt32(asesorusados1.syspar_value) : 4;
            //tipo de rol productivo, solo se ve a si mismo
            var asesorproductivo1 = context.icb_sysparameter.Where(d => d.syspar_cod == "142").FirstOrDefault();
            var asesorproductivo = asesorproductivo1 != null ? Convert.ToInt32(asesorusados1.syspar_value) : 3;
            //permiso de ver asesores de otras bodegas y otras bodegas
            var verbodegas1 = context.rolacceso.Where(d => d.rolpermisos.codigo == "P35").Count();
            //si es algun asesor


            if (rol == asesorproductivo)
                {
                predicate = predicate.And(x => x.user_id == usuario);
                }

            //veo los tipos de trámite a los que está suscrito el usuario
            var tramite = context.tipotramiteasesor.Where(x => x.id_usuario == usuario)
                   .Select(x => x.id_tipotramite).ToList();


            //listado de bodegas
            List<int> bodegas = new List<int>();
            if (param.Count() > 0 && param[0] != null && param[0] != 0)
                {
                foreach (var item in param)
                    {
                    //busco la bodega
                    var nombre = item;
                    var idbodegax = context.bodega_concesionario.Where(d => d.id==nombre).FirstOrDefault();
                    if (idbodegax != null)
                        {
                        bodegas.Add(Convert.ToInt32(idbodegax.id));
                        }

                    }
                }
            else
                {
                if (verbodegas1 > 0 || rol == 1)
                    {
                    //le muestro todas las bodegas que tiene para visualizar
                    bodegas = context.bodega_concesionario.Where(d => d.bodccs_estado == true).Select(d => d.id).ToList();

                    }
                else
                    {
                    //busco las bodegas sobre las cuales está registrado
                    bodegas = context.bodega_usuario.Where(d => d.id_usuario == usuario).Select(d => d.id_bodega).ToList();

                    //busco si tiene bodegas para visualizar
                    var bodegasvisualizar = context.bodega_usuario_visualizacion.Where(d => d.id_usuario == usuario).Select(d => d.id_bodega).ToList();
                    foreach (var item in bodegasvisualizar)
                        {
                        if (!bodegas.Contains(item))
                            {
                            bodegas.Add(item);
                            }
                        }
                    }
                }
            foreach (var item in bodegas)
                {
                bodega_predicate = bodega_predicate.Or(x =>
                                    x.id_bodega == item && tramite.Contains(x.id_tipotramite));
                }
            predicate = predicate.And(bodega_predicate);
            if (rol != 1)
                {
                /* foreach (var item in tramite)
                 {
                     tramite_predicate = tramite_predicate.Or(x =>
                                    x.id_tipotramite == item);
                 }
                 predicate = predicate.And(tramite_predicate);*/
                }

            var asesores = context.vw_usuarios_rol.Where(predicate);
            var lasesor = asesores.Select(s => new
                {
                s.user_id,
                s.nombre
                }).Distinct().ToList();
            return Json(lasesor, JsonRequestBehavior.AllowGet);
            }

        public JsonResult buscarDatos(DateTime? fechaDesde, DateTime? fechaHasta, int?[] asesor, int?[] bodega,
            bool? nuevo, bool? usado)
            {
            var predicate = PredicateBuilder.True<vw_prospecto>();
            var predicate2 = PredicateBuilder.True<vw_cotizacion>();
            var predicate3 = PredicateBuilder.True<vw_creditosSolicitados>();
            var predicate4 = PredicateBuilder.True<vw_creditosAprobados>();
            var predicate5 = PredicateBuilder.True<vw_demos>();
            var predicate6 = PredicateBuilder.True<vw_pedidos>();
            var predicate7 = PredicateBuilder.True<vw_facturas>();
            var predicate8 = PredicateBuilder.True<vw_matriculas>();

            var predicate11 = PredicateBuilder.False<vw_prospecto>();
            var predicate22 = PredicateBuilder.False<vw_cotizacion>();
            var predicate33 = PredicateBuilder.False<vw_creditosSolicitados>();
            var predicate44 = PredicateBuilder.False<vw_creditosAprobados>();
            var predicate55 = PredicateBuilder.False<vw_demos>();
            var predicate66 = PredicateBuilder.False<vw_pedidos>();
            var predicate77 = PredicateBuilder.False<vw_facturas>();
            var predicate88 = PredicateBuilder.False<vw_matriculas>();

            var asesores = PredicateBuilder.False<vw_prospecto>();
            var asesores2 = PredicateBuilder.False<vw_cotizacion>();
            var asesores3 = PredicateBuilder.False<vw_creditosSolicitados>();
            var asesores4 = PredicateBuilder.False<vw_creditosAprobados>();
            var asesores5 = PredicateBuilder.False<vw_demos>();
            var asesores6 = PredicateBuilder.False<vw_pedidos>();
            var asesores7 = PredicateBuilder.False<vw_facturas>();
            var asesores8 = PredicateBuilder.False<vw_matriculas>();


            var bodegas = PredicateBuilder.False<vw_prospecto>();
            var bodegas2 = PredicateBuilder.False<vw_cotizacion>();
            var bodegas3 = PredicateBuilder.False<vw_creditosSolicitados>();
            var bodegas4 = PredicateBuilder.False<vw_creditosAprobados>();
            var bodegas5 = PredicateBuilder.False<vw_demos>();
            var bodegas6 = PredicateBuilder.False<vw_pedidos>();
            var bodegas7 = PredicateBuilder.False<vw_facturas>();
            var bodegas8 = PredicateBuilder.False<vw_matriculas>();


            var usuarioLog = Convert.ToInt32(Session["user_usuarioid"]);

            var rol = (from r in context.users
                       where r.user_id == usuarioLog
                       select new
                           {
                           r.rol_id
                           }).FirstOrDefault();
            var rolasesor = context.rolpermisos.Where(d => d.codigo == "P35").Select(d => d.id).FirstOrDefault();
            var permiso = context.rolacceso.Where(d => d.idpermiso == rolasesor && d.idrol == rol.rol_id).Count();

            //parametro de sistema rol asesor de ventas
            var asesorventas1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P29").FirstOrDefault();
            var asesorventas = asesorventas1 != null ? Convert.ToInt32(asesorventas1.syspar_value) : 4;
            //parametro de sistema rol asesor usados
            var asesorusados1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P99").FirstOrDefault();
            var asesorusados = asesorusados1 != null ? Convert.ToInt32(asesorusados1.syspar_value) : 2030;

            var listarBodegas2 = (from v in context.vw_usuarios_rol
                                  where v.user_id == usuarioLog && v.es_vehiculos
                                  select new listabodegas
                                      {
                                      id = v.id_bodega,
                                      bodccs_nombre = v.bodccs_nombre
                                      }).Distinct().ToList();
            if (permiso > 0 || rol.rol_id == 1)
                {
                listarBodegas2 = (from v in context.bodega_concesionario
                                  where v.es_vehiculos == true && v.bodccs_estado == true
                                  select new listabodegas
                                      {
                                      id = v.id,
                                      bodccs_nombre = v.bodccs_nombre
                                      }).Distinct().ToList();
                }
            else
                {
                var bodegasvisualizar = context.bodega_usuario_visualizacion.Where(d => d.id_usuario == usuarioLog).ToList();
                foreach (var item in bodegasvisualizar)
                    {
                    if (listarBodegas2.Where(d => d.id == item.id_bodega).Count() == 0)
                        {
                        listarBodegas2.Add(new listabodegas
                            {
                            id = item.bodega_concesionario.id,
                            bodccs_nombre = item.bodega_concesionario.bodccs_nombre
                            });
                        }
                    }
                }
            var listarBodegas = (from v in listarBodegas2
                                 select new
                                     {
                                     id_bodega = v.id
                                     }).ToList();

            if (fechaDesde > fechaHasta)
                {
                var mensaje = "No hay información en los filtros seleccionados";
                return Json(new { info = false, mensaje }, JsonRequestBehavior.AllowGet);
                }

            if (fechaDesde != null)
                {
                predicate = predicate.And(x => x.fechaInicial >= fechaDesde);
                predicate2 = predicate2.And(x => x.cot_feccreacion >= fechaDesde);
                predicate3 = predicate3.And(x => x.fec_solicitud >= fechaDesde);
                predicate4 = predicate4.And(x => x.fec_aprobacion >= fechaDesde);
                predicate5 = predicate5.And(x => x.desde >= fechaDesde);
                predicate6 = predicate6.And(x => x.fecha >= fechaDesde);
                predicate7 = predicate7.And(x => x.fecha >= fechaDesde);
                predicate8 = predicate8.And(x => x.fechaevento >= fechaDesde);
                }
            if (fechaHasta != null)
                {
                fechaHasta = fechaHasta.Value.AddDays(1);
                predicate = predicate.And(x => x.fechaInicial <= fechaHasta);
                predicate2 = predicate2.And(x => x.cot_feccreacion <= fechaHasta);
                predicate3 = predicate3.And(x => x.fec_solicitud <= fechaHasta);
                predicate4 = predicate4.And(x => x.fec_aprobacion <= fechaHasta);
                predicate5 = predicate5.And(x => x.desde <= fechaHasta);
                predicate6 = predicate6.And(x => x.fecha <= fechaHasta);
                predicate7 = predicate7.And(x => x.fecha <= fechaHasta);
                predicate8 = predicate8.And(x => x.fechaevento <= fechaHasta);
                }
            //if (fechaDesde != null && fechaHasta != null && fechaDesde <= fechaHasta && nuevo == true)
            //{
            //    fechaHasta = fechaHasta.Value.AddDays(1);
            //    var fechaH = fechaHasta.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"));
            //    predicate = predicate.And(x =>
            //        x.fechaInicial >= fechaDesde && x.fechaInicial <= fechaHasta && x.tptrapros_id == 1);
            //    predicate2 = predicate2.And(x =>
            //        x.cot_feccreacion >= fechaDesde && x.cot_feccreacion <= fechaHasta && x.nuevo == true);
            //    predicate3 = predicate3.And(x =>
            //        x.fec_solicitud >= fechaDesde && x.fec_solicitud <= fechaHasta && x.nuevo);
            //    predicate4 = predicate4.And(x =>
            //        x.fec_aprobacion >= fechaDesde && x.fec_aprobacion <= fechaHasta && x.nuevo);
            //    predicate5 = predicate5.And(x => x.desde >= fechaDesde && x.hasta <= fechaHasta && x.nuevo == true);
            //    predicate6 = predicate6.And(x => x.fecha >= fechaDesde && x.fecha <= fechaHasta && x.nuevo);
            //    predicate7 = predicate7.And(x => x.fecha >= fechaDesde && x.fecha <= fechaHasta && x.nuevo == true);
            //    predicate8 = predicate8.And(x =>
            //        x.fechaevento >= fechaDesde && x.fechaevento <= fechaHasta && x.nuevo == true);
            //}

            if (nuevo == true)
                {
                predicate11 = predicate11.Or(x => x.tptrapros_id == 1);
                predicate22 = predicate22.Or(x => x.nuevo == true);
                predicate33 = predicate33.Or(x => x.nuevo == true);
                predicate44 = predicate44.Or(x => x.nuevo == true);
                predicate55 = predicate55.Or(x => x.nuevo == true);
                predicate66 = predicate66.Or(x => x.nuevo == true);
                predicate77 = predicate77.Or(x => x.nuevo == true);
                predicate88 = predicate88.Or(x => x.nuevo == true);
                }

            if (usado == true)
                {
                predicate11 = predicate11.Or(x => x.tptrapros_id == 2);
                predicate22 = predicate22.Or(x => x.usado == true);
                predicate33 = predicate33.Or(x => x.usado == true);
                predicate44 = predicate44.Or(x => x.usado == true);
                predicate55 = predicate55.Or(x => x.usado == true);
                predicate66 = predicate66.Or(x => x.usado == true);
                predicate77 = predicate77.Or(x => x.usado == true);
                predicate88 = predicate88.Or(x => x.usado == true);
                }

            //if (fechaDesde != null && fechaHasta != null && fechaDesde <= fechaHasta && usado == true)
            //{
            //    fechaHasta = fechaHasta.Value.AddDays(1);
            //    predicate11 = predicate11.Or(x =>
            //        x.fechaInicial >= fechaDesde && x.fechaInicial <= fechaHasta && x.tptrapros_id == 2);
            //    predicate22 = predicate22.Or(x =>
            //        x.cot_feccreacion >= fechaDesde && x.cot_feccreacion <= fechaHasta && x.usado == true);
            //    predicate33 = predicate33.Or(x =>
            //        x.fec_solicitud >= fechaDesde && x.fec_solicitud <= fechaHasta && x.usado == true);
            //    predicate44 = predicate44.Or(x =>
            //        x.fec_aprobacion >= fechaDesde && x.fec_aprobacion <= fechaHasta && x.usado == true);
            //    predicate55 = predicate55.Or(x => x.desde >= fechaDesde && x.hasta <= fechaHasta && x.usado == true);
            //    predicate66 = predicate66.Or(x => x.fecha >= fechaDesde && x.fecha <= fechaHasta && x.usado == true);
            //    predicate77 = predicate77.Or(x => x.fecha >= fechaDesde && x.fecha <= fechaHasta && x.usado == true);
            //    predicate88 = predicate88.Or(x =>
            //        x.fechaevento >= fechaDesde && x.fechaevento <= fechaHasta && x.usado == true);
            //}

            predicate = predicate.And(predicate11);
            predicate2 = predicate2.And(predicate22);
            predicate3 = predicate3.And(predicate33);
            predicate4 = predicate4.And(predicate44);
            predicate5 = predicate5.And(predicate55);
            predicate6 = predicate6.And(predicate66);
            predicate7 = predicate7.And(predicate77);
            predicate8 = predicate8.And(predicate88);
            // tiket 3682 fabian vanegas se valida el objeto bodega q no sea nulo
            if (bodega!=null && bodega.Count() > 0 && bodega[0] != null)
                {
                foreach (var i in bodega)
                    {
                    var con = Convert.ToInt32(i);

                    bodegas = bodegas.Or(x => x.id_bodega == con);
                    bodegas2 = bodegas2.Or(x => x.id_bodega == con);
                    bodegas3 = bodegas3.Or(x => x.id_bodega == con);
                    bodegas4 = bodegas4.Or(x => x.id_bodega == con);
                    bodegas5 = bodegas5.Or(x => x.id_bodega == con);
                    bodegas6 = bodegas6.Or(x => x.id_bodega == con);
                    bodegas7 = bodegas7.Or(x => x.id_bodega == con);
                    bodegas8 = bodegas8.Or(x => x.id_bodega == con);
                    }

                predicate = predicate.And(bodegas);
                predicate2 = predicate2.And(bodegas2);
                predicate3 = predicate3.And(bodegas3);
                predicate4 = predicate4.And(bodegas4);
                predicate5 = predicate5.And(bodegas5);
                predicate6 = predicate6.And(bodegas6);
                predicate7 = predicate7.And(bodegas7);
                predicate8 = predicate8.And(bodegas8);
                }
            else
                {
                //busco las bodegas que tiene el usuario logueado

                //o si tiene permiso de ver todas las listo todas

                //mismo foreach del caso anterior
                foreach (var j in listarBodegas)
                    {
                    bodegas = bodegas.Or(x => x.id_bodega == j.id_bodega);
                    bodegas2 = bodegas2.Or(x => x.id_bodega == j.id_bodega);
                    bodegas3 = bodegas3.Or(x => x.id_bodega == j.id_bodega);
                    bodegas4 = bodegas4.Or(x => x.id_bodega == j.id_bodega);
                    bodegas5 = bodegas5.Or(x => x.id_bodega == j.id_bodega);
                    bodegas6 = bodegas6.Or(x => x.id_bodega == j.id_bodega);
                    bodegas7 = bodegas7.Or(x => x.id_bodega == j.id_bodega);
                    bodegas8 = bodegas8.Or(x => x.id_bodega == j.id_bodega);
                    }

                predicate = predicate.And(bodegas);
                predicate2 = predicate2.And(bodegas2);
                predicate3 = predicate3.And(bodegas3);
                predicate4 = predicate4.And(bodegas4);
                predicate5 = predicate5.And(bodegas5);
                predicate6 = predicate6.And(bodegas6);
                predicate7 = predicate7.And(bodegas7);
                predicate8 = predicate8.And(bodegas8);
                }


            //pregunto por los asesores
            if (asesor.Count() > 0 && asesor[0] != null)
                {
                var asesorex = new List<int>();

                foreach (var i in asesor)
                    {
                    var con = Convert.ToInt32(i);
                    asesorex.Add(con);
                    asesores = asesores.Or(x => x.asesor_id == con);
                    asesores2 = asesores2.Or(x => x.asesor_id == con);
                    asesores3 = asesores3.Or(x => x.asesor_id == con);
                    asesores4 = asesores4.Or(x => x.asesor_id == con);
                    asesores5 = asesores5.Or(x => x.asesor_id == con);
                    asesores6 = asesores6.Or(x => x.asesor_id == con);
                    asesores7 = asesores7.Or(x => x.asesor_id == con);
                    asesores8 = asesores8.Or(x => x.asesor_id == con);
                    }

                predicate = predicate.And(asesores);
                predicate2 = predicate2.And(asesores2);
                predicate3 = predicate3.And(asesores3);
                predicate4 = predicate4.And(asesores4);
                predicate5 = predicate5.And(asesores5);
                predicate6 = predicate6.And(asesores6);
                predicate7 = predicate7.And(asesores7);
                predicate8 = predicate8.And(asesores8);
                }
            else if (rol.rol_id == asesorventas || rol.rol_id == asesorusados)
                {
                asesores = asesores.Or(x => x.asesor_id == usuarioLog);
                asesores2 = asesores2.Or(x => x.asesor_id == usuarioLog);
                asesores3 = asesores3.Or(x => x.asesor_id == usuarioLog);
                asesores4 = asesores4.Or(x => x.asesor_id == usuarioLog);
                asesores5 = asesores5.Or(x => x.asesor_id == usuarioLog);
                asesores6 = asesores6.Or(x => x.asesor_id == usuarioLog);
                asesores7 = asesores7.Or(x => x.asesor_id == usuarioLog);
                asesores8 = asesores8.Or(x => x.asesor_id == usuarioLog);

                predicate = predicate.And(asesores);
                predicate2 = predicate2.And(asesores2);
                predicate3 = predicate3.And(asesores3);
                predicate4 = predicate4.And(asesores4);
                predicate5 = predicate5.And(asesores5);
                predicate6 = predicate6.And(asesores6);
                predicate7 = predicate7.And(asesores7);
                predicate8 = predicate8.And(asesores8);
                }

            var OrigenProspectos = context.vw_prospecto.Where(predicate).GroupBy(x => new
                { x.tporigen_id, x.tporigen_nombre }).Select(grp => new
                    {
                    cantidad = grp.Count(),
                    idorigen = grp.Key.tporigen_id,
                    nombre = grp.Key.tporigen_nombre
                    }).ToList();
            var prospectosListados = context.vw_prospecto.Where(predicate).Count();


            var cotizaciones2 = context.vw_cotizacion.Where(predicate2).ToList();
            var cotizaciones = cotizaciones2.Select(x => new
                {
                x.cot_numcotizacion,
                fecha = x.cot_feccreacion.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")),
                x.asesor,
                cliente = x.prinom_tercero + " " + x.segapellido_tercero + " " + x.apellido_tercero + " " +
                          x.segapellido_tercero + " " + x.razon_social,
                x.tporigen_nombre,
                x.tipo,
                telefono = x.telf_tercero != null ? x.telf_tercero : "",
                celular = x.celular_tercero != null ? x.celular_tercero : "",
                correo = x.email_tercero != null ? x.email_tercero : "",
                bodega = x.bodccs_nombre
                }).ToList();

            var solicitados2 = context.vw_creditosSolicitados.Where(predicate3).ToList();
            var solicitados = solicitados2.Select(x => new
                {
                x.pedido,
                fecha = x.fec_solicitud != null
                    ? x.fec_solicitud.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                x.valor_solicitado,
                x.estado,
                x.plazo,
                x.financiera,
                cliente = x.prinom_tercero + " " + x.segapellido_tercero + " " + x.apellido_tercero + " " +
                          x.segapellido_tercero + " " + x.razon_social,
                telefono = x.telf_tercero != null ? x.telf_tercero : "",
                celular = x.celular_tercero != null ? x.celular_tercero : "",
                correo = x.email_tercero != null ? x.email_tercero : "",
                x.asesor,
                x.tipo,
                bodega = x.bodccs_nombre
                }).ToList();

            var aprobados2 = context.vw_creditosAprobados.Where(predicate4).ToList();
            var aprobados = aprobados2.Select(x => new
                {
                x.numero,
                fecha = x.fec_aprobacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")),
                x.modvh_nombre,
                x.vsolicitado,
                x.plazo,
                x.financiera_nombre,
                cliente = x.prinom_tercero + " " + x.segapellido_tercero + " " + x.apellido_tercero + " " +
                          x.segapellido_tercero + " " + x.razon_social,
                telefono = x.telf_tercero != null ? x.telf_tercero : "",
                celular = x.celular_tercero != null ? x.celular_tercero : "",
                correo = x.email_tercero != null ? x.email_tercero : "",
                x.asesor,
                x.tipo,
                bodega = x.bodccs_nombre
                }).ToList();

            var demos2 = context.vw_demos.Where(predicate5).ToList();
            var demos = demos2.Select(x => new
                {
                x.ref_codigo,
                x.ref_descripcion,
                fecha = x.desde.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")),
                x.ncliente,
                nombreCliente = x.nombre,
                telefono = x.telefono != null ? x.telefono : "",
                celular = x.celular != null ? x.celular : "",
                correo = x.correo != null ? x.correo : "",
                x.asesor,
                x.tipo,
                nombre = x.bodccs_nombre
                }).ToList();

            var pedidos2 = context.vw_pedidos.Where(predicate6).ToList();
            var pedidos = pedidos2.Select(x => new
                {
                x.numero,
                fecha = x.fecha != null ? x.fecha.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")) : "",
                x.modvh_nombre,
                x.id_anio_modelo,
                x.colvh_nombre,
                placa = x.numeroplaca != null ? x.numeroplaca : "",
                plan_venta = x.plan_venta != null ? x.plan_venta : 0,
                plan_mayor = x.planmayor != null ? x.planmayor : "",
                fecha_asignacion = x.fecha_asignacion_planmayor != null
                    ? x.fecha_asignacion_planmayor.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                x.vrtotal,
                x.asesor,
                cliente = x.prinom_tercero + " " + x.segapellido_tercero + " " + x.apellido_tercero + " " +
                          x.segapellido_tercero + " " + x.razon_social,
                telefono = x.telf_tercero != null ? x.telf_tercero : "",
                celular = x.celular_tercero != null ? x.celular_tercero : "",
                correo = x.email_tercero != null ? x.email_tercero : "",
                x.tipo,
                bodega = x.bodccs_nombre
                }).ToList();

            var facturas2 = context.vw_facturas.Where(predicate7).ToList();
            var facturas = facturas2.Select(x => new
                {
                x.numero,
                tipo_factura = "(" + x.prefijo + ")" + x.tpdoc_nombre,
                fecha = x.fecha != null ? x.fecha.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-Us")) : "",
                nombre = x.prinom_tercero + " " + x.segapellido_tercero + " " + x.apellido_tercero + " " +
                         x.segapellido_tercero + " " + x.razon_social,
                telefono = x.telf_tercero != null ? x.telf_tercero : "",
                celular = x.celular_tercero != null ? x.celular_tercero : "",
                correo = x.email_tercero != null ? x.email_tercero : "",
                x.ref_descripcion,
                financiera = x.financiera_nombre != null ? x.financiera_nombre : "",
                x.fpago_nombre,
                fecha_vencimiento = x.vencimiento != null
                    ? x.vencimiento.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                x.valor_total,
                x.asesor,
                x.tipo,
                bodega = x.bodccs_nombre
                }).ToList();

            var matriculas2 = context.vw_matriculas.Where(predicate8).ToList();
            var matriculas = matriculas2.Select(x => new
                {
                fecha = x.fechaevento != null
                    ? x.fechaevento.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                x.planmayor,
                nombre = x.modvh_nombre,
                color = x.colvh_nombre,
                anio = x.anio_vh,
                x.tipo,
                x.Asesor,
                bodega = x.bodccs_nombre
                }).ToList();
            if (OrigenProspectos.Count == 0 && prospectosListados == 0 && cotizaciones.Count == 0 &&
                solicitados.Count == 0
                && aprobados.Count == 0 && demos.Count == 0 && pedidos.Count == 0 && facturas.Count == 0 &&
                matriculas.Count == 0)
                {
                var mensaje = "No hay información en los filtros seleccionados";
                return Json(new { info = false, mensaje }, JsonRequestBehavior.AllowGet);
                }

            var data = new
                {
                OrigenProspectos,
                prospectosListados,
                cotizaciones,
                solicitados,
                aprobados,
                demos,
                pedidos,
                facturas,
                matriculas
                };
            return Json(new { info = true, data }, JsonRequestBehavior.AllowGet);
            }


        public JsonResult buscarCitasPorVitrina(DateTime? fechaDesde, DateTime? fechaHasta, int?[] asesor, int?[] bodega,
            bool? nuevo, bool? usado)
            {
            var predicate = PredicateBuilder.True<vw_citas_agendamiento>();
            var asesores = PredicateBuilder.False<vw_citas_agendamiento>();
            var bodegas = PredicateBuilder.False<vw_citas_agendamiento>();
            var usuarioLog = Convert.ToInt32(Session["user_usuarioid"]);

            var rol = (from r in context.users
                       where r.user_id == usuarioLog
                       select new
                           {
                           r.rol_id
                           }).FirstOrDefault();

            var listarBodegas = (from v in context.vw_usuarios_rol
                                 where v.user_id == usuarioLog && v.es_vehiculos
                                 select new
                                     {
                                     v.id_bodega
                                     }).ToList();


            if (fechaDesde > fechaHasta)
                {
                var mensaje = "No hay información en los filtros seleccionados";
                return Json(new { info = false, mensaje }, JsonRequestBehavior.AllowGet);
                }

            if (fechaDesde != null)
                {
                predicate = predicate.And(d => d.fec_actualizacion >= fechaDesde);
                }
            if (fechaHasta != null)
                {
                fechaHasta = fechaHasta.Value.AddDays(1);
                predicate = predicate.And(d => d.fec_actualizacion <= fechaHasta);
                }
            //tiket 3682 fabian vanegas
            //se valida el objeto bodega que no sea nulo
            if (bodega!=null && bodega.Count() > 0 && bodega[0] != null)
                {
                foreach (var i in bodega)
                    {
                    var con = Convert.ToInt32(i);
                    bodegas = bodegas.Or(x => x.bodega == con);
                    }
                predicate = predicate.And(bodegas);
                }
            else
                {
                foreach (var j in listarBodegas)
                    {
                    bodegas = bodegas.Or(x => x.bodega == j.id_bodega);
                    }
                predicate = predicate.And(bodegas);
                }
            if (asesor.Count() > 0 && asesor[0] != null)
                {
                var asesorex = new List<int>();

                foreach (var i in asesor)
                    {
                    var con = Convert.ToInt32(i);
                    asesorex.Add(con);
                    asesores = asesores.Or(x => x.asesor_id == con);
                    }

                predicate = predicate.And(asesores);
                }
            else if (rol.rol_id == 4)
                {
                asesores = asesores.Or(x => x.asesor_id == usuarioLog);
                predicate = predicate.And(asesores);
                }

            var citax = context.vw_citas_agendamiento.Where(predicate).ToList();
            var citas = citax.GroupBy(d => new { d.bodega, d.nombre_bodega }).Select(d => new {
                vitrina = d.Key.bodega,
                nombrevitrina = d.Key.nombre_bodega,
                atendido = d.Where(e => e.estado == "Atendida").Count(),
                cancelado = d.Where(e => e.estado == "Cancelada").Count(),
                }).ToList();
            return Json(citas);
            }

        public JsonResult buscarDetallesProspectos(DateTime? fechaDesde, DateTime? fechaHasta, int?[] asesor,
            int?[] bodega, int origen, bool? nuevo, bool? usado)
            {
            var predicate = PredicateBuilder.True<vw_prospecto>();
            var asesores = PredicateBuilder.False<vw_prospecto>();
            var bodegas = PredicateBuilder.False<vw_prospecto>();
            var usuarioLog = Convert.ToInt32(Session["user_usuarioid"]);

            var rol = (from r in context.users
                       where r.user_id == usuarioLog
                       select new
                           {
                           r.rol_id
                           }).FirstOrDefault();

            var listarBodegas = (from v in context.vw_usuarios_rol
                                 where v.user_id == usuarioLog && v.es_vehiculos
                                 select new
                                     {
                                     v.id_bodega
                                     }).ToList();

            if (fechaDesde != null && fechaHasta != null && fechaDesde <= fechaHasta && nuevo == true && usado == false)
                {
                fechaHasta = fechaHasta.Value.AddDays(1);
                predicate = predicate.And(x =>
                    x.fechaInicial >= fechaDesde && x.fechaInicial <= fechaHasta && x.tporigen_id == origen &&
                    x.tptrapros_id == 1);
                }

            if (fechaDesde != null && fechaHasta != null && fechaDesde <= fechaHasta && nuevo == false && usado == true)
                {
                fechaHasta = fechaHasta.Value.AddDays(1);
                predicate = predicate.And(x =>
                    x.fechaInicial >= fechaDesde && x.fechaInicial <= fechaHasta && x.tporigen_id == origen &&
                    x.tptrapros_id == 2);
                }

            if (fechaDesde != null && fechaHasta != null && fechaDesde <= fechaHasta && nuevo == true && usado == true)
                {
                fechaHasta = fechaHasta.Value.AddDays(1);
                predicate = predicate.And(x =>
                    x.fechaInicial >= fechaDesde && x.fechaInicial <= fechaHasta && x.tporigen_id == origen);
                }

            if (bodega!=null && bodega.Count() > 0 && bodega[0] != null)
                {
                foreach (var i in bodega)
                    {
                    var con = Convert.ToInt32(i);

                    bodegas = bodegas.Or(x => x.id_bodega == con);
                    }

                predicate = predicate.And(bodegas);
                }
            else
                {
                foreach (var j in listarBodegas) bodegas = bodegas.Or(x => x.id_bodega == j.id_bodega);
                }

            predicate = predicate.And(bodegas);
            if (asesor.Count() > 0 && asesor[0] != null)
                {
                foreach (var i in asesor)
                    {
                    var con = Convert.ToInt32(i);

                    asesores = asesores.Or(x => x.asesor_id == con);
                    }

                predicate = predicate.And(asesores);
                }
            else if (rol.rol_id == 4)
                {
                asesores = asesores.Or(x => x.asesor_id == usuarioLog);
                predicate = predicate.And(asesores);
                }

            var prospectos1 = context.vw_prospecto.Where(predicate).ToList();
            var prospectos = prospectos1.Select(d => new
                {
                cliente = d.prinom_tercero + " " + d.segapellido_tercero + " " + d.apellido_tercero + " " +
                          d.segapellido_tercero + " " + d.razonsocial,
                dv = d.digitoverificacion != null ? d.digitoverificacion : "",
                documento = d.documento != null ? d.documento : "",
                telefono = d.telefono != null ? d.telefono : "",
                d.celular,
                d.correo,
                direccion = d.direccion != null ? d.direccion : "",
                fecha_inicial = d.fechaInicial.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")),
                d.asesor,
                d.tramite,
                d.asesor_id,
                d.bodccs_nombre,
                d.id_bodega,
                d.tporigen_nombre,
                fec_creacion = d.fechaInicial.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                }).ToList();
            var data = new
                {
                prospectos
                };


            return Json(data, JsonRequestBehavior.AllowGet);
            }

        public JsonResult PermisoVerIndicador()
            {
            var usuario = Convert.ToInt32(Session["user_usuarioid"]);

            var rol = (from r in context.users
                       where r.user_id == usuario
                       select
                           r.rol_id
                ).FirstOrDefault();

            //Validamos que el usuario loguado tenga el rol y el permiso para modificar los valores del pedido
            var permiso = (from u in context.users
                           join r in context.rols
                               on u.rol_id equals r.rol_id
                           join ra in context.rolacceso
                               on r.rol_id equals ra.idrol
                           where u.user_id == usuario && u.rol_id == rol
                           select new
                               {
                               u.user_id,
                               u.rol_id,
                               r.rol_nombre,
                               ra.idpermiso
                               }).ToList();

            return Json(permiso, JsonRequestBehavior.AllowGet);
            }

        //public JsonResult detalleCitas(DateTime? fechaDesde, DateTime? fechaHasta, int?[] asesor, int?[] bodega,
        //    bool? nuevo, bool? usado)
        //{
        //    var predicate = PredicateBuilder.True<vw_venta_accesorios_asesor>();
        //    var asesores = PredicateBuilder.False<vw_venta_accesorios_asesor>();
        //    var bodegas = PredicateBuilder.False<vw_venta_accesorios_asesor>();

        //}
        public JsonResult detalleAccesorios(DateTime? fechaDesde, DateTime? fechaHasta, int?[] asesor, int?[] bodega,
            bool? nuevo, bool? usado)
            {
            var predicate = PredicateBuilder.True<vw_venta_accesorios_asesor>();
            var asesores = PredicateBuilder.False<vw_venta_accesorios_asesor>();
            var bodegas = PredicateBuilder.False<vw_venta_accesorios_asesor>();
            if (fechaDesde > fechaHasta)
                {
                var mensaje = "No hay información en los filtros seleccionados";
                return Json(new { info = false, mensaje }, JsonRequestBehavior.AllowGet);
                }

            if (fechaDesde != null && fechaHasta != null && fechaDesde <= fechaHasta)
                predicate = predicate.And(x => x.fec_creacion >= fechaDesde && x.fec_creacion <= fechaHasta);
            //tiket 3682 fabian vanegas    
            //se valida el objeto bodega que no sea nulo
                if (bodega != null && bodega.Count() > 0 && bodega[0] != null)
                {
                foreach (var i in bodega)
                    {
                    var con = Convert.ToInt32(i);
                    bodegas = bodegas.Or(x => x.bodega_id == con);
                    }

                predicate = predicate.And(bodegas);
                }

            if (asesor.Count() > 0 && asesor[0] != null)
                {
                foreach (var i in asesor)
                    {
                    var con = Convert.ToInt32(i);

                    asesores = asesores.Or(x => x.asesor_id == con);
                    }

                predicate = predicate.And(asesores);
                }

            var accesorios = context.vw_venta_accesorios_asesor.Where(predicate);

            var cantidad = accesorios.Sum(x => x.cantidad) != null ? accesorios.Sum(x => x.cantidad) : 0;
            var valor_total = accesorios.Sum(x => x.valor_unitario) != null ? accesorios.Sum(x => x.valor_unitario) : 0;

            if (cantidad == 0)
                {
                var data = new
                    {
                    cantidad,
                    valor_total,
                    share = 0
                    };
                return Json(new { info = false, data }, JsonRequestBehavior.AllowGet);
                }
            else
                {
                var data = new
                    {
                    cantidad,
                    valor_total,
                    share = valor_total / cantidad
                    };
                return Json(new { info = true, data }, JsonRequestBehavior.AllowGet);
                }
            }

        public JsonResult metasAsesor(DateTime? fechaDesde, DateTime fechaHasta, int? asesor, int? bodega, bool? nuevo,
            bool? usado)
            {
            #region validar Usuario

            var usuarioLog = Convert.ToInt32(Session["user_usuarioid"]);

            var rol = (from r in context.users
                       where r.user_id == usuarioLog
                       select new
                           {
                           r.rol_id
                           }).FirstOrDefault();

            var listarBodegas = (from v in context.vw_usuarios_rol
                                 where v.user_id == usuarioLog && v.es_vehiculos
                                 select new
                                     {
                                     v.id_bodega
                                     }).ToList();

            ViewBag.listarBodegas = listarBodegas;

            fechaHasta = fechaHasta.AddDays(1);

            #endregion

            #region nuevo            

            if (fechaDesde > fechaHasta)
                {
                var mensaje = "No hay información en los filtros seleccionados";
                return Json(new { info = false, mensaje }, JsonRequestBehavior.AllowGet);
                }

            if (nuevo == true)
                {
                var consultaCotizacion = (from a in context.vw_cotizacion
                                          where a.asesor_id == asesor
                                                && a.id_bodega == bodega
                                                && a.cot_feccreacion >= fechaDesde
                                                && a.cot_feccreacion <= fechaHasta
                                                && a.nuevo == true
                                          select new
                                              {
                                              a.cot_numcotizacion
                                              }).ToList();

                var consultaCreditosSolicitados = (from a in context.vw_creditosSolicitados
                                                   where a.asesor_id == asesor
                                                         && a.id_bodega == bodega
                                                         && a.fec_solicitud >= fechaDesde
                                                         && a.fec_solicitud <= fechaHasta
                                                         && a.nuevo
                                                   select new
                                                       {
                                                       a.pedido
                                                       }).ToList();

                var consultaCreditosAprobados = (from a in context.vw_creditosAprobados
                                                 where a.asesor_id == asesor
                                                       && a.id_bodega == bodega
                                                       && a.fec_solicitud >= fechaDesde
                                                       && a.fec_solicitud <= fechaHasta
                                                       && a.nuevo
                                                 select new
                                                     {
                                                     a.numero
                                                     }).ToList();

                var consultaDemos = (from a in context.vw_demos
                                     where a.asesor_id == asesor
                                           && a.id_bodega == bodega
                                           && a.desde >= fechaDesde
                                           && a.hasta <= fechaHasta
                                           && a.nuevo == true
                                     select new
                                         {
                                         a.ref_codigo
                                         }).ToList();

                var consultaFacturas = (from a in context.vw_facturas
                                        where a.asesor_id == asesor
                                              && a.id_bodega == bodega
                                              && a.fecha >= fechaDesde
                                              && a.fecha <= fechaHasta
                                              && a.nuevo == true
                                        select new
                                            {
                                            a.numero
                                            }).ToList();

                var consultaMatriculas = (from a in context.vw_matriculas
                                          where a.asesor_id == asesor
                                                && a.id_bodega == bodega
                                                && a.fechaevento >= fechaDesde
                                                && a.fechaevento <= fechaHasta
                                                && a.nuevo == true
                                          select new
                                              {
                                              a.planmayor
                                              }).ToList();

                var consultaPedidos = (from a in context.vw_pedidos
                                       where a.asesor_id == asesor
                                             && a.id_bodega == bodega
                                             && a.fecha >= fechaDesde
                                             && a.fecha <= fechaHasta
                                             && a.nuevo
                                       select new
                                           {
                                           a.planmayor
                                           }).ToList();
                var consultaMetas = (from a in context.metas_asesor
                                     join b in context.item_metas
                                         on a.meta equals b.id
                                     where a.bodega == bodega && a.meta != 8
                                     select new
                                         {
                                         id = a.meta,
                                         meta = b.descripcion,
                                         a.valor
                                         }).ToList();
                var consultaMetaShare = (from a in context.metas_asesor
                                         join b in context.item_metas
                                             on a.meta equals b.id
                                         where a.bodega == bodega && a.meta == 8
                                         select new
                                             {
                                             id = a.meta,
                                             meta = b.descripcion,
                                             a.valor
                                             }).ToList();

                var consultaMetasCotizaciones =
                    consultaMetas.Where(x => x.id == 1).Select(x => x.valor).FirstOrDefault();
                var consultaMetasCreditosSolicitados =
                    consultaMetas.Where(x => x.id == 2).Select(x => x.valor).FirstOrDefault();
                var consultaMetasCreditosAprobados =
                    consultaMetas.Where(x => x.id == 3).Select(x => x.valor).FirstOrDefault();
                var consultaMetasDemos = consultaMetas.Where(x => x.id == 4).Select(x => x.valor).FirstOrDefault();
                var consultaMetasFacturas = consultaMetas.Where(x => x.id == 5).Select(x => x.valor).FirstOrDefault();
                var consultaMetasMatriculas = consultaMetas.Where(x => x.id == 6).Select(x => x.valor).FirstOrDefault();
                var consultaMetasPedidos = consultaMetas.Where(x => x.id == 7).Select(x => x.valor).FirstOrDefault();
                //tiket 3682 fabian vanegas se adiciona validacion para evitar la division x 0
                var porcentCotizaciones = Convert.ToInt32(consultaMetasCotizaciones)>0?consultaCotizacion.Count() * 100 / Convert.ToInt32(consultaMetasCotizaciones):0;
                var porcentCreditosSolicitados = Convert.ToInt32(consultaMetasCreditosSolicitados)>0?consultaCreditosSolicitados.Count() * 100 /Convert.ToInt32(consultaMetasCreditosSolicitados):0;
                var porcentConsultaCreditosAprobados = Convert.ToInt32(consultaMetasCreditosAprobados)>0?consultaCreditosAprobados.Count() * 100 / Convert.ToInt32(consultaMetasCreditosAprobados):0;
                var porcentConsultaDemos = Convert.ToInt32(consultaMetasDemos)>0?consultaDemos.Count() * 100 / Convert.ToInt32(consultaMetasDemos):0;
                var porcentConsultaPedidos = Convert.ToInt32(consultaMetasPedidos)>0?consultaPedidos.Count() * 100 / Convert.ToInt32(consultaMetasPedidos):0;
                var porcentConsultaFacturas = Convert.ToInt32(consultaMetasFacturas)>0?consultaFacturas.Count() * 100 / Convert.ToInt32(consultaMetasFacturas):0;
                var porcentConsultaMatriculas = Convert.ToInt32(consultaMetasMatriculas)>0?consultaMatriculas.Count() * 100 / Convert.ToInt32(consultaMetasMatriculas):0;
                var colorCot = "";
                var colorSol = "";
                var colorApr = "";
                var colorDem = "";
                var colorPed = "";
                var colorFac = "";
                var colorMat = "";
                int[] porcentaje =
                {
                    porcentCotizaciones,
                    porcentCreditosSolicitados,
                    porcentConsultaCreditosAprobados,
                    porcentConsultaDemos,
                    porcentConsultaPedidos,
                    porcentConsultaFacturas,
                    porcentConsultaMatriculas
                };

                colorCot = colorfunel(porcentaje[0]);
                colorSol = colorfunel(porcentaje[1]);
                colorApr = colorfunel(porcentaje[2]);
                colorDem = colorfunel(porcentaje[3]);
                colorPed = colorfunel(porcentaje[4]);
                colorFac = colorfunel(porcentaje[5]);
                colorMat = colorfunel(porcentaje[6]);
                var data = new
                    {
                    consultaCotizacion,
                    consultaCreditosAprobados,
                    consultaCreditosSolicitados,
                    consultaDemos,
                    consultaFacturas,
                    consultaMatriculas,
                    consultaPedidos,
                    consultaMetas,
                    consultaMetaShare,
                    colorCot,
                    colorSol,
                    colorApr,
                    colorDem,
                    colorPed,
                    colorFac,
                    colorMat
                    };
                return Json(new { info = true, data }, JsonRequestBehavior.AllowGet);
                }

            #endregion

            #region Usado            

            else
                {
                var consultaCotizacion = (from a in context.vw_cotizacion
                                          where a.asesor_id == asesor
                                                && a.id_bodega == bodega
                                                && a.cot_feccreacion >= fechaDesde
                                                && a.cot_feccreacion <= fechaHasta
                                                && a.usado == true
                                          select new
                                              {
                                              a.cot_numcotizacion
                                              }).ToList();

                var consultaCreditosSolicitados = (from a in context.vw_creditosSolicitados
                                                   where a.asesor_id == asesor
                                                         && a.id_bodega == bodega
                                                         && a.fec_solicitud >= fechaDesde
                                                         && a.fec_solicitud <= fechaHasta
                                                         && a.usado == true
                                                   select new
                                                       {
                                                       a.pedido
                                                       }).ToList();

                var consultaCreditosAprobados = (from a in context.vw_creditosAprobados
                                                 where a.asesor_id == asesor
                                                       && a.id_bodega == bodega
                                                       && a.fec_solicitud >= fechaDesde
                                                       && a.fec_solicitud <= fechaHasta
                                                       && a.usado == true
                                                 select new
                                                     {
                                                     a.numero
                                                     }).ToList();

                var consultaDemos = (from a in context.vw_demos
                                     where a.asesor_id == asesor
                                           && a.id_bodega == bodega
                                           && a.desde >= fechaDesde
                                           && a.hasta <= fechaHasta
                                           && a.usado == true
                                     select new
                                         {
                                         a.ref_codigo
                                         }).ToList();

                var consultaFacturas = (from a in context.vw_facturas
                                        where a.asesor_id == asesor
                                              && a.id_bodega == bodega
                                              && a.fecha >= fechaDesde
                                              && a.fecha <= fechaHasta
                                              && a.usado == true
                                        select new
                                            {
                                            a.numero
                                            }).ToList();

                var consultaMatriculas = (from a in context.vw_matriculas
                                          where a.asesor_id == asesor
                                                && a.id_bodega == bodega
                                                && a.fechaevento >= fechaDesde
                                                && a.fechaevento <= fechaHasta
                                                && a.usado == true
                                          select new
                                              {
                                              a.planmayor
                                              }).ToList();

                var consultaPedidos = (from a in context.vw_pedidos
                                       where a.asesor_id == asesor
                                             && a.id_bodega == bodega
                                             && a.fecha >= fechaDesde
                                             && a.fecha <= fechaHasta
                                             && a.usado == true
                                       select new
                                           {
                                           a.planmayor
                                           }).ToList();
                var consultaMetas = (from a in context.metas_asesor
                                     join b in context.item_metas
                                         on a.meta equals b.id
                                     where a.bodega == bodega && a.meta != 8
                                     select new
                                         {
                                         id = a.meta,
                                         meta = b.descripcion,
                                         a.valor
                                         }).ToList();
                var consultaMetaShare = (from a in context.metas_asesor
                                         join b in context.item_metas
                                             on a.meta equals b.id
                                         where a.bodega == bodega && a.meta == 8
                                         select new
                                             {
                                             id = a.meta,
                                             meta = b.descripcion,
                                             a.valor
                                             }).ToList();

                var consultaMetasCotizaciones =
                    consultaMetas.Where(x => x.id == 1).Select(x => x.valor).FirstOrDefault();
                var consultaMetasCreditosSolicitados =
                    consultaMetas.Where(x => x.id == 2).Select(x => x.valor).FirstOrDefault();
                var consultaMetasCreditosAprobados =
                    consultaMetas.Where(x => x.id == 3).Select(x => x.valor).FirstOrDefault();
                var consultaMetasDemos = consultaMetas.Where(x => x.id == 4).Select(x => x.valor).FirstOrDefault();
                var consultaMetasFacturas = consultaMetas.Where(x => x.id == 5).Select(x => x.valor).FirstOrDefault();
                var consultaMetasMatriculas = consultaMetas.Where(x => x.id == 6).Select(x => x.valor).FirstOrDefault();
                var consultaMetasPedidos = consultaMetas.Where(x => x.id == 7).Select(x => x.valor).FirstOrDefault();
                //tiket 3682 fabian vanegas
                //se coloca validaciones para evitar division X 0
                var porcentCotizaciones = Convert.ToInt32(consultaMetasCotizaciones)>0?consultaCotizacion.Count() * 100 / Convert.ToInt32(consultaMetasCotizaciones):0;
                var porcentCreditosSolicitados = Convert.ToInt32(consultaMetasCreditosSolicitados)>0?consultaCreditosSolicitados.Count() * 100 /Convert.ToInt32(consultaMetasCreditosSolicitados):0;
                var porcentConsultaCreditosAprobados = Convert.ToInt32(consultaMetasCreditosAprobados)>0?consultaCreditosAprobados.Count() * 100 / Convert.ToInt32(consultaMetasCreditosAprobados):0;
                var porcentConsultaDemos = Convert.ToInt32(consultaMetasDemos)>0?consultaDemos.Count() * 100 / Convert.ToInt32(consultaMetasDemos):0;
                var porcentConsultaPedidos = Convert.ToInt32(consultaMetasPedidos)>0?consultaPedidos.Count() * 100 / Convert.ToInt32(consultaMetasPedidos):0;
                var porcentConsultaFacturas = Convert.ToInt32(consultaMetasFacturas)>0?consultaFacturas.Count() * 100 / Convert.ToInt32(consultaMetasFacturas):0;
                var porcentConsultaMatriculas = Convert.ToInt32(consultaMetasMatriculas)>0?consultaMatriculas.Count() * 100 / Convert.ToInt32(consultaMetasMatriculas):0;
                var colorCot = "";
                var colorSol = "";
                var colorApr = "";
                var colorDem = "";
                var colorPed = "";
                var colorFac = "";
                var colorMat = "";
                int[] porcentaje =
                {
                    porcentCotizaciones,
                    porcentCreditosSolicitados,
                    porcentConsultaCreditosAprobados,
                    porcentConsultaDemos,
                    porcentConsultaPedidos,
                    porcentConsultaFacturas,
                    porcentConsultaMatriculas
                };

                colorCot = colorfunel(porcentaje[0]);
                colorSol = colorfunel(porcentaje[1]);
                colorApr = colorfunel(porcentaje[2]);
                colorDem = colorfunel(porcentaje[3]);
                colorPed = colorfunel(porcentaje[4]);
                colorFac = colorfunel(porcentaje[5]);
                colorMat = colorfunel(porcentaje[6]);
                var data = new
                    {
                    consultaCotizacion,
                    consultaCreditosAprobados,
                    consultaCreditosSolicitados,
                    consultaDemos,
                    consultaFacturas,
                    consultaMatriculas,
                    consultaPedidos,
                    consultaMetas,
                    consultaMetaShare,
                    colorCot,
                    colorSol,
                    colorApr,
                    colorDem,
                    colorPed,
                    colorFac,
                    colorMat
                    };
                return Json(new { info = true, data }, JsonRequestBehavior.AllowGet);
                }

            #endregion
            }

        public JsonResult tablaRanking(int?[] bodega, DateTime fechaDesde, DateTime fechaHasta)
            {
            var predicate = PredicateBuilder.True<vw_matriculas>();
            var predicate2 = PredicateBuilder.False<vw_matriculas>();

            if (fechaDesde != null && fechaHasta != null && fechaDesde <= fechaHasta)
                predicate = predicate.And(x => x.fechaevento >= fechaDesde && x.fechaevento <= fechaHasta);
            if (bodega != null && bodega.Count() > 0 && bodega[0] != null)
                {
                foreach (var i in bodega)
                    {
                    var con = Convert.ToInt32(i);
                    predicate2 = predicate2.Or(x => x.id_bodega == con);
                    }

                predicate = predicate.And(predicate2);
                }

            var matriculas = context.vw_matriculas.Where(predicate).GroupBy(x => new
                { x.asesor_id, x.Asesor }).OrderByDescending(g => g.Count()).Select(grp => new
                    {
                    cantidad = grp.Count(),
                    id = grp.Key.asesor_id,
                    nombre = grp.Key.Asesor
                    }).ToList();
            if (matriculas.Count == 0)
                {
                var mensaje = "No hay información en los filtros seleccionados";
                return Json(new { info = false, mensaje }, JsonRequestBehavior.AllowGet);
                }

            return Json(new { info = true, matriculas }, JsonRequestBehavior.AllowGet);
            }

        public JsonResult pronostico(DateTime fechaHasta, int? asesor, int? bodega, bool? nuevo, bool? usado)
            {
            var mes = DateTime.Now.Month;
            var anio = DateTime.Now.Year;
            var now = DateTime.Now;
            var fechaDesde = new DateTime(now.Year, now.Month, 1);
            var fechaActual = DateTime.Now;
            var diaMes = DateTime.DaysInMonth(fechaActual.Year, fechaActual.Month);
            decimal diasRestantes = diaMes - DateTime.Now.Day;


            #region validar Usuario

            var usuarioLog = Convert.ToInt32(Session["user_usuarioid"]);

            var rol = (from r in context.users
                       where r.user_id == usuarioLog
                       select new
                           {
                           r.rol_id
                           }).FirstOrDefault();

            var listarBodegas = (from v in context.vw_usuarios_rol
                                 where v.user_id == usuarioLog && v.es_vehiculos
                                 select new
                                     {
                                     v.id_bodega
                                     }).ToList();

            ViewBag.listarBodegas = listarBodegas;

            fechaHasta = fechaHasta.AddDays(1);

            #endregion

            #region nuevo            

            if (fechaDesde > fechaHasta)
                {
                var mensaje = "No hay información en los filtros seleccionados";
                return Json(new { info = false, mensaje }, JsonRequestBehavior.AllowGet);
                }

            if (nuevo == true)
                {
                var consultaCotizacion = (from a in context.vw_cotizacion
                                          where a.asesor_id == asesor
                                                && a.id_bodega == bodega
                                                && a.cot_feccreacion >= fechaDesde
                                                && a.cot_feccreacion <= fechaHasta
                                                && a.nuevo == true
                                          select new
                                              {
                                              a.cot_numcotizacion
                                              }).Count();

                var consultaCreditosSolicitados = (from a in context.vw_creditosSolicitados
                                                   where a.asesor_id == asesor
                                                         && a.id_bodega == bodega
                                                         && a.fec_solicitud >= fechaDesde
                                                         && a.fec_solicitud <= fechaHasta
                                                         && a.nuevo
                                                   select new
                                                       {
                                                       a.pedido
                                                       }).Count();

                var consultaCreditosAprobados = (from a in context.vw_creditosAprobados
                                                 where a.asesor_id == asesor
                                                       && a.id_bodega == bodega
                                                       && a.fec_solicitud >= fechaDesde
                                                       && a.fec_solicitud <= fechaHasta
                                                       && a.nuevo
                                                 select new
                                                     {
                                                     a.numero
                                                     }).Count();

                var consultaDemos = (from a in context.vw_demos
                                     where a.asesor_id == asesor
                                           && a.id_bodega == bodega
                                           && a.desde >= fechaDesde
                                           && a.hasta <= fechaHasta
                                           && a.nuevo == true
                                     select new
                                         {
                                         a.ref_codigo
                                         }).Count();

                var consultaFacturas = (from a in context.vw_facturas
                                        where a.asesor_id == asesor
                                              && a.id_bodega == bodega
                                              && a.fecha >= fechaDesde
                                              && a.fecha <= fechaHasta
                                              && a.nuevo == true
                                        select new
                                            {
                                            a.numero
                                            }).Count();

                var consultaMatriculas = (from a in context.vw_matriculas
                                          where a.asesor_id == asesor
                                                && a.id_bodega == bodega
                                                && a.fechaevento >= fechaDesde
                                                && a.fechaevento <= fechaHasta
                                                && a.nuevo == true
                                          select new
                                              {
                                              a.planmayor
                                              }).Count();

                var consultaPedidos = (from a in context.vw_pedidos
                                       where a.asesor_id == asesor
                                             && a.id_bodega == bodega
                                             && a.fecha >= fechaDesde
                                             && a.fecha <= fechaHasta
                                             && a.nuevo
                                       select new
                                           {
                                           a.planmayor
                                           }).Count();

                var cotizacion = Convert.ToDecimal(consultaCotizacion, miCultura) / Convert.ToDecimal(now.Day, miCultura) * diasRestantes +
                                 Convert.ToDecimal(consultaCotizacion, miCultura);
                var creditosSolicitados =
                    Convert.ToDecimal(consultaCreditosSolicitados, miCultura) / Convert.ToDecimal(now.Day, miCultura) * diasRestantes +
                    Convert.ToDecimal(consultaCreditosSolicitados, miCultura);
                var creditosAprobados =
                    Convert.ToDecimal(consultaCreditosAprobados, miCultura) / Convert.ToDecimal(now.Day, miCultura) * diasRestantes +
                    Convert.ToDecimal(consultaCreditosAprobados, miCultura);
                var demos = Convert.ToDecimal(consultaDemos, miCultura) / Convert.ToDecimal(now.Day, miCultura) * diasRestantes +
                            Convert.ToDecimal(consultaDemos, miCultura);
                var facturas = Convert.ToDecimal(consultaFacturas, miCultura) / Convert.ToDecimal(now.Day, miCultura) * diasRestantes +
                               Convert.ToDecimal(consultaFacturas, miCultura);
                var matriculas = Convert.ToDecimal(consultaMatriculas, miCultura) / Convert.ToDecimal(now.Day, miCultura) * diasRestantes +
                                 Convert.ToDecimal(consultaMatriculas, miCultura);
                var pedidos = Convert.ToDecimal(consultaPedidos, miCultura) / Convert.ToDecimal(now.Day, miCultura) * diasRestantes +
                              Convert.ToDecimal(consultaPedidos, miCultura);


                var data = new
                    {
                    cotizacion = Convert.ToInt32(cotizacion),
                    creditosSolicitados = Convert.ToInt32(creditosSolicitados),
                    creditosAprobados = Convert.ToInt32(creditosAprobados),
                    demos = Convert.ToInt32(demos),
                    facturas = Convert.ToInt32(facturas),
                    matriculas = Convert.ToInt32(matriculas),
                    pedidos = Convert.ToInt32(pedidos)
                    };
                return Json(new { info = true, data }, JsonRequestBehavior.AllowGet);
                }

            #endregion

            #region Usado            

            else
                {
                var consultaCotizacion = (from a in context.vw_cotizacion
                                          where a.asesor_id == asesor
                                                && a.id_bodega == bodega
                                                && a.cot_feccreacion >= fechaDesde
                                                && a.cot_feccreacion <= fechaHasta
                                                && a.usado == true
                                          select new
                                              {
                                              a.cot_numcotizacion
                                              }).Count();

                var consultaCreditosSolicitados = (from a in context.vw_creditosSolicitados
                                                   where a.asesor_id == asesor
                                                         && a.id_bodega == bodega
                                                         && a.fec_solicitud >= fechaDesde
                                                         && a.fec_solicitud <= fechaHasta
                                                         && a.usado == true
                                                   select new
                                                       {
                                                       a.pedido
                                                       }).Count();

                var consultaCreditosAprobados = (from a in context.vw_creditosAprobados
                                                 where a.asesor_id == asesor
                                                       && a.id_bodega == bodega
                                                       && a.fec_solicitud >= fechaDesde
                                                       && a.fec_solicitud <= fechaHasta
                                                       && a.usado == true
                                                 select new
                                                     {
                                                     a.numero
                                                     }).Count();

                var consultaDemos = (from a in context.vw_demos
                                     where a.asesor_id == asesor
                                           && a.id_bodega == bodega
                                           && a.desde >= fechaDesde
                                           && a.hasta <= fechaHasta
                                           && a.usado == true
                                     select new
                                         {
                                         a.ref_codigo
                                         }).Count();

                var consultaFacturas = (from a in context.vw_facturas
                                        where a.asesor_id == asesor
                                              && a.id_bodega == bodega
                                              && a.fecha >= fechaDesde
                                              && a.fecha <= fechaHasta
                                              && a.usado == true
                                        select new
                                            {
                                            a.numero
                                            }).Count();

                var consultaMatriculas = (from a in context.vw_matriculas
                                          where a.asesor_id == asesor
                                                && a.id_bodega == bodega
                                                && a.fechaevento >= fechaDesde
                                                && a.fechaevento <= fechaHasta
                                                && a.usado == true
                                          select new
                                              {
                                              a.planmayor
                                              }).Count();

                var consultaPedidos = (from a in context.vw_pedidos
                                       where a.asesor_id == asesor
                                             && a.id_bodega == bodega
                                             && a.fecha >= fechaDesde
                                             && a.fecha <= fechaHasta
                                             && a.usado == true
                                       select new
                                           {
                                           a.planmayor
                                           }).Count();


                var cotizacion = Convert.ToDecimal(consultaCotizacion, miCultura) / Convert.ToDecimal(now.Day, miCultura) * diasRestantes +
                                 Convert.ToDecimal(consultaCotizacion, miCultura);
                var creditosSolicitados =
                    Convert.ToDecimal(consultaCreditosSolicitados, miCultura) / Convert.ToDecimal(now.Day, miCultura) * diasRestantes +
                    Convert.ToDecimal(consultaCreditosSolicitados, miCultura);
                var creditosAprobados =
                    Convert.ToDecimal(consultaCreditosAprobados, miCultura) / Convert.ToDecimal(now.Day, miCultura) * diasRestantes +
                    Convert.ToDecimal(consultaCreditosAprobados);
                var demos = Convert.ToDecimal(consultaDemos, miCultura) / Convert.ToDecimal(now.Day, miCultura) * diasRestantes +
                            Convert.ToDecimal(consultaDemos, miCultura);
                var facturas = Convert.ToDecimal(consultaFacturas, miCultura) / Convert.ToDecimal(now.Day, miCultura) * diasRestantes +
                               Convert.ToDecimal(consultaFacturas, miCultura);
                var matriculas = Convert.ToDecimal(consultaMatriculas, miCultura) / Convert.ToDecimal(now.Day, miCultura) * diasRestantes +
                                 Convert.ToDecimal(consultaMatriculas, miCultura);
                var pedidos = Convert.ToDecimal(consultaPedidos, miCultura) / Convert.ToDecimal(now.Day, miCultura) * diasRestantes +
                              Convert.ToDecimal(consultaPedidos, miCultura);


                var data = new
                    {
                    cotizacion = Convert.ToInt32(cotizacion),
                    creditosSolicitados = Convert.ToInt32(creditosSolicitados),
                    creditosAprobados = Convert.ToInt32(creditosAprobados),
                    demos = Convert.ToInt32(demos),
                    facturas = Convert.ToInt32(facturas),
                    matriculas = Convert.ToInt32(matriculas),
                    pedidos = Convert.ToInt32(pedidos)
                    };
                return Json(new { info = true, data }, JsonRequestBehavior.AllowGet);
                }

            #endregion
            }

        public JsonResult EntradasAnfitriona(DateTime? fechaDesde, string fechaHasta2, int?[] bodega, bool? nuevo,
            bool? usado)
            {
            var rol = Convert.ToInt32(Session["user_rolid"]);
            var usuario = Convert.ToInt32(Session["user_usuarioid"]);
            var fechaHasta = DateTime.Now;
            var buscarBodega = new List<string>();
            var coordinador = false;
            var asesor = false;

            //busco el rol y su permiso
            var tiene = context.rolacceso.Where(d => d.idrol == rol && d.rolpermisos.codigo == "P44").Count();
            if (fechaDesde == null) fechaDesde = DateTime.Now.AddDays(-30);
            if (fechaHasta2 == null || fechaHasta2 == "")
                {
                fechaHasta = DateTime.Now.AddDays(1);
                }
            else
                {
                fechaHasta = Convert.ToDateTime(fechaHasta2, new CultureInfo("en-US"));
                fechaHasta = fechaHasta.AddDays(1);
                }

            var predicate = PredicateBuilder.True<vw_creditos_prospectos>();
            var bodegas = PredicateBuilder.False<vw_creditos_prospectos>();

            if (bodega != null && bodega[0] != null)
            {
                foreach (var item in bodega)
                {
                    var id_bodega = Convert.ToInt32(item);
                    bodegas = bodegas.Or(x => x.id_bodega == item);
                }

                predicate = predicate.And(bodegas);
            }

            /*if (rol == 8 || rol == 14 || rol == 2039 || rol == 2034) //Roles de coordinador/
            {*/
            if (tiene>0) /*Roles de coordinador*/
                {
                    predicate = predicate.And(x =>
                    x.fechaProspecto >= fechaDesde && x.fechaProspecto <= fechaHasta && x.rol_id == 7 &&
                    x.tporigen_id == 1);
                coordinador = true;

            }   
            //else if (rol == 4 || rol == 2030) /*Roles de Asesor*/
            else /*Roles de Asesor*/
                    {
                predicate = predicate.And(x =>
                    x.fechaProspecto >= fechaDesde && x.fechaProspecto <= fechaHasta && x.rol_id == 7 &&
                    x.tporigen_id == 1 && x.asesor_id == usuario);
                asesor = true;
            }

            var prospectos = context.vw_creditos_prospectos.Where(predicate).GroupBy(x => new
                    { x.bodccs_nombre }).Select(grp => new
                        {
                        bodega = grp.Key.bodccs_nombre,
                        cantidad = grp.GroupBy(x => x.idtercero).Count(),
                        credito = grp.Where(x => x.id_credito != null).GroupBy(x => x.idtercero).Count()
                        }).ToList();

                var data = new
                    {
                    prospectos
                    };
                

            return Json(new
                {
                data,
                coordinador,
                asesor
                }, JsonRequestBehavior.AllowGet);
            }

        public JsonResult buscarMatriculas(int anio_matriculas)
            {
            var rol = Convert.ToInt32(Session["user_rolid"]);
            var usuario = Convert.ToInt32(Session["user_usuarioid"]);
            var anio = anio_matriculas;
            var listaConsulta = new List<ConsultaMatriculasModel>();


            #region asesores
            //busco el rol y su permiso
            var tiene = context.rolacceso.Where(d => d.idrol == rol && d.rolpermisos.codigo == "P44").Count();


            if (rol == 4 || rol == 2030) // If para asesores
                {
                var buscarMatriculas = context.vw_matriculas.Where(x => x.asesor_id == usuario)
                    .GroupBy(x => x.asesor_id).Select(grupo => new
                        {
                        enero = grupo.Where(x => x.fechaevento.Month == 1 && x.fechaevento.Year == anio).Count(),
                        febrero = grupo.Where(x => x.fechaevento.Month == 2 && x.fechaevento.Year == anio).Count(),
                        marzo = grupo.Where(x => x.fechaevento.Month == 3 && x.fechaevento.Year == anio).Count(),
                        abril = grupo.Where(x => x.fechaevento.Month == 4 && x.fechaevento.Year == anio).Count(),
                        mayo = grupo.Where(x => x.fechaevento.Month == 5 && x.fechaevento.Year == anio).Count(),
                        junio = grupo.Where(x => x.fechaevento.Month == 6 && x.fechaevento.Year == anio).Count(),
                        julio = grupo.Where(x => x.fechaevento.Month == 7 && x.fechaevento.Year == anio).Count(),
                        agosto = grupo.Where(x => x.fechaevento.Month == 8 && x.fechaevento.Year == anio).Count(),
                        septiembre = grupo.Where(x => x.fechaevento.Month == 9 && x.fechaevento.Year == anio).Count(),
                        octubre = grupo.Where(x => x.fechaevento.Month == 10 && x.fechaevento.Year == anio).Count(),
                        noviembre = grupo.Where(x => x.fechaevento.Month == 11 && x.fechaevento.Year == anio).Count(),
                        diciembre = grupo.Where(x => x.fechaevento.Month == 12 && x.fechaevento.Year == anio).Count()
                        }).ToList();

                if (buscarMatriculas.Count == 0)
                    buscarMatriculas = context.vw_matriculas.GroupBy(x => x.asesor_id).Select(grupo => new
                        {
                        enero = 0,
                        febrero = 0,
                        marzo = 0,
                        abril = 0,
                        mayo = 0,
                        junio = 0,
                        julio = 0,
                        agosto = 0,
                        septiembre = 0,
                        octubre = 0,
                        noviembre = 0,
                        diciembre = 0
                        }).ToList();

                var buscarAsesor = context.users.Where(x => x.user_id == usuario).Select(x => new
                    {
                    nombre = x.user_nombre + " " + x.user_apellido
                    }).FirstOrDefault();

                var data = new
                    {
                    buscarMatriculas,
                    buscarAsesor
                    };

                return Json(new { data, asesor = true }, JsonRequestBehavior.AllowGet);
                }

            #endregion

            #region coordinadores

            //if (rol == 14 || rol == 8)
            if (tiene>0)
                {
                var bodegas = context.bodega_usuario.Where(x => x.id_usuario == usuario).Select(x => x.id_bodega)
                    .ToList();
                var asesores = new List<int>();

                foreach (var item in bodegas)
                    {
                    var bodega_id = Convert.ToInt32(item);

                    var buscar = (from a in context.users
                                  join b in context.bodega_usuario
                                      on a.user_id equals b.id_usuario
                                  where b.id_bodega == bodega_id && (a.rol_id == 4 || a.rol_id == 2030)
                                  select a.user_id).ToList();

                    foreach (var asesor in buscar)
                        {
                        var user = Convert.ToInt32(asesor);
                        asesores.Add(user);
                        }
                    }

                foreach (var a in asesores)
                    {
                    var buscarMatriculas = context.vw_matriculas.Where(x => x.asesor_id == a).GroupBy(x => x.Asesor)
                        .Select(grupo => new
                            {
                            enero = grupo.Where(x => x.fechaevento.Month == 1 && x.fechaevento.Year == anio).Count(),
                            febrero = grupo.Where(x => x.fechaevento.Month == 2 && x.fechaevento.Year == anio).Count(),
                            marzo = grupo.Where(x => x.fechaevento.Month == 3 && x.fechaevento.Year == anio).Count(),
                            abril = grupo.Where(x => x.fechaevento.Month == 4 && x.fechaevento.Year == anio).Count(),
                            mayo = grupo.Where(x => x.fechaevento.Month == 5 && x.fechaevento.Year == anio).Count(),
                            junio = grupo.Where(x => x.fechaevento.Month == 6 && x.fechaevento.Year == anio).Count(),
                            julio = grupo.Where(x => x.fechaevento.Month == 7 && x.fechaevento.Year == anio).Count(),
                            agosto = grupo.Where(x => x.fechaevento.Month == 8 && x.fechaevento.Year == anio).Count(),
                            septiembre = grupo.Where(x => x.fechaevento.Month == 9 && x.fechaevento.Year == anio)
                                .Count(),
                            octubre = grupo.Where(x => x.fechaevento.Month == 10 && x.fechaevento.Year == anio).Count(),
                            noviembre = grupo.Where(x => x.fechaevento.Month == 11 && x.fechaevento.Year == anio)
                                .Count(),
                            diciembre = grupo.Where(x => x.fechaevento.Month == 12 && x.fechaevento.Year == anio)
                                .Count()
                            }).ToList();

                    if (buscarMatriculas.Count == 0)
                        buscarMatriculas = context.vw_matriculas.GroupBy(x => x.Asesor).Select(grupo => new
                            {
                            enero = 0,
                            febrero = 0,
                            marzo = 0,
                            abril = 0,
                            mayo = 0,
                            junio = 0,
                            julio = 0,
                            agosto = 0,
                            septiembre = 0,
                            octubre = 0,
                            noviembre = 0,
                            diciembre = 0
                            }).ToList();

                    var nombre = context.users.Where(x => x.user_id == a)
                        .Select(x => new { nom = x.user_nombre + " " + x.user_apellido }).FirstOrDefault();

                    listaConsulta.Add(new ConsultaMatriculasModel
                        {
                        nombre = nombre.nom,
                        enero = buscarMatriculas.Select(x => x.enero).FirstOrDefault(),
                        febrero = buscarMatriculas.Select(x => x.febrero).FirstOrDefault(),
                        marzo = buscarMatriculas.Select(x => x.marzo).FirstOrDefault(),
                        abril = buscarMatriculas.Select(x => x.abril).FirstOrDefault(),
                        mayo = buscarMatriculas.Select(x => x.mayo).FirstOrDefault(),
                        junio = buscarMatriculas.Select(x => x.junio).FirstOrDefault(),
                        julio = buscarMatriculas.Select(x => x.julio).FirstOrDefault(),
                        agosto = buscarMatriculas.Select(x => x.agosto).FirstOrDefault(),
                        septiembre = buscarMatriculas.Select(x => x.septiembre).FirstOrDefault(),
                        octubre = buscarMatriculas.Select(x => x.octubre).FirstOrDefault(),
                        noviembre = buscarMatriculas.Select(x => x.noviembre).FirstOrDefault(),
                        diciembre = buscarMatriculas.Select(x => x.diciembre).FirstOrDefault()
                        });
                    }

                return Json(new { listaConsulta, coordinador = true }, JsonRequestBehavior.AllowGet);
                }

            #endregion

            #region director

            if (rol == 2034)
                {
                var buscarBodegas = context.bodega_usuario.Where(x => x.id_usuario == usuario).Select(x => x.id_bodega)
                    .ToList();
                foreach (var a in buscarBodegas)
                    {
                    var buscarMatriculas = context.vw_matriculas.Where(x => x.id_bodega == a)
                        .GroupBy(x => x.bodccs_nombre).Select(grupo => new
                            {
                            enero = grupo.Where(x => x.fechaevento.Month == 1 && x.fechaevento.Year == anio).Count(),
                            febrero = grupo.Where(x => x.fechaevento.Month == 2 && x.fechaevento.Year == anio).Count(),
                            marzo = grupo.Where(x => x.fechaevento.Month == 3 && x.fechaevento.Year == anio).Count(),
                            abril = grupo.Where(x => x.fechaevento.Month == 4 && x.fechaevento.Year == anio).Count(),
                            mayo = grupo.Where(x => x.fechaevento.Month == 5 && x.fechaevento.Year == anio).Count(),
                            junio = grupo.Where(x => x.fechaevento.Month == 6 && x.fechaevento.Year == anio).Count(),
                            julio = grupo.Where(x => x.fechaevento.Month == 7 && x.fechaevento.Year == anio).Count(),
                            agosto = grupo.Where(x => x.fechaevento.Month == 8 && x.fechaevento.Year == anio).Count(),
                            septiembre = grupo.Where(x => x.fechaevento.Month == 9 && x.fechaevento.Year == anio)
                                .Count(),
                            octubre = grupo.Where(x => x.fechaevento.Month == 10 && x.fechaevento.Year == anio).Count(),
                            noviembre = grupo.Where(x => x.fechaevento.Month == 11 && x.fechaevento.Year == anio)
                                .Count(),
                            diciembre = grupo.Where(x => x.fechaevento.Month == 12 && x.fechaevento.Year == anio)
                                .Count()
                            }).ToList();

                    if (buscarMatriculas.Count == 0)
                        buscarMatriculas = context.vw_matriculas.GroupBy(x => x.bodccs_nombre).Select(grupo => new
                            {
                            enero = 0,
                            febrero = 0,
                            marzo = 0,
                            abril = 0,
                            mayo = 0,
                            junio = 0,
                            julio = 0,
                            agosto = 0,
                            septiembre = 0,
                            octubre = 0,
                            noviembre = 0,
                            diciembre = 0
                            }).ToList();

                    var nombre = context.bodega_concesionario.Where(x => x.id == a)
                        .Select(x => new { nom = x.bodccs_nombre }).FirstOrDefault();

                    listaConsulta.Add(new ConsultaMatriculasModel
                        {
                        nombre = nombre.nom,
                        enero = buscarMatriculas.Select(x => x.enero).FirstOrDefault(),
                        febrero = buscarMatriculas.Select(x => x.febrero).FirstOrDefault(),
                        marzo = buscarMatriculas.Select(x => x.marzo).FirstOrDefault(),
                        abril = buscarMatriculas.Select(x => x.abril).FirstOrDefault(),
                        mayo = buscarMatriculas.Select(x => x.mayo).FirstOrDefault(),
                        junio = buscarMatriculas.Select(x => x.junio).FirstOrDefault(),
                        julio = buscarMatriculas.Select(x => x.julio).FirstOrDefault(),
                        agosto = buscarMatriculas.Select(x => x.agosto).FirstOrDefault(),
                        septiembre = buscarMatriculas.Select(x => x.septiembre).FirstOrDefault(),
                        octubre = buscarMatriculas.Select(x => x.octubre).FirstOrDefault(),
                        noviembre = buscarMatriculas.Select(x => x.noviembre).FirstOrDefault(),
                        diciembre = buscarMatriculas.Select(x => x.diciembre).FirstOrDefault()
                        });
                    }

                return Json(new { listaConsulta, director = true }, JsonRequestBehavior.AllowGet);
                }

            #endregion

            return Json(0);
            }

        public JsonResult graficoTorta(DateTime fechaInicio, DateTime fechaFin, int[] bodegas)
            {
            var predicate = PredicateBuilder.True<vw_ventas_repuestos>();
            var bodegasPredicate = PredicateBuilder.False<vw_ventas_repuestos>();

            if (fechaInicio <= fechaFin) predicate = predicate.And(x => x.fecha >= fechaInicio && x.fecha <= fechaFin);
            if (bodegas[0] != 0)
                {
                foreach (var item in bodegas) bodegasPredicate = bodegasPredicate.Or(x => x.bodega == item);

                predicate = predicate.And(bodegasPredicate);
                }

            var data2 = context.vw_ventas_repuestos.Where(predicate).GroupBy(x => new
                { x.bodccs_nombre, x.bodega }).Select(grp => new
                    {
                    cantidad = grp.Count(),
                    nombreBodega = grp.Key.bodccs_nombre,
                    valor = grp.Sum(x => x.valor),
                    id = grp.Key.bodega
                    }).ToList();

            var data = data2.Select(x => new
                {
                x.valor,
                x.nombreBodega,
                x.id
                });

            return Json(data, JsonRequestBehavior.AllowGet);
            }

        public JsonResult graficoKardex(int anio, int mes, int?[] bodegas, string referencias)
            {
            var predicate = PredicateBuilder.True<vw_kardex2>();
            var bodega = PredicateBuilder.False<vw_kardex2>();
            var referencia = PredicateBuilder.False<vw_kardex2>();

            predicate = predicate.And(x => x.ano == anio && x.mes == mes && x.modulo == "R");
            if (bodegas.Count() > 0 && bodegas[0] != null)
                {
                foreach (int item in bodegas) bodega = bodega.Or(x => x.bodega == item);
                predicate = predicate.And(bodega);
                }

            if (referencias != null && referencias != "")
                {
                referencia = referencia.Or(x => x.codigo == referencias);

                predicate = predicate.And(referencia);
                }

            var datos = context.vw_kardex2.Where(predicate).GroupBy(x => new { x.bodega, x.bodccs_nombre }).Select(a =>
                  new
                      {
                      a.Key.bodccs_nombre,
                      a.Key.bodega,
                      costoStock = a.Sum(x => x.CostoStock)
                      }).ToList();

            var data = datos.Select(x => new
                {
                bodega = x.bodccs_nombre,
                id = x.bodega,
                costoStock = x.costoStock < 0 ? 0 : x.costoStock
                }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
            }

        public JsonResult tortaClasificacionABC()
            {
            var buscar = context.vw_inventario_hoy.Where(x => x.stock > 0 && x.modulo == "R")
                .GroupBy(x => new { x.clasificacion_ABC }).Select(grp => new
                    {
                    id = grp.Key.clasificacion_ABC != null ? grp.Key.clasificacion_ABC : "Sin Clasificación",
                    referencia = grp.Select(x => x.ref_codigo),
                    cantidad = grp.Select(x => x.ref_codigo).Count()
                    }).ToList();


            return Json(buscar, JsonRequestBehavior.AllowGet);
            }

        public JsonResult tortaInventarioBodega()
            {
            var buscar = context.vw_inventario_bodega
                .GroupBy(x => new { x.bodega, x.bodccs_nombre }).Select(grp => new
                    {
                    id = grp.Select(x => x.bodega),
                    bodega = grp.Select(x => x.bodccs_nombre),
                    totalCosto = grp.Select(x => x.totalCosto)
                    }).ToList();


            return Json(buscar, JsonRequestBehavior.AllowGet);
            }


        public JsonResult tortaVentasBodega(int? bodega, int? anio, int? mes)
            {
            var buscar = context.vw_ventas_repuestosGrafica.Where(x => x.bodega == bodega || x.ano == anio || x.mes == mes)
            .GroupBy(x => x.bodega).Select(grp => new ventasbodegas
                {
                id = grp.Key,
                bodccs_nombre = grp.Select(x => x.bodccs_nombre).FirstOrDefault(),
                total_ventas = grp.Sum(x => x.total_ventas),
                participacion = grp.Sum(x => x.Participacion),
                }).ToList();
            foreach (var item in buscar)
                {
                item.participacion = Redondear(item.participacion);
                }
            return Json(buscar, JsonRequestBehavior.AllowGet);
            }
        public decimal Redondear(decimal? cifra)
            {
            decimal resultado = 0;

            if (cifra != null)
                {
                var cifraString = cifra.Value.ToString("N2", new CultureInfo("is-IS"));
                resultado = Convert.ToDecimal(cifraString, new CultureInfo("is-IS"));
                }

            return resultado;
            }

        public JsonResult tortaVentas(int? bodega, int? anio, int? mes)//*******************
            {
            Expression<Func<vw_ventas_repuestosGrafica, bool>> predicado = PredicateBuilder.True<vw_ventas_repuestosGrafica>();

            if (bodega != null)
                {
                predicado = predicado.And(d => d.bodega == bodega);
                }

            if (anio != null)
                {
                predicado = predicado.And(d => d.ano == anio);
                }

            if (mes != null)
                {
                predicado = predicado.And(d => d.mes == mes);
                }

            var buscar = context.vw_ventas_repuestosGrafica.Where(predicado)
            .GroupBy(x => x.bodega).Select(grp => new ventasbodegas
                {
                id = grp.Key,
                bodccs_nombre = grp.Select(x => x.bodccs_nombre).FirstOrDefault(),
                total_ventas = grp.Sum(x => x.total_ventas),
                participacion = grp.Sum(x => x.Participacion),
                }).ToList();
            foreach (var item in buscar)
                {
                item.participacion = Redondear(item.participacion);
                }

            return Json(buscar, JsonRequestBehavior.AllowGet);

            }
        public JsonResult buscarVentasXBodega(int? id, int? anio, int? mes)
            {
            if (anio != null && mes != null && id != null)
                {
                var buscar = context.vw_ventas_repuestosGrafica
                .Where(x => x.bodega == id && x.ano == anio.Value && x.mes == mes.Value)
                .GroupBy(x => x.bodega)
                .Select(grp => new
                    {
                    id = grp.Key,
                    bodccs_nombre = grp.Select(x => x.bodccs_nombre).FirstOrDefault(),
                    total_ventas = grp.Sum(x => x.total_ventas),
                    numero_ventas = grp.Sum(x => x.numero_ventas),
                    participacion = grp.Sum(x => x.Participacion),
                    ano = grp.Select(x => x.ano).FirstOrDefault(),
                    mes = grp.Select(x => x.mes).FirstOrDefault(),
                    }).ToList();
                return Json(buscar, JsonRequestBehavior.AllowGet);
                }
            else
                {
                return Json(0, JsonRequestBehavior.AllowGet);
                }
            }
        public JsonResult buscarClasificadosXBodega(string id)
            {
            if (id == "Sin Clasificación") id = null;
            var buscar = context.vw_inventario_hoy
                .Where(x => x.stock > 0 && x.clasificacion_ABC == id && x.modulo == "R").GroupBy(x => x.nombreBodega)
                .Select(grp => new
                    {
                    id = grp.Select(x => x.bodega).FirstOrDefault(),
                    nombre = grp.Key,
                    cantidad = grp.Select(x => x.ref_codigo).Count(),
                    clasificacion = id
                    }).ToList();

            return Json(buscar, JsonRequestBehavior.AllowGet);
            }

        public JsonResult buscarInventarioXBodega(int id)
            {
            //if (id == "Sin Clasificación") id = null;
            var buscar = context.vw_inventario_bodega
                .Where(x => x.bodega == id).GroupBy(x => new { x.bodega, x.bodccs_nombre })
                .Select(grp => new
                    {
                    bodega = grp.Select(x => x.bodega),
                    bodccs_nombre = grp.Select(x => x.bodccs_nombre),
                    totalCosto = grp.Select(x => x.totalCosto),
                    }).ToList();

            return Json(buscar, JsonRequestBehavior.AllowGet);
            }

        public JsonResult buscarReferencias(int idBodega, string clasificacion)
            {
            if (clasificacion == "Sin Clasificación" || clasificacion == "null") clasificacion = null;
            var buscar = context.vw_inventario_hoy.Where(x => x.bodega == idBodega &&
                x.stock > 0 && x.clasificacion_ABC == clasificacion && x.modulo == "R").Select(x => new
                    {
                    x.ref_codigo,
                    x.ref_descripcion,
                    clasificacion_ABC = x.clasificacion_ABC != null ? x.clasificacion_ABC : "Sin Clasificación",
                    x.stock
                    }).ToList();

            return Json(buscar, JsonRequestBehavior.AllowGet);
            }

        public string colorfunel(int? numero)
            {
            var color = "ffffff";
            if (numero < 25)
                color = "#D60C0C";
            else if (numero > 25 && numero < 50)
                color = "#F66D04";
            else if (numero >= 50 && numero < 75)
                color = "#F9DE1A";
            else if (numero > 75) color = "#04AF1D";

            return color;
            }


        public ActionResult graficoClasificacionRepuestos() {



            return View();
            }


        public JsonResult consultaDatosClasi(int ano, int mes, int[] bodega) {



            var buscar = context.vw_Clasificacion_rto.Where(x => x.ano == ano && x.mes == mes && bodega.Contains(x.bodega))
          .GroupBy(x => new { x.clarpto_nombre }).Select(grp => new
              {
              id = grp.Key.clarpto_nombre != null ? grp.Key.clarpto_nombre : "Sin Clasificación",
              referencia = grp.Select(x => x.ref_codigo),
              cantidad = grp.Select(x => x.ref_codigo).Count()
              }).ToList();

            return Json(buscar, JsonRequestBehavior.AllowGet);
            }


        public JsonResult BuscarReferenciasClasi(int ano, int mes, int[] bodega, string clasificacion) {

            if (clasificacion == "") clasificacion = null;


            var dato = context.vw_Clasificacion_rto
                .Where(x => x.ano == ano && x.mes == mes && bodega.Contains(x.bodega) && x.clarpto_nombre == clasificacion).ToList();


            return Json(dato, JsonRequestBehavior.AllowGet);
            }




        public ActionResult EdadesInventario()
            {
            return View();
            }


        public ActionResult GraficoNivelServicio() {

            return View();
            }


        public JsonResult BuscarNivelServicio(string fechainicio, string fechafin, int[] bodega) {

            DateTime fini = Convert.ToDateTime(fechainicio, new CultureInfo("en-US"));
            DateTime ffin = Convert.ToDateTime(fechafin, new CultureInfo("en-US"));
            ffin = ffin.AddDays(1);
            var solicitudes = context.vw_Solcitues_bodega.Where(x => x.fecha_creacion >= fini && x.fecha_creacion <= ffin && bodega.Contains(x.bodega)).ToList();
            var solicitados2 = solicitudes.GroupBy(d => d.bodega).Select(e => new Solicitados { bodega = e.Key, cantidad = Convert.ToDecimal(e.Sum(f => f.cantidad), miCultura) }).ToList();


            var vendidos = context.vw_Cantidades_Bodega.Where(x => x.fecha_creacion >= fini && x.fecha_creacion <= ffin && bodega.Contains(x.bodega)).ToList();
            var vendidos2 = vendidos.GroupBy(d => d.bodega).Select(e => new Participacion { bodega = e.Key, nombre = e.Select(f => f.bodccs_nombre).FirstOrDefault(), cantidad = Convert.ToDecimal(e.Sum(f => f.suma), miCultura) }).ToList();

            var cantidadtotal = vendidos2.Sum(d => d.cantidad);
            foreach (var item in vendidos2)
                {
                var cantidadsolicitada = solicitados2.Where(d => d.bodega == item.bodega).Select(d => d.cantidad).FirstOrDefault();
                item.participacion = calcularParticipacion(item.cantidad, cantidadsolicitada, cantidadtotal);
                }

            var data = new
                {
                vendidos2,
                cantidadtotal
                };

            return Json(data, JsonRequestBehavior.AllowGet);
            }


        public decimal calcularParticipacion(decimal ventasbodega, decimal solicitadosbodega, decimal ventastotales) {


            decimal resultado = 0;
            decimal denominador = 0;
            denominador = solicitadosbodega + ventastotales;
            if (denominador != 0)
                {
                resultado = (ventasbodega / denominador) * 100;
                }

            return resultado;
            }


        public JsonResult DatosNivelServicioBod(string fechainicio, string fechafin, int bodega) {
            DateTime fini = Convert.ToDateTime(fechainicio, new CultureInfo("en-US"));
            DateTime ffin = Convert.ToDateTime(fechafin, new CultureInfo("en-US"));
            var solicitudes = context.vw_Solcitues_bodega.Where(x => x.fecha_creacion >= fini && x.fecha_creacion <= ffin && x.bodega == bodega).ToList();
            var solicitados2 = solicitudes.GroupBy(d => d.bodega).Select(e => new Solicitados { bodega = e.Key, cantidad = Convert.ToDecimal(e.Sum(f => f.cantidad), miCultura) }).ToList();


            var cantidadsolicitada = solicitados2.Where(d => d.bodega == bodega).Select(d => d.cantidad).FirstOrDefault();
            var vendidos = context.vw_Cantidades_Bodega.Where(x => x.fecha_creacion >= fini && x.fecha_creacion <= ffin && x.bodega == bodega).ToList();
            var vendidos2 = vendidos.GroupBy(d => d.bodega).Select(e => new Participacion { bodega = e.Key, nombre = e.Select(f => f.bodccs_nombre).FirstOrDefault(), cantidad = Convert.ToDecimal(e.Sum(f => f.suma), miCultura) }).ToList();

            var cantVendida = vendidos2.Select(f => f.cantidad).FirstOrDefault();
            var nombre_bodega = vendidos2.Select(f => f.nombre).FirstOrDefault();

            var data = new {
                nombre_bodega,
                cantVendida,
                cantidadsolicitada
                };

            return Json(data, JsonRequestBehavior.AllowGet);
            }



        public ActionResult graficoRotacionMercancia() {

            return View();
            }



        public JsonResult graficoRotacion(int anio, int mes, int[] bodegasin) {
            
            List<BodegaRotacion> ListaBodegasRotacion = new List<BodegaRotacion>();
            var Costo_prom = context.vw_costo_mes_bodega.Where(x => x.año == anio && x.mes == mes && bodegasin.Contains(x.bodega)).
                Select(f => new { f.Costo_Total, f.bodega, f.bodccs_nombre }).ToList();

            var Costo_prom2 = Costo_prom.GroupBy(d => d.bodega).Select(e => new BodegaRotacion { idbodega = e.Key, indicador = Convert.ToDecimal(e.Sum(f => f.Costo_Total), miCultura) }).ToList();

            var valor_invent = context.vw_valor_inventario_bodega.Where(d => d.ano == anio && d.mes == mes && bodegasin.Contains(d.bodega)).
               Select(a => new { a.Total, a.bodega, a.bodccs_nombre }).ToList();


            for (int i = 0; i < Costo_prom2.Count; i++)
                {
                for (int j = 0; j < valor_invent.Count; j++)
                    {


                    if (Costo_prom[i].bodega == valor_invent[j].bodega)
                        {
                        if (valor_invent[j].Total != 0)
                            {
                            BodegaRotacion bodrotacion = new BodegaRotacion();
                            bodrotacion.indicador = (Convert.ToDecimal(Costo_prom2[i].indicador, miCultura) * 12) / Convert.ToDecimal(valor_invent[j].Total, miCultura);
                            bodrotacion.idbodega = Costo_prom[i].bodega;
                            bodrotacion.nombre_bodega = Costo_prom[i].bodccs_nombre;
                            ListaBodegasRotacion.Add(bodrotacion);
                            }


                        }

                    }


                }

            return Json(ListaBodegasRotacion, JsonRequestBehavior.AllowGet);
            }

        public JsonResult Datosrotacion(int anio, int bodega) {

            var Costo_prom = context.vw_costo_mes_bodega.Where(x => x.año == anio && x.bodega == bodega).
              Select(f => new { f.Costo_Total, f.bodega, f.bodccs_nombre }).ToList();

            var Costo_prom2 = Costo_prom.GroupBy(d => d.bodega).Select(e => new BodegaRotacion { idbodega = e.Key, indicador = Convert.ToDecimal(e.Sum(f => f.Costo_Total), miCultura) }).ToList();

            var valor = context.vw_valor_inventario_bodega.Where(x => x.ano == anio && x.bodega == bodega).
                  Select(a => new { a.Total, a.bodega, a.bodccs_nombre, a.mes }).ToList();

            List<BodegaRotacion> ListaBodegasRotacion = new List<BodegaRotacion>();

            for (int i = 0; i < Costo_prom2.Count; i++)
                {
                if (valor[i].Total != 0)
                    {

                    BodegaRotacion bodrotacion = new BodegaRotacion();
                    bodrotacion.indicador = (Convert.ToDecimal(Costo_prom2[i].indicador, miCultura) * 12) / Convert.ToDecimal(valor[i].Total, miCultura);
                    bodrotacion.idbodega = Costo_prom[i].bodega;
                    bodrotacion.nombre_bodega = Costo_prom[i].bodccs_nombre;
                    ListaBodegasRotacion.Add(bodrotacion);

                    }



                }

            var data = new
                {
                Costo_prom2,
                ListaBodegasRotacion,
                valor

                };



            return Json(data, JsonRequestBehavior.AllowGet);
            }




        public ActionResult GraficoLealtad() {

            return View();
            }



        public JsonResult ConsultaGraficoLealtad(string fechainicio, string fechafin, int[] bodegas) {

            DateTime fini = Convert.ToDateTime(fechainicio);
            DateTime ffin = Convert.ToDateTime(fechafin);
            var data = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fini && x.fec_creacion <= ffin && bodegas.Contains(x.bodega)).
                Select(s => new { s.cantidadref, s.descripcion, s.bodega, s.bodccs_nombre, s.Clasificacion_pro, s.nit }).ToList();

            var data2 = data.GroupBy(x => new { x.descripcion, x.Clasificacion_pro, x.bodega }).Select(c => new { cantidad = c.Sum(f => f.cantidadref), Preovedor = c.Key.descripcion, idbodega = c.Key.bodega, idclasipro = c.Key.Clasificacion_pro }).ToList();




            return Json(data2, JsonRequestBehavior.AllowGet);
            }





        public JsonResult Datostablalealtad(string fechainicio, string fechafin, int[] bodegas) {


            DateTime fini = Convert.ToDateTime(fechainicio);
            DateTime ffin = Convert.ToDateTime(fechafin);
            var data = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fini && x.fec_creacion <= ffin && bodegas.Contains(x.bodega) && x.Clasificacion_pro == 1).
                Select(s => new { s.cantidadref, s.Nombre_proveedor, s.bodega, s.bodccs_nombre, s.nit, s.valor_total }).ToList();

            var datat = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fini && x.fec_creacion <= ffin && bodegas.Contains(x.bodega)).
              Select(s => new { s.valor_total }).ToList();

            var dataneto = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fini && x.fec_creacion <= ffin && bodegas.Contains(x.bodega) && x.Clasificacion_pro == 1 && x.Clasificacion_pro == 2).
             Select(s => new { s.cantidadref, s.Nombre_proveedor, s.bodega, s.bodccs_nombre, s.nit, s.valor_total }).ToList();

            List<decimal> porBruto = new List<decimal>();
            List<decimal> porneto = new List<decimal>();


            var cantidadTotal = data.GroupBy(x => new { x.bodega, x.Nombre_proveedor }).Select(x => x.Sum(f => f.cantidadref)).ToList();

            var nombreCon = data.GroupBy(x => new { x.bodega, x.Nombre_proveedor, x.bodccs_nombre }).Select(x => new { x.Key.Nombre_proveedor, x.Key.bodccs_nombre }).ToList();

            var costocom_pro = data.GroupBy(x => new { x.bodega, x.Nombre_proveedor }).Select(x => x.Sum(f => f.valor_total)).ToList();

            var valortotal = datat.Sum(x => x.valor_total);
            var valorgmmasred = dataneto.GroupBy(x => new { x.bodega, x.Nombre_proveedor }).Select(x => x.Sum(f => f.valor_total)).ToList();

            for (int i = 0; i < nombreCon.Count; i++)
                {

                decimal numero = (Convert.ToInt32(costocom_pro[i]) / Convert.ToInt32(valortotal))*100;
                porBruto.Add(numero);
                decimal numero1 = (Convert.ToInt32(valorgmmasred[i]) / Convert.ToInt32(valortotal))*100;
                porneto.Add(numero1);

                }


            var Datos = new {

                nombreCon,
                cantidadTotal,
                porBruto,
                porneto,
                valortotal
                };

            return Json(Datos, JsonRequestBehavior.AllowGet);

            }




        public ActionResult indicadoresGenerales() {
            int rol_id = Convert.ToInt32(Session["user_rolid"]);
            ViewBag.nombreVergeneral = (from r in context.rolacceso
                                 join rp in context.rolpermisos on r.idpermiso equals rp.id                                 
                                 where r.idrol == rol_id && rp.codigo == "P43" select rp.codigo).FirstOrDefault();


            ViewBag.nombreVergenbodega = (from r in context.rolacceso
                                        join rp in context.rolpermisos on r.idpermiso equals rp.id
                                        where r.idrol == rol_id && rp.codigo == "P44"
                                        select rp.codigo).FirstOrDefault();
            return View();
            }



        public JsonResult DatosIndicadoresGeneral()
            {
            var año = DateTime.Now.Year;
            var Datos = context.indicadores_generales.OrderBy(c => c.Año).ThenBy(x => x.mes).Where(x=> x.Año== año).ToList();


            return Json(Datos, JsonRequestBehavior.AllowGet);
            }


        public JsonResult DatosIndicadoresGenBodega( int?[] bodegas)
            {
            var año = DateTime.Now.Year;

            var Datosb = context.indicadores_gen_Bodega.OrderBy(c=> c.Año).ThenBy(x=> x.mes).Where(x => x.Año == año  && bodegas.Contains(x.bodega)).ToList();

            var DatosF = Datosb.Select(x => new { 
                Anio = x.Año,
                mesbo = x.mes,
                nombodega= x.bodega_concesionario.bodccs_nombre.ToString(),
                x.inventario_inicial,
                x.compras_gm_ingresis,
                x.compras_concesionarios,
                x.compras_otros_repuestos,
               compras_ot_acce =  x.compras_otros_accesorios,
                x.total_compras,
                x.total_ventas,
                x.ajustes_inventario,
                x.costo_venta,
                x.inventario_final,
                diferencia = x.dif_inventario_enero,
                x.inv_proceso,
                x.inv_disponible,
                x.inv_final,
                x.variacion_inventario,
                x.rotacion_inven,
                x.lealtad_bruta,
                x.lealtad_neta,
                x.lealtad_neta_sin_acceso,
                x.margen_bruto_sin_incent,
                x.margen_bruto_con_incent,
                x.fof,
                x.obsolencia
                
                });



            return Json(DatosF, JsonRequestBehavior.AllowGet);
            }
        }
    
    }