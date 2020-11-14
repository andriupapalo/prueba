using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class nom_departamentoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        // GET: nom_departamento/Create
        public ActionResult Create(int? menu)
        {
            ViewBag.pais_id = new SelectList(context.nom_pais, "pais_id", "pais_nombre");
            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 4).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 4);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            nom_departamento crearDepartamento = new nom_departamento { dpto_estado = true, dpto_razoninactivo = "No aplica" };
            BuscarFavoritos(menu);
            return View(crearDepartamento);
        }

        // POST: nom_departamento/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(nom_departamento nom_departamento, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.nom_departamento
                           where a.dpto_nombre == nom_departamento.dpto_nombre
                           select a.dpto_nombre).Count();

                if (nom == 0)
                {
                    nom_departamento.dptofec_creacion = DateTime.Now;
                    nom_departamento.dptouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.nom_departamento.Add(nom_departamento);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro del nuevo departamento fue exitoso!";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            ViewBag.pais_id = new SelectList(context.nom_pais, "pais_id", "pais_nombre");
            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 4).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 4);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(nom_departamento);
        }


        // GET: nom_departamento/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            nom_departamento nom_departamento = context.nom_departamento.Find(id);
            if (nom_departamento == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from c in context.users
                                        join b in context.nom_departamento on c.user_id equals b.dptouserid_creacion
                                        where b.dpto_id == id
                                        select c.user_nombre;
            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }
            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result1 = from c in context.users
                                         join b in context.nom_departamento on c.user_id equals b.dptouserid_actualizacion
                                         where b.dpto_id == id
                                         select c.user_nombre;
            foreach (string i in result1)
            {
                ViewBag.user_nombre_act = i;
            }

            ViewBag.pais_id = new SelectList(context.nom_pais, "pais_id", "pais_nombre", nom_departamento.pais_id);
            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 4).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 4);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(nom_departamento);
        }


        // POST: nom_departamento/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(nom_departamento nom_departamento, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.nom_departamento
                           where a.dpto_nombre == nom_departamento.dpto_nombre || a.dpto_id == nom_departamento.dpto_id
                           select a.dpto_nombre).Count();

                if (nom == 1)
                {
                    nom_departamento.dptofec_actualizacion = DateTime.Now;
                    nom_departamento.dptouserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(nom_departamento).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del departamento fue exitoso!";
                    ConsultaDatosCreacion(nom_departamento);
                    ViewBag.pais_id = new SelectList(context.nom_pais, "pais_id", "pais_nombre");
                    BuscarFavoritos(menu);
                    return View(nom_departamento);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            ViewBag.pais_id = new SelectList(context.nom_pais, "pais_id", "pais_nombre", nom_departamento.pais_id);
            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 4).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 4);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            ConsultaDatosCreacion(nom_departamento);
            ViewBag.pais_id = new SelectList(context.nom_pais, "pais_id", "pais_nombre");
            BuscarFavoritos(menu);
            return View(nom_departamento);
        }


        public void ConsultaDatosCreacion(nom_departamento nom_departamento)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(nom_departamento.dptouserid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(nom_departamento.dptouserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarDepartamentosPaginados()
        {
            var data = from dpto in context.nom_departamento
                       join pais in context.nom_pais
                           on dpto.pais_id equals pais.pais_id
                       select new
                       {
                           dpto.dpto_id,
                           dpto.dpto_nombre,
                           dpto_estado = dpto.dpto_estado ? "Activo" : "Inactivo",
                           pais.pais_nombre,
                           codigo = dpto.cod_dpto
                       };
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