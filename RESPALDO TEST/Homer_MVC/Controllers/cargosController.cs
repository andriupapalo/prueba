using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class cargosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        private void parametrosBusqueda()
        {
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 118);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
        }


        // GET: cargos
        public ActionResult Crear(int? menu)
        {
            parametrosBusqueda();
            ViewBag.cargo_area_id =
                new SelectList(context.icb_area.OrderBy(x => x.area_nombre), "area_id", "area_nombre");
            icb_cargo crearCargo = new icb_cargo { cargo_estado = true, cargo_razon_inactivo = "No aplica" };
            BuscarFavoritos(menu);
            return View(crearCargo);
        }

        [HttpPost]
        public ActionResult Crear(icb_cargo postCargo, int? menu)
        {
            if (ModelState.IsValid)
            {
                icb_cargo buscarNombre = context.icb_cargo.FirstOrDefault(x => x.cargo_nombre == postCargo.cargo_nombre);
                if (buscarNombre == null)
                {
                    postCargo.cargo_fec_creacion = DateTime.Now;
                    postCargo.cargo_userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.icb_cargo.Add(postCargo);
                    context.SaveChanges();
                    TempData["mensaje"] = "El cargo " + postCargo.cargo_nombre + " se creo correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "El cargo " + postCargo.cargo_nombre + " ya existe";
                    parametrosBusqueda();
                    ViewBag.cargo_area_id = new SelectList(context.icb_area.OrderBy(x => x.area_nombre), "area_id",
                        "area_nombre");
                    BuscarFavoritos(menu);
                    return View();
                }
            }

            parametrosBusqueda();
            ViewBag.cargo_area_id =
                new SelectList(context.icb_area.OrderBy(x => x.area_nombre), "area_id", "area_nombre");
            BuscarFavoritos(menu);
            return View();
        }

        // POST: cargos/Edit/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(icb_cargo cargos, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.icb_cargo
                           where a.cargo_nombre == cargos.cargo_nombre || a.cargo_id == cargos.cargo_id
                           select a.cargo_nombre).Count();

                if (nom == 1)
                {
                    cargos.cargo_fec_actualizacion = DateTime.Now;
                    cargos.cargo_userid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(cargos).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del cargo fue exitoso!";
                    ViewBag.cargo_area_id = new SelectList(context.icb_area.OrderBy(x => x.area_nombre), "area_id",
                        "area_nombre");
                    parametrosBusqueda();
                    ConsultaDatosCreacion(cargos);
                    BuscarFavoritos(menu);
                    return View(cargos);
                }

                TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
            }

            parametrosBusqueda();
            ViewBag.cargo_area_id =
                new SelectList(context.icb_area.OrderBy(x => x.area_nombre), "area_id", "area_nombre");
            ConsultaDatosCreacion(cargos);
            BuscarFavoritos(menu);
            return View(cargos);
        }

        // GET: cargos/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_cargo cargos = context.icb_cargo.Find(id);
            if (cargos == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from a in context.users
                                        join b in context.icb_cargo on a.user_id equals b.cargo_userid_creacion
                                        where b.cargo_id == id
                                        select a.user_nombre;
            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }
            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result1 = from a in context.users
                                         join b in context.icb_cargo on a.user_id equals b.cargo_userid_actualizacion
                                         where b.cargo_id == id
                                         select a.user_nombre;
            foreach (string i in result1)
            {
                ViewBag.user_nombre_act = i;
            }

            parametrosBusqueda();
            ViewBag.cargo_area_id =
                new SelectList(context.icb_area.OrderBy(x => x.area_nombre), "area_id", "area_nombre");
            BuscarFavoritos(menu);
            return View(cargos);
        }


        public void ConsultaDatosCreacion(icb_cargo cargos)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(cargos.cargo_userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(cargos.cargo_userid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarCargosPaginados()
        {
            var data = from cargo in context.icb_cargo
                       join area in context.icb_area
                           on cargo.cargo_area_id equals area.area_id
                       select new
                       {
                           cargo.cargo_id,
                           cargo.cargo_nombre,
                           cargo_estado = cargo.cargo_estado ? "Activo" : "Inactivo",
                           area.area_nombre
                       };
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