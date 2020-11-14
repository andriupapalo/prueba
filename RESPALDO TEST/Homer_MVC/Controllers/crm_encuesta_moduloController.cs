using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class crm_encuesta_moduloController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: crm_encuesta_modulo
        public ActionResult Index(int? menu)
        {
            System.Collections.Generic.List<crm_encuesta_modulo> crm_encuesta_modulo = db.crm_encuesta_modulo.ToList();
            ViewBag.datos = crm_encuesta_modulo;
            BuscarFavoritos(menu);
            return View();
        }

        // GET: crm_encuesta_modulo/Create
        public ActionResult Create(int? menu)
        {
            ViewBag.id = new SelectList(db.crm_encuesta_modulo, "id", "Descripcion");
            BuscarFavoritos(menu);
            return View();
        }

        // POST: crm_encuesta_modulo/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(crm_encuesta_modulo crm_encuesta_modulo, int? menu)
        {
            if (ModelState.IsValid)
            {
                db.crm_encuesta_modulo.Add(crm_encuesta_modulo);
                db.SaveChanges();
                TempData["mensaje"] = "El registro de la encuesta fue exitoso!";
                return RedirectToAction("Create", new { crm_encuesta_modulo.id, menu });
            }

            BuscarFavoritos(menu);
            return View(crm_encuesta_modulo);
        }

        // GET: crm_encuesta_modulo/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            crm_encuesta_modulo crm_encuesta_modulo = db.crm_encuesta_modulo.Find(id);
            if (crm_encuesta_modulo == null)
            {
                return HttpNotFound();
            }

            BuscarFavoritos(menu);
            return View(crm_encuesta_modulo);
        }

        // POST: crm_encuesta_modulo/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(crm_encuesta_modulo crm_encuesta_modulo, int? menu)
        {
            if (ModelState.IsValid)
            {
                db.Entry(crm_encuesta_modulo).State = EntityState.Modified;
                db.SaveChanges();
                TempData["mensaje"] = "La actualización de la encuesta fue exitosa!";
            }

            BuscarFavoritos(menu);
            return View(crm_encuesta_modulo);
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