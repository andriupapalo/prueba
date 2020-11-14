using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class ref_subgrupoController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: ref_subgrupo/Create
        public ActionResult Create(int? menu)
        {
            ViewBag.cod_grupo = new SelectList(db.ref_grupo, "codigo", "Descripcion");
            BuscarFavoritos(menu);
            return View();
        }

        // POST: ref_subgrupo/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ref_subgrupo ref_subgrupo, int? menu)
        {
            if (ModelState.IsValid)
            {
                ref_subgrupo.fec_creacion = DateTime.Now;
                ref_subgrupo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                ;
                db.ref_subgrupo.Add(ref_subgrupo);

                ref_subgrupo buscarCodigo = db.ref_subgrupo.FirstOrDefault(x =>
                    x.codigo == ref_subgrupo.codigo && x.cod_grupo == ref_subgrupo.cod_grupo);
                if (buscarCodigo == null)
                {
                    db.SaveChanges();
                    TempData["mensaje"] = "La creación del registro fue exitoso";
                }
                else
                {
                    TempData["mensaje_error"] = "El registro ingresado ya existe, por favor valide";
                }
            }

            ViewBag.cod_grupo = new SelectList(db.ref_grupo, "codigo", "Descripcion", ref_subgrupo.cod_grupo);
            BuscarFavoritos(menu);
            return View(ref_subgrupo);
        }

        // GET: ref_subgrupo/Edit/5
        public ActionResult Edit(string cd, string g, int? menu)
        {
            if (cd == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ref_subgrupo ref_subgrupo = db.ref_subgrupo.FirstOrDefault(x => x.codigo == cd && x.cod_grupo == g);
            if (ref_subgrupo == null)
            {
                return HttpNotFound();
            }

            ViewBag.cod_grupo = new SelectList(db.ref_grupo, "codigo", "Descripcion", ref_subgrupo.cod_grupo);
            ConsultaDatosCreacion(ref_subgrupo);
            BuscarFavoritos(menu);
            return View(ref_subgrupo);
        }

        // POST: ref_subgrupo/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ref_subgrupo ref_subgrupo, int? menu)
        {
            if (ModelState.IsValid)
            {
                ref_subgrupo.fec_actualizacion = DateTime.Now;
                ref_subgrupo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.Entry(ref_subgrupo).State = EntityState.Modified;
                db.SaveChanges();
                TempData["mensaje"] = "La actualización del registro fue exitoso";
            }

            ViewBag.cod_grupo = new SelectList(db.ref_grupo, "codigo", "Descripcion", ref_subgrupo.cod_grupo);
            ConsultaDatosCreacion(ref_subgrupo);
            BuscarFavoritos(menu);
            return View(ref_subgrupo);
        }

        // GET: ref_subgrupo/Delete/5
        public void ConsultaDatosCreacion(ref_subgrupo ref_subgrupo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(ref_subgrupo.userid_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = db.users.Find(ref_subgrupo.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult BuscarDatos()
        {
            var data = from sg in db.ref_subgrupo
                       join g in db.ref_grupo
                           on sg.cod_grupo equals g.codigo
                       select new
                       {
                           sg.cod_grupo,
                           nom_grupo = g.Descripcion,
                           sg.Descripcion,
                           sg.codigo,
                           estado = g.estado ? "Activo" : "Inactivo"
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