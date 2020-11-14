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
using System.Threading;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    // alex
    public class registroReferenciasController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        private readonly CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
        private CultureInfo Cultura = new CultureInfo("is-IS");
        private CultureInfo miCultura = new CultureInfo("is-IS");

        private static Expression<Func<vw_browser_referencias, string>> GetColumnName(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(vw_browser_referencias), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<vw_browser_referencias, string>> lambda = Expression.Lambda<Func<vw_browser_referencias, string>>(menuProperty, menu);

            return lambda;
        }

        public void listas(RegistroReferenciasModel post)
        {
            //var linea = Convert.ToInt32(post.linea_id);
            var list = (from t in context.icb_terceros
                        join tp in context.tercero_proveedor
                            on t.tercero_id equals tp.tercero_id
                        where tp.tercpro_estado
                        select new
                        {
                            t.tercero_id,
                            t.doc_tercero,
                            t.prinom_tercero,
                            t.apellido_tercero,
                            t.razon_social
                        }).OrderBy(x => x.doc_tercero).OrderBy(x => x.prinom_tercero).OrderBy(x => x.razon_social).ToList()
                .OrderBy(x => x.apellido_tercero);


            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in list)
            {
                lista.Add(new SelectListItem
                {
                    Text = item.prinom_tercero != null
                        ? item.doc_tercero + ' ' + item.prinom_tercero + ' ' + item.apellido_tercero
                        : item.doc_tercero + ' ' + item.razon_social,
                    Value = item.tercero_id.ToString(),
                });
            }

            var referencias = (from u in context.icb_referencia
                               join a in context.rremplazos
                                   on u.ref_codigo equals a.referencia into zz
                               from a in zz.DefaultIfEmpty()
                               where u.ref_estado && u.ref_codigo != post.ref_codigo && u.ref_codigo != a.referencia
                               orderby u.ref_descripcion
                               select new
                               {
                                   nombre = "(" + u.ref_codigo + ") - " + u.ref_descripcion,
                                   u.ref_codigo
                               }).ToList();
            List<SelectListItem> referencia = new List<SelectListItem>();
            foreach (var item in referencias)
            {
                referencia.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.ref_codigo
                });
            }

            int? proveedorDefault = post.proveedor_ppal;
            if (proveedorDefault == null)
            {
                int numero = 0;
                icb_sysparameter resultado = context.icb_sysparameter.Where(d => d.syspar_cod == "P138").FirstOrDefault();
                if (resultado != null)
                {
                    bool convertir = int.TryParse(resultado.syspar_value, out numero);
                    proveedorDefault = numero;
                }
            }
            ViewBag.proveedor_ppal = new SelectList(lista, "Value", "Text", proveedorDefault);
            ViewBag.clasificacion_id =
                new SelectList(
                    context.clasificacion_repuesto.OrderBy(x => x.clarpto_nombre).Where(x => x.clarpto_estado),
                    "clarpto_id", "clarpto_nombre", post.clasificacion_id);
            ViewBag.tipo_id =
                new SelectList(context.grupo_repuesto.OrderBy(x => x.grupo_nombre).Where(x => x.grupo_estado),
                    "grupo_id", "grupo_nombre", post.tipo_id);
            ViewBag.linea_id = new SelectList(context.ref_linea.OrderBy(x => x.Descripcion).Where(x => x.estado),
                "codigo", "Descripcion", post.linea_id);
            ViewBag.grupo_id = new SelectList(context.ref_grupo.OrderBy(x => x.Descripcion).Where(x => x.estado),
                "codigo", "Descripcion", post.grupo_id);
            ViewBag.familia_id = new SelectList(context.ref_familia.OrderBy(x => x.Descripcion).Where(x => x.estado),
                "codigo", "Descripcion", post.familia_id);
            ViewBag.unidad_medida = new SelectList(context.unidad_medida.OrderBy(x => x.nombre).Where(x => x.estado),
                "nombre", "nombre", post.unidad_medida);
            ViewBag.por_iva =
                new SelectList(context.codigo_iva.OrderBy(x => x.porcentaje).Where(x => x.Descripcion == "VENTA"), "id",
                    "Porcentaje", post.idporivaventa);
            ViewBag.por_iva_compra =
                new SelectList(context.codigo_iva.OrderBy(x => x.porcentaje).Where(x => x.Descripcion == "COMPRA"),
                    "id", "Porcentaje", post.idporivacompra);
            ViewBag.perfil =
                new SelectList(context.perfilcontable_referencia.OrderBy(x => x.descripcion).Where(x => x.estado), "id",
                    "descripcion", post.perfil);
            ViewBag.cargarReferencia = referencia;

            //ViewBag.tipo_id = new SelectList(context.grupo_repuesto.OrderBy(x => x.grupo_nombre).Where(x => x.grupo_estado != false), "grupo_id", "grupo_nombre", post.tipo_id);
            //ViewBag.linea_id = new SelectList(context.ref_linea.OrderBy(x => x.Descripcion).Where(x => x.estado != false), "codigo", "Descripcion", post.linea_id);
            //ViewBag.grupo_id = new SelectList(context.ref_grupo.OrderBy(x => x.Descripcion).Where(x => x.estado != false), "codigo", "Descripcion", post.grupo_id);
            //ViewBag.familia_id = new SelectList(context.ref_familia.OrderBy(x => x.Descripcion).Where(x => x.estado != false), "codigo", "Descripcion", post.familia_id);

            ViewBag.subgrupo = post.subgrupo;
        }

        public ActionResult consultaTerceros()
        {
            var buscarClientes = (from t in context.icb_terceros
                                  join c in context.tercero_cliente
                                      on t.tercero_id equals c.tercero_id
                                  select new
                                  {
                                      value = t.tercero_id,
                                      text = t.doc_tercero + " " + t.razon_social + " " + t.prinom_tercero + " " + t.segnom_tercero +
                                             " " + t.apellido_tercero + " " + t.segapellido_tercero
                                  }).ToList();

            var referencias = (from r in context.icb_referencia
                               where r.modulo == "R"
                               select new
                               {
                                   value = r.ref_codigo,
                                   text = r.ref_codigo + " " + r.ref_descripcion
                               }).ToList();

            ViewBag.referencias = new SelectList(referencias, "value", "text");
            ViewBag.terceros = new SelectList(buscarClientes, "value", "text");
            return View();
        }

        public JsonResult BuscarHistoricoTercero(int? id_tercero, string ref_codigo)
        {
            var buscarCotizaciones = (from a in context.tp_doc_registros
                                      join b in context.tp_doc_registros_tipo
                                          on a.tipo equals b.id
                                      join c in context.icb_referencia_mov
                                          on a.tpdoc_id equals c.tpdocid
                                      join d in context.icb_referencia_movdetalle
                                          on c.refmov_id equals d.refmov_id
                                      join t in context.icb_terceros
                                          on c.cliente equals t.tercero_id
                                      join r in context.icb_referencia
                                          on d.ref_codigo equals r.ref_codigo
                                      where b.id == 25
                                      select new
                                      {
                                          c.refmov_numero,
                                          cliente = t.razon_social + t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero +
                                                    " " + t.segapellido_tercero,
                                          documento = t.doc_tercero,
                                          fecha = c.refmov_fecela,
                                          referencia = d.ref_codigo + " " + r.ref_descripcion,
                                          cantidad = d.refdet_cantidad,
                                          precio = d.valor_total,
                                          descuento = c.valordescuento,
                                          iva = c.valoriva,
                                          c.valor_total,
                                          tercero = c.cliente,
                                          d.ref_codigo
                                      }).ToList();

            var buscarPedidos = (from a in context.tp_doc_registros
                                 join b in context.tp_doc_registros_tipo
                                     on a.tipo equals b.id
                                 join c in context.icb_referencia_mov
                                     on a.tpdoc_id equals c.tpdocid
                                 join d in context.icb_referencia_movdetalle
                                     on c.refmov_id equals d.refmov_id
                                 join t in context.icb_terceros
                                     on c.cliente equals t.tercero_id
                                 join r in context.icb_referencia
                                     on d.ref_codigo equals r.ref_codigo
                                 where b.id == 27
                                 select new
                                 {
                                     c.refmov_numero,
                                     cliente = t.razon_social + t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero +
                                               " " + t.segapellido_tercero,
                                     documento = t.doc_tercero,
                                     fecha = c.refmov_fecela,
                                     referencia = d.ref_codigo + " " + r.ref_descripcion,
                                     cantidad = d.refdet_cantidad,
                                     precio = d.valor_total,
                                     descuento = c.valordescuento,
                                     iva = c.valoriva,
                                     c.valor_total,
                                     tercero = c.cliente,
                                     d.ref_codigo
                                 }).ToList();

            var buscarFacturados = (from a in context.encab_documento
                                    join b in context.tp_doc_registros
                                        on a.tipo equals b.tpdoc_id
                                    join c in context.tp_doc_registros_tipo
                                        on b.tipo equals c.id
                                    join t in context.icb_terceros
                                        on a.nit equals t.tercero_id
                                    join l in context.lineas_documento
                                        on a.idencabezado equals l.id_encabezado
                                    join r in context.icb_referencia
                                        on l.codigo equals r.ref_codigo
                                    where c.id == 4
                                    select new
                                    {
                                        a.numero,
                                        cliente = t.razon_social + t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero +
                                                  " " + t.segapellido_tercero,
                                        documento = t.doc_tercero,
                                        a.fecha,
                                        referencia = r.ref_codigo + " " + r.ref_descripcion,
                                        l.cantidad,
                                        valorUnitario = l.valor_unitario,
                                        descuento = l.porcentaje_descuento,
                                        iva = l.porcentaje_iva,
                                        tercero = a.nit,
                                        r.ref_codigo
                                    }).ToList();

            var buscarDevolucionFacturados = (from a in context.encab_documento
                                              join b in context.tp_doc_registros
                                                  on a.tipo equals b.tpdoc_id
                                              join c in context.tp_doc_registros_tipo
                                                  on b.tipo equals c.id
                                              join t in context.icb_terceros
                                                  on a.nit equals t.tercero_id
                                              join l in context.lineas_documento
                                                  on a.idencabezado equals l.id_encabezado
                                              join r in context.icb_referencia
                                                  on l.codigo equals r.ref_codigo
                                              where c.id == 18
                                              select new
                                              {
                                                  a.numero,
                                                  cliente = t.razon_social + t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero +
                                                            " " + t.segapellido_tercero,
                                                  documento = t.doc_tercero,
                                                  a.fecha,
                                                  referencia = r.ref_codigo + " " + r.ref_descripcion,
                                                  l.cantidad,
                                                  valorUnitario = l.valor_unitario,
                                                  descuento = l.porcentaje_descuento,
                                                  iva = l.porcentaje_iva,
                                                  tercero = a.nit,
                                                  r.ref_codigo
                                              }).ToList();


            var pedido = buscarPedidos.Select(x => new
            {
                numero = x.refmov_numero.ToString(),
                x.cliente,
                x.documento,
                fecha = x.fecha.ToString("yyyy/MM/dd"),
                x.referencia,
                x.cantidad,
                precio = x.precio.Value.ToString("0,0", elGR),
                descuento = x.descuento.ToString("0,0", elGR),
                iva = x.iva.ToString("0,0", elGR),
                valor_total = x.valor_total.ToString("0,0", elGR),
                x.tercero,
                x.ref_codigo
            });
            var cotizacion = buscarCotizaciones.Select(x => new
            {
                numero = x.refmov_numero.ToString(),
                x.cliente,
                x.documento,
                fecha = x.fecha.ToString("yyyy/MM/dd"),
                x.referencia,
                x.cantidad,
                precio = x.precio.Value.ToString("0,0", elGR),
                descuento = x.descuento.ToString("0,0", elGR),
                iva = x.iva.ToString("0,0", elGR),
                valor_total = x.valor_total.ToString("0,0", elGR),
                x.tercero,
                x.ref_codigo
            });
            var facturas = buscarFacturados.Select(x => new
            {
                numero = x.numero.ToString(),
                x.cliente,
                x.documento,
                fecha = x.fecha.ToString("yyyy/MM/dd"),
                x.referencia,
                x.cantidad,
                precio = (x.cantidad * x.valorUnitario).ToString("0,0", elGR),
                descuento = (x.cantidad * x.valorUnitario * Convert.ToDecimal(x.descuento, miCultura) / 100).ToString("0,0", elGR),
                iva = (x.cantidad * x.valorUnitario * Convert.ToDecimal(x.iva, miCultura) / 100).ToString("0,0", elGR),
                valor_total =
                    (x.cantidad * x.valorUnitario + x.cantidad * x.valorUnitario * Convert.ToDecimal(x.iva,miCultura) / 100 -
                     x.cantidad * x.valorUnitario * Convert.ToDecimal(x.descuento, miCultura) / 100).ToString("0,0", elGR),
                x.tercero,
                x.ref_codigo
            });
            var devolucionfacturas = buscarDevolucionFacturados.Select(x => new
            {
                numero = x.numero.ToString(),
                x.cliente,
                x.documento,
                fecha = x.fecha.ToString("yyyy/MM/dd"),
                x.referencia,
                x.cantidad,
                precio = (x.cantidad * x.valorUnitario).ToString("0,0", elGR),
                descuento = (x.cantidad * x.valorUnitario * Convert.ToDecimal(x.descuento, miCultura) / 100).ToString("0,0", elGR),
                iva = (x.cantidad * x.valorUnitario * Convert.ToDecimal(x.iva, miCultura) / 100).ToString("0,0", elGR),
                valor_total =
                    (x.cantidad * x.valorUnitario + x.cantidad * x.valorUnitario * Convert.ToDecimal(x.iva, miCultura) / 100 -
                     x.cantidad * x.valorUnitario * Convert.ToDecimal(x.descuento, miCultura) / 100).ToString("0,0", elGR),
                x.tercero,
                x.ref_codigo
            });

            if (id_tercero != null && ref_codigo == "")
            {
                cotizacion = cotizacion.Where(x => x.tercero == id_tercero);
                pedido = pedido.Where(x => x.tercero == id_tercero);
                facturas = facturas.Where(x => x.tercero == id_tercero);
                devolucionfacturas = devolucionfacturas.Where(x => x.tercero == id_tercero);
            }
            else if (id_tercero == null && ref_codigo != "")
            {
                cotizacion = cotizacion.Where(x => x.ref_codigo == ref_codigo);
                pedido = pedido.Where(x => x.ref_codigo == ref_codigo);
                facturas = facturas.Where(x => x.ref_codigo == ref_codigo);
                devolucionfacturas = devolucionfacturas.Where(x => x.ref_codigo == ref_codigo);
            }
            else if (id_tercero != null && ref_codigo != "")
            {
                cotizacion = cotizacion.Where(x => x.ref_codigo == ref_codigo && x.tercero == id_tercero);
                pedido = pedido.Where(x => x.tercero == id_tercero && x.ref_codigo == ref_codigo);
                facturas = facturas.Where(x => x.tercero == id_tercero && x.ref_codigo == ref_codigo);
                devolucionfacturas =
                    devolucionfacturas.Where(x => x.tercero == id_tercero && x.ref_codigo == ref_codigo);
            }

            var data = new
            {
                cotizacion,
                pedido,
                facturas,
                devolucionfacturas
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        // GET: 
        public ActionResult Crear(int? menu)
        {

            RegistroReferenciasModel refer = new RegistroReferenciasModel();
            listas(refer);
            BuscarFavoritos(menu);
            return View(new RegistroReferenciasModel { manejo_inv = true, ref_estado = true });
        }

        [HttpPost]
        //POST: crear
        public ActionResult Crear(RegistroReferenciasModel post, int? menu)
        {
            float? textoIva;
            float? textoIvaCompra;
            string porIVA = Request["textIVAVenta"];
            string porIVAcompra = Request["textIVACompra"];
            int idIva = 0;
            int idIvaCompra = 0;

            if (porIVA == "")
            {
                textoIva = null;
                idIva = 4;
            }
            else
            {
                textoIva = float.Parse(Request["textIVAVenta"]);
                idIva = Convert.ToInt32(post.por_iva);
            }

            if (porIVAcompra == "")
            {
                textoIvaCompra = null;
                idIvaCompra = 1;
            }
            else
            {
                textoIvaCompra = float.Parse(Request["textIVAVenta"]);
                idIvaCompra = Convert.ToInt32(post.por_iva_compra);
            }

            if (ModelState.IsValid)
            {
                icb_referencia buscarCodigo = context.icb_referencia.FirstOrDefault(x => x.ref_codigo == post.ref_codigo);
                if (buscarCodigo == null)
                {
                    icb_referencia ir = new icb_referencia
                    {
                        ref_codigo = post.ref_codigo,
                        ref_descripcion = post.ref_descripcion,
                        alias = post.ref_alternativa ?? "",
                        ref_fecha_creacion = DateTime.Now,
                        ref_usuario_actualizacion = Convert.ToInt32(Session["user_usuarioid"]),
                        grupo_id = post.grupo_id,
                        clasificacion_id = post.clasificacion_id,
                        ref_razon_inactivo = post.ref_razon_inactivo,
                        ref_cantidad_min = post.ref_cantidad_min ?? 0,
                        ref_cantidad_max = post.ref_cantidad_max ?? 0,
                        ref_valor_total = post.ref_valor_total,
                        subgrupo = post.subgrupo,
                        perfil = post.perfil,
                        costo_unitario = 0,
                        manejo_inv = post.manejo_inv,
                        unidad_medida = post.unidad_medida,
                        costo_emergencia = Convert.ToDecimal(post.costo_emergencia, miCultura),
                        ref_valor_unitario = Convert.ToDecimal(post.ref_valor_unitario, miCultura),
                        precio_alterno = !string.IsNullOrEmpty(post.precio_alterno)
                            ? Convert.ToDecimal(post.precio_alterno, miCultura)
                            : 0,
                        precio_garantia = !string.IsNullOrEmpty(post.precio_garantia)
                            ? Convert.ToDecimal(post.precio_garantia, miCultura)
                            : 0,
                        precio_diesel = !string.IsNullOrEmpty(post.precio_diesel)
                            ? Convert.ToDecimal(post.precio_diesel, miCultura)
                            : 0,
                        precio_venta = Convert.ToDecimal(post.ref_valor_unitario, miCultura),
                        clasificacion_ABC = post.clasificacion_ABC,
                        proveedor_ppal = post.proveedor_ppal,
                        por_dscto = post.por_dscto ?? 0,
                        por_dscto_max = post.por_dscto_max ?? 0,
                        vida_util = post.vida_util,
                        linea_id = Convert.ToString(post.linea_id),
                        tipo_id = post.tipo_id,
                        familia_id = post.familia_id,
                        modulo = "R",
                        ref_estado = post.ref_estado,
                        por_iva = textoIva ?? 0,
                        idporivaventa = idIva,
                        idporivacompra = idIvaCompra,
                        por_iva_compra = textoIvaCompra ?? 0,
                        costo_promedio= post.ref_valor_total,
                    };
                    ir.clasificacion_ABC = "C";

                    context.icb_referencia.Add(ir);

                    decimal precio1 = !string.IsNullOrEmpty(post.ref_valor_unitario)
                        ? Convert.ToDecimal(post.ref_valor_unitario, miCultura)
                        : 0;
                    decimal precio2 = !string.IsNullOrEmpty(Request["precio_2"])
                        ? Convert.ToDecimal(Request["precio_2"], miCultura)
                        : 0;
                    decimal precio3 = !string.IsNullOrEmpty(Request["precio_3"])
                        ? Convert.ToDecimal(Request["precio_3"], miCultura)
                        : 0;
                    decimal precio4 = !string.IsNullOrEmpty(Request["precio_4"])
                        ? Convert.ToDecimal(Request["precio_4"], miCultura)
                        : 0;
                    decimal precio5 = !string.IsNullOrEmpty(Request["precio_5"])
                        ? Convert.ToDecimal(Request["precio_5"], miCultura)
                        : 0;
                    decimal precio6 = !string.IsNullOrEmpty(Request["precio_6"])
                        ? Convert.ToDecimal(Request["precio_6"], miCultura)
                        : 0;
                    decimal precio7 = !string.IsNullOrEmpty(Request["precio_7"])
                        ? Convert.ToDecimal(Request["precio_7"], miCultura)
                        : 0;
                    decimal precio8 = !string.IsNullOrEmpty(Request["precio_8"])
                        ? Convert.ToDecimal(Request["precio_8"], miCultura)
                        : 0;
                    decimal precio9 = !string.IsNullOrEmpty(Request["precio_9"])
                        ? Convert.ToDecimal(Request["precio_9"], miCultura)
                        : 0;
                    decimal precioGarantia = !string.IsNullOrEmpty(post.precio_garantia)
                        ? Convert.ToDecimal(post.precio_garantia, miCultura)
                        : 0;
                    if (ir.costo_unitario != 0)
                    {
                        rprecios buscarCod = context.rprecios.FirstOrDefault(x => x.codigo == post.ref_codigo);
                        if (buscarCod == null)
                        {
                            if (precio1 != 0 && precio1 < ir.costo_unitario ||
                                precio2 != 0 && precio2 < ir.costo_unitario ||
                                precio3 != 0 && precio3 < ir.costo_unitario ||
                                precio4 != 0 && precio4 < ir.costo_unitario ||
                                precio5 != 0 && precio5 < ir.costo_unitario ||
                                precio6 != 0 && precio6 < ir.costo_unitario ||
                                precio7 != 0 && precio7 < ir.costo_unitario ||
                                precio8 != 0 && precio8 < ir.costo_unitario ||
                                precio9 != 0 && precio9 < ir.costo_unitario)
                            {
                                TempData["mensaje_error"] =
                                    "Alguno de los precios de la referencia es menor que el costo, por favor valide";
                                listas(post);
                                ViewBag.subgrupo = post.subgrupo;
                                BuscarFavoritos(menu);
                                return View();
                            }

                            rprecios cod = new rprecios
                            {
                                codigo = post.ref_codigo,
                                fec_creacion = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                precio1 = precio1,
                                precio2 = precio2,
                                precio3 = precio3,
                                precio4 = precio4,
                                precio5 = precio5,
                                precio6 = precio6,
                                precio7 = precio7,
                                precio8 = precio8,
                                precio9 = precio9,
                                precio_garantia = precioGarantia
                            };
                            context.rprecios.Add(cod);
                        }
                    }
                    else
                    {
                        rprecios cod = new rprecios
                        {
                            codigo = post.ref_codigo,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            precio1 = precio1,
                            precio2 = precio2,
                            precio3 = precio3,
                            precio4 = precio4,
                            precio5 = precio5,
                            precio6 = precio6,
                            precio7 = precio7,
                            precio8 = precio8,
                            precio9 = precio9,
                            precio_garantia = precioGarantia
                        };
                        context.rprecios.Add(cod);
                    }


                    #region comentarios

                    ////ir.precio_venta = (!string.IsNullOrEmpty(Request["precio_venta"])) ? Convert.ToDecimal(Request["precio_venta"]) : 0;
                    ////ir.costo_unitario = (!string.IsNullOrEmpty(Request["costo_unitario_1"])) ? Convert.ToDecimal(Request["costo_unitario_1"]) : 0;
                    //ir.ref_cantidad_min = (!string.IsNullOrEmpty(Request["ref_cantidad_min_1"])) ? Convert.ToInt32(Request["ref_cantidad_min"]) : 0;
                    //ir.costo_emergencia = (!string.IsNullOrEmpty(Request["costo_emergencia_1"])) ? Convert.ToDecimal(Request["costo_emergencia_1"]) : 0;
                    ////ir.ref_cantidad_max = (!string.IsNullOrEmpty(Request["ref_cantidad_max_1"])) ? Convert.ToInt32(Request["ref_cantidad_max"]) : 0;
                    //ir.proveedor_ppal = post.proveedor_ppal;
                    //ir.vida_util = post.vida_util;
                    //ir.clasificacion_ABC = post.clasificacion_ABC;
                    //ir.ref_stock = 0;
                    //ir.perfil = post.perfil;
                    //ir.manejo_inv = post.manejo_inv;
                    //ir.ref_estado = post.ref_estado;
                    //ir.ref_razon_inactivo = post.ref_razon_inactivo;
                    //ir.ref_fecha_creacion = DateTime.Now;
                    //ir.ref_usuario_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    //ir.modulo = "R";
                    //ir.ref_valor_total = Convert.ToDecimal(Request["ref_valor_unitario"]) * post.ref_stock;

                    //ir.subgrupo = Request["subgrupo"];
                    //ir.tipo_id = post.tipo_id;
                    //ir.clasificacion_id = post.clasificacion_id;
                    //ir.familia_id = post.familia_id;
                    //ir.grupo_id = post.grupo_id;
                    //ir.linea_id = post.linea_id;

                    #endregion

                    context.SaveChanges();
                    TempData["mensaje"] = "La referencia " + post.ref_codigo + " se creo correctamente";
                    listas(post);
                    ViewBag.subgrupo = post.subgrupo;
                    BuscarFavoritos(menu);
                    return RedirectToAction("Crear", new { menu });
                }

                TempData["mensaje_error"] = "La referencia " + post.ref_codigo + " ya existe";
            }
            else
            {
                TempData["mensaje_error"] = "No fue posible guardar el registro, por favor valide";
                List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();
            }

            listas(post);
            ViewBag.subgrupo = post.subgrupo;
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult Editar(string id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_referencia referencia = context.icb_referencia.Where(d => d.ref_codigo == id).FirstOrDefault();
            if (referencia == null)
            {
                return HttpNotFound();
            }

            //consulta el nombre de usuario con el id, lo envia a la vista a traves de ViewBag
            string creator = context.users.Where(x => x.user_id == referencia.ref_usuario_creacion)
                .Select(nombre => nombre.user_nombre + " " + nombre.user_apellido).FirstOrDefault();
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator;
                ViewBag.ref_fecha_creacion = referencia.ref_fecha_creacion;
            }

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            string modificator = context.users.Where(x => x.user_id == referencia.ref_usuario_creacion)
                .Select(nombre => nombre.user_nombre + " " + nombre.user_apellido).FirstOrDefault();

            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator;
                ViewBag.ref_fecha_actualizacion = referencia.ref_fecha_actualizacion;
            }

            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from a in context.users
                                        join b in context.icb_referencia on a.user_id equals b.ref_usuario_creacion
                                        where b.ref_codigo == id
                                        select a.user_nombre;

            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }

            cargarPrecios(id);

            var list = (from t in context.icb_terceros
                        join tp in context.tercero_proveedor
                            on t.tercero_id equals tp.tercero_id
                        join i in context.icb_referencia
                            on tp.tercero_id equals i.proveedor_ppal
                        where tp.tercpro_estado
                        select new
                        {
                            t.tercero_id,
                            t.doc_tercero,
                            t.prinom_tercero,
                            t.apellido_tercero,
                            t.razon_social
                        }).OrderBy(x => x.doc_tercero).Distinct().ToList();

            var lista = list.Select(d => new
            {
                Text = !string.IsNullOrWhiteSpace(d.razon_social)
                    ? d.doc_tercero + ' ' + d.razon_social
                    : d.doc_tercero + (!string.IsNullOrWhiteSpace(d.prinom_tercero) ? ' ' + d.prinom_tercero : "") +
                      (!string.IsNullOrWhiteSpace(d.apellido_tercero) ? ' ' + d.apellido_tercero : ""),
                Value = d.tercero_id.ToString()
            });

            ViewBag.precio_venta = referencia.precio_venta;

            string str = Convert.ToString(referencia.precio_venta);
            double precio = double.Parse(str, Thread.CurrentThread.CurrentCulture.NumberFormat);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("is-IS");
            string cst = Convert.ToString(referencia.costo_unitario);
            double costo = double.Parse(cst, Thread.CurrentThread.CurrentCulture.NumberFormat);
            string cst_eme = Convert.ToString(referencia.costo_emergencia);
            double costo_eme = double.Parse(cst_eme, Thread.CurrentThread.CurrentCulture.NumberFormat);
            int idbodega = Convert.ToInt32(Session["user_bodega"]);

            //var costoReferencia = context.vw_promedio.FirstOrDefault(x =>
            //    x.codigo == referencia.ref_codigo && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month && x.bodega == idbodega);

            icb_referencia costoReferencia = context.icb_referencia.FirstOrDefault(x =>
                x.ref_codigo == referencia.ref_codigo && x.ref_estado);

            RegistroReferenciasModel refer = new RegistroReferenciasModel
            {
                ref_codigo = referencia.ref_codigo,
                ref_descripcion = referencia.ref_descripcion,
                por_dscto = referencia.por_dscto,
                por_dscto_max = referencia.por_dscto_max,
                costo_unitario = costoReferencia != null ? costoReferencia.costo_promedio!=null?costoReferencia.costo_promedio.Value.ToString("0,0", elGR):"0" : "0",
                costo_emergencia = referencia.costo_emergencia.ToString("0,0", elGR),
                ref_valor_unitario = referencia.ref_valor_unitario.ToString("0,0", elGR),
                precio_alterno = referencia.precio_alterno.ToString("0,0", elGR),
                precio_garantia = referencia.precio_garantia.ToString("0,0", elGR),
                precio_diesel = referencia.precio_diesel.ToString("0,0", elGR),
                ref_cantidad_min = referencia.ref_cantidad_min,
                ref_cantidad_max = referencia.ref_cantidad_max,
                vida_util = referencia.vida_util,
                clasificacion_ABC = referencia.clasificacion_ABC,
                ref_estado = referencia.ref_estado,
                ref_razon_inactivo = referencia.ref_razon_inactivo,
                manejo_inv = referencia.manejo_inv,
                ref_fecha_creacion = referencia.ref_fecha_creacion,
                ref_usuario_creacion = referencia.ref_usuario_creacion,
                ref_fecha_actualizacion = referencia.ref_fecha_actualizacion,
                ref_usuario_actualizacion = referencia.ref_usuario_actualizacion,
                idporivaventa = referencia.idporivaventa,
                idporivacompra = referencia.idporivacompra,
                unidad_medida = referencia.unidad_medida,
                proveedor_ppal = referencia.proveedor_ppal,
                perfil = referencia.perfil,
                tipo_id = referencia.tipo_id,
                clasificacion_id = referencia.clasificacion_id,
                familia_id = !string.IsNullOrWhiteSpace(referencia.familia_id) ? referencia.familia_id : "",
                grupo_id = !string.IsNullOrWhiteSpace(referencia.grupo_id) ? referencia.grupo_id : "",
                subgrupo = !string.IsNullOrWhiteSpace(referencia.subgrupo) ? referencia.subgrupo : "",
                linea_id = !string.IsNullOrWhiteSpace(referencia.linea_id) ? referencia.linea_id : ""
            };

            /*
			    RegistroReferenciasModel refer = new RegistroReferenciasModel {
			        };

			    refer.ref_codigo = referencia.ref_codigo;
			    refer.ref_descripcion = referencia.ref_descripcion;
			    refer.por_dscto = referencia.por_dscto;
			    refer.por_dscto_max = referencia.por_dscto_max;
			    refer.costo_unitario = costoReferencia != null ? costoReferencia.Promedio.Value.ToString("0,0", elGR) : "0";
			    refer.costo_emergencia = referencia.costo_emergencia.ToString("0,0", elGR);
			    refer.ref_cantidad_min = referencia.ref_cantidad_min;
			    refer.ref_cantidad_max = referencia.ref_cantidad_max;
			    refer.vida_util = referencia.vida_util;
			    refer.clasificacion_ABC = referencia.clasificacion_ABC;
			    refer.ref_estado = referencia.ref_estado;
			    refer.ref_razon_inactivo = referencia.ref_razon_inactivo;
			    refer.manejo_inv = referencia.manejo_inv;
			    refer.ref_fecha_creacion = referencia.ref_fecha_creacion;
			    refer.ref_usuario_creacion = referencia.ref_usuario_creacion;
			    refer.ref_fecha_actualizacion = referencia.ref_fecha_actualizacion;
			    refer.ref_usuario_actualizacion = referencia.ref_usuario_actualizacion;
			    refer.idporivaventa = referencia.idporivaventa;
			    refer.idporivacompra = referencia.idporivacompra;
			    refer.unidad_medida = referencia.unidad_medida;
			    refer.proveedor_ppal = referencia.proveedor_ppal;
			    refer.perfil = referencia.perfil;
			    refer.tipo_id = referencia.tipo_id;
			    refer.clasificacion_id = referencia.clasificacion_id;
			    refer.familia_id = referencia.familia_id;
			    refer.grupo_id = referencia.grupo_id;
			    refer.subgrupo = referencia.subgrupo;
			    refer.linea_id = !string.IsNullOrWhiteSpace(referencia.linea_id)?referencia.linea_id:"";
			    refer.ref_fecha_creacion = referencia.ref_fecha_creacion;
			    refer.ref_usuario_creacion = referencia.ref_usuario_creacion;
			    refer.ref_usuario_actualizacion = referencia.ref_usuario_actualizacion;
			    refer.ref_fecha_actualizacion = referencia.ref_fecha_actualizacion;
			    */

            rprecios listaprecios = context.rprecios.Where(x => x.codigo == refer.ref_codigo).FirstOrDefault();
            ViewBag.precio1 = listaprecios != null ? listaprecios.precio1 : 0;
            ViewBag.precio2 = listaprecios != null ? listaprecios.precio2 : 0;
            ViewBag.precio3 = listaprecios != null ? listaprecios.precio3 : 0;
            ViewBag.precio4 = listaprecios != null ? listaprecios.precio4 : 0;
            ViewBag.precio5 = listaprecios != null ? listaprecios.precio5 : 0;
            ViewBag.precio6 = listaprecios != null ? listaprecios.precio6 : 0;
            ViewBag.precio7 = listaprecios != null ? listaprecios.precio7 : 0;
            ViewBag.precio8 = listaprecios != null ? listaprecios.precio8 : 0;
            ViewBag.precio9 = listaprecios != null ? listaprecios.precio9 : 0;
            ViewBag.precioGarantia = listaprecios != null
                ? listaprecios.precio_garantia != null ? listaprecios.precio_garantia.Value : 0
                : 0;
            ViewBag.precioanterior = listaprecios != null ? listaprecios.precioanterior : 0;

            listas(refer);


            BuscarFavoritos(menu);
            //return View(new RegistroReferenciasModel() { manejo_inv = true, ref_estado = true });           
            return View(refer);
        }

        // POST: 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(RegistroReferenciasModel post, int? menu)
        {
            icb_referencia refer = context.icb_referencia.Where(d => d.ref_codigo == post.ref_codigo).FirstOrDefault();
            decimal precio1 = !string.IsNullOrEmpty(post.ref_valor_unitario)
                ? Convert.ToDecimal(post.ref_valor_unitario, miCultura)
                : 0;
            decimal precio2 = !string.IsNullOrEmpty(Request["precio_2"]) ? Convert.ToDecimal(Request["precio_2"], miCultura) : 0;
            decimal precio3 = !string.IsNullOrEmpty(Request["precio_3"]) ? Convert.ToDecimal(Request["precio_3"], miCultura) : 0;
            decimal precio4 = !string.IsNullOrEmpty(Request["precio_4"]) ? Convert.ToDecimal(Request["precio_4"], miCultura) : 0;
            decimal precio5 = !string.IsNullOrEmpty(Request["precio_5"]) ? Convert.ToDecimal(Request["precio_5"], miCultura) : 0;
            decimal precio6 = !string.IsNullOrEmpty(Request["precio_6"]) ? Convert.ToDecimal(Request["precio_6"], miCultura) : 0;
            decimal precio7 = !string.IsNullOrEmpty(Request["precio_7"]) ? Convert.ToDecimal(Request["precio_7"], miCultura) : 0;
            decimal precio8 = !string.IsNullOrEmpty(Request["precio_8"]) ? Convert.ToDecimal(Request["precio_8"], miCultura) : 0;
            decimal precio9 = !string.IsNullOrEmpty(Request["precio_9"]) ? Convert.ToDecimal(Request["precio_9"], miCultura) : 0;
            decimal precioGarantia = !string.IsNullOrEmpty(post.precio_garantia)
                ? Convert.ToDecimal(post.precio_garantia, miCultura)
                : 0;
            decimal precioanterior = refer.ref_valor_unitario;
            float? textoIva;
            float? textoIvaCompra;
            string porIVA = Request["textIVAVenta"];
            string porIVAcompra = Request["textIVACompra"];
            int idIva = 0;
            int idIvaCompra = 0;

            if (porIVA == "")
            {
                textoIva = null;
                idIva = 4;
            }
            else
            {
                textoIva = float.Parse(Request["textIVAVenta"]);
                idIva = Convert.ToInt32(post.por_iva);
            }

            if (porIVAcompra == "")
            {
                textoIvaCompra = null;
                idIvaCompra = 1;
            }
            else
            {
                textoIvaCompra = float.Parse(Request["textIVAVenta"]);
                idIvaCompra = Convert.ToInt32(post.por_iva_compra);
            }


            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta

                int nom = (from a in context.icb_referencia
                           where a.ref_codigo == post.ref_codigo
                           select a.ref_codigo).Count();

                if (nom == 1)
                {
                    refer.ref_codigo = refer.ref_codigo;
                    refer.ref_descripcion = post.ref_descripcion;
                    refer.alias = post.ref_alternativa;
                    refer.ref_fecha_actualizacion = DateTime.Now;
                    refer.ref_usuario_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    refer.por_iva = textoIva ?? 0;
                    refer.idporivaventa = idIva;
                    refer.idporivacompra = idIvaCompra;
                    refer.por_iva_compra = textoIvaCompra ?? 0;
                    refer.unidad_medida = post.unidad_medida;
                    refer.por_dscto = post.por_dscto ?? 0;
                    refer.por_dscto_max = post.por_dscto_max ?? 0;
                    refer.costo_unitario = post.costo_unitario != null ? Convert.ToDecimal(post.costo_unitario, miCultura) : 0;
                    refer.ref_valor_unitario = !string.IsNullOrEmpty(post.ref_valor_unitario)
                        ? Convert.ToDecimal(post.ref_valor_unitario, miCultura)
                        : 0;
                    refer.precio_venta = !string.IsNullOrEmpty(post.ref_valor_unitario)
                        ? Convert.ToDecimal(post.ref_valor_unitario, miCultura)
                        : 0;
                    refer.costo_emergencia = !string.IsNullOrEmpty(post.costo_emergencia)
                        ? Convert.ToDecimal(post.costo_emergencia, miCultura)
                        : 0;
                    refer.precio_alterno = !string.IsNullOrEmpty(post.precio_alterno)
                        ? Convert.ToDecimal(post.precio_alterno, miCultura)
                        : 0;
                    refer.precio_garantia = !string.IsNullOrEmpty(post.precio_garantia)
                        ? Convert.ToDecimal(post.precio_garantia, miCultura)
                        : 0;
                    refer.ref_cantidad_min = post.ref_cantidad_min ?? 0;
                    refer.ref_cantidad_max = post.ref_cantidad_max ?? 0;
                    refer.proveedor_ppal = post.proveedor_ppal;
                    refer.vida_util = post.vida_util;
                    refer.clasificacion_ABC = post.clasificacion_ABC;
                    refer.perfil = post.perfil;
                    refer.manejo_inv = post.manejo_inv;
                    refer.ref_estado = post.ref_estado;
                    refer.tipo_id = post.tipo_id;
                    refer.clasificacion_id = post.clasificacion_id;
                    refer.familia_id = post.familia_id;
                    refer.grupo_id = post.grupo_id;
                    refer.linea_id = Convert.ToString(post.linea_id);
                    refer.subgrupo = post.subgrupo;
                    refer.precio_diesel= !string.IsNullOrEmpty(post.precio_diesel)
                        ? Convert.ToDecimal(post.precio_diesel, miCultura)
                        : 0;
                    context.Entry(refer).State = EntityState.Modified;

                    if (refer.costo_unitario != 0)
                    {
                        rprecios buscarCod = context.rprecios.FirstOrDefault(x => x.codigo == post.ref_codigo);
                        if (buscarCod != null)
                        {
                            if (precio1 != 0 && precio1 < refer.costo_unitario ||
                                precio2 != 0 && precio2 < refer.costo_unitario ||
                                precio3 != 0 && precio3 < refer.costo_unitario ||
                                precio4 != 0 && precio4 < refer.costo_unitario ||
                                precio5 != 0 && precio5 < refer.costo_unitario ||
                                precio6 != 0 && precio6 < refer.costo_unitario ||
                                precio7 != 0 && precio7 < refer.costo_unitario ||
                                precio8 != 0 && precio8 < refer.costo_unitario ||
                                precio9 != 0 && precio9 < refer.costo_unitario)
                            {
                                TempData["mensaje_error"] =
                                    "Alguno de los precios de la referencia es menor que el costo, por favor valide";
                                listas(post);
                                ViewBag.subgrupo = post.subgrupo;
                                ViewBag.precio1 = precio1;
                                ViewBag.precio2 = precio2;
                                ViewBag.precio3 = precio3;
                                ViewBag.precio4 = precio4;
                                ViewBag.precio5 = precio5;
                                ViewBag.precio6 = precio6;
                                ViewBag.precio7 = precio7;
                                ViewBag.precio8 = precio8;
                                ViewBag.precio9 = precio9;
                                ViewBag.precioGarantia = precioGarantia;
                                ViewBag.precioanterior = precioanterior;
                                BuscarFavoritos(menu);
                                return View();
                            }

                            rprecios cod = context.rprecios.Where(x => x.codigo == refer.ref_codigo).FirstOrDefault();
                            cod.codigo = refer.ref_codigo;
                            cod.fec_actualizacion = DateTime.Now;
                            cod.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                            cod.precio1 = precio1;
                            cod.precio2 = precio2;
                            cod.precio3 = precio3;
                            cod.precio4 = precio4;
                            cod.precio5 = precio5;
                            cod.precio6 = precio6;
                            cod.precio7 = precio7;
                            cod.precio8 = precio8;
                            cod.precio9 = precio9;
                            cod.precio_garantia = precioGarantia;
                            cod.precioanterior = precio1;
                            context.Entry(cod).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        rprecios cod = context.rprecios.Where(x => x.codigo == refer.ref_codigo).FirstOrDefault();
                        if (cod != null)
                        {
                            cod.codigo = refer.ref_codigo;
                            cod.fec_actualizacion = DateTime.Now;
                            cod.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                            cod.precio1 = precio1;
                            cod.precio2 = precio2;
                            cod.precio3 = precio3;
                            cod.precio4 = precio4;
                            cod.precio5 = precio5;
                            cod.precio6 = precio6;
                            cod.precio7 = precio7;
                            cod.precio8 = precio8;
                            cod.precio9 = precio9;
                            cod.precio_garantia = precioGarantia;
                            cod.precioanterior = precioanterior;
                            context.Entry(cod).State = EntityState.Modified;
                        }
                        else
                        {
                            rprecios crear = new rprecios
                            {
                                codigo = refer.ref_codigo,
                                fec_creacion = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                fec_actualizacion = DateTime.Now,
                                user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]),
                                precio1 = precio1,
                                precio2 = precio2,
                                precio3 = precio3,
                                precio4 = precio4,
                                precio5 = precio5,
                                precio6 = precio6,
                                precio7 = precio7,
                                precio8 = precio8,
                                precio9 = precio9,
                                precio_garantia = precioGarantia,
                                precioanterior = precio1
                            };
                            context.rprecios.Add(crear);
                        }
                    }

                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización de la referencia fue exitoso";

                    ViewBag.precio1 = precio1;
                    ViewBag.precio2 = precio2;
                    ViewBag.precio3 = precio3;
                    ViewBag.precio4 = precio4;
                    ViewBag.precio5 = precio5;
                    ViewBag.precio6 = precio6;
                    ViewBag.precio7 = precio7;
                    ViewBag.precio8 = precio8;
                    ViewBag.precio9 = precio9;
                    ViewBag.precioGarantia = precioGarantia;
                    ViewBag.precioanterior = precioanterior;
                    listas(post);

                    string asesorCrea = context.users.Where(x => x.user_id == refer.ref_usuario_creacion)
                        .Select(nombre => nombre.user_nombre + " " + nombre.user_apellido).FirstOrDefault();
                    string asesorActualiza = context.users.Where(x => x.user_id == refer.ref_usuario_actualizacion)
                        .Select(nombre => nombre.user_nombre + " " + nombre.user_apellido).FirstOrDefault();

                    ViewBag.user_nombre_cre = asesorCrea;
                    ViewBag.user_nombre_act = asesorActualiza;
                    ViewBag.ref_fecha_creacion = refer.ref_fecha_creacion;
                    ViewBag.ref_fecha_actualizacion = refer.ref_fecha_actualizacion;
                    return View(post);
                }
                else
                {
                    TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
                    listas(post);

                    ViewBag.precio1 = precio1;
                    ViewBag.precio2 = precio2;
                    ViewBag.precio3 = precio3;
                    ViewBag.precio4 = precio4;
                    ViewBag.precio5 = precio5;
                    ViewBag.precio6 = precio6;
                    ViewBag.precio7 = precio7;
                    ViewBag.precio8 = precio8;
                    ViewBag.precio9 = precio9;
                    ViewBag.precioGarantia = precioGarantia;
                    ViewBag.precioanterior = precioanterior;
                    string asesorCrea = context.users.Where(x => x.user_id == refer.ref_usuario_creacion)
                        .Select(nombre => nombre.user_nombre + " " + nombre.user_apellido).FirstOrDefault();
                    string asesorActualiza = context.users.Where(x => x.user_id == refer.ref_usuario_actualizacion)
                        .Select(nombre => nombre.user_nombre + " " + nombre.user_apellido).FirstOrDefault();

                    ViewBag.user_nombre_cre = asesorCrea;
                    ViewBag.user_nombre_act = asesorActualiza;
                    ViewBag.ref_fecha_creacion = refer.ref_fecha_creacion;
                    ViewBag.ref_fecha_actualizacion = refer.ref_fecha_actualizacion;
                    return View(post);
                }
            }

            ViewBag.precio1 = precio1;
            ViewBag.precio2 = precio2;
            ViewBag.precio3 = precio3;
            ViewBag.precio4 = precio4;
            ViewBag.precio5 = precio5;
            ViewBag.precio6 = precio6;
            ViewBag.precio7 = precio7;
            ViewBag.precio8 = precio8;
            ViewBag.precio9 = precio9;
            ViewBag.precioGarantia = precioGarantia;
            ViewBag.precioanterior = precioanterior;
            BuscarFavoritos(menu);
            listas(post);
            return View(post);
        }

        // GET:
        public void cargarPrecios(string id)
        {
            ViewBag.precio1 = context.rprecios.Where(x => x.codigo == id).Select(x => x.precio1).FirstOrDefault();
            ViewBag.precio2 = context.rprecios.Where(x => x.codigo == id).Select(x => x.precio2).FirstOrDefault();
            ViewBag.precio3 = context.rprecios.Where(x => x.codigo == id).Select(x => x.precio3).FirstOrDefault();
            ViewBag.precio4 = context.rprecios.Where(x => x.codigo == id).Select(x => x.precio4).FirstOrDefault();
            ViewBag.precio5 = context.rprecios.Where(x => x.codigo == id).Select(x => x.precio5).FirstOrDefault();
            ViewBag.precio6 = context.rprecios.Where(x => x.codigo == id).Select(x => x.precio6).FirstOrDefault();
            ViewBag.precio7 = context.rprecios.Where(x => x.codigo == id).Select(x => x.precio7).FirstOrDefault();
            ViewBag.precio8 = context.rprecios.Where(x => x.codigo == id).Select(x => x.precio8).FirstOrDefault();
            ViewBag.precio9 = context.rprecios.Where(x => x.codigo == id).Select(x => x.precio9).FirstOrDefault();
            ViewBag.precioanterior = context.rprecios.Where(x => x.codigo == id).Select(x => x.precioanterior)
                .FirstOrDefault();
        }

        //browser referencias alternas
        public ActionResult BuscarPrecios(string id, int? menu)
        {
            IQueryable<rremplazos> rreemplazo = context.rremplazos.Where(x => x.referencia == id);
            ViewBag.cod_referencia = id;
            return View(rreemplazo.ToList());
        }

        public ActionResult BrowserVentasXbodega()
        {
            var listaBodega = (from a in context.vw_ventas_repuestosGrafica
                               select new
                               {
                                   id = a.bodega,
                                   nombre = a.bodccs_nombre
                               }).ToList().Distinct();

            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in listaBodega)
            {
                lista.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }
            ViewBag.Bodegas = lista;

            var listaBodega1 = (from a in context.vw_ventas_repuestosGrafica
                               select new
                               {
                                   id = a.bodega,
                                   nombre = a.bodccs_nombre
                               }).ToList().Distinct();

            List<SelectListItem> listaB = new List<SelectListItem>();
            foreach (var item in listaBodega1)
            {
                listaB.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }
            ViewBag.listar_bodegas = listaB;

            var listaAño = (from a in context.vw_ventas_repuestosGrafica
                            where a.ano>= (from d in context.vw_ventas_repuestosGrafica select d.ano).Min() && a.ano<= (from d in context.vw_ventas_repuestosGrafica select d.ano).Max()
                            select new
                               {
                                id = a.ano,
                                nombre = a.ano
                            }).ToList().Distinct();

            List<SelectListItem> lista1 = new List<SelectListItem>();
            foreach (var item in listaAño)
            {
                lista1.Add(new SelectListItem
                {
                    Text = item.nombre.ToString(),
                    Value = item.id.ToString()
                });
            }
            ViewBag.Año = lista1;



            ViewBag.horas = DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;





            return View();
        }

        public JsonResult buscarSubGrupo(string grupo)
        {
            var data = from g in context.ref_subgrupo
                       where g.cod_grupo == grupo
                             && g.estado
                       select new
                       {
                           g.codigo,
                           g.Descripcion
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarJson(string filtrogeneral)
        {
            if (Session["user_usuarioid"] != null)
            {
                string draw = Request.Form.GetValues("draw").FirstOrDefault();
                string start = Request.Form.GetValues("start").FirstOrDefault();
                string length = Request.Form.GetValues("length").FirstOrDefault();
                string search = Request.Form.GetValues("search[value]").FirstOrDefault();
                //esto me sirve para reiniciar la consulta cuando ordeno las columnas de menor a mayor y que no me vuelva a recalcular todo
                //ES IMPORTANTE QUE LA COLUMNA EN EL DATATABLE TENGA EL NOMBRE DE LA TABLA O VISTA A CONSULTAR, porque vamos a usarla para ordenar.
                string sortColumn = Request.Form
                    .GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]")
                    .FirstOrDefault();
                string sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                search = search.Replace(" ", "");
                int pagina = Convert.ToInt32(start);
                int pageSize = Convert.ToInt32(length);

                int skip = 0;
                if (pagina == 0)
                {
                    skip = 0;
                }
                else
                {
                    skip = pagina;
                }

                CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

                int idusuario = Convert.ToInt32(Session["user_usuarioid"]);
                //busco el usuario
                users usuario = context.users.Where(d => d.user_id == idusuario).FirstOrDefault();
                int rol = Convert.ToInt32(Session["user_rolid"]);

                //por si necesito el rol del usuario

                icb_sysparameter admin1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P109").FirstOrDefault();
                int admin = admin1 != null ? Convert.ToInt32(admin1.syspar_value) : 1;

                Expression<Func<vw_browser_referencias, bool>> predicado = PredicateBuilder.True<vw_browser_referencias>();
                Expression<Func<vw_browser_referencias, bool>> predicado2 = PredicateBuilder.False<vw_browser_referencias>();
                Expression<Func<vw_browser_referencias, bool>> predicado3 = PredicateBuilder.False<vw_browser_referencias>();

                if (!string.IsNullOrWhiteSpace(filtrogeneral))
                {
                    predicado2 = predicado2.Or(d => 1 == 1 && d.ref_codigo.ToString().Contains(filtrogeneral));
                    predicado2 = predicado2.Or(d =>
                        1 == 1 && d.ref_descripcion.ToUpper().Contains(filtrogeneral.ToUpper()));
                    predicado2 =
                        predicado2.Or(d => 1 == 1 && d.precio_garantia_string.Contains(filtrogeneral.ToUpper()));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.grupo_nombre.Contains(filtrogeneral.ToUpper()));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.clarpto_nombre.Contains(filtrogeneral.ToUpper()));
                    predicado2 = predicado2.Or(d =>
                        1 == 1 && d.Descripcion.ToUpper().Contains(filtrogeneral.ToUpper()));
                    predicado2 = predicado2.Or(d =>
                        1 == 1 && d.familia_nombre.ToUpper().Contains(filtrogeneral.ToUpper()));
                    predicado2 = predicado2.Or(d =>
                        1 == 1 && d.grupo_repuesto_nombre.ToUpper().Contains(filtrogeneral.ToUpper()));
                    predicado2 = predicado2.Or(d =>
                        1 == 1 && d.subgrupo_nombre.ToUpper().Contains(filtrogeneral.ToUpper()));
                    predicado2 = predicado2.Or(d =>
                        1 == 1 && d.nombre_estado.ToUpper().Contains(filtrogeneral.ToUpper()));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.alias.Contains(filtrogeneral.ToUpper()));
                    predicado = predicado.And(predicado2);
                }

                predicado = predicado.And(d => d.modulo == "R");
                int registrostotales = context.vw_browser_referencias.Where(predicado).Count();
                //si el ordenamiento es ascendente o descendente es distinto
                if (pageSize == -1)
                {
                    pageSize = registrostotales;
                }

                if (sortColumnDir == "asc")
                {
                    List<vw_browser_referencias> query2 = context.vw_browser_referencias.Where(predicado)
                        .OrderBy(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
                    var query = query2.Select(d => new
                    {
                        d.ref_codigo,
                        d.ref_descripcion,
                        d.precio_garantia_string,
                        d.grupo_nombre,
                        d.clarpto_nombre,
                        d.Descripcion,
                        d.familia_nombre,
                        d.grupo_repuesto_nombre,
                        d.subgrupo_nombre,
                        d.nombre_estado,
                        alias = d.alias ?? ""
                    }).ToList();

                    int contador = query.Count();
                    return Json(
                        new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                        JsonRequestBehavior.AllowGet);
                }
                else
                {
                    List<vw_browser_referencias> query2 = context.vw_browser_referencias.Where(predicado)
                        .OrderByDescending(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
                    var query = query2.Select(d => new
                    {
                        d.ref_codigo,
                        d.ref_descripcion,
                        d.precio_garantia_string,
                        d.grupo_nombre,
                        d.clarpto_nombre,
                        d.Descripcion,
                        d.familia_nombre,
                        d.grupo_repuesto_nombre,
                        d.subgrupo_nombre,
                        d.nombre_estado
                    }).ToList();

                    int contador = query.Count();
                    return Json(
                        new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                        JsonRequestBehavior.AllowGet);
                }
            }

            return Json(0);
        }

        public JsonResult BuscarJsonAsesor()
        {
            var data = from r in context.vw_preciosaccesorios
                       select new
                       {
                           r.ref_codigo,
                           r.ref_descripcion,
                           precio1 = r.precio1 != null ? r.precio1 : 0
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarHistorico(string codigo)
        {
            var data = from d in context.icb_referencia_movdetalle
                       join m in context.icb_referencia_mov
                           on d.refmov_id equals m.refmov_id
                       //join mv in context.icb_tpmovimiento
                       //on m.tpmov_id equals mv.mov_id
                       join r in context.icb_referencia
                           on d.ref_codigo equals r.ref_codigo
                       //join t in context.users
                       //on m.refmov_usuela equals t.user_id
                       where d.ref_codigo == codigo
                       select new
                       {
                           //nummov = mv.mov_cod,
                           //tpmov = mv.mov_nombre,
                           cantidad = d.refdet_cantidad,
                           fecha = m.refmov_fecela != null ? m.refmov_fecela.ToString() : "",
                           //usuario = t.user_nombre + " " + t.user_apellido,
                           stock = d.refdet_saldo
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BuscarDatos(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult BrowserAsesor(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult buscarAlternos(string id_referencia)
        {
            int? buscarPadre = context.rremplazos.Where(x => x.referencia == id_referencia || x.alterno == id_referencia)
                .Select(x => x.idpadre).FirstOrDefault();
            var buscar = (from a in context.rremplazos
                          where a.idpadre == buscarPadre
                          select new
                          {
                              a.referencia,
                              a.descripcion,
                              a.alterno
                          }).ToList();
            var data = buscar.Select(x => new
            {
                x.alterno,
                x.descripcion
            }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CargarReferencias(string id_referencia)
        {
            var referencias = from u in context.icb_referencia
                              join a in context.rremplazos
                                  on u.ref_codigo equals a.referencia into zz
                              from a in zz.DefaultIfEmpty()
                              where u.ref_estado && u.ref_codigo != id_referencia && u.ref_codigo != a.referencia
                              orderby u.ref_descripcion
                              select new
                              {
                                  nombre = "(" + u.ref_codigo + ") - " + u.ref_descripcion,
                                  u.ref_codigo
                              };
            List<SelectListItem> referencia = new List<SelectListItem>();
            foreach (var item in referencias)
            {
                referencia.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.ref_codigo
                });
            }

            ViewBag.cargarReferencia = referencia;

            return Json(ViewBag.cargarReferencia, JsonRequestBehavior.AllowGet);
        }

        public JsonResult asignarAlterno(string id_referenciaAlterna, string referenciaPadre, bool eliminar)
        {
            var buscar = context.rremplazos.Where(x => x.referencia == referenciaPadre)
                .Select(x => new { x.id, x.referencia, x.alterno, x.idpadre, x.descripcion }).FirstOrDefault();

            if (eliminar == false)
            {
                rremplazos asignar = new rremplazos();
                int buscarIguales = context.rremplazos
                    .Where(x => x.referencia == referenciaPadre && x.alterno == id_referenciaAlterna).Count();
                if (buscarIguales == 1)
                {
                    return Json(new { data = false }, JsonRequestBehavior.AllowGet);
                }

                string buscarReferencia = context.icb_referencia.Where(x => x.ref_codigo == id_referenciaAlterna)
                    .Select(x => x.ref_descripcion).FirstOrDefault();
                if (buscar == null)
                {
                    asignar.referencia = referenciaPadre;
                    asignar.alterno = id_referenciaAlterna;
                    asignar.idpadre = asignar.id;
                    asignar.descripcion = buscarReferencia;
                }
                else
                {
                    if (buscar.id != buscar.idpadre)
                    {
                        asignar.referencia = referenciaPadre;
                        asignar.alterno = id_referenciaAlterna;
                        asignar.idpadre = buscar.idpadre;
                        asignar.descripcion = buscarReferencia;
                    }
                    else
                    {
                        asignar.referencia = referenciaPadre;
                        asignar.alterno = id_referenciaAlterna;
                        asignar.idpadre = buscar.id;
                        asignar.descripcion = buscarReferencia;
                    }
                }

                context.rremplazos.Add(asignar);
                context.SaveChanges();
                if (buscar == null)
                {
                    rremplazos buscarId = context.rremplazos.Where(a => a.referencia == referenciaPadre).FirstOrDefault();
                    if (buscarId != null)
                    {
                        buscarId.idpadre = buscarId.id;
                        context.Entry(buscarId).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                }
            }
            else if (eliminar)
            {
                rremplazos buscar2 = context.rremplazos
                    .Where(x => x.referencia == referenciaPadre && x.alterno == id_referenciaAlterna).FirstOrDefault();
                context.Entry(buscar2).State = EntityState.Deleted;
                context.SaveChanges();
            }

            return Json(new { data = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarBodegas()
        {
            int usuarioLog = Convert.ToInt32(Session["user_usuarioid"]);

            var rol = (from r in context.users
                       where r.user_id == usuarioLog
                       select new
                       {
                           r.rol_id
                       }).FirstOrDefault();

            var listarBodegas = (from v in context.vw_usuarios_rol
                                 where v.user_id == usuarioLog
                                 select new
                                 {
                                     v.id_bodega,
                                     v.bodccs_nombre
                                 }).ToList();

            return Json(listarBodegas, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarDatosVentas(DateTime inicio, DateTime fin, int?[] bodega)
        {
            var data = (from vv in context.vw_ventas_repuestos
                        where vv.fecha >= inicio && vv.fecha <= fin && bodega.Contains(vv.bodega)
                        group vv by vv.bodega
                into g
                        select new
                        {
                            idBodega = g.Key,
                            nombre = g.Select(x => x.bodccs_nombre).FirstOrDefault(),
                            valorImpuesto = g.Sum(x => x.valor),
                            valorIVA = g.Sum(x => x.iva),
                            valorCosto = g.Sum(x => x.costo),
                            valorMercancia = g.Sum(x => x.valormercancia)
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarDetalleVentas(DateTime inicio, DateTime fin, int id)
        {
            var data2 = (from vv in context.vw_ventas_repuestos
                         where vv.fecha >= inicio && vv.fecha <= fin && vv.bodega == id
                         select new
                         {
                             vv.idencabezado,
                             nombre = vv.tpdoc_nombre,
                             vv.numero,
                             cliente = vv.prinom_tercero != null
                                 ? vv.prinom_tercero + " " + vv.segnom_tercero + " " + vv.apellido_tercero + " " +
                                   vv.segapellido_tercero
                                 : vv.razon_social,
                             valorImpuesto = vv.valor,
                             valorIVA = vv.iva,
                             valorCosto = vv.costo,
                             valorMercancia = vv.valormercancia,
                             vv.retencion,
                             vv.retencion_causada,
                             vv.retencion_ica,
                             vv.retencion_iva,
                             asesor = vv.user_nombre + " " + vv.user_apellido,
                             vv.fecha
                         }).ToList();

            var data = data2.Select(x => new
            {
                x.idencabezado,
                x.nombre,
                x.numero,
                x.cliente,
                x.valorImpuesto,
                x.valorIVA,
                x.valorCosto,
                x.valorMercancia,
                x.retencion,
                x.retencion_causada,
                x.retencion_ica,
                x.retencion_iva,
                x.asesor,
                fecha = x.fecha.ToString("yyyy/MM/dd")
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult recalcularClasificacionABC()
        {
            List<string> buscarReferencias =
                context.icb_referencia.Where(x => x.modulo == "R").Select(x => x.ref_codigo).ToList();

            var parametros = context.rparametrizacionabc.Select(x => new
            {
                x.rangomes,
                x.cantidadmov_desde,
                x.cantidadmes,
                x.clasificacion,
                x.porcentaje_reserva
            }).ToList();

            DateTime fecha_param_a = parametros.Where(x => x.clasificacion == "a")
                .Select(x => DateTime.Now.AddMonths(-x.rangomes)).FirstOrDefault();
            DateTime fecha_param_b = parametros.Where(x => x.clasificacion == "b")
                .Select(x => DateTime.Now.AddMonths(-x.rangomes)).FirstOrDefault();
            DateTime fecha_param_c = parametros.Where(x => x.clasificacion == "c")
                .Select(x => DateTime.Now.AddMonths(-x.rangomes)).FirstOrDefault();
            DateTime fecha_param_d = parametros.Where(x => x.clasificacion == "d")
                .Select(x => DateTime.Now.AddMonths(-x.rangomes)).FirstOrDefault();
            DateTime fecha_fin_param_d = parametros.Where(x => x.clasificacion == "d")
                .Select(x => DateTime.Now.AddMonths(-x.cantidadmes)).FirstOrDefault();
            DateTime fecha_param_e = parametros.Where(x => x.clasificacion == "e")
                .Select(x => DateTime.Now.AddMonths(-x.rangomes)).FirstOrDefault();
            DateTime hoy = DateTime.Now;


            foreach (string ref_codigo in buscarReferencias)
            {
                icb_referencia traer = context.icb_referencia.Find(ref_codigo);

                var buscarMovimientos = (from a in context.encab_documento
                                         join b in context.lineas_documento
                                             on a.idencabezado equals b.id_encabezado
                                         join c in context.tp_doc_registros
                                             on a.tipo equals c.tpdoc_id
                                         join d in context.tp_doc_registros_tipo
                                             on c.tipo equals d.id
                                         where d.id == 4 && b.codigo == ref_codigo
                                         select new
                                         {
                                             a.fecha,
                                             b.cantidad
                                         }).ToList();

                if (buscarMovimientos == null)
                {
                    traer.clasificacion_ABC = "e";
                    context.Entry(traer).State = EntityState.Modified;
                }

                if (buscarMovimientos != null)
                {
                    int contar_a = buscarMovimientos.Where(x => x.fecha >= fecha_param_a && x.fecha <= hoy).Count();
                    int contar_b = buscarMovimientos.Where(x => x.fecha >= fecha_param_b && x.fecha <= hoy).Count();
                    int contar_c = buscarMovimientos.Where(x => x.fecha >= fecha_param_c && x.fecha <= hoy).Count();
                    int contar_d = buscarMovimientos
                        .Where(x => x.fecha >= fecha_param_d && x.fecha <= fecha_fin_param_d).Count();
                    int contar_e = buscarMovimientos.Where(x => x.fecha >= fecha_param_e && x.fecha <= hoy).Count();

                    var divisor_a = parametros.Where(x => x.clasificacion == "a").Select(x =>
                        new { x.cantidadmes, x.rangomes, x.cantidadmov_desde, x.porcentaje_reserva }).FirstOrDefault();
                    var divisor_b = parametros.Where(x => x.clasificacion == "b").Select(x =>
                        new { x.cantidadmes, x.rangomes, x.cantidadmov_desde, x.porcentaje_reserva }).FirstOrDefault();
                    var divisor_c = parametros.Where(x => x.clasificacion == "c").Select(x =>
                        new { x.cantidadmes, x.rangomes, x.cantidadmov_desde, x.porcentaje_reserva }).FirstOrDefault();
                    var divisor_d = parametros.Where(x => x.clasificacion == "d").Select(x =>
                        new { x.cantidadmes, x.rangomes, x.cantidadmov_desde, x.porcentaje_reserva }).FirstOrDefault();
                    var divisor_e = parametros.Where(x => x.clasificacion == "e").Select(x =>
                        new { x.cantidadmes, x.rangomes, x.cantidadmov_desde, x.porcentaje_reserva }).FirstOrDefault();

                    decimal? minima_a = Convert.ToDecimal(divisor_a.cantidadmes, miCultura) / Convert.ToDecimal(divisor_a.rangomes, miCultura) *
                                   divisor_a.cantidadmov_desde;
                    decimal? minima_b = Convert.ToDecimal(divisor_b.cantidadmes, miCultura) / Convert.ToDecimal(divisor_b.rangomes, miCultura) *
                                   divisor_b.cantidadmov_desde;
                    decimal? minima_c = Convert.ToDecimal(divisor_c.cantidadmes, miCultura) / Convert.ToDecimal(divisor_c.rangomes, miCultura) *
                                   divisor_c.cantidadmov_desde;
                    decimal? minima_d = Convert.ToDecimal(divisor_d.cantidadmes, miCultura) / Convert.ToDecimal(divisor_d.rangomes, miCultura) *
                                   divisor_d.cantidadmov_desde;

                    decimal? calculo_a = Convert.ToDecimal(contar_a, miCultura) / minima_a;
                    decimal? calculo_b = Convert.ToDecimal(contar_b, miCultura) / minima_b;
                    decimal? calculo_c = Convert.ToDecimal(contar_c, miCultura) / minima_c;
                    decimal? calculo_d = Convert.ToDecimal(contar_d, miCultura) / minima_d;

                    if (calculo_a > divisor_a.cantidadmov_desde)
                    {
                        traer.clasificacion_ABC = "a";
                        traer.ref_cantidad_min = Convert.ToInt32(Math.Round(
                            Convert.ToDecimal(traer.ref_cantidad_max,miCultura) *
                            Convert.ToDecimal(divisor_a.porcentaje_reserva, miCultura) / 100));
                        context.Entry(traer).State = EntityState.Modified;
                    }
                    else if (calculo_b < divisor_b.cantidadmov_desde && calculo_b > minima_b)
                    {
                        traer.clasificacion_ABC = "b";
                        traer.ref_cantidad_min = Convert.ToInt32(Math.Round(
                            Convert.ToDecimal(traer.ref_cantidad_max, miCultura) *
                            Convert.ToDecimal(divisor_b.porcentaje_reserva, miCultura) / 100));
                        context.Entry(traer).State = EntityState.Modified;
                    }
                    else if (calculo_c == divisor_c.cantidadmov_desde)
                    {
                        traer.clasificacion_ABC = "c";
                        traer.ref_cantidad_min = Convert.ToInt32(Math.Round(
                            Convert.ToDecimal(traer.ref_cantidad_max, miCultura) *
                            Convert.ToDecimal(divisor_b.porcentaje_reserva, miCultura) / 100));
                        context.Entry(traer).State = EntityState.Modified;
                    }
                    else if (calculo_d > minima_d)
                    {
                        traer.clasificacion_ABC = "d";
                        traer.ref_cantidad_min = Convert.ToInt32(Math.Round(
                            Convert.ToDecimal(traer.ref_cantidad_max, miCultura) *
                            Convert.ToDecimal(divisor_b.porcentaje_reserva, miCultura) / 100));
                        context.Entry(traer).State = EntityState.Modified;
                    }
                    else
                    {
                        traer.clasificacion_ABC = "e";
                        traer.ref_cantidad_min = Convert.ToInt32(Math.Round(
                            Convert.ToDecimal(traer.ref_cantidad_max, miCultura) *
                            Convert.ToDecimal(divisor_b.porcentaje_reserva, miCultura) / 100));
                        context.Entry(traer).State = EntityState.Modified;
                    }
                }
            }

            context.SaveChanges();
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult buscarconsecutivo()
        {
            int consecutivo = context.inicioconsecutivo_variosb.Select(x => x.valor_inicial).FirstOrDefault();
            bool noExiste = true;
            int largo = consecutivo.ToString().Length;
            string ref_cod = "";
            string ceros = "";
            string extraerNumeros = "";
            if (consecutivo == 0)
            {
                var codigo = new
                {
                    noExiste
                };

                return Json(codigo, JsonRequestBehavior.AllowGet);
            }

            if (consecutivo > 0)
            {
                string buscarOtras = context.icb_referencia.Where(x => x.ref_codigo.Contains("VARIOSB"))
                    .OrderByDescending(x => x.ref_fecha_creacion).Select(x => x.ref_codigo).FirstOrDefault();

                int bodega = Convert.ToInt32(Session["user_bodega"]);

                var RefBod=context.bodega_concesionario.Where(x => x.id== bodega).Select(x => x.bodccs_cod).FirstOrDefault();

                if (buscarOtras == null)
                {
                    for (int i = largo; i < 6; i++)
                    {
                        ceros += "0";
                    }

                    ref_cod = "VARIOSB" + ceros + consecutivo;
                }

                if (buscarOtras != null)
                {
                    extraerNumeros = buscarOtras.Replace("VARIOSB", "");
                    int num = Convert.ToInt32(extraerNumeros);
                    num = num + 1;
                    largo = num.ToString().Length;
                    for (int i = largo; i < 5; i++)
                    {
                        ceros += "0";
                    }

                    ref_cod = "VARIOSB" + RefBod + ceros + num;
                }

                var codigo = new
                {
                    noExiste = false,
                    ref_cod
                };

                return Json(codigo, JsonRequestBehavior.AllowGet);
            }

            return Json(JsonRequestBehavior.AllowGet);
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

        #region get, post EditPrecios

        //public ActionResult EditPrecios(string id, int? menu)
        //{
        //    //valida si el id es null
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    rprecios precios = context.rprecios.Find(id);
        //    if (precios == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
        //    var result = from a in context.users
        //                 join b in context.rprecios on a.user_id equals b.userid_creacion
        //                 where b.codigo == id
        //                 select a.user_nombre.ToString();

        //    foreach (var i in result)
        //    {
        //        ViewBag.user_nombre_cre = i;
        //    }

        //    //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
        //    var result1 = from a in context.users
        //                  join b in context.rprecios on a.user_id equals b.user_idactualizacion
        //                  where b.codigo == id
        //                  select a.user_nombre.ToString();

        //    foreach (var i in result1)
        //    {
        //        ViewBag.user_nombre_act = i;
        //    }


        //    BuscarFavoritos(menu);
        //    return View(precios);
        //}

        // POST: 
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult EditPrecios(rprecios precios, int? menu)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        precios.fec_actualizacion = DateTime.Now;
        //        precios.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
        //        context.Entry(precios).State = System.Data.Entity.EntityState.Modified;
        //        context.SaveChanges();
        //        TempData["mensaje"] = "Precios actualizados corectamente";

        //    }
        //    BuscarFavoritos(menu);
        //    return View(precios);

        //}

        #endregion
    }
}