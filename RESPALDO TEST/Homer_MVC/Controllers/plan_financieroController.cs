using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class plan_financieroController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();


        public ActionResult Create(int? menu)
        {
            var bodegas = (from b in db.bodega_concesionario
                           select new
                           {
                               b.id,
                               nombre = "(" + b.bodccs_cod + ") " + b.bodccs_nombre
                           }).ToList();

            List<SelectListItem> lista_bodegas = new List<SelectListItem>();
            foreach (var item in bodegas)
            {
                lista_bodegas.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }

            ViewBag.bodega_id = lista_bodegas;


            var fianancieras = (from fin in db.icb_unidad_financiera //modelopdf.vm_list_icb_terceros
                                select new
                                {
                                    fin.financiera_id,
                                    nombre = fin.financiera_nombre
                                }).ToList();
            ViewBag.idfinanciera = new SelectList(fianancieras, "financiera_id", "nombre");
            PlanFinancieroForm modelo = new PlanFinancieroForm();
            BuscarFavoritos(menu);
            return View(modelo);
        }

        // POST: plan_financiero/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( PlanFinancieroForm icb_plan_financiero, HttpPostedFileBase imagen, int? menu)
        {

            string[] Vectorbodegas = Request["txtBodegas"].Split(',');

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {


                        double? paas = !string.IsNullOrEmpty(icb_plan_financiero.tasa_interes) ? Convert.ToDouble(icb_plan_financiero.tasa_interes, new CultureInfo("is-IS")):0;
                        double? paas2 = !string.IsNullOrEmpty(icb_plan_financiero.plan_porcentaje_comision) ? Convert.ToDouble(icb_plan_financiero.plan_porcentaje_comision, new CultureInfo("is-IS")) : 0;

                        double psi = Convert.ToDouble(Request["tasa_interes"], new CultureInfo("is-IS"));
                        string psi2 = Request["tasa_interes"];
                        float psi3 = float.Parse(Request["tasa_interes"], new CultureInfo("is-IS"));
                        
                        icb_plan_financiero existe =
                            db.icb_plan_financiero.FirstOrDefault(x =>
                                x.plan_nombre == icb_plan_financiero.plan_nombre);
                        if (existe == null)
                        {
                            icb_plan_financiero.plan_fecela = DateTime.Now;
                            icb_plan_financiero.plan_usuela = Convert.ToInt32(Session["user_usuarioid"]);
                            //icb_plan_financiero.tasa_interes = psi3;
                            /*
                            if (imagen != null)
                            {
                                icb_plan_financiero.plan_imagen = icb_plan_financiero.plan_id + "_" + imagen.FileName;
                                string path = Server.MapPath("~/Content/img/" + icb_plan_financiero.plan_id + "_" +
                                                          imagen.FileName);
                                imagen.SaveAs(path);
                            }*/

                            var planfi = new icb_plan_financiero
                            {
                                idfinanciera = icb_plan_financiero.idfinanciera.Value,
                                plan_comision=icb_plan_financiero.plan_comision,
                                plan_nombre=icb_plan_financiero.plan_nombre,
                                plan_porcentaje_comision= paas2,
                                plan_descripcion=icb_plan_financiero.plan_descripcion,
                                plan_estado=icb_plan_financiero.plan_estado,
                                plan_fecela=DateTime.Now,
                                plan_razon_inactivo=icb_plan_financiero.plan_razon_inactivo,
                                plan_usuela=icb_plan_financiero.plan_usuela,
                                tasa_interes=paas,                                
                            };
                            db.icb_plan_financiero.Add(planfi);
                            db.SaveChanges();


                            if (Vectorbodegas.Count() > 0)
                            {
                                foreach (string j in Vectorbodegas)
                                {
                                    if (!string.IsNullOrEmpty(j))
                                    {
                                        int bod = Convert.ToInt32(j);
                                        int bodegaviene = Convert.ToInt32(bod);
                                        int planviene = planfi.plan_id;
                                        planfinancierobodega existe2 = db.planfinancierobodega.FirstOrDefault(x =>
                                            x.idplanfinanciero == planfi.idfinanciera &&
                                            x.idbodega == bodegaviene);
                                        if (existe2 == null)
                                        {
                                            db.planfinancierobodega.Add(new planfinancierobodega
                                            {
                                                idbodega = bodegaviene,
                                                idplanfinanciero = planviene,
                                                // idplanfinanciero = icb_plan_financiero.idfinanciera,
                                                //float.Parse(ncm.por_retencion, System.Globalization.CultureInfo.InvariantCulture);//esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal
                                                //porcentaje = Convert.ToDecimal(icb_plan_financiero.plan_porcentaje_comision),
                                                porcentaje = Convert.ToDouble(
                                                    planfi.plan_porcentaje_comision,
                                                    new CultureInfo("is-Is")), //esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal
                                                fechadesde = Convert.ToDateTime(Request["txtFechaInicio"]),
                                                fechahasta = Convert.ToDateTime(Request["txtFechaFin"]),
                                                estado = planfi.plan_estado,
                                                razoninactividad = planfi.plan_razon_inactivo
                                            });
                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }

                            TempData["mensaje"] = "Registro Creado Correctamente";
                            dbTran.Commit();
                            return RedirectToAction("Edit", new { id = planfi.plan_id, menu });
                        }

                        if (Vectorbodegas.Count() > 0)
                        {
                            foreach (string j in Vectorbodegas)
                            {
                                if (!string.IsNullOrEmpty(j))
                                {
                                    int bod = Convert.ToInt32(j);
                                    int bodegaviene = Convert.ToInt32(bod);
                                    int planviene = icb_plan_financiero.plan_id;
                                    planfinancierobodega existe2 = db.planfinancierobodega.FirstOrDefault(x =>
                                        x.idplanfinanciero == icb_plan_financiero.idfinanciera &&
                                        x.idbodega == bodegaviene);
                                    if (existe2 == null)
                                    {
                                        db.planfinancierobodega.Add(new planfinancierobodega
                                        {
                                            idbodega = bodegaviene,
                                            idplanfinanciero = planviene,
                                            // idplanfinanciero = icb_plan_financiero.idfinanciera,
                                            porcentaje = Convert.ToDouble(icb_plan_financiero.plan_porcentaje_comision,
                                                CultureInfo
                                                    .InvariantCulture), //esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal //Convert.ToDecimal(icb_plan_financiero.plan_porcentaje_comision),
                                            fechadesde = Convert.ToDateTime(Request["txtFechaInicio"]),
                                            fechahasta = Convert.ToDateTime(Request["txtFechaFin"]),
                                            estado = icb_plan_financiero.plan_estado,
                                            razoninactividad = icb_plan_financiero.plan_razon_inactivo
                                        });
                                        db.SaveChanges();
                                        dbTran.Commit();
                                    }
                                }
                            }
                        }
                        //var 

                        //db.Entry(existe).State = EntityState.Modified;
                        //var actualizar = db.SaveChanges();

                        TempData["mensaje_error"] =
                            "El plan ingresado ya existe solo se agragaron las Bodegas , por favor valide";
                        dbTran.Rollback();
                    }
                    catch (DbEntityValidationException)
                    {
                        dbTran.Rollback();
                        throw;
                    }
                }
            }
            else
            {
                TempData["mensaje_error"] = "Error en la creación del registro, por favor valide";
            }

            ViewBag.plan = icb_plan_financiero.plan_id;

            var bodegas = (from b in db.bodega_concesionario
                           select new
                           {
                               b.id,
                               nombre = "(" + b.bodccs_cod + ") " + b.bodccs_nombre
                           }).ToList();

            List<SelectListItem> lista_bodegas = new List<SelectListItem>();
            foreach (var item in bodegas)
            {
                lista_bodegas.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString(),
                    Selected = Vectorbodegas.Contains(item.id.ToString()) ? true : false
                });
            }

            var fianancieras = (from fin in db.icb_unidad_financiera //modelopdf.vm_list_icb_terceros
                                select new
                                {
                                    fin.financiera_id,
                                    nombre = fin.financiera_nombre
                                }).ToList();
            ViewBag.idfinanciera =
                new SelectList(fianancieras, "financiera_id", "nombre", icb_plan_financiero.idfinanciera);
            ViewBag.bodega_id = lista_bodegas;
            BuscarFavoritos(menu);
            return View(icb_plan_financiero);
        }

        // GET: plan_financiero/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //icb_plan_financiero icb_plan_financiero = db.icb_plan_financiero.Find(id);
            //if (icb_plan_financiero == null)
            //{
            //    return HttpNotFound();
            //}
            int xid = id ?? 0;
            ConsultaDatosCreacion(xid);

            icb_plan_financiero planx = db.icb_plan_financiero.Find(id);

            var financiera = (from plan in db.icb_plan_financiero
                              join fina in db.icb_unidad_financiera
                                  on plan.idfinanciera equals fina.financiera_id
                              where plan.plan_id == id
                              select new
                              {
                                  fina.financiera_nombre
                              }).FirstOrDefault();

            var buscarMuchos = (from planbod in db.planfinancierobodega
                                join fin in db.icb_unidad_financiera
                                    on planbod.idplanfinanciero equals fin.financiera_id
                                join bod in db.bodega_concesionario
                                    on planbod.idbodega equals bod.id
                                where planbod.idplanfinanciero == id
                                select new
                                {
                                    planbod.idplanfinanciero,
                                    fin.financiera_nombre,
                                    planbod.idbodega,
                                    bod.bodccs_cod,
                                    bod.bodccs_nombre,
                                    planbod.porcentaje,
                                    planbod.fechadesde,
                                    planbod.fechahasta,
                                    planbod.estado,
                                    planbod.razoninactividad
                                }).ToList();
            List<Listaplanfinancierobodega> bodegasall = buscarMuchos.Select(c => new Listaplanfinancierobodega
            {
                id = c.idbodega,
                idbodega = c.idbodega,
                nomBodega = "(" + c.bodccs_cod + ") " + c.bodccs_nombre,
                idplanfinanciero = c.idplanfinanciero,
                nomFinaciera = financiera.financiera_nombre,
                porcentaje =
                    Convert.ToDouble(c.porcentaje,
                        CultureInfo
                            .InvariantCulture), //esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal //c.porcentaje,
                fechadesde = c.fechadesde != null ? c.fechadesde.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                fechahasta = c.fechahasta != null ? c.fechahasta.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                estado = c.estado,
                Desestado = c.estado ? "Activo" : "Inactivo",
                razoninactividad = c.razoninactividad
            }).ToList();

            string pfa = planx.plan_fecha_actualizacion != null
                ? planx.plan_fecha_actualizacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                : "";
            double ppc = planx.plan_porcentaje_comision != null ? planx.plan_porcentaje_comision.Value : 0;
            int pua = planx.plan_usuario_actualizacion != null ? planx.plan_usuario_actualizacion.Value : 0;

            ModeloPlanFinanciero mpf = new ModeloPlanFinanciero
            {
                plan_id = planx.plan_id,
                plan_descripcion = planx.plan_descripcion,
                plan_usuela = planx.plan_usuela.ToString(),
                plan_estado = planx.plan_estado, // == true ? "Activo":"Inactivo";
                plan_nombre = planx.plan_nombre,
                plan_imagen = planx.plan_imagen,
                plan_comision = planx.plan_comision,
                plan_porcentaje_comision = ppc,
                tasa_interes = planx.tasa_interes,
                //   mpf.plan_usuario_actualizacion = pua;
                //   mpf.plan_fecha_actualizacion = Convert.ToDateTime(pfa);
                plan_razon_inactivo = planx.plan_razon_inactivo,
                idfinanciera = planx.idfinanciera,

                Listaplanfinancierobodega = bodegasall
            };


            //ConsultaDatosCreacion(icb_plan_financiero);
            //  ConsultaDatosCreacion(mpf);

            ViewBag.bodccs_cod = db.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();

            var buscarBodegas = from bodegas in db.planfinancierobodega
                                where bodegas.idplanfinanciero == planx.plan_id
                                select new { bodegas.idbodega };
            string bodegasString = "";
            bool primera = true;
            foreach (var item in buscarBodegas)
            {
                if (primera)
                {
                    bodegasString += item.idbodega;
                    primera = !primera;
                }
                else
                {
                    bodegasString += "," + item.idbodega;
                }
            }

            ViewBag.bodegasSeleccionadas = bodegasString;

            var fianancieras = (from fin in db.icb_unidad_financiera //modelopdf.vm_list_icb_terceros
                                select new
                                {
                                    fin.financiera_id,
                                    nombre = fin.financiera_nombre
                                }).ToList();
            ViewBag.idfinanciera =
                new SelectList(fianancieras, "financiera_id", "nombre", mpf.idfinanciera);

            BuscarFavoritos(menu);
            //return View(icb_plan_financiero);
            ViewBag.plan = id;
            return View(mpf);
        }


        // POST: plan_financiero/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        //	public ActionResult Edit(icb_plan_financiero icb_plan_financiero, HttpPostedFileBase imagen, int? menu)
        public ActionResult Edit(ModeloPlanFinanciero modeloPlanFinanciero, HttpPostedFileBase imagen, int? menu)
        {
            string bodegasSeleccionadas = Request["bodccs_cod"];
            float psi3 = float.Parse(Request["tasa_interes"]);
            ViewBag.bodccs_cod = db.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(bodegasSeleccionadas))
                {
                    TempData["mensaje_error"] = "Debe asignar minimo una bodega!";

                    ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;

                    BuscarFavoritos(menu);
                    //return View(icb_plan_financiero);
                    return View(modeloPlanFinanciero);
                }

                if (!string.IsNullOrEmpty(bodegasSeleccionadas))
                {
                    planfinancierobodega planToBodega = new planfinancierobodega();


                    string[] bodegasId = bodegasSeleccionadas.Split(',');
                    foreach (string substring in bodegasId)
                    {
                        int xbod = Convert.ToInt32(substring);
                        int xplan = Convert.ToInt32(modeloPlanFinanciero.plan_id);

                        planfinancierobodega existeRegistro =
                            db.planfinancierobodega.FirstOrDefault(x =>
                                x.idbodega == xbod && x.idplanfinanciero == xplan);

                        if (existeRegistro != null)
                        {
                            existeRegistro.porcentaje = Convert.ToDouble(modeloPlanFinanciero.plan_porcentaje_comision,
                                CultureInfo.InvariantCulture);
                            existeRegistro.fechadesde = Convert.ToDateTime(Request["txtFechaInicio"]);
                            existeRegistro.fechahasta = Convert.ToDateTime(Request["txtFechaFin"]);
                            db.Entry(existeRegistro).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            planToBodega.idbodega = Convert.ToInt32(substring);
                            planToBodega.idplanfinanciero = modeloPlanFinanciero.plan_id;
                            planToBodega.porcentaje = Convert.ToDouble(modeloPlanFinanciero.plan_porcentaje_comision,
                                CultureInfo.InvariantCulture);
                            planToBodega.fechadesde = Convert.ToDateTime(Request["txtFechaInicio"]);
                            planToBodega.fechahasta = Convert.ToDateTime(Request["txtFechaFin"]);
                            planToBodega.estado = true;
                            db.planfinancierobodega.Add(planToBodega);
                            db.SaveChanges();
                        }
                    }
                }


                icb_plan_financiero cabeceraPlan =
                    db.icb_plan_financiero.FirstOrDefault(x => x.plan_id == modeloPlanFinanciero.plan_id);

                if (cabeceraPlan != null)
                {
                    cabeceraPlan.plan_fecha_actualizacion = DateTime.Now;
                    cabeceraPlan.plan_usuario_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    cabeceraPlan.plan_porcentaje_comision =
                        Convert.ToDouble(modeloPlanFinanciero.plan_porcentaje_comision, CultureInfo.InvariantCulture);
                    cabeceraPlan.tasa_interes =
                        Convert.ToDouble(modeloPlanFinanciero.tasa_interes, CultureInfo.InvariantCulture);
                    if (imagen != null)
                    {
                        cabeceraPlan.plan_imagen = cabeceraPlan.plan_id + "_" + imagen.FileName;
                        string path = Server.MapPath("~/Content/img/" + cabeceraPlan.plan_id + "_" + imagen.FileName);
                        imagen.SaveAs(path);
                    }

                    db.Entry(cabeceraPlan).State = EntityState.Modified;
                    db.SaveChanges();
                }


                //db.Entry(icb_plan_financiero).State = EntityState.Modified;
                //  			db.SaveChanges();

                ViewBag.bodccs_cod = db.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();

                var buscarBodegas = from bodegas in db.planfinancierobodega
                                    where bodegas.idplanfinanciero == cabeceraPlan.plan_id
                                    select new { bodegas.idbodega };
                string bodegasString = "";
                bool primera = true;
                foreach (var item in buscarBodegas)
                {
                    if (primera)
                    {
                        bodegasString += item.idbodega;
                        primera = !primera;
                    }
                    else
                    {
                        bodegasString += "," + item.idbodega;
                    }
                }

                ViewBag.bodegasSeleccionadas = bodegasString;


                TempData["mensaje"] = "Registro editado Correctamente";
            }
            else
            {
                TempData["mensaje_error"] = "Error al editar el registro, por favor valide";
            }
            //  ConsultaDatosCreacion(icb_plan_financiero);

            var fianancieras = (from fin in db.icb_unidad_financiera //modelopdf.vm_list_icb_terceros
                                select new
                                {
                                    fin.financiera_id,
                                    nombre = fin.financiera_nombre
                                }).ToList();
            ViewBag.idfinanciera =
                new SelectList(fianancieras, "financiera_id", "nombre", modeloPlanFinanciero.idfinanciera);
            BuscarFavoritos(menu);
            return View(modeloPlanFinanciero);
            //ConsultaDatosCreacion(modeloPlanFinanciero);
            // BuscarFavoritos(menu);
            // return View(modeloPlanFinanciero);
        }

        // public void ConsultaDatosCreacion(icb_plan_financiero plan)
        //   public void ConsultaDatosCreacion(ModeloPlanFinanciero plan)
        public void ConsultaDatosCreacion(int id)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag

            icb_plan_financiero QueryPlan = db.icb_plan_financiero.FirstOrDefault(x => x.plan_id == id);

            users creator = db.users.Find(QueryPlan.plan_usuela);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = db.users.Find(QueryPlan.plan_usuela);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }
            //  ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();

            ViewBag.fechaCrea = QueryPlan.plan_fecela;
            ViewBag.user_fec_act = QueryPlan.plan_fecha_actualizacion.ToString();
            //var creator = db.users.Find(plan.plan_usuela);
            //if (creator != null)
            //{
            //    ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            //}

            //var modificator = db.users.Find(plan.plan_usuela);
            //if (modificator != null)
            //{
            //    ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            //    ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            //}
        }

        // GET: plan_financiero
        public ActionResult Browser(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }


        public JsonResult BuscarDatos()
        {
            var data = db.icb_plan_financiero.ToList().Select(x => new
            {
                x.plan_nombre,
                x.plan_descripcion,
                comision = x.plan_comision ? "Si" : "No",
                x.plan_porcentaje_comision,
                estado = x.plan_estado ? "Activo" : "Inactivo",
                x.plan_id,
                tasa_interes = x.tasa_interes != null ? x.tasa_interes.Value : 0
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarBodegasPlan(int plan_id)
        {
            var financiera = (from plan in db.icb_plan_financiero
                              join fina in db.icb_unidad_financiera
                                  on plan.idfinanciera equals fina.financiera_id
                              where plan.plan_id == plan_id
                              select new
                              {
                                  fina.financiera_nombre
                              }).FirstOrDefault();

            //var buscarMuchos = (from planbod in db.planfinancierobodega
            //                    join fin in db.icb_unidad_financiera
            //                    on planbod.idplanfinanciero equals fin.financiera_id
            //                    join bod in db.bodega_concesionario
            //                    on planbod.idbodega equals bod.id
            //                    where planbod.idplanfinanciero == plan_id
            //                    select new
            //                    {
            //                        planbod.id,
            //                        planbod.idplanfinanciero,
            //                        fin.financiera_nombre,
            //                        planbod.idbodega,
            //                        bod.bodccs_cod,
            //                        bod.bodccs_nombre,
            //                        planbod.porcentaje,
            //                        planbod.fechadesde,
            //                        planbod.fechahasta,
            //                        planbod.estado,
            //                        planbod.razoninactividad
            //                    }).ToList();
            var buscarMuchos = (from planbod in db.planfinancierobodega
                                join plfin in db.icb_plan_financiero
                                    on planbod.idplanfinanciero equals plfin.plan_id
                                join bod in db.bodega_concesionario
                                    on planbod.idbodega equals bod.id
                                join fin in db.icb_unidad_financiera
                                    on plfin.idfinanciera equals fin.financiera_id
                                where planbod.idplanfinanciero == plan_id
                                select new
                                {
                                    planbod.id,
                                    planbod.idplanfinanciero,
                                    fin.financiera_nombre,
                                    planbod.idbodega,
                                    bod.bodccs_cod,
                                    bod.bodccs_nombre,
                                    planbod.porcentaje,
                                    planbod.fechadesde,
                                    planbod.fechahasta,
                                    planbod.estado,
                                    planbod.razoninactividad
                                }).ToList();
            List<Listaplanfinancierobodega> data = buscarMuchos.Select(c => new Listaplanfinancierobodega
            {
                id = c.id,
                idplanfinanciero = c.idplanfinanciero,
                idbodega = c.idbodega,
                nomBodega = "(" + c.bodccs_cod + ") " + c.bodccs_nombre,
                nomFinaciera = financiera.financiera_nombre,
                porcentaje = c.porcentaje,
                fechadesde = c.fechadesde != null ? c.fechadesde.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                fechahasta = c.fechahasta != null ? c.fechahasta.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                estado = c.estado,
                Desestado = c.estado ? "Activo" : "Inactivo",
                razoninactividad = c.razoninactividad
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BorraBodega_id(int id)
        {
            planfinancierobodega buscarBodega = db.planfinancierobodega.FirstOrDefault(m => m.id == id);
            if (buscarBodega != null)
            {
                db.Entry(buscarBodega).State = EntityState.Deleted;
                int eliminar = db.SaveChanges();
                if (eliminar > 0)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ActualizarPlan(int id, string plan_descripcion, string plan_nombre, bool plan_comision,
            bool plan_estado, string plan_razon_inactivo, HttpPostedFileBase imagen)
        {
            int result = 0;
            icb_plan_financiero QueryPlan = db.icb_plan_financiero.FirstOrDefault(x => x.plan_id == id);

            //var motorVh = context.icb_vehiculo.FirstOrDefault(x => x.nummot_vh == motor);
            //var serieVh = context.icb_vehiculo.FirstOrDefault(x => x.vin == serie);
            //var placaVh = context.icb_vehiculo.FirstOrDefault(x => x.plac_vh == placa);

            if (!string.IsNullOrWhiteSpace(plan_nombre))
            {
                QueryPlan.plan_nombre = plan_nombre;
            }

            if (!string.IsNullOrWhiteSpace(plan_descripcion))
            {
                QueryPlan.plan_descripcion = plan_descripcion;
            }

            if (!string.IsNullOrWhiteSpace(plan_razon_inactivo))
            {
                QueryPlan.plan_razon_inactivo = plan_razon_inactivo;
            }

            if (imagen != null)
            {
                QueryPlan.plan_imagen = id + "_" + imagen.FileName;
                string path = Server.MapPath("~/Content/img/" + id + "_" + imagen.FileName);
                imagen.SaveAs(path);
            }


            QueryPlan.plan_estado = plan_estado;
            QueryPlan.plan_comision = plan_comision;
            QueryPlan.plan_usuario_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
            QueryPlan.plan_fecha_actualizacion = DateTime.Now;

            db.Entry(QueryPlan).State = EntityState.Modified;
            result = db.SaveChanges();

            ConsultaDatosCreacion(id);
            //var creator = db.users.Find(QueryPlan.plan_usuela);
            //if (creator != null)
            //{
            //    ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            //}

            //var modificator = db.users.Find(QueryPlan.plan_usuela);
            //if (modificator != null)
            //{
            //    ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            //    ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            //}

            //ViewBag.fechaCrea = QueryPlan.plan_fecela;


            return Json(result, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }


        public void BuscarFavoritos(int? menu)
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

            var buscarFavoritosSeleccionados = (from favoritos in db.favoritos
                                                join menu2 in db.Menus
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