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
    public class parametro_retencionesController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        private readonly CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

        // GET: parametro_retenciones
        public ActionResult Create(Modelo_parametroRetencion modeloparametroRetencion)
        {
            //	BuscarFavoritos(menu);
            return View(modeloparametroRetencion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Modelo_parametroRetencion modeloparametroRetencion, int? menu)
        {
            if (ModelState.IsValid)
            {
                tparamretenciones buscarDato = db.tparamretenciones.FirstOrDefault(x => x.id == modeloparametroRetencion.id);
                if (buscarDato == null)
                {
                    tparamretenciones modelore = new tparamretenciones
                    {
                        concepto = modeloparametroRetencion.concepto,
                        baseuvt = modeloparametroRetencion.baseuvt,
                        basepesos = Convert.ToDecimal(modeloparametroRetencion.basepesos)
                    };
                    double importe = 0;
                    if (modeloparametroRetencion.tarifas.Contains(".")
                    ) // si tiene un punto la caja de texto, usa configuracion regional
                    {
                        importe = Convert.ToDouble(modeloparametroRetencion.tarifas, CultureInfo.InvariantCulture);
                    }
                    else // aca quiere decir que puso una coma y lo reemplaza por un punto
                    {
                        string coma = modeloparametroRetencion.tarifas;
                        coma.Replace(',', '.');
                        importe = Convert.ToDouble(coma);
                    }

                    modelore.tarifas = importe;

                    //modelore.tarifas = Convert.ToDouble(modeloparametroRetencion.tarifas);
                    modelore.fec_creacion = DateTime.Now;
                    modelore.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modelore.estado = modeloparametroRetencion.estado;
                    modelore.razon_inactivo = modeloparametroRetencion.razon_inactivo;
                    db.tparamretenciones.Add(modelore);
                    db.SaveChanges();

                    TempData["mensaje"] = "La creación del registro fue exitoso";
                    //return RedirectToAction("Edit", new { id = modeloretenciones.id, menu });
                    return RedirectToAction("Create");
                    //return View();
                }

                TempData["mensaje_error"] = "El registro ingresado ya existe, por favor valide";
            }

            //BuscarFavoritos(menu); 
            return View();
        }

        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tparamretenciones reten = db.tparamretenciones.Find(id);
            if (reten == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(reten.userid_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = db.users.Find(reten.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }

            tparamretenciones ptretencionx = db.tparamretenciones.Find(id);
            Modelo_parametroRetencion mret = new Modelo_parametroRetencion
            {
                id = ptretencionx.id,
                concepto = ptretencionx.concepto,
                baseuvt = ptretencionx.baseuvt,
                basepesos = ptretencionx.basepesos.ToString("0,0", elGR),
                tarifas = ptretencionx.tarifas.ToString("N2"), ///.ToString("0,0", elGR);
                fec_creacion = ptretencionx.fec_creacion,
                userid_creacion = ptretencionx.userid_creacion,
                //mret.fec_actualizacion = ptretencionx.fec_actualizacion;
                //mret.user_idactualizacion = ptretencionx.user_idactualizacion;
                estado = ptretencionx.estado,
                razon_inactivo = ptretencionx.razon_inactivo
            };
            //BuscarFavoritos(menu);
            return View(mret);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Modelo_parametroRetencion modeloparametroRetencion, int? menu)
        {
            if (ModelState.IsValid)
            {
                tparamretenciones cabeceraparRete = db.tparamretenciones.FirstOrDefault(x => x.id == modeloparametroRetencion.id);

                if (cabeceraparRete != null)
                {
                    cabeceraparRete.baseuvt = Convert.ToInt32(modeloparametroRetencion.baseuvt);
                    cabeceraparRete.basepesos = Convert.ToDecimal(modeloparametroRetencion.basepesos);

                    double importe = 0;
                    if (modeloparametroRetencion.tarifas.Contains(".")
                    ) // si tiene un punto la caja de texto, usa configuracion regional
                    {
                        importe = Convert.ToDouble(modeloparametroRetencion.tarifas, CultureInfo.InvariantCulture);
                    }
                    else // aca quiere decir que puso una coma y lo reemplaza por un punto
                    {
                        string coma = modeloparametroRetencion.tarifas;
                        coma.Replace(',', '.');
                        importe = Convert.ToDouble(coma);
                    }

                    cabeceraparRete.tarifas = importe;
                    //cabeceraparRete.tarifas = Convert.ToDouble(modeloparametroRetencion.tarifas);  // xretImp;

                    cabeceraparRete.fec_actualizacion = DateTime.Now;
                    cabeceraparRete.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    cabeceraparRete.estado = modeloparametroRetencion.estado;
                    cabeceraparRete.razon_inactivo = modeloparametroRetencion.razon_inactivo;

                    db.Entry(cabeceraparRete).State = EntityState.Modified;
                    db.SaveChanges();
                }

                TempData["mensaje"] = "Registro editado Correctamente";
            }
            else
            {
                TempData["mensaje_error"] = "error en los datos de la retencion!";

                //BuscarFavoritos(menu);
                //return View(icb_plan_financiero);
                //	return View(modeloretenciones);
            }
            //  ConsultaDatosCreacion(icb_plan_financiero);


            //	BuscarFavoritos(menu);

            return View(modeloparametroRetencion);
        }

        public JsonResult BuscarDatos()
        {
            var data2 = (from rete in db.tparamretenciones
                         select new
                         {
                             rete.id,
                             rete.concepto,
                             rete.baseuvt,
                             rete.basepesos,
                             rete.tarifas,
                             rete.estado
                         }).ToList();
            var data = data2.Select(c => new
            {
                c.id,
                c.concepto,
                baseuvt = c.baseuvt != null ? c.baseuvt.ToString("N0") : "",
                basepesos = c.basepesos != null ? c.basepesos.ToString("N0") : "",
                tarifas = c.tarifas != null ? c.tarifas.ToString("N2") : "",
                xestado = c.estado ? "Activo" : "Inactivo"
                ////userfec_actualizacion = c.userfec_actualizacion != null ? c.userfec_actualizacion: "",
                //// userfec_actualizacion = c.userfec_actualizacion != null ? c.userfec_actualizacion.Value.ToShortDateString() + " " + c.userfec_actualizacion.Value.ToShortTimeString() : "",
                ////  userfec_actualizacion = c.userfec_actualizacion != null ? c.userfec_actualizacion.ToString(): "",  // ojo 
            }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}