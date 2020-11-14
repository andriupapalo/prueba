using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class motivoanulacionesController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();


        // GET: motivoanulaciones/Create
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View(new motivoanulacion { estado = true });
        }

        // POST: motivoanulaciones/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create(motivoanulacion modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                motivoanulacion buscarPorNombre = db.motivoanulacion.FirstOrDefault(x => x.motivo == modelo.motivo);
                if (buscarPorNombre != null)
                {
                    TempData["mensaje_error"] = "El motivo de anulación ingresado ya existe, por favor valide";
                }
                else
                {
                    modelo.fec_creacion = DateTime.Now;
                    modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    db.motivoanulacion.Add(modelo);
                    int guardar = db.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "La creación del motivo de Anulación fue exitoso";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                    }
                }
            }

            BuscarFavoritos(menu);
            return View();
        }


        // GET: motivoanulaciones/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            motivoanulacion motivoanulacion = db.motivoanulacion.Find(id);
            if (motivoanulacion == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(motivoanulacion);
            BuscarFavoritos(menu);
            return View(motivoanulacion);
        }

        // POST: motivoanulaciones/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //  public ActionResult Edit([Bind(Include = "id,motivo,id_licencia,fec_creacion,userid_creacion,fec_actualizacion,user_idactualizacion,estado,razon_inactivo")] motivoanulacion motivoanulacion)
        public ActionResult Edit(motivoanulacion modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                motivoanulacion buscarPorNombre = db.motivoanulacion.FirstOrDefault(x => x.motivo == modelo.motivo);
                if (buscarPorNombre != null)
                {
                    if (buscarPorNombre.id != modelo.id)
                    {
                        TempData["mensaje_error"] = "El motivo de Anulación ingresado ya existe, por favor valide";
                    }
                    else
                    {
                        modelo.fec_actualizacion = DateTime.Now;
                        modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarPorNombre.fec_actualizacion = DateTime.Now;
                        buscarPorNombre.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarPorNombre.estado = modelo.estado;
                        buscarPorNombre.razon_inactivo = modelo.razon_inactivo;
                        db.Entry(buscarPorNombre).State = EntityState.Modified;
                        int guardar = db.SaveChanges();
                        if (guardar > 0)
                        {
                            TempData["mensaje"] = "La actualización del motivo de anulación fue exitoso";
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                        }
                    }
                }
                else
                {
                    modelo.fec_actualizacion = DateTime.Now;
                    modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    db.Entry(modelo).State = EntityState.Modified;
                    int guardar = db.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "La actualización del motivo de anulación fue exitoso";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                    }
                }
            }

            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public void ConsultaDatosCreacion(motivoanulacion motivoanulacion)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(motivoanulacion.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = db.users.Find(motivoanulacion.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        //public JsonResult BuscarMotivosCancelacion()
        public JsonResult BuscarMotivosAnulacion()
        {
            var buscarMotivos = (from motivos in db.motivoanulacion
                                 select new
                                 {
                                     motivos.id,
                                     motivos.motivo,
                                     estado = motivos.estado ? "Activo" : "Inactivo"
                                 }).ToList();

            return Json(buscarMotivos, JsonRequestBehavior.AllowGet);
        }


        //// GET: motivoanulaciones/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    motivoanulacion motivoanulacion = db.motivoanulacion.Find(id);
        //    if (motivoanulacion == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(motivoanulacion);
        //}


        //// POST: motivoanulaciones/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    motivoanulacion motivoanulacion = db.motivoanulacion.Find(id);
        //    db.motivoanulacion.Remove(motivoanulacion);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}


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


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}