using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class horarioTallerController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        // GET: horarioTaller
        public ActionResult Create(int? menu)
        {
            ViewBag.bodega_id =
                new SelectList(context.bodega_concesionario.Where(x => x.es_taller), "id", "bodccs_nombre");
            BuscarFavoritos(menu);
            return View();
        }


        // POST: horarioTaller
        [HttpPost]
        public ActionResult Create(thorario_taller modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                bodega_concesionario buscarBodega = context.bodega_concesionario.FirstOrDefault(x => x.id == modelo.bodega_id);
                if (buscarBodega != null)
                {
                    buscarBodega.hora_inicial = modelo.hora_inicial;
                    buscarBodega.hora_final = modelo.hora_final;
                    buscarBodega.lapso_tiempo = modelo.lapso_tiempo;
                    context.Entry(buscarBodega).State = EntityState.Modified;
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "La creación del horario fue exitosa";
                    }
                }
                else
                {
                    TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                }
            }

            ViewBag.bodega_id =
                new SelectList(context.bodega_concesionario.Where(x => x.es_taller), "id", "bodccs_nombre");
            BuscarFavoritos(menu);
            return View();
        }


        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            bodega_concesionario horario = context.bodega_concesionario.Find(id);
            if (horario == null)
            {
                return HttpNotFound();
            }

            thorario_taller horarioBuscado = new thorario_taller
            {
                hora_inicial = horario.hora_inicial ?? new TimeSpan(),
                hora_final = horario.hora_final ?? new TimeSpan(),
                bodega_id = horario.id,
                lapso_tiempo = horario.lapso_tiempo ?? 0
            };
            ViewBag.bodega_id = new SelectList(context.bodega_concesionario.Where(x => x.es_taller), "id",
                "bodccs_nombre", horario.id);
            BuscarFavoritos(menu);
            return View(horarioBuscado);
        }


        [HttpPost]
        public ActionResult Edit(thorario_taller modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                bodega_concesionario buscarSiExiste = context.bodega_concesionario.FirstOrDefault(x => x.id == modelo.bodega_id);
                if (buscarSiExiste != null)
                {
                    buscarSiExiste.hora_inicial = modelo.hora_inicial;
                    buscarSiExiste.hora_final = modelo.hora_final;
                    buscarSiExiste.lapso_tiempo = modelo.lapso_tiempo;
                    context.Entry(buscarSiExiste).State = EntityState.Modified;
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "La actualización del horario fue exitosa";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                    }
                }
                else
                {
                    TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                    //if (buscarSiExiste.id == modelo.id)
                    //{
                    //    buscarSiExiste.bodega_id = modelo.bodega_id;
                    //    buscarSiExiste.hora_inicial = modelo.hora_inicial;
                    //    buscarSiExiste.hora_final = modelo.hora_final;
                    //    buscarSiExiste.lapso_tiempo = modelo.lapso_tiempo;
                    //    context.Entry(buscarSiExiste).State = System.Data.Entity.EntityState.Modified;
                    //    try
                    //    {
                    //        var guardar = context.SaveChanges();
                    //        if (guardar > 0)
                    //        {
                    //            TempData["mensaje"] = "La actualización de la bahía fue exitosa";
                    //        }
                    //        else
                    //        {
                    //            TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                    //        }

                    //    }
                    //    catch (DbEntityValidationException e)
                    //    {
                    //        foreach (var eve in e.EntityValidationErrors)
                    //        {
                    //            Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                    //                eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    //            foreach (var ve in eve.ValidationErrors)
                    //            {
                    //                Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                    //                    ve.PropertyName, ve.ErrorMessage);
                    //            }
                    //        }
                    //        throw;
                    //    }

                    //}
                    //else
                    //{
                    //    TempData["mensaje_error"] = "El horario con la bodega seleccionada ya se encuentra creada, por favor verifique...";
                    //}
                }
            }

            ViewBag.bodega_id = new SelectList(context.bodega_concesionario.Where(x => x.es_taller), "id",
                "bodccs_nombre", modelo.bodega_id);
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public JsonResult BuscarHorarios()
        {
            var buscarHorarios = (from bodega in context.bodega_concesionario
                                  select new
                                  {
                                      bodega.id,
                                      bodega.bodccs_nombre,
                                      bodega.hora_inicial,
                                      bodega.hora_final,
                                      bodega.lapso_tiempo
                                  }).ToList();

            var data = buscarHorarios.Select(x => new
            {
                x.id,
                x.bodccs_nombre,
                hora_inicial = x.hora_inicial.ToString(),
                hora_final = x.hora_final.ToString(),
                lapso_tiempo = x.lapso_tiempo != null ? x.lapso_tiempo.ToString() : ""
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