using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

/// <summary>
/// //////
/// </summary>
namespace Homer_MVC.Controllers
{
    public class abrirMesController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: abrirMes
        public ActionResult Index()
        {
            InicioMesModel modelo = new InicioMesModel();
            referencias_inven buscarUltimoMes = context.referencias_inven.OrderByDescending(x => x.ano).ThenByDescending(x => x.mes)
                .FirstOrDefault();
            if (buscarUltimoMes != null)
            {
                if (buscarUltimoMes.mes == 12)
                {
                    modelo.anio = buscarUltimoMes.ano + 1;
                }
                //modelo.mes = 1;
                else
                {
                    modelo.anio = buscarUltimoMes.ano;
                }
                //modelo.mes = buscarUltimoMes.mes + 1;
                //
            }

            return View(modelo);
        }

        [HttpPost]
        public ActionResult Index(InicioMesModel modelo)
        {
            string bodegas = Request["listar_bodegas"];
            ViewBag.bodegas = bodegas;
            ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();

            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(bodegas))
                        {
                            string[] bodegasId = bodegas.Split(',');
                            List<int> listaBodegas = new List<int>();
                            foreach (string item in bodegasId)
                            {
                                listaBodegas.Add(Convert.ToInt32(item));
                            }

                            foreach (int j in listaBodegas)
                            {
                                mesactualbodega bodega = context.mesactualbodega.FirstOrDefault(x => x.idbodega == j);
                                if (bodega == null)
                                {
                                    mesactualbodega mesactualbodega = new mesactualbodega
                                    {
                                        idbodega = j,
                                        mesacual = modelo.mes,
                                        anoactual = modelo.anio,
                                        fec_creacion = DateTime.Now,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                                    };
                                    context.mesactualbodega.Add(mesactualbodega);
                                    context.SaveChanges();
                                }
                                else
                                {
                                    int idb = bodega.id;
                                    mesactualbodega mab = context.mesactualbodega.Find(idb);
                                    mab.mesacual = modelo.mes;
                                    mab.anoactual = modelo.anio;
                                    context.Entry(mab).State = EntityState.Modified;
                                    context.SaveChanges();
                                }

                            }

                            int mesAnterior = modelo.mes;
                            int anioAnterior = modelo.anio;
                            mesAnterior = mesAnterior == 1 ? 12 : mesAnterior - 1;
                            anioAnterior = mesAnterior == 12 ? anioAnterior - 1 : anioAnterior;

                            #region referencias inven

                            List<referencias_inven> buscarReferencia_Inven = context.referencias_inven.Where(x =>
                                    listaBodegas.Contains(x.bodega) && x.ano == anioAnterior && x.mes == mesAnterior)
                                .ToList();
                            foreach (referencias_inven referencia_inven in buscarReferencia_Inven)
                            {
                                referencias_inven inven = new referencias_inven();
                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                    x.ano == modelo.anio && x.mes == modelo.mes &&
                                    x.bodega == referencia_inven.bodega && x.codigo == referencia_inven.codigo);
                                if (existe == null)
                                {
                                    inven.bodega = referencia_inven.bodega;
                                    inven.ano = (short)modelo.anio;
                                    inven.mes = (short)modelo.mes;
                                    inven.can_ini = referencia_inven.can_ini + referencia_inven.can_ent -
                                                    referencia_inven.can_sal;
                                    inven.cos_ini = referencia_inven.cos_ini + referencia_inven.cos_ent -
                                                    referencia_inven.cos_sal;
                                    inven.codigo = referencia_inven.codigo;
                                    inven.modulo = referencia_inven.modulo;
                                    inven.costo_prom = referencia_inven.costo_prom;
                                    context.referencias_inven.Add(inven);
                                    context.SaveChanges();
                                }
                                else
                                {
                                    existe.can_ini = referencia_inven.can_ini + referencia_inven.can_ent -
                                                     referencia_inven.can_sal;
                                    existe.cos_ini = referencia_inven.cos_ini + referencia_inven.cos_ent -
                                                     referencia_inven.cos_sal;
                                    existe.costo_prom = referencia_inven.costo_prom;
                                    context.Entry(existe).State = EntityState.Modified;
                                    context.SaveChanges();
                                }
                            }

                            #endregion

                            #region cuentas valores

                            List<cuentas_valores> buscarCuentasValores = context.cuentas_valores
                                .Where(x => x.ano == anioAnterior && x.mes == mesAnterior).ToList();

                            foreach (cuentas_valores cuentaValor in buscarCuentasValores)
                            {
                                cuentas_valores cv = new cuentas_valores();
                                cuentas_valores existe = context.cuentas_valores.FirstOrDefault(x =>
                                    x.ano == modelo.anio && x.mes == modelo.mes && x.centro == cuentaValor.centro &&
                                    x.cuenta == cuentaValor.cuenta && x.nit == cuentaValor.nit);
                                if (existe != null)
                                {
                                    existe.saldo_ini +=
                                        cuentaValor.saldo_ini + cuentaValor.debito - cuentaValor.credito;
                                    existe.saldo_ininiff +=
                                        cuentaValor.saldo_ininiff + cuentaValor.debitoniff - cuentaValor.creditoniff;
                                    context.Entry(existe).State = EntityState.Modified;
                                    context.SaveChanges();
                                }
                                else
                                {
                                    cv.ano = (short)modelo.anio;
                                    cv.mes = (short)modelo.mes;
                                    cv.cuenta = cuentaValor.cuenta;
                                    cv.centro = cuentaValor.centro;
                                    cv.nit = cuentaValor.nit;
                                    cv.saldo_ini += cuentaValor.saldo_ini + cuentaValor.debito - cuentaValor.credito;
                                    cv.saldo_ininiff += cuentaValor.saldo_ininiff + cuentaValor.debitoniff -
                                                        cuentaValor.creditoniff;
                                    context.cuentas_valores.Add(cv);
                                    context.SaveChanges();
                                }

                                /*context.cuentas_valores.Add(new cuentas_valores()
								{
									ano = (short)modelo.anio,
									mes = (short)modelo.mes,
									cuenta = cuentaValor.cuenta,
									centro = cuentaValor.centro,
									nit = cuentaValor.nit,
									saldo_ini = (cuentaValor.saldo_ini) + (cuentaValor.debito) - (cuentaValor.credito),
									saldo_ininiff = (cuentaValor.saldo_ininiff) + (cuentaValor.debitoniff) - (cuentaValor.creditoniff)
								});*/
                            }

                            #endregion

                            if (buscarReferencia_Inven.Count > 0 || buscarCuentasValores.Count > 0)
                            {
                                TempData["mensaje"] = "El inicio de mes se ha realizado exitosamente.";
                                ViewBag.bodegas = "";
                                dbTran.Commit();
                                return RedirectToAction("Index");
                            }

                            TempData["mensaje_error"] =
                                "No se han actualizado los registros porque no se encontraron datos para la(s) bodegas seleccionadas en el mes seleccionado";
                        }
                    }
                    catch (DbEntityValidationException)
                    {
                        dbTran.Rollback();
                        throw;
                    }
                }
            }

            return View(modelo);
        }

        public JsonResult cargarBodegas(int mes)
        {
            int mesConsulta = 0;
            if (mes == 1)
            {
                mesConsulta = 12;
            }
            else
            {
                mesConsulta = mes - 1;
            }

            var data = (from b in context.bodega_concesionario
                        join m in context.mesactualbodega
                            on b.id equals m.idbodega
                        where m.mesacual == mesConsulta
                        select new
                        {
                            b.id,
                            b.bodccs_nombre
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}