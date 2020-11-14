using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tpLlamadaRescateController : Controller
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
        public ActionResult Crear(icb_tpllamadarescate post, int? menu)
        {
            if (ModelState.IsValid)
            {
                icb_tpllamadarescate buscarNombre =
                    context.icb_tpllamadarescate.FirstOrDefault(x => x.tpllamada_nombre == post.tpllamada_nombre);
                if (buscarNombre == null)
                {
                    post.tpllamada_fec_creacion = DateTime.Now;
                    post.tpllamada_usuela = Convert.ToInt32(Session["user_usuarioid"]);
                    context.icb_tpllamadarescate.Add(post);
                    context.SaveChanges();
                    TempData["mensaje"] = "Registro creado correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "El registro ingresado ya existe";
                    BuscarFavoritos(menu);
                    return View();
                }
            }

            BuscarFavoritos(menu);
            return View();
        }

        // POST: status/Edit/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(icb_tpllamadarescate reg, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.icb_tpllamadarescate
                           where a.tpllamada_nombre == reg.tpllamada_nombre || a.tpllamada_id == reg.tpllamada_id
                           select a.tpllamada_nombre).Count();

                if (nom == 1)
                {
                    reg.tpllamada_fec_actualizacion = DateTime.Now;
                    reg.tpllamada_usu_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(reg).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del registro fue exitoso";
                    BuscarFavoritos(menu);
                    return View(reg);
                }

                TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
            }

            BuscarFavoritos(menu);
            return View(reg);
        }

        // GET: /Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_tpllamadarescate reg = context.icb_tpllamadarescate.Find(id);
            if (reg == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result = from a in context.users
                                        join b in context.icb_tpllamadarescate on a.user_id equals b.tpllamada_usuela
                                        where b.tpllamada_id == id
                                        select a.user_nombre;
            foreach (string i in result)
            {
                ViewBag.user_nombre_cre = i;
            }

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            IQueryable<string> result1 = from a in context.users
                                         join b in context.icb_tpllamadarescate on a.user_id equals b.tpllamada_usu_actualizacion
                                         where b.tpllamada_id == id
                                         select a.user_nombre;
            foreach (string i in result1)
            {
                ViewBag.user_nombre_act = i;
            }

            BuscarFavoritos(menu);
            return View(reg);
        }

        public JsonResult BuscarDatos()
        {
            System.Collections.Generic.List<icb_tpllamadarescate> result = context.icb_tpllamadarescate.ToList();

            var reg = result.Select(x => new
            {
                x.tpllamada_id,
                x.tpllamada_nombre,
                estado = x.tpllamada_estado ? "Activo" : "Inactivo"
            });

            var data = new
            {
                reg
            };
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