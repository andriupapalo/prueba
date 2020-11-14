using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tp_DpteController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: tp_Dpte/Create
        public ActionResult Create(int? menu)
        {
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 15);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            tp_Dpte crearDeporte = new tp_Dpte { tpdpte_estado = true, tpdpte_razoninactivo = "No aplica" };
            BuscarFavoritos(menu);
            return View(crearDeporte);
        }

        // POST: tp_Dpte/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(tp_Dpte tp_Dpte, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.tp_Dpte
                           where a.tpdpte_nombre == tp_Dpte.tpdpte_nombre
                           select a.tpdpte_nombre).Count();

                if (nom == 0)
                {
                    tp_Dpte.tpdptefec_creacion = DateTime.Now;
                    tp_Dpte.tpdpteuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.tp_Dpte.Add(tp_Dpte);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro del nuevo deporte fue exitoso!";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 15);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(tp_Dpte);
        }

        // GET: tp_Dpte/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tp_Dpte tp_Dpte = context.tp_Dpte.Find(id);
            if (tp_Dpte == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from a in context.users
                                        join b in context.tp_Dpte on a.user_id equals b.tpdpteuserid_creacion
                                        where b.tpdpte_id == id
                                        select a.user_nombre;
            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }
            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result1 = from a in context.users
                                         join b in context.tp_Dpte on a.user_id equals b.tpdpteuserid_actualizacion
                                         where b.tpdpte_id == id
                                         select a.user_nombre;
            foreach (string i in result1)
            {
                ViewBag.user_nombre_act = i;
            }

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 15);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(tp_Dpte);
        }

        // POST: tp_Dpte/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(tp_Dpte tp_Dpte, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.tp_Dpte
                           where a.tpdpte_nombre == tp_Dpte.tpdpte_nombre || a.tpdpte_id == tp_Dpte.tpdpte_id
                           select a.tpdpte_nombre).Count();

                if (nom == 1)
                {
                    tp_Dpte.tpdptefec_actualizacion = DateTime.Now;
                    tp_Dpte.tpdpteuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(tp_Dpte).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del deporte fue exitoso!";
                    ConsultaDatosCreacion(tp_Dpte);
                    BuscarFavoritos(menu);
                    return View(tp_Dpte);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 15);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            ConsultaDatosCreacion(tp_Dpte);
            BuscarFavoritos(menu);
            return View(tp_Dpte);
        }


        public void ConsultaDatosCreacion(tp_Dpte tp_Dpte)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tp_Dpte.tpdpteuserid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(tp_Dpte.tpdpteuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarTiposDeportePaginados()
        {
            var data = context.tp_Dpte.ToList().Select(x => new
            {
                x.tpdpte_id,
                x.tpdpte_nombre,
                tpdpte_estado = x.tpdpte_estado ? "Activo" : "Inactivo"
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