using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class definicionContableController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: definicionContable
        public ActionResult Create(int? menu)
        {
            ListasDesplegables(new tparametrocontable());
            BuscarFavoritos(menu);
            return View();
        }


        public void ListasDesplegables(tparametrocontable modelo)
        {
            ViewBag.bodega = new SelectList(context.bodega_concesionario, "id", "bodccs_nombre", modelo.bodega);
            ViewBag.tipodocid = new SelectList(context.tp_doc_registros, "tpdoc_id", "tpdoc_nombre", modelo.tipodocid);
            ViewBag.tclasetrabajo =
                new SelectList(context.tclasetrabajo, "codigo", "descripcion", modelo.tclasetrabajo);
            ViewBag.tipooperacion = new SelectList(context.ttipooperacion, "id", "Descripcion", modelo.tipooperacion);
            var buscarCuentas = (from cuentas in context.cuenta_puc
                                 where cuentas.esafectable
                                 select new
                                 {
                                     cuentas.cntpuc_id,
                                     descripcion = "(" + cuentas.cntpuc_numero + ") " + cuentas.cntpuc_descp
                                 }).ToList();
            ViewBag.credito = new SelectList(buscarCuentas, "cntpuc_id", "descripcion", modelo.credito);
            ViewBag.debito = new SelectList(buscarCuentas, "cntpuc_id", "descripcion", modelo.debito);
            ViewBag.debcentro = new SelectList(buscarCuentas, "cntpuc_id", "descripcion", modelo.debcentro);
            ViewBag.credcentro = new SelectList(buscarCuentas, "cntpuc_id", "descripcion", modelo.credcentro);
            ViewBag.ctaiva = new SelectList(buscarCuentas, "cntpuc_id", "descripcion", modelo.ctaiva);
            ViewBag.centroiva = new SelectList(buscarCuentas, "cntpuc_id", "descripcion", modelo.centroiva);
            ViewBag.ctadescto = new SelectList(buscarCuentas, "cntpuc_id", "descripcion", modelo.ctadescto);
            ViewBag.centrodscto = new SelectList(buscarCuentas, "cntpuc_id", "descripcion", modelo.centrodscto);
            ViewBag.ctacosto = new SelectList(buscarCuentas, "cntpuc_id", "descripcion", modelo.ctacosto);
            ViewBag.centrocosto = new SelectList(buscarCuentas, "cntpuc_id", "descripcion", modelo.centrocosto);
            ViewBag.ctainventario = new SelectList(buscarCuentas, "cntpuc_id", "descripcion", modelo.ctainventario);
            ViewBag.centroinventario =
                new SelectList(buscarCuentas, "cntpuc_id", "descripcion", modelo.centroinventario);
        }


        [HttpPost]
        public ActionResult Create(tparametrocontable modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                tparametrocontable buscarParametro = context.tparametrocontable.FirstOrDefault(x =>
                    x.bodega == modelo.bodega && x.tipodocid == modelo.tipodocid
                                              && x.tclasetrabajo == modelo.tclasetrabajo &&
                                              x.tipooperacion == modelo.tipooperacion);
                if (buscarParametro != null)
                {
                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                }
                else
                {
                    context.tparametrocontable.Add(modelo);
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "El registro del nuevo parametro fue exitoso!";
                    }
                }
            }

            ListasDesplegables(new tparametrocontable());
            BuscarFavoritos(menu);
            return View();
        }


        public ActionResult update(int idBodega, int tpDocumento, string clase, int tpOperacion, int? menu)
        {
            tparametrocontable buscarParametro = context.tparametrocontable.FirstOrDefault(x =>
                x.bodega == idBodega && x.tipodocid == tpDocumento
                                     && x.tclasetrabajo == clase && x.tipooperacion == tpOperacion);
            if (buscarParametro == null)
            {
                return HttpNotFound();
            }

            ListasDesplegables(buscarParametro);
            BuscarFavoritos(menu);
            return View(buscarParametro);
        }


        [HttpPost]
        public ActionResult update(tparametrocontable modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                context.Entry(modelo).State = EntityState.Modified;
                int guardar = context.SaveChanges();
                if (guardar > 0)
                {
                    TempData["mensaje"] = "La actualizacion del parametro fue exitosa!";
                }
                else
                {
                    TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide!";
                }
            }

            ListasDesplegables(new tparametrocontable());
            BuscarFavoritos(menu);
            return View();
        }


        public JsonResult BuscarParametrosContables()
        {
            var buscarParametros = (from parametro in context.tparametrocontable
                                    join bodega in context.bodega_concesionario
                                        on parametro.bodega equals bodega.id
                                    join tipoDocumento in context.tp_doc_registros
                                        on parametro.tipodocid equals tipoDocumento.tpdoc_id
                                    join claseTrabajo in context.tclasetrabajo
                                        on parametro.tclasetrabajo equals claseTrabajo.codigo
                                    join tipoOperacion in context.ttipooperacion
                                        on parametro.tipooperacion equals tipoOperacion.id
                                    select new
                                    {
                                        parametro.bodega,
                                        parametro.tipodocid,
                                        parametro.tclasetrabajo,
                                        parametro.tipooperacion,
                                        bodega.bodccs_nombre,
                                        tipoDocumento.tpdoc_nombre,
                                        claseTrabajo.descripcion,
                                        tipoOperacion.Descripcion
                                    }).ToList();
            return Json(buscarParametros, JsonRequestBehavior.AllowGet);
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