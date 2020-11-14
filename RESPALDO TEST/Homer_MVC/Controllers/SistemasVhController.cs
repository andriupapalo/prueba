using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class SistemasVhController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: SistemasVh
        public List<listaGeneral> operaciones()
        {
            List<listaGeneral> operaciones = db.ttempario.Select(x => new listaGeneral
            {
                codigo = x.codigo,
                descripcion = x.operacion
            }).ToList();
            return operaciones;
        }

        public List<string> tempario(int id)
        {
            List<string> tempario_2 = new List<string>();
            var tempar = (from a in db.icb_tsistemas
                          join b in db.icb_tsistemas_operaciones
                              on a.tsis_id equals b.tsis_id
                          where a.tsis_id == id
                          select new { b.ttemp_codigo }).ToList();
            tempar.ForEach(x => tempario_2.Add(x.ttemp_codigo));
            return tempario_2;
        }

        public ActionResult SistemasC(int? id)
        {
            ViewBag.tab = id != null ? 1 : 2;
            SistemasVh sist = new SistemasVh
            {
                tsis_estado = true
            };
            if (id != null && id > 0)
            {
                int idn = Convert.ToInt32(id);
                icb_tsistemas tsis = db.icb_tsistemas.Where(x => x.tsis_id == idn).FirstOrDefault();
                if (tsis != null)
                {
                    sist.id = Convert.ToInt32(id);
                    sist.tsis_sistema = tsis.tsis_sistema;
                    sist.tsis_estado = Convert.ToBoolean(tsis.tsis_estado);
                    sist.tsis_razoninactivo = tsis.tsis_razoninactivo;
                    ViewBag.lista = tempario(Convert.ToInt32(id));
                }
            }

            sist.ttemmpario_list = operaciones();
            return View(sist);
        }

        public List<string> sistemas_operaciones(string temp, int idn)
        {
            string[] slc = temp.Split(',');
            string Query = "Delete from icb_tsistemas_operaciones where tsis_id =" + idn;
            db.Database.ExecuteSqlCommand(Query);
            List<string> cod = new List<string>();
            foreach (string s in slc)
            {
                icb_tsistemas_operaciones tsis_op = new icb_tsistemas_operaciones
                {
                    ttemp_codigo = s,
                    tsis_id = idn
                };
                db.icb_tsistemas_operaciones.Add(tsis_op);
                db.SaveChanges();
                cod.Add(s);
            }

            return cod;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SistemasC(SistemasVh sist)
        {
            ModelState.Remove("ttemmpario_list");
            ViewBag.tab = 1;
            int sist_id = 0;
            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (sist.id > 0)
                        {
                            icb_tsistemas tsis = db.icb_tsistemas.FirstOrDefault(x => x.tsis_id == sist.id);
                            if (tsis != null)
                            {
                                icb_tsistemas tsis_nomb = db.icb_tsistemas.FirstOrDefault(x =>
                                    x.tsis_id != sist.id && x.tsis_sistema == sist.tsis_sistema);
                                if (tsis_nomb == null)
                                {
                                    tsis.tsis_sistema = sist.tsis_sistema;
                                    tsis.tsis_fecmod = DateTime.Now;
                                    tsis.tsis_usumod = Convert.ToInt32(Session["user_usuarioid"]);
                                    tsis.tsis_razoninactivo = sist.tsis_razoninactivo;
                                    tsis.tsis_estado = sist.tsis_estado;
                                    db.Entry(tsis).State = EntityState.Modified;
                                    db.SaveChanges();
                                    ViewBag.lista = sistemas_operaciones(Request["tempario_id"], sist.id);
                                    TempData["mensaje"] = "Se a guardado la informacion";
                                }
                                else
                                {
                                    TempData["mensaje_error"] = "Ya existe un sistemas con ese nombre.";
                                }
                            }
                            else
                            {
                                TempData["mensaje_error"] = "No se encontro.";
                            }
                        }
                        else
                        {
                            icb_tsistemas tsis_nomb = db.icb_tsistemas.FirstOrDefault(x => x.tsis_sistema == sist.tsis_sistema);
                            if (tsis_nomb == null)
                            {
                                icb_tsistemas tsis = new icb_tsistemas
                                {
                                    tsis_sistema = sist.tsis_sistema,
                                    tsis_fecela = DateTime.Now,
                                    tsis_usuela = Convert.ToInt32(Session["user_usuarioid"]),
                                    tsis_estado = sist.tsis_estado,
                                    tsis_razoninactivo = sist.tsis_razoninactivo
                                };
                                db.icb_tsistemas.Add(tsis);
                                db.SaveChanges();
                                sist_id = tsis.tsis_id;
                                ViewBag.lista = sistemas_operaciones(Request["tempario_id"], sist_id);
                                TempData["mensaje"] = "Se a guardado la informacion";
                            }
                            else
                            {
                                TempData["mensaje_error"] = "Ya existe un sistemas con ese nombre.";
                            }
                        }

                        dbTran.Commit();
                        if (sist_id > 0)
                        {
                            return RedirectToAction("SistemasC", new { id = sist_id });
                        }
                    }
                    catch (Exception ex)
                    {
                        Exception exp = ex;
                        TempData["mensaje_error"] = "No se encontro.";
                        dbTran.Rollback();
                    }
                }
            }
            else
            {
                TempData["mensaje_error"] = "Valide la informacion ingresada.";
            }

            sist.ttemmpario_list = operaciones();
            return View(sist);
        }

        public JsonResult tablaPaginada()
        {
            var tsis = db.icb_tsistemas.Select(x => new
            {
                x.tsis_id,
                x.tsis_sistema
            }).ToList();
            return Json(tsis, JsonRequestBehavior.AllowGet);
        }
    }
}