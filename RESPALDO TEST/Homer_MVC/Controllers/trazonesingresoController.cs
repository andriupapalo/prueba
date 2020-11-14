using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class trazonesingresoController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: trazonesingreso
        public ActionResult Index(int? menu)
        {
            IQueryable<trazonesingreso> trazonesingreso = db.trazonesingreso.Include(t => t.users).Include(t => t.users1);
            BuscarFavoritos(menu);
            return View(trazonesingreso.ToList());
        }


        // GET: trazonesingreso/Create
        public ActionResult Create(int? menu)
        {
            ViewBag.userid_creacion = new SelectList(db.users, "user_id", "user_nombre");
            ViewBag.user_idactualizacion = new SelectList(db.users, "user_id", "user_nombre");
            BuscarFavoritos(menu);
            return View();
        }

        // POST: trazonesingreso/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(trazonesingreso trazonesingreso, int? menu)
        {
            if (ModelState.IsValid)
            {
                trazonesingreso.fec_creacion = DateTime.Now;
                trazonesingreso.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.trazonesingreso.Add(trazonesingreso);
                db.SaveChanges();

                TempData["mensaje"] = "Registro creado correctamente";

                return RedirectToAction("Update", new { trazonesingreso.id, menu });
            }

            TempData["mensaje_error"] = "Error al crear el registro, por favor valide";
            ViewBag.userid_creacion =
                new SelectList(db.users, "user_id", "user_nombre", trazonesingreso.userid_creacion);
            ViewBag.user_idactualizacion =
                new SelectList(db.users, "user_id", "user_nombre", trazonesingreso.user_idactualizacion);
            BuscarFavoritos(menu);
            return View(trazonesingreso);
        }

        // GET: trazonesingreso/Edit/5
        public ActionResult Update(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            trazonesingreso trazonesingreso = db.trazonesingreso.Find(id);
            if (trazonesingreso == null)
            {
                return HttpNotFound();
            }

            ViewBag.userid_creacion =
                new SelectList(db.users, "user_id", "user_nombre", trazonesingreso.userid_creacion);
            ViewBag.user_idactualizacion =
                new SelectList(db.users, "user_id", "user_nombre", trazonesingreso.user_idactualizacion);
            ConsultaDatosCreacion(trazonesingreso);
            BuscarFavoritos(menu);
            return View(trazonesingreso);
        }

        // POST: trazonesingreso/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(trazonesingreso trazonesingreso, int? menu)
        {
            if (ModelState.IsValid)
            {
                trazonesingreso.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                trazonesingreso.fec_actualizacion = DateTime.Now;
                db.Entry(trazonesingreso).State = EntityState.Modified;
                db.SaveChanges();

                TempData["mensaje"] = "Registro actualizado correctamente";
                ConsultaDatosCreacion(trazonesingreso);
                BuscarFavoritos(menu);
                return View(trazonesingreso);
            }

            TempData["mensaje"] = "Error al actualizar el registro, por favor valide";

            ViewBag.userid_creacion =
                new SelectList(db.users, "user_id", "user_nombre", trazonesingreso.userid_creacion);
            ViewBag.user_idactualizacion =
                new SelectList(db.users, "user_id", "user_nombre", trazonesingreso.user_idactualizacion);
            ConsultaDatosCreacion(trazonesingreso);
            BuscarFavoritos(menu);
            return View(trazonesingreso);
        }


        public void ConsultaDatosCreacion(trazonesingreso trazonesingreso)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(trazonesingreso.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = db.users.Find(trazonesingreso.user_idactualizacion);
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