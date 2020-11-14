using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class generarchequesController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");

        public void listas(int? idNitPagar)
        {
            List<SelectListItem> lista = new List<SelectListItem>();
            if (idNitPagar != null)
            {
                var list2 = (from t in context.icb_terceros
                             where t.tercero_id == idNitPagar
                             select new
                             {
                                 t.tercero_id,
                                 t.prinom_tercero,
                                 t.apellido_tercero,
                                 t.segnom_tercero,
                                 t.segapellido_tercero,
                                 t.doc_tercero,
                                 t.razon_social
                             }).OrderBy(x => x.prinom_tercero).ToList();

                foreach (var item in list2)
                {
                    lista.Add(new SelectListItem
                    {
                        Text = item.doc_tercero + " " + item.prinom_tercero + ' ' + item.segnom_tercero + ' ' +
                               item.apellido_tercero + ' ' + item.segnom_tercero + ' ' + item.razon_social,
                        Value = item.tercero_id.ToString()
                    });
                }
            }
            else
            {
                var list = (from t in context.icb_terceros
                                //where t.tercero_estado == true
                            select new
                            {
                                t.tercero_id,
                                t.prinom_tercero,
                                t.apellido_tercero,
                                t.segnom_tercero,
                                t.segapellido_tercero,
                                t.doc_tercero,
                                t.razon_social
                            }).OrderBy(x => x.prinom_tercero).ToList();

                foreach (var item in list)
                {
                    lista.Add(new SelectListItem
                    {
                        Text = item.doc_tercero + " " + item.prinom_tercero + ' ' + item.segnom_tercero + ' ' +
                               item.apellido_tercero + ' ' + item.segnom_tercero + ' ' + item.razon_social,
                        Value = item.tercero_id.ToString()
                    });
                }
            }

            ViewBag.nitp = lista.OrderBy(x => x.Text);
            ViewBag.id_pedido_vehiculo = "";

            ViewBag.doc_registros = context.tp_doc_registros.Where(x => x.tipo == 32);

            var buscarConcepto1 = (from Concepto1 in context.tpdocconceptos
                                   join tp in context.tp_doc_registros
                                       on Concepto1.tipodocid equals tp.tpdoc_id
                                   where tp.tipo == 32 /*&& Concepto1.estado == true*/
                                   select new
                                   {
                                       Concepto1.id,
                                       Concepto1.tipodocid,
                                       Concepto1.Descripcion,
                                       Concepto1.estado
                                   }).ToList();
            ViewBag.concepto1 = new SelectList(buscarConcepto1, "id", "Descripcion");
            var buscarConcepto2 = (from Concepto2 in context.tpdocconceptos2
                                   join tp in context.tp_doc_registros
                                       on Concepto2.tipodocid equals tp.tpdoc_id
                                   where tp.tipo == 32 /*&& Concepto2.estado == true*/
                                   select new
                                   {
                                       Concepto2.id,
                                       Concepto2.tipodocid,
                                       Concepto2.Descripcion,
                                       Concepto2.estado
                                   }).ToList();
            ViewBag.concepto2 = new SelectList(buscarConcepto2, "id", "Descripcion");


            ViewBag.fpago = new SelectList(context.tipopagorecibido, "id", "pago");

            var buscarTipoDocumento = (from tipoDocumento in context.tp_doc_registros
                                       select new
                                       {
                                           tipoDocumento.sw,
                                           tipoDocumento.tpdoc_id,
                                           nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                           tipoDocumento.tipo
                                       }).ToList();
            ViewBag.tipo_documentoFiltro =
                new SelectList(buscarTipoDocumento.Where(x => x.sw == 1), "tpdoc_id", "nombre");

            ViewBag.tipo = new SelectList(context.tp_doc_registros.Where(x => x.tipo == 20), "tpdoc_id",
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

            var cntAnticipos = from c in context.cuenta_puc
                               where c.cntpuc_numero == "13309501" || c.cntpuc_numero == "13309502" || c.cntpuc_numero == "13309503" ||
                                     c.cntpuc_numero == "13309505"
                               select new
                               {
                                   c.cntpuc_id,
                                   c.cntpuc_numero,
                                   c.cntpuc_descp
                               };
            List<SelectListItem> itemsCuentas = new List<SelectListItem>();
            foreach (var item in cntAnticipos)
            {
                string descripcion = "(" + item.cntpuc_numero + ") " + item.cntpuc_descp;
                itemsCuentas.Add(new SelectListItem { Text = descripcion, Value = item.cntpuc_id.ToString() });
            }

            ViewBag.vendedor = itemsU;
            ViewBag.cuentasAnticipos = itemsCuentas;
            ViewBag.moneda = new SelectList(context.monedas, "moneda", "descripcion");
            ViewBag.tasa = new SelectList(context.moneda_conversion, "id", "conversion");
            ViewBag.motivocompra = new SelectList(context.motcompra, "id", "Motivo");

            ViewBag.condicion_pago = context.fpago_tercero;

            encab_documento buscarUltimoComprobante =
                context.encab_documento.OrderByDescending(x => x.idencabezado).FirstOrDefault();
            ViewBag.numComprobanteCreado = buscarUltimoComprobante != null ? buscarUltimoComprobante.numero : 0;
        }

        public ActionResult Create(int? menu, int? idNitPagar)
        {
            listas(idNitPagar);
            string varAux = "NOPARAMETRO";
            if (idNitPagar != null)
            {
                varAux = Convert.ToString(idNitPagar);
            }

            ViewBag.proveedorSeleccionado = varAux;
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Create(notascontableschequeModel ce, int? menu)
        {
            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                {
                    try
                    {
                        string seleccionados = Request["arrayDocumentos"];
                        seleccionados = seleccionados.TrimEnd(',', ' ');

                        string[] registros = seleccionados.Split(',');
                        int cuantos = registros.Count();

                        //if (cuantos > 0){
                        //consecutivo
                        grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                            x.documento_id == ce.tipo && x.bodega_id == ce.bodega);
                        if (grupo != null)
                        {
                            DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                            long consecutivo = doc.BuscarConsecutivo(grupo.grupo);

                            //Encabezado documento

                            #region encabezado

                            encab_documento encabezado = new encab_documento
                            {
                                tipo = ce.tipo,
                                numero = consecutivo,
                                nit = ce.nitp,
                                fecha = DateTime.Now,
                                documento = Convert.ToString(0),
                                nota1 = ce.nota1,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                fec_creacion = DateTime.Now,
                                perfilcontable = ce.perfilcontable,
                                bodega = ce.bodega,
                                anticipo = ce.anticipo
                            };

                            string total = Request["totalPagar"];
                            if (ce.anticipo)
                            {
                                encabezado.valor_total = Convert.ToDecimal(ce.valor_total ,miCultura);
                                context.encab_documento.Add(encabezado);
                                context.SaveChanges();
                            }
                            else
                            {
                                encabezado.valor_total = Convert.ToDecimal(total, miCultura);
                                context.encab_documento.Add(encabezado);
                                context.SaveChanges();

                                for (int i = 0; i <= cuantos - 1; i++)
                                {
                                    int tipo = Convert.ToInt32(Request["tipo" + i]);
                                    int numero = Convert.ToInt32(Request["numero" + i]);
                                    int id = Convert.ToInt32(Request["id" + i]);
                                    decimal valor = Convert.ToDecimal(Request["valor_aPagar" + i], miCultura);

                                    encab_documento factura = context.encab_documento
                                        .Where(d => d.idencabezado == id && d.tipo == tipo).FirstOrDefault();

                                    if (valor != 0)
                                    {
                                        int valoraplicado = Convert.ToInt32(factura.valor_aplicado);

                                        decimal nuevovalor = Convert.ToDecimal(valoraplicado, miCultura) + valor;
                                        factura.valor_aplicado = Convert.ToDecimal(nuevovalor, miCultura);
                                        context.Entry(factura).State = EntityState.Modified;

                                        // si al crear el comprobande de egreso se selecciono facturas se debe guardar el registro tambien en cruce documentos, de lo contrario NO
                                        //Cruce documentos

                                        #region cruce documentos

                                        cruce_documentos cd = new cruce_documentos
                                        {
                                            idtipo = ce.tipo,
                                            numero = consecutivo,
                                            //tipo de la factura cruzada
                                            idtipoaplica = Convert.ToInt32(tipo),
                                            //numero de la factura cruzada
                                            numeroaplica = Convert.ToInt32(numero),
                                            //valor aplicado a cada factura
                                            valor = Convert.ToDecimal(valor, miCultura),
                                            fecha = DateTime.Now,
                                            fechacruce = DateTime.Now,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                                        };
                                        context.cruce_documentos.Add(cd);

                                        #endregion
                                    }
                                }
                            }

                            #endregion

                            //movimiento contable

                            #region Mov contable y cuentas valores

                            #region Si NO es anticipo

                            if (ce.anticipo == false)
                            {
                                string pagosR = Request["lista_pagos"];
                                int secuencia = 1;
                                bool flagDebito = false;
                                if (pagosR != "")
                                {
                                    decimal totalDebitos = 0;
                                    decimal totalCreditos = 0;
                                    encab_documento buscarUltimoEncabezado = context.encab_documento
                                        .OrderByDescending(x => x.fec_creacion).FirstOrDefault();

                                    List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();
                                    List<cuentas_valores> ids_cuentas_valores = new List<cuentas_valores>();
                                    centro_costo centroValorCero =
                                        context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                                    int idCentroCero = centroValorCero != null
                                        ? Convert.ToInt32(centroValorCero.centcst_id)
                                        : 0;
                                    int pagosRealizados = Convert.ToInt32(pagosR);
                                    for (int j = 1; j <= pagosRealizados; j++)
                                    {
                                        int perfilC = Convert.ToInt32(Request["perfil" + j]);
                                        if (perfilC != 0)
                                        {
                                            decimal valorPagado = Convert.ToDecimal(Request["valor" + j], miCultura);
                                            int formaPago = Convert.ToInt32(Request["fpago" + j]);
                                            DateTime fechaPago = Convert.ToDateTime(Request["fecha" + j]);
                                            string observacionPago = Request["observaciones" + j];
                                            string cheque = Request["cheque" + j];

                                            #region Guardamos los pagos comprobante egreso

                                            pagoscomprobanteegreso pce = new pagoscomprobanteegreso
                                            {
                                                idEncab = buscarUltimoEncabezado.idencabezado,
                                                formaPago = formaPago,
                                                valor = valorPagado,
                                                perfilContable = perfilC,
                                                fecha = fechaPago,
                                                cheque = cheque,
                                                observaciones = observacionPago,
                                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                                fecha_creacion = DateTime.Now
                                            };
                                            context.pagoscomprobanteegreso.Add(pce);

                                            #endregion

                                            #region Guardamos el movimiento por cada documento pagado (valor > 0)

                                            if (flagDebito == false)
                                            {
                                                for (int i = 0; i <= cuantos - 1; i++)
                                                {
                                                    int tipo = Convert.ToInt32(Request["tipo" + i]);
                                                    int numero = Convert.ToInt32(Request["numero" + i]);
                                                    int id = Convert.ToInt32(Request["id" + i]);
                                                    decimal valor = Convert.ToDecimal(Request["valor_aPagar" + i], miCultura);
                                                    if (valor > 0)
                                                    {
                                                        encab_documento factura = context.encab_documento
                                                            .Where(d => d.idencabezado == id && d.tipo == tipo)
                                                            .FirstOrDefault();
                                                        List<mov_contable> movFactura = context.mov_contable
                                                            .Where(x => x.id_encab == id).ToList();

                                                        foreach (mov_contable item in movFactura)
                                                        {
                                                            if (item.idparametronombre == 1)
                                                            {
                                                                mov_contable movNuevoD = new mov_contable
                                                                {
                                                                    id_encab =
                                                                    buscarUltimoEncabezado.idencabezado,
                                                                    seq = secuencia,
                                                                    idparametronombre = item.idparametronombre,
                                                                    cuenta = item.cuenta,
                                                                    centro = item.centro,
                                                                    fec = DateTime.Now,
                                                                    fec_creacion = DateTime.Now,
                                                                    userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]),
                                                                    detalle =
                                                                    "Pago por medio del comrpobante de egreso numero: " +
                                                                    buscarUltimoEncabezado.numero,

                                                                    credito = 0,
                                                                    debito = valor,
                                                                    creditoniif = 0,
                                                                    debitoniif = valor
                                                                };

                                                                cuenta_puc info1 = context.cuenta_puc
                                                                    .Where(x => x.cntpuc_id == item.cuenta)
                                                                    .FirstOrDefault();

                                                                if (info1.tercero)
                                                                {
                                                                    movNuevoD.nit = ce.nitp;
                                                                }
                                                                else
                                                                {
                                                                    icb_terceros tercero = context.icb_terceros
                                                                        .Where(t => t.doc_tercero == "0")
                                                                        .FirstOrDefault();
                                                                    movNuevoD.nit = tercero.tercero_id;
                                                                }

                                                                totalDebitos += Convert.ToDecimal(valor, miCultura);
                                                                context.mov_contable.Add(movNuevoD);
                                                                secuencia++;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }

                                                context.SaveChanges();
                                                flagDebito = true;
                                            }

                                            #endregion

                                            #region buscamos en perfil cuenta documento, por medio del perfil seleccionado

                                            var parametrosCuentasVerificar =
                                                (from perfil in context.perfil_cuentas_documento
                                                 join nombreParametro in context.paramcontablenombres
                                                     on perfil.id_nombre_parametro equals nombreParametro.id
                                                 join cuenta in context.cuenta_puc
                                                     on perfil.cuenta equals cuenta.cntpuc_id
                                                 where perfil.id_perfil == perfilC
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

                                            #endregion

                                            #region MovContable y CuentasValores

                                            foreach (var parametro in parametrosCuentasVerificar)
                                            {
                                                string descripcionParametro = context.paramcontablenombres
                                                    .FirstOrDefault(x => x.id == parametro.id_nombre_parametro)
                                                    .descripcion_parametro;
                                                cuenta_puc buscarCuenta =
                                                    context.cuenta_puc.FirstOrDefault(x =>
                                                        x.cntpuc_id == parametro.cuenta);

                                                if (buscarCuenta != null)
                                                {
                                                    if (parametro.id_nombre_parametro == 16 ||
                                                        parametro.id_nombre_parametro == 23)
                                                    {
                                                        mov_contable movNuevo = new mov_contable
                                                        {
                                                            id_encab = buscarUltimoEncabezado.idencabezado,
                                                            seq = secuencia,
                                                            idparametronombre = parametro.id_nombre_parametro,
                                                            cuenta = parametro.cuenta,
                                                            centro = parametro.centro,
                                                            fec = DateTime.Now,
                                                            fec_creacion = DateTime.Now,
                                                            userid_creacion =
                                                            Convert.ToInt32(Session["user_usuarioid"]),
                                                            detalle = ce.nota1
                                                        };

                                                        cuenta_puc info = context.cuenta_puc
                                                            .Where(i => i.cntpuc_id == parametro.cuenta)
                                                            .FirstOrDefault();

                                                        if (info.tercero)
                                                        {
                                                            movNuevo.nit = ce.nitp;
                                                        }
                                                        else
                                                        {
                                                            icb_terceros tercero = context.icb_terceros
                                                                .Where(t => t.doc_tercero == "0").FirstOrDefault();
                                                            movNuevo.nit = tercero.tercero_id;
                                                        }

                                                        // las siguientes validaciones se hacen dependiendo de la cuenta que esta moviendo la nota credito, para guardar la informacion acorde

                                                        #region caja = 16

                                                        if (parametro.id_nombre_parametro == 16)
                                                        {
                                                            /*if (info.aplicaniff==true)
															{

															}*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(valorPagado, miCultura);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = ce.documento;
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = Convert.ToDecimal(valorPagado, miCultura);
                                                                movNuevo.debito = 0;

                                                                movNuevo.creditoniif = Convert.ToDecimal(valorPagado, miCultura);
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = Convert.ToDecimal(valorPagado, miCultura);
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = Convert.ToDecimal(valorPagado, miCultura);
                                                                movNuevo.debito = 0;
                                                            }
                                                        }

                                                        #endregion

                                                        #region Banco = 23

                                                        if (parametro.id_nombre_parametro == 23)
                                                        {
                                                            /*if (info.aplicaniff==true)
															{

															}*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(valorPagado, miCultura);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = ce.documento;
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = Convert.ToDecimal(valorPagado, miCultura);
                                                                movNuevo.debito = 0;

                                                                movNuevo.creditoniif = Convert.ToDecimal(valorPagado, miCultura);
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = Convert.ToDecimal(valorPagado, miCultura);
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = Convert.ToDecimal(valorPagado, miCultura);
                                                                movNuevo.debito = 0;
                                                            }
                                                        }

                                                        #endregion

                                                        secuencia++;

                                                        //Cuentas valores

                                                        #region Cuentas valores

                                                        DateTime fechaHoy = DateTime.Now;
                                                        cuentas_valores buscar_cuentas_valores =
                                                            context.cuentas_valores.FirstOrDefault(x =>
                                                                x.centro == parametro.centro &&
                                                                x.cuenta == parametro.cuenta && x.nit == movNuevo.nit &&
                                                                x.ano == fechaHoy.Year && x.mes == fechaHoy.Month);
                                                        if (buscar_cuentas_valores != null)
                                                        {
                                                            buscar_cuentas_valores.debito += movNuevo.debito;
                                                            buscar_cuentas_valores.credito += movNuevo.credito;
                                                            buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                                                            buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                                                            context.Entry(buscar_cuentas_valores).State =
                                                                EntityState.Modified;
                                                            context.SaveChanges();
                                                        }
                                                        else
                                                        {
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
                                                            context.SaveChanges();
                                                        }

                                                        #endregion

                                                        context.mov_contable.Add(movNuevo);

                                                        totalCreditos += movNuevo.credito;


                                                        listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                        {
                                                            NumeroCuenta =
                                                                "(" + info.cntpuc_numero + ")" + info.cntpuc_descp,
                                                            DescripcionParametro = descripcionParametro,
                                                            ValorDebito = movNuevo.debito,
                                                            ValorCredito = movNuevo.credito
                                                        });
                                                    }
                                                }
                                            }

                                            #endregion
                                        }
                                    }

                                    if (totalDebitos != totalCreditos)
                                    {
                                        TempData["mensaje_error"] =
                                            "El documento no tiene los movimientos calculados correctamente, verifique el perfil del documento";
                                        dbTran.Rollback();
                                        ViewBag.documentoSeleccionado = encabezado.tipo;
                                        ViewBag.bodegaSeleccionado = encabezado.bodega;
                                        ViewBag.perfilSeleccionado = encabezado.perfilcontable;

                                        ViewBag.documentoDescuadrado = listaDescuadrados;
                                        ViewBag.calculoDebito = totalDebitos;
                                        ViewBag.calculoCredito = totalCreditos;

                                        listas(null);
                                        return View(ce);
                                    }

                                    try
                                    {
                                        context.SaveChanges();
                                        dbTran.Commit();
                                    }
                                    catch (DbEntityValidationException dbEx)
                                    {
                                        dbTran.Rollback();
                                        Exception raise = dbEx;
                                        foreach (DbEntityValidationResult validationErrors in dbEx.EntityValidationErrors)
                                        {
                                            foreach (DbValidationError validationError in validationErrors.ValidationErrors)
                                            {
                                                string message = string.Format("{0}:{1}",
                                                    validationErrors.Entry.Entity,
                                                    validationError.ErrorMessage);
                                                raise = new InvalidOperationException(message, raise);
                                            }
                                        }

                                        throw raise;
                                    }

                                    TempData["mensaje"] = "registro creado correctamente";
                                    DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
                                    doc.ActualizarConsecutivo(grupo.grupo, consecutivo);

                                    return RedirectToAction("Create");
                                }
                            }

                            #endregion

                            #region Si SI es anticipo

                            else
                            {
                                string pagosR = Request["lista_pagos"];
                                if (pagosR != "")
                                {
                                    int secuencia = 1;
                                    decimal totalDebitos = 0;
                                    decimal totalCreditos = 0;
                                    encab_documento buscarUltimoEncabezado = context.encab_documento
                                        .OrderByDescending(x => x.fec_creacion).FirstOrDefault();

                                    List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();
                                    List<cuentas_valores> ids_cuentas_valores = new List<cuentas_valores>();
                                    centro_costo centroValorCero =
                                        context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                                    int idCentroCero = centroValorCero != null
                                        ? Convert.ToInt32(centroValorCero.centcst_id)
                                        : 0;

                                    #region guardamos el anticipo

                                    int cuentaAnticipo = Convert.ToInt32(Request["cuentasAnticipos"]);

                                    string descripcionParametro2 = context.paramcontablenombres
                                        .FirstOrDefault(x => x.id == 24).descripcion_parametro;
                                    cuenta_puc buscarCuenta2 =
                                        context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == cuentaAnticipo);

                                    if (buscarCuenta2 != null)
                                    {
                                        mov_contable movNuevo = new mov_contable
                                        {
                                            id_encab = buscarUltimoEncabezado.idencabezado,
                                            seq = secuencia,
                                            idparametronombre = 24,
                                            cuenta = cuentaAnticipo,
                                            centro = idCentroCero,
                                            fec = DateTime.Now,
                                            fec_creacion = DateTime.Now,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            detalle = ce.nota1
                                        };

                                        cuenta_puc info = context.cuenta_puc.Where(i => i.cntpuc_id == cuentaAnticipo)
                                            .FirstOrDefault();

                                        if (info.tercero)
                                        {
                                            movNuevo.nit = ce.nitp;
                                        }
                                        else
                                        {
                                            icb_terceros tercero = context.icb_terceros.Where(t => t.doc_tercero == "0")
                                                .FirstOrDefault();
                                            movNuevo.nit = tercero.tercero_id;
                                        }

                                        // las siguientes validaciones se hacen dependiendo de la cuenta que esta moviendo la nota credito, para guardar la informacion acorde

                                        #region si es de anticipo

                                        if (info.manejabase)
                                        {
                                            movNuevo.basecontable = Convert.ToDecimal(total, miCultura);
                                        }
                                        else
                                        {
                                            movNuevo.basecontable = 0;
                                        }

                                        if (info.documeto)
                                        {
                                            movNuevo.documento = ce.documento;
                                        }

                                        if (buscarCuenta2.concepniff == 1)
                                        {
                                            movNuevo.credito = 0;
                                            movNuevo.debito = Convert.ToDecimal(total, miCultura);

                                            movNuevo.creditoniif = 0;
                                            movNuevo.debitoniif = Convert.ToDecimal(total, miCultura);
                                        }

                                        if (buscarCuenta2.concepniff == 4)
                                        {
                                            movNuevo.creditoniif = 0;
                                            movNuevo.debitoniif = Convert.ToDecimal(total, miCultura);
                                        }

                                        if (buscarCuenta2.concepniff == 5)
                                        {
                                            movNuevo.credito = 0;
                                            movNuevo.debito = Convert.ToDecimal(total, miCultura);
                                        }

                                        #endregion

                                        secuencia++;

                                        //Cuentas valores

                                        #region cuentas valores

                                        DateTime fechaHoy = DateTime.Now;
                                        cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                            x.centro == idCentroCero && x.cuenta == cuentaAnticipo &&
                                            x.nit == movNuevo.nit && x.ano == fechaHoy.Year && x.mes == fechaHoy.Month);
                                        if (buscar_cuentas_valores != null)
                                        {
                                            buscar_cuentas_valores.debito += movNuevo.debito;
                                            buscar_cuentas_valores.credito += movNuevo.credito;
                                            buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                                            buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                                            context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
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
                                            context.SaveChanges();
                                        }

                                        context.mov_contable.Add(movNuevo);

                                        #endregion

                                        totalCreditos += movNuevo.credito;
                                        totalDebitos += Convert.ToDecimal(total, miCultura);

                                        listaDescuadrados.Add(new DocumentoDescuadradoModel
                                        {
                                            NumeroCuenta = "(" + info.cntpuc_numero + ")" + info.cntpuc_descp,
                                            DescripcionParametro = descripcionParametro2,
                                            ValorDebito = movNuevo.debito,
                                            ValorCredito = movNuevo.credito
                                        });
                                    }

                                    #endregion

                                    #region guardamos los movimientos de las formas de pago

                                    int pagosRealizados = Convert.ToInt32(pagosR);
                                    for (int j = 1; j <= pagosRealizados; j++)
                                    {
                                        int perfilC = Convert.ToInt32(Request["perfil" + j]);
                                        if (perfilC != 0)
                                        {
                                            decimal valorPagado = Convert.ToDecimal(Request["valor" + j], miCultura);
                                            int formaPago = Convert.ToInt32(Request["fpago" + j]);
                                            DateTime fechaPago = Convert.ToDateTime(Request["fecha" + j]);
                                            string observacionPago = Request["observaciones" + j];
                                            string cheque = Request["cheque" + j];

                                            pagoscomprobanteegreso pce = new pagoscomprobanteegreso
                                            {
                                                idEncab = buscarUltimoEncabezado.idencabezado,
                                                formaPago = formaPago,
                                                valor = valorPagado,
                                                perfilContable = perfilC,
                                                fecha = fechaPago,
                                                cheque = cheque,
                                                observaciones = observacionPago,
                                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                                fecha_creacion = DateTime.Now
                                            };
                                            context.pagoscomprobanteegreso.Add(pce);

                                            //buscamos en perfil cuenta documento, por medio del perfil seleccionado
                                            var parametrosCuentasVerificar =
                                                (from perfil in context.perfil_cuentas_documento
                                                 join nombreParametro in context.paramcontablenombres
                                                     on perfil.id_nombre_parametro equals nombreParametro.id
                                                 join cuenta in context.cuenta_puc
                                                     on perfil.cuenta equals cuenta.cntpuc_id
                                                 where perfil.id_perfil == perfilC
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

                                            foreach (var parametro in parametrosCuentasVerificar)
                                            {
                                                string descripcionParametro = context.paramcontablenombres
                                                    .FirstOrDefault(x => x.id == parametro.id_nombre_parametro)
                                                    .descripcion_parametro;
                                                cuenta_puc buscarCuenta =
                                                    context.cuenta_puc.FirstOrDefault(x =>
                                                        x.cntpuc_id == parametro.cuenta);

                                                if (buscarCuenta != null)
                                                {
                                                    if (parametro.id_nombre_parametro == 16 ||
                                                        parametro.id_nombre_parametro == 23)
                                                    {
                                                        mov_contable movNuevo = new mov_contable
                                                        {
                                                            id_encab = buscarUltimoEncabezado.idencabezado,
                                                            seq = secuencia,
                                                            idparametronombre = parametro.id_nombre_parametro,
                                                            cuenta = parametro.cuenta,
                                                            centro = parametro.centro,
                                                            fec = DateTime.Now,
                                                            fec_creacion = DateTime.Now,
                                                            userid_creacion =
                                                            Convert.ToInt32(Session["user_usuarioid"]),
                                                            detalle = ce.nota1
                                                        };

                                                        cuenta_puc info = context.cuenta_puc
                                                            .Where(i => i.cntpuc_id == parametro.cuenta)
                                                            .FirstOrDefault();

                                                        if (info.tercero)
                                                        {
                                                            movNuevo.nit = ce.nitp;
                                                        }
                                                        else
                                                        {
                                                            icb_terceros tercero = context.icb_terceros
                                                                .Where(t => t.doc_tercero == "0").FirstOrDefault();
                                                            movNuevo.nit = tercero.tercero_id;
                                                        }

                                                        // las siguientes validaciones se hacen dependiendo de la cuenta que esta moviendo la nota credito, para guardar la informacion acorde

                                                        #region si es de caja

                                                        if (parametro.id_nombre_parametro == 16)
                                                        {
                                                            /*if (info.aplicaniff==true)
															{

															}*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(valorPagado, miCultura);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = ce.documento;
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = Convert.ToDecimal(valorPagado, miCultura);
                                                                movNuevo.debito = 0;

                                                                movNuevo.creditoniif = Convert.ToDecimal(valorPagado, miCultura);
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = Convert.ToDecimal(valorPagado, miCultura);
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = Convert.ToDecimal(valorPagado, miCultura);
                                                                movNuevo.debito = 0;
                                                            }
                                                        }

                                                        #endregion

                                                        #region si es de banco

                                                        if (parametro.id_nombre_parametro == 23)
                                                        {
                                                            /*if (info.aplicaniff==true)
															{

															}*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(valorPagado, miCultura);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = ce.documento;
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = Convert.ToDecimal(valorPagado, miCultura);
                                                                movNuevo.debito = 0;

                                                                movNuevo.creditoniif = Convert.ToDecimal(valorPagado, miCultura);
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = Convert.ToDecimal(valorPagado, miCultura);
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = Convert.ToDecimal(valorPagado, miCultura);
                                                                movNuevo.debito = 0;
                                                            }
                                                        }

                                                        #endregion

                                                        secuencia++;

                                                        //Cuentas valores
                                                        DateTime fechaHoy = DateTime.Now;
                                                        cuentas_valores buscar_cuentas_valores =
                                                            context.cuentas_valores.FirstOrDefault(x =>
                                                                x.centro == parametro.centro &&
                                                                x.cuenta == parametro.cuenta && x.nit == movNuevo.nit &&
                                                                x.ano == fechaHoy.Year && x.mes == fechaHoy.Month);
                                                        if (buscar_cuentas_valores != null)
                                                        {
                                                            buscar_cuentas_valores.debito += movNuevo.debito;
                                                            buscar_cuentas_valores.credito += movNuevo.credito;
                                                            buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                                                            buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                                                            context.Entry(buscar_cuentas_valores).State =
                                                                EntityState.Modified;
                                                            context.SaveChanges();
                                                        }
                                                        else
                                                        {
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
                                                            context.SaveChanges();
                                                        }

                                                        context.mov_contable.Add(movNuevo);

                                                        totalCreditos += movNuevo.credito;
                                                        //totalDebitos += Convert.ToDecimal(valorPagado);

                                                        listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                        {
                                                            NumeroCuenta =
                                                                "(" + info.cntpuc_numero + ")" + info.cntpuc_descp,
                                                            DescripcionParametro = descripcionParametro,
                                                            ValorDebito = movNuevo.debito,
                                                            ValorCredito = movNuevo.credito
                                                        });
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    #endregion

                                    if (totalDebitos != totalCreditos)
                                    {
                                        TempData["mensaje_error"] =
                                            "El documento no tiene los movimientos calculados correctamente, verifique el perfil del documento";
                                        dbTran.Rollback();
                                        ViewBag.documentoSeleccionado = encabezado.tipo;
                                        ViewBag.bodegaSeleccionado = encabezado.bodega;
                                        ViewBag.perfilSeleccionado = encabezado.perfilcontable;

                                        ViewBag.documentoDescuadrado = listaDescuadrados;
                                        ViewBag.calculoDebito = totalDebitos;
                                        ViewBag.calculoCredito = totalCreditos;

                                        listas(null);
                                        return View(ce);
                                    }

                                    try
                                    {
                                        context.SaveChanges();
                                        dbTran.Commit();
                                    }
                                    catch (DbEntityValidationException dbEx)
                                    {
                                        dbTran.Rollback();
                                        Exception raise = dbEx;
                                        foreach (DbEntityValidationResult validationErrors in dbEx.EntityValidationErrors)
                                        {
                                            foreach (DbValidationError validationError in validationErrors.ValidationErrors)
                                            {
                                                string message = string.Format("{0}:{1}",
                                                    validationErrors.Entry.Entity,
                                                    validationError.ErrorMessage);
                                                raise = new InvalidOperationException(message, raise);
                                            }
                                        }

                                        throw raise;
                                    }

                                    TempData["mensaje"] = "registro creado correctamente";
                                    DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
                                    doc.ActualizarConsecutivo(grupo.grupo, consecutivo);

                                    return RedirectToAction("Create");
                                }
                            }

                            #endregion

                            #endregion
                        }
                        else
                        {
                            TempData["mensaje_error"] = "no hay consecutivo";
                            dbTran.Rollback();
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
                TempData["mensaje_error"] = "No fue posible guardar el registro, por favor valide";
                List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();
            }

            listas(null);
            return View(ce);
        }

        public ActionResult Browser(int? menu)
        {
            // BuscarFavoritos(menu);
            return View();
        }

        public JsonResult BuscarDatos()
        {
            var data2 = (from e in context.encab_documento
                         join tp in context.tp_doc_registros
                             on e.tipo equals tp.tpdoc_id
                         join b in context.bodega_concesionario
                             on e.bodega equals b.id
                         join t in context.icb_terceros
                             on e.nit equals t.tercero_id
                         where tp.tipo == 32
                         select new
                         {
                             tipo = "(" + tp.prefijo + ") " + tp.tpdoc_nombre,
                             e.numero,
                             nit = t.prinom_tercero != null
                                 ? "(" + t.doc_tercero + ") " + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                   t.apellido_tercero + " " + t.segapellido_tercero
                                 : "(" + t.doc_tercero + ") " + t.razon_social,
                             e.fecha,
                             e.valor_total,
                             id = e.idencabezado,
                             bodega = b.bodccs_nombre
                         }).ToList();
            List<paraBrowser> data = data2.Select(c => new paraBrowser
            {
                tipo = c.tipo,
                numero = c.numero,
                nit = c.nit,
                fecha = c.fecha != null ? c.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                valor_total = c.valor_total,
                id = c.id,
                bodega = c.bodega
            }).ToList();


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Detalles(int id, int? menu)
        {
            encab_documento encab = context.encab_documento.Find(id);
            ViewBag.encab = id;
            ViewBag.numero = encab.numero;
            ViewBag.valor_total = encab.valor_total;
            ViewBag.fecha = encab.fecha;
            icb_terceros t = context.icb_terceros.Find(encab.nit);
            ViewBag.cliente = t.prinom_tercero != null
                ? "(" + t.doc_tercero + ") " + t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero +
                  " " + t.segapellido_tercero
                : "(" + t.doc_tercero + ") " + t.razon_social;

            //var p = context.documentos_pago.Where(x => x.tercero == encab.nit && x.idtencabezado == id).Select(x => x.pedido).FirstOrDefault();
            //if (p != null)
            //{
            //    ViewBag.pedido = p;
            //}

            //var desP = (from vp in context.vpedido
            //            join m in context.modelo_vehiculo
            //            on vp.modelo equals m.modvh_codigo
            //            where vp.nit == encab.nit
            //            select new desP
            //            {
            //                id = vp.id,
            //                numero = vp.numero ?? 0,
            //                carro = m.modvh_nombre
            //            }).FirstOrDefault();
            //ViewBag.desP = desP;

            BuscarFavoritos(menu);
            return View(encab);
        }

        public JsonResult BuscarDatosDetalles(int tipo, int numero)
        {
            var data2 = (from c in context.cruce_documentos
                         join e in context.encab_documento
                             on c.idtipoaplica equals e.tipo
                         join t in context.tp_doc_registros
                             on c.idtipoaplica equals t.tpdoc_id
                         where c.numeroaplica == e.numero
                               && c.numero == numero
                               && c.idtipo == tipo
                         select new
                         {
                             tipo = "(" + t.prefijo + ") " + t.tpdoc_nombre,
                             e.numero,
                             e.fecha,
                             e.vencimiento,
                             e.valor_total,
                             e.valor_aplicado,
                             saldo = e.valor_total - e.valor_aplicado
                         }).ToList();
            List<paraBrowserDetalle> data = data2.Select(c => new paraBrowserDetalle
            {
                tipo = c.tipo,
                numero = c.numero,
                fecha = c.fecha != null ? c.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                vencimiento = c.vencimiento != null
                    ? c.vencimiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : "",
                valor_total = c.valor_total,
                valor_aplicado = c.valor_aplicado,
                saldo = c.saldo
            }).ToList();


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPagosRecibidos(int id)
        {
            var data2 = (from e in context.encab_documento
                         join p in context.pagoscomprobanteegreso
                             on e.idencabezado equals p.idEncab
                         join c in context.perfil_contable_documento
                             on p.perfilContable equals c.id
                         where e.idencabezado == id
                         select new
                         {
                             p.formaPago,
                             p.valor,
                             p.fecha,
                             p.observaciones,
                             p.cheque,
                             c.descripcion
                         }).ToList();

            var data = data2.Select(x => new
            {
                fpago = x.formaPago == 1 ? "Efectivo" :
                    x.formaPago == 2 ? "Cheque" :
                    x.formaPago == 3 ? "Transacción" :
                    x.formaPago == 4 ? "Maquina Efectivo" :
                    x.formaPago == 5 ? "Endoso" : "",
                x.valor,
                fecha = x.fecha.ToString("yyyy/MM/dd"),
                observaciones = x.observaciones != null ? x.observaciones.ToString() : "",
                cheque = x.cheque != null ? x.cheque.ToString() : "",
                perfil = x.descripcion
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarBloqueoTercero(int tercero_id)
        {
            var proveedor = (from t in context.icb_terceros
                             where t.tercero_id == tercero_id
                             select new
                             {
                                 t.tercero_estado,
                                 t.razoninactivo
                             }).ToList();
            var data = new
            {
                proveedor
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarFacturasFiltro(int nit, DateTime? desde, DateTime? hasta, int? id_documento,
            int? factura)
        {
            if (desde == null)
            {
                desde = context.encab_documento.OrderBy(x => x.fecha).Select(x => x.fecha).FirstOrDefault();
            }

            if (hasta == null)
            {
                hasta = context.encab_documento.OrderByDescending(x => x.fecha).Select(x => x.fecha).FirstOrDefault();
            }
            else
            {
                hasta = hasta.GetValueOrDefault();
            }

            List<int> listaDocumentos = new List<int>();
            if (id_documento != null)
            {
                listaDocumentos.Add(id_documento ?? 0);
            }
            else
            {
                listaDocumentos = context.tp_doc_registros.Where(x => x.sw == 1).Select(x => x.tpdoc_id).ToList();
            }

            if (desde != null && hasta != null && desde < hasta)
            {
                //hago todo el resto de funcion
                if (factura != null)
                {
                    var buscarFacturasConSaldo = (from e in context.encab_documento
                                                  join v in context.icb_vehiculo
                                                      on e.documento equals v.plan_mayor into tmp
                                                  from v in tmp.DefaultIfEmpty()
                                                  join t in context.tp_doc_registros
                                                      on e.tipo equals t.tpdoc_id
                                                  join tp in context.tp_doc_registros_tipo
                                                      on t.tipo equals tp.id
                                                  where listaDocumentos.Contains(t.tpdoc_id)
                                                        && (v.codigo_pago != "FG03" || v.codigo_pago == null)
                                                        && e.valor_total - e.valor_aplicado != 0
                                                        && e.fecha >= desde && e.fecha <= hasta
                                                        && e.numero == factura
                                                        && e.nit == nit
                                                        && e.vencimiento != null
                                                  select new
                                                  {
                                                      id = e.idencabezado,
                                                      e.fecha,
                                                      e.valor_aplicado,
                                                      e.valor_total,
                                                      e.numero,
                                                      e.vencimiento,
                                                      idTipo = t.tpdoc_id,
                                                      tipo = "(" + t.prefijo + ") " + t.tpdoc_nombre,
                                                      saldo = e.valor_total - e.valor_aplicado,
                                                      descripcion = t.tpdoc_nombre,
                                                      numeroFactura = e.numero,
                                                      prefijo = e.idencabezado,
                                                      tp = e.tipo,
                                                      factura = e.documento != null ? e.documento : ""
                                                  }).ToList();

                    var data = buscarFacturasConSaldo.Select(x => new
                    {
                        x.id,
                        fecha = x.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                        x.valor_aplicado,
                        x.valor_total,
                        x.numero,
                        vencimiento = x.vencimiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                        x.idTipo,
                        x.tipo,
                        x.saldo,
                        x.descripcion,
                        x.numeroFactura,
                        x.prefijo,
                        x.tp,
                        x.factura
                    });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var buscarFacturasConSaldo = (from e in context.encab_documento
                                                  join v in context.icb_vehiculo
                                                      on e.documento equals v.plan_mayor into tmp
                                                  from v in tmp.DefaultIfEmpty()
                                                  join t in context.tp_doc_registros
                                                      on e.tipo equals t.tpdoc_id
                                                  join tp in context.tp_doc_registros_tipo
                                                      on t.tipo equals tp.id
                                                  where listaDocumentos.Contains(t.tpdoc_id)
                                                        && (v.codigo_pago != "FG03" || v.codigo_pago == null)
                                                        && e.valor_total - e.valor_aplicado != 0
                                                        && e.fecha >= desde && e.fecha <= hasta
                                                        && e.nit == nit
                                                        && e.vencimiento != null
                                                  select new
                                                  {
                                                      id = e.idencabezado,
                                                      e.fecha,
                                                      e.valor_aplicado,
                                                      e.valor_total,
                                                      e.numero,
                                                      e.vencimiento,
                                                      idTipo = t.tpdoc_id,
                                                      tipo = "(" + t.prefijo + ") " + t.tpdoc_nombre,
                                                      saldo = e.valor_total - e.valor_aplicado,
                                                      descripcion = t.tpdoc_nombre,
                                                      numeroFactura = e.numero,
                                                      t.prefijo,
                                                      tp = e.tipo,
                                                      factura = e.documento != null ? e.documento : ""
                                                  }).ToList();

                    var data = buscarFacturasConSaldo.Select(x => new
                    {
                        x.id,
                        fecha = x.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                        x.valor_aplicado,
                        x.valor_total,
                        x.numero,
                        vencimiento = x.vencimiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                        x.idTipo,
                        x.tipo,
                        x.saldo,
                        x.descripcion,
                        x.numeroFactura,
                        x.prefijo,
                        x.tp,
                        x.factura
                    });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public ActionResult comprobante(int id)
        {
            var generarCabeceraComprobante = (from nom in context.encab_documento
                                              join reg in context.tp_doc_registros
                                                  on nom.tipo equals reg.tpdoc_id
                                              join ter in context.icb_terceros
                                                  on nom.nit equals ter.tercero_id
                                              join bc in context.bodega_concesionario
                                                  on nom.bodega equals bc.id
                                              join empr in context.tablaempresa
                                                  on bc.concesionarioid equals empr.id
                                              where nom.idencabezado == id
                                              select new
                                              {
                                                  // empresa
                                                  id_emp = empr.id,
                                                  desemp = empr.nombre_empresa,
                                                  diremp = empr.direccion,
                                                  rifemp = empr.nit,
                                                  // comprobante
                                                  id_com = nom.idencabezado,
                                                  numcom = nom.numero,
                                                  feccom = nom.fecha,
                                                  totcom = nom.valor_total,
                                                  // proveedor
                                                  id_pro = ter.tercero_id,
                                                  nitpro = nom.nit,
                                                  docpro = ter.doc_tercero,
                                                  nompro = ter.tpdoc_id == 1 ? ter.razon_social : ter.prinom_tercero + " " + ter.apellido_tercero
                                              }).FirstOrDefault();

            var data = (from enc in context.encab_documento
                        join cru in context.cruce_documentos
                            on enc.numero equals cru.numero
                        join mov in context.mov_contable
                            on enc.idencabezado equals mov.id_encab
                        join cp in context.cuenta_puc
                            on mov.cuenta equals cp.cntpuc_id
                        join ter in context.icb_terceros
                            on enc.nit equals ter.tercero_id
                        where enc.idencabezado == id && cru.idtipo == 3057 //id de cruce documentos
                        select new
                        {
                            iddetalle = enc.idencabezado,
                            cuenta = cp.cntpuc_numero,
                            detalle = cp.cntpuc_descp,
                            docProveedor = ter.doc_tercero,
                            nombreProveedor = ter.razon_social,
                            idebito = cp.mov_cnt == "Credito" ? cru.valor : 0,
                            icredito = cp.mov_cnt == "Debito" ? cru.valor : 0,

                            tdeb = cp.mov_cnt == "Credito" ? mov.debito : 0,
                            tcre = cp.mov_cnt == "Debito" ? mov.credito : 0
                        }).ToList(); //OrderBy(x => x.credito)
            List<detalleComprobante> detallevomp = data.Select(d => new detalleComprobante
            {
                iddetalle = d.iddetalle,
                cuenta = d.cuenta,
                detalle = d.detalle,
                docProveedor = d.docProveedor,
                nombreProveedor = d.nombreProveedor,
                debito = d.tdeb.ToString("N2"),
                credito = d.tcre.ToString("N2"),

                tdeb = d.idebito.ToString("N2"),
                tcre = d.icredito.ToString("N2")
            }).OrderBy(x => x.credito).ToList();

            string root = Server.MapPath("~/Pdf/");
            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
            string path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);
            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

            ModeloComprobante comprobante = new ModeloComprobante
            {
                idEmpresa = generarCabeceraComprobante.id_emp,
                descripcionEmpresa = generarCabeceraComprobante.desemp,
                direccionEmpresa = generarCabeceraComprobante.diremp,
                rifEmpresa = generarCabeceraComprobante.rifemp,

                idComprobante = generarCabeceraComprobante.id_com,
                numeroComprobante = generarCabeceraComprobante.numcom,
                fechaComprobante = generarCabeceraComprobante.feccom != null
                    ? generarCabeceraComprobante.feccom.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : "",
                //  fecha = c.fecha != null ? c.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                totalComprobante = generarCabeceraComprobante.totcom.ToString("N2"),

                idProveedor = generarCabeceraComprobante.id_pro,
                nitProveedor = generarCabeceraComprobante.nitpro,
                docProveedor = generarCabeceraComprobante.docpro,
                nomProveedor = generarCabeceraComprobante.nompro,
                detalleComprobante = detallevomp
            };
            //   reciboCaja.referencias.ForEach(item2 => item2.valorInicial = Math.Truncate(item2.valorInicial));
            ViewAsPdf something = new ViewAsPdf("comprobante", comprobante);
            return something;

            //    CrystalReport1 report = new CrystalReport1();
            //    report.SetDataSource(datos)

            //ReportObject reportObjectText1 = report.Section2.ReportObjects("Text1")

            //reportObjectText1.Border.BackgroundColor = Color.Blue

            //CrystalReportViewer1.ReportSource = report
        }

        public JsonResult BuscarPagos()
        {
            var pagos = from p in context.tipopagorecibido
                        select new
                        {
                            p.id,
                            p.pago
                        };

            var bancos = from b in context.icb_unidad_financiera
                         select new
                         {
                             id = b.financiera_id,
                             b.financiera_nombre
                         };

            var data = new
            {
                pagos,
                bancos
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarAutorizacion(string codigo)
        {
            var data = (from a in context.autorizaciones
                        where a.ref_codigo == codigo
                              && a.autorizado
                              && a.fecha_autorizacion != null
                              && a.documento == null
                              && a.fecha_creacion.Value.Year == DateTime.Now.Year
                              && a.fecha_creacion.Value.Month == DateTime.Now.Month
                              && a.fecha_creacion.Value.Day == DateTime.Now.Day
                        select new
                        {
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SolicitarAutorizacion(string codigo, int bodega)
        {
            int usuario_actual = Convert.ToInt32(Session["user_usuarioid"]);
            icb_referencia refe = context.icb_referencia.Find(codigo);
            int result = 0;
            configuracion_envio_correos correoconfig = context.configuracion_envio_correos.Where(d => d.activo).FirstOrDefault();

            usuarios_autorizaciones usuarios_autorizacion = context.usuarios_autorizaciones.FirstOrDefault(x => x.tipoautorizacion == 4);
            if (usuarios_autorizacion != null)
            {
                autorizaciones existe = context.autorizaciones.FirstOrDefault(x => x.ref_codigo == codigo
                                                                        && x.user_autorizacion ==
                                                                        usuarios_autorizacion.user_id
                                                                        && x.tipo_autorizacion == 4
                                                                        && x.fecha_creacion.Value.Year ==
                                                                        DateTime.Now.Year
                                                                        && x.fecha_creacion.Value.Month ==
                                                                        DateTime.Now.Month
                                                                        && x.fecha_creacion.Value.Day ==
                                                                        DateTime.Now.Day);

                if (existe == null)
                {
                    int usuario_autorizacion = usuarios_autorizacion.user_id;

                    autorizaciones autorizacion = new autorizaciones
                    {
                        ref_codigo = codigo,
                        bodega = bodega,
                        user_autorizacion = usuario_autorizacion,
                        user_creacion = usuario_actual,
                        fecha_creacion = DateTime.Now,
                        tipo_autorizacion = 4
                    };
                    context.autorizaciones.Add(autorizacion);
                    context.SaveChanges();
                    int autorizacion_id = context.autorizaciones.OrderByDescending(x => x.id)
                        .FirstOrDefault(x => x.tipo_autorizacion == 4).id;
                    result = 1;

                    try
                    {
                        notificaciones correo_enviado = context.notificaciones.FirstOrDefault(x =>
                            x.user_destinatario == usuario_autorizacion && x.enviado != true &&
                            x.autorizacion_id == autorizacion_id);
                        if (correo_enviado == null)
                        {
                            users user_destinatario = context.users.Find(usuario_autorizacion);
                            users user_remitente = context.users.Find(usuario_actual);

                            MailAddress de = new MailAddress(correoconfig.correo, correoconfig.nombre_remitente);
                            MailAddress para = new MailAddress(user_destinatario.user_email,
                                user_destinatario.user_nombre + " " + user_destinatario.user_apellido);
                            MailMessage mensaje = new MailMessage(de, para);
                            //mensaje.Bcc.Add("liliana.avila@exiware.com");
                            mensaje.Bcc.Add("correospruebaexi2019@gmail.com");
                            mensaje.Subject = "Solicitud Autorización facturación referencia " + codigo;
                            mensaje.ReplyToList.Add(new MailAddress(user_remitente.user_email,
                                user_remitente.user_nombre + " " + user_remitente.user_apellido));
                            mensaje.BodyEncoding = Encoding.Default;
                            mensaje.IsBodyHtml = true;
                            string html = "";
                            html += "<h4>Cordial Saludo</h4><br>";
                            html += "<p>El usuario " + user_remitente.user_nombre + " " + user_remitente.user_apellido +
                                    " solicita autorización para facturacion de la "
                                    + " referencia " + codigo + " por precio de venta menor al costo </p><br /><br />";
                            html += "Por favor ingrese a la plataforma para dar autorización.";
                            mensaje.Body = html;

                            SmtpClient cliente = new SmtpClient(correoconfig.smtp_server)
                            {
                                Port = correoconfig.puerto,
                                UseDefaultCredentials = false,
                                Credentials = new NetworkCredential(correoconfig.usuario, correoconfig.password),
                                EnableSsl = true
                            };
                            cliente.Send(mensaje);

                            notificaciones envio = new notificaciones
                            {
                                user_remitente = usuario_actual,
                                asunto = "Notificación solicitud autorización facturación referencia",
                                fecha_envio = DateTime.Now,
                                enviado = true,
                                user_destinatario = usuario_autorizacion,
                                autorizacion_id = autorizacion_id
                            };
                            context.notificaciones.Add(envio);
                            context.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        notificaciones envio = new notificaciones
                        {
                            user_remitente = usuario_actual,
                            asunto = "Notificación solicitud autorización facturación referencia",
                            fecha_envio = DateTime.Now,
                            user_destinatario = usuario_autorizacion,
                            autorizacion_id = autorizacion_id,
                            enviado = false,
                            razon_no_envio = ex.Message
                        };
                        context.notificaciones.Add(envio);
                        context.SaveChanges();
                        //notificacion no enviada
                        result = -1;
                    }
                }
                else
                {
                    autorizaciones noAutorizo = context.autorizaciones.FirstOrDefault(x => x.ref_codigo == codigo
                                                                                && x.autorizado == false
                                                                                && x.fecha_autorizacion != null
                                                                                && x.fecha_creacion.Value.Year ==
                                                                                DateTime.Now.Year
                                                                                && x.fecha_creacion.Value.Month ==
                                                                                DateTime.Now.Month
                                                                                && x.fecha_creacion.Value.Day ==
                                                                                DateTime.Now.Day);
                    if (noAutorizo != null)
                    {
                        result = 3;
                    }
                    else
                    {
                        // ya existe
                        result = 2;
                    }
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPerfilPorBodega(int bodega, int tipoD, int fpago)
        {
            if (fpago == 1 || fpago == 4 || fpago == 5)
            {
                var data = (from b in context.perfil_contable_bodega
                            join t in context.perfil_contable_documento
                                on b.idperfil equals t.id
                            join c in context.perfil_cuentas_documento
                                on t.id equals c.id_perfil
                            where b.idbodega == bodega && t.tipo == tipoD && c.id_nombre_parametro == 16
                            select new
                            {
                                id = b.idperfil,
                                perfil = t.descripcion
                            }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var data = (from b in context.perfil_contable_bodega
                            join t in context.perfil_contable_documento
                                on b.idperfil equals t.id
                            join c in context.perfil_cuentas_documento
                                on t.id equals c.id_perfil
                            where b.idbodega == bodega && t.tipo == tipoD && c.id_nombre_parametro == 23
                            select new
                            {
                                id = b.idperfil,
                                perfil = t.descripcion
                            }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult BuscarBancos()
        {
            var data = from b in context.bancos
                       select new
                       {
                           b.id,
                           b.Descripcion
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult cargarCuentas(int idBanco)
        {
            var data = (from b in context.bancos
                        join c in context.cuentasbancarias
                            on b.id equals c.idBanco into temp
                        from c in temp.DefaultIfEmpty()
                        where c.idBanco == idBanco && c.estado
                        select new
                        {
                            c.id,
                            descripcion = "(" + c.cuenta + ") " + c.numeroCuenta
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
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

        public class paraBrowser
        {
            public string tipo { get; set; }
            public long numero { get; set; }
            public string nit { get; set; }
            public string fecha { get; set; }
            public decimal valor_total { get; set; }
            public int id { get; set; }
            public string bodega { get; set; }
        }

        public class paraBrowserDetalle
        {
            public string tipo { get; set; }
            public long numero { get; set; }
            public string fecha { get; set; }
            public string vencimiento { get; set; }
            public decimal valor_total { get; set; }
            public decimal valor_aplicado { get; set; }
            public decimal saldo { get; set; }
        }
    }
}

//public ActionResult CreateREspaldo(notascontableschequeModel rc)
//{
//    if (ModelState.IsValid)
//    {
//        var seleccionados = Request["totalregistros"];
//        seleccionados = seleccionados.TrimEnd(',', ' ');

//        String[] registros = seleccionados.Split(',');
//        var cuantos = registros.Count();

//        if (cuantos > 0)
//        {


//            //}
//            //consecutivo
//            var grupo = context.grupoconsecutivos.FirstOrDefault(x => x.documento_id == rc.tipo && x.bodega_id == rc.bodega);
//            if (grupo != null)
//            {
//                DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
//                var consecutivo = doc.BuscarConsecutivo(grupo.grupo);

//                //Encabezado documento
//                encab_documento encabezado = new encab_documento();
//                encabezado.tipo = rc.tipo;
//                encabezado.numero = consecutivo;
//                encabezado.nit = rc.nitp;
//                encabezado.fecha = DateTime.Now;
//                var total = Request["totalPagar"];
//                encabezado.valor_total = Convert.ToDecimal(total);
//                encabezado.documento = Convert.ToString(0);
//                encabezado.nota1 = rc.nota1;
//                encabezado.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
//                encabezado.fec_creacion = DateTime.Now;
//                encabezado.perfilcontable = rc.perfilcontable;
//                encabezado.bodega = rc.bodega;
//                encabezado.anticipo = rc.anticipo;
//                context.encab_documento.Add(encabezado);

//                //var pedido = Request["id_pedido_vehiculo"];
//                //Convert.ToString(pedido);
//                /************************************************************ Guardo informacion si selecciona factura ************************************************************/

//                //if (pedido == "")
//                //{
//                //valor aplicado tengo que actualizarlo en la(s) factura(s) a la(s) que estoy generando el recibo de caja
//                //buscas la factura

//                //var seleccionados = Request["totalregistros"];
//                //seleccionados = seleccionados.TrimEnd(',', ' ');

//                //String[] registros = seleccionados.Split(',');
//                //var cuantos = registros.Count();
//                if (cuantos > 0)
//                {
//                    var ok = registros;

//                    //for (int i = 1; i <= lista_facturas; i++)
//                    for (int i = 0; i <= cuantos - 1; i++)

//                    {
//                        var tipo = Convert.ToInt32(Request["tipo" + i]);
//                        var numero = Convert.ToInt32(Request["numero" + i]);
//                        var id = Convert.ToInt32(Request["id" + i]);
//                        var valor = Convert.ToDecimal(Request["valor_aPagar" + i]);

//                        //var tipo = Convert.ToInt32(Request["tipo"]);
//                        //var numero = Convert.ToInt32(Request["numero"]);
//                        //var id = Convert.ToInt32(Request["id"]);
//                        //var valor = Convert.ToDecimal(Request["valor_aPagar"]);

//                        var factura = context.encab_documento.Where(d => d.idencabezado == id && d.tipo == tipo).FirstOrDefault();

//                        if (valor != 0)
//                        {
//                            var valoraplicado = Convert.ToInt32(factura.valor_aplicado);

//                            var nuevovalor = Convert.ToDecimal(valoraplicado) + valor;
//                            factura.valor_aplicado = Convert.ToDecimal(nuevovalor);
//                            context.Entry(factura).State = EntityState.Modified;

//                            // si al crear la nota credito se selecciono factura se debe guardar el registro tambien en cruce documentos, de lo contrario NO
//                            //Cruce documentos
//                            cruce_documentos cd = new cruce_documentos();
//                            cd.idtipo = rc.tipo;
//                            cd.numero = consecutivo;
//                            //tipo de la factura cruzada
//                            cd.idtipoaplica = Convert.ToInt32(tipo);
//                            //numero de la factura cruzada
//                            cd.numeroaplica = Convert.ToInt32(numero);
//                            //valor aplicado a cada factura
//                            cd.valor = Convert.ToDecimal(valor);
//                            cd.fecha = DateTime.Now;
//                            cd.fechacruce = DateTime.Now;
//                            cd.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
//                            context.cruce_documentos.Add(cd);

//                            // agrego los pagos de las facturas en documentos pago
//                            //pagos
//                            //var lista_pagos = Convert.ToInt32(Request["lista_pagos"]);
//                            //for (int j = 1; j <= lista_pagos; j++)
//                            //{
//                            //    if (!string.IsNullOrEmpty(Request["fpago" + j]))
//                            //    {
//                            //        documentos_pago dpago = new documentos_pago();
//                            //        dpago.idtencabezado = rc.idencabezado;
//                            //        dpago.tercero = rc.nitp;
//                            //        dpago.fecha = DateTime.Now;
//                            //        dpago.forma_pago = Convert.ToInt32(Request["fpago" + j]);
//                            //        dpago.valor = Convert.ToDecimal(Request["valor" + j]);
//                            //        dpago.cuenta_banco = Convert.ToString(Request["cuenta" + j]);
//                            //        dpago.documento = Convert.ToString(Request["cheque" + j]);
//                            //        dpago.notas = Request["observaciones" + j];
//                            //        var banco = Request["banco" + j];
//                            //        if (!string.IsNullOrEmpty(Request["banco" + j]))
//                            //        {
//                            //            dpago.banco = Convert.ToInt32(Request["banco" + j]);
//                            //        }
//                            //        context.documentos_pago.Add(dpago);
//                            //    }
//                            //}
//                        }
//                    }


//                }


//                ////////var lista_facturas = Convert.ToInt32(Request["listaFacturas"]);
//                ////////    for (int i = 1; i <= lista_facturas; i++)
//                ////////    {
//                ////////        var tipo = Convert.ToInt32(Request["tipo" + i]);
//                ////////        var numero = Convert.ToInt32(Request["numero" + i]);
//                ////////        var id = Convert.ToInt32(Request["id" + i]);
//                ////////        var valor = Convert.ToDecimal(Request["valor_aPagar" + i]);

//                ////////        var factura = context.encab_documento.Where(d => d.idencabezado == id && d.tipo == tipo).FirstOrDefault();

//                ////////        if (valor != 0)
//                ////////        {
//                ////////            var valoraplicado = Convert.ToInt32(factura.valor_aplicado) ;

//                ////////            var nuevovalor = Convert.ToDecimal(valoraplicado) + valor;
//                ////////            factura.valor_aplicado = Convert.ToDecimal(nuevovalor);
//                ////////            context.Entry(factura).State = EntityState.Modified;

//                ////////            // si al crear la nota credito se selecciono factura se debe guardar el registro tambien en cruce documentos, de lo contrario NO
//                ////////            //Cruce documentos
//                ////////            cruce_documentos cd = new cruce_documentos();
//                ////////            cd.idtipo = rc.tipo;
//                ////////            cd.numero = consecutivo;
//                ////////            //tipo de la factura cruzada
//                ////////            cd.idtipoaplica = Convert.ToInt32(tipo);
//                ////////            //numero de la factura cruzada
//                ////////            cd.numeroaplica = Convert.ToInt32(numero);
//                ////////            //valor aplicado a cada factura
//                ////////            cd.valor = Convert.ToDecimal(valor);
//                ////////            cd.fecha = DateTime.Now;
//                ////////            cd.fechacruce = DateTime.Now;
//                ////////            cd.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
//                ////////            context.cruce_documentos.Add(cd);

//                ////////    // agrego los pagos de las facturas en documentos pago
//                ////////    //pagos
//                ////////    var lista_pagos = Convert.ToInt32(Request["lista_pagos"]);
//                ////////            for (int j = 1; j <= lista_pagos; j++)
//                ////////            {
//                ////////                if (!string.IsNullOrEmpty(Request["fpago" + j]))
//                ////////                {
//                ////////                    documentos_pago dpago = new documentos_pago();
//                ////////                    dpago.idtencabezado = rc.idencabezado;
//                ////////                    dpago.tercero = rc.nitp;
//                ////////                    dpago.fecha = DateTime.Now;
//                ////////                    dpago.forma_pago = Convert.ToInt32(Request["fpago" + j]);
//                ////////                    dpago.valor = Convert.ToDecimal(Request["valor" + j]);
//                ////////                    dpago.cuenta_banco = Convert.ToString(Request["cuenta" + j]);
//                ////////                    dpago.documento = Convert.ToString(Request["cheque" + j]);
//                ////////                    dpago.notas = Request["observaciones" + j];
//                ////////                    var banco = Request["banco" + j];
//                ////////                    if (!string.IsNullOrEmpty(Request["banco" + j]))
//                ////////                    {
//                ////////                        dpago.banco = Convert.ToInt32(Request["banco" + j]);
//                ////////                    }
//                ////////                    context.documentos_pago.Add(dpago);
//                ////////                }
//                ////////            }
//                ////////        }
//                ////////    }
//                //}
//                //else
//                //{
//                //    // agrego los pagos del anticipo en documentos pago
//                //    //pagos
//                //    var lista_pagos = Convert.ToInt32(Request["lista_pagos"]);
//                //    for (int j = 1; j <= lista_pagos; j++)
//                //    {
//                //        if (!string.IsNullOrEmpty(Request["fpago" + j]))
//                //        {
//                //            documentos_pago dpago = new documentos_pago();
//                //            dpago.idtencabezado = rc.idencabezado;
//                //            dpago.tercero = rc.nit;
//                //            dpago.fecha = DateTime.Now;
//                //            dpago.forma_pago = Convert.ToInt32(Request["fpago" + j]);
//                //            dpago.valor = Convert.ToDecimal(Request["valor" + j]);
//                //            dpago.cuenta_banco = Convert.ToString(Request["cuenta" + j]);
//                //            dpago.documento = Convert.ToString(Request["cheque" + j]);
//                //            dpago.notas = Request["observaciones" + j];
//                //            var banco = Request["banco" + j];
//                //            if (!string.IsNullOrEmpty(Request["banco" + j]))
//                //            {
//                //                dpago.banco = Convert.ToInt32(Request["banco" + j]);
//                //            }
//                //            context.documentos_pago.Add(dpago);
//                //        }
//                //    }
//                //}


//                //movimiento contable
//                //buscamos en perfil cuenta documento, por medio del perfil seleccionado
//                var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
//                                                  join nombreParametro in context.paramcontablenombres
//                                                  on perfil.id_nombre_parametro equals nombreParametro.id
//                                                  join cuenta in context.cuenta_puc
//                                                  on perfil.cuenta equals cuenta.cntpuc_id
//                                                  where perfil.id_perfil == rc.perfilcontable
//                                                  select new
//                                                  {
//                                                      perfil.id,
//                                                      perfil.id_nombre_parametro,
//                                                      perfil.cuenta,
//                                                      perfil.centro,
//                                                      perfil.id_perfil,
//                                                      nombreParametro.descripcion_parametro,
//                                                      cuenta.cntpuc_numero
//                                                  }).ToList();

//                var secuencia = 1;
//                decimal totalDebitos = 0;
//                decimal totalCreditos = 0;

//                List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();
//                List<cuentas_valores> ids_cuentas_valores = new List<cuentas_valores>();
//                var centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
//                var idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
//                foreach (var parametro in parametrosCuentasVerificar)
//                {
//                    var descripcionParametro = context.paramcontablenombres.FirstOrDefault(x => x.id == parametro.id_nombre_parametro).descripcion_parametro;
//                    var buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);

//                    if (buscarCuenta != null)
//                    {
//                        //if ((parametro.id_nombre_parametro == 2 && Convert.ToDecimal(rc.iva) != 0)
//                        //   || (parametro.id_nombre_parametro == 3 && Convert.ToDecimal(rc.retencion) != 0)
//                        //   || (parametro.id_nombre_parametro == 4 && Convert.ToDecimal(rc.por_retencion_iva) != 0)
//                        //   || (parametro.id_nombre_parametro == 5 && Convert.ToDecimal(rc.retencion_ica) != 0)
//                        //   || parametro.id_nombre_parametro == 10
//                        //   || parametro.id_nombre_parametro == 11
//                        //   || parametro.id_nombre_parametro == 16)


//                        if (parametro.id_nombre_parametro == 1 || parametro.id_nombre_parametro == 16)
//                        {
//                            mov_contable movNuevo = new mov_contable();
//                            movNuevo.id_encab = rc.id_encab;
//                            movNuevo.seq = secuencia;
//                            movNuevo.idparametronombre = parametro.id_nombre_parametro;
//                            movNuevo.cuenta = parametro.cuenta;
//                            movNuevo.centro = parametro.centro;
//                            movNuevo.fec = DateTime.Now;
//                            movNuevo.fec_creacion = DateTime.Now;
//                            movNuevo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
//                            movNuevo.detalle = rc.nota1;

//                            var info = context.cuenta_puc.Where(i => i.cntpuc_id == parametro.cuenta).FirstOrDefault();

//                            if (info.tercero == true)
//                            {
//                                movNuevo.nit = rc.nitp;
//                            }
//                            else
//                            {
//                                var tercero = context.icb_terceros.Where(t => t.doc_tercero == "0").FirstOrDefault();
//                                movNuevo.nit = tercero.tercero_id;
//                            }

//                            // las siguientes validaciones se hacen dependiendo de la cuenta que esta moviendo la nota credito, para guardar la informacion acorde
//                            //  if (parametro.id_nombre_parametro == 10)
//                            if (parametro.id_nombre_parametro == 1)
//                            {
//                                /*if (info.aplicaniff==true)
//                                 * 
//                                {

//                                }*/

//                                if (info.manejabase == true)
//                                {
//                                    movNuevo.basecontable = Convert.ToDecimal(total);
//                                }
//                                else
//                                {
//                                    movNuevo.basecontable = 0;
//                                }

//                                if (info.documeto == true)
//                                {
//                                    movNuevo.documento = rc.documento;
//                                }

//                                if (buscarCuenta.concepniff == 1)
//                                {
//                                    movNuevo.credito = 0;
//                                    movNuevo.debito = Convert.ToDecimal(total);

//                                    movNuevo.creditoniif = 0;
//                                    movNuevo.debitoniif = Convert.ToDecimal(total);
//                                }

//                                if (buscarCuenta.concepniff == 4)
//                                {
//                                    movNuevo.creditoniif = 0;
//                                    movNuevo.debitoniif = Convert.ToDecimal(total);
//                                }

//                                if (buscarCuenta.concepniff == 5)
//                                {
//                                    movNuevo.credito = 0;
//                                    movNuevo.debito = Convert.ToDecimal(total);
//                                }
//                            }
//                            if (parametro.id_nombre_parametro == 16)
//                            {
//                                /*if (info.aplicaniff==true)
//                                {

//                                }*/

//                                if (info.manejabase == true)
//                                {
//                                    movNuevo.basecontable = Convert.ToDecimal(total);
//                                }
//                                else
//                                {
//                                    movNuevo.basecontable = 0;
//                                }

//                                if (info.documeto == true)
//                                {
//                                    movNuevo.documento = rc.documento;
//                                }

//                                if (buscarCuenta.concepniff == 1)
//                                {
//                                    movNuevo.credito = Convert.ToDecimal(total);
//                                    movNuevo.debito = 0;

//                                    movNuevo.creditoniif = Convert.ToDecimal(total);
//                                    movNuevo.debitoniif = 0;
//                                }

//                                if (buscarCuenta.concepniff == 4)
//                                {
//                                    movNuevo.creditoniif = Convert.ToDecimal(total);
//                                    movNuevo.debitoniif = 0;
//                                }

//                                if (buscarCuenta.concepniff == 5)
//                                {
//                                    movNuevo.credito = Convert.ToDecimal(total);
//                                    movNuevo.debito = 0;
//                                }
//                            }
//                            //if (parametro.id_nombre_parametro == 11)
//                            //{
//                            //    /*if (info.aplicaniff == true)
//                            //    {

//                            //    }*/

//                            //    if (info.manejabase == true)
//                            //    {
//                            //        movNuevo.basecontable = Convert.ToDecimal(total);
//                            //    }
//                            //    else
//                            //    {
//                            //        movNuevo.basecontable = 0;
//                            //    }

//                            //    if (info.documeto == true)
//                            //    {
//                            //        movNuevo.documento = rc.documento;
//                            //    }

//                            //    if (buscarCuenta.concepniff == 1)
//                            //    {
//                            //        movNuevo.credito = 0;
//                            //        movNuevo.debito = Convert.ToDecimal(total);

//                            //        movNuevo.creditoniif = 0;
//                            //        movNuevo.debitoniif = Convert.ToDecimal(total);
//                            //    }

//                            //    if (buscarCuenta.concepniff == 4)
//                            //    {
//                            //        movNuevo.creditoniif = 0;
//                            //        movNuevo.debitoniif = Convert.ToDecimal(total);
//                            //    }

//                            //    if (buscarCuenta.concepniff == 5)
//                            //    {
//                            //        movNuevo.credito = 0;
//                            //        movNuevo.debito = Convert.ToDecimal(total);
//                            //    }
//                            //}
//                            //if (parametro.id_nombre_parametro == 2)
//                            //{
//                            //    /*if (info.aplicaniff == true)
//                            //    {

//                            //    }*/

//                            //    if (info.manejabase == true)
//                            //    {
//                            //        movNuevo.basecontable = Convert.ToDecimal(total);
//                            //    }
//                            //    else
//                            //    {
//                            //        movNuevo.basecontable = 0;
//                            //    }

//                            //    if (info.documeto == true)
//                            //    {
//                            //        movNuevo.documento = rc.documento;
//                            //    }

//                            //    if (buscarCuenta.concepniff == 1)
//                            //    {
//                            //        movNuevo.credito = 0;
//                            //        movNuevo.debito = Convert.ToDecimal(rc.iva);
//                            //        movNuevo.creditoniif = 0;
//                            //        movNuevo.debitoniif = Convert.ToDecimal(rc.iva);
//                            //    }

//                            //    if (buscarCuenta.concepniff == 4)
//                            //    {
//                            //        movNuevo.creditoniif = 0;
//                            //        movNuevo.debitoniif = Convert.ToDecimal(rc.iva);
//                            //    }

//                            //    if (buscarCuenta.concepniff == 5)
//                            //    {
//                            //        movNuevo.credito = 0;
//                            //        movNuevo.debito = Convert.ToDecimal(rc.iva);
//                            //    }
//                            //}
//                            //if (parametro.id_nombre_parametro == 3)
//                            //{
//                            //    /*if (info.aplicaniff == true)
//                            //    {

//                            //    }*/

//                            //    if (info.manejabase == true)
//                            //    {
//                            //        movNuevo.basecontable = Convert.ToDecimal(total);
//                            //    }
//                            //    else
//                            //    {
//                            //        movNuevo.basecontable = 0;
//                            //    }

//                            //    if (info.documeto == true)
//                            //    {
//                            //        movNuevo.documento = rc.documento;
//                            //    }

//                            //    if (buscarCuenta.concepniff == 1)
//                            //    {
//                            //        movNuevo.credito = Convert.ToDecimal(rc.retencion);
//                            //        movNuevo.debito = 0;

//                            //        movNuevo.creditoniif = Convert.ToDecimal(rc.retencion);
//                            //        movNuevo.debitoniif = 0;
//                            //    }

//                            //    if (buscarCuenta.concepniff == 4)
//                            //    {
//                            //        movNuevo.creditoniif = Convert.ToDecimal(rc.retencion);
//                            //        movNuevo.debitoniif = 0;
//                            //    }

//                            //    if (buscarCuenta.concepniff == 5)
//                            //    {
//                            //        movNuevo.credito = Convert.ToDecimal(rc.retencion);
//                            //        movNuevo.debito = 0;
//                            //    }
//                            //}
//                            //if (parametro.id_nombre_parametro == 4)
//                            //{
//                            //    /*if (info.aplicaniff == true)
//                            //    {

//                            //    }*/

//                            //    if (info.manejabase == true)
//                            //    {
//                            //        movNuevo.basecontable = Convert.ToDecimal(rc.iva);
//                            //    }
//                            //    else
//                            //    {
//                            //        movNuevo.basecontable = 0;
//                            //    }

//                            //    if (info.documeto == true)
//                            //    {
//                            //        movNuevo.documento = rc.documento;
//                            //    }

//                            //    if (buscarCuenta.concepniff == 1)
//                            //    {
//                            //        movNuevo.credito = Convert.ToDecimal(rc.retencion_iva);
//                            //        movNuevo.debito = 0;

//                            //        movNuevo.creditoniif = Convert.ToDecimal(rc.retencion_iva);
//                            //        movNuevo.debitoniif = 0;
//                            //    }

//                            //    if (buscarCuenta.concepniff == 4)
//                            //    {
//                            //        movNuevo.creditoniif = Convert.ToDecimal(rc.retencion_iva);
//                            //        movNuevo.debitoniif = 0;
//                            //    }

//                            //    if (buscarCuenta.concepniff == 5)
//                            //    {
//                            //        movNuevo.credito = Convert.ToDecimal(rc.retencion_iva);
//                            //        movNuevo.debito = 0;
//                            //    }
//                            //}
//                            //if (parametro.id_nombre_parametro == 5)
//                            //{
//                            //    /*if (info.aplicaniff == true)
//                            //    {

//                            //    }*/

//                            //    if (info.manejabase == true)
//                            //    {
//                            //        movNuevo.basecontable = Convert.ToDecimal(total);
//                            //    }
//                            //    else
//                            //    {
//                            //        movNuevo.basecontable = 0;
//                            //    }

//                            //    if (info.documeto == true)
//                            //    {
//                            //        movNuevo.documento = rc.documento;
//                            //    }

//                            //    if (buscarCuenta.concepniff == 1)
//                            //    {
//                            //        movNuevo.credito = Convert.ToDecimal(rc.retencion_ica);
//                            //        movNuevo.debito = 0;
//                            //        movNuevo.creditoniif = Convert.ToDecimal(rc.retencion_ica);
//                            //        movNuevo.debitoniif = 0;
//                            //    }

//                            //    if (buscarCuenta.concepniff == 4)
//                            //    {
//                            //        movNuevo.creditoniif = Convert.ToDecimal(rc.retencion_ica);
//                            //        movNuevo.debitoniif = 0;
//                            //    }

//                            //    if (buscarCuenta.concepniff == 5)
//                            //    {
//                            //        movNuevo.credito = Convert.ToDecimal(rc.retencion_ica);
//                            //        movNuevo.debito = 0;
//                            //    }
//                            //}

//                            secuencia++;

//                            //Cuentas valores
//                            var buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x => x.centro == parametro.centro && x.cuenta == parametro.cuenta && x.nit == movNuevo.nit);
//                            if (buscar_cuentas_valores != null)
//                            {
//                                buscar_cuentas_valores.debito += movNuevo.debito;
//                                buscar_cuentas_valores.credito += movNuevo.credito;
//                                buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
//                                buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
//                                context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
//                            }
//                            else
//                            {
//                                var fechaHoy = DateTime.Now;
//                                var crearCuentaValor = new cuentas_valores();
//                                crearCuentaValor.ano = fechaHoy.Year;
//                                crearCuentaValor.mes = fechaHoy.Month;
//                                crearCuentaValor.cuenta = movNuevo.cuenta;
//                                crearCuentaValor.centro = movNuevo.centro;
//                                crearCuentaValor.nit = movNuevo.nit;
//                                crearCuentaValor.debito = movNuevo.debito;
//                                crearCuentaValor.credito = movNuevo.credito;
//                                crearCuentaValor.debitoniff = movNuevo.debitoniif;
//                                crearCuentaValor.creditoniff = movNuevo.creditoniif;
//                                context.cuentas_valores.Add(crearCuentaValor);
//                            }
//                            context.mov_contable.Add(movNuevo);

//                            totalCreditos += movNuevo.credito;
//                            totalDebitos += movNuevo.debito;

//                            listaDescuadrados.Add(new DocumentoDescuadradoModel()
//                            {
//                                NumeroCuenta = "(" + info.cntpuc_numero + ")" + info.cntpuc_descp,
//                                DescripcionParametro = descripcionParametro,
//                                ValorDebito = movNuevo.debito,
//                                ValorCredito = movNuevo.credito
//                            });
//                        }
//                    }
//                }

//                if (totalDebitos != totalCreditos)
//                {
//                    TempData["mensaje_error"] = "El documento no tiene los movimientos calculados correctamente, verifique el perfil del documento";

//                    ViewBag.documentoSeleccionado = encabezado.tipo;
//                    ViewBag.bodegaSeleccionado = encabezado.bodega;
//                    ViewBag.perfilSeleccionado = encabezado.perfilcontable;

//                    ViewBag.documentoDescuadrado = listaDescuadrados;
//                    ViewBag.calculoDebito = totalDebitos;
//                    ViewBag.calculoCredito = totalCreditos;

//                    listas();
//                    return View(rc);
//                }
//                else
//                {
//                    try
//                    {
//                        context.SaveChanges();
//                    }
//                    catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
//                    {
//                        Exception raise = dbEx;
//                        foreach (var validationErrors in dbEx.EntityValidationErrors)
//                        {
//                            foreach (var validationError in validationErrors.ValidationErrors)
//                            {
//                                string message = string.Format("{0}:{1}",
//                                    validationErrors.Entry.Entity.ToString(),
//                                    validationError.ErrorMessage);
//                                // raise a new exception nesting
//                                // the current instance as InnerException
//                                raise = new InvalidOperationException(message, raise);
//                            }
//                        }
//                        throw raise;
//                    }
//                    TempData["mensaje"] = "registro creado correctamente";
//                    DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
//                    doc.ActualizarConsecutivo(grupo.grupo, consecutivo);

//                    return RedirectToAction("Create");
//                }

//            }
//            else
//            {
//                TempData["mensaje_error"] = "no hay consecutivo";
//            }

//        } /////esto es de si hay registros 
//    }
//    else
//    {
//        TempData["mensaje_error"] = "No fue posible guardar el registro, por favor valide";
//        var errors = ModelState.Select(x => x.Value.Errors)
//                               .Where(y => y.Count > 0)
//                               .ToList();
//    }
//    listas();
//    return View(rc);
//}


////////var lista_facturas = Convert.ToInt32(Request["listaFacturas"]);
////////    for (int i = 1; i <= lista_facturas; i++)
////////    {
////////        var tipo = Convert.ToInt32(Request["tipo" + i]);
////////        var numero = Convert.ToInt32(Request["numero" + i]);
////////        var id = Convert.ToInt32(Request["id" + i]);
////////        var valor = Convert.ToDecimal(Request["valor_aPagar" + i]);

////////        var factura = context.encab_documento.Where(d => d.idencabezado == id && d.tipo == tipo).FirstOrDefault();

////////        if (valor != 0)
////////        {
////////            var valoraplicado = Convert.ToInt32(factura.valor_aplicado) ;

////////            var nuevovalor = Convert.ToDecimal(valoraplicado) + valor;
////////            factura.valor_aplicado = Convert.ToDecimal(nuevovalor);
////////            context.Entry(factura).State = EntityState.Modified;

////////            // si al crear la nota credito se selecciono factura se debe guardar el registro tambien en cruce documentos, de lo contrario NO
////////            //Cruce documentos
////////            cruce_documentos cd = new cruce_documentos();
////////            cd.idtipo = rc.tipo;
////////            cd.numero = consecutivo;
////////            //tipo de la factura cruzada
////////            cd.idtipoaplica = Convert.ToInt32(tipo);
////////            //numero de la factura cruzada
////////            cd.numeroaplica = Convert.ToInt32(numero);
////////            //valor aplicado a cada factura
////////            cd.valor = Convert.ToDecimal(valor);
////////            cd.fecha = DateTime.Now;
////////            cd.fechacruce = DateTime.Now;
////////            cd.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
////////            context.cruce_documentos.Add(cd);

////////    // agrego los pagos de las facturas en documentos pago
////////    //pagos
////////    var lista_pagos = Convert.ToInt32(Request["lista_pagos"]);
////////            for (int j = 1; j <= lista_pagos; j++)
////////            {
////////                if (!string.IsNullOrEmpty(Request["fpago" + j]))
////////                {
////////                    documentos_pago dpago = new documentos_pago();
////////                    dpago.idtencabezado = rc.idencabezado;
////////                    dpago.tercero = rc.nitp;
////////                    dpago.fecha = DateTime.Now;
////////                    dpago.forma_pago = Convert.ToInt32(Request["fpago" + j]);
////////                    dpago.valor = Convert.ToDecimal(Request["valor" + j]);
////////                    dpago.cuenta_banco = Convert.ToString(Request["cuenta" + j]);
////////                    dpago.documento = Convert.ToString(Request["cheque" + j]);
////////                    dpago.notas = Request["observaciones" + j];
////////                    var banco = Request["banco" + j];
////////                    if (!string.IsNullOrEmpty(Request["banco" + j]))
////////                    {
////////                        dpago.banco = Convert.ToInt32(Request["banco" + j]);
////////                    }
////////                    context.documentos_pago.Add(dpago);
////////                }
////////            }
////////        }
////////    }
//}
//else
//{
//    // agrego los pagos del anticipo en documentos pago
//    //pagos
//    var lista_pagos = Convert.ToInt32(Request["lista_pagos"]);
//    for (int j = 1; j <= lista_pagos; j++)
//    {
//        if (!string.IsNullOrEmpty(Request["fpago" + j]))
//        {
//            documentos_pago dpago = new documentos_pago();
//            dpago.idtencabezado = rc.idencabezado;
//            dpago.tercero = rc.nit;
//            dpago.fecha = DateTime.Now;
//            dpago.forma_pago = Convert.ToInt32(Request["fpago" + j]);
//            dpago.valor = Convert.ToDecimal(Request["valor" + j]);
//            dpago.cuenta_banco = Convert.ToString(Request["cuenta" + j]);
//            dpago.documento = Convert.ToString(Request["cheque" + j]);
//            dpago.notas = Request["observaciones" + j];
//            var banco = Request["banco" + j];
//            if (!string.IsNullOrEmpty(Request["banco" + j]))
//            {
//                dpago.banco = Convert.ToInt32(Request["banco" + j]);
//            }
//            context.documentos_pago.Add(dpago);
//        }
//    }
//}