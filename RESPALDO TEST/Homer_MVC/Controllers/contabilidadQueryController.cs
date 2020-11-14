using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class contabilidadQueryController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");
        private readonly contabilidadAuxiliar modelopdf = new contabilidadAuxiliar();

        // GET: contabilidadQuery
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult InformeAuxiliar()
        {
            var buscarCuentas = (from cuenta in context.cuenta_puc
                                 orderby cuenta.cntpuc_numero
                                 select new
                                 {
                                     id = cuenta.cntpuc_id,
                                     numero = cuenta.cntpuc_numero,
                                     nombre = cuenta.cntpuc_descp
                                 }).ToList();

            var cuentas = buscarCuentas.OrderBy(x => long.Parse(x.numero)).Select(x => new
            {
                x.id,
                x.numero,
                cta = x.numero + " (" + x.nombre + ")"
            });
            ViewBag.txtCuentaInicio = new SelectList(cuentas, "numero", "cta");
            ViewBag.txtCuentaFin = new SelectList(cuentas, "numero", "cta");

            var buscarCentrosCosto = (from centro in context.centro_costo
                                      orderby centro.pre_centcst
                                      select new
                                      {
                                          id = centro.centcst_id,
                                          numero = "(" + centro.pre_centcst + ") " + centro.centcst_nombre
                                      }).ToList();
            ViewBag.txtCentrosCosto = new SelectList(buscarCentrosCosto, "id", "numero");

            var buscarNits = (from terceros in context.icb_terceros
                              select new
                              {
                                  terceros.tercero_id,
                                  nombre = "(" + terceros.doc_tercero + ") " + terceros.prinom_tercero + " " +
                                           terceros.segnom_tercero + " " + terceros.apellido_tercero + " " +
                                           terceros.segapellido_tercero + " " + terceros.razon_social
                              }).ToList();
            ViewBag.txtNit = new SelectList(buscarNits, "tercero_id", "nombre");
            var buscarTipoDocuemntos = (from documentos in context.tp_doc_registros
                                        select new
                                        {
                                            documentos.tpdoc_id,
                                            nombre = "(" + documentos.prefijo + ") " + documentos.tpdoc_nombre + " "
                                        }).ToList();
            ViewBag.txtTipoDocumentos = new SelectList(buscarTipoDocuemntos, "tpdoc_id", "nombre");

            return View();
        }

        public JsonResult BuscarInformacionCuentas(string cuentaInicio, string cuentaFin, DateTime? FechaInicio,
            DateTime? FechaFin, int[] centros, bool? filtroCentro, bool? filtroNit, int? nit, bool? filtroMovimiento)
        {
            if (string.IsNullOrEmpty(cuentaInicio) || string.IsNullOrEmpty(cuentaFin))
            {
                // No se hace filtrado si no se seleccionaron un rango de cuentas
            }
            else
            {
                string cuenti = cuentaInicio;
                string cuentf = cuentaFin;


                int digitosCuenta1 = 10 - (cuentaInicio != null ? cuentaInicio.Length : 0);
                for (int c = 0; c < digitosCuenta1; c++)
                {
                    cuentaInicio += "0";
                }

                int digitosCuenta2 = 10 - (cuentaFin != null ? cuentaFin.Length : 0);
                for (int c = 0; c < digitosCuenta2; c++)
                {
                    cuentaFin += "9";
                }

                if (FechaInicio == null)
                {
                    cuentas_valores primeraFecha = context.cuentas_valores.OrderBy(x => x.ano).ThenBy(x => x.mes).FirstOrDefault();
                    if (primeraFecha != null)
                    {
                        FechaInicio = new DateTime(primeraFecha.ano, primeraFecha.mes, 1);
                    }
                    else
                    {
                        FechaInicio = DateTime.Now;
                    }
                }

                if (FechaFin == null)
                {
                    cuentas_valores ultimaFecha = context.cuentas_valores.OrderByDescending(x => x.ano).ThenByDescending(x => x.mes)
                        .FirstOrDefault();
                    if (ultimaFecha != null)
                    {
                        FechaFin = new DateTime(ultimaFecha.ano, ultimaFecha.mes, 1);
                    }
                    else
                    {
                        FechaFin = DateTime.Now;
                    }
                }

                var filtroCuentas = (from cValor in context.cuentas_valores
                                     join cPuc in context.cuenta_puc
                                         on cValor.cuenta equals cPuc.cntpuc_id
                                     select new
                                     {
                                         cPuc.cntpuc_id,
                                         cPuc.cntpuc_numero
                                     }).OrderBy(x => x.cntpuc_numero).ToList();

                int A1 = cuenti.Length;
                int B2 = cuentf.Length;


                string cuentaInicioLv1 = cuentaInicio != null ? cuentaInicio.Substring(0, A1) : "1";
                string cuentaFinLv1 = cuentaFin != null ? cuentaFin.Substring(0, B2) : "9";


                List<string> indices = new List<string>();
                for (int i = Convert.ToInt32(cuentaInicioLv1); i <= Convert.ToInt32(cuentaFinLv1); i++)
                {
                    indices.Add(i.ToString());
                }

                List<CuentaConsulta> lista = getNivel1(indices, FechaInicio ?? DateTime.Now, FechaFin ?? DateTime.Now, centros,
                    filtroCentro ?? false, filtroNit ?? false, nit, filtroMovimiento ?? false);


                string cuentaInicioLv2 = cuentaInicio != null ? cuentaInicio.Substring(0, 2) : "10";
                string cuentaFinLv2 = cuentaFin != null ? cuentaFin.Substring(0, 2) : "99";
                List<string> indices2 = new List<string>();
                for (int i = Convert.ToInt32(cuentaInicioLv2); i <= Convert.ToInt32(cuentaFinLv2); i++)
                {
                    indices2.Add(i.ToString());
                }

                List<CuentaConsulta> lista2 = getNivel2(indices2, DateTime.Now, DateTime.Now, lista, filtroCentro ?? false,
                    filtroNit ?? false, nit);


                string cuentaInicioLv3 = cuentaInicio != null ? cuentaInicio.Substring(0, 4) : "1000";
                string cuentaFinLv3 = cuentaFin != null ? cuentaFin.Substring(0, 4) : "9999";
                List<string> indices3 = new List<string>();
                for (int i = Convert.ToInt32(cuentaInicioLv3); i <= Convert.ToInt32(cuentaFinLv3); i++)
                {
                    indices3.Add(i.ToString());
                }

                List<CuentaConsulta> lista3 = getNivel3(indices3, DateTime.Now, DateTime.Now, lista, filtroCentro ?? false,
                    filtroNit ?? false, nit);


                string cuentaInicioLv4 = cuentaInicio != null ? cuentaInicio.Substring(0, 6) : "100000";
                string cuentaFinLv4 = cuentaFin != null ? cuentaFin.Substring(0, 6) : "999999";
                List<string> indices4 = new List<string>();
                for (int i = Convert.ToInt32(cuentaInicioLv4); i <= Convert.ToInt32(cuentaFinLv4); i++)
                {
                    indices4.Add(i.ToString());
                }

                List<CuentaConsulta> lista4 = getNivel4(indices4, DateTime.Now, DateTime.Now, lista, filtroCentro ?? false,
                    filtroNit ?? false, nit);


                string cuentaInicioLv5 = cuentaInicio != null ? cuentaInicio.Substring(0, 8) : "10000000";
                string cuentaFinLv5 = cuentaFin != null ? cuentaFin.Substring(0, 8) : "99999999";
                List<string> indices5 = new List<string>();
                long inicioIndice = Convert.ToInt64(cuentaInicioLv5);
                long finIndice = Convert.ToInt64(cuentaFinLv5);
                indices5 = context.cuenta_puc.ToList()
                    .Where(x => long.Parse(x.cntpuc_numero) >= inicioIndice && long.Parse(x.cntpuc_numero) <= finIndice)
                    .Select(x => x.cntpuc_numero).ToList();
                List<CuentaConsulta> lista5 = getNivel5(indices5, DateTime.Now, DateTime.Now, lista, filtroCentro ?? false,
                    filtroNit ?? false, nit);


                string cuentaInicioLv6 = cuentaInicio != null ? cuentaInicio.Substring(0, 10) : "1000000000";
                string cuentaFinLv6 = cuentaFin != null ? cuentaFin.Substring(0, 10) : "9999999999";
                List<string> indices6 = new List<string>();
                long inicioIndice6 = Convert.ToInt64(cuentaInicioLv6);
                long finIndice6 = Convert.ToInt64(cuentaFinLv6);
                indices6 = context.cuenta_puc.ToList()
                    .Where(x => long.Parse(x.cntpuc_numero) >= inicioIndice6 &&
                                long.Parse(x.cntpuc_numero) <= finIndice6).Select(x => x.cntpuc_numero).ToList();
                List<CuentaConsulta> lista6 = getNivel6(indices6, DateTime.Now, DateTime.Now, lista, filtroCentro ?? false,
                    filtroNit ?? false, nit);

                return Json(lista, JsonRequestBehavior.AllowGet);
            }


            List<CuentaConsulta> listaVacia = new List<CuentaConsulta>();
            return Json(listaVacia, JsonRequestBehavior.AllowGet);
        }

        public List<CuentaConsulta> getNivel1(List<string> indices, DateTime FechaInicio, DateTime FechaFin,
            int[] centros, bool filtroCentro, bool filtroNit, int? nit, bool filtroMovimiento)
        {
            if (centros.Length == 1 && centros[0] == 0)
            {
                centros = context.centro_costo.Select(x => x.centcst_id).ToArray();
            }

            var indicesNivel1 = (from cuentaValor in context.cuentas_valores
                                 join cuenta in context.cuenta_puc
                                     on cuentaValor.cuenta equals cuenta.cntpuc_id
                                 where indices.Contains(cuenta.cntpuc_numero.Substring(0, 1))
                                       && cuentaValor.ano >= FechaInicio.Year && cuentaValor.ano <= FechaFin.Year
                                       && cuentaValor.mes >= FechaInicio.Month && cuentaValor.mes <= FechaFin.Month
                                       && centros.Contains(cuentaValor.centro)
                                 select new
                                 {
                                     cntpuc_numero = cuenta.cntpuc_numero.Substring(0, 1),
                                     context.cuenta_puc.FirstOrDefault(x => x.cntpuc_numero == cuenta.cntpuc_numero.Substring(0, 1))
                                         .esafectable,
                                     cuentaValor.ano,
                                     cuentaValor.mes
                                 }).Distinct().ToList();

            var indicesNombres = (from cuenta in context.cuenta_puc
                                  where cuenta.cntpuc_numero.Length == 1
                                  select new { cuenta.cntpuc_numero, cuenta.cntpuc_descp }).ToList();

            List<string> indicesNumericos = indicesNivel1.Select(x => x.cntpuc_numero).ToList();
            List<CuentaConsulta> listaCuentas = new List<CuentaConsulta>();
            foreach (var indicesUno in indicesNivel1)
            {
                var buscarCuentasNivel1 = (from cuentaValor in context.cuentas_valores
                                           join cuentaPuc in context.cuenta_puc
                                               on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                           where cuentaPuc.cntpuc_numero.Substring(0, 1) == indicesUno.cntpuc_numero
                                                 && cuentaValor.ano == indicesUno.ano && cuentaValor.mes == indicesUno.mes
                                           select new
                                           {
                                               nivel = cuentaPuc.cntpuc_numero.Substring(0, 1),
                                               indicesUno.esafectable,

                                               total = (from cuentasValores in context.cuentas_valores
                                                        join cuenta in context.cuenta_puc
                                                            on cuentasValores.cuenta equals cuenta.cntpuc_id
                                                        where cuenta.cntpuc_numero.Substring(0, 1) == cuentaPuc.cntpuc_numero.Substring(0, 1)
                                                              && cuentasValores.ano >= indicesUno.ano && cuentasValores.ano <= indicesUno.ano
                                                              && cuentasValores.mes >= indicesUno.mes && cuentasValores.mes <= indicesUno.mes
                                                        group cuentasValores by new
                                                        { cuentasValores.ano, cuentasValores.mes /*, cuenta.cntpuc_numero, cuenta.cntpuc_descp*/}
                                                   into g
                                                        select new
                                                        {
                                                            anio = g.Key.ano,
                                                            g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                            saldoInicial = g.Sum(x => x.saldo_ini),
                                                            saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                            debito = g.Sum(x => x.debito),
                                                            credito = g.Sum(x => x.credito),
                                                            debitoNiff = g.Sum(x => x.debitoniff),
                                                            creditoNiff = g.Sum(x => x.creditoniff)
                                                        }).FirstOrDefault()
                                           }).Distinct().OrderBy(x => x.nivel).ToList();


                foreach (var item in buscarCuentasNivel1)
                {
                    listaCuentas.Add(new CuentaConsulta
                    {
                        visible = true,
                        esAfectable = item.esafectable,
                        cuenta = item.nivel,
                        cuentaDescripcion = indicesNombres.FirstOrDefault(x => x.cntpuc_numero == item.nivel)
                            .cntpuc_descp,
                        ano = item.total.anio,
                        mes = item.total.mes,
                        saldo_ini = item.total.saldoInicial,
                        saldo_inicial_niff = item.total.saldoInicialNiff,
                        debito = item.total.debito,
                        credito = item.total.credito,
                        debitoNiff = item.total.debitoNiff,
                        creditoNiff = item.total.creditoNiff,
                        total = item.total.saldoInicial + item.total.debito - item.total.credito,
                        totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff - item.total.creditoNiff
                    });
                }
            }


            if (filtroCentro)
            {
                foreach (CuentaConsulta cuenta in listaCuentas)
                {
                    List<CuentaConsulta> listaCentros = new List<CuentaConsulta>();

                    var buscarCentrosNivel1 = (from cuentaValor in context.cuentas_valores
                                               join centro in context.centro_costo
                                                   on cuentaValor.centro equals centro.centcst_id
                                               join cuentaPuc in context.cuenta_puc
                                                   on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                               where cuentaPuc.cntpuc_numero.Substring(0, 1) == cuenta.cuenta
                                                     && cuentaValor.ano == cuenta.ano && cuentaValor.mes == cuenta.mes
                                               select new
                                               {
                                                   centro.centcst_id,
                                                   centro.centcst_nombre,
                                                   centro.pre_centcst,

                                                   total = (from cuentasValores in context.cuentas_valores
                                                            join centroC in context.centro_costo
                                                                on cuentasValores.centro equals centroC.centcst_id
                                                            join cuentaP in context.cuenta_puc
                                                                on cuentasValores.cuenta equals cuentaP.cntpuc_id
                                                            where cuentaP.cntpuc_numero.Substring(0, 1) == cuenta.cuenta
                                                                  && centroC.centcst_id == centro.centcst_id
                                                                  && cuentasValores.ano >= cuenta.ano && cuentasValores.ano <= cuenta.ano
                                                                  && cuentasValores.mes >= cuenta.mes && cuentasValores.mes <= cuenta.mes
                                                            group cuentasValores by new
                                                            {
                                                                cuentasValores.ano,
                                                                cuentasValores.mes,
                                                                cuentasValores.centro /*, cuenta.cntpuc_descp*/
                                                            }
                                                       into g
                                                            select new
                                                            {
                                                                g.Key.centro,
                                                                anio = g.Key.ano,
                                                                g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                                saldoInicial = g.Sum(x => x.saldo_ini),
                                                                saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                debito = g.Sum(x => x.debito),
                                                                credito = g.Sum(x => x.credito),
                                                                debitoNiff = g.Sum(x => x.debitoniff),
                                                                creditoNiff = g.Sum(x => x.creditoniff)
                                                            }).FirstOrDefault()
                                               }).Distinct().OrderBy(x => x.centcst_id).ToList();

                    foreach (var item in buscarCentrosNivel1)
                    {
                        listaCentros.Add(new CuentaConsulta
                        {
                            centroPrefijo = item.pre_centcst,
                            centro = item.centcst_nombre,
                            ano = item.total.anio,
                            mes = item.total.mes,
                            saldo_ini = item.total.saldoInicial,
                            saldo_inicial_niff = item.total.saldoInicialNiff,
                            debito = item.total.debito,
                            credito = item.total.credito,
                            debitoNiff = item.total.debitoNiff,
                            creditoNiff = item.total.creditoNiff,
                            total = item.total.saldoInicial + item.total.debito - item.total.credito,
                            totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff - item.total.creditoNiff
                        });
                    }

                    cuenta.centrosHijos = listaCentros;
                }
            }

            if (filtroMovimiento)
            {
                foreach (CuentaConsulta cuenta in listaCuentas)
                {
                    List<CuentaConsulta> listaMovimientos = new List<CuentaConsulta>();

                    var buscarMovimientosNivel1 = (from cuentaValor in context.cuentas_valores
                                                   join movContable in context.mov_contable
                                                       on cuentaValor.cuenta equals movContable.cuenta
                                                   join encabezado in context.encab_documento
                                                       on movContable.id_encab equals encabezado.idencabezado
                                                   join tpDocumento in context.tp_doc_registros
                                                       on encabezado.tipo equals tpDocumento.tpdoc_id
                                                   where cuentaValor.ano == cuenta.ano && cuentaValor.mes == cuenta.mes &&
                                                         movContable.fec.Year == cuentaValor.ano && movContable.fec.Month == cuentaValor.mes
                                                   select new
                                                   {
                                                       tpDocumento.prefijo,
                                                       tpDocumento.tpdoc_nombre,
                                                       movContable.fec.Year,
                                                       movContable.fec.Month,
                                                       movContable.debito,
                                                       movContable.credito,
                                                       movContable.debitoniif,
                                                       movContable.creditoniif
                                                   }).ToList();

                    foreach (var item in buscarMovimientosNivel1)
                    {
                        listaMovimientos.Add(new CuentaConsulta
                        {
                            prefijoDocumento = item.prefijo,
                            nombreDocumento = item.tpdoc_nombre,
                            ano = item.Year,
                            mes = item.Month,
                            debito = item.debito,
                            credito = item.credito,
                            debitoNiff = item.debitoniif,
                            creditoNiff = item.creditoniif
                        });
                    }

                    cuenta.MovimientosHijos = listaMovimientos;
                }
            }

            if (filtroNit)
            {
                foreach (CuentaConsulta cuenta in listaCuentas)
                {
                    List<CuentaConsulta> listaNits = new List<CuentaConsulta>();
                    var buscarNitsNivel1 = (from cuentaValor in context.cuentas_valores
                                            join tercero in context.icb_terceros
                                                on cuentaValor.nit equals tercero.tercero_id
                                            join cuentaPuc in context.cuenta_puc
                                                on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                            where cuentaPuc.cntpuc_numero.Substring(0, 1) == cuenta.cuenta
                                                  && cuentaValor.ano == cuenta.ano && cuentaValor.mes == cuenta.mes
                                            select new
                                            {
                                                tercero.tercero_id,
                                                tercero.prinom_tercero,
                                                tercero.segnom_tercero,
                                                tercero.apellido_tercero,
                                                tercero.segapellido_tercero,
                                                tercero.razon_social,
                                                tercero.doc_tercero,
                                                total = (from cuentasValores in context.cuentas_valores
                                                         join terceroC in context.icb_terceros
                                                             on cuentasValores.nit equals terceroC.tercero_id
                                                         join cuentaP in context.cuenta_puc
                                                             on cuentasValores.cuenta equals cuentaP.cntpuc_id
                                                         where cuentaP.cntpuc_numero.Substring(0, 1) == cuenta.cuenta
                                                               && terceroC.tercero_id == tercero.tercero_id
                                                               && cuentasValores.ano >= cuenta.ano && cuentasValores.ano <= cuenta.ano
                                                               && cuentasValores.mes >= cuenta.mes && cuentasValores.mes <= cuenta.mes
                                                         group cuentasValores by new
                                                         {
                                                             cuentasValores.ano,
                                                             cuentasValores.mes,
                                                             cuentasValores.nit /*, cuenta.cntpuc_descp*/
                                                         }
                                                    into g
                                                         select new
                                                         {
                                                             tercero = g.Key.nit,
                                                             anio = g.Key.ano,
                                                             g.Key.mes,
                                                             saldoInicial = g.Sum(x => x.saldo_ini),
                                                             saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                             debito = g.Sum(x => x.debito),
                                                             credito = g.Sum(x => x.credito),
                                                             debitoNiff = g.Sum(x => x.debitoniff),
                                                             creditoNiff = g.Sum(x => x.creditoniff)
                                                         }).FirstOrDefault()
                                            }).Distinct().OrderBy(x => x.tercero_id).ToList();

                    if (nit != null)
                    {
                        foreach (var item in buscarNitsNivel1)
                        {
                            if (item.tercero_id == nit)
                            {
                                listaNits.Add(new CuentaConsulta
                                {
                                    nit = item.tercero_id,
                                    terceroNombre = item.prinom_tercero + " " + item.segnom_tercero + " " +
                                                    item.apellido_tercero + " " + item.segapellido_tercero + " " +
                                                    item.razon_social,
                                    terceroDocumento = item.doc_tercero,
                                    ano = item.total.anio,
                                    mes = item.total.mes,
                                    saldo_ini = item.total.saldoInicial,
                                    saldo_inicial_niff = item.total.saldoInicialNiff,
                                    debito = item.total.debito,
                                    credito = item.total.credito,
                                    debitoNiff = item.total.debitoNiff,
                                    creditoNiff = item.total.creditoNiff,
                                    total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                    totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff -
                                                item.total.creditoNiff
                                });
                            }
                        }

                        CuentaConsulta buscarNit = listaNits.FirstOrDefault(x => x.nit == nit);
                        if (buscarNit == null)
                        {
                            cuenta.visible = false;
                        }
                        else
                        {
                            cuenta.visible = true;
                            cuenta.NitsHijos = listaNits;
                        }
                    }
                    else
                    {
                        foreach (var item in buscarNitsNivel1)
                        {
                            listaNits.Add(new CuentaConsulta
                            {
                                nit = item.tercero_id,
                                terceroNombre = item.prinom_tercero + " " + item.segnom_tercero + " " +
                                                item.apellido_tercero + " " + item.segapellido_tercero + " " +
                                                item.razon_social,
                                terceroDocumento = item.doc_tercero,
                                ano = item.total.anio,
                                mes = item.total.mes,
                                saldo_ini = item.total.saldoInicial,
                                saldo_inicial_niff = item.total.saldoInicialNiff,
                                debito = item.total.debito,
                                credito = item.total.credito,
                                debitoNiff = item.total.debitoNiff,
                                creditoNiff = item.total.creditoNiff,
                                total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff - item.total.creditoNiff
                            });
                        }

                        cuenta.visible = true;
                        cuenta.NitsHijos = listaNits;
                    }
                }
            }

            if (!filtroNit)
            {
                if (nit != null)
                {
                    foreach (CuentaConsulta cuenta in listaCuentas)
                    {
                        List<CuentaConsulta> listaNits = new List<CuentaConsulta>();
                        var buscarNitsNivel1 = (from cuentaValor in context.cuentas_valores
                                                join tercero in context.icb_terceros
                                                    on cuentaValor.nit equals tercero.tercero_id
                                                join cuentaPuc in context.cuenta_puc
                                                    on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                                where cuentaPuc.cntpuc_numero.Substring(0, 1) == cuenta.cuenta
                                                      && cuentaValor.ano == cuenta.ano && cuentaValor.mes == cuenta.mes
                                                select new
                                                {
                                                    tercero.tercero_id,
                                                    tercero.prinom_tercero,
                                                    tercero.segnom_tercero,
                                                    tercero.apellido_tercero,
                                                    tercero.segapellido_tercero,
                                                    tercero.razon_social,
                                                    tercero.doc_tercero,
                                                    total = (from cuentasValores in context.cuentas_valores
                                                             join terceroC in context.icb_terceros
                                                                 on cuentasValores.nit equals terceroC.tercero_id
                                                             join cuentaP in context.cuenta_puc
                                                                 on cuentasValores.cuenta equals cuentaP.cntpuc_id
                                                             where cuentaP.cntpuc_numero.Substring(0, 1) == cuenta.cuenta
                                                                   && terceroC.tercero_id == tercero.tercero_id
                                                                   && cuentasValores.ano >= cuenta.ano && cuentasValores.ano <= cuenta.ano
                                                                   && cuentasValores.mes >= cuenta.mes && cuentasValores.mes <= cuenta.mes
                                                             group cuentasValores by new
                                                             {
                                                                 cuentasValores.ano,
                                                                 cuentasValores.mes,
                                                                 cuentasValores.nit /*, cuenta.cntpuc_descp*/
                                                             }
                                                        into g
                                                             select new
                                                             {
                                                                 tercero = g.Key.nit,
                                                                 anio = g.Key.ano,
                                                                 g.Key.mes,
                                                                 saldoInicial = g.Sum(x => x.saldo_ini),
                                                                 saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                 debito = g.Sum(x => x.debito),
                                                                 credito = g.Sum(x => x.credito),
                                                                 debitoNiff = g.Sum(x => x.debitoniff),
                                                                 creditoNiff = g.Sum(x => x.creditoniff)
                                                             }).FirstOrDefault()
                                                }).Distinct().OrderBy(x => x.tercero_id).ToList();

                        foreach (var item in buscarNitsNivel1)
                        {
                            listaNits.Add(new CuentaConsulta
                            {
                                nit = item.tercero_id,
                                terceroNombre = item.prinom_tercero + " " + item.segnom_tercero + " " +
                                                item.apellido_tercero + " " + item.segapellido_tercero + " " +
                                                item.razon_social,
                                terceroDocumento = item.doc_tercero,
                                ano = item.total.anio,
                                mes = item.total.mes,
                                saldo_ini = item.total.saldoInicial,
                                saldo_inicial_niff = item.total.saldoInicialNiff,
                                debito = item.total.debito,
                                credito = item.total.credito,
                                debitoNiff = item.total.debitoNiff,
                                creditoNiff = item.total.creditoNiff,
                                total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff - item.total.creditoNiff
                            });
                        }

                        CuentaConsulta buscarNit = listaNits.FirstOrDefault(x => x.nit == nit);
                        if (buscarNit == null)
                        {
                            cuenta.visible = false;
                        }
                        else
                        {
                            cuenta.visible = true;
                        }
                    }
                }
            }

            return listaCuentas;
        }

        public List<CuentaConsulta> getNivel2(List<string> indices, DateTime FechaInicio, DateTime FechaFin,
            List<CuentaConsulta> cuentasNivelAnterior, bool filtroCentro, bool filtroNit, int? nit)
        {
            foreach (CuentaConsulta cuentaNivelAnterior in cuentasNivelAnterior)
            {
                var indicesNivel2 = (from cuenta in context.cuenta_puc
                                     where cuenta.cntpuc_numero.Length == 2 &&
                                           cuenta.cntpuc_numero.Substring(0, 1) == cuentaNivelAnterior.cuenta
                                     select new { cuenta.cntpuc_numero, cuenta.cntpuc_descp, cuenta.esafectable }).ToList();
                List<string> indicesNumericos = indicesNivel2.Select(x => x.cntpuc_numero).ToList();
                List<CuentaConsulta> listaCuentas = new List<CuentaConsulta>();
                var buscarCuentasNivel2 = (from cuentaValor in context.cuentas_valores
                                           join cuentaPuc in context.cuenta_puc
                                               on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                           where cuentaPuc.cntpuc_numero.Substring(0, 1) == cuentaNivelAnterior.cuenta
                                                 && indicesNumericos.Contains(cuentaPuc.cntpuc_numero.Substring(0, 2))
                                                 && cuentaValor.ano == cuentaNivelAnterior.ano && cuentaValor.mes == cuentaNivelAnterior.mes
                                           select new
                                           {
                                               nivel = cuentaPuc.cntpuc_numero.Substring(0, 2),

                                               total = (from cuentasValores in context.cuentas_valores
                                                        join cuenta in context.cuenta_puc
                                                            on cuentasValores.cuenta equals cuenta.cntpuc_id
                                                        where cuenta.cntpuc_numero.Substring(0, 2) == cuentaPuc.cntpuc_numero.Substring(0, 2)
                                                              && cuentasValores.ano >= cuentaNivelAnterior.ano &&
                                                              cuentasValores.ano <= cuentaNivelAnterior.ano
                                                              && cuentasValores.mes >= cuentaNivelAnterior.mes &&
                                                              cuentasValores.mes <= cuentaNivelAnterior.mes
                                                        group cuentasValores by new
                                                        { cuentasValores.ano, cuentasValores.mes /*, cuenta.cntpuc_numero, cuenta.cntpuc_descp*/}
                                                   into g
                                                        select new
                                                        {
                                                            anio = g.Key.ano,
                                                            g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                            saldoInicial = g.Sum(x => x.saldo_ini),
                                                            saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                            debito = g.Sum(x => x.debito),
                                                            credito = g.Sum(x => x.credito),
                                                            debitoNiff = g.Sum(x => x.debitoniff),
                                                            creditoNiff = g.Sum(x => x.creditoniff)
                                                        }).FirstOrDefault()
                                           }).Distinct().OrderBy(x => x.nivel).ToList();


                foreach (var item in buscarCuentasNivel2)
                {
                    listaCuentas.Add(new CuentaConsulta
                    {
                        cuenta = item.nivel,
                        esAfectable = indicesNivel2.FirstOrDefault(x => x.cntpuc_numero == item.nivel).esafectable,
                        cuentaDescripcion = indicesNivel2.FirstOrDefault(x => x.cntpuc_numero == item.nivel)
                            .cntpuc_descp,
                        ano = item.total.anio,
                        mes = item.total.mes,
                        saldo_ini = item.total.saldoInicial,
                        saldo_inicial_niff = item.total.saldoInicialNiff,
                        debito = item.total.debito,
                        credito = item.total.credito,
                        debitoNiff = item.total.debitoNiff,
                        creditoNiff = item.total.creditoNiff,
                        total = item.total.saldoInicial + item.total.debito - item.total.credito,
                        totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff - item.total.creditoNiff
                    });
                }

                cuentaNivelAnterior.cuentasHijas = listaCuentas;

                if (filtroCentro)
                {
                    foreach (CuentaConsulta cuenta in cuentaNivelAnterior.cuentasHijas)
                    {
                        List<CuentaConsulta> listaCentros = new List<CuentaConsulta>();
                        var buscarCentrosNivel2 = (from cuentaValor in context.cuentas_valores
                                                   join centro in context.centro_costo
                                                       on cuentaValor.centro equals centro.centcst_id
                                                   join cuentaPuc in context.cuenta_puc
                                                       on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                                   where cuentaPuc.cntpuc_numero.Substring(0, 2) == cuenta.cuenta
                                                         && cuentaValor.ano == cuenta.ano && cuentaValor.mes == cuenta.mes
                                                   select new
                                                   {
                                                       centro.centcst_id,
                                                       centro.centcst_nombre,
                                                       centro.pre_centcst,

                                                       total = (from cuentasValores in context.cuentas_valores
                                                                join centroC in context.centro_costo
                                                                    on cuentasValores.centro equals centroC.centcst_id
                                                                join cuentaP in context.cuenta_puc
                                                                    on cuentasValores.cuenta equals cuentaP.cntpuc_id
                                                                where cuentaP.cntpuc_numero.Substring(0, 2) == cuenta.cuenta
                                                                      && centroC.centcst_id == centro.centcst_id
                                                                      && cuentasValores.ano >= cuenta.ano && cuentasValores.ano <= cuenta.ano
                                                                      && cuentasValores.mes >= cuenta.mes && cuentasValores.mes <= cuenta.mes
                                                                group cuentasValores by new
                                                                {
                                                                    cuentasValores.ano,
                                                                    cuentasValores.mes,
                                                                    cuentasValores.centro /*, cuenta.cntpuc_descp*/
                                                                }
                                                           into g
                                                                select new
                                                                {
                                                                    g.Key.centro,
                                                                    anio = g.Key.ano,
                                                                    g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                                    saldoInicial = g.Sum(x => x.saldo_ini),
                                                                    saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                    debito = g.Sum(x => x.debito),
                                                                    credito = g.Sum(x => x.credito),
                                                                    debitoNiff = g.Sum(x => x.debitoniff),
                                                                    creditoNiff = g.Sum(x => x.creditoniff)
                                                                }).FirstOrDefault()
                                                   }).Distinct().OrderBy(x => x.centcst_id).ToList();

                        foreach (var item2 in buscarCentrosNivel2)
                        {
                            listaCentros.Add(new CuentaConsulta
                            {
                                //cuentaId = item.cntpuc_id,
                                centroPrefijo = item2.pre_centcst,
                                centro = item2.centcst_nombre,
                                ano = item2.total.anio,
                                mes = item2.total.mes,
                                saldo_ini = item2.total.saldoInicial,
                                saldo_inicial_niff = item2.total.saldoInicialNiff,
                                debito = item2.total.debito,
                                credito = item2.total.credito,
                                debitoNiff = item2.total.debitoNiff,
                                creditoNiff = item2.total.creditoNiff,
                                total = item2.total.saldoInicial + item2.total.debito - item2.total.credito,
                                totalNiff = item2.total.saldoInicialNiff + item2.total.debitoNiff -
                                            item2.total.creditoNiff
                            });
                        }

                        cuenta.centrosHijos = listaCentros;
                    }
                }

                if (filtroNit)
                {
                    foreach (CuentaConsulta cuenta in cuentaNivelAnterior.cuentasHijas)
                    {
                        List<CuentaConsulta> listaNits = new List<CuentaConsulta>();
                        var buscarNitsNivel2 = (from cuentaValor in context.cuentas_valores
                                                join tercero in context.icb_terceros
                                                    on cuentaValor.nit equals tercero.tercero_id
                                                join cuentaPuc in context.cuenta_puc
                                                    on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                                where cuentaPuc.cntpuc_numero.Substring(0, 2) == cuenta.cuenta
                                                      && cuentaValor.ano == cuenta.ano && cuentaValor.mes == cuenta.mes
                                                select new
                                                {
                                                    tercero.tercero_id,
                                                    tercero.prinom_tercero,
                                                    tercero.segnom_tercero,
                                                    tercero.apellido_tercero,
                                                    tercero.segapellido_tercero,
                                                    tercero.razon_social,
                                                    tercero.doc_tercero,
                                                    total = (from cuentasValores in context.cuentas_valores
                                                             join terceroC in context.icb_terceros
                                                                 on cuentasValores.nit equals terceroC.tercero_id
                                                             join cuentaP in context.cuenta_puc
                                                                 on cuentasValores.cuenta equals cuentaP.cntpuc_id
                                                             where cuentaP.cntpuc_numero.Substring(0, 2) == cuenta.cuenta
                                                                   && terceroC.tercero_id == tercero.tercero_id
                                                                   && cuentasValores.ano >= cuenta.ano && cuentasValores.ano <= cuenta.ano
                                                                   && cuentasValores.mes >= cuenta.mes && cuentasValores.mes <= cuenta.mes
                                                             group cuentasValores by new
                                                             {
                                                                 cuentasValores.ano,
                                                                 cuentasValores.mes,
                                                                 cuentasValores.nit /*, cuenta.cntpuc_descp*/
                                                             }
                                                        into g
                                                             select new
                                                             {
                                                                 tercero = g.Key.nit,
                                                                 anio = g.Key.ano,
                                                                 g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                                 saldoInicial = g.Sum(x => x.saldo_ini),
                                                                 saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                 debito = g.Sum(x => x.debito),
                                                                 credito = g.Sum(x => x.credito),
                                                                 debitoNiff = g.Sum(x => x.debitoniff),
                                                                 creditoNiff = g.Sum(x => x.creditoniff)
                                                             }).FirstOrDefault()
                                                }).Distinct().OrderBy(x => x.tercero_id).ToList();

                        if (nit != null)
                        {
                            foreach (var item in buscarNitsNivel2)
                            {
                                if (item.tercero_id == nit)
                                {
                                    listaNits.Add(new CuentaConsulta
                                    {
                                        nit = item.tercero_id,
                                        terceroNombre = item.prinom_tercero + " " + item.segnom_tercero + " " +
                                                        item.apellido_tercero + " " + item.segapellido_tercero + " " +
                                                        item.razon_social,
                                        terceroDocumento = item.doc_tercero,
                                        ano = item.total.anio,
                                        mes = item.total.mes,
                                        saldo_ini = item.total.saldoInicial,
                                        saldo_inicial_niff = item.total.saldoInicialNiff,
                                        debito = item.total.debito,
                                        credito = item.total.credito,
                                        debitoNiff = item.total.debitoNiff,
                                        creditoNiff = item.total.creditoNiff,
                                        total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                        totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff -
                                                    item.total.creditoNiff
                                    });
                                }
                            }

                            CuentaConsulta buscarNit = listaNits.FirstOrDefault(x => x.nit == nit);
                            if (buscarNit == null)
                            {
                                cuenta.visible = false;
                            }
                            else
                            {
                                cuenta.visible = true;
                                cuenta.NitsHijos = listaNits;
                            }
                        }
                        else
                        {
                            foreach (var item in buscarNitsNivel2)
                            {
                                listaNits.Add(new CuentaConsulta
                                {
                                    nit = item.tercero_id,
                                    terceroNombre = item.prinom_tercero + " " + item.segnom_tercero + " " +
                                                    item.apellido_tercero + " " + item.segapellido_tercero + " " +
                                                    item.razon_social,
                                    terceroDocumento = item.doc_tercero,
                                    ano = item.total.anio,
                                    mes = item.total.mes,
                                    saldo_ini = item.total.saldoInicial,
                                    saldo_inicial_niff = item.total.saldoInicialNiff,
                                    debito = item.total.debito,
                                    credito = item.total.credito,
                                    debitoNiff = item.total.debitoNiff,
                                    creditoNiff = item.total.creditoNiff,
                                    total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                    totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff -
                                                item.total.creditoNiff
                                });
                            }

                            cuenta.visible = true;
                            cuenta.NitsHijos = listaNits;
                        }
                    }
                }

                if (!filtroNit)
                {
                    if (nit != null)
                    {
                        foreach (CuentaConsulta cuenta in listaCuentas)
                        {
                            List<CuentaConsulta> listaNits = new List<CuentaConsulta>();
                            var buscarNitsNivel2 = (from cuentaValor in context.cuentas_valores
                                                    join tercero in context.icb_terceros
                                                        on cuentaValor.nit equals tercero.tercero_id
                                                    join cuentaPuc in context.cuenta_puc
                                                        on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                                    where cuentaPuc.cntpuc_numero.Substring(0, 2) == cuenta.cuenta
                                                          && cuentaValor.ano == cuenta.ano && cuentaValor.mes == cuenta.mes
                                                    select new
                                                    {
                                                        tercero.tercero_id,
                                                        tercero.prinom_tercero,
                                                        tercero.segnom_tercero,
                                                        tercero.apellido_tercero,
                                                        tercero.segapellido_tercero,
                                                        tercero.razon_social,
                                                        tercero.doc_tercero,
                                                        total = (from cuentasValores in context.cuentas_valores
                                                                 join terceroC in context.icb_terceros
                                                                     on cuentasValores.nit equals terceroC.tercero_id
                                                                 join cuentaP in context.cuenta_puc
                                                                     on cuentasValores.cuenta equals cuentaP.cntpuc_id
                                                                 where cuentaP.cntpuc_numero.Substring(0, 2) == cuenta.cuenta
                                                                       && terceroC.tercero_id == tercero.tercero_id
                                                                       && cuentasValores.ano >= cuenta.ano && cuentasValores.ano <= cuenta.ano
                                                                       && cuentasValores.mes >= cuenta.mes && cuentasValores.mes <= cuenta.mes
                                                                 group cuentasValores by new
                                                                 {
                                                                     cuentasValores.ano,
                                                                     cuentasValores.mes,
                                                                     cuentasValores.nit /*, cuenta.cntpuc_descp*/
                                                                 }
                                                            into g
                                                                 select new
                                                                 {
                                                                     tercero = g.Key.nit,
                                                                     anio = g.Key.ano,
                                                                     g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                                     saldoInicial = g.Sum(x => x.saldo_ini),
                                                                     saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                     debito = g.Sum(x => x.debito),
                                                                     credito = g.Sum(x => x.credito),
                                                                     debitoNiff = g.Sum(x => x.debitoniff),
                                                                     creditoNiff = g.Sum(x => x.creditoniff)
                                                                 }).FirstOrDefault()
                                                    }).Distinct().OrderBy(x => x.tercero_id).ToList();

                            foreach (var item in buscarNitsNivel2)
                            {
                                listaNits.Add(new CuentaConsulta
                                {
                                    nit = item.tercero_id,
                                    terceroNombre = item.prinom_tercero + " " + item.segnom_tercero + " " +
                                                    item.apellido_tercero + " " + item.segapellido_tercero + " " +
                                                    item.razon_social,
                                    terceroDocumento = item.doc_tercero,
                                    ano = item.total.anio,
                                    mes = item.total.mes,
                                    saldo_ini = item.total.saldoInicial,
                                    saldo_inicial_niff = item.total.saldoInicialNiff,
                                    debito = item.total.debito,
                                    credito = item.total.credito,
                                    debitoNiff = item.total.debitoNiff,
                                    creditoNiff = item.total.creditoNiff,
                                    total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                    totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff -
                                                item.total.creditoNiff
                                });
                            }

                            CuentaConsulta buscarNit = listaNits.FirstOrDefault(x => x.nit == nit);
                            if (buscarNit == null)
                            {
                                cuenta.visible = false;
                            }
                            else
                            {
                                cuenta.visible = true;
                            }
                        }
                    }
                }
            }


            return cuentasNivelAnterior;
        }

        public List<CuentaConsulta> getNivel3(List<string> indices, DateTime FechaInicio, DateTime FechaFin,
            List<CuentaConsulta> cuentasNivelAnterior, bool filtroCentro, bool filtroNit, int? nit)
        {
            foreach (CuentaConsulta cuentaNivelPadre in cuentasNivelAnterior)
            {
                foreach (CuentaConsulta cuentaNivelAnterior in cuentaNivelPadre.cuentasHijas)
                {
                    var indicesNivel2 = (from cuenta in context.cuenta_puc
                                         where cuenta.cntpuc_numero.Length == 4 &&
                                               cuenta.cntpuc_numero.Substring(0, 2) == cuentaNivelAnterior.cuenta
                                         select new { cuenta.cntpuc_numero, cuenta.cntpuc_descp, cuenta.esafectable }).ToList();
                    List<string> indicesNumericos = indicesNivel2.Select(x => x.cntpuc_numero).ToList();
                    List<CuentaConsulta> listaCuentas = new List<CuentaConsulta>();
                    var buscarCuentasNivel2 = (from cuentaValor in context.cuentas_valores
                                               join cuentaPuc in context.cuenta_puc
                                                   on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                               where cuentaPuc.cntpuc_numero.Substring(0, 2) == cuentaNivelAnterior.cuenta
                                                     && indicesNumericos.Contains(cuentaPuc.cntpuc_numero.Substring(0, 4))
                                                     && cuentaValor.ano == cuentaNivelAnterior.ano && cuentaValor.mes == cuentaNivelAnterior.mes
                                               select new
                                               {
                                                   nivel = cuentaPuc.cntpuc_numero.Substring(0, 4),

                                                   total = (from cuentasValores in context.cuentas_valores
                                                            join cuenta in context.cuenta_puc
                                                                on cuentasValores.cuenta equals cuenta.cntpuc_id
                                                            where cuenta.cntpuc_numero.Substring(0, 4) == cuentaPuc.cntpuc_numero.Substring(0, 4)
                                                                  && cuentasValores.ano >= cuentaNivelAnterior.ano &&
                                                                  cuentasValores.ano <= cuentaNivelAnterior.ano
                                                                  && cuentasValores.mes >= cuentaNivelAnterior.mes &&
                                                                  cuentasValores.mes <= cuentaNivelAnterior.mes
                                                            group cuentasValores by new
                                                            { cuentasValores.ano, cuentasValores.mes /*, cuenta.cntpuc_numero, cuenta.cntpuc_descp*/}
                                                       into g
                                                            select new
                                                            {
                                                                anio = g.Key.ano,
                                                                g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                                saldoInicial = g.Sum(x => x.saldo_ini),
                                                                saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                debito = g.Sum(x => x.debito),
                                                                credito = g.Sum(x => x.credito),
                                                                debitoNiff = g.Sum(x => x.debitoniff),
                                                                creditoNiff = g.Sum(x => x.creditoniff)
                                                            }).FirstOrDefault()
                                               }).Distinct().OrderBy(x => x.nivel).ToList();


                    foreach (var item in buscarCuentasNivel2)
                    {
                        listaCuentas.Add(new CuentaConsulta
                        {
                            //cuentaId = item.cntpuc_id,
                            cuenta = item.nivel,
                            esAfectable = indicesNivel2.FirstOrDefault(x => x.cntpuc_numero == item.nivel).esafectable,
                            cuentaDescripcion = indicesNivel2.FirstOrDefault(x => x.cntpuc_numero == item.nivel)
                                .cntpuc_descp,
                            ano = item.total.anio,
                            mes = item.total.mes,
                            saldo_ini = item.total.saldoInicial,
                            saldo_inicial_niff = item.total.saldoInicialNiff,
                            debito = item.total.debito,
                            credito = item.total.credito,
                            debitoNiff = item.total.debitoNiff,
                            creditoNiff = item.total.creditoNiff,
                            total = item.total.saldoInicial + item.total.debito - item.total.credito,
                            totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff - item.total.creditoNiff
                        });
                    }
                    //}
                    cuentaNivelAnterior.cuentasHijas = listaCuentas;


                    if (filtroCentro)
                    {
                        foreach (CuentaConsulta cuenta in cuentaNivelAnterior.cuentasHijas)
                        {
                            List<CuentaConsulta> listaCentros = new List<CuentaConsulta>();
                            var buscarCentrosNivel3 = (from cuentaValor in context.cuentas_valores
                                                       join centro in context.centro_costo
                                                           on cuentaValor.centro equals centro.centcst_id
                                                       join cuentaPuc in context.cuenta_puc
                                                           on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                                       where cuentaPuc.cntpuc_numero.Substring(0, 4) == cuenta.cuenta
                                                             && cuentaValor.ano == cuenta.ano && cuentaValor.mes == cuenta.mes
                                                       select new
                                                       {
                                                           //cuentaPuc.cntpuc_id,
                                                           centro.centcst_id,
                                                           centro.centcst_nombre,
                                                           centro.pre_centcst,
                                                           total = (from cuentasValores in context.cuentas_valores
                                                                    join centroC in context.centro_costo
                                                                        on cuentasValores.centro equals centroC.centcst_id
                                                                    join cuentaP in context.cuenta_puc
                                                                        on cuentasValores.cuenta equals cuentaP.cntpuc_id
                                                                    where cuentaP.cntpuc_numero.Substring(0, 4) == cuenta.cuenta
                                                                          && centroC.centcst_id == centro.centcst_id
                                                                          && cuentasValores.ano >= cuenta.ano && cuentasValores.ano <= cuenta.ano
                                                                          && cuentasValores.mes >= cuenta.mes && cuentasValores.mes <= cuenta.mes
                                                                    group cuentasValores by new
                                                                    {
                                                                        cuentasValores.ano,
                                                                        cuentasValores.mes,
                                                                        cuentasValores.centro /*, cuenta.cntpuc_descp*/
                                                                    }
                                                               into g
                                                                    select new
                                                                    {
                                                                        g.Key.centro,
                                                                        anio = g.Key.ano,
                                                                        g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                                        saldoInicial = g.Sum(x => x.saldo_ini),
                                                                        saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                        debito = g.Sum(x => x.debito),
                                                                        credito = g.Sum(x => x.credito),
                                                                        debitoNiff = g.Sum(x => x.debitoniff),
                                                                        creditoNiff = g.Sum(x => x.creditoniff)
                                                                    }).FirstOrDefault()
                                                       }).Distinct().OrderBy(x => x.centcst_id).ToList();

                            foreach (var item3 in buscarCentrosNivel3)
                            {
                                listaCentros.Add(new CuentaConsulta
                                {
                                    centroPrefijo = item3.pre_centcst,
                                    centro = item3.centcst_nombre,
                                    ano = item3.total.anio,
                                    mes = item3.total.mes,
                                    saldo_ini = item3.total.saldoInicial,
                                    saldo_inicial_niff = item3.total.saldoInicialNiff,
                                    debito = item3.total.debito,
                                    credito = item3.total.credito,
                                    debitoNiff = item3.total.debitoNiff,
                                    creditoNiff = item3.total.creditoNiff,
                                    total = item3.total.saldoInicial + item3.total.debito - item3.total.credito,
                                    totalNiff = item3.total.saldoInicialNiff + item3.total.debitoNiff -
                                                item3.total.creditoNiff
                                });
                            }

                            cuenta.centrosHijos = listaCentros;
                        }
                    }

                    if (filtroNit)
                    {
                        foreach (CuentaConsulta cuenta in cuentaNivelAnterior.cuentasHijas)
                        {
                            List<CuentaConsulta> listaNits = new List<CuentaConsulta>();
                            var buscarNitsNivel3 = (from cuentaValor in context.cuentas_valores
                                                    join tercero in context.icb_terceros
                                                        on cuentaValor.nit equals tercero.tercero_id
                                                    join cuentaPuc in context.cuenta_puc
                                                        on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                                    where cuentaPuc.cntpuc_numero.Substring(0, 4) == cuenta.cuenta
                                                          && cuentaValor.ano == cuenta.ano && cuentaValor.mes == cuenta.mes
                                                    select new
                                                    {
                                                        tercero.tercero_id,
                                                        tercero.prinom_tercero,
                                                        tercero.segnom_tercero,
                                                        tercero.apellido_tercero,
                                                        tercero.segapellido_tercero,
                                                        tercero.razon_social,
                                                        tercero.doc_tercero,
                                                        total = (from cuentasValores in context.cuentas_valores
                                                                 join terceroC in context.icb_terceros
                                                                     on cuentasValores.nit equals terceroC.tercero_id
                                                                 join cuentaP in context.cuenta_puc
                                                                     on cuentasValores.cuenta equals cuentaP.cntpuc_id
                                                                 where cuentaP.cntpuc_numero.Substring(0, 4) == cuenta.cuenta
                                                                       && terceroC.tercero_id == tercero.tercero_id
                                                                       && cuentasValores.ano >= cuenta.ano && cuentasValores.ano <= cuenta.ano
                                                                       && cuentasValores.mes >= cuenta.mes && cuentasValores.mes <= cuenta.mes
                                                                 group cuentasValores by new
                                                                 {
                                                                     cuentasValores.ano,
                                                                     cuentasValores.mes,
                                                                     cuentasValores.nit /*, cuenta.cntpuc_descp*/
                                                                 }
                                                            into g
                                                                 select new
                                                                 {
                                                                     tercero = g.Key.nit,
                                                                     anio = g.Key.ano,
                                                                     g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                                     saldoInicial = g.Sum(x => x.saldo_ini),
                                                                     saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                     debito = g.Sum(x => x.debito),
                                                                     credito = g.Sum(x => x.credito),
                                                                     debitoNiff = g.Sum(x => x.debitoniff),
                                                                     creditoNiff = g.Sum(x => x.creditoniff)
                                                                 }).FirstOrDefault()
                                                    }).Distinct().OrderBy(x => x.tercero_id).ToList();

                            if (nit != null)
                            {
                                foreach (var item in buscarNitsNivel3)
                                {
                                    if (item.tercero_id == nit)
                                    {
                                        listaNits.Add(new CuentaConsulta
                                        {
                                            nit = item.tercero_id,
                                            terceroNombre = item.prinom_tercero + " " + item.segnom_tercero + " " +
                                                            item.apellido_tercero + " " + item.segapellido_tercero + " " +
                                                            item.razon_social,
                                            terceroDocumento = item.doc_tercero,
                                            ano = item.total.anio,
                                            mes = item.total.mes,
                                            saldo_ini = item.total.saldoInicial,
                                            saldo_inicial_niff = item.total.saldoInicialNiff,
                                            debito = item.total.debito,
                                            credito = item.total.credito,
                                            debitoNiff = item.total.debitoNiff,
                                            creditoNiff = item.total.creditoNiff,
                                            total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                            totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff -
                                                        item.total.creditoNiff
                                        });
                                    }
                                }

                                CuentaConsulta buscarNit = listaNits.FirstOrDefault(x => x.nit == nit);
                                if (buscarNit == null)
                                {
                                    cuenta.visible = false;
                                }
                                else
                                {
                                    cuenta.visible = true;
                                    cuenta.NitsHijos = listaNits;
                                }
                            }
                            else
                            {
                                foreach (var item in buscarNitsNivel3)
                                {
                                    listaNits.Add(new CuentaConsulta
                                    {
                                        nit = item.tercero_id,
                                        terceroNombre = item.prinom_tercero + " " + item.segnom_tercero + " " +
                                                        item.apellido_tercero + " " + item.segapellido_tercero + " " +
                                                        item.razon_social,
                                        terceroDocumento = item.doc_tercero,
                                        ano = item.total.anio,
                                        mes = item.total.mes,
                                        saldo_ini = item.total.saldoInicial,
                                        saldo_inicial_niff = item.total.saldoInicialNiff,
                                        debito = item.total.debito,
                                        credito = item.total.credito,
                                        debitoNiff = item.total.debitoNiff,
                                        creditoNiff = item.total.creditoNiff,
                                        total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                        totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff -
                                                    item.total.creditoNiff
                                    });
                                }

                                cuenta.visible = true;
                                cuenta.NitsHijos = listaNits;
                            }
                        }
                    }

                    if (!filtroNit)
                    {
                        if (nit != null)
                        {
                            foreach (CuentaConsulta cuenta in listaCuentas)
                            {
                                List<CuentaConsulta> listaNits = new List<CuentaConsulta>();
                                var buscarNitsNivel3 = (from cuentaValor in context.cuentas_valores
                                                        join tercero in context.icb_terceros
                                                            on cuentaValor.nit equals tercero.tercero_id
                                                        join cuentaPuc in context.cuenta_puc
                                                            on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                                        where /*cuentaValor.cuenta || cuentaPuc.cntpuc_numero.Length == 2*/
                                                            cuentaPuc.cntpuc_numero.Substring(0, 4) == cuenta.cuenta
                                                            //where /*(cuentaPuc.cntpuc_numero.Length == 2) &&*/
                                                            //&& indicesNumericos.Contains(cuentaPuc.cntpuc_numero.Substring(0, 1))
                                                            && cuentaValor.ano == cuenta.ano && cuentaValor.mes == cuenta.mes
                                                        //&& cuentaValor.ano >= FechaInicio.Year && cuentaValor.ano <= FechaFin.Year && cuentaValor.mes >= FechaInicio.Month && cuentaValor.mes <= FechaFin.Month
                                                        select new
                                                        {
                                                            tercero.tercero_id,
                                                            tercero.prinom_tercero,
                                                            tercero.segnom_tercero,
                                                            tercero.apellido_tercero,
                                                            tercero.segapellido_tercero,
                                                            tercero.razon_social,
                                                            tercero.doc_tercero,
                                                            total = (from cuentasValores in context.cuentas_valores
                                                                     join terceroC in context.icb_terceros
                                                                         on cuentasValores.nit equals terceroC.tercero_id
                                                                     join cuentaP in context.cuenta_puc
                                                                         on cuentasValores.cuenta equals cuentaP.cntpuc_id
                                                                     where cuentaP.cntpuc_numero.Substring(0, 4) == cuenta.cuenta
                                                                           && terceroC.tercero_id == tercero.tercero_id
                                                                           && cuentasValores.ano >= cuenta.ano && cuentasValores.ano <= cuenta.ano
                                                                           && cuentasValores.mes >= cuenta.mes && cuentasValores.mes <= cuenta.mes
                                                                     group cuentasValores by new
                                                                     {
                                                                         cuentasValores.ano,
                                                                         cuentasValores.mes,
                                                                         cuentasValores.nit /*, cuenta.cntpuc_descp*/
                                                                     }
                                                                into g
                                                                     select new
                                                                     {
                                                                         tercero = g.Key.nit,
                                                                         anio = g.Key.ano,
                                                                         g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                                         saldoInicial = g.Sum(x => x.saldo_ini),
                                                                         saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                         debito = g.Sum(x => x.debito),
                                                                         credito = g.Sum(x => x.credito),
                                                                         debitoNiff = g.Sum(x => x.debitoniff),
                                                                         creditoNiff = g.Sum(x => x.creditoniff)
                                                                     }).FirstOrDefault()
                                                        }).Distinct().OrderBy(x => x.tercero_id).ToList();

                                foreach (var item in buscarNitsNivel3)
                                {
                                    listaNits.Add(new CuentaConsulta
                                    {
                                        nit = item.tercero_id,
                                        terceroNombre = item.prinom_tercero + " " + item.segnom_tercero + " " +
                                                        item.apellido_tercero + " " + item.segapellido_tercero + " " +
                                                        item.razon_social,
                                        terceroDocumento = item.doc_tercero,
                                        ano = item.total.anio,
                                        mes = item.total.mes,
                                        saldo_ini = item.total.saldoInicial,
                                        saldo_inicial_niff = item.total.saldoInicialNiff,
                                        debito = item.total.debito,
                                        credito = item.total.credito,
                                        debitoNiff = item.total.debitoNiff,
                                        creditoNiff = item.total.creditoNiff,
                                        total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                        totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff -
                                                    item.total.creditoNiff
                                    });
                                }

                                CuentaConsulta buscarNit = listaNits.FirstOrDefault(x => x.nit == nit);
                                if (buscarNit == null)
                                {
                                    cuenta.visible = false;
                                }
                                else
                                {
                                    cuenta.visible = true;
                                }
                                //cuenta.NitsHijos = listaNits;
                            }
                        }
                    }
                }
            }

            return cuentasNivelAnterior;
        }

        public List<CuentaConsulta> getNivel4(List<string> indices, DateTime FechaInicio, DateTime FechaFin,
            List<CuentaConsulta> cuentasNivelAnterior, bool filtroCentro, bool filtroNit, int? nit)
        {
            foreach (CuentaConsulta cuentaNivelPadre2 in cuentasNivelAnterior)
            {
                foreach (CuentaConsulta cuentaNivelPadre in cuentaNivelPadre2.cuentasHijas)
                {
                    foreach (CuentaConsulta cuentaNivelAnterior in cuentaNivelPadre.cuentasHijas)
                    {
                        var indicesNivel3 = (from cuenta in context.cuenta_puc
                                             where cuenta.cntpuc_numero.Length == 6 &&
                                                   cuenta.cntpuc_numero.Substring(0, 4) == cuentaNivelAnterior.cuenta
                                             select new { cuenta.cntpuc_numero, cuenta.cntpuc_descp, cuenta.esafectable }).ToList();
                        List<string> indicesNumericos = indicesNivel3.Select(x => x.cntpuc_numero).ToList();
                        List<CuentaConsulta> listaCuentas = new List<CuentaConsulta>();
                        var buscarCuentasNivel4 = (from cuentaValor in context.cuentas_valores
                                                   join cuentaPuc in context.cuenta_puc
                                                       on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                                   where /*cuentaValor.cuenta || cuentaPuc.cntpuc_numero.Length == 2*/
                                                       cuentaPuc.cntpuc_numero.Substring(0, 4) == cuentaNivelAnterior.cuenta
                                                       //where /*(cuentaPuc.cntpuc_numero.Length == 2) &&*/
                                                       && indicesNumericos.Contains(cuentaPuc.cntpuc_numero.Substring(0, 6))
                                                       && cuentaValor.ano == cuentaNivelAnterior.ano && cuentaValor.mes == cuentaNivelAnterior.mes
                                                   select new
                                                   {
                                                       //cuentaPuc.cntpuc_id,
                                                       nivel = cuentaPuc.cntpuc_numero.Substring(0, 6),
                                                       //cuentaPuc.cntpuc_numero,
                                                       //cntpuc_descp = (from indicesNombre in indicesNivel2 where indicesNombre.cntpuc_numero == cuentaPuc.cntpuc_numero.Substring(0, 2) select indicesNombre.cntpuc_descp),
                                                       //cntpuc_descp = indicesNivel2.FirstOrDefault(x=>x.cntpuc_numero == cuentaPuc.cntpuc_numero.Substring(0, 2)).cntpuc_descp,
                                                       total = (from cuentasValores in context.cuentas_valores
                                                                join cuenta in context.cuenta_puc
                                                                    on cuentasValores.cuenta equals cuenta.cntpuc_id
                                                                where cuenta.cntpuc_numero.Substring(0, 6) == cuentaPuc.cntpuc_numero.Substring(0, 6)
                                                                      && cuentasValores.ano >= cuentaNivelAnterior.ano &&
                                                                      cuentasValores.ano <= cuentaNivelAnterior.ano
                                                                      && cuentasValores.mes >= cuentaNivelAnterior.mes &&
                                                                      cuentasValores.mes <= cuentaNivelAnterior.mes
                                                                group cuentasValores by new
                                                                { cuentasValores.ano, cuentasValores.mes /*, cuenta.cntpuc_numero, cuenta.cntpuc_descp*/}
                                                           into g
                                                                select new
                                                                {
                                                                    anio = g.Key.ano,
                                                                    g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                                    saldoInicial = g.Sum(x => x.saldo_ini),
                                                                    saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                    debito = g.Sum(x => x.debito),
                                                                    credito = g.Sum(x => x.credito),
                                                                    debitoNiff = g.Sum(x => x.debitoniff),
                                                                    creditoNiff = g.Sum(x => x.creditoniff)
                                                                }).FirstOrDefault()
                                                   }).Distinct().OrderBy(x => x.nivel).ToList();


                        foreach (var item in buscarCuentasNivel4)
                        {
                            //foreach (var items2 in item.total)
                            //{
                            listaCuentas.Add(new CuentaConsulta
                            {
                                //cuentaId = item.cntpuc_id,
                                cuenta = item.nivel,
                                esAfectable = indicesNivel3.FirstOrDefault(x => x.cntpuc_numero == item.nivel).esafectable,
                                cuentaDescripcion = indicesNivel3.FirstOrDefault(x => x.cntpuc_numero == item.nivel)
                                    .cntpuc_descp,
                                ano = item.total.anio,
                                mes = item.total.mes,
                                saldo_ini = item.total.saldoInicial,
                                saldo_inicial_niff = item.total.saldoInicialNiff,
                                debito = item.total.debito,
                                credito = item.total.credito,
                                debitoNiff = item.total.debitoNiff,
                                creditoNiff = item.total.creditoNiff,
                                total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff - item.total.creditoNiff
                            });
                        }
                        //}
                        cuentaNivelAnterior.cuentasHijas = listaCuentas;

                        if (filtroCentro)
                        {
                            foreach (CuentaConsulta cuenta in cuentaNivelAnterior.cuentasHijas)
                            {
                                List<CuentaConsulta> listaCentros = new List<CuentaConsulta>();
                                var buscarCentrosNivel4 = (from cuentaValor in context.cuentas_valores
                                                           join centro in context.centro_costo
                                                               on cuentaValor.centro equals centro.centcst_id
                                                           join cuentaPuc in context.cuenta_puc
                                                               on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                                           where /*cuentaValor.cuenta || cuentaPuc.cntpuc_numero.Length == 2*/
                                                               cuentaPuc.cntpuc_numero.Substring(0, 6) == cuenta.cuenta
                                                               //where /*(cuentaPuc.cntpuc_numero.Length == 2) &&*/
                                                               //&& indicesNumericos.Contains(cuentaPuc.cntpuc_numero.Substring(0, 1))
                                                               && cuentaValor.ano == cuenta.ano && cuentaValor.mes == cuenta.mes
                                                           //&& cuentaValor.ano >= FechaInicio.Year && cuentaValor.ano <= FechaFin.Year && cuentaValor.mes >= FechaInicio.Month && cuentaValor.mes <= FechaFin.Month
                                                           select new
                                                           {
                                                               //cuentaPuc.cntpuc_id,
                                                               centro.centcst_id,
                                                               centro.centcst_nombre,
                                                               centro.pre_centcst,
                                                               //cuentaPuc.cntpuc_numero,
                                                               //cntpuc_descp = (from indicesNombre in indicesNivel2 where indicesNombre.cntpuc_numero == cuentaPuc.cntpuc_numero.Substring(0, 2) select indicesNombre.cntpuc_descp),
                                                               //cntpuc_descp = indicesNivel2.FirstOrDefault(x=>x.cntpuc_numero == cuentaPuc.cntpuc_numero.Substring(0, 2)).cntpuc_descp,
                                                               total = (from cuentasValores in context.cuentas_valores
                                                                        join centroC in context.centro_costo
                                                                            on cuentasValores.centro equals centroC.centcst_id
                                                                        join cuentaP in context.cuenta_puc
                                                                            on cuentasValores.cuenta equals cuentaP.cntpuc_id
                                                                        where cuentaP.cntpuc_numero.Substring(0, 6) == cuenta.cuenta
                                                                              && centroC.centcst_id == centro.centcst_id
                                                                              && cuentasValores.ano >= cuenta.ano && cuentasValores.ano <= cuenta.ano
                                                                              && cuentasValores.mes >= cuenta.mes && cuentasValores.mes <= cuenta.mes
                                                                        group cuentasValores by new
                                                                        {
                                                                            cuentasValores.ano,
                                                                            cuentasValores.mes,
                                                                            cuentasValores.centro /*, cuenta.cntpuc_descp*/
                                                                        }
                                                                   into g
                                                                        select new
                                                                        {
                                                                            g.Key.centro,
                                                                            anio = g.Key.ano,
                                                                            g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                                            saldoInicial = g.Sum(x => x.saldo_ini),
                                                                            saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                            debito = g.Sum(x => x.debito),
                                                                            credito = g.Sum(x => x.credito),
                                                                            debitoNiff = g.Sum(x => x.debitoniff),
                                                                            creditoNiff = g.Sum(x => x.creditoniff)
                                                                        }).FirstOrDefault()
                                                           }).Distinct().OrderBy(x => x.centcst_id).ToList();

                                foreach (var item4 in buscarCentrosNivel4)
                                {
                                    //foreach (var items2 in item.total)
                                    //{
                                    listaCentros.Add(new CuentaConsulta
                                    {
                                        //cuentaId = item.cntpuc_id,
                                        centroPrefijo = item4.pre_centcst,
                                        centro = item4.centcst_nombre,
                                        ano = item4.total.anio,
                                        mes = item4.total.mes,
                                        saldo_ini = item4.total.saldoInicial,
                                        saldo_inicial_niff = item4.total.saldoInicialNiff,
                                        debito = item4.total.debito,
                                        credito = item4.total.credito,
                                        debitoNiff = item4.total.debitoNiff,
                                        creditoNiff = item4.total.creditoNiff,
                                        total = item4.total.saldoInicial + item4.total.debito - item4.total.credito,
                                        totalNiff = item4.total.saldoInicialNiff + item4.total.debitoNiff -
                                                    item4.total.creditoNiff
                                    });
                                }
                                //}
                                cuenta.centrosHijos = listaCentros;
                            }
                        }

                        if (filtroNit)
                        {
                            foreach (CuentaConsulta cuenta in cuentaNivelAnterior.cuentasHijas)
                            {
                                List<CuentaConsulta> listaNits = new List<CuentaConsulta>();
                                var buscarNitsNivel4 = (from cuentaValor in context.cuentas_valores
                                                        join tercero in context.icb_terceros
                                                            on cuentaValor.nit equals tercero.tercero_id
                                                        join cuentaPuc in context.cuenta_puc
                                                            on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                                        where /*cuentaValor.cuenta || cuentaPuc.cntpuc_numero.Length == 2*/
                                                            cuentaPuc.cntpuc_numero.Substring(0, 6) == cuenta.cuenta
                                                            //where /*(cuentaPuc.cntpuc_numero.Length == 2) &&*/
                                                            //&& indicesNumericos.Contains(cuentaPuc.cntpuc_numero.Substring(0, 1))
                                                            && cuentaValor.ano == cuenta.ano && cuentaValor.mes == cuenta.mes
                                                        //&& cuentaValor.ano >= FechaInicio.Year && cuentaValor.ano <= FechaFin.Year && cuentaValor.mes >= FechaInicio.Month && cuentaValor.mes <= FechaFin.Month
                                                        select new
                                                        {
                                                            tercero.tercero_id,
                                                            tercero.prinom_tercero,
                                                            tercero.segnom_tercero,
                                                            tercero.apellido_tercero,
                                                            tercero.segapellido_tercero,
                                                            tercero.razon_social,
                                                            tercero.doc_tercero,
                                                            total = (from cuentasValores in context.cuentas_valores
                                                                     join terceroC in context.icb_terceros
                                                                         on cuentasValores.nit equals terceroC.tercero_id
                                                                     join cuentaP in context.cuenta_puc
                                                                         on cuentasValores.cuenta equals cuentaP.cntpuc_id
                                                                     where cuentaP.cntpuc_numero.Substring(0, 6) == cuenta.cuenta
                                                                           && terceroC.tercero_id == tercero.tercero_id
                                                                           && cuentasValores.ano >= cuenta.ano && cuentasValores.ano <= cuenta.ano
                                                                           && cuentasValores.mes >= cuenta.mes && cuentasValores.mes <= cuenta.mes
                                                                     group cuentasValores by new
                                                                     {
                                                                         cuentasValores.ano,
                                                                         cuentasValores.mes,
                                                                         cuentasValores.nit /*, cuenta.cntpuc_descp*/
                                                                     }
                                                                into g
                                                                     select new
                                                                     {
                                                                         tercero = g.Key.nit,
                                                                         anio = g.Key.ano,
                                                                         g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                                         saldoInicial = g.Sum(x => x.saldo_ini),
                                                                         saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                         debito = g.Sum(x => x.debito),
                                                                         credito = g.Sum(x => x.credito),
                                                                         debitoNiff = g.Sum(x => x.debitoniff),
                                                                         creditoNiff = g.Sum(x => x.creditoniff)
                                                                     }).FirstOrDefault()
                                                        }).Distinct().OrderBy(x => x.tercero_id).ToList();

                                if (nit != null)
                                {
                                    foreach (var item in buscarNitsNivel4)
                                    {
                                        if (item.tercero_id == nit)
                                        {
                                            listaNits.Add(new CuentaConsulta
                                            {
                                                nit = item.tercero_id,
                                                terceroNombre = item.prinom_tercero + " " + item.segnom_tercero + " " +
                                                                item.apellido_tercero + " " + item.segapellido_tercero + " " +
                                                                item.razon_social,
                                                terceroDocumento = item.doc_tercero,
                                                ano = item.total.anio,
                                                mes = item.total.mes,
                                                saldo_ini = item.total.saldoInicial,
                                                saldo_inicial_niff = item.total.saldoInicialNiff,
                                                debito = item.total.debito,
                                                credito = item.total.credito,
                                                debitoNiff = item.total.debitoNiff,
                                                creditoNiff = item.total.creditoNiff,
                                                total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                                totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff -
                                                            item.total.creditoNiff
                                            });
                                        }
                                    }

                                    CuentaConsulta buscarNit = listaNits.FirstOrDefault(x => x.nit == nit);
                                    if (buscarNit == null)
                                    {
                                        cuenta.visible = false;
                                    }
                                    else
                                    {
                                        cuenta.visible = true;
                                        cuenta.NitsHijos = listaNits;
                                    }
                                }
                                else
                                {
                                    foreach (var item in buscarNitsNivel4)
                                    {
                                        listaNits.Add(new CuentaConsulta
                                        {
                                            nit = item.tercero_id,
                                            terceroNombre = item.prinom_tercero + " " + item.segnom_tercero + " " +
                                                            item.apellido_tercero + " " + item.segapellido_tercero + " " +
                                                            item.razon_social,
                                            terceroDocumento = item.doc_tercero,
                                            ano = item.total.anio,
                                            mes = item.total.mes,
                                            saldo_ini = item.total.saldoInicial,
                                            saldo_inicial_niff = item.total.saldoInicialNiff,
                                            debito = item.total.debito,
                                            credito = item.total.credito,
                                            debitoNiff = item.total.debitoNiff,
                                            creditoNiff = item.total.creditoNiff,
                                            total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                            totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff -
                                                        item.total.creditoNiff
                                        });
                                    }

                                    cuenta.visible = true;
                                    cuenta.NitsHijos = listaNits;
                                }
                            }
                        }

                        if (!filtroNit)
                        {
                            if (nit != null)
                            {
                                foreach (CuentaConsulta cuenta in listaCuentas)
                                {
                                    List<CuentaConsulta> listaNits = new List<CuentaConsulta>();
                                    var buscarNitsNivel4 = (from cuentaValor in context.cuentas_valores
                                                            join tercero in context.icb_terceros
                                                                on cuentaValor.nit equals tercero.tercero_id
                                                            join cuentaPuc in context.cuenta_puc
                                                                on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                                            where /*cuentaValor.cuenta || cuentaPuc.cntpuc_numero.Length == 2*/
                                                                cuentaPuc.cntpuc_numero.Substring(0, 6) == cuenta.cuenta
                                                                //where /*(cuentaPuc.cntpuc_numero.Length == 2) &&*/
                                                                //&& indicesNumericos.Contains(cuentaPuc.cntpuc_numero.Substring(0, 1))
                                                                && cuentaValor.ano == cuenta.ano && cuentaValor.mes == cuenta.mes
                                                            //&& cuentaValor.ano >= FechaInicio.Year && cuentaValor.ano <= FechaFin.Year && cuentaValor.mes >= FechaInicio.Month && cuentaValor.mes <= FechaFin.Month
                                                            select new
                                                            {
                                                                tercero.tercero_id,
                                                                tercero.prinom_tercero,
                                                                tercero.segnom_tercero,
                                                                tercero.apellido_tercero,
                                                                tercero.segapellido_tercero,
                                                                tercero.razon_social,
                                                                tercero.doc_tercero,
                                                                total = (from cuentasValores in context.cuentas_valores
                                                                         join terceroC in context.icb_terceros
                                                                             on cuentasValores.nit equals terceroC.tercero_id
                                                                         join cuentaP in context.cuenta_puc
                                                                             on cuentasValores.cuenta equals cuentaP.cntpuc_id
                                                                         where cuentaP.cntpuc_numero.Substring(0, 6) == cuenta.cuenta
                                                                               && terceroC.tercero_id == tercero.tercero_id
                                                                               && cuentasValores.ano >= cuenta.ano && cuentasValores.ano <= cuenta.ano
                                                                               && cuentasValores.mes >= cuenta.mes && cuentasValores.mes <= cuenta.mes
                                                                         group cuentasValores by new
                                                                         {
                                                                             cuentasValores.ano,
                                                                             cuentasValores.mes,
                                                                             cuentasValores.nit /*, cuenta.cntpuc_descp*/
                                                                         }
                                                                    into g
                                                                         select new
                                                                         {
                                                                             tercero = g.Key.nit,
                                                                             anio = g.Key.ano,
                                                                             g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                                             saldoInicial = g.Sum(x => x.saldo_ini),
                                                                             saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                             debito = g.Sum(x => x.debito),
                                                                             credito = g.Sum(x => x.creditoniff),
                                                                             debitoNiff = g.Sum(x => x.debitoniff),
                                                                             creditoNiff = g.Sum(x => x.credito)
                                                                         }).FirstOrDefault()
                                                            }).Distinct().OrderBy(x => x.tercero_id).ToList();

                                    foreach (var item in buscarNitsNivel4)
                                    {
                                        listaNits.Add(new CuentaConsulta
                                        {
                                            nit = item.tercero_id,
                                            terceroNombre = item.prinom_tercero + " " + item.segnom_tercero + " " +
                                                            item.apellido_tercero + " " + item.segapellido_tercero + " " +
                                                            item.razon_social,
                                            terceroDocumento = item.doc_tercero,
                                            ano = item.total.anio,
                                            mes = item.total.mes,
                                            saldo_ini = item.total.saldoInicial,
                                            saldo_inicial_niff = item.total.saldoInicialNiff,
                                            debito = item.total.debito,
                                            credito = item.total.credito,
                                            debitoNiff = item.total.debitoNiff,
                                            creditoNiff = item.total.creditoNiff,
                                            total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                            totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff -
                                                        item.total.creditoNiff
                                        });
                                    }

                                    CuentaConsulta buscarNit = listaNits.FirstOrDefault(x => x.nit == nit);
                                    if (buscarNit == null)
                                    {
                                        cuenta.visible = false;
                                    }
                                    else
                                    {
                                        cuenta.visible = true;
                                    }
                                    //cuenta.NitsHijos = listaNits;
                                }
                            }
                        }
                    }
                }
            }

            return cuentasNivelAnterior;
        }

        public List<CuentaConsulta> getNivel5(List<string> indices, DateTime FechaInicio, DateTime FechaFin,
            List<CuentaConsulta> cuentasNivelAnterior, bool filtroCentro, bool filtroNit, int? nit)
        {
            foreach (CuentaConsulta cuentaNivelPadre2 in cuentasNivelAnterior)
            {
                foreach (CuentaConsulta cuentaNivelPadre in cuentaNivelPadre2.cuentasHijas)
                {
                    foreach (CuentaConsulta cuentaNivelAnterior1 in cuentaNivelPadre.cuentasHijas)
                    {
                        foreach (CuentaConsulta cuentaNivelAnterior in cuentaNivelAnterior1.cuentasHijas)
                        {
                            var indicesNivel4 = (from cuenta in context.cuenta_puc
                                                 where cuenta.cntpuc_numero.Length == 8 &&
                                                       cuenta.cntpuc_numero.Substring(0, 6) == cuentaNivelAnterior.cuenta
                                                 select new { cuenta.cntpuc_numero, cuenta.cntpuc_descp, cuenta.esafectable }).ToList();
                            List<string> indicesNumericos = indicesNivel4.Select(x => x.cntpuc_numero).ToList();
                            List<CuentaConsulta> listaCuentas = new List<CuentaConsulta>();
                            var buscarCuentasNivel5 = (from cuentaValor in context.cuentas_valores
                                                       join cuentaPuc in context.cuenta_puc
                                                           on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                                       where /*cuentaValor.cuenta || cuentaPuc.cntpuc_numero.Length == 2*/
                                                           cuentaPuc.cntpuc_numero.Substring(0, 6) == cuentaNivelAnterior.cuenta
                                                           //where /*(cuentaPuc.cntpuc_numero.Length == 2) &&*/
                                                           && indicesNumericos.Contains(cuentaPuc.cntpuc_numero.Substring(0, 8))
                                                           && cuentaValor.ano == cuentaNivelAnterior.ano && cuentaValor.mes == cuentaNivelAnterior.mes
                                                       select new
                                                       {
                                                           //cuentaPuc.cntpuc_id,
                                                           nivel = cuentaPuc.cntpuc_numero.Substring(0, 8),
                                                           //cuentaPuc.cntpuc_numero,
                                                           //cntpuc_descp = (from indicesNombre in indicesNivel2 where indicesNombre.cntpuc_numero == cuentaPuc.cntpuc_numero.Substring(0, 2) select indicesNombre.cntpuc_descp),
                                                           //cntpuc_descp = indicesNivel2.FirstOrDefault(x=>x.cntpuc_numero == cuentaPuc.cntpuc_numero.Substring(0, 2)).cntpuc_descp,
                                                           total = (from cuentasValores in context.cuentas_valores
                                                                    join cuenta in context.cuenta_puc
                                                                        on cuentasValores.cuenta equals cuenta.cntpuc_id
                                                                    where cuenta.cntpuc_numero.Substring(0, 8) == cuentaPuc.cntpuc_numero.Substring(0, 8)
                                                                          && cuentasValores.ano >= cuentaNivelAnterior.ano &&
                                                                          cuentasValores.ano <= cuentaNivelAnterior.ano
                                                                          && cuentasValores.mes >= cuentaNivelAnterior.mes &&
                                                                          cuentasValores.mes <= cuentaNivelAnterior.mes
                                                                    group cuentasValores by new
                                                                    { cuentasValores.ano, cuentasValores.mes /*, cuenta.cntpuc_numero, cuenta.cntpuc_descp*/}
                                                               into g
                                                                    select new
                                                                    {
                                                                        anio = g.Key.ano,
                                                                        g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                                        saldoInicial = g.Sum(x => x.saldo_ini),
                                                                        saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                        debito = g.Sum(x => x.debito),
                                                                        credito = g.Sum(x => x.credito),
                                                                        debitoNiff = g.Sum(x => x.debitoniff),
                                                                        creditoNiff = g.Sum(x => x.creditoniff)
                                                                    }).FirstOrDefault()
                                                       }).Distinct().OrderBy(x => x.nivel).ToList();


                            foreach (var item in buscarCuentasNivel5)
                            {
                                //foreach (var items2 in item.total)
                                //{
                                listaCuentas.Add(new CuentaConsulta
                                {
                                    //cuentaId = item.cntpuc_id,
                                    cuenta = item.nivel,
                                    esAfectable = indicesNivel4.FirstOrDefault(x => x.cntpuc_numero == item.nivel).esafectable,
                                    cuentaDescripcion = indicesNivel4.FirstOrDefault(x => x.cntpuc_numero == item.nivel)
                                        .cntpuc_descp,
                                    ano = item.total.anio,
                                    mes = item.total.mes,
                                    saldo_ini = item.total.saldoInicial,
                                    saldo_inicial_niff = item.total.saldoInicialNiff,
                                    debito = item.total.debito,
                                    credito = item.total.credito,
                                    debitoNiff = item.total.debitoNiff,
                                    creditoNiff = item.total.creditoNiff,
                                    total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                    totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff - item.total.creditoNiff
                                });
                            }
                            //}
                            cuentaNivelAnterior.cuentasHijas = listaCuentas;

                            if (filtroCentro)
                            {
                                foreach (CuentaConsulta cuenta in cuentaNivelAnterior.cuentasHijas)
                                {
                                    List<CuentaConsulta> listaCentros = new List<CuentaConsulta>();
                                    var buscarCentrosNivel5 = (from cuentaValor in context.cuentas_valores
                                                               join centro in context.centro_costo
                                                                   on cuentaValor.centro equals centro.centcst_id
                                                               join cuentaPuc in context.cuenta_puc
                                                                   on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                                               where /*cuentaValor.cuenta || cuentaPuc.cntpuc_numero.Length == 2*/
                                                                   cuentaPuc.cntpuc_numero.Substring(0, 8) == cuenta.cuenta
                                                                   //where /*(cuentaPuc.cntpuc_numero.Length == 2) &&*/
                                                                   //&& indicesNumericos.Contains(cuentaPuc.cntpuc_numero.Substring(0, 1))
                                                                   && cuentaValor.ano == cuenta.ano && cuentaValor.mes == cuenta.mes
                                                               //&& cuentaValor.ano >= FechaInicio.Year && cuentaValor.ano <= FechaFin.Year && cuentaValor.mes >= FechaInicio.Month && cuentaValor.mes <= FechaFin.Month
                                                               select new
                                                               {
                                                                   //cuentaPuc.cntpuc_id,
                                                                   centro.centcst_id,
                                                                   centro.centcst_nombre,
                                                                   centro.pre_centcst,
                                                                   //cuentaPuc.cntpuc_numero,
                                                                   //cntpuc_descp = (from indicesNombre in indicesNivel2 where indicesNombre.cntpuc_numero == cuentaPuc.cntpuc_numero.Substring(0, 2) select indicesNombre.cntpuc_descp),
                                                                   //cntpuc_descp = indicesNivel2.FirstOrDefault(x=>x.cntpuc_numero == cuentaPuc.cntpuc_numero.Substring(0, 2)).cntpuc_descp,
                                                                   total = (from cuentasValores in context.cuentas_valores
                                                                            join centroC in context.centro_costo
                                                                                on cuentasValores.centro equals centroC.centcst_id
                                                                            join cuentaP in context.cuenta_puc
                                                                                on cuentasValores.cuenta equals cuentaP.cntpuc_id
                                                                            where cuentaP.cntpuc_numero.Substring(0, 8) == cuenta.cuenta
                                                                                  && centroC.centcst_id == centro.centcst_id
                                                                                  && cuentasValores.ano >= cuenta.ano && cuentasValores.ano <= cuenta.ano
                                                                                  && cuentasValores.mes >= cuenta.mes && cuentasValores.mes <= cuenta.mes
                                                                            group cuentasValores by new
                                                                            {
                                                                                cuentasValores.ano,
                                                                                cuentasValores.mes,
                                                                                cuentasValores.centro /*, cuenta.cntpuc_descp*/
                                                                            }
                                                                       into g
                                                                            select new
                                                                            {
                                                                                g.Key.centro,
                                                                                anio = g.Key.ano,
                                                                                g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                                                saldoInicial = g.Sum(x => x.saldo_ini),
                                                                                saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                                debito = g.Sum(x => x.debito),
                                                                                credito = g.Sum(x => x.credito),
                                                                                debitoNiff = g.Sum(x => x.debitoniff),
                                                                                creditoNiff = g.Sum(x => x.creditoniff)
                                                                            }).FirstOrDefault()
                                                               }).Distinct().OrderBy(x => x.centcst_id).ToList();

                                    foreach (var item5 in buscarCentrosNivel5)
                                    {
                                        //foreach (var items2 in item.total)
                                        //{
                                        listaCentros.Add(new CuentaConsulta
                                        {
                                            //cuentaId = item.cntpuc_id,
                                            centroPrefijo = item5.pre_centcst,
                                            centro = item5.centcst_nombre,
                                            ano = item5.total.anio,
                                            mes = item5.total.mes,
                                            saldo_ini = item5.total.saldoInicial,
                                            saldo_inicial_niff = item5.total.saldoInicialNiff,
                                            debito = item5.total.debito,
                                            credito = item5.total.credito,
                                            debitoNiff = item5.total.debitoNiff,
                                            creditoNiff = item5.total.creditoNiff,
                                            total = item5.total.saldoInicial + item5.total.debito - item5.total.credito,
                                            totalNiff = item5.total.saldoInicialNiff + item5.total.debitoNiff -
                                                        item5.total.creditoNiff
                                        });
                                    }
                                    //}
                                    cuenta.centrosHijos = listaCentros;
                                }
                            }

                            if (filtroNit)
                            {
                                foreach (CuentaConsulta cuenta in cuentaNivelAnterior.cuentasHijas)
                                {
                                    List<CuentaConsulta> listaNits = new List<CuentaConsulta>();
                                    var buscarNitsNivel5 = (from cuentaValor in context.cuentas_valores
                                                            join tercero in context.icb_terceros
                                                                on cuentaValor.nit equals tercero.tercero_id
                                                            join cuentaPuc in context.cuenta_puc
                                                                on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                                            where /*cuentaValor.cuenta || cuentaPuc.cntpuc_numero.Length == 2*/
                                                                cuentaPuc.cntpuc_numero.Substring(0, 8) == cuenta.cuenta
                                                                //where /*(cuentaPuc.cntpuc_numero.Length == 2) &&*/
                                                                //&& indicesNumericos.Contains(cuentaPuc.cntpuc_numero.Substring(0, 1))
                                                                && cuentaValor.ano == cuenta.ano && cuentaValor.mes == cuenta.mes
                                                            //&& cuentaValor.ano >= FechaInicio.Year && cuentaValor.ano <= FechaFin.Year && cuentaValor.mes >= FechaInicio.Month && cuentaValor.mes <= FechaFin.Month
                                                            select new
                                                            {
                                                                tercero.tercero_id,
                                                                tercero.prinom_tercero,
                                                                tercero.segnom_tercero,
                                                                tercero.apellido_tercero,
                                                                tercero.segapellido_tercero,
                                                                tercero.razon_social,
                                                                tercero.doc_tercero,
                                                                total = (from cuentasValores in context.cuentas_valores
                                                                         join terceroC in context.icb_terceros
                                                                             on cuentasValores.nit equals terceroC.tercero_id
                                                                         join cuentaP in context.cuenta_puc
                                                                             on cuentasValores.cuenta equals cuentaP.cntpuc_id
                                                                         where cuentaP.cntpuc_numero.Substring(0, 8) == cuenta.cuenta
                                                                               && terceroC.tercero_id == tercero.tercero_id
                                                                               && cuentasValores.ano >= cuenta.ano && cuentasValores.ano <= cuenta.ano
                                                                               && cuentasValores.mes >= cuenta.mes && cuentasValores.mes <= cuenta.mes
                                                                         group cuentasValores by new
                                                                         {
                                                                             cuentasValores.ano,
                                                                             cuentasValores.mes,
                                                                             cuentasValores.nit /*, cuenta.cntpuc_descp*/
                                                                         }
                                                                    into g
                                                                         select new
                                                                         {
                                                                             tercero = g.Key.nit,
                                                                             anio = g.Key.ano,
                                                                             g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                                             saldoInicial = g.Sum(x => x.saldo_ini),
                                                                             saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                             debito = g.Sum(x => x.debito),
                                                                             credito = g.Sum(x => x.credito),
                                                                             debitoNiff = g.Sum(x => x.debitoniff),
                                                                             creditoNiff = g.Sum(x => x.creditoniff)
                                                                         }).FirstOrDefault()
                                                            }).Distinct().OrderBy(x => x.tercero_id).ToList();

                                    if (nit != null)
                                    {
                                        foreach (var item in buscarNitsNivel5)
                                        {
                                            if (item.tercero_id == nit)
                                            {
                                                listaNits.Add(new CuentaConsulta
                                                {
                                                    nit = item.tercero_id,
                                                    terceroNombre = item.prinom_tercero + " " + item.segnom_tercero + " " +
                                                                    item.apellido_tercero + " " + item.segapellido_tercero + " " +
                                                                    item.razon_social,
                                                    terceroDocumento = item.doc_tercero,
                                                    ano = item.total.anio,
                                                    mes = item.total.mes,
                                                    saldo_ini = item.total.saldoInicial,
                                                    saldo_inicial_niff = item.total.saldoInicialNiff,
                                                    debito = item.total.debito,
                                                    credito = item.total.credito,
                                                    debitoNiff = item.total.debitoNiff,
                                                    creditoNiff = item.total.creditoNiff,
                                                    total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                                    totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff -
                                                                item.total.creditoNiff
                                                });
                                            }
                                        }

                                        CuentaConsulta buscarNit = listaNits.FirstOrDefault(x => x.nit == nit);
                                        if (buscarNit == null)
                                        {
                                            cuenta.visible = false;
                                        }
                                        else
                                        {
                                            cuenta.visible = true;
                                            cuenta.NitsHijos = listaNits;
                                        }
                                    }
                                    else
                                    {
                                        foreach (var item in buscarNitsNivel5)
                                        {
                                            listaNits.Add(new CuentaConsulta
                                            {
                                                nit = item.tercero_id,
                                                terceroNombre = item.prinom_tercero + " " + item.segnom_tercero + " " +
                                                                item.apellido_tercero + " " + item.segapellido_tercero + " " +
                                                                item.razon_social,
                                                terceroDocumento = item.doc_tercero,
                                                ano = item.total.anio,
                                                mes = item.total.mes,
                                                saldo_ini = item.total.saldoInicial,
                                                saldo_inicial_niff = item.total.saldoInicialNiff,
                                                debito = item.total.debito,
                                                credito = item.total.credito,
                                                debitoNiff = item.total.debitoNiff,
                                                creditoNiff = item.total.creditoNiff,
                                                total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                                totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff -
                                                            item.total.creditoNiff
                                            });
                                        }

                                        cuenta.visible = true;
                                        cuenta.NitsHijos = listaNits;
                                    }
                                }
                            }

                            if (!filtroNit)
                            {
                                if (nit != null)
                                {
                                    foreach (CuentaConsulta cuenta in listaCuentas)
                                    {
                                        List<CuentaConsulta> listaNits = new List<CuentaConsulta>();
                                        var buscarNitsNivel5 = (from cuentaValor in context.cuentas_valores
                                                                join tercero in context.icb_terceros
                                                                    on cuentaValor.nit equals tercero.tercero_id
                                                                join cuentaPuc in context.cuenta_puc
                                                                    on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                                                where /*cuentaValor.cuenta || cuentaPuc.cntpuc_numero.Length == 2*/
                                                                    cuentaPuc.cntpuc_numero.Substring(0, 8) == cuenta.cuenta
                                                                    //where /*(cuentaPuc.cntpuc_numero.Length == 2) &&*/
                                                                    //&& indicesNumericos.Contains(cuentaPuc.cntpuc_numero.Substring(0, 1))
                                                                    && cuentaValor.ano == cuenta.ano && cuentaValor.mes == cuenta.mes
                                                                //&& cuentaValor.ano >= FechaInicio.Year && cuentaValor.ano <= FechaFin.Year && cuentaValor.mes >= FechaInicio.Month && cuentaValor.mes <= FechaFin.Month
                                                                select new
                                                                {
                                                                    tercero.tercero_id,
                                                                    tercero.prinom_tercero,
                                                                    tercero.segnom_tercero,
                                                                    tercero.apellido_tercero,
                                                                    tercero.segapellido_tercero,
                                                                    tercero.razon_social,
                                                                    tercero.doc_tercero,
                                                                    total = (from cuentasValores in context.cuentas_valores
                                                                             join terceroC in context.icb_terceros
                                                                                 on cuentasValores.nit equals terceroC.tercero_id
                                                                             join cuentaP in context.cuenta_puc
                                                                                 on cuentasValores.cuenta equals cuentaP.cntpuc_id
                                                                             where cuentaP.cntpuc_numero.Substring(0, 8) == cuenta.cuenta
                                                                                   && terceroC.tercero_id == tercero.tercero_id
                                                                                   && cuentasValores.ano >= cuenta.ano && cuentasValores.ano <= cuenta.ano
                                                                                   && cuentasValores.mes >= cuenta.mes && cuentasValores.mes <= cuenta.mes
                                                                             group cuentasValores by new
                                                                             {
                                                                                 cuentasValores.ano,
                                                                                 cuentasValores.mes,
                                                                                 cuentasValores.nit /*, cuenta.cntpuc_descp*/
                                                                             }
                                                                        into g
                                                                             select new
                                                                             {
                                                                                 tercero = g.Key.nit,
                                                                                 anio = g.Key.ano,
                                                                                 g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                                                 saldoInicial = g.Sum(x => x.saldo_ini),
                                                                                 saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                                 debito = g.Sum(x => x.debito),
                                                                                 credito = g.Sum(x => x.credito),
                                                                                 debitoNiff = g.Sum(x => x.debitoniff),
                                                                                 creditoNiff = g.Sum(x => x.creditoniff)
                                                                             }).FirstOrDefault()
                                                                }).Distinct().OrderBy(x => x.tercero_id).ToList();

                                        foreach (var item in buscarNitsNivel5)
                                        {
                                            listaNits.Add(new CuentaConsulta
                                            {
                                                nit = item.tercero_id,
                                                terceroNombre = item.prinom_tercero + " " + item.segnom_tercero + " " +
                                                                item.apellido_tercero + " " + item.segapellido_tercero + " " +
                                                                item.razon_social,
                                                terceroDocumento = item.doc_tercero,
                                                ano = item.total.anio,
                                                mes = item.total.mes,
                                                saldo_ini = item.total.saldoInicial,
                                                saldo_inicial_niff = item.total.saldoInicialNiff,
                                                debito = item.total.debito,
                                                credito = item.total.credito,
                                                debitoNiff = item.total.debitoNiff,
                                                creditoNiff = item.total.creditoNiff,
                                                total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                                totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff -
                                                            item.total.creditoNiff
                                            });
                                        }

                                        CuentaConsulta buscarNit = listaNits.FirstOrDefault(x => x.nit == nit);
                                        if (buscarNit == null)
                                        {
                                            cuenta.visible = false;
                                        }
                                        else
                                        {
                                            cuenta.visible = true;
                                        }
                                        //cuenta.NitsHijos = listaNits;
                                    }
                                }
                            }

                            if (!filtroNit)
                            {
                                if (nit != null)
                                {
                                    foreach (CuentaConsulta cuenta in listaCuentas)
                                    {
                                        List<CuentaConsulta> listaNits = new List<CuentaConsulta>();
                                        var buscarNitsNivel5 = (from cuentaValor in context.cuentas_valores
                                                                join tercero in context.icb_terceros
                                                                    on cuentaValor.nit equals tercero.tercero_id
                                                                join cuentaPuc in context.cuenta_puc
                                                                    on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                                                where /*cuentaValor.cuenta || cuentaPuc.cntpuc_numero.Length == 2*/
                                                                    cuentaPuc.cntpuc_numero.Substring(0, 8) == cuenta.cuenta
                                                                    //where /*(cuentaPuc.cntpuc_numero.Length == 2) &&*/
                                                                    //&& indicesNumericos.Contains(cuentaPuc.cntpuc_numero.Substring(0, 1))
                                                                    && cuentaValor.ano == cuenta.ano && cuentaValor.mes == cuenta.mes
                                                                //&& cuentaValor.ano >= FechaInicio.Year && cuentaValor.ano <= FechaFin.Year && cuentaValor.mes >= FechaInicio.Month && cuentaValor.mes <= FechaFin.Month
                                                                select new
                                                                {
                                                                    tercero.tercero_id,
                                                                    tercero.prinom_tercero,
                                                                    tercero.segnom_tercero,
                                                                    tercero.apellido_tercero,
                                                                    tercero.segapellido_tercero,
                                                                    tercero.razon_social,
                                                                    tercero.doc_tercero,
                                                                    total = (from cuentasValores in context.cuentas_valores
                                                                             join terceroC in context.icb_terceros
                                                                                 on cuentasValores.nit equals terceroC.tercero_id
                                                                             join cuentaP in context.cuenta_puc
                                                                                 on cuentasValores.cuenta equals cuentaP.cntpuc_id
                                                                             where cuentaP.cntpuc_numero.Substring(0, 8) == cuenta.cuenta
                                                                                   && terceroC.tercero_id == tercero.tercero_id
                                                                                   && cuentasValores.ano >= cuenta.ano && cuentasValores.ano <= cuenta.ano
                                                                                   && cuentasValores.mes >= cuenta.mes && cuentasValores.mes <= cuenta.mes
                                                                             group cuentasValores by new
                                                                             {
                                                                                 cuentasValores.ano,
                                                                                 cuentasValores.mes,
                                                                                 cuentasValores.nit /*, cuenta.cntpuc_descp*/
                                                                             }
                                                                        into g
                                                                             select new
                                                                             {
                                                                                 tercero = g.Key.nit,
                                                                                 anio = g.Key.ano,
                                                                                 g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                                                 saldoInicial = g.Sum(x => x.saldo_ini),
                                                                                 saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                                 debito = g.Sum(x => x.debito),
                                                                                 credito = g.Sum(x => x.credito),
                                                                                 debitoNiff = g.Sum(x => x.debitoniff),
                                                                                 creditoNiff = g.Sum(x => x.creditoniff)
                                                                             }).FirstOrDefault()
                                                                }).Distinct().OrderBy(x => x.tercero_id).ToList();

                                        foreach (var item in buscarNitsNivel5)
                                        {
                                            listaNits.Add(new CuentaConsulta
                                            {
                                                nit = item.tercero_id,
                                                terceroNombre = item.prinom_tercero + " " + item.segnom_tercero + " " +
                                                                item.apellido_tercero + " " + item.segapellido_tercero + " " +
                                                                item.razon_social,
                                                terceroDocumento = item.doc_tercero,
                                                ano = item.total.anio,
                                                mes = item.total.mes,
                                                saldo_ini = item.total.saldoInicial,
                                                saldo_inicial_niff = item.total.saldoInicialNiff,
                                                debito = item.total.debito,
                                                credito = item.total.credito,
                                                debitoNiff = item.total.debitoNiff,
                                                creditoNiff = item.total.creditoNiff,
                                                total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                                totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff -
                                                            item.total.creditoNiff
                                            });
                                        }

                                        CuentaConsulta buscarNit = listaNits.FirstOrDefault(x => x.nit == nit);
                                        if (buscarNit == null)
                                        {
                                            cuenta.visible = false;
                                        }
                                        else
                                        {
                                            cuenta.visible = true;
                                        }
                                        //cuenta.NitsHijos = listaNits;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return cuentasNivelAnterior;
        }

        public List<CuentaConsulta> getNivel6(List<string> indices, DateTime FechaInicio, DateTime FechaFin,
            List<CuentaConsulta> cuentasNivelAnterior, bool filtroCentro, bool filtroNit, int? nit)
        {
            foreach (CuentaConsulta cuentaNivelPadre2 in cuentasNivelAnterior)
            {
                foreach (CuentaConsulta cuentaNivelPadre in cuentaNivelPadre2.cuentasHijas)
                {
                    foreach (CuentaConsulta cuentaNivelAnterior1 in cuentaNivelPadre.cuentasHijas)
                    {
                        foreach (CuentaConsulta cuentaNivelAnterior0 in cuentaNivelAnterior1.cuentasHijas)
                        {
                            foreach (CuentaConsulta cuentaNivelAnterior in cuentaNivelAnterior0.cuentasHijas)
                            {
                                var indicesNivel5 = (from cuenta in context.cuenta_puc
                                                     where cuenta.cntpuc_numero.Length == 10 &&
                                                           cuenta.cntpuc_numero.Substring(0, 8) == cuentaNivelAnterior.cuenta
                                                     select new { cuenta.cntpuc_numero, cuenta.cntpuc_descp, cuenta.esafectable }).ToList();
                                List<string> indicesNumericos = indicesNivel5.Select(x => x.cntpuc_numero).ToList();
                                List<CuentaConsulta> listaCuentas = new List<CuentaConsulta>();
                                var buscarCuentasNivel6 = (from cuentaValor in context.cuentas_valores
                                                           join cuentaPuc in context.cuenta_puc
                                                               on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                                           where /*cuentaValor.cuenta || cuentaPuc.cntpuc_numero.Length == 2*/
                                                               cuentaPuc.cntpuc_numero.Substring(0, 8) == cuentaNivelAnterior.cuenta
                                                               //where /*(cuentaPuc.cntpuc_numero.Length == 2) &&*/
                                                               && indicesNumericos.Contains(cuentaPuc.cntpuc_numero.Substring(0, 10))
                                                               && cuentaValor.ano == cuentaNivelAnterior.ano && cuentaValor.mes == cuentaNivelAnterior.mes
                                                           select new
                                                           {
                                                               //cuentaPuc.cntpuc_id,
                                                               nivel = cuentaPuc.cntpuc_numero.Substring(0, 10),
                                                               //cuentaPuc.cntpuc_numero,
                                                               //cntpuc_descp = (from indicesNombre in indicesNivel2 where indicesNombre.cntpuc_numero == cuentaPuc.cntpuc_numero.Substring(0, 2) select indicesNombre.cntpuc_descp),
                                                               //cntpuc_descp = indicesNivel2.FirstOrDefault(x=>x.cntpuc_numero == cuentaPuc.cntpuc_numero.Substring(0, 2)).cntpuc_descp,
                                                               total = (from cuentasValores in context.cuentas_valores
                                                                        join cuenta in context.cuenta_puc
                                                                            on cuentasValores.cuenta equals cuenta.cntpuc_id
                                                                        where cuenta.cntpuc_numero.Substring(0, 10) == cuentaPuc.cntpuc_numero.Substring(0, 10)
                                                                              && cuentasValores.ano >= cuentaNivelAnterior.ano &&
                                                                              cuentasValores.ano <= cuentaNivelAnterior.ano
                                                                              && cuentasValores.mes >= cuentaNivelAnterior.mes &&
                                                                              cuentasValores.mes <= cuentaNivelAnterior.mes
                                                                        group cuentasValores by new
                                                                        { cuentasValores.ano, cuentasValores.mes /*, cuenta.cntpuc_numero, cuenta.cntpuc_descp*/}
                                                                   into g
                                                                        select new
                                                                        {
                                                                            anio = g.Key.ano,
                                                                            g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                                            saldoInicial = g.Sum(x => x.saldo_ini),
                                                                            saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                            debito = g.Sum(x => x.debito),
                                                                            credito = g.Sum(x => x.credito),
                                                                            debitoNiff = g.Sum(x => x.debitoniff),
                                                                            creditoNiff = g.Sum(x => x.creditoniff)
                                                                        }).FirstOrDefault()
                                                           }).Distinct().OrderBy(x => x.nivel).ToList();


                                foreach (var item in buscarCuentasNivel6)
                                {
                                    //foreach (var items2 in item.total)
                                    //{
                                    listaCuentas.Add(new CuentaConsulta
                                    {
                                        //cuentaId = item.cntpuc_id,
                                        cuenta = item.nivel,
                                        esAfectable = indicesNivel5.FirstOrDefault(x => x.cntpuc_numero == item.nivel).esafectable,
                                        cuentaDescripcion = indicesNivel5.FirstOrDefault(x => x.cntpuc_numero == item.nivel)
                                            .cntpuc_descp,
                                        ano = item.total.anio,
                                        mes = item.total.mes,
                                        saldo_ini = item.total.saldoInicial,
                                        saldo_inicial_niff = item.total.saldoInicialNiff,
                                        debito = item.total.debito,
                                        credito = item.total.credito,
                                        debitoNiff = item.total.debitoNiff,
                                        creditoNiff = item.total.creditoNiff,
                                        total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                        totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff - item.total.creditoNiff
                                    });
                                }
                                //}
                                cuentaNivelAnterior.cuentasHijas = listaCuentas;

                                if (filtroCentro)
                                {
                                    foreach (CuentaConsulta cuenta in cuentaNivelAnterior.cuentasHijas)
                                    {
                                        List<CuentaConsulta> listaCentros = new List<CuentaConsulta>();
                                        var buscarCentrosNivel6 = (from cuentaValor in context.cuentas_valores
                                                                   join centro in context.centro_costo
                                                                       on cuentaValor.centro equals centro.centcst_id
                                                                   join cuentaPuc in context.cuenta_puc
                                                                       on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                                                   where /*cuentaValor.cuenta || cuentaPuc.cntpuc_numero.Length == 2*/
                                                                       cuentaPuc.cntpuc_numero.Substring(0, 10) == cuenta.cuenta
                                                                       //where /*(cuentaPuc.cntpuc_numero.Length == 2) &&*/
                                                                       //&& indicesNumericos.Contains(cuentaPuc.cntpuc_numero.Substring(0, 1))
                                                                       && cuentaValor.ano == cuenta.ano && cuentaValor.mes == cuenta.mes
                                                                   //&& cuentaValor.ano >= FechaInicio.Year && cuentaValor.ano <= FechaFin.Year && cuentaValor.mes >= FechaInicio.Month && cuentaValor.mes <= FechaFin.Month
                                                                   select new
                                                                   {
                                                                       //cuentaPuc.cntpuc_id,
                                                                       centro.centcst_id,
                                                                       centro.centcst_nombre,
                                                                       centro.pre_centcst,
                                                                       //cuentaPuc.cntpuc_numero,
                                                                       //cntpuc_descp = (from indicesNombre in indicesNivel2 where indicesNombre.cntpuc_numero == cuentaPuc.cntpuc_numero.Substring(0, 2) select indicesNombre.cntpuc_descp),
                                                                       //cntpuc_descp = indicesNivel2.FirstOrDefault(x=>x.cntpuc_numero == cuentaPuc.cntpuc_numero.Substring(0, 2)).cntpuc_descp,
                                                                       total = (from cuentasValores in context.cuentas_valores
                                                                                join centroC in context.centro_costo
                                                                                    on cuentasValores.centro equals centroC.centcst_id
                                                                                join cuentaP in context.cuenta_puc
                                                                                    on cuentasValores.cuenta equals cuentaP.cntpuc_id
                                                                                where cuentaP.cntpuc_numero.Substring(0, 10) == cuenta.cuenta
                                                                                      && centroC.centcst_id == centro.centcst_id
                                                                                      && cuentasValores.ano >= cuenta.ano && cuentasValores.ano <= cuenta.ano
                                                                                      && cuentasValores.mes >= cuenta.mes && cuentasValores.mes <= cuenta.mes
                                                                                group cuentasValores by new
                                                                                {
                                                                                    cuentasValores.ano,
                                                                                    cuentasValores.mes,
                                                                                    cuentasValores.centro /*, cuenta.cntpuc_descp*/
                                                                                }
                                                                           into g
                                                                                select new
                                                                                {
                                                                                    g.Key.centro,
                                                                                    anio = g.Key.ano,
                                                                                    g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                                                    saldoInicial = g.Sum(x => x.saldo_ini),
                                                                                    saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                                    debito = g.Sum(x => x.debito),
                                                                                    credito = g.Sum(x => x.credito),
                                                                                    debitoNiff = g.Sum(x => x.debitoniff),
                                                                                    creditoNiff = g.Sum(x => x.creditoniff)
                                                                                }).FirstOrDefault()
                                                                   }).Distinct().OrderBy(x => x.centcst_id).ToList();

                                        foreach (var item6 in buscarCentrosNivel6)
                                        {
                                            //foreach (var items2 in item.total)
                                            //{
                                            listaCentros.Add(new CuentaConsulta
                                            {
                                                //cuentaId = item.cntpuc_id,
                                                centroPrefijo = item6.pre_centcst,
                                                centro = item6.centcst_nombre,
                                                ano = item6.total.anio,
                                                mes = item6.total.mes,
                                                saldo_ini = item6.total.saldoInicial,
                                                saldo_inicial_niff = item6.total.saldoInicialNiff,
                                                debito = item6.total.debito,
                                                credito = item6.total.credito,
                                                debitoNiff = item6.total.debitoNiff,
                                                creditoNiff = item6.total.creditoNiff,
                                                total = item6.total.saldoInicial + item6.total.debito - item6.total.credito,
                                                totalNiff = item6.total.saldoInicialNiff + item6.total.debitoNiff -
                                                            item6.total.creditoNiff
                                            });
                                        }
                                        //}
                                        cuenta.centrosHijos = listaCentros;
                                    }
                                }

                                if (filtroNit)
                                {
                                    foreach (CuentaConsulta cuenta in cuentaNivelAnterior.cuentasHijas)
                                    {
                                        List<CuentaConsulta> listaNits = new List<CuentaConsulta>();
                                        var buscarNitsNivel6 = (from cuentaValor in context.cuentas_valores
                                                                join tercero in context.icb_terceros
                                                                    on cuentaValor.nit equals tercero.tercero_id
                                                                join cuentaPuc in context.cuenta_puc
                                                                    on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                                                where /*cuentaValor.cuenta || cuentaPuc.cntpuc_numero.Length == 2*/
                                                                    cuentaPuc.cntpuc_numero.Substring(0, 10) == cuenta.cuenta
                                                                    //where /*(cuentaPuc.cntpuc_numero.Length == 2) &&*/
                                                                    //&& indicesNumericos.Contains(cuentaPuc.cntpuc_numero.Substring(0, 1))
                                                                    && cuentaValor.ano == cuenta.ano && cuentaValor.mes == cuenta.mes
                                                                //&& cuentaValor.ano >= FechaInicio.Year && cuentaValor.ano <= FechaFin.Year && cuentaValor.mes >= FechaInicio.Month && cuentaValor.mes <= FechaFin.Month
                                                                select new
                                                                {
                                                                    tercero.tercero_id,
                                                                    tercero.prinom_tercero,
                                                                    tercero.segnom_tercero,
                                                                    tercero.apellido_tercero,
                                                                    tercero.segapellido_tercero,
                                                                    tercero.razon_social,
                                                                    tercero.doc_tercero,
                                                                    total = (from cuentasValores in context.cuentas_valores
                                                                             join terceroC in context.icb_terceros
                                                                                 on cuentasValores.nit equals terceroC.tercero_id
                                                                             join cuentaP in context.cuenta_puc
                                                                                 on cuentasValores.cuenta equals cuentaP.cntpuc_id
                                                                             where cuentaP.cntpuc_numero.Substring(0, 10) == cuenta.cuenta
                                                                                   && terceroC.tercero_id == tercero.tercero_id
                                                                                   && cuentasValores.ano >= cuenta.ano && cuentasValores.ano <= cuenta.ano
                                                                                   && cuentasValores.mes >= cuenta.mes && cuentasValores.mes <= cuenta.mes
                                                                             group cuentasValores by new
                                                                             {
                                                                                 cuentasValores.ano,
                                                                                 cuentasValores.mes,
                                                                                 cuentasValores.nit /*, cuenta.cntpuc_descp*/
                                                                             }
                                                                        into g
                                                                             select new
                                                                             {
                                                                                 tercero = g.Key.nit,
                                                                                 anio = g.Key.ano,
                                                                                 g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                                                 saldoInicial = g.Sum(x => x.saldo_ini),
                                                                                 saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                                 debito = g.Sum(x => x.debito),
                                                                                 credito = g.Sum(x => x.credito),
                                                                                 debitoNiff = g.Sum(x => x.debitoniff),
                                                                                 creditoNiff = g.Sum(x => x.creditoniff)
                                                                             }).FirstOrDefault()
                                                                }).Distinct().OrderBy(x => x.tercero_id).ToList();

                                        if (nit != null)
                                        {
                                            foreach (var item in buscarNitsNivel6)
                                            {
                                                if (item.tercero_id == nit)
                                                {
                                                    listaNits.Add(new CuentaConsulta
                                                    {
                                                        nit = item.tercero_id,
                                                        terceroNombre = item.prinom_tercero + " " + item.segnom_tercero + " " +
                                                                        item.apellido_tercero + " " + item.segapellido_tercero + " " +
                                                                        item.razon_social,
                                                        terceroDocumento = item.doc_tercero,
                                                        ano = item.total.anio,
                                                        mes = item.total.mes,
                                                        saldo_ini = item.total.saldoInicial,
                                                        saldo_inicial_niff = item.total.saldoInicialNiff,
                                                        debito = item.total.debito,
                                                        credito = item.total.credito,
                                                        debitoNiff = item.total.debitoNiff,
                                                        creditoNiff = item.total.creditoNiff,
                                                        total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                                        totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff -
                                                                    item.total.creditoNiff
                                                    });
                                                }
                                            }

                                            CuentaConsulta buscarNit = listaNits.FirstOrDefault(x => x.nit == nit);
                                            if (buscarNit == null)
                                            {
                                                cuenta.visible = false;
                                            }
                                            else
                                            {
                                                cuenta.visible = true;
                                                cuenta.NitsHijos = listaNits;
                                            }
                                        }
                                        else
                                        {
                                            foreach (var item in buscarNitsNivel6)
                                            {
                                                listaNits.Add(new CuentaConsulta
                                                {
                                                    nit = item.tercero_id,
                                                    terceroNombre = item.prinom_tercero + " " + item.segnom_tercero + " " +
                                                                    item.apellido_tercero + " " + item.segapellido_tercero + " " +
                                                                    item.razon_social,
                                                    terceroDocumento = item.doc_tercero,
                                                    ano = item.total.anio,
                                                    mes = item.total.mes,
                                                    saldo_ini = item.total.saldoInicial,
                                                    saldo_inicial_niff = item.total.saldoInicialNiff,
                                                    debito = item.total.debito,
                                                    credito = item.total.credito,
                                                    debitoNiff = item.total.debitoNiff,
                                                    creditoNiff = item.total.creditoNiff,
                                                    total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                                    totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff -
                                                                item.total.creditoNiff
                                                });
                                            }

                                            cuenta.visible = true;
                                            cuenta.NitsHijos = listaNits;
                                        }
                                    }
                                }

                                if (!filtroNit)
                                {
                                    if (nit != null)
                                    {
                                        foreach (CuentaConsulta cuenta in listaCuentas)
                                        {
                                            List<CuentaConsulta> listaNits = new List<CuentaConsulta>();
                                            var buscarNitsNivel6 = (from cuentaValor in context.cuentas_valores
                                                                    join tercero in context.icb_terceros
                                                                        on cuentaValor.nit equals tercero.tercero_id
                                                                    join cuentaPuc in context.cuenta_puc
                                                                        on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                                                    where /*cuentaValor.cuenta || cuentaPuc.cntpuc_numero.Length == 2*/
                                                                        cuentaPuc.cntpuc_numero.Substring(0, 10) == cuenta.cuenta
                                                                        //where /*(cuentaPuc.cntpuc_numero.Length == 2) &&*/
                                                                        //&& indicesNumericos.Contains(cuentaPuc.cntpuc_numero.Substring(0, 1))
                                                                        && cuentaValor.ano == cuenta.ano && cuentaValor.mes == cuenta.mes
                                                                    //&& cuentaValor.ano >= FechaInicio.Year && cuentaValor.ano <= FechaFin.Year && cuentaValor.mes >= FechaInicio.Month && cuentaValor.mes <= FechaFin.Month
                                                                    select new
                                                                    {
                                                                        tercero.tercero_id,
                                                                        tercero.prinom_tercero,
                                                                        tercero.segnom_tercero,
                                                                        tercero.apellido_tercero,
                                                                        tercero.segapellido_tercero,
                                                                        tercero.razon_social,
                                                                        tercero.doc_tercero,
                                                                        total = (from cuentasValores in context.cuentas_valores
                                                                                 join terceroC in context.icb_terceros
                                                                                     on cuentasValores.nit equals terceroC.tercero_id
                                                                                 join cuentaP in context.cuenta_puc
                                                                                     on cuentasValores.cuenta equals cuentaP.cntpuc_id
                                                                                 where cuentaP.cntpuc_numero.Substring(0, 10) == cuenta.cuenta
                                                                                       && terceroC.tercero_id == tercero.tercero_id
                                                                                       && cuentasValores.ano >= cuenta.ano && cuentasValores.ano <= cuenta.ano
                                                                                       && cuentasValores.mes >= cuenta.mes && cuentasValores.mes <= cuenta.mes
                                                                                 group cuentasValores by new
                                                                                 {
                                                                                     cuentasValores.ano,
                                                                                     cuentasValores.mes,
                                                                                     cuentasValores.nit /*, cuenta.cntpuc_descp*/
                                                                                 }
                                                                            into g
                                                                                 select new
                                                                                 {
                                                                                     tercero = g.Key.nit,
                                                                                     anio = g.Key.ano,
                                                                                     g.Key.mes /*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
                                                                                     saldoInicial = g.Sum(x => x.saldo_ini),
                                                                                     saldoInicialNiff = g.Sum(x => x.saldo_ininiff),
                                                                                     debito = g.Sum(x => x.debito),
                                                                                     credito = g.Sum(x => x.credito),
                                                                                     debitoNiff = g.Sum(x => x.debitoniff),
                                                                                     creditoNiff = g.Sum(x => x.creditoniff)
                                                                                 }).FirstOrDefault()
                                                                    }).Distinct().OrderBy(x => x.tercero_id).ToList();

                                            foreach (var item in buscarNitsNivel6)
                                            {
                                                listaNits.Add(new CuentaConsulta
                                                {
                                                    nit = item.tercero_id,
                                                    terceroNombre = item.prinom_tercero + " " + item.segnom_tercero + " " +
                                                                    item.apellido_tercero + " " + item.segapellido_tercero + " " +
                                                                    item.razon_social,
                                                    terceroDocumento = item.doc_tercero,
                                                    ano = item.total.anio,
                                                    mes = item.total.mes,
                                                    saldo_ini = item.total.saldoInicial,
                                                    saldo_inicial_niff = item.total.saldoInicialNiff,
                                                    debito = item.total.debito,
                                                    credito = item.total.credito,
                                                    debitoNiff = item.total.debitoNiff,
                                                    creditoNiff = item.total.creditoNiff,
                                                    total = item.total.saldoInicial + item.total.debito - item.total.credito,
                                                    totalNiff = item.total.saldoInicialNiff + item.total.debitoNiff -
                                                                item.total.creditoNiff
                                                });
                                            }

                                            CuentaConsulta buscarNit = listaNits.FirstOrDefault(x => x.nit == nit);
                                            if (buscarNit == null)
                                            {
                                                cuenta.visible = false;
                                            }
                                            else
                                            {
                                                cuenta.visible = true;
                                            }
                                            //cuenta.NitsHijos = listaNits;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return cuentasNivelAnterior;
        }

        public ActionResult repconAux(DateTime? fechaIni, DateTime? fechaFin, int? cuentaini, int? cuentafin,
            int? idtercero,
            string centros,
            string tipos_doc,
            bool checkCentro,
            bool checkNit,
            bool checkMovimiento,
            bool checkNiff
        )
        {
            DateTime? fecha_inicial = fechaIni;
            DateTime? fecha_final = fechaFin;
            int? wcuentaini = cuentaini;
            int? wcuentafin = cuentafin;
            int? widtercero = idtercero;

            string[] Vectorcentros = centros.Split(',');
            string[] VectorTipos = tipos_doc.Split(',');

            #region Declaracion de PredicateBuilder2 Pdf

            //Declaramos el predicado de tipo And con el parámetro True. Si fuera de tipo Or lo declaramos con el parámetro False
            System.Linq.Expressions.Expression<Func<vw_contabilidad, bool>> predicateMovAnd = PredicateBuilder.True<vw_contabilidad>();
            System.Linq.Expressions.Expression<Func<vw_contabilidad, bool>> predicateMovOr = PredicateBuilder.False<vw_contabilidad>();

            //var predicate = PredicateBuilder.True<vw_contabilidad>();
            //var predicate2 = PredicateBuilder.False<vw_contabilidad>();

            #endregion

            #region Criterios aplicados a los PredicateBuilder Pdf

            #region fech ini y fecha fin

            if (fecha_inicial != null)
            {
                predicateMovAnd = predicateMovAnd.And(x => x.fec == fecha_inicial);
            }

            if (fecha_final != null)
            {
                predicateMovAnd = predicateMovAnd.And(x => x.fec == fecha_final);
            }

            #endregion

            string paso = "";
            // Evaluar las cuentas+
            if (wcuentaini != null && wcuentafin != null)
            {
                #region cuenta 1

                if (Convert.ToString(wcuentaini).Length == 1 && Convert.ToString(wcuentafin).Length == 1)
                {
                    int ci11 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf11 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    string numi11 = Convert.ToString(wcuentaini);
                    string numf11 = Convert.ToString(wcuentafin);

                    //var numi11 = Convert.ToString(wcuentaini);
                    //var numf11 = Convert.ToString(wcuentafin);
                    //  predicate = predicate.And(x => x.clase >= ci11 && x.clase <= cf11   /* && x.cntpuc_numero <= wcuentaini*/ );


                    //int t = 0;
                    //int t2 = 0;
                    //predicateMovAnd = predicateMovAnd.And(x => (int.TryParse(x.clase,out t)?t:int.MinValue) >= ci11 && (int.TryParse(x.clase, out t2) ? t2 : int.MinValue) <= cf11   /* && x.cntpuc_numero <= wcuentaini*/ );

                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase2 >= ci11 && x.clase2 <= cf11 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = "-11-";
                }

                if (Convert.ToString(wcuentaini).Length == 1 && Convert.ToString(wcuentafin).Length == 2)
                {
                    int ci12 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf12 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gf12 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));

                    string numi12 = Convert.ToString(wcuentaini);
                    string numf12 = Convert.ToString(wcuentafin);
                    //predicate = predicate.And(x => x.clase >= ci12 && x.clase <= cf12 && x.grupo <= gf12  /* && x.cntpuc_numero <= wcuentaini*/ );
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => Convert.ToInt32(x.clase) >= ci12 && Convert.ToInt32(x.clase) <= cf12 &&
                                 Convert.ToInt32(x.grupo) <= gf12 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-12-";
                }

                if (Convert.ToString(wcuentaini).Length == 1 && Convert.ToString(wcuentafin).Length == 3)
                {
                    int ci13 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf13 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gf13 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));


                    string numi13 = Convert.ToString(wcuentaini);
                    string numf13 = Convert.ToString(wcuentafin);
                    //predicate = predicate.And(x => x.clase >= ci13 && x.clase <= cf13 && x.grupo <= gf13  /* && x.cntpuc_numero <= wcuentaini*/ );
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase2 >= ci13 && x.clase2 <= cf13 &&
                                 x.grupo2 <= gf13 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-13-";
                }

                if (Convert.ToString(wcuentaini).Length == 1 && Convert.ToString(wcuentafin).Length == 4)
                {
                    int ci14 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf14 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gf14 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));

                    int xcf14 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 4));

                    string numi14 = Convert.ToString(wcuentaini);
                    string numf14 = Convert.ToString(wcuentafin);
                    //predicate = predicate.And(x => x.clase >= ci14 && x.clase <= cf14 && x.grupo <= gf14 && x.xcuenta <= xcf14 /* && x.cntpuc_numero <= wcuentaini*/ );
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase2 >= ci14 && x.clase2 <= cf14 && x.grupo2 <= gf14 &&
                                 x.xcuenta2 <= xcf14 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-14-";
                }

                if (Convert.ToString(wcuentaini).Length == 1 && Convert.ToString(wcuentafin).Length == 5)
                {
                    int ci15 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf15 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gf15 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));

                    int xcf15 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 4));

                    string numi15 = Convert.ToString(wcuentaini);
                    string numf15 = Convert.ToString(wcuentafin);
                    //predicate = predicate.And(x => x.clase >= ci15 && x.clase <= cf15 && x.grupo <= gf15 && x.xcuenta <= xcf15 /* && x.cntpuc_numero <= wcuentaini*/ );
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase2 >= ci15 && x.clase2 <= cf15 && x.grupo2 <= gf15 &&
                                 x.xcuenta2 <= xcf15 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-15-";
                }

                if (Convert.ToString(wcuentaini).Length == 1 && Convert.ToString(wcuentafin).Length == 6)
                {
                    int ci16 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf16 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));


                    int gf16 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));


                    int xcf16 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 4));

                    int scf16 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 6));

                    string numi16 = Convert.ToString(wcuentaini);
                    string numf16 = Convert.ToString(wcuentafin);
                    //predicate = predicate.And(x => x.clase >= ci16 && x.clase <= cf16 && x.grupo <= gf16 && x.xcuenta <= xcf16 && x.subcuenta <= scf16/* && x.cntpuc_numero <= wcuentaini*/ );
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase2 >= ci16 && x.clase2 <= cf16 && x.grupo2 <= gf16 && x.xcuenta2 <= xcf16 &&
                                 x.subcuenta2 <= scf16 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-1>5-";
                }

                if (Convert.ToString(wcuentaini).Length == 1 && Convert.ToString(wcuentafin).Length > 6)
                {
                    #region Predicate cuentas

                    string desde11 = Convert.ToString(wcuentaini);
                    string hasta11 = Convert.ToString(wcuentafin);
                    List<string> rango11 = (from p in context.vw_mov_contables
                                            where p.cntpuc_numero.Substring(0, 12).CompareTo(desde11.Substring(0, 12)) >= 0 &&
                                                  p.cntpuc_numero.Substring(0, 12).CompareTo(hasta11.Substring(0, 12)) <= 0
                                            orderby p.cntpuc_numero, p.clase, p.grupo, p.cuenta, p.subcuenta
                                            select p.cntpuc_numero).ToList();
                    predicateMovAnd = predicateMovAnd.And(p => 1 == 1 && rango11.Contains(p.cntpuc_numero));
                    //  var ConsultaRnago = db.vw_cuentas_valores.Where(predicateMovAnd2).ToList();

                    #endregion

                    paso = paso + "-1>6-";
                }

                #endregion

                #region cuenta 2

                if (Convert.ToString(wcuentaini).Length == 2 && Convert.ToString(wcuentafin).Length == 2)
                {
                    int ci22 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf22 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gi22 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 2));
                    int gf22 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));


                    string numi22 = Convert.ToString(wcuentaini);
                    string numf22 = Convert.ToString(wcuentafin);
                    //predicate = predicate.And(x => x.clase >= ci22 && x.clase <= cf22 && x.grupo >= gi22 && x.grupo <= gf22  /* && x.cntpuc_numero <= wcuentaini*/ );
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase2 >= ci22 && x.clase2 <= cf22 && x.grupo2 >= gi22 &&
                                 x.grupo2 <= gf22 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-22-";
                }

                if (Convert.ToString(wcuentaini).Length == 2 && Convert.ToString(wcuentafin).Length == 3)
                {
                    int ci23 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf23 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gi23 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 2));
                    int gf23 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));


                    string numi23 = Convert.ToString(wcuentaini);
                    string numf23 = Convert.ToString(wcuentafin);
                    //predicate = predicate.And(x => x.clase >= ci23 && x.clase <= cf23 && x.grupo >= gi23 && x.grupo <= gf23  /* && x.cntpuc_numero <= wcuentaini*/ );
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase2 >= ci23 && x.clase2 <= cf23 && x.grupo2 >= gi23 &&
                                 x.grupo2 <= gf23 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-23-";
                }

                if (Convert.ToString(wcuentaini).Length == 2 && Convert.ToString(wcuentafin).Length == 4)
                {
                    int ci24 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf24 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gi24 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 2));
                    int gf24 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));


                    int xcf24 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 4));

                    string numi24 = Convert.ToString(wcuentaini);
                    string numf24 = Convert.ToString(wcuentafin);
                    //predicate = predicate.And(x => x.clase >= ci24 && x.clase <= cf24 && x.grupo >= gi24 && x.grupo <= gf24 && x.xcuenta <= xcf24 /* && x.cntpuc_numero <= wcuentaini*/ );
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase2 >= ci24 && x.clase2 <= cf24 && x.grupo2 >= gi24 && x.grupo2 <= gf24 &&
                                 x.xcuenta2 <= xcf24 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-24-";
                }

                if (Convert.ToString(wcuentaini).Length == 2 && Convert.ToString(wcuentafin).Length == 5)
                {
                    int ci25 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf25 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gi25 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 2));
                    int gf25 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));


                    int xcf25 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 4));

                    string numi25 = Convert.ToString(wcuentaini);
                    string numf25 = Convert.ToString(wcuentafin);
                    //predicate = predicate.And(x => x.clase >= ci25 && x.clase <= cf25 && x.grupo >= gi25 && x.grupo <= gf25 && x.xcuenta <= xcf25 /* && x.cntpuc_numero <= wcuentaini*/ );
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase2 >= ci25 && x.clase2 <= cf25 && x.grupo2 >= gi25 && x.grupo2 <= gf25 &&
                                 x.xcuenta2 <= xcf25 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-25-";
                }

                if (Convert.ToString(wcuentaini).Length == 2 && Convert.ToString(wcuentafin).Length == 6)
                {
                    int ci26 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf26 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gi26 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 2));
                    int gf26 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));


                    int xcf26 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 4));

                    int scf26 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 6));

                    string numi26 = Convert.ToString(wcuentaini);
                    string numf26 = Convert.ToString(wcuentafin);
                    //predicate = predicate.And(x => x.clase >= ci26 && x.clase <= cf26 && x.grupo >= gi26 && x.grupo <= gf26 && x.xcuenta <= xcf26 && x.subcuenta <= scf26/* && x.cntpuc_numero <= wcuentaini*/ );
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase2 >= ci26 && x.clase2 <= cf26 && x.grupo2 >= gi26 && x.grupo2 <= gf26 &&
                                 x.xcuenta2 <= xcf26 && x.subcuenta2 <= scf26 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-2>5-";
                }

                if (Convert.ToString(wcuentaini).Length == 2 && Convert.ToString(wcuentafin).Length > 6)
                {
                    #region Predicate cuentas

                    string desde22 = Convert.ToString(wcuentaini);
                    string hasta22 = Convert.ToString(wcuentafin);
                    List<string> rango22 = (from p in context.vw_mov_contables
                                            where p.cntpuc_numero.Substring(0, 12).CompareTo(desde22.Substring(0, 12)) >= 0 &&
                                                  p.cntpuc_numero.Substring(0, 12).CompareTo(hasta22.Substring(0, 12)) <= 0
                                            orderby p.cntpuc_numero, p.clase, p.grupo, p.cuenta, p.subcuenta
                                            select p.cntpuc_numero).ToList();
                    predicateMovAnd = predicateMovAnd.And(p => 1 == 1 && rango22.Contains(p.cntpuc_numero));
                    //  var ConsultaRnago = db.vw_cuentas_valores.Where(predicateMovAnd2).ToList();

                    #endregion

                    paso = paso + "-2>6-";
                }

                #endregion

                //  #endregion

                #region cuenta 3

                if (Convert.ToString(wcuentaini).Length == 3 && Convert.ToString(wcuentafin).Length == 3)
                {
                    int ci33 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf33 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gi33 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 2));
                    int gf33 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));

                    string numi33 = Convert.ToString(wcuentaini);
                    string numf33 = Convert.ToString(wcuentafin);
                    //predicate = predicate.And(x => x.clase >= ci33 && x.clase <= cf33 && x.grupo >= gi33 && x.grupo <= gf33 /* && x.cntpuc_numero <= wcuentaini*/ );
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase2 >= ci33 && x.clase2 <= cf33 && x.grupo2 >= gi33 &&
                                 x.grupo2 <= gf33 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = "-33-";
                }

                if (Convert.ToString(wcuentaini).Length == 3 && Convert.ToString(wcuentafin).Length == 4)
                {
                    int ci34 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf34 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gi34 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 2));
                    int gf34 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));

                    int xcf34 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 4));

                    string numi34 = Convert.ToString(wcuentaini);
                    string numf34 = Convert.ToString(wcuentafin);
                    ////predicate = predicate.And(x => x.clase >= ci34 && x.clase <= cf34 && x.grupo >= gi34 && x.grupo <= gf34 && x.xcuenta <= xcf34 /* && x.cntpuc_numero <= wcuentaini*/ );
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase2 >= ci34 && x.clase2 <= cf34 && x.grupo2 >= gi34 && x.grupo2 <= gf34 &&
                                 x.xcuenta2 <= xcf34 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-34-";
                }

                if (Convert.ToString(wcuentaini).Length == 3 && Convert.ToString(wcuentafin).Length == 5)
                {
                    int ci35 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf35 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gi35 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 2));
                    int gf35 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));


                    int xcf35 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 4));

                    string numi35 = Convert.ToString(wcuentaini);
                    string numf35 = Convert.ToString(wcuentafin);
                    //predicate = predicate.And(x => x.clase >= ci35 && x.clase <= cf35 && x.grupo >= gi35 && x.grupo <= gf35 && x.xcuenta <= xcf35 /* && x.cntpuc_numero <= wcuentaini*/ );
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase2 >= ci35 && x.clase2 <= cf35 && x.grupo2 >= gi35 && x.grupo2 <= gf35 &&
                                 x.xcuenta2 <= xcf35 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-35-";
                }

                if (Convert.ToString(wcuentaini).Length == 3 && Convert.ToString(wcuentafin).Length == 6)
                {
                    int ci36 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf36 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gi36 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 2));
                    int gf36 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));


                    int xcf36 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 4));

                    int scf36 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 6));

                    string numi36 = Convert.ToString(wcuentaini);
                    string numf36 = Convert.ToString(wcuentafin);
                    //predicate = predicate.And(x => x.clase >= ci36 && x.clase <= cf36 && x.grupo >= gi36 && x.grupo <= gf36 && x.xcuenta <= xcf36 && x.subcuenta <= scf36/* && x.cntpuc_numero <= wcuentaini*/ );
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase2 >= ci36 && x.clase2 <= cf36 && x.grupo2 >= gi36 && x.grupo2 <= gf36 &&
                                 x.xcuenta2 <= xcf36 && x.subcuenta2 <= scf36 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-3>5-";
                }

                if (Convert.ToString(wcuentaini).Length == 3 && Convert.ToString(wcuentafin).Length > 6)
                {
                    #region Predicate cuentas

                    string desde33 = Convert.ToString(wcuentaini);
                    string hasta33 = Convert.ToString(wcuentafin);
                    List<string> rango33 = (from p in context.vw_mov_contables
                                            where p.cntpuc_numero.Substring(0, 12).CompareTo(desde33.Substring(0, 12)) >= 0 &&
                                                  p.cntpuc_numero.Substring(0, 12).CompareTo(hasta33.Substring(0, 12)) <= 0
                                            orderby p.cntpuc_numero, p.clase, p.grupo, p.cuenta, p.subcuenta
                                            select p.cntpuc_numero).ToList();
                    predicateMovAnd = predicateMovAnd.And(p => 1 == 1 && rango33.Contains(p.cntpuc_numero));
                    //  var ConsultaRnago = db.vw_cuentas_valores.Where(predicateMovAnd2).ToList();

                    #endregion

                    paso = paso + "-1>6-";
                }

                #endregion

                #region cuenta 4

                if (Convert.ToString(wcuentaini).Length == 4 && Convert.ToString(wcuentafin).Length == 4)
                {
                    int ci44 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf44 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gi44 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 2));
                    int gf44 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));

                    int xci44 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 4));
                    int xcf44 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 4));

                    string numi44 = Convert.ToString(wcuentaini);
                    string numf44 = Convert.ToString(wcuentafin);
                    // predicate = predicate.And(x => x.clase >= ci44 && x.clase <= cf44 && x.grupo >= gi44 && x.grupo <= gf44 && x.xcuenta >= xci44 && x.xcuenta <= xcf44 /* && x.cntpuc_numero <= wcuentaini*/ );
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase2 >= ci44 && x.clase2 <= cf44 && x.grupo2 >= gi44 && x.grupo2 <= gf44 &&
                                 x.xcuenta2 >= xci44 && x.xcuenta2 <= xcf44 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-44-";
                }

                if (Convert.ToString(wcuentaini).Length == 4 && Convert.ToString(wcuentafin).Length == 5)
                {
                    int ci45 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf45 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gi45 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 2));
                    int gf45 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));

                    int xci45 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 4));
                    int xcf45 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 4));

                    string numi45 = Convert.ToString(wcuentaini);
                    string numf45 = Convert.ToString(wcuentafin);
                    //  predicate = predicate.And(x => x.clase >= ci45 && x.clase <= cf45 && x.grupo >= gi45 && x.grupo <= gf45 && x.xcuenta >= xci45 && x.xcuenta <= xcf45 /* && x.cntpuc_numero <= wcuentaini*/ );
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase2 >= ci45 && x.clase2 <= cf45 && x.grupo2 >= gi45 && x.grupo2 <= gf45 &&
                                 x.xcuenta2 >= xci45 && x.xcuenta2 <= xcf45 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-45-";
                }

                if (Convert.ToString(wcuentaini).Length == 4 && Convert.ToString(wcuentafin).Length == 6)
                {
                    int ci46 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf46 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gi46 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 2));
                    int gf46 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));

                    int xci46 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 4));
                    int xcf46 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 4));

                    int scf46 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 6));

                    string numi45 = Convert.ToString(wcuentaini);
                    string numf45 = Convert.ToString(wcuentafin);
                    // predicate = predicate.And(x => x.clase >= ci46 && x.clase <= cf46 && x.grupo >= gi46 && x.grupo <= gf46 && x.xcuenta >= xci46 && x.xcuenta <= xcf46 && x.subcuenta <= scf46/* && x.cntpuc_numero <= wcuentaini*/ );
                    predicateMovAnd = predicateMovAnd.And(
                        x => x.clase2 >= ci46 && x.clase2 <= cf46 && x.grupo2 >= gi46 && x.grupo2 <= gf46 &&
                             x.xcuenta2 >= xci46 && x.xcuenta2 <= xcf46 &&
                             x.subcuenta2 <= scf46 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-4>5-";
                }

                if (Convert.ToString(wcuentaini).Length == 4 && Convert.ToString(wcuentafin).Length > 6)
                {
                    #region Predicate cuentas

                    string desde44 = Convert.ToString(wcuentaini);
                    string hasta44 = Convert.ToString(wcuentafin);
                    List<string> rango44 = (from p in context.vw_mov_contables
                                            where p.cntpuc_numero.Substring(0, 12).CompareTo(desde44.Substring(0, 12)) >= 0 &&
                                                  p.cntpuc_numero.Substring(0, 12).CompareTo(hasta44.Substring(0, 12)) <= 0
                                            orderby p.cntpuc_numero, p.clase, p.grupo, p.cuenta, p.subcuenta
                                            select p.cntpuc_numero).ToList();
                    predicateMovAnd = predicateMovAnd.And(p => 1 == 1 && rango44.Contains(p.cntpuc_numero));
                    //  var ConsultaRnago = db.vw_cuentas_valores.Where(predicateMovAnd2).ToList();

                    #endregion

                    paso = paso + "-1>6-";
                }

                #endregion

                #region cuenta 5

                if (Convert.ToString(wcuentaini).Length == 5 && Convert.ToString(wcuentafin).Length == 5)
                {
                    int ci55 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf55 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gi55 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 2));
                    int gf55 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));

                    int xci55 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 4));
                    int xcf55 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 4));

                    string numi55 = Convert.ToString(wcuentaini);
                    string numf55 = Convert.ToString(wcuentafin);
                    //predicate = predicate.And(x => x.clase >= ci55 && x.clase <= cf55 && x.grupo >= gi55 && x.grupo <= gf55 && x.xcuenta >= xci55 && x.xcuenta <= xcf55 /* && x.cntpuc_numero <= wcuentaini*/ );
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase2 >= ci55 && x.clase2 <= cf55 && x.grupo2 >= gi55 && x.grupo2 <= gf55 &&
                                 x.xcuenta2 >= xci55 && x.xcuenta2 <= xcf55 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-55-";
                }

                if (Convert.ToString(wcuentaini).Length == 5 && Convert.ToString(wcuentafin).Length == 6)
                {
                    int ci56 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf56 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gi56 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 2));
                    int gf56 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));

                    int xci56 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 4));
                    int xcf56 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 4));

                    int sci56 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 6));
                    int scf56 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 6));

                    string numi56 = Convert.ToString(wcuentaini);
                    string numf56 = Convert.ToString(wcuentafin);
                    // predicate = predicate.And(x => x.clase >= ci56 && x.clase <= cf56 && x.grupo >= gi56 && x.grupo <= gf56 && x.xcuenta >= xci56 && x.xcuenta <= xcf56 && x.subcuenta >= sci56 && x.subcuenta <= scf56 /*&& x.cntpuc_numero <= wcuentaini*/ );
                    predicateMovAnd = predicateMovAnd.And(
                        x => x.clase2 >= ci56 && x.clase2 <= cf56 && x.grupo2 >= gi56 && x.grupo2 <= gf56 &&
                             x.xcuenta2 >= xci56 && x.xcuenta2 <= xcf56 && x.subcuenta2 >= sci56 &&
                             x.subcuenta2 <= scf56 /*&& x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-5>5-";
                }

                if (Convert.ToString(wcuentaini).Length == 5 && Convert.ToString(wcuentafin).Length > 6)
                {
                    #region Predicate cuentas

                    string desde55 = Convert.ToString(wcuentaini);
                    string hasta55 = Convert.ToString(wcuentafin);
                    List<string> rango55 = (from p in context.vw_mov_contables
                                            where p.cntpuc_numero.Substring(0, 12).CompareTo(desde55.Substring(0, 12)) >= 0 &&
                                                  p.cntpuc_numero.Substring(0, 12).CompareTo(hasta55.Substring(0, 12)) <= 0
                                            orderby p.cntpuc_numero, p.clase, p.grupo, p.cuenta, p.subcuenta
                                            select p.cntpuc_numero).ToList();
                    predicateMovAnd = predicateMovAnd.And(p => 1 == 1 && rango55.Contains(p.cntpuc_numero));
                    //  var ConsultaRnago = db.vw_cuentas_valores.Where(predicateMovAnd2).ToList();

                    #endregion

                    paso = paso + "-1>6-";
                }

                #endregion

                #region cuenta 6

                if (Convert.ToString(wcuentaini).Length == 6 && Convert.ToString(wcuentafin).Length == 6)
                {
                    int ci66 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf66 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gi66 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 2));
                    int gf66 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));

                    int xci66 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 4));
                    int xcf66 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 4));

                    int sci66 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 6));
                    int scf66 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 6));

                    string numi66 = Convert.ToString(wcuentaini);
                    string numf66 = Convert.ToString(wcuentafin);
                    // predicate = predicate.And(x => x.clase >= ci66 && x.clase <= cf66 && x.grupo >= gi66 && x.grupo <= gf66 && x.xcuenta >= xci66 && x.xcuenta <= xcf66 && x.subcuenta >= sci66 && x.subcuenta <= scf66 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd = predicateMovAnd.And(x =>
                        Convert.ToInt32(x.clase) >= ci66 && Convert.ToInt32(x.clase) <= cf66 &&
                        Convert.ToInt32(x.grupo) >= gi66 && Convert.ToInt32(x.grupo) <= gf66 &&
                        Convert.ToInt32(x.xcuenta) >= xci66 && Convert.ToInt32(x.xcuenta) <= xcf66 &&
                        Convert.ToInt32(x.subcuenta) >= sci66 &&
                        Convert.ToInt32(x.subcuenta) <= scf66 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-6-6-";
                }

                if (Convert.ToString(wcuentaini).Length >= 6 && Convert.ToString(wcuentafin).Length > 6)
                {
                    #region Predicate cuentas

                    string desde66 = Convert.ToString(wcuentaini);
                    string hasta66 = Convert.ToString(wcuentafin);
                    List<string> rango66 = (from p in context.vw_mov_contables
                                            where p.cntpuc_numero.Substring(0, 12).CompareTo(desde66.Substring(0, 12)) >= 0 &&
                                                  p.cntpuc_numero.Substring(0, 12).CompareTo(hasta66.Substring(0, 12)) <= 0
                                            orderby p.cntpuc_numero, p.clase, p.grupo, p.cuenta, p.subcuenta
                                            select p.cntpuc_numero).ToList();
                    predicateMovAnd = predicateMovAnd.And(p => 1 == 1 && rango66.Contains(p.cntpuc_numero));
                    //  var ConsultaRnago = db.vw_cuentas_valores.Where(predicateMovAnd2).ToList();

                    #endregion

                    paso = paso + "-6>6-";
                }

                #endregion

                // predicate = predicate.And(predicate2);
                paso = paso + "-";
            }

            #region Nit

            if (widtercero != null)
            {
                predicateMovAnd = predicateMovAnd.And(x => x.nit == widtercero);
            }

            #endregion

            #region Centros

            if (Vectorcentros.Count() > 0)
            {
                if (Vectorcentros[0] != "null")
                {
                    foreach (string j in Vectorcentros)
                    {
                        int cen = Convert.ToInt32(j);
                        // predicate2 = predicate2.Or(x => x.centro == cen);
                        predicateMovOr = predicateMovOr.Or(x => x.centro == cen);
                    }

                    //  predicate = predicate.And(predicate2);
                    predicateMovAnd = predicateMovAnd.And(predicateMovOr);
                }
            }

            #endregion

            #region Tipos_Documentos

            if (VectorTipos.Count() > 0)
            {
                if (VectorTipos[0] != "null")
                {
                    foreach (string j in VectorTipos)
                    {
                        int tip = Convert.ToInt32(j);
                        predicateMovOr = predicateMovOr.Or(x => x.tpdoc_id == tip);
                    }

                    predicateMovAnd = predicateMovAnd.And(predicateMovOr);
                }
            }

            #endregion

            string wpaso = paso;

            #endregion

            #region validacion de cuentas

            string desde = Convert.ToString(wcuentaini);
            if (string.IsNullOrWhiteSpace(desde))
            {
                //desde = (from c in db.mov_contable
                //         join p in db.cuenta_puc
                //         on c.cuenta equals p.cntpuc_id
                //         where p.esafectable == true
                //         orderby c.cuenta
                //         select p.cntpuc_numero).FirstOrDefault();
            }

            string hasta = Convert.ToString(wcuentafin);
            if (string.IsNullOrWhiteSpace(hasta))
            {
                //hasta = (from c in db.mov_contable
                //         join p in db.cuenta_puc
                //         on c.cuenta equals p.cntpuc_id
                //         where p.esafectable == true
                //         orderby c.cuenta descending
                //         select p.cntpuc_numero).FirstOrDefault();
            }

            #endregion

            //var ini = (from c in db.cuenta_puc
            //           where c.cntpuc_numero == desde
            //           orderby c.cuenta
            //           select c.cntpuc_id).FirstOrDefault();

            //var fin = (from c in db.cuenta_puc
            //           where c.cntpuc_numero == hasta
            //           orderby c.cuenta
            //           select c.cntpuc_id).FirstOrDefault();


            //var rango = (from p in db.cuenta_puc
            //             where p.cntpuc_numero.Substring(0, 12).CompareTo(desde.Substring(0, 12)) >= 0 && p.cntpuc_numero.Substring(0, 12).CompareTo(hasta.Substring(0, 12)) <= 0 && p.esafectable == true
            //             orderby p.clase, p.grupo, p.cuenta, p.subcuenta
            //             select p.cntpuc_id).ToList();

            #region Declaro los predicate a utilizar

            System.Linq.Expressions.Expression<Func<vw_contabilidad, bool>> anioP = PredicateBuilder.True<vw_contabilidad>();
            System.Linq.Expressions.Expression<Func<vw_contabilidad, bool>> nitP = PredicateBuilder.False<vw_contabilidad>();
            System.Linq.Expressions.Expression<Func<vw_contabilidad, bool>> centroP = PredicateBuilder.False<vw_contabilidad>();
            System.Linq.Expressions.Expression<Func<vw_contabilidad, bool>> cuentaP = PredicateBuilder.False<vw_contabilidad>();

            //anioP = anioP.And(x => x.fec == anio);
            //anioP = anioP.And(x => x.fec.Month == mes);
            anioP = anioP.And(x => x.fec >= fechaIni);
            anioP = anioP.And(x => x.fec <= fechaFin);

            #region Predicate cuentas

            // anioP = anioP.And(p => 1 == 1 && rango.Contains(p.cuenta));

            #endregion

            #region Predicate nit

            if (widtercero != null)
            {
                nitP = nitP.Or(p => p.nit == widtercero);
                anioP = anioP.And(nitP);
            }

            #endregion

            #region Predicate centro

            if (Vectorcentros.Count() > 0)
            {
                if (Vectorcentros[0] != "null")
                {
                    foreach (string j in Vectorcentros)
                    {
                        int cen = Convert.ToInt32(j);
                        centroP = centroP.Or(x => x.centro == cen);
                    }

                    anioP = anioP.And(centroP);
                }
            }

            //if (centro != null)
            //{

            //    centroP = centroP.Or(p => p.centro == centro);
            //    anioP = anioP.And(centroP);
            //}

            #endregion

            #endregion

            //ViewBag.checkMovimiento = checkMovimiento;
            //ViewBag.checkNiff = checkNiff;
            //ViewBag.checkCentro = checkCentro;
            //ViewBag.checkNit = checkNit;


            ViewBag.checkMovimiento = true;
            ViewBag.checkNiff = true;
            ViewBag.checkCentro = true;
            ViewBag.checkNit = true;


            ViewAsPdf something = new ViewAsPdf("repconAux", "");

            List<vw_contabilidad> consulta = context.vw_contabilidad.Where(anioP).ToList();

            predicateMovAnd = predicateMovAnd.And(x => x.debito + x.credito + x.debitoniif + x.creditoniif != 0);
            //var numclase=
            //predicateMovAnd = predicateMovAnd.And(x => Convert.ToInt32(x.clase) > 0);

            if (checkNiff == false)
            {
                predicateMovAnd = predicateMovAnd.And(x => x.debito + x.credito != 0);
            }

            if (checkNiff)
            {
                predicateMovAnd = predicateMovAnd.And(x => x.debitoniif + x.creditoniif != 0);
            }

            List<vw_contabilidad> consulta_con = context.vw_contabilidad.Where(predicateMovAnd).ToList();

            string root = Server.MapPath("~/Pdf/");
            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
            string path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);


            //// ******** para Movimientos Inicio **********

            #region select de Mov

            ///*****  MOVIMIENTOS AGRUPADOS - INICIO
            List<Bodegas01> seleccion_de_Bod_01 = consulta_con.GroupBy(bo1 => new { bo1.bodega }).Select(bo1 => new Bodegas01
            {
                Id_Bodega = bo1.Key.bodega, //  Id_Bodega = bo1.Key.bodega ?? 0,
                cod_bodega = bo1.Select(f => f.bodccs_cod).First(),
                nom_bodega = bo1.Select(f => f.bodccs_nombre).First(),
                Btotaldebito = bo1.Sum(f => Convert.ToDecimal(f.debito, miCultura)),
                Btotalcredito = bo1.Sum(f => Convert.ToDecimal(f.credito, miCultura)),
                Btotalsaldo = bo1.Sum(f => Convert.ToDecimal(f.debito, miCultura) - Convert.ToDecimal(f.credito, miCultura)),
                //   TerceroA01 = bo1.GroupBy(ter => new { ter.nit }).Select(ter => new Tercero01
                CuentaA01 = bo1.GroupBy(cta => new { cta.cuenta }).Select(cta => new Cuenta01
                {
                    Id_cuenta = cta.Key.cuenta ?? 0,
                    cod_cuenta = cta.Select(f => f.cntpuc_numero).First(),
                    nom_cuenta = cta.Select(f => f.cntpuc_descp).First(),
                    Ctotaldebito = cta.Sum(f => Convert.ToDecimal(f.debito, miCultura)),
                    Ctotalcredito = cta.Sum(f => Convert.ToDecimal(f.credito, miCultura)),
                    Ctotalsaldo = cta.Sum(f => Convert.ToDecimal(f.debito, miCultura) - Convert.ToDecimal(f.credito, miCultura)),
                    TerceroA01 = cta.GroupBy(ter => new { ter.nit }).Select(ter => new Tercero01
                    {
                        Id_tercero = ter.Key.nit,
                        doc_tercero = ter.Select(f => f.doc_tercero).First(),
                        nom_tercero = ter.Select(f => f.prinom_tercero + ' ' + f.apellido_tercero).First(),
                        Tertotaldebito = ter.Sum(f => Convert.ToDecimal(f.debito, miCultura)),
                        Tertotalcredito = ter.Sum(f => Convert.ToDecimal(f.credito, miCultura)),
                        Tertotalsaldo = ter.Sum(f => Convert.ToDecimal(f.debito, miCultura) - Convert.ToDecimal(f.credito, miCultura)),
                        CentroCostoA01 = ter.GroupBy(cc => new { cc.centro }).Select(cc => new CentroCosto01
                        {
                            Id_Centro = cc.Key.centro ?? 0,
                            pref_Centro = cc.Select(f => Convert.ToString(f.centro)).First(),
                            nom_Centro = cc.Select(f => f.centcst_nombre).First(),
                            CCtotaldebito = cc.Sum(f => Convert.ToDecimal(f.debito, miCultura)),
                            CCtotalcredito = cc.Sum(f => Convert.ToDecimal(f.credito)),
                            CCtotalsaldo = cc.Sum(f => Convert.ToDecimal(f.debito, miCultura) - Convert.ToDecimal(f.credito, miCultura)),
                            DocumentosA01 = cc.GroupBy(doc => new { doc.tpdoc_id }).Select(doc => new Documentos01
                            {
                                Id_documento = doc.Key.tpdoc_id ?? 0,
                                cod_documento = doc.Select(f => f.prefijo).First(),
                                nom_documento = doc.Select(f => f.tpdoc_nombre).First(),
                                fechaDocumento = doc.Select(f => Convert.ToString(f.fec.Value.ToShortDateString()))
                                      .First(),
                                Dtotaldebito = doc.Sum(f => Convert.ToDecimal(f.debito, miCultura)),
                                Dtotalcredito = doc.Sum(f => Convert.ToDecimal(f.credito, miCultura)),
                                Dtotalsaldo = doc.Sum(f => Convert.ToDecimal(f.debito, miCultura) - Convert.ToDecimal(f.credito, miCultura))
                            }).OrderBy(c => c.Id_documento).ToList()
                        }).OrderBy(c => c.Id_Centro).ToList()
                    }).OrderBy(c => c.Id_tercero).ToList()
                }).OrderBy(c => c.Id_cuenta).ToList()
            }).OrderBy(c => c.Id_Bodega).ToList();

            //*****  MOVIMIENTOS AGRUPADOS -FIN

            #endregion


            ViewAsPdf salida = new ViewAsPdf("repconAux", "");
            contabilidadAuxiliar reporte10 = new contabilidadAuxiliar
            {
                titulo = "Reporte Contable Auxiliar",
                fechaReporte = DateTime.Now,
                fechaInicio = Convert.ToString(fechaIni),
                fechafin = Convert.ToString(fechaFin),
                cuentaIni = Convert.ToString(wcuentaini),
                cuentaFin = Convert.ToString(wcuentafin),
                // Grantotalsaldoini = seleccionSegundo_04.Sum((f => f.Ctotalsaldoini)),
                Grantotaldebito = seleccion_de_Bod_01.Sum(f => f.Btotaldebito),
                Grantotalcredito = seleccion_de_Bod_01.Sum(f => f.Btotalcredito),
                Grantotalsaldo = seleccion_de_Bod_01.Sum(f => f.Btotalsaldo),
                //  Grantotalsaldoininiff = seleccionSegundo_04.Sum((f => f.Ctotalsaldoininiff)),
                Grantotaldebitoniff = seleccion_de_Bod_01.Sum(f => f.Btotaldebitoniff),
                Grantotalcreditoniff = seleccion_de_Bod_01.Sum(f => f.Btotalcreditoniff),
                Grantotalsaldoniff = seleccion_de_Bod_01.Sum(f => f.Btotalsaldoniff),
                //seleccion_cuenta = seleccionCentro,
                BodegasA01 = seleccion_de_Bod_01
            };

            salida = new ViewAsPdf("repconAux", reporte10);
            something = salida;


            //// ******** para Movimientos Fin **********

            return something;
        }
    }
}