using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tramitador_vhController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public ActionResult Create(int? menu)
        {
            tramitador_vh createTram = new tramitador_vh
            {
                tramitador_estado = true
            };
            IOrderedQueryable<tptramitador_vh> tiposEncontrados = context.tptramitador_vh.OrderBy(x => x.tptramivh_nombre);
            List<SelectListItem> tipos = new List<SelectListItem>();
            foreach (tptramitador_vh item in tiposEncontrados)
            {
                tipos.Add(new SelectListItem { Text = item.tptramivh_nombre, Value = item.tptramivh_id.ToString() });
            }

            ViewBag.tipoTramitador = new SelectList(tipos);
            ViewBag.tercero = new SelectList(context.icb_terceros, "tercero_id", "doc_tercero");

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 72);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(createTram);
        }

        // POST: tramitador/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(tramitador_vh tramitador, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.tramitador_vh
                           where a.tramitador_documento == tramitador.tramitador_documento
                           select a.tramitador_documento).Count();

                if (nom == 0)
                {
                    icb_terceros tercero =
                        context.icb_terceros.FirstOrDefault(x => x.doc_tercero == tramitador.tramitador_documento);
                    int idTercero = tercero.tercero_id;

                    tramitador.tramitador_idtercero = idTercero;
                    tramitador.tramitadorfec_creacion = DateTime.Now;
                    tramitador.tramitadoruserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.tramitador_vh.Add(tramitador);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro del tramitador fue exitoso!";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El numero de documento ingreso ya se encuentra, por favor valide!";
            }

            IOrderedQueryable<tptramitador_vh> tiposEncontrados = context.tptramitador_vh.OrderBy(x => x.tptramivh_nombre);
            List<SelectListItem> tipos = new List<SelectListItem>();
            foreach (tptramitador_vh item in tiposEncontrados)
            {
                tipos.Add(new SelectListItem { Text = item.tptramivh_nombre, Value = item.tptramivh_id.ToString() });
            }

            ViewBag.tipoTramitador = new SelectList(tipos);

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 72);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(tramitador);
        }

        // GET: tramitador/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tramitador_vh tramitador = context.tramitador_vh.Find(id);
            if (tramitador == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tramitador.tramitadoruserid_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(tramitador.tramitadoruserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }

            IOrderedQueryable<tptramitador_vh> tiposEncontrados = context.tptramitador_vh.OrderBy(x => x.tptramivh_nombre);
            List<SelectListItem> tipos = new List<SelectListItem>();
            foreach (tptramitador_vh item in tiposEncontrados)
            {
                tipos.Add(new SelectListItem { Text = item.tptramivh_nombre, Value = item.tptramivh_id.ToString() });
            }

            ViewBag.tipoTramitador = new SelectList(tipos);

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 72);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(tramitador);
        }

        // POST: tramitador/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(tramitador_vh tramitador, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.tramitador_vh
                           where a.tramitador_documento == tramitador.tramitador_documento ||
                                 a.tramitador_id == tramitador.tramitador_id
                           select a.tramitador_id).Count();

                if (nom == 1)
                {
                    tramitador.tramitadorfec_actualizacion = DateTime.Now;
                    tramitador.tramitadoruserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);

                    context.Entry(tramitador).State = EntityState.Modified;
                    context.SaveChanges();
                    ConsultaDatosCreacion(tramitador);
                    TempData["mensaje"] = "La actualización del tramitador fue exitosa!";
                    IOrderedQueryable<tptramitador_vh> tiposTramitadores = context.tptramitador_vh.OrderBy(x => x.tptramivh_nombre);
                    List<SelectListItem> tiposTram = new List<SelectListItem>();
                    foreach (tptramitador_vh item in tiposTramitadores)
                    {
                        tiposTram.Add(new SelectListItem
                        { Text = item.tptramivh_nombre, Value = item.tptramivh_id.ToString() });
                    }

                    ViewBag.tipoTramitador = new SelectList(tiposTram);
                    BuscarFavoritos(menu);
                    return View(tramitador);
                }

                TempData["mensaje_error"] = "El documento que ingreso ya se encuentra, por favor valide!";
            }

            IOrderedQueryable<tptramitador_vh> tiposEncontrados = context.tptramitador_vh.OrderBy(x => x.tptramivh_nombre);
            List<SelectListItem> tipos = new List<SelectListItem>();
            foreach (tptramitador_vh item in tiposEncontrados)
            {
                tipos.Add(new SelectListItem { Text = item.tptramivh_nombre, Value = item.tptramivh_id.ToString() });
            }

            ViewBag.tipoTramitador = new SelectList(tipos);
            ConsultaDatosCreacion(tramitador);

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 72);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(tramitador);
        }

        public void ConsultaDatosCreacion(tramitador_vh tramitador_vh)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tramitador_vh.tramitadoruserid_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = context.users.Find(tramitador_vh.tramitadoruserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult BuscarTramitadoresPaginadas()
        {
            var data = from tramitador in context.tramitador_vh
                       join tipo in context.tptramitador_vh
                           on tramitador.tramitador_idtipo equals tipo.tptramivh_id
                       select new
                       {
                           tramitador.tramitador_id,
                           tramitador.tramitador_documento,
                           tramitador.tramitadorpri_nombre,
                           tramitadorseg_nombre =
                               tramitador.tramitadorseg_nombre == null ? "" : tramitador.tramitadorseg_nombre,
                           tramitador.tramitador_apellidos,
                           tramitador.tramitador_celular,
                           tramitador_estado = tramitador.tramitador_estado ? "Activo" : "Inactivo",
                           tipo.tptramivh_nombre
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarTerceroPorDocumento(string documento)
        {
            icb_terceros buscarCliente = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == documento);

            if (buscarCliente != null)
            {
                var data = new
                {
                    clienteExiste = true,
                    buscarCliente.doc_tercero,
                    buscarCliente.prinom_tercero,
                    buscarCliente.segnom_tercero,
                    buscarCliente.apellido_tercero,
                    buscarCliente.segapellido_tercero,
                    buscarCliente.celular_tercero
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(new { clienteExiste = false }, JsonRequestBehavior.AllowGet);
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