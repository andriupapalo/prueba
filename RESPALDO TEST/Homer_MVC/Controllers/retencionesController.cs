using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class retencionesController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        private readonly CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

        // GET: retenciones
        public ActionResult Create(Modelo_retenciones modeloretenciones)
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

            ViewBag.txtBodegas = lista_bodegas;


            var perfiles = (from b in db.tpregimen_tercero
                            select new
                            {
                                b.tpregimen_id,
                                nombre = "(" + b.tpregimen_codigo + ") " + b.tpregimen_nombre
                            }).ToList();

            List<SelectListItem> lista_perfiles = new List<SelectListItem>();
            foreach (var item in perfiles)
            {
                lista_perfiles.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.tpregimen_id.ToString()
                });
            }

            ViewBag.tpregimen_id = lista_perfiles;

            var cuentaimpuesto = (from ctaImp in db.cuenta_puc
                                  where ctaImp.esafectable && ctaImp.cuentaimpuestos
                                  select new
                                  {
                                      ctaImp.cntpuc_id,
                                      nombre = "(" + ctaImp.cntpuc_numero + ") " + ctaImp.cntpuc_descp
                                  }).ToList();
            ViewBag.ctaimpuesto = new SelectList(cuentaimpuesto, "cntpuc_id", "nombre");
            var cuentaimpuestoret = (from ctaRetImp in db.cuenta_puc
                                     where ctaRetImp.esafectable && ctaRetImp.cuentaimpuestos
                                     select new
                                     {
                                         ctaRetImp.cntpuc_id,
                                         nombre = "(" + ctaRetImp.cntpuc_numero + ") " + ctaRetImp.cntpuc_descp
                                     }).ToList();
            ViewBag.ctareteiva = new SelectList(cuentaimpuestoret, "cntpuc_id", "nombre");

            var cuentaretenciones = (from ctaImp in db.cuenta_puc
                                     where ctaImp.esafectable && ctaImp.cuentaimpuestos
                                     select new
                                     {
                                         ctaImp.cntpuc_id,
                                         nombre = "(" + ctaImp.cntpuc_numero + ") " + ctaImp.cntpuc_descp
                                     }).ToList();
            ViewBag.ctaretencion = new SelectList(cuentaretenciones, "cntpuc_id", "nombre");
            var cuentaica = (from ctaImp in db.cuenta_puc
                             where ctaImp.esafectable && ctaImp.cuentaimpuestos
                             select new
                             {
                                 ctaImp.cntpuc_id,
                                 nombre = "(" + ctaImp.cntpuc_numero + ") " + ctaImp.cntpuc_descp
                             }).ToList();
            ViewBag.ctaica = new SelectList(cuentaica, "cntpuc_id", "nombre");
            var cuentaporpagar = (from ctaImp in db.cuenta_puc
                                  where ctaImp.esafectable && ctaImp.cuentaproveedor
                                  select new
                                  {
                                      ctaImp.cntpuc_id,
                                      nombre = "(" + ctaImp.cntpuc_numero + ") " + ctaImp.cntpuc_descp
                                  }).ToList();
            ViewBag.ctaxpagar = new SelectList(cuentaporpagar, "cntpuc_id", "nombre");
            var conceptos = (from conce in db.tparamretenciones
                             where conce.estado
                             select new
                             {
                                 conce.id,
                                 nombre = conce.concepto
                             }).ToList();
            ViewBag.concepto = new SelectList(conceptos, "id", "nombre");

            //	BuscarFavoritos(menu);
            return View(modeloretenciones);


            //return View(new banco() { estado = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Modelo_retenciones modeloretenciones, int? menu)
        {
            if (ModelState.IsValid)
            {
                tablaretenciones buscarDato = db.tablaretenciones.FirstOrDefault(x => x.id == modeloretenciones.id);
                if (buscarDato == null)
                {
                    tablaretenciones modelore = new tablaretenciones
                    {
                        concepto = modeloretenciones.concepto,
                        ctaimpuesto = modeloretenciones.ctaimpuesto,
                        ctareteiva = modeloretenciones.ctareteiva,
                        ctaretencion = modeloretenciones.ctaretencion,
                        ctaica = modeloretenciones.ctaica,
                        ctaxpagar = modeloretenciones.ctaxpagar,
                        fec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        estado = modeloretenciones.estado,
                        razon_inactivo = modeloretenciones.razon_inactivo
                    };
                    db.tablaretenciones.Add(modelore);
                    db.SaveChanges();

                    //var conceptos = (from conce in db.tparamretenciones
                    //				 where conce.estado == true
                    //				 select new
                    //				 {
                    //					 conce.id,
                    //					 nombre = conce.concepto
                    //				 }).ToList();
                    //ViewBag.concepto = new SelectList(conceptos, "id", "nombre");


                    string[] Vectorperfiles = Request["txtPerfiles"].Split(',');

                    if (Vectorperfiles.Count() > 0)
                    {
                        foreach (string j in Vectorperfiles)
                        {
                            if (!string.IsNullOrEmpty(j))
                            {
                                //var bod = Convert.ToInt32(j);
                                //var bodegaviene = Convert.ToInt32(bod);
                                //var planviene = icb_plan_financiero.plan_id;
                                int perf01 = Convert.ToInt32(j);
                                int perf01viene = Convert.ToInt32(perf01);
                                int reteviene = modelore.id;
                                retencionperfil existe2 = db.retencionperfil.FirstOrDefault(x =>
                                    x.idretencion == modeloretenciones.id && x.idperfiltributario == perf01viene);
                                if (existe2 == null)
                                {
                                    db.retencionperfil.Add(new retencionperfil
                                    {
                                        idperfiltributario = perf01viene,
                                        idretencion = reteviene
                                    });
                                    db.SaveChanges();
                                }
                            }
                        }
                    }

                    string[] VectorBodegas = Request["txtBodegas"].Split(',');

                    if (VectorBodegas.Count() > 0)
                    {
                        foreach (string j in VectorBodegas)
                        {
                            if (!string.IsNullOrEmpty(j))
                            {
                                //var bod = Convert.ToInt32(j);
                                //var bodegaviene = Convert.ToInt32(bod);
                                //var planviene = icb_plan_financiero.plan_id;
                                int bodf01 = Convert.ToInt32(j);
                                int bod01viene = Convert.ToInt32(bodf01);
                                int reteviene = modelore.id;
                                retencionesbodega existe3 = db.retencionesbodega.FirstOrDefault(x =>
                                    x.idretencion == modeloretenciones.id && x.idbodega == bod01viene);
                                if (existe3 == null)
                                {
                                    db.retencionesbodega.Add(new retencionesbodega
                                    {
                                        idretencion = reteviene,
                                        idbodega = bod01viene,
                                        ctaiva = modeloretenciones.ctaimpuesto,
                                        ctareteiva = modeloretenciones.ctareteiva,
                                        ctaretencion = modeloretenciones.ctaretencion,
                                        ctareteica = modeloretenciones.ctaica,
                                        cuentaxpagar = modeloretenciones.ctaxpagar,
                                        fec_creacion = DateTime.Now,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                        estado = true
                                    });
                                    db.SaveChanges();
                                }
                            }
                        }
                    }

                    TempData["mensaje"] = "La creación del registro fue exitoso";
                    //return RedirectToAction("Edit", new { id = modeloretenciones.id, menu });
                    return RedirectToAction("Create");
                    //return View();
                }
                else
                {
                    string[] Vectorperfiles = Request["txtPerfiles"].Split(',');

                    if (Vectorperfiles.Count() > 0)
                    {
                        foreach (string j in Vectorperfiles)
                        {
                            if (!string.IsNullOrEmpty(j))
                            {
                                //var bod = Convert.ToInt32(j);
                                //var bodegaviene = Convert.ToInt32(bod);
                                //var planviene = icb_plan_financiero.plan_id;
                                int perf01 = Convert.ToInt32(j);
                                int perf01viene = Convert.ToInt32(perf01);
                                int reteviene = modeloretenciones.id;
                                retencionperfil existe2 = db.retencionperfil.FirstOrDefault(x =>
                                    x.idretencion == modeloretenciones.id && x.idperfiltributario == perf01viene);
                                if (existe2 == null)
                                {
                                    db.retencionperfil.Add(new retencionperfil
                                    {
                                        idperfiltributario = perf01viene,
                                        idretencion = reteviene
                                    });
                                    db.SaveChanges();
                                }
                            }
                        }
                    }

                    string[] VectorBodegas = Request["txtBodegas"].Split(',');

                    if (VectorBodegas.Count() > 0)
                    {
                        foreach (string j in VectorBodegas)
                        {
                            if (!string.IsNullOrEmpty(j))
                            {
                                //var bod = Convert.ToInt32(j);
                                //var bodegaviene = Convert.ToInt32(bod);
                                //var planviene = icb_plan_financiero.plan_id;
                                int bodf01 = Convert.ToInt32(j);
                                int bod01viene = Convert.ToInt32(bodf01);
                                int reteviene = modeloretenciones.id;
                                retencionesbodega existe3 = db.retencionesbodega.FirstOrDefault(x =>
                                    x.idretencion == modeloretenciones.id && x.idbodega == bod01viene);
                                if (existe3 == null)
                                {
                                    db.retencionesbodega.Add(new retencionesbodega
                                    {
                                        idretencion = bod01viene,
                                        idbodega = bod01viene,
                                        ctaiva = modeloretenciones.ctaimpuesto,
                                        ctareteiva = modeloretenciones.ctareteiva,
                                        ctaretencion = modeloretenciones.ctaretencion,
                                        ctareteica = modeloretenciones.ctaica,
                                        cuentaxpagar = modeloretenciones.ctaxpagar,
                                        fec_creacion = DateTime.Now,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                        estado = true
                                    });
                                    db.SaveChanges();
                                }
                            }
                        }
                    }

                    TempData["mensaje_error"] = "El registro ingresado ya existe, por favor valide";
                }
            }

            var conceptos = (from conce in db.tparamretenciones
                             where conce.estado
                             select new
                             {
                                 conce.id,
                                 nombre = conce.concepto
                             }).ToList();
            ViewBag.concepto = new SelectList(conceptos, "id", "nombre");

            var cuentaimpuesto = (from ctaImp in db.cuenta_puc
                                  where ctaImp.esafectable && ctaImp.cuentaimpuestos
                                  select new
                                  {
                                      ctaImp.cntpuc_id,
                                      nombre = "(" + ctaImp.cntpuc_numero + ") " + ctaImp.cntpuc_descp
                                  }).ToList();
            ViewBag.ctaimpuesto = new SelectList(cuentaimpuesto, "cntpuc_id", "nombre");
            var cuentaimpuestoret = (from ctaRetImp in db.cuenta_puc
                                     where ctaRetImp.esafectable && ctaRetImp.cuentaimpuestos
                                     select new
                                     {
                                         ctaRetImp.cntpuc_id,
                                         nombre = "(" + ctaRetImp.cntpuc_numero + ") " + ctaRetImp.cntpuc_descp
                                     }).ToList();
            ViewBag.ctareteiva = new SelectList(cuentaimpuestoret, "cntpuc_id", "nombre");
            var cuentaretenciones = (from ctaImp in db.cuenta_puc
                                     where ctaImp.esafectable && ctaImp.cuentaimpuestos
                                     select new
                                     {
                                         ctaImp.cntpuc_id,
                                         nombre = "(" + ctaImp.cntpuc_numero + ") " + ctaImp.cntpuc_descp
                                     }).ToList();
            ViewBag.ctaretencion = new SelectList(cuentaretenciones, "cntpuc_id", "nombre");
            var cuentaica = (from ctaImp in db.cuenta_puc
                             where ctaImp.esafectable && ctaImp.cuentaimpuestos
                             select new
                             {
                                 ctaImp.cntpuc_id,
                                 nombre = "(" + ctaImp.cntpuc_numero + ") " + ctaImp.cntpuc_descp
                             }).ToList();
            ViewBag.ctaica = new SelectList(cuentaica, "cntpuc_id", "nombre");
            var cuentaporpagar = (from ctaImp in db.cuenta_puc
                                  where ctaImp.esafectable && ctaImp.cuentaproveedor
                                  select new
                                  {
                                      ctaImp.cntpuc_id,
                                      nombre = "(" + ctaImp.cntpuc_numero + ") " + ctaImp.cntpuc_descp
                                  }).ToList();
            ViewBag.ctaxpagar = new SelectList(cuentaporpagar, "cntpuc_id", "nombre");

            var perfiles = (from b in db.tpregimen_tercero
                            select new
                            {
                                b.tpregimen_id,
                                nombre = "(" + b.tpregimen_codigo + ") " + b.tpregimen_nombre
                            }).ToList();

            List<SelectListItem> lista_perfiles = new List<SelectListItem>();
            foreach (var item in perfiles)
            {
                lista_perfiles.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.tpregimen_id.ToString()
                });
            }

            ViewBag.tpregimen_id = lista_perfiles;

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

            ViewBag.txtBodegas = lista_bodegas;
            //BuscarFavoritos(menu); 
            return View();
        }

        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tablaretenciones reten = db.tablaretenciones.Find(id);
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
            //var retex = db.tablaretenciones.Find(id);

            var buscarMuchos = (from retper in db.retencionperfil
                                join regim in db.tpregimen_tercero
                                    on retper.idperfiltributario equals regim.tpregimen_id
                                where retper.idperfiltributario == id
                                select new
                                {
                                    retper.idperfiltributario,
                                    nomReg = "(" + regim.tpregimen_codigo + ")" + regim.tpregimen_nombre
                                }).ToList();
            List<ListaPerfiles001> perfilesall = buscarMuchos.Select(c => new ListaPerfiles001
            {
                id = c.idperfiltributario,
                nomPerfil = c.nomReg
            }).ToList();

            var buscarMuchos2 = (from retbod in db.retencionesbodega
                                 join bodeg in db.bodega_concesionario
                                     on retbod.idbodega equals bodeg.id
                                 where retbod.idretencion == id
                                 select new
                                 {
                                     retbod.idbodega,
                                     nomBog = "(" + bodeg.bodccs_cod + ")" + bodeg.bodccs_nombre
                                 }).ToList();
            List<ListaBodegas001> bodegasall = buscarMuchos2.Select(c => new ListaBodegas001
            {
                id = c.idbodega,
                nomBodega = c.nomBog
            }).ToList();

            tablaretenciones retencionx = db.tablaretenciones.Find(id);

            Modelo_retenciones mret = new Modelo_retenciones
            {
                id = retencionx.id,
                concepto = retencionx.concepto,
                //mret.baseuvt = retencionx.baseuvt;
                //mret.basepesos = retencionx.basepesos.ToString("0,0", elGR);
                //mret.tarifas = retencionx.tarifas.ToString("N2"); ///.ToString("0,0", elGR);
                ctaimpuesto = retencionx.ctaimpuesto ?? 0,
                ctareteiva = retencionx.ctareteiva ?? 0,
                ctaretencion = retencionx.ctaretencion ?? 0,
                ctaica = retencionx.ctaica ?? 0,
                ctaxpagar = retencionx.ctaxpagar ?? 0,
                fec_creacion = retencionx.fec_creacion,
                userid_creacion = retencionx.userid_creacion,
                //mret.fec_actualizacion = retencionx.fec_actualizacion;
                //mret.user_idactualizacion = retencionx.user_idactualizacion;
                estado = retencionx.estado,
                razon_inactivo = retencionx.razon_inactivo,
                ListaPerfiles001 = perfilesall,
                ListaBodegas001 = bodegasall
            };

            //	ViewBag.refreg = db.retencionperfil.OrderBy(x => x.idperfiltributario).ToList();
            ViewBag.refreg = db.tpregimen_tercero.OrderBy(x => x.tpregimen_id).ToList();

            var buscarPerfiles = from perfiles in db.retencionperfil
                                 where perfiles.idretencion == retencionx.id
                                 select new { perfiles.idperfiltributario };
            string perfilesString = "";
            bool primera = true;
            foreach (var item in buscarPerfiles)
            {
                if (primera)
                {
                    perfilesString += item.idperfiltributario;
                    primera = !primera;
                }
                else
                {
                    perfilesString += "," + item.idperfiltributario;
                }
            }

            ViewBag.perfilesSeleccionadas = perfilesString;

            ViewBag.bodreg = db.bodega_concesionario.OrderBy(x => x.id).ToList();
            var buscarBodegas = from bodegas in db.retencionesbodega
                                where bodegas.idretencion == retencionx.id
                                select new { bodegas.idbodega };
            string bodegasString = "";
            bool primera2 = true;
            foreach (var item in buscarBodegas)
            {
                if (primera2)
                {
                    bodegasString += item.idbodega;
                    primera2 = !primera2;
                }
                else
                {
                    bodegasString += "," + item.idbodega;
                }
            }

            ViewBag.bodegasSeleccionadas = bodegasString;


            var cuentaimpuesto = (from ctaImp in db.cuenta_puc
                                  where ctaImp.esafectable && ctaImp.cuentaimpuestos
                                  select new
                                  {
                                      ctaImp.cntpuc_id,
                                      nombre = "(" + ctaImp.cntpuc_numero + ") " + ctaImp.cntpuc_descp
                                  }).ToList();
            ViewBag.ctaimpuesto = new SelectList(cuentaimpuesto, "cntpuc_id", "nombre", mret.ctaimpuesto);
            var cuentaimpuestoret = (from ctaRetImp in db.cuenta_puc
                                     where ctaRetImp.esafectable && ctaRetImp.cuentaimpuestos
                                     select new
                                     {
                                         ctaRetImp.cntpuc_id,
                                         nombre = "(" + ctaRetImp.cntpuc_numero + ") " + ctaRetImp.cntpuc_descp
                                     }).ToList();
            ViewBag.ctareteiva = new SelectList(cuentaimpuestoret, "cntpuc_id", "nombre", mret.ctareteiva);
            var cuentaretenciones = (from ctaImp in db.cuenta_puc
                                     where ctaImp.esafectable && ctaImp.cuentaimpuestos
                                     select new
                                     {
                                         ctaImp.cntpuc_id,
                                         nombre = "(" + ctaImp.cntpuc_numero + ") " + ctaImp.cntpuc_descp
                                     }).ToList();
            ViewBag.ctaretencion = new SelectList(cuentaretenciones, "cntpuc_id", "nombre", mret.ctaretencion);
            var cuentaica = (from ctaImp in db.cuenta_puc
                             where ctaImp.esafectable && ctaImp.cuentaimpuestos
                             select new
                             {
                                 ctaImp.cntpuc_id,
                                 nombre = "(" + ctaImp.cntpuc_numero + ") " + ctaImp.cntpuc_descp
                             }).ToList();
            ViewBag.ctaica = new SelectList(cuentaica, "cntpuc_id", "nombre", mret.ctaica);
            var cuentaporpagar = (from ctaImp in db.cuenta_puc
                                  where ctaImp.esafectable && ctaImp.cuentaproveedor
                                  select new
                                  {
                                      ctaImp.cntpuc_id,
                                      nombre = "(" + ctaImp.cntpuc_numero + ") " + ctaImp.cntpuc_descp
                                  }).ToList();
            ViewBag.ctaxpagar = new SelectList(cuentaporpagar, "cntpuc_id", "nombre", mret.ctaxpagar);

            var conceptos = (from conce in db.tparamretenciones
                             where conce.estado
                             select new
                             {
                                 conce.id,
                                 nombre = conce.concepto
                             }).ToList();
            ViewBag.concepto = new SelectList(conceptos, "id", "nombre", mret.concepto);


            //BuscarFavoritos(menu);

            return View(mret);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Modelo_retenciones modeloretenciones, int? menu)
        {
            string perfilesSeleccionadas = Request["refreg"];

            string bodegasSeleccionadas = Request["bodreg"];

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(perfilesSeleccionadas))
                {
                    TempData["mensaje_error"] = "Debe asignar minimo una regimen !";

                    //ViewBag.refreg = db.retencionperfil.OrderBy(x => x.idretencion).ToList();
                    ViewBag.refreg = db.tpregimen_tercero.OrderBy(x => x.tpregimen_id).ToList();

                    ViewBag.perfilesSeleccionadas = perfilesSeleccionadas;

                    //					BuscarFavoritos(menu);
                    //return View(icb_plan_financiero);
                    //return View(modeloretenciones);
                }

                if (!string.IsNullOrEmpty(perfilesSeleccionadas))
                {
                    retencionperfil retper = new retencionperfil();

                    db.retencionperfil.RemoveRange(db.retencionperfil.Where(x =>
                        x.idretencion == modeloretenciones.id));
                    db.SaveChanges();

                    string[] perfilesId = perfilesSeleccionadas.Split(',');
                    foreach (string substring in perfilesId)
                    {
                        int xperf = Convert.ToInt32(substring);
                        int xretencion = Convert.ToInt32(modeloretenciones.id);

                        retencionperfil existeRegistro = db.retencionperfil.FirstOrDefault(x =>
                            x.idperfiltributario == xperf && x.idretencion == xretencion);

                        if (existeRegistro != null)
                        {
                            existeRegistro.idretencion = Convert.ToInt32(modeloretenciones.id);
                            existeRegistro.idperfiltributario = Convert.ToInt32(xperf);
                            db.Entry(existeRegistro).State = EntityState.Modified;
                            //db.Entry(existeRegistro).State = EntityState.Deleted;
                            db.SaveChanges();
                        }
                        else
                        {
                            retper.idretencion = Convert.ToInt32(xretencion);
                            retper.idperfiltributario = Convert.ToInt32(xperf);
                            db.retencionperfil.Add(retper);
                            db.SaveChanges();
                        }
                    }
                }


                //************
                if (string.IsNullOrEmpty(bodegasSeleccionadas))
                {
                    TempData["mensaje_error"] = "Debe asignar minimo una bodega !";
                    ViewBag.bodreg = db.bodega_concesionario.OrderBy(x => x.id).ToList();
                    ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                }

                if (!string.IsNullOrEmpty(bodegasSeleccionadas))
                {
                    retencionesbodega retbod = new retencionesbodega();

                    db.retencionesbodega.RemoveRange(db.retencionesbodega.Where(x =>
                        x.idretencion == modeloretenciones.id));
                    db.SaveChanges();

                    string[] bodegasId = bodegasSeleccionadas.Split(',');
                    foreach (string substring in bodegasId)
                    {
                        int xbod = Convert.ToInt32(substring);
                        int xretencion = Convert.ToInt32(modeloretenciones.id);

                        retencionesbodega existeRegistro =
                            db.retencionesbodega.FirstOrDefault(x => x.idbodega == xbod && x.idretencion == xretencion);

                        if (existeRegistro != null)
                        {
                            existeRegistro.idretencion = Convert.ToInt32(modeloretenciones.id);
                            existeRegistro.idbodega = Convert.ToInt32(xbod);
                            existeRegistro.ctaiva = modeloretenciones.ctaimpuesto;
                            existeRegistro.ctareteiva = modeloretenciones.ctareteiva;
                            existeRegistro.ctareteica = modeloretenciones.ctaica;
                            existeRegistro.ctaretencion = modeloretenciones.ctaretencion;
                            existeRegistro.cuentaxpagar = modeloretenciones.ctaxpagar;
                            existeRegistro.fec_creacion = DateTime.Now;
                            existeRegistro.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                            existeRegistro.estado = true;
                            db.Entry(existeRegistro).State = EntityState.Modified;
                            //db.Entry(existeRegistro).State = EntityState.Deleted;
                            db.SaveChanges();
                        }
                        else
                        {
                            retbod.idretencion = Convert.ToInt32(xretencion);
                            retbod.idbodega = Convert.ToInt32(xbod);
                            retbod.ctaiva = modeloretenciones.ctaimpuesto;
                            retbod.ctareteiva = modeloretenciones.ctareteiva;
                            retbod.ctareteica = modeloretenciones.ctaica;
                            retbod.ctaretencion = modeloretenciones.ctaretencion;
                            retbod.cuentaxpagar = modeloretenciones.ctaxpagar;
                            retbod.fec_creacion = DateTime.Now;
                            retbod.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                            retbod.estado = true;
                            db.retencionesbodega.Add(retbod);
                            db.SaveChanges();
                        }
                    }
                }

                //************

                tablaretenciones cabeceraRete = db.tablaretenciones.FirstOrDefault(x => x.id == modeloretenciones.id);
                //string temp = modeloretenciones.tarifas.Replace('.', ',');

                //Decimal xretImp = Decimal.Parse(temp);


                if (cabeceraRete != null)
                {
                    //cabeceraRete.baseuvt = Convert.ToInt32(modeloretenciones.baseuvt);  
                    //cabeceraRete.basepesos = Convert.ToDecimal(modeloretenciones.basepesos);
                    //cabeceraRete.tarifas = Convert.ToDouble(modeloretenciones.tarifas);  // xretImp;
                    cabeceraRete.ctaimpuesto = Convert.ToInt32(modeloretenciones.ctaimpuesto);
                    cabeceraRete.ctareteiva = Convert.ToInt32(modeloretenciones.ctareteiva);
                    cabeceraRete.ctaretencion = Convert.ToInt32(modeloretenciones.ctaretencion);
                    cabeceraRete.ctaica = Convert.ToInt32(modeloretenciones.ctaica);
                    cabeceraRete.ctaxpagar = Convert.ToInt32(modeloretenciones.ctaxpagar);
                    cabeceraRete.fec_actualizacion = DateTime.Now;
                    cabeceraRete.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    cabeceraRete.estado = modeloretenciones.estado;
                    cabeceraRete.razon_inactivo = modeloretenciones.razon_inactivo;

                    db.Entry(cabeceraRete).State = EntityState.Modified;
                    db.SaveChanges();
                }


                tablaretenciones XRet = db.tablaretenciones.FirstOrDefault(x => x.id == modeloretenciones.id);

                //ViewBag.refreg = db.retencionperfil.OrderBy(x => x.idretencion).ToList();
                ViewBag.refreg = db.tpregimen_tercero.OrderBy(x => x.tpregimen_id).ToList();

                var buscarPerfiles = from perfiles in db.retencionperfil
                                     where perfiles.idretencion == XRet.id
                                     select new { perfiles.idperfiltributario };
                string perfilesString = "";
                bool primera = true;
                foreach (var item in buscarPerfiles)
                {
                    if (primera)
                    {
                        perfilesString += item.idperfiltributario;
                        primera = !primera;
                    }
                    else
                    {
                        perfilesString += "," + item.idperfiltributario;
                    }
                }

                ViewBag.perfilesSeleccionadas = perfilesString;

                ViewBag.bodreg = db.bodega_concesionario.OrderBy(x => x.id).ToList();

                var buscarBodegas = from bodret in db.retencionesbodega
                                    where bodret.idretencion == XRet.id
                                    select new { bodret.idbodega };
                string bodegasString = "";
                bool primera2 = true;
                foreach (var item in buscarBodegas)
                {
                    if (primera2)
                    {
                        bodegasString += item.idbodega;
                        primera2 = !primera2;
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
                TempData["mensaje_error"] = "error en los datos de la retencion!";


                //BuscarFavoritos(menu);
                //return View(icb_plan_financiero);
                //	return View(modeloretenciones);
            }
            //  ConsultaDatosCreacion(icb_plan_financiero);


            //	BuscarFavoritos(menu);
            var cuentaimpuesto = (from ctaImp in db.cuenta_puc
                                  where ctaImp.esafectable && ctaImp.cuentaimpuestos
                                  select new
                                  {
                                      ctaImp.cntpuc_id,
                                      nombre = "(" + ctaImp.cntpuc_numero + ") " + ctaImp.cntpuc_descp
                                  }).ToList();
            ViewBag.ctaimpuesto = new SelectList(cuentaimpuesto, "cntpuc_id", "nombre", modeloretenciones.ctaimpuesto);
            var cuentaimpuestoret = (from ctaRetImp in db.cuenta_puc
                                     where ctaRetImp.esafectable && ctaRetImp.cuentaimpuestos
                                     select new
                                     {
                                         ctaRetImp.cntpuc_id,
                                         nombre = "(" + ctaRetImp.cntpuc_numero + ") " + ctaRetImp.cntpuc_descp
                                     }).ToList();
            ViewBag.ctareteiva = new SelectList(cuentaimpuestoret, "cntpuc_id", "nombre", modeloretenciones.ctareteiva);
            var cuentaretenciones = (from ctaImp in db.cuenta_puc
                                     where ctaImp.esafectable && ctaImp.cuentaimpuestos
                                     select new
                                     {
                                         ctaImp.cntpuc_id,
                                         nombre = "(" + ctaImp.cntpuc_numero + ") " + ctaImp.cntpuc_descp
                                     }).ToList();
            ViewBag.ctaretencion =
                new SelectList(cuentaretenciones, "cntpuc_id", "nombre", modeloretenciones.ctaretencion);
            var cuentaica = (from ctaImp in db.cuenta_puc
                             where ctaImp.esafectable && ctaImp.cuentaimpuestos
                             select new
                             {
                                 ctaImp.cntpuc_id,
                                 nombre = "(" + ctaImp.cntpuc_numero + ") " + ctaImp.cntpuc_descp
                             }).ToList();
            ViewBag.ctaica = new SelectList(cuentaica, "cntpuc_id", "nombre", modeloretenciones.ctaica);
            var cuentaporpagar = (from ctaImp in db.cuenta_puc
                                  where ctaImp.esafectable && ctaImp.cuentaproveedor
                                  select new
                                  {
                                      ctaImp.cntpuc_id,
                                      nombre = "(" + ctaImp.cntpuc_numero + ") " + ctaImp.cntpuc_descp
                                  }).ToList();
            ViewBag.ctaxpagar = new SelectList(cuentaporpagar, "cntpuc_id", "nombre", modeloretenciones.ctaxpagar);

            var conceptos = (from conce in db.tparamretenciones
                             where conce.estado
                             select new
                             {
                                 conce.id,
                                 nombre = conce.concepto
                             }).ToList();
            ViewBag.concepto = new SelectList(conceptos, "id", "nombre", modeloretenciones.concepto);

            return View(modeloretenciones);
        }

        public JsonResult BuscarDatos()
        {
            var data2 = (from rete in db.tablaretenciones
                         join ctaimp in db.cuenta_puc
                             on rete.ctaimpuesto equals ctaimp.cntpuc_id
                         join ctaretIva in db.cuenta_puc
                             on rete.ctareteiva equals ctaretIva.cntpuc_id
                         join ctaret in db.cuenta_puc
                             on rete.ctaretencion equals ctaret.cntpuc_id
                         join ctaica in db.cuenta_puc
                             on rete.ctaica equals ctaica.cntpuc_id
                         join ctapaga in db.cuenta_puc
                             on rete.ctaxpagar equals ctapaga.cntpuc_id
                         join para in db.tparamretenciones
                             on rete.id equals para.id
                         select new
                         {
                             rete.id,
                             rete.concepto,
                             nom = para.concepto,
                             //rete.baseuvt,
                             //rete.basepesos,
                             //rete.tarifas,
                             rete.ctaimpuesto,
                             numctaimp = ctaimp.cntpuc_numero,
                             desctaimp = ctaimp.cntpuc_descp,
                             rete.ctareteiva,
                             numctaretiva = ctaretIva.cntpuc_numero,
                             desctaretiva = ctaretIva.cntpuc_descp,
                             rete.ctaretencion,
                             numctaret = ctaret.cntpuc_numero,
                             desctaret = ctaret.cntpuc_descp,
                             rete.ctaica,
                             numctaica = ctaica.cntpuc_numero,
                             desctaica = ctaica.cntpuc_descp,
                             rete.ctaxpagar,
                             numctapxp = ctapaga.cntpuc_numero,
                             desctapxp = ctapaga.cntpuc_descp,
                             rete.estado
                         }).ToList();
            var data = data2.Select(c => new
            {
                c.id,
                c.concepto,
                c.nom,
                //baseuvt = c.baseuvt != null ? c.baseuvt.ToString("N0") : "",
                //basepesos = c.basepesos != null ? c.basepesos.ToString("N0") : "",
                //tarifas = c.tarifas != null ? c.tarifas.ToString("N2") : "",
                c.ctareteiva,
                ctretimp = "(" + c.numctaretiva + ") " + c.desctaretiva,
                c.ctaimpuesto,
                ctimp = "(" + c.numctaimp + ") " + c.desctaimp,
                c.ctaretencion,
                ctret = "(" + c.numctaret + ") " + c.desctaret,
                c.ctaica,
                cta_ica = "(" + c.numctaica + ") " + c.desctaica,
                c.ctaxpagar,
                ctapxp = "(" + c.numctapxp + ") " + c.desctapxp,
                xestado = c.estado ? "Activo" : "Inactivo"
                ////userfec_actualizacion = c.userfec_actualizacion != null ? c.userfec_actualizacion: "",
                //// userfec_actualizacion = c.userfec_actualizacion != null ? c.userfec_actualizacion.Value.ToShortDateString() + " " + c.userfec_actualizacion.Value.ToShortTimeString() : "",
                ////  userfec_actualizacion = c.userfec_actualizacion != null ? c.userfec_actualizacion.ToString(): "",  // ojo 
            }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}