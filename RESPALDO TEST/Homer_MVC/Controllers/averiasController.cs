using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class averiasController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        private void parametrosBusqueda()
        {
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 121);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
        }


        // GET: averias
        public ActionResult Crear(int? menu)
        {
            parametrosBusqueda();
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Crear(icb_averias postAveria, int? menu)
        {
            if (ModelState.IsValid)
            {
                icb_averias buscarNombre = context.icb_averias.FirstOrDefault(x => x.ave_codigo == postAveria.ave_codigo);
                if (buscarNombre == null)
                {
                    postAveria.ave_fec_creacion = DateTime.Now;
                    postAveria.ave_userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.icb_averias.Add(postAveria);
                    context.SaveChanges();
                    TempData["mensaje"] = "El dato ingresado se ha creado correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "El dato ingresado ya existe";
                    parametrosBusqueda();
                    BuscarFavoritos(menu);
                    return View();
                }
            }

            parametrosBusqueda();
            BuscarFavoritos(menu);
            return View();
        }

        // POST: averias/Edit/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(icb_averias averias, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.icb_averias
                           where a.ave_codigo == averias.ave_codigo || a.ave_id == averias.ave_id
                           select a.ave_codigo).Count();

                if (nom == 1)
                {
                    averias.ave_fec_actualizacion = DateTime.Now;
                    averias.ave_userid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(averias).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización de la averia fue exitosa!";
                    parametrosBusqueda();
                    BuscarFavoritos(menu);
                    return View(averias);
                }

                TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
            }

            parametrosBusqueda();
            BuscarFavoritos(menu);
            return View(averias);
        }

        // GET: averias/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_averias averias = context.icb_averias.Find(id);
            if (averias == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from a in context.users
                                        join b in context.icb_averias on a.user_id equals b.ave_userid_creacion
                                        where b.ave_id == id
                                        select a.user_nombre;
            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }
            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result1 = from a in context.users
                                         join b in context.icb_averias on a.user_id equals b.ave_userid_actualizacion
                                         where b.ave_id == id
                                         select a.user_nombre;
            foreach (string i in result1)
            {
                ViewBag.user_nombre_act = i;
            }

            parametrosBusqueda();
            BuscarFavoritos(menu);
            return View(averias);
        }

        public JsonResult BuscarAveriasPaginados()
        {
            var data = context.icb_averias.ToList().Select(x => new
            {
                x.ave_id,
                x.ave_codigo,
                x.ave_descripcion,
                ave_estado = x.ave_estado ? "Activo" : "Inactivo"
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