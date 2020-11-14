using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class claseModeloController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public void ParametrosVista()
        {
            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 152).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 152);
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
            vclase crearClase = new vclase
            {
                estado = true
            };
            ParametrosVista();
            BuscarFavoritos(menu);
            return View(crearClase);
        }


        // POST: col_vh/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(vclase clase, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.vclase
                           where a.nombre == clase.nombre || a.clase_id == clase.clase_id
                           select a.nombre).Count();

                if (nom == 0)
                {
                    clase.fec_creacion = DateTime.Now;
                    clase.usuario_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.vclase.Add(clase);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro de la nueva clase fue exitoso!";
                    BuscarFavoritos(menu);
                    return View(clase);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";
            ParametrosVista();
            BuscarFavoritos(menu);
            return View(clase);
        }


        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            vclase clase = context.vclase.Find(id);
            if (clase == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(clase.usuario_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(clase.usuario_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }

            ParametrosVista();
            BuscarFavoritos(menu);
            return View(clase);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(vclase clase, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.vclase
                           where a.nombre == clase.nombre && a.clase_id == clase.clase_id
                           select a.nombre).Count();

                if (nom == 1)
                {
                    clase.fec_actualizacion = DateTime.Now;
                    clase.usuario_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(clase).State = EntityState.Modified;
                    context.SaveChanges();
                    ConsultaDatosCreacion(clase);
                    TempData["mensaje"] = "La actualización de la clase fue exitoso!";
                    BuscarFavoritos(menu);
                    return View(clase);
                }

                {
                    int nom2 = (from a in context.vclase
                                where a.nombre == clase.nombre
                                select a.nombre).Count();
                    if (nom2 == 0)
                    {
                        clase.fec_actualizacion = DateTime.Now;
                        clase.usuario_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        context.Entry(clase).State = EntityState.Modified;
                        context.SaveChanges();
                        ConsultaDatosCreacion(clase);
                        TempData["mensaje"] = "La actualización de la clase fue exitoso!";
                        BuscarFavoritos(menu);
                        return View(clase);
                    }

                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                }
            }

            ConsultaDatosCreacion(clase);
            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";
            ParametrosVista();
            BuscarFavoritos(menu);
            return View(clase);
        }


        public void ConsultaDatosCreacion(vclase clase)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(clase.usuario_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(clase.usuario_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult GetClasesJson()
        {
            var buscarClases = context.vclase.ToList().Select(x => new
            {
                x.clase_id,
                x.nombre,
                estado = x.estado ? "Activo" : "Inactivo"
            });
            return Json(buscarClases, JsonRequestBehavior.AllowGet);
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