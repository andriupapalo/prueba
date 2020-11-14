using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Rotativa.Options;
using Newtonsoft.Json;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq.Expressions;



namespace Homer_MVC.Controllers
{
    public class ReciboCajaController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        private readonly CultureInfo inter = CultureInfo.CreateSpecificCulture("is-IS");

        public void listas(int? idpedido)
        {
            int idclientepedido = 0;
            int bodegapedido = 0;
            int idvendedor = 0;

            //verifico si el pedido no es nulo si corresponde a un pedido
            vpedido existepedido = context.vpedido.Where(d => d.id == idpedido).FirstOrDefault();

            if (existepedido != null)
            {
                idclientepedido = existepedido.nit != null ? existepedido.nit.Value : 0;
                bodegapedido = existepedido.bodega;
                idvendedor = existepedido.vendedor != null ? existepedido.vendedor.Value : 0;
                var pedido = (from p in context.vpedido
                              join m in context.modelo_vehiculo
                                  on p.modelo equals m.modvh_codigo
                              where p.nit == existepedido.nit
                              select new
                              {
                                  p.id,
                                  p.numero,
                                  carro = m.modvh_nombre
                              }).ToList();
                var pedidos = pedido.Select(d => new
                {
                    d.id,
                    numero = "(" + d.numero + ") - " + d.carro
                }).ToList();
                ViewBag.id_pedido_vehiculo = new SelectList(pedidos, "id", "numero", existepedido.id);
            }
            else
            {
                ViewBag.id_pedido_vehiculo = new SelectList(context.vpedido.Take(0).ToList(), "id", "numero");
            }

            //ViewBag.tipo = context.tp_doc_registros.Where(x => x.tipo == 16);
            ViewBag.tipo = new SelectList(context.tp_doc_registros.Where(x => x.tipo == 16), "tpdoc_id",
                "tpdoc_nombre");

            var list = (from t in context.icb_terceros
                        join tp in context.tercero_cliente
                            on t.tercero_id equals tp.tercero_id
                        select new
                        {
                            t.tercero_id,
                            t.prinom_tercero,
                            t.apellido_tercero,
                            t.segnom_tercero,
                            t.segapellido_tercero,
                            t.doc_tercero,
                            t.razon_social
                        }).ToList();

            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in list)
            {
                lista.Add(new SelectListItem
                {
                    Text = item.doc_tercero + " " + item.prinom_tercero + ' ' + item.segnom_tercero + ' ' +
                           item.apellido_tercero + ' ' + item.segnom_tercero + ' ' + item.razon_social,
                    Value = item.tercero_id.ToString()
                });
            }

            ViewBag.nit = new SelectList(lista, "Value", "Text", idclientepedido);
            // ViewBag.id_pedido_vehiculo = "";
            ViewBag.fpago = new SelectList(context.tipopagorecibido, "id", "pago");

            var buscarTipoDocumento = (from tipoDocumento in context.tp_doc_registros
                                       select new
                                       {
                                           tipoDocumento.sw,
                                           tipoDocumento.tpdoc_id,
                                           nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                           tipoDocumento.tipo
                                       }).ToList();
            icb_sysparameter swND = context.icb_sysparameter.Where(d => d.syspar_cod == "P128").FirstOrDefault();
            int swND2 = swND != null ? Convert.ToInt32(swND.syspar_value) : 1013;
            icb_sysparameter swF = context.icb_sysparameter.Where(d => d.syspar_cod == "P129").FirstOrDefault();
            int swF2 = swF != null ? Convert.ToInt32(swF.syspar_value) : 3;
            ViewBag.tipo_documentoFiltro = new SelectList(buscarTipoDocumento.Where(x => x.sw == swF2 || x.sw == swND2),
                "tpdoc_id", "nombre",
                buscarTipoDocumento.Where(x => x.sw == swF2 || x.sw == swND2).Select(d => d.tpdoc_id).FirstOrDefault());
            //ViewBag.tipo = new SelectList(context.tp_doc_registros.Where(x => x.tipo == 20), "tpdoc_id", "tpdoc_nombre");

            /*var users = from u in context.users
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
			    var nombre = "(" + item.user_numIdent + ") - " + item.nombre + " " + item.apellidos;
			    itemsU.Add(new SelectListItem() { Text = nombre, Value = item.idTercero.ToString() });
			}*/
            //ViewBag.vendedor = new SelectList(itemsU, "Value", "Text", idvendedor);

            ViewBag.moneda = new SelectList(context.monedas, "moneda", "descripcion");
            ViewBag.tasa = new SelectList(context.moneda_conversion, "id", "conversion");
            ViewBag.motivocompra = new SelectList(context.motcompra, "id", "Motivo");

            ViewBag.condicion_pago = context.fpago_tercero;

            //var provedores = (from pro in context.tercero_cliente
            //                  join ter in context.icb_terceros
            //                  on pro.tercero_id equals ter.tercero_id
            //                  select new
            //                  {
            //                      idTercero = ter.tercero_id,
            //                      nombreTErcero = ter.prinom_tercero,
            //                      apellidosTercero = ter.apellido_tercero,
            //                      razonSocial = ter.razon_social,
            //                      ter.doc_tercero
            //                  }).ToList();
            //List<SelectListItem> items = new List<SelectListItem>();
            //foreach (var item in provedores)
            //{

            //    var nombre = item.doc_tercero + " - " + item.nombreTErcero + " " + item.apellidosTercero + " " + item.razonSocial;
            //    items.Add(new SelectListItem() { Text = nombre, Value = item.idTercero.ToString() });
            //}
            //ViewBag.nit = new SelectList(items, "Value", "Text"); 

            List<users> asesores = context.users.Where(x => x.rol_id == 4 || x.rol_id == 6).ToList();
            List<SelectListItem> listaAsesores = new List<SelectListItem>();
            foreach (users asesor in asesores)
            {
                listaAsesores.Add(new SelectListItem
                { Value = asesor.user_id.ToString(), Text = asesor.user_nombre + " " + asesor.user_apellido });
            }

            ViewBag.vendedor = new SelectList(listaAsesores, "Value", "Text", idvendedor);

            //ViewBag.vendedor = new SelectList(listaAsesores, "Value", "Text"); 

            ViewBag.parametroEfectivo =
                context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P71").syspar_value;
            ViewBag.parametroCredito = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P72").syspar_value;

