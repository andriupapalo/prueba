using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tiposumi_terceroController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: tiposumi_tercero
        public ActionResult Create(int? menu)
        {
            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 17).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 17);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            tiposumi_tercero crearSuministro = new tiposumi_tercero
            { tpsuministro_estado = true, tpsuministro_razoninactivo = "No aplica" };
            BuscarFavoritos(menu);
            return View(crearSuministro);
        }

        // POST: tiposumi_tercero/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(tiposumi_tercero tiposumi_tercero, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.tiposumi_tercero
                           where a.tpsuministro_nombre == tiposumi_tercero.tpsuministro_nombre
                           select a.tpsuministro_nombre).Count();

                if (nom == 0)
                {
                    tiposumi_tercero.tpsuministrofec_creacion = DateTime.Now;
                    tiposumi_tercero.tpsuministrouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.tiposumi_tercero.Add(tiposumi_tercero);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro de la nuevo tipo suministro fue exitoso!";
                    return RedirectToAction("Create", new { menu });
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 17).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 17);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(tiposumi_tercero);
        }

        // GET: tiposumi_tercero/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tiposumi_tercero tiposumi_tercero = context.tiposumi_tercero.Find(id);
            if (tiposumi_tercero == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from c in context.users
                                        join b in context.tiposumi_tercero on c.user_id equals b.tpsuministrouserid_creacion
                                        where b.tpsuministro_id == id
                                        select c.user_nombre;
            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }
            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result1 = from c in context.users
                                         join b in context.tiposumi_tercero on c.user_id equals b.tpsuministrouserid_actualizacion
                                         where b.tpsuministro_id == id
                                         select c.user_nombre;
            foreach (string i in result1)
            {
                ViewBag.user_nombre_act = i;
            }

            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 17).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 17);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(tiposumi_tercero);
        }

        // POST: tiposumi_tercero/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(tiposumi_tercero tiposumi_tercero, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.tiposumi_tercero
                           where a.tpsuministro_nombre == tiposumi_tercero.tpsuministro_nombre ||
                                 a.tpsuministro_id == tiposumi_tercero.tpsuministro_id
                           select a.tpsuministro_nombre).Count();

                if (nom == 1)
                {
                    tiposumi_tercero.tpsuministrofec_actualizacion = DateTime.Now;
                    tiposumi_tercero.tpsuministrouserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(tiposumi_tercero).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del tipo suministro fue exitoso!";
                    ConsultaDatosCreacion(tiposumi_tercero);
                    BuscarFavoritos(menu);
                    return View(tiposumi_tercero);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            //var parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 17).ToList();
            //ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 17);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            ConsultaDatosCreacion(tiposumi_tercero);
            BuscarFavoritos(menu);
            return View(tiposumi_tercero);
        }


        public void ConsultaDatosCreacion(tiposumi_tercero tiposumi_tercero)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tiposumi_tercero.tpsuministrouserid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(tiposumi_tercero.tpsuministrouserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarTipoSuministroPaginados()
        {
            var data = context.tiposumi_tercero.ToList().Select(x => new
            {
                x.tpsuministro_id,
                x.tpsuministro_nombre,
                tpsuministro_estado = x.tpsuministro_estado ? "Activo" : "Inactivo"
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