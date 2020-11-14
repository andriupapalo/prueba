using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tipoTecnicoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: tipoTecnico
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View(new ttipotecnico { estado = true });
        }

        // POST: tipoTecnico
        [HttpPost]
        public ActionResult Create(ttipotecnico modelo, int? menu)
        {
            if (ModelState.IsValid)
            {

                string tipo = Request["tipo"];
                string valor = Request["valor"];
                string porcentaje = Request["porcentaje"];

                ttipotecnico buscarPorNombre = context.ttipotecnico.FirstOrDefault(x => x.Especializacion == modelo.Especializacion);
                if (buscarPorNombre != null)
                {
                    TempData["mensaje_error"] = "El registro ingresado ya existe, por favor valide";
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(valor) && !string.IsNullOrWhiteSpace(porcentaje))
                    {
                        modelo.tipo = tipo;
                        modelo.valorHr = Convert.ToDecimal(valor);
                        modelo.porcentaje = Convert.ToDouble(porcentaje);
                        modelo.fec_creacion = DateTime.Now;
                        modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                        context.ttipotecnico.Add(modelo);
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            TempData["mensaje"] = "La creación del tipo de tecnico fue exitoso";
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                        }

                    } else if (!string.IsNullOrWhiteSpace(valor)) {

                        modelo.tipo = tipo;
                        modelo.valorHr = Convert.ToDecimal(valor);
                        //modelo.porcentaje = Convert.ToDouble(porcentaje);
                        modelo.fec_creacion = DateTime.Now;
                        modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                        context.ttipotecnico.Add(modelo);
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
                    else if (!string.IsNullOrWhiteSpace(porcentaje)) {

                        modelo.tipo = tipo;
                        //modelo.valorHr = Convert.ToDecimal(valor);
                        modelo.porcentaje = Convert.ToDouble(porcentaje);
                        modelo.fec_creacion = DateTime.Now;
                        modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                        context.ttipotecnico.Add(modelo);
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
            }

            BuscarFavoritos(menu);
            return View();
        }


        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ttipotecnico tipoTecnico = context.ttipotecnico.Find(id);
            if (tipoTecnico == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(tipoTecnico);
            BuscarFavoritos(menu);
            return View(tipoTecnico);
        }


        [HttpPost]
        public ActionResult Edit(ttipotecnico modelo, int? menu)
        {

            string tipo = Request["tipo"];
            string valor = Request["valorHr"];
            string porcentaje = Request["porcentaje"];

            ttipotecnico buscarPorNombre = context.ttipotecnico.FirstOrDefault(x => x.Especializacion == modelo.Especializacion);
            if (buscarPorNombre != null)
            {
                if (buscarPorNombre.id != modelo.id)
                {
                    TempData["mensaje_error"] = "El registro ingresado ya existe, por favor valide";
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(valor) && !string.IsNullOrWhiteSpace(porcentaje))
                    {
                        buscarPorNombre.tipo = tipo;
                        buscarPorNombre.valorHr = Convert.ToDecimal(valor);
                        buscarPorNombre.porcentaje = Convert.ToDouble(porcentaje);
                        buscarPorNombre.fec_actualizacion = DateTime.Now;
                        buscarPorNombre.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarPorNombre.fec_actualizacion = DateTime.Now;
                        buscarPorNombre.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        context.Entry(buscarPorNombre).State = EntityState.Modified;
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            TempData["mensaje"] = "La actualización del tipo de tecnico fue exitoso";
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(valor))
                    {
                        buscarPorNombre.tipo = tipo;
                        buscarPorNombre.valorHr = Convert.ToDecimal(valor);
                        //buscarPorNombre.porcentaje = Convert.ToDouble(porcentaje);
                        buscarPorNombre.fec_actualizacion = DateTime.Now;
                        buscarPorNombre.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarPorNombre.fec_actualizacion = DateTime.Now;
                        buscarPorNombre.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        context.Entry(buscarPorNombre).State = EntityState.Modified;
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            TempData["mensaje"] = "La actualización del tipo de tecnico fue exitoso";
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(porcentaje))
                    {
                        buscarPorNombre.tipo = tipo;
                        //buscarPorNombre.valorHr = Convert.ToDecimal(valor);
                        buscarPorNombre.porcentaje = Convert.ToDouble(porcentaje);
                        buscarPorNombre.fec_actualizacion = DateTime.Now;
                        buscarPorNombre.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarPorNombre.fec_actualizacion = DateTime.Now;
                        buscarPorNombre.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        context.Entry(buscarPorNombre).State = EntityState.Modified;
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            TempData["mensaje"] = "La actualización del tipo de tecnico fue exitoso";
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                        }
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
                    TempData["mensaje"] = "La actualización del tipo de tecnico fue exitoso";
                }
                else
                {
                    TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                }
            }


            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public void ConsultaDatosCreacion(ttipotecnico tipoTecnico)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tipoTecnico.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(tipoTecnico.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarTiposTecnicos()
        {
            var buscarTipos = (from tiposTecnicos in context.ttipotecnico
                               select new
                               {
                                   tiposTecnicos.id,
                                   tiposTecnicos.Especializacion,
                                   tiposTecnicos.tipo,
                                   tiposTecnicos.valorHr,
                                   tiposTecnicos.porcentaje,
                                   estado = tiposTecnicos.estado ? "Activo" : "Inactivo"
                               }).ToList();

            return Json(buscarTipos, JsonRequestBehavior.AllowGet);
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