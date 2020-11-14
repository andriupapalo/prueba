using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tp_vehiculoController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();


        // GET: tp_vehiculo/Create
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        // POST: tp_vehiculo/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(tp_vehiculo tp_vehiculo, int? menu)
        {
            if (ModelState.IsValid)
            {
                tp_vehiculo existe = db.tp_vehiculo.FirstOrDefault(x => x.nombre == tp_vehiculo.nombre);
                if (existe == null)
                {
                    tp_vehiculo.user_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    tp_vehiculo.fec_creacion = DateTime.Now;
                    tp_vehiculo.bodega_id = Convert.ToInt32(Session["user_bodega"]);
                    db.tp_vehiculo.Add(tp_vehiculo);
                    db.SaveChanges();
                    TempData["mensaje"] = "Registro creado correctamente";
                    //return RedirectToAction("Edit", new { id = tp_vehiculo.id, menu = menu });
                    return View();
                }

                TempData["mensaje_error"] = "El registro ingresado ya existe, por favor valide";
            }
            else
            {
                TempData["mensaje_error"] = "Error al crear el registro por favor valide";
            }

            BuscarFavoritos(menu);
            return View();
        }

        // GET: tp_vehiculo/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tp_vehiculo tp_vehiculo = db.tp_vehiculo.Find(id);
            if (tp_vehiculo == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(tp_vehiculo);
            BuscarFavoritos(menu);
            return View(tp_vehiculo);
        }

        // POST: tp_vehiculo/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(tp_vehiculo tp_vehiculo, int? menu)
        {
            if (ModelState.IsValid)
            {
                tp_vehiculo.user_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                tp_vehiculo.fec_actualziacion = DateTime.Now;
                db.Entry(tp_vehiculo).State = EntityState.Modified;
                db.SaveChanges();
                TempData["mensaje"] = "Registro editado correctamente";
                ConsultaDatosCreacion(tp_vehiculo);
                return RedirectToAction("Edit", new { tp_vehiculo.id, menu });
            }

            TempData["mensaje_error"] = "Error al editar el registro por favor valide";
            ConsultaDatosCreacion(tp_vehiculo);
            BuscarFavoritos(menu);
            return View(tp_vehiculo);
        }

        public void ConsultaDatosCreacion(tp_vehiculo tp_vehiculo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(tp_vehiculo.user_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = db.users.Find(tp_vehiculo.user_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public ActionResult Browser()
        {
            ViewBag.datos = db.tp_vehiculo.ToList();
            return View();
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