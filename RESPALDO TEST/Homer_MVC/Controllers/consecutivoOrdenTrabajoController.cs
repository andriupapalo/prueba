using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class listaGeneralLeft
    {
        public int id { get; set; }
        public string descripcion { get; set; }
        public int? verif { get; set; }
    }

    public class consecutivoOrdenTrabajoController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        public List<listaGeneralLeft> bodega(int? id)
        {
            List<listaGeneralLeft> bodega = new List<listaGeneralLeft>();
            if (id != null && id > 0)
            {
                bodega = (from b in db.bodega_concesionario
                          join o in db.icb_consecutivo_ot on
                              b.id equals o.otcon_bodega into bo
                          from o in bo.DefaultIfEmpty()
                          select new listaGeneralLeft
                          {
                              id = b.id,
                              descripcion = b.bodccs_nombre,
                              verif = o.otcon_bodega == null ? 0 : o.otcon_bodega
                          }).OrderBy(x => x.descripcion).Where(o => o.verif > 0).ToList();
            }
            else
            {
                bodega = (from b in db.bodega_concesionario
                          join o in db.icb_consecutivo_ot on
                              b.id equals o.otcon_bodega into bo
                          from o in bo.DefaultIfEmpty()
                          select new listaGeneralLeft
                          {
                              id = b.id,
                              descripcion = b.bodccs_nombre,
                              verif = o.otcon_bodega == null ? 0 : o.otcon_bodega
                          }).OrderBy(x => x.descripcion).Where(o => o.verif == 0).ToList();
            }

            return bodega;
        }

        // GET: consecutivoOrdenTrabajo
        public ActionResult consecutivo(int? id)
        {
            consecutivoOrdenTrabajoModel cons_ord = new consecutivoOrdenTrabajoModel();
            if (id == null)
            {
                ViewBag.tab = 2;
                cons_ord.otcon_id = 0;
                cons_ord.otcon_estado = true;
            }
            else
            {
                ViewBag.tab = 1;
                if (id > 0)
                {
                    icb_consecutivo_ot cons_ot = db.icb_consecutivo_ot.FirstOrDefault(x => x.otcon_id == id);
                    cons_ord.otcon_id = cons_ot.otcon_id;
                    cons_ord.otcon_bodega = Convert.ToInt32(cons_ot.otcon_bodega);
                    cons_ord.otcon_prefijo = cons_ot.otcon_prefijo.ToUpper();
                    cons_ord.otcon_consecutivo = Convert.ToInt32(cons_ot.otcon_consecutivo);
                    cons_ord.otcon_estado = Convert.ToBoolean(cons_ot.otcon_estado);
                    cons_ord.otcon_razoninactivo = cons_ot.otcon_razoninactivo;
                }
                else
                {
                    cons_ord.otcon_id = 0;
                    cons_ord.otcon_estado = true;
                }
            }

            ViewBag.otcon_bodega = new SelectList(bodega(id), "id", "descripcion", cons_ord.otcon_bodega);
            return View(cons_ord);
        }

        [HttpPost]
        public ActionResult consecutivo(consecutivoOrdenTrabajoModel cons_ord)
        {
            ViewBag.tab = 1;
            if (ModelState.IsValid)
            {
                icb_consecutivo_ot cons_valid = db.icb_consecutivo_ot.FirstOrDefault(x => x.otcon_prefijo == cons_ord.otcon_prefijo);
                if (cons_ord.otcon_id > 0)
                {
                    icb_consecutivo_ot cons_ot = db.icb_consecutivo_ot.FirstOrDefault(x => x.otcon_id == cons_ord.otcon_id);
                    if (cons_ot != null && (cons_valid.otcon_id == cons_ord.otcon_id && cons_valid.otcon_id > 0 ||
                                            cons_valid == null))
                    {
                        cons_ot.otcon_consecutivo = cons_ord.otcon_consecutivo;
                        cons_ot.otcon_estado = cons_ord.otcon_estado;
                        cons_ot.otcon_razoninactivo = cons_ord.otcon_razoninactivo;
                        cons_ot.otcon_fecmod = DateTime.Now;
                        cons_ot.otcon_usumod = Convert.ToInt32(Session["user_usuarioid"]);
                        db.Entry(cons_ot).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["mensaje"] = "Se ha modificado.";
                    }
                    else
                    {
                        TempData["mensaje_error"] = cons_valid != null
                            ? "El prefijo ya existe."
                            : "No se encontro el registro a actualizar.";
                    }

                    return Redirect("./consecutivo?id=" + cons_ord.otcon_id);
                }

                if (cons_valid == null)
                {
                    icb_consecutivo_ot cons_ot = new icb_consecutivo_ot
                    {
                        otcon_bodega = cons_ord.otcon_bodega,
                        otcon_consecutivo = cons_ord.otcon_consecutivo,
                        otcon_prefijo = cons_ord.otcon_prefijo.ToUpper(),
                        otcon_estado = cons_ord.otcon_estado,
                        otcon_razoninactivo = cons_ord.otcon_razoninactivo,
                        otcon_usuela = Convert.ToInt32(Session["user_usuarioid"]),
                        otcon_fecela = DateTime.Now
                    };
                    db.icb_consecutivo_ot.Add(cons_ot);
                    db.SaveChanges();
                    TempData["mensaje"] = "Se ha guardado.";
                    return Redirect("./consecutivo?id=" + cons_ot.otcon_id);
                }

                TempData["mensaje_error"] = "El prefijo ya existe";
            }
            else
            {
                TempData["mensaje_error"] = "Valide los datos ingresados";
            }

            ViewBag.otcon_bodega = new SelectList(bodega(cons_ord.otcon_id), "id", "descripcion", cons_ord.otcon_id);
            return View(cons_ord);
        }

        public JsonResult tablaPaginadaConsec()
        {
            var tablaConsec = (from o in db.icb_consecutivo_ot
                               join b in db.bodega_concesionario
                                   on o.otcon_bodega equals b.id
                               select new
                               {
                                   o.otcon_id,
                                   o.otcon_prefijo,
                                   b.bodccs_nombre,
                                   o.otcon_consecutivo,
                                   otcon_estado = o.otcon_estado == true ? "Activo" : "Inactivo"
                               }).ToList();
            return Json(tablaConsec, JsonRequestBehavior.AllowGet);
        }
    }
}