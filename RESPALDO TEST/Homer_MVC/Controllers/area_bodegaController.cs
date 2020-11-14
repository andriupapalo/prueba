using Homer_MVC.IcebergModel;
using Homer_MVC.ViewModels.medios;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class area_bodegaController : Controller
    {
        //private VehiculosContexto context = new VehiculosContexto();
        private readonly Iceberg_Context context = new Iceberg_Context();

        public ActionResult Create(int? menu)
        {
            area_bodega createArea = new area_bodega
            {
                areabod_estado = true
            };

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 64);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            int bodega = Convert.ToInt32(Session["user_bodega"]);
            int rol = Convert.ToInt32(Session["user_rolid"]);


            var bod = context.bodega_concesionario.Where(x => x.bodccs_estado && x.id == bodega)
                .OrderBy(x => x.bodccs_nombre).Select(x => new
                {
                    value = x.id,
                    text = x.bodccs_nombre
                }).ToList();

            int permiso = context.opcion_acceso_rol.Where(d => d.id_rol == rol && d.opcion_acceso.codigo == "OA66").Count();
            if (rol == 1 || permiso > 0)
            {
                bod = context.bodega_concesionario.Where(x => x.bodccs_estado)
                .OrderBy(x => x.bodccs_nombre).Select(x => new
                {
                    value = x.id,
                    text = x.bodccs_nombre
                }).ToList();
            }
            ViewBag.id_bodega = new SelectList(bod, "value", "text");
            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(createArea);
        }


        // POST: area_bod/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(area_bodega area_bod, int? menu)
        {
            int bodega = Convert.ToInt32(Session["user_bodega"]);
            int rol = Convert.ToInt32(Session["user_rolid"]);
            int permiso = context.opcion_acceso_rol.Where(d => d.id_rol == rol && d.opcion_acceso.codigo == "OA66").Count();

            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD

                int nom = (from a in context.area_bodega
                           where a.areabod_nombre == area_bod.areabod_nombre && a.id_bodega == bodega
                           select a.areabod_nombre).Count();

                if (nom == 0)
                {
                    //area_bod.id_bodega = Convert.ToInt32(Request["bodegas"]);
                    area_bod.areabodfec_creacion = DateTime.Now;
                    area_bod.areaboduserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.area_bodega.Add(area_bod);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro de la nueva area de bodega fue exitoso!";
                    var bode = context.bodega_concesionario.Where(x => x.bodccs_estado && x.id == bodega)
                        .OrderBy(x => x.bodccs_nombre).Select(x => new
                        {
                            value = x.id,
                            text = x.bodccs_nombre
                        }).ToList();
                    if (rol == 1 || permiso > 0)
                    {
                        bode = context.bodega_concesionario.Where(x => x.bodccs_estado)
                        .OrderBy(x => x.bodccs_nombre).Select(x => new
                        {
                            value = x.id,
                            text = x.bodccs_nombre
                        }).ToList();
                    }
                    ViewBag.id_bodega = new SelectList(bode, "value", "text");
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 64);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            var bod = context.bodega_concesionario.Where(x => x.bodccs_estado && x.id == bodega)
                .OrderBy(x => x.bodccs_nombre).Select(x => new
                {
                    value = x.id,
                    text = x.bodccs_nombre
                }).ToList();
            if (rol == 1 || permiso > 0)
            {
                bod = context.bodega_concesionario.Where(x => x.bodccs_estado)
                .OrderBy(x => x.bodccs_nombre).Select(x => new
                {
                    value = x.id,
                    text = x.bodccs_nombre
                }).ToList();
            }
            ViewBag.id_bodega = new SelectList(bod, "value", "text");
            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(area_bod);
        }


        // GET:area_bod/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            int bodega = Convert.ToInt32(Session["user_bodega"]);
            int rol = Convert.ToInt32(Session["user_rolid"]);
            int permiso = context.opcion_acceso_rol.Where(d => d.id_rol == rol && d.opcion_acceso.codigo == "OA66").Count();

            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            area_bodega area_bod = context.area_bodega.Find(id);
            if (area_bod == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(area_bod.areaboduserid_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(area_bod.areaboduserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 64);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            var bod = context.bodega_concesionario.Where(x => x.bodccs_estado && x.id == bodega)
                .OrderBy(x => x.bodccs_nombre).Select(x => new
                {
                    value = x.id,
                    text = x.bodccs_nombre
                }).ToList();
            if (rol == 1 || permiso > 0)
            {
                bod = context.bodega_concesionario.Where(x => x.bodccs_estado)
                .OrderBy(x => x.bodccs_nombre).Select(x => new
                {
                    value = x.id,
                    text = x.bodccs_nombre
                }).ToList();
            }
            ViewBag.id_bodega = new SelectList(bod, "value", "text", area_bod.id_bodega);
            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(area_bod);
        }


        // POST: area_bod/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(area_bodega area_bod, int? menu)
        {
            int rol = Convert.ToInt32(Session["user_rolid"]);
            int permiso = context.opcion_acceso_rol.Where(d => d.id_rol == rol && d.opcion_acceso.codigo == "OA66").Count();
            int bodega = Convert.ToInt32(Session["user_bodega"]);

            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta

                int nom = (from a in context.area_bodega
                           where a.areabod_nombre == area_bod.areabod_nombre && a.id_bodega == area_bod.id_bodega ||
                                 a.areabod_id == area_bod.areabod_id
                           select a.areabod_nombre).Count();

                if (nom == 1)
                {
                    area_bod.areabodfec_actualizacion = DateTime.Now;
                    area_bod.areaboduserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);

                    context.Entry(area_bod).State = EntityState.Modified;
                    context.SaveChanges();
                    ConsultaDatosCreacion(area_bod);
                    var bod = context.bodega_concesionario.Where(x => x.bodccs_estado && x.id == bodega)
                        .OrderBy(x => x.bodccs_nombre).Select(x => new
                        {
                            value = x.id,
                            text = x.bodccs_nombre
                        }).ToList();
                    if (rol == 1 || permiso > 0)
                    {
                        bod = context.bodega_concesionario.Where(x => x.bodccs_estado)
                        .OrderBy(x => x.bodccs_nombre).Select(x => new
                        {
                            value = x.id,
                            text = x.bodccs_nombre
                        }).ToList();
                    }
                    ViewBag.id_bodega = new SelectList(bod, "value", "text", area_bod.id_bodega);
                    TempData["mensaje"] = "La actualización de la area de bodega fue exitosa!";
                    BuscarFavoritos(menu);
                    return View(area_bod);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }
            var bode = context.bodega_concesionario.Where(x => x.bodccs_estado && x.id == bodega)
                        .OrderBy(x => x.bodccs_nombre).Select(x => new
                        {
                            value = x.id,
                            text = x.bodccs_nombre
                        }).ToList();
            if (rol == 1 || permiso > 0)
            {
                bode = context.bodega_concesionario.Where(x => x.bodccs_estado)
                .OrderBy(x => x.bodccs_nombre).Select(x => new
                {
                    value = x.id,
                    text = x.bodccs_nombre
                }).ToList();
            }
            ViewBag.id_bodega = new SelectList(bode, "value", "text", area_bod.id_bodega);
            ConsultaDatosCreacion(area_bod);
            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 64);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(area_bod);
        }


        public void ConsultaDatosCreacion(area_bodega area_bodega)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(area_bodega.areaboduserid_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = context.users.Find(area_bodega.areaboduserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarAreasBodegaPaginadas()
        {
            int bodega = Convert.ToInt32(Session["user_bodega"]);
            int rol = Convert.ToInt32(Session["user_rolid"]);
            System.Linq.Expressions.Expression<Func<area_bodega, bool>> predicado = PredicateBuilder.True<area_bodega>();
            int permiso = context.opcion_acceso_rol.Where(d => d.id_rol == rol && d.opcion_acceso.codigo == "OA66").Count();
            if (rol == 1 || permiso > 0)
            {
            }
            else
            {
                predicado = predicado.And(d => d.id_bodega == bodega);
            }
            System.Collections.Generic.List<area_bodega> datos = context.area_bodega.Where(predicado).ToList();
            var data = datos.Select(x => new
            {
                x.areabod_id,
                bodega = x.bodega_concesionario.bodccs_nombre,
                bod = x.id_bodega,
                x.areabod_nombre,
                areabod_estado = x.areabod_estado ? "Activo" : "Inactivo"
            }).ToList();
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