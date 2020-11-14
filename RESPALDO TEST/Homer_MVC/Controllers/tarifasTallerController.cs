using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tarifasTallerController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: tarifasTaller
        public ActionResult Create(int? menu)
        {
            ViewBag.tipotarifa = new SelectList(context.ttipostarifa, "id", "Descripcion");
            ViewBag.bodega = new SelectList(context.bodega_concesionario, "id", "bodccs_nombre");
            BuscarFavoritos(menu);
            return View();
        }


        // POST: tarifasTaller
        [HttpPost]
        public ActionResult Create(ttarifastaller modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                ttarifastaller buscarSiExiste = context.ttarifastaller.FirstOrDefault(x =>
                    x.tipotarifa == modelo.tipotarifa && x.bodega == modelo.bodega);
                if (buscarSiExiste != null)
                {
                    TempData["mensaje_error"] = "La bodega y tipo de tarifa ya se encuentran registrados.";
                }
                else
                {
                    if (modelo.valorhora!=null)
                    {
                        modelo.fec_creacion = DateTime.Now;
                        modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                        context.ttarifastaller.Add(modelo);
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            TempData["mensaje"] = "La tarifa del taller se ha creado exitosamente.";
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error en la base de datos, por favor verifique su conexion...";
                        }
                    }
                    else
                    {
                        TempData["mensaje_error"] = "El costo debe ser menor al precio, por favor valide";
                    }
                }
            }

            ViewBag.tipotarifa = new SelectList(context.ttipostarifa, "id", "Descripcion", modelo.tipotarifa);
            ViewBag.bodega = new SelectList(context.bodega_concesionario, "id", "bodccs_nombre", modelo.bodega);
            BuscarFavoritos(menu);
            return View();
        }


        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ttarifastaller tarifa = context.ttarifastaller.Find(id);
            if (tarifa == null)
            {
                return HttpNotFound();
            }

            ViewBag.tipotarifa = new SelectList(context.ttipostarifa, "id", "Descripcion", tarifa.tipotarifa);
            ViewBag.bodega = new SelectList(context.bodega_concesionario, "id", "bodccs_nombre", tarifa.bodega);
            BuscarFavoritos(menu);
            return View(tarifa);
        }


        [HttpPost]
        public ActionResult Edit(ttarifastaller modelo, int? menu)
        {
            if (ModelState.IsValid)
            {

                ttarifastaller buscarSiExiste = context.ttarifastaller.FirstOrDefault(x =>
                    x.tipotarifa == modelo.tipotarifa && x.bodega == modelo.bodega);
                if (buscarSiExiste != null)
                {
                    if (buscarSiExiste.id != modelo.id)
                    {
                        TempData["mensaje_error"] = "La bodega y tipo de tarifa ya se encuentran registrados";
                    }
                    else
                    {

                        buscarSiExiste.bodega = modelo.bodega;
                        buscarSiExiste.iva = modelo.iva;
                        buscarSiExiste.tipotarifa = modelo.tipotarifa;
                        buscarSiExiste.valorhora = modelo.valorhora;
                        buscarSiExiste.fec_actualizacion = DateTime.Now;
                        buscarSiExiste.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);

                        context.Entry(buscarSiExiste).State = EntityState.Modified;
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            TempData["mensaje"] = "La tarifa del taller se ha actualizado exitosamente.";
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error en la base de datos, por favor verifique su conexion...";
                        }
                    }
                }
                else
                {
                    ttarifastaller tarifa = new ttarifastaller
                    {
                        bodega = modelo.bodega,
                        iva = modelo.iva,
                        tipotarifa = modelo.tipotarifa,
                        valorhora = modelo.valorhora
                    };

                    context.Entry(tarifa).State = EntityState.Added;
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "La tarifa del taller se ha actualizado exitosamente.";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error en la base de datos, por favor verifique su conexion...";
                    }
                }
            }

            ViewBag.tipotarifa = new SelectList(context.ttipostarifa, "id", "Descripcion", modelo.tipotarifa);
            ViewBag.bodega = new SelectList(context.bodega_concesionario, "id", "bodccs_nombre", modelo.bodega);
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public JsonResult BuscarTarifasTaller()
        {
            var buscarTarifasTaller = (from tarifas in context.ttarifastaller
                                       join tiposTarifa in context.ttipostarifa
                                           on tarifas.tipotarifa equals tiposTarifa.id
                                       join bodega in context.bodega_concesionario
                                           on tarifas.bodega equals bodega.id
                                       select new
                                       {
                                           tarifas.id,
                                           tipoTarifa = tiposTarifa.Descripcion,
                                           bodega.bodccs_nombre,
                                           valorhora = tarifas.valorhora != null ? tarifas.valorhora.ToString() : "",
                                           iva = tarifas.iva != null ? tarifas.iva : "",
                                           total_tarifa = tarifas.total_tarifa != null ? tarifas.total_tarifa : 0
                                       }).ToList();

            return Json(buscarTarifasTaller, JsonRequestBehavior.AllowGet);
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