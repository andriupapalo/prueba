using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class consultaGeneralCuentasController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: consultaGeneralCuentas
        public ActionResult Index()
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

            return View();
        }

        public JsonResult BuscarInformacionCuentas(string cuentaInicio, string cuentaFin, DateTime? FechaInicio,
            DateTime? FechaFin, int[] centros, bool? filtroCentro, bool? filtroNit, int? nit, bool filtroMovimiento)
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

                /// //FechaFin = FechaFin.Value.AddMonths(1).AddDays(-1);

                int A1 = cuenti.Length;
                int B2 = cuentf.Length;

                //var cuentaInicioLv1 = cuentaInicio != null ? cuentaInicio.Substring(0, 1) : "1";
                //var cuentaFinLv1 = cuentaFin != null ? cuentaFin.Substring(0, 1) : "9";

                string cuentaInicioLv1 = cuentaInicio != null ? cuentaInicio.Substring(0, A1) : "1";
                string cuentaFinLv1 = cuentaFin != null ? cuentaFin.Substring(0, B2) : "9";

                //var cuentaInicioLv1 = cuentaInicio != null ? cuentaInicio : "1";
                //var cuentaFinLv1 = cuentaFin != null ? cuentaFin : "9";


                List<string> indices = new List<string>();
                for (int i = Convert.ToInt32(cuentaInicioLv1); i <= Convert.ToInt32(cuentaFinLv1); i++)
                {
                    indices.Add(i.ToString());
                }

                List<CuentaConsulta> lista = getNivel1(indices, FechaInicio ?? DateTime.Now, FechaFin ?? DateTime.Now, centros,
                    filtroCentro ?? false, filtroNit ?? false, nit, filtroMovimiento);


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


            //var buscarCuentas = (from cuentaPuc in context.cuenta_puc
            //                    where cuentaPuc.cntpuc_numero.Length == 1 && indices.Contains(cuentaPuc.cntpuc_numero.Substring(0,1))
            //                    select new
            //                    {
            //                        cuentaPuc.cntpuc_numero,
            //                        cuentaPuc.cntpuc_descp,
            //                        sumaInicial = (from cuentasValores in context.cuentas_valores
            //                                       join cuenta in context.cuenta_puc
            //                                       on cuentasValores.cuenta equals cuenta.cntpuc_id
            //                                       where cuenta.cntpuc_numero.Substring(0, 1) == cuentaPuc.cntpuc_numero.Substring(0, 1)
            //                                       && cuentasValores.ano >= FechaInicio.Value.Year && cuentasValores.ano <= FechaFin.Value.Year
            //                                       select cuentasValores.saldo_ini).Sum(),
            //                        debito = (from cuentasValores in context.cuentas_valores
            //                                  join cuenta in context.cuenta_puc
            //                                  on cuentasValores.cuenta equals cuenta.cntpuc_id
            //                                  where cuenta.cntpuc_numero.Substring(0, 1) == cuentaPuc.cntpuc_numero.Substring(0, 1)
            //                                  && cuentasValores.ano >= FechaInicio.Value.Year && cuentasValores.ano <= FechaFin.Value.Year
            //                                  select cuentasValores.debito).Sum(),
            //                        credito = (from cuentasValores in context.cuentas_valores
            //                                   join cuenta in context.cuenta_puc
            //                                   on cuentasValores.cuenta equals cuenta.cntpuc_id
            //                                   where cuenta.cntpuc_numero.Substring(0, 1) == cuentaPuc.cntpuc_numero.Substring(0, 1)
            //                                   && cuentasValores.ano >= FechaInicio.Value.Year && cuentasValores.ano <= FechaFin.Value.Year
            //                                   select cuentasValores.credito).Sum()
            //                    }).ToList();

            //var buscarCuentasNivel1 = (from cuentaPuc in context.cuenta_puc
            //where (cuentaPuc.cntpuc_numero.Length == 1 /*|| cuentaPuc.cntpuc_numero.Length == 2*/) && indices.Contains(cuentaPuc.cntpuc_numero.Substring(0, 1))
            //select new
            //{
            //    cuentaPuc.cntpuc_numero,
            //    cuentaPuc.cntpuc_descp,
            //    total = (from cuentasValores in context.cuentas_valores
            //             join cuenta in context.cuenta_puc
            //             on cuentasValores.cuenta equals cuenta.cntpuc_id
            //             where cuenta.cntpuc_numero.Substring(0, 1) == cuentaPuc.cntpuc_numero.Substring(0, 1)
            //             && cuentasValores.ano >= FechaInicio.Value.Year && cuentasValores.ano <= FechaFin.Value.Year
            //             group cuentasValores by new { cuentasValores.ano, cuentasValores.mes, cuenta.cntpuc_numero, cuenta.cntpuc_descp } into g
            //             select new { anio = g.Key.ano, mes = g.Key.mes/*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/, saldoInicial = g.Sum(x => x.saldo_ini), debito = g.Sum(x => x.debito), credito = g.Sum(x => x.credito) }).ToList(),
            //    //meses =  /*cuentaPuc.cntpuc_numero.Length == 1 ?*/ (from cuentasValores in context.cuentas_valores
            //    //               join cuenta in context.cuenta_puc
            //    //               on cuentasValores.cuenta equals cuenta.cntpuc_id
            //    //               where cuenta.cntpuc_numero.Substring(0, 1) == cuentaPuc.cntpuc_numero.Substring(0, 1)
            //    //               && cuentasValores.ano >= FechaInicio.Value.Year && cuentasValores.ano <= FechaFin.Value.Year
            //    //               group cuentasValores by new { cuentasValores.cuenta, cuentasValores.ano, cuentasValores.mes/*, cuentasValores.debito, cuentasValores.credito*/ } into g
            //    //               select new { cuenta = g.Key.cuenta, anio = g.Key.ano, g.Key.mes, saldoInicial = g.Sum(x => x.saldo_ini), debito = g.Sum(x => x.debito), credito = g.Sum(x=>x.credito) }).ToList()
            //                   //:
            //                   //(from cuentasValores in context.cuentas_valores
            //                   //join cuenta in context.cuenta_puc
            //                   //on cuentasValores.cuenta equals cuenta.cntpuc_id
            //                   //where cuenta.cntpuc_numero.Substring(0, 2) == cuentaPuc.cntpuc_numero.Substring(0, 2)
            //                   //&& cuentasValores.ano >= FechaInicio.Value.Year && cuentasValores.ano <= FechaFin.Value.Year
            //                   //group cuentasValores by new { cuentasValores.cuenta, cuentasValores.ano, cuentasValores.mes/*, cuentasValores.debito, cuentasValores.credito*/ } into g
            //                   //select new { cuenta = g.Key.cuenta, anio = g.Key.ano, g.Key.mes, saldoInicial = g.Sum(x => x.saldo_ini), debito = g.Sum(x => x.debito), credito = g.Sum(x=>x.credito) }).ToList()//:
            //                                                         //select cuentasValores.saldo_ini).Sum() : 
            //                                                             //(from cuentasValores in context.cuentas_valores
            //                                                             //join cuenta in context.cuenta_puc
            //                                                             //on cuentasValores.cuenta equals cuenta.cntpuc_id
            //                                                             //where cuenta.cntpuc_numero.Substring(0, 2) == cuentaPuc.cntpuc_numero.Substring(0, 2)
            //                                                             //&& cuentasValores.ano >= FechaInicio.Value.Year && cuentasValores.ano <= FechaFin.Value.Year
            //                                                             //group cuentasValores by new { cuentasValores.cuenta, cuentasValores.ano,cuentasValores.mes } into g
            //                                                             //select new { cuenta = g.Key.cuenta, anio = g.Key.ano, g.Key.mes, suma = g.Sum(x => x.debito), credito = g.Sum(x => x.credito) }).ToList(),
            //                                                             //select cuentasValores.saldo_ini).Sum(),
            //    //debito = cuentaPuc.cntpuc_numero.Length == 1 ? (from cuentasValores in context.cuentas_valores
            //    //          join cuenta in context.cuenta_puc
            //    //          on cuentasValores.cuenta equals cuenta.cntpuc_id
            //    //          where cuenta.cntpuc_numero.Substring(0, 1) == cuentaPuc.cntpuc_numero.Substring(0, 1)
            //    //          && cuentasValores.ano >= FechaInicio.Value.Year && cuentasValores.ano <= FechaFin.Value.Year
            //    //          select cuentasValores.debito).Sum() :  (from cuentasValores in context.cuentas_valores
            //    //                                                       join cuenta in context.cuenta_puc
            //    //                                                       on cuentasValores.cuenta equals cuenta.cntpuc_id
            //    //                                                       where cuenta.cntpuc_numero.Substring(0, 2) == cuentaPuc.cntpuc_numero.Substring(0, 2)
            //    //                                                       && cuentasValores.ano >= FechaInicio.Value.Year && cuentasValores.ano <= FechaFin.Value.Year
            //    //                                                       select cuentasValores.debito).Sum(),
            //    //credito = cuentaPuc.cntpuc_numero.Length == 1 ? (from cuentasValores in context.cuentas_valores
            //    //           join cuenta in context.cuenta_puc
            //    //           on cuentasValores.cuenta equals cuenta.cntpuc_id
            //    //           where cuenta.cntpuc_numero.Substring(0, 1) == cuentaPuc.cntpuc_numero.Substring(0, 1)
            //    //           && cuentasValores.ano >= FechaInicio.Value.Year && cuentasValores.ano <= FechaFin.Value.Year
            //    //           select cuentasValores.credito).Sum() : (from cuentasValores in context.cuentas_valores
            //    //                                                   join cuenta in context.cuenta_puc
            //    //                                                   on cuentasValores.cuenta equals cuenta.cntpuc_id
            //    //                                                   where cuenta.cntpuc_numero.Substring(0, 2) == cuentaPuc.cntpuc_numero.Substring(0, 2)
            //    //                                                   && cuentasValores.ano >= FechaInicio.Value.Year && cuentasValores.ano <= FechaFin.Value.Year
            //    //                                                   select cuentasValores.credito).Sum()
            //}).OrderBy(x=>x.cntpuc_numero).ToList();


            //var buscarCuentasNivel2 = buscarCuentasNivel1.Select(x=>new {
            //    x.cntpuc_numero,
            //    x.cntpuc_descp,
            //    x.total,
            //    saldoInicial = x.total.Sum(l => l.saldoInicial),
            //    debito = x.total.Sum(l => l.debito),
            //    credito = x.total.Sum(l => l.credito),
            //    nivel0 = x.total.GroupBy(b=>new {b.anio,b.mes }).Select(n=>new { inicial = x.total.Sum(x.) })
            //    //nivel1 = x.total.Select(j=>new {
            //    //    j.cntpuc_numero,
            //    //    j.cntpuc_descp,
            //    //    nivel2 = (from cuentasValores in context.cuentas_valores
            //    //              join cuenta in context.cuenta_puc
            //    //              on cuentasValores.cuenta equals cuenta.cntpuc_id
            //    //              where cuenta.cntpuc_numero.Substring(0, 1) == j.cntpuc_numero.Substring(0, 1)
            //    //              && cuentasValores.ano >= FechaInicio.Value.Year && cuentasValores.ano <= FechaFin.Value.Year
            //    //              group cuentasValores by new { cuenta.cntpuc_numero, cuentasValores.ano, cuentasValores.mes/*, cuentasValores.debito, cuentasValores.credito*/ } into g
            //    //              select new { cntpuc_numero = g.Key.cntpuc_numero, anio = g.Key.ano, g.Key.mes, saldoInicial = g.Sum(k => k.saldo_ini), debito = g.Sum(k => k.debito), credito = g.Sum(k => k.credito) }).ToList()
            //    //}).ToList(),

            //}).ToList();

            //var sumaInicial = (from cuentasValores in context.cuentas_valores
            //                   join cuenta in context.cuenta_puc
            //                   on cuentasValores.cuenta equals cuenta.cntpuc_id
            //                   //where cuenta.cntpuc_numero.Substring(0, 1) == cuentaPuc.cntpuc_numero.Substring(0, 1)
            //                   where cuentasValores.ano >= FechaInicio.Value.Year && cuentasValores.ano <= FechaFin.Value.Year
            //                   select cuentasValores.debito).Sum();

            //var buscarCuents = (from cuentaPuc in context.cuenta_puc
            //                    where cuentaPuc.cntpuc_numero.Length == 1
            //                    select new {
            //                        cntpuc_numeroAux = cuentaPuc.cntpuc_numero.Trim(),
            //                        cuentaPuc.cntpuc_descp,
            //                        sumaInicial = (from cuentasValores in context.cuentas_valores
            //                                       join cuenta in context.cuenta_puc
            //                                       on cuentasValores.cuenta equals cuenta.cntpuc_id
            //                                       where cuenta.cntpuc_numero.Substring(0, 1) == cuentaPuc.cntpuc_numero.Substring(0, 1)
            //                                       && cuentasValores.ano >= FechaInicio.Value.Year && cuentasValores.ano <= FechaFin.Value.Year
            //                                       group cuentasValores by new { cuentasValores.cuenta } into g
            //                                       select new { cuenta= g.Key.cuenta,suma = g.Sum(x=>x.debito)}).ToList().Select(x=>x.suma).Sum()
            //                        //debito = (from cuentasValores in context.cuentas_valores
            //                        //               join cuenta in context.cuenta_puc
            //                        //               on cuentasValores.cuenta equals cuenta.cntpuc_id
            //                        //               where cuenta.cntpuc_numero.Substring(0, 1) == cuentaPuc.cntpuc_numero.Substring(0, 1)
            //                        //               && cuentasValores.ano >= FechaInicio.Value.Year && cuentasValores.ano <= FechaFin.Value.Year
            //                        //               select cuentasValores.debito).Sum(),
            //                        //debito = (from cuentasValores in context.cuentas_valores
            //                        //          join cuenta in context.cuenta_puc
            //                        //          on cuentasValores.cuenta equals cuenta.cntpuc_id
            //                        //          where cuenta.cntpuc_numero.Substring(0, 1) == cuentaPuc.cntpuc_numero.Substring(0, 1)
            //                        //          && cuentasValores.ano >= FechaInicio.Value.Year && cuentasValores.ano <= FechaFin.Value.Year
            //                        //          select cuentasValores.debito).Sum(),
            //                        //credito = (from cuentasValores in context.cuentas_valores
            //                        //           join cuenta in context.cuenta_puc
            //                        //           on cuentasValores.cuenta equals cuenta.cntpuc_id
            //                        //           where cuenta.cntpuc_numero.Substring(0, 1) == cuentaPuc.cntpuc_numero.Substring(0, 1)
            //                        //           && cuentasValores.ano >= FechaInicio.Value.Year && cuentasValores.ano <= FechaFin.Value.Year
            //                        //           select cuentasValores.credito).Sum()
            //                    }).ToList();

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
                                 //group cuentaValor by new { cuentaValor.ano, cuentaValor.mes/*, cuenta.cntpuc_numero, cuenta.cntpuc_descp*/ } into g
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
                //var indicesNivel2 = (from cuenta in context.cuenta_puc
                //                     where cuenta.cntpuc_numero.Length == 2 && cuenta.cntpuc_numero.Substring(0, 1) == indicesUno.cntpuc_numero
                //                     select new { cuenta.cntpuc_numero, cuenta.cntpuc_descp }).ToList();
                //var indicesNumericos = indicesNivel1.Select(x => x.cntpuc_numero).ToList();

                var buscarCuentasNivel1 = (from cuentaValor in context.cuentas_valores
                                           join cuentaPuc in context.cuenta_puc
                                               on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                           where /*cuentaValor.cuenta || cuentaPuc.cntpuc_numero.Length == 2*/
                                               cuentaPuc.cntpuc_numero.Substring(0, 1) == indicesUno.cntpuc_numero
                                               //where /*(cuentaPuc.cntpuc_numero.Length == 2) &&*/
                                               //&& indicesNumericos.Contains(cuentaPuc.cntpuc_numero.Substring(0, 1))
                                               && cuentaValor.ano == indicesUno.ano && cuentaValor.mes == indicesUno.mes
                                           //&& cuentaValor.ano >= FechaInicio.Year && cuentaValor.ano <= FechaFin.Year && cuentaValor.mes >= FechaInicio.Month && cuentaValor.mes <= FechaFin.Month
                                           select new
                                           {
                                               //cuentaPuc.cntpuc_id,
                                               nivel = cuentaPuc.cntpuc_numero.Substring(0, 1),
                                               indicesUno.esafectable,
                                               //cuentaPuc.cntpuc_numero,
                                               //cntpuc_descp = (from indicesNombre in indicesNivel2 where indicesNombre.cntpuc_numero == cuentaPuc.cntpuc_numero.Substring(0, 2) select indicesNombre.cntpuc_descp),
                                               //cntpuc_descp = indicesNivel2.FirstOrDefault(x=>x.cntpuc_numero == cuentaPuc.cntpuc_numero.Substring(0, 2)).cntpuc_descp,
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
                    //foreach (var items2 in item.total)
                    //{
                    listaCuentas.Add(new CuentaConsulta
                    {
                        //cuentaId = item.cntpuc_id,
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
                //}
                //cuentaNivelAnterior.cuentasHijas = listaCuentas;
            }

            //List<CuentaConsulta> listaCuentas = new List<CuentaConsulta>();
            //var buscarCuentasNivel1 = //(from cuentaPuc in context.cuenta_puc
            //                            (from cuentaValor in context.cuentas_valores
            //                             join cuentaPuc in context.cuenta_puc
            //                             on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
            //                             where /*cuentaPuc.cntpuc_numero.Length == 1 || cuentaPuc.cntpuc_numero.Length == 2*/ indices.Contains(cuentaPuc.cntpuc_numero.Substring(0, 1))
            //                             && cuentaValor.ano >= FechaInicio.Year && cuentaValor.mes <= FechaFin.Year && cuentaValor.mes >= FechaInicio.Month && cuentaValor.mes <= FechaFin.Month
            //                             select new
            //                           {
            //                               cuentaPuc.cntpuc_id,
            //                               cuentaPuc.cntpuc_numero,
            //                               cuentaPuc.cntpuc_descp,
            //                               total = (from cuentasValores in context.cuentas_valores
            //                                        join cuenta in context.cuenta_puc
            //                                        on cuentasValores.cuenta equals cuenta.cntpuc_id
            //                                        where cuenta.cntpuc_numero.Substring(0, 1) == cuentaPuc.cntpuc_numero.Substring(0, 1)
            //                                        && cuentasValores.ano >= FechaInicio.Year && cuentasValores.ano <= FechaFin.Year
            //                                        group cuentasValores by new { cuentasValores.ano, cuentasValores.mes/*, cuenta.cntpuc_numero, cuenta.cntpuc_descp*/ } into g
            //                                        select new { anio = g.Key.ano, mes = g.Key.mes/*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/, saldoInicial = g.Sum(x => x.saldo_ini),
            //                                            debito = g.Sum(x => x.debito), credito = g.Sum(x => x.credito) }).ToList(),
            //                           }).OrderBy(x => x.cntpuc_numero).ToList();

            //foreach (var item in buscarCuentasNivel1) {
            //    foreach (var items2 in item.total) {
            //        listaCuentas.Add(new CuentaConsulta() {
            //            cuentaId = item.cntpuc_id,
            //            cuenta = item.cntpuc_numero,
            //            cuentaDescripcion = item.cntpuc_descp,
            //            ano = items2.anio,
            //            mes= items2.mes,
            //            saldo_ini = items2.saldoInicial??0,
            //            debito = items2.debito??0,
            //            credito = items2.credito??0,
            //            total = (items2.saldoInicial ?? 0) + (items2.debito ?? 0) - (items2.credito ?? 0)
            //        });
            //    }
            //}

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
                                               where /*cuentaValor.cuenta || cuentaPuc.cntpuc_numero.Length == 2*/
                                                   cuentaPuc.cntpuc_numero.Substring(0, 1) == cuenta.cuenta
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
                        //foreach (var items2 in item.total)
                        //{
                        listaCentros.Add(new CuentaConsulta
                        {
                            //cuentaId = item.cntpuc_id,
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
                    //}
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
                        //foreach (var items2 in item.total)
                        //{
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
                    //}
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
                        //cuenta.NitsHijos = listaNits;
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
                                           where /*cuentaValor.cuenta || cuentaPuc.cntpuc_numero.Length == 2*/
                                               cuentaPuc.cntpuc_numero.Substring(0, 1) == cuentaNivelAnterior.cuenta
                                               //where /*(cuentaPuc.cntpuc_numero.Length == 2) &&*/
                                               && indicesNumericos.Contains(cuentaPuc.cntpuc_numero.Substring(0, 2))
                                               && cuentaValor.ano == cuentaNivelAnterior.ano && cuentaValor.mes == cuentaNivelAnterior.mes
                                           select new
                                           {
                                               //cuentaPuc.cntpuc_id,
                                               nivel = cuentaPuc.cntpuc_numero.Substring(0, 2),
                                               //cuentaPuc.cntpuc_numero,
                                               //cntpuc_descp = (from indicesNombre in indicesNivel2 where indicesNombre.cntpuc_numero == cuentaPuc.cntpuc_numero.Substring(0, 2) select indicesNombre.cntpuc_descp),
                                               //cntpuc_descp = indicesNivel2.FirstOrDefault(x=>x.cntpuc_numero == cuentaPuc.cntpuc_numero.Substring(0, 2)).cntpuc_descp,
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
                    //foreach (var items2 in item.total)
                    //{
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
                        var buscarCentrosNivel2 = (from cuentaValor in context.cuentas_valores
                                                   join centro in context.centro_costo
                                                       on cuentaValor.centro equals centro.centcst_id
                                                   join cuentaPuc in context.cuenta_puc
                                                       on cuentaValor.cuenta equals cuentaPuc.cntpuc_id
                                                   where /*cuentaValor.cuenta || cuentaPuc.cntpuc_numero.Length == 2*/
                                                       cuentaPuc.cntpuc_numero.Substring(0, 2) == cuenta.cuenta
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
                            //foreach (var items2 in item.total)
                            //{
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
                        //}
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
                            //cuenta.NitsHijos = listaNits;
                        }
                    }
                }
            }


            //var indicesNivelAnterior = cuentasNivelAnterior.Select(x => x.cuenta).ToList().Distinct();
            //foreach (var cuentaNivelAnterior in cuentasNivelAnterior) {
            //List<CuentaConsulta> listaCuentas = new List<CuentaConsulta>();
            //var buscarCuentasNivel2 = (from cuentaPuc in context.cuenta_puc
            //                           join cuentasValor in context.cuentas_valores
            //                           on cuentaPuc.cntpuc_id equals cuentasValor.cuenta
            //                           where (cuentaPuc.cntpuc_numero.Length == 2) && indices.Contains(cuentaPuc.cntpuc_numero.Substring(0, 2))
            //                           //&& cuentasValor.ano == cuentaNivelAnterior.ano && cuentasValor.mes == cuentaNivelAnterior.mes 
            //                           select new
            //                           {
            //                               cuentaPuc.cntpuc_id,
            //                               cuentaPuc.cntpuc_numero,
            //                               cuentaPuc.cntpuc_descp,
            //                               total = (from cuentasValores in context.cuentas_valores
            //                                        join cuenta in context.cuenta_puc
            //                                        on cuentasValores.cuenta equals cuenta.cntpuc_id
            //                                        where cuenta.cntpuc_numero.Substring(0, 2) == cuentaPuc.cntpuc_numero.Substring(0, 2)
            //                                        && cuentasValores.ano >= cuentaNivelAnterior.ano && cuentasValores.ano <= cuentaNivelAnterior.ano 
            //                                        && cuentasValores.mes >= cuentaNivelAnterior.mes && cuentasValores.mes <= cuentaNivelAnterior.mes
            //                                        group cuentasValores by new { cuentasValores.ano, cuentasValores.mes/*, cuenta.cntpuc_numero, cuenta.cntpuc_descp*/ } into g
            //                                        select new
            //                                        {
            //                                            anio = g.Key.ano,
            //                                            mes = g.Key.mes/*, g.Key.cntpuc_numero, g.Key.cntpuc_descp,*/,
            //                                            saldoInicial = g.Sum(x => x.saldo_ini),
            //                                            debito = g.Sum(x => x.debito),
            //                                            credito = g.Sum(x => x.credito)
            //                                        }).ToList(),
            //                           }).OrderBy(x => x.cntpuc_numero).ToList();


            //foreach (var item in buscarCuentasNivel2)
            //{
            //    foreach (var items2 in item.total)
            //    {
            //        listaCuentas.Add(new CuentaConsulta()
            //        {
            //            cuentaId = item.cntpuc_id,
            //            cuenta = item.cntpuc_numero,
            //            cuentaDescripcion = item.cntpuc_descp,
            //            ano = items2.anio,
            //            mes = items2.mes,
            //            saldo_ini = items2.saldoInicial ?? 0,
            //            debito = items2.debito ?? 0,
            //            credito = items2.credito ?? 0,
            //            total = (items2.saldoInicial ?? 0) + (items2.debito ?? 0) - (items2.credito ?? 0)
            //        });
            //    }
            //}
            //cuentaNivelAnterior.cuentasHijas = listaCuentas;
            //}

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
                                               where /*cuentaValor.cuenta || cuentaPuc.cntpuc_numero.Length == 2*/
                                                   cuentaPuc.cntpuc_numero.Substring(0, 2) == cuentaNivelAnterior.cuenta
                                                   //where /*(cuentaPuc.cntpuc_numero.Length == 2) &&*/
                                                   && indicesNumericos.Contains(cuentaPuc.cntpuc_numero.Substring(0, 4))
                                                   && cuentaValor.ano == cuentaNivelAnterior.ano && cuentaValor.mes == cuentaNivelAnterior.mes
                                               select new
                                               {
                                                   //cuentaPuc.cntpuc_id,
                                                   nivel = cuentaPuc.cntpuc_numero.Substring(0, 4),
                                                   //cuentaPuc.cntpuc_numero,
                                                   //cntpuc_descp = (from indicesNombre in indicesNivel2 where indicesNombre.cntpuc_numero == cuentaPuc.cntpuc_numero.Substring(0, 2) select indicesNombre.cntpuc_descp),
                                                   //cntpuc_descp = indicesNivel2.FirstOrDefault(x=>x.cntpuc_numero == cuentaPuc.cntpuc_numero.Substring(0, 2)).cntpuc_descp,
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
                        //foreach (var items2 in item.total)
                        //{
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
                                                       where /*cuentaValor.cuenta || cuentaPuc.cntpuc_numero.Length == 2*/
                                                           cuentaPuc.cntpuc_numero.Substring(0, 4) == cuenta.cuenta
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
                                //foreach (var items2 in item.total)
                                //{
                                listaCentros.Add(new CuentaConsulta
                                {
                                    //cuentaId = item.cntpuc_id,
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
                            //}
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
    }
}