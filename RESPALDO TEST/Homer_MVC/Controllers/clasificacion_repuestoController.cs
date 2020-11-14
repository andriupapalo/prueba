using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class clasificacion_repuestoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public ActionResult Create(int? menu)
        {
            clasificacion_repuesto claRpto = new clasificacion_repuesto
            {
                clarpto_estado = true
            };

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 43);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(claRpto);
        }

        // POST: cla_rpto/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(clasificacion_repuesto cla_rpto, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.clasificacion_repuesto
                           where a.clarpto_nombre == cla_rpto.clarpto_nombre
                           select a.clarpto_nombre).Count();

                if (nom == 0)
                {
                    cla_rpto.clarptofec_creacion = DateTime.Now;
                    cla_rpto.clarptouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.clasificacion_repuesto.Add(cla_rpto);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro de la nueva clasificacion de repuesto fue exitoso!";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 43);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(cla_rpto);
        }


        // GET: cla_rpto/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            clasificacion_repuesto cla_rpto = context.clasificacion_repuesto.Find(id);
            if (cla_rpto == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(cla_rpto.clarptouserid_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(cla_rpto.clarptouserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 43);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(cla_rpto);
        }


        // POST: cla_rpto/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(clasificacion_repuesto cla_rpto, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.clasificacion_repuesto
                           where a.clarpto_nombre == cla_rpto.clarpto_nombre || a.clarpto_id == cla_rpto.clarpto_id
                           select a.clarpto_nombre).Count();

                if (nom == 1)
                {
                    cla_rpto.clarptofec_actualizacion = DateTime.Now;
                    cla_rpto.clarptouserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(cla_rpto).State = EntityState.Modified;
                    context.SaveChanges();
                    ConsultaDatosCreacion(cla_rpto);
                    TempData["mensaje"] = "La actualización de la clasificacion de repuesto fue exitosa!";
                    BuscarFavoritos(menu);
                    return View(cla_rpto);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            ConsultaDatosCreacion(cla_rpto);
            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 43);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(cla_rpto);
        }


        public void ConsultaDatosCreacion(clasificacion_repuesto cla_rpto)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(cla_rpto.clarptouserid_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = context.users.Find(cla_rpto.clarptouserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarClasifRptoPaginadas()
        {
            //try
            //{
            //    var draw = Request.Form.GetValues("draw").FirstOrDefault();
            //    var start = Request.Form.GetValues("start").FirstOrDefault();
            //    var length = Request.Form.GetValues("length").FirstOrDefault();
            //    var search = Request.Form.GetValues("search[value]").FirstOrDefault();
            //    var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            //    var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            //    int pageSize = Convert.ToInt32(length);
            //    int skip = Convert.ToInt32(start);
            //    int totalRecords = 0;

            //    var v = from a in context.clasificacion_repuesto where a.clarpto_nombre.Contains(search) select a;

            //    totalRecords = v.Count();
            //    var dataAux = v.OrderBy(x => x.clarpto_nombre).Skip(skip).Take(pageSize).ToList();
            //    var data = dataAux.Select(x=>new {
            //        x.clarpto_nombre,
            //        x.clarpto_estado,
            //        x.clarpto_id
            //    });
            //    return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);
            //}
            //catch (Exception ex)
            //{
            //    // Exception
            //}
            ///////////////////////////////////////////////////
            var data = context.clasificacion_repuesto.ToList().Select(x => new
            {
                x.clarpto_id,
                x.clarpto_nombre,
                clarpto_estado = x.clarpto_estado ? "Activo" : "Inactivo"
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