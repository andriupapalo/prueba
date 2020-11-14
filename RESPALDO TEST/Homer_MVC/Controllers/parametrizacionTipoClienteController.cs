using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class parametrizacionTipoClienteController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        private void parametrosBusqueda()
        {
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 119);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
        }

        public ActionResult Create(int? menu)
        {
            parametrosBusqueda();
            rparametrotipocliente tipi = new rparametrotipocliente { estado = true, razon_inactivo = "No aplica" };
            ViewBag.idtipocliente =
                new SelectList(context.rtipocliente.Where(x => x.estado).OrderByDescending(x => x.descripcion), "id",
                    "descripcion");
            var clientes = from t in context.icb_terceros
                           join c in context.tercero_cliente
                               on t.tercero_id equals c.tercero_id
                           where t.doc_tercero != null && t.tercero_estado
                           orderby t.prinom_tercero
                           select new
                           {
                               cedula = t.doc_tercero + " - " + t.prinom_tercero + " " + t.apellido_tercero + " " +
                                        t.segapellido_tercero + " " + t.razon_social,
                               t.tercero_id
                           };
            ViewBag.idtercero = new SelectList(clientes, "tercero_id", "cedula");

            BuscarFavoritos(menu);
            return View(tipi);
        }

        [HttpPost]
        public ActionResult Create(rparametrotipocliente tipi, int? menu)
        {
            if (ModelState.IsValid)
            {
                rparametrotipocliente buscarNombre = context.rparametrotipocliente.FirstOrDefault(x =>
                    x.idtipocliente == tipi.idtipocliente && x.valor == tipi.valor && x.porcentaje == tipi.porcentaje);
                if (buscarNombre == null)
                {
                    tipi.fec_creacion = DateTime.Now;
                    tipi.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.rparametrotipocliente.Add(tipi);
                    context.SaveChanges();
                    TempData["mensaje"] = "La parametrización del cliente se creo correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "La parametrización del cliente ya existe";
                }
            }

            ViewBag.idtipocliente =
                new SelectList(context.rtipocliente.Where(x => x.estado).OrderByDescending(x => x.descripcion), "id",
                    "descripcion");
            var clientes = from t in context.icb_terceros
                           join c in context.tercero_cliente
                               on t.tercero_id equals c.tercero_id
                           where t.doc_tercero != null && t.tercero_estado
                           orderby t.prinom_tercero
                           select new
                           {
                               cedula = t.doc_tercero + " - " + t.prinom_tercero + " " + t.apellido_tercero + " " +
                                        t.segapellido_tercero + " " + t.razon_social,
                               t.tercero_id
                           };
            ViewBag.idtercero = new SelectList(clientes, "tercero_id", "cedula");

            parametrosBusqueda();
            BuscarFavoritos(menu);
            return RedirectToAction("Create", new { menu });
        }

        public ActionResult Edit(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            rparametrotipocliente tipi = context.rparametrotipocliente.Find(id);
            if (tipi == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from a in context.users
                                        join b in context.rparametrotipocliente on a.user_id equals b.userid_creacion
                                        where b.id == id
                                        select a.user_nombre;
            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }
            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result1 = from a in context.users
                                         join b in context.rparametrotipocliente on a.user_id equals b.user_idactualizacion
                                         where b.id == id
                                         select a.user_nombre;
            foreach (string i in result1)
            {
                ViewBag.user_nombre_act = i;
            }

            ViewBag.idtipocliente =
                new SelectList(context.rtipocliente.Where(x => x.estado).OrderByDescending(x => x.descripcion), "id",
                    "descripcion", tipi.idtipocliente);
            var clientes = from t in context.icb_terceros
                           join c in context.tercero_cliente
                               on t.tercero_id equals c.tercero_id
                           where t.doc_tercero != null && t.tercero_estado
                           orderby t.prinom_tercero
                           select new
                           {
                               cedula = t.doc_tercero + " - " + t.prinom_tercero + " " + t.apellido_tercero + " " +
                                        t.segapellido_tercero + " " + t.razon_social,
                               t.tercero_id
                           };
            ViewBag.idtercero = new SelectList(clientes, "tercero_id", "cedula");

            parametrosBusqueda();
            BuscarFavoritos(menu);
            return View(tipi);
        }

        [HttpPost]
        public ActionResult Edit(rparametrotipocliente tipi, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.rparametrotipocliente
                           where a.id == tipi.id
                           select a.id).Count();

                if (nom == 1)
                {
                    tipi.fec_actualizacion = DateTime.Now;
                    tipi.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(tipi).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización de la parametrización del cliente fue exitoso!";
                    parametrosBusqueda();
                    ConsultaDatosCreacion(tipi);
                    BuscarFavoritos(menu);
                    //return View(tipi);
                }
                else
                {
                    TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
                }
            }

            ViewBag.idtipocliente =
                new SelectList(context.rtipocliente.Where(x => x.estado).OrderByDescending(x => x.descripcion), "id",
                    "descripcion", tipi.idtipocliente);
            var clientes = from t in context.icb_terceros
                           join c in context.tercero_cliente
                               on t.tercero_id equals c.tercero_id
                           where t.doc_tercero != null && t.tercero_estado
                           orderby t.prinom_tercero
                           select new
                           {
                               cedula = t.doc_tercero + " - " + t.prinom_tercero + " " + t.apellido_tercero + " " +
                                        t.segapellido_tercero + " " + t.razon_social,
                               t.tercero_id
                           };
            ViewBag.idtercero = new SelectList(clientes, "tercero_id", "cedula");

            parametrosBusqueda();
            ConsultaDatosCreacion(tipi);
            BuscarFavoritos(menu);
            return View(tipi);
        }

        public JsonResult buscarDatos()
        {
            var data = (from a in context.rparametrotipocliente
                        join b in context.rtipocliente
                            on a.idtipocliente equals b.id
                        join c in context.icb_terceros
                            on a.idtercero equals c.tercero_id into temp
                        from c in temp.DefaultIfEmpty()
                        select new
                        {
                            a.id,
                            b.descripcion,
                            valor = a.valor == 1 ? "Costo" : "Precio",
                            a.porcentaje,
                            tercero = a.idtercero != null
                                ? c.doc_tercero + " - " + c.prinom_tercero + " " + c.segnom_tercero + " " + c.apellido_tercero +
                                  " " + c.segapellido_tercero + " " + c.razon_social
                                : "",
                            estado = a.estado ? "Activo" : "Inactivo"
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public void ConsultaDatosCreacion(rparametrotipocliente tipi)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tipi.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(tipi.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
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