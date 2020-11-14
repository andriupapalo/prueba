using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class ventaRepuestoAsesorController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: ventaRepuestoAsesor
        public ActionResult Index()
        {
            ViewBag.tp_doc_registros = context.tp_doc_registros.Where(x => x.tipo == 4).ToList();
            ViewBag.asesores = context.users.Where(x => x.rol_id == 4).ToList();
            return View();
        }


        public JsonResult BuscarVentasPorAsesor(int? tpDocumento, DateTime? desde, DateTime? hasta, int? asesor_id)
        {
            List<int> listaTipoDoc = new List<int>();
            List<int> listaAsesor_id = new List<int>();
            if (tpDocumento == null)
            {
                listaTipoDoc = context.tp_doc_registros.Where(x => x.tipo == 4).Select(x => x.tpdoc_id).ToList();
            }
            else
            {
                listaTipoDoc.Add(tpDocumento ?? 0);
            }

            if (desde == null)
            {
                desde = context.encab_documento.OrderBy(x => x.fecha).Select(x => x.fecha).FirstOrDefault();
            }

            if (hasta == null)
            {
                hasta = context.encab_documento.OrderByDescending(x => x.fecha).Select(x => x.fecha).FirstOrDefault();
            }
            else
            {
                hasta = hasta.GetValueOrDefault().AddDays(1);
            }

            if (asesor_id == null)
            {
                var buscarVentas = (from encabezado in context.encab_documento
                                    join tipoDocumento in context.tp_doc_registros
                                        on encabezado.tipo equals tipoDocumento.tpdoc_id
                                    join bodega in context.bodega_concesionario
                                        on encabezado.bodega equals bodega.id
                                    join referencia in context.icb_referencia
                                        on encabezado.documento equals referencia.ref_codigo
                                    join tercero in context.icb_terceros
                                        on encabezado.nit equals tercero.tercero_id into ter
                                    from tercero in ter.DefaultIfEmpty()
                                    join terCliente in context.tercero_cliente
                                        on tercero.tercero_id equals terCliente.tercero_id into terCli
                                    from terCliente in terCli.DefaultIfEmpty()
                                    join asesor in context.users
                                        on encabezado.vendedor equals asesor.user_id into ases
                                    from asesor in ases.DefaultIfEmpty()
                                    where listaTipoDoc.Contains(encabezado.tipo)
                                          && encabezado.fecha >= desde
                                          && encabezado.fecha <= hasta
                                    select new
                                    {
                                        tipoDocumento.prefijo,
                                        encabezado.numero,
                                        bodega.bodccs_nombre,
                                        encabezado.fecha,
                                        cliente = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                  tercero.apellido_tercero + " " + tercero.segapellido_tercero + tercero.razon_social,
                                        asesor = asesor.user_nombre + " " + asesor.user_apellido,
                                        referencia.ref_codigo,
                                        referencia.ref_descripcion,
                                        encabezado.valor_total,
                                        encabezado.iva,
                                        encabezado.valor_mercancia,
                                        encabezado.idencabezado
                                    }).ToList();

                var dataVentas = buscarVentas.Select(x => new
                {
                    x.prefijo,
                    x.bodccs_nombre,
                    x.numero,
                    fecha = x.fecha.ToShortDateString(),
                    cliente = x.cliente.Trim(),
                    x.asesor,
                    x.ref_codigo,
                    x.ref_descripcion,
                    x.valor_total,
                    x.iva,
                    x.valor_mercancia,
                    x.idencabezado
                });
                return Json(dataVentas, JsonRequestBehavior.AllowGet);
            }
            else
            {
                // Cuando el usuario no filtra por tipo de cliente, el campo tipo cliente puede ser nulo por tanto por eso se hace la consulta de dos maneras diferentes
                var buscarVentas = (from encabezado in context.encab_documento
                                    join tipoDocumento in context.tp_doc_registros
                                        on encabezado.tipo equals tipoDocumento.tpdoc_id
                                    join bodega in context.bodega_concesionario
                                        on encabezado.bodega equals bodega.id
                                    join referencia in context.icb_referencia
                                        on encabezado.documento equals referencia.ref_codigo
                                    join tercero in context.icb_terceros
                                        on encabezado.nit equals tercero.tercero_id into ter
                                    from tercero in ter.DefaultIfEmpty()
                                    join terCliente in context.tercero_cliente
                                        on tercero.tercero_id equals terCliente.tercero_id into terCli
                                    from terCliente in terCli.DefaultIfEmpty()
                                    join asesor in context.users
                                        on encabezado.vendedor equals asesor.user_id into ases
                                    from asesor in ases.DefaultIfEmpty()
                                    where listaTipoDoc.Contains(encabezado.tipo)
                                          && encabezado.vendedor == asesor_id
                                          && encabezado.fecha >= desde
                                          && encabezado.fecha <= hasta
                                    select new
                                    {
                                        tipoDocumento.prefijo,
                                        encabezado.numero,
                                        bodega.bodccs_nombre,
                                        encabezado.fecha,
                                        cliente = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                  tercero.apellido_tercero + " " + tercero.segapellido_tercero + tercero.razon_social,
                                        asesor = asesor.user_nombre + " " + asesor.user_apellido,
                                        referencia.ref_codigo,
                                        referencia.ref_descripcion,
                                        encabezado.valor_total,
                                        encabezado.iva,
                                        encabezado.valor_mercancia,
                                        encabezado.idencabezado
                                    }).ToList();

                var dataVentas = buscarVentas.Select(x => new
                {
                    x.prefijo,
                    x.bodccs_nombre,
                    x.numero,
                    fecha = x.fecha.ToShortDateString(),
                    cliente = x.cliente.Trim(),
                    x.asesor,
                    x.ref_codigo,
                    x.ref_descripcion,
                    x.valor_total,
                    x.iva,
                    x.valor_mercancia,
                    x.idencabezado
                });
                return Json(dataVentas, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult DetallesVenta(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //var detalles = from encabezado in context.encab_documento
            //               join tercero in context.icb_terceros
            //               on encabezado.nit equals tercero.
            //if (detalles == null)
            //{
            //    return HttpNotFound();
            //}
            //ViewBag.fecha = detalles.fecha.ToShortDateString();

            ViewBag.id_encabezado = id;
            return View();
        }


        public JsonResult BuscarReferenciasDeVenta(int? id)
        {
            var detalles = (from lineas in context.lineas_documento
                            join encabezado in context.encab_documento
                                on lineas.id_encabezado equals encabezado.idencabezado
                            join referencia in context.icb_referencia
                                on lineas.codigo equals referencia.ref_codigo
                            where lineas.id_encabezado == id
                            select new
                            {
                                referencia.ref_codigo,
                                referencia.ref_descripcion,
                                lineas.cantidad,
                                lineas.valor_unitario,
                                lineas.porcentaje_iva
                            }).ToList();
            var detallesCalculados = detalles.Select(x => new
            {
                x.ref_codigo,
                x.ref_descripcion,
                x.cantidad,
                x.valor_unitario,
                iva = x.valor_unitario * (decimal)x.porcentaje_iva / 100,
                valorTotal = x.valor_unitario * (decimal)x.porcentaje_iva / 100 + x.valor_unitario
            });
            return Json(detallesCalculados, JsonRequestBehavior.AllowGet);
        }
    }
}