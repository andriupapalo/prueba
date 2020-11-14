using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class ref_lineaController : Controller
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
        public ActionResult Create(ref_linea ref_linea, int? menu)
        {
            if (ModelState.IsValid)
            {
                ref_linea.fec_creacion = DateTime.Now;
                ref_linea.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.ref_linea.Add(ref_linea);

                ref_linea buscarCodigo = db.ref_linea.FirstOrDefault(x => x.codigo == ref_linea.codigo);
                if (buscarCodigo == null)
                {
                    db.SaveChanges();
                    TempData["mensaje"] = "La creación del registro fue exitoso";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El codigo ingresado ya existe, por favor valide";
            }

            BuscarFavoritos(menu);
            return View(ref_linea);
        }

        // GET: ref_linea/Edit/5
        public ActionResult Edit(string id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ref_linea ref_linea = db.ref_linea.Find(id);
            if (ref_linea == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(ref_linea);
            BuscarFavoritos(menu);
            return View(ref_linea);
        }

        // POST: ref_linea/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ref_linea ref_linea, int? menu)
        {
            if (ModelState.IsValid)
            {
                ref_linea.fec_actualizacion = DateTime.Now;
                ref_linea.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.Entry(ref_linea).State = EntityState.Modified;
                db.SaveChanges();
                TempData["mensaje"] = "La actualización del registro fue exitoso";
            }

            ConsultaDatosCreacion(ref_linea);
            BuscarFavoritos(menu);
            return View(ref_linea);
        }

        public void ConsultaDatosCreacion(ref_linea ref_linea)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(ref_linea.userid_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = db.users.Find(ref_linea.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarDatos()
        {
            var data = db.ref_linea.ToList().Select(x => new
            {
                x.codigo,
                x.Descripcion,
                estado = x.estado ? "Activo" : "Inactivo"
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