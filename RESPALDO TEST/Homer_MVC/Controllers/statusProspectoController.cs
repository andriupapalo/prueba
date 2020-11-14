using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class statusProspectoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        [HttpGet]
        // GET: status
        public ActionResult Crear(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        //POST: crear
        public ActionResult Crear(icb_statusprospecto postStatus, int? menu)
        {
            if (ModelState.IsValid)
            {
                icb_statusprospecto buscarNombre =
                    context.icb_statusprospecto.FirstOrDefault(x => x.status_nombre == postStatus.status_nombre);
                if (buscarNombre == null)
                {
                    postStatus.status_fecela = DateTime.Now;
                    postStatus.status_usuela = Convert.ToInt32(Session["user_usuarioid"]);
                    context.icb_statusprospecto.Add(postStatus);
                    context.SaveChanges();
                    TempData["mensaje"] = "Registro creado correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "El estado " + postStatus.status_nombre + " ya existe";
                    BuscarFavoritos(menu);
                    return RedirectToAction("Crear");
                }
            }

            BuscarFavoritos(menu);
            return View();
        }

        // POST: status/Edit/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(icb_statusprospecto status, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.icb_statusprospecto
                           where a.status_nombre == status.status_nombre || a.status_id == status.status_id
                           select a.status_nombre).Count();

                if (nom == 1)
                {
                    status.status_fecha_actualizacion = DateTime.Now;
                    status.status_usuario_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(status).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del estado fue exitoso";
                    ConsultaDatosCreacion(status.status_id);
                    BuscarFavoritos(menu);
                    return View(status);
                }

                TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
            }

            ConsultaDatosCreacion(status.status_id);
            BuscarFavoritos(menu);
            return View(status);
        }

        // GET: areas/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_statusprospecto status = context.icb_statusprospecto.Find(id);
            if (status == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            //var result = from a in context.users
            //             join b in context.icb_statusprospecto on a.user_id equals b.status_usuela
            //             where b.status_id == id
            //             select a.user_nombre.ToString();
            //foreach (var i in result)
            //{
            //    ViewBag.user_nombre_cre = i;
            //}

            ////consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            //var result1 = from a in context.users
            //              join b in context.icb_statusprospecto on a.user_id equals b.status_usuario_actualizacion
            //              where b.status_id == id
            //              select a.user_nombre.ToString();
            //foreach (var i in result1)
            //{
            //    ViewBag.user_nombre_act = i;
            //}
            ConsultaDatosCreacion(status.status_id);
            BuscarFavoritos(menu);
            return View(status);
        }

        public JsonResult BuscarStatus()
        {
            System.Collections.Generic.List<icb_statusprospecto> result = context.icb_statusprospecto.ToList();

            var reg = result.Select(x => new
            {
                x.status_id,
                x.status_nombre,
                estado = x.status_estado ? "Activo" : "Inactivo"
            });

            var data = new
            {
                reg
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public void ConsultaDatosCreacion(int id)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creador = (from c in context.users
                             join b in context.icb_statusprospecto on c.user_id equals b.status_usuela
                             where b.status_id == id
                             select c).FirstOrDefault();

            ViewBag.user_nombre_cre = creador != null ? creador.user_nombre + " " + creador.user_apellido : null;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users actualizador = (from c in context.users
                                  join b in context.icb_statusprospecto on c.user_id equals b.status_usuario_actualizacion
                                  where b.status_id == id
                                  select c).FirstOrDefault();

            ViewBag.user_nombre_act =
                actualizador != null ? actualizador.user_nombre + " " + actualizador.user_apellido : null;
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