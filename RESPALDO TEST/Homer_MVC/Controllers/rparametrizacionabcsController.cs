using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class rparametrizacionabcsController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: rparametrizacionabcs
        public ActionResult Browser(int? menu)
        {
            BuscarFavoritos(menu);
            return View(db.rparametrizacionabc.ToList());
        }


        // GET: rparametrizacionabcs/Create
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        // POST: rparametrizacionabcs/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(rparametrizacionabc rparametrizacionabc, int? menu)
        {
            int periodo = 0;
            var convertir = Int32.TryParse(Request["periodoHidden"],out periodo);
            if (convertir==true && periodo != 0)
            {
                rparametrizacionabc.periodo = periodo;
            }

            if (ModelState.IsValid)
            {
                string clasificacion = Request["clasificacion"];
                string porcenreserva = Request["porcenreserva"];

                rclasificacionabc existe = db.rclasificacionabc.FirstOrDefault(x => x.clasificacion == clasificacion);
                if (existe != null)
                {
                    TempData["mensaje_error"] =
                        "Ya existe una parametrización para la clasificacion ingresada, por favor valide";
                    BuscarFavoritos(menu);
                    return View(rparametrizacionabc);
                }

                rparametrizacionabc.meses = Request["meses"];
                db.rparametrizacionabc.Add(rparametrizacionabc);
                db.SaveChanges();

                int id_parametrizacion = db.rparametrizacionabc.OrderByDescending(x => x.id).FirstOrDefault().id;

                rclasificacionabc clas = new rclasificacionabc
                {
                    idparam = id_parametrizacion,
                    clasificacion = clasificacion,
                    porcenreserva = Convert.ToInt32(porcenreserva)
                };
                db.rclasificacionabc.Add(clas);
                //db.SaveChanges();

                TempData["mensaje"] = "Parametrización guardada correctamente";

                return RedirectToAction("Edit", new { id = id_parametrizacion, menu });
            }

            TempData["mensaje_error"] = "Error al guardar la parametrización, por favor valide";
            System.Collections.Generic.List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                .Where(y => y.Count > 0)
                .ToList();

            BuscarFavoritos(menu);
            return View(rparametrizacionabc);
        }

        // GET: rparametrizacionabcs/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            rparametrizacionabc rparametrizacionabc = db.rparametrizacionabc.Find(id);
            if (rparametrizacionabc == null)
            {
                return HttpNotFound();
            }

            rparametrizacionabc buscar = db.rparametrizacionabc.Where(x => x.id == id).FirstOrDefault();

            ViewBag.clasificacion = buscar.clasificacion;
            ViewBag.reserva = buscar.porcentaje_reserva;
            ViewBag.rangomeses = buscar.rangomes;
            ViewBag.cantidadmeses = buscar.cantidadmes;
            ViewBag.cantidadmov = buscar.cantidadmov_desde;

            BuscarFavoritos(menu);
            return View(rparametrizacionabc);
        }

        // POST: rparametrizacionabcs/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(rparametrizacionabc rparametrizacionabc, int? menu)
        {
            rparametrizacionabc.periodo = Convert.ToInt32(Request["periodoHidden"]);
            if (ModelState.IsValid)
            {
                string clasificacion = Request["clasificacion"];
                string porcenreserva = Request["porcenreserva"];

                rparametrizacionabc.meses = Request["meses"];
                db.Entry(rparametrizacionabc).State = EntityState.Modified;

                rclasificacionabc clas = db.rclasificacionabc.FirstOrDefault(x => x.idparam == rparametrizacionabc.id);
                clas.clasificacion = clasificacion;
                clas.porcenreserva = Convert.ToInt32(porcenreserva);
                db.Entry(clas).State = EntityState.Modified;

                db.SaveChanges();
                TempData["mensaje"] = "Parametrización guardada correctamente";
                ViewBag.clasificacion = clasificacion;
                ViewBag.porcenreserva = porcenreserva;
            }

            ViewBag.clasificacion = rparametrizacionabc.clasificacion;
            ViewBag.reserva = rparametrizacionabc.porcentaje_reserva;
            ViewBag.rangomeses = rparametrizacionabc.rangomes;
            ViewBag.cantidadmeses = rparametrizacionabc.cantidadmes;
            ViewBag.cantidadmov = rparametrizacionabc.cantidadmov_desde;
            BuscarFavoritos(menu);
            return View(rparametrizacionabc);
        }

        public JsonResult ClasificarReferencias()
        {
            //-------- borrar la clasificación actual por si no entra en ninguna clasificación-------
            const string sqlClasifAllRefToNull = "UPDATE icb_referencia SET clasificacion_ABC = NULL WHERE modulo = 'R' AND clasificacion_ABC IS NOT NULL";
            int allRefClasifToNull = db.Database.ExecuteSqlCommand(sqlClasifAllRefToNull);
            //------------------------------------------------
            var clasificacion = (from rcf in db.rclasificacionabc select new { rcf.idparam, rcf.porcenreserva, rcf.clasificacion }).ToList();
            System.Collections.Generic.List<icb_referencia> datos = db.icb_referencia.Where(x => x.modulo == "R")./*Take(10).*/ToList();
            var abcs = (from param in db.rparametrizacionabc select new { param.id, param.rangomes, param.cantidadmov_desde }).ToList();
            DateTime fecha;
            bool modificado;
            foreach (icb_referencia row in datos)
            {
                modificado = false;
                foreach (var item in clasificacion)
                {
                    if (!modificado) {
                        var parametros = abcs.FirstOrDefault(x => x.id == item.idparam);
                        fecha = DateTime.Now.AddMonths(-parametros.rangomes);

                        var referencia = (from x in db.referencias_inven where 
                                ((x.ano >= fecha.Year && x.mes >= fecha.Month) || (x.ano == DateTime.Now.Year && x.mes < DateTime.Now.Month))
                                && (x.codigo == row.ref_codigo && x.can_vta - x.can_dev_vta > 0) select new { x.can_vta, x.can_dev_vta }).ToList();
                        var cant_mov = referencia.Count();
                        if (cant_mov >= parametros.cantidadmov_desde)
                        {
                            decimal cant_ventas = referencia.Sum(x => x.can_vta - x.can_dev_vta);
                            decimal porreserva = Convert.ToDecimal(item.porcenreserva ?? 0);
                            decimal reserva = cant_ventas / parametros.rangomes * porreserva;

                            /*Guardar en el Historial de Cambios*/
                            var historico = new icb_tracking_clasifabc
                            {
                                codigo_ref = row.ref_codigo,
                                old_clasifabc = row.clasificacion_ABC,
                                new_clasifabc = item.clasificacion,
                                cantidmov = cant_mov,
                                fecela = DateTime.Now
                            };
                            db.icb_tracking_clasifabc.Add(historico);
                            /* -------------------------*/

                            /*Modificar Calificacion ABC*/
                            row.clasificacion_ABC = item.clasificacion;
                            row.ref_cantidad_min = Convert.ToInt32(reserva);
                            db.Entry(row).State = EntityState.Modified;
                            /* -------------------------*/

                            modificado = true;
                            db.SaveChanges();
                        }
                    }
                }
            }
            string data = "Referencias clasificadas correctamente";
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult cargarBrowser()
        {
            var buscar = db.rparametrizacionabc.Select(x => new
            {
                x.id,
                x.clasificacion,
                x.porcentaje_reserva,
                x.rangomes,
                x.cantidadmov_desde,
                cantidadmes = x.cantidadmes == 1 ? "Mensual" :
                    x.cantidadmes == 2 ? "Bimestral" :
                    x.cantidadmes == 3 ? "Trimestral" :
                    x.cantidadmes == 6 ? "Semestral" :
                    x.cantidadmes == 12 ? "Anual" : "",
                periodo = x.periodo != 0 ? "Primer Semestre" : ""
            }).ToList();

            return Json(buscar, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }


        public void BuscarFavoritos(int? menu)
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

            var buscarFavoritosSeleccionados = (from favoritos in db.favoritos
                                                join menu2 in db.Menus
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