using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class MotivosDesistidoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: MotivosDesistido
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Create(vcredmotdesistio post, int? menu)
        {
            if (ModelState.IsValid)
            {
                vcredmotdesistio buscarNombre = context.vcredmotdesistio.FirstOrDefault(x => x.motivo == post.motivo);
                if (buscarNombre == null)
                {
                    post.fec_creacion = DateTime.Now;
                    post.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.vcredmotdesistio.Add(post);
                    context.SaveChanges();
                    TempData["mensaje"] = "El motivo " + post.motivo + " se creo correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "El motivo " + post.motivo + " ya existe";
                }
            }

            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult Update(int? menu, int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            vcredmotdesistio modelo = context.vcredmotdesistio.Find(id);
            if (modelo == null)
            {
                return HttpNotFound();
            }

            BuscarFavoritos(menu);
            return View(modelo);
        }

        [HttpPost]
        public ActionResult Update(vcredmotdesistio post, int? menu)
        {
            if (ModelState.IsValid)
            {
                vcredmotdesistio buscar = context.vcredmotdesistio.Find(post.id);

                buscar.motivo = post.motivo;
                buscar.estado = post.estado;
                buscar.razon_inactivo = post.razon_inactivo;
                buscar.fec_actualizacion = DateTime.Now;
                buscar.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                context.Entry(buscar).State = EntityState.Modified;
                context.SaveChanges();
                TempData["mensaje"] = "La actualización del motivo fue exitosa!";
            }
            else
            {
                TempData["mensaje_error"] = "No se pudo modificar el motivo, por favor valide!";
            }

            BuscarFavoritos(menu);
            return View(post);
        }

        public JsonResult Browser()
        {
            System.Collections.Generic.List<vcredmotdesistio> buscar = context.vcredmotdesistio.ToList();

            var data = buscar.Select(x => new
            {
                x.id,
                x.motivo,
                estado = x.estado ? "Activo" : "Inactivo"
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