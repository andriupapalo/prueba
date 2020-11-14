using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class ref_grupoController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();


        // GET: ref_grupo/Create
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        // POST: ref_grupo/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ref_grupo ref_grupo, int? menu)
        {
            if (ModelState.IsValid)
            {
                ref_grupo.fec_creacion = DateTime.Now;
                ref_grupo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.ref_grupo.Add(ref_grupo);

                ref_grupo buscarCodigo = db.ref_grupo.FirstOrDefault(x => x.codigo == ref_grupo.codigo);
                if (buscarCodigo == null)
                {
                    ref_subgrupo obj = new ref_subgrupo
                    {
                        cod_grupo = ref_grupo.codigo,
                        codigo = "01",
                        Descripcion = "Sin subgrupo",
                        estado = true,
                        fec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                    };
                    db.ref_subgrupo.Add(obj);

                    db.SaveChanges();
                    TempData["mensaje"] = "La creación del registro fue exitoso";
                }
                else
                {
                    TempData["mensaje_error"] = "El codigo ingresado ya existe, por favor valide";
                }
            }

            BuscarFavoritos(menu);
            return View(ref_grupo);
        }

        // GET: ref_grupo/Edit/5
        public ActionResult Edit(string id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ref_grupo ref_grupo = db.ref_grupo.Find(id);
            if (ref_grupo == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(ref_grupo);
            BuscarFavoritos(menu);
            return View(ref_grupo);
        }

        // POST: ref_grupo/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ref_grupo ref_grupo, int? menu)
        {
            if (ModelState.IsValid)
            {
                ref_grupo.fec_actualizacion = DateTime.Now;
                ref_grupo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.Entry(ref_grupo).State = EntityState.Modified;
                db.SaveChanges();
                TempData["mensaje"] = "La actualización del registro fue exitoso";
            }

            ConsultaDatosCreacion(ref_grupo);
            BuscarFavoritos(menu);
            return View(ref_grupo);
        }

        public void ConsultaDatosCreacion(ref_grupo ref_grupo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(ref_grupo.userid_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = db.users.Find(ref_grupo.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarDatos()
        {
            var data = db.ref_grupo.ToList().Select(x => new
            {
                x.codigo,
                x.Descripcion,
                estado = x.estado ? "Activo" : "Inactivo"
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // GET: ref_grupo/Delete/5
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