            encab_documento buscarUltimoRecibo = context.encab_documento.OrderByDescending(x => x.idencabezado).FirstOrDefault();
            ViewBag.numReciboCreado = buscarUltimoRecibo != null ? buscarUltimoRecibo.numero : 0;
        }

        // GET: ReciboCaja
        public ActionResult Create(int? menu, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                listas(null);
                BuscarFavoritos(menu);
                return View();
            }

            BuscarFavoritos(menu);
            string planmayor2 = id != null ? id : "";
            var reciboCaja = (from vped in context.vpedido
                              join ter in context.icb_terceros on
                                  vped.nit equals ter.tercero_id into temp
                              from ter in temp.DefaultIfEmpty()
                              join bod in context.bodega_concesionario on
                                  vped.bodega equals bod.id
                              join vend in context.users on
                                  vped.vendedor equals vend.user_id into temp2
                              from vend in temp2.DefaultIfEmpty()
                              where vped.planmayor == planmayor2
                              select new
                              {
                                  vped.id,
                                  nit = ter.tercero_id,
                                  nombreCliente = ter.prinom_tercero != null ? ter.prinom_tercero : "",
                                  apellidoCliente = ter.apellido_tercero != null ? ter.apellido_tercero : "",
                                  nombreBodega = bod.bodccs_nombre != null ? bod.bodccs_nombre : "",
                                  nombreVendedor = vend.user_nombre != null ? vend.user_nombre : "",
                                  apellidoVendedor = vend.user_apellido != null ? vend.user_apellido : "",
                                  idVendedor = vend.user_id,
                                  bodega_nombre = bod.bodccs_nombre,
                                  bodega_id = bod.id
                              }).FirstOrDefault();
            int idpedidoint = reciboCaja.id;
            listas(idpedidoint);
            ViewBag.bodega = reciboCaja.bodega_nombre;
            ViewBag.bodega_id = reciboCaja.bodega_id;
            ViewBag.planmayor = planmayor2;
            ViewBag.nit2 = reciboCaja.nit;
            ViewBag.nombreCliente =
                reciboCaja != null ? reciboCaja.nombreCliente + " " + reciboCaja.apellidoCliente : "";
            ViewBag.vendedorDesembolso =
                reciboCaja != null ? reciboCaja.nombreVendedor + " " + reciboCaja.apellidoVendedor : "";
            ViewBag.idVendedor = reciboCaja.idVendedor;

            return View();
        }

        [HttpPost]
        public ActionResult Create(NotasContablesModel rc)
        {
            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                {
                    //tipo de factura factura vehiculo
                    icb_sysparameter fac = context.icb_sysparameter.Where(d => d.syspar_cod == "P101").FirstOrDefault();
                    int ventas = fac != null ? Convert.ToInt32(fac.syspar_value) : 4;

                    //consecutivo
                    decimal pagoFacturas = 0;
                    int lista_pagos = Convert.ToInt32(Request["lista_pagos"]);
                    if (Request["id_pedido_vehiculo"] == "")
                    {
                        int lista_facturas = Convert.ToInt32(Request["listaFacturas"]);

                        for (int i = 0; i <= lista_facturas; i++)
                        {
                            decimal valor = Convert.ToDecimal(Request["valor_aPagar" + i],inter);
                            if (valor != 0)
                            {
                                pagoFacturas += valor;
                            }
                        }
                    }
                    grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == rc.tipo && x.bodega_id == rc.bodega);
                    if (grupo != null)
                    {
                        try
                        {
                            DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                            long consecutivo = doc.BuscarConsecutivo(grupo.grupo);
                            string total = Request["totalPagar"];
                            //Encabezado documento
                            encab_documento encabezado = new encab_documento
                            {
                                tipo = rc.tipo,
                                numero = consecutivo,
                                nit = rc.nit,
                                fecha = DateTime.Now,
                                valor_total = Convert.ToDecimal(total, inter),
                                vendedor = rc.vendedor,
                                documento = rc.documento,
                                prefijo = rc.prefijo,
                                valor_mercancia = Convert.ToDecimal(rc.costo, inter),
                                nota1 = rc.nota1,
                                valor_aplicado = 0,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                fec_creacion = DateTime.Now,
                                perfilcontable = rc.perfilcontable,
                                bodega = rc.bodega,
                                anticipo = rc.anticipo
                            };

                            //encabezado.valor_total = Convert.ToDecimal(total);
                            //encabezado.vendedor = Convert.ToInt32(Request["vendedor"]);
                            ////encabezado.iva = Convert.ToDecimal(rc.iva);
                            ////encabezado.retencion = Convert.ToDecimal(rc.retencion);
                            ////encabezado.retencion_ica = Convert.ToDecimal(rc.retencion_ica);
                            ////encabezado.retencion_iva = Convert.ToDecimal(rc.retencion_iva);
                            ////encabezado.vendedor = rc.vendedor;
                            //encabezado.documento = Convert.ToString(0);
                            ////encabezado.prefijo = rc.prefijo;
                            ////encabezado.valor_mercancia = Convert.ToDecimal(rc.costo);
                            //encabezado.nota1 = rc.nota1;
                            //encabezado.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                            //encabezado.fec_creacion = DateTime.Now;
                            //encabezado.valor_aplicado = pagoFacturas;
                            ////encabezado.porcen_retencion = float.Parse(rc.por_retencion, System.Globalization.CultureInfo.InvariantCulture);//esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal
                            ////encabezado.porcen_reteiva = float.Parse(rc.por_retencion_iva, System.Globalization.CultureInfo.InvariantCulture);//esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal
                            ////encabezado.porcen_retica = float.Parse(rc.por_retencion_ica, System.Globalization.CultureInfo.InvariantCulture);//esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal
                            //encabezado.perfilcontable = rc.perfilcontable;
                            //encabezado.bodega = rc.bodega;
                            //encabezado.anticipo = rc.anticipo;

                            context.encab_documento.Add(encabezado);
                            context.SaveChanges();
                            int cruce = encabezado.idencabezado;
                            long numcruce = encabezado.numero;
                            //guardo los pagos del documento
                            for (int i = 1; i <= lista_pagos; i++)
                            {
                                int id = Convert.ToInt32(Request["id" + i]);
                                if (!string.IsNullOrEmpty(Request["fpago" + i]) && !string.IsNullOrEmpty(Request["valor" + i]))
                                {
                                    documentos_pago dpago = new documentos_pago
                                    {
                                        idtencabezado = cruce,
                                        tercero = encabezado.nit,
                                        fecha = DateTime.Now,
                                        forma_pago = Convert.ToInt32(Request["fpago" + i]),
                                        valor = Convert.ToDecimal(Request["valor" + i], inter),
                                        cuenta_banco = Convert.ToString(Request["cuenta" + i]),
                                        documento = Convert.ToString(Request["cheque" + i]),
                                        notas = Request["observaciones" + i]
                                    };
                                    string banco = Request["banco" + i];
                                    if (!string.IsNullOrEmpty(Request["banco" + i]))
                                    {
                                        dpago.banco = Convert.ToInt32(Request["banco" + i]);
                                    }
                                    if (!string.IsNullOrEmpty(Request["id_pedido_vehiculo"]))
                                    {
                                        dpago.pedido = Convert.ToInt32(Request["id_pedido_vehiculo"]);
                                    }
                                    context.documentos_pago.Add(dpago);
                                }
                            }
                            context.SaveChanges();


                            //int cruceDocumento = context.encab_documento.OrderByDescending(d => d.idencabezado).Select(d => d.idencabezado).FirstOrDefault();
                            string pedido = Request["id_pedido_vehiculo"];
                            Convert.ToString(pedido);
                            decimal valorapli = 0;

                            /************************************************************ Guardo informacion si selecciona factura ************************************************************/
                            if (pedido != "" && pedido != null)
                            {
                                //valor total factura
                                decimal valortotal = encabezado.valor_total;
                                string idcredito = Request["creditoId"];
                                string desembolso = Request["desembolsosn"];
                                if (idcredito != "" && idcredito != null && desembolso != "" && desembolso != "false")
                                {
                                    int creditoid = Convert.ToInt32(Request["creditoId"]);
                                    v_creditos credito = context.v_creditos.Where(x => x.infocredito_id == creditoid).FirstOrDefault();
                                    if (credito != null)
                                    {
                                        credito.pedido = Convert.ToInt32(pedido);
                                        credito.fec_desembolso = DateTime.Now;
                                        credito.estadoc = "D";
                                        context.Entry(credito).State = EntityState.Modified;
                                        context.SaveChanges();
                                    }
                                }

                                //BUSCO LA FACTURA DE ESE PEDIDO
                                int lista_facturas = Convert.ToInt32(Request["listaFacturas"]);
                                for (int i = 0; i <= lista_facturas; i++)
                                {
                                    int tipo = Convert.ToInt32(Request["tipo" + i]);
                                    int numero = Convert.ToInt32(Request["numero" + i]);
                                    int id = Convert.ToInt32(Request["id" + i]);
                                    decimal valor = Convert.ToDecimal(Request["valor_aPagar" + i], inter);

                                    encab_documento factura = context.encab_documento.Where(d => d.idencabezado == id && d.tipo == tipo)
                                        .FirstOrDefault();

                                    if (valor != 0)
                                    {


                                        decimal valoraplicado =factura.valor_aplicado;

                                        decimal nuevovalor = valoraplicado + valor;
                                        valorapli = valorapli + valor;
                                        factura.valor_aplicado = Convert.ToDecimal(nuevovalor, inter);
                                        context.Entry(factura).State = EntityState.Modified;

                                        // si al crear la nota credito se selecciono factura se debe guardar el registro tambien en cruce documentos, de lo contrario NO
                                        //Cruce documentos
                                        cruce_documentos cd = new cruce_documentos
                                        {
                                            idtipo = rc.tipo,
                                            numero = numcruce,
                                            id_encab_aplica = id,
                                            id_encabezado = cruce,
                                            //tipo de la factura cruzada
                                            idtipoaplica = Convert.ToInt32(tipo),
                                            //numero de la factura cruzada
                                            numeroaplica = Convert.ToInt32(numero),
                                            //valor aplicado a cada factura
                                            valor = valor,
                                            fecha = DateTime.Now,
                                            fechacruce = DateTime.Now,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                                        };
                                        context.cruce_documentos.Add(cd);
                                    }
                                }
                                int guardar = context.SaveChanges();
                            }
                            else
                            {
                                if (pedido == "")
                                {
                                    //valor aplicado tengo que actualizarlo en la(s) factura(s) a la(s) que estoy generando el recibo de caja
                                    //buscas la factura
                                    int lista_facturas = Convert.ToInt32(Request["listaFacturas"]);
                                    for (int i = 0; i <= lista_facturas; i++)
                                    {
                                        int tipo = Convert.ToInt32(Request["tipo" + i]);
                                        int numero = Convert.ToInt32(Request["numero" + i]);
                                        int id = Convert.ToInt32(Request["id" + i]);
                                        decimal valor = Convert.ToDecimal(Request["valor_aPagar" + i], inter);

                                        encab_documento factura = context.encab_documento.Where(d => d.idencabezado == id && d.tipo == tipo)
                                            .FirstOrDefault();

                                        if (valor != 0)
                                        {
                                            decimal valoraplicado = factura.valor_aplicado;

                                            decimal nuevovalor = valoraplicado + valor;
                                            valorapli = valorapli + valor;

                                            factura.valor_aplicado = nuevovalor;
                                            context.Entry(factura).State = EntityState.Modified;

                                            // si al crear la nota credito se selecciono factura se debe guardar el registro tambien en cruce documentos, de lo contrario NO
                                            //Cruce documentos
                                            cruce_documentos cd = new cruce_documentos
                                            {
                                                idtipo = rc.tipo,
                                                numero = numcruce,
                                                id_encab_aplica = id,
                                                id_encabezado = cruce,
                                                //tipo de la factura cruzada
                                                idtipoaplica = Convert.ToInt32(tipo),
                                                //numero de la factura cruzada
                                                numeroaplica = Convert.ToInt32(numero),
                                                //valor aplicado a cada factura
                                                valor = valor,
                                                fecha = DateTime.Now,
                                                fechacruce = DateTime.Now,
                                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                                            };
                                            context.cruce_documentos.Add(cd);
                                        }
                                    }
                                    int guardar = context.SaveChanges();
                                }
                                else
                                {

                                }
                            }
                            encabezado.valor_aplicado = valorapli;
                            context.SaveChanges();

                            //movimiento contable
                            //buscamos en perfil cuenta documento, por medio del perfil seleccionado
                            var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                                              join nombreParametro in context.paramcontablenombres
                                                                  on perfil.id_nombre_parametro equals nombreParametro.id
                                                              join cuenta in context.cuenta_puc
                                                                  on perfil.cuenta equals cuenta.cntpuc_id
                                                              where perfil.id_perfil == rc.perfilcontable
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
                                    if (parametro.id_nombre_parametro == 2 && Convert.ToDecimal(rc.iva, inter) != 0
                                        || parametro.id_nombre_parametro == 3 && Convert.ToDecimal(rc.retencion, inter) != 0
                                        || parametro.id_nombre_parametro == 4 && Convert.ToDecimal(rc.por_retencion_iva, inter) != 0
                                        || parametro.id_nombre_parametro == 5 && Convert.ToDecimal(rc.retencion_ica, inter) != 0
                                        || parametro.id_nombre_parametro == 10
                                        || parametro.id_nombre_parametro == 11
                                        || parametro.id_nombre_parametro == 16)
                                    {
                                        mov_contable movNuevo = new mov_contable
                                        {
                                            id_encab = cruce,
                                            seq = secuencia,
                                            idparametronombre = parametro.id_nombre_parametro,
                                            cuenta = parametro.cuenta,
                                            centro = parametro.centro,
                                            fec = DateTime.Now,
                                            fec_creacion = DateTime.Now,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            detalle = rc.nota1
                                        };

                                        cuenta_puc info = context.cuenta_puc.Where(i => i.cntpuc_id == parametro.cuenta)
                                            .FirstOrDefault();

                                        if (info.tercero)
                                        {
                                            movNuevo.nit = rc.nit;
                                        }
                                        else
                                        {
                                            /*icb_terceros tercero = context.icb_terceros.Where(t => t.doc_tercero == "0")
                                                .FirstOrDefault();*/
                                            movNuevo.nit = rc.nit;
                                        }

                                        // las siguientes validaciones se hacen dependiendo de la cuenta que esta moviendo la nota credito, para guardar la informacion acorde
                                        if (parametro.id_nombre_parametro == 10)
                                        {
                                            /*if (info.aplicaniff==true)
                                            {

                                            }*/

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(total, inter);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = rc.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(total, inter);

                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(total, inter);
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(total, inter);
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(total, inter);
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 16)
                                        {
                                            /*if (info.aplicaniff==true)
                                            {

                                            }*/

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(total, inter);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = rc.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(total, inter);
                                                movNuevo.debito = 0;

                                                movNuevo.creditoniif = Convert.ToDecimal(total, inter);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(total, inter);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(total, inter);
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
                                                movNuevo.basecontable = Convert.ToDecimal(total, inter);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = rc.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(total, inter);

                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(total, inter);
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(total, inter);
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(total, inter);
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 2)
                                        {
                                            /*if (info.aplicaniff == true)
                                            {

                                            }*/

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(total, inter);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = rc.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(rc.iva, inter);
                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(rc.iva, inter);
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(rc.iva, inter);
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(rc.iva, inter);
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 3)
                                        {
                                            /*if (info.aplicaniff == true)
                                            {

                                            }*/

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(total, inter);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = rc.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(rc.retencion, inter);
                                                movNuevo.debito = 0;

                                                movNuevo.creditoniif = Convert.ToDecimal(rc.retencion, inter);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(rc.retencion, inter);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(rc.retencion, inter);
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
                                                movNuevo.basecontable = Convert.ToDecimal(rc.iva, inter);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = rc.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(rc.retencion_iva, inter);
                                                movNuevo.debito = 0;

                                                movNuevo.creditoniif = Convert.ToDecimal(rc.retencion_iva, inter);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(rc.retencion_iva, inter);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(rc.retencion_iva, inter);
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
                                                movNuevo.basecontable = Convert.ToDecimal(total, inter);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = rc.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(rc.retencion_ica, inter);
                                                movNuevo.debito = 0;
                                                movNuevo.creditoniif = Convert.ToDecimal(rc.retencion_ica, inter);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(rc.retencion_ica, inter);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(rc.retencion_ica, inter);
                                                movNuevo.debito = 0;
                                            }
                                        }

                                        secuencia++;

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
                                            context.SaveChanges();
                                        }
                                        //Cuentas valores


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
                                if (!string.IsNullOrWhiteSpace(pedido))
                                {
                                    int idpedidoint = Convert.ToInt32(pedido);
                                    listas(idpedidoint);
                                }
                                else
                                {
                                    listas(null);
                                }

                                return View(rc);
                            }

                            try
                            {
                                context.SaveChanges();
                                TempData["mensaje"] = "registro creado correctamente";
                                DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
                                doc.ActualizarConsecutivo(grupo.grupo, consecutivo);
                                dbTran.Commit();
                                return RedirectToAction("Create");
                            }
                            catch (DbEntityValidationException dbEx)
                            {
                                Exception raise = dbEx;
                                foreach (DbEntityValidationResult validationErrors in dbEx.EntityValidationErrors)
                                {
                                    foreach (DbValidationError validationError in validationErrors.ValidationErrors)
                                    {
                                        string message = string.Format("{0}:{1}",
                                        validationErrors.Entry.Entity,
                                        validationError.ErrorMessage);
                                        // raise a new exception nesting
                                        // the current instance as InnerException
                                        raise = new InvalidOperationException(message, raise);
                                    }
                                }

                                throw raise;
                            }
                        }
                        catch (Exception ex)
                        {
                            Exception mensaje = ex.InnerException;
                            TempData["mensaje_error"] = mensaje+"error en Guardado. Por favor comuniquese con un Administrador";
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

            if (Request["id_pedido_vehiculo"] != null)
            {
                int idpedidoint = Convert.ToInt32(Request["id_pedido_vehiculo"]);
                listas(idpedidoint);
            }
            else
            {
                listas(null);
            }

            //listas();
            return View(rc);
        }

        public ActionResult Browser(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult traerDatosBusqueda(int? recibo, string cedula, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            System.Linq.Expressions.Expression<Func<vw_recibo_caja, bool>> predicate = PredicateBuilder.True<vw_recibo_caja>();
            System.Linq.Expressions.Expression<Func<vw_recibo_caja, bool>> recibo1 = PredicateBuilder.False<vw_recibo_caja>();
            System.Linq.Expressions.Expression<Func<vw_recibo_caja, bool>> cedula1 = PredicateBuilder.False<vw_recibo_caja>();
            System.Linq.Expressions.Expression<Func<vw_recibo_caja, bool>> fechaDesde1 = PredicateBuilder.False<vw_recibo_caja>();
            System.Linq.Expressions.Expression<Func<vw_recibo_caja, bool>> fechaHasta1 = PredicateBuilder.False<vw_recibo_caja>();

            if (cedula != "")
            {
                cedula1 = cedula1.Or(x => x.doc_tercero == cedula || x.nit.Contains(cedula));
                predicate = predicate.And(cedula1);
            }

            if (recibo != null)
            {
                recibo1 = recibo1.Or(x => x.numero == recibo || x.id == recibo);
                predicate = predicate.And(recibo1);
            }

            if (fechaDesde != null && fechaHasta != null)
            {
                fechaHasta = fechaHasta.Value.AddDays(1);
                predicate = predicate.And(x => x.fecha >= fechaDesde && x.fecha <= fechaHasta);
            }
            else
            {
                fechaHasta = DateTime.Now.Date.AddDays(1);
                fechaDesde = DateTime.Now.Date.AddYears(-1);
                predicate = predicate.And(x => x.fecha >= fechaDesde && x.fecha <= fechaHasta);
            }

            //agregar nombre del cliente
            //if (vehiculosFacturados == true)
            //{
            //    vh_Facturados = vh_Facturados.Or(x => x.id_forma_pago == 1);
            //    predicate = predicate.And(vh_Facturados);
            //}
            List<vw_recibo_caja> buscar = context.vw_recibo_caja.Where(predicate).ToList();

            return Json(buscar, JsonRequestBehavior.AllowGet);
        }

        public JsonResult verDetalle(int id)
        {
            var detalle = (from e in context.encab_documento
                           join l in context.lineas_documento
                               on e.idencabezado equals l.id_encabezado
                           join r in context.icb_referencia
                               on l.codigo equals r.ref_codigo
                           where e.idencabezado == id
                           select new
                           {
                               id = e.idencabezado,
                               referencia = "(" + l.codigo + ") " + r.ref_descripcion,
                               l.valor_unitario,
                               l.cantidad,
                               porcentaje_descuento = l.porcentaje_descuento != null ? l.porcentaje_descuento.Value : 0,
                               l.porcentaje_iva
                           }).ToList();

            return Json(detalle, JsonRequestBehavior.AllowGet);
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

            int? p = context.documentos_pago.Where(x => x.tercero == encab.nit && x.idtencabezado == id)
                .Select(x => x.pedido).FirstOrDefault();
            if (p != null)
            {
                ViewBag.pedido = p;
            }

            desP desP = (from vp in context.vpedido
                         join m in context.modelo_vehiculo
                             on vp.modelo equals m.modvh_codigo
                         where vp.nit == encab.nit
                         select new desP
                         {
                             id = vp.id,
                             numero = vp.numero ?? 0,
                             carro = m.modvh_nombre
                         }).FirstOrDefault();
            ViewBag.desP = desP;

            BuscarFavoritos(menu);
            return View(encab);
        }

        public ActionResult ReciboCaja(int id)
        {
            var generarRecibo = (from nom in context.encab_documento
                                 join cruce in context.cruce_documentos
                                  on nom.idencabezado equals cruce.id_encabezado into cr
                                   from cruc in cr.DefaultIfEmpty()
                                 join reg in context.tp_doc_registros
                                     on nom.tipo equals reg.tpdoc_id
                                 join ter in context.icb_terceros
                                     on nom.nit equals ter.tercero_id
                                 join docPago in context.documentos_pago
                                     on nom.idencabezado equals docPago.idtencabezado into temp
                                 from docPago in temp.DefaultIfEmpty()
                                 join a in context.documentos_pago
                                     on ter.tercero_id equals a.tercero into tmp
                                 from a in tmp.DefaultIfEmpty()
                                 join tpr in context.tipopagorecibido
                                     on docPago.forma_pago equals tpr.id
                                 join bank in context.bancos
                                     on docPago.banco equals bank.id into tempo
                                 from bank in temp.DefaultIfEmpty()
                                 join dir in context.terceros_direcciones
                                     on ter.tercero_id equals dir.idtercero into tmpDir
                                 from dir in tmpDir.DefaultIfEmpty()
                                     /***** adicion mateo ****/
                                 join ur in context.users // usuario responsable
                                     on nom.userid_creacion equals ur.user_id
                                 join uv in context.users // usuario vendedor
                                     on nom.vendedor equals uv.user_id into tmpUV
                                 from uv in tmpUV.DefaultIfEmpty()
                                 join b in context.bodega_concesionario
                                     on nom.bodega equals b.id
                                 /***** fin adicion mateo ****/
                                 where nom.idencabezado == id
                                 select new
                                 {
                                     numeroRegistro = nom.numero,
                                     tipoEntrada = reg.tpdoc_nombre,
                                     ValorEncabezadoTotal = nom.valor_total,
                                     reg.prefijo,
                                     nom.fecha,
                                     docPago.notas,
                                     nombreTercero = ter.prinom_tercero + " " + ter.segnom_tercero + " " + ter.apellido_tercero + " " +
                                                     ter.segapellido_tercero,
                                     idTercero = ter.doc_tercero,
                                     telefono = ter.telf_tercero,
                                     dir.direccion,
                                     fechaT = docPago.fecha,
                                     /***** adicion mateo ****/
                                     responsable = ur.user_nombre + " " + ur.user_apellido,
                                     vendedor = uv.user_nombre + " " + uv.user_apellido,
                                     bodega = b.bodccs_nombre,
                                     /***** fin adicion mateo ****/
                                     docCruzado = context.encab_documento.Where(d=> d.idencabezado== cruc.id_encab_aplica).Select(d => new { d.tp_doc_registros.prefijo, d.numero }).FirstOrDefault(),
                                 }).FirstOrDefault();

            string root = Server.MapPath("~/Pdf/");
            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
            string path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);
            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");


            PDFmodel reciboCaja = new PDFmodel
            {
                numeroRegistro = generarRecibo.numeroRegistro,
                prefijo = generarRecibo.prefijo,
                fechaEncabezado = Convert.ToString(generarRecibo.fecha),
                ValorEncabezadoTotal = generarRecibo.ValorEncabezadoTotal.ToString("0,0", elGR),
                tipoEntrada = generarRecibo.tipoEntrada,
                nombreTercero = generarRecibo.nombreTercero.ToUpper(),
                Idtercero = generarRecibo.idTercero,
                telefono = generarRecibo.telefono,
                direccion = generarRecibo.direccion != null ? generarRecibo.direccion.ToUpper() : "",
                fechaTabla = Convert.ToString(generarRecibo.fechaT),
                prefijoCruzado = generarRecibo.docCruzado.prefijo.ToString() != null ? generarRecibo.docCruzado.prefijo.ToString() : "",
                numeroCruzado = generarRecibo.docCruzado.numero.ToString() != null ? generarRecibo.docCruzado.numero.ToString() : "",
                /***** adicion mateo ****/
                responsable = generarRecibo.responsable != null ? generarRecibo.responsable.ToUpper() : "",
                vendedor = generarRecibo.vendedor != null ? generarRecibo.vendedor.ToUpper() : "",
                bodega = generarRecibo.bodega != null ? generarRecibo.bodega.ToUpper() : "",
                /***** fin adicion mateo ****/
                referencias = (from encab in context.encab_documento
                               join docPago in context.documentos_pago
                                   on encab.idencabezado equals docPago.idtencabezado
                               join tpr in context.tipopagorecibido
                                   on docPago.forma_pago equals tpr.id
                               join bank in context.bancos
                                   on docPago.banco equals bank.id into temp
                               from bank in temp.DefaultIfEmpty()
                               where encab.idencabezado == id
                               select new referenciasPDF
                               {
                                   tipoPagoRecibido = tpr.pago,
                                   valorInicial = docPago.valor,
                                   nombreBanco = bank.Descripcion,
                                   observacion = encab.nota1
                               }).ToList()
            };
            reciboCaja.referencias.ForEach(item2 => item2.valorInicial = Math.Truncate(item2.valorInicial));

            string customSwitches = string.Format("--print-media-type --allow {0} --header-html {0} --header-spacing 5 --footer-html {1} --footer-spacing 0",
                Url.Action("CabeceraFacturaPDF", "ReciboCaja", new { area = "", prefijo = reciboCaja.prefijo, bodega = reciboCaja.bodega, numeroRegistro = reciboCaja.numeroRegistro, fechaEncabezado= reciboCaja.fechaEncabezado, prefijoCruzado = reciboCaja.prefijoCruzado, numeroCruzado = reciboCaja.numeroCruzado }, Request.Url.Scheme), Url.Action("PieFacturaPDF", "ReciboCaja", new { area = "" }, Request.Url.Scheme));

            ViewAsPdf something = new ViewAsPdf("reciboCaja", reciboCaja)
            {
                PageOrientation = Orientation.Landscape,
                CustomSwitches = customSwitches,
                PageSize = Size.Letter,
                PageMargins = new Margins { Top = 40, Bottom = 20 }
            };

            return something;
        }
        [AllowAnonymous]
        public ActionResult CabeceraFacturaPDF(string prefijo, string bodega, long numeroRegistro,string fechaEncabezado, string prefijoCruzado, string numeroCruzado)
        {
            var recibido = Request;
            var modelo = new PDFmodel
            {
                bodega = bodega,
                prefijo = prefijo,
                numeroRegistro = numeroRegistro,
                fechaEncabezado= fechaEncabezado,
                prefijoCruzado = prefijoCruzado,
                numeroCruzado= numeroCruzado
            };

            return View(modelo);
        }
        [AllowAnonymous]
        public ActionResult PieFacturaPDF()
        {
            return View();
        }

        public JsonResult BuscarFacturasFiltro(int? nit, DateTime? desde, DateTime? hasta, int? id_documento,
            int? factura, int? pedido)
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
                icb_sysparameter swND = context.icb_sysparameter.Where(d => d.syspar_cod == "P128").FirstOrDefault();
                int swND2 = swND != null ? Convert.ToInt32(swND.syspar_value) : 1013;
                icb_sysparameter swF = context.icb_sysparameter.Where(d => d.syspar_cod == "P129").FirstOrDefault();
                int swF2 = swF != null ? Convert.ToInt32(swF.syspar_value) : 3;
                listaDocumentos = context.tp_doc_registros.Where(x => x.sw == swF2 || x.sw == swND2)
                    .Select(x => x.tpdoc_id).ToList();
            }
            System.Linq.Expressions.Expression<Func<encab_documento, bool>> predicado = PredicateBuilder.True<encab_documento>();
            if (desde != null)
            {
                predicado = predicado.And(d => d.fecha >= desde);
            }
            if (hasta != null)
            {
                hasta = hasta.Value.AddDays(1);
                predicado = predicado.And(d => d.fecha <= hasta);
            }
            if (factura != null)
            {
                predicado = predicado.And(d => d.numero == factura);
            }
            if (nit != null)
            {
                predicado = predicado.And(d => d.nit == nit);
            }
            if (pedido != null)
            {
                //busco el numero del pedido
                var pedidox = context.vpedido.Where(d => d.id == pedido).FirstOrDefault();
                if (pedidox != null)
                {
                    predicado = predicado.And(d => d.id_pedido_vehiculo == pedidox.numero || d.pedido == pedidox.numero || d.id_pedido_vehiculo == pedidox.id || d.pedido == pedidox.id);
                }
            }
            predicado = predicado.And(d => 1 == 1 && listaDocumentos.Contains(d.tipo));
            predicado = predicado.And(d => d.valor_aplicado < d.valor_total);

            List<encab_documento> facturas = context.encab_documento.Where(predicado).ToList();

            var buscarFacturasConSaldo = facturas.Select(d => new
            {
                id = d.idencabezado,
                fecha = d.fecha.ToString(),
                d.valor_aplicado,
                d.valor_total,
                d.numero,
                vencimiento = d.vencimiento != null ? d.vencimiento.Value.ToString() : "",
                idTipo = d.tp_doc_registros.tpdoc_id,
                tipo = "(" + d.tp_doc_registros.prefijo + ") " + d.tp_doc_registros.tpdoc_nombre,
                saldo = d.valor_total - d.valor_aplicado,
                descripcion = d.tp_doc_registros.tpdoc_nombre,
                numeroFactura = d.numero,
                prefijo = d.idencabezado,
                tp = d.tipo
            }).ToList();
            var data = buscarFacturasConSaldo.Select(x => new
            {
                x.id,
                x.fecha,
                x.valor_aplicado,
                x.valor_total,
                x.numero,
                x.vencimiento,
                x.idTipo,
                x.tipo,
                x.saldo,
                x.descripcion,
                x.numeroFactura,
                x.prefijo,
                x.tp
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarNitTipo(int? enc)
        {
            var nit = (from e in context.encab_documento
                       where e.idencabezado == enc
                       select new
                       {
                           e.nit,
                           e.tipo
                       }).FirstOrDefault();

            return Json(nit, JsonRequestBehavior.AllowGet);
        }

        //cruceL
        public JsonResult Cruce(string[][] facturas, int? rcNcid, int rcNctipo, int rcNcnum)
        {
            int result = 0;
            int r1 = 0;
            int r2 = 0;
            int r3 = 0;
            decimal totalFacturas = 0;
            encab_documento rcNc = context.encab_documento.Where(d => d.idencabezado == rcNcid && d.tipo == rcNctipo)
                .FirstOrDefault();
            if (facturas != null)
            {
                using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                {
                    try
                    {
                        for (int i = 0; i < facturas.Length; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(facturas[i][3]))
                            {
                                totalFacturas += Convert.ToDecimal(facturas[i][3], inter);
                                int tipo = Convert.ToInt32(facturas[i][0]);
                                int numero = Convert.ToInt32(facturas[i][1]);
                                int id = Convert.ToInt32(facturas[i][2]);
                                decimal valor = Convert.ToDecimal(facturas[i][3], inter);
                                encab_documento factura = context.encab_documento.Where(d => d.idencabezado == id && d.tipo == tipo)
                                    .FirstOrDefault();
                                if (valor != 0)
                                {
                                    int valoraplicado = Convert.ToInt32(factura.valor_aplicado);
                                    decimal nuevovalor = Convert.ToDecimal(valoraplicado, inter) + valor;
                                    factura.valor_aplicado = Convert.ToDecimal(nuevovalor, inter);
                                    context.Entry(factura).State = EntityState.Modified;
                                    r1 = context.SaveChanges();
                                    cruce_documentos cd = new cruce_documentos
                                    {
                                        idtipo = rcNctipo,
                                        numero = rcNcnum,
                                        id_encab_aplica = id,
                                        id_encabezado = rcNcid,
                                        //tipo de la factura cruzada
                                        idtipoaplica = Convert.ToInt32(tipo),
                                        //numero de la factura cruzada
                                        numeroaplica = Convert.ToInt32(numero),
                                        //valor aplicado a cada factura
                                        valor = Convert.ToDecimal(valor, inter),
                                        fecha = DateTime.Now,
                                        fechacruce = DateTime.Now,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                                    };
                                    context.cruce_documentos.Add(cd);
                                    r2 = context.SaveChanges();
                                }
                            }
                        }

                        int valoraplicadoRN = Convert.ToInt32(rcNc.valor_aplicado);
                        decimal nuevovalorRN = Convert.ToDecimal(valoraplicadoRN, inter) + totalFacturas;
                        rcNc.valor_aplicado = Convert.ToDecimal(nuevovalorRN, inter);
                        context.Entry(rcNc).State = EntityState.Modified;
                        r3 = context.SaveChanges();

                        if (r1 != 0 && r2 != 0 && r3 != 0)
                        {
                            dbTran.Commit();
                            result = 1;
                        }

                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    catch
                    {
                        dbTran.Rollback();
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPagos()
        {
            var pagos = from p in context.tipopagorecibido
                        select new
                        {
                            p.id,
                            p.pago
                        };

            var bancos = from b in context.bancos
                         select new
                         {
                             b.id,
                             financiera_nombre = b.Descripcion
                         };

            var data = new
            {
                pagos,
                bancos
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //browser facturacion
        public JsonResult BuscarDatos()
        {
            var data = from e in context.encab_documento
                       join tp in context.tp_doc_registros
                           on e.tipo equals tp.tpdoc_id
                       join b in context.bodega_concesionario
                           on e.bodega equals b.id
                       join t in context.icb_terceros
                           on e.nit equals t.tercero_id
                       where tp.tipo == 16
                       select new
                       {
                           tipo = "(" + tp.prefijo + ") " + tp.tpdoc_nombre,
                           e.numero,
                           nit = t.prinom_tercero != null
                               ? "(" + t.doc_tercero + ") " + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                 t.apellido_tercero + " " + t.segapellido_tercero
                               : "(" + t.doc_tercero + ") " + t.razon_social,
                           fecha = e.fecha.ToString(),
                           e.valor_total,
                           id = e.idencabezado,
                           bodega = b.bodccs_nombre
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarSolicitudes(int idcliente)
        {
            DateTime fechaActual = DateTime.Now;
            DateTime dosMesesAntes = fechaActual.AddMonths(-2);
            var creditos = (from info in context.vinfcredito
                            join cred in context.v_creditos
                            on info.id equals cred.infocredito_id
                            where info.tercero == idcliente && cred.estadoc == "A" && cred.fec_solicitud >= dosMesesAntes
                            select new
                            {
                                info.id,
                                cred.financiera_id,
                                cred.icb_unidad_financiera.financiera_nombre,
                                cred.fec_solicitud,
                                estado = context.estados_credito.Where(x => x.codigo == cred.estadoc).FirstOrDefault().descripcion,
                                cred.vsolicitado,
                                cred.vaprobado
                            }).ToList();

            var data = creditos.Select(x => new
            {
                x.id,
                x.financiera_id,
                financiera = x.financiera_nombre,
                solicitud = x.fec_solicitud != null ? x.fec_solicitud.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                x.estado,
                x.vsolicitado,
                x.vaprobado
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPedidoSelecionado(int nit, int? pedido)
        {
            int dp = (from d in context.documentos_pago
                      where d.tercero == nit && d.pedido == pedido
                      select new { d }).Count();
            if (dp > 0)
            {
                var buscarSaldoPedido = (from v in context.vpedido
                                         join m in context.modelo_vehiculo
                                             on v.modelo equals m.modvh_codigo
                                         join c in context.color_vehiculo
                                             on v.Color_Deseado equals c.colvh_id into a
                                         from c in a.DefaultIfEmpty()
                                         where v.id == pedido && v.nit == nit
                                         select new
                                         {
                                             v.id,
                                             v.numero,
                                             idCarro = v.modelo,
                                             carro = m.modvh_nombre,
                                             color = v.Color_Deseado != null ? c.colvh_nombre : "",
                                             fecha = v.fecha.ToString(),
                                             valorTotal = v.vrtotal
                                         }).ToList();


                decimal valorAplicado = context.documentos_pago.Where(x => x.pedido == pedido && x.tercero == nit)
                    .Select(x => x.valor).Sum();

                //determino que creditos hay asociados al pedido y aprobados

                List<v_creditos> creditos = context.v_creditos.Where(d => d.pedido == pedido && d.estadoc == "A").ToList();
                var listacreditos = creditos.Select(d => new
                {
                    id = d.Id,
                    nombre = d.icb_unidad_financiera.financiera_nombre + " - " +
                             d.fec_aprobacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) + " - " +
                             d.vaprobado.Value.ToString("N0", new CultureInfo("is-IS"))
                }).ToList();

                var data = buscarSaldoPedido.Select(x => new
                {
                    x.id,
                    x.numero,
                    x.idCarro,
                    x.carro,
                    x.color,
                    x.fecha,
                    x.valorTotal,
                    valorAplicado,
                    saldo = x.valorTotal - valorAplicado,
                    listacreditos,
                    haycreditos = listacreditos.Count()
                });

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var buscarSaldoPedido = (from v in context.vpedido
                                         join m in context.modelo_vehiculo
                                             on v.modelo equals m.modvh_codigo
                                         join c in context.color_vehiculo
                                             on v.Color_Deseado equals c.colvh_id into a
                                         from c in a.DefaultIfEmpty()
                                         where v.id == pedido && v.nit == nit
                                         select new
                                         {
                                             v.id,
                                             v.numero,
                                             idCarro = v.modelo,
                                             carro = m.modvh_nombre,
                                             color = v.Color_Deseado != null ? c.colvh_nombre : "",
                                             fecha = v.fecha.ToString(),
                                             valorTotal = v.vrtotal,
                                             valorAplicado = 0,
                                             saldo = v.vrtotal
                                         }).ToList();

                List<v_creditos> creditos = context.v_creditos.Where(d => d.pedido == pedido && d.estadoc == "A").ToList();
                var listacreditos = creditos.Select(d => new
                {
                    id = d.Id,
                    nombre = d.icb_unidad_financiera.financiera_nombre + " - " +
                             d.fec_aprobacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) + " - " +
                             d.vaprobado.Value.ToString("N0", new CultureInfo("is-IS"))
                }).ToList();

                var data = buscarSaldoPedido.Select(x => new
                {
                    x.id,
                    x.numero,
                    x.idCarro,
                    x.carro,
                    x.color,
                    x.fecha,
                    x.valorTotal,
                    x.valorAplicado,
                    x.saldo,
                    listacreditos,
                    haycreditos = listacreditos.Count()
                });
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult BuscarDatosDetalles(int tipo, int numero, int encab)
        {


            var datos = (from a in context.encab_documento
                         join sa in context.rseparacion_anticipo
                            on a.idencabezado equals sa.anticipo_id
                         where a.idencabezado == encab
                         select new
                         {
                             tipo = "(Separacion de Mercancia)",
                             numero = sa.separacion_id,
                             fecha = a.fecha.ToString(),
                             vencimiento = a.vencimiento.Value.ToString(),
                             a.valor_total,
                             valor_aplicado = sa.Valor,
                             saldo = a.valor_total - sa.Valor

                         }).ToList();

            var data = (from c in context.cruce_documentos
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
                            fecha = e.fecha.ToString(),
                            vencimiento = e.vencimiento.Value.ToString(),
                            e.valor_total,
                            e.valor_aplicado,
                            saldo = e.valor_total - e.valor_aplicado
                        }).ToList();



            return Json( new { data, datos }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarBloqueoTercero(int tercero_id)
        {
            var cliente = (from t in context.tercero_cliente
                           where t.tercero_id == tercero_id
                           select new
                           {
                               t.bloqueado,
                               t.motivo_bloqueado
                           }).ToList();
            //que me traiga solo facturacion de vehiculos, accesorios y notas de debito? nah
            var pedido2 = (from p in context.vpedido
                           join m in context.modelo_vehiculo
                               on p.modelo equals m.modvh_codigo
                           join x in context.encab_documento on p.id equals x.pedido
                           where p.nit == tercero_id && (x.valor_total > x.valor_aplicado) && x.anulado == false
                           select new
                           {
                               p.id,
                               p.numero,
                               carro = m.modvh_nombre
                           }).ToList();

            var pedido = pedido2.GroupBy(d => d.id).Select(d => new
            {
                id = d.Key,
                numero = d.Select(e => e.numero).FirstOrDefault(),
                carro = d.Select(e => e.carro).FirstOrDefault(),
            }).ToList();

            var data = new
            {
                cliente,
                pedido
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPagosRecibidos(int? pedido, int? id)
        {
            if (pedido != null)
            {
                var data = (from dp in context.documentos_pago
                            join fp in context.tipopagorecibido
                                on dp.forma_pago equals fp.id
                            where dp.pedido == pedido
                            select new
                            {
                                fpago = fp.pago,
                                dp.valor,
                                fecha = dp.fecha.ToString()
                            }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var data = (from dp in context.documentos_pago
                            join fp in context.tipopagorecibido
                                on dp.forma_pago equals fp.id
                            where dp.idtencabezado == id
                            select new
                            {
                                fpago = fp.pago,
                                dp.valor,
                                fecha = dp.fecha.ToString()
                            }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult reciboCajaDesembolso(int? menu, int planmayor)
        {
            BuscarFavoritos(menu);
            listas(null);
            ViewBag.infoUser = "";
            return View();
        }

        public JsonResult BuscarCuentaBanco(int? id)
        {

            if (id != null)
            {
                //busco el banco
                bancos existe = context.bancos.Where(d => d.id == id).FirstOrDefault();
                if (existe != null)
                {
                    string cuenta = existe.cuenta;
                    return Json(cuenta);
                }
                else
                {
                    return Json(0);
                }
            }
            else
            {
                return Json(0);
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

        public class listaCreditos
        {
            public int id { get; set; }
            public string nombre { get; set; }
        }

        public class desP
        {
            public int id { get; set; }
            public int numero { get; set; }
            public string carro { get; set; }
        }
    }
}