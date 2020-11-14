using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class notaCreditoProveedorController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: notaCredito
        public ActionResult Browser()
        {
            return View();
        }

        public JsonResult TraeIva(int? nit)
        {
            icb_sysparameter trae = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P70");
            tercero_proveedor SiIva = context.tercero_proveedor.Where(x => x.tercero_id == nit && x.exentoiva).FirstOrDefault();
            if (SiIva != null)
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }

            return Json(trae, JsonRequestBehavior.AllowGet);
            //var trae = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P70");
            //return Json(trae, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDocumentosFiltro(int nit, DateTime? desde, DateTime? hasta, int? id_documento,
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
                                                  join t in context.tp_doc_registros
                                                      on e.tipo equals t.tpdoc_id
                                                  join tp in context.tp_doc_registros_tipo
                                                      on t.tipo equals tp.id
                                                  where listaDocumentos.Contains(t.tpdoc_id)
                                                        && e.valor_total - e.valor_aplicado != 0
                                                        && e.fecha >= desde && e.fecha <= hasta
                                                        && e.numero == factura
                                                        && e.nit == nit
                                                  select new
                                                  {
                                                      id = e.idencabezado,
                                                      e.fecha,
                                                      valor_aplicado = e.valor_aplicado != null ? e.valor_aplicado : 0,
                                                      e.valor_total,
                                                      e.numero,
                                                      e.vencimiento,
                                                      tipo = t.prefijo,
                                                      saldo = e.valor_total - e.valor_aplicado,
                                                      retencion = e.porcen_retencion,
                                                      reteIca = e.porcen_retica,
                                                      reteIva = e.porcen_reteiva,
                                                      tipodoc = t.tpdoc_id,
                                                      descripcion = t.tpdoc_nombre,
                                                      numeroFactura = e.numero,
                                                      t.prefijo,
                                                      tp = e.tipo
                                                  }).ToList();

                    var data = buscarFacturasConSaldo.Select(x => new
                    {
                        x.id,
                        fecha1 = x.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                        x.valor_aplicado,
                        x.valor_total,
                        x.numero,
                        vencimiento1 = x.vencimiento != null
                            ? x.vencimiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                            : "",
                        x.tipo,
                        x.saldo,
                        x.retencion,
                        x.reteIca,
                        x.reteIva,
                        x.tipodoc,
                        x.descripcion,
                        x.numeroFactura,
                        x.prefijo,
                        x.tp
                    });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var buscarFacturasConSaldo = (from e in context.encab_documento
                                                  join t in context.tp_doc_registros
                                                      on e.tipo equals t.tpdoc_id
                                                  join tp in context.tp_doc_registros_tipo
                                                      on t.tipo equals tp.id
                                                  where listaDocumentos.Contains(t.tpdoc_id)
                                                        && e.valor_total - e.valor_aplicado != 0
                                                        && e.fecha >= desde && e.fecha <= hasta
                                                        && e.nit == nit
                                                  select new
                                                  {
                                                      id = e.idencabezado,
                                                      e.fecha,
                                                      valor_aplicado = e.valor_aplicado != null ? e.valor_aplicado : 0,
                                                      e.valor_total,
                                                      e.numero,
                                                      e.vencimiento,
                                                      tipo = t.prefijo,
                                                      saldo = e.valor_total - (e.valor_aplicado != null ? e.valor_aplicado : 0),
                                                      retencion = e.porcen_retencion,
                                                      reteIca = e.porcen_retica,
                                                      reteIva = e.porcen_reteiva,
                                                      tipodoc = t.tpdoc_id,
                                                      descripcion = t.tpdoc_nombre,
                                                      numeroFactura = e.numero, // numero = 7
                                                      prefijo = e.idencabezado, // t.prefijo,
                                                      tp = e.tipo // tipo = 3041
                                                  }).ToList();

                    var data = buscarFacturasConSaldo.Select(x => new
                    {
                        x.id,
                        fecha1 = x.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                        x.valor_aplicado,
                        x.valor_total,
                        x.numero,
                        vencimiento1 = x.vencimiento != null
                            ? x.vencimiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                            : "",
                        x.tipo,
                        x.saldo,
                        x.retencion,
                        x.reteIca,
                        x.reteIva,
                        x.tipodoc,
                        x.descripcion,
                        x.numeroFactura,
                        x.prefijo,
                        x.tp
                    });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public void listas()
        {
            //var buscarTipoDocumento = (from tipoDocumento in context.tp_doc_registros
            //                           select new
            //                           {
            //                               tipoDocumento.sw,
            //                               tipoDocumento.tpdoc_id,
            //                               nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
            //                               tipoDocumento.tipo
            //                           }).ToList();
            //ViewBag.tipo_documentoFiltro = new SelectList(buscarTipoDocumento.Where(x => x.sw == 3), "tpdoc_id", "nombre");

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


            ViewBag.tipo = new SelectList(context.tp_doc_registros.Where(x => x.tipo == 23), "tpdoc_id",
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

            var provedores = from pro in context.tercero_proveedor
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

            encab_documento buscarSerialUltimaNota =
                context.encab_documento.OrderByDescending(x => x.idencabezado).FirstOrDefault();
            ViewBag.numNotaCreado = buscarSerialUltimaNota != null ? buscarSerialUltimaNota.numero : 0;
        }

        public ActionResult Create()
        {
            listas();

            return View();
        }

        [HttpPost]
        public ActionResult Create(NotasContablesModel ncp)
        {
            if (ModelState.IsValid)
            {
                //consecutivo
                grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == ncp.tipo && x.bodega_id == ncp.bodega);
                if (grupo != null)
                {
                    DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                    long consecutivo = doc.BuscarConsecutivo(grupo.grupo);

                    //Encabezado documento
                    encab_documento encabezado = new encab_documento
                    {
                        tipo = ncp.tipo,
                        numero = consecutivo,
                        nit = ncp.nit,
                        fecha = DateTime.Now,
                        valor_total = Convert.ToDecimal(ncp.valor_total),
                        iva = Convert.ToDecimal(ncp.iva),
                        retencion = Convert.ToDecimal(ncp.retencion),
                        retencion_ica = Convert.ToDecimal(ncp.retencion_ica),
                        retencion_iva = Convert.ToDecimal(ncp.retencion_iva),
                        vendedor = ncp.vendedor,
                        documento = ncp.documento,
                        prefijo = ncp.prefijo,
                        valor_mercancia = Convert.ToDecimal(ncp.costo),
                        nota1 = ncp.nota1,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        fec_creacion = DateTime.Now,
                        porcen_retencion =
                        float.Parse(ncp.por_retencion,
                            CultureInfo
                                .InvariantCulture), //esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal
                        porcen_reteiva =
                        float.Parse(ncp.por_retencion_iva,
                            CultureInfo
                                .InvariantCulture), //esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal
                        porcen_retica =
                        float.Parse(ncp.por_retencion_ica,
                            CultureInfo
                                .InvariantCulture), //esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal
                        perfilcontable = ncp.perfilcontable,
                        bodega = ncp.bodega
                    };
                    context.encab_documento.Add(encabezado); // ***** ACTUALIZA ENCABEZADO *****

                    //valor aplicado tengo que actualizarlo en la factura a la que estoy generando la nota
                    //buscas la factura
                    if (!string.IsNullOrWhiteSpace(ncp.documento) && ncp.prefijo != null)
                    {
                        int numerodocumento = Convert.ToInt32(ncp.documento);
                        encab_documento factura = context.encab_documento
                            .Where(d => d.idencabezado == ncp.prefijo && d.numero == numerodocumento).FirstOrDefault();

                        if (factura != null)
                        {
                            int valoraplicado = Convert.ToInt32(factura.valor_aplicado) != null
                                ? Convert.ToInt32(factura.valor_aplicado)
                                : 0;

                            decimal nuevovalor = Convert.ToDecimal(valoraplicado) + Convert.ToDecimal(ncp.valor_total);
                            factura.valor_aplicado = Convert.ToDecimal(nuevovalor);
                            context.Entry(factura).State = EntityState.Modified;
                            // si al crear la nota credito se selecciono factura se debe guardar el registro tambien en cruce documentos, de lo contrario NO
                            //Cruce documentos
                            cruce_documentos cd = new cruce_documentos
                            {
                                idtipo = ncp.tipo,
                                numero = consecutivo,
                                idtipoaplica = Convert.ToInt32(ncp.tipofactura),
                                numeroaplica = Convert.ToInt32(ncp.documento),
                                valor = Convert.ToDecimal(ncp.valor_total),
                                fecha = DateTime.Now,
                                fechacruce = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                            };
                            context.cruce_documentos.Add(cd); // ***** ACTUALIZA CRUCE DE DOCUMENTOS *****
                        }
                    }

                    //movimiento contable
                    //buscamos en perfil cuenta documento, por medio del perfil seleccionado
                    var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                                      join nombreParametro in context.paramcontablenombres
                                                          on perfil.id_nombre_parametro equals nombreParametro.id
                                                      join cuenta in context.cuenta_puc
                                                          on perfil.cuenta equals cuenta.cntpuc_id
                                                      where perfil.id_perfil == ncp.perfilcontable
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
                            if (parametro.id_nombre_parametro == 2 && Convert.ToDecimal(ncp.iva) != 0
                                || parametro.id_nombre_parametro == 3 && Convert.ToDecimal(ncp.retencion) != 0
                                || parametro.id_nombre_parametro == 4 && Convert.ToDecimal(ncp.por_retencion_iva) != 0
                                || parametro.id_nombre_parametro == 5 && Convert.ToDecimal(ncp.retencion_ica) != 0
                                || parametro.id_nombre_parametro == 1
                                || parametro.id_nombre_parametro == 9)
                            {
                                mov_contable movNuevo = new mov_contable
                                {
                                    id_encab = ncp.id_encab,
                                    seq = secuencia,
                                    idparametronombre = parametro.id_nombre_parametro,
                                    cuenta = parametro.cuenta,
                                    centro = parametro.centro,
                                    fec = DateTime.Now,
                                    fec_creacion = DateTime.Now,
                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                    detalle = ncp.nota1
                                };

                                cuenta_puc info = context.cuenta_puc.Where(i => i.cntpuc_id == parametro.cuenta)
                                    .FirstOrDefault();

                                if (info.tercero)
                                {
                                    movNuevo.nit = ncp.nit;
                                }
                                else
                                {
                                    icb_terceros tercero = context.icb_terceros.Where(t => t.doc_tercero == "0")
                                        .FirstOrDefault();
                                    movNuevo.nit = tercero.tercero_id;
                                }

                                // las siguientes validaciones se hacen dependiendo de la cuenta que esta moviendo la nota credito, para guardar la informacion acorde
                                if (parametro.id_nombre_parametro == 1) // if (parametro.id_nombre_parametro == 10)
                                {
                                    /*if (info.aplicaniff==true)
						            {

						            }*/

                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(ncp.costo);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = ncp.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        //movNuevo.credito = Convert.ToDecimal(ncp.valor_total);
                                        //movNuevo.debito = 0;

                                        //movNuevo.creditoniif = Convert.ToDecimal(ncp.valor_total);
                                        //movNuevo.debitoniif = 0;
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ncp.valor_total);

                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ncp.valor_total);
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ncp.valor_total);
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ncp.valor_total);
                                    }
                                }

                                if (parametro.id_nombre_parametro == 9) // credito
                                {
                                    /*if (info.aplicaniff == true)
						            {

						            }*/

                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(ncp.costo);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = ncp.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = Convert.ToDecimal(ncp.costo);
                                        movNuevo.debito = 0;

                                        movNuevo.creditoniif = Convert.ToDecimal(ncp.costo);
                                        movNuevo.debitoniif = 0;
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = Convert.ToDecimal(ncp.costo);
                                        movNuevo.debitoniif = 0;
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = Convert.ToDecimal(ncp.costo);
                                        movNuevo.debito = 0;
                                    }
                                }

                                if (parametro.id_nombre_parametro == 2) // credito
                                {
                                    /*if (info.aplicaniff == true)
						            {

						            }*/

                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(ncp.costo);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = ncp.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = Convert.ToDecimal(ncp.iva);
                                        movNuevo.debito = 0;
                                        movNuevo.creditoniif = Convert.ToDecimal(ncp.iva);
                                        movNuevo.debitoniif = 0;
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = Convert.ToDecimal(ncp.iva);
                                        movNuevo.debitoniif = 0;
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = Convert.ToDecimal(ncp.iva);
                                        movNuevo.debito = 0;
                                    }
                                }

                                if (parametro.id_nombre_parametro == 3)
                                {
                                    /*if (info.aplicaniff == true)
						            {

						            }*/

                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(ncp.costo);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = ncp.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ncp.retencion);

                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ncp.retencion);
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ncp.retencion);
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ncp.retencion);
                                    }
                                }

                                if (parametro.id_nombre_parametro == 4)
                                {
                                    /*if (info.aplicaniff == true)
						            {

						            }*/

                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(ncp.iva);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = ncp.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ncp.retencion_iva);

                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ncp.retencion_iva);
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ncp.retencion_iva);
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ncp.retencion_iva);
                                    }
                                }

                                if (parametro.id_nombre_parametro == 5)
                                {
                                    /*if (info.aplicaniff == true)
						            {

						            }*/

                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(ncp.costo);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = ncp.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ncp.retencion_ica);
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ncp.retencion_ica);
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ncp.retencion_ica);
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ncp.retencion_ica);
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
                                    context.cuentas_valores
                                        .Add(crearCuentaValor); // ***** ACTUALIZA CUENTA VALORES *****
                                }

                                context.mov_contable.Add(movNuevo); // ***** ACTUALIZA MOV CONTABLES *****

                                totalCreditos += movNuevo.credito;
                                totalDebitos += movNuevo.debito;

                                listaDescuadrados.Add(
                                    new DocumentoDescuadradoModel // ***** ACTUALIZA listaDescuadrados *****
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
                        TempData["mensaje_error"] =
                            "El documento no tiene los movimientos calculados correctamente, verifique el perfil del documento";

                        ViewBag.documentoSeleccionado = encabezado.tipo;
                        ViewBag.bodegaSeleccionado = encabezado.bodega;
                        ViewBag.perfilSeleccionado = encabezado.perfilcontable;

                        ViewBag.documentoDescuadrado = listaDescuadrados;
                        ViewBag.calculoDebito = totalDebitos;
                        ViewBag.calculoCredito = totalCreditos;

                        listas();
                        return View(ncp);
                    }

                    context.SaveChanges();
                    TempData["mensaje"] = "registro creado correctamente";
                    DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
                    doc.ActualizarConsecutivo(grupo.grupo, consecutivo);

                    return RedirectToAction("Create");
                }

                TempData["mensaje_error"] = "no hay consecutivo";
            }
            else
            {
                TempData["mensaje_error"] = "No fue posible guardar el registro, por favor valide";
                List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();
            }

            listas();
            return View(ncp);
        }

        // GET: /Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            // listas();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            encab_documento encabezado = context.encab_documento.Find(id);
            if (encabezado == null)
            {
                return HttpNotFound();
            }

            ViewBag.tipo = new SelectList(context.tp_doc_registros.Where(x => x.tipo == 23), "tpdoc_id",
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

            var bodegas = (from bodega in context.bodega_concesionario
                           select new
                           {
                               bodega.id,
                               nombre = "(" + bodega.bodccs_cod + ") " + bodega.bodccs_nombre
                           }).ToList();
            ViewBag.bodega = new SelectList(bodegas, "id", "nombre", encabezado.bodega);

            var terceros = (from tercero in context.icb_terceros
                            select new
                            {
                                tercero.tercero_id,
                                tercero.doc_tercero,
                                tercero.prinom_tercero,
                                tercero.segnom_tercero,
                                tercero.apellido_tercero,
                                tercero.segapellido_tercero,
                                tercero.tpdoc_id,
                                tercero.razon_social
                            }).ToList();
            var terceroNit = terceros.Select(c => new
            {
                c.tercero_id,
                nombre = c.tpdoc_id != 1
                    ? "(" + c.doc_tercero + ") " + c.prinom_tercero + " " + c.segnom_tercero + " " +
                      c.apellido_tercero + " " + c.segapellido_tercero
                    : "(" + c.doc_tercero + ") " + c.razon_social
            }).ToList();
            ViewBag.nit = new SelectList(terceroNit, "tercero_id", "nombre", encabezado.nit);

            var perfiles = (from b in context.perfil_contable_bodega
                            join t in context.perfil_contable_documento
                                on b.idperfil equals t.id
                            where b.idbodega == encabezado.bodega && t.tipo == encabezado.tipo
                            select new
                            {
                                id = b.idperfil,
                                perfil = t.descripcion
                            }).ToList();
            ViewBag.perfilcontable = new SelectList(perfiles, "id", "perfil", encabezado.perfilcontable);

            int factu = Convert.ToInt32(encabezado.documento);
            var buscarFacturasConSaldo = (from e in context.encab_documento
                                          join t in context.tp_doc_registros
                                              on e.tipo equals t.tpdoc_id
                                          join tp in context.tp_doc_registros_tipo
                                              on t.tipo equals tp.id
                                          where e.numero == factu && t.sw == 1
                                          select new
                                          {
                                              e.valor_total,
                                              tipo = t.prefijo,
                                              descripcion = t.tpdoc_nombre,
                                              numeroFactura = e.numero, // numero = 7
                                              valorTrans = e.valor_mercancia != null ? e.valor_mercancia : 0
                                          }).ToList();

            var datosDocu = buscarFacturasConSaldo.Select(x => new
            {
                valor_total = x.valor_total.ToString("N0"),
                //vencimiento1 = x.vencimiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                x.tipo,
                x.descripcion,
                x.numeroFactura,
                val = x.valorTrans.ToString("N0")
            }).FirstOrDefault();

            ViewBag.idtipo = datosDocu.tipo != null ? datosDocu.tipo : "";
            ViewBag.descripcion = datosDocu.descripcion != null ? datosDocu.descripcion : "";
            ViewBag.documento = datosDocu.numeroFactura != null ? datosDocu.numeroFactura : 0;
            ViewBag.costo = datosDocu.val != null ? datosDocu.val : "";
            ViewBag.valor_total = datosDocu.valor_total != null ? datosDocu.valor_total : "";
            return View(encabezado);
        }

        [HttpPost]
        public ActionResult Edit(encab_documento encabezado)
        {
            if (ModelState.IsValid)
            {
                encabezado.impoconsumo = 0;
                encabezado.fec_actualizacion = DateTime.Now;
                encabezado.fecha = DateTime.Now;
                encabezado.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                context.Entry(encabezado).State = EntityState.Modified;
                context.SaveChanges();
                TempData["mensaje"] = "registro actualizado correctamente";
                return RedirectToAction("Edit", new { id = encabezado.idencabezado });
            }

            return View(encabezado);
        }

        public JsonResult BuscarDatos()
        {
            var data2 = (from e in context.encab_documento
                         join b in context.bodega_concesionario
                             on e.bodega equals b.id
                         join t in context.icb_terceros
                             on e.nit equals t.tercero_id
                         join tp in context.tp_doc_registros
                             on e.tipo equals tp.tpdoc_id
                         join tpt in context.tp_doc_registros_tipo
                             on tp.tipo equals tpt.id
                         where tpt.id == 23 //  where tpt.id == 4019
                         select new
                         {
                             e.numero,
                             nit = t.prinom_tercero != null
                                 ? "(" + t.doc_tercero + ")" + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                   t.apellido_tercero + " " + t.segapellido_tercero
                                 : "(" + t.doc_tercero + ") " + t.razon_social,
                             e.fecha,
                             e.valor_total,
                             id = e.idencabezado,
                             bodega = b.bodccs_nombre,
                             estado = e.valor_aplicado != null ? "Devuelta" : "Comprado",
                             e.fec_actualizacion
                         }).ToList();
            var data = data2.Select(x => new
            {
                x.numero,
                x.nit,
                fecha = x.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                fecha_devolucion = x.fec_actualizacion != null
                    ? x.fec_actualizacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : "",
                valor_total = x.valor_total.ToString("N0"),
                x.id,
                x.bodega,
                x.estado
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Movimientos(int idencab)
        {
            var pre_movimientos = (from mov in context.mov_contable
                                   join cta in context.cuenta_puc
                                       on mov.cuenta equals cta.cntpuc_id
                                   join ccs in context.centro_costo
                                       on mov.centro equals ccs.centcst_id
                                   where mov.id_encab == idencab && (mov.idparametronombre == 12 || mov.idparametronombre == 13)
                                   select new
                                   {
                                       mov.id_encab,
                                       centro = "(" + ccs.centcst_id + ") " + ccs.centcst_nombre,
                                       cuenta = "(" + cta.cntpuc_numero + ") " + cta.cntpuc_descp,
                                       cta.mov_cnt,
                                       mov.debito,
                                       mov.credito,
                                       mov.fec
                                   }).ToList();

            decimal sumaDe = pre_movimientos.Sum(item => item.debito);
            decimal sumaCr = pre_movimientos.Sum(item => item.credito);
            var movimientos = pre_movimientos.Select(c => new
            {
                c.id_encab,
                c.centro,
                c.cuenta,
                porcentaje = c.debito > 0 ? Convert.ToDouble(c.debito * 100 / sumaDe) :
                    c.credito > 0 ? Convert.ToDouble(c.credito * 100 / sumaCr) : 0,
                debito = c.debito > 0 ? "$ " + c.debito.ToString("N0") : "0",
                credito = c.credito > 0 ? "$ " + c.credito.ToString("N0") : "0",
                fecha = c.fec != null ? c.fec.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : ""
            }).ToList();

            return Json(movimientos, JsonRequestBehavior.AllowGet);
        }
    }
}