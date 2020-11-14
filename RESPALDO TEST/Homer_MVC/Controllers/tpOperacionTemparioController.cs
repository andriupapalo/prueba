using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tpOperacionTemparioController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: tpOperacionTempario
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View(new ttipooperacion { estado = true });
        }


        // POST: tpOperacionTempario
        [HttpPost]
        public ActionResult Create(ttipooperacion modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                ttipooperacion buscarNombre = context.ttipooperacion.FirstOrDefault(x => x.Descripcion == modelo.Descripcion);
                if (buscarNombre != null)
                {
                    TempData["mensaje_error"] = "El nombre del tipo de operacion ya existe";
                }
                else
                {
                    modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modelo.fec_creacion = DateTime.Now;
                    context.ttipooperacion.Add(modelo);
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "El tipo de operacion se ha creado exitosamente.";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor verifique...";
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

            ttipooperacion tipoOperacion = context.ttipooperacion.Find(id);
            if (tipoOperacion == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(tipoOperacion);
            BuscarFavoritos(menu);
            return View(tipoOperacion);
        }


        [HttpPost]
        public ActionResult Edit(ttipooperacion modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                ttipooperacion buscarNombre = context.ttipooperacion.FirstOrDefault(x => x.Descripcion == modelo.Descripcion);
                int idBuscada = buscarNombre != null ? buscarNombre.id : 0;
                if (buscarNombre != null)
                {
                    if (idBuscada != modelo.id)
                    {
                        TempData["mensaje_error"] = "El nombre del tipo de operacion ya existe";
                    }
                    else
                    {
                        ttipooperacion buscarTipoOperacion = context.ttipooperacion.FirstOrDefault(x => x.id == modelo.id);
                        buscarTipoOperacion.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarTipoOperacion.fec_actualizacion = DateTime.Now;
                        buscarTipoOperacion.Descripcion = modelo.Descripcion;
                        buscarTipoOperacion.razon_inactivo = modelo.razon_inactivo;
                        buscarTipoOperacion.estado = modelo.estado;
                        context.Entry(buscarTipoOperacion).State = EntityState.Modified;
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            TempData["mensaje"] = "El tipo de operacion se ha actualizado exitosamente.";
                            ConsultaDatosCreacion(buscarTipoOperacion);
                        }
                        else
                        {
                            TempData["mensaje_error"] =
                                "Error de conexion con la base de datos, por favor verifique...";
                        }
                    }
                }
                else
                {
                    ttipooperacion buscarTipoOperacion = context.ttipooperacion.FirstOrDefault(x => x.id == modelo.id);
                    buscarTipoOperacion.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    buscarTipoOperacion.fec_actualizacion = DateTime.Now;
                    buscarTipoOperacion.Descripcion = modelo.Descripcion;
                    buscarTipoOperacion.razon_inactivo = modelo.razon_inactivo;
                    buscarTipoOperacion.estado = modelo.estado;
                    context.Entry(buscarTipoOperacion).State = EntityState.Modified;
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "El tipo de operacion se ha actualizado exitosamente.";
                        ConsultaDatosCreacion(buscarTipoOperacion);
                        BuscarFavoritos(menu);
                        return View(modelo);
                    }

                    TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor verifique...";
                }
            }

            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public void ConsultaDatosCreacion(ttipooperacion tipoOperacion)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tipoOperacion.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(tipoOperacion.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarTiposOperacion()
        {
            var buscarTipos = (from TipoOperacion in context.ttipooperacion
                               select new
                               {
                                   TipoOperacion.id,
                                   TipoOperacion.Descripcion,
                                   estado = TipoOperacion.estado ? "Activo" : "Inactivo"
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