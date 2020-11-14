using Homer_MVC.IcebergModel;
using Homer_MVC.ViewModels.medios;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers.medios
{
    public class medios_formatosController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();


        // GET: medios_formatos
        public ActionResult Index()
        {
            IQueryable<medios_formato> medios_formato = db.medios_formato.Include(m => m.users).Include(m => m.users1);
            return View(medios_formato.ToList());
        }

        // GET: medios_formatos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            medios_formato medios_formato = db.medios_formato.Find(id);
            if (medios_formato == null)
            {
                return HttpNotFound();
            }

            return View(medios_formato);
        }

        // GET: medios_formatos/Create
        public ActionResult Create()
        {
            ViewModeloMediosMtto ViewModeloMediosMtto = new ViewModeloMediosMtto();
            //ViewBag.userid_creacion = new SelectList(db.users, "user_id", "user_nombre");
            //ViewBag.user_idactualizacion = new SelectList(db.users, "user_id", "user_nombre");
            return View(ViewModeloMediosMtto);
        }

        // POST: medios_formatos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ViewModeloMediosMtto medios_formato, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in db.medios_formato
                           where a.formato == medios_formato.View_formato.formato
                           select a.descripcion).Count();

                if (nom == 0)
                {
                    medios_formato.View_formato.fec_creacion = DateTime.Now;
                    medios_formato.View_formato.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    medios_formato.View_formato.fec_actualizacion = DateTime.Now;
                    medios_formato.View_formato.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    db.medios_formato.Add(medios_formato.View_formato);
                    db.SaveChanges();
                    TempData["mensaje"] = "El registro del nuevo servicio de vehiculo fue exitoso!";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            //ViewBag.userid_creacion = new SelectList(db.users, "user_id", "user_nombre", medios_formato.userid_creacion);
            //ViewBag.user_idactualizacion = new SelectList(db.users, "user_id", "user_nombre", medios_formato.user_idactualizacion);
            return View(medios_formato);
        }


        // GET: medios_formatos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            medios_formato mediosformato = db.medios_formato.Find(id);

            if (mediosformato == null)
            {
                return HttpNotFound();
            }

            ViewModeloMediosMtto ViewModeloMediosMtto = new ViewModeloMediosMtto();
            {
                //ViewModeloMediosMtto.View_formato.formato = mediosformato.formato;
                //ViewModeloMediosMtto.View_formato.descripcion = mediosformato.descripcion;
                //ViewModeloMediosMtto.View_formato.formato = mediosformato.formato;
                ViewModeloMediosMtto.View_formato = mediosformato;
                ViewModeloMediosMtto.View_formato.tope = mediosformato.tope;
                ViewModeloMediosMtto.monto = mediosformato.tope.ToString("N0");
                //ViewModeloMediosMtto.View_formato.formato = mediosformato.formato;

                //View_formato = mediosformato,
            }
            ;


            //ViewBag.user_idactualizacion = new SelectList(db.users, "user_id", "user_nombre", medios_formato.user_idactualizacion);
            return View(ViewModeloMediosMtto);
        }

        // POST: medios_formatos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,formato,descripcion,tope,estado,razon_inactivo")]
            medios_formato medios_formato, ViewModeloMediosMtto ViewModeloMediosMtto)
        {
            if (ModelState.IsValid)
            {
                //where a.formato == tpser_vh.tpserv_nombre || a.tpserv_id == tpser_vh.tpserv_id
                int nom = (from a in db.medios_formato
                           where a.formato == medios_formato.formato || a.id == medios_formato.id
                           select a.descripcion).Count();

                if (nom == 1)
                {
                    medios_formato.fec_actualizacion = DateTime.Now;
                    medios_formato.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    medios_formato.tope = Convert.ToDecimal(ViewModeloMediosMtto.monto);

                    db.Entry(medios_formato).State = EntityState.Modified;
                    db.SaveChanges();

                    TempData["mensaje"] = "La actualización del servicio fue exitosa!";
                    // BuscarFavoritos(menu);
                    return View(medios_formato);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            //ViewBag.userid_creacion = new SelectList(db.users, "user_id", "user_nombre", medios_formato.userid_creacion);
            //ViewBag.user_idactualizacion = new SelectList(db.users, "user_id", "user_nombre", medios_formato.user_idactualizacion);
            return View(medios_formato);
        }

        // GET: medios_formatos/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    medios_formato medios_formato = db.medios_formato.Find(id);
        //    if (medios_formato == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(medios_formato);
        //}

        // POST: medios_formatos/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    medios_formato medios_formato = db.medios_formato.Find(id);
        //    db.medios_formato.Remove(medios_formato);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        public JsonResult ActualizarFormato
        (
            int id,
            string descripcion,
            decimal tope,
            bool estado,
            string razon_inactivo
        )
        {
            bool result = false;
            if (id > 0)
            {
                medios_formato buscarValor = db.medios_formato.FirstOrDefault(c => c.id == id);
                if (buscarValor != null)
                {
                    if (estado)
                    {
                        buscarValor.descripcion = descripcion;
                        buscarValor.tope = tope;
                        buscarValor.estado = estado;
                        buscarValor.razon_inactivo = null;
                        buscarValor.fec_actualizacion = DateTime.Now;
                        buscarValor.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    }
                    else
                    {
                        if (razon_inactivo == null)
                        {
                            buscarValor.descripcion = descripcion;
                            buscarValor.tope = tope;
                            buscarValor.estado = estado;
                            buscarValor.razon_inactivo = null;
                            buscarValor.fec_actualizacion = DateTime.Now;
                            buscarValor.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        }
                        else
                        {
                            buscarValor.descripcion = descripcion;
                            buscarValor.tope = tope;
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

        public JsonResult eliminarFormato(int idDetalle)
        {
            medios_formato buscarFormato = db.medios_formato.FirstOrDefault(m => m.id == idDetalle);
            if (buscarFormato != null)
            {
                db.Entry(buscarFormato).State = EntityState.Deleted;
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }

        public JsonResult AgregarFormato
        (
            int formato,
            string descripcion,
            decimal tope,
            bool estado,
            string razon_inactivo
        )
        {
            bool result = false;


            if (formato > 0)
            {
                medios_formato buscarFormato = db.medios_formato.FirstOrDefault(m => m.formato == formato);
                if (buscarFormato != null)
                {
                    result = false;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                if (estado == false)
                {
                    db.medios_formato.Add(new medios_formato
                    {
                        formato = formato,
                        descripcion = descripcion,
                        tope = tope,
                        fec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        fec_actualizacion = DateTime.Now,
                        user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]),
                        estado = estado,
                        razon_inactivo = razon_inactivo
                    });
                }
                else
                {
                    db.medios_formato.Add(new medios_formato
                    {
                        formato = formato,
                        descripcion = descripcion,
                        tope = tope,
                        fec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        fec_actualizacion = DateTime.Now,
                        user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]),
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

        public JsonResult BuscarAllFormatos()
        {
            var data = db.medios_formato.ToList().Select(x => new
            {
                x.id,
                x.formato,
                x.descripcion,
                x.tope,
                form_estado = x.estado ? "Activo" : "Inactivo"
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}