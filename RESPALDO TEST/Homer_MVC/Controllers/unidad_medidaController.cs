using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class unidad_medidaController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();


        // GET: ref_linea/Create
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        // POST: ref_linea/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(unidad_medida unidad, int? menu)
        {
            if (ModelState.IsValid)
            {
                unidad.fec_creacion = DateTime.Now;
                unidad.user_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.unidad_medida.Add(unidad);

                unidad_medida buscarDato = db.unidad_medida.FirstOrDefault(x => x.nombre == unidad.nombre);
                if (buscarDato == null)
                {
                    db.SaveChanges();
                    TempData["mensaje"] = "La creación del registro fue exitoso";
                }
                else
                {
                    TempData["mensaje_error"] = "El registro ingresado ya existe, por favor valide";
                }
            }

            BuscarFavoritos(menu);
            return View(unidad);
        }

        // GET: ref_linea/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            unidad_medida unidad = db.unidad_medida.Find(id);
            if (unidad == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(unidad);
            BuscarFavoritos(menu);
            return View(unidad);
        }

        // POST: ref_linea/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(unidad_medida und, int? menu)
        {
            if (ModelState.IsValid)
            {
                und.fec_actualizacion = DateTime.Now;
                und.user_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.Entry(und).State = EntityState.Modified;
                db.SaveChanges();
                TempData["mensaje"] = "La actualización del registro fue exitoso";
            }

            ConsultaDatosCreacion(und);
            BuscarFavoritos(menu);
            return View(und);
        }

        public void ConsultaDatosCreacion(unidad_medida und)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(und.user_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = db.users.Find(und.user_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult BuscarDatos()
        {
            var data = db.unidad_medida.ToList().Select(x => new
            {
                x.nombre,
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