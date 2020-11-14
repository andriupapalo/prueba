using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class moneda_conversionController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: moneda_conversion
        public ActionResult Browser()
        {
            var datos = (from c in db.moneda_conversion
                         join b in db.monedas
                             on c.idmoneda equals b.moneda
                         select new
                         {
                             c.id,
                             c.fecha,
                             b.descripcion,
                             c.conversion
                         }).ToList();

            System.Collections.Generic.List<datosMoneda> data = datos.OrderBy(x => x.fecha).Select(c => new datosMoneda
            {
                id = c.id,
                fechax = c.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                descripcionx = c.descripcion,
                conversionx = c.conversion.ToString("N0", new CultureInfo("is-IS"))
            }).OrderBy(x => x.fechax).ToList();


            // var moneda_conversion = db.moneda_conversion.Include(m => m.monedas).Include(m => m.users).Include(m => m.users1);

            return View(data);
        }


        // GET: moneda_conversion/Create
        public ActionResult Create()
        {
            ViewBag.idmoneda = new SelectList(db.monedas.OrderBy(x => x.descripcion), "moneda", "descripcion");
            return View();
        }

        // POST: moneda_conversion/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(moneda_conversion moneda_conversion)
        {
            if (ModelState.IsValid)
            {
                moneda_conversion existe = db.moneda_conversion.FirstOrDefault(x =>
                    x.idmoneda == moneda_conversion.idmoneda && x.fecha == moneda_conversion.fecha);
                if (existe == null)
                {
                    moneda_conversion.fecha = moneda_conversion.fecha;
                    moneda_conversion.fec_creacion = DateTime.Now;
                    moneda_conversion.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    db.moneda_conversion.Add(moneda_conversion);
                    db.SaveChanges();
                    TempData["mensaje"] = "El registro fue guardado existosamente";
                    return RedirectToAction("Edit", new { moneda_conversion.id });
                }

                TempData["mensaje_error"] = "El registro ingresado ya existe, por favor valide";
            }

            ViewBag.idmoneda = new SelectList(db.monedas, "moneda", "descripcion", moneda_conversion.idmoneda);
            return View(moneda_conversion);
        }

        // GET: moneda_conversion/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            moneda_conversion moneda_conversion = db.moneda_conversion.Find(id);
            if (moneda_conversion == null)
            {
                return HttpNotFound();
            }

            ViewBag.idmoneda = new SelectList(db.monedas.OrderBy(x => x.descripcion), "moneda", "descripcion",
                moneda_conversion.idmoneda);
            ViewBag.userid_creacion = new SelectList(db.users.OrderBy(x => x.user_nombre), "user_id", "user_nombre",
                moneda_conversion.userid_creacion);
            ViewBag.user_idactualizacion = new SelectList(db.users.OrderBy(x => x.user_nombre), "user_id",
                "user_nombre", moneda_conversion.user_idactualizacion);
            ConsultaDatosCreacion(moneda_conversion);
            return View(moneda_conversion);
        }

        // POST: moneda_conversion/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(moneda_conversion moneda_conversion)
        {
            ConsultaDatosCreacion(moneda_conversion);
            if (ModelState.IsValid)
            {
                moneda_conversion.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                moneda_conversion.fec_actualizacion = DateTime.Now;
                db.Entry(moneda_conversion).State = EntityState.Modified;
                db.SaveChanges();
            }

            ViewBag.idmoneda = new SelectList(db.monedas.OrderBy(x => x.descripcion), "moneda", "descripcion",
                moneda_conversion.idmoneda);
            ViewBag.userid_creacion = new SelectList(db.users.OrderBy(x => x.user_nombre), "user_id", "user_nombre",
                moneda_conversion.userid_creacion);
            ViewBag.user_idactualizacion = new SelectList(db.users.OrderBy(x => x.user_nombre), "user_id",
                "user_nombre", moneda_conversion.user_idactualizacion);
            return View(moneda_conversion);
        }

        public void ConsultaDatosCreacion(moneda_conversion moneda_conversion)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(moneda_conversion.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = db.users.Find(moneda_conversion.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act =
                    modificator.userfec_actualizacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}