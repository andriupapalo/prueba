using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class estadosCitasTallerController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: estadosCitasTaller
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            ViewBag.estadoanterior = new SelectList(context.tcitasestados, "id", "Descripcion");
            return View(new tcitasestados { estado = true });
        }


        // POST: estadosCitasTaller
        [HttpPost]
        public ActionResult Create(tcitasestados modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                tcitasestados buscarPorTipoEstado = context.tcitasestados.FirstOrDefault(x => x.tipoestado == modelo.tipoestado);
                if (buscarPorTipoEstado != null)
                {
                    TempData["mensaje_error"] = "El registro ingresado ya existe, por favor valide";
                }
                else
                {
                    modelo.fec_creacion = DateTime.Now;
                    modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);


                    context.tcitasestados.Add(modelo);
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "La creación del tipo de tecnico fue exitoso";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                    }
                }
            }

            BuscarFavoritos(menu);
            ViewBag.estadoanterior = new SelectList(context.tcitasestados, "id", "Descripcion");
            return View();
        }


        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tcitasestados estado = context.tcitasestados.Find(id);
            if (estado == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(estado);
            BuscarFavoritos(menu);
            ViewBag.estadoanterior = new SelectList(context.tcitasestados, "id", "Descripcion", estado.estadoanterior);
            return View(estado);
        }


        [HttpPost]
        public ActionResult Edit(tcitasestados modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                tcitasestados buscarPorTipoEstado = context.tcitasestados.FirstOrDefault(x => x.tipoestado == modelo.tipoestado);
                if (buscarPorTipoEstado != null)
                {
                    if (buscarPorTipoEstado.id != modelo.id)
                    {
                        TempData["mensaje_error"] = "El registro ingresado ya existe, por favor valide";
                    }
                    else
                    {
                        modelo.fec_actualizacion = DateTime.Now;
                        modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarPorTipoEstado.fec_actualizacion = DateTime.Now;
                        buscarPorTipoEstado.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarPorTipoEstado.estado = modelo.estado;
                        buscarPorTipoEstado.estadoanterior = modelo.estadoanterior;
                        buscarPorTipoEstado.razon_inactivo = modelo.razon_inactivo;
                        buscarPorTipoEstado.tipoestado = modelo.tipoestado;
                        buscarPorTipoEstado.Descripcion = modelo.Descripcion;
                        buscarPorTipoEstado.color_estado = modelo.color_estado;
                        context.Entry(buscarPorTipoEstado).State = EntityState.Modified;
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            TempData["mensaje"] = "La actualización del estado de cita fue exitoso";
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
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
                        TempData["mensaje"] = "La actualización del estado de cita fue exitoso";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                    }
                }
            }

            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            ViewBag.estadoanterior = new SelectList(context.tcitasestados, "id", "Descripcion", modelo.estadoanterior);
            return View(modelo);
        }


        public void ConsultaDatosCreacion(tcitasestados estadoCita)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(estadoCita.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(estadoCita.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarEstadosCitas()
        {
            var buscarEstados = (from estados in context.tcitasestados
                                 select new
                                 {
                                     estados.id,
                                     estados.tipoestado,
                                     estados.Descripcion,
                                     estado = estados.estado ? "Activo" : "Inactivo"
                                 }).ToList();

            return Json(buscarEstados, JsonRequestBehavior.AllowGet);
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