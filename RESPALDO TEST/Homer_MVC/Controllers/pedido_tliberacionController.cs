using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class pedido_tliberacionController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: pedido_tliberacion/Create
        public ActionResult Create(int? menu)
        {
            ViewBag.tpvehiculo = new SelectList(db.tp_vehiculo.OrderBy(x => x.nombre), "id", "nombre");
            BuscarFavoritos(menu);
            return View();
        }

        // POST: pedido_tliberacion/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(pedido_tliberacion pedido_tliberacion, int? menu)
        {
            if (ModelState.IsValid)
            {
                pedido_tliberacion existe = db.pedido_tliberacion.FirstOrDefault(x => x.tpvehiculo == pedido_tliberacion.tpvehiculo);
                if (existe == null)
                {
                    pedido_tliberacion.user_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    pedido_tliberacion.fec_creacion = DateTime.Now;
                    pedido_tliberacion.bodega_id = Convert.ToInt32(Session["user_bodega"]);
                    db.pedido_tliberacion.Add(pedido_tliberacion);
                    db.SaveChanges();
                    TempData["mensaje"] = "Registro creado correctamente";
                    ViewBag.tpvehiculo = new SelectList(db.tp_vehiculo.OrderBy(x => x.nombre), "id", "nombre");
                    //return RedirectToAction("Edit", new { id = pedido_tliberacion.id, menu });
                    return View();
                }

                TempData["mensaje_error"] = "El registro ingresado ya existe, por favor valide";
            }
            else
            {
                TempData["mensaje_error"] = "Error al crear el registro por favor valide";
            }

            ViewBag.tpvehiculo = new SelectList(db.tp_vehiculo.OrderBy(x => x.nombre), "id", "nombre");
            BuscarFavoritos(menu);
            return View(pedido_tliberacion);
        }

        // GET: pedido_tliberacion/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            pedido_tliberacion pedido_tliberacion = db.pedido_tliberacion.Find(id);
            if (pedido_tliberacion == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(pedido_tliberacion);
            ViewBag.tpvehiculo = new SelectList(db.tp_vehiculo.OrderBy(x => x.nombre), "id", "nombre",
                pedido_tliberacion.tpvehiculo);
            BuscarFavoritos(menu);
            return View(pedido_tliberacion);
        }

        // POST: pedido_tliberacion/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(pedido_tliberacion pedido_tliberacion, int? menu)
        {
            if (ModelState.IsValid)
            {
                pedido_tliberacion.user_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                pedido_tliberacion.fec_actualizacion = DateTime.Now;
                db.Entry(pedido_tliberacion).State = EntityState.Modified;
                db.SaveChanges();
                ConsultaDatosCreacion(pedido_tliberacion);
                TempData["mensaje"] = "Registro editado correctamente";
                ViewBag.tpvehiculo = new SelectList(db.tp_vehiculo.OrderBy(x => x.nombre), "id", "nombre",
                    pedido_tliberacion.tpvehiculo);
                return RedirectToAction("Edit", new { pedido_tliberacion.id, menu });
            }

            TempData["mensaje_error"] = "Error al editar el registro";
            ConsultaDatosCreacion(pedido_tliberacion);
            ViewBag.tpvehiculo = new SelectList(db.tp_vehiculo.OrderBy(x => x.nombre), "id", "nombre",
                pedido_tliberacion.tpvehiculo);
            BuscarFavoritos(menu);
            return View(pedido_tliberacion);
        }

        public void ConsultaDatosCreacion(pedido_tliberacion pedido_tliberacion)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(pedido_tliberacion.user_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = db.users.Find(pedido_tliberacion.user_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public ActionResult Browser(int? menu)
        {
            ViewBag.datos = db.pedido_tliberacion.ToList();
            BuscarFavoritos(menu);
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