using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class departamentoGerencialController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: departamentoGerencial
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View(new departamento_gerencial { dpto_estado = true });
        }


        [HttpPost]
        public ActionResult Create(departamento_gerencial modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                departamento_gerencial buscarNombre =
                    context.departamento_gerencial.FirstOrDefault(x => x.dpto_nombre == modelo.dpto_nombre);
                if (buscarNombre != null)
                {
                    TempData["mensaje_error"] = "El nombre del departamento ya se encuentra registrado!";
                }
                else
                {
                    modelo.dptouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modelo.dptofec_creacion = DateTime.Now;
                    context.departamento_gerencial.Add(modelo);
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "El registro del departamento fue exitoso!";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
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

            departamento_gerencial dpto = context.departamento_gerencial.Find(id);
            if (dpto == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(dpto);
            BuscarFavoritos(menu);
            return View(dpto);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(departamento_gerencial modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                departamento_gerencial buscarNombre =
                    context.departamento_gerencial.FirstOrDefault(x => x.dpto_nombre == modelo.dpto_nombre);
                if (buscarNombre != null)
                {
                    if (buscarNombre.dpto_id != modelo.dpto_id)
                    {
                        TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                    }
                    else
                    {
                        modelo.dptofec_actualizacion = DateTime.Now;
                        modelo.dptouserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarNombre.dptofec_actualizacion = DateTime.Now;
                        buscarNombre.dptouserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarNombre.dpto_estado = modelo.dpto_estado;
                        buscarNombre.dpto_razoninactivo = modelo.dpto_razoninactivo;
                        buscarNombre.dpto_nombre = modelo.dpto_nombre;
                        context.Entry(buscarNombre).State = EntityState.Modified;
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            TempData["mensaje"] = "La actualización del departamento fue exitosa!";
                            ConsultaDatosCreacion(modelo);
                            BuscarFavoritos(menu);
                            return RedirectToAction("Create", new { menu });
                        }

                        TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                    }
                }
                else
                {
                    modelo.dptofec_actualizacion = DateTime.Now;
                    modelo.dptouserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(modelo).State = EntityState.Modified;
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "La actualización del departamento fue exitoso!";
                        ConsultaDatosCreacion(modelo);
                        BuscarFavoritos(menu);
                        return RedirectToAction("Create", new { menu });
                    }

                    TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                }
            }

            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public JsonResult BuscarDepartamentos()
        {
            var buscarDepartamentos = (from dpto in context.departamento_gerencial
                                       select new
                                       {
                                           dpto.dpto_id,
                                           dpto.dpto_nombre,
                                           estado = dpto.dpto_estado ? "Activo" : "Inactivo"
                                       }).ToList();
            return Json(buscarDepartamentos, JsonRequestBehavior.AllowGet);
        }


        public void ConsultaDatosCreacion(departamento_gerencial modelo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creador = (from c in context.users
                             join b in context.departamento_gerencial on c.user_id equals b.dptouserid_creacion
                             where b.dpto_id == modelo.dpto_id
                             select c).FirstOrDefault();

            ViewBag.user_nombre_cre = creador != null ? creador.user_nombre + " " + creador.user_apellido : null;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users actualizador = (from c in context.users
                                  join b in context.departamento_gerencial on c.user_id equals b.dptouserid_actualizacion
                                  where b.dpto_id == modelo.dpto_id
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