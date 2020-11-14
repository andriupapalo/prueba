using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class color_vhController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public ActionResult Create(int? menu)
        {
            color_vehiculo createColor = new color_vehiculo
            {
                colvh_estado = true
            };
            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 23).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 23);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;

            System.Collections.Generic.List<color_vehiculo> colores = context.color_vehiculo.ToList();
            ViewBag.colores = colores;
            BuscarFavoritos(menu);
            return View(createColor);
        }

        // POST: col_vh/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(color_vehiculo col_vh, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.color_vehiculo
                           where a.colvh_id == col_vh.colvh_id
                           select a.colvh_id).Count();

                if (nom == 0)
                {
                    col_vh.colvhfec_creacion = DateTime.Now;
                    col_vh.colvhuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.color_vehiculo.Add(col_vh);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro del nuevo color de vehiculo fue exitoso!";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";
            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 23).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 23);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(col_vh);
        }


        // GET: col_vh/Edit/5
        public ActionResult update(string id, int? menu)
        {
            //valida si el id es null
            if (id == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            color_vehiculo col_vh = context.color_vehiculo.Find(id);
            if (col_vh == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(col_vh.colvhuserid_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(col_vh.colvhuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }

            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 23).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 23);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(col_vh);
        }


        // POST: col_vh/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(color_vehiculo col_vh, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.color_vehiculo
                           where a.colvh_nombre == col_vh.colvh_nombre && a.colvh_id == col_vh.colvh_id
                           select a.colvh_nombre).Count();

                if (nom == 1)
                {
                    col_vh.colvhfec_actualizacion = DateTime.Now;
                    col_vh.colvhuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(col_vh).State = EntityState.Modified;
                    context.SaveChanges();
                    ConsultaDatosCreacion(col_vh);
                    TempData["mensaje"] = "La actualización del color fue exitoso!";
                    BuscarFavoritos(menu);
                    return View(col_vh);
                }

                {
                    int nom2 = (from a in context.color_vehiculo
                                where a.colvh_nombre == col_vh.colvh_nombre
                                select a.colvh_nombre).Count();
                    if (nom2 == 0)
                    {
                        col_vh.colvhfec_actualizacion = DateTime.Now;
                        col_vh.colvhuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        context.Entry(col_vh).State = EntityState.Modified;
                        context.SaveChanges();
                        ConsultaDatosCreacion(col_vh);
                        TempData["mensaje"] = "La actualización del color fue exitoso!";
                        BuscarFavoritos(menu);
                        return View(col_vh);
                    }

                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                }
            }

            ConsultaDatosCreacion(col_vh);
            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";
            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 23).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 23);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(col_vh);
        }


        public void ConsultaDatosCreacion(color_vehiculo col_vh)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(col_vh.colvhuserid_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = context.users.Find(col_vh.colvhuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult GetColoresJson()
        {
            var buscarColores = context.color_vehiculo.ToList().Select(x => new
            {
                x.colvh_id,
                x.colvh_nombre,
                colvh_estado = x.colvh_estado ? "Activo" : "Inactivo"
            });
            return Json(buscarColores, JsonRequestBehavior.AllowGet);
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