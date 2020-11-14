using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class naturalezaDocumentoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public void ParametrosVista()
        {
            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 264).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 264);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
        }


        // GET: tipoModelo
        public ActionResult Crear(int? menu)
        {
            tp_doc_sw crearSw = new tp_doc_sw
            {
                estado = true,
                razon_inactivo = "No aplica"
            };
            ParametrosVista();
            BuscarFavoritos(menu);
            return View(crearSw);
        }


        // POST: col_vh/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(tp_doc_sw tpDocSw, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.tp_doc_sw
                           where a.sw == tpDocSw.sw || a.Descripcion == tpDocSw.Descripcion
                           select a.Descripcion).Count();

                if (nom == 0)
                {
                    tpDocSw.fec_creacion = DateTime.Now;
                    tpDocSw.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.tp_doc_sw.Add(tpDocSw);

                    context.SaveChanges();

                    TempData["mensaje"] = "El registro de la nueva naturaleza de documento fue exitoso!";
                    BuscarFavoritos(menu);
                    return RedirectToAction("Crear");
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";
            ParametrosVista();
            BuscarFavoritos(menu);
            return View(tpDocSw);
        }


        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tp_doc_sw tpDocSw = context.tp_doc_sw.Find(id);
            if (tpDocSw == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tpDocSw.userid_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(tpDocSw.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }

            ParametrosVista();
            BuscarFavoritos(menu);
            return View(tpDocSw);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(tp_doc_sw tpDocSw, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta

                int nom = (from a in context.tp_doc_sw
                           where a.sw == tpDocSw.sw || a.Descripcion == tpDocSw.Descripcion
                           select a.Descripcion).Count();

                if (nom == 1)
                {
                    tpDocSw.fec_actualizacion = DateTime.Now;
                    tpDocSw.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(tpDocSw).State = EntityState.Modified;
                    context.SaveChanges();
                    ConsultaDatosCreacion(tpDocSw);
                    TempData["mensaje"] = "La actualización de la naturaleza de documento fue exitoso!";
                    BuscarFavoritos(menu);
                    return View(tpDocSw);
                }

                {
                    int nom2 = (from a in context.tp_doc_sw
                                where a.Descripcion == tpDocSw.Descripcion
                                select a.Descripcion).Count();
                    if (nom2 == 0)
                    {
                        tpDocSw.fec_actualizacion = DateTime.Now;
                        tpDocSw.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        context.Entry(tpDocSw).State = EntityState.Modified;
                        context.SaveChanges();
                        ConsultaDatosCreacion(tpDocSw);
                        TempData["mensaje"] = "La actualización de la naturaleza de documento fue exitoso!";
                        BuscarFavoritos(menu);
                        return View(tpDocSw);
                    }

                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                }
            }

            ConsultaDatosCreacion(tpDocSw);
            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";
            ParametrosVista();
            BuscarFavoritos(menu);
            return View(tpDocSw);
        }


        public void ConsultaDatosCreacion(tp_doc_sw tpDocSw)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tpDocSw.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(tpDocSw.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult GetTiposJson()
        {
            var buscarTipos = context.tp_doc_sw.ToList().Select(x => new
            {
                x.sw,
                x.tpdoc_id,
                x.Descripcion,
                estado = x.estado ? "Activo" : "Inactivo"
            });
            return Json(buscarTipos, JsonRequestBehavior.AllowGet);
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