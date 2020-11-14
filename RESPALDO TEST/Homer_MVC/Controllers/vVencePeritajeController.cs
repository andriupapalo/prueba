using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class vVencePeritajeController : Controller
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

        // GET: vVencePeritaje/Create
        public ActionResult Create(int? menu)
        {
            parametrosBusqueda();
            BuscarFavoritos(menu);
            return View();
        }

        // POST: vVencePeritaje/Create
        [HttpPost]
        public ActionResult Create(vencePeritajeModel modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                vvencimientoperitaje data = new vvencimientoperitaje
                {
                    descripcion = modelo.descripcion,
                    valorKilometraje = Convert.ToDecimal(modelo.valorKilometraje),
                    valorTiempo = Convert.ToDecimal(modelo.valorTiempo),
                    estado = modelo.estado,
                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    fec_creacion = DateTime.Now
                };

                context.vvencimientoperitaje.Add(data);
                context.SaveChanges();
                TempData["mensaje"] = "El parametro de vencimiento peritaje se creo correctamente";
            }
            else
            {
                TempData["mensaje_error"] = "No fue posible guardar el registro, por favor valide";
            }

            parametrosBusqueda();
            BuscarFavoritos(menu);
            return RedirectToAction("Create", menu);
        }

        // GET: vVencePeritaje/update/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            vvencimientoperitaje vence = context.vvencimientoperitaje.Find(id);
            if (vence == null)
            {
                return HttpNotFound();
            }

            vencePeritajeModel modelo = new vencePeritajeModel
            {
                id = vence.id,
                descripcion = vence.descripcion,
                estado = vence.estado,
                fec_actualizacion = vence.fec_actualizacion,
                fec_creacion = vence.fec_creacion,
                razonInactivo = vence.razonInactivo,
                userid_creacion = vence.userid_creacion,
                userid_actualizacion = vence.userid_actualizacion,
                valorKilometraje = vence.valorKilometraje.ToString("N0", new CultureInfo("is-IS")),
                valorTiempo = vence.valorTiempo.ToString("N0", new CultureInfo("is-IS"))
            };
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from a in context.users
                                        join b in context.vvencimientoperitaje on a.user_id equals b.userid_creacion
                                        where b.id == id
                                        select a.user_nombre;
            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }
            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result1 = from a in context.users
                                         join b in context.vvencimientoperitaje on a.user_id equals b.userid_actualizacion
                                         where b.id == id
                                         select a.user_nombre;
            foreach (string i in result1)
            {
                ViewBag.user_nombre_act = i;
            }

            parametrosBusqueda();
            BuscarFavoritos(menu);
            return View(modelo);
        }

        // POST: vVencePeritaje/update/5
        [HttpPost]
        public ActionResult update(vencePeritajeModel modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.vvencimientoperitaje
                           where a.descripcion == modelo.descripcion || a.id == modelo.id
                           select a.descripcion).Count();

                if (nom == 1)
                {
                    vvencimientoperitaje data = context.vvencimientoperitaje.Where(d => d.id == modelo.id).FirstOrDefault();
                    data.descripcion = modelo.descripcion;
                    data.valorKilometraje = Convert.ToDecimal(modelo.valorKilometraje);
                    data.valorTiempo = Convert.ToDecimal(modelo.valorTiempo);
                    data.estado = modelo.estado;
                    data.fec_actualizacion = DateTime.Now;
                    data.userid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);

                    context.Entry(data).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del parametro de vencimiento fue exitoso!";
                    parametrosBusqueda();
                    ConsultaDatosCreacion(modelo);
                    BuscarFavoritos(menu);
                    return View(modelo);
                }

                TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
            }

            parametrosBusqueda();
            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }

        public void ConsultaDatosCreacion(vencePeritajeModel model)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(model.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(model.userid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult buscarVencimientos()
        {
            var data = (from v in context.vvencimientoperitaje
                        select new
                        {
                            v.id,
                            v.descripcion,
                            v.valorKilometraje,
                            v.valorTiempo,
                            estado = v.estado ? "Activo" : "Inactivo"
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