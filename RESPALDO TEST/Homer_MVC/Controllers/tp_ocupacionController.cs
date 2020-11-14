using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tp_ocupacionController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: tp_ocupacion
        public ActionResult Create(int? menu)
        {
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 13);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            tp_ocupacion crearOcupacion = new tp_ocupacion { tpocupacion_estado = true, tpocupacion_razoninactivo = "No aplica" };
            BuscarFavoritos(menu);
            return View(crearOcupacion);
        }

        // POST: tp_ocupacion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(tp_ocupacion tp_ocupacion, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.tp_ocupacion
                           where a.tpocupacion_nombre == tp_ocupacion.tpocupacion_nombre
                           select a.tpocupacion_nombre).Count();

                if (nom == 0)
                {
                    tp_ocupacion.tpocupacionfec_creacion = DateTime.Now;
                    tp_ocupacion.tpocupacionuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.tp_ocupacion.Add(tp_ocupacion);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro del nuevo tipo ocupación fue exitoso!";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 13);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(tp_ocupacion);
        }

        // GET: tp_ocupacion/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tp_ocupacion tp_ocupacion = context.tp_ocupacion.Find(id);
            if (tp_ocupacion == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from c in context.users
                                        join b in context.tp_ocupacion on c.user_id equals b.tpocupacionuserid_creacion
                                        where b.tpocupacion_id == id
                                        select c.user_nombre;
            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }
            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result1 = from c in context.users
                                         join b in context.tp_ocupacion on c.user_id equals b.tpocupacionuserid_actualizacion
                                         where b.tpocupacion_id == id
                                         select c.user_nombre;
            foreach (string i in result1)
            {
                ViewBag.user_nombre_act = i;
            }

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 13);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(tp_ocupacion);
        }


        // POST: tp_ocupacion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(tp_ocupacion tp_ocupacion, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.tp_ocupacion
                           where a.tpocupacion_nombre == tp_ocupacion.tpocupacion_nombre ||
                                 a.tpocupacion_id == tp_ocupacion.tpocupacion_id
                           select a.tpocupacion_nombre).Count();

                if (nom == 1)
                {
                    tp_ocupacion.tpocupacionfec_actualizacion = DateTime.Now;
                    tp_ocupacion.tpocupacionuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(tp_ocupacion).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del tipo ocupación fue exitoso!";
                    ConsultaDatosCreacion(tp_ocupacion);
                    BuscarFavoritos(menu);
                    return View(tp_ocupacion);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 13);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            ConsultaDatosCreacion(tp_ocupacion);
            BuscarFavoritos(menu);
            return View(tp_ocupacion);
        }


        public void ConsultaDatosCreacion(tp_ocupacion tp_ocupacion)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tp_ocupacion.tpocupacionuserid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(tp_ocupacion.tpocupacionuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarTipoOcupacionPaginados()
        {
            var data = context.tp_ocupacion.ToList().Select(x => new
            {
                x.tpocupacion_id,
                x.tpocupacion_nombre,
                tpocupacion_estado = x.tpocupacion_estado ? "Activo" : "Inactivo"
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