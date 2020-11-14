using Homer_MVC.IcebergModel;
using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    namespace Homer_MVC.Controllers
    {
        public class itemInventarioController : Controller
        {
            private readonly Iceberg_Context context = new Iceberg_Context();

            DateTimeFormatInfo usDtfi = new CultureInfo("en-US").DateTimeFormat;
            // GET: itemInventario
            public ActionResult Index()
            {
                ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
                var buscarReferencia = (from referencia in context.icb_referencia
                                        where referencia.modulo == "R"
                                        select new
                                        {
                                            referencia.ref_codigo,
                                            ref_descripcion = "(" + referencia.ref_codigo + ") " + referencia.ref_descripcion
                                        }).OrderBy(x => x.ref_descripcion).ToList();
                ViewBag.referencia = new SelectList(buscarReferencia, "ref_codigo", "ref_descripcion");
                return View();
            }


            public JsonResult BuscarPedidos(string codigo, int[] bodegas, DateTime fechaInicial, DateTime fechaFinal)
            {
                string tipoReferencia = (from referencia in context.icb_referencia
                                         where referencia.ref_codigo == codigo
                                         select referencia.modulo).FirstOrDefault();

                if (tipoReferencia == "R")
                {
                    var buscarPedidosSql = (from detalles in context.icb_referencia_movdetalle
                                            join encabezado in context.icb_referencia_mov
                                                on detalles.refmov_id equals encabezado.refmov_id
                                            join tipoDocumento in context.tp_doc_registros
                                                on encabezado.tpdocid equals tipoDocumento.tpdoc_id
                                            join bodega in context.bodega_concesionario
                                                on encabezado.bodega_id equals bodega.id
                                            join cliente in context.icb_terceros
                                                on encabezado.cliente equals cliente.tercero_id
                                            join condicionPago in context.fpago_tercero
                                                on encabezado.condicion equals condicionPago.fpago_id
                                            where detalles.ref_codigo == codigo && encabezado.refmov_fecela >= fechaInicial &&
                                                  encabezado.refmov_fecela <= fechaFinal && bodegas.Contains(encabezado.bodega_id) &&
                                                  tipoDocumento.tipo == 27
                                            select new
                                            {
                                                tipoDocumento.tpdoc_nombre,
                                                encabezado.refmov_numero,
                                                encabezado.refmov_fecela,
                                                bodega.bodccs_nombre,
                                                docCliente = cliente.doc_tercero,
                                                cliente = cliente.razon_social + cliente.prinom_tercero + " " + cliente.segnom_tercero + " " +
                                                          cliente.apellido_tercero + " " + cliente.segapellido_tercero,
                                                condicionPago.fpago_nombre,
                                                detalles.refdet_cantidad,
                                                detalles.valor_unitario,
                                                detalles.pordscto,
                                                detalles.poriva
                                            }).ToList();

                    var buscarPedidos = buscarPedidosSql.Select(x => new
                    {
                        x.tpdoc_nombre,
                        x.refmov_numero,
                        refmov_fecela = x.refmov_fecela.ToShortDateString(),
                        x.bodccs_nombre,
                        x.docCliente,
                        x.cliente,
                        x.fpago_nombre,
                        x.refdet_cantidad,
                        x.valor_unitario,
                        x.pordscto,
                        x.poriva,
                        total = x.valor_unitario * x.refdet_cantidad -
                                x.valor_unitario * x.refdet_cantidad * (decimal)x.pordscto / 100 +
                                x.valor_unitario * x.refdet_cantidad * (decimal)x.poriva / 100
                    });

                    return Json(buscarPedidos, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var buscarPedidosSql = (from pedido in context.vpedido
                                            join bodega in context.bodega_concesionario
                                                on pedido.bodega equals bodega.id
                                            join cliente in context.icb_terceros
                                                on pedido.nit equals cliente.tercero_id
                                            where pedido.planmayor == codigo
                                            select new
                                            {
                                                tpdoc_nombre = "Pedido",
                                                pedido.numero,
                                                pedido.fecha,
                                                bodega.bodccs_nombre,
                                                docCliente = cliente.doc_tercero,
                                                cliente = cliente.razon_social + cliente.prinom_tercero + " " + cliente.segnom_tercero + " " +
                                                          cliente.apellido_tercero + " " + cliente.segapellido_tercero,
                                                fpago_nombre = "",
                                                refdet_cantidad = 1,
                                                pedido.valor_unitario,
                                                pedido.pordscto,
                                                poriva = pedido.porcentaje_iva
                                            }).ToList();
                    var buscarPedidos = buscarPedidosSql.Select(x => new
                    {
                        x.tpdoc_nombre,
                        refmov_numero = x.numero,
                        refmov_fecela = x.fecha.ToShortDateString(),
                        x.bodccs_nombre,
                        x.docCliente,
                        x.cliente,
                        x.fpago_nombre,
                        x.refdet_cantidad,
                        x.valor_unitario,
                        pordscto = x.pordscto != null ? x.pordscto : 0,
                        poriva = x.poriva != null ? x.poriva : 0,
                        total = x.valor_unitario * x.refdet_cantidad -
                                x.valor_unitario * x.refdet_cantidad * (decimal)(x.pordscto ?? 0) / 100 +
                                x.valor_unitario * x.refdet_cantidad * (decimal)(x.poriva ?? 0) / 100
                    });
                    return Json(buscarPedidos, JsonRequestBehavior.AllowGet);
                }
            }


            public JsonResult BuscarOrdenesCompra(string codigo, int[] bodegas, DateTime fechaInicial, DateTime fechaFinal)
            {
                var buscarOrdenesSql = (from detalles in context.rdetalleordencompra
                                        join encabezado in context.rordencompra
                                            on detalles.idencaborden equals encabezado.idorden
                                        join tipoDocumento in context.tp_doc_registros
                                            on encabezado.idtipodoc equals tipoDocumento.tpdoc_id
                                        join bodega in context.bodega_concesionario
                                            on encabezado.bodega equals bodega.id
                                        join tipoCompra in context.rtipocompra
                                            on encabezado.tipoorden equals tipoCompra.id
                                        join proveedor in context.icb_terceros
                                            on encabezado.proveedor equals proveedor.tercero_id
                                        //join destinatario in context.icb_terceros
                                        //on encabezado.destinatario equals destinatario.tercero_id
                                        join vendedor in context.users
                                            on encabezado.vendedor equals vendedor.user_id
                                        where detalles.codigo_referencia == codigo && encabezado.fec_creacion >= fechaInicial &&
                                              encabezado.fec_creacion <= fechaFinal && bodegas.Contains(encabezado.bodega)
                                        select new
                                        {
                                            tipoDocumento.tpdoc_nombre,
                                            encabezado.numero,
                                            encabezado.fec_creacion,
                                            bodega.bodccs_nombre,
                                            tipoCompra.descripcion,
                                            proveedor = proveedor.razon_social + proveedor.prinom_tercero + " " + proveedor.segnom_tercero +
                                                        " " + proveedor.apellido_tercero + " " + proveedor.segapellido_tercero,
                                            encabezado.destinatario,
                                            vendedor = vendedor.user_nombre + " " + vendedor.user_apellido,
                                            detalles.valorunitario,
                                            detalles.cantidad,
                                            detalles.porcendescto,
                                            detalles.porceniva,
                                            total = detalles.valorunitario * detalles.cantidad -
                                                    detalles.valorunitario * detalles.cantidad * detalles.porcendescto / 100 +
                                                    detalles.valorunitario * detalles.cantidad * detalles.porceniva / 100
                                        }).ToList();

                var buscarOrdenes = buscarOrdenesSql.Select(x => new
                {
                    x.tpdoc_nombre,
                    x.numero,
                    fec_creacion = x.fec_creacion.ToShortDateString(),
                    x.bodccs_nombre,
                    x.descripcion,
                    x.proveedor,
                    x.destinatario,
                    x.vendedor,
                    valorunitario = Math.Round(x.valorunitario ?? 0),
                    x.cantidad,
                    x.porcendescto,
                    x.porceniva,
                    total = Math.Round(x.total ?? 0)
                });

                return Json(buscarOrdenes, JsonRequestBehavior.AllowGet);
            }


            public JsonResult BuscarHistorial(string codigo, int[] bodegas, DateTime fechaInicial, DateTime fechaFinal)
            {
                var buscarHistorialSql = (from lineas in context.lineas_documento
                                          join bodega in context.bodega_concesionario
                                              on lineas.bodega equals bodega.id
                                          join encabezado in context.encab_documento
                                              on lineas.id_encabezado equals encabezado.idencabezado
                                          join tipoDocumento in context.tp_doc_registros
                                              on encabezado.tipo equals tipoDocumento.tpdoc_id
                                          join tercero in context.icb_terceros
                                              on encabezado.nit equals tercero.tercero_id
                                          where lineas.codigo == codigo && lineas.fec >= fechaInicial && lineas.fec <= fechaFinal &&
                                                bodegas.Contains(lineas.bodega)
                                          select new
                                          {
                                              documento = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                              encabezado.numero,
                                              tercero = tercero.razon_social + tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                        tercero.apellido_tercero + " " + tercero.segapellido_tercero,
                                              bodega.bodccs_nombre,
                                              lineas.fec,
                                              lineas.cantidad,
                                              lineas.costo_unitario,
                                              costoTotal = lineas.costo_unitario * lineas.cantidad
                                          }).ToList();

                var buscarHistorial = buscarHistorialSql.Select(x => new
                {
                    x.documento,
                    x.numero,
                    x.tercero,
                    x.bodccs_nombre,
                    fec = x.fec.ToShortDateString(),
                    x.cantidad,
                    x.costo_unitario,
                    x.costoTotal
                }).ToList();
                return Json(buscarHistorial, JsonRequestBehavior.AllowGet);
            }


            public int validarcomprometido(string codigo, string bodccs_cod)
            {
                int resultado = 0;
                if (!string.IsNullOrWhiteSpace(codigo) && !string.IsNullOrWhiteSpace(bodccs_cod))
                {
                    //bsco la bodega,
                    bodega_concesionario bodega = context.bodega_concesionario.Where(d => d.bodccs_cod == bodccs_cod).FirstOrDefault();
                    //busco en rseparacionreferencia
                    resultado = context.rseparacionmercancia
                        .Where(d => d.codigo == codigo && d.bodega == bodega.id && d.estado).Count();
                }

                return resultado;
            }

            public JsonResult BuscarCodigoReferencia(string codigo, int[] bodegas)
            {
                string tipoReferencia = (from referencia in context.icb_referencia
                                         where referencia.ref_codigo == codigo
                                         select referencia.modulo).FirstOrDefault();

                var buscarStock2 = (from inventarioHoy in context.vw_inventario_hoy
                                    join bodega in context.bodega_concesionario
                                        on inventarioHoy.bodega equals bodega.id
                                    where bodegas.Contains(inventarioHoy.bodega) && inventarioHoy.ref_codigo == codigo
                                    select new
                                    {
                                        codigo_bodega = bodega.id,
                                        bodega.bodccs_cod,
                                        bodega.bodccs_nombre,
                                        inventarioHoy.stock,
                                        clasificacion = inventarioHoy.clasificacion_ABC
                                    }).ToList();

                var buscarStock = buscarStock2.Select(d => new
                {
                    codigo_referencia = codigo,
                    d.codigo_bodega,
                    d.bodccs_cod,
                    d.bodccs_nombre,
                    d.stock,
                    cantidadcomprometida = validarcomprometido(codigo, d.bodccs_cod),
                    d.clasificacion
                }).ToList();

                if (tipoReferencia == "R")
                {
                    icb_referencia buscarReferencia2 = context.icb_referencia.Where(d => d.ref_codigo == codigo).FirstOrDefault();

                    var buscarReferencia = new
                    {
                        buscarReferencia2.ref_codigo,
                        buscarReferencia2.ref_descripcion,
                        buscarReferencia2.por_iva,
                        buscarReferencia2.por_iva_compra,
                        buscarReferencia2.unidad_medida,
                        costo_promedio = buscarReferencia2.costo_promedio != null ? buscarReferencia2.costo_promedio.Value.ToString("N0", new CultureInfo("is-IS")) : "0",
                        precio_venta = buscarReferencia2.precio_venta.ToString("N0", new CultureInfo("is-IS")),
                        precio_compra = buscarReferencia2.costo_unitario.ToString("N0", new CultureInfo("is-IS")),
                        costo_emergencia = buscarReferencia2.costo_emergencia.ToString("N0", new CultureInfo("is-IS")),
                        proveedor = buscarReferencia2.icb_terceros != null ? (buscarReferencia2.icb_terceros.razon_social +
                                    buscarReferencia2.icb_terceros.prinom_tercero + " " +
                                    buscarReferencia2.icb_terceros.segnom_tercero + " " +
                                    buscarReferencia2.icb_terceros.apellido_tercero + " " +
                                    buscarReferencia2.icb_terceros.segapellido_tercero) : "",
                        buscarReferencia2.clasificacion_ABC,
                        conteoRotacion = (from documento in context.encab_documento
                                          join lineas in context.lineas_documento
                                            on documento.idencabezado equals lineas.id_encabezado
                                          join registro in context.tp_doc_registros
                                            on documento.tipo equals registro.tpdoc_id
                                          join tipo in context.tp_doc_registros_tipo
                                            on registro.tipo equals tipo.id
                                          where tipo.id == 4 && lineas.codigo == codigo
                                          select new
                                          {
                                              lineas.codigo
                                          }).Count()
                    };


                    return Json(new { tipoReferencia = "R", buscarReferencia, buscarStock }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var buscarReferencia = (from referencia in context.icb_referencia
                                            join vehiculo in context.icb_vehiculo
                                                on referencia.ref_codigo equals vehiculo.plan_mayor
                                            join modelo in context.modelo_vehiculo
                                                on vehiculo.modvh_id equals modelo.modvh_codigo
                                            join marca in context.marca_vehiculo
                                                on modelo.mar_vh_id equals marca.marcvh_id
                                            join color in context.color_vehiculo
                                                on vehiculo.colvh_id equals color.colvh_id
                                            join tipoServicio in context.tpservicio_vehiculo
                                                on vehiculo.tiposervicio equals tipoServicio.tpserv_id into ps
                                            from tipoServicio in ps.DefaultIfEmpty()
                                            join proveedor in context.icb_terceros
                                                on vehiculo.proveedor_id equals proveedor.tercero_id into ps2
                                            from proveedor in ps2.DefaultIfEmpty()
                                            where referencia.ref_codigo == codigo
                                            select new
                                            {
                                                referencia.ref_codigo,
                                                costo_promedio = referencia.costo_promedio != null ? referencia.costo_promedio.Value.ToString("N0", new CultureInfo("is-IS")) : "0",
                                                referencia.ref_descripcion,
                                                vehiculo.vin,
                                                vehiculo.plac_vh,
                                                vehiculo.nummot_vh,
                                                vehiculo.anio_vh,
                                                vehiculo.impconsumo,
                                                proveedor = proveedor.razon_social + proveedor.prinom_tercero + " " + proveedor.segnom_tercero +
                                                            " " + proveedor.apellido_tercero + " " + proveedor.segapellido_tercero,
                                                marca.marcvh_nombre,
                                                color.colvh_nombre,
                                                tipoServicio.tpserv_nombre
                                            }).FirstOrDefault();
                    return Json(new { tipoReferencia = "V", buscarReferencia, buscarStock }, JsonRequestBehavior.AllowGet);
                }
            }

            public JsonResult TraerUbicaciones(string codigo_referencia, int idbodega)
            {
                var ubicacion = (from ubicaciones in context.ubicacion_repuesto
                                 join bodega in context.bodega_concesionario on ubicaciones.bodega equals bodega.id
                                 join ubiBod in context.ubicacion_repuestobod on ubicaciones.ubicacion equals ubiBod.id
                                 join referencia in context.icb_referencia on ubicaciones.codigo equals referencia.ref_codigo
                                 join area in context.area_bodega on ubicaciones.idarea equals area.areabod_id into zz
                                 from area in zz.DefaultIfEmpty()
                                 where referencia.ref_codigo == codigo_referencia && bodega.id == idbodega
                                 select new
                                 {
                                     ubicaciones.id,
                                     bodega.bodccs_nombre,
                                     referencia.ref_codigo,
                                     referencia.ref_descripcion,
                                     ubicacion = ubiBod.descripcion,
                                     area = area.areabod_nombre != null ? area.areabod_nombre : "",
                                     nota = ubicaciones.notaUbicacion != null ? ubicaciones.notaUbicacion : "",
                                 }).ToList();
                return Json(ubicacion, JsonRequestBehavior.AllowGet);
            }

            public JsonResult TraerReemplazos(string codigo_referencia)
            {
                var reemplazos = (from rpz in context.rremplazos
                                  where rpz.referencia == codigo_referencia
                                  join r in context.icb_referencia on rpz.referencia equals r.ref_codigo
                                  join a in context.icb_referencia on rpz.alterno equals a.ref_codigo
                                  select new
                                  {
                                      codigo_referencia_reemplazo = a.ref_codigo,
                                      descripcion_reemplazo = a.ref_descripcion
                                  }).ToList();
                return Json(reemplazos, JsonRequestBehavior.AllowGet);
            }
        }
    }
}