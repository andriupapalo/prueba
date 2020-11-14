using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
using Newtonsoft.Json;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Homer_MVC.Controllers
{
    public class FacturacionComisionesController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");




        // GET: FacturacionComisiones
        public ActionResult Index(string listaD, decimal? totalDebitos, decimal? totalCreditos, int? menu)
        {
            List<DocumentoDescuadradoModel> listaJson = new List<DocumentoDescuadradoModel>();
            JavaScriptSerializer j = new JavaScriptSerializer();
            if (!string.IsNullOrEmpty(listaD))
            {
                listaJson = JsonConvert.DeserializeObject<List<DocumentoDescuadradoModel>>(listaD);
            }

            ViewBag.financiera = new SelectList(context.icb_unidad_financiera, "financiera_id", "financiera_nombre");
            ViewBag.financiera1 = new SelectList(context.icb_unidad_financiera, "financiera_id", "financiera_nombre");
            ViewBag.fpago_id = new SelectList(context.fpago_tercero, "fpago_id", "fpago_nombre");

            //parametro de sistema facturas Comision Financiweas
            icb_sysparameter param = context.icb_sysparameter.Where(d => d.syspar_cod == "P141").FirstOrDefault();
            int comi = param != null ? Convert.ToInt32(param.syspar_value) : 22;
            //var list = (from t in context.tp_doc_registros
            //            where t.tp_doc_sw.sw == comi
            //            select new
            //            {
            //                id = t.tpdoc_id,
            //                nombre = t.tpdoc_nombre
            //            }).ToList();

            var list = (from t in context.tp_doc_registros
                        join f in context.tp_doc_sw//join
                        on t.tpdoc_id equals f.sw
                        where f.sw == comi
                        select new
                        {
                            id = t.tpdoc_id,
                            nombre = t.tpdoc_nombre
                        }).ToList();


            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in list)
            {
                lista.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }

            ViewBag.tipo = lista;

            /**/

            var buscarDocDevolucion = (from t in context.tp_doc_registros
                                       where t.tipo == 24
                                       select new
                                       {
                                           id = t.tpdoc_id,
                                           nombre = "(" + t.prefijo + ") " + t.tpdoc_nombre
                                       }).ToList();

            ViewBag.tipo = lista;
            
            ViewBag.tipo_d = new SelectList(buscarDocDevolucion, "id", "nombre");

            

            var list2 = (from t in context.perfil_contable_documento
                          select new
                          {
                              t.id,
                              nombre = t.descripcion
                          }).ToList();

            List<SelectListItem> lista2 = new List<SelectListItem>();
            foreach (var item in list2)
            {
                lista2.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }
            ViewBag.perfil = lista2;
            //ViewBag.perfil_d = lista2;

            ViewBag.bodega = new SelectList(context.bodega_concesionario, "id", "bodccs_nombre");
            if (listaJson.Count > 0)
            {
                ViewBag.documentoDescuadrado = listaJson;
            }

            ViewBag.calculoDebito = totalDebitos;
            ViewBag.calculoCredito = totalCreditos;
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult DevolucionComision(int? menu)
        {
            ViewBag.financiera = new SelectList(context.icb_unidad_financiera, "financiera_id", "financiera_nombre");
            var buscarDocDevolucion = (from t in context.tp_doc_registros
                                       where t.tipo == 24
                                       select new
                                       {
                                           id = t.tpdoc_id,
                                           nombre = "(" + t.prefijo + ") " + t.tpdoc_nombre
                                       }).ToList();
            ViewBag.tipo_d = new SelectList(buscarDocDevolucion, "id", "nombre");
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult Ver(int id, int? menu)
        {
            BuscarFavoritos(menu);
            return View(new encab_documento { idencabezado = id });
        }

        public JsonResult BuscarDetallesComisionPorId(int id)
        {
            var buscarEncabezadoSQL = (from encabezado in context.encab_documento
                                       join documento in context.tp_doc_registros
                                           on encabezado.tipo equals documento.tpdoc_id
                                       join bodega in context.bodega_concesionario
                                           on encabezado.bodega equals bodega.id
                                       join pago in context.fpago_tercero
                                           on encabezado.fpago_id equals pago.fpago_id
                                       where encabezado.idencabezado == id
                                       select new
                                       {
                                           documento.tpdoc_nombre,
                                           encabezado.numero,
                                           bodega.bodccs_nombre,
                                           encabezado.fecha,
                                           pago.fpago_nombre,
                                           encabezado.iva
                                       }).FirstOrDefault();

            var buscarEncabezado = new
            {
                buscarEncabezadoSQL.tpdoc_nombre,
                buscarEncabezadoSQL.numero,
                buscarEncabezadoSQL.bodccs_nombre,
                fecha = buscarEncabezadoSQL.fecha.ToShortDateString(),
                buscarEncabezadoSQL.fpago_nombre,
                buscarEncabezadoSQL.iva
            };

            var buscarDetallesComision = (from detalle in context.detalle_comision_financiera
                                          where detalle.encabezado_id == id
                                          select new
                                          {
                                              detalle.descripcion,
                                              Porcen_iva = detalle.Porcen_iva != null ? detalle.Porcen_iva : 0,
                                              porcen_descuento = detalle.porcen_descuento != null ? detalle.porcen_descuento : 0,
                                              valor_unitario = detalle.valor_unitario != null ? detalle.valor_unitario : 0,
                                              valor_total = detalle.valor_total != null ? detalle.valor_total : 0
                                          }).ToList();

            var buscarMovimientosComision = (from movimiento in context.mov_contable
                                             join parametro in context.paramcontablenombres
                                                 on movimiento.idparametronombre equals parametro.id
                                             join cuenta in context.cuenta_puc
                                                 on movimiento.cuenta equals cuenta.cntpuc_id
                                             join centro in context.centro_costo
                                                 on movimiento.centro equals centro.centcst_id
                                             where movimiento.id_encab == id
                                             select new
                                             {
                                                 parametro.descripcion_parametro,
                                                 cuenta = "(" + cuenta.cntpuc_numero + ") " + cuenta.cntpuc_descp,
                                                 centro.centcst_nombre,
                                                 movimiento.debito,
                                                 movimiento.credito
                                             }).ToList();

            return Json(new { buscarEncabezado, buscarDetallesComision, buscarMovimientosComision },
                JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPerfilesYBodegasPorDocumento(int id_documento)
        {
            var buscarPerfiles = (from t in context.perfil_contable_documento
                                  where t.tipo == id_documento
                                  select new
                                  {
                                      t.id,
                                      nombre = t.descripcion
                                  }).ToList();

            var buscarBodegas = (from consecutivos in context.icb_doc_consecutivos
                                 join bodega in context.bodega_concesionario
                                     on consecutivos.doccons_bodega equals bodega.id
                                 where consecutivos.doccons_idtpdoc == id_documento
                                 select new
                                 {
                                     bodega.bodccs_nombre,
                                     bodega.id
                                 }).ToList();

            return Json(new { buscarPerfiles, buscarBodegas }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(encab_documento doc, int? menu)
        {
            string lista_seleccion = Request["lista_seleccion"];
            string[] lista = lista_seleccion.Split(',');
            List<int> listaCreditos = new List<int>();
            foreach (string credit in lista)
            {
                if (!string.IsNullOrEmpty(credit))
                {
                    listaCreditos.Add(Convert.ToInt32(credit));
                }
            }

            ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
            long numeroConsecutivo = 0;
            icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
            tp_doc_registros buscarTipoDocRegistro = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == doc.tipo);
            numeroConsecutivoAux = gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro, doc.tipo, doc.bodega);
            grupoconsecutivos grupoConsecutivo =
                context.grupoconsecutivos.FirstOrDefault(x => x.documento_id == doc.tipo && x.bodega_id == doc.bodega);

            if (numeroConsecutivoAux != null)
            {
                numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
            }
            else
            {
                TempData["mensaje_error"] = "No existe un numero consecutivo asociado a este tipo de documento";
                return RedirectToAction("Index");
            }

            bool actividadEconomicaValido = false;
            decimal porcentajeReteICA = 0;
            decimal porcentajeAutorretencion = 0;
            icb_unidad_financiera buscarFinanciera = context.icb_unidad_financiera.FirstOrDefault(x => x.financiera_id == doc.nit);
            var buscarNitCliente = (from tercero in context.icb_terceros
                                    join cliente in context.tercero_cliente
                                        on tercero.tercero_id equals cliente.tercero_id into ps0
                                    from cliente in ps0.DefaultIfEmpty()
                                    join regimen in context.tpregimen_tercero
                                        on cliente.tpregimen_id equals regimen.tpregimen_id into ps2
                                    join acteco in context.acteco_tercero
                                        on cliente.actividadEconomica_id equals acteco.acteco_id into ps3
                                    from acteco in ps3.DefaultIfEmpty()
                                    join bodega in context.terceros_bod_ica
                                        on acteco.acteco_id equals bodega.bodega into ps
                                    from bodega in ps.DefaultIfEmpty()
                                    from regimen in ps2.DefaultIfEmpty()
                                    where tercero.tercero_id == buscarFinanciera.Nit
                                    select new
                                    {
                                        actividadEconomica_id = cliente != null
                                            ? cliente.actividadEconomica_id != null ? cliente.actividadEconomica_id : 0
                                            : null,
                                        acteco_nombre = acteco != null ? acteco.acteco_nombre != null ? acteco.acteco_nombre : "" : null,
                                        tarifaPorBodega = bodega != null ? bodega.porcentaje : 0,
                                        actecoTarifa = acteco != null ? acteco.tarifa : 0,
                                        actecoAutorretencion = acteco != null ? acteco.autorretencion : 0,
                                        regimen_id = regimen != null ? regimen.tpregimen_id : 0,
                                        tercero.tercero_id,
                                        tarifa = acteco != null ? acteco.tarifa : 0
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

            int bodegaSeleccionada = Convert.ToInt32(Request["bodega"]);
            int perfilSeleccionado = Convert.ToInt32(Request["perfil"]);
            int regimen_proveedor = buscarNitCliente != null ? buscarNitCliente.regimen_id : 0;
            int? sw_encontrado = buscarTipoDocRegistro != null ? buscarTipoDocRegistro.sw : 0;
            perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                x.bodega == bodegaSeleccionada && x.sw == sw_encontrado && x.tipo_regimenid == regimen_proveedor);

            var parametrosCuentasVerificar = (from perfilCuentas in context.perfil_cuentas_documento
                                              join nombreParametro in context.paramcontablenombres
                                                  on perfilCuentas.id_nombre_parametro equals nombreParametro.id
                                              join cuenta in context.cuenta_puc
                                                  on perfilCuentas.cuenta equals cuenta.cntpuc_id
                                              where perfilCuentas.id_perfil == perfilSeleccionado
                                              select new
                                              {
                                                  perfilCuentas.id,
                                                  perfilCuentas.id_nombre_parametro,
                                                  perfilCuentas.cuenta,
                                                  perfilCuentas.centro,
                                                  perfilCuentas.id_perfil,
                                                  nombreParametro.descripcion_parametro,
                                                  cuenta.cntpuc_numero
                                              }).ToList();

            decimal? sumatoriaComisiones = (from credito in context.v_creditos
                                            where listaCreditos.Contains(credito.Id)
                                            select credito.valor_comision).Sum();

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
                        valorCredito = doc.iva * (sumatoriaComisiones ?? 0) / 100;
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
                                    if (buscarTipoDocRegistro.baseretencion <= (sumatoriaComisiones ?? 0))
                                    {
                                        decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                        valorDebito = (sumatoriaComisiones ?? 0) * porcentajeRetencion / 100;
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
                                    if (buscarTipoDocRegistro.baseiva <= (sumatoriaComisiones ?? 0) * doc.iva / 100)
                                    {
                                        decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                        valorDebito = (sumatoriaComisiones ?? 0) * doc.iva / 100 * porcentajeRetIva /
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
                                        if (buscarTipoDocRegistro.baseica <= (sumatoriaComisiones ?? 0))
                                        {
                                            decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                            valorDebito = (sumatoriaComisiones ?? 0) * porcentajeRetIca / 1000;
                                            CreditoDebito = "Debito";
                                        }
                                    }
                                    else
                                    {
                                        valorCredito = (sumatoriaComisiones ?? 0) * porcentajeReteICA / 1000;
                                        valorDebito = (sumatoriaComisiones ?? 0) * porcentajeReteICA / 1000;
                                    }
                                }
                            }
                        }
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
                                    if (buscarTipoDocRegistro.baseretencion <= (sumatoriaComisiones ?? 0))
                                    {
                                        decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                        decimal sdf = (sumatoriaComisiones ?? 0) * porcentajeRetencion / 100;
                                        restarIcaIvaRetencion += (sumatoriaComisiones ?? 0) * porcentajeRetencion / 100;
                                    }
                                }

                                if (buscarPerfilTributario.retiva == "A")
                                {
                                    if (buscarTipoDocRegistro.baseiva <= (sumatoriaComisiones ?? 0))
                                    {
                                        decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                        decimal tt = (sumatoriaComisiones ?? 0) * doc.iva / 100 * porcentajeRetIva / 100;
                                        restarIcaIvaRetencion +=
                                            (sumatoriaComisiones ?? 0) * doc.iva / 100 * porcentajeRetIva / 100;
                                    }
                                }

                                if (buscarPerfilTributario.retica == "A")
                                {
                                    if (!actividadEconomicaValido)
                                    {
                                        if (buscarTipoDocRegistro.baseica <= (sumatoriaComisiones ?? 0))
                                        {
                                            decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                            decimal hh = (sumatoriaComisiones ?? 0) * porcentajeRetIca / 1000;
                                            restarIcaIvaRetencion +=
                                                (sumatoriaComisiones ?? 0) * porcentajeRetIca / 1000;
                                        }
                                    }
                                    else
                                    {
                                        restarIcaIvaRetencion += (sumatoriaComisiones ?? 0) * porcentajeReteICA / 1000;
                                    }
                                }
                            }
                        }

                        valorDebito = (sumatoriaComisiones ?? 0) + doc.iva * (sumatoriaComisiones ?? 0) / 100 -
                                      restarIcaIvaRetencion;
                        CreditoDebito = "Debito";
                    }

                    if (parametro.id_nombre_parametro == 11)
                    {
                        valorCredito = sumatoriaComisiones ?? 0;
                        CreditoDebito = "Credito";
                    }

                    if (parametro.id_nombre_parametro == 17)
                    {
                        if (buscarPerfilTributario != null)
                        {
                            if (buscarPerfilTributario.autorretencion == "A")
                            {
                                valorDebito = (sumatoriaComisiones ?? 0) * porcentajeAutorretencion / 100;
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
                                valorCredito = (sumatoriaComisiones ?? 0) * porcentajeAutorretencion / 100;
                                CreditoDebito = "Credito";
                            }
                        }
                    }

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
                TempData["documento_descuadrado"] = "El documento no tiene los movimientos calculados correctamente";
                ViewBag.documentoDescuadrado = listaDescuadrados;
                ViewBag.calculoDebito = calcularDebito;
                ViewBag.calculoCredito = calcularCredito;
                BuscarFavoritos(menu);
                return RedirectToAction(
                    "Index" /*, new { listaD = jsonList, totalDebitos = totalDebitos, totalCreditos = totalCreditos, menu }*/);
            }
            // Fin de la validacion para el calculo del debito y credito del movimiento contable

            int dias_vencimiento = context.fpago_tercero.Find(doc.fpago_id).dvencimiento ?? 0;
            doc.vencimiento = DateTime.Now.AddDays(dias_vencimiento);
            doc.fecha = DateTime.Now;
            doc.fec_creacion = DateTime.Now;
            doc.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
            doc.estado = true;
            doc.nit = buscarNitCliente.tercero_id;
            doc.numero = numeroConsecutivo;

            if (buscarPerfilTributario != null)
            {
                if (buscarTipoDocRegistro != null)
                {
                    if (buscarPerfilTributario.retfuente == "A")
                    {
                        if (buscarTipoDocRegistro.baseretencion <= (sumatoriaComisiones ?? 0))
                        {
                            decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                            doc.retencion = (sumatoriaComisiones ?? 0) * porcentajeRetencion / 100;
                        }
                    }

                    if (buscarPerfilTributario.retiva == "A")
                    {
                        if (buscarTipoDocRegistro.baseiva <= (sumatoriaComisiones ?? 0))
                        {
                            decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                            doc.retencion_iva = (sumatoriaComisiones ?? 0) * doc.iva / 100 * porcentajeRetIva / 100;
                        }
                    }

                    if (buscarPerfilTributario.retica == "A")
                    {
                        if (!actividadEconomicaValido)
                        {
                            if (buscarTipoDocRegistro.baseica <= (sumatoriaComisiones ?? 0))
                            {
                                decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                doc.retencion_ica += (sumatoriaComisiones ?? 0) * porcentajeRetIca / 1000;
                            }
                        }
                        else
                        {
                            doc.retencion_ica = (sumatoriaComisiones ?? 0) * porcentajeReteICA / 1000;
                        }
                    }
                }
            }

            decimal porcentajeIVA = doc.iva;

            doc.valor_mercancia = sumatoriaComisiones ?? 0;
            doc.valor_total = (sumatoriaComisiones ?? 0) + (sumatoriaComisiones ?? 0) * doc.iva / 100;
            doc.iva = (sumatoriaComisiones ?? 0) * doc.iva / 100;
            doc.tipo_facturadoa = "financiera";
            doc.facturado_a = buscarNitCliente.tercero_id;
            //encabezado.nit = datos.financiera_id;
            //doc.vendedor = datos.asesor_id;

            context.encab_documento.Add(doc);
            context.SaveChanges();
            encab_documento encabezado = context.encab_documento.OrderByDescending(x => x.idencabezado).FirstOrDefault();
            doc.iva = porcentajeIVA;
            int i = 0;
            foreach (int id in listaCreditos)
            {
                i++;
                v_creditos datos = context.v_creditos.Find(id);
                datos.fec_facturacomision = DateTime.Now;
                datos.numfactura = doc.numero;
                context.Entry(datos).State = EntityState.Modified;

                detalle_comision_financiera detalle = new detalle_comision_financiera
                {
                    encabezado_id = encabezado.idencabezado,
                    seq = i,
                    Porcen_iva = Convert.ToInt64(doc.iva),
                    valor_unitario = datos.valor_comision ?? 0
                };
                decimal? valor_coniva = detalle.valor_unitario * doc.iva;
                valor_coniva = valor_coniva / 100;
                detalle.valor_total = datos.valor_comision + valor_coniva;
                string cliente = datos.vinfcredito.icb_terceros.prinom_tercero + " " +
                              datos.vinfcredito.icb_terceros.segnom_tercero + " " +
                              datos.vinfcredito.icb_terceros.apellido_tercero + " " +
                              datos.vinfcredito.icb_terceros.segapellido_tercero;
                detalle.descripcion = "COMISION CREDITO " + cliente + " valor credito $" + datos.vaprobado;
                context.detalle_comision_financiera.Add(detalle);

                int secuencia = 1;
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
                            valorCredito = doc.iva * (sumatoriaComisiones ?? 0) / 100;
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
                                        if (buscarTipoDocRegistro.baseretencion <= (sumatoriaComisiones ?? 0))
                                        {
                                            decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                            valorDebito = (sumatoriaComisiones ?? 0) * porcentajeRetencion / 100;
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
                                        if (buscarTipoDocRegistro.baseiva <= (sumatoriaComisiones ?? 0) * doc.iva / 100)
                                        {
                                            decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                            valorDebito =
                                                (sumatoriaComisiones ?? 0) * doc.iva / 100 * porcentajeRetIva / 100;
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
                                            if (buscarTipoDocRegistro.baseica <= (sumatoriaComisiones ?? 0))
                                            {
                                                decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                                valorDebito = (sumatoriaComisiones ?? 0) * porcentajeRetIca / 1000;
                                                CreditoDebito = "Debito";
                                            }
                                        }
                                        else
                                        {
                                            valorCredito = (sumatoriaComisiones ?? 0) * porcentajeReteICA / 1000;
                                            valorDebito = (sumatoriaComisiones ?? 0) * porcentajeReteICA / 1000;
                                        }
                                    }
                                }
                            }
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
                                        if (buscarTipoDocRegistro.baseretencion <= (sumatoriaComisiones ?? 0))
                                        {
                                            decimal porcentajeRetencion = (decimal)buscarTipoDocRegistro.retencion;
                                            decimal sdf = (sumatoriaComisiones ?? 0) * porcentajeRetencion / 100;
                                            restarIcaIvaRetencion +=
                                                (sumatoriaComisiones ?? 0) * porcentajeRetencion / 100;
                                        }
                                    }

                                    if (buscarPerfilTributario.retiva == "A")
                                    {
                                        if (buscarTipoDocRegistro.baseiva <= (sumatoriaComisiones ?? 0))
                                        {
                                            decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                            decimal tt = (sumatoriaComisiones ?? 0) * doc.iva / 100 * porcentajeRetIva /
                                                     100;
                                            restarIcaIvaRetencion +=
                                                (sumatoriaComisiones ?? 0) * doc.iva / 100 * porcentajeRetIva / 100;
                                        }
                                    }

                                    if (buscarPerfilTributario.retica == "A")
                                    {
                                        if (!actividadEconomicaValido)
                                        {
                                            if (buscarTipoDocRegistro.baseica <= (sumatoriaComisiones ?? 0))
                                            {
                                                decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                                decimal hh = (sumatoriaComisiones ?? 0) * porcentajeRetIca / 1000;
                                                restarIcaIvaRetencion +=
                                                    (sumatoriaComisiones ?? 0) * porcentajeRetIca / 1000;
                                            }
                                        }
                                        else
                                        {
                                            restarIcaIvaRetencion +=
                                                (sumatoriaComisiones ?? 0) * porcentajeReteICA / 1000;
                                        }
                                    }
                                }
                            }

                            valorDebito = (sumatoriaComisiones ?? 0) + doc.iva * (sumatoriaComisiones ?? 0) / 100 -
                                          restarIcaIvaRetencion;
                            CreditoDebito = "Debito";
                        }

                        if (parametro.id_nombre_parametro == 11)
                        {
                            valorCredito = sumatoriaComisiones ?? 0;
                            CreditoDebito = "Credito";
                        }

                        if (parametro.id_nombre_parametro == 17)
                        {
                            if (buscarPerfilTributario != null)
                            {
                                if (buscarPerfilTributario.autorretencion == "A")
                                {
                                    valorDebito = (sumatoriaComisiones ?? 0) * porcentajeAutorretencion / 100;
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
                                    valorCredito = (sumatoriaComisiones ?? 0) * porcentajeAutorretencion / 100;
                                    CreditoDebito = "Credito";
                                }
                            }
                        }

                        mov_contable mov_contable = new mov_contable
                        {
                            id_encab = encabezado.idencabezado,
                            seq = secuencia,
                            fec_creacion = DateTime.Now,
                            idparametronombre = parametro.id_nombre_parametro,
                            cuenta = parametro.cuenta,
                            centro = parametro.centro,
                            nit = buscarFinanciera.Nit ?? 0,
                            fec = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                        };
                        mov_contable.seq = secuencia;
                        mov_contable.debito = valorDebito;
                        mov_contable.credito = valorCredito;

                        if (parametro.id_nombre_parametro == 4)
                        {
                            mov_contable.basecontable = doc.iva * (sumatoriaComisiones ?? 0) / 100;
                        }

                        if (buscarTipoDocRegistro.aplicaniff)
                        {
                            if (buscarCuenta.concepniff == 1 || buscarCuenta.concepniff == 4)
                            {
                                if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                {
                                    mov_contable.debitoniif = valorDebito;
                                }

                                if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                {
                                    mov_contable.creditoniif = valorCredito;
                                }
                            }
                        }

                        if (buscarCuenta.manejabase)
                        {
                            mov_contable.basecontable = sumatoriaComisiones ?? 0;
                        }

                        if (buscarCuenta.tercero)
                        {
                            mov_contable.nit = buscarFinanciera.Nit ?? 0;
                        }

                        if (buscarCuenta.documeto)
                        {
                            mov_contable.documento = encabezado.numero.ToString();
                        }

                        mov_contable.detalle = "Facturacion comisiones numero " + encabezado.documento;
                        secuencia++;

                        DateTime fechaHoy = DateTime.Now;
                        cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                            x.centro == parametro.centro && x.cuenta == parametro.cuenta &&
                            x.nit == buscarFinanciera.Nit && x.ano == fechaHoy.Year && x.mes == fechaHoy.Month);
                        if (buscar_cuentas_valores != null)
                        {
                            buscar_cuentas_valores.debito += mov_contable.debito;
                            buscar_cuentas_valores.credito += mov_contable.credito;
                            buscar_cuentas_valores.debitoniff += mov_contable.debitoniif;
                            buscar_cuentas_valores.creditoniff += mov_contable.creditoniif;
                            context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                        }
                        else
                        {
                            cuentas_valores crearCuentaValor = new cuentas_valores
                            {
                                ano = fechaHoy.Year,
                                mes = fechaHoy.Month,
                                cuenta = mov_contable.cuenta,
                                centro = mov_contable.centro,
                                nit = mov_contable.nit,
                                debito = mov_contable.debito,
                                credito = mov_contable.credito,
                                debitoniff = mov_contable.debitoniif,
                                creditoniff = mov_contable.creditoniif
                            };
                            context.cuentas_valores.Add(crearCuentaValor);
                        }

                        context.mov_contable.Add(mov_contable);
                        context.SaveChanges();
                    }
                }
            }

            // Actualiza los numeros consecutivos por documento
            int grupoId = grupoConsecutivo != null ? grupoConsecutivo.grupo : 0;
            List<grupoconsecutivos> gruposConsecutivos = context.grupoconsecutivos.Where(x => x.grupo == grupoId).ToList();
            foreach (grupoconsecutivos grupo in gruposConsecutivos)
            {
                icb_doc_consecutivos buscarElemento = context.icb_doc_consecutivos.FirstOrDefault(x =>
                    x.doccons_idtpdoc == doc.tipo && x.doccons_bodega == grupo.bodega_id);
                buscarElemento.doccons_siguiente = buscarElemento.doccons_siguiente + 1;
                context.Entry(buscarElemento).State = EntityState.Modified;
            }

            context.SaveChanges();
            ViewBag.numFacturacionCreada = numeroConsecutivo;
            TempData["mensaje"] = numeroConsecutivo;
            //var buscarTerceroCliente = (from tercero in context.icb_terceros
            //                            join proveedor in context.tercero_proveedor
            //                            on tercero.tercero_id equals proveedor.tercero_id
            //                            join tributario in context.perfiltributario
            //                            on proveedor.tpregimen_id equals tributario.tipo_regimenid
            //                            join acteco in context.acteco_tercero
            //                            on proveedor.acteco_id equals acteco.acteco_id into ps
            //                            from acteco in ps.DefaultIfEmpty()
            //                            where tercero.tercero_id == encabezado.nit
            //                            select new
            //                            {
            //                                tercero.tercero_id,
            //                                proveedor.tpregimen_id,
            //                                tributario.retfuente,
            //                                tributario.retiva,
            //                                tributario.retica,
            //                                proveedor.acteco_id,
            //                                acteco.tarifa
            ////////                            }).FirstOrDefault();

            //decimal? retencionAux = 0;
            //decimal? reteIvaAux = 0;
            //decimal? reteIcaAux = 0;
            //if (buscarNitCliente != null)
            //{
            //    var buscarPrefijoDocumento = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == doc.tipo);
            //    if (buscarPrefijoDocumento != null)
            //    {

            //        var baseRetencion = buscarPrefijoDocumento.baseretencion;
            //        if (baseRetencion < encabezado.valor_mercancia)
            //        {
            //            if (buscarNitCliente.retfuente == "A")
            //            {
            //                var porcentajeRetencion = buscarPrefijoDocumento.retiva;
            //                retencionAux = (encabezado.valor_mercancia * (decimal)porcentajeRetencion) / 100;
            //            }
            //        }

            //        var baseIva = buscarPrefijoDocumento.baseiva;
            //        var porcentajeIva = buscarPrefijoDocumento.retiva;
            //        var calcularIva = (encabezado.valor_mercancia * (decimal)porcentajeIva) / 100;
            //        if (baseIva < calcularIva)
            //        {
            //            if (buscarPerfilTributario.retfuente == "A")
            //            {
            //                reteIvaAux = calcularIva;
            //            }
            //        }

            //        var baseIca = buscarPrefijoDocumento.baseica;
            //        if (baseIca < encabezado.valor_mercancia)
            //        {
            //            if (buscarPerfilTributario.retfuente == "A")
            //            {
            //                var bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            //                var buscarActecoPorBodega = (from actecoBodega in context.terceros_bod_ica
            //                                             where actecoBodega.bodega == bodegaActual
            //                                             select new
            //                                             {
            //                                                 actecoBodega.porcentaje
            //                                             }).FirstOrDefault();
            //                decimal tarifaActeco = 0;
            //                if (buscarActecoPorBodega != null)
            //                {
            //                    tarifaActeco = buscarActecoPorBodega.porcentaje;
            //                }
            //                else
            //                {
            //                    if (buscarPerfilTributario.tarifa != null)
            //                    {
            //                        tarifaActeco = buscarPerfilTributario.tarifa;
            //                    }
            //                    else
            //                    {
            //                        tarifaActeco = (decimal)buscarPrefijoDocumento.retica;
            //                    }
            //                }
            //                reteIcaAux = (encabezado.valor_mercancia * tarifaActeco) / 100;
            //            }
            //        }
            //    }
            //}
            //decimal retenciones = retencionAux ?? 0 + reteIvaAux ?? 0 + reteIcaAux ?? 0;

            //encabezado.valor_mercancia = valor_mercancia;
            //encabezado.valor_total = valor_total;
            //encabezado.iva = (valor_total * encabezado.iva) / 100;
            //encabezado.tipo_facturadoa = "financiera";
            //context.Entry(encabezado).State = EntityState.Modified;

            return RedirectToAction("Index", new { id = encabezado.idencabezado, menu });

            ////var regimen_proveedor = buscarNitCliente != null ? buscarNitCliente.regimen_id : 0;
            ////var sw_encontrado = buscarTipoDocRegistro != null ? buscarTipoDocRegistro.sw : 0;
            ////var buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x => x.bodega == modelo.bodega && x.sw == sw_encontrado && x.tipo_regimenid == regimen_proveedor);


            ////movimiento contable
            //var j = 0;
            //var idperfil = Convert.ToInt32(Request["perfil"]);
            //var perfil = context.perfil_cuentas_documento.Where(x => x.id_perfil == idperfil).ToList();
            ////List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();
            //var centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
            //var idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
            //var terceroValorCero = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
            //var idTerceroCero = centroValorCero != null ? Convert.ToInt32(terceroValorCero.tercero_id) : 0;
            //decimal totalDebitos = 0;
            //decimal totalCreditos = 0;

            //foreach (var item in perfil)
            //{
            //    j++;
            //    var cuentas = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == item.cuenta);
            //    var descripcionParametro = context.paramcontablenombres.FirstOrDefault(x => x.id == item.id_nombre_parametro).descripcion_parametro;

            //    mov_contable mov_contable = new mov_contable();
            //    mov_contable.id_encab = encabezado.idencabezado;
            //    mov_contable.seq = j;
            //    mov_contable.idparametronombre = item.id_nombre_parametro;
            //    mov_contable.cuenta = item.cuenta;
            //    mov_contable.centro = item.centro;
            //    if(cuentas.tercero == true)
            //    {
            //        mov_contable.nit = encabezado.nit;
            //    }
            //    mov_contable.fec = DateTime.Now;
            //    //if (cuentas.mov_cnt == "Debito")
            //    //{
            //        if(item.id_nombre_parametro == 10)
            //        {
            //            mov_contable.debito = encabezado.valor_total + retenciones;
            //        }
            //        if(item.id_nombre_parametro == 11)
            //        {
            //            mov_contable.credito = encabezado.valor_mercancia;
            //        }
            //        if(item.id_nombre_parametro == 2)
            //        {
            //            mov_contable.credito = encabezado.valor_total - encabezado.valor_mercancia;
            //            mov_contable.basecontable = encabezado.valor_mercancia;
            //        }

            //    //}
            //    //if (cuentas.mov_cnt == "Credito")
            //    //{
            //    //if (item.id_nombre_parametro == 10)
            //    //{
            //    //    mov_contable.credito = encabezado.valor_total;
            //    //}
            //    //if (item.id_nombre_parametro == 11)
            //    //{
            //    //    mov_contable.credito = encabezado.valor_mercancia;
            //    //}
            //    //if (item.id_nombre_parametro == 2)
            //    //{
            //    //    mov_contable.credito = encabezado.valor_total - encabezado.valor_mercancia;
            //    //    mov_contable.basecontable = encabezado.valor_mercancia;
            //    //}
            //    //}
            //    if (cuentas.concepniff == 1)
            //    {
            //        mov_contable.debitoniif = mov_contable.debito;
            //        mov_contable.creditoniif = mov_contable.credito;
            //    }
            //    if(cuentas.concepniff == 4)
            //    {
            //        mov_contable.debitoniif = mov_contable.debito;
            //        mov_contable.creditoniif = mov_contable.credito;
            //    }
            //    mov_contable.detalle = "Factura comisión financiera #" + encabezado.numero;
            //    mov_contable.fec_creacion = DateTime.Now;
            //    mov_contable.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);


            //    var buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x => x.centro == item.centro && x.cuenta == item.cuenta && x.nit == 0);
            //    var fechaHoy = DateTime.Now;
            //    if (buscar_cuentas_valores != null)
            //    {
            //        buscar_cuentas_valores.ano = fechaHoy.Year;
            //        buscar_cuentas_valores.mes = fechaHoy.Month;
            //        buscar_cuentas_valores.cuenta = mov_contable.cuenta;
            //        buscar_cuentas_valores.centro = mov_contable.centro;
            //        buscar_cuentas_valores.nit = mov_contable.nit;
            //        buscar_cuentas_valores.debito += mov_contable.debito;
            //        buscar_cuentas_valores.credito += mov_contable.credito;
            //        buscar_cuentas_valores.debitoniff += mov_contable.debitoniif;
            //        buscar_cuentas_valores.creditoniff += mov_contable.creditoniif;
            //        context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
            //    }
            //    else
            //    {
            //        var crearCuentaValor = new cuentas_valores();
            //        crearCuentaValor.ano = fechaHoy.Year;
            //        crearCuentaValor.mes = fechaHoy.Month;
            //        crearCuentaValor.cuenta = mov_contable.cuenta;
            //        crearCuentaValor.centro = mov_contable.centro;
            //        crearCuentaValor.nit = mov_contable.nit;
            //        crearCuentaValor.debito = mov_contable.debito;
            //        crearCuentaValor.credito = mov_contable.credito;
            //        crearCuentaValor.debitoniff = mov_contable.debitoniif;
            //        crearCuentaValor.creditoniff = mov_contable.creditoniif;
            //        context.cuentas_valores.Add(crearCuentaValor);
            //    }


            //    context.mov_contable.Add(mov_contable);
            //    totalCreditos += mov_contable.credito;
            //    totalDebitos += mov_contable.debito;
            //    listaDescuadrados.Add(new DocumentoDescuadradoModel()
            //    {
            //        NumeroCuenta = cuentas.cntpuc_numero,
            //        DescripcionParametro = descripcionParametro,
            //        ValorDebito = totalDebitos,
            //        ValorCredito = totalCreditos
            //    });
            //}

            //if(totalDebitos != totalCreditos)
            //{
            //    TempData["mensaje_error"] = "El documento no tiene los movimientos calculados correctamente";
            //    var jsonSerialiser = new JavaScriptSerializer();
            //    var jsonList = jsonSerialiser.Serialize(listaDescuadrados);
            //    return RedirectToAction("Index", new { listaD = jsonList,totalDebitos = totalDebitos, totalCreditos = totalCreditos, menu });
            //}

            //context.SaveChanges();
            //return RedirectToAction("FacturaComision", new { id = encabezado.idencabezado, menu });
        }

        public ActionResult DevolverFacturaAuto(int encab_id_dev, int tipo_d, int? menu)
        {
            encab_documento buscarEncabezado = context.encab_documento.FirstOrDefault(x => x.idencabezado == encab_id_dev);
            if (buscarEncabezado != null)
            {
                long numeroConsecutivo = 0;
                grupoconsecutivos buscarGrupoConsecutivos = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == tipo_d && x.bodega_id == buscarEncabezado.bodega);
                int numeroGrupo = buscarGrupoConsecutivos != null ? buscarGrupoConsecutivos.grupo : 0;

                icb_doc_consecutivos numeroConsecutivoAux = context.icb_doc_consecutivos.OrderByDescending(x => x.doccons_ano)
                    .FirstOrDefault(x => x.doccons_idtpdoc == tipo_d && x.doccons_bodega == buscarEncabezado.bodega);
                if (numeroConsecutivoAux != null)
                {
                    if (numeroConsecutivoAux.doccons_requiere_mes)
                    {
                        if (numeroConsecutivoAux.doccons_mes != DateTime.Now.Month)
                        {
                            // Requiere mes pero no hay consecutivo para el año actual
                            TempData["mensaje_error"] =
                                "No existe un numero consecutivo asignado para este tipo de documento en el mes actual que es requerido.";
                            return RedirectToAction("Index", new { menu });
                        }

                        //requiereAnio = true;
                        numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                    }
                    else if (numeroConsecutivoAux.doccons_requiere_anio)
                    {
                        if (numeroConsecutivoAux.doccons_ano != DateTime.Now.Year)
                        {
                            // Requiere anio pero no hay consecutivo para el anio actual
                            TempData["mensaje_error"] =
                                "No existe un numero consecutivo asignado para este tipo de documento en el año actual que es requerido.";
                            return RedirectToAction("Index", new { menu });
                        }

                        numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                    }
                    else
                    {
                        numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                    }
                }
                else
                {
                    TempData["mensaje_error"] = "No existe un numero consecutivo asignado para este tipo de documento.";
                    return RedirectToAction("Index", new { menu });
                }

                // Aqui se termino de validar el consecutivo

                encab_documento crearEncabezado = new encab_documento
                {
                    tipo = tipo_d,
                    pedido = buscarEncabezado.pedido,
                    bodega = buscarEncabezado.bodega,
                    prefijo = buscarEncabezado.prefijo,
                    fpago_id = buscarEncabezado.fpago_id,
                    nit = buscarEncabezado.nit,
                    numero = numeroConsecutivo,
                    documento = buscarEncabezado.documento,
                    fecha = DateTime.Now,
                    fec_creacion = DateTime.Now,
                    vendedor = buscarEncabezado.vendedor,
                    vencimiento = DateTime.Now,
                    id_pedido_vehiculo = buscarEncabezado.id_pedido_vehiculo,
                    valor_total = buscarEncabezado.valor_total,
                    iva = buscarEncabezado.iva,
                    valor_mercancia = buscarEncabezado.valor_mercancia,
                    impoconsumo = buscarEncabezado.impoconsumo,
                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                };
                context.encab_documento.Add(crearEncabezado);
                bool guardarEncabezado = context.SaveChanges() > 0;
                encab_documento buscarUltimoEncabezado =
                    context.encab_documento.OrderByDescending(x => x.idencabezado).FirstOrDefault();

                //lineas_documento crearLineas = new lineas_documento()
                //{
                //    codigo = buscarEncabezado.documento,
                //    fec_creacion = DateTime.Now,
                //    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                //    nit = buscarEncabezado.nit,
                //    cantidad = 1,
                //    bodega = buscarEncabezado.bodega,
                //    seq = 1,
                //    porcentaje_iva = (float)buscarEncabezado.iva,
                //    valor_unitario = buscarEncabezado.valor_mercancia,
                //    impoconsumo = (float)buscarEncabezado.impoconsumo,
                //    estado = true,
                //    fec = DateTime.Now,
                //    id_encabezado = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0,
                //};
                //context.lineas_documento.Add(crearLineas);

                int secuencia = 1;
                List<mov_contable> buscarMovimientos = context.mov_contable.Where(x => x.id_encab == buscarEncabezado.idencabezado)
                    .ToList();
                centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
                icb_terceros terceroValorCero = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
                int idTerceroCero = centroValorCero != null ? Convert.ToInt32(terceroValorCero.tercero_id) : 0;

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
                            cuenta = movimiento.cuenta,
                            debito = movimiento.credito,
                            credito = movimiento.debito,
                            documento = buscarEncabezado.numero.ToString(),

                            //if (string.IsNullOrEmpty(buscarCuenta.ctareversion))
                            //{
                            //    movNuevo.cuenta = movimiento.cuenta;
                            //}
                            //else
                            //{
                            //    var buscarCuentaReversion = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_numero == buscarCuenta.ctareversion);
                            //    if (buscarCuentaReversion != null)
                            //    {
                            //        movNuevo.cuenta = buscarCuentaReversion.cntpuc_id;
                            //    }
                            //}

                            debitoniif = movimiento.creditoniif,
                            creditoniif = movimiento.debitoniif
                        };

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
                    // Actualiza los numeros consecutivos por documento
                    int grupoId = buscarGrupoConsecutivos != null ? buscarGrupoConsecutivos.grupo : 0;
                    List<grupoconsecutivos> gruposConsecutivos = context.grupoconsecutivos.Where(x => x.grupo == grupoId).ToList();
                    foreach (grupoconsecutivos grupo in gruposConsecutivos)
                    {
                        icb_doc_consecutivos buscarElemento = context.icb_doc_consecutivos.FirstOrDefault(x =>
                            x.doccons_idtpdoc == tipo_d && x.doccons_bodega == grupo.bodega_id);
                        buscarElemento.doccons_siguiente = buscarElemento.doccons_siguiente + 1;
                        context.Entry(buscarElemento).State = EntityState.Modified;
                    }

                    context.SaveChanges();
                    TempData["mensaje"] = numeroConsecutivo;
                    return RedirectToAction("Index", new { menu });
                    //return Json(new { mensaje, valorGuardado = true }, JsonRequestBehavior.AllowGet);
                }

                TempData["mensaje_error"] = "Error de conexion con base de datos.";
                return RedirectToAction("Index", new { menu });
                //var mensaje = "Error de conexion con base de datos.";
                //return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
            }

            TempData["mensaje_error"] = "No se encontro el encabezado de este documento.";
            return RedirectToAction("Index", new { menu });
            //var mensaje = "No se encontro el encabezado de este documento.";
            //return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
            //var movimientos = context.mov_contable.Where(x=> x.id_encab == id);

            //foreach(var mov_contable in movimientos) { 
            //    if(mov_contable.debito != null)
            //    {
            //        mov_contable.credito = mov_contable.debito;
            //        mov_contable.debito = null;
            //    }
            //    else
            //    {
            //        mov_contable.debito = mov_contable.credito;
            //        mov_contable.credito = null;
            //    }
            //    if(mov_contable.debitoniif != null)
            //    {
            //        mov_contable.creditoniif = mov_contable.debitoniif;
            //        mov_contable.debitoniif = null;
            //    }
            //    else
            //    {
            //        mov_contable.debitoniif = mov_contable.creditoniif;
            //        mov_contable.creditoniif = null;
            //    }
            //    mov_contable.detalle = "Devolución factura comisión de forma automatica";
            //    mov_contable.fec_actualizacion = DateTime.Now;
            //    mov_contable.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
            //    context.Entry(mov_contable).State = System.Data.Entity.EntityState.Modified;
            //}
            //context.SaveChanges();
            //TempData["mensaje"] = "Devolución de factura automática realizada correctamente";
            //return RedirectToAction("Index", new { menu });
        }

        [HttpPost]
        public ActionResult DevolverFacturaManual(int? encab_id, int? tipo_d, int? perfil_d, int? menu)
        {
            if (encab_id == null || perfil_d == null)
            {
                string mensaje = "No existe un numero de encabezado o perfil en esta compra, asegurese que exista";
                return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
            }

            encab_documento buscarEncabezado = context.encab_documento.FirstOrDefault(x => x.idencabezado == encab_id);
            if (buscarEncabezado != null)
            {
                long numeroConsecutivo = 0;
                int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                if (bodegaActual != buscarEncabezado.bodega)
                {
                    TempData["mensaje_error"] =
                        "La devolucion de la factura debe hacerse desde la bodega en la que se facturo.";
                    return RedirectToAction("Index", new { menu });
                }

                ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
                icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
                tp_doc_registros buscarTipoDocRegistro = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == tipo_d);
                numeroConsecutivoAux =
                    gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro, tipo_d ?? 0, buscarEncabezado.bodega);

                grupoconsecutivos grupoConsecutivo = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == tipo_d && x.bodega_id == buscarEncabezado.bodega);
                if (numeroConsecutivoAux != null)
                {
                    numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                    // PRIMERO SE VALIDA QUE LA RESOLUCION ESTE EN LAS FECHAS CORRECTAS Y LOS CONSECUTIVOS DENTRO DE LA RESOLUCION APLIQUEN
                    int grupo = grupoConsecutivo != null ? grupoConsecutivo.grupo : 0;
                }
                else
                {
                    TempData["mensaje_error"] = "No existe un numero consecutivo asignado para este tipo de documento";
                    return RedirectToAction("Index", new { menu });
                }

                //var buscarGrupoConsecutivos = context.grupoconsecutivos.FirstOrDefault(x => x.documento_id == tipo_d && x.bodega_id == buscarEncabezado.bodega);
                //var numeroGrupo = buscarGrupoConsecutivos != null ? buscarGrupoConsecutivos.grupo : 0;

                //var numeroConsecutivoAux = context.icb_doc_consecutivos.OrderByDescending(x => x.doccons_ano).FirstOrDefault(x => x.doccons_idtpdoc == tipo_d && x.doccons_bodega == buscarEncabezado.bodega);
                //if (numeroConsecutivoAux != null)
                //{
                //    if (numeroConsecutivoAux.doccons_requiere_mes)
                //    {
                //        if (numeroConsecutivoAux.doccons_mes != DateTime.Now.Month)
                //        {
                //            // Requiere mes pero no hay consecutivo para el año actual
                //            TempData["mensaje_error"] = "No existe un numero consecutivo asignado para este tipo de documento en el mes actual que es requerido.";
                //            return RedirectToAction("Index", new { menu });
                //        }
                //        else
                //        {
                //            //requiereAnio = true;
                //            numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                //        }
                //    }
                //    else if (numeroConsecutivoAux.doccons_requiere_anio)
                //    {
                //        if (numeroConsecutivoAux.doccons_ano != DateTime.Now.Year)
                //        {
                //            // Requiere anio pero no hay consecutivo para el anio actual
                //            TempData["mensaje_error"] = "No existe un numero consecutivo asignado para este tipo de documento en el año actual que es requerido.";
                //            return RedirectToAction("Index", new { menu });
                //        }
                //        else
                //        {
                //            numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                //        }
                //    }
                //    else
                //    {
                //        numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                //    }
                //}
                //else
                //{
                //    TempData["mensaje_error"] = "No existe un numero consecutivo asignado para este tipo de documento.";
                //    return RedirectToAction("Index", new { menu });
                //}

                // Se valida que el valor de credito sea igual al valor de debito para que la ecuacion quede balanceada
                //var parametrosCuentas = context.perfil_cuentas_documento.Where(x => x.id_perfil == modelo.perfilContable).ToList();
                var parametrosCuentasVerificar = (from perfil_cuenta in context.perfil_cuentas_documento
                                                  join nombreParametro in context.paramcontablenombres
                                                      on perfil_cuenta.id_nombre_parametro equals nombreParametro.id
                                                  join cuenta in context.cuenta_puc
                                                      on perfil_cuenta.cuenta equals cuenta.cntpuc_id
                                                  where perfil_cuenta.id_perfil == perfil_d
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
                int? sw_encontrado = buscarTipoDocRegistro != null ? buscarTipoDocRegistro.sw : 0;
                perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                    x.bodega == buscarEncabezado.bodega && x.sw == sw_encontrado &&
                    x.tipo_regimenid == regimen_proveedor);
                //var buscarTerceroProveedor = (from tercero in context.icb_terceros
                //                              join proveedor in context.tercero_proveedor
                //                              on tercero.tercero_id equals proveedor.tercero_id
                //                              join tributario in context.perfiltributario
                //                              on proveedor.tpregimen_id equals tributario.tipo_regimenid
                //                              join acteco in context.acteco_tercero
                //                              on proveedor.acteco_id equals acteco.acteco_id into ps
                //                              from acteco in ps.DefaultIfEmpty()
                //                              where tercero.tercero_id == buscarEncabezado.nit
                //                              select new
                //                              {
                //                                  tercero.tercero_id,
                //                                  proveedor.tpregimen_id,
                //                                  tributario.retfuente,
                //                                  tributario.retiva,
                //                                  tributario.retica,
                //                                  proveedor.acteco_id,
                //                                  acteco.tarifa
                //                              }).FirstOrDefault();

                decimal? retencionAux = 0;
                decimal? reteIvaAux = 0;
                decimal? reteIcaAux = 0;
                //if (buscarTerceroProveedor != null)
                //{
                //    var buscarPrefijoDocumento = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == tipo_d);
                //    if (buscarPrefijoDocumento != null)
                //    {
                //        var baseRetencion = buscarPrefijoDocumento.baseretencion;
                //        if (baseRetencion < buscarEncabezado.valor_mercancia)
                //        {
                //            if (buscarTerceroProveedor.retfuente == "A")
                //            {
                //                var porcentajeRetencion = buscarPrefijoDocumento.retiva;
                //                retencionAux = (buscarEncabezado.valor_mercancia * (decimal)porcentajeRetencion) / 100;
                //            }
                //        }

                //        var baseIva = buscarPrefijoDocumento.baseiva;
                //        var porcentajeIva = buscarPrefijoDocumento.retiva;
                //        var calcularIva = (buscarEncabezado.valor_mercancia * (decimal)porcentajeIva) / 100;
                //        if (baseIva < calcularIva)
                //        {
                //            if (buscarTerceroProveedor.retfuente == "A")
                //            {
                //                reteIvaAux = calcularIva;
                //            }
                //        }

                //        var baseIca = buscarPrefijoDocumento.baseica;
                //        if (baseIca < buscarEncabezado.valor_mercancia)
                //        {
                //            if (buscarTerceroProveedor.retfuente == "A")
                //            {
                //                var buscarActecoPorBodega = (from actecoBodega in context.terceros_bod_ica
                //                                             where actecoBodega.bodega == bodegaActual
                //                                             select new
                //                                             {
                //                                                 actecoBodega.porcentaje
                //                                             }).FirstOrDefault();
                //                decimal tarifaActeco = 0;
                //                if (buscarActecoPorBodega != null)
                //                {
                //                    tarifaActeco = buscarActecoPorBodega.porcentaje;
                //                }
                //                else
                //                {
                //                    if (buscarTerceroProveedor.tarifa != null)
                //                    {
                //                        tarifaActeco = buscarTerceroProveedor.tarifa;
                //                    }
                //                    else
                //                    {
                //                        tarifaActeco = (decimal)buscarPrefijoDocumento.retica;
                //                    }
                //                }
                //                reteIcaAux = (buscarEncabezado.valor_mercancia * tarifaActeco) / 100;
                //            }
                //        }
                //    }
                //}
                decimal retenciones = retencionAux ?? 0 + reteIvaAux ?? 0 + reteIcaAux ?? 0;

                List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();
                centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
                icb_terceros terceroValorCero = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
                int idTerceroCero = centroValorCero != null ? Convert.ToInt32(terceroValorCero.tercero_id) : 0;
                decimal calcularDebito = 0;
                decimal calcularCredito = 0;

                foreach (var parametro in parametrosCuentasVerificar)
                {
                    //var tipoParametro = 0;

                    cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
                    decimal valorCredito = 0;
                    decimal valorDebito = 0;
                    string CreditoDebito = "";
                    if (buscarCuenta != null)
                    {
                        if (parametro.id_nombre_parametro == 10)
                        {
                            valorCredito = buscarEncabezado.valor_total;
                            CreditoDebito = "Credito";
                        }

                        if (parametro.id_nombre_parametro == 11)
                        {
                            valorDebito = buscarEncabezado.valor_mercancia;
                            CreditoDebito = "Debito";
                        }

                        if (parametro.id_nombre_parametro == 2)
                        {
                            valorDebito = buscarEncabezado.valor_total - buscarEncabezado.valor_mercancia;
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
                                            valorDebito = buscarEncabezado.valor_mercancia * porcentajeRetencion / 100;
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
                                        if (buscarTipoDocRegistro.baseiva <= buscarEncabezado.valor_total -
                                            buscarEncabezado.valor_mercancia)
                                        {
                                            decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                            valorDebito =
                                                (buscarEncabezado.valor_total -
                                                 buscarEncabezado.valor_mercancia * porcentajeRetIva) / 100;
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
                                            if (buscarTipoDocRegistro.baseica <= buscarEncabezado.valor_mercancia)
                                            {
                                                decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                                //valorCredito = (modelo.precio * porcentajeRetIca) / 100;
                                                valorDebito =
                                                    buscarEncabezado.valor_mercancia * porcentajeRetIca / 1000;
                                                CreditoDebito = "Debito";
                                            }
                                        }
                                        else
                                        {
                                            valorCredito = buscarEncabezado.valor_mercancia * porcentajeReteICA / 1000;
                                            valorDebito = buscarEncabezado.valor_mercancia * porcentajeReteICA / 1000;
                                        }
                                    }
                                }
                            }
                        }

                        if (parametro.id_nombre_parametro == 17)
                        {
                            if (buscarPerfilTributario != null)
                            {
                                if (buscarPerfilTributario.autorretencion == "A")
                                {
                                    valorDebito = Convert.ToDecimal(buscarEncabezado.valor_mercancia, miCultura) *
                                                  porcentajeAutorretencion / 100;
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
                                    valorCredito = Convert.ToDecimal(buscarEncabezado.valor_mercancia, miCultura) *
                                                   porcentajeAutorretencion / 100;
                                    CreditoDebito = "Credito";
                                }
                            }
                        }

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
                    TempData["mensaje_error"] = "El documento se encuentra mal validado.";
                    return RedirectToAction("Index", new { menu });
                }
                // Fin de la validacion para el calculo del debito y credito del movimiento contable

                encab_documento crearEncabezado = new encab_documento
                {
                    tipo = tipo_d ?? 0,
                    pedido = buscarEncabezado.pedido,
                    bodega = buscarEncabezado.bodega,
                    prefijo = buscarEncabezado.prefijo,
                    fpago_id = buscarEncabezado.fpago_id,
                    nit = buscarEncabezado.nit,
                    numero = numeroConsecutivo,
                    documento = buscarEncabezado.documento,
                    fecha = DateTime.Now,
                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    fec_creacion = DateTime.Now,
                    vendedor = buscarEncabezado.vendedor,
                    vencimiento = DateTime.Now,
                    id_pedido_vehiculo = buscarEncabezado.id_pedido_vehiculo,
                    valor_total = buscarEncabezado.valor_total,
                    iva = buscarEncabezado.iva,
                    valor_mercancia = buscarEncabezado.valor_mercancia,
                    impoconsumo = buscarEncabezado.impoconsumo
                };

                context.encab_documento.Add(crearEncabezado);
                bool guardarEncabezado = context.SaveChanges() > 0;
                encab_documento buscarUltimoEncabezado =
                    context.encab_documento.OrderByDescending(x => x.idencabezado).FirstOrDefault();

                //lineas_documento crearLineas = new lineas_documento()
                //{
                //    //tipo = 20,
                //    codigo = buscarEncabezado.documento,
                //    fec_creacion = DateTime.Now,
                //    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                //    nit = buscarEncabezado.nit,
                //    cantidad = 1,
                //    bodega = buscarEncabezado.bodega,
                //    seq = 1,
                //    porcentaje_iva = (float)buscarEncabezado.iva,
                //    valor_unitario = buscarEncabezado.valor_mercancia,
                //    impoconsumo = (float)buscarEncabezado.impoconsumo,
                //    //numero = numeroConsecutivo,
                //    estado = true,
                //    fec = DateTime.Now,
                //    id_encabezado = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0,
                //};
                //context.lineas_documento.Add(crearLineas);

                List<perfil_cuentas_documento> perfil = context.perfil_cuentas_documento.Where(x => x.id_perfil == perfil_d).ToList();
                foreach (perfil_cuentas_documento parametro in perfil)
                {
                    decimal valorCredito = 0;
                    decimal valorDebito = 0;
                    int secuencia = 1;
                    string CreditoDebito = "";
                    cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
                    if (buscarCuenta != null)
                    {
                        if (parametro.id_nombre_parametro == 10)
                        {
                            valorCredito = buscarEncabezado.valor_total;
                            CreditoDebito = "Credito";
                        }

                        if (parametro.id_nombre_parametro == 11)
                        {
                            valorDebito = buscarEncabezado.valor_mercancia;
                            CreditoDebito = "Debito";
                        }

                        if (parametro.id_nombre_parametro == 2)
                        {
                            valorDebito = buscarEncabezado.valor_total - buscarEncabezado.valor_mercancia;
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
                                            valorDebito = buscarEncabezado.valor_mercancia * porcentajeRetencion / 100;
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
                                        if (buscarTipoDocRegistro.baseiva <= buscarEncabezado.valor_total -
                                            buscarEncabezado.valor_mercancia)
                                        {
                                            decimal porcentajeRetIva = (decimal)buscarTipoDocRegistro.retiva;
                                            valorDebito =
                                                (buscarEncabezado.valor_total -
                                                 buscarEncabezado.valor_mercancia * porcentajeRetIva) / 100;
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
                                            if (buscarTipoDocRegistro.baseica <= buscarEncabezado.valor_mercancia)
                                            {
                                                decimal porcentajeRetIca = (decimal)buscarTipoDocRegistro.retica;
                                                valorDebito =
                                                    buscarEncabezado.valor_mercancia * porcentajeRetIca / 1000;
                                                CreditoDebito = "Debito";
                                            }
                                        }
                                        else
                                        {
                                            valorCredito = buscarEncabezado.valor_mercancia * porcentajeReteICA / 1000;
                                            valorDebito = buscarEncabezado.valor_mercancia * porcentajeReteICA / 1000;
                                        }
                                    }
                                }
                            }
                        }

                        if (parametro.id_nombre_parametro == 17)
                        {
                            if (buscarPerfilTributario != null)
                            {
                                if (buscarPerfilTributario.autorretencion == "A")
                                {
                                    valorDebito = Convert.ToDecimal(buscarEncabezado.valor_mercancia, miCultura) *
                                                  porcentajeAutorretencion / 100;
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
                                    valorCredito = Convert.ToDecimal(buscarEncabezado.valor_mercancia, miCultura) *
                                                   porcentajeAutorretencion / 100;
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

                        //if (string.IsNullOrEmpty(buscarCuenta.ctareversion))
                        //{
                        //    movNuevo.cuenta = parametro.cuenta;
                        //}
                        //else
                        //{
                        //    var buscarCuentaReversion = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_numero == buscarCuenta.ctareversion);
                        //    if (buscarCuentaReversion != null)
                        //    {
                        //        movNuevo.cuenta = buscarCuentaReversion.cntpuc_id;
                        //    }
                        //}

                        movNuevo.detalle = "Devolución Facturación Comisiones a documento " +
                                           buscarUltimoEncabezado.documento;
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
                            x.doccons_idtpdoc == tipo_d && x.doccons_bodega == grupo.bodega_id);
                        buscarElemento.doccons_siguiente = buscarElemento.doccons_siguiente + 1;
                        context.Entry(buscarElemento).State = EntityState.Modified;
                    }

                    context.SaveChanges();
                    ViewBag.numFacturacionCreada = numeroConsecutivo;
                    TempData["mensaje"] = numeroConsecutivo;
                    return RedirectToAction("Index", new { menu });
                }

                TempData["mensaje_error"] = "Error de conexion con base de datos.";
                return RedirectToAction("Index", new { menu });
            }

            TempData["mensaje_error"] = "No se encontro el encabezado de este documento.";
            return RedirectToAction("Index", new { menu });

            //***********************************************************************************************************************
            //var idencab = Convert.ToInt32(Request["encab_id"]);
            //var encabezado = context.encab_documento.Find(idencab);

            //var idperfil = Convert.ToInt32(Request["perfil_d"]);
            //var perfil = context.perfil_cuentas_documento.Where(x => x.id_perfil == idperfil).ToList();
            //foreach (var item in perfil)
            //{
            //    var cuentas = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == item.cuenta);
            //    var mov_contable = context.mov_contable.FirstOrDefault(x => x.idparametronombre == item.id_nombre_parametro && x.id_encab == idencab);
            //    mov_contable.id_encab = encabezado.idencabezado;
            //    mov_contable.cuenta = item.cuenta;
            //    mov_contable.centro = item.centro;
            //    if (cuentas.tercero == true)
            //    {
            //        mov_contable.nit = encabezado.nit;
            //    }
            //    mov_contable.fec = DateTime.Now;
            //    if (cuentas.mov_cnt == "Debito")
            //    {
            //        if (item.id_nombre_parametro == 10)
            //        {
            //            mov_contable.debito = encabezado.valor_total;
            //        }
            //        if (item.id_nombre_parametro == 11)
            //        {
            //            mov_contable.debito = encabezado.valor_mercancia;
            //        }
            //        if (item.id_nombre_parametro == 2)
            //        {
            //            mov_contable.debito = encabezado.valor_total - encabezado.valor_mercancia;
            //            mov_contable.basecontable = encabezado.valor_mercancia;
            //        }
            //    }
            //    if (cuentas.mov_cnt == "Credito")
            //    {
            //        if (item.id_nombre_parametro == 10)
            //        {
            //            mov_contable.credito = encabezado.valor_total;
            //        }
            //        if (item.id_nombre_parametro == 11)
            //        {
            //            mov_contable.credito = encabezado.valor_mercancia;
            //        }
            //        if (item.id_nombre_parametro == 2)
            //        {
            //            mov_contable.credito = encabezado.valor_total - encabezado.valor_mercancia;
            //            mov_contable.basecontable = encabezado.valor_mercancia;
            //        }
            //    }
            //    if (cuentas.concepniff == 1)
            //    {
            //        mov_contable.debitoniif = mov_contable.debito;
            //        mov_contable.creditoniif = mov_contable.credito;
            //    }
            //    if (cuentas.concepniff == 4)
            //    {
            //        mov_contable.debitoniif = mov_contable.debito;
            //        mov_contable.creditoniif = mov_contable.credito;
            //    }
            //    mov_contable.detalle = "Devolución factura comisión de forma manual";
            //    mov_contable.fec_actualizacion = DateTime.Now;
            //    mov_contable.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
            //    context.Entry(mov_contable).State = System.Data.Entity.EntityState.Modified;
            //}
            //context.SaveChanges();
            //TempData["mensaje"] = "Devolución de factura automática realizada correctamente";
            //return RedirectToAction("Index", new { menu });
        }

        // PDf FacturacionComisiones
        public ActionResult FacturaComision(int id)
        {
            encab_documento encabezado = context.encab_documento.FirstOrDefault(x => x.idencabezado == id);
            List<detalle_comision_financiera> detalle = context.detalle_comision_financiera.Where(x => x.encabezado_id == id).ToList();
            ViewBag.total_detalles = detalle.Count();
            string financiera = context.icb_unidad_financiera.Find(encabezado.facturado_a).financiera_nombre;
            ViewBag.detalles = detalle;
            ViewBag.financiera = financiera;
            ViewBag.valor_iva = encabezado.valor_total - encabezado.valor_mercancia;
            users vendedor = context.users.Find(encabezado.vendedor);
            ViewBag.vendedor = vendedor.user_nombre + " " + vendedor.user_apellido;

            //var cliente = from v in context.vpedido
            //              join t in  context.icb_terceros
            //              on v.nit equals Convert.ToInt32(t.doc_tercero)
            //              join c in context.v_creditos
            //              on v.


            string root = Server.MapPath("~/Pdf/");
            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
            string path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);

            ViewAsPdf something = new ViewAsPdf("FacturaComision", encabezado);
            return something;
            //return View();
        }

        public JsonResult BuscarDatos(DateTime? fechaDesde, DateTime? fechaHasta, string financiera)
        {
            var predicado = PredicateBuilder.True<vwFinancieraDatos>();
            CultureInfo myCultura = new CultureInfo("is-IS");

            if (fechaDesde != null)
            {
                predicado = predicado.And(d => d.fec_facturacomision >= fechaDesde);
            }
            if (fechaHasta != null)
            {
                fechaHasta = fechaHasta.Value.AddDays(1);
                predicado = predicado.And(d => d.fec_facturacomision <= fechaHasta);
            }
            if (!string.IsNullOrWhiteSpace(financiera))
            {
                int financiera_id2 = Convert.ToInt32(financiera);
                predicado = predicado.And(d => d.financiera_id == financiera_id2);
            }

            var data = context.vwFinancieraDatos.Where(predicado).ToList();
            var data1 = (from fd in data
                         select new//
                         {
                             fd.id,
                             fd.financiera_id,
                             fd.numero,
                             valor_comision=fd.valor_comision,
                             fec_confirmacion = fd.fec_confirmacion.Value.ToString("yyyy/MM/dd HH:mm:ss", myCultura),
                             fecha2 =fd.fec_facturacomision.ToString(),
                             financiera=fd.financiera,
                             fd.asesor,
                             fd.tercero_id
                         }).ToList();
            return Json(data1, JsonRequestBehavior.AllowGet);

        }

        public JsonResult BuscarDatosDevolucion(DateTime? fechaDesde, DateTime? fechaHasta, string financiera)
        {
            icb_sysparameter param = context.icb_sysparameter.Where(d => d.syspar_cod == "P141").FirstOrDefault();
            int comi = param != null ? Convert.ToInt32(param.syspar_value) : 22;
            CultureInfo myCultura = new CultureInfo("is-IS");

            var predicado = PredicateBuilder.True<vwDatosFinanciera>();
            if (fechaDesde != null)
            {
                predicado = predicado.And(d => d.fec_facturacomision >= fechaDesde);
            }
            if (fechaHasta != null)
            {
                fechaHasta = fechaHasta.Value.AddDays(1);
                predicado = predicado.And(d => d.fec_facturacomision <= fechaHasta);
            }
            if (!string.IsNullOrWhiteSpace(financiera))
            {
                int financiera_id2 = Convert.ToInt32(financiera);
                predicado = predicado.And(d => d.financiera_id == financiera_id2);
            }

            predicado = predicado.And(d => d.sw2 == comi);

            var data = context.vwDatosFinanciera.Where(predicado).ToList();
            var data1 =( from f in data
                        select new
                        {
                            f.id,
                            financiera=f.financiera_nombre,
                            f.asesor,
                            f.numero,
                            valor_comision = f.valor_total,
                            fec_facturacomision = f.fec_facturacomision.ToString("yyyy/MM/dd HH:mm:ss", myCultura)
                        }).ToList();
                return Json(data1, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ValidarResolucion(string grup, string tip)
        {
            int tipo = Convert.ToInt32(tip);
            int bodega_actual = Convert.ToInt32(Session["user_bodega"]);
            grupoconsecutivos grupoconsecutivos =
                context.grupoconsecutivos.FirstOrDefault(x => x.documento_id == tipo && x.bodega_id == bodega_actual);
            int grupo = grupoconsecutivos != null ? grupoconsecutivos.grupo : 0;

            resolucionfactura data = context.resolucionfactura.FirstOrDefault(x => x.tipodoc == tipo && x.grupo == grupo);
            int result = 0;

            if (data != null)
            {
                int d = DateTime.Now.AddDays(-data.diasaviso).CompareTo(data.fechafin);
                if (DateTime.Now.AddDays(-data.diasaviso).CompareTo(data.fechafin) < 0)
                {
                    result = 1;
                }

                icb_doc_consecutivos consecutivos = context.icb_doc_consecutivos.FirstOrDefault(x => x.doccons_idtpdoc == tipo);
                if (data.consecaviso <= consecutivos.doccons_siguiente)
                {
                    result = 2;
                }

                if (data.fechafin.CompareTo(DateTime.Now) < 0)
                {
                    result = 3;
                }

                if (data.consecfin < consecutivos.doccons_siguiente)
                {
                    result = 4;
                }
            }


            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarComisiones()
        {
            var data = (from d in context.detalle_comision_financiera
                        join e in context.encab_documento
                            on d.encabezado_id equals e.idencabezado into tmp1
                        from e in tmp1.DefaultIfEmpty()
                        join t in context.tp_doc_registros
                            on e.tipo equals t.tpdoc_id into tmp2
                        from t in tmp2.DefaultIfEmpty()
                        join b in context.bodega_concesionario
                            on e.bodega equals b.id into tmp3
                        from b in tmp3.DefaultIfEmpty()
                        join e2 in context.encab_documento
                            on e.prefijo equals e2.idencabezado into tmp4
                        from e2 in tmp4.DefaultIfEmpty()
                        join ter in context.icb_terceros
                            on e.nit equals ter.tercero_id into tmp5
                        from ter in tmp5.DefaultIfEmpty()
                        where e.tipo == 3042
                        select new
                        {
                            tipoDev = e.tipo,
                            tipoVen =e2!=null? e2.tipo : 0,
                            numDev = e.numero,
                            numVen = e2 != null ?e2.numero : 0,
                            idDev = e.idencabezado,
                            idVen = e2 != null ? e2.idencabezado : 0,
                            t.tpdoc_nombre,
                            e.documento,
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
                            prefijo = e.prefijo != null ? e.prefijo : 0
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
                x.prefijo
            }).ToList();

            return Json(info, JsonRequestBehavior.AllowGet);
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