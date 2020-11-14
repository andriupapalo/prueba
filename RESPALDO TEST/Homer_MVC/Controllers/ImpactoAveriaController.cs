using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class ImpactoAveriaController : Controller
    {

        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: ImpactoAveria
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            ViewBag.id_menu = menu;
            ImpactoAveriaModel model = new ImpactoAveriaModel
            {
                impacto_estado = true,
                impacto_razon_inactivo = ""
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(ImpactoAveriaModel impacto, int? menu)
        {
            if (ModelState.IsValid)
            {
                icb_impacto_averia buscarNombre =
                    context.icb_impacto_averia.FirstOrDefault(x => x.impacto_descripcion == impacto.impacto_descripcion);
                if (buscarNombre == null)
                {
                    icb_impacto_averia guardarimpacto = new icb_impacto_averia
                    {
                        impacto_descripcion = impacto.impacto_descripcion,
                        impacto_estado = impacto.impacto_estado,
                        impacto_razon_inactivo = impacto.impacto_razon_inactivo
                    };
                    guardarimpacto.impacto_fec_creacion = DateTime.Now;
                    guardarimpacto.impacto_userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.icb_impacto_averia.Add(guardarimpacto);
                    context.SaveChanges();
                    ViewBag.id_menu = menu;
                    TempData["mensaje"] = "El dato ingresado se ha creado correctamente";
                }
                else
                {
                    TempData["mensaje_error"] = "El dato ingresado ya existe";
                    BuscarFavoritos(menu);
                    return View(impacto);
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

            icb_impacto_averia impacto2 = context.icb_impacto_averia.Find(id);
            if (impacto2 == null)
            {
                return HttpNotFound();
            }

            ImpactoAveriaModel impacto = new ImpactoAveriaModel
            {
                impacto_descripcion = impacto2.impacto_descripcion,
                impacto_id = impacto2.impacto_id,
                impacto_razon_inactivo = impacto2.impacto_razon_inactivo,
                impacto_estado = impacto2.impacto_estado,
                impacto_fec_creacion = impacto2.impacto_fec_creacion
            };

            if (impacto2.impacto_fec_actualizacion != null)
            {
                impacto.impacto_fec_actualizacion = impacto2.impacto_fec_actualizacion.Value;
            }
            IQueryable<string> userCre = from a in context.users
                                         join b in context.icb_impacto_averia on a.user_id equals b.impacto_userid_creacion
                                         where b.impacto_id == id
                                         select a.user_nombre;

            foreach (string userCreacion in userCre)
            {
                ViewBag.user_nombre_cre = userCreacion;
            }

            IQueryable<string> userAct = from a in context.users
                                         join b in context.icb_impacto_averia on a.user_id equals b.impacto_userid_actualizacion
                                         where b.impacto_id == id
                                         select a.user_nombre;

            foreach (string userActualizacion in userAct)
            {
                ViewBag.user_nombre_act = userActualizacion;
            }

            BuscarFavoritos(menu);
            return View(impacto);
        }

        [HttpPost]
        public ActionResult Update(ImpactoAveriaModel impacto, int? menu)
        {
            if (ModelState.IsValid)
            {
                int nom = (from a in context.icb_impacto_averia
                           where a.impacto_descripcion == impacto.impacto_descripcion || a.impacto_id == impacto.impacto_id
                           select a.impacto_descripcion).Count();
                if (nom == 1)
                {
                    icb_impacto_averia impacto2 = context.icb_impacto_averia.Find(impacto.impacto_id);
                    impacto2.impacto_descripcion = impacto.impacto_descripcion;
                    impacto2.impacto_estado = impacto.impacto_estado;
                    impacto2.impacto_razon_inactivo = impacto.impacto_razon_inactivo;
                    impacto2.impacto_fec_actualizacion = DateTime.Now;
                    impacto2.impacto_userid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(impacto2).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización de la averia fue exitosa!";
                    BuscarFavoritos(menu);
                    IQueryable<string> userCre = from a in context.users
                                                 join b in context.icb_impacto_averia on a.user_id equals b.impacto_userid_creacion
                                                 where b.impacto_id == impacto.impacto_id
                                                 select a.user_nombre;

                    foreach (string userCreacion in userCre)
                    {
                        ViewBag.user_nombre_cre = userCreacion;
                    }

                    IQueryable<string> userAct = from a in context.users
                                                 join b in context.icb_impacto_averia on a.user_id equals b.impacto_userid_actualizacion
                                                 where b.impacto_id == impacto.impacto_id
                                                 select a.user_nombre;

                    foreach (string userActualizacion in userAct)
                    {
                        ViewBag.user_nombre_act = userActualizacion;
                    }

                    return View(impacto);
                }

                TempData["mensaje_error"] = "El registro que ingreso no se encuentra, por favor valide!";
            }

            BuscarFavoritos(menu);
            return View(impacto);
        }

        public ActionResult BuscarImpactoAveriasPaginados()
        {
            var data = context.icb_impacto_averia.ToList().Select(x => new
            {
                x.impacto_id,
                x.impacto_descripcion,
                impacto_estado = x.impacto_estado ? "Activo" : "Inactivo"
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