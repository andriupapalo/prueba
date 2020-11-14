using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class resolucionFacturasController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: resolucionFacturas
        public ActionResult Create(int? id, int? menu)
        {
            //if (id == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            ViewBag.tipodoc = context.tp_doc_registros
                .Where(x => x.tipo == 2 || x.tipo == 4 || x.tipo == 5 || x.tipo == 37).ToList();
            grupoconsecutivos buscarTipoDoc = context.grupoconsecutivos.FirstOrDefault(x => x.grupo == id);
            int documento = buscarTipoDoc != null ? buscarTipoDoc.documento_id : 0;
            resolucionfactura buscarResolucion =
                context.resolucionfactura.FirstOrDefault(x => x.grupo == id && x.tipodoc == documento);
            ViewBag.tipoDocSeleccionado = buscarTipoDoc != null ? buscarTipoDoc.documento_id : 0;

            var modelo = new resolucionForm();
            if (buscarResolucion != null)
            {
                modelo.consecaviso = buscarResolucion.consecaviso;
                modelo.consecfin = buscarResolucion.consecfin;
                modelo.consecini = buscarResolucion.consecini;
                modelo.diasaviso = buscarResolucion.diasaviso;
                modelo.estado = buscarResolucion.estado;
                modelo.fechafin = buscarResolucion.fechafin.ToString("yyyy/MM/dd", new CultureInfo("en-US"));
                modelo.fechaini = buscarResolucion.fechaini.ToString("yyyy/MM/dd", new CultureInfo("en-US"));
                modelo.fec_actualizacion = buscarResolucion.fec_actualizacion;
                modelo.fec_creacion = buscarResolucion.fec_creacion;
                modelo.grupo = buscarResolucion.grupo;
                modelo.id = buscarResolucion.id;
                modelo.id_licencia = buscarResolucion.id_licencia;
                modelo.numfacturas = buscarResolucion.numfacturas;
                modelo.razon_inactivo = buscarResolucion.razon_inactivo;
                modelo.resolucion = buscarResolucion.resolucion;
                modelo.tipodoc = buscarResolucion.tipodoc;
                modelo.userid_creacion = buscarResolucion.userid_creacion;
                modelo.user_idactualizacion = buscarResolucion.user_idactualizacion;
            }
            else
            {
                modelo.estado = true;
                modelo.fechaini = DateTime.Now.ToString("yyyy/MM/dd", new CultureInfo("en-US"));
                modelo.fechafin = new DateTime(DateTime.Now.Year,12,31).ToString("yyyy/MM/dd", new CultureInfo("en-US")); ;
                modelo.grupo = id ?? 0;
                modelo.tipodoc = buscarTipoDoc != null ? buscarTipoDoc.documento_id : 0;
            }


            BuscarFavoritos(menu);
            return View(modelo);
        }

        [HttpPost]
        public ActionResult Create(resolucionForm modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                resolucionfactura buscarResolucion =
                    context.resolucionfactura.FirstOrDefault(
                        x => x.tipodoc == modelo.tipodoc && x.grupo == modelo.grupo);
                if (buscarResolucion != null)
                {
                    var fecha1 = DateTime.Now;
                    var convertir1 = DateTime.TryParse(modelo.fechaini, out fecha1);

                    var fecha2 = DateTime.Now;
                    var convertir2 = DateTime.TryParse(modelo.fechafin, out fecha2); 
                    if (convertir1==true && convertir2==true && fecha1 < fecha2)
                    {
                        buscarResolucion.fec_actualizacion = DateTime.Now;
                        buscarResolucion.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarResolucion.tipodoc = modelo.tipodoc.Value;
                        buscarResolucion.resolucion = modelo.resolucion;
                        buscarResolucion.fechaini = fecha1;
                        buscarResolucion.fechafin = fecha2;
                        buscarResolucion.consecini = modelo.consecini;
                        buscarResolucion.consecfin = modelo.consecfin;
                        buscarResolucion.numfacturas = modelo.numfacturas;
                        buscarResolucion.diasaviso = modelo.diasaviso;
                        buscarResolucion.consecaviso = modelo.consecaviso;
                        buscarResolucion.estado = modelo.estado;
                        buscarResolucion.razon_inactivo = modelo.razon_inactivo;
                        context.Entry(buscarResolucion).State = EntityState.Modified;
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            ConsultaDatosCreacion(modelo);
                            TempData["mensaje"] = "La actualización de la resolucion fue exitoso!";
                        }
                    }
                    else
                    {
                        TempData["mensaje_error"] =
                            "La fecha inicial no puede ser mayor que la fecha final, por favor valide!";
                    }
                }
                else
                {
                    var fecha1 = DateTime.Now;
                    var convertir1 = DateTime.TryParse(modelo.fechaini, out fecha1);

                    var fecha2 = DateTime.Now;
                    var convertir2 = DateTime.TryParse(modelo.fechafin, out fecha2);
                    if (convertir1 == true && convertir2 == true && fecha1 < fecha2)
                    {
                        buscarResolucion = new resolucionfactura();
                        buscarResolucion.fec_actualizacion = DateTime.Now;
                        buscarResolucion.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarResolucion.tipodoc = modelo.tipodoc.Value;
                        buscarResolucion.resolucion = modelo.resolucion;
                        buscarResolucion.fechaini = fecha1;
                        buscarResolucion.fechafin = fecha2;
                        buscarResolucion.consecini = modelo.consecini;
                        buscarResolucion.consecfin = modelo.consecfin;
                        buscarResolucion.numfacturas = modelo.numfacturas;
                        buscarResolucion.diasaviso = modelo.diasaviso;
                        buscarResolucion.consecaviso = modelo.consecaviso;
                        buscarResolucion.estado = modelo.estado;
                        buscarResolucion.razon_inactivo = modelo.razon_inactivo;
                        modelo.fec_creacion = DateTime.Now;
                        modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                        context.resolucionfactura.Add(buscarResolucion);
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            TempData["mensaje"] = "El registro de la nueva resolucion de vehiculo fue exitoso!";
                            ViewBag.tipodoc = context.tp_doc_registros
                                .Where(x => x.tipo == 2 || x.tipo == 4 || x.tipo == 5 || x.tipo == 37).ToList();
                            return RedirectToAction("Create", "DocumentoPorBodega", new { menu });
                        }
                    }
                    else
                    {
                        TempData["mensaje_error"] =
                            "La fecha inicial no puede ser mayor que la fecha final, por favor valide!";
                    }
                }

                //var buscarSiExiste = context.resolucionfactura.FirstOrDefault(x=>x.tipodoc==modelo.tipodoc && x.grupo==modelo.grupo);
                //if (buscarSiExiste != null)
                //{
                //    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                //}
                //else {
                //    modelo.fec_creacion = DateTime.Now;
                //    modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                //    context.resolucionfactura.Add(modelo);
                //    var guardar = context.SaveChanges();
                //    if (guardar > 0) {
                //        TempData["mensaje"] = "El registro de la nueva resolucion de vehiculo fue exitoso!";
                //        ViewBag.tipodoc = context.tp_doc_registros.ToList();
                //        return RedirectToAction("Create","DocumentoPorBodega");
                //    }
                //    else{
                //        TempData["mensaje_error"] = "Error en base de datos, verifique su conexion!";
                //    }
                //}
            }

            ViewBag.tipodoc = context.tp_doc_registros
                .Where(x => x.tipo == 2 || x.tipo == 4 || x.tipo == 5 || x.tipo == 37).ToList();
            ViewBag.tipoDocSeleccionado = modelo.tipodoc;
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public ActionResult update(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            resolucionfactura resolucion = context.resolucionfactura.FirstOrDefault(x => x.id == id);
            if (resolucion == null)
            {
                return HttpNotFound();
            }
            var modelo = new resolucionForm();
            modelo.consecaviso = resolucion.consecaviso;
            modelo.consecfin = resolucion.consecfin;
            modelo.consecini = resolucion.consecini;
            modelo.diasaviso = resolucion.diasaviso;
            modelo.estado = resolucion.estado;
            modelo.fechafin = resolucion.fechafin.ToString("yyyy/MM/dd", new CultureInfo("en-US"));
            modelo.fechaini = resolucion.fechaini.ToString("yyyy/MM/dd", new CultureInfo("en-US"));
            modelo.fec_actualizacion = resolucion.fec_actualizacion;
            modelo.fec_creacion = resolucion.fec_creacion;
            modelo.grupo = resolucion.grupo;
            modelo.id = resolucion.id;
            modelo.id_licencia = resolucion.id_licencia;
            modelo.numfacturas = resolucion.numfacturas;
            modelo.razon_inactivo = resolucion.razon_inactivo;
            modelo.resolucion = resolucion.resolucion;
            modelo.tipodoc = resolucion.tipodoc;
            modelo.userid_creacion = resolucion.userid_creacion;
            modelo.user_idactualizacion = resolucion.user_idactualizacion;
            ConsultaDatosCreacion(modelo);
            ViewBag.tipodoc = context.tp_doc_registros
                .Where(x => x.tipo == 2 || x.tipo == 4 || x.tipo == 5 || x.tipo == 37).ToList();
            ViewBag.tipodoc_id = modelo.tipodoc;
            BuscarFavoritos(menu);
            return View(modelo);
        }


        [HttpPost]
        public ActionResult update(resolucionForm modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                resolucionfactura buscarResolucion = context.resolucionfactura.FirstOrDefault(x => x.tipodoc == modelo.tipodoc);
                if (buscarResolucion != null)
                {
                    if (buscarResolucion.id == modelo.id)
                    {
                        var fecha1 = DateTime.Now;
                        var convertir1 = DateTime.TryParse(modelo.fechaini, out fecha1);

                        var fecha2 = DateTime.Now;
                        var convertir2 = DateTime.TryParse(modelo.fechafin, out fecha2);

                        buscarResolucion.fec_actualizacion = DateTime.Now;
                        buscarResolucion.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarResolucion.tipodoc = modelo.tipodoc.Value;
                        buscarResolucion.resolucion = modelo.resolucion;
                        buscarResolucion.fechaini = fecha1;                     
                        buscarResolucion.fechafin = fecha2;
                        buscarResolucion.consecini = modelo.consecini;
                        buscarResolucion.consecfin = modelo.consecfin;
                        buscarResolucion.numfacturas = modelo.numfacturas;
                        buscarResolucion.diasaviso = modelo.diasaviso;
                        buscarResolucion.consecaviso = modelo.consecaviso;
                        buscarResolucion.estado = modelo.estado;
                        buscarResolucion.razon_inactivo = modelo.razon_inactivo;
                        context.Entry(buscarResolucion).State = EntityState.Modified;
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            ConsultaDatosCreacion(modelo);
                            TempData["mensaje"] = "La actualización de la resolucion fue exitoso!";
                        }
                    }
                    else
                    {
                        ConsultaDatosCreacion(modelo);
                        TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                    }
                }
                else
                {
                    var fecha1 = DateTime.Now;
                    var convertir1 = DateTime.TryParse(modelo.fechaini, out fecha1);

                    var fecha2 = DateTime.Now;
                    var convertir2 = DateTime.TryParse(modelo.fechafin, out fecha2);

                    resolucionfactura buscarResolucionPorId = context.resolucionfactura.FirstOrDefault(x => x.id == modelo.id);
                    buscarResolucionPorId.fec_actualizacion = DateTime.Now;
                    buscarResolucionPorId.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    buscarResolucionPorId.tipodoc = modelo.tipodoc.Value;
                    buscarResolucionPorId.resolucion = modelo.resolucion;
                    buscarResolucionPorId.fechaini = fecha1;
                    buscarResolucionPorId.fechafin = fecha2;
                    buscarResolucionPorId.consecini = modelo.consecini;
                    buscarResolucionPorId.consecfin = modelo.consecfin;
                    buscarResolucionPorId.numfacturas = modelo.numfacturas;
                    buscarResolucion.diasaviso = modelo.diasaviso;
                    buscarResolucion.consecaviso = modelo.consecaviso;
                    buscarResolucionPorId.estado = modelo.estado;
                    buscarResolucionPorId.razon_inactivo = modelo.razon_inactivo;
                    context.Entry(buscarResolucionPorId).State = EntityState.Modified;
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "La actualización de la resolucion fue exitoso!";
                        ConsultaDatosCreacion(modelo);
                    }
                }
            }

            ViewBag.tipodoc = context.tp_doc_registros
                .Where(x => x.tipo == 2 || x.tipo == 4 || x.tipo == 5 || x.tipo == 37).ToList();
            ViewBag.tipodoc_id = modelo.tipodoc;
            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public void ConsultaDatosCreacion(resolucionForm resolucion)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(resolucion.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(resolucion.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult GetResolucionesJson()
        {
            var buscarResoluciones = (from resolucion in context.resolucionfactura
                                      join tipoDocumento in context.tp_doc_registros
                                          on resolucion.tipodoc equals tipoDocumento.tpdoc_id
                                      select new
                                      {
                                          resolucion.id,
                                          tipoDocumento.prefijo,
                                          tipoDocumento.tpdoc_nombre,
                                          resolucion.resolucion,
                                          resolucion.fechaini,
                                          resolucion.fechafin,
                                          resolucion.consecini,
                                          resolucion.consecfin,
                                          resolucion.numfacturas,
                                          resolucion.estado,
                                          resolucion.grupo
                                      }).ToList();
            var data = buscarResoluciones.Select(x => new
            {
                x.id,
                x.prefijo,
                x.tpdoc_nombre,
                x.resolucion,
                fechaini = x.fechaini.ToShortDateString(),
                fechafin = x.fechafin.ToShortDateString(),
                x.consecini,
                x.consecfin,
                x.numfacturas,
                estado = x.estado ? "Activo" : "Inactivo"
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