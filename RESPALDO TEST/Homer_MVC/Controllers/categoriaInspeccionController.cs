using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class categoriaInspeccionController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: categoriaInspeccion
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            ViewBag.idconcepto = new SelectList(context.tconceptoinspeccion, "id", "Descripcion");
            return View(new tcategoriainspeccion { estado = true });
        }

        [HttpPost]
        public ActionResult Create(tcategoriainspeccion modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                tcategoriainspeccion buscarNombre = context.tcategoriainspeccion.FirstOrDefault(x =>
                    x.descripcion == modelo.descripcion && x.idconcepto == modelo.idconcepto);
                if (buscarNombre != null)
                {
                    TempData["mensaje_error"] =
                        "El nombre de la categoria ya se encuentra registrado para el concepto seleccionado!";
                }
                else
                {
                    modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modelo.fec_creacion = DateTime.Now;
                    context.tcategoriainspeccion.Add(modelo);
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "El registro de la categoria se guardo exitosamente!";
                        BuscarFavoritos(menu);
                        return RedirectToAction("Create", new { menu });
                    }

                    TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                }
            }

            BuscarFavoritos(menu);
            ViewBag.idconcepto = new SelectList(context.tconceptoinspeccion, "id", "Descripcion", modelo.idconcepto);
            return View(modelo);
        }


        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tcategoriainspeccion categoria = context.tcategoriainspeccion.Find(id);
            if (categoria == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(categoria);
            BuscarFavoritos(menu);
            ViewBag.idconcepto = new SelectList(context.tconceptoinspeccion, "id", "Descripcion", categoria.idconcepto);
            return View(categoria);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(tcategoriainspeccion modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                tcategoriainspeccion buscarNombre = context.tcategoriainspeccion.FirstOrDefault(x =>
                    x.descripcion == modelo.descripcion && x.idconcepto == modelo.idconcepto);
                if (buscarNombre != null)
                {
                    if (buscarNombre.id != modelo.id)
                    {
                        TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                    }
                    else
                    {
                        //modelo.fec_actualizacion = DateTime.Now;
                        //modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarNombre.fec_actualizacion = DateTime.Now;
                        buscarNombre.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarNombre.estado = modelo.estado;
                        buscarNombre.descripcion = modelo.descripcion;
                        buscarNombre.idconcepto = modelo.idconcepto;
                        buscarNombre.razon_inactivo = modelo.razon_inactivo;
                        context.Entry(buscarNombre).State = EntityState.Modified;
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            TempData["mensaje"] = "La actualización de la categoria fue exitosa!";
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
                        TempData["mensaje"] = "La actualización de la subfuente fue exitosa!";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                    }
                }
            }

            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            ViewBag.idconcepto = new SelectList(context.tconceptoinspeccion, "id", "Descripcion", modelo.idconcepto);
            return View(modelo);
        }


        public JsonResult BuscarCategorias()
        {
            var buscarCategorias = (from categoria in context.tcategoriainspeccion
                                    join conceptos in context.tconceptoinspeccion
                                        on categoria.idconcepto equals conceptos.id
                                    select new
                                    {
                                        categoria.id,
                                        categoria.descripcion,
                                        estado = categoria.estado ? "Activo" : "Inactivo",
                                        concepto = conceptos.Descripcion
                                    }).ToList();
            return Json(buscarCategorias, JsonRequestBehavior.AllowGet);
        }


        public void ConsultaDatosCreacion(tcategoriainspeccion modelo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creador = (from c in context.users
                             join b in context.tcategoriainspeccion on c.user_id equals b.userid_creacion
                             where b.id == modelo.id
                             select c).FirstOrDefault();

            ViewBag.user_nombre_cre = creador != null ? creador.user_nombre + " " + creador.user_apellido : null;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users actualizador = (from c in context.users
                                  join b in context.tcategoriainspeccion on c.user_id equals b.user_idactualizacion
                                  where b.id == modelo.id
                                  select c).FirstOrDefault();

            ViewBag.user_nombre_act =
                actualizador != null ? actualizador.user_nombre + " " + actualizador.user_apellido : null;
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