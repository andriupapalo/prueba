using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class administracionVehiculosController : Controller
    {
        // GET: administracionVehiculos
        public ActionResult BrowserAdministracionVehiculosNuevos()
        {
            return View();
        }

        // GET: administracionVehiculos/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }


        // GET: administracionVehiculos/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: administracionVehiculos/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: administracionVehiculos/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: administracionVehiculos/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: administracionVehiculos/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: administracionVehiculos/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}