using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class vmodelogsController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: vmodelogs
        public ActionResult Browser(int? menu)
        {
            BuscarFavoritos(menu);
            return View(db.vmodelog.ToList());
        }

        // GET: vmodelogs/Create
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        // POST: vmodelogs/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(vmodelog vmodelog, int? menu)
        {
            if (ModelState.IsValid)
            {
                vmodelog existe = db.vmodelog.FirstOrDefault(x => x.Descripcion == vmodelog.Descripcion);
                if (existe == null)
                {
                    vmodelog.fec_creacion = DateTime.Now;
                    vmodelog.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    db.vmodelog.Add(vmodelog);
                    db.SaveChanges();
                    TempData["mensaje"] = "Registro creado correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "El modelo ingresado ya existe, por favor valide";
                }
            }
            else
            {
                TempData["mensaje_error"] = "Error en la creación del modelo, por favor valide";
            }

            BuscarFavoritos(menu);
            return View(vmodelog);
        }

        // GET: vmodelogs/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            vmodelog vmodelog = db.vmodelog.Find(id);
            if (vmodelog == null)
            {
                return HttpNotFound();
            }

            BuscarFavoritos(menu);
            return View(vmodelog);
        }

        // POST: vmodelogs/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(vmodelog vmodelog, int? menu)
        {
            if (ModelState.IsValid)
            {
                vmodelog.fec_actualizacion = DateTime.Now;
                vmodelog.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.Entry(vmodelog).State = EntityState.Modified;
                db.SaveChanges();
                TempData["mensaje"] = "Registro editado correctamente";
            }
            else
            {
                TempData["mensaje_error"] = "Errores al editar el modelo, por favor valide";
            }

            BuscarFavoritos(menu);
            return View(vmodelog);
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