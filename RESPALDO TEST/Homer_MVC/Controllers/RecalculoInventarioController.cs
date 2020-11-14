using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class RecalculoInventarioController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: RecalculoInventario
        public ActionResult Index()
        {
            ViewBag.codigo = context.icb_referencia.OrderBy(x => x.ref_descripcion).ToList();
            return View();
        }

        public JsonResult BuscarDocumentos()
        {
            var listarDocumentos = (from d in context.tp_doc_registros
                                    where d.sw == 1 || d.sw == 2 || d.sw == 3 || d.sw == 4 || d.sw == 8 || d.sw == 9 || d.sw == 10 ||
                                          d.sw == 11 || d.sw == 12
                                    select new
                                    {
                                        d.tpdoc_id,
                                        d.tpdoc_nombre
                                    }).ToList();

            return Json(listarDocumentos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RecalculoInventario(int anio, int mes, int?[] bodega,
            string codigo /*, int?[] documento, int? numero*/)
        {
            int cerrado = (from c in context.meses_cierre
                           where c.ano == anio && c.mes == mes
                           select c).Count();

            if (cerrado == 0)
            {
                System.Linq.Expressions.Expression<Func<vw_recalculoInventario, bool>> anioP = PredicateBuilder.True<vw_recalculoInventario>();

                System.Linq.Expressions.Expression<Func<vw_recalculoInventario, bool>> bodegaP = PredicateBuilder.False<vw_recalculoInventario>();
                System.Linq.Expressions.Expression<Func<vw_recalculoInventario, bool>> documentoP = PredicateBuilder.False<vw_recalculoInventario>();
                System.Linq.Expressions.Expression<Func<vw_recalculoInventario, bool>> numeroP = PredicateBuilder.False<vw_recalculoInventario>();
                System.Linq.Expressions.Expression<Func<vw_recalculoInventario, bool>> codigoP = PredicateBuilder.False<vw_recalculoInventario>();

                anioP = anioP.And(x => x.anio == anio);
                anioP = anioP.And(x => x.mes == mes);

                #region Predicate bodega

                if (bodega.Count() > 0 && bodega[0] != null)
                {
                    foreach (int? item in bodega)
                    {
                        if (item != null)
                        {
                            bodegaP = bodegaP.Or(d => d.bodega == item);
                        }
                    }

                    anioP = anioP.And(bodegaP);
                }

                #endregion

                #region Predicate codigo

                if (!string.IsNullOrEmpty(codigo))
                {
                    codigoP = codigoP.Or(d => d.codigo == codigo);
                    anioP = anioP.And(codigoP);
                }

                #endregion

                #region Predicate documento

                //if (documento.Count() > 0 && documento[0] != null)
                //{
                //    foreach (var item in documento)
                //    {
                //        if (item != null)
                //        {
                //            documentoP = documentoP.Or(d => d.tipo == item);
                //        }
                //    }
                //    anioP = anioP.And(documentoP);
                //}

                #endregion

                #region Predicate numero

                //if (numero != null)
                //{
                //    numeroP = numeroP.Or(d => d.numero == numero);
                //    anioP = anioP.And(numeroP);
                //}

                #endregion

                List<vw_recalculoInventario> consulta = context.vw_recalculoInventario.Where(anioP).ToList();
                bool borrado = false;
                bool consolidado = false;
                List<listaRecalculo> listaReferencias = new List<listaRecalculo>();
                foreach (vw_recalculoInventario item in consulta)
                {
                    using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                    {
                        try
                        {
                            listaRecalculo registro = listaReferencias.FirstOrDefault(x =>
                                x.codigoReferencia == item.codigo && x.anio == anio && x.mes == mes &&
                                x.bodega == item.bodega && x.sw == item.sw);

                            if (registro == null)
                            {
                                #region Si tiene filtros aplicados

                                if (bodega.Count() > 0 && bodega[0] != null)
                                {
                                    #region Si tiene bodega y codigo

                                    if (codigo != "")
                                    {
                                        #region Validaciones (1 → por el mes filtrado y  2 → por el sw que tenga la referencia)

                                        #region Si el mes es enero (debo restar tambien el año)

                                        if (mes == 1)
                                        {
                                            int anioAnterior = anio - 1;
                                            int mesAnterior = 12;

                                            #region Borrar mes filtrado

                                            if (borrado == false)
                                            {
                                                List<referencias_inven> borrarMes = context.referencias_inven.Where(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo).ToList();
                                                for (int j = 0; j < borrarMes.Count(); j++)
                                                {
                                                    context.Entry((object)borrarMes[j]).State = EntityState.Deleted;
                                                    borrado = true;
                                                }

                                                context.SaveChanges();
                                            }

                                            #endregion

                                            #region Consolido lo del mes anterior para recalcular el mes filtrado

                                            if (consolidado == false)
                                            {
                                                List<referencias_inven> infoReferencias = context.referencias_inven.Where(x =>
                                                    x.ano == anio && x.mes == mesAnterior &&
                                                    (x.can_ini + x.can_ent - x.can_sal != 0 ||
                                                     x.cos_ini + x.cos_ent - x.cos_sal != 0)).ToList();
                                                foreach (referencias_inven a in infoReferencias)
                                                {
                                                    referencias_inven refin = new referencias_inven();
                                                    referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                        x.ano == anio && x.mes == mes && x.bodega == a.bodega &&
                                                        x.codigo == a.codigo);
                                                    if (existe != null)
                                                    {
                                                        existe.can_ini += a.can_ini + a.can_ent - a.can_sal;
                                                        existe.cos_ini += a.cos_ini + a.cos_ent - a.cos_sal;
                                                        context.Entry(existe).State = EntityState.Modified;
                                                        context.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        refin.bodega = a.bodega;
                                                        refin.codigo = a.codigo;
                                                        refin.ano = (short)anio;
                                                        refin.mes = (short)mes;
                                                        refin.can_ini = a.can_ini + a.can_ent - a.can_sal;
                                                        refin.cos_ini = a.cos_ini + a.cos_ent - a.cos_sal;
                                                        refin.modulo = a.modulo;
                                                        context.referencias_inven.Add(refin);
                                                        context.SaveChanges();
                                                    }
                                                }

                                                consolidado = true;
                                            }

                                            #endregion

                                            #region Si SW = 1

                                            if (item.sw == 1)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);

                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConCanCom = info.Sum(x => x.cantidad);
                                                decimal? ConCosCom = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_com += ConCanCom;
                                                    existe.cos_com += Convert.ToDecimal(ConCosCom);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_com = ConCanCom;
                                                    refin.cos_com = Convert.ToDecimal(ConCosCom);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 2

                                            if (item.sw == 2)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanSal = info.Sum(x => x.cantidad);
                                                decimal? ConCosSal = info.Sum(x => x.costoTotal);
                                                decimal ConcanDevCom = info.Sum(x => x.cantidad);
                                                decimal? ConCosDevCom = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_sal += ConCanSal;
                                                    existe.cos_sal += Convert.ToDecimal(ConCosSal);
                                                    existe.can_dev_com += ConcanDevCom;
                                                    existe.cos_dev_com += Convert.ToDecimal(ConCosDevCom);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_sal = ConCanSal;
                                                    refin.cos_sal = Convert.ToDecimal(ConCosSal);
                                                    refin.can_dev_com = ConcanDevCom;
                                                    refin.cos_dev_com = Convert.ToDecimal(ConCosDevCom);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 3

                                            if (item.sw == 3)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanSal = info.Sum(x => x.cantidad);
                                                decimal? ConCosSal = info.Sum(x => x.costoTotal);
                                                decimal ConcanVenta = info.Sum(x => x.cantidad);
                                                decimal? ConCosVenta = info.Sum(x => x.costoTotal);
                                                decimal? ConValVenta = info.Sum(x => x.valorTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_sal += ConCanSal;
                                                    existe.cos_sal += Convert.ToDecimal(ConCosSal);
                                                    existe.can_vta += ConcanVenta;
                                                    existe.cos_vta += Convert.ToDecimal(ConCosVenta);
                                                    existe.val_vta += Convert.ToDecimal(ConValVenta);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_sal = ConCanSal;
                                                    refin.cos_sal = Convert.ToDecimal(ConCosSal);
                                                    refin.can_vta = ConcanVenta;
                                                    refin.cos_vta = Convert.ToDecimal(ConCosVenta);
                                                    refin.val_vta = Convert.ToDecimal(ConValVenta);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 4

                                            if (item.sw == 4)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConcanDevVenta = info.Sum(x => x.cantidad);
                                                decimal? ConCosDevVenta = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_dev_vta += ConcanDevVenta;
                                                    existe.cos_dev_vta += Convert.ToDecimal(ConCosDevVenta);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_dev_vta = ConcanDevVenta;
                                                    refin.cos_dev_vta = Convert.ToDecimal(ConCosDevVenta);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 8

                                            if (item.sw == 8)
                                            {
                                                if (item.bodega == item.bodegaOrigen)
                                                {
                                                    IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                        x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo && x.sw == item.sw);
                                                    decimal ConCanSal = info.Sum(x => x.cantidad);
                                                    decimal? ConCosSal = info.Sum(x => x.costoTotal);
                                                    decimal ConcanTra = info.Sum(x => x.cantidad);
                                                    decimal? ConCosTra = info.Sum(x => x.costoTotal);
                                                    referencias_inven refin = new referencias_inven();

                                                    referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                        x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo);
                                                    if (existe != null)
                                                    {
                                                        existe.codigo = item.codigo;
                                                        existe.can_sal += ConCanSal;
                                                        existe.cos_sal += Convert.ToDecimal(ConCosSal);
                                                        existe.can_tra += ConcanTra;
                                                        existe.cos_tra += Convert.ToDecimal(ConCosTra);
                                                        context.Entry(existe).State = EntityState.Modified;
                                                        context.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        refin.bodega = item.bodega;
                                                        refin.codigo = item.codigo;
                                                        refin.ano = (short)anio;
                                                        refin.mes = (short)mes;
                                                        refin.can_sal = ConCanSal;
                                                        refin.cos_sal = Convert.ToDecimal(ConCosSal);
                                                        refin.can_tra = ConcanTra;
                                                        refin.cos_tra = Convert.ToDecimal(ConCosTra);
                                                        refin.modulo = item.modulo;
                                                        context.referencias_inven.Add(refin);
                                                        context.SaveChanges();
                                                    }
                                                }
                                                else
                                                {
                                                    IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                        x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo && x.sw == item.sw);
                                                    decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                    decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                    decimal ConcanTra = info.Sum(x => x.cantidad);
                                                    decimal? ConCosTra = info.Sum(x => x.costoTotal);
                                                    referencias_inven refin = new referencias_inven();

                                                    referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                        x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo);
                                                    if (existe != null)
                                                    {
                                                        existe.codigo = item.codigo;
                                                        existe.can_ent = ConCanEnt;
                                                        existe.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                        existe.can_tra = ConcanTra;
                                                        existe.cos_tra = Convert.ToDecimal(ConCosTra);
                                                        context.Entry(existe).State = EntityState.Modified;
                                                        context.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        refin.bodega = item.bodega;
                                                        refin.codigo = item.codigo;
                                                        refin.ano = (short)anio;
                                                        refin.mes = (short)mes;
                                                        refin.can_ent = ConCanEnt;
                                                        refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                        refin.can_tra = ConcanTra;
                                                        refin.cos_tra = Convert.ToDecimal(ConCosTra);
                                                        refin.modulo = item.modulo;
                                                        context.referencias_inven.Add(refin);
                                                        context.SaveChanges();
                                                    }
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 9

                                            if (item.sw == 9)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConCanOtrEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosOtrEnt = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_otr_ent += ConCanOtrEnt;
                                                    existe.cos_otr_ent += Convert.ToDecimal(ConCosOtrEnt);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_otr_ent = ConCanOtrEnt;
                                                    refin.cos_otr_ent = Convert.ToDecimal(ConCosOtrEnt);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 10

                                            if (item.sw == 10)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConCanOtrSal = info.Sum(x => x.cantidad);
                                                decimal? ConCosOtrSal = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_otr_sal += ConCanOtrSal;
                                                    existe.cos_otr_sal += Convert.ToDecimal(ConCosOtrSal);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_otr_sal = ConCanOtrSal;
                                                    refin.cos_otr_sal = Convert.ToDecimal(ConCosOtrSal);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion
                                        }

                                        #endregion

                                        #region Si el mes es diferente a enero, solo resto 1 mes al mes filtrado

                                        else
                                        {
                                            int mesAnterior = mes - 1;

                                            #region Borrar mes anterior

                                            if (borrado == false)
                                            {
                                                List<referencias_inven> borrarMes = context.referencias_inven.Where(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo).ToList();
                                                for (int j = 0; j < borrarMes.Count(); j++)
                                                {
                                                    context.Entry((object)borrarMes[j]).State = EntityState.Deleted;
                                                    borrado = true;
                                                }

                                                context.SaveChanges();
                                            }

                                            #endregion

                                            #region Consolido lo del mes anterior para recalcular el mes filtrado

                                            if (consolidado == false)
                                            {
                                                List<referencias_inven> infoReferencias = context.referencias_inven.Where(x =>
                                                    x.ano == anio && x.mes == mesAnterior &&
                                                    (x.can_ini + x.can_ent - x.can_sal != 0 ||
                                                     x.cos_ini + x.cos_ent - x.cos_sal != 0)).ToList();
                                                foreach (referencias_inven a in infoReferencias)
                                                {
                                                    referencias_inven refin = new referencias_inven();
                                                    referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                        x.ano == anio && x.mes == mes && x.bodega == a.bodega &&
                                                        x.codigo == a.codigo);
                                                    if (existe != null)
                                                    {
                                                        existe.can_ini += a.can_ini + a.can_ent - a.can_sal;
                                                        existe.cos_ini += a.cos_ini + a.cos_ent - a.cos_sal;
                                                        context.Entry(existe).State = EntityState.Modified;
                                                        context.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        refin.bodega = a.bodega;
                                                        refin.codigo = a.codigo;
                                                        refin.ano = (short)anio;
                                                        refin.mes = (short)mes;
                                                        refin.can_ini = a.can_ini + a.can_ent - a.can_sal;
                                                        refin.cos_ini = a.cos_ini + a.cos_ent - a.cos_sal;
                                                        refin.modulo = a.modulo;
                                                        context.referencias_inven.Add(refin);
                                                        context.SaveChanges();
                                                    }
                                                }

                                                consolidado = true;
                                            }

                                            #endregion

                                            #region Si SW = 1

                                            if (item.sw == 1)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);

                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConCanCom = info.Sum(x => x.cantidad);
                                                decimal? ConCosCom = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_com += ConCanCom;
                                                    existe.cos_com += Convert.ToDecimal(ConCosCom);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_com = ConCanCom;
                                                    refin.cos_com = Convert.ToDecimal(ConCosCom);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 2

                                            if (item.sw == 2)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanSal = info.Sum(x => x.cantidad);
                                                decimal? ConCosSal = info.Sum(x => x.costoTotal);
                                                decimal ConcanDevCom = info.Sum(x => x.cantidad);
                                                decimal? ConCosDevCom = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_sal += ConCanSal;
                                                    existe.cos_sal += Convert.ToDecimal(ConCosSal);
                                                    existe.can_dev_com += ConcanDevCom;
                                                    existe.cos_dev_com += Convert.ToDecimal(ConCosDevCom);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_sal = ConCanSal;
                                                    refin.cos_sal = Convert.ToDecimal(ConCosSal);
                                                    refin.can_dev_com = ConcanDevCom;
                                                    refin.cos_dev_com = Convert.ToDecimal(ConCosDevCom);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 3

                                            if (item.sw == 3)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanSal = info.Sum(x => x.cantidad);
                                                decimal? ConCosSal = info.Sum(x => x.costoTotal);
                                                decimal ConcanVenta = info.Sum(x => x.cantidad);
                                                decimal? ConCosVenta = info.Sum(x => x.costoTotal);
                                                decimal? ConValVenta = info.Sum(x => x.valorTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_sal += ConCanSal;
                                                    existe.cos_sal += Convert.ToDecimal(ConCosSal);
                                                    existe.can_vta += ConcanVenta;
                                                    existe.cos_vta += Convert.ToDecimal(ConCosVenta);
                                                    existe.val_vta += Convert.ToDecimal(ConValVenta);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_sal = ConCanSal;
                                                    refin.cos_sal = Convert.ToDecimal(ConCosSal);
                                                    refin.can_vta = ConcanVenta;
                                                    refin.cos_vta = Convert.ToDecimal(ConCosVenta);
                                                    refin.val_vta = Convert.ToDecimal(ConValVenta);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 4

                                            if (item.sw == 4)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConcanDevVenta = info.Sum(x => x.cantidad);
                                                decimal? ConCosDevVenta = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_dev_vta += ConcanDevVenta;
                                                    existe.cos_dev_vta += Convert.ToDecimal(ConCosDevVenta);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_dev_vta = ConcanDevVenta;
                                                    refin.cos_dev_vta = Convert.ToDecimal(ConCosDevVenta);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 8

                                            if (item.sw == 8)
                                            {
                                                if (item.bodega == item.bodegaOrigen)
                                                {
                                                    IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                        x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo && x.sw == item.sw);
                                                    decimal ConCanSal = info.Sum(x => x.cantidad);
                                                    decimal? ConCosSal = info.Sum(x => x.costoTotal);
                                                    decimal ConcanTra = info.Sum(x => x.cantidad);
                                                    decimal? ConCosTra = info.Sum(x => x.costoTotal);
                                                    referencias_inven refin = new referencias_inven();

                                                    referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                        x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo);
                                                    if (existe != null)
                                                    {
                                                        existe.codigo = item.codigo;
                                                        existe.can_sal += ConCanSal;
                                                        existe.cos_sal += Convert.ToDecimal(ConCosSal);
                                                        existe.can_tra += ConcanTra;
                                                        existe.cos_tra += Convert.ToDecimal(ConCosTra);
                                                        context.Entry(existe).State = EntityState.Modified;
                                                        context.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        refin.bodega = item.bodega;
                                                        refin.codigo = item.codigo;
                                                        refin.ano = (short)anio;
                                                        refin.mes = (short)mes;
                                                        refin.can_sal = ConCanSal;
                                                        refin.cos_sal = Convert.ToDecimal(ConCosSal);
                                                        refin.can_tra = ConcanTra;
                                                        refin.cos_tra = Convert.ToDecimal(ConCosTra);
                                                        refin.modulo = item.modulo;
                                                        context.referencias_inven.Add(refin);
                                                        context.SaveChanges();
                                                    }
                                                }
                                                else
                                                {
                                                    IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                        x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo && x.sw == item.sw);
                                                    decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                    decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                    decimal ConcanTra = info.Sum(x => x.cantidad);
                                                    decimal? ConCosTra = info.Sum(x => x.costoTotal);
                                                    referencias_inven refin = new referencias_inven();

                                                    referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                        x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo);
                                                    if (existe != null)
                                                    {
                                                        existe.codigo = item.codigo;
                                                        existe.can_ent = ConCanEnt;
                                                        existe.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                        existe.can_tra = ConcanTra;
                                                        existe.cos_tra = Convert.ToDecimal(ConCosTra);
                                                        context.Entry(existe).State = EntityState.Modified;
                                                        context.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        refin.bodega = item.bodega;
                                                        refin.codigo = item.codigo;
                                                        refin.ano = (short)anio;
                                                        refin.mes = (short)mes;
                                                        refin.can_ent = ConCanEnt;
                                                        refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                        refin.can_tra = ConcanTra;
                                                        refin.cos_tra = Convert.ToDecimal(ConCosTra);
                                                        refin.modulo = item.modulo;
                                                        context.referencias_inven.Add(refin);
                                                        context.SaveChanges();
                                                    }
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 9

                                            if (item.sw == 9)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConCanOtrEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosOtrEnt = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_otr_ent += ConCanOtrEnt;
                                                    existe.cos_otr_ent += Convert.ToDecimal(ConCosOtrEnt);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_otr_ent = ConCanOtrEnt;
                                                    refin.cos_otr_ent = Convert.ToDecimal(ConCosOtrEnt);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 10

                                            if (item.sw == 10)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConCanOtrSal = info.Sum(x => x.cantidad);
                                                decimal? ConCosOtrSal = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_otr_sal += ConCanOtrSal;
                                                    existe.cos_otr_sal += Convert.ToDecimal(ConCosOtrSal);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_otr_sal = ConCanOtrSal;
                                                    refin.cos_otr_sal = Convert.ToDecimal(ConCosOtrSal);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion
                                        }

                                        #endregion

                                        #endregion
                                    }

                                    #endregion

                                    #region Si solo tiene bodega

                                    else
                                    {
                                        #region Validaciones (1 → por el mes filtrado y  2 → por el sw que tenga la referencia)

                                        #region Si el mes es enero (debo restar tambien el año)

                                        if (mes == 1)
                                        {
                                            int anioAnterior = anio - 1;
                                            int mesAnterior = 12;

                                            #region Borrar mes anterior

                                            if (borrado == false)
                                            {
                                                List<referencias_inven> borrarMes = context.referencias_inven.Where(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega).ToList();
                                                for (int j = 0; j < borrarMes.Count(); j++)
                                                {
                                                    context.Entry((object)borrarMes[j]).State = EntityState.Deleted;
                                                    borrado = true;
                                                }

                                                context.SaveChanges();
                                            }

                                            #endregion

                                            #region Consolido lo del mes anterior para recalcular el mes filtrado

                                            if (consolidado == false)
                                            {
                                                List<referencias_inven> infoReferencias = context.referencias_inven.Where(x =>
                                                    x.ano == anio && x.mes == mesAnterior &&
                                                    (x.can_ini + x.can_ent - x.can_sal != 0 ||
                                                     x.cos_ini + x.cos_ent - x.cos_sal != 0)).ToList();
                                                foreach (referencias_inven a in infoReferencias)
                                                {
                                                    referencias_inven refin = new referencias_inven();
                                                    referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                        x.ano == anio && x.mes == mes && x.bodega == a.bodega &&
                                                        x.codigo == a.codigo);
                                                    if (existe != null)
                                                    {
                                                        existe.can_ini += a.can_ini + a.can_ent - a.can_sal;
                                                        existe.cos_ini += a.cos_ini + a.cos_ent - a.cos_sal;
                                                        context.Entry(existe).State = EntityState.Modified;
                                                        context.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        refin.bodega = a.bodega;
                                                        refin.codigo = a.codigo;
                                                        refin.ano = (short)anio;
                                                        refin.mes = (short)mes;
                                                        refin.can_ini = a.can_ini + a.can_ent - a.can_sal;
                                                        refin.cos_ini = a.cos_ini + a.cos_ent - a.cos_sal;
                                                        refin.modulo = a.modulo;
                                                        context.referencias_inven.Add(refin);
                                                        context.SaveChanges();
                                                    }
                                                }

                                                consolidado = true;
                                            }

                                            #endregion

                                            #region Si SW = 1

                                            if (item.sw == 1)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);

                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConCanCom = info.Sum(x => x.cantidad);
                                                decimal? ConCosCom = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_com += ConCanCom;
                                                    existe.cos_com += Convert.ToDecimal(ConCosCom);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_com = ConCanCom;
                                                    refin.cos_com = Convert.ToDecimal(ConCosCom);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 2

                                            if (item.sw == 2)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanSal = info.Sum(x => x.cantidad);
                                                decimal? ConCosSal = info.Sum(x => x.costoTotal);
                                                decimal ConcanDevCom = info.Sum(x => x.cantidad);
                                                decimal? ConCosDevCom = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_sal += ConCanSal;
                                                    existe.cos_sal += Convert.ToDecimal(ConCosSal);
                                                    existe.can_dev_com += ConcanDevCom;
                                                    existe.cos_dev_com += Convert.ToDecimal(ConCosDevCom);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_sal = ConCanSal;
                                                    refin.cos_sal = Convert.ToDecimal(ConCosSal);
                                                    refin.can_dev_com = ConcanDevCom;
                                                    refin.cos_dev_com = Convert.ToDecimal(ConCosDevCom);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 3

                                            if (item.sw == 3)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanSal = info.Sum(x => x.cantidad);
                                                decimal? ConCosSal = info.Sum(x => x.costoTotal);
                                                decimal ConcanVenta = info.Sum(x => x.cantidad);
                                                decimal? ConCosVenta = info.Sum(x => x.costoTotal);
                                                decimal? ConValVenta = info.Sum(x => x.valorTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_sal += ConCanSal;
                                                    existe.cos_sal += Convert.ToDecimal(ConCosSal);
                                                    existe.can_vta += ConcanVenta;
                                                    existe.cos_vta += Convert.ToDecimal(ConCosVenta);
                                                    existe.val_vta += Convert.ToDecimal(ConValVenta);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_sal = ConCanSal;
                                                    refin.cos_sal = Convert.ToDecimal(ConCosSal);
                                                    refin.can_vta = ConcanVenta;
                                                    refin.cos_vta = Convert.ToDecimal(ConCosVenta);
                                                    refin.val_vta = Convert.ToDecimal(ConValVenta);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 4

                                            if (item.sw == 4)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConcanDevVenta = info.Sum(x => x.cantidad);
                                                decimal? ConCosDevVenta = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_dev_vta += ConcanDevVenta;
                                                    existe.cos_dev_vta += Convert.ToDecimal(ConCosDevVenta);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_dev_vta = ConcanDevVenta;
                                                    refin.cos_dev_vta = Convert.ToDecimal(ConCosDevVenta);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 8

                                            if (item.sw == 8)
                                            {
                                                if (item.bodega == item.bodegaOrigen)
                                                {
                                                    IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                        x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo && x.sw == item.sw);
                                                    decimal ConCanSal = info.Sum(x => x.cantidad);
                                                    decimal? ConCosSal = info.Sum(x => x.costoTotal);
                                                    decimal ConcanTra = info.Sum(x => x.cantidad);
                                                    decimal? ConCosTra = info.Sum(x => x.costoTotal);
                                                    referencias_inven refin = new referencias_inven();

                                                    referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                        x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo);
                                                    if (existe != null)
                                                    {
                                                        existe.codigo = item.codigo;
                                                        existe.can_sal += ConCanSal;
                                                        existe.cos_sal += Convert.ToDecimal(ConCosSal);
                                                        existe.can_tra += ConcanTra;
                                                        existe.cos_tra += Convert.ToDecimal(ConCosTra);
                                                        context.Entry(existe).State = EntityState.Modified;
                                                        context.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        refin.bodega = item.bodega;
                                                        refin.codigo = item.codigo;
                                                        refin.ano = (short)anio;
                                                        refin.mes = (short)mes;
                                                        refin.can_sal = ConCanSal;
                                                        refin.cos_sal = Convert.ToDecimal(ConCosSal);
                                                        refin.can_tra = ConcanTra;
                                                        refin.cos_tra = Convert.ToDecimal(ConCosTra);
                                                        refin.modulo = item.modulo;
                                                        context.referencias_inven.Add(refin);
                                                        context.SaveChanges();
                                                    }
                                                }
                                                else
                                                {
                                                    IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                        x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo && x.sw == item.sw);
                                                    decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                    decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                    decimal ConcanTra = info.Sum(x => x.cantidad);
                                                    decimal? ConCosTra = info.Sum(x => x.costoTotal);
                                                    referencias_inven refin = new referencias_inven();

                                                    referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                        x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo);
                                                    if (existe != null)
                                                    {
                                                        existe.codigo = item.codigo;
                                                        existe.can_ent = ConCanEnt;
                                                        existe.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                        existe.can_tra = ConcanTra;
                                                        existe.cos_tra = Convert.ToDecimal(ConCosTra);
                                                        context.Entry(existe).State = EntityState.Modified;
                                                        context.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        refin.bodega = item.bodega;
                                                        refin.codigo = item.codigo;
                                                        refin.ano = (short)anio;
                                                        refin.mes = (short)mes;
                                                        refin.can_ent = ConCanEnt;
                                                        refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                        refin.can_tra = ConcanTra;
                                                        refin.cos_tra = Convert.ToDecimal(ConCosTra);
                                                        refin.modulo = item.modulo;
                                                        context.referencias_inven.Add(refin);
                                                        context.SaveChanges();
                                                    }
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 9

                                            if (item.sw == 9)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConCanOtrEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosOtrEnt = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_otr_ent += ConCanOtrEnt;
                                                    existe.cos_otr_ent += Convert.ToDecimal(ConCosOtrEnt);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_otr_ent = ConCanOtrEnt;
                                                    refin.cos_otr_ent = Convert.ToDecimal(ConCosOtrEnt);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 10

                                            if (item.sw == 10)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConCanOtrSal = info.Sum(x => x.cantidad);
                                                decimal? ConCosOtrSal = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_otr_sal += ConCanOtrSal;
                                                    existe.cos_otr_sal += Convert.ToDecimal(ConCosOtrSal);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_otr_sal = ConCanOtrSal;
                                                    refin.cos_otr_sal = Convert.ToDecimal(ConCosOtrSal);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion
                                        }

                                        #endregion

                                        #region Si el mes es diferente a enero, solo resto 1 mes al mes filtrado

                                        else
                                        {
                                            int mesAnterior = mes - 1;

                                            #region Borrar mes anterior

                                            List<referencias_inven> borrarMes = context.referencias_inven.Where(x =>
                                                x.ano == anio && x.mes == mes && x.bodega == item.bodega).ToList();
                                            for (int j = 0; j < borrarMes.Count(); j++)
                                            {
                                                context.Entry((object)borrarMes[j]).State = EntityState.Deleted;
                                            }

                                            context.SaveChanges();

                                            #endregion

                                            #region Consolido lo del mes anterior para recalcular el mes filtrado

                                            if (consolidado == false)
                                            {
                                                List<referencias_inven> infoReferencias = context.referencias_inven.Where(x =>
                                                    x.ano == anio && x.mes == mesAnterior &&
                                                    (x.can_ini + x.can_ent - x.can_sal != 0 ||
                                                     x.cos_ini + x.cos_ent - x.cos_sal != 0)).ToList();
                                                foreach (referencias_inven a in infoReferencias)
                                                {
                                                    referencias_inven refin = new referencias_inven();
                                                    referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                        x.ano == anio && x.mes == mes && x.bodega == a.bodega &&
                                                        x.codigo == a.codigo);
                                                    if (existe != null)
                                                    {
                                                        existe.can_ini += a.can_ini + a.can_ent - a.can_sal;
                                                        existe.cos_ini += a.cos_ini + a.cos_ent - a.cos_sal;
                                                        context.Entry(existe).State = EntityState.Modified;
                                                        context.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        refin.bodega = a.bodega;
                                                        refin.codigo = a.codigo;
                                                        refin.ano = (short)anio;
                                                        refin.mes = (short)mes;
                                                        refin.can_ini = a.can_ini + a.can_ent - a.can_sal;
                                                        refin.cos_ini = a.cos_ini + a.cos_ent - a.cos_sal;
                                                        refin.modulo = a.modulo;
                                                        context.referencias_inven.Add(refin);
                                                        context.SaveChanges();
                                                    }
                                                }

                                                consolidado = true;
                                            }

                                            #endregion

                                            #region Si SW = 1

                                            if (item.sw == 1)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);

                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConCanCom = info.Sum(x => x.cantidad);
                                                decimal? ConCosCom = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_com += ConCanCom;
                                                    existe.cos_com += Convert.ToDecimal(ConCosCom);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_com = ConCanCom;
                                                    refin.cos_com = Convert.ToDecimal(ConCosCom);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 2

                                            if (item.sw == 2)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanSal = info.Sum(x => x.cantidad);
                                                decimal? ConCosSal = info.Sum(x => x.costoTotal);
                                                decimal ConcanDevCom = info.Sum(x => x.cantidad);
                                                decimal? ConCosDevCom = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_sal += ConCanSal;
                                                    existe.cos_sal += Convert.ToDecimal(ConCosSal);
                                                    existe.can_dev_com += ConcanDevCom;
                                                    existe.cos_dev_com += Convert.ToDecimal(ConCosDevCom);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_sal = ConCanSal;
                                                    refin.cos_sal = Convert.ToDecimal(ConCosSal);
                                                    refin.can_dev_com = ConcanDevCom;
                                                    refin.cos_dev_com = Convert.ToDecimal(ConCosDevCom);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 3

                                            if (item.sw == 3)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanSal = info.Sum(x => x.cantidad);
                                                decimal? ConCosSal = info.Sum(x => x.costoTotal);
                                                decimal ConcanVenta = info.Sum(x => x.cantidad);
                                                decimal? ConCosVenta = info.Sum(x => x.costoTotal);
                                                decimal? ConValVenta = info.Sum(x => x.valorTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_sal += ConCanSal;
                                                    existe.cos_sal += Convert.ToDecimal(ConCosSal);
                                                    existe.can_vta += ConcanVenta;
                                                    existe.cos_vta += Convert.ToDecimal(ConCosVenta);
                                                    existe.val_vta += Convert.ToDecimal(ConValVenta);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_sal = ConCanSal;
                                                    refin.cos_sal = Convert.ToDecimal(ConCosSal);
                                                    refin.can_vta = ConcanVenta;
                                                    refin.cos_vta = Convert.ToDecimal(ConCosVenta);
                                                    refin.val_vta = Convert.ToDecimal(ConValVenta);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 4

                                            if (item.sw == 4)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConcanDevVenta = info.Sum(x => x.cantidad);
                                                decimal? ConCosDevVenta = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_dev_vta += ConcanDevVenta;
                                                    existe.cos_dev_vta += Convert.ToDecimal(ConCosDevVenta);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_dev_vta = ConcanDevVenta;
                                                    refin.cos_dev_vta = Convert.ToDecimal(ConCosDevVenta);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 8

                                            if (item.sw == 8)
                                            {
                                                if (item.bodega == item.bodegaOrigen)
                                                {
                                                    IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                        x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo && x.sw == item.sw);
                                                    decimal ConCanSal = info.Sum(x => x.cantidad);
                                                    decimal? ConCosSal = info.Sum(x => x.costoTotal);
                                                    decimal ConcanTra = info.Sum(x => x.cantidad);
                                                    decimal? ConCosTra = info.Sum(x => x.costoTotal);
                                                    referencias_inven refin = new referencias_inven();

                                                    referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                        x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo);
                                                    if (existe != null)
                                                    {
                                                        existe.codigo = item.codigo;
                                                        existe.can_sal += ConCanSal;
                                                        existe.cos_sal += Convert.ToDecimal(ConCosSal);
                                                        existe.can_tra += ConcanTra;
                                                        existe.cos_tra += Convert.ToDecimal(ConCosTra);
                                                        context.Entry(existe).State = EntityState.Modified;
                                                        context.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        refin.bodega = item.bodega;
                                                        refin.codigo = item.codigo;
                                                        refin.ano = (short)anio;
                                                        refin.mes = (short)mes;
                                                        refin.can_sal = ConCanSal;
                                                        refin.cos_sal = Convert.ToDecimal(ConCosSal);
                                                        refin.can_tra = ConcanTra;
                                                        refin.cos_tra = Convert.ToDecimal(ConCosTra);
                                                        refin.modulo = item.modulo;
                                                        context.referencias_inven.Add(refin);
                                                        context.SaveChanges();
                                                    }
                                                }
                                                else
                                                {
                                                    IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                        x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo && x.sw == item.sw);
                                                    decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                    decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                    decimal ConcanTra = info.Sum(x => x.cantidad);
                                                    decimal? ConCosTra = info.Sum(x => x.costoTotal);
                                                    referencias_inven refin = new referencias_inven();

                                                    referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                        x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo);
                                                    if (existe != null)
                                                    {
                                                        existe.codigo = item.codigo;
                                                        existe.can_ent = ConCanEnt;
                                                        existe.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                        existe.can_tra = ConcanTra;
                                                        existe.cos_tra = Convert.ToDecimal(ConCosTra);
                                                        context.Entry(existe).State = EntityState.Modified;
                                                        context.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        refin.bodega = item.bodega;
                                                        refin.codigo = item.codigo;
                                                        refin.ano = (short)anio;
                                                        refin.mes = (short)mes;
                                                        refin.can_ent = ConCanEnt;
                                                        refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                        refin.can_tra = ConcanTra;
                                                        refin.cos_tra = Convert.ToDecimal(ConCosTra);
                                                        refin.modulo = item.modulo;
                                                        context.referencias_inven.Add(refin);
                                                        context.SaveChanges();
                                                    }
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 9

                                            if (item.sw == 9)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConCanOtrEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosOtrEnt = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_otr_ent += ConCanOtrEnt;
                                                    existe.cos_otr_ent += Convert.ToDecimal(ConCosOtrEnt);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_otr_ent = ConCanOtrEnt;
                                                    refin.cos_otr_ent = Convert.ToDecimal(ConCosOtrEnt);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 10

                                            if (item.sw == 10)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConCanOtrSal = info.Sum(x => x.cantidad);
                                                decimal? ConCosOtrSal = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_otr_sal += ConCanOtrSal;
                                                    existe.cos_otr_sal += Convert.ToDecimal(ConCosOtrSal);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_otr_sal = ConCanOtrSal;
                                                    refin.cos_otr_sal = Convert.ToDecimal(ConCosOtrSal);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion
                                        }

                                        #endregion

                                        #endregion
                                    }

                                    #endregion
                                }
                                else
                                {
                                    if (codigo != "")
                                    {
                                        #region Validaciones (1 → por el mes filtrado y  2 → por el sw que tenga la referencia)

                                        #region Si el mes es enero (debo restar tambien el año)

                                        if (mes == 1)
                                        {
                                            int anioAnterior = anio - 1;
                                            int mesAnterior = 12;

                                            #region Borrar mes anterior

                                            if (borrado == false)
                                            {
                                                List<referencias_inven> borrarMes = context.referencias_inven.Where(x =>
                                                    x.ano == anio && x.mes == mes && x.codigo == item.codigo).ToList();
                                                for (int j = 0; j < borrarMes.Count(); j++)
                                                {
                                                    context.Entry((object)borrarMes[j]).State = EntityState.Deleted;
                                                    borrado = true;
                                                }

                                                context.SaveChanges();
                                            }

                                            #endregion

                                            #region Consolido lo del mes anterior para recalcular el mes filtrado

                                            if (consolidado == false)
                                            {
                                                List<referencias_inven> infoReferencias = context.referencias_inven.Where(x =>
                                                    x.ano == anio && x.mes == mesAnterior &&
                                                    (x.can_ini + x.can_ent - x.can_sal != 0 ||
                                                     x.cos_ini + x.cos_ent - x.cos_sal != 0)).ToList();
                                                foreach (referencias_inven a in infoReferencias)
                                                {
                                                    referencias_inven refin = new referencias_inven();
                                                    referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                        x.ano == anio && x.mes == mes && x.bodega == a.bodega &&
                                                        x.codigo == a.codigo);
                                                    if (existe != null)
                                                    {
                                                        existe.can_ini += a.can_ini + a.can_ent - a.can_sal;
                                                        existe.cos_ini += a.cos_ini + a.cos_ent - a.cos_sal;
                                                        context.Entry(existe).State = EntityState.Modified;
                                                        context.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        refin.bodega = a.bodega;
                                                        refin.codigo = a.codigo;
                                                        refin.ano = (short)anio;
                                                        refin.mes = (short)mes;
                                                        refin.can_ini = a.can_ini + a.can_ent - a.can_sal;
                                                        refin.cos_ini = a.cos_ini + a.cos_ent - a.cos_sal;
                                                        refin.modulo = a.modulo;
                                                        context.referencias_inven.Add(refin);
                                                        context.SaveChanges();
                                                    }
                                                }

                                                consolidado = true;
                                            }

                                            #endregion

                                            #region Si SW = 1

                                            if (item.sw == 1)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);

                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConCanCom = info.Sum(x => x.cantidad);
                                                decimal? ConCosCom = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_com += ConCanCom;
                                                    existe.cos_com += Convert.ToDecimal(ConCosCom);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_com = ConCanCom;
                                                    refin.cos_com = Convert.ToDecimal(ConCosCom);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 2

                                            if (item.sw == 2)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanSal = info.Sum(x => x.cantidad);
                                                decimal? ConCosSal = info.Sum(x => x.costoTotal);
                                                decimal ConcanDevCom = info.Sum(x => x.cantidad);
                                                decimal? ConCosDevCom = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_sal += ConCanSal;
                                                    existe.cos_sal += Convert.ToDecimal(ConCosSal);
                                                    existe.can_dev_com += ConcanDevCom;
                                                    existe.cos_dev_com += Convert.ToDecimal(ConCosDevCom);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_sal = ConCanSal;
                                                    refin.cos_sal = Convert.ToDecimal(ConCosSal);
                                                    refin.can_dev_com = ConcanDevCom;
                                                    refin.cos_dev_com = Convert.ToDecimal(ConCosDevCom);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 3

                                            if (item.sw == 3)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanSal = info.Sum(x => x.cantidad);
                                                decimal? ConCosSal = info.Sum(x => x.costoTotal);
                                                decimal ConcanVenta = info.Sum(x => x.cantidad);
                                                decimal? ConCosVenta = info.Sum(x => x.costoTotal);
                                                decimal? ConValVenta = info.Sum(x => x.valorTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_sal += ConCanSal;
                                                    existe.cos_sal += Convert.ToDecimal(ConCosSal);
                                                    existe.can_vta += ConcanVenta;
                                                    existe.cos_vta += Convert.ToDecimal(ConCosVenta);
                                                    existe.val_vta += Convert.ToDecimal(ConValVenta);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_sal = ConCanSal;
                                                    refin.cos_sal = Convert.ToDecimal(ConCosSal);
                                                    refin.can_vta = ConcanVenta;
                                                    refin.cos_vta = Convert.ToDecimal(ConCosVenta);
                                                    refin.val_vta = Convert.ToDecimal(ConValVenta);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 4

                                            if (item.sw == 4)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConcanDevVenta = info.Sum(x => x.cantidad);
                                                decimal? ConCosDevVenta = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_dev_vta += ConcanDevVenta;
                                                    existe.cos_dev_vta += Convert.ToDecimal(ConCosDevVenta);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_dev_vta = ConcanDevVenta;
                                                    refin.cos_dev_vta = Convert.ToDecimal(ConCosDevVenta);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 8

                                            if (item.sw == 8)
                                            {
                                                if (item.bodega == item.bodegaOrigen)
                                                {
                                                    IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                        x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo && x.sw == item.sw);
                                                    decimal ConCanSal = info.Sum(x => x.cantidad);
                                                    decimal? ConCosSal = info.Sum(x => x.costoTotal);
                                                    decimal ConcanTra = info.Sum(x => x.cantidad);
                                                    decimal? ConCosTra = info.Sum(x => x.costoTotal);
                                                    referencias_inven refin = new referencias_inven();

                                                    referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                        x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo);
                                                    if (existe != null)
                                                    {
                                                        existe.codigo = item.codigo;
                                                        existe.can_sal += ConCanSal;
                                                        existe.cos_sal += Convert.ToDecimal(ConCosSal);
                                                        existe.can_tra += ConcanTra;
                                                        existe.cos_tra += Convert.ToDecimal(ConCosTra);
                                                        context.Entry(existe).State = EntityState.Modified;
                                                        context.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        refin.bodega = item.bodega;
                                                        refin.codigo = item.codigo;
                                                        refin.ano = (short)anio;
                                                        refin.mes = (short)mes;
                                                        refin.can_sal = ConCanSal;
                                                        refin.cos_sal = Convert.ToDecimal(ConCosSal);
                                                        refin.can_tra = ConcanTra;
                                                        refin.cos_tra = Convert.ToDecimal(ConCosTra);
                                                        refin.modulo = item.modulo;
                                                        context.referencias_inven.Add(refin);
                                                        context.SaveChanges();
                                                    }
                                                }
                                                else
                                                {
                                                    IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                        x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo && x.sw == item.sw);
                                                    decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                    decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                    decimal ConcanTra = info.Sum(x => x.cantidad);
                                                    decimal? ConCosTra = info.Sum(x => x.costoTotal);
                                                    referencias_inven refin = new referencias_inven();

                                                    referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                        x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo);
                                                    if (existe != null)
                                                    {
                                                        existe.codigo = item.codigo;
                                                        existe.can_ent = ConCanEnt;
                                                        existe.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                        existe.can_tra = ConcanTra;
                                                        existe.cos_tra = Convert.ToDecimal(ConCosTra);
                                                        context.Entry(existe).State = EntityState.Modified;
                                                        context.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        refin.bodega = item.bodega;
                                                        refin.codigo = item.codigo;
                                                        refin.ano = (short)anio;
                                                        refin.mes = (short)mes;
                                                        refin.can_ent = ConCanEnt;
                                                        refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                        refin.can_tra = ConcanTra;
                                                        refin.cos_tra = Convert.ToDecimal(ConCosTra);
                                                        refin.modulo = item.modulo;
                                                        context.referencias_inven.Add(refin);
                                                        context.SaveChanges();
                                                    }
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 9

                                            if (item.sw == 9)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConCanOtrEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosOtrEnt = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_otr_ent += ConCanOtrEnt;
                                                    existe.cos_otr_ent += Convert.ToDecimal(ConCosOtrEnt);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_otr_ent = ConCanOtrEnt;
                                                    refin.cos_otr_ent = Convert.ToDecimal(ConCosOtrEnt);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 10

                                            if (item.sw == 10)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConCanOtrSal = info.Sum(x => x.cantidad);
                                                decimal? ConCosOtrSal = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_otr_sal += ConCanOtrSal;
                                                    existe.cos_otr_sal += Convert.ToDecimal(ConCosOtrSal);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_otr_sal = ConCanOtrSal;
                                                    refin.cos_otr_sal = Convert.ToDecimal(ConCosOtrSal);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion
                                        }

                                        #endregion

                                        #region Si el mes es diferente a enero, solo resto 1 mes al mes filtrado

                                        else
                                        {
                                            int mesAnterior = mes - 1;

                                            #region Borrar mes anterior

                                            if (borrado == false)
                                            {
                                                List<referencias_inven> borrarMes = context.referencias_inven.Where(x =>
                                                    x.ano == anio && x.mes == mes && x.codigo == item.codigo).ToList();
                                                for (int j = 0; j < borrarMes.Count(); j++)
                                                {
                                                    context.Entry((object)borrarMes[j]).State = EntityState.Deleted;
                                                    borrado = true;
                                                }

                                                context.SaveChanges();
                                            }

                                            #endregion

                                            #region Consolido lo del mes anterior para recalcular el mes filtrado

                                            if (consolidado == false)
                                            {
                                                List<referencias_inven> infoReferencias = context.referencias_inven.Where(x =>
                                                    x.ano == anio && x.mes == mesAnterior &&
                                                    (x.can_ini + x.can_ent - x.can_sal != 0 ||
                                                     x.cos_ini + x.cos_ent - x.cos_sal != 0)).ToList();
                                                foreach (referencias_inven a in infoReferencias)
                                                {
                                                    referencias_inven refin = new referencias_inven();
                                                    referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                        x.ano == anio && x.mes == mes && x.bodega == a.bodega &&
                                                        x.codigo == a.codigo);
                                                    if (existe != null)
                                                    {
                                                        existe.can_ini += a.can_ini + a.can_ent - a.can_sal;
                                                        existe.cos_ini += a.cos_ini + a.cos_ent - a.cos_sal;
                                                        context.Entry(existe).State = EntityState.Modified;
                                                        context.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        refin.bodega = a.bodega;
                                                        refin.codigo = a.codigo;
                                                        refin.ano = (short)anio;
                                                        refin.mes = (short)mes;
                                                        refin.can_ini = a.can_ini + a.can_ent - a.can_sal;
                                                        refin.cos_ini = a.cos_ini + a.cos_ent - a.cos_sal;
                                                        refin.modulo = a.modulo;
                                                        context.referencias_inven.Add(refin);
                                                        context.SaveChanges();
                                                    }
                                                }

                                                consolidado = true;
                                            }

                                            #endregion

                                            #region Si SW = 1

                                            if (item.sw == 1)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);

                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConCanCom = info.Sum(x => x.cantidad);
                                                decimal? ConCosCom = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_com += ConCanCom;
                                                    existe.cos_com += Convert.ToDecimal(ConCosCom);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_com = ConCanCom;
                                                    refin.cos_com = Convert.ToDecimal(ConCosCom);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 2

                                            if (item.sw == 2)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanSal = info.Sum(x => x.cantidad);
                                                decimal? ConCosSal = info.Sum(x => x.costoTotal);
                                                decimal ConcanDevCom = info.Sum(x => x.cantidad);
                                                decimal? ConCosDevCom = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_sal += ConCanSal;
                                                    existe.cos_sal += Convert.ToDecimal(ConCosSal);
                                                    existe.can_dev_com += ConcanDevCom;
                                                    existe.cos_dev_com += Convert.ToDecimal(ConCosDevCom);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_sal = ConCanSal;
                                                    refin.cos_sal = Convert.ToDecimal(ConCosSal);
                                                    refin.can_dev_com = ConcanDevCom;
                                                    refin.cos_dev_com = Convert.ToDecimal(ConCosDevCom);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 3

                                            if (item.sw == 3)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanSal = info.Sum(x => x.cantidad);
                                                decimal? ConCosSal = info.Sum(x => x.costoTotal);
                                                decimal ConcanVenta = info.Sum(x => x.cantidad);
                                                decimal? ConCosVenta = info.Sum(x => x.costoTotal);
                                                decimal? ConValVenta = info.Sum(x => x.valorTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_sal += ConCanSal;
                                                    existe.cos_sal += Convert.ToDecimal(ConCosSal);
                                                    existe.can_vta += ConcanVenta;
                                                    existe.cos_vta += Convert.ToDecimal(ConCosVenta);
                                                    existe.val_vta += Convert.ToDecimal(ConValVenta);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_sal = ConCanSal;
                                                    refin.cos_sal = Convert.ToDecimal(ConCosSal);
                                                    refin.can_vta = ConcanVenta;
                                                    refin.cos_vta = Convert.ToDecimal(ConCosVenta);
                                                    refin.val_vta = Convert.ToDecimal(ConValVenta);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 4

                                            if (item.sw == 4)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConcanDevVenta = info.Sum(x => x.cantidad);
                                                decimal? ConCosDevVenta = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_dev_vta += ConcanDevVenta;
                                                    existe.cos_dev_vta += Convert.ToDecimal(ConCosDevVenta);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_dev_vta = ConcanDevVenta;
                                                    refin.cos_dev_vta = Convert.ToDecimal(ConCosDevVenta);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 8

                                            if (item.sw == 8)
                                            {
                                                if (item.bodega == item.bodegaOrigen)
                                                {
                                                    IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                        x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo && x.sw == item.sw);
                                                    decimal ConCanSal = info.Sum(x => x.cantidad);
                                                    decimal? ConCosSal = info.Sum(x => x.costoTotal);
                                                    decimal ConcanTra = info.Sum(x => x.cantidad);
                                                    decimal? ConCosTra = info.Sum(x => x.costoTotal);
                                                    referencias_inven refin = new referencias_inven();

                                                    referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                        x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo);
                                                    if (existe != null)
                                                    {
                                                        existe.codigo = item.codigo;
                                                        existe.can_sal += ConCanSal;
                                                        existe.cos_sal += Convert.ToDecimal(ConCosSal);
                                                        existe.can_tra += ConcanTra;
                                                        existe.cos_tra += Convert.ToDecimal(ConCosTra);
                                                        context.Entry(existe).State = EntityState.Modified;
                                                        context.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        refin.bodega = item.bodega;
                                                        refin.codigo = item.codigo;
                                                        refin.ano = (short)anio;
                                                        refin.mes = (short)mes;
                                                        refin.can_sal = ConCanSal;
                                                        refin.cos_sal = Convert.ToDecimal(ConCosSal);
                                                        refin.can_tra = ConcanTra;
                                                        refin.cos_tra = Convert.ToDecimal(ConCosTra);
                                                        refin.modulo = item.modulo;
                                                        context.referencias_inven.Add(refin);
                                                        context.SaveChanges();
                                                    }
                                                }
                                                else
                                                {
                                                    IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                        x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo && x.sw == item.sw);
                                                    decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                    decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                    decimal ConcanTra = info.Sum(x => x.cantidad);
                                                    decimal? ConCosTra = info.Sum(x => x.costoTotal);
                                                    referencias_inven refin = new referencias_inven();

                                                    referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                        x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                        x.codigo == item.codigo);
                                                    if (existe != null)
                                                    {
                                                        existe.codigo = item.codigo;
                                                        existe.can_ent = ConCanEnt;
                                                        existe.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                        existe.can_tra = ConcanTra;
                                                        existe.cos_tra = Convert.ToDecimal(ConCosTra);
                                                        context.Entry(existe).State = EntityState.Modified;
                                                        context.SaveChanges();
                                                    }
                                                    else
                                                    {
                                                        refin.bodega = item.bodega;
                                                        refin.codigo = item.codigo;
                                                        refin.ano = (short)anio;
                                                        refin.mes = (short)mes;
                                                        refin.can_ent = ConCanEnt;
                                                        refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                        refin.can_tra = ConcanTra;
                                                        refin.cos_tra = Convert.ToDecimal(ConCosTra);
                                                        refin.modulo = item.modulo;
                                                        context.referencias_inven.Add(refin);
                                                        context.SaveChanges();
                                                    }
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 9

                                            if (item.sw == 9)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConCanOtrEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosOtrEnt = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_otr_ent += ConCanOtrEnt;
                                                    existe.cos_otr_ent += Convert.ToDecimal(ConCosOtrEnt);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = item.codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_otr_ent = ConCanOtrEnt;
                                                    refin.cos_otr_ent = Convert.ToDecimal(ConCosOtrEnt);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion

                                            #region Si SW = 10

                                            if (item.sw == 10)
                                            {
                                                IEnumerable<vw_recalculoInventario> info = consulta.Where(x =>
                                                    x.anio == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo && x.sw == item.sw);
                                                decimal ConCanEnt = info.Sum(x => x.cantidad);
                                                decimal? ConCosEnt = info.Sum(x => x.costoTotal);
                                                decimal ConCanOtrSal = info.Sum(x => x.cantidad);
                                                decimal? ConCosOtrSal = info.Sum(x => x.costoTotal);
                                                referencias_inven refin = new referencias_inven();

                                                referencias_inven existe = context.referencias_inven.FirstOrDefault(x =>
                                                    x.ano == anio && x.mes == mes && x.bodega == item.bodega &&
                                                    x.codigo == item.codigo);
                                                if (existe != null)
                                                {
                                                    existe.codigo = item.codigo;
                                                    existe.can_ent += ConCanEnt;
                                                    existe.cos_ent += Convert.ToDecimal(ConCosEnt);
                                                    existe.can_otr_sal += ConCanOtrSal;
                                                    existe.cos_otr_sal += Convert.ToDecimal(ConCosOtrSal);
                                                    context.Entry(existe).State = EntityState.Modified;
                                                    context.SaveChanges();
                                                }
                                                else
                                                {
                                                    refin.bodega = item.bodega;
                                                    refin.codigo = codigo;
                                                    refin.ano = (short)anio;
                                                    refin.mes = (short)mes;
                                                    refin.can_ent = ConCanEnt;
                                                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
                                                    refin.can_otr_sal = ConCanOtrSal;
                                                    refin.cos_otr_sal = Convert.ToDecimal(ConCosOtrSal);
                                                    refin.modulo = item.modulo;
                                                    context.referencias_inven.Add(refin);
                                                    context.SaveChanges();
                                                }
                                            }

                                            #endregion
                                        }

                                        #endregion

                                        #endregion
                                    }
                                }

                                #endregion

                                //se comento la siguiente region de codigo por que ahora debe seleccionar minimo una bodega para realizar el recalculo, por tal motivo siempre va a existir un filtro

                                #region Si no tiene filtros

                                /*
								if (codigo == "" && bodega.Count() > 0 && bodega[0] == null)
								{
								    #region Validaciones (1 → por el mes filtrado y  2 → por el sw que tenga la referencia)

								    #region Si el mes es enero (debo restar tambien el año)
								    if (mes == 1)
								    {
								        var anioAnterior = anio - 1;
								        var mesAnterior = 12;

								        #region Borrar mes anterior
								        if (borrado == false)
								        {
								            var borrarMes = context.referencias_inven.Where(x => x.ano == anio && x.mes == mes).ToList();
								            for (int j = 0; j < borrarMes.Count(); j++)
								            {
								                context.Entry(borrarMes[j]).State = EntityState.Deleted;
								                borrado = true;
								            }
								            context.SaveChanges();
								        }
								        #endregion

								        #region Consolido lo del mes anterior para recalcular el mes filtrado
								        if (consolidado == false)
								        {
								            var infoReferencias = context.referencias_inven.Where(x => x.ano == anioAnterior && x.mes == mesAnterior && (((x.can_ini + x.can_ent - x.can_sal) != 0) || ((x.cos_ini + x.cos_ent - x.cos_sal != 0)))).ToList();
								            foreach (var a in infoReferencias)
								            {
								                referencias_inven refin = new referencias_inven();
								                var existe = context.referencias_inven.FirstOrDefault(x => x.ano == anio && x.mes == mes && x.bodega == a.bodega && x.codigo == a.codigo);
								                if (existe != null)
								                {
								                    existe.can_ini += ((a.can_ini + a.can_ent) - a.can_sal);
								                    existe.cos_ini += ((a.cos_ini + a.cos_ent) - a.cos_sal);
								                    context.Entry(existe).State = EntityState.Modified;
								                    context.SaveChanges();
								                }
								                else
								                {
								                    refin.bodega = a.bodega;
								                    refin.codigo = a.codigo;
								                    refin.ano = (short)anio;
								                    refin.mes = (short)mes;
								                    refin.can_ini = ((a.can_ini + a.can_ent) - a.can_sal);
								                    refin.cos_ini = ((a.cos_ini + a.cos_ent) - a.cos_sal);
								                    refin.modulo = a.modulo;
								                    context.referencias_inven.Add(refin);
								                    context.SaveChanges();
								                }
								            }
								            consolidado = true;
								        }
								        #endregion

								        #region Si SW = 1
								        if (item.sw == 1)
								        {
								            referencias_inven refin = new referencias_inven();

								            var info = consulta;
								            var ConCanEnt = info.Sum(x => x.cantidad);
								            var ConCosEnt = info.Sum(x => x.costo_unitario);
								            var ConCanCom = info.Sum(x => x.cantidad);
								            var ConCosCom = info.Sum(x => x.costo_unitario);

								            var existe = context.referencias_inven.FirstOrDefault(x => x.ano == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo);
								            if (existe != null)
								            {
								                existe.can_ent += ConCanEnt;
								                existe.cos_ent += ConCosEnt;
								                existe.can_com += ConCanCom;
								                existe.cos_com += ConCosCom;
								                context.Entry(existe).State = EntityState.Modified;
								            }
								            else
								            {
								                refin.bodega = item.bodega;
								                refin.codigo = codigo;
								                refin.ano = (short)anio;
								                refin.mes = (short)mes;
								                refin.can_ent = ConCanEnt;
								                refin.cos_ent = ConCosEnt;
								                refin.can_com = ConCanCom;
								                refin.cos_com = ConCosCom;
								                refin.modulo = item.modulo;
								                context.referencias_inven.Add(refin);
								            }

								        }
								        #endregion
								        #region Si SW = 2
								        if (item.sw == 2)
								        {
								            var info = consulta;
								            var ConCanSal = info.Sum(x => x.cantidad);
								            var ConCosSal = info.Sum(x => x.costoTotal);
								            var ConcanDevCom = info.Sum(x => x.cantidad);
								            var ConCosDevCom = info.Sum(x => x.costoTotal);
								            referencias_inven refin = new referencias_inven();

								            var existe = context.referencias_inven.FirstOrDefault(x => x.ano == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo);
								            if (existe != null)
								            {
								                existe.can_sal += ConCanSal;
								                existe.cos_sal += Convert.ToDecimal(ConCosSal);
								                existe.can_dev_com += ConcanDevCom;
								                existe.cos_dev_com += Convert.ToDecimal(ConCosDevCom);
								                context.Entry(existe).State = EntityState.Modified;
								            }
								            else
								            {
								                refin.bodega = item.bodega;
								                refin.codigo = codigo;
								                refin.ano = (short)anio;
								                refin.mes = (short)mes;
								                refin.can_sal = ConCanSal;
								                refin.cos_sal = Convert.ToDecimal(ConCosSal);
								                refin.can_dev_com = ConcanDevCom;
								                refin.cos_dev_com = Convert.ToDecimal(ConCosDevCom);
								                refin.modulo = item.modulo;
								                context.referencias_inven.Add(refin);
								            }
								        }
								        #endregion
								        #region Si SW = 3
								        if (item.sw == 3)
								        {
								            var info = consulta;
								            var ConCanSal = info.Sum(x => x.cantidad);
								            var ConCosSal = info.Sum(x => x.costoTotal);
								            var ConcanVenta = info.Sum(x => x.cantidad);
								            var ConCosVenta = info.Sum(x => x.costoTotal);
								            var ConValVenta = info.Sum(x => x.valorTotal);
								            referencias_inven refin = new referencias_inven();

								            var existe = context.referencias_inven.FirstOrDefault(x => x.ano == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo);
								            if (existe != null)
								            {
								                existe.can_sal += ConCanSal;
								                existe.cos_sal += Convert.ToDecimal(ConCosSal);
								                existe.can_vta += ConcanVenta;
								                existe.cos_vta += Convert.ToDecimal(ConCosVenta);
								                existe.val_vta += Convert.ToDecimal(ConValVenta);
								                context.Entry(existe).State = EntityState.Modified;
								            }
								            else
								            {
								                refin.bodega = item.bodega;
								                refin.codigo = codigo;
								                refin.ano = (short)anio;
								                refin.mes = (short)mes;
								                refin.can_sal = ConCanSal;
								                refin.cos_sal = Convert.ToDecimal(ConCosSal);
								                refin.can_vta = ConcanVenta;
								                refin.cos_vta = Convert.ToDecimal(ConCosVenta);
								                refin.val_vta = Convert.ToDecimal(ConValVenta);
								                refin.modulo = item.modulo;
								                context.referencias_inven.Add(refin);
								            }
								        }
								        #endregion
								        #region Si SW = 4
								        if (item.sw == 4)
								        {
								            var info = consulta;
								            var ConCanEnt = info.Sum(x => x.cantidad);
								            var ConCosEnt = info.Sum(x => x.costo_unitario);
								            var ConcanDevVenta = info.Sum(x => x.cantidad);
								            var ConCosDevVenta = info.Sum(x => x.costo_unitario);
								            referencias_inven refin = new referencias_inven();

								            var existe = context.referencias_inven.FirstOrDefault(x => x.ano == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo);
								            if (existe != null)
								            {
								                existe.can_ent += ConCanEnt;
								                existe.cos_ent += ConCosEnt;
								                existe.can_dev_vta += ConcanDevVenta;
								                existe.cos_dev_vta += ConCosDevVenta;
								                context.Entry(existe).State = EntityState.Modified;
								            }
								            else
								            {
								                refin.bodega = item.bodega;
								                refin.codigo = codigo;
								                refin.ano = (short)anio;
								                refin.mes = (short)mes;
								                refin.can_ent = ConCanEnt;
								                refin.cos_ent = ConCosEnt;
								                refin.can_dev_vta = ConcanDevVenta;
								                refin.cos_dev_vta = ConCosDevVenta;
								                refin.modulo = item.modulo;
								                context.referencias_inven.Add(refin);
								            }
								        }
								        #endregion
								        #region Si SW = 8
								        if (item.sw == 8)
								        {
								            if (item.bodega == item.bodegaOrigen)
								            {
								                var info = consulta;
								                var ConCanSal = info.Sum(x => x.cantidad);
								                var ConCosSal = info.Sum(x => x.costoTotal);
								                var ConcanTra = info.Sum(x => x.cantidad);
								                var ConCosTra = info.Sum(x => x.costoTotal);
								                referencias_inven refin = new referencias_inven();

								                var existe = context.referencias_inven.FirstOrDefault(x => x.ano == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo);
								                if (existe != null)
								                {
								                    existe.can_sal = ConCanSal;
								                    existe.cos_sal = Convert.ToDecimal(ConCosSal);
								                    existe.can_vta = ConcanTra;
								                    existe.cos_vta = Convert.ToDecimal(ConCosTra);
								                    context.Entry(existe).State = EntityState.Modified;
								                }
								                else
								                {
								                    refin.bodega = item.bodega;
								                    refin.codigo = codigo;
								                    refin.ano = (short)anio;
								                    refin.mes = (short)mes;
								                    refin.can_sal = ConCanSal;
								                    refin.cos_sal = Convert.ToDecimal(ConCosSal);
								                    refin.can_vta = ConcanTra;
								                    refin.cos_vta = Convert.ToDecimal(ConCosTra);
								                    refin.modulo = item.modulo;
								                    context.referencias_inven.Add(refin);
								                }
								            }
								            else
								            {
								                var info = consulta.Where(x => x.bodega == item.bodega && x.codigo == item.codigo);
								                var ConCanEnt = info.Sum(x => x.cantidad);
								                var ConCosEnt = info.Sum(x => x.costoTotal);
								                var ConcanTra = info.Sum(x => x.cantidad);
								                var ConCosTra = info.Sum(x => x.costoTotal);
								                referencias_inven refin = new referencias_inven();
								                refin.bodega = item.bodega;
								                refin.codigo = codigo;
								                refin.ano = (short)anio;
								                refin.mes = (short)mes;
								                refin.can_ent = ConCanEnt;
								                refin.cos_ent = Convert.ToDecimal(ConCosEnt);
								                refin.can_vta = ConcanTra;
								                refin.cos_vta = Convert.ToDecimal(ConCosTra);
								                refin.modulo = item.modulo;
								                context.referencias_inven.Add(refin);
								            }

								        }
								        #endregion
								        #region Si SW = 9
								        if (item.sw == 9)
								        {
								            var info = consulta;
								            var ConCanEnt = info.Sum(x => x.cantidad);
								            var ConCosEnt = info.Sum(x => x.costo_unitario);
								            var ConCanOtrEnt = info.Sum(x => x.cantidad);
								            var ConCosOtrEnt = info.Sum(x => x.costo_unitario);
								            referencias_inven refin = new referencias_inven();

								            var existe = context.referencias_inven.FirstOrDefault(x => x.ano == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo);
								            if (existe != null)
								            {
								                existe.can_ent += ConCanEnt;
								                existe.cos_ent += ConCosEnt;
								                existe.can_otr_ent += ConCanOtrEnt;
								                existe.cos_otr_ent += ConCosOtrEnt;
								                context.Entry(existe).State = EntityState.Modified;
								            }
								            else
								            {
								                refin.bodega = item.bodega;
								                refin.codigo = codigo;
								                refin.ano = (short)anio;
								                refin.mes = (short)mes;
								                refin.can_ent = ConCanEnt;
								                refin.cos_ent = ConCosEnt;
								                refin.can_otr_ent = ConCanOtrEnt;
								                refin.cos_otr_ent = ConCosOtrEnt;
								                refin.modulo = item.modulo;
								                context.referencias_inven.Add(refin);
								            }
								        }
								        #endregion
								        #region Si SW = 10
								        if (item.sw == 10)
								        {
								            var info = consulta;
								            var ConCanEnt = info.Sum(x => x.cantidad);
								            var ConCosEnt = info.Sum(x => x.costo_unitario);
								            var ConCanOtrSal = info.Sum(x => x.cantidad);
								            var ConCosOtrSal = info.Sum(x => x.costo_unitario);
								            referencias_inven refin = new referencias_inven();

								            var existe = context.referencias_inven.FirstOrDefault(x => x.ano == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo);
								            if (existe != null)
								            {
								                existe.can_ent += ConCanEnt;
								                existe.cos_ent += ConCosEnt;
								                existe.can_otr_sal += ConCanOtrSal;
								                existe.cos_otr_sal += ConCosOtrSal;
								                context.Entry(existe).State = EntityState.Modified;
								            }
								            else
								            {
								                refin.bodega = item.bodega;
								                refin.codigo = codigo;
								                refin.ano = (short)anio;
								                refin.mes = (short)mes;
								                refin.can_ent = ConCanEnt;
								                refin.cos_ent = ConCosEnt;
								                refin.can_otr_sal = ConCanOtrSal;
								                refin.cos_otr_sal = ConCosOtrSal;
								                refin.modulo = item.modulo;
								                context.referencias_inven.Add(refin);
								            }
								        }
								        #endregion
								    }
								    #endregion
								    #region Si el mes es diferente a enero, solo resto 1 mes al mes filtrado
								    else
								    {
								        var mesAnterior = mes - 1;

								        #region Borrar mes anterior
								        if (borrado == false)
								        {
								            var borrarMes = context.referencias_inven.Where(x => x.ano == anio && x.mes == mes).ToList();
								            for (int j = 0; j < borrarMes.Count(); j++)
								            {
								                context.Entry(borrarMes[j]).State = EntityState.Deleted;
								                borrado = true;
								            }
								            context.SaveChanges();
								        }
								        #endregion

								        #region Consolido lo del mes anterior para recalcular el mes filtrado
								        if (consolidado == false)
								        {
								            var infoReferencias = context.referencias_inven.Where(x => x.ano == anio && x.mes == mesAnterior && (((x.can_ini + x.can_ent - x.can_sal) != 0) || ((x.cos_ini + x.cos_ent - x.cos_sal != 0)))).ToList();
								            foreach (var a in infoReferencias)
								            {
								                referencias_inven refin = new referencias_inven();
								                var existe = context.referencias_inven.FirstOrDefault(x => x.ano == anio && x.mes == mes && x.bodega == a.bodega && x.codigo == a.codigo);
								                if (existe != null)
								                {
								                    existe.can_ini += ((a.can_ini + a.can_ent) - a.can_sal);
								                    existe.cos_ini += ((a.cos_ini + a.cos_ent) - a.cos_sal);
								                    context.Entry(existe).State = EntityState.Modified;
								                    context.SaveChanges();
								                }
								                else
								                {
								                    refin.bodega = a.bodega;
								                    refin.codigo = a.codigo;
								                    refin.ano = (short)anio;
								                    refin.mes = (short)mes;
								                    refin.can_ini = ((a.can_ini + a.can_ent) - a.can_sal);
								                    refin.cos_ini = ((a.cos_ini + a.cos_ent) - a.cos_sal);
								                    refin.modulo = a.modulo;
								                    context.referencias_inven.Add(refin);
								                    context.SaveChanges();
								                }
								            }
								            consolidado = true;
								        }
								        #endregion

								        #region Si SW = 1
								        if (item.sw == 1)
								        {
								            var info = consulta.Where(x => x.anio == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo && x.sw == item.sw);

								            var ConCanEnt = info.Sum(x => x.cantidad);
								            var ConCosEnt = info.Sum(x => x.costoTotal);
								            var ConCanCom = info.Sum(x => x.cantidad);
								            var ConCosCom = info.Sum(x => x.costoTotal);
								            referencias_inven refin = new referencias_inven();

								            var existe = context.referencias_inven.FirstOrDefault(x => x.ano == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo);
								            if (existe != null)
								            {
								                existe.codigo = item.codigo;
								                existe.can_ent += ConCanEnt;
								                existe.cos_ent += Convert.ToDecimal(ConCosEnt);
								                existe.can_com += ConCanCom;
								                existe.cos_com += Convert.ToDecimal(ConCosCom);
								                context.Entry(existe).State = EntityState.Modified;
								                context.SaveChanges();
								            }
								            else
								            {
								                refin.bodega = item.bodega;
								                refin.codigo = item.codigo;
								                refin.ano = (short)anio;
								                refin.mes = (short)mes;
								                refin.can_ent = ConCanEnt;
								                refin.cos_ent = Convert.ToDecimal(ConCosEnt);
								                refin.can_com = ConCanCom;
								                refin.cos_com = Convert.ToDecimal(ConCosCom);
								                refin.modulo = item.modulo;
								                context.referencias_inven.Add(refin);
								                context.SaveChanges();
								            }
								        }
								        #endregion
								        #region Si SW = 2
								        if (item.sw == 2)
								        {
								            var info = consulta.Where(x => x.anio == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo && x.sw == item.sw);
								            var ConCanSal = info.Sum(x => x.cantidad);
								            var ConCosSal = info.Sum(x => x.costoTotal);
								            var ConcanDevCom = info.Sum(x => x.cantidad);
								            var ConCosDevCom = info.Sum(x => x.costoTotal);
								            referencias_inven refin = new referencias_inven();

								            var existe = context.referencias_inven.FirstOrDefault(x => x.ano == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo);
								            if (existe != null)
								            {
								                existe.codigo = item.codigo;
								                existe.can_sal += ConCanSal;
								                existe.cos_sal += Convert.ToDecimal(ConCosSal);
								                existe.can_dev_com += ConcanDevCom;
								                existe.cos_dev_com += Convert.ToDecimal(ConCosDevCom);
								                context.Entry(existe).State = EntityState.Modified;
								                context.SaveChanges();
								            }
								            else
								            {
								                refin.bodega = item.bodega;
								                refin.codigo = item.codigo;
								                refin.ano = (short)anio;
								                refin.mes = (short)mes;
								                refin.can_sal = ConCanSal;
								                refin.cos_sal = Convert.ToDecimal(ConCosSal);
								                refin.can_dev_com = ConcanDevCom;
								                refin.cos_dev_com = Convert.ToDecimal(ConCosDevCom);
								                refin.modulo = item.modulo;
								                context.referencias_inven.Add(refin);
								                context.SaveChanges();
								            }
								        }
								        #endregion
								        #region Si SW = 3
								        if (item.sw == 3)
								        {
								            var info = consulta.Where(x => x.anio == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo && x.sw == item.sw);
								            var ConCanSal = info.Sum(x => x.cantidad);
								            var ConCosSal = info.Sum(x => x.costoTotal);
								            var ConcanVenta = info.Sum(x => x.cantidad);
								            var ConCosVenta = info.Sum(x => x.costoTotal);
								            var ConValVenta = info.Sum(x => x.valorTotal);
								            referencias_inven refin = new referencias_inven();

								            var existe = context.referencias_inven.FirstOrDefault(x => x.ano == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo);
								            if (existe != null)
								            {
								                existe.codigo = item.codigo;
								                existe.can_sal += ConCanSal;
								                existe.cos_sal += Convert.ToDecimal(ConCosSal);
								                existe.can_vta += ConcanVenta;
								                existe.cos_vta += Convert.ToDecimal(ConCosVenta);
								                existe.val_vta += Convert.ToDecimal(ConValVenta);
								                context.Entry(existe).State = EntityState.Modified;
								                context.SaveChanges();
								            }
								            else
								            {
								                refin.bodega = item.bodega;
								                refin.codigo = item.codigo;
								                refin.ano = (short)anio;
								                refin.mes = (short)mes;
								                refin.can_sal = ConCanSal;
								                refin.cos_sal = Convert.ToDecimal(ConCosSal);
								                refin.can_vta = ConcanVenta;
								                refin.cos_vta = Convert.ToDecimal(ConCosVenta);
								                refin.val_vta = Convert.ToDecimal(ConValVenta);
								                refin.modulo = item.modulo;
								                context.referencias_inven.Add(refin);
								                context.SaveChanges();
								            }
								        }
								        #endregion
								        #region Si SW = 4
								        if (item.sw == 4)
								        {
								            var info = consulta.Where(x => x.anio == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo && x.sw == item.sw);
								            var ConCanEnt = info.Sum(x => x.cantidad);
								            var ConCosEnt = info.Sum(x => x.costoTotal);
								            var ConcanDevVenta = info.Sum(x => x.cantidad);
								            var ConCosDevVenta = info.Sum(x => x.costoTotal);
								            referencias_inven refin = new referencias_inven();

								            var existe = context.referencias_inven.FirstOrDefault(x => x.ano == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo);
								            if (existe != null)
								            {
								                existe.codigo = item.codigo;
								                existe.can_ent += ConCanEnt;
								                existe.cos_ent += Convert.ToDecimal(ConCosEnt);
								                existe.can_dev_vta += ConcanDevVenta;
								                existe.cos_dev_vta += Convert.ToDecimal(ConCosDevVenta);
								                context.Entry(existe).State = EntityState.Modified;
								                context.SaveChanges();
								            }
								            else
								            {
								                refin.bodega = item.bodega;
								                refin.codigo = item.codigo;
								                refin.ano = (short)anio;
								                refin.mes = (short)mes;
								                refin.can_ent = ConCanEnt;
								                refin.cos_ent = Convert.ToDecimal(ConCosEnt);
								                refin.can_dev_vta = ConcanDevVenta;
								                refin.cos_dev_vta = Convert.ToDecimal(ConCosDevVenta);
								                refin.modulo = item.modulo;
								                context.referencias_inven.Add(refin);
								                context.SaveChanges();
								            }
								        }
								        #endregion
								        #region Si SW = 8
								        if (item.sw == 8)
								        {
								            if (item.bodega == item.bodegaOrigen)
								            {
								                var info = consulta.Where(x => x.anio == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo && x.sw == item.sw);
								                var ConCanSal = info.Sum(x => x.cantidad);
								                var ConCosSal = info.Sum(x => x.costoTotal);
								                var ConcanTra = info.Sum(x => x.cantidad);
								                var ConCosTra = info.Sum(x => x.costoTotal);
								                referencias_inven refin = new referencias_inven();

								                var existe = context.referencias_inven.FirstOrDefault(x => x.ano == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo);
								                if (existe != null)
								                {
								                    existe.codigo = item.codigo;
								                    existe.can_sal += ConCanSal;
								                    existe.cos_sal += Convert.ToDecimal(ConCosSal);
								                    existe.can_tra += ConcanTra;
								                    existe.cos_tra += Convert.ToDecimal(ConCosTra);
								                    context.Entry(existe).State = EntityState.Modified;
								                    context.SaveChanges();
								                }
								                else
								                {
								                    refin.bodega = item.bodega;
								                    refin.codigo = item.codigo;
								                    refin.ano = (short)anio;
								                    refin.mes = (short)mes;
								                    refin.can_sal = ConCanSal;
								                    refin.cos_sal = Convert.ToDecimal(ConCosSal);
								                    refin.can_tra = ConcanTra;
								                    refin.cos_tra = Convert.ToDecimal(ConCosTra);
								                    refin.modulo = item.modulo;
								                    context.referencias_inven.Add(refin);
								                    context.SaveChanges();
								                }
								            }
								            else
								            {
								                var info = consulta.Where(x => x.anio == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo && x.sw == item.sw);
								                var ConCanEnt = info.Sum(x => x.cantidad);
								                var ConCosEnt = info.Sum(x => x.costoTotal);
								                var ConcanTra = info.Sum(x => x.cantidad);
								                var ConCosTra = info.Sum(x => x.costoTotal);
								                referencias_inven refin = new referencias_inven();

								                var existe = context.referencias_inven.FirstOrDefault(x => x.ano == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo);
								                if (existe != null)
								                {
								                    existe.codigo = item.codigo;
								                    existe.can_ent = ConCanEnt;
								                    existe.cos_ent = Convert.ToDecimal(ConCosEnt);
								                    existe.can_tra = ConcanTra;
								                    existe.cos_tra = Convert.ToDecimal(ConCosTra);
								                    context.Entry(existe).State = EntityState.Modified;
								                    context.SaveChanges();
								                }
								                else
								                {
								                    refin.bodega = item.bodega;
								                    refin.codigo = item.codigo;
								                    refin.ano = (short)anio;
								                    refin.mes = (short)mes;
								                    refin.can_ent = ConCanEnt;
								                    refin.cos_ent = Convert.ToDecimal(ConCosEnt);
								                    refin.can_tra = ConcanTra;
								                    refin.cos_tra = Convert.ToDecimal(ConCosTra);
								                    refin.modulo = item.modulo;
								                    context.referencias_inven.Add(refin);
								                    context.SaveChanges();
								                }
								            }

								        }
								        #endregion
								        #region Si SW = 9
								        if (item.sw == 9)
								        {
								            var info = consulta.Where(x => x.anio == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo && x.sw == item.sw);
								            var ConCanEnt = info.Sum(x => x.cantidad);
								            var ConCosEnt = info.Sum(x => x.costoTotal);
								            var ConCanOtrEnt = info.Sum(x => x.cantidad);
								            var ConCosOtrEnt = info.Sum(x => x.costoTotal);
								            referencias_inven refin = new referencias_inven();

								            var existe = context.referencias_inven.FirstOrDefault(x => x.ano == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo);
								            if (existe != null)
								            {
								                existe.codigo = item.codigo;
								                existe.can_ent += ConCanEnt;
								                existe.cos_ent += Convert.ToDecimal(ConCosEnt);
								                existe.can_otr_ent += ConCanOtrEnt;
								                existe.cos_otr_ent += Convert.ToDecimal(ConCosOtrEnt);
								                context.Entry(existe).State = EntityState.Modified;
								                context.SaveChanges();
								            }
								            else
								            {
								                refin.bodega = item.bodega;
								                refin.codigo = item.codigo;
								                refin.ano = (short)anio;
								                refin.mes = (short)mes;
								                refin.can_ent = ConCanEnt;
								                refin.cos_ent = Convert.ToDecimal(ConCosEnt);
								                refin.can_otr_ent = ConCanOtrEnt;
								                refin.cos_otr_ent = Convert.ToDecimal(ConCosOtrEnt);
								                refin.modulo = item.modulo;
								                context.referencias_inven.Add(refin);
								                context.SaveChanges();
								            }
								        }
								        #endregion
								        #region Si SW = 10
								        if (item.sw == 10)
								        {
								            var info = consulta.Where(x => x.anio == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo && x.sw == item.sw);
								            var ConCanEnt = info.Sum(x => x.cantidad);
								            var ConCosEnt = info.Sum(x => x.costoTotal);
								            var ConCanOtrSal = info.Sum(x => x.cantidad);
								            var ConCosOtrSal = info.Sum(x => x.costoTotal);
								            referencias_inven refin = new referencias_inven();

								            var existe = context.referencias_inven.FirstOrDefault(x => x.ano == anio && x.mes == mes && x.bodega == item.bodega && x.codigo == item.codigo);
								            if (existe != null)
								            {
								                existe.codigo = item.codigo;
								                existe.can_ent += ConCanEnt;
								                existe.cos_ent += Convert.ToDecimal(ConCosEnt);
								                existe.can_otr_sal += ConCanOtrSal;
								                existe.cos_otr_sal += Convert.ToDecimal(ConCosOtrSal);
								                context.Entry(existe).State = EntityState.Modified;
								                context.SaveChanges();
								            }
								            else
								            {
								                refin.bodega = item.bodega;
								                refin.codigo = codigo;
								                refin.ano = (short)anio;
								                refin.mes = (short)mes;
								                refin.can_ent = ConCanEnt;
								                refin.cos_ent = Convert.ToDecimal(ConCosEnt);
								                refin.can_otr_sal = ConCanOtrSal;
								                refin.cos_otr_sal = Convert.ToDecimal(ConCosOtrSal);
								                refin.modulo = item.modulo;
								                context.referencias_inven.Add(refin);
								                context.SaveChanges();
								            }
								        }
								        #endregion
								    }
								    #endregion

								    #endregion
								}*/

                                #endregion

                                listaReferencias.Add(new listaRecalculo
                                {
                                    codigoReferencia = item.codigo,
                                    anio = Convert.ToInt32(item.anio),
                                    mes = Convert.ToInt32(item.mes),
                                    sw = item.sw,
                                    bodega = item.bodega
                                });
                            }

                            dbTran.Commit();
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