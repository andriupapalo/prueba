using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using Homer_MVC.IcebergModel;

namespace Homer_MVC.Controllers
{
    public class usersController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        private readonly ParametrosBusquedaModel parametrosBusqueda = new ParametrosBusquedaModel();
        CultureInfo miCultura = new CultureInfo("is-IS");
        CultureInfo miCulturaTime = new CultureInfo("en-US");

        /// <summary>
        ///     ajdjasdjasdjavsdvajsdjasd sandra
        /// </summary>
        /// <param name="menu"></param>
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


        public ActionResult usuario(int? menu)
        {
            listasDesplegables(new UsuarioModel());
            ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
            ViewBag.bodegas_visualizacion = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();

            ViewBag.icb_tptramite_prospecto =
                context.icb_tptramite_prospecto.OrderBy(x => x.tptrapros_descripcion).ToList();
            ViewBag.nombreEnlaces = parametrosBusqueda.EnlacesBusqueda(6);
            ViewBag.ciu_id = new SelectList(context.nom_ciudad.OrderBy(x => x.ciu_nombre).Where(x => x.ciu_estado),
                "ciu_id", "ciu_nombre");
            ViewBag.dpto_id =
                new SelectList(context.nom_departamento.OrderBy(x => x.dpto_nombre).Where(x => x.dpto_estado),
                    "dpto_id", "dpto_nombre");
            var crearUsuario = new UsuarioModel { user_estado = true, user_razoninactivo = "No aplica", aut_repuestos = false };
            BuscarFavoritos(menu ?? 0);
            return View(crearUsuario);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult usuario(UsuarioModel usuario, int? menu)
        {
            var bodegasSeleccionadas = Request["bodccs_cod"];
            var bodegasSeleccionadas2 = Request["bodegas_visualizacion"];

            var tramitesSeleccionados = Request["icb_tptramite_prospecto"];
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(bodegasSeleccionadas))
                {
                    TempData["mensaje_error"] = "Debe asignar mínimo una bodega!";
                    listasDesplegables(usuario);
                    ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
                    ViewBag.bodegas_visualizacion = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();

                    ViewBag.icb_tptramite_prospecto =
                        context.icb_tptramite_prospecto.OrderBy(x => x.tptrapros_descripcion).ToList();
                    ViewBag.nombreEnlaces = parametrosBusqueda.EnlacesBusqueda(6);
                    ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                    ViewBag.tramitesSeleccionados = tramitesSeleccionados;
                    ViewBag.ciu_id =
                        new SelectList(context.nom_ciudad.OrderBy(x => x.ciu_nombre).Where(x => x.ciu_estado), "ciu_id",
                            "ciu_nombre");
                    ViewBag.dpto_id =
                        new SelectList(context.nom_departamento.OrderBy(x => x.dpto_nombre).Where(x => x.dpto_estado),
                            "dpto_id", "dpto_nombre");
                    BuscarFavoritos(menu ?? 0);
                    return View(usuario);
                }

                //consulta si el registro documento esta en BD
                var nom = (from a in context.users
                           where a.user_numIdent == usuario.user_numIdent || a.user_usuario == usuario.user_usuario
                           select a.user_numIdent).Count();

                if (nom == 0)
                {
                    var crearUsuario = new users
                    {
                        tpdoc_id = usuario.tpdoc_id,
                        user_numIdent = usuario.user_numIdent,
                        user_nombre = usuario.user_nombre,
                        user_apellido = usuario.user_apellido,
                        rol_id = usuario.rol_id,
                        ciu_id = usuario.ciu_id,
                        dpto_id = usuario.dpto_id,
                        user_email = usuario.user_email,
                        user_telefono = usuario.user_telefono,
                        user_direccion = usuario.user_direccion,
                        user_usuario = usuario.user_usuario,
                        user_estado = usuario.user_estado,
                        user_razoninactivo = usuario.user_razoninactivo,
                        userfec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        cambio_contrasena = true,
                        aut_repuestos = usuario.aut_repuestos
                    };

                    var md5 = new MD5CryptoServiceProvider();
                    md5.ComputeHash(Encoding.ASCII.GetBytes(usuario.user_password));
                    var result = md5.Hash;
                    var str = new StringBuilder();
                    for (var i = 0; i < result.Length; i++) str.Append(result[i].ToString("x2"));
                    crearUsuario.user_password = str.ToString();
                    crearUsuario.user_confirPassword = str.ToString();
                    if (!string.IsNullOrWhiteSpace(usuario.fechainiplanta))
                    {
                        crearUsuario.fechainiplanta = DateTime.Parse(usuario.fechainiplanta, miCulturaTime);
                    }
                    if (!string.IsNullOrWhiteSpace(usuario.fechafinplanta))
                    {
                        crearUsuario.fechafinplanta = DateTime.Parse(usuario.fechafinplanta, miCulturaTime);
                    }
                    context.users.Add(crearUsuario);

                    if (usuario.user_password == usuario.user_confirPassword)
                    {
                        if (crearUsuario.fechainiplanta != null && crearUsuario.fechafinplanta != null)
                        {                           
                            if (crearUsuario.fechainiplanta < crearUsuario.fechafinplanta)
                            {
                                var guardar = context.SaveChanges();
                                if (guardar > 0)
                                {
                                    // Se agregan los tipos de tramites correspondientes al usuario que se va a crear
                                    var ultimoUsuario = context.users.OrderByDescending(x => x.user_id)
                                        .FirstOrDefault();

                                    if (!string.IsNullOrEmpty(tramitesSeleccionados))
                                    {
                                        var tramitesId = tramitesSeleccionados.Split(',');
                                        foreach (var substring in tramitesId)
                                            context.tipotramiteasesor.Add(new tipotramiteasesor
                                            {
                                                id_tipotramite = Convert.ToInt32(substring),
                                                id_usuario = ultimoUsuario.user_id
                                            });
                                    }
                                    // Primero se agregan los datos del usuario, una vez agregado se agregan las bodegas de ese usuario en la tabla bodega_usuario

                                    if (!string.IsNullOrEmpty(bodegasSeleccionadas))
                                    {
                                        var bodegasId = bodegasSeleccionadas.Split(',');
                                        foreach (var substring in bodegasId)
                                            context.bodega_usuario.Add(new bodega_usuario
                                            {
                                                id_bodega = Convert.ToInt32(substring),
                                                id_usuario = ultimoUsuario.user_id
                                            });
                                        var guardarBodegas = context.SaveChanges();
                                    }

                                    if (!string.IsNullOrEmpty(bodegasSeleccionadas2))
                                    {
                                        var bodegasId = bodegasSeleccionadas2.Split(',');
                                        foreach (var substring in bodegasId)
                                            context.bodega_usuario_visualizacion.Add(new bodega_usuario_visualizacion
                                            {
                                                id_bodega = Convert.ToInt32(substring),
                                                id_usuario = ultimoUsuario.user_id
                                            });
                                        var guardarBodegas = context.SaveChanges();
                                    }

                                    TempData["mensaje"] = "El registro del nuevo usuario fue exitoso!";
                                    return RedirectToAction("usuario");
                                }

                                TempData["mensaje_error"] = "No hay conexion con la base de datos, por favor valide!";
                            }
                            else
                            {
                                TempData["mensaje_error"] =
                                    "La fecha final no puede ser mayor a la fecha inicial, por favor valide!";
                            }
                        }
                        else
                        {
                            var guardar = context.SaveChanges();
                            if (guardar > 0)
                            {
                                // Se agregan los tipos de tramites correspondientes al usuario que se va a crear
                                var ultimoUsuario = context.users.OrderByDescending(x => x.user_id).FirstOrDefault();

                                if (!string.IsNullOrEmpty(tramitesSeleccionados))
                                {
                                    var tramitesId = tramitesSeleccionados.Split(',');
                                    foreach (var substring in tramitesId)
                                        context.tipotramiteasesor.Add(new tipotramiteasesor
                                        {
                                            id_tipotramite = Convert.ToInt32(substring),
                                            id_usuario = ultimoUsuario.user_id
                                        });
                                }
                                // Primero se agregan los datos del usuario, una vez agregado se agregan las bodegas de ese usuario en la tabla bodega_usuario

                                if (!string.IsNullOrEmpty(bodegasSeleccionadas))
                                {
                                    var bodegasId = bodegasSeleccionadas.Split(',');
                                    foreach (var substring in bodegasId)
                                        context.bodega_usuario.Add(new bodega_usuario
                                        {
                                            id_bodega = Convert.ToInt32(substring),
                                            id_usuario = ultimoUsuario.user_id
                                        });
                                    var guardarBodegas = context.SaveChanges();
                                }

                                if (!string.IsNullOrEmpty(bodegasSeleccionadas2))
                                {
                                    var bodegasId = bodegasSeleccionadas2.Split(',');
                                    foreach (var substring in bodegasId)
                                        context.bodega_usuario_visualizacion.Add(new bodega_usuario_visualizacion
                                        {
                                            id_bodega = Convert.ToInt32(substring),
                                            id_usuario = ultimoUsuario.user_id
                                        });
                                    var guardarBodegas = context.SaveChanges();
                                }

                                TempData["mensaje"] = "El registro del nuevo usuario fue exitoso!";
                                return RedirectToAction("usuario");
                            }

                            TempData["mensaje_error"] = "No hay conexion con la base de datos, por favor valide!";
                        }
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Los campos de contraseña no coinciden, por favor valide!";
                    }
                }
                else
                {
                    TempData["mensaje_error"] =
                        "El número de documento y/o usuario que ingreso ya se encuentra, por favor valide!";
                }
            }
            else
            {
                TempData["mensaje_error"] = "Error al ingresar los datos, por favor valide";
            }

