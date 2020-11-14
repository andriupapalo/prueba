using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class vdocumentosCreditoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: vdocumentosCredito
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            ViewBag.id_menu = menu;
            DocumentosCreditoModel model = new DocumentosCreditoModel
            {
                obligatorio = false,
                estado = true,
                razon_inactivo = ""
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(DocumentosCreditoModel doc, int? menu)
        {
            if (ModelState.IsValid)
            {
                vdocumentoscredito buscarNombre =
                    context.vdocumentoscredito.FirstOrDefault(x => x.nombre == doc.nombre);
                if (buscarNombre == null)
                {
                    vdocumentoscredito guardardoc = new vdocumentoscredito
                    {
                        nombre = doc.nombre,
                        obligatorio = doc.obligatorio,
                        estado = doc.estado,
                        razon_inactivo = doc.razon_inactivo
                    };
                    guardardoc.fec_creacion = DateTime.Now;
                    guardardoc.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.vdocumentoscredito.Add(guardardoc);
                    context.SaveChanges();
                    ViewBag.id_menu = menu;
                    TempData["mensaje"] = "El dato ingresado se ha creado correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "El dato ingresado ya existe";
                    BuscarFavoritos(menu);
                    return View(doc);
                }
            }

            return View();
        }

        public ActionResult Update(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            vdocumentoscredito doc2 = context.vdocumentoscredito.Find(id);
            if (doc2 == null)
            {
                return HttpNotFound();
            }

            DocumentosCreditoModel doc = new DocumentosCreditoModel
            {
                nombre = doc2.nombre,
                id = doc2.id,
                razon_inactivo = doc2.razon_inactivo,
                estado = doc2.estado,
                obligatorio = doc2.obligatorio,
                fec_creacion = doc2.fec_creacion.Value
            };
            IQueryable<string> userCre = from a in context.users
                                         join b in context.vdocumentoscredito on a.user_id equals b.userid_creacion
                                         where b.id == id
                                         select a.user_nombre;
            if (doc2.fec_actualizacion != null)
            {
                doc.fec_actualizacion = doc2.fec_actualizacion.Value;
            }
            foreach (string userCreacion in userCre)
            {
                ViewBag.user_nombre_cre = userCreacion;
            }

            IQueryable<string> userAct = from a in context.users
                                         join b in context.vdocumentoscredito on a.user_id equals b.userid_actualizacion
                                         where b.id == id
                                         select a.user_nombre;

            foreach (string userActualizacion in userAct)
            {
                ViewBag.user_nombre_act = userActualizacion;
            }

            BuscarFavoritos(menu);
            return View(doc);
        }

        [HttpPost]
        public ActionResult Update(DocumentosCreditoModel doc, int? menu)
        {
            if (ModelState.IsValid)
            {
                int nom = (from a in context.vdocumentoscredito
                           where a.nombre == doc.nombre || a.id == doc.id
                           select a.nombre).Count();
                if (nom == 1)
                {
                    vdocumentoscredito doc2 = context.vdocumentoscredito.Find(doc.id);
                    doc2.nombre = doc.nombre;
                    doc2.estado = doc.estado;
                    doc2.obligatorio = doc.obligatorio;
                    doc2.razon_inactivo = doc.razon_inactivo;
                    doc2.fec_actualizacion = DateTime.Now;
                    doc2.userid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(doc2).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización de la averia fue exitosa!";
                    BuscarFavoritos(menu);
                    IQueryable<string> userCre = from a in context.users
                                                 join b in context.vdocumentoscredito on a.user_id equals b.userid_creacion
                                                 where b.id == doc.id
                                                 select a.user_nombre;

                    foreach (string userCreacion in userCre)
                    {
                        ViewBag.user_nombre_cre = userCreacion;
                    }

                    IQueryable<string> userAct = from a in context.users
                                                 join b in context.vdocumentoscredito on a.user_id equals b.userid_actualizacion
                                                 where b.id == doc.id
                                                 select a.user_nombre;

                    foreach (string userActualizacion in userAct)
                    {
                        ViewBag.user_nombre_act = userActualizacion;
                    }

                    return View(doc);
                }

                TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
            }
            BuscarFavoritos(menu);
            return View(doc);
        }

        public ActionResult BuscarDocumentosPaginados()
        {
            var data = context.vdocumentoscredito.ToList().Select(x => new
            {
                x.id,
                x.nombre,
                estado = x.estado ? "Activo" : "Inactivo",
                obligatorio = x.obligatorio ? "Si" : "No",
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public void BuscarFavoritos(int? menu)
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

            var buscarFavoritosSeleccionados = (from favoritos in context.favoritos
                                                join menu2 in context.Menus
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