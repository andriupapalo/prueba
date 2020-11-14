using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class ftipo_excepcionesController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        private void parametrosBusqueda()
        {
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 119);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
        }

        // GET: ftipo_excepciones/Create
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        // POST: ftipo_excepciones/Create
        [HttpPost]
        public ActionResult Create(ftipo_excepciones modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.ftipo_excepciones
                           where a.cod == modelo.cod && a.descripcion == modelo.descripcion
                           select a.descripcion).Count();

                if (nom == 0)
                {
                    modelo.fec_creacion = DateTime.Now;
                    modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.ftipo_excepciones.Add(modelo);
                    bool guardar = context.SaveChanges() > 0;
                    if (guardar)
                    {
                        parametrosBusqueda();
                        TempData["mensaje"] = "El registro de la nueva excepción fue exitoso!";
                        BuscarFavoritos(menu);
                        return View();
                    }

                    TempData["mensaje_error"] = "Error de conexion!";
                }
                else
                {
                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                }
            }

            parametrosBusqueda();
            BuscarFavoritos(menu);
            return View();
        }

        // GET: ftipo_excepciones/update/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ftipo_excepciones excepcion = context.ftipo_excepciones.Find(id);
            if (excepcion == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from a in context.users
                                        join b in context.ftipo_excepciones
                                            on a.user_id equals b.userid_creacion
                                        where b.id == id
                                        select a.user_nombre;
            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }
            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result1 = from a in context.users
                                         join b in context.ftipo_excepciones on a.user_id equals b.userid_actualizacion
                                         where b.id == id
                                         select a.user_nombre;
            foreach (string i in result1)
            {
                ViewBag.user_nombre_act = i;
            }

            parametrosBusqueda();
            BuscarFavoritos(menu);
            return View(excepcion);
        }

        // POST: ftipo_excepciones/update/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(ftipo_excepciones modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.ftipo_excepciones
                           where a.cod == modelo.cod || a.descripcion == modelo.descripcion
                           select a.descripcion).Count();

                if (nom == 1)
                {
                    modelo.fec_actualizacion = DateTime.Now;
                    modelo.userid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(modelo).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización de la excepción fue exitosa!";
                    parametrosBusqueda();
                    ConsultaDatosCreacion(modelo);
                    BuscarFavoritos(menu);
                    return View(modelo);
                }

                TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
            }

            parametrosBusqueda();
            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }

        public void ConsultaDatosCreacion(ftipo_excepciones modelo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(modelo.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(modelo.userid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult BuscarDatos()
        {
            var data = (from e in context.ftipo_excepciones
                        select new
                        {
                            e.id,
                            e.cod,
                            e.descripcion,
                            estado = e.estado ? "Activo" : "Inactivo"
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