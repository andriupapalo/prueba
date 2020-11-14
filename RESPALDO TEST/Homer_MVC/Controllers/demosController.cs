using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class demosController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: demos/Create
        public ActionResult Create(int? menu)
        {
            ViewBag.planmayor = new SelectList(db.icb_vehiculo, "plan_mayor", "plan_mayor");
            ViewBag.ubicacion = new SelectList(db.ubicacion_vehiculo, "ubivh_id", "ubivh_nombre");
            BuscarFavoritos(menu);
            return View();
        }

        // POST: demos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(vdemos vdemos, int? menu)
        {
            if (ModelState.IsValid)
            {
                vdemos existe = db.vdemos.FirstOrDefault(x => x.placa == vdemos.placa && x.planmayor == vdemos.planmayor);
                if (existe == null)
                {
                    vdemos.fecha_creacion = DateTime.Now;
                    vdemos.user_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    db.vdemos.Add(vdemos);
                    int result = db.SaveChanges();
                    if (result == 1)
                    {
                        TempData["mensaje"] = " Demo creado correctamente";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error al crear el demo, por favor valide los datos";
                    }
                }
                else
                {
                    TempData["mensaje_error"] = "El demo que ingreso ya existe, por favor valide";
                }
            }

            ViewBag.planmayor = new SelectList(db.icb_vehiculo, "plan_mayor", "plan_mayor", vdemos.planmayor);
            ViewBag.ubicacion = new SelectList(db.ubicacion_vehiculo, "ubivh_id", "ubivh_nombre", vdemos.ubicacion);
            BuscarFavoritos(menu);
            return View(vdemos);
        }

        // GET: demos/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            vdemos vdemos = db.vdemos.Find(id);
            if (vdemos == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(vdemos);
            ViewBag.planmayor = new SelectList(db.icb_vehiculo, "plan_mayor", "plan_mayor", vdemos.planmayor);
            ViewBag.ubicacion = new SelectList(db.ubicacion_vehiculo, "ubivh_id", "ubivh_nombre", vdemos.ubicacion);
            BuscarFavoritos(menu);
            return View(vdemos);
        }

        // POST: demos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "planmayor,serie,ubicacion,notas,placa,estado")]
            vdemos vdemos, int? menu)
        {
            if (ModelState.IsValid)
            {
                vdemos.fecha_actualizacion = DateTime.Now;
                vdemos.user_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.Entry(vdemos).State = EntityState.Modified;
                int result = db.SaveChanges();
                if (result == 1)
                {
                    TempData["mensaje"] = " Demo actualizaso correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "Error al actualizar el demo, por favor valide los datos";
                }
            }

            ConsultaDatosCreacion(vdemos);
            ViewBag.planmayor = new SelectList(db.icb_vehiculo, "plan_mayor", "plan_mayor", vdemos.planmayor);
            ViewBag.ubicacion = new SelectList(db.ubicacion_vehiculo, "ubivh_id", "ubivh_nombre", vdemos.ubicacion);
            BuscarFavoritos(menu);
            return View(vdemos);
        }

        public void ConsultaDatosCreacion(vdemos demos)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(demos.user_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = db.users.Find(demos.user_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult BuscarVehiculo(string plan_mayor)
        {
            var data = from v in db.icb_vehiculo
                       where v.plan_mayor == plan_mayor
                       select new
                       {
                           placa = v.plac_vh
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDatos()
        {
            var data = from d in db.vdemos
                       join v in db.icb_vehiculo
                           on d.planmayor equals v.plan_mayor
                       join u in db.ubicacion_vehiculo
                           on d.ubicacion equals u.ubivh_id
                       select new
                       {
                           d.planmayor,
                           d.placa,
                           d.notas,
                           d.id,
                           d.serie,
                           u.ubivh_nombre,
                           estado = d.estado ? "Activo" : "Inactivo"
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