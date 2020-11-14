using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
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
    public class notaCreditoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        private CultureInfo Cultureinfo = new CultureInfo("is-IS");

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
                                           sw_tipo = tipoDocumento.tp_doc_sw.sw,
                                           tipoDocumento.sw,
                                           tipoDocumento.tpdoc_id,
                                           nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                           tipoDocumento.tipo
                                       }).ToList();
            ViewBag.tipo_documentoFiltro =
                new SelectList(buscarTipoDocumento.Where(x => x.sw_tipo == 3 || x.sw_tipo == 13 || x.sw_tipo == 18), "tpdoc_id", "nombre");

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

            ViewBag.vendedor = itemsU;


            ViewBag.moneda = new SelectList(context.monedas, "moneda", "descripcion");
            ViewBag.tasa = new SelectList(context.moneda_conversion, "id", "conversion");
            ViewBag.motivocompra = new SelectList(context.motcompra, "id", "Motivo");

            ViewBag.condicion_pago = context.fpago_tercero;

            var provedores = (from pro in context.tercero_cliente
                              join ter in context.icb_terceros
                                  on pro.tercero_id equals ter.tercero_id
                              select new
                              {
                                  idTercero = ter.tercero_id,
                                  nombreTErcero = ter.prinom_tercero,
                                  apellidosTercero = ter.apellido_tercero,
                                  razonSocial = ter.razon_social,
                                  ter.doc_tercero
                              }).ToList();
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

        [HttpPost]
        public ActionResult Create(NotasContablesModel ncm, int? menu)
        {
            if (ModelState.IsValid)
            {

                using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                {
                    //consecutivo
                    grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == ncm.tipo && x.bodega_id == ncm.bodega);
                    if (grupo != null)
                    {
                        try
                        {
                            DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                            long consecutivo = doc.BuscarConsecutivo(grupo.grupo);

                            //Encabezado documento
                            encab_documento encabezado = new encab_documento
                            {
                                tipo = ncm.tipo,
                                numero = consecutivo,
                                nit = ncm.nit,
                                fecha = DateTime.Now,
                                valor_total = Convert.ToDecimal(ncm.valor_total,Cultureinfo),
                                iva = Convert.ToDecimal(ncm.iva,Cultureinfo),
                                retencion = Convert.ToDecimal(ncm.retencion,Cultureinfo),
                                retencion_ica = Convert.ToDecimal(ncm.retencion_ica,Cultureinfo),
                                retencion_iva = Convert.ToDecimal(ncm.retencion_iva, Cultureinfo),
                                vendedor = ncm.vendedor,
                                documento = ncm.documento,
                                prefijo = ncm.prefijo,
                                valor_mercancia = Convert.ToDecimal(ncm.costo,Cultureinfo),
                                nota1 = ncm.nota1,
                                valor_aplicado = Convert.ToDecimal(ncm.valor_aplicado, Cultureinfo),
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                fec_creacion = DateTime.Now,
                                porcen_retencion =
                            float.Parse(ncm.por_retencion,
                                CultureInfo
                                    .InvariantCulture), //esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal
                                porcen_reteiva =
                            float.Parse(ncm.por_retencion_iva,
                                CultureInfo
                                    .InvariantCulture), //esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal
                                porcen_retica =
                            float.Parse(ncm.por_retencion_ica,
                                CultureInfo
                                    .InvariantCulture), //esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal
                                perfilcontable = ncm.perfilcontable,
                                bodega = ncm.bodega
                            };
                            context.encab_documento.Add(encabezado);
                            context.SaveChanges();
                            int cruce = encabezado.idencabezado;
                            int cruceDocumento = context.encab_documento.OrderByDescending(d => d.idencabezado)
                            .Select(d => d.idencabezado).FirstOrDefault();
                            //valor aplicado tengo que actualizarlo en la factura a la que estoy generando la nota
                            //buscas la factura
                            if (!string.IsNullOrWhiteSpace(ncm.documento) && ncm.prefijo != null)
                            {
                                int numerodocumento = Convert.ToInt32(ncm.documento);
                                encab_documento factura = context.encab_documento
                                .Where(d => d.idencabezado == ncm.prefijo && d.numero == numerodocumento).FirstOrDefault();

                                if (factura != null)
                                {
                                    decimal valoraplicado = Convert.ToDecimal(factura.valor_aplicado) != null
                                    ? Convert.ToInt32(factura.valor_aplicado)
                                    : 0;
                                    decimal totalfactura = factura.valor_total;
                                    decimal valorrecibo = encabezado.valor_total;
                                    decimal nuevovalor = 0;
                                    decimal valorcruce = 0;
                                    //si el monto de la nota crédito es menor al saldo actual de la factura
                                    if (valorrecibo <= (totalfactura - valoraplicado))
                                    {
                                        //sumatoria del valor de la nota crédito al valor aplicado de la factura
                                        nuevovalor = valoraplicado + valorrecibo;
                                        valorcruce = valorrecibo;
                                        factura.valor_aplicado = nuevovalor;
                                        //tambien aplico ese valor a la nota crédito para colocarla en 0
                                        encabezado.valor_aplicado = encabezado.valor_total;
                                    }
                                    else
                                    {
                                        //calculo cual es el saldo de la factura
                                        decimal diferencia = totalfactura - valoraplicado;
                                        valorcruce = diferencia;
                                        //a la nota crédito le resto el saldo que queda de la factura
                                        nuevovalor = valoraplicado + diferencia;
                                        factura.valor_aplicado = nuevovalor;
                                        encabezado.valor_aplicado = valorrecibo - diferencia;
                                    }
                                    context.Entry(factura).State = EntityState.Modified;
                                    context.Entry(encabezado).State = EntityState.Modified;
                                    // si al crear la nota credito se selecciono factura se debe guardar el registro tambien en cruce documentos, de lo contrario NO
                                    //Cruce documentos
                                    cruce_documentos cd = new cruce_documentos
                                    {
                                        idtipo = ncm.tipo,
                                        numero = consecutivo,
                                        idtipoaplica = Convert.ToInt32(ncm.tipofactura),
                                        numeroaplica = Convert.ToInt32(ncm.documento),
                                        valor = valorcruce,
                                        fecha = DateTime.Now,
                                        fechacruce = DateTime.Now,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),

                                    };
                                    if (factura != null)
                                    {
                                        cd.id_encabezado = encabezado.idencabezado;
                                        cd.id_encab_aplica = factura.idencabezado;
                                    }
                                    context.cruce_documentos.Add(cd);
                                    context.SaveChanges();
                                }
                            }

                            //movimiento contable
                            //buscamos en perfil cuenta documento, por medio del perfil seleccionado
                            var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                                              join nombreParametro in context.paramcontablenombres
                                                                  on perfil.id_nombre_parametro equals nombreParametro.id
                                                              join cuenta in context.cuenta_puc
                                                                  on perfil.cuenta equals cuenta.cntpuc_id
                                                              where perfil.id_perfil == ncm.perfilcontable
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
                                    if (parametro.id_nombre_parametro == 2 && Convert.ToDecimal(ncm.iva, Cultureinfo) != 0
                                    || parametro.id_nombre_parametro == 3 && Convert.ToDecimal(ncm.retencion, Cultureinfo) != 0
                                    || parametro.id_nombre_parametro == 4 && Convert.ToDecimal(ncm.por_retencion_iva, Cultureinfo) != 0
                                    || parametro.id_nombre_parametro == 5 && Convert.ToDecimal(ncm.retencion_ica, Cultureinfo) != 0
                                    || parametro.id_nombre_parametro == 10
                                    || parametro.id_nombre_parametro == 11)
                                    {
                                        mov_contable movNuevo = new mov_contable
                                        {
                                            //id_encab = ncm.id_encab,
                                            id_encab = cruce,
                                            seq = secuencia,
                                            idparametronombre = parametro.id_nombre_parametro,
                                            cuenta = parametro.cuenta,
                                            centro = parametro.centro,
                                            fec = DateTime.Now,
                                            fec_creacion = DateTime.Now,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            detalle = ncm.nota1
                                        };

                                        cuenta_puc info = context.cuenta_puc.Where(i => i.cntpuc_id == parametro.cuenta)
                                        .FirstOrDefault();

                                        if (info.tercero)
                                        {
                                            movNuevo.nit = ncm.nit;
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
                                                movNuevo.basecontable = Convert.ToDecimal(ncm.costo, Cultureinfo);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = ncm.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(ncm.valor_total, Cultureinfo);
                                                movNuevo.debito = 0;

                                                movNuevo.creditoniif = Convert.ToDecimal(ncm.valor_total, Cultureinfo);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(ncm.valor_total, Cultureinfo);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(ncm.valor_total, Cultureinfo);
                                                movNuevo.debito = 0;
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 11)
                                        {
                                            /*if (info.aplicaniff == true)
                                            {

                                            }*/

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(ncm.costo, Cultureinfo);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = ncm.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(ncm.costo, Cultureinfo);

                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(ncm.costo, Cultureinfo);
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(ncm.costo, Cultureinfo);
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(ncm.costo, Cultureinfo);
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 2)
                                        {
                                            /*if (info.aplicaniff == true)
                                            {

                                            }*/

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(ncm.costo, Cultureinfo);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = ncm.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(ncm.iva, Cultureinfo);
                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(ncm.iva, Cultureinfo);
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(ncm.iva, Cultureinfo);
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(ncm.iva, Cultureinfo);
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 3)
                                        {
                                            /*if (info.aplicaniff == true)
                                            {

                                            }*/

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(ncm.costo, Cultureinfo);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = ncm.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(ncm.retencion, Cultureinfo);
                                                movNuevo.debito = 0;

                                                movNuevo.creditoniif = Convert.ToDecimal(ncm.retencion, Cultureinfo);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(ncm.retencion, Cultureinfo);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(ncm.retencion, Cultureinfo);
                                                movNuevo.debito = 0;
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 4)
                                        {
                                            /*if (info.aplicaniff == true)
                                            {

                                            }*/

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(ncm.iva, Cultureinfo);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = ncm.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(ncm.retencion_iva, Cultureinfo);
                                                movNuevo.debito = 0;

                                                movNuevo.creditoniif = Convert.ToDecimal(ncm.retencion_iva);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(ncm.retencion_iva, Cultureinfo);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(ncm.retencion_iva, Cultureinfo);
                                                movNuevo.debito = 0;
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 5)
                                        {
                                            /*if (info.aplicaniff == true)
                                            {

                                            }*/

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(ncm.costo, Cultureinfo);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = ncm.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(ncm.retencion_ica, Cultureinfo);
                                                movNuevo.debito = 0;
                                                movNuevo.creditoniif = Convert.ToDecimal(ncm.retencion_ica, Cultureinfo);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(ncm.retencion_ica, Cultureinfo);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(ncm.retencion_ica, Cultureinfo);
                                                movNuevo.debito = 0;
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
                                            context.SaveChanges();
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
                                        context.SaveChanges();
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
                                dbTran.Rollback();
                                listas();
                                BuscarFavoritos(menu);
                                return View(ncm);
                            }

                            context.SaveChanges();
                            dbTran.Commit();
                            TempData["mensaje"] = "registro creado correctamente";
                            DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
                            doc.ActualizarConsecutivo(grupo.grupo, consecutivo);
                            BuscarFavoritos(menu);
                            return RedirectToAction("Create");
                        }
                        catch (Exception ex)
                        {
                            Exception mensaje = ex;
                            TempData["mensaje_error"] = ex.Message;
                            dbTran.Rollback();
                        }

                    }
                    else
                    {
                        TempData["mensaje_error"] = "no hay consecutivo";
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

            listas();
            BuscarFavoritos(menu);
            return View(ncm);
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


            ViewBag.idTercero = encabezado.nit;
            ViewBag.idvendedor = encabezado.vendedor;
            ViewBag.bodega = encabezado.bodega;
            ViewBag.perfilcontable = encabezado.perfilcontable;
            ViewBag.prefijo = prefijo != null ? prefijo.prefijo != null ? prefijo.prefijo : "" : "";
            ViewBag.descripcion = prefijo != null ? prefijo.descripcion != null ? prefijo.descripcion : "" : "";
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

        public ActionResult notaCredito(int id)
        {
            var nota = (from nom in context.encab_documento
                        join cruceDoc in context.cruce_documentos
                 on nom.idencabezado equals cruceDoc.id_encabezado
                        join tpdoc in context.tp_doc_registros
                        on cruceDoc.idtipo equals tpdoc.tpdoc_id
                        join ter in context.icb_terceros
                        on nom.nit equals ter.tercero_id
                        join ciu in context.nom_ciudad
                            on ter.ciu_id equals ciu.ciu_id into tmp
                        from ciu in tmp.DefaultIfEmpty()
                        join vendedor in context.users
                                     on nom.vendedor equals vendedor.user_id into ven
                        from vendedor in ven.DefaultIfEmpty()
                        join resp in context.users
                        on nom.userid_creacion equals resp.user_id
                        where nom.idencabezado == id
                        select new
                        {
                            nom.idencabezado,
                            tipoEntrada = tpdoc.tpdoc_nombre,
                            numeroRecibo = nom.numero,
                            prefijoEntrada = tpdoc.prefijo,
                            numeroEntrada = nom.numero,
                            fechaEncabezado = nom.fecha,
                            cliente = ter.prinom_tercero + " " + ter.segnom_tercero + " " + ter.apellido_tercero + " " +
                                      ter.segapellido_tercero,
                            clienteId = ter.doc_tercero,
                            clienteTelefono = ter.celular_tercero + " / " + ter.telf_tercero,
                            clienteCiudad = ciu.ciu_nombre,
                            vendedor = vendedor.user_nombre + " " + vendedor.user_apellido,
                            detalle = nom.nota1,
                            nom.iva,
                            valorTotal = nom.valor_total,
                            descuento = 0,
                            subtotal = nom.valor_total - nom.iva,
                            totalTotales = nom.valor_total,
                            responsable = resp.user_nombre + " " + resp.user_apellido,
                            direc_tercero = (from direccion in context.terceros_direcciones
                                             join ciudad in context.nom_ciudad
                                                 on direccion.ciudad equals ciudad.ciu_id
                                             where direccion.idtercero == ter.tercero_id
                                             orderby direccion.id descending
                                             select new
                                             {
                                                 direccion.id,
                                                 direccion = direccion.direccion != null ? direccion.direccion : " "
                                             }).FirstOrDefault(),
                        }).FirstOrDefault();

            string root = Server.MapPath("~/Pdf/");
            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
            string path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);
            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");


            PDFmodel notaCredito = new PDFmodel
            {
                tipoEntrada = nota.tipoEntrada,
                prefijo = nota.prefijoEntrada,
                numeroRegistro = nota.numeroRecibo,
                fechaEncab = Convert.ToString(nota.fechaEncabezado),
                numeroEntrada = nota.numeroEntrada,
                nombreTercero = nota.cliente,
                Idtercero = nota.clienteId,
                telefono = nota.clienteTelefono,
                Direccion = nota.direc_tercero != null ? nota.direc_tercero.direccion != null ? nota.direc_tercero.direccion : "" : "",
                ciudad = nota.clienteCiudad,
                vendedor = nota.vendedor != null ? nota.vendedor : "",
                responsable = nota.responsable,
                iva = nota.iva.ToString("N0", new CultureInfo("is-IS")),
                valorTotal = nota.valorTotal.ToString("N0", new CultureInfo("is-IS")),
                descuento = nota.descuento.ToString("N0", new CultureInfo("is-IS")),
                subtotal = nota.subtotal.ToString("N0", new CultureInfo("is-IS")),
                detalleConcepto = nota.detalle,
                valorConDescuento = nota.descuento,
                totalfinal = nota.totalTotales.ToString("N0", new CultureInfo("is-IS")),
                referencias = (from encab in context.encab_documento
                               join cruceDoc in context.cruce_documentos
                        on encab.idencabezado equals cruceDoc.id_encabezado
                               join tpdoc in context.tp_doc_registros
                               on cruceDoc.idtipo equals tpdoc.tpdoc_id
                               where encab.idencabezado == id
                               select new referenciasPDF
                               {
                                   numeroFactura = cruceDoc.numeroaplica,
                                   valorFactura = cruceDoc.valor,
                                   valorInicial = encab.valor_mercancia,
                                   prefijo = tpdoc.prefijo
                               }).ToList()
            };
            notaCredito.referencias.ForEach(item => item.valorInicial = Math.Truncate(item.valorInicial));
            ViewAsPdf something = new ViewAsPdf("notaCredito", notaCredito);
            return something;
        }

        public JsonResult BuscarDocumentosFiltro(int nit, DateTime? desde, DateTime? hasta, int? id_documento,
            int? factura)
        {

            var predicado = PredicateBuilder.True<encab_documento>();
            if (desde == null)
            {
                desde = context.encab_documento.OrderBy(x => x.fecha).Select(x => x.fecha).FirstOrDefault();
                predicado = predicado.And(d => d.fecha >= desde);
            }

            if (hasta == null)
            {
                hasta = context.encab_documento.OrderByDescending(x => x.fecha).Select(x => x.fecha).FirstOrDefault();
            }
            else
            {
                hasta = hasta.GetValueOrDefault();
            }
            predicado = predicado.And(d => d.fecha <= hasta);

            List<int> listaDocumentos = new List<int>();
            if (id_documento != null)
            {
                listaDocumentos.Add(id_documento ?? 0);
            }
            else
            {
                listaDocumentos = context.tp_doc_registros.Where(x => x.tp_doc_sw.sw == 3 || x.tp_doc_sw.sw == 13 || x.tp_doc_sw.sw == 18).Select(x => x.tpdoc_id).ToList();
            }
            predicado = predicado.And(d =>1==1 && listaDocumentos.Contains(d.tp_doc_registros.tpdoc_id));

            if (factura != null)
            {
                predicado = predicado.And(d => d.numero == factura);
            }
            if (nit != null)
            {
                predicado = predicado.And(d => d.nit == nit);
            }
            predicado = predicado.And(d => d.valor_total-d.valor_aplicado >0);

            var buscarFacturasConSaldo2 = context.encab_documento.Where(predicado).ToList();

            var buscarFacturasConSaldo3 = buscarFacturasConSaldo2.Select(d => new {
                id = d.idencabezado,
                fecha = d.fecha.ToString(),
                valor_aplicado = d.valor_aplicado != null ? d.valor_aplicado : 0,
                d.valor_total,
                d.numero,
                vencimiento = d.vencimiento!=null?d.vencimiento.Value.ToString():"",
                tipo = "(" + d.tp_doc_registros.prefijo + ") " + d.tp_doc_registros.tpdoc_nombre,
                saldo = d.valor_total - d.valor_aplicado,
                retencion = d.porcen_retencion,
                reteIca = d.porcen_retica,
                reteIva = d.porcen_reteiva,
                tipodoc = d.tp_doc_registros.tpdoc_id,
                descripcion = d.tp_doc_registros.tpdoc_nombre,
                numeroFactura = d.numero,
                prefijo = d.idencabezado,
                tp = d.tipo
            }).ToList();

            if (desde != null && hasta != null && desde < hasta)
            {
                
                var data = buscarFacturasConSaldo3.Select(x => new
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
                       where tp.tipo == 20
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