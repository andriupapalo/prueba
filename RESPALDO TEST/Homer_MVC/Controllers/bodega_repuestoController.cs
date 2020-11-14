using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class bodega_repuestoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public ActionResult Create()
        {
            bodega_repuesto createBodRepuesto = new bodega_repuesto
            {
                bodrpto_estado = true,
                bodrptorazoninactivo = "No aplica"
            };

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 44);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            return View(createBodRepuesto);
        }

        // POST: bod_rpto/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(bodega_repuesto bod_rpto)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.bodega_repuesto
                           where a.bodrpto_nombre == bod_rpto.bodrpto_nombre
                           select a.bodrpto_nombre).Count();

                if (nom == 0)
                {
                    bod_rpto.bodrptofec_creacion = DateTime.Now;
                    bod_rpto.bodrptouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.bodega_repuesto.Add(bod_rpto);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro de la nueva bodega fue exitoso!";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 44);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            return View(bod_rpto);
        }


        // GET: bod_rpto/Edit/5
        public ActionResult update(int? id)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            bodega_repuesto bod_rpto = context.bodega_repuesto.Find(id);
            if (bod_rpto == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(bod_rpto.bodrptouserid_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(bod_rpto.bodrptouserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 44);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            return View(bod_rpto);
        }


        // POST: bod_rpto/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(bodega_repuesto bod_rpto)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.bodega_repuesto
                           where a.bodrpto_nombre == bod_rpto.bodrpto_nombre || a.bodrpto_id == bod_rpto.bodrpto_id
                           select a.bodrpto_nombre).Count();

                if (nom == 1)
                {
                    bod_rpto.bodrptofec_actualizacion = DateTime.Now;
                    bod_rpto.bodrptouserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(bod_rpto).State = EntityState.Modified;
                    context.SaveChanges();
                    ConsultaDatosCreacion(bod_rpto);
                    TempData["mensaje"] = "La actualización de la bodega fue exitoso!";
                    return View(bod_rpto);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            ConsultaDatosCreacion(bod_rpto);
            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 44);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            return View(bod_rpto);
        }


        public void ConsultaDatosCreacion(bodega_repuesto bod_rpto)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(bod_rpto.bodrptouserid_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = context.users.Find(bod_rpto.bodrptouserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarBodegaRptoPaginadas()
        {
            var data = context.bodega_repuesto.ToList().Select(x => new
            {
                x.bodrpto_id,
                x.bodrpto_nombre,
                bodrpto_estado = x.bodrpto_estado ? "Activo" : "Inactivo"
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}