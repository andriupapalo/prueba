using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tipomotor_vhController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public ActionResult Create(int? menu)
        {
            tpmotor_vehiculo tpMotor = new tpmotor_vehiculo
            {
                tpmot_estado = true
            };

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 30);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(tpMotor);
        }


        // POST: tomot_vh/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(tpmotor_vehiculo tpmot_vh, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.tpmotor_vehiculo
                           where a.tpmot_nombre == tpmot_vh.tpmot_nombre
                           select a.tpmot_nombre).Count();

                if (nom == 0)
                {
                    tpmot_vh.tpmotorfec_creacion = DateTime.Now;
                    tpmot_vh.tpmotuser_id_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.tpmotor_vehiculo.Add(tpmot_vh);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro del nuevo tipo de motor de vehiculo fue exitoso!";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 30);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(tpmot_vh);
        }


        // GET: tpmot_vh/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tpmotor_vehiculo tpmot_vh = context.tpmotor_vehiculo.Find(id);
            if (tpmot_vh == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tpmot_vh.tpmotuser_id_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(tpmot_vh.tpmotuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 30);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(tpmot_vh);
        }


        // POST: tpmot_vh/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(tpmotor_vehiculo tpmot_vh, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.tpmotor_vehiculo
                           where a.tpmot_nombre == tpmot_vh.tpmot_nombre || a.tpmot_id == tpmot_vh.tpmot_id
                           select a.tpmot_nombre).Count();

                if (nom == 1)
                {
                    tpmot_vh.tpmotfec_actualizacion = DateTime.Now;
                    tpmot_vh.tpmotuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(tpmot_vh).State = EntityState.Modified;
                    context.SaveChanges();
                    ConsultaDatosCreacion(tpmot_vh);
                    TempData["mensaje"] = "La actualización del tipo de motor fue exitosa!";
                    BuscarFavoritos(menu);
                    return View(tpmot_vh);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            ConsultaDatosCreacion(tpmot_vh);
            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 30);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(tpmot_vh);
        }


        public void ConsultaDatosCreacion(tpmotor_vehiculo tpmotor_vh)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tpmotor_vh.tpmotuser_id_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = context.users.Find(tpmotor_vh.tpmotuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarTiposMotorPaginados()
        {
            var data = context.tpmotor_vehiculo.ToList().Select(x => new
            {
                x.tpmot_id,
                x.tpmot_nombre,
                tpmot_estado = x.tpmot_estado ? "Activo" : "Inactivo"
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