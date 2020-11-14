using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class nom_paisController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: nom_pais/Create
        public ActionResult Create(int? menu)
        {
            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 3).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 3);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            nom_pais crearPais = new nom_pais { pais_estado = true, pais_razoninactivo = "No aplica" };
            BuscarFavoritos(menu);
            return View(crearPais);
        }


        // POST: nom_pais/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(nom_pais nom_pais, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.nom_pais
                           where a.pais_nombre == nom_pais.pais_nombre
                           select a.pais_nombre).Count();

                if (nom == 0)
                {
                    nom_pais.paisfec_creacion = DateTime.Now;
                    nom_pais.paisuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.nom_pais.Add(nom_pais);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro del nuevo pais fue exitoso!";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 3).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 3);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(nom_pais);
        }


        // GET: nom_pais/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            nom_pais nom_pais = context.nom_pais.Find(id);
            if (nom_pais == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from c in context.users
                                        join b in context.nom_pais on c.user_id equals b.paisuserid_creacion
                                        where b.pais_id == id
                                        select c.user_nombre;
            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }
            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result1 = from c in context.users
                                         join b in context.nom_pais on c.user_id equals b.paisuserid_actualizacion
                                         where b.pais_id == id
                                         select c.user_nombre;
            foreach (string i in result1)
            {
                ViewBag.user_nombre_act = i;
            }

            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 3).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 3);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(nom_pais);
        }


        // POST: nom_pais/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(nom_pais nom_pais, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.nom_pais
                           where a.pais_nombre == nom_pais.pais_nombre || a.pais_id == nom_pais.pais_id
                           select a.pais_nombre).Count();

                if (nom == 1)
                {
                    nom_pais.paisfec_actualizacion = DateTime.Now;
                    nom_pais.paisuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(nom_pais).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del pais fue exitoso!";
                    ConsultaDatosCreacion(nom_pais);
                    BuscarFavoritos(menu);
                    return View(nom_pais);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            //var parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 3).ToList();
            //ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 3);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            ConsultaDatosCreacion(nom_pais);
            BuscarFavoritos(menu);
            return View(nom_pais);
        }


        public void ConsultaDatosCreacion(nom_pais nom_pais)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(nom_pais.paisuserid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(nom_pais.paisuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarPaises()
        {
            var data = context.nom_pais.ToList().Select(x => new
            {
                x.pais_id,
                x.pais_nombre,
                codigo = x.cod_pais,
                pais_estado = x.pais_estado ? "Activo" : "Inactivo"
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