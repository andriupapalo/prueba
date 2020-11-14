using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class TipoCompraRepuestoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: TipoCompraRepuesto
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View(new rtipocompra { estado = true });
        }

        [HttpPost]
        public ActionResult Create(rtipocompra modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                string nom = (from a in context.rtipocompra
                              where a.descripcion == modelo.descripcion
                              select a.descripcion).FirstOrDefault();

                if (nom == null)
                {
                    modelo.fec_creacion = DateTime.Now;
                    modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.rtipocompra.Add(modelo);
                    context.SaveChanges();
                    TempData["mensaje"] = "El registro del nuevo tipo de compra de repuestos fue exitoso!";
                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";
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

            rtipocompra tipo = context.rtipocompra.Find(id);
            if (tipo == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tipo.userid_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(tipo.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }

            BuscarFavoritos(menu);
            return View(tipo);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(rtipocompra modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                var nom = (from a in context.rtipocompra
                           where a.descripcion == modelo.descripcion
                           select new { a.id, a.descripcion }).FirstOrDefault();

                if (nom != null)
                {
                    if (nom.id == modelo.id)
                    {
                        modelo.fec_actualizacion = DateTime.Now;
                        modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        context.Entry(modelo).State = EntityState.Modified;
                        context.SaveChanges();
                        ConsultaDatosCreacion(modelo);
                        TempData["mensaje"] = "La actualización del tipo de compra fue exitoso!";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                    }
                }
                else
                {
                    modelo.fec_actualizacion = DateTime.Now;
                    modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(modelo).State = EntityState.Modified;
                    context.SaveChanges();
                    ConsultaDatosCreacion(modelo);
                    TempData["mensaje"] = "La actualización del tipo de compra fue exitoso!";
                }
            }
            else
            {
                ConsultaDatosCreacion(modelo);
                TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";
            }

            BuscarFavoritos(menu);
            return View(modelo);
        }


        public void ConsultaDatosCreacion(rtipocompra modelo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(modelo.userid_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = context.users.Find(modelo.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult GetTipoComprasJson()
        {
            var buscarTiposCompra = context.rtipocompra.ToList().Select(x => new
            {
                x.id,
                x.descripcion,
                estado = x.estado ? "Activo" : "Inactivo"
            });
            return Json(buscarTiposCompra, JsonRequestBehavior.AllowGet);
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