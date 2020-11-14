using Homer_MVC.IcebergModel;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class clasificacionClienteController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        // GET: clasificacionCliente
        public ActionResult Crear(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Crear(clasificacion_cliente modelo, int? menu)
        {
            modelo.fec_creacion = DateTime.Now;
            modelo.user_creacion = Convert.ToInt32(Session["user_usuarioid"]);
            if (ModelState.IsValid)
            {
                context.clasificacion_cliente.Add(modelo);
                context.SaveChanges();
            }

            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult editar(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult editar(clasificacion_cliente modelo, int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult datos()
        {
            System.Collections.Generic.List<clasificacion_cliente> buscar = context.clasificacion_cliente.ToList();
            return Json(buscar, JsonRequestBehavior.AllowGet);
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