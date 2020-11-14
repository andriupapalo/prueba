using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tipoModeloController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public void ParametrosVista()
        {
            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 155).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 155);
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
            vtipo crearTipo = new vtipo
            {
                estado = true
            };
            ParametrosVista();
            BuscarFavoritos(menu);
            return View(crearTipo);
        }


        // POST: col_vh/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(vtipo tipo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.vtipo
                           where a.nombre == tipo.nombre || a.tipo_id == tipo.tipo_id
                           select a.nombre).Count();

                if (nom == 0)
                {
                    tipo.fec_creacion = DateTime.Now;
                    tipo.usuario_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.vtipo.Add(tipo);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro del nuevo tipo de modelo fue exitoso!";
                    BuscarFavoritos(menu);
                    return View(tipo);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";
            ParametrosVista();
            BuscarFavoritos(menu);
            return View(tipo);
        }


        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            vtipo tipo = context.vtipo.Find(id);
            if (tipo == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tipo.usuario_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(tipo.usuario_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }

            ParametrosVista();
            BuscarFavoritos(menu);
            return View(tipo);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(vtipo tipo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.vtipo
                           where a.nombre == tipo.nombre && a.tipo_id == tipo.tipo_id
                           select a.nombre).Count();

                if (nom == 1)
                {
                    tipo.fec_actualizacion = DateTime.Now;
                    tipo.usuario_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(tipo).State = EntityState.Modified;
                    context.SaveChanges();
                    ConsultaDatosCreacion(tipo);
                    TempData["mensaje"] = "La actualización del tipo de modelo fue exitoso!";
                    BuscarFavoritos(menu);
                    return View(tipo);
                }

                {
                    int nom2 = (from a in context.vtipo
                                where a.nombre == tipo.nombre
                                select a.nombre).Count();
                    if (nom2 == 0)
                    {
                        tipo.fec_actualizacion = DateTime.Now;
                        tipo.usuario_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        context.Entry(tipo).State = EntityState.Modified;
                        context.SaveChanges();
                        ConsultaDatosCreacion(tipo);
                        TempData["mensaje"] = "La actualización del tipo de modelo fue exitoso!";
                        BuscarFavoritos(menu);
                        return View(tipo);
                    }

                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                }
            }

            ConsultaDatosCreacion(tipo);
            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";
            ParametrosVista();
            BuscarFavoritos(menu);
            return View(tipo);
        }


        public void ConsultaDatosCreacion(vtipo tipo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tipo.usuario_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(tipo.usuario_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult GetTiposJson()
        {
            var buscarTipos = context.vtipo.ToList().Select(x => new
            {
                x.tipo_id,
                x.nombre,
                estado = x.estado ? "Activo" : "Inactivo"
            });
            return Json(buscarTipos, JsonRequestBehavior.AllowGet);
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