using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Homer_MVC.IcebergModel;

namespace Homer_MVC.Controllers
{
    public class rolsController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        public void ParametrosVista()
        {
            var enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 9);
            var enlaces = "";
            foreach (var item in enlacesBuscar)
            {
                var buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;          
        }


        // GET: Rols/Create
        public ActionResult Create(int? menu)
        {
            ParametrosVista();
            var crearRol = new rols { rol_estado = true, rol_razoninactivo = "No aplica" };
            var jerarquia = context.clasificacion_rol.Select(d => new { id = d.id_clasificacion, nombre = d.nombre_clasificacion }).ToList();
            ViewBag.clasificacion_rol = new SelectList(jerarquia,"id","nombre");
            BuscarFavoritos(menu);
            return View(crearRol);
        }


        // POST: Rols/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(rols rol, int? menu)
        {
            var jerarquia = context.clasificacion_rol.Select(d => new { id = d.id_clasificacion, nombre = d.nombre_clasificacion }).ToList();

            if (ModelState.IsValid)
            {
                ViewBag.clasificacion_rol = new SelectList(jerarquia, "id", "nombre",rol.clasificacion_rol);
                //consulta si el registro esta en BD
                var nom = (from a in context.rols
                           where a.rol_nombre == rol.rol_nombre
                           select a.rol_nombre).Count();

                if (nom == 0)
                {
                    rol.rolfec_creacion = DateTime.Now;
                    rol.roluserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.rols.Add(rol);
                    var guardar = context.SaveChanges() > 0;
                    if (guardar)
                    {
                        ParametrosVista();
                        TempData["mensaje"] = "El registro del nuevo rol fue exitoso!";
                        BuscarFavoritos(menu);
                        return View(rol);
                    }

                    TempData["mensaje_error"] = "Error de conexion!";
                }
                else
                {
                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                }
            }

            ParametrosVista();
            ViewBag.clasificacion_rol = new SelectList(jerarquia, "id", "nombre", rol.clasificacion_rol);

            BuscarFavoritos(menu);
            return View(rol);
        }

        // GET: Rols/Edit/5
        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var rol = context.rols.Find(id);
            if (rol == null) return HttpNotFound();
            ConsultaDatosCreacion(id ?? 0);
            var jerarquia = context.clasificacion_rol.Select(d => new { id = d.id_clasificacion, nombre = d.nombre_clasificacion }).ToList();
            ViewBag.clasificacion_rol = new SelectList(jerarquia, "id", "nombre", rol.clasificacion_rol);

            var modulosAsignables = context.Menus.Where(x => x.url != "#").OrderBy(x => x.nombreMenu).ToList();
            ViewBag.ModulosAsignables = modulosAsignables;
            var modulosAsignados = context.Menu_rol.Where(x => x.idperfil == id).ToList();
            var idModulosAsignados = "";
            var index = 0;
            foreach (var ids in modulosAsignados)
                if (index == 0)
                {
                    idModulosAsignados += ids.idmenu;
                    index++;
                }
                else
                {
                    idModulosAsignados += "," + ids.idmenu;
                }

            ViewBag.menusAsignados = idModulosAsignados;
            //ViewBag.permisosVistas = context.rolpermisos.ToList();

