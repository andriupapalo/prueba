using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tipoBahiaController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        // GET: tipoBahia
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View(new ttipobahia { estado = true });
        }


        // POST: tipoBahia
        [HttpPost]
        public ActionResult Create(ttipobahia modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                ttipobahia buscarPorNombre = context.ttipobahia.FirstOrDefault(x => x.descripcion == modelo.descripcion);
                if (buscarPorNombre != null)
                {
                    TempData["mensaje_error"] = "El tipo de bahia ya existe, por favor valide";
                }
                else
                {
                    modelo.fec_creacion = DateTime.Now;
                    modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.ttipobahia.Add(modelo);
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "La creación del tipo de bahia fue exitoso";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                    }
                }
            }

            BuscarFavoritos(menu);
            return View();
        }


        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ttipobahia tipoBahia = context.ttipobahia.Find(id);
            if (tipoBahia == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(tipoBahia);
            BuscarFavoritos(menu);
            return View(tipoBahia);
        }


        [HttpPost]
        public ActionResult Edit(ttipobahia modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                ttipobahia buscarPorNombre = context.ttipobahia.FirstOrDefault(x => x.descripcion == modelo.descripcion);
                if (buscarPorNombre != null)
                {
                    if (buscarPorNombre.id != modelo.id)
                    {
                        TempData["mensaje_error"] = "El tipo de bahia ya existe, por favor valide";
                    }
                    else
                    {
                        modelo.fec_actualizacion = DateTime.Now;
                        modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarPorNombre.fec_actualizacion = DateTime.Now;
                        buscarPorNombre.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarPorNombre.estado = modelo.estado;
                        buscarPorNombre.razon_inactivo = modelo.razon_inactivo;
                        context.Entry(buscarPorNombre).State = EntityState.Modified;
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            TempData["mensaje"] = "La actualización del tipo de bahia fue exitoso";
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                        }
                    }
                }
                else
                {
                    modelo.fec_actualizacion = DateTime.Now;
                    modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(modelo).State = EntityState.Modified;
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "La actualización del tipo de bahia fue exitoso";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                    }
                }
            }

            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public void ConsultaDatosCreacion(ttipobahia tipoBahia)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tipoBahia.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(tipoBahia.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarTiposBahias()
        {
            var buscarTiposBahias = (from tipoBahia in context.ttipobahia
                                     select new
                                     {
                                         tipoBahia.id,
                                         tipoBahia.descripcion,
                                         estado = tipoBahia.estado ? "Activo" : "Inactivo"
                                     }).ToList();

            return Json(buscarTiposBahias, JsonRequestBehavior.AllowGet);
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