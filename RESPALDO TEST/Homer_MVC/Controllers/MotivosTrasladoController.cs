using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class MotivosTrasladoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: MotivosTraslado
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            ViewBag.id_menu = menu;
            MotivosTrasladoModel model = new MotivosTrasladoModel
            {
                estado = true,
                razon_inactivo = ""
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(MotivosTrasladoModel model, int? menu)
        {
            if (ModelState.IsValid)
            {
                motivos_traslado buscarNombre =
                    context.motivos_traslado.FirstOrDefault(x => x.motivo == model.motivo);
                if (buscarNombre == null)
                {
                    motivos_traslado guardarmotivo = new motivos_traslado
                    {
                        motivo = model.motivo,
                        estado = model.estado,
                        razon_inactivo = model.razon_inactivo
                    };
                    guardarmotivo.fec_creacion = DateTime.Now;
                    guardarmotivo.user_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.motivos_traslado.Add(guardarmotivo);
                    context.SaveChanges();
                    ViewBag.id_menu = menu;
                    TempData["mensaje"] = "El dato ingresado se ha creado correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "El dato ingresado ya existe";
                    BuscarFavoritos(menu);
                    return View(model);
                }
            }

            return View();
        }

        public ActionResult Update(int id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            motivos_traslado motivos = context.motivos_traslado.Find(id);
            if (motivos == null)
            {
                return HttpNotFound();
            }

            MotivosTrasladoModel motivo = new MotivosTrasladoModel();
            if (motivos.fec_actualizacion != null)
            {
                motivo = new MotivosTrasladoModel
                {
                    motivo = motivos.motivo,
                    id = motivos.id,
                    razon_inactivo = motivos.razon_inactivo,
                    estado = motivos.estado,
                    fec_creacion = motivos.fec_creacion,
                    fec_actualizacion = motivos.fec_actualizacion.Value,
                };
            }
            else
            {
                motivo = new MotivosTrasladoModel
                {
                    motivo = motivos.motivo,
                    id = motivos.id,
                    razon_inactivo = motivos.razon_inactivo,
                    estado = motivos.estado,
                    fec_creacion = motivos.fec_creacion,
                };
            }

            IQueryable<string> userCre = from a in context.users
                                         join b in context.motivos_traslado on a.user_id equals b.user_creacion
                                         where b.id == id
                                         select a.user_nombre;

            foreach (string userCreacion in userCre)
            {
                ViewBag.user_nombre_cre = userCreacion;
            }

            IQueryable<string> userAct = from a in context.users
                                         join b in context.motivos_traslado on a.user_id equals b.user_actualizacion
                                         where b.id == id
                                         select a.user_nombre;
            ViewBag.fec_actualizacion = motivo.fec_actualizacion;
            foreach (string userActualizacion in userAct)
            {
                ViewBag.user_nombre_act = userActualizacion;
            }

            BuscarFavoritos(menu);
            return View(motivo);
        }

        [HttpPost]
        public ActionResult Update(MotivosTrasladoModel model, int? menu)
        {
            if (ModelState.IsValid)
            {
                int nom = (from a in context.motivos_traslado
                           where a.motivo == model.motivo || a.id == model.id
                           select a.motivo).Count();
                if (nom == 1)
                {
                    motivos_traslado motivo2 = context.motivos_traslado.Find(model.id);
                    motivo2.motivo = model.motivo;
                    motivo2.estado = model.estado;
                    motivo2.razon_inactivo = model.razon_inactivo;
                    motivo2.fec_actualizacion = DateTime.Now;
                    motivo2.user_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(motivo2).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización de la averia fue exitosa!";
                    BuscarFavoritos(menu);
                    IQueryable<string> userCre = from a in context.users
                                                 join b in context.motivos_traslado on a.user_id equals b.user_creacion
                                                 where b.id == model.id
                                                 select a.user_nombre;

                    foreach (string userCreacion in userCre)
                    {
                        ViewBag.user_nombre_cre = userCreacion;
                    }

                    IQueryable<string> userAct = from a in context.users
                                                 join b in context.motivos_traslado on a.user_id equals b.user_actualizacion
                                                 where b.id == model.id
                                                 select a.user_nombre;

                    foreach (string userActualizacion in userAct)
                    {
                        ViewBag.user_nombre_act = userActualizacion;
                    }

                    return View(model);
                }

                TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
            }

            BuscarFavoritos(menu);
            return View(model);
        }

        [HttpPost]
        public ActionResult BuscarMotivosTrasladoPaginados()
        {
            var data = context.motivos_traslado.ToList().Select(x => new
            {
                x.id,
                x.motivo,
                estado = x.estado ? "Activo" : "Inactivo"
            });
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