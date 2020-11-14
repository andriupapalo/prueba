using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tesoreriaController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        private readonly CultureInfo elGr = CultureInfo.CreateSpecificCulture("el-GR");

        // GET: tesoreria/Create
        public ActionResult BrowserProveedores(int? menu)
        {
            BuscarFavoritos(menu);
            var buscarTipoDocumento = (from tipoDocumento in context.tp_doc_registros
                                       select new
                                       {
                                           tipoDocumento.sw,
                                           tipoDocumento.tpdoc_id,
                                           nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                           tipoDocumento.tipo
                                       }).ToList();
            ViewBag.tipo_documentoFiltro =
                new SelectList(buscarTipoDocumento.Where(x => x.sw == 1), "tpdoc_id", "nombre");
            return View();
        }

        public JsonResult cargarInfo()
        {
            var data = (from e in context.encab_documento
                        join t in context.tercero_proveedor
                            on e.nit equals t.tercero_id
                        join ter in context.icb_terceros
                            on t.tercero_id equals ter.tercero_id
                        join d in context.tp_doc_registros
                            on e.tipo equals d.tpdoc_id
                        where e.valor_total - e.valor_aplicado > 0 && d.sw == 1
                        select new
                        {
                            e.nit,
                            nombre = ter.prinom_tercero != null
                                ? "(" + ter.doc_tercero + ") " + ter.prinom_tercero + " " + ter.segnom_tercero + " " +
                                  ter.apellido_tercero + " " + ter.segapellido_tercero
                                : "(" + ter.doc_tercero + ") " + ter.razon_social,
                            e.valor_total,
                            e.valor_aplicado,
                            saldo = e.valor_total - e.valor_aplicado
                        }).ToList();

            var nuevo = data.GroupBy(x => x.nit).Select(grp => new
            {
                llave = grp.Key,
                nombre = grp.Select(x => x.nombre).Distinct(),
                suma = grp.Sum(x => x.saldo)
            });

            var info = nuevo.Select(x => new
            {
                x.llave,
                x.nombre,
                suma = x.suma.ToString("0,0", elGr)
            });

            return Json(info, JsonRequestBehavior.AllowGet);
        }

        public JsonResult pendientesProveedor(int nit, DateTime? desde, DateTime? hasta, int? id_documento,
            int? factura)
        {
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
                hasta = hasta.GetValueOrDefault();
            }

            List<int> listaDocumentos = new List<int>();
            if (id_documento != null)
            {
                listaDocumentos.Add(id_documento ?? 0);
            }
            else
            {
                listaDocumentos = context.tp_doc_registros.Where(x => x.sw == 1).Select(x => x.tpdoc_id).ToList();
            }

            if (desde != null && hasta != null && desde < hasta)
            {
                //hago todo el resto de funcion
                if (factura != null)
                {
                    var buscarFacturasConSaldo = (from e in context.encab_documento
                                                  join t in context.tp_doc_registros
                                                      on e.tipo equals t.tpdoc_id
                                                  join tp in context.tp_doc_registros_tipo
                                                      on t.tipo equals tp.id
                                                  where listaDocumentos.Contains(t.tpdoc_id)
                                                        && e.valor_total - e.valor_aplicado != 0
                                                        && e.fecha >= desde && e.fecha <= hasta
                                                        && e.numero == factura
                                                        && e.nit == nit
                                                        && e.vencimiento != null
                                                  select new
                                                  {
                                                      id = e.idencabezado,
                                                      e.fecha,
                                                      e.valor_aplicado,
                                                      e.valor_total,
                                                      e.numero,
                                                      e.vencimiento,
                                                      idTipo = t.tpdoc_id,
                                                      tipo = "(" + t.prefijo + ") " + t.tpdoc_nombre,
                                                      saldo = e.valor_total - e.valor_aplicado,
                                                      descripcion = t.tpdoc_nombre,
                                                      numeroFactura = e.numero,
                                                      prefijo = e.idencabezado,
                                                      tp = e.tipo
                                                  }).ToList();

                    var data = buscarFacturasConSaldo.Select(x => new
                    {
                        x.id,
                        fecha = x.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                        x.valor_aplicado,
                        x.valor_total,
                        x.numero,
                        vencimiento = x.vencimiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                        x.idTipo,
                        x.tipo,
                        x.saldo,
                        x.descripcion,
                        x.numeroFactura,
                        x.prefijo,
                        x.tp
                    });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var buscarFacturasConSaldo = (from e in context.encab_documento
                                                  join t in context.tp_doc_registros
                                                      on e.tipo equals t.tpdoc_id
                                                  join tp in context.tp_doc_registros_tipo
                                                      on t.tipo equals tp.id
                                                  where listaDocumentos.Contains(t.tpdoc_id)
                                                        && e.valor_total - e.valor_aplicado != 0
                                                        && e.fecha >= desde && e.fecha <= hasta
                                                        && e.nit == nit
                                                        && e.vencimiento != null
                                                  select new
                                                  {
                                                      id = e.idencabezado,
                                                      e.fecha,
                                                      e.valor_aplicado,
                                                      e.valor_total,
                                                      e.numero,
                                                      e.vencimiento,
                                                      idTipo = t.tpdoc_id,
                                                      tipo = "(" + t.prefijo + ") " + t.tpdoc_nombre,
                                                      saldo = e.valor_total - e.valor_aplicado,
                                                      descripcion = t.tpdoc_nombre,
                                                      numeroFactura = e.numero,
                                                      t.prefijo,
                                                      tp = e.tipo
                                                  }).ToList();

                    var data = buscarFacturasConSaldo.Select(x => new
                    {
                        x.id,
                        fecha = x.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                        x.valor_aplicado,
                        x.valor_total,
                        x.numero,
                        vencimiento = x.vencimiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                        x.idTipo,
                        x.tipo,
                        x.saldo,
                        x.descripcion,
                        x.numeroFactura,
                        x.prefijo,
                        x.tp
                    });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(0, JsonRequestBehavior.AllowGet);
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