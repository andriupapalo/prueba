using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class codigo_ivaController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");

        // GET: codigo_iva
        public ActionResult Browser(int? menu)
        {
            BuscarFavoritos(menu);
            return View(db.codigo_iva.ToList());
        }

        // GET: codigo_iva/Create
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        // POST: codigo_iva/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(codigo_iva codigo_iva, int? menu)
        {
            if (ModelState.IsValid)
            {
                codigo_iva existe = db.codigo_iva.FirstOrDefault(x =>
                    x.Descripcion == codigo_iva.Descripcion && x.porcentaje == codigo_iva.porcentaje);
                if (existe == null)
                {
                    db.codigo_iva.Add(codigo_iva);
                    db.SaveChanges();
                    TempData["mensaje"] = "Código de iva creado correctamente";
                    return RedirectToAction("Create", new { codigo_iva.id, menu });
                }

                TempData["mensaje_error"] = "El código de iva ingresado ya existe, por favor valide";
            }
            else
            {
                TempData["mensaje_error"] = "Error al guardar el código de iva, por favor valide";
            }

            BuscarFavoritos(menu);
            return View(codigo_iva);
        }

        // GET: codigo_iva/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            codigo_iva codigo_iva = db.codigo_iva.Find(id);
            if (codigo_iva == null)
            {
                return HttpNotFound();
            }

            BuscarFavoritos(menu);
            return View(codigo_iva);
        }

        // POST: codigo_iva/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(codigo_iva codigo_iva, int? menu)
        {
            if (ModelState.IsValid)
            {
                db.Entry(codigo_iva).State = EntityState.Modified;

                System.Collections.Generic.List<icb_referencia> codigo = db.icb_referencia
                    .Where(d => d.idporivaventa == codigo_iva.id || d.idporivacompra == codigo_iva.id).ToList();

                foreach (icb_referencia referencia in codigo)
                {
                    if (codigo_iva.Descripcion == "COMPRA")
                    {
                        float porcentajeCompra = (float)codigo_iva.porcentaje;
                        referencia.por_iva_compra = porcentajeCompra;
                    }
                    else if (codigo_iva.Descripcion == "VENTA")
                    {
                        float porcentajeVenta = (float)codigo_iva.porcentaje;
                        referencia.por_iva = porcentajeVenta;
                    }

                    db.Entry(referencia).State = EntityState.Modified;
                }

                System.Collections.Generic.List<anio_modelo> codigoVenta = db.anio_modelo.Where(d => d.idporcentajeiva == codigo_iva.id).ToList();
                System.Collections.Generic.List<anio_modelo> codigoCompra = db.anio_modelo.Where(d => d.idporcentajecompra == codigo_iva.id).ToList();
                System.Collections.Generic.List<anio_modelo> codigoImpo = db.anio_modelo.Where(d => d.idporcentajeimpoconsumo == codigo_iva.id).ToList();

                foreach (anio_modelo aModel in codigoCompra)
                {
                    if (codigo_iva.Descripcion == "COMPRA")
                    {
                        float porcentajeCompra = (float)codigo_iva.porcentaje;
                        aModel.porcentaje_compra = Convert.ToDecimal(porcentajeCompra, miCultura);
                    }

                    db.Entry(aModel).State = EntityState.Modified;
                }

                foreach (anio_modelo aModel in codigoVenta)
                {
                    if (codigo_iva.Descripcion == "VENTA")
                    {
                        float porcentajeVenta = (float)codigo_iva.porcentaje;
                        aModel.porcentaje_iva = Convert.ToDecimal(porcentajeVenta, miCultura);
                    }

                    db.Entry(aModel).State = EntityState.Modified;
                }

                foreach (anio_modelo aModel in codigoImpo)
                {
                    if (codigo_iva.Descripcion == "IMPOCONSUMO")
                    {
                        float porcentajeImpo = (float)codigo_iva.porcentaje;
                        aModel.impuesto_consumo = Convert.ToDecimal(porcentajeImpo, miCultura);
                    }

                    db.Entry(aModel).State = EntityState.Modified;
                }


                db.SaveChanges();
                TempData["mensaje"] = "Código de iva guardado correctamente";
            }
            else
            {
                TempData["mensaje_error"] = "Error al guardar el código de iva, por favor valide";
            }

            BuscarFavoritos(menu);
            return View(codigo_iva);
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