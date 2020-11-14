using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class zonasPeritajeController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: zonasPeritaje
        public ActionResult Crear(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Crear(icb_zonaperitaje zona, int? menu)
        {
            if (ModelState.IsValid)
            {
                icb_zonaperitaje buscarZona = context.icb_zonaperitaje.FirstOrDefault(x => x.zonaper_nombre == zona.zonaper_nombre);
                if (buscarZona == null)
                {
                    zona.zonaperfec_creacion = DateTime.Now;
                    zona.zonaperuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.icb_zonaperitaje.Add(zona);

                    context.SaveChanges();
                    TempData["mensaje"] = "La Zona " + zona.zonaper_nombre + " se creo correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "La zona " + zona.zonaper_nombre + " ya existe";
                }
            }

            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Update(icb_zonaperitaje zona, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.icb_zonaperitaje
                           where a.zonaper_nombre == zona.zonaper_nombre || a.zonaper_id == zona.zonaper_id
                           select a.zonaper_nombre).Count();
                if (nom == 1)
                {
                    zona.zonaperfec_actualizacion = DateTime.Now;
                    zona.zonaperuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(zona).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización de la zona fue exitoso";
                }
                else
                {
                    TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
                }
            }

            BuscarFavoritos(menu);
            return View(zona);
        }

        // GET: piezas/Edit/5
        public ActionResult Update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_zonaperitaje zona = context.icb_zonaperitaje.Find(id);
            if (zona == null)
            {
                return HttpNotFound();
            }

            BuscarFavoritos(menu);
            return View(zona);
        }


        public JsonResult BuscarZonas()
        {
            var zonas = context.icb_zonaperitaje.Select(x => new
            {
                x.zonaper_id,
                x.zonaper_nombre
            });

            var data = new
            {
                zonas
            };

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