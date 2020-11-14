using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class solicitudRepuestosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        private readonly CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
        private CultureInfo culturaInfo = new CultureInfo("is-IS");

        // GET: solicitudRepuestos
        public ActionResult Create(int? menu, string id, int? ot, string cadena = "")
        {
            rsolicitudesrepuestos mode = new rsolicitudesrepuestos();
            /* Traer datos de la OT */
            if (ot != null && id != null)
            {
                ViewBag.ot = ot;
                var datosOT = (
                    from t in context.tencabezaorden
                    join d in context.tsolicitudrepuestosot
                        on t.id equals d.idorden
                    where t.id == ot && !d.pedido && d.idrepuesto == id
                    select new
                    {
                        t.tercero,
                        t.bodega,
                        d.cantidad,
                        d.idorden
                    }).FirstOrDefault();
                mode.bodega = datosOT.bodega;
                mode.cliente = datosOT.tercero;
                mode.id_ot = datosOT.idorden;
                ViewBag.cantidadInput = datosOT.cantidad;
            }
            else
            {
                ViewBag.ot = "";
                ViewBag.cantidadInput = "";
            }
            /* *** */



            ListasDesplegables(mode);

            if (!string.IsNullOrWhiteSpace(id))
            {
                //busco si el codigo corresponde a una referencia
                icb_referencia refer = context.icb_referencia.Where(d => d.ref_codigo == id).FirstOrDefault();
                if (refer != null)
                {
                    ViewBag.codigo = refer.ref_codigo + " | " + refer.ref_descripcion;
                }
            }

            BuscarFavoritos(menu);


            if (cadena != "")
            {
                ViewBag.cadena = cadena;
                var Subcadena = cadena.Split(',');
                int dataBogeda = Convert.ToInt32(Subcadena[0]);
                int dataCliente = Convert.ToInt32(Subcadena[1]);
                string idreferencia = Subcadena[2];


                ViewBag.Id_bodega_origen = dataBogeda;
                ViewBag.CodReferencia = idreferencia;
                ViewBag.Cliente1 = dataCliente;

                return View();
            }

            return View();
        }
        
        
        public JsonResult createSolicitud(tempSolicitud tempSolicitud)
        {
            var data = 0;

            tempSolicitud.dataBogeda = tempSolicitud.dataBogeda;
            tempSolicitud.dataCliente = tempSolicitud.dataCliente;
            tempSolicitud.idreferencia = tempSolicitud.idreferencia;
            tempSolicitud.idkitaccesorios = tempSolicitud.idkitaccesorios; 

            context.tempSolicitud.Add(tempSolicitud);
            int result = context.SaveChanges();

            if (result == 1)
            {
                data = 1;
            }
            else
            {
                data = 0;
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }



        // POST: solicitudRepuestos
        [HttpPost]
        public ActionResult Create(rsolicitudesrepuestos modelo, int? menu, string ot)
        {
            if (ModelState.IsValid)
            {
                var idorden = 0;

                bool convertir1 = int.TryParse(ot, out int num_ot);
                if (convertir1 == true)
                {
                    idorden = num_ot;
                }



                tdetallerepuestosot orden = context.tdetallerepuestosot.Where(d => d.idorden == idorden).FirstOrDefault();

                int contador = Convert.ToInt32(Request["listaReferencias"]);

                if (contador > 0)
                {
                    modelo.fecha = DateTime.Now;
                    modelo.usuario = Convert.ToInt32(Session["user_usuarioid"]);
                    modelo.estado_solicitud = 1;
                    if (!string.IsNullOrWhiteSpace(ot))
                    {
                        bool convertir = int.TryParse(ot, out int numot);
                        if (convertir == true)
                        {
                            modelo.id_ot = numot;
                        }
                    }

                    if (idorden > 0)
                    {
                        orden.estadorepuesto = 3;
                        context.Entry(orden).State = EntityState.Modified;
                        context.SaveChanges();
                    }

                    context.rsolicitudesrepuestos.Add(modelo);
                    context.SaveChanges();

                    for (int i = 0; i < contador; i++)
                    {
                        string codigo = Request["codigoTabla" + i];
                        if (codigo != "")
                        {
                            rdetallesolicitud detalle = new rdetallesolicitud
                            {
                                id_solicitud = modelo.id,
                                referencia = Request["codigoTabla" + i],
                                cantidad = Convert.ToInt32(Request["cantidadTabla" + i]),
                                iva = Convert.ToInt32(Request["ivaTabla" + i]),
                                valor = Convert.ToDecimal(Request["precioTabla" + i], culturaInfo),
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                fecha_creacion = DateTime.Now,
                                esta_pedido = 1,

                            };
                            context.rdetallesolicitud.Add(detalle);
                        }
                    }

                    context.SaveChanges();
                    TempData["mensaje"] = "La solicitud se ha agregado exitosamente";
                }
                else
                {
                    TempData["mensaje_error"] = "Ingrese al menos una referencia";
                }
            }

            ListasDesplegables(new rsolicitudesrepuestos());
            var referencias = context.icb_referencia.Where(x => x.ref_estado && x.modulo == "R").Select(x => new
            {
                codigo = x.ref_codigo,
                descripcion = x.ref_codigo + " - " + x.ref_descripcion
            }).OrderBy(x => x.descripcion).ToList();
            //ViewBag.codigo = new SelectList(referencias, "codigo", "descripcion");
            BuscarFavoritos(menu);
            return View();
        }



        public JsonResult DatosPorPlaca(string placa)
        {

            if (!string.IsNullOrWhiteSpace(placa))
            {

                var data = (from ve in context.icb_vehiculo
                            join mo in context.modelo_vehiculo on
                           ve.modvh_id equals mo.modvh_codigo
                            join te in context.icb_terceros on
                            ve.propietario equals te.tercero_id
                            where ve.plac_vh == placa

                            select new
                            {
                                mo.modvh_nombre,
                                ve.propietario,
                                ve.plac_vh,
                                cedula = te.doc_tercero,

                                nombre = te.prinom_tercero + " " + te.segnom_tercero + " " + te.apellido_tercero + " " + te.segapellido_tercero
                            }).ToList();


                var selectdata = data.Select(x => new
                {
                    x.modvh_nombre,
                    x.nombre,
                    x.cedula,
                    x.propietario,
                    placa = x.plac_vh,

                }).FirstOrDefault();

                return Json(selectdata, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);

        }

        public JsonResult DatosPorCedula(string idcliente)
        {

            int numero = 0;

            if (!string.IsNullOrWhiteSpace(idcliente))
            {
                var convertirx = int.TryParse(idcliente, out numero);


                var data = (from ve in context.icb_vehiculo
                            join mo in context.modelo_vehiculo on
                           ve.modvh_id equals mo.modvh_codigo
                            join te in context.icb_terceros on
                            ve.propietario equals te.tercero_id
                            where ve.propietario == numero

                            select new
                            {
                                mo.modvh_nombre,
                                ve.propietario,
                                ve.plac_vh,
                                cedula = te.doc_tercero,
                                ve.plan_mayor,
                                nombre = te.prinom_tercero + " " + te.segnom_tercero + " " + te.apellido_tercero + " " + te.segapellido_tercero
                            }


                            ).ToList();
                var selectdata = data.Select(x =>
               new
               {
                   x.modvh_nombre,
                   x.nombre,
                   x.cedula,
                   x.propietario,
                   x.plan_mayor,
                   placa = x.plac_vh

               }).FirstOrDefault();

                return Json(selectdata, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult validarTraslado(string idreferencia1, int dataCliente)
        {
            var result = 0;

            var data = (from solicitud in context.Solicitud_traslado
                        join bodegaorigen in context.bodega_concesionario on solicitud.Id_bodega_origen equals bodegaorigen.id
                        join bodegadestino in context.bodega_concesionario on solicitud.Id_bodega_destino equals bodegadestino.id
                        where solicitud.Estado_atendido == 3 
                        select new
                        {
                            solicitud.Id,
                            solicitud.Fecha_creacion,
                            origen = bodegaorigen.bodccs_nombre,
                            destino = bodegadestino.bodccs_nombre,
                            referencias = (from Detalle in context.Detalle_Solicitud_Traslado
                                           join referencia in context.icb_referencia
                                           on Detalle.Cod_referencia.ToString() equals referencia.ref_codigo
                                           where Detalle.Id_Solicitud_Traslado == solicitud.Id && referencia.ref_codigo== idreferencia1
                                           select new
                                           {
                                               id1= referencia.ref_codigo,
                                               //referencia.ref_descripcion
                                           }).Distinct().ToList()
                        }).OrderByDescending(x => x.referencias).Count();

            if (data <= 0)
            {
                result = 0;
            }
            else
            {
                result = 1;
            }



            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult validarSolicitudRepuestos(string dataCliente1, string idreferencia1)
        {
            var result = 0;
            var buscarSolicitudes = (from a in context.rsolicitudesrepuestos
                                     join b in context.bodega_concesionario//join
                                         on a.bodega equals b.id
                                     join c in context.icb_terceros//left
                                         on a.cliente equals c.tercero_id
                                     join es in context.restado_solicitud_Repuestos on
                                     a.estado_solicitud equals es.id_estado_solicitud
                                     join d in context.users//join
                                         on a.usuario equals d.user_id
                                     join e in context.rtiposolicitudrepuesto//join
                                         on a.tiposolicitud equals e.id
                                     join f in context.rdetallesolicitud//join
                                    on a.id equals f.id_solicitud
                                     join g in context.icb_referencia//join
                                     on f.referencia equals g.ref_codigo
                                     join m in context.tencabezaorden
                                     on a.id_ot equals m.id into orderit
                                     from m in orderit.DefaultIfEmpty()
                                     where c.doc_tercero == dataCliente1 && f.referencia== idreferencia1
                                     select new
                                     {
                                         id1 = a.id
                                     }).OrderByDescending(x => x.id1).Count();
            if (buscarSolicitudes <= 0)
            {
                result = 0;
            }
            else
            {
                result = 1;
            }



            return Json(result, JsonRequestBehavior.AllowGet);
        }



        public JsonResult BuscarSolicitudesRepuestos()
        {
            CultureInfo myCultura = new CultureInfo("is-IS");

            //var anticipos=0;
            var buscarSolicitudes = (from a in context.rsolicitudesrepuestos
                                     join b in context.bodega_concesionario//join
                                         on a.bodega equals b.id
                                     join c in context.icb_terceros//left
                                         on a.cliente equals c.tercero_id                                 
                                     join es in  context.restado_solicitud_Repuestos on
                                     a.estado_solicitud equals es.id_estado_solicitud
                                     join d in context.users//join
                                         on a.usuario equals d.user_id
                                     join e in context.rtiposolicitudrepuesto//join
                                         on a.tiposolicitud equals e.id
                                     join f in context.rdetallesolicitud//join
                                    on a.id equals f.id_solicitud
                                     join g in context.icb_referencia//join
                                     on f.referencia equals g.ref_codigo
                                     join m in context.tencabezaorden
                                     on a.id_ot equals m.id into  orderit
                                     from m in orderit.DefaultIfEmpty()
                                     join tc in context.rtipocompra
                                     on a.tipo_compra equals tc.id into tcp
                                     from tc in tcp.DefaultIfEmpty()
                                     where f.esta_pedido!=1
                                     select new
                                     {
                                         id_detalle = f.id,
                                         a.id,
                                         b.bodccs_nombre,
                                         a.fecha,
                                         clienteCedula = c.doc_tercero,
                                         clienteNombre = c.prinom_tercero + " " + c.segnom_tercero + " " + c.apellido_tercero + " " +
                                                           c.segapellido_tercero + " " + c.razon_social,
                                         a.cantidad,
                                         usuarioSolicitud = d.user_nombre + " " + d.user_apellido,
                                         a.Detalle,
                                         referenciasSolicitadas = f.referencia + "  " + g.ref_descripcion,
                                         g.clasificacion_ABC,
                                         cantidad_actual = (from refe in context.referencias_inven where refe.bodega == a.bodega && refe.codigo == f.referencia  orderby refe.ano descending, refe.mes descending select ((refe.can_ini + refe.can_ent) - refe.can_sal)).FirstOrDefault(),
                                         /*can_backoprder = 0,
                                         cantidad_recibida = 0,*/
                                         cantidad_pedida = f.cantidad,
                                         tipo_compra = tc.descripcion,
                                         costo_prom = (from refe in context.referencias_inven where refe.bodega == a.bodega && refe.codigo == f.referencia orderby refe.ano descending, refe.mes descending select refe.costo_prom).FirstOrDefault(),
                                         cantidad_clasificacion = 0,
                                         responsable = "",
                                         vehiculo = (from a in context.rsolicitudesrepuestos
                                                     join h in context.tencabezaorden
                                                        on a.id_ot equals h.id into tencab
                                                     from h in tencab.DefaultIfEmpty()
                                                     join i in context.icb_vehiculo
                                                       on h.placa equals i.plan_mayor
                                                     join j in context.modelo_vehiculo
                                                        on i.modvh_id equals j.modvh_codigo select new { modvh_nombre = j.modvh_nombre != null ? j.modvh_nombre :"" }).FirstOrDefault(),
                                         fecha_pedido = (from a in context.rsolicitudesrepuestos
                                                         join l in context.rseparacionmercancia
                                                         on a.separacion_consecutivo equals l.separacion select l.fecha).FirstOrDefault(),

                                         No_de_orden =  m != null ? m.numero: 0 ,
                                         No_de_anticipo = (from fa2 in context.rsolicitudesrepuestos //revision de el ticket 2597 correccion de los anticipos
                                                           join n in context.rseparacionmercancia
                                                           on fa2.separacion_consecutivo equals n.separacion
                                                           join nd in context.rseparacion_anticipo
                                                           on n.id equals nd.separacion_id
                                                           where
                                                           n.separacion == a.separacion_consecutivo
                                                           group nd by nd.anticipo_id into anticipo                                                           
                                                           select anticipo.Key
                                                             ),
                                        a.separacion_consecutivo,
                                       es.Descripcion

                                     }).ToList();


            var data = buscarSolicitudes.Select(x => new
            {
                x.id_detalle,
                x.id,
                x.bodccs_nombre,
                fecha = x.fecha.ToString("yyyy/MM/dd HH:mm", myCultura),
                clienteCedula = x.clienteCedula != null ? x.clienteCedula : "",
                clienteNombre = x.clienteNombre != null && x.clienteNombre != "" ? x.clienteNombre : "Pedido Sugerido",
                x.cantidad,
                x.usuarioSolicitud,
                Detalle = x.Detalle != null ? x.Detalle : "",
                x.referenciasSolicitadas,
                x.clasificacion_ABC,
                x.cantidad_actual,
                /*x.can_backoprder,
                x.cantidad_recibida,*/
                x.cantidad_pedida,
                tipo_compra = x.tipo_compra != null? x.tipo_compra.ToUpper() : "",
                x.costo_prom,
                x.cantidad_clasificacion,
                x.responsable,
                vehiculo = x.vehiculo.modvh_nombre,
                fecha_pedido =  x.fecha_pedido.ToString("yyyy/MM/dd HH:mm"),
                No_de_orden = x.No_de_orden,
                x.No_de_anticipo ,
                separacion_consecutivo = x.separacion_consecutivo != null ? x.separacion_consecutivo : 0,
              x.Descripcion
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult verificarSeleccionados(string[] seleccionados)
        {
            var listado = (from f in context.rdetallesolicitud
                           join a in context.rsolicitudesrepuestos
                            on f.id_solicitud equals a.id
                           where seleccionados.Contains(f.id.ToString())
                           select a.tipo_compra).ToList();
            var valido = !listado.Any(x => x != listado[0]);
            return Json(valido, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarObservacionesSol(int idSolicitud) {
            var buscardata = (from a in context.robservacion_solicitud
                              where a.id_solicitud_referencia == idSolicitud
                              select new { a.Observacion,
                                  a.fecha_creacion
                              }
                              ).ToList();

            var data = buscardata.Select(x => new { 
                fecha_creacion = x.fecha_creacion.Value.ToString("yyyy/MM/dd HH:mm"),
                x.Observacion
            });



            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GuardarObservacion(int Solicitud, string observacion) {


            robservacion_solicitud robservacion = new robservacion_solicitud();

            robservacion.fecha_creacion = DateTime.Now;
            robservacion.id_solicitud_referencia = Solicitud;
            robservacion.Observacion = observacion;
            robservacion.usuario_creacion = Convert.ToInt32(Session["user_usuarioid"]);

            context.robservacion_solicitud.Add(robservacion);
            bool result= context.SaveChanges()>0;

            if (result)
            {
                return Json(1, JsonRequestBehavior.AllowGet);
            }


            return Json(0, JsonRequestBehavior.AllowGet);

        }



        public JsonResult BuscarFiltros(int? Estadosol, string fechaini, string fechafin, int? ftipocompra, int? idbodega) {


            int bodega = Convert.ToInt32(Session["user_bodega"]);

            CultureInfo myCultura = new CultureInfo("is-IS");


            Expression<Func<vw_control_pedidos, bool>> predicado = PredicateBuilder.True<vw_control_pedidos>();


            if (Estadosol != null)
            {
                predicado = predicado.And(d => d.estado_solicitud == Estadosol);
            }

            if (ftipocompra != null)
            {
                predicado = predicado.And(d => d.idtipo_compra == ftipocompra);
            }

            if (idbodega != null)
            {
                predicado = predicado.And(d => d.idbodega == idbodega);
            }

            if (string.IsNullOrWhiteSpace(fechaini))
            {
                var fecha = DateTime.Now;
                var fecha2 = new DateTime(fecha.Year, fecha.Month, 1);
                    predicado = predicado.And(d => d.fecha >= fecha2);
            }
            else {
                var fecha = Convert.ToDateTime(fechaini);

                var convertir = DateTime.TryParse(fechaini, out fecha);
                if (convertir == true)
                {
                    predicado = predicado.And(d => d.fecha >= fecha);
                }                    
            }


            if (string.IsNullOrWhiteSpace(fechafin))
            {
                var fecha2 = DateTime.Now;
                    fecha2 = fecha2.AddDays(1);
                    predicado = predicado.And(d => d.fecha <= fecha2);
               
            }
            else {

                var fecha2 = Convert.ToDateTime(fechafin);

                var convertir = DateTime.TryParse(fechafin, out fecha2);
                if (convertir == true)
                {
                    fecha2 = fecha2.AddDays(1);
                    predicado = predicado.And(d => d.fecha <= fecha2);
                }          
            }

            //predicado = predicado.And(d => d.esta_pedido<3);

            List<vw_control_pedidos> query = context.vw_control_pedidos.Where(predicado).ToList();

 

            var data = query.Select(x => new
            {
                x.id_detalle,
                x.id,
                x.bodccs_nombre,
                fechadate=x.fecha,
                fecha = x.fecha.ToString("yyyy/MM/dd HH:mm", myCultura),
                clienteCedula = x.clienteCedula != null ? x.clienteCedula : "",
                clienteNombre = x.clienteNombre != null && x.clienteNombre != "" ? x.clienteNombre : "Pedido Sugerido",
                x.cantidad,
                x.usuarioSolicitud,
                Detalle = x.Detalle != null ? x.Detalle : "",
                x.codreferencia,
                x.referencia,
                //x.referenciasSolicitadas,
                x.clasificacion_ABC,
                cantidad_actual = (from refe in context.referencias_inven where refe.bodega == x.idbodega && refe.codigo == x.codreferencia orderby refe.ano descending, refe.mes descending select ((refe.can_ini + refe.can_ent) - refe.can_sal)).FirstOrDefault(),
                cantidad_bodega = (from refe in context.referencias_inven where refe.bodega == bodega && refe.codigo == x.codreferencia orderby refe.ano descending, refe.mes descending select ((refe.can_ini + refe.can_ent) - refe.can_sal)).FirstOrDefault(),
                cantidad_comprometida=(from refe in context.referencias_inven where refe.bodega == bodega && refe.codigo == x.codreferencia orderby refe.ano descending, refe.mes descending select ((refe.can_ini + refe.can_ent+ x.cantidad) - refe.can_sal)).FirstOrDefault() != null ? (from refe in context.referencias_inven where refe.bodega == bodega && refe.codigo == x.codreferencia orderby refe.ano descending, refe.mes descending select ((refe.can_ini + refe.can_ent + x.cantidad) - refe.can_sal)).FirstOrDefault():0,
                cantidad_minima = x.ref_cantidad_min,
                cantidad_pedida = x.cantidad,
                reemplazo= (from r in context.rremplazos where r.referencia == x.codreferencia select r.alterno).FirstOrDefault() != null ? "R" :"",
                tipo_compra = x.tipo_compra != null? x.tipo_compra.ToUpper() : "",
                costo_prom =  (from refe in context.referencias_inven where refe.bodega == x.idbodega && refe.codigo == x.codreferencia orderby refe.ano descending, refe.mes descending select refe.costo_prom).FirstOrDefault(),
                cantidad_clasificacion = x.cantidad_clasificacio,
                x.responsable,
                vehiculo = x.vehiculo != null ? x.vehiculo : "" ,
                fecha_pedido = x.fecha_separacion!= null ? Convert.ToDateTime(x.fecha_separacion).ToString("yyyy/MM/dd HH:mm", myCultura) :"",
                No_de_orden = x.No_de_orden != null ? x.No_de_orden : 0,
                No_de_anticipo = !string.IsNullOrWhiteSpace(x.listaanticipos) ? x.listaanticipos : "",
                separacion_consecutivo = x.separacion != null ? x.separacion : 0,
                x.Descripcion,
                x.estado_solicitud,
                cantidad_backorder= (from pre in context.rprecarga where pre.codigo == x.codreferencia select pre.codigo).FirstOrDefault() != null ? (from pre in context.rprecarga where pre.codigo == x.codreferencia select ((pre.cant_ped) - pre.cant_fact)).Sum() : 0,  
                //cantidad_backorder= (x.cant_ped != null ? x.cant_ped : 0) - (x.cant_fact != null ? x.cant_fact : 0)
            });



            data = data.OrderBy(x => x.fechadate).ToList();



            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult editarcantidadMinima(int solicitud, int cantidad) {

            var buscar = (from s in context.rdetallesolicitud
                          join i in context.icb_referencia on
                              s.referencia equals i.ref_codigo
                          where s.id == solicitud
                          select new
                          {
                              s.id,
                              s.referencia,
                          }).FirstOrDefault();

            var referencia = context.icb_referencia.Find(buscar.referencia);
            referencia.ref_cantidad_min = cantidad;

            context.Entry(referencia).State = EntityState.Modified;
            bool result=context.SaveChanges()>0;

            if (result)
            {
                return Json(1, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GuardarMotivo(int? solicitud, string motivo)
        {

            if (solicitud!= null && !string.IsNullOrWhiteSpace(motivo))
            {
                var solicitudesrepuestos = context.rsolicitudesrepuestos.Find(solicitud);

                solicitudesrepuestos.habilitado = false;
                solicitudesrepuestos.motivo = motivo;

                context.Entry(solicitudesrepuestos).State = EntityState.Modified;

                bool result = context.SaveChanges() > 0;

                if (result)
                {
                    return Json(1, JsonRequestBehavior.AllowGet);
                }
                return Json(0, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            rsolicitudesrepuestos solicitud = context.rsolicitudesrepuestos.Find(id);

            //var contador = Convert.ToInt32(Request["listaReferencias"]);
            //string [] arrayEliminados;

            ////arrayEliminados = Request["arrayEliminados"];

            //if (contador > 0)
            //{

            //    TempData["mensaje"] = "La solicitud se ha agregado exitosamente";
            //    context.SaveChanges();
            //}
            //else
            //{
            //    TempData["mensaje_error"] = "Ingrese al menos una referencia";
            //}


            if (solicitud == null)
            {
                return HttpNotFound();
            }

            var referencias = context.icb_referencia.Where(x => x.ref_estado && x.modulo == "R").Select(x => new
            {
                codigo = x.ref_codigo,
                descripcion = x.ref_codigo + " - " + x.ref_descripcion
            }).OrderBy(x => x.descripcion).ToList();

            ViewBag.codigo = new SelectList(referencias, "codigo", "descripcion");
            ViewBag.pedidoVacio = solicitud.Pedido != null ? 0 : 1;
            ViewBag.id = id;
            ListasDesplegables(solicitud);
            BuscarFavoritos(menu);
            return View(solicitud);
        }

        [HttpPost]
        public ActionResult Edit(rsolicitudesrepuestos modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                context.Entry(modelo).State = EntityState.Modified;
                int guardar = context.SaveChanges();
                if (guardar > 0)
                {
                    TempData["mensaje"] = "La solicitud se ha agregado exitosamente";
                }
                else
                {
                    TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor verifique...";
                }
            }

            var referencias = context.icb_referencia.Where(x => x.ref_estado && x.modulo == "R").Select(x => new
            {
                codigo = x.ref_codigo,
                descripcion = x.ref_codigo + " - " + x.ref_descripcion
            }).OrderBy(x => x.descripcion).ToList();

            ViewBag.codigo = new SelectList(referencias, "codigo", "descripcion");
            ViewBag.pedidoVacio = modelo.Pedido != null ? 0 : 1;
            ListasDesplegables(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }

        public void ListasDesplegables(rsolicitudesrepuestos solicitud)
        {
            int bod = Convert.ToInt32(Session["user_bodega"]);
            if (solicitud.id_ot != null)
            {
                bod = solicitud.bodega;
            }
            ViewBag.bodega = new SelectList(context.bodega_concesionario.Where(x => x.bodccs_estado && x.id == bod).OrderBy(x => x.bodccs_nombre), "id", "bodccs_nombre", bod);
            ViewBag.tiposolicitud = new SelectList(context.rtiposolicitudrepuesto, "id", "Descripcion", solicitud.tiposolicitud);

            List<icb_referencia> buscarReferencias = context.icb_referencia.Where(x => x.modulo == "R").ToList();
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (icb_referencia item in buscarReferencias)
            {
                string nombre = "(" + item.ref_codigo + ") " + item.ref_descripcion;
                items.Add(new SelectListItem
                {
                    Text = nombre,
                    Value = item.ref_codigo
                });
            }

            ViewBag.ref_codigo = new SelectList(items, "Value", "Text", solicitud.ref_codigo);

            var buscarClientes = (from terceros in context.icb_terceros
                                  join clientes in context.tercero_cliente
                                      on terceros.tercero_id equals clientes.tercero_id
                                  where terceros.tercero_estado
                                  select new
                                  {
                                      terceros.tercero_id,
                                      nombreCompleto = terceros.doc_tercero + " - " + terceros.prinom_tercero + " " + terceros.segnom_tercero + " " + terceros.apellido_tercero + " " + terceros.segapellido_tercero,
                                      terceros.razon_social
                                  }).ToList();


            List<SelectListItem> itemsClientes = new List<SelectListItem>();
            foreach (var item in buscarClientes)
            {
                string nombre = item.nombreCompleto + " " + item.razon_social;
                itemsClientes.Add(new SelectListItem
                {
                    Text = nombre,
                    Value = item.tercero_id.ToString()
                });
            }
            if (solicitud.cliente != null)
            {
                ViewBag.cliente = new SelectList(itemsClientes, "Value", "Text", solicitud.cliente);
            }
            else
            {
                ViewBag.cliente = new SelectList(itemsClientes, "Value", "Text");
            }

            ViewBag.tipo_compra = new SelectList(context.rtipocompra, "id", "Descripcion", solicitud.tipo_compra);


            ViewBag.clasificacion_solicitud = new SelectList(context.rclasificacion_solicitud, "id", "descripcion", solicitud.clasificacion_solicitud);
            var buscarPlacas = (from pla in context.icb_vehiculo
                                where pla.plac_vh != null
                                select new
                                {
                                    pla.plan_mayor,
                                    pla.plac_vh
                                }
                                 ).ToList();

            List<SelectListItem> itemsPlacas = new List<SelectListItem>();
            foreach (var item in buscarPlacas)
            {
                string placa = item.plac_vh;
                itemsPlacas.Add(new SelectListItem
                {
                    Text = placa,
                    Value = item.plan_mayor
                });

            }
            //este viewbag llena el compo de las placas
            ViewBag.planm_vehiculo = new SelectList(itemsPlacas, "Value", "Text", solicitud.planm_vehiculo);
            //viewBag referencia desde facturacio repuestos

            var listR = 
                (from b in context.tempSolicitud
                 select new { idtemp = b.idtemp, id = b.idkitaccesorios}
                ).OrderByDescending(x => x.idtemp).FirstOrDefault();

            if (listR.id != null)
            {
                var listaR = (from a in context.referenciaskits
                              join b in context.icb_referencia
                                 on a.codigo equals b.ref_codigo
                              join c in context.tempSolicitud
                                 on a.idkitaccesorios equals c.idkitaccesorios
                              select new
                              {
                                  id = a.codigo,
                                  idtemp = c.idtemp
                                  //nombre = b.ref_descripcion
                              }).OrderByDescending(x => x.idtemp).FirstOrDefault();
                ViewBag.Referencia = listaR.id;
            }




            var listRef = (from b in context.tempSolicitud
                           select new { idtemp=b.idtemp,id=b.idreferencia }
                           ).OrderByDescending(x => x.idtemp).FirstOrDefault();
            if (listRef.id!=null)
            {
                var listaR1 = (
                    from b in context.icb_referencia
                    join c in context.tempSolicitud
                        on b.ref_codigo equals c.idreferencia
                    select new
                    {
                        id = c.idreferencia,
                        idtemp = c.idtemp
                    }).OrderByDescending(x => x.idtemp).FirstOrDefault();
                ViewBag.Referencia1 = listaR1.id;
            }


            /*
            //viewBag referencia desde facturacio repuestos
            var listaC = (from a in context.icb_terceros
                          join c in context.tempSolicitud
                            on a.tercero_id equals c.dataCliente
                          select new
                          {
                              id = c.dataCliente,
                              idtemp=c.idtemp
                              //nombre = b.ref_descripcion
                          }).OrderByDescending(x => x.idtemp).FirstOrDefault();
            
            ViewBag.Cliente1 = listaC.id;
            */
            if (solicitud.cliente != null)
            {
                ViewBag.Cliente1 = solicitud.cliente;
            }
            else           {
                ViewBag.Cliente1 = null;
            }

        }

        public JsonResult buscarReferencias(int id)
        {
            var buscar = (from a in context.rdetallesolicitud
                          join b in context.icb_referencia
                              on a.referencia equals b.ref_codigo
                          where a.id_solicitud == id
                          select new
                          {
                              a.id,
                              b.ref_codigo,
                              b.ref_descripcion,
                              a.cantidad,
                              a.iva,
                              a.valor
                          }).ToList();

            var data = buscar.Select(x => new
            {
                x.id,
                codigotxt = x.ref_codigo + " - " + x.ref_descripcion,
                codigo = x.ref_codigo,
                x.cantidad,
                x.iva,
                valor = x.valor.Value.ToString("0,0", elGR)
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarReferencia(string codigo)
        {
            var buscar = (from a in context.icb_referencia
                          join b in context.rprecios
                              on a.ref_codigo equals b.codigo
                          where a.modulo == "R" && a.ref_estado && a.ref_codigo == codigo
                          select new
                          {
                              a.ref_codigo,
                              b.precio1,
                              a.por_iva
                          }).FirstOrDefault();

            if (buscar != null)
            {
                var data = new
                {
                    buscar.ref_codigo,
                    precio1 = buscar.precio1.ToString("0,0", elGR),
                    precio1Hidden = buscar.precio1,
                    buscar.por_iva,
                    por_ivaHidden = buscar.por_iva,
                    precioIva = (buscar.precio1 * Convert.ToDecimal(buscar.por_iva, culturaInfo) / 100).ToString("0,0", elGR),
                    precioIvaHidden = buscar.precio1 * Convert.ToDecimal(buscar.por_iva, culturaInfo) / 100
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var data = new
                {
                    ref_codigo = codigo,
                    precio1 = 0,
                    precio1Hidden = 0,
                    por_iva = 0,
                    por_ivaHidden = 0,
                    precioIva = 0,
                    precioIvaHidden = 0
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult validarStock(string codigoRef, int bodega, int cantidad)
        {
            decimal cant = context.vw_inventario_hoy.Where(x => x.ref_codigo == codigoRef && x.bodega == bodega)
                .Select(x => x.stock).FirstOrDefault();
            if (cant < cantidad)
            {
                return Json(new { data = false, cant }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { data = true }, JsonRequestBehavior.AllowGet);
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
    }
}