using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class ventaTipoClienteController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: ventaTipoCliente
        public ActionResult Index(int? menu)
        {
            ViewBag.tp_doc_registros = context.tp_doc_registros.Where(x => x.tipo == 2).ToList();
            ViewBag.tipo_cliente = new SelectList(context.tipocliente, "tipo", "nombre");
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult BuscarVentasPorTipoCliente(int? tpDocumento, DateTime? desde, DateTime? hasta,
            int? tipoCliente)
        {
            List<int> listaTipoDoc = new List<int>();
            List<int> listaTipoCliente = new List<int>();
            if (tpDocumento == null)
            {
                listaTipoDoc = context.tp_doc_registros.Where(x => x.tipo == 2).Select(x => x.tpdoc_id).ToList();
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

            if (tipoCliente == null)
            {
                var buscarVentas = (from encabezado in context.encab_documento
                                    join tipoDocumento in context.tp_doc_registros
                                        on encabezado.tipo equals tipoDocumento.tpdoc_id
                                    join bodega in context.bodega_concesionario
                                        on encabezado.bodega equals bodega.id
                                    join vehiculo in context.icb_vehiculo
                                        on encabezado.documento equals vehiculo.plan_mayor
                                    join modelo in context.modelo_vehiculo
                                        on vehiculo.modvh_id equals modelo.modvh_codigo
                                    join color in context.color_vehiculo
                                        on vehiculo.colvh_id equals color.colvh_id
                                    join tercero in context.icb_terceros
                                        on encabezado.nit equals tercero.tercero_id into ter
                                    from tercero in ter.DefaultIfEmpty()
                                    join terCliente in context.tercero_cliente
                                        on tercero.tercero_id equals terCliente.tercero_id into terCli
                                    from terCliente in terCli.DefaultIfEmpty()
                                    join tipoClienteAux in context.tipocliente
                                        on terCliente.tipo_cliente equals tipoClienteAux.tipo into tpCli
                                    from tipoClienteAux in tpCli.DefaultIfEmpty()
                                    where listaTipoDoc.Contains(encabezado.tipo)
                                          && encabezado.fecha >= desde
                                          && encabezado.fecha <= hasta
                                    select new
                                    {
                                        tipoDocumento.prefijo,
                                        encabezado.numero,
                                        bodega.bodccs_nombre,
                                        encabezado.fecha,
                                        encabezado.documento,
                                        cliente = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                  tercero.apellido_tercero + " " + tercero.segapellido_tercero + tercero.razon_social,
                                        tipoClienteAux.nombre,
                                        modelo.modvh_nombre,
                                        color.colvh_nombre,
                                        encabezado.valor_total
                                    }).ToList();

                var dataVentas = buscarVentas.Select(x => new
                {
                    x.prefijo,
                    x.bodccs_nombre,
                    x.numero,
                    fecha = x.fecha.ToShortDateString(),
                    plan_mayor = x.documento,
                    cliente = x.cliente.Trim(),
                    tipo_cliente = x.nombre != null ? x.nombre : "",
                    x.modvh_nombre,
                    x.colvh_nombre,
                    x.valor_total
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
                                    join vehiculo in context.icb_vehiculo
                                        on encabezado.documento equals vehiculo.plan_mayor
                                    join modelo in context.modelo_vehiculo
                                        on vehiculo.modvh_id equals modelo.modvh_codigo
                                    join color in context.color_vehiculo
                                        on vehiculo.colvh_id equals color.colvh_id
                                    join tercero in context.icb_terceros
                                        on encabezado.nit equals tercero.tercero_id into ter
                                    from tercero in ter.DefaultIfEmpty()
                                    join terCliente in context.tercero_cliente
                                        on tercero.tercero_id equals terCliente.tercero_id into terCli
                                    from terCliente in terCli.DefaultIfEmpty()
                                    join tipoClienteAux in context.tipocliente
                                        on terCliente.tipo_cliente equals tipoClienteAux.tipo into tpCli
                                    from tipoClienteAux in tpCli.DefaultIfEmpty()
                                    where listaTipoDoc.Contains(encabezado.tipo)
                                          && terCliente.tipo_cliente == tipoCliente
                                          && encabezado.fecha >= desde
                                          && encabezado.fecha <= hasta
                                    select new
                                    {
                                        tipoDocumento.prefijo,
                                        bodega.bodccs_nombre,
                                        encabezado.fecha,
                                        encabezado.documento,
                                        encabezado.numero,
                                        cliente = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                  tercero.apellido_tercero + " " + tercero.segapellido_tercero + tercero.razon_social,
                                        tipoClienteAux.nombre,
                                        modelo.modvh_nombre,
                                        color.colvh_nombre,
                                        encabezado.valor_total
                                    }).ToList();
                var dataVentas = buscarVentas.Select(x => new
                {
                    x.prefijo,
                    x.bodccs_nombre,
                    x.numero,
                    fecha = x.fecha.ToShortDateString(),
                    plan_mayor = x.documento,
                    cliente = x.cliente.Trim(),
                    tipo_cliente = x.nombre != null ? x.nombre : "",
                    x.modvh_nombre,
                    x.colvh_nombre,
                    x.valor_total
                });
                return Json(dataVentas, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ConsultaPorTercero(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult BuscarDatosPorTercero(DateTime? desde, DateTime? hasta)
        {
            if (desde != null && hasta != null)
            {
                var datos = (from d in context.encab_documento
                             join td in context.tp_doc_registros_tipo
                                 on d.tipo equals td.id
                             join i in context.icb_terceros
                                 on d.nit equals i.tercero_id
                             where d.fecha >= desde && d.fecha <= hasta
                             group d by new { d.icb_terceros }
                    into grupo
                             select new
                             {
                                 id = grupo.Key.icb_terceros.tercero_id,
                                 documento = grupo.Key.icb_terceros.doc_tercero,
                                 cliente = grupo.Key.icb_terceros.prinom_tercero + " " + grupo.Key.icb_terceros.segnom_tercero +
                                           " " + grupo.Key.icb_terceros.apellido_tercero + " " +
                                           grupo.Key.icb_terceros.segapellido_tercero + " " +
                                           grupo.Key.icb_terceros.razon_social,
                                 valor_total = grupo.Sum(x => x.valor_total),
                                 valor_aplicado = grupo.Sum(x => x.valor_aplicado),
                                 saldo = grupo.Sum(x => x.valor_total) - grupo.Sum(x => x.valor_aplicado)
                             }).ToList();

                return Json(datos, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var datos = (from d in context.encab_documento
                             join td in context.tp_doc_registros_tipo
                                 on d.tipo equals td.id
                             group d by new { d.icb_terceros }
                    into grupo
                             select new
                             {
                                 id = grupo.Key.icb_terceros.tercero_id,
                                 documento = grupo.Key.icb_terceros.doc_tercero,
                                 cliente = grupo.Key.icb_terceros.prinom_tercero + " " + grupo.Key.icb_terceros.segnom_tercero +
                                           " " + grupo.Key.icb_terceros.apellido_tercero + " " +
                                           grupo.Key.icb_terceros.segapellido_tercero + " " +
                                           grupo.Key.icb_terceros.razon_social,
                                 valor_total = grupo.Sum(x => x.valor_total),
                                 valor_aplicado = grupo.Sum(x => x.valor_aplicado),
                                 saldo = grupo.Sum(x => x.valor_total) - grupo.Sum(x => x.valor_aplicado)
                             }).ToList();


                return Json(datos, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult BuscarDetallesTercero(int id, DateTime? desde, DateTime? hasta)
        {
            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");


            if (desde != null && hasta != null)
            {
                var datos = (from d in context.encab_documento
                             join td in context.tp_doc_registros
                                 on d.tipo equals td.tpdoc_id
                             join f in context.fpago_tercero
                                 on d.fpago_id equals f.fpago_id
                             join b in context.bodega_concesionario
                                 on d.bodega equals b.bodccscentro_id
                             where d.nit == id
                                   && d.fecha >= desde && d.fecha <= hasta
                             select new
                             {
                                 descripcion = td.tpdoc_nombre,
                                 td.prefijo,
                                 d.numero,
                                 bodega = b.bodccs_nombre,
                                 fecha = d.fecha.ToString(),
                                 fpago = f.fpago_nombre,
                                 vencimiento = d.vencimiento.ToString(),
                                 d.valor_total,
                                 valor_iva = d.iva,
                                 d.retencion,
                                 rete_causada = d.retencion_causada,
                                 rete_iva = d.retencion_iva,
                                 rete_ica = d.retencion_ica
                             }).ToList();


                return Json(datos, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var datos = (from d in context.encab_documento
                             join td in context.tp_doc_registros
                                 on d.tipo equals td.tpdoc_id
                             join f in context.fpago_tercero
                                 on d.fpago_id equals f.fpago_id
                             join b in context.bodega_concesionario
                                 on d.bodega equals b.bodccscentro_id
                             where d.nit == id
                             select new
                             {
                                 descripcion = td.tpdoc_nombre,
                                 td.prefijo,
                                 d.numero,
                                 bodega = b.bodccs_nombre,
                                 fecha = d.fecha.ToString(),
                                 fpago = f.fpago_nombre,
                                 vencimiento = d.vencimiento.ToString(),
                                 d.valor_total,
                                 valor_iva = d.iva,
                                 d.retencion,
                                 rete_causada = d.retencion_causada,
                                 rete_iva = d.retencion_iva,
                                 rete_ica = d.retencion_ica
                             }).ToList();


                return Json(datos, JsonRequestBehavior.AllowGet);
            }
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