using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class ubicacion_bodegaController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();


        // GET: ubicacion_bodega/Create
        public ActionResult Create(int? menu)
        {
            ViewBag.idbodega =
                new SelectList(db.bodega_concesionario.Where(x => x.bodccs_estado).OrderBy(x => x.bodccs_nombre), "id",
                    "bodccs_nombre");
            ViewBag.tipo = new SelectList(db.ubicacion_bodega_tipo.OrderBy(x => x.Descripcion), "id", "Descripcion");
            BuscarFavoritos(menu);
            return View(new ubicacion_bodega { estado = true });
        }

        // POST: ubicacion_bodega/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ubicacion_bodega ubicacion_bodega, int? menu)
        {
            if (ModelState.IsValid)
            {
                IQueryable<ubicacion_bodega> existe = db.ubicacion_bodega.Where(x =>
                    x.bodega == ubicacion_bodega.bodega && x.ubicacion == ubicacion_bodega.ubicacion);

                if (existe != null)
                {
                    string buscarCodigo = db.bodega_concesionario.Where(x => x.id == ubicacion_bodega.idbodega)
                        .Select(x => x.bodccs_cod).FirstOrDefault();
                    ubicacion_bodega.fec_creacion = DateTime.Now;
                    ubicacion_bodega.user_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    ubicacion_bodega.bodega = buscarCodigo;
                    db.ubicacion_bodega.Add(ubicacion_bodega);
                    int result = db.SaveChanges();
                    if (result > 0)
                    {
                        TempData["mensaje"] = "Registro creado correctamente";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error al crear el registro, por favor intente nuevamente";
                    }
                }
                else
                {
                    TempData["mensaje_error"] = "La ubicación y la bodega ingresada ya existen ";
                }
            }
            else
            {
                TempData["mensaje_error"] = "Error al crear el registro, por favor intente nuevamente";
            }

            ViewBag.idbodega =
                new SelectList(db.bodega_concesionario.Where(x => x.bodccs_estado).OrderBy(x => x.bodccs_nombre), "id",
                    "bodccs_nombre");
            ViewBag.tipo = new SelectList(db.ubicacion_bodega_tipo.OrderBy(x => x.Descripcion), "id", "Descripcion");
            BuscarFavoritos(menu);
            return View(ubicacion_bodega);
        }

        // GET: ubicacion_bodega/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ubicacion_bodega ubicacion_bodega = db.ubicacion_bodega.Find(id);
            if (ubicacion_bodega == null)
            {
                return HttpNotFound();
            }

            //ViewBag.bodega = new SelectList(db.bodega_concesionario, "bodccs_cod", "bodccs_nombre");
            ViewBag.idbodega =
                new SelectList(db.bodega_concesionario.Where(x => x.bodccs_estado).OrderBy(x => x.bodccs_nombre), "id",
                    "bodccs_nombre", ubicacion_bodega.idbodega);
            ViewBag.tipo = new SelectList(db.ubicacion_bodega_tipo.OrderBy(x => x.Descripcion), "id", "Descripcion");
            ConsultaDatosCreacion(ubicacion_bodega);
            BuscarFavoritos(menu);
            return View(ubicacion_bodega);
        }


        // POST: ubicacion_bodega/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ubicacion_bodega ubicacion_bodega, int? menu)
        {
            if (ModelState.IsValid)
            {
                string buscarCodigo = db.bodega_concesionario.Where(x => x.id == ubicacion_bodega.idbodega)
                    .Select(x => x.bodccs_cod).FirstOrDefault();
                ubicacion_bodega.bodega = buscarCodigo;
                ubicacion_bodega.fec_actualizacion = DateTime.Now;
                ubicacion_bodega.user_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.Entry(ubicacion_bodega).State = EntityState.Modified;
                int result = db.SaveChanges();
                if (result > 0)
                {
                    TempData["mensaje"] = "Registro actualizado correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "Error al actualizar el registro, por favor intente nuevamente";
                }
            }

            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag

            // ViewBag.bodega = new SelectList(db.bodega_concesionario, "bodccs_cod", "bodccs_nombre");

            ViewBag.idbodega =
                new SelectList(db.bodega_concesionario.Where(x => x.bodccs_estado).OrderBy(x => x.bodccs_nombre), "id",
                    "bodccs_nombre", ubicacion_bodega.idbodega);
            ViewBag.tipo = new SelectList(db.ubicacion_bodega_tipo.OrderBy(x => x.Descripcion), "id", "Descripcion");
            ConsultaDatosCreacion(ubicacion_bodega);
            BuscarFavoritos(menu);
            return View(ubicacion_bodega);
        }

        public void ConsultaDatosCreacion(ubicacion_bodega ub)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(ub.user_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            int idUsuarioActualizacion = ub.user_actualizacion ?? 0;
            users modificator = db.users.Find(idUsuarioActualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult BuscarDatos()
        {
            var data = from u in db.ubicacion_bodega
                       join b in db.bodega_concesionario
                           on u.bodega equals b.bodccs_cod
                       select new
                       {
                           u.bodega,
                           u.descripcion,
                           u.ubicacion,
                           u.id
                       };
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