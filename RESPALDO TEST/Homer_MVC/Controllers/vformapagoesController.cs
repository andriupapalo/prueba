using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class vformapagoesController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: vformapagoes/Create
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        // POST: vformapagoes/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(vformapago vformapago, int? menu)
        {
            if (ModelState.IsValid)
            {
                vformapago fpago = db.vformapago.FirstOrDefault(x => x.descripcion == vformapago.descripcion);

                if (fpago == null)
                {
                    vformapago.fec_creacion = DateTime.Now;
                    vformapago.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    db.vformapago.Add(vformapago);
                    db.SaveChanges();
                    TempData["mensaje"] = "Registro Creado Correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "La condición de pago ingresada ya existe, por favor valide";
                }

                return View();
            }

            BuscarFavoritos(menu);
            return View(vformapago);
        }

        // GET: vformapagoes/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            vformapago vformapago = db.vformapago.Find(id);
            if (vformapago == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(vformapago);
            BuscarFavoritos(menu);
            return View(vformapago);
        }

        // POST: vformapagoes/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(vformapago vformapago, int? menu)
        {
            if (ModelState.IsValid)
            {
                vformapago.fec_actualizacion = DateTime.Now;
                vformapago.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.Entry(vformapago).State = EntityState.Modified;
                db.SaveChanges();
                TempData["mensaje"] = "Registro Editado Correctamente";
            }

            ConsultaDatosCreacion(vformapago);
            BuscarFavoritos(menu);
            return View(vformapago);
        }

        // GET: vformapagoes/Details/5
        public ActionResult Browser(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public void ConsultaDatosCreacion(vformapago vformapago)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(vformapago.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = db.users.Find(vformapago.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult BuscarDatos()
        {
            var data = db.vformapago.ToList().Select(x => new
            {
                x.descripcion,
                estado = x.estado ? "Activo" : "Inactivo",
                x.id
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //// GET: vformapagoes/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    vformapago vformapago = db.vformapago.Find(id);
        //    if (vformapago == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(vformapago);
        //}

        //// POST: vformapagoes/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    vformapago vformapago = db.vformapago.Find(id);
        //    db.vformapago.Remove(vformapago);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

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