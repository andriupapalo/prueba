using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class kitsaccesoriosController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");

        // GET: kitsaccesorios/Create
        public ActionResult Create(int? menu)
        {
            var list = (from r in db.icb_referencia
                        where r.modulo == "R"
                        select new
                        {
                            codigo = r.ref_codigo,
                            descripcion = r.ref_codigo + " " + r.ref_descripcion
                        }).ToList();

            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in list)
            {
                lista.Add(new SelectListItem
                {
                    Text = item.descripcion,
                    Value = item.codigo
                });
            }

            ViewBag.referenciarep = lista;
            ViewBag.iva = new SelectList(db.codigo_iva.Where(x => x.Descripcion == "VENTA"), "id", "porcentaje");
            ViewBag.modelokit = new SelectList(db.vmodelog, "id", "Descripcion");
            BuscarFavoritos(menu);
            return View();
        }

        // POST: kitsaccesorios/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(kitsaccesorios kitsaccesorios, int? menu)
        {
            kitsaccesorios existe = db.kitsaccesorios.FirstOrDefault(x => x.Descripcion == kitsaccesorios.Descripcion);
            if (existe == null)
            {
                kitsaccesorios.Descripcion = kitsaccesorios.Descripcion;
                kitsaccesorios.tipo = kitsaccesorios.tipo;
                kitsaccesorios.modelokit = kitsaccesorios.modelokit;
                kitsaccesorios.precio = Convert.ToDecimal(Request["precio"], miCultura);
                kitsaccesorios.iva = kitsaccesorios.iva;
                db.kitsaccesorios.Add(kitsaccesorios);
                db.SaveChanges();

                int idkit = db.kitsaccesorios.OrderByDescending(x => x.id).FirstOrDefault().id;

                int lista = Convert.ToInt32(Request["lista_accesorios"]);
                for (int i = 1; i <= lista; i++)
                {
                    if (!string.IsNullOrEmpty(Request["codigoTable" + i]))
                    {
                        referenciaskits referencias = new referenciaskits
                        {
                            codigo = Convert.ToString(Request["codigoTable" + i]),
                            precio = Convert.ToDecimal(Request["precioTable" + i], miCultura),
                            cantidad = Convert.ToInt32(Request["cantidadTable" + i]),
                            idkitaccesorios = idkit
                        };

                        db.referenciaskits.Add(referencias);
                    }
                }

                db.SaveChanges();

                TempData["mensaje"] = "La creación del registro fue exitoso";
                return RedirectToAction("Create", new { kitsaccesorios.id, menu });
            }

            TempData["mensaje_error"] = "El kit ingresado ya existe, por favor valide";

            var list = (from r in db.icb_referencia
                        where r.modulo == "R"
                        select new
                        {
                            codigo = r.ref_codigo,
                            descripcion = r.ref_codigo + " " + r.ref_descripcion
                        }).ToList();

            List<SelectListItem> listaRe = new List<SelectListItem>();
            foreach (var item in list)
            {
                listaRe.Add(new SelectListItem
                {
                    Text = item.descripcion,
                    Value = item.codigo
                });
            }

            ViewBag.referenciarep = listaRe;
            ViewBag.modelokit = new SelectList(db.vmodelog, "id", "Descripcion", kitsaccesorios.modelokit);
            ViewBag.iva = new SelectList(db.codigo_iva.Where(x => x.Descripcion == "VENTA"), "id", "porcentaje");
            BuscarFavoritos(menu);
            return View(kitsaccesorios);
        }

        // GET: kitsaccesorios/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            kitsaccesorios kitsaccesorios = db.kitsaccesorios.Find(id);
            if (kitsaccesorios == null)
            {
                return HttpNotFound();
            }

            kitsaccesorios modelo = new kitsaccesorios
            {
                Descripcion = kitsaccesorios.Descripcion,
                precio = kitsaccesorios.precio
            };


            ViewBag.idKit = kitsaccesorios.id;
            ViewBag.modelokit = new SelectList(db.vmodelog.Where(x => x.estado).OrderBy(x => x.Descripcion), "id",
                "Descripcion", kitsaccesorios.modelokit);
            ViewBag.referenciarep =
                new SelectList(db.icb_referencia.Where(x => x.ref_estado).OrderBy(x => x.ref_descripcion), "ref_codigo",
                    "ref_descripcion");
            BuscarFavoritos(menu);
            return View(modelo);
        }

        // POST: kitsaccesorios/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(kitsaccesorios kitsaccesorios, int? menu)
        {
            var list = (from r in db.icb_referencia
                        where r.modulo == "R"
                        select new
                        {
                            codigo = r.ref_codigo,
                            descripcion = r.ref_codigo + " " + r.ref_descripcion
                        }).ToList();

            List<SelectListItem> listaRe = new List<SelectListItem>();
            foreach (var item in list)
            {
                listaRe.Add(new SelectListItem
                {
                    Text = item.descripcion,
                    Value = item.codigo
                });
            }

            ViewBag.referenciarep = listaRe;

            ViewBag.accesorios = kitsaccesorios.referenciaskits;
            //ViewBag.referenciarep = new SelectList(db.icb_referencia, "ref_codigo", "ref_descripcion", kitsaccesorios.referenciarep);
            ViewBag.modelokit = new SelectList(db.vmodelog, "id", "Descripcion", kitsaccesorios.modelokit);


            kitsaccesorios.precio = Convert.ToDecimal(Request["precio"], miCultura);
            db.Entry(kitsaccesorios).State = EntityState.Modified;
            db.SaveChanges();

            if (!string.IsNullOrEmpty(Request["lista_accesorios"]))
            {
                int lista = Convert.ToInt32(Request["lista_accesorios"]);
                for (int i = 1; i <= lista; i++)
                {
                    if (!string.IsNullOrEmpty(Request["codigoTable" + i]))
                    {
                        referenciaskits referencias = new referenciaskits
                        {
                            codigo = Convert.ToString(Request["codigoTable" + i]),
                            precio = Convert.ToDecimal(Request["precioTable" + i], miCultura),
                            cantidad = Convert.ToInt32(Request["cantidadTable" + i]),
                            idkitaccesorios = kitsaccesorios.id
                        };

                        db.referenciaskits.Add(referencias);
                    }
                }

                db.SaveChanges();
                TempData["mensaje"] = "La actualización del registro fue exitoso";
            }

            ViewBag.iva = new SelectList(db.codigo_iva.Where(x => x.Descripcion == "VENTA"), "id", "porcentaje",
                kitsaccesorios.iva);
            BuscarFavoritos(menu);
            ViewBag.idKit = kitsaccesorios.id;
            return View(kitsaccesorios);
        }

        public ActionResult Browser(int? menu)
        {
            ViewBag.datos = db.kitsaccesorios.ToList();
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult BuscarDatos()
        {
            var data = (from a in db.kitsaccesorios
                        join b in db.vmodelog
                            on a.modelokit equals b.id
                        select new
                        {
                            a.id,
                            a.Descripcion,
                            modelo = b.Descripcion,
                            a.precio
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public int EliminarAccesorio(int id)
        {
            referenciaskits dato = db.referenciaskits.Find(id);
            db.Entry(dato).State = EntityState.Deleted;
            int result = db.SaveChanges();

            return result;
        }

        public JsonResult BuscarPrecio(string codigo)
        {
            decimal buscar = db.rprecios.Where(x => x.codigo == codigo).Select(x => x.precio1).FirstOrDefault();

            var data = new { buscar };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarReferencias(int id)
        {
            var buscar = (from a in db.referenciaskits
                          join b in db.icb_referencia
                              on a.codigo equals b.ref_codigo
                          join c in db.codigo_iva
                              on b.idporivaventa equals c.id
                          where a.idkitaccesorios == id
                          select new
                          {
                              a.id,
                              a.codigo,
                              b.ref_descripcion,
                              a.precio,
                              a.cantidad,
                              c.porcentaje
                          }).ToList();
            var data = buscar.Select(x => new
            {
                referencia = x.codigo + " " + x.ref_descripcion,
                x.precio,
                x.cantidad,
                x.porcentaje,
                x.id
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarIva(string id)
        {
            decimal? data = (from a in db.icb_referencia
                             join b in db.codigo_iva
                                 on a.idporivaventa equals b.id
                             where a.ref_codigo == id
                             select b.porcentaje).FirstOrDefault();

            return Json(data, JsonRequestBehavior.AllowGet);
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