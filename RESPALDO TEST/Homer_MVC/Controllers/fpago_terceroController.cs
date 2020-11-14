using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class fpago_terceroController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        public void ParametrosVista()
        {
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 20);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
        }


        // GET: fpago_tercero
        public ActionResult Create(int? menu)
        {
            fpago_tercero crearFPago = new fpago_tercero { fpago_estado = true, fpago_razoninactivo = "No aplica" };
            BuscarFavoritos(menu);
            return View(crearFPago);
        }


        // POST: fpago_tercero/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(fpago_tercero fpago_tercero, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD
                int nom = (from a in context.fpago_tercero
                           where a.fpago_nombre == fpago_tercero.fpago_nombre
                           select a.fpago_nombre).Count();

                if (nom == 0)
                {
                    fpago_tercero.fpagofec_creacion = DateTime.Now;
                    fpago_tercero.fpagouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.fpago_tercero.Add(fpago_tercero);
                    bool guardar = context.SaveChanges() > 0;
                    if (guardar)
                    {
                        TempData["mensaje"] = "El registro de la nueva forma de pago fue exitoso!";
                        ParametrosVista();
                        BuscarFavoritos(menu);
                        return View(fpago_tercero);
                    }

                    TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                }
                else
                {
                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                }
            }

            ParametrosVista();
            BuscarFavoritos(menu);
            return View(fpago_tercero);
        }

        // GET: fpago_tercero/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            fpago_tercero fpago_tercero = context.fpago_tercero.Find(id);
            if (fpago_tercero == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(id ?? 0);
            ParametrosVista();
            BuscarFavoritos(menu);
            return View(fpago_tercero);
        }


        // POST: fpago_tercero/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(fpago_tercero fpago_tercero, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.fpago_tercero
                           where a.fpago_nombre == fpago_tercero.fpago_nombre || a.fpago_id == fpago_tercero.fpago_id
                           select a.fpago_nombre).Count();

                if (nom == 1)
                {
                    fpago_tercero.fpagofec_actualizacion = DateTime.Now;
                    fpago_tercero.fpagouserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(fpago_tercero).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización de la forma de pago fue exitoso!";
                    ParametrosVista();
                    ConsultaDatosCreacion(fpago_tercero.fpago_id);
                    BuscarFavoritos(menu);
                    return View(fpago_tercero);
                }

                TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
            }

            ParametrosVista();
            ConsultaDatosCreacion(fpago_tercero.fpago_id);
            BuscarFavoritos(menu);
            return View(fpago_tercero);
        }


        public void ConsultaDatosCreacion(int id)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creador = (from c in context.users
                             join b in context.fpago_tercero on c.user_id equals b.fpagouserid_creacion
                             where b.fpago_id == id
                             select c).FirstOrDefault();

            ViewBag.user_nombre_cre = creador != null ? creador.user_nombre + " " + creador.user_apellido : null;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users actualizador = (from c in context.users
                                  join b in context.fpago_tercero on c.user_id equals b.fpagouserid_actualizacion
                                  where b.fpago_id == id
                                  select c).FirstOrDefault();

            ViewBag.user_nombre_act =
                actualizador != null ? actualizador.user_nombre + " " + actualizador.user_apellido : null;
        }


        public JsonResult BuscarFormasPagoPaginados()
        {
            var data = context.fpago_tercero.ToList().Select(x => new
            {
                x.fpago_id,
                x.fpago_nombre,
                x.codigo,
                x.dvencimiento,
                fpago_estado = x.fpago_estado ? "Activo" : "Inactivo"
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }

            base.Dispose(disposing);
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