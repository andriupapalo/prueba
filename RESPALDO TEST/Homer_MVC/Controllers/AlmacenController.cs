using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class AlmacenController : Controller
    {

        public class listabodegas
        {
            public int idbodega { get; set; }
            public string nombrebodega { get; set; }
            public decimal stockbodega { get; set; }
        }
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");

        private static Expression<Func<vw_solicitud_repuestos_ordentaller, string>> GetColumnName(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(vw_solicitud_repuestos_ordentaller), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<vw_solicitud_repuestos_ordentaller, string>> lambda = Expression.Lambda<Func<vw_solicitud_repuestos_ordentaller, string>>(menuProperty, menu);

            return lambda;
        }

        private static Expression<Func<vw_browser_solicitudes_despacho_taller, string>> GetColumnName2(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(vw_browser_solicitudes_despacho_taller), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<vw_browser_solicitudes_despacho_taller, string>> lambda = Expression.Lambda<Func<vw_browser_solicitudes_despacho_taller, string>>(menuProperty, menu);

            return lambda;
        }

        private static Expression<Func<vw_browser_recepcion_traslados_ot, string>> GetColumnName3(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(vw_browser_recepcion_traslados_ot), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<vw_browser_recepcion_traslados_ot, string>> lambda = Expression.Lambda<Func<vw_browser_recepcion_traslados_ot, string>>(menuProperty, menu);

            return lambda;
        }

        // GET: Almacen
        public ActionResult Index()
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);

            ViewBag.tipoTarifaR = new SelectList(context.ttipostarifa, "id", "Descripcion");
            var buscarRepuestos = (from referencia in context.icb_referencia
                                   where referencia.modulo == "R" && referencia.ref_estado
                                   select new
                                   {
                                       referencia.ref_codigo,
                                       ref_descripcion = referencia.ref_codigo + " - " + referencia.ref_descripcion,
                                       referencia.manejo_inv
                                   }).ToList();
            ViewBag.repuestos = new SelectList(buscarRepuestos, "ref_codigo", "ref_descripcion");

            encab_documento buscarUltimoTraslado = context.encab_documento.OrderByDescending(x => x.idencabezado).FirstOrDefault();
            ViewBag.numTrasladoCreado = buscarUltimoTraslado != null ? buscarUltimoTraslado.numero : 0;

            return View();
        }

        public ActionResult browserSolicitudesOT(int? menu)
        {
            //le digo todas las bodegas
            var bodegas = context.bodega_concesionario.Where(d => d.bodccs_estado)
                .Select(d => new { d.id, nombre = d.bodccs_cod + "-" + d.bodccs_nombre }).ToList();
            ViewBag.bodega = new MultiSelectList(bodegas, "id", "nombre");
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult browserSolicitudesDespachoOT(int? menu)
        {
            //le digo todas las bodegas
            var bodegas = context.bodega_concesionario.Where(d => d.bodccs_estado)
                .Select(d => new { d.id, nombre = d.bodccs_cod + "-" + d.bodccs_nombre }).ToList();
            ViewBag.bodega = new MultiSelectList(bodegas, "id", "nombre");
            ViewBag.bodega2 = new MultiSelectList(bodegas, "id", "nombre");
            ViewBag.bodega3 = new MultiSelectList(bodegas, "id", "nombre");
            BuscarFavoritos(menu);

            int rol = Convert.ToInt32(Session["user_rolid"]);

            var permisos = (from acceso in context.rolacceso
                            join rolPerm in context.rolpermisos
                            on acceso.idpermiso equals rolPerm.id
                            where acceso.idrol == rol //&& rolPerm.codigo == "P40" 
                            select new { rolPerm.codigo }).ToList();

            var resultado = permisos.Where(x => x.codigo == "P42").Count() > 0 ? "Si" : "No";

            ViewBag.Permiso = resultado;

            return View();
        }

        public JsonResult filtroSolicitudes(int[] bodega, string filtroGeneral, string desde, string hasta)
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

            Expression<Func<vw_solicitud_repuestos_ordentaller, bool>> predicado = PredicateBuilder.True<vw_solicitud_repuestos_ordentaller>();
            Expression<Func<vw_solicitud_repuestos_ordentaller, bool>> predicado2 = PredicateBuilder.False<vw_solicitud_repuestos_ordentaller>();
            Expression<Func<vw_solicitud_repuestos_ordentaller, bool>> predicado3 = PredicateBuilder.False<vw_solicitud_repuestos_ordentaller>();

            if (bodega.Count() > 0 && bodega[0] != 0)
            {
                foreach (int item in bodega)
                {
                    predicado2 = predicado2.Or(d => d.idbodega == item);
                }

                predicado = predicado.And(predicado2);
            }


            if (!string.IsNullOrWhiteSpace(filtroGeneral))
            {
                predicado3 = predicado3.Or(d => 1 == 1 && d.codigoentrada.ToString().Contains(filtroGeneral));
                predicado3 = predicado3.Or(d => 1 == 1 && d.nombre_bodega.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.nombretercero.Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.plan_mayor.Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.descripcion.Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.plac_vh.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.doc_tercero.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.referencias.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado = predicado.And(predicado3);
            }

            if (!string.IsNullOrWhiteSpace(desde))
            {
                //inicializo variable fecha
                var fechax = DateTime.Now;
                var convertir = DateTime.TryParse(desde, out fechax);
                if (convertir == true)
                {
                    predicado = predicado.And(d => d.fecha_solicitud_enc >= fechax);
                }
            }

            if (!string.IsNullOrWhiteSpace(hasta))
            {
                //inicializo variable fecha
                var fechax = DateTime.Now;
                var convertir = DateTime.TryParse(hasta, out fechax);
                if (convertir == true)
                {
                    fechax = fechax.AddDays(1);
                    predicado = predicado.And(d => d.fecha_solicitud_enc <= fechax);
                }
            }
            int registrostotales = context.vw_solicitud_repuestos_ordentaller.Where(predicado).Count();
            //si el ordenamiento es ascendente o descendente es distinto
            if (pageSize == -1)
            {
                pageSize = registrostotales;
            }

            if (sortColumnDir == "asc")
            {
                List<vw_solicitud_repuestos_ordentaller> query2 = context.vw_solicitud_repuestos_ordentaller.Where(predicado)
                    .OrderBy(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
                var query = query2.Select(d => new
                {
                    d.codigoentrada,
                    d.plan_mayor,
                    d.descripcion,
                    d.nombretercero,
                    d.nombre_bodega,
                    d.idorden,
                    d.idbodega,
                    referencias = context.tsolicitudrepuestosot.Where(e => e.idorden == d.idorden && e.pedido == false)
                        .Select(e => new datasolicitud
                        {
                            idrepuesto = e.tdetallerepuestosot.idrepuesto,
                            ref_descripcion = e.tdetallerepuestosot.icb_referencia.ref_descripcion
                        }).ToList(),
                    d.fecha2
                }).ToList();

                query.ForEach(item =>
                    item.referencias.ForEach(item2 =>
                        item2.stockbodega = buscarStockBodega(item2.idrepuesto, item.idbodega)));
                query.ForEach(item =>
                    item.referencias.ForEach(item2 =>
                        item2.stockotras = buscarStockOtras(item2.idrepuesto, item.idbodega)));
                query.ForEach(item => item.referencias.ForEach(item2 =>
                    item2.stockreemplazo = buscarStockReemplazo(item2.idrepuesto, item.idbodega)));

                int contador = query.Count();
                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<vw_solicitud_repuestos_ordentaller> query2 = context.vw_solicitud_repuestos_ordentaller.Where(predicado)
                    .OrderByDescending(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();

                var query = query2.Select(d => new
                {
                    d.codigoentrada,
                    d.plan_mayor,
                    d.descripcion,
                    d.nombretercero,
                    d.nombre_bodega,
                    d.idorden,
                    d.idbodega,
                    referencias = context.tsolicitudrepuestosot.Where(e => e.idorden == d.idorden).Select(e =>
                        new datasolicitud
                        {
                            idrepuesto = e.tdetallerepuestosot.idrepuesto,
                            ref_descripcion = e.tdetallerepuestosot.icb_referencia.ref_descripcion
                        }).ToList(),
                    d.fecha2
                }).ToList();

                query.ForEach(item =>
                    item.referencias.ForEach(item2 =>
                        item2.stockbodega = buscarStockBodega(item2.idrepuesto, item.idbodega)));
                query.ForEach(item =>
                    item.referencias.ForEach(item2 =>
                        item2.stockotras = buscarStockOtras(item2.idrepuesto, item.idbodega)));
                query.ForEach(item => item.referencias.ForEach(item2 =>
                    item2.stockreemplazo = buscarStockReemplazo(item2.idrepuesto, item.idbodega)));
                int contador = query.Count();
                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }
        }

        public int buscarStockOtras(string repuesto, int bodega)
        {
            Expression<Func<vw_inventario_hoy, bool>> predicado = PredicateBuilder.True<vw_inventario_hoy>();

            predicado = predicado.And(d => d.bodega != bodega);
            if (!string.IsNullOrWhiteSpace(repuesto))
            {
                predicado = predicado.And(d => d.ref_codigo == repuesto);
            }

            int stock = 0;
            List<vw_inventario_hoy> existestock = context.vw_inventario_hoy.Where(predicado).ToList();
            if (existestock.Count > 0)
            {
                decimal stock2 = existestock.Sum(d => d.stock);
                stock = Convert.ToInt32(stock2);
            }

            return stock;
        }

        public List<listabodegas> buscarStockOtrasCantidad(string repuesto, int bodega, int cantidad)
        {
            Expression<Func<vw_inventario_hoy, bool>> predicado = PredicateBuilder.True<vw_inventario_hoy>();

            predicado = predicado.And(d => d.bodega != bodega);
            if (!string.IsNullOrWhiteSpace(repuesto))
            {
                predicado = predicado.And(d => d.ref_codigo == repuesto);
            }
            predicado = predicado.And(d => d.stock >= cantidad);

            //int stock = 0;
            List<vw_inventario_hoy> existestock = context.vw_inventario_hoy.Where(predicado).ToList();
            var lista = new List<listabodegas>();
            if (existestock.Count > 0)
            {
                lista = existestock.Select(d => new listabodegas
                {
                    idbodega = d.bodega,
                    nombrebodega = d.nombreBodega,
                    stockbodega = d.stock
                }).ToList();

            }

            return lista;
        }
        public int buscarStockBodega(string repuesto, int bodega)
        {
            Expression<Func<vw_inventario_hoy, bool>> predicado = PredicateBuilder.True<vw_inventario_hoy>();

            predicado = predicado.And(d => d.bodega == bodega);
            if (!string.IsNullOrWhiteSpace(repuesto))
            {
                predicado = predicado.And(d => d.ref_codigo == repuesto);
            }

            decimal stock2 = context.vw_inventario_hoy.Where(predicado).Select(d => d.stock).FirstOrDefault();
            int stock = Convert.ToInt32(stock2);
            return stock;
        }


        public JsonResult stockBodega(string repuesto, int bodega)
        {
            Expression<Func<vw_inventario_hoy, bool>> predicado = PredicateBuilder.True<vw_inventario_hoy>();

            predicado = predicado.And(d => d.bodega == bodega);
            if (!string.IsNullOrWhiteSpace(repuesto))
            {
                predicado = predicado.And(d => d.ref_codigo == repuesto);
            }

            var data = context.vw_inventario_hoy.Where(predicado).Select(d => d.nombreBodega).FirstOrDefault();

            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        public int buscarStockReemplazo(string idrepuesto, int? idbodega)
        {
            if (!string.IsNullOrWhiteSpace(idrepuesto) && idbodega != null)
            {
                //busco la bodega que exusta
                bodega_concesionario bod = context.bodega_concesionario.Where(d => d.id == idbodega).FirstOrDefault();
                //busco que exista la referencia
                icb_referencia refer = context.icb_referencia.Where(d => d.ref_codigo == idrepuesto).FirstOrDefault();
                if (bod != null && refer != null)
                {
                    int encontrado = 0;
                    int lista = 0;

                    //busco el reemplazo de esta referencia en ESTA bodega
                    rremplazos reemplazo = context.rremplazos.Where(d => d.referencia == idrepuesto && d.alterno != idrepuesto)
                        .FirstOrDefault();
                    //si tiene reemplazo
                    if (reemplazo != null)
                    {
                        string idreemp = reemplazo.alterno;
                        //busco si tiene stock ese reemplazo
                        do
                        {
                            decimal stock = context.vw_inventario_hoy
                                .Where(e => e.ref_codigo == idreemp && e.bodega == idbodega).Select(e => e.stock)
                                .FirstOrDefault();
                            if (stock > 0)
                            {
                                lista = Convert.ToInt32(stock);
                                encontrado = 1;
                            }
                            else
                            {
                                //veo si ese reemplazo tiene reemplazo y vuelvo
                                rremplazos reemplazo2 = context.rremplazos
                                    .Where(d => d.referencia == idreemp && d.alterno != idreemp).FirstOrDefault();
                                if (reemplazo2 != null)
                                {
                                    idreemp = reemplazo2.alterno;
                                    encontrado = 0;
                                }
                                else
                                {
                                    encontrado = 1;
                                }
                            }
                        } while (encontrado == 0);
                    }

                    return lista;
                }

                return 0;
            }

            return 0;
        }

        public string BuscarUbiRptoPaginadas(int Bodega, string Referencia)
        {
            var data = (from ubicaciones in context.ubicacion_repuesto
                        join bodega in context.bodega_concesionario
                            on ubicaciones.bodega equals bodega.id
                        join ubiBod in context.ubicacion_repuestobod
                            on ubicaciones.ubicacion equals ubiBod.id
                        join referencia in context.icb_referencia
                            on ubicaciones.codigo equals referencia.ref_codigo
                        join area in context.area_bodega
                            on ubicaciones.idarea equals area.areabod_id into zz
                        where bodega.id == Bodega && referencia.ref_codigo == Referencia
                        from area in zz.DefaultIfEmpty()
                        select new
                        {
                            ubiBod.descripcion,
                        }).ToString();
            return data;
        }

        public string ObtenerUbicacion(string repuesto, int bodega)
        {

            string resultado = "";
            var existe = context.ubicacion_repuesto.Where(c => c.codigo == repuesto && c.bodega == bodega).FirstOrDefault();
            if (existe != null)
            {
                var idUbicacion = context.ubicacion_repuestobod.Where(d => d.id == existe.ubicacion).FirstOrDefault();
                resultado = idUbicacion.descripcion;
            }
            return resultado;

        }

        public string ObtenerNotaubicacion(string repuesto, int bodega)
        {

            string resultado = "";
            var existe = context.ubicacion_repuesto.Where(c => c.codigo == repuesto && c.bodega == bodega).FirstOrDefault();
            if (existe != null)
            {
                //var idUbicacion = context.ubicacion_repuestobod.Where(d => d.id == existe.ubicacion).FirstOrDefault();
                var idUbicacion = context.ubicacion_repuesto.Where(d => d.id == existe.id).FirstOrDefault();
                resultado = idUbicacion.notaUbicacion;
            }
            return resultado;

        }

        public JsonResult filtroSolicitudesDespacho(int[] bodega, string filtroGeneral)
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

            Expression<Func<vw_browser_solicitudes_despacho_taller, bool>> predicado = PredicateBuilder.True<vw_browser_solicitudes_despacho_taller>();
            Expression<Func<vw_browser_solicitudes_despacho_taller, bool>> predicado2 = PredicateBuilder.False<vw_browser_solicitudes_despacho_taller>();
            Expression<Func<vw_browser_solicitudes_despacho_taller, bool>> predicado3 = PredicateBuilder.False<vw_browser_solicitudes_despacho_taller>();
            Expression<Func<vw_browser_solicitudes_despacho_taller, bool>> predicado4 = PredicateBuilder.False<vw_browser_solicitudes_despacho_taller>();

            if (bodega.Count() > 0 && bodega[0] != 0)
            {
                foreach (int item in bodega)
                {
                    predicado2 = predicado2.Or(d => d.bodega == item);
                }

                predicado = predicado.And(predicado2);
            }

            predicado = predicado.And(d => d.procesado == 0);
            if (!string.IsNullOrWhiteSpace(filtroGeneral))
            {
                predicado3 = predicado3.Or(d => 1 == 1 && d.idstring.ToString().Contains(filtroGeneral));
                predicado3 = predicado3.Or(d => 1 == 1 && d.nombre.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.bodegaorigen.Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.bodegadestino.Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.fecha2.Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.marcvh_nombre.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.modvh_nombre.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.anio_vh2.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.codigoentrada.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.tipodespacho.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.referencias.ToUpper().Contains(filtroGeneral.ToUpper()));
                //predicado3 = predicado3.Or(d => 1 == 1 && d.descripcion.ToUpper().Contains(filtroGeneral.ToUpper()));// se agrego campo
                predicado = predicado.And(predicado3);
            }

            int registrostotales = context.vw_browser_solicitudes_despacho_taller.Where(predicado).Count();
            //si el ordenamiento es ascendente o descendente es distinto
            if (pageSize == -1)
            {
                pageSize = registrostotales;
            }

            if (sortColumnDir == "asc")
            {
                List<vw_browser_solicitudes_despacho_taller> query2 = context.vw_browser_solicitudes_despacho_taller.Where(predicado)
                    .OrderBy(GetColumnName2(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
                var query = query2.Select(d => new
                {
                    d.idstring,
                    d.codigoentrada,
                    d.tipodespacho,
                    d.fecha2,
                    d.nombre,
                    d.marcvh_nombre,
                    d.modvh_nombre,
                    d.anio_vh2,
                    d.bodegaorigen,
                    d.bodegadestino,
                    d.bodega2,
                    referencias = context.lineas_documento.Where(e => e.id_encabezado == d.id).Select(e =>
                        new datapedido
                        {
                            idrepuesto = e.codigo,
                            ref_descripcion = e.icb_referencia.ref_descripcion,
                            //idrepuesto = e.tsolicitudrepuestosot.tdetallerepuestosot.idrepuesto,
                            //ref_descripcion = e.tsolicitudrepuestosot.tdetallerepuestosot.icb_referencia.ref_descripcion,


                        }).ToList(),
                    //d.descripcion,
                    d.orden_taller,
                    d.id,
                    d.bodega,
                    //notaUbicacion= context.ubicacion_repuesto.Select(e =>e.notaUbicacion).ToList()
                }).ToList();

                query.ForEach(item =>
                    item.referencias.ForEach(item2 =>
                        item2.stockbodega = buscarStockBodega(item2.idrepuesto, item.bodega)
                        ));

                query.ForEach(item =>
                                   item.referencias.ForEach(item2 =>
                                       item2.ubicacion = ObtenerUbicacion(item2.idrepuesto, item.bodega)));

                //query.ForEach(item =>
                //   item.referencias.ForEach(item2 =>
                //       item2.notaUbicacion = ObtenerNotaubicacion(item2.idrepuesto, item.bodega)));


                int contador = query.Count();
                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<vw_browser_solicitudes_despacho_taller> query2 = context.vw_browser_solicitudes_despacho_taller.Where(predicado)
                    .OrderByDescending(GetColumnName2(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();

                var query = query2.Select(d => new
                {
                    d.idstring,
                    d.codigoentrada,
                    d.tipodespacho,
                    d.fecha2,
                    d.nombre,
                    d.marcvh_nombre,
                    d.modvh_nombre,
                    d.anio_vh2,
                    d.bodegaorigen,
                    d.bodegadestino,
                    referencias = context.lineas_documento.Where(e => e.id_encabezado == d.id).Select(e =>
                        new datapedido
                        {
                            idrepuesto = e.tsolicitudrepuestosot.tdetallerepuestosot.idrepuesto,
                            ref_descripcion = e.tsolicitudrepuestosot.tdetallerepuestosot.icb_referencia.ref_descripcion
                        }).ToList(),
                    //d.descripcion,
                    d.id,
                    d.orden_taller,
                    d.bodega
                }).ToList();

                int contador = query.Count();

                query.ForEach(item =>
                    item.referencias.ForEach(item2 =>
                        item2.stockbodega = buscarStockBodega(item2.idrepuesto, item.bodega)));

                query.ForEach(item =>
                   item.referencias.ForEach(item2 =>
                       item2.ubicacion = ObtenerUbicacion(item2.idrepuesto, item.bodega)));

                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult filtroSolicitudesDespachoProcesado(int[] bodega, string filtroGeneral)
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

            Expression<Func<vw_browser_solicitudes_despacho_taller, bool>> predicado = PredicateBuilder.True<vw_browser_solicitudes_despacho_taller>();
            Expression<Func<vw_browser_solicitudes_despacho_taller, bool>> predicado2 = PredicateBuilder.False<vw_browser_solicitudes_despacho_taller>();
            Expression<Func<vw_browser_solicitudes_despacho_taller, bool>> predicado3 = PredicateBuilder.False<vw_browser_solicitudes_despacho_taller>();

            if (bodega.Count() > 0 && bodega[0] != 0)
            {
                foreach (int item in bodega)
                {
                    predicado2 = predicado2.Or(d => d.bodega == item);
                }

                predicado = predicado.And(predicado2);
            }

            predicado = predicado.And(d => d.procesado > 0);
            if (!string.IsNullOrWhiteSpace(filtroGeneral))
            {
                predicado3 = predicado3.Or(d => 1 == 1 && d.idstring.ToString().Contains(filtroGeneral));
                predicado3 = predicado3.Or(d => 1 == 1 && d.nombre.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.bodegaorigen.Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.bodegadestino.Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.fecha2.Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.marcvh_nombre.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.modvh_nombre.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.anio_vh2.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.codigoentrada.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.tipodespacho.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.referencias.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado = predicado.And(predicado3);
            }

            int registrostotales = context.vw_browser_solicitudes_despacho_taller.Where(predicado).Count();
            //si el ordenamiento es ascendente o descendente es distinto
            if (pageSize == -1)
            {
                pageSize = registrostotales;
            }

            if (sortColumnDir == "asc")
            {
                List<vw_browser_solicitudes_despacho_taller> query2 = context.vw_browser_solicitudes_despacho_taller.Where(predicado)
                    .OrderBy(GetColumnName2(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
                var query = query2.Select(d => new
                {
                    d.idstring,
                    d.codigoentrada,
                    d.tipodespacho,
                    d.fecha2,
                    d.nombre,
                    d.marcvh_nombre,
                    d.modvh_nombre,
                    d.anio_vh2,
                    d.bodegaorigen,
                    d.bodegadestino,
                    referencias = context.lineas_documento.Where(e => e.id_encabezado == d.id).Select(e =>
                        new datapedido
                        {
                            idrepuesto = e.tsolicitudrepuestosot.tdetallerepuestosot.idrepuesto,
                            ref_descripcion = e.tsolicitudrepuestosot.tdetallerepuestosot.icb_referencia.ref_descripcion
                        }).ToList(),
                    d.id,
                    d.bodega
                }).ToList();

                query.ForEach(item =>
                    item.referencias.ForEach(item2 =>
                        item2.stockbodega = buscarStockBodega(item2.idrepuesto, item.bodega)));

                query.ForEach(item =>
                   item.referencias.ForEach(item2 =>
                       item2.ubicacion = ObtenerUbicacion(item2.idrepuesto, item.bodega)));

                int contador = query.Count();
                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<vw_browser_solicitudes_despacho_taller> query2 = context.vw_browser_solicitudes_despacho_taller.Where(predicado)
                    .OrderByDescending(GetColumnName2(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();

                var query = query2.Select(d => new
                {
                    d.idstring,
                    d.codigoentrada,
                    d.tipodespacho,
                    d.fecha2,
                    d.nombre,
                    d.marcvh_nombre,
                    d.modvh_nombre,
                    d.anio_vh2,
                    d.bodegaorigen,
                    d.bodegadestino,
                    referencias = context.lineas_documento.Where(e => e.id_encabezado == d.id).Select(e =>
                        new datapedido
                        {
                            idrepuesto = e.tsolicitudrepuestosot.tdetallerepuestosot.idrepuesto,
                            ref_descripcion = e.tsolicitudrepuestosot.tdetallerepuestosot.icb_referencia.ref_descripcion
                        }).ToList(),
                    d.id,
                    d.bodega
                }).ToList();
                int contador = query.Count();

                query.ForEach(item =>
                    item.referencias.ForEach(item2 =>
                        item2.stockbodega = buscarStockBodega(item2.idrepuesto, item.bodega)));

                query.ForEach(item =>
                    item.referencias.ForEach(item2 =>
                        item2.ubicacion = ObtenerUbicacion(item2.idrepuesto, item.bodega)));

                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult filtroTrasladoSinProcesar(int[] bodega, string filtroGeneral)
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

            Expression<Func<vw_browser_recepcion_traslados_ot, bool>> predicado = PredicateBuilder.True<vw_browser_recepcion_traslados_ot>();
            Expression<Func<vw_browser_recepcion_traslados_ot, bool>> predicado2 = PredicateBuilder.False<vw_browser_recepcion_traslados_ot>();
            Expression<Func<vw_browser_recepcion_traslados_ot, bool>> predicado3 = PredicateBuilder.False<vw_browser_recepcion_traslados_ot>();

            if (bodega.Count() > 0 && bodega[0] != 0)
            {
                foreach (int item in bodega)
                {
                    predicado2 = predicado2.Or(d => d.bodega_destino == item);
                }

                predicado = predicado.And(predicado2);
            }

            if (!string.IsNullOrWhiteSpace(filtroGeneral))
            {
                predicado3 = predicado3.Or(d => 1 == 1 && d.idstring.ToString().Contains(filtroGeneral));
                predicado3 = predicado3.Or(d => 1 == 1 && d.nombre.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.bodegaorigen.Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.bodegadestino.Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.fecha2.Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.marcvh_nombre.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.modvh_nombre.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.anio_vh2.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.codigoentrada.ToUpper().Contains(filtroGeneral.ToUpper()));
                // predicado3 = predicado3.Or(d => 1 == 1 && d.referencias.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado = predicado.And(predicado3);
            }

            int registrostotales = context.vw_browser_recepcion_traslados_ot.Where(predicado).Count();
            //si el ordenamiento es ascendente o descendente es distinto
            if (pageSize == -1)
            {
                pageSize = registrostotales;
            }

            if (sortColumnDir == "asc")
            {
                List<vw_browser_recepcion_traslados_ot> query2 = context.vw_browser_recepcion_traslados_ot.Where(predicado)
                    .OrderBy(GetColumnName3(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
                var query = query2.Select(d => new
                {
                    d.idstring,
                    d.codigoentrada,
                    d.tipodespacho,
                    d.fecha2,
                    d.nombre,
                    d.marcvh_nombre,
                    d.modvh_nombre,
                    d.anio_vh2,
                    d.bodegaorigen,
                    d.bodegadestino,
                    referencias = context.recibidotraslados
                        .Where(e => e.idtraslado == d.idtraslado && e.recibido == false).Select(e => new datapedido
                        {
                            idrepuesto = e.icb_referencia.ref_codigo,
                            ref_descripcion = e.icb_referencia.ref_descripcion,
                            stockbodega = e.cantidad
                        }).ToList(),
                    d.bodega,
                    id = Convert.ToInt32(d.idstring)
                }).ToList();

                query.ForEach(item =>
                   item.referencias.ForEach(item2 =>
                       item2.ubicacion = ObtenerUbicacion(item2.idrepuesto, item.bodega)));

                int contador = query.Count();
                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<vw_browser_recepcion_traslados_ot> query2 = context.vw_browser_recepcion_traslados_ot.Where(predicado)
                    .OrderByDescending(GetColumnName3(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();

                var query = query2.Select(d => new
                {
                    d.idstring,
                    d.codigoentrada,
                    d.tipodespacho,
                    d.fecha2,
                    d.nombre,
                    d.marcvh_nombre,
                    d.modvh_nombre,
                    d.anio_vh2,
                    d.bodegaorigen,
                    d.bodegadestino,
                    referencias = context.recibidotraslados
                        .Where(e => e.idtraslado == d.idtraslado && e.recibido == false).Select(e => new datapedido
                        {
                            idrepuesto = e.icb_referencia.ref_codigo,
                            ref_descripcion = e.icb_referencia.ref_descripcion,
                            stockbodega = e.cantidad
                        }).ToList(),
                    d.bodega,
                    id = Convert.ToInt32(d.idstring)
                }).ToList();
                int contador = query.Count();

                query.ForEach(item =>
                   item.referencias.ForEach(item2 =>
                       item2.ubicacion = ObtenerUbicacion(item2.idrepuesto, item.bodega)));

                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }
        }

        //crea funcion que lee el pedido, y el tipo de pedido
        //las lineas de documento del pedido
        //manda a formulario. Copiar el formulario del formulario de traslado. No lleva tarifa cliente.
        public ActionResult procesarOrdenTraslado(int? id, int? menu)
        {
            if (Session["user_usuarioid"] != null)
            {
                BuscarFavoritos(menu);
                //veo si existe el traslado de repuestos
                encab_documento existe = context.encab_documento.Where(d => d.idencabezado == id && d.orden_taller != null)
                    .FirstOrDefault();
                if (existe != null)
                {
                    //verificar si ya fue procesada la orden(con el tipo de documento que vaya yo a crear a continuacion
                    DespachoOrden modelo = new DespachoOrden
                    {
                        BodegaDestino = existe.bodega_destino != null ? existe.bodega_destino.Value : existe.bodega,
                        BodegaOrigen = existe.bodega,
                        idorden = existe.orden_taller.Value,
                        idpedido = existe.idencabezado,
                        TipoDocumento = existe.tipo
                    };

                    icb_sysparameter tipot = context.icb_sysparameter.Where(d => d.syspar_cod == "P108").FirstOrDefault();
                    int tipo = tipot != null ? Convert.ToInt32(tipot.syspar_value) : 3076;

                    IQueryable<tp_doc_registros> TipoDocumento = context.tp_doc_registros.Where(x => x.tpdoc_id == tipo);
                    ViewBag.TipoDocumento = new SelectList(TipoDocumento, "tpdoc_id", "tpdoc_nombre");
                    ViewBag.bodegaOrigenDes = context.bodega_concesionario.Where(d => d.id == existe.bodega)
                        .Select(d => d.bodccs_nombre).FirstOrDefault();
                    ViewBag.cliente = existe.tencabezaorden.icb_terceros.doc_tercero + " - " +
                                      (!string.IsNullOrWhiteSpace(existe.tencabezaorden.icb_terceros.razon_social)
                                          ? existe.tencabezaorden.icb_terceros.razon_social
                                          : existe.tencabezaorden.icb_terceros.prinom_tercero + " " +
                                            existe.tencabezaorden.icb_terceros.segnom_tercero + " " +
                                            existe.tencabezaorden.icb_terceros.apellido_tercero + " " +
                                            existe.tencabezaorden.icb_terceros.segapellido_tercero);
                    if (existe.bodega_destino != null)
                    {
                        ViewBag.bodegaDestinoDes = context.bodega_concesionario
                            .Where(d => d.id == existe.bodega_destino).Select(d => d.bodccs_nombre).FirstOrDefault();
                    }
                    else
                    {
                        ViewBag.bodegaDestinoDes = context.bodega_concesionario.Where(d => d.id == existe.bodega)
                            .Select(d => d.bodccs_nombre).FirstOrDefault();
                    }

                    ViewBag.codigoorden = context.tencabezaorden.Where(d => d.id == existe.orden_taller)
                        .Select(d => d.codigoentrada).FirstOrDefault();

                    return View(modelo);
                }

                TempData["mensaje_error"] = "NO EXISTE LA ORDEN DE DESPACHO";
                BuscarFavoritos(menu);
                return RedirectToAction("browserSolicitudesDespachoOT", "Almacen", new { menu });
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        public ActionResult procesarOrdenTraslado(DespachoOrden modelo, int? menu)
        {
            if (Session["user_usuarioid"] != null)
            {
                //veo si la orden de traslado existe
                BuscarFavoritos(menu);

                encab_documento existe = context.encab_documento
                    .Where(d => d.idencabezado == modelo.idpedido && d.orden_taller != null).FirstOrDefault();
                if (existe != null)
                {
                    if (ModelState.IsValid)
                    {
                        using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                        {
                            try
                            {
                                bool listaref = int.TryParse(Request["lista_referencias"], out int lista);
                                string procesar = "";

                                #region recepcion del traslado

                                //tipo de documento recepcion de traslado taller
                                icb_sysparameter tipotra = context.icb_sysparameter.Where(d => d.syspar_cod == "P108")
                                    .FirstOrDefault();
                                int tipotraslado = tipotra != null ? Convert.ToInt32(tipotra.syspar_value) : 3076;
                                //busco el tipo de documento traslados taller
                                icb_sysparameter docu = context.icb_sysparameter.Where(d => d.syspar_cod == "P37").FirstOrDefault();
                                int documentoTaller = docu != null ? Convert.ToInt32(docu.syspar_value) : 11;
                                //busco el tipo de documento entrada de repuestos inventario
                                icb_sysparameter docureentrada = context.icb_sysparameter.Where(d => d.syspar_cod == "P40")
                                    .FirstOrDefault();
                                int tipoEntradaInventario = docureentrada != null
                                    ? Convert.ToInt32(docureentrada.syspar_value)
                                    : 1029;
                                int guardadoentrada = 0;
                                //cargo el documento de despacho de repuestos
                                icb_sysparameter buscarNit = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P33");
                                int nitDespacho = buscarNit != null ? Convert.ToInt32(buscarNit.syspar_value) : 0;
                                //busco el pedido de donde salió el traslado para despues cruzarlo
                                encab_documento pedido = context.encab_documento.Where(d => d.idencabezado == modelo.idpedido)
                                    .FirstOrDefault();

                                tdetallerepuestosot orden = context.tdetallerepuestosot.Where(d => d.idorden == pedido.orden_taller).FirstOrDefault();

                                long numeroConsecutivo = 0;
                                grupoconsecutivos buscarGrupoConsecutivos = context.grupoconsecutivos.FirstOrDefault(x =>
                                    x.documento_id == tipotraslado && x.bodega_id == pedido.bodega);
                                int numeroGrupo = buscarGrupoConsecutivos != null ? buscarGrupoConsecutivos.grupo : 0;
                                //consecutivo de recepcion de traslado
                                ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
                                icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
                                tp_doc_registros buscarTipoDocRegistro =
                                    context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == tipotraslado);
                                numeroConsecutivoAux = gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro,
                                    tipotraslado, modelo.BodegaDestino);

                                if (numeroConsecutivoAux != null)
                                {
                                    numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                                }
                                else
                                {
                                    TempData["mensaje_error"] =
                                        "No existe un numero consecutivo asignado para este tipo de documento";
                                }

                                //consecutivo de entrada a inventario
                                long numeroConsecutivoEntrada = 0;
                                grupoconsecutivos buscarGrupoConsecutivosEntrada = context.grupoconsecutivos.FirstOrDefault(x =>
                                    x.documento_id == tipoEntradaInventario && x.bodega_id == modelo.BodegaDestino);
                                int numeroGrupoEntrada = buscarGrupoConsecutivosEntrada != null
                                    ? buscarGrupoConsecutivosEntrada.grupo
                                    : 0;
                                //consecutivo de entrada a inventario
                                ConsecutivosGestion gestionConsecutivoEntrada = new ConsecutivosGestion();
                                icb_doc_consecutivos numeroConsecutivoAuxEntrada = new icb_doc_consecutivos();
                                tp_doc_registros buscarTipoDocRegistroEntrada =
                                    context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == tipoEntradaInventario);
                                numeroConsecutivoAuxEntrada =
                                    gestionConsecutivoEntrada.BuscarConsecutivo(buscarTipoDocRegistroEntrada,
                                        tipoEntradaInventario, modelo.BodegaDestino);

                                List<string> listaReferencias = new List<string>();
                                int numero_repuestos = lista;
                                List<decimal> costoPromedioTotal2 = new List<decimal>();

                                int sequ = 1;
                                List<string> codigosReferencias = new List<string>();

                                int valor_totalenca = 0;
                                for (int i = 0; i <= numero_repuestos; i++)
                                {
                                    string referencia_codigo = Request["referencia" + i];
                                    if (!string.IsNullOrEmpty(referencia_codigo))
                                    {
                                        codigosReferencias.Add(referencia_codigo);
                                    }
                                }

                                decimal costoPromedioTotal = 0;
                                for (int n = 0; n < numero_repuestos; n++)
                                {
                                    string referencia_cantidadx = Request["cantidadFacturar" + n];
                                    string referenciax = Request["referencia" + n];
                                    costoPromedioTotal =
                                        costoPromedioTotal + traercosto(referenciax) *
                                        Convert.ToInt32(referencia_cantidadx);
                                }

                                //creo el documento de recepción de traslado
                                encab_documento crearEncabezado = new encab_documento
                                {
                                    notas = modelo.Notas,
                                    tipo = tipotraslado,
                                    bodega = modelo.BodegaOrigen,
                                    numero = numeroConsecutivo,
                                    ///////crearEncabezado.documento = modelo.Referencia;
                                    fecha = DateTime.Now,
                                    fec_creacion = DateTime.Now,
                                    impoconsumo = 0,
                                    destinatario = pedido.userid_creacion,
                                    bodega_destino = modelo.BodegaDestino,
                                    //crearEncabezado.perfilcontable = perfilContableTraslado;
                                    nit = pedido.nit,
                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                    orden_taller = pedido.orden_taller
                                };
                                context.encab_documento.Add(crearEncabezado);

                                bool guardar = context.SaveChanges() > 0;
                                encab_documento buscarUltimoEncabezado = context.encab_documento
                                    .Where(x => x.idencabezado == crearEncabezado.idencabezado).FirstOrDefault();
                                encab_documento eg = buscarUltimoEncabezado;
                                //totales de movimientos contables
                                decimal totalCreditos = 0;
                                decimal totalDebitos = 0;

                                //guardo las lineas de recepción
                                for (int i = 0; i <= numero_repuestos; i++)
                                {
                                    if (Request["referencia" + i] != null && Request["cantidadFacturar" + i] != null)
                                    {
                                        string referencia_codigo = Request["referencia" + i];
                                        string referencia_cantidad = Request["cantidadFacturar" + i];
                                        string referencia_costo = Request["valorAntesIvaReferencia" + i];
                                        string idsolicitudrepuesto2 = Request["idsolicitudreferenciaP" + i];
                                        bool convertirsolicitud =
                                            int.TryParse(idsolicitudrepuesto2, out int idsolicitudrepuesto);
                                        if (string.IsNullOrEmpty(referencia_codigo) ||
                                            string.IsNullOrEmpty(referencia_cantidad))
                                        {
                                            // Significa que la agregaron y la eliminaron
                                        }
                                        else
                                        {
                                            listaReferencias.Add(referencia_codigo);
                                            vw_promedio buscarPromedio = context.vw_promedio.FirstOrDefault(x =>
                                                x.codigo == referencia_codigo && x.ano == DateTime.Now.Year &&
                                                x.mes == DateTime.Now.Month);
                                            decimal promedio = buscarPromedio != null ? buscarPromedio.Promedio ?? 0 : 0;
                                            //para poner el promedio como el costo de precios
                                            promedio = traercosto(referencia_codigo);

                                            referencia_costo = promedio.ToString();

                                            decimal costototal = promedio * Convert.ToDecimal(referencia_cantidad, miCultura);
                                            decimal baseUnitario = promedio * Convert.ToDecimal(referencia_cantidad, miCultura);

                                            decimal cr = promedio * Convert.ToDecimal(referencia_cantidad, miCultura);


                                            referencias_inven buscarReferenciasInvenDestino =
                                                context.referencias_inven.FirstOrDefault(x =>
                                                    x.codigo == referencia_codigo && x.ano == DateTime.Now.Year &&
                                                    x.mes == DateTime.Now.Month && x.bodega == modelo.BodegaDestino);
                                            if (buscarReferenciasInvenDestino != null)
                                            {
                                                buscarReferenciasInvenDestino.can_ent =
                                                    buscarReferenciasInvenDestino.can_ent +
                                                    Convert.ToInt32(referencia_cantidad);
                                                buscarReferenciasInvenDestino.cos_ent =
                                                    buscarReferenciasInvenDestino.cos_ent +
                                                    promedio * Convert.ToInt32(referencia_cantidad);
                                                buscarReferenciasInvenDestino.can_tra =
                                                    buscarReferenciasInvenDestino.can_tra +
                                                    Convert.ToInt32(referencia_cantidad);
                                                buscarReferenciasInvenDestino.cos_tra =
                                                    buscarReferenciasInvenDestino.cos_tra +
                                                    promedio * Convert.ToInt32(referencia_cantidad);
                                                context.Entry(buscarReferenciasInvenDestino).State =
                                                    EntityState.Modified;
                                            }
                                            else
                                            {
                                                referencias_inven crearReferencia = new referencias_inven
                                                {
                                                    bodega = modelo.BodegaDestino,
                                                    codigo = referencia_codigo,
                                                    ano = (short)DateTime.Now.Year,
                                                    mes = (short)DateTime.Now.Month,
                                                    can_ini = 0,
                                                    can_ent = Convert.ToInt32(referencia_cantidad),
                                                    can_sal = 0,
                                                    cos_ent =
                                                    traercosto(referencia_codigo) +
                                                    promedio * Convert.ToInt32(referencia_cantidad),
                                                    can_tra = Convert.ToInt32(referencia_cantidad),
                                                    cos_tra =
                                                    traercosto(referencia_codigo) +
                                                    promedio * Convert.ToInt32(referencia_cantidad),
                                                    modulo = "R"
                                                };

                                                context.referencias_inven.Add(crearReferencia);
                                                context.SaveChanges();
                                            }


                                            lineas_documento crearLineasDestino = new lineas_documento
                                            {
                                                codigo = referencia_codigo,
                                                fec_creacion = DateTime.Now,
                                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                                nit = pedido.nit,
                                                cantidad = !string.IsNullOrEmpty(referencia_cantidad)
                                                    ? Convert.ToDecimal(referencia_cantidad, miCultura)
                                                    : 0,
                                                bodega = modelo.BodegaDestino,
                                                seq = sequ,
                                                estado = true,
                                                fec = DateTime.Now,
                                                costo_unitario = !string.IsNullOrEmpty(referencia_costo)
                                                    ? Convert.ToDecimal(referencia_costo, miCultura)
                                                    : 0,
                                                id_encabezado = buscarUltimoEncabezado != null
                                                    ? buscarUltimoEncabezado.idencabezado
                                                    : 0
                                            };
                                            if (idsolicitudrepuesto > 0)
                                            {
                                                crearLineasDestino.id_solicitud_repuesto = idsolicitudrepuesto;
                                            }

                                            context.lineas_documento.Add(crearLineasDestino);
                                            int guardar2 = context.SaveChanges();

                                            //AQUI VA EL MOVIMIENTO CONTABLE

                                            #region parte contable. Si está bien hago el commit

                                            //busco el perfil contable de RTR (Recepcion de repuestos de Inventario)
                                            perfil_contable_documento perfilcont = context.perfil_contable_documento
                                                .Where(d => d.tipo == buscarTipoDocRegistro.tpdoc_id).FirstOrDefault();
                                            if (perfilcont != null)
                                            {
                                                var parametrosCuentasVerificar =
                                                    (from perfil in context.perfil_cuentas_documento
                                                     join nombreParametro in context.paramcontablenombres
                                                         on perfil.id_nombre_parametro equals nombreParametro.id
                                                     join cuenta in context.cuenta_puc
                                                         on perfil.cuenta equals cuenta.cntpuc_id
                                                     where perfil.id_perfil == perfilcont.id
                                                     select new
                                                     {
                                                         perfil.id,
                                                         perfil.id_nombre_parametro,
                                                         perfil.cuenta,
                                                         perfil.centro,
                                                         perfil.id_perfil,
                                                         nombreParametro.descripcion_parametro,
                                                         cuenta.cntpuc_numero
                                                     }).ToList();

                                                int secuencia = 1;
                                                List<cuentas_valores> ids_cuentas_valores = new List<cuentas_valores>();
                                                centro_costo centroValorCero =
                                                    context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                                                int idCentroCero = centroValorCero != null
                                                    ? Convert.ToInt32(centroValorCero.centcst_id)
                                                    : 0;
                                                List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();

                                                foreach (var parametro in parametrosCuentasVerificar)
                                                {
                                                    string descripcionParametro = context.paramcontablenombres
                                                        .FirstOrDefault(x => x.id == parametro.id_nombre_parametro)
                                                        .descripcion_parametro;
                                                    cuenta_puc buscarCuenta =
                                                        context.cuenta_puc.FirstOrDefault(x =>
                                                            x.cntpuc_id == parametro.cuenta);

                                                    if (buscarCuenta != null)
                                                    {
                                                        if (parametro.id_nombre_parametro == 9 &&
                                                            Convert.ToDecimal(costoPromedioTotal, miCultura) != 0 //costo promedio
                                                            || parametro.id_nombre_parametro == 20 &&
                                                            Convert.ToDecimal(costoPromedioTotal, miCultura) != 0 //costo promedio
                                                        ) //costo promedio
                                                        {
                                                            mov_contable movNuevo = new mov_contable
                                                            {
                                                                id_encab = buscarUltimoEncabezado.idencabezado,
                                                                seq = secuencia,
                                                                idparametronombre = parametro.id_nombre_parametro,
                                                                cuenta = parametro.cuenta,
                                                                centro = parametro.centro,
                                                                fec = DateTime.Now,
                                                                fec_creacion = DateTime.Now,
                                                                //movNuevo.tipo_tarifa = Convert.ToInt32(Request["tipo_tarifa_hidden_" + i]);
                                                                userid_creacion =
                                                                Convert.ToInt32(Session["user_usuarioid"])
                                                            };
                                                            //movNuevo.documento = Convert.ToString(modelo.pedido);

                                                            cuenta_puc info = context.cuenta_puc
                                                                .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                .FirstOrDefault();

                                                            if (info.tercero)
                                                            {
                                                                movNuevo.nit = nitDespacho;
                                                            }
                                                            else
                                                            {
                                                                icb_terceros tercero = context.icb_terceros
                                                                    .Where(t => t.doc_tercero == "0").FirstOrDefault();
                                                                movNuevo.nit = tercero.tercero_id;
                                                            }


                                                            #region Inventario				

                                                            if (parametro.id_nombre_parametro == 9 ||
                                                                parametro.id_nombre_parametro == 20)
                                                            {
                                                                icb_referencia perfilReferencia =
                                                                    context.icb_referencia.FirstOrDefault(x =>
                                                                        x.ref_codigo == referencia_codigo);
                                                                int perfilBuscar =
                                                                    Convert.ToInt32(perfilReferencia.perfil);
                                                                perfilcontable_referencia pcr = context.perfilcontable_referencia
                                                                    .FirstOrDefault(r => r.id == perfilBuscar);

                                                                #region Tiene perfil la referencia

                                                                if (pcr != null)
                                                                {
                                                                    int? cuentaInven = pcr.cta_inventario;

                                                                    movNuevo.id_encab = crearEncabezado.idencabezado;
                                                                    movNuevo.seq = secuencia;
                                                                    movNuevo.idparametronombre =
                                                                        parametro.id_nombre_parametro;

                                                                    #region tiene perfil y cuenta asignada al perfil

                                                                    if (cuentaInven != null)
                                                                    {
                                                                        movNuevo.cuenta = Convert.ToInt32(cuentaInven);
                                                                        movNuevo.centro = parametro.centro;
                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        //movNuevo.documento = Convert.ToString(eg.numero);

                                                                        cuenta_puc infoReferencia = context.cuenta_puc
                                                                            .Where(a => a.cntpuc_id == cuentaInven)
                                                                            .FirstOrDefault();
                                                                        if (infoReferencia.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (infoReferencia.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (infoReferencia.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debito = 0;

                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debito = 0;
                                                                        }

                                                                        //context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion

                                                                    #region tiene perfil pero no tiene cuenta asignada

                                                                    else
                                                                    {
                                                                        movNuevo.cuenta = parametro.cuenta;
                                                                        movNuevo.centro = parametro.centro;
                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        movNuevo.documento =
                                                                            Convert.ToString(eg.numero);

                                                                        cuenta_puc infoReferencia = context.cuenta_puc
                                                                            .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                            .FirstOrDefault();
                                                                        if (infoReferencia.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(valor_totalenca, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (infoReferencia.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (infoReferencia.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debito = 0;

                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debito = 0;
                                                                        }

                                                                        //context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion
                                                                }

                                                                #endregion

                                                                #region La referencia no tiene perfil

                                                                else
                                                                {
                                                                    movNuevo.id_encab = crearEncabezado.idencabezado;
                                                                    movNuevo.seq = secuencia;
                                                                    movNuevo.idparametronombre =
                                                                        parametro.id_nombre_parametro;
                                                                    movNuevo.cuenta = parametro.cuenta;
                                                                    movNuevo.centro = parametro.centro;
                                                                    movNuevo.fec = DateTime.Now;
                                                                    movNuevo.fec_creacion = DateTime.Now;
                                                                    movNuevo.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);
                                                                    /*if (info.aplicaniff==true)
																	{

																	}*/

                                                                    if (info.manejabase)
                                                                    {
                                                                        movNuevo.basecontable =
                                                                            Convert.ToDecimal(valor_totalenca, miCultura);
                                                                    }
                                                                    else
                                                                    {
                                                                        movNuevo.basecontable = 0;
                                                                    }

                                                                    if (info.documeto)
                                                                    {
                                                                        movNuevo.documento =
                                                                            Convert.ToString(eg.numero);
                                                                    }

                                                                    if (buscarCuenta.concepniff == 1)
                                                                    {
                                                                        movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                        movNuevo.debito = 0;

                                                                        movNuevo.creditoniif = Convert.ToDecimal(cr, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (buscarCuenta.concepniff == 4)
                                                                    {
                                                                        movNuevo.creditoniif = Convert.ToDecimal(cr, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (buscarCuenta.concepniff == 5)
                                                                    {
                                                                        movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                        movNuevo.debito = 0;
                                                                    }

                                                                    //context.mov_contable.Add(movNuevo);
                                                                }

                                                                #endregion

                                                                mov_contable buscarInventario =
                                                                    context.mov_contable.FirstOrDefault(x =>
                                                                        x.id_encab == crearEncabezado.idencabezado &&
                                                                        x.cuenta == movNuevo.cuenta &&
                                                                        x.idparametronombre ==
                                                                        parametro.id_nombre_parametro);
                                                                if (buscarInventario != null)
                                                                {
                                                                    buscarInventario.basecontable +=
                                                                        movNuevo.basecontable;
                                                                    buscarInventario.debito += movNuevo.debito;
                                                                    buscarInventario.debitoniif += movNuevo.debitoniif;
                                                                    buscarInventario.credito += movNuevo.credito;
                                                                    buscarInventario.creditoniif +=
                                                                        movNuevo.creditoniif;
                                                                    context.Entry(buscarInventario).State =
                                                                        EntityState.Modified;
                                                                }
                                                                else
                                                                {
                                                                    mov_contable crearMovContable = new mov_contable
                                                                    {
                                                                        id_encab =
                                                                        crearEncabezado.idencabezado,
                                                                        seq = secuencia,
                                                                        idparametronombre =
                                                                        parametro.id_nombre_parametro,
                                                                        cuenta =
                                                                        Convert.ToInt32(movNuevo.cuenta),
                                                                        centro = parametro.centro,
                                                                        nit = crearEncabezado.nit,
                                                                        fec = DateTime.Now,
                                                                        debito = movNuevo.debito,
                                                                        debitoniif = movNuevo.debitoniif,
                                                                        basecontable =
                                                                        movNuevo.basecontable,
                                                                        credito = movNuevo.credito,
                                                                        creditoniif = movNuevo.creditoniif,
                                                                        fec_creacion = DateTime.Now,
                                                                        userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]),
                                                                        detalle =
                                                                        "Facturacion de repuestos con consecutivo " +
                                                                        eg.numero,
                                                                        estado = true
                                                                    };
                                                                    context.mov_contable.Add(crearMovContable);
                                                                    context.SaveChanges();
                                                                }
                                                            }

                                                            #endregion

                                                            #region Ingreso				

                                                            bool siva = Request["tipo_tarifa_hidden_" + i] == "2";

                                                            if (parametro.id_nombre_parametro == 11 && siva != true)
                                                            {
                                                                icb_referencia perfilReferencia =
                                                                    context.icb_referencia.FirstOrDefault(x =>
                                                                        x.ref_codigo == referencia_codigo);
                                                                int perfilBuscar =
                                                                    Convert.ToInt32(perfilReferencia.perfil);
                                                                perfilcontable_referencia pcr = context.perfilcontable_referencia
                                                                    .FirstOrDefault(r => r.id == perfilBuscar);

                                                                #region Tiene perfil la referencia

                                                                if (pcr != null)
                                                                {
                                                                    int? cuentaVenta = pcr.cuenta_ventas;

                                                                    movNuevo.id_encab = crearEncabezado.idencabezado;
                                                                    movNuevo.seq = secuencia;
                                                                    movNuevo.idparametronombre =
                                                                        parametro.id_nombre_parametro;

                                                                    #region tiene perfil y cuenta asignada al perfil

                                                                    if (cuentaVenta != null)
                                                                    {
                                                                        movNuevo.cuenta = Convert.ToInt32(cuentaVenta);
                                                                        movNuevo.centro =
                                                                            Request["tipo_tarifa_hidden_" + i] == "2"
                                                                                ? parametro.id_nombre_parametro == 11
                                                                                    ?
                                                                                    Convert.ToInt32(
                                                                                        Request["centro_costo_tf" + i])
                                                                                    : parametro.centro
                                                                                : parametro.centro;
                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        movNuevo.documento =
                                                                            Convert.ToString(eg.numero);

                                                                        cuenta_puc infoReferencia = context.cuenta_puc
                                                                            .Where(a => a.cntpuc_id == cuentaVenta)
                                                                            .FirstOrDefault();
                                                                        if (infoReferencia.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (infoReferencia.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (infoReferencia.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debito = 0;

                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debito = 0;
                                                                        }

                                                                        //context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion

                                                                    #region tiene perfil pero no tiene cuenta asignada

                                                                    else
                                                                    {
                                                                        movNuevo.cuenta = parametro.cuenta;
                                                                        movNuevo.centro =
                                                                            Request["tipo_tarifa_hidden_" + i] == "2"
                                                                                ? parametro.id_nombre_parametro == 11
                                                                                    ?
                                                                                    Convert.ToInt32(
                                                                                        Request["centro_costo_tf" + i])
                                                                                    : parametro.centro
                                                                                : parametro.centro;
                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        movNuevo.documento =
                                                                            Convert.ToString(eg.numero);

                                                                        cuenta_puc infoReferencia = context.cuenta_puc
                                                                            .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                            .FirstOrDefault();
                                                                        if (infoReferencia.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(valor_totalenca, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (infoReferencia.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (infoReferencia.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debito = 0;

                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debito = 0;
                                                                        }

                                                                        //context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion
                                                                }

                                                                #endregion

                                                                #region La referencia no tiene perfil

                                                                else
                                                                {
                                                                    movNuevo.id_encab = crearEncabezado.idencabezado;
                                                                    movNuevo.seq = secuencia;
                                                                    movNuevo.idparametronombre =
                                                                        parametro.id_nombre_parametro;
                                                                    movNuevo.cuenta = parametro.cuenta;
                                                                    movNuevo.centro =
                                                                        Request["tipo_tarifa_hidden_" + i] == "2"
                                                                            ? parametro.id_nombre_parametro == 11
                                                                                ?
                                                                                Convert.ToInt32(
                                                                                    Request["centro_costo_tf" + i])
                                                                                : parametro.centro
                                                                            : parametro.centro;
                                                                    ;
                                                                    movNuevo.fec = DateTime.Now;
                                                                    movNuevo.fec_creacion = DateTime.Now;
                                                                    movNuevo.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);
                                                                    /*if (info.aplicaniff==true)
																	{

																	}*/

                                                                    if (info.manejabase)
                                                                    {
                                                                        movNuevo.basecontable =
                                                                            Convert.ToDecimal(valor_totalenca, miCultura);
                                                                    }
                                                                    else
                                                                    {
                                                                        movNuevo.basecontable = 0;
                                                                    }

                                                                    if (info.documeto)
                                                                    {
                                                                        movNuevo.documento =
                                                                            Convert.ToString(eg.numero);
                                                                    }

                                                                    if (buscarCuenta.concepniff == 1)
                                                                    {
                                                                        movNuevo.credito =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                        movNuevo.debito = 0;

                                                                        movNuevo.creditoniif =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (buscarCuenta.concepniff == 4)
                                                                    {
                                                                        movNuevo.creditoniif =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (buscarCuenta.concepniff == 5)
                                                                    {
                                                                        movNuevo.credito =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                        movNuevo.debito = 0;
                                                                    }

                                                                    //context.mov_contable.Add(movNuevo);
                                                                }

                                                                #endregion

                                                                mov_contable buscarVenta =
                                                                    context.mov_contable.FirstOrDefault(x =>
                                                                        x.id_encab == crearEncabezado.idencabezado &&
                                                                        x.cuenta == movNuevo.cuenta &&
                                                                        x.idparametronombre ==
                                                                        parametro.id_nombre_parametro);
                                                                if (buscarVenta != null)
                                                                {
                                                                    buscarVenta.basecontable += movNuevo.basecontable;
                                                                    buscarVenta.debito += movNuevo.debito;
                                                                    buscarVenta.debitoniif += movNuevo.debitoniif;
                                                                    buscarVenta.credito += movNuevo.credito;
                                                                    buscarVenta.creditoniif += movNuevo.creditoniif;
                                                                    context.Entry(buscarVenta).State =
                                                                        EntityState.Modified;
                                                                }
                                                                else
                                                                {
                                                                    mov_contable crearMovContable = new mov_contable
                                                                    {
                                                                        id_encab =
                                                                        crearEncabezado.idencabezado,
                                                                        seq = secuencia,
                                                                        idparametronombre =
                                                                        parametro.id_nombre_parametro,
                                                                        cuenta =
                                                                        Convert.ToInt32(movNuevo.cuenta),
                                                                        centro =
                                                                        Request["tipo_tarifa_hidden_" + i] == "2"
                                                                            ? parametro.id_nombre_parametro == 11
                                                                                ?
                                                                                Convert.ToInt32(
                                                                                    Request["centro_costo_tf" + i])
                                                                                : parametro.centro
                                                                            : parametro.centro,
                                                                        nit = crearEncabezado.nit,
                                                                        fec = DateTime.Now,
                                                                        debito = movNuevo.debito,
                                                                        debitoniif = movNuevo.debitoniif,
                                                                        basecontable =
                                                                        movNuevo.basecontable,
                                                                        credito = movNuevo.credito,
                                                                        creditoniif = movNuevo.creditoniif,
                                                                        fec_creacion = DateTime.Now,
                                                                        userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]),
                                                                        detalle =
                                                                        "Facturacion de repuestos con consecutivo " +
                                                                        eg.numero,
                                                                        estado = true
                                                                    };
                                                                    context.mov_contable.Add(crearMovContable);
                                                                    context.SaveChanges();
                                                                }
                                                            }

                                                            #endregion

                                                            #region Costo				

                                                            if (parametro.id_nombre_parametro == 12)
                                                            {
                                                                icb_referencia perfilReferencia =
                                                                    context.icb_referencia.FirstOrDefault(x =>
                                                                        x.ref_codigo == referencia_codigo);
                                                                int perfilBuscar =
                                                                    Convert.ToInt32(perfilReferencia.perfil);
                                                                perfilcontable_referencia pcr = context.perfilcontable_referencia
                                                                    .FirstOrDefault(r => r.id == perfilBuscar);

                                                                #region Tiene perfil la referencia

                                                                if (pcr != null)
                                                                {
                                                                    int? cuentaCosto = pcr.cuenta_costo;

                                                                    movNuevo.id_encab = crearEncabezado.idencabezado;
                                                                    movNuevo.seq = secuencia;
                                                                    movNuevo.idparametronombre =
                                                                        parametro.id_nombre_parametro;

                                                                    #region tiene perfil y cuenta asignada al perfil

                                                                    if (cuentaCosto != null)
                                                                    {
                                                                        movNuevo.cuenta = Convert.ToInt32(cuentaCosto);
                                                                        movNuevo.centro =
                                                                            Request["tipo_tarifa_hidden_" + i] == "2"
                                                                                ? parametro.id_nombre_parametro == 12
                                                                                    ?
                                                                                    Convert.ToInt32(
                                                                                        Request["centro_costo_tf" + i])
                                                                                    : parametro.centro
                                                                                : parametro.centro;
                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        movNuevo.documento =
                                                                            Convert.ToString(eg.numero);

                                                                        cuenta_puc infoReferencia = context.cuenta_puc
                                                                            .Where(a => a.cntpuc_id == cuentaCosto)
                                                                            .FirstOrDefault();
                                                                        if (infoReferencia.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(valor_totalenca, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (infoReferencia.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (infoReferencia.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito = 0;
                                                                            movNuevo.debito = Convert.ToDecimal(cr, miCultura);

                                                                            movNuevo.creditoniif = 0;
                                                                            movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                        }

                                                                        if (infoReferencia.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif = 0;
                                                                            movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                        }

                                                                        if (infoReferencia.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito = 0;
                                                                            movNuevo.debito = Convert.ToDecimal(cr, miCultura);
                                                                        }

                                                                        //context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion

                                                                    #region tiene perfil pero no tiene cuenta asignada

                                                                    else
                                                                    {
                                                                        movNuevo.cuenta = parametro.cuenta;
                                                                        movNuevo.centro =
                                                                            Request["tipo_tarifa_hidden_" + i] == "2"
                                                                                ? parametro.id_nombre_parametro == 12
                                                                                    ?
                                                                                    Convert.ToInt32(
                                                                                        Request["centro_costo_tf" + i])
                                                                                    : parametro.centro
                                                                                : parametro.centro;
                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        movNuevo.documento =
                                                                            Convert.ToString(eg.numero);

                                                                        cuenta_puc infoReferencia = context.cuenta_puc
                                                                            .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                            .FirstOrDefault();
                                                                        if (infoReferencia.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(valor_totalenca, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (infoReferencia.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (infoReferencia.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito = 0;
                                                                            movNuevo.debito = Convert.ToDecimal(cr, miCultura);

                                                                            movNuevo.creditoniif = 0;
                                                                            movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                        }

                                                                        if (infoReferencia.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif = 0;
                                                                            movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                        }

                                                                        if (infoReferencia.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito = 0;
                                                                            movNuevo.debito = Convert.ToDecimal(cr, miCultura);
                                                                        }

                                                                        //context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion
                                                                }

                                                                #endregion

                                                                #region La referencia no tiene perfil

                                                                else
                                                                {
                                                                    movNuevo.id_encab = crearEncabezado.idencabezado;
                                                                    movNuevo.seq = secuencia;
                                                                    movNuevo.idparametronombre =
                                                                        parametro.id_nombre_parametro;
                                                                    movNuevo.cuenta = parametro.cuenta;
                                                                    movNuevo.centro =
                                                                        Request["tipo_tarifa_hidden_" + i] == "2"
                                                                            ? parametro.id_nombre_parametro == 12
                                                                                ?
                                                                                Convert.ToInt32(
                                                                                    Request["centro_costo_tf" + i])
                                                                                : parametro.centro
                                                                            : parametro.centro;
                                                                    ;
                                                                    movNuevo.fec = DateTime.Now;
                                                                    movNuevo.fec_creacion = DateTime.Now;
                                                                    movNuevo.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);
                                                                    /*if (info.aplicaniff==true)
																	{

																	}*/

                                                                    if (info.manejabase)
                                                                    {
                                                                        movNuevo.basecontable =
                                                                            Convert.ToDecimal(valor_totalenca, miCultura);
                                                                    }
                                                                    else
                                                                    {
                                                                        movNuevo.basecontable = 0;
                                                                    }

                                                                    if (info.documeto)
                                                                    {
                                                                        movNuevo.documento =
                                                                            Convert.ToString(eg.numero);
                                                                    }

                                                                    if (buscarCuenta.concepniff == 1)
                                                                    {
                                                                        movNuevo.credito = 0;
                                                                        movNuevo.debito = Convert.ToDecimal(cr, miCultura);

                                                                        movNuevo.creditoniif = 0;
                                                                        movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                    }

                                                                    if (buscarCuenta.concepniff == 4)
                                                                    {
                                                                        movNuevo.creditoniif = 0;
                                                                        movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                    }

                                                                    if (buscarCuenta.concepniff == 5)
                                                                    {
                                                                        movNuevo.credito = 0;
                                                                        movNuevo.debito = Convert.ToDecimal(cr, miCultura);
                                                                    }

                                                                    //context.mov_contable.Add(movNuevo);
                                                                }

                                                                #endregion

                                                                mov_contable buscarCosto =
                                                                    context.mov_contable.FirstOrDefault(x =>
                                                                        x.id_encab == crearEncabezado.idencabezado &&
                                                                        x.cuenta == movNuevo.cuenta &&
                                                                        x.idparametronombre ==
                                                                        parametro.id_nombre_parametro);
                                                                if (buscarCosto != null)
                                                                {
                                                                    buscarCosto.basecontable += movNuevo.basecontable;
                                                                    buscarCosto.debito += movNuevo.debito;
                                                                    buscarCosto.debitoniif += movNuevo.debitoniif;
                                                                    buscarCosto.credito += movNuevo.credito;
                                                                    buscarCosto.creditoniif += movNuevo.creditoniif;
                                                                    context.Entry(buscarCosto).State =
                                                                        EntityState.Modified;
                                                                }
                                                                else
                                                                {
                                                                    mov_contable crearMovContable = new mov_contable
                                                                    {
                                                                        id_encab =
                                                                        crearEncabezado.idencabezado,
                                                                        seq = secuencia,
                                                                        idparametronombre =
                                                                        parametro.id_nombre_parametro,
                                                                        cuenta =
                                                                        Convert.ToInt32(movNuevo.cuenta),
                                                                        centro =
                                                                        Request["tipo_tarifa_hidden_" + i] == "2"
                                                                            ? parametro.id_nombre_parametro == 12
                                                                                ?
                                                                                Convert.ToInt32(
                                                                                    Request["centro_costo_tf" + i])
                                                                                : parametro.centro
                                                                            : parametro.centro
                                                                    };
                                                                    ;
                                                                    crearMovContable.nit = crearEncabezado.nit;
                                                                    crearMovContable.fec = DateTime.Now;
                                                                    crearMovContable.debito = movNuevo.debito;
                                                                    crearMovContable.debitoniif = movNuevo.debitoniif;
                                                                    crearMovContable.basecontable =
                                                                        movNuevo.basecontable;
                                                                    crearMovContable.credito = movNuevo.credito;
                                                                    crearMovContable.creditoniif = movNuevo.creditoniif;
                                                                    crearMovContable.fec_creacion = DateTime.Now;
                                                                    crearMovContable.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);
                                                                    crearMovContable.detalle =
                                                                        "Facturacion de repuestos con consecutivo " +
                                                                        eg.numero;
                                                                    crearMovContable.estado = true;
                                                                    context.mov_contable.Add(crearMovContable);
                                                                    context.SaveChanges();
                                                                }
                                                            }

                                                            #endregion

                                                            secuencia++;
                                                            //Cuentas valores

                                                            #region Cuentas valores

                                                            cuentas_valores buscar_cuentas_valores =
                                                                context.cuentas_valores.FirstOrDefault(x =>
                                                                    x.centro == parametro.centro &&
                                                                    x.cuenta == movNuevo.cuenta &&
                                                                    x.nit == movNuevo.nit);
                                                            if (buscar_cuentas_valores != null)
                                                            {
                                                                buscar_cuentas_valores.debito +=
                                                                    Math.Round(movNuevo.debito);
                                                                buscar_cuentas_valores.credito +=
                                                                    Math.Round(movNuevo.credito);
                                                                buscar_cuentas_valores.debitoniff +=
                                                                    Math.Round(movNuevo.debitoniif);
                                                                buscar_cuentas_valores.creditoniff +=
                                                                    Math.Round(movNuevo.creditoniif);
                                                                context.Entry(buscar_cuentas_valores).State =
                                                                    EntityState.Modified;
                                                                //context.SaveChanges();
                                                            }
                                                            else
                                                            {
                                                                DateTime fechaHoy = DateTime.Now;
                                                                cuentas_valores crearCuentaValor = new cuentas_valores
                                                                {
                                                                    ano = fechaHoy.Year,
                                                                    mes = fechaHoy.Month,
                                                                    cuenta = movNuevo.cuenta,
                                                                    centro =
                                                                    Request["tipo_tarifa_hidden_" + i] == "2"
                                                                        ? parametro.id_nombre_parametro == 11
                                                                            ?
                                                                            Convert.ToInt32(
                                                                                Request["centro_costo_tf" + i])
                                                                            : parametro.id_nombre_parametro == 12
                                                                                ? Convert.ToInt32(
                                                                                    Request["centro_costo_tf" + i])
                                                                                : parametro.centro
                                                                        : parametro.centro,
                                                                    nit = movNuevo.nit,
                                                                    debito = Math.Round(movNuevo.debito),
                                                                    credito = Math.Round(movNuevo.credito),
                                                                    debitoniff =
                                                                    Math.Round(movNuevo.debitoniif),
                                                                    creditoniff =
                                                                    Math.Round(movNuevo.creditoniif)
                                                                };
                                                                context.cuentas_valores.Add(crearCuentaValor);
                                                                context.SaveChanges();
                                                            }

                                                            #endregion

                                                            //totalCreditos += Math.Round(movNuevo.credito);
                                                            //totalDebitos += Math.Round(movNuevo.debito);

                                                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                            {
                                                                NumeroCuenta =
                                                                    "(" + buscarCuenta.cntpuc_numero + ")" +
                                                                    buscarCuenta.cntpuc_descp,
                                                                DescripcionParametro = descripcionParametro,
                                                                ValorDebito = movNuevo.debito,
                                                                ValorCredito = movNuevo.credito
                                                            });
                                                        }
                                                    }
                                                }
                                            }

                                            #endregion


                                            //FIN DEL MOVIMiENTo CONTABLE
                                            sequ++;
                                            //aqui marco los repuestos como recibidos
                                            recibidotraslados recibidos = context.recibidotraslados.Where(d =>
                                                    d.refcodigo == referencia_codigo &&
                                                    d.idtraslado == pedido.idencabezado)
                                                .FirstOrDefault();
                                            recibidos.recibido = true;
                                            recibidos.cant_recibida = Convert.ToInt32(referencia_cantidad);
                                            recibidos.fecharecibido = DateTime.Now;
                                            recibidos.userrecibido = Convert.ToInt32(Session["user_usuarioid"]);

                                            context.Entry(recibidos).State = EntityState.Modified;
                                            int guardar3 = context.SaveChanges();
                                        }
                                    }
                                }

                                //cruzo el recibo de traslado con la emisión del mismo
                                //hago el cruze de documento de ese traslado con ese pedido 
                                cruce_documentos cruce = new cruce_documentos
                                {
                                    id_encabezado = crearEncabezado.idencabezado,
                                    id_encab_aplica = pedido.idencabezado,
                                    idtipo = crearEncabezado.tipo,
                                    numero = crearEncabezado.numero,
                                    idtipoaplica = pedido.tipo,
                                    numeroaplica = pedido.numero,
                                    fecha = DateTime.Now,
                                    fechacruce = DateTime.Now,
                                    userid_creacion = crearEncabezado.userid_creacion
                                };
                                context.cruce_documentos.Add(cruce);
                                int guardarcruce = context.SaveChanges();
                                //hago documento de entrada de repuestos a este inventario

                                if (numeroConsecutivoAuxEntrada != null)
                                {
                                    numeroConsecutivoEntrada = numeroConsecutivoAuxEntrada.doccons_siguiente;
                                    ////////Entro a inventario
                                    encab_documento crearEncabezadoEntrada = new encab_documento
                                    {
                                        notas = modelo.Notas,
                                        tipo = tipotraslado,
                                        bodega = modelo.BodegaDestino,
                                        numero = numeroConsecutivoEntrada,
                                        ///////crearEncabezado.documento = modelo.Referencia;
                                        fecha = DateTime.Now,
                                        fec_creacion = DateTime.Now,
                                        impoconsumo = 0,
                                        destinatario = pedido.userid_creacion,
                                        bodega_destino = modelo.BodegaDestino,
                                        //crearEncabezado.perfilcontable = perfilContableTraslado;
                                        nit = pedido.nit,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                        orden_taller = pedido.orden_taller
                                    };
                                    context.encab_documento.Add(crearEncabezadoEntrada);

                                    bool guardarEntrada = context.SaveChanges() > 0;

                                    List<lineas_documento> lineasdocuadespachar = context.lineas_documento
                                        .Where(d => d.id_encabezado == crearEncabezado.idencabezado).ToList();
                                    foreach (lineas_documento item in lineasdocuadespachar)
                                    {
                                        lineas_documento crearLineasEntrada = new lineas_documento
                                        {
                                            codigo = item.codigo,
                                            fec_creacion = DateTime.Now,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            nit = pedido.nit,
                                            cantidad = item.cantidad,
                                            bodega = modelo.BodegaDestino,
                                            seq = sequ,
                                            estado = true,
                                            fec = DateTime.Now,
                                            costo_unitario = item.costo_unitario,
                                            id_encabezado = crearEncabezadoEntrada != null
                                                ? crearEncabezadoEntrada.idencabezado
                                                : 0
                                        };
                                        if (item.id_solicitud_repuesto != null)
                                        {
                                            crearLineasEntrada.id_solicitud_repuesto = item.id_solicitud_repuesto;
                                        }

                                        context.lineas_documento.Add(crearLineasEntrada);
                                        int guardar1 = context.SaveChanges();
                                    }

                                    guardadoentrada = 1;
                                }
                                else
                                {
                                    procesar =
                                        "No existe un numero consecutivo asignado para este tipo de documento (Documento de entrada a inventario)";
                                    TempData["mensaje_error"] =
                                        "No existe un numero consecutivo asignado para este tipo de documento";
                                }

                                #endregion

                                //ahora hago el documento de salida de repuestos del stock en la BODEGA DESTINO
                                // Me traigo el id del tipo de documento de despacho de repuestos a taller
                                //consecutivo de despacho de repuestos

                                #region despacho de los repuestos

                                icb_sysparameter param12 = context.icb_sysparameter.Where(d => d.syspar_cod == "P100")
                                    .FirstOrDefault();
                                int TipoDocumentodespacho =
                                    param12 != null ? Convert.ToInt32(param12.syspar_value) : 19;
                                grupoconsecutivos buscarGrupoConsecutivosSalida = context.grupoconsecutivos.FirstOrDefault(x =>
                                    x.documento_id == TipoDocumentodespacho && x.bodega_id == modelo.BodegaDestino);
                                int numeroGrupoSalida =
                                    buscarGrupoConsecutivos != null ? buscarGrupoConsecutivos.grupo : 0;

                                ConsecutivosGestion gestionConsecutivoSalida = new ConsecutivosGestion();
                                icb_doc_consecutivos numeroConsecutivoAuxSalida = new icb_doc_consecutivos();
                                tp_doc_registros buscarTipoDocRegistroSalida =
                                    context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == TipoDocumentodespacho);
                                numeroConsecutivoAuxSalida =
                                    gestionConsecutivoSalida.BuscarConsecutivo(buscarTipoDocRegistroSalida,
                                        TipoDocumentodespacho, modelo.BodegaDestino);
                                if (numeroConsecutivoAuxSalida != null)
                                {
                                    numeroConsecutivo = numeroConsecutivoAuxSalida.doccons_siguiente;

                                    encab_documento crearEncabezadoDespacho = new encab_documento
                                    {
                                        notas = modelo.Notas,
                                        tipo = TipoDocumentodespacho,
                                        bodega = modelo.BodegaDestino,
                                        numero = numeroConsecutivo,
                                        ///////crearEncabezado.documento = modelo.Referencia;
                                        fecha = DateTime.Now,
                                        fec_creacion = DateTime.Now,
                                        impoconsumo = 0,
                                        destinatario = pedido.userid_creacion,
                                        bodega_destino = modelo.BodegaDestino,
                                        nit = nitDespacho,
                                        userid_creacion =
                                        Convert.ToInt32(Session["user_usuarioid"]),
                                        orden_taller = pedido.orden_taller
                                    };
                                    context.encab_documento.Add(crearEncabezadoDespacho);

                                    bool guardarsalida = context.SaveChanges() > 0;
                                    encab_documento buscarUltimoEncabezadosalida = context.encab_documento
                                        .Where(x => x.idencabezado == crearEncabezadoDespacho.idencabezado)
                                        .FirstOrDefault();
                                    //busco las lineas del documento que acaba de entrar y los uso aqui mismo
                                    List<lineas_documento> lineasdocuadespachar = context.lineas_documento
                                        .Where(d => d.id_encabezado == crearEncabezado.idencabezado).ToList();
                                    foreach (lineas_documento item in lineasdocuadespachar)
                                    {
                                        lineas_documento crearLineasOrigen = new lineas_documento
                                        {
                                            codigo = item.codigo,
                                            fec_creacion = DateTime.Now,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            nit = nitDespacho,
                                            cantidad = item.cantidad,
                                            bodega = modelo.BodegaDestino,
                                            seq = sequ,
                                            estado = true,
                                            fec = DateTime.Now,
                                            costo_unitario = item.costo_unitario,
                                            id_encabezado = buscarUltimoEncabezadosalida != null
                                                ? buscarUltimoEncabezadosalida.idencabezado
                                                : 0
                                        };
                                        if (item.id_solicitud_repuesto != null)
                                        {
                                            crearLineasOrigen.id_solicitud_repuesto = item.id_solicitud_repuesto;
                                        }

                                        context.lineas_documento.Add(crearLineasOrigen);
                                        int guardar1 = context.SaveChanges();
                                    }

                                    icb_sysparameter tipodocsal = context.icb_sysparameter.Where(d => d.syspar_cod == "P42")
                                        .FirstOrDefault();
                                    int TipoDocumentoSalida =
                                        tipodocsal != null ? Convert.ToInt32(tipodocsal.syspar_value) : 21;
                                    //busco el perfil contable de salida
                                    perfil_contable_documento perfilcs = context.perfil_contable_documento
                                        .Where(d => d.tipo == TipoDocumentoSalida).FirstOrDefault();
                                    //busco el consecutivo de salida de repuestos
                                    grupoconsecutivos buscarGrupoConsecutivosSalidaInven =
                                        context.grupoconsecutivos.FirstOrDefault(x =>
                                            x.documento_id == TipoDocumentoSalida &&
                                            x.bodega_id == modelo.BodegaDestino);
                                    int numeroGrupoSalidaInven = buscarGrupoConsecutivosSalida != null
                                        ? buscarGrupoConsecutivosSalida.grupo
                                        : 0;

                                    long numeroConsecutivoSalidaInven = 1;
                                    ConsecutivosGestion gestionConsecutivoSalidaInven = new ConsecutivosGestion();
                                    icb_doc_consecutivos numeroConsecutivoAuxSalidaInven = new icb_doc_consecutivos();
                                    tp_doc_registros buscarTipoDocRegistroSalidaInven =
                                        context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == TipoDocumentoSalida);
                                    numeroConsecutivoAuxSalidaInven =
                                        gestionConsecutivoSalidaInven.BuscarConsecutivo(
                                            buscarTipoDocRegistroSalidaInven, TipoDocumentoSalida,
                                            modelo.BodegaDestino);
                                    if (numeroConsecutivoAuxSalidaInven != null)
                                    {
                                        numeroConsecutivoSalidaInven =
                                            numeroConsecutivoAuxSalidaInven.doccons_siguiente;
                                    }
                                    //registro la salida de inventario de dichos repuestos
                                    encab_documento crearEncabezadoSalida = new encab_documento
                                    {
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                        //
                                        tipo = TipoDocumentoSalida,
                                        bodega = modelo.BodegaOrigen,
                                        numero = numeroConsecutivoSalidaInven,
                                        nit = nitDespacho,
                                        fecha = DateTime.Now,
                                        fec_creacion = DateTime.Now,
                                        impoconsumo = 0,
                                        perfilcontable = perfilcs.id,
                                        orden_taller = pedido.orden_taller
                                    };

                                    context.encab_documento.Add(crearEncabezadoSalida);
                                    int guardarsalidainven = context.SaveChanges();

                                    List<lineas_documento> listalineastraslado = context.lineas_documento.Where(d =>
                                        d.id_encabezado == crearEncabezadoDespacho.idencabezado &&
                                        d.bodega == modelo.BodegaDestino).ToList();
                                    foreach (lineas_documento item in listalineastraslado)
                                    {
                                        icb_referencia referencia = context.icb_referencia.Where(d => d.ref_codigo == item.codigo)
                                            .FirstOrDefault();
                                        lineas_documento crearLineasSalida = new lineas_documento
                                        {
                                            codigo = item.codigo,
                                            fec_creacion = DateTime.Now,
                                            userid_creacion = item.userid_creacion,
                                            nit = item.nit,
                                            cantidad = item.cantidad,
                                            bodega = modelo.BodegaDestino,
                                            seq = sequ,
                                            estado = true,
                                            fec = DateTime.Now,
                                            costo_unitario = item.costo_unitario,
                                            id_encabezado = crearEncabezadoSalida.idencabezado
                                        };
                                        if (item.id_solicitud_repuesto != null)
                                        {
                                            crearLineasSalida.id_solicitud_repuesto = item.id_solicitud_repuesto;
                                        }

                                        context.lineas_documento.Add(crearLineasSalida);
                                        //creo el registro de que esos repuestos fueron despachados a esa OT
                                        ttrasladosorden traslados = new ttrasladosorden
                                        {
                                            idorden = pedido.orden_taller.Value,
                                            idtraslado = crearEncabezado.idencabezado,
                                            idtercero = pedido.tencabezaorden.tercero,
                                            codigo = item.codigo,
                                            cantidad = Convert.ToInt32(item.cantidad),
                                            preciounitario = traercosto(referencia.ref_codigo),
                                            poriva = Convert.ToDecimal(referencia.por_iva, miCultura),
                                            pordescuento = Convert.ToDecimal(referencia.por_dscto, miCultura),
                                            costopromedio = buscarvalor(item.codigo),
                                            fec_creacion = DateTime.Now,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            estado = true
                                        };
                                        if (item.id_solicitud_repuesto != null)
                                        {
                                            //busco la solicitud de repuesto
                                            tsolicitudrepuestosot solic = context.tsolicitudrepuestosot.Where(d =>
                                                d.idorden == pedido.orden_taller.Value &&
                                                d.id == item.id_solicitud_repuesto).FirstOrDefault();
                                            if (solic != null)
                                            {
                                                solic.recibido = true;
                                                solic.canttraslado = Convert.ToInt32(item.cantidad);
                                                if (solic.idrepuesto != item.codigo)
                                                {
                                                    solic.reemplazo = item.codigo;
                                                }

                                                context.Entry(solic).State = EntityState.Modified;
                                            }
                                        }

                                        context.ttrasladosorden.Add(traslados);
                                    }

                                    if (buscarGrupoConsecutivosSalidaInven != null)
                                    {
                                        List<icb_doc_consecutivos> numerosConsecutivos2 = context.icb_doc_consecutivos
                                            .Where(x => x.doccons_grupoconsecutivo == numeroGrupoSalidaInven).ToList();
                                        foreach (icb_doc_consecutivos item in numerosConsecutivos2)
                                        {
                                            item.doccons_siguiente = item.doccons_siguiente + 1;
                                            context.Entry(item).State = EntityState.Modified;
                                        }

                                        //context.SaveChanges();
                                    }

                                    int guardartraslado = context.SaveChanges();
                                    if (guardartraslado > 0)
                                    {
                                        procesar = "ok";
                                    }

                                    #endregion
                                }
                                else
                                {
                                    TempData["mensaje_error"] = "No se ha podido procesar la orden de traslado. ";
                                }

                                //si fue exitoso el proceso
                                if (procesar == "ok")
                                {
                                    if (totalCreditos == totalDebitos)
                                    {
                                        dbTran.Commit();
                                        TempData["mensaje"] = "Se ha procesado la orden de despacho satisfactoriamente";

                                        //cambio los consecutivos
                                        if (buscarGrupoConsecutivos != null)
                                        {
                                            List<icb_doc_consecutivos> numerosConsecutivos = context.icb_doc_consecutivos
                                                .Where(x => x.doccons_grupoconsecutivo == numeroGrupo).ToList();
                                            foreach (icb_doc_consecutivos item in numerosConsecutivos)
                                            {
                                                item.doccons_siguiente = item.doccons_siguiente + 1;
                                                context.Entry(item).State = EntityState.Modified;
                                            }

                                            context.SaveChanges();
                                        }

                                        if (buscarGrupoConsecutivosSalida != null)
                                        {
                                            List<icb_doc_consecutivos> numerosConsecutivos2 = context.icb_doc_consecutivos
                                                .Where(x => x.doccons_grupoconsecutivo == numeroGrupoSalida).ToList();
                                            foreach (icb_doc_consecutivos item in numerosConsecutivos2)
                                            {
                                                item.doccons_siguiente = item.doccons_siguiente + 1;
                                                context.Entry(item).State = EntityState.Modified;
                                            }

                                            context.SaveChanges();
                                        }

                                        if (guardadoentrada > 0 && buscarGrupoConsecutivosEntrada != null)
                                        {
                                            List<icb_doc_consecutivos> numerosConsecutivos3 = context.icb_doc_consecutivos
                                                .Where(x => x.doccons_grupoconsecutivo == numeroGrupoEntrada).ToList();
                                            foreach (icb_doc_consecutivos item in numerosConsecutivos3)
                                            {
                                                item.doccons_siguiente = item.doccons_siguiente + 1;
                                                context.Entry(item).State = EntityState.Modified;
                                            }

                                            context.SaveChanges();
                                        }
                                        //sigo

                                        orden.estadorepuesto = 1;
                                        context.Entry(orden).State = EntityState.Modified;
                                        context.SaveChanges();

                                        TempData["mensaje"] = "Se ha procesado la orden de despacho satisfactoriamente";
                                        return RedirectToAction("browserSolicitudesDespachoOT", new { menu });
                                    }

                                    TempData["mensaje_error"] =
                                        "Error en las cuentas contables. Total Crédito: $ " + totalCreditos +
                                        ", Total Débito: $ " + totalDebitos;
                                    dbTran.Rollback();
                                }
                                else
                                {
                                    TempData["mensaje_error"] = procesar;
                                    dbTran.Rollback();
                                }
                            }
                            catch (DbEntityValidationException)
                            {
                                dbTran.Rollback();
                                TempData["mensaje_error"] =
                                    "Errores en procesamiento del formulario. Por favor llene correctamente todos los campos requeridos";

                                //mensajito de error
                            }
                        }
                    }
                    else
                    {
                        TempData["mensaje_error"] =
                            "Errores en procesamiento del formulario. Por favor llene correctamente todos los campos requeridos";
                    }

                    icb_sysparameter tipot = context.icb_sysparameter.Where(d => d.syspar_cod == "P98").FirstOrDefault();
                    int tipo = tipot != null ? Convert.ToInt32(tipot.syspar_value) : 36;

                    icb_sysparameter tipot2 = context.icb_sysparameter.Where(d => d.syspar_cod == "P97").FirstOrDefault();
                    int tipo2 = tipot2 != null ? Convert.ToInt32(tipot2.syspar_value) : 36;
                    IQueryable<tp_doc_registros> TipoDocumento = context.tp_doc_registros.Where(x => x.tipo == tipo && x.tpdoc_id == tipo2);
                    ViewBag.TipoDocumento = new SelectList(TipoDocumento, "tpdoc_id", "tpdoc_nombre");
                    ViewBag.bodegaOrigenDes = context.bodega_concesionario.Where(d => d.id == existe.bodega)
                        .Select(d => d.bodccs_nombre).FirstOrDefault();
                    ViewBag.cliente = existe.tencabezaorden.icb_terceros.doc_tercero + " - " +
                                      (!string.IsNullOrWhiteSpace(existe.tencabezaorden.icb_terceros.razon_social)
                                          ? existe.tencabezaorden.icb_terceros.razon_social
                                          : existe.tencabezaorden.icb_terceros.prinom_tercero + " " +
                                            existe.tencabezaorden.icb_terceros.segnom_tercero + " " +
                                            existe.tencabezaorden.icb_terceros.apellido_tercero + " " +
                                            existe.tencabezaorden.icb_terceros.segapellido_tercero);
                    if (existe.bodega_destino != null)
                    {
                        ViewBag.bodegaDestinoDes = context.bodega_concesionario
                            .Where(d => d.id == existe.bodega_destino).Select(d => d.bodccs_nombre).FirstOrDefault();
                    }
                    else
                    {
                        ViewBag.bodegaDestinoDes = context.bodega_concesionario.Where(d => d.id == existe.bodega)
                            .Select(d => d.bodccs_nombre).FirstOrDefault();
                    }

                    ViewBag.codigoorden = context.tencabezaorden.Where(d => d.id == existe.orden_taller)
                        .Select(d => d.codigoentrada).FirstOrDefault();

                    return View(modelo);
                }

                TempData["mensaje_error"] = "NO EXISTE LA ORDEN DE DESPACHO";
                return RedirectToAction("browserSolicitudesDespachoOT", "Almacen", new { menu });
            }

            return RedirectToAction("Login", "Login");
        }

        //crea funcion que lee el pedido, y el tipo de pedido
        //las lineas de documento del pedido
        //manda a formulario. Copiar el formulario del formulario de traslado. No lleva tarifa cliente.
        public ActionResult procesarOrdenDespachoViejo(int? id, int? menu)
        {
            if (Session["user_usuarioid"] != null)
            {
                BuscarFavoritos(menu);
                //veo si existe el pedido de repuestos
                encab_documento existe = context.encab_documento.Where(d => d.idencabezado == id && d.orden_taller != null)
                    .FirstOrDefault();
                if (existe != null)
                {
                    //verificar si ya fue procesada la orden(con el tipo de documento que vaya yo a crear a continuacion
                    DespachoOrden modelo = new DespachoOrden
                    {
                        BodegaDestino = existe.bodega_destino != null ? existe.bodega_destino.Value : existe.bodega,
                        BodegaOrigen = existe.bodega,
                        idorden = existe.orden_taller.Value,
                        idpedido = existe.idencabezado,
                        TipoDocumento = existe.tipo
                    };

                    icb_sysparameter tipot = context.icb_sysparameter.Where(d => d.syspar_cod == "P98").FirstOrDefault();
                    int tipo = tipot != null ? Convert.ToInt32(tipot.syspar_value) : 36;

                    icb_sysparameter tipot2 = context.icb_sysparameter.Where(d => d.syspar_cod == "P97").FirstOrDefault();
                    int tipo2 = tipot2 != null ? Convert.ToInt32(tipot2.syspar_value) : 36;
                    IQueryable<tp_doc_registros> TipoDocumento = context.tp_doc_registros.Where(x => x.tipo == tipo && x.tpdoc_id == tipo2);
                    ViewBag.TipoDocumento = new SelectList(TipoDocumento, "tpdoc_id", "tpdoc_nombre");
                    ViewBag.bodegaOrigenDes = context.bodega_concesionario.Where(d => d.id == existe.bodega)
                        .Select(d => d.bodccs_nombre).FirstOrDefault();
                    ViewBag.cliente = existe.tencabezaorden.icb_terceros.doc_tercero + " - " +
                                      (!string.IsNullOrWhiteSpace(existe.tencabezaorden.icb_terceros.razon_social)
                                          ? existe.tencabezaorden.icb_terceros.razon_social
                                          : existe.tencabezaorden.icb_terceros.prinom_tercero + " " +
                                            existe.tencabezaorden.icb_terceros.segnom_tercero + " " +
                                            existe.tencabezaorden.icb_terceros.apellido_tercero + " " +
                                            existe.tencabezaorden.icb_terceros.segapellido_tercero);
                    if (existe.bodega_destino != null)
                    {
                        ViewBag.bodegaDestinoDes = context.bodega_concesionario
                            .Where(d => d.id == existe.bodega_destino).Select(d => d.bodccs_nombre).FirstOrDefault();
                    }
                    else
                    {
                        ViewBag.bodegaDestinoDes = context.bodega_concesionario.Where(d => d.id == existe.bodega)
                            .Select(d => d.bodccs_nombre).FirstOrDefault();
                    }

                    ViewBag.codigoorden = context.tencabezaorden.Where(d => d.id == existe.orden_taller)
                        .Select(d => d.codigoentrada).FirstOrDefault();

                    return View(modelo);
                }

                TempData["mensaje_error"] = "NO EXISTE LA ORDEN DE DESPACHO";
                BuscarFavoritos(menu);
                return RedirectToAction("browserSolicitudesDespachoOT", "Almacen", new { menu });
            }

            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        public ActionResult procesarOrdenDespachoViejo(DespachoOrden modelo, int? menu)
        {
            if (Session["user_usuarioid"] != null)
            {
                //veo si la orden de despacho existe
                BuscarFavoritos(menu);

                encab_documento existe = context.encab_documento
                    .Where(d => d.idencabezado == modelo.idpedido && d.orden_taller != null).FirstOrDefault();
                if (existe != null)
                {
                    if (ModelState.IsValid)
                    {
                        string procesar = "";
                        //busco el tipo de documento traslados taller
                        icb_sysparameter docu = context.icb_sysparameter.Where(d => d.syspar_cod == "P37").FirstOrDefault();
                        int documentoTaller = docu != null ? Convert.ToInt32(docu.syspar_value) : 11;
                        //busco si las bodegas de origen y destino no coinciden para hacer un traslado de repuestos o si coinciden hacer un despacho
                        int numreferencias = Convert.ToInt32(Request["lista_referencias"]);
                        int estado_sol = context.testado_solicitud_repuesto.Where(s => s.codigo == "DT").Select(d => d.id).FirstOrDefault();


                        if (existe.bodega_destino != null && existe.bodega != existe.bodega_destino)
                        {
                            procesar = trasladox(Request, existe.idencabezado, modelo);
                        }
                        else if (existe.bodega_destino == null || existe.bodega == existe.bodega_destino)
                        {
                            procesar = pedidox(Request, existe.idencabezado, modelo);
                        }

                        //si fue exitoso el proceso
                        if (procesar == "ok")
                        {

                            for (int i = 0; i < numreferencias; i++)
                            {
                                string repuestoform = Request["referencia" + i];
                                if (!string.IsNullOrWhiteSpace(repuestoform))
                                {
                                    tsolicitudrepuestosot solicitudRep = context.tsolicitudrepuestosot.Where(x => x.idorden == modelo.idorden && x.idrepuesto == repuestoform).FirstOrDefault();
                                    solicitudRep.estado_solicitud = estado_sol;
                                    context.Entry(solicitudRep).State = EntityState.Modified;
                                    context.SaveChanges();
                                }

                            }


                            TempData["mensaje"] = "Se ha procesado la orden de despacho satisfactoriamente";
                            return RedirectToAction("browserSolicitudesDespachoOT", new { menu });
                        }









                        TempData["mensaje_error"] = procesar;
                    }
                    else
                    {
                        TempData["mensaje_error"] =
                            "Errores en procesamiento del formulario. Por favor llene correctamente todos los campos requeridos";
                    }

                    icb_sysparameter tipot = context.icb_sysparameter.Where(d => d.syspar_cod == "P98").FirstOrDefault();
                    int tipo = tipot != null ? Convert.ToInt32(tipot.syspar_value) : 36;

                    icb_sysparameter tipot2 = context.icb_sysparameter.Where(d => d.syspar_cod == "P97").FirstOrDefault();
                    int tipo2 = tipot2 != null ? Convert.ToInt32(tipot2.syspar_value) : 36;
                    IQueryable<tp_doc_registros> TipoDocumento = context.tp_doc_registros.Where(x => x.tipo == tipo && x.tpdoc_id == tipo2);
                    ViewBag.TipoDocumento = new SelectList(TipoDocumento, "tpdoc_id", "tpdoc_nombre");
                    ViewBag.bodegaOrigenDes = context.bodega_concesionario.Where(d => d.id == existe.bodega)
                        .Select(d => d.bodccs_nombre).FirstOrDefault();
                    ViewBag.cliente = existe.tencabezaorden.icb_terceros.doc_tercero + " - " +
                                      (!string.IsNullOrWhiteSpace(existe.tencabezaorden.icb_terceros.razon_social)
                                          ? existe.tencabezaorden.icb_terceros.razon_social
                                          : existe.tencabezaorden.icb_terceros.prinom_tercero + " " +
                                            existe.tencabezaorden.icb_terceros.segnom_tercero + " " +
                                            existe.tencabezaorden.icb_terceros.apellido_tercero + " " +
                                            existe.tencabezaorden.icb_terceros.segapellido_tercero);
                    ViewBag.codigoorden = context.tencabezaorden.Where(d => d.id == existe.orden_taller)
                        .Select(d => d.codigoentrada).FirstOrDefault();

                    if (existe.bodega_destino != null)
                    {
                        ViewBag.bodegaDestinoDes = context.bodega_concesionario
                            .Where(d => d.id == existe.bodega_destino).Select(d => d.bodccs_nombre).FirstOrDefault();
                    }
                    else
                    {
                        ViewBag.bodegaDestinoDes = context.bodega_concesionario.Where(d => d.id == existe.bodega)
                            .Select(d => d.bodccs_nombre).FirstOrDefault();
                    }

                    return View(modelo);
                }

                TempData["mensaje_error"] = "NO EXISTE LA ORDEN DE DESPACHO";
                return RedirectToAction("browserSolicitudesDespachoOT", "Almacen", new { menu });
            }

            return RedirectToAction("Login", "Login");
        }

        [HttpGet]
        public ActionResult procesarOrdenDespacho(int? id, int? menu)
        {
            if (Session["user_usuarioid"] != null)
            {
                BuscarFavoritos(menu);
                //veo si existe el pedido de repuestos
                encab_documento existe = context.encab_documento.Where(d => d.idencabezado == id && d.orden_taller != null)
                    .FirstOrDefault();
                if (existe != null)
                {
                    //verificar si ya fue procesada la orden(con el tipo de documento que vaya yo a crear a continuacion
                    DespachoOrden modelo = new DespachoOrden
                    {
                        BodegaDestino = existe.bodega_destino != null ? existe.bodega_destino.Value : existe.bodega,
                        BodegaOrigen = existe.bodega,
                        idorden = existe.orden_taller.Value,
                        idpedido = existe.idencabezado,
                        TipoDocumento = existe.tipo
                    };

                    icb_sysparameter tipot = context.icb_sysparameter.Where(d => d.syspar_cod == "P98").FirstOrDefault();
                    int tipo = tipot != null ? Convert.ToInt32(tipot.syspar_value) : 36;

                    icb_sysparameter tipot2 = context.icb_sysparameter.Where(d => d.syspar_cod == "P97").FirstOrDefault();
                    int tipo2 = tipot2 != null ? Convert.ToInt32(tipot2.syspar_value) : 36;


                    IQueryable<tp_doc_registros> TipoDocumento = context.tp_doc_registros.Where(x => x.tipo == tipo && x.tpdoc_id == tipo2);
                    ViewBag.TipoDocumento = new SelectList(TipoDocumento, "tpdoc_id", "tpdoc_nombre");
                    ViewBag.bodegaOrigenDes = context.bodega_concesionario.Where(d => d.id == existe.bodega)
                        .Select(d => d.bodccs_nombre).FirstOrDefault();
                    ViewBag.cliente = existe.tencabezaorden.icb_terceros.doc_tercero + " - " +
                                      (!string.IsNullOrWhiteSpace(existe.tencabezaorden.icb_terceros.razon_social)
                                          ? existe.tencabezaorden.icb_terceros.razon_social
                                          : existe.tencabezaorden.icb_terceros.prinom_tercero + " " +
                                            existe.tencabezaorden.icb_terceros.segnom_tercero + " " +
                                            existe.tencabezaorden.icb_terceros.apellido_tercero + " " +
                                            existe.tencabezaorden.icb_terceros.segapellido_tercero);
                    if (existe.bodega_destino != null)
                    {
                        ViewBag.bodegaDestinoDes = context.bodega_concesionario
                            .Where(d => d.id == existe.bodega_destino).Select(d => d.bodccs_nombre).FirstOrDefault();
                    }
                    else
                    {
                        ViewBag.bodegaDestinoDes = context.bodega_concesionario.Where(d => d.id == existe.bodega)
                            .Select(d => d.bodccs_nombre).FirstOrDefault();
                    }

                    ViewBag.codigoorden = context.tencabezaorden.Where(d => d.id == existe.orden_taller)
                        .Select(d => d.codigoentrada).FirstOrDefault();

                    return View(modelo);
                }

                TempData["mensaje_error"] = "NO EXISTE LA ORDEN DE DESPACHO";
                BuscarFavoritos(menu);
                return RedirectToAction("browserSolicitudesDespachoOT", "Almacen", new { menu });
            }

            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        public ActionResult procesarOrdenDespacho(DespachoOrden modelo, int? menu)
        {
            if (Session["user_usuarioid"] != null)
            {
                //veo si la orden de despacho existe
                BuscarFavoritos(menu);

                encab_documento existe = context.encab_documento.Where(d => d.idencabezado == modelo.idpedido && d.orden_taller != null).FirstOrDefault();
                List<lineas_documento> lineas = context.lineas_documento.Where(d => d.id_encabezado == modelo.idpedido).ToList();

                bool result = false;

                //documento Salida por Traslado
                icb_sysparameter parped2 = context.icb_sysparameter.Where(d => d.syspar_cod == "P159").FirstOrDefault();
                int pedidorep2 = parped2 != null ? Convert.ToInt32(parped2.syspar_value) : 3091;
                //busco el id del tipo de documento
                tp_doc_registros conse2 = context.tp_doc_registros.Where(d => d.tpdoc_id == pedidorep2).FirstOrDefault();

                //busco el consecutivo
                grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == conse2.tpdoc_id && x.bodega_id == existe.bodega);
                //creo el encabezado de la solicitud
                DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                long consecutivo = doc.BuscarConsecutivo(grupo.grupo);

                if (existe != null)
                {
                    if (ModelState.IsValid)
                    {
                        string procesar = "";
                        //busco si las bodegas de origen y destino no coinciden para hacer un traslado de repuestos o si coinciden hacer un despacho
                        int numreferencias = Convert.ToInt32(Request["lista_referencias"]);
                        int estado_sol = context.testado_solicitud_repuesto.Where(s => s.codigo == "DT").Select(d => d.id).FirstOrDefault();


                        if (existe.bodega_destino != null && existe.bodega != existe.bodega_destino)
                        {
                            procesar = trasladox(Request, existe.idencabezado, modelo);
                        }
                        else if (existe.bodega_destino == null || existe.bodega == existe.bodega_destino)
                        {
                            procesar = pedidox(Request, existe.idencabezado, modelo);
                        }

                        //si fue exitoso el proceso
                        if (procesar == "ok")
                        {

                            for (int i = 0; i < numreferencias; i++)
                            {
                                string repuestoform = Request["referencia" + i];
                                //string cantidadform = Request["cantidad" + i];

                                if (!string.IsNullOrWhiteSpace(repuestoform))
                                {
                                    tsolicitudrepuestosot solicitudRep = context.tsolicitudrepuestosot.Where(x => x.idorden == modelo.idorden && x.idrepuesto == repuestoform).FirstOrDefault();
                                    solicitudRep.comprometido = false;
                                    solicitudRep.estado_solicitud = estado_sol;
                                    context.Entry(solicitudRep).State = EntityState.Modified;
                                    context.SaveChanges();

                                    encab_documento crearEncabezado = new encab_documento
                                    {
                                        fecha = DateTime.Now,
                                        fec_creacion = DateTime.Now,
                                        tipo = pedidorep2,
                                        bodega = existe.bodega,
                                        numero = consecutivo,
                                        valor_total = 0,
                                        valor_aplicado = 0,
                                        valor_mercancia = 0,
                                        iva = 0,
                                        notas = "",
                                        vendedor = Convert.ToInt32(Session["user_usuarioid"]),
                                        costo = 0,
                                        bodega_destino = existe.bodega_destino,
                                        nit = existe.nit,
                                        orden_taller = existe.orden_taller,
                                        impoconsumo = 0,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                                    };

                                    context.encab_documento.Add(crearEncabezado);



                                    foreach (var item in lineas)
                                    {

                                        int li = 1;
                                        lineas_documento crearLineasOrigen = new lineas_documento
                                        {
                                            codigo = item.codigo,
                                            fec_creacion = DateTime.Now,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            nit = item.nit,
                                            cantidad = item.cantidad,
                                            bodega = item.bodega,
                                            seq = li,
                                            estado = true,
                                            fec = DateTime.Now,
                                            costo_unitario = traercosto(!string.IsNullOrWhiteSpace(item.codigo_reemplazo) ? item.codigo_reemplazo : item.codigo),
                                            //id_encabezado = item.id_encabezado != null ? item.id_encabezado : 0,
                                            id_encabezado = item.id_encabezado,
                                            codigo_reemplazo = !string.IsNullOrWhiteSpace(item.codigo_reemplazo) ? item.codigo : null,
                                            comprometido = true,
                                        };
                                        li++;

                                        context.lineas_documento.Add(crearLineasOrigen);


                                        referencias_inven existencia = context.referencias_inven.FirstOrDefault(x =>
                                        x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month && x.codigo == item.codigo &&
                                        x.bodega == item.bodega);

                                        if (existencia != null)
                                        {

                                            existencia.can_sal -= Convert.ToDecimal(item.cantidad, miCultura);

                                        }
                                        context.Entry(existencia).State = EntityState.Modified;
                                        result=context.SaveChanges()>0;

                                    }


                                }

                            }


                            TempData["mensaje"] = "Se ha procesado la orden de despacho satisfactoriamente";
                            return RedirectToAction("browserSolicitudesDespachoOT", new { menu });
                        }









                        TempData["mensaje_error"] = procesar;
                    }
                    else
                    {
                        TempData["mensaje_error"] =
                            "Errores en procesamiento del formulario. Por favor llene correctamente todos los campos requeridos";
                    }

                    icb_sysparameter tipot = context.icb_sysparameter.Where(d => d.syspar_cod == "P98").FirstOrDefault();
                    int tipo = tipot != null ? Convert.ToInt32(tipot.syspar_value) : 36;

                    icb_sysparameter tipot2 = context.icb_sysparameter.Where(d => d.syspar_cod == "P97").FirstOrDefault();
                    int tipo2 = tipot2 != null ? Convert.ToInt32(tipot2.syspar_value) : 36;
                    IQueryable<tp_doc_registros> TipoDocumento = context.tp_doc_registros.Where(x => x.tipo == tipo && x.tpdoc_id == tipo2);
                    ViewBag.TipoDocumento = new SelectList(TipoDocumento, "tpdoc_id", "tpdoc_nombre");
                    ViewBag.bodegaOrigenDes = context.bodega_concesionario.Where(d => d.id == existe.bodega)
                        .Select(d => d.bodccs_nombre).FirstOrDefault();
                    ViewBag.cliente = existe.tencabezaorden.icb_terceros.doc_tercero + " - " +
                                      (!string.IsNullOrWhiteSpace(existe.tencabezaorden.icb_terceros.razon_social)
                                          ? existe.tencabezaorden.icb_terceros.razon_social
                                          : existe.tencabezaorden.icb_terceros.prinom_tercero + " " +
                                            existe.tencabezaorden.icb_terceros.segnom_tercero + " " +
                                            existe.tencabezaorden.icb_terceros.apellido_tercero + " " +
                                            existe.tencabezaorden.icb_terceros.segapellido_tercero);
                    ViewBag.codigoorden = context.tencabezaorden.Where(d => d.id == existe.orden_taller)
                        .Select(d => d.codigoentrada).FirstOrDefault();

                    if (existe.bodega_destino != null)
                    {
                        ViewBag.bodegaDestinoDes = context.bodega_concesionario
                            .Where(d => d.id == existe.bodega_destino).Select(d => d.bodccs_nombre).FirstOrDefault();
                    }
                    else
                    {
                        ViewBag.bodegaDestinoDes = context.bodega_concesionario.Where(d => d.id == existe.bodega)
                            .Select(d => d.bodccs_nombre).FirstOrDefault();
                    }

                    return View(modelo);
                }

                TempData["mensaje_error"] = "NO EXISTE LA ORDEN DE DESPACHO";
                return RedirectToAction("browserSolicitudesDespachoOT", "Almacen", new { menu });
            }

            return RedirectToAction("Login", "Login");
        }

        public string pedidox(HttpRequestBase request, int? idpedido, DespachoOrden modelo)
        {
            bool listaref = int.TryParse(Request["lista_referencias"], out int lista);
            if (lista == 0)
            {
                return "Debe seleccionar referencias para poder hacer el despacho";
            }

            //Me traigo el id del tipo de documento de despacho de repuestos a taller
            icb_sysparameter param1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P100").FirstOrDefault();
            int TipoDocumento = param1 != null ? Convert.ToInt32(param1.syspar_value) : 19;
            //busco el pedido
            encab_documento pedido = context.encab_documento.Where(d => d.idencabezado == idpedido).FirstOrDefault();
            long numeroConsecutivo = 0;
            grupoconsecutivos buscarGrupoConsecutivos = context.grupoconsecutivos.FirstOrDefault(x =>
                x.documento_id == TipoDocumento && x.bodega_id == pedido.bodega);
            int numeroGrupo = buscarGrupoConsecutivos != null ? buscarGrupoConsecutivos.grupo : 0;

            ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
            icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
            tp_doc_registros buscarTipoDocRegistro = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == TipoDocumento);
            numeroConsecutivoAux =
                gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro, TipoDocumento, pedido.bodega);
            if (numeroConsecutivoAux != null)
            {
                numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
            }
            else
            {
                return "No existe un numero consecutivo asignado para este tipo de documento";
            }

            //si llego hasta aqui es porque hay un consecutivo
            using (DbContextTransaction dbTran = context.Database.BeginTransaction())
            {
                try
                {
                    List<string> listaReferencias = new List<string>();
                    int numero_repuestos = lista;
                    int sequ = 1;
                    List<string> codigosReferencias = new List<string>();
                    for (int i = 0; i <= numero_repuestos; i++)
                    {
                        string referencia_codigo = Request["referencia" + i];
                        if (!string.IsNullOrEmpty(referencia_codigo))
                        {
                            codigosReferencias.Add(referencia_codigo);
                        }
                    }

                    int buscarCantidadRefEnOrigen = (from referencia in context.referencias_inven
                                                     where referencia.ano == DateTime.Now.Year
                                                           && referencia.mes == DateTime.Now.Month && referencia.bodega == pedido.bodega
                                                           && codigosReferencias.Contains(referencia.codigo)
                                                     select referencia.codigo).Count();

                    if (buscarCantidadRefEnOrigen > 0)
                    {
                        icb_sysparameter buscarNit = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P33");
                        int nitDespacho = buscarNit != null ? Convert.ToInt32(buscarNit.syspar_value) : 0;

                        string referenciasNoProcesadas =
                            ", los siguientes codigos no se despacharon por error en inventario";

                        encab_documento crearEncabezado = new encab_documento
                        {
                            notas = modelo.Notas,
                            tipo = TipoDocumento,
                            bodega = modelo.BodegaOrigen,
                            numero = numeroConsecutivo,
                            ///////crearEncabezado.documento = modelo.Referencia;
                            fecha = DateTime.Now,
                            fec_creacion = DateTime.Now,
                            impoconsumo = 0,
                            destinatario = pedido.userid_creacion,
                            bodega_destino = modelo.BodegaDestino,
                            nit = nitDespacho,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            orden_taller = pedido.orden_taller
                        };
                        context.encab_documento.Add(crearEncabezado);

                        bool guardar = context.SaveChanges() > 0;
                        encab_documento buscarUltimoEncabezado = context.encab_documento
                            .Where(x => x.idencabezado == crearEncabezado.idencabezado).FirstOrDefault();


                        for (int i = 0; i <= numero_repuestos; i++)
                        {
                            if (Request["referencia" + i] != null && Request["cantidadFacturar" + i] != null)
                            {
                                string referencia_codigo = Request["referencia" + i];
                                string referencia_cantidad = Request["cantidadFacturar" + i];
                                string referencia_costo = Request["valorAntesIvaReferencia" + i];
                                string idsolicitudreferencia = Request["idsolicitudreferenciaP" + i];
                                if (string.IsNullOrEmpty(referencia_codigo) ||
                                    string.IsNullOrEmpty(referencia_cantidad))
                                {
                                    // Significa que la agregaron y la eliminaron
                                }
                                else
                                {
                                    listaReferencias.Add(referencia_codigo);
                                    referencia_costo = traercosto(referencia_codigo).ToString();
                                    vw_promedio buscarPromedio = context.vw_promedio.FirstOrDefault(x =>
                                        x.codigo == referencia_codigo && x.ano == DateTime.Now.Year &&
                                        x.mes == DateTime.Now.Month);
                                    decimal promedio = buscarPromedio != null ? buscarPromedio.Promedio ?? 0 : 0;
                                    promedio = traercosto(referencia_codigo);
                                    referencias_inven buscarReferenciasInvenOrigen = context.referencias_inven.FirstOrDefault(x =>
                                        x.codigo == referencia_codigo && x.ano == DateTime.Now.Year &&
                                        x.mes == DateTime.Now.Month && x.bodega == modelo.BodegaOrigen);
                                    if (buscarReferenciasInvenOrigen != null)
                                    {
                                        buscarReferenciasInvenOrigen.can_sal =
                                            buscarReferenciasInvenOrigen.can_sal + Convert.ToInt32(referencia_cantidad);
                                        buscarReferenciasInvenOrigen.cos_sal =
                                            buscarReferenciasInvenOrigen.cos_sal +
                                            promedio * Convert.ToInt32(referencia_cantidad);
                                        //buscarReferenciasInvenOrigen.can_tra = buscarReferenciasInvenOrigen.can_tra + Convert.ToInt32(referencia_cantidad);
                                        //buscarReferenciasInvenOrigen.cos_tra = (buscarReferenciasInvenOrigen.cos_tra) + (promedio * Convert.ToInt32(referencia_cantidad));
                                        context.Entry(buscarReferenciasInvenOrigen).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        // Validado arriba porque debe haber referencias inven en la bodega de origen
                                        referenciasNoProcesadas += ", " + referencia_codigo;
                                    }

                                    lineas_documento crearLineasOrigen = new lineas_documento
                                    {
                                        codigo = referencia_codigo,
                                        fec_creacion = DateTime.Now,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                        nit = nitDespacho,
                                        cantidad = !string.IsNullOrEmpty(referencia_cantidad)
                                            ? Convert.ToDecimal(referencia_cantidad, miCultura)
                                            : 0,
                                        bodega = pedido.bodega,
                                        seq = sequ,
                                        estado = true,
                                        fec = DateTime.Now,
                                        costo_unitario = !string.IsNullOrEmpty(referencia_costo)
                                            ? Convert.ToDecimal(referencia_costo, miCultura)
                                            : 0,
                                        id_encabezado = buscarUltimoEncabezado != null
                                            ? buscarUltimoEncabezado.idencabezado
                                            : 0,
                                        id_solicitud_repuesto = Convert.ToInt32(idsolicitudreferencia)
                                    };
                                    context.lineas_documento.Add(crearLineasOrigen);
                                    int guardar1 = context.SaveChanges();
                                    sequ++;
                                }
                            }
                        }

                        /////////////////////////////SALIDA DE REPUESTOS///////////////////////////////////////////////////////////
                        icb_sysparameter tipodocsal = context.icb_sysparameter.Where(d => d.syspar_cod == "P42").FirstOrDefault();
                        int TipoDocumentoSalida = tipodocsal != null ? Convert.ToInt32(tipodocsal.syspar_value) : 21;
                        //busco el perfil contable de salida
                        perfil_contable_documento perfilcs = context.perfil_contable_documento.Where(d => d.tipo == TipoDocumentoSalida)
                            .FirstOrDefault();
                        //busco el consecutivo de salida de repuestos
                        grupoconsecutivos buscarGrupoConsecutivosSalida = context.grupoconsecutivos.FirstOrDefault(x =>
                            x.documento_id == TipoDocumentoSalida && x.bodega_id == pedido.bodega);
                        int numeroGrupoSalida = buscarGrupoConsecutivosSalida != null
                            ? buscarGrupoConsecutivosSalida.grupo
                            : 0;

                        long numeroConsecutivoSalida = 1;
                        ConsecutivosGestion gestionConsecutivoSalida = new ConsecutivosGestion();
                        icb_doc_consecutivos numeroConsecutivoAuxSalida = new icb_doc_consecutivos();
                        tp_doc_registros buscarTipoDocRegistroSalida =
                            context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == TipoDocumentoSalida);
                        numeroConsecutivoAuxSalida =
                            gestionConsecutivoSalida.BuscarConsecutivo(buscarTipoDocRegistroSalida, TipoDocumentoSalida,
                                pedido.bodega);
                        if (numeroConsecutivoAuxSalida != null)
                        {
                            numeroConsecutivoSalida = numeroConsecutivoAuxSalida.doccons_siguiente;
                        }
                        //registro la salida de inventario de dichos repuestos
                        encab_documento crearEncabezadoSalida = new encab_documento
                        {
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            //
                            tipo = TipoDocumentoSalida,
                            bodega = modelo.BodegaOrigen,
                            numero = numeroConsecutivoSalida,
                            nit = nitDespacho,
                            fecha = DateTime.Now,
                            fec_creacion = DateTime.Now,
                            impoconsumo = 0,
                            perfilcontable = perfilcs.id
                        };
                        context.encab_documento.Add(crearEncabezadoSalida);
                        int guardarsalida = context.SaveChanges();


                        List<lineas_documento> listalineastraslado = context.lineas_documento.Where(d =>
                                d.id_encabezado == buscarUltimoEncabezado.idencabezado &&
                                d.bodega == modelo.BodegaOrigen)
                            .ToList();
                        foreach (lineas_documento item in listalineastraslado)
                        {
                            icb_referencia referencia = context.icb_referencia.Where(d => d.ref_codigo == item.codigo)
                                .FirstOrDefault();
                            lineas_documento crearLineasSalida = new lineas_documento
                            {
                                codigo = item.codigo,
                                fec_creacion = DateTime.Now,
                                userid_creacion = item.userid_creacion,
                                nit = item.nit,
                                cantidad = item.cantidad,
                                bodega = modelo.BodegaDestino,
                                seq = sequ,
                                estado = true,
                                fec = DateTime.Now,
                                costo_unitario = item.costo_unitario,
                                id_encabezado = crearEncabezadoSalida.idencabezado,
                                id_solicitud_repuesto = item.id_solicitud_repuesto
                            };
                            context.lineas_documento.Add(crearLineasSalida);
                            //creo el registro de que esos repuestos fueron despachados a esa OT
                            ttrasladosorden traslados = new ttrasladosorden
                            {
                                idorden = pedido.orden_taller.Value,
                                idtraslado = crearEncabezado.idencabezado,
                                idtercero = pedido.tencabezaorden.tercero,
                                codigo = item.codigo,
                                cantidad = Convert.ToInt32(item.cantidad),
                                preciounitario = traercosto(referencia.ref_codigo),
                                poriva = Convert.ToDecimal(referencia.por_iva, miCultura),
                                pordescuento = Convert.ToDecimal(referencia.por_dscto, miCultura),
                                costopromedio = buscarvalor(item.codigo),
                                fec_creacion = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                estado = true
                            };
                            context.ttrasladosorden.Add(traslados);

                            //marco las referencias en solicitud de repuestos como recibidas
                            tsolicitudrepuestosot solicitudre = context.tsolicitudrepuestosot
                                .Where(d => d.id == item.id_solicitud_repuesto).FirstOrDefault();
                            if (solicitudre != null)
                            {
                                solicitudre.recibido = true;
                                solicitudre.canttraslado = Convert.ToInt32(item.cantidad);
                                if (solicitudre.idrepuesto != item.codigo)
                                {
                                    solicitudre.reemplazo = item.codigo;
                                }

                                context.Entry(solicitudre).State = EntityState.Modified;
                            }
                        }

                        //hago el cruze de documento de ese despacho con ese pedido 
                        cruce_documentos cruce = new cruce_documentos
                        {
                            id_encabezado = crearEncabezado.idencabezado,
                            id_encab_aplica = pedido.idencabezado,
                            idtipo = crearEncabezado.tipo,
                            numero = crearEncabezado.numero,
                            idtipoaplica = pedido.tipo,
                            numeroaplica = pedido.numero,
                            fecha = DateTime.Now,
                            fechacruce = DateTime.Now,
                            userid_creacion = crearEncabezado.userid_creacion
                        };
                        context.cruce_documentos.Add(cruce);
                        //context.SaveChanges();
                        int guardarLineasYMovimientos = context.SaveChanges();

                        if (guardarLineasYMovimientos > 0)
                        {
                            if (buscarGrupoConsecutivos != null)
                            {
                                List<icb_doc_consecutivos> numerosConsecutivos = context.icb_doc_consecutivos
                                    .Where(x => x.doccons_grupoconsecutivo == numeroGrupo).ToList();
                                foreach (icb_doc_consecutivos item in numerosConsecutivos)
                                {
                                    item.doccons_siguiente = item.doccons_siguiente + 1;
                                    context.Entry(item).State = EntityState.Modified;
                                }

                                context.SaveChanges();
                            }

                            if (buscarGrupoConsecutivosSalida != null)
                            {
                                List<icb_doc_consecutivos> numerosConsecutivos2 = context.icb_doc_consecutivos
                                    .Where(x => x.doccons_grupoconsecutivo == numeroGrupoSalida).ToList();
                                foreach (icb_doc_consecutivos item in numerosConsecutivos2)
                                {
                                    item.doccons_siguiente = item.doccons_siguiente + 1;
                                    context.Entry(item).State = EntityState.Modified;
                                }

                                context.SaveChanges();
                            }

                            dbTran.Commit();
                            return "ok";
                        }

                        dbTran.Rollback();
                        return "Error en guardado";
                    }

                    dbTran.Rollback();
                    return "Actualmente no existen stock de una o más referencias seleccionadas en este momento";
                }
                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    return ex.Message;
                }
            }
        }

        public decimal buscarvalor(string codigo)
        {
            if (!string.IsNullOrWhiteSpace(codigo))
            {
                icb_referencia referencia = context.icb_referencia.Where(d => d.ref_codigo == codigo).FirstOrDefault();
                if (referencia != null)
                {
                    vw_promedio buscarPromedio = context.vw_promedio.FirstOrDefault(x =>
                        x.codigo == codigo && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month);
                    decimal promedio = buscarPromedio != null ? buscarPromedio.Promedio ?? 0 : 0;
                    return promedio;
                }

                return 0;
            }

            return 0;
        }

        public float traerdescuento(string codigo)
        {
            if (!string.IsNullOrWhiteSpace(codigo))
            {
                //
                float resultado = 0;
                icb_referencia referencia = context.icb_referencia.Where(d => d.ref_codigo == codigo).FirstOrDefault();
                if (referencia != null)
                {
                    resultado = referencia.por_dscto;
                    return resultado;
                }

                return 0;
            }

            return 0;
        }

        public float traeriva(string codigo)
        {
            if (!string.IsNullOrWhiteSpace(codigo))
            {
                //
                float resultado = 0;
                icb_referencia referencia = context.icb_referencia.Where(d => d.ref_codigo == codigo).FirstOrDefault();
                if (referencia != null)
                {
                    resultado = referencia.por_iva;
                    return resultado;
                }

                return 0;
            }

            return 0;
        }

        public decimal traercosto(string codigo)
        {
            var buscarReferenciaSQL = (from referencia in context.icb_referencia
                                       join precios in context.rprecios
                                           on referencia.ref_codigo equals precios.codigo into ps2
                                       from precios in ps2.DefaultIfEmpty()
                                       where referencia.ref_codigo == codigo
                                       select new
                                       {
                                           precio = precios != null ? precios.precio1 : 0
                                       }
                ).FirstOrDefault();
            if (buscarReferenciaSQL != null)
            {
                return buscarReferenciaSQL.precio;
            }

            return 0;
        }

        public string trasladox(HttpRequestBase request, int? idpedido, DespachoOrden modelo)
        {
            bool listaref = int.TryParse(Request["lista_referencias"], out int lista);
            if (lista == 0)
            {
                return "Debe seleccionar referencias para poder hacer el traslado";
            }

            //Me traigo el id del tipo de documento de traslado de repuestos entre bodegas
            icb_sysparameter param1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P37").FirstOrDefault();
            int TipoDocumento = param1 != null ? Convert.ToInt32(param1.syspar_value) : 1028;
            //busco el perfil contable del tipo
            perfil_contable_documento perfilc = context.perfil_contable_documento.Where(d => d.tipo == TipoDocumento).FirstOrDefault();
            int perfilContableTraslado = perfilc.id;
            //busco el pedido
            encab_documento pedido = context.encab_documento.Where(d => d.idencabezado == idpedido).FirstOrDefault();
            long numeroConsecutivo = 0;
            grupoconsecutivos buscarGrupoConsecutivos = context.grupoconsecutivos.FirstOrDefault(x =>
                x.documento_id == TipoDocumento && x.bodega_id == pedido.bodega);
            int numeroGrupo = buscarGrupoConsecutivos != null ? buscarGrupoConsecutivos.grupo : 0;

            ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
            icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
            tp_doc_registros buscarTipoDocRegistro = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == TipoDocumento);
            numeroConsecutivoAux =
                gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro, TipoDocumento, pedido.bodega);
            if (numeroConsecutivoAux != null)
            {
                numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
            }
            else
            {
                return "No existe un numero consecutivo asignado para este tipo de documento";
            }

            //si llego hasta aqui es porque hay un consecutivo
            List<string> listaReferencias = new List<string>();
            int numero_repuestos = lista;
            int sequ = 1;
            List<string> codigosReferencias = new List<string>();
            for (int i = 0; i <= numero_repuestos; i++)
            {
                string referencia_codigo = Request["referencia" + i];
                if (!string.IsNullOrEmpty(referencia_codigo))
                {
                    codigosReferencias.Add(referencia_codigo);
                }
            }

            int buscarCantidadRefEnOrigen = (from referencia in context.referencias_inven
                                             where referencia.ano == DateTime.Now.Year
                                                   && referencia.mes == DateTime.Now.Month && referencia.bodega == pedido.bodega
                                                   && codigosReferencias.Contains(referencia.codigo)
                                             select referencia.codigo).Count();

            if (buscarCantidadRefEnOrigen > 0)
            {
                icb_sysparameter buscarNit = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P33");
                int nitTraslado = buscarNit != null ? Convert.ToInt32(buscarNit.syspar_value) : 0;

                string referenciasNoProcesadas = ", los siguientes codigos no se trasladaron por error en inventario";

                encab_documento crearEncabezado = new encab_documento
                {
                    notas = modelo.Notas,
                    tipo = TipoDocumento,
                    bodega = modelo.BodegaOrigen,
                    numero = numeroConsecutivo,
                    ///////crearEncabezado.documento = modelo.Referencia;
                    fecha = DateTime.Now,
                    fec_creacion = DateTime.Now,
                    impoconsumo = 0,
                    destinatario = pedido.userid_creacion,
                    bodega_destino = modelo.BodegaDestino,
                    perfilcontable = perfilContableTraslado,
                    nit = nitTraslado,
                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    orden_taller = pedido.orden_taller
                };
                context.encab_documento.Add(crearEncabezado);

                bool guardar = context.SaveChanges() > 0;
                encab_documento buscarUltimoEncabezado = context.encab_documento
                    .Where(x => x.idencabezado == crearEncabezado.idencabezado).FirstOrDefault();


                for (int i = 0; i <= numero_repuestos; i++)
                {
                    if (Request["referencia" + i] != null && Request["cantidadFacturar" + i] != null)
                    {
                        string referencia_codigo = Request["referencia" + i];
                        string referencia_cantidad = Request["cantidadFacturar" + i];
                        string referencia_costo = Request["valorAntesIvaReferencia" + i];
                        bool idsolicitudreferencia2 = int.TryParse(Request["idsolicitudreferenciaP" + i],
                            out int idsolicitudreferencia);
                        if (string.IsNullOrEmpty(referencia_codigo) || string.IsNullOrEmpty(referencia_cantidad))
                        {
                            // Significa que la agregaron y la eliminaron
                        }
                        else
                        {
                            listaReferencias.Add(referencia_codigo);
                            vw_promedio buscarPromedio = context.vw_promedio.FirstOrDefault(x =>
                                x.codigo == referencia_codigo && x.ano == DateTime.Now.Year &&
                                x.mes == DateTime.Now.Month);
                            decimal promedio = buscarPromedio != null ? buscarPromedio.Promedio ?? 0 : 0;
                            //para poner el promedio como el costo de precios
                            promedio = traercosto(referencia_codigo);
                            referencia_costo = promedio.ToString();
                            referencias_inven buscarReferenciasInvenOrigen = context.referencias_inven.FirstOrDefault(x =>
                                x.codigo == referencia_codigo && x.ano == DateTime.Now.Year &&
                                x.mes == DateTime.Now.Month && x.bodega == modelo.BodegaOrigen);
                            if (buscarReferenciasInvenOrigen != null)
                            {
                                buscarReferenciasInvenOrigen.can_sal =
                                    buscarReferenciasInvenOrigen.can_sal + Convert.ToInt32(referencia_cantidad);
                                buscarReferenciasInvenOrigen.cos_sal =
                                    buscarReferenciasInvenOrigen.cos_sal +
                                    promedio * Convert.ToInt32(referencia_cantidad);
                                buscarReferenciasInvenOrigen.can_tra =
                                    buscarReferenciasInvenOrigen.can_tra + Convert.ToInt32(referencia_cantidad);
                                buscarReferenciasInvenOrigen.cos_tra =
                                    buscarReferenciasInvenOrigen.cos_tra +
                                    promedio * Convert.ToInt32(referencia_cantidad);
                                context.Entry(buscarReferenciasInvenOrigen).State = EntityState.Modified;

                                //// La bodega de origen debe existir
                                //var buscarReferenciasInvenDestino = context.referencias_inven.FirstOrDefault(x => x.codigo == referencia_codigo && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month && x.bodega == modelo.BodegaDestino);
                                //if (buscarReferenciasInvenDestino != null)
                                //{
                                //    buscarReferenciasInvenDestino.can_ent = buscarReferenciasInvenDestino.can_ent + Convert.ToInt32(referencia_cantidad);
                                //    buscarReferenciasInvenDestino.cos_ent = (buscarReferenciasInvenDestino.cos_ent) + (promedio * Convert.ToInt32(referencia_cantidad));
                                //    buscarReferenciasInvenDestino.can_tra = buscarReferenciasInvenDestino.can_tra + Convert.ToInt32(referencia_cantidad);
                                //    buscarReferenciasInvenDestino.cos_tra = (buscarReferenciasInvenDestino.cos_tra) + (promedio * Convert.ToInt32(referencia_cantidad));
                                //    context.Entry(buscarReferenciasInvenDestino).State = EntityState.Modified;
                                //}
                                //else
                                //{
                                //    referencias_inven crearReferencia = new referencias_inven();

                                //    crearReferencia.bodega = modelo.BodegaDestino;
                                //    crearReferencia.codigo = referencia_codigo;
                                //    crearReferencia.ano = (short)DateTime.Now.Year;
                                //    crearReferencia.mes = (short)DateTime.Now.Month;
                                //    crearReferencia.can_ini = 0;
                                //    crearReferencia.can_ent = Convert.ToInt32(referencia_cantidad);
                                //    crearReferencia.can_sal = 0;
                                //    crearReferencia.cos_ent = (buscarReferenciasInvenOrigen.cos_ent) + (promedio * Convert.ToInt32(referencia_cantidad));
                                //    crearReferencia.can_tra = Convert.ToInt32(referencia_cantidad);
                                //    crearReferencia.cos_tra = (buscarReferenciasInvenOrigen.cos_tra) + (promedio * Convert.ToInt32(referencia_cantidad));
                                //    crearReferencia.modulo = "R";

                                //    context.referencias_inven.Add(crearReferencia);
                                //    context.SaveChanges();
                                //}
                            }
                            else
                            {
                                // Validado arriba porque debe haber referencias inven en la bodega de origen
                                referenciasNoProcesadas += ", " + referencia_codigo;
                            }

                            lineas_documento crearLineasOrigen = new lineas_documento
                            {
                                codigo = referencia_codigo,
                                fec_creacion = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                nit = nitTraslado,
                                cantidad = !string.IsNullOrEmpty(referencia_cantidad)
                                    ? Convert.ToDecimal(referencia_cantidad, miCultura)
                                    : 0,
                                bodega = pedido.bodega,
                                seq = sequ,
                                estado = true,
                                fec = DateTime.Now,
                                costo_unitario = !string.IsNullOrEmpty(referencia_costo)
                                    ? Convert.ToDecimal(referencia_costo, miCultura)
                                    : 0,
                                id_encabezado = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0
                            };
                            if (idsolicitudreferencia != 0)
                            {
                                crearLineasOrigen.id_solicitud_repuesto = idsolicitudreferencia;
                            }

                            context.lineas_documento.Add(crearLineasOrigen);
                            int guardar1 = context.SaveChanges();

                            lineas_documento crearLineasDestino = new lineas_documento
                            {
                                codigo = referencia_codigo,
                                fec_creacion = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                nit = nitTraslado,
                                cantidad = !string.IsNullOrEmpty(referencia_cantidad)
                                    ? Convert.ToDecimal(referencia_cantidad, miCultura)
                                    : 0,
                                bodega = modelo.BodegaDestino,
                                seq = sequ,
                                estado = true,
                                fec = DateTime.Now,
                                costo_unitario = !string.IsNullOrEmpty(referencia_costo)
                                    ? Convert.ToDecimal(referencia_costo, miCultura)
                                    : 0,
                                id_encabezado = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0
                            };
                            if (idsolicitudreferencia != 0)
                            {
                                crearLineasDestino.id_solicitud_repuesto = idsolicitudreferencia;
                            }

                            context.lineas_documento.Add(crearLineasDestino);
                            int guardar2 = context.SaveChanges();

                            sequ++;

                            recibidotraslados recibidos = new recibidotraslados
                            {
                                idtraslado = buscarUltimoEncabezado.idencabezado,
                                refcodigo = referencia_codigo,
                                cantidad = Convert.ToInt32(referencia_cantidad),
                                recibido = false,
                                fecharecibido = null,
                                userrecibido = null,
                                fechatraslado = DateTime.Now,
                                usertraslado = Convert.ToInt32(Session["user_usuarioid"]),
                                costo = Convert.ToDecimal(referencia_costo, miCultura),
                                idorigen = pedido.bodega,
                                iddestino = modelo.BodegaDestino,
                                tipo = "R"
                            };
                            context.recibidotraslados.Add(recibidos);
                            int guardar3 = context.SaveChanges();
                        }
                    }
                }

                icb_sysparameter tipodocsal = context.icb_sysparameter.Where(d => d.syspar_cod == "P42").FirstOrDefault();
                int TipoDocumentoSalida = tipodocsal != null ? Convert.ToInt32(tipodocsal.syspar_value) : 21;
                //busco el perfil contable de salida
                perfil_contable_documento perfilcs = context.perfil_contable_documento.Where(d => d.tipo == TipoDocumentoSalida)
                    .FirstOrDefault();
                //busco el consecutivo de salida de repuestos
                grupoconsecutivos buscarGrupoConsecutivosSalida = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == TipoDocumentoSalida && x.bodega_id == pedido.bodega);
                int numeroGrupoSalida = buscarGrupoConsecutivos != null ? buscarGrupoConsecutivosSalida.grupo : 0;

                long numeroConsecutivoSalida = 1;
                ConsecutivosGestion gestionConsecutivoSalida = new ConsecutivosGestion();
                icb_doc_consecutivos numeroConsecutivoAuxSalida = new icb_doc_consecutivos();
                tp_doc_registros buscarTipoDocRegistroSalida =
                    context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == TipoDocumentoSalida);
                numeroConsecutivoAuxSalida = gestionConsecutivoSalida.BuscarConsecutivo(buscarTipoDocRegistroSalida,
                    TipoDocumentoSalida, pedido.bodega);
                if (numeroConsecutivoAuxSalida != null)
                {
                    numeroConsecutivoSalida = numeroConsecutivoAuxSalida.doccons_siguiente;
                }
                //registro la salida de inventario de dichos repuestos
                encab_documento crearEncabezadoSalida = new encab_documento
                {
                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    //
                    tipo = TipoDocumentoSalida,
                    bodega = modelo.BodegaOrigen,
                    numero = numeroConsecutivoSalida,
                    nit = nitTraslado,
                    fecha = DateTime.Now,
                    fec_creacion = DateTime.Now,
                    impoconsumo = 0,
                    perfilcontable = perfilcs.id
                };
                context.encab_documento.Add(crearEncabezadoSalida);
                int guardarsalida = context.SaveChanges();


                List<lineas_documento> listalineastraslado = context.lineas_documento.Where(d =>
                    d.id_encabezado == buscarUltimoEncabezado.idencabezado && d.bodega == modelo.BodegaOrigen).ToList();
                foreach (lineas_documento item in listalineastraslado)
                {
                    lineas_documento crearLineasSalida = new lineas_documento
                    {
                        codigo = item.codigo,
                        fec_creacion = DateTime.Now,
                        userid_creacion = item.userid_creacion,
                        nit = item.nit,
                        cantidad = item.cantidad,
                        bodega = modelo.BodegaDestino,
                        seq = sequ,
                        estado = true,
                        fec = DateTime.Now,
                        costo_unitario = item.costo_unitario,
                        id_encabezado = crearEncabezadoSalida.idencabezado
                    };
                    if (item.id_solicitud_repuesto != null)
                    {
                        crearLineasSalida.id_solicitud_repuesto = item.id_solicitud_repuesto;
                    }

                    context.lineas_documento.Add(crearLineasSalida);
                }

                //hago el cruze de documento de ese traslado con ese pedido 
                cruce_documentos cruce = new cruce_documentos
                {
                    id_encabezado = crearEncabezado.idencabezado,
                    id_encab_aplica = pedido.idencabezado,
                    idtipo = crearEncabezado.tipo,
                    numero = crearEncabezado.numero,
                    idtipoaplica = pedido.tipo,
                    numeroaplica = pedido.numero,
                    fecha = DateTime.Now,
                    fechacruce = DateTime.Now,
                    userid_creacion = crearEncabezado.userid_creacion
                };
                context.cruce_documentos.Add(cruce);
                //context.SaveChanges();


                #region parametros contables de salida de inventario (se comenta)

                /*
				    var promedioReferencias = (from referencia in context.icb_referencia
				                               join promedioAux in context.vw_promedio
				                               on referencia.ref_codigo equals promedioAux.codigo into pro
				                               from promedioAux in pro.DefaultIfEmpty()
				                               where listaReferencias.Contains(promedioAux.codigo)
				                               select promedioAux.Promedio).Sum();

				    var cuentas = new trasladoRepuestosController();
				    var perfilcontable = context.perfil_contable_documento.Where(d => d.tipo == TipoDocumento).FirstOrDefault();
				    var parametrosCuentas = context.perfil_cuentas_documento.Where(x => x.id_perfil == perfilcontable.id).ToList();
				    var secuencia = 1;
				    var centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
				    var idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
				    var terceroValorCero = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
				    var idTerceroCero = centroValorCero != null ? Convert.ToInt32(terceroValorCero.tercero_id) : 0;

				    var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
				                                      join nombreParametro in context.paramcontablenombres
				                                      on perfil.id_nombre_parametro equals nombreParametro.id
				                                      join cuenta in context.cuenta_puc
				                                      on perfil.cuenta equals cuenta.cntpuc_id
				                                      where perfil.id_perfil == TipoDocumento
				                                      select new
				                                      {
				                                          perfil.id,
				                                          perfil.id_nombre_parametro,
				                                          perfil.cuenta,
				                                          perfil.centro,
				                                          perfil.id_perfil,
				                                          nombreParametro.descripcion_parametro,
				                                          cuenta.cntpuc_numero
				                                      }).ToList();

				    foreach (var parametro in parametrosCuentas)
				    {
				        var buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
				        if (buscarCuenta != null)
				        {
				            //foreach (var cuenta in parametrosCuentasVerificar)
				            //{
				            if (parametro.id_nombre_parametro == 9 || parametro.id_nombre_parametro == 11)
				            {
				                mov_contable movNuevo = new mov_contable();
				                movNuevo.id_encab = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0;
				                movNuevo.fec_creacion = DateTime.Now;
				                movNuevo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
				                movNuevo.idparametronombre = parametro.id_nombre_parametro;
				                movNuevo.cuenta = parametro.cuenta;
				                movNuevo.centro = parametro.centro;
				                movNuevo.nit = idTerceroCero;
				                movNuevo.fec = DateTime.Now;
				                movNuevo.seq = secuencia;
				                //movNuevo.credito = modelo.Costo;                
				                if (buscarCuenta.tercero)
				                {
				                    movNuevo.nit = nitTraslado;
				                }
				                if (buscarCuenta.concepniff == 1)
				                {

				                    movNuevo.debitoniif = promedioReferencias ?? 0;
				                    movNuevo.debito = promedioReferencias ?? 0;
				                }

				                if (buscarCuenta.concepniff == 4)
				                {
				                    movNuevo.debitoniif = promedioReferencias ?? 0;
				                }

				                if (buscarCuenta.concepniff == 5)
				                {
				                    movNuevo.debito = promedioReferencias ?? 0;
				                }

				                movNuevo.detalle = "Traslado de repuesto ";
				                secuencia++;
				                cuentas.AgregarRegistroCuentasValores(movNuevo, parametro.centro, parametro.cuenta, idCentroCero, idTerceroCero);
				                context.mov_contable.Add(movNuevo);
				                context.SaveChanges();
				            }
				            if (parametro.id_nombre_parametro == 20)
				            {
				                mov_contable movNuevo2 = new mov_contable();
				                movNuevo2.id_encab = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0;
				                movNuevo2.fec_creacion = DateTime.Now;
				                movNuevo2.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
				                movNuevo2.idparametronombre = parametro.id_nombre_parametro;
				                movNuevo2.cuenta = parametro.cuenta;
				                movNuevo2.centro = parametro.centro;
				                movNuevo2.nit = idTerceroCero;
				                movNuevo2.fec = DateTime.Now;
				                movNuevo2.seq = secuencia;
				                //movNuevo2.debito = modelo.Costo;
				                movNuevo2.credito = promedioReferencias ?? 0;
				                if (buscarCuenta.tercero)
				                {
				                    movNuevo2.nit = nitTraslado;
				                }
				                if (buscarCuenta.concepniff == 1)
				                {


				                    movNuevo2.credito = promedioReferencias ?? 0;
				                    movNuevo2.creditoniif = promedioReferencias ?? 0;

				                }

				                if (buscarCuenta.concepniff == 4)
				                {

				                    movNuevo2.creditoniif = promedioReferencias ?? 0;

				                }

				                if (buscarCuenta.concepniff == 5)
				                {

				                    movNuevo2.credito = promedioReferencias ?? 0;

				                }
				                movNuevo2.detalle = "Traslado de repuesto ";
				                secuencia++;
				                
				                cuentas.AgregarRegistroCuentasValores(movNuevo2, parametro.centro, parametro.cuenta, idCentroCero, idTerceroCero);
				                context.mov_contable.Add(movNuevo2);
				            }

				            //}


				        }
				    }*/

                #endregion

                int guardarLineasYMovimientos = context.SaveChanges();

                if (guardarLineasYMovimientos > 0)
                {
                    if (buscarGrupoConsecutivos != null)
                    {
                        List<icb_doc_consecutivos> numerosConsecutivos = context.icb_doc_consecutivos
                            .Where(x => x.doccons_grupoconsecutivo == numeroGrupo).ToList();
                        foreach (icb_doc_consecutivos item in numerosConsecutivos)
                        {
                            item.doccons_siguiente = item.doccons_siguiente + 1;
                            context.Entry(item).State = EntityState.Modified;
                        }

                        context.SaveChanges();
                    }

                    if (buscarGrupoConsecutivosSalida != null)
                    {
                        List<icb_doc_consecutivos> numerosConsecutivos2 = context.icb_doc_consecutivos
                            .Where(x => x.doccons_grupoconsecutivo == numeroGrupoSalida).ToList();
                        foreach (icb_doc_consecutivos item in numerosConsecutivos2)
                        {
                            item.doccons_siguiente = item.doccons_siguiente + 1;
                            context.Entry(item).State = EntityState.Modified;
                        }

                        context.SaveChanges();
                    }

                    return "ok";
                }

                return "Error en guardado";
            }

            return "Actualmente no existen stock de una o más referencias seleccionadas en este momento";
        }

        public JsonResult buscardatosorden(int? idorden)
        {
            if (idorden != null)
            {
                //veo si existe la ot

                tencabezaorden orden = context.tencabezaorden.Where(d => d.id == idorden).FirstOrDefault();
                if (orden != null)
                {
                    string codigoentrada = orden.codigoentrada;
                    icb_vehiculo vehiculo = context.icb_vehiculo.Where(d => d.plan_mayor == orden.placa).FirstOrDefault();
                    icb_terceros tercero = context.icb_terceros.Where(d => d.tercero_id == orden.tercero).FirstOrDefault();
                    int bodega = orden.bodega;
                    //veo si tiene solicitudes de repuestos sin pedir
                    List<tsolicitudrepuestosot> solicitudes = context.tsolicitudrepuestosot
                        .Where(d => d.idorden == idorden && d.pedido == false).ToList();
                    //creo mis objetos que voy a meter en la clase de objeto
                    List<tablasolicitud> formulario = solicitudes.Select(d => new tablasolicitud
                    {
                        idorden = d.idorden,
                        idsolicitud = d.id,
                        referencia = d.idrepuesto,
                        idbodega = bodega,
                        desc_referencia = d.icb_referencia.ref_descripcion,
                        cantidad = d.cantidad != null ? d.cantidad.Value : 0,
                        stock_bodega = context.vw_inventario_hoy
                            .Where(e => e.ref_codigo == d.idrepuesto && e.bodega == bodega).Select(e => e.stock)
                            .FirstOrDefault(),
                        stock_otras_bodegas = context.vw_inventario_hoy
                            .Where(e => e.ref_codigo == d.idrepuesto && e.bodega != bodega &&
                                        e.stock >= (d.cantidad != null ? d.cantidad.Value : 0)).Select(e =>
                                new tablastock_otras_bodegas
                                {
                                    stockotrabodega = e.bodega + "," + e.stock,
                                    nombrebodega = e.nombreBodega + " (stock: " + e.stock + ")"
                                }).ToList(),
                        tablastockreemplazo = traerstockreemplazo(d.idrepuesto, d.tencabezaorden.bodega,
                            d.cantidad != null ? d.cantidad.Value : 0),
                        cantidadComprometida = d.cantidad != null ? d.cantidad.Value : 0,
                        //cantidadComprometida = context.vw_kardex2.Where(x => x.codigo == d.idrepuesto && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month).FirstOrDefault() != null ?
                        //context.vw_kardex2.Where(x => x.codigo == d.idrepuesto && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month).FirstOrDefault().separado != null ?
                        //context.vw_kardex2.Where(x => x.codigo == d.idrepuesto && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month).FirstOrDefault().separado.Value : 0 : 0,

                    }).ToList();

                    var data = new
                    {
                        idorden,
                        codigoorden = codigoentrada,
                        vehiculo = vehiculo.marca_vehiculo.marcvh_nombre + " " + vehiculo.modelo_vehiculo.modvh_nombre +
                                   " " + vehiculo.anio_vh,
                        tercero = !string.IsNullOrWhiteSpace(tercero.razon_social)
                            ? tercero.razon_social
                            : tercero.prinom_tercero + " " + tercero.segnom_tercero + " " + tercero.apellido_tercero +
                              " " + tercero.segapellido_tercero,
                        fecha = orden.fec_creacion.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")),
                        bodega = orden.bodega_concesionario.bodccs_nombre,
                        solicitudes = formulario
                    };
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                return Json(0, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public List<tablastockreemplazo> traerstockreemplazo(string idrepuesto, int? idbodega, int? cantidad)
        {
            if (!string.IsNullOrWhiteSpace(idrepuesto) && idbodega != null && cantidad != null)
            {
                //busco la bodega que exusta
                bodega_concesionario bod = context.bodega_concesionario.Where(d => d.id == idbodega).FirstOrDefault();
                //busco que exista la referencia
                icb_referencia refer = context.icb_referencia.Where(d => d.ref_codigo == idrepuesto).FirstOrDefault();
                if (bod != null && refer != null)
                {
                    int encontrado = 0;
                    List<tablastockreemplazo> lista = new List<tablastockreemplazo>();

                    //busco el reemplazo de esta referencia en ESTA bodega
                    rremplazos reemplazo = context.rremplazos.Where(d => d.referencia == idrepuesto && d.alterno != idrepuesto)
                        .FirstOrDefault();
                    //si tiene reemplazo
                    if (reemplazo != null)
                    {
                        string idreemp = reemplazo.alterno;
                        //busco si tiene stock ese reemplazo
                        do
                        {
                            decimal stock = context.vw_inventario_hoy
                                .Where(e => e.ref_codigo == idreemp && e.bodega == idbodega).Select(e => e.stock)
                                .FirstOrDefault();
                            if (stock > 0 && stock >= cantidad)
                            {
                                lista.Add(context.vw_inventario_hoy
                                    .Where(e => e.ref_codigo == idreemp && e.bodega == idbodega).Select(e =>
                                        new tablastockreemplazo
                                        {
                                            stockreemplazo = e.bodega + "," + e.stock,
                                            nombrereferencia = e.ref_descripcion + " (stock: " + e.stock + ")"
                                        }).FirstOrDefault());
                                encontrado = 1;
                            }
                            else
                            {
                                //veo si ese reemplazo tiene reemplazo y vuelvo
                                rremplazos reemplazo2 = context.rremplazos
                                    .Where(d => d.referencia == idreemp && d.alterno != idreemp).FirstOrDefault();
                                if (reemplazo2 != null)
                                {
                                    idreemp = reemplazo2.alterno;
                                    encontrado = 0;
                                }
                                else
                                {
                                    encontrado = 1;
                                }
                            }
                        } while (encontrado == 0);
                    }

                    return lista;
                }

                return new List<tablastockreemplazo>();
            }

            return new List<tablastockreemplazo>();
        }

        public void operacionesPedidoTaller()
        {
            //string sIdOperacion = db.icb_sysparameter.Where(x => x.syspar_cod.Contains("P81")).Select(x => x.syspar_value).FirstOrDefault();
            //iIdOperacion = Convert.ToInt32(sIdOperacion); // id Operacion Alistamiento
            //string sCreateOt = db.icb_sysparameter.Where(x => x.syspar_cod.Contains("P78")).Select(x => x.syspar_value).FirstOrDefault();
            //iCreateOt = Convert.ToInt32(sCreateOt); // id estado creacion Ot
            //string sFinishOt = db.icb_sysparameter.Where(x => x.syspar_cod.Contains("P89")).Select(x => x.syspar_value).FirstOrDefault();
            //iFinishOt = Convert.ToInt16(sFinishOt); // id estado finalizacion Ot TEMPORAL
            //string sExecutionOt = db.icb_sysparameter.Where(x => x.syspar_cod.Contains("P84")).Select(x => x.syspar_value).FirstOrDefault();
            //iExecutionOt = Convert.ToInt16(sExecutionOt); // id estado ejecucion Ot TEMPORAL
            //string tpbahia_alis = db.icb_sysparameter.Where(x => x.syspar_cod.Contains("P82")).Select(x => x.syspar_value).FirstOrDefault();
            //idtpbahia_alis = Convert.ToInt32(tpbahia_alis); // id bahia alistamiento
            //bahia = db.tbahias.Where(x => x.bodega == bodegaActual && x.tipo_bahia == idtpbahia_alis).Select(x => x.id).FirstOrDefault(); // Id bahia alistamiento
            //string sEnvAlis = db.icb_sysparameter.Where(x => x.syspar_cod == "P85").Select(x => x.syspar_value).FirstOrDefault(); // Envio Alistamiento
            //iEnvAlis = Convert.ToInt32(sEnvAlis);// Envio Alistamiento
            //string sFinAlis = db.icb_sysparameter.Where(x => x.syspar_cod == "P86").Select(x => x.syspar_value).FirstOrDefault(); // Fin Alistamiento
            //iFinAlis = Convert.ToInt32(sFinAlis);// Fin Alistamiento
            //string sEnvRAlis = db.icb_sysparameter.Where(x => x.syspar_cod == "P92").Select(x => x.syspar_value).FirstOrDefault(); // Envio Alistamiento
            //iEnvRAlis = Convert.ToInt32(sEnvRAlis);// Envio Realistamiento
            //string sFinRAlis = db.icb_sysparameter.Where(x => x.syspar_cod == "P93").Select(x => x.syspar_value).FirstOrDefault(); // Fin Alistamiento
            //iFinRAlis = Convert.ToInt32(sFinRAlis);// Fin Realistamiento
        }

        [HttpPost]
        public JsonResult generarPedido()
        {
            int bodegaLog = Convert.ToInt32(Session["user_bodega"]);
            int userLog = Convert.ToInt32(Session["user_usuarioid"]);

            operacionesPedidoTaller();
            int idorden = int.Parse(Request["idordenmodal"]);
            int numeroreferencias = int.Parse(Request["numero_solicitudes"]);
            //busco si la orden existe
            tencabezaorden orden = context.tencabezaorden.Where(d => d.id == idorden).FirstOrDefault();

            tdetallerepuestosot rep = context.tdetallerepuestosot.Where(d => d.idorden == idorden).FirstOrDefault();

            int bodega = orden.bodega;
            List<referenciasSolicita> pedido = new List<referenciasSolicita>();
            List<referenciasTraslada> traslado = new List<referenciasTraslada>();
            int registros = 0;
            if (orden != null)
            {
                try
                {
                    for (int i = 0; i < numeroreferencias; i++)
                    {
                        //si se seleccionaron las referencias
                        if (Request["tipoau_" + i] != null)
                        {
                            bool convertir = int.TryParse(Request["tipoau_" + i], out int refer);
                            string referencia = Request["referencia_" + i];

                            bool convertir22 = int.TryParse(Request["cantidad_" + i], out int canti);

                            bool convertirsoli = int.TryParse(Request["idsolireferencia_" + i], out int idsolicitud);
                            string referemp = "";

                            if (convertir)
                            {
                                //si la referencia es de la bodega o si es el reemplazo de la bodega
                                if (refer == 1 || refer == 3)
                                {

                                    rep.comprometido = true;
                                    rep.estadorepuesto = 4;
                                    context.Entry(rep).State = EntityState.Modified;
                                    context.SaveChanges();

                                    if (refer == 1)
                                    {
                                        //var convertir2 = int.TryParse(Request["stock_bodega_" + i],out canti);
                                    }

                                    if (refer == 3)
                                    {
                                        string convertir3 = Request["stock_reemplazo_" + i];
                                        int bodegareemp = 0;
                                        string[] arre = convertir3.Split(',');

                                        if (arre.Length == 2)
                                        {
                                            bodegareemp = Convert.ToInt32(arre[0]);
                                            referemp = arre[1];
                                        }
                                    }

                                    pedido.Add(new referenciasSolicita
                                    {
                                        cantidad = canti,
                                        idorden = orden.id,
                                        id_referencia = referencia,
                                        id_reemplazo = referemp,
                                        iddetallesolicitud = idsolicitud
                                    });
                                }
                                else if (refer == 2) //si la referencia es de otra bodega;
                                {
                                    rep.comprometido = true;
                                    rep.estadorepuesto = 2;
                                    context.Entry(rep).State = EntityState.Modified;
                                    context.SaveChanges();

                                    int otrabodega = 0;
                                    string convertir3 = Request["stock_otras_bodegas_" + i];
                                    string[] arre = convertir3.Split(',');

                                    if (arre.Length == 2)
                                    {
                                        otrabodega = Convert.ToInt32(arre[0]);
                                        referemp = arre[1];
                                    }

                                    traslado.Add(new referenciasTraslada
                                    {
                                        idorden = orden.id,
                                        idbodega = otrabodega,
                                        id_referencia = referencia,
                                        cantidad = canti,
                                        idbodegarecibe = bodega,
                                        iddetallesolicitud = idsolicitud
                                    });
                                }

                                registros++;
                            }
                        }
                    }

                    //var x = 1;
                    if (registros > 0)
                    {
                        // procedo a crear los traslados o pedidos
                        if (pedido.Count > 0)
                        {
                            int pedidox = realizarSolicitudDespacho(orden.id, null, pedido, bodega, bodega);
                        }

                        if (traslado.Count > 0)
                        {
                            //agrupo el listado de traslados por bodegas
                            List<int> trasladox = traslado.GroupBy(d => d.idbodega).Select(d => d.Key).ToList();
                            foreach (int item in trasladox)
                            {
                                //creo las solicitudes de traslado
                                //int trasladohecho = realizarSolicitudDespacho(orden.id, traslado.Where(d => d.idbodega == item).ToList(), null, bodega, item);
                                int trasladohecho = realizarSalidaTraslado(orden.id, traslado.Where(d => d.idbodega == item).ToList(), bodega, item);

                            }
                        }

                        var data = new
                        {
                            respuesta = 1,
                            descripcion = "Se ha(n) generado el(los) pedido(s) de despacho de repuestos exitosamente"
                        };
                        return Json(data, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var data = new
                        {
                            respuesta = 0,
                            descripcion = "No se procesó la solicitud de ninguna referencia para esta orden"
                        };
                        return Json(data, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    Exception exp = ex;
                    var data = new
                    {
                        respuesta = 0,
                        descripcion = "No existe la orden de trabajo"
                    };
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }

            {
                var data = new
                {
                    respuesta = 0,
                    descripcion = "No existe la orden de trabajo"
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }
        //[ValidateAntiForgeryToken]
        //public JsonResult PedidoRepuestos()
        //{
        //    operacionAlistamiento();
        //    var resp = 0;
        //    int idpedido = int.Parse(Request["idpedido"]);
        //    icb_bahia_alistamiento bhAlis = db.icb_bahia_alistamiento.SingleOrDefault(x => x.id_pedido == idpedido && (x.tencabezaorden.estadoorden == iCreateOt || x.tencabezaorden.estadoorden == iExecutionOt));
        //    if (bhAlis != null)
        //    {
        //        using (DbContextTransaction dbTran = db.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                tencabezaorden ot = db.tencabezaorden.SingleOrDefault(x => x.id == bhAlis.ot_id);
        //                if (ot != null)
        //                {
        //                    ot.estadoorden = (ot.estadoorden == iCreateOt) ? iExecutionOt : iFinishOt;
        //                    ot.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
        //                    ot.fec_actualizacion = DateTime.Now;
        //                    db.Entry(ot).State = EntityState.Modified;
        //                    resp = db.SaveChanges();

        //                    var pendAlis = db.vw_pendientesEntrega.Where(x => x.id == idpedido).FirstOrDefault();
        //                    // Validar si evento es realistaminto o alistamiento
        //                    var icbvh_even = db.icb_vehiculo_eventos.Where(x => x.planmayor.Contains(pendAlis.planmayor) && x.id_tpevento == iFinAlis).FirstOrDefault();
        //                    int idevent = (icbvh_even == null) ? iFinAlis : iFinRAlis;
        //                    // Registrar evento fin alistamiento
        //                    var buscarEvento = db.icb_tpeventos.FirstOrDefault(x => x.tpevento_id == idevent);// Fin (Alistamiento - Re Alistamiento)
        //                                                                                                      // Condicionar evento 
        //                    if (ot.estadoorden == iFinishOt)
        //                    {
        //                        // Validar qué se debe finalizar (alistamiento - re-alistamiento)
        //                        icb_vehiculo_eventos crearEvento = new icb_vehiculo_eventos();
        //                        var propietario_vh = db.icb_vehiculo.Find(pendAlis.planmayor).propietario;
        //                        if (propietario_vh != null || propietario_vh > 0)
        //                        {
        //                            crearEvento.terceroid = propietario_vh;
        //                        }
        //                        crearEvento.eventofec_creacion = DateTime.Now;
        //                        crearEvento.eventouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
        //                        crearEvento.evento_estado = true;
        //                        crearEvento.bodega_id = Convert.ToInt32(Session["user_bodega"]);
        //                        crearEvento.evento_nombre = buscarEvento.tpevento_nombre;
        //                        crearEvento.planmayor = pendAlis.planmayor;
        //                        crearEvento.id_tpevento = idevent;
        //                        crearEvento.fechaevento = DateTime.Now;
        //                        crearEvento.placa = pendAlis.plac_vh;
        //                        crearEvento.ubicacion = pendAlis.ubivh_id;
        //                        db.icb_vehiculo_eventos.Add(crearEvento);
        //                        db.SaveChanges();
        //                    }
        //                }
        //                dbTran.Commit();
        //            }
        //            catch (Exception ex)
        //            {
        //                var exp = ex;
        //                dbTran.Rollback();
        //            }
        //        }
        //    }
        //    return Json(new { resp = resp }, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult buscarOrdenes()
        {
            var repuestosAgendados = (from te in context.tencabezaorden
                                      join c in context.icb_terceros
                                          on te.tercero equals c.tercero_id
                                      join b in context.bodega_concesionario
                                          on te.bodega equals b.id
                                      join v in context.icb_vehiculo
                                          on te.placa equals v.plan_mayor
                                      join m in context.modelo_vehiculo
                                          on v.modvh_id equals m.modvh_codigo
                                      join ts in context.tsolicitudrepuestosot
                                          on te.id equals ts.idorden
                                      where ts.cantidad - ts.canttraslado > 0
                                      select new
                                      {
                                          idRS = te.id,
                                          numero = te.codigoentrada,
                                          idTercero = c.doc_tercero,
                                          cliente = "(" + c.doc_tercero + ") " + c.prinom_tercero + " " + c.segnom_tercero + " " +
                                                    c.prinom_tercero + " " + c.segapellido_tercero + " " + c.razon_social,
                                          vehiculo = "(" + v.plan_mayor + ") " + m.modvh_nombre,
                                          anioV = v.anio_vh,
                                          fechaO = te.fecha,
                                          te.kilometraje,
                                          bodega = b.bodccs_nombre
                                      }).Distinct().ToList();

            var data = repuestosAgendados.Select(x => new
            {
                x.numero,
                x.idTercero,
                x.idRS,
                x.cliente,
                x.vehiculo,
                x.anioV,
                fechaO = x.fechaO.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                x.kilometraje,
                x.bodega
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarDatos(int idORden)
        {
            var repuestosAgendados = (from ts in context.tsolicitudrepuestosot
                                      join te in context.tencabezaorden
                                          on ts.idorden equals te.id
                                      join tr in context.tdetallerepuestosot
                                          on new { ax = te.id, bx = ts.iddetalle ?? 0 } equals new { ax = tr.idorden, bx = tr.id } into temp
                                      from tr in temp.DefaultIfEmpty()
                                      join c in context.icb_terceros
                                          on te.tercero equals c.tercero_id
                                      /*join tt in context.ttipostarifa 
                                      on tr.tipotarifa equals tt.id*/
                                      join b in context.bodega_concesionario
                                          on te.bodega equals b.id
                                      join r in context.icb_referencia
                                          on ts.idrepuesto equals r.ref_codigo
                                      join v in context.icb_vehiculo
                                          on te.placa equals v.plan_mayor
                                      join m in context.modelo_vehiculo
                                          on v.modvh_id equals m.modvh_codigo
                                      where ts.idorden == idORden
                                      select new
                                      {
                                          idRS = tr.id,
                                          nOrden = te.numero,
                                          cliente = "(" + c.doc_tercero + ") " + c.prinom_tercero + " " + c.segnom_tercero + " " +
                                                    c.prinom_tercero + " " + c.segapellido_tercero + " " + c.razon_social,
                                          vehiculo = "(" + v.plan_mayor + ") " + m.modvh_nombre,
                                          anioV = v.anio_vh,
                                          fechaO = te.fecha,
                                          te.kilometraje,
                                          /*idTarifa = tr.tipotarifa,
                                          desTarifa = tt.Descripcion,*/
                                          idBodega = te.bodega,
                                          bodega = b.bodccs_nombre,
                                          r.ref_codigo,
                                          repuesto = "(" + r.ref_codigo + ") " + r.ref_descripcion,
                                          cantidad = ts.cantidad - ts.canttraslado,
                                          fechaS = ts.fecsolicitud,
                                          ts.recibido
                                      }).ToList();

            var data = repuestosAgendados.Select(x => new
            {
                x.ref_codigo,
                x.idBodega,
                /*x.idTarifa,
				x.desTarifa,*/
                x.idRS,
                x.nOrden,
                x.cliente,
                x.vehiculo,
                x.anioV,
                fechaO = x.fechaO.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                x.kilometraje,
                x.bodega,
                x.repuesto,
                x.cantidad,
                fechaS = x.fechaS.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                x.recibido
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult recibidoReferencia(int idSolicitado, string referencia)
        {
            icb_referencia generico =
                context.icb_referencia.FirstOrDefault(x => x.manejo_inv == false && x.ref_codigo == referencia);
            if (generico == null)
            {
                tsolicitudrepuestosot existe = context.tsolicitudrepuestosot.FirstOrDefault(x => x.iddetalle == idSolicitado);
                if (existe != null)
                {
                    existe.recibido = true;
                    existe.fecrecibidor = DateTime.Now;
                    context.Entry(existe).State = EntityState.Modified;
                    int ok = context.SaveChanges();

                    return Json(new { error = false, ok }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { error = true, mensaje = "" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { generico = true, error = true, mensaje = "" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult trasladoRepuestos(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int bodegaLog = Convert.ToInt32(Session["user_bodega"]);
            string bodegaNombre = context.bodega_concesionario.FirstOrDefault(x => x.id == bodegaLog).bodccs_nombre;

            ViewBag.bodegaOrigen = bodegaLog;
            ViewBag.bodegaOrigenNombre = bodegaNombre;
            ViewBag.tipoTarifa = new SelectList(context.ttipostarifa.Where(x => x.estado), "id", "Descripcion");

            //ViewBag.tipoTarifa = context.ttipostarifa.Where(x => x.estado == true);

            ViewBag.idsolicitado = id;
            listas();
            return View();
        }

        public int realizarSolicitudDespacho(int orden, List<referenciasTraslada> listaTraslada, List<referenciasSolicita> listaSolicita, int bodega, int bodegaorigen)
        {
            //busco la orden
            tencabezaorden order = context.tencabezaorden.Where(d => d.id == orden).FirstOrDefault();

            //traigo el código de tipo documento orden de despacho de repuesto
            icb_sysparameter parped = context.icb_sysparameter.Where(d => d.syspar_cod == "P97").FirstOrDefault();
            int pedidorep = parped != null ? Convert.ToInt32(parped.syspar_value) : 3074;
            //busco el id del tipo de documento
            tp_doc_registros conse = context.tp_doc_registros.Where(d => d.tpdoc_id == pedidorep).FirstOrDefault();

            //busco el consecutivo
            grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                x.documento_id == conse.tpdoc_id && x.bodega_id == bodega);
            //creo el encabezado de la solicitud
            DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
            long consecutivo = doc.BuscarConsecutivo(grupo.grupo);
            //creo el pedido
            encab_documento crearEncabezado = new encab_documento
            {
                fecha = DateTime.Now,
                fec_creacion = DateTime.Now,
                tipo = conse.tpdoc_id,
                bodega = listaTraslada != null ? bodegaorigen : bodega,
                numero = consecutivo,
                valor_total = 0,
                valor_aplicado = 0,
                valor_mercancia = 0,
                iva = 0,
                notas = "",
                vendedor = Convert.ToInt32(Session["user_usuarioid"]),
                costo = 0,
                bodega_destino = bodega,
                nit = order.tercero,
                orden_taller = order.id,
                impoconsumo = 0,
                userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
            };

            context.encab_documento.Add(crearEncabezado);

            if (grupo != null)
            {
                bool guardar = context.SaveChanges() > 0;

                encab_documento encabezado = crearEncabezado;

                int id_encabezado = crearEncabezado.idencabezado;
                int li = 1;
                //creo las referencias para agregar (primero las que sean de esta bodega
                if (listaSolicita != null)
                {
                    foreach (referenciasSolicita item in listaSolicita)
                    {
                        lineas_documento crearLineasOrigen = new lineas_documento
                        {
                            codigo = !string.IsNullOrWhiteSpace(item.id_reemplazo)
                                ? item.id_reemplazo
                                : item.id_referencia,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            nit = order.tercero,
                            cantidad = item.cantidad,
                            bodega = bodega,
                            seq = li,
                            estado = true,
                            fec = DateTime.Now,
                            costo_unitario = traercosto(!string.IsNullOrWhiteSpace(item.id_reemplazo)
                                ? item.id_reemplazo
                                : item.id_referencia),
                            id_encabezado = encabezado != null ? encabezado.idencabezado : 0,
                            codigo_reemplazo = !string.IsNullOrWhiteSpace(item.id_reemplazo) ? item.id_referencia : null,
                            comprometido = true,
                        };

                        li++;
                        //actualizo la referencia de solicitud y la marco como actualizado
                        tsolicitudrepuestosot actualizarSolicitud = context.tsolicitudrepuestosot.FirstOrDefault(x =>
                            x.idorden == orden && x.idrepuesto == item.id_referencia &&
                            x.id == item.iddetallesolicitud);
                        if (actualizarSolicitud != null)
                        {
                            crearLineasOrigen.id_solicitud_repuesto = actualizarSolicitud.id;
                            if (!string.IsNullOrWhiteSpace(item.id_reemplazo))
                            {
                                crearLineasOrigen.codigo_reemplazo = item.id_reemplazo;
                                actualizarSolicitud.reemplazo = item.id_reemplazo;
                            }

                            actualizarSolicitud.comprometido = true;
                            actualizarSolicitud.pedido = true;
                            context.Entry(actualizarSolicitud).State = EntityState.Modified;
                        }

                        context.lineas_documento.Add(crearLineasOrigen);
                    }
                }

                int guardad = context.SaveChanges();
                doc.ActualizarConsecutivo(grupo.grupo, consecutivo);
                context.SaveChanges();
                return 1;
            }

            return 0;
        }

        public int realizarSalidaTraslado(int orden, List<referenciasTraslada> listaTraslada, int bodega, int bodegaorigen)
        {
            //busco la orden
            tencabezaorden order = context.tencabezaorden.Where(d => d.id == orden).FirstOrDefault();

            //traigo el código de tipo documento salida por traslado
            icb_sysparameter parped2 = context.icb_sysparameter.Where(d => d.syspar_cod == "P159").FirstOrDefault();
            int pedidorep2 = parped2 != null ? Convert.ToInt32(parped2.syspar_value) : 3091;
            //busco el id del tipo de documento
            tp_doc_registros conse2 = context.tp_doc_registros.Where(d => d.tpdoc_id == pedidorep2).FirstOrDefault();

            //busco el consecutivo
            grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                x.documento_id == conse2.tpdoc_id && x.bodega_id == bodega);
            //creo el encabezado de la solicitud
            DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
            long consecutivo = doc.BuscarConsecutivo(grupo.grupo);

            //crear salida traslado
            encab_documento crearEncabezado2 = new encab_documento
            {
                fecha = DateTime.Now,
                fec_creacion = DateTime.Now,
                tipo = conse2.tpdoc_id,
                bodega = listaTraslada != null ? bodegaorigen : bodega,
                numero = consecutivo,
                valor_total = 0,
                valor_aplicado = 0,
                valor_mercancia = 0,
                iva = 0,
                notas = "",
                vendedor = Convert.ToInt32(Session["user_usuarioid"]),
                costo = 0,
                bodega_destino = bodega,
                nit = order.tercero,
                orden_taller = order.id,
                impoconsumo = 0,
                userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
            };

            context.encab_documento.Add(crearEncabezado2);

            if (grupo != null)
            {
                bool guardar = context.SaveChanges() > 0;

                encab_documento encabezado2 = crearEncabezado2;
                int id_encabezado = crearEncabezado2.idencabezado;
                int li = 1;

                //creo las referencias para agregar (primero las que sean de esta bodega
                if (listaTraslada != null)
                {
                    foreach (referenciasTraslada item in listaTraslada)
                    {
                        lineas_documento crearLineasOrigen = new lineas_documento
                        {
                            codigo = item.id_referencia,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            nit = order.tercero,
                            cantidad = item.cantidad,
                            bodega = bodegaorigen,
                            seq = li,
                            estado = true,
                            fec = DateTime.Now,
                            costo_unitario = 0,
                            id_encabezado = encabezado2 != null ? encabezado2.idencabezado : 0,
                            id_solicitud_repuesto = item.iddetallesolicitud,
                            comprometido = false,
                        };
                        li++;
                        //actualizo la referencia de solicitud y la marco como actualizado
                        tsolicitudrepuestosot actualizarSolicitud = context.tsolicitudrepuestosot.FirstOrDefault(x =>
                            x.idorden == orden && x.idrepuesto == item.id_referencia &&
                            x.id == item.iddetallesolicitud);
                        if (actualizarSolicitud != null)
                        {
                            actualizarSolicitud.pedido = true;
                            actualizarSolicitud.comprometido = false;
                            crearLineasOrigen.id_solicitud_repuesto = actualizarSolicitud.id;
                            context.Entry(actualizarSolicitud).State = EntityState.Modified;
                        }

                        context.lineas_documento.Add(crearLineasOrigen);
                    }
                }

                int guardad = context.SaveChanges();
                doc.ActualizarConsecutivo(grupo.grupo, consecutivo);
                context.SaveChanges();
                return 1;
            }

            return 0;
        }

        [HttpPost]
        public ActionResult trasladoRepuestos(TrasladoModel modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                {
                    try
                    {
                        int funciono = 0;
                        decimal totalCreditos = 0;
                        decimal totalDebitos = 0;

                        var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                                          join nombreParametro in context.paramcontablenombres
                                                              on perfil.id_nombre_parametro equals nombreParametro.id
                                                          join cuenta in context.cuenta_puc
                                                              on perfil.cuenta equals cuenta.cntpuc_id
                                                          where perfil.id_perfil == modelo.PerfilContable
                                                          select new
                                                          {
                                                              perfil.id,
                                                              perfil.id_nombre_parametro,
                                                              perfil.cuenta,
                                                              perfil.centro,
                                                              perfil.id_perfil,
                                                              nombreParametro.descripcion_parametro,
                                                              cuenta.cntpuc_numero
                                                          }).ToList();

                        int secuencia = 1;
                        int secuenciaLineas = 1;
                        List<cuentas_valores> ids_cuentas_valores = new List<cuentas_valores>();
                        centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                        int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;

                        List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();

                        string lista = Request["lista_referencias"];
                        if (!string.IsNullOrEmpty(lista))
                        {
                            int datos = Convert.ToInt32(lista);
                            decimal costoTotal = Convert.ToDecimal(Request["valorFinal"], miCultura); //costo con retenciones y fletes
                            decimal ivaEncabezado = Convert.ToDecimal(Request["valorIVA"], miCultura); //valor total del iva
                            decimal descuentoEncabezado =
                                Convert.ToDecimal(Request["valorDes"], miCultura); //valor total del descuento
                            decimal costoEncabezado = Convert.ToDecimal(Request["valorSub"], miCultura); //valor antes de impuestos

                            decimal valor_totalenca = costoEncabezado - descuentoEncabezado;

                            //consecutivo
                            grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                                x.documento_id == modelo.TipoDocumento && x.bodega_id == modelo.BodegaOrigen);
                            if (grupo != null)
                            {
                                decimal promedio = 0;
                                decimal costoPromedioTotal = 0;

                                List<string> listaReferencias = new List<string>();
                                int numero_repuestos = Convert.ToInt32(Request["lista_referencias"]);
                                int sequ = 1;
                                List<string> codigosReferencias = new List<string>();
                                for (int i = 0; i <= numero_repuestos; i++)
                                {
                                    string referencia_codigo = Request["referencia" + i];
                                    if (!string.IsNullOrEmpty(referencia_codigo))
                                    {
                                        codigosReferencias.Add(referencia_codigo);
                                        vw_promedio buscarPromedio = context.vw_promedio.FirstOrDefault(x =>
                                            x.codigo == referencia_codigo && x.ano == DateTime.Now.Year &&
                                            x.mes == DateTime.Now.Month);
                                        promedio = buscarPromedio != null ? buscarPromedio.Promedio ?? 0 : 0;
                                        costoPromedioTotal +=
                                            promedio * Convert.ToDecimal(Request["cantidadReferencia" + i], miCultura);
                                    }
                                }

                                int buscarCantidadRefEnOrigen = (from referencia in context.referencias_inven
                                                                 where referencia.ano == DateTime.Now.Year
                                                                       && referencia.mes == DateTime.Now.Month &&
                                                                       referencia.bodega == modelo.BodegaOrigen
                                                                       && codigosReferencias.Contains(referencia.codigo)
                                                                 select referencia.codigo).Count();

                                // Significa que al menos una referencia se encuentra en referecias inven en la bodega de origen
                                // Se supone que si existe al menos una que este en el origen se hace el proceso
                                if (buscarCantidadRefEnOrigen > 0)
                                {
                                    DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                                    long consecutivo = doc.BuscarConsecutivo(grupo.grupo);

                                    icb_sysparameter buscarNit = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P33");
                                    int nitTraslado = buscarNit != null ? Convert.ToInt32(buscarNit.syspar_value) : 0;

                                    //Encabezado documento
                                    string referenciasNoProcesadas =
                                        ", los siguientes codigos no se trasladaron por error en inventario";

                                    #region Encabezado documento

                                    encab_documento crearEncabezado = new encab_documento
                                    {
                                        notas = modelo.Notas,
                                        tipo = modelo.TipoDocumento,
                                        bodega = modelo.BodegaOrigen,
                                        numero = consecutivo,
                                        valor_total = costoTotal,
                                        valor_aplicado = costoTotal,
                                        valor_mercancia = valor_totalenca,
                                        iva = ivaEncabezado,
                                        vendedor = Convert.ToInt32(Session["user_usuarioid"]),
                                        costo = costoPromedioTotal,
                                        fecha = DateTime.Now,
                                        fec_creacion = DateTime.Now,
                                        impoconsumo = 0,
                                        //crearEncabezado.destinatario = modelo.UsuarioRecepcion;
                                        bodega_destino = modelo.BodegaDestino,
                                        perfilcontable = modelo.PerfilContable,
                                        nit = nitTraslado,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                                    };
                                    context.encab_documento.Add(crearEncabezado);

                                    bool guardar = context.SaveChanges() > 0;
                                    encab_documento encabezado = context.encab_documento.OrderByDescending(x => x.idencabezado)
                                        .FirstOrDefault();

                                    #endregion

                                    int id_encabezado = context.encab_documento.OrderByDescending(x => x.idencabezado)
                                        .FirstOrDefault().idencabezado;

                                    //Lineas documento

                                    #region lineasDocumento

                                    List<mov_contable> listaMov = new List<mov_contable>();
                                    int listaLineas = Convert.ToInt32(Request["lista_referencias"]);
                                    for (int i = 0; i <= listaLineas; i++)
                                        if (!string.IsNullOrEmpty(Request["referencia" + i]))
                                        {
                                            string codigo = Request["referencia" + i];
                                            decimal porDescuento = !string.IsNullOrEmpty(Request["descuentoReferencia" + i])
                                                ? Convert.ToDecimal(Request["descuentoReferencia" + i], miCultura)
                                                : 0;
                                            decimal final = 0;
                                            decimal valorReferencia =
                                                Convert.ToDecimal(Request["valorUnitarioReferencia" + i], miCultura);
                                            decimal descontar = porDescuento / 100;
                                            decimal porIVAReferencia =
                                                Convert.ToDecimal(Request["ivaReferencia" + i], miCultura) / 100;
                                            decimal qwerty = valorReferencia * descontar;
                                            decimal qwe = qwerty % 1;
                                            if (qwe < Convert.ToDecimal(0.5, miCultura))
                                            {
                                                final = valorReferencia - Math.Round(valorReferencia * descontar);
                                            }
                                            else if (qwe >= Convert.ToDecimal(0.5, miCultura))
                                            {
                                                final = valorReferencia - Math.Ceiling(valorReferencia * descontar);
                                            }

                                            decimal baseUnitario =
                                                final * Convert.ToDecimal(Request["cantidadReferencia" + i], miCultura);
                                            decimal ivaReferencia =
                                                final * porIVAReferencia *
                                                Convert.ToDecimal(Request["cantidadReferencia" + i], miCultura);
                                            vw_promedio buscarPromedio = context.vw_promedio.FirstOrDefault(x =>
                                                x.codigo == codigo && x.ano == DateTime.Now.Year &&
                                                x.mes == DateTime.Now.Month);
                                            promedio = buscarPromedio != null ? buscarPromedio.Promedio ?? 0 : 0;
                                            decimal promedioReferencia = 0;
                                            promedioReferencia =
                                                promedio * Convert.ToDecimal(Request["cantidadReferencia" + i], miCultura);

                                            int orden = Convert.ToInt32(Request["idSolicitado"]);
                                            int idOrden = context.tencabezaorden.FirstOrDefault(x => x.numero == orden)
                                                .id;

                                            int terceroTraslado = context.tdetallerepuestosot
                                                .FirstOrDefault(x => x.idorden == idOrden).idtercero;

                                            tsolicitudrepuestosot actualizarSolicitud = context.tsolicitudrepuestosot.FirstOrDefault(x =>
                                                x.idorden == idOrden && x.idrepuesto == codigo);
                                            if (actualizarSolicitud != null)
                                            {
                                                actualizarSolicitud.canttraslado +=
                                                    Convert.ToInt32(Request["cantidadReferencia" + i]);
                                                context.Entry(actualizarSolicitud).State = EntityState.Modified;
                                            }

                                            ttrasladosorden traslados = new ttrasladosorden
                                            {
                                                idorden = idOrden,
                                                idtraslado = encabezado.idencabezado,
                                                idtercero = terceroTraslado,
                                                codigo = codigo,
                                                idtipotarifa = Convert.ToInt32(Request["tarifaReferencia" + i]),
                                                cantidad = Convert.ToInt32(Request["cantidadReferencia" + i]),
                                                preciounitario = valorReferencia,
                                                poriva = Convert.ToDecimal(Request["ivaReferencia" + i], miCultura),
                                                pordescuento = porDescuento,
                                                costopromedio = promedio,
                                                fec_creacion = DateTime.Now,
                                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                                estado = true
                                            };
                                            context.ttrasladosorden.Add(traslados);

                                            lineas_documento crearLineasOrigen = new lineas_documento
                                            {
                                                codigo = codigo,
                                                fec_creacion = DateTime.Now,
                                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                                nit = nitTraslado,
                                                cantidad = Convert.ToDecimal(Request["cantidadReferencia" + i], miCultura),
                                                bodega = modelo.BodegaOrigen,
                                                seq = secuenciaLineas,
                                                estado = true,
                                                fec = DateTime.Now,
                                                costo_unitario = final,
                                                id_encabezado = encabezado != null ? encabezado.idencabezado : 0
                                            };
                                            context.lineas_documento.Add(crearLineasOrigen);
                                            secuenciaLineas++;
                                            lineas_documento crearLineasDestino = new lineas_documento
                                            {
                                                codigo = codigo,
                                                fec_creacion = DateTime.Now,
                                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                                nit = nitTraslado,
                                                cantidad = Convert.ToDecimal(Request["cantidadReferencia" + i], miCultura),
                                                bodega = modelo.BodegaDestino,
                                                seq = secuenciaLineas,
                                                estado = true,
                                                fec = DateTime.Now,
                                                costo_unitario = final,
                                                id_encabezado = encabezado != null ? encabezado.idencabezado : 0
                                            };
                                            context.lineas_documento.Add(crearLineasDestino);
                                            sequ++;
                                            secuenciaLineas++;

                                            #endregion

                                            //Referencias Inven

                                            #region Referencias inven

                                            for (int j = 0; j <= numero_repuestos; j++)
                                            {
                                                string referencia_codigo = Request["referencia" + j];
                                                string referencia_cantidad = Request["cantidadReferencia" + j];
                                                string referencia_costo = Request["valorUnitarioReferencia" + j];
                                                if (string.IsNullOrEmpty(referencia_codigo) ||
                                                    string.IsNullOrEmpty(referencia_cantidad))
                                                {
                                                    // Significa que la agregaron y la eliminaron
                                                }
                                                else
                                                {
                                                    listaReferencias.Add(referencia_codigo);

                                                    referencias_inven buscarReferenciasInvenOrigen =
                                                        context.referencias_inven.FirstOrDefault(x =>
                                                            x.codigo == referencia_codigo &&
                                                            x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month &&
                                                            x.bodega == modelo.BodegaOrigen);
                                                    if (buscarReferenciasInvenOrigen != null)
                                                    {
                                                        buscarReferenciasInvenOrigen.can_sal +=
                                                            Convert.ToInt32(referencia_cantidad);
                                                        buscarReferenciasInvenOrigen.cos_sal +=
                                                            promedio * Convert.ToInt32(referencia_cantidad);
                                                        buscarReferenciasInvenOrigen.can_tra +=
                                                            Convert.ToInt32(referencia_cantidad);
                                                        buscarReferenciasInvenOrigen.cos_tra +=
                                                            promedio * Convert.ToInt32(referencia_cantidad);
                                                        context.Entry(buscarReferenciasInvenOrigen).State =
                                                            EntityState.Modified;

                                                        // La bodega de origen debe existir
                                                        referencias_inven buscarReferenciasInvenDestino =
                                                            context.referencias_inven.FirstOrDefault(x =>
                                                                x.codigo == referencia_codigo &&
                                                                x.ano == DateTime.Now.Year &&
                                                                x.mes == DateTime.Now.Month &&
                                                                x.bodega == modelo.BodegaDestino);
                                                        if (buscarReferenciasInvenDestino != null)
                                                        {
                                                            buscarReferenciasInvenDestino.can_ent =
                                                                buscarReferenciasInvenDestino.can_ent +
                                                                Convert.ToInt32(referencia_cantidad);
                                                            buscarReferenciasInvenDestino.cos_ent =
                                                                buscarReferenciasInvenDestino.cos_ent +
                                                                promedio * Convert.ToInt32(referencia_cantidad);
                                                            buscarReferenciasInvenDestino.can_tra =
                                                                buscarReferenciasInvenDestino.can_tra +
                                                                Convert.ToInt32(referencia_cantidad);
                                                            buscarReferenciasInvenDestino.cos_tra =
                                                                buscarReferenciasInvenDestino.cos_tra +
                                                                promedio * Convert.ToInt32(referencia_cantidad);
                                                            context.Entry(buscarReferenciasInvenDestino).State =
                                                                EntityState.Modified;
                                                        }
                                                        else
                                                        {
                                                            referencias_inven crearReferencia = new referencias_inven
                                                            {
                                                                bodega = modelo.BodegaDestino,
                                                                codigo = referencia_codigo,
                                                                ano = (short)DateTime.Now.Year,
                                                                mes = (short)DateTime.Now.Month,
                                                                can_ini = 0,
                                                                can_ent =
                                                                Convert.ToInt32(referencia_cantidad),
                                                                can_sal = 0,
                                                                cos_ent =
                                                                buscarReferenciasInvenOrigen.cos_ent +
                                                                promedio * Convert.ToInt32(referencia_cantidad),
                                                                can_tra =
                                                                Convert.ToInt32(referencia_cantidad),
                                                                cos_tra =
                                                                buscarReferenciasInvenOrigen.cos_tra +
                                                                promedio * Convert.ToInt32(referencia_cantidad),
                                                                modulo = "R"
                                                            };

                                                            context.referencias_inven.Add(crearReferencia);
                                                            context.SaveChanges();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // Validado arriba porque debe haber referencias inven en la bodega de origen
                                                        referenciasNoProcesadas += ", " + referencia_codigo;
                                                    }
                                                }
                                            }

                                            #endregion

                                            //Mov Contable (Inventario (credito y debito) y cuentas valores)

                                            #region Mov Contable (Inventario (credito y debito) y cuentas valores

                                            foreach (var parametro in parametrosCuentasVerificar)
                                            {
                                                string descripcionParametro = context.paramcontablenombres
                                                    .FirstOrDefault(x => x.id == parametro.id_nombre_parametro)
                                                    .descripcion_parametro;
                                                cuenta_puc buscarCuenta =
                                                    context.cuenta_puc.FirstOrDefault(x =>
                                                        x.cntpuc_id == parametro.cuenta);

                                                if (buscarCuenta != null)
                                                {
                                                    if (parametro.id_nombre_parametro == 9 &&
                                                        Convert.ToDecimal(costoPromedioTotal, miCultura) != 0
                                                        || parametro.id_nombre_parametro == 20 &&
                                                        Convert.ToDecimal(costoPromedioTotal, miCultura) != 0)
                                                    {
                                                        mov_contable movNuevo = new mov_contable
                                                        {
                                                            id_encab = encabezado.idencabezado,
                                                            seq = secuencia,
                                                            idparametronombre = parametro.id_nombre_parametro,
                                                            cuenta = parametro.cuenta,
                                                            centro = parametro.centro,
                                                            fec = DateTime.Now,
                                                            fec_creacion = DateTime.Now,
                                                            userid_creacion =
                                                            Convert.ToInt32(Session["user_usuarioid"])
                                                        };

                                                        cuenta_puc info = context.cuenta_puc
                                                            .Where(a => a.cntpuc_id == parametro.cuenta)
                                                            .FirstOrDefault();

                                                        if (info.tercero)
                                                        {
                                                            movNuevo.nit = nitTraslado;
                                                        }
                                                        else
                                                        {
                                                            icb_terceros tercero = context.icb_terceros
                                                                .Where(t => t.doc_tercero == "0").FirstOrDefault();
                                                            movNuevo.nit = tercero.tercero_id;
                                                        }

                                                        #region Inventario Debito

                                                        if (parametro.id_nombre_parametro == 9)
                                                        {
                                                            icb_referencia perfilReferencia =
                                                                context.icb_referencia.FirstOrDefault(x =>
                                                                    x.ref_codigo == crearLineasOrigen.codigo);
                                                            int perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                                            perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(
                                                                r => r.id == perfilBuscar);

                                                            #region Tiene perfil la referencia

                                                            if (pcr != null)
                                                            {
                                                                int? cuentaIva = pcr.cuenta_iva_compras;
                                                                movNuevo.id_encab = encabezado.idencabezado;
                                                                movNuevo.seq = secuencia;
                                                                movNuevo.idparametronombre =
                                                                    parametro.id_nombre_parametro;

                                                                #region si tiene perfil y cuenta asignada a ese perfil

                                                                if (cuentaIva != null)
                                                                {
                                                                    movNuevo.cuenta = Convert.ToInt32(cuentaIva);
                                                                    movNuevo.centro = parametro.centro;
                                                                    movNuevo.fec = DateTime.Now;
                                                                    movNuevo.fec_creacion = DateTime.Now;
                                                                    movNuevo.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);

                                                                    cuenta_puc infoReferencia = context.cuenta_puc
                                                                        .Where(a => a.cntpuc_id == cuentaIva)
                                                                        .FirstOrDefault();
                                                                    if (infoReferencia.manejabase)
                                                                    {
                                                                        movNuevo.basecontable = promedioReferencia;
                                                                    }
                                                                    else
                                                                    {
                                                                        movNuevo.basecontable = 0;
                                                                    }

                                                                    if (infoReferencia.documeto)
                                                                    {
                                                                        movNuevo.documento =
                                                                            Convert.ToString(consecutivo);
                                                                    }

                                                                    if (infoReferencia.concepniff == 1)
                                                                    {
                                                                        movNuevo.credito = 0;
                                                                        movNuevo.debito = promedioReferencia;

                                                                        movNuevo.creditoniif = 0;
                                                                        movNuevo.debitoniif = promedioReferencia;
                                                                    }

                                                                    if (infoReferencia.concepniff == 4)
                                                                    {
                                                                        movNuevo.creditoniif = 0;
                                                                        movNuevo.debitoniif = promedioReferencia;
                                                                    }

                                                                    if (infoReferencia.concepniff == 5)
                                                                    {
                                                                        movNuevo.credito = 0;
                                                                        movNuevo.debito = promedioReferencia;
                                                                    }

                                                                    //context.mov_contable.Add(movNuevo);
                                                                }

                                                                #endregion

                                                                #region si tiene perfil pero no tiene cuenta asignada

                                                                else
                                                                {
                                                                    movNuevo.cuenta = parametro.cuenta;
                                                                    movNuevo.centro = parametro.centro;
                                                                    movNuevo.fec = DateTime.Now;
                                                                    movNuevo.fec_creacion = DateTime.Now;
                                                                    movNuevo.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);

                                                                    cuenta_puc infoReferencia = context.cuenta_puc
                                                                        .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                        .FirstOrDefault();
                                                                    if (infoReferencia.manejabase)
                                                                    {
                                                                        movNuevo.basecontable = promedioReferencia;
                                                                    }
                                                                    else
                                                                    {
                                                                        movNuevo.basecontable = 0;
                                                                    }

                                                                    if (infoReferencia.documeto)
                                                                    {
                                                                        movNuevo.documento =
                                                                            Convert.ToString(consecutivo);
                                                                    }

                                                                    if (infoReferencia.concepniff == 1)
                                                                    {
                                                                        movNuevo.credito = 0;
                                                                        movNuevo.debito = promedioReferencia;

                                                                        movNuevo.creditoniif = 0;
                                                                        movNuevo.debitoniif = promedioReferencia;
                                                                    }

                                                                    if (infoReferencia.concepniff == 4)
                                                                    {
                                                                        movNuevo.creditoniif = 0;
                                                                        movNuevo.debitoniif = promedioReferencia;
                                                                    }

                                                                    if (infoReferencia.concepniff == 5)
                                                                    {
                                                                        movNuevo.credito = 0;
                                                                        movNuevo.debito = promedioReferencia;
                                                                    }

                                                                    //context.mov_contable.Add(movNuevo);
                                                                }

                                                                #endregion
                                                            }

                                                            #endregion

                                                            #region No tiene perfil la referencia

                                                            else
                                                            {
                                                                movNuevo.id_encab = encabezado.idencabezado;
                                                                movNuevo.seq = secuencia;
                                                                movNuevo.idparametronombre =
                                                                    parametro.id_nombre_parametro;
                                                                movNuevo.cuenta = parametro.cuenta;
                                                                movNuevo.centro = parametro.centro;
                                                                movNuevo.fec = DateTime.Now;
                                                                movNuevo.fec_creacion = DateTime.Now;
                                                                movNuevo.userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]);
                                                                /*if (info.aplicaniff==true)
																{

																}*/

                                                                if (info.manejabase)
                                                                {
                                                                    movNuevo.basecontable = promedioReferencia;
                                                                }
                                                                else
                                                                {
                                                                    movNuevo.basecontable = 0;
                                                                }

                                                                if (info.documeto)
                                                                {
                                                                    movNuevo.documento = Convert.ToString(consecutivo);
                                                                }

                                                                if (buscarCuenta.concepniff == 1)
                                                                {
                                                                    movNuevo.credito = 0;
                                                                    movNuevo.debito = promedioReferencia;

                                                                    movNuevo.creditoniif = 0;
                                                                    movNuevo.debitoniif = promedioReferencia;
                                                                }

                                                                if (buscarCuenta.concepniff == 4)
                                                                {
                                                                    movNuevo.creditoniif = 0;
                                                                    movNuevo.debitoniif = promedioReferencia;
                                                                }

                                                                if (buscarCuenta.concepniff == 5)
                                                                {
                                                                    movNuevo.credito = 0;
                                                                    movNuevo.debito = promedioReferencia;
                                                                }

                                                                //context.mov_contable.Add(movNuevo);
                                                            }

                                                            #endregion

                                                            #region Buscamos si ya esta o si creamos el registo en mov contable

                                                            mov_contable buscarIVA = context.mov_contable.FirstOrDefault(x =>
                                                                x.id_encab == id_encabezado &&
                                                                x.cuenta == movNuevo.cuenta &&
                                                                x.idparametronombre == parametro.id_nombre_parametro);
                                                            if (buscarIVA != null)
                                                            {
                                                                buscarIVA.debito += movNuevo.debito;
                                                                buscarIVA.debitoniif += movNuevo.debitoniif;
                                                                buscarIVA.credito += movNuevo.credito;
                                                                buscarIVA.creditoniif += movNuevo.creditoniif;
                                                                buscarIVA.basecontable += movNuevo.basecontable;
                                                                context.Entry(buscarIVA).State = EntityState.Modified;
                                                                context.SaveChanges();
                                                            }
                                                            else
                                                            {
                                                                mov_contable crearMovContable = new mov_contable
                                                                {
                                                                    id_encab = encabezado.idencabezado,
                                                                    seq = secuencia,
                                                                    idparametronombre =
                                                                    parametro.id_nombre_parametro,
                                                                    cuenta =
                                                                    Convert.ToInt32(movNuevo.cuenta),
                                                                    centro = parametro.centro,
                                                                    nit = encabezado.nit,
                                                                    fec = DateTime.Now,
                                                                    debito = movNuevo.debito,
                                                                    debitoniif = movNuevo.debitoniif,
                                                                    basecontable = movNuevo.basecontable,
                                                                    credito = movNuevo.credito,
                                                                    creditoniif = movNuevo.creditoniif,
                                                                    fec_creacion = DateTime.Now,
                                                                    userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]),
                                                                    detalle =
                                                                    "Traslado de repuestos por orden de taller con consecutivo " +
                                                                    consecutivo,
                                                                    estado = true
                                                                };
                                                                context.mov_contable.Add(crearMovContable);
                                                                context.SaveChanges();
                                                            }

                                                            #endregion
                                                        }

                                                        #endregion

                                                        #region Inventario Credito			

                                                        if (parametro.id_nombre_parametro == 20)
                                                        {
                                                            icb_referencia perfilReferencia =
                                                                context.icb_referencia.FirstOrDefault(x =>
                                                                    x.ref_codigo == crearLineasOrigen.codigo);
                                                            int perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                                            perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(
                                                                r => r.id == perfilBuscar);

                                                            #region Tiene perfil la referencia

                                                            if (pcr != null)
                                                            {
                                                                int? cuentaInven = pcr.cuenta_inv_compra;
                                                                movNuevo.id_encab = encabezado.idencabezado;
                                                                movNuevo.seq = secuencia;
                                                                movNuevo.idparametronombre =
                                                                    parametro.id_nombre_parametro;

                                                                #region Si tiene perfil y cuenta asignada a ese perfil

                                                                if (cuentaInven != null)
                                                                {
                                                                    movNuevo.cuenta = Convert.ToInt32(cuentaInven);
                                                                    movNuevo.centro = parametro.centro;
                                                                    movNuevo.fec = DateTime.Now;
                                                                    movNuevo.fec_creacion = DateTime.Now;
                                                                    movNuevo.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);

                                                                    cuenta_puc infoReferencias = context.cuenta_puc
                                                                        .Where(a => a.cntpuc_id == cuentaInven)
                                                                        .FirstOrDefault();
                                                                    if (infoReferencias.manejabase)
                                                                    {
                                                                        movNuevo.basecontable = promedioReferencia;
                                                                    }
                                                                    else
                                                                    {
                                                                        movNuevo.basecontable = 0;
                                                                    }

                                                                    if (infoReferencias.documeto)
                                                                    {
                                                                        movNuevo.documento =
                                                                            Convert.ToString(consecutivo);
                                                                    }

                                                                    if (infoReferencias.concepniff == 1)
                                                                    {
                                                                        movNuevo.credito = promedioReferencia;
                                                                        movNuevo.debito = 0;

                                                                        movNuevo.creditoniif = promedioReferencia;
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (infoReferencias.concepniff == 4)
                                                                    {
                                                                        movNuevo.creditoniif = promedioReferencia;
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (infoReferencias.concepniff == 5)
                                                                    {
                                                                        movNuevo.credito = promedioReferencia;
                                                                        movNuevo.debito = 0;
                                                                    }

                                                                    //context.mov_contable.Add(movNuevo);
                                                                }

                                                                #endregion

                                                                #region Si tiene perfil pero no tiene cuenta asignada

                                                                else
                                                                {
                                                                    movNuevo.cuenta = parametro.cuenta;
                                                                    movNuevo.centro = parametro.centro;
                                                                    movNuevo.fec = DateTime.Now;
                                                                    movNuevo.fec_creacion = DateTime.Now;
                                                                    movNuevo.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);

                                                                    cuenta_puc infoReferencia = context.cuenta_puc
                                                                        .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                        .FirstOrDefault();
                                                                    if (infoReferencia.manejabase)
                                                                    {
                                                                        movNuevo.basecontable = promedioReferencia;
                                                                    }
                                                                    else
                                                                    {
                                                                        movNuevo.basecontable = 0;
                                                                    }

                                                                    if (infoReferencia.documeto)
                                                                    {
                                                                        movNuevo.documento =
                                                                            Convert.ToString(consecutivo);
                                                                    }

                                                                    if (infoReferencia.concepniff == 1)
                                                                    {
                                                                        movNuevo.debito = 0;
                                                                        movNuevo.credito = promedioReferencia;

                                                                        movNuevo.debitoniif = 0;
                                                                        movNuevo.creditoniif = promedioReferencia;
                                                                    }

                                                                    if (infoReferencia.concepniff == 4)
                                                                    {
                                                                        movNuevo.debitoniif = 0;
                                                                        movNuevo.creditoniif = promedioReferencia;
                                                                    }

                                                                    if (infoReferencia.concepniff == 5)
                                                                    {
                                                                        movNuevo.debito = 0;
                                                                        movNuevo.credito = promedioReferencia;
                                                                    }

                                                                    //context.mov_contable.Add(movNuevo);

                                                                    #endregion
                                                                }
                                                            }

                                                            #endregion

                                                            #region No tiene prefil la referencia

                                                            else
                                                            {
                                                                movNuevo.id_encab = encabezado.idencabezado;
                                                                movNuevo.seq = secuencia;
                                                                movNuevo.idparametronombre =
                                                                    parametro.id_nombre_parametro;
                                                                movNuevo.cuenta = parametro.cuenta;
                                                                movNuevo.centro = parametro.centro;
                                                                movNuevo.fec = DateTime.Now;
                                                                movNuevo.fec_creacion = DateTime.Now;
                                                                movNuevo.userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]);
                                                                /*if (info.aplicaniff==true)
																{

																}*/

                                                                if (info.manejabase)
                                                                {
                                                                    movNuevo.basecontable = promedioReferencia;
                                                                }
                                                                else
                                                                {
                                                                    movNuevo.basecontable = 0;
                                                                }

                                                                if (info.documeto)
                                                                {
                                                                    movNuevo.documento = Convert.ToString(consecutivo);
                                                                }

                                                                if (buscarCuenta.concepniff == 1)
                                                                {
                                                                    movNuevo.debito = 0;
                                                                    movNuevo.credito = promedioReferencia;

                                                                    movNuevo.debitoniif = 0;
                                                                    movNuevo.creditoniif = promedioReferencia;
                                                                }

                                                                if (buscarCuenta.concepniff == 4)
                                                                {
                                                                    movNuevo.debitoniif = 0;
                                                                    movNuevo.creditoniif = promedioReferencia;
                                                                }

                                                                if (buscarCuenta.concepniff == 5)
                                                                {
                                                                    movNuevo.debito = 0;
                                                                    movNuevo.credito = promedioReferencia;
                                                                }

                                                                //context.mov_contable.Add(movNuevo);
                                                            }

                                                            #endregion

                                                            #region Buscamos si ya esta o si creamos el registro en mov contable

                                                            mov_contable buscarInventario =
                                                                context.mov_contable.FirstOrDefault(x =>
                                                                    x.id_encab == id_encabezado &&
                                                                    x.cuenta == movNuevo.cuenta &&
                                                                    x.idparametronombre ==
                                                                    parametro.id_nombre_parametro);
                                                            if (buscarInventario != null)
                                                            {
                                                                buscarInventario.basecontable += movNuevo.basecontable;
                                                                buscarInventario.debito += movNuevo.debito;
                                                                buscarInventario.debitoniif += movNuevo.debitoniif;
                                                                buscarInventario.credito += movNuevo.credito;
                                                                buscarInventario.creditoniif += movNuevo.creditoniif;
                                                                context.Entry(buscarInventario).State =
                                                                    EntityState.Modified;
                                                                context.SaveChanges();
                                                            }
                                                            else
                                                            {
                                                                mov_contable crearMovContable = new mov_contable
                                                                {
                                                                    id_encab = encabezado.idencabezado,
                                                                    seq = secuencia,
                                                                    idparametronombre =
                                                                    parametro.id_nombre_parametro,
                                                                    cuenta =
                                                                    Convert.ToInt32(movNuevo.cuenta),
                                                                    centro = parametro.centro,
                                                                    nit = encabezado.nit,
                                                                    fec = DateTime.Now,
                                                                    debito = movNuevo.debito,
                                                                    debitoniif = movNuevo.debitoniif,
                                                                    basecontable = movNuevo.basecontable,
                                                                    credito = movNuevo.credito,
                                                                    creditoniif = movNuevo.creditoniif,
                                                                    fec_creacion = DateTime.Now,
                                                                    userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]),
                                                                    detalle =
                                                                    "Traslado de repuestos por orden de taller con consecutivo " +
                                                                    consecutivo,
                                                                    estado = true
                                                                };
                                                                context.mov_contable.Add(crearMovContable);
                                                                context.SaveChanges();
                                                            }

                                                            #endregion
                                                        }

                                                        #endregion

                                                        secuencia++;
                                                        //Cuentas valores

                                                        #region Cuentas valores

                                                        DateTime fechaHoy = DateTime.Now;
                                                        cuentas_valores buscar_cuentas_valores =
                                                            context.cuentas_valores.FirstOrDefault(x =>
                                                                x.centro == parametro.centro &&
                                                                x.cuenta == movNuevo.cuenta && x.nit == movNuevo.nit &&
                                                                x.ano == fechaHoy.Year && x.mes == fechaHoy.Month);
                                                        if (buscar_cuentas_valores != null)
                                                        {
                                                            buscar_cuentas_valores.debito += movNuevo.debito;
                                                            buscar_cuentas_valores.credito += movNuevo.credito;
                                                            buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                                                            buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                                                            context.Entry(buscar_cuentas_valores).State =
                                                                EntityState.Modified;
                                                            context.SaveChanges();
                                                        }
                                                        else
                                                        {
                                                            cuentas_valores crearCuentaValor = new cuentas_valores
                                                            {
                                                                ano = fechaHoy.Year,
                                                                mes = fechaHoy.Month,
                                                                cuenta = movNuevo.cuenta,
                                                                centro = movNuevo.centro,
                                                                nit = movNuevo.nit,
                                                                debito = movNuevo.debito,
                                                                credito = movNuevo.credito,
                                                                debitoniff = movNuevo.debitoniif,
                                                                creditoniff = movNuevo.creditoniif
                                                            };
                                                            context.cuentas_valores.Add(crearCuentaValor);
                                                            context.SaveChanges();
                                                        }

                                                        #endregion

                                                        totalCreditos += movNuevo.credito;
                                                        totalDebitos += movNuevo.debito;
                                                        listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                        {
                                                            NumeroCuenta =
                                                                "(" + buscarCuenta.cntpuc_numero + ")" +
                                                                buscarCuenta.cntpuc_descp,
                                                            DescripcionParametro = descripcionParametro,
                                                            ValorDebito = movNuevo.debito,
                                                            ValorCredito = movNuevo.credito
                                                        });
                                                    }
                                                }
                                            }

                                            #endregion
                                        }

                                    #region validaciones para guardar

                                    if (Math.Round(totalDebitos) != Math.Round(totalCreditos))
                                    {
                                        TempData["documento_descuadrado"] =
                                            "El documento no tiene los movimientos calculados correctamente, verifique el perfil del documento";

                                        dbTran.Rollback();
                                        listas();
                                        BuscarFavoritos(menu);
                                        return RedirectToAction("Index");
                                    }

                                    funciono = 1;
                                    if (funciono > 0)
                                    {
                                        TempData["mensaje"] = "registro creado correctamente";
                                        DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
                                        doc.ActualizarConsecutivo(grupo.grupo, consecutivo);
                                        context.SaveChanges();
                                        dbTran.Commit();
                                        listas();
                                        return RedirectToAction("Index");
                                    }

                                    #endregion
                                }
                                else
                                {
                                    // Significa que ninguna referencia de las agregadas se encuentra en la tabla referencias inven en la bodega de origen, al menos debe haber una
                                    // pero por no haber ninguna entonces nse guarda ni encabezado ni nada de lo demas.
                                    TempData["mensaje_error"] =
                                        "Referencia(s) no valida(s) en inventario, realice recalculo de la(s) misma(s)";
                                    listas();
                                    BuscarFavoritos(menu);
                                    return View(modelo);
                                }
                            }
                            else
                            {
                                TempData["mensaje_error"] = "no hay consecutivo";
                            }
                        }
                        //cierre
                        else
                        {
                            TempData["mensaje_error"] = "Lista vacia";
                        }
                    }
                    catch (DbEntityValidationException)
                    {
                        dbTran.Rollback();
                        throw;
                    }
                }
            }
            else
            {
                List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();
            }

            int bodegaLog = Convert.ToInt32(Session["user_bodega"]);
            string bodegaNombre = context.bodega_concesionario.FirstOrDefault(x => x.id == bodegaLog).bodccs_nombre;

            ViewBag.bodegaOrigen = bodegaLog;
            ViewBag.bodegaOrigenNombre = bodegaNombre;

            ViewBag.idsolicitado = Convert.ToInt32(Request["idSolicitado"]);
            listas();
            ViewBag.tipoTarifa = context.ttipostarifa.Where(x => x.estado);
            BuscarFavoritos(menu);
            return View(modelo);
        }

        public void listas()
        {
            icb_sysparameter tipot = context.icb_sysparameter.Where(d => d.syspar_cod == "P98").FirstOrDefault();
            int tipo = tipot != null ? Convert.ToInt32(tipot.syspar_value) : 36;
            ViewBag.doc_registros = context.tp_doc_registros.Where(x => x.tipo == tipo);

            var provedores = from pro in context.tercero_cliente
                             join ter in context.icb_terceros
                                 on pro.tercero_id equals ter.tercero_id
                             select new
                             {
                                 idTercero = ter.tercero_id,
                                 nombreTErcero = ter.prinom_tercero + " " + ter.segnom_tercero,
                                 apellidosTercero = ter.apellido_tercero + " " + ter.segapellido_tercero,
                                 razonSocial = ter.razon_social
                             };
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in provedores)
            {
                string nombre = item.nombreTErcero + " " + item.apellidosTercero + " " + item.razonSocial;
                items.Add(new SelectListItem { Text = nombre, Value = item.idTercero.ToString() });
            }

            ViewBag.cliente = items;

            ViewBag.codigo = context.icb_referencia.Where(x => x.modulo == "R" && x.ref_estado).ToList();
            ViewBag.bodegaLog = Convert.ToInt32(Session["user_bodega"]);
        }

        public JsonResult buscarRepuestosSolicitados(int idORden, int tarifa)
        {
            int id = context.tencabezaorden.FirstOrDefault(x => x.id == idORden).id;

            var repuestosSolicitados = (from ts in context.tsolicitudrepuestosot
                                        join te in context.tencabezaorden
                                            on ts.idorden equals te.id
                                        join tr in context.tdetallerepuestosot
                                            on new { a = te.id, b = ts.iddetalle ?? 0 } equals new { a = tr.idorden, b = tr.id } into temp
                                        from tr in temp.DefaultIfEmpty()
                                        join r in context.icb_referencia
                                            on ts.idrepuesto equals r.ref_codigo
                                        where ts.idorden == id && ts.recibido
                                        //&& tr.tipotarifa == tarifa
                                        select new
                                        {
                                            idRS = tr.id,
                                            nOrden = te.numero,
                                            idTarifa = tr.tipotarifa,
                                            r.ref_codigo,
                                            repuesto = "(" + r.ref_codigo + ") - " + r.ref_descripcion,
                                            cantidad = ts.cantidad - ts.canttraslado,
                                            porIVA = tr.poriva,
                                            porDes = tr.pordescto,
                                            valorUnitario = ts.valor
                                        }).ToList();

            var data = repuestosSolicitados.Select(x => new
            {
                x.idRS,
                x.nOrden,
                x.idTarifa,
                x.ref_codigo,
                x.repuesto,
                x.cantidad,
                x.porIVA,
                x.porDes,
                x.valorUnitario
            });

            //var terceroTarifa = (from tr in context.tdetallerepuestosot
            //                     join t in context.icb_terceros
            //                     on tr.idtercero equals t.tercero_id
            //                     where tr.tipotarifa == tarifa && tr.idorden == id
            //                     select new
            //                     {
            //                         idTer = t.tercero_id,
            //                         nombre = t.doc_tercero + " - " + t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero + " " + t.segapellido_tercero + " " + t.razon_social
            //                     }).FirstOrDefault();

            var terceroTarifa = (from t in context.tencabezaorden
                                 where t.id == id
                                 select new
                                 {
                                     idTer = t.icb_terceros.tercero_id,
                                     nombre = t.icb_terceros.doc_tercero + " - " + t.icb_terceros.prinom_tercero + " " +
                                              t.icb_terceros.segnom_tercero + " " + t.icb_terceros.apellido_tercero + " " +
                                              t.icb_terceros.segapellido_tercero + " " + t.icb_terceros.razon_social
                                 }).FirstOrDefault();

            return Json(new { data, terceroTarifa }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarRepuestosSolicitadosTraslado(int idPedido, int idbodega)
        {
            encab_documento idorden = context.encab_documento.Where(x => x.idencabezado == idPedido).FirstOrDefault();

            List<lineas_documento> repuestosSolicitados2 = context.lineas_documento
                .Where(d => d.id_encabezado == idorden.idencabezado && d.bodega == idbodega).ToList();

            List<recibidotraslados> repuestostraslado = context.recibidotraslados
                .Where(d => d.idtraslado == idPedido && d.iddestino == idbodega).ToList();
            var repuestosSolicitados = repuestosSolicitados2.Select(d => new
            {
                idRS = d.id_solicitud_repuesto != null ? d.tsolicitudrepuestosot.id : 0,
                nOrden = d.encab_documento.orden_taller,
                d.icb_referencia.ref_codigo,
                repuesto = "(" + d.icb_referencia.ref_codigo + ") - " + d.icb_referencia.ref_descripcion,
                d.cantidad,
                porIVA = traeriva(d.codigo),
                porDes = traerdescuento(d.codigo),
                valorUnitario = traercosto(d.codigo),
                idsolicitudrepuesto = d.id_solicitud_repuesto
            }).ToList();

            var data = repuestosSolicitados.Select(x => new
            {
                x.idRS,
                x.nOrden,
                x.ref_codigo,
                x.repuesto,
                x.cantidad,
                x.porIVA,
                x.porDes,
                x.valorUnitario,
                x.idsolicitudrepuesto
            });


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarRepuestosSolicitadosDespacho(int idPedido)
        {
            encab_documento idorden = context.encab_documento.Where(x => x.idencabezado == idPedido).FirstOrDefault();

            List<lineas_documento> repuestosSolicitados2 =
                context.lineas_documento.Where(d => d.id_encabezado == idorden.idencabezado).ToList();

            var repuestosSolicitados = repuestosSolicitados2.Select(d => new
            {
                idRS = d.id_solicitud_repuesto != null ? d.tsolicitudrepuestosot.id : 0,
                nOrden = d.encab_documento.orden_taller,
                d.icb_referencia.ref_codigo,
                repuesto = "(" + d.icb_referencia.ref_codigo + ") - " + d.icb_referencia.ref_descripcion,
                d.cantidad,
                porIVA = d.id_solicitud_repuesto != null
                    ? float.Parse(d.tsolicitudrepuestosot.tdetallerepuestosot.poriva.ToString())
                    : d.icb_referencia.por_iva,
                porDes = d.id_solicitud_repuesto != null
                    ? float.Parse(d.tsolicitudrepuestosot.tdetallerepuestosot.pordescto.ToString())
                    : d.icb_referencia.por_dscto,
                valorUnitario = d.id_solicitud_repuesto != null
                    ? d.tsolicitudrepuestosot.tdetallerepuestosot.valorunitario
                    : d.icb_referencia.ref_valor_unitario,
                idsolicitudrepuesto = d.id_solicitud_repuesto,
                ubicacion = ubicacionRep(d.bodega, d.codigo) != "null"
                    ? ubicacionRep(d.bodega, d.codigo)
                    : "Ubicación No Asignada"
            }).ToList();

            var data = repuestosSolicitados.Select(x => new
            {
                x.idRS,
                x.nOrden,
                x.ref_codigo,
                x.repuesto,
                x.cantidad,
                x.porIVA,
                x.porDes,
                x.valorUnitario,
                x.idsolicitudrepuesto,
                x.ubicacion
            });


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public string ubicacionRep(int bodega, string codigo)
        {
            string ubicacionRep = "null";
            ubicacion_repuesto ubicacion = context.ubicacion_repuesto.Where(z => z.bodega == bodega && z.codigo == codigo)
                .FirstOrDefault();
            if (ubicacion != null)
            {
                ubicacionRep = context.ubicacion_repuestobod.Where(x => x.id == ubicacion.ubicacion).First()
                    .descripcion;
            }

            return ubicacionRep;
        }


        public JsonResult BuscarBodegasPorDocumento(int? id)
        {
            int bodegaLog = Convert.ToInt32(Session["user_bodega"]);
            var buscarBodega = (from consecutivos in context.icb_doc_consecutivos
                                join bodega in context.bodega_concesionario
                                    on consecutivos.doccons_bodega equals bodega.id
                                where consecutivos.doccons_idtpdoc == id
                                select new
                                {
                                    bodega.bodccs_nombre,
                                    bodega.id
                                }).Distinct().OrderBy(bn => bn.bodccs_nombre).ToList();

            var data = new
            {
                buscarBodega
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult cambioGenerico(int id, string repuesto, decimal valorU, int iva, int descuento, int cantidad)
        {
            tdetallerepuestosot buscarRepuesto = context.tdetallerepuestosot.FirstOrDefault(x => x.id == id);
            tsolicitudrepuestosot buscarSolicitud = context.tsolicitudrepuestosot.FirstOrDefault(x => x.iddetalle == id);

            if (buscarRepuesto != null && buscarSolicitud != null)
            {
                buscarRepuesto.idrepuesto = repuesto;
                buscarRepuesto.valorunitario = valorU;
                buscarRepuesto.pordescto = descuento;
                buscarRepuesto.poriva = iva;
                context.Entry(buscarRepuesto).State = EntityState.Modified;

                buscarSolicitud.idrepuesto = repuesto;
                buscarSolicitud.canttraslado = cantidad;
                buscarSolicitud.valor = valorU;
                context.Entry(buscarSolicitud).State = EntityState.Modified;

                context.SaveChanges();

                return Json(new { exito = true }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { exito = false }, JsonRequestBehavior.AllowGet);
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

        public class datasolicitud
        {
            public string idrepuesto { get; set; }
            public string ref_descripcion { get; set; }
            public int stockbodega { get; set; }
            public int stockotras { get; set; }
            public int stockreemplazo { get; set; }
        }

        public class datapedido
        {
            public string idrepuesto { get; set; }
            public string ref_descripcion { get; set; }
            public int stockbodega { get; set; }
            public int stockreemplazo { get; set; }
            public string ubicacion { get; set; }
        }

        public class tablasolicitud
        {
            public int idorden { get; set; }
            public int idsolicitud { get; set; }

            public string referencia { get; set; }
            public string desc_referencia { get; set; }
            public int cantidad { get; set; }
            public int idbodega { get; set; }
            public decimal stock_bodega { get; set; }
            public List<tablastock_otras_bodegas> stock_otras_bodegas { get; set; }
            public List<tablastockreemplazo> tablastockreemplazo { get; set; }
            public int cantidadComprometida { get; set; }
        }

        public class tablastockreemplazo
        {
            public string stockreemplazo { get; set; }
            public string nombrereferencia { get; set; }
        }

        public class tablastock_otras_bodegas
        {
            public string stockotrabodega { get; set; }
            public string nombrebodega { get; set; }
        }

        public class referenciasSolicita
        {
            public string id_referencia { get; set; }
            public int cantidad { get; set; }
            public int idorden { get; set; }
            public string id_reemplazo { get; set; }
            public int iddetallesolicitud { get; set; }
        }

        public class referenciasTraslada
        {
            public string id_referencia { get; set; }
            public int cantidad { get; set; }
            public int idorden { get; set; }
            public int idbodega { get; set; }
            public int idbodegarecibe { get; set; }
            public int iddetallesolicitud { get; set; }
        }
    }
}