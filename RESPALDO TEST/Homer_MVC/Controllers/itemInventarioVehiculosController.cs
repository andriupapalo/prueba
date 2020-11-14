using Homer_MVC.IcebergModel;
using Homer_MVC.ViewModels.medios;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class itemInventarioVehiculosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        private readonly CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

        // GET: itemInventario
        public ActionResult Index()
        {
            ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
            var buscarReferencia = (from referencia in context.icb_referencia
                                    where referencia.modulo == "V"
                                    select new
                                    {
                                        referencia.ref_codigo,
                                        ref_descripcion = "(" + referencia.ref_codigo + ") " + referencia.ref_descripcion
                                    }).OrderBy(x => x.ref_descripcion).ToList();
            ViewBag.referencia = new SelectList(buscarReferencia, "ref_codigo", "ref_descripcion");
            return View();
        }

        public JsonResult BuscarPedidos(string codigo, int?[] bodegas, DateTime? fechaInicial, DateTime? fechaFinal)
        {
            System.Linq.Expressions.Expression<Func<vw_pedidos, bool>> predicate = PredicateBuilder.True<vw_pedidos>();
            System.Linq.Expressions.Expression<Func<vw_pedidos, bool>> bodegasPredicate = PredicateBuilder.False<vw_pedidos>();
            if (fechaInicial != null && fechaFinal != null && fechaInicial <= fechaFinal)
            {
                predicate = predicate.And(
                    x => x.fecha >= fechaInicial && x.fecha <= fechaFinal && x.planmayor == codigo);
                if (bodegas != null)
                {
                    if (bodegas[0] == 0)
                    {
                        bodegas[0] = Convert.ToInt32(Session["user_bodega"]);
                    }

                    foreach (int item in bodegas)
                    {
                        bodegasPredicate = bodegasPredicate.Or(x => x.id_bodega == item);
                    }

                    predicate = predicate.And(bodegasPredicate);
                }

                List<vw_pedidos> datos = context.vw_pedidos.Where(predicate).ToList();

                var data = datos.Select(x => new
                {
                    x.numero,
                    x.cot_numcotizacion,
                    fecha = x.fecha.ToString("yyyy/MM/dd"),
                    x.bodccs_nombre,
                    x.doc_tercero,
                    nombre = x.prinom_tercero + " " + x.segnom_tercero + " " + x.apellido_tercero + " " +
                             x.segapellido_tercero,
                    vendedor = x.asesor,
                    x.modelo,
                    x.modvh_nombre,
                    x.id_anio_modelo,
                    color = x.colvh_nombre,
                    fecha_asignacion_planmayor = x.fecha_asignacion_planmayor.Value.ToString("yyyy/MM/dd"),
                    valor_unitario = x.valor_unitario != null ? x.valor_unitario.Value.ToString("0,0", elGR) : "",
                    porcentaje_iva = x.porcentaje_iva != null ? x.porcentaje_iva : 0,
                    porcentaje_impoconsumo = x.porcentaje_impoconsumo != null ? x.porcentaje_impoconsumo : 0,
                    vrtotal = x.vrtotal != null ? x.vrtotal.Value.ToString("0,0", elGR) : "",
                    valorPoliza = x.valorPoliza != null ? x.valorPoliza.Value.ToString("0,0", elGR) : "",
                    nit_prenda = x.nit_prenda != null ? x.nit_prenda : "",
                    numfactura = x.numfactura != null ? x.numfactura : 0,
                    codigoFlota = x.codigoFlota != null ? x.codigoFlota : "",
                    descripcionFlota = x.descripcionFlota != null ? x.descripcionFlota : "",
                    valormatricula = x.valormatricula != null ? x.valormatricula.Value.ToString("0,0", elGR) : "",
                    valorsoat = x.valorsoat != null ? x.valorsoat.Value.ToString("0,0", elGR) : ""
                });
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
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

        public JsonResult BuscarCodigoReferencia(string codigo, int[] bodegas)
        {
            string tipoReferencia = (from referencia in context.icb_referencia
                                     where referencia.ref_codigo == codigo
                                     select referencia.modulo).FirstOrDefault();

            var buscarStock = (from inventarioHoy in context.vw_inventario_hoy
                               join bodega in context.bodega_concesionario
                                   on inventarioHoy.bodega equals bodega.id
                               where bodegas.Contains(inventarioHoy.bodega) && inventarioHoy.ref_codigo == codigo
                               select new
                               {
                                   bodega.bodccs_cod,
                                   bodega.bodccs_nombre,
                                   inventarioHoy.stock
                               }).ToList();


            if (tipoReferencia == "R")
            {
                var buscarReferencia = (from referencia in context.icb_referencia
                                        join tercero in context.icb_terceros
                                            on referencia.proveedor_ppal equals tercero.tercero_id into ps2
                                        from tercero in ps2.DefaultIfEmpty()
                                        where referencia.ref_codigo == codigo
                                        select new
                                        {
                                            referencia.ref_codigo,
                                            referencia.ref_descripcion,
                                            referencia.por_iva,
                                            referencia.por_iva_compra,
                                            referencia.unidad_medida,
                                            proveedor = tercero.razon_social + tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                        tercero.apellido_tercero + " " + tercero.segapellido_tercero,
                                            referencia.clasificacion_ABC
                                        }).FirstOrDefault();

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

        public JsonResult buscarTrazabilidad(string codigo, int[] bodegas, DateTime fechaInicial, DateTime fechaFinal)
        {
            List<trazabilidadClass> trazabilidad = new List<trazabilidadClass>();
            if (bodegas[0] == 0)
            {
                bodegas[0] = Convert.ToInt32(Session["user_bodega"]);
            }

            foreach (int item in bodegas)
            {
                var buscar = (from a in context.icb_vehiculo_eventos
                              join b in context.icb_tpeventos
                                  on a.id_tpevento equals b.tpevento_id
                              join c in context.bodega_concesionario
                                  on a.bodega_id equals c.id
                              join d in context.ubicacion_bodega
                                  on a.ubicacion equals d.id into xx
                              from d in xx.DefaultIfEmpty()
                              join e in context.tramitador_vh
                                  on a.idtramitador equals e.tramitador_id into aa
                              from e in aa.DefaultIfEmpty()
                              join f in context.ttecnicos
                                  on a.idtecnico equals f.id into bb
                              from f in bb.DefaultIfEmpty()
                              join u in context.users
                                  on f.idusuario equals u.user_id into uu
                              from u in uu.DefaultIfEmpty()
                              where a.planmayor == codigo && a.bodega_id == item
                              select new
                              {
                                  a.fechaevento,
                                  a.planmayor,
                                  b.tpevento_nombre,
                                  c.bodccs_nombre,
                                  a.evento_observacion,
                                  ubicacion = d.descripcion,
                                  a.placa,
                                  e.tramitadorpri_nombre,
                                  e.tramitadorseg_nombre,
                                  e.tramitador_apellidos,
                                  a.idtramitador,
                                  a.idtecnico,
                                  u.user_nombre,
                                  u.user_apellido
                              }).ToList();

                for (int i = 0; i < buscar.Count; i++)
                {
                    trazabilidad.Add(new trazabilidadClass
                    {
                        fechaEvento = buscar[i].fechaevento.ToString("yyyy/MM/dd HH:mm"),
                        planMayor = buscar[i].planmayor,
                        descripcion = buscar[i].tpevento_nombre,
                        bodega = buscar[i].bodccs_nombre != null ? buscar[i].bodccs_nombre : "",
                        observacion = buscar[i].evento_observacion != null ? buscar[i].evento_observacion : "",
                        placa = buscar[i].placa != null ? buscar[i].placa : "",
                        tramitador = buscar[i].idtramitador != null
                            ? buscar[i].tramitadorpri_nombre + " " + buscar[i].tramitador_apellidos
                            : "",
                        tecnico = buscar[i].idtecnico != null
                            ? buscar[i].user_nombre + " " + buscar[i].user_apellido
                            : "",
                        ubicacion = buscar[i].ubicacion != null ? buscar[i].ubicacion : ""
                    });
                }
            }

            return Json(trazabilidad.OrderBy(x => x.fechaEvento), JsonRequestBehavior.AllowGet);
        }

        public class trazabilidadClass
        {
            public string fechaEvento { get; set; }
            public string planMayor { get; set; }
            public string descripcion { get; set; }
            public string bodega { get; set; }
            public string observacion { get; set; }
            public string ubicacion { get; set; }
            public string placa { get; set; }
            public string tramitador { get; set; }
            public string tecnico { get; set; }
        }
    }
}