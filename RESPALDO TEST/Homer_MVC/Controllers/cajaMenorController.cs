using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class cajaMenorController : Controller
    {
        // GET: cajaMenor
        private readonly Iceberg_Context db = new Iceberg_Context();
        private readonly int nroDoc = 3067;
        CultureInfo miCultura = new CultureInfo("is-IS");

        public List<bodegaCajaMenor> bodegas()
        {
            // id del prfijo del documento que corresponde a Reembolso de Caja
            List<bodegaCajaMenor> bodegas = (from consecutivos in db.icb_doc_consecutivos
                                             join bodega in db.bodega_concesionario
                                                 on consecutivos.doccons_bodega equals bodega.id
                                             where consecutivos.doccons_idtpdoc == nroDoc
                                             select new bodegaCajaMenor
                                             {
                                                 id = bodega.id,
                                                 bodccs_nombre = bodega.bodccs_nombre
                                             }).Distinct().OrderBy(bn => bn.bodccs_nombre).ToList();
            return bodegas;
        }

        public List<usuarioList> usuarios()
        {
            List<usuarioList> usuarios = db.icb_terceros
                .Where(x => x.tercero_id > 0)
                .Select(x => new usuarioList
                {
                    user_id = x.tercero_id,
                    user_nombre = x.prinom_tercero + " " + x.segnom_tercero + " " + x.apellido_tercero + " " +
                                  x.segapellido_tercero
                }).ToList();
            return usuarios;
        }

        public void save_bodega(cajaMenorModel cajaM)
        {
            caja_menor_bodega cmb = new caja_menor_bodega
            {
                cjm_id = cajaM.cjm_id,
                id_bodega = Convert.ToInt32(cajaM.id_bodega),
                cjmb_fecela = DateTime.Now,
                cjmb_usuela = Convert.ToInt32(Session["user_usuarioid"])
            };
            db.caja_menor_bodega.Add(cmb);
            db.SaveChanges();
        }

        public ActionResult caja(int? id)
        {
            cajaMenorModel cajaMenor = new cajaMenorModel();
            List<bodegaCajaMenor> bodega = bodegas();

            if (id != null)
            {
                ViewBag.tab = 1;
                cajaMenor.cjm_id = 0;
                cajaMenor.cjm_estado = true;
                if (id > 0)
                {
                    caja_menor cm = db.caja_menor.FirstOrDefault(x => x.cjm_id == id);
                    caja_menor_bodega bd = db.caja_menor_bodega.FirstOrDefault(x => x.cjm_id == id);

                    cajaMenor.cjm_id = cm.cjm_id;
                    cajaMenor.cjm_desc = cm.cjm_desc;
                    cajaMenor.id_bodega = Convert.ToInt32(bd.id_bodega);
                    cajaMenor.cjm_estado = Convert.ToBoolean(cm.cjm_estado);
                    cajaMenor.cjm_razoninactivo = cm.cjm_razoninactivo;
                    cajaMenor.cjm_valor = Convert.ToDecimal(cm.cjm_valor, miCultura);
                    cajaMenor.id_responsable = Convert.ToInt32(cm.id_responsable);
                }
            }
            else
            {
                cajaMenor.cjm_id = 0;
                cajaMenor.cjm_estado = true;
                ViewBag.tab = 2;
            }

            ViewBag.id_bodega = new SelectList(bodegas(), "id", "bodccs_nombre", cajaMenor.id_bodega);
            ViewBag.id_responsable = new SelectList(usuarios(), "user_id", "user_nombre", cajaMenor.id_responsable);
            return View(cajaMenor);
        }

        [HttpPost]
        public ActionResult caja(cajaMenorModel cajaM)
        {
            ViewBag.tab = 1;
            if (ModelState.IsValid)
            {
                if (cajaM.cjm_id > 0)
                {
                    caja_menor cm = db.caja_menor.FirstOrDefault(x => x.cjm_id == cajaM.cjm_id);
                    cm.cjm_desc = cajaM.cjm_desc;
                    cm.id_responsable = cajaM.id_responsable;
                    cm.cjm_valor = cajaM.cjm_valor;
                    cm.cjm_estado = cajaM.cjm_estado;
                    cm.cjm_razoninactivo = cajaM.cjm_razoninactivo;
                    cm.cjm_usumod = Convert.ToInt32(Session["user_usuarioid"]);
                    cm.cjm_fecmod = DateTime.Now;
                    db.Entry(cm).State = EntityState.Modified;
                    db.SaveChanges();

                    // Asociar las bodegas al usuario
                    TempData["mensaje"] = "Se ha actualizado la caja";
                    return Redirect("./caja?id=" + cm.cjm_id);
                }

                caja_menor cajaMenor = db.caja_menor.FirstOrDefault(x => x.cjm_desc == cajaM.cjm_desc);
                if (cajaMenor == null)
                {
                    caja_menor cm = new caja_menor
                    {
                        cjm_desc = cajaM.cjm_desc,
                        id_responsable = cajaM.id_responsable,
                        cjm_valor = cajaM.cjm_valor,
                        cjm_estado = cajaM.cjm_estado,
                        cjm_razoninactivo = cajaM.cjm_razoninactivo,
                        cjm_usuela = Convert.ToInt32(Session["user_usuarioid"]),
                        cjm_fecela = DateTime.Now
                    };
                    db.caja_menor.Add(cm);
                    db.SaveChanges();
                    cajaM.cjm_id = cm.cjm_id;
                    // Asociar las bodegas al usuario
                    save_bodega(cajaM);

                    TempData["mensaje"] = "Se ha guardado la caja";
                    return Redirect("./caja?id=" + cm.cjm_id);
                    //return View(cajaM);
                }

                TempData["mensaje_error"] = "Ya hay una caja con ese nombre.";
            }
            else
            {
                TempData["mensaje_error"] = "Valide la informacion ingresada.";
            }

            ViewBag.id_responsable = new SelectList(usuarios(), "user_id", "user_nombre", cajaM.id_responsable);
            ViewBag.id_bodega = new SelectList(bodegas(), "id", "bodccs_nombre", cajaM.id_bodega);
            return View(cajaM);
        }

        public JsonResult tablaPaginada()
        {
            var cajaMenor = (from a in db.caja_menor
                             join b in db.icb_terceros on a.id_responsable equals b.tercero_id
                             where a.cjm_estado == true
                             select new
                             {
                                 a.cjm_id,
                                 a.cjm_desc,
                                 responsable = b.prinom_tercero + " " + b.segnom_tercero + " " + b.apellido_tercero + " " +
                                               b.segapellido_tercero,
                                 a.cjm_valor
                             }).ToList();
            return Json(cajaMenor, JsonRequestBehavior.AllowGet);
        }
    }
}