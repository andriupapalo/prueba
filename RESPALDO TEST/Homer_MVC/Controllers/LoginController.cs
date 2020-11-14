using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class LoginController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: IniciarSesion
        public ActionResult login()
        {
            if (TempData["mesCerrado"] != null)
            {
                CerrarSesion();
                ViewBag.mesCerrado = true;
            }
            else
            {
                ViewBag.mesCerrado = false;
            }

            return View();
        }

        private int ConsultarDiasCambioContrasena(users usuario)
        {
            int id = usuario.user_id;
            int dias = 0;

            icb_claveslog UltimoCambio = context.icb_claveslog.OrderByDescending(x => x.clalog_id)
                .FirstOrDefault(x => x.id_usuario == id);
            if (UltimoCambio != null)
            {
                var calcularDias = (from cambioClave in context.icb_claveslog
                                    where cambioClave.id_usuario == id
                                    select new
                                    {
                                        dias = SqlFunctions.DateDiff("day", UltimoCambio.clalog_fecha, DateTime.Now)
                                    }).FirstOrDefault();
                return calcularDias.dias ?? 0;
            }

            var calcularDiasCreado = (from user in context.users
                                      where user.user_id == id
                                      select new
                                      {
                                          dias = SqlFunctions.DateDiff("day", user.userfec_creacion, DateTime.Now)
                                      }).FirstOrDefault();
            dias = calcularDiasCreado.dias ?? 0;
            return dias;
        }

        //Valida lo que ingreso en la base de datos y verifica que usuario y contraseña este correctamente y pueda ingresar al sistema
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult login(LoginModel objUsu)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                    md5.ComputeHash(Encoding.ASCII.GetBytes(objUsu.user_password));
                    byte[] result = md5.Hash;
                    StringBuilder str = new StringBuilder();
                    for (int i = 0; i < result.Length; i++)
                    {
                        str.Append(result[i].ToString("x2"));
                    }

                    string pass = str.ToString();
                    users obj = context.users
                        .Where(a => a.user_usuario.Equals(objUsu.user_usuario) && a.user_password.Equals(pass))
                        .FirstOrDefault();
                    if (obj != null)
                    {
                        Session["user_usuario"] = obj.user_nombre;
                        Session["user_usuarioid"] = obj.user_id.ToString();
                        Session["user_rolid"] = obj.rol_id.ToString();

                        if (Session["user_usuario"] == null && obj == null)
                        {
                            CerrarSesion();
                            Session.Abandon();
                        }
                        else if (Session["user_usuario"] != null && obj != null && obj != null && obj.user_estado)
                        {
                            //borro los intentos fallidos de acceso del usuario del día de hoy
                            DateTime diahoy = DateTime.Now.Date;

                            System.Collections.Generic.List<intentos_fallidos_login> cuantosintentos = context.intentos_fallidos_login
                                .Where(d => d.id_usuario == obj.user_id && d.fecha >= diahoy).ToList();
                            if (cuantosintentos.Count() > 0)
                            {
                                foreach (intentos_fallidos_login item in cuantosintentos)
                                {
                                    context.Entry(item).State = EntityState.Deleted;
                                }

                                context.SaveChanges();
                            }

                            //pregunto si va a cambiarcontraseña
                            if (obj.cambio_contrasena)
                            {
                                TempData["claveExpiro"] = "Debe cambiar la contraseña para poder ingresar al sistema";
                                return View();
                            }

                            //Cuando el usuario escribe sus credenciales correctamente
                            // Primero buscamos la ultima conexion que ha tenido antes de agregar la aque abrira en este momento
                            int idUsuarioActual = Convert.ToInt32(Session["user_usuarioid"]);
                            icb_sessionlog buscaAcceso = context.icb_sessionlog.Where(x => x.id_usuario == idUsuarioActual)
                                .OrderByDescending(x => x.fecha_ingreso).FirstOrDefault();
                            if (buscaAcceso != null)
                            {
                                Session["user_ultimoacceso"] =
                                    buscaAcceso.fecha_ingreso.ToShortDateString() + " " +
                                    buscaAcceso.fecha_ingreso.ToShortTimeString();
                            }
                            else
                            {
                                Session["user_ultimoacceso"] = "";
                            }

                            context.icb_sessionlog.Add(new icb_sessionlog
                            {
                                estado = true,
                                fecha_ingreso = DateTime.Now,
                                fecha_fin = DateTime.Now,
                                id_usuario = Convert.ToInt32(Session["user_usuarioid"])
                            });
                            context.SaveChanges();

                            //var buscarBodegasUsuario = context.bodega_usuario.Where(x => x.id_usuario == obj.user_id).ToList();
                            int va = 0;
                            var buscarBodegasUsuario = (from bodegaUsuario in context.bodega_usuario
                                                        join bodega in context.bodega_concesionario
                                                            on bodegaUsuario.id_bodega equals bodega.id
                                                        where bodegaUsuario.id_usuario == obj.user_id
                                                        select new
                                                        {
                                                            bodegaUsuario.id_bodega,
                                                            bodega.bodccs_nombre
                                                        }).ToList();
                            if (buscarBodegasUsuario.Count == 0)
                            {
                                if (va == 0)
                                {
                                    TempData["mensajeError"] =
                                        "El usuario no está asignado a ninguna bodega";
                                    return RedirectToAction("login", "Login");
                                }
                            }
                            else if (buscarBodegasUsuario.Count > 1)
                            {
                                TempData["variasBodegas"] = "Usuario asignado a varias bodegas";
                                return View();
                            }
                            

                            Session["user_bodegaNombre"] = buscarBodegasUsuario.Count > 0
                                ? buscarBodegasUsuario.FirstOrDefault().bodccs_nombre
                                : null;
                            Session["user_bodega"] = buscarBodegasUsuario.Count > 0
                                ? buscarBodegasUsuario.FirstOrDefault().id_bodega.ToString()
                                : null;

                            va = validarApertura(Convert.ToInt32(Session["user_bodega"]),
                                Convert.ToInt32(Session["user_usuarioid"]));
                            if (va == 0)
                            {
                                TempData["mesCerrado"] =
                                    "Mes se encuentra cerrado y el usuario no tiene permiso para abrirlo";
                                return RedirectToAction("login", "Login");
                            }

                            if (va == 2)
                            {
                                return RedirectToAction("Index", "abrirMes");
                            }

                            #region el mes esta abierto, se validan los roles y el usuario ingresa

                            if (va == 1)
                            {
                                // Validacion cuando el ususario tiene rol de ANFITRION TALLER
                                //if (obj.rol_id == 2024)
                                //{
                                //    return RedirectToAction("inicioAnfitrionTaller", "Inicio");
                                //}

                                //// Validacion cuando el ususario tiene rol de TECNICO
                                //if (obj.rol_id == 1014)
                                //{
                                //    return RedirectToAction("inicioTecnico", "Inicio");
                                //}

                                //// Validacion cuando el ususario tiene rol de PERITO
                                //if (obj.rol_id == 3)
                                //{
                                //    return RedirectToAction("Agendar", "peritaje");
                                //}

                                //// El rol 7 pertenece a anfitriona
                                //if (obj.rol_id == 7)
                                //{
                                //    return RedirectToAction("Create", "prospectos");
                                //}

                                //// Validacion cuando el ususario tiene rol de MENSAJERO
                                //if (obj.rol_id == 2038)
                                //{
                                //    return RedirectToAction("Index", "agendaMensajero");
                                //}

                                //Si rol user quiere decir que es asesor
                                if (obj.rols.clasificacion_rol == 3)
                                {
                                    sesion_logasesor buscarSesion = context.sesion_logasesor.OrderByDescending(x => x.id).Where(x =>
                                            x.user_id == obj.user_id
                                            && x.fecha_inicia.Year == DateTime.Now.Year
                                            && x.fecha_inicia.Month == DateTime.Now.Month &&
                                            x.fecha_inicia.Day == DateTime.Now.Day //&&x.estado==4
                                    ).FirstOrDefault();

                                    if (buscarSesion != null)
                                    {
                                        // Si se encuentra en estado 4 significa que esta desconectado 
                                        if (buscarSesion.estado == 4 ||
                                            buscarSesion.fecha_inicia.Year != DateTime.Now.Year ||
                                            buscarSesion.fecha_inicia.Month != DateTime.Now.Month
                                            || buscarSesion.fecha_inicia.Day != DateTime.Now.Day)
                                        {
                                            sesion_logasesor nuevaSesion = new sesion_logasesor
                                            {
                                                estado = 1,
                                                fecha_inicia = DateTime.Now,
                                                fecha_termina = DateTime.Now,
                                                user_id = obj.user_id,
                                                bodega = Convert.ToInt32(Session["user_bodega"])
                                            };
                                            context.sesion_logasesor.Add(nuevaSesion);
                                            obj.sesion = true;
                                            context.Entry(obj).State = EntityState.Modified;
                                            int guardaLogSesion = context.SaveChanges();
                                            if (guardaLogSesion > 0)
                                            {
                                                sesion_logasesor ultimaSesion = context.sesion_logasesor.OrderByDescending(x => x.id)
                                                    .FirstOrDefault();
                                                Session["id_sesion_asesor"] = ultimaSesion.id;
                                            }

                                            //Session["user_rol"] = "asesor";
                                        }
                                    }
                                    else
                                    {
                                        sesion_logasesor nuevaSesion = new sesion_logasesor
                                        {
                                            estado = 1,
                                            fecha_inicia = DateTime.Now,
                                            fecha_termina = DateTime.Now,
                                            user_id = obj.user_id,
                                            bodega = Convert.ToInt32(Session["user_bodega"])
                                        };
                                        context.sesion_logasesor.Add(nuevaSesion);
                                        context.SaveChanges();
                                        sesion_logasesor ultimaSesion = context.sesion_logasesor.OrderByDescending(x => x.id)
                                            .FirstOrDefault();
                                        Session["id_sesion_asesor"] = ultimaSesion.id;
                                    }

                                    //return RedirectToAction("inicioAsesor", "Inicio");
                                }
                            }

                            #endregion

                            //// Actualmente el valor del rol asesor corresponde al id 4 de la tabla roles
                            //var buscarRolAsesor = context.icb_sysparameter.FirstOrDefault(x=>x.syspar_cod=="P29");
                            //var idRol = buscarRolAsesor != null ? buscarRolAsesor.syspar_value : "4";
                            //var rolUser = obj.rol_id == Convert.ToInt32(idRol) ? true : false;

                            int calcularDias = ConsultarDiasCambioContrasena(obj);
                            rols buscarDiasDelRol = context.rols.FirstOrDefault(x => x.rol_id == obj.rol_id);
                            if (calcularDias > buscarDiasDelRol.dias_expiracion_clave)
                            {
                                TempData["claveExpiro"] = "Contraseña se vencio";
                                return View();
                            }

                            return RedirectToAction("Inicio", "inicio");
                        }
                        else
                        {
                            TempData["mensajeError"] = "Usuario se encuentra inactivo";
                        }
                    }
                    else
                    {
                        // El usuario o la contrasenia esta mal escrito
                        TempData["mensajeError"] = "Usuario o contraseña incorrectos";
                        DateTime fechadehoy = DateTime.Now.Date;

                        //busco el usuario si existe
                        users usuexiste = context.users.Where(d => d.user_usuario == objUsu.user_usuario)
                            .FirstOrDefault();
                        if (usuexiste != null)
                        {
                            intentos_fallidos_login nuevointentofallido = new intentos_fallidos_login
                            {
                                fecha = DateTime.Now,
                                id_usuario = usuexiste.user_id
                            };
                            context.intentos_fallidos_login.Add(nuevointentofallido);
                            int guardado = context.SaveChanges();
                            //veo cuantos intentos tiene el día de hoy;
                            int cuantosintentos = context.intentos_fallidos_login
                                .Where(d => d.id_usuario == usuexiste.user_id && d.fecha >= fechadehoy).Count();
                            if (cuantosintentos > 2)
                            {
                                usuexiste.user_estado = false;
                                context.Entry(usuexiste).State = EntityState.Modified;
                                context.SaveChanges();
                                TempData["mensajeError"] =
                                    "Usuario o contraseña incorrectos. Ha excedido el límite máximo de intentos. Su cuenta ha sido bloqueada. Por favor contacte con un administrador";
                            }
                        }
                    }
                }
            }
            catch (ArgumentNullException)
            {
                TempData["mensajeError"] = "Usuario o contraseña incorrectos";
            }

            return View();
        }

        public JsonResult GetBodegasUsuarioActual()
        {
            int idUsuario = Convert.ToInt32(Session["user_usuarioid"]);
            var bodegasUsuario = (from bodegaUsuario in context.bodega_usuario
                                  join bodega in context.bodega_concesionario
                                      on bodegaUsuario.id_bodega equals bodega.id
                                  where bodegaUsuario.id_usuario == idUsuario
                                  select new { bodega.id, bodega.bodccs_nombre }).ToList();
            return Json(bodegasUsuario, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ValidarBodega(int? id)
        {
            int va = 0;
            string bodegaActualUsuario = Request["bodega_id"];
            Session["user_bodega"] = bodegaActualUsuario;
            Session["user_bodegaNombre"] = Request["bodega_nombre"];

            va = validarApertura(Convert.ToInt32(Session["user_bodega"]), Convert.ToInt32(Session["user_usuarioid"]));

            string rol = Convert.ToString(Session["user_rolid"]);
            if (va == 0)
            {
                TempData["mesCerrado"] = "Mes se encuentra cerrado y el usuario no tiene permiso para abrirlo";
                return RedirectToAction("login", "Login");
            }

            if (va == 2)
            {
                return RedirectToAction("Index", "abrirMes");
            }

            if (va == 1)
            {
                //verifico el tipo de rol
                //if (rol == "3")
                //{
                //    return RedirectToAction("Agendar", "peritaje");
                //}

                //if (rol == "2024")
                //{
                //    return RedirectToAction("inicioAnfitrionTaller", "Inicio");
                //}
                //veo si el rol es productivo
                var idrl = Convert.ToInt32(rol);
                var rolx = context.rols.Where(d => d.rol_id == idrl).FirstOrDefault();
                //if (rol == "4" || rol == "2030")
                if (rolx.clasificacion_rol==3)
                {
                    int usuarioActualId = Convert.ToInt32(Session["user_usuarioid"]);
                    sesion_logasesor buscarSesion = context.sesion_logasesor.OrderByDescending(x => x.fecha_inicia).Where(x =>
                            x.user_id == usuarioActualId
                            && x.fecha_inicia.Year == DateTime.Now.Year
                            && x.fecha_inicia.Month == DateTime.Now.Month &&
                            x.fecha_inicia.Day == DateTime.Now.Day //&&x.estado==4
                    ).FirstOrDefault();

                    if (buscarSesion != null)
                    {
                        if (buscarSesion.bodega.ToString() != bodegaActualUsuario)
                        {
                            sesion_logasesor nuevaSesion = new sesion_logasesor
                            {
                                estado = buscarSesion.estado,
                                fecha_inicia = DateTime.Now,
                                fecha_termina = DateTime.Now,
                                user_id = usuarioActualId,
                                bodega = bodegaActualUsuario != null ? Convert.ToInt32(bodegaActualUsuario) : 0
                            };
                            context.sesion_logasesor.Add(nuevaSesion);
                            int guardaLogSesion = context.SaveChanges();
                            if (guardaLogSesion > 0)
                            {
                                sesion_logasesor ultimaSesion = context.sesion_logasesor.OrderByDescending(x => x.id)
                                    .FirstOrDefault();
                                Session["id_sesion_asesor"] = ultimaSesion.id;
                            }
                        }

                        // Si se encuentra en estado 4 significa que esta desconectado 
                        if (buscarSesion.estado == 4 || buscarSesion.fecha_inicia.Year != DateTime.Now.Year ||
                            buscarSesion.fecha_inicia.Month != DateTime.Now.Month
                            || buscarSesion.fecha_inicia.Day != DateTime.Now.Day)
                        {
                            sesion_logasesor nuevaSesion = new sesion_logasesor
                            {
                                estado = 1,
                                fecha_inicia = DateTime.Now,
                                fecha_termina = DateTime.Now,
                                user_id = usuarioActualId,
                                bodega = bodegaActualUsuario != null ? Convert.ToInt32(bodegaActualUsuario) : 0
                            };
                            context.sesion_logasesor.Add(nuevaSesion);
                            //obj.sesion = true;
                            //context.Entry(obj).State = EntityState.Modified;
                            int guardaLogSesion = context.SaveChanges();
                            if (guardaLogSesion > 0)
                            {
                                sesion_logasesor ultimaSesion = context.sesion_logasesor.OrderByDescending(x => x.id)
                                    .FirstOrDefault();
                                Session["id_sesion_asesor"] = ultimaSesion.id;
                            }

                            //Session["user_rol"] = "asesor";
                        }
                    }
                    else
                    {
                        sesion_logasesor nuevaSesion = new sesion_logasesor
                        {
                            estado = 1,
                            fecha_inicia = DateTime.Now,
                            fecha_termina = DateTime.Now,
                            user_id = usuarioActualId,
                            bodega = bodegaActualUsuario != null ? Convert.ToInt32(bodegaActualUsuario) : 0
                        };
                        context.sesion_logasesor.Add(nuevaSesion);
                        context.SaveChanges();
                        sesion_logasesor ultimaSesion = context.sesion_logasesor.OrderByDescending(x => x.id).FirstOrDefault();
                        Session["id_sesion_asesor"] = ultimaSesion.id;
                    }

                  //  return RedirectToAction("inicioAsesor", "Inicio");
                }

                //if (rol == "7")
                //{
                //    return RedirectToAction("Create", "prospectos");
                //}
            }

            return RedirectToAction("inicio", "Inicio");
        }

        public JsonResult RecordarContrasena(string nombreUsuario)
        {
            configuracion_envio_correos correoconfig = context.configuracion_envio_correos.Where(d => d.activo).FirstOrDefault();

            bool guardar = false;
            users buscaUsuario = context.users.FirstOrDefault(x => x.user_usuario == nombreUsuario);
            if (buscaUsuario != null)
            {
                if (buscaUsuario.user_estado)
                {
                    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                    md5.ComputeHash(Encoding.ASCII.GetBytes(nombreUsuario));
                    byte[] result = md5.Hash;
                    StringBuilder str = new StringBuilder();
                    for (int i = 0; i < result.Length; i++)
                    {
                        str.Append(result[i].ToString("x2"));
                    }

                    Random rdm = new Random();
                    int aleatorio = rdm.Next(1, 1000);
                    string llave = DateTime.Now.ToString() + aleatorio;
                    md5.ComputeHash(Encoding.ASCII.GetBytes(llave));
                    byte[] resultLlave = md5.Hash;
                    StringBuilder strLlave = new StringBuilder();
                    for (int i = 0; i < resultLlave.Length; i++)
                    {
                        strLlave.Append(resultLlave[i].ToString("x2"));
                    }

                    icb_solicitudnuevaclave crearLlave = context.icb_solicitudnuevaclave.Add(new icb_solicitudnuevaclave
                    {
                        solicitud_fecha = DateTime.Now,
                        solicitud_llave = strLlave.ToString(),
                        solucitud_usuario = nombreUsuario
                    });
                    bool guardarLlave = context.SaveChanges() > 0;
                    if (guardarLlave)
                    {
                        //Esto es para enviar el correo, temporalmente se esta guardando en base de datos para probar el enlace
                        string urlActual0 = Request.Url.Scheme + "://" + Request.Url.Authority;
                        //var lastFolder = Path.GetDirectoryName(urlActual0);
                        //var pathWithoutLastFolder = Path.GetDirectoryName(lastFolder);
                        string url = urlActual0 += @"/restablecerPassword/Index?id=" + str + "&&llave=" + strLlave;

                        string body = url;

                        context.envio_correo_test.Add(new envio_correo_test
                        {
                            correo_envio = buscaUsuario.user_email,
                            usuario = nombreUsuario,
                            body = body
                        });
                    }

                    guardar = context.SaveChanges() > 0;

                    try
                    {
                        var enlace = (from n in context.icb_solicitudnuevaclave
                                      join e in context.envio_correo_test
                                          on n.solicitud_id equals e.id_tabla
                                      orderby n.solicitud_fecha descending
                                      where e.correo_envio == buscaUsuario.user_email
                                      select new
                                      {
                                          e.body
                                      }).FirstOrDefault();

                        string enlaces = enlace.body.Replace("<a href='", "");
                        enlaces = enlaces.Replace("'></a>", "");

                        int aniocorreo = DateTime.Now.Year;
                        users user_destinatario = context.users.Find(buscaUsuario.user_id);
                        //var user_remitente = context.users.Find(usuario_actual);

                        //MailAddress de = new MailAddress("noti@iceberg.com.co", "Notificación Iceberg");
                        MailAddress de = new MailAddress(correoconfig.correo, correoconfig.nombre_remitente);
                        MailAddress para = new MailAddress(user_destinatario.user_email,
                            user_destinatario.user_nombre + " " + user_destinatario.user_apellido);
                        MailMessage mensaje = new MailMessage(de, para);
                        //texto del mensaje de correo
                        string titulocorreo = "Restauración de Contraseña";
                        string asuntocorreo =
                            "<p>Hemos recibido una solicitud para reestablecer tu contraseña, por favor haz click en el siguiente boton</p><br />";
                        string textoinicial = "";

                        textoinicial +=
                            "<a style='background-color: #1A6AA2; color: white; padding: 7px 12px; text-align: center; text-decoration: none; display: inline-block;' href=" +
                            enlaces + ">Reestablecer</a>";
                        textoinicial +=
                            "<p>Por motivos de seguridad, el vinculo caducará 72 horas despues de su envío.</p>";
                        string html2 =
                            @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
                        <html xmlns='http://www.w3.org/1999/xhtml'>
                        <head>
                            <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
                            <title>Notificacion Iceberg Email</title>
                            <style type='text/css'>
                                body {margin: 0; padding: 0; min-width: 100%!important;}
                           img {height: auto;}
                          .content {width: 100%; max-width: 600px;}
                          .header {padding: 40px 30px 20px 30px;}
                          .innerpadding {padding: 30px 30px 30px 30px;}
                          .borderbottom {border-bottom: 1px solid #f2eeed;}
                          .subhead {font-size: 15px; color: #ffffff; font-family: sans-serif; letter-spacing: 10px;}
                          .h1, .h2, .bodycopy {color: #153643; font-family: sans-serif;}
                          .h1 {font-size: 33px; line-height: 38px; font-weight: bold;}
                          .h2 {padding: 0 0 15px 0; font-size: 24px; line-height: 28px; font-weight: bold;}
                  
                          .bodycopy {font-size: 16px; line-height: 22px;}
                          .button {text-align: center; font-size: 18px; font-family: sans-serif; font-weight: bold; padding: 0 30px 0 30px;}
                          .button a {color: #ffffff; text-decoration: none;}
                          .footer {padding: 20px 30px 15px 30px;}
                          .footercopy {font-family: sans-serif; font-size: 14px; color: #ffffff;}
                          .footercopy a {color: #ffffff; text-decoration: underline;}
                          .unsubscribe {display: block; margin-top: 20px; padding: 10px 50px; background: #2f3942; border-radius: 5px; text-decoration: none!important; font-weight: bold;}
                          .hide {display: none!important;}
                          @media only screen and (max-width: 550px), screen and (max-device-width: 550px) {
                          body[yahoo] .hide {display: none!important;}
                          body[yahoo] .buttonwrapper {background-color: transparent!important;}
                          body[yahoo] .button {padding: 0px!important;}
                          body[yahoo] .button a {background-color: #e05443; padding: 15px 15px 13px!important;}
                          body[yahoo] .unsubscribe {display: block; margin-top: 20px; padding: 10px 50px; background: #2f3942; border-radius: 5px; text-decoration: none!important; font-weight: bold;}
                          }
                            </style>
                        </head>
                        <body yahoo=yahoo bgcolor='#ffffff'>

                            <img src='cid:logoiceberg' alt='Alternate Text' style='width: 150px'/>
                        <br/>
                            <table width='600px' bgcolor='#ffffff' border='0' cellpadding='0' cellspacing='0'>
                            <tr>
                                <td class='innerpadding borderbottom'>
                                    <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                        <tr>
                                            <td class='bodycopy'><b>" + titulocorreo + @"</b>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class='bodycopy'>
                                               <b> Hola, " + user_destinatario.user_nombre + " " +
                            user_destinatario.user_apellido + @"</b><br/>
                                            </td>
                                        </tr>                                       
                                    </table>
                                </td>
                            </tr>

                            <tr>
                                <td class='innerpadding borderbottom'>
                                    <table width='115' align='left' border='0' cellpadding='0' cellspacing='0'>
                               
                                    </table>
                                    <!--[if (gte mso 9)|(IE)]>
                      <table width='420' align='left' cellpadding='0' cellspacing='0' border='0'>
                        <tr>
                          <td>
                    <![endif]-->
                                    <table class='col380' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%; max-width: 420px;'>
                                        <tr>
                                            <td>
                                                <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                                    <tr>
                                                        <td class='bodycopy'>
                                                            <p>" + asuntocorreo + @"</p>
                                                            <p>" + textoinicial + @"</p>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>
                                    <!--[if (gte mso 9)|(IE)]>
                          </td>
                        </tr>
                    </table>
                    <![endif]-->
                                </td>
                            </tr>

                            <tr>
                                <td class='footer' bgcolor='#09599C'>
                                    <table width='100%' border='0' cellspacing='0' cellpadding='0'>
                                        <tr>
                                            <td align='center' class='footercopy'>
                                                &reg; ICEBERG, " + aniocorreo +
                            @"<br />                                       
                                            </td>
                                        </tr>                                
                                    </table>
                                </td>
                            </tr>
                        </table>
                        <!--[if (gte mso 9)|(IE)]>
                                </td>
                            </tr>
                    </table>
                    <![endif]-->

                    </td>
                </tr>
            </table>
        </body>
        </html>
        ";


                        //mensaje.Bcc.Add("liliana.avila@exiware.com");
                        mensaje.Bcc.Add("correospruebaexi2019@gmail.com");
                        //mensaje.Bcc.Add("yor.lopez@exiware.com");
                        mensaje.Subject = "Solicitud cambio de contraseña";
                        //mensaje.ReplyToList.Add("erion23@gmail.com");
                        mensaje.BodyEncoding = Encoding.Default;
                        mensaje.IsBodyHtml = true;
                        LinkedResource theEmailImage6 = new LinkedResource(HostingEnvironment.MapPath("~/Images/icebergerp03.png"),
                            "image/png")
                        {
                            ContentId = "logoiceberg",
                            TransferEncoding = TransferEncoding.Base64
                        };
                        theEmailImage6.ContentType.Name = theEmailImage6.ContentId;
                        theEmailImage6.ContentLink = new Uri("cid:" + theEmailImage6.ContentId);

                        //create Alrternative HTML view
                        //string body = string.Format(html2, theEmailImage.ContentId, theEmailImage2.ContentId, theEmailImage3.ContentId, theEmailImage4.ContentId);
                        AlternateView htmlView = AlternateView.CreateAlternateViewFromString(html2, null, "text/html");
                        htmlView.LinkedResources.Add(theEmailImage6);

                        mensaje.AlternateViews.Add(htmlView);
                        //mensaje.Body = html2;
                        //set the Email subject
                        mensaje.Subject = "Recuperar Contraseña Iceberg";
                        //var html = "";
                        //var a = Request.Url.Scheme + "://" + Request.Url.Authority + "/Images/icebergerp03.png";
                        //html += "<img src='"+a+"' width='90' height='56'/>";
                        //html += "<h4>Reestauracion de contraseña</h4>";
                        //html += "<p>Hola "+ user_destinatario.user_nombre + " " + user_destinatario.user_apellido + "</p><br>";
                        //html += "<p>Hemos recibido un solicitud para reestablecer tu contraseña, por favor da click en el siguiente boton</p><br />";
                        //html += "<a style='background-color: #1A6AA2; color: white; padding: 14px 25px; text-align: center; text-decoration: none; display: inline-block;' href=" +enlaces+ ">Reestablecer</a>";
                        //html += "<p>Por motivos de seguridad, el vinculo caducara 72 horas despues de su envio.</p>";
                        //mensaje.Body = html;

                        //SmtpClient cliente = new SmtpClient("smtp.mi.com.co");
                        SmtpClient cliente = new SmtpClient(correoconfig.smtp_server)
                        {
                            Port = correoconfig.puerto,
                            UseDefaultCredentials = false,
                            Credentials = new NetworkCredential(correoconfig.usuario, correoconfig.password),
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network
                        };
                        cliente.Send(mensaje);

                        var data = new
                        {
                            tipo = "success",
                            mensaje = "Mensaje enviado exitosamente"
                        };
                        return Json(data, JsonRequestBehavior.AllowGet);
                        // procesado = true;
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message;
                        var data = new
                        {
                            tipo = "error",
                            mensaje = error
                        };
                        return Json(data, JsonRequestBehavior.AllowGet);
                    }
                }

                {
                    var data = new
                    {
                        tipo = "error",
                        mensaje =
                            "El usuario se encuentra bloqueado. Por favor contacte con el administrador del sistema"
                    };
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var data = new
                {
                    tipo = "error",
                    mensaje = "No existe un usuario con el nombre de usuario suministrado. Por favor valide"
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            //return Json(guardar, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CambiarClaveExpiro(string usuario, string contrasena, string contrasenaNueva,
            string repetirContrasena)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            md5.ComputeHash(Encoding.ASCII.GetBytes(contrasena));
            byte[] result0 = md5.Hash;
            StringBuilder str0 = new StringBuilder();
            for (int i = 0; i < result0.Length; i++)
            {
                str0.Append(result0[i].ToString("x2"));
            }

            string contrasenaActual = str0.ToString();

            users buscaUsuario = context.users.FirstOrDefault(a =>
                a.user_usuario.Equals(usuario) && a.user_password.Equals(contrasenaActual));
            int error = 0;
            if (buscaUsuario != null)
            {
                if (contrasenaNueva == repetirContrasena && !string.IsNullOrEmpty(contrasenaNueva))
                {
                    md5.ComputeHash(Encoding.ASCII.GetBytes(contrasenaNueva));
                    byte[] result = md5.Hash;
                    StringBuilder str = new StringBuilder();
                    for (int i = 0; i < result.Length; i++)
                    {
                        str.Append(result[i].ToString("x2"));
                    }

                    buscaUsuario.user_password = str.ToString();
                    buscaUsuario.user_confirPassword = str.ToString();
                    buscaUsuario.cambio_contrasena = false;
                    context.Entry(buscaUsuario).State = EntityState.Modified;
                    bool guardarUsuario = context.SaveChanges() > 0;

                    //Guardar en el historico de cambio de clave
                    context.icb_claveslog.Add(new icb_claveslog
                    {
                        clalog_contrasena = str.ToString(),
                        clalog_fecha = DateTime.Now,
                        id_usuario = buscaUsuario.user_id
                    });
                    bool guardaHistorico = context.SaveChanges() > 0;

                    if (guardaHistorico && guardarUsuario)
                    {
                        error = 1;
                    }
                }
                //Error cuando las contraseñas nueva no esta escrita igual en los dos campos
                else
                {
                    error = -1;
                }
            }
            else
            {
                //Error cuando la contraseña actual esta mal escrita
                error = -2;
            }

            return Json(error, JsonRequestBehavior.AllowGet);
        }

        //Cierra la sesion la limipia y asi pueda retornar a login y no pueda navegar dentro del sistema
        public ActionResult CerrarSesion()
        {
            //var idUsuarioActual = Convert.ToInt32(Session["user_usuarioid"]);
            //var buscarUsuario = context.users.FirstOrDefault(x=>x.user_id==idUsuarioActual);
            //if (buscarUsuario!=null) {
            //    if (Session["id_sesion_asesor"]!=null) {
            //        var id_sesion_asesor = Convert.ToInt32(Session["id_sesion_asesor"]);
            //        var buscaSesionAsesor = context.sesion_logasesor.FirstOrDefault(x=>x.id==id_sesion_asesor);
            //        if (buscaSesionAsesor!=null) {
            //            buscaSesionAsesor.fecha_termina = DateTime.Now;
            //            context.Entry(buscaSesionAsesor).State = EntityState.Modified;
            //        }
            //    }
            //    buscarUsuario.sesion = false;
            //    context.Entry(buscarUsuario).State = EntityState.Modified;
            //    context.SaveChanges();
            //}
            int idUsuarioActual = Convert.ToInt32(Session["user_usuarioid"]);
            if (idUsuarioActual != 0)
            {
                sesion_logasesor lastConection = context.sesion_logasesor.OrderByDescending(x => x.fecha_inicia)
                    .FirstOrDefault(x => x.user_id == idUsuarioActual);
                if (lastConection != null)
                {
                    lastConection.fecha_termina = DateTime.Now;
                    lastConection.estado = 4; // 4 es el estado en db como desconectado
                    context.Entry(lastConection).State = EntityState.Modified;
                    int guardar = context.SaveChanges();
                }
            }


            Session["user_usuario"] = null;
            Session.Abandon();
            Session.Clear();
            Session.RemoveAll();
            return RedirectToAction("Login", "login");
        }

        public static int validarApertura(int bodega, int usuario)
        {
            Iceberg_Context context2 = new Iceberg_Context();
            int mesActual = DateTime.Now.Month;

            int apertura = (from mb in context2.mesactualbodega
                            where mb.idbodega == bodega && mb.mesacual == mesActual
                            select mb).Count();

            //var usuario = Convert.ToInt32(Session["user_usuarioid"]);
            //Validamos que el usuario loguado tenga el rol y el permiso para ver toda la informacion
            int permiso = (from u in context2.users
                           join r in context2.rols
                               on u.rol_id equals r.rol_id
                           join ra in context2.rolacceso
                               on r.rol_id equals ra.idrol
                           where u.user_id == usuario && ra.idpermiso == 17
                           select new
                           {
                               u.user_id,
                               u.rol_id,
                               r.rol_nombre,
                               ra.idpermiso
                           }).Count();

            if (apertura > 0)
            {
                return 1;
            }

            if (permiso > 0)
            {
                return 2;
            }

            return 0;

            //return Json(new { data = apertura, autorizado = permiso}, JsonRequestBehavior.AllowGet);
        }
    }
}