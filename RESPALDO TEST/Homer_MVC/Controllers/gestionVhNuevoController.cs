using Homer_MVC.IcebergModel;
using Homer_MVC.ModeloVehiculos;
using Homer_MVC.Models;
using Microsoft.Ajax.Utilities;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    //Leonardo
    public class gestionVhNuevoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");

        private class datosProveedor
        {
            public int? acteco_id { get; set; }
            public string acteco_nombre { get; set; }
            public int? fpago_id { get; set; }
            public string retfuente { get; set; }
            public string retica { get; set; }
            public string retiva { get; set; }
            public decimal tarifaPorBodega { get; set; }
            public decimal actecoTarifa { get; set; }
            public int? regimen_id { get; set; }

        }
        // GET: inventario_vhNuevo
        public ActionResult FacturacionGM(int? menu)
        {
            var buscarTipoDocumento = (from tipoDocumento in context.tp_doc_registros
                                       where tipoDocumento.tipo == 1
                                       select new
                                       {
                                           tipoDocumento.tpdoc_id,
                                           nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                           tipoDocumento.tipo,
                                           nomoden = tipoDocumento.tpdoc_nombre
                                       }).OrderBy(td => td.nomoden).ToList();
            ViewBag.tipo_documentoFiltro = new SelectList(buscarTipoDocumento, "tpdoc_id", "nombre");
            var provedores = (from pro in context.tercero_proveedor
                              join ter in context.icb_terceros
                                  on pro.tercero_id equals ter.tercero_id
                              select new
                              {
                                  idTercero = ter.tercero_id,
                                  // nombreTErcero =  ter.prinom_tercero, 
                                  nombreTErcero = ter.prinom_tercero,
                                  apellidosTercero = ter.apellido_tercero,
                                  razonSocial = ter.razon_social,
                                  nit = ter.doc_tercero
                              }).OrderBy(x => x.nombreTErcero);
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in provedores)
            {
                // var nombre = item.nombreTErcero + " " + item.apellidosTercero + " " + item.razonSocial;   
                string nombre = "(" + item.nit + ") " + item.nombreTErcero + " " + item.apellidosTercero + " " +
                             item.razonSocial;
                items.Add(new SelectListItem { Text = nombre, Value = item.idTercero.ToString() });
            }

            ViewBag.proveedor = items;
            icb_sysparameter buscarParametroSw = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P23");
            string swParametro = buscarParametroSw != null ? buscarParametroSw.syspar_value : "1";
            int idTipoSw = Convert.ToInt32(swParametro);
            ViewBag.doc_registros = context.tp_doc_registros.Where(x => x.sw == idTipoSw && x.tipo == 1)
                .OrderBy(x => x.tpdoc_nombre);
            ViewBag.condicion_pago = context.fpago_tercero;
            ViewBag.Cantidad = (context.icb_vehiculo.Count() + 9) / 10;

            List<menu_busqueda> detallesFacturacion = context.menu_busqueda
                .Where(x => x.menu_busqueda_id_menu == 79 && x.menu_busqueda_id_pestana == 4).ToList();
            ViewBag.paramBusquedaDetalles = detallesFacturacion;
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult BuscarTipoPagoProveedor(int? id_tercero)
        {
            var buscarTercero = (from tercero in context.icb_terceros
                                 join proveedor in context.tercero_proveedor
                                     on tercero.tercero_id equals proveedor.tercero_id
                                 join pago in context.fpago_tercero
                                     on proveedor.fpago_id equals pago.fpago_id
                                 where tercero.tercero_id == id_tercero
                                 select new { pago.fpago_id, pago.fpago_nombre }).FirstOrDefault();
            return Json(buscarTercero, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPerfilesDocumentoYBodegaProveedor(int? idBodega, int? idTipoDoc, int? idProveedor)
        {
            var proveedorIva =
                (from proveedor in context.tercero_proveedor
                 where proveedor.tercero_id == idProveedor
                 select new { proveedor.exentoiva, proveedor.tpregimen_id }).FirstOrDefault();
            string proveedorIva2 = "";
            int tpregimen = 0;
            if (proveedorIva != null)
            {
                tpregimen = proveedorIva.tpregimen_id;
                if (proveedorIva.exentoiva)
                {
                    proveedorIva2 = "N";
                }
                else if (proveedorIva.exentoiva == false)
                {
                    proveedorIva2 = "A";
                }
            }

            var buscarPerfiles = (from perfilContable in context.perfil_contable_documento
                                  join perfilBodega in context.perfil_contable_bodega
                                      on perfilContable.id equals perfilBodega.idperfil
                                  where perfilContable.tipo == idTipoDoc && perfilBodega.idbodega == idBodega
                                  select new
                                  {
                                      perfilContable.id,
                                      perfilContable.codigo,
                                      perfilContable.descripcion,
                                      perfilContable.iva,
                                      perfilContable.regimen_tercero
                                  }).ToList();
            var buscarPerfiles2 = buscarPerfiles;
            int existeiva = buscarPerfiles.Where(d => d.iva != null).Count();
            if (existeiva > 0)
            {
                buscarPerfiles2 = buscarPerfiles.Where(d => d.iva == proveedorIva2 && d.regimen_tercero == tpregimen)
                    .ToList();
            }

            if (buscarPerfiles2.Count == 0)
            {
                buscarPerfiles2 = buscarPerfiles;
            }

            return Json(buscarPerfiles2, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPerfilesDocumentoYBodega(int? idBodega, int? idTipoDoc)
        {
            var buscarPerfiles = (from perfilContable in context.perfil_contable_documento
                                  join perfilBodega in context.perfil_contable_bodega
                                      on perfilContable.id equals perfilBodega.idperfil
                                  where perfilContable.tipo == idTipoDoc && perfilBodega.idbodega == idBodega
                                  select new
                                  {
                                      perfilContable.id,
                                      perfilContable.codigo,
                                      perfilContable.descripcion,
                                      perfilContable.iva,
                                      perfilContable.regimen_tercero
                                  }).ToList();


            return Json(buscarPerfiles, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarBodegasPorDocumento(int? id)
        {
            var buscarBodega = (from consecutivos in context.icb_doc_consecutivos
                                join bodega in context.bodega_concesionario
                                    on consecutivos.doccons_bodega equals bodega.id
                                where consecutivos.doccons_idtpdoc == id
                                select new
                                {
                                    bodega.bodccs_nombre,
                                    bodega.id
                                }).Distinct().OrderBy(bn => bn.bodccs_nombre).ToList();

            //busco las bodegas que tenga habilitado el 
            var sesion = Session["user_usuarioid"] != null ? Session["user_usuarioid"].ToString() : "";
            var id1 = 0;
            var convertir = int.TryParse(sesion, out id1);
            var bods = context.bodega_usuario.Where(d => d.id_usuario == id1).GroupBy(d => d.id_bodega).Select(d => d.Key).ToList();
            if (convertir == true)
            {
                buscarBodega = buscarBodega.Where(d => bods.Contains(d.id)).ToList();
            }

            var buscarConceptos1 = (from concepto1 in context.tpdocconceptos
                                    where concepto1.tipodocid == id
                                    select new
                                    {
                                        concepto1.id,
                                        concepto1.Descripcion
                                    }).ToList();

            var buscarConceptos2 = (from concepto2 in context.tpdocconceptos2
                                    where concepto2.tipodocid == id
                                    select new
                                    {
                                        concepto2.id,
                                        concepto2.Descripcion
                                    }).ToList();

            var buscarPerfilContable = context.perfil_contable_documento.Where(x => x.tipo == id).Select(x => new
            {
                value = x.id,
                text = x.descripcion
            }).ToList();

            var data = new
            {
                buscarBodega,
                buscarConceptos1,
                buscarConceptos2,
                buscarPerfilContable
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FacturacionGM(HttpPostedFileBase txtfile, int? menu)
        {
            List<anioModeloModel> anios_modelo = new List<anioModeloModel> { };
            using (DbContextTransaction dbTran = context.Database.BeginTransaction())
            {
                try
                {
                    #region creacion y guardado de archivo plano

                    string path = Server.MapPath("~/Content/" + txtfile.FileName);
                    // Validacion para cuando el archivo esta en uso y no puede ser usado desde visual 
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    txtfile.SaveAs(path);

                    #endregion

                    #region Variables tipo Request

                    List<vconceptocompra> tipoCompraVehiculos = context.vconceptocompra.ToList();
                    int codigoBodega = Convert.ToInt32(Request["selectBodegas"]);
                    int idTipoDocumento = Convert.ToInt32(Request["selectTipoDocumento"]);
                    decimal fletes = !string.IsNullOrEmpty(Request["txtFletes"])
                        ? Convert.ToDecimal(Request["txtFletes"].Replace(".", ","), miCultura)
                        : 0;
                    decimal porcentajeIvaFletes = !string.IsNullOrEmpty(Request["txtPorcentajeFletes"])
                        ? Convert.ToDecimal(Request["txtPorcentajeFletes"], miCultura)
                        : 0;
                    decimal descuentoPie = !string.IsNullOrEmpty(Request["txtDescuentoPie"])
                        ? Convert.ToDecimal(Request["txtDescuentoPie"], miCultura)
                        : 0;

                    #endregion
                    using (var lector2 = System.IO.File.Open(path, FileMode.Open))
                    {
                        using (StreamReader lector = new StreamReader(lector2))
                        {
                            #region Variables

                            bool encabezado = true;
                            int numeroLinea = 1;
                            int itemsFallidos = 0;
                            int itemsCorrectos = 0;
                            string log = "";
                            List<CrearVehiculoExcel> listaVehiculos = new List<CrearVehiculoExcel>();
                            string planMayor = "";
                            DateTime? fecha = null;
                            int anio = 0;
                            string codigoModelo = "";
                            string codigoColor = "";
                            string kmat = "";
                            string numeroMotor = "";
                            string notas = "";
                            bool flota = false;
                            string codigoPago = "";
                            string bodegaGM = "";
                            string iva = "";
                            long numpedido_vh = 0;
                            decimal ValorFactura = 0;
                            decimal valorSinIva = 0;
                            decimal porcentajeIva = 0;
                            decimal impuestoConsumo = 0;
                            DateTime? fechaFinal = null;
                            string numManifiesto = "";
                            string fechaManifiesto = null;
                            int dia;
                            int mes;
                            int anho;

                            bool errorVin = false;
                            bool errorPedido = false;
                            int lineaError = 1;
                            int idProveedor = 0;
                            idProveedor = Convert.ToInt32(Request["selectProveedor"]);

                            int guardarLIneas = 0;

                            #endregion

                            #region Consulta y datos del proveedor

                            tp_doc_registros buscarTipoDocRegistro =
                                context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == idTipoDocumento);
                            // Validacion para reteIVA, reteICA y retencion dependiendo del proveedor seleccionado
                            bool actividadEconomicaValido = false;
                            decimal porcentajeReteICA = 0;
                            datosProveedor buscarProveedor = (from tercero in context.icb_terceros
                                                              join proveedor in context.tercero_proveedor
                                                                  on tercero.tercero_id equals proveedor.tercero_id
                                                              join acteco in context.acteco_tercero
                                                                  on tercero.id_acteco equals acteco.acteco_id
                                                              join bodega in context.terceros_bod_ica
                                                                  on new { val1 = acteco.acteco_id, val2 = codigoBodega } equals new { val1 = bodega.idcodica, val2 = bodega.bodega } into ps
                                                              from bodega in ps.DefaultIfEmpty()
                                                              join regimen in context.tpregimen_tercero
                                                                  on tercero.tpregimen_id equals regimen.tpregimen_id into ps2
                                                              from regimen in ps2.DefaultIfEmpty()
                                                              join pago in context.fpago_tercero
                                                                  on proveedor.fpago_id equals pago.fpago_id into tp
                                                              from pago in tp.DefaultIfEmpty()
                                                              where proveedor.tercero_id == idProveedor
                                                              select new datosProveedor
                                                              {
                                                                  acteco_id = proveedor.acteco_id,
                                                                  acteco_nombre = acteco.acteco_nombre,
                                                                  fpago_id = pago.fpago_id,
                                                                  retfuente = tercero.retfuente,
                                                                  retica = tercero.retica,
                                                                  retiva = tercero.retiva,
                                                                  tarifaPorBodega = bodega != null ? bodega.porcentaje : 0,
                                                                  actecoTarifa = acteco.tarifa,
                                                                  regimen_id = regimen.tpregimen_id
                                                              }).FirstOrDefault();
                            if (buscarProveedor != null)
                            {
                                if (buscarProveedor.tarifaPorBodega != 0)
                                {
                                    actividadEconomicaValido = true;
                                    porcentajeReteICA = buscarProveedor.tarifaPorBodega;
                                }
                                else if (buscarProveedor.actecoTarifa != 0)
                                {
                                    actividadEconomicaValido = true;
                                    porcentajeReteICA = buscarProveedor.actecoTarifa;
                                }
                            }

                            int? regimen_proveedor = buscarProveedor != null ? buscarProveedor.regimen_id : 0;
                            /*var buscarProveedor2 =
                                context.tercero_proveedor.FirstOrDefault(x => x.tercero_id == idProveedor);*/
                            datosProveedor buscarProveedor2 = buscarProveedor;
                            #endregion

                            #region Lectura archivo plano

                            vtipo_compra_vehiculos buscarTipoCompra = new vtipo_compra_vehiculos();
                            while (lector.Peek() > -1)
                            {
                                string linea = lector.ReadLine();
                                if (!encabezado)
                                {
                                    if (!string.IsNullOrEmpty(linea))
                                    {
                                        if (numeroLinea == 1)
                                        {
                                            lineaError++;
                                            planMayor = linea.Substring(2, 10);
                                            fecha = new DateTime(Convert.ToInt32(linea.Substring(12, 4)),
                                                Convert.ToInt32(linea.Substring(16, 2)),
                                                Convert.ToInt32(linea.Substring(18, 2)));
                                            ValorFactura = Convert.ToDecimal(linea.Substring(42, 11).Trim(), miCultura);
                                            codigoPago = linea.Substring(74, 4);
                                            iva = linea.Substring(58, 10);

                                            numeroLinea = 2;
                                        }
                                        else
                                        {
                                            lineaError++;
                                            anio = 2000 + Convert.ToInt32(linea.Substring(37, 2));
                                            codigoModelo = linea.Substring(39, 11);
                                            kmat = linea.Substring(37, 17);
                                            modelo_vehiculo buscaModelo =
                                                context.modelo_vehiculo.FirstOrDefault(x => x.modvh_codigo == codigoModelo);
                                            if (buscaModelo == null)
                                            {
                                                TempData["mensajeError"] =
                                                    "El codigo de modelo " + codigoModelo +
                                                    " no existe, debe registrarlo antes de continuar, linea de error " +
                                                    lineaError;
                                                return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
                                            }
                                            valorSinIva = Convert.ToDecimal(linea.Substring(127, 21).Trim(), miCultura);
                                            //impuestoConsumo = buscaAnioModelo.impuesto_consumo;

                                            porcentajeIva = decimal.Round(ValorFactura * 100 / valorSinIva - 100, 2);


                                            decimal precioconiva = ValorFactura;

                                            codigoColor = linea.Substring(51, 3);
                                            color_vehiculo buscaColor =
                                                context.color_vehiculo.FirstOrDefault(x => x.colvh_id == codigoColor);
                                            if (buscaColor == null)
                                            {
                                                dbTran.Rollback();
                                                TempData["mensajeError"] =
                                                    "El codigo de color " + codigoColor +
                                                    " no existe, debe registrarlo antes de continuar, linea de error " +
                                                    lineaError;
                                                return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
                                            }

                                            string VIN = linea.Substring(75, 17);
                                            numeroMotor = linea.Substring(55, 20).Trim();
                                            //numpedido_vh = Convert.ToInt64(linea.Substring(27, 10));
                                            bodegaGM = linea.Substring(393, 4);
                                            notas = linea.Substring(319, 45).Trim();
                                            numManifiesto = linea.Substring(451, 15).Trim();
                                            fechaManifiesto = linea.Substring(481, 8);
                                            anho = Convert.ToInt32(linea.Substring(481, 4));
                                            mes = Convert.ToInt32(linea.Substring(485, 2));
                                            dia = Convert.ToInt32(linea.Substring(487, 2));
                                            string fec_man = anho + "/" + mes + "/" + dia;
                                            errorVin = false;
                                            errorPedido = false;

                                            var buscarAutomovil = (from vehiculo in context.icb_vehiculo
                                                                   join encabezadoDoc in context.encab_documento
                                                                       on vehiculo.plan_mayor equals encabezadoDoc.documento into veh
                                                                   from encabezadoDoc in veh.DefaultIfEmpty()
                                                                   where vehiculo.plan_mayor == planMayor
                                                                   select new
                                                                   {
                                                                       anulado = encabezadoDoc != null ? encabezadoDoc.anulado : false
                                                                   }).FirstOrDefault();

                                            pedido_GM buscarPedidoFirme =
                                                context.pedido_GM.FirstOrDefault(x => x.pedido_codigo == numpedido_vh);

                                            bool vehiculo_actualizar = false;
                                            if (buscarAutomovil != null)
                                            {
                                                if (buscarAutomovil.anulado)
                                                {
                                                    if (buscarPedidoFirme != null)
                                                    {
                                                        // Significa que ya esta agregado pero esta anulado, por tanto se puede volver a agregar
                                                        vehiculo_actualizar = true;
                                                        itemsCorrectos++;
                                                    }
                                                    else
                                                    {
                                                        //itemsFallidos++;
                                                        //errorPedido = true;
                                                        //if (log.Length < 1)
                                                        //    log += numpedido_vh + "*El numero de pedido en firme NO existe";
                                                        //else
                                                        //    log += "|" + numpedido_vh +
                                                        //           "*El numero de pedido en firme NO existe";

                                                        vehiculo_actualizar = true;
                                                        itemsCorrectos++;
                                                    }
                                                }
                                                else
                                                {
                                                    itemsFallidos++;
                                                    errorVin = true;
                                                    if (log.Length < 1)
                                                    {
                                                        log += VIN + "*El numero VIN ya existe";
                                                    }
                                                    else
                                                    {
                                                        log += "|" + VIN + "*El numero VIN ya existe";
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (buscarPedidoFirme != null)
                                                {
                                                    // Significa que no lo encontro, por tanto se puede agregar sin problemas
                                                    itemsCorrectos++;
                                                }
                                                else
                                                {
                                                    itemsFallidos++;
                                                    errorPedido = true;
                                                    if (log.Length < 1)
                                                    {
                                                        log += numpedido_vh + "*El numero de pedido en firme NO existe";
                                                    }
                                                    else
                                                    {
                                                        log += "|" + numpedido_vh +
                                                               "*El numero de pedido en firme NO existe";
                                                    }
                                                }
                                            }

                                            if (!errorVin && !errorPedido)
                                            {
                                                //veo si existe el año modelo
                                                anio_modelo buscaAnioModelo = context.anio_modelo.FirstOrDefault(x =>
                                                x.codigo_modelo == codigoModelo && x.anio == anio);
                                                if (buscaAnioModelo == null)
                                                {
                                                    //busco el concesionario al cual está adscrita la bodega
                                                    bodega_concesionario bod = context.bodega_concesionario.Where(d => d.id == codigoBodega).FirstOrDefault();

                                                    //busco si el codigoModelo y el año estan en el arreglo
                                                    int existeEnArreglo = anios_modelo.Where(d => d.codigo_modelo == codigoModelo && d.anio == anio).Count();
                                                    if (existeEnArreglo == 0)
                                                    {
                                                        codigo_iva existeporcentajeiva = context.codigo_iva.Where(d => d.porcentaje == porcentajeIva && d.Descripcion == "COMPRA").FirstOrDefault();
                                                        //busco el id del porcentaje iva
                                                        anios_modelo.Add(new anioModeloModel
                                                        {
                                                            anio = anio,
                                                            codigo_modelo = codigoModelo,
                                                            costosiniva = valorSinIva,
                                                            valor = valorSinIva,
                                                            impuesto_consumo = 0,
                                                            matricula = 0,
                                                            poliza = 0,
                                                            precio = ValorFactura,
                                                            porcentaje_compra = porcentajeIva,
                                                            porcentaje_iva = porcentajeIva,
                                                            idporcentajeimpoconsumo = 13,
                                                            idporcentajeiva = existeporcentajeiva != null ? existeporcentajeiva.id : 1,
                                                            descripcion = buscaModelo.modvh_nombre,
                                                            idporcentajecompra = existeporcentajeiva != null ? existeporcentajeiva.id : 1,
                                                            concesionarioid = bod.concesionarioid
                                                        });
                                                    }
                                                }
                                                CrearVehiculoExcel nuevoVehiculo = new CrearVehiculoExcel
                                                {
                                                    fecfact_fabrica = fecha ?? DateTime.Now,
                                                    plan_mayor = planMayor,
                                                    anio_vh = anio,
                                                    modvh_id = codigoModelo,
                                                    colvh_id = codigoColor,
                                                    costototal_vh = ValorFactura,
                                                    costosiniva_vh = valorSinIva,
                                                    iva_vh = ValorFactura - valorSinIva,
                                                    porcentajeIva = porcentajeIva,
                                                    vin = VIN,
                                                    porcentajeReteIca = porcentajeReteICA,
                                                    porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion,
                                                    porcentajeReteIva = (decimal)buscarTipoDocRegistro.retiva,
                                                    nummot_vh = numeroMotor,
                                                    numpedido_vh = numpedido_vh,
                                                    numManifiesto = numManifiesto,
                                                    fechaManifiesto = Convert.ToDateTime(fec_man),
                                                    impuesto_consumo = impuestoConsumo,
                                                    bodegaGM = bodegaGM,
                                                    codigoPago = codigoPago,
                                                    notas = notas,
                                                    flota = flota,
                                                    vehiculo_actualizar = vehiculo_actualizar,
                                                    kmat = kmat,
                                                    porcentaje_iva = porcentajeIva
                                                };
                                                listaVehiculos.Add(nuevoVehiculo);
                                            }

                                            fechaFinal = new DateTime(anho, mes, dia);
                                            if (notas.ToUpper().Contains("FLOTA"))
                                            {
                                                flota = true;
                                            }

                                            numeroLinea = 1;
                                            // Validacion para el debito y credito de las cuentas verificar que los datos estan bien escritos
                                            int idConceptoCompra = 0;
                                            foreach (vconceptocompra item in tipoCompraVehiculos)
                                            {
                                                if (item.codigo.ToUpper().Contains(codigoPago))
                                                {
                                                    idConceptoCompra = item.id;
                                                }
                                            }

                                            buscarTipoCompra = context.vtipo_compra_vehiculos.FirstOrDefault(x =>
                                                x.idtipocomprav == idConceptoCompra && x.idbodega == codigoBodega &&
                                                x.iddoc == idTipoDocumento);

                                            // Significa que hay un perfil contable encontrado
                                            if (buscarTipoCompra != null)
                                            {
                                                List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();
                                                ListaCuentas lista = new ListaCuentas();
                                                if (idTipoDocumento == 1)
                                                {
                                                    lista.ListaCuentasNecesarias = (from p in context.perfiltipocompra
                                                                                    join c in context.vconceptocompra
                                                                                        on p.id equals c.id
                                                                                    join ct in context.perfilcuentastipocompra
                                                                                        on p.id equals ct.idTipoCompra
                                                                                    join cb in context.perfilbodegastipocompra
                                                                                        on p.id equals cb.idTipoCompra
                                                                                    join nombreParametro in context.paramcontablenombres
                                                                                        on ct.idParametro equals nombreParametro.id
                                                                                    join cuenta in context.cuenta_puc
                                                                                        on ct.cuenta equals cuenta.cntpuc_id
                                                                                    where c.codigo == codigoPago && cb.idBodega == codigoBodega
                                                                                    select new cuentasNecesarias
                                                                                    {
                                                                                        id = ct.id,
                                                                                        id_nombre_parametro = ct.idParametro,
                                                                                        cuenta = ct.cuenta,
                                                                                        centro = ct.centro,
                                                                                        codigo = null,
                                                                                        descripcion = p.descripcion,
                                                                                        descripcion_parametro = nombreParametro.descripcion_parametro,
                                                                                        cntpuc_numero = cuenta.cntpuc_numero
                                                                                    }).ToList();
                                                }
                                                else
                                                {
                                                    lista.ListaCuentasNecesarias =
                                                        (from perfil in context.perfil_cuentas_documento
                                                         join nombreParametro in context.paramcontablenombres
                                                             on perfil.id_nombre_parametro equals nombreParametro.id
                                                         join cuenta in context.cuenta_puc
                                                             on perfil.cuenta equals cuenta.cntpuc_id
                                                         join documento in context.perfil_contable_documento
                                                             on perfil.id_perfil equals documento.id
                                                         where perfil.id_perfil == buscarTipoCompra.idperfilcontable
                                                         select new cuentasNecesarias
                                                         {
                                                             id = perfil.id,
                                                             id_nombre_parametro = perfil.id_nombre_parametro,
                                                             cuenta = perfil.cuenta,
                                                             centro = perfil.centro,
                                                             idperfil = perfil.id_perfil,
                                                             codigo = documento.codigo,
                                                             descripcion = documento.descripcion,
                                                             descripcion_parametro =
                                                                 nombreParametro.descripcion_parametro,
                                                             cntpuc_numero = cuenta.cntpuc_numero
                                                         }).ToList();
                                                }

                                                decimal calcularDebito = 0;
                                                decimal calcularCredito = 0;
                                                decimal valorCredito = 0;
                                                decimal valorDebito = 0;
                                                foreach (cuentasNecesarias parametro in lista.ListaCuentasNecesarias)
                                                {
                                                    string descripcionParametro = context.paramcontablenombres
                                                        .FirstOrDefault(x => x.id == parametro.id_nombre_parametro)
                                                        .descripcion_parametro;
                                                    cuenta_puc buscarCuenta =
                                                        context.cuenta_puc.FirstOrDefault(x =>
                                                            x.cntpuc_id == parametro.cuenta);
                                                    if (buscarCuenta != null)
                                                    {
                                                        #region Cuentas por pagar = 1

                                                        if (parametro.id_nombre_parametro == 1)
                                                        {
                                                            #region Perfil tributario

                                                            decimal restarIcaIvaRetencion = 0;

                                                            if (buscarProveedor2.retfuente == null)
                                                            {
                                                                perfiltributario buscarPerfilTributario =
                                                                    context.perfiltributario.FirstOrDefault(x =>
                                                                        x.bodega == codigoBodega &&
                                                                        x.sw == buscarTipoDocRegistro.sw &&
                                                                        x.tipo_regimenid == regimen_proveedor);
                                                                if (buscarPerfilTributario != null)
                                                                // if (buscarTipoDocRegistro != null)
                                                                {
                                                                    if (buscarPerfilTributario.retfuente == "A")
                                                                    {
                                                                        if (valorSinIva >= (buscarPerfilTributario.baseretfuente ?? 0))
                                                                        {
                                                                            decimal porcentajeRetencion =
                                                                                buscarPerfilTributario.pretfuente ?? 0;
                                                                            restarIcaIvaRetencion +=
                                                                                valorSinIva * porcentajeRetencion / 100;
                                                                        }
                                                                    }

                                                                    if (buscarPerfilTributario.retiva == "A")
                                                                    {
                                                                        if (ValorFactura - valorSinIva >= (buscarPerfilTributario.baseretiva ?? 0))
                                                                        {
                                                                            decimal porcentajeRetIva =
                                                                                buscarPerfilTributario.pretiva ?? 0;
                                                                            restarIcaIvaRetencion +=
                                                                                (ValorFactura - valorSinIva) *
                                                                                porcentajeRetIva / 100;
                                                                        }
                                                                    }

                                                                    if (buscarPerfilTributario.retica == "A" &&
                                                                         valorSinIva >= (buscarPerfilTributario.baseretica ?? 0))

                                                                    {


                                                                        if (!actividadEconomicaValido)
                                                                        {
                                                                            if (valorSinIva >= (buscarPerfilTributario.baseretica ?? 0))
                                                                            {
                                                                                decimal porcentajeRetIca = porcentajeReteICA;
                                                                                //(decimal)buscarTipoDocRegistro
                                                                                //    .retica;
                                                                                restarIcaIvaRetencion +=
                                                                                    valorSinIva * porcentajeRetIca /
                                                                                    1000;
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            restarIcaIvaRetencion +=
                                                                                valorSinIva * porcentajeReteICA / 1000;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            else if (buscarProveedor2.retfuente != null && buscarProveedor2.retfuente == "A")
                                                            {

                                                                // if (buscarTipoDocRegistro != null)
                                                                perfiltributario buscarPerfilTributario =
                                                                               context.perfiltributario.FirstOrDefault(x =>
                                                                                   x.bodega == codigoBodega &&
                                                                                   x.sw == buscarTipoDocRegistro.sw &&
                                                                                   x.tipo_regimenid == regimen_proveedor);
                                                                if (buscarPerfilTributario != null)
                                                                {
                                                                    if (buscarProveedor2.retfuente == "A")
                                                                    {
                                                                        if (valorSinIva >= (buscarPerfilTributario.baseretfuente ?? 0))
                                                                        {
                                                                            decimal porcentajeRetencion =
                                                                                buscarPerfilTributario.pretfuente ?? 0;
                                                                            restarIcaIvaRetencion +=
                                                                                valorSinIva * porcentajeRetencion / 100;
                                                                        }
                                                                    }

                                                                    if (buscarProveedor2.retiva == "A")
                                                                    {
                                                                        if (ValorFactura - valorSinIva >= (buscarPerfilTributario.baseretiva ?? 0))
                                                                        {
                                                                            decimal porcentajeRetIva =
                                                                                buscarPerfilTributario.pretiva ?? 0;
                                                                            restarIcaIvaRetencion +=
                                                                                (ValorFactura - valorSinIva) *
                                                                                porcentajeRetIva / 100;
                                                                        }
                                                                    }

                                                                    if (buscarPerfilTributario.retica == "A" &&
                                                                             valorSinIva >= (buscarPerfilTributario.baseretica ?? 0))

                                                                    {


                                                                        if (!actividadEconomicaValido)
                                                                        {
                                                                            if (valorSinIva >= (buscarPerfilTributario.baseretica ?? 0))
                                                                            {
                                                                                decimal porcentajeRetIca = porcentajeReteICA;
                                                                                //(decimal)buscarTipoDocRegistro
                                                                                //    .retica;
                                                                                restarIcaIvaRetencion +=
                                                                                    valorSinIva * porcentajeRetIca /
                                                                                    1000;
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            restarIcaIvaRetencion +=
                                                                                valorSinIva * porcentajeReteICA / 1000;
                                                                        }
                                                                    }

                                                                }

                                                            }

                                                            #endregion

                                                            valorCredito = ValorFactura - restarIcaIvaRetencion;
                                                            calcularCredito += valorCredito;
                                                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                            {
                                                                NumeroCuenta = parametro.cntpuc_numero,
                                                                DescripcionParametro = parametro.descripcion_parametro,
                                                                ValorDebito = 0,
                                                                ValorCredito = valorCredito
                                                            });
                                                        }

                                                        #endregion

                                                        #region IVA = 2

                                                        if (parametro.id_nombre_parametro == 2)
                                                        {
                                                            valorDebito = ValorFactura - valorSinIva;
                                                            calcularDebito += valorDebito;
                                                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                            {
                                                                NumeroCuenta = parametro.cntpuc_numero,
                                                                DescripcionParametro = parametro.descripcion_parametro,
                                                                ValorDebito = valorDebito,
                                                                ValorCredito = 0
                                                            });
                                                        }

                                                        #endregion

                                                        #region Retencion = 3

                                                        if (parametro.id_nombre_parametro == 3)
                                                        {
                                                            perfiltributario buscarPerfilTributario =
                                                                    context.perfiltributario.FirstOrDefault(x =>
                                                                        x.bodega == codigoBodega &&
                                                                        x.sw == buscarTipoDocRegistro.sw &&
                                                                        x.tipo_regimenid == regimen_proveedor);

                                                            if (buscarProveedor2.retfuente == null)
                                                            {
                                                                if (buscarPerfilTributario != null)
                                                                {
                                                                    if (buscarPerfilTributario.retfuente == "A")
                                                                    {
                                                                        if (valorSinIva >= (buscarPerfilTributario.baseretfuente ?? 0))
                                                                        {
                                                                            decimal porcentajeRetencion =
                                                                                buscarPerfilTributario.pretfuente ?? 0;
                                                                            valorCredito =
                                                                                valorSinIva * porcentajeRetencion / 100;
                                                                            calcularCredito += valorCredito;
                                                                            listaDescuadrados.Add(
                                                                                new DocumentoDescuadradoModel
                                                                                {
                                                                                    NumeroCuenta =
                                                                                        parametro.cntpuc_numero,
                                                                                    DescripcionParametro =
                                                                                        parametro.descripcion_parametro,
                                                                                    ValorDebito = 0,
                                                                                    ValorCredito = valorCredito
                                                                                });
                                                                        }
                                                                    }
                                                                }

                                                            }

                                                            if (buscarProveedor2 != null)
                                                            {
                                                                if (buscarProveedor2.retfuente == "A")
                                                                {
                                                                    if (valorSinIva >= (buscarPerfilTributario.baseretfuente ?? 0))
                                                                    {
                                                                        decimal porcentajeRetencion =
                                                                                buscarPerfilTributario.pretfuente ?? 0;
                                                                        valorCredito =
                                                                                valorSinIva * porcentajeRetencion / 100;
                                                                        calcularCredito += valorCredito;
                                                                        listaDescuadrados.Add(
                                                                            new DocumentoDescuadradoModel
                                                                            {
                                                                                NumeroCuenta = parametro.cntpuc_numero,
                                                                                DescripcionParametro =
                                                                                    parametro.descripcion_parametro,
                                                                                ValorDebito = 0,
                                                                                ValorCredito = valorCredito
                                                                            });
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        #endregion

                                                        #region Retencion Iva = 4

                                                        if (parametro.id_nombre_parametro == 4)
                                                        {
                                                            if (buscarProveedor2 == null || string.IsNullOrWhiteSpace(buscarProveedor2.retiva))
                                                            {
                                                                perfiltributario buscarPerfilTributario =
                                                                    context.perfiltributario.FirstOrDefault(x =>
                                                                        x.bodega == codigoBodega &&
                                                                        x.sw == buscarTipoDocRegistro.sw &&
                                                                        x.tipo_regimenid == regimen_proveedor);
                                                                if (buscarPerfilTributario != null)
                                                                {
                                                                    if (buscarPerfilTributario.retiva == "A")
                                                                    {
                                                                        if (valorSinIva >= (buscarPerfilTributario.baseretfuente ?? 0))
                                                                        {
                                                                            decimal porcentajeRetIva =
                                                                                    buscarPerfilTributario.pretiva ?? 0;
                                                                            valorCredito =
                                                                                (ValorFactura - valorSinIva) *
                                                                                porcentajeRetIva / 100;
                                                                            calcularCredito += valorCredito;
                                                                            listaDescuadrados.Add(
                                                                                new DocumentoDescuadradoModel
                                                                                {
                                                                                    NumeroCuenta =
                                                                                        parametro.cntpuc_numero,
                                                                                    DescripcionParametro =
                                                                                        parametro.descripcion_parametro,
                                                                                    ValorDebito = 0,
                                                                                    ValorCredito = valorCredito
                                                                                });
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            if (buscarProveedor2 != null)
                                                            {
                                                                perfiltributario buscarPerfilTributario =
                                                                              context.perfiltributario.FirstOrDefault(x =>
                                                                                  x.bodega == codigoBodega &&
                                                                                  x.sw == buscarTipoDocRegistro.sw &&
                                                                                  x.tipo_regimenid == regimen_proveedor);

                                                                if (buscarProveedor2.retiva == "A")
                                                                {
                                                                    if ((buscarPerfilTributario.baseretiva ?? 0) <= valorSinIva)
                                                                    {
                                                                        decimal porcentajeRetIva =
                                                                            buscarPerfilTributario.pretiva ?? 0;
                                                                        valorCredito =
                                                                            (ValorFactura - valorSinIva) *
                                                                            porcentajeRetIva / 100;
                                                                        calcularCredito += valorCredito;
                                                                        listaDescuadrados.Add(
                                                                            new DocumentoDescuadradoModel
                                                                            {
                                                                                NumeroCuenta = parametro.cntpuc_numero,
                                                                                DescripcionParametro =
                                                                                    parametro.descripcion_parametro,
                                                                                ValorDebito = 0,
                                                                                ValorCredito = valorCredito
                                                                            });
                                                                    }
                                                                }
                                                            }

                                                        }

                                                        #endregion

                                                        #region Retencion Ica = 5

                                                        if (parametro.id_nombre_parametro == 5)
                                                        {
                                                            if (string.IsNullOrWhiteSpace(buscarProveedor2.retfuente))
                                                            {
                                                                perfiltributario buscarPerfilTributario =
                                                                    context.perfiltributario.FirstOrDefault(x =>
                                                                        x.bodega == codigoBodega &&
                                                                        x.sw == buscarTipoDocRegistro.sw &&
                                                                        x.tipo_regimenid == regimen_proveedor);

                                                                if (buscarPerfilTributario != null)
                                                                {
                                                                    if (buscarPerfilTributario.retica == "A")
                                                                    {
                                                                        if (!actividadEconomicaValido)
                                                                        {
                                                                            if ((buscarPerfilTributario.baseretica ?? 0) <=
                                                                                valorSinIva)
                                                                            {
                                                                                decimal porcentajeRetIca = (buscarPerfilTributario.pretica ?? 0);
                                                                                valorCredito =
                                                                                    valorSinIva * porcentajeRetIca /
                                                                                    1000;
                                                                                calcularCredito += valorCredito;
                                                                                listaDescuadrados.Add(
                                                                                    new DocumentoDescuadradoModel
                                                                                    {
                                                                                        NumeroCuenta =
                                                                                            parametro.cntpuc_numero,
                                                                                        DescripcionParametro =
                                                                                            parametro
                                                                                                .descripcion_parametro,
                                                                                        ValorDebito = 0,
                                                                                        ValorCredito = valorCredito
                                                                                    });
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            valorCredito =
                                                                                valorSinIva * porcentajeReteICA / 1000;
                                                                            calcularCredito += valorCredito;
                                                                            listaDescuadrados.Add(
                                                                                new DocumentoDescuadradoModel
                                                                                {
                                                                                    NumeroCuenta =
                                                                                        parametro.cntpuc_numero,
                                                                                    DescripcionParametro =
                                                                                        parametro.descripcion_parametro,
                                                                                    ValorDebito = 0,
                                                                                    ValorCredito = valorCredito
                                                                                });
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            if (buscarProveedor2.retfuente != null)
                                                            {
                                                                perfiltributario buscarPerfilTributario =
                                                                    context.perfiltributario.FirstOrDefault(x =>
                                                                        x.bodega == codigoBodega &&
                                                                        x.sw == buscarTipoDocRegistro.sw &&
                                                                        x.tipo_regimenid == regimen_proveedor);

                                                                if (buscarPerfilTributario != null)
                                                                {
                                                                    if (buscarProveedor2.retica == "A")
                                                                    {
                                                                        if (!actividadEconomicaValido)
                                                                        {
                                                                            if ((buscarPerfilTributario.baseretica ?? 0) <= valorSinIva
                                                                            )
                                                                            {
                                                                                decimal porcentajeRetIca = (buscarPerfilTributario.pretica ?? 0);
                                                                                valorCredito =
                                                                                    valorSinIva * porcentajeRetIca / 1000;
                                                                                calcularCredito += valorCredito;
                                                                                listaDescuadrados.Add(
                                                                                    new DocumentoDescuadradoModel
                                                                                    {
                                                                                        NumeroCuenta =
                                                                                            parametro.cntpuc_numero,
                                                                                        DescripcionParametro =
                                                                                            parametro.descripcion_parametro,
                                                                                        ValorDebito = 0,
                                                                                        ValorCredito = valorCredito
                                                                                    });
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            valorCredito =
                                                                                valorSinIva * porcentajeReteICA / 1000;
                                                                            calcularCredito += valorCredito;
                                                                            listaDescuadrados.Add(
                                                                                new DocumentoDescuadradoModel
                                                                                {
                                                                                    NumeroCuenta = parametro.cntpuc_numero,
                                                                                    DescripcionParametro =
                                                                                        parametro.descripcion_parametro,
                                                                                    ValorDebito = 0,
                                                                                    ValorCredito = valorCredito
                                                                                });
                                                                        }
                                                                    }
                                                                }
                                                            }


                                                        }

                                                        #endregion

                                                        #region Fletes = 6

                                                        if (parametro.id_nombre_parametro == 6)
                                                        {
                                                            valorDebito = fletes;
                                                            calcularDebito += valorDebito;
                                                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                            {
                                                                NumeroCuenta = parametro.cntpuc_numero,
                                                                DescripcionParametro = parametro.descripcion_parametro,
                                                                ValorDebito = valorDebito,
                                                                ValorCredito = 0
                                                            });
                                                        }

                                                        #endregion

                                                        #region Descuento pie = 7

                                                        if (parametro.id_nombre_parametro == 7)
                                                        {
                                                            valorCredito = fletes * porcentajeIvaFletes / 100;
                                                            calcularCredito += valorCredito;
                                                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                            {
                                                                NumeroCuenta = parametro.cntpuc_numero,
                                                                DescripcionParametro = parametro.descripcion_parametro,
                                                                ValorDebito = 0,
                                                                ValorCredito = valorCredito
                                                            });
                                                        }

                                                        #endregion

                                                        #region Inventario Débito = 9

                                                        if (parametro.id_nombre_parametro == 9)
                                                        {
                                                            valorDebito = valorSinIva;
                                                            calcularDebito += valorDebito;
                                                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                            {
                                                                NumeroCuenta = parametro.cntpuc_numero,
                                                                DescripcionParametro = parametro.descripcion_parametro,
                                                                ValorDebito = valorDebito,
                                                                ValorCredito = 0
                                                            });
                                                        }

                                                        #endregion

                                                        #region Iva Fletes = 14

                                                        if (parametro.id_nombre_parametro == 14)
                                                        {
                                                            valorDebito = descuentoPie;
                                                            calcularDebito += valorDebito;
                                                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                            {
                                                                NumeroCuenta = parametro.cntpuc_numero,
                                                                DescripcionParametro = parametro.descripcion_parametro,
                                                                ValorDebito = valorDebito,
                                                                ValorCredito = 0
                                                            });
                                                        }

                                                        #endregion

                                                        #region anticipos = 24

                                                        if (parametro.id_nombre_parametro == 24)
                                                        {
                                                            #region Perfil tributario
                                                            perfiltributario buscarPerfilTributario =
                                                                   context.perfiltributario.FirstOrDefault(x =>
                                                                       x.bodega == codigoBodega &&
                                                                       x.sw == buscarTipoDocRegistro.sw &&
                                                                       x.tipo_regimenid == regimen_proveedor);
                                                            if (buscarPerfilTributario != null)
                                                            {

                                                            }

                                                            decimal restarIcaIvaRetencion = 0;
                                                            //resta de retefuente
                                                            if (string.IsNullOrWhiteSpace(buscarProveedor2.retfuente))
                                                            {
                                                                if (buscarPerfilTributario != null)
                                                                {
                                                                    if (buscarPerfilTributario.retfuente == "A")
                                                                    {
                                                                        if ((buscarPerfilTributario.baseretfuente ?? 0) <=
                                                                            valorSinIva)
                                                                        {
                                                                            decimal porcentajeRetencion =
                                                                                buscarPerfilTributario.pretfuente ?? 0;
                                                                            restarIcaIvaRetencion +=
                                                                                valorSinIva * porcentajeRetencion / 100;
                                                                        }
                                                                    }
                                                                }

                                                            }
                                                            else if (!string.IsNullOrWhiteSpace(buscarProveedor2.retfuente))
                                                            {
                                                                if (buscarPerfilTributario != null)
                                                                {
                                                                    if (buscarProveedor2.retfuente == "A")
                                                                    {
                                                                        if ((buscarPerfilTributario.baseretfuente ?? 0) <=
                                                                            valorSinIva)
                                                                        {
                                                                            decimal porcentajeRetencion =
                                                                                buscarPerfilTributario.pretfuente ?? 0;
                                                                            restarIcaIvaRetencion +=
                                                                                valorSinIva * porcentajeRetencion / 100;
                                                                        }
                                                                    }
                                                                }

                                                            }
                                                            //resta de reteiva
                                                            if (string.IsNullOrWhiteSpace(buscarProveedor2.retiva))
                                                            {
                                                                if (buscarPerfilTributario != null)
                                                                {
                                                                    if (buscarPerfilTributario.retiva == "A")
                                                                    {
                                                                        if ((buscarPerfilTributario.baseretiva ?? 0) <=
                                                                            ValorFactura - valorSinIva)
                                                                        {
                                                                            decimal porcentajeRetIva =
                                                                                buscarPerfilTributario.pretiva ?? 0;
                                                                            restarIcaIvaRetencion +=
                                                                                (ValorFactura - valorSinIva) *
                                                                                porcentajeRetIva / 100;
                                                                        }
                                                                    }
                                                                }

                                                            }
                                                            else if (!string.IsNullOrWhiteSpace(buscarProveedor2.retiva))
                                                            {
                                                                if (buscarPerfilTributario != null)
                                                                {
                                                                    if (buscarProveedor2.retiva == "A")
                                                                    {
                                                                        if ((buscarPerfilTributario.baseretiva ?? 0) <=
                                                                            ValorFactura - valorSinIva)
                                                                        {
                                                                            decimal porcentajeRetIva =
                                                                                buscarPerfilTributario.pretiva ?? 0;
                                                                            restarIcaIvaRetencion +=
                                                                                (ValorFactura - valorSinIva) *
                                                                                porcentajeRetIva / 100;
                                                                        }
                                                                    }
                                                                }

                                                            }

                                                            if (string.IsNullOrWhiteSpace(buscarProveedor2.retica))
                                                            {
                                                                if (buscarPerfilTributario != null)
                                                                {
                                                                    if (buscarPerfilTributario.retica == "A")
                                                                    {
                                                                        if (!actividadEconomicaValido)
                                                                        {
                                                                            if ((buscarPerfilTributario.baseretica ?? 0) <= valorSinIva
                                                                            )
                                                                            {
                                                                                decimal porcentajeRetIca =
                                                                                    buscarPerfilTributario.pretica ?? 0;
                                                                                restarIcaIvaRetencion +=
                                                                                    valorSinIva * porcentajeRetIca / 1000;
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            restarIcaIvaRetencion +=
                                                                                valorSinIva * porcentajeReteICA / 1000;
                                                                        }
                                                                    }
                                                                }

                                                            }
                                                            else if (string.IsNullOrWhiteSpace(buscarProveedor2.retica))
                                                            {
                                                                if (buscarPerfilTributario != null)
                                                                {
                                                                    if (buscarProveedor2.retica == "A")
                                                                    {
                                                                        if (!actividadEconomicaValido)
                                                                        {
                                                                            if (buscarTipoDocRegistro.baseica <= valorSinIva
                                                                            )
                                                                            {
                                                                                decimal porcentajeRetIca =
                                                                                    (decimal)buscarTipoDocRegistro.retica;
                                                                                restarIcaIvaRetencion +=
                                                                                    valorSinIva * porcentajeRetIca / 1000;
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            restarIcaIvaRetencion +=
                                                                                valorSinIva * porcentajeReteICA / 1000;
                                                                        }
                                                                    }
                                                                }

                                                            }

                                                            #endregion

                                                            valorCredito = ValorFactura - restarIcaIvaRetencion;
                                                            calcularCredito += valorCredito;
                                                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                            {
                                                                NumeroCuenta = parametro.cntpuc_numero,
                                                                DescripcionParametro = parametro.descripcion_parametro,
                                                                ValorDebito = 0,
                                                                ValorCredito = valorCredito
                                                            });
                                                        }

                                                        #endregion
                                                    }
                                                }

                                                if (calcularCredito != calcularDebito)
                                                {
                                                    dbTran.Rollback();
                                                    TempData["mensajeError"] =
                                                        "El documento no tiene los movimientos calculados correctamente, error linea "
                                                        + (lineaError - 1) + ", plan mayor de error es " + planMayor +
                                                        ", el perfil contable usado es " + lista.ListaCuentasNecesarias
                                                            .FirstOrDefault().descripcion
                                                        + " codigo " + lista.ListaCuentasNecesarias.FirstOrDefault().codigo;
                                                    TempData["documento_descuadrado"] =
                                                        "El documento no tiene los movimientos calculados correctamente";
                                                    ViewBag.documentoDescuadrado = listaDescuadrados;
                                                    ViewBag.calculoDebito = calcularDebito;
                                                    ViewBag.calculoCredito = calcularCredito;

                                                    icb_sysparameter buscarParametroSw =
                                                        context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P23");
                                                    string swParametro = buscarParametroSw != null
                                                        ? buscarParametroSw.syspar_value
                                                        : "1";
                                                    int idTipoSw = Convert.ToInt32(swParametro);
                                                    ViewBag.doc_registros = context.tp_doc_registros
                                                        .Where(x => x.sw == idTipoSw).OrderBy(x => x.tpdoc_nombre);
                                                    var provedores = (from pro in context.tercero_proveedor
                                                                      join ter in context.icb_terceros
                                                                          on pro.tercero_id equals ter.tercero_id
                                                                      select new
                                                                      {
                                                                          idTercero = ter.tercero_id,
                                                                          nombreTErcero = ter.prinom_tercero,
                                                                          apellidosTercero = ter.apellido_tercero,
                                                                          razonSocial = ter.razon_social
                                                                      }).OrderBy(x => x.nombreTErcero);
                                                    List<SelectListItem> items = new List<SelectListItem>();
                                                    foreach (var item in provedores)
                                                    {
                                                        string nombre =
                                                            item.nombreTErcero + " " + item.apellidosTercero + " " +
                                                            item.razonSocial;
                                                        items.Add(new SelectListItem
                                                        { Text = nombre, Value = item.idTercero.ToString() });
                                                    }

                                                    ViewBag.proveedor = items;
                                                    var buscarTipoDocumento =
                                                        (from tipoDocumento in context.tp_doc_registros
                                                         where tipoDocumento.tipo == 1
                                                         select new
                                                         {
                                                             tipoDocumento.tpdoc_id,
                                                             nombre = "(" + tipoDocumento.prefijo + ") " +
                                                                      tipoDocumento.tpdoc_nombre,
                                                             tipoDocumento.tipo
                                                         }).ToList();
                                                    ViewBag.tipo_documentoFiltro = new SelectList(buscarTipoDocumento,
                                                        "tpdoc_id", "nombre");
                                                    ViewBag.condicion_pago = context.fpago_tercero;
                                                    BuscarFavoritos(menu);
                                                    return View();
                                                }
                                            }
                                            else
                                            {
                                                dbTran.Rollback();
                                                // Significa que no hay perfil contable asignado para la bodega y el tipo de documento seleccionado
                                                TempData["mensajeError"] =
                                                    "No existe un perfil contable asignado para la bodega y tipo de documento seleccionados";
                                                return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
                                            }

                                            // Finalizacion de la validacion de los debitos y creditos
                                        }
                                    }
                                }
                                else
                                {
                                    string numpedido_vh1 = linea.Substring(0, linea.IndexOf("MAESTRO") - 1);
                                    if (!string.IsNullOrWhiteSpace(numpedido_vh1))
                                    {
                                        bool convertir = long.TryParse(numpedido_vh1, out numpedido_vh);
                                        if (convertir == false)
                                        {
                                            TempData["mensajeError"] = "No se ha procesado un número de pedido válido";
                                            return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
                                        }
                                    }


                                    encabezado = false;
                                }
                            } // Aqui termina de leer el archivo plano

                            #endregion

                            // El proveedor esta arriba, llega por request tambien
                            //var idTipoDocumento = 0;
                            int idCondicionPago = 0;
                            //var numeroCargue = 0;
                            int nitPago = 0;
                            try
                            {
                                nitPago = Convert.ToInt32(Request["txtNitPago"]);
                                // El tipo de documento llega por request tambien, esta arriba
                                //idCondicionPago = Convert.ToInt32(Request["selectCondicionPago"]);
                                idCondicionPago = buscarProveedor.fpago_id ?? 1;
                                // numeroCargue = Convert.ToInt32(Request["txtNumeroCargue"]);
                                //------------------------------------------------------------------------------------------
                                // Si llega hasta aqui significa que el archivo se leyo correctamente
                                //------------------------------------------------------------------------------------------
                                long numeroConsecutivo = 0;
                                ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
                                icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
                                numeroConsecutivoAux = gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro,
                                    idTipoDocumento, codigoBodega);
                                //var numeroCargueAux = context.icb_doc_consecutivos.OrderByDescending(x => x.doccons_ano).FirstOrDefault(x => x.doccons_idtpdoc == idTipoDocumento && x.doccons_bodega == codigoBodega);

                                grupoconsecutivos grupoConsecutivo = context.grupoconsecutivos.FirstOrDefault(x =>
                                    x.documento_id == idTipoDocumento && x.bodega_id == codigoBodega);
                                if (numeroConsecutivoAux != null)
                                {
                                    numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                                }
                                else
                                {
                                    dbTran.Rollback();
                                    TempData["mensajeError"] =
                                        "No existe un numero consecutivo asignado para este tipo de documento y bodega";
                                    return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
                                }

                                icb_arch_facturacion archivoFacturacion = new icb_arch_facturacion
                                {
                                    arch_fac_nombre = txtfile.FileName,
                                    arch_fac_fecha = DateTime.Now,
                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                    items = itemsCorrectos + itemsFallidos
                                };
                                context.icb_arch_facturacion.Add(archivoFacturacion);
                                int guardarArchFacturacion = context.SaveChanges();

                                #region Significa que el nuevo registro o encabezado de archivo de facturacion se guardo correctamente							

                                if (guardarArchFacturacion > 0)
                                {
                                    #region variables y parametros para el guardado de la informacion del archivo plano

                                    int codigoArchFacturacion = context.icb_arch_facturacion
                                        .OrderByDescending(x => x.arch_fac_id).First().arch_fac_id;
                                    bodega_concesionario sinBodega = context.bodega_concesionario.FirstOrDefault(x => x.bodccs_cod == "000");

                                    // ---------------------------------------------------------------------------------------------------
                                    // Parametros que estaban quemados y ahora se toman de la tabla icb_sys_parameter
                                    // ---------------------------------------------------------------------------------------------------
                                    icb_sysparameter buscarParametroStatus =
                                        context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P17");
                                    string statusParametro = buscarParametroStatus != null
                                        ? buscarParametroStatus.syspar_value
                                        : "0";
                                    //var buscarParametroTpVh = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P3");
                                    //var tpHvParametro = buscarParametroTpVh != null ? buscarParametroTpVh.syspar_value : "4";
                                    icb_sysparameter buscarParametroTpEvento =
                                        context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P19");
                                    string tpEventoParametro = buscarParametroTpEvento != null
                                        ? buscarParametroTpEvento.syspar_value
                                        : "11";
                                    icb_sysparameter buscarParametroMarcaVh =
                                        context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P5");
                                    string marcaVhParametro = buscarParametroMarcaVh != null
                                        ? buscarParametroMarcaVh.syspar_value
                                        : "1";
                                    //var buscarParametroEventoFact = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P4");
                                    //var eventoFacturacionParametro = buscarParametroEventoFact != null ? buscarParametroEventoFact.syspar_value : "1";
                                    icb_sysparameter buscarParametroTpEventoMan =
                                        context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P64");
                                    string tpEventoParametro2 = buscarParametroTpEventoMan != null
                                        ? buscarParametroTpEventoMan.syspar_value
                                        : "20";
                                    int idEventoManifiesto2 = Convert.ToInt32(tpEventoParametro2);

                                    icb_tpeventos buscoideventomanifiesto = context.icb_tpeventos.Where(d => d.codigoevento == idEventoManifiesto2).FirstOrDefault();

                                    int idEventoManifiesto = buscoideventomanifiesto != null ? buscoideventomanifiesto.tpevento_id : Convert.ToInt32(tpEventoParametro2);

                                    int idEventoFacturacion = Convert.ToInt32(tpEventoParametro);

                                    //var tpvh_idAux = Convert.ToInt32(tpHvParametro);
                                    //var id_eventoAux = Convert.ToInt32(tpEventoParametro);
                                    int marcvh_idAux = Convert.ToInt32(marcaVhParametro);

                                    #endregion

                                    foreach (CrearVehiculoExcel vehiculo in listaVehiculos)
                                    {
                                        #region variables datos Vehiculo a crear y/o actualizar

                                        short anoFacturacion = (short)vehiculo.fecfact_fabrica.Year;
                                        short mesFacturacion = (short)vehiculo.fecfact_fabrica.Month;
                                        icb_referencia buscarReferencia =
                                            context.icb_referencia.FirstOrDefault(x => x.ref_codigo == vehiculo.plan_mayor);
                                        referencias_inven buscarReferenciaInven = context.referencias_inven.FirstOrDefault(x =>
                                            x.bodega == codigoBodega && x.codigo == vehiculo.plan_mayor &&
                                            x.ano == anoFacturacion && x.mes == mesFacturacion);
                                        int diasVencimiento =
                                            context.fpago_tercero.FirstOrDefault(x => x.fpago_id == idCondicionPago)
                                                .dvencimiento ?? 0;

                                        #endregion

                                        #region Validacion de la referencia y/o del vehiculo

                                        if (vehiculo.vehiculo_actualizar)
                                        {
                                            icb_vehiculo buscarVehiculo =
                                                context.icb_vehiculo.FirstOrDefault(
                                                    x => x.plan_mayor == vehiculo.plan_mayor);
                                            //context.Entry(buscarVehiculo).State = EntityState.Modified;
                                            //context.SaveChanges();

                                            if (buscarReferencia != null && buscarReferenciaInven != null)
                                            {
                                                buscarReferencia.modulo = "V";
                                                context.Entry(buscarReferencia).State = EntityState.Modified;
                                                //context.Entry(buscarReferenciaInven).State = EntityState.Modified;
                                                //context.SaveChanges();
                                            }
                                            else
                                            {
                                                modelo_vehiculo buscarModelo =
                                                    context.modelo_vehiculo.FirstOrDefault(x =>
                                                        x.modvh_codigo == vehiculo.modvh_id);
                                                icb_referencia referenciaNueva = new icb_referencia
                                                {
                                                    ref_codigo = vehiculo.plan_mayor,
                                                    ref_descripcion = buscarModelo != null ? buscarModelo.modvh_nombre : "",
                                                    ref_valor_unitario = vehiculo.costosiniva_vh,
                                                    ref_valor_total = vehiculo.costototal_vh,
                                                    por_iva = (float)vehiculo.porcentajeIva,
                                                    manejo_inv = true,
                                                    ref_usuario_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                                    ref_fecha_creacion = DateTime.Now,
                                                    modulo = "V",
                                                    fec_ultima_entrada = DateTime.Now
                                                };
                                                context.icb_referencia.Add(referenciaNueva);

                                                short anoFacturacion2 = (short)DateTime.Now.Year;
                                                short mesFacturacion2 = (short)DateTime.Now.Month;

                                                referencias_inven referenciaInvenNueva = new referencias_inven
                                                {
                                                    bodega = codigoBodega,
                                                    codigo = vehiculo.plan_mayor,
                                                    ano = anoFacturacion2,
                                                    mes = mesFacturacion2,
                                                    can_ini = 0,
                                                    can_ent = 1,
                                                    can_sal = 0,
                                                    cos_ent = vehiculo.costosiniva_vh,
                                                    can_com = 1,
                                                    cos_com = vehiculo.costosiniva_vh,
                                                    modulo = "V"
                                                };
                                                context.referencias_inven.Add(referenciaInvenNueva);
                                                if (anoFacturacion2 != vehiculo.fecfact_fabrica.Year || mesFacturacion2 != (short)DateTime.Now.Month)
                                                {
                                                    referencias_inven referenciaInvenNueva2 = new referencias_inven
                                                    {
                                                        bodega = codigoBodega,
                                                        codigo = vehiculo.plan_mayor,
                                                        ano = anoFacturacion2,
                                                        mes = mesFacturacion2,
                                                        can_ini = 0,
                                                        can_ent = 1,
                                                        can_sal = 0,
                                                        cos_ent = vehiculo.costosiniva_vh,
                                                        can_com = 1,
                                                        cos_com = vehiculo.costosiniva_vh,
                                                        modulo = "V"
                                                    };
                                                    context.referencias_inven.Add(referenciaInvenNueva2);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            icb_vehiculo vehiculoNuevo = new icb_vehiculo
                                            {
                                                fecfact_fabrica = vehiculo.fecfact_fabrica,
                                                plan_mayor = vehiculo.plan_mayor,
                                                anio_vh = vehiculo.anio_vh,
                                                kmat_zvsk = vehiculo.kmat,
                                                modvh_id = vehiculo.modvh_id,
                                                colvh_id = vehiculo.colvh_id,
                                                vin = vehiculo.vin,
                                                nummot_vh = vehiculo.nummot_vh,
                                                costototal_vh = vehiculo.costototal_vh,
                                                costosiniva_vh = vehiculo.costosiniva_vh,
                                                arch_fact_id = codigoArchFacturacion,
                                                icbvhuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                                icbvhfec_creacion = DateTime.Now,
                                                id_bod = codigoBodega,
                                                //tpvh_id = tpvh_idAux,
                                                id_evento = idEventoFacturacion,
                                                marcvh_id = marcvh_idAux,
                                                icbvh_estado = true,
                                                icbvh_estatus = statusParametro,
                                                proveedor_id = idProveedor,
                                                bodegaGM = vehiculo.bodegaGM,
                                                codigo_pago = vehiculo.codigoPago,
                                                Notas = vehiculo.notas,
                                                flota = vehiculo.flota,
                                                nuevo = true,
                                                iva_vh = vehiculo.porcentaje_iva,
                                                nummanf_vh = vehiculo.numManifiesto,
                                                fecentman_vh = vehiculo.fechaManifiesto,
                                                ciumanf_vh =
                                                    3 //el dia 26/03/2019 en el ticket -> Caso Nro.  3286 - 806 <- se solicito el cambio y se dejo quemado el codigo 3 que corresponde al id de la ciudad de bogota en la tabla nom_ciudad
                                            };
                                            if (vehiculo.numpedido_vh > 0)
                                            {
                                                vehiculoNuevo.numpedido_vh = vehiculo.numpedido_vh;
                                            }

                                            context.icb_vehiculo.Add(vehiculoNuevo);
                                            if (buscarReferencia == null)
                                            {
                                                modelo_vehiculo buscarModelo =
                                                    context.modelo_vehiculo.FirstOrDefault(x =>
                                                        x.modvh_codigo == vehiculo.modvh_id);
                                                icb_referencia referenciaNueva = new icb_referencia
                                                {
                                                    ref_codigo = vehiculo.plan_mayor,
                                                    ref_descripcion = buscarModelo != null ? buscarModelo.modvh_nombre : "",
                                                    ref_valor_unitario = vehiculo.costosiniva_vh,
                                                    ref_valor_total = vehiculo.costototal_vh,
                                                    por_iva = (float)vehiculo.porcentajeIva,
                                                    manejo_inv = true,
                                                    ref_usuario_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                                    ref_fecha_creacion = DateTime.Now,
                                                    modulo = "V",
                                                    fec_ultima_entrada = DateTime.Now
                                                };
                                                context.icb_referencia.Add(referenciaNueva);
                                            }
                                            else
                                            {
                                                dbTran.Rollback();
                                                TempData["mensajeError"] =
                                                    "El plan mayor " + vehiculo.plan_mayor +
                                                    " ya existia con anterioridad en la tabla icb_referencia";
                                                return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
                                            }

                                            if (buscarReferenciaInven == null)
                                            {
                                                referencias_inven referenciaInvenNueva = new referencias_inven
                                                {
                                                    bodega = codigoBodega,
                                                    codigo = vehiculo.plan_mayor,
                                                    ano = (short)vehiculo.fecfact_fabrica.Year,
                                                    mes = (short)vehiculo.fecfact_fabrica.Month,
                                                    can_ini = 0,
                                                    can_ent = 1,
                                                    can_sal = 0,
                                                    cos_ent = vehiculo.costosiniva_vh,
                                                    can_com = 1,
                                                    cos_com = vehiculo.costosiniva_vh,
                                                    modulo = "V"
                                                };
                                                context.referencias_inven.Add(referenciaInvenNueva);
                                                //si el vehiculo se está registrando en un mes menor al actual o del año pasado, verifico y creo tantas veces sea necesario en el inventario hasta el año y mes actual
                                                if ((short)vehiculo.fecfact_fabrica.Year < DateTime.Now.Year || ((short)vehiculo.fecfact_fabrica.Year == DateTime.Now.Year && (short)vehiculo.fecfact_fabrica.Month <= DateTime.Now.Month))
                                                {
                                                    //veo año y mes de fabricacion y le sumo un mes
                                                    var listo = 0;
                                                    var fechaguar = vehiculo.fecfact_fabrica;

                                                    while (listo == 0)
                                                    {
                                                        fechaguar = fechaguar.AddMonths(1);
                                                        if ((short)fechaguar.Year < DateTime.Now.Year || ((short)fechaguar.Year == DateTime.Now.Year && (short)fechaguar.Month <= DateTime.Now.Month))
                                                        {
                                                            listo = 0;
                                                        }
                                                        else if ((short)fechaguar.Year == DateTime.Now.Year && (short)fechaguar.Month <= DateTime.Now.Month)
                                                        {
                                                            listo = 1;
                                                        }
                                                        referencias_inven referenciaInvenNueva2 = new referencias_inven
                                                        {
                                                            bodega = codigoBodega,
                                                            codigo = vehiculo.plan_mayor,
                                                            ano = (short)fechaguar.Year,
                                                            mes = (short)fechaguar.Month,
                                                            can_ini = 1,
                                                            can_ent = 0,
                                                            can_sal = 0,
                                                            cos_ent = 0,
                                                            can_com = 0,
                                                            cos_ini = vehiculo.costosiniva_vh,
                                                            cos_com = 0,
                                                            modulo = "V"
                                                        };
                                                        context.referencias_inven.Add(referenciaInvenNueva2);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                dbTran.Rollback();
                                                TempData["mensajeError"] =
                                                    "El plan mayor " + vehiculo.plan_mayor +
                                                    "  ya existia con anterioridad en la tabla referencias_inven";
                                                return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
                                            }
                                        }

                                        int guardarVehiculo = context.SaveChanges();

                                        #endregion

                                        #region validamos el tipo de compra y asignamos el NIT correspondiente

                                        int nitFinal = 0;
                                        if (vehiculo.codigoPago == "FG03" || vehiculo.codigoPago == "WNA1")
                                        {
                                            nitFinal = idProveedor;
                                        }
                                        else if (vehiculo.codigoPago == "FG01")
                                        {
                                            nitFinal = Convert.ToInt32(Request["txtNitPago"]);
                                        }

                                        #endregion

                                        #region Encabezado documento

                                        encab_documento encabezadoNuevo = new encab_documento
                                        {
                                            tipo = idTipoDocumento,
                                            nit = nitFinal,
                                            nit_pago = nitPago,
                                            fecha = vehiculo.fecfact_fabrica,
                                            fpago_id = idCondicionPago,
                                            vencimiento = vehiculo.fecfact_fabrica.AddDays(diasVencimiento),
                                            valor_total = vehiculo.costototal_vh,
                                            iva = vehiculo.costototal_vh - vehiculo.costosiniva_vh,
                                            bodega = codigoBodega,
                                            estado = true,
                                            documento = vehiculo.plan_mayor,
                                            valor_mercancia = vehiculo.costosiniva_vh,
                                            pedido = vehiculo.numpedido_vh,
                                            numero = (int)numeroConsecutivo,
                                            fec_creacion = DateTime.Now,
                                            perfilcontable = buscarTipoCompra.idperfilcontable,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            costo = vehiculo.costosiniva_vh
                                        };

                                        #region Perfil tributario encab_documento

                                        if (buscarProveedor2.retfuente != null)
                                        {
                                            if (buscarTipoDocRegistro != null)
                                            {
                                                perfiltributario buscarPerfilTributario =
                                                                    context.perfiltributario.FirstOrDefault(x =>
                                                                        x.bodega == codigoBodega &&
                                                                        x.sw == buscarTipoDocRegistro.sw &&
                                                                        x.tipo_regimenid == regimen_proveedor);
                                                if (buscarProveedor2.retfuente == "A")
                                                {
                                                    if ((buscarPerfilTributario.baseretfuente ?? 0) <= vehiculo.costosiniva_vh)
                                                    {
                                                        decimal porcentajeRetencion = buscarPerfilTributario.pretfuente ?? 0;
                                                        encabezadoNuevo.retencion =
                                                            vehiculo.costosiniva_vh * porcentajeRetencion / 100;
                                                        encabezadoNuevo.porcen_retencion = (float)porcentajeRetencion;
                                                    }
                                                }

                                                if (buscarProveedor2.retiva == "A")
                                                {
                                                    if ((buscarPerfilTributario.baseretiva ?? 0) <= vehiculo.costosiniva_vh)
                                                    {
                                                        decimal porcentajeRetIva = buscarPerfilTributario.pretiva ?? 0;
                                                        encabezadoNuevo.retencion_iva =
                                                            vehiculo.iva_vh * porcentajeRetIva / 100;
                                                        encabezadoNuevo.porcen_reteiva = (float)porcentajeRetIva;
                                                    }
                                                }

                                                if (buscarProveedor2.retica == "A")
                                                {
                                                    if (!actividadEconomicaValido)
                                                    {
                                                        if ((buscarPerfilTributario.baseretica ?? 0) <= vehiculo.costosiniva_vh)
                                                        {
                                                            decimal porcentajeRetIca = buscarPerfilTributario.pretica ?? 0;
                                                            encabezadoNuevo.retencion_ica =
                                                                vehiculo.costosiniva_vh * porcentajeRetIca / 1000;
                                                            encabezadoNuevo.porcen_retica = (float)porcentajeRetIca;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        encabezadoNuevo.retencion_ica =
                                                            vehiculo.costosiniva_vh * porcentajeReteICA / 1000;
                                                        encabezadoNuevo.porcen_retica = (float)porcentajeReteICA;
                                                    }
                                                }
                                            }
                                        }

                                        if (buscarProveedor2.retfuente == null)
                                        {
                                            perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                                                x.bodega == codigoBodega && x.sw == buscarTipoDocRegistro.sw &&
                                                x.tipo_regimenid == regimen_proveedor);
                                            if (buscarPerfilTributario != null)
                                            {
                                                if (buscarTipoDocRegistro != null)
                                                {
                                                    if (buscarPerfilTributario.retfuente == "A")
                                                    {
                                                        if ((buscarPerfilTributario.baseretfuente ?? 0) <= vehiculo.costosiniva_vh)
                                                        {
                                                            decimal porcentajeRetencion =
                                                                buscarPerfilTributario.pretfuente ?? 0;
                                                            encabezadoNuevo.retencion =
                                                                vehiculo.costosiniva_vh * porcentajeRetencion / 100;
                                                            encabezadoNuevo.porcen_retencion = (float)porcentajeRetencion;
                                                        }
                                                    }

                                                    if (buscarPerfilTributario.retiva == "A")
                                                    {
                                                        if ((buscarPerfilTributario.baseretiva ?? 0) <= vehiculo.costosiniva_vh)
                                                        {
                                                            decimal porcentajeRetIva = buscarPerfilTributario.pretiva ?? 0;
                                                            encabezadoNuevo.retencion_iva =
                                                                vehiculo.iva_vh * porcentajeRetIva / 100;
                                                            encabezadoNuevo.porcen_reteiva = (float)porcentajeRetIva;
                                                        }
                                                    }

                                                    if (buscarPerfilTributario.retica == "A")
                                                    {
                                                        if (!actividadEconomicaValido)
                                                        {
                                                            if (buscarTipoDocRegistro.baseica <= vehiculo.costosiniva_vh)
                                                            {
                                                                decimal porcentajeRetIca =
                                                                    buscarPerfilTributario.pretica ?? 0;
                                                                encabezadoNuevo.retencion_ica =
                                                                    vehiculo.costosiniva_vh * porcentajeRetIca / 1000;
                                                                encabezadoNuevo.porcen_retica = (float)porcentajeRetIca;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            encabezadoNuevo.retencion_ica =
                                                                vehiculo.costosiniva_vh * porcentajeReteICA / 1000;
                                                            encabezadoNuevo.porcen_retica = (float)porcentajeReteICA;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        #endregion

                                        context.encab_documento.Add(encabezadoNuevo);
                                        int guardarEncabDoc = context.SaveChanges();
                                        encab_documento buscarUltimoEncabezado = context.encab_documento
                                            .OrderByDescending(x => x.idencabezado).FirstOrDefault();

                                        #endregion

                                        //var buscaAnioModelo = context.anio_modelo.FirstOrDefault(x => x.codigo_modelo == vehiculo.modvh_id);
                                        //var valorIva = buscaAnioModelo != null ? buscaAnioModelo.porcentaje_iva : 0;
                                        //var valorImpuestoConsumo = buscaAnioModelo != null ? buscaAnioModelo.impuesto_consumo : 0;

                                        #region Lineas Documento

                                        context.lineas_documento.Add(new lineas_documento
                                        {
                                            codigo = vehiculo.plan_mayor,
                                            fec_creacion = DateTime.Now,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            nit = nitFinal,
                                            cantidad = 1,
                                            //valor_unitario = vehiculo.costototal_vh,
                                            valor_unitario = vehiculo.costosiniva_vh,
                                            bodega = codigoBodega,
                                            porcentaje_iva = (float)vehiculo.porcentajeIva,
                                            seq = 1,
                                            //impoconsumo = (float)vehiculo.impuesto_consumo,
                                            estado = true,
                                            fec = vehiculo.fecfact_fabrica,
                                            costo_unitario = vehiculo.costosiniva_vh,
                                            id_encabezado = buscarUltimoEncabezado != null
                                                ? buscarUltimoEncabezado.idencabezado
                                                : 0
                                        });

                                        #endregion

                                        #region Mov contable y cuentas valores									

                                        int idConceptoCompra = 0;
                                        foreach (vconceptocompra item in tipoCompraVehiculos)
                                        {
                                            if (item.codigo.ToUpper().Contains(vehiculo.codigoPago))
                                            {
                                                idConceptoCompra = item.id;
                                            }
                                        }
                                        //var buscarTipoCompra = context.vtipo_compra_vehiculos.FirstOrDefault(x => x.idtipocomprav == idConceptoCompra && x.idbodega == codigoBodega && x.iddoc == idTipoDocumento);
                                        // Significa que hay un perfil contable encontrado
                                        if (buscarTipoCompra != null)
                                        {
                                            ListaCuentas lista = new ListaCuentas();
                                            centro_costo centroValorCero =
                                                context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                                            int idCentroCero = centroValorCero != null
                                                ? Convert.ToInt32(centroValorCero.centcst_id)
                                                : 0;
                                            icb_terceros terceroValorCero =
                                                context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
                                            int idTerceroCero = centroValorCero != null
                                                ? Convert.ToInt32(terceroValorCero.tercero_id)
                                                : 0;
                                            if (idTipoDocumento == 1)
                                            {
                                                lista.ListaCuentasNecesarias = (from p in context.perfiltipocompra
                                                                                join c in context.vconceptocompra
                                                                                    on p.id equals c.id
                                                                                join ct in context.perfilcuentastipocompra
                                                                                    on p.id equals ct.idTipoCompra
                                                                                join cb in context.perfilbodegastipocompra
                                                                                    on p.id equals cb.idTipoCompra
                                                                                join nombreParametro in context.paramcontablenombres
                                                                                    on ct.idParametro equals nombreParametro.id
                                                                                join cuenta in context.cuenta_puc
                                                                                    on ct.cuenta equals cuenta.cntpuc_id
                                                                                where c.codigo == vehiculo.codigoPago && cb.idBodega == codigoBodega
                                                                                select new cuentasNecesarias
                                                                                {
                                                                                    id = ct.id,
                                                                                    id_nombre_parametro = ct.idParametro,
                                                                                    cuenta = ct.cuenta,
                                                                                    centro = ct.centro,
                                                                                    descripcion_parametro = nombreParametro.descripcion_parametro,
                                                                                    cntpuc_numero = cuenta.cntpuc_numero
                                                                                }).ToList();
                                            }
                                            else
                                            {
                                                lista.ListaCuentasNecesarias =
                                                    (from perfil in context.perfil_cuentas_documento
                                                     join nombreParametro in context.paramcontablenombres
                                                         on perfil.id_nombre_parametro equals nombreParametro.id
                                                     join cuenta in context.cuenta_puc
                                                         on perfil.cuenta equals cuenta.cntpuc_id
                                                     where perfil.id_perfil == buscarTipoCompra.idperfilcontable
                                                     select new cuentasNecesarias
                                                     {
                                                         id = perfil.id,
                                                         id_nombre_parametro = perfil.id_nombre_parametro,
                                                         cuenta = perfil.cuenta,
                                                         centro = perfil.centro,
                                                         idperfil = perfil.id_perfil,
                                                         descripcion_parametro = nombreParametro.descripcion_parametro,
                                                         cntpuc_numero = cuenta.cntpuc_numero
                                                     }).ToList();
                                            }

                                            int secuencia = 1;
                                            decimal valorCredito = 0;
                                            decimal valorDebito = 0;
                                            foreach (cuentasNecesarias parametro in lista.ListaCuentasNecesarias)
                                            {
                                                int tipoParametro = parametro.id_nombre_parametro;
                                                string CreditoDebito = "";
                                                cuenta_puc buscarCuenta =
                                                    context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
                                                if (buscarCuenta != null)
                                                {
                                                    if (parametro.id_nombre_parametro ==
                                                        1 || parametro.id_nombre_parametro == 2
                                                          || parametro.id_nombre_parametro == 3 ||
                                                          parametro.id_nombre_parametro == 4
                                                          || parametro.id_nombre_parametro == 5 ||
                                                          parametro.id_nombre_parametro == 6
                                                          || parametro.id_nombre_parametro == 7 ||
                                                          parametro.id_nombre_parametro == 9
                                                          || parametro.id_nombre_parametro == 14 ||
                                                          parametro.id_nombre_parametro == 24)
                                                    {
                                                        mov_contable movimiento = new mov_contable
                                                        {
                                                            basecontable = 0,
                                                            creditoniif = 0,
                                                            debitoniif = 0,
                                                            id_encab = buscarUltimoEncabezado != null
                                                            ? buscarUltimoEncabezado.idencabezado
                                                            : 0,
                                                            fec_creacion = DateTime.Now,
                                                            userid_creacion =
                                                            Convert.ToInt32(Session["user_usuarioid"]),
                                                            idparametronombre = tipoParametro,
                                                            cuenta = parametro.cuenta,
                                                            centro = parametro.centro,
                                                            fec = buscarUltimoEncabezado.fecha,
                                                            seq = secuencia
                                                        };

                                                        #region Cuentas Por Pagar = 1

                                                        if (parametro.id_nombre_parametro == 1)
                                                        {
                                                            decimal restarIcaIvaRetencion = 0;
                                                            if (buscarProveedor2.retfuente != null)
                                                            {
                                                                perfiltributario buscarPerfilTributario =
                                                                   context.perfiltributario.FirstOrDefault(x =>
                                                                       x.bodega == codigoBodega &&
                                                                       x.sw == buscarTipoDocRegistro.sw &&
                                                                       x.tipo_regimenid == regimen_proveedor);
                                                                if (buscarTipoDocRegistro != null)
                                                                {
                                                                    if (buscarProveedor2.retfuente == "A")
                                                                    {
                                                                        if ((buscarPerfilTributario.baseretfuente ?? 0) <=
                                                                            vehiculo.costosiniva_vh)
                                                                        {
                                                                            decimal porcentajeRetencion =
                                                                                buscarPerfilTributario.pretfuente ?? 0;
                                                                            restarIcaIvaRetencion +=
                                                                                vehiculo.costosiniva_vh *
                                                                                porcentajeRetencion / 100;
                                                                        }
                                                                    }

                                                                    if (buscarProveedor2.retiva == "A")
                                                                    {
                                                                        if ((buscarPerfilTributario.baseretiva ?? 0) <= vehiculo.iva_vh
                                                                        )
                                                                        {
                                                                            decimal porcentajeRetIva =
                                                                                buscarPerfilTributario.pretiva ?? 0;
                                                                            restarIcaIvaRetencion +=
                                                                                vehiculo.iva_vh * porcentajeRetIva / 100;
                                                                        }
                                                                    }

                                                                    if (buscarProveedor2.retica == "A")
                                                                    {
                                                                        if (!actividadEconomicaValido)
                                                                        {
                                                                            if ((buscarPerfilTributario.baseretica ?? 0) <=
                                                                                vehiculo.costosiniva_vh)
                                                                            {
                                                                                decimal porcentajeRetIca =
                                                                                    buscarPerfilTributario.pretica ?? 0;
                                                                                restarIcaIvaRetencion +=
                                                                                    vehiculo.costosiniva_vh *
                                                                                    porcentajeRetIca / 1000;
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            restarIcaIvaRetencion +=
                                                                                vehiculo.costosiniva_vh *
                                                                                porcentajeReteICA / 1000;
                                                                        }
                                                                    }
                                                                }

                                                                valorCredito =
                                                                    vehiculo.costosiniva_vh + vehiculo.iva_vh -
                                                                    restarIcaIvaRetencion;
                                                                CreditoDebito = "Credito";
                                                                //valorDebito = ((vehiculo.costosiniva_vh + vehiculo.iva_vh) - restarIcaIvaRetencion);
                                                                //valorCredito = ValorFactura - restarIcaIvaRetencion;
                                                                //valorDebito = ValorFactura - restarIcaIvaRetencion;
                                                            }

                                                        }

                                                        if (parametro.id_nombre_parametro == 1)
                                                        {
                                                            decimal restarIcaIvaRetencion = 0;
                                                            if (!string.IsNullOrWhiteSpace(buscarProveedor2.retfuente))
                                                            {
                                                                perfiltributario buscarPerfilTributario =
                                                                    context.perfiltributario.FirstOrDefault(x =>
                                                                        x.bodega == codigoBodega &&
                                                                        x.sw == buscarTipoDocRegistro.sw &&
                                                                        x.tipo_regimenid == regimen_proveedor);
                                                                if (buscarPerfilTributario != null)
                                                                {
                                                                    if (buscarTipoDocRegistro != null)
                                                                    {
                                                                        if (buscarPerfilTributario.retfuente == "A")
                                                                        {
                                                                            if ((buscarPerfilTributario.baseretfuente ?? 0) <=
                                                                                vehiculo.costosiniva_vh)
                                                                            {
                                                                                decimal porcentajeRetencion =
                                                                                    buscarPerfilTributario.pretfuente ?? 0;
                                                                                restarIcaIvaRetencion +=
                                                                                    vehiculo.costosiniva_vh *
                                                                                    porcentajeRetencion / 100;
                                                                            }
                                                                        }

                                                                        if (buscarPerfilTributario.retiva == "A")
                                                                        {
                                                                            if ((buscarPerfilTributario.baseretiva ?? 0) <=
                                                                                vehiculo.iva_vh)
                                                                            {
                                                                                decimal porcentajeRetIva =
                                                                                    buscarPerfilTributario.pretiva ?? 0;
                                                                                restarIcaIvaRetencion +=
                                                                                    vehiculo.iva_vh * porcentajeRetIva /
                                                                                    100;
                                                                            }
                                                                        }

                                                                        if (buscarPerfilTributario.retica == "A")
                                                                        {
                                                                            if (!actividadEconomicaValido)
                                                                            {
                                                                                if ((buscarPerfilTributario.baseretica ?? 0) <=
                                                                                    vehiculo.costosiniva_vh)
                                                                                {
                                                                                    decimal porcentajeRetIca =
                                                                                        buscarPerfilTributario.pretica ?? 0;
                                                                                    restarIcaIvaRetencion +=
                                                                                        vehiculo.costosiniva_vh *
                                                                                        porcentajeRetIca / 1000;
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                restarIcaIvaRetencion +=
                                                                                    vehiculo.costosiniva_vh *
                                                                                    porcentajeReteICA / 1000;
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            valorCredito =
                                                                vehiculo.costosiniva_vh + vehiculo.iva_vh -
                                                                restarIcaIvaRetencion;
                                                            CreditoDebito = "Credito";
                                                            //valorDebito = ((vehiculo.costosiniva_vh + vehiculo.iva_vh) - restarIcaIvaRetencion);
                                                            //valorCredito = ValorFactura - restarIcaIvaRetencion;
                                                            //valorDebito = ValorFactura - restarIcaIvaRetencion;
                                                        }

                                                        #endregion

                                                        #region IVA = 2

                                                        if (parametro.id_nombre_parametro == 2)
                                                        {
                                                            //valorCredito = vehiculo.iva_vh;
                                                            valorDebito = vehiculo.iva_vh;
                                                            CreditoDebito = "Debito";
                                                        }

                                                        #endregion

                                                        #region Retencion = 3

                                                        if (parametro.id_nombre_parametro == 3)
                                                        {
                                                            perfiltributario buscarPerfilTributario2 =
                                                                    context.perfiltributario.FirstOrDefault(x =>
                                                                        x.bodega == codigoBodega &&
                                                                        x.sw == buscarTipoDocRegistro.sw &&
                                                                        x.tipo_regimenid == regimen_proveedor);
                                                            if (!string.IsNullOrWhiteSpace(buscarProveedor2.retfuente))
                                                            {
                                                                if (buscarTipoDocRegistro != null)
                                                                {
                                                                    if (buscarProveedor2.retfuente == "A")
                                                                    {
                                                                        if ((buscarPerfilTributario2.baseretfuente ?? 0) <=
                                                                            vehiculo.costosiniva_vh)
                                                                        {
                                                                            decimal porcentajeRetencion =
                                                                                buscarPerfilTributario2.pretfuente ?? 0;
                                                                            valorCredito =
                                                                                vehiculo.costosiniva_vh *
                                                                                porcentajeRetencion / 100;
                                                                            //valorDebito = (vehiculo.costosiniva_vh * porcentajeRetencion) / 100;
                                                                            CreditoDebito = "Credito";
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            if (!string.IsNullOrWhiteSpace(buscarProveedor2.retfuente))
                                                            {
                                                                perfiltributario buscarPerfilTributario =
                                                                    context.perfiltributario.FirstOrDefault(x =>
                                                                        x.bodega == codigoBodega &&
                                                                        x.sw == buscarTipoDocRegistro.sw &&
                                                                        x.tipo_regimenid == regimen_proveedor);
                                                                if (buscarPerfilTributario != null)
                                                                {
                                                                    if (buscarTipoDocRegistro != null)
                                                                    {
                                                                        if (buscarPerfilTributario.retfuente == "A")
                                                                        {
                                                                            if ((buscarPerfilTributario.baseretfuente ?? 0) <=
                                                                                vehiculo.costosiniva_vh)
                                                                            {
                                                                                decimal porcentajeRetencion =
                                                                                    buscarPerfilTributario.pretfuente ?? 0;
                                                                                valorCredito =
                                                                                    vehiculo.costosiniva_vh *
                                                                                    porcentajeRetencion / 100;
                                                                                //valorDebito = (vehiculo.costosiniva_vh * porcentajeRetencion) / 100;
                                                                                CreditoDebito = "Credito";
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        #endregion

                                                        #region Rete IVA = 4

                                                        if (parametro.id_nombre_parametro == 4)
                                                        {
                                                            if (buscarProveedor2 == null || string.IsNullOrWhiteSpace(buscarProveedor2.retiva))
                                                            {
                                                                perfiltributario buscarPerfilTributario =
                                                               context.perfiltributario.FirstOrDefault(x =>
                                                                   x.bodega == codigoBodega &&
                                                                   x.sw == buscarTipoDocRegistro.sw &&
                                                                   x.tipo_regimenid == regimen_proveedor);
                                                                if (buscarPerfilTributario != null)
                                                                {
                                                                    if (buscarProveedor2.retiva == "A")
                                                                    {
                                                                        if ((buscarPerfilTributario.baseretiva ?? 0) <=
                                                                            vehiculo.costosiniva_vh)
                                                                        {
                                                                            decimal porcentajeRetIva =
                                                                                buscarPerfilTributario.pretiva ?? 0;
                                                                            valorCredito =
                                                                                vehiculo.iva_vh * porcentajeRetIva / 100;
                                                                            //valorDebito = (vehiculo.iva_vh * porcentajeRetIva) / 100;
                                                                            CreditoDebito = "Credito";
                                                                        }
                                                                    }
                                                                }
                                                            }


                                                            if (buscarProveedor2 != null)
                                                            {
                                                                perfiltributario buscarPerfilTributario =
                                                                    context.perfiltributario.FirstOrDefault(x =>
                                                                        x.bodega == codigoBodega &&
                                                                        x.sw == buscarTipoDocRegistro.sw &&
                                                                        x.tipo_regimenid == regimen_proveedor);
                                                                if (buscarTipoDocRegistro != null)
                                                                {
                                                                    if (buscarProveedor2.retiva == "A")
                                                                    {
                                                                        if ((buscarPerfilTributario.baseretiva ?? 0) <=
                                                                            vehiculo.costosiniva_vh)
                                                                        {
                                                                            decimal porcentajeRetIva =
                                                                                buscarPerfilTributario.pretiva ?? 0;
                                                                            valorCredito =
                                                                                vehiculo.iva_vh * porcentajeRetIva / 100;
                                                                            //valorDebito = (vehiculo.iva_vh * porcentajeRetIva) / 100;
                                                                            CreditoDebito = "Credito";
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        #endregion

                                                        #region Rete ICA = 5

                                                        if (parametro.id_nombre_parametro == 5)
                                                        {
                                                            if (!string.IsNullOrWhiteSpace(buscarProveedor2.retfuente))
                                                            {
                                                                perfiltributario buscarPerfilTributario =
                                                                    context.perfiltributario.FirstOrDefault(x =>
                                                                        x.bodega == codigoBodega &&
                                                                        x.sw == buscarTipoDocRegistro.sw &&
                                                                        x.tipo_regimenid == regimen_proveedor);
                                                                if (buscarPerfilTributario != null)
                                                                {
                                                                    if (buscarProveedor2.retica == "A")
                                                                    {
                                                                        if (!actividadEconomicaValido)
                                                                        {
                                                                            if ((buscarPerfilTributario.baseretica ?? 0) <=
                                                                                vehiculo.costosiniva_vh)
                                                                            {
                                                                                decimal porcentajeRetIca =
                                                                                    buscarPerfilTributario.pretica ?? 0;
                                                                                valorCredito =
                                                                                    vehiculo.costosiniva_vh *
                                                                                    porcentajeRetIca / 1000;
                                                                                CreditoDebito = "Credito";
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            valorCredito =
                                                                                vehiculo.costosiniva_vh *
                                                                                porcentajeReteICA / 1000;
                                                                            valorDebito =
                                                                                vehiculo.costosiniva_vh *
                                                                                porcentajeReteICA / 1000;
                                                                            CreditoDebito = "Credito";
                                                                        }
                                                                    }
                                                                }
                                                            }


                                                            if (buscarProveedor2.retfuente == null)
                                                            {
                                                                perfiltributario buscarPerfilTributario =
                                                                    context.perfiltributario.FirstOrDefault(x =>
                                                                        x.bodega == codigoBodega &&
                                                                        x.sw == buscarTipoDocRegistro.sw &&
                                                                        x.tipo_regimenid == regimen_proveedor);
                                                                if (buscarPerfilTributario != null)
                                                                {
                                                                    if (buscarTipoDocRegistro != null)
                                                                    {
                                                                        if (buscarPerfilTributario.retica == "A")
                                                                        {
                                                                            if (!actividadEconomicaValido)
                                                                            {
                                                                                if ((buscarPerfilTributario.baseretica ?? 0) <=
                                                                                    vehiculo.costosiniva_vh)
                                                                                {
                                                                                    decimal porcentajeRetIca =
                                                                                        buscarPerfilTributario.pretica ?? 0;
                                                                                    valorCredito =
                                                                                        vehiculo.costosiniva_vh *
                                                                                        porcentajeRetIca / 1000;
                                                                                    CreditoDebito = "Credito";
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                valorCredito =
                                                                                    vehiculo.costosiniva_vh *
                                                                                    porcentajeReteICA / 1000;
                                                                                valorDebito =
                                                                                    vehiculo.costosiniva_vh *
                                                                                    porcentajeReteICA / 1000;
                                                                                CreditoDebito = "Credito";
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        #endregion

                                                        #region Fletes = 6

                                                        if (parametro.id_nombre_parametro == 6)
                                                        {
                                                            //valorCredito = valorSinIva;
                                                            valorDebito = fletes;
                                                            CreditoDebito = "Debito";
                                                        }

                                                        #endregion

                                                        #region Despiento Pie = 7

                                                        if (parametro.id_nombre_parametro == 7)
                                                        {
                                                            valorCredito = fletes * porcentajeIvaFletes / 100;
                                                            CreditoDebito = "Credito";
                                                        }

                                                        #endregion

                                                        #region Inventario Debito = 9

                                                        if (parametro.id_nombre_parametro == 9)
                                                        {
                                                            //valorCredito = vehiculo.costosiniva_vh;
                                                            valorDebito = vehiculo.costosiniva_vh;
                                                            CreditoDebito = "Debito";
                                                        }

                                                        #endregion

                                                        #region IVA Fletes = 14

                                                        if (parametro.id_nombre_parametro == 14)
                                                        {
                                                            //valorCredito = valorSinIva;
                                                            valorDebito = descuentoPie;
                                                            CreditoDebito = "Debito";
                                                        }

                                                        #endregion

                                                        #region anticipos = 24

                                                        if (parametro.id_nombre_parametro == 24)
                                                        {
                                                            perfiltributario buscarPerfilTributario =
                                                                  context.perfiltributario.FirstOrDefault(x =>
                                                                      x.bodega == codigoBodega &&
                                                                      x.sw == buscarTipoDocRegistro.sw &&
                                                                      x.tipo_regimenid == regimen_proveedor);
                                                            decimal restarIcaIvaRetencion = 0;
                                                            if (!string.IsNullOrWhiteSpace(buscarProveedor2.retfuente))
                                                            {
                                                                if (buscarTipoDocRegistro != null)
                                                                {
                                                                    if (buscarProveedor2.retfuente == "A")
                                                                    {
                                                                        if ((buscarPerfilTributario.baseretfuente ?? 0) <=
                                                                            vehiculo.costosiniva_vh)
                                                                        {
                                                                            decimal porcentajeRetencion =
                                                                                buscarPerfilTributario.pretfuente ?? 0;
                                                                            restarIcaIvaRetencion +=
                                                                                vehiculo.costosiniva_vh *
                                                                                porcentajeRetencion / 100;
                                                                        }
                                                                    }

                                                                    if (buscarProveedor2.retiva == "A")
                                                                    {
                                                                        if ((buscarPerfilTributario.baseretiva ?? 0) <= vehiculo.iva_vh
                                                                        )
                                                                        {
                                                                            decimal porcentajeRetIva =
                                                                                buscarPerfilTributario.pretiva ?? 0;
                                                                            restarIcaIvaRetencion +=
                                                                                vehiculo.iva_vh * porcentajeRetIva / 100;
                                                                        }
                                                                    }

                                                                    if (buscarProveedor2.retica == "A")
                                                                    {
                                                                        if (!actividadEconomicaValido)
                                                                        {
                                                                            if ((buscarPerfilTributario.baseretica ?? 0) <=
                                                                                vehiculo.costosiniva_vh)
                                                                            {
                                                                                decimal porcentajeRetIca =
                                                                                    buscarPerfilTributario.pretica ?? 0;
                                                                                restarIcaIvaRetencion +=
                                                                                    vehiculo.costosiniva_vh *
                                                                                    porcentajeRetIca / 1000;
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            restarIcaIvaRetencion +=
                                                                                vehiculo.costosiniva_vh *
                                                                                porcentajeReteICA / 1000;
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            if (buscarProveedor2.retfuente == null)
                                                            {
                                                                if (buscarPerfilTributario != null)
                                                                {
                                                                    if (buscarTipoDocRegistro != null)
                                                                    {
                                                                        if (buscarPerfilTributario.retfuente == "A")
                                                                        {
                                                                            if ((buscarPerfilTributario.baseretfuente ?? 0) <=
                                                                                vehiculo.costosiniva_vh)
                                                                            {
                                                                                decimal porcentajeRetencion =
                                                                                    buscarPerfilTributario.pretfuente ?? 0;
                                                                                restarIcaIvaRetencion +=
                                                                                    vehiculo.costosiniva_vh *
                                                                                    porcentajeRetencion / 100;
                                                                            }
                                                                        }

                                                                        if (buscarPerfilTributario.retiva == "A")
                                                                        {
                                                                            if ((buscarPerfilTributario.baseretiva ?? 0) <=
                                                                                vehiculo.iva_vh)
                                                                            {
                                                                                decimal porcentajeRetIva =
                                                                                    buscarPerfilTributario.pretiva ?? 0;
                                                                                restarIcaIvaRetencion +=
                                                                                    vehiculo.iva_vh * porcentajeRetIva /
                                                                                    100;
                                                                            }
                                                                        }

                                                                        if (buscarPerfilTributario.retica == "A")
                                                                        {
                                                                            if (!actividadEconomicaValido)
                                                                            {
                                                                                if ((buscarPerfilTributario.baseretica ?? 0) <=
                                                                                    vehiculo.costosiniva_vh)
                                                                                {
                                                                                    decimal porcentajeRetIca =
                                                                                        buscarPerfilTributario.pretica ?? 0;
                                                                                    restarIcaIvaRetencion +=
                                                                                        vehiculo.costosiniva_vh *
                                                                                        porcentajeRetIca / 1000;
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                restarIcaIvaRetencion +=
                                                                                    vehiculo.costosiniva_vh *
                                                                                    porcentajeReteICA / 1000;
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }

                                                            valorCredito =
                                                                vehiculo.costosiniva_vh + vehiculo.iva_vh -
                                                                restarIcaIvaRetencion;
                                                            CreditoDebito = "Credito";
                                                            //valorDebito = ((vehiculo.costosiniva_vh + vehiculo.iva_vh) - restarIcaIvaRetencion);
                                                            //valorCredito = ValorFactura - restarIcaIvaRetencion;
                                                            //valorDebito = ValorFactura - restarIcaIvaRetencion;
                                                        }

                                                        #endregion

                                                        #region Validamos si la cuenta maneja niff o solo local	

                                                        if (buscarTipoDocRegistro.aplicaniff)
                                                        {
                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                                                {
                                                                    movimiento.debitoniif = valorDebito;
                                                                    movimiento.debito = valorDebito;
                                                                }

                                                                if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                                                {
                                                                    movimiento.credito = valorCredito;
                                                                    movimiento.creditoniif = valorCredito;
                                                                }
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                                                {
                                                                    movimiento.debitoniif = valorDebito;
                                                                }

                                                                if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                                                {
                                                                    movimiento.creditoniif = valorCredito;
                                                                }
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                                                {
                                                                    movimiento.debito = valorDebito;
                                                                }

                                                                if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                                                {
                                                                    movimiento.credito = valorCredito;
                                                                }
                                                            }
                                                        }

                                                        #endregion

                                                        #region Validamos si maneja base / Tercero / Documento 

                                                        if (buscarCuenta.manejabase)
                                                        {
                                                            if (parametro.id_nombre_parametro == 4)
                                                            {
                                                                movimiento.basecontable = vehiculo.iva_vh;
                                                            }
                                                            else
                                                            {
                                                                movimiento.basecontable = vehiculo.costosiniva_vh;
                                                            }
                                                        }

                                                        if (buscarCuenta.tercero)
                                                        {
                                                            movimiento.nit = nitFinal;
                                                        }

                                                        if (buscarCuenta.documeto)
                                                        {
                                                            movimiento.documento = vehiculo.plan_mayor;
                                                        }

                                                        #endregion

                                                        movimiento.detalle =
                                                            "Compra vehiculos fact " + vehiculo.plan_mayor + " y serie " +
                                                            vehiculo.vin;
                                                        context.mov_contable.Add(movimiento);
                                                        secuencia++;

                                                        #region Cuentas Valores

                                                        DateTime fechaBuscada = buscarUltimoEncabezado.fecha;
                                                        cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(
                                                            x => x.centro == parametro.centro &&
                                                                 x.cuenta == parametro.cuenta && x.nit == movimiento.nit &&
                                                                 x.ano == fechaBuscada.Year && x.mes == fechaBuscada.Month);

                                                        if (buscar_cuentas_valores != null)
                                                        {
                                                            buscar_cuentas_valores.debito += movimiento.debito;
                                                            buscar_cuentas_valores.credito += movimiento.credito;
                                                            buscar_cuentas_valores.debitoniff += movimiento.debitoniif;
                                                            buscar_cuentas_valores.creditoniff += movimiento.creditoniif;
                                                            context.Entry(buscar_cuentas_valores).State =
                                                                EntityState.Modified;
                                                            guardarLIneas = context.SaveChanges();
                                                        }
                                                        else
                                                        {
                                                            cuentas_valores crearCuentaValor = new cuentas_valores
                                                            {
                                                                saldo_ini = 0,
                                                                debito = 0,
                                                                credito = 0,
                                                                saldo_ininiff = 0,
                                                                debitoniff = 0,
                                                                creditoniff = 0,
                                                                ano = buscarUltimoEncabezado.fecha.Year,
                                                                mes = buscarUltimoEncabezado.fecha.Month,
                                                                cuenta = parametro.cuenta,
                                                                centro = parametro.centro,
                                                                nit = nitFinal
                                                            };
                                                            crearCuentaValor.debito = movimiento.debito;
                                                            crearCuentaValor.credito = movimiento.credito;
                                                            crearCuentaValor.debitoniff = movimiento.debitoniif;
                                                            crearCuentaValor.creditoniff = movimiento.creditoniif;
                                                            context.cuentas_valores.Add(crearCuentaValor);
                                                            guardarLIneas = context.SaveChanges();
                                                        }

                                                        #endregion
                                                    }
                                                }
                                            }
                                        }

                                        #endregion

                                        #region Validamos si se hizo la insercion en encabezado, lineas y cuentas valores -> si es asi actualizamos el consecutivo

                                        if (guardarEncabDoc > 0 && guardarLIneas > 0)
                                        {
                                            //registro los años modelo que no existía                                     
                                            int grupoId = grupoConsecutivo != null ? grupoConsecutivo.grupo : 0;
                                            List<grupoconsecutivos> gruposConsecutivos = context.grupoconsecutivos
                                                .Where(x => x.grupo == grupoId).ToList();
                                            foreach (grupoconsecutivos grupo in gruposConsecutivos)
                                            {
                                                icb_doc_consecutivos buscarElemento = context.icb_doc_consecutivos.FirstOrDefault(x =>
                                                    x.doccons_idtpdoc == idTipoDocumento &&
                                                    x.doccons_bodega == grupo.bodega_id);
                                                buscarElemento.doccons_siguiente = buscarElemento.doccons_siguiente + 1;
                                                context.Entry(buscarElemento).State = EntityState.Modified;
                                            }

                                            context.SaveChanges();
                                            numeroConsecutivo++;
                                            //}
                                            //else {
                                            //numeroCargueAuxPorAnio.doccons_siguiente = numeroCargueAuxPorAnio.doccons_siguiente + 1;
                                            //context.Entry(numeroCargueAuxPorAnio).State = EntityState.Modified;
                                            //context.SaveChanges();
                                            //var buscarConsecPorAnio = context.icb_doc_consecutivos.OrderByDescending(x => x.doccons_ano).FirstOrDefault();
                                            //buscarConsecPorAnio.doccons_siguiente = buscarConsecPorAnio.doccons_siguiente + 1;
                                            //context.Entry(buscarConsecPorAnio).State = EntityState.Modified;
                                            //context.SaveChanges();
                                            //}
                                        }

                                        #endregion

                                        #region Se agrega la trazabilidad del vehiculo nuevo, en este caso lo primero es facturacion

                                        context.icb_vehiculo_eventos.Add(new icb_vehiculo_eventos
                                        {
                                            evento_estado = true,
                                            eventofec_creacion = DateTime.Now,
                                            fechaevento = DateTime.Now,
                                            eventouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            evento_nombre = "Facturacion",
                                            id_tpevento = idEventoFacturacion,
                                            bodega_id = codigoBodega,
                                            vin = vehiculo.vin,
                                            planmayor = vehiculo.plan_mayor
                                        });
                                        int guardarVehEventos = context.SaveChanges();

                                        #endregion

                                        #region Se agrega la fecha de manifiesto del vehículo

                                        context.icb_vehiculo_eventos.Add(new icb_vehiculo_eventos
                                        {
                                            evento_estado = true,
                                            eventofec_creacion = DateTime.Now,
                                            fechaevento = vehiculo.fechaManifiesto,
                                            eventouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            evento_nombre = "Recepcion Manifiesto",
                                            id_tpevento = idEventoManifiesto,
                                            bodega_id = codigoBodega,
                                            vin = vehiculo.vin,
                                            planmayor = vehiculo.plan_mayor
                                        });

                                        #endregion
                                    }

                                    icb_arch_facturacion_log archLog = new icb_arch_facturacion_log
                                    {
                                        fact_log_fecha = DateTime.Now,
                                        fact_log_itemscorrecto = itemsCorrectos,
                                        fact_log_itemserror = itemsFallidos,
                                        fact_log_nombrearchivo = txtfile.FileName,
                                        fact_log_items = itemsCorrectos + itemsFallidos,
                                        fact_log_log = log,
                                        id_arch_facturacion = codigoArchFacturacion
                                    };
                                    context.icb_arch_facturacion_log.Add(archLog);

                                    int guardarLogs = context.SaveChanges();

                                    if (guardarLogs > 0)
                                    {
                                        //guardamos los anios modelo
                                        foreach (anioModeloModel item in anios_modelo)
                                        {
                                            anio_modelo aniomo = new anio_modelo
                                            {
                                                anio = item.anio,
                                                codigo_modelo = item.codigo_modelo,
                                                concesionarioid = item.concesionarioid,
                                                costosiniva = item.costosiniva,
                                                descripcion = item.descripcion,
                                                idporcentajecompra = item.idporcentajecompra,
                                                idporcentajeimpoconsumo = item.idporcentajeimpoconsumo,
                                                idporcentajeiva = item.idporcentajeiva,
                                                impuesto_consumo = item.impuesto_consumo,
                                                matricula = item.matricula,
                                                poliza = item.poliza,
                                                porcentaje_compra = item.porcentaje_compra,
                                                porcentaje_iva = item.porcentaje_iva,
                                                precio = item.precio,
                                                valor = item.valor
                                            };
                                            context.anio_modelo.Add(aniomo);
                                        }
                                        int guardar = context.SaveChanges();
                                        dbTran.Commit();
                                        TempData["mensaje"] =
                                            "La carga del archivo se realizo correctamente, registros cargados : " +
                                            itemsCorrectos + ", registros no cargados : " + itemsFallidos;
                                        return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
                                    }
                                }
                                else
                                {
                                    dbTran.Rollback();
                                    return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
                                }

                                #endregion
                            }
                            catch (FormatException)
                            {
                                dbTran.Rollback();
                                int gg = lineaError;
                                int g = numeroLinea;
                                TempData["mensajeError"] =
                                    "Los datos ingresados para el cargue del archivo plano pueden estar mal escritos, por favor verifica";
                                return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
                                //Validacion para cuando no se asigna algun dato tipo entero al importar el archivo plano
                            }
                        }
                    }
                    

                }
                catch (DbEntityValidationException)
                {
                    dbTran.Rollback();
                    throw;
                }
            }

            //}
            //catch (Exception e)
            //{
            //    TempData["mensajeError"] = "Error al leer el archivo, verifique que el archivo sea el correcto o verifique la siguiente excepcion : " + e.Message;
            //    return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
            //}
            return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
        }

        public ActionResult DetallesFacturacionGM(int id, int? menu)
        {
            ViewBag.tipos = new SelectList(context.tipo_vehiculo, "tpvh_id", "tpvh_nombre");
            ViewBag.marcas = new SelectList(context.marca_vehiculo, "marcvh_id", "marcvh_nombre");
            ViewBag.modelos = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewBag.color = new SelectList(context.color_vehiculo, "colvh_id", "colvh_nombre");
            ViewBag.clasificacion = new SelectList(context.clacompra_vehiculo, "clacompvh_id", "clacompvh_nombre");
            ViewBag.ciudad = new SelectList(context.nom_ciudad.OrderBy(x => x.ciu_nombre), "ciu_id", "ciu_nombre");
            ViewBag.idFactura = id;
            var provedores = (from pro in context.tercero_proveedor
                              join ter in context.icb_terceros
                                  on pro.tercero_id equals ter.tercero_id
                              select new
                              {
                                  idTercero = ter.tercero_id,
                                  nombreTErcero = ter.prinom_tercero,
                                  apellidosTercero = ter.apellido_tercero
                              }).OrderBy(x => x.nombreTErcero);
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in provedores)
            {
                items.Add(new SelectListItem { Text = item.nombreTErcero, Value = item.idTercero.ToString() });
            }

            ViewBag.proveedor = items;
            ViewBag.tipoTramitador = new SelectList(context.tptramitador_vh.OrderBy(x => x.tptramivh_nombre),
                "tptramivh_id", "tptramivh_nombre");
            ViewBag.tramitador_id = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewBag.bodegas = context.bodega_concesionario;
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult Ver(int id, int? menu)
        {
            #region Se comento esta area de codigo por que estaba mostrando la informacion del vehiculo y no la de la compra ->18/12/18

            // Campos para ver la informacion detallada de un vehiculo en particular

            //var buscarVehiculo = context.icb_vehiculo.FirstOrDefault(x => x.icbvh_id == id);

            //ViewBag.tipos = new SelectList(context.tipo_vehiculo, "tpvh_id", "tpvh_nombre");
            //ViewBag.marcas = new SelectList(context.marca_vehiculo, "marcvh_id", "marcvh_nombre");
            //ViewBag.modelos = new SelectList(Enumerable.Empty<SelectListItem>());
            //ViewBag.color = new SelectList(context.color_vehiculo, "colvh_id", "colvh_nombre");
            //ViewBag.clasificacion = new SelectList(context.clacompra_vehiculo, "clacompvh_id", "clacompvh_nombre");
            //ViewBag.ciudad = new SelectList(context.nom_ciudad.OrderBy(x => x.ciu_nombre), "ciu_id", "ciu_nombre");
            //ViewBag.idFactura = id;
            //var provedores = (from pro in context.tercero_proveedor
            //				  join ter in context.icb_terceros
            //				  on pro.tercero_id equals ter.tercero_id
            //				  select new
            //				  {
            //					  idTercero = ter.tercero_id,
            //					  nombreTErcero = ter.razon_social + ter.prinom_tercero + " " + ter.segnom_tercero + " " + ter.apellido_tercero + " " + ter.segapellido_tercero,
            //				  }).OrderBy(x => x.nombreTErcero).ToList();
            //List<SelectListItem> items = new List<SelectListItem>();
            //foreach (var item in provedores)
            //{
            //	items.Add(new SelectListItem() { Text = item.nombreTErcero, Value = item.idTercero.ToString() });
            //}

            //ViewBag.proveedor = new SelectList(provedores, "idTercero", "nombreTErcero", buscarVehiculo.proveedor_id);
            //ViewBag.tipoTramitador = new SelectList(context.tptramitador_vh.OrderBy(x => x.tptramivh_nombre), "tptramivh_id", "tptramivh_nombre");
            //ViewBag.tramitador_id = new SelectList(Enumerable.Empty<SelectListItem>());

            //var planMayor = buscarVehiculo.plan_mayor;

            //ViewBag.facturaProveedor = context.encab_documento.FirstOrDefault(x => x.documento == planMayor).documento;

            #endregion

            verVehiculoComprado buscarVehiculo = (from v in context.icb_vehiculo
                                                  join l in context.lineas_documento
                                                      on v.plan_mayor equals l.codigo
                                                  join e in context.encab_documento
                                                      on l.id_encabezado equals e.idencabezado
                                                  where l.id_encabezado == id
                                                  || v.icbvh_id == id
                                                  select new verVehiculoComprado
                                                  {
                                                      proveedor_id = e.nit,
                                                      marcvh_id = v.marcvh_id,
                                                      modvh_id = v.modvh_id,
                                                      colvh_id = v.colvh_id,
                                                      plan_mayor = v.plan_mayor,
                                                      vin = v.vin,
                                                      plac_vh = v.plac_vh,
                                                      fecfact_fabrica = v.fecfact_fabrica,
                                                      costosiniva_vh = l.valor_unitario,
                                                      iva_vh = l.porcentaje_iva,
                                                      nummot_vh = v.nummot_vh,
                                                      anio_vh = v.anio_vh
                                                  }).FirstOrDefault();

            ViewBag.tipos = new SelectList(context.tipo_vehiculo, "tpvh_id", "tpvh_nombre");
            ViewBag.marcas = new SelectList(context.marca_vehiculo, "marcvh_id", "marcvh_nombre",
                buscarVehiculo.marcvh_id);
            ViewBag.modelos = new SelectList(context.modelo_vehiculo, "modvh_codigo", "modvh_nombre",
                buscarVehiculo.modvh_id);
            //ViewBag.modelos = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewBag.color = new SelectList(context.color_vehiculo, "colvh_id", "colvh_nombre", buscarVehiculo.colvh_id);
            ViewBag.clasificacion = new SelectList(context.clacompra_vehiculo, "clacompvh_id", "clacompvh_nombre");
            ViewBag.ciudad = new SelectList(context.nom_ciudad.OrderBy(x => x.ciu_nombre), "ciu_id", "ciu_nombre");
            ViewBag.idFactura = id;
            var provedores = (from pro in context.tercero_proveedor
                              join ter in context.icb_terceros
                                  on pro.tercero_id equals ter.tercero_id
                              select new
                              {
                                  idTercero = ter.tercero_id,
                                  nombreTErcero = ter.razon_social + ter.prinom_tercero + " " + ter.segnom_tercero + " " +
                                                  ter.apellido_tercero + " " + ter.segapellido_tercero
                              }).OrderBy(x => x.nombreTErcero).ToList();
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in provedores)
            {
                items.Add(new SelectListItem { Text = item.nombreTErcero, Value = item.idTercero.ToString() });
            }

            ViewBag.proveedor = new SelectList(provedores, "idTercero", "nombreTErcero", buscarVehiculo.proveedor_id);
            ViewBag.tipoTramitador = new SelectList(context.tptramitador_vh.OrderBy(x => x.tptramivh_nombre),
                "tptramivh_id", "tptramivh_nombre");
            ViewBag.tramitador_id = new SelectList(Enumerable.Empty<SelectListItem>());

            string planMayor = buscarVehiculo.plan_mayor;

            ViewBag.facturaProveedor = context.encab_documento.FirstOrDefault(x => x.documento == planMayor).documento;

            BuscarFavoritos(menu);
            return View(buscarVehiculo);
        }

        public JsonResult cargarDatosManifiesto(int idActual)
        {
            var data2 = (from v in context.icb_vehiculo
                         join c in context.nom_ciudad
                             on v.ciumanf_vh equals c.ciu_id into tmp
                         from c in tmp.DefaultIfEmpty()
                         where v.icbvh_id == idActual
                         select new
                         {
                             v.fecentman_vh,
                             v.ciumanf_vh,
                             c.ciu_nombre,
                             v.nummanf_vh,
                             v.diaslibres_vh
                         }).ToList();

            var data = data2.Select(x => new
            {
                fecentman_vh = x.fecentman_vh.Value.ToString("dd/MM/yyyy"),
                x.ciumanf_vh,
                x.ciu_nombre,
                x.nummanf_vh,
                x.diaslibres_vh
            }).FirstOrDefault();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Inventario(int? menu)
        {
            List<bodega_concesionario> bodegas = context.bodega_concesionario.ToList();
            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (bodega_concesionario item in bodegas)
            {
                lista.Add(new SelectListItem { Text = item.bodccs_nombre, Value = item.bodccs_cod });
            }

            ViewBag.bodegas = new SelectList(lista);
            ViewBag.totalVehiculos = context.icb_vehiculo.Where(x => x.nuevo == true).Count();
            ViewBag.totalPedidos = context.detallePedido_GM.Count();
            List<icb_tpeventos> estatus = context.icb_tpeventos.ToList();
            List<SelectListItem> listaStatus = new List<SelectListItem>();
            foreach (icb_tpeventos item2 in estatus)
            {
                listaStatus.Add(new SelectListItem
                { Text = item2.tpevento_nombre, Value = item2.tpevento_id.ToString() });
            }

            ViewBag.EstatusBusqueda = new SelectList(listaStatus);

            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Inventario(string cod_bodega, DateTime? fecha_desde, DateTime? fecha_hasta, int? evento_id,
            int? menu)
        {
            List<bodega_concesionario> bodegas = context.bodega_concesionario.ToList();
            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (bodega_concesionario item in bodegas)
            {
                lista.Add(new SelectListItem { Text = item.bodccs_nombre, Value = item.bodccs_cod });
            }

            ViewBag.bodegas = new SelectList(lista);

            //List<string> listaContiene = new List<string>() { "P7", "P16" };
            //var estatus = (from paramSistema in context.icb_sysparameter
            //               where listaContiene.Contains(paramSistema.syspar_cod)
            //               select new { paramSistema.syspar_value, paramSistema.syspar_nombre }).ToList();
            List<icb_tpeventos> estatus = context.icb_tpeventos.ToList();
            List<SelectListItem> listaStatus = new List<SelectListItem>();
            foreach (icb_tpeventos item2 in estatus)
            {
                listaStatus.Add(new SelectListItem
                { Text = item2.tpevento_nombre, Value = item2.tpevento_id.ToString() });
            }

            ViewBag.EstatusBusqueda = new SelectList(listaStatus);

            ViewBag.totalVehiculos = context.icb_vehiculo.Where(x => x.nuevo == true).Count();
            ViewBag.totalPedidos = context.detallePedido_GM.Count();

            if (cod_bodega == "" && fecha_desde == null && fecha_hasta == null && evento_id == null)
            {
                return View("Inventario", new { menu });
            }

            return View("Inventario", new { cod_bodega, fecha_desde, fecha_hasta, evento_id, menu });
        }

        //fdsafsafda
        // GET: 
        public ActionResult ComprasIndividuales(int? menu)
        {
            var buscarTipoDocumento = (from tipoDocumento in context.tp_doc_registros
                                       where tipoDocumento.tipo == 1
                                       select new
                                       {
                                           tipoDocumento.tpdoc_id,
                                           nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                           tipoDocumento.tipo,
                                           nomorden = tipoDocumento.tpdoc_nombre
                                       }).OrderBy(td => td.nomorden).ToList();
            ViewBag.tipo_documentoFiltro = new SelectList(buscarTipoDocumento, "tpdoc_id", "nombre");
            var provedores = (from pro in context.tercero_proveedor
                              join ter in context.icb_terceros
                                  on pro.tercero_id equals ter.tercero_id
                              select new
                              {
                                  idTercero = ter.tercero_id,
                                  nombreTErcero = ter.prinom_tercero,
                                  apellidosTercero = ter.apellido_tercero,
                                  razonSocial = ter.razon_social
                              }).OrderBy(x => x.nombreTErcero);
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in provedores)
            {
                string nombre = item.nombreTErcero + " " + item.apellidosTercero + " " + item.razonSocial;
                items.Add(new SelectListItem { Text = nombre, Value = item.idTercero.ToString() });
            }

            ViewBag.proveedor = items;
            icb_sysparameter buscarParametroSw = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P23");
            string swParametro = buscarParametroSw != null ? buscarParametroSw.syspar_value : "1";
            int idTipoSw = Convert.ToInt32(swParametro);
            ViewBag.doc_registros = context.tp_doc_registros.Where(x => x.sw == idTipoSw && x.tipo == 1)
                .OrderBy(x => x.tpdoc_nombre);
            ViewBag.condicion_pago = context.fpago_tercero;
            ViewBag.Cantidad = (context.icb_vehiculo.Count() + 9) / 10;

            List<menu_busqueda> detallesFacturacion = context.menu_busqueda
                .Where(x => x.menu_busqueda_id_menu == 79 && x.menu_busqueda_id_pestana == 4).ToList();
            ViewBag.paramBusquedaDetalles = detallesFacturacion;
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult ComprasIndividuales(HttpPostedFileBase txtfile, int? menu)
        {
            try
            {
                string path = Server.MapPath("~/Content/" + txtfile.FileName);
                // Validacion para cuando el archivo esta en uso y no puede ser usado desde visual 
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }

                txtfile.SaveAs(path);

                List<vconceptocompra> tipoCompraVehiculos = context.vconceptocompra.ToList();
                List<perfil_cuentas_documento> listaPerfilCuentas = context.perfil_cuentas_documento.ToList();
                List<cuenta_puc> listaCuentasPucAfectables = context.cuenta_puc.Where(x => x.esafectable).ToList();

                int codigoBodega = Convert.ToInt32(Request["selectBodegas"]);
                int idTipoDocumento = Convert.ToInt32(Request["selectTipoDocumento"]);
                decimal fletes = !string.IsNullOrEmpty(Request["txtFletes"])
                    ? Convert.ToDecimal(Request["txtFletes"].Replace(".", ","), miCultura)
                    : 0;
                decimal porcentajeIvaFletes = !string.IsNullOrEmpty(Request["txtPorcentajeFletes"])
                    ? Convert.ToDecimal(Request["txtPorcentajeFletes"], miCultura)
                    : 0;
                decimal descuentoPie = !string.IsNullOrEmpty(Request["txtDescuentoPie"])
                    ? Convert.ToDecimal(Request["txtDescuentoPie"], miCultura)
                    : 0;

                using (StreamReader lector = new StreamReader(path))
                {
                    bool encabezado = true;
                    int numeroLinea = 1;
                    int itemsFallidos = 0;
                    int itemsCorrectos = 0;
                    string log = "";
                    List<CrearVehiculoExcel> listaVehiculos = new List<CrearVehiculoExcel>();
                    string planMayor = "";
                    DateTime? fecha = null;
                    int anio = 0;
                    string codigoModelo = "";
                    string codigoColor = "";
                    string kmat = "";
                    string numeroMotor = "";
                    string notas = "";
                    bool flota = false;
                    string codigoPago = "";
                    string bodegaGM = "";
                    long numpedido_vh = 0;
                    decimal ValorFactura = 0;
                    decimal valorSinIva = 0;
                    decimal porcentajeIva = 0;
                    decimal impuestoConsumo = 0;
                    bool errorVin = false;
                    int lineaError = 1;
                    int idProveedor = 0;
                    idProveedor = Convert.ToInt32(Request["selectProveedor"]);


                    tp_doc_registros buscarTipoDocRegistro =
                        context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == idTipoDocumento);
                    // Validacion para reteIVA, reteICA y retencion dependiendo del proveedor seleccionado
                    bool actividadEconomicaValido = false;
                    decimal porcentajeReteICA = 0;
                    var buscarProveedor = (from tercero in context.icb_terceros
                                           join proveedor in context.tercero_proveedor
                                               on tercero.tercero_id equals proveedor.tercero_id
                                           join acteco in context.acteco_tercero
                                               on proveedor.acteco_id equals acteco.acteco_id
                                           join bodega in context.terceros_bod_ica
                                               on acteco.acteco_id equals bodega.idcodica into ps
                                           from bodega in ps.DefaultIfEmpty()
                                           join regimen in context.tpregimen_tercero
                                               on proveedor.tpregimen_id equals regimen.tpregimen_id into ps2
                                           from regimen in ps2.DefaultIfEmpty()
                                           join pago in context.fpago_tercero
                                               on proveedor.fpago_id equals pago.fpago_id into tp
                                           from pago in tp.DefaultIfEmpty()
                                           where proveedor.tercero_id == idProveedor
                                           select new
                                           {
                                               proveedor.acteco_id,
                                               acteco.acteco_nombre,
                                               pago.fpago_id,
                                               tarifaPorBodega = bodega != null ? bodega.porcentaje : 0,
                                               actecoTarifa = acteco.tarifa,
                                               regimen_id = regimen.tpregimen_id
                                           }).FirstOrDefault();
                    if (buscarProveedor != null)
                    {
                        if (buscarProveedor.tarifaPorBodega != 0)
                        {
                            actividadEconomicaValido = true;
                            porcentajeReteICA = buscarProveedor.tarifaPorBodega;
                        }
                        else if (buscarProveedor.actecoTarifa != 0)
                        {
                            actividadEconomicaValido = true;
                            porcentajeReteICA = buscarProveedor.actecoTarifa;
                        }
                    }

                    int regimen_proveedor = buscarProveedor != null ? buscarProveedor.regimen_id : 0;
                    tercero_proveedor buscarPerfilTributario =
                        context.tercero_proveedor.FirstOrDefault(x => x.tercero_id == idProveedor);
                    // var buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x => x.bodega == codigoBodega && x.sw == buscarTipoDocRegistro.sw && x.tipo_regimenid == regimen_proveedor);
                    vtipo_compra_vehiculos buscarTipoCompra = new vtipo_compra_vehiculos();

                    while (lector.Peek() > -1)
                    {
                        string linea = lector.ReadLine();
                        if (!encabezado)
                        {
                            if (!string.IsNullOrEmpty(linea))
                            {
                                if (numeroLinea == 1)
                                {
                                    lineaError++;
                                    planMayor = linea.Substring(2, 10);
                                    fecha = new DateTime(Convert.ToInt32(linea.Substring(12, 4)),
                                        Convert.ToInt32(linea.Substring(16, 2)),
                                        Convert.ToInt32(linea.Substring(18, 2)));
                                    ValorFactura = Convert.ToDecimal(linea.Substring(42, 11).Trim(), miCultura);
                                    codigoPago = linea.Substring(74, 4);
                                    numeroLinea = 2;
                                }
                                else
                                {
                                    lineaError++;
                                    anio = 2000 + Convert.ToInt32(linea.Substring(37, 2));
                                    codigoModelo = linea.Substring(39, 11);
                                    kmat = linea.Substring(37, 17);
                                    modelo_vehiculo buscaModelo =
                                        context.modelo_vehiculo.FirstOrDefault(x => x.modvh_codigo == codigoModelo);
                                    if (buscaModelo == null)
                                    {
                                        TempData["mensajeError"] =
                                            "El codigo de modelo " + codigoModelo +
                                            " no existe, debe registrarlo antes de continuar, linea de error " +
                                            lineaError;
                                        return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
                                    }


                                    anio_modelo buscaAnioModelo = context.anio_modelo.FirstOrDefault(x =>
                                        x.codigo_modelo == codigoModelo && x.anio == anio);
                                    if (buscaAnioModelo == null)
                                    {
                                        TempData["mensajeError"] =
                                            "El codigo " + codigoModelo + " de modelo " + anio +
                                            " no esta registrado, debe registrarlo antes de continuar, linea de error " +
                                            lineaError;
                                        return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
                                    }

                                    valorSinIva = Convert.ToDecimal(linea.Substring(127, 21).Trim(), miCultura);
                                    impuestoConsumo = buscaAnioModelo.impuesto_consumo;
                                    porcentajeIva = buscaAnioModelo.porcentaje_iva;
                                    //var test = (long)((buscaAnioModelo.valor * buscaAnioModelo.porcentaje_iva / 100) + buscaAnioModelo.valor);
                                    if (buscaAnioModelo.valor == null)
                                    {
                                        buscaAnioModelo.valor = valorSinIva;
                                        buscaAnioModelo.porcentaje_iva =
                                            decimal.Round(ValorFactura * 100 / valorSinIva - 100, 2);
                                        context.Entry(buscaAnioModelo).State = EntityState.Modified;
                                        context.SaveChanges();
                                        //TempData["mensajeError"] = "El codigo " + codigoModelo + " de modelo " + anio + " no tiene asignado un precio, verifique el modelo, linea de error" + lineaError;
                                        //return RedirectToAction("FacturacionGM", "gestionVhNuevo");
                                    }
                                    else
                                    {
                                        if ((long)(buscaAnioModelo.valor * buscaAnioModelo.porcentaje_iva / 100 +
                                                    buscaAnioModelo.valor) == ValorFactura &&
                                            buscaAnioModelo.valor == valorSinIva)
                                        {
                                            // Significa que el valor y el iva estan actualizados correctamente
                                        }
                                        else
                                        {
                                            buscaAnioModelo.valor = valorSinIva;
                                            buscaAnioModelo.porcentaje_iva =
                                                decimal.Round(ValorFactura * 100 / valorSinIva - 100, 2);
                                            context.Entry(buscaAnioModelo).State = EntityState.Modified;
                                            context.SaveChanges();
                                            //TempData["mensajeError"] = "El codigo " + codigoModelo + " de modelo " + anio + " tiene diferente el valor de vehiculo o del iva, debe actualizarlo antes de continuar, linea de error " + lineaError;
                                            //return RedirectToAction("FacturacionGM", "gestionVhNuevo");
                                        }
                                    }

                                    codigoColor = linea.Substring(51, 3);
                                    color_vehiculo buscaColor =
                                        context.color_vehiculo.FirstOrDefault(x => x.colvh_id == codigoColor);
                                    if (buscaColor == null)
                                    {
                                        TempData["mensajeError"] =
                                            "El codigo de color " + codigoColor +
                                            " no existe, debe registrarlo antes de continuar, linea de error " +
                                            lineaError;
                                        return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
                                    }

                                    string VIN = linea.Substring(75, 17);
                                    errorVin = false;

                                    var buscarAutomovil = (from vehiculo in context.icb_vehiculo
                                                           join encabezadoDoc in context.encab_documento
                                                               on vehiculo.plan_mayor equals encabezadoDoc.documento into veh
                                                           from encabezadoDoc in veh.DefaultIfEmpty()
                                                           where vehiculo.plan_mayor == planMayor
                                                           select new
                                                           {
                                                               anulado = encabezadoDoc != null ? encabezadoDoc.anulado : false
                                                           }).FirstOrDefault();

                                    bool vehiculo_actualizar = false;
                                    if (buscarAutomovil != null)
                                    {
                                        if (buscarAutomovil.anulado)
                                        {
                                            // Significa que ya esta agregado pero esta anulado, por tanto se puede volver a agregar
                                            vehiculo_actualizar = true;
                                            itemsCorrectos++;
                                        }
                                        else
                                        {
                                            itemsFallidos++;
                                            errorVin = true;
                                            if (log.Length < 1)
                                            {
                                                log += VIN + "*El numero VIN ya existe";
                                            }
                                            else
                                            {
                                                log += "|" + VIN + "*El numero VIN ya existe";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Significa que no lo encontro, por tanto se puede agregar sin problemas
                                        itemsCorrectos++;
                                    }

                                    numeroMotor = linea.Substring(55, 20).Trim();
                                    numpedido_vh = Convert.ToInt64(linea.Substring(27, 10));
                                    bodegaGM = linea.Substring(393, 4);
                                    notas = linea.Substring(319, 45).Trim();
                                    if (notas.ToUpper().Contains("FLOTA"))
                                    {
                                        flota = true;
                                    }

                                    if (!errorVin)
                                    {
                                        CrearVehiculoExcel nuevoVehiculo = new CrearVehiculoExcel
                                        {
                                            fecfact_fabrica = fecha ?? DateTime.Now,
                                            plan_mayor = planMayor,
                                            anio_vh = anio,
                                            modvh_id = codigoModelo,
                                            colvh_id = codigoColor,
                                            costototal_vh = ValorFactura,
                                            costosiniva_vh = valorSinIva,
                                            iva_vh = ValorFactura - valorSinIva,
                                            porcentajeIva = porcentajeIva,
                                            vin = VIN,
                                            porcentajeReteIca = porcentajeReteICA,
                                            porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion,
                                            porcentajeReteIva = (decimal)buscarTipoDocRegistro.retiva,
                                            nummot_vh = numeroMotor,
                                            numpedido_vh = numpedido_vh,
                                            impuesto_consumo = impuestoConsumo,
                                            bodegaGM = bodegaGM,
                                            codigoPago = codigoPago,
                                            notas = notas,
                                            flota = flota,
                                            vehiculo_actualizar = vehiculo_actualizar,
                                            kmat = kmat,
                                            porcentaje_iva = buscaAnioModelo.porcentaje_iva
                                        };
                                        listaVehiculos.Add(nuevoVehiculo);
                                    }

                                    numeroLinea = 1;


                                    // Validacion para el debito y credito de las cuentas verificar que los datos estan bien escritos

                                    int idConceptoCompra = 0;
                                    foreach (vconceptocompra item in tipoCompraVehiculos)
                                    {
                                        if (item.codigo.ToUpper().Contains(codigoPago))
                                        {
                                            idConceptoCompra = item.id;
                                        }
                                    }

                                    buscarTipoCompra = context.vtipo_compra_vehiculos.FirstOrDefault(x =>
                                        x.idtipocomprav == idConceptoCompra && x.idbodega == codigoBodega &&
                                        x.iddoc == idTipoDocumento);

                                    List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();

                                    // Significa que hay un perfil contable encontrado
                                    if (buscarTipoCompra != null)
                                    {
                                        //var parametrosCuentas = listaPerfilCuentas.Where(x => x.id_perfil == buscarTipoCompra.idperfilcontable).ToList();
                                        var parametrosCuentas = (from perfil in context.perfil_cuentas_documento
                                                                 join nombreParametro in context.paramcontablenombres
                                                                     on perfil.id_nombre_parametro equals nombreParametro.id
                                                                 join cuenta in context.cuenta_puc
                                                                     on perfil.cuenta equals cuenta.cntpuc_id
                                                                 join documento in context.perfil_contable_documento
                                                                     on perfil.id_perfil equals documento.id
                                                                 where perfil.id_perfil == buscarTipoCompra.idperfilcontable
                                                                 select new
                                                                 {
                                                                     perfil.id,
                                                                     perfil.id_nombre_parametro,
                                                                     perfil.cuenta,
                                                                     perfil.centro,
                                                                     perfil.id_perfil,
                                                                     nombreParametro.descripcion_parametro,
                                                                     cuenta.cntpuc_numero,
                                                                     documento.descripcion,
                                                                     documento.codigo,
                                                                     perfil_contable_id = documento.id
                                                                 }).ToList();

                                        decimal calcularDebito = 0;
                                        decimal calcularCredito = 0;
                                        foreach (var parametro in parametrosCuentas)
                                        {
                                            cuenta_puc buscarCuenta =
                                                listaCuentasPucAfectables.FirstOrDefault(x =>
                                                    x.cntpuc_id == parametro.cuenta);
                                            if (buscarCuenta != null)
                                            {
                                                decimal valorCredito = 0;
                                                decimal valorDebito = 0;
                                                if (parametro.id_nombre_parametro == 1)
                                                {
                                                    decimal restarIcaIvaRetencion = 0;
                                                    // if (buscarPerfilTributario != null)
                                                    if (buscarPerfilTributario != null)
                                                    {
                                                        if (buscarTipoDocRegistro != null)
                                                        {
                                                            if (buscarPerfilTributario.retfuente == "A")
                                                            {
                                                                if (buscarTipoDocRegistro.baseretencion <= valorSinIva)
                                                                {
                                                                    decimal porcentajeRetencion =
                                                                        (decimal)buscarTipoDocRegistro.retencion;
                                                                    restarIcaIvaRetencion +=
                                                                        valorSinIva * porcentajeRetencion / 100;
                                                                }
                                                            }

                                                            if (buscarPerfilTributario.retiva == "A")
                                                            {
                                                                if (buscarTipoDocRegistro.baseiva <=
                                                                    ValorFactura - valorSinIva)
                                                                {
                                                                    decimal porcentajeRetIva =
                                                                        (decimal)buscarTipoDocRegistro.retiva;
                                                                    restarIcaIvaRetencion +=
                                                                        (ValorFactura - valorSinIva) *
                                                                        porcentajeRetIva / 100;
                                                                }
                                                            }

                                                            if (buscarPerfilTributario.retica == "A")
                                                            {
                                                                if (!actividadEconomicaValido)
                                                                {
                                                                    if (buscarTipoDocRegistro.baseica <= valorSinIva)
                                                                    {
                                                                        decimal porcentajeRetIca =
                                                                            (decimal)buscarTipoDocRegistro.retica;
                                                                        restarIcaIvaRetencion +=
                                                                            valorSinIva * porcentajeRetIca / 1000;
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    restarIcaIvaRetencion +=
                                                                        valorSinIva * porcentajeReteICA / 1000;
                                                                }
                                                            }
                                                        }
                                                    }

                                                    valorCredito = ValorFactura - restarIcaIvaRetencion;
                                                    calcularCredito += valorCredito;
                                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                    {
                                                        NumeroCuenta = parametro.cntpuc_numero,
                                                        DescripcionParametro = parametro.descripcion_parametro,
                                                        ValorDebito = 0,
                                                        ValorCredito = valorCredito
                                                    });
                                                }

                                                if (parametro.id_nombre_parametro == 2)
                                                {
                                                    valorDebito = ValorFactura - valorSinIva;
                                                    calcularDebito += valorDebito;
                                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                    {
                                                        NumeroCuenta = parametro.cntpuc_numero,
                                                        DescripcionParametro = parametro.descripcion_parametro,
                                                        ValorDebito = valorDebito,
                                                        ValorCredito = 0
                                                    });
                                                }

                                                if (parametro.id_nombre_parametro == 3)
                                                {
                                                    if (buscarPerfilTributario != null)
                                                    {
                                                        if (buscarTipoDocRegistro != null)
                                                        {
                                                            if (buscarPerfilTributario.retfuente == "A")
                                                            {
                                                                if (buscarTipoDocRegistro.baseretencion <= valorSinIva)
                                                                {
                                                                    decimal porcentajeRetencion =
                                                                        (decimal)buscarTipoDocRegistro.retencion;
                                                                    valorCredito =
                                                                        valorSinIva * porcentajeRetencion / 100;
                                                                    calcularCredito += valorCredito;
                                                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                                    {
                                                                        NumeroCuenta = parametro.cntpuc_numero,
                                                                        DescripcionParametro =
                                                                            parametro.descripcion_parametro,
                                                                        ValorDebito = 0,
                                                                        ValorCredito = valorCredito
                                                                    });
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                if (parametro.id_nombre_parametro == 4)
                                                {
                                                    if (buscarPerfilTributario != null)
                                                    {
                                                        if (buscarTipoDocRegistro != null)
                                                        {
                                                            if (buscarPerfilTributario.retiva == "A")
                                                            {
                                                                if (buscarTipoDocRegistro.baseiva <= valorSinIva)
                                                                {
                                                                    decimal porcentajeRetIva =
                                                                        (decimal)buscarTipoDocRegistro.retiva;
                                                                    valorCredito =
                                                                        (ValorFactura - valorSinIva) *
                                                                        porcentajeRetIva / 100;
                                                                    calcularCredito += valorCredito;
                                                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                                    {
                                                                        NumeroCuenta = parametro.cntpuc_numero,
                                                                        DescripcionParametro =
                                                                            parametro.descripcion_parametro,
                                                                        ValorDebito = 0,
                                                                        ValorCredito = valorCredito
                                                                    });
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                if (parametro.id_nombre_parametro == 5)
                                                {
                                                    if (buscarPerfilTributario != null)
                                                    {
                                                        if (buscarTipoDocRegistro != null)
                                                        {
                                                            if (buscarPerfilTributario.retica == "A")
                                                            {
                                                                if (!actividadEconomicaValido)
                                                                {
                                                                    if (buscarTipoDocRegistro.baseica <= valorSinIva)
                                                                    {
                                                                        decimal porcentajeRetIca =
                                                                            (decimal)buscarTipoDocRegistro.retica;
                                                                        valorCredito =
                                                                            valorSinIva * porcentajeRetIca / 1000;
                                                                        calcularCredito += valorCredito;
                                                                        listaDescuadrados.Add(
                                                                            new DocumentoDescuadradoModel
                                                                            {
                                                                                NumeroCuenta = parametro.cntpuc_numero,
                                                                                DescripcionParametro =
                                                                                    parametro.descripcion_parametro,
                                                                                ValorDebito = 0,
                                                                                ValorCredito = valorCredito
                                                                            });
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    valorCredito =
                                                                        valorSinIva * porcentajeReteICA / 1000;
                                                                    calcularCredito += valorCredito;
                                                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                                    {
                                                                        NumeroCuenta = parametro.cntpuc_numero,
                                                                        DescripcionParametro =
                                                                            parametro.descripcion_parametro,
                                                                        ValorDebito = 0,
                                                                        ValorCredito = valorCredito
                                                                    });
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                                if (parametro.id_nombre_parametro == 6)
                                                {
                                                    valorDebito = fletes;
                                                    calcularDebito += valorDebito;
                                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                    {
                                                        NumeroCuenta = parametro.cntpuc_numero,
                                                        DescripcionParametro = parametro.descripcion_parametro,
                                                        ValorDebito = valorDebito,
                                                        ValorCredito = 0
                                                    });
                                                }

                                                if (parametro.id_nombre_parametro == 7)
                                                {
                                                    valorCredito = fletes * porcentajeIvaFletes / 100;
                                                    calcularCredito += valorCredito;
                                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                    {
                                                        NumeroCuenta = parametro.cntpuc_numero,
                                                        DescripcionParametro = parametro.descripcion_parametro,
                                                        ValorDebito = 0,
                                                        ValorCredito = valorCredito
                                                    });
                                                }

                                                if (parametro.id_nombre_parametro == 9)
                                                {
                                                    valorDebito = valorSinIva;
                                                    calcularDebito += valorDebito;
                                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                    {
                                                        NumeroCuenta = parametro.cntpuc_numero,
                                                        DescripcionParametro = parametro.descripcion_parametro,
                                                        ValorDebito = valorDebito,
                                                        ValorCredito = 0
                                                    });
                                                }

                                                if (parametro.id_nombre_parametro == 14)
                                                {
                                                    valorDebito = descuentoPie;
                                                    calcularDebito += valorDebito;
                                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                    {
                                                        NumeroCuenta = parametro.cntpuc_numero,
                                                        DescripcionParametro = parametro.descripcion_parametro,
                                                        ValorDebito = valorDebito,
                                                        ValorCredito = 0
                                                    });
                                                }
                                            }
                                        }

                                        if (calcularCredito != calcularDebito)
                                        {
                                            TempData["mensajeError"] =
                                                "El documento no tiene los movimientos calculados correctamente, error linea "
                                                + (lineaError - 1) + ", plan mayor de error es " + planMayor +
                                                ", el perfil contable usado es " +
                                                parametrosCuentas.FirstOrDefault().descripcion
                                                + " codigo " + parametrosCuentas.FirstOrDefault().codigo;
                                            TempData["documento_descuadrado"] =
                                                "El documento no tiene los movimientos calculados correctamente";
                                            ViewBag.documentoDescuadrado = listaDescuadrados;
                                            ViewBag.calculoDebito = calcularDebito;
                                            ViewBag.calculoCredito = calcularCredito;

                                            icb_sysparameter buscarParametroSw =
                                                context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P23");
                                            string swParametro = buscarParametroSw != null
                                                ? buscarParametroSw.syspar_value
                                                : "1";
                                            int idTipoSw = Convert.ToInt32(swParametro);
                                            ViewBag.doc_registros = context.tp_doc_registros
                                                .Where(x => x.sw == idTipoSw).OrderBy(x => x.tpdoc_nombre);
                                            var provedores = (from pro in context.tercero_proveedor
                                                              join ter in context.icb_terceros
                                                                  on pro.tercero_id equals ter.tercero_id
                                                              select new
                                                              {
                                                                  idTercero = ter.tercero_id,
                                                                  nombreTErcero = ter.prinom_tercero,
                                                                  apellidosTercero = ter.apellido_tercero,
                                                                  razonSocial = ter.razon_social
                                                              }).OrderBy(x => x.nombreTErcero);
                                            List<SelectListItem> items = new List<SelectListItem>();
                                            foreach (var item in provedores)
                                            {
                                                string nombre =
                                                    item.nombreTErcero + " " + item.apellidosTercero + " " +
                                                    item.razonSocial;
                                                items.Add(new SelectListItem
                                                { Text = nombre, Value = item.idTercero.ToString() });
                                            }

                                            ViewBag.proveedor = items;
                                            var buscarTipoDocumento = (from tipoDocumento in context.tp_doc_registros
                                                                       where tipoDocumento.tipo == 1
                                                                       select new
                                                                       {
                                                                           tipoDocumento.tpdoc_id,
                                                                           nombre = "(" + tipoDocumento.prefijo + ") " +
                                                                                    tipoDocumento.tpdoc_nombre,
                                                                           tipoDocumento.tipo
                                                                       }).OrderBy(x => x.nombre).ToList();
                                            ViewBag.tipo_documentoFiltro = new SelectList(buscarTipoDocumento,
                                                "tpdoc_id", "nombre");
                                            ViewBag.condicion_pago = context.fpago_tercero;
                                            BuscarFavoritos(menu);
                                            return View();
                                        }
                                    }
                                    else
                                    {
                                        // Significa que no hay perfil contable asignado para la bodega y el tipo de documento seleccionado
                                        TempData["mensajeError"] =
                                            "No existe un perfil contable asignado para la bodega y tipo de documento seleccionados";
                                        return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
                                    }

                                    // Finalizacion de la validacion de los debitos y creditos
                                }
                            }
                        }
                        else
                        {
                            encabezado = false;
                        }
                    } // Aqui termina de leer el archivo plano

                    // El proveedor esta arriba, llega por request tambien

                    //var idTipoDocumento = 0;
                    int idCondicionPago = 0;
                    //var numeroCargue = 0;
                    int nitPago = 0;
                    try
                    {
                        nitPago = Convert.ToInt32(Request["txtNitPago"]);
                        // El tipo de documento llega por request tambien, esta arriba
                        //idCondicionPago = Convert.ToInt32(Request["selectCondicionPago"]);
                        idCondicionPago = buscarProveedor.fpago_id;
                        // numeroCargue = Convert.ToInt32(Request["txtNumeroCargue"]);

                        /// ------------------------------------------------------------------------------------------
                        /// Si llega hasta aqui significa que el archivo se leyo correctamente
                        /// ------------------------------------------------------------------------------------------
                        long numeroConsecutivo = 0;
                        ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
                        icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
                        numeroConsecutivoAux =
                            gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro, idTipoDocumento, codigoBodega);
                        //var numeroCargueAux = context.icb_doc_consecutivos.OrderByDescending(x => x.doccons_ano).FirstOrDefault(x => x.doccons_idtpdoc == idTipoDocumento && x.doccons_bodega == codigoBodega);

                        grupoconsecutivos grupoConsecutivo = context.grupoconsecutivos.FirstOrDefault(x =>
                            x.documento_id == idTipoDocumento && x.bodega_id == codigoBodega);
                        if (numeroConsecutivoAux != null)
                        {
                            numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                        }
                        else
                        {
                            TempData["mensajeError"] =
                                "No existe un numero consecutivo asignado para este tipo de documento y bodega";
                            return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
                        }


                        icb_arch_facturacion archivoFacturacion = new icb_arch_facturacion
                        {
                            arch_fac_nombre = txtfile.FileName,
                            arch_fac_fecha = DateTime.Now,
                            items = itemsCorrectos + itemsFallidos
                        };
                        context.icb_arch_facturacion.Add(archivoFacturacion);
                        int guardarArchFacturacion = context.SaveChanges();
                        // Significa que el nuevo registro o encabezado de archivo de facturacion se guardo correctamente
                        if (guardarArchFacturacion > 0)
                        {
                            int codigoArchFacturacion = context.icb_arch_facturacion
                                .OrderByDescending(x => x.arch_fac_id).First().arch_fac_id;
                            bodega_concesionario sinBodega = context.bodega_concesionario.FirstOrDefault(x => x.bodccs_cod == "000");

                            // ---------------------------------------------------------------------------------------------------
                            // Parametros que estaban quemados y ahora se toman de la tabla icb_sys_parameter
                            // ---------------------------------------------------------------------------------------------------
                            icb_sysparameter buscarParametroStatus =
                                context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P17");
                            string statusParametro =
                                buscarParametroStatus != null ? buscarParametroStatus.syspar_value : "0";
                            icb_sysparameter buscarParametroTpVh =
                                context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P3");
                            string tpHvParametro = buscarParametroTpVh != null ? buscarParametroTpVh.syspar_value : "4";
                            icb_sysparameter buscarParametroTpEvento =
                                context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P19");
                            string tpEventoParametro = buscarParametroTpEvento != null
                                ? buscarParametroTpEvento.syspar_value
                                : "11";
                            icb_sysparameter buscarParametroMarcaVh =
                                context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P5");
                            string marcaVhParametro = buscarParametroMarcaVh != null
                                ? buscarParametroMarcaVh.syspar_value
                                : "1";
                            //var buscarParametroEventoFact = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P4");
                            //var eventoFacturacionParametro = buscarParametroEventoFact != null ? buscarParametroEventoFact.syspar_value : "1";
                            int idEventoFacturacion = Convert.ToInt32(tpEventoParametro);

                            int tpvh_idAux = Convert.ToInt32(tpHvParametro);
                            //var id_eventoAux = Convert.ToInt32(tpEventoParametro);
                            int marcvh_idAux = Convert.ToInt32(marcaVhParametro);
                            ///////////////////////////////////////

                            foreach (CrearVehiculoExcel vehiculo in listaVehiculos)
                            {
                                short anoFacturacion = (short)vehiculo.fecfact_fabrica.Year;
                                short mesFacturacion = (short)vehiculo.fecfact_fabrica.Month;
                                icb_referencia buscarReferencia =
                                    context.icb_referencia.FirstOrDefault(x => x.ref_codigo == vehiculo.plan_mayor);
                                referencias_inven buscarReferenciaInven = context.referencias_inven.FirstOrDefault(x =>
                                    x.bodega == codigoBodega && x.codigo == vehiculo.plan_mayor &&
                                    x.ano == anoFacturacion && x.mes == mesFacturacion);
                                if (vehiculo.vehiculo_actualizar)
                                {
                                    icb_vehiculo buscarVehiculo =
                                        context.icb_vehiculo.FirstOrDefault(x => x.plan_mayor == vehiculo.plan_mayor);
                                    //context.Entry(buscarVehiculo).State = EntityState.Modified;
                                    //context.SaveChanges();

                                    if (buscarReferencia != null && buscarReferenciaInven != null)
                                    {
                                        buscarReferencia.modulo = "V";
                                        context.Entry(buscarReferencia).State = EntityState.Modified;
                                        //context.Entry(buscarReferenciaInven).State = EntityState.Modified;
                                        //context.SaveChanges();
                                    }
                                    else
                                    {
                                        modelo_vehiculo buscarModelo =
                                            context.modelo_vehiculo.FirstOrDefault(x =>
                                                x.modvh_codigo == vehiculo.modvh_id);
                                        icb_referencia referenciaNueva = new icb_referencia
                                        {
                                            ref_codigo = vehiculo.plan_mayor,
                                            ref_descripcion = buscarModelo != null ? buscarModelo.modvh_nombre : "",
                                            ref_valor_unitario = vehiculo.costosiniva_vh,
                                            ref_valor_total = vehiculo.costototal_vh,
                                            por_iva = (float)vehiculo.porcentajeIva,
                                            manejo_inv = true,
                                            ref_usuario_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            ref_fecha_creacion = DateTime.Now,
                                            modulo = "V"
                                        };
                                        context.icb_referencia.Add(referenciaNueva);

                                        referencias_inven referenciaInvenNueva = new referencias_inven
                                        {
                                            bodega = codigoBodega,
                                            codigo = vehiculo.plan_mayor,
                                            ano = (short)vehiculo.fecfact_fabrica.Year,
                                            mes = (short)vehiculo.fecfact_fabrica.Month,
                                            can_ini = 0,
                                            can_ent = 1,
                                            can_sal = 0,
                                            cos_ent = vehiculo.costosiniva_vh,
                                            can_com = 1,
                                            cos_com = vehiculo.costosiniva_vh,
                                            modulo = "V"
                                        };
                                        context.referencias_inven.Add(referenciaInvenNueva);
                                    }
                                }
                                else
                                {
                                    icb_vehiculo vehiculoNuevo = new icb_vehiculo
                                    {
                                        fecfact_fabrica = vehiculo.fecfact_fabrica,
                                        plan_mayor = vehiculo.plan_mayor,
                                        anio_vh = vehiculo.anio_vh,
                                        kmat_zvsk = vehiculo.kmat,
                                        modvh_id = vehiculo.modvh_id,
                                        colvh_id = vehiculo.colvh_id,
                                        vin = vehiculo.vin,
                                        nummot_vh = vehiculo.nummot_vh,
                                        numpedido_vh = vehiculo.numpedido_vh,
                                        costototal_vh = vehiculo.costototal_vh,
                                        arch_fact_id = codigoArchFacturacion,
                                        icbvhuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                        icbvhfec_creacion = DateTime.Now,
                                        id_bod = codigoBodega,
                                        tpvh_id = tpvh_idAux,
                                        id_evento = idEventoFacturacion,
                                        marcvh_id = marcvh_idAux,
                                        icbvh_estatus = statusParametro,
                                        proveedor_id = idProveedor,
                                        bodegaGM = vehiculo.bodegaGM,
                                        codigo_pago = vehiculo.codigoPago,
                                        Notas = vehiculo.notas,
                                        flota = vehiculo.flota,
                                        nuevo = true,
                                        iva_vh = vehiculo.porcentaje_iva
                                    };
                                    context.icb_vehiculo.Add(vehiculoNuevo);

                                    if (buscarReferencia == null)
                                    {
                                        modelo_vehiculo buscarModelo =
                                            context.modelo_vehiculo.FirstOrDefault(x =>
                                                x.modvh_codigo == vehiculo.modvh_id);
                                        icb_referencia referenciaNueva = new icb_referencia
                                        {
                                            ref_codigo = vehiculo.plan_mayor,
                                            ref_descripcion = buscarModelo != null ? buscarModelo.modvh_nombre : "",
                                            ref_valor_unitario = vehiculo.costosiniva_vh,
                                            ref_valor_total = vehiculo.costototal_vh,
                                            por_iva = (float)vehiculo.porcentajeIva,
                                            manejo_inv = true,
                                            ref_usuario_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            ref_fecha_creacion = DateTime.Now,
                                            modulo = "V"
                                        };
                                        context.icb_referencia.Add(referenciaNueva);
                                    }
                                    else
                                    {
                                        TempData["mensajeError"] =
                                            "El plan mayor " + vehiculo.plan_mayor +
                                            " ya existia con anterioridad en la tabla icb_referencia";
                                        return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
                                    }

                                    if (buscarReferenciaInven == null)
                                    {
                                        referencias_inven referenciaInvenNueva = new referencias_inven
                                        {
                                            bodega = codigoBodega,
                                            codigo = vehiculo.plan_mayor,
                                            ano = (short)vehiculo.fecfact_fabrica.Year,
                                            mes = (short)vehiculo.fecfact_fabrica.Month,
                                            can_ini = 0,
                                            can_ent = 1,
                                            can_sal = 0,
                                            cos_ent = vehiculo.costosiniva_vh,
                                            can_com = 1,
                                            cos_com = vehiculo.costosiniva_vh,
                                            modulo = "V"
                                        };
                                        context.referencias_inven.Add(referenciaInvenNueva);
                                    }
                                    else
                                    {
                                        TempData["mensajeError"] =
                                            "El plan mayor " + vehiculo.plan_mayor +
                                            "  ya existia con anterioridad en la tabla referencias_inven";
                                        return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
                                    }
                                }

                                int guardarVehiculo = context.SaveChanges();

                                // ----------------------------------------------------------------------------------
                                // Guardar registros en la tabla encab_documento 
                                // ----------------------------------------------------------------------------------
                                int diasVencimiento =
                                    context.fpago_tercero.FirstOrDefault(x => x.fpago_id == idCondicionPago)
                                        .dvencimiento ?? 0;

                                encab_documento encabezadoNuevo = new encab_documento
                                {
                                    tipo = idTipoDocumento,
                                    nit = idProveedor,
                                    nit_pago = nitPago,
                                    fecha = vehiculo.fecfact_fabrica,
                                    fpago_id = idCondicionPago,
                                    vencimiento = vehiculo.fecfact_fabrica.AddDays(diasVencimiento),
                                    valor_total = vehiculo.costototal_vh,
                                    iva = vehiculo.costototal_vh - vehiculo.costosiniva_vh,
                                    bodega = codigoBodega,
                                    estado = true,
                                    documento = vehiculo.plan_mayor,
                                    valor_mercancia = vehiculo.costosiniva_vh,
                                    pedido = vehiculo.numpedido_vh,
                                    numero = (int)numeroConsecutivo,
                                    fec_creacion = DateTime.Now,
                                    perfilcontable = buscarTipoCompra.idperfilcontable,
                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                    costo = vehiculo.costosiniva_vh
                                };

                                if (buscarPerfilTributario != null)
                                {
                                    if (buscarTipoDocRegistro != null)
                                    {
                                        if (buscarPerfilTributario.retfuente == "A")
                                        {
                                            if (buscarTipoDocRegistro.baseretencion <= vehiculo.costosiniva_vh)
                                            {
                                                decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                                encabezadoNuevo.retencion =
                                                    vehiculo.costosiniva_vh * porcentajeRetencion / 100;
                                                encabezadoNuevo.porcen_retencion = (float)porcentajeRetencion;
                                            }
                                        }

                                        if (buscarPerfilTributario.retiva == "A")
                                        {
                                            if (buscarTipoDocRegistro.baseiva <= vehiculo.costosiniva_vh)
                                            {
                                                decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                                encabezadoNuevo.retencion_iva =
                                                    vehiculo.iva_vh * porcentajeRetIva / 100;
                                                encabezadoNuevo.porcen_reteiva = (float)porcentajeRetIva;
                                            }
                                        }

                                        if (buscarPerfilTributario.retica == "A")
                                        {
                                            if (!actividadEconomicaValido)
                                            {
                                                if (buscarTipoDocRegistro.baseica <= vehiculo.costosiniva_vh)
                                                {
                                                    decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                                    encabezadoNuevo.retencion_ica =
                                                        vehiculo.costosiniva_vh * porcentajeRetIca / 1000;
                                                    encabezadoNuevo.porcen_retica = (float)porcentajeRetIca;
                                                }
                                            }
                                            else
                                            {
                                                encabezadoNuevo.retencion_ica =
                                                    vehiculo.costosiniva_vh * porcentajeReteICA / 1000;
                                                encabezadoNuevo.porcen_retica = (float)porcentajeReteICA;
                                            }
                                        }
                                    }
                                }

                                context.encab_documento.Add(encabezadoNuevo);
                                int guardarEncabDoc = context.SaveChanges();
                                encab_documento buscarUltimoEncabezado = context.encab_documento
                                    .OrderByDescending(x => x.idencabezado).FirstOrDefault();

                                //var buscaAnioModelo = context.anio_modelo.FirstOrDefault(x => x.codigo_modelo == vehiculo.modvh_id);
                                //var valorIva = buscaAnioModelo != null ? buscaAnioModelo.porcentaje_iva : 0;
                                //var valorImpuestoConsumo = buscaAnioModelo != null ? buscaAnioModelo.impuesto_consumo : 0;


                                // --------------------------------------------------------------------------------
                                // Aqui guardar lineas documento de la tabla lineas_documento
                                // --------------------------------------------------------------------------------
                                context.lineas_documento.Add(new lineas_documento
                                {
                                    codigo = vehiculo.plan_mayor,
                                    fec_creacion = DateTime.Now,
                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                    nit = nitPago,
                                    cantidad = 1,
                                    //valor_unitario = vehiculo.costototal_vh,
                                    valor_unitario = vehiculo.costosiniva_vh,
                                    bodega = codigoBodega,
                                    porcentaje_iva = (float)vehiculo.porcentajeIva,
                                    seq = 1,
                                    //impoconsumo = (float)vehiculo.impuesto_consumo,
                                    estado = true,
                                    fec = vehiculo.fecfact_fabrica,
                                    costo_unitario = vehiculo.costosiniva_vh,
                                    id_encabezado = buscarUltimoEncabezado != null
                                        ? buscarUltimoEncabezado.idencabezado
                                        : 0
                                });

                                // ----------------------------------------------------------------------------- 
                                // Aqui guardar movimiento contable de la tabla mov_contable
                                // -----------------------------------------------------------------------------
                                int idConceptoCompra = 0;
                                foreach (vconceptocompra item in tipoCompraVehiculos)
                                {
                                    if (item.codigo.ToUpper().Contains(vehiculo.codigoPago))
                                    {
                                        idConceptoCompra = item.id;
                                    }
                                }
                                //var buscarTipoCompra = context.vtipo_compra_vehiculos.FirstOrDefault(x => x.idtipocomprav == idConceptoCompra && x.idbodega == codigoBodega && x.iddoc == idTipoDocumento);
                                // Significa que hay un perfil contable encontrado
                                if (buscarTipoCompra != null)
                                {
                                    centro_costo centroValorCero =
                                        context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                                    int idCentroCero = centroValorCero != null
                                        ? Convert.ToInt32(centroValorCero.centcst_id)
                                        : 0;
                                    icb_terceros terceroValorCero =
                                        context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
                                    int idTerceroCero = centroValorCero != null
                                        ? Convert.ToInt32(terceroValorCero.tercero_id)
                                        : 0;

                                    List<perfil_cuentas_documento> parametrosCuentas = listaPerfilCuentas
                                        .Where(x => x.id_perfil == buscarTipoCompra.idperfilcontable).ToList();
                                    int secuencia = 1;
                                    foreach (perfil_cuentas_documento parametro in parametrosCuentas)
                                    {
                                        int tipoParametro = parametro.id_nombre_parametro;
                                        string CreditoDebito = "";

                                        cuenta_puc buscarCuenta =
                                            listaCuentasPucAfectables.FirstOrDefault(x =>
                                                x.cntpuc_id == parametro.cuenta);
                                        if (buscarCuenta != null)
                                        {
                                            decimal valorCredito = 0;
                                            decimal valorDebito = 0;
                                            bool seAgregaMovimiento = false;
                                            if (parametro.id_nombre_parametro == 1)
                                            {
                                                seAgregaMovimiento = true;
                                                decimal restarIcaIvaRetencion = 0;
                                                if (buscarPerfilTributario != null)
                                                {
                                                    if (buscarTipoDocRegistro != null)
                                                    {
                                                        if (buscarPerfilTributario.retfuente == "A")
                                                        {
                                                            if (buscarTipoDocRegistro.baseretencion <=
                                                                vehiculo.costosiniva_vh)
                                                            {
                                                                decimal porcentajeRetencion =
                                                                    (decimal)buscarTipoDocRegistro.retencion;
                                                                restarIcaIvaRetencion +=
                                                                    vehiculo.costosiniva_vh * porcentajeRetencion / 100;
                                                            }
                                                        }

                                                        if (buscarPerfilTributario.retiva == "A")
                                                        {
                                                            if (buscarTipoDocRegistro.baseiva <= vehiculo.iva_vh)
                                                            {
                                                                decimal porcentajeRetIva =
                                                                    (decimal)buscarTipoDocRegistro.retiva;
                                                                restarIcaIvaRetencion +=
                                                                    vehiculo.iva_vh * porcentajeRetIva / 100;
                                                            }
                                                        }

                                                        if (buscarPerfilTributario.retica == "A")
                                                        {
                                                            if (!actividadEconomicaValido)
                                                            {
                                                                if (buscarTipoDocRegistro.baseica <=
                                                                    vehiculo.costosiniva_vh)
                                                                {
                                                                    decimal porcentajeRetIca =
                                                                        (decimal)buscarTipoDocRegistro.retica;
                                                                    restarIcaIvaRetencion +=
                                                                        vehiculo.costosiniva_vh * porcentajeRetIca /
                                                                        1000;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                restarIcaIvaRetencion +=
                                                                    vehiculo.costosiniva_vh * porcentajeReteICA / 1000;
                                                            }
                                                        }
                                                    }
                                                }

                                                valorCredito =
                                                    vehiculo.costosiniva_vh + vehiculo.iva_vh - restarIcaIvaRetencion;
                                                CreditoDebito = "Credito";
                                                //valorDebito = ((vehiculo.costosiniva_vh + vehiculo.iva_vh) - restarIcaIvaRetencion);
                                                //valorCredito = ValorFactura - restarIcaIvaRetencion;
                                                //valorDebito = ValorFactura - restarIcaIvaRetencion;
                                            }

                                            if (parametro.id_nombre_parametro == 2)
                                            {
                                                seAgregaMovimiento = true;
                                                //valorCredito = vehiculo.iva_vh;
                                                valorDebito = vehiculo.iva_vh;
                                                CreditoDebito = "Debito";
                                            }

                                            if (parametro.id_nombre_parametro == 3)
                                            {
                                                if (buscarPerfilTributario != null)
                                                {
                                                    if (buscarTipoDocRegistro != null)
                                                    {
                                                        if (buscarPerfilTributario.retfuente == "A")
                                                        {
                                                            if (buscarTipoDocRegistro.baseretencion <=
                                                                vehiculo.costosiniva_vh)
                                                            {
                                                                seAgregaMovimiento = true;
                                                                decimal porcentajeRetencion =
                                                                    (decimal)buscarTipoDocRegistro.retencion;
                                                                valorCredito =
                                                                    vehiculo.costosiniva_vh * porcentajeRetencion / 100;
                                                                //valorDebito = (vehiculo.costosiniva_vh * porcentajeRetencion) / 100;
                                                                CreditoDebito = "Credito";
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            if (parametro.id_nombre_parametro == 4)
                                            {
                                                if (buscarPerfilTributario != null)
                                                {
                                                    if (buscarTipoDocRegistro != null)
                                                    {
                                                        if (buscarPerfilTributario.retiva == "A")
                                                        {
                                                            if (buscarTipoDocRegistro.baseiva <= vehiculo.costosiniva_vh
                                                            )
                                                            {
                                                                seAgregaMovimiento = true;
                                                                decimal porcentajeRetIva =
                                                                    (decimal)buscarTipoDocRegistro.retiva;
                                                                valorCredito = vehiculo.iva_vh * porcentajeRetIva / 100;
                                                                //valorDebito = (vehiculo.iva_vh * porcentajeRetIva) / 100;
                                                                CreditoDebito = "Credito";
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            if (parametro.id_nombre_parametro == 5)
                                            {
                                                if (buscarPerfilTributario != null)
                                                {
                                                    if (buscarTipoDocRegistro != null)
                                                    {
                                                        if (buscarPerfilTributario.retica == "A")
                                                        {
                                                            if (!actividadEconomicaValido)
                                                            {
                                                                if (buscarTipoDocRegistro.baseica <=
                                                                    vehiculo.costosiniva_vh)
                                                                {
                                                                    seAgregaMovimiento = true;
                                                                    decimal porcentajeRetIca =
                                                                        (decimal)buscarTipoDocRegistro.retica;
                                                                    valorCredito =
                                                                        vehiculo.costosiniva_vh * porcentajeRetIca /
                                                                        1000;
                                                                    CreditoDebito = "Credito";
                                                                }
                                                            }
                                                            else
                                                            {
                                                                seAgregaMovimiento = true;
                                                                valorCredito =
                                                                    vehiculo.costosiniva_vh * porcentajeReteICA / 1000;
                                                                valorDebito =
                                                                    vehiculo.costosiniva_vh * porcentajeReteICA / 1000;
                                                                CreditoDebito = "Credito";
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            if (parametro.id_nombre_parametro == 6)
                                            {
                                                //valorCredito = valorSinIva;
                                                valorDebito = fletes;
                                                CreditoDebito = "Debito";
                                            }

                                            if (parametro.id_nombre_parametro == 7)
                                            {
                                                valorCredito = fletes * porcentajeIvaFletes / 100;
                                                CreditoDebito = "Credito";
                                            }

                                            if (parametro.id_nombre_parametro == 9)
                                            {
                                                seAgregaMovimiento = true;
                                                //valorCredito = vehiculo.costosiniva_vh;
                                                valorDebito = vehiculo.costosiniva_vh;
                                                CreditoDebito = "Debito";
                                            }

                                            if (parametro.id_nombre_parametro == 14)
                                            {
                                                //valorCredito = valorSinIva;
                                                valorDebito = descuentoPie;
                                                CreditoDebito = "Debito";
                                            }

                                            if (seAgregaMovimiento)
                                            {
                                                mov_contable movimiento = new mov_contable
                                                {
                                                    basecontable = 0,
                                                    creditoniif = 0,
                                                    debitoniif = 0,
                                                    id_encab = buscarUltimoEncabezado != null
                                                    ? buscarUltimoEncabezado.idencabezado
                                                    : 0,
                                                    fec_creacion = DateTime.Now,
                                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                                    idparametronombre = tipoParametro,
                                                    cuenta = parametro.cuenta,
                                                    centro = parametro.centro,
                                                    fec = buscarUltimoEncabezado.fecha,
                                                    seq = secuencia
                                                };


                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                                    {
                                                        movimiento.debitoniif = valorDebito;
                                                    }

                                                    if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                                    {
                                                        movimiento.creditoniif = valorCredito;
                                                    }
                                                }
                                                else
                                                {
                                                    if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                                    {
                                                        movimiento.debito = valorDebito;
                                                    }

                                                    if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                                    {
                                                        movimiento.credito = valorCredito;
                                                    }
                                                }


                                                if (buscarTipoDocRegistro.aplicaniff)
                                                {
                                                    if (buscarCuenta.concepniff == 1)
                                                    {
                                                        if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                                        {
                                                            movimiento.debitoniif = valorDebito;
                                                            movimiento.debito = valorDebito;
                                                        }

                                                        if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                                        {
                                                            movimiento.credito = valorCredito;
                                                            movimiento.creditoniif = valorCredito;
                                                        }
                                                    }

                                                    if (buscarCuenta.concepniff == 4)
                                                    {
                                                        if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                                        {
                                                            movimiento.debitoniif = valorDebito;
                                                        }

                                                        if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                                        {
                                                            movimiento.creditoniif = valorCredito;
                                                        }
                                                    }

                                                    if (buscarCuenta.concepniff == 5)
                                                    {
                                                        if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                                        {
                                                            movimiento.debito = valorDebito;
                                                        }

                                                        if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                                        {
                                                            movimiento.credito = valorCredito;
                                                        }
                                                    }
                                                }


                                                if (buscarCuenta.manejabase)
                                                {
                                                    if (parametro.id_nombre_parametro == 4)
                                                    {
                                                        movimiento.basecontable = vehiculo.iva_vh;
                                                    }
                                                    else
                                                    {
                                                        movimiento.basecontable = vehiculo.costosiniva_vh;
                                                    }
                                                }

                                                if (buscarCuenta.tercero)
                                                {
                                                    movimiento.nit = nitPago;
                                                }

                                                if (buscarCuenta.documeto)
                                                {
                                                    movimiento.documento = vehiculo.plan_mayor;
                                                }

                                                movimiento.detalle =
                                                    "Compra vehiculos fact " + vehiculo.plan_mayor + " y serie " +
                                                    vehiculo.vin;
                                                context.mov_contable.Add(movimiento);
                                                secuencia++;

                                                DateTime fechaBuscada = buscarUltimoEncabezado.fecha;
                                                cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                                    x.centro == parametro.centro && x.cuenta == parametro.cuenta &&
                                                    x.nit == movimiento.nit && x.ano == fechaBuscada.Year &&
                                                    x.mes == fechaBuscada.Month);

                                                if (buscar_cuentas_valores != null)
                                                {
                                                    buscar_cuentas_valores.debito += movimiento.debito;
                                                    buscar_cuentas_valores.credito += movimiento.credito;
                                                    buscar_cuentas_valores.debitoniff += movimiento.debitoniif;
                                                    buscar_cuentas_valores.creditoniff += movimiento.creditoniif;
                                                    context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                                }
                                                else
                                                {
                                                    cuentas_valores crearCuentaValor = new cuentas_valores
                                                    {
                                                        saldo_ini = 0,
                                                        debito = 0,
                                                        credito = 0,
                                                        saldo_ininiff = 0,
                                                        debitoniff = 0,
                                                        creditoniff = 0,
                                                        ano = buscarUltimoEncabezado.fecha.Year,
                                                        mes = buscarUltimoEncabezado.fecha.Month,
                                                        cuenta = parametro.cuenta,
                                                        centro = parametro.centro,
                                                        nit = movimiento.nit
                                                    };
                                                    crearCuentaValor.debito = movimiento.debito;
                                                    crearCuentaValor.credito = movimiento.credito;
                                                    crearCuentaValor.debitoniff = movimiento.debitoniif;
                                                    crearCuentaValor.creditoniff = movimiento.creditoniif;
                                                    context.cuentas_valores.Add(crearCuentaValor);
                                                }
                                            }
                                        }
                                    }
                                }

                                int guardarLIneas = context.SaveChanges();
                                if (guardarEncabDoc > 0 && guardarLIneas > 0)
                                {
                                    int grupoId = grupoConsecutivo != null ? grupoConsecutivo.grupo : 0;
                                    List<grupoconsecutivos> gruposConsecutivos = context.grupoconsecutivos.Where(x => x.grupo == grupoId)
                                        .ToList();
                                    foreach (grupoconsecutivos grupo in gruposConsecutivos)
                                    {
                                        icb_doc_consecutivos buscarElemento = context.icb_doc_consecutivos.FirstOrDefault(x =>
                                            x.doccons_idtpdoc == idTipoDocumento &&
                                            x.doccons_bodega == grupo.bodega_id);
                                        buscarElemento.doccons_siguiente = buscarElemento.doccons_siguiente + 1;
                                        context.Entry(buscarElemento).State = EntityState.Modified;
                                    }

                                    context.SaveChanges();
                                    numeroConsecutivo++;
                                    //}
                                    //else {
                                    //numeroCargueAuxPorAnio.doccons_siguiente = numeroCargueAuxPorAnio.doccons_siguiente + 1;
                                    //context.Entry(numeroCargueAuxPorAnio).State = EntityState.Modified;
                                    //context.SaveChanges();
                                    //var buscarConsecPorAnio = context.icb_doc_consecutivos.OrderByDescending(x => x.doccons_ano).FirstOrDefault();
                                    //buscarConsecPorAnio.doccons_siguiente = buscarConsecPorAnio.doccons_siguiente + 1;
                                    //context.Entry(buscarConsecPorAnio).State = EntityState.Modified;
                                    //context.SaveChanges();
                                    //}
                                }

                                // Se agrega la trazabilidad del vehiculo nuevo, en este caso lo primero es facturacion
                                context.icb_vehiculo_eventos.Add(new icb_vehiculo_eventos
                                {
                                    evento_estado = true,
                                    eventofec_creacion = DateTime.Now,
                                    fechaevento = DateTime.Now,
                                    eventouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                    evento_nombre = "Facturacion",
                                    id_tpevento = idEventoFacturacion,
                                    bodega_id = codigoBodega,
                                    vin = vehiculo.vin,
                                    planmayor = vehiculo.plan_mayor
                                });
                                int guardarVehEventos = context.SaveChanges();
                            }

                            icb_arch_facturacion_log archLog = new icb_arch_facturacion_log
                            {
                                fact_log_fecha = DateTime.Now,
                                fact_log_itemscorrecto = itemsCorrectos,
                                fact_log_itemserror = itemsFallidos,
                                fact_log_nombrearchivo = txtfile.FileName,
                                fact_log_items = itemsCorrectos + itemsFallidos,
                                fact_log_log = log,
                                id_arch_facturacion = codigoArchFacturacion
                            };
                            context.icb_arch_facturacion_log.Add(archLog);

                            int guardarLogs = context.SaveChanges();

                            if (guardarLogs > 0)
                            {
                                TempData["mensaje"] =
                                    "La carga del archivo se realizo correctamente, registros cargados : " +
                                    itemsCorrectos + ", registros no cargados : " + itemsFallidos;
                                return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
                            }
                        }
                    }
                    catch (FormatException)
                    {
                        int gg = lineaError;
                        int g = numeroLinea;
                        TempData["mensajeError"] =
                            "Los datos ingresados para el cargue del archivo plano pueden estar mal escritos, por favor verifica";
                        return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
                        //Validacion para cuando no se asigna algun dato tipo entero al importar el archivo plano
                    }
                }
            }
            catch (Exception e)
            {
                TempData["mensajeError"] =
                    "Error al leer el archivo, verifique que el archivo sea el correcto o verifique la siguiente excepcion : " +
                    e.Message;
                return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
            }

            return RedirectToAction("FacturacionGM", "gestionVhNuevo", new { menu });
        }

        public JsonResult GetTotalVehiculosFiltro(int? cod_bodega, DateTime? fecha_desde, DateTime? fecha_hasta,
            int? evento_id)
        {
            // Filtro sin importar parametros
            if (cod_bodega == null && fecha_desde == null && fecha_hasta == null && evento_id == null)
            {
                var queryCargados = (from vehiculo in context.icb_vehiculo
                                     join modelo in context.modelo_vehiculo
                                         on vehiculo.modvh_id equals modelo.modvh_codigo
                                     join color in context.color_vehiculo
                                         on vehiculo.colvh_id equals color.colvh_id
                                     join evento in context.icb_tpeventos
                                         on vehiculo.id_evento equals evento.tpevento_id
                                     where vehiculo.nuevo == true
                                     select new
                                     {
                                         vehiculo.nummot_vh,
                                         modelo.modvh_nombre,
                                         color.colvh_nombre,
                                         vehiculo.vin,
                                         vehiculo.plac_vh,
                                         vehiculo.fecfact_fabrica,
                                         vehiculo.anio_vh,
                                         vehiculo.plan_mayor,
                                         vehiculo.icbvh_id,
                                         evento.tpevento_nombre,
                                         diasInventario = DbFunctions.DiffDays(vehiculo.fecfact_fabrica, DateTime.Now)
                                     }).ToList();
                var result2 = queryCargados.Select(x => new
                {
                    x.nummot_vh,
                    x.modvh_nombre,
                    x.colvh_nombre,
                    x.vin,
                    x.plac_vh,
                    fecfact_fabrica = x.fecfact_fabrica != null ? x.fecfact_fabrica.Value.ToString("dd/MM/yyyy") : null,
                    x.anio_vh,
                    x.plan_mayor,
                    x.icbvh_id,
                    x.tpevento_nombre,
                    x.diasInventario
                });
                return Json(result2, JsonRequestBehavior.AllowGet);
            }

            if (cod_bodega != null && fecha_desde == null && fecha_hasta == null && evento_id == null)
            {
                // Filtro para buscar solamente por codigo de la bodega
                var queryCargados = (from vehiculo in context.icb_vehiculo
                                     join modelo in context.modelo_vehiculo
                                         on vehiculo.modvh_id equals modelo.modvh_codigo
                                     join color in context.color_vehiculo
                                         on vehiculo.colvh_id equals color.colvh_id
                                     join evento in context.icb_tpeventos
                                         on vehiculo.id_evento equals evento.tpevento_id
                                     where vehiculo.id_bod == cod_bodega && vehiculo.nuevo == true
                                     select new
                                     {
                                         vehiculo.nummot_vh,
                                         modelo.modvh_nombre,
                                         color.colvh_nombre,
                                         vehiculo.vin,
                                         vehiculo.plac_vh,
                                         vehiculo.fecfact_fabrica,
                                         vehiculo.anio_vh,
                                         vehiculo.plan_mayor,
                                         vehiculo.icbvh_id,
                                         evento.tpevento_nombre,
                                         diasInventario = DbFunctions.DiffDays(vehiculo.fecfact_fabrica, DateTime.Now)
                                     }).ToList();
                var result2 = queryCargados.Select(x => new
                {
                    x.nummot_vh,
                    x.modvh_nombre,
                    x.colvh_nombre,
                    x.vin,
                    x.plac_vh,
                    fecfact_fabrica = x.fecfact_fabrica != null ? x.fecfact_fabrica.Value.ToString("dd/MM/yyyy") : null,
                    x.anio_vh,
                    x.plan_mayor,
                    x.icbvh_id,
                    x.tpevento_nombre,
                    x.diasInventario
                });
                return Json(result2, JsonRequestBehavior.AllowGet);
            }

            if (cod_bodega == null && fecha_desde != null && fecha_hasta != null &&
                evento_id == null || cod_bodega == null && fecha_desde != null && fecha_hasta == null &&
                                  evento_id == null
                                  || cod_bodega == null && fecha_desde == null && fecha_hasta != null &&
                                  evento_id == null)
            {
                // Filtro para buscar solamente por fecha
                // Cuando la fecha desde es nula 
                if (fecha_desde == null)
                {
                    DateTime fechaHasta = Convert.ToDateTime(fecha_hasta);
                    var queryCargados = (from vehiculo in context.icb_vehiculo
                                         join modelo in context.modelo_vehiculo
                                             on vehiculo.modvh_id equals modelo.modvh_codigo
                                         join color in context.color_vehiculo
                                             on vehiculo.colvh_id equals color.colvh_id
                                         join evento in context.icb_tpeventos
                                             on vehiculo.id_evento equals evento.tpevento_id
                                         where vehiculo.fecfact_fabrica <= fechaHasta
                                               && vehiculo.nuevo == true
                                         select new
                                         {
                                             vehiculo.nummot_vh,
                                             modelo.modvh_nombre,
                                             color.colvh_nombre,
                                             vehiculo.vin,
                                             vehiculo.plac_vh,
                                             vehiculo.fecfact_fabrica,
                                             vehiculo.anio_vh,
                                             vehiculo.plan_mayor,
                                             vehiculo.icbvh_id,
                                             evento.tpevento_nombre,
                                             diasInventario = DbFunctions.DiffDays(vehiculo.fecfact_fabrica, DateTime.Now)
                                         }).ToList();
                    var result2 = queryCargados.Select(x => new
                    {
                        x.nummot_vh,
                        x.modvh_nombre,
                        x.colvh_nombre,
                        x.vin,
                        x.plac_vh,
                        fecfact_fabrica = x.fecfact_fabrica != null
                            ? x.fecfact_fabrica.Value.ToString("dd/MM/yyyy")
                            : null,
                        x.anio_vh,
                        x.plan_mayor,
                        x.icbvh_id,
                        x.tpevento_nombre,
                        x.diasInventario
                    });
                    return Json(result2, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    // Cuando la fecha desde es nula
                    DateTime fechaDesde = Convert.ToDateTime(fecha_desde);
                    DateTime fechaHasta = fecha_hasta != null ? Convert.ToDateTime(fecha_hasta) : DateTime.Now;
                    var queryCargados = (from vehiculo in context.icb_vehiculo
                                         join modelo in context.modelo_vehiculo
                                             on vehiculo.modvh_id equals modelo.modvh_codigo
                                         join color in context.color_vehiculo
                                             on vehiculo.colvh_id equals color.colvh_id
                                         join evento in context.icb_tpeventos
                                             on vehiculo.id_evento equals evento.tpevento_id
                                         where vehiculo.fecfact_fabrica >= fechaDesde && vehiculo.fecfact_fabrica <= fechaHasta
                                                                                      && vehiculo.nuevo == true
                                         select new
                                         {
                                             vehiculo.nummot_vh,
                                             modelo.modvh_nombre,
                                             color.colvh_nombre,
                                             vehiculo.vin,
                                             vehiculo.plac_vh,
                                             vehiculo.fecfact_fabrica,
                                             vehiculo.anio_vh,
                                             vehiculo.plan_mayor,
                                             vehiculo.icbvh_id,
                                             evento.tpevento_nombre,
                                             diasInventario = DbFunctions.DiffDays(vehiculo.fecfact_fabrica, DateTime.Now)
                                         }).ToList();
                    var result2 = queryCargados.Select(x => new
                    {
                        x.nummot_vh,
                        x.modvh_nombre,
                        x.colvh_nombre,
                        x.vin,
                        x.plac_vh,
                        fecfact_fabrica = x.fecfact_fabrica != null
                            ? x.fecfact_fabrica.Value.ToString("dd/MM/yyyy")
                            : null,
                        x.anio_vh,
                        x.plan_mayor,
                        x.icbvh_id,
                        x.tpevento_nombre,
                        x.diasInventario
                    });
                    return Json(result2, JsonRequestBehavior.AllowGet);
                }
            }

            if (cod_bodega != null && fecha_desde != null && fecha_hasta != null &&
                evento_id == null || cod_bodega != null && fecha_desde == null && fecha_hasta != null &&
                                  evento_id == null
                                  || cod_bodega != null && fecha_desde != null && fecha_hasta == null &&
                                  evento_id == null)
            {
                // Filtro cuando se asigna una bodega con fecha
                if (fecha_desde == null)
                {
                    DateTime fechaHasta = Convert.ToDateTime(fecha_hasta);
                    var queryCargados = (from vehiculo in context.icb_vehiculo
                                         join modelo in context.modelo_vehiculo
                                             on vehiculo.modvh_id equals modelo.modvh_codigo
                                         join color in context.color_vehiculo
                                             on vehiculo.colvh_id equals color.colvh_id
                                         join evento in context.icb_tpeventos
                                             on vehiculo.id_evento equals evento.tpevento_id
                                         where vehiculo.id_bod == cod_bodega && vehiculo.fecfact_fabrica <= fechaHasta
                                                                             && vehiculo.nuevo == true
                                         select new
                                         {
                                             vehiculo.nummot_vh,
                                             modelo.modvh_nombre,
                                             color.colvh_nombre,
                                             vehiculo.vin,
                                             vehiculo.plac_vh,
                                             vehiculo.fecfact_fabrica,
                                             vehiculo.anio_vh,
                                             vehiculo.plan_mayor,
                                             vehiculo.icbvh_id,
                                             evento.tpevento_nombre,
                                             diasInventario = DbFunctions.DiffDays(vehiculo.fecfact_fabrica, DateTime.Now)
                                         }).ToList();
                    var result2 = queryCargados.Select(x => new
                    {
                        x.nummot_vh,
                        x.modvh_nombre,
                        x.colvh_nombre,
                        x.vin,
                        x.plac_vh,
                        fecfact_fabrica = x.fecfact_fabrica != null
                            ? x.fecfact_fabrica.Value.ToString("dd/MM/yyyy")
                            : null,
                        x.anio_vh,
                        x.plan_mayor,
                        x.icbvh_id,
                        x.tpevento_nombre,
                        x.diasInventario
                    });
                    return Json(result2, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DateTime fechaDesde = Convert.ToDateTime(fecha_desde);
                    DateTime fechaHasta = fecha_hasta != null ? Convert.ToDateTime(fecha_hasta) : DateTime.Now;
                    var queryCargados = (from vehiculo in context.icb_vehiculo
                                         join modelo in context.modelo_vehiculo
                                             on vehiculo.modvh_id equals modelo.modvh_codigo
                                         join color in context.color_vehiculo
                                             on vehiculo.colvh_id equals color.colvh_id
                                         join evento in context.icb_tpeventos
                                             on vehiculo.id_evento equals evento.tpevento_id
                                         where vehiculo.id_bod == cod_bodega && vehiculo.fecfact_fabrica >= fechaDesde &&
                                               vehiculo.fecfact_fabrica <= fechaHasta
                                               && vehiculo.nuevo == true
                                         select new
                                         {
                                             vehiculo.nummot_vh,
                                             modelo.modvh_nombre,
                                             color.colvh_nombre,
                                             vehiculo.vin,
                                             vehiculo.plac_vh,
                                             vehiculo.fecfact_fabrica,
                                             vehiculo.anio_vh,
                                             vehiculo.plan_mayor,
                                             vehiculo.icbvh_id,
                                             evento.tpevento_nombre,
                                             diasInventario = DbFunctions.DiffDays(vehiculo.fecfact_fabrica, DateTime.Now)
                                         }).ToList();

                    var result2 = queryCargados.Select(x => new
                    {
                        x.nummot_vh,
                        x.modvh_nombre,
                        x.colvh_nombre,
                        x.vin,
                        x.plac_vh,
                        fecfact_fabrica = x.fecfact_fabrica != null
                            ? x.fecfact_fabrica.Value.ToString("dd/MM/yyyy")
                            : null,
                        x.anio_vh,
                        x.plan_mayor,
                        x.icbvh_id,
                        x.tpevento_nombre,
                        x.diasInventario
                    });
                    return Json(result2, JsonRequestBehavior.AllowGet);
                }
            }

            if (cod_bodega == null && fecha_desde == null && fecha_hasta == null && evento_id != null)
            {
                // Filtro solo por el estado
                var queryCargados = (from vehiculo in context.icb_vehiculo
                                     join modelo in context.modelo_vehiculo
                                         on vehiculo.modvh_id equals modelo.modvh_codigo
                                     join color in context.color_vehiculo
                                         on vehiculo.colvh_id equals color.colvh_id
                                     join evento in context.icb_tpeventos
                                         on vehiculo.id_evento equals evento.tpevento_id
                                     where vehiculo.id_evento == evento_id
                                           && vehiculo.nuevo == true
                                     select new
                                     {
                                         vehiculo.nummot_vh,
                                         modelo.modvh_nombre,
                                         color.colvh_nombre,
                                         vehiculo.vin,
                                         vehiculo.plac_vh,
                                         vehiculo.fecfact_fabrica,
                                         vehiculo.anio_vh,
                                         vehiculo.plan_mayor,
                                         vehiculo.icbvh_id,
                                         evento.tpevento_nombre,
                                         diasInventario = DbFunctions.DiffDays(vehiculo.fecfact_fabrica, DateTime.Now)
                                     }).ToList();

                var result2 = queryCargados.Select(x => new
                {
                    x.nummot_vh,
                    x.modvh_nombre,
                    x.colvh_nombre,
                    x.vin,
                    x.plac_vh,
                    fecfact_fabrica = x.fecfact_fabrica != null ? x.fecfact_fabrica.Value.ToString("dd/MM/yyyy") : null,
                    x.anio_vh,
                    x.plan_mayor,
                    x.icbvh_id,
                    x.tpevento_nombre,
                    x.diasInventario
                });
                return Json(result2, JsonRequestBehavior.AllowGet);
            }

            if (cod_bodega == null && fecha_desde != null && fecha_hasta != null &&
                evento_id != null || cod_bodega == null && fecha_desde == null && fecha_hasta != null &&
                                  evento_id != null
                                  || cod_bodega == null && fecha_desde != null && fecha_hasta == null &&
                                  evento_id != null)
            {
                // Filtro por el estado y por la fecha
                if (fecha_desde == null)
                {
                    DateTime fechaHasta = fecha_hasta != null ? Convert.ToDateTime(fecha_hasta) : DateTime.Now;
                    var queryCargados = (from vehiculo in context.icb_vehiculo
                                         join modelo in context.modelo_vehiculo
                                             on vehiculo.modvh_id equals modelo.modvh_codigo
                                         join color in context.color_vehiculo
                                             on vehiculo.colvh_id equals color.colvh_id
                                         join evento in context.icb_tpeventos
                                             on vehiculo.id_evento equals evento.tpevento_id
                                         where vehiculo.id_evento == evento_id && vehiculo.fecfact_fabrica <= fechaHasta
                                                                               && vehiculo.nuevo == true
                                         select new
                                         {
                                             vehiculo.nummot_vh,
                                             modelo.modvh_nombre,
                                             color.colvh_nombre,
                                             vehiculo.vin,
                                             vehiculo.plac_vh,
                                             vehiculo.fecfact_fabrica,
                                             vehiculo.anio_vh,
                                             vehiculo.plan_mayor,
                                             vehiculo.icbvh_id,
                                             evento.tpevento_nombre,
                                             diasInventario = DbFunctions.DiffDays(vehiculo.fecfact_fabrica, DateTime.Now)
                                         }).ToList();
                    var result2 = queryCargados.Select(x => new
                    {
                        x.nummot_vh,
                        x.modvh_nombre,
                        x.colvh_nombre,
                        x.vin,
                        x.plac_vh,
                        fecfact_fabrica = x.fecfact_fabrica != null
                            ? x.fecfact_fabrica.Value.ToString("dd/MM/yyyy")
                            : null,
                        x.anio_vh,
                        x.plan_mayor,
                        x.icbvh_id,
                        x.tpevento_nombre,
                        x.diasInventario
                    });
                    return Json(result2, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DateTime fechaDesde = Convert.ToDateTime(fecha_desde);
                    DateTime fechaHasta = fecha_hasta != null ? Convert.ToDateTime(fecha_hasta) : DateTime.Now;
                    var queryCargados = (from vehiculo in context.icb_vehiculo
                                         join modelo in context.modelo_vehiculo
                                             on vehiculo.modvh_id equals modelo.modvh_codigo
                                         join color in context.color_vehiculo
                                             on vehiculo.colvh_id equals color.colvh_id
                                         join evento in context.icb_tpeventos
                                             on vehiculo.id_evento equals evento.tpevento_id
                                         where vehiculo.id_evento == evento_id && vehiculo.fecfact_fabrica >= fechaDesde &&
                                               vehiculo.fecfact_fabrica <= fechaHasta
                                               && vehiculo.nuevo == true
                                         select new
                                         {
                                             vehiculo.nummot_vh,
                                             modelo.modvh_nombre,
                                             color.colvh_nombre,
                                             vehiculo.vin,
                                             vehiculo.plac_vh,
                                             vehiculo.fecfact_fabrica,
                                             vehiculo.anio_vh,
                                             vehiculo.plan_mayor,
                                             vehiculo.icbvh_id,
                                             evento.tpevento_nombre,
                                             diasInventario = DbFunctions.DiffDays(vehiculo.fecfact_fabrica, DateTime.Now)
                                         }).ToList();
                    var result2 = queryCargados.Select(x => new
                    {
                        x.nummot_vh,
                        x.modvh_nombre,
                        x.colvh_nombre,
                        x.vin,
                        x.plac_vh,
                        fecfact_fabrica = x.fecfact_fabrica != null
                            ? x.fecfact_fabrica.Value.ToString("dd/MM/yyyy")
                            : null,
                        x.anio_vh,
                        x.plan_mayor,
                        x.icbvh_id,
                        x.tpevento_nombre,
                        x.diasInventario
                    });
                    return Json(result2, JsonRequestBehavior.AllowGet);
                }
            }

            if (cod_bodega != null && fecha_desde == null && fecha_hasta == null && evento_id != null)
            {
                // Filtro por estado y por bodega
                var queryCargados = (from vehiculo in context.icb_vehiculo
                                     join modelo in context.modelo_vehiculo
                                         on vehiculo.modvh_id equals modelo.modvh_codigo
                                     join color in context.color_vehiculo
                                         on vehiculo.colvh_id equals color.colvh_id
                                     join evento in context.icb_tpeventos
                                         on vehiculo.id_evento equals evento.tpevento_id
                                     where vehiculo.id_evento == evento_id && vehiculo.id_bod == cod_bodega
                                                                           && vehiculo.nuevo == true
                                     select new
                                     {
                                         vehiculo.nummot_vh,
                                         modelo.modvh_nombre,
                                         color.colvh_nombre,
                                         vehiculo.vin,
                                         vehiculo.plac_vh,
                                         vehiculo.fecfact_fabrica,
                                         vehiculo.anio_vh,
                                         vehiculo.plan_mayor,
                                         vehiculo.icbvh_id,
                                         evento.tpevento_nombre,
                                         diasInventario = DbFunctions.DiffDays(vehiculo.fecfact_fabrica, DateTime.Now)
                                     }).ToList();
                var result2 = queryCargados.Select(x => new
                {
                    x.nummot_vh,
                    x.modvh_nombre,
                    x.colvh_nombre,
                    x.vin,
                    x.plac_vh,
                    fecfact_fabrica = x.fecfact_fabrica != null ? x.fecfact_fabrica.Value.ToString("dd/MM/yyyy") : null,
                    x.anio_vh,
                    x.plan_mayor,
                    x.icbvh_id,
                    x.tpevento_nombre,
                    x.diasInventario
                });
                return Json(result2, JsonRequestBehavior.AllowGet);
            }

            if (cod_bodega != null && fecha_desde != null && fecha_hasta != null && evento_id != null)
            {
                //Filtro por los tres parametros
                if (fecha_desde == null)
                {
                    DateTime fechaHasta = fecha_hasta != null ? Convert.ToDateTime(fecha_hasta) : DateTime.Now;
                    var queryCargados = (from vehiculo in context.icb_vehiculo
                                         join modelo in context.modelo_vehiculo
                                             on vehiculo.modvh_id equals modelo.modvh_codigo
                                         join color in context.color_vehiculo
                                             on vehiculo.colvh_id equals color.colvh_id
                                         join evento in context.icb_tpeventos
                                             on vehiculo.id_evento equals evento.tpevento_id
                                         where vehiculo.id_evento == evento_id && vehiculo.fecfact_fabrica <= fechaHasta &&
                                               vehiculo.id_bod == cod_bodega
                                               && vehiculo.nuevo == true
                                         select new
                                         {
                                             vehiculo.nummot_vh,
                                             modelo.modvh_nombre,
                                             color.colvh_nombre,
                                             vehiculo.vin,
                                             vehiculo.plac_vh,
                                             vehiculo.fecfact_fabrica,
                                             vehiculo.anio_vh,
                                             vehiculo.plan_mayor,
                                             vehiculo.icbvh_id,
                                             evento.tpevento_nombre,
                                             diasInventario = DbFunctions.DiffDays(vehiculo.fecfact_fabrica, DateTime.Now)
                                         }).ToList();
                    var result2 = queryCargados.Select(x => new
                    {
                        x.nummot_vh,
                        x.modvh_nombre,
                        x.colvh_nombre,
                        x.vin,
                        x.plac_vh,
                        fecfact_fabrica = x.fecfact_fabrica != null
                            ? x.fecfact_fabrica.Value.ToString("dd/MM/yyyy")
                            : null,
                        x.anio_vh,
                        x.plan_mayor,
                        x.icbvh_id,
                        x.tpevento_nombre,
                        x.diasInventario
                    });
                    return Json(result2, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    DateTime fechaDesde = Convert.ToDateTime(fecha_desde);
                    DateTime fechaHasta = fecha_hasta != null ? Convert.ToDateTime(fecha_hasta) : DateTime.Now;
                    var queryCargados = (from vehiculo in context.icb_vehiculo
                                         join modelo in context.modelo_vehiculo
                                             on vehiculo.modvh_id equals modelo.modvh_codigo
                                         join color in context.color_vehiculo
                                             on vehiculo.colvh_id equals color.colvh_id
                                         join evento in context.icb_tpeventos
                                             on vehiculo.id_evento equals evento.tpevento_id
                                         where vehiculo.id_evento == evento_id && vehiculo.fecfact_fabrica >= fechaDesde &&
                                               vehiculo.fecfact_fabrica <= fechaHasta && vehiculo.id_bod == cod_bodega
                                               && vehiculo.nuevo == true
                                         select new
                                         {
                                             vehiculo.nummot_vh,
                                             modelo.modvh_nombre,
                                             color.colvh_nombre,
                                             vehiculo.vin,
                                             vehiculo.plac_vh,
                                             vehiculo.fecfact_fabrica,
                                             vehiculo.anio_vh,
                                             vehiculo.plan_mayor,
                                             vehiculo.icbvh_id,
                                             evento.tpevento_nombre,
                                             diasInventario = DbFunctions.DiffDays(vehiculo.fecfact_fabrica, DateTime.Now)
                                         }).ToList();
                    var result2 = queryCargados.Select(x => new
                    {
                        x.nummot_vh,
                        x.modvh_nombre,
                        x.colvh_nombre,
                        x.vin,
                        x.plac_vh,
                        fecfact_fabrica = x.fecfact_fabrica != null
                            ? x.fecfact_fabrica.Value.ToString("dd/MM/yyyy")
                            : null,
                        x.anio_vh,
                        x.plan_mayor,
                        x.icbvh_id,
                        x.tpevento_nombre,
                        x.diasInventario
                    });
                    return Json(result2, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCantidadModelo(string idModelo)
        {
            var vehiculosLista = (from vehiculos in context.icb_vehiculo
                                  join modelo in context.modelo_vehiculo
                                      on vehiculos.modvh_id equals modelo.modvh_codigo
                                  join color in context.color_vehiculo
                                      on vehiculos.colvh_id equals color.colvh_id
                                  where vehiculos.modvh_id == idModelo
                                  select new
                                  {
                                      vehiculos.vin,
                                      modelo.modvh_nombre,
                                      vehiculos.anio_vh,
                                      color.colvh_nombre,
                                      diasInventario = DbFunctions.DiffDays(vehiculos.fecfact_fabrica, DateTime.Now),
                                      vehiculos.fecfact_fabrica,
                                      vehiculos.plan_mayor
                                  }).ToList();
            var data = vehiculosLista.Select(x => new
            {
                x.vin,
                x.modvh_nombre,
                x.anio_vh,
                x.colvh_nombre,
                x.diasInventario,
                fecfact_fabrica = x.fecfact_fabrica != null ? x.fecfact_fabrica.Value.ToString("yyyy/MM/dd") : null,
                x.plan_mayor
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCantidadRangoDias(string rango)
        {
            if (rango == "0-30")
            {
                var data = (from vh in context.icb_vehiculo
                            join modelo in context.modelo_vehiculo
                                on vh.modvh_id equals modelo.modvh_codigo
                            where DbFunctions.DiffDays(vh.fecfact_fabrica, DateTime.Now) <= 30
                            select new
                            {
                                vh.vin,
                                modelo.modvh_nombre
                            }).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            if (rango == "30-60")
            {
                var data = (from vh in context.icb_vehiculo
                            join modelo in context.modelo_vehiculo
                                on vh.modvh_id equals modelo.modvh_codigo
                            where DbFunctions.DiffDays(vh.fecfact_fabrica, DateTime.Now) <= 60 &&
                                  DbFunctions.DiffDays(vh.fecfact_fabrica, DateTime.Now) > 30
                            select new
                            {
                                vh.vin,
                                modelo.modvh_nombre
                            }).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            if (rango == "60-90")
            {
                var data = (from vh in context.icb_vehiculo
                            join modelo in context.modelo_vehiculo
                                on vh.modvh_id equals modelo.modvh_codigo
                            where DbFunctions.DiffDays(vh.fecfact_fabrica, DateTime.Now) <= 90 &&
                                  DbFunctions.DiffDays(vh.fecfact_fabrica, DateTime.Now) > 60
                            select new
                            {
                                vh.vin,
                                modelo.modvh_nombre
                            }).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            if (rango == "90Mas")
            {
                var data = (from vh in context.icb_vehiculo
                            join modelo in context.modelo_vehiculo
                                on vh.modvh_id equals modelo.modvh_codigo
                            where DbFunctions.DiffDays(vh.fecfact_fabrica, DateTime.Now) > 90
                            select new
                            {
                                vh.vin,
                                modelo.modvh_nombre
                            }).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCantidadVehiculos()
        {
            //se quita filtro de vehiculos del mes
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            //Validamos que el usuario loguado tenga el rol y el permiso para ver toda la informacion
            /*int permiso = (from u in context.users
                           join r in context.rols
                               on u.rol_id equals r.rol_id
                           join ra in context.rolacceso
                               on r.rol_id equals ra.idrol
                           where u.user_id == usuario && u.rol_id == 4 && ra.idpermiso == 3
                           select new
                           {
                               u.user_id,
                               u.rol_id,
                               r.rol_nombre,
                               ra.idpermiso
                           }).Count();*/

            int permiso = 1;
            if (permiso > 0)
            {
                string sDate = DateTime.Now.ToString();
                DateTime datevalue = Convert.ToDateTime(sDate);
                int mn = datevalue.Month;
                int yy = datevalue.Year;
                var modelos = context.vpedido.Where(d => d.planmayor != null && d.planmayor != string.Empty && d.anulado == false).ToList();
                List<string> datdistinctct = modelos.GroupBy(d => d.planmayor)
                    .Where(p => p.Key != null && p.Key != string.Empty).Select(d => d.Key).ToList();

                var cantModelos = (from vehiculos in context.vw_referencias_total
                                   where vehiculos.nuevo == true && vehiculos.stock > 0 && vehiculos.ano == yy && vehiculos.mes == mn && !datdistinctct.Contains(vehiculos.codigo)
                                   group vehiculos by new { vehiculos.modvh_id, vehiculos.modvh_nombre }
                    into modeloGrupo
                                   select new
                                   {
                                       modvh_codigo = modeloGrupo.Key.modvh_id,
                                       modeloGrupo.Key.modvh_nombre,
                                       modvh_cantidad = modeloGrupo.Count()
                                   }).ToList();

                int totalModelos = (from vehiculos in context.vw_referencias_total
                                    where vehiculos.nuevo == true && vehiculos.stock > 0 && vehiculos.ano == yy && vehiculos.mes == mn && !datdistinctct.Contains(vehiculos.codigo)
                                    select new
                                    {
                                        vehiculos.modvh_id,
                                        vehiculos.modvh_nombre
                                    }).Count();

                var datos = new
                {
                    cantModelos,
                    totalModelos
                };

                return Json(datos, JsonRequestBehavior.AllowGet);
                //var data = context.vw_referencias_total.ToList();
                //return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                string sDate = DateTime.Now.ToString();
                DateTime datevalue = Convert.ToDateTime(sDate);
                int mn = datevalue.Month;
                int yy = datevalue.Year;
                //Creamos un distinct para validar los datos Leo 23/09/2019
                List<string> datdistinctct = context.vpedido.GroupBy(d => d.planmayor)
                    .Where(p => p.Key != null && p.Key != string.Empty).Select(d => d.Key).ToList();
                var cantModelos = (from vehiculos in context.vw_referencias_total
                                   where vehiculos.nuevo == true && vehiculos.stock > 0 && vehiculos.ano == yy &&
                                         vehiculos.mes == mn && vehiculos.bodega == bodegaActual &&
                                         !datdistinctct.Contains(vehiculos.codigo)
                                   group vehiculos by new { vehiculos.modvh_id, vehiculos.modvh_nombre }
                    into modeloGrupo
                                   select new
                                   {
                                       modvh_codigo = modeloGrupo.Key.modvh_id,
                                       modeloGrupo.Key.modvh_nombre,
                                       modvh_cantidad = modeloGrupo.Count()
                                   }).ToList();

                int totalModelos = (from vehiculos in context.vw_referencias_total
                                    where vehiculos.nuevo == true && vehiculos.stock > 0 && vehiculos.ano == yy &&
                                          vehiculos.mes == mn && vehiculos.bodega == bodegaActual &&
                                          !datdistinctct.Contains(vehiculos.codigo)
                                    select new
                                    {
                                        vehiculos.modvh_id,
                                        vehiculos.modvh_nombre
                                    }).Count();

                var datos = new
                {
                    cantModelos,
                    totalModelos
                };
                return Json(datos, JsonRequestBehavior.AllowGet);
                //return Json(permiso, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetCantidadesPorDias()
        {
            var cantidades = new
            {
                Dias30 = (from vh in context.icb_vehiculo
                          where DbFunctions.DiffDays(vh.fecfact_fabrica, DateTime.Now) <= 30
                          select vh).Count(),
                Dias60 = (from vh in context.icb_vehiculo
                          where DbFunctions.DiffDays(vh.fecfact_fabrica, DateTime.Now) <= 60 &&
                                DbFunctions.DiffDays(vh.fecfact_fabrica, DateTime.Now) > 30
                          select vh).Count(),
                Dias90 = (from vh in context.icb_vehiculo
                          where DbFunctions.DiffDays(vh.fecfact_fabrica, DateTime.Now) <= 90 &&
                                DbFunctions.DiffDays(vh.fecfact_fabrica, DateTime.Now) > 60
                          select vh).Count(),
                Dias90Mas = (from vh in context.icb_vehiculo
                             where DbFunctions.DiffDays(vh.fecfact_fabrica, DateTime.Now) > 90
                             select vh).Count()
            };
            return Json(cantidades, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarVehiculosNuevosCant(string texto)
        {
            // Si la busqueda es por fecha entonces hay que hacer el parseo a fecha fuera de la consulta linq
            DateTime parseo = new DateTime();
            try
            {
                parseo = DateTime.Parse(texto);
            }
            catch (FormatException)
            {
                // Cuando el campo de busqueda no tiene formato de fecha se captura la excepcion
            }

            IQueryable<int> test = from icb in context.icb_vehiculo
                                   join mar in context.marca_vehiculo
                                       on icb.marcvh_id equals mar.marcvh_id
                                   join mod in context.modelo_vehiculo
                                       on icb.modvh_id equals mod.modvh_codigo
                                   join col in context.color_vehiculo
                                       on icb.colvh_id equals col.colvh_id
                                   select icb.icbvh_id;

            int result = (from icb in context.icb_vehiculo
                              //join mar in context.marca_vehiculo
                              //on icb.marcvh_id equals mar.marcvh_id
                              //join est in context.estilo_vehiculo
                              //on icb.estvh_id equals est.estvh_id
                          join mod in context.modelo_vehiculo
                              on icb.modvh_id equals mod.modvh_codigo
                          join col in context.color_vehiculo
                              on icb.colvh_id equals col.colvh_id
                          where ( //mar.marcvh_nombre.ToString().Contains(texto)
                                    mod.modvh_nombre.Contains(texto)
                                    //|| est.estvh_nombre.ToString().Contains(texto)
                                    || col.colvh_nombre.Contains(texto)
                                    || icb.nummot_vh.Contains(texto)
                                    || icb.vin.Contains(texto)
                                    || icb.plac_vh.Contains(texto)
                                    || icb.fecfact_fabrica == parseo
                                    || icb.anio_vh.ToString().Contains(texto)
                                    || icb.id_bod.ToString().Contains(texto)
                                    || icb.plan_mayor.Contains(texto)) &&
                                icb.tpvh_id == 4
                          orderby icb.fecfact_fabrica descending
                          select new
                          {
                              //marca = mar.marcvh_nombre,
                              motor = icb.nummot_vh,
                              //estilo = est.estvh_nombre,
                              modelo = mod.modvh_nombre,
                              color = col.colvh_nombre,
                              serie = icb.vin,
                              placa = icb.plac_vh,
                              fechaCompra = icb.fecfact_fabrica,
                              anio = icb.anio_vh,
                              codBodega = icb.id_bod,
                              planMayor = icb.plan_mayor,
                              id = icb.icbvh_id
                          }).ToList().Count;

            var resultAux = (from icb in context.icb_vehiculo
                                 //join mar in context.marca_vehiculo
                                 //on icb.marcvh_id equals mar.marcvh_id
                                 //join est in context.estilo_vehiculo
                                 //on icb.estvh_id equals est.estvh_id
                             join mod in context.modelo_vehiculo
                                 on icb.modvh_id equals mod.modvh_codigo
                             join col in context.color_vehiculo
                                 on icb.colvh_id equals col.colvh_id
                             join bod in context.bodega_concesionario
                                 on icb.id_bod equals bod.id
                             where ( //mar.marcvh_nombre.ToString().Contains(texto)
                                       mod.modvh_nombre.Contains(texto)
                                       //|| est.estvh_nombre.ToString().Contains(texto)
                                       || col.colvh_nombre.Contains(texto)
                                       || icb.nummot_vh.Contains(texto)
                                       || icb.vin.Contains(texto)
                                       || icb.plac_vh.Contains(texto)
                                       || icb.fecfact_fabrica == parseo
                                       || icb.anio_vh.ToString().Contains(texto)
                                       || icb.id_bod.ToString().Contains(texto)
                                       || icb.plan_mayor.Contains(texto)) &&
                                   icb.tpvh_id == 4
                             orderby icb.fecfact_fabrica descending
                             select new
                             {
                                 //marca = mar.marcvh_nombre,
                                 motor = icb.nummot_vh,
                                 //estilo = est.estvh_nombre,
                                 modelo = mod.modvh_nombre,
                                 color = col.colvh_nombre,
                                 serie = icb.vin,
                                 placa = icb.plac_vh,
                                 fechaCompra = icb.fecfact_fabrica,
                                 anio = icb.anio_vh,
                                 codBodega = bod.bodccs_cod,
                                 planMayor = icb.plan_mayor,
                                 id = icb.icbvh_id,
                                 diasEnInventario = SqlFunctions.DateDiff("day", icb.fecfact_fabrica, DateTime.Now)
                             }).Skip(1 * 10 - 10).Take(10).ToList();

            var result2 = resultAux.Select(x => new
            {
                //x.marca,
                x.motor,
                //x.estilo,
                x.modelo,
                x.color,
                x.serie,
                placa = x.placa != null ? x.placa : "Sin asignar",
                fechaCompra = x.fechaCompra != null ? x.fechaCompra.Value.ToString("yyyy/MM/dd") : null,
                x.anio,
                x.codBodega,
                x.planMayor,
                x.id,
                x.diasEnInventario
            });

            var data = new
            {
                result,
                result2
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarVehiculosNuevos(string texto, int pagina)
        {
            // Si la busqueda es por fecha entonces hay que hacer el parseo a fecha fuera de la consulta linq
            DateTime parseo = new DateTime();
            try
            {
                parseo = DateTime.Parse(texto);
            }
            catch (FormatException)
            {
                // Cuando el campo de busqueda no tiene formato de fecha se captura la excepcion
            }

            var resultAux = (from icb in context.icb_vehiculo
                                 //join mar in context.marca_vehiculo
                                 //on icb.marcvh_id equals mar.marcvh_id
                                 //join est in context.estilo_vehiculo
                                 //on icb.estvh_id equals est.estvh_id
                             join mod in context.modelo_vehiculo
                                 on icb.modvh_id equals mod.modvh_codigo
                             join col in context.color_vehiculo
                                 on icb.colvh_id equals col.colvh_id
                             join bod in context.bodega_concesionario
                                 on icb.id_bod equals bod.id
                             where ( //mar.marcvh_nombre.ToString().Contains(texto)
                                       mod.modvh_nombre.Contains(texto)
                                       //|| est.estvh_nombre.ToString().Contains(texto)
                                       || col.colvh_nombre.Contains(texto)
                                       || icb.nummot_vh.Contains(texto)
                                       || icb.vin.Contains(texto)
                                       || icb.plac_vh.Contains(texto)
                                       || icb.fecfact_fabrica == parseo
                                       || icb.anio_vh.ToString().Contains(texto)
                                       || icb.id_bod.ToString().Contains(texto)
                                       || icb.plan_mayor.Contains(texto)) &&
                                   icb.tpvh_id == 4
                             orderby icb.fecfact_fabrica descending
                             select new
                             {
                                 //marca = mar.marcvh_nombre,
                                 motor = icb.nummot_vh,
                                 //estilo = est.estvh_nombre,
                                 modelo = mod.modvh_nombre,
                                 color = col.colvh_nombre,
                                 serie = icb.vin,
                                 placa = icb.plac_vh,
                                 fechaCompra = icb.fecfact_fabrica,
                                 anio = icb.anio_vh,
                                 codBodega = bod.bodccs_cod,
                                 planMayor = icb.plan_mayor,
                                 id = icb.icbvh_id,
                                 diasEnInventario = SqlFunctions.DateDiff("day", icb.fecfact_fabrica, DateTime.Now)
                             }).Skip(pagina * 10 - 10).Take(10).ToList();

            var result = resultAux.Select(x => new
            {
                //x.marca,
                x.motor,
                //x.estilo,
                x.modelo,
                x.color,
                x.serie,
                placa = x.placa != null ? x.placa : "Sin asignar",
                fechaCompra = x.fechaCompra != null ? x.fechaCompra.Value.ToString("yyyy/MM/dd") : null,
                x.anio,
                x.codBodega,
                x.planMayor,
                x.id,
                x.diasEnInventario
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ActualizarInfCompra(int id, string fechaManifiesto, int ciudadManifiesto,
            string numeroManifiesto, int diasLibres)
        {
            icb_vehiculo QueryVehiculo = context.icb_vehiculo.FirstOrDefault(x => x.icbvh_id == id);
            int result = 0;
            if (QueryVehiculo != null)
            {
                int dia = Convert.ToInt32(fechaManifiesto.Substring(0, 2));
                int mes = Convert.ToInt32(fechaManifiesto.Substring(3, 2));
                int anioFecha = Convert.ToInt32(fechaManifiesto.Substring(6, 4));
                QueryVehiculo.fecman_vh = new DateTime(anioFecha, mes, dia);
                QueryVehiculo.ciumanf_vh = ciudadManifiesto;
                QueryVehiculo.nummanf_vh = numeroManifiesto;
                QueryVehiculo.diaslibres_vh = diasLibres;
                context.Entry(QueryVehiculo).State = EntityState.Modified;
                result = context.SaveChanges();
            }
            else
            {
                result = -1;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ActualizarInfEntrega(int id, string fecEntrTrans, string fecLlegada, string hora,
            string fechaEntMan, int tipoTram, int idTramitador, string fechaRunt)
        {
            icb_vehiculo QueryVehiculo = context.icb_vehiculo.FirstOrDefault(x => x.icbvh_id == id);
            if (fecEntrTrans != "")
            {
                QueryVehiculo.fecentregatransp_vh = new DateTime(Convert.ToInt32(fecEntrTrans.Substring(6, 4)),
                    Convert.ToInt32(fecEntrTrans.Substring(3, 2)),
                    Convert.ToInt32(fecEntrTrans.Substring(0, 2)));
            }

            if (fecLlegada != "")
            {
                QueryVehiculo.fecllegadaccs_vh = new DateTime(Convert.ToInt32(fecLlegada.Substring(6, 4)),
                    Convert.ToInt32(fecLlegada.Substring(3, 2)),
                    Convert.ToInt32(fecLlegada.Substring(0, 2)), Convert.ToInt32(hora.Substring(0, 2)),
                    Convert.ToInt32(hora.Substring(3, 2)), 0);
            }

            if (fechaEntMan != "")
            {
                QueryVehiculo.fecentman_vh = new DateTime(Convert.ToInt32(fechaEntMan.Substring(6, 4)),
                    Convert.ToInt32(fechaEntMan.Substring(3, 2)),
                    Convert.ToInt32(fechaEntMan.Substring(0, 2)));
            }

            if (fechaRunt != "")
            {
                QueryVehiculo.fecharunt_vh = new DateTime(Convert.ToInt32(fechaRunt.Substring(6, 4)),
                    Convert.ToInt32(fechaRunt.Substring(3, 2)),
                    Convert.ToInt32(fechaRunt.Substring(0, 2)));
            }

            if (idTramitador != 0)
            {
                QueryVehiculo.tramitador_id = idTramitador;
            }

            context.Entry(QueryVehiculo).State = EntityState.Modified;
            int result = context.SaveChanges();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ManualCargueNUevos(int? menu)
        {
            ListasDesplegablesManualVhNuevo();
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult BuscarVehiculoPorPlanMayor(string planMayor, int? id_provedor, int? id_tp_documento,
            int? id_bodega)
        {
            //verificamos que el vehiculo si existe
            if (id_provedor == null || id_tp_documento == null || id_bodega == null)
            {
                return Json(
                    new
                    {
                        CamposVacios = true,
                        NoEncontrado = false,
                        NoAnulado = false,
                        mensaje = "Debe digitar el proveedor, la bodega y el tipo de documento"
                    }, JsonRequestBehavior.AllowGet);
            }

            bool actividadEconomicaValido = false;
            tp_doc_registros buscarTipoDocRegistro = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == id_tp_documento);
            var buscarProveedor = (from tercero in context.icb_terceros
                                   join proveedor in context.tercero_proveedor
                                       on tercero.tercero_id equals proveedor.tercero_id
                                   join acteco in context.acteco_tercero
                                       on proveedor.acteco_id equals acteco.acteco_id
                                   join bodega in context.terceros_bod_ica
                                       on acteco.acteco_id equals bodega.idcodica into ps
                                   from bodega in ps.DefaultIfEmpty()
                                   join regimen in context.tpregimen_tercero
                                       on proveedor.tpregimen_id equals regimen.tpregimen_id into ps2
                                   from regimen in ps2.DefaultIfEmpty()
                                   where proveedor.tercero_id == id_provedor
                                   select new
                                   {
                                       proveedor.acteco_id,
                                       acteco.acteco_nombre,
                                       tarifaPorBodega = bodega != null ? bodega.porcentaje : 0,
                                       actecoTarifa = acteco.tarifa,
                                       regimen_id = regimen.tpregimen_id
                                   }).FirstOrDefault();
            decimal porcentajeRetIca = 0;
            decimal porcentajeRetIva = 0;
            decimal porcentajeRetencion = 0;
            decimal restarIcaIvaRetencion = 0;
            decimal valorReteica = 0;
            if (buscarProveedor != null)
            {
                if (buscarProveedor.tarifaPorBodega != 0)
                {
                    actividadEconomicaValido = true;
                    porcentajeRetIca = buscarProveedor.tarifaPorBodega;
                }
                else if (buscarProveedor.actecoTarifa != 0)
                {
                    actividadEconomicaValido = true;
                    porcentajeRetIca = buscarProveedor.actecoTarifa;
                }
            }

            int regimen_proveedor = buscarProveedor != null ? buscarProveedor.regimen_id : 0;
            //filtro nuevo a para consultar Aplica/No aplica por proveedor y no por perfil tributario.
            tercero_proveedor buscarProveedorTributario = context.tercero_proveedor.FirstOrDefault(x => x.tercero_id == id_provedor);
            //if (buscarProveedorTributario.retfuente == null)
            // {
            // var buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x => x.bodega == id_bodega && x.sw == buscarTipoDocRegistro.sw && x.tipo_regimenid == regimen_proveedor);
            //}
            //filtro anterior a modificaciones de octubre del 2019 para presentación.
            //var buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x => x.bodega == id_bodega && x.sw == buscarTipoDocRegistro.sw && x.tipo_regimenid == regimen_proveedor);

            var buscarAutomovil = (from vehiculo in context.icb_vehiculo
                                   join modelo in context.modelo_vehiculo
                                       on vehiculo.modvh_id equals modelo.modvh_codigo
                                   join color in context.color_vehiculo
                                       on vehiculo.colvh_id equals color.colvh_id
                                   where vehiculo.plan_mayor == planMayor
                                   select new
                                   {
                                       vehiculo.plan_mayor,
                                       modelo.modvh_nombre,
                                       modelo.modvh_codigo,
                                       vehiculo.anio_vh,
                                       color.colvh_nombre,
                                       costosiniva_vh = vehiculo.costosiniva_vh != null ? vehiculo.costosiniva_vh : 0,
                                       iva_vh = vehiculo.iva_vh != null ? vehiculo.iva_vh : 0,
                                       costototal_vh = vehiculo.costototal_vh != null ? vehiculo.costototal_vh : 0
                                   }).FirstOrDefault();

            if (buscarAutomovil != null)
            {
                // si el vehiculo existe verificamos que no este vendido
                var buscarAutomovilAnulado = (from vehiculo in context.icb_vehiculo
                                              join encabezado in context.encab_documento
                                                  on vehiculo.plan_mayor equals encabezado.documento into ps
                                              from encabezado in ps.DefaultIfEmpty()
                                              join tipo_documento in context.tp_doc_registros
                                                  on encabezado.tipo equals tipo_documento.tpdoc_id into ps2
                                              from tipo_documento in ps2.DefaultIfEmpty()
                                              where vehiculo.plan_mayor == planMayor && tipo_documento.tipo == 1 && encabezado.bodega == id_bodega
                                              select new
                                              {
                                                  vehiculo.plan_mayor,
                                                  anulado = encabezado != null ? encabezado.anulado : true,
                                                  numero = encabezado != null ? encabezado.numero : 0,
                                                  encabezado.fecha,
                                                  costosiniva_vh = vehiculo.costosiniva_vh != null ? vehiculo.costosiniva_vh : 0,
                                                  iva_vh = vehiculo.iva_vh != null ? vehiculo.iva_vh : 0,
                                                  costototal_vh = vehiculo.costototal_vh != null ? vehiculo.costototal_vh : 0
                                              }).OrderByDescending(x => x.fecha).FirstOrDefault();


                if (buscarAutomovilAnulado != null)
                {
                    if (!buscarAutomovilAnulado.anulado)
                    {
                        var dataDevuelto = new
                        {
                            NoEncontrado = false,
                            NoAnulado = false,
                            Encontrado = false,
                            mensaje =
                                "El vehiculo ya fue comprado con numero de documento " + buscarAutomovilAnulado.numero
                                                                                       + " en la fecha " +
                                                                                       buscarAutomovilAnulado.fecha
                                                                                           .ToShortDateString()
                                                                                       + " y no ha sido anulado ni devuelto",
                            CamposVacios = false
                        };
                        return Json(dataDevuelto, JsonRequestBehavior.AllowGet);
                    }

                    // Se valida tambien si el vehiculo tuvo una devolucion en compra porque anulado y devuelto es diferente
                    string numeroAnulado = buscarAutomovilAnulado.numero.ToString();
                    encab_documento buscarAutomovilDevuelto = (from encabezado in context.encab_documento
                                                               join tipo_documento in context.tp_doc_registros
                                                                   on encabezado.tipo equals tipo_documento.tpdoc_id into ps2
                                                               from tipo_documento in ps2.DefaultIfEmpty()
                                                               where tipo_documento.tipo == 7 && encabezado.bodega == id_bodega &&
                                                                     encabezado.documento == numeroAnulado
                                                               select encabezado).OrderByDescending(x => x.fecha).FirstOrDefault();

                    //var buscarAutomovilDevuelto = context.tipo_documento(x => x.tipo_documento.tpdoc_id == context.idtipocomprav);


                    if (buscarAutomovilDevuelto != null)
                    {
                        var dataDevuelto = new
                        {
                            NoEncontrado = false,
                            NoAnulado = false,
                            Encontrado = false,
                            mensaje =
                                "El vehiculo ya fue devuelto con numero de documento " + buscarAutomovilDevuelto.numero
                                                                                       + " en la fecha " +
                                                                                       buscarAutomovilDevuelto.fecha
                                                                                           .ToShortDateString()
                                                                                       + " y no ha sido anulado ni devuelto",
                            CamposVacios = false
                        };
                        return Json(dataDevuelto, JsonRequestBehavior.AllowGet);
                    }
                }

                //else {
                if (buscarProveedorTributario != null)
                {
                    if (buscarTipoDocRegistro != null)
                    {
                        if (buscarProveedorTributario.retfuente == "A")
                        {
                            if (buscarTipoDocRegistro.baseretencion <=
                                (buscarAutomovil != null ? buscarAutomovil.costosiniva_vh : 0))
                            {
                                porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                restarIcaIvaRetencion +=
                                    (buscarAutomovil != null ? buscarAutomovil.costosiniva_vh : 0) *
                                    porcentajeRetencion / 100;
                            }
                        }

                        if (buscarProveedorTributario.retiva == "A")
                        {
                            if (buscarTipoDocRegistro.baseiva <=
                                (buscarAutomovil != null ? buscarAutomovil.costosiniva_vh : 0))
                            {
                                porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                restarIcaIvaRetencion += buscarAutomovil.iva_vh * porcentajeRetIva / 100;
                            }
                        }

                        if (buscarProveedorTributario.retica == "A")
                        {
                            if (!actividadEconomicaValido)
                            {
                                if (buscarTipoDocRegistro.baseica <=
                                    (buscarAutomovil != null ? buscarAutomovil.costosiniva_vh : 0))
                                {
                                    porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                    restarIcaIvaRetencion +=
                                        (buscarAutomovil != null ? buscarAutomovil.costosiniva_vh : 0) *
                                        porcentajeRetIca / 1000;
                                    valorReteica = (buscarAutomovil != null ? buscarAutomovil.costosiniva_vh : 0) *
                                                   porcentajeRetIca / 1000;
                                }
                            }
                            else
                            {
                                restarIcaIvaRetencion +=
                                    (buscarAutomovil != null ? buscarAutomovil.costosiniva_vh : 0) * porcentajeRetIca /
                                    1000;
                                valorReteica = (buscarAutomovil != null ? buscarAutomovil.costosiniva_vh : 0) *
                                               porcentajeRetIca / 1000;
                            }
                        }
                    }
                }

                if (buscarProveedorTributario == null)
                {
                    perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                        x.bodega == id_bodega && x.sw == buscarTipoDocRegistro.sw &&
                        x.tipo_regimenid == regimen_proveedor);
                    if (buscarPerfilTributario == null)
                    {
                        if (buscarTipoDocRegistro != null)
                        {
                            if (buscarPerfilTributario.retfuente == "A")
                            {
                                if (buscarTipoDocRegistro.baseretencion <=
                                    (buscarAutomovil != null ? buscarAutomovil.costosiniva_vh : 0))
                                {
                                    porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                    restarIcaIvaRetencion +=
                                        (buscarAutomovil != null ? buscarAutomovil.costosiniva_vh : 0) *
                                        porcentajeRetencion / 100;
                                }
                            }

                            if (buscarPerfilTributario.retiva == "A")
                            {
                                if (buscarTipoDocRegistro.baseiva <=
                                    (buscarAutomovil != null ? buscarAutomovil.costosiniva_vh : 0))
                                {
                                    porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                    restarIcaIvaRetencion += buscarAutomovil.iva_vh * porcentajeRetIva / 100;
                                }
                            }

                            if (buscarPerfilTributario.retica == "A")
                            {
                                if (!actividadEconomicaValido)
                                {
                                    if (buscarTipoDocRegistro.baseica <=
                                        (buscarAutomovil != null ? buscarAutomovil.costosiniva_vh : 0))
                                    {
                                        porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                        restarIcaIvaRetencion +=
                                            (buscarAutomovil != null ? buscarAutomovil.costosiniva_vh : 0) *
                                            porcentajeRetIca / 1000;
                                        valorReteica = (buscarAutomovil != null ? buscarAutomovil.costosiniva_vh : 0) *
                                                       porcentajeRetIca / 1000;
                                    }
                                }
                                else
                                {
                                    restarIcaIvaRetencion +=
                                        (buscarAutomovil != null ? buscarAutomovil.costosiniva_vh : 0) *
                                        porcentajeRetIca / 1000;
                                    valorReteica = (buscarAutomovil != null ? buscarAutomovil.costosiniva_vh : 0) *
                                                   porcentajeRetIca / 1000;
                                }
                            }
                        }
                    }
                }

                var data = new
                {
                    Encontrado = true,
                    costo = buscarAutomovil.costosiniva_vh,
                    iva = buscarAutomovil.iva_vh,
                    total = buscarAutomovil.costototal_vh,
                    buscarAutomovil.modvh_nombre,
                    buscarAutomovil.modvh_codigo,
                    buscarAutomovil.anio_vh,
                    buscarAutomovil.colvh_nombre,
                    porcentajeRetIca,
                    porcentajeRetencion,
                    porcentajeRetIva,
                    valorRetencion = (buscarAutomovil != null ? buscarAutomovil.costosiniva_vh : 0) *
                                     porcentajeRetencion / 100,
                    valorReteIva = (buscarAutomovil != null ? buscarAutomovil.costosiniva_vh : 0) * porcentajeRetIva /
                                   100,
                    valorReteica
                };
                return Json(data, JsonRequestBehavior.AllowGet);
                //}
                //}
                //else
                //{
                //var dataDevuelto = new
                //{
                //    NoEncontrado = false,
                //    NoAnulado = false,
                //    Encontrado = false,
                //    mensaje = "El vehículo ingresado ya se encuentra registrado, pero no para la bodega y tipo de documento compra, por favor verifique",
                //    CamposVacios = false
                //};
                //return Json(dataDevuelto, JsonRequestBehavior.AllowGet);

                //var data = new
                //{
                //    NoEncontrado = false,
                //    NoAnulado = false,
                //    Encontrado = false,
                //    mensaje = "El vehiculo ya fue comprado con numero de documento " + buscarAutomovilAnulado.numero != null+ " en la fecha " + buscarAutomovilAnulado.fecha.ToShortDateString() + " y no ha sido anulado ni devuelto",
                //    CamposVacios = false
                //};
                //return Json(data, JsonRequestBehavior.AllowGet);
                //}
            }

            return Json(
                new
                {
                    CamposVacios = false,
                    NoEncontrado = true,
                    NoAnulado = false,
                    mensaje = "El vehiculo no se encuentra registrado"
                }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ManualCargueNuevos(CargueManualVhModel modelo, int? menu)
        {
            ViewBag.bodegaSeleccionada = modelo.id_bod;
            ViewBag.perfilSeleccionado = modelo.perfilContable;

            decimal restarIcaIvaRetencion = 0;


            if (ModelState.IsValid)
            {
                bool actividadEconomicaValido = false;
                decimal porcentajeReteICA = 0;
                List<cuenta_puc> listaCuentasPucAfectables = context.cuenta_puc.Where(x => x.esafectable).ToList();
                tp_doc_registros buscarTipoDocRegistro =
                    context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == modelo.doc_registros);
                var buscarProveedor = (from tercero in context.icb_terceros
                                       join proveedor in context.tercero_proveedor
                                           on tercero.tercero_id equals proveedor.tercero_id
                                       join acteco in context.acteco_tercero
                                           on proveedor.acteco_id equals acteco.acteco_id
                                       join bodega in context.terceros_bod_ica
                                           on acteco.acteco_id equals bodega.idcodica into ps
                                       from bodega in ps.DefaultIfEmpty()
                                       join regimen in context.tpregimen_tercero
                                           on proveedor.tpregimen_id equals regimen.tpregimen_id into ps2
                                       from regimen in ps2.DefaultIfEmpty()
                                       where proveedor.tercero_id == modelo.proveedor_id
                                       select new
                                       {
                                           proveedor.acteco_id,
                                           acteco.acteco_nombre,
                                           tarifaPorBodega = bodega != null ? bodega.porcentaje : 0,
                                           actecoTarifa = acteco.tarifa,
                                           regimen_id = regimen.tpregimen_id
                                       }).FirstOrDefault();
                if (buscarProveedor != null)
                {
                    if (buscarProveedor.tarifaPorBodega != 0)
                    {
                        actividadEconomicaValido = true;
                        porcentajeReteICA = buscarProveedor.tarifaPorBodega;
                    }
                    else if (buscarProveedor.actecoTarifa != 0)
                    {
                        actividadEconomicaValido = true;
                        porcentajeReteICA = buscarProveedor.actecoTarifa;
                    }
                    else
                    {
                        porcentajeReteICA = (decimal)buscarTipoDocRegistro.retica;
                    }
                }

                int regimen_proveedor = buscarProveedor != null ? buscarProveedor.regimen_id : 0;
                // Aca esta el perfil cambiar
                //consulta anterior sin modificaciones. El nombre de la variable es buscarPerfilTributario, se cambio para hacer pruebas.
                // var buscarPerfilTributario2 = context.perfiltributario.FirstOrDefault(x => x.bodega == modelo.id_bod && x.sw == buscarTipoDocRegistro.sw && x.tipo_regimenid == regimen_proveedor);
                //consulta prueba para traer la información desde tercero_proveedor

                tercero_proveedor buscarPerfilTributario =
                    context.tercero_proveedor.FirstOrDefault(x => x.tercero_id == modelo.proveedor_id);


                //var buscarPlanMayor = context.icb_vehiculo.FirstOrDefault(x=>x.plan_mayor == modelo.plan_mayor);
                var buscarAutomovil = (from vehiculo in context.icb_vehiculo
                                       join encabezado in context.encab_documento
                                           on vehiculo.plan_mayor equals encabezado.documento into ps
                                       from encabezado in ps.DefaultIfEmpty()
                                       where vehiculo.plan_mayor == modelo.plan_mayor
                                       select new
                                       {
                                           vehiculo.plan_mayor,
                                           anulado = encabezado != null ? encabezado.anulado : true,
                                           numero = encabezado != null ? encabezado.numero : 0,
                                           fecha = encabezado != null ? encabezado.fecha : DateTime.Now
                                       }).FirstOrDefault();
                if (buscarAutomovil == null)
                {
                    TempData["mensaje_error"] = "El plan mayor no se encuentra registrado, por favor verifique";
                }
                else
                {
                    if (buscarAutomovil.anulado)
                    {
                        icb_sysparameter buscarParametroStatus = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P17");
                        string statusParametro = buscarParametroStatus != null ? buscarParametroStatus.syspar_value : "0";

                        List<vconceptocompra> tipoCompraVehiculos = context.vconceptocompra.ToList();

                        long numeroConsecutivo = 0;
                        ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
                        icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
                        numeroConsecutivoAux = gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro,
                            modelo.doc_registros, modelo.id_bod);
                        grupoconsecutivos grupoConsecutivo = context.grupoconsecutivos.FirstOrDefault(x =>
                            x.documento_id == modelo.doc_registros && x.bodega_id == modelo.id_bod);
                        if (numeroConsecutivoAux != null)
                        {
                            numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                        }
                        else
                        {
                            TempData["mensaje_error"] =
                                "No existe un numero consecutivo asignado para este tipo de documento";
                            ListasDesplegablesManualVhNuevo();
                            BuscarFavoritos(menu);
                            return View(modelo);
                        }

                        List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();
                        var parametrosCuentas = (from perfilCuentas in context.perfil_cuentas_documento
                                                 join nombres in context.paramcontablenombres
                                                     on perfilCuentas.id_nombre_parametro equals nombres.id
                                                 join cuenta in context.cuenta_puc
                                                     on perfilCuentas.cuenta equals cuenta.cntpuc_id
                                                 join documento in context.perfil_contable_documento
                                                     on perfilCuentas.id_perfil equals documento.id
                                                 where perfilCuentas.id_perfil == modelo.perfilContable
                                                 select new
                                                 {
                                                     perfilCuentas.cuenta,
                                                     perfilCuentas.id_nombre_parametro,
                                                     nombres.descripcion_parametro,
                                                     cuenta.cntpuc_numero,
                                                     perfilCuentas.centro,
                                                     documento.descripcion,
                                                     documento.codigo
                                                 }).ToList();
                        decimal calcularDebito = 0;
                        decimal calcularCredito = 0;
                        foreach (var parametro in parametrosCuentas)
                        {
                            cuenta_puc buscarCuenta =
                                listaCuentasPucAfectables.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
                            if (buscarCuenta != null)
                            {
                                decimal valorCredito = 0;
                                decimal valorDebito = 0;
                                decimal valorFletes = 0;
                                bool convertirDescuento = decimal.TryParse(modelo.valorDescuentoPie, out decimal vDescuento);

                                bool convertirVFlete = decimal.TryParse(modelo.valorFlete, out decimal vFlete);
                                if (convertirVFlete)
                                {
                                    valorFletes = Convert.ToDecimal(modelo.valorFlete, miCultura);
                                }

                                decimal ValorFactura = Convert.ToDecimal(modelo.costosiniva_vh, miCultura) + valorFletes +
                                                   (Convert.ToDecimal(modelo.costosiniva_vh, miCultura) - vDescuento) *
                                                   Convert.ToDecimal(modelo.iva_vh, miCultura) / 100 - vDescuento;
                                decimal baseiva = (Convert.ToDecimal(modelo.costosiniva_vh, miCultura) - vDescuento) *
                                              Convert.ToDecimal(modelo.iva_vh, miCultura) / 100;
                                if (parametro.id_nombre_parametro == 1)
                                {
                                    //   decimal restarIcaIvaRetencion = 0;
                                    if (buscarPerfilTributario != null)
                                    {
                                        if (buscarTipoDocRegistro != null)
                                        {
                                            if (buscarPerfilTributario.retfuente == "A")
                                            {
                                                if (buscarTipoDocRegistro.baseretencion <=
                                                    Convert.ToDecimal(modelo.costosiniva_vh, miCultura))
                                                {
                                                    decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                                    restarIcaIvaRetencion +=
                                                        Convert.ToDecimal(modelo.costosiniva_vh, miCultura) * porcentajeRetencion /
                                                        100;
                                                }
                                            }

                                            if (buscarPerfilTributario.retiva == "A")
                                            {
                                                if (buscarTipoDocRegistro.baseiva <=
                                                    ValorFactura - Convert.ToDecimal(modelo.costosiniva_vh, miCultura))
                                                {
                                                    decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                                    //restarIcaIvaRetencion += ((ValorFactura - Convert.ToDecimal(modelo.costosiniva_vh)) * porcentajeRetIva) / 100;
                                                    restarIcaIvaRetencion += baseiva * porcentajeRetIva / 100;
                                                }
                                            }

                                            if (buscarPerfilTributario.retica == "A")
                                            {
                                                if (!actividadEconomicaValido)
                                                {
                                                    if (buscarTipoDocRegistro.baseica <=
                                                        Convert.ToDecimal(modelo.costosiniva_vh, miCultura))
                                                    {
                                                        decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                                        restarIcaIvaRetencion +=
                                                            Convert.ToDecimal(modelo.costosiniva_vh, miCultura) *
                                                            porcentajeRetIca / 1000;
                                                    }
                                                }
                                                else
                                                {
                                                    restarIcaIvaRetencion +=
                                                        Convert.ToDecimal(modelo.costosiniva_vh, miCultura) * porcentajeReteICA /
                                                        1000;
                                                }
                                            }
                                        }
                                    }

                                    valorCredito = ValorFactura - restarIcaIvaRetencion;
                                    calcularCredito += valorCredito;
                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                    {
                                        NumeroCuenta = buscarCuenta.cntpuc_numero,
                                        DescripcionParametro = parametro.descripcion_parametro,
                                        ValorDebito = 0,
                                        ValorCredito = valorCredito
                                    });
                                }

                                if (parametro.id_nombre_parametro == 2)
                                {
                                    //var montoiva= ((Convert.ToDecimal(modelo.costosiniva_vh) * Convert.ToDecimal(modelo.iva_vh)) / 100);
                                    decimal montoiva = baseiva;

                                    valorDebito = montoiva;

                                    calcularDebito += valorDebito;
                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                    {
                                        NumeroCuenta = buscarCuenta.cntpuc_numero,
                                        DescripcionParametro = parametro.descripcion_parametro,
                                        ValorDebito = valorDebito,
                                        ValorCredito = 0
                                    });
                                }

                                if (parametro.id_nombre_parametro == 3)
                                {
                                    if (buscarPerfilTributario != null)
                                    {
                                        if (buscarTipoDocRegistro != null)
                                        {
                                            if (buscarPerfilTributario.retfuente == "A")
                                            {
                                                if (buscarTipoDocRegistro.baseretencion <=
                                                    Convert.ToDecimal(modelo.costosiniva_vh, miCultura))
                                                {
                                                    decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                                    valorCredito =
                                                        Convert.ToDecimal(modelo.costosiniva_vh, miCultura) * porcentajeRetencion /
                                                        100;
                                                    calcularCredito += valorCredito;
                                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                    {
                                                        NumeroCuenta = buscarCuenta.cntpuc_numero,
                                                        DescripcionParametro = parametro.descripcion_parametro,
                                                        ValorDebito = 0,
                                                        ValorCredito = valorCredito
                                                    });
                                                }
                                            }
                                        }
                                    }
                                }

                                if (parametro.id_nombre_parametro == 4)
                                {
                                    if (buscarPerfilTributario != null)
                                    {
                                        if (buscarTipoDocRegistro != null)
                                        {
                                            if (buscarPerfilTributario.retiva == "A")
                                            {
                                                if (buscarTipoDocRegistro.baseiva <=
                                                    Convert.ToDecimal(modelo.costosiniva_vh, miCultura))
                                                {
                                                    decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                                    //valorCredito = ((ValorFactura - Convert.ToDecimal(modelo.costosiniva_vh)) * porcentajeRetIva) / 100;
                                                    valorCredito = baseiva * porcentajeRetIva / 100;

                                                    calcularCredito += valorCredito;
                                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                    {
                                                        NumeroCuenta = buscarCuenta.cntpuc_numero,
                                                        DescripcionParametro = parametro.descripcion_parametro,
                                                        ValorDebito = 0,
                                                        ValorCredito = valorCredito
                                                    });
                                                }
                                            }
                                        }
                                    }
                                }

                                if (parametro.id_nombre_parametro == 5)
                                {
                                    if (buscarPerfilTributario != null)
                                    {
                                        if (buscarTipoDocRegistro != null)
                                        {
                                            if (buscarPerfilTributario.retica == "A")
                                            {
                                                if (!actividadEconomicaValido)
                                                {
                                                    if (buscarTipoDocRegistro.baseica <=
                                                        Convert.ToDecimal(modelo.costosiniva_vh, miCultura))
                                                    {
                                                        decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                                        valorCredito =
                                                            Convert.ToDecimal(modelo.costosiniva_vh, miCultura) *
                                                            porcentajeRetIca / 1000;
                                                        calcularCredito += valorCredito;
                                                        listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                        {
                                                            NumeroCuenta = buscarCuenta.cntpuc_numero,
                                                            DescripcionParametro = parametro.descripcion_parametro,
                                                            ValorDebito = 0,
                                                            ValorCredito = valorCredito
                                                        });
                                                    }
                                                }
                                                else
                                                {
                                                    valorCredito =
                                                        Convert.ToDecimal(modelo.costosiniva_vh, miCultura) * porcentajeReteICA /
                                                        1000;
                                                    calcularCredito += valorCredito;
                                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                    {
                                                        NumeroCuenta = buscarCuenta.cntpuc_numero,
                                                        DescripcionParametro = parametro.descripcion_parametro,
                                                        ValorDebito = 0,
                                                        ValorCredito = valorCredito
                                                    });
                                                }
                                            }
                                        }
                                    }
                                }

                                if (parametro.id_nombre_parametro == 6)
                                {
                                    valorDebito = Convert.ToDecimal(modelo.valorFlete, miCultura);
                                    calcularDebito += valorDebito;
                                    restarIcaIvaRetencion += valorDebito;
                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                    {
                                        NumeroCuenta = buscarCuenta.cntpuc_numero,
                                        DescripcionParametro = parametro.descripcion_parametro,
                                        ValorDebito = valorDebito,
                                        ValorCredito = 0
                                    });
                                    //valorCredito = ValorFactura - restarIcaIvaRetencion;
                                }

                                if (parametro.id_nombre_parametro == 7)
                                {
                                    valorCredito = Convert.ToDecimal(modelo.valorFlete, miCultura) *
                                                   Convert.ToDecimal(modelo.porcentajeIvaFlete, miCultura) / 100;
                                    calcularCredito += valorCredito;
                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                    {
                                        NumeroCuenta = buscarCuenta.cntpuc_numero,
                                        DescripcionParametro = parametro.descripcion_parametro,
                                        ValorDebito = 0,
                                        ValorCredito = valorCredito
                                    });
                                }

                                if (parametro.id_nombre_parametro == 9)
                                {
                                    valorDebito = Convert.ToDecimal(modelo.costosiniva_vh, miCultura);
                                    calcularDebito += valorDebito;
                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                    {
                                        NumeroCuenta = buscarCuenta.cntpuc_numero,
                                        DescripcionParametro = parametro.descripcion_parametro,
                                        ValorDebito = valorDebito,
                                        ValorCredito = 0
                                    });
                                }

                                if (parametro.id_nombre_parametro == 14)
                                {
                                    valorDebito = Convert.ToDecimal(modelo.valorDescuentoPie, miCultura);
                                    calcularDebito += valorDebito;
                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                    {
                                        NumeroCuenta = buscarCuenta.cntpuc_numero,
                                        DescripcionParametro = parametro.descripcion_parametro,
                                        ValorDebito = valorDebito,
                                        ValorCredito = 0
                                    });
                                }
                            }
                        }

                        if (calcularCredito != calcularDebito)
                        {
                            TempData["mensajeError"] = "El documento no tiene los movimientos calculados correctamente"
                                                       + "el perfil contable usado es " +
                                                       parametrosCuentas.FirstOrDefault().descripcion
                                                       + " codigo " + parametrosCuentas.FirstOrDefault().codigo;
                            TempData["documento_descuadrado"] =
                                "El documento no tiene los movimientos calculados correctamente";
                            ViewBag.documentoDescuadrado = listaDescuadrados;
                            ViewBag.calculoDebito = calcularDebito;
                            ViewBag.calculoCredito = calcularCredito;
                            ListasDesplegablesManualVhNuevo();
                            BuscarFavoritos(menu);
                            return View(modelo);
                        }

                        icb_vehiculo buscarPlanMayor =
                            context.icb_vehiculo.FirstOrDefault(x => x.plan_mayor == modelo.plan_mayor);
                        buscarPlanMayor.icbvh_estatus = statusParametro;
                        buscarPlanMayor.fecfact_fabrica = DateTime.Now;
                        context.Entry(buscarPlanMayor).State = EntityState.Modified;

                        int guardarVehiculo = context.SaveChanges();

                        // Guardar registros en la tabla encab_documento 
                        int diasVencimiento = context.fpago_tercero
                                                  .FirstOrDefault(x => x.fpago_id == modelo.condicion_pago)
                                                  .dvencimiento ?? 0;

                        bool convertirDescuento2 = decimal.TryParse(modelo.valorDescuentoPie, out decimal vDescuento2);
                        decimal baseiva2 = (Convert.ToDecimal(modelo.costosiniva_vh, miCultura) - vDescuento2) *
                                       Convert.ToDecimal(modelo.iva_vh, miCultura) / 100;

                        encab_documento encabezadoNuevo = new encab_documento
                        {
                            tipo = modelo.doc_registros,
                            nit = modelo.proveedor_id,
                            fecha = DateTime.Now,
                            fpago_id = modelo.condicion_pago,
                            vencimiento = DateTime.Now.AddDays(diasVencimiento),
                            costo = Convert.ToDecimal(modelo.costosiniva_vh, miCultura),
                            //iva = Convert.ToDecimal(modelo.costototal_vh) - Convert.ToDecimal(modelo.costosiniva_vh),
                            iva = baseiva2,
                            valor_total = Convert.ToDecimal(modelo.costototal_vh, miCultura),
                            bodega = modelo.id_bod,
                            estado = true,
                            documento = Convert.ToString(modelo.num_pedido),
                            pedido = null,
                            numero = (int)numeroConsecutivo,
                            porcen_retica = (float)porcentajeReteICA,
                            porcen_retencion = buscarTipoDocRegistro.retencion,
                            porcen_reteiva = buscarTipoDocRegistro.retiva,
                            fec_creacion = DateTime.Now,
                            perfilcontable = modelo.perfilContable,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            concepto = modelo.concepto1_id,
                            concepto2 = modelo.concepto2_id
                        };


                        if (buscarPerfilTributario != null)
                        {
                            if (buscarTipoDocRegistro != null)
                            {
                                if (buscarPerfilTributario.retfuente == "A")
                                {
                                    if (buscarTipoDocRegistro.baseretencion <= Convert.ToDecimal(modelo.costosiniva_vh, miCultura))
                                    {
                                        decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                        encabezadoNuevo.retencion =
                                            Convert.ToDecimal(modelo.costosiniva_vh, miCultura) * porcentajeRetencion / 100;
                                    }
                                }

                                if (buscarPerfilTributario.retiva == "A")
                                {
                                    if (buscarTipoDocRegistro.baseiva <= Convert.ToDecimal(modelo.costosiniva_vh, miCultura))
                                    {
                                        decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                        encabezadoNuevo.retencion_iva =
                                            Convert.ToDecimal(modelo.iva_vh, miCultura) * porcentajeRetIva / 100;
                                    }
                                }

                                if (buscarPerfilTributario.retica == "A")
                                {
                                    if (!actividadEconomicaValido)
                                    {
                                        if (buscarTipoDocRegistro.baseica <= Convert.ToDecimal(modelo.costosiniva_vh, miCultura))
                                        {
                                            decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                            encabezadoNuevo.retencion_ica =
                                                Convert.ToDecimal(modelo.costosiniva_vh, miCultura) * porcentajeRetIca / 1000;
                                        }
                                    }
                                    else
                                    {
                                        encabezadoNuevo.retencion_ica =
                                            Convert.ToDecimal(modelo.costosiniva_vh, miCultura) * porcentajeReteICA / 1000;
                                    }
                                }
                            }
                        }

                        context.encab_documento.Add(encabezadoNuevo);

                        int guardarEncabDoc = context.SaveChanges();
                        encab_documento buscarUltimoEncabezado = context.encab_documento.OrderByDescending(x => x.idencabezado)
                            .FirstOrDefault();

                        // Guardar registros en la tabla lineas documento

                        context.lineas_documento.Add(new lineas_documento
                        {
                            codigo = modelo.plan_mayor,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            nit = modelo.nit_pago_id ?? 0,
                            cantidad = 1,
                            bodega = modelo.id_bod,
                            seq = 1,
                            porcentaje_iva = (float)Convert.ToDecimal(modelo.iva_vh, miCultura),
                            valor_unitario = Convert.ToDecimal(modelo.costosiniva_vh, miCultura),
                            costo_unitario = Convert.ToDecimal(modelo.costosiniva_vh, miCultura),
                            estado = true,
                            fec = DateTime.Now,
                            id_encabezado = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0
                        });

                        centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                        int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
                        icb_terceros terceroValorCero = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
                        int idTerceroCero = centroValorCero != null ? Convert.ToInt32(terceroValorCero.tercero_id) : 0;
                        int secuencia = 1;
                        foreach (var parametro in parametrosCuentas)
                        {
                            string CreditoDebito = "";
                            cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
                            if (buscarCuenta != null)
                            {
                                decimal valorCredito = 0;
                                decimal valorDebito = 0;
                                decimal ValorFactura = Convert.ToDecimal(modelo.costosiniva_vh, miCultura) +
                                                   Convert.ToDecimal(modelo.costosiniva_vh, miCultura) *
                                                   Convert.ToDecimal(modelo.iva_vh, miCultura) / 100;
                                if (parametro.id_nombre_parametro == 1)
                                {
                                    // decimal restarIcaIvaRetencion = 0;
                                    if (buscarPerfilTributario != null)
                                    {
                                        if (buscarTipoDocRegistro != null)
                                        {
                                            if (buscarPerfilTributario.retfuente == "A")
                                            {
                                                if (buscarTipoDocRegistro.baseretencion <=
                                                    Convert.ToDecimal(modelo.costosiniva_vh, miCultura))
                                                {
                                                    decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                                    restarIcaIvaRetencion +=
                                                        Convert.ToDecimal(modelo.costosiniva_vh, miCultura) * porcentajeRetencion /
                                                        100;
                                                }
                                            }

                                            if (buscarPerfilTributario.retiva == "A")
                                            {
                                                if (buscarTipoDocRegistro.baseiva <=
                                                    ValorFactura - Convert.ToDecimal(modelo.costosiniva_vh, miCultura))
                                                {
                                                    decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                                    restarIcaIvaRetencion +=
                                                        (ValorFactura - Convert.ToDecimal(modelo.costosiniva_vh, miCultura)) *
                                                        porcentajeRetIva / 100;
                                                }
                                            }

                                            if (buscarPerfilTributario.retica == "A")
                                            {
                                                if (!actividadEconomicaValido)
                                                {
                                                    if (buscarTipoDocRegistro.baseica <=
                                                        Convert.ToDecimal(modelo.costosiniva_vh, miCultura))
                                                    {
                                                        decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                                        restarIcaIvaRetencion +=
                                                            Convert.ToDecimal(modelo.costosiniva_vh, miCultura) *
                                                            porcentajeRetIca / 1000;
                                                    }
                                                }
                                                else
                                                {
                                                    restarIcaIvaRetencion +=
                                                        Convert.ToDecimal(modelo.costosiniva_vh, miCultura) * porcentajeReteICA /
                                                        1000;
                                                }
                                            }
                                        }
                                    }

                                    valorCredito = ValorFactura - restarIcaIvaRetencion;
                                    CreditoDebito = "Credito";
                                }

                                if (parametro.id_nombre_parametro == 2)
                                {
                                    //valorCredito = ValorFactura - valorSinIva;
                                    valorDebito = ValorFactura - Convert.ToDecimal(modelo.costosiniva_vh, miCultura);
                                    CreditoDebito = "Debito";
                                }

                                if (parametro.id_nombre_parametro == 3)
                                {
                                    if (buscarPerfilTributario != null)
                                    {
                                        if (buscarTipoDocRegistro != null)
                                        {
                                            if (buscarPerfilTributario.retfuente == "A")
                                            {
                                                if (buscarTipoDocRegistro.baseretencion <=
                                                    Convert.ToDecimal(modelo.costosiniva_vh, miCultura))
                                                {
                                                    decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                                    valorCredito =
                                                        Convert.ToDecimal(modelo.costosiniva_vh, miCultura) * porcentajeRetencion /
                                                        100;
                                                    //valorDebito = (valorSinIva * porcentajeRetencion) / 100;
                                                    CreditoDebito = "Credito";
                                                }
                                            }
                                        }
                                    }
                                }

                                if (parametro.id_nombre_parametro == 4)
                                {
                                    if (buscarPerfilTributario != null)
                                    {
                                        if (buscarTipoDocRegistro != null)
                                        {
                                            if (buscarPerfilTributario.retiva == "A")
                                            {
                                                if (buscarTipoDocRegistro.baseiva <=
                                                    Convert.ToDecimal(modelo.costosiniva_vh, miCultura))
                                                {
                                                    decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                                    valorCredito =
                                                        (ValorFactura - Convert.ToDecimal(modelo.costosiniva_vh, miCultura)) *
                                                        porcentajeRetIva / 100;
                                                    //valorDebito = ((ValorFactura - valorSinIva) * porcentajeRetIva) / 100;
                                                    CreditoDebito = "Credito";
                                                }
                                            }
                                        }
                                    }
                                }

                                if (parametro.id_nombre_parametro == 5)
                                {
                                    if (buscarPerfilTributario != null)
                                    {
                                        if (buscarTipoDocRegistro != null)
                                        {
                                            if (buscarPerfilTributario.retica == "A")
                                            {
                                                if (!actividadEconomicaValido)
                                                {
                                                    if (buscarTipoDocRegistro.baseica != null &&
                                                        buscarTipoDocRegistro.baseica <=
                                                        Convert.ToDecimal(modelo.costosiniva_vh, miCultura))
                                                    {
                                                        decimal porcentajeRetIca = buscarTipoDocRegistro.retica != null
                                                            ? (decimal)buscarTipoDocRegistro.retica
                                                            : 0;
                                                        valorCredito =
                                                            Convert.ToDecimal(modelo.costosiniva_vh, miCultura) *
                                                            porcentajeRetIca / 1000;
                                                        //valorDebito = (valorSinIva * porcentajeRetIca) / 100;
                                                        CreditoDebito = "Credito";
                                                    }
                                                }
                                                else
                                                {
                                                    valorCredito =
                                                        Convert.ToDecimal(modelo.costosiniva_vh, miCultura) * porcentajeReteICA /
                                                        1000;
                                                    //valorDebito = (valorSinIva * porcentajeReteICA) / 100;
                                                    CreditoDebito = "Credito";
                                                }
                                            }
                                        }
                                    }
                                }

                                if (parametro.id_nombre_parametro == 6)
                                {
                                    //valorCredito = valorSinIva;
                                    valorDebito = Convert.ToDecimal(modelo.valorFlete, miCultura);
                                    CreditoDebito = "Debito";
                                }

                                if (parametro.id_nombre_parametro == 7)
                                {
                                    valorCredito = Convert.ToDecimal(modelo.valorFlete, miCultura) *
                                                   Convert.ToDecimal(modelo.porcentajeIvaFlete) / 100;
                                    CreditoDebito = "Credito";
                                }

                                if (parametro.id_nombre_parametro == 9)
                                {
                                    //valorCredito = valorSinIva;
                                    valorDebito = Convert.ToDecimal(modelo.costosiniva_vh, miCultura);
                                    CreditoDebito = "Debito";
                                }

                                if (parametro.id_nombre_parametro == 14)
                                {
                                    //valorCredito = valorSinIva;
                                    valorDebito = Convert.ToDecimal(modelo.valorDescuentoPie, miCultura);
                                    CreditoDebito = "Debito";
                                }

                                if (valorCredito <= 0 || valorDebito <= 0)
                                {
                                    if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                    {
                                        mov_contable nuevoMovimiento = new mov_contable
                                        {
                                            //tipo_documento = modelo.doc_registros,
                                            id_encab = buscarUltimoEncabezado != null
                                                ? buscarUltimoEncabezado.idencabezado
                                                : 0,
                                            fec_creacion = DateTime.Now,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            //numero = numeroConsecutivo,
                                            idparametronombre = parametro.id_nombre_parametro,
                                            cuenta = parametro.cuenta,
                                            centro = parametro.centro,
                                            nit = modelo.nit_pago_id ?? 0,
                                            fec = DateTime.Now,
                                            seq = secuencia,
                                            debito = valorDebito
                                        };

                                        if (buscarTipoDocRegistro.aplicaniff)
                                        {
                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                                {
                                                    nuevoMovimiento.debitoniif = valorDebito;
                                                    nuevoMovimiento.debito = valorDebito;
                                                }

                                                if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                                {
                                                    nuevoMovimiento.credito = valorCredito;
                                                    nuevoMovimiento.creditoniif = valorCredito;
                                                }
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                                {
                                                    nuevoMovimiento.debitoniif = valorDebito;
                                                }

                                                if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                                {
                                                    nuevoMovimiento.creditoniif = valorCredito;
                                                }
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                                {
                                                    nuevoMovimiento.debito = valorDebito;
                                                }

                                                if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                                {
                                                    nuevoMovimiento.credito = valorCredito;
                                                }
                                            }
                                        }

                                        context.mov_contable.Add(nuevoMovimiento);
                                        secuencia++;

                                        DateTime fechaHoy = DateTime.Now;
                                        cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                            x.centro == parametro.centro && x.cuenta == parametro.cuenta &&
                                            x.nit == modelo.nit_pago_id && x.ano == fechaHoy.Year &&
                                            x.mes == fechaHoy.Month);

                                        if (buscar_cuentas_valores != null)
                                        {
                                            //buscar_cuentas_valores.ano = fechaHoy.Year;
                                            //buscar_cuentas_valores.mes = fechaHoy.Month;
                                            //buscar_cuentas_valores.cuenta = parametro.cuenta;
                                            //buscar_cuentas_valores.centro = parametro.centro;
                                            //buscar_cuentas_valores.nit = modelo.nit_pago_id ?? idTerceroCero;
                                            buscar_cuentas_valores.debito += valorDebito;
                                            buscar_cuentas_valores.credito += 0;
                                            buscar_cuentas_valores.debitoniff += 0;
                                            buscar_cuentas_valores.creditoniff += 0;
                                            context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            cuentas_valores crearCuentaValor = new cuentas_valores
                                            {
                                                ano = fechaHoy.Year,
                                                mes = fechaHoy.Month,
                                                cuenta = parametro.cuenta,
                                                centro = parametro.centro,
                                                nit = modelo.nit_pago_id ?? idTerceroCero,
                                                debito = valorDebito,
                                                credito = valorCredito,
                                                debitoniff = 0,
                                                creditoniff = 0
                                            };
                                            context.cuentas_valores.Add(crearCuentaValor);
                                        }
                                    }

                                    if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                    {
                                        mov_contable nuevoMovimiento = new mov_contable
                                        {
                                            //tipo_documento = modelo.doc_registros,
                                            id_encab = buscarUltimoEncabezado != null
                                                ? buscarUltimoEncabezado.idencabezado
                                                : 0,
                                            fec_creacion = DateTime.Now,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            //numero = numeroConsecutivo,
                                            idparametronombre = parametro.id_nombre_parametro,
                                            cuenta = parametro.cuenta,
                                            centro = parametro.centro,
                                            nit = modelo.nit_pago_id ?? 0,
                                            fec = DateTime.Now,
                                            seq = secuencia,
                                            credito = valorCredito
                                        };

                                        if (buscarTipoDocRegistro.aplicaniff)
                                        {
                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                                {
                                                    nuevoMovimiento.debitoniif = valorDebito;
                                                    nuevoMovimiento.debito = valorDebito;
                                                }

                                                if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                                {
                                                    nuevoMovimiento.credito = valorCredito;
                                                    nuevoMovimiento.creditoniif = valorCredito;
                                                }
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                                {
                                                    nuevoMovimiento.debitoniif = valorDebito;
                                                }

                                                if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                                {
                                                    nuevoMovimiento.creditoniif = valorCredito;
                                                }
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                                {
                                                    nuevoMovimiento.debito = valorDebito;
                                                }

                                                if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                                {
                                                    nuevoMovimiento.credito = valorCredito;
                                                }
                                            }
                                        }

                                        context.mov_contable.Add(nuevoMovimiento);
                                        secuencia++;

                                        DateTime fechaHoy = DateTime.Now;
                                        cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                            x.centro == parametro.centro && x.cuenta == parametro.cuenta &&
                                            x.nit == modelo.nit_pago_id && x.ano == fechaHoy.Year &&
                                            x.mes == fechaHoy.Month);

                                        if (buscar_cuentas_valores != null)
                                        {
                                            buscar_cuentas_valores.saldo_ini =
                                                buscar_cuentas_valores.saldo_ini - valorCredito;
                                            buscar_cuentas_valores.debito += 0;
                                            buscar_cuentas_valores.credito += valorCredito;
                                            buscar_cuentas_valores.saldo_ininiff =
                                                buscar_cuentas_valores
                                                    .saldo_ininiff /* + (movNuevo.debitoniif ?? 0) - (movNuevo.creditoniif ?? 0)*/
                                                ;
                                            buscar_cuentas_valores.debitoniff += 0;
                                            buscar_cuentas_valores.creditoniff += 0;
                                            context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            cuentas_valores crearCuentaValor = new cuentas_valores
                                            {
                                                ano = fechaHoy.Year,
                                                mes = fechaHoy.Month,
                                                cuenta = parametro.cuenta,
                                                centro = parametro.centro,
                                                nit = modelo.nit_pago_id ?? idTerceroCero,
                                                saldo_ini = valorDebito,
                                                debito = valorDebito,
                                                credito = valorCredito,
                                                saldo_ininiff = 0,
                                                debitoniff = 0,
                                                creditoniff = 0
                                            };
                                            context.cuentas_valores.Add(crearCuentaValor);
                                        }

                                        int guardarcuentavalor = context.SaveChanges();
                                    }
                                }
                            }
                        }
                        //}

                        short anoFacturacion = (short)DateTime.Now.Year;
                        short mesFacturacion = (short)DateTime.Now.Month;
                        referencias_inven buscarReferenciaInven = context.referencias_inven.FirstOrDefault(x =>
                            x.bodega == modelo.id_bod && x.codigo == modelo.plan_mayor && x.ano == anoFacturacion &&
                            x.mes == mesFacturacion);
                        // Guardar registro en la tabla referencias inventario (referencias_inven)

                        if (buscarReferenciaInven != null)
                        {
                            buscarReferenciaInven.can_ent = buscarReferenciaInven.can_ent + 1;
                            buscarReferenciaInven.cos_ent =
                                buscarReferenciaInven.cos_ent + Convert.ToDecimal(modelo.costosiniva_vh, miCultura);
                            buscarReferenciaInven.can_com = buscarReferenciaInven.can_com + 1;
                            buscarReferenciaInven.cos_com =
                                buscarReferenciaInven.cos_com + Convert.ToDecimal(modelo.costosiniva_vh, miCultura);
                            context.Entry(buscarReferenciaInven).State = EntityState.Modified;
                        }
                        else
                        {
                            context.referencias_inven.Add(new referencias_inven
                            {
                                bodega = modelo.id_bod,
                                codigo = modelo.plan_mayor,
                                ano = (short)DateTime.Now.Year,
                                mes = (short)DateTime.Now.Month,
                                can_ini = 0,
                                can_ent = 1,
                                can_sal = 0,
                                can_com = 1,
                                modulo = "V"
                            });
                        }

                        icb_referencia buscarReferencia =
                            context.icb_referencia.FirstOrDefault(x => x.ref_codigo == modelo.plan_mayor);
                        if (buscarReferencia != null)
                        {
                            buscarReferencia.fec_ultima_entrada = DateTime.Now;
                            context.Entry(buscarReferencia).State = EntityState.Modified;
                        }

                        int guardarLIneas = context.SaveChanges();
                        if (guardarEncabDoc > 0 && guardarLIneas > 0 && guardarVehiculo > 0)
                        {
                            icb_sysparameter buscarParametroEventoFact =
                                context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P19");
                            string eventoFacturacionParametro = buscarParametroEventoFact != null
                                ? buscarParametroEventoFact.syspar_value
                                : "1";
                            int idEventoFacturacion = Convert.ToInt32(eventoFacturacionParametro);
                            // Se agrega la trazabilidad del vehiculo nuevo, en este caso lo primero es facturacion
                            context.icb_vehiculo_eventos.Add(new icb_vehiculo_eventos
                            {
                                evento_estado = true,
                                eventofec_creacion = DateTime.Now,
                                fechaevento = DateTime.Now,
                                eventouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                evento_nombre = "Facturacion",
                                id_tpevento = idEventoFacturacion,
                                bodega_id = modelo.id_bod,
                                //vin = vehiculo.vin,
                                planmayor = modelo.plan_mayor
                            });
                            int guardarVehEventos = context.SaveChanges();

                            int grupoId = grupoConsecutivo != null ? grupoConsecutivo.grupo : 0;
                            List<grupoconsecutivos> gruposConsecutivos = context.grupoconsecutivos.Where(x => x.grupo == grupoId).ToList();
                            foreach (grupoconsecutivos grupo in gruposConsecutivos)
                            {
                                icb_doc_consecutivos buscarElemento = context.icb_doc_consecutivos.FirstOrDefault(x =>
                                    x.doccons_idtpdoc == modelo.doc_registros && x.doccons_bodega == grupo.bodega_id);
                                buscarElemento.doccons_siguiente = buscarElemento.doccons_siguiente + 1;
                                context.Entry(buscarElemento).State = EntityState.Modified;
                            }

                            context.SaveChanges();

                            // actualizacion  070718
                            icb_vehiculo vehiculoactualiza =
                                context.icb_vehiculo.FirstOrDefault(x => x.plan_mayor == modelo.plan_mayor);
                            vehiculoactualiza.proveedor_id = modelo.proveedor_id;
                            context.Entry(vehiculoactualiza).State = EntityState.Modified;
                            context.SaveChanges();
                            // actualizacion  070718

                            ViewBag.numFacturacionCreada = numeroConsecutivo;
                            TempData["mensaje"] = "El vehiculo se ha guardado exitosamente con numero de documento ";
                            TempData["mensaje_valor_numerico"] = numeroConsecutivo;
                            return RedirectToAction("ManualCargueNuevos");
                        }

                        TempData["mensaje_error"] = "Error de conexion con base de datos";
                        ;
                    }
                    else
                    {
                        TempData["mensaje_error"] = "El vehiculo ya fue comprado con numero de documento " +
                                                    buscarAutomovil.numero + " en la fecha " +
                                                    buscarAutomovil.fecha.ToShortDateString();
                    }
                }
            }
            else
            {
                TempData["mensaje_error_cero"] = "Por favor validar campos del formulario";
                List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();
            }

            ListasDesplegablesManualVhNuevo();
            BuscarFavoritos(menu);
            return View(modelo);
        }

        public void ListasDesplegablesManualVhNuevo()
        {
            //ViewBag.ciudadplaca = new SelectList(context.nom_ciudad, "ciu_id", "ciu_nombre");
            ViewBag.modvh_id = new SelectList(context.modelo_vehiculo, "modvh_codigo", "modvh_nombre");
            ViewBag.colvh_id = new SelectList(context.color_vehiculo, "colvh_id", "colvh_nombre");
            ViewBag.tpvh_id = new SelectList(context.tipo_vehiculo, "tpvh_id", "tpvh_nombre");

            var buscarTipoDocumento = (from tipoDocumento in context.tp_doc_registros
                                       where tipoDocumento.tipo == 1
                                       select new
                                       {
                                           tipoDocumento.tpdoc_id,
                                           nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                           tipoDocumento.tipo
                                       }).ToList();
            ViewBag.tipo_documentoFiltro = new SelectList(buscarTipoDocumento, "tpdoc_id", "nombre");
            var provedores = (from pro in context.tercero_proveedor
                              join ter in context.icb_terceros
                                  on pro.tercero_id equals ter.tercero_id
                              select new
                              {
                                  idTercero = ter.tercero_id,
                                  ter.doc_tercero,
                                  nombreTErcero = ter.prinom_tercero + " " + ter.segnom_tercero,
                                  apellidosTercero = ter.apellido_tercero + " " + ter.segapellido_tercero,
                                  razonSocial = ter.razon_social
                              }).OrderBy(x => x.nombreTErcero);

            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in provedores)
            {
                string nombre = "(" + item.doc_tercero + ") " + item.razonSocial + item.nombreTErcero + " " +
                             item.apellidosTercero;
                items.Add(new SelectListItem { Text = nombre, Value = item.idTercero.ToString() });
            }

            ViewBag.proveedor_id = items;

            var descripcion = from a in context.icb_vehiculo
                              join b in context.modelo_vehiculo
                                  on a.modvh_id equals b.modvh_codigo
                              join c in context.color_vehiculo
                                  on a.colvh_id equals c.colvh_nombre
                              select new
                              {
                                  a.modvh_id,
                                  b.modvh_nombre,
                                  c.colvh_nombre
                              };
            List<SelectListItem> itemsVe = new List<SelectListItem>();
            foreach (var itemV in descripcion)
            {
                string nombre = itemV.modvh_nombre + " | " + itemV.colvh_nombre;
                itemsVe.Add(new SelectListItem { Text = nombre, Value = itemV.modvh_id });
            }

            ViewBag.Descripcion = itemsVe;
            icb_sysparameter buscarParametroSw = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P23");
            string swParametro = buscarParametroSw != null ? buscarParametroSw.syspar_value : "1";
            int idTipoSw = Convert.ToInt32(swParametro);
            ViewBag.doc_registros =
                new SelectList(
                    context.tp_doc_registros.Where(x => x.sw == idTipoSw && x.tipo == 1).OrderBy(x => x.tpdoc_nombre),
                    "tpdoc_id", "tpdoc_nombre");
            ViewBag.condicion_pago = new SelectList(context.fpago_tercero, "fpago_id", "fpago_nombre");
        }

        public ActionResult ManualCargueUsados(int? menu)
        {
            //ViewBag.ciudadplaca = new SelectList(context.nom_ciudad, "ciu_id", "ciu_nombre");
            ViewBag.modvh_id = new SelectList(context.modelo_vehiculo, "modvh_codigo", "modvh_nombre");
            ViewBag.colvh_id = new SelectList(context.color_vehiculo, "colvh_id", "colvh_nombre");
            //ViewBag.tpvh_id = new SelectList(context.tipo_vehiculo, "tpvh_id", "tpvh_nombre");
            var provedores = (from pro in context.tercero_proveedor
                              join ter in context.icb_terceros
                                  on pro.tercero_id equals ter.tercero_id
                              select new
                              {
                                  idTercero = ter.tercero_id,
                                  nombreTErcero = ter.prinom_tercero,
                                  apellidosTercero = ter.apellido_tercero
                              }).OrderBy(x => x.nombreTErcero);
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in provedores)
            {
                items.Add(new SelectListItem { Text = item.nombreTErcero, Value = item.idTercero.ToString() });
            }

            ViewBag.proveedor_id = items;
            ViewBag.cod_bod = new SelectList(context.bodega_concesionario, "bodccs_cod", "bodccs_nombre");
            icb_sysparameter buscarParametroSw = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P23");
            string swParametro = buscarParametroSw != null ? buscarParametroSw.syspar_value : "1";
            int idTipoSw = Convert.ToInt32(swParametro);
            ViewBag.doc_registros =
                new SelectList(context.tp_doc_registros.Where(x => x.sw == idTipoSw).OrderBy(x => x.tpdoc_nombre),
                    "tpdoc_id", "tpdoc_nombre");
            ViewBag.condicion_pago = new SelectList(context.fpago_tercero, "fpago_id", "fpago_nombre");
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult ManualCargueUsados(CargueManualVhUsadoModel modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                icb_sysparameter buscarParametroTpVh = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P24");
                string tpVhParametro = buscarParametroTpVh != null ? buscarParametroTpVh.syspar_value : "5";
                // El tipo de vehicul 5 corresponde a tipo usados de la tabla tipo vehiculo en DB
                int idTipoVehiculo = Convert.ToInt32(tpVhParametro);

                context.icb_vehiculo.Add(new icb_vehiculo
                {
                    plan_mayor = modelo.plac_vh,
                    icbvhfec_creacion = DateTime.Now,
                    icbvhuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    icbvh_estado = true,
                    vin = modelo.vin,
                    nummot_vh = modelo.nummot_vh,
                    plac_vh = modelo.plac_vh,
                    modvh_id = modelo.modvh_id,
                    anio_vh = modelo.anio_vh,
                    colvh_id = modelo.colvh_id,
                    proveedor_id = modelo.proveedor_id,
                    tpvh_id = idTipoVehiculo,
                    Notas = modelo.notas,
                    flota = modelo.es_flota
                });
                int guardarVehiculo = context.SaveChanges();
                if (guardarVehiculo > 0)
                {
                    TempData["mensaje"] = "El vehiculo se ha guardado exitosamente";
                }
                else
                {
                    TempData["mensaje_error"] = "Error de conexion con base de datos";
                }
            }

            //ViewBag.ciudadplaca = new SelectList(context.nom_ciudad, "ciu_id", "ciu_nombre");
            ViewBag.modvh_id = new SelectList(context.modelo_vehiculo, "modvh_codigo", "modvh_nombre");
            ViewBag.colvh_id = new SelectList(context.color_vehiculo, "colvh_id", "colvh_nombre");
            //ViewBag.tpvh_id = new SelectList(context.tipo_vehiculo, "tpvh_id", "tpvh_nombre");
            var provedores = (from pro in context.tercero_proveedor
                              join ter in context.icb_terceros
                                  on pro.tercero_id equals ter.tercero_id
                              select new
                              {
                                  idTercero = ter.tercero_id,
                                  nombreTErcero = ter.prinom_tercero,
                                  apellidosTercero = ter.apellido_tercero
                              }).OrderBy(x => x.nombreTErcero);
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in provedores)
            {
                items.Add(new SelectListItem { Text = item.nombreTErcero, Value = item.idTercero.ToString() });
            }

            ViewBag.proveedor_id = items;
            ViewBag.cod_bod = new SelectList(context.bodega_concesionario, "bodccs_cod", "bodccs_nombre");
            icb_sysparameter buscarParametroSw = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P23");
            string swParametro = buscarParametroSw != null ? buscarParametroSw.syspar_value : "1";
            int idTipoSw = Convert.ToInt32(swParametro);
            ViewBag.doc_registros =
                new SelectList(context.tp_doc_registros.Where(x => x.sw == idTipoSw).OrderBy(x => x.tpdoc_nombre),
                    "tpdoc_id", "tpdoc_nombre");
            ViewBag.condicion_pago = new SelectList(context.fpago_tercero, "fpago_id", "fpago_nombre");
            ViewBag.anioSeleccionado = modelo.anio_vh;
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult BuscarAniosModeloPorId(string modeloCodigo)
        {
            var buscarAnios = context.anio_modelo.Where(x => x.codigo_modelo == modeloCodigo).Select(x => new
            {
                x.anio
            }).ToList();
            return Json(buscarAnios, JsonRequestBehavior.AllowGet);
        }

        // GET: inventario_vhNuevo
        public ActionResult Crear(int? menu)
        {
            ViewBag.tipos = new SelectList(context.tipo_vehiculo, "tpvh_id", "tpvh_nombre");
            ViewBag.marcas = new SelectList(context.marca_vehiculo, "marcvh_id", "marcvh_nombre");
            ViewBag.modelos = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewBag.estilos = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewBag.color = new SelectList(context.color_vehiculo, "colvh_id", "colvh_nombre");
            ViewBag.clasificacion = new SelectList(context.clacompra_vehiculo, "clacompvh_id", "clacompvh_nombre");
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult Update(int id)
        {
            icb_vehiculo vehiculo = context.icb_vehiculo.FirstOrDefault(x => x.icbvh_id == id);
            List<modelo_vehiculo> modelosActual = context.modelo_vehiculo.Where(x => x.mar_vh_id == vehiculo.marcvh_id).ToList();
            List<estilo_vehiculo> estilosActual = context.estilo_vehiculo.Where(x => x.mod_vh_id == vehiculo.modvh_id).ToList();

            List<tramitador_vh> listaTramitadores = new List<tramitador_vh>();
            tramitador_vh tramitadorActual = context.tramitador_vh.Find(vehiculo.tramitador_id);
            if (tramitadorActual != null)
            {
                listaTramitadores = context.tramitador_vh
                    .Where(x => x.tramitador_idtipo == tramitadorActual.tramitador_idtipo).ToList();
            }

            var vehiculoAux = new
            {
                vehiculo.icbvh_id,
                vehiculo.colvh_id,
                vehiculo.marcvh_id,
                vehiculo.modvh_id,
                num_serie = vehiculo.vin,
                vehiculo.nummot_vh,
                vehiculo.anio_vh,
                vehiculo.plac_vh,
                vehiculo.proveedor_id,
                cod_bod = vehiculo.id_bod,
                vehiculo.fecfact_fabrica,
                vehiculo.plan_mayor,
                vehiculo.numpedido_vh,
                vehiculo.iva_vh,
                vehiculo.nummanf_vh,
                vehiculo.diaslibres_vh,
                vehiculo.ciumanf_vh,
                vehiculo.tramitador_id
            };

            var mod = modelosActual.Select(x => new
            {
                x.modvh_codigo,
                x.modvh_nombre
            });
            var est = estilosActual.Select(x => new
            {
                x.estvh_id,
                x.estvh_nombre
            });

            var tram = listaTramitadores.Select(x => new
            {
                x.tramitador_id,
                nombreCompleto = x.tramitadorpri_nombre + " " + x.tramitadorseg_nombre + " " + x.tramitador_apellidos
            });

            int anioManifiesto = 0;
            int mesManifiesto = 0;
            int diaManifiesto = 0;
            if (vehiculo.fecman_vh != null)
            {
                anioManifiesto = vehiculo.fecman_vh.Value.Year;
                mesManifiesto = vehiculo.fecman_vh.Value.Month - 1;
                diaManifiesto = vehiculo.fecman_vh.Value.Day;
            }

            var result = new
            {
                anio = vehiculo.fecfact_fabrica.Value.Year,
                mes = vehiculo.fecfact_fabrica.Value.Month - 1,
                dia = vehiculo.fecfact_fabrica.Value.Day,
                anioManifiesto,
                mesManifiesto = mesManifiesto - 1,
                diaManifiesto,
                // Fechas panel de entrega de vehiculos 
                anioFechaEntregaTrans =
                    vehiculo.fecentregatransp_vh != null ? vehiculo.fecentregatransp_vh.Value.Year : 0,
                mesFechaEntregaTrans = vehiculo.fecentregatransp_vh != null
                    ? vehiculo.fecentregatransp_vh.Value.Month - 1
                    : 0,
                diaFechaEntregaTrans =
                    vehiculo.fecentregatransp_vh != null ? vehiculo.fecentregatransp_vh.Value.Day : 0,
                anioFechaLlegada = vehiculo.fecllegadaccs_vh != null ? vehiculo.fecllegadaccs_vh.Value.Year : 0,
                mesFechaLlegada = vehiculo.fecllegadaccs_vh != null ? vehiculo.fecllegadaccs_vh.Value.Month - 1 : 0,
                diaFechaLlegada = vehiculo.fecllegadaccs_vh != null ? vehiculo.fecllegadaccs_vh.Value.Day : 0,
                horaLlegada = vehiculo.fecllegadaccs_vh != null ? vehiculo.fecllegadaccs_vh.Value.Hour : 0,
                minutoLlegada = vehiculo.fecllegadaccs_vh != null ? vehiculo.fecllegadaccs_vh.Value.Minute : 0,
                anioEntregaManif = vehiculo.fecentman_vh != null ? vehiculo.fecentman_vh.Value.Year : 0,
                mesEntregaManif = vehiculo.fecentman_vh != null ? vehiculo.fecentman_vh.Value.Month - 1 : 0,
                diaEntregaManif = vehiculo.fecentman_vh != null ? vehiculo.fecentman_vh.Value.Day : 0,
                anioMatrRunt = vehiculo.fecharunt_vh != null ? vehiculo.fecharunt_vh.Value.Year : 0,
                mesMatrRunt = vehiculo.fecharunt_vh != null ? vehiculo.fecharunt_vh.Value.Month - 1 : 0,
                diaMatrRunt = vehiculo.fecharunt_vh != null ? vehiculo.fecharunt_vh.Value.Day : 0,
                buscado = vehiculoAux,
                costosiniva = string.Format(CultureInfo.CreateSpecificCulture("el-GR"), "{0:0,0}",
                    vehiculo.costosiniva_vh),
                valorIva = string.Format(CultureInfo.CreateSpecificCulture("el-GR"), "{0:0,0}",
                    vehiculo.costosiniva_vh * vehiculo.iva_vh / 100),
                valorTotal = string.Format(CultureInfo.CreateSpecificCulture("el-GR"), "{0:0,0}",
                    vehiculo.costosiniva_vh + vehiculo.costosiniva_vh * vehiculo.iva_vh / 100),
                mod,
                est,
                tram,
                tipoTramitador = tramitadorActual != null ? tramitadorActual.tramitador_idtipo : 0
            };


            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ActualizarVehiculo(int id, int bodega, int marca, string modelo, string color, string serie,
            string motor, int anio, string placa)
        {
            int result = 0;
            icb_vehiculo QueryVehiculo = context.icb_vehiculo.FirstOrDefault(x => x.icbvh_id == id);

            icb_vehiculo motorVh = context.icb_vehiculo.FirstOrDefault(x => x.nummot_vh == motor);
            icb_vehiculo serieVh = context.icb_vehiculo.FirstOrDefault(x => x.vin == serie);
            icb_vehiculo placaVh = context.icb_vehiculo.FirstOrDefault(x => x.plac_vh == placa);

            if (motorVh != null && QueryVehiculo.icbvh_id != motorVh.icbvh_id)
            {
                result = -1;
            }
            else if (serieVh != null && QueryVehiculo.icbvh_id != serieVh.icbvh_id)
            {
                result = -2;
            }
            else if (placaVh != null && QueryVehiculo.icbvh_id != placaVh.icbvh_id)
            {
                result = -3;
            }
            else
            {
                QueryVehiculo.anio_vh = anio;
                QueryVehiculo.colvh_id = color;
                QueryVehiculo.icbvhfec_actualizacion = DateTime.Now;
                QueryVehiculo.icbvhuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                QueryVehiculo.marcvh_id = marca;
                QueryVehiculo.modvh_id = modelo;
                QueryVehiculo.nummot_vh = motor;
                QueryVehiculo.plac_vh = placa;
                QueryVehiculo.vin = serie;
                QueryVehiculo.id_bod = bodega;
                context.Entry(QueryVehiculo).State = EntityState.Modified;
                result = context.SaveChanges();
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarModelos(int marca)
        {
            List<modelo_vehiculo> marcas = context.modelo_vehiculo.Where(x => x.mar_vh_id == marca).ToList();
            var result = marcas.Select(x => new
            {
                x.modvh_codigo,
                x.modvh_nombre
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarEstilos(string modelo)
        {
            List<estilo_vehiculo> modelos = context.estilo_vehiculo.Where(x => x.mod_vh_id == modelo).ToList();
            var result = modelos.Select(x => new
            {
                x.estvh_id,
                x.estvh_nombre
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarTramitadores(int idTipoTramitador)
        {
            List<tramitador_vh> tramitadores = context.tramitador_vh.Where(x => x.tramitador_idtipo == idTipoTramitador).ToList();
            var result = tramitadores.Select(x => new
            {
                x.tramitador_id,
                nombreCompleto = x.tramitadorpri_nombre + " " + x.tramitadorseg_nombre + " " + x.tramitador_apellidos
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public ActionResult Import(HttpPostedFileBase excelfile)
        //{
        //    var sinBodega = context.bodega_concesionario.FirstOrDefault(x=>x.bodccs_cod=="000");
        //    var codigoBodega = 0;
        //    try
        //    {
        //        codigoBodega = Convert.ToInt32(Request["cod_bodega"]);
        //    }
        //    catch (System.FormatException)
        //    {
        //        //Validacion para cuando no se asigna una bodega al importar el archivo excel
        //    }

        //    var codigoProveedor = 0;
        //    try
        //    {
        //        codigoProveedor = Convert.ToInt32(Request["proveedor_id"]);
        //    }
        //    catch (System.FormatException)
        //    {
        //        //Validacion para cuando no se asigna un proveedor al importar el archivo excel
        //    }

        //    if (excelfile == null || excelfile.ContentLength == 0)
        //    {
        //        TempData["mensajeError"] = "El archivo esta vacio o no es un archivo valido!";
        //        return RedirectToAction("FacturacionGM", "gestionVhNuevo");
        //    }
        //    var buscaNombre = context.icb_arch_facturacion.FirstOrDefault(x => x.arch_fac_nombre == excelfile.FileName);

        //    if (buscaNombre != null)
        //    {
        //        TempData["mensajeError"] = "El nombre del archivo ya se encuentra registrado, verifique que el archivo no ha sido cargado antes o cambie de nombre!";
        //        return RedirectToAction("FacturacionGM", "gestionVhNuevo");
        //    }

        //    else
        //    {
        //        if (excelfile.FileName.EndsWith("xls") || excelfile.FileName.EndsWith("xlsx"))
        //        {

        //            string path = Server.MapPath("~/Content/" + excelfile.FileName);
        //            // Validacion para cuando el archivo esta en uso y no puede ser usado desde visual 
        //            try
        //            {
        //                if (System.IO.File.Exists(path))
        //                {
        //                    System.IO.File.Delete(path);
        //                }
        //                excelfile.SaveAs(path);

        //            }
        //            catch (System.IO.IOException)
        //            {
        //                TempData["mensajeError"] = "El archivo esta siendo usado por otro proceso, asegurece de cerrarlo o cree una copia del archivo e intente de nuevo!";
        //                return RedirectToAction("FacturacionGM", "gestionVhNuevo");
        //            }


        //            // Read data from excel file
        //            Excel.Application application = new Excel.Application();
        //            Excel.Workbook workbook = application.Workbooks.Open(path);
        //            Excel.Worksheet worksheet = workbook.ActiveSheet;
        //            Excel.Range range = worksheet.UsedRange;


        //            List<CrearVehiculoExcel> listaVehiculos = new List<CrearVehiculoExcel>();
        //            //var errorFecha = false;
        //            //var errorNumeroFactura = false;

        //            var items = 0;
        //            string log = "";
        //            //int itemsTotales = 0;
        //            int itemsCorrectos = 0;
        //            int itemsFallidos = 0;
        //            var nombreArchivo = excelfile.FileName;
        //            items = range.Rows.Count - 5;
        //            //***icb_arch_facturacion archivoFacturacion = new icb_arch_facturacion();
        //            //***archivoFacturacion.arch_fac_nombre = nombreArchivo;
        //            //***archivoFacturacion.arch_fac_fecha = DateTime.Now;
        //            //***archivoFacturacion.items = items;
        //            //***context.icb_arch_facturacion.Add(archivoFacturacion);
        //            //***var resultArchFacturacion = context.SaveChanges();

        //            //**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**// Parametros quemados
        //            var buscarParametroStatus = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P17");
        //            var statusParametro = buscarParametroStatus!=null?buscarParametroStatus.syspar_value:"0";
        //            var buscarParametroTpVh = context.icb_sysparameter.FirstOrDefault(x=>x.syspar_cod=="P3");
        //            var tpHvParametro = buscarParametroTpVh != null ? buscarParametroTpVh.syspar_value : "4";
        //            var buscarParametroTpEvento = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P19");
        //            var tpEventoParametro = buscarParametroTpEvento != null ? buscarParametroTpEvento.syspar_value : "11";
        //            var buscarParametroMarcaVh = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P5");
        //            var marcaVhParametro = buscarParametroMarcaVh != null ? buscarParametroMarcaVh.syspar_value : "1";

        //            var tpvh_idAux = Convert.ToInt32(tpHvParametro);
        //            var id_eventoAux = Convert.ToInt32(tpEventoParametro);
        //            var marcvh_idAux = Convert.ToInt32(marcaVhParametro);

        //            //***var codigoArchFacturacion = context.icb_arch_facturacion.OrderByDescending(x => x.arch_fac_id).First().arch_fac_id;
        //            {
        //                items = range.Rows.Count - 5;
        //                //var usersList = new List<CrearVehiculoExcel>();

        //                for (int row = 5; row <= range.Rows.Count; row++)
        //                {
        //                    var errorVin = false;
        //                    CrearVehiculoExcel vehiculo = new CrearVehiculoExcel();
        //                    //***icb_vehiculo nuevoRegistroFactura = new icb_vehiculo();
        //                    var capturaFecha = (((Excel.Range)range.Cells[row, 2]).Text);

        //                    DateTime parseo = new DateTime();
        //                    try
        //                    {
        //                        parseo = DateTime.Parse(capturaFecha);
        //                        //***nuevoRegistroFactura.fecfact_fabrica = parseo;
        //                        vehiculo.fecfact_fabrica = parseo;

        //                        //Captura del numero de factura
        //                        //string test = (((Excel.Range)range.Cells[row, 3]).Text);
        //                        //decimal convert = Int64.Parse(test, System.Globalization.NumberStyles.Float);
        //                        //***nuevoRegistroFactura.plan_mayor = Convert.ToInt64((((Excel.Range)range.Cells[row, 3]).Text));
        //                        vehiculo.plan_mayor= Convert.ToInt64((((Excel.Range)range.Cells[row, 3]).Text));

        //                        string codigo = Convert.ToString((((Excel.Range)range.Cells[row, 6]).Text));
        //                        var anio = 2000 + Convert.ToInt32(codigo.Substring(0, 2));
        //                        //***nuevoRegistroFactura.anio_vh = anio;
        //                        vehiculo.anio_vh = anio;

        //                        //***nuevoRegistroFactura.kmat_zvsk = codigo.Substring(0, 13);
        //                        vehiculo.kmat = codigo.Substring(0, 13);

        //                        //Captura del codigo del modelo para conocer sus caracteristicas (modelo)
        //                        string codigoModelo = codigo.Substring(2, 11);
        //                        var buscaModelo = context.modelo_vehiculo.FirstOrDefault(x => x.modvh_codigo == codigoModelo);
        //                        if (buscaModelo == null)
        //                        {
        //                            //context.modelo_vehiculo.Add(new modelo_vehiculo()
        //                            //{
        //                            //    modvhuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
        //                            //    modvhfec_creacion = DateTime.Now,
        //                            //    modvh_codigo = codigoModelo,
        //                            //    modvh_estado = true,
        //                            //    modvhrazoninactivo = "No aplica",

        //                            //    modvh_nombre = ((((Excel.Range)range.Cells[row, 7]).Text)),
        //                            //    //La marca 1 es equivalente a chevrolet en la base de datos
        //                            //    mar_vh_id = Convert.ToInt32(marcaVhParametro),
        //                            //});
        //                            //context.SaveChanges();
        //                            TempData["mensajeError"] = "El codigo de modelo "+codigoModelo+" no existe, debe registrarlo antes de continuar, linea "+(row+1);
        //                            workbook.Close(0);
        //                            application.Quit();
        //                            System.IO.File.Delete(path);
        //                            return RedirectToAction("FacturacionGM", "gestionVhNuevo");
        //                            //***nuevoRegistroFactura.modvh_id = codigo.Substring(2, 11);
        //                        }
        //                        else
        //                        {
        //                            //***nuevoRegistroFactura.modvh_id = buscaModelo.modvh_codigo;
        //                            vehiculo.modvh_id = codigoModelo;
        //                        }

        //                        var buscaAnioModelo = context.anio_modelo.FirstOrDefault(x=>x.codigo_modelo==codigoModelo&&x.anio==anio);
        //                        if (buscaAnioModelo == null)
        //                        {
        //                            context.anio_modelo.Add(new anio_modelo() {
        //                                anio=anio,
        //                                codigo_modelo=codigoModelo,
        //                                descripcion = ((((Excel.Range)range.Cells[row, 7]).Text))
        //                            });
        //                            context.SaveChanges();
        //                        }

        //                        //Captura de la parte final del codigo del modelo referente al codigo del color del vehiculo
        //                        string codigoColor = codigo.Substring(14, 3);
        //                        //vehiculo.colvh_id = codigoColor;
        //                        var buscaColor = context.color_vehiculo.FirstOrDefault(x => x.colvh_id == codigoColor);
        //                        if (buscaColor == null)
        //                        {
        //                            //context.color_vehiculo.Add(new color_vehiculo()
        //                            //{
        //                            //    colvhuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
        //                            //    colvhfec_creacion = DateTime.Now,
        //                            //    colvh_id = codigoColor,
        //                            //    colvh_estado = true,
        //                            //    colvhrazoninactivo = null,
        //                            //    colvh_nombre = "Pendiente"
        //                            //});
        //                            //context.SaveChanges();
        //                            TempData["mensajeError"] = "El codigo de color " + codigoColor + " no existe, debe registrarlo antes de continuar, linea "+(row+1);
        //                            workbook.Close(0);
        //                            application.Quit();
        //                            System.IO.File.Delete(path);
        //                            return RedirectToAction("FacturacionGM", "gestionVhNuevo");
        //                            //***nuevoRegistroFactura.colvh_id = codigoColor;
        //                        }
        //                        else
        //                        {
        //                            //***nuevoRegistroFactura.colvh_id = buscaColor.colvh_id;
        //                            vehiculo.colvh_id = codigoColor;
        //                        }


        //                        string VIN = (((Excel.Range)range.Cells[row, 8]).Text);
        //                        var buscarSerie = context.icb_vehiculo.FirstOrDefault(x => x.vin == VIN);
        //                        if (buscarSerie != null)
        //                        {
        //                            itemsFallidos++;
        //                            errorVin = true;
        //                            if (log.Length < 1)
        //                            {
        //                                log += VIN + "*El numero VIN ya existe";
        //                            }
        //                            else
        //                            {
        //                                log += "|" + VIN + "*El numero VIN ya existe";
        //                            }
        //                        }
        //                        else
        //                        {
        //                            itemsCorrectos++;
        //                            //***nuevoRegistroFactura.vin = VIN;
        //                            vehiculo.vin = VIN;
        //                        }

        //                        string numeroMotor = (((Excel.Range)range.Cells[row, 9]).Text);
        //                        //***nuevoRegistroFactura.nummot_vh = numeroMotor;
        //                        vehiculo.nummot_vh = numeroMotor;
        //                        //***nuevoRegistroFactura.numpedido_vh = Convert.ToInt64((((Excel.Range)range.Cells[row, 11]).Text));
        //                        vehiculo.numpedido_vh = Convert.ToInt64((((Excel.Range)range.Cells[row, 11]).Text));
        //                        string ValorFactura = Convert.ToString(((Excel.Range)range.Cells[row, 12]).Text);
        //                        var convertirFormato = ValorFactura.Replace(".", "");
        //                        var valorFacturaFinal = Convert.ToDecimal(convertirFormato);
        //                        //***nuevoRegistroFactura.costosiniva_vh = (valorFacturaFinal * 100) / 119;
        //                        vehiculo.costototal_vh = valorFacturaFinal;

        //                        //***nuevoRegistroFactura.arch_fact_id = codigoArchFacturacion;
        //                        //***nuevoRegistroFactura.icbvhuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
        //                        //***nuevoRegistroFactura.icbvhfec_creacion = DateTime.Now;
        //                        //***nuevoRegistroFactura.icbvh_bodpro = codigoBodega;
        //                        //***nuevoRegistroFactura.cod_bod = sinBodega.bodccs_cod;
        //                        //***nuevoRegistroFactura.tpvh_id = Convert.ToInt32(buscarParametroTpVh);
        //                        //***nuevoRegistroFactura.id_evento = Convert.ToInt32(buscarParametroTpEvento);
        //                        //***nuevoRegistroFactura.marcvh_id = Convert.ToInt32(marcaVhParametro);                           
        //                        //***nuevoRegistroFactura.icbvh_estatus = statusParametro;


        //                        if (!errorVin)
        //                        {
        //                            listaVehiculos.Add(vehiculo);
        //                            //    context.icb_vehiculo.Add(nuevoRegistroFactura);
        //                            //    context.SaveChanges();
        //                            //    var ultimoRegistro = context.icb_vehiculo.OrderByDescending(x => x.icbvh_id).First();
        //                            //    // Se agrega la trazabilidad del vehiculo nuevo, en este caso lo primero es facturacion
        //                            //    context.icb_vehiculo_eventos.Add(new icb_vehiculo_eventos() {
        //                            //        evento_estado=true,
        //                            //        eventofec_creacion=DateTime.Now,
        //                            //        eventouserid_creacion= Convert.ToInt32(Session["user_usuarioid"]),
        //                            //        evento_nombre= "Facturacion",
        //                            //        id_tpevento=1,
        //                            //        bodega_cod = codigoBodega,
        //                            //        id_vehiculo=ultimoRegistro.icbvh_id,
        //                            //        vin=ultimoRegistro.vin
        //                            //    });
        //                            //    context.SaveChanges();
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        TempData["mensajeError"] = "Error al leer archivo, revise que los campos no esten vacios o mal escritos, linea " + (row + 1);
        //                        workbook.Close(0);
        //                        application.Quit();
        //                        System.IO.File.Delete(path);
        //                        return RedirectToAction("FacturacionGM", "gestionVhNuevo");
        //                    }
        //                }// Fin del ciclo FOR
        //            }

        //            /// Si llega hasta aqui significa que el archivo se leyo correctamente
        //            icb_arch_facturacion archivoFacturacion = new icb_arch_facturacion();
        //            archivoFacturacion.arch_fac_nombre = nombreArchivo;
        //            archivoFacturacion.arch_fac_fecha = DateTime.Now;
        //            archivoFacturacion.items = (itemsCorrectos+itemsFallidos);
        //            context.icb_arch_facturacion.Add(archivoFacturacion);
        //            var resultArchFacturacion = context.SaveChanges();

        //            if (resultArchFacturacion > 0) {
        //                var codigoArchFacturacion = context.icb_arch_facturacion.OrderByDescending(x => x.arch_fac_id).First().arch_fac_id;
        //                foreach (var vehiculo in listaVehiculos) {

        //                    context.icb_vehiculo.Add(new icb_vehiculo() {
        //                        fecfact_fabrica = vehiculo.fecfact_fabrica,
        //                        plan_mayor=vehiculo.plan_mayor,
        //                        anio_vh = vehiculo.anio_vh,
        //                        kmat_zvsk = vehiculo.kmat,
        //                        modvh_id = vehiculo.modvh_id,
        //                        colvh_id=vehiculo.colvh_id,
        //                        vin = vehiculo.vin,
        //                        nummot_vh = vehiculo.nummot_vh,
        //                        numpedido_vh = vehiculo.numpedido_vh,
        //                        costototal_vh = vehiculo.costototal_vh,
        //                        arch_fact_id = codigoArchFacturacion,
        //                        icbvhuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
        //                        icbvhfec_creacion = DateTime.Now,
        //                        //icbvh_bodpro = codigoBodega,
        //                        id_bod = sinBodega.id,
        //                        tpvh_id = tpvh_idAux,
        //                        id_evento = id_eventoAux,
        //                        marcvh_id = marcvh_idAux,                         
        //                        icbvh_estatus = statusParametro
        //                    });
        //                    context.SaveChanges();

        //                    var ultimoRegistro = context.icb_vehiculo.OrderByDescending(x => x.icbvh_id).FirstOrDefault();
        //                    // Se agrega la trazabilidad del vehiculo nuevo, en este caso lo primero es facturacion
        //                    context.icb_vehiculo_eventos.Add(new icb_vehiculo_eventos()
        //                    {
        //                        evento_estado = true,
        //                        eventofec_creacion = DateTime.Now,
        //                        eventouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
        //                        evento_nombre = "Facturacion",
        //                        id_tpevento = 1,
        //                        bodega_id = codigoBodega,
        //                        //id_vehiculo = ultimoRegistro.icbvh_id,
        //                        vin = ultimoRegistro.vin
        //                    });                        
        //                        context.SaveChanges();  
        //                }


        //                icb_arch_facturacion_log archLog = new icb_arch_facturacion_log();
        //                archLog.fact_log_fecha = DateTime.Now;
        //                archLog.fact_log_itemscorrecto = itemsCorrectos;
        //                archLog.fact_log_itemserror = itemsFallidos;
        //                archLog.fact_log_nombrearchivo = nombreArchivo;
        //                archLog.fact_log_items = (itemsCorrectos+itemsFallidos);
        //                archLog.fact_log_log = log;
        //                archLog.id_arch_facturacion = codigoArchFacturacion;
        //                context.icb_arch_facturacion_log.Add(archLog);

        //                context.SaveChanges();

        //            }

        //            //excelfile.InputStream.Close();
        //            //excelfile.InputStream.Dispose();
        //            TempData["mensajeError"] = "La lectura del archivo se realizo correctamente!";
        //            return RedirectToAction("FacturacionGM", "gestionVhNuevo");
        //        }
        //        else
        //        {
        //            TempData["mensajeError"] = "La lectura del archivo no fue correcta, verifique que selecciono un archivo valido.";
        //            return RedirectToAction("FacturacionGM", "gestionVhNuevo");
        //        }
        //    }
        //}

        public JsonResult AgregarVehiculo(int bodega, string fechaCompra, int factura, int marca, string modelo,
            int estilo, string color,
            string serie, string motor, int anio, string placa, string planMayor)
        {
            int result = 0;
            int motorVh = context.icb_vehiculo.Where(x => x.nummot_vh == motor).Count();
            if (motorVh > 0 && motor.Length > 0)
            {
                result = -1;
            }

            int serieVh = context.icb_vehiculo.Where(x => x.vin == serie).Count();
            if (serieVh > 0 && serie.Length > 0)
            {
                result = -2;
            }

            int placaVh = context.icb_vehiculo.Where(x => x.plac_vh == placa).Count();
            if (placaVh > 0 && placa.Length > 0)
            {
                result = -3;
            }

            if (motorVh == 0 && serieVh == 0 && placaVh == 0)
            {
                int dia = Convert.ToInt32(fechaCompra.Substring(0, 2));
                int mes = Convert.ToInt32(fechaCompra.Substring(3, 2));
                int anioFecha = Convert.ToInt32(fechaCompra.Substring(6, 4));

                icb_sysparameter buscarParametroTpVh = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P3");
                string tpHvParametro = buscarParametroTpVh != null ? buscarParametroTpVh.syspar_value : "4";

                icb_vehiculo vehiculoNuevo = new icb_vehiculo
                {
                    anio_vh = anio,
                    colvh_id = color,
                    estvh_id = estilo,
                    icbvhfec_creacion = DateTime.Now,
                    icbvhuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    marcvh_id = marca,
                    modvh_id = modelo,
                    nummot_vh = motor,
                    plac_vh = placa,
                    // Id del tipo de vehiculo usado registrado en la BD
                    tpvh_id = Convert.ToInt32(tpHvParametro),
                    vin = serie,
                    fecfact_fabrica = new DateTime(anioFecha, mes, dia),
                    id_bod = bodega,
                    plan_mayor = planMayor,
                    numpedido_vh = factura
                };
                context.icb_vehiculo.Add(vehiculoNuevo);
                result = context.SaveChanges();
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ImportarPedidos(HttpPostedFileBase excelfile, int? menu)
        {
            long codigoFactura = 0;
            int cantidadesTotal = 0;
            int cantidadesParcial = 0;
            int SumaTotalExcel = 0;
            string path = "";
            List<CrearPedidoExcel> listaPedidos = new List<CrearPedidoExcel>();
            try
            {
                codigoFactura = Convert.ToInt64(Request["cod_factura"]);
            }
            catch (FormatException)
            {
                //Validacion para cuando no se asigna un codigo factura al importar el archivo excel
                TempData["mensajeError"] = "No ha digito el numero de pedido requerido en la opcion cargar pedido!";
                return RedirectToAction("PedidoEnFirme", "gestionVhNuevo", new { menu });
            }

            pedido_GM buscaPedido = context.pedido_GM.FirstOrDefault(x => x.pedido_codigo == codigoFactura);
            // Significa que el numero del pedido ya esta en base de datos
            if (buscaPedido != null)
            {
                TempData["mensajeError"] = "El numero de pedido digitado ya se encuentra registrado en base de datos!";
                return RedirectToAction("PedidoEnFirme", "gestionVhNuevo", new { menu });
            }

            // Cuando se valida el numero de pedido y no existe en base de datos, por tanto se empieza a leer el archivo excel
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                TempData["mensajeError"] = "El archivo esta vacio o no es un archivo valido!";
                return RedirectToAction("PedidoEnFirme", "gestionVhNuevo", new { menu });
            }

            if (excelfile.FileName.EndsWith("xls") || excelfile.FileName.EndsWith("xlsx"))
            {
                path = Server.MapPath("~/Content/" + excelfile.FileName);
                // Validacion para cuando el archivo esta en uso y no puede ser usado desde visual 
                try
                {
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    excelfile.SaveAs(path);
                }
                catch (IOException)
                {
                    TempData["mensajeError"] =
                        "El archivo esta siendo usado por otro proceso, asegurece de cerrarlo o cree una copia del archivo e intente de nuevo!";
                    return RedirectToAction("PedidoEnFirme", "gestionVhNuevo", new { menu });
                }


                List<CrearVehiculoExcel> usersList = new List<CrearVehiculoExcel>();

                ExcelPackage package = new ExcelPackage(new FileInfo(path));
                ExcelWorksheet workSheet = package.Workbook.Worksheets[1];

                ////// Read data from excel file
                //////Excel.Application application = new Excel.Application();
                //////Excel.Workbook workbook = application.Workbooks.Open(path);
                //////Excel.Worksheet worksheet = workbook.ActiveSheet;
                //////Excel.Range range = worksheet.UsedRange;
                //////int totalFilas = range.Rows.Count;
                ////for (int row = 2; row < range.Rows.Count; row++)
                ////{
                int totalFilas = workSheet.Dimension.End.Row;
                for (int i = workSheet.Dimension.Start.Row + 1; i < workSheet.Dimension.End.Row; i++)
                {
                    CrearPedidoExcel crearPedido = new CrearPedidoExcel();
                    try
                    {
                        // kmatAuxiliar contiene el valor de las filas que contienen el total
                        ////string kmatAuxiliar = Convert.ToString((((Excel.Range)range.Cells[row, 1]).Text));
                        string kmatAuxiliar = workSheet.Cells[i, 1].Value.ToString();

                        //string kmat = Convert.ToString((((Excel.Range)range.Cells[row, 1]).Text));
                        string kmat = workSheet.Cells[i, 1].Value.ToString();
                        if (kmat.Count() < 12)
                        {
                            kmatAuxiliar = kmat;
                            ////cantidadesTotal += Convert.ToInt32((((Excel.Range)range.Cells[row, 3]).Text));
                            cantidadesTotal += Convert.ToInt32(workSheet.Cells[i, 3].Value.ToString());
                        }
                        else
                        {
                            kmat = kmat.Trim();
                            if (kmat.Count() == 17)
                            {
                                crearPedido.kmat = kmat.Substring(0, 17);
                                int anio = 2000 + Convert.ToInt32(kmat.Substring(0, 2));
                                crearPedido.anioModelo = anio;

                                //Captura del codigo del modelo para conocer sus caracteristicas (modelo)
                                string codigoModelo = kmat.Substring(2, 11);
                                crearPedido.codigoModelo = codigoModelo;

                                //Captura de la parte final del codigo del modelo referente al codigo del color del vehiculo
                                string codigoColor = kmat.Substring(14, 3);
                                crearPedido.codigoColor = codigoColor;

                                //Captura de la descripcion del modelo
                                ////crearPedido.nombreModelo = (((Excel.Range)range.Cells[row, 2]).Text);
                                crearPedido.nombreModelo = workSheet.Cells[i, 2].Value.ToString();

                                ////int cantidad = Convert.ToInt32((((Excel.Range)range.Cells[row, 3]).Text));
                                int cantidad = Convert.ToInt32(workSheet.Cells[i, 3].Value.ToString());
                                crearPedido.cantidadPedida = cantidad;

                                ////cantidadesParcial += Convert.ToInt32((((Excel.Range)range.Cells[row, 3]).Text));
                                cantidadesParcial += Convert.ToInt32(workSheet.Cells[i, 3].Value.ToString());
                                listaPedidos.Add(crearPedido);
                            }
                            else
                            {

                            }

                        }
                    }
                    // Excepcion cuando el kmat no esta escrito completo o el campo esta vacio
                    catch (Exception ex)
                    {
                        if (ex is ArgumentOutOfRangeException || ex is FormatException)
                        {
                            //workbook.Close(0);
                            //application.Quit();
                            excelfile.InputStream.Close();
                            excelfile.InputStream.Dispose();
                            System.IO.File.Delete(path);
                            TempData["mensajeError"] =
                                "Error al leer el archivo, verifique que los datos estan bien escritos, linea " + i;
                            return RedirectToAction("PedidoEnFirme", "gestionVhNuevo", new { menu });
                        }
                    }
                } // Fin del ciclo for donde se leen los registros de excel

                try
                {
                    //var test = range.Rows.Count;
                    //var test1 = (((Excel.Range)range.Cells[range.Rows.Count, 3]).Text);
                    //int SumaTotalCeldaExcel = Convert.ToInt32((((Excel.Range)range.Cells[range.Rows.Count, 3]).Text));
                    int SumaTotalCeldaExcel = Convert.ToInt32(workSheet.Cells[totalFilas, 3].Value);
                    SumaTotalExcel = SumaTotalCeldaExcel;
                }
                catch (FormatException)
                {
                    excelfile.InputStream.Close();
                    excelfile.InputStream.Dispose();
                    System.IO.File.Delete(path);
                    //workbook.Close(0);
                    //application.Quit();
                    //System.IO.File.Delete(path);
                    //workbook.Close(0);
                    //application.Quit();
                    System.IO.File.Delete(path);
                    TempData["mensajeError"] =
                        "Error al leer el archivo, el dato del total esta mal escrito o escrito en la columna incorrecta, linea " +
                        totalFilas;
                    return RedirectToAction("PedidoEnFirme", "gestionVhNuevo", new { menu });
                }

                //workbook.Close(0);
                //application.Quit();
                excelfile.InputStream.Close();
                excelfile.InputStream.Dispose();
            }

            //excelfile.InputStream.Close();
            //excelfile.InputStream.Dispose();
            //System.IO.File.Delete(path);
            //}


            // workbook.Close(0);
            //application.Quit();
            //.IO.File.Delete(path);

            //Validacion para conocer si la suma de cada vehiculo agregado es igual al total
            if (cantidadesParcial != SumaTotalExcel)
            {
                TempData["mensajeError"] = "La cantidad de vehiculos agregados es " + cantidadesParcial +
                                           " y la suma total es " + SumaTotalExcel + "!";
                try
                {
                    System.IO.File.Delete(path);
                }
                catch (Exception ex)
                {
                    Exception exception = ex.InnerException;
                }
            }
            else
            {
                pedido_GM nuevoPedido = new pedido_GM
                {
                    pedido_codigo = codigoFactura,
                    pedido_usuario_id = Convert.ToInt32(Session["user_usuarioid"]),
                    pedido_fecha = DateTime.Now
                };
                context.pedido_GM.Add(nuevoPedido);
                int guardar = context.SaveChanges();


                if (guardar > 0)
                {
                    pedido_GM ultimoPedido = context.pedido_GM.OrderByDescending(x => x.pedido_fecha).First();
                    foreach (CrearPedidoExcel item in listaPedidos)
                    {
                        //Consulta si existe el modelo, si no existe lo agrega
                        modelo_vehiculo buscarModelo =
                            context.modelo_vehiculo.FirstOrDefault(x => x.modvh_codigo == item.codigoModelo);

                        icb_sysparameter buscarParametroMarcaVh = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P5");
                        string marcaVhParametro =
                            buscarParametroMarcaVh != null ? buscarParametroMarcaVh.syspar_value : "1";

                        if (buscarModelo == null)
                        {
                            modelo_vehiculo nuevoModelo = new modelo_vehiculo
                            {
                                modvh_codigo = item.codigoModelo,
                                modvh_nombre = item.nombreModelo,
                                modvh_estado = true,
                                mar_vh_id = Convert.ToInt32(marcaVhParametro),
                                modvhrazoninactivo = "no aplica",
                                modvhuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                modvhfec_creacion = DateTime.Now
                            };
                            context.modelo_vehiculo.Add(nuevoModelo);
                            context.SaveChanges();
                        }

                        // Consulta si existe el color, si no existe lo agrega
                        color_vehiculo buscarColor = context.color_vehiculo.FirstOrDefault(x => x.colvh_id == item.codigoColor);
                        if (buscarColor == null)
                        {
                            color_vehiculo nuevoColor = new color_vehiculo
                            {
                                colvh_id = item.codigoColor,
                                colvh_nombre = "Pendiente",
                                colvh_estado = true,
                                colvhrazoninactivo = "no aplica",
                                colvhuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                colvhfec_creacion = DateTime.Now
                            };
                            context.color_vehiculo.Add(nuevoColor);
                            context.SaveChanges();
                        }

                        //Se agrega cada detalle del pedido
                        detallePedido_GM nuevoDetalle = new detallePedido_GM
                        {
                            detallePedido_kmat_zvsk = item.kmat,
                            detallePedido_anioModelo = item.anioModelo,
                            detallePedido_cantidad = item.cantidadPedida,
                            detallePedido_colorCodigo = item.codigoColor,
                            detallePedido_modeloCodigo = item.codigoModelo,
                            detallePedido_descripcion = item.nombreModelo,
                            detallePedido_pedidoCodigo = ultimoPedido.pedido_codigo
                        };
                        context.detallePedido_GM.Add(nuevoDetalle);
                    }

                    context.SaveChanges();
                    TempData["mensajeSuccess"] = "La lectura del archivo se realizo correctamente!";
                }
            }

            return RedirectToAction("PedidoEnFirme", "gestionVhNuevo", new { menu });
            //TempData["mensajeError"] = "La lectura del archivo no fue correcta!";
            //return RedirectToAction("PedidoEnFirme", "gestionVhNuevo");
        }

        public ActionResult BrowserAsesor(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult BuscarPedidosPaginados( /*string campo, string texto, int pagina*/)
        {
            //var pedidosTotal = (from pedido in context.pedido_GM
            //                        //join usuario in db.users
            //                        //on pedido.pedido_usuario_id equals usuario.user_id
            //                        //where (pedido.pedido_codigo.ToString().Contains(texto)                                   
            //                        //|| pedido.pedido_fecha.ToString().Contains(texto))                      
            //                    select pedido.pedido_codigo).ToList().Count;

            //ObjectParameter Output = new ObjectParameter("total", typeof(Int32));

            //var resultQuery = context.GetPedidosPaginados(campo, texto, pagina, Output).ToList();

            //var total = Output.Value;

            //var result2 = resultQuery.Select(x => new
            //{
            //    pedidoCodigo = x.pedido_codigo,
            //    pedidoFecha = x.pedido_fecha.ToShortDateString(),
            //    pedidoUsuario = x.user_nombre + " " + x.user_apellido,
            //    detallesTotal = x.detalles,
            //    facturadosTotal = context.icb_vehiculo.Where(j => j.numpedido_vh == x.pedido_codigo).Count()
            //});
            //var data = new
            //{
            //    result = pedidosTotal,
            //    result2
            //};
            var query = (from pedido in context.pedido_GM
                         join usuario in context.users
                             on pedido.pedido_usuario_id equals usuario.user_id
                         join detalles in context.detallePedido_GM
                             on pedido.pedido_codigo equals detalles.detallePedido_pedidoCodigo
                             into g
                         from p in g.DefaultIfEmpty()
                         select new
                         {
                             pedido.pedido_codigo,
                             pedido.pedido_fecha, //.ToString("yyyy/MM/dd"),   //fechaPEF = pedido.pedido_fecha.Year + "/" + pedido.pedido_fecha.Month + "/" + pedido.pedido_fecha.Day,   // 
                             pedidoUsuario = usuario.user_nombre + " " + usuario.user_apellido,
                             suma = g.Sum(c => c.detallePedido_cantidad).ToString(),
                             facturadosTotal = context.icb_vehiculo.Where(j => j.numpedido_vh == pedido.pedido_codigo).Count()
                         }).DistinctBy(x => new { x.pedido_codigo, x.pedido_fecha, x.pedidoUsuario }).ToList();
            //   }).DistinctBy(x=>new { x.pedido_codigo,x.fechaPEF, x.pedidoUsuario }).ToList();
            var data = query.Select(x => new
            {
                x.pedido_codigo,
                x.pedidoUsuario,
                pedido_fecha =
                    x.pedido_fecha
                        .ToString(
                            "yyyy/MM/dd"), // x.pedido_fecha.Year + "/" + x.pedido_fecha.Month + "/" + x.pedido_fecha.Day,   //   pedido_fecha=x.pedido_fecha.ToShortDateString(),
                x.suma,
                facturadosTotal = context.icb_vehiculo.Where(j => j.numpedido_vh == x.pedido_codigo).Count()
            }).OrderByDescending(pp => pp.pedido_fecha); // .OrderByDescending(x => x.cantidad).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PedidoEnFirme(int? menu)
        {
            //var busquedas = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 79 && x.menu_busqueda_id_pestana == 1).ToList();
            //ViewBag.paramBusqueda = busquedas;

            //var busquedasDetalles = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 79 && x.menu_busqueda_id_pestana == 2).ToList();
            //ViewBag.paramBusquedaDetalles = busquedasDetalles;
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult VerDetallePedidoEnFirme(long id /*, string campo, string texto, int pagina*/)
        {
            //var detallesTotal = (from detalle in context.detallePedido_GM
            //                     join color in context.color_vehiculo
            //                     on detalle.detallePedido_colorCodigo equals color.colvh_id
            //                     where detalle.detallePedido_pedidoCodigo == id
            //                     //join modelo in context.modelo_vehiculo
            //                     //on detalle.detallePedido_modeloCodigo equals modelo.modvh_id
            //                     select detalle.detallePedido_kmat_zvsk).ToList().Count;

            //var pedido = (from pedidoAux in context.pedido_GM
            //              where pedidoAux.pedido_codigo == id
            //              select new
            //              {
            //                  pedidoAux.pedido_fecha,
            //                  pedidoAux.pedido_codigo
            //              }).First();

            ////var detalles = (from detalle in context.detallePedido_GM
            ////               join color in context.color_vehiculo
            ////               on detalle.detallePedido_colorCodigo equals color.colvh_id
            ////               where detalle.detallePedido_pedidoCodigo == id
            ////               //join modelo in context.modelo_vehiculo
            ////               //on detalle.detallePedido_modeloCodigo equals modelo.modvh_id
            ////               orderby detalle.detallePedido_id
            ////               select new {
            ////                   kmat = detalle.detallePedido_kmat_zvsk,
            ////                   descripcion = detalle.detallePedido_descripcion,
            ////                   color = color.colvh_nombre,
            ////                   cantidad = detalle.detallePedido_cantidad,
            ////                   facturadosCantidad= context.icb_vehiculo.Where(x=>x.numfactura_vh==id && x.kmat_zvsk==detalle.detallePedido_kmat_zvsk && x.colvh_id==detalle.detallePedido_colorCodigo).Count()
            ////               }).Skip((pagina * 30) - 30).Take(30).ToList();

            //ObjectParameter Output = new ObjectParameter("total", typeof(Int32));

            //var resultQuery = context.GetDetallesPedidoPaginados(id, campo, texto, pagina, Output).ToList();

            //var total = Output.Value;

            //var detalles = resultQuery.Select(x => new
            //{
            //    kmat = x.detallePedido_kmat_zvsk,
            //    descripcion = x.detallePedido_descripcion,
            //    color = x.colvh_nombre,
            //    cantidad = x.detallePedido_cantidad,
            //    facturadosCantidad = x.facturados
            //});

            //var detallesPedidoActual = context.detallePedido_GM.Where(x => x.detallePedido_pedidoCodigo == id).Sum(x => x.detallePedido_cantidad);

            //var data = new
            //{
            //    numeroPedido = pedido.pedido_codigo,
            //    fecha = pedido.pedido_fecha.ToShortDateString(),
            //    totalDatos = detallesTotal,
            //    detalles,
            //    detallesPedidoActual
            //};
            var query = (from detalles in context.detallePedido_GM
                         join color in context.color_vehiculo
                             on detalles.detallePedido_colorCodigo equals color.colvh_id
                         join modelo in context.modelo_vehiculo
                             on detalles.detallePedido_modeloCodigo equals modelo.modvh_codigo
                             into g
                         where detalles.detallePedido_pedidoCodigo == id
                         select new
                         {
                             detalles.detallePedido_kmat_zvsk,
                             detalles.detallePedido_descripcion,
                             color.colvh_nombre,
                             detalles.detallePedido_cantidad,
                             cantidad = context.icb_vehiculo
                                 .Where(c => c.numpedido_vh == id && c.kmat_zvsk == detalles.detallePedido_kmat_zvsk).Count()
                         }).ToList();

            var pedido = (from pedidoAux in context.pedido_GM
                          where pedidoAux.pedido_codigo == id
                          select new
                          {
                              pedidoAux.pedido_fecha,
                              pedidoAux.pedido_codigo
                          }).First();

            int totalDatos = context.detallePedido_GM.Where(x => x.detallePedido_pedidoCodigo == id)
                .Select(x => x.detallePedido_cantidad).Sum();

            var datosPedido = new
            {
                numeroPedido = pedido.pedido_codigo,
                fecha = pedido.pedido_fecha.ToShortDateString(),
                totalDatos
            };

            var data = new
            {
                query,
                datosPedido
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult VerPedidoEnFirme()
        {
            var data = (from pedidoAux in context.pedido_GM
                        select new
                        {
                            fecha = pedidoAux.pedido_fecha.ToString("yyyy/MM/dd"),
                            codigo = pedidoAux.pedido_codigo
                        }).OrderBy(pef => pef.fecha);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarFacturacionesPaginados( /*string texto, int pagina*/)
        {
            //var facturasTotal = (from factura in context.icb_arch_facturacion
            //                     join facturaLog in context.icb_arch_facturacion_log
            //                     on factura.arch_fac_id equals facturaLog.id_arch_facturacion
            //                        //join usuario in db.users
            //                        //on pedido.pedido_usuario_id equals usuario.user_id
            //                        //where (pedido.pedido_codigo.ToString().Contains(texto)                                   
            //                        //|| pedido.pedido_fecha.ToString().Contains(texto))                      
            //                    select factura.arch_fac_id).ToList().Count;

            //var resultAux = (from factura in context.icb_arch_facturacion
            //                 join facturaLog in context.icb_arch_facturacion_log
            //                 on factura.arch_fac_id equals facturaLog.id_arch_facturacion
            //                 //join usuario in db.users
            //                 //on pedido.pedido_usuario_id equals usuario.user_id
            //                 //where (pedido.pedido_codigo.ToString().Contains(texto)
            //                 //|| pedido.pedido_fecha.ToString().Contains(texto))
            //                 orderby factura.arch_fac_id descending
            //                 select new
            //                 {
            //                     facturaId = factura.arch_fac_id,
            //                     facturaArchivo = factura.arch_fac_nombre,
            //                     facturaFecha = factura.arch_fac_fecha.Value.Day+"/"+ factura.arch_fac_fecha.Value.Month+"/"+factura.arch_fac_fecha.Value.Year,
            //                     facturaItems = factura.items,
            //                     facturaFallas = facturaLog.fact_log_itemserror,
            //                     facturaCorrecto = facturaLog.fact_log_itemscorrecto
            //                 }).Skip((pagina * 30) - 30).Take(30).ToList();

            //var data = new
            //{
            //    result = facturasTotal,
            //    resultAux
            //};


            IOrderedEnumerable<datosfecha> data = (from factura in context.icb_arch_facturacion
                                                   join usuario in context.users
                                                       on factura.userid_creacion equals usuario.user_id
                                                   join facturaLog in context.icb_arch_facturacion_log
                                                       on factura.arch_fac_id equals facturaLog.id_arch_facturacion
                                                   orderby factura.arch_fac_id descending
                                                   select new datosfecha
                                                   {
                                                       facturaId = factura.arch_fac_id,
                                                       facturaArchivo = factura.arch_fac_nombre,
                                                       facturaFecha =
                                                           factura
                                                               .arch_fac_fecha, //.ToString("yyyy/MM/dd"), // factura.arch_fac_fecha.Year + "/" + factura.arch_fac_fecha.Month + "/" + factura.arch_fac_fecha.Day,
                                                       facturaFecha2 = "", //  
                                                       facturaItems = factura.items,
                                                       facturaFallas = facturaLog.fact_log_itemserror,
                                                       facturaCorrecto = facturaLog.fact_log_itemscorrecto,
                                                       usuario = usuario.user_nombre + " " + usuario.user_apellido
                                                   }).ToList().OrderBy(p => p.facturaFecha);

            data.ForEach(
                item => item.facturaFecha2 = item.facturaFecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")));
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDetallesFacturacionPaginados(int id /*, string campo, string texto, int pagina*/)
        {
            //ObjectParameter Output = new ObjectParameter("total", typeof(Int32));
            //var resultQuery = context.GetDetallesFacturaPaginados(id, campo, texto, pagina, Output).ToList();
            //var total = Output.Value;
            //var result2 = resultQuery.Select(x => new
            //{
            //    motor = x.nummot_vh,
            //    modelo = x.modvh_nombre,
            //    color = x.colvh_nombre,
            //    serie = x.vin,
            //    placa = x.plac_vh,
            //    fechaCompra = x.fecfact_fabrica != null ? x.fecfact_fabrica.Value.ToString("dd/MM/yyyy") : null,
            //    anio = x.anio_vh,
            //    planMayor = x.plan_mayor,
            //    id = x.icbvh_id,
            //    diasEnInventario = x.diasInventario
            //});
            var queryCargados = (from vehiculo in context.icb_vehiculo
                                 join archivo in context.icb_arch_facturacion
                                     on vehiculo.arch_fact_id equals archivo.arch_fac_id
                                 join modelo in context.modelo_vehiculo
                                     on vehiculo.modvh_id equals modelo.modvh_codigo
                                 join color in context.color_vehiculo
                                     on vehiculo.colvh_id equals color.colvh_id
                                 join encabezado in context.encab_documento
                                     on vehiculo.plan_mayor equals encabezado.documento into ps
                                 from encabezado in ps.DefaultIfEmpty()
                                 where vehiculo.arch_fact_id == id && archivo.arch_fac_fecha.Year == encabezado.fec_creacion.Year &&
                                       archivo.arch_fac_fecha.Month == encabezado.fec_creacion.Month
                                       && archivo.arch_fac_fecha.Day == encabezado.fec_creacion.Day
                                 select new
                                 {
                                     vehiculo.nummot_vh,
                                     modelo.modvh_nombre,
                                     color.colvh_nombre,
                                     vehiculo.vin,
                                     vehiculo.plac_vh,
                                     vehiculo.fecfact_fabrica,
                                     vehiculo.anio_vh,
                                     vehiculo.plan_mayor,
                                     vehiculo.icbvh_id,
                                     vehiculo.nummanf_vh,
                                     vehiculo.fecentman_vh,
                                     vehiculo.fecman_vh,
                                     vehiculo.diaslibres_vh,
                                     encabezado.numero,
                                     diasInventario = DbFunctions.DiffDays(vehiculo.fecfact_fabrica, DateTime.Now)
                                 }).ToList();

            var result2 = queryCargados.Select(x => new
            {
                x.nummot_vh,
                x.modvh_nombre,
                x.colvh_nombre,
                x.vin,
                x.plac_vh,
                fecfact_fabrica = x.fecfact_fabrica != null ? x.fecfact_fabrica.Value.ToString("dd/MM/yyyy") : null,
                x.anio_vh,
                x.plan_mayor,
                x.icbvh_id,
                x.diasInventario,
                x.numero,
                manif = x.nummanf_vh != null ? x.nummanf_vh : "",
                fecmanif1 = x.fecentman_vh != null ? x.fecentman_vh.Value.ToString("dd/MM/yyyy") : null,
                fecmanif2 = x.fecman_vh != null ? x.fecman_vh.Value.ToString("dd/MM/yyyy") : null,
                dias = x.diaslibres_vh != null ? x.diaslibres_vh : 0
            });

            string logs = context.icb_arch_facturacion_log.FirstOrDefault(x => x.id_arch_facturacion == id).fact_log_log;
            List<ListaLogs> listaLogsExcel = new List<ListaLogs>();
            string[] substring = logs.Split('|');
            try
            {
                foreach (string item in substring)
                {
                    string vinAuxiliar = item.Substring(0, item.IndexOf('*'));
                    icb_vehiculo asd = context.icb_vehiculo.FirstOrDefault(x => x.vin == vinAuxiliar);
                    if (asd != null)
                    {
                        var consultaModelo = (from vehiculo in context.icb_vehiculo
                                              join modelo in context.modelo_vehiculo
                                                  on vehiculo.modvh_id equals modelo.modvh_codigo
                                              where vehiculo.vin == vinAuxiliar
                                              select new { modelo.modvh_nombre, vehiculo.plan_mayor }).First();

                        if (consultaModelo != null)
                        {
                            listaLogsExcel.Add(new ListaLogs
                            {
                                pedido = "",
                                Vin = item.Substring(0, item.IndexOf('*')),
                                Excepcion =
                                    item.Substring(item.IndexOf('*') + 1, item.Length - (item.IndexOf('*') + 1)),
                                Modelo = consultaModelo.modvh_nombre,
                                PlanMayor = consultaModelo.plan_mayor
                            });
                        }
                    }
                    else
                    {
                        listaLogsExcel.Add(new ListaLogs
                        {
                            pedido = item.Substring(0, item.IndexOf('*')),
                            Vin = "",
                            Excepcion = item.Substring(item.IndexOf('*') + 1, item.Length - (item.IndexOf('*') + 1)),
                            Modelo = "",
                            PlanMayor = ""
                        });
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // Excepcion cuando no hay logs para mostrar, por tanto no hay substrings
            }

            var data = new
            {
                detallesFactura = result2,
                listaLogsExcel
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

        public class datosfecha
        {
            public int facturaId { get; set; }
            public string facturaArchivo { get; set; }
            public DateTime facturaFecha { get; set; }
            public string facturaFecha2 { get; set; }
            public int? facturaItems { get; set; }
            public int? facturaFallas { get; set; }
            public int? facturaCorrecto { get; set; }
            public string usuario { get; set; }
        }

        public class cuentasNecesarias
        {
            public int id { get; set; }
            public int id_nombre_parametro { get; set; }
            public int cuenta { get; set; }
            public int centro { get; set; }
            public int? idperfil { get; set; }
            public int? codigo { get; set; }
            public string descripcion_parametro { get; set; }
            public string descripcion { get; set; }
            public string cntpuc_numero { get; set; }
        }

        public class ListaCuentas
        {
            public List<cuentasNecesarias> ListaCuentasNecesarias { get; set; }
        }
    }
}