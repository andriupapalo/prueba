using Homer_MVC.IcebergModel;
using Homer_MVC.ViewModels.medios;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class rlistaPreciosController : Controller
    {

        private readonly Iceberg_Context context = new Iceberg_Context();

        private static Expression<Func<vw_lista_referencias, string>> GetColumnName2(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(vw_lista_referencias), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<vw_lista_referencias, string>> lambda = Expression.Lambda<Func<vw_lista_referencias, string>>(menuProperty, menu);

            return lambda;
        }

        // GET: rlistaPrecios
        public ActionResult browserPrecios(int? menu)
        {
            BuscarFavoritos(menu);
            ViewBag.id_menu = menu;
            return View();
        }

        public JsonResult buscarDatos(string filtroGeneral)
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

            Expression<Func<vw_lista_referencias, bool>> predicado = PredicateBuilder.True<vw_lista_referencias>();
            Expression<Func<vw_lista_referencias, bool>> predicado2 = PredicateBuilder.False<vw_lista_referencias>();


            predicado = predicado.And(d => d.ref_estado == true && d.modulo == "R");
            if (!string.IsNullOrWhiteSpace(filtroGeneral))
            {
                predicado2 = predicado2.Or(d => 1 == 1 && d.ref_codigo.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado2 = predicado2.Or(d => 1 == 1 && d.ref_descripcion.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado2 = predicado2.Or(d => 1 == 1 && d.costo_promedio.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado2 = predicado2.Or(d => 1 == 1 && d.precio_venta.Contains(filtroGeneral.ToUpper()));

                predicado = predicado.And(predicado2);
            }

            int registrostotales = context.vw_lista_referencias.Where(predicado).Count();
            //si el ordenamiento es ascendente o descendente es distinto
            if (pageSize == -1)
            {
                pageSize = registrostotales;
            }

            if (sortColumnDir == "asc")
            {
                List<vw_lista_referencias> query2 = context.vw_lista_referencias.Where(predicado)
                    .OrderBy(GetColumnName2(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
                var query = query2.Select(d => new
                {
                    d.ref_codigo,
                    d.ref_descripcion,
                    costo_promedio = d.costo_promedio != null ? d.costo_promedio : "0",
                    d.precio_venta,
                    d.precio_iva,
                }).ToList();


                int contador = query.Count();
                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<vw_lista_referencias> query2 = context.vw_lista_referencias.Where(predicado)
                    .OrderByDescending(GetColumnName2(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();

                var query = query2.Select(d => new
                {
                    d.ref_codigo,
                    d.ref_descripcion,
                    costo_promedio = d.costo_promedio != null ? d.costo_promedio : "0",
                    d.precio_venta,
                    d.precio_iva
                }).ToList();

                int contador = query.Count();

                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult buscarStock(string cod)
        {
            List<vw_inventario_hoy> buscar = context.vw_inventario_hoy.Where(x => x.ref_codigo == cod && x.modulo == "R").ToList();

            var data = buscar.Select(z => new
            {
                bodega = z.nombreBodega,
                z.stock
            });

            return Json(data, JsonRequestBehavior.AllowGet);
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