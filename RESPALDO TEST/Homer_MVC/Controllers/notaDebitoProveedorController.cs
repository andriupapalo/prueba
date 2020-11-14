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
    public class notaDebitoProveedorController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: notaCredito
        public ActionResult Browser()
        {
            return View();
        }

        public void listas()
        {
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

            ViewBag.tipo = new SelectList(context.tp_doc_registros.Where(x => x.tipo == 22), "tpdoc_id",
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
        public ActionResult Create(NotasContablesModel ndp)
        {
            if (ModelState.IsValid)
            {
                //consecutivo
                grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == ndp.tipo && x.bodega_id == ndp.bodega);
                if (grupo != null)
                {
                    DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                    long consecutivo = doc.BuscarConsecutivo(grupo.grupo);

                    //Encabezado documento
                    encab_documento encabezado = new encab_documento
                    {
                        tipo = ndp.tipo,
                        numero = consecutivo,
                        nit = ndp.nit,
                        fecha = DateTime.Now,
                        valor_total = Convert.ToDecimal(ndp.valor_total),
                        iva = Convert.ToDecimal(ndp.iva),
                        retencion = Convert.ToDecimal(ndp.retencion),
                        retencion_ica = Convert.ToDecimal(ndp.retencion_ica),
                        retencion_iva = Convert.ToDecimal(ndp.retencion_iva),
                        vendedor = ndp.vendedor,
                        documento = ndp.documento,
                        prefijo = ndp.prefijo,
                        valor_mercancia = Convert.ToDecimal(ndp.costo),
                        nota1 = ndp.nota1,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        fec_creacion = DateTime.Now,
                        porcen_retencion =
                        float.Parse(ndp.por_retencion,
                            CultureInfo
                                .InvariantCulture), //esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal
                        porcen_reteiva =
                        float.Parse(ndp.por_retencion_iva,
                            CultureInfo
                                .InvariantCulture), //esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal
                        porcen_retica =
                        float.Parse(ndp.por_retencion_ica,
                            CultureInfo
                                .InvariantCulture), //esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal
                        perfilcontable = ndp.perfilcontable,
                        bodega = ndp.bodega,
                        estado = true
                    };
                    context.encab_documento.Add(encabezado);

                    //valor aplicado tengo que actualizarlo en la factura a la que estoy generando la nota
                    //buscas la factura
                    if (!string.IsNullOrWhiteSpace(ndp.documento) && ndp.prefijo != null)
                    {
                        int numerodocumento = Convert.ToInt32(ndp.documento);
                        encab_documento factura = context.encab_documento
                            .Where(d => d.idencabezado == ndp.prefijo && d.numero == numerodocumento).FirstOrDefault();

                        if (factura != null)
                        {
                            int valoraplicado = Convert.ToInt32(factura.valor_aplicado) != null
                                ? Convert.ToInt32(factura.valor_aplicado)
                                : 0;

                            decimal nuevovalor = Convert.ToDecimal(valoraplicado) + Convert.ToDecimal(ndp.valor_total);
                            factura.valor_aplicado = Convert.ToDecimal(nuevovalor);
                            context.Entry(factura).State = EntityState.Modified;
                            // si al crear la nota credito se selecciono factura se debe guardar el registro tambien en cruce documentos, de lo contrario NO
                            //Cruce documentos
                            cruce_documentos cd = new cruce_documentos
                            {
                                idtipo = ndp.tipo,
                                numero = consecutivo,
                                idtipoaplica = Convert.ToInt32(ndp.tipofactura),
                                numeroaplica = Convert.ToInt32(ndp.documento),
                                valor = Convert.ToDecimal(ndp.valor_total),
                                fecha = DateTime.Now,
                                fechacruce = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                            };
                            context.cruce_documentos.Add(cd);
                        }
                    }

                    //movimiento contable
                    //buscamos en perfil cuenta documento, por medio del perfil seleccionado
                    var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                                      join nombreParametro in context.paramcontablenombres
                                                          on perfil.id_nombre_parametro equals nombreParametro.id
                                                      join cuenta in context.cuenta_puc
                                                          on perfil.cuenta equals cuenta.cntpuc_id
                                                      where perfil.id_perfil == ndp.perfilcontable
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
                            if (parametro.id_nombre_parametro == 2 && Convert.ToDecimal(ndp.iva) != 0
                                || parametro.id_nombre_parametro == 3 && Convert.ToDecimal(ndp.retencion) != 0
                                || parametro.id_nombre_parametro == 4 && Convert.ToDecimal(ndp.por_retencion_iva) != 0
                                || parametro.id_nombre_parametro == 5 && Convert.ToDecimal(ndp.retencion_ica) != 0
                                || parametro.id_nombre_parametro == 1
                                || parametro.id_nombre_parametro == 9)
                            {
                                mov_contable movNuevo = new mov_contable
                                {
                                    id_encab = ndp.id_encab,
                                    seq = secuencia,
                                    idparametronombre = parametro.id_nombre_parametro,
                                    cuenta = parametro.cuenta,
                                    centro = parametro.centro,
                                    fec = DateTime.Now,
                                    fec_creacion = DateTime.Now,
                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                    detalle = ndp.nota1
                                };

                                cuenta_puc info = context.cuenta_puc.Where(i => i.cntpuc_id == parametro.cuenta)
                                    .FirstOrDefault();

                                if (info.tercero)
                                {
                                    movNuevo.nit = ndp.nit;
                                }
                                else
                                {
                                    icb_terceros tercero = context.icb_terceros.Where(t => t.doc_tercero == "0")
                                        .FirstOrDefault();
                                    movNuevo.nit = tercero.tercero_id;
                                }

                                // las siguientes validaciones se hacen dependiendo de la cuenta que esta moviendo la nota credito, para guardar la informacion acorde
                                if (parametro.id_nombre_parametro == 1)
                                {
                                    /*if (info.aplicaniff==true)
									{

									}*/

                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(ndp.costo);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = ndp.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ndp.valor_total);

                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ndp.valor_total);
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ndp.valor_total);
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ndp.valor_total);
                                    }
                                }

                                if (parametro.id_nombre_parametro == 9)
                                {
                                    /*if (info.aplicaniff == true)
									{

									}*/

                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(ndp.costo);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = ndp.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = Convert.ToDecimal(ndp.costo);
                                        movNuevo.debito = 0;

                                        movNuevo.creditoniif = Convert.ToDecimal(ndp.costo);
                                        movNuevo.debitoniif = 0;
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = Convert.ToDecimal(ndp.costo);
                                        movNuevo.debitoniif = 0;
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = Convert.ToDecimal(ndp.costo);
                                        movNuevo.debito = 0;
                                    }
                                }

                                if (parametro.id_nombre_parametro == 2)
                                {
                                    /*if (info.aplicaniff == true)
									{

									}*/

                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(ndp.costo);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = ndp.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = Convert.ToDecimal(ndp.iva);
                                        movNuevo.debito = 0;
                                        movNuevo.creditoniif = Convert.ToDecimal(ndp.iva);
                                        movNuevo.debitoniif = 0;
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = Convert.ToDecimal(ndp.iva);
                                        movNuevo.debitoniif = 0;
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = Convert.ToDecimal(ndp.iva);
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
                                        movNuevo.basecontable = Convert.ToDecimal(ndp.costo);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = ndp.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ndp.retencion);

                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ndp.retencion);
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ndp.retencion);
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ndp.retencion);
                                    }
                                }

                                if (parametro.id_nombre_parametro == 4)
                                {
                                    /*if (info.aplicaniff == true)
									{

									}*/

                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(ndp.iva);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = ndp.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ndp.retencion_iva);

                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ndp.retencion_iva);
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ndp.retencion_iva);
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ndp.retencion_iva);
                                    }
                                }

                                if (parametro.id_nombre_parametro == 5)
                                {
                                    /*if (info.aplicaniff == true)
									{

									}*/

                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(ndp.costo);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = ndp.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ndp.retencion_ica);
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ndp.retencion_ica);
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ndp.retencion_ica);
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ndp.retencion_ica);
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
                        TempData["mensaje_error"] =
                            "El documento no tiene los movimientos calculados correctamente, verifique el perfil del documento";

                        ViewBag.documentoSeleccionado = encabezado.tipo;
                        ViewBag.bodegaSeleccionado = encabezado.bodega;
                        ViewBag.perfilSeleccionado = encabezado.perfilcontable;

                        ViewBag.documentoDescuadrado = listaDescuadrados;
                        ViewBag.calculoDebito = totalDebitos;
                        ViewBag.calculoCredito = totalCreditos;

                        listas();
                        return View(ndp);
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
            return View(ndp);
        }

        // GET: /Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            listas();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            encab_documento encabezado = context.encab_documento.Find(id);
            if (encabezado == null)
            {
                return HttpNotFound();
            }

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
                                                      fecha = e.fecha.ToString(),
                                                      valor_aplicado = e.valor_aplicado != null ? e.valor_aplicado : 0,
                                                      e.valor_total,
                                                      e.numero,
                                                      vencimiento = e.vencimiento.Value.ToString(),
                                                      tipo = "(" + t.prefijo + ") " + t.tpdoc_nombre,
                                                      saldo = e.valor_total - e.valor_aplicado,
                                                      retencion = e.porcen_retencion,
                                                      reteIca = e.porcen_retica,
                                                      reteIva = e.porcen_reteiva,
                                                      tipodoc = t.tpdoc_id,
                                                      descripcion = t.tpdoc_nombre,
                                                      numeroFactura = e.numero,
                                                      prefijo = e.idencabezado,
                                                      tp = e.tipo
                                                  }).ToList();

                    var data = buscarFacturasConSaldo.Select(x => new
                    {
                        x.id,
                        x.fecha,
                        x.valor_aplicado,
                        x.valor_total,
                        x.numero,
                        x.vencimiento,
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
                                                      fecha = e.fecha.ToString(),
                                                      valor_aplicado = e.valor_aplicado != null ? e.valor_aplicado : 0,
                                                      e.valor_total,
                                                      e.numero,
                                                      vencimiento = e.vencimiento.Value.ToString(),
                                                      tipo = "(" + t.prefijo + ") " + t.tpdoc_nombre,
                                                      saldo = e.valor_total - (e.valor_aplicado != null ? e.valor_aplicado : 0),
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
                        x.fecha,
                        x.valor_aplicado,
                        x.valor_total,
                        x.numero,
                        x.vencimiento,
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

        public JsonResult BuscarDatos()
        {
            var data = from e in context.encab_documento
                       join b in context.bodega_concesionario
                           on e.bodega equals b.id
                       join t in context.icb_terceros
                           on e.nit equals t.tercero_id
                       join tp in context.tp_doc_registros
                           on e.tipo equals tp.tpdoc_id
                       join tpt in context.tp_doc_registros_tipo
                           on tp.tipo equals tpt.id
                       where tpt.id == 4018
                       select new
                       {
                           e.numero,
                           nit = t.prinom_tercero != null
                               ? "(" + t.doc_tercero + ")" + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                 t.apellido_tercero + " " + t.segapellido_tercero
                               : "(" + t.doc_tercero + ") " + t.razon_social,
                           fecha = e.fecha.ToString(),
                           e.valor_total,
                           id = e.idencabezado,
                           bodega = b.bodccs_nombre,
                           estado = e.valor_aplicado != null ? "Devuelta" : "Comprado",
                           fecha_devolucion = e.fec_actualizacion.Value.ToString()
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}