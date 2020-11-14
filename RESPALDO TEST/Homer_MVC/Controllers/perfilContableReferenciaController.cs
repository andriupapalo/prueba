using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class perfilContableReferenciaController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: perfilContableReferencia
        public ActionResult Create(int? menu)
        {
            ViewBag.cuentas_puc = context.cuenta_puc.Where(x => x.esafectable).OrderBy(x => x.cntpuc_descp).ToList();
            ViewBag.centroCosto = context.centro_costo.OrderBy(x => x.centcst_nombre).ToList();
            BuscarFavoritos(menu);
            return View(new perfilcontable_referencia { estado = true });
        }

        [HttpPost]
        public ActionResult Create(perfilcontable_referencia modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                perfilcontable_referencia buscarPerfil =
                    context.perfilcontable_referencia.FirstOrDefault(x => x.descripcion == modelo.descripcion);
                if (buscarPerfil != null)
                {
                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                }
                else
                {
                    modelo.fec_creacion = DateTime.Now;
                    modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.perfilcontable_referencia.Add(modelo);
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "El registro del nuevo perfil contable fue exitoso!";
                        return RedirectToAction("Create", new { menu });
                    }
                }
            }

            ViewBag.cuentas_puc = context.cuenta_puc.Where(x => x.esafectable).ToList();
            ViewBag.centroCosto = context.centro_costo.OrderBy(x => x.centcst_nombre).ToList();
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public ActionResult update(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            perfilcontable_referencia perfil = context.perfilcontable_referencia.FirstOrDefault(x => x.id == id);
            if (perfil == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(perfil);
            ViewBag.cuentas_puc = context.cuenta_puc.Where(x => x.esafectable).ToList();
            ViewBag.centroCosto = context.centro_costo.OrderBy(x => x.centcst_nombre).ToList();
            ViewBag.perfilActual = perfil;
            BuscarFavoritos(menu);
            return View(perfil);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(perfilcontable_referencia modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.perfilcontable_referencia
                           where a.descripcion == modelo.descripcion || a.id == modelo.id
                           select a.descripcion).Count();

                if (nom == 1)
                {
                    modelo.fec_actualizacion = DateTime.Now;
                    modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(modelo).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del perfil contable fue exitoso!";
                }
                else
                {
                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                }
            }

            ViewBag.cuentas_puc = context.cuenta_puc.Where(x => x.esafectable).ToList();
            ViewBag.centroCosto = context.centro_costo.OrderBy(x => x.centcst_nombre).ToList();
            ConsultaDatosCreacion(modelo);
            ViewBag.perfilActual = modelo;
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public void ConsultaDatosCreacion(perfilcontable_referencia modelo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(modelo.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(modelo.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarPerfilesPaginadas()
        {
            var buscarPerfiles = context.perfilcontable_referencia.Select(x => new
            {
                x.id,
                x.descripcion,
                estado = x.estado ? "Activo" : "Inactivo"
            }).ToList();
            return Json(buscarPerfiles, JsonRequestBehavior.AllowGet);
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