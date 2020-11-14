using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class RecalculoContableController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: RecalculoContable
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult buscarCentros()
        {
            var data = (from c in context.centro_costo
                        select new
                        {
                            c.centcst_id,
                            c.centcst_nombre
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarNit()
        {
            var data = (from t in context.icb_terceros
                        select new
                        {
                            id = t.tercero_id,
                            nombre = t.doc_tercero + " - " + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                     t.apellido_tercero + " " + t.segapellido_tercero + " " + t.razon_social
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarCuentas()
        {
            var data = (from p in context.cuenta_puc
                        where p.esafectable
                        orderby p.clase, p.grupo, p.cuenta, p.subcuenta
                        select new
                        {
                            id = p.cntpuc_numero,
                            descripcion = p.cntpuc_numero + " - " + p.cntpuc_descp
                        }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RecalculoContable(int anio, int mes, string desde, string hasta, int? nit, int? centro)
        {
            int cerrado = (from c in context.meses_cierre
                           where c.ano == anio && c.mes == mes
                           select c).Count();

            if (cerrado == 0)
            {
                #region validacion de cuentas

                if (string.IsNullOrWhiteSpace(desde))
                {
                    desde = (from c in context.mov_contable
                             join p in context.cuenta_puc
                                 on c.cuenta equals p.cntpuc_id
                             where p.esafectable
                             orderby c.cuenta
                             select p.cntpuc_numero).FirstOrDefault();
                }

                if (string.IsNullOrWhiteSpace(hasta))
                {
                    hasta = (from c in context.mov_contable
                             join p in context.cuenta_puc
                                 on c.cuenta equals p.cntpuc_id
                             where p.esafectable
                             orderby c.cuenta descending
                             select p.cntpuc_numero).FirstOrDefault();
                }

                int ini = (from c in context.cuenta_puc
                           where c.cntpuc_numero == desde
                           orderby c.cuenta
                           select c.cntpuc_id).FirstOrDefault();

                int fin = (from c in context.cuenta_puc
                           where c.cntpuc_numero == hasta
                           orderby c.cuenta
                           select c.cntpuc_id).FirstOrDefault();


                List<int> rango = (from p in context.cuenta_puc
                                   where p.cntpuc_numero.Substring(0, 12).CompareTo(desde.Substring(0, 12)) >= 0 &&
                                         p.cntpuc_numero.Substring(0, 12).CompareTo(hasta.Substring(0, 12)) <= 0 && p.esafectable
                                   orderby p.clase, p.grupo, p.cuenta, p.subcuenta
                                   select p.cntpuc_id).ToList();

                #endregion

                #region Declaro los predicate a utilizar

                System.Linq.Expressions.Expression<System.Func<vw_movimientoContable, bool>> anioP = PredicateBuilder.True<vw_movimientoContable>();

                System.Linq.Expressions.Expression<System.Func<vw_movimientoContable, bool>> nitP = PredicateBuilder.False<vw_movimientoContable>();
                System.Linq.Expressions.Expression<System.Func<vw_movimientoContable, bool>> centroP = PredicateBuilder.False<vw_movimientoContable>();
                System.Linq.Expressions.Expression<System.Func<vw_movimientoContable, bool>> cuentaP = PredicateBuilder.False<vw_movimientoContable>();

                anioP = anioP.And(x => x.fec.Year == anio);
                anioP = anioP.And(x => x.fec.Month == mes);

                #region Predicate cuentas

                anioP = anioP.And(p => 1 == 1 && rango.Contains(p.cuenta));

                #endregion

                #region Predicate nit

                if (nit != null)
                {
                    nitP = nitP.Or(p => p.nit == nit);
                    anioP = anioP.And(nitP);
                }

                #endregion

                #region Predicate centro

                if (centro != null)
                {
                    centroP = centroP.Or(p => p.centro == centro);
                    anioP = anioP.And(centroP);
                }

                #endregion

                #endregion

                List<vw_movimientoContable> consulta = context.vw_movimientoContable.Where(anioP).ToList();
                if (consulta.Count == 0)
                {
                    return Json(new { vacio = true }, JsonRequestBehavior.AllowGet);
                }

                bool borrado = false;
                bool consolidado = false;
                List<listaRecalculo> listaReferencias = new List<listaRecalculo>();
                foreach (vw_movimientoContable item in consulta)
                {
                    using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                    {
                        try
                        {
                            //var registro = listaReferencias.FirstOrDefault(x => x.anio == anio && x.mes == mes && x.centro == item.centro && x.nit == item.nit && x.cuenta == item.cuenta);

                            //if (registro == null)
                            //{

                            #region Si tiene filtros aplicados

                            if (centro != null)
                            {
                                if (nit != null)
                                {
                                    #region Validaciones (1 → por el mes filtrado)

                                    #region Si el mes es enero (debo restar tambien el año)

                                    if (mes == 1)
                                    {
                                        int anioAnterior = anio - 1;
                                        int mesAnterior = 12;

                                        #region Borrar mes anterior

                                        if (borrado == false)
                                        {
                                            List<cuentas_valores> borrarMes = context.cuentas_valores.Where(x =>
                                                x.ano == anio && x.mes == mes && x.nit == item.nit &&
                                                x.centro == item.centro).ToList();
                                            for (int j = 0; j < borrarMes.Count(); j++)
                                            {
                                                context.Entry((object)borrarMes[j]).State = EntityState.Deleted;
                                            }

                                            borrado = true;
                                            context.SaveChanges();
                                        }

                                        #endregion

                                        #region Consolido lo del mes anterior para recalcular el mes filtrado

                                        if (consolidado == false)
                                        {
                                            List<cuentas_valores> infoMovimientos = context.cuentas_valores.Where(x =>
                                                x.ano == anioAnterior && x.mes == mesAnterior && x.nit == nit &&
                                                x.centro == centro &&
                                                (x.saldo_ini + x.debito - x.credito != 0 ||
                                                 x.saldo_ininiff + x.debitoniff - x.creditoniff != 0)).ToList();
                                            foreach (cuentas_valores a in infoMovimientos)
                                            {
                                                cuentas_valores cv = new cuentas_valores();
                                                cuentas_valores existe = context.cuentas_valores.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.centro == a.centro &&
                                                    x.cuenta == a.cuenta && x.nit == a.nit);
                                                if (existe != null)
                                                {
                                                    existe.saldo_ini += a.saldo_ini + a.debito - a.credito;
                                                    existe.saldo_ininiff +=
                                                        a.saldo_ininiff + a.debitoniff - a.creditoniff;
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    cv.ano = (short)anio;
                                                    cv.mes = (short)mes;
                                                    cv.cuenta = a.cuenta;
                                                    cv.centro = a.centro;
                                                    cv.nit = a.nit;
                                                    cv.saldo_ini += a.saldo_ini + a.debito - a.credito;
                                                    cv.saldo_ininiff += a.saldo_ininiff + a.debitoniff - a.creditoniff;
                                                    context.cuentas_valores.Add(cv);
                                                    context.SaveChanges();
                                                }
                                            }

                                            consolidado = true;
                                        }

                                        #endregion

                                        #region Cuentas valores

                                        cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                            x.centro == item.centro && x.cuenta == item.cuenta && x.nit == item.nit &&
                                            x.ano == anio && x.mes == mes);
                                        if (buscar_cuentas_valores != null)
                                        {
                                            buscar_cuentas_valores.debito += item.debito;
                                            buscar_cuentas_valores.credito += item.credito;
                                            buscar_cuentas_valores.debitoniff += item.debitoniif;
                                            buscar_cuentas_valores.creditoniff += item.creditoniif;
                                            context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            cuentas_valores crearCuentaValor = new cuentas_valores
                                            {
                                                ano = anio,
                                                mes = mes,
                                                cuenta = item.cuenta,
                                                centro = item.centro,
                                                nit = item.nit,
                                                debito = item.debito,
                                                credito = item.credito,
                                                debitoniff = item.debitoniif,
                                                creditoniff = item.creditoniif
                                            };
                                            context.cuentas_valores.Add(crearCuentaValor);
                                            context.SaveChanges();
                                        }

                                        #endregion

                                        dbTran.Commit();
                                    }

                                    #endregion

                                    #region Si el mes es diferente a enero, solo resto 1 mes al mes filtrado

                                    else
                                    {
                                        int mesAnterior = mes - 1;

                                        #region Borrar mes anterior

                                        if (borrado == false)
                                        {
                                            List<cuentas_valores> borrarMes = context.cuentas_valores.Where(x =>
                                                x.ano == anio && x.mes == mes && x.nit == item.nit &&
                                                x.centro == item.centro).ToList();
                                            for (int j = 0; j < borrarMes.Count(); j++)
                                            {
                                                context.Entry((object)borrarMes[j]).State = EntityState.Deleted;
                                            }

                                            borrado = true;
                                            context.SaveChanges();
                                        }

                                        #endregion

                                        #region Consolido lo del mes anterior para recalcular el mes filtrado

                                        if (consolidado == false)
                                        {
                                            List<cuentas_valores> infoMovimientos = context.cuentas_valores.Where(x =>
                                                x.ano == anio && x.mes == mesAnterior && x.nit == nit &&
                                                x.centro == centro &&
                                                (x.saldo_ini + x.debito - x.credito != 0 ||
                                                 x.saldo_ininiff + x.debitoniff - x.creditoniff != 0)).ToList();
                                            foreach (cuentas_valores a in infoMovimientos)
                                            {
                                                cuentas_valores cv = new cuentas_valores();
                                                cuentas_valores existe = context.cuentas_valores.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.centro == a.centro &&
                                                    x.cuenta == a.cuenta && x.nit == a.nit);
                                                if (existe != null)
                                                {
                                                    existe.saldo_ini += a.saldo_ini + a.debito - a.credito;
                                                    existe.saldo_ininiff +=
                                                        a.saldo_ininiff + a.debitoniff - a.creditoniff;
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    cv.ano = (short)anio;
                                                    cv.mes = (short)mes;
                                                    cv.cuenta = a.cuenta;
                                                    cv.centro = a.centro;
                                                    cv.nit = a.nit;
                                                    cv.saldo_ini += a.saldo_ini + a.debito - a.credito;
                                                    cv.saldo_ininiff += a.saldo_ininiff + a.debitoniff - a.creditoniff;
                                                    context.cuentas_valores.Add(cv);
                                                    context.SaveChanges();
                                                }
                                            }

                                            consolidado = true;
                                        }

                                        #endregion

                                        #region Cuentas valores

                                        cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                            x.centro == item.centro && x.cuenta == item.cuenta && x.nit == item.nit &&
                                            x.ano == anio && x.mes == mes);
                                        if (buscar_cuentas_valores != null)
                                        {
                                            buscar_cuentas_valores.debito += item.debito;
                                            buscar_cuentas_valores.credito += item.credito;
                                            buscar_cuentas_valores.debitoniff += item.debitoniif;
                                            buscar_cuentas_valores.creditoniff += item.creditoniif;
                                            context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            cuentas_valores crearCuentaValor = new cuentas_valores
                                            {
                                                ano = anio,
                                                mes = mes,
                                                cuenta = item.cuenta,
                                                centro = item.centro,
                                                nit = item.nit,
                                                debito = item.debito,
                                                credito = item.credito,
                                                debitoniff = item.debitoniif,
                                                creditoniff = item.creditoniif
                                            };
                                            context.cuentas_valores.Add(crearCuentaValor);
                                            context.SaveChanges();
                                        }

                                        #endregion

                                        dbTran.Commit();
                                    }

                                    #endregion

                                    #endregion
                                }
                                else
                                {
                                    #region Validaciones (1 → por el mes filtrado)

                                    #region Si el mes es enero (debo restar tambien el año)

                                    if (mes == 1)
                                    {
                                        int anioAnterior = anio - 1;
                                        int mesAnterior = 12;

                                        #region Borrar mes anterior

                                        if (borrado == false)
                                        {
                                            List<cuentas_valores> borrarMes = context.cuentas_valores.Where(x =>
                                                x.ano == anio && x.mes == mes && x.centro == item.centro).ToList();
                                            for (int j = 0; j < borrarMes.Count(); j++)
                                            {
                                                context.Entry((object)borrarMes[j]).State = EntityState.Deleted;
                                            }

                                            borrado = true;
                                            context.SaveChanges();
                                        }

                                        #endregion

                                        #region Consolido lo del mes anterior para recalcular el mes filtrado

                                        if (consolidado == false)
                                        {
                                            List<cuentas_valores> infoMovimientos = context.cuentas_valores.Where(x =>
                                                x.ano == anioAnterior && x.mes == mesAnterior && x.centro == centro &&
                                                (x.saldo_ini + x.debito - x.credito != 0 ||
                                                 x.saldo_ininiff + x.debitoniff - x.creditoniff != 0)).ToList();
                                            foreach (cuentas_valores a in infoMovimientos)
                                            {
                                                cuentas_valores cv = new cuentas_valores();
                                                cuentas_valores existe = context.cuentas_valores.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.centro == a.centro &&
                                                    x.cuenta == a.cuenta && x.nit == a.nit);
                                                if (existe != null)
                                                {
                                                    existe.saldo_ini += a.saldo_ini + a.debito - a.credito;
                                                    existe.saldo_ininiff +=
                                                        a.saldo_ininiff + a.debitoniff - a.creditoniff;
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    cv.ano = (short)anio;
                                                    cv.mes = (short)mes;
                                                    cv.cuenta = a.cuenta;
                                                    cv.centro = a.centro;
                                                    cv.nit = a.nit;
                                                    cv.saldo_ini += a.saldo_ini + a.debito - a.credito;
                                                    cv.saldo_ininiff += a.saldo_ininiff + a.debitoniff - a.creditoniff;
                                                    context.cuentas_valores.Add(cv);
                                                    context.SaveChanges();
                                                }
                                            }

                                            consolidado = true;
                                        }

                                        #endregion

                                        #region Cuentas valores

                                        cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                            x.centro == item.centro && x.cuenta == item.cuenta && x.nit == item.nit &&
                                            x.ano == anio && x.mes == mes);
                                        if (buscar_cuentas_valores != null)
                                        {
                                            buscar_cuentas_valores.debito += item.debito;
                                            buscar_cuentas_valores.credito += item.credito;
                                            buscar_cuentas_valores.debitoniff += item.debitoniif;
                                            buscar_cuentas_valores.creditoniff += item.creditoniif;
                                            context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            cuentas_valores crearCuentaValor = new cuentas_valores
                                            {
                                                ano = anio,
                                                mes = mes,
                                                cuenta = item.cuenta,
                                                centro = item.centro,
                                                nit = item.nit,
                                                debito = item.debito,
                                                credito = item.credito,
                                                debitoniff = item.debitoniif,
                                                creditoniff = item.creditoniif
                                            };
                                            context.cuentas_valores.Add(crearCuentaValor);
                                            context.SaveChanges();
                                        }

                                        #endregion

                                        dbTran.Commit();
                                    }

                                    #endregion

                                    #region Si el mes es diferente a enero, solo resto 1 mes al mes filtrado

                                    else
                                    {
                                        int mesAnterior = mes - 1;

                                        #region Borrar mes anterior

                                        if (borrado == false)
                                        {
                                            List<cuentas_valores> borrarMes = context.cuentas_valores.Where(x =>
                                                x.ano == anio && x.mes == mes && x.centro == item.centro).ToList();
                                            for (int j = 0; j < borrarMes.Count(); j++)
                                            {
                                                context.Entry((object)borrarMes[j]).State = EntityState.Deleted;
                                            }

                                            borrado = true;
                                            context.SaveChanges();
                                        }

                                        #endregion

                                        #region Consolido lo del mes anterior para recalcular el mes filtrado

                                        if (consolidado == false)
                                        {
                                            List<cuentas_valores> infoMovimientos = context.cuentas_valores.Where(x =>
                                                x.ano == anio && x.mes == mesAnterior && x.centro == centro &&
                                                (x.saldo_ini + x.debito - x.credito != 0 ||
                                                 x.saldo_ininiff + x.debitoniff - x.creditoniff != 0)).ToList();
                                            foreach (cuentas_valores a in infoMovimientos)
                                            {
                                                cuentas_valores cv = new cuentas_valores();
                                                cuentas_valores existe = context.cuentas_valores.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.centro == a.centro &&
                                                    x.cuenta == a.cuenta && x.nit == a.nit);
                                                if (existe != null)
                                                {
                                                    existe.saldo_ini += a.saldo_ini + a.debito - a.credito;
                                                    existe.saldo_ininiff +=
                                                        a.saldo_ininiff + a.debitoniff - a.creditoniff;
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    cv.ano = (short)anio;
                                                    cv.mes = (short)mes;
                                                    cv.cuenta = a.cuenta;
                                                    cv.centro = a.centro;
                                                    cv.nit = a.nit;
                                                    cv.saldo_ini += a.saldo_ini + a.debito - a.credito;
                                                    cv.saldo_ininiff += a.saldo_ininiff + a.debitoniff - a.creditoniff;
                                                    context.cuentas_valores.Add(cv);
                                                    context.SaveChanges();
                                                }
                                            }

                                            consolidado = true;
                                        }

                                        #endregion

                                        #region Cuentas valores

                                        cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                            x.centro == item.centro && x.cuenta == item.cuenta && x.nit == item.nit &&
                                            x.ano == anio && x.mes == mes);
                                        if (buscar_cuentas_valores != null)
                                        {
                                            buscar_cuentas_valores.debito += item.debito;
                                            buscar_cuentas_valores.credito += item.credito;
                                            buscar_cuentas_valores.debitoniff += item.debitoniif;
                                            buscar_cuentas_valores.creditoniff += item.creditoniif;
                                            context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            cuentas_valores crearCuentaValor = new cuentas_valores
                                            {
                                                ano = anio,
                                                mes = mes,
                                                cuenta = item.cuenta,
                                                centro = item.centro,
                                                nit = item.nit,
                                                debito = item.debito,
                                                credito = item.credito,
                                                debitoniff = item.debitoniif,
                                                creditoniff = item.creditoniif
                                            };
                                            context.cuentas_valores.Add(crearCuentaValor);
                                            context.SaveChanges();
                                        }

                                        #endregion

                                        dbTran.Commit();
                                    }

                                    #endregion

                                    #endregion
                                }
                            }
                            else
                            {
                                if (nit != null)
                                {
                                    #region Validaciones (1 → por el mes filtrado)

                                    #region Si el mes es enero (debo restar tambien el año)

                                    if (mes == 1)
                                    {
                                        int anioAnterior = anio - 1;
                                        int mesAnterior = 12;

                                        #region Borrar mes anterior

                                        if (borrado == false)
                                        {
                                            List<cuentas_valores> borrarMes = context.cuentas_valores.Where(x =>
                                                x.ano == anio && x.mes == mes && x.nit == item.nit).ToList();
                                            for (int j = 0; j < borrarMes.Count(); j++)
                                            {
                                                context.Entry((object)borrarMes[j]).State = EntityState.Deleted;
                                            }

                                            borrado = true;
                                            context.SaveChanges();
                                        }

                                        #endregion

                                        #region Consolido lo del mes anterior para recalcular el mes filtrado

                                        if (consolidado == false)
                                        {
                                            List<cuentas_valores> infoMovimientos = context.cuentas_valores.Where(x =>
                                                x.ano == anioAnterior && x.mes == mesAnterior && x.nit == nit &&
                                                (x.saldo_ini + x.debito - x.credito != 0 ||
                                                 x.saldo_ininiff + x.debitoniff - x.creditoniff != 0)).ToList();
                                            foreach (cuentas_valores a in infoMovimientos)
                                            {
                                                cuentas_valores cv = new cuentas_valores();
                                                cuentas_valores existe = context.cuentas_valores.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.centro == a.centro &&
                                                    x.cuenta == a.cuenta && x.nit == a.nit);
                                                if (existe != null)
                                                {
                                                    existe.saldo_ini += a.saldo_ini + a.debito - a.credito;
                                                    existe.saldo_ininiff +=
                                                        a.saldo_ininiff + a.debitoniff - a.creditoniff;
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    cv.ano = (short)anio;
                                                    cv.mes = (short)mes;
                                                    cv.cuenta = a.cuenta;
                                                    cv.centro = a.centro;
                                                    cv.nit = a.nit;
                                                    cv.saldo_ini += a.saldo_ini + a.debito - a.credito;
                                                    cv.saldo_ininiff += a.saldo_ininiff + a.debitoniff - a.creditoniff;
                                                    context.cuentas_valores.Add(cv);
                                                    context.SaveChanges();
                                                }
                                            }

                                            consolidado = true;
                                        }

                                        #endregion

                                        #region Cuentas valores

                                        cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                            x.centro == item.centro && x.cuenta == item.cuenta && x.nit == item.nit &&
                                            x.ano == anio && x.mes == mes);
                                        if (buscar_cuentas_valores != null)
                                        {
                                            buscar_cuentas_valores.debito += item.debito;
                                            buscar_cuentas_valores.credito += item.credito;
                                            buscar_cuentas_valores.debitoniff += item.debitoniif;
                                            buscar_cuentas_valores.creditoniff += item.creditoniif;
                                            context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            cuentas_valores crearCuentaValor = new cuentas_valores
                                            {
                                                ano = anio,
                                                mes = mes,
                                                cuenta = item.cuenta,
                                                centro = item.centro,
                                                nit = item.nit,
                                                debito = item.debito,
                                                credito = item.credito,
                                                debitoniff = item.debitoniif,
                                                creditoniff = item.creditoniif
                                            };
                                            context.cuentas_valores.Add(crearCuentaValor);
                                            context.SaveChanges();
                                        }

                                        #endregion

                                        dbTran.Commit();
                                    }

                                    #endregion

                                    #region Si el mes es diferente a enero, solo resto 1 mes al mes filtrado

                                    else
                                    {
                                        int mesAnterior = mes - 1;

                                        #region Borrar mes anterior

                                        if (borrado == false)
                                        {
                                            List<cuentas_valores> borrarMes = context.cuentas_valores.Where(x =>
                                                x.ano == anio && x.mes == mes && x.nit == item.nit).ToList();
                                            for (int j = 0; j < borrarMes.Count(); j++)
                                            {
                                                context.Entry((object)borrarMes[j]).State = EntityState.Deleted;
                                            }

                                            borrado = true;
                                            context.SaveChanges();
                                        }

                                        #endregion

                                        #region Consolido lo del mes anterior para recalcular el mes filtrado

                                        if (consolidado == false)
                                        {
                                            List<cuentas_valores> infoMovimientos = context.cuentas_valores.Where(x =>
                                                x.ano == anio && x.mes == mesAnterior && x.nit == nit &&
                                                (x.saldo_ini + x.debito - x.credito != 0 ||
                                                 x.saldo_ininiff + x.debitoniff - x.creditoniff != 0)).ToList();
                                            foreach (cuentas_valores a in infoMovimientos)
                                            {
                                                cuentas_valores cv = new cuentas_valores();
                                                cuentas_valores existe = context.cuentas_valores.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.centro == a.centro &&
                                                    x.cuenta == a.cuenta && x.nit == a.nit);
                                                if (existe != null)
                                                {
                                                    existe.saldo_ini += a.saldo_ini + a.debito - a.credito;
                                                    existe.saldo_ininiff +=
                                                        a.saldo_ininiff + a.debitoniff - a.creditoniff;
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    cv.ano = (short)anio;
                                                    cv.mes = (short)mes;
                                                    cv.cuenta = a.cuenta;
                                                    cv.centro = a.centro;
                                                    cv.nit = a.nit;
                                                    cv.saldo_ini += a.saldo_ini + a.debito - a.credito;
                                                    cv.saldo_ininiff += a.saldo_ininiff + a.debitoniff - a.creditoniff;
                                                    context.cuentas_valores.Add(cv);
                                                    context.SaveChanges();
                                                }
                                            }

                                            consolidado = true;
                                        }

                                        #endregion

                                        #region Cuentas valores

                                        cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                            x.centro == item.centro && x.cuenta == item.cuenta && x.nit == item.nit &&
                                            x.ano == anio && x.mes == mes);
                                        if (buscar_cuentas_valores != null)
                                        {
                                            buscar_cuentas_valores.debito += item.debito;
                                            buscar_cuentas_valores.credito += item.credito;
                                            buscar_cuentas_valores.debitoniff += item.debitoniif;
                                            buscar_cuentas_valores.creditoniff += item.creditoniif;
                                            context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            cuentas_valores crearCuentaValor = new cuentas_valores
                                            {
                                                ano = anio,
                                                mes = mes,
                                                cuenta = item.cuenta,
                                                centro = item.centro,
                                                nit = item.nit,
                                                debito = item.debito,
                                                credito = item.credito,
                                                debitoniff = item.debitoniif,
                                                creditoniff = item.creditoniif
                                            };
                                            context.cuentas_valores.Add(crearCuentaValor);
                                            context.SaveChanges();
                                        }

                                        #endregion

                                        dbTran.Commit();
                                    }

                                    #endregion

                                    #endregion
                                }
                            }

                            #endregion

                            #region Si no tiene filtros

                            if (nit == null && centro == null)
                            {
                                #region Validaciones (1 → por el mes filtrado)

                                #region Si el mes es enero (debo restar tambien el año)

                                if (mes == 1)
                                {
                                    int anioAnterior = anio - 1;
                                    int mesAnterior = 12;

                                    #region Borrar mes anterior

                                    if (borrado == false)
                                    {
                                        List<cuentas_valores> borrarMes = context.cuentas_valores
                                            .Where(x => x.ano == anio && x.mes == mes).ToList();
                                        for (int j = 0; j < borrarMes.Count(); j++)
                                        {
                                            context.Entry(borrarMes[j]).State = EntityState.Deleted;
                                        }

                                        borrado = true;
                                        context.SaveChanges();
                                    }

                                    #endregion

                                    #region Consolido lo del mes anterior para recalcular el mes filtrado

                                    if (consolidado == false)
                                    {
                                        List<cuentas_valores> infoMovimientos = context.cuentas_valores.Where(x =>
                                            x.ano == anioAnterior && x.mes == mesAnterior &&
                                            (x.saldo_ini + x.debito - x.credito != 0 ||
                                             x.saldo_ininiff + x.debitoniff - x.creditoniff != 0)).ToList();
                                        foreach (cuentas_valores a in infoMovimientos)
                                        {
                                            cuentas_valores cv = new cuentas_valores();
                                            cuentas_valores existe = context.cuentas_valores.FirstOrDefault(x =>
                                                x.ano == anio && x.mes == mes && x.centro == a.centro &&
                                                x.cuenta == a.cuenta && x.nit == a.nit);
                                            if (existe != null)
                                            {
                                                existe.saldo_ini += a.saldo_ini + a.debito - a.credito;
                                                existe.saldo_ininiff += a.saldo_ininiff + a.debitoniff - a.creditoniff;
                                                context.Entry(existe).State = EntityState.Modified;
                                                context.SaveChanges();
                                            }
                                            else
                                            {
                                                cv.ano = (short)anio;
                                                cv.mes = (short)mes;
                                                cv.cuenta = a.cuenta;
                                                cv.centro = a.centro;
                                                cv.nit = a.nit;
                                                cv.saldo_ini += a.saldo_ini + a.debito - a.credito;
                                                cv.saldo_ininiff += a.saldo_ininiff + a.debitoniff - a.creditoniff;
                                                context.cuentas_valores.Add(cv);
                                                context.SaveChanges();
                                            }
                                        }

                                        consolidado = true;
                                    }

                                    #endregion

                                    #region Cuentas valores

                                    cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                        x.centro == item.centro && x.cuenta == item.cuenta && x.nit == item.nit &&
                                        x.ano == anio && x.mes == mes);
                                    if (buscar_cuentas_valores != null)
                                    {
                                        buscar_cuentas_valores.debito += item.debito;
                                        buscar_cuentas_valores.credito += item.credito;
                                        buscar_cuentas_valores.debitoniff += item.debitoniif;
                                        buscar_cuentas_valores.creditoniff += item.creditoniif;
                                        context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                        context.SaveChanges();
                                    }
                                    else
                                    {
                                        cuentas_valores crearCuentaValor = new cuentas_valores
                                        {
                                            ano = anio,
                                            mes = mes,
                                            cuenta = item.cuenta,
                                            centro = item.centro,
                                            nit = item.nit,
                                            debito = item.debito,
                                            credito = item.credito,
                                            debitoniff = item.debitoniif,
                                            creditoniff = item.creditoniif
                                        };
                                        context.cuentas_valores.Add(crearCuentaValor);
                                        context.SaveChanges();
                                    }

                                    #endregion

                                    dbTran.Commit();
                                }

                                #endregion

                                #region Si el mes es diferente a enero, solo resto 1 mes al mes filtrado

                                else
                                {
                                    int mesAnterior = mes - 1;

                                    #region Borrar mes anterior

                                    if (borrado == false)
                                    {
                                        context.cuentas_valores.RemoveRange(
                                            context.cuentas_valores.Where(x => x.ano == anio && x.mes == mes));
                                        context.SaveChanges();
                                        borrado = true;
                                    }

                                    #endregion

                                    #region Consolido lo del mes anterior para recalcular el mes filtrado

                                    if (consolidado == false)
                                    {
                                        List<cuentas_valores> infoMovimientos = context.cuentas_valores.Where(x =>
                                            x.ano == anio && x.mes == mesAnterior &&
                                            (x.saldo_ini + x.debito - x.credito != 0 ||
                                             x.saldo_ininiff + x.debitoniff - x.creditoniff != 0)).ToList();
                                        foreach (cuentas_valores a in infoMovimientos)
                                        {
                                            cuentas_valores cv = new cuentas_valores();
                                            cuentas_valores existe = context.cuentas_valores.FirstOrDefault(x =>
                                                x.ano == anio && x.mes == mes && x.centro == a.centro &&
                                                x.cuenta == a.cuenta && x.nit == a.nit);
                                            if (existe != null)
                                            {
                                                existe.saldo_ini += a.saldo_ini + a.debito - a.credito;
                                                existe.saldo_ininiff += a.saldo_ininiff + a.debitoniff - a.creditoniff;
                                                context.Entry(existe).State = EntityState.Modified;
                                                context.SaveChanges();
                                            }
                                            else
                                            {
                                                cv.ano = (short)anio;
                                                cv.mes = (short)mes;
                                                cv.cuenta = a.cuenta;
                                                cv.centro = a.centro;
                                                cv.nit = a.nit;
                                                cv.saldo_ini += a.saldo_ini + a.debito - a.credito;
                                                cv.saldo_ininiff += a.saldo_ininiff + a.debitoniff - a.creditoniff;
                                                context.cuentas_valores.Add(cv);
                                                context.SaveChanges();
                                            }
                                        }

                                        consolidado = true;
                                    }

                                    #endregion

                                    #region Cuentas valores

                                    cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                        x.centro == item.centro && x.cuenta == item.cuenta && x.nit == item.nit &&
                                        x.ano == anio && x.mes == mes);
                                    if (buscar_cuentas_valores != null)
                                    {
                                        buscar_cuentas_valores.debito += item.debito;
                                        buscar_cuentas_valores.credito += item.credito;
                                        buscar_cuentas_valores.debitoniff += item.debitoniif;
                                        buscar_cuentas_valores.creditoniff += item.creditoniif;

                                        context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                        context.SaveChanges();
                                    }
                                    else
                                    {
                                        cuentas_valores crearCuentaValor = new cuentas_valores
                                        {
                                            ano = anio,
                                            mes = mes,
                                            cuenta = item.cuenta,
                                            centro = item.centro,
                                            nit = item.nit,
                                            debito = item.debito,
                                            credito = item.credito,
                                            debitoniff = item.debitoniif,
                                            creditoniff = item.creditoniif
                                        };
                                        context.cuentas_valores.Add(crearCuentaValor);
                                        context.SaveChanges();
                                    }

                                    #endregion

                                    dbTran.Commit();
                                }

                                #endregion

                                #endregion
                            }

                            #endregion

                            //listaReferencias.Add(new listaRecalculo()
                            //{
                            //    anio = Convert.ToInt32(item.fec.Year),
                            //    mes = Convert.ToInt32(item.fec.Month),
                            //    centro = item.centro,
                            //    cuenta = item.cuenta,
                            //    nit = item.nit
                            //});
                            //}
                        }
                        catch (DbEntityValidationException)
                        {
                            dbTran.Rollback();
                            throw;
                        }
                    }
                }

                var data = new
                {
                    consulta
                };

                return Json(new { cerrado = false, data }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { cerrado = true }, JsonRequestBehavior.AllowGet);
        }
    }
}