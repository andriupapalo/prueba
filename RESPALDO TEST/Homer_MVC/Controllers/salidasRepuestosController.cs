﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Rotativa;

namespace Homer_MVC.Controllers
{
    public class salidasRepuestosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");


        // GET: salidasRepuestos
        public ActionResult Create(int? menu)
        {
            ListasDesplegables();
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Create(EntradaSalidaModel modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                {
                    try
                    {
                        int documentointerno = 0;
                        grupoconsecutivos grupo2 = new grupoconsecutivos();
                        long consecutivo2 = 0;
                        DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                        long numeroConsecutivo = 0;
                        grupoconsecutivos buscarGrupoConsecutivos = context.grupoconsecutivos.FirstOrDefault(x =>
                            x.documento_id == modelo.TipoDocumento && x.bodega_id == modelo.BodegaOrigen);
                        int numeroGrupo = buscarGrupoConsecutivos != null ? buscarGrupoConsecutivos.grupo : 0;

                        ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
                        icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
                        tp_doc_registros buscarTipoDocRegistro =
                            context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == modelo.TipoDocumento);
                        numeroConsecutivoAux = gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro, modelo.TipoDocumento,
                            modelo.BodegaOrigen);
                        //var numeroConsecutivoAux = context.icb_doc_consecutivos.OrderByDescending(x => x.doccons_ano).FirstOrDefault(x => x.doccons_idtpdoc == modelo.TipoDocumento && x.doccons_bodega == modelo.BodegaOrigen);
                        if (numeroConsecutivoAux != null)
                        {
                            numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                        }
                        else
                        {
                            dbTran.Rollback();
                            TempData["mensaje_error"] = "No existe un numero consecutivo asignado para este tipo de documento";
                            ListasDesplegables();
                            ViewBag.documentoSeleccionado = modelo.TipoDocumento;
                            ViewBag.bodegaSeleccionada = modelo.BodegaOrigen;
                            ViewBag.perfilSeleccionado = modelo.PerfilContable;
                            BuscarFavoritos(menu);
                            return View(modelo);
                        }

                        // Si llega aqui significa que si hay un numero de documento consecutivo 
                        decimal sumaCostosReferenciasAux = 0;
                        int numero_repuestosAux = Convert.ToInt32(Request["lista_repuestos"]);
                        ;
                        for (int i = 1; i <= numero_repuestosAux; i++)
                        {
                            string referencia_codigo = Request["codigo_referencia" + i];
                            string referencia_cantidad = Request["cantidad_referencia" + i];
                            string referencia_costo = Request["costo_referencia" + i];
                            if (string.IsNullOrEmpty(referencia_codigo) || string.IsNullOrEmpty(referencia_cantidad))
                            {
                                // Significa que la agregaron y la eliminaron
                            }
                            else
                            {
                                sumaCostosReferenciasAux += Convert.ToDecimal(referencia_costo,miCultura);
                            }
                        }


                        var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                                          join nombreParametro in context.paramcontablenombres
                                                              on perfil.id_nombre_parametro equals nombreParametro.id
                                                          join cuenta in context.cuenta_puc
                                                              on perfil.cuenta equals cuenta.cntpuc_id
                                                          where perfil.id_perfil == modelo.PerfilContable
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
                        List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();
                        decimal calcularDebito = 0;
                        decimal calcularCredito = 0;

                        foreach (var parametro in parametrosCuentasVerificar)
                        {
                            cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
                            if (buscarCuenta != null)
                            {
                                if (parametro.id_nombre_parametro == 9 || parametro.id_nombre_parametro == 12 ||
                                    parametro.id_nombre_parametro == 13)
                                {
                                    if (parametro.id_nombre_parametro == 9)
                                    {
                                        if (buscarCuenta.concepniff == 1 || buscarCuenta.concepniff == 5)
                                        {
                                            if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                            {
                                                calcularCredito += sumaCostosReferenciasAux;
                                                listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                {
                                                    NumeroCuenta = parametro.cntpuc_numero,
                                                    DescripcionParametro = parametro.descripcion_parametro,
                                                    ValorDebito = 0,
                                                    ValorCredito = sumaCostosReferenciasAux
                                                });
                                            }

                                            if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                            {
                                                calcularDebito += sumaCostosReferenciasAux;
                                                listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                {
                                                    NumeroCuenta = parametro.cntpuc_numero,
                                                    DescripcionParametro = parametro.descripcion_parametro,
                                                    ValorDebito = sumaCostosReferenciasAux,
                                                    ValorCredito = 0
                                                });
                                            }
                                        }
                                        else
                                        {
                                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                                            {
                                                NumeroCuenta = parametro.cntpuc_numero,
                                                DescripcionParametro = parametro.descripcion_parametro,
                                                ValorDebito = 0,
                                                ValorCredito = 0
                                            });
                                        }
                                    }
                                    else
                                    {
                                        if (buscarCuenta.concepniff == 1 || buscarCuenta.concepniff == 5)
                                        {
                                            if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                            {
                                                calcularDebito += sumaCostosReferenciasAux;
                                                listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                {
                                                    NumeroCuenta = parametro.cntpuc_numero,
                                                    DescripcionParametro = parametro.descripcion_parametro,
                                                    ValorDebito = sumaCostosReferenciasAux,
                                                    ValorCredito = 0
                                                });
                                            }

                                            if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                            {
                                                calcularCredito += sumaCostosReferenciasAux;
                                                listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                {
                                                    NumeroCuenta = parametro.cntpuc_numero,
                                                    DescripcionParametro = parametro.descripcion_parametro,
                                                    ValorDebito = 0,
                                                    ValorCredito = sumaCostosReferenciasAux
                                                });
                                            }
                                        }
                                        else
                                        {
                                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                                            {
                                                NumeroCuenta = parametro.cntpuc_numero,
                                                DescripcionParametro = parametro.descripcion_parametro,
                                                ValorDebito = 0,
                                                ValorCredito = 0
                                            });
                                        }
                                    }
                                }
                            }
                        }

                        if (calcularCredito != calcularDebito)
                        {
                            dbTran.Rollback();
                            TempData["documento_descuadrado"] =
                                "El documento no tiene los movimientos calculados correctamente";
                            ViewBag.documentoSeleccionado = modelo.TipoDocumento;
                            ViewBag.bodegaSeleccionada = modelo.BodegaOrigen;
                            ViewBag.perfilSeleccionado = modelo.PerfilContable;

                            ViewBag.documentoDescuadrado = listaDescuadrados;
                            ViewBag.calculoDebito = calcularDebito;
                            ViewBag.calculoCredito = calcularCredito;
                            ListasDesplegables();
                            BuscarFavoritos(menu);
                            return View(modelo);
                        }

                        icb_sysparameter buscarNit = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P33");
                        int nitTraslado = buscarNit != null ? Convert.ToInt32(buscarNit.syspar_value) : 0;

                        encab_documento crearEncabezado = new encab_documento
                        {
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            tipo = modelo.TipoDocumento,
                            bodega = modelo.BodegaOrigen,
                            numero = numeroConsecutivo,
                            nit = nitTraslado,
                            //crearEncabezado.documento = modelo.Referencia; // Puede haber varias referencias
                            fecha = DateTime.Now,
                            fec_creacion = DateTime.Now,
                            impoconsumo = 0,
                            perfilcontable = modelo.PerfilContable
                        };
                        context.encab_documento.Add(crearEncabezado);
                        var guardar2 = context.SaveChanges();
                        //veo si el documento externo tiene documento interno asociado
                        tp_doc_registros doc_interno = context.tp_doc_registros.Where(d => d.tpdoc_id == modelo.TipoDocumento).FirstOrDefault();
                        if (doc_interno.doc_interno_asociado != null)
                        {
                            //se consulta consecutivo de documento interno
                            grupo2 = context.grupoconsecutivos.FirstOrDefault(x => x.documento_id == doc_interno.doc_interno_asociado && x.bodega_id == modelo.BodegaOrigen);
                            if (grupo2 != null)
                            {
                                consecutivo2 = doc.BuscarConsecutivo(grupo2.grupo);
                                //calculo y guardo el encabezado del movimiento interno
                                encab_documento encabezado2 = new encab_documento
                                {
                                    tipo = doc_interno.doc_interno_asociado.Value,
                                    numero = consecutivo2,
                                    nit = nitTraslado,
                                    fecha = DateTime.Now,
                                    fpago_id = crearEncabezado.fpago_id,
                                    vencimiento = crearEncabezado.vencimiento,
                                    valor_total = crearEncabezado.valor_total,
                                    iva = crearEncabezado.iva,
                                    porcen_retencion = crearEncabezado.porcen_retencion,
                                    retencion = crearEncabezado.retencion,
                                    porcen_reteiva = crearEncabezado.porcen_reteiva,
                                    retencion_iva = crearEncabezado.retencion_iva,
                                    porcen_retica = crearEncabezado.porcen_retica,
                                    retencion_ica = crearEncabezado.retencion_ica,
                                    fletes = crearEncabezado.fletes,
                                    iva_fletes = crearEncabezado.iva_fletes,
                                    costo = crearEncabezado.costo,
                                    vendedor = crearEncabezado.vendedor,
                                    documento = crearEncabezado.documento,
                                    remision = crearEncabezado.remision,
                                    bodega = crearEncabezado.bodega,
                                    concepto = crearEncabezado.concepto,
                                    moneda = crearEncabezado.moneda,
                                    perfilcontable = crearEncabezado.perfilcontable,
                                    valor_mercancia = crearEncabezado.valor_mercancia,
                                    fec_creacion = crearEncabezado.fec_creacion,
                                    userid_creacion = crearEncabezado.userid_creacion,
                                    estado = true,
                                    concepto2 = crearEncabezado.concepto2,
                                    id_movimiento_interno = crearEncabezado.idencabezado,
                                };
                                context.encab_documento.Add(encabezado2);
                                context.SaveChanges();
                                documentointerno = encabezado2.idencabezado;
                            }
                        }

                        encab_documento buscarUltimoEncabezado =
                           context.encab_documento.OrderByDescending(x => x.idencabezado).Where(d => d.idencabezado == crearEncabezado.idencabezado).FirstOrDefault();

                        decimal sumaCostosReferencias = 0;
                        int numero_repuestos = Convert.ToInt32(Request["lista_repuestos"]);
                        int sequ = 1;
                        for (int i = 1; i <= numero_repuestos; i++)
                        {
                            string referencia_codigo = Request["codigo_referencia" + i];
                            string referencia_cantidad = Request["cantidad_referencia" + i];
                            string referencia_costo = Request["costo_referencia" + i];
                            if (string.IsNullOrEmpty(referencia_codigo) || string.IsNullOrEmpty(referencia_cantidad))
                            {
                                // Significa que la agregaron y la eliminaron
                            }
                            else
                            {
                                sumaCostosReferencias += Convert.ToDecimal(referencia_costo,miCultura);

                                //Calculo de costo promedio
                                CsCalcularCostoPromedio calcular = new CsCalcularCostoPromedio();
                                decimal costo_promedio = 0;
                                //verifico la naturaleza del documento interno asociado
                                var entrada = false;
                                if (doc_interno.doc_interno_asociado != null)
                                {//calculo el comportamiento del documento interno asociado

                                    var docinternoaso = context.tp_doc_registros.Where(d => d.tpdoc_id == doc_interno.doc_interno_asociado.Value).FirstOrDefault();
                                    if (docinternoaso.entrada_salida != null)
                                    {
                                        entrada = docinternoaso.entrada_salida.Value;
                                    }
                                }
                                referencias_inven buscarReferenciasInvenOrigen = context.referencias_inven.FirstOrDefault(x =>
                                    x.codigo == referencia_codigo && x.ano == DateTime.Now.Year &&
                                    x.mes == DateTime.Now.Month && x.bodega == modelo.BodegaOrigen);
                                if (buscarReferenciasInvenOrigen != null)
                                {
                                    if (entrada == false)
                                    {
                                        buscarReferenciasInvenOrigen.can_sal =
                                       buscarReferenciasInvenOrigen.can_sal + Convert.ToInt32(referencia_cantidad);
                                        buscarReferenciasInvenOrigen.cos_sal =
                                            buscarReferenciasInvenOrigen.cos_sal + (!string.IsNullOrEmpty(referencia_costo)
                                                ? Convert.ToDecimal(referencia_costo,miCultura) * Convert.ToDecimal(referencia_cantidad,miCultura)
                                                : 0);
                                        buscarReferenciasInvenOrigen.can_otr_sal =
                                            buscarReferenciasInvenOrigen.can_otr_sal + Convert.ToInt32(referencia_cantidad);
                                        buscarReferenciasInvenOrigen.cos_otr_sal =
                                            buscarReferenciasInvenOrigen.cos_otr_sal + (!string.IsNullOrEmpty(referencia_costo)
                                                ? Convert.ToDecimal(referencia_costo,miCultura) * Convert.ToDecimal(referencia_cantidad,miCultura)
                                                : 0);
                                    }
                                    else
                                    {
                                        costo_promedio= calcular.calcularCostoPromedio(Convert.ToDecimal(Request["cantidad" + i], miCultura), Convert.ToDecimal(referencia_costo, miCultura),
                                        referencia_codigo, modelo.BodegaOrigen);
                                        buscarReferenciasInvenOrigen.costo_prom = costo_promedio;
                                        buscarReferenciasInvenOrigen.can_ent =
                                       buscarReferenciasInvenOrigen.can_ent + Convert.ToInt32(referencia_cantidad);
                                        buscarReferenciasInvenOrigen.cos_ent =
                                            buscarReferenciasInvenOrigen.cos_ent + (!string.IsNullOrEmpty(referencia_costo)
                                                ? Convert.ToDecimal(referencia_costo,miCultura) * Convert.ToDecimal(referencia_cantidad,miCultura)
                                                : 0);
                                        buscarReferenciasInvenOrigen.can_otr_ent =
                                            buscarReferenciasInvenOrigen.can_otr_ent + Convert.ToInt32(referencia_cantidad);
                                        buscarReferenciasInvenOrigen.cos_otr_ent =
                                            buscarReferenciasInvenOrigen.cos_otr_ent + (!string.IsNullOrEmpty(referencia_costo)
                                                ? Convert.ToDecimal(referencia_costo) * Convert.ToDecimal(referencia_cantidad,miCultura)
                                                : 0);
                                    }
                                   
                                    context.Entry(buscarReferenciasInvenOrigen).State = EntityState.Modified;
                                }
                                else
                                {
                                    referencias_inven crearReferencia = new referencias_inven
                                    {
                                        bodega = modelo.BodegaOrigen,
                                        codigo = referencia_codigo,
                                        ano = (short)DateTime.Now.Year,
                                        mes = (short)DateTime.Now.Month,
                                       
                                        modulo = "R"
                                    };
                                    if (entrada == false)
                                    {
                                        crearReferencia.can_ini = 0;
                                        crearReferencia.can_sal = Convert.ToInt32(referencia_cantidad);
                                        crearReferencia.cos_sal = !string.IsNullOrEmpty(referencia_costo)
                                            ? Convert.ToDecimal(referencia_costo,miCultura)
                                            : 0;
                                        crearReferencia.can_otr_sal = Convert.ToInt32(referencia_cantidad);
                                        crearReferencia.cos_otr_sal = !string.IsNullOrEmpty(referencia_costo)
                                            ? Convert.ToDecimal(referencia_costo,miCultura)
                                            : 0;
                                    }
                                    else
                                    {
                                        crearReferencia.can_ini = 0;
                                        crearReferencia.can_ent = Convert.ToInt32(referencia_cantidad);
                                        crearReferencia.cos_ent = !string.IsNullOrEmpty(referencia_costo)
                                            ? Convert.ToDecimal(referencia_costo, miCultura)
                                            : 0;
                                        crearReferencia.can_otr_ent = Convert.ToInt32(referencia_cantidad);
                                        crearReferencia.cos_otr_ent = !string.IsNullOrEmpty(referencia_costo)
                                            ? Convert.ToDecimal(referencia_costo, miCultura)
                                            : 0;
                                        costo_promedio = calcular.calcularCostoPromedio(Convert.ToDecimal(Request["cantidad" + i], miCultura), Convert.ToDecimal(referencia_costo, miCultura),
                                        referencia_codigo, modelo.BodegaOrigen);
                                        crearReferencia.costo_prom = costo_promedio;

                                    }
                                    context.referencias_inven.Add(crearReferencia);
                                }


                                lineas_documento crearLineasOrigen = new lineas_documento
                                {
                                    codigo = referencia_codigo,
                                    fec_creacion = DateTime.Now,
                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                    nit = nitTraslado,
                                    cantidad = !string.IsNullOrEmpty(referencia_cantidad)
                                        ? Convert.ToDecimal(referencia_cantidad, miCultura)
                                        : 0,
                                    bodega = modelo.BodegaOrigen,
                                    seq = sequ,
                                    estado = true,
                                    fec = DateTime.Now,
                                    costo_unitario = !string.IsNullOrEmpty(referencia_costo)
                                        ? Convert.ToDecimal(referencia_costo, miCultura)
                                        : 0,
                                    id_encabezado = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0
                                };
                                context.lineas_documento.Add(crearLineasOrigen);
                                var guardar4 = context.SaveChanges();

                                if (doc_interno.doc_interno_asociado != null && documentointerno != 0)
                                {
                                    lineas_documento lineas2 = new lineas_documento
                                    {
                                        id_encabezado = documentointerno,
                                        codigo = crearLineasOrigen.codigo,
                                        seq = sequ,
                                        fec = DateTime.Now,
                                        nit = crearLineasOrigen.nit,
                                        cantidad = crearLineasOrigen.cantidad,
                                        porcentaje_iva = crearLineasOrigen.porcentaje_iva,
                                        valor_unitario = crearLineasOrigen.valor_unitario,
                                        porcentaje_descuento = crearLineasOrigen.porcentaje_descuento,
                                        costo_unitario = crearLineasOrigen.costo_unitario,
                                        bodega = crearLineasOrigen.bodega,
                                        fec_creacion = crearLineasOrigen.fec_creacion,
                                        userid_creacion = crearLineasOrigen.userid_creacion,
                                        tasa = crearLineasOrigen.tasa,
                                    };

                                    context.lineas_documento.Add(lineas2);
                                    var guardar5 = context.SaveChanges();

                                }

                                sequ++;
                            }
                        }


                        int secuencia = 1;
                        foreach (var parametro in parametrosCuentasVerificar)
                        {
                            cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
                            if (buscarCuenta != null)
                            {
                                if (parametro.id_nombre_parametro == 9 || parametro.id_nombre_parametro == 12 ||
                                    parametro.id_nombre_parametro == 13)
                                {
                                    mov_contable movNuevo = new mov_contable
                                    {
                                        id_encab =
                                        buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0,
                                        fec_creacion = DateTime.Now,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                        idparametronombre = parametro.id_nombre_parametro,
                                        cuenta = parametro.cuenta,
                                        centro = parametro.centro,
                                        fec = DateTime.Now,
                                        seq = secuencia
                                    };

                                    // La validacion se hace ya que cuando el parametro es 9 (es decir inventario) se invierte el credito con debito
                                    if (parametro.id_nombre_parametro == 9)
                                    {
                                        if (buscarCuenta.concepniff == 1)
                                        {
                                            if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                            {
                                                movNuevo.credito = sumaCostosReferencias;
                                                movNuevo.creditoniif = sumaCostosReferencias;
                                            }

                                            if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                            {
                                                movNuevo.debitoniif = sumaCostosReferencias;
                                                movNuevo.debito = sumaCostosReferencias;
                                            }
                                        }

                                        if (buscarCuenta.concepniff == 4)
                                        {
                                            if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                            {
                                                movNuevo.creditoniif = sumaCostosReferencias;
                                            }

                                            if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                            {
                                                movNuevo.debitoniif = sumaCostosReferencias;
                                            }
                                        }

                                        if (buscarCuenta.concepniff == 5)
                                        {
                                            if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                            {
                                                movNuevo.credito = sumaCostosReferencias;
                                            }

                                            if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                            {
                                                movNuevo.debito = sumaCostosReferencias;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (buscarCuenta.concepniff == 1)
                                        {
                                            if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                            {
                                                movNuevo.debitoniif = sumaCostosReferencias;
                                                movNuevo.debito = sumaCostosReferencias;
                                            }

                                            if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                            {
                                                movNuevo.credito = sumaCostosReferencias;
                                                movNuevo.creditoniif = sumaCostosReferencias;
                                            }
                                        }

                                        if (buscarCuenta.concepniff == 4)
                                        {
                                            if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                            {
                                                movNuevo.debitoniif = sumaCostosReferencias;
                                            }

                                            if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                            {
                                                movNuevo.creditoniif = sumaCostosReferencias;
                                            }
                                        }

                                        if (buscarCuenta.concepniff == 5)
                                        {
                                            if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                            {
                                                movNuevo.debito = sumaCostosReferencias;
                                            }

                                            if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                            {
                                                movNuevo.credito = sumaCostosReferencias;
                                            }
                                        }
                                    }

                                    if (buscarCuenta.manejabase)
                                    {
                                        movNuevo.basecontable = sumaCostosReferencias;
                                    }

                                    if (buscarCuenta.documeto)
                                    {
                                        movNuevo.documento = "1";
                                    }

                                    if (buscarCuenta.tercero)
                                    {
                                        movNuevo.nit = nitTraslado;
                                    }
                                    else
                                    {
                                        icb_terceros buscarNitCero = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
                                        movNuevo.nit = buscarNitCero != null ? buscarNitCero.tercero_id : 0;
                                    }

                                    movNuevo.detalle = "Salida de referencias";
                                    secuencia++;
                                    context.mov_contable.Add(movNuevo);
                                }
                            }
                        }


                        int guardarLineasYMovimientos = context.SaveChanges();
                        dbTran.Commit();

                        if (guardarLineasYMovimientos > 0)
                        {
                            if (buscarGrupoConsecutivos != null)
                            {
                                List<icb_doc_consecutivos> numerosConsecutivos = context.icb_doc_consecutivos
                                    .Where(x => x.doccons_grupoconsecutivo == numeroGrupo).ToList();
                                foreach (icb_doc_consecutivos item in numerosConsecutivos)
                                {
                                    item.doccons_siguiente = item.doccons_siguiente + 1;
                                    context.Entry(item).State = EntityState.Modified;
                                }
                                if (documentointerno != 0)
                                {
                                    doc.ActualizarConsecutivo(grupo2.grupo, consecutivo2);
                                }
                                context.SaveChanges();
                            }

                            TempData["mensaje"] = "La salida de la(s) referencia(s) se ha realizado correctamente";
                            ViewBag.numDocumentoCreado = numeroConsecutivo;
                            ListasDesplegables();
                            BuscarFavoritos(menu);
                            //return RedirectToAction("create");
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["mensaje_error"] = ex.InnerException.InnerException.Message;
                    }
                }
            }

            ListasDesplegables();
            BuscarFavoritos(menu);
            return View();
        }


        public JsonResult BuscarReferencias(string referencia_id)
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            var buscarReferencia = (from referencia in context.icb_referencia
                                    join promedio in context.vw_promedio
                                        on referencia.ref_codigo equals promedio.codigo into pro
                                    from promedio in pro.DefaultIfEmpty()
                                    join inventarioHoy in context.vw_inventario_hoy
                                        on referencia.ref_codigo equals inventarioHoy.ref_codigo into hoy
                                    from inventarioHoy in hoy.DefaultIfEmpty()
                                    where referencia.ref_codigo == referencia_id && inventarioHoy.bodega == bodegaActual &&
                                          promedio.ano == DateTime.Now.Year && promedio.mes == DateTime.Now.Month
                                    select new
                                    {
                                        referencia.ref_codigo,
                                        referencia.ref_descripcion,
                                        promedio.Promedio,
                                        inventarioHoy.stock
                                    }).FirstOrDefault();
            if (buscarReferencia == null)
            {
                return Json(new { encontrado = false, buscarReferencia }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { encontrado = true, buscarReferencia }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Ver(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            encab_documento encabezado = context.encab_documento.Find(id);
            if (encabezado == null)
            {
                return HttpNotFound();
            }

            EntradaSalidaModel modelo = new EntradaSalidaModel
            {
                TipoDocumento = encabezado.tipo,
                BodegaOrigen = encabezado.bodega,
                PerfilContable = encabezado.perfilcontable ?? 0,
                encabezado_id = encabezado.idencabezado
            };

            ViewBag.documentoSeleccionado = encabezado.tipo;
            ViewBag.bodegaSeleccionada = encabezado.bodega;
            ViewBag.perfilSeleccionado = encabezado.perfilcontable;

            icb_sysparameter buscarTipoDoc = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P41");
            int id_tipo_doc = buscarTipoDoc != null ? Convert.ToInt32(buscarTipoDoc.syspar_value) : 0;

            ViewBag.tp_doc_registros = context.tp_doc_registros.Where(x => x.tipo == id_tipo_doc && x.interno == false).ToList();
            BuscarFavoritos(menu);
            return View(modelo);
        }

        public ActionResult salidaRespuesto(int? id)
        {
            var buscarSalidaRepuesto = (from doc in context.encab_documento
                                        join bodegacs in context.bodega_concesionario
                                            on doc.bodega equals bodegacs.id
                                        join t in context.tp_doc_registros
                                            on doc.tipo equals t.tpdoc_id
                                        where doc.idencabezado == id
                                        select new
                                        {
                                            fecha = doc.fecha.ToString(),
                                            numeroRegistro = doc.numero,
                                            nombreBodegas = bodegacs.bodccs_nombre,
                                            doc.notas,
                                            nombreDetalle = t.tpdoc_nombre,
                                            TotalTotales = 0
                                        }).FirstOrDefault();

            string root = Server.MapPath("~/Pdf/");
            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
            string path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);
            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");


            SalidaPDF modeloSalidaPDF = new SalidaPDF
            {
                nombreDetalle = buscarSalidaRepuesto.nombreDetalle,
                numeroRegistro = buscarSalidaRepuesto.numeroRegistro,
                fechaRegistro = buscarSalidaRepuesto.fecha,
                nombreBodega = buscarSalidaRepuesto.nombreBodegas,


                referencias = (from doc in context.encab_documento
                               join l in context.lineas_documento
                                   on doc.idencabezado equals l.id_encabezado
                               join r in context.icb_referencia
                                   on l.codigo equals r.ref_codigo
                               where doc.idencabezado == id
                               select new referenciasPDF
                               {
                                   cantidad = l.cantidad,
                                   codigo = l.codigo,
                                   descripcion = r.ref_descripcion,
                                   costo = l.costo_unitario,
                                   total = l.costo_unitario * l.cantidad,
                                   secuencia = l.seq
                               }).ToList()
            };

            modeloSalidaPDF.referencias.ForEach(item2 => item2.total = Convert.ToDecimal(item2.total, miCultura));
            modeloSalidaPDF.referencias.ForEach(item2 => item2.cantidad = Convert.ToInt32(item2.cantidad));
            modeloSalidaPDF.referencias.ForEach(item2 => item2.total = Math.Truncate(item2.total));
            modeloSalidaPDF.referencias.ForEach(item2 => item2.costo = Math.Truncate(item2.costo));


            decimal totales = modeloSalidaPDF.referencias.Sum(d => d.total);
            modeloSalidaPDF.TotalTotales = Convert.ToDecimal(totales, miCultura);


            ViewAsPdf something = new ViewAsPdf("salidaRespuesto", modeloSalidaPDF);
            return something;
        }

        public JsonResult BuscarSalidasRepuestos()
        {
            //var tipoDocSalidasRepuestos = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P42");//************************************************************
            //var id_tipo_documento = tipoDocSalidasRepuestos != null ? Convert.ToInt32(tipoDocSalidasRepuestos.syspar_value) : 0;

            var buscarTraslados = (from encabezado in context.encab_documento
                                   join origen in context.bodega_concesionario
                                       on encabezado.bodega equals origen.id into ori
                                   from origen in ori.DefaultIfEmpty()
                                   join t in context.tp_doc_registros
                                       on encabezado.tipo equals t.tpdoc_id
                                   join tipodocumento in context.tp_doc_registros
                                                                      on encabezado.tipo equals tipodocumento.tpdoc_id
                                   join tipodocsw in context.tp_doc_sw
                                   on tipodocumento.sw equals tipodocsw.tpdoc_id
                                   where tipodocsw.sw == 10
                                   //where encabezado.tipo == id_tipo_documento
                                   select new
                                   {
                                       encabezado.idencabezado,
                                       encabezado.numero,
                                       encabezado.fecha,
                                       origen = origen.bodccs_nombre,
                                       encabezado.notas,
                                       tipodocumento.prefijo,
                                       tipodocumento.tpdoc_nombre,
                                       referencias = (from lineas in context.lineas_documento
                                                      join referencia in context.icb_referencia
                                                          on lineas.codigo equals referencia.ref_codigo
                                                      where lineas.id_encabezado == encabezado.idencabezado
                                                      select new
                                                      {
                                                          lineas.seq,
                                                          lineas.cantidad,
                                                          referencia.ref_descripcion
                                                      }).Distinct().ToList()
                                   }).ToList();

            var data = buscarTraslados.Select(x => new
            {
                x.idencabezado,
                x.numero,
                fecha = x.fecha.ToShortDateString(),
                x.origen,
                notas = x.notas != null ? x.notas : " ",
                documento = x.prefijo + " - " + x.tpdoc_nombre,
                x.referencias
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public void ListasDesplegables()
        {
            icb_sysparameter buscarTipoDoc = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P41");
            int id_tipo_doc = buscarTipoDoc != null ? Convert.ToInt32(buscarTipoDoc.syspar_value) : 0;

            ViewBag.tp_doc_registros = context.tp_doc_registros.Where(x => x.tipo == id_tipo_doc && x.interno == false).ToList();

            //var buscarReferencias = context.icb_referencia.Where(x => x.modulo == "R").ToList();
            //List<SelectListItem> items = new List<SelectListItem>();
            //foreach (var item in buscarReferencias)
            //{
            //    var nombre = "(" + item.ref_codigo + ") " + item.ref_descripcion;
            //    items.Add(new SelectListItem() { Text = nombre, Value = item.ref_codigo });
            //}
            //ViewBag.Referencia = new SelectList(items, "Value", "Text");
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