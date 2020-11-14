using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class grupoCentroCostoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: grupoCentroCosto
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View(new grupocentroscosto { estado = true });
        }


        [HttpPost]
        public ActionResult Create(grupocentroscosto modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                grupocentroscosto buscarNombre = context.grupocentroscosto.FirstOrDefault(x => x.descripcion == modelo.descripcion);
                if (buscarNombre != null)
                {
                    TempData["mensaje_error"] = "El nombre del grupo que ingreso ya se encuentra, por favor valide!";
                }
                else
                {
                    modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modelo.fec_creacion = DateTime.Now;
                    context.grupocentroscosto.Add(modelo);
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "El registro del nuevo grupo fue exitoso!";
                        return RedirectToAction("Create");
                    }
                }
            }

            BuscarFavoritos(menu);
            return View(modelo);
        }


        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            grupocentroscosto grupo = context.grupocentroscosto.Find(id);
            if (grupo == null)
            {
                return HttpNotFound();
            }

            consultaDatosCreacion(grupo);
            BuscarFavoritos(menu);
            return View(grupo);
        }


        [HttpPost]
        public ActionResult update(grupocentroscosto modelo, int? menu)
        {
            bool guardar = false;
            if (ModelState.IsValid)
            {
                grupocentroscosto buscarNombre = context.grupocentroscosto.FirstOrDefault(x => x.descripcion == modelo.descripcion);
                if (buscarNombre != null)
                {
                    if (buscarNombre.id != modelo.id)
                    {
                        TempData["mensaje_error"] =
                            "El nombre del grupo que ingreso ya se encuentra, por favor valide!";
                    }
                    else
                    {
                        modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        modelo.fec_actualizacion = DateTime.Now;
                        buscarNombre.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarNombre.fec_actualizacion = DateTime.Now;
                        context.Entry(buscarNombre).State = EntityState.Modified;
                        guardar = context.SaveChanges() > 0;
                    }
                }
                else
                {
                    modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modelo.fec_actualizacion = DateTime.Now;
                    context.Entry(modelo).State = EntityState.Modified;
                    guardar = context.SaveChanges() > 0;
                }
            }

            if (guardar)
            {
                TempData["mensaje"] = "El registro del grupo se actualizo correctamente!";
            }

            consultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public void consultaDatosCreacion(grupocentroscosto grupo)
        {
            users creator = context.users.Find(grupo.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(grupo.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult GetGruposCentrosJson()
        {
            var buscarGrupos = (from grupo in context.grupocentroscosto
                                select new
                                {
                                    grupo.id,
                                    grupo.descripcion,
                                    estado = grupo.estado ? "Activo" : "Inactivo"
                                }).ToList();

            return Json(buscarGrupos, JsonRequestBehavior.AllowGet);
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