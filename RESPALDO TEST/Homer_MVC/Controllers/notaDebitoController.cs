using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    //Leo
    public class notaDebitoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        private CultureInfo Cultureinfo = new CultureInfo("is-IS");

        //Leo
        // GET: notaCredito
        public ActionResult Browser(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public void listas()
        {
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

            encab_documento buscarSerialUltimaNota =
                context.encab_documento.OrderByDescending(x => x.idencabezado).FirstOrDefault();
            ViewBag.numNotaCreado = buscarSerialUltimaNota != null ? buscarSerialUltimaNota.numero : 0;
        }

        public ActionResult Create(int? menu)
        {
            listas();
            BuscarFavoritos(menu);
            return View();
        }

        /*
		[HttpPost]
		public ActionResult Create(encab_documento encabezado)
		{
		    if (ModelState.IsValid)
		    {
		        //consecutivo
		        var grupo = context.grupoconsecutivos.FirstOrDefault(x => x.documento_id == encabezado.tipo && x.bodega_id == encabezado.bodega).grupo;
		        DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
		        var consecutivo = doc.BuscarConsecutivo(grupo);

		        encabezado.impoconsumo = 0;
		        encabezado.fec_creacion = DateTime.Now;
		        encabezado.fecha = DateTime.Now;
		        encabezado.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
		        context.encab_documento.Add(encabezado);
		        context.SaveChanges();
		        TempData["mensaje"] = "registro creado correctamente";

		        DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
		        doc.ActualizarConsecutivo(grupo, consecutivo);

		        return RedirectToAction("Edit", new { id = encabezado.idencabezado });


		    }
		    return View(encabezado);
		}*/

        [HttpPost]
        public ActionResult Create(NotasContablesModel ndm, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consecutivo
                grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == ndm.tipo && x.bodega_id == ndm.bodega);
                if (grupo != null)
                {
                    DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                    long consecutivo = doc.BuscarConsecutivo(grupo.grupo);

                    //Encabezado documento
                    encab_documento encabezado = new encab_documento
                    {
                        tipo = ndm.tipo,
                        numero = consecutivo,
                        nit = ndm.nit,
                        fecha = DateTime.Now,
                        valor_total = Convert.ToDecimal(ndm.valor_total, Cultureinfo),
                        iva = Convert.ToDecimal(ndm.iva, Cultureinfo),
                        retencion = Convert.ToDecimal(ndm.retencion, Cultureinfo),
                        retencion_ica = Convert.ToDecimal(ndm.retencion_ica, Cultureinfo),
                        retencion_iva = Convert.ToDecimal(ndm.retencion_iva, Cultureinfo),
                        vendedor = ndm.vendedor,
                        documento = ndm.documento,
                        prefijo = ndm.prefijo,
                        valor_mercancia = Convert.ToDecimal(ndm.costo),
                        nota1 = ndm.nota1,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        fec_creacion = DateTime.Now,
                        porcen_retencion = float.Parse(ndm.por_retencion, Cultureinfo),
                        porcen_reteiva = float.Parse(ndm.por_retencion_iva, Cultureinfo),
                        porcen_retica = float.Parse(ndm.por_retencion_ica, Cultureinfo),
                        perfilcontable = ndm.perfilcontable,
                        bodega = ndm.bodega
                    };
                    context.encab_documento.Add(encabezado);
                    context.SaveChanges();
                    if (!string.IsNullOrWhiteSpace(ndm.documento) && ndm.prefijo != null)
                    {
                        int numerodocumento = Convert.ToInt32(ndm.documento);
                        encab_documento factura = context.encab_documento
                            .Where(d => d.idencabezado == ndm.prefijo && d.numero == numerodocumento).FirstOrDefault();

                        /*
                        if (factura != null)
                        {
                            // si al crear la nota debito se selecciono factura se debe guardar el registro tambien en cruce documentos, de lo contrario NO
                            //Cruce documentos
                            cruce_documentos cd = new cruce_documentos
                            {
                                idtipo = ndm.tipo,
                                numero = consecutivo,
                                idtipoaplica = Convert.ToInt32(ndm.tipofactura),
                                numeroaplica = Convert.ToInt32(ndm.documento),
                                valor = Convert.ToDecimal(ndm.valor_total, Cultureinfo),
                                fecha = DateTime.Now,
                                fechacruce = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                id_encabezado=encabezado.idencabezado,
                                id_encab_aplica=factura.idencabezado
                            };
                            context.cruce_documentos.Add(cd);
                        }*/
                    }

                    //movimiento contable
                    //buscamos en perfil cuenta documento, por medio del perfil seleccionado
                    var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                                      join nombreParametro in context.paramcontablenombres
                                                          on perfil.id_nombre_parametro equals nombreParametro.id
                                                      join cuenta in context.cuenta_puc
                                                          on perfil.cuenta equals cuenta.cntpuc_id
                                                      where perfil.id_perfil == ndm.perfilcontable
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
                            if (parametro.id_nombre_parametro == 2 && Convert.ToDecimal(ndm.iva, Cultureinfo) != 0
                                || parametro.id_nombre_parametro == 3 && Convert.ToDecimal(ndm.retencion, Cultureinfo) != 0
                                || parametro.id_nombre_parametro == 4 && Convert.ToDecimal(ndm.por_retencion_iva, Cultureinfo) != 0
                                || parametro.id_nombre_parametro == 5 && Convert.ToDecimal(ndm.retencion_ica, Cultureinfo) != 0
                                || parametro.id_nombre_parametro == 10
                                || parametro.id_nombre_parametro == 11)
                            {
                                mov_contable movNuevo = new mov_contable
                                {
                                    id_encab = encabezado.idencabezado,
                                    seq = secuencia,
                                    idparametronombre = parametro.id_nombre_parametro,
                                    cuenta = parametro.cuenta,
                                    centro = parametro.centro,
                                    fec = DateTime.Now,
                                    fec_creacion = DateTime.Now,
                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                    detalle = ndm.nota1
                                };

                                cuenta_puc info = context.cuenta_puc.Where(i => i.cntpuc_id == parametro.cuenta)
                                    .FirstOrDefault();

                                if (info.tercero)
                                {
                                    movNuevo.nit = ndm.nit;
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
                                    /*if (info.aplicaniff==true)
									{

									}*/

                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(ndm.costo, Cultureinfo);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = ndm.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ndm.valor_total, Cultureinfo);

                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ndm.valor_total, Cultureinfo);
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ndm.valor_total, Cultureinfo);
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ndm.valor_total, Cultureinfo);
                                    }
                                }

                                if (parametro.id_nombre_parametro == 11)
                                {
                                    /*if (info.aplicaniff == true)
									{

									}*/

                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(ndm.costo, Cultureinfo);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = ndm.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = Convert.ToDecimal(ndm.costo, Cultureinfo);
                                        movNuevo.debito = 0;

                                        movNuevo.creditoniif = Convert.ToDecimal(ndm.costo, Cultureinfo);
                                        movNuevo.debitoniif = 0;
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = Convert.ToDecimal(ndm.costo, Cultureinfo);
                                        movNuevo.debitoniif = 0;
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = Convert.ToDecimal(ndm.costo, Cultureinfo);
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
                                        movNuevo.basecontable = Convert.ToDecimal(ndm.costo, Cultureinfo);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = ndm.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = Convert.ToDecimal(ndm.iva, Cultureinfo);
                                        movNuevo.debito = 0;
                                        movNuevo.creditoniif = Convert.ToDecimal(ndm.iva, Cultureinfo);
                                        movNuevo.debitoniif = 0;
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = Convert.ToDecimal(ndm.iva, Cultureinfo);
                                        movNuevo.debitoniif = 0;
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = Convert.ToDecimal(ndm.iva, Cultureinfo);
                                        movNuevo.debito = 0;
                                    }
                                }

                                if (parametro.id_nombre_parametro == 3 && Convert.ToDecimal(ndm.retencion, Cultureinfo) != 0)
                                {
                                    /*if (info.aplicaniff == true)
									{

									}*/

                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(ndm.costo, Cultureinfo);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = ndm.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ndm.retencion, Cultureinfo);

                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ndm.retencion, Cultureinfo);
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ndm.retencion, Cultureinfo);
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ndm.retencion, Cultureinfo);
                                    }
                                }

                                if (parametro.id_nombre_parametro == 4)
                                {
                                    /*if (info.aplicaniff == true)
									{

									}*/

                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(ndm.iva, Cultureinfo);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = ndm.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ndm.retencion_iva, Cultureinfo);

                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ndm.retencion_iva, Cultureinfo);
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ndm.retencion_iva, Cultureinfo);
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ndm.retencion_iva, Cultureinfo);
                                    }
                                }

                                if (parametro.id_nombre_parametro == 5)
                                {
                                    /*if (info.aplicaniff == true)
									{

									}*/

                                    if (info.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(ndm.costo, Cultureinfo);
                                    }
                                    else
                                    {
                                        movNuevo.basecontable = 0;
                                    }

                                    if (info.documeto)
                                    {
                                        movNuevo.documento = ndm.documento;
                                    }

                                    if (buscarCuenta.concepniff == 1)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ndm.retencion_ica, Cultureinfo);
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ndm.retencion_ica, Cultureinfo);
                                    }

                                    if (buscarCuenta.concepniff == 4)
                                    {
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(ndm.retencion_ica, Cultureinfo);
                                    }

                                    if (buscarCuenta.concepniff == 5)
                                    {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(ndm.retencion_ica, Cultureinfo);
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
                        BuscarFavoritos(menu);
                        return View(ndm);
                    }

                    context.SaveChanges();
                    TempData["mensaje"] = "registro creado correctamente";
                    DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
                    doc.ActualizarConsecutivo(grupo.grupo, consecutivo);
                    BuscarFavoritos(menu);
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
            BuscarFavoritos(menu);
            return View(ndm);
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

            var prefijo = (from a in context.encab_documento
                           join t in context.tp_doc_registros
                               on a.tipo equals t.tpdoc_id
                           join b in context.encab_documento
                               on a.idencabezado equals b.prefijo
                           where b.idencabezado == encabezado.idencabezado
                           select new
                           {
                               id = a.idencabezado,
                               t.prefijo,
                               descripcion = t.tpdoc_nombre
                           }).FirstOrDefault();

            icb_terceros tercero = context.icb_terceros.Where(x => x.tercero_id == encabezado.nit).FirstOrDefault();
            List<SelectListItem> itemsU = new List<SelectListItem>();

            string nombre = "(" + tercero.doc_tercero + ") - " + tercero.prinom_tercero + " " + tercero.segnom_tercero + " " + tercero.apellido_tercero + " " + tercero.segapellido_tercero;
            itemsU.Add(new SelectListItem { Text = nombre, Value = tercero.tercero_id.ToString() });

            ViewBag.nit = itemsU;

            ViewBag.idTercero = encabezado.nit;
            ViewBag.idvendedor = encabezado.vendedor;
            ViewBag.bodega = encabezado.bodega;
            ViewBag.perfilcontable = encabezado.perfilcontable;
            ViewBag.prefijo = prefijo != null ? prefijo.prefijo != null ? prefijo.prefijo : "" : "";
            ViewBag.descripcion = prefijo != null ? prefijo.descripcion != null ? prefijo.descripcion : "" : "";
            ViewBag.bodega = encabezado.bodega != null ? encabezado.bodega_concesionario.bodccs_nombre : "";
            ViewBag.bodegaid = encabezado.bodega;
            BuscarFavoritos(menu);
            return View(encabezado);
        }

        [HttpPost]
        public ActionResult Edit(encab_documento encabezado, int? menu)
        {
            /*if (ModelState.IsValid)
			{
			    encabezado.impoconsumo = 0;
			    encabezado.fec_actualizacion = DateTime.Now;
			    encabezado.fecha = DateTime.Now;
			    encabezado.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
			    context.Entry(encabezado).State = EntityState.Modified;
			    context.SaveChanges();
			    TempData["mensaje"] = "registro actualizado correctamente";
			    return RedirectToAction("Edit", new { id = encabezado.idencabezado });
			}*/
            BuscarFavoritos(menu);
            return View(encabezado);
        }

        public ActionResult notaDebito(int id)
        {
            var notaDeb = (from nom in context.encab_documento
                           join tpdoc in context.tp_doc_registros
                               on nom.tipo equals tpdoc.tpdoc_id
                           join ter in context.icb_terceros
                               on nom.nit equals ter.tercero_id
                           join vendor in context.users
                               on nom.vendedor equals vendor.user_id
                           join ciu in context.nom_ciudad
                               on ter.ciu_id equals ciu.ciu_id into tmp
                           from ciu in tmp.DefaultIfEmpty()
                           join dirTer in context.terceros_direcciones
                               on ter.tercero_id equals dirTer.idtercero into temp
                           from dirTer in temp.DefaultIfEmpty()
                           join user in context.users
                               on nom.userid_creacion equals user.user_id
                           where nom.idencabezado == id
                           select new
                           {
                               tipoEntrada = tpdoc.tpdoc_nombre,
                               prefijoEntrada = tpdoc.prefijo,
                               numeroEntrada = nom.numero,
                               fechaEncabezado = nom.fecha,
                               cliente = ter.prinom_tercero + " " + ter.segnom_tercero + " " + ter.apellido_tercero + " " +
                                         ter.segapellido_tercero,
                               clienteId = ter.doc_tercero,
                               clienteTelefono = ter.celular_tercero + " / " + ter.telf_tercero,
                               clienteDireccion = dirTer.direccion,
                               clienteCiudad = ciu.ciu_nombre,
                               vendedor = vendor.user_nombre + " " + vendor.user_apellido,
                               detalle = nom.nota1,
                               nom.iva,
                               valorTotal = nom.valor_total,
                               descuento = 0,
                               subtotal = nom.valor_total - nom.iva,
                               totalTotales = nom.valor_total,
                               factura = nom.numero,
                               responsable = user.user_nombre + " " + user.user_apellido
                           }).FirstOrDefault();

            string root = Server.MapPath("~/Pdf/");
            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
            string path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);
            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");


            PDFmodel notaDebito = new PDFmodel
            {
                tipoEntrada = notaDeb.tipoEntrada,
                prefijo = notaDeb.prefijoEntrada,
                fechaEncab = Convert.ToString(notaDeb.fechaEncabezado),
                numeroEntrada = notaDeb.numeroEntrada,
                numeroFactura = notaDeb.factura.ToString(),
                nombreTercero = notaDeb.cliente,
                Idtercero = notaDeb.clienteId,
                telefono = notaDeb.clienteTelefono,
                direccion = notaDeb.clienteDireccion,
                ciudad = notaDeb.clienteCiudad,
                vendedor = notaDeb.vendedor,
                iva = notaDeb.iva.ToString("0,0", elGR),
                valorTotal = notaDeb.valorTotal.ToString("0,0", elGR),
                descuento = notaDeb.descuento.ToString("0,0", elGR),
                subtotal = notaDeb.subtotal.ToString("0,0", elGR),
                detalleConcepto = notaDeb.detalle,
                totalfinal = notaDeb.totalTotales.ToString("0,0", elGR),
                responsable = notaDeb.responsable,
                referencias = (from encab in context.encab_documento
                               join cruceDoc in context.cruce_documentos
                                   on encab.tipo equals cruceDoc.idtipo
                               join tpdoc in context.tp_doc_registros
                                   on cruceDoc.idtipoaplica equals tpdoc.tpdoc_id
                               where encab.idencabezado == id
                               select new referenciasPDF
                               {
                                   numeroFactura = cruceDoc.numeroaplica,
                                   valorFactura = cruceDoc.valor,
                                   valorInicial = encab.valor_mercancia,
                                   prefijo = tpdoc.prefijo
                               }).ToList()
            };
            ViewAsPdf something = new ViewAsPdf("notaDebito", notaDebito);
            return something;
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
                listaDocumentos = context.tp_doc_registros.Where(x => x.sw == 3).Select(x => x.tpdoc_id).ToList();
            }

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

        //Leonardo 23/09/2019
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
                       where tp.tipo == 21
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