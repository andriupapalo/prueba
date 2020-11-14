using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tipo_carroceriaController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: tipo_carroceria
        public ActionResult Browser(int? menu)
        {
            BuscarFavoritos(menu);
            return View(db.tipo_carroceria.ToList());
        }

        // GET: tipo_carroceria/Create
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        // POST: tipo_carroceria/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(tipo_carroceria tipo_carroceria, int? menu)
        {
            if (ModelState.IsValid)
            {
                tipo_carroceria existe = db.tipo_carroceria.FirstOrDefault(x => x.descripcion == tipo_carroceria.descripcion);
                if (existe == null)
                {
                    tipo_carroceria.fec_creacion = DateTime.Now;
                    tipo_carroceria.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    db.tipo_carroceria.Add(tipo_carroceria);
                    db.SaveChanges();
                    TempData["mensaje"] = "Carroceria guardada correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "La correria ingresa ya existe, por favor valide";
                }

                //return RedirectToAction("Edit", new { id= tipo_carroceria.idcarroceria, menu});
                return RedirectToAction("Create");
            }

            TempData["mensaje_error"] = "Error al guardar los datos ingresados, por favor valide";
            BuscarFavoritos(menu);
            return View(tipo_carroceria);
        }

        // GET: tipo_carroceria/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tipo_carroceria tipo_carroceria = db.tipo_carroceria.Find(id);
            if (tipo_carroceria == null)
            {
                return HttpNotFound();
            }

            BuscarFavoritos(menu);
            return View(tipo_carroceria);
        }

        // POST: tipo_carroceria/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(tipo_carroceria tipo_carroceria, int? menu)
        {
            if (ModelState.IsValid)
            {
                tipo_carroceria.fec_actualizacion = DateTime.Now;
                tipo_carroceria.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.Entry(tipo_carroceria).State = EntityState.Modified;
                db.SaveChanges();
            }

            TempData["mensaje"] = "Carroceria guardada correctamente";
            BuscarFavoritos(menu);
            return View(tipo_carroceria);
        }

        public JsonResult buscarAjaxTipoCarroceria()
        {
            var data = db.tipo_carroceria.ToList().Select(x => new
            {
                x.idcarroceria,
                x.descripcion,
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