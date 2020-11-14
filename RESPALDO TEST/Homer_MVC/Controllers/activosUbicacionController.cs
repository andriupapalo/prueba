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
    public class activosUbicacionController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        // GET: activosClasificacion

        public ActionResult Create(activoUbicacionModelo activoubicacion)
        {
            return View(activoubicacion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(activoUbicacionModelo activoubicacion, int? menu)
        {
            if (ModelState.IsValid)
            {
                activosfubicacion buscarDato =
                    context.activosfubicacion.FirstOrDefault(x => x.descripcion == activoubicacion.descripcion);
                if (buscarDato == null)
                {
                    activosfubicacion modeloActivo = new activosfubicacion
                    {
                        descripcion = activoubicacion.descripcion,
                        fec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        estado = activoubicacion.estado,
                        razon_inactivo = activoubicacion.razon_inactivo
                    };
                    context.activosfubicacion.Add(modeloActivo);
                    context.SaveChanges();

                    TempData["mensaje"] = "La creación de la Ubicacion del Activo fue exitoso";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro ingresado ya existe, por favor valide";
            }

            return View(activoubicacion);
        }

        public ActionResult Update(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            activosfubicacion act_ubi = context.activosfubicacion.Find(id);
            if (act_ubi == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(act_ubi.userid_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(act_ubi.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }

            activosfubicacion anio_modelo = new activosfubicacion();
            activoUbicacionModelo modelo = new activoUbicacionModelo
            {
                id = act_ubi.id,
                descripcion = act_ubi.descripcion,
                fec_creacion = act_ubi.fec_creacion.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                userid_creacion = act_ubi.userid_creacion,
                fec_actualizacion = act_ubi.fec_actualizacion != null
                    ? act_ubi.fec_actualizacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : "",
                user_idactualizacion = act_ubi.user_idactualizacion ?? 0,
                razon_inactivo = act_ubi.razon_inactivo
            };
            //BuscarFavoritos(menu);
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(activoUbicacionModelo activoubicacion, int? menu)
        {
            if (ModelState.IsValid)
            {
                int nom = (from a in context.activosfubicacion
                           where a.descripcion == activoubicacion.descripcion && a.id == activoubicacion.id
                           select a.descripcion).Count();

                if (nom == 1)
                {
                    activosfubicacion modeloActual =
                        context.activosfubicacion.FirstOrDefault(x => x.descripcion == activoubicacion.descripcion);
                    modeloActual.fec_actualizacion = DateTime.Now;
                    modeloActual.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modeloActual.estado = activoubicacion.estado;
                    modeloActual.razon_inactivo = activoubicacion.razon_inactivo;
                    context.Entry(modeloActual).State = EntityState.Modified;
                    context.SaveChanges();

                    TempData["mensaje"] = "La actualización de la Ubicación del Activo fue exitoso!";
                    ConsultaDatosCreacion(modeloActual);
                    return View(activoubicacion);
                }

                TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
            }

            activosfubicacion modeloAux = context.activosfubicacion.FirstOrDefault(x => x.descripcion == activoubicacion.descripcion);
            ConsultaDatosCreacion(modeloAux);
            return View(activoubicacion);
        }

        public void ConsultaDatosCreacion(activosfubicacion ubica)
        {
            if (ubica != null)
            {
                users creator = context.users.Find(ubica.userid_creacion);
                if (creator != null)
                {
                    ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
                }

                users modificator = context.users.Find(ubica.user_idactualizacion);
                if (modificator != null)
                {
                    ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                    ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
                }
            }

            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
        }


        public JsonResult BuscarUbicacionPaginados()
        {
            var data = from ubica in context.activosfubicacion
                       select new
                       {
                           ubica.id,
                           ubica.descripcion,
                           estado = ubica.estado ? "Activo" : "Inactivo"
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}