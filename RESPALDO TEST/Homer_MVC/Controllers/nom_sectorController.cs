using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class nom_sectorController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        // GET: nom_sector
        public ActionResult Create(int? menu)
        {
            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 10).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 10);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            nom_sector crearSector = new nom_sector { sec_estado = true, sec_razoninactivo = "No aplica" };

            ViewBag.ciudad_id = new SelectList(context.nom_ciudad.OrderBy(x => x.ciu_nombre), "ciu_id", "ciu_nombre");

            BuscarFavoritos(menu);
            return View(crearSector);
        }


        // POST: nom_sector/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(nom_sector nom_sector, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.nom_sector
                           where a.sec_nombre == nom_sector.sec_nombre
                           select a.sec_nombre).Count();

                if (nom == 0)
                {
                    nom_sector.secfec_creacion = DateTime.Now;
                    nom_sector.secuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.nom_sector.Add(nom_sector);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro del nuevo sector fue exitoso!";
                    ViewBag.ciudad_id = new SelectList(context.nom_ciudad.OrderBy(x => x.ciu_nombre), "ciu_id",
                        "ciu_nombre", nom_sector.ciudad_id);
                    return RedirectToAction("Create", new { menu });
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 10).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 10);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(nom_sector);
        }


        // GET: nom_sector/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            nom_sector nom_sector = context.nom_sector.Find(id);
            if (nom_sector == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from c in context.users
                                        join b in context.nom_sector on c.user_id equals b.secuserid_creacion
                                        where b.sec_id == id
                                        select c.user_nombre;
            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }
            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result1 = from c in context.users
                                         join b in context.nom_sector on c.user_id equals b.secuserid_actualizacion
                                         where b.sec_id == id
                                         select c.user_nombre;
            foreach (string i in result1)
            {
                ViewBag.user_nombre_act = i;
            }

            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 10).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 10);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            ViewBag.ciudad_id = new SelectList(context.nom_ciudad.OrderBy(x => x.ciu_nombre), "ciu_id", "ciu_nombre",
                nom_sector.ciudad_id);
            return View(nom_sector);
        }


        // POST: nom_sector/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(nom_sector nom_sector, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.nom_sector
                           where a.sec_nombre == nom_sector.sec_nombre || a.sec_id == nom_sector.sec_id
                           select a.sec_nombre).Count();

                if (nom == 1)
                {
                    nom_sector.secfec_actualizacion = DateTime.Now;
                    nom_sector.secuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(nom_sector).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del sector fue exitoso!";
                    ConsultaDatosCreacion(nom_sector);
                    BuscarFavoritos(menu);
                    ViewBag.ciudad_id = new SelectList(context.nom_ciudad.OrderBy(x => x.ciu_nombre), "ciu_id",
                        "ciu_nombre", nom_sector.ciudad_id);
                    return View(nom_sector);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 10).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 10);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            ConsultaDatosCreacion(nom_sector);
            BuscarFavoritos(menu);
            ViewBag.ciudad_id = new SelectList(context.nom_ciudad.OrderBy(x => x.ciu_nombre), "ciu_id", "ciu_nombre",
                nom_sector.ciudad_id);
            return View(nom_sector);
        }


        public void ConsultaDatosCreacion(nom_sector nom_sector)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(nom_sector.secuserid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(nom_sector.secuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarSectoresPaginados()
        {
            var data = context.nom_sector.ToList().Select(x => new
            {
                x.sec_id,
                x.sec_nombre,
                sec_estado = x.sec_estado ? "Activo" : "Inactivo"
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