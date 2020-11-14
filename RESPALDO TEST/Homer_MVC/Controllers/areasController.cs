using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class areasController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        private void parametrosBusqueda()
        {
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 119);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
        }

        [HttpGet]
        // GET: areas
        public ActionResult Crear(int? menu)
        {
            parametrosBusqueda();
            icb_area crearArea = new icb_area { area_estado = true, area_razon_inactivo = "No aplica" };
            BuscarFavoritos(menu);
            return View(crearArea);
        }

        [HttpPost]
        //POST: crear
        public ActionResult Crear(icb_area postArea, int? menu)
        {
            if (ModelState.IsValid)
            {
                icb_area buscarNombre = context.icb_area.FirstOrDefault(x => x.area_nombre == postArea.area_nombre);
                if (buscarNombre == null)
                {
                    postArea.area_fec_creacion = DateTime.Now;
                    postArea.area_userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.icb_area.Add(postArea);
                    context.SaveChanges();
                    TempData["mensaje"] = "El area " + postArea.area_nombre + " se creo correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "El area " + postArea.area_nombre + " ya existe";
                }
            }

            parametrosBusqueda();
            BuscarFavoritos(menu);
            return View();
        }

        // POST: areas/Edit/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(icb_area areas, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.icb_area
                           where a.area_nombre == areas.area_nombre || a.area_id == areas.area_id
                           select a.area_nombre).Count();

                if (nom == 1)
                {
                    areas.area_fec_actualizacion = DateTime.Now;
                    areas.area_userid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(areas).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del area fue exitoso!";
                    parametrosBusqueda();
                    ConsultaDatosCreacion(areas);
                    BuscarFavoritos(menu);
                    return View(areas);
                }

                TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
            }

            parametrosBusqueda();
            ConsultaDatosCreacion(areas);
            BuscarFavoritos(menu);
            return View(areas);
        }

        // GET: areas/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_area area = context.icb_area.Find(id);
            if (area == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from a in context.users
                                        join b in context.icb_area on a.user_id equals b.area_userid_creacion
                                        where b.area_id == id
                                        select a.user_nombre;
            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }
            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result1 = from a in context.users
                                         join b in context.icb_area on a.user_id equals b.area_userid_actualizacion
                                         where b.area_id == id
                                         select a.user_nombre;
            foreach (string i in result1)
            {
                ViewBag.user_nombre_act = i;
            }

            parametrosBusqueda();
            BuscarFavoritos(menu);
            return View(area);
        }


        public void ConsultaDatosCreacion(icb_area areas)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(areas.area_userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(areas.area_userid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarAreasPaginados()
        {
            var data = context.icb_area.ToList().Select(x => new
            {
                x.area_id,
                x.area_nombre,
                area_estado = x.area_estado ? "Activo" : "Inactivo"
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