            var dashboard = context.dashboard_rol.Select(d => new { id = d.id_vista, nombre = d.nombre_vista }).ToList();
            ViewBag.dashboard_inicial = new SelectList(dashboard, "id", "nombre",rol.dashboard_inicial);
            BuscarFavoritos(menu);
            ParametrosVista();
            return View(rol);
        }

        // POST: Rols/Edit/5
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult update(rols rol, int? menu)
        {
            var jerarquia = context.clasificacion_rol.Select(d => new { id = d.id_clasificacion, nombre = d.nombre_clasificacion }).ToList();
            ViewBag.clasificacion_rol = new SelectList(jerarquia, "id", "nombre", rol.clasificacion_rol);
            var dashboard = context.dashboard_rol.Select(d => new { id = d.id_vista, nombre = d.nombre_vista }).ToList();
            ViewBag.dashboard_inicial = new SelectList(dashboard, "id", "nombre", rol.dashboard_inicial);
            if (ModelState.IsValid)
            {
                var nombrerol = rol.rol_nombre.Trim();
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                var nom = context.rols
                    .Where(d => d.rol_nombre.ToUpper() == nombrerol.ToUpper() && d.rol_id == rol.rol_id).Count();

                if (nom == 1)
                {
                    var cantidad = Convert.ToInt32(Request["totalPermisosBodegas"]);
                    const string queryAcceso = "DELETE FROM [dbo].[rolacceso] WHERE [idrol]={0}";
                    var rowsAcceso = context.Database.ExecuteSqlCommand(queryAcceso, rol.rol_id);
                    for (var i = 0; i < cantidad; i++)
                    {
                        var idRolPermiso = Request["txtPermisoVista" + i];
                        var checkPermiso = Request["checkPermisoVista" + i];
                        if (idRolPermiso != null && checkPermiso != null)
                            context.rolacceso.Add(new rolacceso
                            {
                                idrol = rol.rol_id,
                                idpermiso = Convert.ToInt32(idRolPermiso)
                            });
                    }

                    var cantidades = Convert.ToInt32(Request["totalOpcionesAcceso"]);
                    const string queryOpcion = "DELETE FROM [dbo].[opcion_acceso_rol] WHERE [id_rol]={0}";
                    var rowsOpcion = context.Database.ExecuteSqlCommand(queryOpcion, rol.rol_id);
                    for (var i = 0; i < cantidades; i++)
                    {
                        var idRolOpcion = Request["txtopcionAcceso" + i];
                        var checkOpcion = Request["checkOpcionAcceso" + i];
                        if (idRolOpcion != null && checkOpcion != null)
                            context.opcion_acceso_rol.Add(new opcion_acceso_rol
                            {
                                id_rol = rol.rol_id,
                                id_opcion_acceso = Convert.ToInt32(idRolOpcion)
                            });
                    }

                    var idsModulos = Request["selectBusquedaModulos"];
                    if (idsModulos != null)
                    {
                        var idModulo = idsModulos.Split(',');
                        var modulosActuales = context.Menu_rol.Where(x => x.idperfil == rol.rol_id).ToList();
                        foreach (var id in modulosActuales)
                        {
                            var query = "DELETE FROM [dbo].[Menu_rol] WHERE [idmenu]={0} AND [idperfil] =" +
                                        id.idperfil;
                            var rows = context.Database.ExecuteSqlCommand(query, id.idmenu);
                        }

                        foreach (var id in idModulo)
                            context.Menu_rol.Add(new Menu_rol { idmenu = Convert.ToInt32(id), idperfil = rol.rol_id });
                        context.SaveChanges();
                    }

                    rol.rol_nombre = nombrerol.Trim();
                    rol.rolfec_actualizacion = DateTime.Now;
                    rol.roluserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(rol).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "La actualización del rol fue exitoso!";
                    var modulosAsignables1 = context.Menus.Where(x => x.url != "#").OrderBy(x => x.nombreMenu).ToList();
                    ViewBag.ModulosAsignables = modulosAsignables1;
                    ConsultaDatosCreacion(rol.rol_id);
                    BuscarFavoritos(menu);
                    return RedirectToAction("update", new { id = rol.rol_id, menu });
                }

                //verifico que NO exista el mismo rol con otro nombre
                var rol2 = context.rols
                    .Where(d => d.rol_nombre.ToUpper() == nombrerol.ToUpper() && d.rol_id != rol.rol_id).Count();
                if (rol2 > 0)
                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                else
                    TempData["mensaje_error"] =
                        "El rol no existe en base de datos, por favor crearlo en la pestaña Crear.";
            }

            var modulosAsignables = context.Menus.Where(x => x.url != "#").OrderBy(x => x.nombreMenu).ToList();
            ViewBag.ModulosAsignables = modulosAsignables;
            var modulosAsignados = context.Menu_rol.Where(x => x.idperfil == rol.rol_id).ToList();
            var idModulosAsignados = "";
            var index = 0;
            foreach (var ids in modulosAsignados)
                if (index == 0)
                {
                    idModulosAsignados += ids.idmenu;
                    index++;
                }
                else
                {
                    idModulosAsignados += "," + ids.idmenu;
                }

            ViewBag.menusAsignados = idModulosAsignados;
            //ViewBag.permisosVistas = context.rolpermisos.ToList();
            ParametrosVista();
            ConsultaDatosCreacion(rol.rol_id);
            BuscarFavoritos(menu);
            return View(rol);
        }


        public JsonResult PermisosABodegas(int? idRol)
        {
            var permisosVistas = (from Permiso in context.rolpermisos
                                      //join Acceso in context.rolacceso
                                      //on Permiso.id equals Acceso.idpermiso into ps
                                      //from g in ps.DefaultIfEmpty()
                                  select new
                                  {
                                      Permiso.id,
                                      Permiso.descripcion,
                                      activo = context.rolacceso.Where(x => x.idpermiso == Permiso.id && x.idrol == idRol).Count()
                                  }).ToList();
            return Json(permisosVistas, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult opcionesAcceso(int? idRoles)
        //{
        //    var data = (from opcion in context.opcionAcceso
        //                where opcion.idrol == idRoles
        //                          select new
        //                          {
        //                              opcion.id,
        //                              opcion.descripcion,
        //                              activos = context.rolopcionAcceso.Where(x => x.idopcionAcceso == opcion.id && x.idRol == idRoles).Count()
        //                          }).ToList();
        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult opcionesAcceso(int? idRoles)
        {
            var data = (from opcion in context.opcion_acceso
                        select new
                        {
                            id = opcion.id_opcion_acceso,
                            opcion.descripcion,
                            activos = context.opcion_acceso_rol
                                .Where(x => x.id_opcion_acceso == opcion.id_opcion_acceso && x.id_rol == idRoles).Count()
                        }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public void ConsultaDatosCreacion(int id)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            var creador = (from c in context.users
                           join b in context.rols on c.user_id equals b.roluserid_creacion
                           where b.rol_id == id
                           select c).FirstOrDefault();

            ViewBag.user_nombre_cre = creador != null ? creador.user_nombre + " " + creador.user_apellido : null;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            var actualizador = (from c in context.users
                                join b in context.rols on c.user_id equals b.roluserid_actualizacion
                                where b.rol_id == id
                                select c).FirstOrDefault();

            ViewBag.user_nombre_act =
                actualizador != null ? actualizador.user_nombre + " " + actualizador.user_apellido : null;
        }


        public JsonResult BuscarRolesPaginados()
        {
            var data = context.rols.ToList().Select(x => new
            {
                x.rol_id,
                x.rol_nombre,
                rol_estado = x.rol_estado ? "Activo" : "Inactivo"
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing) context.Dispose();
            base.Dispose(disposing);
        }


        public void BuscarFavoritos(int? menu)
        {
            var usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

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

            var esFavorito = false;

            foreach (var favoritosSeleccionados in buscarFavoritosSeleccionados)
                if (favoritosSeleccionados.idMenu == menu)
                {
                    esFavorito = true;
                    break;
                }

            if (esFavorito)
                ViewBag.Favoritos =
                    "<div id='areaFavoritos'><i class='fa fa-close'></i>&nbsp;&nbsp;<a href='#' onclick='AgregarQuitarFavorito();return false;'>Quitar de Favoritos</a><div>";
            else
                ViewBag.Favoritos =
                    "<div id='areaFavoritos'><i class='fa fa-check'></i>&nbsp;&nbsp;<a href='#' onclick='AgregarQuitarFavorito();return false;'>Agregar a Favoritos</a></div>";
            ViewBag.id_menu = menu != null ? menu : 0;
        }
    }
}