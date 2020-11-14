using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Rotativa;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class LibrosContablesController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");

        private readonly CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

        public ActionResult CuentaMayor(int? menu)
        {
            //ListasDesplegables();
            //BuscarFavoritos(menu);
            return View();
        }

        public ActionResult CuentaDiaria(int? menu)
        {
            //ListasDesplegables();
            //BuscarFavoritos(menu);
            return View();
        }

        public ActionResult InformeMayor(int ano, int mes, bool local, bool niff)
        {
            string root = Server.MapPath("~/Pdf/");
            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
            string path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);


            var cuentaMayorLocal = (from v in context.vw_cuenta_mayor
                                    where v.ano == ano && v.mes == mes
                                    select new
                                    {
                                        ano_cuenta = v.ano,
                                        mes_cuenta = v.mes
                                    }).FirstOrDefault();

            #region meses Letras

            string mesLetras = "";
            if (mes == 1)
            {
                mesLetras = "Enero";
            }
            else if (mes == 2)
            {
                mesLetras = "Febrero";
            }
            else if (mes == 3)
            {
                mesLetras = "Marzo";
            }
            else if (mes == 4)
            {
                mesLetras = "Abril";
            }
            else if (mes == 5)
            {
                mesLetras = "Mayo";
            }
            else if (mes == 6)
            {
                mesLetras = "Junio";
            }
            else if (mes == 7)
            {
                mesLetras = "Julio";
            }
            else if (mes == 8)
            {
                mesLetras = "Agosto";
            }
            else if (mes == 9)
            {
                mesLetras = "Septiembre";
            }
            else if (mes == 10)
            {
                mesLetras = "Octubre";
            }
            else if (mes == 11)
            {
                mesLetras = "Noviembre";
            }
            else if (mes == 12)
            {
                mesLetras = "Diciembre";
            }

            #endregion

            if (local)
            {
                PDFmodel libroMayorLocal = new PDFmodel
                {
                    nombre_libro = "Local",
                    anio_cuenta = cuentaMayorLocal.ano_cuenta,
                    mes_cuenta = mesLetras,

                    referencias = (from v in context.vw_cuenta_mayor
                                   where v.ano == ano && v.mes == mes
                                   select new referenciasPDF
                                   {
                                       numero_cuenta = v.cntpuc_numero,
                                       descripcion_cuenta = v.cntpuc_descp,
                                       credito = v.credito,
                                       debito = v.debito,
                                       saldo_inicial = v.saldo_ini,
                                       saldo_actual = v.debito + v.saldo_ini - v.credito
                                   }).ToList()
                };

                decimal total_saldo_actual = libroMayorLocal.referencias.Sum(d => d.saldo_actual);
                libroMayorLocal.total_saldoActual = Convert.ToDecimal(total_saldo_actual, miCultura);

                decimal total_saldo_inicial = libroMayorLocal.referencias.Sum(d => d.saldo_inicial);
                libroMayorLocal.total_saldoInicial = Convert.ToDecimal(total_saldo_inicial, miCultura);

                decimal total_debito = libroMayorLocal.referencias.Sum(d => d.debito);
                libroMayorLocal.total_debito = Convert.ToDecimal(total_debito, miCultura);

                decimal total_credito = libroMayorLocal.referencias.Sum(d => d.credito);
                libroMayorLocal.total_credito = Convert.ToDecimal(total_credito, miCultura);


                ViewAsPdf something = new ViewAsPdf("InformeMayor", libroMayorLocal);
                return something;
            }
            else
            {
                PDFmodel libroMayorLocal = new PDFmodel
                {
                    nombre_libro = "NIFF",
                    anio_cuenta = cuentaMayorLocal.ano_cuenta,
                    mes_cuenta = mesLetras,

                    referencias = (from v in context.vw_cuenta_mayor
                                   where v.ano == ano && v.mes == mes
                                   select new referenciasPDF
                                   {
                                       numero_cuenta = v.cntpuc_numero,
                                       descripcion_cuenta = v.cntpuc_descp,
                                       credito = v.creditoniff,
                                       debito = v.debitoniff,
                                       saldo_inicial = v.saldo_ininiff,
                                       saldo_actual = v.debitoniff + v.saldo_ininiff - v.creditoniff
                                   }).ToList()
                };
                decimal total_saldo_actual = libroMayorLocal.referencias.Sum(d => d.saldo_actual);
                libroMayorLocal.total_saldoActual = Convert.ToDecimal(total_saldo_actual, miCultura);

                decimal total_saldo_inicial = libroMayorLocal.referencias.Sum(d => d.saldo_inicial);
                libroMayorLocal.total_saldoInicial = Convert.ToDecimal(total_saldo_inicial, miCultura);

                decimal total_debito = libroMayorLocal.referencias.Sum(d => d.debito);
                libroMayorLocal.total_debito = Convert.ToDecimal(total_debito, miCultura);

                decimal total_credito = libroMayorLocal.referencias.Sum(d => d.credito);
                libroMayorLocal.total_credito = Convert.ToDecimal(total_credito, miCultura);

                ViewAsPdf something = new ViewAsPdf("InformeMayor", libroMayorLocal);
                return something;
            }

            return View();
        }

        public ActionResult InformeDiario(int ano, int mes, bool local, bool niff)
        {
            var cuentaMayorLocal = (from v in context.vw_cuenta_diario
                                    where v.ano == ano && v.mes == mes
                                    select new
                                    {
                                        ano_cuenta = v.ano ?? 0,
                                        mes_cuenta = v.mes
                                    }).FirstOrDefault();

            #region meses Letras

            string mesLetras = "";
            if (mes == 1)
            {
                mesLetras = "Enero";
            }
            else if (mes == 2)
            {
                mesLetras = "Febrero";
            }
            else if (mes == 3)
            {
                mesLetras = "Marzo";
            }
            else if (mes == 4)
            {
                mesLetras = "Abril";
            }
            else if (mes == 5)
            {
                mesLetras = "Mayo";
            }
            else if (mes == 6)
            {
                mesLetras = "Junio";
            }
            else if (mes == 7)
            {
                mesLetras = "Julio";
            }
            else if (mes == 8)
            {
                mesLetras = "Agosto";
            }
            else if (mes == 9)
            {
                mesLetras = "Septiembre";
            }
            else if (mes == 10)
            {
                mesLetras = "Octubre";
            }
            else if (mes == 11)
            {
                mesLetras = "Noviembre";
            }
            else if (mes == 12)
            {
                mesLetras = "Diciembre";
            }

            #endregion

            if (local)
            {
                PDFmodel libroDiario = new PDFmodel
                {
                    nombre_libro = "Local",
                    anio_cuenta = cuentaMayorLocal.ano_cuenta,
                    mes_cuenta = mesLetras,
                    cuentas = (from v in context.vw_consolidado_documentoscont
                               where v.ano == ano && v.mes == mes
                               select new documento
                               {
                                   ano = v.ano ?? 0,
                                   mes = v.mes ?? 0,
                                   nombre_documento = v.tpdoc_nombre,
                                   prefijo = v.prefijo,
                                   total_credito = v.total_credito ?? 0,
                                   total_debito = v.total_debito ?? 0,
                                   total_creniff = v.total_creditoniif ?? 0,
                                   total_deniff = v.total_debitoniif ?? 0,
                                   cuentas = context.vw_cuenta_diario
                                       .Where(d => d.ano == ano && d.mes == mes && d.prefijo == v.prefijo).Select(d =>
                                           new cuenta
                                           {
                                               ano = d.ano ?? 0,
                                               mes = d.mes ?? 0,
                                               cntpuc_descp = d.cntpuc_descp,
                                               numerocuenta = d.cuenta,
                                               credito_cuenta = d.credito,
                                               debito_cuenta = d.debito,
                                               crednif_cuenta = d.creditoniif,
                                               debnif_cuenta = d.debitoniif
                                           }).ToList()
                               }
                        ).ToList()
                };

                decimal total_debito = libroDiario.cuentas.Sum(d => d.total_debito);
                libroDiario.total_debito = Convert.ToDecimal(total_debito, miCultura);

                decimal total_credito = libroDiario.cuentas.Sum(d => d.total_credito);
                libroDiario.total_credito = Convert.ToDecimal(total_credito, miCultura);


                ViewAsPdf something = new ViewAsPdf("InformeDiario", libroDiario);
                return something;
            }
            else
            {
                PDFmodel libroDiario = new PDFmodel
                {
                    nombre_libro = "NIFF",
                    anio_cuenta = cuentaMayorLocal.ano_cuenta,
                    mes_cuenta = mesLetras,
                    cuentas = (from v in context.vw_consolidado_documentoscont
                               where v.ano == ano && v.mes == mes
                               select new documento
                               {
                                   ano = v.ano ?? 0,
                                   mes = v.mes ?? 0,
                                   nombre_documento = v.tpdoc_nombre,
                                   prefijo = v.prefijo,
                                   total_credito = v.total_credito ?? 0,
                                   total_debito = v.total_debito ?? 0,
                                   total_creniff = v.total_creditoniif ?? 0,
                                   total_deniff = v.total_debitoniif ?? 0,
                                   cuentas = context.vw_cuenta_diario
                                       .Where(d => d.ano == ano && d.mes == mes && d.prefijo == v.prefijo).Select(d =>
                                           new cuenta
                                           {
                                               ano = d.ano ?? 0,
                                               mes = d.mes ?? 0,
                                               cntpuc_descp = d.cntpuc_descp,
                                               numerocuenta = d.cuenta,
                                               credito_cuenta = d.credito,
                                               debito_cuenta = d.debito,
                                               crednif_cuenta = d.creditoniif,
                                               debnif_cuenta = d.debitoniif
                                           }).ToList()
                               }
                        ).ToList()
                };

                decimal total_debito = libroDiario.cuentas.Sum(d => d.total_deniff);
                libroDiario.total_debito = Convert.ToDecimal(total_debito, miCultura);

                decimal total_credito = libroDiario.cuentas.Sum(d => d.total_creniff);
                libroDiario.total_credito = Convert.ToDecimal(total_credito, miCultura);


                ViewAsPdf something = new ViewAsPdf("InformeDiario", libroDiario);
                return something;
            }


            return View();
        }
    }
}