using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class asignacionPlantasController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: asignacionPlantas
        public ActionResult Index(int? menu)
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            ViewBag.users = (from bodega in context.bodega_usuario
                             join usuario in context.users
                                 on bodega.id_usuario equals usuario.user_id
                             where bodega.id_bodega == bodegaActual && (usuario.rol_id == 4 || usuario.rol_id == 2030)
                             select usuario).OrderBy(x => x.user_nombre).ToList();

            BuscarFavoritos(menu);
            return View();
        }


        [HttpPost]
        public ActionResult Index(DateTime? fechaDesde, DateTime? fechaHasta, int? menu)
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            string usuarios = Request["users"];
            //DateTime inicio = new DateTime();
            if (!string.IsNullOrEmpty(usuarios))
            {
                string[] usuarioId = usuarios.Split(',');
                foreach (string substring in usuarioId)
                {
                    int id_usuario = !string.IsNullOrEmpty(substring) ? Convert.ToInt32(substring) : 0;
                    users buscarUsuario = context.users.FirstOrDefault(x => x.user_id == id_usuario);

                    if (buscarUsuario != null)
                    {
                        buscarUsuario.fechainiplanta = fechaDesde;
                        buscarUsuario.fechafinplanta = fechaHasta;
                        context.Entry(buscarUsuario).State = EntityState.Modified;
                    }
                }

                int guardar = context.SaveChanges();
                if (guardar > 0)
                {
                    TempData["mensaje"] = "Los usuarios han sido actualizados con sus respectivas fechas de planta!";
                }
            }
            else
            {
                TempData["mensaje_error"] = "No ha seleccionado ningun usuario!";
            }

            ViewBag.users = from user in context.users
                            join bodegas in context.bodega_usuario
                                on user.user_id equals bodegas.id_usuario
                            where bodegas.id_bodega == bodegaActual
                            orderby user.user_nombre
                            select user;
            BuscarFavoritos(menu);
            return View();
        }


        public JsonResult BuscarUsuariosPlanta()
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            var buscarPlantas = (from user in context.users
                                 join bodegas in context.bodega_usuario
                                     on user.user_id equals bodegas.id_usuario
                                 where bodegas.id_bodega == bodegaActual && (user.rol_id == 4 || user.rol_id == 2030)
                                 orderby user.user_nombre
                                 select new
                                 {
                                     user.user_id,
                                     user.user_nombre,
                                     user.user_apellido,
                                     user.user_usuario,
                                     user.fechainiplanta,
                                     user.fechafinplanta
                                 }).ToList();

            var plantas = buscarPlantas.Select(x => new
            {
                x.user_id,
                x.user_nombre,
                x.user_apellido,
                x.user_usuario,
                fechaPlantaInicio = x.fechainiplanta != null ? x.fechainiplanta.Value.ToShortDateString() : "",
                fechaPlantaFin = x.fechafinplanta != null ? x.fechafinplanta.Value.ToShortDateString() : ""
            }).ToList();

            return Json(plantas, JsonRequestBehavior.AllowGet);
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