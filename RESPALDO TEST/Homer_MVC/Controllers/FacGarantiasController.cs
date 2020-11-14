using Homer_MVC.IcebergModel;
using Homer_MVC.ViewModels.medios;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class FacGarantiasController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");


        // GET: FacGarantias
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetPPrefac() {


            var datos = context.vw_Prefacgarantias.Select(a => new
                {
                a.idencabezado,
                a.dias,
                a.id_transaccion,
                a.bodega,
                a.num_prefac2,
                a.ot,
                a.valor_total,
                a.valor_pagado,
                estado_pre =  a.estado_pre != null ? a.estado_pre: "",
                a.seguimientos,
                a.facturagm,
                a.vlmanoobra,
                a.vlpagadomanoobra,
                a.vltot,
                a.vlpagadotot,
                a.vlrepuestos,
                a.vlpagadorepuestos,
                a.vltotal
                }).ToList();
            return Json(datos, JsonRequestBehavior.AllowGet);
            }



        public JsonResult BuscarAdmonGarantiasFiltro(string numprefac, string ot, DateTime fechaini, DateTime fechafin)
            {

            Expression<Func<vw_Prefacgarantias, bool>> predicado = PredicateBuilder.True<vw_Prefacgarantias>();
            Expression<Func<vw_Prefacgarantias, bool>> predicado2 = PredicateBuilder.False<vw_Prefacgarantias>();

            if (!string.IsNullOrWhiteSpace(numprefac))
                {
                predicado2 = predicado2.Or(e => e.num_prefac2 == numprefac);

                predicado = predicado.And(predicado2);
                }
          if (!string.IsNullOrWhiteSpace(ot))
                {
                predicado2 = predicado2.Or(e => e.ot == ot);
                predicado = predicado.And(predicado2);
                }


            if (fechaini != null)
                {
                predicado = predicado.And(d => d.fec_creacion >= fechaini);
                }
            if (fechafin != null)
                {
                predicado = predicado.And(d => d.fec_creacion <= fechafin);
                }




            var datos = context.vw_Prefacgarantias.Where(predicado).Select(a => new
                {
                a.idencabezado,
                a.dias,
                a.id_transaccion,
                a.bodega,
                a.num_prefac2,
                a.ot,
                a.valor_total,
                a.valor_pagado,
                estado_pre = a.estado_pre != null ? a.estado_pre : "",
                a.seguimientos,
                a.facturagm,
                a.vlmanoobra,
                a.vlpagadomanoobra,
                a.vltot,
                a.vlpagadotot,
                a.vlrepuestos,
                a.vlpagadorepuestos,
                a.vltotal
                }).ToList();

            return Json(datos, JsonRequestBehavior.AllowGet);
            }

        }
    }