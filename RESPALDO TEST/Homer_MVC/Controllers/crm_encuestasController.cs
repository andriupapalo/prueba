using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class crm_encuestasController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: crm_encuestas
        public ActionResult Index(int? menu)
        {
            IQueryable<crm_encuestas> crm_encuestas = db.crm_encuestas.Include(c => c.crm_encuesta_modulo);
            BuscarFavoritos(menu);
            return View(crm_encuestas.ToList());
        }

        public ActionResult Create(int? menu)
        {
            ViewBag.modulo = new SelectList(db.crm_encuesta_modulo, "id", "Descripcion");
            BuscarFavoritos(menu);
            return View();
        }

        // POST: crm_encuestas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(crm_encuestas crm_encuestas, int? menu)
        {
            if (ModelState.IsValid)
            {
                crm_encuestas.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                crm_encuestas.fec_creacion = DateTime.Now;
                db.crm_encuestas.Add(crm_encuestas);
                db.SaveChanges();
                TempData["mensaje"] = "Registro creado correctamente";
                return RedirectToAction("Edit", new { crm_encuestas.id, menu });
            }

            ViewBag.modulo = new SelectList(db.crm_encuesta_modulo, "id", "Descripcion", crm_encuestas.modulo);
            return View(crm_encuestas);
        }

        // GET: crm_encuestas/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            crm_encuestas crm_encuestas = db.crm_encuestas.Find(id);
            if (crm_encuestas == null)
            {
                return HttpNotFound();
            }

            ViewBag.modulo = new SelectList(db.crm_encuesta_modulo, "id", "Descripcion", crm_encuestas.modulo);
            ConsultaDatosCreacion(crm_encuestas);
            BuscarFavoritos(menu);
            return View(crm_encuestas);
        }

        // POST: crm_encuestas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(crm_encuestas crm_encuestas, int? menu)
        {
            if (ModelState.IsValid)
            {
                crm_encuestas.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                crm_encuestas.fec_actualizacion = DateTime.Now;
                db.Entry(crm_encuestas).State = EntityState.Modified;
                db.SaveChanges();
                TempData["mensaje"] = "Registro editado correctamente";
            }

            ViewBag.modulo = new SelectList(db.crm_encuesta_modulo, "id", "Descripcion", crm_encuestas.modulo);
            ConsultaDatosCreacion(crm_encuestas);
            BuscarFavoritos(menu);
            return View(crm_encuestas);
        }

        public void ConsultaDatosCreacion(crm_encuestas crm_encuestas)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(crm_encuestas.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = db.users.Find(crm_encuestas.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
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