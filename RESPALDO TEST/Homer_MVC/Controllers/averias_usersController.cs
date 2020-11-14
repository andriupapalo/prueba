using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class averias_usersController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        public void listas(usuarios_autorizaciones averias)
        {
            //var list = (from u in db.users
            //            select new
            //            {
            //                u.user_nombre,
            //                u.user_apellido,
            //                u.user_id
            //            }).ToList();

            //var lista = new List<SelectListItem>();
            //foreach (var item in list)
            //{
            //    lista.Add(new SelectListItem()
            //    {
            //        Text = item.user_nombre + ' ' + item.user_apellido,
            //        Value = item.user_id.ToString(),
            //        Selected = item.user_id == averias.user_id ? true : false
            //    });
            //}

            //ViewBag.user_id = lista;
            ViewBag.tipoautorizacion = new SelectList(db.tipoautorizacion.OrderBy(x => x.descripcion), "id",
                "descripcion", averias.tipoautorizacion);
            ViewBag.bodega_id = new SelectList(db.bodega_concesionario.OrderBy(x => x.bodccs_nombre), "id",
                "bodccs_nombre", averias.bodega_id);
        }


        // GET: averias_users
        //public ActionResult Browser()
        //{
        //    var averias_users = db.usuarios_autorizaciones.Include(a => a.bodega_concesionario).Include(a => a.users);
        //    return View(averias_users.ToList());
        //}


        // GET: averias_users/Create
        public ActionResult Create(int? menu)
        {
            usuarios_autorizaciones averias = new usuarios_autorizaciones();
            listas(averias);
            BuscarFavoritos(menu);
            return View();
        }


        public JsonResult BuscarUsuariosPorBodega(int? id)
        {
            var buscarUsuarios = (from bodegaUsuario in db.bodega_usuario
                                  join bodega in db.bodega_concesionario
                                      on bodegaUsuario.id_bodega equals bodega.id
                                  join usuario in db.users
                                      on bodegaUsuario.id_usuario equals usuario.user_id
                                  where bodegaUsuario.id_bodega == id
                                  select new
                                  {
                                      usuario.user_id,
                                      nombreCompleto = usuario.user_nombre + " " + usuario.user_apellido
                                  }).Distinct().OrderBy(x => x.nombreCompleto).ToList();

            return Json(buscarUsuarios, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarUsuariosAutorizados()
        {
            var buscarUsuarios = (from usuariosAut in db.usuarios_autorizaciones
                                  join usuario in db.users
                                      on usuariosAut.user_id equals usuario.user_id
                                  join bodega in db.bodega_concesionario
                                      on usuariosAut.bodega_id equals bodega.id
                                  join tipoAitorizacion in db.tipoautorizacion
                                      on usuariosAut.tipoautorizacion equals tipoAitorizacion.id
                                  orderby usuario.user_nombre
                                  select new
                                  {
                                      usuariosAut.id,
                                      noombresUsuario = usuario.user_nombre + " " + usuario.user_apellido,
                                      bodega.bodccs_nombre,
                                      tipoAitorizacion.descripcion,
                                      usuariosAut.estado
                                  }).ToList();

            return Json(buscarUsuarios, JsonRequestBehavior.AllowGet);
        }

        // POST: averias_users/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(usuarios_autorizaciones averias_users, int? menu)
        {
            if (ModelState.IsValid)
            {
                usuarios_autorizaciones existe = db.usuarios_autorizaciones.Where(x =>
                    x.user_id == averias_users.user_id && x.bodega_id == averias_users.bodega_id &&
                    x.tipoautorizacion == averias_users.tipoautorizacion).FirstOrDefault();
                if (existe == null)
                {
                    averias_users.fecha_creacion = DateTime.Now;
                    averias_users.user_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    db.usuarios_autorizaciones.Add(averias_users);
                    db.SaveChanges();
                    TempData["mensaje"] = "Registro ingresado correctamente";
                    listas(averias_users);
                    BuscarFavoritos(menu);
                    return View(averias_users);
                }

                TempData["mensaje_error"] = "El usuario ya existe para la bodega seleccionada, por favor valide";
            }
            else
            {
                TempData["mensaje_error"] = "Error al ingresar los datos, por favor valide";
            }

            listas(averias_users);
            BuscarFavoritos(menu);
            return View(averias_users);
        }

        // GET: averias_users/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            usuarios_autorizaciones averias_users = db.usuarios_autorizaciones.Find(id);
            if (averias_users == null)
            {
                return HttpNotFound();
            }

            listas(averias_users);
            ViewBag.usuarioSeleccionado = averias_users.user_id;
            BuscarFavoritos(menu);
            return View(averias_users);
        }


        // POST: averias_users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(usuarios_autorizaciones averias_users, int? menu)
        {
            if (ModelState.IsValid)
            {
                usuarios_autorizaciones existe = db.usuarios_autorizaciones.FirstOrDefault(x =>
                    x.user_id == averias_users.user_id && x.bodega_id == averias_users.bodega_id &&
                    x.tipoautorizacion == averias_users.tipoautorizacion);
                if (existe == null)
                {
                    averias_users.user_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    averias_users.fecha_actualizacion = DateTime.Now;
                    db.Entry(averias_users).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["mensaje"] = "Registro actualizado correctamente";
                }
                else if (existe != null && existe.id == averias_users.id)
                {
                    existe.user_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    existe.fecha_actualizacion = DateTime.Now;
                    db.Entry(existe).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["mensaje"] = "Registro actualizado correctamente";
                }
                else
                {
                    TempData["mensaje_error"] =
                        "El usuario ya existe para la bodega y autorización seleccionada, por favor valide";
                }
            }

            listas(averias_users);
            ViewBag.usuarioSeleccionado = averias_users.user_id;
            BuscarFavoritos(menu);
            return View(averias_users);
        }


        //averias
        public ActionResult BrowserAutorizaciones(int? menu)
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);
            List<int?> tipoAutorizaciones = db.usuarios_autorizaciones
                .Where(x => x.bodega_id == bodegaActual && x.user_id == usuarioActual &&
                            (x.tipoautorizacion == 1 || x.tipoautorizacion == 2)).Select(x => x.tipoautorizacion)
                .ToList();
            List<autorizaciones> autorizaciones = db.autorizaciones.Include(x => x.icb_vehiculo.icb_vehiculo_eventos)
                .Where(x => tipoAutorizaciones.Contains(x.tipo_autorizacion) && x.bodega == bodegaActual)
                .OrderByDescending(x => x.fecha_creacion).ToList();
            BuscarFavoritos(menu);
            return View(autorizaciones);
        }

        //autorizaciones y envio de notificacion de correos para averias
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BrowserAutorizaciones(int? id_s, int? menu)
        {
            configuracion_envio_correos correoconfig = db.configuracion_envio_correos.Where(d => d.activo).FirstOrDefault();
            int id = Convert.ToInt32(Request["autorizacion_id"]);
            autorizaciones autorizacion = db.autorizaciones.Find(id);
            if (autorizacion == null)
            {
                return HttpNotFound();
            }

            autorizacion.fecha_autorizacion = DateTime.Now;
            autorizacion.user_autorizacion = Convert.ToInt32(Session["user_usuarioid"]);
            autorizacion.autorizado = Convert.ToBoolean(Request["autorizado"]);
            autorizacion.comentario = Request["comentario"];
            db.Entry(autorizacion).State = EntityState.Modified;
            db.SaveChanges();

            int idPedido = db.vpedido.OrderByDescending(x => x.id)
                .FirstOrDefault(x => x.planmayor == autorizacion.plan_mayor).id;
            if (autorizacion.autorizado)
            {
                bitacoraExcepcionFactura bitacora = new bitacoraExcepcionFactura
                {
                    id_excepcion = Convert.ToInt32(Request["excepcion"]),
                    id_Autorizacion = autorizacion.id,
                    id_Bodega = autorizacion.bodega,
                    id_Pedido = idPedido,
                    id_usuario_solicitante = autorizacion.user_creacion,
                    id_usuario_apobador = Convert.ToInt32(Session["user_usuarioid"]),
                    fecha_aprobacion = DateTime.Now
                };
                db.bitacoraExcepcionFactura.Add(bitacora);
            }

            db.SaveChanges();

            TempData["mensaje"] = "Autorización guardada correctamente";

            #region envio de notificacion

            try
            {
                users user_destinatario = db.users.Find(autorizacion.user_creacion);
                users user_remitente = db.users.Find(autorizacion.user_autorizacion);
                string autorizado = autorizacion.autorizado ? "Autorizado" : "No autorizado";

                MailAddress de = new MailAddress(correoconfig.correo, correoconfig.nombre_remitente);
                MailAddress para = new MailAddress(user_destinatario.user_email,
                    user_destinatario.user_nombre + " " + user_destinatario.user_apellido);
                MailMessage mensaje = new MailMessage(de, para);
                mensaje.Bcc.Add("jairo.mateus@exiware.com");
                mensaje.Bcc.Add("correospruebaexi2019@gmail.com");
                mensaje.ReplyToList.Add(new MailAddress(user_remitente.user_email,
                    user_remitente.user_nombre + " " + user_remitente.user_apellido));
                mensaje.Subject = "Respuesta Autorización plan mayor " + autorizacion.icb_vehiculo.plan_mayor;
                mensaje.BodyEncoding = Encoding.Default;
                mensaje.IsBodyHtml = true;
                string html = "";
                html += "<h4>Cordial Saludo</h4><br>";
                html += "<p>Respuesta solicitud autorización de asignación del vehículo con plan mayor <b>" +
                        autorizacion.icb_vehiculo.plan_mayor + "</b> por averia </p><br>";
                html += "<p><b>Fecha de solicitud: </b>" + autorizacion.fecha_creacion + "</p>";
                html += "<p><b>Plan Mayor: </b>" + autorizacion.icb_vehiculo.plan_mayor + "</p>";
                html += "<p><b>Estado Solicitud: " + autorizado + " </b></p>";
                html += "<p><b>Observaciones: </b>" + autorizacion.comentario + "</p>";
                html += "<p><b>Fecha Respuesta: </b>" + autorizacion.fecha_autorizacion + "</p>";
                html += "<p><b>Usuario Autorización: </b>" + user_remitente.user_nombre + " " +
                        user_remitente.user_apellido + "</p>";
                mensaje.Body = html;

                SmtpClient cliente = new SmtpClient(correoconfig.smtp_server)
                {
                    Port = correoconfig.puerto,
                    Credentials = new NetworkCredential(correoconfig.usuario, correoconfig.password),
                    EnableSsl = true
                };
                cliente.Send(mensaje);

                notificaciones envio = new notificaciones
                {
                    user_remitente = autorizacion.user_autorizacion,
                    asunto = "Notificación respuesta autorización por averia",
                    fecha_envio = DateTime.Now,
                    enviado = true,
                    user_destinatario = autorizacion.user_creacion,
                    autorizacion_id = autorizacion.id
                };
                db.notificaciones.Add(envio);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                notificaciones envio = new notificaciones
                {
                    user_remitente = autorizacion.user_autorizacion,
                    asunto = "Notificación respuesta autorización por averia",
                    fecha_envio = DateTime.Now,
                    enviado = false,
                    user_destinatario = autorizacion.user_creacion,
                    autorizacion_id = autorizacion.id,
                    razon_no_envio = ex.Message
                };
                db.notificaciones.Add(envio);
                db.SaveChanges();
            }

            #endregion

            //var autorizaciones = db.autorizaciones.Include(x => x.icb_vehiculo.icb_vehiculo_eventos).Where(x=> x.tipo_autorizacion == 1).ToList();
            BuscarFavoritos(menu);
            return RedirectToAction("BrowserAutorizaciones", new { menu });
        }

        public JsonResult BuscarExcepciones()
        {
            var info = (from e in db.ftipo_excepciones
                        select new
                        {
                            e.id,
                            excepcion = "(" + e.cod + ") " + e.descripcion
                        }).ToList();

            return Json(info, JsonRequestBehavior.AllowGet);
        }

        //repuestos
        public ActionResult BrowserAutorizacionesRepuestos(int? menu)
        {
            //var usuario_actual = Convert.ToInt32(Session["user_usuarioid"]);
            //var autorizaciones = db.autorizaciones.Where(x => x.tipo_autorizacion == 4 && x.user_autorizacion == usuario_actual).ToList();
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);
            List<int?> tipoAutorizaciones = db.usuarios_autorizaciones
                .Where(x =>  x.user_id == usuarioActual && x.tipoautorizacion == 4)
                .Select(x => x.tipoautorizacion).ToList();
            List<autorizaciones> autorizaciones = db.autorizaciones
                .Where(x => tipoAutorizaciones.Contains(x.tipo_autorizacion))
                .OrderByDescending(x => new {  x.autorizado, x.fecha_creacion }).ToList();
       

            BuscarFavoritos(menu);
            return View(autorizaciones);
        }


        //autorizaciones y envio de notificacion de correos para repuestos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BrowserAutorizacionesRepuestos(int? id_s, int? menu)
        {
            configuracion_envio_correos correoconfig = db.configuracion_envio_correos.Where(d => d.activo).FirstOrDefault();
            int usuario_actual = Convert.ToInt32(Session["user_usuarioid"]);
            int id = Convert.ToInt32(Request["autorizacion_id"]);
            autorizaciones autorizacion = db.autorizaciones.Find(id);
            if (autorizacion == null)
            {
                return HttpNotFound();
            }

            autorizacion.fecha_autorizacion = DateTime.Now;
            autorizacion.user_autorizacion = Convert.ToInt32(Session["user_usuarioid"]);
            autorizacion.autorizado = Convert.ToBoolean(Request["autorizado"]);
            autorizacion.comentario = Request["comentario"];
            db.Entry(autorizacion).State = EntityState.Modified;
            db.SaveChanges();
            TempData["mensaje"] = "Autorización guardada correctamente";

            // envio de notificacion
            try
            {
                users user_destinatario = db.users.Find(autorizacion.user_creacion);
                users user_remitente = db.users.Find(autorizacion.user_autorizacion);
                string autorizado = autorizacion.autorizado ? "Autorizado" : "No autorizado";

                MailAddress de = new MailAddress(correoconfig.correo, correoconfig.nombre_remitente);
                MailAddress para = new MailAddress(user_destinatario.user_email,
                    user_destinatario.user_nombre + " " + user_destinatario.user_apellido);
                MailMessage mensaje = new MailMessage(de, para);
                mensaje.Bcc.Add("liliana.avila@exiware.com");
                mensaje.Bcc.Add("marley.vargas@exiware.com");
                mensaje.ReplyToList.Add(new MailAddress(user_remitente.user_email,
                    user_remitente.user_nombre + " " + user_remitente.user_apellido));
                mensaje.Subject = "Respuesta Autorización facturación referencia " + autorizacion.ref_codigo;
                mensaje.BodyEncoding = Encoding.Default;
                mensaje.IsBodyHtml = true;
                string html = "";
                html += "<h4>Cordial Saludo</h4><br>";
                html += "<p>Respuesta solicitud autorización de facturación de la referencia <b>" +
                        autorizacion.ref_codigo + "</b> precio de venta menor al costo </p><br>";
                html += "<p><b>Fecha de solicitud: </b>" + autorizacion.fecha_creacion + "</p>";
                html += "<p><b>Referencia: </b>" + autorizacion.ref_codigo + "</p>";
                html += "<p><b>Estado Solicitud: " + autorizado + " </b></p>";
                html += "<p><b>Observaciones: </b>" + autorizacion.comentario + "</p>";
                html += "<p><b>Fecha Respuesta: </b>" + autorizacion.fecha_autorizacion + "</p>";
                html += "<p><b>Usuario Autorización: </b>" + user_remitente.user_nombre + " " +
                        user_remitente.user_apellido + "</p>";
                mensaje.Body = html;

                SmtpClient cliente = new SmtpClient(correoconfig.smtp_server)
                {
                    Port = correoconfig.puerto,
                    Credentials = new NetworkCredential(correoconfig.usuario, correoconfig.password),
                    EnableSsl = true
                };
                cliente.Send(mensaje);

                notificaciones envio = new notificaciones
                {
                    user_remitente = autorizacion.user_autorizacion,
                    asunto = "Notificación respuesta autorización facturación referencia",
                    fecha_envio = DateTime.Now,
                    enviado = true,
                    user_destinatario = autorizacion.user_creacion,
                    autorizacion_id = autorizacion.id
                };
                db.notificaciones.Add(envio);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                notificaciones envio = new notificaciones
                {
                    user_remitente = autorizacion.user_autorizacion,
                    asunto = "Notificación respuesta autorización facturación referencia",
                    fecha_envio = DateTime.Now,
                    enviado = false,
                    user_destinatario = autorizacion.user_creacion,
                    autorizacion_id = autorizacion.id,
                    razon_no_envio = ex.Message
                };
                db.notificaciones.Add(envio);
                db.SaveChanges();
            }

            //var autorizaciones = db.autorizaciones.Where(x => x.tipo_autorizacion == 4 && x.user_autorizacion == usuario_actual).ToList();
            BuscarFavoritos(menu);
            return RedirectToAction("BrowserAutorizacionesRepuestos", new { menu });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }

        public JsonResult BuscarAutorizaciones()
        {
            int usuario_actual = Convert.ToInt32(Session["user_usuarioid"]);
            var data = (from x in db.autorizaciones
                        where x.tipo_autorizacion == 4 && x.user_autorizacion == usuario_actual
                        select new
                        {
                            nombre = x.users1.user_nombre + " " + x.users1.user_apellido,
                            fecha_creacion = x.fecha_creacion.ToString(),
                            ref_codigo = x.ref_codigo == null ? "" : x.ref_codigo,
                            autorizado = x.autorizado ? "Activo" : "Inactivo",
                            fecha_autorizacion = x.fecha_autorizacion.ToString(),
                            comentario = x.comentario == null ? "" : x.comentario,
                            x.id
                        }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarDatos()
        {
            var data = (from a in db.autorizaciones
                        join b in db.bitacoraExcepcionFactura
                            on a.id equals b.id_Autorizacion into tmp
                        from b in tmp.DefaultIfEmpty()
                        join e in db.ftipo_excepciones
                            on b.id_excepcion equals e.id into tmp1
                        from e in tmp1.DefaultIfEmpty()
                        join u in db.users
                            on a.user_creacion equals u.user_id
                        join v in db.icb_vehiculo_eventos
                            on new { a = a.plan_mayor, b = 15 } equals new { a = v.planmayor, b = v.id_tpevento } into tmp2
                        from v in tmp2.DefaultIfEmpty()
                        where a.plan_mayor != null
                        select new
                        {
                            a.id,
                            userSolicitud = u.user_nombre + " " + u.user_apellido,
                            fechaSol = a.fecha_creacion.ToString(),
                            planM = a.plan_mayor != null ? a.plan_mayor : "",
                            detalle = a.comentario != null ? a.comentario : "",
                            a.autorizado,
                            fechaAu = a.fecha_autorizacion != null ? a.fecha_autorizacion.ToString() : "",
                            observaciones = v.evento_observacion != null ? v.evento_observacion : "",
                            excepcion = b.id_excepcion != null ? "(" + e.cod + ") " + e.descripcion : ""
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult verificarPendientes(string planmayor)
        {
            int saldoPendiente = 0;

            vw_browserBackOffice buscarPedido = db.vw_browserBackOffice.FirstOrDefault(x => x.planmayor == planmayor);
            if (buscarPedido != null)
            {
                if (buscarPedido.saldo > 0)
                {
                    saldoPendiente = 1;
                }
            }

            vpedido infoPedido = db.vpedido.FirstOrDefault(x => x.planmayor == planmayor);
            int? flotaAux = infoPedido != null ? infoPedido.flota : null;
            string tercero = (from p in db.vpedido
                              join t in db.icb_terceros
                                  on p.nit equals t.tercero_id
                              select t.doc_tercero).FirstOrDefault();

            var tipoPer = from t in db.icb_terceros
                          join d in db.tp_documento
                              on t.tpdoc_id equals d.tpdoc_id
                          where t.doc_tercero == tercero
                          select new
                          {
                              tipo = d.tipo.Trim()
                          };
            ListaPersonas Lista = new ListaPersonas();
            if (flotaAux != null)
            {
                vflota flota = db.vflota.Find(flotaAux);

                if (flota != null)
                {
                    Lista.ListaDocNecesarios = (from v in db.vvalidacionpeddoc
                                                join r in db.vdocrequeridosflota
                                                    on v.iddocrequerido equals r.id
                                                join d in db.vdocumentosflota
                                                    on r.iddocumento equals d.id
                                                where r.codflota == flota.flota && v.idpedido == infoPedido.id
                                                select new docNecesarios
                                                {
                                                    id = d.id,
                                                    documento = d.documento,
                                                    iddocumento = d.iddocumento,
                                                    estado = v.estado,
                                                    cargado =
                                                        db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x =>
                                                            x.idpedido == infoPedido.id && x.iddocumento == d.iddocumento) != null
                                                            ? db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x =>
                                                                x.idpedido == infoPedido.id && x.iddocumento == d.iddocumento).id
                                                            : 0
                                                }).ToList();
                }
            }
            else if (tipoPer.ToString() == "N")
            {
                Lista.ListaDocNecesarios = (from v in db.vvalidacionpeddoc
                                            join d in db.vdocumentosflota
                                                on v.iddocrequerido equals d.id
                                            where d.id_tipo_documento == 3 && v.idpedido == infoPedido.id
                                            select new docNecesarios
                                            {
                                                id = d.id,
                                                documento = d.documento,
                                                iddocumento = d.iddocumento,
                                                estado = v.estado,
                                                cargado =
                                                    db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x =>
                                                        x.idpedido == infoPedido.id && x.iddocumento == d.iddocumento) != null
                                                        ? db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x =>
                                                            x.idpedido == infoPedido.id && x.iddocumento == d.iddocumento).id
                                                        : 0
                                            }).ToList();
            }
            else if (tipoPer.ToString() != "N")
            {
                Lista.ListaDocNecesarios = (from v in db.vvalidacionpeddoc
                                            join d in db.vdocumentosflota
                                                on v.iddocrequerido equals d.id
                                            where d.id_tipo_documento == 2 && v.idpedido == infoPedido.id
                                            select new docNecesarios
                                            {
                                                id = d.id,
                                                documento = d.documento,
                                                iddocumento = d.iddocumento,
                                                estado = v.estado,
                                                cargado =
                                                    db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x =>
                                                        x.idpedido == infoPedido.id && x.iddocumento == d.iddocumento) != null
                                                        ? db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x =>
                                                            x.idpedido == infoPedido.id && x.iddocumento == d.iddocumento).id
                                                        : 0
                                            }).ToList();
            }

            List<docpendientes> faltante = new List<docpendientes>();
            //var buscarDocumentos = (from v in db.vvalidacionpeddoc
            //					join r in db.vdocrequeridosflota
            //					on v.iddocrequerido equals r.id
            //					join d in db.vdocumentosflota
            //					on r.iddocumento equals d.id
            //					where v.idpedido == buscarPedido.id
            //					select new
            //					{
            //						d.id,
            //						d.documento,
            //						v.estado
            //					}).ToList();			

            foreach (docNecesarios item in Lista.ListaDocNecesarios)
            {
                if (item.estado == false)
                {
                    faltante.Add(new docpendientes { id = item.id, descripcion = item.documento });
                }
            }

            string cliente = buscarPedido.razon_social != null
                ? buscarPedido.razon_social
                : buscarPedido.prinom_tercero + " " + buscarPedido.segnom_tercero + " " +
                  buscarPedido.apellido_tercero + " " + buscarPedido.segapellido_tercero;

            string[] datosCliente = new string[4];
            datosCliente[0] = buscarPedido.doc_tercero;
            datosCliente[1] = cliente;
            datosCliente[2] = buscarPedido.modelo;
            datosCliente[3] = buscarPedido.bodccs_nombre;

            if (saldoPendiente == 1)
            {
                if (faltante != null)
                {
                    return Json(new { datosCliente, dinero = true, documentos = true, buscarPedido.saldo, faltante },
                        JsonRequestBehavior.AllowGet);
                }

                return Json(new { datosCliente, dinero = true, documentos = false, buscarPedido.saldo },
                    JsonRequestBehavior.AllowGet);
            }

            return Json(new { datosCliente, dinero = false, documentos = true, faltante }, JsonRequestBehavior.AllowGet);
        }

        public void BuscarFavoritos(int? menu)
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

            var buscarFavoritosSeleccionados = (from favoritos in db.favoritos
                                                join menu2 in db.Menus
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

        public class docpendientes
        {
            public int id { get; set; }
            public string descripcion { get; set; }
        }

        public class docNecesarios
        {
            public int id { get; set; }
            public string documento { get; set; }
            public int? iddocumento { get; set; }
            public int cargado { get; set; }
            public bool estado { get; set; }
        }

        public class ListaPersonas
        {
            public List<docNecesarios> ListaDocNecesarios { get; set; }
        }
    }
}