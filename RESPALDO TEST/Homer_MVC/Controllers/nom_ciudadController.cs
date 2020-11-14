using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class nom_ciudadController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        // GET: nom_ciudad/Create
        public ActionResult Create(int? menu)
        {
            ViewBag.dpto_id = new SelectList(context.nom_departamento.Take(0), "dpto_id", "dpto_nombre");
            ViewBag.pais_id = new SelectList(context.nom_pais.OrderBy(x => x.pais_nombre), "pais_id", "pais_nombre");
            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 5).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;

            //var enlacesBuscar = context.icb_modulo_enlaces.Where(x=>x.enl_modulo==5);
            //string enlaces = "";
            //foreach (var item in enlacesBuscar)
            //{
            //    var buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
            //    enlaces += "<li><a href='" + buscarEnlace.url+ "'>"+buscarEnlace.nombreMenu+"</a></li>";
            //}
            //ViewBag.nombreEnlaces = enlaces;
            nom_ciudad crearCiudad = new nom_ciudad { ciu_estado = true, ciu_razoninactivo = "No aplica" };
            BuscarFavoritos(menu);
            return View(crearCiudad);
        }

        // POST: nom_ciudad/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(nom_ciudad nom_ciudad, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.nom_ciudad
                           where a.ciu_nombre == nom_ciudad.ciu_nombre
                           select a.ciu_nombre).Count();

                if (nom == 0)
                {
                    nom_ciudad.ciufec_creacion = DateTime.Now;
                    nom_ciudad.ciuuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.nom_ciudad.Add(nom_ciudad);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro de la nueva ciudad fue exitoso!";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            string paisSeleccionado = Request["pais_id"];
            int paisId = paisSeleccionado != "" ? Convert.ToInt32(paisSeleccionado) : 0;
            ViewBag.pais_id = new SelectList(context.nom_pais.OrderBy(x => x.pais_nombre), "pais_id", "pais_nombre",
                paisId);
            ViewBag.dpto_id = new SelectList(context.nom_departamento.Where(x => x.pais_id == paisId), "dpto_id",
                "dpto_nombre");
            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 5).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            BuscarFavoritos(menu);
            return View(nom_ciudad);
        }


        public JsonResult BuscarDeptoPorPais(int paisId)
        {
            var buscaDepto = context.nom_departamento.Where(x => x.pais_id == paisId).OrderBy(x => x.dpto_nombre)
                .Select(x => new
                {
                    x.dpto_id,
                    x.dpto_nombre
                });
            return Json(buscaDepto, JsonRequestBehavior.AllowGet);
        }


        // GET: nom_ciudad/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            nom_ciudad nom_ciudad = context.nom_ciudad.FirstOrDefault(x => x.ciu_id == id);
            if (nom_ciudad == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from c in context.users
                                        join b in context.nom_ciudad on c.user_id equals b.ciuuserid_creacion
                                        where b.ciu_id == id
                                        select c.user_nombre;
            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }
            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result1 = from c in context.users
                                         join b in context.nom_ciudad on c.user_id equals b.ciuuserid_actualizacion
                                         where b.ciu_id == id
                                         select c.user_nombre;
            foreach (string i in result1)
            {
                ViewBag.user_nombre_act = i;
            }

            nom_departamento buscaDepartamento = context.nom_departamento.FirstOrDefault(x => x.dpto_id == nom_ciudad.dpto_id);
            ViewBag.pais_id = new SelectList(context.nom_pais.OrderBy(x => x.pais_nombre), "pais_id", "pais_nombre",
                buscaDepartamento.pais_id);
            ViewBag.dpto_id =
                new SelectList(context.nom_departamento.Where(x => x.pais_id == buscaDepartamento.pais_id), "dpto_id",
                    "dpto_nombre", nom_ciudad.dpto_id);
            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 5).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            BuscarFavoritos(menu);
            return View(nom_ciudad);
        }


        // POST: nom_ciudad/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(nom_ciudad nom_ciudad, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.nom_ciudad
                           where a.ciu_nombre == nom_ciudad.ciu_nombre || a.ciu_id == nom_ciudad.ciu_id
                           select a.ciu_nombre).Count();

                if (nom == 1)
                {
                    nom_ciudad.ciufec_actualizacion = DateTime.Now;
                    nom_ciudad.ciuuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(nom_ciudad).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización de la ciudad fue exitoso!";
                    ConsultaDatosCreacion(nom_ciudad);
                    nom_departamento buscaDepartamento =
                        context.nom_departamento.FirstOrDefault(x => x.dpto_id == nom_ciudad.dpto_id);
                    ViewBag.pais_id = new SelectList(context.nom_pais.OrderBy(x => x.pais_nombre), "pais_id",
                        "pais_nombre", buscaDepartamento.pais_id);
                    ViewBag.dpto_id =
                        new SelectList(context.nom_departamento.Where(x => x.pais_id == buscaDepartamento.pais_id),
                            "dpto_id", "dpto_nombre", nom_ciudad.dpto_id);
                    BuscarFavoritos(menu);
                    return View(nom_ciudad);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            nom_departamento buscaDepartamento2 = context.nom_departamento.FirstOrDefault(x => x.dpto_id == nom_ciudad.dpto_id);
            ViewBag.pais_id = new SelectList(context.nom_pais.OrderBy(x => x.pais_nombre), "pais_id", "pais_nombre",
                buscaDepartamento2.pais_id);
            ViewBag.dpto_id =
                new SelectList(context.nom_departamento.Where(x => x.pais_id == buscaDepartamento2.pais_id), "dpto_id",
                    "dpto_nombre", nom_ciudad.dpto_id);
            ConsultaDatosCreacion(nom_ciudad);
            BuscarFavoritos(menu);
            return View(nom_ciudad);
        }


        public void ConsultaDatosCreacion(nom_ciudad nom_ciudad)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(nom_ciudad.ciuuserid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(nom_ciudad.ciuuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarCiudadesPaginadas()
        {
            var data = from ciudad in context.nom_ciudad
                       join dpto in context.nom_departamento
                           on ciudad.dpto_id equals dpto.dpto_id
                       join pais in context.nom_pais
                           on dpto.pais_id equals pais.pais_id
                       select new
                       {
                           ciudad.ciu_id,
                           ciudad.ciu_nombre,
                           ciu_estado = ciudad.ciu_estado ? "Activo" : "Inactivo",
                           dpto.dpto_nombre,
                           pais.pais_nombre,
                           codigo = ciudad.cod_ciudad
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