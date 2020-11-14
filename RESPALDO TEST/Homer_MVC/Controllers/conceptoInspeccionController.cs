using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class conceptoInspeccionController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: conceptoInspeccion
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View(new tconceptoinspeccion { estado = true });
        }


        [HttpPost]
        public ActionResult Create(tconceptoinspeccion modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                tconceptoinspeccion buscarNombre = context.tconceptoinspeccion.FirstOrDefault(x => x.Descripcion == modelo.Descripcion);
                if (buscarNombre != null)
                {
                    TempData["mensaje_error"] = "El nombre del concepto ya se encuentra registrado!";
                }
                else
                {
                    modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modelo.fec_creacion = DateTime.Now;
                    context.tconceptoinspeccion.Add(modelo);
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "El registro del concepto se guardo exitosamente!";
                        BuscarFavoritos(menu);
                        return RedirectToAction("Create", new { menu });
                    }

                    TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
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

            tconceptoinspeccion tipo = context.tconceptoinspeccion.Find(id);
            if (tipo == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(tipo);
            BuscarFavoritos(menu);
            return View(tipo);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(tconceptoinspeccion modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                tconceptoinspeccion buscarNombre = context.tconceptoinspeccion.FirstOrDefault(x => x.Descripcion == modelo.Descripcion);
                if (buscarNombre != null)
                {
                    if (buscarNombre.id != modelo.id)
                    {
                        TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                    }
                    else
                    {
                        modelo.fec_actualizacion = DateTime.Now;
                        modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarNombre.fec_actualizacion = DateTime.Now;
                        buscarNombre.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarNombre.estado = modelo.estado;
                        buscarNombre.Descripcion = modelo.Descripcion;
                        buscarNombre.razon_inactivo = modelo.razon_inactivo;
                        context.Entry(buscarNombre).State = EntityState.Modified;
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            TempData["mensaje"] = "La actualización del concepto fue exitosa!";
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                        }
                    }
                }
                else
                {
                    modelo.fec_actualizacion = DateTime.Now;
                    modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(modelo).State = EntityState.Modified;
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "La actualización del concepto fue exitosa!";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                    }
                }
            }

            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public void ConsultaDatosCreacion(tconceptoinspeccion modelo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creador = (from c in context.users
                             join b in context.tconceptoinspeccion on c.user_id equals b.userid_creacion
                             where b.id == modelo.id
                             select c).FirstOrDefault();

            ViewBag.user_nombre_cre = creador != null ? creador.user_nombre + " " + creador.user_apellido : null;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users actualizador = (from c in context.users
                                  join b in context.tconceptoinspeccion on c.user_id equals b.user_idactualizacion
                                  where b.id == modelo.id
                                  select c).FirstOrDefault();

            ViewBag.user_nombre_act =
                actualizador != null ? actualizador.user_nombre + " " + actualizador.user_apellido : null;
        }


        public JsonResult BuscarConceptosInspeccion()
        {
            var buscarConceptos = (from conceptos in context.tconceptoinspeccion
                                   select new
                                   {
                                       conceptos.id,
                                       conceptos.Descripcion,
                                       estado = conceptos.estado ? "Activo" : "Inactivo"
                                   }).ToList();
            return Json(buscarConceptos, JsonRequestBehavior.AllowGet);
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