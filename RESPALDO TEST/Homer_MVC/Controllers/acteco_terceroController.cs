using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class acteco_terceroController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");

        public void ParametrosVista()
        {
            ViewBag.bodega = new SelectList(context.bodega_concesionario.OrderBy(x => x.bodccs_nombre), "id",
                "bodccs_nombre");
        }


        // GET: acteco_tercero
        public ActionResult Create(int? menu)
        {
            ParametrosVista();
            acteco_tercero crearActEco = new acteco_tercero { acteco_estado = true, acteco_razoninactivo = "No aplica" };
            BuscarFavoritos(menu);
            return View(crearActEco);
        }


        // POST: acteco_tercero/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(acteco_tercero acteco_tercero, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.acteco_tercero
                           where a.acteco_nombre == acteco_tercero.acteco_nombre
                           select a.acteco_nombre).Count();
                if (nom == 0)
                {
                    acteco_tercero.actecofec_creacion = DateTime.Now;
                    acteco_tercero.actecouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.acteco_tercero.Add(acteco_tercero);
                    bool guardar = context.SaveChanges() > 0;
                    if (guardar)
                    {
                        TempData["mensaje"] = "El registro de la nueva actividad económica fue exitoso!";
                        ParametrosVista();

                        int idact_eco = context.acteco_tercero.OrderByDescending(x => x.acteco_id).FirstOrDefault()
                            .acteco_id;
                        int cant = Convert.ToInt32(Request["listActividad"]);
                        for (int i = 1; i < cant + 1; i++)
                        {
                            if (!string.IsNullOrEmpty(Request["bodega" + i]) &&
                                !string.IsNullOrEmpty(Request["tarifa" + i]))
                            {
                                terceros_bod_ica bodega = new terceros_bod_ica
                                {
                                    idcodica = idact_eco,
                                    bodega = Convert.ToInt32(Request["bodega" + i])
                                };
                                string tarifa = Request["tarifa" + i];
                                bodega.porcentaje = Convert.ToDecimal(tarifa, miCultura);
                                context.terceros_bod_ica.Add(bodega);
                                context.SaveChanges();
                            }
                        }

                        BuscarFavoritos(menu);
                        return RedirectToAction("Create");
                    }

                    TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                    ParametrosVista();
                    BuscarFavoritos(menu);
                    return View(acteco_tercero);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            BuscarFavoritos(menu);
            ParametrosVista();
            return View(acteco_tercero);
        }


        // GET: acteco_tercero/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            acteco_tercero acteco_tercero = context.acteco_tercero.Find(id);
            if (acteco_tercero == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(id ?? 0);
            ParametrosVista();
            BuscarFavoritos(menu);
            return View(acteco_tercero);
        }


        // POST: acteco_tercero/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(acteco_tercero acteco_tercero, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.acteco_tercero
                           where a.acteco_nombre == acteco_tercero.acteco_nombre || a.acteco_id == acteco_tercero.acteco_id
                           select a.acteco_nombre).Count();

                if (nom == 1)
                {
                    acteco_tercero.actecofec_actualizacion = DateTime.Now;
                    acteco_tercero.actecouserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(acteco_tercero).State = EntityState.Modified;

                    context.SaveChanges();

                    TempData["mensaje"] = "La actualización de la actividad económica fue exitoso!";
                    ParametrosVista();
                    ConsultaDatosCreacion(acteco_tercero.acteco_id);

                    int cant = Convert.ToInt32(Request["listActividad"]);
                    for (int i = 1; i < cant + 1; i++)
                    {
                        if (!string.IsNullOrEmpty(Request["bodega" + i]) &&
                            !string.IsNullOrEmpty(Request["tarifa" + i]))
                        {
                            terceros_bod_ica bodega = new terceros_bod_ica
                            {
                                idcodica = acteco_tercero.acteco_id,
                                bodega = Convert.ToInt32(Request["bodega" + i])
                            };
                            string tarifa = Request["tarifa" + i];
                            bodega.porcentaje = Convert.ToDecimal(tarifa, miCultura);
                            context.terceros_bod_ica.Add(bodega);
                            context.SaveChanges();
                        }
                    }

                    BuscarFavoritos(menu);
                    return View(acteco_tercero);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            ParametrosVista();
            ConsultaDatosCreacion(acteco_tercero.acteco_id);
            BuscarFavoritos(menu);
            return View(acteco_tercero);
        }


        public void ConsultaDatosCreacion(int id)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creador = (from c in context.users
                             join b in context.acteco_tercero on c.user_id equals b.actecouserid_creacion
                             where b.acteco_id == id
                             select c).FirstOrDefault();

            ViewBag.user_nombre_cre = creador != null ? creador.user_nombre + " " + creador.user_apellido : null;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users actualizador = (from c in context.users
                                  join b in context.acteco_tercero on c.user_id equals b.actecouserid_actualizacion
                                  where b.acteco_id == id
                                  select c).FirstOrDefault();

            ViewBag.user_nombre_act =
                actualizador != null ? actualizador.user_nombre + " " + actualizador.user_apellido : null;
        }


        public JsonResult BuscarActividadEconomicaPaginados(int? id)
        {
            var d = context.acteco_tercero.ToList().Select(x => new
            {
                x.acteco_id,
                x.acteco_nombre,
                x.nroacteco_tercero,
                acteco_estado = x.acteco_estado ? "Activo" : "Inactivo"
            });

            if (id != null)
            {
                var bod = from x in context.terceros_bod_ica
                          where x.idcodica == id
                          select new
                          {
                              x.id,
                              bodega = x.bodega_concesionario.bodccs_nombre,
                              tarifa = x.porcentaje
                          };

                var data = new
                {
                    d,
                    bod
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var data = new
                {
                    d
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }

            base.Dispose(disposing);
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