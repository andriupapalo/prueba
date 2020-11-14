using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class restablecerPasswordController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: restablecerPassword
        public ActionResult Index(string id, string llave)
        {
            Session["user_usuario"] = null;
            Session.Abandon();
            Session.Clear();
            Session.RemoveAll();
            int? buscaUsuario = context.GetUsuarioEncriptado(id).FirstOrDefault();

            if (buscaUsuario != null)
            {
                icb_solicitudnuevaclave buscarFechaSolicitud =
                    context.icb_solicitudnuevaclave.FirstOrDefault(x => x.solicitud_llave == llave);
                if (buscarFechaSolicitud != null)
                {
                    TimeSpan diff = DateTime.Now - buscarFechaSolicitud.solicitud_fecha;
                    double hours = diff.TotalHours;
                    if (hours > 0 && hours < 72)
                    {
                        users usuario = context.users.FirstOrDefault(x => x.user_id == buscaUsuario);

                        if (usuario != null && usuario.user_estado)
                        {
                            CambioContrasenaModel modelo = new CambioContrasenaModel
                            {
                                id_usuario = usuario.user_id
                            };
                            return View(modelo);
                        }

                        return RedirectToAction("Error");
                    }

                    return RedirectToAction("Error");
                }
            }

            return RedirectToAction("Error");
        }

        [HttpPost]
        public ActionResult Index(CambioContrasenaModel modelo)
        {
            if (string.IsNullOrEmpty(modelo.ContrasenaNueva) && string.IsNullOrEmpty(modelo.ConfirmarContrasena))
            {
                TempData["mensaje_error"] = "Campos vacios";
                return View(modelo);
            }

            if (modelo.ContrasenaNueva == modelo.ConfirmarContrasena)
            {
                var historico = (from h in context.icb_claveslog
                                 where h.id_usuario == modelo.id_usuario
                                 orderby h.clalog_fecha descending
                                 select new
                                 {
                                     h.clalog_contrasena
                                 }).Take(5).ToList();

                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                md5.ComputeHash(Encoding.ASCII.GetBytes(modelo.ContrasenaNueva));
                byte[] result = md5.Hash;
                StringBuilder str = new StringBuilder();
                for (int i = 0; i < result.Length; i++)
                {
                    str.Append(result[i].ToString("x2"));
                }

                int error = 0;

                //buscamos si la clave nueva ingresada ya ha sido utilizada en las ultimas 5 claves del usuario
                foreach (var item in historico)
                {
                    if (str.ToString() == item.clalog_contrasena)
                    {
                        TempData["mensaje_error"] = "Las contraseña ingresada ya ha sido utilizada previamente";
                        error = 1;
                        break;
                    }
                }

                if (error > 0)
                {
                    return View(modelo);
                }

                users buscaUsuario = context.users.FirstOrDefault(x => x.user_id == modelo.id_usuario);
                if (buscaUsuario != null)
                {
                    buscaUsuario.user_password = str.ToString();
                    buscaUsuario.user_confirPassword = str.ToString();
                    context.Entry(buscaUsuario).State = EntityState.Modified;
                    bool guardar = context.SaveChanges() > 0;

                    //Guardar en el historico de cambio de clave
                    context.icb_claveslog.Add(new icb_claveslog
                    {
                        clalog_contrasena = str.ToString(),
                        clalog_fecha = DateTime.Now,
                        id_usuario = buscaUsuario.user_id
                    });
                    bool guardaHistorico = context.SaveChanges() > 0;

                    if (guardar && guardaHistorico)
                    {
                        TempData["mensaje"] = "Cambio de contraseña exitoso";
                    }
                }
            }
            else
            {
                TempData["mensaje_error"] = "Las contraseñas no son iguales";
                return View(modelo);
            }

            return View();
        }

        public ActionResult Error()
        {
            return View();
        }
    }
}