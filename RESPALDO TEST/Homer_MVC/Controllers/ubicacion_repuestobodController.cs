using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class ubicacion_repuestobodController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: ubicacion_repuestobod
        public ActionResult Index(int? menu)
        {
            var datos = (from a in db.ubicacion_repuestobod
                         join b in db.estanterias
                             on a.id_estanteria equals b.id
                         join c in db.area_bodega
                             on b.id_area equals c.areabod_id
                         join d in db.bodega_concesionario
                             on c.id_bodega equals d.id
                         select new
                         {
                             a.id,
                             ubicacion = a.descripcion,
                             estanteria = b.descripcion,
                             area = c.areabod_nombre,
                             bodega = d.bodccs_nombre,
                             estado = a.ubirpto_estado
                         }).ToList();

            System.Collections.Generic.List<modelo_ubicacionrepuestosbod> data = datos.OrderBy(x => x.ubicacion).Select(c => new modelo_ubicacionrepuestosbod
            {
                id = c.id,
                ubicacion = c.ubicacion,
                estanteria = c.estanteria,
                area = c.area,
                bodega = c.bodega,
                estado = c.estado ? "Activo" : "Inactivo"
            }).OrderBy(x => x.ubicacion).ToList();

            BuscarFavoritos(menu);
            return View(data);


            //BuscarFavoritos(menu);
            // return View(db.ubicacion_repuestobod.ToList());
        }


        // GET: ubicacion_repuestobod/Create
        public ActionResult Create(int? menu)
        {
            int bodega = Convert.ToInt32(Session["user_bodega"]);
            ViewBag.bodegas =
                new SelectList(
                    db.bodega_concesionario.Where(x => x.bodccs_estado && x.id == bodega).OrderBy(x => x.bodccs_nombre),
                    "id", "bodccs_nombre");
            ViewBag.area_bodega =
                new SelectList(db.area_bodega.Where(x => x.areabod_estado).OrderBy(x => x.areabod_nombre), "areabod_id",
                    "areabod_nombre");

            BuscarFavoritos(menu);
            return View();
        }

        // POST: ubicacion_repuestobod/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Ubicacion_RepuestoModel ubicacion, int? menu)
        {
            int bodega = Convert.ToInt32(Session["user_bodega"]);
            int stand = Convert.ToInt32(ubicacion.id_estanteria);
            ubicacion_repuestobod buscar = db.ubicacion_repuestobod
                .Where(x => x.id_estanteria == stand && x.descripcion == ubicacion.descripcion).FirstOrDefault();
            if (buscar == null)
            {
                if (ModelState.IsValid)
                {
                    ubicacion_repuestobod nuevo = new ubicacion_repuestobod
                    {
                        id_estanteria = Convert.ToInt32(ubicacion.id_estanteria),
                        descripcion = ubicacion.descripcion,
                        ubirptouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        ubirptofec_creacion = DateTime.Now,
                        ubirpto_estado = ubicacion.ubirpto_estado,
                        ubirptorazoninactivo = ubicacion.ubirptorazoninactivo,
                        fisico = false
                    };
                    db.ubicacion_repuestobod.Add(nuevo);
                    db.SaveChanges();
                    TempData["mensaje"] = "El registro de la nueva ubicacion de repuestos fue exitoso!";
                    ViewBag.area_bodega =
                        new SelectList(db.area_bodega.Where(x => x.areabod_estado).OrderBy(x => x.areabod_nombre),
                            "areabod_id", "areabod_nombre");
                    ViewBag.bodegas =
                        new SelectList(
                            db.bodega_concesionario.Where(x => x.bodccs_estado && x.id == bodega)
                                .OrderBy(x => x.bodccs_nombre), "id", "bodccs_nombre");
                    return View();
                }
            }
            else
            {
                TempData["mensaje_error"] = "Error en el registro de la ubicacion, por favor valide!";
                BuscarFavoritos(menu);
                ViewBag.area_bodega =
                    new SelectList(db.area_bodega.Where(x => x.areabod_estado).OrderBy(x => x.areabod_nombre),
                        "areabod_id", "areabod_nombre");
                ViewBag.bodegas =
                    new SelectList(
                        db.bodega_concesionario.Where(x => x.bodccs_estado && x.id == bodega)
                            .OrderBy(x => x.bodccs_nombre), "id", "bodccs_nombre");
                return View();
            }

            TempData["mensaje_error"] = "Error en el registro de la ubicacion, por favor valide!";
            BuscarFavoritos(menu);
            ViewBag.area_bodega =
                new SelectList(db.area_bodega.Where(x => x.areabod_estado).OrderBy(x => x.areabod_nombre), "areabod_id",
                    "areabod_nombre");
            ViewBag.bodegas =
                new SelectList(
                    db.bodega_concesionario.Where(x => x.bodccs_estado && x.id == bodega).OrderBy(x => x.bodccs_nombre),
                    "id", "bodccs_nombre");
            return View();
        }

        // GET: ubicacion_repuestobod/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ubicacion_repuestobod ubicacion_repuestobod = db.ubicacion_repuestobod.Find(id);
            string nombreEstanteria = db.estanterias.Where(x => x.id == ubicacion_repuestobod.id_estanteria)
                .Select(x => x.descripcion).FirstOrDefault();
            Ubicacion_RepuestoModel ubicacion = new Ubicacion_RepuestoModel
            {
                id = ubicacion_repuestobod.id,
                id_estanteria = Convert.ToString(ubicacion_repuestobod.id_estanteria)
            };
            ubicacion.id_estanteria = nombreEstanteria;
            ubicacion.descripcion = ubicacion_repuestobod.descripcion;
            ubicacion.ubirptofec_creacion = ubicacion_repuestobod.ubirptofec_creacion;
            ubicacion.ubirptofec_creacion = ubicacion_repuestobod.ubirptofec_creacion;
            ubicacion.ubirpto_estado = ubicacion_repuestobod.ubirpto_estado;
            ubicacion.ubirptorazoninactivo = ubicacion_repuestobod.ubirptorazoninactivo;

            if (ubicacion_repuestobod == null)
            {
                return HttpNotFound();
            }

            var enlace = (from a in db.ubicacion_repuestobod
                          join b in db.estanterias
                              on a.id_estanteria equals b.id
                          join c in db.area_bodega
                              on b.id_area equals c.areabod_id
                          join d in db.bodega_concesionario
                              on c.id_bodega equals d.id
                          where a.id == ubicacion_repuestobod.id
                          select new
                          {
                              estanteria = b.id,
                              c.areabod_id,
                              d.id
                          }).FirstOrDefault();
            //ViewBag.area_bodega = new SelectList(db.area_bodega.Where(x => x.areabod_estado != false).OrderBy(x => x.areabod_nombre), "areabod_id", "areabod_nombre",ubicacion_repuestobod.idarea);
            int bodega = Convert.ToInt32(Session["user_bodega"]);
            ViewBag.bodegas =
                new SelectList(
                    db.bodega_concesionario.Where(x => x.bodccs_estado && x.id == bodega).OrderBy(x => x.bodccs_nombre),
                    "id", "bodccs_nombre", enlace.id);
            ViewBag.id_area = new SelectList(db.area_bodega.Where(x => x.areabod_estado).OrderBy(x => x.areabod_nombre),
                "areabod_id", "areabod_nombre", enlace.areabod_id);
            ViewBag.estanteria = new SelectList(db.estanterias.Where(x => x.estado).OrderBy(x => x.descripcion), "id",
                "descripcion", enlace.estanteria);
            //ViewBag.area_bodega = new SelectList(db.area_bodega.Where(x => x.areabod_estado != false).OrderBy(x => x.areabod_nombre), "areabod_id", "areabod_nombre");
            ConsultaDatosCreacion(ubicacion_repuestobod);
            BuscarFavoritos(menu);
            return View(ubicacion);
        }

        // POST: ubicacion_repuestobod/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Ubicacion_RepuestoModel ubicacion, int? menu)
        {
            int area = Convert.ToInt32(Request["areaVal"]);
            ubicacion_repuestobod ubicacion_repuestobod = db.ubicacion_repuestobod.Find(ubicacion.id);
            if (ubicacion_repuestobod != null)
            {
                if (ModelState.IsValid)
                {
                    ubicacion_repuestobod nuevo = db.ubicacion_repuestobod.Where(x => x.id == ubicacion.id).FirstOrDefault();
                    nuevo.id_estanteria = nuevo.id_estanteria;
                    nuevo.descripcion = ubicacion.descripcion;
                    nuevo.ubirpto_estado = ubicacion.ubirpto_estado;
                    nuevo.ubirptouserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    nuevo.ubirptofec_actualizacion = DateTime.Now;
                    nuevo.fisico = false;
                    nuevo.ubirptorazoninactivo = ubicacion.ubirptorazoninactivo;
                    db.Entry(nuevo).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["mensaje"] = "El registro de la nueva ubicacion de repuestos fue exitoso!";
                }

                ConsultaDatosCreacion(ubicacion_repuestobod);
                BuscarFavoritos(menu);
            }
            else
            {
                TempData["mensaje_error"] = "La ubicación no fue encontrada, por favor valide!";
                BuscarFavoritos(menu);
            }

            var enlace = (from a in db.ubicacion_repuestobod
                          join b in db.estanterias
                              on a.id_estanteria equals b.id
                          join c in db.area_bodega
                              on b.id_area equals c.areabod_id
                          join d in db.bodega_concesionario
                              on c.id_bodega equals d.id
                          where a.id == ubicacion_repuestobod.id
                          select new
                          {
                              estanteria = b.id,
                              c.areabod_id,
                              d.id
                          }).FirstOrDefault();
            //ViewBag.area_bodega = new SelectList(db.area_bodega.Where(x => x.areabod_estado != false).OrderBy(x => x.areabod_nombre), "areabod_id", "areabod_nombre",ubicacion_repuestobod.idarea);
            int bodega = Convert.ToInt32(Session["user_bodega"]);
            ViewBag.bodegas =
                new SelectList(
                    db.bodega_concesionario.Where(x => x.bodccs_estado && x.id == bodega).OrderBy(x => x.bodccs_nombre),
                    "id", "bodccs_nombre", enlace.id);
            ViewBag.id_area = new SelectList(db.area_bodega.Where(x => x.areabod_estado).OrderBy(x => x.areabod_nombre),
                "areabod_id", "areabod_nombre", enlace.areabod_id);
            ViewBag.estanteria = new SelectList(db.estanterias.Where(x => x.estado).OrderBy(x => x.descripcion), "id",
                "descripcion", enlace.estanteria);

            return View(ubicacion);
        }

        public void ConsultaDatosCreacion(ubicacion_repuestobod ubi_rpto)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(ubi_rpto.ubirptouserid_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = db.users.Find(ubi_rpto.ubirptouserid_actualizacion);
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

        public JsonResult BuscarUbicacionR()
        {
            var datos = (from a in db.ubicacion_repuestobod
                         join b in db.estanterias
                             on a.id_estanteria equals b.id
                         join c in db.area_bodega
                             on b.id_area equals c.areabod_id
                         join d in db.bodega_concesionario
                             on c.id_bodega equals d.id
                         select new
                         {
                             a.id,
                             ubicacion = a.descripcion,
                             estanteria = b.descripcion,
                             area = c.areabod_nombre,
                             bodega = d.bodccs_nombre,
                             estado = a.ubirpto_estado
                         }).ToList();

            System.Collections.Generic.List<modelo_ubicacionrepuestosbod> data = datos.OrderBy(x => x.ubicacion).Select(c => new modelo_ubicacionrepuestosbod
            {
                id = c.id,
                ubicacion = c.ubicacion,
                estanteria = c.estanteria,
                area = c.area,
                bodega = c.bodega,
                estado = c.estado ? "Activo" : "Inactivo"
            }).OrderBy(x => x.ubicacion).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
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