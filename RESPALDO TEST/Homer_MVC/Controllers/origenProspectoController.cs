using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class origenProspectoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        public ActionResult Create(int? menu)
        {
            tp_origen crear = new tp_origen { tporigen_estado = true };
            BuscarFavoritos(menu);
            return View(crear);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(tp_origen modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.tp_origen
                           where a.tporigen_nombre == modelo.tporigen_nombre || a.tporigen_id == modelo.tporigen_id
                           select a.tporigen_nombre).Count();

                if (nom == 0)
                {
                    modelo.tporigenfec_creacion = DateTime.Now;
                    modelo.tporigenuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.tp_origen.Add(modelo);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro del nuevo origen de prospecto fue exitoso!";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";
            BuscarFavoritos(menu);
            return View();
        }
        

        public ActionResult update(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tp_origen tp_origen = context.tp_origen.Find(id);
            if (tp_origen == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(tp_origen);
            BuscarFavoritos(menu);
            return View(tp_origen);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(tp_origen modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                int nom = (from a in context.tp_origen
                           where a.tporigen_nombre == modelo.tporigen_nombre && a.tporigen_id == modelo.tporigen_id
                           select a.tporigen_nombre).Count();

                if (nom == 1)
                {
                    modelo.tporigenfec_actualizacion = DateTime.Now;
                    modelo.tporigenuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(modelo).State = EntityState.Modified;
                    context.SaveChanges();
                    ConsultaDatosCreacion(modelo);
                    TempData["mensaje"] = "La actualización del tipo de origen fue exitoso!";
                    BuscarFavoritos(menu);
                    return View(modelo);
                }

                {
                    int nom2 = (from a in context.tp_origen
                                where a.tporigen_nombre == modelo.tporigen_nombre
                                select a.tporigen_nombre).Count();
                    if (nom2 == 0)
                    {
                        modelo.tporigenfec_actualizacion = DateTime.Now;
                        modelo.tporigenuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        context.Entry(modelo).State = EntityState.Modified;
                        context.SaveChanges();
                        ConsultaDatosCreacion(modelo);
                        TempData["mensaje"] = "La actualización del tipo de origen fue exitoso!";
                        BuscarFavoritos(menu);
                        return View(modelo);
                    }

                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                }
            }

            ConsultaDatosCreacion(modelo);
            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public void ConsultaDatosCreacion(tp_origen tp_origen)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tp_origen.tporigenuserid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(tp_origen.tporigenuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult GetOrigenesJson()
        {
            var buscarOrigenes = context.tp_origen.Select(x => new
            {
                x.tporigen_id,
                x.tporigen_nombre,
                tporigen_estado = x.tporigen_estado ? "Activo" : "Inactivo"
            });

            return Json(buscarOrigenes, JsonRequestBehavior.AllowGet);
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