using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class gravedad_averiasController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        private void parametrosBusqueda()
        {
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 122);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
        }


        [HttpGet]
        // GET: averias
        public ActionResult Crear(int? menu)
        {
            parametrosBusqueda();
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        //POST: crear
        public ActionResult Crear(icb_gravedad_averias posGrave, int? menu)
        {
            if (ModelState.IsValid)
            {
                icb_gravedad_averias buscarNombre =
                    context.icb_gravedad_averias.FirstOrDefault(x => x.grave_codigo == posGrave.grave_codigo);
                if (buscarNombre == null)
                {
                    posGrave.grave_fec_creacion = DateTime.Now;
                    posGrave.grave_userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.icb_gravedad_averias.Add(posGrave);
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

        // POST: gravedad/Edit/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(icb_gravedad_averias grave, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.icb_gravedad_averias
                           where a.grave_codigo == grave.grave_codigo || a.grave_id == grave.grave_id
                           select a.grave_codigo).Count();

                if (nom == 1)
                {
                    grave.grave_fec_actualizacion = DateTime.Now;
                    grave.grave_userid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(grave).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del registro fue exitoso!";
                    parametrosBusqueda();
                    BuscarFavoritos(menu);
                    return View(grave);
                }

                TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
            }

            parametrosBusqueda();
            BuscarFavoritos(menu);
            return View(grave);
        }

        // GET: gravedad/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_gravedad_averias grave = context.icb_gravedad_averias.Find(id);
            if (grave == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from a in context.users
                                        join b in context.icb_gravedad_averias on a.user_id equals b.grave_userid_creacion
                                        where b.grave_id == id
                                        select a.user_nombre;
            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }
            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result1 = from a in context.users
                                         join b in context.icb_gravedad_averias on a.user_id equals b.grave_userid_actualizacion
                                         where b.grave_id == id
                                         select a.user_nombre;
            foreach (string i in result1)
            {
                ViewBag.user_nombre_act = i;
            }

            parametrosBusqueda();
            BuscarFavoritos(menu);
            return View(grave);
        }

        public JsonResult BuscarGravedadAvePaginados()
        {
            var data = context.icb_gravedad_averias.ToList().Select(x => new
            {
                x.grave_id,
                x.grave_codigo,
                x.grave_descripcion,
                grave_estado = x.grave_estado ? "Activo" : "Inactivo"
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