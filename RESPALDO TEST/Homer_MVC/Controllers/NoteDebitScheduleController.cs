using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class NoteDebitScheduleController : Controller
    {
        /// <summary>
        ///     Este es el controlador para el modal de nota debito, en este controlador contiene las acciones de manera similar
        ///     del controlador de Nota de debito, cabe resaltar que los nombres de las variables están en inglés todo esto para
        ///     fomentar las buenas pr+acticas de programación, en este controlador
        ///     puede apreciar que se maneja dos métodos que serán los que estructuren los datos y el envio de datos para la vista
        ///     de Note debit schedule ( agenda nota debito) en que se realiza un llenado de datos en la forma clave-valor
        /// </summary>
        private readonly Iceberg_Context context = new Iceberg_Context();

        private readonly int CreateLic = 0;
        private readonly int ExecutionLic = 0;

        //private readonly int nIdOperation = 0;

        public void enrollOperation()
        {
            // Id de nota de debito 
            icb_sysparameter vparam = context.icb_sysparameter.Where(d => d.syspar_cod == "P95").FirstOrDefault();
            int debitNote = vparam != null ? Convert.ToInt32(vparam.syspar_value) : 3038;
        }

        public void listas(NoteDebitScheduleModel modelo)
        {
            icb_sysparameter vparam = context.icb_sysparameter.Where(d => d.syspar_cod == "P95").FirstOrDefault();
            int debitNote = vparam != null ? Convert.ToInt32(vparam.syspar_value) : 3038;
            var buscarTipoDocumento = (from tipoDocumento in context.tp_doc_registros
                                       select new
                                       {
                                           tipoDocumento.sw,
                                           tipoDocumento.tpdoc_id,
                                           nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                           tipoDocumento.tipo
                                       }).ToList();
            ViewBag.tipo_documentoFiltro =
                new SelectList(buscarTipoDocumento.Where(x => x.sw == 3), "tpdoc_id", "nombre");

            ViewBag.tipo = new SelectList(context.tp_doc_registros.Where(x => x.tipo == 21), "tpdoc_id",
                "tpdoc_nombre");

            var users = from u in context.users
                        select new
                        {
                            idTercero = u.user_id,
                            nombre = u.user_nombre,
                            apellidos = u.user_apellido,
                            u.user_numIdent
                        };
            List<SelectListItem> itemsU = new List<SelectListItem>();
            foreach (var item in users)
            {
                string nombre = "(" + item.user_numIdent + ") - " + item.nombre + " " + item.apellidos;
                itemsU.Add(new SelectListItem { Text = nombre, Value = item.idTercero.ToString() });
            }

            ViewBag.vendedor = itemsU;


            ViewBag.moneda = new SelectList(context.monedas, "moneda", "descripcion");
            ViewBag.tasa = new SelectList(context.moneda_conversion, "id", "conversion");
            ViewBag.motivocompra = new SelectList(context.motcompra, "id", "Motivo");

            ViewBag.condicion_pago = context.fpago_tercero;

            var provedores = from pro in context.tercero_cliente
                             join ter in context.icb_terceros
                                 on pro.tercero_id equals ter.tercero_id
                             select new
                             {
                                 idTercero = ter.tercero_id,
                                 nombreTErcero = ter.prinom_tercero,
                                 apellidosTercero = ter.apellido_tercero,
                                 razonSocial = ter.razon_social,
                                 ter.doc_tercero
                             };
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in provedores)
            {
                string nombre = item.doc_tercero + " - " + item.nombreTErcero + " " + item.apellidosTercero + " " +
                             item.razonSocial;
                items.Add(new SelectListItem { Text = nombre, Value = item.idTercero.ToString() });
            }

            ViewBag.nit = items;

            encab_documento buscarSerialUltimaNota = context.encab_documento.OrderByDescending(x => x.idencabezado)
                .Where(d => d.tipo == debitNote).FirstOrDefault();
            ViewBag.numNotaCreado = buscarSerialUltimaNota != null ? buscarSerialUltimaNota.numero : 0;
            //busco el perfil contable del documento y si está disponible para esa bodega
            var perfilcon = context.perfil_contable_bodega.Where(d => d.idbodega == modelo.IdLarder && d.perfil_contable_documento.tipo == debitNote).FirstOrDefault();
            modelo.Document = perfilcon != null ? perfilcon.perfil_contable_documento.tp_doc_registros.tpdoc_nombre : "";
            modelo.typeDocument = perfilcon != null ? perfilcon.perfil_contable_documento.tp_doc_registros.tpdoc_id : 0;
            modelo.CountableProfile = perfilcon != null ? perfilcon.perfil_contable_documento.id : 0;
            modelo.CountablenNameProfile = perfilcon != null
                ? perfilcon.perfil_contable_documento.descripcion
                : "";
            modelo.ReceiptValue = 0;
            modelo.Value = 0;

            /*Tipo Cartera*/
            var listC = (from t in context.Tipos_Cartera
                         select new
                         {
                             t.id,
                             nombre = t.descripcion
                         }).ToList();

            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in listC)
            {
                lista.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }
            ViewBag.cartera = lista;

        }

        [HttpGet]
        public ActionResult DebitNoteModal(int idn)
        {
            var idTramitador = (from t in context.tramitador_vh
                                select new
                                {
                                    value = t.tramitador_id
                                }).FirstOrDefault();
            var tramitador = (from t in context.tramitador_vh
                              select new
                              {
                                  value = t.tramitador_id,
                                  text = "(" + t.tramitador_documento + ") " + t.tramitadorpri_nombre + " " + t.tramitadorseg_nombre +
                                         " " + t.tramitador_apellidos + " " + t.tramitador_apellido2
                              }).ToList();
            vw_pendientesAlistamiento x = context.vw_pendientesAlistamiento.FirstOrDefault(t => t.id == idn);
            NoteDebitScheduleModel note = new NoteDebitScheduleModel
            {
                IdDocument = Convert.ToInt64(x.idCliente),
                NameClient = x.cliente,
                IdLarder = x.bodega,
                NameLarder = x.bodccs_nombre,
                IdNit = Convert.ToInt64(x.doc_tercero),
                IdAgent = Convert.ToInt32(x.idAsesor),
                NameAgent = x.asesor,
                tramitador = idTramitador.value,
                orderNumber = x.numero,
                IdOrder = x.id
            };
            ViewBag.tramitador = new SelectList(tramitador, "value", "text");

            note.Total = note.Value;

            icb_bahia_alistamiento bhals = context.icb_bahia_alistamiento.Where(t =>
                t.id_pedido == idn && (t.tencabezaorden.estadoorden == CreateLic ||
                                       t.tencabezaorden.estadoorden == ExecutionLic)).FirstOrDefault();
            note.estadoMatrc = false;
            if (bhals != null)
            {
                note.Id = bhals.bh_als_id;
                note.estadoMatrc = true;
                note.pregisDateLic = bhals.bh_als_fecha != null ? bhals.bh_als_fecha.Value.ToString("dd/MM/yyyy") : "";
                note.motive = bhals.tp_movimiento;
            }

            //Se crean viewbags para la ejecución del modal de la vista, hay un viewbag de crear matricula que proviene de estado orden, seguidamente un viewbag la ejecución matricula y por último un viewbag de estado de vehiculo
            ViewBag.CreateLic = CreateLic;
            ViewBag.ExecutionLic = ExecutionLic;
            ViewBag.idn = idn;
            ViewBag.vehicleState = context.icb_bahia_alistamiento
                .Where(t => t.id_pedido == idn && (t.tencabezaorden.estadoorden == CreateLic ||
                                                   t.tencabezaorden.estadoorden == ExecutionLic))
                .Select(t => t.tencabezaorden.estadoorden).FirstOrDefault();
            //parametro de nota bebito a cliente cartera
            icb_sysparameter vparam = context.icb_sysparameter.Where(d => d.syspar_cod == "P95").FirstOrDefault();
            int debitNote = vparam != null ? Convert.ToInt32(vparam.syspar_value) : 3038;
            //note.
            //Verifico si el vehiculo seleccionado es el que corresponde dentro de pendientes de alistamiento
            vw_pendientesAlistamiento enrolAlistexist = context.vw_pendientesAlistamiento.Where(d => d.planmayor == x.planmayor)
                .FirstOrDefault();

            //Se trae los vehiculos que están en pendientes matricula
            listas(note);

            return PartialView("noteDebitModal", note);
        }

        [HttpPost]
        public ActionResult crearMatricula(int id,int tramitador_id, int cartera, string placa, int valor, string fechamatricula, string observacion, encab_documento encabezado)/*cartera,tramitador*/
        {
            placaVh(id, placa, fechamatricula);
            string mensaje = "";
            int iduser = Convert.ToInt32(Session["user_usuarioid"]);
            icb_tpeventos tpevento = context.icb_tpeventos.Where(a => a.tpevento_nombre == "Matriculado").FirstOrDefault();
            vw_pendientesAlistamiento x = context.vw_pendientesAlistamiento.FirstOrDefault(t => t.id == id);
            icb_terceros tercero = context.icb_terceros.Where(b => b.doc_tercero == x.doc_tercero).FirstOrDefault();
            icb_vehiculo vh = context.icb_vehiculo.Where(c => c.plan_mayor == x.plan_mayor).FirstOrDefault();
            icb_vehiculo_eventos ve = new icb_vehiculo_eventos
            {
                planmayor = x.planmayor,
                eventofec_creacion = DateTime.Now,
                eventouserid_creacion = iduser,
                evento_nombre = "Matriculado",
                evento_estado = true,
                bodega_id = x.bodega,
                id_tpevento = tpevento.tpevento_id,
                fechaevento = DateTime.Now,
                terceroid = tercero.tercero_id,
                placa = placa,
                vin = vh.vin,
                evento_observacion = observacion,
                cartera_id= cartera,/*cartera*/
                
            };
            if (encabezado.idencabezado != 0)
            {
                ve.idencabezado = encabezado.idencabezado;
                encabezado.id_pedido_vehiculo = id;
                vh.tramitador_id= tramitador_id;/*tramitador*/
            }
            context.icb_vehiculo_eventos.Add(ve);

            try
            {
                context.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                mensaje = e.Message;
            }

            return Json(valor, JsonRequestBehavior.AllowGet);
        }

        public void placaVh(int id, string placa, string fechamatricula)
        {
            string mensaje = "";
            DateTime fechaM = DateTime.Now;
            vw_pendientesAlistamiento x = context.vw_pendientesAlistamiento.FirstOrDefault(t => t.id == id);
            int iduser = Convert.ToInt32(Session["user_usuarioid"]);
            icb_vehiculo vehiculo = context.icb_vehiculo.Where(d => d.plan_mayor == x.planmayor).FirstOrDefault();
            vehiculo.icbvhfec_actualizacion = DateTime.Now;
            vehiculo.icbvhuserid_actualizacion = iduser;
            vehiculo.plac_vh = placa;
            bool convertir = DateTime.TryParse(fechamatricula, out fechaM);
            if (convertir)
            {
                vehiculo.fecmatricula = fechaM;
            }
            else
            {
                vehiculo.fecmatricula = DateTime.Now;
            }

            context.icb_vehiculo.Attach(vehiculo);
            context.Entry(vehiculo).Property(z => z.icbvhfec_actualizacion).IsModified = true;
            context.Entry(vehiculo).Property(z => z.icbvhuserid_actualizacion).IsModified = true;
            context.Entry(vehiculo).Property(z => z.plac_vh).IsModified = true;
            context.Entry(vehiculo).Property(z => z.fecmatricula).IsModified = true;

            try
            {
                context.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                mensaje = e.Message;
            }
        }

        [HttpPost]
        public ActionResult crearMatriculaND(int id,string placa, string observacion, int valor, string fechamatricula,
            int tipodoc, string bodega, string nit, string asesor, string perfilc,int tramitador_id, int cartera)
        {
            int response = 0;
            int idbodega = context.bodega_concesionario.Where(x => x.bodccs_nombre == bodega).Select(z => z.id)
                .FirstOrDefault();
            int tpdocumento = context.tp_doc_registros.Where(x => x.tpdoc_id == tipodoc).Select(z => z.tpdoc_id)
                .FirstOrDefault();
            int terceroid = context.icb_terceros.Where(x => x.doc_tercero == nit).Select(z => z.tercero_id)
                .FirstOrDefault();
            int? idasesor = context.vw_pendientesEntrega.Where(x => x.asesor == asesor).Select(z => z.idAsesor)
                .FirstOrDefault();
            int idperfilc = context.perfil_contable_documento.Where(x => x.tipo == tpdocumento).Select(z => z.id)
                .FirstOrDefault();

            
            if (tramitador_id == 0 && cartera==0)
            {
                tramitador_id = Convert.ToInt32(Session["user_usuarioid"]);
            }


            if (ModelState.IsValid)
            {
                //consecutivo
                grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == tpdocumento && x.bodega_id == idbodega);
                if (grupo != null)
                {
                    DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                    long consecutivo = doc.BuscarConsecutivo(grupo.grupo);

                    //Encabezado documento
                    encab_documento encabezado = new encab_documento
                    {
                        tipo = tpdocumento,
                        numero = consecutivo,
                        nit = terceroid,
                        fecha = DateTime.Now,
                        valor_total = Convert.ToDecimal(valor),
                        iva = Convert.ToDecimal(0),
                        retencion = Convert.ToDecimal(0),
                        retencion_ica = Convert.ToDecimal(0),
                        retencion_iva = Convert.ToDecimal(0),
                        vendedor = idasesor,
                        documento = nit,
                        valor_mercancia = Convert.ToDecimal(valor),
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        fec_creacion = DateTime.Now,
                        porcen_retencion = float.Parse("0", CultureInfo.InvariantCulture),
                        porcen_reteiva = float.Parse("0", CultureInfo.InvariantCulture),
                        porcen_retica = float.Parse("0", CultureInfo.InvariantCulture),
                        perfilcontable = idperfilc,
                        bodega = idbodega,
                        id_pedido_vehiculo = id
                    };
                    context.encab_documento.Add(encabezado);

                    //movimiento contable
                    //buscamos en perfil cuenta documento, por medio del perfil seleccionado
                    var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                                      join nombreParametro in context.paramcontablenombres
                                                          on perfil.id_nombre_parametro equals nombreParametro.id
                                                      join cuenta in context.cuenta_puc
                                                          on perfil.cuenta equals cuenta.cntpuc_id
                                                      where perfil.id_perfil == idperfilc
                                                      select new
                                                      {
                                                          perfil.id,
                                                          perfil.id_nombre_parametro,
                                                          perfil.cuenta,
                                                          perfil.centro,
                                                          perfil.id_perfil,
                                                          nombreParametro.descripcion_parametro,
                                                          cuenta.cntpuc_numero
                                                      }).ToList();
                    int secuencia = 1;
                    decimal totalDebitos = 0;
                    decimal totalCreditos = 0;

                    List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();
                    List<cuentas_valores> ids_cuentas_valores = new List<cuentas_valores>();
                    centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                    int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
                    foreach (var parametro in parametrosCuentasVerificar)
                    {
                        string descripcionParametro = context.paramcontablenombres
                            .FirstOrDefault(x => x.id == parametro.id_nombre_parametro).descripcion_parametro;
                        cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);

                        if (buscarCuenta != null)
                        {
                            if (parametro.id_nombre_parametro == 2 && Convert.ToDecimal(0) != 0
                                || parametro.id_nombre_parametro == 3 && Convert.ToDecimal(0) != 0
                                || parametro.id_nombre_parametro == 4 && Convert.ToDecimal(0) != 0
                                || parametro.id_nombre_parametro == 5 && Convert.ToDecimal(0) != 0
                                || parametro.id_nombre_parametro == 10
                                || parametro.id_nombre_parametro == 11)
                            {
                                mov_contable movNuevo = new mov_contable
                                {
                                    id_encab = 0,
                                    seq = secuencia,
                                    idparametronombre = parametro.id_nombre_parametro,
                                    cuenta = parametro.cuenta,
                                    centro = parametro.centro,
                                    fec = DateTime.Now,
                                    fec_creacion = DateTime.Now,
                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                                };
                                //            movNuevo.detalle = ndm.nota1;

                                cuenta_puc info = context.cuenta_puc.Where(i => i.cntpuc_id == parametro.cuenta)
                                    .FirstOrDefault();

                                if (info.tercero)
                                {
                                    movNuevo.nit = terceroid;
                                }
                                else
                                {
                                    icb_terceros tercero = context.icb_terceros.Where(t => t.doc_tercero == "0")
                                        .FirstOrDefault();
                                    movNuevo.nit = tercero.tercero_id;
                                }

                                // las siguientes validaciones se hacen dependiendo de la cuenta que esta moviendo la nota credito, para guardar la informacion acorde
                                if (parametro.id_nombre_parametro == 10)
                                {
                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(valor);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = encabezado.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(valor);

                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(valor);
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(valor);
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(valor);
                                    }
                                }

                                if (parametro.id_nombre_parametro == 11)
                                {
                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(valor);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = encabezado.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = Convert.ToDecimal(valor);
                                        movNuevo.debito = 0;

                                        movNuevo.creditoniif = Convert.ToDecimal(valor);
                                        movNuevo.debitoniif = 0;
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = Convert.ToDecimal(valor);
                                        movNuevo.debitoniif = 0;
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = Convert.ToDecimal(valor);
                                        movNuevo.debito = 0;
                                    }
                                }

                                if (parametro.id_nombre_parametro == 2)
                                {
                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(valor);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = encabezado.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = Convert.ToDecimal(encabezado.iva);
                                        movNuevo.debito = 0;
                                        movNuevo.creditoniif = Convert.ToDecimal(encabezado.iva);
                                        movNuevo.debitoniif = 0;
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = Convert.ToDecimal(encabezado.iva);
                                        movNuevo.debitoniif = 0;
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = Convert.ToDecimal(encabezado.iva);
                                        movNuevo.debito = 0;
                                    }
                                }

                                if (parametro.id_nombre_parametro == 3 && Convert.ToDecimal(encabezado.retencion) != 0)
                                {
                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(valor);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = encabezado.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(encabezado.retencion);

                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(encabezado.retencion);
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(encabezado.retencion);
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(encabezado.retencion);
                                    }
                                }

                                if (parametro.id_nombre_parametro == 4)
                                {
                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(encabezado.iva);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = encabezado.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(encabezado.retencion_iva);

                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(encabezado.retencion_iva);
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(encabezado.retencion_iva);
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(encabezado.retencion_iva);
                                    }
                                }

                                if (parametro.id_nombre_parametro == 5)
                                {
                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(valor);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = encabezado.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(encabezado.retencion_ica);
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(encabezado.retencion_ica);
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(encabezado.retencion_ica);
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(encabezado.retencion_ica);
                                    }
                                }

                                secuencia++;

                                //Cuentas valores
                                cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                    x.centro == parametro.centro && x.cuenta == parametro.cuenta &&
                                    x.nit == movNuevo.nit);
                                if (buscar_cuentas_valores != null)
                                {
                                    buscar_cuentas_valores.debito += movNuevo.debito;
                                    buscar_cuentas_valores.credito += movNuevo.credito;
                                    buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                                    buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                                    context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                }
                                else
                                {
                                    DateTime fechaHoy = DateTime.Now;
                                    cuentas_valores crearCuentaValor = new cuentas_valores
                                    {
                                        ano = fechaHoy.Year,
                                        mes = fechaHoy.Month,
                                        cuenta = movNuevo.cuenta,
                                        centro = movNuevo.centro,
                                        nit = movNuevo.nit,
                                        debito = movNuevo.debito,
                                        credito = movNuevo.credito,
                                        debitoniff = movNuevo.debitoniif,
                                        creditoniff = movNuevo.creditoniif
                                    };
                                    context.cuentas_valores.Add(crearCuentaValor);
                                }

                                context.mov_contable.Add(movNuevo);

                                totalCreditos += movNuevo.credito;
                                totalDebitos += movNuevo.debito;

                                listaDescuadrados.Add(new DocumentoDescuadradoModel
                                {
                                    NumeroCuenta = "(" + info.cntpuc_numero + ")" + info.cntpuc_descp,
                                    DescripcionParametro = descripcionParametro,
                                    ValorDebito = movNuevo.debito,
                                    ValorCredito = movNuevo.credito
                                });
                            }
                        }
                    }

                    if (totalDebitos != totalCreditos)
                    {
                        response = 0;
                    }
                    else
                    {
                        context.SaveChanges();
                        DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
                        doc.ActualizarConsecutivo(grupo.grupo, consecutivo);
                        crearMatricula(id, tramitador_id, cartera, placa, valor, fechamatricula, observacion, encabezado);
                        response = 1;
                    }
                }
                else
                {
                    response = 0;
                }
            }
            else
            {
                List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();
                response = 0;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}