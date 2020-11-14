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
    public class amortizacionClasificacionController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        // GET: activosClasificacion

        public ActionResult Create(amortizacionClasificacionModel activoclasificacion)
        {
            var cuentasActivos = (from cuentaA in context.cuenta_puc
                                  where cuentaA.cuenta == "1592"
                                  select new
                                  {
                                      cuentaA.cntpuc_id,
                                      cuentaA.cntpuc_numero,
                                      cuentaA.cntpuc_descp,
                                      nombre = "(" + cuentaA.cntpuc_numero + ") " + cuentaA.cntpuc_descp
                                  }).ToList();
            ViewBag.cuentaactivo = new SelectList(cuentasActivos, "cntpuc_id", "nombre");
            var cuentasDepre = (from cuentaD in context.cuenta_puc
                                where cuentaD.cuenta == "5160"
                                select new
                                {
                                    cuentaD.cntpuc_id,
                                    cuentaD.cntpuc_numero,
                                    cuentaD.cntpuc_descp,
                                    nombre = "(" + cuentaD.cntpuc_numero + ") " + cuentaD.cntpuc_descp
                                }).ToList();
            ViewBag.cuentadepre = new SelectList(cuentasDepre, "cntpuc_id", "nombre");

            return View(activoclasificacion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(amortizacionClasificacionModel activoclasificacion, int? menu)
        {
            if (ModelState.IsValid)
            {
                //var buscarDato = context.activoclasificacion.FirstOrDefault(x => x.id == activoclasificacion.id);
                activoclasificacion buscarDato =
                    context.activoclasificacion.FirstOrDefault(x => x.Descripcion == activoclasificacion.descripcion);
                if (buscarDato == null)
                {
                    amortizacionclasificacion modeloActivo = new amortizacionclasificacion
                    {
                        Descripcion = activoclasificacion.descripcion,
                        cuentadepre = activoclasificacion.cuentadepre,
                        cuentaactivo = activoclasificacion.cuentaactivo,
                        fec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        estado = activoclasificacion.estado,
                        razon_inactivo = activoclasificacion.razon_inactivo
                    };
                    context.amortizacionclasificacion.Add(modeloActivo);
                    context.SaveChanges();

                    TempData["mensaje"] = "La creación de la Clasificacion del Activo fue exitoso";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro ingresado ya existe, por favor valide";
            }

            return View(activoclasificacion);
        }

        public ActionResult Update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            amortizacionclasificacion act_cla = context.amortizacionclasificacion.Find(id);
            if (act_cla == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(act_cla.userid_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(act_cla.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }

            //	activoclasificacion anio_modelo = new activoclasificacion();
            amortizacionClasificacionModel modelo = new amortizacionClasificacionModel
            {
                id = act_cla.id,
                descripcion = act_cla.Descripcion,
                cuentadepre = act_cla.cuentadepre,
                cuentaactivo = act_cla.cuentaactivo,
                fec_creacion = act_cla.fec_creacion.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                userid_creacion = act_cla.userid_creacion,
                fec_actualizacion = act_cla.fec_actualizacion != null
                    ? act_cla.fec_actualizacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : "",
                user_idactualizacion = act_cla.user_idactualizacion ?? 0,
                estado = act_cla.estado,
                razon_inactivo = act_cla.razon_inactivo
            };
            //BuscarFavoritos(menu);
            var cuentasActivos = (from cuenta in context.cuenta_puc
                                  where cuenta.cuenta == "1592"
                                  select new
                                  {
                                      cuenta.cntpuc_id,
                                      cuenta.cntpuc_numero,
                                      cuenta.cntpuc_descp,
                                      nombre = "(" + cuenta.cntpuc_numero + ") " + cuenta.cntpuc_descp
                                  }).ToList();
            ViewBag.cuentaactivo = new SelectList(cuentasActivos, "cntpuc_id", "nombre", modelo.cuentaactivo);
            var cuentasDepre = (from cuenta in context.cuenta_puc
                                where cuenta.cuenta == "5160"
                                select new
                                {
                                    cuenta.cntpuc_id,
                                    cuenta.cntpuc_numero,
                                    cuenta.cntpuc_descp,
                                    nombre = "(" + cuenta.cntpuc_numero + ") " + cuenta.cntpuc_descp
                                }).ToList();
            ViewBag.cuentadepre = new SelectList(cuentasDepre, "cntpuc_id", "nombre", modelo.cuentadepre);
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(amortizacionClasificacionModel activoclasificacion, int? menu)
        {
            if (ModelState.IsValid)
            {
                int nom = (from a in context.amortizacionclasificacion
                           where a.Descripcion == activoclasificacion.descripcion && a.id == activoclasificacion.id
                           select a.Descripcion).Count();

                if (nom == 1)
                {
                    amortizacionclasificacion modeloActual =
                        context.amortizacionclasificacion.FirstOrDefault(x =>
                            x.Descripcion == activoclasificacion.descripcion);
                    modeloActual.fec_actualizacion = DateTime.Now;
                    modeloActual.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modeloActual.cuentadepre = activoclasificacion.cuentadepre;
                    modeloActual.cuentaactivo = activoclasificacion.cuentaactivo;
                    modeloActual.estado = activoclasificacion.estado;
                    modeloActual.razon_inactivo = activoclasificacion.razon_inactivo;
                    context.Entry(modeloActual).State = EntityState.Modified;
                    context.SaveChanges();

                    TempData["mensaje"] = "La actualización de la Clasificación de Activo fue exitoso!";
                    ConsultaDatosCreacion(modeloActual);
                    return View(activoclasificacion);
                }

                TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
            }

            amortizacionclasificacion modeloAux =
                context.amortizacionclasificacion.FirstOrDefault(x => x.Descripcion == activoclasificacion.descripcion);
            ConsultaDatosCreacion(modeloAux);

            var cuentasActivos = (from cuentaA in context.cuenta_puc
                                  where cuentaA.cuenta == "1592"
                                  select new
                                  {
                                      cuentaA.cntpuc_id,
                                      cuentaA.cntpuc_numero,
                                      cuentaA.cntpuc_descp,
                                      nombre = "(" + cuentaA.cntpuc_numero + ") " + cuentaA.cntpuc_descp
                                  }).ToList();
            ViewBag.cuentaactivo =
                new SelectList(cuentasActivos, "cntpuc_id", "nombre", activoclasificacion.cuentaactivo);
            var cuentasDepre = (from cuenta in context.cuenta_puc
                                where cuenta.cuenta == "5160"
                                select new
                                {
                                    cuenta.cntpuc_id,
                                    cuenta.cntpuc_numero,
                                    cuenta.cntpuc_descp,
                                    nombre = "(" + cuenta.cntpuc_numero + ") " + cuenta.cntpuc_descp
                                }).ToList();
            ViewBag.cuentadepre = new SelectList(cuentasDepre, "cntpuc_id", "nombre", activoclasificacion.cuentadepre);
            return View(activoclasificacion);
        }

        public void ConsultaDatosCreacion(amortizacionclasificacion clasif)
        {
            if (clasif != null)
            {
                //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
                users creator = context.users.Find(clasif.userid_creacion);
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

                users modificator = context.users.Find(clasif.user_idactualizacion);
                if (modificator != null)
                {
                    ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                    ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
                }
            }
        }

        public JsonResult BuscarClasificacionPaginados()
        {
            var data = (from clasif in context.amortizacionclasificacion
                        join cuenAc in context.cuenta_puc
                            on clasif.cuentaactivo equals cuenAc.cntpuc_id
                        join cuenDe in context.cuenta_puc
                            on clasif.cuentadepre equals cuenDe.cntpuc_id
                        select new
                        {
                            clasif.id,
                            clasif.Descripcion,
                            activo = "(" + cuenAc.cntpuc_numero + ") " + cuenAc.cntpuc_descp,
                            deprec = "(" + cuenDe.cntpuc_numero + ") " + cuenDe.cntpuc_descp,
                            estado = clasif.estado ? "Activo" : "Inactivo"
                        }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}