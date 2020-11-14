using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tipoproveedorController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: tipoproveedor/Create
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }


        // POST: tipoproveedor/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include =
                "tipo,nombre,fec_creacion,usuario_creacion,fec_actualizacion,estado,razoninactivo,usuario_actualizacion,licencia_id")]
            tipoproveedor tipoproveedor, int? menu)
        {
            if (ModelState.IsValid)
            {
                tipoproveedor.fec_creacion = DateTime.Now;
                tipoproveedor.usuario_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.tipoproveedor.Add(tipoproveedor);

                tipocliente buscarDato = db.tipocliente.FirstOrDefault(x => x.nombre == tipoproveedor.nombre);
                if (buscarDato == null)
                {
                    try
                    {
                        db.SaveChanges();
                        TempData["mensaje"] = "La creación del registro fue exitoso";
                    }
                    catch (DbEntityValidationException dbEx)
                    {
                        Exception raise = dbEx;
                        foreach (DbEntityValidationResult validationErrors in dbEx.EntityValidationErrors)
                        {
                            foreach (DbValidationError validationError in validationErrors.ValidationErrors)
                            {
                                string message = string.Format("{0}:{1}",
                                    validationErrors.Entry.Entity,
                                    validationError.ErrorMessage);
                                // raise a new exception nesting
                                // the current instance as InnerException
                                raise = new InvalidOperationException(message, raise);
                            }
                        }

                        throw raise;
                    }
                }
                else
                {
                    TempData["mensaje_error"] = "El registro ingresado ya existe, por favor valide";
                }
            }

            BuscarFavoritos(menu);
            return View(tipoproveedor);
        }

        // GET: tipoproveedor/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tipoproveedor tipoproveedor = db.tipoproveedor.Find(id);
            if (tipoproveedor == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(tipoproveedor);
            BuscarFavoritos(menu);
            return View(tipoproveedor);
        }

        // POST: tipoproveedor/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include =
                "tipo,nombre,fec_creacion,usuario_creacion,fec_actualizacion,estado,razoninactivo,usuario_actualizacion,licencia_id")]
            tipoproveedor tipoproveedor, int? menu)
        {
            if (ModelState.IsValid)
            {
                tipoproveedor.fec_actualizacion = DateTime.Now;
                tipoproveedor.usuario_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.Entry(tipoproveedor).State = EntityState.Modified;
                db.SaveChanges();
                ConsultaDatosCreacion(tipoproveedor);
            }

            BuscarFavoritos(menu);
            return View(tipoproveedor);
        }

        public void ConsultaDatosCreacion(tipoproveedor tipoproveedor)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(tipoproveedor.usuario_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = db.users.Find(tipoproveedor.usuario_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult BuscarDatos()
        {
            var data = db.tipoproveedor.ToList().Select(x => new
            {
                x.tipo,
                x.nombre,
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