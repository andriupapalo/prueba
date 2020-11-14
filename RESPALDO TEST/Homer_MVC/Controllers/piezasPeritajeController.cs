using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class piezasPeritajeController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: piezasPeritaje
        public ActionResult Crear(int? menu)
        {
            ViewBag.zona_id = new SelectList(context.icb_zonaperitaje.OrderBy(x => x.zonaper_nombre), "zonaper_id",
                "zonaper_nombre");
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Crear(icb_piezaperitaje pieza, int? menu)
        {
            if (ModelState.IsValid)
            {
                icb_piezaperitaje buscarPieza = context.icb_piezaperitaje.FirstOrDefault(x => x.pieza_nombre == pieza.pieza_nombre);
                if (buscarPieza == null)
                {
                    pieza.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    pieza.fec_creacion = DateTime.Now;
                    context.icb_piezaperitaje.Add(pieza);
                    context.SaveChanges();

                    int idpieza = context.icb_piezaperitaje.OrderByDescending(x => x.pieza_id).FirstOrDefault()
                        .pieza_id;
                    int cantidad_convenciones = Convert.ToInt32(Request["cantidad_convenciones"]);
                    CrearConvenciones(cantidad_convenciones, idpieza);
                    TempData["mensaje"] = "La pieza " + pieza.pieza_nombre + " se creo correctamente";
                    ViewBag.zona_id = new SelectList(context.icb_zonaperitaje.OrderBy(x => x.zonaper_nombre),
                        "zonaper_id", "zonaper_nombre");
                    BuscarFavoritos(menu);
                    return RedirectToAction("Crear", new { menu });
                }

                TempData["mensaje_error"] = "La pieza " + pieza.pieza_nombre + " ya existe";
                ViewBag.zona_id = new SelectList(context.icb_zonaperitaje.OrderBy(x => x.zonaper_nombre), "zonaper_id",
                    "zonaper_nombre");
                BuscarFavoritos(menu);
                return RedirectToAction("Crear", new { menu });
            }

            ViewBag.zona_id = new SelectList(context.icb_zonaperitaje.OrderBy(x => x.zonaper_nombre), "zonaper_id",
                "zonaper_nombre");
            BuscarFavoritos(menu);
            return View();
        }

        public void CrearConvenciones(int cantCovenciones, int idpieza)
        {
            for (int i = 1; i <= cantCovenciones; i++)
            {
                string conve = Request["convencion" + i];

                if (!string.IsNullOrEmpty(conve))
                {
                    context.icb_conveperitaje.Add(new icb_conveperitaje
                    {
                        fec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        conve_nombre = conve,
                        pieza_id = idpieza
                    });
                    context.SaveChanges();
                }
            }
        }

        public ActionResult Update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_piezaperitaje pieza = context.icb_piezaperitaje.Find(id);
            if (pieza == null)
            {
                ViewBag.zona_id = new SelectList(context.icb_zonaperitaje.OrderBy(x => x.zonaper_nombre), "zonaper_id",
                    "zonaper_nombre");
                return HttpNotFound();
            }

            ViewBag.zona_id = new SelectList(context.icb_zonaperitaje.OrderBy(x => x.zonaper_nombre), "zonaper_id",
                "zonaper_nombre");
            BuscarFavoritos(menu);
            return View(pieza);
        }

        [HttpPost]
        public ActionResult Update(icb_piezaperitaje pieza, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.icb_piezaperitaje
                           where a.pieza_nombre == pieza.pieza_nombre || a.pieza_id == pieza.pieza_id
                           select a.pieza_nombre).Count();
                if (nom == 1)
                {
                    pieza.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    pieza.fec_actualizacion = DateTime.Now;
                    context.Entry(pieza).State = EntityState.Modified;
                    context.SaveChanges();

                    System.Collections.Generic.List<icb_conveperitaje> listaConve = context.icb_conveperitaje.Where(x => x.pieza_id == pieza.pieza_id).ToList();

                    foreach (icb_conveperitaje item in listaConve)
                    {
                        if (Request["convencion" + item.conve_id] != null)
                        {
                            item.conve_nombre = Request["convencion" + item.conve_id];
                            context.Entry(item).State = EntityState.Modified;
                            context.SaveChanges();
                        }
                    }

                    int cantidad_convenciones = Convert.ToInt32(Request["cantidad_convenciones"]);
                    if (cantidad_convenciones > 0)
                    {
                        CrearConvenciones(cantidad_convenciones, pieza.pieza_id);
                    }

                    TempData["mensaje"] = "La pieza " + pieza.pieza_nombre + " se ha actualizado correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
                }
            }

            ViewBag.zona_id = new SelectList(context.icb_zonaperitaje.OrderBy(x => x.zonaper_nombre), "zonaper_id",
                "zonaper_nombre");
            BuscarFavoritos(menu);
            return View(pieza);
        }

        public int EliminarConvencion(int id)
        {
            icb_conveperitaje dato = context.icb_conveperitaje.Find(id);
            context.Entry(dato).State = EntityState.Deleted;
            int result = context.SaveChanges();

            return result;
        }

        public JsonResult BuscarPiezas()
        {
            var piezas = from p in context.icb_piezaperitaje
                         join z in context.icb_zonaperitaje
                             on p.zona_id equals z.zonaper_id
                         select new
                         {
                             p.pieza_id,
                             p.pieza_nombre,
                             z.zonaper_nombre
                         };

            var data = new
            {
                piezas
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarConvenciones(int piezaid)
        {
            var data = from c in context.icb_conveperitaje
                       where c.pieza_id == piezaid
                       select new
                       {
                           c.conve_id,
                           c.conve_nombre
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