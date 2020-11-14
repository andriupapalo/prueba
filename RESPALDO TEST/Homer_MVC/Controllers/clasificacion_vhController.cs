using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class clasificacion_vhController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public ActionResult Create(int? menu)
        {
            clasificacion_vehiculo claVh = new clasificacion_vehiculo
            {
                clavh_estado = true,
                clavhrazoninactivo = "No aplica"
            };

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 40);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(claVh);
        }

        // POST: cla_vh/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(clasificacion_vehiculo cla_vh, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.clasificacion_vehiculo
                           where a.clavh_nombre == cla_vh.clavh_nombre
                           select a.clavh_nombre).Count();

                if (nom == 0)
                {
                    cla_vh.clavhfec_creacion = DateTime.Now;
                    cla_vh.clavhuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.clasificacion_vehiculo.Add(cla_vh);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro de la nueva clasificacion de vehiculo fue exitoso!";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 40);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(cla_vh);
        }


        // GET: cla_vh/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            clasificacion_vehiculo cla_vh = context.clasificacion_vehiculo.Find(id);
            if (cla_vh == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(cla_vh.clavhuserid_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(cla_vh.clavhuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 40);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(cla_vh);
        }


        // POST: cla_vh/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(clasificacion_vehiculo cla_vh, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.clasificacion_vehiculo
                           where a.clavh_nombre == cla_vh.clavh_nombre || a.clavh_id == cla_vh.clavh_id
                           select a.clavh_nombre).Count();

                if (nom == 1)
                {
                    cla_vh.clavhfec_actualizacion = DateTime.Now;
                    cla_vh.clavhuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(cla_vh).State = EntityState.Modified;
                    context.SaveChanges();
                    ConsultaDatosCreacion(cla_vh);
                    TempData["mensaje"] = "La actualización de la clasificacion fue exitoso!";
                    BuscarFavoritos(menu);
                    return View(cla_vh);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            ConsultaDatosCreacion(cla_vh);
            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 40);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(cla_vh);
        }


        public void ConsultaDatosCreacion(clasificacion_vehiculo cla_vh)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(cla_vh.clavhuserid_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = context.users.Find(cla_vh.clavhuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarClasifVhPaginadas()
        {
            var data = context.clasificacion_vehiculo.ToList().Select(x => new
            {
                x.clavh_id,
                x.clavh_nombre,
                clavh_estado = x.clavh_estado ? "Activo" : "Inactivo"
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