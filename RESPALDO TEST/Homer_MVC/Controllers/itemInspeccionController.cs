using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class itemInspeccionController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: itemInspeccion
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            ViewBag.idcategoria = new SelectList(context.tcategoriainspeccion, "id", "descripcion");
            ViewBag.itemsEstadosColores = context.titemcolorinspeccion.ToList();
            return View(new titemsinspeccion { estado = true });
        }

        [HttpPost]
        public ActionResult Create(titemsinspeccion modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                titemsinspeccion buscarNombre = context.titemsinspeccion.FirstOrDefault(x =>
                    x.Descripcion == modelo.Descripcion && x.idcategoria == modelo.idcategoria);
                if (buscarNombre != null)
                {
                    TempData["mensaje_error"] =
                        "El nombre del item de inspección ya se encuentra registrado para la categoria seleccionada!";
                }
                else
                {
                    modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modelo.fec_creacion = DateTime.Now;
                    context.titemsinspeccion.Add(modelo);
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        if (modelo.tiporespuesta == "select")
                        {
                            titemsinspeccion buscarUltimoItem =
                                context.titemsinspeccion.OrderByDescending(x => x.id).FirstOrDefault();
                            int cantidadOpciones = Convert.ToInt32(Request["cantidad_opcines"]);
                            for (int i = 0; i < cantidadOpciones; i++)
                            {
                                string opcion = Request["opcion" + i + ""];
                                if (!string.IsNullOrEmpty(opcion))
                                {
                                    context.titemopcioninspeccion.Add(new titemopcioninspeccion
                                    {
                                        iditem = buscarUltimoItem.id,
                                        descripcion = opcion,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                        fec_creacion = DateTime.Now,
                                        estado = true
                                    });
                                }
                            }

                            context.SaveChanges();
                        }

                        TempData["mensaje"] = "El registro del item se guardo exitosamente!";
                        BuscarFavoritos(menu);
                        return RedirectToAction("Create", new { menu });
                    }

                    TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                }
            }

            BuscarFavoritos(menu);
            ViewBag.idcategoria = new SelectList(context.tcategoriainspeccion, "id", "descripcion");
            ViewBag.itemsEstadosColores = context.titemcolorinspeccion.ToList();
            return View(new titemsinspeccion { estado = true });
        }


        public ActionResult update(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            titemsinspeccion item = context.titemsinspeccion.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(item);
            BuscarFavoritos(menu);
            ViewBag.idcategoria = new SelectList(context.tcategoriainspeccion, "id", "descripcion", item.idcategoria);
            ViewBag.itemsEstadosColores = context.titemcolorinspeccion.ToList();
            ViewBag.tipoRespuesta = item.tiporespuesta;
            return View(item);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(titemsinspeccion modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                titemsinspeccion buscarNombre = context.titemsinspeccion.FirstOrDefault(x =>
                    x.Descripcion == modelo.Descripcion && x.idcategoria == modelo.idcategoria);
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
                        buscarNombre.idcategoria = modelo.idcategoria;
                        buscarNombre.tiporespuesta = modelo.tiporespuesta;
                        buscarNombre.razon_inactivo = modelo.razon_inactivo;
                        context.Entry(buscarNombre).State = EntityState.Modified;
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            const string query = "DELETE FROM [dbo].[titemopcioninspeccion] WHERE [iditem]={0}";
                            int rows = context.Database.ExecuteSqlCommand(query, buscarNombre.id);
                            if (modelo.tiporespuesta == "select")
                            {
                                titemsinspeccion buscarUltimoItem = context.titemsinspeccion.OrderByDescending(x => x.id)
                                    .FirstOrDefault();
                                int cantidadOpciones = Convert.ToInt32(Request["cantidad_opcines"]);
                                for (int i = 0; i < cantidadOpciones; i++)
                                {
                                    string opcion = Request["opcion" + i + ""];
                                    if (!string.IsNullOrEmpty(opcion))
                                    {
                                        context.titemopcioninspeccion.Add(new titemopcioninspeccion
                                        {
                                            iditem = buscarUltimoItem.id,
                                            descripcion = opcion,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            fec_creacion = DateTime.Now,
                                            estado = true
                                        });
                                    }
                                }

                                context.SaveChanges();
                            }

                            TempData["mensaje"] = "La actualización del item de inspección fue exitosa!";
                            return RedirectToAction("Create", new { menu });
                        }

                        TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
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
                        const string query = "DELETE FROM [dbo].[titemopcioninspeccion] WHERE [iditem]={0}";
                        int rows = context.Database.ExecuteSqlCommand(query, buscarNombre.id);
                        if (modelo.tiporespuesta == "select")
                        {
                            titemsinspeccion buscarUltimoItem =
                                context.titemsinspeccion.OrderByDescending(x => x.id).FirstOrDefault();
                            int cantidadOpciones = Convert.ToInt32(Request["cantidad_opcines"]);
                            for (int i = 0; i < cantidadOpciones; i++)
                            {
                                string opcion = Request["opcion" + i + ""];
                                if (!string.IsNullOrEmpty(opcion))
                                {
                                    context.titemopcioninspeccion.Add(new titemopcioninspeccion
                                    {
                                        iditem = buscarUltimoItem.id,
                                        descripcion = opcion,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                        fec_creacion = DateTime.Now,
                                        estado = true
                                    });
                                }
                            }

                            context.SaveChanges();
                        }

                        TempData["mensaje"] = "La actualización del item de inspección fue exitosa!";
                        return RedirectToAction("Create", new { menu });
                    }

                    TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                }
            }

            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            ViewBag.idcategoria = new SelectList(context.tcategoriainspeccion, "id", "descripcion");
            ViewBag.itemsEstadosColores = context.titemcolorinspeccion.ToList();
            return View(modelo);
        }


        public void ConsultaDatosCreacion(titemsinspeccion modelo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creador = (from c in context.users
                             join b in context.titemsinspeccion on c.user_id equals b.userid_creacion
                             where b.id == modelo.id
                             select c).FirstOrDefault();

            ViewBag.user_nombre_cre = creador != null ? creador.user_nombre + " " + creador.user_apellido : null;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users actualizador = (from c in context.users
                                  join b in context.titemsinspeccion on c.user_id equals b.user_idactualizacion
                                  where b.id == modelo.id
                                  select c).FirstOrDefault();

            ViewBag.user_nombre_act =
                actualizador != null ? actualizador.user_nombre + " " + actualizador.user_apellido : null;
        }


        public JsonResult BuscarOpcionesPorItem(int id_item)
        {
            var buscarOpciones = (from opcion in context.titemopcioninspeccion
                                  where opcion.iditem == id_item
                                  select new
                                  {
                                      opcion.id,
                                      opcion.descripcion
                                  }).ToList();
            return Json(buscarOpciones, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarItemsInspeccion()
        {
            var buscarItems = (from item in context.titemsinspeccion
                               join categoria in context.tcategoriainspeccion
                                   on item.idcategoria equals categoria.id
                               select new
                               {
                                   item.id,
                                   item.Descripcion,
                                   item.tiporespuesta,
                                   nombre_categoria = categoria.descripcion,
                                   estado = item.estado ? "Activo" : "Inactivo"
                               }).ToList();
            return Json(buscarItems, JsonRequestBehavior.AllowGet);
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