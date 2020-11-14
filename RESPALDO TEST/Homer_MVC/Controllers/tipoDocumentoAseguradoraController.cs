using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class tipoDocumentoAseguradoraController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: documentosAseguradora
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View(new ttipodocaseguradora { estado = true });
        }


        // POST: documentosAseguradora
        [HttpPost]
        public ActionResult Create(ttipodocaseguradora modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                ttipodocaseguradora buscarPorNombre = context.ttipodocaseguradora.FirstOrDefault(x => x.documento == modelo.documento);
                if (buscarPorNombre != null)
                {
                    TempData["mensaje_error"] = "El nombre del tipo de documento ya existe, por favor verifique...";
                }
                else
                {
                    modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modelo.fec_creacion = DateTime.Now;
                    context.ttipodocaseguradora.Add(modelo);
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "El registro del nuevo tipo de documento de aseguradora fue exitoso!";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error de conexion con base de datos, por favor verifique...";
                    }
                }
            }

            BuscarFavoritos(menu);
            return View(new ttipodocaseguradora { estado = true });
        }


        [HttpGet]
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ttipodocaseguradora tipoDocumento = context.ttipodocaseguradora.Find(id);
            if (tipoDocumento == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(tipoDocumento);
            BuscarFavoritos(menu);
            return View(tipoDocumento);
        }


        [HttpPost]
        public ActionResult Edit(ttipodocaseguradora modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                ttipodocaseguradora buscarNombre = context.ttipodocaseguradora.FirstOrDefault(x => x.documento == modelo.documento);
                if (buscarNombre != null)
                {
                    if (buscarNombre.id != modelo.id)
                    {
                        TempData["mensaje_error"] = "El nombre del tipo de documento ya existe";
                    }
                    else
                    {
                        ttipodocaseguradora buscarTipoDocumento = context.ttipodocaseguradora.FirstOrDefault(x => x.id == modelo.id);
                        buscarTipoDocumento.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarTipoDocumento.fec_actualizacion = DateTime.Now;
                        modelo.fec_actualizacion = DateTime.Now;
                        buscarTipoDocumento.documento = modelo.documento;
                        buscarTipoDocumento.razon_inactivo = modelo.razon_inactivo;
                        buscarTipoDocumento.estado = modelo.estado;
                        context.Entry(buscarTipoDocumento).State = EntityState.Modified;
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            TempData["mensaje"] = "El tipo de documento se ha actualizado exitosamente.";
                            ConsultaDatosCreacion(buscarTipoDocumento);
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
                    ttipodocaseguradora buscarTipoDocumento = context.ttipodocaseguradora.FirstOrDefault(x => x.id == modelo.id);
                    buscarTipoDocumento.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    buscarTipoDocumento.fec_actualizacion = DateTime.Now;
                    modelo.fec_actualizacion = DateTime.Now;
                    buscarTipoDocumento.documento = modelo.documento;
                    buscarTipoDocumento.razon_inactivo = modelo.razon_inactivo;
                    buscarTipoDocumento.estado = modelo.estado;
                    context.Entry(buscarTipoDocumento).State = EntityState.Modified;
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "El tipo de documento se ha actualizado exitosamente.";
                        ConsultaDatosCreacion(buscarTipoDocumento);
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


        public void ConsultaDatosCreacion(ttipodocaseguradora tipoDocumento)
        {
            users creator = context.users.Find(tipoDocumento.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(tipoDocumento.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }
        }


        public JsonResult BuscarTiposDocumentos()
        {
            var buscarTipos = (from tipoDocumento in context.ttipodocaseguradora
                               select new
                               {
                                   tipoDocumento.id,
                                   tipoDocumento.documento,
                                   estado = tipoDocumento.estado ? "Activo" : "Inactivo"
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