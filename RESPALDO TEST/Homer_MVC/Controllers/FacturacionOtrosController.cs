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
    public class FacturacionOtrosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        private CultureInfo Cultureinfo = new CultureInfo("is-IS");


        public void listas()
        {
            ViewBag.doc_registros = context.tp_doc_registros.Where(x => x.tipo == 37);
            ViewBag.moneda = new SelectList(context.monedas, "moneda", "descripcion");
            ViewBag.pedido = context.icb_referencia_mov.ToList();

            // ViewBag.condicion_pago = context.fpago_tercero;

            var provedores = from pro in context.tercero_cliente
                             join ter in context.icb_terceros
                                 on pro.tercero_id equals ter.tercero_id
                             select new
                             {
                                 idTercero = ter.tercero_id,
                                 nombreTErcero = ter.prinom_tercero + " " + ter.segnom_tercero,
                                 apellidosTercero = ter.apellido_tercero + " " + ter.segapellido_tercero,
                                 razonSocial = ter.razon_social
                             };
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in provedores)
            {
                string nombre = item.nombreTErcero + " " + item.apellidosTercero + " " + item.razonSocial;
                items.Add(new SelectListItem { Text = nombre, Value = item.idTercero.ToString() });
            }

            ViewBag.cliente = items;

            //ViewBag.codigo = new SelectList(context.icb_referencia.Where(x=> x.modulo == "R"), "ref_codigo", "ref_descripcion");
            ViewBag.codigo = context.icb_referencia.Where(x => x.modulo == "R" && x.tipo_id == 3).ToList();

            List<users> asesores =
                context.users.Where(x => x.rol_id == 4 || x.rol_id == 6)
                    .ToList(); // 4 Asesor Comercial / 6 Asesor de Repuestos
            List<SelectListItem> listaAsesores = new List<SelectListItem>();
            foreach (users asesor in asesores)
            {
                listaAsesores.Add(new SelectListItem
                { Value = asesor.user_id.ToString(), Text = asesor.user_nombre + " " + asesor.user_apellido });
            }

            ViewBag.vendedor = listaAsesores;

            encab_documento buscarUltimaFactura = context.encab_documento.OrderByDescending(x => x.idencabezado).FirstOrDefault();
            ViewBag.numFacturaCreada = buscarUltimaFactura != null ? buscarUltimaFactura.numero : 0;
        }

        // GET: FacturacionOtros
        public ActionResult Index(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult Facturar(int? menu)
        {
            listas();
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Facturar(NotasContablesModel modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                {
                    try
                    {
                        int funciono = 0;
                        decimal totalCreditos = 0;
                        decimal totalDebitos = 0;
                        decimal costoPromedioTotal = 0;

                        var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                                          join nombreParametro in context.paramcontablenombres
                                                              on perfil.id_nombre_parametro equals nombreParametro.id
                                                          join cuenta in context.cuenta_puc
                                                              on perfil.cuenta equals cuenta.cntpuc_id
                                                          where perfil.id_perfil == modelo.perfilcontable
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
                        //decimal totalDebitos = 0;
                        //decimal totalCreditos = 0;


                        List<cuentas_valores> ids_cuentas_valores = new List<cuentas_valores>();
                        centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                        int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;

                        List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();

                        int costoLineas = Convert.ToInt32(Request["lista_referencias"]);
                        for (int i = 0; i <= costoLineas; i++)
                        {
                            if (!string.IsNullOrEmpty(Request["referencia" + i]))
                            {
                                int anio = DateTime.Now.Year;
                                int mes = DateTime.Now.Month;

                                string precio = Request["valorUnitarioReferencia" + i];

                                string referencia = Request["referencia" + i];
                                //      var vwPromedio = Request["valor_uni" + i];

                                //    var vwPromedio = context.vw_promedio.FirstOrDefault(x => x.codigo == referencia && x.ano == anio && x.mes == mes);
                                //    var costoReferencia = vwPromedio.Promedio;
                                string costoReferencia = precio;
                                costoPromedioTotal += Convert.ToDecimal(costoReferencia,Cultureinfo) *
                                                      Convert.ToDecimal(Request["cantidadReferencia" + i], Cultureinfo);
                            }
                        }

                        int bodega = Convert.ToInt32(Request["selectBodegas"]);
                        string lista = Request["lista_referencias"];
                        if (!string.IsNullOrEmpty(lista))
                        {
                            int datos = Convert.ToInt32(lista);
                            decimal costoTotal =
                                Convert.ToDecimal(Request["valor_proveedor"], Cultureinfo); //costo con retenciones y fletes
                            decimal ivaEncabezado = Convert.ToDecimal(Request["valorIVA"], Cultureinfo); //valor total del iva
                            decimal descuentoEncabezado =
                                Convert.ToDecimal(Request["valorDes"], Cultureinfo); //valor total del descuento
                            decimal costoEncabezado = Convert.ToDecimal(Request["valorSub"], Cultureinfo); //valor antes de impuestos

                            decimal valor_totalenca = costoEncabezado - descuentoEncabezado;

                            //consecutivo
                            grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                                x.documento_id == modelo.tipo && x.bodega_id == bodega);
                            if (grupo != null)
                            {
                                DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                                long consecutivo = doc.BuscarConsecutivo(grupo.grupo);

                                //Encabezado documento

                                #region encabezado

                                encab_documento encabezado = new encab_documento
                                {
                                    tipo = modelo.tipo,
                                    numero = consecutivo,
                                    nit = modelo.nit,
                                    fecha = DateTime.Now
                                };
                                int? condicion = modelo.fpago_id;
                                encabezado.fpago_id = condicion;
                                int dias = context.fpago_tercero.Find(condicion).dvencimiento ?? 0;
                                DateTime vencimiento = DateTime.Now.AddDays(dias);
                                encabezado.vencimiento = vencimiento;
                                encabezado.valor_total = costoTotal;
                                encabezado.iva = ivaEncabezado;
                                // Validacion para reteIVA, reteICA y retencion dependiendo del proveedor seleccionado

                                #region calculo de retenciones

                                tp_doc_registros buscarTipoDocRegistro =
                                    context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == modelo.tipo);
                                tercero_proveedor buscarProveedor =
                                    context.tercero_proveedor.FirstOrDefault(x => x.tercero_id == modelo.nit);
                                int regimen_proveedor = buscarProveedor != null ? buscarProveedor.tpregimen_id : 0;
                                perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                                    x.bodega == bodega && x.sw == buscarTipoDocRegistro.sw &&
                                    x.tipo_regimenid == regimen_proveedor);

                                decimal retenciones = 0;

                                if (buscarPerfilTributario != null)
                                {
                                    if (buscarPerfilTributario.retfuente == "A" &&
                                        valor_totalenca >= buscarTipoDocRegistro.baseretencion)
                                    {
                                        encabezado.porcen_retencion = buscarTipoDocRegistro.retencion;
                                        encabezado.retencion =
                                            Math.Round(valor_totalenca *
                                                       Convert.ToDecimal(buscarTipoDocRegistro.retencion / 100, Cultureinfo));
                                        retenciones += encabezado.retencion;
                                    }

                                    if (buscarPerfilTributario.retiva == "A" &&
                                        ivaEncabezado >= buscarTipoDocRegistro.baseiva)
                                    {
                                        encabezado.porcen_reteiva = buscarTipoDocRegistro.retiva;
                                        encabezado.retencion_iva =
                                            Math.Round(encabezado.iva *
                                                       Convert.ToDecimal(buscarTipoDocRegistro.retiva / 100, Cultureinfo));
                                        retenciones += encabezado.retencion_iva;
                                    }

                                    if (buscarPerfilTributario.autorretencion == "A")
                                    {
                                        decimal tercero_acteco = buscarProveedor.acteco_tercero.autorretencion;
                                        encabezado.porcen_autorretencion = (float)tercero_acteco;
                                        encabezado.retencion_causada =
                                            Math.Round(valor_totalenca * Convert.ToDecimal(tercero_acteco / 100, Cultureinfo));
                                        retenciones += encabezado.retencion_causada;
                                    }

                                    if (buscarPerfilTributario.retica == "A" &&
                                        valor_totalenca >= buscarTipoDocRegistro.baseica)
                                    {
                                        terceros_bod_ica bodega_acteco = context.terceros_bod_ica.FirstOrDefault(x =>
                                            x.idcodica == buscarProveedor.acteco_id && x.bodega == bodega);
                                        decimal tercero_acteco = buscarProveedor.acteco_tercero.tarifa;
                                        if (bodega_acteco != null)
                                        {
                                            encabezado.porcen_retica = (float)bodega_acteco.porcentaje;
                                            encabezado.retencion_ica =
                                                Math.Round(valor_totalenca *
                                                           Convert.ToDecimal(bodega_acteco.porcentaje / 1000, Cultureinfo));
                                            retenciones += encabezado.retencion_ica;
                                        }

                                        if (tercero_acteco != 0)
                                        {
                                            encabezado.porcen_retica = (float)buscarProveedor.acteco_tercero.tarifa;
                                            encabezado.retencion_ica =
                                                Math.Round(valor_totalenca *
                                                           Convert.ToDecimal(
                                                               buscarProveedor.acteco_tercero.tarifa / 1000, Cultureinfo));
                                            retenciones += encabezado.retencion_ica;
                                        }
                                        else
                                        {
                                            encabezado.porcen_retica = buscarTipoDocRegistro.retica;
                                            encabezado.retencion_ica =
                                                Math.Round(valor_totalenca *
                                                           Convert.ToDecimal(buscarTipoDocRegistro.retica / 1000, Cultureinfo));
                                            retenciones += encabezado.retencion_ica;
                                        }
                                    }
                                }

                                #endregion

                                if (modelo.fletes != null)
                                {
                                    encabezado.fletes = Convert.ToDecimal(modelo.fletes, Cultureinfo);
                                    encabezado.iva_fletes = Convert.ToDecimal(modelo.iva_fletes, Cultureinfo);
                                }

                                encabezado.costo = costoPromedioTotal;
                                encabezado.vendedor = Convert.ToInt32(Request["vendedor"]);
                                encabezado.perfilcontable = Convert.ToInt32(Request["perfilcontable"]);
                                string pedido = Request["pedido"];
                                if (!string.IsNullOrEmpty(pedido))
                                {
                                    encabezado.pedido = Convert.ToInt32(Request["pedido"]);
                                }

                                encabezado.bodega = bodega;
                                encabezado.moneda = Convert.ToInt32(Request["moneda"]);
                                if (Request["tasa"] != "")
                                {
                                    encabezado.tasa = Convert.ToInt32(Request["tasa"]);
                                }

                                encabezado.valor_mercancia = valor_totalenca;
                                encabezado.fec_creacion = DateTime.Now;
                                encabezado.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                encabezado.estado = true;
                                encabezado.concepto = modelo.concepto;
                                encabezado.concepto2 = modelo.concepto2;

                                context.encab_documento.Add(encabezado);
                                context.SaveChanges();

                                #endregion

                                int id_encabezado = context.encab_documento.OrderByDescending(x => x.idencabezado)
                                    .FirstOrDefault().idencabezado;

                                encab_documento eg = context.encab_documento.FirstOrDefault(x => x.idencabezado == id_encabezado);

                                //Mov Contable

                                #region movimientos contables

                                //buscamos en perfil cuenta documento, por medio del perfil seleccionado

                                foreach (var parametro in parametrosCuentasVerificar)
                                {
                                    string descripcionParametro = context.paramcontablenombres
                                        .FirstOrDefault(x => x.id == parametro.id_nombre_parametro)
                                        .descripcion_parametro;
                                    cuenta_puc buscarCuenta =
                                        context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);

                                    if (buscarCuenta != null)
                                    {
                                        if (parametro.id_nombre_parametro == 10 &&
                                            Convert.ToDecimal(valor_totalenca, Cultureinfo) != 0
                                            || parametro.id_nombre_parametro == 3 &&
                                            Convert.ToDecimal(eg.retencion, Cultureinfo) != 0
                                            || parametro.id_nombre_parametro == 4 &&
                                            Convert.ToDecimal(eg.retencion_iva, Cultureinfo) != 0
                                            || parametro.id_nombre_parametro == 5 &&
                                            Convert.ToDecimal(eg.retencion_ica, Cultureinfo) != 0
                                            || parametro.id_nombre_parametro == 6 && Convert.ToDecimal(eg.fletes, Cultureinfo) != 0
                                            || parametro.id_nombre_parametro == 14 &&
                                            Convert.ToDecimal(eg.iva_fletes, Cultureinfo) != 0
                                            || parametro.id_nombre_parametro == 17 &&
                                            Convert.ToDecimal(eg.retencion_causada, Cultureinfo) != 0
                                            || parametro.id_nombre_parametro == 18 &&
                                            Convert.ToDecimal(eg.retencion_causada, Cultureinfo) != 0)
                                        {
                                            mov_contable movNuevo = new mov_contable
                                            {
                                                id_encab = eg.idencabezado,
                                                seq = secuencia,
                                                idparametronombre = parametro.id_nombre_parametro,
                                                cuenta = parametro.cuenta,
                                                centro = parametro.centro,
                                                fec = DateTime.Now,
                                                fec_creacion = DateTime.Now,
                                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                                documento = Convert.ToString(modelo.pedido),
                                                detalle = "Facturacion de repuestos con consecutivo " + eg.numero,
                                                estado = true
                                            };

                                            cuenta_puc info = context.cuenta_puc.Where(a => a.cntpuc_id == parametro.cuenta)
                                                .FirstOrDefault();

                                            if (info.tercero)
                                            {
                                                movNuevo.nit = modelo.nit;
                                            }
                                            else
                                            {
                                                icb_terceros tercero = context.icb_terceros.Where(t => t.doc_tercero == "0")
                                                    .FirstOrDefault();
                                                movNuevo.nit = tercero.tercero_id;
                                            }

                                            // las siguientes validaciones se hacen dependiendo de la cuenta que esta moviendo la compra manual, para guardar la informacion acorde

                                            #region Cuentas X Cobrar

                                            if (parametro.id_nombre_parametro == 10)
                                            {
                                                /*if (info.aplicaniff==true)
												{

												}*/

                                                if (info.manejabase)
                                                {
                                                    movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                                }
                                                else
                                                {
                                                    movNuevo.basecontable = 0;
                                                }

                                                if (info.documeto)
                                                {
                                                    movNuevo.documento = Convert.ToString(modelo.pedido);
                                                }

                                                if (buscarCuenta.concepniff == 1)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = Convert.ToDecimal(costoTotal, Cultureinfo);

                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif = Convert.ToDecimal(costoTotal, Cultureinfo);
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif = Convert.ToDecimal(costoTotal, Cultureinfo);
                                                }

                                                if (buscarCuenta.concepniff == 5)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = Convert.ToDecimal(costoTotal, Cultureinfo);
                                                }
                                            }

                                            #endregion

                                            #region Retencion

                                            if (parametro.id_nombre_parametro == 3)
                                            {
                                                /*if (info.aplicaniff==true)
												{

												}*/

                                                if (info.manejabase)
                                                {
                                                    movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                                }
                                                else
                                                {
                                                    movNuevo.basecontable = 0;
                                                }

                                                if (info.documeto)
                                                {
                                                    movNuevo.documento = modelo.documento;
                                                }

                                                if (buscarCuenta.concepniff == 1)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = eg.retencion;

                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif = eg.retencion;
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif = eg.retencion;
                                                }

                                                if (buscarCuenta.concepniff == 5)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = eg.retencion;
                                                }
                                            }

                                            #endregion

                                            #region ReteIVA

                                            if (parametro.id_nombre_parametro == 4)
                                            {
                                                /*if (info.aplicaniff==true)
												{

												}*/

                                                if (info.manejabase)
                                                {
                                                    movNuevo.basecontable = Convert.ToDecimal(ivaEncabezado, Cultureinfo);
                                                }
                                                else
                                                {
                                                    movNuevo.basecontable = 0;
                                                }

                                                if (info.documeto)
                                                {
                                                    movNuevo.documento = modelo.documento;
                                                }

                                                if (buscarCuenta.concepniff == 1)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = eg.retencion_iva;

                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif = eg.retencion_iva;
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif = eg.retencion_iva;
                                                }

                                                if (buscarCuenta.concepniff == 5)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = eg.retencion_iva;
                                                }
                                            }

                                            #endregion

                                            #region ReteICA

                                            if (parametro.id_nombre_parametro == 5)
                                            {
                                                /*if (info.aplicaniff==true)
												{

												}*/

                                                if (info.manejabase)
                                                {
                                                    movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                                }
                                                else
                                                {
                                                    movNuevo.basecontable = 0;
                                                }

                                                if (info.documeto)
                                                {
                                                    movNuevo.documento = modelo.documento;
                                                }

                                                if (buscarCuenta.concepniff == 1)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = eg.retencion_ica;

                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif = eg.retencion_ica;
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif = eg.retencion_ica;
                                                }

                                                if (buscarCuenta.concepniff == 5)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = eg.retencion_ica;
                                                }
                                            }

                                            #endregion

                                            #region Fletes

                                            if (parametro.id_nombre_parametro == 6)
                                            {
                                                /*if (info.aplicaniff==true)
												{

												}*/

                                                if (info.manejabase)
                                                {
                                                    movNuevo.basecontable = Convert.ToDecimal(modelo.fletes, Cultureinfo);
                                                }
                                                else
                                                {
                                                    movNuevo.basecontable = 0;
                                                }

                                                if (info.documeto)
                                                {
                                                    movNuevo.documento = modelo.documento;
                                                }

                                                if (buscarCuenta.concepniff == 1)
                                                {
                                                    movNuevo.credito = eg.fletes;
                                                    movNuevo.debito = 0;

                                                    movNuevo.creditoniif = eg.fletes;
                                                    movNuevo.debitoniif = 0;
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    movNuevo.creditoniif = eg.fletes;
                                                    ;
                                                    movNuevo.debitoniif = 0;
                                                }

                                                if (buscarCuenta.concepniff == 5)
                                                {
                                                    movNuevo.credito = eg.fletes;
                                                    movNuevo.debito = 0;
                                                }
                                            }

                                            #endregion

                                            #region IVA fletes

                                            if (parametro.id_nombre_parametro == 14)
                                            {
                                                /*if (info.aplicaniff==true)
												{

												}*/

                                                if (info.manejabase)
                                                {
                                                    movNuevo.basecontable = Convert.ToDecimal(modelo.fletes, Cultureinfo);
                                                }
                                                else
                                                {
                                                    movNuevo.basecontable = 0;
                                                }

                                                if (info.documeto)
                                                {
                                                    movNuevo.documento = modelo.documento;
                                                }

                                                if (buscarCuenta.concepniff == 1)
                                                {
                                                    movNuevo.credito = eg.iva_fletes;
                                                    movNuevo.debito = 0;

                                                    movNuevo.creditoniif = eg.iva_fletes;
                                                    movNuevo.debitoniif = 0;
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    movNuevo.creditoniif = eg.iva_fletes;
                                                    movNuevo.debitoniif = 0;
                                                }

                                                if (buscarCuenta.concepniff == 5)
                                                {
                                                    movNuevo.credito = eg.iva_fletes;
                                                    movNuevo.debito = 0;
                                                }
                                            }

                                            #endregion

                                            #region AutoRetencion Debito

                                            if (parametro.id_nombre_parametro == 17)
                                            {
                                                /*if (info.aplicaniff==true)
												{

												}*/

                                                if (info.manejabase)
                                                {
                                                    movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                                }
                                                else
                                                {
                                                    movNuevo.basecontable = 0;
                                                }

                                                if (info.documeto)
                                                {
                                                    movNuevo.documento = modelo.documento;
                                                }

                                                if (buscarCuenta.concepniff == 1)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = eg.retencion_causada;

                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif = eg.retencion_causada;
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif = eg.retencion_causada;
                                                }

                                                if (buscarCuenta.concepniff == 5)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = eg.retencion_causada;
                                                }
                                            }

                                            #endregion

                                            #region AutoRetencion Credito

                                            if (parametro.id_nombre_parametro == 18)
                                            {
                                                /*if (info.aplicaniff==true)
												{

												}*/

                                                if (info.manejabase)
                                                {
                                                    movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                                }
                                                else
                                                {
                                                    movNuevo.basecontable = 0;
                                                }

                                                if (info.documeto)
                                                {
                                                    movNuevo.documento = modelo.documento;
                                                }

                                                if (buscarCuenta.concepniff == 1)
                                                {
                                                    movNuevo.credito = eg.retencion_causada;
                                                    movNuevo.debito = 0;

                                                    movNuevo.creditoniif = eg.retencion_causada;
                                                    movNuevo.debitoniif = 0;
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    movNuevo.creditoniif = eg.retencion_causada;
                                                    movNuevo.debitoniif = 0;
                                                }

                                                if (buscarCuenta.concepniff == 5)
                                                {
                                                    movNuevo.credito = eg.retencion_causada;
                                                    movNuevo.debito = 0;
                                                }
                                            }

                                            #endregion

                                            context.mov_contable.Add(movNuevo);
                                            //context.SaveChanges();

                                            secuencia++;
                                            //Cuentas valores

                                            #region Cuentas valores

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
                                                //context.SaveChanges();
                                            }

                                            #endregion

                                            totalCreditos += movNuevo.credito;
                                            totalDebitos += movNuevo.debito;
                                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                                            {
                                                NumeroCuenta =
                                                    "(" + buscarCuenta.cntpuc_numero + ")" + buscarCuenta.cntpuc_descp,
                                                DescripcionParametro = descripcionParametro,
                                                ValorDebito = movNuevo.debito,
                                                ValorCredito = movNuevo.credito
                                            });
                                        }
                                    }
                                }

                                #endregion

                                //Lineas documento

                                #region lineasDocumento

                                List<mov_contable> listaMov = new List<mov_contable>();
                                int listaLineas = Convert.ToInt32(Request["lista_referencias"]);
                                for (int i = 0; i <= listaLineas; i++)
                                    if (!string.IsNullOrEmpty(Request["referencia" + i]))
                                    {
                                        decimal porDescuento = !string.IsNullOrEmpty(Request["descuentoReferencia" + i])
                                            ? Convert.ToDecimal(Request["descuentoReferencia" + i], Cultureinfo)
                                            : 0;

                                        string codigo = Request["referencia" + i];
                                        decimal cantidadFacturada = Convert.ToDecimal(Request["cantidadReferencia" + i], Cultureinfo);
                                        decimal valorReferencia = Convert.ToDecimal(Request["valorUnitarioReferencia" + i], Cultureinfo);
                                        decimal descontar = porDescuento / 100;
                                        decimal porIVAReferencia = Convert.ToDecimal(Request["ivaReferencia" + i], Cultureinfo) / 100;
                                        decimal final = Math.Round(valorReferencia - valorReferencia * descontar);
                                        decimal baseUnitario = final * Convert.ToDecimal(Request["cantidadReferencia" + i], Cultureinfo);
                                        decimal ivaReferencia =
                                            Math.Round(final * porIVAReferencia *
                                                       Convert.ToDecimal(Request["cantidadReferencia" + i], Cultureinfo));
                                        icb_referencia unidadCodigo =
                                            context.icb_referencia.FirstOrDefault(x => x.ref_codigo == codigo);
                                        string und = unidadCodigo.unidad_medida;

                                        // var vwPromedio = context.vw_promedio.FirstOrDefault(x => x.codigo == codigo && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month);


                                        decimal costoReferencia = valorReferencia;

                                        // var vwPromedio =
                                        //var costoReferencia = vwPromedio.Promedio;
                                        decimal cr = Convert.ToDecimal(costoReferencia, Cultureinfo) *
                                                 Convert.ToDecimal(Request["cantidadReferencia" + i], Cultureinfo);

                                        if (!string.IsNullOrEmpty(Request["pedidoID" + i]))
                                        {
                                            int pedidoSeleccionado = Convert.ToInt32(Request["pedidoID" + i]);

                                            icb_referencia_movdetalle buscar_movimientoPedido =
                                                context.icb_referencia_movdetalle.FirstOrDefault(x =>
                                                    x.refmov_id == pedidoSeleccionado && x.ref_codigo == codigo);
                                            if (buscar_movimientoPedido != null)
                                            {
                                                if (buscar_movimientoPedido.refdet_saldo != null)
                                                {
                                                    buscar_movimientoPedido.refdet_saldo += cantidadFacturada;
                                                }
                                                else
                                                {
                                                    buscar_movimientoPedido.refdet_saldo = cantidadFacturada;
                                                }

                                                context.Entry(buscar_movimientoPedido).State = EntityState.Modified;
                                            }
                                        }

                                        lineas_documento lineas = new lineas_documento
                                        {
                                            id_encabezado = id_encabezado,
                                            codigo = Request["referencia" + i],
                                            seq = i + 1,
                                            fec = DateTime.Now,
                                            nit = modelo.nit,
                                            und = Convert.ToString(und),
                                            cantidad = Convert.ToDecimal(Request["cantidadReferencia" + i], Cultureinfo)
                                        };
                                        decimal ivaLista = Convert.ToDecimal(Request["ivaReferencia" + i], Cultureinfo);
                                        lineas.porcentaje_iva = (float)ivaLista;
                                        lineas.valor_unitario = final;
                                        decimal descuento = porDescuento;
                                        lineas.porcentaje_descuento = (float)descuento;
                                        lineas.costo_unitario = Convert.ToDecimal(costoReferencia, Cultureinfo);
                                        lineas.bodega = bodega;
                                        lineas.fec_creacion = DateTime.Now;
                                        lineas.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                        lineas.estado = true;
                                        lineas.moneda = Convert.ToInt32(Request["moneda"]);
                                        if (Request["tasa"] != "")
                                        {
                                            lineas.tasa = Convert.ToInt32(Request["tasa"]);
                                        }

                                        lineas.vendedor = eg.vendedor;

                                        context.lineas_documento.Add(lineas);

                                        #endregion

                                        //Referencias Inven

                                        #region referencias inven

                                        string codigoRef = Request["referencia" + i];
                                        var Si_Inv = (from refe in context.icb_referencia
                                                      where refe.ref_codigo == codigoRef
                                                      select new
                                                      { opcion = refe.manejo_inv }).FirstOrDefault();

                                        bool opcionSi = Si_Inv.opcion;


                                        int anio = DateTime.Now.Year;
                                        int mes = DateTime.Now.Month;


                                        if (opcionSi)
                                        {
                                            referencias_inven refin = new referencias_inven();

                                            referencias_inven existencia = context.referencias_inven.FirstOrDefault(x =>
                                                x.ano == anio && x.mes == mes && x.codigo == codigo &&
                                                x.bodega == bodega);

                                            if (existencia != null)
                                            {
                                                existencia.codigo = codigo;
                                                existencia.can_sal +=
                                                    Convert.ToDecimal(Request["cantidadReferencia" + i], Cultureinfo);
                                                existencia.cos_sal +=
                                                    Convert.ToDecimal(
                                                        cr, Cultureinfo); //(final * Convert.ToDecimal(Request["cantidadReferencia" + i])); cambio solicitado por la ingeniera liliana el dia 10/09/18
                                                existencia.can_vta +=
                                                    Convert.ToDecimal(Request["cantidadReferencia" + i], Cultureinfo);
                                                existencia.cos_vta +=
                                                    Convert.ToDecimal(
                                                        cr, Cultureinfo); //(final * Convert.ToDecimal(Request["cantidadReferencia" + i])); cambio solicitado por la ingeniera liliana el dia 10/09/18
                                                existencia.val_vta += baseUnitario;
                                                context.Entry(existencia).State = EntityState.Modified;
                                            }
                                            else
                                            {
                                                refin.bodega = bodega;
                                                refin.codigo = codigo;
                                                refin.ano = Convert.ToInt16(DateTime.Now.Year);
                                                refin.mes = Convert.ToInt16(DateTime.Now.Month);
                                                refin.can_sal = Convert.ToDecimal(Request["cantidadReferencia" + i], Cultureinfo);
                                                refin.cos_sal =
                                                    Convert.ToDecimal(
                                                        cr, Cultureinfo); //final; cambio solicitado por la ingeniera liliana el dia 10/09/18
                                                refin.can_vta = Convert.ToDecimal(Request["cantidadReferencia" + i]);
                                                refin.cos_vta =
                                                    Convert.ToDecimal(
                                                        cr, Cultureinfo); //final; cambio solicitado por la ingeniera liliana el dia 10/09/18
                                                refin.val_vta = baseUnitario;
                                                refin.modulo = "R";
                                                context.referencias_inven.Add(refin);
                                            }
                                        }

                                        #endregion

                                        //Mov Contable (IVA, Inventario, Costo, Ingreso)

                                        #region Mov Contable (IVA, Inventario, Costo, Ingreso)

                                        foreach (var parametro in parametrosCuentasVerificar)
                                        {
                                            string descripcionParametro = context.paramcontablenombres
                                                .FirstOrDefault(x => x.id == parametro.id_nombre_parametro)
                                                .descripcion_parametro;
                                            cuenta_puc buscarCuenta =
                                                context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);

                                            if (buscarCuenta != null)
                                            {
                                                if (parametro.id_nombre_parametro == 2 &&
                                                    Convert.ToDecimal(ivaEncabezado, Cultureinfo) != 0
                                                    || parametro.id_nombre_parametro == 9 &&
                                                    Convert.ToDecimal(costoPromedioTotal, Cultureinfo) != 0 //costo promedio
                                                    || parametro.id_nombre_parametro == 20 &&
                                                    Convert.ToDecimal(costoPromedioTotal, Cultureinfo) != 0 //costo promedio
                                                    || parametro.id_nombre_parametro == 11 &&
                                                    Convert.ToDecimal(costoEncabezado, Cultureinfo) != 0
                                                    || parametro.id_nombre_parametro == 12 &&
                                                    Convert.ToDecimal(costoPromedioTotal, Cultureinfo) != 0) //costo promedio
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
                                                        userid_creacion =
                                                        Convert.ToInt32(Session["user_usuarioid"]),
                                                        documento = Convert.ToString(modelo.pedido)
                                                    };

                                                    cuenta_puc info = context.cuenta_puc
                                                        .Where(a => a.cntpuc_id == parametro.cuenta).FirstOrDefault();

                                                    if (info.tercero)
                                                    {
                                                        movNuevo.nit = modelo.nit;
                                                    }
                                                    else
                                                    {
                                                        icb_terceros tercero = context.icb_terceros
                                                            .Where(t => t.doc_tercero == "0").FirstOrDefault();
                                                        movNuevo.nit = tercero.tercero_id;
                                                    }

                                                    #region IVA

                                                    if (parametro.id_nombre_parametro == 2)
                                                    {
                                                        icb_referencia perfilReferencia =
                                                            context.icb_referencia.FirstOrDefault(x =>
                                                                x.ref_codigo == lineas.codigo);
                                                        int perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                                        perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(r =>
                                                            r.id == perfilBuscar);

                                                        #region Tiene perfil la referencia

                                                        if (pcr != null)
                                                        {
                                                            int? cuentaIva = pcr.cuenta_dev_iva_compras;

                                                            movNuevo.id_encab = encabezado.idencabezado;
                                                            movNuevo.seq = secuencia;
                                                            movNuevo.idparametronombre = parametro.id_nombre_parametro;

                                                            #region si tiene perfil y cuenta asignada a ese perfil

                                                            if (cuentaIva != null)
                                                            {
                                                                movNuevo.cuenta = Convert.ToInt32(cuentaIva);
                                                                movNuevo.centro = parametro.centro;
                                                                movNuevo.fec = DateTime.Now;
                                                                movNuevo.fec_creacion = DateTime.Now;
                                                                movNuevo.userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]);
                                                                movNuevo.documento = Convert.ToString(eg.numero);

                                                                cuenta_puc infoReferencia = context.cuenta_puc
                                                                    .Where(a => a.cntpuc_id == cuentaIva)
                                                                    .FirstOrDefault();
                                                                if (infoReferencia.manejabase)
                                                                {
                                                                    movNuevo.basecontable =
                                                                        Convert.ToDecimal(baseUnitario, Cultureinfo);
                                                                }
                                                                else
                                                                {
                                                                    movNuevo.basecontable = 0;
                                                                }

                                                                if (infoReferencia.documeto)
                                                                {
                                                                    movNuevo.documento = Convert.ToString(eg.numero);
                                                                }

                                                                if (infoReferencia.concepniff == 1)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                                                    movNuevo.debito = 0;

                                                                    movNuevo.creditoniif =
                                                                        Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (infoReferencia.concepniff == 4)
                                                                {
                                                                    movNuevo.creditoniif =
                                                                        Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (infoReferencia.concepniff == 5)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                                                    movNuevo.debito = 0;
                                                                }

                                                                // context.mov_contable.Add(movNuevo);
                                                            }

                                                            #endregion

                                                            #region si tiene perfil pero no tiene cuenta asignada

                                                            else
                                                            {
                                                                movNuevo.cuenta = parametro.cuenta;
                                                                movNuevo.centro = parametro.centro;
                                                                movNuevo.fec = DateTime.Now;
                                                                movNuevo.fec_creacion = DateTime.Now;
                                                                movNuevo.userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]);
                                                                movNuevo.documento = Convert.ToString(eg.numero);

                                                                cuenta_puc infoReferencia = context.cuenta_puc
                                                                    .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                    .FirstOrDefault();
                                                                if (infoReferencia.manejabase)
                                                                {
                                                                    movNuevo.basecontable =
                                                                        Convert.ToDecimal(baseUnitario, Cultureinfo);
                                                                }
                                                                else
                                                                {
                                                                    movNuevo.basecontable = 0;
                                                                }

                                                                if (infoReferencia.documeto)
                                                                {
                                                                    movNuevo.documento = Convert.ToString(eg.numero);
                                                                }

                                                                if (infoReferencia.concepniff == 1)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                                                    movNuevo.debito = 0;

                                                                    movNuevo.creditoniif =
                                                                        Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (infoReferencia.concepniff == 4)
                                                                {
                                                                    movNuevo.creditoniif =
                                                                        Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (infoReferencia.concepniff == 5)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                                                    movNuevo.debito = 0;
                                                                }

                                                                //context.mov_contable.Add(movNuevo);
                                                            }

                                                            #endregion
                                                        }

                                                        #endregion

                                                        #region La referencia no tiene perfil

                                                        else
                                                        {
                                                            movNuevo.id_encab = encabezado.idencabezado;
                                                            movNuevo.seq = secuencia;
                                                            movNuevo.idparametronombre = parametro.id_nombre_parametro;
                                                            movNuevo.cuenta = parametro.cuenta;
                                                            movNuevo.centro = parametro.centro;
                                                            movNuevo.fec = DateTime.Now;
                                                            movNuevo.fec_creacion = DateTime.Now;
                                                            movNuevo.userid_creacion =
                                                                Convert.ToInt32(Session["user_usuarioid"]);
                                                            /*if (info.aplicaniff==true)
															{

															}*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(baseUnitario, Cultureinfo);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = Convert.ToString(eg.numero);
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                                                movNuevo.debito = 0;

                                                                movNuevo.creditoniif = Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                                                movNuevo.debito = 0;
                                                            }

                                                            //context.mov_contable.Add(movNuevo);
                                                        }

                                                        #endregion

                                                        mov_contable buscarIVA = context.mov_contable.FirstOrDefault(x =>
                                                            x.id_encab == id_encabezado &&
                                                            x.cuenta == movNuevo.cuenta &&
                                                            x.idparametronombre == parametro.id_nombre_parametro);
                                                        if (buscarIVA != null)
                                                        {
                                                            buscarIVA.debito += movNuevo.debito;
                                                            buscarIVA.debitoniif += movNuevo.debitoniif;
                                                            buscarIVA.credito += movNuevo.credito;
                                                            buscarIVA.creditoniif += movNuevo.creditoniif;
                                                            context.Entry(buscarIVA).State = EntityState.Modified;
                                                        }
                                                        else
                                                        {
                                                            mov_contable crearMovContable = new mov_contable
                                                            {
                                                                id_encab = encabezado.idencabezado,
                                                                seq = secuencia,
                                                                idparametronombre =
                                                                parametro.id_nombre_parametro,
                                                                cuenta = Convert.ToInt32(movNuevo.cuenta),
                                                                centro = parametro.centro,
                                                                nit = encabezado.nit,
                                                                fec = DateTime.Now,
                                                                debito = movNuevo.debito,
                                                                debitoniif = movNuevo.debitoniif,
                                                                basecontable = movNuevo.basecontable,
                                                                credito = movNuevo.credito,
                                                                creditoniif = movNuevo.creditoniif,
                                                                fec_creacion = DateTime.Now,
                                                                userid_creacion =
                                                                Convert.ToInt32(Session["user_usuarioid"]),
                                                                detalle =
                                                                "Facturacion de repuestos con consecutivo " + eg.numero,
                                                                estado = true
                                                            };
                                                            context.mov_contable.Add(crearMovContable);
                                                            context.SaveChanges();
                                                        }
                                                    }

                                                    #endregion

                                                    #region Inventario				

                                                    if (parametro.id_nombre_parametro == 9 ||
                                                        parametro.id_nombre_parametro == 20)
                                                    {
                                                        icb_referencia perfilReferencia =
                                                            context.icb_referencia.FirstOrDefault(x =>
                                                                x.ref_codigo == lineas.codigo);
                                                        int perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                                        perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(r =>
                                                            r.id == perfilBuscar);

                                                        #region Tiene perfil la referencia

                                                        if (pcr != null)
                                                        {
                                                            int? cuentaInven = pcr.cta_inventario;

                                                            movNuevo.id_encab = encabezado.idencabezado;
                                                            movNuevo.seq = secuencia;
                                                            movNuevo.idparametronombre = parametro.id_nombre_parametro;

                                                            #region tiene perfil y cuenta asignada al perfil

                                                            if (cuentaInven != null)
                                                            {
                                                                movNuevo.cuenta = Convert.ToInt32(cuentaInven);
                                                                movNuevo.centro = parametro.centro;
                                                                movNuevo.fec = DateTime.Now;
                                                                movNuevo.fec_creacion = DateTime.Now;
                                                                movNuevo.userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]);
                                                                movNuevo.documento = Convert.ToString(eg.numero);

                                                                cuenta_puc infoReferencia = context.cuenta_puc
                                                                    .Where(a => a.cntpuc_id == cuentaInven)
                                                                    .FirstOrDefault();
                                                                if (infoReferencia.manejabase)
                                                                {
                                                                    movNuevo.basecontable =
                                                                        Convert.ToDecimal(baseUnitario, Cultureinfo);
                                                                }
                                                                else
                                                                {
                                                                    movNuevo.basecontable = 0;
                                                                }

                                                                if (infoReferencia.documeto)
                                                                {
                                                                    movNuevo.documento = Convert.ToString(eg.numero);
                                                                }

                                                                if (infoReferencia.concepniff == 1)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(cr, Cultureinfo);
                                                                    movNuevo.debito = 0;

                                                                    movNuevo.creditoniif = Convert.ToDecimal(cr, Cultureinfo);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (infoReferencia.concepniff == 4)
                                                                {
                                                                    movNuevo.creditoniif = Convert.ToDecimal(cr, Cultureinfo);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (infoReferencia.concepniff == 5)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(cr, Cultureinfo);
                                                                    movNuevo.debito = 0;
                                                                }

                                                                //context.mov_contable.Add(movNuevo);
                                                            }

                                                            #endregion

                                                            #region tiene perfil pero no tiene cuenta asignada

                                                            else
                                                            {
                                                                movNuevo.cuenta = parametro.cuenta;
                                                                movNuevo.centro = parametro.centro;
                                                                movNuevo.fec = DateTime.Now;
                                                                movNuevo.fec_creacion = DateTime.Now;
                                                                movNuevo.userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]);
                                                                movNuevo.documento = Convert.ToString(eg.numero);

                                                                cuenta_puc infoReferencia = context.cuenta_puc
                                                                    .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                    .FirstOrDefault();
                                                                if (infoReferencia.manejabase)
                                                                {
                                                                    movNuevo.basecontable =
                                                                        Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                                                }
                                                                else
                                                                {
                                                                    movNuevo.basecontable = 0;
                                                                }

                                                                if (infoReferencia.documeto)
                                                                {
                                                                    movNuevo.documento = Convert.ToString(eg.numero);
                                                                }

                                                                if (infoReferencia.concepniff == 1)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(cr, Cultureinfo);
                                                                    movNuevo.debito = 0;

                                                                    movNuevo.creditoniif = Convert.ToDecimal(cr, Cultureinfo);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (infoReferencia.concepniff == 4)
                                                                {
                                                                    movNuevo.creditoniif = Convert.ToDecimal(cr, Cultureinfo);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (infoReferencia.concepniff == 5)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(cr, Cultureinfo);
                                                                    movNuevo.debito = 0;
                                                                }

                                                                //context.mov_contable.Add(movNuevo);
                                                            }

                                                            #endregion
                                                        }

                                                        #endregion

                                                        #region La referencia no tiene perfil

                                                        else
                                                        {
                                                            movNuevo.id_encab = encabezado.idencabezado;
                                                            movNuevo.seq = secuencia;
                                                            movNuevo.idparametronombre = parametro.id_nombre_parametro;
                                                            movNuevo.cuenta = parametro.cuenta;
                                                            movNuevo.centro = parametro.centro;
                                                            movNuevo.fec = DateTime.Now;
                                                            movNuevo.fec_creacion = DateTime.Now;
                                                            movNuevo.userid_creacion =
                                                                Convert.ToInt32(Session["user_usuarioid"]);
                                                            /*if (info.aplicaniff==true)
															{

															}*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable =
                                                                    Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = Convert.ToString(eg.numero);
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = Convert.ToDecimal(cr, Cultureinfo);
                                                                movNuevo.debito = 0;

                                                                movNuevo.creditoniif = Convert.ToDecimal(cr, Cultureinfo);
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = Convert.ToDecimal(cr, Cultureinfo);
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = Convert.ToDecimal(cr, Cultureinfo);
                                                                movNuevo.debito = 0;
                                                            }

                                                            //context.mov_contable.Add(movNuevo);
                                                        }

                                                        #endregion

                                                        mov_contable buscarInventario = context.mov_contable.FirstOrDefault(x =>
                                                            x.id_encab == id_encabezado &&
                                                            x.cuenta == movNuevo.cuenta &&
                                                            x.idparametronombre == parametro.id_nombre_parametro);
                                                        if (buscarInventario != null)
                                                        {
                                                            buscarInventario.basecontable += movNuevo.basecontable;
                                                            buscarInventario.debito += movNuevo.debito;
                                                            buscarInventario.debitoniif += movNuevo.debitoniif;
                                                            buscarInventario.credito += movNuevo.credito;
                                                            buscarInventario.creditoniif += movNuevo.creditoniif;
                                                            context.Entry(buscarInventario).State =
                                                                EntityState.Modified;
                                                        }
                                                        else
                                                        {
                                                            mov_contable crearMovContable = new mov_contable
                                                            {
                                                                id_encab = encabezado.idencabezado,
                                                                seq = secuencia,
                                                                idparametronombre =
                                                                parametro.id_nombre_parametro,
                                                                cuenta = Convert.ToInt32(movNuevo.cuenta),
                                                                centro = parametro.centro,
                                                                nit = encabezado.nit,
                                                                fec = DateTime.Now,
                                                                debito = movNuevo.debito,
                                                                debitoniif = movNuevo.debitoniif,
                                                                basecontable = movNuevo.basecontable,
                                                                credito = movNuevo.credito,
                                                                creditoniif = movNuevo.creditoniif,
                                                                fec_creacion = DateTime.Now,
                                                                userid_creacion =
                                                                Convert.ToInt32(Session["user_usuarioid"]),
                                                                detalle =
                                                                "Facturacion de repuestos con consecutivo " + eg.numero,
                                                                estado = true
                                                            };
                                                            context.mov_contable.Add(crearMovContable);
                                                            context.SaveChanges();
                                                        }
                                                    }

                                                    #endregion

                                                    #region Ingreso				

                                                    if (parametro.id_nombre_parametro == 11)
                                                    {
                                                        icb_referencia perfilReferencia =
                                                            context.icb_referencia.FirstOrDefault(x =>
                                                                x.ref_codigo == lineas.codigo);
                                                        int perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                                        perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(r =>
                                                            r.id == perfilBuscar);

                                                        #region Tiene perfil la referencia

                                                        if (pcr != null)
                                                        {
                                                            int? cuentaVenta = pcr.cuenta_ventas;

                                                            movNuevo.id_encab = encabezado.idencabezado;
                                                            movNuevo.seq = secuencia;
                                                            movNuevo.idparametronombre = parametro.id_nombre_parametro;

                                                            #region tiene perfil y cuenta asignada al perfil

                                                            if (cuentaVenta != null)
                                                            {
                                                                movNuevo.cuenta = Convert.ToInt32(cuentaVenta);
                                                                movNuevo.centro = parametro.centro;
                                                                movNuevo.fec = DateTime.Now;
                                                                movNuevo.fec_creacion = DateTime.Now;
                                                                movNuevo.userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]);
                                                                movNuevo.documento = Convert.ToString(eg.numero);

                                                                cuenta_puc infoReferencia = context.cuenta_puc
                                                                    .Where(a => a.cntpuc_id == cuentaVenta)
                                                                    .FirstOrDefault();
                                                                if (infoReferencia.manejabase)
                                                                {
                                                                    movNuevo.basecontable =
                                                                        Convert.ToDecimal(baseUnitario, Cultureinfo);
                                                                }
                                                                else
                                                                {
                                                                    movNuevo.basecontable = 0;
                                                                }

                                                                if (infoReferencia.documeto)
                                                                {
                                                                    movNuevo.documento = Convert.ToString(eg.numero);
                                                                }

                                                                if (infoReferencia.concepniff == 1)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(baseUnitario, Cultureinfo);
                                                                    movNuevo.debito = 0;

                                                                    movNuevo.creditoniif =
                                                                        Convert.ToDecimal(baseUnitario, Cultureinfo);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (infoReferencia.concepniff == 4)
                                                                {
                                                                    movNuevo.creditoniif =
                                                                        Convert.ToDecimal(baseUnitario, Cultureinfo);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (infoReferencia.concepniff == 5)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(baseUnitario, Cultureinfo);
                                                                    movNuevo.debito = 0;
                                                                }

                                                                //context.mov_contable.Add(movNuevo);
                                                            }

                                                            #endregion

                                                            #region tiene perfil pero no tiene cuenta asignada

                                                            else
                                                            {
                                                                movNuevo.cuenta = parametro.cuenta;
                                                                movNuevo.centro = parametro.centro;
                                                                movNuevo.fec = DateTime.Now;
                                                                movNuevo.fec_creacion = DateTime.Now;
                                                                movNuevo.userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]);
                                                                movNuevo.documento = Convert.ToString(eg.numero);

                                                                cuenta_puc infoReferencia = context.cuenta_puc
                                                                    .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                    .FirstOrDefault();
                                                                if (infoReferencia.manejabase)
                                                                {
                                                                    movNuevo.basecontable =
                                                                        Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                                                }
                                                                else
                                                                {
                                                                    movNuevo.basecontable = 0;
                                                                }

                                                                if (infoReferencia.documeto)
                                                                {
                                                                    movNuevo.documento = Convert.ToString(eg.numero);
                                                                }

                                                                if (infoReferencia.concepniff == 1)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(baseUnitario, Cultureinfo);
                                                                    movNuevo.debito = 0;

                                                                    movNuevo.creditoniif =
                                                                        Convert.ToDecimal(baseUnitario, Cultureinfo);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (infoReferencia.concepniff == 4)
                                                                {
                                                                    movNuevo.creditoniif =
                                                                        Convert.ToDecimal(baseUnitario, Cultureinfo);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (infoReferencia.concepniff == 5)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(baseUnitario, Cultureinfo);
                                                                    movNuevo.debito = 0;
                                                                }

                                                                //context.mov_contable.Add(movNuevo);
                                                            }

                                                            #endregion
                                                        }

                                                        #endregion

                                                        #region La referencia no tiene perfil

                                                        else
                                                        {
                                                            movNuevo.id_encab = encabezado.idencabezado;
                                                            movNuevo.seq = secuencia;
                                                            movNuevo.idparametronombre = parametro.id_nombre_parametro;
                                                            movNuevo.cuenta = parametro.cuenta;
                                                            movNuevo.centro = parametro.centro;
                                                            movNuevo.fec = DateTime.Now;
                                                            movNuevo.fec_creacion = DateTime.Now;
                                                            movNuevo.userid_creacion =
                                                                Convert.ToInt32(Session["user_usuarioid"]);
                                                            /*if (info.aplicaniff==true)
															{

															}*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable =
                                                                    Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = Convert.ToString(eg.numero);
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = Convert.ToDecimal(baseUnitario, Cultureinfo);
                                                                movNuevo.debito = 0;

                                                                movNuevo.creditoniif = Convert.ToDecimal(baseUnitario, Cultureinfo);
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = Convert.ToDecimal(baseUnitario, Cultureinfo);
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = Convert.ToDecimal(baseUnitario, Cultureinfo);
                                                                movNuevo.debito = 0;
                                                            }

                                                            //context.mov_contable.Add(movNuevo);
                                                        }

                                                        #endregion

                                                        mov_contable buscarVenta = context.mov_contable.FirstOrDefault(x =>
                                                            x.id_encab == id_encabezado &&
                                                            x.cuenta == movNuevo.cuenta &&
                                                            x.idparametronombre == parametro.id_nombre_parametro);
                                                        if (buscarVenta != null)
                                                        {
                                                            buscarVenta.basecontable += movNuevo.basecontable;
                                                            buscarVenta.debito += movNuevo.debito;
                                                            buscarVenta.debitoniif += movNuevo.debitoniif;
                                                            buscarVenta.credito += movNuevo.credito;
                                                            buscarVenta.creditoniif += movNuevo.creditoniif;
                                                            context.Entry(buscarVenta).State = EntityState.Modified;
                                                        }
                                                        else
                                                        {
                                                            mov_contable crearMovContable = new mov_contable
                                                            {
                                                                id_encab = encabezado.idencabezado,
                                                                seq = secuencia,
                                                                idparametronombre =
                                                                parametro.id_nombre_parametro,
                                                                cuenta = Convert.ToInt32(movNuevo.cuenta),
                                                                centro = parametro.centro,
                                                                nit = encabezado.nit,
                                                                fec = DateTime.Now,
                                                                debito = movNuevo.debito,
                                                                debitoniif = movNuevo.debitoniif,
                                                                basecontable = movNuevo.basecontable,
                                                                credito = movNuevo.credito,
                                                                creditoniif = movNuevo.creditoniif,
                                                                fec_creacion = DateTime.Now,
                                                                userid_creacion =
                                                                Convert.ToInt32(Session["user_usuarioid"]),
                                                                detalle =
                                                                "Facturacion de repuestos con consecutivo " + eg.numero,
                                                                estado = true
                                                            };
                                                            context.mov_contable.Add(crearMovContable);
                                                            context.SaveChanges();
                                                        }
                                                    }

                                                    #endregion

                                                    #region Costo				

                                                    if (parametro.id_nombre_parametro == 12)
                                                    {
                                                        icb_referencia perfilReferencia =
                                                            context.icb_referencia.FirstOrDefault(x =>
                                                                x.ref_codigo == lineas.codigo);
                                                        int perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                                        perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(r =>
                                                            r.id == perfilBuscar);

                                                        #region Tiene perfil la referencia

                                                        if (pcr != null)
                                                        {
                                                            int? cuentaCosto = pcr.cuenta_costo;

                                                            movNuevo.id_encab = encabezado.idencabezado;
                                                            movNuevo.seq = secuencia;
                                                            movNuevo.idparametronombre = parametro.id_nombre_parametro;

                                                            #region tiene perfil y cuenta asignada al perfil

                                                            if (cuentaCosto != null)
                                                            {
                                                                movNuevo.cuenta = Convert.ToInt32(cuentaCosto);
                                                                movNuevo.centro = parametro.centro;
                                                                movNuevo.fec = DateTime.Now;
                                                                movNuevo.fec_creacion = DateTime.Now;
                                                                movNuevo.userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]);
                                                                movNuevo.documento = Convert.ToString(eg.numero);

                                                                cuenta_puc infoReferencia = context.cuenta_puc
                                                                    .Where(a => a.cntpuc_id == cuentaCosto)
                                                                    .FirstOrDefault();
                                                                if (infoReferencia.manejabase)
                                                                {
                                                                    movNuevo.basecontable =
                                                                        Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                                                }
                                                                else
                                                                {
                                                                    movNuevo.basecontable = 0;
                                                                }

                                                                if (infoReferencia.documeto)
                                                                {
                                                                    movNuevo.documento = Convert.ToString(eg.numero);
                                                                }

                                                                if (infoReferencia.concepniff == 1)
                                                                {
                                                                    movNuevo.credito = 0;
                                                                    movNuevo.debito = Convert.ToDecimal(cr, Cultureinfo);

                                                                    movNuevo.creditoniif = 0;
                                                                    movNuevo.debitoniif = Convert.ToDecimal(cr, Cultureinfo);
                                                                }

                                                                if (infoReferencia.concepniff == 4)
                                                                {
                                                                    movNuevo.creditoniif = 0;
                                                                    movNuevo.debitoniif = Convert.ToDecimal(cr, Cultureinfo);
                                                                }

                                                                if (infoReferencia.concepniff == 5)
                                                                {
                                                                    movNuevo.credito = 0;
                                                                    movNuevo.debito = Convert.ToDecimal(cr, Cultureinfo);
                                                                }

                                                                //context.mov_contable.Add(movNuevo);
                                                            }

                                                            #endregion

                                                            #region tiene perfil pero no tiene cuenta asignada

                                                            else
                                                            {
                                                                movNuevo.cuenta = parametro.cuenta;
                                                                movNuevo.centro = parametro.centro;
                                                                movNuevo.fec = DateTime.Now;
                                                                movNuevo.fec_creacion = DateTime.Now;
                                                                movNuevo.userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]);
                                                                movNuevo.documento = Convert.ToString(eg.numero);

                                                                cuenta_puc infoReferencia = context.cuenta_puc
                                                                    .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                    .FirstOrDefault();
                                                                if (infoReferencia.manejabase)
                                                                {
                                                                    movNuevo.basecontable =
                                                                        Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                                                }
                                                                else
                                                                {
                                                                    movNuevo.basecontable = 0;
                                                                }

                                                                if (infoReferencia.documeto)
                                                                {
                                                                    movNuevo.documento = Convert.ToString(eg.numero);
                                                                }

                                                                if (infoReferencia.concepniff == 1)
                                                                {
                                                                    movNuevo.credito = 0;
                                                                    movNuevo.debito = Convert.ToDecimal(cr, Cultureinfo);

                                                                    movNuevo.creditoniif = 0;
                                                                    movNuevo.debitoniif = Convert.ToDecimal(cr, Cultureinfo);
                                                                }

                                                                if (infoReferencia.concepniff == 4)
                                                                {
                                                                    movNuevo.creditoniif = 0;
                                                                    movNuevo.debitoniif = Convert.ToDecimal(cr, Cultureinfo);
                                                                }

                                                                if (infoReferencia.concepniff == 5)
                                                                {
                                                                    movNuevo.credito = 0;
                                                                    movNuevo.debito = Convert.ToDecimal(cr, Cultureinfo);
                                                                }

                                                                //context.mov_contable.Add(movNuevo);
                                                            }

                                                            #endregion
                                                        }

                                                        #endregion

                                                        #region La referencia no tiene perfil

                                                        else
                                                        {
                                                            movNuevo.id_encab = encabezado.idencabezado;
                                                            movNuevo.seq = secuencia;
                                                            movNuevo.idparametronombre = parametro.id_nombre_parametro;
                                                            movNuevo.cuenta = parametro.cuenta;
                                                            movNuevo.centro = parametro.centro;
                                                            movNuevo.fec = DateTime.Now;
                                                            movNuevo.fec_creacion = DateTime.Now;
                                                            movNuevo.userid_creacion =
                                                                Convert.ToInt32(Session["user_usuarioid"]);
                                                            /*if (info.aplicaniff==true)
															{

															}*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable =
                                                                    Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = Convert.ToString(eg.numero);
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = Convert.ToDecimal(cr, Cultureinfo);

                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = Convert.ToDecimal(cr, Cultureinfo);
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = Convert.ToDecimal(cr, Cultureinfo);
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = Convert.ToDecimal(cr, Cultureinfo);
                                                            }

                                                            //context.mov_contable.Add(movNuevo);
                                                        }

                                                        #endregion

                                                        mov_contable buscarCosto = context.mov_contable.FirstOrDefault(x =>
                                                            x.id_encab == id_encabezado &&
                                                            x.cuenta == movNuevo.cuenta &&
                                                            x.idparametronombre == parametro.id_nombre_parametro);
                                                        if (buscarCosto != null)
                                                        {
                                                            buscarCosto.basecontable += movNuevo.basecontable;
                                                            buscarCosto.debito += movNuevo.debito;
                                                            buscarCosto.debitoniif += movNuevo.debitoniif;
                                                            buscarCosto.credito += movNuevo.credito;
                                                            buscarCosto.creditoniif += movNuevo.creditoniif;
                                                            context.Entry(buscarCosto).State = EntityState.Modified;
                                                        }
                                                        else
                                                        {
                                                            mov_contable crearMovContable = new mov_contable
                                                            {
                                                                id_encab = encabezado.idencabezado,
                                                                seq = secuencia,
                                                                idparametronombre =
                                                                parametro.id_nombre_parametro,
                                                                cuenta = Convert.ToInt32(movNuevo.cuenta),
                                                                centro = parametro.centro,
                                                                nit = encabezado.nit,
                                                                fec = DateTime.Now,
                                                                debito = movNuevo.debito,
                                                                debitoniif = movNuevo.debitoniif,
                                                                basecontable = movNuevo.basecontable,
                                                                credito = movNuevo.credito,
                                                                creditoniif = movNuevo.creditoniif,
                                                                fec_creacion = DateTime.Now,
                                                                userid_creacion =
                                                                Convert.ToInt32(Session["user_usuarioid"]),
                                                                detalle =
                                                                "Facturacion de repuestos con consecutivo " + eg.numero,
                                                                estado = true
                                                            };
                                                            context.mov_contable.Add(crearMovContable);
                                                            context.SaveChanges();
                                                        }
                                                    }

                                                    #endregion

                                                    secuencia++;
                                                    //Cuentas valores

                                                    #region Cuentas valores

                                                    cuentas_valores buscar_cuentas_valores =
                                                        context.cuentas_valores.FirstOrDefault(x =>
                                                            x.centro == parametro.centro &&
                                                            x.cuenta == movNuevo.cuenta && x.nit == movNuevo.nit);
                                                    if (buscar_cuentas_valores != null)
                                                    {
                                                        buscar_cuentas_valores.debito += Math.Round(movNuevo.debito);
                                                        buscar_cuentas_valores.credito += Math.Round(movNuevo.credito);
                                                        buscar_cuentas_valores.debitoniff +=
                                                            Math.Round(movNuevo.debitoniif);
                                                        buscar_cuentas_valores.creditoniff +=
                                                            Math.Round(movNuevo.creditoniif);
                                                        context.Entry(buscar_cuentas_valores).State =
                                                            EntityState.Modified;
                                                        //context.SaveChanges();
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
                                                            debito = Math.Round(movNuevo.debito),
                                                            credito = Math.Round(movNuevo.credito),
                                                            debitoniff = Math.Round(movNuevo.debitoniif),
                                                            creditoniff = Math.Round(movNuevo.creditoniif)
                                                        };
                                                        context.cuentas_valores.Add(crearCuentaValor);
                                                        context.SaveChanges();
                                                    }

                                                    #endregion

                                                    totalCreditos += Math.Round(movNuevo.credito);
                                                    totalDebitos += Math.Round(movNuevo.debito);
                                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                    {
                                                        NumeroCuenta =
                                                            "(" + buscarCuenta.cntpuc_numero + ")" +
                                                            buscarCuenta.cntpuc_descp,
                                                        DescripcionParametro = descripcionParametro,
                                                        ValorDebito = movNuevo.debito,
                                                        ValorCredito = movNuevo.credito
                                                    });
                                                }
                                            }
                                        }

                                        #endregion
                                    }

                                #region validaciones para guardar

                                if (Math.Round(totalDebitos) != Math.Round(totalCreditos))
                                {
                                    TempData["documento_descuadrado"] =
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
                                    return View(modelo);
                                    //return RedirectToAction("Facturar", "FacturacionRepuestos", new { menu });
                                }

                                funciono = 1;

                                #endregion

                                if (funciono > 0)
                                {
                                    context.SaveChanges();
                                    dbTran.Commit();
                                    TempData["mensaje"] = "Registro creado correctamente";
                                    DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
                                    doc.ActualizarConsecutivo(grupo.grupo, consecutivo);

                                    return RedirectToAction("Facturar", "FacturacionOtros", new { menu });
                                }
                            }
                            else
                            {
                                TempData["mensaje_error"] = "no hay consecutivo";
                            }
                        }
                        //cierre
                        else
                        {
                            TempData["mensaje_error"] = "Lista vacia";
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

            listas();
            BuscarFavoritos(menu);
            return RedirectToAction("Facturar", "FacturacionOtros", new { menu });
        }

        public ActionResult DetalleFactura(int id, int? menu)
        {
            listas();
            lineas_documento lineas = context.lineas_documento.FirstOrDefault(x => x.id_encabezado == id);
            ViewBag.numero = lineas.encab_documento.numero;
            ViewBag.valor_total = lineas.encab_documento.valor_total;
            ViewBag.fecha = lineas.encab_documento.fecha;
            ViewBag.facid = lineas.encab_documento.idencabezado;
            ViewBag.idEncabezado = id;
            ViewBag.factura = context.encab_documento.Where(x => x.idencabezado == id).Select(x => x.numero)
                .FirstOrDefault();
            ViewBag.fecha = context.encab_documento.Where(x => x.idencabezado == id).Select(x => x.fec_creacion)
                .FirstOrDefault();
            ViewBag.fechaVencimiento = context.encab_documento.Where(x => x.idencabezado == id)
                .Select(x => x.vencimiento).FirstOrDefault();
            ViewBag.tipoDocumento = (from a in context.encab_documento
                                     join b in context.tp_doc_registros
                                         on a.tipo equals b.tpdoc_id
                                     where a.idencabezado == id
                                     select b.tpdoc_nombre).FirstOrDefault();

            ViewBag.bodega = (from a in context.encab_documento
                              join b in context.bodega_concesionario
                                  on a.bodega equals b.id
                              where a.idencabezado == id
                              select b.bodccs_nombre).FirstOrDefault();

            var cliente = (from a in context.encab_documento
                           join b in context.icb_terceros
                               on a.nit equals b.tercero_id
                           where a.idencabezado == id
                           select new
                           {
                               b.prinom_tercero,
                               b.segnom_tercero,
                               b.apellido_tercero,
                               b.segapellido_tercero
                           }).FirstOrDefault();
            ViewBag.cliente = cliente.prinom_tercero + " " + cliente.segnom_tercero + " " + cliente.apellido_tercero +
                              " " + cliente.segapellido_tercero;

            ViewBag.tipoPago = (from c in context.encab_documento
                                join f in context.fpago_tercero
                                    on c.fpago_id equals f.fpago_id
                                where c.idencabezado == id
                                select f.fpago_nombre).FirstOrDefault();

            ViewBag.moneda = (from c in context.encab_documento
                              join f in context.monedas
                                  on c.moneda equals f.moneda
                              where c.idencabezado == id
                              select f.descripcion).FirstOrDefault();

            ViewBag.perfil = (from a in context.encab_documento
                              join b in context.perfil_contable_documento
                                  on a.perfilcontable equals b.id
                              where a.idencabezado == id
                              select b.descripcion).FirstOrDefault();

            ViewBag.fletes = (from a in context.encab_documento
                              where a.idencabezado == id
                              select a.fletes).FirstOrDefault();

            ViewBag.ivafletes = (from a in context.encab_documento
                                 where a.idencabezado == id
                                 select a.iva_fletes).FirstOrDefault();

            var asesor = (from a in context.encab_documento
                          join b in context.users
                              on a.vendedor equals b.user_id
                          where a.idencabezado == id
                          select new
                          {
                              b.user_nombre,
                              b.user_apellido
                          }).FirstOrDefault();

            ViewBag.asesor = asesor.user_nombre + " " + asesor.user_apellido;

            ViewBag.concepto1 = (from a in context.encab_documento
                                 join b in context.tpdocconceptos
                                     on a.concepto equals b.id
                                 where a.idencabezado == id
                                 select b.Descripcion).FirstOrDefault();

            ViewBag.concepto2 = (from a in context.encab_documento
                                 join b in context.tpdocconceptos2
                                     on a.concepto equals b.id
                                 where a.idencabezado == id
                                 select b.Descripcion).FirstOrDefault();

            ViewBag.observaciones = (from a in context.encab_documento
                                     where a.idencabezado == id
                                     select a.notas != null ? a.notas : "").FirstOrDefault();

            ViewBag.pedido = (from a in context.encab_documento
                              join b in context.icb_referencia_mov
                                  on a.pedido equals b.refmov_id
                              where a.idencabezado == id
                              select b.refmov_numero).FirstOrDefault();


            BuscarFavoritos(menu);
            return View();
        }


        public JsonResult BuscarCostoReferencia(string codigo, int id_cliente)
        {
            var buscarReferencia = (from referencia in context.icb_referencia
                                    join precios in context.rprecios
                                        on referencia.ref_codigo equals precios.codigo into pre
                                    from precios in pre.DefaultIfEmpty()
                                    where referencia.ref_codigo == codigo
                                    select new
                                    {
                                        referencia.ref_codigo,
                                        referencia.ref_descripcion,
                                        referencia.por_iva,
                                        referencia.por_dscto,
                                        referencia.por_dscto_max,
                                        precio1 = precios != null ? precios.precio1 : 0,
                                        precio2 = precios != null ? precios.precio2 : 0,
                                        precio3 = precios != null ? precios.precio3 : 0,
                                        precio4 = precios != null ? precios.precio4 : 0,
                                        precio5 = precios != null ? precios.precio5 : 0,
                                        precio6 = precios != null ? precios.precio6 : 0,
                                        precio7 = precios != null ? precios.precio7 : 0,
                                        precio8 = precios != null ? precios.precio8 : 0,
                                        precio9 = precios != null ? precios.precio9 : 0
                                    }).FirstOrDefault();

            var buscarPrecioCliente = (from cliente in context.tercero_cliente
                                       where cliente.tercero_id == id_cliente
                                       select new
                                       {
                                           cliente.lprecios_repuestos,
                                           cliente.dscto_rep
                                       }).FirstOrDefault();

            decimal precio = 0;
            decimal iva = 0;
            decimal descuento = 0;
            if (buscarPrecioCliente != null && buscarReferencia != null)
            {
                iva = buscarReferencia.por_iva != null ? (decimal)buscarReferencia.por_iva : 0;
                decimal descuento_maximo = buscarReferencia.por_dscto_max != null
                    ? (decimal)buscarReferencia.por_dscto_max
                    : 0;

                if (buscarReferencia.por_dscto == null)
                {
                    if (buscarPrecioCliente.dscto_rep != null)
                    {
                        if (descuento_maximo > (decimal)buscarPrecioCliente.dscto_rep)
                        {
                            descuento = (decimal)buscarPrecioCliente.dscto_rep;
                        }
                        else
                        {
                            descuento = descuento_maximo;
                        }
                    }
                    else
                    {
                        descuento = 0;
                    }

                    //descuento = buscarPrecioCliente.dscto_rep != null ? (decimal)buscarPrecioCliente.dscto_rep : 0;
                    //descuento = descuento_maximo < descuento ? descuento_maximo : descuento;
                }
                else
                {
                    descuento = buscarReferencia.por_dscto != null ? (decimal)buscarReferencia.por_dscto : 0;
                    decimal descuentoCliente = buscarPrecioCliente.dscto_rep != null
                        ? (decimal)buscarPrecioCliente.dscto_rep
                        : 0;
                    descuento = descuento < descuentoCliente ? descuentoCliente : descuento;
                    if (descuento_maximo < descuento)
                    {
                        descuento = descuento_maximo;
                    }
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio1")
                {
                    precio = buscarReferencia.precio1;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio2")
                {
                    precio = buscarReferencia.precio2;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio3")
                {
                    precio = buscarReferencia.precio3;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio4")
                {
                    precio = buscarReferencia.precio4;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio5")
                {
                    precio = buscarReferencia.precio5;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio6")
                {
                    precio = buscarReferencia.precio6;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio7")
                {
                    precio = buscarReferencia.precio7;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio8")
                {
                    precio = buscarReferencia.precio8;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio9")
                {
                    precio = buscarReferencia.precio9;
                }

                return Json(new { precio, iva, descuento }, JsonRequestBehavior.AllowGet);
            }

            if (buscarReferencia != null)
            {
                descuento = buscarReferencia.por_dscto != null ? (decimal)buscarReferencia.por_dscto : 0;
                decimal descuento_maximoAux = buscarReferencia.por_dscto_max != null
                    ? (decimal)buscarReferencia.por_dscto_max
                    : 0;
                if (descuento_maximoAux < descuento)
                {
                    descuento = descuento_maximoAux;
                }
            }

            return Json(new { precio, iva, descuento }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarDescuento(string codigo, int cliente)
        {
            float data = (from r in context.icb_referencia
                          where r.ref_codigo == codigo
                          select
                              r.por_dscto_max
                ).FirstOrDefault();


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CompletarTablaRepuestos(int? id)
        {
            var buscar = (from a in context.lineas_documento
                          join b in context.icb_referencia
                              on a.codigo equals b.ref_codigo
                          where a.id_encabezado == id
                          select new
                          {
                              a.codigo,

                              a.cantidad,
                              b.ref_descripcion,
                              a.valor_unitario,
                              a.porcentaje_descuento,
                              a.porcentaje_iva
                          }).ToList();


            var data = new
            {
                buscar
                //codigo,
                //cantidad,
                //valorUnitario,/* = valorUnitario / (100 - Convert.ToDecimal(pordescuento)) * 100,*/
                //pordescuento,
                //poriva,
            };


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CompletarRetenciones(int? id)
        {
            var buscarRetenciones = (from a in context.encab_documento
                                     where a.idencabezado == id
                                     select new
                                     {
                                         a.retencion,
                                         a.retencion_iva,
                                         a.retencion_ica,
                                         a.retencion_causada
                                     }).ToList();

            var data = new
            {
                buscarRetenciones
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPedido(int id)
        {
            var data = (from r in context.icb_referencia_movdetalle
                        join mov in context.icb_referencia_mov
                            on r.refmov_id equals mov.refmov_id
                        where r.refmov_id == id
                        select new
                        {
                            codigo = r.ref_codigo,
                            cantidad = r.refdet_cantidad,
                            r.valor_unitario,
                            descuento = r.pordscto,
                            iva = r.poriva,
                            r.valor_total,
                            nit = mov.cliente
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarPedidoCliente(int nit, int bodega)
        {
            string a = context.icb_sysparameter.Where(x => x.syspar_cod == "P26").Select(x => x.syspar_value)
                .FirstOrDefault();
            int param = Convert.ToInt32(a);

            var data = (from r in context.icb_referencia_mov
                        where r.tpdocid == param && r.cliente == nit && r.bodega_id == bodega
                        select new
                        {
                            id = r.refmov_id,
                            descripcion = r.refmov_numero + " - " + r.refmov_fecela
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarCosto(string codigo)
        {
            var datax = (from r in context.vw_promedio
                         where r.codigo == codigo
                               && r.mes == DateTime.Now.Month
                               && r.ano == DateTime.Now.Year
                         select new
                         {
                             costo = r.Promedio
                         }).ToList();
            var data = datax.Select(d => new
            {
                costo = d.costo != null ? d.costo.Value.ToString("N0", new CultureInfo("is-IS")) : "0"
            }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarStock(string codigo, int bodega, string codigoPedido)
        {
            decimal stock = (from i in context.vw_inventario_hoy
                             where i.bodega == bodega && i.ref_codigo == codigo || i.bodega == bodega && i.ref_codigo == codigoPedido
                             select i.stock).FirstOrDefault();
            if (stock > 0)
            {
                return Json(new { puede = true, cantidad = stock }, JsonRequestBehavior.AllowGet);
            }

            {
                var data = (from i in context.vw_inventario_hoy
                            join r in context.icb_referencia
                                on i.ref_codigo equals r.ref_codigo
                            join b in context.bodega_concesionario
                                on i.bodega equals b.id
                            where i.ref_codigo == codigo && i.stock > 0 || i.ref_codigo == codigoPedido && i.stock > 0
                            select new
                            {
                                r.ref_codigo,
                                r.ref_descripcion,
                                b.bodccs_nombre,
                                i.stock
                            }).ToList();
                int cuantos = data.Count();
                if (cuantos > 0)
                {
                    return Json(new { puede = false, inven = true, info = data }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { puede = false, inven = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult BuscarRetenciones(int tipo_doc, int nit, int bodega, decimal subTotal, decimal totalDes,
            decimal totalIVA)
        {
            decimal retefuente = 0;
            decimal reteica = 0;
            decimal reteiva = 0;
            decimal autoRetencion = 0;

            // Validacion para reteIVA, reteICA y retencion dependiendo del proveedor seleccionado
            tp_doc_registros buscarTipoDocRegistro = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == tipo_doc);
            tercero_proveedor buscarProveedor = context.tercero_proveedor.FirstOrDefault(x => x.tercero_id == nit);
            int regimen_proveedor = buscarProveedor != null ? buscarProveedor.tpregimen_id : 0;
            perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                x.bodega == bodega && x.sw == buscarTipoDocRegistro.sw && x.tipo_regimenid == regimen_proveedor);

            if (buscarPerfilTributario != null)
            {
                if (buscarPerfilTributario.retfuente == "A" &&
                    subTotal - totalDes >= buscarTipoDocRegistro.baseretencion) //retencion
                {
                    retefuente = Math.Round((subTotal - totalDes) *
                                            Convert.ToDecimal(buscarTipoDocRegistro.retencion / 100, Cultureinfo));
                }

                if (buscarPerfilTributario.retiva == "A" && totalIVA >= buscarTipoDocRegistro.baseiva) //reteiva
                {
                    reteiva = Math.Round(totalIVA * Convert.ToDecimal(buscarTipoDocRegistro.retiva / 100, Cultureinfo));
                }

                if (buscarPerfilTributario.autorretencion == "A") //autoretencion
                {
                    decimal tercero_acteco = buscarProveedor.acteco_tercero.autorretencion;
                    autoRetencion = Math.Round((subTotal - totalDes) * Convert.ToDecimal(tercero_acteco / 100, Cultureinfo));
                }

                if (buscarPerfilTributario.retica == "A" &&
                    subTotal - totalDes >= buscarTipoDocRegistro.baseica) //reteica
                {
                    terceros_bod_ica bodega_acteco = context.terceros_bod_ica.FirstOrDefault(x =>
                        x.idcodica == buscarProveedor.acteco_id && x.bodega == bodega);
                    decimal tercero_acteco = buscarProveedor.acteco_tercero.tarifa;
                    if (bodega_acteco != null)
                    {
                        reteica = Math.Round((subTotal - totalDes) *
                                             Convert.ToDecimal(bodega_acteco.porcentaje / 1000, Cultureinfo));
                    }

                    if (tercero_acteco != 0)
                    {
                        reteica = Math.Round((subTotal - totalDes) *
                                             Convert.ToDecimal(buscarProveedor.acteco_tercero.tarifa / 1000, Cultureinfo));
                    }
                    else
                    {
                        reteica = Math.Round((subTotal - totalDes) *
                                             Convert.ToDecimal(buscarTipoDocRegistro.retica / 1000, Cultureinfo));
                    }
                }
            }

            var data = new
            {
                retefuente,
                reteica,
                reteiva,
                autoRetencion
            };

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
                            mensaje.Bcc.Add("correospruebaexi2019@gmail.com");
                            mensaje.ReplyToList.Add(new MailAddress(user_remitente.user_email,
                                user_remitente.user_nombre + " " + user_remitente.user_apellido));

                            mensaje.Subject = "Solicitud Autorización facturación referencia " + codigo;
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

        public JsonResult BuscarCondicionPorTercero(int id)
        {
            var data = (from t in context.fpago_tercero
                        join f in context.tercero_cliente
                            on t.fpago_id equals f.cod_pago_id
                        where f.tercero_id == id
                        select new
                        {
                            id = t.fpago_id,
                            nombre = t.fpago_nombre
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDescuentoPorTercero(int cliente, string codigo)
        {
            var tercero = (from t in context.tercero_cliente
                           where t.tercero_id == cliente
                           select new
                           {
                               descuento = t.dscto_rep
                           }).ToList();

            var repuesto = from t in context.icb_referencia
                           where t.ref_codigo == codigo
                           select new
                           {
                               descuento = t.max_desc
                           };

            var data = new
            {
                tercero,
                repuesto
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult clienteDesbloqueado(int cliente)
        {
            var data = (from t in context.tercero_cliente
                        where t.tercero_id == cliente
                        select new
                        {
                            t.bloqueado,
                            t.razoninactivo
                        }).FirstOrDefault();

            if (data.bloqueado == false)
            {
                return Json(new { bloqueado = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { bloqueado = true, mensaje = "Cliente bloqueado para facturar por " + data.razoninactivo },
                JsonRequestBehavior.AllowGet);
        }

        public JsonResult ValidarCupo(int cliente, int valorFactura)
        {
            var data = (from c in context.vw_doccartera
                            //where c.tipofacturacion == 4 && c.nit == cliente
                        where c.tipofacturacion == 37 && c.nit == cliente
                        select new
                        {
                            c.saldo
                        }).ToList();

            decimal? acumulado = data.Sum(x => x.saldo);
            decimal? total = acumulado + valorFactura;

            decimal? cupo = (from t in context.tercero_cliente
                             where t.tercero_id == cliente
                             select
                                 t.cupocredito
                ).FirstOrDefault();
            if (cupo == null || cupo == 0)
            {
                return Json(new { permitir = true }, JsonRequestBehavior.AllowGet);
            }

            decimal? cupoDisponible = cupo - acumulado;

            if (total <= cupo)
            {
                return Json(new { permitir = true }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { permitir = false, cupoDisponible }, JsonRequestBehavior.AllowGet);
        }

        //browser facturacion
        public JsonResult BuscarDatosVenta()
        {
            var data2 = (from e in context.encab_documento
                         join tp in context.tp_doc_registros
                             on e.tipo equals tp.tpdoc_id
                         join b in context.bodega_concesionario
                             on e.bodega equals b.id
                         join t in context.icb_terceros
                             on e.nit equals t.tercero_id
                         where tp.tipo == 37
                         select new
                         {
                             tipoDocumento = "(" + tp.prefijo + ") " + tp.tpdoc_nombre,
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
                             fecha_devolucion = e.fec_actualizacion
                         }).ToList();

            var data = data2.Select(c => new
            {
                c.tipoDocumento,
                c.numero,
                c.nit,
                fecha = c.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                valor_total = c.valor_total.ToString("N0"),
                c.id,
                c.bodega,
                c.estado,
                fecha_devolucion = c.fecha_devolucion != null
                    ? c.fecha_devolucion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : ""
            }).ToList();


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarConceptos(int? idTpDoc)
        {
            var buscarConceptos = context.tpdocconceptos.Where(x => x.tipodocid == idTpDoc).Select(x => new
            {
                x.id,
                x.Descripcion
            }).ToList();
            var buscarConceptos2 = context.tpdocconceptos2.Where(x => x.tipodocid == idTpDoc).Select(x => new
            {
                x.id,
                x.Descripcion
            }).ToList();

            return Json(new { buscarConceptos, buscarConceptos2 }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult crearPDFfacturacionotros(int? id)
        {
            var existeResolucion = (from e in context.encab_documento
                                    join d in context.resolucionfactura
                                        on e.tipo equals d.tipodoc
                                    where e.idencabezado == id
                                    select new
                                    {
                                        d.resolucion
                                    }).ToList();
            if (existeResolucion.Count > 0)
            {
                var resolucion = (from e in context.encab_documento
                                  join tp in context.tp_doc_registros
                                      on e.tipo equals tp.tpdoc_id
                                  join b in context.bodega_concesionario
                                      on e.bodega equals b.id
                                  join c in context.grupoconsecutivos
                                      on new { x1 = e.tipo, x2 = e.bodega } equals new { x1 = c.documento_id, x2 = c.bodega_id } into
                                      refa1
                                  from c in refa1.DefaultIfEmpty()
                                  join d in context.resolucionfactura
                                      on new { y1 = e.tipo, y2 = c.grupo } equals new { y1 = d.tipodoc, y2 = d.grupo } into refa2
                                  from d in refa2.DefaultIfEmpty()
                                  where e.idencabezado == id
                                  select new
                                  {
                                      tp.prefijo,
                                      d.resolucion,
                                      d.fechaini,
                                      d.fechafin,
                                      d.consecini,
                                      d.consecfin
                                  }).ToList();

                var formapagoter = (from a in context.encab_documento
                                    join b in context.fpago_tercero
                                        on a.fpago_id equals b.fpago_id
                                    where a.idencabezado == id
                                    select new
                                    {
                                        b.fpago_nombre
                                    }).ToList();


                var dircliente = (from en in context.encab_documento
                                  join te in context.terceros_direcciones
                                      on en.nit equals te.idtercero
                                  where en.idencabezado == id
                                  select new
                                  {
                                      te.direccion
                                  }).ToList();
                string dircli = dircliente.Max(c => c.direccion);

                var data2 = (from df in context.lineas_documento
                             join re in context.icb_referencia
                                 on df.codigo equals re.ref_codigo
                             where df.id_encabezado == id
                             select new
                             {
                                 id_Factura = df.id_encabezado,
                                 id_detalleFactura = df.id,
                                 referenciaFactura = df.codigo,
                                 descripcionFactura = re.ref_descripcion,
                                 cantFactura = df.cantidad,
                                 pordescuentoFactura = df.porcentaje_descuento,
                                 porivaFactura = df.porcentaje_iva,
                                 preciounitarioFactura = df.valor_unitario,
                                 valorFactura = df.valor_unitario * df.cantidad
                             }).ToList();

                List<detalleFacturacion> detalleFacturacion001 = data2.Select(c => new detalleFacturacion
                {
                    id_Factura = c.id_Factura,
                    id_detalleFactura = c.id_detalleFactura,
                    referenciaFactura = c.referenciaFactura,
                    descripcionFactura = c.descripcionFactura,
                    cantFactura = c.cantFactura.ToString("N0"),
                    pordescuentoFactura = c.pordescuentoFactura.Value.ToString("N0"),
                    porivaFactura = c.porivaFactura.Value.ToString("N0"),
                    preciounitarioFactura =
                        (c.preciounitarioFactura / ((100 - Convert.ToDecimal(c.pordescuentoFactura, Cultureinfo)) / 100))
                        .ToString("N0"),
                    valorFactura =
                        (c.cantFactura * (c.preciounitarioFactura /
                                          ((100 - Convert.ToDecimal(c.pordescuentoFactura, Cultureinfo)) / 100))).ToString("N0"),
                    montoivaFactura =
                        (c.cantFactura *
                         (c.preciounitarioFactura / ((100 - Convert.ToDecimal(c.pordescuentoFactura, Cultureinfo)) / 100)) *
                         (Convert.ToDecimal(c.porivaFactura, Cultureinfo) / 100)).ToString("N0")
                }).ToList();

                var cabeceraFactura = (from e in context.encab_documento
                                       join tp in context.tp_doc_registros
                                           on e.tipo equals tp.tpdoc_id
                                       join b in context.bodega_concesionario
                                           on e.bodega equals b.id
                                       join t in context.icb_terceros
                                           on e.nit equals t.tercero_id
                                       join emp in context.tablaempresa
                                           on b.concesionarioid equals emp.id
                                       where tp.tipo == 37 && e.idencabezado == id
                                       select new
                                       {
                                           // Empresa
                                           id_emp = emp.id,
                                           nomemp = emp.nombre_empresa,
                                           diremp = emp.direccion,
                                           nitemp = emp.nit,
                                           //Factura
                                           numfactura = e.numero,
                                           fechaFactura = e.fecha,
                                           fechavenceFactura = e.vencimiento,
                                           id_Cliente = t.tercero_id,
                                           docCliente = t.doc_tercero,
                                           nomCliente = t.prinom_tercero != null
                                               ? t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero + " " +
                                                 t.segapellido_tercero
                                               : t.razon_social,
                                           telCliente = t.telf_tercero,
                                           ciuCliente = emp.direccion,
                                           //vehiculo

                                           //Vendedor

                                           //TOTALES
                                           totalFactura = e.valor_total,
                                           valorfletes = e.fletes,
                                           valoriva = e.iva
                                       }).FirstOrDefault();

                string root = Server.MapPath("~/Pdf/");
                string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
                string path = Path.Combine(root, pdfname);
                path = Path.GetFullPath(path);
                CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");


                // var num


                FacturacionPDFModel obj = new FacturacionPDFModel
                {
                    //Empresa
                    id_Empresa = cabeceraFactura.id_emp,
                    nomEmpresa = cabeceraFactura.nomemp,
                    dirEmpresa = cabeceraFactura.diremp,
                    nitEmpresa = cabeceraFactura.nitemp,
                    //Cliente
                    id_Cliente = cabeceraFactura.id_Cliente,
                    docCliente = ": " + cabeceraFactura.docCliente,
                    nomCliente = ": " + cabeceraFactura.nomCliente,
                    dirCliente = ": " + dircli,
                    telCliente = ": " + cabeceraFactura.telCliente,
                    ciuCliente = ": " + cabeceraFactura.diremp,
                    //Factura
                    numFactura = Convert.ToString(cabeceraFactura.numfactura),
                    fechaFactura = cabeceraFactura.fechaFactura.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                    formapagoFactura = formapagoter[0].fpago_nombre,
                    fechavenceFactura =
                        cabeceraFactura.fechavenceFactura.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                    //TOTALES
                    valorbruto = Convert.ToDecimal(detalleFacturacion001.Sum(vb =>
                            Convert.ToDecimal(vb.cantFactura, Cultureinfo) * Convert.ToDecimal(vb.preciounitarioFactura, Cultureinfo)), Cultureinfo)
                        .ToString("N0"),
                    valordescuento = Convert.ToDecimal(detalleFacturacion001.Sum(vb =>
                        Convert.ToDecimal(vb.pordescuentoFactura, Cultureinfo) / 100 * Convert.ToDecimal(vb.cantFactura, Cultureinfo) *
                        Convert.ToDecimal(vb.preciounitarioFactura, Cultureinfo)), Cultureinfo).ToString("N0"),
                    valorfletes = cabeceraFactura.valorfletes.ToString("N0"),
                    baseiva =
                        (Convert.ToDecimal(detalleFacturacion001.Sum(vb =>
                             Convert.ToDecimal(vb.cantFactura, Cultureinfo) * Convert.ToDecimal(vb.preciounitarioFactura, Cultureinfo)), Cultureinfo) -
                         Convert.ToDecimal(detalleFacturacion001.Sum(vb =>
                             Convert.ToDecimal(vb.pordescuentoFactura, Cultureinfo) / 100 * Convert.ToDecimal(vb.cantFactura, Cultureinfo) *
                             Convert.ToDecimal(vb.preciounitarioFactura, Cultureinfo)), Cultureinfo)).ToString("N0"),
                    valoriva = cabeceraFactura.valoriva.ToString("N0"),
                    //subtotal = 
                    //valoriva = cabeceraFactura
                    totalFactura = cabeceraFactura.totalFactura.ToString("N0"),
                    detalleFacturacion = detalleFacturacion001,
                    //Resolucion
                    prefi = resolucion[0].prefijo,
                    referencia = resolucion[0].resolucion,
                    fechai = resolucion[0].fechaini.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                    facti = resolucion[0].consecini.ToString(),
                    factf = resolucion[0].consecfin.ToString()
                };

                ViewAsPdf something = new ViewAsPdf("crearPDFfacturacionotros", obj);
                return something;
            }
            else
            {
                ViewAsPdf something = new ViewAsPdf("", "");
                return something;
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