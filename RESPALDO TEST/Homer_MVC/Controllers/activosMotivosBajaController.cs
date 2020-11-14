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
    public class activosMotivosBajaController : Controller
    {
        // GET: activosMotivosBaja
        private readonly Iceberg_Context context = new Iceberg_Context();


        public ActionResult Create(activoMotivoBajasModelo activomotivobajasmodelo)
        {
            return View(activomotivobajasmodelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(activoMotivoBajasModelo activomotivobajasmodelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //var buscarDato = context.activoclasificacion.FirstOrDefault(x => x.id == activoclasificacion.id);
                motivobajaactivo buscarDato =
                    context.motivobajaactivo.FirstOrDefault(x => x.Descripcion == activomotivobajasmodelo.descripcion);
                if (buscarDato == null)
                {
                    motivobajaactivo modeloActivo = new motivobajaactivo
                    {
                        Descripcion = activomotivobajasmodelo.descripcion,
                        fec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        estado = activomotivobajasmodelo.estado,
                        razon_inactivo = activomotivobajasmodelo.razon_inactivo
                    };
                    context.motivobajaactivo.Add(modeloActivo);
                    context.SaveChanges();

                    TempData["mensaje"] = "La creación del Motivo de Baja del Activo fue exitoso";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro ingresado ya existe, por favor valide";
            }

            return View(activomotivobajasmodelo);
        }

        public ActionResult Update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            motivobajaactivo act_baja = context.motivobajaactivo.Find(id);
            if (act_baja == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(act_baja.userid_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(act_baja.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }

            //	motivobajaactivo anio_modelo = new motivobajaactivo();
            activoMotivoBajasModelo modelo = new activoMotivoBajasModelo
            {
                id = act_baja.id,
                descripcion = act_baja.Descripcion,
                fec_creacion = act_baja.fec_creacion.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                userid_creacion = act_baja.userid_creacion,
                fec_actualizacion = act_baja.fec_actualizacion != null
                    ? act_baja.fec_actualizacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : "",
                user_idactualizacion = act_baja.user_idactualizacion ?? 0,
                estado = act_baja.estado,
                razon_inactivo = act_baja.razon_inactivo
            };
            //BuscarFavoritos(menu);
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(activoMotivoBajasModelo activomotivobajasmodelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                int nom = (from a in context.motivobajaactivo
                           where a.Descripcion == activomotivobajasmodelo.descripcion && a.id == activomotivobajasmodelo.id
                           select a.Descripcion).Count();

                if (nom == 1)
                {
                    motivobajaactivo modeloActual =
                        context.motivobajaactivo.FirstOrDefault(x =>
                            x.Descripcion == activomotivobajasmodelo.descripcion);
                    modeloActual.fec_actualizacion = DateTime.Now;
                    modeloActual.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modeloActual.estado = activomotivobajasmodelo.estado;
                    modeloActual.razon_inactivo = activomotivobajasmodelo.razon_inactivo;
                    context.Entry(modeloActual).State = EntityState.Modified;
                    context.SaveChanges();

                    TempData["mensaje"] = "La actualización del Motivo de Baja del Activo fue exitoso!";
                    ConsultaDatosCreacion(modeloActual);
                    return View(activomotivobajasmodelo);
                }

                TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
            }

            motivobajaactivo modeloAux =
                context.motivobajaactivo.FirstOrDefault(x => x.Descripcion == activomotivobajasmodelo.descripcion);
            ConsultaDatosCreacion(modeloAux);
            return View(activomotivobajasmodelo);
        }

        public void ConsultaDatosCreacion(motivobajaactivo mot_baja)
        {
            if (mot_baja != null)
            {
                //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
                users creator = context.users.Find(mot_baja.userid_creacion);
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

                users modificator = context.users.Find(mot_baja.user_idactualizacion);
                if (modificator != null)
                {
                    ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                    ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
                }
            }
        }


        public JsonResult BuscarMotivosBajaPaginados()
        {
            var data = from clasif in context.motivobajaactivo
                       select new
                       {
                           clasif.id,
                           clasif.Descripcion,
                           estado = clasif.estado ? "Activo" : "Inactivo"
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}