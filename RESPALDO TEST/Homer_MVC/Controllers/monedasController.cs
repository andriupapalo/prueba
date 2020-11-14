using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class monedasController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: monedas/Create
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        // POST: monedas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include =
                "moneda,descripcion,id_licencia,fec_creacion,userid_creacion,fec_actualizacion,user_idactualizacion,estado,razon_inactivo")]
            monedas monedas, int? menu)
        {
            if (ModelState.IsValid)
            {
                monedas.fec_creacion = DateTime.Now;
                monedas.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.monedas.Add(monedas);

                monedas buscarDato = db.monedas.FirstOrDefault(x => x.descripcion == monedas.descripcion);
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
            return View(monedas);
        }

        // GET: monedas/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            monedas monedas = db.monedas.Find(id);
            if (monedas == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(monedas);
            BuscarFavoritos(menu);
            return View(monedas);
        }

        // POST: monedas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include =
                "moneda,descripcion,id_licencia,fec_creacion,userid_creacion,fec_actualizacion,user_idactualizacion,estado,razon_inactivo")]
            monedas monedas, int? menu)
        {
            if (ModelState.IsValid)
            {
                monedas.fec_actualizacion = DateTime.Now;
                monedas.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.Entry(monedas).State = EntityState.Modified;
                db.SaveChanges();
                TempData["mensaje"] = "La actualización del registro fue exitoso";
            }

            ConsultaDatosCreacion(monedas);
            BuscarFavoritos(menu);
            return View(monedas);
        }

        public void ConsultaDatosCreacion(monedas moneda)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(moneda.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = db.users.Find(moneda.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult BuscarDatos()
        {
            var data = db.monedas.ToList().Select(x => new
            {
                x.descripcion,
                x.moneda,
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