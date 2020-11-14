﻿using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.Models.CotizacionModel;
using Homer_MVC.ViewModels.medios;
using Newtonsoft.Json.Linq;
using Rotativa;
using Rotativa.Options;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class cotizacionController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        private readonly CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
        private readonly CultureInfo inter = CultureInfo.CreateSpecificCulture("is-IS");

        private static Expression<Func<vw_cotizacion, string>> GetColumnName(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(vw_cotizacion), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<vw_cotizacion, string>> lambda = Expression.Lambda<Func<vw_cotizacion, string>>(menuProperty, menu);

            return lambda;
        }
        public void ParametrosBusqueda()
        {
            List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 104).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 104);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
        }

        public JsonResult UpdateTercero(Cotizacion ClienteDato)
        {
            var mensaje = "";
            var OK = false;

            var tercero = context.icb_terceros.Where(x => x.tercero_id == ClienteDato.tercero_id).FirstOrDefault();
            if (tercero != null)
            {
                if (tercero.tpdoc_id != 1)
                {
                    tercero.prinom_tercero = ClienteDato.prinom_tercero;
                    tercero.segnom_tercero = ClienteDato.segnom_tercero;
                    tercero.apellido_tercero = ClienteDato.apellido_tercero;
                    tercero.segapellido_tercero = ClienteDato.segapellido_tercero;
                    //tercero.razon_social = ClienteDato.razon_social;
                    tercero.telf_tercero = ClienteDato.telefono_tercero;
                    tercero.celular_tercero = ClienteDato.celular_tercero;
                    tercero.email_tercero = ClienteDato.correo;
                    context.Entry(tercero).State = EntityState.Modified;
                    var guardar = context.SaveChanges();
                    mensaje = "Se actualizo el Cliente";
                    OK = true;
                }
                else
                {
                    tercero.razon_social = ClienteDato.razon_social;
                    tercero.telf_tercero = ClienteDato.telefono_tercero;
                    tercero.celular_tercero = ClienteDato.celular_tercero;
                    tercero.email_tercero = ClienteDato.correo;
                    context.Entry(tercero).State = EntityState.Modified;
                    var guardar = context.SaveChanges();
                    mensaje = "Se actualizo la Empresa Cliente";
                    OK = true;
                };
            }
            else {
                mensaje = "Tercero no existe";
            };

            if (OK==true)
            {
                #region seguimiento
                var BuscarSeguimiento = context.Seguimientos.Where(x => x.EsManual == false && x.ModuloSeguimientos.Codigo == 6 && x.Codigo == 4).FirstOrDefault();

                vcotseguimiento seguimiento = new vcotseguimiento
                {
                    cot_id = ClienteDato.cotizacion_id,
                    fecha = DateTime.Now,
                    responsable = Convert.ToInt32(Session["user_usuarioid"]),
                    Notas = "Actualizo Tecero: " + Convert.ToString(ClienteDato.tercero_id),
                    Motivo = null,
                    fec_creacion = DateTime.Now,
                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    estado = true,
                    tipo_seguimiento = BuscarSeguimiento.Id
                };
                context.vcotseguimiento.Add(seguimiento);
                int guardarGeneral = context.SaveChanges();

                #endregion
            }
            var dato = new 
            {
                mensaje,OK
            };
            return Json(dato, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Se valida si el tipo de documento es NIT, ya que si es NIT el campo razon social se habilita y es obligatorio
        /// </summary>
        /// <return>
        ///     Devuelve true si el tipo de documento es NIT
        /// </return>
        /// <param name="tpDocumento">
        ///     id del tipo de documento.
        /// </param>
        public JsonResult BuscarSiEsNit(int? tpDocumento)
        {
            tp_documento buscarSiEsNit = context.tp_documento.FirstOrDefault(x => x.tpdoc_id == tpDocumento);
            if (buscarSiEsNit != null)
            {
                if (buscarSiEsNit.tpdoc_nombre.ToUpper().Contains("NIT"))
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscasTasaInteres(int id_plan)
        {
            double? buscar = context.icb_plan_financiero.FirstOrDefault(x => x.plan_id == id_plan).tasa_interes;
            string valor = "0";
            if (buscar != null)
            {
                valor = buscar.Value.ToString("N2",new CultureInfo("is-IS"));
            }

            return Json(valor, JsonRequestBehavior.AllowGet);
        }


        public JsonResult buscarPlacaRetoma(string placa)
        {
            var buscarPlaca = (from vehiculo in context.icb_vehiculo
                               join modelo in context.modelo_vehiculo
                                   on vehiculo.modvh_id equals modelo.modvh_codigo
                               where vehiculo.plac_vh == placa
                               select new
                               {
                                   modelo.modvh_nombre,
                                   vehiculo.anio_vh
                               }).FirstOrDefault();

            var datosPeritaje = (from encabezado in context.icb_encabezado_insp_peritaje
                                 join solicitud in context.icb_solicitud_peritaje
                                     on encabezado.encab_insper_idsolicitud equals solicitud.id_peritaje
                                 join terVhRet in context.icb_tercero_vhretoma
                                     on solicitud.id_tercero_vhretoma equals terVhRet.veh_id
                                 join cita in context.icb_cita_perito
                                     on encabezado.encab_insper_idsolicitud equals cita.solicitud_peritaje_id
                                 join modelo in context.modelo_vehiculo
                                     on terVhRet.modelo_codigo equals modelo.modvh_codigo
                                 join perito in context.users
                                     on cita.perito_id equals perito.user_id
                                 where terVhRet.placa == placa
                                 select new
                                 {
                                     encabezado.encab_insper_fecha
                                 }).OrderByDescending(x => x.encab_insper_fecha).FirstOrDefault();
            string fecha = datosPeritaje.encab_insper_fecha.ToString("yyyy/MM/dd");

            if (buscarPlaca != null)
            {
                return Json(new { encontrado = true, buscarPlaca, fecha }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { encontrado = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDocumentoExiste(string documento, int tipoDocumento)
        {
            icb_terceros buscarDocumento = (from tercero in context.icb_terceros
                                            where tercero.tpdoc_id == tipoDocumento && tercero.doc_tercero == documento
                                            select tercero).FirstOrDefault();
            if (buscarDocumento != null)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        // GET: cotizacion
        public ActionResult Create(int? id, int? menu, int? pdf, int? cotizacionn)
        {
            if (pdf == null)
            {
                ViewBag.pdf = 0;
                ViewBag.cotizacionn = 0;
            }
            else
            {
                ViewBag.pdf = pdf;
                ViewBag.cotizacionn = cotizacionn;
            }

            ViewBag.financiera_id = new SelectList(context.icb_unidad_financiera.OrderBy(x => x.financiera_nombre),
                "financiera_id", "financiera_nombre");
            if (id != null)
            {
                //var buscaTercero = context.icb_terceros.FirstOrDefault(x => x.tercero_id == id);
                var buscaTercero = (from tercero in context.icb_terceros
                                    where tercero.tercero_id == id
                                    select new
                                    {
                                        tercero.tpdoc_id,
                                        tercero.doc_tercero,
                                        tercero.prinom_tercero,
                                        tercero.segnom_tercero,
                                        tercero.apellido_tercero,
                                        tercero.segapellido_tercero,
                                        tercero.medcomun_id,
                                        tercero.email_tercero,
                                        tercero.tercero_id,
                                        tercero.habdtautor_celular,
                                        tercero.habdtautor_correo,
                                        tercero.habdtautor_msm,
                                        tercero.habdtautor_watsap,
                                        tercero.observaciones,
                                        tercero.telf_tercero,
                                        tercero.celular_tercero,
                                        tercero.origen_id,
                                        tercero.subfuente,
                                        tercero.razon_social,
                                        tercero.digito_verificacion,
                                        tercero.genero_tercero,
                                        tercero.sitioevento,
                                        direccion = (from direccion in context.terceros_direcciones
                                                     where direccion.idtercero == tercero.tercero_id
                                                     select new
                                                     {
                                                         direccion.id,
                                                         direccion.direccion,
                                                         direccion.ciudad
                                                     }).OrderByDescending(x => x.id).FirstOrDefault()
                                    }).FirstOrDefault();
                if (buscaTercero.direccion != null)
                {
                    Cotizacion cotiza = new Cotizacion
                    {
                        tpdoc_id = buscaTercero.tpdoc_id ?? 0,
                        doc_tercero = buscaTercero.doc_tercero,
                        prinom_tercero = buscaTercero.prinom_tercero,
                        segnom_tercero = buscaTercero.segnom_tercero,
                        apellido_tercero = buscaTercero.apellido_tercero,
                        segapellido_tercero = buscaTercero.segapellido_tercero,
                        direc_tercero = buscaTercero.direccion.direccion ?? "",
                        //direc_tercero = buscaTercero.direccion.direccion != null ? buscaTercero.direccion.direccion : "",
                        ciu_id = buscaTercero.direccion.ciudad,
                        medcomun_id = buscaTercero.medcomun_id ?? 0,
                        tercero_id = buscaTercero.tercero_id,
                        correo = buscaTercero.email_tercero,
                        gentercero_id = buscaTercero.genero_tercero ?? 0,
                        habdtautor_celular = buscaTercero.habdtautor_celular,
                        habdtautor_correo = buscaTercero.habdtautor_correo,
                        habdtautor_msm = buscaTercero.habdtautor_msm,
                        habdtautor_whatsapp = buscaTercero.habdtautor_watsap,
                        observacion = buscaTercero.observaciones ?? "",
                        telefono_tercero = buscaTercero.telf_tercero ?? "",
                        celular_tercero = buscaTercero.celular_tercero ?? "",
                        tporigen_id = buscaTercero.origen_id ?? 0,
                        subfuente_id = buscaTercero.subfuente ?? 0,
                        eventoOrigen = buscaTercero.sitioevento ?? 0,
                        razon_social = buscaTercero.razon_social ?? "",
                        digito_verificacion = buscaTercero.digito_verificacion ?? ""
                    };
                    ListasDesplegables(cotiza);
                    ParametrosBusqueda();
                    BuscarFavoritos(menu);
                    return View(cotiza);
                }
                else
                {
                    Cotizacion cotiza = new Cotizacion
                    {
                        tpdoc_id = buscaTercero.tpdoc_id ?? 0,
                        doc_tercero = buscaTercero.doc_tercero,
                        prinom_tercero = buscaTercero.prinom_tercero,
                        segnom_tercero = buscaTercero.segnom_tercero,
                        apellido_tercero = buscaTercero.apellido_tercero,
                        segapellido_tercero = buscaTercero.segapellido_tercero,
                        medcomun_id = buscaTercero.medcomun_id ?? 0,
                        tercero_id = buscaTercero.tercero_id,
                        correo = buscaTercero.email_tercero,
                        gentercero_id = buscaTercero.genero_tercero ?? 0,
                        habdtautor_celular = buscaTercero.habdtautor_celular,
                        habdtautor_correo = buscaTercero.habdtautor_correo,
                        habdtautor_msm = buscaTercero.habdtautor_msm,
                        habdtautor_whatsapp = buscaTercero.habdtautor_watsap,
                        observacion = buscaTercero.observaciones ?? "",
                        telefono_tercero = buscaTercero.telf_tercero ?? "",
                        celular_tercero = buscaTercero.celular_tercero ?? "",
                        tporigen_id = buscaTercero.origen_id ?? 0,
                        subfuente_id = buscaTercero.subfuente ?? 0,
                        eventoOrigen = buscaTercero.sitioevento ?? 0,
                        razon_social = buscaTercero.razon_social ?? "",
                        digito_verificacion = buscaTercero.digito_verificacion ?? ""
                    };
                    ListasDesplegables(cotiza);
                    ParametrosBusqueda();
                    BuscarFavoritos(menu);
                    return View(cotiza);
                }
            }

            ListasDesplegables(null);
            ParametrosBusqueda();
            BuscarFavoritos(menu);
            return View(new Cotizacion());
        }

        [HttpPost]
        public ActionResult Create(Cotizacion cotizacion, int? menu)
                           {
            if (Session["user_usuarioid"] != null)
            {
                //  modvh_codigo anio  colVh_id  valor 
                int idcotizacioncreada = 0;
                string ocupacion2 = Request["ocupacion"];
                int ocupacion = 0;

                if (ocupacion2 == "" || ocupacion2 == "")
                {
                    ocupacion = context.tp_ocupacion.FirstOrDefault().tpocupacion_id;
                }
                else 
                {
                    ocupacion = Convert.ToInt32(ocupacion2);
                }

                string camposObligatorios = "<label>Los siguientes campos son obligatorios:<label><ul>";
                int vacio = 0;
                if (cotizacion.tpdoc_id == 0) { camposObligatorios += "<li>Tipo Documento</li>"; vacio = 1; }
                if (string.IsNullOrWhiteSpace(cotizacion.doc_tercero)) { camposObligatorios += "<li>Documento</li>"; vacio = 1; }
                if (cotizacion.tpdoc_id != 1)
                {
                    if (string.IsNullOrWhiteSpace(cotizacion.prinom_tercero)) { camposObligatorios += "<li>Primer Nombre</li>"; vacio = 1; }
                    if (string.IsNullOrWhiteSpace(cotizacion.apellido_tercero)) { camposObligatorios += "<li>Primer Apellido</li>"; vacio = 1; }
                    if (cotizacion.gentercero_id == null) { camposObligatorios += "<li>Genero</li>"; vacio = 1; }
                    if (string.IsNullOrWhiteSpace(Request["ocupacion"])) { camposObligatorios += "<li>Ocupación</li>"; vacio = 1; }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(cotizacion.digito_verificacion)) { camposObligatorios += "<li>Digito de Verificación</li>"; vacio = 1; }
                    if (string.IsNullOrWhiteSpace(cotizacion.razon_social)) { camposObligatorios += "<li>Razón Social</li>"; vacio = 1; }
                }
                if (string.IsNullOrWhiteSpace(cotizacion.correo)) { camposObligatorios += "<li>Correo</li>"; vacio = 1; }
                if (string.IsNullOrWhiteSpace(cotizacion.celular_tercero)) { camposObligatorios += "<li>Celular</li>"; vacio = 1; }
                if (string.IsNullOrWhiteSpace(cotizacion.direc_tercero)) { camposObligatorios += "<li>Dirección Completa</li>"; vacio = 1; }
                if (cotizacion.dpto_id == 0) { camposObligatorios += "<li>Departamento</li>"; vacio = 1; }
                if (cotizacion.ciu_id == 0) { camposObligatorios += "<li>Ciudad</li>"; vacio = 1; }
                if (cotizacion.tporigen_id == 0) { camposObligatorios += "<li>Fuente</li>"; vacio = 1; }
                if (cotizacion.subfuente_id == null) { if (cotizacion.tporigen_id == 1) { camposObligatorios += "<li>Sub-Fuente</li>"; vacio = 1; } }
                if (cotizacion.medcomun_id == 0) { camposObligatorios += "<li>Medio Comunicación</li>"; vacio = 1; }
                if (cotizacion.asesorAsignado == 0) { camposObligatorios += "<li>Asesor</li>"; vacio = 1; }
                camposObligatorios += "</ul>";
                if (vacio == 1)
                {
                    TempData["mensaje_obligatorios"] = camposObligatorios;
                }

                if (ModelState.IsValid && vacio == 0)
                {
                    using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                    {
                        try
                        {
                            #region varables generales

                            bool result = false;
                            int terceroIdCotizacion = 0;
                            decimal numeroConsecutivo = 0;
                            icb_terceros buscaDocumento =context.icb_terceros.FirstOrDefault(x => x.doc_tercero == cotizacion.doc_tercero);

                            string tp_via = !string.IsNullOrEmpty(Request["tp_via"]) ? Request["tp_via"] + " " : "";
                            string num_via = !string.IsNullOrEmpty(Request["num_via"]) ? Request["num_via"] + " " : "";
                            string sector_via = !string.IsNullOrEmpty(Request["sector_via"])
                                ? Request["sector_via"] + " "
                                : "";
                            string num_via_generadora = !string.IsNullOrEmpty(Request["num_via_generadora"])
                                ? Request["num_via_generadora"] + " "
                                : "";
                            string num_placa = !string.IsNullOrEmpty(Request["num_placa"]) ? Request["num_placa"] + " " : "";
                            string sector_via_generadora = !string.IsNullOrEmpty(Request["sector_via_generadora"])
                                ? Request["sector_via_generadora"] + " "
                                : "";
                            string otros = !string.IsNullOrEmpty(Request["otros"]) ? Request["otros"] : "";
                            string direccionCompleta = cotizacion.direc_tercero;
                            icb_terceros tercero = context.icb_terceros.FirstOrDefault(x => x.tercero_id == cotizacion.tercero_id);

                            #endregion

                            #region Creacion de tercero si no existe en la BD

                            if (tercero == null)
                            {
                                if (buscaDocumento == null)
                                {
                                    context.icb_terceros.Add(new icb_terceros
                                    {
                                        tpdoc_id = cotizacion.tpdoc_id,
                                        doc_tercero = cotizacion.doc_tercero,
                                        digito_verificacion = cotizacion.digito_verificacion,
                                        prinom_tercero = cotizacion.prinom_tercero,
                                        segnom_tercero = cotizacion.segnom_tercero,
                                        apellido_tercero = cotizacion.apellido_tercero,
                                        segapellido_tercero = cotizacion.segapellido_tercero,
                                        email_tercero = cotizacion.correo,
                                        celular_tercero = cotizacion.celular_tercero,
                                        ciu_id = cotizacion.ciu_id,
                                        genero_tercero = cotizacion.gentercero_id,
                                        tercerofec_creacion = DateTime.Now,
                                        tercerouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                        medcomun_id = cotizacion.medcomun_id,
                                        origen_id = cotizacion.tporigen_id,
                                        razon_social = cotizacion.razon_social,
                                        tercero_estado = true
                                    });

                                    result = context.SaveChanges() > 0;
                                    if (result)
                                    {
                                        terceroIdCotizacion = context.icb_terceros.OrderByDescending(x => x.tercero_id)
                                            .First().tercero_id;
                                    }
                                }
                                else
                                {
                                    terceroIdCotizacion = buscaDocumento.tercero_id;
                                    buscaDocumento.tpdoc_id = cotizacion.tpdoc_id;
                                    buscaDocumento.digito_verificacion = cotizacion.digito_verificacion;
                                    buscaDocumento.doc_tercero = cotizacion.doc_tercero;
                                    buscaDocumento.prinom_tercero = cotizacion.prinom_tercero;
                                    buscaDocumento.segnom_tercero = cotizacion.segnom_tercero;
                                    buscaDocumento.apellido_tercero = cotizacion.apellido_tercero;
                                    buscaDocumento.segapellido_tercero = cotizacion.segapellido_tercero;
                                    buscaDocumento.email_tercero = cotizacion.correo;
                                    buscaDocumento.celular_tercero = cotizacion.celular_tercero;
                                    buscaDocumento.ciu_id = cotizacion.ciu_id;
                                    buscaDocumento.genero_tercero = cotizacion.gentercero_id;
                                    buscaDocumento.medcomun_id = cotizacion.medcomun_id;
                                    buscaDocumento.origen_id = cotizacion.tporigen_id;
                                    buscaDocumento.razon_social = cotizacion.razon_social;
                                    buscaDocumento.tercero_estado = true;
                                    context.Entry(buscaDocumento).State = EntityState.Modified;
                                }
                            }
                            else
                            {
                                terceroIdCotizacion = tercero.tercero_id;
                                tercero.tpdoc_id = cotizacion.tpdoc_id;
                                tercero.digito_verificacion = cotizacion.digito_verificacion;
                                tercero.doc_tercero = cotizacion.doc_tercero;
                                tercero.prinom_tercero = cotizacion.prinom_tercero;
                                tercero.segnom_tercero = cotizacion.segnom_tercero;
                                tercero.apellido_tercero = cotizacion.apellido_tercero;
                                tercero.segapellido_tercero = cotizacion.segapellido_tercero;
                                tercero.email_tercero = cotizacion.correo;
                                tercero.celular_tercero = cotizacion.celular_tercero;
                                tercero.ciu_id = cotizacion.ciu_id;
                                tercero.genero_tercero = cotizacion.gentercero_id;
                                tercero.medcomun_id = cotizacion.medcomun_id;
                                tercero.origen_id = cotizacion.tporigen_id;
                                tercero.razon_social = cotizacion.razon_social;
                                tercero.tercero_estado = true;
                                context.Entry(tercero).State = EntityState.Modified;

                                tercero_cliente buscarOcup = context.tercero_cliente.Where(x => x.cltercero_id == tercero.tercero_id)
                                    .FirstOrDefault();
                                if (buscarOcup != null)
                                {
                                    buscarOcup.tpocupacion_id = ocupacion;
                                    context.Entry(buscarOcup).State = EntityState.Modified;
                                }
                            }

                            #endregion

                            #region Actualizar ultima direccion

                            terceros_direcciones buscarUltimaDireccion = context.terceros_direcciones.OrderByDescending(x => x.id)
                                .FirstOrDefault(x => x.idtercero == terceroIdCotizacion);
                            if (buscarUltimaDireccion != null)
                            {
                                buscarUltimaDireccion.sector = null;
                                buscarUltimaDireccion.ciudad = cotizacion.ciu_id;
                                buscarUltimaDireccion.direccion = direccionCompleta;
                                context.Entry(buscarUltimaDireccion).State = EntityState.Modified;
                            }
                            else
                            {
                                context.terceros_direcciones.Add(new terceros_direcciones
                                {
                                    sector = null,
                                    ciudad = cotizacion.ciu_id,
                                    direccion = direccionCompleta,
                                    idtercero = terceroIdCotizacion
                                });
                            }

                            #endregion

                            result = context.SaveChanges() > 0; //guardamos el tercero y la direccion

                            // El modulo de cotizacion es el 104 actualmente
                            //var buscarParamDocConsecutivoCotiza = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P10");
                            //var docConsecutivoCotizacionParametro = buscarParamDocConsecutivoCotiza != null ? buscarParamDocConsecutivoCotiza.syspar_value : "104";
                            //var idModuloCotizacion = Convert.ToInt32(docConsecutivoCotizacionParametro);

                            // Tipo de documento, el necesario es tipo de documento, en este caso en necesario el 1 correpondiente el tipo de doc cotizacion
                            icb_sysparameter buscarParametroTpDocCotiza =
                                context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P11");
                            string documentoCotizacionParametro = buscarParametroTpDocCotiza != null
                                ? buscarParametroTpDocCotiza.syspar_value
                                : "1";
                            int idTpDocCotizacion = Convert.ToInt32(documentoCotizacionParametro);

                            bool guardaCotizacion = false;
                            //var buscaAnioModelo = context.anio_modelo.FirstOrDefault(x => x.codigo_modelo == cotizacion.modvh_codigo && x.anio == cotizacion.anio);
                            // 104 es el id del menu para cotizacion
                            //var anioActual = DateTime.Now.Year;
                            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);

                            #region buscamos el consecutivo de la cotizacion y la bodega y guardamos en icb_cotizacion

                            ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
                            icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
                            tp_doc_registros buscarTipoDocRegistro =context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == idTpDocCotizacion);
                            numeroConsecutivoAux =
                                gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro, idTpDocCotizacion,
                                    bodegaActual);

                            //var buscaConsecutivo = context.icb_doc_consecutivos.OrderByDescending(x=>x.doccons_id).FirstOrDefault(x => x.doccons_bodega == bodegaActual && x.doccons_idtpdoc == idTpDocCotizacion);
                            grupoconsecutivos grupoConsecutivo = context.grupoconsecutivos.FirstOrDefault(x =>
                                x.documento_id == idTpDocCotizacion && x.bodega_id == bodegaActual);
                            try
                            {
                                if (numeroConsecutivoAux != null)
                                {
                                    numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                                }
                                else
                                {
                                    TempData["mensaje_error"] =
                                        "No existe un numero consecutivo asignado para este tipo de documento";
                                    dbTran.Rollback();
                                    ListasDesplegables(cotizacion);
                                    BuscarFavoritos(menu);
                                    ViewBag.pdf = 0;
                                    ViewBag.cotizacionn = 0;
                                    return View(cotizacion);
                                }

                                #region guardado de la cotizacion

                                icb_cotizacion nuevaCotizacion = new icb_cotizacion
                                {
                                    id_tercero = terceroIdCotizacion,
                                    cot_numcotizacion = numeroConsecutivo,
                                    cot_feccreacion = DateTime.Now,
                                    cot_usucreacion = Convert.ToInt32(Session["user_usuarioid"]),
                                    id_medio = cotizacion.medcomun_id,
                                    cot_envio_cotizacion = cotizacion.envio_correo,
                                    cot_origen = cotizacion.tporigen_id,
                                    id_subfuente = cotizacion.subfuente_id,
                                    bodegaid = bodegaActual,
                                    asesor = cotizacion.asesorAsignado
                                };
                                if (cotizacion.eventoOrigen == 0)
                                {
                                    nuevaCotizacion.cot_sitio = null;
                                }
                                else
                                {
                                    nuevaCotizacion.cot_sitio = cotizacion.eventoOrigen;
                                }

                                context.icb_cotizacion.Add(nuevaCotizacion);
                                guardaCotizacion = context.SaveChanges() > 0;
                                idcotizacioncreada = nuevaCotizacion.cot_idserial;
                                #endregion

                                #region creacion de planpago, plan matriz 

                                string json = Request["txt_json_listas"];
                                if (json != null && json != "")
                                {
                                    icb_plan_pago planPago = new icb_plan_pago();
                                    icb_plan_matriz planMatriz = new icb_plan_matriz();
                                    bool guardaPlanPago = false;
                                    bool guardaPlanMatriz = false;
                                    JArray a = JArray.Parse(json);

                                    if (a.Count >= 1)
                                    {
                                        string vehiculo1 = Convert.ToString(a[0]);
                                        JArray vehiculo_uno = JArray.Parse(vehiculo1);

                                        #region Extraccion de las coutas

                                        string cuota12 = Convert.ToString(vehiculo_uno[5]) != ""
                                            ? Convert.ToString(vehiculo_uno[5] + ",")
                                            : "";
                                        string cuota24 = Convert.ToString(vehiculo_uno[6]) != ""
                                            ? Convert.ToString(vehiculo_uno[6] + ",")
                                            : "";
                                        string cuota36 = Convert.ToString(vehiculo_uno[7]) != ""
                                            ? Convert.ToString(vehiculo_uno[7] + ",")
                                            : "";
                                        string cuota48 = Convert.ToString(vehiculo_uno[8]) != ""
                                            ? Convert.ToString(vehiculo_uno[8] + ",")
                                            : "";
                                        string cuota60 = Convert.ToString(vehiculo_uno[9]) != ""
                                            ? Convert.ToString(vehiculo_uno[9] + ",")
                                            : "";
                                        string cuota72 = Convert.ToString(vehiculo_uno[10]) != ""
                                            ? Convert.ToString(vehiculo_uno[10] + ",")
                                            : "";
                                        string cuota84 = Convert.ToString(vehiculo_uno[11]) != ""
                                            ? Convert.ToString(vehiculo_uno[11] + ",")
                                            : "";

                                        #endregion

                                        #region Creacion en icb_plan_pago

                                        planPago.asesor_id = Convert.ToInt32(Session["user_usuarioid"]);
                                        planPago.cotizacion_id = nuevaCotizacion.cot_idserial;
                                        planPago.tasa_interes = Convert.ToString(vehiculo_uno[4]) != null
                                            ? Convert.ToString(vehiculo_uno[4])
                                            : "";
                                        planPago.valor_total = Convert.ToDecimal(vehiculo_uno[0], inter);
                                        planPago.credito = Convert.ToDecimal(vehiculo_uno[3], inter);
                                        planPago.cuota_inicial = Convert.ToDecimal(vehiculo_uno[1], inter);
                                        planPago.cuotas =
                                            cuota12 + cuota24 + cuota36 + cuota48 + cuota60 + cuota72 + cuota84;
                                        planPago.plan_elegido = false;
                                        planPago.modelo = Convert.ToString(vehiculo_uno[19]);
                                        if (Convert.ToInt32(vehiculo_uno[20]) != 0)
                                        {
                                            planPago.plan_id = Convert.ToInt32(vehiculo_uno[20]);
                                        }

                                        string valorResidual = vehiculo_uno[21] != null ? vehiculo_uno[21].ToString() : "0";
                                        planPago.valor_residual =
                                            Convert.ToDecimal(!string.IsNullOrWhiteSpace(valorResidual)
                                                ? valorResidual
                                                : "0", inter);
                                        string otrovalor = vehiculo_uno[22] != null ? vehiculo_uno[22].ToString() : "0";
                                        planPago.otros_valores =
                                            Convert.ToDecimal(!string.IsNullOrWhiteSpace(otrovalor) ? otrovalor : "0", inter);

                                        context.icb_plan_pago.Add(planPago);
                                        guardaPlanPago = context.SaveChanges() > 0;

                                        #endregion

                                        #region Creacion en icb_plan_matriz

                                        int contador = 4;
                                        int contador2 = 11;

                                        for (int i = 0; i < vehiculo_uno.Count; i++)
                                        {
                                            contador = contador + 1;
                                            contador2 = contador2 + 1;
                                            if (contador < vehiculo_uno.Count)
                                            {
                                                if (vehiculo_uno[contador] != null &&
                                                    Convert.ToString(vehiculo_uno[contador]) != "" && contador < 12)
                                                {
                                                    planMatriz.plan_pago_id = planPago.id;
                                                    planMatriz.plazo = Convert.ToString(vehiculo_uno[contador]);
                                                    planMatriz.valor = Convert.ToDecimal(vehiculo_uno[contador2], inter);
                                                    planMatriz.cotizacion_id = nuevaCotizacion.cot_idserial;
                                                    planMatriz.tasa = Convert.ToDecimal(vehiculo_uno[4], inter);
                                                    context.icb_plan_matriz.Add(planMatriz);
                                                    guardaPlanMatriz = context.SaveChanges() > 0;
                                                }
                                            }
                                        }

                                        #endregion
                                    }

                                    if (a.Count >= 2)
                                    {
                                        string vehiculo2 = Convert.ToString(a[1]);
                                        JArray vehiculo_dos = JArray.Parse(vehiculo2);

                                        #region Extraccion de las coutas

                                        string cuota12 = Convert.ToString(vehiculo_dos[5]) != ""
                                            ? Convert.ToString(vehiculo_dos[5] + ",")
                                            : "";
                                        string cuota24 = Convert.ToString(vehiculo_dos[6]) != ""
                                            ? Convert.ToString(vehiculo_dos[6] + ",")
                                            : "";
                                        string cuota36 = Convert.ToString(vehiculo_dos[7]) != ""
                                            ? Convert.ToString(vehiculo_dos[7] + ",")
                                            : "";
                                        string cuota48 = Convert.ToString(vehiculo_dos[8]) != ""
                                            ? Convert.ToString(vehiculo_dos[8] + ",")
                                            : "";
                                        string cuota60 = Convert.ToString(vehiculo_dos[9]) != ""
                                            ? Convert.ToString(vehiculo_dos[9] + ",")
                                            : "";
                                        string cuota72 = Convert.ToString(vehiculo_dos[10]) != ""
                                            ? Convert.ToString(vehiculo_dos[10] + ",")
                                            : "";
                                        string cuota84 = Convert.ToString(vehiculo_dos[11]) != ""
                                            ? Convert.ToString(vehiculo_dos[11] + ",")
                                            : "";

                                        #endregion

                                        #region Creacion en icb_plan_pago

                                        planPago.asesor_id = Convert.ToInt32(Session["user_usuarioid"]);
                                        planPago.cotizacion_id = nuevaCotizacion.cot_idserial;
                                        planPago.tasa_interes = Convert.ToString(vehiculo_dos[4]) != null
                                            ? Convert.ToString(vehiculo_dos[4])
                                            : "";
                                        planPago.valor_total = Convert.ToDecimal(vehiculo_dos[0], inter);
                                        planPago.credito = Convert.ToDecimal(vehiculo_dos[3], inter);
                                        planPago.cuota_inicial = Convert.ToDecimal(vehiculo_dos[1], inter);
                                        planPago.cuotas =
                                            cuota12 + cuota24 + cuota36 + cuota48 + cuota60 + cuota72 + cuota84;
                                        planPago.plan_elegido = false;
                                        planPago.modelo = Convert.ToString(vehiculo_dos[19]);
                                        if (Convert.ToInt32(vehiculo_dos[20]) != 0)
                                        {
                                            planPago.plan_id = Convert.ToInt32(vehiculo_dos[20]);
                                        }

                                        if (vehiculo_dos[21] != null)
                                        {
                                        }

                                        planPago.valor_residual = vehiculo_dos[21].HasValues
                                            ? Convert.ToDecimal(vehiculo_dos[21], inter)
                                            : 0;
                                        planPago.otros_valores = vehiculo_dos[22].HasValues
                                            ? Convert.ToDecimal(vehiculo_dos[22], inter)
                                            : 0;

                                        context.icb_plan_pago.Add(planPago);
                                        guardaPlanPago = context.SaveChanges() > 0;

                                        #endregion

                                        #region Creacion en icb_plan_matriz

                                        int contador = 4;
                                        int contador2 = 11;

                                        for (int i = 0; i < vehiculo_dos.Count; i++)
                                        {
                                            contador = contador + 1;
                                            contador2 = contador2 + 1;
                                            if (contador < vehiculo_dos.Count)
                                            {
                                                if (vehiculo_dos[contador] != null &&
                                                    Convert.ToString(vehiculo_dos[contador]) != "" && contador < 12)
                                                {
                                                    planMatriz.plan_pago_id = planPago.id;
                                                    planMatriz.plazo = Convert.ToString(vehiculo_dos[contador]);
                                                    planMatriz.valor = Convert.ToDecimal(vehiculo_dos[contador2], inter);
                                                    planMatriz.cotizacion_id = nuevaCotizacion.cot_idserial;
                                                    planMatriz.tasa = Convert.ToDecimal(vehiculo_dos[4], inter);
                                                    context.icb_plan_matriz.Add(planMatriz);
                                                    guardaPlanMatriz = context.SaveChanges() > 0;
                                                }
                                            }
                                        }

                                        #endregion
                                    }

                                    if (a.Count >= 3)
                                    {
                                        string vehiculo3 = Convert.ToString(a[2]);
                                        JArray vehiculo_tres = JArray.Parse(vehiculo3);

                                        #region Extraccion de las coutas

                                        string cuota12 = Convert.ToString(vehiculo_tres[5]) != ""
                                            ? Convert.ToString(vehiculo_tres[5] + ",")
                                            : "";
                                        string cuota24 = Convert.ToString(vehiculo_tres[6]) != ""
                                            ? Convert.ToString(vehiculo_tres[6] + ",")
                                            : "";
                                        string cuota36 = Convert.ToString(vehiculo_tres[7]) != ""
                                            ? Convert.ToString(vehiculo_tres[7] + ",")
                                            : "";
                                        string cuota48 = Convert.ToString(vehiculo_tres[8]) != ""
                                            ? Convert.ToString(vehiculo_tres[8] + ",")
                                            : "";
                                        string cuota60 = Convert.ToString(vehiculo_tres[9]) != ""
                                            ? Convert.ToString(vehiculo_tres[9] + ",")
                                            : "";
                                        string cuota72 = Convert.ToString(vehiculo_tres[10]) != ""
                                            ? Convert.ToString(vehiculo_tres[10] + ",")
                                            : "";
                                        string cuota84 = Convert.ToString(vehiculo_tres[11]) != ""
                                            ? Convert.ToString(vehiculo_tres[11] + ",")
                                            : "";

                                        #endregion

                                        #region Creacion en icb_plan_pago

                                        planPago.asesor_id = Convert.ToInt32(Session["user_usuarioid"]);
                                        planPago.cotizacion_id = nuevaCotizacion.cot_idserial;
                                        planPago.tasa_interes = Convert.ToString(vehiculo_tres[4]) != null
                                            ? Convert.ToString(vehiculo_tres[4])
                                            : "";
                                        planPago.valor_total = Convert.ToDecimal(vehiculo_tres[0], inter);
                                        planPago.credito = Convert.ToDecimal(vehiculo_tres[3], inter);
                                        planPago.cuota_inicial = Convert.ToDecimal(vehiculo_tres[1], inter);
                                        planPago.cuotas =
                                            cuota12 + cuota24 + cuota36 + cuota48 + cuota60 + cuota72 + cuota84;
                                        planPago.plan_elegido = false;
                                        planPago.modelo = Convert.ToString(vehiculo_tres[19]);
                                        if (Convert.ToInt32(vehiculo_tres[20]) != 0)
                                        {
                                            planPago.plan_id = Convert.ToInt32(vehiculo_tres[20]);
                                        }

                                        planPago.valor_residual = vehiculo_tres[21].HasValues
                                            ? Convert.ToDecimal(vehiculo_tres[21], inter)
                                            : 0;
                                        planPago.otros_valores = vehiculo_tres[22].HasValues
                                            ? Convert.ToDecimal(vehiculo_tres[22], inter)
                                            : 0;

                                        context.icb_plan_pago.Add(planPago);
                                        guardaPlanPago = context.SaveChanges() > 0;

                                        #endregion

                                        #region Creacion en icb_plan_matriz

                                        int contador = 4;
                                        int contador2 = 11;

                                        for (int i = 0; i < vehiculo_tres.Count; i++)
                                        {
                                            contador = contador + 1;
                                            contador2 = contador2 + 1;
                                            if (contador < vehiculo_tres.Count)
                                            {
                                                if (vehiculo_tres[contador] != null &&
                                                    Convert.ToString(vehiculo_tres[contador]) != "" && contador < 12)
                                                {
                                                    planMatriz.plan_pago_id = planPago.id;
                                                    planMatriz.plazo = Convert.ToString(vehiculo_tres[contador]);
                                                    planMatriz.valor = Convert.ToDecimal(vehiculo_tres[contador2], inter);
                                                    planMatriz.cotizacion_id = nuevaCotizacion.cot_idserial;
                                                    planMatriz.tasa = Convert.ToDecimal(vehiculo_tres[4], inter);
                                                    context.icb_plan_matriz.Add(planMatriz);
                                                    guardaPlanMatriz = context.SaveChanges() > 0;
                                                }
                                            }
                                        }

                                        #endregion
                                    }

                                    if (a.Count >= 4)
                                    {
                                        string vehiculo4 = Convert.ToString(a[3]);
                                        JArray vehiculo_cuatro = JArray.Parse(vehiculo4);

                                        #region Extraccion de las coutas

                                        string cuota12 = Convert.ToString(vehiculo_cuatro[5]) != ""
                                            ? Convert.ToString(vehiculo_cuatro[5] + ",")
                                            : "";
                                        string cuota24 = Convert.ToString(vehiculo_cuatro[6]) != ""
                                            ? Convert.ToString(vehiculo_cuatro[6] + ",")
                                            : "";
                                        string cuota36 = Convert.ToString(vehiculo_cuatro[7]) != ""
                                            ? Convert.ToString(vehiculo_cuatro[7] + ",")
                                            : "";
                                        string cuota48 = Convert.ToString(vehiculo_cuatro[8]) != ""
                                            ? Convert.ToString(vehiculo_cuatro[8] + ",")
                                            : "";
                                        string cuota60 = Convert.ToString(vehiculo_cuatro[9]) != ""
                                            ? Convert.ToString(vehiculo_cuatro[9] + ",")
                                            : "";
                                        string cuota72 = Convert.ToString(vehiculo_cuatro[10]) != ""
                                            ? Convert.ToString(vehiculo_cuatro[10] + ",")
                                            : "";
                                        string cuota84 = Convert.ToString(vehiculo_cuatro[11]) != ""
                                            ? Convert.ToString(vehiculo_cuatro[11] + ",")
                                            : "";

                                        #endregion

                                        #region Creacion en icb_plan_pago

                                        planPago.asesor_id = Convert.ToInt32(Session["user_usuarioid"]);
                                        planPago.cotizacion_id = nuevaCotizacion.cot_idserial;
                                        planPago.tasa_interes = Convert.ToString(vehiculo_cuatro[4]) != null
                                            ? Convert.ToString(vehiculo_cuatro[4])
                                            : "";
                                        planPago.valor_total = Convert.ToDecimal(vehiculo_cuatro[0], inter);
                                        planPago.credito = Convert.ToDecimal(vehiculo_cuatro[3], inter);
                                        planPago.cuota_inicial = Convert.ToDecimal(vehiculo_cuatro[1], inter);
                                        planPago.cuotas =
                                            cuota12 + cuota24 + cuota36 + cuota48 + cuota60 + cuota72 + cuota84;
                                        planPago.plan_elegido = false;
                                        planPago.modelo = Convert.ToString(vehiculo_cuatro[19]);
                                        if (Convert.ToInt32(vehiculo_cuatro[20]) != 0)
                                        {
                                            planPago.plan_id = Convert.ToInt32(vehiculo_cuatro[20]);
                                        }

                                        planPago.valor_residual = vehiculo_cuatro[21].HasValues
                                            ? Convert.ToDecimal(vehiculo_cuatro[21], inter)
                                            : 0;
                                        planPago.otros_valores = vehiculo_cuatro[22].HasValues
                                            ? Convert.ToDecimal(vehiculo_cuatro[22], inter)
                                            : 0;

                                        context.icb_plan_pago.Add(planPago);
                                        guardaPlanPago = context.SaveChanges() > 0;

                                        #endregion

                                        #region Creacion en icb_plan_matriz

                                        int contador = 4;
                                        int contador2 = 11;

                                        for (int i = 0; i < vehiculo_cuatro.Count; i++)
                                        {
                                            contador = contador + 1;
                                            contador2 = contador2 + 1;
                                            if (contador < vehiculo_cuatro.Count)
                                            {
                                                if (vehiculo_cuatro[contador] != null &&
                                                    Convert.ToString(vehiculo_cuatro[contador]) != "" && contador < 12)
                                                {
                                                    planMatriz.plan_pago_id = planPago.id;
                                                    planMatriz.plazo = Convert.ToString(vehiculo_cuatro[contador]);
                                                    planMatriz.valor = Convert.ToDecimal(vehiculo_cuatro[contador2], inter);
                                                    planMatriz.cotizacion_id = nuevaCotizacion.cot_idserial;
                                                    planMatriz.tasa = Convert.ToDecimal(vehiculo_cuatro[4], inter);
                                                    context.icb_plan_matriz.Add(planMatriz);
                                                    guardaPlanMatriz = context.SaveChanges() > 0;
                                                }
                                            }
                                        }

                                        #endregion
                                    }
                                }

                                #endregion
                            }
                            catch (DbEntityValidationException)
                            {
                                TempData["mensaje_error"] = "Error al actualizar los datos, intente mas tarde";
                                dbTran.Rollback();
                            }

                            #endregion

                            if (result && guardaCotizacion)
                            {
                                // -------------------------------------------------------------------------------------------------------------------------------------------
                                // Si llega hasta aqui significa que ya se guardo el tercero y el encabezado de la cotizacion, solo falta agregar los elementos cotizados.
                                // -------------------------------------------------------------------------------------------------------------------------------------------
                                icb_cotizacion buscarSerialUltimaCot = context.icb_cotizacion.OrderByDescending(x => x.cot_idserial).Where(d => d.cot_idserial == idcotizacioncreada)
                                    .FirstOrDefault();

                                #region Seguimiento de la cotizacion
                                var BuscarSeguimiento = context.Seguimientos.Where(x => x.EsManual == false && x.ModuloSeguimientos.Codigo == 6 && x.Codigo == 1).FirstOrDefault();
                                vcotseguimiento seguimiento = new vcotseguimiento
                                {
                                    cot_id = buscarSerialUltimaCot.cot_idserial,
                                    fecha = DateTime.Now,
                                    responsable = Convert.ToInt32(Session["user_usuarioid"]),
                                    Notas = "Creo Cotizacion: Cliente en proceso de negociacion",
                                    Motivo = null,
                                    fec_creacion = DateTime.Now,
                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                    estado = true,
                                    tipo_seguimiento = BuscarSeguimiento.Id
                                };
                                context.vcotseguimiento.Add(seguimiento);

                                #endregion

                                //#region Guardamos los vehiculos que hayan sido cotizados

                                //#endregion

                                int guardarGeneral =
                                    context
                                        .SaveChanges(); //con este solo SaveChanges guardamos el seguimiento, vehiculos, accesorios y retomas

                                #region Actualizamos el consecutivo, creamos el seguimiento del tercero y finaliza el proceso

                                // Cambiar el numero consecutivo asignando un numero mas
                                bool guardarConsecutivo = false;
                                //var grupoConsecutivo = context.grupoconsecutivos.FirstOrDefault(x => x.documento_id == 8 && x.bodega_id == bodegaActual);
                                int grupoId = grupoConsecutivo != null ? grupoConsecutivo.grupo : 0;
                                List<grupoconsecutivos> gruposConsecutivos = context.grupoconsecutivos.Where(x => x.grupo == grupoId).ToList();
                                foreach (grupoconsecutivos grupo in gruposConsecutivos)
                                {
                                    icb_doc_consecutivos buscarElemento = context.icb_doc_consecutivos.FirstOrDefault(x =>
                                        x.doccons_idtpdoc == grupo.documento_id && x.doccons_bodega == grupo.bodega_id);
                                    buscarElemento.doccons_siguiente = buscarElemento.doccons_siguiente + 1;
                                    context.Entry(buscarElemento).State = EntityState.Modified;
                                }

                                guardarConsecutivo = context.SaveChanges() > 0;
                                //seguimiento tercero cotizacion
                                if (guardarConsecutivo)
                                {
                                    //string buscarParametroSeguimiento = (from parametro in context.icb_sysparameter
                                    //                                     where parametro.syspar_cod == "P57" select parametro.syspar_value).FirstOrDefault();
                                    var BuscarSeguimientoTercero = context.Seguimientos.Where(x => x.EsManual == false && x.ModuloSeguimientos.Codigo == 4 && x.Codigo == 20).FirstOrDefault();
                                    if (BuscarSeguimientoTercero!=null)
                                    {
                                        context.seguimientotercero.Add(new seguimientotercero
                                        {
                                            idtercero = terceroIdCotizacion,
                                            fecha = DateTime.Now,
                                            nota = "El prospecto realizó una cotización con número " + numeroConsecutivo,
                                            tipo = BuscarSeguimientoTercero.Id,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                                        });
                                        int guardar = context.SaveChanges();
                                    }

                                    TempData["mensaje_correcto"] = "La cotizacion se ha guardado con exito";
                                    ViewBag.pdf = 1;
                                    TempData["numCotizacionCreada"] = numeroConsecutivo;
                                    dbTran.Commit();

                                    #region cotizacion digital

                                    //buscamos la ultima cotizacion
                                    icb_cotizacion buscarUltimaCot = context.icb_cotizacion.OrderByDescending(x => x.cot_feccreacion).Where(d => d.cot_idserial == idcotizacioncreada)
                                        .FirstOrDefault();
                                    //buscamos si el tercero tiene habilitada la opcion de recibir correo electronico
                                    icb_terceros terceroCot = context.icb_terceros.FirstOrDefault(x =>x.tercero_id == buscarUltimaCot.id_tercero);
                                    if (terceroCot.habdtautor_correo && cotizacion.envio_correo)
                                    {
                                        //buscamos si el vehiculo cotizado tiene informacion para la cotizacion digital
                                        int vehiculosCot = Convert.ToInt32(Request["lista_vehiculos"]);
                                        for (int i = 1; i <= vehiculosCot; i++)
                                        {
                                            string descripcionVH = Request["descripcion" + i];
                                            pcotizaciondigital buscarVHcotDigital =
                                                context.pcotizaciondigital.FirstOrDefault(x => x.modelo == descripcionVH);
                                            if (buscarVHcotDigital != null)
                                            {
                                                decimal valoretoma1 = 0;
                                                decimal valorAccesorios = 0;
                                                vw_cotizacion buscar2 = context.vw_cotizacion.Where(a =>
                                                    a.cot_idserial == buscarUltimaCot.cot_idserial &&
                                                    a.codigo_modelo == descripcionVH).FirstOrDefault();
                                                if (buscar2 != null)
                                                {
                                                    List<vcotretoma> retomass = context.vcotretoma
                                                        .Where(r => r.idcot == buscar2.cot_idserial).ToList();
                                                    if (retomass.Count() > 0)
                                                    {
                                                        valoretoma1 = retomass.Select(r => r.valor).Sum();
                                                    }

                                                    List<vcotrepuestos> accesoriosCOT = context.vcotrepuestos
                                                        .Where(a => a.cot_id == buscar2.cot_idserial).ToList();
                                                    if (accesoriosCOT != null)
                                                    {
                                                        valorAccesorios =
                                                            Convert.ToDecimal(accesoriosCOT.Select(a => a.vrunitario)
                                                                .Sum(), inter);
                                                    }
                                                }

                                                List<icb_plan_pago> buscarPlanPago = context.icb_plan_pago
                                                    .Where(x => x.cotizacion_id == buscarUltimaCot.cot_idserial).ToList();

                                                #region variables plan de pago

                                                decimal precioFinanciacion = 0;
                                                decimal cuotaInicial = 0;
                                                decimal credito = 0;
                                                decimal valorResidual = 0;
                                                decimal otrosValores = 0;
                                                decimal tasaInteres = 0;

                                                #endregion

                                                if (buscarPlanPago.Count > 0)
                                                {
                                                    for (int contador = 0; contador < buscarPlanPago.Count; contador++)
                                                    {
                                                        if (buscarPlanPago[contador].modelo == descripcionVH)
                                                        {
                                                            int cotIdPlanPago = buscarPlanPago[contador].id;
                                                            List<planmatriz> planMatriz = context.icb_plan_matriz
                                                                .Where(x => x.plan_pago_id == cotIdPlanPago)
                                                                .GroupBy(x => x.plan_pago_id).Select(grp => new planmatriz
                                                                {
                                                                    llave = grp.Key.Value,
                                                                    cuenta = grp.Count(),
                                                                    plan = grp.Select(x => new planes_pago
                                                                    { valor = x.valor, plazo = x.plazo }).ToList()
                                                                }).ToList();

                                                            precioFinanciacion =
                                                                Convert.ToDecimal(buscarPlanPago[contador].valor_total, inter);
                                                            credito = Convert.ToDecimal(buscarPlanPago[contador].credito, inter);
                                                            cuotaInicial =
                                                                Convert.ToDecimal(buscarPlanPago[contador].cuota_inicial, inter);
                                                            valorResidual =
                                                                Convert.ToDecimal(buscarPlanPago[contador].valor_residual, inter);
                                                            otrosValores =
                                                                Convert.ToDecimal(buscarPlanPago[contador].otros_valores, inter);
                                                            tasaInteres =
                                                                Convert.ToDecimal(buscarPlanPago[contador].tasa_interes, inter);

                                                            string[] plazos = buscarPlanPago[contador].cuotas.Split(',');

                                                            #region variable buscar(toda la info a enviar al obj)

                                                            //var buscar = new
                                                            //{
                                                            //	buscar2.asesor,
                                                            //	buscar2.telefeno_asesor,
                                                            //	buscar2.correo_asesor,
                                                            //	buscar2.prinom_tercero,
                                                            //	buscar2.vehiculo,
                                                            //	buscar2.anio,
                                                            //	buscar2.color,
                                                            //	buscar2.precio,
                                                            //	buscar2.matricula,
                                                            //	buscar2.soat,
                                                            //	buscar2.poliza,
                                                            //	retoma = valoretoma1,
                                                            //	accesoriosCot = valorAccesorios,
                                                            //	precioFinanciacion,
                                                            //	credito,
                                                            //	cuotas,
                                                            //	cuotaInicial,
                                                            //	valorResidual,
                                                            //	otrosValores,
                                                            //	tasaInteres
                                                            //};

                                                            #endregion

                                                            #region variables PDF

                                                            //var root = Server.MapPath("~/Pdf/");
                                                            //var pdfname = String.Format("{0}.pdf", Guid.NewGuid().ToString());
                                                            //var path = Path.Combine(root, pdfname);
                                                            //path = Path.GetFullPath(path);
                                                            //CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

                                                            #endregion

                                                            #region chevystar

                                                            //var chevyStar = buscarVHcotDigital.chevyStar;
                                                            //var chevyStarIMG = "";
                                                            //var server = Request.Url.Scheme + "://" + Request.Url.Authority;

                                                            //if (chevyStar == 1)
                                                            //	chevyStarIMG =
                                                            //		server +
                                                            //		"/Images/imgCotizacionDigital/incluyeChevyStar.png";
                                                            //else if (chevyStar == 2)
                                                            //	chevyStarIMG =
                                                            //		server +
                                                            //		"/Images/imgCotizacionDigital/compatibleChevyStar.png";
                                                            //else
                                                            //	chevyStarIMG = "";

                                                            #endregion

                                                            #region titulos que envio en el obj

                                                            //var titulo1 = buscarVHcotDigital.tituloDet1;
                                                            //var split1 = titulo1.Split(' ');

                                                            //var titulo2 = buscarVHcotDigital.tituloDet2;
                                                            //var split2 = titulo2.Split(' ');

                                                            //var titulo3 = buscarVHcotDigital.tituloDet3;
                                                            //var split3 = titulo3.Split(' ');

                                                            #endregion

                                                            #region obj
                                                            #endregion

                                                            #region armar PDF y convertirlo para enviarlo en el correo electronico

                                                            //var something = new Rotativa.ViewAsPdf("CotizacionDigital", obj)
                                                            //{
                                                            //	FileName = "CotizacionDigital_" + obj.vehiculo + "_" + DateTime.Now + ".pdf",
                                                            //	SaveOnServerPath = path,
                                                            //	PageSize = Size.Letter,
                                                            //	PageOrientation = Orientation.Portrait,
                                                            //	PageMargins = new Margins(00, 00, 00, 00),

                                                            //};
                                                            //byte[] applicationPDFData = something.BuildPdf(ControllerContext);
                                                            //var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
                                                            //fileStream.Write(applicationPDFData, 0, applicationPDFData.Length);
                                                            //fileStream.Close();

                                                            #endregion

                                                        }
                                                    }
                                                }
                                                #region variable buscar(toda la info a enviar al obj)

                                                //	var buscar = new
                                                //	{
                                                //		buscar2.asesor,
                                                //		buscar2.telefeno_asesor,
                                                //		buscar2.correo_asesor,
                                                //		buscar2.prinom_tercero,
                                                //		buscar2.vehiculo,
                                                //		buscar2.anio,
                                                //		buscar2.color,
                                                //		buscar2.precio,
                                                //		buscar2.matricula,
                                                //		buscar2.soat,
                                                //		buscar2.poliza,
                                                //		retoma = valoretoma1,
                                                //		accesoriosCot = valorAccesorios,
                                                //		precioFinanciacion,
                                                //		credito,
                                                //		cuotas,
                                                //		cuotaInicial,
                                                //		valorResidual,
                                                //		otrosValores,
                                                //		tasaInteres
                                                //	};

                                                #endregion

                                                #region variables PDF

                                                //	var root = Server.MapPath("~/Pdf/");
                                                //	var pdfname = String.Format("{0}.pdf", Guid.NewGuid().ToString());
                                                //	var path = Path.Combine(root, pdfname);
                                                //	path = Path.GetFullPath(path);
                                                //	CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

                                                #endregion

                                                #region chevystar

                                                //	var chevyStar = buscarVHcotDigital.chevyStar;
                                                //	var chevyStarIMG = "";
                                                //	var server = Request.Url.Scheme + "://" + Request.Url.Authority;

                                                //	if (chevyStar == 1)
                                                //		chevyStarIMG =
                                                //			server + "/Images/imgCotizacionDigital/incluyeChevyStar.png";
                                                //	else if (chevyStar == 2)
                                                //		chevyStarIMG =
                                                //			server + "/Images/imgCotizacionDigital/compatibleChevyStar.png";
                                                //	else
                                                //		chevyStarIMG = "";

                                                #endregion

                                                #region titulos que envio en el obj

                                                //	var titulo1 = buscarVHcotDigital.tituloDet1;
                                                //	var split1 = titulo1.Split(' ');

                                                //	var titulo2 = buscarVHcotDigital.tituloDet2;
                                                //	var split2 = titulo2.Split(' ');

                                                //	var titulo3 = buscarVHcotDigital.tituloDet3;
                                                //	var split3 = titulo3.Split(' ');

                                                #endregion

                                                #region obj


                                                #endregion

                                                #region armar PDF y convertirlo para enviarlo en el correo electronico

                                                //	var something = new Rotativa.ViewAsPdf("CotizacionDigital", obj)
                                                //	{
                                                //		FileName = "CotizacionDigital_" + obj.vehiculo + "_" + DateTime.Now + ".pdf",
                                                //		SaveOnServerPath = path,
                                                //		PageSize = Size.Letter,
                                                //		PageOrientation = Orientation.Portrait,
                                                //		PageMargins = new Margins(00, 00, 00, 00),

                                                //	};
                                                //	byte[] applicationPDFData = something.BuildPdf(ControllerContext);
                                                //	var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
                                                //	fileStream.Write(applicationPDFData, 0, applicationPDFData.Length);
                                                //	fileStream.Close();

                                                #endregion

                                            }
                                        }
                                    }

                                    #endregion

                                    //return RedirectToAction("Create",
                                    //    new { id = "null", menu, pdf = 1, cotizacionn = buscarSerialUltimaCot.cot_idserial });
                                    return RedirectToAction("Ver",
                                       new { id = buscarSerialUltimaCot.cot_idserial, menu, pdf = 1, cotizacionn = buscarSerialUltimaCot.cot_idserial });
                                }

                                #endregion
                            }
                        }
                        catch (DbEntityValidationException)
                        {
                            dbTran.Rollback();
                            throw;
                        }
                    }
                }
                else
                {
                    List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                        .Where(y => y.Count > 0)
                        .ToList();
                    Dictionary<string, string[]> errorList = ModelState.Where(elem => elem.Value.Errors.Any()).ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? e.Exception.Message : e.ErrorMessage).ToArray());
                    List<string[]> errores = errorList.Select(d => d.Value).ToList();
                    string error = "";
                    if (errores.Count > 0)
                    {
                        error = error + "<ul>";
                        foreach (string[] item in errores)
                        {
                            error = error + "<li>" + item[0] + "</li>";

                        }
                        error = error + "</ul>";
                    }

                    TempData["mensaje_error"] = error;
                }

                
                //
                
                ListasDesplegables(cotizacion);
                BuscarFavoritos(menu);
                ViewBag.pdf = 0;
                ViewBag.cotizacionn = 0;

                return View(cotizacion);
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }

        public void objetosvehiculo(Cotizacion modelo,HttpRequest peticion)
        {
            var listadovehi = new List<ListaVehiculos>();
            var listadoacce = new List<ListaAccesorios>();
            var listadore = new List<ListaRetomas>();
            //
            #region guardado de vehiculos, accesorios y retomas para enviar al modelo de vuelta.

            #endregion

            modelo.listadovehiculos = listadovehi.Count();
            modelo.listadoaccesorios = listadoacce.Count();
            modelo.listadoretomas = listadore.Count();
            modelo.vehiculos = listadovehi;
            modelo.accesorios = listadoacce;
            modelo.retomas = listadore;
        }
        public ActionResult CotizacionDigital(int cotizacion_id)
        {
            string variable = "1";
            int exito = 0;
            string detalle = "";
            //buscamos la ultima cotizacion
            icb_cotizacion buscarUltimaCot = context.icb_cotizacion.Where(x => x.cot_idserial == cotizacion_id).OrderByDescending(x => x.cot_feccreacion).FirstOrDefault();
            icb_cotizacion buscarUltimaCot2 = context.icb_cotizacion.Where(x => x.cot_idserial == cotizacion_id).FirstOrDefault();
            //buscamos si el tercero tiene habilitada la opcion de recibir correo electronico
            icb_terceros terceroCot = context.icb_terceros.FirstOrDefault(x => x.tercero_id == buscarUltimaCot.id_tercero);
            //if (terceroCot.habdtautor_correo && buscarUltimaCot.cot_envio_cotizacion == true)
            if (buscarUltimaCot.cot_envio_cotizacion == true)
            {
                #region cotizacion digital
                //buscamos si el vehiculo cotizado tiene informacion para la cotizacion digital
                List<vcotdetallevehiculo> vehiculosCot = context.vcotdetallevehiculo.Where(x => x.idcotizacion == buscarUltimaCot.cot_idserial).ToList();
                var numvehiculos = vehiculosCot.Count();

                vcotdetallevehiculo vehiculosCota = context.vcotdetallevehiculo.FirstOrDefault(x => x.idcotizacion == cotizacion_id);
                var enviados = 0;
                var nombreenviados = "";
                var nombrenoenviados = "";
                var buscarVHcotDigital2 = "";
                foreach (vcotdetallevehiculo item in vehiculosCot)
                {
                    nombrenoenviados = item.modelo_vehiculo.modvh_nombre;
                    pcotizaciondigital buscarVHcotDigital = context.pcotizaciondigital.FirstOrDefault(x => x.modelo == item.idmodelo);
                    /// var buscarVHcotDigital22 = context.pcotizaciondigital.Where(x => x.modelo == item.idmodelo).FirstOrDefault();
                    if (buscarVHcotDigital == null)
                    {
                        buscarVHcotDigital2 = null;
                    }
                   else  if (buscarVHcotDigital != null)
                    {
                        decimal valoretoma1 = 0;
                        decimal valorAccesorios = 0;
                        vw_cotizacion buscar2 = context.vw_cotizacion.Where(a => a.cot_idserial == cotizacion_id).FirstOrDefault();
                        if (buscar2 != null)
                        {
                            List<vcotretoma> retomass = context.vcotretoma.Where(r => r.idcot == buscar2.cot_idserial).ToList();
                            if (retomass.Count() > 0)
                            {
                                valoretoma1 = retomass.Select(r => r.valor).Sum();
                            }

                            List<vcotrepuestos> accesoriosCOT = context.vcotrepuestos.Where(a => a.cot_id == buscar2.cot_idserial)
                                .ToList();
                            if (accesoriosCOT != null)
                            {
                                valorAccesorios = Convert.ToDecimal(accesoriosCOT.Select(a => a.vrunitario).Sum(), inter);
                            }
                        }

                        List<icb_plan_pago> buscarPlanPago = context.icb_plan_pago
                            .Where(x => x.cotizacion_id == cotizacion_id && x.modelo==item.idmodelo).ToList();

                        #region variables plan de pago

                        decimal precioFinanciacion = 0;
                        decimal cuotaInicial = 0;
                        decimal credito = 0;
                        int cuotas = 0;
                        decimal valorResidual = 0;
                        decimal otrosValores = 0;
                        decimal tasaInteres = 0;

                        #endregion

                        List<planmatriz> planMatriz = new List<planmatriz>();
                        if (buscarPlanPago.Count > 0)
                        {
                            for (int contador = 0; contador < buscarPlanPago.Count; contador++)
                            {
                                if (buscarPlanPago[contador].modelo == item.idmodelo)
                                {
                                    int cotIdPlanPago = buscarPlanPago[contador].id;

                                    planMatriz = context.icb_plan_matriz.Where(x => x.plan_pago_id == cotIdPlanPago)
                                        .GroupBy(x => x.plan_pago_id).Select(grp => new planmatriz
                                        {
                                            llave = grp.Key.Value,
                                            cuenta = grp.Count(),
                                            plan = grp.Select(x => new planes_pago { valor = x.valor, plazo = x.plazo })
                                                .ToList()
                                        }).ToList();

                                    precioFinanciacion = Convert.ToDecimal(buscarPlanPago[contador].valor_total, inter);
                                    credito = Convert.ToDecimal(buscarPlanPago[contador].credito);
                                    //cuotas = buscarPlanPago[contador].cuotas.Count();
                                    cuotaInicial = Convert.ToDecimal(buscarPlanPago[contador].cuota_inicial, inter);
                                    valorResidual = Convert.ToDecimal(buscarPlanPago[contador].valor_residual, inter);
                                    otrosValores = Convert.ToDecimal(buscarPlanPago[contador].otros_valores, inter);
                                    tasaInteres = Convert.ToDecimal(buscarPlanPago[contador].tasa_interes, inter);

                                    string[] plazos = buscarPlanPago[contador].cuotas.Split(',');

                                    #region variable buscar(toda la info a enviar al obj)

                                    var buscar = new
                                    {
                                        id=buscar2.cot_idserial,
                                        buscar2.tercero_id,
                                        buscar2.asesor,
                                        buscar2.telefeno_asesor,
                                        buscar2.correo_asesor,
                                        buscar2.prinom_tercero,
                                        buscar2.vehiculo,
                                        buscar2.anio,
                                        buscar2.color,
                                        buscar2.precio,
                                        buscar2.matricula,
                                        buscar2.soat,
                                        buscar2.poliza,
                                        retoma = valoretoma1,
                                        accesoriosCot = valorAccesorios,
                                        precioFinanciacion,
                                        credito,
                                        cuotas,
                                        cuotaInicial,
                                        valorResidual,
                                        otrosValores,
                                        tasaInteres
                                    };

                                    #endregion

                                    #region variables PDF

                                    string root = Server.MapPath("~/Pdf/");
                                    string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
                                    string path = Path.Combine(root, pdfname);
                                    path = Path.GetFullPath(path);
                                    CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

                                    #endregion

                                    #region chevystar

                                    int? chevyStar = buscarVHcotDigital.chevyStar;
                                    string chevyStarIMG = "";
                                    string server = Request.Url.Scheme + "://" + Request.Url.Authority;

                                    if (chevyStar == 1)
                                    {
                                        chevyStarIMG = server + "/Images/imgCotizacionDigital/incluyeChevyStar.png";
                                    }
                                    else if (chevyStar == 2)
                                    {
                                        chevyStarIMG = server + "/Images/imgCotizacionDigital/compatibleChevyStar.png";
                                    }
                                    else
                                    {
                                        chevyStarIMG = "";
                                    }

                                    #endregion

                                    #region titulos que envio en el obj

                                    string titulo1 = buscarVHcotDigital.tituloDet1;
                                    string[] split1 = titulo1.Split(' ');

                                    string titulo2 = buscarVHcotDigital.tituloDet2;
                                    string[] split2 = titulo2.Split(' ');

                                    string titulo3 = buscarVHcotDigital.tituloDet3;
                                    string[] split3 = titulo3.Split(' ');

                                    #endregion

                                    #region obj

                                    PDFmodel obj = new PDFmodel
                                    {
                                        idcotizacion = buscar.id,
                                        idtercero = buscar.tercero_id,
                                        Idtercero = buscar.tercero_id.ToString(),
                                        nombreAsesor = buscar.asesor,
                                        telefono_asesor = buscar.telefeno_asesor,
                                        correo_asesor = buscar.correo_asesor,
                                        nombreCompletoTercero = buscar.prinom_tercero,
                                        vehiculo = buscar.vehiculo,
                                        anioModelo = buscar.anio != null ? buscar.anio : 0,
                                        color = buscar.color,
                                        valorRetoma = buscar.retoma,
                                        valorAccesorios = buscar.accesoriosCot,
                                        /*valorRetoma = buscar.retoma != null ? buscar.retoma : 0,
                                        valorAccesorios = buscar.accesoriosCot != null ? buscar.accesoriosCot : 0,*/
                                        matricula = buscar.matricula,
                                        soat = buscar.soat,
                                        poliza = buscar.poliza,
                                        imgPrincipal = server + buscarVHcotDigital.imgPrincipal,
                                        imgDetalle1 = server + buscarVHcotDigital.imgDetalle1,
                                        imgDetalle2 = server + buscarVHcotDigital.imgDetalle2,
                                        imgDetalle3 = server + buscarVHcotDigital.imgDetalle3,
                                        texto1 = buscarVHcotDigital.texto1,
                                        pieFoto = buscarVHcotDigital.pieFoto,
                                        tituloDet1 = split1,
                                        palabraResaltada1 = buscarVHcotDigital.palabraResaltada1,
                                        tituloDet2 = split2,
                                        palabraResaltada2 = buscarVHcotDigital.palabraResaltada2,
                                        tituloDet3 = split3,
                                        palabraResaltada3 = buscarVHcotDigital.palabraResaltada3,
                                        cuerpoTitulo1 = buscarVHcotDigital.tituloCuerpo1,
                                        cuerpoTitulo2 = buscarVHcotDigital.tituloCuerpo3,
                                        cuerpoTitulo3 = buscarVHcotDigital.tituloCuerpo2,
                                        chevyStar = buscarVHcotDigital.chevyStar,
                                        chevyStarImg = chevyStarIMG,
                                        precioFinanciacion = buscar.precioFinanciacion,
                                        credito = buscar.credito,
                                        cuotas = buscar.cuotas,
                                        cuotaInicial = buscar.cuotaInicial,
                                        valorResidual = buscar.valorResidual,
                                        otrosValores = buscar.otrosValores,
                                        tasaInteres = buscar.tasaInteres,
                                        planes = planMatriz,
                                        notasCotizacion = vehiculosCota.observacion
                                    };

                                    #endregion

                                    #region armar PDF y convertirlo para enviarlo en el correo electronico

                                    ViewAsPdf something = new Rotativa.ViewAsPdf("CotizacionDigital", obj)
                                    {
                                        FileName = "CotizacionDigital_" + obj.vehiculo + "_" + DateTime.Now + ".pdf",
                                        SaveOnServerPath = path,
                                        PageSize = Size.Letter,
                                        PageOrientation = Orientation.Portrait,
                                        PageMargins = new Margins(00, 00, 00, 00),
                                    };

                                    byte[] applicationPDFData = something.BuildPdf(ControllerContext);
                                    FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
                                    fileStream.Write(applicationPDFData, 0, applicationPDFData.Length);
                                    fileStream.Close();

                                    #endregion
                                    //buscamos el habeas data del correo tercero. Si es true, la enviamos. Si no, no.
                                    int envio = envioCotizacionDigital(applicationPDFData, "CotizacionDigital" + buscarUltimaCot.cot_idserial + "_" + DateTime.Now.ToShortDateString() + ".pdf",
                                        obj);
                                    if (envio == 1)
                                    {
                                        variable = "1";
                                        enviados = enviados + 1;
                                        nombreenviados = nombreenviados + item.modelo_vehiculo.modvh_nombre + " " + item.anomodelo + ",";
                                    }
                                    else
                                    {
                                        variable = "2";
                                        nombrenoenviados = nombrenoenviados + item.modelo_vehiculo.modvh_nombre + " " + item.anomodelo + ",";
                                    }

                                    //return Json(variable, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                        else
                        {
                            #region variable buscar(toda la info a enviar al obj)

                            var buscar = new
                            {
                                id = buscar2.cot_idserial,
                                buscar2.tercero_id,
                                buscar2.asesor,
                                buscar2.telefeno_asesor,
                                buscar2.correo_asesor,
                                buscar2.prinom_tercero,
                                buscar2.vehiculo,
                                buscar2.anio,
                                buscar2.color,
                                buscar2.precio,
                                buscar2.matricula,
                                buscar2.soat,
                                buscar2.poliza,
                                buscar2.email_tercero,
                                buscar2.cot_idserial,
                                buscar2.apellido_tercero,
                                retoma = valoretoma1,
                                accesoriosCot = valorAccesorios,
                                precioFinanciacion,
                                credito,
                                cuotas,
                                cuotaInicial,
                                valorResidual,
                                otrosValores,
                                tasaInteres,
                            };

                            #endregion

                            #region variables PDF

                            string root = Server.MapPath("~/Pdf/");
                            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
                            string path = Path.Combine(root, pdfname);
                            path = Path.GetFullPath(path);
                            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

                            #endregion

                            #region chevystar

                            int? chevyStar = buscarVHcotDigital.chevyStar;
                            string chevyStarIMG = "";
                            string server = Request.Url.Scheme + "://" + Request.Url.Authority;

                            if (chevyStar == 1)
                            {
                                chevyStarIMG = server + "/Images/imgCotizacionDigital/incluyeChevyStar.png";
                            }
                            else if (chevyStar == 2)
                            {
                                chevyStarIMG = server + "/Images/imgCotizacionDigital/compatibleChevyStar.png";
                            }
                            else
                            {
                                chevyStarIMG = "";
                            }

                            #endregion

                            #region titulos que envio en el obj

                            string titulo1 = buscarVHcotDigital.tituloDet1;
                            string[] split1 = titulo1.Split(' ');

                            string titulo2 = buscarVHcotDigital.tituloDet2;
                            string[] split2 = titulo2.Split(' ');

                            string titulo3 = buscarVHcotDigital.tituloDet3;
                            string[] split3 = titulo3.Split(' ');

                            #endregion

                            #region obj

                            PDFmodel obj = new PDFmodel
                            {
                                idcotizacion = buscar.id,
                                idtercero=buscar.tercero_id,                                
                                nombreAsesor = buscar.asesor,
                                telefono_asesor = buscar.telefeno_asesor,
                                correo_asesor = buscar.correo_asesor,
                                nombreCompletoTercero = buscar2.prinom_tercero,
                                Idtercero= buscar2.tercero_id.ToString(),
                                vehiculo = buscar.vehiculo,
                                anioModelo = buscar.anio != null ? buscar.anio : 0,
                                color = buscar.color,
                                /*valorRetoma = buscar.retoma != null ? buscar.retoma : 0,
                                valorAccesorios = buscar.accesoriosCot != null ? buscar.accesoriosCot : 0,*/
                                valorRetoma = buscar.retoma,
                                valorAccesorios = buscar.accesoriosCot,
                                matricula = buscar.matricula,
                                soat = buscar.soat,
                                poliza = buscar.poliza,
                                imgPrincipal = server + buscarVHcotDigital.imgPrincipal,
                                imgDetalle1 = server + buscarVHcotDigital.imgDetalle1,
                                imgDetalle2 = server + buscarVHcotDigital.imgDetalle2,
                                imgDetalle3 = server + buscarVHcotDigital.imgDetalle3,
                                texto1 = buscarVHcotDigital.texto1,
                                pieFoto = buscarVHcotDigital.pieFoto,
                                tituloDet1 = split1,
                                palabraResaltada1 = buscarVHcotDigital.palabraResaltada1,
                                tituloDet2 = split2,
                                palabraResaltada2 = buscarVHcotDigital.palabraResaltada2,
                                tituloDet3 = split3,
                                palabraResaltada3 = buscarVHcotDigital.palabraResaltada3,
                                cuerpoTitulo1 = buscarVHcotDigital.tituloCuerpo1,
                                cuerpoTitulo2 = buscarVHcotDigital.tituloCuerpo3,
                                cuerpoTitulo3 = buscarVHcotDigital.tituloCuerpo2,
                                chevyStar = buscarVHcotDigital.chevyStar,
                                chevyStarImg = chevyStarIMG,
                                precioFinanciacion = buscar.precioFinanciacion,
                                credito = buscar.credito,
                                cuotas = buscar.cuotas,
                                cuotaInicial = buscar.cuotaInicial,
                                valorResidual = buscar.valorResidual,
                                otrosValores = buscar.otrosValores,
                                tasaInteres = buscar.tasaInteres,
                                planes = planMatriz
                            };

                            #endregion

                            #region armar PDF y convertirlo para enviarlo en el correo electronico

                            ViewAsPdf something = new Rotativa.ViewAsPdf("CotizacionDigital", obj)
                            {
                                FileName = "CotizacionDigital_" + obj.vehiculo + "_" + DateTime.Now + ".pdf",
                                SaveOnServerPath = path,
                                PageSize = Size.Letter,
                                PageOrientation = Orientation.Portrait,
                                PageMargins = new Margins(00, 00, 00, 00),

                            };
                            byte[] applicationPDFData = something.BuildPdf(ControllerContext);
                            FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
                            fileStream.Write(applicationPDFData, 0, applicationPDFData.Length);
                            fileStream.Close();

                            #endregion

                            int envio = envioCotizacionDigital(applicationPDFData, "CotizacionDigital" + buscarUltimaCot.cot_idserial + "_" + DateTime.Now.ToShortDateString() + ".pdf",
                                obj);
                            if (envio == 1)
                            {
                                variable = "1";
                                enviados = enviados + 1;
                                nombreenviados = nombreenviados + item.modelo_vehiculo.modvh_nombre + " " + item.anomodelo+",";
                            }
                            else
                            {
                                variable = "2";
                                nombrenoenviados = nombrenoenviados + item.modelo_vehiculo.modvh_nombre + " " + item.anomodelo+",";

                            }

                            //return Json(variable, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        exito = 0;
                        detalle = "Debes agregar al menos un Vehiculo.";
                    }
                    //return View();
                }
                if (numvehiculos == 0 ) {
                    exito = 0;
                    detalle = "Debes agregar al menos un Vehiculo. ";
                }
                else if (buscarVHcotDigital2 == null)
                {
                    exito = 0;
                    detalle = "El modelo del Vehiculo Seleccionado no tiene una cotización Digital parametrizada. ";
                }
                else if (enviados == 0)
                {
                    exito = 0;
                    detalle = ("No se enviaron las siguientes cotizaciones digitales: ")+ nombrenoenviados;
                }
                else  if(enviados>0 && enviados< numvehiculos)
                {
                    exito = 1;
                    detalle = detalle + ("Se enviaron las siguientes cotizaciones digitales: ") + nombreenviados + @" * ";
                    detalle = detalle + ("No se enviaron las siguientes cotizaciones digitales: ") + nombrenoenviados;
                }
                else if (enviados > 0 && enviados == numvehiculos)
                {
                    exito = 1;
                    detalle = detalle + ("Se enviaron las siguientes cotizaciones digitales: ") + nombreenviados + @" * ";
                    //detalle = detalle + "No se enviaron las siguientes cotizaciones digitales:" + nombrenoenviados;
                }
                #endregion
            }
            else
            {
                exito = 0;
                if (terceroCot.habdtautor_correo == false)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    detalle="El cliente no desea envío de correo";

                }
                else if (buscarUltimaCot.cot_envio_cotizacion == false)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    detalle = "El cliente no desea envío de correo";
                }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    detalle = "El cliente no desea envío de correo";
                }
            }

            //var data = new
            //{
            //    exito =1,
            //    detalle="OK"
            //};
            return Json(new { exito, detalle });
        }

        public int envioCotizacionDigital(byte[] pdf, string nombre, PDFmodel obj)
        {
            icb_cotizacion buscarUltimaCot = context.icb_cotizacion.Where(x => x.cot_idserial==obj.idcotizacion).FirstOrDefault();
            
            icb_terceros terceroCot = context.icb_terceros.FirstOrDefault(x => x.tercero_id == obj.idtercero);
            configuracion_envio_correos correoconfig = context.configuracion_envio_correos.Where(d => d.activo).FirstOrDefault();

            try
            {
                int aniocorreo = DateTime.Now.Year;
                icb_terceros user_destinatario = context.icb_terceros.Find(terceroCot.tercero_id);
                users user_remitente = context.users.Find(buscarUltimaCot.asesor);


                //MailAddress de = new MailAddress("noti@iceberg.com.co", "Notificación Iceberg");
                MailAddress de = new MailAddress(correoconfig.correo, correoconfig.nombre_remitente);

                MailAddress para = new MailAddress(user_destinatario.email_tercero,
                    user_destinatario.prinom_tercero + " " + user_destinatario.apellido_tercero);
                MailMessage mensaje = new MailMessage(de, para);

                //texto del mensaje de correo
                string titulocorreo = user_destinatario.prinom_tercero + " estás a un paso de estrenar tu chevrolet ";
                //var asuntocorreo = "<p>Hemos recibido una solicitud para reestablecer tu contraseña, por favor haz click en el siguiente boton</p><br />";
                string tituloDet1 = "";
                string tituloDet2 = "";
                string tituloDet3 = "";
                string server = Request.Url.Scheme + "://" + Request.Url.Authority;
                string imglayout1 = "";
                string imglayout2 = "";
                string imglayout3 = "";
                string imglayout4 = "";
                string imglayout5 = "";
                string imglayout6 = "";
                string imglayout7 = "";
                string imglayout8 = "";
                string imglayout9 = "";
                string imglayout10 = "";
                string imglayout11 = "";
                imglayout1 = server + "/Images/imgCotizacionDigital/sliceBlank_0_0.png";
                imglayout2 = server + "/Images/imgCotizacionDigital/sliceBlank_2_0.png";
                imglayout3 = server + "/Images/imgCotizacionDigital/sliceBlank_4_0.png";
                imglayout4 = server + "/Images/imgCotizacionDigital/sliceDatosCot_2_3.png";
                imglayout5 = server + "/Images/imgCotizacionDigital/sliceBlank_7_0.png";
                imglayout6 = server + "/Images/imgCotizacionDigital/sliceBlank_8_0.png";
                imglayout7 = server + "/Images/imgCotizacionDigital/sliceDatosContacto_1_2.png";
                imglayout8 = server + "/Images/imgCotizacionDigital/sliceBlank_10_0.png";
                imglayout9 = server + "/Images/imgCotizacionDigital/sliceBlank_11_0.png";
                imglayout10 = server + "/Images/imgCotizacionDigital/sliceBlank_12_0.png";
                imglayout11 = server + "/Images/imgCotizacionDigital/sliceBlank_13_0.png";

                string planito = "";
                string planitas = "";
                if (obj.planes != null && obj.planes.Count > 0)
                {
                    foreach (planmatriz item in obj.planes)
                    {
                        planito +=
                            "<label style='font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>NUMERO DE CUOTAS: </label><label style='font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>" +
                            item.plan.Count() + "</label><br />";
                        foreach (planes_pago item2 in item.plan)
                        {
                            planitas +=
                                "<label style='font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>" +
                                item2.plazo + " meses cuota apróx: </label><label style='font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>" + 
                                item2.valor.ToString("0,0", elGR) +
                                "</label><br />";
                        }
                    }
                }

                #region palabras resaltadas

                foreach (string item in obj.tituloDet1)
                {
                    if (item == obj.palabraResaltada1)
                    {
                        tituloDet1 +=
                            "<label style='font-family:LouisBoldItalic; font-size:23pt; color:#EEB700; text-align:center'> " +
                            item + "</label>";
                    }
                    else
                    {
                        tituloDet1 +=
                            "<label style='font-family:LouisBoldItalic; font-size:23pt; color:#4C5054; text-align:center'> " +
                            item + "</label>";
                    }
                };
                foreach (string item in obj.tituloDet2)
                {
                    if (item == obj.palabraResaltada2)
                    {
                        tituloDet2 +=
                            "<label style='font-family:LouisBoldItalic; font-size:23pt; color:#EEB700; text-align:center'> " +
                            item + "</label>";
                    }
                    else
                    {
                        tituloDet2 +=
                            "<label style='font-family:LouisBoldItalic; font-size:23pt; color:#4C5054; text-align:center'> " +
                            item + "</label>";
                    }
                };
                foreach (string item in obj.tituloDet3)
                {
                    if (item == obj.palabraResaltada3)
                    {
                        tituloDet3 +=
                            "<label style='font-family:LouisBoldItalic; font-size:23pt; color:#EEB700; text-align:center'> " +
                            item + "</label>";
                    }
                    else
                    {
                        tituloDet3 +=
                            "<label style='font-family:LouisBoldItalic; font-size:23pt; color:#4C5054; text-align:center'> " +
                            item + "</label>";
                    }
                };

                #endregion

                string html2 =
                    @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
								<html xmlns='http://www.w3.org/1999/xhtml'>
									<head>
										<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
				                        <title>Notificacion Iceberg Email</title>
				                        <style type='text/css'>
											@@font-face {
												font-family: 'LouisRegular';
												src: url(data:application/font-ttf;charset=utf-8;base64,AAEAAAAPAIAAAwBwRkZUTW4c45YAAVHAAAAAHEdERUYFEwdbAAEPbAAAACpHUE9TLmajVAABD/AAAEHQR1NVQtvl3zQAAQ+YAAAAWE9TLzKvmWhiAAABeAAAAGBjbWFw5boPAgAAC7AAAANmZ2FzcP//AAMAAQ9kAAAACGdseWaJB62XAAAUCAAA46hoZWFk+PnEqgAAAPwAAAA2aGhlYQgBBYMAAAE0AAAAJGhtdHhuDnKwAAAB2AAACdhsb2Nh5YqtPgAADxgAAATubWF4cALAAGMAAAFYAAAAIG5hbWUk3eqLAAD3sAAABbVwb3N0o4DywAAA/WgAABH8AAEAAAABTMyq6DsWXw889QALA+gAAAAAyvGaCAAAAADK8ZoI/5z+7ARbA/0AAAAIAAIAAAAAAAAAAQAABAf/DAAABGT/nP+cBFsAAQAAAAAAAAAAAAAAAAAAAnYAAQAAAnYAYAAIAAAAAAACAAAAAQABAAAAQAAAAAAAAAACAb8BkAAFAAACvAKKAAAAjAK8AooAAAHdADIA+gAAAgAAAAAAAAAAAIAAAq9QACBKAAAAAAAAAABwc3kAAEAADfsCAu7/BgAABAcA9CAAAJcAAAAAAjAC7gAAACAAAgJEACgAAAAAAU0AAADcAAAA3AAAAP4AQgFqADICJgARAiYAKQImAAQChAAoAL4AMgFaAD8BWgAKAboAGgJKAB4A5AAoAb4AKADkADMBqAASAlgAQQF8ABYCKQA2AjgAJAJLACICQwBAAk0AQQIBABwCSAAvAk0AOAD1ADsA9QAyAiYANAImAC8CJgA0Ae8AIAOfADYCVAAMAl8ATAJUAEECagBMAisATAImAEwCawBBAokATAECAEwCIQAtAnMATAIjAEwDDABMAogATAJoAEECWwBMAmgAQQJeAEwCSAArAjAAFgJ2AEgCUAAMA3AAGgJIAAkCRgADAkoALQF2AFUBqAASAXYADwHcAB4BvgAAAjYAZQIcADICNABJAgsANwI0ADcCGAA3AWkAFAI0ADcCQwBJAPYARgD//8QCDgBJAPYASQNTAEkCQwBJAhgANwI0AEkCNAA3AaEASQHpACYBeQAUAkEARgH0AAkC9AARAgYADAH8AAkB8gAjAagAIAE6AG4BqAAMAh8AKgDcAAAA/gBCAg4ANwImAAkCJgACAToAbgINADIB9ABiA1oAPAFYACgCEgAcAkQAKAHqADAB9ABfATMAKAImAAwCRAAoAkQAKAH0AJsCRgBJAloANwDkADMB9ACeAkQAKAFZACoCEgAyAkQAKAJEACgCRAAoAfIAJgJUAAwCUAAMAlAADAJUAAwCVAAMAlQADANv//gCVABBAisATAIrAEwCKwBMAisATAEC/8EBAgBMAQL/vAEC/+kCmQAPAogATAJoAEECaABBAmgAQQJoAEECaABBAi4AJAJoAEECdgBIAnYASAJ2AEgCdgBIAkYAAwJbAEwCWQBJAhwAMgIcADICHAAyAhwAMgIcADICHAAyA1QAMgIOADcCFgA3AhYANwIWADcCFgA3APb/uAD2AEkA9v/GAPb/4wIYADcCQwBJAhgANwIYADcCGAA3AhgANwIYADcCSgAeAhgANwJBAEYCQQBGAkEARgJBAEYB/AAJAjYASwH8AAkCVAAMAhwAMgJUAAwCHAAyAlQADAIcADICVABBAgsANwJUAEECCwA3AlQAQQILADcCVABBAgsANwJqAEwChAA3ApkADwI/ADcCKwBMAhgANwIrAEwCGAA3AisATAIYADcCKwBMAhgANwIrAEwCGAA3AmsAQQI0ADcCawBBAjQANwJrAEECNAA3AmsAQQI0ADcCiQBMAkMASQKZ//YCTv/2AQL/vAD2/7UBAv/mAPb/4AEC/9cA9v/RAQL//wD2//gBAgBKAPYASQMZAEwB9QBGAiEALQD//7sCcwBMAg4ASQIOAEkCIwBMAPYASQIjAEwA9gA+AiMATAFGAEkCIwBMAXcASQJTAA8BYgAPAogATAJDAEkCiABMAkMASQKIAEwCQwBJAp8ACgKIAEwCQwBJAmgAQQIYADcCaABBAhgANwJoAEECGAA3A1MAQQNhADcCXgBMAaEASQJeAEwBoQBEAl4ATAGhACUCSAArAekAJgJIACsB6QAmAkgAKwHpACYCSAArAekAJgIxABYBeQAUAjEAFgHbABQCMQAWAYMAHgJ2AEgCQQBGAnYASAJBAEYCdgBIAkEARgJ2AEgCQQBGAnYASAJBAEYCdgBIAkEARgNwABoC9AARAkYAAwH8AAkCRgADAkoALQHyACMCSgAtAfIAIwJKAC0B8gAjATIAFAJEACgA///EAfQANQH0ADUB9ABQAfQAwgH0AIkB9ACKAfQAMgH0AFUCRAAoAisATAIrAEwC3AAWAhkATAJPAEECSAArAQIATAEC/+kCIQAtA7cACAOhAEwC2gAWAncATAKIAEwCOQAQAoIATAJUAAwCWwBMAl8ATAIZAEwC6AAMAisATAPEAAsCSQApAogATAKIAEwCdwBMApYACAMMAEwCiQBMAmgAQQKBAEwCWwBMAlQAQQIwABYCOQAQAwQAMAJIAAkCtgBMAm8APANgAEwDlQBMAv4AFgMoAEwCZQBMAk8AJANfAEwCXgAZAhwAMgIbADoCDgBJAckASQJpAAgCGAA3AzAAFAHpACUCNwBJAjcASQIlAEkCNgAIApQASQI4AEkCGAA3AjEASQI0AEkCCwA3Ad4AFAH8AAkCogA3AgYADAJPAEkCEgAsAuwASQMNAEkCfgAUAsYASQIYAEkCDQArAtcASQIJABICGAA3AhgANwJO//YByQBJAg0AKwHpACYA9gBGAPb/4wD//8QDGwAIAxQASQJO//YCJQBJAjcASQH8AAkCMABJA1QAPALDADcC2QAQAmEADAM6AEwC4ABJAooABAIwAAcDhABMAwwASQLMADgCagA2A8gATANAAEkCTwAbAdsAEgLyADwCsQBAAmgAQQIYADcCiQAMAiEACQKJAAwCIf/+BGQAQQQMADcCaABBAhgANwNUADwCwwA3A1QAPALDADcCZABBAhIANwJCABYB9ABVAfQANQH0AFUB9ABQAfQAMwNBACQDQQAjAsEATAJqAEkCrQAWAkMADAJbAEwCNABJAhkATAHJAEkCSAAPAfAAEgJuAEwCAQA6A+cACwNDABQCTwApAekAJQKfAEwCPQBJAosATAJDAEkCqQAKAkEADAMEAAoClQAUAr4ATAJZAEkDIgBMAqgASQPKAEwDLQBJArMAQQJBADcCVABBAgsANwIwABYB3gAUAkYAAwH0AAkCRgADAfQACQJ1AAkCIwAMAxsAFgKRABQCpAA8AjMALAKDADwCJgAsAm8ATAISAEkC6gAFAoQAAgLqAAUChAACAQIATAPEAAsDMAAUAoAATAIqAEkCzwAIAmkACAKJAEwCOABJAsIATAJrAEkCbwA8AhIALANFAEwCxwBJAPYASQJUAAwCHAAyAlQADAIcADIDb//4A1QAMgIrAEwCGAA3AmkAQQIYADcCaQBBAhgANwPEAAsDMAAUAk8AKQHpACUCQgArAgoAGgKIAEwCNwBJAogATAI3AEkCaABBAhgANwJoAEECGAA3AmgAQQIYADcCTwAkAg0AKwI5ABAB/AAJAjkAEAH8AAkCOQAQAfwACQJvADwCEgAsAhkATAHJAEkDKABMAsYASQJIAA8B8AASAmcACQIdAAwCUgAOAgYADAJAAC0CuAAtAOQALQDjACoA5AAoAawALQGrACoBrAAoAfIAKAH8AC0BNQA8AqwAMwOMACMBNgAcATYAMgFN/5wCJgAAAlIAHwJKABICRAAoAkQAKAJEACgCRAAoAkQAKAImACACRAAoAkQAKAJEACgCRAAoAiYAJQImADICJgAxAkQAKAOzAAACPAAUAlwAFAH0ALwAvwAmAfQAUANaADwDWgA8AfQAAAAAAAMAAAADAAAAHAABAAAAAAFcAAMAAQAAABwABAFAAAAATABAAAUADAAAAA0AfgCjAKwBfwGSAjcCxwLdA8AE/yAUIBogHiAiICYgMCA6IEQgrCEgISIhJiICIgYiDyISIhoiHiIrIkgiYCJlJcr4//sC//8AAAAAAA0AIACgAKUArgGSAjcCxgLYA8AEACATIBggHCAgICYgMCA5IEQgrCEgISIhJiICIgYiDyIRIhoiHiIrIkgiYCJkJcr4//sB//8AAf/2/+T/w//C/8H/r/8L/n3+bf2L/UziOeI24jXiNOIx4ijiIOIX4bDhPeE84TngXuBb4FPgUuBL4EjgPOAg4AngBtyiCW4HbQABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAGAgoAAAAAAQAAAQAAAAAAAAAAAAAAAAAAAAEAAgAAAAAAAAADAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEAAAAAAAQABQAGAAcACAAJAAoACwAMAA0ADgAPABAAEQASABMAFAAVABYAFwAYABkAGgAbABwAHQAeAB8AIAAhACIAIwAkACUAJgAnACgAKQAqACsALAAtAC4ALwAwADEAMgAzADQANQA2ADcAOAA5ADoAOwA8AD0APgA/AEAAQQBCAEMARABFAEYARwBIAEkASgBLAEwATQBOAE8AUABRAFIAUwBUAFUAVgBXAFgAWQBaAFsAXABdAF4AXwBgAGEAYgAAAIUAhgCIAIoAkgCXAJ0AogChAKMApQCkAKYAqACqAKkAqwCsAK4ArQCvALAAsgC0ALMAtQC3ALYAuwC6ALwAvQJUAHEAZQBmAGkCVgB3AKAAbwBrAl4AdQBqAmkAhwCZAmYAcgJqAmsAZwB2AmACYwJiAUsCZwBsAHsAAACnALkAgABkAG4CZQFBAmgCYQBtAHwCVwBjAIEAhACWARMBFAJMAk0CUQJSAk4CTwC4AmwAwAE5AlsCXAJZAloCbgJvAlUAeAJQAlMCWACDAIsAggCMAIkAjgCPAJAAjQCUAJUCbQCTAJsAnACaAPIBQwFJAHABRQFGAUcAeQFKAUgBRAAAAAAAKgAqACoAKgAqAD4AUgCCAM4BHgFmAXQBigGgAb4B1AHqAfgCBAISAkwCXgKeAtYC8gMqA3gDjgPQBBwELgRKBF4EcgSGBLYFKAVEBYIFwgXoBf4GEgZWBm4GegaeBrwGzAbuBwwHTgd4B8QH/gg4CEoIdAiICKgIwgjaCPIJBAkSCSQJOAlGCVQJhgnGCgIKRAp8CqAK9AscCy4LTgtmC3ILpgvQDAgMSgyKDKYM3A0CDSoNQA1gDXoNnA20DeQN8g4kDkwOTA5gDp4O1A74DwwPZg94D9IQAhAcEEYQfhCMEKoQxhDwERoRKBFMEWgRdBGaEcQR6hIEEi4SWBKCErIS1BL2ExwTVhN8E7AT0hQwFE4UbBSOFLAUxBTYFPAVCBU4FXQVvhYIFlYWuBcGFyAXdBekF9QYCBg+GFwYiBjGGQAZOhl4GcoaCBpSGqIa/Bs6G3gbuhv8HBAcJBw8HFQcpBzsHSwdbB2wHggeTB5mHrQe5B8UH0gffB+kH+YgEiA0IG4gmiDeIQ4hVCGcId4iKiJyIrgi+iNGI44jwCQUJEQkjiSsJOolEiVaJXYltCXiJi4mUCaUJuInQCeWJ/woRiigKPYpXCl+KbIp2CoIKjQqYCp0KogqpirEKuQrCiscKygrUit+K64r0iwCLCwsRCxaLG4skCyuLNAs8C0GLRgtNC1MLXAtoC3QLgwuNC5oLqQu0C8EL04vji/iMCwwfDDCMPYxPjF+MaAx7DIcMmAyiDLIMwYzTDOOM+Y0OjSANMI05jUeNTw1dDWONbw2BDZMNnw2rDboNyI3ZDekN9o4EDhQOIw4uDjkOQg5NDlYOXY5lDmwOc458DoSOjA6Wjp0OoY6mDqwOr463Dr6OyA7NDteO3w7njvQO+Y8KjxkPHA8iDysPOw9ID1EPXg9nD3QPeg+BD4yPnA+gD6oPr4/DD9GP2I/kj/AP+JABEAcQF5AcECaQNpA7EEOQV5BeEGQQbJBykHmQhRCRkJwQrRC9kMwQ2JDpEPgQ/BEGkRSRJ5E1ETyRSRFUEVwRZJFpkXeRfBGMkZuRoBGokbwRwpHIkdER1xHeEekR9RH/EgySHJIqkjoSSpJaEl+SbRJ6kn8ShRKNEpuSp5KzksASyZLWktwS6hL2EwKTDpMiEzGTOhNBk0yTVpNjk3CTgBOOE6eTwJPNE9qT8ZQElA0UFhQhlC4URZRalHIUiJSZFKeUuZTJlNkU6BTxlPWU/hUClQcVD5U0lVaVYxVwFXyViJWVlakVrZWyFbiVvpXLFdeV7JYAlhOWJZYyFj4WSxZXlmSWcZZ9lokWkBaWlp2Wo5axFr4W0pbnFvuXDpcUFxmXH5clly0XNJc8l0QXSxdSF1uXZRdwF3qXgxeLl6EXsxfJl9yX35f3mA8YHxgtmDcYQJhJmFIYWZhgGGmYcxh8mIYYiRiUGKUYrpi+GMaY2pjkmPaZCBkWGSqZO5lSGWgZeRmJmZQZnpmnmbEZu5nGmdoZ6xn9GgwaIRozGkcaV5phmmuadpqBmo0amJqkGq+atJq5mska15rhmusa9hsAGwgbEBsTmxcbHJsiGyebMJs5m0KbSBtPm1KbWBt1G3kbfRuBG5EbopurG7WbwBvKm9Ub35vjG+2b+BwCnA0cFRwbnCIcLJwsnDWcPhxEHEqcUJxkHHUcdQAAAAFACgAAAIcAu4AAwAGAAkADAAPAAABESEREwMRGwERJRsBCwEhAhz+DN2r5av+jqqqqqkBUQLu/RIC7v6IARn9zgEZ/ucCMi3+6QEX/or+7AAAAAACAEIAAAC8Au4AAwAHAAA3IwMzEyM1M6tYDnQDenrnAgf9En8AAAAAAgAyAegBRwLuAAMABwAAEzMDIxMzAyMyaRVUrGkVVALu/voBBv76AAIAEQAAAhUCvAAbAB8AABMjNTM3MwczNzMHMxUjBzMVIwcjNyMHIzcjNTsBNyMHj210FEsUhRRLFGRrE210FEsUhRRLFGRr0BOFEwG5RL+/v79EtkS/v7+/RLa2AAAAAAMAKf+6AfYDKgAkACsAMQAAARUeAQcjNCYnFRcWFRQGBxUjNS4BJzMeARc1Jy4DNTQ2NzUTNCYnFT4BAxQXNQ4BAS1bbANkMS8Ou3FYMmFvAmQCPS8XKTA2GWlWmDYwLjj1XTIrAypaBWdbKj8GzAQ8klFmB2JjB3JTKkMH4wgPGCtCLUxcB1r9tCkvEdEGNgFtQyC8BTMABQAE//YCIgL4AAMADwAdACkANwAAMyMBMwUVFBYyNj0BNCYiBgc1NDYzMhYdARQGIyImARUUFjI2PQE0JiIGBzU0NjMyFh0BFAYjIiZ/QAFpQP5hIjwjIzwiRUs6O0tLOzpLAVgiPCMjPCJFSzo7S0s7OksC7ooUKSsrKRQpKys9FENRUUMUQ1JS/pIUKSsrKRQpKys9FENRUUMUQ1JSAAMAKP/2AoAC+AAcACQALgAAARc2NzMGBxcVIycGIyImNTQ3LgI1NDYzMhYVFAMyNycGFRQWAxQXNjU0JiMiBgE+hRUJYwwweH85U2JnhIUaGRpsTVZpxkQ3qVRHE0JuLSktLQF/mjdRam+KCkROdGJ5USIlRCNXXVtOdP55N8Y2Tzk/AfAyTjlOIi4xAAAAAAEAMgHoAJYC7gADAAATMwMjMmQVTwLu/voAAAABAD//SgFQA1MACQAAExA3MwYREBcjJj+2W6enW7YBTgEb6vL+7f7u8uoAAAEACv9KARsDUwAJAAABEAcjNhEQJzMWARu2W6enW7YBT/7l6vIBEwES8uoAAQAaAXsBoALuAA4AABMzBzcXBxcHJwcnNyc3F7RSC4wZkmFDT09DYZIZjALulzlOJHMwf38wcyROOQAAAQAeABQCLAJEAAsAAAEzFSMVIzUjNTM1MwFS2tpa2tpaAVhY7OxY7AAAAAABACj/XQC3AIMACwAAMzUzFRQGBzU2NTQjMoVDTEsLg21HaAoyGE0MAAAAAAEAKAFaAZYBsgADAAABITUhAZb+kgFuAVpYAAABADMAAACxAIMAAwAAMyM1M7F+foMAAAABABL/swGWAyQAAwAAFyMBM3NhASRgTQNxAAAAAgBB//YCFwL4AA8AIwAAJRE0JyYiBwYVERQXFjI3NiURNDc+ATIWFxYVERQHDgEiJicmAa0LH64fCwsfrh8L/pQNGHqYehgNDRh6mHoYDewBFisbSUkbK/7qKxtJSRskASQ3JEVPT0UkN/7cNyRFT09FJAABABYAAAEmAu4ABwAAEyM1NzMRIxEYAq1jaQHweYX9EgJvAAABADYAAAH3AvgALAAAMzU0PgI3Njc+AzU0JiMiBwYdASM1NDc+ATMyFhUUDgIHBgcGBwYHIRU2GzgyKAUKJCY0GDw0Sx4LaQ4XckZefxs+KSsIBE8bKAMBUCg6X0wxIAQIHSE4OB4sMjgWJBERMSI7SGpYKktHJiMHA0AhMi9lAAEAJP/2AggC+AAnAAATNTMyNjU0JiMiBhUjNDYzMhYVFAceARUUBiMiJjUzHgEzMjY1NCYju0NHR0AzOz5pf2NffWQ6PYxmb4NpAUdBPExQUAFdXUU2MDI9L1Z3alpqOxxkOWCAdl82O0g+OEUAAAACACIAAAI1AvgACgAOAAABMxEzFSMVIzUhNRczESMBOpBra2n+wWzTAgL4/idewcFfAQFsAAAAAAEAQP/2AgwC7gAlAAABIgYHJxEhFSEVNjMyFhcWHQEUBw4BIyImNTMUFjMyNzY9ATQnJgEtKEMQZwGY/tE4UU5oEQgNGHdNYYJqQThXHQsHFgGbIhoBAY5nuzJMRyMvMzQnRVB/Zj1BSBssJisVSQACAEH/9gIVAvgAIgA0AAABMhYXIyYjIgYdATM2MzIWFxYdARQHDgEjIiYnJjURNDc+ARMyNjc2PQE0JyYjIgYdARQXFgEpXn0HbA9qO0ABNmhHZxMKChZ5UEx6GA0NGHlNKj8NCQgXVjpRCx8C+GZYWU1EYkhGQiUvJzMfSVRPRSQ/ARw3JEVP/WQpJRgpFiYaRUw+ESsbSQAAAAABABwAAAHhAu4ACQAAMyM2EyE1IRUGAvZyFtf+qwHFZnjzAZZlX8D+xAAAAAMAL//2AhkC+AAVAB8AKQAAEjIWFRQGBx4BFRQGIiY1NDY3LgE1NAE0JiIGFRQWMjYBFBYyNjU0JiIGwMh+NTQ9P4rWij89NDUBbUiGSEqCSv78QXBBQ2xDAvhpWzFXHRxkOWF/f2E5ZBwdVzFb/k00Sko0P0VFAZUxQkIxMDk5AAAAAAIAOP/2AgwC+AAiADQAAAUiJiczFjMyNj0BIwYjIiYnJj0BNDc+ATMyFhcWFREUBw4BAyIGBwYdARQXFjMyNj0BNCcmASRefQdsD2o7QAE2aEdnEwoKFnlQTHoYDQ0YeU0qPw0JCBdWOlELHwpmWFlNRGJIRkIlLyczH0lUT0UkP/7kNyRFTwKcKSUYKRYmGkVMPhErG0kAAgA7AAAAuQIiAAMABwAAMyM1MxEjNTO5fn5+foMBHIMAAAAAAgAy/10AwQIiAAsADwAAMzUzFRQGBzU2NTQjEyM1MzyFQ0xLC0l+foNtR2gKMhhNDAGfgwAAAAABADQAKwHyAi0ABgAALQE1JRUNAQHy/kIBvv6WAWorzmbOYKGhAAAAAgAvAJcB9wHBAAMABwAAASE1IREhNSEB9/44Acj+OAHIAWlY/tZYAAEANAArAfICLQAGAAATBRUFNS0BNAG+/kIBav6WAi3OZs5goaEAAAACACAAAAHIAvgAGwAfAAAlIyY+AzU0JiMiBh0BIzU0NjMyFhUUDgMTIzUzARNeBiA1Nyg9MjM+Y3lbXXcpOTciF3p65z5eOC04IiosPS8SEllzYFYtRzIzT/7mfwAAAAACADb/agNgAu4ADABLAAABBwYzMjY/ATYmIyIGEyImPwE+ATIXNzMDBhYXFjY3NiYrASIGDwEGHgE7ATI2NxcOASsBIi4CPwE+AjsBMh4CBw4CJy4BJwYBdw8SWCtJBg4HKTAvNiJPUg0QDmmeJhFDMwgOHDdMAwWkiheGtBMOC0OAVhQ+fCUWLopCFE2BXywKDg52pmkXT4BkNQMDLF0+JjYKOgFbbHZMLWU0SkL+j21db2VsQTT+mCY0Awa+cZ6RpaiEZ5FDHhc2GiEuWpJdhIW0UChTj2FfoWkEAiwjTQAAAAACAAwAAAJIAu4AAwALAAABAyMDFyMHIxMzEyMBglcEVczoPW3SmNJtAUkBPP7EZeQC7v0SAAAAAAMATAAAAjAC7gARABwAKAAAMxEzMhceARUUBx4BFRQGBwYjAyMVMzI3NjU0JyYDIxUzMjc+ATU0JyZM10UoQEhcODxLQShCB32GMho9Rxs5dHMqGR4lQhcC7gwUXUZpOBdhOEhtFw4BT+kOH0dMHgsBOdMLDTckQRcIAAABAEH/9gIoAvgAKwAAJTY1MwYHDgErASImJyY1ETQ3PgE7ATIWFxYXIyYnJisBIgcGFREUFxY7ATIBrw5rAREbekkISHkcEhIceUgISXobEQFrAgwgVwhXIA4OIFcIV6IhMUMsREtMQy1FAQBFLUNMTEMqPzAcRkYfNv8ANh9FAAIATAAAAikC7gALABcAADczMjc2PQE0JyYrARMjETMyFxYdARQHBrZ6YSAODiBhenrk5Kw7EhI7Z0YfNew1H0b9eQLuji1G7EYtjgABAEwAAAIDAu4ACwAAMxEhFSEVIRUhFSEVTAG3/rMBE/7tAU0C7mfTZ+ZnAAEATAAAAgMC7gAJAAAzESEVIRUhFSERTAG3/rMBE/7tAu5n02f+swABAEH/9gIoAvgALgAAJTMyNj0BIzUzESMnBisBIiYnJjURNDc+ATsBMhYXFhcjJicuASsBIgcGFREUFxYBMAg6TYvzMxk+awdIdRsSEhx5SAhKexkPA2wDCxBAJghXIA4PIFxMVRpk/oVLVUtELUUBAEUtQ0xMQyk3KBokI0YfNv8ANCFGAAAAAQBMAAACPQLuAAsAADMRMxEhETMRIxEhEUxqAR1qav7jAu7+zAE0/RIBU/6tAAAAAQBMAAAAtgLuAAMAADMRMxFMagLu/RIAAQAt//YB2QLuABUAABMVFBcWMjc2NREzERQHDgEiJicmPQGXEh92HxJqFRllhmUZFQEPMS4fNTUfLwIP/fZBMjlCQjkyQCwAAQBMAAACbALuAA4AACEjAwcVIxEzERc2NxMzAwJsfNBqamoBExD8gu0BfoT6Au7+ogEYFAEz/uUAAAAAAQBMAAACBQLuAAUAADMRMxEhFUxqAU8C7v15ZwAAAAABAEwAAALAAu4AEQAAAQMjAyMWFREjETMbATMRIxE3AlqrUqsBAWZ/u7t/ZgECNv5cAaQhGf4EAu7+NAHM/RIB/DoAAAEATAAAAjwC7gAPAAATIxYVESMRMwEzJjURMxEjsgEBZn4BDAEBZn8COSEZ/gEC7v3HIRkB//0SAAAAAAIAQf/2AicC+AATACsAACURNCcmKwEiBwYVERQXFjsBMjc2JRE0Nz4BOwEyFhcWFREUBw4BKwEiJicmAb0OIFcIVyAODiBXCFcgDv6EEhx5SAhIeRwSEhx5SAhIeRwS9wEANx9FRR83/wA3H0VFHzcBAEUtQ0xMQy1F/wBFLUNMTEMtAAIATAAAAisC7gAKABkAABMzMjc2NTQnJisBEyMRIxEzMhceARUUBgcGtng6HTw8HTp4enpq5EYnQkxMQicBihEiSk0iEf6c/t0C7g8Yc01McRgPAAIAQf+tAmwC+AAeADIAADcRNDc+ATsBMhYXFhURFAcGBxYXFS4BJwYrASImJyYlETQnJisBIgcGFREUFxY7ATI3NkESHHlICEh5HBISGTM3bGB9MhQRCEh5HBIBfA4gVwhXIA4OIFcIVyAO9wEARS1DTExDLUX/AEUtPCQUBV8DIiYCTEMtRQEANx9FRR83/wA3H0VFHwAAAgBMAAACRQLuABoAJAAAJRUjJi8BLgErAREjETMyFx4BFRQGBxUWHwEWASMVMzI2NTQnJgJFdBgGDAQ9O3Vq1DImTFU7N2EICwX+/GZrREpQGQICGUGJNCv+vgLuCBFhTThZGQEibIxFAmvgQzdNEwYAAAABACv/9gIWAvgAJwAAASM0JiMiBhUUHwEWFRQGIyImJzMeATMyNjU0Ji8BLgM1NDYzMhYCD2tAQEM4YkTHh2hzhwJsAk87PUxAN0MrNDgbgmVrgQIZNkg6K00hF0KiYHOBYjVNPjgwNRQYEBswSTJbaXQAAAEAFgAAAhoC7gAHAAATIRUjESMRIxYCBM1qzQLuZ/15AocAAAEASP/2Ai4C7gAZAAAlETMRFAcOASsBIiYnJjURMxEUFxY7ATI3NgHEahIceUgISHkcEmoOIFcIVyAO9wH3/glFLUNMTEMtRQH3/gk3H0VFHwAAAAEADAAAAkQC7gAHAAAbATMTMwMjA3urAq1v1ozWAu79iQJ3/RIC7gABABoAAANWAu4ADwAAATMTMxMzAyMDIwMjAzMTMwF6e4ACbnGYi3sCeYyXcG4CAu79mAJo/RICOf3HAu79mAAAAQAJAAACPwLuAAsAADMjEwMzGwEzAxMjA4J538h6iop6yN95ogGGAWj++wEF/pj+egEiAAEAAwAAAkMC7gAJAAABEzMDESMRAzMTASSneOtq63inAbcBN/5W/rwBRAGq/skAAAEALQAAAhsC7gAJAAATNSEVASEVITUBOwHa/pkBbf4SAWQCh2dd/dZnXgIpAAAAAAEAVf9CAWcDWwAHAAAXESEVIxEzFVUBEq6uvgQZSfx5SQAAAAEAEv+zAZYDJAADAAAFIwEzAZZh/t1gTQNxAAABAA//QgEhA1sABwAAAREhNTMRIzUBIf7urq4DW/vnSQOHSQABAB4BkAG+AvgABgAAGwEzEyMLAR6kWKRfcXEBkAFo/pgBBv76AAAAAQAA/3cBvv/BAAMAAAUhNSEBvv5CAb6JSgAAAAEAZQKRAVkDRQADAAABIyczAVlHrX4CkbQAAAACADL/9gHWAjoAFwAiAAATIz4BMzIVESMnIwYjIiY1NDY7ATU0IyIDFBYzMjY9ASMiBrBoD2dSxkcMATheVWVqXXljSTAyMjBIeC03AaBHU8j+jkhSY1dPYRtk/tItM1EqPS8AAgBJ//YB/QMRABMAKwAAExUUFx4BMzI3Nj0BNCcmIyIGBwYnMzYzMhYXFh0BFAcOASMiJicjByMRMxWtCA07KlAZCQoXTCQ9EQ0BATRaRmASCgkSZUcuUBkBCktkAUVXJBooMkwaK2YrHkQtIxyESEtEIjloNyRJTikjQgMR5QABADf/9gHgAjoAJwAAJTY3MwYHDgEjIiYnJj0BNDc+ATMyFhcWFyMmJyYjIgcGHQEUFxYzMgFxBQNmBQcWaUhHaRYPDxZpR0hpFggFZgMGGUxLGgwMGktMlgwOHxY/RkdALTtmOy1AR0Y/GSYVD0BCHDNiMxxCAAACADf/9gHrAxEAEwArAAAlNTQnLgEjIgcGHQEUFxYzMjY3NgMyFzMmPQEzESMnIwYjIiYnJj0BNDc+AQGHCA07KlAZCQoXTCQ9EQ2JVDUBAWRLDQE2Y0ZgEgoJEmXrVyQaKDJMGitmKx5ELSMcAXg6IRnX/O9JU0tEIjloNyRJTgAAAAACADf/9gHiAjoAGwAjAAASMhYXFh0BIRUUFjMyNjczDgEjIiYnJj0BNDc2FwYHMyYnJiLGjmkWD/65PTQoOQxnEXFSR2kWDw8WSwsB5AELGpgCOkdALTtKIkFNLShVW0dALTtmOy1AVhooKBpCAAAAAQAUAAABZAMcABcAABMRIxEjNTM1NDYzMhcVJiMiBh0BMxUjFtFkWVlpXBoYFBc3MYSFAQGW/moB0GAhZ2QDYAMvPSBgIQAAAgA3/xIB6wI6ABMAOQAAJTU0Jy4BIyIHBh0BFBcWMzI2NzYDMjc2PQE3IwYjIiYnJj0BNDc+ATMyFhczNzMRFAcOASMiJiczFgGHCA07KlAZCQoXTCQ9EQ1xSxoMAQE0WkZgEgoJEmVHLlAZAQpLDxZqR1NxEWkY9U0kGigyTBorXCseRC0jHP6kQxwzDDpIS0QiOV43JElOKSNC/dE7LUBHV1NMAAAAAQBJAAAB/QMRABkAACEjETQnJiMiBgcGFREjETMVBzM2MzIWFxYVAf1kChdMJD0RDWRkAQE0XkVdEgoBTSseRC0jHCn+uwMR5TpIS0QiOQACAEYAAACwAvgAAwAHAAAzETMRAzUzFUlkZ2oCMP3QAodxcQAC/8T/EwC5AvgADQARAAAXETMRFAYjIic1FjMyNgM1MxVSZGlcGRQYDjcxA2ohAlH9rmdkAmACLwLlcXEAAAAAAQBJAAACBgMRAAsAADcVIxEzETczBxMjA61kZMuCx9N4otnZAxH+R9jK/poBGwAAAQBJAAAArQMRAAMAADMRMxFJZAMR/O8AAQBJAAADDQI6ACIAAAERIxE0IyIGFREjETMXMzYzMhc2MzIWFxYVESMRNCcmIyIGAd1jXS1DZEkMAS9XYTY3YUdZEAlkBxNCLUMBU/6tAViCUDf+rQIwO0VaWklCJTj+rgFPMBlCUAABAEkAAAH9AjoAGAAAATIWFxYVESMRNCcmIyIGBwYVESMRMxczNgE/RV0SCmQKF0wkPRENZEsNATYCOktEIjn+sAFNKx5ELSMcKf67AjBJUwAAAAACADf/9gHhAjoAEwAjAAABFRQHDgEiJicmPQE0Nz4BMhYXFgc1NCcmIgcGHQEUFxYyNzYB4Q8WaY5pFg8PFmmOaRYPZAwalhoMDBqWGgwBS2Y7LUBHR0AtO2Y7LUBHR0Atn2IzHEJCHDNiMxxCQhwAAgBJ/wwB/QI6ABMAKwAAExUUFx4BMzI3Nj0BNCcmIyIGBwYTIicjFh0BIxEzFzM2MzIWFxYdARQHDgGtCA07KlAZCQoXTCQ9EQ2JVDUBAWRLDQE2Y0ZgEgoJEmUBRVckGigyTBorZiseRC0jHP6IOiEZ6gMkSVNLRCI5aDckSU4AAAAAAgA3/wwB6wI6ABMAKwAAJTU0Jy4BIyIHBh0BFBcWMzI2NzYXIwYjIiYnJj0BNDc+ATMyFhczNzMRIzUBhwgNOypQGQkKF0wkPRENAQE0WkZgEgoJEmVHLlAZAQpLZOtXJBooMkwaK2YrHkQtIxyESEtEIjloNyRJTikjQvzc+AABAEkAAAGMAjoADwAAAQcmIyIGFREjETMXMzYzMgGMAyopOk9kSg4BNmYmAi9mEFZD/sACMFFbAAEAJv/2Ab8COgAkAAA3LgM1NDYzMhYXIyYjIgYUFhceARUUBiMiJiczFjMyNjc2JuE3SB4KZ1JRaQpmFEomMS08WWFtVFlyDWYXWSszAQE29w0qMiYWR1dPTkUmPCIOFU5NTlxbVVYpIyQmAAEAFP/9AVYCvAAYAAAlBwYjIicmPQEjNTM1MxUzFSMVFBceATMyAVYBFRGgGwdZWWSEhAYIKScVX18DhB81+2CMjGD8LBYdGAAAAAABAEb/9gH4AjAAGAAABSImJyY1ETMRFBcWMzI2NzY1ETMRIycjBgEDRVwSCmQKF0skPBENZEsNATYKS0QiOQFQ/rMrHkQtIxwpAUX90ElTAAEACQAAAesCMAAHAAABAyMDMxMzEwHru2y7aoYChgIw/dACMP5JAbcAAAAAAQARAAAC4wIwAA8AAAEzAyMDIwMjAzMTMxMzEzMCeGuUamoCamuTamICbVttAgIw/dABi/51AjD+YwGd/mQAAAEADAAAAfoCMAALAAATMxc3MwMTIycHIxMcc3RzdbLBcoWFcsECMLa2/vb+2tHRASYAAAABAAn/KAHzAjAAEQAAFzI2PwEDMxMzEzMDBiMiJzUWTSYsDx7Ba4oCh2rqM30sJCx4KShQAgf+dgGK/YOLC18KAAAAAAEAIwAAAc8CMAAJAAApATUBITUhFQEhAc/+VAEd/u0BkP7kAS5VAXtgVv6GAAAAAAEAIP9CAZwDWwAfAAATJyY2OwEVIyIfARYHFg8BBjsBFSMiPwE2JisBNTMyNpgTEHJzQjiQFhIYdncZEhaQOEL0HxMLMDgbGzgwAgGHcWJJinOgKCegcYpJ04VQOlM6AAEAbv9CAMwDWwADAAAXETMRbl6+BBn75wAAAAABAAz/QgGIA1sAHwAAAQcGFjsBFSMiBh8BFisBNTMyLwEmNyY/ATYrATUzMhYBIxMLMDgbGzgwCxMf9EI4kBYSGXd2GBIWkDhCc3ICiIdQOlM6UIXTSYpxoCcooHOKSWIAAAAAAQAqAToB9QHvABQAAAEeATc2NzMGBwYnLgEHDgEHIzY3NgERKigWKQdMC3AwPCooFhUXBEwLcDABxR4VAQJHlgkDKh4VAQIgKJcJAwAAAAACAEL/QgC8AjAAAwAHAAATMxMjAzMVI1NYDnQDenoBSf35Au5/AAAAAgA3/7oB4gK9AB0AJwAAFzUuAScmPQE0Nz4BNzUzFR4BFyMmJxE2NzMOAQcVAxYXEQYHBh0BFPU/XBQPDxRcPzJOYwpkD0hFEmMNYkuCFTs6FgxGZQVHOik9aj0pOkYGXFwIX1JMDf5+C0ROWgdlAQY5CQGDCTgcMmQyAAEACQAAAg8C+AAmAAABIy4BIyIGHQEzFSMVFAcVMzI2NzMOASMhNTI2PQEjNTM1NDYzMhYB9mQGNywyOcDALcgsKgNdA1da/r8jHVFRdVpXcgIrNjpEP39Eg1oZAiE3aUxdMjyKRH9neWwAAAEAAgAAAiQC7gAWAAAlIxUjNSM1MzUjNTMDMxsBMwMzFSMVMwIMx2THx8euxnGgoHHGrsfHuLi4RGFEAU3+5AEc/rNEYQAAAAIAbv9CAMwDWwADAAcAABcRMxEDETMRbl5eXr4Bwf4/AlgBwf4/AAACADL/iQHbAu4ACwA6AAABJw4BBwYfAT4BNzYFNDY3JjU0NjMyFhcjJiMiBhUUFh8BFhUUBgcWFRQGIyImJzMWMzI2NzYmLwEuAQEkNiQ2AQJeQCMsAQL+sTkzWGdSUGgKYhRPKDA0Oh6rMilLbVRZcQxiF1srNQEBODsaYkkBkhEEMh9BHBMEMiBBNC5JETBXR1dPSkUpHyEtEQkzeSxNEC5UTlxYUVYwIyYwEAcbWgAAAAACAGICogGSAxEAAwAHAAATIzUzFyM1M8tpacdpaQKib29vAAADADz/9gMeAvgAKwAzADsAACU2NTMGBw4BKwEiJicmPQE0Nz4BOwEyFhcWFyMmJyYrASIHBh0BFBcWOwEyJBA2IBYQBiACEBYgNhAmIAH8CU4BCxFSMQcwUhIMDBJSMAcxUhELAU4CBxM2BzQVCQkVNAc2/lPNAUjNzf64mq8BHq+v/uL+FSIyGy4zMy0eLootHi00NC0bLiESLCwTIooiEywCAUbe3v663gIQ/uLBwQEewQACACgBnAEoAvgAFgAgAAATIz4BMzIdASMnBiMiJjU0NjsBNTQjIgcUMzI2PQEjIgZ1PwhAMXktByI2Nz0/Nk07LB08HStJGSICnCoyeN4pLzs1LzoQPbY5MBklHQAAAAACABwATQHgAj8ABQALAAATNxUHFxUnNxUHFxUc6IGBDOiBgQFG+W+Kim/5+W+Kim8AAAAABQAoAAACHALuAAMABgAJAAwADwAAAREhERMDERsBESUbAQsBIQIc/gzdq+Wr/o6qqqqpAVEC7v0SAu7+iAEZ/c4BGf7nAjIt/ukBF/6K/uwAAAAABAAwAX0BugMRAAcADwAcACQAABI0NjIWFAYiAhQWMjY0JiITIycjFSM1MzIWFRQHJyMVMzI2NTQwb6xvb6xRXpJeXpKpLDAmJ08kLi4kKS0QFQHyqnV1qnUBE5JlZZJl/uBgYOkiIDMOYEMTER8AAAEAXwKjAZUC5AADAAABFSE1AZX+ygLkQUEAAAACACgCFgELAvkABwAQAAASNDYyFhQGIiYUFjI2NTQmIihCXkNDXhMmOCcnOAJYXkNDXkKOOicoHB0oAAACAAwAAAIaArwAAwAPAAApATUhAzMVIxUjNSM1MzUzAhr98gIO2traWtraWlgBglji4ljiAAAAAAUAKAAAAhwC7gADAAYACQAMAA8AAAERIRETAxEbARElGwELASECHP4M3avlq/6OqqqqqQFRAu79EgLu/ogBGf3OARn+5wIyLf7pARf+iv7sAAAAAAUAKAAAAhwC7gADAAYACQAMAA8AAAERIRETAxEbARElGwELASECHP4M3avlq/6OqqqqqQFRAu79EgLu/ogBGf3OARn+5wIyLf7pARf+iv7sAAAAAAEAmwKRAY8DRQADAAATIzcz4kd2fgKRtAAAAAABAEn/DAH9AjAAFgAAIScGIyInIxYdASMRMxEUFjMyNjURMxEBshA4WDwpAwNkZDg0M01kSFImKx3IAyT+rD5HVDIBU/3QAAABADf/UQIIAu4ADgAAASMiJjU0NjMhESMRIxEjAQUOT3FwUAERSm1MAW5wT1Bx/GMDXvyiAAAAAAEAMwFKALEBzQADAAATIzUzsX5+AUqDAAEAnv8SAXcAGgAWAAAlBxYVFAYjIiYnMxYzMjY1NCYvASY/AQEeE2w6LC4/BjUNMBYcHiMZGQgcE0QaRik0MSYpGhMUGgoHBhlPAAAFACgAAAIcAu4AAwAGAAkADAAPAAABESEREwMRGwERJRsBCwEhAhz+DN2r5av+jqqqqqkBUQLu/RIC7v6IARn9zgEZ/ucCMi3+6QEX/or+7AAAAAACACoBnAEvAvgACwAXAAATFRQWMjY9ATQmIgYHNTQ2MhYdARQGIiZoJEAlJUAkPklySkpySQJtRiYrKyZGJisrbko+S0s+Sj5LSwAAAgAyAE0B9gI/AAUACwAAAQc1Nyc1BQc1Nyc1ARrogYEBxOiBgQFG+W+Kim/5+W+Kim8AAAUAKAAAAhwC7gADAAYACQAMAA8AAAERIRETAxEbARElGwELASECHP4M3avlq/6OqqqqqQFRAu79EgLu/ogBGf3OARn+5wIyLf7pARf+iv7sAAAAAAUAKAAAAhwC7gADAAYACQAMAA8AAAERIRETAxEbARElGwELASECHP4M3avlq/6OqqqqqQFRAu79EgLu/ogBGf3OARn+5wIyLf7pARf+iv7sAAAAAAUAKAAAAhwC7gADAAYACQAMAA8AAAERIRETAxEbARElGwELASECHP4M3avlq/6OqqqqqQFRAu79EgLu/ogBGf3OARn+5wIyLf7pARf+iv7sAAAAAAIAJv84Ac4CMAAbAB8AABMzFg4DFRQWMzI2PQEzFRQGIyImNTQ+AwMzFSPbXgYgNTcoPTIzPmN5W113KTk3Ihd6egFJPl44LTgiKiw9LxISWXNgVi1HMjNPARp/AAAAAAMADAAAAkgD2wADAAsADwAAAQMjAxcjByMTMxMjAyMnMwGCVwRVzOg9bdKY0m1hR61+AUkBPP7EZeQC7v0SAye0AAAAAwAMAAACSAPbAAMACwAPAAABAyMDFyMHIxMzEyMDIzczAYJXBFXM6D1t0pjSbchHdn4BSQE8/sRl5ALu/RIDJ7QAAAADAAwAAAJIA+cAAwALABIAAAEDIwMXIwcjEzMTIwMXIycHIzcBglcEVczoPW3SmNJtiZtTcnJTmwFJATz+xGXkAu79EgPnv4SEvwAAAwAMAAACSAO9AAMACwAiAAABAyMDFyMHIxMzEyMDHgEzMjY3Mw4BIyInLgEjIgcjPgEzMgGCVwRVzOg9bdKY0m2wJSEUEhsDPQU6MistJSEUKQc9BToxKwFJATz+xGXkAu79EgOcGhEfIEBJIRoRP0BJAAAEAAwAAAJIA6cAAwALAA8AEwAAAQMjAxcjByMTMxMjAyM1MxcjNTMBglcEVczoPW3SmNJt4Wlpx2lpAUkBPP7EZeQC7v0SAzhvb28ABAAMAAACSAP9AAMACwATABwAAAEDIwMXIwcjEzMTIwA0NjIWFAYiJhQWMjY1NCYiAYJXBFXM6D1t0pjSbf7dQl5DQ14TJjgnJzgBSQE8/sRl5ALu/RIDXF5DQ15CjjonKBwdKAAAAAAC//gAAANHAu4ADwATAAAzIwEhFSEVMxUjFSEVITUjEwMzEXB4AUsCBP7W8vIBKv5s4LOHtALuZ9Nn5mfmAaH+xgE6AAEAQf8SAigC+ABBAAAlNjUzBgcOASsBBxYVFAYjIiYnMxYzMjY1NCYvASY/AS4BJyY1ETQ3PgE7ATIWFxYXIyYnJisBIgcGFREUFxY7ATIBrw5rAREbekkEC2w6LC4/BjUNMBYcHiMZGQgQO2AXEhIceUgISXobEQFrAgwgVwhXIA4OIFcIV6IhMUMsREsnGkYpNDEmKRoTFBoKBwYZLwlJOS1FAQBFLUNMTEMqPzAcRkYfNv8ANh9FAAAAAgBMAAACAwPbAAsADwAAMxEhFSEVIRUhFSEVAyMnM0wBt/6zARP+7QFNgEetfgLuZ9Nn5mcDJ7QAAAAAAgBMAAACAwPbAAsADwAAMxEhFSEVIRUhFSEVAyM3M0wBt/6zARP+7QFN80d2fgLuZ9Nn5mcDJ7QAAAAAAgBMAAACAwPnAAsAEgAAMxEhFSEVIRUhFSEVAxcjJwcjN0wBt/6zARP+7QFNtptTcnJTmwLuZ9Nn5mcD57+EhL8AAAADAEwAAAIDA6cACwAPABMAADMRIRUhFSEVIRUhFQEjNTMXIzUzTAG3/rMBE/7tAU3+9mlpx2lpAu5n02fmZwM4b29vAAL/wQAAALYD2wADAAcAADMRMxEDIyczTGoBR61+Au79EgMntAAAAAACAEwAAAFBA9sAAwAHAAAzETMRAyM3M0xqIkd2fgLu/RIDJ7QAAAAAAv+8AAABRgPnAAMACgAAMxEzEQMXIycHIzdMagubU3JyU5sC7v0SA+e/hIS/AAAAA//pAAABGQOnAAMABwALAAAzETMRAyM1MxcjNTNMamRpacdpaQLu/RIDOG9vbwAAAgAPAAACWQLuAA8AHwAAASMVMxUjFTMyNzY9ATQnJgMjESM1MxEzMhcWHQEUBwYBYHuUlHthIA4OIGHlbGzlrDsSEjsCh9ZM/kYfNew1H0b9eQFlTAE9ji1G7EYtjgAAAgBMAAACPAO9AA8AJgAAEyMWFREjETMBMyY1ETMRIwMeATMyNjczDgEjIicuASMiByM+ATMysgEBZn4BDAEBZn94JSEUEhsDPQU6MistJSEUKQc9BToxKgI5IRn+AQLu/cchGQH//RIDnBoRHyBASSEaET9ASQAAAwBB//YCJwPbABMAKwAvAAAlETQnJisBIgcGFREUFxY7ATI3NiURNDc+ATsBMhYXFhURFAcOASsBIiYnJgEjJzMBvQ4gVwhXIA4OIFcIVyAO/oQSHHlICEh5HBISHHlICEh5HBIBWUetfvcBADcfRUUfN/8ANx9FRR83AQBFLUNMTEMtRf8ARS1DTExDLQJ1tAAAAAMAQf/2AicD2wATACsALwAAJRE0JyYrASIHBhURFBcWOwEyNzYlETQ3PgE7ATIWFxYVERQHDgErASImJyYTIzczAb0OIFcIVyAODiBXCFcgDv6EEhx5SAhIeRwSEhx5SAhIeRwS1Ed2fvcBADcfRUUfN/8ANx9FRR83AQBFLUNMTEMtRf8ARS1DTExDLQJ1tAAAAAADAEH/9gInA90AEwArADIAACURNCcmKwEiBwYVERQXFjsBMjc2JRE0Nz4BOwEyFhcWFREUBw4BKwEiJicmARcjJwcjNwG9DiBXCFcgDg4gVwhXIA7+hBIceUgISHkcEhIceUgISHkcEgEdm1NyclOb9wEANx9FRR83/wA3H0VFHzcBAEUtQ0xMQy1F/wBFLUNMTEMtAyu/hIS/AAADAEH/9gInA70AEwArAEIAACURNCcmKwEiBwYVERQXFjsBMjc2JRE0Nz4BOwEyFhcWFREUBw4BKwEiJicmEx4BMzI2NzMOASMiJy4BIyIHIz4BMzIBvQ4gVwhXIA4OIFcIVyAO/oQSHHlICEh5HBISHHlICEh5HBL0JSEUEhsDPQU6MistJSEUKQc9BToxKvcBADcfRUUfN/8ANx9FRR83AQBFLUNMTEMtRf8ARS1DTExDLQLqGhEfIEBJIRoRP0BJAAAABABB//YCJwOnABMAKwAvADMAACURNCcmKwEiBwYVERQXFjsBMjc2JRE0Nz4BOwEyFhcWFREUBw4BKwEiJicmEyM1MxcjNTMBvQ4gVwhXIA4OIFcIVyAO/oQSHHlICEh5HBISHHlICEh5HBLEaWnHaWn3AQA3H0VFHzf/ADcfRUUfNwEARS1DTExDLUX/AEUtQ0xMQy0Chm9vbwAAAQAkADkCCgIfAAsAACUHJzcnNxc3FwcXBwEXtT61tT61tT61tT7utT61tT61tT61tT4AAAMAQf/dAicDEQAfACoANQAAFzcmJyY1ETQ3PgE7ATIXNzMHFhcWFREUBw4BKwEiJwcBETQnAxY7ATI3NgERFBcTJisBIgcGTy8cDxISHHlICEI4G0wvHA8SEhx5SAhDOBoBIgjVIi4IVyAO/u4H1SMsCFcgDiNlHiYtRAEARC1DTSA5Zh8kLUT/AEQtQ00gOQEaAQArGf44F0YfATb/ACkbAcgXRh8AAAIASP/2Ai4D2wAZAB0AACURMxEUBw4BKwEiJicmNREzERQXFjsBMjc2AyMnMwHEahIceUgISHkcEmoOIFcIVyAON0etfvcB9/4JRS1DTExDLUUB9/4JNx9FRR8CZ7QAAAIASP/2Ai4D2wAZAB0AACURMxEUBw4BKwEiJicmNREzERQXFjsBMjc2AyM3MwHEahIceUgISHkcEmoOIFcIVyAOlEd2fvcB9/4JRS1DTExDLUUB9/4JNx9FRR8CZ7QAAAIASP/2Ai4D5wAZACAAACURMxEUBw4BKwEiJicmNREzERQXFjsBMjc2AxcjJwcjNwHEahIceUgISHkcEmoOIFcIVyAOX5tTcnJTm/cB9/4JRS1DTExDLUUB9/4JNx9FRR8DJ7+EhL8AAwBI//YCLgOnABkAHQAhAAAlETMRFAcOASsBIiYnJjURMxEUFxY7ATI3NgMjNTMXIzUzAcRqEhx5SAhIeRwSag4gVwhXIA64aWnHaWn3Aff+CUUtQ0xMQy1FAff+CTcfRUUfAnhvb28AAAAAAgADAAACQwPbAAkADQAAARMzAxEjEQMzGwEjNzMBJKd462rreKcKR3Z+AbcBN/5W/rwBRAGq/skBcLQAAgBMAAACKwLuABAAGwAAEzMyFx4BFRQGBwYrARUjETMRMzI3NjU0JyYrAbZ6RidCTExCJ0Z6amp4Oh08PB06eAJiDxhzTUxxGA+XAu7+EBEiSk0iEQAAAAEASf/2AiUC+AArAAAFIic1FjMyNjU0JisBNTMyNjU0JiMiBgcGFREjETQ3PgEzMhYVFAceARUUBgFSPDQwNjVEXUQUHDFKQTUmPQ8OZAwXdUxefGQ1TXkKD14TRD1DS1JGOTA5IiAcLv3zAgg3JEZPbFduOBBnR2pxAAADADL/9gHWAycAFwAiACYAABMjPgEzMhURIycjBiMiJjU0NjsBNTQjIgMUFjMyNj0BIyIGEyMnM7BoD2dSxkcMATheVWVqXXljSTAyMjBIeC03z0etfgGgR1PI/o5IUmNXT2EbZP7SLTNRKj0vAZm0AAAAAAMAMv/2AdYDJwAXACIAJgAAEyM+ATMyFREjJyMGIyImNTQ2OwE1NCMiAxQWMzI2PQEjIgYTIzczsGgPZ1LGRwwBOF5VZWpdeWNJMDIyMEh4LTdhR3Z+AaBHU8j+jkhSY1dPYRtk/tItM1EqPS8BmbQAAAAAAwAy//YB1gMpABcAIgApAAATIz4BMzIVESMnIwYjIiY1NDY7ATU0IyIDFBYzMjY9ASMiBhMXIycHIzewaA9nUsZHDAE4XlVlal15Y0kwMjIwSHgtN5+bU3JyU5sBoEdTyP6OSFJjV09hG2T+0i0zUSo9LwJPv4SEvwAAAAMAMv/2AdYDCQAXACIAOQAAEyM+ATMyFREjJyMGIyImNTQ2OwE1NCMiAxQWMzI2PQEjIgYTHgEzMjY3Mw4BIyInLgEjIgcjPgEzMrBoD2dSxkcMATheVWVqXXljSTAyMjBIeC03diUhFBIbAz0FOjIrLSUhFCkHPQU6MSsBoEdTyP6OSFJjV09hG2T+0i0zUSo9LwIOGhEfIEBJIRoRP0BJAAAABAAy//YB1gLzABcAIgAmACoAABMjPgEzMhURIycjBiMiJjU0NjsBNTQjIgMUFjMyNj0BIyIGEyM1MxcjNTOwaA9nUsZHDAE4XlVlal15Y0kwMjIwSHgtN0ZpacdpaQGgR1PI/o5IUmNXT2EbZP7SLTNRKj0vAapvb28AAAQAMv/2AdYDSQAXACIAKgAzAAATIz4BMzIVESMnIwYjIiY1NDY7ATU0IyIDFBYzMjY9ASMiBhI0NjIWFAYiJhQWMjY1NCYisGgPZ1LGRwwBOF5VZWpdeWNJMDIyMEh4LTcJQl5DQ14TJjgnJzgBoEdTyP6OSFJjV09hG2T+0i0zUSo9LwHOXkNDXkKOOicoHB0oAAADADL/9gMfAjoAJwAyADgAACUzDgEjIicGIyImNTQ2OwE1NCMiByM+ATMyFzYzMhYdASEVFBYzMjYlFBYzMjY9ASMiBiUzLgEjIgK0aBFxU3o0Om9ZZWhbfWJJFmsPaVJzKzVlZ3D+tz00KDn97jIyMEh6KjgBQOUDPDRsqVZdaWljV05cIWQ/R1NJSYN/Mi4+SS4yLTNRKjotgUNCAAAAAQA3/xIB4AI6AD0AACU2NzMGBw4BDwEWFRQGIyImJzMWMzI2NTQmLwEmPwEuAScmPQE0Nz4BMzIWFxYXIyYnJiMiBwYdARQXFjMyAXEFA2YFBxZoSAtsOiwuPwY1DTAWHB4jGRkIEDdPEg8PFmlHSGkWCAVmAwYZTEsaDAwaS0yWDA4fFj9FAScaRik0MSYpGhMUGgoHBhkvCkQ1LTtmOy1AR0Y/GSYVD0BCHDNiMxxCAAAAAwA3//YB4gMnABsAIwAnAAASMhYXFh0BIRUUFjMyNjczDgEjIiYnJj0BNDc2FwYHMyYnJiI3Iyczxo5pFg/+uT00KDkMZxFxUkdpFg8PFksLAeQBCxqYpketfgI6R0AtO0oiQU0tKFVbR0AtO2Y7LUBWGigoGkKUtAAAAAMAN//2AeIDJwAbACMAJwAAEjIWFxYdASEVFBYzMjY3Mw4BIyImJyY9ATQ3NhcGBzMmJyYiNyM3M8aOaRYP/rk9NCg5DGcRcVJHaRYPDxZLCwHkAQsamCtHdn4COkdALTtKIkFNLShVW0dALTtmOy1AVhooKBpClLQAAAADADf/9gHiAykAGwAjACoAABIyFhcWHQEhFRQWMzI2NzMOASMiJicmPQE0NzYXBgczJicmIhMXIycHIzfGjmkWD/65PTQoOQxnEXFSR2kWDw8WSwsB5AELGph0m1NyclObAjpHQC07SiJBTS0oVVtHQC07ZjstQFYaKCgaQgFKv4SEvwAEADf/9gHiAvMAGwAjACcAKwAAEjIWFxYdASEVFBYzMjY3Mw4BIyImJyY9ATQ3NhcGBzMmJyYiNyM1MxcjNTPGjmkWD/65PTQoOQxnEXFSR2kWDw8WSwsB5AELGpgbaWnHaWkCOkdALTtKIkFNLShVW0dALTtmOy1AVhooKBpCpW9vbwAC/7gAAACtAycAAwAHAAAzETMRAyMnM0lkAUetfgIw/dACc7QAAAAAAgBJAAABPgMnAAMABwAAMxEzEQMjNzNJZBxHdn4CMP3QAnO0AAAAAAL/xgAAATADMwAGAAoAABMXIycHIzcDETMRo41SY2NSjQpkAzO/gYG//M0CMP3QAAP/4wAAARMC8wADAAcACwAAMxEzEQMjNTMXIzUzSWRhaWnHaWkCMP3QAoRvb28AAAIAN//2AeEDEQAjADMAAAEVFAcOASImJyY9ATQ3PgEzMhYXJicHJzcmJzcWFzcXBxYXFgc1NCYjIgcGHQEUFxYyNzYB4Q8WaY5pFg8PFmE9FToOBTd6F2seH1UkHnEXYi4bI2Q9OEYaDQwalhoMASQ/Oy1AR0dALTsxQiw/RRENI1cuPygqJCYpKSs/JU1OZKA5OE88HjcvMxxCQhwAAAACAEkAAAH9AwkAGAAvAAABMhYXFhURIxE0JyYjIgYHBhURIxEzFzM2Nx4BMzI2NzMOASMiJy4BIyIHIz4BMzIBP0VdEgpkChdMJD0RDWRLDQE2RyUhFBIbAz0FOjIrLSUhFCkHPQU6MSoCOktEIjn+sAFNKx5ELSMcKf67AjBJU64aER8gQEkhGhE/QEkAAAADADf/9gHhAycAEwAjACcAAAEVFAcOASImJyY9ATQ3PgEyFhcWBzU0JyYiBwYdARQXFjI3NgMjJzMB4Q8WaY5pFg8PFmmOaRYPZAwalhoMDBqWGgwVR61+AUtmOy1AR0dALTtmOy1AR0dALZ9iMxxCQhwzYjMcQkIcAb+0AAAAAAMAN//2AeEDJwATACMAJwAAARUUBw4BIiYnJj0BNDc+ATIWFxYHNTQnJiIHBh0BFBcWMjc2AyM3MwHhDxZpjmkWDw8WaY5pFg9kDBqWGgwMGpYaDIVHdn4BS2Y7LUBHR0AtO2Y7LUBHR0Atn2IzHEJCHDNiMxxCQhwBv7QAAAAAAwA3//YB4QMpABMAIwAqAAABFRQHDgEiJicmPQE0Nz4BMhYXFgc1NCcmIgcGHQEUFxYyNzYDFyMnByM3AeEPFmmOaRYPDxZpjmkWD2QMGpYaDAwalhoMR5tTcnJTmwFLZjstQEdHQC07ZjstQEdHQC2fYjMcQkIcM2IzHEJCHAJ1v4SEvwAAAAMAN//2AeEDCQATACMAOgAAARUUBw4BIiYnJj0BNDc+ATIWFxYHNTQnJiIHBh0BFBcWMjc2Ax4BMzI2NzMOASMiJy4BIyIHIz4BMzIB4Q8WaY5pFg8PFmmOaRYPZAwalhoMDBqWGgxtJSEUEhsDPQU6MistJSEUKQc9BToxKwFLZjstQEdHQC07ZjstQEdHQC2fYjMcQkIcM2IzHEJCHAI0GhEfIEBJIRoRP0BJAAAABAA3//YB4QLzABMAIwAnACsAAAEVFAcOASImJyY9ATQ3PgEyFhcWBzU0JyYiBwYdARQXFjI3NgMjNTMXIzUzAeEPFmmOaRYPDxZpjmkWD2QMGpYaDAwalhoMoGlpx2lpAUtmOy1AR0dALTtmOy1AR0dALZ9iMxxCQhwzYjMcQkIcAdBvb28AAAMAHgAoAiwCMAADAAcACwAAASM1MxEjNTM3ITUhAVxzc3Nz0P3yAg4BtXv9+HtdWAAAAAADADf/3QHhAlMAHQAnADEAAAEHFhcWHQEUBw4BIyInByM3JicmPQE0Nz4BMzIXNwMTJiMiBwYdARQ3NTQnAxYzMjc2AcUmIRIPDxdoRy8uFEYlJBAPDxdoRzIsFdWWGBxLGgziDpYVHksaDAJTUB8xKT1qPSlARxIrTyEwKT1qPihARxMs/j4BPQtCHTBkMDBkMCP+xApCHQAAAAACAEb/9gH4AycAGAAcAAAFIiYnJjURMxEUFxYzMjY3NjURMxEjJyMGEyMnMwEDRVwSCmQKF0skPBENZEsNATYJR61+CktEIjkBUP6zKx5ELSMcKQFF/dBJUwJ9tAAAAAACAEb/9gH4AycAGAAcAAAFIiYnJjURMxEUFxYzMjY3NjURMxEjJyMGAyM3MwEDRVwSCmQKF0skPBENZEsNATZUR3Z+CktEIjkBUP6zKx5ELSMcKQFF/dBJUwJ9tAAAAAACAEb/9gH4AzMAGAAfAAAFIiYnJjURMxEUFxYzMjY3NjURMxEjJyMGAxcjJwcjNwEDRVwSCmQKF0skPBENZEsNATYfm1NyclObCktEIjkBUP6zKx5ELSMcKQFF/dBJUwM9v4SEvwAAAAMARv/2AfgC8wAYABwAIAAABSImJyY1ETMRFBcWMzI2NzY1ETMRIycjBgMjNTMXIzUzAQNFXBIKZAoXSyQ8EQ1kSw0BNnhpacdpaQpLRCI5AVD+syseRC0jHCkBRf3QSVMCjm9vbwAAAgAJ/ygB8wMnABEAFQAAFzI2PwEDMxMzEzMDBiMiJzUWEyM3M00mLA8ewWuKAodq6jN9LCQs0kd2fngpKFACB/52AYr9g4sLXwoC67QAAAACAEv/DAH/AxEAEwAsAAATFRQXHgEzMjc2PQE0JyYjIgYHBgMzFQczNjMyFhcWHQEUBw4BIyInIxYdASOvCA07KlAZCQoXTCQ9EQ1kZAEBNFpGYBIKCRJlR1Q1AQFkAUVXJBooMkwaK2YrHkQtIxwBo+U6SEtEIjloNyRJTjohGeoAAAADAAn/KAHzAvMAEQAVABkAABcyNj8BAzMTMxMzAwYjIic1FhMjNTMXIzUzTSYsDx7Ba4oCh2rqM30sJCyaaWnHaWl4KShQAgf+dgGK/YOLC18KAvxvb28AAwAMAAACSAN6AAMABwAPAAABFSE1EwMjAxcjByMTMxMjAcX+yvNXBFXM6D1t0pjSbQN6QUH9zwE8/sRl5ALu/RIAAAADADL/9gHWAsYAFwAiACYAABMjPgEzMhURIycjBiMiJjU0NjsBNTQjIgMUFjMyNj0BIyIGARUhNbBoD2dSxkcMATheVWVqXXljSTAyMjBIeC03ARf+ygGgR1PI/o5IUmNXT2EbZP7SLTNRKj0vAexBQQAAAAMADAAAAkgDygADAAsAFwAAAQMjAxcjByMTMxMjAiImJzMeATI2NzMGAYJXBFXM6D1t0pjSbWeUWwVABjpUOgZABQFJATz+xGXkAu79EgMjW0wuNTUuTAADADL/9gHWAxYAFwAiAC4AABMjPgEzMhURIycjBiMiJjU0NjsBNTQjIgMUFjMyNj0BIyIGEiImJzMeATI2NzMGsGgPZ1LGRwwBOF5VZWpdeWNJMDIyMEh4LTfClFsFQAY6VDoGQAUBoEdTyP6OSFJjV09hG2T+0i0zUSo9LwGVW0wuNTUuTAAAAgAM/x0CTALuABcAGwAAJSMHIxMzEwYHBhUUFjMyNxcGIyImJyY3CwEjAwGe6D1t0pjSOiUwIBwhIxMuLjVEAQNoWVcEVeTkAu79EhAdJysZHRIoGDAtTDkBSgE8/sQAAgAy/x0B9wI6ACYAMQAABSY3JyMGIyImNTQ2OwE1NCMiByM+ATMyFREOARUUFjMyNxcGIyImAxQWMzI2PQEjIgYBIQNwCwE4XlVlal15Y0kWaA9nUsY7NyAcIiITLi41RIwyMjBIeC03hk08RVJjV09hG2Q/R1PI/o4lOSEZHRIoGDABZC0zUSo9LwACAEH/9gIoA9sAKwAvAAAlNjUzBgcOASsBIiYnJjURNDc+ATsBMhYXFhcjJicmKwEiBwYVERQXFjsBMgMjNzMBrw5rAREbekkISHkcEhIceUgISXobEQFrAgwgVwhXIA4OIFcIV3pHdn6iITFDLERLTEMtRQEARS1DTExDKj8wHEZGHzb/ADYfRQLKtAAAAAACADf/9gHgAycAJwArAAAlNjczBgcOASMiJicmPQE0Nz4BMzIWFxYXIyYnJiMiBwYdARQXFjMyAyM3MwFxBQNmBQcWaUhHaRYPDxZpR0hpFggFZgMGGUxLGgwMGktMbUd2fpYMDh8WP0ZHQC07ZjstQEdGPxkmFQ9AQhwzYjMcQgIdtAACAEH/9gIoA+cAKwAyAAAlNjUzBgcOASsBIiYnJjURNDc+ATsBMhYXFhcjJicmKwEiBwYVERQXFjsBMgMXIycHIzcBrw5rAREbekkISHkcEhIceUgISXobEQFrAgwgVwhXIA4OIFcIVzGbU3JyU5uiITFDLERLTEMtRQEARS1DTExDKj8wHEZGHzb/ADYfRQOKv4SEvwAAAAIAN//2AeADMwAnAC4AACU2NzMGBw4BIyImJyY9ATQ3PgEzMhYXFhcjJicmIyIHBh0BFBcWMzIDFyMnByM3AXEFA2YFBxZpSEdpFg8PFmlHSGkWCAVmAwYZTEsaDAwaS0wfm1NyclOblgwOHxY/RkdALTtmOy1AR0Y/GSYVD0BCHDNiMxxCAt2/hIS/AAAAAAIAQf/2AigDqQArAC8AACU2NTMGBw4BKwEiJicmNRE0Nz4BOwEyFhcWFyMmJyYrASIHBhURFBcWOwEyAyM1MwGvDmsBERt6SQhIeRwSEhx5SAhJehsRAWsCDCBXCFcgDg4gVwhXJG9voiExQyxES0xDLUUBAEUtQ0xMQyo/MBxGRh82/wA2H0UC2nIAAgA3//YB4AL1ACcAKwAAJTY3MwYHDgEjIiYnJj0BNDc+ATMyFhcWFyMmJyYjIgcGHQEUFxYzMgMjNTMBcQUDZgUHFmlIR2kWDw8WaUdIaRYIBWYDBhlMSxoMDBpLTBtvb5YMDh8WP0ZHQC07ZjstQEdGPxkmFQ9AQhwzYjMcQgItcgAAAgBB//YCKAPnACsAMgAAJTY1MwYHDgErASImJyY1ETQ3PgE7ATIWFxYXIyYnJisBIgcGFREUFxY7ATITByMnMxc3Aa8OawERG3pJCEh5HBISHHlICEl6GxEBawIMIFcIVyAODiBXCFdqm1SbU3JyoiExQyxES0xDLUUBAEUtQ0xMQyo/MBxGRh82/wA2H0UDir+/hIQAAAACADf/9gHgAzMAJwAuAAAlNjczBgcOASMiJicmPQE0Nz4BMzIWFxYXIyYnJiMiBwYdARQXFjMyEwcjJzMXNwFxBQNmBQcWaUhHaRYPDxZpR0hpFggFZgMGGUxLGgwMGktMcptUm1NycpYMDh8WP0ZHQC07ZjstQEdGPxkmFQ9AQhwzYjMcQgLdv7+EhAAAAAADAEwAAAIpA+cACwAXAB4AADczMjc2PQE0JyYrARMjETMyFxYdARQHBhMHIyczFze2emEgDg4gYXp65OSsOxISOwmbVJtTcnJnRh817DUfRv15Au6OLUbsRi2OA+e/v4SEAAAAAwA3//YCnAMRABMAKwA5AAAlNTQnLgEjIgcGHQEUFxYzMjY3NgMyFzMmPQEzESMnIwYjIiYnJj0BNDc+ASU1MxUUBw4BBzU2NTQjAYcIDTsqUBkJChdMJD0RDYlUNQEBZEsNATZjRmASCgkSZQF6awMGODI8CetXJBooMkwaK2YrHkQtIxwBeDohGdf870lTS0QiOWg3JElObmlOFxMpOgcoFDMKAAAAAgAPAAACWQLuAA8AHwAAASMVMxUjFTMyNzY9ATQnJgMjESM1MxEzMhcWHQEUBwYBYHuUlHthIA4OIGHlbGzlrDsSEjsCh9ZM/kYfNew1H0b9eQFlTAE9ji1G7EYtjgAAAgA3//YCSQMRAB8AMwAAEzIXMyY9ASM1MzUzFTMVIxEjJyMGIyImJyY9ATQ3PgETNTQnLgEjIgcGHQEUFxYzMjY3Nv5UNQEBrKxkXl5LDQE2Y0ZgEgoJEmXQCA07KlAZCQoXTCQ9EQ0COjohGUlGSEhG/X1JU0tEIjloNyRJTv6xVyQaKDJMGitmKx5ELSMcAAAAAAIATAAAAgMDegALAA8AADMRIRUhFSEVIRUhFQMVITVMAbf+swET/u0BTUH+ygLuZ9Nn5mcDekFBAAAAAAMAN//2AeICxgAbACMAJwAAEjIWFxYdASEVFBYzMjY3Mw4BIyImJyY9ATQ3NhcGBzMmJyYiNxUhNcaOaRYP/rk9NCg5DGcRcVJHaRYPDxZLCwHkAQsamOj+ygI6R0AtO0oiQU0tKFVbR0AtO2Y7LUBWGigoGkLnQUEAAAACAEwAAAIDA8oACwAXAAAzESEVIRUhFSEVIRUCIiYnMx4BMjY3MwZMAbf+swET/u0BTZiUWwVABjpUOgZABQLuZ9Nn5mcDI1tMLjU1LkwAAAMAN//2AeIDFgAbACMALwAAEjIWFxYdASEVFBYzMjY3Mw4BIyImJyY9ATQ3NhcGBzMmJyYiNiImJzMeATI2NzMGxo5pFg/+uT00KDkMZxFxUkdpFg8PFksLAeQBCxqYlZRbBUAGOlQ6BkAFAjpHQC07SiJBTS0oVVtHQC07ZjstQFYaKCgaQpBbTC41NS5MAAIATAAAAgMDqQALAA8AADMRIRUhFSEVIRUhFQMjNTNMAbf+swET/u0BTaBvbwLuZ9Nn5mcDN3IAAwA3//YB4gL1ABsAIwAnAAASMhYXFh0BIRUUFjMyNjczDgEjIiYnJj0BNDc2FwYHMyYnJiI3IzUzxo5pFg/+uT00KDkMZxFxUkdpFg8PFksLAeQBCxqYgm9vAjpHQC07SiJBTS0oVVtHQC07ZjstQFYaKCgaQqRyAAAAAAEATP8dAgkC7gAdAAAzESEVIRUhFSEVIRUiBw4BFRQWMzI3FwYjIiYnJjdMAbf+swET/u0BTR0dKyggHCIiEy4uNUQBA2sC7mfTZ+ZnFB4xHBkdEigYMC1NOQAAAgA3/x0B4gI6ACoAMgAAEjIWFxYdASEVFBYzMjY3MwYHDgEVFBYzMjcXBiMiJicmNy4BJyY9ATQ3NhcGBzMmJyYixo5pFg/+uT00KDkMZxRJUUAgHCIiEy4uNUQBA1pFZhUPDxZLCwHkAQsamAI6R0AtO0oiQU0tKGIuND8iGR0SKBgwLUY2Akc+LTtmOy1AVhooKBpCAAACAEwAAAIDA+cACwASAAAzESEVIRUhFSEVIRUDByMnMxc3TAG3/rMBE/7tAU0Vm1SbU3JyAu5n02fmZwPnv7+EhAAAAAMAN//2AeIDMwAbACMAKgAAEjIWFxYdASEVFBYzMjY3Mw4BIyImJyY9ATQ3NhcGBzMmJyYiAQcjJzMXN8aOaRYP/rk9NCg5DGcRcVJHaRYPDxZLCwHkAQsamAESm1SbU3JyAjpHQC07SiJBTS0oVVtHQC07ZjstQFYaKCgaQgFUv7+EhAAAAAACAEH/9gIoA+cALgA1AAAlMzI2PQEjNTMRIycGKwEiJicmNRE0Nz4BOwEyFhcWFyMmJy4BKwEiBwYVERQXFhMXIycHIzcBMAg6TYvzMxk+awdIdRsSEhx5SAhKexkPA2wDCxBAJghXIA4PIISbU3JyU5tcTFUaZP6FS1VLRC1FAQBFLUNMTEMpNygaJCNGHzb/ADQhRgOLv4SEvwADADf/EgHrAzMAEwA5AEAAACU1NCcuASMiBwYdARQXFjMyNjc2AzI3Nj0BNyMGIyImJyY9ATQ3PgEzMhYXMzczERQHDgEjIiYnMxYTFyMnByM3AYcIDTsqUBkJChdMJD0RDXFLGgwBATRaRmASCgkSZUcuUBkBCksPFmpHU3ERaRiDm1NyclOb9U0kGigyTBorXCseRC0jHP6kQxwzDDpIS0QiOV43JElOKSNC/dE7LUBHV1NMA8O/hIS/AAIAQf/2AigDygAuADoAACUzMjY9ASM1MxEjJwYrASImJyY1ETQ3PgE7ATIWFxYXIyYnLgErASIHBhURFBcWEiImJzMeATI2NzMGATAIOk2L8zMZPmsHSHUbEhIceUgISnsZDwNsAwsQQCYIVyAODyCjlFsFQAY6VDoGQAVcTFUaZP6FS1VLRC1FAQBFLUNMTEMpNygaJCNGHzb/ADQhRgLHW0wuNTUuTAAAAAADADf/EgHrAxYAEwA5AEUAACU1NCcuASMiBwYdARQXFjMyNjc2AzI3Nj0BNyMGIyImJyY9ATQ3PgEzMhYXMzczERQHDgEjIiYnMxYSIiYnMx4BMjY3MwYBhwgNOypQGQkKF0wkPRENcUsaDAEBNFpGYBIKCRJlRy5QGQEKSw8WakdTcRFpGKCUWwVABjpUOgZABfVNJBooMkwaK1wrHkQtIxz+pEMcMww6SEtEIjleNyRJTikjQv3ROy1AR1dTTAL/W0wuNTUuTAAAAAACAEH/9gIoA6kALgAyAAAlMzI2PQEjNTMRIycGKwEiJicmNRE0Nz4BOwEyFhcWFyMmJy4BKwEiBwYVERQXFhMjNTMBMAg6TYvzMxk+awdIdRsSEhx5SAhKexkPA2wDCxBAJghXIA4PIJNvb1xMVRpk/oVLVUtELUUBAEUtQ0xMQyk3KBokI0YfNv8ANCFGAttyAAAAAwA3/xIB6wL1ABMAOQA9AAAlNTQnLgEjIgcGHQEUFxYzMjY3NgMyNzY9ATcjBiMiJicmPQE0Nz4BMzIWFzM3MxEUBw4BIyImJzMWEyM1MwGHCA07KlAZCQoXTCQ9EQ1xSxoMAQE0WkZgEgoJEmVHLlAZAQpLDxZqR1NxEWkYi29v9U0kGigyTBorXCseRC0jHP6kQxwzDDpIS0QiOV43JElOKSNC/dE7LUBHV1NMAxNyAAAAAgBB/uwCKAL4AC4APAAAJTMyNj0BIzUzESMnBisBIiYnJjURNDc+ATsBMhYXFhcjJicuASsBIgcGFREUFxYXNTMVFAcOAQc1NjU0IwEwCDpNi/MzGT5rB0h1GxISHHlICEp7GQ8DbAMLEEAmCFcgDg8gKWsDBjgyPAlcTFUaZP6FS1VLRC1FAQBFLUNMTEMpNygaJCNGHzb/ADQhRvdpThYUKToHKBQzCgAAAAMAN/8SAesDYwANACEARwAAARUjNTQ3PgE3FQYVFDMTNTQnLgEjIgcGHQEUFxYzMjY3NgMyNzY9ATcjBiMiJicmPQE0Nz4BMzIWFzM3MxEUBw4BIyImJzMWAUdrAwY4MjwJawgNOypQGQkKF0wkPRENcUsaDAEBNFpGYBIKCRJlRy5QGQEKSw8WakdTcRFpGALqaU4XEyk6BygUMwr+C00kGigyTBorXCseRC0jHP6kQxwzDDpIS0QiOV43JElOKSNC/dE7LUBHV1NMAAIATAAAAj0D8QALABIAADMRMxEhETMRIxEhERMXIycHIzdMagEdamr+47ibU3JyU5sC7v7MATT9EgFT/q0D8b+EhL8AAgBJAAAB/QP7ABkAIAAAISMRNCcmIyIGBwYVESMRMxUHMzYzMhYXFhUDFyMnByM3Af1kChdMJD0RDWRkAQE0XkVdEgqym1NyclObAU0rHkQtIxwp/rsDEeU6SEtEIjkCq7+EhL8AAAAC//YAAAKjAu4AEwAXAAAzESM1MzUzFSE1MxUzFSMRIxEhGQEhNSFUXl5qAR1qXl5q/uMBHf7jAjBGeHh4eEb90AFT/q0BunYAAAAAAf/2AAACCAMRACEAACEjETQnJiMiBgcGFREjESM1MzUzFTMVIxUHMzYzMhYXFhUCCGQKF0wkPRENZF5eZKysAQE0XkVdEgoBTSseRC0jHCn+uwKDRkhIRlc6SEtEIjkAAv+8AAABTAO9AAMAGgAAMxEzEQMeATMyNjczDgEjIicuASMiByM+ATMyTGoxJSEUEhsDPQU6MistJSEUKQc9BToxKgLu/RIDnBoRHyBASSEaET9ASQAAAAL/tQAAAUUDCQADABoAADMRMxEDHgEzMjY3Mw4BIyInLgEjIgcjPgEzMklkLyUhFBIbAz0FOjIrLSUhFCkHPQU6MSsCMP3QAugaER8gQEkhGhE/QEkAAAAC/+YAAAEcA3oAAwAHAAAzETMRExUhNUxqZv7KAu79EgN6QUEAAAAAAv/gAAABFgLGAAMABwAAMxEzERMVITVJZGn+ygIw/dACxkFBAAAAAAL/1wAAASsDygADAA8AADMRMxESIiYnMx4BMjY3MwZMahWUWwVABjpUOgZABQLu/RIDI1tMLjU1LkwAAAL/0QAAASUDFgADAA8AADMRMxESIiYnMx4BMjY3MwZJZBiUWwVABjpUOgZABQIw/dACb1tMLjU1LkwAAAH///8dANgC7gASAAAXJjcRMxEOARUUFjMyNxcGIyImAgNNajs2IBwhIxMuLjVEhj8zAwL9EiY4IRkdEigYMAAC//j/HQDPAvgAEgAWAAAHJjcRMxEOARUUFjMyNxcGIyImEzUzFQcBUWQ7NiAcIiITLi41RExqhkEzAkL90CY4IRkdEigYMAM6cXEAAgBKAAAAuQOpAAMABwAAMxEzERMjNTNMagNvbwLu/RIDN3IAAQBJAAAArQIwAAMAADMRMxFJZAIw/dAAAgBM//YC0QLuAAMAGQAAMxEzERMVFBcWMjc2NREzERQHDgEiJicmPQFMatkSH3YfEmoVGWWGZRkVAu79EgEPMS4fNTUfLwIP/fZBMjlCQjkyQCwABABG/xMBrwL4AAMABwAVABkAADMRMxEDNTMVExEzERQGIyInNRYzMjYDNTMVSWRnaphkaVwZFBgONzEDagIw/dACh3Fx/VgCUf2uZ2QCYAIvAuVxcQAAAAIALf/2AmkD5wAVABwAABMVFBcWMjc2NREzERQHDgEiJicmPQEBFyMnByM3lxIfdh8SahUZZYZlGRUBoZtTcnJTmwEPMS4fNTUfLwIP/fZBMjlCQjkyQCwC2L+EhL8AAAL/u/8TAUUDMwANABQAABcRMxEUBiMiJzUWMzI2ExcjJwcjN1JkaVwZFBgONzFYm1NyclObIQJR/a5nZAJgAi8Dkb+EhL8AAAIATP7sAmwC7gAOABwAACEjAwcVIxEzERc2NxMzCwE1MxUUBw4BBzU2NTQjAmx80GpqagETEPyC7WFrAwY4MjwJAX6E+gLu/qIBGBQBM/7l/ZJpThYUKToHKBQzCgAAAAIASf7sAgYDEQALABkAADcVIxEzETczBxMjCwE1MxUUBw4BBzU2NTQjrWRky4LH03iiC2sDBjgyPAnZ2QMR/kfYyv6aARv+SmlOFhQpOgcoFDMKAAEASQAAAgYCMAALAAA3FSMRMxU3MwcTIwOtZGTLgsfTeKLZ2QIw2NjK/poBGwAAAAIATAAAAgUD2wAFAAkAADMRMxEhFQEjNzNMagFP/pJHdn4C7v15ZwMntAAAAgBJAAABQQP5AAMABwAAMxEzEQMjNzNJZBlHdn4DEfzvA0W0AAAAAAIATP7sAgUC7gAFABMAADMRMxEhFQU1MxUUBw4BBzU2NTQjTGoBT/77awMGODI8CQLu/Xlnm2lOFhQpOgcoFDMKAAAAAgA+/uwAsQMRAAMAEQAAMxEzEQc1MxUUBw4BBzU2NTQjSWRnawMGODI8CQMR/O+baU4WFCk6BygUMwoAAgBMAAACBQMRAAUAEwAAMxEzESEVAzUzFRQHDgEHNTY1NCNMagFPpGsDBjgyPAkC7v15ZwKoaU4XEyk6BygUMwoAAAACAEkAAAFeAxEAAwARAAAzETMREzUzFRQHDgEHNTY1NCNJZEZrAwY4MjwJAxH87wKoaU4XEyk6BygUMwoAAAAAAgBMAAACBQLuAAUACQAAMxEzESEVAyM1M0xqAU80b28C7v15ZwFccgAAAAACAEkAAAFtAxEAAwAHAAAzETMREyM1M0lkwG9vAxH87wFccgABAA8AAAI1Au4ADQAAARUHFSEVIREHNTcRMxEBlrABT/5HbW1qAh9YZvpnASQ/WD8Bcv7LAAAAAAEADwAAAVMDEQALAAAzEQc1NxEzETcVBxF/cHBkcHABVU9WTwFm/uBPVk/+ZQAAAAIATAAAAjwD2wAPABMAABMjFhURIxEzATMmNREzESMDIzczsgEBZn4BDAEBZn+BR3Z+AjkhGf4BAu79xyEZAf/9EgMntAAAAAIASQAAAf0DJwAYABwAAAEyFhcWFREjETQnJiMiBgcGFREjETMXMzY3IzczAT9FXRIKZAoXTCQ9EQ1kSw0BNipHdn4COktEIjn+sAFNKx5ELSMcKf67AjBJUzm0AAAAAAIATP7sAjwC7gAPAB0AABMjFhURIxEzATMmNREzESMHNTMVFAcOAQc1NjU0I7IBAWZ+AQwBAWZ/t2sDBjgyPAkCOSEZ/gEC7v3HIRkB//0Sm2lOFhQpOgcoFDMKAAAAAAIASf7sAf0COgAYACYAAAEyFhcWFREjETQnJiMiBgcGFREjETMXMzYTNTMVFAcOAQc1NjU0IwE/RV0SCmQKF0wkPRENZEsNATYXawMGODI8CQI6S0QiOf6wAU0rHkQtIxwp/rsCMElT/StpThYUKToHKBQzCgAAAAIATAAAAjwD5wAPABYAABMjFhURIxEzATMmNREzESMTByMnMxc3sgEBZn4BDAEBZn9Mm1SbU3JyAjkhGf4BAu79xyEZAf/9EgPnv7+EhAAAAgBJAAAB/QMzABgAHwAAATIWFxYVESMRNCcmIyIGBwYVESMRMxczNiUHIyczFzcBP0VdEgpkChdMJD0RDWRLDQE2AQ6bVJtTcnICOktEIjn+sAFNKx5ELSMcKf67AjBJU/m/v4SEAAACAAoAAAJZAxEAGAAmAAABMhYXFhURIxE0JyYjIgYHBhURIxEzFzM2JTUzFRQHDgEHNTY1NCMBm0VdEgpkChdMJD0RDWRLDQE2/t5rAwY4MjwJAjpLRCI5/rABTSseRC0jHCn+uwIwSVNuaU4XEyk6BygUMwoAAAABAEz/EwI8Au4AGwAAEyMWFREjETMBMyY1ETMRFAYjIic1FjMyNj0BI7IBAWZ+AQwBAWZpXBkUGA44Mh0COSEZ/gEC7v3HIRkB//zwZ2QCYAIvPSEAAAAAAQBJ/xMB/QI6ACIAAAURNCcmIyIGBwYVESMRMxczNjMyFhcWFREUBiMiJzUWMzI2AZkKF0wkPRENZEsNATZnRV0SCmlcGRQYDjcxIQFuKx5ELSMcKf67AjBJU0tEIjn+jmdkAmACLwADAEH/9gInA3oAEwArAC8AACURNCcmKwEiBwYVERQXFjsBMjc2JRE0Nz4BOwEyFhcWFREUBw4BKwEiJicmARUhNQG9DiBXCFcgDg4gVwhXIA7+hBIceUgISHkcEhIceUgISHkcEgGO/sr3AQA3H0VFHzf/ADcfRUUfNwEARS1DTExDLUX/AEUtQ0xMQy0CyEFBAAAAAwA3//YB4QLGABMAIwAnAAABFRQHDgEiJicmPQE0Nz4BMhYXFgc1NCcmIgcGHQEUFxYyNzYTFSE1AeEPFmmOaRYPDxZpjmkWD2QMGpYaDAwalhoMKv7KAUtmOy1AR0dALTtmOy1AR0dALZ9iMxxCQhwzYjMcQkIcAhJBQQAAAAADAEH/9gInA8oAEwArADcAACURNCcmKwEiBwYVERQXFjsBMjc2JRE0Nz4BOwEyFhcWFREUBw4BKwEiJicmACImJzMeATI2NzMGAb0OIFcIVyAODiBXCFcgDv6EEhx5SAhIeRwSEhx5SAhIeRwSAT2UWwVABjpUOgZABfcBADcfRUUfN/8ANx9FRR83AQBFLUNMTEMtRf8ARS1DTExDLQJxW0wuNTUuTAADADf/9gHhAxYAEwAjAC8AAAEVFAcOASImJyY9ATQ3PgEyFhcWBzU0JyYiBwYdARQXFjI3NgIiJiczHgEyNjczBgHhDxZpjmkWDw8WaY5pFg9kDBqWGgwMGpYaDCeUWwVABjpUOgZABQFLZjstQEdHQC07ZjstQEdHQC2fYjMcQkIcM2IzHEJCHAG7W0wuNTUuTAAABABB//YCOgPbABMAKwAvADMAACURNCcmKwEiBwYVERQXFjsBMjc2JRE0Nz4BOwEyFhcWFREUBw4BKwEiJicmEyM3MxcjNzMBvQ4gVwhXIA4OIFcIVyAO/oQSHHlICEh5HBISHHlICEh5HBKWRXF1IUVxdfcBADcfRUUfN/8ANx9FRR83AQBFLUNMTEMtRf8ARS1DTExDLQJ1tLS0AAAAAAQAN//2AhwDJwATACMAJwArAAABFRQHDgEiJicmPQE0Nz4BMhYXFgc1NCcmIgcGHQEUFxYyNzYDIzczFyM3MwHhDxZpjmkWDw8WaY5pFg9kDBqWGgwMGpYaDMRFcXUhRXF1AUtmOy1AR0dALTtmOy1AR0dALZ9iMxxCQhwzYjMcQkIcAb+0tLQAAAAAAgBBAAADKwLuABUAIQAAKQEiJicmPQE0Nz4BMyEVIRUzFSMVKQEzESMiBwYdARQXFgMr/gVNdBwSEhx0TQH7/tby8gEq/gVnZ1cgDg4gS0UtROxELUVLZ9Nn5gIgRR827DYfRQAAAAADADf/9gMrAjoAHgAsADEAADc1NDYzMhc2MzIWHQEhFR4BMzI2NzMOASMiJwYjIiY3FRQWMzI2PQE0JiMiBgUzJiMiN3debzU0cGdw/rcBPDQoOQxoEXFTbzQ0cF53ZDw1ND4+NDU8AUflBm1s23pofVlZg384JT9LLipWXVlZfd90QEdEPH4+Rkc3hAAAAAADAEwAAAJFA9sAGgAkACgAACUVIyYvAS4BKwERIxEzMhceARUUBgcVFh8BFgEjFTMyNjU0JyYnIzczAkV0GAYMBD07dWrUMiZMVTs3YQgLBf78ZmtESlAZSkd2fgICGUGJNCv+vgLuCBFhTThZGQEibIxFAmvgQzdNEwaftAAAAAIASQAAAYwDJwAPABMAAAEHJiMiBhURIxEzFzM2MzInIzczAYwDKik6T2RKDgE2ZiacR3Z+Ai9mEFZD/sACMFFbObQAAwBM/uwCRQLuABoAJAAyAAAlFSMmLwEuASsBESMRMzIXHgEVFAYHFRYfARYBIxUzMjY1NCcmAzUzFRQHDgEHNTY1NCMCRXQYBgwEPTt1atQyJkxVOzdhCAsF/vxma0RKUBk9awMGODI8CQICGUGJNCv+vgLuCBFhTThZGQEibIxFAmvgQzdNEwb83WlOFhQpOgcoFDMKAAACAET+7AGMAjoADwAdAAABByYjIgYVESMRMxczNjMyATUzFRQHDgEHNTY1NCMBjAMqKTpPZEoOATZmJv7oawMGODI8CQIvZhBWQ/7AAjBRW/0raU4WFCk6BygUMwoAAAADAEwAAAJFA+cAGgAkACsAACUVIyYvAS4BKwERIxEzMhceARUUBgcVFh8BFgEjFTMyNjU0JyYTByMnMxc3AkV0GAYMBD07dWrUMiZMVTs3YQgLBf78ZmtESlAZpZtUm1NycgICGUGJNCv+vgLuCBFhTThZGQEibIxFAmvgQzdNEwYBX7+/hIQAAgAlAAABrwMzAA8AFgAAAQcmIyIGFREjETMXMzYzMjcHIyczFzcBjAMqKTpPZEoOATZmJkubVJtTcnICL2YQVkP+wAIwUVv5v7+EhAAAAAACACv/9gIWA9sAJwArAAABIzQmIyIGFRQfARYVFAYjIiYnMx4BMzI2NTQmLwEuAzU0NjMyFiUjNzMCD2tAQEM4YkTHh2hzhwJsAk87PUxAN0MrNDgbgmVrgf78R3Z+Ahk2SDorTSEXQqJgc4FiNU0+ODA1FBgQGzBJMltpdKO0AAIAJv/2Ab8DJwAkACgAADcuAzU0NjMyFhcjJiMiBhQWFx4BFRQGIyImJzMWMzI2NzYmAyM3M+E3SB4KZ1JRaQpmFEomMS08WWFtVFlyDWYXWSszAQE2PEd2fvcNKjImFkdXT05FJjwiDhVOTU5cW1VWKSMkJgGNtAAAAAACACv/9gIWA+cAJwAuAAABIzQmIyIGFRQfARYVFAYjIiYnMx4BMzI2NTQmLwEuAzU0NjMyFgMXIycHIzcCD2tAQEM4YkTHh2hzhwJsAk87PUxAN0MrNDgbgmVrgcObU3JyU5sCGTZIOitNIRdComBzgWI1TT44MDUUGBAbMEkyW2l0AWO/hIS/AAAAAAIAJv/2Ab8DMwAkACsAADcuAzU0NjMyFhcjJiMiBhQWFx4BFRQGIyImJzMWMzI2NzYmAxcjJwcjN+E3SB4KZ1JRaQpmFEomMS08WWFtVFlyDWYXWSszAQE2B5tTcnJTm/cNKjImFkdXT05FJjwiDhVOTU5cW1VWKSMkJgJNv4SEvwAAAAEAK/8SAhYC+AA9AAABIzQmIyIGFRQfARYVFAYjBxYVFAYjIiYnMxYzMjY1NCYvASY/AS4BJzMeATMyNjU0Ji8BLgM1NDYzMhYCD2tAQEM4YkTHh2gLbDosLj8GNQ0wFhweIxkZCBBebQJsAk87PUxAN0MrNDgbgmVrgQIZNkg6K00hF0KiYHMnGkYpNDEmKRoTFBoKBwYZLg19VjVNPjgwNRQYEBswSTJbaXQAAAAAAQAm/xIBvwI6ADoAADcuAzU0NjMyFhcjJiMiBhQWFx4BFRQGDwEWFRQGIyImJzMWMzI2NTQmLwEmPwEuASczFjMyNjc2JuE3SB4KZ1JRaQpmFEomMS08WWFtUwtsOiwuPwY1DTAWHB4jGRkIEEdZCmYXWSszAQE29w0qMiYWR1dPTkUmPCIOFU5NTlsBJxpGKTQxJikaExQaCgcGGS8LWElWKSMkJgAAAgAr//YCFgPnACcALgAAASM0JiMiBhUUHwEWFRQGIyImJzMeATMyNjU0Ji8BLgM1NDYzMhYDByMnMxc3Ag9rQEBDOGJEx4doc4cCbAJPOz1MQDdDKzQ4G4Jla4Enm1SbU3JyAhk2SDorTSEXQqJgc4FiNU0+ODA1FBgQGzBJMltpdAFjv7+EhAAAAAACACb/9gG/AzMAJAArAAA3LgM1NDYzMhYXIyYjIgYUFhceARUUBiMiJiczFjMyNjc2JhMHIyczFzfhN0geCmdSUWkKZhRKJjEtPFlhbVRZcg1mF1krMwEBNpSbVJtTcnL3DSoyJhZHV09ORSY8Ig4VTk1OXFtVVikjJCYCTb+/hIQAAAACABb+7AIaAu4ABwAVAAATIRUjESMRIxM1MxUUBw4BBzU2NTQjFgIEzWrNz2sDBjgyPAkC7mf9eQKH/N5pThYUKToHKBQzCgACABT+7AFWArwAGAAmAAAlBwYjIicmPQEjNTM1MxUzFSMVFBceATMyBzUzFRQHDgEHNTY1NCMBVgEVEaAbB1lZZISEBggpJxWQawMGODI8CV9fA4QfNftgjIxg/CwWHRj4aU4WFCk6BygUMwoAAAAAAgAWAAACGgPnAAcADgAAEyEVIxEjESMBByMnMxc3FgIEzWrNAcebVJtTcnIC7mf9eQKHAWC/v4SEAAAAAgAU//0B+QMRABgAJgAAJQcGIyInJj0BIzUzNTMVMxUjFRQXHgEzMhM1MxUUBw4BBzU2NTQjAVYBFRGgGwdZWWSEhAYIKScVSmsDBjgyPAlfXwOEHzX7YIyMYPwsFh0YAktpThcTKToHKBQzCgAAAAEAFgAAAhsC7gAPAAATIRUjFTMVIxEjESM1MzUjFgIFzKuraq6uzwLuZ89G/o4BckbPAAABAB7//QFgArwAIAAAJQcGIyInJj0BIzUzNSM1MzUzFTMVIxUzFSMVFBceATMyAWABFRGgGwdZWVlZZISEhIQGCCknFV9fA4QfNUVGcGCMjGBwRkYsFh0YAAAAAAIASP/2Ai4DvQAZADAAACURMxEUBw4BKwEiJicmNREzERQXFjsBMjc2Ax4BMzI2NzMOASMiJy4BIyIHIz4BMzIBxGoSHHlICEh5HBJqDiBXCFcgDoglIRQSGwM9BToyKy0lIRQpBz0FOjEr9wH3/glFLUNMTEMtRQH3/gk3H0VFHwLcGhEfIEBJIRoRP0BJAAIARv/2AfgDCQAYAC8AAAUiJicmNREzERQXFjMyNjc2NREzESMnIwYDHgEzMjY3Mw4BIyInLgEjIgcjPgEzMgEDRVwSCmQKF0skPBENZEsNATZIJSEUEhsDPQU6MistJSEUKQc9BToxKgpLRCI5AVD+syseRC0jHCkBRf3QSVMC8hoRHyBASSEaET9ASQAAAAIASP/2Ai4DegAZAB0AACURMxEUBw4BKwEiJicmNREzERQXFjsBMjc2ExUhNQHEahIceUgISHkcEmoOIFcIVyAOEv7K9wH3/glFLUNMTEMtRQH3/gk3H0VFHwK6QUEAAAIARv/2AfgCxgAYABwAAAUiJicmNREzERQXFjMyNjc2NREzESMnIwYTFSE1AQNFXBIKZAoXSyQ8EQ1kSw0BNlL+ygpLRCI5AVD+syseRC0jHCkBRf3QSVMC0EFBAAAAAAIASP/2Ai4DygAZACUAACURMxEUBw4BKwEiJicmNREzERQXFjsBMjc2AiImJzMeATI2NzMGAcRqEhx5SAhIeRwSag4gVwhXIA4/lFsFQAY6VDoGQAX3Aff+CUUtQ0xMQy1FAff+CTcfRUUfAmNbTC41NS5MAAAAAAIARv/2AfgDFgAYACQAAAUiJicmNREzERQXFjMyNjc2NREzESMnIwYSIiYnMx4BMjY3MwYBA0VcEgpkChdLJDwRDWRLDQE2AZRbBUAGOlQ6BkAFCktEIjkBUP6zKx5ELSMcKQFF/dBJUwJ5W0wuNTUuTAAAAwBI//YCLgP9ABkAIQAqAAAlETMRFAcOASsBIiYnJjURMxEUFxY7ATI3NgI0NjIWFAYiJhQWMjY1NCYiAcRqEhx5SAhIeRwSag4gVwhXIA77Ql5DQ14TJjgnJzj3Aff+CUUtQ0xMQy1FAff+CTcfRUUfApxeQ0NeQo46JygcHSgAAAAAAwBG//YB+ANJABgAIAApAAAFIiYnJjURMxEUFxYzMjY3NjURMxEjJyMGAjQ2MhYUBiImFBYyNjU0JiIBA0VcEgpkChdLJDwRDWRLDQE2ukJeQ0NeEyY4Jyc4CktEIjkBUP6zKx5ELSMcKQFF/dBJUwKyXkNDXkKOOicoHB0oAAADAEj/9gJLA9sAGQAdACEAACURMxEUBw4BKwEiJicmNREzERQXFjsBMjc2AyM3MxcjNzMBxGoSHHlICEh5HBJqDiBXCFcgDtxFcXUhRXF19wH3/glFLUNMTEMtRQH3/gk3H0VFHwJntLS0AAADAEb/9gIpAycAGAAcACAAAAUiJicmNREzERQXFjMyNjc2NREzESMnIwYDIzczFyM3MwEDRVwSCmQKF0skPBENZEsNATajRXF1IUVxdQpLRCI5AVD+syseRC0jHCkBRf3QSVMCfbS0tAAAAAABAEj/HQIuAu4AKQAAJREzERQHBgcGBw4BFRQWMzI3FwYjIiYnJjcuAScmNREzERQXFjsBMjc2AcRqEiBLHQc5NSAcISMTLi41RAEDWkd3GxJqDiBXCFcgDvcB9/4JRS1NJw4FJTchGR0SKBgwLUY2AktCLUUB9/4JNx9FRR8AAAEARv8dAhkCMAAnAAAFIiYnJjURMxEUFxYzMjY3NjURMxEOARUUFjMyNxcGIyImJyY3JyMGAQNFXBIKZAoXSyQ8EQ1kOzcgHCIiEy4uNUQBA20NATYKS0QiOQFQ/rMrHkQtIxwpAUX90CU5IRkdEigYMC1NOkhTAAIAGgAAA1YD5wAPABYAAAEzEzMTMwMjAyMDIwMzEzMTFyMnByM3AXp7gAJucZiLewJ5jJdwbgLom1NyclObAu79mAJo/RICOf3HAu79mANhv4SEvwAAAAACABEAAALjAzMADwAWAAABMwMjAyMDIwMzEzMTMxMzAxcjJwcjNwJ4a5RqagJqa5NqYgJtW20CcptTcnJTmwIw/dABi/51AjD+YwGd/mQCn7+EhL8AAAAAAgADAAACQwPnAAkAEAAAARMzAxEjEQMzGwEXIycHIzcBJKd462rreKcrm1NyclObAbcBN/5W/rwBRAGq/skCML+EhL8AAAAAAgAJ/ygB8wMzABEAGAAAFzI2PwEDMxMzEzMDBiMiJzUWExcjJwcjN00mLA8ewWuKAodq6jN9LCQs85tTcnJTm3gpKFACB/52AYr9g4sLXwoDq7+EhL8AAAMAAwAAAkMDpwAJAA0AEQAAARMzAxEjEQMzEwMjNTMXIzUzASSneOtq63inLmlpx2lpAbcBN/5W/rwBRAGq/skBgW9vbwAAAAIALQAAAhsD2wAJAA0AABM1IRUBIRUhNQEnIzczOwHa/pkBbf4SAWR3R3Z+AodnXf3WZ14CKaC0AAAAAAIAIwAAAc8DJwAJAA0AACkBNQEhNSEVASEDIzczAc/+VAEd/u0BkP7kAS7rR3Z+VQF7YFb+hgITtAAAAAIALQAAAhsDqQAJAA0AABM1IRUBIRUhNQEnIzUzOwHa/pkBbf4SAWQ1b28Ch2dd/dZnXgIpsHIAAgAjAAABzwL1AAkADQAAKQE1ASE1IRUBIQMjNTMBz/5UAR3+7QGQ/uQBLp5vb1UBe2BW/oYCI3IAAAAAAgAtAAACGwPnAAkAEAAAEzUhFQEhFSE1ARMHIyczFzc7Adr+mQFt/hIBZFibVJtTcnICh2dd/dZnXgIpAWC/v4SEAAACACMAAAHPAzMACQAQAAApATUBITUhFQEhAwcjJzMXNwHP/lQBHf7tAZD+5AEuEZtUm1NyclUBe2BW/oYC07+/hIQAAAEAFAAAAWQDHAARAAATESMRIzUzNTQ2MzIXFSYjIgbRZFlZaVwaGBQXNzECUP2wAdBgIWdkA2ADLwAAAAUAKAAAAhwC7gADAAYACQAMAA8AAAERIRETAxEbARElGwELASECHP4M3avlq/6OqqqqqQFRAu79EgLu/ogBGf3OARn+5wIyLf7pARf+iv7sAAAAAAH/xP8TALYCMAANAAAXETMRFAYjIic1FjMyNlJkaVwZFBgONzEhAlH9rmdkAmACLwAAAAABADUCkgG/A1EABgAAARcjJwcjNwEkm1NyclObA1G/hIS/AAABADUCkgG/A1EABgAAAQcjJzMXNwG/m1SbU3JyA1G/v4SEAAABAFACjQGkAzQACwAAACImJzMeATI2NzMGAUSUWwVABjpUOgZABQKNW0wuNTUuTAABAMICoQExAxMAAwAAASM1MwExb28CoXIAAAAAAgCJAoQBbANnAAcAEAAAEjQ2MhYUBiImFBYyNjU0JiKJQl5DQ14TJjgnJzgCxl5DQ15CjjonKBwdKAAAAQCK/x0BYwASABAAACUXDgEVFBYzMjcXBiMiJicmARonOzYgHCIiEy4uNUQBAxISJjghGR0SKBgwLVgAAQAyApEBwgMnABYAABMeATMyNjczDgEjIicuASMiByM+ATMy+yUhFBIbAz0FOjIrLSUhFCkHPQU6MSoDBhoRHyBASSEaET9ASQAAAAIAVQKRAf0DRQADAAcAABMjNzMXIzczmkVxdSFFcXUCkbS0tAAAAAAFACgAAAIcAu4AAwAGAAkADAAPAAABESEREwMRGwERJRsBCwEhAhz+DN2r5av+jqqqqqkBUQLu/RIC7v6IARn9zgEZ/ucCMi3+6QEX/or+7AAAAAACAEwAAAIDA9sACwAPAAAzESEVIRUhFSEVIRUDIyczTAG3/rMBE/7tAU2AR61+Au5n02fmZwMntAAAAAADAEwAAAIDA6cACwAPABMAADMRIRUhFSEVIRUhFQEjNTMXIzUzTAG3/rMBE/7tAU3+9Wlpx2lpAu5n02fmZwM4b29vAAEAFgAAAqwC7gAhAAAhIzUzMjc+ATU0JicmKwERIxEjNSEVIxUzMhceARUUBgcGAa8ICDodHCAgHB06iGqnAcm4ikYnQkxMQidnERA5ISA3EBH+pgKHZ2fGDxhwSEpxGA8AAAACAEwAAAH7A9sABQAJAAAzESEVIRETIzczTAGv/rtcR3Z+Au5n/XkDJ7QAAAEAQf/2AisC+AAvAAAlNjczBgcOASsBIiYnJjURNDc+ATsBMhYXFhcjJicmKwEiBwYdATMVIxUUFxY7ATIBtAgDbAULGXtKDUh5HBISHHlIDUl6GwsEbAMHHlkNVyAO8fEOIFcNV6IUFiodREtMQy1FAQBFLUNMTEMdJBUPRkYfNkNnVjYfRQABACv/9gIWAvgAJwAAASM0JiMiBhUUHwEWFRQGIyImJzMeATMyNjU0Ji8BLgM1NDYzMhYCD2tAQEM4YkTHh2hzhwJsAk87PUxAN0MrNDgbgmVrgQIZNkg6K00hF0KiYHOBYjVNPjgwNRQYEBswSTJbaXQAAAEATAAAALYC7gADAAAzETMRTGoC7v0SAAP/6QAAARkDpwADAAcACwAAMxEzEQMjNTMXIzUzTGpkaWnHaWkC7v0SAzhvb28AAAEALf/2AdkC7gAVAAATFRQXFjI3NjURMxEUBw4BIiYnJj0BlxIfdh8SahUZZYZlGRUBDzEuHzU1Hy8CD/32QTI5QkI5MkAsAAIACAAAA4cC7gAdACgAAAEzMhceARUUBgcGKwERIwMOBAc1PgM3EyETIxUzMjc2NTQnJgIccEYnQkxMQidG2rERAwoeL1Q6JTIaCwMUAYFubm46HTw8HQHLDxhxTE1zGA8Ch/7XOE5lPzEDZwIrTks1AYz+dv0RIk1KIhEAAAAAAgBMAAADcQLuABYAIQAAMxEzETMRMxEzMhceARUUBgcGKwERIxEBIxUzMjc2NTQnJkxq5mpwRidCTExCJ0ba5gG+bm46HTw8HQLu/t0BI/7dDxhxTE1zGA8BZP6cAWT9ESJNSiIRAAABABYAAAKeAu4AFgAAARUjFTMyFhcWHQEjNTQnJisBESMRIzUB37h/a3gQBWoEEnl+aqcC7mfGY1ggMbW1MBNi/qYCh2cAAAACAEwAAAJsA9sAGwAfAAAzETMRMzI2PwE+ATMyFxUmIyIGDwEGBxMjAyMREyM3M0xqVj85DAsPPj4cGxEOHR0GCRhM23zFdYdHdn4C7v7MMz42S0YEYAMdIzGFKv6PAVP+rQMntAAAAAIATAAAAjwD2wAPABMAAAEzBhURMxEjASM2NREjETMTIyczAdIBAWp+/vgBAWp/1UetfgI/IRn9+wLu/cEhGQIF/RIDJ7QAAAIAEP/2Ai8DygARAB0AADcyNjcDMxMzEzMDDgEjIic1FgAiJiczHgEyNjczBoEuLhLfcKUCmW/eH15HNSQsAQ2QXgRGBTRWNAVGBFgpMQI8/kUBu/2lU0oLYQoCy1tMMDk5MEwAAAAAAQBM/wwCNgLuAAsAAAERIxUjNSMRMxEhEQI2w2TDagEWAu79EvT0Au79eQKHAAAAAgAMAAACSALuAAMACwAAAQMjAxcjByMTMxMjAYJXBFXM6D1t0pjSbQFJATz+xGXkAu79EgAAAAACAEwAAAIrAu4AEAAbAAATMzIXHgEVFAYHBisBESEVIRMjFTMyNzY1NCcmtnpGJ0JMTEInRuQBp/7DeHh4Oh08PB0Byw8YcUxNcxgPAu5n/t39ESJNSiIRAAAAAAMATAAAAjAC7gARABwAKAAAMxEzMhceARUUBx4BFRQGBwYjAyMVMzI3NjU0JyYDIxUzMjc+ATU0JyZM10UoQEhcODxLQShCB32GMho9Rxs5dHMqGR4lQhcC7gwUXUZpOBdhOEhtFw4BT+kOH0dMHgsBOdMLDTckQRcIAAABAEwAAAH7Au4ABQAAMxEhFSERTAGv/rsC7mf9eQAAAAIADP8MAtwC7gANABQAACURIzUhFSMRMzI3EyERAQMOAQchEQLcZP34ZBt4DRQBp/6/EQYdJwEyZ/6l9PQBW/sBjP15AiD+11h3KAIgAAAAAQBMAAACAwLuAAsAADMRIRUhFSEVIRUhFUwBt/6zARP+7QFNAu5n02fmZwABAAsAAAO5AvIAMwAAATMRMzI2PwE+ATMyFxUmIyIGDwEGBxMjAyMRIxEjAyMTJi8BLgEjIgc1NjMyFh8BHgE7AQGtakI/OQwLDz4+HBsRDh0dBgkYTNt8xWFqYcV820wYCQcdHQ4QGxs+Pw8LDDk/QgLu/swzPjZLRgRgAx0jMYUq/o8BU/6tAVP+rQFxKoUxIx0DYARGSzY+MwABACn/9gINAvgAKAAAARUeARUUBiMiJjUzHgEzMjY1NCYrATUzMjY1NCYjIgYHIzQ2MzIWFRQBljs8jGZvg2kBR0E8TFFPV1dOTUk6QUcBaYNvZIkBewEcYDpYdnZfNjs+NjRKXUY0Njs7Nl92c1h7AAAAAAEATAAAAjwC7gANAAABIwEjETMRBzMBMxEjEQHTAf75f2oBAQEIfmoCP/3BAu79+zoCP/0SAgUAAgBMAAACPAPKAA8AGwAAATMGFREzESMBIzY1ESMRMxIiJiczHgEyNjczBgHSAQFqfv74AQFqf8CQXgRGBTRWNAVGBAI/IRn9+wLu/cEhGQIF/RIDI1tMMDk5MEwAAAAAAQBMAAACbALyABsAADMRMxEzMjY/AT4BMzIXFSYjIgYPAQYHEyMDIxFMalY/OQwLDz4+HBsRDh0dBgkYTNt8xXUC7v7MMz42S0YEYAMdIzGFKv6PAVP+rQAAAAABAAgAAAJKAu4AEgAAEyERIxEjAw4EBzU+AzebAa9q3xEDCh4vVDolMhoLAwLu/RICh/7XOE5lPzEDZwIrTks1AAAAAAEATAAAAsAC7gARAAABAyMDIxYVESMRMxsBMxEjETcCWqtSqwEBZn+7u39mAQI2/lwBpCEZ/gQC7v40Acz9EgH8OgAAAQBMAAACPQLuAAsAADMRMxEhETMRIxEhEUxqAR1qav7jAu7+zAE0/RIBU/6tAAAAAgBB//YCJwL4ABMAKwAAJRE0JyYrASIHBhURFBcWOwEyNzYlETQ3PgE7ATIWFxYVERQHDgErASImJyYBvQ4gVwhXIA4OIFcIVyAO/oQSHHlICEh5HBISHHlICEh5HBL3AQA3H0VFHzf/ADcfRUUfNwEARS1DTExDLUX/AEUtQ0xMQy0AAQBMAAACNQLuAAcAADMRIREjESERTAHpav7rAu79EgKH/XkAAgBMAAACKwLuAAoAGQAAEzMyNzY1NCcmKwETIxEjETMyFx4BFRQGBwa2eDodPDwdOnh6emrkRidCTExCJwGKESJKTSIR/pz+3QLuDxhzTUxxGA8AAQBB//YCKAL4ACsAACU2NTMGBw4BKwEiJicmNRE0Nz4BOwEyFhcWFyMmJyYrASIHBhURFBcWOwEyAa8OawERG3pJCEh5HBISHHlICEl6GxEBawIMIFcIVyAODiBXCFeiITFDLERLTEMtRQEARS1DTExDKj8wHEZGHzb/ADYfRQABABYAAAIaAu4ABwAAEyEVIxEjESMWAgTNas0C7mf9eQKHAAABABD/9gIvAu4AEQAANzI2NwMzEzMTMwMOASMiJzUWgS4uEt9wpQKZb94fXkc1JCxYKTECPP5FAbv9pVNKC2EKAAAAAAMAMP/YAtQDFgAfACsANwAABTUjIiYnJj0BNDc+ATsBNTMVMzIWFxYdARQHDgErARUTNTQnJisBETMyNzYFFjsBESMiBwYdARQBTSRPfBwSEhx8TyRqJE98HBISHHxPJLMOIGEkJGEgDv4+IGEkJGEgDiiCS0QtRThFLURLgoJLRC1FOEUtREuCAYM4Nx9F/pJFHx9FAW5FHzc4NwAAAAABAAkAAAI/Au4ACwAAMyMTAzMbATMDEyMDgnniy3qKinrL4nmiAYYBaP75AQf+mP56ASQAAQBM/wwCqgLuAAsAACURIzUhETMRIREzEQKqZP4GagEVamf+pfQC7v15Aof9eQAAAQA8AAACIwLuABQAACERBiMiJicmPQEzFRQXFjMyNxEzEQG5RVJfcw8FagQSeUg8agEjC2NXIDHLyzATYgkBZ/0SAAABAEwAAAMUAu4ACwAAAREhETMRMxEzETMRAxT9OGrFasUC7v0SAu79eQKH/XkChwABAEz/DAOJAu4ADwAAKQERMxEzETMRMxEzETMRIwMl/SdqxWrFanVkAu79eQKH/XkCh/15/qUAAAIAFgAAAs4C7gAQABsAAAEzMhceARUUBgcGKwERIzUhEyMVMzI3NjU0JyYBT4RGJ0JMTEInRu7PATmCgoI6HTw8HQHLDxhxTE1zGA8Ch2f+dv0RIk1KIhEAAAAAAwBMAAAC3ALuAA4AGQAdAAATMzIXHgEVFAYHBisBETMTIxUzMjc2NTQnJgERMxG2ZkYnQkxMQidG0GpkZGQ6HTw8HQEeagHLDxhxTE1zGA8C7v52/REiTUoiEf6cAu79EgAAAAIATAAAAjUC7gAOABkAABMzMhceARUUBgcGKwERMxMjFTMyNzY1NCcmtoRGJ0JMTEInRu5qgoKCOh08PB0Byw8YcUxNcxgPAu7+dv0RIk1KIhEAAAEAJP/2Ag4C+AAvAAA3FjsBMjc2PQEjNTM1NCcmKwEiBwYHIzY3PgE7ATIWFxYVERQHDgErASImJyYnMxabIFcNVyAO8fEOIFcNWR4HA2wECxt6SQ1IeRwSEhx5SA1KexkLBWwDokVFHzZWZ0M2H0ZGDxUkHUNMTEMtRf8ARS1DTEtEHSoWAAACAEz/9gMeAvgAGwArAAAEIiYnJj0BIxEjETMRMzU0Nz4BMhYXFhURFAcGAiIHBhURFBcWMjc2NRE0JwJ3kHkcEopqaooSHHmQeRwSEhxqriAODiCuIA4OCkxDLUVc/q0C7v7MPUUtQ0xMQy1F/wBFLUMCUEUfN/8ANx9FRR83AQA3HwACABkAAAISAu4AGgAkAAAzNTY/ATY3NS4BNTQ2NzY7AREjESMiBg8BBgcBIyIHBhUUFjsBGSUFCwhhNztVTCYy1Gp1Oz0EDAYYARtmKhlQSkRrAhtFjGwiARlZOE1hEQj9EgFCKzSJQRkCiAYTTTdDAAAAAAIAMv/2AdYCOgAXACIAABMjPgEzMhURIycjBiMiJjU0NjsBNTQjIgMUFjMyNj0BIyIGsGgPZ1LGRwwBOF5VZWpdeWNJMDIyMEh4LTcBoEdTyP6OSFJjV09hG2T+0i0zUSo9LwACADr/9gHkAxUAGwAqAAABFRQHDgEiJicmNRE0Nj8BFQcOAR0BNjMyFhcWBzU0JiMiBh0BFBcWMjc2AeQPFmmOaRYPd4SQfVRWM1JGYBIJZDUyOkEMGpYaDAE3UjstQEdHQC07AQGGig8QXQ8LV1kZPUtBH4BOO0JUPTozHEJCHAAAAAADAEkAAAHlAjAAEQAcACcAADMRMzIXHgEVFAceARUUBgcGIycjFTMyNzY1NCcmJyMVMzI3NjU0JyZJvCYhO0tJLDBBOCE2BGRrIhQxOhYlXVoiFi8wEQIwBQpFPlAoFEUnPFAQCvKWBxA0OA4F4oYIEi8tDAQAAAABAEkAAAGvAjAABQAAMxEhFSERSQFm/v4CMGD+MAAAAAIACP84AmECMAAQABYAACURIzUhFSMRMzI+AjcTIREBBwYHMxECYWD+Z2AcHCYUCgIRAWr+9g0HNO5g/tjIyAEoHTU1IwEm/jABcMtvNgFwAAAAAAIAN//2AeICOgAbACMAABIyFhcWHQEhFRQWMzI2NzMOASMiJicmPQE0NzYXBgczJicmIsaOaRYP/rk9NCg5DGcRcVJHaRYPDxZLCwHkAQsamAI6R0AtO0oiQU0tKFVbR0AtO2Y7LUBWGigoGkIAAAABABQAAAMcAjUAMwAAARUzMjY/AT4BMzIXFSYjIg8BDgEHEyMnIxUjNSMHIxMuAS8BJiMiBzU2MzIWHwEeATsBNQHKLC0pCQoONjYfGBcNJgkHCiMgs3afPWQ9n3azICMKBwkmDRcYHzY2DgoJKS0sAjDgIykqODcFVgUqJDI8Ev7v9vb29gEREjwyJCoFVgU3OCopI+AAAAEAJf/2AcACOgAmAAABFAcWFRQGIyImNTMUFjMyNjU0JisBNTMyNjU0JiMiBhUjNDYzMhYBvFtfbVtgc2M7NS42OTpFRTo1My01O2NzYFVvAaRcLSlbQ15gSyQpKCIhMFgtISIlJyVMXlQAAAEASQAAAe4CMAAPAAABIwMjETMRBzM2EjczESMRAYsBz3JkAQEckSNxZAGT/m0CMP6nOjcBGET90AFZAAIASQAAAe4DFgARAB0AAAEzBhURMxEjBgIHIzY1ESMRMxIiJiczHgEyNjczBgGKAQFkcSORHAEBZHKokF4ERgU0VjQFRgQBkyEZ/qcCMET+6DchGQFZ/dACb1tMMDk5MEwAAAABAEkAAAIRAjUAGwAAMxEzFTMyNj8BPgEzMhcVJiMiDwEOAQcTIycjFUlkPi0pCQoONjYfGBcNJgkHCiMgs3afTwIw4CMpKjg3BVYFKiQyPBL+7/b2AAAAAQAIAAAB7QIwABEAABMhESMRIwcOBAc1PgI3fQFwZK0LAwgaKUkyJi4OAwIw/dAB0MspO0swIwNfAjdBMQAAAAEASQAAAksCMAASAAAlKwEDIxYVESMRMxsBMxEjETcjAW8BSXwBAWBxkJBxYAEBaQEZIRn+uAIw/rkBR/3QAUg6AAAAAQBJAAAB7wIwAAsAADMRMxUzNTMRIzUjFUlk3mRk3gIw3d390PPzAAIAN//2AeECOgATACMAAAEVFAcOASImJyY9ATQ3PgEyFhcWBzU0JyYiBwYdARQXFjI3NgHhDxZpjmkWDw8WaY5pFg9kDBqWGgwMGpYaDAFLZjstQEdHQC07ZjstQEdHQC2fYjMcQkIcM2IzHEJCHAABAEkAAAHoAjAABwAAMxEhESMRIxFJAZ9k1wIw/dAB0P4wAAACAEn/DAH9AjoAEwArAAATFRQXHgEzMjc2PQE0JyYjIgYHBhMiJyMWHQEjETMXMzYzMhYXFh0BFAcOAa0IDTsqUBkJChdMJD0RDYlUNQEBZEsNATZjRmASCgkSZQFFVyQaKDJMGitmKx5ELSMc/og6IRnqAyRJU0tEIjloNyRJTgAAAAABADf/9gHgAjoAJwAAJTY3MwYHDgEjIiYnJj0BNDc+ATMyFhcWFyMmJyYjIgcGHQEUFxYzMgFxBQNmBQcWaUhHaRYPDxZpR0hpFggFZgMGGUxLGgwMGktMlgwOHxY/RkdALTtmOy1AR0Y/GSYVD0BCHDNiMxxCAAABABQAAAHKAjAABwAAEyEVIxEjESMUAbapZKkCMF/+LwHRAAABAAn/KAHzAjAAEQAAFzI2PwEDMxMzEzMDBiMiJzUWTSYsDx7Ba4oCh2rqM30sJCx4KShQAgf+dgGK/YOLC18KAAAAAAMAN/8MAmsDEQAfACsANwAANzU0Nz4BOwE1MxUzMhYXFh0BFAcOASsBFSM1IyImJyY3FRQXFjsBESMiBwYFNj0BNCcmKwERMzI3DxZsTg5aDk5sFg8PFmxODloOTmwWD2QMGlUODlUaDAFgDAwaVQ4OVeVmOy1AR9fXR0AtO2Y7LUBH6upHQC2fYjMcQgGEQhzkHDNiMxxC/nwAAAEADAAAAfoCMAALAAATMxc3MwMTIycHIxMcc3RzdbLBcoWFcsECMLa2/vb+2tHRASYAAAABAEn/OAJHAjAACwAAKQERMxEzETMRMxEjAef+YmTWZGBgAjD+MAHQ/jD+2AAAAAABACwAAAHJAjAAFAAAITUGIyImJyY9ATMVFBcWMzI3ETMRAWU4N1JmDQVkBRJYOC5k0AdOQRYlnZwcEj4HAQH90AAAAAEASQAAAqMCMAALAAABESERMxEzETMRMxECo/2mZJdklwIw/dACMP4wAdD+MAHQAAEASf84AwMCMAAPAAApAREzETMRMxEzETMRMxEjAqP9pmSXZJdkYGACMP4wAdD+MAHQ/jD+2AAAAgAUAAACTgIwAA8AGQAAARUzMhceARQGBwYrAREjNQEVMzI3NjQnJiMBHWE0JjZAQDYmNMWlAQlgKxcrKxcrAjDMDBJYeFgSDAHRX/7WqAwWZBYMAAAAAAMASQAAAn0CMAADABEAGwAAIREzEQEVMzIXHgEUBgcGKwERExUzMjc2NCcmIwIZZP4wVzQmNkBANiY0u2RWKxcrKxYsAjD90AIwzAwSWHhYEgwCMP7WqAwWZBYMAAAAAAIASQAAAegCMAANABcAABMVMzIXHgEUBgcGKwERExUzMjc2NCcmI61rNCY2QEA2JjTPZGorFysrFysCMMwMElh4WBIMAjD+1qgMFmQWDAAAAQAr//YB1gI6ACQAACUyNzY9ASM1MzU0JyYjIgYHIz4BMzIWFxYdARQHDgEjIiYnMxYBAUsaDLu7DBpLMDoGZgxuXEdpFg8PFmlHWHILZhJWQhwzAl0DMxxCLytaYEdALTtmOy1AR2BQUAAAAgBJ//YCoAI6ABsAKwAAMxEzFTM1NDc+ATIWFxYdARQHDgEiJicmPQEjFSU1NCcmIgcGHQEUFxYyNzZJZGMOFGOGYxQODhRjhmMUDmMBjwsXhBcLCxeEFwsCMOgDPipAR0dAKj5mPipAR0dAKj4D6OdiMR5CQh4xYjEeQkIeAAACABIAAAHAAjAAGgAjAAAzNTY/AT4BNy4BNTQ2NzY7AREjNSMiBg8BBgcTIyIGFRQWOwESHgYHBCInKCxGPRwvvGRqKSYCBgUb4WAoNzAuYQIWQ1AsMg0TRic8TAwG/dDoHyJPPhoB0iAkIiYAAAAAAwA3//YB4gMnABsAIwAnAAASMhYXFh0BIRUUFjMyNjczDgEjIiYnJj0BNDc2FwYHMyYnJiI3Iyczxo5pFg/+uT00KDkMZxFxUkdpFg8PFksLAeQBCxqYp0etfgI6R0AtO0oiQU0tKFVbR0AtO2Y7LUBWGigoGkKUtAAAAAQAN//2AeIC8wAbACMAJwArAAASMhYXFh0BIRUUFjMyNjczDgEjIiYnJj0BNDc2FwYHMyYnJiI3IzUzFyM1M8aOaRYP/rk9NCg5DGcRcVJHaRYPDxZLCwHkAQsamBxpacdpaQI6R0AtO0oiQU0tKFVbR0AtO2Y7LUBWGigoGkKlb29vAAH/9v8TAggDEQArAAABERQGIyInNRYzMjY1ETQnJiMiBgcGFREjESM1MzUzFTMVIxUHMzYzMhYXFgIIaVwZFBgONzEKF0wkPRENZF5eZKysAQE0XkVdEgoBUP6OZ2QCYAIvPQFuKx5ELSMcKf67AoNGSEhGVzpIS0QiAAACAEkAAAGvAycABQAJAAAzESEVIRETIzczSQFm/v40R3Z+AjBg/jACc7QAAAEAK//2AdYCOgAkAAAlMjczDgEjIiYnJj0BNDc+ATMyFhcjLgEjIgcGHQEzFSMVFBcWAQBdEmYLclhHaRYPDxZpR1xuDGYGOjBLGgy7uwwaVlBQYEdALTtmOy1AR2BaKy9CHDMDXQIzHEIAAAEAJv/2Ab8COgAkAAA3LgM1NDYzMhYXIyYjIgYUFhceARUUBiMiJiczFjMyNjc2JuE3SB4KZ1JRaQpmFEomMS08WWFtVFlyDWYXWSszAQE29w0qMiYWR1dPTkUmPCIOFU5NTlxbVVYpIyQmAAIARgAAALAC+AADAAcAADMRMxEDNTMVSWRnagIw/dACh3FxAAP/4wAAARMC8wADAAcACwAAMxEzEQMjNTMXIzUzSWRhaWnHaWkCMP3QAoRvb28AAAL/xP8TALkC+AANABEAABcRMxEUBiMiJzUWMzI2AzUzFVJkaVwZFBgONzEDaiECUf2uZ2QCYAIvAuVxcQAAAAACAAgAAALrAjAAGwAlAAATIRUzMhceARQGBwYrAREjBw4EBzU+AjcFFTMyNzY0JyYjfQFHVzQmNkBANiY0u4QLAgkaKUkyJi4OAwFXViwWKysWLAIwzAwSWHhYEgwB0MsoPEoxIwNfAjdBMQSoDBZkFgwAAAIASQAAAuQCMAAVAB8AADMRMxUzNTMVMzIXHgEUBgcGKwERIxEBFTMyNzY0JyYjSWSsZFc0JjZAQDYmNLusARBWKxcrKxcrAjDMzMwMElh4WBIMAQT+/AEGqAwWZBYMAAH/9gAAAggDEQAhAAAhIxE0JyYjIgYHBhURIxEjNTM1MxUzFSMVBzM2MzIWFxYVAghkChdMJD0RDWReXmSsrAEBNF5FXRIKAU0rHkQtIxwp/rsCg0ZISEZXOkhLRCI5AAIASQAAAhEDJwAbAB8AADMRMxUzMjY/AT4BMzIXFSYjIg8BDgEHEyMnIxUTIzczSWQ+LSkJCg42Nh8YFw0mCQcKIyCzdp9Pb0d2fgIw4CMpKjg3BVYFKiQyPBL+7/b2AnO0AAACAEkAAAHuAycAEQAVAAABMwYVETMRIwYCByM2NREjETMTIyczAYoBAWRxI5EcAQFkcsNHrX4BkyEZ/qcCMET+6DchGQFZ/dACc7QAAgAJ/ygB8wMWABEAHQAAFzI2PwEDMxMzEzMDBiMiJzUWACImJzMeATI2NzMGTSYsDx7Ba4oCh2rqM30sJCwBFpBeBEYFNFY0BUYEeCkoUAIH/nYBiv2DiwtfCgLnW0wwOTkwTAAAAAABAEn/OAHnAjAACwAAISMVIzUjETMRMxEzAeefYJ9k1mTIyAIw/jAB0AAAAAEAPP/2AxgC7gAmAAABNTMVFBYzMjY1NCYnMx4BFRQGIyInBiMiJjU0NjczDgEVFBYzMjYBdWoyLy4+IhttHCB2V3gpKXhXdiAcbRsiPi4vMgEd4uJdZJWcUMZLSMJW08WHh8XTVsJIS8ZQnJVkAAABADf/9gKMAjAAIQAAJTUzFRQzMjY1NCczFhUUBiMiJwYjIiY1NDczDgEVFBYzMgEvZEYgLjVnM2NIWiYmWkdjM2cYHS0hReClpYpma3yNeY+elGdnlJ6TdTuUOmxlAAACABAAAAKpAu4AFgAhAAATMzUzFTMVIxUzMhceARUUBgcGKwERIwUjFTMyNzY1NCcmELBqy8uERidCTExCJ0busAGcgoI6HTw8HQJ2eHhGZQ8YcUxNcxgPAjDM/REiTUoiEQAAAgAMAAACMQIwABUAHwAAExUzMhceARQGBwYrAREjNTM1MxUzFQcVMzI3NjQnJiP2azQmNkBANiY0z4aGZKKiaiwWKysWLAGiPgwSWHhYEgwBokZISEacqAwWZBYMAAAAAQBM//YDFgL4ADcAACU2NzMGBw4BKwEiJicmPQEjESMRMxEzNTQ3PgE7ATIWFxYXIyYnJisBIgcGHQEzFSMVFBcWOwEyAp8IA2wFCxl7SgNIeRwSgGpqgBIceUgDSXobCwRsAwceWQNXIA7n5w4gVwNXohQWKh1ES0xDLUVc/q0C7v7MPUUtQ0xMQx0kFQ9GRh82PWdcNh9FAAAAAQBJ//YCtQI6ACwAACUyNzMOASMiJicmPQEjFSMRMxUzNTQ3PgEzMhYXIy4BIyIHBh0BMxUjFRQXFgHjWRJmC3FVRWcWD2VkZGUPFmdFWmwMZgY5LUcaDLOzDBpWUFBgSD8tOwTpAjDqBTstP0hgWisvQhwzA10CMxxCAAIABAAAAoYC7gALAA8AAAERIxEjAyMTMxMjCwEjAzMBc1xLXWv4kvhrXXUIV7UBIP7gASD+4ALu/RIBIAFq/vUAAAAAAgAHAAACKQIwAAsADwAAJRUjNSMHIxMzEyMvAiMHAUFSOkllzYjNZUkcRQRFz8/PzwIw/dDPU8HBAAAAAgBMAAADgALuABMAFwAAAREjESMDIxMjESMRMxEzEzMTIwsBIwMzAm1cS11rX6dqasZ6kvhrXXUIV7UBIP7gASD+4AEg/uAC7v6RAW/9EgEgAWr+9QAAAAIASQAAAwUCMAATABcAACUVIzUjByM3IxUjETMRMxMzEyMvAiMHAh1SOkllTIJkZKBjiM1lSRxFBEXPz8/Pz88CMP7yAQ790M9TwcEAAAAAAgA4AAAClALuAB0AIQAAISMRIyIHBh0BIzU0NzY3AyEDFhcWHQEjNTQnJisBEyMXMwGYZBBZIQ5kES2CvAJUvIItEWQOIVkQSvh7AgFcRx83v8FHK28XATX+yxdvK0fBvzcfRwEy0AACADYAAAI0AjAAAgAgAAABNyMTIzUjIgcGHQEjNTQ3NjcnIQcWFxYdASM1NCcmKwEBNVmyimIOQBcKXQ8kZJkB/plkJA9dChdADgFSif4l/C4UJpSXNiFKFOTkFEohNpeUJhQuAAAAAAIATAAAA5AC7gAkACgAACEjESMiBwYdASM1NDcGKwERIxEzESEDIQMWFxYdASM1NCcmKwETIxczApRkEFkhDmQ1FxmDamoBPbsCVLyCLRFkDiFZEEr4ewIBXEcfN7/BZTkD/qQC7v7NATP+yxdvK0fBvzcfRwEy0AAAAAACAEkAAAMKAjAAJAAnAAAhIzUjIgcGHQEjNTQ3BisBFSMRMxUzJyEHFhcWHQEjNTQnJisBJzcjAjxiDkAXCl0tHhtVZGT3mAH+mWQkD10KF0AOMVmy/C4UJpSXRyIE/AIw4+PkFEohNpeUJhQuVokAAQAb/w0CDQO2AEsAAAE3NjM6ATMXFSMiJiMiDwEeARUUBxUeARUUBg8BDgEVFBYzMjYzMhcVJiMiBiMiJjU0PwE+ATU0JisBNTMyNjU0JiMiBgcjNDY3JzMBHEUoOwUNBQUDAwoEHxtNVW5yOzx9aDlBMS8iHIU1CRYaCTB0Lk5lyDg+SlFPV1dOTUk6QUcBaWpdmlgDJFw2ATsBImUMbk57NwEcYDpXaQkFBichHSQvAl8CKVFLmRIFBjk1NEpdRjQ2Ozs2VXILvgAAAQAS/0IBswLMAEoAACUUBg8BBhUUFjMyNjMyFjMVIiYjIgYjIiY1ND8BPgE1NCYrATUzMjY1NCYjIgYVIzQ2NyczFzc2MzoBMxcVIyImIyIPAR4BFRQHFgGzaFYzUyIXG3YmBhECAhUGInYbQFamMiw5OTpFRTo1My01OmRRSHlYVC0oOwUNBQUDAwoEHxstRVVbX5dDUgYEBy4TGSEBWQIeRT2CDAMDJiAhMFgtISIlJyU/WQ2TbTs2ATsBIjgLTzlcLSkAAAABADwAAAK2Au4AIQAAIREjIiYnJj0BMxUUFxY7AREzETMyNzY9ATMVFAcOASsBEQFHI1JtGBFkDiFZH2QfWSEOZBEYbVIjARxKPCpI2tg3H0cBdf6LRx832NpIKjxK/uQAAAAAAQBA/wwCeAMRACQAAAEzFhUUBisBFSM1IyImJyY1ETMRFBcWOwERMxEzMj4CNTQmJwH8ZBhlfgxeDE5sFg9kDBpVDF4MKjQZCAwGAjB5eKmg6upHQC07AUv+tzMcQgK7/UUgRUs7J3cpAAAAAwBB//YCJwL4ABcAKwA/AAA3ETQ3PgE7ATIWFxYVERQHDgErASImJyYlNQYjIi4BIyIGBxUUFxY7ATI3NgMyHgEzMjY3NTQnJisBIgcGHQE2QRIceUgISHkcEhIceUgISHkcEgF8FTMeMy0XEx8DDiBXCFcgDsseNC0XEx8DDiBXCFcgDhb3AQBFLUNMTEMtRf8ARS1DTExDLUVmKSYmHyBKNx9FRR8BCiYmHyA6Nx9FRR83VikAAAMAN//2AeECOgATACIAMQAAARUUBw4BIiYnJj0BNDc+ATIWFxYFMhYzMjcmJyYiBwYdATYXNQYjIiYjIgcUFxYyNzYB4Q8WaY5pFg8PFmmOaRYP/vQgShIoBAIKGpYaDBXNFCcgSRMmBQwalhoMAUtmOy1AR0dALTtmOy1AR0dALS07MC8bQkIcMxEhcg8hOzAuGkJCHAAAAQAMAAACiQLuABEAAAEDIwMzEzMTPgEzMhcVJiMiBgILpoPWb6sCgRVURxwUGA4kJgI5/ccC7v2JAd1PSwJgAicAAAABAAkAAAIfAjAAFAAAAQMjAzMTMxM+ATM6ATMXFSoBJiMiAbGHZrtqhgJeFk0+BxIGBgEHDAY+AZX+awIw/kkBNUc7AVcBAAADAAwAAAKJA9sAAwAHABkAAAEjJzMHIyczAQMjAzMTMxM+ATMyFxUmIyIGAd5FoXVRRaF1AWCmg9ZvqwKBFVRHHBQYDiQmAye0tLT+Xv3HAu79iQHdT0sCYAInAAP//gAAAh8DRQADAAcAHAAAASMnMwcjJzMBAyMDMxMzEz4BMzoBMxcVKgEmIyIBpkWhdVFFoXUBPodmu2qGAl4WTT4HEgYGAQcMBj4CkbS0tP5Q/msCMP5JATVHOwFXAQAAAAADAEH/KARbAvgAEwArAD0AACURNCcmKwEiBwYVERQXFjsBMjc2JRE0Nz4BOwEyFhcWFREUBw4BKwEiJicmATI2PwEDMxMzEzMDBiMiJzUWAb0OIFcIVyAODiBXCFcgDv6EEhx5SAhIeRwSEhx5SAhIeRwSAnQmLA8ewWuKAodq6jN9LCQs9wEANx9FRR83/wA3H0VFHzcBAEUtQ0xMQy1F/wBFLUNMTEMt/tYpKFACB/52AYr9g4sLXwoAAAMAN/8oBAMCOgATACMANQAAARUUBw4BIiYnJj0BNDc+ATIWFxYHNTQnJiIHBh0BFBcWMjc2EzI2PwEDMxMzEzMDBiMiJzUWAeEPFmmOaRYPDxZpjmkWD2QMGpYaDAwalhoM4CYsDx7Ba4oCh2rqM30sJCwBS2Y7LUBHR0AtO2Y7LUBHR0Atn2IzHEJCHDNiMxxCQhz+1CkoUAIH/nYBiv2DiwtfCgAAAAIAQf/JAicDJQAfADsAADcRNDc+ATc+ATIWFx4BFxYVERQHDgEHDgEiJicuAScmJRE0JyYnDgEiJicGBwYVERQXFhc+ATIWFzY3NkESF1s5Ah8qHwI5WxcSEhdbOQIfKh8COVsXEgF8DhQzBRwmHAUyFQ4OFTIFHCYcBTIVDvcBAEUtN0cMFR0dFQxHNy1F/wBFLTdHDBUdHRUMRzctQAEKNx8sEhIXFxIRLR83/vY3Hy8PEhcXEg8vHwAAAgA3/8kB4QJnAB0AOwAAARUUBwYHDgEjIiYnJicmPQE0NzY3PgEzMhYXFhcWBzU0JyYnDgEjIiYnBgcGHQEUFxYXPgEzMhYXNjc2AeEPJWsCHxUWHgJsJA8PI20CHhYVHwJrJQ9kDA8iBR0SExwFIg8MDA8iBRwTEh0FIg8MAUtmOy1pGBUeHhUYaS07ZjstaBkVHh4VGWgtpGwyHSYREhYWEhEmHTJsMh0mERIWFhIRJh0AAgA8//YDGAOsACYALgAAATUzFRQWMzI2NTQmJzMeARUUBiMiJwYjIiY1NDY3Mw4BFRQWMzI2AzUzNTMVIxUBdWoyLy4+IhttHCB2V3gpKXhXdiAcbRsiPi4vMm76UPoBHeLiXWSVnFDGS0jCVtPFh4fF01bCSEvGUJyVZAJgXy1fLQAAAgA3//YCjAMWACEAKQAAJTUzFRQzMjY1NCczFhUUBiMiJwYjIiY1NDczDgEVFBYzMgM1MzUzFSMVAS9kRiAuNWczY0haJiZaR2MzZxgdLSFFdPpQ+uClpYpma3yNeY+elGdnlJ6TdTuUOmxlAjRfLV8tAAACADz/9gMYA4YACwAyAAABIwcjNSEVIycjByMDNTMVFBYzMjY1NCYnMx4BFRQGIyInBiMiJjU0NjczDgEVFBYzMjYBjlUKLQFPLQpVCiMjajIvLj4iG20cIHZXeCkpeFd2IBxtGyI+Li8yA1UuX18uLv324uJdZJWcUMZLSMJW08WHh8XTVsJIS8ZQnJVkAAACADf/9gKMAuYACwAtAAABIwcjNSEVIycjByMDNTMVFDMyNjU0JzMWFRQGIyInBiMiJjU0NzMOARUUFjMyAUZVCi0BTy0KVQojIWRGIC41ZzNjSFomJlpHYzNnGB0tIUUCtS5fXy4u/lmlpYpma3yNeY+elGdnlJ6TdTuUOmxlAAEAQf8MAigC+AApAAAlBiMiJicmNRE0Nz4BOwEyFhcWFyMmJyYrASIHBhURFBcWOwEyNjczESMBxDZiRnccEhIceUgISXobEQFrAgwgVwhXIA4OIFcIQkECa2Q/SUlDLUgBAEUtQ0xMQyo/MBxGRh82/wA2H0VRRv4YAAABADf/OAHgAjoAJwAAJQYjIiYnJj0BNDc+ATMyFhcWFyMmJyYjIgcGHQEUFxYzMjc2NzMRIwF/Lk1BaBUPDxZpR0hpFggFZgMGGUxLGgwMGktMGQUDZmAsNkg/LTtmOy1AR0Y/GSYVD0BCHDNiMxxCQAwO/ogAAAABABb/8QIsAvwAEwAAEzcnNxc3FwcXBycHFwcnByc3JzfQY54dnWE/YJ0cnmOeHZ1hP2CdHAEb1Uk9Sc8ez0o8SdVJPUnPHs9KPAAAAQBVAooBnwMWAAcAABM1MzUzFSMVVfpQ+gKKXy1fLQABADUCkQHBAycAEwAAASIGBwYrATUzMjc+AjMyFhcjJgFCH1cYITQqJzAZEjc2ITVDBEALAt07Bwo6DAgsHEhBPwAAAAEAVQKHAaQDFAAHAAATIRUhFSM1M4wBGP7oNzcC5jEujQAAAAEAUAKHAZ8DFAAHAAATITUzFSM1IVABGDc3/ugC5i6NLgAAAAEAMwKRAb8DJwATAAATIgcjPgEzMh4BFxY7ARUjIicuAbI0C0AEQzUhNjcSGTAnKjQhGFcC3T9BSBwsCAw6Cgc7AAAACAAkAAwDDAKQAAsAFwAjAC8AOwBHAFMAXwAAADIWFyMuASIGByM2EjIWFyMuASIGByM2AjIWFyMuASIGByM2JDIWFyMuASIGByM2JDIWFyMuASIGByM2JDIWFyMuASIGByM2ADIWFyMuASIGByM2JDIWFyMuASIGByM2AW5UNQIkBCEwIQQkAjVUNQIkBCEwIQQkAt5UNQIkBCEwIQQkAgJbVDUCJAQhMCEEJAL+Z1Q1AiQEITAhBCQCAatUNQIkBCEwIQQkAv6/VDUCJAQhMCEEJAIBq1Q1AiQEITAhBCQCApA0KxkfHxkr/g80KxkfHxkrAUc0KxkfHxkrNDQrGR8fGSvUNCsZHx8ZKzQ0KxkfHxkr/u80KxkfHxkrNDQrGR8fGSsAAAAIACP/4gMNAswACgAVACAAKwA2AEEATABXAAAlNTMVFAc1NjU0IxMVIzU0NxUGFRQzASM1MzIXIyYjIhUlMxUjIiczFjMyNQUnNxcWByc2JyYHARcHJyY3FwYXFjcFByc3NhcHJgcGFwE3FwcGJzcWNzYnAXBQVi0HMFBWLQcBC05BYwweDi4I/dpOQWMMHg4uCAG5NzkuRjUVFyIFBf6dNzkuRjUVFyIFBQGcNzkuRkUVKiAFBP5ZNzkuRkUVKiAFBEROQWMMHg4uCAImTkFjDB4OLgj+xVBWLQcwUFYtB+c3OS5GRRUqIAYFAac3OS5GRRUqIAYFIjc5LkY1FRciBgT+nTc5LkY1FRciBgQAAAIATP8MArUDygAQABwAAAkBIxEzEQczATMRMwMjNyMRAiImJzMeATI2NzMGAdL++X9qAQEBCH55SWQ0akaQXgRGBTRWNAVGBAI7/cUC7v3/OgI7/Xn+pfQCAQEiW0wwOTkwTAACAEn/OAJiAxYAEwAfAAAhIxE3IwMjETMRBzM2EjczETMDIwIiJiczHgEyNjczBgHuZAEBz3JkAQEekCJxdD9gYJBeBEYFNFY0BUYEAVc6/m8CMP6pOjoBFEP+MP7YAzdbTDA5OTBMAAIAFgAAAn0C7gAWACEAABMzNTMVMxUjFTMyFx4BFRQGBwYrAREjBSMVMzI3NjU0JyYWfmqZmYRGJ0JMTEInRu5+AWqCgjodPDwdAnZ4eEZlDxhxTE1zGA8CMMz9ESJNSiIRAAACAAwAAAITAjAAFQAfAAATFTMyFx4BFAYHBisBESM1MzUzFTMVBxUzMjc2NCcmI9hrNCY2QEA2JjTPaGhkcHBqLBYrKxYsAaI+DBJYeFgSDAGiRkhIRpyoDBZkFgwAAAACAEwAAAIrAu4AEQAfAAABIxEjETMyFx4BFRQGBxcHJwYnNxc2NTQnJisBFTMyNwEwemrkRidCTDs0LzQ0HUY0Mzo8HTp4eBUJASP+3QLuDxhzTUNnHVEeWQe0HVgiSU0iEf0BAAAAAAIASf8MAf0COgAbADQAAAUiJyMWHQEjETMXMzYzMhYXFh0BFAcGBxcHJwYnNxc2NzY9ATQnJiMiBgcGHQEUFx4BMzI3ATZUNQEBZEsNATZjRmASCgkTMy40LyNFNDQQBwkKF0wkPRENCA07KhAPCjohGeoDJElTS0QiOWg3JEsmTh5QCsAdWxMXGitmKx5ELSMcKVckGigyAwAAAAEATAAAAfsDugAHAAAzESE1MxEhEUwBS2T+uwLuzP7N/XkAAAEASQAAAa8C2gAHAAATESMRITUzEa1kAQZgAdD+MAIwqv72AAEADwAAAioC7gANAAATESMRIzUzESEVIRUzFeVqbGwBr/67vAFl/psBZUwBPWfWTAAAAAABABIAAAHWAjAADQAANxUjNSM1MzUhFSEVMxXUZF5eAWb+/qLz8/NG92CXRgAAAAABAEz/DwIyAu4AIAAAExEjESEVIRU2MzIWFxYdARQGIyInNxYzMjY1NzQnJiMitmoBr/67QktldhAEd29mQkgoPDNFAQQSfkABZf6bAu5nvQxiWBk4s3mQQ0wnTUi/MBNiAAAAAQA6/xMBywIwACEAACUVFAYjIic1FjMyNj0BNCcuASMiBxUjESEVIRU2MzIWFxYBy2lcGRQYDjcxBAcwMyoxZAFm/v4rPVRiDAOTtWdkAmACLz20Hg8fIAf4AjBgeQdLRBMAAAEAC/8MA9sC8gA3AAABMxEzMjY/AT4BMzIXFSYjIgYPAQYHEzMRIzUjAyMRIxEjAyMTJi8BLgEjIgc1NjMyFh8BHgE7AQGtakI/OQwLDz4+HBsRDh0dBgkYTJ5fZDrFYWphxXzbTBgJBx0dDhAbGz4/DwsMOT9CAu7+zDM+NktGBGADHSMxhSr+9v6l9AFT/q0BU/6tAXEqhTEjHQNgBEZLNj4zAAAAAAEAFP84AzQCNQA3AAABFTMyNj8BPgEzMhcVJiMiDwEOAQcXMxEjNSMnIxUjNSMHIxMuAS8BJiMiBzU2MzIWHwEeATsBNQHKLC0pCQoONjYfGBcNJgkHCiMgdFdgLp89ZD2fdrMgIwoHCSYNFxgfNjYOCgkpLSwCMOAjKSo4NwVWBSokMjwSsf7YyPb29vYBERI8MiQqBVYFNzgqKSPgAAABACn/HAINAvcANQAAARUeARUUBgcWBw4BIic3FjMyNTQnLgE1Mx4BMzI2NTQmKwE1MzI2NTQmIyIGByM0NjMyFhUUAZY7PGhRLwECPGImECMfLSprfWkBR0E8TFFPV1dOTUk6QUcBaYNvZIkBegEcYDpLbw9INys0EzYOMydDA3VdNjs/NjRKXUY0Njs7Nl92c1h7AAAAAAEAJf8cAcACOgAzAAABFAcWFRQGBxYHDgEiJzcWMzI1NCcuATUzFBYzMjY1NCYrATUzMjY1NCYjIgYVIzQ2MzIWAbxbX0xDMAECPGImECMfLSxcbGQ6NS42OTpFRTo1My01OmRzYFVvAaRcLSlbOFYNSzYrNBM2DjMnRQNeSiQpKCIhMFgtISIlJyVMXlQAAAEATP8MApMC8gAfAAAzETMRMzI2PwE+ATMyFxUmIyIGDwEGBxMzESM1IwMjEUxqVj85DAsPPj4cGxEOHR0GCRhMnmRkP8V1Au7+zDM+NktGBGADHSMxhSr+9v6l9AFT/q0AAAABAEn/OAIuAjUAHwAAMxEzFTMyNj8BPgEzMhcVJiMiDwEOAQcXMxEjNSMnIxVJZD4tKQkKDjY2HxgXDSYJBwojIHNdYDOfTwIw4CMpKjg3BVYFKiQyPBKx/tjI9vYAAAABAEwAAAKAAvIAIQAAMxEzETM1MxU+AT8BPgEzMhcVJiMiBg8BBgcTIwMVIzUjEUxqTjwtLAsLDz4+HBsRDh0dBgkYTNt8xDxOAu7+zK6sBjQ1NktGBGADHSMxhSr+jwFRrK7+rQAAAAEASQAAAi8CNQAhAAAzETMVMzUzFT4BPwE+ATMyFxUmIyIPAQ4BBxMjJxUjNSMVSWQ8OCAeCQoONjYfGBcNJgkHCiMgs3aYODwCMOCrqgQjJCo4NwVWBSokMjwS/u/rk572AAABAAoAAAKeAvIAIwAAMxEjNTM1MxUzFSMVMzI2PwE+ATMyFxUmIyIGDwEGBxMjAyMRfnR0amxsVj85DAsPPj4cGxAPHR0GCRhM23zFdQIwRnh4RnYzPjZLRgRgAx0jMYUq/o8BU/6tAAEADAAAAi0CNQAjAAAzESM1MzUzFTMVIxUzMjY/AT4BMzIXFSYjIg8BDgEHEyMnIxVlWVlkVFQ+LSkJCg42Nh8YFw0mCQcKIyCzdp9PAaJGSEhGUiMpKjg3BVYFKiQyPBL+7/b2AAAAAQAKAAAC+QLyAB0AADMRIzUhETMyNj8BPgEzMhcVJiMiBg8BBgcTIwMjEdnPATlWPzkMCw8+PhwbEQ4dHQYJGEzbfMV1Aodn/swzPjZLRgRgAx0jMYUq/o8BU/6tAAAAAQAUAAACgQI1AB0AADMRIzUhFTMyNj8BPgEzMhcVJiMiDwEOAQcTIycjFbmlAQk+LSkJCg42Nh8YFw0mCQcKIyCzdp9PAdFf4CMpKjg3BVYFKiQyPBL+7/b2AAABAEz/DAKyAu4ADwAAISMRIREjETMRIREzETMRIwJOe/7jamoBHWp1ZAFT/q0C7v7MATT9ef6lAAEASf84Ak8CMAAPAAAhIzUjFSMRMxUzNTMRMxEjAe9k3mRk3mRgYPPzAjDd3f4w/tgAAAABAEwAAAMMAu4ADQAAAREjESERIxEzESERIRUCPWr+42pqAR0BOQKH/XkBU/6tAu7+zAE0ZwAAAAEASQAAApQCMAANAAABESM1IxUjETMVMzUhFQHvZN5kZN4BCQHR/i/z8wIw3d1fAAEATP8PA44C7gAiAAABETYzMhYXFh0BFAYjIic3FjMyNjU3NCcmIyIHESMRIxEjEQIcOUpldhAEd29mQkgoPDNFAQQSfjk8avxqAu7+3gpiWBk4s3mQQ0wnTUi/MBNiCv6aAof9eQLuAAAAAAEASf8TAvcCMAAjAAAzESEVNjMyFhcWHQEUBiMiJzUWMzI2PQE0Jy4BIyIHFSMRIxFJAYsoNlRiDANpXBkUGA43MQQHMDMtJGTDAjDYBktEFCi1Z2QCYAIvPbQeDx8gBvkB0P4wAAAAAgBB/34CogL4ADIAOQAABTI3FQYjIicjIiYnJjURNDc+ATsBMhYXIy4BKwEiBwYVERQXFjsBNTQ2MzIWHQEUBgcWAxU2PQE0IgIzPDMyP6AxK0t7HBISHHlICFuFDG4GSS8IVyAODiNZGkxLSE1XVSA+amoqGFoWeEtELUUBAEUtQ0xtWys3Rh82/wA3H0vtU2RgV15egQ0jAWvrBI1aWQAAAAACADf/nwIxAjcAMwA7AAAEMjcVBiMiJyMiJicmPQE0Nz4BOwEyFhcjLgErASIHBh0BFBceATsBPQE0NjMyFh0BFAYHJzY9ATQjIhUBol4xLDZ/KStBZRQLCBJjSAVHbA9qCjAeBUQWCAcKMCEaPz47QUFCHUklJA8WVhJaRDohOJAyHj5JUUUaHTgVK5ItEh0jAZtBTkdDQUFeEFEKVUI2NgAAAAEAQf8cAigC+AA3AAAlNjUzBgcOAQcWBw4BIic3FjMyNTQnLgEnJjURNDc+ATsBMhYXFhcjJicmKwEiBwYVERQXFjsBMgGvDmsBERdePC8BAjxiJhAjHy0sQ3AaEhIceUgISXobEQFrAgwgVwhXIA4OIFcIV6IhMUMsOUgKSDcrNBM2DjMoRQRKQC1FAQBFLUNMTEMqPzAcRkYfNv8ANh9FAAAAAQA3/xwB4AI6ADMAACU2NzMGBwYHFgcOASInNxYzMjU0Jy4BJyY9ATQ3PgEzMhYXFhcjJicmIyIHBh0BFBcWMzIBcQUDZgUHJXMvAQI8YiYQIx8tLEBdEw8PFmlHSGkWCAVmAwYZTEsaDAwaS0yWDA4fFmwVSDcrNBM2DjMoRQVGOy07ZjstQEdGPxkmFQ9AQhwzYjMcQgABABb/DAIaAu4ACwAAISMRIzUhFSMRMxEjAV57zQIEzXVkAodnZ/3g/qUAAAEAFP84AcoCMAALAAAhIxEjNSEVIxEzESMBIWSpAbapYGAB0V9f/o/+2AAAAQADAAACQwLuAAkAAAETMwMRIxEDMxMBJKd462rreKcBtwE3/lb+vAFEAar+yQAAAQAJ/wwB6wIwAAkAAAEDFSM1AzMTMxMB679kv2qGAoYCMP3Q9PQCMP5OAbIAAAAAAQADAAACQwLuABEAABM1AzMTMxMzAxUzFSMVIzUjNe7reKcCp3jrq6tqrgEmHgGq/skBN/5WHkbg4EYAAQAJ/wwB6wIwAA8AAAEDMxUjFSM1IzUzAzMTMxMB67+FhWSFhb9qhgKGAjD90EaurkYCMP5OAbIAAAAAAQAJ/wwCaQLuAA8AACEjCwEjEwMzGwEzAxMzESMCBT+ionnfyHqKinrIpGVkASL+3gGGAWj++wEF/pj+4f6lAAEADP84AhsCMAAPAAAhJwcjEwMzFzczAxczESM1AYiFhXLBsXN0c3WygmBg0dEBJgEKtrb+9sb+2MgAAAEAFv8MAw8C7gAPAAATIRUjESERMxEzESM1IREjFgGgmwEVanVk/gabAu5n/eACh/15/qX0AocAAQAU/zgCiQIwAA8AABMhFSMRMxEzETMRIzUhESMUAVJ31mRgYP5idwIwX/6PAdD+MP7YyAHRAAABADz/DAKYAu4AGAAAIREGIyImJyY9ATMVFBcWMzI3ETMRMxEjNQG5RVJfcw8FagQSeUg8anVkASMLY1cgMcvLMBNiCQFn/Xn+pfQAAQAs/zgCKQIwABgAACEjNQYjIiYnJj0BMxUUFxYzMjcRMxEzESMByWQ4N1JmDQVkBRJYOC5kYGDQB05BFiWdnBwSPgcBAf4w/tgAAAEAPAAAAjcC7gAaAAABMxU2NxEzESMRBgcVIzUuAScmPQEzFRQXFhcBHTxHLWpqNT88XXAPBWoEEGMCI6UCBwFn/RIBIwkBnJsBY1YgMcvLMBNZCAAAAAABACwAAAHdAjAAGgAAEzMVMjcRMxEjNQYHFSM1LgEnJj0BMxUUFxYX6jgoL2RkMSY4T14MBWQFDkcBt48HAQH90NAGAXR0BU09FiWdnBwSNAgAAAABAEwAAAIzAu4AFAAAExE2MzIWFxYdASM1NCcmIyIHESMRtkVSX3MPBWoEEnlIPGoC7v7dC2NXIDHLyzATYgn+mQLuAAEASQAAAeYCMAAUAAATFTYzMhYXFh0BIzU0JyYjIgcRIxGtODdSZg0FZAUSWDguZAIw0AdOQRYlnZwcEj4H/v8CMAAAAgAF//YCqQL4ADEAPQAAARUhFRQXFjsBMjc2NzMGBw4BKwEiJicmPQEjIiY1NDczBhUUFjsBNTQ3PgE7ATIWFxYHNTQnJisBIgcGHQECqP6EDiBXCFcgCgNsAw8bekkISHkcEgtaWAJgAiQvChIceUgISXobEWoOIFcIVyAOAfqjYDYfRkYVIy4nQ0xMQy1FYFhEHA8OFB8iPEUtQ0xLRC2BPzEhRUUfNjwAAAIAAv/2Ak4COgAoADAAAAAyFhcWHQEhFRQWMzI2NzMOASMiJicmPQEjIiY1NDczBhUUFjsBNjc2FwYHMyYnJiIBMo5pFg/+uT00KDkMZxFxUkdpFg8FTU8CVwIeKAQBDhZLCwHkAQsamAI6R0AtO0oiQU0tKFVbR0AtOxxQPRoNDBMcHy8pQFYaKCgaQgAAAAIABf84AqkC+AAzAD8AAAEVIRUUFxY7ATI3NjczBgcOAQcVIzUuAScmPQEjIiY1NDczBhUUFjsBNTQ3PgE7ATIWFxYHNTQnJisBIgcGHQECqP6EDiBXCFcgCgNsAw8XXzxgO18XEgtaWAJgAiQvChIceUgISXobEWoOIFcIVyAOAfqjYDYfRkYVIy4nOUgKwsIKSDktRWBYRBwPDhQfIjxFLUNMS0QtgT8xIUVFHzY8AAAAAAIAAv84Ak4COgArADMAAAAyFhcWHQEhFRQWMzI2NzMOAQcVIzUuAScmPQEjIiY1NDczBhUUFjsBNjc2FwYHMyYnJiIBMo5pFg/+uT00KDkMZw9UP2A3TxIPBU1PAlcCHigEAQ4WSwsB5AELGpgCOkdALTtKIkFNLShIVwzDwgpENS07HFA9Gg0MExwfLylAVhooKBpCAAAAAQBMAAAAtgLuAAMAADMRMxFMagLu/RIAAgALAAADuQPKADMAPwAAATMRMzI2PwE+ATMyFxUmIyIGDwEGBxMjAyMRIxEjAyMTJi8BLgEjIgc1NjMyFh8BHgE7ARIiJiczHgEyNjczBgGtakI/OQwLDz4+HBsRDh0dBgkYTNt8xWFqYcV820wYCQcdHQ4QGxs+Pw8LDDk/Qn2QXgRGBTRWNAVGBALu/swzPjZLRgRgAx0jMYUq/o8BU/6tAVP+rQFxKoUxIx0DYARGSzY+MwFpW0wwOTkwTAAAAgAUAAADHAMWADMAPwAAARUzMjY/AT4BMzIXFSYjIg8BDgEHEyMnIxUjNSMHIxMuAS8BJiMiBzU2MzIWHwEeATsBNTYiJiczHgEyNjczBgHKLC0pCQoONjYfGBcNJgkHCiMgs3afPWQ9n3azICMKBwkmDRcYHzY2DgoJKS0sepBeBEYFNFY0BUYEAjDgIykqODcFVgUqJDI8Ev7v9vb29gEREjwyJCoFVgU3OCopI+A/W0wwOTkwTAAAAAABAEz/DwJiAvIAKQAAJTQmJwcRIxEzETMyNj8BPgEzMhcVJiMiBg8BBgceARUUBiMiJzcWMzI2AfRaT5VqalY/OQwLDz4+HBsRDh0dBgkULlxTe29mQkgoPDNFDE2wTQP+rQLu/swzPjZLRgRgAx0jMWosZbJbeJFDTCdNAAAAAAEASf9TAgUCNQAnAAAlNCYnIxUjETMVMzI2PwE+ATMyFxUmIyIPAQYHFhUUBiMiJzcWMzI2AZ1EPHBkZD4tKQkKDjY2HxgXDSYJBxEhiGVcVjxAJS8nNBMzeTf2AjDgIykqODcFVgUqJE4ciYJbbjpEIDMAAQAI/wwCwwLuABYAACEjESMDDgQHNT4DNxMhETMDIwJKat8RAwoeL1Q6JTIaCwMUAa95SWQCh/7XOE5lPzEDZwIrTks1AYz9ef6lAAEACP84AmECMAAVAAAhIxEjBw4EBzU+AjcTIREzAyMB7WStCwIJGilJMiYuDgMQAXB0P2AB0MsoPEoxIwNfAjdBMQEm/jD+2AAAAAABAEz/EwI9Au4AFQAAASERIxEzESERMxEUBiMiJzUWMzI2NQHT/uNqagEdamlcGRQYDjIwAVP+rQLu/swBNPzwZ2QCYAIwPAABAEn/EwHvAjAAFQAABREjFSMRMxUzNTMRFAYjIic1FjMyNgGL3mRk3mRpXBkUGA43MSEBFPMCMN3d/a5nZAJgAi8AAAEATP8MArYC7gAPAAAhIxEhESMRMxEhETMRMwMjAj1q/uNqagEdanlJZAFT/q0C7v7MATT9ef6lAAAAAAEASf84AmMCMAAPAAAhIzUjFSMRMxUzNTMRMwMjAe9k3mRk3mR0P2Dz8wIw3d3+MP7YAAABADz/DAIjAu4AGAAAISMVIxEzNQYjIiYnJj0BMxUUFxYzMjcRMwIja2RlRVJfcw8FagQSeUg8avQBW7wLY1cgMcvLMBNiCQFnAAAAAQAs/zgByQIwABgAACEVIxEzNQYjIiYnJj0BMxUUFxYzMjcRMxEBZWBgODdSZg0FZAUSWDguZMgBKHAHTkEWJZ2cHBI+BwEB/dAAAAEATP8MAzkC7gAVAAAhIxE3IwMjAyMWFREjETMbATMRMwMjAsBmAQGrUqsBAWZ/u7t/eUlkAfw6/lwBpCEZ/gQC7v40Acz9ef6lAAABAEn/OAK/AjAAFQAAISMRNyMDIwMjFhURIxEzGwEzETMDIwJLYAEBfEp8AQFgcZCQcXQ/YAFIOv7nARkhGf64AjD+uQFH/jD+2AAAAQBJAAAArQIwAAMAADMRMxFJZAIw/dAAAwAMAAACSAPKAAMACwAXAAABAyMDFyMHIxMzEyMCIiYnMx4BMjY3MwYBglcEVczoPW3SmNJtaZBeBEYFNFY0BUYEAUkBPP7EZeQC7v0SAyNbTDA5OTBMAAMAMv/2AdYDNAAXACIALgAAEyM+ATMyFREjJyMGIyImNTQ2OwE1NCMiAxQWMzI2PQEjIgYSIiYnMx4BMjY3MwawaA9nUsZHDAE4XlVlal15Y0kwMjIwSHgtN8CQXgRGBTRWNAVGBAGgR1PI/o5IUmNXT2EbZP7SLTNRKj0vAbNbTDA5OTBMAAAEAAwAAAJIA6cAAwALAA8AEwAAAQMjAxcjByMTMxMjAyM1MxcjNTMBglcEVczoPW3SmNJt4Glpx2lpAUkBPP7EZeQC7v0SAzhvb28ABAAy//YB1gLzABcAIgAmACoAABMjPgEzMhURIycjBiMiJjU0NjsBNTQjIgMUFjMyNj0BIyIGEyM1MxcjNTOwaA9nUsZHDAE4XlVlal15Y0kwMjIwSHgtN0lpacdpaQGgR1PI/o5IUmNXT2EbZP7SLTNRKj0vAapvb28AAAL/+AAAA0cC7gAPABMAADMjASEVIRUzFSMVIRUhNSMTAzMRcHgBSwIE/tby8gEq/mzgs4e0Au5n02fmZ+YBof7GAToAAwAy//YDHwI6ACcAMgA4AAAlMw4BIyInBiMiJjU0NjsBNTQjIgcjPgEzMhc2MzIWHQEhFRQWMzI2JRQWMzI2PQEjIgYlMy4BIyICtGgRcVN6NDpvWWVoW31iSRZrD2lScys1ZWdw/rc9NCg5/e4yMjBIeio4AUDlAzw0bKlWXWlpY1dOXCFkP0dTSUmDfzIuPkkuMi0zUSo6LYFDQgAAAAIATAAAAgMDygALABcAADMRIRUhFSEVIRUhFQIiJiczHgEyNjczBkwBt/6zARP+7QFNmpBeBEYFNFY0BUYEAu5n02fmZwMjW0wwOTkwTAAAAwA3//YB4gMWABsAIwAvAAASMhYXFh0BIRUUFjMyNjczDgEjIiYnJj0BNDc2FwYHMyYnJiI2IiYnMx4BMjY3MwbGjmkWD/65PTQoOQxnEXFSR2kWDw8WSwsB5AELGpiTkF4ERgU0VjQFRgQCOkdALTtKIkFNLShVW0dALTtmOy1AVhooKBpCkFtMMDk5MEwAAgBB//YCKAL4ACMALwAANzUhNTQnJisBIgcGByM2Nz4BOwEyFhcWFREUBw4BKwEiJicmNxUUFxY7ATI3Nj0BQgF8DiBXCFcgCgNsAw8bekkISHkcEhIceUgISXobEWoOIFcIVyAO9KNgNh9GRhUjLidDTExDLUX/AEUtQ0xLRC2BPzEhRUUfNjwAAAACADf/9gHiAjoAGwAjAAAEIiYnJj0BITU0JiMiBgcjPgEzMhYXFh0BFAcGJzY3IxYXFjIBU45pFg8BRz00KDkMZxFxUkdpFg8PFksLAeQBCxqYCkdALTtKIkFNLShVW0dALTtmOy1AVhooKBpCAAAABABB//YCKAOnACMALwAzADcAABMVFBceATsBMjY3NjURNCcuASsBIgYHBgczNjc2OwEyFxYdAQU1IRUUBwYrASInJhMjNTMXIzUzQhEbekkISHkcEhIceUgISXobDwNsAwogVwhXIA7+7gESDiBXCFcgDllpacdpaQGXo0ItREtMQy1FAQBFLUNMTEMnLiMVRkYfNmCjPzw2H0VFIQJ1b29vAAAEADf/9gHiAvMAGwAjACcAKwAABCImJyY9ASE1NCYjIgYHIz4BMzIWFxYdARQHBic2NyMWFxYyAyM1MxcjNTMBU45pFg8BRz00KDkMZxFxUkdpFg8PFksLAeQBCxqYe2lpx2lpCkdALTtKIkFNLShVW0dALTtmOy1AVhooKBpCAjNvb28AAAAAAwALAAADuQOnADMANwA7AAABMxEzMjY/AT4BMzIXFSYjIgYPAQYHEyMDIxEjESMDIxMmLwEuASMiBzU2MzIWHwEeATsBEyM1MxcjNTMBrWpCPzkMCw8+PhwbEQ4dHQYJGEzbfMVhamHFfNtMGAkHHR0OEBsbPj8PCww5P0IGaWnHaWkC7v7MMz42S0YEYAMdIzGFKv6PAVP+rQFT/q0BcSqFMSMdA2AERks2PjMBfm9vbwAAAwAUAAADHALzADMANwA7AAABFTMyNj8BPgEzMhcVJiMiDwEOAQcTIycjFSM1IwcjEy4BLwEmIyIHNTYzMhYfAR4BOwE1NyM1MxcjNTMByiwtKQkKDjY2HxgXDSYJBwojILN2nz1kPZ92syAjCgcJJg0XGB82Ng4KCSktLANpacdpaQIw4CMpKjg3BVYFKiQyPBL+7/b29vYBERI8MiQqBVYFNzgqKSPgVG9vbwAAAAADACn/9gINA6cAKAAsADAAAAE1NjU0JiMiBhUzPgEzMhYVFAYrARUzMhYVFAYjIiYnIxQWMzI2NTQmAyM1MxcjNTMBlnKJZG+DaQFHQTpJTU5XV09RTDxBRwFpg29mjDztaWnHaWkBegE3e1hzdl82Ozs2NEZdSjQ2Pjs2X3Z2WDpgAdpvb28AAwAl//YBwALzACYAKgAuAAABNCYjIgYVMzQ2MzIWFRQGKwEVMzIWFRQGIyImNSMUFjMyNjU0JzYDIzUzFyM1MwG8b1Vgc2M7NS0zNTpFRTo5Ni41O2NzYFttX1v3aWnHaWkBpEJUXkwlJyUiIS1YMCEiKCkkS2BeQ1spLQE8b29vAAAAAQAr//YCEwLuABsAABMnNyE1IRUHHgEdARQGIyImNTMUFjMyNj0BNCPXFL3+xAHJyF9vhnFoiWpIP0NKjAFzV71nXcgCdGILZop+Zz1BTT8EhgAAAQAa/x8B5gIwABkAAAUiJiczHgEzMjY3NCYrASc3ITUhFQceARQGAQBmewVlBEA9QUABTFcsGcT+zwGuyWRweeF+ZDZMUUA/Vk3iXFboCHfKigAAAgBMAAACPAN6AA8AEwAAATMGFREzESMBIzY1ESMRMwEVITUB0gEBan7++AEBan8BFP7KAj8hGf37Au79wSEZAgX9EgN6QUEAAgBJAAAB7gLGABEAFQAAATMGFREzESMGAgcjNjURIxEzExUhNQGKAQFkcSORHAEBZHL7/soBkyEZ/qcCMET+6DchGQFZ/dACxkFBAAMATAAAAjwDzwAPABMAFwAAATMGFREzESMBIzY1ESMRMxMjNTMXIzUzAdIBAWp+/vgBAWp/Smlpx2lpAj8hGf37Au79wSEZAgX9EgNgb29vAAAAAAMASQAAAe4DEQARABUAGQAAATMGFREzESMGAgcjNjURIxEzEyM1MxcjNTMBigEBZHEjkRwBAWRyMWlpx2lpAZMhGf6nAjBE/ug3IRkBWf3QAqJvb28AAAAEAEH/9gInA6cAEwArAC8AMwAAJRE0JyYrASIHBhURFBcWOwEyNzYlETQ3PgE7ATIWFxYVERQHDgErASImJyYTIzUzFyM1MwG9DiBXCFcgDg4gVwhXIA7+hBIceUgISHkcEhIceUgISHkcEsRpacdpafcBADcfRUUfN/8ANx9FRR83AQBFLUNMTEMtRf8ARS1DTExDLQKGb29vAAAEADf/9gHhAvMAEwAjACcAKwAAARUUBw4BIiYnJj0BNDc+ATIWFxYHNTQnJiIHBh0BFBcWMjc2AyM1MxcjNTMB4Q8WaY5pFg8PFmmOaRYPZAwalhoMDBqWGgygaWnHaWkBS2Y7LUBHR0AtO2Y7LUBHR0Atn2IzHEJCHDNiMxxCQhwB0G9vbwAAAwBB//YCJwL4ABcAIwAvAAA3ETQ3PgE7ATIWFxYVERQHDgErASImJyYlNSEVFBcWOwEyNzYBFSE1NCcmKwEiBwZBEhx5SAhIeRwSEhx5SAhIeRwSAXz+7g4gVwhXIA7+7gESDiBXCFcgDvcBAEUtQ0xMQy1F/wBFLUNMTEMtRVBQNx9FRR8BN0xMNx9FRR8AAwA3//YB4QI6ABMAHQAlAAABFRQHDgEiJicmPQE0Nz4BMhYXFgc1IxUUFxYyNzYnMzQnJiIHBgHhDxZpjmkWDw8WaY5pFg9k4gwalhoM4uIMGpYaDAFLZjstQEdHQC07ZjstQEdHQC2fBwczHEJCHJQ0HEJCHAAABQBB//YCJwOnABcAIwAvADMANwAANxE0Nz4BOwEyFhcWFREUBw4BKwEiJicmJTUhFRQXFjsBMjc2ARUhNTQnJisBIgcGEyM1MxcjNTNBEhx5SAhIeRwSEhx5SAhIeRwSAXz+7g4gVwhXIA7+7gESDiBXCFcgDlppacdpafcBAEUtQ0xMQy1F/wBFLUNMTEMtRVBQNx9FRR8BN0xMNx9FRR8BCm9vbwAABQA3//YB4QLzABMAHQAlACkALQAAARUUBw4BIiYnJj0BNDc+ATIWFxYHNSMVFBcWMjc2JzM0JyYiBwYTIzUzFyM1MwHhDxZpjmkWDw8WaY5pFg9k4gwalhoM4uIMGpYaDEJpacdpaQFLZjstQEdHQC07ZjstQEdHQC2fBwczHEJCHJQ0HEJCHAEIb29vAAAAAwAk//YCDgOnAC8AMwA3AAA3JicjFhceATsBMjY3NjURNCcuASsBIgYHBgczNjc2OwEyFxYdASMVMxUUBwYrASITIzUzFyM1M5sIA2wFCxl7Sg1IeRwSEhx5SA1JehsLBGwDBx5ZDVcgDvHxDiBXDVczaWnHaWmiFBYqHURLTEMtRQEARS1DTExDHSQVD0ZGHzZDZ1Y2H0UC229vbwAAAAMAK//2AdYC8wAkACgALAAAJSInIx4BMzI2NzY9ATQnLgEjIgYHMz4BMzIXFh0BIxUzFRQHBgMjNTMXIzUzAQFdEmYLclhHaRYPDxZpR1xuDGYGOjBLGgy7uwwafWlpx2lpVlBQYEdALTtmOy1AR2BaKy9CHDMDXQIzHEICLm9vbwAAAAIAEP/2Ai8DegARABUAADcyNjcDMxMzEzMDDgEjIic1FgEVITWBLi4S33ClAplv3h9eRzUkLAFh/spYKTECPP5FAbv9pVNKC2EKAyJBQQAAAgAJ/ygB8wLGABEAFQAAFzI2PwEDMxMzEzMDBiMiJzUWARUhNU0mLA8ewWuKAodq6jN9LCQsAWf+yngpKFACB/52AYr9g4sLXwoDPkFBAAADABD/9gIvA6cAEQAVABkAADcyNjcDMxMzEzMDDgEjIic1FhMjNTMXIzUzgS4uEt9wpQKZb94fXkc1JCyXaWnHaWlYKTECPP5FAbv9pVNKC2EKAuBvb28AAwAJ/ygB8wLzABEAFQAZAAAXMjY/AQMzEzMTMwMGIyInNRYTIzUzFyM1M00mLA8ewWuKAodq6jN9LCQsnWlpx2lpeCkoUAIH/nYBiv2DiwtfCgL8b29vAAMAEP/2Ai8D2wARABUAGQAANzI2NwMzEzMTMwMOASMiJzUWEyM3MxcjNzOBLi4S33ClAplv3h9eRzUkLGNFcXUhRXF1WCkxAjz+RQG7/aVTSgthCgLPtLS0AAAAAwAJ/ygCDgMnABEAFQAZAAAXMjY/AQMzEzMTMwMGIyInNRYTIzczFyM3M00mLA8ewWuKAodq6jN9LCQsdkVxdSFFcXV4KShQAgf+dgGK/YOLC18KAuu0tLQAAAADADwAAAIjA6cAFAAYABwAACERBiMiJicmPQEzFRQXFjMyNxEzEQEjNTMXIzUzAblFUl9zDwVqBBJ5SDxq/uVpacdpaQEjC2NXIDHLyzATYgkBZ/0SAzhvb28AAAMALAAAAckC8wAUABgAHAAAITUGIyImJyY9ATMVFBcWMzI3ETMRAyM1MxcjNTMBZTg3UmYNBWQFElg4LmT8aWnHaWnQB05BFiWdnBwSPgcBAf3QAoRvb28AAAAAAQBM/wwB+wLuAAkAADMjESEVIREzESPHewGv/rt1ZALuZ/3g/qUAAAEASf84Aa8CMAAJAAAzIxEhFSERMxEjrWQBZv7+YGACMGD+kP7YAAAFAEwAAALcA6cADgAZAB0AIQAlAAATMzIXHgEVFAYHBisBETMTIxUzMjc2NTQnJgERMxEBIzUzFyM1M7ZmRidCTExCJ0bQamRkZDodPDwdAR5q/olpacdpaQHLDxhxTE1zGA8C7v52/REiTUoiEf6cAu79EgM4b29vAAAABQBJAAACfQLzAAMAEQAbAB8AIwAAIREzEQEVMzIXHgEUBgcGKwERExUzMjc2NCcmIxMjNTMXIzUzAhlk/jBXNCY2QEA2JjS7ZFYrFysrFiwxaWnHaWkCMP3QAjDMDBJYeFgSDAIw/taoDBZkFgwBfm9vbwABAA//EwIqAu4AGwAAFzUjESM1MxEhFSEVMxUjFTMVFAYjIic1FjMyNvh9bGwBr/67vLx1aVwZFBgONzMhIQFlTAE9Z9ZM/olnZAJeAjEAAAEAEv8gAdYCMAAaAAAzNSM1MzUhFSEVMxUjFTMVFCMiJzUWMzI2PQFwXl4BZv7+oqJgviIJFhI1MPNG92CXRpN+wgFZAjE7HAAAAAABAAn/EwJbAu4AGQAAISMLASMTAzMbATMDEzMVFAYjIic1FjMyNjUB+TOionnfyHqKinrIpFdpXBkUGA43MwEi/t4BhgFo/vsBBf6Y/uGJZ2QCXgIxPQAAAQAM/yACDwIwABgAACEjJwcjEwMzFzczAxczFRQjIic1FjMyNjUBsyuFhXLBsXN0c3WyglS+IgkWEjUw0dEBJgEKtrb+9sZ+wgFZAjE7AAABAA4AAAJEAu4AEQAAMyMTIzUzAzMbATMDMxUjEyMDh3nNqam2eoqKereqqc15ogFjRgFF/vsBBf67Rv6dASIAAQAMAAAB+gIwABEAABMzFzczBzMVIxMjJwcjEyM1MxxzdHN1n4iHrXKFhXKshocCMLa26kL+/NHRAQRCAAAAAAEALQFaAhMBsgADAAABITUhAhP+GgHmAVpYAAABAC0BWgKLAbIAAwAAASE1IQKL/aICXgFaWAAAAQAtAesAvAMRAAsAABMVIzU0NjcVBhUUM7KFQ0xLCwJug21HaAoyGE0MAAABACoB6wC5AxEACwAAEzUzFRQGBzU2NTQjNIVDTEsLAo6DbUdoCjIYTQwAAAEAKP9dALcAgwALAAAzNTMVFAYHNTY1NCMyhUNMSwuDbUdoCjIYTQwAAAAAAgAtAesBhAMRAAsAFwAAExUjNTQ2NxUGFRQ7ARUjNTQ2NxUGFRQzsoVDTEsL/oVDTEsLAm6DbUdoCjIYTQyDbUdoCjIYTQwAAgAqAesBgQMRAAsAFwAAEzUzFRQGBzU2NTQjMzUzFRQGBzU2NTQjNIVDTEsLkoVDTEsLAo6DbUdoCjIYTQyDbUdoCjIYTQwAAgAo/10BfwCDAAsAFwAAMzUzFRQGBzU2NTQjMzUzFRQGBzU2NTQjMoVDTEsLkoVDTEsLg21HaAoyGE0Mg21HaAoyGE0MAAAAAQAo/1cBygLuAAsAAAERIxEjNTM1MxUzFQEcRq6uRq4BzP2LAnVD399DAAABAC3/VwHPAu4AEwAANxEjNTM1MxUzFSMRMxUjFSM1IzXbrq5Grq6urkaueQFTQ9/fQ/6tQ9/fQwAAAAABADwBOwD5AgAAAwAAEyM1M/m9vQE7xQADADMAAAJ5AIMAAwAHAAsAADMjNTMXIzUzFyM1M7F+fuR+fuR+foODg4ODAAcAI//2A2kC+AALABkAJQAzAD8ATQBRAAAlFRQWMjY9ATQmIgYHNTQ2MzIWHQEUBiMiJiUVFBYyNj0BNCYiBgc1NDYzMhYdARQGIyImARUUFjI2PQE0JiIGBzU0NjMyFh0BFAYjIiYTIwEzAXciPCMjPCJFSzo7S0s7OksBcSI8IyM8IkVLOjtLSzs6S/4KIjwjIzwiRUs6O0tLOzpLekABaUCfFCkrKykUKSsrPRRDUVFDFENSUlcUKSsrKRQpKys9FENRUUMUQ1JSAhwUKSsrKRQpKys9FENRUUMUQ1JS/fMC7gAAAAABABwATQEEAj8ABQAAEzcVBxcVHOiBgQFG+W+Kim8AAAEAMgBNARoCPwAFAAABBzU3JzUBGuiBgQFG+W+Kim8AAf+c/34BsQNQAAMAAAcBFwFkAdRB/ixmA7Yd/EsAAAABAAD/9gIkAvgALwAAJTMOASsBIiY9ASM3MzUjNzM1NDY7ATIWFyMuASsBIgYdATMHIxUzByMVFBY7ATI2Ab1nB4JaAl2HWxZFWxZFh10CWYMHZgpHLAI1S+UYzbIYmks1AitGyl91hWwYRFxEJGyFd183P0pKIURcRBVKSj0AAAIAHwG/AiUC8QAiAC8AABMjNCYjIhUUHwEWFRQGIyImJzMeATMyNjU0LwEuATQ2MzIWFycVIxEzFzcXESM1B+wzGxcuJCFROiwwOQEzAR8WFx0rIiEnNyssOJpAM0BISUAzQAKTFB4iGw0LG0AmMDcoFxwYEx4NCwomSisx/szMASzq6gH+1cvLAAAAAgASAcICHQLuAAcAFAAAEzMVIxUjNSMFJxUjETMXNzMRIzUHEtRQM1EBbUAzQEhJQDNAAu4v/f39zMwBLOrq/tTLywAFACgAAAIcAu4AAwAGAAkADAAPAAABESEREwMRGwERJRsBCwEhAhz+DN2r5av+jqqqqqkBUQLu/RIC7v6IARn9zgEZ/ucCMi3+6QEX/or+7AAAAAAFACgAAAIcAu4AAwAGAAkADAAPAAABESEREwMRGwERJRsBCwEhAhz+DN2r5av+jqqqqqkBUQLu/RIC7v6IARn9zgEZ/ucCMi3+6QEX/or+7AAAAAAFACgAAAIcAu4AAwAGAAkADAAPAAABESEREwMRGwERJRsBCwEhAhz+DN2r5av+jqqqqqkBUQLu/RIC7v6IARn9zgEZ/ucCMi3+6QEX/or+7AAAAAAFACgAAAIcAu4AAwAGAAkADAAPAAABESEREwMRGwERJRsBCwEhAhz+DN2r5av+jqqqqqkBUQLu/RIC7v6IARn9zgEZ/ucCMi3+6QEX/or+7AAAAAAFACgAAAIcAu4AAwAGAAkADAAPAAABESEREwMRGwERJRsBCwEhAhz+DN2r5av+jqqqqqkBUQLu/RIC7v6IARn9zgEZ/ucCMi3+6QEX/or+7AAAAAABACABAAIGAVgAAwAAASE1IQIG/hoB5gEAWAAABQAoAAACHALuAAMABgAJAAwADwAAAREhERMDERsBESUbAQsBIQIc/gzdq+Wr/o6qqqqpAVEC7v0SAu7+iAEZ/c4BGf7nAjIt/ukBF/6K/uwAAAAABQAoAAACHALuAAMABgAJAAwADwAAAREhERMDERsBESUbAQsBIQIc/gzdq+Wr/o6qqqqpAVEC7v0SAu7+iAEZ/c4BGf7nAjIt/ukBF/6K/uwAAAAABQAoAAACHALuAAMABgAJAAwADwAAAREhERMDERsBESUbAQsBIQIc/gzdq+Wr/o6qqqqpAVEC7v0SAu7+iAEZ/c4BGf7nAjIt/ukBF/6K/uwAAAAABQAoAAACHALuAAMABgAJAAwADwAAAREhERMDERsBESUbAQsBIQIc/gzdq+Wr/o6qqqqpAVEC7v0SAu7+iAEZ/c4BGf7nAjIt/ukBF/6K/uwAAAAAAQAlAAACAQJZABMAADMjNyM1MzcjNSE3MwczFSMHMxUhlExTdqZD6QEZVExUd6dD6v7ml1h6WJiYWHpYAAAAAAIAMgAAAfUCkQADAAoAACkBNSE1JTUlFQ0BAfX+PQHD/kIBvv6WAWpYN9Bi0GChoQAAAAACADEAAAH0ApEAAwAKAAApATUhAwU1LQE1BQH0/j0BwwX+QgFq/pYBvlgBB9BgoaFg0AAABQAoAAACHALuAAMABgAJAAwADwAAAREhERMDERsBESUbAQsBIQIc/gzdq+Wr/o6qqqqpAVEC7v0SAu7+iAEZ/c4BGf7nAjIt/ukBF/6K/uwAAAAAAQAUAAAB8wMcABYAACERIxEjESM1MzU0MzIXFSYjIgYdASERAY++ZFlZ1h8gGiU+NAEiAdT+LAHUXCHLBGEELz4e/dAAAAAAAQAUAAACEwMcABYAACERJiMiBh0BMxUjESMRIzUzNTQzMhcRAa84ND40hIRkWVnZWHUCuAUuPiFg/jAB0GAhywv87wABALz+7AEv/84ADQAAFzUzFRQHDgEHNTY1NCPEawMGODI8CZtpThYUKToHKBQzCgABACYCLwCZAxEADQAAEzUzFRQHDgEHNTY1NCMuawMGODI8CQKoaU4XEyk6BygUMwoAAAAAAQBQAo0BpAM0AAsAAAAiJiczHgEyNjczBgFCkF4ERgU0VjQFRgQCjVtMMDk5MEwABAA8//YDHgL4AAcADwAmADAAADYQNiAWEAYgAhAWIDYQJiABByMmLwEmKwEVIxEzMhcWFRQHFh8BFgMjFTMyNjU0JyY8zQFIzc3+uJqvAR6vr/7iAUoDUBAEBgVFQ06LHxlrQjYGBgSrOT0oLTAN1AFG3t7+ut4CEP7iwcEBHsH9zQYRLEw4wQHYBRtiSCMXQ08rAXiFKCEtCwQABAA8//YDHgL4AAcADwAaACkAADYQNiAWEAYgAhAWIDYQJiATMzI3NjU0JyYrARcjFSMRMzIXHgEVFAYHBjzNAUjNzf64mq8BHq+v/uJeRSEUIyMUIUVHR06VLxgpNDIrGtQBRt7e/rreAhD+4sHBAR7B/rAKFC0xEgrhrgHYChBMMDFJEQkAAAAAGgE+AAEAAAAAAAAALgBeAAEAAAAAAAEADQCpAAEAAAAAAAIABwDHAAEAAAAAAAMAFwD/AAEAAAAAAAQADQEzAAEAAAAAAAUADQFdAAEAAAAAAAYADQGHAAEAAAAAAAcAIwHdAAEAAAAAAAgAAgIHAAEAAAAAAAoALgJoAAEAAAAAAA0AkQO7AAEAAAAAABAABQRZAAEAAAAAABEABwRvAAMAAQQJAAAAXAAAAAMAAQQJAAEAGgCNAAMAAQQJAAIADgC3AAMAAQQJAAMALgDPAAMAAQQJAAQAGgEXAAMAAQQJAAUAGgFBAAMAAQQJAAYAGgFrAAMAAQQJAAcARgGVAAMAAQQJAAgABAIBAAMAAQQJAAoAXAIKAAMAAQQJAA0BIgKXAAMAAQQJABAACgRNAAMAAQQJABEADgRfAEMAbwBwAHkAcgBpAGcAaAB0ACAAKABjACkAIAAyADAAMQAxACAAYgB5ACAARwBNAC4AIABBAGwAbAAgAHIAaQBnAGgAdABzACAAcgBlAHMAZQByAHYAZQBkAC4AAENvcHlyaWdodCAoYykgMjAxMSBieSBHTS4gQWxsIHJpZ2h0cyByZXNlcnZlZC4AAEwAbwB1AGkAcwAgAFIAZQBnAHUAbABhAHIAAExvdWlzIFJlZ3VsYXIAAFIAZQBnAHUAbABhAHIAAFJlZ3VsYXIAAEcATQA6ACAATABvAHUAaQBzACAAUgBlAGcAdQBsAGEAcgA6ACAAMgAwADEAMQAAR006IExvdWlzIFJlZ3VsYXI6IDIwMTEAAEwAbwB1AGkAcwAtAFIAZQBnAHUAbABhAHIAAExvdWlzLVJlZ3VsYXIAAFYAZQByAHMAaQBvAG4AIAAxAC4AMwAwADAAAFZlcnNpb24gMS4zMDAAAEwAbwB1AGkAcwAtAFIAZQBnAHUAbABhAHIAAExvdWlzLVJlZ3VsYXIAAEwAbwB1AGkAcwAgAFIAZQBnAHUAbABhAHIAIABpAHMAIABhACAAdAByAGEAZABlAG0AYQByAGsAIABvAGYAIABHAE0ALgAATG91aXMgUmVndWxhciBpcyBhIHRyYWRlbWFyayBvZiBHTS4AAEcATQAAR00AAEMAbwBwAHkAcgBpAGcAaAB0ACAAKABjACkAIAAyADAAMQAxACAAYgB5ACAARwBNAC4AIABBAGwAbAAgAHIAaQBnAGgAdABzACAAcgBlAHMAZQByAHYAZQBkAC4AAENvcHlyaWdodCAoYykgMjAxMSBieSBHTS4gQWxsIHJpZ2h0cyByZXNlcnZlZC4AAFQAaABpAHMAIABmAG8AbgB0ACAAcwBvAGYAdAB3AGEAcgBlACAAbQBhAHkAIABvAG4AbAB5ACAAYgBlACAAdQBzAGUAZAAgAGIAeQAgAGEAdQB0AGgAbwByAGkAegBlAGQAIABhAGcAZQBuAHQAcwAgAGEAbgBkACAAcgBlAHAAcgBlAHMAZQBuAHQAYQB0AGkAdgBlAHMAIABvAGYAIABHAE0ALgDKAEEAbgB5ACAAdQBuAGEAdQB0AGgAbwByAGkAegBlAGQAIAB1AHMAZQAgAG8AcgAgAGQAaQBzAHQAcgBpAGIAdQB0AGkAbwBuACAAaQBzACAAZQB4AHAAcgBlAHMAcwBsAHkAIABwAHIAbwBoAGkAYgBpAHQAZQBkAC4AAFRoaXMgZm9udCBzb2Z0d2FyZSBtYXkgb25seSBiZSB1c2VkIGJ5IGF1dGhvcml6ZWQgYWdlbnRzIGFuZCByZXByZXNlbnRhdGl2ZXMgb2YgR00u5kFueSB1bmF1dGhvcml6ZWQgdXNlIG9yIGRpc3RyaWJ1dGlvbiBpcyBleHByZXNzbHkgcHJvaGliaXRlZC4AAEwAbwB1AGkAcwAATG91aXMAAFIAZQBnAHUAbABhAHIAAFJlZ3VsYXIAAAAAAAIAAAAAAAD/gwAyAAAAAAAAAAAAAAAAAAAAAAAAAAACdgAAAAEAAgECAAMABAAFAAYABwAIAAkACgALAAwADQAOAA8AEAARABIAEwAUABUAFgAXABgAGQAaABsAHAAdAB4AHwAgACEAIgAjACQAJQAmACcAKAApACoAKwAsAC0ALgAvADAAMQAyADMANAA1ADYANwA4ADkAOgA7ADwAPQA+AD8AQABBAEIAQwBEAEUARgBHAEgASQBKAEsATABNAE4ATwBQAFEAUgBTAFQAVQBWAFcAWABZAFoAWwBcAF0AXgBfAGAAYQEDAKMAhACFAJYA6ACGAI4AiwCdAKkApACKANoAgwCTAPIA8wCNAJcAiADDAN4A8QCeAKoA9QD0APYAogCtAMkAxwCuAGIAYwCQAGQAywBlAMgAygDPAMwAzQDOAOkAZgDTANAA0QCvAGcA8ACRANYA1ADVAGgA6wDtAIkAagBpAGsAbQBsAG4AoABvAHEAcAByAHMAdQB0AHYAdwDqAHgAegB5AHsAfQB8ALgAoQB/AH4AgACBAOwA7gC6AQQBBQEGAQcBCAEJAP0A/gEKAQsBDAENAP8BAAEOAQ8BEAEBAREBEgETARQBFQEWARcBGAEZARoBGwEcAPgA+QEdAR4BHwEgASEBIgEjASQBJQEmAScBKAEpASoBKwEsAPoA1wEtAS4BLwEwATEBMgEzATQBNQE2ATcBOAE5AToBOwDiAOMBPAE9AT4BPwFAAUEBQgFDAUQBRQFGAUcBSAFJAUoAsACxAUsBTAFNAU4BTwFQAVEBUgFTAVQA+wD8AOQA5QFVAVYBVwFYAVkBWgFbAVwBXQFeAV8BYAFhAWIBYwFkAWUBZgFnAWgBaQFqALsBawFsAW0BbgDmAOcBbwCmAXAA2ADhANsA3ADdAOAA2QDfAJsBcQFyAXMBdAF1AXYBdwF4AXkBegF7AXwBfQF+AX8BgAGBAYIBgwGEAYUBhgGHAYgBiQGKAYsBjAGNAY4BjwGQAZEBkgGTAZQBlQGWAZcBmAGZAZoBmwGcAZ0BngGfAaABoQGiAaMBpAGlAaYBpwGoAakBqgGrAawBrQGuAa8BsAGxAbIBswG0AbUBtgG3AbgBuQG6AbsBvAG9Ab4BvwHAAcEBwgHDAcQBxQHGAccByAHJAcoBywHMAc0BzgHPAdAB0QHSAdMB1AHVAdYB1wHYAdkB2gHbAdwB3QHeAd8B4AHhAeIB4wHkAeUB5gHnAegB6QHqAesB7AHtAe4B7wHwAfEB8gHzAfQB9QH2AfcB+AH5AfoB+wH8Af0B/gH/AgACAQICAgMCBAIFAgYCBwIIAgkCCgILAgwCDQIOAg8CEAIRAhICEwIUAhUCFgIXAhgCGQIaAhsCHAIdAh4CHwIgAiECIgIjAiQCJQImAicCKAIpAioCKwIsAi0CLgIvAjACMQIyAjMCNAI1AjYCNwI4AjkCOgI7AjwCPQI+Aj8CQAJBAkICQwJEAkUCRgJHAkgCSQJKAksCTAJNAk4CTwJQAlECUgJTAlQCVQJWAlcCWAJZAloCWwJcAl0CXgJfAmACYQJiAmMCZAJlAmYCZwJoAmkCagJrAmwCbQJuAm8CcACyALMAtgC3AMQAtAC1AMUAggDCAIcAqwDGAL4AvwC8AnECcgCMAJ8AmACoAJoAmQDvAKUAkgCcAKcAjwCUAJUAuQDSAMAAwQJzAnQCdQJ2AncCeAJDUgd1bmkwMEEwB0FtYWNyb24HYW1hY3JvbgZBYnJldmUGYWJyZXZlB0FvZ29uZWsHYW9nb25lawtDY2lyY3VtZmxleAtjY2lyY3VtZmxleApDZG90YWNjZW50CmNkb3RhY2NlbnQGRGNhcm9uBmRjYXJvbgZEY3JvYXQHRW1hY3JvbgdlbWFjcm9uBkVicmV2ZQZlYnJldmUKRWRvdGFjY2VudAplZG90YWNjZW50B0VvZ29uZWsHZW9nb25lawZFY2Fyb24GZWNhcm9uC0djaXJjdW1mbGV4C2djaXJjdW1mbGV4Ckdkb3RhY2NlbnQKZ2RvdGFjY2VudAxHY29tbWFhY2NlbnQMZ2NvbW1hYWNjZW50C0hjaXJjdW1mbGV4C2hjaXJjdW1mbGV4BEhiYXIEaGJhcgZJdGlsZGUGaXRpbGRlB3VuaTAxMkEHaW1hY3JvbgZJYnJldmUHdW5pMDEyRAdJb2dvbmVrB2lvZ29uZWsCSUoCaWoLSmNpcmN1bWZsZXgLamNpcmN1bWZsZXgMS2NvbW1hYWNjZW50DGtjb21tYWFjY2VudAxrZ3JlZW5sYW5kaWMGTGFjdXRlBmxhY3V0ZQxMY29tbWFhY2NlbnQMbGNvbW1hYWNjZW50BkxjYXJvbgZsY2Fyb24ETGRvdARsZG90Bk5hY3V0ZQZuYWN1dGUMTmNvbW1hYWNjZW50DG5jb21tYWFjY2VudAZOY2Fyb24GbmNhcm9uC25hcG9zdHJvcGhlA0VuZwNlbmcHT21hY3JvbgdvbWFjcm9uBk9icmV2ZQZvYnJldmUNT2h1bmdhcnVtbGF1dA1vaHVuZ2FydW1sYXV0BlJhY3V0ZQZyYWN1dGUMUmNvbW1hYWNjZW50DHJjb21tYWFjY2VudAZSY2Fyb24GcmNhcm9uBlNhY3V0ZQZzYWN1dGULU2NpcmN1bWZsZXgLc2NpcmN1bWZsZXgMVGNvbW1hYWNjZW50DHRjb21tYWFjY2VudAZUY2Fyb24GdGNhcm9uBFRiYXIEdGJhcgZVdGlsZGUGdXRpbGRlB1VtYWNyb24HdW1hY3JvbgZVYnJldmUGdWJyZXZlBVVyaW5nBXVyaW5nDVVodW5nYXJ1bWxhdXQNdWh1bmdhcnVtbGF1dAdVb2dvbmVrB3VvZ29uZWsLV2NpcmN1bWZsZXgLd2NpcmN1bWZsZXgLWWNpcmN1bWZsZXgLeWNpcmN1bWZsZXgGWmFjdXRlBnphY3V0ZQpaZG90YWNjZW50Cnpkb3RhY2NlbnQFbG9uZ3MIZG90bGVzc2oHdW5pMDQwMAlhZmlpMTAwMjMJYWZpaTEwMDUxCWFmaWkxMDA1MglhZmlpMTAwNTMJYWZpaTEwMDU0CWFmaWkxMDA1NQlhZmlpMTAwNTYJYWZpaTEwMDU3CWFmaWkxMDA1OAlhZmlpMTAwNTkJYWZpaTEwMDYwCWFmaWkxMDA2MQd1bmkwNDBECWFmaWkxMDA2MglhZmlpMTAxNDUJYWZpaTEwMDE3CWFmaWkxMDAxOAlhZmlpMTAwMTkJYWZpaTEwMDIwCWFmaWkxMDAyMQlhZmlpMTAwMjIJYWZpaTEwMDI0CWFmaWkxMDAyNQlhZmlpMTAwMjYJYWZpaTEwMDI3CWFmaWkxMDAyOAlhZmlpMTAwMjkJYWZpaTEwMDMwCWFmaWkxMDAzMQlhZmlpMTAwMzIJYWZpaTEwMDMzCWFmaWkxMDAzNAlhZmlpMTAwMzUJYWZpaTEwMDM2CWFmaWkxMDAzNwlhZmlpMTAwMzgJYWZpaTEwMDM5CWFmaWkxMDA0MAlhZmlpMTAwNDEJYWZpaTEwMDQyCWFmaWkxMDA0MwlhZmlpMTAwNDQJYWZpaTEwMDQ1CWFmaWkxMDA0NglhZmlpMTAwNDcJYWZpaTEwMDQ4CWFmaWkxMDA0OQlhZmlpMTAwNjUJYWZpaTEwMDY2CWFmaWkxMDA2NwlhZmlpMTAwNjgJYWZpaTEwMDY5CWFmaWkxMDA3MAlhZmlpMTAwNzIJYWZpaTEwMDczCWFmaWkxMDA3NAlhZmlpMTAwNzUJYWZpaTEwMDc2CWFmaWkxMDA3NwlhZmlpMTAwNzgJYWZpaTEwMDc5CWFmaWkxMDA4MAlhZmlpMTAwODEJYWZpaTEwMDgyCWFmaWkxMDA4MwlhZmlpMTAwODQJYWZpaTEwMDg1CWFmaWkxMDA4NglhZmlpMTAwODcJYWZpaTEwMDg4CWFmaWkxMDA4OQlhZmlpMTAwOTAJYWZpaTEwMDkxCWFmaWkxMDA5MglhZmlpMTAwOTMJYWZpaTEwMDk0CWFmaWkxMDA5NQlhZmlpMTAwOTYJYWZpaTEwMDk3B3VuaTA0NTAJYWZpaTEwMDcxCWFmaWkxMDA5OQlhZmlpMTAxMDAJYWZpaTEwMTAxCWFmaWkxMDEwMglhZmlpMTAxMDMJYWZpaTEwMTA0CWFmaWkxMDEwNQlhZmlpMTAxMDYJYWZpaTEwMTA3CWFmaWkxMDEwOAlhZmlpMTAxMDkHdW5pMDQ1RAlhZmlpMTAxMTAJYWZpaTEwMTkzB3VuaTA0NjAHdW5pMDQ2MQlhZmlpMTAxNDYJYWZpaTEwMTk0B3VuaTA0NjQHdW5pMDQ2NQd1bmkwNDY2B3VuaTA0NjcHdW5pMDQ2OAd1bmkwNDY5BF8xMDYHdW5pMDQ2Qgd1bmkwNDZDB3VuaTA0NkQHdW5pMDQ2RQd1bmkwNDZGB3VuaTA0NzAHdW5pMDQ3MQlhZmlpMTAxNDcJYWZpaTEwMTk1CWFmaWkxMDE0OAlhZmlpMTAxOTYHdW5pMDQ3Ngd1bmkwNDc3B3VuaTA0NzgHdW5pMDQ3OQd1bmkwNDdBB3VuaTA0N0IHdW5pMDQ3Qwd1bmkwNDdEB3VuaTA0N0UHdW5pMDQ3Rgd1bmkwNDgwB3VuaTA0ODEHdW5pMDQ4Mgd1bmkwNDgzB3VuaTA0ODQHdW5pMDQ4NQd1bmkwNDg2B3VuaTA0ODcHdW5pMDQ4OAd1bmkwNDg5B3VuaTA0OEEHdW5pMDQ4Qgd1bmkwNDhDB3VuaTA0OEQHdW5pMDQ4RQd1bmkwNDhGCWFmaWkxMDA1MAlhZmlpMTAwOTgHdW5pMDQ5Mgd1bmkwNDkzB3VuaTA0OTQHdW5pMDQ5NQd1bmkwNDk2B3VuaTA0OTcHdW5pMDQ5OAd1bmkwNDk5B3VuaTA0OUEHdW5pMDQ5Qgd1bmkwNDlDB3VuaTA0OUQHdW5pMDQ5RQd1bmkwNDlGB3VuaTA0QTAHdW5pMDRBMQd1bmkwNEEyB3VuaTA0QTMHdW5pMDRBNAd1bmkwNEE1B3VuaTA0QTYHdW5pMDRBNwd1bmkwNEE4B3VuaTA0QTkHdW5pMDRBQQd1bmkwNEFCB3VuaTA0QUMHdW5pMDRBRAd1bmkwNEFFB3VuaTA0QUYHdW5pMDRCMAd1bmkwNEIxB3VuaTA0QjIHdW5pMDRCMwd1bmkwNEI0B3VuaTA0QjUHdW5pMDRCNgd1bmkwNEI3B3VuaTA0QjgHdW5pMDRCOQd1bmkwNEJBB3VuaTA0QkIHdW5pMDRCQwd1bmkwNEJEB3VuaTA0QkUHdW5pMDRCRgd1bmkwNEMwB3VuaTA0QzEHdW5pMDRDMgd1bmkwNEMzB3VuaTA0QzQHdW5pMDRDNQd1bmkwNEM2B3VuaTA0QzcHdW5pMDRDOAd1bmkwNEM5B3VuaTA0Q0EHdW5pMDRDQgd1bmkwNENDB3VuaTA0Q0QHdW5pMDRDRQd1bmkwNENGB3VuaTA0RDAHdW5pMDREMQd1bmkwNEQyB3VuaTA0RDMHdW5pMDRENAd1bmkwNEQ1B3VuaTA0RDYHdW5pMDRENwd1bmkwNEQ4CWFmaWkxMDg0Ngd1bmkwNERBB3VuaTA0REIHdW5pMDREQwd1bmkwNEREB3VuaTA0REUHdW5pMDRERgd1bmkwNEUwB3VuaTA0RTEHdW5pMDRFMgd1bmkwNEUzB3VuaTA0RTQHdW5pMDRFNQd1bmkwNEU2B3VuaTA0RTcHdW5pMDRFOAd1bmkwNEU5B3VuaTA0RUEHdW5pMDRFQgd1bmkwNEVDB3VuaTA0RUQHdW5pMDRFRQd1bmkwNEVGB3VuaTA0RjAHdW5pMDRGMQd1bmkwNEYyB3VuaTA0RjMHdW5p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/9gBm//7AAIBgP/OAZv/+wABAYAACAACACP/zgGA//wABgBOAAUAWAAUAFoAFABcAAgBYP/YAYD/5AAHADz/+ABb//gAXP/4AWD/zgFw//YBgP/YAZv/9gACAEQABgAGAAAACwAMAAEAEAAQAAMAEgAUAAQAFgAdAAcAIwAjAA8AJQBAABAARQBfACwAZABkAEcAbQBtAEgAfAB8AEkAgACQAEoAkgCWAFsAmgCeAGAAoACmAGUAqACwAGwAsgC3AHUAugC+AHsAwADAAIAA8gDyAIEBAgECAIIBFAEUAIMBIQEiAIQBOQE5AIYBPgE/AIcBTAFPAIkBVQFWAI0BWAFYAI8BWgFaAJABXAFjAJEBZgFmAJkBagFqAJoBbAFwAJsBcgFyAKABdQF2AKEBeAF6AKMBfAGDAKYBhgGGAK4BigGKAK8BjAGSALABlQGWALcBmAGaALkBnAGdALwBnwGfAL4BpQGmAL8BqAGoAMEBqgGqAMIBvgG/AMMB1gHXAMUB3AHcAMcB3gHfAMgB5AHlAMoB5wHxAMwB9gH5ANcCAAIDANsCDQIOAN8CEQISAOECFQIXAOMCGQIaAOYCHAIrAOgCMgIzAPgCNQI/APoCQgJDAQUCRgJHAQcCSwJLAQkCTgJPAQoCUQJSAQwCWQJaAQ4AAQ+sAAQAAAAeAEYAUABeARgBGAE6AUQCygNkBPYAUAaABpYHkAfKCIwJZgpACqIAUAtsC5INGA1ODWgNgg2MDZoO+A9uAAIAXgAQAT8AEAADAF0AKAC+ACgAwAAoAC4AJ//6ACv/+gAz//oANf/6AEf/zgBI/84ASf/OAEv/zgBR/84AUv/OAFP/zgBV/84AVv/OAFf/2ABZ/+IAXf/2AF7/2ACI//oAk//6AJT/+gCV//oAlv/6AJf/+gCo/84Aqf/OAKr/zgCr/84ArP/OALH/zgCy/84As//OALT/zgC1/84Atv/OALf/zgC6/+IAu//iALz/4gC9/+IAvv/2AMD/9gDy/84BE//6ART/zgEi/9gBP//YAAgASgAKAF0ACgBeAAoAvgAKAMAACgE/AAoCbgAKAm8ACgACAG3/7AJZ/+wAYQAG//YAC//2ACX/+wAm//sAJ//cACj/+wAp//sAKv/7ACv/3AAs//sALf/7AC7/8QAv//sAMP/7ADH/+wAy//sAM//cADT/+wA1/9wANv/7ADf/7AA4//MAOf/pADr/7AA7/+MAPf/YAEf/4gBI/+IASf/iAEr/5wBL/+IATf/4AFH/7ABS/+wAU//iAFX/4gBW/+wAV//4AFn/1gBd/90AXgAEAG3/2ACB//sAgv/7AIP/+wCE//sAhf/7AIb/+wCI/9wAjf/7AI7/+wCP//sAkP/7AJL/+wCT/9wAlP/cAJX/3ACW/9wAl//cAJr/6QCb/+kAnP/pAJ3/6QCe/9gAqP/iAKn/4gCq/+IAq//iAKz/4gCu//gAsf/iALL/7ACz/+IAtP/iALX/4gC2/+IAt//iALr/1gC7/9YAvP/WAL3/1gC+/90AwP/dAPL/7AET/9wBFP/iASH/7AEi//gBOf/YAT8ABAJO/+wCT//4AlH/7AJS//gCWf/YAm7/5wJv/+cAJgA5AAUARwAGAEgABgBJAAYASwAGAE0ABQBRAAUAUgAFAFMABgBVAAYAVgAFAF0AGQBeABIAmgAFAJsABQCcAAUAnQAFAKgABgCpAAYAqgAGAKsABgCsAAYArQAFAK4ABQCvAAUAsAAFALEABgCyAAUAswAGALQABgC1AAYAtgAGALcABgC+ABkAwAAZAPIABQEUAAYBPwASAGQABgAUAAsAFAAQ/7oAEv+6ACX/sgAm//wAJ//dACj//AAp//wAKv/8ACv/3QAs//wALf/8AC7/jgAv//wAMP/8ADH//AAy//wAM//dADT//AA1/90ANv/8ADf/7AA4AAwAOgAKADsABQA9AA4APv/7AEX/kgBG//wAR/+SAEj/kgBJ/5IASv/qAEv/kgBM//wATf/4AE///ABQ//wAUf+hAFL/oQBT/5IAVf+SAFb/oQBX/5wAWf+hAF3/sABe/7oAbf/EAHz/4gCB/7IAgv+yAIP/sgCE/7IAhf+yAIb/sgCI/90Ajf/8AI7//ACP//wAkP/8AJL//ACT/90AlP/dAJX/3QCW/90Al//dAJ4ADgCh/5IAov+SAKT/kgCm/5IAp/+SAKj/kgCp/5IAqv+SALH/kgCy/6EAs/+SALT/kgC2/5IAuv+hALv/oQC9/6EAvv+wAMD/sADy/6EBE//dART/kgEh/+wBOQAOAT7/+wJPABQCUgAUAln/xAJa/+ICXQAPAl4ADwJu/+oCb//qAGIABv/6AAv/+gAl//wAJv/6ACf/3gAo//oAKf/6ACr/+gAr/94ALP/6AC3/+gAu//sAL//6ADD/+gAx//oAMv/6ADP/3gA0//oANf/eADb/+gA4//oAOf/yAD3/9ABF/+4ARv/8AEf/6gBI/+kASf/qAEv/6QBM//wAT//8AFD//ABR//QAUv/0AFP/6gBV/+kAVv/0AFf/9gBZ/+gAXf/2AF4AFABt/+wAgf/8AIL//ACD//wAhP/8AIX//ACG//wAiP/eAI3/+gCO//oAj//6AJD/+gCS//oAk//eAJT/3gCV/94Alv/eAJf/3gCa//IAm//yAJz/8gCd//IAnv/0AKH/7gCi/+4Ao//uAKT/7gCl/+4Apv/uAKf/7gCo/+oAqf/qAKr/6gCr/+oArP/qALH/6gCy//QAs//qALT/6gC1/+oAtv/qALf/6gC6/+gAu//oALz/6AC9/+gAvv/2AMD/9gDy//QBE//eART/6gEi//YBOf/0AT8AFAJO//gCUf/4Aln/7AAFAEr/7ABX/+wBIv/sAm7/7AJv/+wAPgAGAAUACwAFAEX/8gBG//sAR//kAEj/5gBJ/+QASv/8AEv/5gBM//sATf/8AE//+wBQ//sAUf/3AFL/9wBT/+QAVf/mAFb/9wBX//QAWf/lAF3/+ABeAAUAbf/iAKD/+wCh//IAov/yAKP/8gCk//IApf/yAKb/8gCn//IAqP/kAKn/5ACq/+QAq//kAKz/5ACt//wArv/8AK///ACw//wAsf/kALL/9wCz/+QAtP/kALX/5AC2/+QAt//kALr/5QC7/+UAvP/lAL3/5QC+//gAwP/4APL/9wEU/+QBIv/0AT8ABQJOAAUCUQAFAln/4gJu//wCb//8AA4ABv/2AAv/9gBK//YAXf/4AL7/+ADA//gCTv/2Ak//9gJR//YCUv/2Al3/4gJe/+ICbv/2Am//9gAwAAYAFAALABQAEP+6ABL/ugBF/+UAR//xAEj/8QBJ//EASgATAEv/8QBT//EAVf/xAFf//ABdABAAXgAKAG3/9gB8ABQAof/lAKL/5QCj/+UApP/lAKX/5QCm/+UAp//lAKj/8QCp//EAqv/xAKv/8QCs//EAsf/xALP/8QC0//EAtf/xALb/8QC3//EAvgAQAMAAEAEU//EBIv/8AT8ACgJOAB4CTwAUAlEAHgJSABQCWf/2AloAFAJuABMCbwATADYABgAKAAsACgAQAAgAEgAIAEX/+QBGAAYAR//2AEj/9gBJ//YASgAKAEv/9gBMAAYATwAGAFAABgBRAAYAUgAGAFP/9gBV//YAVgAGAFf//QBdAAwAXgAcAHwAHgCgAAYAof/5AKL/+QCj//kApP/5AKX/+QCm//kAp//5AKj/9gCp//YAqv/2AKv/9gCs//YAsf/2ALIABgCz//YAtP/2ALX/9gC2//YAt//2AL4ADADAAAwA8gAGART/9gEi//0BPwAcAk8AFAJSABQCWgAeAm4ACgJvAAoANgAGABQACwAUABD/2AAS/9gARf/vAEb/+wBH//kASP/4AEn/+QBKAA4AS//4AEz/+wBP//sAUP/7AFH/+wBS//sAU//5AFX/+ABW//sAV//8AF0ADgB8AA4AoP/7AKH/7wCi/+8Ao//vAKT/7wCl/+8Apv/vAKf/7wCo//kAqf/5AKr/+QCr//kArP/5ALH/+QCy//sAs//5ALT/+QC1//kAtv/5ALf/+QC+AA4AwAAOAPL/+wEU//kBIv/8Ak4AFAJPABQCUQAUAlIAFAJaAA4CbgAOAm8ADgAYAAYACgALAAoAEP/YABL/2ABF//wASgAMAFf//QBdAA8AfAAKAKH//ACi//wAo//8AKT//ACl//wApv/8AKf//AC+AA8AwAAPASL//QJOAAgCUQAIAloACgJuAAwCbwAMADIARf/wAEb/9gBH/+UASP/qAEn/5QBL/+oATP/2AE3/9gBP//YAUP/2AFH/9gBS//YAU//lAFX/6gBW//YAV//1AFn/6gBt//YAoP/2AKH/8ACi//AAo//wAKT/8ACl//AApv/wAKf/8ACo/+UAqf/lAKr/5QCr/+UArP/lAK3/9gCu//YAr//2ALD/9gCx/+UAsv/2ALP/5QC0/+UAtf/lALb/5QC3/+UAuv/qALv/6gC8/+oAvf/qAPL/9gEU/+UBIv/1Aln/9gAJADj/2AA6/+IAO//vAD3/0gBdAAoAnv/SAL4ACgDAAAoBOf/SAGEAEAAUABIAFAAm//EAJ//SACj/8QAp//EAKv/xACv/0gAs//EALf/xAC7/7AAv//EAMP/xADH/8QAy//EAM//SADT/8QA1/9IANv/xADf/2gA4/7AAOf/YADr/ugA7/84APf+mAD7/+wBF/90ARv/sAEf/3QBI/90ASf/dAEr/3QBL/90ATP/sAE3/7gBP/+wAUP/sAFH/7ABS/+wAU//dAFX/3QBW/+wAV//aAFn/2ACI/9IAjf/xAI7/8QCP//EAkP/xAJL/8QCT/9IAlP/SAJX/0gCW/9IAl//SAJr/2ACb/9gAnP/YAJ3/2ACe/6YAoP/sAKH/3QCi/90Ao//dAKT/3QCl/90Apv/dAKf/3QCo/90Aqf/dAKr/3QCr/90ArP/dAK3/7gCu/+4Ar//uALD/7gCx/90Asv/sALP/3QC0/90Atf/dALb/3QC3/90Auv/YALv/2AC8/9gAvf/YAPL/7AET/9IBFP/dASH/2gEi/9oBOf+mAT7/+wJu/90Cb//dAA0ARv/7AEr/+ABM//sAT//7AFD/+wBd//QAXv/6AKD/+wC+//QAwP/0AT//+gJu//gCb//4AAYAOABGADoAMgA7ADIAPQAoAJ4AKAE5ACgABgA4AEYAOgA8ADsAPAA9ADwAngA8ATkAPAACAk//7AJS/+wAAwGW//YB7f/2AgH/9gBXABD/ugAS/7oBTv/iAVX/zgFX/+IBXP/OAWL/2AFjAAoBZ//OAW7/9gF2/+IBeQAKAXz/2AF+//gBf//4AYH/7AGE//gBhf/4AYb/+AGH/84BiP/4AYn/+AGK/+wBi//4AYz/+AGN/+wBkP/sAZH/7AGS//gBlP/4AZX/+AGX//gBmP/4AZr/+AGc/+wBnf/sAZ//+AGg/+wBov/4AaP/+AGl/84Bpv/4Aaj/+AGp//gBq//4Ab//7AHX//gB2//4Ad3/+AHh//gB4v/YAeQACgHn//gB6f/4Aez/4gHv//gB8f/4AfP/+AH1/+wB9//sAfj/9gH//+wCAP/iAg3/2AIQ//gCEf/OAhL/zgIU//gCFv/4Ahr/+AIc/84CHf/YAh7/zgIf/9gCIf/YAiP/7AIo/9gCKgAKAi//+AIx//gCM//sAjX/7AI3/+wCOAAKAkX/+AJJ/+wCS//sAB0AEP/iABL/4gFO/7oBV/+6AVr/4gFc//EBYv/TAW7/xAFv/+IBc//4AXb/ugGH/+wBpf/sAeL/0wHs/7oB+P/EAgD/ugIC//gCBP/4Ag3/0wIS/+wCF//4Ahz/8QIe//ECKP/TAjr/4gI8/+ICPv/iAkD/+AAPABD/9gAS//YBh//7AY7/4gGP/+4Blv/YAaX/+wGq/+4B7f/YAfn/4gIB/9gCEv/7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AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACAAAAAAAAP/s//0AAAAA//UAAP/8AAAAAAAA//wABQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA//YAAAAAAAAAAAAAAAAAAAAAAAAAAAAA/+z/7AAAAAD/2AAAAAAAAAAAAAAAAAAAAAAAAAAA/+QAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP/4AAAAAAAAAAAAAAAAAAAAAAAAAAD/+QAAAAAAAAAA/+IABf/2AAAAAAAAAAAABQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD//QALAAAAAAAAAAD/+wAAAAAAAAAA//YAAAAAAAAACv/sAAQAAAAAAAQAAP/8AAQAAP/8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAr/7AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAOAAAAAAAAAAAAAAAAAAAAAAAAAAAADgAKABQAAAAAAAD/9v/EAAD/7P/8//v/9AAA//v/+QAUAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAANAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA//sACgAA//wAAAAA//j/9wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA/+wAAAAA/+z/xP/s/9j/4v/YAAAAAP/s/7D/sP/YABT/kv/2AAD/+AAAAAD/7P/2AAAAAAAA/7AAAP+6/6YAFP/i/9j/zgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABIABAAA/+IAAP/2//v/9gAKAA8ADAAAAAAAAAAAAAAAAAAUAAAAAAAKAAAAAAAAAAAACgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP/7AAAAAP/EAAD/9v/7/9j/9v/sAAAAAAAAAAAAAAAAAAD/zv/2AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABQAAP/EAAAAFAAAAAoACgAKAAD/2AAUAAAAAAAAAAAAAP/n/7AAAP/iAAAAAP/sAAoAAP/2AAAAAAAAAAD/xAAAAAAAAP/i//b/7P/sAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAUAAj/xAAAAAoAAAAKAAYAAAAA/9gAFAAAAAAAAAAAAAD/7P+wAAD/7AAAAAD/5wAKAAD/+wAAAAAAAAAAAAAACgAAAAAAAP/2AAD/4gAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP/4/6b/9AAIAAAACgAGAAQAAP/YAAAAAAAAAAAAAAAA/8T/sP/y/8n/7gAA/8kAAP/i/9gAAAAAAAAAAAAAAAAAAAAAAAD/7P/s/+IACgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA/+wAAAAAAAAAAAAAAAAAAAAA//b/4gAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP+cAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/4gAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA/8b/oQAZ/9j/7P/EAAoAAAAAABT/ugAA//gAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA/+wAAAAAAAAAAAAA//YAAAAAAAAAAAAAAAAAAAAAAAAAAP/i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AAAAAAAAAAAAAAAAAAD/6P/2AAoAAAAA//kAAAAAAAAAAAAAAAAAAP/2AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP/6AAD/+wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP/2AAAAAAAAAAAAAP/xAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/5gAA/+4AAAAAAAD/9gAAAAD/9v/2AAD/9AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/+AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP/4AAAAAAAAAAAAAAAAAAD/+AAA//sAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACgAAAAAAAAAA/8kAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMAAgACgAA/+r/5v/gAAAAAAAAAAAAAAAA//oAAAAA//z/+gAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP/JAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABwAAAAAAAP/s/+z/8QAAAAAAAAAAAAAAAAAAAAAAAP/7AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/zv/YAAAAAAAAAAD//AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA/7D/0/+mAAAAAAAAAAAAAAAA/8r/9gAA//L/+wAAAAAAAP/6AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA/84AAAAAAAAAAAAAAAAAAAAAAAAAAAAA//YAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP/4AAAAAP/4//QAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEABgJZ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AY8BjwAqAZABkAAmAZEBkQAtAZIBkgAuAZMBkwAiAZQBlQAuAZYBlgAjAZcBmAAuAZkBmQAxAZoBmgAuAZwBnQAmAZ8BnwAuAaABoAAmAaIBowAuAaUBpQAnAaYBpgAuAagBqQAuAaoBqgAqAasBqwAuAb4BvgAwAb8BvwAmAdcB1wAuAdsB2wAyAd0B3QAuAeEB4QAuAeIB4gApAeMB4wArAeQB5AAvAeUB5QAxAecB5wAuAekB6QAuAewB7AAfAe0B7QAjAe8B7wAuAfEB8QAuAfMB8wAuAfQB9AAwAfUB9QAmAfYB9gAwAfcB9wAmAfgB+AAoAfkB+QAhAf4B/gAsAf8B/wAtAgACAAAfAgECAQAjAgICAgAeAgMCAwAiAgQCBAAeAgUCBQAiAg0CDQApAg4CDgArAhACEAAuAhECEQAgAhICEgAnAhQCFAAuAhYCFgAuAhcCFwAeAhgCGAAiAhoCGgAuAhwCHAAkAh0CHQAlAh4CHgAkAh8CHwAlAiECIQAlAiMCIwAmAigCKAApAikCKQArAioCKgAvAisCKwAxAi8CLwAuAjECMQAuAjICMgAwAjMCMwAmAjUCNQAmAjYCNgAwAjcCNwAmAjgCOAAvAjkCOQAxAjoCOgA0AjsCOwAqAjwCPAA0Aj0CPQAqAj4CPgA0Aj8CPwAqAkACQAAeAkECQQAiAkUCRQAuAkgCSAAsAkkCSQAtAkoCSgAsAksCSwAtAkwCTQAzAk4CTgAOAk8CTwAcAlECUQAOAlICUgAcAlkCWQAPAloCWgAQAl0CXgARAm4CbwAMAAIAOwAGAAYAAAALAAsAAQAQABIAAgAlAC4ABQAwADQADwA2ADcAFAA5ADsAFgA9AD4AGQBFAE4AGwBQAFAAJQBSAFUAJgBXAFcAKgBZAFkAKwBdAF4ALABtAG0ALgB8AHwALwCBAJAAMACSAJYAQACaAJ4ARQChAKYASgCoALAAUACyALcAWQC6AL4AXwDAAMAAZADyAPIAZQECAQIAZgEUARQAZwEhASIAaAE5ATkAagE+AT8AawFMAU8AbQFSAVMAcQFVAVYAcwFYAWMAdQFmAWsAgQFtAW8AhwFxAXwAigF+AZ0AlgGfAZ8AtgGiAaMAtwGlAaYAuQGoAasAuwG+Ab8AvwHWAdcAwQHcAdwAwwHeAd8AxAHkAeUAxgHnAfEAyAH2AfkA0wIAAgUA1wIMAg4A3QIRAhMA4AIVAisA4wIuAjMA+gI1AkcBAAJKAk8BEwJRAlIBGQJZAloBGwJdAl4BHQAAAAEAAAAA2DmvhQAAAADK8ZoIAAAAAMrxmgg=) format('truetype');
												font-weight: normal;
												font-style: normal;
											}
											@@font-face {
												font-family: 'LouisBoldItalic';
												src: url(data:application/font-ttf;charset=utf-8;base64,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/+sAW//zAFz/8QBg//YAsv/sAMgAXAABAA7/9gAMACb/1AAv/+wAVP/1AFsACQBeAAcAiP/OAK4ACACwACAAsQAPALL/7wDEABQAyAAiAAEAGP/2ABIAFv/lABf/7QAY/+0AHP/tACb/9AAv/+MAOP/4ADn/xwA7/+YAPP/tAD3/5AA+/9MAP//tAEv/9wBZ//cAXf/pAF//7wCI//cAEgAM/4IAFf/1ABb/4QAb//YAHP/vADT/7QA5/8YAOv/xADv/xgA8/9EAPv/CAEv/+ABZ//AAW//cAFz/5ABe/9wA5/9zAOj/cwAYABT+5wAV/+sAGf/WABv/6gAd/+0AHv/2ACb/wwAv//EANP/nADj/8ABG/9cAS//zAFP/5ABU/9IAWP/VAFr/5gBb//YAXP/1AF3/7wBf/+YAiP++AJEACACy/+gAwwARAAcADv/tABP/9QAU/+gAJv/2AEH/8wBC/+sA9P/yAAMAEv/1AHn/7wD8//YAAQAU//AACgAM/+sADv/xABb/9AAc//MAOf/sADv/8wA+//EAQf/uAEL/8QBy/+gAAQAU//IAAQAU//MADAAS/+YAE/++ABT/xAAZ/+wAJv/YADsABgA+ABAAZf/sAHn/5gCI/9QA9P/FAPz/6AAEAA7/8AAU//EAQf/2AEL/8AAHAA7/7QAT//QAFP/oACb/9gBB//QAQv/sAPT/8gAJAAz/9wA5/8kAO//kADz/6wA+/9MAWf/4AFv/9gBe//cA6P/CAAUAOf/nADv/8AA8//YAPv/mAOj/7gAbAAz/0gAP/9QAEv/0ABX/9gAW/+sAG//2ACT/6wA0/+4AOf/JADr/7wA7/9gAPP/fAD7/zQBB/8kAQv/zAEv/9wBU//UAWf/oAFr/+ABb/+MAXP/nAF7/4wBw//QAsv/3AOf/ywDo/8sA9v/RABMADv/vABT/8wAm//gAL//5ADn/7wA7//QAPP/5AD3/9gA+/+0AQf/yAEL/7ABL//gAWP/5AFn/9wBb//gAXP/5AF3/8wBe//gAX//7AAsAEv/2ADT/8QA4//sAU//7AFT/8QBY//kAWv/5ALAAFwCy//MAxAAKAMgAGQAGADT/9gBL//oAVP/2AFn/+ABa//kAsv/2ABYAE//IABT/0gAm/9UAL//vAEb/6gBL//kAU//xAFT/8QBY/+sAWf/7AFr/9ABd//cAX//wAIj/zwCh//gArgAOAK//9gCwABUAsv/sAMQACQDF//EAyAAXAA0ADv/2ACb/+QA5//gAO//4AD3/+wA+//UAQv/2AEv/9wBZ//cAW//3AFz/+ABd//oAXv/4AAsARv/6AEv/+QBN//sAUf/7AFP/+wBU//cAWP/6AFn/+ABa//sAX//7ALL/9wARABP/9wAU/+sAJv/3AC//+ABG//kAS//5AE3/+wBR//sAU//7AFT/+ABY//YAWf/5AFr/+gBd//oAX//4AIj/9wCy//UAEgAS/+UANP/hADj/9ABG//cAS//3AFP/+wBU/9cAWf/zAFr/6gBb/+YAXP/jAF7/5gBw//YArgAiALEAEwCy/98AxAAQAPL/8wAZAAz/qQAP/6gAEv+/ABb/6gAk/+gANP/sADn/uAA6/+8AO//EADz/yAA+/74AQf+8AEv/8wBU//cAWf/hAFr/+QBb/8QAXP/PAF7/xABw//YAef+2ALL/+wDn/6kA6P+pAPb/qgAZAA7/6gAT/+0AFP/kACb/7gAv/+sAOf/tADv/9AA8//kAPf/nAD7/6QA///QAQf/uAEL/5wBG//sATf/7AE7/+wBR//sAU//7AFj/+gBd//QAX//5AGL/9QCI/+0Asv/7APb/9gAPAA7/7gAT/8EAFP/QACb/2gAv/+EAPf/tAD7/+AA///kAQv/tAEb/9gBU//oAWP/6AIj/zwCy//MA8v/2AAMAQv/0AE8APQBi//sADQAv//sAOf/2ADv/9wA+//MAQf/0AEb/9gBL//sAU//7AFT/8QBZ//oAWv/3ALL/7ADy//EADgAU//UAJv/5ADv/+wA+//oAS//1AFP/+gBY//oAWf/0AFr/+gBb//YAXP/2AF3/9wBe//YAX//6ACYAC//yABL/xwAT/8YAFP/JABn/4AAf/8kAJf/lACb/yQAv//MANP/tAEb/twBL/+sATf/7AE7/+wBR//sAU//PAFT/wABY/7gAWf/2AFr/zABb/7wAXP+/AF3/vQBe/7wAX/+4AHD/9gCI/8oAof/zAK4AGACv/+wAsAAgALEACACy/9gAxAAUAMX/zwDIACIA8v/CAPP/ygASABP/8AAU/+YAJv/vAC//8gBG//YAS//6AE3/+gBO//oAUf/6AFP/+wBU//cAWP/zAFn/+QBa//cAXf/6AF//9gCI/+0Asv/zACIAC//yABL/5gAT/8YAFP/HABYABQAZ/+wAH//kACX/8AAm/9gAL//zADT/9AA4//oARv/VAEv/+gBT/+IAVP/VAFj/1ABZ//sAWv/nAF3/9wBf/+oAiP/TAKH/9ACuADMAr//yALAAGwCxABsAsv/aAMQAHwDF/+IAyAAdAPL/3QDz/+8A9gANAB0AC//2ABL/7QAT/9EAFP/QABn/8wAf/+sAJf/2ACb/3wAv//QANP/5AEb/2wBT/+cAVP/aAFj/2QBa/+sAXf/7AF//7wCI/9kAof/1AK4AKwCv//QAsAAYALEAGQCy/90AxAASAMX/5wDIABoA8v/kAPP/9QAQABL/5QA0/+cARv/5AEv/9ABT//oAVP/gAFn/8ABa/+sAW//oAFz/5QBe/+gAcP/2AK4AFACxAAgAsv/nAPL/8QAnAAv/7gAS/9MAE//CABT/xgAWAA8AGf/gABv/9gAf/9QAJf/kACb/zQAv//YANP/pADj/9wBG/88AS//1AFP/3QBU/8QAWP/MAFn/+QBa/9wAW//4AFz/8wBd/+sAXv/3AF//2ABw//EAiP/JAKH/8gCuAD0Ar//tALAAFgCxACYAsv/UAMQAJgDF/9wAyAAXAPL/ywDz/+AA9gAXAA0AEv/vADT/9ABL//cAU//5AFT/8gBZ//YAWv/1AFv/+ABc//YAXv/4ALAAEgCy//QAyAASABcAFf/rABn/5QAb/+sAHf/wAB7/9gAm//MANP/nADj/8wBG/+oAS//2AE8AWgBT/+4AVP/jAFj/6ABZ//YAWv/rAFv/8gBc//AAX//uAIj/9QCuABAAsv/sAMgAWgAPAAz/ywAW/+wAJgAHADT/9QA5/9cAOv/0ADv/2AA8/+EAPv/UAFn/9QBb/+kAXP/uAF7/6QCIABMA6P/CABcADv/wAC7/+wAv//kANP/6ADj/+wA5/8sAOv/4ADv/3QA8/+YAPf/7AD7/0QA///cAQf/eAEL/6QBL//wAWf/6AFv/+gBc//sAXv/6AGL/9QDn//AA6P/wAPb/6QARAA7/8wAS//EANP/2ADj/+gA5/7AAOv/6ADv/6gA8/+8APv/UAEH/7ABC//AARv/8AFT/9QBY//wAsv/0APL/8wD2/+8ABgAu//sAL//4ADT/+wA5//sAOv/6AD//+gAYAA7/6QAU/+8AJv/5AC7/+gAv//cAOP/5ADn/swA6//kAO//VADz/3AA9/+8APv/HAD//8gBB/98AQv/nAEv//ABZ//wAW//6AF3/9QBe//oAYv/0AOf/7gDo/+4A9v/sABgAEv/hABP/4AAU/+EAH//sACb/2AAv//oAOQATADsALgA8ACYAPQAQAD4AOQBCAA4ARv/7AFT/8wBY//wAiP/UAK4ASgCwABIAsQAxALL/4QDEAC4AyAAUAPL/5QD2ABwAAgBPABAAyAAQAAwAEv/kADT/7wA5/78AOv/7ADv/+AA+/+0ARv/3AFT/5wBY//sAsv/mAPL/7AD2//AABwAu//sAL//4ADT/+wA5//sAOv/6AD//+gB5/7cAGAAO//AAD//4AC7/+wAv//kANP/6ADj/+wA5/8sAOv/4ADv/3QA8/+YAPf/7AD7/0QA///YAQf/dAEL/6QBL//sAWf/5AFv/+gBc//oAXv/5AGL/9QDn/+8A6P/vAPb/6AAeAAz/9QAO/+UAD//1ABT/6wAk//YAJv/1AC7/9wAv//IAOP/1ADn/wAA6//cAO//VADz/2gA9/+AAPv/EAD//7ABB/9sAQv/jAEv/+ABZ//gAW//3AFz/+QBd/+0AXv/2AF//+gBi/+8AiP/2AOf/5gDo/+YA9v/oAAEATwA6ABQAC//zAA7/7gAS/+QAE//TABT/1QAf//AAJv/UAC//1gA5/7wAPf/iAD7/+QA///IAQv/tAEb/+wBU//YAYv/2AIj/0wCy/+QA8v/lAPb/+AARAA7/7AAU//YAJv/7AC7/+gA0//sAOf+5ADr/9wA7/+IAPP/nAD3/+AA+/9MAQf/mAEL/6wBb//wAXf/8AGL/9gD2/+kACQA5/8YAOv/6ADv/7gA8//QAPv/fAEH/8ABC//QAVP/8APb/7QAOAA7/8QAu//sAL//4ADT/+wA5/88AOv/7ADv/4gA8/+cAPf/5AD7/3QA///kAQf/sAEL/7gD2/+4AEgAO//MADwAJABP/3AAU/9gAH//3ACb/4wAv/+YAOf+8AD3/6AA+//gAP//6AEL/8gBG//kAVP/3AFj/+QCI/+AAsv/vAPL/7gAQAA7/8QAT/+MAFP/dACb/5wAv/+QAOf+/AD3/5gA+//MAP//4AEL/8ABG//sAVP/5AFj//ACI/+MAsv/yAPL/8gALABL/6AA0//QAOf++ADr/+gA7//gAPv/tAEb/+wBU/+wAsv/qAPL/6gD2//MAEgAO//UADwAGABP/3AAU/9sAH//3ACb/4wAv/+gAOf+8AD3/6QA+//gAP//6AEL/9QBG//kAVP/3AFj/+gCI/+AAsv/vAPL/7gAOABL/7wAu//sANP/6ADn/twA6//YAO//sADz/8AA+/9oAQf/uAEL/8ABU//kAsv/5APL/8QD2/+sACAA0//QARv/2AE8AUgBU/+8AWP/2AFr/9ACy//UAyABSAAIATwAzAMgAMwAGACb/8wAv//QAOf/0AD3/9AA+//AAiP/0AAEAGf/oAAUAFv/jABf/5wAY/+YAHP/sAFH/twAEADn/7AA7/+4APP/xAD7/7QABAGIACgARAA7/6gAT/+AAFP/iACb/7QAv/+gAOf/uADv/9AA8//oAPf/iAD7/6AA///MAQf/vAEL/5wBd//kAX//7AIj/7QD2//QAFgAO//AAFP/xACb/+QAu//gAL//5ADT/+wA4//sAOf/xADr/9gA7//AAPP/zAD3/8QA+/+4AP//5AEH/9gBC//QAS//6AFn/+QBb//kAXP/7AF3/9wBe//kABQAPABIAQQAJAEIAGwBiABAA9gAsAAMADwApACQAEQD2ACMABAAPABUAQgAQAGIABgD2ACIAHQAM//cADv/tAA//9wAU/+0AJv/2AC7/+gAv//QAOP/4ADn/2AA6//cAO//eADz/4wA9/+IAPv/ZAD//7gBB//AAQv/tAEv/+wBZ//oAW//5AFz//ABd//MAXv/6AF///ABi//YAiP/5AOf/7gDo/+4A9v/xAAYADgAPAA8AEgBBABoAQgAdAGIAEgD2ACsAAQBPABAABAAPACcAJAAPAE8AEAD2ACIACQAT/2kAJv/DAC//8gA0//UARv/rAFT/3ABY/+gAiP+/ALL/7QAPAAv/6gAT/2kAFP+sAB//tAAl/+QAJv/DAC//8gA0//UARv/rAFT/3ABY/+gAcP/0AIj/vwCy/+0A8v/wAAQAOf/KADv/7wA8//UAPv/gABAADP/yAC//8wA5/8IAO//dADz/5AA9//AAPv/LAD//9QBL//MAWP/2AFn/8wBb/+4AXP/yAF3/6wBe/+4AX//0AAQAFf/1ABYACgAZ/9YAG//0AAYAJv/lAC//7wCI/+IArgAHALAADwDIABEABAAW/+cAF//uABj/7QAc/+4AAgAaAAsAEAAAABIAFQAGABcAHwAKACUAKAATACoALAAXAC4AMQAaADQAQQAeAEYARgAsAEgASwAtAE4AUQAxAFMAVAA1AFYAYgA3AHAAcABEAHIAcgBFAHkAeQBGAIEAgQBHAJAAkABIAKAAoQBJAK8AsgBLAMQAxABPAMYAxgBQAMgAyABRAOcA6ABSAPIA9ABUAPYA9gBXAPwA/ABYAAAAAQAAAAoAHAAeAAFsYXRuAAgABAAAAAD//wAAAAAAAAAAAAEAAAAA) format('truetype');
												font-weight: normal;
												font-style: normal;
											}
											@@font-face {
												font-family: 'LouisBold';
												src: url(data:application/font-ttf;charset=utf-8;base64,AAEAAAAPAIAAAwBwRkZUTW4c46AAAVG4AAAAHEdERUYFEwdbAAEO/AAAACpHUE9Tld4r7wABD4AAAEI2R1NVQtvl3zQAAQ8oAAAAWE9TLzKty2g1AAABeAAAAGBjbWFw5boPAgAAC7AAAANmZ2FzcP//AAMAAQ70AAAACGdseWbEiWGGAAAUCAAA43hoZWFk+SHEvQAAAPwAAAA2aGhlYQgpBakAAAE0AAAAJGhtdHihmm81AAAB2AAACdhsb2NhLiL1sgAADxgAAATubWF4cALAAG8AAAFYAAAAIG5hbWX3C9hLAAD3gAAABXZwb3N0/7YeAwAA/PgAABH8AAEAAAABTMwS4wg3Xw889QALA+gAAAAAyvGaDQAAAADK8ZoN/5r+4QSFBBEAAAAIAAIAAAAAAAAAAQAABAf/DAAABIz/mv+aBIUAAQAAAAAAAAAAAAAAAAAAAnYAAQAAAnYAbAAIAAAAAAACAAAAAQABAAAAQAAAAAAAAAACAdIBwgAFAAACvAKKAAAAjAK8AooAAAHdADIA+gAAAAAAAAAAAAAAAIAAAq9QACBKAAAAAAAAAABwc3kAAAAADfsCAu7/BgAABAcA9CAAAJcAAAAAAjAC7gAAACAAAgJEACgAAAAAAU0AAADcAAAA3AAAARwAQAGgADACQAAeAkAANgJAAAQCjwAmANoAMAF0AD0BdAAIAdIAGAJAABkA/wAmAboAJgEJADEBuAAQAl4APwGTABQCLgA0AjwAIgJPACACSwA+AlUAPwIGABoCTgAtAlUANgEaADoBGgA2AjEAMgIiAC0CMQAyAfoAHgOkACsCZAAKAm0ASgJlAD8CeQBKAjUASgIyAEoCewA/ApoASgEmAEoCNwArAncASgIyAEoDEABKApoASgJ3AD8CcwBKAncAPwJtAEoCWQApAjUAFAKFAEYCZgAKA5YAGAJ0AAcCagABAlUAKwGLAFMBuAAQAYsADQH8ABwBuv/+AhgAYwIuADACRABHAh0ANQJEADUCKQA1AX4AEgJEADUCUwBHARUARAEf/9ICMABHARsARwN2AEcCUwBHAioANQJEAEcCRAA1Ab4ARwH3ACQBlAASAlMARAIQAAcDEgAPAhIACgIkAAcCBgAkAb0AHgFUAGwBvQAKAj0AKADcAAABHABAAkAARwJAABYCQP/9AVQAbAIZADACGABgA1YAOgFdACYCNAAaAkQAKAHmAC4CGABmAUIAJgJAABkCRAAoAkQAKAIYAKQCVgBHAnoANQEJADECGACSAkQAKAFcACgCNAAwAkQAKAJEACgCRAAoAf0AJAJkAAoCZAAKAmQACgJkAAoCZAAKAmQACgON//YCZQA/AjUASgI1AEoCNQBKAjUASgEm/8wBJgBKASb/vAEm/+cCmwAMApoASgJ3AD8CdwA/AncAPwJ3AD8CdwA/AkAAIgJ3AD8ChQBGAoUARgKFAEYChQBGAmoAAQJzAEoCcABHAi4AMAIuADACLgAwAi4AMAIuADACLgAwA1AAMAIdADUCKQA1AikANQIpADUCHwA3AQ3/uwENAEABDf+vAQ3/2gInADMCUwBHAioANQIqADUCKgA1AioANQIqADUCQAAZAioANQJTAEQCUwBEAlMARAJTAEQCJAAHAkYASQIkAAcCZAAKAi4AMAJkAAoCLgAwAmQACgIuADACZQA/Ah0ANQJlAD8CHQA1AmUAPwIdADUCZQA/Ah0ANQJ5AEoCowA1ApoADAJMADUCNQBKAikANQI1AEoCKQA1AjUASgIpADUCNQBKAikANQI1AEoCKQA1AnsAPwI0ADUCewA/AkQANQJ7AD8CRAA1AnsAPwJEADUCmgBKAlMARwKa/+wCW//2ASb/yQEV/70BJv/uARX/5QEm/+ABFf/XASYACgEV//8BJgBKARUARANTAEoCNABEAjcAKwEf/7sCdwBKAjAARwIwAEcCMgBKARsARwIyAEoBGwBHAjIASgF6AEcCMgBKAacARwJdAA0BcgANApoASgJTAEcCmgBKAlMARwKaAEoCUwBHAscACgKaAEoCUwBHAncAPwIqADUCdwA/AioANQJ3AD8CKgA1A3IAPwNcADUCbQBKAb4ARwJtAEoBvgBHAm0ASgG+ACYCWQApAfcAJAJZACkB9wAkAlkAKQH3ACQCWQApAfcAJAI1ABQBlAASAjUAFAH4ABICNQAUAZ4AHAKFAEYCUwBEAoUARgJTAEQChQBGAlMARAKFAEYCUwBEAoUARgJTAEQChQBGAlMARAOWABgDEgAPAmoAAQIkAAcCagABAlUAKwIGACQCVQArAgYAJAJVACsCBgAkAU4AEgJEACgBH//SAhgANQIYADUCGABZAhgAxQIYAJECGACNAhgAPwIYAFkCRAAoAjUASgI1AEoC+AAUAiUASgJqAD8CWQApASYASgEm/+cCNwArA7EABgOhAEoC+QAUAowASQKaAEoCVwAOApIASgJkAAoCfQBKAm0ASgIlAEoDFgAOAjUASgPgAAMCUQAoApoASgKaAEoCjABJAqIACAMQAEoCmgBKAncAPwKSAEoCcwBKAmUAPwI1ABQCVwAOAxwALgJ0AAcCzgBKAn4AOgN2AEoDsgBKAwoAFANaAEoCfQBKAmoALgN0AEoCbQAXAi4AMAItADgCKgBHAesARwKuAAoCKQA1A1MABQIAACUCWABHAlgARwJGAEcCVAAKAroARwJPAEcCKgA1AkoARwJEAEcCHQA1AfUAEgIkAAcCyAA1AhIACgJzAEcCNAAsAxoARwNEAEcClgASAwgARwI1AEcCHQArAwEARwIoABQCKQA1AikANQJb//YB6wBHAh0ANQH3ACQBFQBEAQ3/2gEf/9IDPAAKAzcARwJb//YCRgBHAlgARwIkAAcCSgBHA34AOgL2ADUC6QAOAnMACgNmAEoC/gBHArgABAJoAAUDyQBKA2QARwMGADgCqAA2BBwASgOhAEcCTgAaAhEAIQMaADoC7ABCAncAPwIqADUClwAKAjgABwKXAAoCOP/1BIwAPwQ0ADUCdwA/AioANQN+ADoC9gA1A34AOgL2ADUCdQA/AiQANQJLABAB9ABBAfQAIQH0AEEB9AA8AfQAHgNBACADQQAeAtQASgKCAEcCwgAUAl8ADAJzAEoCRABHAhsASgHrAEcCVAANAhIAEAKIAEoCMgBHBAgAAwNpAAMCUQAoAgAAJQK/AEoCXwBHAqoASQJiAEcCuAAMAmUADwMZABQCrwASAtYASgJ5AEcDJwBKAroARwPpAEoDVwBHAr4APwJbADUCZQA/Ah0ANQI1ABQB9QASAmoAAQIQAAcCagABAhAABwKiAAcCKwAKAzQAFAK4ABICugA6Al4ALAKSADoCSAAsAn4ASgI0AEcDCgAIApoACAMKAAgCmgAIASYASgPgAAMDUwAFApsASQJGAEcC4AAIAn4ACgKaAEoCTwBHAtgASgJ5AEcCfgA6AjEALANOAEoC5ABHARUARAJkAAoCLgAwAmQACgIuADADjf/2A1AAMAI1AEoCKQA1AnkAPwIpADQCeQA/AikANAPgAAMDUwAFAlEAKAIAACUCQgAZAhUAFgKaAEoCWABHApoASgJYAEcCdwA/AioANQJ3AD8CKgA1AncAPwIqADUCagAuAh0AKwJXAA4CJAAHAlcADgIkAAcCVwAOAiQABwJ+ADoCNAAsAn0ASgHrAEcDWgBKAwgARwJUAA0CEgAQApMABwIvAAoCfgAMAhwADwJGACsCvgArAPcAKAD0ACgA/wAmAdMAKAHQACgB5wAmAgIAJgIMACsBWQA6Au0AMQO+ACEBSQAaAUkAMAFW/5oCQAAJAk4AGgJLABACRAAoAkQAKAJEACgCRAAoAkQAKAJAAC0CRAAoAkQAKAJEACgCRAAoAkAAMgJAADoCQAA6AkQAKAGQAAACYwASAnQAEgIYAFkDVgA6A1YAOgH0AAAB9AC9AL8AJgAAAAMAAAADAAAAHAABAAAAAAFcAAMAAQAAABwABAFAAAAATABAAAUADAAAAA0AfgCjAKwBfwGSAjcCxwLdA8AE/yAUIBogHiAiICYgMCA6IEQgrCEgISIhJiICIgYiDyISIhoiHiIrIkgiYCJlJcr4//sC//8AAAAAAA0AIACgAKUArgGSAjcCxgLYA8AEACATIBggHCAgICYgMCA5IEQgrCEgISIhJiICIgYiDyIRIhoiHiIrIkgiYCJkJcr4//sB//8AAf/2/+T/w//C/8H/r/8L/n3+bf2L/UziOeI24jXiNOIx4ijiIOIX4bDhPeE84TngXuBb4FPgUuBL4EjgPOAg4AngBtyiCW4HbQABAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAGAgoAAAAAAQAAAQAAAAAAAAAAAAAAAAAAAAEAAgAAAAAAAAADAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEAAAAAAAQABQAGAAcACAAJAAoACwAMAA0ADgAPABAAEQASABMAFAAVABYAFwAYABkAGgAbABwAHQAeAB8AIAAhACIAIwAkACUAJgAnACgAKQAqACsALAAtAC4ALwAwADEAMgAzADQANQA2ADcAOAA5ADoAOwA8AD0APgA/AEAAQQBCAEMARABFAEYARwBIAEkASgBLAEwATQBOAE8AUABRAFIAUwBUAFUAVgBXAFgAWQBaAFsAXABdAF4AXwBgAGEAYgAAAIUAhgCIAIoAkgCXAJ0AogChAKMApQCkAKYAqACqAKkAqwCsAK4ArQCvALAAsgC0ALMAtQC3ALYAuwC6ALwAvQJUAHEAZQBmAGkCVgB3AKAAbwBrAl4AdQBqAmkAhwCZAmYAcgJqAmsAZwB2AmACYwJiAUsCZwBsAHsAAACnALkAgABkAG4CZQFBAmgCYQBtAHwCVwBjAIEAhACWARMBFAJMAk0CUQJSAk4CTwC4AmwAwAE5AlsCXAJZAloCbgJvAlUAeAJQAlMCWACDAIsAggCMAIkAjgCPAJAAjQCUAJUCbQCTAJsAnACaAPIBQwFJAHABRQFGAUcAeQFKAUgBRAAAAAAAKgAqACoAKgAqAD4AUgCEAMoBCgFSAWABdgGMAbABxgHcAeoB9gIEAj4CUAKCArgC0gMIA1QDagOmA/AEAgQeBDIERgRaBIoE+AUSBVAFkgW4Bc4F4gYoBj4GSgZwBo4GngbABtwHHAdGB44HyAgACBIIPAhQCHAIigigCLgIygjYCOoI/gkMCRoJUgmSCcwKDgpICmgKvArkCvYLFgsuCzoLcgucC9QMFgxWDHIMqAzMDPQNCg0qDUQNZA16DbgNxg4EDjAOMA5EDo4Owg7kDvgPUA9iD74P9BASEDwQdBCCEKAQvBDmERARIBFIEWQRcBGWEcAR+BIWEkASahKUEsQS5hMIEywTbBOSE8IT5hREFGIUgBSgFMIU2BTuFQYVHhVOFZIV2hYkFnAW2BckFz4XjBe+F/AYJBhYGHgYpBjcGRwZXhmiGgIaRhqWGvAbRhuIG8wcEhxYHG4chBycHLQdAh1SHZId1B4YHngevB7UHyAfUB+CH7Yf6iASIFQgfiCeIN4hCiFUIYQh0CIaIlwiqCLsIzQjdCPAJAQkNiSKJLolBCUgJWIliiXWJfImMiZeJqwmzCcQJ2AnvigWKHooxikgKXYp2in8KjAqVCqEKrgq7CsAKxQrMitQK3ArliuoK7Qr4CwKLDwsYCyQLLgs0CzoLP4tHi08LVwtei2QLaItvC3WLfwuLi5cLpYuvi7yLywvWC+ML9QwFDBmMLAxAjFMMYgx3DIeMkQykDK+MwIzKjNsM6oz7jQuNIQ01jUYNVg1fDWwNcw2ADYaNkY2ljbmNxY3RjeCN7w3/Dg8OHY4sDjsOSg5Ujl8OZ45yDnqOgo6KDpE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AQIAAwAEAAUABgAHAAgACQAKAAsADAANAA4ADwAQABEAEgATABQAFQAWABcAGAAZABoAGwAcAB0AHgAfACAAIQAiACMAJAAlACYAJwAoACkAKgArACwALQAuAC8AMAAxADIAMwA0ADUANgA3ADgAOQA6ADsAPAA9AD4APwBAAEEAQgBDAEQARQBGAEcASABJAEoASwBMAE0ATgBPAFAAUQBSAFMAVABVAFYAVwBYAFkAWgBbAFwAXQBeAF8AYABhAQMAowCEAIUAlgDoAIYAjgCLAJ0AqQCkAIoA2gCDAJMA8gDzAI0AlwCIAMMA3gDxAJ4AqgD1APQA9gCiAK0AyQDHAK4AYgBjAJAAZADLAGUAyADKAM8AzADNAM4A6QBmANMA0ADRAK8AZwDwAJEA1gDUANUAaADrAO0AiQBqAGkAawBtAGwAbgCgAG8AcQBwAHIAcwB1AHQAdgB3AOoAeAB6AHkAewB9AHwAuAChAH8AfgCAAIEA7ADuALoBBAEFAQYBBwEIAQkA/QD+AQoBCwEMAQ0A/wEAAQ4BDwEQAQEBEQESARMBFAEVARYBFwEYARkBGgEbARwA+AD5AR0BHgEfASABIQEiASMBJAElASYBJwEoASkBKgErASwA+gDXAS0BLgEvATABMQEyATMBNAE1ATYBNwE4ATkBOgE7AOIA4wE8AT0BPgE/AUABQQFCAUMBRAFFAUYBRwFIAUkBSgCwALEBSwFMAU0BTgFPAVABUQFSAVMBVAD7APwA5ADlAVUBVgFXAVgBWQFaAVsBXAFdAV4BXwFgAWEBYgFjAWQBZQFmAWcBaAFpAWoAuwFrAWwBbQFuAOYA5wFvAKYBcADYAOEA2wDcAN0A4ADZAN8AmwFxAXIBcwF0AXUBdgF3AXgBeQF6AXsBfAF9AX4BfwGAAYEBggGDAYQBhQGGAYcBiAGJAYoBiwGMAY0BjgGPAZABkQGSAZMBlAGVAZYBlwGYAZkBmgGbAZwBnQGeAZ8BoAGhAaIBowGkAaUBpgGnAagBqQGqAasBrAGtAa4BrwGwAbEBsgGzAbQBtQG2AbcBuAG5AboBuwG8Ab0BvgG/AcABwQHCAcMBxAHFAcYBxwHIAckBygHLAcwBzQHOAc8B0AHRAdIB0wHUAdUB1gHXAdgB2QHaAdsB3AHdAd4B3wHgAeEB4gHjAeQB5QHmAecB6AHpAeoB6wHsAe0B7gHvAfAB8QHyAfMB9AH1AfYB9wH4AfkB+gH7AfwB/QH+Af8CAAIBAgICAwIEAgUCBgIHAggCCQIKAgsCDAINAg4CDwIQAhECEgITAhQCFQIWAhcCGAIZAhoCGwIcAh0CHgIfAiACIQIiAiMCJAIlAiYCJwIoAikCKgIrAiwCLQIuAi8CMAIxAjICMwI0AjUCNgI3AjgCOQI6AjsCPAI9Aj4CPwJAAkECQgJDAkQCRQJGAkcCSAJJAkoCSwJMAk0CTgJPAlACUQJSAlMCVAJVAlYCVwJYAlkCWgJbAlwCXQJeAl8CYAJhAmICYwJkAmUCZgJnAmgCaQJqAmsCbAJtAm4CbwJwALIAswC2ALcAxAC0ALUAxQCCAMIAhwCrAMYAvgC/ALwCcQJyAIwAnwCYAKgAmgCZAO8ApQCSAJwApwCPAJQAlQC5ANIAwADBAnMCdAJ1AnYCdwJ4AkNSB3VuaTAwQTAHQW1hY3JvbgdhbWFjcm9uBkFicmV2ZQZhYnJldmUHQW9nb25lawdhb2dvbmVrC0NjaXJjdW1mbGV4C2NjaXJjdW1mbGV4CkNkb3RhY2NlbnQKY2RvdGFjY2VudAZEY2Fyb24GZGNhcm9uBkRjcm9hdAdFbWFjcm9uB2VtYWNyb24GRWJyZXZlBmVicmV2ZQpFZG90YWNjZW50CmVkb3RhY2NlbnQHRW9nb25lawdlb2dvbmVrBkVjYXJvbgZlY2Fyb24LR2NpcmN1bWZsZXgLZ2NpcmN1bWZsZXgKR2RvdGFjY2VudApnZG90YWNjZW50DEdjb21tYWFjY2VudAxnY29tbWFhY2NlbnQLSGNpcmN1bWZsZXgLaGNpcmN1bWZsZXgESGJhcgRoYmFyBkl0aWxkZQZpdGlsZGUHdW5pMDEyQQdpbWFjcm9uBklicmV2ZQd1bmkwMTJEB0lvZ29uZWsHaW9nb25lawJJSgJpagtKY2lyY3VtZmxleAtqY2lyY3VtZmxleAxLY29tbWFhY2NlbnQMa2NvbW1hYWNjZW50DGtncmVlbmxhbmRpYwZMYWN1dGUGbGFjdXRlDExjb21tYWFjY2VudAxsY29tbWFhY2NlbnQGTGNhcm9uBmxjYXJvbgRMZG90BGxkb3QGTmFjdXRlBm5hY3V0ZQxOY29tbWFhY2NlbnQMbmNvbW1hYWNjZW50Bk5jYXJvbgZuY2Fyb24LbmFwb3N0cm9waGUDRW5nA2VuZwdPbWFjcm9uB29tYWNyb24GT2JyZXZlBm9icmV2ZQ1PaHVuZ2FydW1sYXV0DW9odW5nYXJ1bWxhdXQGUmFjdXRlBnJhY3V0ZQxSY29tbWFhY2NlbnQMcmNvbW1hYWNjZW50BlJjYXJvbgZyY2Fyb24GU2FjdXRlBnNhY3V0ZQtTY2lyY3VtZmxleAtzY2lyY3VtZmxleAxUY29tbWFhY2NlbnQMdGNvbW1hYWNjZW50BlRjYXJvbgZ0Y2Fyb24EVGJhcgR0YmFyBlV0aWxkZQZ1dGlsZGUHVW1hY3Jvbgd1bWFjcm9uBlVicmV2ZQZ1YnJldmUFVXJpbmcFdXJpbmcNVWh1bmdhcnVtbGF1dA11aHVuZ2FydW1sYXV0B1VvZ29uZWsHdW9nb25lawtXY2lyY3VtZmxleAt3Y2lyY3VtZmxleAtZY2lyY3VtZmxleAt5Y2lyY3VtZmxleAZaYWN1dGUGemFjdXRlClpkb3RhY2NlbnQKemRvdGFjY2VudAVsb25ncwhkb3RsZXNzagd1bmkwNDAwCWFmaWkxMDAyMwlhZmlpMTAwNTEJYWZpaTEwMDUyCWFmaWkxMDA1MwlhZmlpMTAwNTQJYWZpaTEwMDU1CWFmaWkxMDA1NglhZmlpMTAwNTcJYWZpaTEwMDU4CWFmaWkxMDA1OQlhZmlpMTAwNjAJYWZpaTEwMDYxB3VuaTA0MEQJYWZpaTEwMDYyCWFmaWkxMDE0NQlhZmlpMTAwMTcJYWZpaTEwMDE4CWFmaWkxMDAxOQlhZmlpMTAwMjAJYWZpaTEwMDIxCWFmaWkxMDAyMglhZmlpMTAwMjQJYWZpaTEwMDI1CWFmaWkxMDAyNglhZmlpMTAwMjcJYWZpaTEwMDI4CWFmaWkxMDAyOQlhZmlpMTAwMzAJYWZpaTEwMDMxCWFmaWkxMDAzMglhZmlpMTAwMzMJYWZpaTEwMDM0CWFmaWkxMDAzNQlhZmlpMTAwMzYJYWZpaTEwMDM3CWFmaWkxMDAzOAlhZmlpMTAwMzkJYWZpaTEwMDQwCWFmaWkxMDA0MQlhZmlpMTAwNDIJYWZpaTEwMDQzCWFmaWkxMDA0NAlhZmlpMTAwNDUJYWZpaTEwMDQ2CWFmaWkxMDA0NwlhZmlpMTAwNDgJYWZpaTEwMDQ5CWFmaWkxMDA2NQlhZmlpMTAwNjYJYWZpaTEwMDY3CWFmaWkxMDA2OAlhZmlpMTAwNjkJYWZpaTEwMDcwCWFmaWkxMDA3MglhZmlpMTAwNzMJYWZpaTEwMDc0CWFmaWkxMDA3NQlhZmlpMTAwNzYJYWZpaTEwMDc3CWFmaWkxMDA3OAlhZmlpMTAwNzkJYWZpaTEwMDgwCWFmaWkxMDA4MQlhZmlpMTAwODIJYWZpaTEwMDgzCWFmaWkxMDA4NAlhZmlpMTAwODUJYWZpaTEwMDg2CWFmaWkxMDA4NwlhZmlpMTAwODgJYWZpaTEwMDg5CWFmaWkxMDA5MAlhZmlpMTAwOTEJYWZpaTEwMDkyCWFmaWkxMDA5MwlhZmlpMTAwOTQJYWZpaTEwMDk1CWFmaWkxMDA5NglhZmlpMTAwOTcHdW5pMDQ1MAlhZmlpMTAwNzEJYWZpaTEwMDk5CWFmaWkxMDEwMAlhZmlpMTAxMDEJYWZpaTEwMTAyCWFmaWkxMDEwMwlhZmlpMTAxMDQJYWZpaTEwMTA1CWFmaWkxMDEwNglhZmlpMTAxMDcJYWZpaTEwMTA4CWFmaWkxMDEwOQd1bmkwNDVECWFmaWkxMDExMAlhZmlpMTAxOTMHdW5pMDQ2MAd1bmkwNDYxCWFmaWkxMDE0NglhZmlpMTAxOTQHdW5pMDQ2NAd1bmkwNDY1B3VuaTA0NjYHdW5pMDQ2Nwd1bmkwNDY4B3VuaTA0NjkEXzEwNgd1bmkwNDZCB3VuaTA0NkMHdW5pMDQ2RAd1bmkwNDZFB3VuaTA0NkYHdW5pMDQ3MAd1bmkwNDcxCWFmaWkxMDE0NwlhZmlpMTAxOTUJYWZpaTEwMTQ4CWFmaWkxMDE5Ngd1bmkwNDc2B3VuaTA0NzcHdW5pMDQ3OAd1bmkwNDc5B3VuaTA0N0EHdW5pMDQ3Qgd1bmkwNDdDB3VuaTA0N0QHdW5pMDQ3RQd1bmkwNDdGB3VuaTA0ODAHdW5pMDQ4MQd1bmkwNDgyB3VuaTA0ODMHdW5pMDQ4NAd1bmkwNDg1B3VuaTA0ODYHdW5pMDQ4Nwd1bmkwNDg4B3VuaTA0ODkHdW5pMDQ4QQd1bmkwNDhCB3VuaTA0OEMHdW5pMDQ4RAd1bmkwNDhFB3VuaTA0OEYJYWZpaTEwMDUwCWFmaWkxMDA5OAd1bmkwNDkyB3VuaTA0OTMHdW5pMDQ5NAd1bmkwNDk1B3VuaTA0OTYHdW5pMDQ5Nwd1bmkwNDk4B3VuaTA0OTkHdW5pMDQ5QQd1bmkwNDlCB3VuaTA0OUMHdW5pMDQ5RAd1bmkwNDlFB3VuaTA0OUYHdW5pMDRBMAd1bmkwNEExB3VuaTA0QTIHdW5pMDRBMwd1bmkwNEE0B3VuaTA0QTUHdW5pMDRBNgd1bmkwNEE3B3VuaTA0QTgHdW5pMDRBOQd1bmkwNEFBB3VuaTA0QUIHdW5pMDRBQwd1bmkwNEFEB3VuaTA0QUUHdW5pMDRBRgd1bmkwNEIwB3VuaTA0QjEHdW5pMDRCMgd1bmkwNEIzB3VuaTA0QjQHdW5pMDRCNQd1bmkwNEI2B3VuaTA0QjcHdW5pMDRCOAd1bmkwNEI5B3VuaTA0QkEHdW5pMDRCQgd1bmkwNEJDB3VuaTA0QkQHdW5pMDRCRQd1bmkwNEJGB3VuaTA0QzAHdW5pMDRDMQd1bmkwNEMyB3VuaTA0QzMHdW5pMDRDNAd1bmkwNEM1B3VuaTA0QzYHdW5pMDRDNwd1bmkwNEM4B3VuaTA0QzkHdW5pMDRDQQd1bmkwNENCB3VuaTA0Q0MHdW5pMDRDRAd1bmkwNENFB3VuaTA0Q0YHdW5pMDREMAd1bmkwNEQxB3VuaTA0RDIHdW5pMDREMwd1bmkwNEQ0B3VuaTA0RDUHdW5pMDRENgd1bmkwNEQ3B3VuaTA0RDgJYWZpaTEwODQ2B3VuaTA0REEHdW5pMDREQgd1bmkwNERDB3VuaTA0REQHdW5pMDRERQd1bmkwNERGB3VuaTA0RTAHdW5pMDRFMQd1bmkwNEUyB3VuaTA0RTMHdW5pMDRFNAd1bmkwNEU1B3VuaTA0RTYHdW5pMDRFNwd1bmkwNEU4B3VuaTA0RTkHdW5pMDRFQQd1bmkwNEVCB3VuaTA0RUMHdW5pMDRFRAd1bmkwNEVFB3VuaTA0RUYHdW5pMDRGMAd1bmkwNEYxB3VuaTA0RjIHdW5pMDRGMwd1bmkwNEY0B3VuaTA0RjUHdW5pMDRGNgd1bmkwNEY3B3VuaTA0RjgHdW5pMDRGOQd1bmkwNEZBB3VuaTA0RkIHdW5pMDRGQwd1bmkwNEZEB3VuaTA0RkUHdW5pMDRGRgRFdXJvB3VuaTIxMjANYnJldmVj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AEAAEABFAF8ALABkAGQARwBtAG0ASAB8AHwASQCAAJAASgCSAJYAWwCaAJ4AYACgAKYAZQCoALAAbACyALcAdQC6AL4AewDAAMAAgADyAPIAgQECAQIAggEUARQAgwEhASIAhAE5ATkAhgE+AT8AhwFMAU8AiQFVAVYAjQFYAVgAjwFaAVoAkAFcAWMAkQFmAWYAmQFqAWoAmgFsAXIAmwF1AXYAogF4AXoApAF8AYMApwGGAYYArwGKAYoAsAGMAZIAsQGVAZYAuAGYAZoAugGcAZ0AvQGfAZ8AvwGlAaYAwAGoAagAwgGqAaoAwwG+Ab8AxAHWAdcAxgHcAdwAyAHeAd8AyQHkAeUAywHnAfEAzQH2AfkA2AIAAgMA3AINAg4A4AIRAhIA4gIVAhcA5AIZAhoA5wIcAisA6QIyAjMA+QI1Aj8A+wJCAkMBBgJGAkcBCAJKAksBCgJOAk8BDAJRAlIBDgJZAloBEAABD34ABAAAAB4ARgBQAF4BGAE6AVwBZgLsA4YE0ABQBloGcAdqB6QIZgkUCe4KUABQCxoLQAzGDRANKg1EDU4NXA66DzAAAgBeABIBPwASAAMAXQAoAL4AKADAACgALgAn//AAK//wADP/8AA1//AAR//OAEj/xABJ/84AS//EAFH/zgBS/84AU//OAFX/xABW/84AV//YAFn/4gBd//YAXv/YAIj/8ACT//AAlP/wAJX/8ACW//AAl//wAKj/zgCp/84Aqv/OAKv/zgCs/84Asf/OALL/zgCz/84AtP/OALX/zgC2/84At//OALr/4gC7/+IAvP/iAL3/4gC+//YAwP/2APL/zgET//ABFP/OASL/2AE//9gACABKABQAXQAKAF4AFAC+AAoAwAAKAT8AFAJuABQCbwAUAAgASgAKAF0ACgBeAAoAvgAKAMAACgE/AAoCbgAKAm8ACgACAG3/zgJZ/84AYQAG//YAC//2ACX//AAm//kAJ//aACj/+QAp//kAKv/5ACv/2gAs//kALf/5AC7/8QAv//kAMP/5ADH/+QAy//kAM//aADT/+QA1/9oANv/5ADf/6wA4//gAOf/kADr/7AA7/+MAPf/iAEf/4gBI/90ASf/iAEr/5wBL/90ATf/2AFH/7ABS/+wAU//iAFX/3QBW/+wAV//3AFn/2ABd/9MAXgAHAG3/zgCB//wAgv/8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/9gAEv/YAEX/9ABG//wAR//7AEj/+wBJ//sASgAYAEv/+wBM//wAT//8AFD//ABR//sAUv/7AFP/+wBV//sAVv/7AFf//ABdABYAfAAOAKD//ACh//QAov/0AKP/9ACk//QApf/0AKb/9ACn//QAqP/7AKn/+wCq//sAq//7AKz/+wCx//sAsv/7ALP/+wC0//sAtf/7ALb/+wC3//sAvgAWAMAAFgDy//sBFP/7ASL//AJOABQCTwAUAlEAFAJSABQCWgAOAm4AGAJvABgAGAAGAAoACwAKABD/2AAS/9gARf/8AEoADABX//0AXQAPAHwACgCh//wAov/8AKP//ACk//wApf/8AKb//ACn//wAvgAPAMAADwEi//0CTgAIAlEACAJaAAoCbgAMAm8ADAAyAEX/8ABG//YAR//lAEj/6gBJ/+UAS//qAEz/9gBN//YAT//2AFD/9gBR//YAUv/2AFP/5QBV/+oAVv/2AFf/9QBZ/+oAbf/2AKD/9gCh//AAov/wAKP/8ACk//AApf/wAKb/8ACn//AAqP/lAKn/5QCq/+UAq//lAKz/5QCt//YArv/2AK//9gCw//YAsf/lALL/9gCz/+UAtP/lALX/5QC2/+UAt//lALr/6gC7/+oAvP/qAL3/6gDy//YBFP/lASL/9QJZ//YACQA4/9gAOv/iADv/7wA9/9IAXQAKAJ7/0gC+AAoAwAAKATn/0gBhABAAFAASABQAJv/nACf/vgAo/+cAKf/nACr/5wAr/74ALP/nAC3/5wAu/9gAL//nADD/5wAx/+cAMv/nADP/vgA0/+cANf++ADb/5wA3/8YAOP+cADn/zgA6/6YAO/+6AD3/kgA+//EARf/TAEb/7ABH/90ASP/TAEn/3QBK/90AS//TAEz/7ABN/+4AT//sAFD/7ABR/+wAUv/sAFP/3QBV/9MAVv/sAFf/2gBZ/9gAiP++AI3/5wCO/+cAj//nAJD/5wCS/+cAk/++AJT/vgCV/74Alv++AJf/vgCa/84Am//OAJz/zgCd/84Anv+SAKD/7ACh/9MAov/TAKP/0wCk/9MApf/TAKb/0wCn/9MAqP/dAKn/3QCq/90Aq//dAKz/3QCt/+4Arv/uAK//7gCw/+4Asf/dALL/7ACz/90AtP/dALX/3QC2/90At//dALr/2AC7/9gAvP/YAL3/2ADy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//oAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/9AAAAAAAAAAAAAAAAAAAAAAAAP/8/+z/7AAAAAD/2AAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/7AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/+AAAAAAAAAAAAAAAAAAAAAAAAP/6/+z/7AAAAAD/2AAE//YAAAAAAAAAAAAAAAAAAAAAAAD/7AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFAAAAAAAAAAAAAAAAAAAAAAAA//YAAAAAAAD/2AAEAAAAAAAAAAQAAAAAAAAAAAAAAAn/9gAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD//AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA//wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/+wAAAAAAAAAAAAAAAAAAAAAAAP/7//b/9gAAAAD/4gAFAAAAAAAAAAAAAAAFAAAAAAAAAAb/9gAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAHgAUAAAAAAAAAB4AHgAeAAAAAAAYACgAKP/2ACAAKP/5/84AAAAA//QAEv/5AAAADgAAAAAALQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAoAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/7AAAAAAAAAAA//gAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/8wAAAAAAAAAAAAAAAAAAAAAAAAAA/+z/7AAAAAD/zgAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/5AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/+AAAAAAAAAAAAAAAAAAAAAAAAP/4AAAAAAAAAAD/4gAF//YAAAAAAAAAAAAFAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/+wALAAAAAAAA//sAAAAAAAAAAP/5AAAAAAAAAAr/7AAEAAAAAAAAAAX//AAFAAAAAP/8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAr/7AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADgAAAAAAAAAAAAAAAAAAAAAAAAASAAoAHgAAAAAAAP/2/8QAAAAA/+z/+//0//oAAP/7//kAFAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAARAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP/7AAoAAAAAAAAAAAAAAAQAAAAA//sAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/7AAA//b/uv/s/87/4v/EAAAAAAAA/7D/sP/YABT/kgAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/sAAA/7r/pgAU/+L/2P/OAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAASAAD/4gAA//b/+//2AAoADwAMAAAAAAAAAAAAAAAAABQABAAAAAAAAAAAAAoAAAAAAAoAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP/7AAD/xAAA//b/+//Y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) format('truetype');
												font-weight: normal;
												font-style: normal;
											}
											@@font-face {
												font-family: 'LouisItalic';
												src: url(data:application/font-ttf;charset=utf-8;base64,AAEAAAAPAIAAAwBwRkZUTW1CFMIAAMY8AAAAHEdERUYDEQRWAACizAAAACpHUE9TGpK/3wAAo1AAACLqR1NVQtri3jAAAKL4AAAAWE9TLzKrMVgbAAABeAAAAGBjbWFwpVkhZgAAB6wAAANeZ2FzcP//AAMAAKLEAAAACGdseWbLA5R+AAAN+AAAiARoZWFk93AtpwAAAPwAAAA2aGhlYQchA10AAAE0AAAAJGhtdHjvJkPBAAAB2AAABdRsb2NhO2ZdQgAACwwAAALsbWF4cAG+AFcAAAFYAAAAIG5hbWWXdYDYAACV/AAABaBwb3N08lQ8PgAAm5wAAAclAAEAAAABGZm2B+GEXw889QALA+gAAAAAypzoIAAAAADKnOgg/2v+7AOsA/0AAAAIAAIAAAAAAAAAAQAABAf/DAAAA7P/a/8oA6wAAQAAAAAAAAAAAAAAAAAAAXUAAQAAAXUAVAAHAAAAAAACAAAAAQABAAAAQAAAAAAAAAACAbgBqQAFAAACvAKKAAAAjAK8AooAAAHdADIA+gAAAAAAAAAAAAAAAIAAAK9AACBKAAAAAAAAAABwc3kAAAAAIPsCAu7/BgAABAcA9CAAAAMAAAAAAjAC7gAAACAAAgI6AAoAAAAAAU0AAADYAAAA+wAkAWQAagIbABQCHQAwAhsAHwJ4ACoAvABqAVQAKwFU/8kBswBhAhsAGwDh/+0BtwBHAOEAFQGg/+YCQAA2AXkATwIaABgCKAAeAkAAJgI5ADoCOgA2AgQAZgI6ACsCOwBCAPUAHQD1//cCHQBBAh0AKwIdAB0B5wBjA48APwJI/+4CVgAuAkoASAJhAC4CIwAuAh4ALgJhAEgCgAAuAQAALgIZADACaAAuAhsALgMAAC4CfwAuAl4ASAJSAC4CXgBIAlQALgIwACcCJgBqAmwATwJEAHIDYACAAjz/6gI6AGkCQAAPAXEAFQGgAIEBcP/QAdQARgG1/8oB6gC7AhQAKgIrACsCAgA2AisAOQIPADYBYgAyAisAFwI6ACsA8wArAPr/fQIFACsA9AArA0QAKwI6ACsCDwA2Aiv//wIrADkBmgArAeEAJQFyAEgCOABHAeoATQLlAFUB/P/sAfL/xwHqAAUBoQA1ATgALgGg/80CFgBMANgAAAD7AAUCBQA9Ahv/+wIbABsBOAAuAgQAHgHqALoDSwBVAVMAYAIJADcCOgAKAeIAdwHqALYBLwB8Ahv/7gI6AAoCOgAKAeoA8gI9//8CUQB0AOEATwHqAGICOgAKAVQAZwIJACACOgAKAjoACgI6AAoB6gAAAkj/7gJI/+4CSP/uAkj/7gJI/+4CSP/uA2H/3AJKAEgCIwAuAiMALgIjAC4CIwAuAQAALgEAAC4BAAArAQAALgKSADQCfwAuAl4ASAJeAEgCXgBIAl4ASAJeAEgCJQAYAl4AKQJsAE8CbABPAmwATwJsAE8COgBpAlIALgJRABACFAAqAhQAKgIUACoCFAAqAhQAKgIUACoDRQAqAgIANgIPADYCDwA2Ag8ANgIPADYA8wALAPMAKwDzABYA8wArAg8ANQI6ACsCDwA2Ag8ANgIPADYCDwA2Ag8ANgJAAC0CDwAvAjgARwI4AEcCOABHAjgARwHy/8cCLv//AfL/xwJI/+4CFAAqAkj/7gIUACoCSP/uAhQAKgJKAEgCAgA2AkoASAICADYCSgBIAgIANgJKAEgCAgA2AmEALgJ6ADkCkgA0AjIAOQIjAC4CDwA2AiMALgIPADYCIwAuAg8ANgIjAC4CDwA2AiMALgIPADYCYQBIAisAFwJhAEgCKwAXAmEASAI0ABcCYQBIAisAFwKAAC4COgArApAANgJFADYBAAAuAPMADAEAAC4A8wArAQAALgDzACsBAP/UAPP/ywECAC4A8wArAw8ALgHtACsCGQAwAPr/fQJoAC4CBQArAgUAKwIbAC4A9AArAhsALgD0/+8CGwAuAUQAKwIbAC4BaAArAkgAGQFbAB8CfwAuAjoAKwJ/AC4COgArAn8ALgI6ACsCtwBCAn8ALgI6ACsCaABIAhgANgJoAEgCGAA2Al4ASAIPADYDRABKA1AANgJUAC4BmgArAlQALgGa/+8CVAAuAZoAKwIwACcB4QAlAjAAJwHhACUCMAApAeEAJQIwACcB4QAlAjEAagF5AEgCMQBqAeAASAImAFoBfAAxAmwATwI4AEcCbABPAjgARwJsAE8COABHAmwATwI4AEcCbABPAjgARwJsAE8COABHA2AAgALlAFUCOgBpAfL/xwI6AGkCQAAPAeoABQJAAA8B6gAFAkAADwHqAAUBKwAyAjoACgD6/30B6gCKAeoAogHqALwB6gEWAeoA7AHqAFcB6gCPAeoAqwI6AAoCOgAK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AWwBbQDmAOcBbgCmAW8A2ADhANsA3ADdAOAA2QDfAKgAnwCbALIAswC2ALcAxAC0ALUAxQCCAMIAhwCrAMYAvgC/ALwBcAFxAIwAmACaAJkA7wClAJIAnACnAI8AlACVALkAwADBANIBcgFzAXQBdQF2AXcHdW5pMDBBMAdBbWFjcm9uB2FtYWNyb24GQWJyZXZlBmFicmV2ZQdBb2dvbmVrB2FvZ29uZWsLQ2NpcmN1bWZsZXgLY2NpcmN1bWZsZXgKQ2RvdGFjY2VudApjZG90YWNjZW50BkRjYXJvbgZkY2Fyb24GRGNyb2F0B0VtYWNyb24HZW1hY3JvbgZFYnJldmUGZWJyZXZlCkVkb3RhY2NlbnQKZWRvdGFjY2VudAdFb2dvbmVrB2VvZ29uZWsGRWNhcm9uBmVjYXJvbgtHY2lyY3VtZmxleAtnY2lyY3VtZmxleApHZG90YWNjZW50Cmdkb3RhY2NlbnQMR2NvbW1hYWNjZW50DGdjb21tYWFjY2VudAtIY2lyY3VtZmxleAtoY2lyY3VtZmxleARIYmFyBGhiYXIGSXRpbGRlBml0aWxkZQd1bmkwMTJBB2ltYWNyb24GSWJyZXZlB3VuaTAxMkQHSW9nb25lawdpb2dvbmVrAklKAmlqC0pjaXJjdW1mbGV4C2pjaXJjdW1mbGV4DEtjb21tYWFjY2VudAxrY29tbWFhY2NlbnQMa2dyZWVubGFuZGljBkxhY3V0ZQZsYWN1dGUMTGNvbW1hYWNjZW50DGxjb21tYWFjY2VudAZMY2Fyb24GbGNhcm9uBExkb3QEbGRvdAZOYWN1dGUGbmFjdXRlDE5jb21tYWFjY2VudAxuY29tbWFhY2NlbnQGTmNhcm9uBm5jYXJvbgtuYXBvc3Ryb3BoZQNFbmcDZW5nB09tYWNyb24Hb21hY3JvbgZPYnJldmUGb2JyZXZlDU9odW5nYXJ1bWxhdXQNb2h1bmdhcnVtbGF1dAZSYWN1dGUGcmFjdXRlDFJjb21tYWFjY2VudAxyY29tbWFhY2NlbnQGUmNhcm9uBnJjYXJvbgZTYWN1dGUGc2FjdXRlC1NjaXJjdW1mbGV4C3NjaXJjdW1mbGV4DFRjb21tYWFjY2VudAx0Y29tbWFhY2NlbnQGVGNhcm9uBnRjYXJvbgRUYmFyBHRiYXIGVXRpbGRl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ABEABwAi/9gAP//OAFP//ABX//cAWf/4AFr/+gBb//MABwAdAA8AHgAWAFMABgBXAAgAWQAMAFoACgBbAAsAAgBX//wAW//0AAUAEv/sAFP/+wBXABYAWQASAFoADgADAFcADABZAA4AWgAGAAMAP//iAFP/8gBX//wABwAS/+QAHQAKAB4ACgBT//wAVwAUAFkADgBaAAoABQAJ//gAHQAKAB4ACgA//+IAV//+AAEATQBkAAIAO//7AE0APAADAFcAFABZAA4AWgAKAAMAIv/YADv/7ABb//YABgAi/8QATQAeAFP//ABX/8YAWf/OAFr/0wAFAFP/+ABX/+kAWf/vAFr/9ABb/+wAAwA7AB4AV//9AFv/9gADADsAKABX//0AW//2AAQATQAFAFcAFABZABQAWwAIAAMAO//4AFr/+ABb//gAAgAcAAUABQAAAAoACwABAA8ADwADABEAEwAEABUAHAAHACIAIgAPACQAPwAQAEQAXgAsAGMAYwBHAGwAbABIAHsAewBJAH8AjwBKAJEAlQBbAJkAnQBgAJ8ApQBlAKcArwBsALEAtgB1ALkAvQB7AL8AvwCAAPEA8QCBAQEBAQCCARMBEwCDASABIQCEATgBOACGAT0BPgCHAU8BUACJAVIBUwCLAVoBWwCNAAENrgAEAAAAGgA+AEgAVgEQARABMgE8Ar4DWAUCBogGlgasB5oH1AiWCWgKNgqYBogLYguIDQoNcA2KDaQAAgBdABABPgAQAAMAXAAeAL0AHgC/AB4ALgAm//oAKv/6ADL/+gA0//oARv/OAEf/zgBI/84ASv/OAFD/zgBR/84AUv/OAFT/zgBV/84AVv/YAFj/4gBc//YAXf/YAIf/+gCS//oAk//6AJT/+gCV//oAlv/6AKf/zgCo/84Aqf/OAKr/zgCr/84AsP/OALH/zgCy/84As//OALT/zgC1/84Atv/OALn/4gC6/+IAu//iALz/4gC9//YAv//2APH/zgES//oBE//OASH/2AE+/9gACABJAAoAXAAKAF0ACgC9AAoAvwAKAT4ACgFsAAoBbQAKAAIAbP/sAVr/7ABgAAX/7AAK/+wAJP/7ACX/8wAm/9cAJ//zACj/8wAp//MAKv/XACv/8wAt/+MALv/zAC//8wAw//MAMf/zADL/1wAz//MANP/XADX/8wA2/+IAN//zADj/4gA5/+wAOv/jADz/2ABG/9oAR//aAEj/2gBJ/+UASv/aAEz/7ABQ/+wAUf/sAFL/2gBU/9oAVf/sAFb/6gBY/9IAXP/TAF0ABABs/84AgP/7AIH/+wCC//sAg//7AIT/+wCF//sAh//XAIz/8wCN//MAjv/zAI//8wCR//MAkv/XAJP/1wCU/9cAlf/XAJb/1wCZ/+IAmv/iAJv/4gCc/+IAnf/YAKf/2gCo/9oAqf/aAKr/2gCr/9oArf/sALD/2gCx/+wAsv/aALP/2gC0/9oAtf/aALb/2gC5/9IAuv/SALv/0gC8/9IAvf/TAL//0wDx/+wBEv/XARP/2gEg/+IBIf/qATj/2AE+AAQBT//iAVD/7gFS/+IBU//uAVr/zgFs/+UBbf/lACYAOAAFAEYABgBHAAYASAAGAEoABgBMAAUAUAAFAFEABQBSAAYAVAAGAFUABQBcAB8AXQASAJkABQCaAAUAmwAFAJwABQCnAAYAqAAGAKkABgCqAAYAqwAGAKwABQCtAAUArgAFAK8ABQCwAAYAsQAFALIABgCzAAYAtAAGALUABgC2AAYAvQAfAL8AHwDxAAUBEwAGAT4AEgBqAAUAFAAKABQAD/+6ABH/ugAk/74AJf/8ACb/4gAn//wAKP/8ACn//AAq/+IAK//8AC3/jgAu//wAL//8ADD//AAx//wAMv/iADP//AA0/+IANf/8ADb/7AA3AAwAOQAKADoABQA8AA4APf/7AET/kgBF//wARv+SAEf/kgBI/5IASf/kAEr/kgBL//wATP/4AE7//ABP//wAUP+hAFH/oQBS/5IAVP+SAFX/oQBW/5wAWP+hAFz/sABd/7oAbP/EAHv/4gCA/74Agf++AIL/vgCD/74AhP++AIX/vgCH/+IAjP/8AI3//ACO//wAj//8AJH//ACS/+IAk//iAJT/4gCV/+IAlv/iAJ0ADgCg/5IAof+SAKL/kgCj/5IApP+SAKX/kgCm/5IAp/+SAKj/kgCp/5IAqv+SAKv/kgCw/5IAsf+hALL/kgCz/5IAtP+SALX/kgC2/5IAuf+hALr/oQC7/6EAvP+hAL3/sAC//7AA8f+hARL/4gET/5IBIP/sATgADgE9//sBUAAUAVMAFAFa/8QBW//iAV4ADwFfAA8BbP/kAW3/5ABhAAX/+gAK//oAJP/8ACX/+gAm/9YAJ//6ACj/+gAp//oAKv/WACv/+gAt/+cALv/6AC//+gAw//oAMf/6ADL/1gAz//oANP/WADX/+gA3//oAOP/yADz/9ABE/+4ARf/6AEb/6gBH/+kASP/qAEr/6QBL//oATv/6AE//+gBQ//IAUf/yAFL/6gBU/+kAVf/yAFb/7wBY/94AXP/pAF0ADwBs/+IAgP/8AIH//ACC//wAg//8AIT//ACF//wAh//WAIz/+gCN//oAjv/6AI//+gCR//oAkv/WAJP/1gCU/9YAlf/WAJb/1gCZ//IAmv/yAJv/8gCc//IAnf/0AKD/7gCh/+4Aov/uAKP/7gCk/+4Apf/uAKb/7gCn/+oAqP/qAKn/6gCq/+oAq//qALD/6gCx//IAsv/qALP/6gC0/+oAtf/qALb/6gC5/94Auv/eALv/3gC8/94Avf/pAL//6QDx//IBEv/WARP/6gEh/+8BOP/0AT4ADwFP//gBUv/4AVr/4gADAFwAKAC9ACgAvwAoAAUASf/sAFb/6AEh/+gBbP/sAW3/7AA7AAUABQAKAAUARP/wAEX/+wBG/+YAR//mAEj/5gBK/+YAS//7AEz//ABO//sAT//7AFD/9wBR//cAUv/mAFT/5gBV//cAVv/0AFj/5wBc//oAXQAJAGz/4gCf//sAoP/wAKH/8ACi//AAo//wAKT/8ACl//AApv/wAKf/5gCo/+YAqf/mAKr/5gCr/+YArP/8AK3//ACu//wAr//8ALD/5gCx//cAsv/mALP/5gC0/+YAtf/mALb/5gC5/+cAuv/nALv/5wC8/+cAvf/6AL//+gDx//cBE//mASH/9AE+AAkBTwAFAVIABQFa/+IADgAF//YACv/2AEn/9gBc//QAvf/0AL//9AFP//YBUP/2AVL/9gFT//YBXv/iAV//4gFs//YBbf/2ADAABQAUAAoAFAAP/8QAEf/EAET/7gBG//QAR//1AEj/9ABJABQASv/1AFL/9ABU//UAVv/8AFwAEgBdAAwAbP/2AHsAFACg/+4Aof/uAKL/7gCj/+4ApP/uAKX/7gCm/+4Ap//0AKj/9ACp//QAqv/0AKv/9ACw//QAsv/0ALP/9AC0//QAtf/0ALb/9AC9ABIAvwASARP/9AEh//wBPgAMAU8AHgFQABQBUgAeAVMAFAFa//YBWwAUAWwAFAFtABQANAAFAAoACgAKAA8ACAARAAgARP/8AEUACABG//YAR//4AEj/9gBJAAwASv/4AEsACABOAAgATwAIAFAACABRAAgAUv/2AFT/+ABVAAgAXAAOAF0AHAB7AB4AnwAIAKD//ACh//wAov/8AKP//ACk//wApf/8AKb//ACn//YAqP/2AKn/9gCq//YAq//2ALD/9gCxAAgAsv/2ALP/9gC0//YAtf/2ALb/9gC9AA4AvwAOAPEACAET//YBPgAcAVAAFAFTABQBWwAeAWwADAFtAAwAMwAFABQACgAUAA//2AAR/9gARP/5AEX/+wBGAAEASAABAEkAFABL//sATv/7AE//+wBQ//sAUf/7AFIAAQBV//sAVgAEAFwAEgB7AA4An//7AKD/+QCh//kAov/5AKP/+QCk//kApf/5AKb/+QCnAAEAqAABAKkAAQCqAAEAqwABALAAAQCx//sAsgABALMAAQC0AAEAtQABALYAAQC9ABIAvwASAPH/+wETAAEBIQAEAU8AFAFQABQBUgAUAVMAFAFbAA4BbAAUAW0AFAAYAAUACgAKAAoAD//dABH/3QBE//wASQARAFYAAQBcAA8AewAKAKD//ACh//wAov/8AKP//ACk//wApf/8AKb//AC9AA8AvwAPASEAAQFPAAgBUgAIAVsACgFsABEBbQARADIARP/tAEX/9gBG/+EAR//mAEj/4QBK/+YAS//2AEz/9gBO//YAT//2AFD/9gBR//YAUv/hAFT/5gBV//YAVv/xAFj/6ABs//YAn//2AKD/7QCh/+0Aov/tAKP/7QCk/+0Apf/tAKb/7QCn/+EAqP/hAKn/4QCq/+EAq//hAKz/9gCt//YArv/2AK//9gCw/+EAsf/2ALL/4QCz/+EAtP/hALX/4QC2/+EAuf/oALr/6AC7/+gAvP/oAPH/9gET/+EBIf/xAVr/9gAJADf/2AA5/+IAOv/vADz/0gBcAAoAnf/SAL0ACgC/AAoBOP/SAGAADwAUABEAFAAl/+cAJv/IACf/5wAo/+cAKf/nACr/yAAr/+cALf/YAC7/5wAv/+cAMP/nADH/5wAy/8gAM//nADT/yAA1/+cANv/QADf/pgA4/84AOf+wADr/xAA8/5IAPf/xAET/0wBF/+wARv/dAEf/0wBI/90ASf/dAEr/0wBL/+wATP/uAE7/7ABP/+wAUP/s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//T/8gAIAAAACgAGAAQAAP/YAAAAAAAAAAAAAAAA/8T/sP/J/6b/7v/JAAD/4v/YAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/nAAAAAAAAAAAAAAAAAAAAAAAAAACAEwABQAFACMACgAKACMADwAPACAAEQARACAAJQAlAAEAJgAmAAMAJwAnAAkAKAAoAAQAKQApAAUAKgAqAAYAKwAsAAIALQAtAAcALwAvAAgAMAAxAAIAMgAyAAkAMwAzAAoANQA1AAsANgA2AAwAOAA4AA0AOQA5AA4AOgA6AA8APAA8ABAAPQA9ABEARABEABIARQBFABMARgBGABQARwBHABUASABIABYASQBJABcASgBKABkASwBLABoATABMABgATQBNABkATwBPABUAUQBRABoAUgBSABsAUwBTABMAVABUABkAVgBWABwAWABYAB0AXABcAB4AXQBdAB8AbABsACEAewB7ACIAhgCGAAQAhwCHAAMAiACLAAQAjACPAAIAkQCRAAIAkgCVAAkAmQCcAA0AnQCdABAAoAClABIApwCnABQAqACrABYArACvABgAsQCxABoAsgC2ABsAuQC8AB0AvQC9AB4AvwC/AB4A8QDxABgBAQEBAAgBEwETABYBIAEgAAwBIQEhABwBOAE4ABABPQE9ABEBPgE+AB8BTwFPACQBUAFQACUBUgFSACQBUwFTACUBWgFaACEBWwFbACIBXgFfACYAAgBMAAUABQAOAAoACgAOAA8ADwAUABEAEQAUACQAJAAMACUAJQAdACYAJgAEACcAKQAdACoAKgAEACsAKwAdAC0ALQAWAC4AMQAdADIAMgAEADMAMwAdADQANAAEADUANQAdADYANgAFADcANwAGADgAOAAHADkAOQAIADoAOgAJADwAPAAKAD0APQALAEQARAAVAEUARQABAEYARgATAEcARwAYAEgASAATAEkASQANAEoASgAYAEsASwABAEwATAAZAE4ATwABAFAAUQAaAFIAUgATAFQAVAAYAFUAVQAaAFYAVgAbAFgAWAAXAFwAXAACAF0AXQADAGwAbAAQAHsAewARAIAAhQAMAIcAhwAEAIwAjwAdAJEAkQAdAJIAlgAEAJkAnAAHAJ0AnQAKAJ8AnwABAKAApgAVAKcAqwATAKwArwAZALAAsAATALEAsQAaALIAtgATALkAvAAXAL0AvQACAL8AvwACAPEA8QAaARIBEgAEARMBEwATASABIAAFASEBIQAbATgBOAAKAT0BPQALAT4BPgADAU8BTwAPAVABUAAcAVIBUgAPAVMBUwAcAVoBWgAQAVsBWwARAV4BXwASAWwBbQANAAIAIwAFAAUAAAAKAAoAAQAPAA8AAgARABEAAwAkAC0ABAAvADMADgA1ADYAEwA4ADoAFQA8AD0AGABEAE0AGgBPAE8AJABRAFQAJQBWAFYAKQBYAFgAKgBcAF0AKwBsAGwALQB7AHsALgCAAI8ALwCRAJUAPwCZAJ0ARACgAKUASQCnAK8ATwCxALYAWAC5AL0AXgC/AL8AYwDxAPEAZAEBAQEAZQETARMAZgEgASEAZwE4ATgAaQE9AT4AagFPAVAAbAFSAVMAbgFaAVsAcAFeAV8AcgAAAAAAAQAAAADYCESBAAAAAMqc6CAAAAAAypzoIA==) format('truetype');
												font-weight: normal;
												font-style: normal;
											}
											.txt1 {
												font-family: mateo;
											}
											body {
												width:800px;
												margin: 0;
												padding: 0;
												font-family: Arial, Helvetica, sans-serif;
												font-size: 18pt;
											}
				                        </style>
				                    </head>
				                    <body>
										<table cellpadding='0' border='0' cellspacing='0'>
											<tr>
												<td><img alt='' src='" + imglayout1 +
                    @"' style='width:100%; height: 146px; border-width: 0px;'></td>	   
											</tr>
										    <tr>
										    <!--mensaje de entrada -->
										    	    <td style='width: 1500px; height: 136px;'>
											         <div style='width: 100%; height: 136px; border-width: 0px; text-align:center'>
											   	 	 <label style='font-family:LouisRegular; font-size:33pt; color:#4C5054; text-align:center'>H o l a </label><br/>
											 	  	 <label style='font-family:LouisBold; font-size:33pt; color:#4C5054; text-align:center'>" +
                    obj.nombreCompletoTercero.ToUpper() + @",</label>
											 	    </div>
											     </td>							
											 	<tr>							
											 		<td colspan='3'>							 
											 			<table style='width:100%'>							  
											 				<tr>							  
											 					<td style='width: 18%; min-height:100px'>
																	   <div style='width: 100%; height: 200px; border-width: 0px; background-color: white;'></ div >								
											 					</td>
											 						
											 					<td style='width: 64%; min-height:100px'>								 
											 							<div style='width:100%; height:100%; align-content:center'>								  
											 								<p style='font-family:LouisRegular; font-size:16pt; color:#4C5054; text-align:center'>" +
                    obj.texto1 + @"</p>								   
											 								<p style='font-family:LouisBoldItalic; font-size:16pt; color:#4C5054; text-align:center'> DESCUBRE A CONTINUACIÓN LA NUEVA GENERACIÓN CHEVROLET</p>
											 							</div>
											 					</td>
											 					<td style='width: 18%; min-height:100px'>
											 						<div style='width: 100%; height: 200px; border-width: 0px; background-color: white;'></div>
											 					</td>
											 				</tr>
											 			</table>
											 		</td>
											 	</tr>
										    </tr>
											<tr>
												<td><a href='#'><img alt='' src='" + imglayout2 +
                    @"' style='width: 100%; height: 16px; border-width: 0px;'></a></td>
											</tr>
											<tr>
											<!--imagen principal del carro-->
												<td style='width: 1500px; height: 1014px;'>
													<img src=" + obj.imgPrincipal +
                    @" style='width: 100%; height: 100%; border-width: 0px;'>
												</td>
											</tr>
											<tr>
												<td><img alt='' src='" + imglayout3 +
                    @"' style='width: 100%; height: 14px; border-width: 0px;'></td>
											</tr>
											<tr>
												<td colspan='3'>
													<table width='100%'>
														<td style='width: 18%; min-height:100px'>
															<div style='width: 100%; height: 200px; border-width: 0px; background-color: white;'></div>
														</td>
														<td style='width: 64%; min-height:100px'>
														<!--pie de foto-->
															<div style='width: 100%; min-height:100px; border-width: 0px; text-align:center'>
																<label style='font-family:LouisRegular; font-size:15pt; color:#4C5054; text-align:center'>" +
                    obj.pieFoto + @"</label>
															</div>
														</td>
														<td style='width: 18%; min-height:100px;'>
															<div style='width: 100%; height: 200px; border-width: 0px; background-color: white;'></div>
														</td>
													</table>
												</td>
											</tr>
											<tr>
												<td><div style='width: 100%; height: 50px; border-width: 0px;'></td>
											</tr>
											<tr>
												<table cellpadding='0' cellspacing='0'>
													<tr>
														<td colspan='3'>
															<table width='100%' cellpadding='0' cellspacing='0'>
																<tr>
																	<td>
																		<div style='width: 140px; height: 328px; border-width: 0px;'></div>
																	</td>
																	<td>
																		<img alt='' src=" + obj.imgDetalle1 +
                    @" style='width: 326px; height: 328px; border-width: 0px;'>
										 							</td>
										 							<td>
										 								<div style='width: 121px; height: 328px; border-width: 0px;'></div>
										  							</td>
										         					<td>
																		<img alt='' src=" + obj.imgDetalle2 +
                    @" style='width: 326px; height: 328px; border-width: 0px;'>
																	</ td >
																	<td>
																		<div style='width: 122px; height: 328px; border-width: 0px;'></div>
										  							</td>
																	<td>
																		<img alt='' src=" + obj.imgDetalle3 +
                    @" style='width: 326px; height: 328px; border-width: 0px;'>
																	</td>
																	<td>
																		<div style='width: 130px; height: 328px; border-width: 0px;'></div>
										  							</td>
										   						</tr>
										  					</table>	  
														</td>	  
													</tr>
													<tr>
										  				<td>
										  					<div style='width: 100%; height: 20px; border-width: 0px;'></div>	   
														</td>	   
													</tr>
										   		    <tr>
														<td colspan='3'>
															<table width='100%'>
																<td style='width: 5%; height: 121px;'>
											  						<div style='width: 100%; height: 200px; border-width: 0px; background-color: white;'></div>
											   				    </td>
																<td style='width: 27%; height: 121px;'>
																<!--titulo detalle 1-->
																	<div style='width: 100%; height: 200px; border-width: 0px; text-align:center'>"
                    + tituloDet1 +
                    @"</div>
																</td>
																<td>
																	<div style='width: 80px; height: 200px; border-width: 0px;'></div> 
																</td>
 															    <td style='width: 27%; height: 121px;'>
 																<!--titulo detalle 2-->
  																	<div style='width: 100%; height: 200px; border-width: 0px; text-align:center'>"
                    + tituloDet2 +
                    @"</div>
																</td>
																<td>
																	<div style='width: 80px; height: 200px; border-width: 0px;'></div>
																</td>
										                        <td style='width: 27%; height: 121px;'>
  																<!--titulo detalle 3-->
  																	<div style='width: 100%; height: 200px; border-width: 0px; text-align:center'>"
                    + tituloDet3 +
                    @"</div>
																</td>
																<td style='width: 5%; height: 121px;'> 
																	<div style='width: 100%; height: 200px; border-width: 0px; background-color: white;'></div>
																</td>
  															</table>
														</td>  
													</tr>
													<tr>
  														<td>
  															<div style='width: 100%; height: 30px; border-width: 0px;'></div>
														</td>
   												    </tr>
   												    <tr>
														<td colspan='3'>	
															<table width='100%'>
																<td style='width: 5%; min-height:100px'>
																	<div style='width: 100%; height: 350px; border-width: 0px; background-color: white;'></div>
																</td>
																<td style='width: 27%; min-height:100px'>
																<!--cuerpo detalle 1-->
																	<div style='width: 100%; min-height:100px; border-width: 0px; text-align:center'>
											 							<label style='font-family:LouisRegular; font-size:15pt; color:#4C5054; text-align:center'>" +
                    obj.cuerpoTitulo1 + @"</label>
																	</div>
																</td>
																<td>
																	<div style='width: 80px; min-height:100px; border-width: 0px;'></div>
																</td>
																<td style='width: 27%; min-height:100px'>
																<!--cuerpo detalle 1-->
																	<div style='width: 100%; min-height:100px; border-width: 0px; text-align:center'>
																		<label style='font-family:LouisRegular; font-size:15pt; color:#4C5054; text-align:center'>" +
                    obj.cuerpoTitulo2 + @"</label>
																	</div>
																</td>
																<td>
																	<div style='width: 80px; min-height:100px; border-width: 0px;'></div>
																</td>
																<td style='width: 27%; min-height:100px;'>
																<!--cuerpo detalle 1-->
																	<div style='width: 100%; min-height:100px; border-width: 0px; text-align:center'>
																		<label style='font-family:LouisRegular; font-size:15pt; color:#4C5054; text-align:center'>" +
                    obj.cuerpoTitulo3 + @"</label>
																	</div>
																</td>
																<td style='width: 5%; min-height:100px;'>
																	<div style='width: 100%; height: 350px; border-width: 0px; background-color: white;'></div>
																</td>
															</table>
														</td>
													</tr>
													<!--<tr>
														<td>
															<img alt='' src='sliceGeneral_4_0.png' style='width: 520px;  height: 439px; border-width: 0px;'>
														</td>
														<td>
															<a href='#'><img alt='' src='sliceGeneral_4_1.png' style = 'width: 451px; height: 439px; border-width: 0px;'></a>
														</td>
														<td>
															<img alt=' ' src='sliceGeneral_4_2.png' style='width: 521px;  height: 439px; border-width: 0px;'>
														</td>
													</tr>-->
												</table>
											</tr>
											<tr style='width: 100%; min-height:50px; border-width: 0px;'>
												<table cellpadding='0' cellspacing='0'>
													<td colspan='3'>
														<table style='width: 100%;  min-height:50px; border-width: 0px;'>
															<td>
																<div style='width: 100%;  height: 200px; border-width: 0px;'>
																	<img alt='' src=" + obj.chevyStarImg +
                    @" style='width: 100%; height: 100%; border-width: 0px;'>
																</div>
															</td>
														</table>
													</td>
												</table>
											</tr>
											<tr>
												<td>
													<div style='width: 100%; height: 30px; border-width: 0px;'></div>											  
												</td>											  
											</tr>
											<tr>
												<table cellpadding='0' cellspacing='0'>
													<td colspan='5'>
														<table width='100%' cellpadding='0' cellspacing='0'>
															<tr>
																<td style='width: 114px; height: 78px;'>
																	<div style='width: 100%; height: 100%; border-width: 0px; background-color: white;'></div>
																</td>
																<td style='width: 610px; height: 78px;'>
																<!--COTIZACION-->
																	<div style='width: 610px;  height: 100%; border-width: 0px; padding-top:5px; text-align:center; background-color: #EEB700;'>
																		<label style='font-family:LouisBold; font-size:23pt; color:#FFF;'> COTIZACIÓN </label>
																	</div>
																</td>
																<td>
																	<div style='width: 56px; height: 78px; border-width: 0px;'></div>
																</td>
																<td style='width: 610px; height: 78px;'>
																<!--PLAN DE FINANCIACION-->
																	<div style='width: 610px; height: 100%; border-width: 0px; padding-top:5px; text-align:center; background-color: #EEB700;'>
																		<label style='font-family:LouisBold; font-size:23pt; color:#FFF;'> PLAN DE FINANCIACIÓN</label>
																	</div>
																</td>
																<td style='width: 114px;  height: 78px;'>
																	<div style='width: 100%; height: 100%; border-width: 0px; background-color: white;'></div>
																</td>
															</tr>
														</table>
													</td>
												</table>
											</tr>
											<tr>
												<table cellpadding='0' cellspacing='0'>
													<td colspan='5'>
														<table style='width:100%' cellpadding='0' cellspacing='0'>
															<tr>
																<td style='width: 114px; height: 78px;'>
																	<div style='width: 100%; height: 100%; border-width: 0px; background-color: white;'></div>
																</td>
																<td style='width: 610px; height: 200px;'>
						<!--DATOS COTIZACION-->
																	<div style='width: 100%; height: 100%; border-width: 0px;'>
																	<br/>
					<label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>VEHICULO: </label><label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>" +
                    obj.vehiculo + @"</label><br />

					<label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>MODELO: </label><label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>" +
                     obj.anioModelo + @"</label><br />

					<label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>COLOR: </label><label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>" +                   
                    obj.color + @"</label><br />

					<label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>PRECIO DEL VEHICULO: </label><label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>" +                   
                    obj.precioFinanciacion.ToString("0,0", elGR) + @"</label><br />

				    <label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>MATRICULA: </label><label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>" +                   
                    obj.matricula.Value.ToString("0,0", elGR) + @"</label><br />

					<label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>SOAT: </label><label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>" +                    
                    obj.soat.Value.ToString("0,0", elGR) + @"</label><br />

	                <label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>ACCESORIOS: </label><label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>" +                   
                    obj.valorAccesorios.ToString("0,0", elGR) + @"</label><br />

					<label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>PRECIO DEL SEGURO: </label><label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>" +                    
                    obj.poliza.Value.ToString("0,0", elGR) + @"</label><br />

										                            </div>
										                        </td>
										                        <td>
										                            <div style='width: 22px; height: 200px; border-width: 0px;'></div>
																</td>
																<td>
																	<img src='" + imglayout4 +
                    @"' style='width: 8px;  height: 200px; border-width: 0px;'>
										                        </td>
										                        <td>
										                            <div style = 'width: 26px; height: 200px; border-width: 0px;'></div>
																</td>
																<td style='width: 610px;  height: 200px;'>
										                        <!-- DATOS PLAN DE FINANCIACION -->
																	<div style='width: 100%; height: 100%; border-width: 0px;'>
																	<br/>
					<label style='font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>PRECIO: </label><label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>" +
                    obj.precioFinanciacion.ToString("0,0", elGR) + @"</label><br />

					<label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>CUOTA INICIAL: </label><label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>" +
                    obj.cuotaInicial.ToString("0,0", elGR) + @"</label><br />
					
                    <label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>MONTO A FINANCIAR: </label><label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>" +
                    obj.credito.ToString("0,0", elGR) + @"</label><br />"
                    + planito
                    + planitas + @"
		
                    <label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>CUOTAS INICIAL: </label><label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>"+
                    obj.cuotaInicial.ToString("0,0", elGR) + @"</label><br />

                    <label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>CUOTA RESIDUAL: </label><label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>" +
                    obj.valorResidual.ToString("0,0", elGR) + @"</label><br />
										                                
                    <label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>OTROS: </label><label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>" +
                    obj.otrosValores.ToString("0,0", elGR) + @"</label><br /> 
									                                
                                                                </div>
										                        </td>
										                        <td style = 'width: 114px;  height: 78px;'>
																	<div style='width: 100%; height: 100%; border-width: 0px; background-color: white;'></div>
										                        </td>
										                    </tr>
                                                            <tr>
					<td style = 'width: 114px;  height: 78px;'>
						<div style='width: 100%; height: 100%; border-width: 0px; background-color: white;'></div>
					</td>
                    <td colspan='5'>
                    <label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>NOTAS DEL VEHICULO: </label><label style = 'font-family:LouisBold; font-size:15pt; color:#4C5054;' class='control-label col-md-4'>" +
                    obj.notasCotizacion + @"</label><br /><td>
                    <td></td>
                    <td></td>
                    <td></td>
                    <td></td>

					<td style = 'width: 114px;  height: 78px;'>
						<div style='width: 100%; height: 100%; border-width: 0px; background-color: white;'></div>
					</td>
										                </tr>
                                                        </table>
										            </td>
										        </table>
										    </tr>
										    <tr>
										        <td>
										            <div style = 'width: 100%;  height: 20px; border-width: 0px;'></ div>
												</td>
											</tr>
											<tr>
												<table cellpadding='0' cellspacing='0'>
										            <td colspan='3'>
														<table width='100%'>
										                    <td><a href='#'><img alt='' src='" + imglayout5 +
                    @"' style='width: 1500px; height: 28px; border-width: 0px;'></a></td>
										                </table>
										            </td>
										        </table>
										    </tr>
										    <tr>
										        <table cellpadding='0' cellspacing='0'>
										            <td colspan='3'>
														<table width='100%'>
										                    <td><a href='#'><img alt='' src='" + imglayout6 +
                    @"' style='width: 1500px; height: 74px; border-width: 0px;'></a></td>
										                </table>
										            </td>
										        </table>
										    </tr>
										    <tr>
										        <table cellpadding='0' cellspacing='0'>
										            <td colspan='3'>
														<table width='100%'>
										                    <tr>
										                        <td style='width: 182px;  height: 151px; border-width: 0px;' >
																	<div style='width: 100%; height: 100%; border-width: 0px; background-color: white;'></div>
										                        </td>
										                        <td style='width: 487px; height: 151px; border-width: 0px;'>
																	<!--Nombre asesor-->
										                            <div style = 'width: 100%; height: 100%; border-width: 0px;'>
																		<p style='font-family:LouisBold; font-size:33pt; color:#4C5054; text-align:center'>" +
                    obj.nombreAsesor + @"</p>
										                            </div>
										                        </td>
										                        <td>
										                            <img alt= '' src='" + imglayout7 +
                    @"' style='width: 8px; height: 151px; border-width: 0px;'>
										                        </td>
										                        <td style = 'width: 718px; height: 151px; border-width: 0px;' >
																	<!--Datos contacto asesor-->
										                            <div style = 'width: 100%; height: 100%; border-width: 0px;'>
																		<br />
																		<label style='font-family:LouisBold; font-size:16pt; color:#4C5054; text-align:left'>Celular: &nbsp; </label><label style='font-family:LouisRegular; font-size:16pt; color:#4C5054; text-align:left'>" +
                    obj.telefono_asesor + @"</label><br/>
 																		<label style='font-family:LouisBold; font-size:16pt; color:#4C5054; text-align:left'>Correo: &nbsp;  </label><label style='font-family:LouisRegular; font-size:16pt; color:#4C5054; text-align:left'>" +
                    obj.correo_asesor + @" </label><br/>
																		<label style='font-family:LouisBold; font-size:16pt; color:#4C5054; text-align:left'>Telefono: &nbsp;</label><label style='font-family:LouisRegular; font-size:16pt; color:#4C5054; text-align:left'> 313 5000</label><br/>
										                            </div>
										                        </td>
										                        <td style='width: 90px; height: 151px; border-width: 0px;'>
																	<div style='width: 100%; height: 100%; border-width: 0px; background-color: white;'></div>
										                        </td>
										                    </tr>
										                </table>
										            </td>
										        </table>
										    </tr>
										    <tr>
										        <table cellpadding='0' cellspacing='0'>
										            <td colspan='3'>
														<table style='width: 100%;'>
										                    <td><a href='#'><img alt='' src='" + imglayout8 +
                    @"' style='width: 1500px; height: 192px; border-width: 0px;'></a></td>
										                </table>
										            </td>
										        </table>
										    </tr>
										    <tr>
										        <table cellpadding='0' cellspacing='0'>
										            <td colspan='3'>
														<table style='width: 100%;'>
										                    <td><a href='https://www.caminos.com.co'><img alt='' src='" +
                    imglayout9 + @"' style='width: 1500px; height: 151px; border-width: 0px;'></a></td>
										                </table>
										            </td>
										        </table>
										    </tr>
										    <tr>
										        <table cellpadding='0' cellspacing='0'>
										            <td colspan='3'>
														<table style='width: 100%;'>
										                    <td><a href='#'><img alt='' src='" + imglayout10 +
                    @"' style='width: 1500px; height: 123px; border-width: 0px;'></a></td>
										                </table>
										            </td>
										        </table>
										    </tr>
										    <tr>
										        <table cellpadding='0' cellspacing='0'>
										            <td colspan='3'>
														<table style='width: 100%;'>
										                    <td><img alt='' src='" + imglayout11 +
                    @"' style='width: 1500px;  height: 31px; border-width: 0px;'></td>
										                </table>
										            </td>
										        </table>
										    </tr>
										</table>
                                        <tfoot>
                                            <tr>
                                                <td colspan='4' style= 'background-color:#D8D8D8'>
                                                   <h5 style='text - align:center; background - color:#D8D8D8; padding:0; margin:0 '>
                                                        Nota: Esta información debe ser verificada con el asesor del concesionario y está sujeta a cambios de precio sin previo aviso,
                                                        el valor de la cuota mensual y la tasa asignada está sujeta a cambios por la financiera. Los datos de contacto aquí suministrados por el cliente
                                                        podrán ser utilizados para enviar promociones por medio de correo eletrónico, por mensaje de texto o por llamada telefónica
                                                  </h5>
                                                </td>
                                            </tr>
                                        </tfoot>
									</body>
								</html>";

                mensaje.Bcc.Add("correospruebaexi2019@gmail.com");
                if (user_remitente != null)
                {
                    mensaje.ReplyToList.Add(new MailAddress(user_remitente.user_email,
                        user_remitente.user_nombre + " " + user_remitente.user_apellido));
                }

                mensaje.Subject = user_destinatario.prinom_tercero + " estás a un paso de estrenar tu chevrolet ";
                mensaje.BodyEncoding = Encoding.Default;
                mensaje.IsBodyHtml = true;

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(html2, null, "text/html");

                mensaje.AlternateViews.Add(htmlView);

                //SmtpClient cliente = new SmtpClient("smtp.mi.com.co");
                SmtpClient cliente = new SmtpClient(correoconfig.smtp_server, correoconfig.puerto)
                {
                    //cliente.Port = correoconfig.puerto;
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(correoconfig.usuario, correoconfig.password),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    EnableSsl = true
                };
                cliente.Send(mensaje);

                #region Seguimiento de la cotizacion

                //icb_cotizacion buscarSerialUltimaCot = context.icb_cotizacion.OrderByDescending(x => x.cot_idserial).Where(d => d.cot_idserial == idcotizacioncreada).FirstOrDefault();

                var BuscarSeguimiento = context.Seguimientos.Where(x => x.EsManual == false && x.ModuloSeguimientos.Codigo == 6 && x.Codigo == 5).FirstOrDefault();
                vcotseguimiento seguimiento = new vcotseguimiento
                {
                    cot_id = obj.idcotizacion, //buscarSerialUltimaCot.cot_idserial,
                    fecha = DateTime.Now,
                    responsable = Convert.ToInt32(Session["user_usuarioid"]),
                    Notas = "Envió Cotizacion ID: "+ Convert.ToString(obj.idcotizacion),
                    Motivo = null,
                    fec_creacion = DateTime.Now,
                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    estado = true,
                    tipo_seguimiento = BuscarSeguimiento.Id
                };
                context.vcotseguimiento.Add(seguimiento);
                int guardarGeneral = context.SaveChanges();
                #endregion

                var data = new
                {
                    tipo = "success",
                    mensaje = "Mensaje enviado exitosamente"
                };

                return 1;
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                var data = new
                {
                    tipo = "error",
                    mensaje = error
                };

                return 0;
            }
        }

        public ActionResult Ver(int id, int? menu)
        {
            bool buscarAnuladas = context.icb_cotizacion.Where(x => x.cot_idserial == id).Select(x => x.anulado)
                .FirstOrDefault();
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            //int permiso = (from u in context.users
            //               join r in context.rols
            //                   on u.rol_id equals r.rol_id
            //               join ra in context.rolacceso
            //                   on r.rol_id equals ra.idrol
            //               where u.user_id == usuario && ra.idpermiso == 23
            //               select new
            //               {
            //                   u.user_id,
            //                   u.rol_id,
            //                   r.rol_nombre,
            //                   ra.idpermiso
            //               }).Count();
            var buscartercero = (from cotizacion in context.icb_cotizacion
                                 join tercero in context.icb_terceros
                                     on cotizacion.id_tercero equals tercero.tercero_id
                                 where cotizacion.cot_idserial == id && cotizacion.bodegaid == bodegaActual
                                 select new
                                 {
                                     cotizacion.cot_numcotizacion,
                                     cotizacion.cot_idserial,
                                     tercero.tercero_id,
                                     tercero.doc_tercero,
                                     tercero.telf_tercero,
                                     tercero.celular_tercero,
                                     tercero.email_tercero,
                                     tercero.prinom_tercero,
                                     tercero.segnom_tercero,
                                     tercero.apellido_tercero,
                                     tercero.segapellido_tercero,
                                     tercero.razon_social,
                                     tercero.tpdoc_id,

                                     cliente = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                               tercero.apellido_tercero + " " + tercero.segapellido_tercero,
                                     cotizacion.cot_feccreacion,
                                     direccion = (from direccion in context.terceros_direcciones
                                                  join ciudad in context.nom_ciudad
                                                      on direccion.ciudad equals ciudad.ciu_id
                                                  where direccion.idtercero == tercero.tercero_id
                                                  select new
                                                  {
                                                      direccion.id,
                                                      direccion = direccion.direccion + " (" + ciudad.ciu_nombre + ")"
                                                  }).OrderByDescending(x => x.id).FirstOrDefault()
                                 }).FirstOrDefault();

            ViewBag.idCotizacion = id;
            ViewBag.idTercero = buscartercero.tercero_id;
            ViewBag.numeroCotizacion = buscartercero.cot_numcotizacion;
            ViewBag.cliente = buscartercero.cliente;
            ViewBag.PrimerNombre = buscartercero.prinom_tercero;
            ViewBag.SegundoNombre = buscartercero.segnom_tercero;
            ViewBag.PrimerApellido = buscartercero.apellido_tercero;
            ViewBag.SegundoApellido = buscartercero.segapellido_tercero;
            ViewBag.RazonSocial = buscartercero.razon_social;
            ViewBag.TipoIdentificacion = buscartercero.tpdoc_id;
            ViewBag.documento_cliente = buscartercero.doc_tercero;
            ViewBag.direccion = buscartercero.direccion != null ? buscartercero.direccion.direccion : "";
            ViewBag.telefono = buscartercero.telf_tercero;
            ViewBag.celular = buscartercero.celular_tercero;
            ViewBag.correo = buscartercero.email_tercero;
            ViewBag.fecha = buscartercero.cot_feccreacion.ToString("yyyy/MM/dd");
            ViewBag.marcvh_id = new SelectList(context.marca_vehiculo.Where(x => x.marcvh_estado), "marcvh_id",
                "marcvh_nombre");
            ViewBag.colVh_id = new SelectList(context.color_vehiculo, "colvh_id", "colvh_nombre");
            ViewBag.plan_id =
                new SelectList(context.icb_plan_financiero.Where(x => x.plan_estado).OrderBy(x => x.plan_nombre),
                    "plan_id", "plan_nombre");
            ViewBag.cotizacion_id = id;
            //if (permiso == 0)
            //{
                
            //}
            //else
            //{
               
            //}

            if (buscarAnuladas)
            {
                TempData["mensaje_error"] = "Esta cotización ha sido finalizada";
                BuscarFavoritos(menu);
                return View();
            }

            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult agregarDatosProspecto(int? idTercero, int tpDocumento, string documento, string primNombre,
            string segNombre, string primApellido, string segApellido,
            int genero, string correo, string telefono, string celular, string direccion, int ciudad, int origen,
            int medComunicacion, bool enviarCorreo, bool hdCorreo, bool hdCelular, bool hdMsm, bool hdWhatsapp,
            string obs)
        {
            int terceroIdCotizacion = 0;
            icb_terceros buscaDocumento = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == documento);
            if (buscaDocumento == null)
            {
                // Si la cotizacion no viene de un prospecto se agrega un nuevo tercero
                if (idTercero == null)
                {
                    context.icb_terceros.Add(new icb_terceros
                    {
                        tpdoc_id = tpDocumento,
                        doc_tercero = documento,
                        prinom_tercero = primNombre,
                        segnom_tercero = segNombre,
                        apellido_tercero = primApellido,
                        segapellido_tercero = segApellido,
                        email_tercero = correo,
                        celular_tercero = celular,
                        //direc_tercero = direccion,
                        ciu_id = ciudad,
                        telf_tercero = telefono,
                        origen_id = origen,
                        medcomun_id = medComunicacion,
                        habdtautor_correo = hdCorreo,
                        habdtautor_celular = hdCelular,
                        habdtautor_msm = hdMsm,
                        habdtautor_watsap = hdWhatsapp,
                        observaciones = obs,
                        genero_tercero = genero,
                        tercerofec_creacion = DateTime.Now,
                        tercerouserid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                    });
                    bool result = context.SaveChanges() > 0;
                    if (result)
                    {
                        terceroIdCotizacion =
                            context.icb_terceros.OrderByDescending(x => x.tercero_id).First().tercero_id;
                    }
                }
                // Si la cotizacion viene de un registro de prospecto ya existente se actualiza el prospecto buscado por su id
                else
                {
                    icb_terceros terceroBuscado = context.icb_terceros.FirstOrDefault(x => x.tercero_id == idTercero);
                    terceroIdCotizacion = terceroBuscado.tercero_id;
                    if (terceroBuscado != null)
                    {
                        terceroBuscado.tpdoc_id = tpDocumento;
                        terceroBuscado.doc_tercero = documento;
                        terceroBuscado.prinom_tercero = primNombre;
                        terceroBuscado.segnom_tercero = segNombre;
                        terceroBuscado.apellido_tercero = primApellido;
                        terceroBuscado.segapellido_tercero = segApellido;
                        terceroBuscado.email_tercero = correo;
                        terceroBuscado.celular_tercero = celular;
                        //terceroBuscado.direc_tercero = direccion;
                        terceroBuscado.habdtautor_correo = hdCorreo;
                        terceroBuscado.habdtautor_celular = hdCelular;
                        terceroBuscado.habdtautor_msm = hdMsm;
                        terceroBuscado.habdtautor_watsap = hdWhatsapp;
                        terceroBuscado.observaciones = obs;
                        terceroBuscado.ciu_id = ciudad;
                        terceroBuscado.genero_tercero = genero;
                        context.Entry(terceroBuscado).State = EntityState.Modified;
                        bool result = context.SaveChanges() > 0;
                    }
                }
            }
            else
            {
                terceroIdCotizacion = buscaDocumento.tercero_id;
                if (buscaDocumento != null)
                {
                    buscaDocumento.tpdoc_id = tpDocumento;
                    buscaDocumento.doc_tercero = documento;
                    buscaDocumento.prinom_tercero = primNombre;
                    buscaDocumento.segnom_tercero = segNombre;
                    buscaDocumento.apellido_tercero = primApellido;
                    buscaDocumento.segapellido_tercero = segApellido;
                    buscaDocumento.email_tercero = correo;
                    buscaDocumento.celular_tercero = celular;
                    buscaDocumento.habdtautor_correo = hdCorreo;
                    buscaDocumento.habdtautor_celular = hdCelular;
                    buscaDocumento.habdtautor_msm = hdMsm;
                    buscaDocumento.habdtautor_watsap = hdWhatsapp;
                    buscaDocumento.observaciones = obs;
                    //buscaDocumento.direc_tercero = direccion;
                    buscaDocumento.ciu_id = ciudad;
                    buscaDocumento.genero_tercero = genero;
                    context.Entry(buscaDocumento).State = EntityState.Modified;
                    bool result = context.SaveChanges() > 0;
                }
            }

            // Tipo de documento, el necesario es tipo de documento, en este caso en necesario el 1 correpondiente el tipo de doc cotizacion
            icb_sysparameter buscarParametroTpDocCotiza = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P11");
            string documentoCotizacionParametro =
                buscarParametroTpDocCotiza != null ? buscarParametroTpDocCotiza.syspar_value : "8";
            int idTpDocCotizacion = Convert.ToInt32(documentoCotizacionParametro);

            icb_doc_consecutivos buscaConsecutivo =
                context.icb_doc_consecutivos.FirstOrDefault(x => x.doccons_idtpdoc == idTpDocCotizacion);

            try
            {
                decimal numeroConsecutivo = 0;
                if (buscaConsecutivo != null)
                {
                    numeroConsecutivo = buscaConsecutivo.doccons_siguiente;
                    buscaConsecutivo.doccons_siguiente = buscaConsecutivo.doccons_siguiente + 1;
                    context.Entry(buscaConsecutivo).State = EntityState.Modified;
                }
                else
                {
                    var respuesta = new
                    {
                        tipoRespuesta = false,
                        mensaje = "No hay un numero consecutivo para realizar una nueva cotizacion"
                    };
                    // Si sale aqui significa que no hay un numero consecutivo en la tabla icb_doc_consecutivo, por tanto no se puede agrega una cotizacion sin un numero
                    return Json(respuesta, JsonRequestBehavior.AllowGet);
                }

                icb_cotizacion nuevaCotizacion = new icb_cotizacion
                {
                    id_tercero = terceroIdCotizacion,
                    cot_numcotizacion = numeroConsecutivo,
                    //id_formapago = formaPago,
                    //id_aseguradora = aseguradora,
                    cot_feccreacion = DateTime.Now,
                    cot_usucreacion = Convert.ToInt32(Session["user_usuarioid"]),
                    cot_envio_cotizacion = enviarCorreo,
                    id_medio = medComunicacion
                    //cot_observacion = cotizacion.observacion,
                };
                context.icb_cotizacion.Add(nuevaCotizacion);
                bool guardaCotizacion = context.SaveChanges() > 0;
                if (guardaCotizacion)
                {
                    icb_cotizacion buscarUltimaCotizacion =
                        context.icb_cotizacion.OrderByDescending(x => x.cot_idserial).FirstOrDefault();
                    var respuesta = new
                    {
                        tipoRespuesta = true,
                        mensaje = "Los datos se han registrado con exito",
                        idCotizacion = buscarUltimaCotizacion.cot_numcotizacion,
                        numeroCotizacion = numeroConsecutivo
                    };
                    return Json(respuesta, JsonRequestBehavior.AllowGet);
                }
            }
            catch (DbEntityValidationException)
            {
                var respuesta = new
                {
                    tipoRespuesta = false,
                    mensaje = "Error en base de datos, por favor valide su conexion"
                };

                return Json(respuesta, JsonRequestBehavior.AllowGet);
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarRepuestosGenericos(string cod)
        {
            var buscarRepuestos = context.icb_referencia.Where(x => x.modulo == "R" && x.tipo_id == 2 && x.ref_estado && (x.ref_codigo.Contains(cod) || x.ref_descripcion.Contains(cod) || x.alias.Contains(cod)))
                .OrderBy(x => x.ref_descripcion).Select(x => new
                {
                    x.ref_codigo,
                    x.ref_descripcion,
                    alias = x.alias ?? ""
                }).ToList();

            return Json(buscarRepuestos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarRepuestosGenericosPedido()
        {
            var buscarRepuestos = context.icb_referencia.Where(x => x.modulo == "R" && x.tipo_id == 2 && x.ref_estado)
                .OrderBy(x => x.ref_descripcion).Select(x => new
                {
                    x.ref_codigo,
                    x.ref_descripcion,
                    alias = x.alias ?? ""
                }).ToList();

            return Json(buscarRepuestos, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult buscarAccesorios()
        {
            var buscarRepuestos = context.icb_referencia.Where(x => x.modulo == "R" && x.tipo_id == 2 && x.ref_estado)
                .OrderBy(x => x.ref_descripcion).Select(x => new
                {
                    x.ref_codigo,
                    x.ref_descripcion
                }).ToList();

            return Json(buscarRepuestos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarAccesoriosConsulta(string referencia)
        {
            var referencias = (from r in context.icb_referencia
                               where r.modulo == "R" && r.tipo_id == 2 && r.ref_estado && (r.ref_descripcion.Contains(referencia) || r.ref_codigo.Contains(referencia))
                               select new
                               {
                                   referencia = r.ref_codigo + " | " + r.ref_descripcion
                               }).ToList();

            List<string> referencias_data = referencias.Select(d => d.referencia).ToList();
            return Json(referencias_data);
        }

        public JsonResult buscarRepuestosPorModelo(string modelo)
        {
            var buscarAccesoriosXModelo = context.vaccesoriomodelo.Where(x => x.modeloid == modelo && x.estado)
                .OrderBy(x => x.descripcion).Select(x => new
                {
                    x.referencia,
                    x.descripcion
                }).ToList();

            return Json(buscarAccesoriosXModelo, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarPrecioReferencia(string idRepuesto)
        {
            var consultarPrecio = (from referencia in context.icb_referencia
                                   join precios in context.rprecios
                                       on referencia.ref_codigo equals precios.codigo
                                   where referencia.ref_codigo == idRepuesto
                                   select new
                                   {
                                       precios.precio1,
                                       precios.codigo
                                   }).FirstOrDefault();

            return Json(consultarPrecio, JsonRequestBehavior.AllowGet);
        }

        public JsonResult agregarReferencia(string idReferencia, decimal costo, int cantidad, int idCotizacion,
            int? idAccesorio, string codigo_modelovh)
        {
            var OK = false;
            var mensaje = "error";
            if (codigo_modelovh!="" && idReferencia!="" && costo >0 && cantidad>0 && idCotizacion>0)
            {
                var modelo_id1 = context.vcotrepuestos.Where(x => x.cot_id == idCotizacion).FirstOrDefault();
                var modelo_id = context.icb_cotizacion.Where(x => x.cot_idserial == idCotizacion).FirstOrDefault();

                if (idAccesorio != null)
                {
                    vcotrepuestos buscarDetalleCotizado = context.vcotrepuestos.FirstOrDefault(x => x.id == idAccesorio);
                    if (buscarDetalleCotizado != null)
                    {
                        buscarDetalleCotizado.cantidad = cantidad;
                        buscarDetalleCotizado.vrunitario = costo;
                        buscarDetalleCotizado.referencia = idReferencia;
                        buscarDetalleCotizado.modelo_id = codigo_modelovh;
                        //buscarDetalleCotizado.modelo_id ;
                        context.Entry(buscarDetalleCotizado).State = EntityState.Modified;
                        int actualizar = context.SaveChanges();
                        if (actualizar > 0)
                        {
                            OK = true;
                            mensaje = "Se guardo la Referencia";
                        }
                    }
                }
                else
                {
                    context.vcotrepuestos.Add(new vcotrepuestos
                    {
                        cot_id = idCotizacion,
                        referencia = idReferencia,
                        cantidad = cantidad,
                        vrunitario = costo,
                        fec_creacion = DateTime.Now,
                        modelo_id = codigo_modelovh,
                        user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"])
                    });
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        var BuscarSeguimiento = context.Seguimientos.Where(x => x.EsManual == false && x.ModuloSeguimientos.Codigo == 6 && x.Codigo == 3).FirstOrDefault();
                        vcotseguimiento seguimiento = new vcotseguimiento
                        {
                            cot_id = idCotizacion,
                            fecha = DateTime.Now,
                            responsable = Convert.ToInt32(Session["user_usuarioid"]),
                            Notas = "Agrego Referencia Modelo: "+ codigo_modelovh +" cot ID: "+ Convert.ToString(idCotizacion),
                            Motivo = null,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            estado = true,
                            tipo_seguimiento = BuscarSeguimiento.Id
                        };
                        context.vcotseguimiento.Add(seguimiento);
                        int guardarGeneral = context.SaveChanges();

                        OK = true;
                        mensaje = "Se guardo la Referencia: "+ Convert.ToString(idReferencia) ;
                    }
                }
            }
            else
            {
                mensaje = "Faltan campos esenciales ";
            }

            var Data = new
            {
                OK,
                mensaje
            };
            return Json(Data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AgregarVehiculoPosterior(int? idCotizacion, string idModelo, int anio, string color,
            decimal valor, decimal? poliza, decimal? matricula, decimal? soat, string observacion, bool esNuevo,
            int? idVehiculoCotizado)
        {
            // VALIDACION PARA SABER SI SE AGREGA UNO NUEVO O UNO USADO
            bool nuevo = false;
            bool usado = false;
            if (esNuevo)
            {
                nuevo = true;
            }
            else
            {
                usado = true;
            }

            if (idVehiculoCotizado != null)
            {
                vcotdetallevehiculo buscarVhCotizado =
                    context.vcotdetallevehiculo.FirstOrDefault(x => x.detalle_id == idVehiculoCotizado);
                if (buscarVhCotizado != null)
                {
                    buscarVhCotizado.idmodelo = idModelo;
                    buscarVhCotizado.anomodelo = anio;
                    buscarVhCotizado.color = color;
                    buscarVhCotizado.precio = valor;
                    buscarVhCotizado.poliza = poliza;
                    buscarVhCotizado.matricula = matricula;
                    buscarVhCotizado.soat = soat;
                    buscarVhCotizado.observacion = observacion;
                    buscarVhCotizado.nuevo = nuevo;
                    buscarVhCotizado.usado = usado;
                    context.Entry(buscarVhCotizado).State = EntityState.Modified;
                    int actualizar = context.SaveChanges();
                    if (actualizar > 0)
                    {
                        return Json(new { estado = true, respuesta = "Registro actualizado exitosamente" },
                            JsonRequestBehavior.AllowGet);
                    }
                }
            }
            else
            {
                if (idCotizacion != null)
                {
                    context.vcotdetallevehiculo.Add(new vcotdetallevehiculo
                    {
                        idcotizacion = idCotizacion ?? 0,
                        idmodelo = idModelo,
                        matricula = matricula,
                        soat = soat,
                        poliza = poliza,
                        precio = valor,
                        observacion = observacion,
                        anomodelo = anio,
                        color = color,
                        nuevo = nuevo,
                        usado = usado
                    });
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        return Json(new { estado = true, respuesta = "Registro agregado exitosamente" },
                            JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { estado = false, respuesta = "Error de conexion", JsonRequestBehavior.AllowGet });
                }
            }

            return Json(new { estado = false, respuesta = "Registro no guardado, verifique los datos" },
                JsonRequestBehavior.AllowGet);
        }


        public JsonResult agregarRetomaVehiculoACotizacion(retoma retomas)
        {

            if (retomas != null)
            {
                if (retomas.idcot != 0)
                {
                    context.vcotretoma.Add(new vcotretoma
                    {
                       ano=retomas.ano,
                       idcot=retomas.idcot,
                       Kilometraje=retomas.Kilometraje,
                       modelo=retomas.modelo,
                       placa=retomas.placa,
                    });
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        return Json(new { estado = true, respuesta = "Registro agregado exitosamente" },
                            JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { estado = false, respuesta = "Error de conexion", JsonRequestBehavior.AllowGet });
                }

            }
            else
            {
 
            }

            return Json(new { estado = false, respuesta = "Registro no guardado, verifique los datos" },
                JsonRequestBehavior.AllowGet);
        }
     
        public JsonResult eliminarVehiculo(int idDetalle)
        {
            vcotdetallevehiculo buscarVehiculoCotizado = context.vcotdetallevehiculo.FirstOrDefault(x => x.detalle_id == idDetalle);
            if (buscarVehiculoCotizado != null)
            {
                context.Entry(buscarVehiculoCotizado).State = EntityState.Deleted;
                int eliminar = context.SaveChanges();
                if (eliminar > 0)
                {
                    var BuscarSeguimiento = context.Seguimientos.Where(x => x.EsManual == false && x.ModuloSeguimientos.Codigo == 6 && x.Codigo == 9).FirstOrDefault();
                    vcotseguimiento seguimiento = new vcotseguimiento
                    {
                        cot_id = buscarVehiculoCotizado.idcotizacion,
                        fecha = DateTime.Now,
                        responsable = Convert.ToInt32(Session["user_usuarioid"]),
                        Notas = "Elimino Modelo Vehiculo ID COT: "+ Convert.ToString(buscarVehiculoCotizado.idcotizacion) +" Modelo: "+ Convert.ToString(buscarVehiculoCotizado.idmodelo),
                        Motivo = null,
                        fec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        estado = true,
                        tipo_seguimiento = BuscarSeguimiento.Id
                    };
                    context.vcotseguimiento.Add(seguimiento);
                    int guardarGeneral = context.SaveChanges();

                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult eliminarAccesorio(int idDetalle)
        {
            vcotrepuestos buscarAccesorioCotizado = context.vcotrepuestos.FirstOrDefault(x => x.id == idDetalle);
            if (buscarAccesorioCotizado != null)
            {
                context.Entry(buscarAccesorioCotizado).State = EntityState.Deleted;
                int eliminar = context.SaveChanges();
                if (eliminar > 0)
                {
                    var BuscarSeguimiento = context.Seguimientos.Where(x => x.EsManual == false && x.ModuloSeguimientos.Codigo == 6 && x.Codigo == 10).FirstOrDefault();
                    vcotseguimiento seguimiento = new vcotseguimiento
                    {
                        cot_id = buscarAccesorioCotizado.cot_id,
                        fecha = DateTime.Now,
                        responsable = Convert.ToInt32(Session["user_usuarioid"]),
                        Notas = "Elimino Referencia: " +" Ref ID: " + Convert.ToString(idDetalle),
                        Motivo = null,
                        fec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        estado = true,
                        tipo_seguimiento = BuscarSeguimiento.Id
                    };
                    context.vcotseguimiento.Add(seguimiento);
                    int guardarGeneral = context.SaveChanges();
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BuscarRetoma(int idDetalle)
        {
            var BuscarRetoma = context.vcotretoma.Where(d => d.id == idDetalle).FirstOrDefault();

            //var buscaAccesorio = context.vcotrepuestos.FirstOrDefault(x=>x.id==idDetalle);
            if (BuscarRetoma != null)
            {
                return Json(new
                {
                    encontrado = true,

                    BuscarRetoma.placa,
                    BuscarRetoma.valor,
                    BuscarRetoma.modelo,
                    BuscarRetoma.Kilometraje,
                    BuscarRetoma.ano,
                    BuscarRetoma.id,
                });
            }
            

            return Json(new { encontrado = false }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult eliminarRetoma(int idDetalle)
        {
            vcotretoma buscarRetoma = context.vcotretoma.FirstOrDefault(x => x.id == idDetalle);
            if (buscarRetoma != null)
            {
                context.Entry(buscarRetoma).State = EntityState.Deleted;
                int eliminar = context.SaveChanges();
                if (eliminar > 0)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarClientePorDocumento(string documento, int? tipoDocumento)
        {
            icb_terceros buscarTercero =
                context.icb_terceros.FirstOrDefault(x => x.doc_tercero == documento && x.tpdoc_id == tipoDocumento);

            if (buscarTercero != null)
            {
                prospectos buscarProspecto = context.prospectos.OrderByDescending(x => x.id)
                    .FirstOrDefault(x => x.idtercero == buscarTercero.tercero_id);
                tercero_cliente buscarCliente = context.tercero_cliente.OrderByDescending(x => x.cltercero_id)
                    .FirstOrDefault(x => x.tercero_id == buscarTercero.tercero_id);

                var buscarUltimaDireccion = (from direcciones in context.terceros_direcciones
                                             join ciudad in context.nom_ciudad
                                                 on direcciones.ciudad equals ciudad.ciu_id
                                             where direcciones.idtercero == buscarTercero.tercero_id
                                             select new
                                             {
                                                 direcciones.id,
                                                 direcciones.direccion,
                                                 direcciones.ciudad,
                                                 ciudad.ciu_nombre
                                             }).OrderByDescending(x => x.id).FirstOrDefault();

                if (buscarProspecto != null)
                {
                    if (buscarCliente != null)
                    {
                        var data = new
                        {
                            clienteExiste = true,
                            terceroExiste = true,
                            buscarTercero.tercero_id,
                            buscarTercero.prinom_tercero,
                            buscarTercero.segnom_tercero,
                            buscarTercero.apellido_tercero,
                            buscarTercero.segapellido_tercero,
                            buscarTercero.razon_social,
                            buscarTercero.genero_tercero,
                            buscarTercero.email_tercero,
                            buscarTercero.telf_tercero,
                            buscarTercero.celular_tercero,
                            direc_tercero = buscarUltimaDireccion != null ? buscarUltimaDireccion.direccion : "",
                            ciudad_direccionId = buscarUltimaDireccion != null ? buscarUltimaDireccion.ciudad : 0,
                            ciudad_nombre = buscarUltimaDireccion != null ? buscarUltimaDireccion.ciu_nombre : "",
                            buscarTercero.ciu_id,
                            //buscarTercero.origen_id,
                            buscarTercero.medcomun_id,
                            buscarTercero.asesor_id,
                            buscarProspecto.origen_id,
                            buscarProspecto.subfuente
                        };
                        var buscar_creditos = context.vinfcredito.Where(x => x.tercero == buscarTercero.tercero_id)
                            .Select(x => new
                            {
                                estado = x.v_creditos.Select(a => a.estadoc).FirstOrDefault(),
                                valor = x.v_creditos.Select(b => b.vsolicitado).FirstOrDefault(),
                                aprobado = x.v_creditos.Select(c => c.vaprobado).FirstOrDefault()
                            }).ToList();

                        return Json(new { data, buscar_creditos }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var data = new
                        {
                            clienteExiste = false,
                            terceroExiste = true,
                            buscarTercero.tercero_id,
                            buscarTercero.prinom_tercero,
                            buscarTercero.segnom_tercero,
                            buscarTercero.apellido_tercero,
                            buscarTercero.segapellido_tercero,
                            buscarTercero.razon_social,
                            buscarTercero.genero_tercero,
                            buscarTercero.email_tercero,
                            buscarTercero.telf_tercero,
                            buscarTercero.celular_tercero,
                            direc_tercero = buscarUltimaDireccion != null ? buscarUltimaDireccion.direccion : "",
                            ciudad_direccionId = buscarUltimaDireccion != null ? buscarUltimaDireccion.ciudad : 0,
                            ciudad_nombre = buscarUltimaDireccion != null ? buscarUltimaDireccion.ciu_nombre : "",
                            buscarTercero.ciu_id,
                            //buscarTercero.origen_id,
                            buscarTercero.medcomun_id,
                            buscarTercero.asesor_id,
                            buscarProspecto.origen_id,
                            buscarProspecto.subfuente
                        };
                        var buscar_creditos = context.vinfcredito.Where(x => x.tercero == buscarTercero.tercero_id)
                            .Select(x => new
                            {
                                estado = x.v_creditos.Select(a => a.estadoc).FirstOrDefault(),
                                valor = x.v_creditos.Select(b => b.vsolicitado).FirstOrDefault(),
                                aprobado = x.v_creditos.Select(c => c.vaprobado).FirstOrDefault()
                            }).ToList();

                        return Json(new { data, buscar_creditos }, JsonRequestBehavior.AllowGet);
                    }
                }

                if (buscarCliente != null)
                {
                    var data = new
                    {
                        clienteExiste = true,
                        terceroExiste = true,
                        buscarTercero.tercero_id,
                        buscarTercero.prinom_tercero,
                        buscarTercero.segnom_tercero,
                        buscarTercero.apellido_tercero,
                        buscarTercero.segapellido_tercero,
                        buscarTercero.razon_social,
                        buscarTercero.genero_tercero,
                        buscarTercero.email_tercero,
                        buscarTercero.telf_tercero,
                        buscarTercero.celular_tercero,
                        direc_tercero = buscarUltimaDireccion != null ? buscarUltimaDireccion.direccion : "",
                        ciudad_direccionId = buscarUltimaDireccion != null ? buscarUltimaDireccion.ciudad : 0,
                        ciudad_nombre = buscarUltimaDireccion != null ? buscarUltimaDireccion.ciu_nombre : "",
                        buscarTercero.ciu_id,
                        //buscarTercero.origen_id,
                        buscarTercero.medcomun_id,
                        buscarTercero.asesor_id
                    };
                    var buscar_creditos = context.vinfcredito.Where(x => x.tercero == buscarTercero.tercero_id).Select(
                        x => new
                        {
                            estado = x.v_creditos.Select(a => a.estadoc).FirstOrDefault(),
                            valor = x.v_creditos.Select(b => b.vsolicitado).FirstOrDefault(),
                            aprobado = x.v_creditos.Select(c => c.vaprobado).FirstOrDefault()
                        }).ToList();

                    return Json(new { data, buscar_creditos }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var data = new
                    {
                        clienteExiste = false,
                        terceroExiste = true,
                        buscarTercero.tercero_id,
                        buscarTercero.prinom_tercero,
                        buscarTercero.segnom_tercero,
                        buscarTercero.apellido_tercero,
                        buscarTercero.segapellido_tercero,
                        buscarTercero.razon_social,
                        buscarTercero.genero_tercero,
                        buscarTercero.email_tercero,
                        buscarTercero.telf_tercero,
                        buscarTercero.celular_tercero,
                        direc_tercero = buscarUltimaDireccion != null ? buscarUltimaDireccion.direccion : "",
                        ciudad_direccionId = buscarUltimaDireccion != null ? buscarUltimaDireccion.ciudad : 0,
                        ciudad_nombre = buscarUltimaDireccion != null ? buscarUltimaDireccion.ciu_nombre : "",
                        buscarTercero.ciu_id,
                        //buscarTercero.origen_id,
                        buscarTercero.medcomun_id,
                        buscarTercero.asesor_id
                    };
                    var buscar_creditos = context.vinfcredito.Where(x => x.tercero == buscarTercero.tercero_id).Select(
                        x => new
                        {
                            estado = x.v_creditos.Select(a => a.estadoc).FirstOrDefault(),
                            valor = x.v_creditos.Select(b => b.vsolicitado).FirstOrDefault(),
                            aprobado = x.v_creditos.Select(c => c.vaprobado).FirstOrDefault()
                        }).ToList();

                    return Json(new { data, buscar_creditos }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { clienteExiste = false, terceroExiste = false }, JsonRequestBehavior.AllowGet);
        }

        public void ListasDesplegables(Cotizacion cotizacion)
        {
            ViewBag.rol = Convert.ToInt32(Session["user_rolid"]);
            ViewBag.gentercero_id =   new SelectList(context.gen_tercero.OrderBy(x => x.gentercero_nombre).Where(x => x.gentercero_estado),"gentercero_id", "gentercero_nombre", cotizacion != null ? cotizacion.gentercero_id : 0);
            ViewBag.ciu_id = new SelectList(context.nom_ciudad.OrderBy(x => x.ciu_nombre).Where(x => x.ciu_estado),"ciu_id", "ciu_nombre", cotizacion != null ? cotizacion.ciu_id : 0);
            ViewBag.dpto_id = new SelectList(context.nom_departamento.OrderBy(x => x.dpto_nombre).Where(x => x.dpto_estado),"dpto_id", "dpto_nombre", cotizacion != null ? cotizacion.dpto_id : 0);
            ViewBag.marcvh_id = context.marca_vehiculo.Where(x => x.marcvh_estado).OrderBy(x => x.marcvh_nombre).ToList();
            ViewBag.colVh_id =new SelectList(context.color_vehiculo.OrderBy(x => x.colvh_nombre).Where(x => x.colvh_estado),
                    "colvh_id", "colvh_nombre");
            ViewBag.fpago_id =
                new SelectList(context.fpago_tercero.OrderBy(x => x.fpago_nombre).Where(x => x.fpago_estado),
                    "fpago_id", "fpago_nombre", cotizacion != null ? cotizacion.fpago_id : 0);
            ViewBag.aseg_id = new SelectList(context.icb_aseguradoras.OrderBy(x => x.nombre).Where(x => x.estado),
                "aseg_id", "aseg_nombre", cotizacion != null ? cotizacion.aseg_id : 0);
            ViewBag.tporigen_id =
                new SelectList(context.tp_origen.OrderBy(x => x.tporigen_nombre).Where(x => x.tporigen_estado),
                    "tporigen_id", "tporigen_nombre", cotizacion != null ? cotizacion.tporigen_id : 0);
            ViewBag.medcomun_id =
                new SelectList(
                    context.icb_medio_comunicacion.OrderBy(x => x.medcomun_descripcion).Where(x => x.medcomun_estado),
                    "medcomun_id", "medcomun_descripcion", cotizacion != null ? cotizacion.medcomun_id : 0);
            ViewBag.tpdoc_id =
                new SelectList(context.tp_documento.OrderBy(x => x.tpdoc_nombre).Where(x => x.tpdoc_estado), "tpdoc_id",
                    "tpdoc_nombre", cotizacion != null ? cotizacion.tpdoc_id : 0);
            ViewBag.plan_id =
                new SelectList(context.icb_plan_financiero.OrderBy(x => x.plan_nombre).Where(x => x.plan_estado),
                    "plan_id", "plan_nombre", cotizacion != null ? cotizacion.idplan_pago : 0);
            ViewBag.plan_idDetalle =
                new SelectList(context.icb_plan_financiero.OrderBy(x => x.plan_nombre).Where(x => x.plan_estado),
                    "plan_id", "plan_nombre", cotizacion != null ? cotizacion.idplan_pago : 0);
            ViewBag.ocupacion =
                new SelectList(context.tp_ocupacion.Where(x => x.tpocupacion_estado).OrderBy(x => x.tpocupacion_nombre),
                    "tpocupacion_id", "tpocupacion_nombre");
            List<users> asesoresAux = context.users.Where(x => x.rol_id == 4 || x.rol_id == 2030).ToList();
            List<SelectListItem> listaAsesoresAux = new List<SelectListItem>();
            foreach (users asesor in asesoresAux)
            {
                listaAsesoresAux.Add(new SelectListItem
                { Value = asesor.user_id.ToString(), Text = asesor.user_nombre + " " + asesor.user_apellido });
            }

            ViewBag.asesorAsignado = listaAsesoresAux.OrderBy(x => x.Text);
            int tipoOrigen = cotizacion != null ? cotizacion.tporigen_id : 0;
            ViewBag.subfuente_id =
                new SelectList(context.tp_subfuente.Where(x => x.fuente == tipoOrigen).OrderBy(x => x.subfuente), "id",
                    "subfuente", cotizacion != null ? cotizacion.subfuente_id : 0);
            ViewBag.motivo_anulacion = new SelectList(context.motivoanulacion.OrderBy(x => x.motivo), "id", "motivo");
        }

        public JsonResult AgregarPlanPago(string idXCot, string tasa_interes, int valor_total, int cuota_inicial,
            string cuotas, int credito, string cuota12, string cuota24, string cuota36,
            string cuota48, string cuota60, string cuota72, string cuota84, string valor_cuota12, string valor_cuota24,
            string valor_cuota36, string valor_cuota48,
            string valor_cuota60, string valor_cuota72, string valor_cuota84, int? plan_id, int? valor_residual,
            int? otros_valores, string mod)
        {
            int idXCotAux = Convert.ToInt32(idXCot);
            decimal tasa_interesAux = Convert.ToDecimal(tasa_interes, inter);
            string elmodelo = mod;

            //var cotizacion_id = context.icb_cotizacion.OrderByDescending(x => x.cot_idserial).FirstOrDefault().cot_idserial;
            context.icb_plan_pago.Add(new icb_plan_pago
            {
                asesor_id = Convert.ToInt32(Session["user_usuarioid"]),
                cotizacion_id = int.Parse(idXCot),
                tasa_interes = tasa_interes,
                valor_total = valor_total,
                credito = credito,
                cuota_inicial = cuota_inicial,
                cuotas = cuotas,
                plan_id = plan_id,
                plan_elegido = false,
                valor_residual = valor_residual != null ? Convert.ToDecimal(valor_residual, inter) : 0,
                otros_valores = otros_valores != null ? Convert.ToDecimal(otros_valores, inter) : 0,
                modelo = elmodelo
            });
            int result = context.SaveChanges();

            #region seguimiento
            var BuscarSeguimiento = context.Seguimientos.Where(x => x.EsManual == false && x.ModuloSeguimientos.Codigo == 6 && x.Codigo == 6).FirstOrDefault();
            if (result==1)
            {
                vcotseguimiento seguimiento = new vcotseguimiento
                {
                    cot_id = idXCotAux,
                    fecha = DateTime.Now,
                    responsable = Convert.ToInt32(Session["user_usuarioid"]),
                    Notas = "Agrego Plan de Pago ID: " + Convert.ToString(plan_id) + " Modelo: " + mod,
                    Motivo = null,
                    fec_creacion = DateTime.Now,
                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    estado = true,
                    tipo_seguimiento = BuscarSeguimiento.Id
                };
                context.vcotseguimiento.Add(seguimiento);
                int guardarGeneral = context.SaveChanges();
            }
            #endregion

            int plan_pago_id = context.icb_plan_pago.OrderByDescending(x => x.id).FirstOrDefault().id;

            icb_plan_matriz plan_matriz = new icb_plan_matriz
            {
                plan_pago_id = plan_pago_id
            };

            if (!string.IsNullOrEmpty(cuota12))
            {
                int valor_cuota12Aux = Convert.ToInt32(valor_cuota12);
                IQueryable<icb_plan_matriz> esta1 = context.icb_plan_matriz.Where(x =>
                    x.cotizacion_id == idXCotAux && x.valor == valor_cuota12Aux && x.plazo == cuota12 &&
                    x.tasa == tasa_interesAux);
                if (esta1.Count() == 0)
                {
                    plan_matriz.plazo = cuota12;
                    plan_matriz.valor = Convert.ToDecimal(valor_cuota12, inter);
                    plan_matriz.cotizacion_id = int.Parse(idXCot);
                    plan_matriz.tasa = Convert.ToDecimal(tasa_interes, inter);
                    context.icb_plan_matriz.Add(plan_matriz);
                    result = context.SaveChanges();
                }
            }

            if (!string.IsNullOrEmpty(cuota24))
            {
                int valor_cuota24Aux = Convert.ToInt32(valor_cuota24);
                IQueryable<icb_plan_matriz> esta2 = context.icb_plan_matriz.Where(x =>
                    x.cotizacion_id == idXCotAux && x.valor == valor_cuota24Aux && x.plazo == cuota24 &&
                    x.tasa == tasa_interesAux);
                if (esta2.Count() == 0)
                {
                    plan_matriz.plazo = cuota24;
                    plan_matriz.valor = Convert.ToDecimal(valor_cuota24, inter);
                    plan_matriz.cotizacion_id = int.Parse(idXCot);
                    plan_matriz.tasa = Convert.ToDecimal(tasa_interes, inter);
                    context.icb_plan_matriz.Add(plan_matriz);
                    result = context.SaveChanges();
                }
            }

            if (!string.IsNullOrEmpty(cuota36))
            {
                int valor_cuota36Aux = Convert.ToInt32(valor_cuota36);
                IQueryable<icb_plan_matriz> esta3 = context.icb_plan_matriz.Where(x =>
                    x.cotizacion_id == idXCotAux && x.valor == valor_cuota36Aux && x.plazo == cuota36 &&
                    x.tasa == tasa_interesAux);
                if (esta3.Count() == 0)
                {
                    plan_matriz.plazo = cuota36;
                    plan_matriz.valor = Convert.ToDecimal(valor_cuota36, inter);
                    plan_matriz.cotizacion_id = int.Parse(idXCot);
                    plan_matriz.tasa = Convert.ToDecimal(tasa_interes, inter);
                    context.icb_plan_matriz.Add(plan_matriz);
                    result = context.SaveChanges();
                }
            }

            if (!string.IsNullOrEmpty(cuota48))
            {
                int valor_cuota48Aux = Convert.ToInt32(valor_cuota48);
                IQueryable<icb_plan_matriz> esta4 = context.icb_plan_matriz.Where(x =>
                    x.cotizacion_id == idXCotAux && x.valor == valor_cuota48Aux && x.plazo == cuota48 &&
                    x.tasa == tasa_interesAux);
                if (esta4.Count() == 0)
                {
                    plan_matriz.plazo = cuota48;
                    plan_matriz.valor = Convert.ToDecimal(valor_cuota48, inter);
                    plan_matriz.cotizacion_id = int.Parse(idXCot);
                    plan_matriz.tasa = Convert.ToDecimal(tasa_interes, inter);
                    context.icb_plan_matriz.Add(plan_matriz);
                    result = context.SaveChanges();
                }
            }

            if (!string.IsNullOrEmpty(cuota60))
            {
                int valor_cuota60Aux = Convert.ToInt32(valor_cuota60);
                IQueryable<icb_plan_matriz> esta5 = context.icb_plan_matriz.Where(x =>
                    x.cotizacion_id == idXCotAux && x.valor == valor_cuota60Aux && x.plazo == cuota60 &&
                    x.tasa == tasa_interesAux);
                if (esta5.Count() == 0)
                {
                    plan_matriz.plazo = cuota60;
                    plan_matriz.valor = Convert.ToDecimal(valor_cuota60, inter);
                    plan_matriz.cotizacion_id = int.Parse(idXCot);
                    plan_matriz.tasa = Convert.ToDecimal(tasa_interes, inter);
                    context.icb_plan_matriz.Add(plan_matriz);
                    result = context.SaveChanges();
                }
            }

            if (!string.IsNullOrEmpty(cuota72))
            {
                int valor_cuota72Aux = Convert.ToInt32(valor_cuota72);
                IQueryable<icb_plan_matriz> esta6 = context.icb_plan_matriz.Where(x =>
                    x.cotizacion_id == idXCotAux && x.valor == valor_cuota72Aux && x.plazo == cuota72 &&
                    x.tasa == tasa_interesAux);
                if (esta6.Count() == 0)
                {
                    plan_matriz.plazo = cuota72;
                    plan_matriz.valor = Convert.ToDecimal(valor_cuota72, inter);
                    plan_matriz.cotizacion_id = int.Parse(idXCot);
                    plan_matriz.tasa = Convert.ToDecimal(tasa_interes, inter);
                    context.icb_plan_matriz.Add(plan_matriz);
                    result = context.SaveChanges();
                }
            }

            if (!string.IsNullOrEmpty(cuota84))
            {
                int valor_cuota84Aux = Convert.ToInt32(valor_cuota84);
                IQueryable<icb_plan_matriz> esta7 = context.icb_plan_matriz.Where(x =>
                    x.cotizacion_id == idXCotAux && x.valor == valor_cuota84Aux && x.plazo == cuota84 &&
                    x.tasa == tasa_interesAux);
                if (esta7.Count() == 0)
                {
                    plan_matriz.plazo = cuota84;
                    plan_matriz.valor = Convert.ToDecimal(valor_cuota84, inter);
                    plan_matriz.cotizacion_id = int.Parse(idXCot);
                    plan_matriz.tasa = Convert.ToDecimal(tasa_interes, inter);
                    context.icb_plan_matriz.Add(plan_matriz);
                    result = context.SaveChanges();
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult EliminarPlanDePago(int id_plan,int id_plazo,int id_cotizacion)
        {

            using (DbContextTransaction dbTran = context.Database.BeginTransaction()) {
                try
                {
                    icb_plan_matriz buscarPlanDePagoMatriz = context.icb_plan_matriz.Where(x => x.id == id_plazo).FirstOrDefault();
                    icb_plan_pago buscarPlanDePago = context.icb_plan_pago.Where(x => x.id == id_plan).FirstOrDefault();

                    var buscarPlanDePagoMatrizCot = context.icb_plan_matriz.Where(x => x.plan_pago_id == id_plan && x.cotizacion_id== id_cotizacion).ToList();

                    var count = buscarPlanDePagoMatrizCot.Count();

                    if (count>1)
                    {
                        var cuotas = buscarPlanDePago.cuotas;
                        var plazo = buscarPlanDePagoMatriz.plazo + ",";

                        cuotas = cuotas.Replace(plazo, "");
                        
                        if (buscarPlanDePago != null)
                        {
                            buscarPlanDePago.cuotas = cuotas;
                            context.SaveChanges();

                            if (buscarPlanDePagoMatriz != null)
                            {
                                context.Entry(buscarPlanDePagoMatriz).State = EntityState.Deleted;
                                int eliminar = context.SaveChanges();
                                if (eliminar > 0)
                                {
                                    dbTran.Commit();
                                    #region seguimiento
                                    var BuscarSeguimiento = context.Seguimientos.Where(x => x.EsManual == false && x.ModuloSeguimientos.Codigo == 6 && x.Codigo == 8).FirstOrDefault();

                                    vcotseguimiento seguimiento = new vcotseguimiento
                                    {
                                        cot_id = id_cotizacion,
                                        fecha = DateTime.Now,
                                        responsable = Convert.ToInt32(Session["user_usuarioid"]),
                                        Notas = "Elimino Plan de Pago ID: " + Convert.ToString(id_plan)+" Plazo: " + Convert.ToString(id_plazo),
                                        Motivo = null,
                                        fec_creacion = DateTime.Now,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                        estado = true,
                                        tipo_seguimiento = BuscarSeguimiento.Id
                                    };
                                    context.vcotseguimiento.Add(seguimiento);
                                    int guardarGeneral = context.SaveChanges();

                                    #endregion
                                    return Json(true, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                    }else if (count==1)
                    {
                        context.Entry(buscarPlanDePago).State = EntityState.Deleted;
                        int eliminarPlanDePago = context.SaveChanges();
                        if (eliminarPlanDePago > 0)
                        {
                            context.Entry(buscarPlanDePagoMatriz).State = EntityState.Deleted;
                            int eliminareliminarPlanDePagoMt = context.SaveChanges();
                            if (eliminareliminarPlanDePagoMt > 0)
                            {
                                dbTran.Commit();
                                #region seguimiento
                                var BuscarSeguimiento = context.Seguimientos.Where(x => x.EsManual == false && x.ModuloSeguimientos.Codigo == 6 && x.Codigo == 8).FirstOrDefault();
                         
                                    vcotseguimiento seguimiento = new vcotseguimiento
                                    {
                                        cot_id = id_cotizacion,
                                        fecha = DateTime.Now,
                                        responsable = Convert.ToInt32(Session["user_usuarioid"]),
                                        Notas = "Elimino Plan de Pago ID: " + Convert.ToString(id_plan) + " Plazo: " + Convert.ToString(id_plazo),
                                        Motivo = null,
                                        fec_creacion = DateTime.Now,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                        estado = true,
                                        tipo_seguimiento = BuscarSeguimiento.Id
                                    };
                                    context.vcotseguimiento.Add(seguimiento);
                                    int guardarGeneral = context.SaveChanges();
                                
                                #endregion
                                return Json(true, JsonRequestBehavior.AllowGet);
                            }
                        }

                    }
                    else
                    {
                        dbTran.Rollback();
                    }

                    return Json(false, JsonRequestBehavior.AllowGet);
                  
                }
                catch (Exception)
                {
                    dbTran.Rollback();
                    return Json(false, JsonRequestBehavior.AllowGet);
                  
                }           
            }
   
        }


        public JsonResult GetPlanDePago(int id) {

            var Plan = context.icb_plan_pago.Where(x=>x.id==id).FirstOrDefault();
            var LisPlanMatriz = context.icb_plan_matriz.Where(x=>x.id==id).ToList();
            var data = new
            {
                Plan,
                LisPlanMatriz
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult buscarCuidadesDepartamento(int id)
        {
            var ciudad = (from c in context.nom_ciudad
                          join d in context.nom_departamento
                              on c.dpto_id equals d.dpto_id
                          where d.dpto_id == id
                          select new
                          {
                              c.ciu_id,
                              c.ciu_nombre
                          }).ToList();
            var ciudades = ciudad.OrderBy(c => c.ciu_nombre);

            return Json(ciudades, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Seguimiento(int id, int? menu)
        {
            ViewBag.idSeguimiento = id;
            ViewBag.tpSeguimiento =
                new SelectList(context.Seguimientos.Where(x=>x.EsManual==true && x.Estado==true && x.ModuloSeguimientos.Codigo==6), "Id", "Evento");
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult ListMotivoanulacion(int Id) {

            var ListMotivoanulacion = context.MotivoSeguimientos.Where(x => x.Estado != false && x.CodigoSeguimiento == Id).Select(x => new
            {
              x.IdMotivoAnulacion,
              x.motivoanulacion.motivo
           }).ToList();

            return Json(ListMotivoanulacion, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AgregarNota(int id, int tpSeguimiento, int?  motivo, string nota)
        {
            vcotseguimiento nuevo = new vcotseguimiento
            {
                //cot_id = id,
                //fec_creacion = DateTime.Now,
                //fecha = DateTime.Now,
                //estado = true,
                //tipo_seguimiento = tpSeguimiento,
                //Motivo = motivo,
                //Notas = nota,
                //responsable = Convert.ToInt32(Session["user_usuarioid"]),
                //userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
            };
            context.vcotseguimiento.Add(nuevo);
            bool guardar = false;
            try
            {
                guardar = context.SaveChanges() > 0;
            }
            catch (DbEntityValidationException)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            if (guardar)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarSeguimientos(int id)
        {
            var data2 = context.vcotseguimiento.Where(x => x.cot_id == id).Select(b=> new{
                nombre_seguimiento= b.Seguimientos.Codigo +" * "+b.Seguimientos.Evento,
                b.fec_creacion,
                responsable=b.users.user_nombre,
                Motivo= b.motivoanulacion.id +" * "+b.motivoanulacion.motivo,
                b.Notas
               }).ToList();
            var data = data2.Select(d => new
            {
                fec_creacion=d.fec_creacion.ToString("yyyy/MM/dd",new CultureInfo("en-US")),
                d.responsable,
                d.Motivo,
                d.nombre_seguimiento,
                d.Notas
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EditarVehiculo(int idDetalle)
        {

            int rolActual = Convert.ToInt32(Session["user_rolid"]);
            var Permiso = (from a in context.rolacceso
                           join r in context.rolpermisos on a.idpermiso equals r.id
                           where r.codigo == "P37" && a.idrol == rolActual
                           select r.id);

            var permiso=Permiso != null ? "Si" : "No";


            var buscarDetalle = (from cotizacion in context.vcotdetallevehiculo
                                 join modelo in context.modelo_vehiculo
                                     on cotizacion.idmodelo equals modelo.modvh_codigo
                                 where cotizacion.detalle_id == idDetalle
                                 select new
                                 {
                                     cotizacion.detalle_id,
                                     modelo.mar_vh_id,
                                     cotizacion.idmodelo,
                                     cotizacion.anomodelo,
                                     cotizacion.color,
                                     cotizacion.nuevo,
                                     cotizacion.precio,//readonly
                                     poliza = cotizacion.poliza != null ? cotizacion.poliza : 0,
                                     matricula = cotizacion.matricula != null ? cotizacion.matricula : 0,
                                     soat = cotizacion.soat != null ? cotizacion.soat : 0,
                                     cotizacion.observacion
                                 }).FirstOrDefault();


            List<SelectListItem> listaAnios = new List<SelectListItem>();
            List<SelectListItem> listaModelos = new List<SelectListItem>();

            if (buscarDetalle != null)
            {
                if (buscarDetalle.nuevo ?? false)
                {
                    var buscarModelos = context.modelo_vehiculo.Where(x => x.mar_vh_id == buscarDetalle.mar_vh_id)
                        .Select(x => new
                        {
                            x.modvh_codigo,
                            x.modvh_nombre
                        });
                    foreach (var modelo in buscarModelos)
                    {
                        listaModelos.Add(new SelectListItem { Value = modelo.modvh_codigo, Text = modelo.modvh_nombre });
                    }

                    var buscarAnios = context.anio_modelo.Where(x => x.codigo_modelo == buscarDetalle.idmodelo).Select(
                        x => new
                        {
                            x.anio,
                            x.anio_modelo_id
                        }).Distinct().ToList();
                    foreach (var anios in buscarAnios)
                    {
                        listaAnios.Add(new SelectListItem
                        { Value = anios.anio_modelo_id.ToString(), Text = anios.anio.ToString() });
                    }
                }
                else
                {
                    DateTime fechaActual = DateTime.Now;
                    var buscarModelosUsados = context.vw_referencias_total.Where(x =>
                        x.marcvh_id == buscarDetalle.mar_vh_id && x.ano == fechaActual.Year &&
                        x.mes == fechaActual.Month).Select(x => new
                        {
                            modvh_codigo = x.modvh_id,
                            x.modvh_nombre
                        }).Distinct().ToList();

                    foreach (var modelo in buscarModelosUsados)
                    {
                        listaModelos.Add(new SelectListItem { Value = modelo.modvh_codigo, Text = modelo.modvh_nombre });
                    }

                    var anios = context.vw_referencias_total.Where(x =>
                            x.modvh_id == buscarDetalle.idmodelo && x.ano == fechaActual.Year &&
                            x.mes == fechaActual.Month).Select(x => new  {anios = x.anio_vh }).Distinct().ToList();

                    foreach (var anio in anios)
                    {
                        listaAnios.Add(new SelectListItem
                        { Value = anio.anios.ToString(), Text = anio.anios.ToString() });
                    }
                }

                var data = new
                {
                    permiso,
                    respuesta = true,
                    buscarDetalle.detalle_id,
                    esNuevo = buscarDetalle.nuevo ?? false,
                    //buscarMarcas,
                    //buscarColores,
                    listaModelos,
                    listaAnios,
                    buscarDetalle.poliza,
                    buscarDetalle.matricula,
                    buscarDetalle.soat,
                    buscarDetalle.precio,
                    buscarDetalle.observacion,
                    marcaElegida = buscarDetalle.mar_vh_id,
                    modeloElegido = buscarDetalle.idmodelo,
                    anioElegido = buscarDetalle.anomodelo,
                    colorElegido = buscarDetalle.color
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(new { respuesta = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CalcularTotalCotizacion(int idCotizacion)
        {
            List<DetalleCotizacionModel> listaAccesorios = new List<DetalleCotizacionModel>();
            List<DetalleCotizacionModel> listaVehiculos = new List<DetalleCotizacionModel>();
            List<DetalleCotizacionModel> listaRetomas = new List<DetalleCotizacionModel>();
            decimal totalCotizado = 0;
            // Consulta a la tabla de cotizacion de accesorios o repuestos (vcotrepuestos) para poder luego listarlos

            List<vcotretoma> buscarRetomas = (from cotRetoma in context.vcotretoma
                                              where cotRetoma.idcot == idCotizacion
                                              select cotRetoma).ToList();
            foreach (vcotretoma retomaVh in buscarRetomas)
            {
                listaRetomas.Add(new DetalleCotizacionModel
                {
                    descripcion = retomaVh.modelo,
                    valor = retomaVh.valor,
                    anioVhretoma = retomaVh.ano,
                    kilometrajeretoma = retomaVh.Kilometraje ?? 0,
                    placaretoma = retomaVh.placa,
                    id_detalle = retomaVh.id,

                });
            }

            var buscarAccesorios = (from cotRepuesto in context.vcotrepuestos
                                    join referencia in context.icb_referencia
                                        on cotRepuesto.referencia equals referencia.ref_codigo
                                    where cotRepuesto.cot_id == idCotizacion
                                    select new
                                    {
                                        cotRepuesto.id,
                                        cotRepuesto.referencia,
                                        referencia.ref_descripcion,
                                        cotRepuesto.vrunitario,
                                        cotRepuesto.cantidad,
                                        valor_total = cotRepuesto.vrunitario * cotRepuesto.cantidad
                                    }).ToList();
            foreach (var accesorios in buscarAccesorios)
            {
                listaAccesorios.Add(new DetalleCotizacionModel
                {
                    id_detalle = accesorios.id,
                    descripcion = accesorios.ref_descripcion,
                    valor = accesorios.vrunitario ?? 0,
                    cantidad = accesorios.cantidad ?? 0,
                    codigo_referencia = accesorios.referencia,
                    valor_total = accesorios.valor_total ?? 0
                });
                totalCotizado += (accesorios.vrunitario ?? 0) * accesorios.cantidad ?? 0;
            }

            // Consulta a la tabla de cotizacion de vehiculos (vcotdetallevehiculo) para poder luego listarlos
            var buscarModelos = (from cotVehiculo in context.vcotdetallevehiculo
                                 join modelos in context.modelo_vehiculo
                                     on cotVehiculo.idmodelo equals modelos.modvh_codigo
                                 where cotVehiculo.idcotizacion == idCotizacion
                                 select new
                                 {
                                     cotVehiculo.detalle_id,
                                     cotVehiculo.idcotizacion,
                                     modelos.modvh_nombre,
                                     cotVehiculo.precio,
                                     cotVehiculo.poliza,
                                     cotVehiculo.matricula,
                                     cotVehiculo.soat,
                                     modelos.modvh_codigo
                                     //cotVehiculo.vrretoma,
                                     //cotVehiculo.modeloretoma
                                 }).ToList();
            foreach (var vehiculo in buscarModelos)
            {
                listaVehiculos.Add(new DetalleCotizacionModel
                {
                    id_detalle = vehiculo.detalle_id,
                    num_cotizacion = vehiculo.idcotizacion,
                    descripcion = vehiculo.modvh_nombre,
                    valor = vehiculo.precio ?? 0,
                    poliza = vehiculo.poliza ?? 0,
                    matricula = vehiculo.matricula ?? 0,
                    soat = vehiculo.soat ?? 0,
                    codigo_modelovh = vehiculo.modvh_codigo
                    //vrretoma = vehiculo.vrretoma ?? 0,
                    //modeloretoma = vehiculo.modeloretoma
                });

                totalCotizado += vehiculo.precio ?? 0;
            }

            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
            var data = new
            {
                listaVehiculos,
                listaAccesorios,
                listaRetomas,
                totalCotizado = totalCotizado.ToString("0,0", elGR)
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ModelosVehiculoCotizacion(int idCotizacion)
        {
            List<DetalleCotizacionModel> listaVehiculos = new List<DetalleCotizacionModel>();

            // Consulta a la tabla de cotizacion de vehiculos (vcotdetallevehiculo) para poder luego listarlos
            List<DetalleCotizacionModel> buscarModelos = (from cotVehiculo in context.vcotdetallevehiculo
                                 join modelos in context.modelo_vehiculo
                                     on cotVehiculo.idmodelo equals modelos.modvh_codigo
                                 where cotVehiculo.idcotizacion == idCotizacion
                                 select new DetalleCotizacionModel
                                 {
                                     id_detalle=cotVehiculo.detalle_id,
                                     num_cotizacion=cotVehiculo.idcotizacion,
                                     descripcion=modelos.modvh_nombre,
                                     codigo_modelovh=modelos.modvh_codigo
                                 }).ToList();
            listaVehiculos = buscarModelos;

            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
            var data = new
            {
                listaVehiculos
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarAccesorioCotizado(int idDetalle)
        {
            var buscaAccesorio = context.vcotrepuestos.Where(d => d.id == idDetalle).FirstOrDefault();

            //var buscaAccesorio = context.vcotrepuestos.FirstOrDefault(x=>x.id==idDetalle);
            if (buscaAccesorio != null)
            {
                return Json(new
                {
                    encontrado = true,
                    detalleCotizado = idDetalle,
                    idAccesorio = buscaAccesorio.referencia,
                    valorUnitario = buscaAccesorio.vrunitario,
                    descripcion = buscaAccesorio.icb_referencia.ref_descripcion,
                    buscaAccesorio.cantidad
                });
            }

            return Json(new { encontrado = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarMarcasVehiculos()
        {
            var buscarMarcas = context.marca_vehiculo.Select(x => new
            {
                x.marcvh_id,
                x.marcvh_nombre
            }).ToList();

            return Json(buscarMarcas, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarColoresModelos()
        {
            var buscarColores = context.color_vehiculo.Select(x => new
            {
                x.colvh_id,
                x.colvh_nombre
            }).ToList();

            return Json(buscarColores, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Listas()
        {
            var marca = from m in context.marca_vehiculo
                        select new
                        {
                            id = m.marcvh_id,
                            nombre = m.marcvh_nombre
                        };

            var data = new
            {
                marca
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarModelosPorMarca(int? idMarca, bool esNuevo)
        {
            if (esNuevo)
            {
                var buscarModelos = context.modelo_vehiculo.Where(x => x.mar_vh_id == idMarca && x.modvh_estado)
                    .OrderBy(x => x.modvh_nombre).ToList().Select(x => new
                    {
                        x.modvh_codigo,
                        x.modvh_nombre
                    });

                return Json(buscarModelos, JsonRequestBehavior.AllowGet);
            }

            DateTime fechaActual = DateTime.Now;
            var buscarModelosUsados = context.vw_referencias_total.Where(x =>
                    x.marcvh_id == idMarca && x.ano == fechaActual.Year && x.mes == fechaActual.Month &&
                    x.usado == true)
                .Select(x => new
                {
                    modvh_codigo = x.modvh_id,
                    x.modvh_nombre
                    //x.colvh_nombre,
                    //x.colvh_id
                }).Distinct();

            return Json(buscarModelosUsados, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarAniosModelos(string codigoModelo, bool esNuevo)
        {
            if (esNuevo)
            {
                List<anio_modelo> anios = context.anio_modelo.Where(x => x.codigo_modelo == codigoModelo).ToList();
                var result = anios.Select(x => new
                {
                    anios = x.anio,
                    codigo = x.anio_modelo_id
                });

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                DateTime fechaActual = DateTime.Now;
                var anios = context.vw_referencias_total.Where(x =>
                    x.modvh_id == codigoModelo && x.ano == fechaActual.Year && x.mes == fechaActual.Month).Select(x =>
                    new
                    {
                        anios = x.anio_vh,
                        x.codigo
                    }).Distinct();

                return Json(anios, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult BuscarColoresNuevos()
        {
            var buscarColores = context.color_vehiculo.Select(x => new
            {
                x.colvh_id,
                x.colvh_nombre
            }).ToList();

            return Json(buscarColores, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPreciosPorAnio(string codigoModelo, int anioModelo, bool esNuevo)
        {
            if (esNuevo)
            {
                int rolActual = Convert.ToInt32(Session["user_rolid"]);
                rolacceso buscarAccesoAlValor =
                    context.rolacceso.FirstOrDefault(x => x.idrol == rolActual && x.idpermiso == 12);
                bool puedeActualizar = buscarAccesoAlValor != null ? true : false;

                int anioModeloAux = Convert.ToInt32(anioModelo);
                anio_modelo buscaPrecio = context.anio_modelo.FirstOrDefault(x =>
                    x.codigo_modelo == codigoModelo && x.anio_modelo_id == anioModeloAux);
                vlistanuevos precioVh = context.vlistanuevos.Where(x => x.anomodelo == anioModeloAux).OrderByDescending(x => x.ano).ThenByDescending(x => x.mes).FirstOrDefault();
                var buscaColor = (from color in context.color_vehiculo select new { color.colvh_id, color.colvh_nombre })
                    .ToList();
                var result = new
                {
                    buscaPrecio.descripcion,
                    valor = precioVh != null ? (precioVh.precioespecial != null ? precioVh.precioespecial.ToString() : "0") : "0",
                    poliza = buscaPrecio.poliza != null ? buscaPrecio.poliza.ToString() : "",
                    matricula = buscaPrecio.matricula != null ? buscaPrecio.matricula.ToString() : "",
                    esNuevo = true,
                    buscaColor,
                    puedeActualizar
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var buscaColor = (from referencias in context.vw_referencias_total
                                  where referencias.codigo == codigoModelo && referencias.anio_vh == anioModelo &&
                                        referencias.usado == true
                                  select new
                                  {
                                      referencias.colvh_id,
                                      referencias.colvh_nombre
                                  }).ToList();
                //var buscaColor = context.vw_referencias_total.Where(x=>x.codigo==anioModelo).ToList();
                var result = new
                {
                    buscaColor,
                    esNuevo = false
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult BuscarCotizacionesPaginadas2(DateTime? inicio, DateTime? fin)
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

            var predicate = PredicateBuilder.True<vw_cotizacion>();
            var predicate2 = PredicateBuilder.False<vw_cotizacion>();

            int skip = 0;
            if (pagina == 0)
            {
                skip = 0;
            }
            else
            {
                skip = pagina;
            }
            if (inicio == null)
            {
                icb_cotizacion buscarFechaInicial = context.icb_cotizacion.OrderBy(x => x.cot_feccreacion).FirstOrDefault();
                inicio = buscarFechaInicial != null ? buscarFechaInicial.cot_feccreacion : DateTime.Now;
            }
            if (fin == null)
            {
                icb_cotizacion buscarFechaFinal =
                    context.icb_cotizacion.OrderByDescending(x => x.cot_feccreacion).FirstOrDefault();
                fin = buscarFechaFinal != null ? buscarFechaFinal.cot_feccreacion.AddDays(1) : DateTime.Now.AddDays(1);
            }
            else
            {
                fin = fin.Value.AddDays(1);
            }
            
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            int bodegaActual = 0;

            List<int> listabodegas = new List<int>();
            //Validamos que el usuario loguado tenga el rol y el permiso para ver o no las cotizaciones de otras bodegas
            int permiso = (from u in context.users
                           join r in context.rols
                               on u.rol_id equals r.rol_id
                           join ra in context.rolacceso
                               on r.rol_id equals ra.idrol
                           where u.user_id == usuario && ra.idpermiso == 23
                           select new
                           {
                               u.user_id,
                               u.rol_id,
                               r.rol_nombre,
                               ra.idpermiso
                           }).Count();
            if (permiso == 0)
            {
                bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                listabodegas.Add(bodegaActual);
            }
            else
            {
                listabodegas = context.bodega_usuario.Where(x => x.id_usuario == usuario).Select(x => x.id_bodega)
                    .ToList();
            }

            //hacer la consulta filtrada por asesor si es rol asesor solo para que el asesor vea lo que le corresponde
            int rol = Convert.ToInt32(Session["user_rolid"]);
            if (rol == 4 || rol == 2030)
            {
                predicate = predicate.And(d => d.cot_usucreacion == usuario);
            }
            predicate = predicate.And(d => d.cot_feccreacion >= inicio);
            predicate = predicate.And(d => d.cot_feccreacion <= fin);
            predicate = predicate.And(d => 1 == 1 && listabodegas.Contains(d.id_bodega.Value));
            int registrostotales = context.vw_cotizacion.Where(predicate).Count();
            if (pageSize == -1)
            {
                pageSize = registrostotales;
            }

            if (sortColumnDir == "asc")
            {
                List<vw_cotizacion> query2 = context.vw_cotizacion.Where(predicate)
                    .OrderBy(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
                var query = query2.Select(d => new
                {
                    d.cot_idserial,
                    d.documento,
                    d.nombrecompleto,
                    d.celular_tercero,
                    d.email_tercero,
                    d.numero_cotizacion,
                    d.fecha2,
                    d.tporigen_nombre,
                    d.subfuente,
                    idTipoultimoSeg = context.vcotseguimiento.OrderByDescending(x => x.id)
                                                  .FirstOrDefault(x => x.cot_id == d.cot_idserial).tipo_seguimiento,
                    fechaUltimoSeg =
                                                  context.vcotseguimiento.OrderByDescending(x => x.id)
                                                      .FirstOrDefault(x => x.cot_id == d.cot_idserial).fec_creacion != null
                                                      ? context.vcotseguimiento.OrderByDescending(x => x.id)
                                                          .FirstOrDefault(x => x.cot_id == d.cot_idserial).fec_creacion.ToString()
                                                      : "",
                    desTipoUltimoSeg =
                                                  context.vtiposeguimientocot.FirstOrDefault(x =>
                                                          x.id_tipo_seguimiento == context.vcotseguimiento.OrderByDescending(y => y.id)
                                                              .FirstOrDefault(y => y.cot_id == d.cot_idserial).tipo_seguimiento)
                                                      .nombre_seguimiento != null
                                                      ? context.vtiposeguimientocot.FirstOrDefault(x =>
                                                              x.id_tipo_seguimiento == context.vcotseguimiento.OrderByDescending(y => y.id)
                                                                  .FirstOrDefault(y => y.cot_id == d.cot_idserial).tipo_seguimiento)
                                                          .nombre_seguimiento
                                                      : "",
                    notaultimoSeg =
                                                  context.vcotseguimiento.OrderByDescending(x => x.id)
                                                      .FirstOrDefault(x => x.cot_id == d.cot_idserial).Notas != null
                                                      ? context.vcotseguimiento.OrderByDescending(x => x.id)
                                                          .FirstOrDefault(x => x.cot_id == d.cot_idserial).Notas
                                                      : "",
                    d.asesor,
                }).ToList();

                int contador = query.Count();
                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<vw_cotizacion> query2 = context.vw_cotizacion.Where(predicate)
                      .OrderByDescending(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
                var query = query2.Select(d => new
                {
                    d.cot_idserial,
                    d.documento,
                    d.nombrecompleto,
                    d.celular_tercero,
                    d.email_tercero,
                    d.numero_cotizacion,
                    d.fecha2,
                    d.tporigen_nombre,
                    d.subfuente,
                    idTipoultimoSeg = context.vcotseguimiento.OrderByDescending(x => x.id)
                                                  .FirstOrDefault(x => x.cot_id == d.cot_idserial).tipo_seguimiento,
                    fechaUltimoSeg =
                                                  context.vcotseguimiento.OrderByDescending(x => x.id)
                                                      .FirstOrDefault(x => x.cot_id == d.cot_idserial).fec_creacion != null
                                                      ? context.vcotseguimiento.OrderByDescending(x => x.id)
                                                          .FirstOrDefault(x => x.cot_id == d.cot_idserial).fec_creacion.ToString()
                                                      : "",
                    desTipoUltimoSeg =
                                                  context.Seguimientos.FirstOrDefault(x =>
                                                          x.Id == context.vcotseguimiento.OrderByDescending(y => y.id)
                                                              .FirstOrDefault(y => y.cot_id == d.cot_idserial).tipo_seguimiento)
                                                       != null
                                                      ? context.Seguimientos.FirstOrDefault(x =>
                                                              x.Id == context.vcotseguimiento.OrderByDescending(y => y.id)
                                                                  .FirstOrDefault(y => y.cot_id == d.cot_idserial).tipo_seguimiento)
                                                          .Evento
                                                      : "",
                    notaultimoSeg =
                                                  context.vcotseguimiento.OrderByDescending(x => x.id)
                                                      .FirstOrDefault(x => x.cot_id == d.cot_idserial).Notas != null
                                                      ? context.vcotseguimiento.OrderByDescending(x => x.id)
                                                          .FirstOrDefault(x => x.cot_id == d.cot_idserial).Notas
                                                      : "",
                    d.asesor,
                }).ToList();
                int contador = query.Count();
                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }
           
        }


        public JsonResult BuscarCotizacionesPaginadas(DateTime? inicio, DateTime? fin)
        {
            if (inicio == null)
            {
                icb_cotizacion buscarFechaInicial = context.icb_cotizacion.OrderBy(x => x.cot_feccreacion).FirstOrDefault();
                inicio = buscarFechaInicial != null ? buscarFechaInicial.cot_feccreacion : DateTime.Now;
            }

            if (fin == null)
            {
                icb_cotizacion buscarFechaFinal =
                    context.icb_cotizacion.OrderByDescending(x => x.cot_feccreacion).FirstOrDefault();
                fin = buscarFechaFinal != null ? buscarFechaFinal.cot_feccreacion.AddDays(1) : DateTime.Now.AddDays(1);
            }
            else
            {
                fin = fin.Value.AddDays(1);
            }

            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            int bodegaActual = 0;

            List<int> listabodegas = new List<int>();
            //Validamos que el usuario loguado tenga el rol y el permiso para ver o no las cotizaciones de otras bodegas
            int permiso = (from u in context.users
                           join r in context.rols
                               on u.rol_id equals r.rol_id
                           join ra in context.rolacceso
                               on r.rol_id equals ra.idrol
                           where u.user_id == usuario && ra.idpermiso == 23
                           select new
                           {
                               u.user_id,
                               u.rol_id,
                               r.rol_nombre,
                               ra.idpermiso
                           }).Count();
            if (permiso == 0)
            {
                bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                listabodegas.Add(bodegaActual);
            }
            else
            {
                listabodegas = context.bodega_usuario.Where(x => x.id_usuario == usuario).Select(x => x.id_bodega)
                    .ToList();
            }

            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

            //hacer la consulta filtrada por asesor si es rol asesor solo para que el asesor vea lo que le corresponde
            int rol = Convert.ToInt32(Session["user_rolid"]);
            if (rol == 4 || rol == 2030)
            {
                var buscaCotizacionSQL = (from tercero in context.icb_terceros
                                          join cotizacion in context.icb_cotizacion
                                              on tercero.tercero_id equals cotizacion.id_tercero
                                          join user in context.users
                                              on cotizacion.asesor equals user.user_id
                                          join tpOrigen in context.tp_origen
                                              on tercero.origen_id equals tpOrigen.tporigen_id into ps1
                                          from tpOrigen in ps1.DefaultIfEmpty()
                                          join tpSubFuente in context.tp_subfuente
                                              on tercero.subfuente equals tpSubFuente.id into ps2
                                          from tpSubFuente in ps2.DefaultIfEmpty()
                                          where listabodegas.Contains(cotizacion.bodegaid.Value) && cotizacion.cot_feccreacion >= inicio &&
                                                cotizacion.cot_feccreacion <= fin && cotizacion.cot_usucreacion == usuario
                                          select new
                                          {
                                              tercero.doc_tercero,
                                              nombre = tercero.razon_social + tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                       tercero.apellido_tercero + " " + tercero.segapellido_tercero,
                                              tercero.celular_tercero,
                                              tercero.email_tercero,
                                              cotizacion.cot_numcotizacion,
                                              cotizacion.cot_idserial,
                                              cot_feccreacion = cotizacion.cot_feccreacion.Year + "/" + cotizacion.cot_feccreacion.Month +
                                                                "/" + cotizacion.cot_feccreacion.Day,
                                              fuente = tpOrigen.tporigen_nombre != null ? tpOrigen.tporigen_nombre : "",
                                              subfuente = tpSubFuente.subfuente != null ? tpSubFuente.subfuente : "",
                                              idTipoultimoSeg = context.vcotseguimiento.OrderByDescending(x => x.id)
                                                  .FirstOrDefault(x => x.cot_id == cotizacion.cot_idserial).tipo_seguimiento,
                                              fechaUltimoSeg =
                                                  context.vcotseguimiento.OrderByDescending(x => x.id)
                                                      .FirstOrDefault(x => x.cot_id == cotizacion.cot_idserial).fec_creacion != null
                                                      ? context.vcotseguimiento.OrderByDescending(x => x.id)
                                                          .FirstOrDefault(x => x.cot_id == cotizacion.cot_idserial).fec_creacion.ToString()
                                                      : "",
                                              desTipoUltimoSeg =
                                                  context.Seguimientos.FirstOrDefault(x =>
                                                          x.Id == context.vcotseguimiento.OrderByDescending(y => y.id)
                                                              .FirstOrDefault(y => y.cot_id == cotizacion.cot_idserial).tipo_seguimiento)
                                                       != null
                                                      ? context.Seguimientos.FirstOrDefault(x =>
                                                              x.Id == context.vcotseguimiento.OrderByDescending(y => y.id)
                                                                  .FirstOrDefault(y => y.cot_id == cotizacion.cot_idserial).tipo_seguimiento)
                                                          .Evento
                                                      : "",
                                              notaultimoSeg =
                                                  context.vcotseguimiento.OrderByDescending(x => x.id)
                                                      .FirstOrDefault(x => x.cot_id == cotizacion.cot_idserial).Notas != null
                                                      ? context.vcotseguimiento.OrderByDescending(x => x.id)
                                                          .FirstOrDefault(x => x.cot_id == cotizacion.cot_idserial).Notas
                                                      : "",
                                              asesor = user.user_nombre + " " + user.user_apellido
                                          }).OrderByDescending(x => x.fechaUltimoSeg).ToList();

                return Json(new { buscaCotizacionSQL, rol }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var buscaCotizacionSQL = (from tercero in context.icb_terceros
                                          join cotizacion in context.icb_cotizacion
                                              on tercero.tercero_id equals cotizacion.id_tercero
                                          join user in context.users
                                              on cotizacion.asesor equals user.user_id
                                          join tpOrigen in context.tp_origen
                                              on tercero.origen_id equals tpOrigen.tporigen_id into ps1
                                          from tpOrigen in ps1.DefaultIfEmpty()
                                          join tpSubFuente in context.tp_subfuente
                                              on tercero.subfuente equals tpSubFuente.id into ps2
                                          from tpSubFuente in ps2.DefaultIfEmpty()
                                          where listabodegas.Contains(cotizacion.bodegaid.Value) && cotizacion.cot_feccreacion >= inicio &&
                                                cotizacion.cot_feccreacion <= fin
                                          select new
                                          {
                                              tercero.doc_tercero,
                                              nombre = tercero.razon_social + tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                       tercero.apellido_tercero + " " + tercero.segapellido_tercero,
                                              tercero.celular_tercero,
                                              tercero.email_tercero,
                                              cotizacion.cot_numcotizacion,
                                              cotizacion.cot_idserial,
                                              cot_feccreacion = cotizacion.cot_feccreacion.Year + "/" + cotizacion.cot_feccreacion.Month +
                                                                "/" + cotizacion.cot_feccreacion.Day,
                                              fuente = tpOrigen.tporigen_nombre != null ? tpOrigen.tporigen_nombre : "",
                                              subfuente = tpSubFuente.subfuente != null ? tpSubFuente.subfuente : "",
                                              idTipoultimoSeg = context.vcotseguimiento.OrderByDescending(x => x.id)
                                                  .FirstOrDefault(x => x.cot_id == cotizacion.cot_idserial).tipo_seguimiento,
                                              fechaUltimoSeg =
                                                  context.vcotseguimiento.OrderByDescending(x => x.id)
                                                      .FirstOrDefault(x => x.cot_id == cotizacion.cot_idserial).fec_creacion != null
                                                      ? context.vcotseguimiento.OrderByDescending(x => x.id)
                                                          .FirstOrDefault(x => x.cot_id == cotizacion.cot_idserial).fec_creacion.ToString()
                                                      : "",
                                              desTipoUltimoSeg =
                                                  context.Seguimientos.FirstOrDefault(x =>
                                                          x.Id == context.vcotseguimiento.OrderByDescending(y => y.id)
                                                              .FirstOrDefault(y => y.cot_id == cotizacion.cot_idserial).tipo_seguimiento)
                                                       != null
                                                      ? context.Seguimientos.FirstOrDefault(x =>
                                                              x.Id == context.vcotseguimiento.OrderByDescending(y => y.id)
                                                                  .FirstOrDefault(y => y.cot_id == cotizacion.cot_idserial).tipo_seguimiento)
                                                          .Evento
                                                      : "",
                                              notaultimoSeg =
                                                  context.vcotseguimiento.OrderByDescending(x => x.id)
                                                      .FirstOrDefault(x => x.cot_id == cotizacion.cot_idserial).Notas != null
                                                      ? context.vcotseguimiento.OrderByDescending(x => x.id)
                                                          .FirstOrDefault(x => x.cot_id == cotizacion.cot_idserial).Notas
                                                      : "",
                                          }).OrderByDescending(x => x.fechaUltimoSeg).ToList();

                return Json(new { buscaCotizacionSQL, rol }, JsonRequestBehavior.AllowGet);
            }
        }

        public string tipoSeguimiento(int? id)
        {
            var resultado = "";
            if (id != null)
            {
                var coti = context.icb_cotizacion.Where(d => d.cot_idserial == id).FirstOrDefault();
                if (coti != null)
                {

                }
            }


            return resultado;
        }

        public JsonResult BuscarPlanes(string modelo)
        {
            var planes = (from p in context.icb_plan_financiero
                          join d in context.icb_detalle_plan
                              on p.plan_id equals d.detplan_plan_id
                          where d.detplan_modelo_vehiculo == modelo
                          select new
                          {
                              p.plan_id,
                              p.plan_nombre
                          }).ToList();

            var financiera = (from f in context.icb_unidad_financiera
                              select new
                              {
                                  f.financiera_id,
                                  f.financiera_nombre
                              }
                ).ToList();

            var data = new
            {
                planes,
                financiera
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPlazos(int planid)
        {
            var plazos = (from p in context.icb_plazo
                          join pm in context.icb_plan_matriz
                              on p.plazo_plan_id equals pm.plan_pago_id
                          where p.plazo_plan_id == planid
                          select new
                          {
                              p.plazo_id,
                              p.plazo_tiempo,
                              p.plazo_plan_id,
                              pm.plan_pago_id
                          }).ToList();

            var data = new
            {
                plazos
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPlanesPagoCotizacion(int xcotizacionid, string modelo, int id_detalle)
        {
            var data = (from plpg in context.icb_plan_pago
                        join plmat in context.icb_plan_matriz//inner
                            on plpg.id equals plmat.plan_pago_id
                        join plan in context.icb_plan_financiero//inner
                            on plpg.plan_id equals plan.plan_id
                        join detalle in context.vcotdetallevehiculo//inner
                            on plpg.cotizacion_id equals detalle.idcotizacion
                        where plpg.cotizacion_id == xcotizacionid
                              && plpg.modelo == modelo && detalle.detalle_id == id_detalle
                        select new
                        {
                            id_plan =plpg.id,
                            id_plazo= plmat.id,
                            plpg.credito,
                            plpg.tasa_interes,
                            plmat.plazo,
                            plmat.valor,
                            plan.plan_nombre,
                            plpg.valor_residual,
                            plpg.otros_valores
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarSiExiste(int xcotizacionid, decimal xci, string xint, string xcuo)
        {
            //var data = (from plpg in context.icb_plan_pago
            //            join plmat in context.icb_plan_matriz
            //            on plpg.id equals plmat.plan_pago_id
            //            where plpg.cotizacion_id == xcotizacionid
            //            select new
            //            {
            //                plpg.credito,
            //                plpg.tasa_interes,
            //                plmat.plazo,
            //                plmat.valor
            //            }).ToList();


            //var buscarCuotaExiste = data.FirstOrDefault(x => x.credito == xci && x.tasa_interes == xint && x.plazo == xcuo );
            //if (buscarCuotaExiste != null)
            //{
            //    var si = true;
            //    return Json(si, JsonRequestBehavior.AllowGet);

            //}
            //else
            //{
            bool si = false;

            return Json(si, JsonRequestBehavior.AllowGet);
            //}
        }

        public JsonResult BuscarPlanPago(int planid)
        {
            var planpago = from pp in context.icb_plan_pago
                           join pl in context.icb_plan_financiero
                               on pp.plan_id equals pl.plan_id
                           join dp in context.icb_detalle_plan
                               on pl.plan_id equals dp.detplan_plan_id
                           join c in context.icb_plazo
                               on pl.plan_id equals c.plazo_plan_id
                           where pp.plan_id == planid
                           select new
                           {
                               pp.id,
                               pl.plan_nombre
                           };

            string plazos = "";

            var data = new
            {
                planpago,
                plazos
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult crearPDF(int? cotizacion)
        {
            var buscar = (from a in context.vw_cotizacion
                          where cotizacion == a.cot_idserial
                          select new
                          {
                              id=a.cot_idserial,
                              a.tercero_id,
                              a.cot_feccreacion,
                              a.cot_numcotizacion,
                              a.asesor,
                              a.telefeno_asesor,
                              a.correo_asesor,
                              a.prinom_tercero,
                              a.segnom_tercero,
                              a.apellido_tercero,
                              a.segapellido_tercero,
                              a.telf_tercero,
                              a.celular_tercero,
                              a.email_tercero,
                              a.direccion,
                              a.documento,
                              a.dv,
                              a.razon_social,
                              a.observacion
                          }).FirstOrDefault();


            string root = Server.MapPath("~/Pdf/");
            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
            string path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);
            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

            PDFmodel obj = new PDFmodel
            {idcotizacion=buscar.id,
            idtercero=buscar.tercero_id,
            Idtercero=buscar.tercero_id.ToString(),
                fechaCreacion = buscar.cot_feccreacion.ToString("yyyy/MM/dd"),
                numeroCotizacion = Convert.ToString(buscar.cot_numcotizacion),
                nombreAsesor = buscar.asesor,
                telefono_asesor = buscar.telefeno_asesor,
                correo_asesor = buscar.correo_asesor,
                nombreCompletoTercero = buscar.prinom_tercero + ' ' + buscar.segnom_tercero + ' ' +
                                        buscar.apellido_tercero + ' ' + buscar.segapellido_tercero + ' ' +
                                        buscar.razon_social,
                numeroDocumento = buscar.documento,
                digitoVerificacion = buscar.dv != null ? "-" + buscar.dv : "",
                telefono_cliente = buscar.telf_tercero,
                celular_cliente = buscar.celular_tercero,
                direccion_cliente = buscar.direccion,
                correo_cliente = buscar.email_tercero,
                notasCotizacion = buscar.observacion != null ? buscar.observacion : "Sin notas",
                referencias = (from b in context.vw_cotizacion
                               where cotizacion == b.cot_idserial
                               select new referenciasPDF
                               {
                                   vehiculo = b.vehiculo,
                                   precio = b.precio,
                                   matricula = b.matricula != null ? b.matricula : 0,
                                   seguro = b.poliza != null ? b.poliza : 0,
                                   soat = b.soat ?? 0,
                                   planesDePago = (from x in context.icb_plan_pago
                                                   where x.id == b.id
                                                   select new planesDePago
                                                   {
                                                       nombrePlan = x.icb_plan_financiero.plan_nombre,
                                                       cuotaInicial = b.cuota_inicial,
                                                       financiacion = b.precioFinanciacion,
                                                       tasa = b.tasa_interes,
                                                       plazosYcoutasCotizacion = (from y in context.icb_plan_matriz
                                                                                  where b.id == y.plan_pago_id
                                                                                  select new plazosYcoutas
                                                                                  {
                                                                                      plazo = y.plazo,
                                                                                      valor = y.valor
                                                                                  }).ToList()
                                                   }).ToList()
                               }).ToList()
            };

            ViewAsPdf something = new ViewAsPdf("crearPDF", obj);
            return something;
        }

        public JsonResult buscarMotivos()
        {
            var buscar = (from a in context.motivo_anulacion
                          select new
                          {
                              a.id,
                              a.motivo
                          }).ToList();

            return Json(buscar, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(int? id, int? menu, int? motivo, string descripcion, int tercero, int numeroCot)
        {
            icb_cotizacion cotizar = context.icb_cotizacion.Find(id);
            cotizar.anulado = true;
            cotizar.idmotivo = motivo;
            //db.vpedido.Remove(vpedido);
            context.Entry(cotizar).State = EntityState.Modified;

            //string parametro = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P73").syspar_value;
            var BuscarSeguimientoCOT = context.Seguimientos.Where(x => x.EsManual == false && x.ModuloSeguimientos.Codigo == 6 && x.Codigo == 26).FirstOrDefault();
            vcotseguimiento seguimiento = new vcotseguimiento
            {
                cot_id = Convert.ToInt32(id),
                fecha = DateTime.Now,
                fec_creacion = DateTime.Now,
                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                responsable = Convert.ToInt32(Session["user_usuarioid"]),
                Notas = descripcion,
                estado = true,
                tipo_seguimiento = BuscarSeguimientoCOT.Id
            };
            context.vcotseguimiento.Add(seguimiento);
            var BuscarSeguimientoTER = context.Seguimientos.Where(x => x.EsManual == false && x.ModuloSeguimientos.Codigo == 6 && x.Codigo == 12).FirstOrDefault();
            seguimientotercero segTercero = new seguimientotercero
            {
                idtercero = tercero,
                tipo = BuscarSeguimientoTER.Id,
                nota = "No compro cotizacion " + numeroCot + " motivo: " + descripcion,
                fecha = DateTime.Now,
                userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
            };
            context.seguimientotercero.Add(segTercero);

            context.SaveChanges();

            TempData["mensaje"] = "cotizacion no comprada Correctamente";
            return RedirectToAction("ver", new { id, menu });
        }

        public JsonResult buscaranuladas(int id)
        {
            bool buscar = context.icb_cotizacion.Where(x => x.cot_idserial == id).Select(x => x.anulado)
                .FirstOrDefault();
            if (buscar)
            {
                return Json(new { data = true }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { data = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarVehiculo(int? id)
        {
            icb_cotizacion buscar = context.vcotdetallevehiculo.Where(x => x.idcotizacion == id).Select(x => x.icb_cotizacion)
                .FirstOrDefault();
            int data = 0;
            if (buscar != null)
            {
                data = 1;
            }
            else
            {
                data = 0;
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarVHinteresado(string tercero)
        {
            string buscarModelo = "";
            var buscarTercero = context.icb_terceros.OrderByDescending(x => x.tercero_id).FirstOrDefault(x => x.doc_tercero == tercero);
            if (buscarTercero != null)
            {
                prospectos buscarProspecto = context.prospectos.OrderByDescending(x => x.id).FirstOrDefault(x => x.idtercero == buscarTercero.tercero_id);
                if (buscarProspecto != null)
                {
                    buscarModelo= buscarProspecto.modelo_vehiculo.modvh_nombre != null ? buscarProspecto.modelo_vehiculo.modvh_nombre : "";             
                }
                
            }          
            return Json(buscarModelo, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarInfoActivacion(int tipo)
        {
            //el tipo me informara si la activacion la esta haciendo un asesor o es desde el contac center y de acuerdo a eso listare la informacion en el dropdown del modal
            if (tipo == 1) // voy a activar asesor entonces cargo las bodegas
            {
                var data = context.bodega_concesionario.Where(x => x.bodccs_estado).OrderBy(x => x.bodccs_nombre)
                    .ToList().Select(x => new
                    {
                        x.id,
                        nombre = x.bodccs_nombre
                    });

                return Json(new { info = data, tipo }, JsonRequestBehavior.AllowGet);
            }

            if (tipo == 2) // voy a activar call center
            {
                var data = context.users.Where(x => x.rol_id == 2029 && x.user_estado).OrderBy(x => x.user_nombre)
                    .ToList().Select(x => new
                    {
                        id = x.user_id,
                        nombre = x.user_nombre + " " + x.user_apellido
                    });

                return Json(new { info = data, tipo }, JsonRequestBehavior.AllowGet);
            }

            return Json(JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarAsesorBodega(int idBodega)
        {
            var data = (from u in context.users
                        join b in context.bodega_usuario
                            on u.user_id equals b.id_usuario
                        where (u.rol_id == 4 || u.rol_id == 2030) && u.user_estado && b.id_bodega == idBodega
                        select new
                        {
                            id = u.user_id,
                            nombre = u.user_nombre + " " + u.user_apellido
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult guardarActivacion(int idCot, int usuario, string nota)
        {
            tareasasignadas tarea = new tareasasignadas
            {
                idcotizacion = idCot,
                fec_creacion = DateTime.Now,
                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                estado = true,
                idusuarioasignado = usuario,
                notas = nota
            };
            context.tareasasignadas.Add(tarea);
            int result = context.SaveChanges();

            if (result > 0)
            {
                return Json(new { exito = true }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { exito = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ParametrizarCotizacionDigital(int? menu)
        {
            BuscarFavoritos(menu);
            ViewBag.modelo =
                new SelectList(
                    context.modelo_vehiculo.OrderBy(x => x.modvh_nombre).Where(x => x.modvh_estado && x.mar_vh_id == 1),
                    "modvh_codigo", "modvh_nombre");
            return View();
        }

        [HttpPost]
        public ActionResult ParametrizarCotizacionDigital(pcotizaciondigital formulario,
            HttpPostedFileBase imgPrincipal, HttpPostedFileBase imgDetalle1, HttpPostedFileBase imgDetalle2,
            HttpPostedFileBase imgDetalle3, int? menu)
        {
            string pathP = "";
            string pathD1 = "";
            string pathD2 = "";
            string pathD3 = "";
            int error = 0;

            if (imgPrincipal == null || imgPrincipal.ContentLength == 0 || imgDetalle1 == null ||
                imgDetalle1.ContentLength == 0 || imgDetalle2 == null || imgDetalle2.ContentLength == 0 ||
                imgDetalle3 == null || imgDetalle3.ContentLength == 0)
            {
                TempData["mensaje_error"] = "El archivo esta vacio o no es un archivo valido!";
                BuscarFavoritos(menu);

                return RedirectToAction("ParametrizarCotizacionDigital");
            }

            int existe = context.pcotizaciondigital
                .Where(d => d.anioModelo == formulario.anioModelo && d.modelo == formulario.modelo).Count();
            if (existe == 0)
            {
                if (CheckFileType2(imgPrincipal.FileName) == false)
                {
                    error = 1;
                }

                if (CheckFileType2(imgDetalle1.FileName) == false)
                {
                    error = 1;
                }

                if (CheckFileType2(imgDetalle2.FileName) == false)
                {
                    error = 1;
                }

                if (CheckFileType2(imgDetalle3.FileName) == false)
                {
                    error = 1;
                }

                if (error == 1)
                {
                    TempData["mensaje_error"] =
                        "La extension del archivo no es permitida, se permite .PNG, .JPEG, .JPG, por favor valide!";
                }
                else
                {
                    pathP = Server.MapPath("~/Images/imgCotizacionDigital/" + imgPrincipal.FileName);
                    pathD1 = Server.MapPath("~/Images/imgCotizacionDigital/" + imgDetalle1.FileName);
                    pathD2 = Server.MapPath("~/Images/imgCotizacionDigital/" + imgDetalle2.FileName);
                    pathD3 = Server.MapPath("~/Images/imgCotizacionDigital/" + imgDetalle3.FileName);
                    //Validacion para cuando el archivo esta en uso y no puede ser usado desde visual
                    try
                    {
                        if (System.IO.File.Exists(pathP))
                        {
                            System.IO.File.Delete(pathP);
                        }

                        if (System.IO.File.Exists(pathD1))
                        {
                            System.IO.File.Delete(pathD1);
                        }

                        if (System.IO.File.Exists(pathD2))
                        {
                            System.IO.File.Delete(pathD2);
                        }

                        if (System.IO.File.Exists(pathD3))
                        {
                            System.IO.File.Delete(pathD3);
                        }

                        imgPrincipal.SaveAs(pathP);
                        imgDetalle1.SaveAs(pathD1);
                        imgDetalle2.SaveAs(pathD2);
                        imgDetalle3.SaveAs(pathD3);
                    }
                    catch (IOException)
                    {
                        TempData["mensaje_error"] =
                            "El archivo esta siendo usado por otro proceso, asegurece de cerrarlo o cree una copia del archivo e intente de nuevo!";
                        BuscarFavoritos(menu);
                        ViewBag.modelo =
                            new SelectList(
                                context.modelo_vehiculo.OrderBy(x => x.modvh_nombre)
                                    .Where(x => x.modvh_estado && x.mar_vh_id == 1), "modvh_codigo", "modvh_nombre",
                                formulario.modelo);

                        var aniosmodelo2 = context.anio_modelo.Where(d => d.codigo_modelo == formulario.modelo)
                            .Select(d => new { id = d.anio_modelo_id, nombre = d.anio }).ToList();
                        ViewBag.anioModelo = new SelectList(aniosmodelo2, "id", "nombre", formulario.anioModelo);
                        BuscarFavoritos(menu);

                        return RedirectToAction("ParametrizarCotizacionDigital");
                    }

                    pcotizaciondigital nuevo = new pcotizaciondigital
                    {
                        modelo = formulario.modelo,
                        anioModelo = formulario.anioModelo,
                        imgPrincipal = "/Images/imgCotizacionDigital/" + imgPrincipal.FileName,
                        imgDetalle1 = "/Images/imgCotizacionDigital/" + imgDetalle1.FileName,
                        imgDetalle2 = "/Images/imgCotizacionDigital/" + imgDetalle2.FileName,
                        imgDetalle3 = "/Images/imgCotizacionDigital/" + imgDetalle3.FileName,
                        texto1 = formulario.texto1,
                        pieFoto = formulario.pieFoto,
                        tituloDet1 = formulario.tituloDet1,
                        palabraResaltada1 = formulario.palabraResaltada1,
                        tituloDet2 = formulario.tituloDet2,
                        palabraResaltada2 = formulario.palabraResaltada2,
                        tituloDet3 = formulario.tituloDet3,
                        palabraResaltada3 = formulario.palabraResaltada3,
                        tituloCuerpo1 = formulario.tituloCuerpo1,
                        tituloCuerpo2 = formulario.tituloCuerpo2,
                        tituloCuerpo3 = formulario.tituloCuerpo3,
                        chevyStar = formulario.chevyStar,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        fecha_creacion = DateTime.Now
                    };
                    context.pcotizaciondigital.Add(nuevo);
                    context.SaveChanges();
                }

                BuscarFavoritos(menu);
                ViewBag.modelo =
                    new SelectList(
                        context.modelo_vehiculo.OrderBy(x => x.modvh_nombre)
                            .Where(x => x.modvh_estado && x.mar_vh_id == 1), "modvh_codigo", "modvh_nombre",
                        formulario.modelo);

                var aniosmodelo = context.anio_modelo.Where(d => d.codigo_modelo == formulario.modelo)
                    .Select(d => new { id = d.anio_modelo_id, nombre = d.anio }).ToList();
                ViewBag.anioModelo = new SelectList(aniosmodelo, "id", "nombre", formulario.anioModelo);
                BuscarFavoritos(menu);
                TempData["mensaje"] =
                   "Parametrización finalizada Exitosamente";

                return RedirectToAction("ParametrizarCotizacionDigital", new { menu = menu });
            }
            else
            {
                TempData["mensaje_error"] =
                    "El modelo y año seleccionados ya están parametrizados en otro registro. Por favor seleccione otro modelo y/o otro año";
                ViewBag.modelo =
                    new SelectList(
                        context.modelo_vehiculo.OrderBy(x => x.modvh_nombre)
                            .Where(x => x.modvh_estado && x.mar_vh_id == 1), "modvh_codigo", "modvh_nombre",
                        formulario.modelo);

                var aniosmodelo = context.anio_modelo.Where(d => d.codigo_modelo == formulario.modelo)
                    .Select(d => new { id = d.anio_modelo_id, nombre = d.anio }).ToList();
                ViewBag.anioModelo = new SelectList(aniosmodelo, "id", "nombre", formulario.anioModelo);
                BuscarFavoritos(menu);

                return View(formulario);
            }
        }

        public JsonResult BuscarCotizacionesParametrizadas()
        {
            var data = (from p in context.pcotizaciondigital
                        join m in context.modelo_vehiculo
                            on p.modelo equals m.modvh_codigo
                        join a in context.anio_modelo
                            on p.anioModelo equals a.anio_modelo_id
                        select new
                        {
                            p.id,
                            m.modvh_nombre,
                            a.anio
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //Se crear metodo de validación de base de datos para front
        public JsonResult FrontCarYear(string modelcar, int year)
        {
            int respuesta = 1;
            int data = (from p in context.pcotizaciondigital
                        where p.modelo == modelcar && p.anioModelo == year
                        select new
                        {
                            p.anioModelo
                        }).Count();
            if (data > 0)
            {
                respuesta = 0;
            }

            return Json(respuesta);
        }

        [HttpGet]
        public ActionResult ActParCotDig(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            pcotizaciondigital cotDigital = context.pcotizaciondigital.Find(id);
            if (cotDigital == null)
            {
                return HttpNotFound();
            }
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(cotDigital.userid_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(cotDigital.userid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
            }

            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 64);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
            BuscarFavoritos(menu);
            ViewBag.anioModelo = cotDigital.anioModelo;
            ViewBag.chevyStar = cotDigital.chevyStar;
            ViewBag.imgPrincipalOK = cotDigital.imgPrincipal;
            ViewBag.imgDetalle1OK = cotDigital.imgDetalle1;
            ViewBag.imgDetalle2OK = cotDigital.imgDetalle2;
            ViewBag.imgDetalle3OK = cotDigital.imgDetalle3;
            ViewBag.imgPrincipal = Request.Url.Scheme + "://" + Request.Url.Authority + cotDigital.imgPrincipal;
            ViewBag.imgDetalle1 = Request.Url.Scheme + "://" + Request.Url.Authority + cotDigital.imgDetalle1;
            ViewBag.imgDetalle2 = Request.Url.Scheme + "://" + Request.Url.Authority + cotDigital.imgDetalle2;
            ViewBag.imgDetalle3 = Request.Url.Scheme + "://" + Request.Url.Authority + cotDigital.imgDetalle3;
            ViewBag.modelo =
                new SelectList(
                    context.modelo_vehiculo.OrderBy(x => x.modvh_nombre).Where(x => x.modvh_estado && x.mar_vh_id == 1),
                    "modvh_codigo", "modvh_nombre");

            return View(cotDigital);
        }

        [HttpPost]
        public ActionResult ActParCotDig(pcotizaciondigital formulario, HttpPostedFileBase imgPrincipal,
            HttpPostedFileBase imgDetalle1, HttpPostedFileBase imgDetalle2, HttpPostedFileBase imgDetalle3, int? menu)
        {
            string pathP = "";
            string pathD1 = "";
            string pathD2 = "";
            string pathD3 = "";
            int error = 0;

            #region validamos si los objetos de las imagenes que vienen del parametro tienen algo, y si es asi verificamos que sean archivos validos

            if (imgPrincipal != null /*|| imgPrincipal.ContentLength == 0*/)
            {
                if (CheckFileType2(imgPrincipal.FileName) == false)
                {
                    error = 1;
                }
                else
                {
                    pathP = Server.MapPath("~/Images/imgCotizacionDigital/" + imgPrincipal.FileName);
                }
            }

            if (imgDetalle1 != null /*|| imgDetalle1.ContentLength == 0*/)
            {
                if (CheckFileType2(imgDetalle1.FileName) == false)
                {
                    error = 1;
                }
                else
                {
                    pathD1 = Server.MapPath("~/Images/imgCotizacionDigital/" + imgDetalle1.FileName);
                }
            }

            if (imgDetalle2 != null /*|| imgDetalle2.ContentLength == 0*/)
            {
                if (CheckFileType2(imgDetalle2.FileName) == false)
                {
                    error = 1;
                }
                else
                {
                    pathD2 = Server.MapPath("~/Images/imgCotizacionDigital/" + imgDetalle2.FileName);
                }
            }

            if (imgDetalle3 != null /*|| imgDetalle3.ContentLength == 0*/)
            {
                if (CheckFileType2(imgDetalle3.FileName) == false)
                {
                    error = 1;
                }
                else
                {
                    pathD3 = Server.MapPath("~/Images/imgCotizacionDigital/" + imgDetalle3.FileName);
                }
            }

            #endregion

            if (error == 1)
            {
                TempData["mensaje_error"] =
                    "La extension del archivo no es permitida, se permite .PNG, .JPEG, .JPG, por favor valide!";
            }
            else
            {
                //Validacion para cuando el archivo esta en uso y no puede ser usado desde visual
                try
                {
                    if (System.IO.File.Exists(pathP))
                    {
                        System.IO.File.Delete(pathP);
                    }

                    if (System.IO.File.Exists(pathD1))
                    {
                        System.IO.File.Delete(pathD1);
                    }

                    if (System.IO.File.Exists(pathD2))
                    {
                        System.IO.File.Delete(pathD2);
                    }

                    if (System.IO.File.Exists(pathD3))
                    {
                        System.IO.File.Delete(pathD3);
                    }
                }
                catch (IOException)
                {
                    TempData["mensaje_error"] =
                        "El archivo esta siendo usado por otro proceso, asegurece de cerrarlo o cree una copia del archivo e intente de nuevo!";
                    BuscarFavoritos(menu);
                    ViewBag.modelo =
                        new SelectList(
                            context.modelo_vehiculo.OrderBy(x => x.modvh_nombre)
                                .Where(x => x.modvh_estado && x.mar_vh_id == 1), "modvh_codigo", "modvh_nombre");
                    return RedirectToAction("ActParCotDig", new { formulario.id, menu });
                }

                pcotizaciondigital actualiza = context.pcotizaciondigital.FirstOrDefault(x => x.id == formulario.id);
                if (actualiza != null)
                {
                    actualiza.modelo = formulario.modelo;
                    actualiza.anioModelo = formulario.anioModelo;
                    if (pathP != "")
                    {
                        actualiza.imgPrincipal = "/Images/imgCotizacionDigital/" + imgPrincipal.FileName;
                        imgPrincipal.SaveAs(pathP);
                    }
                    else
                    {
                        actualiza.imgPrincipal = Request["imgPrincipalOK"];
                    }

                    if (pathD1 != "")
                    {
                        actualiza.imgDetalle1 = "/Images/imgCotizacionDigital/" + imgDetalle1.FileName;
                        imgDetalle1.SaveAs(pathD1);
                    }
                    else
                    {
                        actualiza.imgDetalle1 = Request["imgDetalle1OK"];
                    }

                    if (pathD2 != "")
                    {
                        actualiza.imgDetalle2 = "/Images/imgCotizacionDigital/" + imgDetalle2.FileName;
                        imgDetalle2.SaveAs(pathD2);
                    }
                    else
                    {
                        actualiza.imgDetalle2 = Request["imgDetalle2OK"];
                    }

                    if (pathD3 != "")
                    {
                        actualiza.imgDetalle3 = "/Images/imgCotizacionDigital/" + imgDetalle3.FileName;
                        imgDetalle3.SaveAs(pathD3);
                    }
                    else
                    {
                        actualiza.imgDetalle3 = Request["imgDetalle3OK"];
                    }

                    actualiza.texto1 = formulario.texto1;
                    actualiza.pieFoto = formulario.pieFoto;
                    actualiza.tituloDet1 = formulario.tituloDet1;
                    actualiza.palabraResaltada1 = formulario.palabraResaltada1;
                    actualiza.tituloDet2 = formulario.tituloDet2;
                    actualiza.palabraResaltada2 = formulario.palabraResaltada2;
                    actualiza.tituloDet3 = formulario.tituloDet3;
                    actualiza.palabraResaltada3 = formulario.palabraResaltada3;
                    actualiza.tituloCuerpo1 = formulario.tituloCuerpo1;
                    actualiza.tituloCuerpo2 = formulario.tituloCuerpo2;
                    actualiza.tituloCuerpo3 = formulario.tituloCuerpo3;
                    actualiza.chevyStar = formulario.chevyStar;
                    actualiza.userid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    actualiza.fecha_actualizacion = DateTime.Now;
                    context.Entry(actualiza).State = EntityState.Modified;
                    context.SaveChanges();
                    TempData["mensaje"] = "Registro actualizado con exito!";
                }
            }

            BuscarFavoritos(menu);
            ViewBag.modelo =
                new SelectList(
                    context.modelo_vehiculo.OrderBy(x => x.modvh_nombre).Where(x => x.modvh_estado && x.mar_vh_id == 1),
                    "modvh_codigo", "modvh_nombre");
            ViewBag.anioModelo = formulario.anioModelo;
            ViewBag.chevyStar = formulario.chevyStar;
            ViewBag.imgPrincipal = Request.Url.Scheme + "://" + Request.Url.Authority + formulario.imgPrincipal;
            ViewBag.imgDetalle1 = Request.Url.Scheme + "://" + Request.Url.Authority + formulario.imgDetalle1;
            ViewBag.imgDetalle2 = Request.Url.Scheme + "://" + Request.Url.Authority + formulario.imgDetalle2;
            ViewBag.imgDetalle3 = Request.Url.Scheme + "://" + Request.Url.Authority + formulario.imgDetalle3;
            ViewBag.modelo =
                new SelectList(
                    context.modelo_vehiculo.OrderBy(x => x.modvh_nombre).Where(x => x.modvh_estado && x.mar_vh_id == 1),
                    "modvh_codigo", "modvh_nombre");
            return RedirectToAction("ActParCotDig", new { formulario.id, menu });
        }

        private bool CheckFileType2(string fileName)
        {
            string ext = Path.GetExtension(fileName);
            switch (ext.ToLower())
            {
                case ".jpg":
                    return true;
                case ".png":
                    return true;
                case ".jpeg":
                    return true;
                default:
                    return false;
            }
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