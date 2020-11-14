using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tipoTramiteController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public void enlacesModulo()
        {
            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 23).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 23);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
        }


        // GET: tipoTramite
        public ActionResult Crear(int? menu)
        {
            icb_tptramite_prospecto crearTipoTramite = new icb_tptramite_prospecto
            {
                tptrapros_estado = true,
                tptrapros_razoninactivo = "No aplica"
            };
            enlacesModulo();
            BuscarFavoritos(menu);
            return View(crearTipoTramite);
        }

        [HttpPost]
        public ActionResult Crear(icb_tptramite_prospecto modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.icb_tptramite_prospecto
                           where a.tptrapros_descripcion == modelo.tptrapros_descripcion ||
                                 a.tptrapros_id == modelo.tptrapros_id
                           select a.tptrapros_descripcion).Count();

                if (nom == 0)
                {
                    modelo.tptrapros_feccreacion = DateTime.Now;
                    modelo.tptrapros_usucreacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.icb_tptramite_prospecto.Add(modelo);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro del nuevo tipo de tramite fue exitoso!";
                    BuscarFavoritos(menu);
                    return View(modelo);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            enlacesModulo();
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_tptramite_prospecto tpTramite = context.icb_tptramite_prospecto.Find(id);
            if (tpTramite == null)
            {
                return HttpNotFound();
            }

            ConsultarDatosCreacion(tpTramite);
            enlacesModulo();
            BuscarFavoritos(menu);
            return View(tpTramite);
        }


        [HttpPost]
        public ActionResult update(icb_tptramite_prospecto modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.icb_tptramite_prospecto
                           where /*a.tptrapros_descripcion == modelo.tptrapros_descripcion &&*/
                               a.tptrapros_id == modelo.tptrapros_id
                           select a.tptrapros_descripcion).Count();

                if (nom == 1)
                {
                    modelo.tptrapros_fecactualizacion = DateTime.Now;
                    modelo.tptrapros_usuactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(modelo).State = EntityState.Modified;
                    context.SaveChanges();
                    ConsultarDatosCreacion(modelo);
                    TempData["mensaje"] = "La actualización del tipo de tramite fue exitoso!";
                    BuscarFavoritos(menu);
                    return View(modelo);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            ConsultarDatosCreacion(modelo);
            enlacesModulo();
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public JsonResult GetTipoTramitesJson()
        {
            var buscarTipoTramites = context.icb_tptramite_prospecto.ToList().Select(x => new
            {
                x.tptrapros_id,
                x.tptrapros_descripcion,
                tptrapros_estado = x.tptrapros_estado ? "Activo" : "Inactivo"
            });
            return Json(buscarTipoTramites, JsonRequestBehavior.AllowGet);
        }


        public void ConsultarDatosCreacion(icb_tptramite_prospecto modelo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(modelo.tptrapros_usucreacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(modelo.tptrapros_usuactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
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