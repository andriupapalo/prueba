using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class estilo_vhController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        public ActionResult Create()
        {
            ViewBag.mod_vh_id = new SelectList(context.modelo_vehiculo, "modvh_id", "modvh_nombre");
            estilo_vehiculo estVh = new estilo_vehiculo
            {
                estvh_estado = true,
                estvhrazoninactivo = "no aplica"
            };
            return View(estVh);
        }


        // POST: est_vh/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(estilo_vehiculo est_vh)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                estilo_vehiculo modSearch = context.estilo_vehiculo.FirstOrDefault(x => x.estvh_nombre == est_vh.estvh_nombre
                                                                            && x.mod_vh_id == est_vh.mod_vh_id &&
                                                                            x.estvh_estado == est_vh.estvh_estado);

                if (modSearch == null)
                {
                    est_vh.estvhfec_creacion = DateTime.Now;
                    est_vh.estvhuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.estilo_vehiculo.Add(est_vh);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro del nuevo estilo de vehiculo fue exitoso!";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            ViewBag.mod_vh_id = new SelectList(context.modelo_vehiculo, "modvh_id", "modvh_nombre");
            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";
            return View(est_vh);
        }


        // GET: est_vh/Edit/5
        public ActionResult update(int? id)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            estilo_vehiculo est_vh = context.estilo_vehiculo.Find(id);
            if (est_vh == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(est_vh.estvhuserid_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(est_vh.estvhuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }

            ViewBag.mod_vh_id = new SelectList(context.modelo_vehiculo, "modvh_id", "modvh_nombre", est_vh.mod_vh_id);
            return View(est_vh);
        }


        // POST: est_vh/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(estilo_vehiculo est_vh)
        {
            if (ModelState.IsValid)
            {
                estilo_vehiculo modSearch = context.estilo_vehiculo.FirstOrDefault(x => x.estvh_nombre == est_vh.estvh_nombre
                                                                            && x.mod_vh_id == est_vh.mod_vh_id);

                if (modSearch == null)
                {
                    est_vh.estvhfec_actualizacion = DateTime.Now;
                    est_vh.estvhuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    //if (est_vh.estvh_estado)
                    //{
                    //    est_vh.estvhrazoninactivo = "";
                    //}
                    context.Entry(est_vh).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del estilo fue exitoso!";
                    return RedirectToAction("update");
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            ViewBag.mod_vh_id = new SelectList(context.modelo_vehiculo, "modvh_id", "modvh_nombre", est_vh.mod_vh_id);
            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";
            return View(est_vh);
        }


        //Busqueda filtro
        [HttpPost]
        public ActionResult BuscarEst_vh(string text)
        {
            System.Collections.Generic.List<estilo_vehiculo> results = context.estilo_vehiculo.Where(x => x.estvh_nombre.Contains(text)).ToList();

            return PartialView("Buscarest_vhPartial", results);
        }
    }
}