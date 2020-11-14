using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class devolucionVentaAutomaticaController : Controller
    {
        /// <summary>
        ///     Contexto BD
        /// </summary>
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: devolucionVentaAutomatica
        public ActionResult Index(int? menu)
        {
            var buscarTipoDocumento = (from tipoDocumento in context.tp_doc_registros
                                       select new
                                       {
                                           tipoDocumento.tpdoc_id,
                                           nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre
                                       }).ToList();
            ViewBag.tipo_documentoDev_manual = new SelectList(buscarTipoDocumento, "tpdoc_id", "nombre");
            ViewBag.tipo_documentoDevAuto = new SelectList(buscarTipoDocumento, "tpdoc_id", "nombre");
            BuscarFavoritos(menu);
            return View();
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

            #region Consulta

            var data = (from e in context.encab_documento
                        join t in context.tp_doc_registros
                            on e.tipo equals t.tpdoc_id into tmp1
                        from t in tmp1.DefaultIfEmpty()
                        join l in context.lineas_documento
                            on e.idencabezado equals l.id_encabezado into tmp2
                        from l in tmp2.DefaultIfEmpty()
                        join pc in context.perfil_contable_documento
                            on e.perfilcontable equals pc.id into tmp3
                        from pc in tmp3.DefaultIfEmpty()
                        join b in context.bodega_concesionario
                            on e.bodega equals b.id into tmp4
                        from b in tmp4.DefaultIfEmpty()
                        join e2 in context.encab_documento
                            on e.prefijo equals e2.idencabezado into tmp5
                        from e2 in tmp5.DefaultIfEmpty()
                        join ter in context.icb_terceros
                            on e.nit equals ter.tercero_id into tmp6
                        from ter in tmp6.DefaultIfEmpty()
                        join cl in context.tercero_cliente
                            on ter.tercero_id equals cl.tercero_id into tmp7
                        from cl in tmp7.DefaultIfEmpty()
                        join p in context.fpago_tercero
                            on cl.cod_pago_id equals p.fpago_id into tmp8
                        from p in tmp8.DefaultIfEmpty()
                        join v in context.icb_vehiculo
                            on l.codigo equals v.plan_mayor into tmp9
                        from v in tmp9.DefaultIfEmpty()
                        join c in context.color_vehiculo
                            on v.colvh_id equals c.colvh_id into tmp10
                        from c in tmp10.DefaultIfEmpty()
                        join mar in context.marca_vehiculo
                            on v.marcvh_id equals mar.marcvh_id into tmp11
                        from mar in tmp11.DefaultIfEmpty()
                        join m in context.modelo_vehiculo
                            on v.modvh_id equals m.modvh_codigo into tmp12
                        from m in tmp12.DefaultIfEmpty()
                        where e.idencabezado == id
                        select new
                        {
                            tipoDev = e.tipo,
                            tipoVen = e2.tipo,
                            numDev = e.numero,
                            numVen = e2.numero,
                            idDev = e.idencabezado,
                            idVen = e2.idencabezado,
                            preDoc = t.prefijo,
                            t.tpdoc_nombre,
                            e.documento,
                            l.codigo,
                            e.nit,
                            ter.doc_tercero,
                            ter.prinom_tercero,
                            ter.segnom_tercero,
                            ter.apellido_tercero,
                            ter.segapellido_tercero,
                            ter.razon_social,
                            ter.telf_tercero,
                            ter.celular_tercero,
                            ter.email_tercero,
                            b.bodccs_nombre,
                            e.fecha,
                            e.valor_total,
                            e.prefijo,
                            mar.marcvh_nombre,
                            m.modvh_nombre,
                            v.anio_vh,
                            c.colvh_nombre,
                            l.valor_unitario,
                            l.porcentaje_iva,
                            e.iva,
                            e.impoconsumo,
                            p.fpago_nombre,
                            pc.descripcion,
                            context.terceros_direcciones.OrderByDescending(x => x.id).FirstOrDefault(x => x.idtercero == e.nit)
                                .direccion
                        }).FirstOrDefault();

            #endregion

            #region datosTercero

            ViewBag.numVenta = data.numVen;
            ViewBag.numDevolucion = data.numDev;
            ViewBag.fecha = data.fecha;
            ViewBag.boodega = data.bodccs_nombre;
            ViewBag.documento = data.preDoc + " - " + data.tpdoc_nombre;
            if (data.razon_social != null)
            {
                ViewBag.cliente = data.razon_social;
            }
            else
            {
                ViewBag.cliente = data.prinom_tercero + " " + data.segnom_tercero + " " + data.apellido_tercero + " " +
                                  data.segapellido_tercero;
            }

            ViewBag.docTercero = data.doc_tercero;
            ViewBag.telTercero = data.telf_tercero;
            ViewBag.direccion = data.direccion;
            ViewBag.celTercero = data.celular_tercero;
            ViewBag.emailTercero = data.email_tercero;
            ViewBag.condicionPago = data.fpago_nombre;
            ViewBag.perfilContable = data.descripcion;

            #endregion

            #region datosVenta

            ViewBag.planMayor = data.codigo;
            ViewBag.marca = data.marcvh_nombre;
            ViewBag.modelo = data.modvh_nombre;
            ViewBag.color = data.colvh_nombre;
            ViewBag.precio = data.valor_unitario;
            ViewBag.porIva = data.porcentaje_iva;
            ViewBag.valIva = data.iva;
            ViewBag.porImpoCon = data.impoconsumo;
            ViewBag.total = data.valor_total;
            ViewBag.anio = data.anio_vh;

            #endregion

            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult BuscarDevolucionesVentas()
        {
            var data = (from e in context.encab_documento
                        join t in context.tp_doc_registros
                            on e.tipo equals t.tpdoc_id
                        join l in context.lineas_documento
                            on e.idencabezado equals l.id_encabezado
                        join b in context.bodega_concesionario
                            on e.bodega equals b.id
                        join e2 in context.encab_documento
                            on e.prefijo equals e2.idencabezado
                        join ter in context.icb_terceros
                            on e.nit equals ter.tercero_id
                        where e.tipo == 19
                        select new
                        {
                            tipoDev = e.tipo,
                            tipoVen = e2.tipo,
                            numDev = e.numero,
                            numVen = e2.numero,
                            idDev = e.idencabezado,
                            idVen = e2.idencabezado,
                            t.tpdoc_nombre,
                            e.documento,
                            l.codigo,
                            e.nit,
                            ter.doc_tercero,
                            ter.prinom_tercero,
                            ter.segnom_tercero,
                            ter.apellido_tercero,
                            ter.segapellido_tercero,
                            ter.razon_social,
                            b.bodccs_nombre,
                            e.fecha,
                            e.valor_total,
                            e.prefijo
                        }).ToList();

            var info = data.Select(x => new
            {
                x.tipoDev,
                x.tipoVen,
                x.tpdoc_nombre,
                x.numDev,
                x.numVen,
                x.idDev,
                x.idVen,
                x.documento,
                x.nit,
                x.doc_tercero,
                nombre = x.razon_social != null
                    ? x.razon_social
                    : x.prinom_tercero + " " + x.segnom_tercero + " " + x.apellido_tercero + " " +
                      x.segapellido_tercero,
                fecha = x.fecha != null ? x.fecha.ToString("yyyy/MM/dd") : "",
                x.valor_total,
                x.bodccs_nombre,
                x.codigo,
                x.prefijo
            }).ToList();

            return Json(info, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDocumentosFiltro(DateTime? desde, DateTime? hasta)
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
                hasta = hasta.GetValueOrDefault().AddDays(1);
            }

            var buscarEncabezados = (from encabezado in context.encab_documento
                                     join tpDocumento in context.tp_doc_registros
                                         on encabezado.tipo equals tpDocumento.tpdoc_id
                                     join cliente in context.icb_terceros
                                         on encabezado.nit equals cliente.tercero_id into cli
                                     from cliente in cli.DefaultIfEmpty()
                                     join bodega in context.bodega_concesionario
                                         on encabezado.bodega equals bodega.id into bod
                                     from bodega in bod.DefaultIfEmpty()
                                     where encabezado.fecha >= desde
                                           && encabezado.fecha <= hasta && tpDocumento.tipo == 2
                                     select new
                                     {
                                         tpDocumento.tpdoc_nombre,
                                         tpDocumento.prefijo,
                                         encabezado.idencabezado,
                                         encabezado.numero,
                                         bodega.id,
                                         bodega.bodccs_cod,
                                         bodega.bodccs_nombre,
                                         cliente.doc_tercero,
                                         encabezado.fecha
                                     }).ToList();
            var listaEncabezados = buscarEncabezados.Select(x => new
            {
                x.tpdoc_nombre,
                x.prefijo,
                x.idencabezado,
                x.numero,
                x.bodccs_cod,
                id_bodega = x.id,
                x.bodccs_nombre,
                x.doc_tercero,
                fecha = x.fecha.ToShortDateString()
            });
            return Json(listaEncabezados, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DevolverFacturaManual(int? id_encabezado, int id_tp_documento, int? idperfil, string nota)
        {
            if (id_encabezado == null || idperfil == null)
            {
                string mensaje = "No existe un numero de encabezado o perfil en esta venta, asegurese que exista";
                return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
            }

            encab_documento buscarEncabezado = context.encab_documento.FirstOrDefault(x => x.idencabezado == id_encabezado);
            lineas_documento buscarLineas = context.lineas_documento.FirstOrDefault(x => x.id_encabezado == id_encabezado);
            if (buscarEncabezado != null)
            {
                // Validacion para saber si el documento ya se le realizo una devolucion
                int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                string castString = buscarEncabezado.numero.ToString();
                encab_documento buscarDevolucion = context.encab_documento.Where(x =>
                        x.prefijo == buscarEncabezado.tipo && x.bodega == bodegaActual && x.documento == castString)
                    .FirstOrDefault();
                if (buscarDevolucion != null)
                {
                    string mensaje =
                        "La factura ya fue devuelta, el numero de devolucion es <span class='label label-default'>" +
                        buscarDevolucion.numero + "</span>";
                    return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
                }
                // Fin validacion para saber si el documento ya se le realizo una devolucion

                long numeroConsecutivo = 0;

                ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
                icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
                tp_doc_registros buscarTipoDocRegistro = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == id_tp_documento);
                numeroConsecutivoAux = gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro, id_tp_documento,
                    buscarEncabezado.bodega);

                grupoconsecutivos grupoConsecutivo = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == id_tp_documento && x.bodega_id == buscarEncabezado.bodega);
                //var alertasFechaOConsecutivo = "";
                if (numeroConsecutivoAux != null)
                {
                    numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                }
                else
                {
                    string mensaje = "No existe un numero consecutivo asignado para este tipo de documento";
                    return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
                }

                bool actividadEconomicaValido = false;
                decimal porcentajeReteICA = 0;
                decimal porcentajeAutorretencion = 0;
                var buscarNitCliente = (from tercero in context.icb_terceros
                                        join cliente in context.tercero_cliente
                                            on tercero.tercero_id equals cliente.tercero_id
                                        join regimen in context.tpregimen_tercero
                                            on cliente.tpregimen_id equals regimen.tpregimen_id into ps2
                                        join acteco in context.acteco_tercero
                                            on cliente.actividadEconomica_id equals acteco.acteco_id into ps3
                                        from acteco in ps3.DefaultIfEmpty()
                                        join bodega in context.terceros_bod_ica
                                            on acteco.acteco_id equals bodega.idcodica into ps
                                        from bodega in ps.DefaultIfEmpty()
                                        from regimen in ps2.DefaultIfEmpty()
                                        where tercero.tercero_id == buscarEncabezado.nit
                                        select new
                                        {
                                            cliente.actividadEconomica_id,
                                            acteco.acteco_nombre,
                                            tarifaPorBodega = bodega != null ? bodega.porcentaje : 0,
                                            actecoTarifa = acteco.tarifa,
                                            actecoAutorretencion = acteco.autorretencion,
                                            regimen_id = regimen.tpregimen_id
                                        }).FirstOrDefault();

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
                int sw_encontrado = buscarTipoDocRegistro != null ? buscarTipoDocRegistro.tpdoc_id : 0;
                perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                    x.bodega == buscarEncabezado.bodega && x.sw == sw_encontrado &&
                    x.tipo_regimenid == regimen_proveedor);

                // Se valida que el valor de credito sea igual al valor de debito para que la ecuacion quede balanceada
                //var parametrosCuentas = context.perfil_cuentas_documento.Where(x => x.id_perfil == modelo.perfilContable).ToList();
                var parametrosCuentasVerificar = (from perfil_cuenta in context.perfil_cuentas_documento
                                                  join nombreParametro in context.paramcontablenombres
                                                      on perfil_cuenta.id_nombre_parametro equals nombreParametro.id
                                                  join cuenta in context.cuenta_puc
                                                      on perfil_cuenta.cuenta equals cuenta.cntpuc_id
                                                  where perfil_cuenta.id_perfil == idperfil
                                                  select new
                                                  {
                                                      perfil_cuenta.id,
                                                      perfil_cuenta.id_nombre_parametro,
                                                      perfil_cuenta.cuenta,
                                                      perfil_cuenta.centro,
                                                      perfil_cuenta.id_perfil,
                                                      nombreParametro.descripcion_parametro,
                                                      cuenta.cntpuc_numero
                                                  }).ToList();

                List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();
                decimal calcularDebito = 0;
                decimal calcularCredito = 0;

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
                            valorDebito = buscarEncabezado.iva;
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
                                        if (buscarTipoDocRegistro.baseretencion <= buscarEncabezado.valor_mercancia)
                                        {
                                            decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                            valorCredito = buscarEncabezado.valor_mercancia * porcentajeRetencion / 100;
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
                                        if (buscarTipoDocRegistro.baseiva <= buscarEncabezado.valor_mercancia *
                                            buscarEncabezado.iva / 100)
                                        {
                                            decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                            valorCredito =
                                                buscarEncabezado.valor_mercancia * buscarEncabezado.iva / 100 *
                                                porcentajeRetIva / 100;
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
                                            if (buscarTipoDocRegistro.baseica <= buscarEncabezado.valor_mercancia)
                                            {
                                                decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                                valorCredito =
                                                    buscarEncabezado.valor_mercancia * porcentajeRetIca / 1000;
                                                CreditoDebito = "Credito";
                                            }
                                        }
                                        else
                                        {
                                            valorCredito = buscarEncabezado.valor_mercancia * porcentajeReteICA / 1000;
                                            CreditoDebito = "Credito";
                                        }
                                    }
                                }
                            }
                        }

                        if (parametro.id_nombre_parametro == 8)
                        {
                            valorDebito = buscarEncabezado.impoconsumo * buscarEncabezado.valor_mercancia / 100;
                            CreditoDebito = "Debito";
                        }

                        if (parametro.id_nombre_parametro == 9)
                        {
                            valorDebito = buscarEncabezado.valor_mercancia;
                            CreditoDebito = "Debito";
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
                                        if (buscarTipoDocRegistro.baseretencion <= buscarEncabezado.valor_mercancia)
                                        {
                                            decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                            restarIcaIvaRetencion +=
                                                buscarEncabezado.valor_mercancia * porcentajeRetencion / 100;
                                        }
                                    }

                                    if (buscarPerfilTributario.retiva == "A")
                                    {
                                        if (buscarTipoDocRegistro.baseiva <= buscarEncabezado.valor_mercancia)
                                        {
                                            decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                            restarIcaIvaRetencion +=
                                                buscarEncabezado.valor_mercancia * buscarEncabezado.iva / 100 *
                                                porcentajeRetIva / 100;
                                        }
                                    }

                                    if (buscarPerfilTributario.retica == "A")
                                    {
                                        if (!actividadEconomicaValido)
                                        {
                                            if (buscarTipoDocRegistro.baseica <= buscarEncabezado.valor_mercancia)
                                            {
                                                decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                                restarIcaIvaRetencion +=
                                                    buscarEncabezado.valor_mercancia * porcentajeRetIca / 1000;
                                            }
                                        }
                                        else
                                        {
                                            restarIcaIvaRetencion +=
                                                buscarEncabezado.valor_mercancia * porcentajeReteICA / 1000;
                                        }
                                    }
                                }
                            }

                            valorCredito = buscarEncabezado.valor_mercancia + buscarEncabezado.iva +
                                           buscarEncabezado.impoconsumo * buscarEncabezado.valor_mercancia / 100 -
                                           restarIcaIvaRetencion;
                            CreditoDebito = "Credito";
                        }

                        if (parametro.id_nombre_parametro == 11)
                        {
                            valorDebito = buscarEncabezado.valor_mercancia;
                            CreditoDebito = "Debito";
                        }

                        if (parametro.id_nombre_parametro == 12)
                        {
                            valorCredito = buscarEncabezado.valor_mercancia;
                            CreditoDebito = "Credito";
                        }

                        if (parametro.id_nombre_parametro == 17)
                        {
                            if (buscarPerfilTributario != null)
                            {
                                if (buscarPerfilTributario.autorretencion == "A")
                                {
                                    valorDebito = buscarEncabezado.valor_mercancia * porcentajeAutorretencion / 100;
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
                                    valorCredito = buscarEncabezado.valor_mercancia * porcentajeAutorretencion / 100;
                                    CreditoDebito = "Credito";
                                }
                            }
                        }

                        valorCredito = Math.Round(valorCredito);
                        valorDebito = Math.Round(valorDebito);
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
                    string mensaje = "El documento se encuentra mal validado";
                    return Json(new { mensaje, valorGuardado = false, docDescuadrado = true, listaDescuadrados },
                        JsonRequestBehavior.AllowGet);
                }
                // Fin de la validacion para el calculo del debito y credito del movimiento contable


                referencias_inven buscarReferenciasInven = context.referencias_inven.FirstOrDefault(x =>
                    x.codigo == buscarEncabezado.documento && x.ano == DateTime.Now.Year &&
                    x.mes == DateTime.Now.Month);
                if (buscarReferenciasInven != null)
                {
                    buscarReferenciasInven.can_ent = 1;
                    buscarReferenciasInven.can_dev_vta = 1;
                    context.Entry(buscarReferenciasInven).State = EntityState.Modified;

                    // Se busca el vehiculo  de la tabla icb_vehiculo, para actualizarle el campo propietario
                    icb_vehiculo buscarVehiculo =
                        context.icb_vehiculo.FirstOrDefault(x => x.plan_mayor == buscarEncabezado.documento);
                    if (buscarVehiculo != null)
                    {
                        buscarVehiculo.propietario = null;
                        buscarVehiculo.fecha_venta = null;
                        context.Entry(buscarVehiculo).State = EntityState.Modified;
                    }

                    bool guardarReferenciasInven = context.SaveChanges() > 0;
                }
                else
                {
                    string mensaje = "El vehiculo buscado no se encuentra disponible en el inventario";
                    return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
                }


                encab_documento crearEncabezado = new encab_documento
                {
                    tipo = id_tp_documento,
                    pedido = buscarEncabezado.pedido,
                    bodega = buscarEncabezado.bodega,
                    fpago_id = buscarEncabezado.fpago_id,
                    nit = buscarEncabezado.nit,
                    numero = numeroConsecutivo,
                    documento = buscarEncabezado.numero.ToString(),
                    fecha = DateTime.Now,
                    fec_creacion = DateTime.Now,
                    notas = nota,
                    vendedor = buscarEncabezado.vendedor,
                    vencimiento = buscarEncabezado.vencimiento,
                    id_pedido_vehiculo = buscarEncabezado.id_pedido_vehiculo,
                    valor_total = buscarEncabezado.valor_total,
                    iva = buscarEncabezado.iva,
                    valor_mercancia = buscarEncabezado.valor_mercancia,
                    impoconsumo = buscarEncabezado.impoconsumo,
                    prefijo = buscarEncabezado.idencabezado
                };
                context.encab_documento.Add(crearEncabezado);
                bool guardarEncabezado = context.SaveChanges() > 0;
                encab_documento buscarUltimoEncabezado =
                    context.encab_documento.OrderByDescending(x => x.idencabezado).FirstOrDefault();

                lineas_documento crearLineas = new lineas_documento
                {
                    codigo = buscarEncabezado.documento,
                    fec_creacion = DateTime.Now,
                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    nit = buscarEncabezado.nit,
                    cantidad = 1,
                    bodega = buscarEncabezado.bodega,
                    seq = 1,
                    porcentaje_iva = (float)buscarLineas.porcentaje_iva,
                    valor_unitario = buscarEncabezado.valor_mercancia,
                    impoconsumo = (float)buscarEncabezado.impoconsumo,
                    estado = true,
                    fec = DateTime.Now,
                    id_encabezado = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0
                };
                context.lineas_documento.Add(crearLineas);


                centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
                icb_terceros terceroValorCero = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
                int idTerceroCero = centroValorCero != null ? Convert.ToInt32(terceroValorCero.tercero_id) : 0;

                List<perfil_cuentas_documento> perfil = context.perfil_cuentas_documento.Where(x => x.id_perfil == idperfil).ToList();
                foreach (perfil_cuentas_documento parametro in perfil)
                {
                    decimal valorCredito = 0;
                    decimal valorDebito = 0;
                    string CreditoDebito = "";
                    int secuencia = 1;
                    cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
                    if (buscarCuenta != null)
                    {
                        if (parametro.id_nombre_parametro == 2)
                        {
                            valorDebito = buscarEncabezado.iva;
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
                                        if (buscarTipoDocRegistro.baseretencion <= buscarEncabezado.valor_mercancia)
                                        {
                                            decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                            valorCredito = buscarEncabezado.valor_mercancia * porcentajeRetencion / 100;
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
                                        if (buscarTipoDocRegistro.baseiva <= buscarEncabezado.valor_mercancia *
                                            buscarEncabezado.iva / 100)
                                        {
                                            decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                            valorCredito =
                                                buscarEncabezado.valor_mercancia * buscarEncabezado.iva / 100 *
                                                porcentajeRetIva / 100;
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
                                            if (buscarTipoDocRegistro.baseica <= buscarEncabezado.valor_mercancia)
                                            {
                                                decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                                valorCredito =
                                                    buscarEncabezado.valor_mercancia * porcentajeRetIca / 1000;
                                                CreditoDebito = "Credito";
                                            }
                                        }
                                        else
                                        {
                                            valorCredito = buscarEncabezado.valor_mercancia * porcentajeReteICA / 1000;
                                            CreditoDebito = "Credito";
                                        }
                                    }
                                }
                            }
                        }

                        if (parametro.id_nombre_parametro == 8)
                        {
                            valorDebito = buscarEncabezado.impoconsumo;
                            CreditoDebito = "Debito";
                        }

                        if (parametro.id_nombre_parametro == 9)
                        {
                            valorDebito = buscarEncabezado.valor_mercancia;
                            CreditoDebito = "Debito";
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
                                        if (buscarTipoDocRegistro.baseretencion <= buscarEncabezado.valor_mercancia)
                                        {
                                            decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                            restarIcaIvaRetencion +=
                                                buscarEncabezado.valor_mercancia * porcentajeRetencion / 100;
                                        }
                                    }

                                    if (buscarPerfilTributario.retiva == "A")
                                    {
                                        if (buscarTipoDocRegistro.baseiva <= buscarEncabezado.valor_mercancia)
                                        {
                                            decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                            restarIcaIvaRetencion +=
                                                buscarEncabezado.valor_mercancia * buscarEncabezado.iva / 100 *
                                                porcentajeRetIva / 100;
                                        }
                                    }

                                    if (buscarPerfilTributario.retica == "A")
                                    {
                                        if (!actividadEconomicaValido)
                                        {
                                            if (buscarTipoDocRegistro.baseica <= buscarEncabezado.valor_mercancia)
                                            {
                                                decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                                restarIcaIvaRetencion +=
                                                    buscarEncabezado.valor_mercancia * porcentajeRetIca / 1000;
                                            }
                                        }
                                        else
                                        {
                                            restarIcaIvaRetencion =
                                                buscarEncabezado.valor_mercancia * porcentajeReteICA / 1000;
                                        }
                                    }
                                }
                            }

                            valorCredito = buscarEncabezado.valor_mercancia + buscarEncabezado.iva +
                                           buscarEncabezado.impoconsumo - restarIcaIvaRetencion;
                            CreditoDebito = "Credito";
                        }

                        if (parametro.id_nombre_parametro == 11)
                        {
                            valorDebito = buscarEncabezado.valor_mercancia;
                            CreditoDebito = "Debito";
                        }

                        if (parametro.id_nombre_parametro == 12)
                        {
                            valorCredito = buscarEncabezado.valor_mercancia;
                            CreditoDebito = "Credito";
                        }

                        if (parametro.id_nombre_parametro == 17)
                        {
                            if (buscarPerfilTributario != null)
                            {
                                if (buscarPerfilTributario.autorretencion == "A")
                                {
                                    valorDebito = buscarEncabezado.valor_mercancia * porcentajeAutorretencion / 100;
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
                                    valorCredito = buscarEncabezado.valor_mercancia * porcentajeAutorretencion / 100;
                                    CreditoDebito = "Credito";
                                }
                            }
                        }

                        mov_contable movNuevo = new mov_contable
                        {
                            id_encab = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            idparametronombre = parametro.id_nombre_parametro,
                            cuenta = parametro.cuenta,
                            centro = parametro.centro,
                            nit = buscarEncabezado.nit,
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
                                    movNuevo.debitoniif = valorDebito;
                                    movNuevo.debito = valorDebito;
                                }

                                if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                {
                                    movNuevo.credito = valorCredito;
                                    movNuevo.creditoniif = valorCredito;
                                }
                            }

                            if (buscarCuenta.concepniff == 4)
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

                            if (buscarCuenta.concepniff == 5)
                            {
                                if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                {
                                    movNuevo.debito = valorDebito;
                                }

                                if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                {
                                    movNuevo.credito = valorCredito;
                                }
                            }
                        }


                        if (buscarCuenta.manejabase)
                        {
                            movNuevo.basecontable = buscarEncabezado.valor_mercancia;
                        }

                        if (buscarCuenta.tercero)
                        {
                            movNuevo.nit = buscarEncabezado.nit;
                        }

                        if (buscarCuenta.documeto)
                        {
                            movNuevo.documento = buscarUltimoEncabezado.numero.ToString();
                        }

                        if (string.IsNullOrEmpty(buscarCuenta.ctareversion))
                        {
                            movNuevo.cuenta = parametro.cuenta;
                        }
                        else
                        {
                            cuenta_puc buscarCuentaReversion =
                                context.cuenta_puc.FirstOrDefault(x => x.cntpuc_numero == buscarCuenta.ctareversion);
                            if (buscarCuentaReversion != null)
                            {
                                movNuevo.cuenta = buscarCuentaReversion.cntpuc_id;
                            }
                        }

                        movNuevo.detalle = "Devolucion Venta vehiculos fact " + buscarUltimoEncabezado.documento;
                        secuencia++;

                        DateTime fechaHoy = DateTime.Now;
                        cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                            x.centro == parametro.centro && x.cuenta == parametro.cuenta && x.nit == movNuevo.nit &&
                            x.ano == fechaHoy.Year && x.mes == fechaHoy.Month);

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


                bool guardarCuenta = context.SaveChanges() > 0;
                if (guardarEncabezado && guardarCuenta)
                {
                    vpedido buscarPedidoParaActualizar =
                        context.vpedido.FirstOrDefault(x => x.numero == buscarEncabezado.id_pedido_vehiculo);
                    if (buscarPedidoParaActualizar != null)
                    {
                        buscarPedidoParaActualizar.facturado = false;
                        buscarPedidoParaActualizar.numfactura = null;
                        context.Entry(buscarPedidoParaActualizar).State = EntityState.Modified;
                    }


                    // Actualiza los numeros consecutivos por documento
                    int grupoId = grupoConsecutivo != null ? grupoConsecutivo.grupo : 0;
                    List<grupoconsecutivos> gruposConsecutivos = context.grupoconsecutivos.Where(x => x.grupo == grupoId).ToList();
                    foreach (grupoconsecutivos grupo in gruposConsecutivos)
                    {
                        icb_doc_consecutivos buscarElemento = context.icb_doc_consecutivos.FirstOrDefault(x =>
                            x.doccons_idtpdoc == 19 && x.doccons_bodega == grupo.bodega_id);
                        buscarElemento.doccons_siguiente = buscarElemento.doccons_siguiente + 1;
                        context.Entry(buscarElemento).State = EntityState.Modified;
                    }

                    context.SaveChanges();
                    string mensaje =
                        "La devolución se ha guardado exitosamente con numero <span class='label label-default'>" +
                        numeroConsecutivo + "</span>";
                    return Json(new { mensaje, valorGuardado = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string mensaje = "Error de conexion con base de datos";
                    return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
                }
            }

            {
                string mensaje = "No se encontro el encabezado de este documento.";
                return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DevolverFacturaAutomatica(int? id_encabezado, int id_tp_documento, string nota)
        {
            encab_documento buscarEncabezado = context.encab_documento.FirstOrDefault(x => x.idencabezado == id_encabezado);
            lineas_documento buscarLineas = context.lineas_documento.FirstOrDefault(x => x.id_encabezado == id_encabezado);
            if (buscarEncabezado != null)
            {
                // Validacion para saber si el documento ya se le realizo una devolucion
                int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                string castString = buscarEncabezado.numero.ToString();
                encab_documento buscarDevolucion = context.encab_documento.Where(x =>
                        x.prefijo == buscarEncabezado.tipo && x.bodega == bodegaActual && x.documento == castString)
                    .FirstOrDefault();
                if (buscarDevolucion != null)
                {
                    string mensaje =
                        "La factura ya fue devuelta, el numero de devolucion es <span class='label label-default'>" +
                        buscarDevolucion.numero + "</span>";
                    return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
                }
                // Fin validacion para saber si el documento ya se le realizo una devolucion

                long numeroConsecutivo = 0;
                ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
                icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
                tp_doc_registros buscarTipoDocRegistro =
                    context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == buscarEncabezado.tipo);
                numeroConsecutivoAux = gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro, id_tp_documento,
                    buscarEncabezado.bodega);
                grupoconsecutivos grupoConsecutivo = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == id_tp_documento && x.bodega_id == buscarEncabezado.bodega);

                if (numeroConsecutivoAux != null)
                {
                    numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                }
                else
                {
                    string mensaje = "No existe un numero consecutivo asignado para este tipo de documento";
                    return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
                }

                // Aqui se termino de validar el consecutivo
                referencias_inven buscarReferenciasInven = context.referencias_inven.FirstOrDefault(x =>
                    x.codigo == buscarEncabezado.documento && x.ano == DateTime.Now.Year &&
                    x.mes == DateTime.Now.Month);
                if (buscarReferenciasInven != null)
                {
                    buscarReferenciasInven.can_ent = 1;
                    buscarReferenciasInven.can_dev_vta = 1;
                    context.Entry(buscarReferenciasInven).State = EntityState.Modified; ///

                    // Se busca el vehiculo de la tabla icb_vehiculo, para actualizar el campo propietario
                    icb_vehiculo buscarVehiculo =
                        context.icb_vehiculo.FirstOrDefault(x => x.plan_mayor == buscarEncabezado.documento);
                    if (buscarVehiculo != null)
                    {
                        buscarVehiculo.propietario = null;
                        buscarVehiculo.fecha_venta = null;
                        context.Entry(buscarVehiculo).State = EntityState.Modified;
                    }

                    bool guardarReferenciasInven = context.SaveChanges() > 0;
                }
                else
                {
                    string mensaje = "El vehiculo buscado con plan mayor " + buscarEncabezado.documento +
                                  " no se encuentra en el inventario o la referencia no coincide con el mes y año actual";
                    return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
                }

                //
                encab_documento crearEncabezado = new encab_documento
                {
                    tipo = id_tp_documento,
                    pedido = buscarEncabezado.pedido,
                    bodega = buscarEncabezado.bodega,
                    fpago_id = buscarEncabezado.fpago_id,
                    nit = buscarEncabezado.nit,
                    numero = numeroConsecutivo,
                    documento = buscarEncabezado.numero.ToString(),
                    fecha = DateTime.Now,
                    fec_creacion = DateTime.Now,
                    vendedor = buscarEncabezado.vendedor,
                    vencimiento = buscarEncabezado.vencimiento,
                    id_pedido_vehiculo = buscarEncabezado.id_pedido_vehiculo,
                    valor_total = buscarEncabezado.valor_total,
                    iva = buscarEncabezado.iva,
                    notas = nota,
                    prefijo = buscarEncabezado.idencabezado,
                    valor_mercancia = buscarEncabezado.valor_mercancia,
                    impoconsumo = buscarEncabezado.impoconsumo
                };
                context.encab_documento.Add(crearEncabezado);
                bool guardarEncabezado = context.SaveChanges() > 0;
                encab_documento buscarUltimoEncabezado =
                    context.encab_documento.OrderByDescending(x => x.idencabezado).FirstOrDefault();

                lineas_documento crearLineas = new lineas_documento
                {
                    //tipo = 19,
                    codigo = buscarEncabezado.documento,
                    fec_creacion = DateTime.Now,
                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    nit = buscarEncabezado.nit,
                    cantidad = 1,
                    bodega = buscarEncabezado.bodega,
                    seq = 1,
                    porcentaje_iva = (float)buscarLineas.porcentaje_iva,
                    valor_unitario = buscarEncabezado.valor_mercancia,
                    impoconsumo = (float)buscarEncabezado.impoconsumo,
                    //numero = numeroConsecutivo,
                    estado = true,
                    fec = DateTime.Now,
                    id_encabezado = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0
                };
                context.lineas_documento.Add(crearLineas);

                int secuencia = 1;
                List<mov_contable> buscarMovimientos = context.mov_contable.Where(x => x.id_encab == buscarEncabezado.idencabezado)
                    .ToList();
                centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
                icb_terceros terceroValorCero = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
                int idTerceroCero = centroValorCero != null ? Convert.ToInt32(terceroValorCero.tercero_id) : 0;

                //var buscarTipoDocRegistro = context.tp_doc_registros.FirstOrDefault(x=>x.tpdoc_id == buscarEncabezado.tipo);
                foreach (mov_contable movimiento in buscarMovimientos)
                {
                    cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == movimiento.cuenta);
                    if (buscarCuenta != null)
                    {
                        mov_contable movNuevo = new mov_contable
                        {
                            id_encab = buscarUltimoEncabezado.idencabezado,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            idparametronombre = movimiento.idparametronombre,
                            centro = movimiento.centro,
                            nit = movimiento.nit,
                            fec = DateTime.Now,
                            seq = secuencia,
                            debito = movimiento.credito,
                            credito = movimiento.debito,
                            documento = buscarEncabezado.numero.ToString()
                        };

                        if (buscarTipoDocRegistro.aplicaniff)
                        {
                            movNuevo.creditoniif = movimiento.creditoniif;
                            movNuevo.debitoniif = movimiento.debitoniif;
                        }

                        if (string.IsNullOrEmpty(buscarCuenta.ctareversion))
                        {
                            movNuevo.cuenta = movimiento.cuenta;
                        }
                        else
                        {
                            cuenta_puc buscarCuentaReversion =
                                context.cuenta_puc.FirstOrDefault(x => x.cntpuc_numero == buscarCuenta.ctareversion);
                            if (buscarCuentaReversion != null)
                            {
                                movNuevo.cuenta = buscarCuentaReversion.cntpuc_id;
                            }
                        }

                        movNuevo.debitoniif = movimiento.creditoniif;
                        movNuevo.creditoniif = movimiento.debitoniif;


                        DateTime fechaHoy = DateTime.Now;
                        cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                            x.centro == movimiento.centro && x.cuenta == movimiento.cuenta && x.nit == movNuevo.nit &&
                            x.ano == fechaHoy.Year && x.mes == fechaHoy.Month);
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
                        secuencia++;
                    }
                }

                bool guardarCuenta = context.SaveChanges() > 0;
                if (guardarEncabezado && guardarCuenta)
                {
                    vpedido buscarPedidoParaActualizar =
                        context.vpedido.FirstOrDefault(x => x.numero == buscarEncabezado.id_pedido_vehiculo);
                    if (buscarPedidoParaActualizar != null)
                    {
                        buscarPedidoParaActualizar.facturado = false;
                        buscarPedidoParaActualizar.numfactura = null;
                        context.Entry(buscarPedidoParaActualizar).State = EntityState.Modified;
                    }

                    // Actualiza los numeros consecutivos por documento
                    int grupoId = grupoConsecutivo != null ? grupoConsecutivo.grupo : 0;
                    List<grupoconsecutivos> gruposConsecutivos = context.grupoconsecutivos.Where(x => x.grupo == grupoId).ToList();
                    foreach (grupoconsecutivos grupo in gruposConsecutivos)
                    {
                        icb_doc_consecutivos buscarElemento = context.icb_doc_consecutivos.FirstOrDefault(x =>
                            x.doccons_idtpdoc == 19 && x.doccons_bodega == grupo.bodega_id);
                        buscarElemento.doccons_siguiente = buscarElemento.doccons_siguiente + 1;
                        context.Entry(buscarElemento).State = EntityState.Modified;
                    }

                    context.SaveChanges();
                    string mensaje = "La devolución se ha guardado exitosamente. ";
                    return Json(new { mensaje, valorGuardado = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string mensaje = "Error de conexion con base de datos";
                    return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
                }
            }

            {
                string mensaje = "No se encontro el encabezado de este documento.";
                return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
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