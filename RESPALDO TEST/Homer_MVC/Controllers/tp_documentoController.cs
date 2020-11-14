﻿using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tp_documentoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        // GET: tp_documento/Create
        public ActionResult Create(int? menu)
        {
            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 2).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 2);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            tp_documento crearDocumento = new tp_documento { tpdoc_estado = true, tpdoc_razoninactivo = "No aplica" };
            BuscarFavoritos(menu);
            return View(crearDocumento);
        }

        // POST: tp_documento/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(tp_documento tp_documento, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.tp_documento
                           where a.tpdoc_nombre == tp_documento.tpdoc_nombre
                           select a.tpdoc_nombre).Count();

                if (nom == 0)
                {
                    tp_documento.tpdocfec_creacion = DateTime.Now;
                    tp_documento.tpdocuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.tp_documento.Add(tp_documento);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro del nuevo tipo documento fue exitoso!";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 2).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 2);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(tp_documento);
        }

        // GET: tp_documento/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tp_documento tp_documento = context.tp_documento.Find(id);
            if (tp_documento == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from c in context.users
                                        join b in context.tp_documento on c.user_id equals b.tpdocuserid_creacion
                                        where b.tpdoc_id == id
                                        select c.user_nombre;
            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }
            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result1 = from c in context.users
                                         join b in context.tp_documento on c.user_id equals b.tpdocuserid_actualizacion
                                         where b.tpdoc_id == id
                                         select c.user_nombre;
            foreach (string i in result1)
            {
                ViewBag.user_nombre_act = i;
            }

            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 2).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 2);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(tp_documento);
        }

        // POST: tp_documento/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(tp_documento tp_documento, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.tp_documento
                           where a.tpdoc_nombre == tp_documento.tpdoc_nombre || a.tpdoc_id == tp_documento.tpdoc_id
                           select a.tpdoc_nombre).Count();

                if (nom == 1)
                {
                    tp_documento.tpdocfec_actualizacion = DateTime.Now;
                    tp_documento.tpdocuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(tp_documento).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del tipo documento fue exitoso!";
                    ConsultaDatosCreacion(tp_documento);
                    BuscarFavoritos(menu);
                    return View(tp_documento);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            //var parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 2).ToList();
            //ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 2);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            ConsultaDatosCreacion(tp_documento);
            BuscarFavoritos(menu);
            return View(tp_documento);
        }

        public void ConsultaDatosCreacion(tp_documento tp_documento)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tp_documento.tpdocuserid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(tp_documento.tpdocuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarTiposDocumento()
        {
            var data = context.tp_documento.ToList().Select(x => new
            {
                x.tpdoc_id,
                x.tpdoc_nombre,
                tpdoc_estado = x.tpdoc_estado ? "Activo" : "Inactivo"
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }

            base.Dispose(disposing);
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