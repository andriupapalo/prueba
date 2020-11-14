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
    public class metodosDepreciacionController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        // GET: activosClasificacion

        public ActionResult Create(activoMetodoModelo activometodo)
        {
            return View(activometodo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(activoMetodoModelo activometodo, int? menu)
        {
            if (ModelState.IsValid)
            {
                activometodo buscarDato = context.activometodo.FirstOrDefault(x => x.Descripcion == activometodo.descripcion);
                if (buscarDato == null)
                {
                    activometodo modeloActivo = new activometodo
                    {
                        Descripcion = activometodo.descripcion,
                        fec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        estado = activometodo.estado,
                        razon_inactivo = activometodo.razon_inactivo
                    };
                    context.activometodo.Add(modeloActivo);
                    context.SaveChanges();

                    TempData["mensaje"] = "La creación del Metodo de Depreciación de Activo fue exitoso";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro ingresado ya existe, por favor valide";
            }

            return View(activometodo);
        }

        public ActionResult Update(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            activometodo act_met = context.activometodo.Find(id);
            if (act_met == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(act_met.userid_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(act_met.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }

            //activometodo anio_modelo = new activometodo();
            activoMetodoModelo modelo = new activoMetodoModelo
            {
                id = act_met.id,
                descripcion = act_met.Descripcion,
                fec_creacion = act_met.fec_creacion.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                userid_creacion = act_met.userid_creacion,
                fec_actualizacion = act_met.fec_actualizacion != null
                    ? act_met.fec_actualizacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : "",
                user_idactualizacion = act_met.user_idactualizacion ?? 0,
                estado = act_met.estado,
                razon_inactivo = act_met.razon_inactivo
            };
            //BuscarFavoritos(menu);
            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(activoMetodoModelo activometodo, int? menu)
        {
            if (ModelState.IsValid)
            {
                int nom = (from a in context.activometodo
                           where a.Descripcion == activometodo.descripcion && a.id == activometodo.id
                           select a.Descripcion).Count();

                if (nom == 1)
                {
                    activometodo modeloActual =
                        context.activometodo.FirstOrDefault(x => x.Descripcion == activometodo.descripcion);
                    modeloActual.fec_actualizacion = DateTime.Now;
                    modeloActual.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modeloActual.estado = activometodo.estado;
                    modeloActual.razon_inactivo = activometodo.razon_inactivo;
                    context.Entry(modeloActual).State = EntityState.Modified;
                    context.SaveChanges();

                    TempData["mensaje"] = "La actualización del Metodo de Depreciación del Activo fue exitoso!";
                    ConsultaDatosCreacion(modeloActual);
                    return View(activometodo);
                }

                TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
            }

            activometodo modeloAux = context.activometodo.FirstOrDefault(x => x.Descripcion == activometodo.descripcion);
            ConsultaDatosCreacion(modeloAux);
            return View(activometodo);
        }

        public void ConsultaDatosCreacion(activometodo meto)
        {
            if (meto != null)
            {
                //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
                users creator = context.users.Find(meto.userid_creacion);
                if (creator != null)
                {
                    ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
                }

                users modificator = context.users.Find(meto.user_idactualizacion);
                if (modificator != null)
                {
                    ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                    ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
                }
            }
        }


        public JsonResult BuscarMetodosPaginados()
        {
            var data = from metodo in context.activometodo
                       select new
                       {
                           metodo.id,
                           metodo.Descripcion,
                           estado = metodo.estado ? "Activo" : "Inactivo"
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}