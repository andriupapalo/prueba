using Homer_MVC.IcebergModel;
using Homer_MVC.ViewModels.medios;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class medios_conceptossController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: medios_conceptoss
        public ActionResult Index()
        {
            IQueryable<medios_conceptos> medios_conceptos = db.medios_conceptos.Include(m => m.medios_formato).Include(m => m.users)
                .Include(m => m.users1);
            return View(medios_conceptos.ToList());
        }

        // GET: medios_conceptoss/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            medios_conceptos medios_conceptos = db.medios_conceptos.Find(id);
            if (medios_conceptos == null)
            {
                return HttpNotFound();
            }

            return View(medios_conceptos);
        }

        // GET: medios_conceptoss/Create
        public ActionResult Create()
        {
            ViewModeloMediosMtto ViewModeloMediosMtto = new ViewModeloMediosMtto();

            var listaformato = (from formatos in db.medios_formato
                                select new
                                {
                                    formatos.id,
                                    des_formato = formatos.formato + " - " + formatos.descripcion
                                }).ToList();

            ViewBag.idformato = new SelectList(listaformato, "id", "des_formato");
            ViewBag.userid_creacion = new SelectList(db.users, "user_id", "user_nombre");
            ViewBag.user_idactualizacion = new SelectList(db.users, "user_id", "user_nombre");
            return View(ViewModeloMediosMtto);
        }

        // POST: medios_conceptoss/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ViewModeloMediosMtto ViewModeloMediosMtto, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in db.medios_conceptos
                           where a.concepto == ViewModeloMediosMtto.View_concepto.concepto
                           select a.descripcion).Count();

                if (nom == 0)
                {
                    ViewModeloMediosMtto.View_concepto.fec_creacion = DateTime.Now;
                    ViewModeloMediosMtto.View_concepto.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    db.medios_conceptos.Add(ViewModeloMediosMtto.View_concepto);
                    db.SaveChanges();
                    TempData["mensaje"] = "El registro del nuevo Concepto de Medios Magneticos fue exitoso!";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] =
                    "El registro que ingreso de Concepto de Medios Magneticos ya se encuentra, por favor valide!";
            }

            //ViewBag.userid_creacion = new SelectList(db.users, "user_id", "user_nombre", medios_formato.userid_creacion);
            //ViewBag.user_idactualizacion = new SelectList(db.users, "user_id", "user_nombre", medios_formato.user_idactualizacion);
            return View(ViewModeloMediosMtto);
        }


        //public ActionResult Create([Bind(Include = "id,idformato,concepto,descripcion,tope,id_licencia,fec_creacion,userid_creacion,fec_actualizacion,user_idactualizacion,estado,razon_inactivo")] medios_conceptos medios_conceptos, ViewModeloMediosMtto ViewModeloMediosMtto)
        //{
        //    if (ModelState.IsValid)
        //    {

        //        db.medios_conceptos.Add(medios_conceptos);
        //        db.SaveChanges();
        //        return RedirectToAction("Create");
        //    }

        //    ViewBag.idformato = new SelectList(db.medios_formato, "id", "descripcion", medios_conceptos.idformato);
        //    ViewBag.userid_creacion = new SelectList(db.users, "user_id", "user_nombre", medios_conceptos.userid_creacion);
        //    ViewBag.user_idactualizacion = new SelectList(db.users, "user_id", "user_nombre", medios_conceptos.user_idactualizacion);
        //    return View(medios_conceptos);
        //}

        // GET: medios_conceptoss/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            medios_conceptos medios_conceptos = db.medios_conceptos.Find(id);
            if (medios_conceptos == null)
            {
                return HttpNotFound();
            }

            var listaformato = (from formatos in db.medios_formato
                                select new
                                {
                                    formatos.id,
                                    des_formato = formatos.formato + " - " + formatos.descripcion
                                }).ToList();

            ViewBag.idformato = new SelectList(listaformato, "id", "des_formato");
            //var listaConcepto = (from con in db.medios_conceptos
            //                     select new
            //                     {
            //                         con.id,
            //                         des_concepto = con.concepto + " - " + con.descripcion,
            //                     }).ToList();
            //ViewBag.idformato = new SelectList(listaConcepto, "id", "des_concepto", medios_conceptos.idformato);
            ViewBag.userid_creacion =
                new SelectList(db.users, "user_id", "user_nombre", medios_conceptos.userid_creacion);
            ViewBag.user_idactualizacion =
                new SelectList(db.users, "user_id", "user_nombre", medios_conceptos.user_idactualizacion);
            ViewModeloMediosMtto ViewModeloMediosMtto = new ViewModeloMediosMtto
            {
                View_concepto = medios_conceptos
            };

            //ViewBag.user_idactualizacion = new SelectList(db.users, "user_id", "user_nombre", medios_formato.user_idactualizacion);
            return View(ViewModeloMediosMtto);
        }

        // POST: medios_conceptoss/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include =
                "id,idformato,concepto,descripcion,tope,id_licencia,fec_creacion,userid_creacion,fec_actualizacion,user_idactualizacion,estado,razon_inactivo")]
            medios_conceptos medios_conceptos)
        {
            if (ModelState.IsValid)
            {
                db.Entry(medios_conceptos).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.idformato = new SelectList(db.medios_formato, "id", "descripcion", medios_conceptos.idformato);
            ViewBag.userid_creacion =
                new SelectList(db.users, "user_id", "user_nombre", medios_conceptos.userid_creacion);
            ViewBag.user_idactualizacion =
                new SelectList(db.users, "user_id", "user_nombre", medios_conceptos.user_idactualizacion);
            return View(medios_conceptos);
        }

        // GET: medios_conceptoss/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            medios_conceptos medios_conceptos = db.medios_conceptos.Find(id);
            if (medios_conceptos == null)
            {
                return HttpNotFound();
            }

            return View(medios_conceptos);
        }

        // POST: medios_conceptoss/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            medios_conceptos medios_conceptos = db.medios_conceptos.Find(id);
            db.medios_conceptos.Remove(medios_conceptos);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }

        public JsonResult ActualizarConcepto(int idfor, int idCon, string descripcion, bool estado,
            string razon_inactivo)
        {
            bool result = false;
            if (idCon > 0)
            {
                medios_conceptos buscarValor = db.medios_conceptos.FirstOrDefault(c => c.id == idCon);
                if (buscarValor != null)
                {
                    if (estado)
                    {
                        // buscarValor.idformato = idfor;
                        buscarValor.descripcion = descripcion;
                        buscarValor.estado = estado;
                        buscarValor.razon_inactivo = null;
                        buscarValor.fec_actualizacion = DateTime.Now;
                        buscarValor.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    }
                    else
                    {
                        if (razon_inactivo == null)
                        {
                            // buscarValor.idformato = idfor;
                            buscarValor.descripcion = descripcion;
                            buscarValor.estado = estado;
                            buscarValor.fec_actualizacion = DateTime.Now;
                            buscarValor.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        }
                        else
                        {
                            // buscarValor.idformato = idfor;
                            buscarValor.descripcion = descripcion;
                            buscarValor.estado = estado;
                            buscarValor.razon_inactivo = razon_inactivo;
                            buscarValor.fec_actualizacion = DateTime.Now;
                            buscarValor.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        }
                    }


                    db.Entry(buscarValor).State = EntityState.Modified;
                    int actualizar = db.SaveChanges();
                    if (actualizar > 0)
                    {
                        result = true;
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    result = true;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }

            result = false;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult eliminarConcepto(int idDetalle)
        {
            medios_conceptos buscarConcepto = db.medios_conceptos.FirstOrDefault(m => m.id == idDetalle);
            if (buscarConcepto != null)
            {
                db.Entry(buscarConcepto).State = EntityState.Deleted;
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

        public JsonResult AgregarConcepto(int concepto, int idformato, string descripcion, bool estado,
            string razon_inactivo)
        {
            bool result = false;


            if (concepto > 0)
            {
                medios_conceptos buscarConcepto = db.medios_conceptos.FirstOrDefault(m => m.concepto == concepto);
                if (buscarConcepto != null)
                {
                    result = false;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                if (estado == false)
                {
                    db.medios_conceptos.Add(new medios_conceptos
                    {
                        concepto = concepto,
                        idformato = idformato,
                        descripcion = descripcion,
                        //tope = tope,
                        fec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        //fec_actualizacion = DateTime.Now,
                        //user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]),
                        estado = estado,
                        razon_inactivo = razon_inactivo
                    });
                }
                else
                {
                    db.medios_conceptos.Add(new medios_conceptos
                    {
                        concepto = concepto,
                        idformato = idformato,
                        descripcion = descripcion,
                        //  tope = tope,
                        fec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        //fec_actualizacion = DateTime.Now,
                        //user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]),
                        estado = estado
                    });
                }

                int guardar = db.SaveChanges();

                if (guardar > 0)
                {
                    result = true;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }

            result = false;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarAllConceptos()
        {
            var data = from conc in db.medios_conceptos
                       join form in db.medios_formato
                           on conc.idformato equals form.id
                       select new
                       {
                           conc.id,
                           conc.concepto,
                           conc.idformato,
                           desf = form.formato,
                           conc.descripcion,
                           //  conc.tope,
                           conc_estado = conc.estado ? "Activo" : "Inactivo"
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}