            listasDesplegables(usuario);
            ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
            ViewBag.bodegas_visualizacion = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();

            ViewBag.icb_tptramite_prospecto =
                context.icb_tptramite_prospecto.OrderBy(x => x.tptrapros_descripcion).ToList();
            ViewBag.nombreEnlaces = parametrosBusqueda.EnlacesBusqueda(6);
            ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
            ViewBag.tramitesSeleccionados = tramitesSeleccionados;
            BuscarFavoritos(menu ?? 0);
            ViewBag.ciu_id = new SelectList(context.nom_ciudad.OrderBy(x => x.ciu_nombre).Where(x => x.ciu_estado),
                "ciu_id", "ciu_nombre");
            ViewBag.dpto_id =
                new SelectList(context.nom_departamento.OrderBy(x => x.dpto_nombre).Where(x => x.dpto_estado),
                    "dpto_id", "dpto_nombre");
            return View(usuario);
        }


        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var usuario = context.users.Find(id);
            if (usuario == null) return HttpNotFound();
            var usuarioBuscado = new UsuarioModel
            {
                user_id = usuario.user_id,
                tpdoc_id = usuario.tpdoc_id,
                user_numIdent = usuario.user_numIdent,
                user_nombre = usuario.user_nombre,
                user_apellido = usuario.user_apellido,
                rol_id = usuario.rol_id,
                ciu_id = usuario.ciu_id,
                dpto_id = usuario.dpto_id,
                user_email = usuario.user_email,
                user_telefono = usuario.user_telefono,
                user_direccion = usuario.user_direccion,
                user_usuario = usuario.user_usuario,
                user_estado = usuario.user_estado,
                user_razoninactivo = usuario.user_razoninactivo,
                userfec_creacion = usuario.userfec_creacion,
                userid_creacion = usuario.userid_creacion,
                userfec_actualizacion = usuario.userfec_actualizacion,
                user_password = usuario.user_password,
                user_confirPassword = usuario.user_confirPassword,
                fechainiplanta = usuario.fechainiplanta!=null?usuario.fechainiplanta.Value.ToString("yyyy/MM/dd",miCulturaTime):"",
                fechafinplanta = usuario.fechafinplanta != null ? usuario.fechafinplanta.Value.ToString("yyyy/MM/dd", miCulturaTime) : "",
                aut_repuestos = usuario.aut_repuestos != null ? usuario.aut_repuestos.Value : false
            };
            ConsultaDatosCreacion(usuario.user_id);
            listasDesplegables(usuarioBuscado);
            ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
            ViewBag.bodegas_visualizacion = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();

            ViewBag.icb_tptramite_prospecto =
                context.icb_tptramite_prospecto.OrderBy(x => x.tptrapros_descripcion).ToList();
            ViewBag.nombreEnlaces = parametrosBusqueda.EnlacesBusqueda(6);
            ViewBag.ciu_id = new SelectList(context.nom_ciudad.OrderBy(x => x.ciu_nombre).Where(x => x.ciu_estado),
                "ciu_id", "ciu_nombre", usuarioBuscado.ciu_id);
            ViewBag.dpto_id =
                new SelectList(context.nom_departamento.OrderBy(x => x.dpto_nombre).Where(x => x.dpto_estado),
                    "dpto_id", "dpto_nombre");

            var buscarBodegas = from bodegas in context.bodega_usuario
                                where bodegas.id_usuario == id
                                select new { bodegas.id_bodega };
            var bodegasString = "";
            var primera = true;
            foreach (var item in buscarBodegas)
                if (primera)
                {
                    bodegasString += item.id_bodega;
                    primera = !primera;
                }
                else
                {
                    bodegasString += "," + item.id_bodega;
                }

            ViewBag.bodegasSeleccionadas = bodegasString;

            var buscarBodegas2 = from bodegas in context.bodega_usuario_visualizacion
                                where bodegas.id_usuario == id
                                select new { bodegas.id_bodega };
            var bodegasString2 = "";
            var primera2 = true;
            foreach (var item in buscarBodegas2)
                if (primera2)
                {
                    bodegasString2 += item.id_bodega;
                    primera2 = !primera2;
                }
                else
                {
                    bodegasString2 += "," + item.id_bodega;
                }

            ViewBag.bodegasSeleccionadas2 = bodegasString2;

            var buscarTramites = from tramites in context.tipotramiteasesor
                                 where tramites.id_usuario == id
                                 select new { tramites.id_tipotramite };
            var tramitesString = "";
            var primeraTramite = true;
            foreach (var item in buscarTramites)
                if (primeraTramite)
                {
                    tramitesString += item.id_tipotramite;
                    primeraTramite = !primeraTramite;
                }
                else
                {
                    tramitesString += "," + item.id_tipotramite;
                }

            ViewBag.tramitesSeleccionados = tramitesString;
            BuscarFavoritos(menu ?? 0);
            return View(usuarioBuscado);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(UsuarioModel usuario, int? menu)
        {
            var bodegasSeleccionadas = Request["bodccs_cod"];
            var bodegasSeleccionadas2 = Request["bodegas_visualizacion"];

            var tramitesSeleccionados = Request["icb_tptramite_prospecto"];
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(bodegasSeleccionadas))
                {
                    TempData["mensaje_error"] = "Debe asignar minimo una bodega!";
                    listasDesplegables(usuario);
                    ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
                    ViewBag.bodegas_visualizacion = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();

                    ViewBag.icb_tptramite_prospecto =
                        context.icb_tptramite_prospecto.OrderBy(x => x.tptrapros_descripcion).ToList();
                    ViewBag.nombreEnlaces = parametrosBusqueda.EnlacesBusqueda(6);
                    ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                    ViewBag.tramitesSeleccionados = tramitesSeleccionados;
                    ViewBag.ciu_id =
                        new SelectList(context.nom_ciudad.OrderBy(x => x.ciu_nombre).Where(x => x.ciu_estado), "ciu_id",
                            "ciu_nombre");
                    ViewBag.dpto_id =
                        new SelectList(context.nom_departamento.OrderBy(x => x.dpto_nombre).Where(x => x.dpto_estado),
                            "dpto_id", "dpto_nombre");

                    BuscarFavoritos(menu ?? 0);
                    return View(usuario);
                }

                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                var nom = (from a in context.users
                           where a.user_numIdent == usuario.user_numIdent || a.user_id == usuario.user_id ||
                                 a.user_usuario == usuario.user_usuario
                           select a.user_numIdent).Count();

                if (nom == 1)
                {
                    var buscarUsuario = context.users.FirstOrDefault(x => x.user_id == usuario.user_id);
                    buscarUsuario.tpdoc_id = usuario.tpdoc_id;
                    buscarUsuario.user_numIdent = usuario.user_numIdent;
                    buscarUsuario.user_nombre = usuario.user_nombre;
                    buscarUsuario.user_apellido = usuario.user_apellido;
                    buscarUsuario.rol_id = usuario.rol_id;
                    buscarUsuario.ciu_id = usuario.ciu_id;
                    buscarUsuario.dpto_id = usuario.dpto_id;
                    buscarUsuario.user_email = usuario.user_email;
                    buscarUsuario.user_telefono = usuario.user_telefono;
                    buscarUsuario.user_direccion = usuario.user_direccion;
                    buscarUsuario.user_usuario = usuario.user_usuario;
                    buscarUsuario.user_estado = usuario.user_estado;
                    buscarUsuario.user_razoninactivo = usuario.user_razoninactivo;
                    
                    buscarUsuario.userfec_actualizacion = DateTime.Now;
                    buscarUsuario.userid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    buscarUsuario.aut_repuestos = usuario.aut_repuestos;
                    if (!string.IsNullOrWhiteSpace(usuario.fechainiplanta))
                    {
                        buscarUsuario.fechainiplanta = DateTime.Parse(usuario.fechainiplanta,miCulturaTime);
                    }
                    if (!string.IsNullOrWhiteSpace(usuario.fechafinplanta))
                    {
                        buscarUsuario.fechafinplanta = DateTime.Parse(usuario.fechafinplanta, miCulturaTime);
                    }
                    context.Entry(buscarUsuario).State = EntityState.Modified;
                    var guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        // Se agregan los tipos de tramites correspondientes al usuario que se va a crear

                        if (!string.IsNullOrEmpty(tramitesSeleccionados))
                        {
                            const string query = "DELETE FROM [dbo].[tipotramiteasesor] WHERE [id_usuario]={0}";
                            var rows = context.Database.ExecuteSqlCommand(query, usuario.user_id);
                            var tramitesId = tramitesSeleccionados.Split(',');
                            foreach (var substring in tramitesId)
                                context.tipotramiteasesor.Add(new tipotramiteasesor
                                {
                                    id_tipotramite = Convert.ToInt32(substring),
                                    id_usuario = usuario.user_id
                                });
                        }


                        if (!string.IsNullOrEmpty(bodegasSeleccionadas))
                        {
                            const string query = "DELETE FROM [dbo].[bodega_usuario] WHERE [id_usuario]={0}";
                            var rows = context.Database.ExecuteSqlCommand(query, usuario.user_id);
                            var bodegasId = bodegasSeleccionadas.Split(',');
                            foreach (var substring in bodegasId)
                                context.bodega_usuario.Add(new bodega_usuario
                                {
                                    id_bodega = Convert.ToInt32(substring),
                                    id_usuario = usuario.user_id
                                });
                            var guardarBodegas = context.SaveChanges();
                        }

                        if (!string.IsNullOrEmpty(bodegasSeleccionadas2))
                        {
                            const string query = "DELETE FROM [dbo].[bodega_usuario_visualizacion] WHERE [id_usuario]={0}";
                            var rows = context.Database.ExecuteSqlCommand(query, usuario.user_id);
                            var bodegasId = bodegasSeleccionadas2.Split(',');
                            foreach (var substring in bodegasId)

                                context.bodega_usuario_visualizacion.Add(new bodega_usuario_visualizacion
                                {
                                    id_bodega = Convert.ToInt32(substring),
                                    id_usuario = usuario.user_id
                                });
                            var guardarBodegas = context.SaveChanges();
                        }

                        TempData["mensaje"] = "La actualización del usuario fue exitoso!";
                    }

                    listasDesplegables(usuario);
                    ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
                    ViewBag.bodegas_visualizacion = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();

                    ViewBag.icb_tptramite_prospecto =
                        context.icb_tptramite_prospecto.OrderBy(x => x.tptrapros_descripcion).ToList();
                    ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                    ViewBag.bodegasSeleccionadas2 = bodegasSeleccionadas2;

                    ViewBag.tramitesSeleccionados = tramitesSeleccionados;
                    ViewBag.nombreEnlaces = parametrosBusqueda.EnlacesBusqueda(6);
                    ViewBag.ciu_id =
                        new SelectList(context.nom_ciudad.OrderBy(x => x.ciu_nombre).Where(x => x.ciu_estado), "ciu_id",
                            "ciu_nombre");
                    ViewBag.dpto_id =
                        new SelectList(context.nom_departamento.OrderBy(x => x.dpto_nombre).Where(x => x.dpto_estado),
                            "dpto_id", "dpto_nombre");
                    ConsultaDatosCreacion(usuario.user_id);
                    usuario.userfec_actualizacion = buscarUsuario.userfec_actualizacion;
                    if (buscarUsuario.user_estado)
                    {
                        var diahoy = DateTime.Now.Date;
                        var cuantosintentos = context.intentos_fallidos_login
                            .Where(d => d.id_usuario == buscarUsuario.user_id && d.fecha >= diahoy).ToList();
                        if (cuantosintentos.Count() > 0)
                        {
                            foreach (var item in cuantosintentos) context.Entry(item).State = EntityState.Deleted;
                            context.SaveChanges();
                        }
                    }

                    BuscarFavoritos(menu ?? 0);
                    return View(usuario);
                }

                TempData["mensaje_error"] =
                    "El número de documento y/o usuario que ingreso ya se encuentra, por favor valide!";
            }

            listasDesplegables(usuario);
            ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
            ViewBag.icb_tptramite_prospecto =
                context.icb_tptramite_prospecto.OrderBy(x => x.tptrapros_descripcion).ToList();
            ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
            ViewBag.nombreEnlaces = parametrosBusqueda.EnlacesBusqueda(6);
            ViewBag.ciu_id = new SelectList(context.nom_ciudad.OrderBy(x => x.ciu_nombre).Where(x => x.ciu_estado),
                "ciu_id", "ciu_nombre");
            ViewBag.dpto_id =
                new SelectList(context.nom_departamento.OrderBy(x => x.dpto_nombre).Where(x => x.dpto_estado),
                    "dpto_id", "dpto_nombre");
            ConsultaDatosCreacion(usuario.user_id);
            ViewBag.bodegas_visualizacion = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();

            BuscarFavoritos(menu ?? 0);
            return View(usuario);
        }

        public void listasDesplegables(UsuarioModel usuario)
        {
            ViewBag.ciu_id = new SelectList(context.nom_ciudad.OrderBy(x => x.ciu_nombre).ToList(), "ciu_id",
                "ciu_nombre", usuario.ciu_id);
            ViewBag.rol_id = new SelectList(context.rols.OrderBy(x => x.rol_nombre).ToList(), "rol_id", "rol_nombre",
                usuario.rol_id);
            ViewBag.tpdoc_id = new SelectList(context.tp_documento.OrderBy(x => x.tpdoc_nombre).ToList(), "tpdoc_id",
                "tpdoc_nombre", usuario.tpdoc_id);


        }

        public void ConsultaDatosCreacion(int id)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            var creador = (from c in context.users
                           join b in context.users on c.user_id equals b.userid_creacion
                           where b.user_id == id
                           select c).FirstOrDefault();

            ViewBag.user_nombre_cre = creador != null ? creador.user_nombre + " " + creador.user_apellido : null;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            var actualizador = (from c in context.users
                                join b in context.users on c.user_id equals b.userid_actualizacion
                                where b.user_id == id
                                select c).FirstOrDefault();

            ViewBag.user_nombre_act =
                actualizador != null ? actualizador.user_nombre + " " + actualizador.user_apellido : null;
        }


        public JsonResult BuscarUsuariosPaginados()
        {
            var datos = (from c in context.users
                         join b in context.rols
                             on c.rol_id equals b.rol_id
                         select new
                         {
                             c.user_id,
                             c.user_nombre,
                             c.user_apellido,
                             c.user_usuario,
                             roluser = b.rol_nombre,
                             c.userfec_creacion,
                             c.userfec_actualizacion,
                             c.user_estado
                         }).ToList();

            var data = datos.Select(c => new datosuser
            {
                user_id = c.user_id,
                user_nombre = c.user_nombre,
                user_apellido = c.user_apellido,
                user_usuario = c.user_usuario,
                roluser = c.roluser,
                //userfec_actualizacion = c.userfec_actualizacion != null ? c.userfec_actualizacion: "",
                // userfec_actualizacion = c.userfec_actualizacion != null ? c.userfec_actualizacion.Value.ToShortDateString() + " " + c.userfec_actualizacion.Value.ToShortTimeString() : "",
                //  userfec_actualizacion = c.userfec_actualizacion != null ? c.userfec_actualizacion.ToString(): "",  // ojo 
                userfec_actualizacion = c.userfec_actualizacion != null
                    ?
                    c.userfec_actualizacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : c.userfec_creacion != null
                        ? c.userfec_creacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                        : "", //!= null ? c.userfec_actualizacion.Value.Year + "/" + c.userfec_actualizacion.Value.Month + "/" + c.userfec_actualizacion.Value.Day  : "",
                estadoUsuario = c.user_estado ? "Activo" : "Inactivo"
            }).ToList();

            // fec_actualizacion = v_creditos.fec_actualizacion!= null ? v_creditos.fec_actualizacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")) : "",
            //var data = context.users.ToList().Select(x => new
            //{
            //    x.user_id,
            //    x.user_nombre,
            //    x.user_apellido,
            //    x.user_usuario,
            //    roluser = x.rol_id,
            //    userfec_actualizacion = x.userfec_actualizacion != null ? x.userfec_actualizacion.Value.ToShortDateString() + " " + x.userfec_actualizacion.Value.ToShortTimeString() : "",
            //    estadoUsuario = x.user_estado == true ? "Activo" : "Inactivo"
            //});

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing) context.Dispose();
            base.Dispose(disposing);
        }


        [HttpGet]
        public ActionResult CambiarContrasena()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CambiarContrasena(CambioContrasenaModel modelo)
        {
            if (ModelState.IsValid)
            {
                var md5 = new MD5CryptoServiceProvider();
                md5.ComputeHash(Encoding.ASCII.GetBytes(modelo.ContrasenaActual));
                var result = md5.Hash;
                var str = new StringBuilder();
                for (var i = 0; i < result.Length; i++) str.Append(result[i].ToString("x2"));
                var pass = str.ToString();
                var iduser = Convert.ToInt32(Session["user_usuarioid"]);
                var validarContrasenaActual =
                    context.users.FirstOrDefault(x => x.user_id == iduser && x.user_password == pass);
                if (validarContrasenaActual != null)
                {
                    if (modelo.ContrasenaNueva != modelo.ConfirmarContrasena)
                    {
                        TempData["mensaje"] = "Las contraseñas nuevas coinciden";
                    }
                    else
                    {
                        md5.ComputeHash(Encoding.ASCII.GetBytes(modelo.ContrasenaNueva));
                        var result2 = md5.Hash;
                        var str2 = new StringBuilder();
                        for (var i = 0; i < result2.Length; i++) str2.Append(result2[i].ToString("x2"));

                        validarContrasenaActual.user_password = str2.ToString();
                        context.Entry(validarContrasenaActual).State = EntityState.Modified;
                        var guardar = context.SaveChanges() > 0;

                        //Guardar en el historico de cambio de clave
                        context.icb_claveslog.Add(new icb_claveslog
                        {
                            clalog_contrasena = str2.ToString(),
                            clalog_fecha = DateTime.Now,
                            id_usuario = iduser
                        });
                        var guardaHistorico = context.SaveChanges() > 0;
                        if (guardar && guardaHistorico)
                            TempData["mensaje"] = "La contraseña se actualizo correctamente";
                        else
                            TempData["mensaje_error"] = "Error de conexion, intente mas tarde";
                    }
                }
                else
                {
                    TempData["mensaje_error"] = "Las contraseña actual no es la correcta";
                }
            }

            return View(modelo);
        }


        public JsonResult BuscarHistoricoSesion(int pagina)
        {
            var historico = new List<HistoricoSesionModel>();
            var idActualUser = Convert.ToInt32(Session["user_usuarioid"]);
            var consultaSesion = context.icb_sessionlog.Where(x => x.id_usuario == idActualUser);
            foreach (var iniciosSesion in consultaSesion)
                historico.Add(new HistoricoSesionModel
                {
                    Fecha = iniciosSesion.fecha_ingreso,
                    Evento = "Sesion Iniciada"
                });
            var consultaClave = context.icb_claveslog.Where(x => x.id_usuario == idActualUser);
            foreach (var cambiosClave in consultaClave)
                historico.Add(new HistoricoSesionModel
                {
                    Fecha = cambiosClave.clalog_fecha,
                    Evento = "Cambio Contraseña"
                });
            var total = historico.Count;
            var registros = historico.OrderByDescending(x => x.Fecha).Select(x => new
            {
                Fecha = x.Fecha != null ? x.Fecha.ToShortDateString() + " " + x.Fecha.ToShortTimeString() : "",
                x.Evento
            }).Skip(pagina * 30 - 30).Take(30);
            var data = new
            {
                registros,
                total
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public class datosuser
        {
            public int user_id { get; set; }
            public string user_nombre { get; set; }
            public string user_apellido { get; set; }
            public string user_usuario { get; set; }
            public string roluser { get; set; }
            public string userfec_actualizacion { get; set; }
            public string estadoUsuario { get; set; }
        }
    }
}