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
    public class facturacionNuevosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new  CultureInfo("is-IS");
        //,miCultura

        public void ListasDesplegables()
        {
            ViewBag.tp_doc_registros = context.tp_doc_registros.Where(x => x.sw == 3 && x.tipo == 2).ToList();
            ViewBag.fpago_id = new SelectList(context.fpago_tercero.OrderBy(x => x.fpago_nombre), "fpago_id",
                "fpago_nombre");
            List<users> asesores = context.users.Where(x => x.rol_id == 4 || x.rol_id == 1).ToList();
            List<SelectListItem> listaAsesores = new List<SelectListItem>();
            foreach (users asesor in asesores)
            {
                listaAsesores.Add(new SelectListItem
                { Value = asesor.user_id.ToString(), Text = asesor.user_nombre + " " + asesor.user_apellido });
            }

            ViewBag.vendedor = listaAsesores;
        }

        public ActionResult Create(int? menu)
        {
            ListasDesplegables();
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Create(FacturaVehiculoModel modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                long numeroConsecutivo = 0;
                string alertaVencimientoPeritaje = "";

                ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
                icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
                tp_doc_registros buscarTipoDocRegistro = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == modelo.tipo);
                numeroConsecutivoAux =
                    gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro, modelo.tipo, modelo.bodega);

                grupoconsecutivos grupoConsecutivo = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == modelo.tipo && x.bodega_id == modelo.bodega);
                string alertasFechaOConsecutivo = "";

                #region Consecutivo

                if (numeroConsecutivoAux != null)
                {
                    numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                    // PRIMERO SE VALIDA QUE LA RESOLUCION ESTE EN LAS FECHAS CORRECTAS Y LOS CONSECUTIVOS DENTRO DE LA RESOLUCION APLIQUEN
                    int grupo = grupoConsecutivo != null ? grupoConsecutivo.grupo : 0;
                    resolucionfactura buscarResolucion =
                        context.resolucionfactura.FirstOrDefault(x => x.grupo == grupo && x.tipodoc == modelo.tipo);
                    if (buscarResolucion != null)
                    {
                        int avisoPorConsecutivo = buscarResolucion.consecaviso;
                        int avisoPorFecha = buscarResolucion.diasaviso;
                        TimeSpan diasInicio = buscarResolucion.fechaini.Subtract(DateTime.Now);
                        TimeSpan diasFin = buscarResolucion.fechafin.Subtract(DateTime.Now);
                        if (buscarResolucion.consecfin - numeroConsecutivo <= avisoPorConsecutivo)
                        {
                            alertasFechaOConsecutivo = "Quedan " + (buscarResolucion.consecfin - numeroConsecutivo) +
                                                       "consecutivos para usar";
                        }

                        if (buscarResolucion.consecini >= numeroConsecutivo &&
                            buscarResolucion.consecfin <= numeroConsecutivo || diasInicio.Days > 0 || diasFin.Days < 0)
                        {
                            TempData["mensaje_error"] =
                                "El numero consecutivo asignado no esta vigente en la resolucion, consecutivo actual : "
                                + numeroConsecutivo + " consecutivos de la resolucion : " + buscarResolucion.consecini +
                                " hasta " + buscarResolucion.consecfin
                                + " o la fecha de la resolucion ya vencio, fechas validas estan dentro de " +
                                buscarResolucion.fechaini.ToShortDateString() + " - " +
                                buscarResolucion.fechafin.ToShortDateString();
                            ListasDesplegables();
                            ViewBag.documentoSeleccionado = modelo.tipo;
                            ViewBag.bodegaSeleccionado = modelo.bodega;
                            ViewBag.perfilSeleccionado = modelo.perfilContable;
                            BuscarFavoritos(menu);
                            return View(modelo);
                        }

                        if (diasFin.Days < avisoPorFecha)
                        {
                            alertasFechaOConsecutivo =
                                "la fecha de la resolucion esta proxima a vencer, fechas validas estan dentro de " +
                                buscarResolucion.fechaini.ToShortDateString() + " - "
                                + buscarResolucion.fechafin.ToShortDateString();
                        }
                    }
                    else
                    {
                        TempData["mensaje_error"] = "No existe una resolucion asociada a este tipo de documento";
                        ListasDesplegables();
                        ViewBag.documentoSeleccionado = modelo.tipo;
                        ViewBag.bodegaSeleccionado = modelo.bodega;
                        ViewBag.perfilSeleccionado = modelo.perfilContable;
                        BuscarFavoritos(menu);
                        return View(modelo);
                    }
                }
                else
                {
                    TempData["mensaje_error"] = "No existe un numero consecutivo asignado para este tipo de documento";
                    return RedirectToAction("Create", "facturacionNuevos", new { menu });
                }

                #endregion

                bool guardarReferenciasInven = false;
                int bodegaActual = Convert.ToInt32(Session["user_bodega"]);

                #region Busqueda del pedido y del tercero 

                var buscarPedido = (from pedido in context.vpedido
                                    join inventarioHoy in context.vw_inventario_hoy
                                        on pedido.planmayor equals inventarioHoy.ref_codigo
                                    join retoma in context.vpedretoma
                                        on pedido.id equals retoma.pedido_id into ret
                                    from retoma in ret.DefaultIfEmpty()
                                    where pedido.numero == modelo.pedido && pedido.bodega == bodegaActual && inventarioHoy.bodega==bodegaActual
                                    select new
                                    {
                                        pedido.id,
                                        pedido.numero,
                                        pedido.planmayor,
                                        pedido.vrtotal,
                                        pedido.facturado,
                                        pedido.numfactura,
                                        pedido.flota,
                                        retoma.placa,
                                        inventarioHoy.stock,
                                        inventarioHoy.bodega
                                    }).FirstOrDefault();

                bool actividadEconomicaValido = false;
                decimal porcentajeReteICA = 0;
                decimal porcentajeAutorretencion = 0;
                var buscarNitCliente = (from tercero in context.icb_terceros
                                        join regimen in context.tpregimen_tercero
                                            on tercero.tpregimen_id equals regimen.tpregimen_id into ps2
                                        from regimen in ps2.DefaultIfEmpty()
                                        join acteco in context.acteco_tercero
                                            on tercero.id_acteco equals acteco.acteco_id into ps3
                                        from acteco in ps3.DefaultIfEmpty()
                                        join bodega in context.terceros_bod_ica
                                            on acteco.acteco_id equals bodega.idcodica into ps
                                        from bodega in ps.DefaultIfEmpty()
                                        where tercero.doc_tercero == modelo.nit && tercero.tercero_estado
                                        select new
                                        {
                                            actividadEconomica_id = tercero.id_acteco != null ? tercero.id_acteco : 0,
                                            acteco_nombre = tercero.id_acteco != null ? acteco.acteco_nombre : "",
                                            tarifaPorBodega = bodega != null ? bodega.porcentaje : 0,
                                            actecoTarifa = tercero.id_acteco != null ? acteco.tarifa : 0,
                                            actecoAutorretencion = tercero.id_acteco != null ? acteco.autorretencion : 0,
                                            regimen_id = tercero.tpregimen_id != null ? regimen.tpregimen_id : 0
                                        }).FirstOrDefault();

                #endregion


                if (buscarNitCliente != null)
                {
                    porcentajeAutorretencion = buscarNitCliente.actecoAutorretencion;
                    if (buscarNitCliente.tarifaPorBodega != 0)
                    {
                        actividadEconomicaValido = true;
                        porcentajeReteICA = buscarNitCliente.tarifaPorBodega;
                    }
                    else if (buscarNitCliente.actecoTarifa != 0)
                    {
                        actividadEconomicaValido = true;
                        porcentajeReteICA = buscarNitCliente.actecoTarifa;
                    }
                }

                int regimen_proveedor = buscarNitCliente != null ? buscarNitCliente.regimen_id : 0;
                int? sw_encontrado = buscarTipoDocRegistro != null ? buscarTipoDocRegistro.sw : 0;
                perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                    x.bodega == modelo.bodega && x.sw == sw_encontrado && x.tipo_regimenid == regimen_proveedor);

                // Realizamos la validaciones para los movimientos contables, si esta es exitosa proseguimos, si no detenemos la ejecucion del codigo

                #region Movimientos Contables

                var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                                  join nombreParametro in context.paramcontablenombres
                                                      on perfil.id_nombre_parametro equals nombreParametro.id
                                                  join cuenta in context.cuenta_puc
                                                      on perfil.cuenta equals cuenta.cntpuc_id
                                                  where perfil.id_perfil == modelo.perfilContable
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

                DateTime fechaHoy = DateTime.Now;
                decimal? buscarvw_promedio = (from promedio in context.vw_promedio
                                              where promedio.codigo == modelo.planMayor
                                                    && promedio.ano == fechaHoy.Year && promedio.mes == fechaHoy.Month
                                              select promedio.Promedio).FirstOrDefault();

                foreach (var parametro in parametrosCuentasVerificar)
                {
                    cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
                    decimal valorCredito = 0;
                    decimal valorDebito = 0;
                    string CreditoDebito = "";
                    if (buscarCuenta != null)
                    {
                        if (parametro.id_nombre_parametro == 2)
                        {
                            valorCredito = Convert.ToDecimal(modelo.iva,miCultura) * Convert.ToDecimal(modelo.precio,miCultura) / 100;
                            CreditoDebito = "Credito";
                            //valorDebito = ((modelo.iva * modelo.precio) / 100);
                        }

                        if (parametro.id_nombre_parametro == 3)
                        {
                            if (buscarPerfilTributario != null)
                            {
                                if (buscarTipoDocRegistro != null)
                                {
                                    if (buscarPerfilTributario.retfuente == "A")
                                    {
                                        if (buscarTipoDocRegistro.baseretencion <= Convert.ToDecimal(modelo.precio,miCultura))
                                        {
                                            decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                            valorDebito = Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetencion / 100;
                                            CreditoDebito = "Debito";
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
                                            Convert.ToDecimal(modelo.precio,miCultura) * modelo.iva / 100)
                                        {
                                            decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                            valorDebito =
                                                Convert.ToDecimal(modelo.precio,miCultura) * modelo.iva / 100 * porcentajeRetIva /
                                                100;
                                            CreditoDebito = "Debito";
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
                                            if (buscarTipoDocRegistro.baseica <= Convert.ToDecimal(modelo.precio,miCultura))
                                            {
                                                decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                                //valorCredito = (modelo.precio * porcentajeRetIca) / 100;
                                                valorDebito =
                                                    Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetIca / 1000;
                                                CreditoDebito = "Debito";
                                            }
                                        }
                                        else
                                        {
                                            valorCredito = Convert.ToDecimal(modelo.precio,miCultura) * porcentajeReteICA / 1000;
                                            valorDebito = Convert.ToDecimal(modelo.precio,miCultura) * porcentajeReteICA / 1000;
                                        }
                                    }
                                }
                            }
                        }

                        if (parametro.id_nombre_parametro == 8)
                        {
                            valorCredito = modelo.impuestoConsumo * Convert.ToDecimal(modelo.precio,miCultura) / 100;
                            CreditoDebito = "Credito";
                        }

                        if (parametro.id_nombre_parametro == 9)
                        {
                            valorCredito = buscarvw_promedio ?? 0;
                            CreditoDebito = "Credito";
                        }

                        if (parametro.id_nombre_parametro == 10)
                        {
                            decimal restarIcaIvaRetencion = 0;
                            if (buscarPerfilTributario != null)
                            {
                                if (buscarTipoDocRegistro != null)
                                {
                                    if (buscarPerfilTributario.retfuente == "A")
                                    {
                                        if (buscarTipoDocRegistro.baseretencion <= Convert.ToDecimal(modelo.precio,miCultura))
                                        {
                                            decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                            decimal sdf = Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetencion / 100;
                                            restarIcaIvaRetencion +=
                                                Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetencion / 100;
                                        }
                                    }

                                    if (buscarPerfilTributario.retiva == "A")
                                    {
                                        if (buscarTipoDocRegistro.baseiva <= Convert.ToDecimal(modelo.precio,miCultura))
                                        {
                                            decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                            decimal tt = Convert.ToDecimal(modelo.precio,miCultura) * modelo.iva / 100 *
                                                     porcentajeRetIva / 100;
                                            restarIcaIvaRetencion +=
                                                Convert.ToDecimal(modelo.precio,miCultura) * modelo.iva / 100 * porcentajeRetIva /
                                                100;
                                        }
                                    }

                                    if (buscarPerfilTributario.retica == "A")
                                    {
                                        if (!actividadEconomicaValido)
                                        {
                                            if (buscarTipoDocRegistro.baseica <= Convert.ToDecimal(modelo.precio,miCultura))
                                            {
                                                decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                                decimal hh = Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetIca / 1000;
                                                restarIcaIvaRetencion +=
                                                    Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetIca / 1000;
                                            }
                                        }
                                        else
                                        {
                                            restarIcaIvaRetencion +=
                                                Convert.ToDecimal(modelo.precio,miCultura) * porcentajeReteICA / 1000;
                                        }
                                    }
                                }
                            }

                            valorDebito = Convert.ToDecimal(modelo.precio,miCultura) +
                                          modelo.iva * Convert.ToDecimal(modelo.precio,miCultura) / 100 +
                                          modelo.impuestoConsumo * Convert.ToDecimal(modelo.precio,miCultura) / 100 -
                                          restarIcaIvaRetencion;
                            CreditoDebito = "Debito";
                        }

                        if (parametro.id_nombre_parametro == 11)
                        {
                            valorCredito = Convert.ToDecimal(modelo.precio,miCultura);
                            CreditoDebito = "Credito";
                        }

                        if (parametro.id_nombre_parametro == 12)
                        {
                            valorDebito = buscarvw_promedio ?? 0;
                            CreditoDebito = "Debito";
                        }

                        if (parametro.id_nombre_parametro == 17)
                        {
                            if (buscarPerfilTributario != null)
                            {
                                if (buscarPerfilTributario.autorretencion == "A")
                                {
                                    valorDebito = Convert.ToDecimal(modelo.precio,miCultura) * porcentajeAutorretencion / 100;
                                    CreditoDebito = "Debito";
                                }
                            }
                        }

                        if (parametro.id_nombre_parametro == 18)
                        {
                            if (buscarPerfilTributario != null)
                            {
                                if (buscarPerfilTributario.autorretencion == "A")
                                {
                                    valorCredito = Convert.ToDecimal(modelo.precio,miCultura) * porcentajeAutorretencion / 100;
                                    CreditoDebito = "Credito";
                                }
                            }
                        }

                        //valorDebito = Math.Round(valorDebito);
                        //valorCredito = Math.Round(valorCredito);
                        if (CreditoDebito.ToUpper().Contains("DEBITO"))
                        {
                            calcularDebito += valorDebito;
                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                            {
                                NumeroCuenta = parametro.cntpuc_numero,
                                DescripcionParametro = parametro.descripcion_parametro,
                                ValorDebito = valorDebito,
                                ValorCredito = 0
                            });
                        }

                        if (CreditoDebito.ToUpper().Contains("CREDITO"))
                        {
                            calcularCredito += valorCredito;
                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                            {
                                NumeroCuenta = parametro.cntpuc_numero,
                                DescripcionParametro = parametro.descripcion_parametro,
                                ValorDebito = 0,
                                ValorCredito = valorCredito
                            });
                        }
                    }
                }

                if (calcularCredito != calcularDebito)
                {
                    TempData["documento_descuadrado"] =
                        "El documento no tiene los movimientos calculados correctamente";
                    ViewBag.documentoSeleccionado = modelo.tipo;
                    ViewBag.bodegaSeleccionado = modelo.bodega;
                    ViewBag.perfilSeleccionado = modelo.perfilContable;

                    ViewBag.documentoDescuadrado = listaDescuadrados;
                    ViewBag.calculoDebito = calcularDebito;
                    ViewBag.calculoCredito = calcularCredito;
                    ListasDesplegables();
                    BuscarFavoritos(menu);
                    return View(modelo);
                }

                #endregion

                // Fin de la validacion para el calculo del debito y credito del movimiento contable

                #region Referencias inven

                referencias_inven buscarReferenciasInven = context.referencias_inven.FirstOrDefault(x =>
                    x.codigo == modelo.planMayor && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month);
                if (buscarReferenciasInven != null)
                {
                    buscarReferenciasInven.can_sal = 1;
                    buscarReferenciasInven.cos_sal = buscarvw_promedio ?? 0;
                    buscarReferenciasInven.can_vta = 1;
                    buscarReferenciasInven.cos_vta = buscarvw_promedio ?? 0;
                    context.Entry(buscarReferenciasInven).State = EntityState.Modified;

                    // Se busca el vehiculo  de la tabla icb_vehiculo, para actualizarle el campo propietario
                    icb_vehiculo buscarVehiculo = context.icb_vehiculo.FirstOrDefault(x => x.plan_mayor == modelo.planMayor);
                    if (buscarVehiculo != null)
                    {
                        buscarVehiculo.propietario = modelo.id_tercero;
                        buscarVehiculo.fecha_venta = DateTime.Now;
                        context.Entry(buscarVehiculo).State = EntityState.Modified;
                    }

                    guardarReferenciasInven = context.SaveChanges() > 0;
                }
                else
                {
                    TempData["mensaje_error"] = "El vehiculo buscado no se encuentra disponible en el inventario";
                    ViewBag.documentoSeleccionado = modelo.tipo;
                    ViewBag.bodegaSeleccionado = modelo.bodega;
                    ViewBag.perfilSeleccionado = modelo.perfilContable;
                    ListasDesplegables();
                    BuscarFavoritos(menu);
                    return View(modelo);
                }

                #endregion

                fpago_tercero buscarDiasVencimiento = context.fpago_tercero.FirstOrDefault(x => x.fpago_id == modelo.fpago_id);
                icb_terceros buscarCliente = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == modelo.nit);
                int nitCliente = buscarCliente != null ? buscarCliente.tercero_id : 0;

                #region Encabezado documento

                encab_documento crearEncabezado = new encab_documento
                {
                    tipo = modelo.tipo,
                    pedido = modelo.pedido,
                    bodega = modelo.bodega,
                    fpago_id = modelo.fpago_id,
                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    nit = nitCliente,
                    costo = buscarvw_promedio ?? 0,
                    numero = numeroConsecutivo,
                    documento = modelo.planMayor,
                    fecha = DateTime.Now,
                    fec_creacion = DateTime.Now,
                    vendedor = modelo.vendedor,
                    estado = true,
                    notas = modelo.nota
                };
                if (buscarDiasVencimiento != null)
                {
                    crearEncabezado.vencimiento = DateTime.Now.AddDays(buscarDiasVencimiento.dvencimiento != null
                        ? buscarDiasVencimiento.dvencimiento.Value
                        : 0);
                }

                if (modelo.pedido != null)
                {
                    vpedido buscarVPedido = context.vpedido.FirstOrDefault(x => x.id == modelo.pedido);
                    if (buscarVPedido != null)
                    {
                        crearEncabezado.id_pedido_vehiculo = buscarVPedido.id;
                    }
                    else
                    {
                        crearEncabezado.id_pedido_vehiculo = null;
                    }
                }

                crearEncabezado.valor_total = Convert.ToDecimal(modelo.valorTotal,miCultura);
                crearEncabezado.iva = Convert.ToDecimal(modelo.iva) * Convert.ToDecimal(modelo.precio,miCultura) / 100;
                crearEncabezado.valor_mercancia = Convert.ToDecimal(modelo.precio,miCultura);
                crearEncabezado.impoconsumo = modelo.impuestoConsumo;

                if (buscarPerfilTributario != null)
                {
                    if (buscarTipoDocRegistro != null)
                    {
                        if (buscarPerfilTributario.retfuente == "A")
                        {
                            if (buscarTipoDocRegistro.baseretencion <= Convert.ToDecimal(modelo.precio,miCultura))
                            {
                                decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                crearEncabezado.retencion =
                                    Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetencion / 100;
                            }
                        }

                        if (buscarPerfilTributario.retiva == "A")
                        {
                            if (buscarTipoDocRegistro.baseiva <= Convert.ToDecimal(modelo.precio,miCultura))
                            {
                                decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                crearEncabezado.retencion_iva =
                                    Convert.ToDecimal(modelo.precio,miCultura) * modelo.iva / 100 * porcentajeRetIva / 100;
                            }
                        }

                        if (buscarPerfilTributario.retica == "A")
                        {
                            if (!actividadEconomicaValido)
                            {
                                if (buscarTipoDocRegistro.baseica <= Convert.ToDecimal(modelo.precio,miCultura))
                                {
                                    decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                    crearEncabezado.retencion_ica +=
                                        Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetIca / 1000;
                                }
                            }
                            else
                            {
                                crearEncabezado.retencion_ica =
                                    Convert.ToDecimal(modelo.precio,miCultura) * porcentajeReteICA / 1000;
                            }
                        }
                    }
                }

                context.encab_documento.Add(crearEncabezado);
                int save = context.SaveChanges();
                encab_documento buscarUltimoEncabezado =
                    context.encab_documento.OrderByDescending(x => x.idencabezado).FirstOrDefault();

                #endregion

                #region Lineas Documento

                lineas_documento crearLineas = new lineas_documento
                {
                    codigo = modelo.planMayor,
                    fec_creacion = DateTime.Now,
                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    nit = nitCliente,
                    cantidad = 1,
                    bodega = modelo.bodega,
                    seq = 1,
                    porcentaje_iva = (float)modelo.iva,
                    valor_unitario = Convert.ToDecimal(modelo.precio,miCultura),
                    impoconsumo = (float)modelo.impuestoConsumo,
                    estado = true,
                    id_tarifa_cliente = 1,
                    fec = DateTime.Now,
                    costo_unitario = buscarvw_promedio ?? 0,
                    id_encabezado = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0
                };
                context.lineas_documento.Add(crearLineas);

                #endregion

                #region Movimientos contables

                List<perfil_cuentas_documento> parametrosCuentas = context.perfil_cuentas_documento
                    .Where(x => x.id_perfil == modelo.perfilContable).ToList();
                centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
                icb_terceros terceroValorCero = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
                int idTerceroCero = terceroValorCero != null ? Convert.ToInt32(terceroValorCero.tercero_id) : 0;
                int secuencia = 1;
                foreach (perfil_cuentas_documento parametro in parametrosCuentas)
                {
                    decimal valorCredito = 0;
                    decimal valorDebito = 0;
                    string CreditoDebito = "";
                    cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
                    if (buscarCuenta != null)
                    {
                        if (parametro.id_nombre_parametro == 2)
                        {
                            valorCredito = modelo.iva * Convert.ToDecimal(modelo.precio,miCultura) / 100;
                            CreditoDebito = "Credito";
                        }

                        if (parametro.id_nombre_parametro == 3)
                        {
                            if (buscarPerfilTributario != null)
                            {
                                if (buscarTipoDocRegistro != null)
                                {
                                    if (buscarPerfilTributario.retfuente == "A")
                                    {
                                        if (buscarTipoDocRegistro.baseretencion <= Convert.ToDecimal(modelo.precio,miCultura))
                                        {
                                            decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                            valorDebito = Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetencion / 100;
                                            CreditoDebito = "Debito";
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
                                            Convert.ToDecimal(modelo.precio,miCultura) * modelo.iva / 100)
                                        {
                                            decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                            valorDebito =
                                                Convert.ToDecimal(modelo.precio,miCultura) * modelo.iva / 100 * porcentajeRetIva /
                                                100;
                                            CreditoDebito = "Debito";
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
                                            if (buscarTipoDocRegistro.baseica <= Convert.ToDecimal(modelo.precio,miCultura))
                                            {
                                                decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                                valorDebito =
                                                    Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetIca / 1000;
                                                CreditoDebito = "Debito";
                                            }
                                        }
                                        else
                                        {
                                            valorCredito = Convert.ToDecimal(modelo.precio,miCultura) * porcentajeReteICA / 1000;
                                            valorDebito = Convert.ToDecimal(modelo.precio,miCultura) * porcentajeReteICA / 1000;
                                        }
                                    }
                                }
                            }
                        }

                        if (parametro.id_nombre_parametro == 8)
                        {
                            valorCredito = Convert.ToDecimal(modelo.valorImpuestoConsumo,miCultura);
                            CreditoDebito = "Credito";
                        }

                        if (parametro.id_nombre_parametro == 9)
                        {
                            valorCredito = buscarvw_promedio ?? 0;
                            CreditoDebito = "Credito";
                        }

                        if (parametro.id_nombre_parametro == 10)
                        {
                            decimal restarIcaIvaRetencion = 0;
                            if (buscarPerfilTributario != null)
                            {
                                if (buscarTipoDocRegistro != null)
                                {
                                    if (buscarPerfilTributario.retfuente == "A")
                                    {
                                        if (buscarTipoDocRegistro.baseretencion <= Convert.ToDecimal(modelo.precio,miCultura))
                                        {
                                            decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                            restarIcaIvaRetencion +=
                                                Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetencion / 100;
                                        }
                                    }

                                    if (buscarPerfilTributario.retiva == "A")
                                    {
                                        if (buscarTipoDocRegistro.baseiva <= Convert.ToDecimal(modelo.precio,miCultura))
                                        {
                                            decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                            restarIcaIvaRetencion +=
                                                Convert.ToDecimal(modelo.precio,miCultura) * modelo.iva / 100 * porcentajeRetIva /
                                                100;
                                        }
                                    }

                                    if (buscarPerfilTributario.retica == "A")
                                    {
                                        if (!actividadEconomicaValido)
                                        {
                                            if (buscarTipoDocRegistro.baseica <= Convert.ToDecimal(modelo.precio,miCultura))
                                            {
                                                decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                                restarIcaIvaRetencion +=
                                                    Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetIca / 1000;
                                            }
                                        }
                                        else
                                        {
                                            restarIcaIvaRetencion +=
                                                Convert.ToDecimal(modelo.precio,miCultura) * porcentajeReteICA / 1000;
                                        }
                                    }
                                }
                            }

                            valorDebito = Convert.ToDecimal(modelo.precio,miCultura) +
                                          modelo.iva * Convert.ToDecimal(modelo.precio,miCultura) / 100 +
                                          modelo.impuestoConsumo * Convert.ToDecimal(modelo.precio,miCultura) / 100 -
                                          restarIcaIvaRetencion;
                            CreditoDebito = "Debito";
                        }

                        if (parametro.id_nombre_parametro == 11)
                        {
                            valorCredito = Convert.ToDecimal(modelo.precio,miCultura);
                            CreditoDebito = "Credito";
                        }

                        if (parametro.id_nombre_parametro == 12)
                        {
                            valorDebito = buscarvw_promedio ?? 0;
                            CreditoDebito = "Debito";
                        }

                        if (parametro.id_nombre_parametro == 17)
                        {
                            if (buscarPerfilTributario != null)
                            {
                                if (buscarPerfilTributario.autorretencion == "A")
                                {
                                    valorDebito = Convert.ToDecimal(modelo.precio,miCultura) * porcentajeAutorretencion / 100;
                                    CreditoDebito = "Debito";
                                }
                            }
                        }

                        if (parametro.id_nombre_parametro == 18)
                        {
                            if (buscarPerfilTributario != null)
                            {
                                if (buscarPerfilTributario.autorretencion == "A")
                                {
                                    valorCredito = Convert.ToDecimal(modelo.precio,miCultura) * porcentajeAutorretencion / 100;
                                    CreditoDebito = "Credito";
                                }
                            }
                        }

                        if (valorCredito != 0 || valorDebito != 0)
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
                                nit = nitCliente,
                                fec = DateTime.Now,
                                seq = secuencia,
                                debito = valorDebito,
                                credito = valorCredito
                            };

                            if (parametro.id_nombre_parametro == 4)
                            {
                                movNuevo.basecontable = modelo.iva * Convert.ToDecimal(modelo.precio,miCultura) / 100;
                            }

                            if (buscarTipoDocRegistro.aplicaniff)
                            {
                                if (buscarCuenta.concepniff == 1 || buscarCuenta.concepniff == 4)
                                {
                                    if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                    {
                                        movNuevo.debitoniif = valorDebito;
                                    }

                                    if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                    {
                                        movNuevo.creditoniif = valorCredito;
                                    }
                                }
                            }

                            if (buscarCuenta.manejabase)
                            {
                                movNuevo.basecontable = Convert.ToDecimal(modelo.precio,miCultura);
                            }

                            if (buscarCuenta.tercero)
                            {
                                movNuevo.nit = nitCliente;
                            }

                            if (buscarCuenta.documeto)
                            {
                                movNuevo.documento = buscarUltimoEncabezado.numero.ToString();
                            }

                            movNuevo.detalle = "Facturacion vehiculos fact " + buscarUltimoEncabezado.documento;
                            secuencia++;

                            cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                x.centro == parametro.centro && x.cuenta == parametro.cuenta && x.nit == movNuevo.nit);

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
                        }
                    }
                }

                #endregion

                bool guardarCuenta = context.SaveChanges() > 0;
                if (guardarReferenciasInven && guardarCuenta)
                {
                    #region  Actualiza el campo factura, para saber que a un pedido ya se le hizo su respectiva facturacion.

                    vpedido buscarPedidoParaActualizar = context.vpedido.FirstOrDefault(x => x.id == modelo.pedido);
                    if (buscarPedidoParaActualizar != null)
                    {
                        buscarPedidoParaActualizar.facturado = true;
                        buscarPedidoParaActualizar.numfactura = (int)numeroConsecutivo;
                        context.Entry(buscarPedidoParaActualizar).State = EntityState.Modified;
                    }

                    #endregion

                    #region Eventos de vehiculo

                    icb_sysparameter buscarParametroEventoFact = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P4");
                    string eventoFacturacionParametro =
                        buscarParametroEventoFact != null ? buscarParametroEventoFact.syspar_value : "1";
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
                        bodega_id = modelo.bodega,
                        //vin = vehiculo.vin,
                        planmayor = modelo.planMayor
                    });
                    int guardarVehEventos = context.SaveChanges();

                    #endregion

                    #region Seguimiento

                    context.seguimientotercero.Add(new seguimientotercero
                    {
                        idtercero = nitCliente,
                        tipo = 5,
                        nota = "El tercero realizo un pedido con numero " + numeroConsecutivo,
                        fecha = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                    });

                    if (modelo.pedido != null)
                    {
                        vpedido infoPedido = context.vpedido.FirstOrDefault(x => x.id == modelo.pedido);
                        if (infoPedido != null)
                        {
                            int? cotizacionPedido = infoPedido.idcotizacion;
                            if (cotizacionPedido != null)
                            {
                                vcotseguimiento seguimiento = new vcotseguimiento
                                {
                                    cot_id = Convert.ToInt32(cotizacionPedido),
                                    fecha = DateTime.Now,
                                    responsable = Convert.ToInt32(Session["user_usuarioid"]),
                                    Notas = "Facturacion vehiculo con numero " + modelo.numero,
                                    Motivo = null,
                                    fec_creacion = DateTime.Now,
                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                    estado = true,
                                    tipo_seguimiento = 5
                                };
                                context.vcotseguimiento.Add(seguimiento);
                            }
                        }
                    }

                    #endregion

                    // Actualiza los numeros consecutivos por documento

                    #region Guardado final y alcualizacion del consecutivo

                    int grupoId = grupoConsecutivo != null ? grupoConsecutivo.grupo : 0;
                    List<grupoconsecutivos> gruposConsecutivos = context.grupoconsecutivos.Where(x => x.grupo == grupoId).ToList();
                    foreach (grupoconsecutivos grupo in gruposConsecutivos)
                    {
                        icb_doc_consecutivos buscarElemento = context.icb_doc_consecutivos.FirstOrDefault(x =>
                            x.doccons_idtpdoc == modelo.tipo && x.doccons_bodega == grupo.bodega_id);
                        buscarElemento.doccons_siguiente = buscarElemento.doccons_siguiente + 1;
                        context.Entry(buscarElemento).State = EntityState.Modified;
                    }

                    context.SaveChanges();
                    ViewBag.numFacturacionCreada = numeroConsecutivo;
                    modelo.numero = numeroConsecutivo;
                    TempData["mensaje"] = "La facturacion se ha guardado exitosamente. " + alertasFechaOConsecutivo +
                                          ". " + alertaVencimientoPeritaje;

                    #endregion
                }
                else
                {
                    TempData["mensaje_error"] = "Error de conexion con base de datos";
                    ListasDesplegables();
                    BuscarFavoritos(menu);
                    return View(modelo);
                }
            }
            else
            {
                List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();
                TempData["mensaje_error"] = "Error en el modelo, por favor verifique campos obligatorios.";
                ListasDesplegables();
                BuscarFavoritos(menu);
                return View(modelo);
            }

            ListasDesplegables();
            ViewBag.documentoSeleccionado = modelo.tipo;
            ViewBag.bodegaSeleccionado = modelo.bodega;
            ViewBag.perfilSeleccionado = modelo.perfilContable;
            BuscarFavoritos(menu);
            return View(modelo);
        }

        public ActionResult FacturacionNuevosBackoffice(int? menu, string backoffice)
        {
            if (backoffice != null)
            {
                string[] array;
                int tpDoc = 0;
                int bodega = 0;
                int pedidos = 0;

                array = backoffice.Split(',');
                for (int i = 0; i < array.Count(); i++)
                {
                    tpDoc = Convert.ToInt32(array[0]);
                    bodega = Convert.ToInt32(array[1]);
                    pedidos = Convert.ToInt32(array[2]);
                }

                ViewBag.idDocumento = tpDoc;
                var desDoc = (from tpr in context.tp_doc_registros
                              where tpr.tpdoc_id == tpDoc
                              select new
                              {
                                  nombre = "(" + tpr.prefijo + ") " + tpr.tpdoc_nombre
                              }).FirstOrDefault();
                ViewBag.documentoSeleccionado = desDoc.nombre;

                var desBod = (from b in context.bodega_concesionario
                              where b.id == bodega
                              select new
                              {
                                  nombre = b.bodccs_nombre
                              }).FirstOrDefault();
                ViewBag.idBodega = bodega;
                ViewBag.bodegaSeleccionada = desBod.nombre;


                var desPedido = (from pedido in context.vpedido
                                 join tercero in context.icb_terceros
                                     on pedido.nit equals tercero.tercero_id
                                 where pedido.bodega == bodega && pedido.planmayor != null && pedido.facturado == false &&
                                       pedido.numfactura == null && pedido.numero == pedidos
                                 select new
                                 {
                                     tercero.tercero_id,
                                     docTercero = tercero.doc_tercero,
                                     cliente = "(" + pedido.numero + ") " + tercero.razon_social + tercero.prinom_tercero + " " +
                                               tercero.segnom_tercero + " " + tercero.apellido_tercero + " " +
                                               tercero.segapellido_tercero
                                 }).FirstOrDefault();

                int idPedido = context.vpedido.FirstOrDefault(x => x.numero == pedidos).id;
                ViewBag.idPedido = idPedido;
                ViewBag.tercero = desPedido.tercero_id;
                ViewBag.pedidoSeleccionado = desPedido.cliente;
                ViewBag.documentoTercero = desPedido.docTercero;
                int bod_log = Convert.ToInt32(Session["user_bodega"]);
                var centro_costo = context.centro_costo.Where(x => x.bodega == bod_log).Select(x => new
                {
                    value = x.centcst_id,
                    text = x.pre_centcst + " - " + x.centcst_nombre
                }).ToList();
                ViewBag.areaIngreso = new SelectList(centro_costo, "value", "text");
                ViewBag.fpago_id = new SelectList(context.fpago_tercero.OrderBy(x => x.fpago_nombre), "fpago_id",
                    "fpago_nombre");
                List<users> asesores = context.users.Where(x => x.rol_id == 4).ToList();
                List<SelectListItem> listaAsesores = new List<SelectListItem>();
                foreach (users asesor in asesores)
                {
                    listaAsesores.Add(new SelectListItem
                    { Value = asesor.user_id.ToString(), Text = asesor.user_nombre + " " + asesor.user_apellido });
                }

                ViewBag.vendedor = listaAsesores;

                BuscarFavoritos(menu);
                return View();
            }

            ViewBag.bodegaSeleccionada = 0;
            ViewBag.pedidoSeleccionado = 0;
            ViewBag.numFacturacionCreada = 0;
            ListasDesplegables();
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult FacturacionNuevosBackoffice(FacturaVehiculoModel modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                {
                    try
                    {
                        long numeroConsecutivo = 0;
                        string alertaVencimientoPeritaje = "";

                        #region Consecutivo

                        ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
                        icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
                        tp_doc_registros buscarTipoDocRegistro =
                            context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == modelo.tipo);
                        numeroConsecutivoAux =
                            gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro, modelo.tipo, modelo.bodega);

                        grupoconsecutivos grupoConsecutivo = context.grupoconsecutivos.FirstOrDefault(x =>
                            x.documento_id == modelo.tipo && x.bodega_id == modelo.bodega);
                        string alertasFechaOConsecutivo = "";
                        if (numeroConsecutivoAux != null)
                        {
                            numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                            // PRIMERO SE VALIDA QUE LA RESOLUCION ESTE EN LAS FECHAS CORRECTAS Y LOS CONSECUTIVOS DENTRO DE LA RESOLUCION APLIQUEN
                            int grupo = grupoConsecutivo != null ? grupoConsecutivo.grupo : 0;
                            resolucionfactura buscarResolucion =
                                context.resolucionfactura.FirstOrDefault(x =>
                                    x.grupo == grupo && x.tipodoc == modelo.tipo);
                            if (buscarResolucion != null)
                            {
                                int avisoPorConsecutivo = buscarResolucion.consecaviso;
                                int avisoPorFecha = buscarResolucion.diasaviso;
                                TimeSpan diasInicio = buscarResolucion.fechaini.Subtract(DateTime.Now);
                                TimeSpan diasFin = buscarResolucion.fechafin.Subtract(DateTime.Now);
                                if (buscarResolucion.consecfin - numeroConsecutivo <= avisoPorConsecutivo)
                                {
                                    alertasFechaOConsecutivo =
                                        "Quedan " + (buscarResolucion.consecfin - numeroConsecutivo) +
                                        "consecutivos para usar";
                                }

                                if (buscarResolucion.consecini >= numeroConsecutivo &&
                                    buscarResolucion.consecfin <= numeroConsecutivo || diasInicio.Days > 0 ||
                                    diasFin.Days < 0)
                                {
                                    TempData["mensaje_error"] =
                                        "El numero consecutivo asignado no esta vigente en la resolucion, consecutivo actual : "
                                        + numeroConsecutivo + " consecutivos de la resolucion : " +
                                        buscarResolucion.consecini + " hasta " + buscarResolucion.consecfin
                                        + " o la fecha de la resolucion ya vencio, fechas validas estan dentro de " +
                                        buscarResolucion.fechaini.ToShortDateString() + " - " +
                                        buscarResolucion.fechafin.ToShortDateString();
                                    ListasDesplegables();
                                    ViewBag.documentoSeleccionado = modelo.tipo;
                                    ViewBag.bodegaSeleccionado = modelo.bodega;
                                    ViewBag.perfilSeleccionado = modelo.perfilContable;
                                    BuscarFavoritos(menu);
                                    var centroC = context.centro_costo.Where(x => x.bodega == modelo.bodega).Select(x =>
                                        new
                                        {
                                            value = x.centcst_id,
                                            text = x.pre_centcst + " - " + x.centcst_nombre
                                        }).ToList();
                                    ViewBag.areaIngreso = new SelectList(centroC, "value", "text");
                                    return View(modelo);
                                }

                                if (diasFin.Days < avisoPorFecha)
                                {
                                    alertasFechaOConsecutivo =
                                        "la fecha de la resolucion esta proxima a vencer, fechas validas estan dentro de " +
                                        buscarResolucion.fechaini.ToShortDateString() + " - "
                                        + buscarResolucion.fechafin.ToShortDateString();
                                }
                            }
                            else
                            {
                                TempData["mensaje_error"] =
                                    "No existe una resolucion asociada a este tipo de documento";
                                ListasDesplegables();
                                ViewBag.documentoSeleccionado = modelo.tipo;
                                ViewBag.bodegaSeleccionado = modelo.bodega;
                                ViewBag.perfilSeleccionado = modelo.perfilContable;
                                BuscarFavoritos(menu);
                                var centroC = context.centro_costo.Where(x => x.bodega == modelo.bodega).Select(x => new
                                {
                                    value = x.centcst_id,
                                    text = x.pre_centcst + " - " + x.centcst_nombre
                                }).ToList();
                                ViewBag.areaIngreso = new SelectList(centroC, "value", "text");
                                return View(modelo);
                            }
                        }
                        else
                        {
                            TempData["mensaje_error"] =
                                "No existe un numero consecutivo asignado para este tipo de documento";
                            var centroC = context.centro_costo.Where(x => x.bodega == modelo.bodega).Select(x => new
                            {
                                value = x.centcst_id,
                                text = x.pre_centcst + " - " + x.centcst_nombre
                            }).ToList();
                            ViewBag.areaIngreso = new SelectList(centroC, "value", "text");
                            return RedirectToAction("Create", "facturacionNuevos", new { menu });
                        }

                        #endregion

                        bool guardarReferenciasInven = false;
                        int bodegaActual = Convert.ToInt32(Session["user_bodega"]);

                        #region Bussqueda del pedido y del tercero

                        var buscarPedido = (from pedido in context.vpedido
                                            join inventarioHoy in context.vw_inventario_hoy
                                                on pedido.planmayor equals inventarioHoy.ref_codigo
                                            join retoma in context.vpedretoma
                                                on pedido.id equals retoma.pedido_id into ret
                                            from retoma in ret.DefaultIfEmpty()
                                            where pedido.id == modelo.pedido && pedido.bodega == bodegaActual
                                            select new
                                            {
                                                pedido.id,
                                                pedido.numero,
                                                pedido.planmayor,
                                                pedido.vrtotal,
                                                pedido.facturado,
                                                pedido.numfactura,
                                                pedido.flota,
                                                retoma.placa,
                                                inventarioHoy.stock,
                                                inventarioHoy.bodega
                                            }).FirstOrDefault();

                        bool actividadEconomicaValido = false;
                        decimal porcentajeReteICA = 0;
                        decimal porcentajeAutorretencion = 0;
                        //var buscarNitCliente = (from tercero in context.icb_terceros
                        //	join cliente in context.tercero_cliente
                        //		on tercero.tercero_id equals cliente.tercero_id
                        //	join regimen in context.tpregimen_tercero
                        //		on cliente.tpregimen_id equals regimen.tpregimen_id into ps2
                        //	join acteco in context.acteco_tercero
                        //		on cliente.actividadEconomica_id equals acteco.acteco_id into ps3
                        //	from acteco in ps3.DefaultIfEmpty()
                        //	join bodega in context.terceros_bod_ica
                        //		on acteco.acteco_id equals bodega.idcodica into ps
                        //	from bodega in ps.DefaultIfEmpty()
                        //	from regimen in ps2.DefaultIfEmpty()
                        //	where tercero.doc_tercero == modelo.nit
                        //	select new
                        //	{
                        //		cliente.actividadEconomica_id,
                        //		acteco.acteco_nombre,
                        //		tarifaPorBodega = bodega != null ? bodega.porcentaje : 0,
                        //		actecoTarifa = acteco.tarifa,
                        //		actecoAutorretencion = acteco.autorretencion,
                        //		regimen_id = regimen!=null?regimen.tpregimen_id:0
                        //	}).FirstOrDefault();

                        var buscarNitCliente = (from tercero in context.icb_terceros
                                                /*join cliente in context.tercero_cliente
                                                    on tercero.tercero_id equals cliente.tercero_id*/
                                                join acteco in context.acteco_tercero
                                                    on tercero.id_acteco equals acteco.acteco_id into pt
                                                    from acteco in pt.DefaultIfEmpty()
                                                join bodega in context.terceros_bod_ica
                                                    on acteco.acteco_id equals bodega.idcodica into ps
                                                from bodega in ps.DefaultIfEmpty()
                                                join regimen in context.tpregimen_tercero
                                                    on tercero.tpregimen_id equals regimen.tpregimen_id into ps2
                                                from regimen in ps2.DefaultIfEmpty()
                                                where tercero.doc_tercero == modelo.nit
                                                select new
                                                {
                                                    actividadEconomica_id= tercero.id_acteco,
                                                    acteco_nombre = tercero.id_acteco != null ? acteco.acteco_nombre : "",
                                                    actecoAutorretencion = tercero.id_acteco != null ? acteco.autorretencion : 0,
                                                    tarifaPorBodega = bodega != null ? bodega.porcentaje : 0,
                                                    actecoTarifa = acteco.tarifa,
                                                    regimen_id = regimen != null ? regimen.tpregimen_id : 0
                                                }).FirstOrDefault();
                        #endregion


                        if (buscarNitCliente != null)
                        {
                            porcentajeAutorretencion = buscarNitCliente.actecoAutorretencion;
                            if (buscarNitCliente.tarifaPorBodega != 0)
                            {
                                actividadEconomicaValido = true;
                                porcentajeReteICA = buscarNitCliente.tarifaPorBodega;
                            }
                            else if (buscarNitCliente.actecoTarifa != 0)
                            {
                                actividadEconomicaValido = true;
                                porcentajeReteICA = buscarNitCliente.actecoTarifa;
                            }
                        }

                        int regimen_proveedor = buscarNitCliente != null ? buscarNitCliente.regimen_id : 0;
                        int? sw_encontrado = buscarTipoDocRegistro != null ? buscarTipoDocRegistro.sw : 0;
                        perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                            x.bodega == modelo.bodega && x.sw == sw_encontrado &&
                            x.tipo_regimenid == regimen_proveedor);

                        // Realizamos la validaciones para los movimientos contables, si esta es exitosa proseguimos, si no detenemos la ejecucion del codigo

                        #region Movimientos Contables

                        var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                                          join nombreParametro in context.paramcontablenombres
                                                              on perfil.id_nombre_parametro equals nombreParametro.id
                                                          join cuenta in context.cuenta_puc
                                                              on perfil.cuenta equals cuenta.cntpuc_id
                                                          where perfil.id_perfil == modelo.perfilContable
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

                        DateTime fechaHoy = DateTime.Now;
                        decimal? buscarvw_promedio = (from promedio in context.vw_promedio
                                                      where promedio.codigo == modelo.planMayor
                                                            && promedio.ano == fechaHoy.Year && promedio.mes == fechaHoy.Month
                                                      select promedio.Promedio).FirstOrDefault();

                        foreach (var parametro in parametrosCuentasVerificar)
                        {
                            cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
                            decimal valorCredito = 0;
                            decimal valorDebito = 0;
                            string CreditoDebito = "";
                            if (buscarCuenta != null)
                            {
                                if (parametro.id_nombre_parametro == 2)
                                {
                                    valorCredito = Convert.ToDecimal(modelo.iva,miCultura) * Convert.ToDecimal(modelo.precio,miCultura) /
                                                   100;
                                    CreditoDebito = "Credito";
                                    //valorDebito = ((modelo.iva * modelo.precio) / 100);
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
                                                    Convert.ToDecimal(modelo.precio,miCultura))
                                                {
                                                    decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                                    valorDebito =
                                                        Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetencion / 100;
                                                    CreditoDebito = "Debito";
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
                                                    Convert.ToDecimal(modelo.precio,miCultura) * modelo.iva / 100)
                                                {
                                                    decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                                    valorDebito =
                                                        Convert.ToDecimal(modelo.precio,miCultura) * modelo.iva / 100 *
                                                        porcentajeRetIva / 100;
                                                    CreditoDebito = "Debito";
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
                                                        Convert.ToDecimal(modelo.precio,miCultura))
                                                    {
                                                        decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                                        //valorCredito = (modelo.precio * porcentajeRetIca) / 100;
                                                        valorDebito =
                                                            Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetIca / 1000;
                                                        CreditoDebito = "Debito";
                                                    }
                                                }
                                                else
                                                {
                                                    valorCredito =
                                                        Convert.ToDecimal(modelo.precio,miCultura) * porcentajeReteICA / 1000;
                                                    valorDebito =
                                                        Convert.ToDecimal(modelo.precio,miCultura) * porcentajeReteICA / 1000;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (parametro.id_nombre_parametro == 8)
                                {
                                    valorCredito = modelo.impuestoConsumo * Convert.ToDecimal(modelo.precio,miCultura) / 100;
                                    CreditoDebito = "Credito";
                                }

                                if (parametro.id_nombre_parametro == 9)
                                {
                                    valorCredito = buscarvw_promedio ?? 0;
                                    CreditoDebito = "Credito";
                                }

                                if (parametro.id_nombre_parametro == 10)
                                {
                                    decimal restarIcaIvaRetencion = 0;
                                    if (buscarPerfilTributario != null)
                                    {
                                        if (buscarTipoDocRegistro != null)
                                        {
                                            if (buscarPerfilTributario.retfuente == "A")
                                            {
                                                if (buscarTipoDocRegistro.baseretencion <=
                                                    Convert.ToDecimal(modelo.precio,miCultura))
                                                {
                                                    decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                                    decimal sdf = Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetencion /
                                                              100;
                                                    restarIcaIvaRetencion +=
                                                        Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetencion / 100;
                                                }
                                            }

                                            if (buscarPerfilTributario.retiva == "A")
                                            {
                                                if (buscarTipoDocRegistro.baseiva <= Convert.ToDecimal(modelo.precio,miCultura))
                                                {
                                                    decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                                    decimal tt = Convert.ToDecimal(modelo.precio,miCultura) * modelo.iva / 100 *
                                                             porcentajeRetIva / 100;
                                                    restarIcaIvaRetencion +=
                                                        Convert.ToDecimal(modelo.precio,miCultura) * modelo.iva / 100 *
                                                        porcentajeRetIva / 100;
                                                }
                                            }

                                            if (buscarPerfilTributario.retica == "A")
                                            {
                                                if (!actividadEconomicaValido)
                                                {
                                                    if (buscarTipoDocRegistro.baseica <=
                                                        Convert.ToDecimal(modelo.precio,miCultura))
                                                    {
                                                        decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                                        decimal hh = Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetIca /
                                                                 1000;
                                                        restarIcaIvaRetencion +=
                                                            Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetIca / 1000;
                                                    }
                                                }
                                                else
                                                {
                                                    restarIcaIvaRetencion +=
                                                        Convert.ToDecimal(modelo.precio,miCultura) * porcentajeReteICA / 1000;
                                                }
                                            }
                                        }
                                    }

                                    valorDebito = Convert.ToDecimal(modelo.precio,miCultura) +
                                                  modelo.iva * Convert.ToDecimal(modelo.precio,miCultura) / 100 +
                                                  modelo.impuestoConsumo * Convert.ToDecimal(modelo.precio,miCultura) / 100 -
                                                  restarIcaIvaRetencion;
                                    CreditoDebito = "Debito";
                                }

                                if (parametro.id_nombre_parametro == 11)
                                {
                                    valorCredito = Convert.ToDecimal(modelo.precio,miCultura);
                                    CreditoDebito = "Credito";
                                }

                                if (parametro.id_nombre_parametro == 12)
                                {
                                    valorDebito = buscarvw_promedio ?? 0;
                                    CreditoDebito = "Debito";
                                }

                                if (parametro.id_nombre_parametro == 17)
                                {
                                    if (buscarPerfilTributario != null)
                                    {
                                        if (buscarPerfilTributario.autorretencion == "A")
                                        {
                                            valorDebito =
                                                Convert.ToDecimal(modelo.precio,miCultura) * porcentajeAutorretencion / 100;
                                            CreditoDebito = "Debito";
                                        }
                                    }
                                }

                                if (parametro.id_nombre_parametro == 18)
                                {
                                    if (buscarPerfilTributario != null)
                                    {
                                        if (buscarPerfilTributario.autorretencion == "A")
                                        {
                                            valorCredito =
                                                Convert.ToDecimal(modelo.precio,miCultura) * porcentajeAutorretencion / 100;
                                            CreditoDebito = "Credito";
                                        }
                                    }
                                }

                                //valorDebito = Math.Round(valorDebito);
                                //valorCredito = Math.Round(valorCredito);
                                if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                {
                                    calcularDebito += valorDebito;
                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                    {
                                        NumeroCuenta = parametro.cntpuc_numero,
                                        DescripcionParametro = parametro.descripcion_parametro,
                                        ValorDebito = valorDebito,
                                        ValorCredito = 0
                                    });
                                }

                                if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                {
                                    calcularCredito += valorCredito;
                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                    {
                                        NumeroCuenta = parametro.cntpuc_numero,
                                        DescripcionParametro = parametro.descripcion_parametro,
                                        ValorDebito = 0,
                                        ValorCredito = valorCredito
                                    });
                                }
                            }
                        }

                        if (calcularCredito != calcularDebito)
                        {
                            dbTran.Rollback();
                            TempData["documento_descuadrado"] =
                                "El documento no tiene los movimientos calculados correctamente";
                            ViewBag.documentoSeleccionado = modelo.tipo;
                            ViewBag.bodegaSeleccionado = modelo.bodega;
                            ViewBag.perfilSeleccionado = modelo.perfilContable;

                            ViewBag.documentoDescuadrado = listaDescuadrados;
                            ViewBag.calculoDebito = calcularDebito;
                            ViewBag.calculoCredito = calcularCredito;
                            ListasDesplegables();
                            BuscarFavoritos(menu);
                            var centroC = context.centro_costo.Where(x => x.bodega == modelo.bodega).Select(x => new
                            {
                                value = x.centcst_id,
                                text = x.pre_centcst + " - " + x.centcst_nombre
                            }).ToList();
                            ViewBag.areaIngreso = new SelectList(centroC, "value", "text");
                            return View(modelo);
                        }

                        #endregion

                        // Fin de la validacion para el calculo del debito y credito del movimiento contable

                        #region Referencias Inven

                        referencias_inven buscarReferenciasInven = context.referencias_inven.FirstOrDefault(x =>
                            x.codigo == modelo.planMayor && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month);
                        if (buscarReferenciasInven != null)
                        {
                            buscarReferenciasInven.can_sal = 1;
                            buscarReferenciasInven.cos_sal = buscarvw_promedio ?? 0;
                            buscarReferenciasInven.can_vta = 1;
                            buscarReferenciasInven.cos_vta = buscarvw_promedio ?? 0;
                            context.Entry(buscarReferenciasInven).State = EntityState.Modified;

                            // Se busca el vehiculo  de la tabla icb_vehiculo, para actualizarle el campo propietario
                            icb_vehiculo buscarVehiculo =
                                context.icb_vehiculo.FirstOrDefault(x => x.plan_mayor == modelo.planMayor);
                            if (buscarVehiculo != null)
                            {
                                buscarVehiculo.propietario = modelo.id_tercero;
                                buscarVehiculo.fecha_venta = DateTime.Now;
                                context.Entry(buscarVehiculo).State = EntityState.Modified;
                            }

                            guardarReferenciasInven = context.SaveChanges() > 0;
                        }
                        else
                        {
                            dbTran.Rollback();
                            TempData["mensaje_error"] =
                                "El vehiculo buscado no se encuentra disponible en el inventario";
                            ViewBag.documentoSeleccionado = modelo.tipo;
                            ViewBag.bodegaSeleccionado = modelo.bodega;
                            ViewBag.perfilSeleccionado = modelo.perfilContable;
                            ListasDesplegables();
                            BuscarFavoritos(menu);
                            var centroC = context.centro_costo.Where(x => x.bodega == modelo.bodega).Select(x => new
                            {
                                value = x.centcst_id,
                                text = x.pre_centcst + " - " + x.centcst_nombre
                            }).ToList();
                            ViewBag.areaIngreso = new SelectList(centroC, "value", "text");
                            return View(modelo);
                        }

                        #endregion

                        fpago_tercero buscarDiasVencimiento =
                            context.fpago_tercero.FirstOrDefault(x => x.fpago_id == modelo.fpago_id);
                        icb_terceros buscarCliente = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == modelo.nit);
                        int nitCliente = buscarCliente != null ? buscarCliente.tercero_id : 0;

                        #region Encabezado documento

                        encab_documento crearEncabezado = new encab_documento
                        {
                            tipo = modelo.tipo,
                            pedido = modelo.pedido,
                            bodega = modelo.bodega,
                            fpago_id = modelo.fpago_id,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            nit = nitCliente,
                            costo = buscarvw_promedio ?? 0,
                            numero = numeroConsecutivo,
                            documento = modelo.planMayor,
                            fecha = DateTime.Now,
                            fec_creacion = DateTime.Now,
                            vendedor = modelo.vendedor,
                            estado = true,
                            notas = modelo.nota,
                            centro_doc = modelo.areaIngreso
                        };

                        if (buscarDiasVencimiento != null)
                        {
                            crearEncabezado.vencimiento = DateTime.Now.AddDays(
                                buscarDiasVencimiento.dvencimiento != null
                                    ? buscarDiasVencimiento.dvencimiento.Value
                                    : 0);
                        }

                        if (modelo.pedido != null)
                        {
                            vpedido buscarVPedido = context.vpedido.FirstOrDefault(x => x.id == modelo.pedido);
                            if (buscarVPedido != null)
                            {
                                crearEncabezado.id_pedido_vehiculo = buscarVPedido.id;
                            }
                            else
                            {
                                crearEncabezado.id_pedido_vehiculo = null;
                            }
                        }

                        crearEncabezado.valor_total = Convert.ToDecimal(modelo.valorTotal,miCultura);
                        crearEncabezado.iva = Convert.ToDecimal(modelo.iva,miCultura) * Convert.ToDecimal(modelo.precio,miCultura) / 100;
                        crearEncabezado.valor_mercancia = Convert.ToDecimal(modelo.precio,miCultura);
                        crearEncabezado.impoconsumo = modelo.impuestoConsumo;

                        if (buscarPerfilTributario != null)
                        {
                            if (buscarTipoDocRegistro != null)
                            {
                                if (buscarPerfilTributario.retfuente == "A")
                                {
                                    if (buscarTipoDocRegistro.baseretencion <= Convert.ToDecimal(modelo.precio,miCultura))
                                    {
                                        decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                        crearEncabezado.retencion =
                                            Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetencion / 100;
                                    }
                                }

                                if (buscarPerfilTributario.retiva == "A")
                                {
                                    if (buscarTipoDocRegistro.baseiva <= Convert.ToDecimal(modelo.precio,miCultura))
                                    {
                                        decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                        crearEncabezado.retencion_iva =
                                            Convert.ToDecimal(modelo.precio,miCultura) * modelo.iva / 100 * porcentajeRetIva /
                                            100;
                                    }
                                }

                                if (buscarPerfilTributario.retica == "A")
                                {
                                    if (!actividadEconomicaValido)
                                    {
                                        if (buscarTipoDocRegistro.baseica <= Convert.ToDecimal(modelo.precio,miCultura))
                                        {
                                            decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                            crearEncabezado.retencion_ica +=
                                                Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetIca / 1000;
                                        }
                                    }
                                    else
                                    {
                                        crearEncabezado.retencion_ica =
                                            Convert.ToDecimal(modelo.precio,miCultura) * porcentajeReteICA / 1000;
                                    }
                                }
                            }
                        }

                        context.encab_documento.Add(crearEncabezado);
                        int save = context.SaveChanges();
                        encab_documento buscarUltimoEncabezado = context.encab_documento.OrderByDescending(x => x.idencabezado)
                            .FirstOrDefault();

                        #endregion

                        #region Lineas

                        lineas_documento crearLineas = new lineas_documento
                        {
                            codigo = modelo.planMayor,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            nit = nitCliente,
                            cantidad = 1,
                            bodega = modelo.bodega,
                            seq = 1,
                            porcentaje_iva = (float)modelo.iva,
                            valor_unitario = Convert.ToDecimal(modelo.precio,miCultura),
                            impoconsumo = (float)modelo.impuestoConsumo,
                            id_tarifa_cliente = 1,
                            estado = true,
                            fec = DateTime.Now,
                            costo_unitario = buscarvw_promedio ?? 0,
                            id_encabezado = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0
                        };
                        context.lineas_documento.Add(crearLineas);

                        #endregion

                        #region Movimientos contables

                        List<perfil_cuentas_documento> parametrosCuentas = context.perfil_cuentas_documento
                            .Where(x => x.id_perfil == modelo.perfilContable).ToList();
                        centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                        int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
                        icb_terceros terceroValorCero = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
                        int idTerceroCero = terceroValorCero != null ? Convert.ToInt32(terceroValorCero.tercero_id) : 0;
                        int secuencia = 1;
                        foreach (perfil_cuentas_documento parametro in parametrosCuentas)
                        {
                            decimal valorCredito = 0;
                            decimal valorDebito = 0;
                            string CreditoDebito = "";
                            cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
                            if (buscarCuenta != null)
                            {
                                if (parametro.id_nombre_parametro == 2)
                                {
                                    valorCredito = modelo.iva * Convert.ToDecimal(modelo.precio,miCultura) / 100;
                                    CreditoDebito = "Credito";
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
                                                    Convert.ToDecimal(modelo.precio,miCultura))
                                                {
                                                    decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                                    valorDebito =
                                                        Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetencion / 100;
                                                    CreditoDebito = "Debito";
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
                                                    Convert.ToDecimal(modelo.precio,miCultura) * modelo.iva / 100)
                                                {
                                                    decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                                    valorDebito =
                                                        Convert.ToDecimal(modelo.precio,miCultura) * modelo.iva / 100 *
                                                        porcentajeRetIva / 100;
                                                    CreditoDebito = "Debito";
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
                                                        Convert.ToDecimal(modelo.precio,miCultura))
                                                    {
                                                        decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                                        valorDebito =
                                                            Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetIca / 1000;
                                                        CreditoDebito = "Debito";
                                                    }
                                                }
                                                else
                                                {
                                                    valorCredito =
                                                        Convert.ToDecimal(modelo.precio,miCultura) * porcentajeReteICA / 1000;
                                                    valorDebito =
                                                        Convert.ToDecimal(modelo.precio,miCultura) * porcentajeReteICA / 1000;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (parametro.id_nombre_parametro == 8)
                                {
                                    valorCredito = Convert.ToDecimal(modelo.valorImpuestoConsumo,miCultura);
                                    CreditoDebito = "Credito";
                                }

                                if (parametro.id_nombre_parametro == 9)
                                {
                                    valorCredito = buscarvw_promedio ?? 0;
                                    CreditoDebito = "Credito";
                                }

                                if (parametro.id_nombre_parametro == 10)
                                {
                                    decimal restarIcaIvaRetencion = 0;
                                    if (buscarPerfilTributario != null)
                                    {
                                        if (buscarTipoDocRegistro != null)
                                        {
                                            if (buscarPerfilTributario.retfuente == "A")
                                            {
                                                if (buscarTipoDocRegistro.baseretencion <=
                                                    Convert.ToDecimal(modelo.precio,miCultura))
                                                {
                                                    decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                                    restarIcaIvaRetencion +=
                                                        Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetencion / 100;
                                                }
                                            }

                                            if (buscarPerfilTributario.retiva == "A")
                                            {
                                                if (buscarTipoDocRegistro.baseiva <= Convert.ToDecimal(modelo.precio,miCultura))
                                                {
                                                    decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                                    restarIcaIvaRetencion +=
                                                        Convert.ToDecimal(modelo.precio,miCultura) * modelo.iva / 100 *
                                                        porcentajeRetIva / 100;
                                                }
                                            }

                                            if (buscarPerfilTributario.retica == "A")
                                            {
                                                if (!actividadEconomicaValido)
                                                {
                                                    if (buscarTipoDocRegistro.baseica <=
                                                        Convert.ToDecimal(modelo.precio,miCultura))
                                                    {
                                                        decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                                        restarIcaIvaRetencion +=
                                                            Convert.ToDecimal(modelo.precio,miCultura) * porcentajeRetIca / 1000;
                                                    }
                                                }
                                                else
                                                {
                                                    restarIcaIvaRetencion +=
                                                        Convert.ToDecimal(modelo.precio,miCultura) * porcentajeReteICA / 1000;
                                                }
                                            }
                                        }
                                    }

                                    valorDebito = Convert.ToDecimal(modelo.precio,miCultura) +
                                                  modelo.iva * Convert.ToDecimal(modelo.precio,miCultura) / 100 +
                                                  modelo.impuestoConsumo * Convert.ToDecimal(modelo.precio,miCultura) / 100 -
                                                  restarIcaIvaRetencion;
                                    CreditoDebito = "Debito";
                                }

                                if (parametro.id_nombre_parametro == 11)
                                {
                                    valorCredito = Convert.ToDecimal(modelo.precio,miCultura);
                                    CreditoDebito = "Credito";
                                }

                                if (parametro.id_nombre_parametro == 12)
                                {
                                    valorDebito = buscarvw_promedio ?? 0;
                                    CreditoDebito = "Debito";
                                }

                                if (parametro.id_nombre_parametro == 17)
                                {
                                    if (buscarPerfilTributario != null)
                                    {
                                        if (buscarPerfilTributario.autorretencion == "A")
                                        {
                                            valorDebito =
                                                Convert.ToDecimal(modelo.precio,miCultura) * porcentajeAutorretencion / 100;
                                            CreditoDebito = "Debito";
                                        }
                                    }
                                }

                                if (parametro.id_nombre_parametro == 18)
                                {
                                    if (buscarPerfilTributario != null)
                                    {
                                        if (buscarPerfilTributario.autorretencion == "A")
                                        {
                                            valorCredito =
                                                Convert.ToDecimal(modelo.precio,miCultura) * porcentajeAutorretencion / 100;
                                            CreditoDebito = "Credito";
                                        }
                                    }
                                }

                                if (valorCredito != 0 || valorDebito != 0)
                                {
                                    mov_contable movNuevo = new mov_contable
                                    {
                                        id_encab = buscarUltimoEncabezado != null
                                        ? buscarUltimoEncabezado.idencabezado
                                        : 0,
                                        fec_creacion = DateTime.Now,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                        idparametronombre = parametro.id_nombre_parametro,
                                        cuenta = parametro.cuenta,
                                        centro = parametro.centro,
                                        nit = nitCliente,
                                        fec = DateTime.Now,
                                        seq = secuencia,
                                        debito = valorDebito,
                                        credito = valorCredito
                                    };

                                    if (parametro.id_nombre_parametro == 4)
                                    {
                                        movNuevo.basecontable = modelo.iva * Convert.ToDecimal(modelo.precio,miCultura) / 100;
                                    }

                                    if (buscarTipoDocRegistro.aplicaniff)
                                    {
                                        if (buscarCuenta.concepniff == 1 || buscarCuenta.concepniff == 4)
                                        {
                                            if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                            {
                                                movNuevo.debitoniif = valorDebito;
                                            }

                                            if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                            {
                                                movNuevo.creditoniif = valorCredito;
                                            }
                                        }
                                    }

                                    if (buscarCuenta.manejabase)
                                    {
                                        movNuevo.basecontable = Convert.ToDecimal(modelo.precio,miCultura);
                                    }

                                    if (buscarCuenta.tercero)
                                    {
                                        movNuevo.nit = nitCliente;
                                    }

                                    if (buscarCuenta.documeto)
                                    {
                                        movNuevo.documento = buscarUltimoEncabezado.numero.ToString();
                                    }

                                    movNuevo.detalle = "Facturacion vehiculos fact " + buscarUltimoEncabezado.documento;
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
                                }
                            }
                        }

                        #endregion

                        bool guardarCuenta = context.SaveChanges() > 0;
                        if (guardarReferenciasInven && guardarCuenta)
                        {
                            #region Actualiza el campo factura, para saber que a un pedido ya se le hizo su respectiva facturacion.

                            vpedido buscarPedidoParaActualizar = context.vpedido.FirstOrDefault(x => x.id == modelo.pedido);
                            if (buscarPedidoParaActualizar != null)
                            {
                                buscarPedidoParaActualizar.facturado = true;
                                buscarPedidoParaActualizar.numfactura = (int)numeroConsecutivo;
                                context.Entry(buscarPedidoParaActualizar).State = EntityState.Modified;
                            }

                            #endregion

                            #region Eventos del vehiculo

                            icb_sysparameter buscarParametroEventoFact =
                                context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P4");
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
                                bodega_id = modelo.bodega,
                                //vin = vehiculo.vin,
                                planmayor = modelo.planMayor
                            });
                            int guardarVehEventos = context.SaveChanges();

                            #endregion

                            #region Seguimiento

                            context.seguimientotercero.Add(new seguimientotercero
                            {
                                idtercero = nitCliente,
                                tipo = 5,
                                nota = "El tercero realizo un pedido con numero " + numeroConsecutivo,
                                fecha = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                            });

                            #endregion

                            // Actualiza los numeros consecutivos por documento

                            #region Guardado final y actualizacion del consecutivo

                            int grupoId = grupoConsecutivo != null ? grupoConsecutivo.grupo : 0;
                            List<grupoconsecutivos> gruposConsecutivos = context.grupoconsecutivos.Where(x => x.grupo == grupoId).ToList();
                            foreach (grupoconsecutivos grupo in gruposConsecutivos)
                            {
                                icb_doc_consecutivos buscarElemento = context.icb_doc_consecutivos.FirstOrDefault(x =>
                                    x.doccons_idtpdoc == modelo.tipo && x.doccons_bodega == grupo.bodega_id);
                                buscarElemento.doccons_siguiente = buscarElemento.doccons_siguiente + 1;
                                context.Entry(buscarElemento).State = EntityState.Modified;
                            }

                            context.SaveChanges();
                            ViewBag.numFacturacionCreada = numeroConsecutivo;
                            modelo.numero = numeroConsecutivo;
                            dbTran.Commit();
                            TempData["mensaje"] = "La facturacion se ha guardado exitosamente. " +
                                                  alertasFechaOConsecutivo + ". " + alertaVencimientoPeritaje;

                            ListasDesplegables();
                            ViewBag.documentoSeleccionado = modelo.tipo;
                            ViewBag.bodegaSeleccionado = modelo.bodega;
                            ViewBag.perfilSeleccionado = modelo.perfilContable;
                            BuscarFavoritos(menu);
                            var centroC = context.centro_costo.Where(x => x.bodega == modelo.bodega).Select(x => new
                            {
                                value = x.centcst_id,
                                text = x.pre_centcst + " - " + x.centcst_nombre
                            }).ToList();
                            ViewBag.areaIngreso = new SelectList(centroC, "value", "text");
                            string stringencabezado = buscarUltimoEncabezado.idencabezado.ToString();
                            byte[] en2 = Encoding.UTF8.GetBytes(stringencabezado);
                            string en = Convert.ToBase64String(en2);
                            return RedirectToAction("BrowserBackoffice", "vpedidos", new { en = en });

                            #endregion
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error de conexion con base de datos";
                            ListasDesplegables();
                            BuscarFavoritos(menu);
                            var centroC = context.centro_costo.Where(x => x.bodega == modelo.bodega).Select(x => new
                            {
                                value = x.centcst_id,
                                text = x.pre_centcst + " - " + x.centcst_nombre
                            }).ToList();
                            ViewBag.areaIngreso = new SelectList(centroC, "value", "text");
                            return View(modelo);
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
                Dictionary<string, string[]> errorList = ModelState.Where(elem => elem.Value.Errors.Any()).ToDictionary(kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e =>
                        string.IsNullOrEmpty(e.ErrorMessage) ? e.Exception.Message : e.ErrorMessage).ToArray());

                TempData["mensaje_error"] = "Error en el modelo, por favor verifique campos obligatorios.";
                ListasDesplegables();
                BuscarFavoritos(menu);
                var centroC = context.centro_costo.Where(x => x.bodega == modelo.bodega).Select(x => new
                {
                    value = x.centcst_id,
                    text = x.pre_centcst + " - " + x.centcst_nombre
                }).ToList();
                ViewBag.areaIngreso = new SelectList(centroC, "value", "text");
                ViewBag.documentoSeleccionado = modelo.tipo;
                ViewBag.bodegaSeleccionado = modelo.bodega;
                ViewBag.perfilSeleccionado = modelo.perfilContable;
                var centro = context.centro_costo.Where(x => x.bodega == modelo.bodega).Select(x => new
                {
                    value = x.centcst_id,
                    text = x.pre_centcst + " - " + x.centcst_nombre
                }).ToList();
                return View(modelo);
            }
            //ListasDesplegables();

            //BuscarFavoritos(menu);

            //ViewBag.areaIngreso = new SelectList(centro, "value", "text");
            //return View(modelo);
        }


        public JsonResult BuscarPedidosNoFacturados(int? id_bodega)
        {
            var buscarPedidos = (from pedido in context.vpedido
                                 join tercero in context.icb_terceros
                                     on pedido.nit equals tercero.tercero_id
                                 where pedido.bodega == id_bodega && pedido.planmayor != null && pedido.facturado == false &&
                                       pedido.numfactura == null
                                 select new
                                 {
                                     pedido.id,
                                     cliente = "(" + pedido.numero + ") " + tercero.razon_social + tercero.prinom_tercero + " " +
                                               tercero.segnom_tercero + " " + tercero.apellido_tercero + " " +
                                               tercero.segapellido_tercero
                                 }).ToList();
            return Json(buscarPedidos, JsonRequestBehavior.AllowGet);
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

            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            var buscarBodegasDelDocumento = (from doc_consecutivos in context.icb_doc_consecutivos
                                             join bodega in context.bodega_concesionario
                                                 on doc_consecutivos.doccons_bodega equals bodega.id
                                             where doc_consecutivos.doccons_idtpdoc == idTpDoc && doc_consecutivos.doccons_bodega == bodegaActual
                                             select new
                                             {
                                                 bodega.id,
                                                 bodega.bodccs_nombre
                                             }).ToList();

            var buscarPerfiles = context.perfil_contable_documento.Where(x => x.tipo == idTpDoc).Select(x => new
            {
                x.id,
                x.descripcion
            }).ToList();

            return Json(new { buscarConceptos, buscarConceptos2, buscarBodegasDelDocumento, buscarPerfiles },
                JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarAreasIngreso(int bodega)
        {
            var buscar = context.centro_costo.Where(x => x.centcst_estado && x.bodega == bodega).Select(x => new
            {
                value = x.centcst_id,
                text = x.pre_centcst + " - " + x.Tipos_Cartera.descripcion
            }).OrderBy(x => x.text).ToList();

            return Json(buscar, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EnviarNotificacionPorPedido(string plan_mayor)
        {
            int usuario_actual = Convert.ToInt32(Session["user_usuarioid"]);
            icb_vehiculo vh = context.icb_vehiculo.Find(plan_mayor);
            int result = 0;
            configuracion_envio_correos correoconfig = context.configuracion_envio_correos.Where(d => d.activo).FirstOrDefault();

            // El tipo de autorizacion 2 hace referencia a facturacion de vehiculos (tabla tipoautorizacion)
            IQueryable<usuarios_autorizaciones> usuarios_autorizacion =
                context.usuarios_autorizaciones.Where(x => x.bodega_id == vh.id_bod && x.tipoautorizacion == 2);
            if (usuarios_autorizacion != null)
            {
                autorizaciones existe = context.autorizaciones.FirstOrDefault(x =>
                    x.plan_mayor == plan_mayor && x.bodega == vh.id_bod && x.tipo_autorizacion == 2);

                if (existe == null)
                {
                    //var usuario_autorizacion = usuarios_autorizacion.user_id;

                    autorizaciones autorizacion = new autorizaciones
                    {
                        plan_mayor = plan_mayor,
                        //autorizacion.user_autorizacion = usuario_autorizacion; No hay usuario de autorizacion hasta que alguno de los asignados de autorizacion se guarda
                        user_creacion = usuario_actual,
                        comentario = "Notificación solicitud autorización por falta de documentos",
                        fecha_creacion = DateTime.Now,
                        tipo_autorizacion = 2,
                        bodega = vh.id_bod
                    };
                    context.autorizaciones.Add(autorizacion);
                    context.SaveChanges();
                    int autorizacion_id = context.autorizaciones.OrderByDescending(x => x.id).FirstOrDefault().id;
                    result = 1;

                    foreach (usuarios_autorizaciones usuarioCorreo in usuarios_autorizacion)
                    {
                        try
                        {
                            notificaciones correo_enviado = context.notificaciones.FirstOrDefault(x =>
                                x.user_destinatario == usuarioCorreo.user_id && x.enviado != true &&
                                x.autorizacion_id == autorizacion_id);
                            if (correo_enviado == null)
                            {
                                users user_destinatario = context.users.Find(usuarioCorreo.user_id);
                                users user_remitente = context.users.Find(usuario_actual);

                                MailAddress de = new MailAddress(correoconfig.correo, correoconfig.nombre_remitente);
                                MailAddress para = new MailAddress(user_destinatario.user_email,
                                    user_destinatario.user_nombre + " " + user_destinatario.user_apellido);
                                MailMessage mensaje = new MailMessage(de, para);
                                mensaje.Bcc.Add(user_destinatario.user_email);
                                mensaje.ReplyToList.Add(new MailAddress(user_remitente.user_email,
                                    user_remitente.user_nombre + " " + user_remitente.user_apellido));
                                mensaje.Subject = "Solicitud Autorización plan mayor " + plan_mayor;
                                mensaje.BodyEncoding = Encoding.Default;
                                mensaje.IsBodyHtml = true;
                                string html = "";
                                html += "<h4>Cordial Saludo</h4><br>";
                                html += "<p>El usuario " + user_remitente.user_nombre + " " +
                                        user_remitente.user_apellido + " solicita autorización para la asignación del "
                                        + " vehículo con plan mayor " + plan_mayor +
                                        " por falta de documentos en un pedido para facturar </p><br /><br />";
                                html += "Por favor ingrese a la plataforma para dar autorización.";
                                mensaje.Body = html;

                                SmtpClient cliente = new SmtpClient(correoconfig.smtp_server)
                                {
                                    Port = correoconfig.puerto,
                                    UseDefaultCredentials = false,
                                    Credentials =
                                    new NetworkCredential(correoconfig.usuario, correoconfig.password),
                                    EnableSsl = true
                                };
                                cliente.Send(mensaje);

                                notificaciones envio = new notificaciones
                                {
                                    user_remitente = usuario_actual,
                                    asunto = "Notificación solicitud autorización por falta de documentos",
                                    fecha_envio = DateTime.Now,
                                    enviado = true,
                                    user_destinatario = usuarioCorreo.user_id,
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
                                asunto = "Notificación solicitud autorización por falta de documentos",
                                fecha_envio = DateTime.Now,
                                user_destinatario = usuarioCorreo.user_id,
                                autorizacion_id = autorizacion_id,
                                enviado = false,
                                razon_no_envio = ex.Message
                            };
                            context.notificaciones.Add(envio);
                            context.SaveChanges();
                            // notificacion no enviada
                            result = -1;
                        }
                    }
                }
                else
                {
                    // ya existe
                    result = 2;
                }
            }
            else
            {
                result = 3;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ValidarFacturaCompleta(int? pedido_id, int? bodega_id, string planMayor)
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            var buscarPedido = (from pedido in context.vpedido
                                join inventarioHoy in context.vw_inventario_hoy
                                    on pedido.planmayor equals inventarioHoy.ref_codigo
                                join retoma in context.vpedretoma
                                    on pedido.id equals retoma.pedido_id into ret
                                from retoma in ret.DefaultIfEmpty()
                                where pedido.numero == pedido_id && pedido.bodega == bodegaActual
                                select new
                                {
                                    pedido.id,
                                    pedido.numero,
                                    pedido.planmayor,
                                    pedido.vrtotal,
                                    pedido.facturado,
                                    pedido.numfactura,
                                    pedido.flota,
                                    retoma.placa,
                                    inventarioHoy.stock,
                                    inventarioHoy.bodega
                                }).FirstOrDefault();

            if (pedido_id != null)
            {
                if (buscarPedido != null)
                {
                    // Validacion para saber si el pedido tiene algun vehiculo en retoma, y si tiene saber cuando fue el peritaje para saber si el
                    // peritaje ya esta obsoleto por pasar tiempo
                    var buscarPlaca = (from encabezadoPeritaje in context.icb_encabezado_insp_peritaje
                                       join solicitudPeritaje in context.icb_solicitud_peritaje
                                           on encabezadoPeritaje.encab_insper_idsolicitud equals solicitudPeritaje.id_peritaje
                                       join vhRetoma in context.icb_tercero_vhretoma
                                           on solicitudPeritaje.id_tercero_vhretoma equals vhRetoma.veh_id
                                       where vhRetoma.placa == buscarPedido.placa
                                       select new
                                       {
                                           vhRetoma.placa,
                                           encabezadoPeritaje.encab_insper_fecha
                                       }).OrderByDescending(x => x.encab_insper_fecha).FirstOrDefault();

                    if (buscarPlaca != null)
                    {
                        icb_sysparameter buscarTiempoVencimiento =
                            context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P32");
                        int diasVencimiento = buscarTiempoVencimiento != null
                            ? Convert.ToInt32(buscarTiempoVencimiento.syspar_value)
                            : 0;
                        if (buscarPlaca.encab_insper_fecha.AddDays(diasVencimiento) < DateTime.Now)
                        {
                            return Json(
                                new
                                {
                                    correcto = false,
                                    error = 1,
                                    mensaje = "El peritaje ya supero los " + diasVencimiento +
                                              " dias establecidos para estar vigente, fecha del peritaje: " +
                                              buscarPlaca.encab_insper_fecha.ToShortDateString()
                                }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    // Primero hay que validar si el pedido es de una flota y ver que los documentos requeridos cuando es flota esten completos


                    if (buscarPedido.flota != null)
                    {
                        //var documentosRequiereFlota = (from pedido in context.vpedido
                        //                               join flota in context.vflota
                        //                               on pedido.flota equals flota.idflota
                        //                               join codFlota in context.vcodflota
                        //                               on flota.flota equals codFlota.id
                        //                               join docRequeridos in context.vdocrequeridosflota
                        //                               on codFlota.id equals docRequeridos.codflota
                        //                               where pedido.numero == pedido_id && pedido.bodega == bodega_id
                        //                               select new
                        //                               {
                        //                                   docRequeridos.iddocumento
                        //                               }).ToList();
                        var documentosRequiereFlota = (from d in context.vdocrequeridosflota
                                                       where d.codflota == buscarPedido.flota
                                                       select new
                                                       {
                                                           d.id,
                                                           d.iddocumento,
                                                           d.vdocumentosflota.documento
                                                       }).ToList();

                        var documentosQueTienePedido = (from validaPedido in context.vvalidacionpeddoc
                                                        where validaPedido.idpedido == buscarPedido.id && validaPedido.idflota == buscarPedido.flota
                                                        select new
                                                        {
                                                            validaPedido.iddocrequerido,
                                                            validaPedido.estado
                                                        }).ToList();

                        bool documentosCompletos = true;
                        foreach (var documentosRequiere in documentosRequiereFlota)
                        {
                            bool contieneDocumento = false;
                            foreach (var documentosTiene in documentosQueTienePedido)
                            {
                                if (documentosRequiere.iddocumento == documentosTiene.iddocrequerido &&
                                    documentosTiene.estado)
                                {
                                    contieneDocumento = true;
                                    break;
                                }
                            }

                            if (!contieneDocumento)
                            {
                                documentosCompletos = false;
                                break;
                            }
                        }

                        if (!documentosCompletos)
                        {
                            icb_vehiculo vh = context.icb_vehiculo.Find(planMayor);
                            List<usuarios_autorizaciones> usuarios_autorizacion = context.usuarios_autorizaciones
                                .Where(x => x.bodega_id == vh.id_bod && x.tipoautorizacion == 2).ToList();
                            if (usuarios_autorizacion.Count > 0)
                            {
                                foreach (usuarios_autorizaciones usuario in usuarios_autorizacion)
                                {
                                    autorizaciones existe = context.autorizaciones.FirstOrDefault(x =>
                                        x.plan_mayor == planMayor && x.user_autorizacion == usuario.user_id);
                                    if (existe != null)
                                    {
                                        if (existe.autorizado)
                                        {
                                            return Json(
                                                new
                                                {
                                                    correcto = true,
                                                    error = 2,
                                                    mensaje = "El vehículo seleccionado tiene los documentos completos"
                                                }, JsonRequestBehavior.AllowGet);
                                        }

                                        return Json(
                                            new
                                            {
                                                correcto = false,
                                                error = 2,
                                                mensaje =
                                                    "El vehículo seleccionado no tiene los documentos completos, ya se envio la solicitud pero no se ha autorizado para facturacion"
                                            }, JsonRequestBehavior.AllowGet);
                                    }
                                }
                            }

                            return Json(
                                new
                                {
                                    correcto = false,
                                    error = 2,
                                    mensaje =
                                        "El vehículo seleccionado no tiene los documentos completos en el pedido, se debe enviar una notificación a la persona encargada para que de autorización. ¿Desea enviar la notificación?"
                                }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    // Fin de la validacion para validar si el pedido es de una flota y ver que los documentos requeridos cuando es flota esten completos

                    if (buscarPedido.facturado)
                    {
                        return Json(
                            new
                            {
                                correcto = false,
                                error = 3,
                                mensaje = "El pedido ya se encuentra facturado con numero de factura " +
                                          buscarPedido.numfactura
                            }, JsonRequestBehavior.AllowGet);
                    }

                    if (buscarPedido.planmayor == null)
                    {
                        return Json(
                            new
                            {
                                correcto = false,
                                error = 4,
                                mensaje = "El pedido no tiene un plan mayor asignado en el momento" +
                                          buscarPedido.numfactura
                            }, JsonRequestBehavior.AllowGet);
                    }

                    if (buscarPedido.stock < 1)
                    {
                        return Json(
                            new { correcto = false, error = 5, mensaje = "El plan mayor se encuentra en stock cero" },
                            JsonRequestBehavior.AllowGet);
                    }

                    if (buscarPedido.bodega != bodegaActual)
                    {
                        return Json(
                            new
                            {
                                correcto = false,
                                error = 6,
                                mensaje =
                                    "La bodega del usuario actual es diferente a la bodega donde esta el inventario"
                            }, JsonRequestBehavior.AllowGet);
                    }

                    if (buscarPedido.planmayor != planMayor)
                    {
                        return Json(
                            new
                            {
                                correcto = false,
                                error = 7,
                                mensaje = "El plan mayor registrado no es igual al del pedido buscado"
                            }, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            return Json(new { correcto = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Ver(int? numero, int? tpDoc, int? bodega, int? menu)
        {
            var buscarFactura = (from encabezado in context.encab_documento
                                 join tercero in context.icb_terceros
                                     on encabezado.nit equals tercero.tercero_id
                                 join vehiculo in context.icb_vehiculo
                                     on encabezado.documento equals vehiculo.plan_mayor into vh
                                 from vehiculo in vh.DefaultIfEmpty()
                                 join modelo in context.modelo_vehiculo
                                     on vehiculo.modvh_id equals modelo.modvh_codigo into mod
                                 from modelo in mod.DefaultIfEmpty()
                                 join concepto1 in context.tpdocconceptos
                                     on encabezado.concepto equals concepto1.id into ps
                                 from concepto1 in ps.DefaultIfEmpty()
                                 join concepto2 in context.tpdocconceptos2
                                     on encabezado.concepto2 equals concepto2.id into ps2
                                 from concepto2 in ps.DefaultIfEmpty()
                                 where encabezado.numero == numero && (encabezado.tipo == tpDoc) & (encabezado.bodega == bodega)
                                 select new
                                 {
                                     encabezado.pedido,
                                     encabezado.valor_total,
                                     encabezado.valor_mercancia,
                                     encabezado.iva,
                                     encabezado.impoconsumo,
                                     encabezado.numero,
                                     encabezado.bodega,
                                     encabezado.idencabezado,
                                     encabezado.tipo,
                                     concepto1 = concepto1.Descripcion,
                                     concepto2 = concepto2.Descripcion,
                                     tercero.doc_tercero,
                                     tercero.prinom_tercero,
                                     tercero.segnom_tercero,
                                     tercero.apellido_tercero,
                                     tercero.segapellido_tercero,
                                     direc_tercero = (from direccion in context.terceros_direcciones
                                                      join ciudad in context.nom_ciudad
                                                          on direccion.ciudad equals ciudad.ciu_id
                                                      where direccion.idtercero == tercero.tercero_id
                                                      orderby direccion.id descending
                                                      select new
                                                      {
                                                          direccion.id,
                                                          direccion = direccion.direccion + " (" + ciudad.ciu_nombre + ")"
                                                      }).FirstOrDefault(),
                                     tercero.celular_tercero,
                                     vehiculo.plan_mayor,
                                     modelo.modvh_nombre
                                 }).FirstOrDefault();

            if (buscarFactura != null)
            {
                CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
                FacturaVehiculoModel modelo = new FacturaVehiculoModel
                {
                    pedido = buscarFactura.pedido,
                    conceptoLetras = buscarFactura.concepto1,
                    concepto2Letras = buscarFactura.concepto2,
                    nit = buscarFactura.doc_tercero,
                    primerNombre = buscarFactura.prinom_tercero,
                    segundoNombre = buscarFactura.segnom_tercero,
                    primerApellido = buscarFactura.apellido_tercero,
                    segundoApellido = buscarFactura.segapellido_tercero,
                    direccion = buscarFactura.direc_tercero != null ? buscarFactura.direc_tercero.direccion : "",
                    celular = buscarFactura.celular_tercero,
                    planMayor = buscarFactura.plan_mayor,
                    modeloVh = buscarFactura.modvh_nombre,
                    valorTotal = buscarFactura.valor_total.ToString("0,0", elGR),
                    precio = buscarFactura.valor_mercancia.ToString("0,0", elGR),
                    iva = buscarFactura.iva,
                    impuestoConsumo = buscarFactura.impoconsumo,
                    numero = buscarFactura.numero,
                    bodega = buscarFactura.bodega,
                    tipo = buscarFactura.tipo,
                    encabDoc = buscarFactura.idencabezado
                };
                ViewBag.encabezado = modelo.encabDoc;
                BuscarFavoritos(menu);
                return View(modelo);
            }
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult BuscarFacturaciones()
        {
            int rolActual = Convert.ToInt32(Session["user_rolid"]);
            rolacceso buscarSiTodasBodegas = context.rolacceso.FirstOrDefault(x => x.idrol == rolActual && x.idpermiso == 1);
            if (buscarSiTodasBodegas != null)
            {
                var buscarFacturaciones = (from encabezado in context.encab_documento
                                           join tpDoc in context.tp_doc_registros
                                               on encabezado.tipo equals tpDoc.tpdoc_id
                                           join bodega in context.bodega_concesionario
                                               on encabezado.bodega equals bodega.id
                                           join tpDocRegistros in context.tp_doc_registros
                                               on encabezado.tipo equals tpDocRegistros.tpdoc_id
                                           join pedido in context.vpedido
                                               on encabezado.pedido equals pedido.id
                                           join t in context.icb_terceros
                                               on encabezado.nit equals t.tercero_id
                                           where encabezado.tipo == 4
                                           select new
                                           {
                                               tpDoc.tpdoc_id,
                                               tpDoc.tpdoc_nombre,
                                               encabezado.numero,
                                               bodega.id,
                                               bodega.bodccs_cod,
                                               bodega.bodccs_nombre,
                                               encabezado.fecha,
                                               numeroPedido = pedido.numero,
                                               cliente = t.prinom_tercero != null
                                                   ? "(" + t.doc_tercero + ") " + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                                     t.apellido_tercero + " " + t.segapellido_tercero
                                                   : "(" + t.doc_tercero + ") " + t.razon_social
                                           }).ToList();

                var facturaciones = buscarFacturaciones.Select(x => new
                {
                    x.tpdoc_id,
                    x.tpdoc_nombre,
                    x.numero,
                    bodegaId = x.id,
                    x.bodccs_cod,
                    x.bodccs_nombre,
                    fecha = x.fecha.ToString("dd/MM/yyyy"),
                    x.numeroPedido,
                    x.cliente
                });
                return Json(facturaciones, JsonRequestBehavior.AllowGet);
            }
            else
            {
                // Significa que el rol no tiene permiso de ver todas las facturaciones de todas las bodegas, por tantos se busca las bodegas que el usuario actual tiene asignadas
                List<int> lista = new List<int>();
                int usuarioId = Convert.ToInt32(Session["user_usuarioid"]);
                List<bodega_usuario> buscarBodegasUsuario = context.bodega_usuario.Where(x => x.id_usuario == usuarioId).ToList();
                foreach (bodega_usuario item in buscarBodegasUsuario)
                {
                    lista.Add(item.id_bodega);
                }

                var buscarFacturasEnBodegas = (from encabezado in context.encab_documento
                                               join tpDoc in context.tp_doc_registros
                                                   on encabezado.tipo equals tpDoc.tpdoc_id
                                               join bodega in context.bodega_concesionario
                                                   on encabezado.bodega equals bodega.id
                                               join tpDocRegistros in context.tp_doc_registros
                                                   on encabezado.tipo equals tpDocRegistros.tpdoc_id
                                               join pedido in context.vpedido
                                                   on encabezado.pedido equals pedido.id
                                               join t in context.icb_terceros
                                                   on encabezado.nit equals t.tercero_id
                                               where encabezado.tipo == 4 && lista.Contains(encabezado.bodega)
                                               select new
                                               {
                                                   tpDoc.tpdoc_id,
                                                   tpDoc.tpdoc_nombre,
                                                   encabezado.numero,
                                                   encabezado.fecha,
                                                   bodega.id,
                                                   bodega.bodccs_cod,
                                                   bodega.bodccs_nombre,
                                                   numeroPedido = pedido.numero,
                                                   cliente = t.prinom_tercero != null
                                                       ? "(" + t.doc_tercero + ") " + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                                         t.apellido_tercero + " " + t.segapellido_tercero
                                                       : "(" + t.doc_tercero + ") " + t.razon_social
                                               }).ToList();
                var facturaciones = buscarFacturasEnBodegas.Select(x => new
                {
                    x.tpdoc_id,
                    x.tpdoc_nombre,
                    x.numero,
                    bodegaId = x.id,
                    x.bodccs_cod,
                    x.bodccs_nombre,
                    fecha = x.fecha.ToString("dd/MM/yyyy"),
                    x.numeroPedido,
                    x.cliente
                });
                return Json(facturaciones, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult BuscarPlanMayor(string planMayor, string docCliente, int? idBodega, int? id_tp_documento)
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            var buscarPlanMayor = (from vehiculo in context.icb_vehiculo
                                   join modelo in context.modelo_vehiculo
                                       on vehiculo.modvh_id equals modelo.modvh_codigo
                                   join anioModelo in context.anio_modelo
                                       on new { a = vehiculo.modvh_id, b = vehiculo.anio_vh } equals new
                                       { a = anioModelo.codigo_modelo, b = anioModelo.anio }
                                   join inventarioHoy in context.vw_inventario_hoy
                                       on vehiculo.plan_mayor equals inventarioHoy.ref_codigo
                                   join marca in context.marca_vehiculo
                                       on modelo.mar_vh_id equals marca.marcvh_id
                                   join color in context.color_vehiculo
                                       on vehiculo.colvh_id equals color.colvh_id
                                   join tpCaja in context.tpcaja_vehiculo
                                       on modelo.tipocaja equals tpCaja.tpcaj_id into a
                                   from tpCaja in a.DefaultIfEmpty()
                                   join motor in context.tpmotor_vehiculo
                                       on modelo.combustible equals motor.tpmot_id into b
                                   from motor in b.DefaultIfEmpty()
                                   join pedido in context.vpedido
                                       on vehiculo.plan_mayor equals pedido.planmayor
                                   join carroceria in context.tipo_carroceria
                                       on pedido.tipo_carroceria equals carroceria.idcarroceria into c
                                   from carroceria in c.DefaultIfEmpty()
                                   join clase in context.vclase
                                       on modelo.clase equals clase.clase_id into d
                                   from clase in d.DefaultIfEmpty()
                                   join servicio in context.tpservicio_vehiculo
                                       on vehiculo.tiposervicio equals servicio.tpserv_id into e
                                   from servicio in e.DefaultIfEmpty()
                                   join ciudad in context.nom_ciudad
                                       on vehiculo.ciudadplaca equals ciudad.ciu_id into f
                                   from ciudad in f.DefaultIfEmpty()
                                   where vehiculo.plan_mayor == planMayor
                                         && inventarioHoy.bodega == bodegaActual
                                   select new
                                   {
                                       vehiculo.plan_mayor,
                                       modelo.cilindraje,
                                       modelo.capacidad,
                                       modelo.modvh_nombre,
                                       anioModelo.costosiniva,
                                       anioModelo.impuesto_consumo,
                                       anioModelo.precio,
                                       anioModelo.porcentaje_iva,
                                       inventarioHoy.stock,
                                       numeroMotor = vehiculo.nummot_vh,
                                       marcvh_nombre = marca.marcvh_nombre != null ? marca.marcvh_nombre : "",
                                       vin = vehiculo.vin != null ? vehiculo.vin : "",
                                       colvh_nombre = color.colvh_nombre != null ? color.colvh_nombre : "",
                                       tpcaj_nombre = tpCaja.tpcaj_nombre != null ? tpCaja.tpcaj_nombre : "",
                                       combustible = modelo.combustible != null ? modelo.combustible : 0,
                                       tpmot_nombre = motor.tpmot_nombre != null ? motor.tpmot_nombre : "",
                                       carroceria = carroceria.descripcion != null ? carroceria.descripcion : "",
                                       carroceriaVh = carroceria.descripcion != null ? carroceria.descripcion : "",
                                       clase = clase.nombre != null ? clase.nombre : "",
                                       servicio = pedido.servicio != null ? context.tpservicio_vehiculo.Where(x => x.tpserv_id == pedido.servicio).FirstOrDefault().tpserv_nombre : "",
                                       anio_vh = vehiculo.anio_vh,
                                       nummanf_vh = vehiculo.nummanf_vh != null ? vehiculo.nummanf_vh : "",
                                       fecentman_vh = vehiculo.fecentman_vh != null ? vehiculo.fecentman_vh.ToString() : "",
                                       ciu_nombre = ciudad.ciu_nombre != null ? ciudad.ciu_nombre : ""
                                   }).FirstOrDefault();
            if (buscarPlanMayor != null)
            {
                if (buscarPlanMayor.stock > 0)
                {
                    var buscarCliente = (from cliente in context.tercero_cliente
                                         join tercero in context.icb_terceros
                                             on cliente.tercero_id equals tercero.tercero_id
                                         where tercero.doc_tercero == docCliente
                                         select new
                                         {
                                             tercero.tercero_id
                                         }).FirstOrDefault();
                    if (buscarCliente != null)
                    {
                        bool actividadEconomicaValido = false;
                        tp_doc_registros buscarTipoDocRegistro =
                            context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == id_tp_documento);
                        var buscarDatosCliente = (from tercero in context.icb_terceros
                                                      //join cliente in context.tercero_cliente
                                                      //    on tercero.tercero_id equals cliente.tercero_id
                                                  join acteco in context.acteco_tercero
                                                      on tercero.id_acteco equals acteco.acteco_id
                                                  join bodega in context.terceros_bod_ica
                                                      on acteco.acteco_id equals bodega.idcodica into ps
                                                  from bodega in ps.DefaultIfEmpty()
                                                  join regimen in context.tpregimen_tercero
                                                      on tercero.tpregimen_id equals regimen.tpregimen_id into ps2
                                                  from regimen in ps2.DefaultIfEmpty()
                                                  where tercero.tercero_id == buscarCliente.tercero_id
                                                  select new
                                                  {
                                                      actividadEconomica_id = tercero.id_acteco,
                                                      acteco.acteco_nombre,
                                                      tarifaPorBodega =
                                                          bodega != null ? bodega.porcentaje : 0,
                                                      actecoTarifa = acteco.tarifa,
                                                      regimen_id = regimen != null ? regimen.tpregimen_id : 0
                                                  }).FirstOrDefault();
                        decimal porcentajeRetIca = 0;
                        decimal porcentajeRetIva = 0;
                        decimal porcentajeRetencion = 0;
                        decimal valorRetIca = 0;
                        decimal valorRetIva = 0;
                        decimal valorRetencion = 0;
                        if (buscarDatosCliente != null)
                        {
                            if (buscarDatosCliente.tarifaPorBodega != 0)
                            {
                                actividadEconomicaValido = true;
                                porcentajeRetIca = buscarDatosCliente.tarifaPorBodega;
                            }
                            else if (buscarDatosCliente.actecoTarifa != 0)
                            {
                                actividadEconomicaValido = true;
                                porcentajeRetIca = buscarDatosCliente.actecoTarifa;
                            }
                        }

                        int regimen_proveedor = buscarDatosCliente != null ? buscarDatosCliente.regimen_id : 0;
                        perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                            x.bodega == idBodega && x.sw == buscarTipoDocRegistro.sw &&
                            x.tipo_regimenid == regimen_proveedor);
                        if (buscarPerfilTributario != null)
                        {
                            if (buscarTipoDocRegistro != null)
                            {
                                if (buscarPerfilTributario.retfuente == "A")
                                {
                                    if (buscarTipoDocRegistro.baseretencion <= buscarPlanMayor.costosiniva)
                                    {
                                        porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                        valorRetencion = buscarPlanMayor.costosiniva * porcentajeRetencion / 100;
                                    }
                                }

                                if (buscarPerfilTributario.retiva == "A")
                                {
                                    if (buscarTipoDocRegistro.baseiva <= buscarPlanMayor.costosiniva)
                                    {
                                        porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                        valorRetIva = buscarPlanMayor.costosiniva * buscarPlanMayor.porcentaje_iva /
                                                      100 * porcentajeRetIva / 100;
                                    }
                                }

                                if (buscarPerfilTributario.retica == "A")
                                {
                                    if (!actividadEconomicaValido)
                                    {
                                        if (buscarTipoDocRegistro.baseica <= buscarPlanMayor.costosiniva)
                                        {
                                            porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                            valorRetIca = buscarPlanMayor.costosiniva * porcentajeRetIca / 1000;
                                        }
                                    }
                                    else
                                    {
                                        valorRetIca = buscarPlanMayor.costosiniva * porcentajeRetIca / 1000;
                                    }
                                }
                            }

                            valorRetencion = Math.Round(valorRetencion);
                            valorRetIva = Math.Round(valorRetIva);
                            valorRetIca = Math.Round(valorRetIca);
                            return Json(
                                new
                                {
                                    error = false,
                                    error_message = "",
                                    buscarPlanMayor,
                                    porcentajeRetencion,
                                    porcentajeRetIva,
                                    porcentajeRetIca,
                                    valorRetencion,
                                    valorRetIva,
                                    valorRetIca
                                }, JsonRequestBehavior.AllowGet);
                        }

                        return Json(
                            new
                            {
                                error = true,
                                error_message = "No se ha encontrado un perfil tributario asociado a la bodega"
                            }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { error = true, error_message = "El nit del cliente no es valido" },
                        JsonRequestBehavior.AllowGet);
                }

                return Json(
                    new
                    {
                        error = true,
                        error_message = "El plan mayor se encuentra en stock cero para la bodega actual"
                    }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { error = true, error_message = "El plan mayor no se encontro para la bodega actual" },
                JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDatosPedido(int? idPedido, int? idBodega)
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            //var pedidoID = context.vpedido.FirstOrDefault(x => x.numero == idPedido).id;
            int? numeroPedido = context.vpedido.FirstOrDefault(x => x.id == idPedido).numero;

            var buscarPedido = (from pedido in context.vpedido
                                join tercero in context.icb_terceros
                                    on pedido.nit equals tercero.tercero_id
                                join cliente in context.tercero_cliente
                                    on tercero.tercero_id equals cliente.tercero_id
                                join formaPago in context.fpago_tercero
                                    on cliente.cod_pago_id equals formaPago.fpago_id into ps
                                from formaPago in ps.DefaultIfEmpty()
                                join modelo in context.modelo_vehiculo
                                    on pedido.modelo equals modelo.modvh_codigo into mod
                                from modelo in mod.DefaultIfEmpty()
                                join inventarioHoy in context.vw_inventario_hoy
                                    on new { var1 = pedido.planmayor, pedido.bodega } equals new { var1 = inventarioHoy.ref_codigo, inventarioHoy.bodega } into hoy
                                from inventarioHoy in hoy.DefaultIfEmpty()
                                join bodega in context.bodega_concesionario
                                    on inventarioHoy.bodega equals bodega.id into bod
                                from bodega in bod.DefaultIfEmpty()
                                where pedido.numero == numeroPedido && pedido.bodega == idBodega
                                select new
                                {
                                    pedido.numero,
                                    pedido.planmayor,
                                    pedido.facturado,
                                    tercero.tercero_id,
                                    cod_pago_id = cliente.cod_pago_id != null ? cliente.cod_pago_id.ToString() : "",
                                    formaPago.fpago_nombre,
                                    pedido.vendedor,
                                    tercero.doc_tercero,
                                    nom_tercero = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                  tercero.apellido_tercero + " " + tercero.segapellido_tercero,
                                    razon_social = tercero.razon_social != null ? tercero.razon_social + " " : "",
                                    tercero.celular_tercero,
                                    direc_tercero = (from direccion in context.terceros_direcciones
                                                     join ciudad in context.nom_ciudad
                                                         on direccion.ciudad equals ciudad.ciu_id
                                                     where direccion.idtercero == tercero.tercero_id
                                                     orderby direccion.id descending
                                                     select new
                                                     {
                                                         direccion = (direccion.direccion != null ? direccion.direccion : "") + "(" +
                                                                     (ciudad.ciu_nombre != null ? ciudad.ciu_nombre : "") + ")"
                                                     }).FirstOrDefault(),
                                    modelo.modvh_nombre,
                                    pedido.valor_unitario,
                                    pedido.porcentaje_iva,
                                    pedido.porcentaje_impoconsumo,
                                    pedido.vrtotal,
                                    //bodega = inventarioHoy != null ? inventarioHoy.bodega.ToString() : null,
                                    bodega = bodega.id,
                                    bodega.bodccs_nombre,
                                    stock = inventarioHoy != null ? inventarioHoy.stock : 0
                                }).FirstOrDefault();

            var cambioVehiculo = (from c in context.vcambiovehiculo
                                  join p in context.vpedido
                                      on c.idpedido equals p.id
                                  join m in context.marca_vehiculo
                                      on c.idmarca equals m.marcvh_id
                                  join a in context.anio_modelo
                                      on c.idanomodelo equals a.anio_modelo_id
                                  join v in context.modelo_vehiculo
                                      on a.codigo_modelo equals v.modvh_codigo
                                  where c.idpedido == idPedido
                                  orderby c.id descending
                                  select new
                                  {
                                      m.marcvh_nombre,
                                      v.modvh_nombre,
                                      a.anio,
                                      c.motivo
                                  }).FirstOrDefault();

            if (buscarPedido != null)
            {
                if (buscarPedido.planmayor == null)
                {
                    return Json(new { error = true, error_message = "El pedido no tiene asignado un plan mayor" },
                        JsonRequestBehavior.AllowGet);
                }

                if (buscarPedido.stock > 0)
                {
                    //if (buscarPedido.bodega == bodegaActual.ToString())
                    if (buscarPedido.bodega == bodegaActual)
                    {
                        if (cambioVehiculo != null)
                        {
                            return Json(new { error = false, buscarPedido, cambio = cambioVehiculo },
                                JsonRequestBehavior.AllowGet);
                        }

                        return Json(new { error = false, buscarPedido, cambio = 0 }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(
                        new
                        {
                            error = true,
                            error_message = "La bodega del pedido no es la misma a la bodega actual donde inicio sesion"
                        }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { error = true, error_message = "El pedido no tiene stock disponible" },
                    JsonRequestBehavior.AllowGet);
            }

            return Json(
                new
                {
                    error = true,
                    error_message = "No se ha encontrado el numero del pedido para la bodega seleccionada"
                }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarTercero(string documento)
        {
            var buscarTercero = (from tercero in context.icb_terceros
                                 join cliente in context.tercero_cliente
                                     on tercero.tercero_id equals cliente.tercero_id
                                 join formaPago in context.fpago_tercero
                                     on cliente.cod_pago_id equals formaPago.fpago_id into ps
                                 from formaPago in ps.DefaultIfEmpty()
                                 where tercero.doc_tercero == documento
                                 select new
                                 {
                                     tercero.tercero_id,
                                     tercero.doc_tercero,
                                     nom_tercero = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                   tercero.apellido_tercero + " " + tercero.segapellido_tercero,
                                     razon_social = tercero.razon_social != null ? tercero.razon_social + " " : "",
                                     direc_tercero = (from direccion in context.terceros_direcciones
                                                      join ciudad in context.nom_ciudad
                                                          on direccion.ciudad equals ciudad.ciu_id
                                                      where direccion.idtercero == tercero.tercero_id
                                                      orderby direccion.id descending
                                                      select new
                                                      {
                                                          direccion.direccion,
                                                          ciudad.ciu_nombre
                                                      }).FirstOrDefault(),
                                     tercero.celular_tercero,
                                     cod_pago_id = cliente.cod_pago_id != null ? cliente.cod_pago_id.ToString() : "",
                                     formaPago.fpago_nombre
                                 }).FirstOrDefault();
            if (buscarTercero != null)
            {
                return Json(buscarTercero, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GenerarFactura(int? cabeza, int? numero, int? tpDoc, int? bodega)
        {
            //if (numero == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            //busco el documento
            encab_documento tipodocabeza = context.encab_documento.Where(d => d.idencabezado == cabeza).FirstOrDefault();
            tpDoc = tpDoc != null ? tpDoc.Value : tipodocabeza.tp_doc_registros.tpdoc_id;
            bodega = bodega != null ? bodega.Value : tipodocabeza.bodega;
            ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
            icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
            tp_doc_registros buscarTipoDocRegistro = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == tpDoc);
            numeroConsecutivoAux = gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro, tpDoc ?? 0, bodega ?? 0);
            tpDoc = tpDoc != null ? tpDoc : 0;
            bodega = bodega != null ? bodega : 0;
            grupoconsecutivos grupoConsecutivo =
                context.grupoconsecutivos.FirstOrDefault(x => x.documento_id == tpDoc && x.bodega_id == bodega);
            int grupoConsecutivo2 = grupoConsecutivo != null ? grupoConsecutivo.grupo : 0;
            int id_encab = cabeza != null ? cabeza.Value : 0;

            var buscarFactura = (from encabezado in context.encab_documento
                                 join tipoDocumento in context.tp_doc_registros
                                     on encabezado.tipo equals tipoDocumento.tpdoc_id
                                 join tercero in context.icb_terceros
                                     on encabezado.nit equals tercero.tercero_id
                                 join ciudad in context.nom_ciudad
                                     on tercero.ciu_id equals ciudad.ciu_id into ciu
                                 from ciudad in ciu.DefaultIfEmpty()
                                 join vehiculo in context.icb_vehiculo
                                     on encabezado.documento equals vehiculo.plan_mayor into vh
                                 from vehiculo in vh.DefaultIfEmpty()
                                 join color in context.color_vehiculo
                                     on vehiculo.colvh_id equals color.colvh_id into col
                                 from color in col.DefaultIfEmpty()
                                 join modelo in context.modelo_vehiculo
                                     on vehiculo.modvh_id equals modelo.modvh_codigo into mod
                                 from modelo in mod.DefaultIfEmpty()
                                 join marca in context.marca_vehiculo
                                     on modelo.mar_vh_id equals marca.marcvh_id into mar
                                 from marca in mar.DefaultIfEmpty()
                                 join vendedor in context.users
                                     on encabezado.vendedor equals vendedor.user_id into ven
                                 from vendedor in ven.DefaultIfEmpty()
                                 where encabezado.idencabezado == id_encab
                                 select new
                                 {
                                     encabezado.pedido,
                                     encabezado.valor_total,
                                     encabezado.valor_mercancia,
                                     encabezado.iva,
                                     encabezado.impoconsumo,
                                     encabezado.numero,
                                     encabezado.bodega,
                                     encabezado.tipo,
                                     encabezado.nit,
                                     tipoDocumento.prefijo,
                                     tercero.doc_tercero,
                                     tercero.prinom_tercero,
                                     tercero.segnom_tercero,
                                     tercero.apellido_tercero,
                                     tercero.segapellido_tercero,
                                     direc_tercero = (from direccion in context.terceros_direcciones
                                                      join ciudad in context.nom_ciudad
                                                          on direccion.ciudad equals ciudad.ciu_id
                                                      where direccion.idtercero == tercero.tercero_id
                                                      orderby direccion.id descending
                                                      select new
                                                      {
                                                          direccion.id,
                                                          direccion = direccion.direccion != null
                                                              ? direccion.direccion + " (" + ciudad.ciu_nombre + ")"
                                                              : ""
                                                      }).FirstOrDefault(),
                                     clienteuno = (from pedido in context.vpedido
                                                   join encab in context.encab_documento
                                                   on pedido.nit equals encab.nit
                                                   join tercero in context.icb_terceros
                                                   on pedido.nit2 equals tercero.tercero_id
                                                   where encab.idencabezado == cabeza
                                                   select new
                                                   {
                                                       tercero.tercero_id,
                                                       nombre = tercero.prinom_tercero != null ? tercero.prinom_tercero : "",
                                                       apellido = tercero.apellido_tercero != null ? tercero.apellido_tercero : "",
                                                       cedula = tercero.doc_tercero
                                                   }).FirstOrDefault(),
                                     clientedos = (from pedido in context.vpedido
                                                   join encab in context.encab_documento
                                                   on pedido.nit equals encab.nit
                                                   join tercero in context.icb_terceros
                                                   on pedido.nit3 equals tercero.tercero_id
                                                   where encab.idencabezado == cabeza
                                                   select new
                                                   {
                                                       tercero.tercero_id,
                                                       nombre = tercero.prinom_tercero != null ? tercero.prinom_tercero : "",
                                                       apellido = tercero.apellido_tercero != null ? tercero.apellido_tercero : "",
                                                       cedula = tercero.doc_tercero
                                                   }).FirstOrDefault(),
                                     clientetres = (from pedido in context.vpedido
                                                    join encab in context.encab_documento
                                                    on pedido.nit equals encab.nit
                                                    join tercero in context.icb_terceros
                                                    on pedido.nit4 equals tercero.tercero_id
                                                    where encab.idencabezado == cabeza
                                                    select new
                                                    {
                                                        tercero.tercero_id,
                                                        nombre = tercero.prinom_tercero != null ? tercero.prinom_tercero : "",
                                                        apellido = tercero.apellido_tercero != null ? tercero.apellido_tercero : "",
                                                        cedula = tercero.doc_tercero
                                                    }).FirstOrDefault(),
                                     clientecuatro = (from pedido in context.vpedido
                                                      join encab in context.encab_documento
                                                      on pedido.nit equals encab.nit
                                                      join tercero in context.icb_terceros
                                                      on pedido.nit5 equals tercero.tercero_id
                                                      where encab.idencabezado == cabeza
                                                      select new
                                                      {
                                                          tercero.tercero_id,
                                                          nombre = tercero.prinom_tercero != null ? tercero.prinom_tercero : "",
                                                          apellido = tercero.apellido_tercero != null ? tercero.apellido_tercero : "",
                                                          cedula = tercero.doc_tercero
                                                      }).FirstOrDefault(),
                                     asegurado = (from pedido in context.vpedido
                                                  join encab in context.encab_documento
                                                  on pedido.nit equals encab.nit
                                                  join tercero in context.icb_terceros
                                                  on pedido.nit_asegurado equals tercero.tercero_id
                                                  where encab.idencabezado == cabeza
                                                  select new
                                                  {
                                                      tercero.tercero_id,
                                                      nombre = tercero.prinom_tercero != null ? tercero.prinom_tercero : "",
                                                      apellido = tercero.apellido_tercero != null ? tercero.apellido_tercero : "",
                                                      cedula = tercero.doc_tercero
                                                  }).FirstOrDefault(),
                                     nit_prenda = (from pedido in context.vpedido
                                                   join encabezadoPedido in context.encab_documento
                                                       on pedido.id equals encabezadoPedido.id_pedido_vehiculo
                                                   join unidadFinanciera in context.icb_unidad_financiera
                                                       on pedido.nit_prenda equals unidadFinanciera.financiera_id
                                                   where encabezadoPedido.idencabezado == encabezado.idencabezado
                                                   select new
                                                   {
                                                       unidadFinanciera.financiera_nombre
                                                   }).FirstOrDefault(),
                                     numeroResolucion = (from resolucion in context.resolucionfactura
                                                         where resolucion.tipodoc == encabezado.tipo && resolucion.grupo == grupoConsecutivo2
                                                         select new
                                                         {
                                                             resolucion.resolucion,
                                                             resolucion.fechaini,
                                                             resolucion.consecini,
                                                             resolucion.consecfin
                                                         }).FirstOrDefault(),
                                     tercero.celular_tercero,
                                     tercero.telf_tercero,
                                     vendedor.user_id,
                                     vendedor.user_nombre,
                                     vendedor.user_apellido,
                                     ciudad.ciu_nombre,
                                     plan_mayor = vehiculo.plan_mayor != null ? vehiculo.plan_mayor : "",
                                     plac_vh = vehiculo.plac_vh != null ? vehiculo.plac_vh : "",
                                     nummot_vh = vehiculo.nummot_vh != null ? vehiculo.nummot_vh : "",
                                     vin = vehiculo.vin != null ? vehiculo.vin : "",
                                     vehiculo.fecentman_vh,
                                     //fecentman_vh= vehiculo.fecentman_vh != null ? vehiculo.fecentman_vh.Value.ToString("yyyy/MM/dd") : "",
                                     anio_vh = vehiculo.anio_vh,
                                     icbvh_id = vehiculo.icbvh_id,
                                     modvh_nombre = modelo.modvh_nombre != null ? modelo.modvh_nombre : "",
                                     marcvh_nombre = marca.marcvh_nombre != null ? marca.marcvh_nombre : "",
                                     colvh_nombre = color.colvh_nombre != null ? color.colvh_nombre : ""
                                 }).FirstOrDefault();

            //var bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
            FacturaVentaModel modeloFactura = new FacturaVentaModel();
            if (buscarFactura != null)
            {
                modeloFactura.FacturadoA = buscarFactura.prinom_tercero + " " + buscarFactura.segnom_tercero + " " +
                                           buscarFactura.apellido_tercero + " " + buscarFactura.segapellido_tercero;
                modeloFactura.Cedula = buscarFactura.doc_tercero;
                modeloFactura.Telefono = buscarFactura.telf_tercero;
                modeloFactura.Direccion =
                    buscarFactura.direc_tercero != null ? buscarFactura.direc_tercero.direccion : "";
                modeloFactura.Ciudad = buscarFactura.ciu_nombre;
                modeloFactura.Marca = buscarFactura.marcvh_nombre;
                modeloFactura.Estilo = buscarFactura.modvh_nombre;
                modeloFactura.Color = buscarFactura.colvh_nombre;
                modeloFactura.Placa = buscarFactura.plac_vh;
                modeloFactura.FechaManifiesto = buscarFactura.fecentman_vh != null
                    ? buscarFactura.fecentman_vh.Value.ToShortDateString()
                    : "";
                modeloFactura.Motor = buscarFactura.nummot_vh;
                modeloFactura.Serie = buscarFactura.vin;
                modeloFactura.AnioModelo = buscarFactura.anio_vh;
                modeloFactura.PorcentajeIva = Math.Round((buscarFactura.iva / buscarFactura.valor_mercancia) * 100).ToString();
                modeloFactura.Iva = buscarFactura.iva.ToString("0,0", elGR);
                modeloFactura.ImpuestoConsumo =
                    (buscarFactura.valor_mercancia * buscarFactura.impoconsumo / 100).ToString("0,0", elGR);
                modeloFactura.BaseImp = buscarFactura.valor_mercancia.ToString("0,0", elGR);
                modeloFactura.Subtotal = buscarFactura.valor_mercancia.ToString("0,0", elGR);
                modeloFactura.ValorVenta = buscarFactura.valor_mercancia.ToString("0,0", elGR);
                modeloFactura.TotalFactura = buscarFactura.valor_total.ToString("0,0", elGR);
                modeloFactura.Plan = buscarFactura.pedido.ToString();
                modeloFactura.PrendaAFavorDe =
                    buscarFactura.nit_prenda != null ? buscarFactura.nit_prenda.financiera_nombre : "";
                modeloFactura.NumeroResolucion = buscarFactura.numeroResolucion != null
                    ? buscarFactura.numeroResolucion.resolucion
                    : "";
                modeloFactura.FechaInicialResolucion = buscarFactura.numeroResolucion != null
                    ? buscarFactura.numeroResolucion.fechaini.ToShortDateString()
                    : "";
                modeloFactura.TipoDocumento = buscarFactura.prefijo;
                modeloFactura.ConsecutivoInicial = buscarFactura.numeroResolucion != null
                    ? buscarFactura.numeroResolucion.consecini
                    : 0;
                modeloFactura.ConsecutivoFinal = buscarFactura.numeroResolucion != null
                    ? buscarFactura.numeroResolucion.consecfin
                    : 0;
                modeloFactura.NumeroFactura = buscarFactura.numero;
                modeloFactura.Inventario = buscarFactura.icbvh_id;
                modeloFactura.Vendedor = buscarFactura.user_nombre + ' ' + buscarFactura.user_apellido;
                modeloFactura.ClienteUno = buscarFactura.clienteuno != null ? buscarFactura.clienteuno.nombre + ' ' + buscarFactura.clienteuno.apellido + '-' + buscarFactura.clienteuno.cedula : "";
                modeloFactura.ClienteDos = buscarFactura.clientedos != null ? buscarFactura.clientedos.nombre + ' ' + buscarFactura.clientedos.apellido + '-' + buscarFactura.clientedos.cedula : "";
                modeloFactura.ClienteTres = buscarFactura.clientetres != null ? buscarFactura.clientetres.nombre + ' ' + buscarFactura.clientetres.apellido + '-' + buscarFactura.clientetres.cedula : "";
                modeloFactura.ClienteCuatro = buscarFactura.clientecuatro != null ? buscarFactura.clientecuatro.nombre + ' ' + buscarFactura.clientecuatro.apellido + '-' + buscarFactura.clientecuatro.cedula : "";
                modeloFactura.Asegurado = buscarFactura.asegurado != null ? buscarFactura.asegurado.nombre + ' ' + buscarFactura.asegurado.apellido : "";
            }

            //modelo.NumeroFactura = (float)numero;

            string root = Server.MapPath("~/Pdf/");
            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
            string path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);

            //var total = buscaPedido.vrtotal ?? 0;
            //CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
            //var formatoNumericoVrTotal = total.ToString("0,0", elGR);
            ViewAsPdf something = new ViewAsPdf("GenerarFactura", modeloFactura);

            return something;
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