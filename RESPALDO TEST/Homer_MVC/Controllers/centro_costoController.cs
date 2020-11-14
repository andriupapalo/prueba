using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class centro_costoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public ActionResult Create(int? menu)
        {
            centro_costo crearCentro = new centro_costo { centcst_estado = true, centcstrazoninactivo = "No aplica" };
            ViewBag.grupocentroid =
                new SelectList(context.grupocentroscosto.OrderBy(x => x.descripcion), "id", "descripcion");
            ViewBag.bodega =
                new SelectList(context.bodega_concesionario.Where(x => x.bodccs_estado).OrderBy(x => x.bodccs_nombre),
                    "id", "bodccs_nombre");
            ViewBag.cartera = new SelectList(context.Tipos_Cartera.Where(x => x.estado).OrderBy(x => x.descripcion),
                "id", "descripcion");
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 70);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(crearCentro);
        }

        // POST: centro_costo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(centro_costo cent_cst, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD 
                //Se modifica bajo peticion del ticket 3404-967 donde se dice y pide que se valide solo por el prefijo el cual no se puede repetir
                int nom = (from a in context.centro_costo
                           where a.pre_centcst == cent_cst.pre_centcst
                           select a.centcst_nombre).Count();

                if (nom == 0)
                {
                    cent_cst.centcstfec_creacion = DateTime.Now;
                    cent_cst.centcstuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);

                    context.centro_costo.Add(cent_cst);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro del nuevo centro de costo fue exitoso!";
                    return RedirectToAction("Create", new { menu });
                }

                TempData["mensaje_error"] = "El prefijo que ingreso ya se encuentra registrado, por favor valide!";
            }

            ViewBag.cartera = new SelectList(context.Tipos_Cartera.Where(x => x.estado).OrderBy(x => x.descripcion),
                "id", "descripcion");

            ViewBag.bodega =
                new SelectList(context.bodega_concesionario.Where(x => x.bodccs_estado).OrderBy(x => x.bodccs_nombre),
                    "id", "bodccs_nombre", cent_cst.bodega);
            ViewBag.grupocentroid = new SelectList(context.grupocentroscosto.OrderBy(x => x.descripcion), "id",
                "descripcion", cent_cst.grupocentroid);
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 70);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(cent_cst);
        }


        public JsonResult usuariosResponsables() {
            var users = context.users.Select(x => new { x.user_id, x.user_nombre, x.user_apellido }).ToList();
            var data = users.Select(x => new
                {
                x.user_id,
                nombre = x.user_nombre + " " + x.user_apellido
                }).ToList();


            return Json(data, JsonRequestBehavior.AllowGet);
            }

        // GET: centro_costo/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            centro_costo cent_cst = context.centro_costo.Find(id);
            if (cent_cst == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(cent_cst.centcstuserid_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(cent_cst.centcstuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }

            ViewBag.bodega =
                new SelectList(context.bodega_concesionario.Where(x => x.bodccs_estado).OrderBy(x => x.bodccs_nombre),
                    "id", "bodccs_nombre", cent_cst.bodega);
            ViewBag.cartera = new SelectList(context.Tipos_Cartera.Where(x => x.estado).OrderBy(x => x.descripcion),
                "id", "descripcion", cent_cst.cartera);

           
            var users = context.users.Select(x => new { x.user_id, x.user_nombre, x.user_apellido }).ToList();
            var data = users.Select(x => new
                {
                x.user_id,
                nombre = x.user_nombre + " " + x.user_apellido
                }).ToList();
            ViewBag.idresponsdable = new SelectList(data, "user_id", "nombre", cent_cst.idresponsdable);

            ViewBag.grupocentroid = new SelectList(context.grupocentroscosto.OrderBy(x => x.descripcion), "id",
                "descripcion", cent_cst.grupocentroid);
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 70);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            return View(cent_cst);
        }


        // POST: cent_cst/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(centro_costo cent_cst, int? menu)
        {

            var users = context.users.Select(x => new { x.user_id, x.user_nombre, x.user_apellido }).ToList();
            var data = users.Select(x => new
                {
                x.user_id,
                nombre = x.user_nombre + " " + x.user_apellido
                }).ToList();
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.centro_costo
                           where a.pre_centcst == cent_cst.pre_centcst
                           select a.centcst_nombre).Count();

                if (nom == 1)
                {
              
                    ViewBag.idresponsdable = new SelectList(data, "user_id", "nombre", cent_cst.idresponsdable);
                    cent_cst.centcstfec_actualizacion = DateTime.Now;
                    cent_cst.centcstuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(cent_cst).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del centro de costo fue exitosa!";
                    ConsultaDatosCreacion(cent_cst);
                    BuscarFavoritos(menu);
                    ViewBag.bodega =
                        new SelectList(
                            context.bodega_concesionario.Where(x => x.bodccs_estado).OrderBy(x => x.bodccs_nombre),
                            "id", "bodccs_nombre", cent_cst.bodega);
                    ViewBag.cartera =
                        new SelectList(context.Tipos_Cartera.Where(x => x.estado).OrderBy(x => x.descripcion), "id",
                            "descripcion", cent_cst.cartera);
                    ViewBag.grupocentroid = new SelectList(context.grupocentroscosto.OrderBy(x => x.descripcion), "id",
                        "descripcion", cent_cst.grupocentroid);
                    return View(cent_cst);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            ViewBag.cartera = new SelectList(context.Tipos_Cartera.Where(x => x.estado).OrderBy(x => x.descripcion),
                "id", "descripcion", cent_cst.cartera);

            ViewBag.grupocentroid = new SelectList(context.grupocentroscosto.OrderBy(x => x.descripcion), "id",
                "descripcion", cent_cst.grupocentroid);
            ViewBag.bodega =
                new SelectList(context.bodega_concesionario.Where(x => x.bodccs_estado).OrderBy(x => x.bodccs_nombre),
                    "id", "bodccs_nombre", cent_cst.bodega);

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 70);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }
       
            ViewBag.idresponsdable = new SelectList(data, "user_id", "nombre", cent_cst.idresponsdable);
            ViewBag.nombreEnlaces = enlaces;
            ConsultaDatosCreacion(cent_cst);
            BuscarFavoritos(menu);
            return View(cent_cst);
        }


        public void ConsultaDatosCreacion(centro_costo cent_cst)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(cent_cst.centcstuserid_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = context.users.Find(cent_cst.centcstuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarCentroCostoPaginados()
        {
            var data = (from x in context.centro_costo
                        join a in context.grupocentroscosto
                            on x.grupocentroid equals a.id into temp
                        from a in temp.DefaultIfEmpty()
                        select new
                        {
                            x.centcst_id,
                            x.centcst_nombre,
                            x.pre_centcst,
                            centcst_estado = x.centcst_estado ? "Activo" : "Inactivo",
                            descripcion = a.descripcion != null ? a.descripcion : "",
                            responsable = x.responsable != null ? x.responsable : ""
                        }).ToList();
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