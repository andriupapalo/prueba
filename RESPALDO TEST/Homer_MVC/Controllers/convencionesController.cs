using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class convencionesController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: piezasPeritaje
        public ActionResult Crear(int? menu)
        {
            ViewBag.pieza_id = new SelectList(context.icb_piezaperitaje.OrderBy(x => x.pieza_nombre), "pieza_id",
                "pieza_nombre");
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Crear(icb_conveperitaje conve, int? menu)
        {
            if (ModelState.IsValid)
            {
                icb_conveperitaje buscarConve = context.icb_conveperitaje.FirstOrDefault(x => x.conve_nombre == conve.conve_nombre);
                if (buscarConve == null)
                {
                    conve.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    conve.fec_creacion = DateTime.Now;
                    context.icb_conveperitaje.Add(conve);
                    context.SaveChanges();
                    TempData["mensaje"] = "La convención " + conve.conve_nombre + " se creo correctamente";
                    ViewBag.pieza_id = new SelectList(context.icb_piezaperitaje.OrderBy(x => x.pieza_nombre),
                        "pieza_id", "pieza_nombre");
                    BuscarFavoritos(menu);
                    return RedirectToAction("Crear", new { menu });
                }

                TempData["mensaje_error"] = "La convención " + conve.conve_nombre + " ya existe";
            }

            ViewBag.pieza_id = new SelectList(context.icb_piezaperitaje.OrderBy(x => x.pieza_nombre), "pieza_id",
                "pieza_nombre");
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Update(icb_conveperitaje conve, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.icb_conveperitaje
                           where a.conve_nombre == conve.conve_nombre || a.conve_id == conve.conve_id
                           select a.conve_nombre).Count();
                if (nom == 1)
                {
                    conve.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    conve.fec_actualizacion = DateTime.Now;
                    context.Entry(conve).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización de la convención fue exitoso!";
                }
                else
                {
                    TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
                }
            }

            ViewBag.pieza_id = new SelectList(context.icb_piezaperitaje.OrderBy(x => x.pieza_nombre), "pieza_id",
                "pieza_nombre");
            BuscarFavoritos(menu);
            return View(conve);
        }

        // GET: piezas/Edit/5
        public ActionResult Update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_conveperitaje conve = context.icb_conveperitaje.Find(id);
            if (conve == null)
            {
                ViewBag.pieza_id = new SelectList(context.icb_piezaperitaje.OrderBy(x => x.pieza_nombre), "pieza_id",
                    "pieza_nombre");
                return HttpNotFound();
            }

            ViewBag.pieza_id = new SelectList(context.icb_piezaperitaje.OrderBy(x => x.pieza_nombre), "pieza_id",
                "pieza_nombre");
            BuscarFavoritos(menu);
            return View(conve);
        }

        public JsonResult BuscarConvenciones()
        {
            var conve = from c in context.icb_conveperitaje
                        join p in context.icb_piezaperitaje
                            on c.pieza_id equals p.pieza_id
                        select new
                        {
                            c.conve_id,
                            c.conve_nombre,
                            p.pieza_nombre
                        };

            var data = new
            {
                conve
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