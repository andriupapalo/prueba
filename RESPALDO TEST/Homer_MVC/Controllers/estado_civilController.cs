using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class estado_civilController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        // GET: estado_civil/Create
        public ActionResult Create(int? menu)
        {
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 12);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            estado_civil crearEstado = new estado_civil { edocivil_estado = true, edocivil_razoninactivo = "No aplica" };
            BuscarFavoritos(menu);
            return View(crearEstado);
        }


        // POST: estado_civil/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(estado_civil estado_civil, int? menu)
        {
            //consulta si el registro esta en BD
            if (ModelState.IsValid)
            {
                int nom = (from a in context.estado_civil
                           where a.edocivil_nombre == estado_civil.edocivil_nombre
                           select a.edocivil_nombre).Count();

                if (nom == 0)
                {
                    estado_civil.edocivilfec_creacion = DateTime.Now;
                    estado_civil.edociviluserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.estado_civil.Add(estado_civil);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro del nuevo estado civil fue exitoso!";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 12);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(estado_civil);
        }


        // GET: estado_civil/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            estado_civil estado_civil = context.estado_civil.Find(id);

            if (estado_civil == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from c in context.users
                                        join b in context.estado_civil on c.user_id equals b.edociviluserid_creacion
                                        where b.edocivil_id == id
                                        select c.user_nombre;
            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }
            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result1 = from c in context.users
                                         join b in context.estado_civil on c.user_id equals b.edociviluserid_actualizacion
                                         where b.edocivil_id == id
                                         select c.user_nombre;
            foreach (string i in result1)
            {
                ViewBag.user_nombre_act = i;
            }

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 12);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(estado_civil);
        }


        // POST: estado_civil/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(estado_civil estado_civil, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.estado_civil
                           where a.edocivil_nombre == estado_civil.edocivil_nombre || a.edocivil_id == estado_civil.edocivil_id
                           select a.edocivil_nombre).Count();

                if (nom == 1)
                {
                    estado_civil.edocivilfec_actualizacion = DateTime.Now;
                    estado_civil.edociviluserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(estado_civil).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del estado civil fue exitoso!";
                    ConsultaDatosCreacion(estado_civil);
                    BuscarFavoritos(menu);
                    return View(estado_civil);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 12);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            ConsultaDatosCreacion(estado_civil);
            BuscarFavoritos(menu);
            return View(estado_civil);
        }


        public JsonResult BuscarEstadoCivilPaginados()
        {
            var data = context.estado_civil.ToList().Select(x => new
            {
                x.edocivil_id,
                x.edocivil_nombre,
                edocivil_estado = x.edocivil_estado ? "Activo" : "Inactivo"
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public void ConsultaDatosCreacion(estado_civil estado_civil)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(estado_civil.edociviluserid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(estado_civil.edociviluserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
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