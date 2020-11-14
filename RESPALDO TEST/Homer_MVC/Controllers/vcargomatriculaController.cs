using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class vcargomatriculaController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();


        // GET: vcargomatricula/Create
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        // POST: vcargomatricula/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(vcargomatricula vcargomatricula, int? menu)
        {
            if (ModelState.IsValid)
            {
                vcargomatricula cargo = db.vcargomatricula.FirstOrDefault(x => x.descripcion == vcargomatricula.descripcion);

                if (cargo == null)
                {
                    vcargomatricula.fec_creacion = DateTime.Now;
                    vcargomatricula.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    db.vcargomatricula.Add(vcargomatricula);
                    db.SaveChanges();
                    TempData["mensaje"] = "Registro Creado Correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "La condición de pago ingresada ya existe, por favor valide";
                }
            }

            BuscarFavoritos(menu);
            return View(vcargomatricula);
        }

        // GET: vcargomatricula/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            vcargomatricula vcargomatricula = db.vcargomatricula.Find(id);
            if (vcargomatricula == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(vcargomatricula);
            BuscarFavoritos(menu);
            return View(vcargomatricula);
        }

        // POST: vcargomatricula/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(vcargomatricula vcargomatricula, int? menu)
        {
            if (ModelState.IsValid)
            {
                vcargomatricula.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                vcargomatricula.fec_actualizacion = DateTime.Now;
                db.Entry(vcargomatricula).State = EntityState.Modified;
                db.SaveChanges();
                TempData["mensaje"] = "Registro editado Correctamente";
            }

            BuscarFavoritos(menu);
            return View(vcargomatricula);
        }

        public ActionResult Browser(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public void ConsultaDatosCreacion(vcargomatricula vcargomatricula)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(vcargomatricula.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = db.users.Find(vcargomatricula.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult BuscarDatos()
        {
            var data = db.vcargomatricula.ToList().Select(x => new
            {
                x.descripcion,
                estado = x.estado ? "Activo" : "Inactivo",
                x.id
            });
            return Json(data, JsonRequestBehavior.AllowGet);
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