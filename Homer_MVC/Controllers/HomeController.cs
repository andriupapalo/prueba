using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Homer_MVC.IcebergModel;

namespace Homer_MVC.Controllers
{
    public class Home : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();
        CultureInfo miCulturaTime = new CultureInfo("en-US");

        public ActionResult Index()
        {
            var tiendas = db.Tienda.Where(d => d.Id >0).ToList();
            ViewBag.tienda = new SelectList(tiendas, "id", "Descripcion");

            var empleados = db.Empleado.ToList();
            ViewBag.empleado = new SelectList(empleados, "Id", "NombreCompleto");

            var cargos = db.Cargo.ToList();
            ViewBag.cargo = new SelectList(cargos, "Id", "Descripcion");



            ViewBag.fechadesde = DateTime.Now.ToString("yyyy-MM-dd", new CultureInfo("en-US"));
            ViewBag.fechahasta = DateTime.Now.ToString("yyyy-MM-dd", new CultureInfo("en-US"));

            return View();
            //return RedirectToAction("Login", "Login");
        }


        public JsonResult BuscarMovimientos(int tiend1,string emple1,int carg1, DateTime fecha_desde, DateTime fecha_hasta)
        {
            /*var predicate = PredicateBuilder.True<vw_Movimientos>();
            predicate = predicate.And(x => x.fecha>= fecha_desde);
            predicate = predicate.And(x => x.fecha<= fechaHasta);
            predicate = predicate.And(x => x.tiendaId == tiend1 && cardoid==carg1 && x.cedula.Trim()==emple1.Trim());*/
            var movimi = db.vm_Movimientos.Where(x => x.tiendaid == tiend1 && x.cargoid == carg1 && x.Cedula.Trim() == emple1.Trim() && (x.fecha >= fecha_desde && x.fecha <= fecha_hasta));
            fecha_hasta = fecha_hasta.AddDays(1);
            var resulmovi = movimi.Select(s => new
            {
                s.fecha,
                s.nomtienda,
                s.Cedula,
                s.nomempleado,
                s.NomCargo,
                s.totalhoras,
            }).Distinct().ToList();
            return Json(resulmovi, JsonRequestBehavior.AllowGet);
        }
    }
}


