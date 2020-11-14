using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class prospectosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        private static Expression<Func<vw_prospectos_activos, string>> GetColumnName(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(vw_prospectos_activos), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<vw_prospectos_activos, string>> lambda = Expression.Lambda<Func<vw_prospectos_activos, string>>(menuProperty, menu);

            return lambda;
        }
        public ActionResult Index(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public void ParametrosBusqueda()
        {
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 88);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
        }

        public ActionResult Seguimiento(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult VerSeguimiento(int? id, int? menu)
        {
            var buscarProspecto = context.prospectos.Where(x => x.id == id).FirstOrDefault();

            Prospecto modelo = new Prospecto();
            if (buscarProspecto !=null)
            {
                var buscarTercero = (from tercero in context.icb_terceros
                                     join medio in context.icb_medio_comunicacion
                                         on tercero.medcomun_id equals medio.medcomun_id into ps1
                                     from medio in ps1.DefaultIfEmpty()
                                     join tipoTramite in context.icb_tptramite_prospecto
                                         on tercero.tptramite equals tipoTramite.tptrapros_id into ps2
                                     from tipoTramite in ps2.DefaultIfEmpty()
                                     where tercero.tercero_id == buscarProspecto.idtercero
                                     select new
                                     {
                                         tercero.tercero_id,
                                         tercero.doc_tercero,
                                         nombrecompleto = tercero.razon_social + tercero.prinom_tercero + " " + tercero.segnom_tercero +
                                                          " " + tercero.apellido_tercero + " " + tercero.segapellido_tercero,
                                         tercero.telf_tercero,
                                         tercero.celular_tercero,
                                         tercero.email_tercero,
                                         seguimiento = (from seguimiento in context.seguimientotercero
                                                        join tipoSeguimiento in context.Seguimientos on seguimiento.tipo equals tipoSeguimiento.Id
                                                        join usuario in context.users on seguimiento.userid_creacion equals usuario.user_id
                                                        where seguimiento.idtercero == tercero.tercero_id
                                                        select new
                                                        {
                                                            tipoSeguimiento.Evento,
                                                            seguimiento.nota,
                                                            seguimiento.fecha,
                                                            nombreUsuario = usuario.user_nombre + " " + usuario.user_apellido
                                                        }).ToList()
                                     }).FirstOrDefault();
                if (buscarTercero != null)
                {
                    List<Prospecto.Seguimiento> lista = new List<Prospecto.Seguimiento>();
                    foreach (var item in buscarTercero.seguimiento)
                    {
                        lista.Add(new Prospecto.Seguimiento
                        {
                            TipoSeguimiento = item.Evento,
                            Usuario = item.nombreUsuario,
                            Nota = item.nota,
                            Fecha = item.fecha.ToString("yyyy/MM/dd")
                        });
                    }

                    modelo = new Prospecto
                    {
                        tercero_id = buscarTercero.tercero_id,
                        numDocumento = buscarTercero.doc_tercero,
                        prinom_tercero = buscarTercero.nombrecompleto,
                        telf_tercero = buscarTercero.telf_tercero,
                        celular_tercero = buscarTercero.celular_tercero,
                        email_tercero = buscarTercero.email_tercero,
                        listaSeguimiento = lista
                    };

                }
            }
            

            ViewBag.tipo_seguimiento =
                    //new SelectList(context.vtiposeguimientocot.Where(x =>       x.id_tipo_seguimiento != 3 && x.id_tipo_seguimiento != 4 && x.id_tipo_seguimiento != 5),
                    new SelectList(context.Seguimientos.Where(x => x.EsManual == true && x.Modulo == 4), "Id", "Evento");
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public JsonResult buscarSeguimientosPorIdTercero(int id_tercero)
        {

            var buscarSeguimientos2 = context.seguimientotercero.Where(x => x.idtercero == id_tercero).Select(b => new
            {
                nombreUsuario= b.users.user_nombre,
                nombre_seguimiento=b.Seguimientos.Evento,
                b.fecha,
                b.nota
            }).ToList();
            var data2 = buscarSeguimientos2.Select(x => new
            {
                x.nombreUsuario,
                x.nombre_seguimiento,
                fecha = x.fecha.ToString("yyyy/MM/dd"),
                x.nota
            });

            return Json(data2, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AgregarSeguimientoProspecto(int id_tercero, int id_tipo_seguimiento, string nota)
        {
            context.seguimientotercero.Add(new seguimientotercero
            {
                idtercero = id_tercero,
                fecha = DateTime.Now,
                nota = nota,
                tipo = id_tipo_seguimiento,
                userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
            });
            int guardar = context.SaveChanges();
            if (guardar > 0)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        //Get
        [HttpGet]
        public ActionResult Create(int? menu)
        {
            DatosListasDesplegables(new Prospecto());
            ParametrosBusqueda();
            BuscarFavoritos(menu);

            return View(new Prospecto());
        }

        /// <summary>
        /// </summary>
        /// <param name="prospecto"></param>
        /// <param name="menu"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(Prospecto prospecto, int? menu)
        {
            string camposObligatorios = "<label>Los siguientes campos son obligatorios:<label><ul>";
            int vacio = 0;
            string modelo = Request["modelosInt"];
            string modelo1 = Request["modelosOpc"];
            if (prospecto.asesor_id == null && prospecto.buscarAsesor_id != null)
            {
                prospecto.asesor_id = prospecto.buscarAsesor_id;
            }
            prospecto.modelo = modelo;
            prospecto.modeloOpcional = modelo1;
            //ViewBag.asesor_nombre = Request["asesor_id"];

            if (string.IsNullOrWhiteSpace(prospecto.prinom_tercero)) { camposObligatorios += "<li>Primer Nombre</li>"; vacio = 1; }
            if (string.IsNullOrWhiteSpace(prospecto.apellido_tercero)) { camposObligatorios += "<li>Primer Apellido</li>"; vacio = 1; }
            //if (string.IsNullOrWhiteSpace(prospecto.celular_tercero)) { camposObligatorios += "<li>Celular</li>"; vacio = 1; }

            if (prospecto.genero_tercero == null) { camposObligatorios += "<li>Genero</li>"; vacio = 1; }
            if (prospecto.tptrapros_id == 0) { camposObligatorios += "<li>Tipo De Tramite</li>"; vacio = 1; }
            //if (prospecto.asesor_id == null) { if (prospecto.tptrapros_id == 2) { camposObligatorios += "<li>Asesor</li>"; vacio = 1; } }
            if (prospecto.asesor_id == null && prospecto.buscarAsesor_id == null) { if (prospecto.tptrapros_id == 1 || prospecto.tptrapros_id == 4) { camposObligatorios += "<li>Asesor o Buscar Asesor</li>"; vacio = 1; } }

            // if (prospecto.tporigen_id == 0) { camposObligatorios += "<li>Fuente</li>"; }
            if (prospecto.subfuente_id == null) { if (prospecto.tporigen_id == 1) { camposObligatorios += "<li>Sub-Fuente</li>"; vacio = 1; } }
            if (prospecto.medcomun_id == null) { camposObligatorios += "<li>Medio Comunicación</li>"; vacio = 1; }
            camposObligatorios += "</ul>";
            if (vacio == 1)
            {
                TempData["mensaje_obligatorios"] = camposObligatorios;
            }
            if (ModelState.IsValid && vacio == 0)
            {
                string telf_tercero = prospecto.telf_tercero != null ? prospecto.telf_tercero : "---";
                string numDocumento = prospecto.numDocumento != null ? prospecto.numDocumento : "---";
                string numCelular = prospecto.celular_tercero != null ? prospecto.celular_tercero : "";

                var predicado = PredicateBuilder.True<prospectos>();
                if (!string.IsNullOrWhiteSpace(prospecto.numDocumento))
                {
                    predicado = predicado.And(d => d.icb_terceros.doc_tercero == prospecto.numDocumento);
                }
                else if (!string.IsNullOrWhiteSpace(prospecto.celular_tercero))
                {
                    predicado = predicado.And(d => d.icb_terceros.celular_tercero == prospecto.celular_tercero);
                }
                else if (!string.IsNullOrWhiteSpace(prospecto.telf_tercero))
                {
                    predicado = predicado.And(d => d.icb_terceros.telf_tercero == prospecto.telf_tercero);
                }
                else
                {
                    predicado = predicado.And(d => d.icb_terceros.doc_tercero == "999999999999999999");
                }

                predicado = predicado.And(d => d.estado == 1);
                prospectos buscarTercero = context.prospectos.Where(predicado).FirstOrDefault();

                //context.icb_terceros.FirstOrDefault(x => x.celular_tercero == prospecto.celular_tercero || x.telf_tercero == telf_tercero || x.doc_tercero == numDocumento);
                prospectos buscarProspecto = context.prospectos.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x =>
                    x.celular_tercero == prospecto.celular_tercero || x.telf_tercero == telf_tercero ||
                    x.documento == numDocumento);
                // var buscarCedulaProspecto = context.prospectos.FirstOrDefault(x => x.documento == prospecto.numDocumento);

                var session = Convert.ToInt32(Session["user_usuarioid"]);







                if (buscarTercero != null)
                {
                    //#region Si el prospecto ya existe en BD

                    //if (buscarProspecto != null && buscarProspecto.estado == 0)
                    //{
                    //    #region Registro de prospecto

                    //    prospectos prospectoNuevo = new prospectos
                    //    {
                    //        tpdoc_id = prospecto.tipoDocumento,
                    //        documento = prospecto.numDocumento,
                    //        prinom_tercero = prospecto.prinom_tercero,
                    //        segnom_tercero = prospecto.segnom_tercero,
                    //        apellido_tercero = prospecto.apellido_tercero,
                    //        segapellido_tercero = prospecto.segapellido_tercero,
                    //        razonsocial = prospecto.razonSocial,
                    //        digitoverificacion = prospecto.digito_verificacion,
                    //        email_tercero = prospecto.email_tercero,
                    //        telf_tercero = prospecto.telf_tercero,
                    //        celular_tercero = prospecto.celular_tercero,
                    //        genero_tercero = Convert.ToInt32(prospecto.genero_tercero),
                    //        tptramite = Convert.ToInt32(prospecto.tptrapros_id),
                    //        //asesor_id = Convert.ToInt32(prospecto.asesor_id),
                    //        origen_id = prospecto.tporigen_id,
                    //        subfuente = prospecto.subfuente_id,
                    //        medcomun_id = Convert.ToInt32(prospecto.medcomun_id),
                    //        sitioevento = prospecto.eventoOrigen,
                    //        observaciones = prospecto.observacion,
                    //        habdtautor_correo = prospecto.habdtautor_correo,
                    //        habdtautor_celular = prospecto.habdtautor_celular,
                    //        habdtautor_msm = prospecto.habdtautor_msm,
                    //        habdtautor_watsap = prospecto.habdtautor_whatsapp,
                    //        numfamiliares = prospecto.numeroFamiliares ?? 0,
                    //        numamigos = prospecto.numeroAmigos,
                    //        bodega = Convert.ToInt32(Session["user_bodega"]),
                    //        estado = 1,
                    //        temperatura = prospecto.temperatura,
                    //        fec_creacion = DateTime.Now,
                    //        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    //        modelo = prospecto.modelo,
                    //        modelo1 = prospecto.modeloOpcional,
                    //        idtercero = buscarTercero.tercero_id
                    //    };

                    //    #endregion

                    //    context.prospectos.Add(prospectoNuevo);
                    //    context.SaveChanges();

                    //    prospectos buscarUltimoProspecto = context.prospectos.OrderByDescending(x => x.id).FirstOrDefault();
                    //    buscarUltimoProspecto.asesor_id = prospecto.asesor_id;
                    //    context.Entry(buscarUltimoProspecto).State = EntityState.Modified;

                    //    icb_terceros buscarUltimoTercero = context.icb_terceros.OrderByDescending(x => x.tercero_id)
                    //        .FirstOrDefault(x => x.tercero_id == buscarUltimoProspecto.idtercero);
                    //    buscarUltimoTercero.asesor_id = prospecto.asesor_id;
                    //    context.Entry(buscarUltimoTercero).State = EntityState.Modified;

                    //    if (Session["user_rolid"].ToString() == "7")
                    //    {
                    //        #region asignacion

                    //        asignacion asignacion = new asignacion
                    //        {
                    //            idProspecto = prospectoNuevo.id,
                    //            idAsesor = Convert.ToInt32(prospecto.asesor_id),
                    //            estado = false,
                    //            fechaInicio = DateTime.Now
                    //        };
                    //        context.asignacion.Add(asignacion);

                    //        #endregion

                    //        #region mensaje de texto y demas (asesor)

                    //        bool result = context.SaveChanges() > 0;
                    //        if (result)
                    //        {
                    //            // Se debe guardar un registro en la tabla alertaasesor para que este lo atienda
                    //            icb_terceros consultaUltimoTercero = context.icb_terceros.OrderByDescending(x => x.tercero_id)
                    //                .FirstOrDefault();

                    //            string parametroAsesor = context.icb_sysparameter
                    //                .FirstOrDefault(x => x.syspar_cod == "P88").syspar_value;
                    //            string parametroAsesorU = context.icb_sysparameter
                    //                .FirstOrDefault(x => x.syspar_cod == "P99").syspar_value;
                    //            // Si soy rol Asesor no me llegara notificacion, porque el mismo se ha asignado, la notificacion llega cuando la anfitriona asigna un asesor
                    //            // El numero tres se refiere a el tipo de tramite chevyplan de la vista de crear prospecto y rol 4 es asesor comercial
                    //            if ((Session["user_rolid"].ToString() != parametroAsesor ||
                    //                 Session["user_rolid"].ToString() != parametroAsesorU) &&
                    //                prospecto.tptrapros_id != 3)
                    //            {
                    //                context.alertaasesor.Add(new alertaasesor
                    //                {
                    //                    fecha = DateTime.Now,
                    //                    asesor = prospecto.asesor_id,
                    //                    propecto = consultaUltimoTercero.tercero_id,
                    //                    anfitrion = Convert.ToInt32(Session["user_usuarioid"]),
                    //                    recibido = false
                    //                });
                    //            }

                    //            int guardarAlerta = context.SaveChanges();
                    //            if (guardarAlerta > 0)
                    //            {
                    //                // Para poner ocupado al asesor ya que se le acaba de asignar un prospecto
                    //                sesion_logasesor ultimaSesionAsesor = context.sesion_logasesor.OrderByDescending(x => x.id)
                    //                    .FirstOrDefault(x => x.user_id == prospecto.asesor_id);
                    //                if (ultimaSesionAsesor != null)
                    //                {
                    //                    ultimaSesionAsesor.fecha_inicia = DateTime.Now;
                    //                    ultimaSesionAsesor.fecha_termina = DateTime.Now;
                    //                    ultimaSesionAsesor.estado = 4;
                    //                    context.Entry(ultimaSesionAsesor).State = EntityState.Modified;
                    //                    context.SaveChanges();
                    //                }
                    //                //se pone en estado 5, que esta esperando respuesta, de si acepta o rechaza al prospecto
                    //                //creo una nueva sesion de ese asesor=
                    //                sesion_logasesor nuevaSesionAsesor = new sesion_logasesor
                    //                {
                    //                    bodega = ultimaSesionAsesor.bodega,
                    //                    estado = 5,
                    //                    fecha_inicia = DateTime.Now,
                    //                    fecha_termina = DateTime.Now.AddMinutes(5),
                    //                    user_id = ultimaSesionAsesor.user_id,
                    //                };
                    //                context.sesion_logasesor.Add(nuevaSesionAsesor);
                    //                context.SaveChanges();

                    //                // Validacion para envio de mensaje de texto al asesor

                    //                #region mensaje de texto asesor

                    //                string msmEntregado = "";
                    //                try
                    //                {
                    //                    users buscaAsesor =
                    //                        context.users.FirstOrDefault(x => x.user_id == prospecto.asesor_id);
                    //                    string mensajeAlAsesor =
                    //                        "Se le ha asignado el prospecto " + consultaUltimoTercero.prinom_tercero +
                    //                        " " + consultaUltimoTercero.segnom_tercero + " "
                    //                        + consultaUltimoTercero.apellido_tercero + " " +
                    //                        consultaUltimoTercero.segapellido_tercero;
                    //                    string telefonoEnviarMsm = consultaUltimoTercero.celular_tercero;

                    //                    MensajesDeTexto enviar = new MensajesDeTexto();
                    //                    string status = enviar.enviarmensaje(buscaAsesor.user_telefono, mensajeAlAsesor);

                    //                    if (status != "OK")
                    //                    {
                    //                        msmEntregado =
                    //                            ", no se ha enviado notificacion en sms, telefono no disponible";
                    //                    }
                    //                    else
                    //                    {
                    //                        msmEntregado = ", mensaje de texto enviado correctamente al asesor";
                    //                    }
                    //                }
                    //                catch (Exception)
                    //                {
                    //                    TempData["mensaje"] =
                    //                        "Prospecto agregado y asignado exitosamente pero el mesaje de texto al asesor no ha salido, verfique el servicio de mensajeria... ";
                    //                    DatosListasDesplegables(null);
                    //                    ParametrosBusqueda();
                    //                    BuscarFavoritos(menu);

                    //                    // Agregar seguimiento
                    //                    string buscarParametroSeguimiento1 = (from parametro in context.icb_sysparameter
                    //                                                          where parametro.syspar_cod == "P56"
                    //                                                          select parametro.syspar_value).FirstOrDefault();
                    //                    if (!string.IsNullOrEmpty(buscarParametroSeguimiento1))
                    //                    {
                    //                        context.seguimientotercero.Add(new seguimientotercero
                    //                        {
                    //                            idtercero = consultaUltimoTercero.tercero_id,
                    //                            fecha = DateTime.Now,
                    //                            nota = "Se creo el prospecto",
                    //                            tipo = Convert.ToInt32(buscarParametroSeguimiento1),
                    //                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                    //                        });
                    //                        int guardar = context.SaveChanges();
                    //                    }

                    //                    return RedirectToAction("Create", new { menu });
                    //                }

                    //                #endregion

                    //                // Fin validacion para envio de mensaje de texto al asesor

                    //                // Validacion para envio de mensaje de texto al cliente (confirmacion visita)

                    //                #region Mensaje de texto cliente

                    //                if (prospectoNuevo.celular_tercero != null)
                    //                {
                    //                    pmensajevisita mensaje = new pmensajevisita();
                    //                    try
                    //                    {
                    //                        int? buscarClienteProspecto = context.prospectos
                    //                            .FirstOrDefault(x => x.id == prospectoNuevo.id).idtercero;
                    //                        int? id_tercero_cliente = buscarClienteProspecto;
                    //                        icb_terceros buscaCliente =
                    //                            context.icb_terceros.FirstOrDefault(x =>
                    //                                x.tercero_id == id_tercero_cliente);
                    //                        string nombre = !string.IsNullOrEmpty(buscaCliente.prinom_tercero)
                    //                            ? buscaCliente.prinom_tercero + " "
                    //                            : "";
                    //                        string segundoNombre = !string.IsNullOrEmpty(buscaCliente.segnom_tercero)
                    //                            ? buscaCliente.segnom_tercero + " "
                    //                            : "";
                    //                        string apellido = !string.IsNullOrEmpty(buscaCliente.apellido_tercero)
                    //                            ? buscaCliente.apellido_tercero + " "
                    //                            : "";
                    //                        string segundoApellido =
                    //                            !string.IsNullOrEmpty(buscaCliente.segapellido_tercero)
                    //                                ? buscaCliente.segapellido_tercero + " "
                    //                                : "";

                    //                        string urlActual0 = Request.Url.Scheme + "://" + Request.Url.Authority;
                    //                        string llave = EncriptarLlave(id_tercero_cliente.ToString());
                    //                        string url = urlActual0 += @"/prospectos/confirmarVisita?id=" + llave;
                    //                        string asd = Convert.ToString(url);

                    //                        string mensajeAlCliente =
                    //                            "Señor(a) " + nombre + segundoNombre + apellido + segundoApellido +
                    //                            ", Bienvenido a caminos, gracias por preferirnos. " +
                    //                            "Por favor confirme su visita en el siguiente enlace: " + asd;

                    //                        MensajesDeTexto enviar = new MensajesDeTexto();
                    //                        string status = enviar.enviarmensaje(buscaCliente.celular_tercero,
                    //                            mensajeAlCliente);

                    //                        mensaje.llave = llave;
                    //                        mensaje.fecha_envio = DateTime.Now;
                    //                        mensaje.idtercero = Convert.ToInt32(id_tercero_cliente);

                    //                        if (status != "OK")
                    //                        {
                    //                            mensaje.llegadamensaje = false;
                    //                        }
                    //                        else
                    //                        {
                    //                            mensaje.llegadamensaje = true;
                    //                        }
                    //                    }
                    //                    catch (Exception)
                    //                    {
                    //                        mensaje.llegadamensaje = false;
                    //                    }

                    //                    context.pmensajevisita.Add(mensaje);
                    //                    context.SaveChanges();
                    //                }

                    //                #endregion

                    //                // Fin validacion para envio de mensaje de texto al cliente (confirmacion visita)

                    //                TempData["mensaje"] = "Prospecto agregado y asignado exitosamente" + msmEntregado;
                    //                DatosListasDesplegables(null);
                    //                ParametrosBusqueda();
                    //                BuscarFavoritos(menu);

                    //                // Agregar seguimiento
                    //                string buscarParametroSeguimiento2 = (from parametro in context.icb_sysparameter
                    //                                                      where parametro.syspar_cod == "P56"
                    //                                                      select parametro.syspar_value).FirstOrDefault();
                    //                if (!string.IsNullOrEmpty(buscarParametroSeguimiento2))
                    //                {
                    //                    context.seguimientotercero.Add(new seguimientotercero
                    //                    {
                    //                        idtercero = consultaUltimoTercero.tercero_id,
                    //                        fecha = DateTime.Now,
                    //                        nota = "Se creo el prospecto",
                    //                        tipo = Convert.ToInt32(buscarParametroSeguimiento2),
                    //                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                    //                    });
                    //                    int guardar = context.SaveChanges();
                    //                }

                    //                return RedirectToAction("Create", new { menu });
                    //            }

                    //            TempData["mensaje_error"] = "Error de conexion, por favor valide...";
                    //        }

                    //        #endregion
                    //    }
                    //    else
                    //    {
                    //        //Si el registro del prospecto lo hizo un rol diferente a la anfitriona entrara a este segmento de codigo	
                    //        context.SaveChanges();
                    //        TempData["mensaje"] = "Prospecto agregado y asignado exitosamente";
                    //        DatosListasDesplegables(null);
                    //        ParametrosBusqueda();
                    //        BuscarFavoritos(menu);
                    //        icb_terceros consultaUltimoTercero = context.icb_terceros.OrderByDescending(x => x.tercero_id)
                    //            .FirstOrDefault();

                    //        // Validacion para envio de mensaje de texto al asesor

                    //        #region Mensaje de texto al asesor

                    //        try
                    //        {
                    //            users buscaAsesor = context.users.FirstOrDefault(x => x.user_id == prospecto.asesor_id);
                    //            string mensajeAlAsesor =
                    //                "Se le ha asignado el prospecto " + consultaUltimoTercero.prinom_tercero + " " +
                    //                consultaUltimoTercero.segnom_tercero + " "
                    //                + consultaUltimoTercero.apellido_tercero + " " +
                    //                consultaUltimoTercero.segapellido_tercero;
                    //            string telefonoEnviarMsm = consultaUltimoTercero.celular_tercero;

                    //            MensajesDeTexto enviar2 = new MensajesDeTexto();
                    //            string status = enviar2.enviarmensaje(buscaAsesor.user_telefono, mensajeAlAsesor);

                    //            if (status != "OK")
                    //            {
                    //            }
                    //            else
                    //            {
                    //            }
                    //        }
                    //        catch (Exception)
                    //        {
                    //            TempData["mensaje"] =
                    //                "Prospecto agregado y asignado exitosamente pero el mesaje de texto al asesor no ha salido, verfique el servicio de mensajeria... ";
                    //            DatosListasDesplegables(null);
                    //            ParametrosBusqueda();
                    //            BuscarFavoritos(menu);
                    //        }

                    //        #endregion

                    //        //// Fin validacion para envio de mensaje de texto al asesor

                    //        // Validacion para envio de mensaje de texto al cliente (confirmacion visita)

                    //        #region Mensaje de texto cliente

                    //        if (prospectoNuevo.celular_tercero != null)
                    //        {
                    //            pmensajevisita mensaje = new pmensajevisita();
                    //            try
                    //            {
                    //                int? buscarClienteProspecto = context.prospectos
                    //                    .FirstOrDefault(x => x.id == prospectoNuevo.id).idtercero;
                    //                int? id_tercero_cliente = buscarClienteProspecto;
                    //                icb_terceros buscaCliente =
                    //                    context.icb_terceros.FirstOrDefault(x => x.tercero_id == id_tercero_cliente);
                    //                string nombre = !string.IsNullOrEmpty(buscaCliente.prinom_tercero)
                    //                    ? buscaCliente.prinom_tercero + " "
                    //                    : "";
                    //                string segundoNombre = !string.IsNullOrEmpty(buscaCliente.segnom_tercero)
                    //                    ? buscaCliente.segnom_tercero + " "
                    //                    : "";
                    //                string apellido = !string.IsNullOrEmpty(buscaCliente.apellido_tercero)
                    //                    ? buscaCliente.apellido_tercero + " "
                    //                    : "";
                    //                string segundoApellido = !string.IsNullOrEmpty(buscaCliente.segapellido_tercero)
                    //                    ? buscaCliente.segapellido_tercero + " "
                    //                    : "";

                    //                string urlActual0 = Request.Url.Scheme + "://" + Request.Url.Authority;
                    //                string llave = EncriptarLlave(id_tercero_cliente.ToString());
                    //                string url = urlActual0 += @"/prospectos/confirmarVisita?id=" + llave;
                    //                string asd = Convert.ToString(url);

                    //                string mensajeAlCliente =
                    //                    "Señor(a) " + nombre + segundoNombre + apellido + segundoApellido +
                    //                    ", Bienvenido a caminos, gracias por preferirnos. " +
                    //                    "Por favor confirme su visita en el siguiente enlace: " + asd;

                    //                MensajesDeTexto enviar2 = new MensajesDeTexto();
                    //                string status = enviar2.enviarmensaje(buscaCliente.celular_tercero, mensajeAlCliente);

                    //                mensaje.llave = llave;
                    //                mensaje.fecha_envio = DateTime.Now;
                    //                mensaje.idtercero = Convert.ToInt32(id_tercero_cliente);

                    //                if (status != "OK")
                    //                {
                    //                    mensaje.llegadamensaje = false;
                    //                }
                    //                else
                    //                {
                    //                    mensaje.llegadamensaje = true;
                    //                }
                    //            }
                    //            catch (Exception)
                    //            {
                    //                mensaje.llegadamensaje = false;
                    //            }

                    //            context.pmensajevisita.Add(mensaje);
                    //            context.SaveChanges();
                    //        }

                    //        #endregion

                    //        // Fin validacion para envio de mensaje de texto al cliente (confirmacion visita)

                    //        // Agregar seguimiento

                    //        #region seguimiento

                    //        string buscarParametroSeguimiento2 = (from parametro in context.icb_sysparameter
                    //                                              where parametro.syspar_cod == "P56"
                    //                                              select parametro.syspar_value).FirstOrDefault();
                    //        if (!string.IsNullOrEmpty(buscarParametroSeguimiento2))
                    //        {
                    //            context.seguimientotercero.Add(new seguimientotercero
                    //            {
                    //                idtercero = consultaUltimoTercero.tercero_id,
                    //                fecha = DateTime.Now,
                    //                nota = "Se creo el prospecto",
                    //                tipo = Convert.ToInt32(buscarParametroSeguimiento2),
                    //                userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                    //            });
                    //        }

                    //        tareasasignadas tarea = new tareasasignadas
                    //        {
                    //            fec_creacion = DateTime.Now,
                    //            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    //            estado = true,
                    //            idusuarioasignado = Convert.ToInt32(prospecto.asesor_id),
                    //            idtercero = Convert.ToInt32(consultaUltimoTercero.tercero_id),
                    //            notas = "Prospecto por gestionar"
                    //        };
                    //        context.tareasasignadas.Add(tarea);

                    //        int guardar = context.SaveChanges();

                    //        #endregion

                    //        return RedirectToAction("Create", new { menu });
                    //    }
                    //}

                    string campoRepetido = "";
                    //if (buscarTercero.telf_tercero == telf_tercero)
                    //{
                    //    campoRepetido = "telefono";
                    //}

                    //if (buscarTercero.celular_tercero == prospecto.celular_tercero)
                    //{
                    //    campoRepetido = "celular";
                    //}

                    if (buscarTercero.icb_terceros.doc_tercero == numDocumento)
                    {
                        campoRepetido = "documento";
                    }

                    //users asesorAsignado = context.users.FirstOrDefault(x => x.user_id == prospecto.asesor_id);
                    //string nombreAsesor = asesorAsignado != null
                    //    ? ", esta asignado a asesor " + asesorAsignado.user_nombre + " " +
                    //      asesorAsignado.user_apellido + " y se encuentra en estado activo"
                    //    : "";
                    //TempData["mensaje_error"] = "Ya existe un prospecto registrado con el mismo " + campoRepetido +
                    //                            ", su nombre es "
                    //                            + buscarTercero.prinom_tercero + " " + buscarTercero.apellido_tercero +
                    //                            (buscarTercero.razon_social != null ? buscarTercero.razon_social : "") +
                    //                            nombreAsesor;
                    TempData["mensaje_error"] = "Ya existe un prospecto registrado con el mismo " + campoRepetido;

                    //#endregion
                }
                else
                {
                    #region Si el prospecto no existia en la BD

                    if (prospecto.eventoOrigen == 0)
                    {
                        prospecto.eventoOrigen = null;
                    }

                    #region registro tercero

                    //busco si existe el tercero. Si no existe lo creo, si existe lo asigno como prospecto
                    var idprospecto = 0;

                    var predicado2 = PredicateBuilder.True<icb_terceros>();
                    if (!string.IsNullOrWhiteSpace(prospecto.numDocumento))
                    {
                        predicado2 = predicado2.And(d => d.doc_tercero == prospecto.numDocumento);
                    }
                    else if (!string.IsNullOrWhiteSpace(prospecto.celular_tercero))
                    {
                        predicado2 = predicado2.And(d => d.celular_tercero == prospecto.celular_tercero);
                    }
                    else if (!string.IsNullOrWhiteSpace(prospecto.telf_tercero))
                    {
                        predicado2 = predicado2.And(d => d.telf_tercero == prospecto.telf_tercero);
                    }
                    else
                    {
                        predicado2 = predicado2.And(d => d.doc_tercero == "999999999999999999");
                    }

                    predicado2 = predicado2.And(d => d.tercero_estado == true);
                    icb_terceros buscarTercero2 = context.icb_terceros.Where(predicado2).FirstOrDefault();
                    var buscarCedulaProspecto = buscarTercero2;

                    if (buscarCedulaProspecto != null)
                    {
                        idprospecto = buscarCedulaProspecto.tercero_id;
                    }
                    else
                    {

                        //creo el tercero
                        icb_terceros nuevoProspecto = new icb_terceros
                        {
                            doc_tercero = prospecto.numDocumento,
                            tpdoc_id = prospecto.tipoDocumento,
                            digito_verificacion = prospecto.digito_verificacion,
                            razon_social = prospecto.razonSocial,
                            tercerofec_creacion = DateTime.Now,
                            tercerouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            prinom_tercero = prospecto.prinom_tercero,
                            apellido_tercero = prospecto.apellido_tercero,
                            segnom_tercero = prospecto.segnom_tercero,
                            segapellido_tercero = prospecto.segapellido_tercero,
                            email_tercero = prospecto.email_tercero,
                            genero_tercero = prospecto.genero_tercero,
                            tptramite = Convert.ToInt32(prospecto.tptrapros_id),
                            medcomun_id = prospecto.medcomun_id,
                            numfamiliares = prospecto.numeroFamiliares ?? 0,
                            numamigos = prospecto.numeroAmigos,
                            telf_tercero = prospecto.telf_tercero,
                            celular_tercero = prospecto.celular_tercero,
                            prospecto = true,
                            tercero_estado = true,
                            //asesor_id = prospecto.asesor_id,
                            observaciones = prospecto.observacion,
                            origen_id = prospecto.tporigen_id,
                            habdtautor_celular = prospecto.habdtautor_celular,
                            habdtautor_correo = prospecto.habdtautor_correo,
                            habdtautor_msm = prospecto.habdtautor_msm,
                            habdtautor_watsap = prospecto.habdtautor_whatsapp,
                            sitioevento = prospecto.eventoOrigen,
                            subfuente = prospecto.subfuente_id
                        };
                        context.icb_terceros.Add(nuevoProspecto);
                        var guardar = context.SaveChanges();
                        idprospecto = nuevoProspecto.tercero_id;
                    }



                    #endregion

                    #region registro de prospecto

                    prospectos prospectoNuevo = new prospectos
                    {
                        tpdoc_id = prospecto.tipoDocumento,
                        documento = prospecto.numDocumento,
                        prinom_tercero = prospecto.prinom_tercero,
                        segnom_tercero = prospecto.segnom_tercero,
                        apellido_tercero = prospecto.apellido_tercero,
                        segapellido_tercero = prospecto.segapellido_tercero,
                        razonsocial = prospecto.razonSocial,
                        digitoverificacion = prospecto.digito_verificacion,
                        email_tercero = prospecto.email_tercero,
                        telf_tercero = prospecto.telf_tercero,
                        celular_tercero = prospecto.celular_tercero
                    };
                    if (prospecto.genero_tercero != null)
                    {
                        prospectoNuevo.genero_tercero = Convert.ToInt32(prospecto.genero_tercero);
                    }

                    prospectoNuevo.tptramite = Convert.ToInt32(prospecto.tptrapros_id);
                    //prospectoNuevo.asesor_id = Convert.ToInt32(prospecto.asesor_id);
                    prospectoNuevo.origen_id = prospecto.tporigen_id;
                    prospectoNuevo.subfuente = prospecto.subfuente_id;
                    prospectoNuevo.medcomun_id = Convert.ToInt32(prospecto.medcomun_id);
                    prospectoNuevo.sitioevento = prospecto.eventoOrigen;
                    prospectoNuevo.observaciones = prospecto.observacion;
                    prospectoNuevo.habdtautor_correo = prospecto.habdtautor_correo;
                    prospectoNuevo.habdtautor_celular = prospecto.habdtautor_celular;
                    prospectoNuevo.habdtautor_msm = prospecto.habdtautor_msm;
                    prospectoNuevo.habdtautor_watsap = prospecto.habdtautor_whatsapp;
                    prospectoNuevo.numfamiliares = prospecto.numeroFamiliares ?? 0;
                    prospectoNuevo.numamigos = prospecto.numeroAmigos;
                    prospectoNuevo.bodega = Convert.ToInt32(Session["user_bodega"]);
                    prospectoNuevo.estado = 1;
                    prospectoNuevo.temperatura = prospecto.temperatura;
                    prospectoNuevo.fec_creacion = DateTime.Now;
                    prospectoNuevo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    prospectoNuevo.modelo = prospecto.modelo != "0" ? prospecto.modelo : null;
                    prospectoNuevo.modelo1 = prospecto.modeloOpcional != "0" ? prospecto.modelo : null;
                    //prospectoNuevo.idtercero = nuevoProspecto.tercero_id;
                    prospectoNuevo.idtercero = idprospecto;
                    prospectoNuevo.buscarAsesor_id = prospecto.buscarAsesor_id;

                    context.prospectos.Add(prospectoNuevo);
                    //try
                    //{
                    var confirmacion = context.SaveChanges();
                    if (confirmacion == 1)
                    {
                        if (prospecto.buscarAsesor_id != null)
                        {
                            notificacion_asesor_prospecto crearNotificaciones = new notificacion_asesor_prospecto
                            {
                                
                                asesor_id = prospecto.buscarAsesor_id.Value,
                                prospecto_id = prospectoNuevo.id,
                            leido = false,
                                descripcion =
                              "Notificación creacion de prospecto con asesor ocupado",

                            };
                            context.notificacion_asesor_prospecto.Add(crearNotificaciones);
                            context.SaveChanges();
                        }
                    }
                    #endregion

                    if (prospecto.asesor_id != null)
                    {
                        prospectos buscarUltimoProspecto =
                                context.prospectos.OrderByDescending(x => x.id).Where(d => d.id == prospectoNuevo.id).FirstOrDefault();
                        buscarUltimoProspecto.asesor_id = prospecto.asesor_id;
                        context.Entry(buscarUltimoProspecto).State = EntityState.Modified;

                        icb_terceros buscarUltimoTercero = context.icb_terceros.OrderByDescending(x => x.tercero_id)
                            .FirstOrDefault();
                        buscarUltimoTercero.asesor_id = prospecto.asesor_id;
                        context.Entry(buscarUltimoTercero).State = EntityState.Modified;                      
                            #region asignacion

                            asignacion asignacion = new asignacion
                            {
                                idProspecto = prospectoNuevo.id,
                                idAsesor = Convert.ToInt32(prospecto.asesor_id),
                                estado = false,
                                fechaInicio = DateTime.Now
                            };
                            context.asignacion.Add(asignacion);

                            #endregion
                        
                    }

                    if (Session["user_rolid"].ToString() == "7")
                    {
                        #region Guardamos el prospecto y enviamos mensaje de texto al asesor

                        bool result = context.SaveChanges() > 0;
                        if (result)
                        {
                            // Se cambia el estado del asesor a ocupado para que no se asigne a la siguiente persona prospecto
                            //var ultimaSesionAsesor = context.sesion_logasesor.OrderByDescending(x => x.id).FirstOrDefault(x => x.user_id == prospecto.asesor_id);
                            //if (ultimaSesionAsesor != null)
                            //{
                            //    ultimaSesionAsesor.estado = 2;
                            //    context.Entry(ultimaSesionAsesor).State = System.Data.Entity.EntityState.Modified;
                            //    //context.SaveChanges();
                            //}

                            // Se debe guardar un registro en la tabla alertaasesor para que este lo atienda
                            icb_terceros consultaUltimoTercero = context.icb_terceros.OrderByDescending(x => x.tercero_id)
                                .FirstOrDefault();

                            string parametroAsesor = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P88")
                                .syspar_value;
                            string parametroAsesorU = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P99")
                                .syspar_value;
                            // Si soy rol Asesor no me llegara notificacion, porque el mismo se ha asignado, la notificacion llega cuando la anfitriona asigna un asesor
                            // El numero tres se refiere a el tipo de tramite chevyplan de la vista de crear prospecto y rol 4 es asesor comercial
                            if ((Session["user_rolid"].ToString() != parametroAsesor ||
                                 Session["user_rolid"].ToString() != parametroAsesorU) && prospecto.tptrapros_id != 3)
                            {
                                context.alertaasesor.Add(new alertaasesor
                                {
                                    fecha = DateTime.Now,
                                    asesor = prospecto.asesor_id,
                                    propecto = consultaUltimoTercero.tercero_id,
                                    anfitrion = Convert.ToInt32(Session["user_usuarioid"]),
                                    recibido = false
                                });
                            }
                            //else
                            //{
                            //    TempData["BotonCotizarDeAsesor"] = "Cotizar";
                            //    TempData["ultimoTerceroId"] = consultaUltimoTercero.tercero_id;
                            //}

                            int guardarAlerta = context.SaveChanges();
                            if (guardarAlerta > 0)
                            {
                                // Para poner ocupado al asesor ya que se le acaba de asignar un prospecto
                                //se pone en estado 5, que esta esperando respuesta, de si acepta o rechaza al prospecto
                                sesion_logasesor ultimaSesionAsesor = context.sesion_logasesor.OrderByDescending(x => x.id)
                                    .FirstOrDefault(x => x.user_id == prospecto.asesor_id);
                                if (ultimaSesionAsesor != null)
                                {
                                    ultimaSesionAsesor.fecha_inicia = DateTime.Now;
                                    ultimaSesionAsesor.fecha_termina = DateTime.Now;
                                    ultimaSesionAsesor.estado = 5;
                                    context.Entry(ultimaSesionAsesor).State = EntityState.Modified;
                                    context.SaveChanges();
                                }

                                // Validacion para envio de mensaje de texto al asesor

                                #region Mensaje de texto al asesor

                                string msmEntregado = "";
                                try
                                {
                                    users buscaAsesor =
                                        context.users.FirstOrDefault(x => x.user_id == prospecto.asesor_id);
                                    string mensajeAlAsesor =
                                        "Se le ha asignado el prospecto " + consultaUltimoTercero.prinom_tercero + " " +
                                        consultaUltimoTercero.segnom_tercero + " "
                                        + consultaUltimoTercero.apellido_tercero + " " +
                                        consultaUltimoTercero.segapellido_tercero;
                                    string telefonoEnviarMsm = consultaUltimoTercero.celular_tercero;

                                    MensajesDeTexto enviar2 = new MensajesDeTexto();
                                    string status = enviar2.enviarmensaje(buscaAsesor.user_telefono, mensajeAlAsesor);

                                    if (status != "OK")
                                    {
                                        msmEntregado = ", no se ha enviado notificacion en sms, telefono no disponible";
                                    }
                                    else
                                    {
                                        msmEntregado = ", mensaje de texto enviado correctamente al asesor";
                                    }
                                }
                                catch (Exception)
                                {
                                    TempData["mensaje"] =
                                        "Prospecto agregado y asignado exitosamente pero el mesaje de texto al asesor no ha salido, verfique el servicio de mensajeria... ";
                                    DatosListasDesplegables(null);
                                    ParametrosBusqueda();
                                    BuscarFavoritos(menu);

                                    // Agregar seguimiento
                                    //string buscarParametroSeguimiento1 = (from parametro in context.icb_sysparameter
                                    //                                      where parametro.syspar_cod == "P56"
                                    //                                      select parametro.syspar_value).FirstOrDefault();
                                    var buscarParametroSeguimiento1 = context.Seguimientos.Where(x => x.EsManual == false && x.ModuloSeguimientos.Codigo == 4 && x.Codigo == 21).FirstOrDefault();
                                    if (buscarParametroSeguimiento1!=null)
                                    {
                                        context.seguimientotercero.Add(new seguimientotercero
                                        {
                                            idtercero = consultaUltimoTercero.tercero_id,
                                            fecha = DateTime.Now,
                                            nota = "Se Creo el Prospecto",
                                            tipo = buscarParametroSeguimiento1.Id,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                                        });
                                        int guardar = context.SaveChanges();
                                    }

                                    return RedirectToAction("Create", new { menu });
                                }

                                #endregion

                                //// Fin validacion para envio de mensaje de texto al asesor

                                // Validacion para envio de mensaje de texto al cliente (confirmacion visita)

                                #region Mensaje de texto cliente

                                if (prospectoNuevo.celular_tercero != null)
                                {
                                    pmensajevisita mensaje = new pmensajevisita();
                                    try
                                    {
                                        int? buscarClienteProspecto = context.prospectos
                                            .FirstOrDefault(x => x.id == prospectoNuevo.id).idtercero;
                                        int? id_tercero_cliente = buscarClienteProspecto;
                                        icb_terceros buscaCliente =
                                            context.icb_terceros.FirstOrDefault(x =>
                                                x.tercero_id == id_tercero_cliente);
                                        string nombre = !string.IsNullOrEmpty(buscaCliente.prinom_tercero)
                                            ? buscaCliente.prinom_tercero + " "
                                            : "";
                                        string segundoNombre = !string.IsNullOrEmpty(buscaCliente.segnom_tercero)
                                            ? buscaCliente.segnom_tercero + " "
                                            : "";
                                        string apellido = !string.IsNullOrEmpty(buscaCliente.apellido_tercero)
                                            ? buscaCliente.apellido_tercero + " "
                                            : "";
                                        string segundoApellido = !string.IsNullOrEmpty(buscaCliente.segapellido_tercero)
                                            ? buscaCliente.segapellido_tercero + " "
                                            : "";

                                        string urlActual0 = Request.Url.Scheme + "://" + Request.Url.Authority;
                                        string llave = EncriptarLlave(id_tercero_cliente.ToString());
                                        string url = urlActual0 += @"/prospectos/confirmarVisita?id=" + llave;
                                        string asd = Convert.ToString(url);

                                        string mensajeAlCliente =
                                            "Señor(a) " + nombre + segundoNombre + apellido + segundoApellido +
                                            ", Bienvenido a caminos, gracias por preferirnos. " +
                                            "Por favor confirme su visita en el siguiente enlace: " + asd;

                                        MensajesDeTexto enviar2 = new MensajesDeTexto();
                                        string status = enviar2.enviarmensaje(buscaCliente.celular_tercero,
                                            mensajeAlCliente);

                                        mensaje.llave = llave;
                                        mensaje.fecha_envio = DateTime.Now;
                                        mensaje.idtercero = Convert.ToInt32(id_tercero_cliente);

                                        if (status != "OK")
                                        {
                                            mensaje.llegadamensaje = false;
                                        }
                                        else
                                        {
                                            mensaje.llegadamensaje = true;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        mensaje.llegadamensaje = false;
                                    }

                                    context.pmensajevisita.Add(mensaje);
                                    context.SaveChanges();
                                }

                                #endregion

                                // Fin validacion para envio de mensaje de texto al cliente (confirmacion visita)

                                TempData["mensaje"] = "Prospecto agregado y asignado exitosamente" + msmEntregado;
                                DatosListasDesplegables(null);
                                ParametrosBusqueda();
                                BuscarFavoritos(menu);

                                // Agregar seguimiento
                                //string buscarParametroSeguimiento2 = (from parametro in context.icb_sysparameter
                                //                                      where parametro.syspar_cod == "P56"
                                //                                      select parametro.syspar_value).FirstOrDefault();
                                var buscarParametroSeguimiento2 = context.Seguimientos.Where(x => x.EsManual == false && x.ModuloSeguimientos.Codigo == 4 && x.Codigo == 21).FirstOrDefault();
                                if (buscarParametroSeguimiento2!=null)
                                {
                                    context.seguimientotercero.Add(new seguimientotercero
                                    {
                                        idtercero = consultaUltimoTercero.tercero_id,
                                        fecha = DateTime.Now,
                                        nota = "Se creo el prospecto",
                                        tipo = buscarParametroSeguimiento2.Id,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                                    });
                                    int guardar = context.SaveChanges();
                                }

                                return RedirectToAction("Create", new { menu });
                            }

                            TempData["mensaje"] = "Prospecto creado con éxito";
                        }
                        else
                        {
                            TempData["mensaje"] = "Prospecto creado con éxito";
                        }

                        #endregion
                    }
                    else
                    {
                        //Si el registro del prospecto lo hizo un rol diferente a la anfitriona entrara a este segmento de codigo
                        context.SaveChanges();
                        TempData["mensaje"] = "Prospecto agregado y asignado exitosamente";
                        DatosListasDesplegables(null);
                        ParametrosBusqueda();
                        BuscarFavoritos(menu);
                        icb_terceros consultaUltimoTercero =
                            context.icb_terceros.OrderByDescending(x => x.tercero_id).FirstOrDefault();

                        // Validacion para envio de mensaje de texto al asesor

                        #region Mensaje de texto al asesor


                        #endregion

                        ////// Fin validacion para envio de mensaje de texto al asesor

                        //// Validacion para envio de mensaje de texto al cliente (confirmacion visita)

                        #region Mensaje de texto cliente



                        #endregion

                        // Fin validacion para envio de mensaje de texto al cliente (confirmacion visita)

                        // Agregar seguimiento

                        #region seguimiento

                        //string buscarParametroSeguimiento2 = (from parametro in context.icb_sysparameter
                        //                                      where parametro.syspar_cod == "P56"
                        //                                      select parametro.syspar_value).FirstOrDefault();
                        var buscarParametroSeguimiento3 = context.Seguimientos.Where(x => x.EsManual == false && x.ModuloSeguimientos.Codigo == 4 && x.Codigo == 21).FirstOrDefault();
                        if (buscarParametroSeguimiento3!=null)
                        {
                            context.seguimientotercero.Add(new seguimientotercero
                            {
                                idtercero = consultaUltimoTercero.tercero_id,
                                fecha = DateTime.Now,
                                nota = "Se creo el prospecto",
                                tipo = buscarParametroSeguimiento3.Id,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                            });
                        }

                        tareasasignadas tarea = new tareasasignadas
                        {
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            estado = true,
                            idusuarioasignado = Convert.ToInt32(prospecto.asesor_id),
                            idtercero = Convert.ToInt32(consultaUltimoTercero.tercero_id),
                            notas = "Prospecto por gestionar"
                        };
                        context.tareasasignadas.Add(tarea);

                        int guardar = context.SaveChanges();

                        #endregion

                        return RedirectToAction("Create", new { menu });
                    }

                    #endregion
                    // }


                }
            }
            else
            {
                List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();
            }

            DatosListasDesplegables(prospecto);
            ParametrosBusqueda();
            BuscarFavoritos(menu);
            return View(prospecto);
        }

        public JsonResult buscarPermisos()
        {
            int rol = Convert.ToInt32(Session["user_rolid"]);

            List<int> data = context.rolacceso.Where(x => x.idrol == rol).Select(x => x.idpermiso).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDocumentoCliente(string numDocumento)
        {
            var buscarTercero = (from tercero in context.icb_terceros
                                 join p in context.prospectos
                                     on tercero.doc_tercero equals p.documento
                                 where tercero.doc_tercero == numDocumento && p.estado == 1
                                 orderby p.fec_creacion descending
                                 select new
                                 {
                                     tercero.tercero_id,
                                     tercero.tpdoc_id,
                                     tercero.prinom_tercero,
                                     tercero.segnom_tercero,
                                     tercero.apellido_tercero,
                                     tercero.segapellido_tercero,
                                     tercero.email_tercero,
                                     tercero.telf_tercero,
                                     tercero.celular_tercero
                                 }).FirstOrDefault();
            if (buscarTercero != null)
            {
                return Json(new { encontrado = true, buscarTercero }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { encontrado = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarTelefonoCliente(string telefono)
        {
            var buscarTercero = (from tercero in context.icb_terceros
                                 join p in context.prospectos
                                     on tercero.doc_tercero equals p.documento
                                 where tercero.telf_tercero == telefono && p.estado == 1
                                 orderby p.fec_creacion descending
                                 select new
                                 {
                                     tercero.tercero_id,
                                     tercero.tpdoc_id,
                                     tercero.prinom_tercero,
                                     tercero.segnom_tercero,
                                     tercero.apellido_tercero,
                                     tercero.segapellido_tercero,
                                     tercero.email_tercero,
                                     tercero.telf_tercero,
                                     tercero.celular_tercero
                                 }).FirstOrDefault();
            if (buscarTercero != null)
            {
                return Json(new { encontrado = true, buscarTercero }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { encontrado = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarCelularCliente(string celular)
        {
            var buscarTercero = (from tercero in context.icb_terceros
                                 join p in context.prospectos
                                     on tercero.doc_tercero equals p.documento
                                 where tercero.celular_tercero == celular && p.estado == 1
                                 orderby p.fec_creacion descending
                                 select new
                                 {
                                     tercero.tercero_id,
                                     tercero.tpdoc_id,
                                     tercero.prinom_tercero,
                                     tercero.segnom_tercero,
                                     tercero.apellido_tercero,
                                     tercero.segapellido_tercero,
                                     tercero.email_tercero,
                                     tercero.telf_tercero,
                                     tercero.celular_tercero
                                 }).FirstOrDefault();
            if (buscarTercero != null)
            {
                return Json(new { encontrado = true, buscarTercero }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { encontrado = false }, JsonRequestBehavior.AllowGet);
        }
        //public JsonResult ReasignarProspec( int? id_asesor)
        //{
        //    if (id_asesor) { 


        //    return Json(new { respuesta = "ya tiene asignado un aseor" }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        public JsonResult ReasignarProspecto(int? id_Tercero, int? id_tipo_tramite, int? id_asesor, string motivo,
            int? menu, int? idAlerta)
        {
            if (id_Tercero == null || id_tipo_tramite == null || id_asesor == null)

            // if ( id_tipo_tramite == null || id_asesor == null)
            {
                return Json(new { respuesta = "Campos vacios, por favor valide" }, JsonRequestBehavior.AllowGet);
            }
            prospectos objTerceros = new prospectos();// leonardo cambio
                                                      //var buscarMiAsesor = context.prospectos.Find(objTerceros.id == id_Tercero); //leonardo cambio

            // icb_terceros objTerceros = new icb_terceros();
            //var buscarMiAsesor = context.icb_terceros.Find(objTerceros.tercero_id==id_Tercero);
            // var Miasesor = buscarMiAsesor.asesor_id; 

            alertaasesor buscarAlerta = context.alertaasesor.Where(x => x.propecto == id_Tercero && x.finalizado == false).OrderByDescending(x => x.fecha).FirstOrDefault();
            // Se debe guardar un registro en la tabla alertaasesor para que este lo atienda
            prospectos infoTercero = context.prospectos.FirstOrDefault(x => x.id == id_Tercero);
            icb_terceros consultaTercero = context.icb_terceros.FirstOrDefault(x => x.tercero_id == infoTercero.idtercero);
            //prospectos consultaTercero = context.prospectos.FirstOrDefault(x => x.idtercero == infoTercero.idtercero);

            string parametroAsesor = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P88").syspar_value;
            string parametroAsesorU = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P99").syspar_value;
            // Si soy rol Asesor no me llegara notificacion, porque el mismo se ha asignado, la notificacion llega cuando la anfitriona asigna un asesor
            // El numero tres se refiere a el tipo de tramite chevyplan de la vista de crear prospecto y rol 4 es asesor comercial
            if ((Session["user_rolid"].ToString() != parametroAsesor ||
                 Session["user_rolid"].ToString() != parametroAsesorU) && id_tipo_tramite != 3)
            {
                if (buscarAlerta != null)
                {
                    buscarAlerta.finalizado = true;
                    buscarAlerta.rechazado = true;
                    context.Entry(buscarAlerta).State = EntityState.Modified;
                }

                context.alertaasesor.Add(new alertaasesor
                {
                    fecha = DateTime.Now,
                    asesor = id_asesor,
                    propecto = consultaTercero.tercero_id,
                    anfitrion = Convert.ToInt32(Session["user_usuarioid"]),
                    recibido = false,
                    reasignado = false
                });

                infoTercero.asesor_id = id_asesor;
                context.Entry(infoTercero).State = EntityState.Modified;

                if (motivo != "")
                {
                    var buscarParametroSeguimiento4 = context.Seguimientos.Where(x => x.EsManual == false && x.ModuloSeguimientos.Codigo == 4 && x.Codigo == 22).FirstOrDefault();
                    context.seguimientotercero.Add(new seguimientotercero
                    {
                        idtercero = consultaTercero.tercero_id,
                        tipo = buscarParametroSeguimiento4.Id,
                        nota = "Se Reasigno el Prospecto " + motivo,
                        fecha = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                    });
                }
            }

            int guardarAlerta = context.SaveChanges();
            if (guardarAlerta > 0)
            {
                sesion_logasesor ultimaSesionAsesor = context.sesion_logasesor.OrderByDescending(x => x.id)
                    .FirstOrDefault(x => x.user_id == id_asesor);
                if (ultimaSesionAsesor != null)
                {
                    ultimaSesionAsesor.fecha_inicia = DateTime.Now;
                    ultimaSesionAsesor.fecha_termina = DateTime.Now;
                    ultimaSesionAsesor.estado = 5;
                    context.Entry(ultimaSesionAsesor).State = EntityState.Modified;
                    context.SaveChanges();
                }

                int idProspecto = infoTercero.id;

                #region asignacion

                asignacion asignacion = new asignacion
                {
                    idProspecto = idProspecto,
                    idAsesor = Convert.ToInt32(id_asesor),
                    estado = false,
                    fechaInicio = DateTime.Now
                };
                context.asignacion.Add(asignacion);
                context.SaveChanges();

                #endregion

                #region Mensaje de texto

                // Validacion para envio de mensaje de texto al asesor
                try
                {
                    users buscaAsesor = context.users.FirstOrDefault(x => x.user_id == id_asesor);
                    string mensajeAlAsesor = "Se le ha reasignado el prospecto " + consultaTercero.prinom_tercero + " " +
                                          consultaTercero.segnom_tercero + " "
                                          + consultaTercero.apellido_tercero + " " +
                                          consultaTercero.segapellido_tercero;
                    string telefonoEnviarMsm = buscaAsesor.user_telefono;

                    MensajesDeTexto enviar2 = new MensajesDeTexto();
                    string status = enviar2.enviarmensaje(telefonoEnviarMsm, mensajeAlAsesor);

                    if (status != "OK")
                    {
                    }
                    else
                    {
                    }
                }
                catch (Exception)
                {
                    TempData["mensaje"] =
                        "Prospecto agregado y asignado exitosamente pero el mesaje de texto al asesor no ha salido, verfique el servicio de mensajeria... ";
                    DatosListasDesplegables(null);
                    ParametrosBusqueda();
                    BuscarFavoritos(menu);

                    // Agregar seguimiento
                    //string buscarParametroSeguimiento1 = (from parametro in context.icb_sysparameter
                    //                                      where parametro.syspar_cod == "P56"
                    //                                      select parametro.syspar_value).FirstOrDefault();
                    var buscarParametroSeguimiento5 = context.Seguimientos.Where(x => x.EsManual == false && x.ModuloSeguimientos.Codigo == 4 && x.Codigo == 22).FirstOrDefault();
                    if (buscarParametroSeguimiento5!=null)
                    {
                        context.seguimientotercero.Add(new seguimientotercero
                        {
                            idtercero = consultaTercero.tercero_id,
                            fecha = DateTime.Now,
                            nota = "Se Reasigno el Prospecto",
                            tipo = buscarParametroSeguimiento5.Id,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                        });
                        int guardar = context.SaveChanges();
                    }

                    //return RedirectToAction("Create", new { menu });
                }

                //// Fin validacion para envio de mensaje de texto al asesor

                #endregion

                //var buscarProspecto = context.prospectos.OrderByDescending(x => x.id).FirstOrDefault(x => x.idtercero == id_Prospecto);
                if (infoTercero != null)
                {
                    infoTercero.asesor_id = Convert.ToInt32(id_asesor);
                    context.Entry(infoTercero).State = EntityState.Modified;
                    context.SaveChanges();

                }
                return Json(new { respuesta = "Usuario asignado exitosamente" }, JsonRequestBehavior.AllowGet);

            }

            return Json(new { respuesta = "Error de conexion con base de datos, por favor valide" },
                JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarAsesorAsignado(int prospecto)
        {
            int tercero = (from p in context.prospectos
                           where p.id == prospecto
                           orderby p.id descending
                           select p.id).FirstOrDefault();

            var asesorAsignado2 = (from u in context.users
                                   join p in context.prospectos
                                       on u.user_id equals p.asesor_id into temp
                                   from aa in temp.DefaultIfEmpty()
                                   join a in context.asignacion
                                       on aa.asesor_id equals a.idAsesor
                                   where a.idProspecto == tercero
                                   select new
                                   {
                                       id = u.user_id,
                                       idasignacion = a.id,
                                       nombre = u.user_nombre + " " + u.user_apellido
                                   }).ToList();

            var asesorAsignado = asesorAsignado2.OrderByDescending(d => d.idasignacion).FirstOrDefault();
            int tipotramite = context.prospectos.Where(f => f.id == tercero).Select(f => f.tptramite).FirstOrDefault();
            var data = new
            {
                asesorAsignado,
                tipotramite
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult modeloGeneral()
        {
            var data = context.vmodelog.Select(x => new { x.id, x.Descripcion }).OrderBy(x => x.Descripcion).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult modelosXGeneralInt(int id)
        {
            var data = context.modelo_vehiculo.Where(x => x.modelogkit == id).Select(x => new { x.modvh_codigo, x.modvh_nombre }).ToList();
            ViewBag.modeloOpcional = new SelectList(context.modelo_vehiculo.Where(x => x.modelogkit == id).OrderBy(x => x.modvh_nombre),
                "modvh_codigo", "modvh_nombre");
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult modelosXGeneralOpc(int id)
        {
            var data = context.modelo_vehiculo.Where(x => x.modelogkit == id).Select(x => new { x.modvh_codigo, x.modvh_nombre }).ToList();
            ViewBag.modeloOpcional2 = new SelectList(context.modelo_vehiculo.Where(x => x.modelogkit == id).OrderBy(x => x.modvh_nombre),
                "modvh_codigo", "modvh_nombre");
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public void DatosListasDesplegables(Prospecto prospecto)
        {
            ViewBag.RolLog = Convert.ToInt32(Session["user_rolid"]);
            ViewBag.tipoDocumento = new SelectList(context.tp_documento.OrderBy(x => x.tpdoc_nombre), "tpdoc_id",
                "tpdoc_nombre");
            ViewBag.medcomun_id = new SelectList(context.icb_medio_comunicacion.OrderBy(x => x.medcomun_descripcion),
                "medcomun_id", "medcomun_descripcion", prospecto != null ? prospecto.medcomun_id : 0);

            ViewBag.modeloOpcional = new SelectList(context.modelo_vehiculo.OrderBy(x => x.modvh_nombre),
                "modvh_codigo", "modvh_nombre");
            ViewBag.modeloOpcional2 = new SelectList(context.modelo_vehiculo.OrderBy(x => x.modvh_nombre),
                "modvh_codigo", "modvh_nombre");


            List<icb_tptramite_prospecto> tipoTramite = context.icb_tptramite_prospecto.OrderBy(x => x.tptrapros_descripcion).ToList();
            ViewBag.tptrapros_id = new SelectList(tipoTramite, "tptrapros_id", "tptrapros_descripcion",
                prospecto != null ? prospecto.tptrapros_id : 0);
            ViewBag.tptrapros_idReasignado = new SelectList(tipoTramite, "tptrapros_id", "tptrapros_descripcion",
                prospecto != null ? prospecto.tptrapros_id : 0);
            ViewBag.genero_tercero = new SelectList(context.gen_tercero.OrderBy(x => x.gentercero_nombre),
                "gentercero_id", "gentercero_nombre", prospecto != null ? prospecto.genero_tercero : 0);
            ViewBag.tporigen_id = new SelectList(context.tp_origen.OrderBy(x => x.tporigen_nombre), "tporigen_id",
                "tporigen_nombre", prospecto != null ? prospecto.tporigen_id : 0);
            int? tipoOrigen = prospecto != null ? prospecto.tporigen_id : 0;
            ViewBag.subfuente_id =
                new SelectList(context.tp_subfuente.Where(x => x.fuente == tipoOrigen).OrderBy(x => x.subfuente), "id",
                    "subfuente", prospecto != null ? prospecto.subfuente_id : 0);
        }

        public ActionResult confirmarVisita(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            pmensajevisita buscarTerceroMensaje = context.pmensajevisita.FirstOrDefault(x => x.llave == id);
            if (buscarTerceroMensaje == null)
            {
                return HttpNotFound();
            }

            int idTercero = buscarTerceroMensaje.idtercero;
            icb_terceros buscarTercero = context.icb_terceros.FirstOrDefault(x => x.tercero_id == idTercero);
            string nombreCompleto = buscarTercero.prinom_tercero + " " + buscarTercero.segnom_tercero + " " +
                                 buscarTercero.apellido_tercero + " " + buscarTercero.segapellido_tercero;

            prospectos buscarProspecto = context.prospectos.OrderByDescending(x => x.id)
                .FirstOrDefault(x => x.idtercero == idTercero);

            ViewBag.prospecto = buscarProspecto.id;
            ViewBag.nombreTercero = nombreCompleto.ToUpper();

            return View(buscarProspecto);
        }

        public JsonResult confirmacionVisita(bool confirma, int prospecto)
        {
            prospectos buscar = context.prospectos.FirstOrDefault(x => x.id == prospecto);
            if (buscar != null)
            {
                buscar.visita = confirma;
                context.Entry(buscar).State = EntityState.Modified;
                context.SaveChanges();
                return Json(new { exito = true }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { exito = false }, JsonRequestBehavior.AllowGet);
        }

        public string EncriptarLlave(string entrada)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            md5.ComputeHash(Encoding.ASCII.GetBytes(entrada));
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

            return strLlave.ToString();
        }

        public ActionResult Update(int id, int? menu)
        {
            icb_terceros buscarProspecto = context.icb_terceros.FirstOrDefault(x => x.tercero_id == id);
            if (buscarProspecto != null)
            {
                Prospecto prospecto = new Prospecto
                {
                    digito_verificacion = buscarProspecto.digito_verificacion,
                    razonSocial = buscarProspecto.razon_social,
                    tipoDocumento = buscarProspecto.tpdoc_id,
                    numDocumento = buscarProspecto.doc_tercero,
                    tercero_id = buscarProspecto.tercero_id,
                    prinom_tercero = buscarProspecto.prinom_tercero,
                    segnom_tercero = buscarProspecto.segnom_tercero,
                    apellido_tercero = buscarProspecto.apellido_tercero,
                    segapellido_tercero = buscarProspecto.segapellido_tercero,
                    email_tercero = buscarProspecto.email_tercero,
                    genero_tercero = buscarProspecto.genero_tercero,
                    medcomun_id = buscarProspecto.medcomun_id,
                    numeroAmigos = buscarProspecto.numamigos ?? 0,
                    numeroFamiliares = buscarProspecto.numfamiliares ?? 0,
                    tporigen_id = buscarProspecto.origen_id ?? 0,
                    observacion = buscarProspecto.observaciones,
                    celular_tercero = buscarProspecto.celular_tercero,
                    telf_tercero = buscarProspecto.telf_tercero,
                    habdtautor_celular = buscarProspecto.habdtautor_celular,
                    habdtautor_correo = buscarProspecto.habdtautor_correo,
                    habdtautor_msm = buscarProspecto.habdtautor_msm,
                    habdtautor_whatsapp = buscarProspecto.habdtautor_watsap,
                    eventoOrigen = buscarProspecto.sitioevento,
                    subfuente_id = buscarProspecto.subfuente
                };
                DatosListasDesplegables(prospecto);
                ViewBag.eventoOrigen = buscarProspecto.sitioevento;
                ParametrosBusqueda();
                BuscarFavoritos(menu);
                return View(prospecto);
            }

            DatosListasDesplegables(new Prospecto());
            ParametrosBusqueda();
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Update(Prospecto modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                icb_terceros buscaTercero = context.icb_terceros.FirstOrDefault(x => x.tercero_id == modelo.tercero_id);
                if (buscaTercero != null)
                {
                    if (modelo.eventoOrigen == 0)
                    {
                        modelo.eventoOrigen = null;
                    }

                    icb_terceros buscarTercero = context.icb_terceros.FirstOrDefault(x =>
                        x.celular_tercero != null && x.telf_tercero != null &&
                        (x.celular_tercero == modelo.celular_tercero || x.telf_tercero == modelo.telf_tercero));
                    if (buscarTercero != null && buscarTercero.tercero_id != modelo.tercero_id)
                    {
                        string campoRepetido = buscarTercero.telf_tercero == modelo.telf_tercero ? "telefono" : "";
                        campoRepetido = buscarTercero.celular_tercero == modelo.celular_tercero ? "celular" : "";
                        campoRepetido = buscarTercero.doc_tercero == modelo.numDocumento ? "documento" : "";
                        TempData["mensaje_error"] =
                            "Ya existe un prospecto registrado con el mismo " + campoRepetido + ", su nombre es "
                            + buscarTercero.prinom_tercero + buscarTercero.apellido_tercero +
                            (buscarTercero.razon_social != null ? buscarTercero.razon_social : "") + "";
                    }
                    else
                    {
                        buscaTercero.razon_social = modelo.razonSocial;
                        buscaTercero.digito_verificacion = modelo.digito_verificacion;
                        buscaTercero.prinom_tercero = modelo.prinom_tercero;
                        buscaTercero.segnom_tercero = modelo.segnom_tercero;
                        buscaTercero.apellido_tercero = modelo.apellido_tercero;
                        buscaTercero.segapellido_tercero = modelo.segapellido_tercero;
                        buscaTercero.email_tercero = modelo.email_tercero;
                        buscaTercero.medcomun_id = modelo.medcomun_id;
                        buscaTercero.genero_tercero = modelo.genero_tercero;
                        buscaTercero.numfamiliares = modelo.numeroFamiliares;
                        buscaTercero.numamigos = modelo.numeroAmigos;
                        buscaTercero.celular_tercero = modelo.celular_tercero;
                        buscaTercero.telf_tercero = modelo.telf_tercero;
                        buscaTercero.tpdoc_id = modelo.tipoDocumento;
                        buscaTercero.doc_tercero = modelo.numDocumento;
                        buscaTercero.observaciones = modelo.observacion;
                        buscaTercero.habdtautor_celular = modelo.habdtautor_celular;
                        buscaTercero.habdtautor_correo = modelo.habdtautor_correo;
                        buscaTercero.habdtautor_msm = modelo.habdtautor_msm;
                        buscaTercero.habdtautor_watsap = modelo.habdtautor_whatsapp;
                        buscaTercero.sitioevento = modelo.eventoOrigen;
                        buscaTercero.subfuente = modelo.subfuente_id;
                        context.Entry(buscaTercero).State = EntityState.Modified;
                        bool guardar = context.SaveChanges() > 0;
                        if (guardar)
                        {
                            TempData["mensajeCorrecto"] = "El prospecto se ha actualizado correctamente";

                            // Se cambia el estado del asesor a ocupado para que no se asigne a la siguiente persona prospecto
                            //var ultimaSesionAsesor = context.sesion_logasesor.OrderByDescending(x => x.id).FirstOrDefault(x => x.user_id == modelo.asesor_id);
                            //if (ultimaSesionAsesor != null)
                            //{
                            //    ultimaSesionAsesor.estado = 2;
                            //    context.Entry(ultimaSesionAsesor).State = System.Data.Entity.EntityState.Modified;
                            //    //context.SaveChanges();
                            //}

                            // Si soy rol Asesor no me llegara notificacion, porque el mismo se ha asignado, la notificacion llega cuando la anfitriona asigna un asesor
                            //if (Session["user_rolid"].ToString() != "4")
                            //{
                            //    context.alertaasesor.Add(new alertaasesor()
                            //    {
                            //        fecha = DateTime.Now,
                            //        asesor = modelo.asesor_id,
                            //        propecto = modelo.tercero_id??0,
                            //        anfitrion = Convert.ToInt32(Session["user_usuarioid"]),
                            //        recibido = true
                            //    });
                            //}
                            //else
                            //{
                            //    TempData["BotonCotizarDeAsesor"] = "Cotizar";
                            //    TempData["ultimoTerceroId"] = modelo.tercero_id;
                            //}

                            // Se actualiza el alerta que le llega al asesor
                            // Se debe guardar o actualizar un registro en la tabla alertaasesor para que este lo atienda
                            //var usuario = Convert.ToInt32(Session["user_usuarioid"]);
                            //var consultaUltimoAlerta = context.alertaasesor.FirstOrDefault(x => x.aprobado==null && x.anfitrion==usuario);
                            //if (consultaUltimoAlerta != null) {
                            //    consultaUltimoAlerta.asesor = modelo.asesor_id;
                            //    context.Entry(consultaUltimoAlerta).State = System.Data.Entity.EntityState.Modified;
                            //context.SaveChanges();
                            //}

                            DatosListasDesplegables(modelo);
                            ParametrosBusqueda();
                            BuscarFavoritos(menu);
                            return View(new Prospecto());
                        }

                        TempData["mensajeError"] = "Problemas de conexion, intente mas tarde";
                    }
                }
                else
                {
                    TempData["mensajeError"] = "No se encontro el tercero";
                }
            }

            ViewBag.medcomun_id = new SelectList(context.icb_medio_comunicacion, "medcomun_id", "medcomun_descripcion");
            ViewBag.modvh_codigo = new SelectList(context.modelo_vehiculo, "modvh_codigo", "modvh_nombre");
            //ViewBag.tptrapros_id = new SelectList(context.icb_tptramite_prospecto, "tptrapros_id", "tptrapros_descripcion", modelo.tptrapros_id);
            string generoAux = Request["genero"];
            if (generoAux != null)
            {
                modelo.genero_tercero = Convert.ToInt32(generoAux);
            }

            ViewBag.genero_tercero = context.gen_tercero.ToList();
            ViewBag.tporigen_id = new SelectList(context.tp_origen, "tporigen_id", "tporigen_nombre");
            ParametrosBusqueda();
            BuscarFavoritos(menu);
            return View(modelo);
        }

        public JsonResult BuscarLugaresEvento(int idOrigen)
        {

            tp_origen buscarSiRequiere = context.tp_origen.FirstOrDefault(x => x.tporigen_id == idOrigen);
            if (buscarSiRequiere != null)
            {
                bool requiereSubFuente = false;
                var listaSubFuentes = (from subFuentes in context.tp_subfuente
                                       where subFuentes.fuente == idOrigen
                                       select new
                                       {
                                           subFuentes.id,
                                           subFuentes.subfuente,
                                       }).ToList();



                if (buscarSiRequiere.subfuente)
                {
                    requiereSubFuente = true;
                }

                if (buscarSiRequiere.evento)
                {
                    DateTime fechaHoy = DateTime.Now;
                    List<veventosorigen> buscarEventosAsociados = (from eventosLugares in context.veventosorigen
                                                                   where eventosLugares.evento_id == idOrigen
                                                                         && DbFunctions.TruncateTime(eventosLugares.fechaini) <= DbFunctions.TruncateTime(fechaHoy)
                                                                         && DbFunctions.TruncateTime(DbFunctions.AddDays(eventosLugares.fechafin,
                                                                             eventosLugares.diasamostrar)) >= DbFunctions.TruncateTime(fechaHoy)
                                                                   select eventosLugares).ToList();

                    var eventos = buscarEventosAsociados.Select(x => new
                    {
                        x.id,
                        x.descripcion
                    });
                    var data = new
                    {
                        requiere = true,
                        requiereSubFuente,
                        eventos,
                        listaSubFuentes
                    };
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var data = new
                    {
                        requiereSubFuente,
                        listaSubFuentes
                    };
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { requiere = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarAsesoresDisponibles(int? id_tramite)
        {
            #region codigo comentado

            //var buscaAsesores = context.users.Where(x => x.rol_id == 4).ToList();
            //var idUsuarioActual = Convert.ToInt32(Session["user_usuarioid"]);
            //if (idUsuarioActual!=0) {
            //    var lastConection = context.sesion_logasesor.OrderByDescending(x => x.fecha_inicia).FirstOrDefault(x => x.user_id == idUsuarioActual);
            //    if (lastConection !=null) {
            //        lastConection.fecha_termina = DateTime.Now;
            //        context.Entry(lastConection).State = System.Data.Entity.EntityState.Modified;
            //        context.SaveChanges();
            //    }
            //}

            //var buscaAsesores = (from usuarios in context.users
            //                    where usuarios.rol_id == 4
            //                    select new {
            //                        ultimaConexion = context.sesion_logasesor.OrderByDescending(x => x.fecha_termina).FirstOrDefault(x=>x.user_id==usuarios.user_id),
            //                        usuarios.sesion,
            //                        usuarios.user_id,
            //                        usuarios.user_nombre,
            //                        usuarios.user_apellido
            //                    }).OrderBy(x=>x.ultimaConexion.fecha_inicia).ToList();

            //var listaAsesores = new List<SelectListItem>();
            //foreach (var asesor in buscaAsesores) {
            //    if (asesor.ultimaConexion != null) {
            //        DateTime ultimaConexion = asesor.ultimaConexion.fecha_termina ?? DateTime.Now;
            //        TimeSpan differenciaSegundos = DateTime.Now - ultimaConexion;
            //        if (differenciaSegundos.TotalSeconds < 15 && asesor.ultimaConexion.estado == 1)
            //        {
            //            listaAsesores.Add(new SelectListItem() { Value = asesor.user_id.ToString(), Text = asesor.user_nombre + " " + asesor.user_apellido });
            //        }
            //    } 
            //}

            //var listaAsesores = new List<SelectListItem>();
            //foreach (var item in buscaAsesores)
            //{
            //    if (item.sesion == true)
            //    {
            //        listaAsesores.Add(new SelectListItem() { Value = item.user_id.ToString(), Text = item.user_nombre + " " + item.user_apellido, Disabled = true });
            //    }
            //    else {
            //        listaAsesores.Add(new SelectListItem() { Value = item.user_id.ToString(), Text = item.user_nombre + " " + item.user_apellido, Disabled = false });
            //    }
            //}

            #endregion

            List<SelectListItem> listaAsesores = new List<SelectListItem>();
            List<SelectListItem> listaAsesoresPlanta = new List<SelectListItem>();
            int bodegaId = Convert.ToInt32(Session["user_bodega"]);

            string parametroAsesor = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P88").syspar_value;
            string parametroAsesorU = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P99").syspar_value;

            if (Session["user_rolid"].ToString() == parametroAsesor ||
                Session["user_rolid"].ToString() == parametroAsesorU)
            {
                int usuarioId = Convert.ToInt32(Session["user_usuarioid"]);




                users buscarAsesor = context.users.FirstOrDefault(x => x.user_id == usuarioId);
                listaAsesores.Add(new SelectListItem
                {
                    Value = buscarAsesor.user_id.ToString(),
                    Text = buscarAsesor.user_nombre + " " + buscarAsesor.user_apellido
                });

            }
            else
            {
                DateTime fechaHoy = DateTime.Now;
                var buscaAsesores = (from usuarios in context.users
                                     join tipoTramite in context.tipotramiteasesor
                                         on usuarios.user_id equals tipoTramite.id_usuario
                                     where tipoTramite.id_tipotramite == id_tramite
                                     //where usuarios.rol_id == 4
                                     select new
                                     {
                                         ultimaConexion = context.sesion_logasesor.OrderByDescending(x => x.fecha_inicia).Where(x =>
                                             x.user_id == usuarios.user_id /* && x.bodega == bodegaId*/
                                             && x.fecha_inicia.Year == fechaHoy.Year && x.fecha_inicia.Month == fechaHoy.Month &&
                                             x.fecha_inicia.Day == fechaHoy.Day).FirstOrDefault(),
                                         usuarios.sesion,
                                         usuarios.user_id,
                                         usuarios.rol_id,
                                         usuarios.user_nombre,
                                         usuarios.user_apellido,
                                         usuarios.fechainiplanta,
                                         usuarios.fechafinplanta
                                     }).OrderBy(x => x.ultimaConexion.fecha_inicia).ToList();

                buscaAsesores = buscaAsesores.Where(d => d.ultimaConexion != null).OrderByDescending(d => d.ultimaConexion.fecha_inicia).ToList();

                // P31 es el codigo del parametro de las horas en las que un asesor debe estar libre antes de asignarle algun cliente nuevo por anfitriona
                icb_sysparameter buscarParametroHorasAsesor = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P31");
                //var parametroAsesor = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P88").syspar_value;
                foreach (var asesor in buscaAsesores)
                {
                    if (asesor.ultimaConexion != null)
                    {
                        DateTime fechaHoyInicio = DateTime.Now;
                        DateTime fechaHoyFin = DateTime.Now;
                        bool citaInterfiere = false;
                        // 4 es el id del rol asesor en base de datos, tabla rols
                        if (asesor.rol_id == Convert.ToInt32(parametroAsesor) &&
                            asesor.ultimaConexion.bodega == bodegaId)
                        //if (asesor.rol_id == 4 && asesor.ultimaConexion.bodega == bodegaId)
                        {
                            List<agenda_asesor> buscarCitasHoyAsesor = context.agenda_asesor.Where(x =>
                                x.asesor_id == asesor.user_id && x.desde.Year == fechaHoyInicio.Year &&
                                x.desde.Month == fechaHoyInicio.Month && x.desde.Day == fechaHoyInicio.Day).ToList();
                            int horasDeMasAsesor = buscarParametroHorasAsesor != null
                                ? Convert.ToInt32(buscarParametroHorasAsesor.syspar_value)
                                : 0;

                            foreach (agenda_asesor cita in buscarCitasHoyAsesor)
                            {
                                if (fechaHoyInicio >=
                                    cita.desde.AddHours(-horasDeMasAsesor) /*&& fechaHoyFin <= cita.hasta*/ &&
                                    cita.estado != "Atendida")
                                {
                                    // Significa que el asesor tiene una cita validando el tiempo libre antes y despues de cada cita
                                    citaInterfiere = true;
                                    break;
                                }
                            }
                        }
                        //else if (asesor.rol_id == 2030 && asesor.ultimaConexion.bodega == bodegaId)
                        else if (asesor.rol_id == Convert.ToInt32(parametroAsesorU) &&
                                 asesor.ultimaConexion.bodega == bodegaId)
                        {
                            List<agenda_asesor> buscarCitasHoyAsesor = context.agenda_asesor.Where(x =>
                                x.asesor_id == asesor.user_id && x.desde.Year == fechaHoyInicio.Year &&
                                x.desde.Month == fechaHoyInicio.Month && x.desde.Day == fechaHoyInicio.Day).ToList();
                            int horasDeMasAsesor = buscarParametroHorasAsesor != null
                                ? Convert.ToInt32(buscarParametroHorasAsesor.syspar_value)
                                : 0;

                            foreach (agenda_asesor cita in buscarCitasHoyAsesor)
                            {
                                if (fechaHoyInicio >=
                                    cita.desde.AddHours(-horasDeMasAsesor) /*&& fechaHoyFin <= cita.hasta*/ &&
                                    cita.estado != "Atendida")
                                {
                                    // Significa que el asesor tiene una cita validando el tiempo libre antes y despues de cada cita
                                    citaInterfiere = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            citaInterfiere = true;
                        }
                        int usuarioId = asesor.user_id;

                        var estadoAsesor = context.sesion_logasesor.OrderByDescending(d => d.id).Where(x => x.user_id == usuarioId).FirstOrDefault();
                        if (estadoAsesor.estado == 1)
                        {

                            var busqueda = context.parametrizacion_horario_planta.Where(x => x.usuario_id == usuarioId).FirstOrDefault();

                            var busqueda1 = context.vw_Horarios_Asesores_Planta.Where(x => x.usuario_id == usuarioId).FirstOrDefault();
                            if (busqueda1 != null)
                            {
                                if (busqueda1.turno)
                                {

                                    //fecha y hora actual
                                    var tiempo = DateTime.Now;
                                    var permitido = 0;
                                    var disponible = 0;
                                    var dia = tiempo.DayOfWeek;
                                    switch (dia)
                                    {
                                        case DayOfWeek.Sunday:
                                            if (busqueda.domingo_disponible)
                                            {
                                                permitido = !string.IsNullOrWhiteSpace(busqueda1.domingo_total) ? 1 : 0;
                                            }
                                            else
                                            {
                                                permitido = 0;
                                            }
                                            break;
                                        case DayOfWeek.Monday:
                                            if (busqueda.lunes_disponible)
                                            {
                                                permitido = !string.IsNullOrWhiteSpace(busqueda1.lunes_total) ? 1 : 0;
                                            }
                                            else
                                            {
                                                permitido = 0;
                                            }
                                            break;
                                        case DayOfWeek.Tuesday:
                                            if (busqueda.martes_disponible)
                                            {
                                                permitido = !string.IsNullOrWhiteSpace(busqueda1.martes_total) ? 1 : 0;
                                            }
                                            else
                                            {
                                                permitido = 0;
                                            }
                                            break;
                                        case DayOfWeek.Wednesday:
                                            if (busqueda.miercoles_disponible)
                                            {
                                                permitido = !string.IsNullOrWhiteSpace(busqueda1.miercoles_total) ? 1 : 0;
                                            }
                                            else
                                            {
                                                permitido = 0;
                                            }
                                            break;
                                        case DayOfWeek.Thursday:
                                            if (busqueda.jueves_disponible)
                                            {
                                                permitido = !string.IsNullOrWhiteSpace(busqueda1.jueves_total) ? 1 : 0;
                                            }
                                            else
                                            {
                                                permitido = 0;
                                            }
                                            break;
                                        case DayOfWeek.Friday:
                                            if (busqueda.viernes_disponible)
                                            {
                                                permitido = !string.IsNullOrWhiteSpace(busqueda1.viernes_total) ? 1 : 0;
                                            }
                                            else
                                            {
                                                permitido = 0;
                                            }
                                            break;
                                        case DayOfWeek.Saturday:
                                            if (busqueda.sabado_disponible)
                                            {
                                                permitido = !string.IsNullOrWhiteSpace(busqueda1.sabado_total) ? 1 : 0;
                                            }
                                            else
                                            {
                                                permitido = 0;
                                            }
                                            break;
                                        default:
                                            permitido = 0;
                                            break;
                                    }
                                    if (permitido == 1)
                                    {
                                        disponible = buscarDisponibilidadPlanta(dia, tiempo, usuarioId, id_tramite.Value);
                                        if (disponible == 1)
                                        {

                                            users buscarAsesor = context.users.FirstOrDefault(x => x.user_id == usuarioId);
                                            listaAsesores.Add(new SelectListItem
                                            {
                                                Value = buscarAsesor.user_id.ToString(),
                                                Text = buscarAsesor.user_nombre + " " + buscarAsesor.user_apellido
                                            });
                                        }
                                    }
                                }
                            }

                        }
                        if (!citaInterfiere)
                        {
                            if (asesor.ultimaConexion.fecha_inicia.Year == DateTime.Now.Year && asesor.ultimaConexion
                                                                                                 .fecha_inicia.Month ==
                                                                                             DateTime.Now.Month
                                                                                             && asesor.ultimaConexion
                                                                                                 .fecha_inicia.Day ==
                                                                                             DateTime.Now.Day &&
                                                                                             asesor.ultimaConexion
                                                                                                 .estado == 1)
                            {
                                //validar si asesor tiene planta disponble en el momento

                                // Se valida si esta de planta en la fecha actual para asignarlo de ultimo
                                if (DateTime.Now >= asesor.fechainiplanta && DateTime.Now <= asesor.fechafinplanta)
                                {
                                    // Significa que el asesor esta de planta en la fecha actual
                                    listaAsesoresPlanta.Add(new SelectListItem
                                    {
                                        Value = asesor.user_id.ToString(),
                                        Text = asesor.user_nombre + " " + asesor.user_apellido
                                    });
                                }
                                else
                                {
                                    listaAsesores.Add(new SelectListItem
                                    {
                                        Value = asesor.user_id.ToString(),
                                        Text = asesor.user_nombre + " " + asesor.user_apellido
                                    });
                                }
                            }
                        }
                    }
                }

                foreach (SelectListItem asesorPlanta in listaAsesoresPlanta)
                {
                    listaAsesores.Add(new SelectListItem
                    {
                        Value = asesorPlanta.Value,
                        Text = asesorPlanta.Text
                    });
                }
            }

            return Json(listaAsesores, JsonRequestBehavior.AllowGet);
        }
        public JsonResult BusquedaAsesores(int? id_tramite)
        {

            List<SelectListItem> listaAsesores = new List<SelectListItem>();
            List<SelectListItem> listaAsesoresPlanta = new List<SelectListItem>();
            int bodegaId = Convert.ToInt32(Session["user_bodega"]);

            string parametroAsesor = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P88").syspar_value;
            string parametroAsesorU = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P99").syspar_value;

            if (Session["user_rolid"].ToString() == parametroAsesor ||
                Session["user_rolid"].ToString() == parametroAsesorU)
            {
                int usuarioId = Convert.ToInt32(Session["user_usuarioid"]);


                users buscarAsesor = context.users.FirstOrDefault(x => x.user_id == usuarioId);
                listaAsesores.Add(new SelectListItem
                {
                    Value = buscarAsesor.user_id.ToString(),
                    Text = buscarAsesor.user_nombre + " " + buscarAsesor.user_apellido
                });

            }
            else
            {
                DateTime fechaHoy = DateTime.Now;
                var buscaAsesores = (from usuarios in context.users
                                     join tipoTramite in context.tipotramiteasesor
                                         on usuarios.user_id equals tipoTramite.id_usuario
                                     where tipoTramite.id_tipotramite == id_tramite
                                     //where usuarios.rol_id == 4
                                     select new
                                     {
                                         ultimaConexion = context.sesion_logasesor.OrderByDescending(x => x.fecha_inicia).Where(x =>
                                             x.user_id == usuarios.user_id /* && x.bodega == bodegaId*/
                                             && x.fecha_inicia.Year == fechaHoy.Year && x.fecha_inicia.Month == fechaHoy.Month &&
                                             x.fecha_inicia.Day == fechaHoy.Day).FirstOrDefault(),
                                         usuarios.sesion,
                                         usuarios.user_id,
                                         usuarios.rol_id,
                                         usuarios.user_nombre,
                                         usuarios.user_apellido,
                                         usuarios.fechainiplanta,
                                         usuarios.fechafinplanta
                                     }).OrderBy(x => x.ultimaConexion.fecha_inicia).ToList();

                buscaAsesores = buscaAsesores.Where(d => d.ultimaConexion != null).OrderByDescending(d => d.ultimaConexion.fecha_inicia).ToList();

                // P31 es el codigo del parametro de las horas en las que un asesor debe estar libre antes de asignarle algun cliente nuevo por anfitriona
                icb_sysparameter buscarParametroHorasAsesor = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P31");
                //var parametroAsesor = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P88").syspar_value;
                foreach (var asesor in buscaAsesores)
                {
                    if (asesor.ultimaConexion != null)
                    {
                        DateTime fechaHoyInicio = DateTime.Now;
                        DateTime fechaHoyFin = DateTime.Now;
                        bool citaInterfiere = false;
                        // 4 es el id del rol asesor en base de datos, tabla rols
                        if (asesor.rol_id == Convert.ToInt32(parametroAsesor) &&
                            asesor.ultimaConexion.bodega == bodegaId)
                        //if (asesor.rol_id == 4 && asesor.ultimaConexion.bodega == bodegaId)
                        {
                            List<agenda_asesor> buscarCitasHoyAsesor = context.agenda_asesor.Where(x =>
                                x.asesor_id == asesor.user_id && x.desde.Year == fechaHoyInicio.Year &&
                                x.desde.Month == fechaHoyInicio.Month && x.desde.Day == fechaHoyInicio.Day).ToList();
                            int horasDeMasAsesor = buscarParametroHorasAsesor != null
                                ? Convert.ToInt32(buscarParametroHorasAsesor.syspar_value)
                                : 0;

                            foreach (agenda_asesor cita in buscarCitasHoyAsesor)
                            {
                                if (fechaHoyInicio >=
                                    cita.desde.AddHours(-horasDeMasAsesor) /*&& fechaHoyFin <= cita.hasta*/ &&
                                    cita.estado != "Atendida")
                                {
                                    // Significa que el asesor tiene una cita validando el tiempo libre antes y despues de cada cita
                                    citaInterfiere = true;
                                    break;
                                }
                            }
                        }
                        //else if (asesor.rol_id == 2030 && asesor.ultimaConexion.bodega == bodegaId)
                        else if (asesor.rol_id == Convert.ToInt32(parametroAsesorU) &&
                                 asesor.ultimaConexion.bodega == bodegaId)
                        {
                            List<agenda_asesor> buscarCitasHoyAsesor = context.agenda_asesor.Where(x =>
                                x.asesor_id == asesor.user_id && x.desde.Year == fechaHoyInicio.Year &&
                                x.desde.Month == fechaHoyInicio.Month && x.desde.Day == fechaHoyInicio.Day).ToList();
                            int horasDeMasAsesor = buscarParametroHorasAsesor != null
                                ? Convert.ToInt32(buscarParametroHorasAsesor.syspar_value)
                                : 0;

                            foreach (agenda_asesor cita in buscarCitasHoyAsesor)
                            {
                                if (fechaHoyInicio >=
                                    cita.desde.AddHours(-horasDeMasAsesor) /*&& fechaHoyFin <= cita.hasta*/ &&
                                    cita.estado != "Atendida")
                                {
                                    // Significa que el asesor tiene una cita validando el tiempo libre antes y despues de cada cita
                                    citaInterfiere = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            citaInterfiere = true;
                        }
                        int usuarioId = asesor.user_id;

                        var estadoAsesor = context.sesion_logasesor.OrderByDescending(d => d.id).Where(x => x.user_id == usuarioId).FirstOrDefault();
                        if (estadoAsesor.estado == 2)
                        {

                            var busqueda = context.parametrizacion_horario_planta.Where(x => x.usuario_id == usuarioId).FirstOrDefault();

                            var busqueda1 = context.vw_Horarios_Asesores_Planta.Where(x => x.usuario_id == usuarioId).FirstOrDefault();
                            if (busqueda1 != null)
                            {
                                if (busqueda1.turno)
                                {

                                    //fecha y hora actual
                                    var tiempo = DateTime.Now;
                                    var permitido = 0;
                                    var disponible = 0;
                                    var dia = tiempo.DayOfWeek;
                                    switch (dia)
                                    {
                                        case DayOfWeek.Sunday:
                                            if (busqueda.domingo_disponible)
                                            {
                                                permitido = !string.IsNullOrWhiteSpace(busqueda1.domingo_total) ? 1 : 0;
                                            }
                                            else
                                            {
                                                permitido = 0;
                                            }
                                            break;
                                        case DayOfWeek.Monday:
                                            if (busqueda.lunes_disponible)
                                            {
                                                permitido = !string.IsNullOrWhiteSpace(busqueda1.lunes_total) ? 1 : 0;
                                            }
                                            else
                                            {
                                                permitido = 0;
                                            }
                                            break;
                                        case DayOfWeek.Tuesday:
                                            if (busqueda.martes_disponible)
                                            {
                                                permitido = !string.IsNullOrWhiteSpace(busqueda1.martes_total) ? 1 : 0;
                                            }
                                            else
                                            {
                                                permitido = 0;
                                            }
                                            break;
                                        case DayOfWeek.Wednesday:
                                            if (busqueda.miercoles_disponible)
                                            {
                                                permitido = !string.IsNullOrWhiteSpace(busqueda1.miercoles_total) ? 1 : 0;
                                            }
                                            else
                                            {
                                                permitido = 0;
                                            }
                                            break;
                                        case DayOfWeek.Thursday:
                                            if (busqueda.jueves_disponible)
                                            {
                                                permitido = !string.IsNullOrWhiteSpace(busqueda1.jueves_total) ? 1 : 0;
                                            }
                                            else
                                            {
                                                permitido = 0;
                                            }
                                            break;
                                        case DayOfWeek.Friday:
                                            if (busqueda.viernes_disponible)
                                            {
                                                permitido = !string.IsNullOrWhiteSpace(busqueda1.viernes_total) ? 1 : 0;
                                            }
                                            else
                                            {
                                                permitido = 0;
                                            }
                                            break;
                                        case DayOfWeek.Saturday:
                                            if (busqueda.sabado_disponible)
                                            {
                                                permitido = !string.IsNullOrWhiteSpace(busqueda1.sabado_total) ? 1 : 0;
                                            }
                                            else
                                            {
                                                permitido = 0;
                                            }
                                            break;
                                        default:
                                            permitido = 0;
                                            break;
                                    }
                                    if (permitido == 1)
                                    {
                                        disponible = buscarDisponibilidadPlanta(dia, tiempo, usuarioId, id_tramite.Value);
                                        if (disponible == 1)
                                        {

                                            users buscarAsesor = context.users.FirstOrDefault(x => x.user_id == usuarioId);
                                            listaAsesores.Add(new SelectListItem
                                            {
                                                Value = buscarAsesor.user_id.ToString(),
                                                Text = buscarAsesor.user_nombre + " " + buscarAsesor.user_apellido
                                            });
                                        }
                                    }
                                }
                            }

                        }
                        if (!citaInterfiere)
                        {
                            if (asesor.ultimaConexion.fecha_inicia.Year == DateTime.Now.Year && asesor.ultimaConexion
                                                                                                 .fecha_inicia.Month ==
                                                                                             DateTime.Now.Month
                                                                                             && asesor.ultimaConexion
                                                                                                 .fecha_inicia.Day ==
                                                                                             DateTime.Now.Day &&
                                                                                             asesor.ultimaConexion
                                                                                                 .estado == 1)
                            {
                                // Se valida si esta de planta en la fecha actual para asignarlo de ultimo
                                if (DateTime.Now >= asesor.fechainiplanta && DateTime.Now <= asesor.fechafinplanta)
                                {
                                    // Significa que el asesor esta de planta en la fecha actual
                                    listaAsesoresPlanta.Add(new SelectListItem
                                    {
                                        Value = asesor.user_id.ToString(),
                                        Text = asesor.user_nombre + " " + asesor.user_apellido
                                    });
                                }
                                else
                                {
                                    listaAsesores.Add(new SelectListItem
                                    {
                                        Value = asesor.user_id.ToString(),
                                        Text = asesor.user_nombre + " " + asesor.user_apellido
                                    });
                                }
                            }
                        }
                    }
                }

                foreach (SelectListItem asesorPlanta in listaAsesoresPlanta)
                {
                    listaAsesores.Add(new SelectListItem
                    {
                        Value = asesorPlanta.Value,
                        Text = asesorPlanta.Text
                    });
                }
            }

            return Json(listaAsesores, JsonRequestBehavior.AllowGet);
        }
        public int buscarDisponibilidadPlanta(DayOfWeek dia, DateTime tiempo, int usuarioId, int id_tramite)
        {

            int asesorId = usuarioId;
            var resultado = 0;
            var busqueda2 = context.parametrizacion_horario_planta.Where(x => x.usuario_id == asesorId).FirstOrDefault();
            var informacion = tiempo.TimeOfDay;

            if (busqueda2 != null)
            {
                switch (dia)
                {
                    case DayOfWeek.Sunday:
                        if ((busqueda2.domingo_desde <= informacion && busqueda2.domingo_hasta >= informacion) ||
                            (busqueda2.domingo_desde2 <= informacion && busqueda2.domingo_hasta2 >= informacion))
                        {
                            resultado = 1;

                        }
                        else
                        {
                            resultado = 0;

                        }
                        break;
                    case DayOfWeek.Monday:
                        if ((busqueda2.lunes_desde <= informacion && busqueda2.lunes_hasta >= informacion) ||
                           (busqueda2.lunes_desde2 <= informacion && busqueda2.lunes_hasta2 >= informacion))
                        {
                            resultado = 1;

                        }
                        else
                        {
                            resultado = 0;

                        }
                        break;
                    case DayOfWeek.Tuesday:
                        if ((busqueda2.martes_desde <= informacion && busqueda2.martes_hasta >= informacion) ||
                           (busqueda2.martes_desde2 <= informacion && busqueda2.martes_hasta2 >= informacion))
                        {
                            resultado = 1;

                        }
                        else
                        {
                            resultado = 0;

                        }
                        break;
                    case DayOfWeek.Wednesday:
                        if ((busqueda2.miercoles_desde <= informacion && busqueda2.miercoles_hasta >= informacion) ||
                           (busqueda2.miercoles_desde2 <= informacion && busqueda2.miercoles_hasta2 >= informacion))
                        {
                            resultado = 1;

                        }
                        else
                        {
                            resultado = 0;

                        }
                        break;
                    case DayOfWeek.Thursday:
                        if ((busqueda2.jueves_desde <= informacion && busqueda2.jueves_hasta >= informacion) ||
                           (busqueda2.jueves_desde2 <= informacion && busqueda2.jueves_hasta2 >= informacion))
                        {
                            resultado = 1;

                        }
                        else
                        {
                            resultado = 0;

                        }
                        break;
                    case DayOfWeek.Friday:
                        if ((busqueda2.viernes_desde <= informacion && busqueda2.viernes_hasta >= informacion) ||
                           (busqueda2.viernes_desde2 <= informacion && busqueda2.viernes_hasta2 >= informacion))
                        {
                            resultado = 1;

                        }
                        else
                        {
                            resultado = 0;

                        }
                        break;
                    case DayOfWeek.Saturday:
                        if ((busqueda2.sabado_desde <= informacion && busqueda2.sabado_hasta >= informacion) ||
                           (busqueda2.sabado_desde2 <= informacion && busqueda2.sabado_hasta2 >= informacion))
                        {
                            resultado = 1;

                        }
                        else
                        {
                            resultado = 0;

                        }
                        break;
                    default:
                        resultado = 0;

                        break;
                }
            }




            return resultado;
        }
        public JsonResult BuscarProspectosPaginados()

        {
            int asesorLog = Convert.ToInt32(Session["user_usuarioid"]);

            #region Si el que esta consultando es un asesor

            int rol = Convert.ToInt32(Session["user_rolid"]);

            string parametroAsesor = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P88").syspar_value;
            string parametroAsesorU = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P99").syspar_value;
            //inicializo el predicado
            var predicado = PredicateBuilder.True<vw_prospectos_activos>();
            if (rol == Convert.ToInt32(parametroAsesor) || rol == Convert.ToInt32(parametroAsesorU))
            {

                predicado = predicado.And(d => d.asesor_id == asesorLog);
            }
            predicado = predicado.And(d => d.estado == 1);
            //predicado = predicado.And(d => d.celular_tercero == 1);

            // predicado = predicado.And(d => d.celular_tercero == null &&  d.celular_tercero != null);
            // predicado = predicado.And(d => d.celular_tercero == null);
            //predicado = predicado.And(d => d.celular_tercero != null);

            var buscarTerceros = context.vw_prospectos_activos.Where(predicado).ToList();
            var dataProspectos = buscarTerceros.Select(x => new
            {
                id = x.id,
                //tercero = x.tercero_id,
                x.idtercero,
                documento = x.doc_tercero != null ? x.doc_tercero : "",
                nombreCompleto = x.razon_social + x.prinom_tercero + " " + x.segnom_tercero +
                                                           " " + x.apellido_tercero + " " + x.segapellido_tercero,
                telefono = x.telf_tercero != null ? x.telf_tercero : "",
                //celular = x.celular_tercero != null ? x.celular_tercero : "",
                celular = !string.IsNullOrWhiteSpace(x.celular_tercero) ? x.celular_tercero : "",
                correo = x.email_tercero != null ? x.email_tercero : "",
                fuente = x.tporigen_nombre,
                subfuente = x.fuente,
                x.medcomun_descripcion,
                x.tptrapros_descripcion,
                x.tercerofec_creacion,
                fecha = x.tercerofec_creacion.ToString("yyyy/MM/dd HH:mm"),
                asesor = x.user_nombre + ' ' + x.user_apellido,
                desTipoUltimoSeg = buscarTipoSeguimiento(x.idtercero),
                notaultimoSeg = notaSeguimiento(x.idtercero),
                nombreAsesor = buscarAsesorSeguimiento(x.idtercero),
            }).OrderByDescending(x => x.tercerofec_creacion).ToList();


            return Json(dataProspectos, JsonRequestBehavior.AllowGet);




            #endregion
        }
        public ActionResult DescargarExcel(string filtroGeneral)
        {



            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

            int idusuario = Convert.ToInt32(Session["user_usuarioid"]);
            //busco el usuario
            users usuario = context.users.Where(d => d.user_id == idusuario).FirstOrDefault();
            int rol = Convert.ToInt32(Session["user_rolid"]);

            //por si necesito el rol del usuario

            icb_sysparameter admin1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P109").FirstOrDefault();
            int admin = admin1 != null ? Convert.ToInt32(admin1.syspar_value) : 1;

            Expression<Func<vw_prospectos_activos, bool>> predicado = PredicateBuilder.True<vw_prospectos_activos>();
            Expression<Func<vw_prospectos_activos, bool>> predicado2 = PredicateBuilder.False<vw_prospectos_activos>();
            Expression<Func<vw_prospectos_activos, bool>> predicado3 = PredicateBuilder.False<vw_prospectos_activos>();



            if (!string.IsNullOrEmpty(filtroGeneral))
            {
                predicado2 = predicado2.Or(d => 1 == 1 && d.doc_tercero.ToString().Contains(filtroGeneral));
                predicado2 = predicado2.Or(d => 1 == 1 && d.nombre_completo.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado2 = predicado2.Or(d => 1 == 1 && d.tporigen_nombre.Contains(filtroGeneral.ToUpper()));
                predicado2 = predicado2.Or(d => 1 == 1 && d.subfuente.Contains(filtroGeneral.ToUpper()));
                predicado2 = predicado2.Or(d => 1 == 1 && d.medcomun_descripcion.Contains(filtroGeneral.ToUpper()));
                predicado2 = predicado2.Or(d => 1 == 1 && d.tptrapros_descripcion.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado2 = predicado2.Or(d => 1 == 1 && d.fecha2.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado2 = predicado2.Or(d => 1 == 1 && d.nombreasesorcompleto.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado = predicado.And(predicado2);
            }
            //
            List<vw_prospectos_activos> query2 = context.vw_prospectos_activos.Where(predicado).ToList();
            var query = query2.Select(d => new
            {
                d.id,
                d.doc_tercero,
                d.nombre_completo,
                d.tporigen_nombre,
                d.subfuente,
                d.medcomun_descripcion,
                d.tptrapros_descripcion,
                d.fecha2,
                d.nombreasesorcompleto,
            }).ToList();
            ////

            var queryarreglo = query.Select(d => new
            {
                //  d.id,
                d.doc_tercero,
                d.nombre_completo,
                d.tporigen_nombre,
                d.subfuente,
                d.medcomun_descripcion,
                d.tptrapros_descripcion,
                d.fecha2,
                d.nombreasesorcompleto,

            }).ToArray();

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.Cells[1, 1].Value = "Informe Prospectos Activos";

            workSheet.Cells[3, 1].Value = "Documento";
            workSheet.Cells[3, 2].Value = "Nombre Completo";
            workSheet.Cells[3, 3].Value = "Fuente";
            workSheet.Cells[3, 4].Value = "SubFuente";
            workSheet.Cells[3, 5].Value = "Medio de Comunicacion";
            workSheet.Cells[3, 6].Value = "Tramite";
            workSheet.Cells[3, 7].Value = "Fecha";
            workSheet.Cells[3, 8].Value = "Nombre de Asesor";



            workSheet.Cells[4, 1].LoadFromCollection(queryarreglo, false);

            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;  filename=gestionot.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }

            return View();
        }

        public JsonResult BuscarJson(string filtrogeneral)
        {
            if (Session["user_usuarioid"] != null)
            {
                string draw = Request.Form.GetValues("draw").FirstOrDefault();
                string start = Request.Form.GetValues("start").FirstOrDefault();
                string length = Request.Form.GetValues("length").FirstOrDefault();
                string search = Request.Form.GetValues("search[value]").FirstOrDefault();
                //esto me sirve para reiniciar la consulta cuando ordeno las columnas de menor a mayor y que no me vuelva a recalcular todo
                //ES IMPORTANTE QUE LA COLUMNA EN EL DATATABLE TENGA EL NOMBRE DE LA TABLA O VISTA A CONSULTAR, porque vamos a usarla para ordenar.
                string sortColumn = Request.Form
                    .GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]")
                    .FirstOrDefault();
                string sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                search = search.Replace(" ", "");
                int pagina = Convert.ToInt32(start);
                int pageSize = Convert.ToInt32(length);

                int skip = 0;
                if (pagina == 0)
                {
                    skip = 0;
                }
                else
                {
                    skip = pagina;
                }

                CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

                int idusuario = Convert.ToInt32(Session["user_usuarioid"]);
                //busco el usuario
                users usuario = context.users.Where(d => d.user_id == idusuario).FirstOrDefault();
                int rol = Convert.ToInt32(Session["user_rolid"]);

                //por si necesito el rol del usuario

                icb_sysparameter admin1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P109").FirstOrDefault();
                int admin = admin1 != null ? Convert.ToInt32(admin1.syspar_value) : 1;

                Expression<Func<vw_prospectos_activos, bool>> predicado = PredicateBuilder.True<vw_prospectos_activos>();
                Expression<Func<vw_prospectos_activos, bool>> predicado2 = PredicateBuilder.False<vw_prospectos_activos>();
                Expression<Func<vw_prospectos_activos, bool>> predicado3 = PredicateBuilder.False<vw_prospectos_activos>();

                if (!string.IsNullOrWhiteSpace(filtrogeneral))
                {
                    predicado2 = predicado2.Or(d => 1 == 1 && d.doc_tercero.ToString().Contains(filtrogeneral));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.nombre_completo.ToUpper().Contains(filtrogeneral.ToUpper()));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.tporigen_nombre.Contains(filtrogeneral.ToUpper()));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.subfuente.Contains(filtrogeneral.ToUpper()));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.medcomun_descripcion.Contains(filtrogeneral.ToUpper()));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.tptrapros_descripcion.ToUpper().Contains(filtrogeneral.ToUpper()));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.fecha2.ToUpper().Contains(filtrogeneral.ToUpper()));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.nombreasesorcompleto.ToUpper().Contains(filtrogeneral.ToUpper()));
                    predicado = predicado.And(predicado2);
                }

                int registrostotales = context.vw_prospectos_activos.Where(predicado).Count();
                //si el ordenamiento es ascendente o descendente es distinto
                if (pageSize == -1)
                {
                    pageSize = registrostotales;
                }

                if (sortColumnDir == "asc")
                {
                    List<vw_prospectos_activos> query2 = context.vw_prospectos_activos.Where(predicado)
                        .OrderBy(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
                    var query = query2.Select(d => new
                    {
                        d.id,
                        d.doc_tercero,
                        d.nombre_completo,
                        d.tporigen_nombre,
                        d.subfuente,
                        d.medcomun_descripcion,
                        d.tptrapros_descripcion,
                        d.fecha2,
                        d.nombreasesorcompleto,
                    }).ToList();

                    int contador = query.Count();
                    return Json(
                        new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                        JsonRequestBehavior.AllowGet);
                }
                else
                {
                    List<vw_prospectos_activos> query2 = context.vw_prospectos_activos.Where(predicado)
                        .OrderBy(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
                    var query = query2.Select(d => new
                    {
                        d.id,
                        d.doc_tercero,
                        d.nombre_completo,
                        d.tporigen_nombre,
                        d.subfuente,
                        d.medcomun_descripcion,
                        d.tptrapros_descripcion,
                        d.fecha2,
                        d.nombreasesorcompleto,
                    }).ToList();

                    int contador = query.Count();
                    return Json(
                        new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                        JsonRequestBehavior.AllowGet);
                }
            }

            return Json(0);
        }

        public string buscarAsesorSeguimiento(int? tercero)
        {
            var resultado = "";
            if (tercero != null)
            {
                //busco el ultimo seguimiento
                var seugi = context.seguimientotercero.Where(d => d.idtercero == tercero).FirstOrDefault();
                if (seugi != null)
                {
                    resultado = seugi.userid_creacion != null ? seugi.users.user_nombre + " " + seugi.users.user_apellido : "";
                }
            }
            return resultado;
        }

        public string buscarTipoSeguimiento(int? tercero)
        {
            var resultado = "";
            if (tercero != null)
            {
                //busco el ultimo seguimiento
                var seugi = context.seguimientotercero.Where(d => d.idtercero == tercero).FirstOrDefault();
                if (seugi != null)
                {
                    resultado = seugi.Seguimientos.Evento;
                }
            }
            return resultado;
        }
        public string notaSeguimiento(int? tercero)
        {
            var resultado = "";
            if (tercero != null)
            {
                //busco el ultimo seguimiento
                var seugi = context.seguimientotercero.Where(d => d.idtercero == tercero).FirstOrDefault();
                if (seugi != null)
                {
                    resultado = seugi.nota;
                }
            }
            return resultado;
        }

        public JsonResult BuscarProspectosPaginadosSala(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            if (fechaDesde == null)
            {
                fechaDesde = DateTime.Now;
            }

            if (fechaHasta == null)
            {
                fechaHasta = DateTime.Now.AddDays(1);
            }

            if (fechaDesde != null && fechaHasta != null && fechaDesde <= fechaHasta)
            {
                DateTime fecha2 = fechaHasta.Value.AddDays(1);

                #region buscarTerceros

                DateTime fecha = DateTime.Now.Date;

                var buscarTerceros = (from tercero in context.vw_creacion_prospectos
                                      where tercero.fec_creacion >= fechaDesde && tercero.fec_creacion <= fecha2 &&
                                            tercero.estado != null && tercero.bodega == bodegaActual
                                      select new
                                      {
                                          tercero.id,
                                          tercero.fec_creacion,
                                          tercero.documento,
                                          tercero.razonsocial,
                                          tercero.prinom_tercero,
                                          tercero.segnom_tercero,
                                          tercero.apellido_tercero,
                                          tercero.segapellido_tercero,
                                          tercero.fuente,
                                          medio = tercero.medcomun_descripcion,
                                          tramite = tercero.tptrapros_descripcion,
                                          tercero.nombreAsesor,
                                          tercero.apellidoAsesor,
                                          tercero.detalle,
                                          tercero.estado,
                                          asesorAsignado = tercero.asesor_asignado,
                                          fechaTermina = tercero.fecha_termina
                                      }).OrderByDescending(x => x.fec_creacion).ToList();

                List<int> estados = new List<int> { 1, 2 };
                var dataProspectos = buscarTerceros.Select(x => new
                {
                    x.id,
                    documento = x.documento != null ? x.documento : "",
                    nombreProspecto = x.razonsocial + x.prinom_tercero + ' ' + x.segnom_tercero + ' ' +
                                      x.apellido_tercero + ' ' + x.segapellido_tercero,
                    x.fuente,
                    x.medio,
                    x.tramite,
                    hora = x.fec_creacion?.ToString("yyyy/MM/dd HH:mm"),
                    fechaTermina = (SeleccionarEnCurso(x.asesorAsignado, x.id) == 1 && x.fechaTermina != null) ? x.fechaTermina?.ToString("yyyy/MM/dd HH:mm") : "",
                    tiempo = x.fechaTermina != null ? x.fechaTermina - x.fec_creacion : null,
                    asesor = x.nombreAsesor + ' ' + x.apellidoAsesor,
                    modelo = x.detalle != null ? x.detalle : "",
                    x.estado,
                    x.asesorAsignado
                });

                #endregion

                var buscarUltimoAtendido = (from a in context.sesion_logasesor
                                            orderby a.fecha_inicia descending
                                            select new
                                            {
                                                a.user_id,
                                                a.estado,
                                                a.prospectos
                                            }).GroupBy(x => x.user_id).ToList();

                var data = new
                {
                    dataProspectos
                };


                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public int SeleccionarEnCurso(int? asesor_asignado, int? id)
        {
            int resultado = 0;
            sesion_logasesor existe = context.sesion_logasesor.OrderByDescending(d => d.fecha_termina).Where(d => d.id == asesor_asignado && d.idprospecto == id).FirstOrDefault();
            if (existe != null)
            {
                if (existe.estado != 2)
                {
                    resultado = 1;
                }
            }
            return resultado;
        }

        public JsonResult buscarRequiereAsesor(int tipo)
        {
            int rol = Convert.ToInt32(Session["user_rolid"]);
            icb_tptramite_prospecto data = context.icb_tptramite_prospecto.FirstOrDefault(x => x.tptrapros_id == tipo);
            if (data != null)
            {
                if (data.pide_asesor)
                {
                    return Json(new { requiere = true, rol }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { requiere = false, rol }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { requiere = false, rol }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarAsesoresTramite(int? id_tramite)
        {
            int log = Convert.ToInt32(Session["user_usuarioid"]);
            var data = (from u in context.users
                        join r in context.rols
                            on u.rol_id equals r.rol_id
                        join tipoTramite in context.tipotramiteasesor
                            on u.user_id equals tipoTramite.id_usuario
                        where tipoTramite.id_tipotramite == id_tramite && u.user_estado && u.user_id == log
                        select new
                        {
                            Value = u.user_id,
                            Text = u.user_nombre + " " + u.user_apellido
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        ////Post
        //[HttpPost]
        //public ActionResult icb_tercero2Create(icb_terceros2 terceros2)
        //{
        //    //listas de tablas relacionadas enviadas a la vista por ViewBag 
        //    if (ModelState.IsValid)
        //    {
        //        terceros2.tercerofec_creacion = DateTime.Now;
        //        terceros2.tercerouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
        //        bd.icb_terceros2.Add(terceros2);
        //        bd.SaveChanges();
        //        TempData["mensaje"] = "El registro del nuevo prospecto fue exitoso!";
        //        ViewBag.medcomun_id = new SelectList(bd.icb_medio_comunicacion, "medcomun_id", "medcomun_descripcion");
        //        return View(terceros2);
        //    }
        //    ViewBag.medcomun_id = new SelectList(bd.icb_medio_comunicacion, "medcomun_id", "medcomun_descripcion");
        //    return View();
        //}

        //[HttpPost]
        //public ActionResult icb_tercero2Create(ProspectoMV terceros2)
        //{
        //    //listas de tablas relacionadas enviadas a la vista por ViewBag     
        //    if (ModelState.IsValid)
        //    {
        //        context.icb_terceros2.Add(new icb_terceros2()
        //        {
        //            prinom_tercero = terceros2.prinom_tercero,
        //            segnom_tercero = terceros2.segnom_tercero,
        //            apellido_tercero = terceros2.apellido_tercero,
        //            segapellido_tercero = terceros2.segapellido_tercero,
        //            email_tercero = terceros2.email_tercero,
        //            medcomun_id = terceros2.medcomun_id,
        //            genero_tercero = terceros2.genero_tercero,
        //            tercerofec_creacion = DateTime.Now,
        //            tercerouserid_creacion = Convert.ToInt32(Session["user_usuarioid"])
        //        });
        //        context.SaveChanges();
        //        var tercero = context.icb_terceros2.OrderByDescending(x => x.tercero_id).First();
        //        context.icb_hist_prospecto.Add(new icb_hist_prospecto()
        //        {
        //            ter_id = tercero.tercero_id,
        //            hist_prospecto_nrofamilia = terceros2.hist_prospecto_nrofamilia,
        //            hist_prospecto_nroacompanante = terceros2.hist_prospecto_nroacompanante,
        //            modvh_id = terceros2.modvh_id,
        //            tramite_id = terceros2.tptrapros_id,
        //            medio_id = terceros2.medcomun_id,
        //            hist_prospecto_creacion = DateTime.Now,
        //            hist_prospecto_usucreacion = Convert.ToInt32(Session["user_usuarioid"]),
        //        });
        //        //terceros2.tercerofec_creacion = DateTime.Now;
        //        //terceros2.tercerouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
        //        context.SaveChanges();
        //        TempData["mensaje"] = "El registro del nuevo prospecto fue exitoso!";
        //        ViewBag.medcomun_id = new SelectList(context.icb_medio_comunicacion, "medcomun_id", "medcomun_descripcion");
        //        ViewBag.modvh_id = new SelectList(context.modelo_vehiculo, "modvh_id", "modvh_nombre");
        //        ViewBag.Categories = context.icb_tptramite_prospecto.ToList();
        //        return View(terceros2);
        //    }
        //    ViewBag.medcomun_id = new SelectList(context.icb_medio_comunicacion, "medcomun_id", "medcomun_descripcion");
        //    ViewBag.modvh_id = new SelectList(context.modelo_vehiculo, "modvh_id", "modvh_nombre");
        //    ViewBag.tptrapros_id = new SelectList(context.icb_tptramite_prospecto, "tptrapros_id", "tptrapros_descripcion");
        //    ViewBag.Categories = context.icb_tptramite_prospecto.ToList();
        //    return View();
        //}

        [HttpGet]
        public ActionResult icb_tercero2Update(int? id)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            icb_terceros terceros = context.icb_terceros.Find(id);
            icb_hist_prospecto prospecto = context.icb_hist_prospecto.FirstOrDefault(x => x.ter_id == id);

            //ProspectoMV editar = new ProspectoMV()
            //{
            //    prinom_tercero = terceros.prinom_tercero,
            //    segnom_tercero = terceros.segnom_tercero,
            //    apellido_tercero = terceros.apellido_tercero,
            //    segapellido_tercero = terceros.segapellido_tercero,
            //    email_tercero = terceros.email_tercero,
            //    medcomun_id = terceros.medcomun_id,
            //    genero_tercero = terceros.genero_tercero,
            //    tptrapros_id = prospecto.tramite_id,
            //    //---------------------------------------------
            //    ter_id = terceros.tercero_id,
            //    hist_prospecto_nrofamilia = Convert.ToInt32(prospecto.hist_prospecto_nrofamilia),
            //    hist_prospecto_nroacompanante = Convert.ToInt32(prospecto.hist_prospecto_nroacompanante),
            //    modvh_id = prospecto.modvh_id,
            //};

            if (terceros == null)
            {
                return HttpNotFound();
            }

            ViewBag.medcomun_id = new SelectList(context.icb_medio_comunicacion, "medcomun_id", "medcomun_descripcion",
            terceros.medcomun_id);
            ViewBag.modvh_id = new SelectList(context.modelo_vehiculo, "modvh_id", "modvh_nombre", prospecto.modvh_id);
            ViewBag.tptrapros_id =
                new SelectList(context.icb_tptramite_prospecto, "tptrapros_id", "tptrapros_descripcion");
            ViewBag.Categories = context.icb_tptramite_prospecto.ToList();
            return View( /*editar*/);
        }

        //---------------------------------------------------------------------------------//

        public JsonResult BuscarTercerosCant(string texto, int pagina)
        {
            int tercerosTotal = (from tercero in context.icb_terceros
                                 join prospecto in context.icb_hist_prospecto on tercero.tercero_id equals prospecto.ter_id
                                 join tramite in context.icb_tptramite_prospecto on prospecto.tramite_id equals tramite.tptrapros_id
                                 where tercero.doc_tercero.Contains(texto)
                                       || tercero.prinom_tercero.Contains(texto) ||
                                       tercero.tercerofec_creacion.ToString().Contains(texto)
                                       || tramite.tptrapros_descripcion.Contains(texto)
                                 select tercero.prinom_tercero).ToList().Count;


            var resultAux = (from tercero in context.icb_terceros
                             join prospecto in context.icb_hist_prospecto on tercero.tercero_id equals prospecto.ter_id
                             join tramite in context.icb_tptramite_prospecto on prospecto.tramite_id equals tramite.tptrapros_id
                             where tercero.doc_tercero.Contains(texto)
                                   || tercero.prinom_tercero.Contains(texto) ||
                                   tercero.tercerofec_creacion.ToString().Contains(texto)
                                   || tramite.tptrapros_descripcion.Contains(texto)
                             orderby tercero.prinom_tercero
                             select new
                             {
                                 terceroId = tercero.tercero_id,
                                 terceroNombre = /*tercero.prinom_tercero,*/ string.Concat("",
                                     tercero.prinom_tercero + " " + " " + tercero.segnom_tercero + " " + " " +
                                     tercero.apellido_tercero + " " + " " + tercero.segapellido_tercero),
                                 terceroFechaCreacion = tercero.tercerofec_creacion,
                                 prospectoTramite = tramite.tptrapros_descripcion
                             }).Skip(pagina * 5 - 5).Take(5).ToList();

            var result2 = resultAux.Select(x => new
            {
                x.terceroId,
                x.terceroNombre,
                //terceroFechaCreacion = x.terceroFechaCreacion != null ? x.terceroFechaCreacion.Value.ToString("dd/MM/yyyy") : null,
                x.prospectoTramite
            });

            var data = new
            {
                result = tercerosTotal,
                result2
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BrowserProspectosPendientes(int? menu)
        {
            ParametrosBusqueda();
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult BuscarTareasPendientesPaginados()
        {
            int asesorLog = Convert.ToInt32(Session["user_usuarioid"]);
            var buscarTareas = (from tareas in context.vw_tareasPendientes
                                where tareas.idusuarioasignado == asesorLog
                                select new
                                {
                                    tareas.idTarea,
                                    tareas.idTercero,
                                    tareas.idCotizacion,
                                    tareas.cot_numcotizacion,
                                    tareas.documento,
                                    tareas.docTercero,
                                    nombreCompleto = tareas.razonsocial + tareas.prinom_tercero + " " + tareas.segnom_tercero + " " +
                                                     tareas.apellido_tercero + " " + tareas.segapellido_tercero,
                                    nombreCompletoCot = tareas.rzTercero + tareas.pnTercero + " " + tareas.snTercero + " " +
                                                        tareas.paTercero + " " + tareas.saTercero,
                                    tareas.email_tercero,
                                    tareas.emailTercero,
                                    tareas.celular_tercero,
                                    tareas.celTercero,
                                    tareas.telf_tercero,
                                    tareas.telTercero,
                                    tareas.tporigen_nombre,
                                    tareas.subfuente,
                                    tareas.medcomun_descripcion,
                                    tareas.tptrapros_descripcion,
                                    tareas.fec_creacion,
                                    tareas.FecCreacionTerCot,
                                    tareas.user_nombre,
                                    tareas.user_apellido
                                    //idSeguimiento = tareas.estado
                                }).ToList();

            var dataProspectos = buscarTareas.Select(x => new
            {
                x.idTarea,
                tercero = x.idTercero,
                documento = x.documento != null ? x.documento : "",
                docTercero = x.docTercero != null ? x.docTercero : "",
                x.nombreCompleto,
                x.nombreCompletoCot,
                telefono = x.telf_tercero != null ? x.telf_tercero : "",
                telTercero = x.telTercero != null ? x.telTercero : "",
                celular = x.celular_tercero != null ? x.celular_tercero : "",
                celTercero = x.celTercero != null ? x.celTercero : "",
                correo = x.email_tercero != null ? x.email_tercero : "",
                emailTercero = x.emailTercero != null ? x.emailTercero : "",
                fuente = x.tporigen_nombre != null ? x.tporigen_nombre : "",
                subfuente = x.subfuente != null ? x.subfuente : "",
                medcomun_descripcion = x.medcomun_descripcion != null ? x.medcomun_descripcion : "",
                tptrapros_descripcion = x.tptrapros_descripcion != null ? x.tptrapros_descripcion : "",
                fecha = x.fec_creacion != null ? x.fec_creacion?.ToString("yyyy/MM/dd HH:mm") : "",
                FecCreacionTerCot =
                    x.FecCreacionTerCot != null ? x.FecCreacionTerCot?.ToString("yyyy/MM/dd HH:mm") : "",
                asesor = x.user_nombre + ' ' + x.user_apellido,
                //idSeguimiento = x.idSeguimiento,
                idCotizacion = x.idCotizacion != null ? x.idCotizacion.ToString() : "",
                cot_numcotizacion = x.cot_numcotizacion != null ? x.cot_numcotizacion.ToString() : ""
            });

            return Json(dataProspectos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult guardarGestion(int estado, string nota, int tercero)
        {
            seguimientotercero segTercero = new seguimientotercero
            {
                idtercero = tercero,
                tipo = 2,
                tipificacion = estado,
                nota = nota,
                fecha = DateTime.Now,
                userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
            };
            context.seguimientotercero.Add(segTercero);

            tareasasignadas consultaTarea = context.tareasasignadas.FirstOrDefault(x => x.estado && x.idtercero == tercero);
            if (consultaTarea != null)
            {
                consultaTarea.estado = false;
                context.Entry(consultaTarea).State = EntityState.Modified;

                icb_terceros actualizarTercero = context.icb_terceros.FirstOrDefault(x => x.tercero_id == tercero);
                if (actualizarTercero != null)
                {
                    actualizarTercero.asesor_id = consultaTarea.idusuarioasignado;
                    context.Entry(actualizarTercero).State = EntityState.Modified;
                }
            }

            int result = context.SaveChanges();

            if (result > 0)
            {
                return Json(new { exito = true }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { exito = false }, JsonRequestBehavior.AllowGet);
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