using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class vdocumentosflotaController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: vdocumentosflota
        public ActionResult Browser(int? menu)
        {
            BuscarFavoritos(menu);
            return View(db.vdocumentosflota.ToList());
        }

        // GET: vdocumentosflota/Create
        public ActionResult Create(int? menu)
        {
            var listaDocu = (from td in db.tipo_documentos
                             select new
                             {
                                 td.id,
                                 nombre = td.tipo_nombre
                             }).ToList();
            // ViewBag.tercero_id = new SelectList(terceros, "id", "docyNombre");
            ViewBag.tdocumento = new SelectList(listaDocu, "id", "nombre");
            ViewBag.docFlotas =
                new SelectList(db.documento_facturacion.Where(x => x.docfac_estado).OrderBy(x => x.docfac_nombre),
                    "docfac_id", "docfac_nombre");
            BuscarFavoritos(menu);
            return View();
        }

        // POST: vdocumentosflota/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(vdocumentosflota vdocumentosflota, int? menu)
        {
            if (ModelState.IsValid)
            {
                bool coincidencia = false;
                string documentosT = Request["tdocumento"];
                string[] documentos1 = documentosT.Split(',');
                foreach (string item in documentos1)
                {
                    vdocumentosflota existe = db.vdocumentosflota.FirstOrDefault(x =>
                        x.documento == vdocumentosflota.documento && x.id_tipo_documento.ToString() == item);
                    if (existe != null)
                    {
                        coincidencia = true;
                    }

                    break;
                }

                if (coincidencia == false)
                {
                    string docs = Request["tdocumento"];
                    string[] documentos = docs.Split(',');
                    foreach (string item in documentos)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            vdocumentosflota.fec_creacion = DateTime.Now;
                            vdocumentosflota.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                            vdocumentosflota.id_tipo_documento = Convert.ToInt32(item);
                            vdocumentosflota.estado = true;
                            vdocumentosflota.estadotipo = true;
                            db.vdocumentosflota.Add(vdocumentosflota);
                            db.SaveChanges();
                        }
                    }

                    // db.SaveChanges();
                    TempData["mensaje"] = "Documento creado correctamente";
                    //return RedirectToAction("Edit", new { id= vdocumentosflota.id, menu});
                    // ViewBag.tercero_id = new SelectList(terceros, "id", "docyNombre");
                    ViewBag.tdocumento = new SelectList(db.documento_facturacion, "id", "tipo_nombre");
                    ViewBag.docFlotas =
                        new SelectList(
                            db.documento_facturacion.Where(x => x.docfac_estado).OrderBy(x => x.docfac_nombre),
                            "docfac_id", "docfac_nombre");
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El documento ingresado ya existe, por favor valide";
            }
            else
            {
                TempData["mensaje_error"] = "Error al guardar el documento, por favor valide";
            }

            var listaDocu = (from td in db.tipo_documentos
                             select new
                             {
                                 td.id,
                                 nombre = td.tipo_nombre
                             }).ToList();
            // ViewBag.tercero_id = new SelectList(terceros, "id", "docyNombre");
            ViewBag.tdocumento = new SelectList(listaDocu, "id", "nombre");
            ViewBag.docFlotas =
                new SelectList(db.documento_facturacion.Where(x => x.docfac_estado).OrderBy(x => x.docfac_nombre),
                    "docfac_id", "docfac_nombre");
            BuscarFavoritos(menu);
            return View(vdocumentosflota);
        }

        // GET: vdocumentosflota/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            System.Collections.Generic.List<vdocumentosflota> vdocumentosflota = db.vdocumentosflota.Where(x => x.iddocumento == id).ToList();
            if (vdocumentosflota.Count() == 0)
            {
                return HttpNotFound();
            }

            var listaDocu = (from td in db.tipo_documentos
                             select new
                             {
                                 td.id,
                                 nombre = td.tipo_nombre
                             }).ToList();

            System.Collections.Generic.IEnumerable<int> numerodocumentos = vdocumentosflota.Select(d => d.id_tipo_documento);
            // ViewBag.tercero_id = new SelectList(terceros, "id", "docyNombre");
            ViewBag.tdocumento = new MultiSelectList(listaDocu, "id", "nombre", numerodocumentos);
            ViewBag.docFlotas =
                new SelectList(db.documento_facturacion.Where(x => x.docfac_estado).OrderBy(x => x.docfac_nombre),
                    "docfac_id", "docfac_nombre", vdocumentosflota.First().iddocumento);

            vdocumentosflotaform docu = new vdocumentosflotaform
            {
                iddocumento = vdocumentosflota.First().iddocumento.Value,
                documento = vdocumentosflota.First().documento,
                estado = vdocumentosflota.First().estado,
                id_tipo_documento = vdocumentosflota.First().id_tipo_documento,
                razon_inactivo = vdocumentosflota.First().razon_inactivo,
                id_licencia = vdocumentosflota.First().id_licencia,
                fec_actualizacion = vdocumentosflota.First().fec_actualizacion,
                fec_creacion = vdocumentosflota.First().fec_creacion,
                userid_creacion = vdocumentosflota.First().userid_creacion,
                user_idactualizacion = vdocumentosflota.First().user_idactualizacion
            };
            ConsultaDatosCreacion(docu);
            BuscarFavoritos(menu);
            return View(docu);
        }

        // POST: vdocumentosflota/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(vdocumentosflotaform vdocumentosflota, int? menu)
        {
            if (ModelState.IsValid)
            {
                //borro todos los registros que tengan el id que viene seleccionado
                System.Collections.Generic.List<int> buscarBorrar = db.vdocumentosflota.Where(x => x.iddocumento == vdocumentosflota.iddocumento)
                    .Select(x => x.id).ToList();
                foreach (int item in buscarBorrar)
                {
                    vdocumentosflota dato = db.vdocumentosflota.Find(item);
                    vdocumentosflota documento = db.vdocumentosflota.FirstOrDefault(x => x.id == item);
                    documento.estadotipo = false;
                    db.Entry(documento).State = EntityState.Modified;
                    db.SaveChanges();
                }

                string docs = Request["tdocumento"];
                string[] documentos = docs.Split(',');
                foreach (string item in documentos)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        vdocumentosflota existe = db.vdocumentosflota.FirstOrDefault(x =>
                            x.documento == vdocumentosflota.documento && x.id_tipo_documento.ToString() == item);
                        if (existe != null)
                        {
                            existe.fec_actualizacion = DateTime.Now;
                            existe.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                            existe.id_tipo_documento = Convert.ToInt32(item);
                            existe.estado = true;
                            existe.estadotipo = true;
                            db.Entry(existe).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            vdocumentosflota docFlota = new vdocumentosflota
                            {
                                documento = vdocumentosflota.documento,
                                fec_creacion = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                id_tipo_documento = Convert.ToInt32(item),
                                iddocumento = vdocumentosflota.iddocumento,
                                estado = true,
                                estadotipo = true
                            };
                            db.vdocumentosflota.Add(docFlota);
                            db.SaveChanges();
                        }
                    }
                }

                ConsultaDatosCreacion(vdocumentosflota);
            }

            BuscarFavoritos(menu);

            System.Collections.Generic.List<vdocumentosflota> asd = db.vdocumentosflota.Where(x => x.iddocumento == vdocumentosflota.iddocumento).ToList();
            var listaDocu = (from td in db.tipo_documentos
                             select new
                             {
                                 td.id,
                                 nombre = td.tipo_nombre
                             }).ToList();

            System.Collections.Generic.IEnumerable<int> numerodocumentos = asd.Select(d => d.id_tipo_documento);
            // ViewBag.tercero_id = new SelectList(terceros, "id", "docyNombre");
            ViewBag.tdocumento = new MultiSelectList(listaDocu, "id", "nombre", numerodocumentos);
            ViewBag.docFlotas =
                new SelectList(db.documento_facturacion.Where(x => x.docfac_estado).OrderBy(x => x.docfac_nombre),
                    "docfac_id", "docfac_nombre", asd.First().iddocumento);

            return View(vdocumentosflota);
        }

        public void ConsultaDatosCreacion(vdocumentosflotaform vdocumentosflota)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(vdocumentosflota.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = db.users.Find(vdocumentosflota.user_idactualizacion);
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

        public JsonResult BuscarDocFlotas()
        {
            var data = (from modelo in db.vdocumentosflota
                        join tip in db.tipo_documentos
                            on modelo.id_tipo_documento equals tip.id
                        select new
                        {
                            modelo.id,
                            modelo.documento,
                            estado = modelo.estado ? "Activo" : "Inactivo",
                            tipo = tip.tipo_nombre,
                            modelo.iddocumento
                        }).ToList();
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