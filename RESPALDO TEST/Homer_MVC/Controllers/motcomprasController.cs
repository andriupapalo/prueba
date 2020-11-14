using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class motcomprasController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();


        // GET: motcompras/Create
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View(new motcompra { estado = true });
        }

        // POST: motcompras/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include =
                "id,Motivo,fec_creacion,user_creacion,fec_actualizacion,user_actualizacion,estado,razon_inactivo,licencia")]
            motcompra motcompra, int? menu)
        {
            if (ModelState.IsValid)
            {
                motcompra.fec_creacion = DateTime.Now;
                motcompra.user_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.motcompra.Add(motcompra);
                int guardar = db.SaveChanges();
                if (guardar > 0)
                {
                    TempData["mensaje"] = "El registro del motivo de compra fue exitoso!";
                }
                else
                {
                    TempData["mensaje_error"] = "El registro no se ha guardado, por favor valide!";
                }

                return RedirectToAction("Create", new { menu });
            }

            BuscarFavoritos(menu);
            return View(motcompra);
        }

        // GET: motcompras/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            motcompra motcompra = db.motcompra.Find(id);
            if (motcompra == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(motcompra);
            BuscarFavoritos(menu);
            return View(motcompra);
        }

        // POST: motcompras/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include =
                "id,Motivo,fec_creacion,user_creacion,fec_actualizacion,user_actualizacion,estado,razon_inactivo,licencia")]
            motcompra motcompra, int? menu)
        {
            if (ModelState.IsValid)
            {
                motcompra.fec_actualizacion = DateTime.Now;
                motcompra.user_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.Entry(motcompra).State = EntityState.Modified;
                int guardar = db.SaveChanges();
                if (guardar > 0)
                {
                    TempData["mensaje"] = "La actualización del motivo de compra fue exitoso!";
                }
                else
                {
                    TempData["mensaje_error"] = "El registro no se ha guardado, por favor valide!";
                }

                return View();
            }

            BuscarFavoritos(menu);
            ConsultaDatosCreacion(motcompra);
            return View(motcompra);
        }

        public void ConsultaDatosCreacion(motcompra motcompra)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(motcompra.user_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = db.users.Find(motcompra.user_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult BuscarDatos()
        {
            var data = db.motcompra.ToList().Select(x => new
            {
                x.Motivo,
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