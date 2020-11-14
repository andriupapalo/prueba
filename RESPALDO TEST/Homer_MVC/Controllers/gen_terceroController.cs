using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class gen_terceroController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: gen_tercero/Create
        public ActionResult Create(int? menu)
        {
            //var parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 16).ToList();
            //ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 16);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            gen_tercero crearGenero = new gen_tercero { gentercero_estado = true, gentercero_razoninactivo = "No aplica" };
            BuscarFavoritos(menu);
            return View(crearGenero);
        }


        // POST: gen_tercero/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(gen_tercero gen_tercero, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.gen_tercero
                           where a.gentercero_nombre == gen_tercero.gentercero_nombre
                           select a.gentercero_nombre).Count();

                if (nom == 0)
                {
                    gen_tercero.gentercerofec_creacion = DateTime.Now;
                    gen_tercero.gentercerouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.gen_tercero.Add(gen_tercero);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro del nuevo género fue exitoso!";
                    return RedirectToAction("Create", new { menu });
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 16).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 16);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(gen_tercero);
        }


        // GET: gen_tercero/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            gen_tercero gen_tercero = context.gen_tercero.Find(id);
            if (gen_tercero == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from a in context.users
                                        join b in context.gen_tercero on a.user_id equals b.gentercerouserid_creacion
                                        where b.gentercero_id == id
                                        select a.user_nombre;
            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }
            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result1 = from a in context.users
                                         join b in context.gen_tercero on a.user_id equals b.gentercerouserid_actualizacion
                                         where b.gentercero_id == id
                                         select a.user_nombre;
            foreach (string i in result1)
            {
                ViewBag.user_nombre_act = i;
            }

            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 16).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 16);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(gen_tercero);
        }


        // POST: gen_tercero/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(gen_tercero gen_tercero, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.gen_tercero
                           where a.gentercero_nombre == gen_tercero.gentercero_nombre ||
                                 a.gentercero_id == gen_tercero.gentercero_id
                           select a.gentercero_nombre).Count();

                if (nom == 1)
                {
                    gen_tercero.gentercerofec_actualizacion = DateTime.Now;
                    gen_tercero.gentercerouserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(gen_tercero).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del género fue exitoso!";
                    ConsultaDatosCreacion(gen_tercero);
                    BuscarFavoritos(menu);
                    return View(gen_tercero);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            //var parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 16).ToList();
            //ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 16);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            ConsultaDatosCreacion(gen_tercero);
            BuscarFavoritos(menu);
            return View(gen_tercero);
        }


        public void ConsultaDatosCreacion(gen_tercero gen_tercero)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(gen_tercero.gentercerouserid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(gen_tercero.gentercerouserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarGenerosPaginados()
        {
            var data = context.gen_tercero.ToList().Select(x => new
            {
                x.gentercero_id,
                x.gentercero_nombre,
                gentercero_estado = x.gentercero_estado ? "Activo" : "Inactivo"
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }

            base.Dispose(disposing);
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