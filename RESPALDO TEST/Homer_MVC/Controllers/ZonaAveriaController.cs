using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class ZonaAveriaController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: ZonaAveria
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            ViewBag.id_menu = menu;
            ZonaAveriaModel model = new ZonaAveriaModel
            {
                zona_estado = true,
                zona_razon_inactivo = ""
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(ZonaAveriaModel zona, int? menu)
        {
            if (ModelState.IsValid)
            {
                icb_zona_averia buscarNombre =
                    context.icb_zona_averia.FirstOrDefault(x => x.zona_descripcion == zona.zona_descripcion);
                if (buscarNombre == null)
                {
                    icb_zona_averia guardarzona = new icb_zona_averia
                    {
                        zona_licencia = zona.zona_licencia,
                        zona_descripcion = zona.zona_descripcion,
                        zona_estado = zona.zona_estado,
                        zona_razon_inactivo = zona.zona_razon_inactivo
                    };
                    guardarzona.zona_fec_creacion = DateTime.Now;
                    guardarzona.zona_userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.icb_zona_averia.Add(guardarzona);
                    context.SaveChanges();
                    ViewBag.id_menu = menu;
                    TempData["mensaje"] = "El dato ingresado se ha creado correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "El dato ingresado ya existe";
                    BuscarFavoritos(menu);
                    return View(zona);
                }
            }

            return View();
        }

        public ActionResult Update(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_zona_averia zona2 = context.icb_zona_averia.Find(id);
            if (zona2 == null)
            {
                return HttpNotFound();
            }

            ZonaAveriaModel zona = new ZonaAveriaModel
            {
                zona_descripcion = zona2.zona_descripcion,
                zona_id = zona2.zona_id,
                zona_razon_inactivo = zona2.zona_razon_inactivo,
                zona_estado = zona2.zona_estado
            };
            IQueryable<string> userCre = from a in context.users
                                         join b in context.icb_zona_averia on a.user_id equals b.zona_userid_creacion
                                         where b.zona_id == id
                                         select a.user_nombre;

            foreach (string userCreacion in userCre)
            {
                ViewBag.user_nombre_cre = userCreacion;
            }

            IQueryable<string> userAct = from a in context.users
                                         join b in context.icb_zona_averia on a.user_id equals b.zona_userid_actualizacion
                                         where b.zona_id == id
                                         select a.user_nombre;

            foreach (string userActualizacion in userAct)
            {
                ViewBag.user_nombre_act = userActualizacion;
            }

            BuscarFavoritos(menu);
            return View(zona);
        }

        [HttpPost]
        public ActionResult Update(ZonaAveriaModel zona, int? menu)
        {
            if (ModelState.IsValid)
            {
                int nom = (from a in context.icb_zona_averia
                           where a.zona_descripcion == zona.zona_descripcion || a.zona_id == zona.zona_id
                           select a.zona_descripcion).Count();
                if (nom == 1)
                {
                    icb_zona_averia zona2 = context.icb_zona_averia.Find(zona.zona_id);
                    zona2.zona_descripcion = zona.zona_descripcion;
                    zona2.zona_estado = zona.zona_estado;
                    zona2.zona_razon_inactivo = zona.zona_razon_inactivo;
                    string inactivo = Request["zona_razon_inactivo"];
                    zona2.zona_fec_actualizacion = DateTime.Now;
                    zona2.zona_userid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(zona2).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización de la averia fue exitosa!";
                    BuscarFavoritos(menu);
                    IQueryable<string> userCre = from a in context.users
                                                 join b in context.icb_zona_averia on a.user_id equals b.zona_userid_creacion
                                                 where b.zona_id == zona.zona_id
                                                 select a.user_nombre;

                    foreach (string userCreacion in userCre)
                    {
                        ViewBag.user_nombre_cre = userCreacion;
                    }

                    IQueryable<string> userAct = from a in context.users
                                                 join b in context.icb_zona_averia on a.user_id equals b.zona_userid_actualizacion
                                                 where b.zona_id == zona.zona_id
                                                 select a.user_nombre;

                    foreach (string userActualizacion in userAct)
                    {
                        ViewBag.user_nombre_act = userActualizacion;
                    }

                    return View(zona);
                }

                TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
            }

            BuscarFavoritos(menu);
            return View(zona);
        }

        public ActionResult BuscarZonaAveriasPaginados()
        {
            var data = context.icb_zona_averia.ToList().Select(x => new
            {
                x.zona_id,
                x.zona_descripcion,
                zona_estado = x.zona_estado ? "Activo" : "Inactivo"
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