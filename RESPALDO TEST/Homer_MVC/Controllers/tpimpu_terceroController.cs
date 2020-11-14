using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tpimpu_terceroController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: tpimpu_tercero
        public ActionResult Create(int? menu)
        {
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 19);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            tpimpu_tercero crearImpuesto = new tpimpu_tercero { tpimpu_estado = true, tpimpu_razoninactivo = "No aplica" };
            BuscarFavoritos(menu);
            return View(crearImpuesto);
        }

        // POST: tpimpu_tercero/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(tpimpu_tercero tpimpu_tercero, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.tpimpu_tercero
                           where a.tpimpu_nombre == tpimpu_tercero.tpimpu_nombre
                           select a.tpimpu_nombre).Count();

                if (nom == 0)
                {
                    tpimpu_tercero.tpimpufec_creacion = DateTime.Now;
                    tpimpu_tercero.tpimpuuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.tpimpu_tercero.Add(tpimpu_tercero);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro del nuevo tipo impuesto fue exitoso!";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 19);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(tpimpu_tercero);
        }

        // GET: tpimpu_tercero/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tpimpu_tercero tpimpu_tercero = context.tpimpu_tercero.Find(id);
            if (tpimpu_tercero == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from c in context.users
                                        join b in context.tpimpu_tercero on c.user_id equals b.tpimpuuserid_creacion
                                        where b.tpimpu_id == id
                                        select c.user_nombre;
            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }
            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result1 = from c in context.users
                                         join b in context.tpimpu_tercero on c.user_id equals b.tpimpuuserid_actualizacion
                                         where b.tpimpu_id == id
                                         select c.user_nombre;
            foreach (string i in result1)
            {
                ViewBag.user_nombre_act = i;
            }

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 19);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(tpimpu_tercero);
        }

        // POST: tpimpu_tercero/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(tpimpu_tercero tpimpu_tercero, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.tpimpu_tercero
                           where a.tpimpu_nombre == tpimpu_tercero.tpimpu_nombre || a.tpimpu_id == tpimpu_tercero.tpimpu_id
                           select a.tpimpu_nombre).Count();

                if (nom == 1)
                {
                    tpimpu_tercero.tpimpufec_actualizacion = DateTime.Now;
                    tpimpu_tercero.tpimpuuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(tpimpu_tercero).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del tipo impuesto fue exitoso!";
                    ConsultaDatosCreacion(tpimpu_tercero);
                    BuscarFavoritos(menu);
                    return View(tpimpu_tercero);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 19);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            ConsultaDatosCreacion(tpimpu_tercero);
            BuscarFavoritos(menu);
            return View(tpimpu_tercero);
        }


        public void ConsultaDatosCreacion(tpimpu_tercero tpimpu_tercero)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tpimpu_tercero.tpimpuuserid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(tpimpu_tercero.tpimpuuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarTiposImpuestoPaginados()
        {
            var data = context.tpimpu_tercero.ToList().Select(x => new
            {
                x.tpimpu_id,
                x.tpimpu_nombre,
                tpimpu_estado = x.tpimpu_estado ? "Activo" : "Inactivo"
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