using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class grupoModeloController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        public void ParametrosVista()
        {
            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 153).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 153);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
        }


        // GET: tipoModelo
        public ActionResult Crear(int? menu)
        {
            vgrupo crearGrupo = new vgrupo
            {
                estado = true
            };
            ParametrosVista();
            BuscarFavoritos(menu);
            return View(crearGrupo);
        }


        // POST: col_vh/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(vgrupo grupo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.vgrupo
                           where a.nombre == grupo.nombre || a.grupo_id == grupo.grupo_id
                           select a.nombre).Count();

                if (nom == 0)
                {
                    grupo.fec_creacion = DateTime.Now;
                    grupo.usuario_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.vgrupo.Add(grupo);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro del nuevo grupo fue exitoso!";
                    BuscarFavoritos(menu);
                    return View(grupo);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";
            ParametrosVista();
            BuscarFavoritos(menu);
            return View(grupo);
        }


        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            vgrupo grupo = context.vgrupo.Find(id);
            if (grupo == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(grupo.usuario_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(grupo.usuario_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }

            ParametrosVista();
            BuscarFavoritos(menu);
            return View(grupo);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(vgrupo grupo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.vgrupo
                           where a.nombre == grupo.nombre && a.grupo_id == grupo.grupo_id
                           select a.nombre).Count();

                if (nom == 1)
                {
                    grupo.fec_actualizacion = DateTime.Now;
                    grupo.usuario_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(grupo).State = EntityState.Modified;
                    context.SaveChanges();
                    ConsultaDatosCreacion(grupo);
                    TempData["mensaje"] = "La actualización del grupo fue exitoso!";
                    BuscarFavoritos(menu);
                    return View(grupo);
                }

                {
                    int nom2 = (from a in context.vgrupo
                                where a.nombre == grupo.nombre
                                select a.nombre).Count();
                    if (nom2 == 0)
                    {
                        grupo.fec_actualizacion = DateTime.Now;
                        grupo.usuario_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        context.Entry(grupo).State = EntityState.Modified;
                        context.SaveChanges();
                        ConsultaDatosCreacion(grupo);
                        TempData["mensaje"] = "La actualización del grupo fue exitoso!";
                        BuscarFavoritos(menu);
                        return View(grupo);
                    }

                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                }
            }

            ConsultaDatosCreacion(grupo);
            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";
            ParametrosVista();
            BuscarFavoritos(menu);
            return View(grupo);
        }


        public void ConsultaDatosCreacion(vgrupo grupo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(grupo.usuario_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(grupo.usuario_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult GetGruposJson()
        {
            var buscarGrupos = context.vgrupo.ToList().Select(x => new
            {
                x.grupo_id,
                x.nombre,
                estado = x.estado ? "Activo" : "Inactivo"
            });
            return Json(buscarGrupos, JsonRequestBehavior.AllowGet);
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