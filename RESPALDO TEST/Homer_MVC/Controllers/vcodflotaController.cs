using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class vcodflotaController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: vcodflota
        public ActionResult Browser(int? menu)
        {
            IQueryable<vcodflota> vcodflota = db.vcodflota.Include(v => v.users).Include(v => v.users1);
            BuscarFavoritos(menu);
            return View(vcodflota.ToList());
        }


        // GET: vcodflota/Create
        public ActionResult Create(int? menu)
        {
            ViewBag.documentos = db.vdocumentosflota.Where(x => x.estado).ToList();
            BuscarFavoritos(menu);
            return View();
        }

        // POST: vcodflota/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(vcodflota vcodflota, int? menu)
        {
            if (ModelState.IsValid)
            {
                vcodflota existe = db.vcodflota.FirstOrDefault(x => x.codigo == vcodflota.codigo);
                if (existe == null)
                {
                    vcodflota.fec_creacion = DateTime.Now;
                    vcodflota.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    db.vcodflota.Add(vcodflota);
                    db.SaveChanges();

                    int idflota = db.vcodflota.OrderByDescending(x => x.id).FirstOrDefault().id;

                    // documentos
                    string docs = Request["documentos"];
                    string[] documentos = docs.Split(',');
                    foreach (string item in documentos)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            vdocrequeridosflota doc = new vdocrequeridosflota
                            {
                                iddocumento = Convert.ToInt32(item),
                                codflota = idflota,
                                fec_creacion = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                estado = true
                            };
                            db.vdocrequeridosflota.Add(doc);
                        }
                    }

                    db.SaveChanges();

                    TempData["mensaje"] = "Flota creada correctamente";
                    return RedirectToAction("Edit", new { vcodflota.id, menu });
                }

                TempData["mensaje_error"] = "La flota ingresa ya existe, por favor valide";
            }
            else
            {
                TempData["mensaje_error"] = "Error al crear la flota, por favor valide";
            }

            ViewBag.documentos = db.vdocumentosflota.Where(x => x.estado).ToList();
            BuscarFavoritos(menu);
            return View(vcodflota);
        }

        // GET: vcodflota/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            vcodflota vcodflota = db.vcodflota.Find(id);
            if (vcodflota == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(vcodflota);
            ViewBag.documentos = db.vdocumentosflota.Where(x => x.estado).ToList();
            BuscarFavoritos(menu);
            return View(vcodflota);
        }

        // POST: vcodflota/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(vcodflota vcodflota, int? menu)
        {
            if (ModelState.IsValid)
            {
                vcodflota.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                vcodflota.fec_actualizacion = DateTime.Now;
                db.Entry(vcodflota).State = EntityState.Modified;

                //eliminar documentos actuales 
                System.Collections.Generic.List<vdocrequeridosflota> documentos_db = db.vdocrequeridosflota.Where(x => x.codflota == vcodflota.id).ToList();
                foreach (vdocrequeridosflota item in documentos_db)
                {
                    db.Entry(item).State = EntityState.Deleted;
                }

                // insertar nuevamente los documentos
                string documentos_post = Request["documentos"];
                if (!string.IsNullOrEmpty(documentos_post))
                {
                    string[] documentos = documentos_post.Split(',');

                    foreach (string item in documentos)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            vdocrequeridosflota doc = new vdocrequeridosflota
                            {
                                iddocumento = Convert.ToInt32(item),
                                codflota = vcodflota.id,
                                fec_creacion = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                            };
                            db.vdocrequeridosflota.Add(doc);
                        }
                    }
                }


                db.SaveChanges();
                TempData["mansaje"] = "Flota actualizada correctamente";
            }

            ConsultaDatosCreacion(vcodflota);
            ViewBag.documentos = db.vdocumentosflota.Where(x => x.estado).ToList();
            BuscarFavoritos(menu);
            return View(vcodflota);
        }

        public void ConsultaDatosCreacion(vcodflota vcodflota)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(vcodflota.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = db.users.Find(vcodflota.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarDatos()
        {
            var data = from f in db.vcodflota
                       select new
                       {
                           f.codigo,
                           detalle = f.Descripcion.ToString(),
                           estado = f.estado ? "Activo" : "Inactivo",
                           f.id
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