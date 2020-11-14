using Homer_MVC.IcebergModel;
using Homer_MVC.ViewModels.contabpdf;
using Homer_MVC.ViewModels.medios;
using Rotativa;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class generacionPDFcontabilidadController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();
        private readonly ViewModelPDFcontable modelopdf = new ViewModelPDFcontable();

        // GET: generacionPDFcontabilidad

        public ActionResult Index()
        {
            ViewModelPDFcontable modelopdf = new ViewModelPDFcontable
            {
                vm_icb_terceros = new icb_terceros(),
                vm_list_icb_terceros = new List<icb_terceros>(),
                vm_cuenta_pub = new cuenta_puc(),
                vm_list_cuenta_pub = new List<cuenta_puc>(),
                vm_centro_costo = new centro_costo(),
                vm_list_centro_costo = new List<centro_costo>(),
                vm_cuentas_valores = new cuentas_valores(),
                vm_list_cuentas_valores = new List<cuentas_valores>(),
                vm_mov_contable = new mov_contable(),


                vm_list_mov_contable = new List<mov_contable>()
            };

            var cuentas = (from cuenta in db.cuenta_puc //modelopdf.vm_list_cuenta_pub
                           orderby cuenta.cntpuc_numero
                           select new
                           {
                               id = cuenta.cntpuc_id,
                               numero = cuenta.cntpuc_numero,
                               nombre = cuenta.cntpuc_numero + " (" + cuenta.cntpuc_descp + ")"
                           }).ToList().OrderBy(x => x.numero);
            ViewBag.txtCuentaInicio = new SelectList(cuentas, "numero", "nombre");
            ViewBag.txtCuentaFin = new SelectList(cuentas, "numero", "nombre");

            var buscarCentrosCosto = (from centro in db.centro_costo //modelopdf.vm_list_centro_costo
                                      orderby centro.pre_centcst
                                      select new
                                      {
                                          id = centro.centcst_id,
                                          numero = "(" + centro.pre_centcst + ") " + centro.centcst_nombre
                                      }).ToList();
            ViewBag.txtCentrosCosto = new SelectList(buscarCentrosCosto, "id", "numero");


            var buscarNits = (from terceros in db.icb_terceros //modelopdf.vm_list_icb_terceros
                              select new
                              {
                                  terceros.tercero_id,
                                  nombre = "(" + terceros.doc_tercero + ") " + terceros.prinom_tercero + " " +
                                           terceros.segnom_tercero + " " + terceros.apellido_tercero + " " +
                                           terceros.segapellido_tercero + " " + terceros.razon_social
                              }).ToList();
            ViewBag.txtNit = new SelectList(buscarNits, "tercero_id", "nombre");

            var Wtipos = (from tip in db.tp_doc_registros
                          select new
                          {
                              id = tip.tpdoc_id,
                              nombre = "(" + tip.prefijo + ") " + tip.tpdoc_nombre
                          }).ToList().OrderBy(x => x.id);
            ViewBag.WtipoDocu = new SelectList(Wtipos, "id", "nombre");

            return View();
        }

        public ActionResult repcontablepdfmov(int? anioini,
            int? mesini,
            int? aniofin,
            int? mesfin,
            int? cuentaini,
            int? cuentafin,
            int? idtercero,
            string centros,
            string tipos_doc,
            bool checkCentro,
            bool checkNit,
            bool checkMovimiento,
            bool checkNiff,
            int? mes,
            int? anio)
        {
            int? wanioini = anioini;
            int? wmesini = mesini;
            int? waniofin = aniofin;
            int? wmesfin = mesfin;
            int? wcuentaini = cuentaini;
            int? wcuentafin = cuentafin;
            int? widtercero = idtercero;
            int? wanio = anio;
            int? wmes = mes;

            string[] Vectorcentros = centros.Split(',');
            string[] VectorTipos = tipos_doc.Split(',');

            #region Declaracion de PredicateBuilder2 Pdf

            //Declaramos el predicado de tipo And con el parámetro True. Si fuera de tipo Or lo declaramos con el parámetro False
            System.Linq.Expressions.Expression<Func<vw_mov_contables, bool>> predicateMovAnd = PredicateBuilder.True<vw_mov_contables>();
            System.Linq.Expressions.Expression<Func<vw_mov_contables, bool>> predicateMovOr = PredicateBuilder.False<vw_mov_contables>();

            System.Linq.Expressions.Expression<Func<vw_mov_contables, bool>> predicate = PredicateBuilder.True<vw_mov_contables>();
            System.Linq.Expressions.Expression<Func<vw_mov_contables, bool>> predicate2 = PredicateBuilder.False<vw_mov_contables>();

            #endregion

            #region Criterios aplicados a los PredicateBuilder Pdf

            #region año mes

            if (wanio != null)
            {
                //predicate = predicate.And(x => x.ano == wanio);
                predicateMovAnd = predicateMovAnd.And(x => x.ano == wanio);
            }

            if (wmes != null)
            {
                //predicate = predicate.And(x => x.mes == wmes);
                predicateMovAnd = predicateMovAnd.And(x => x.mes == wmes);
            }
            // predicateMes = predicateMes.Or(x => x.mes >= wmesini && x.mes <= wmesfin);

            if (wanioini != null && waniofin != null)
            {
                // predicate = predicate.And(x => x.ano >= wanioini && x.ano <= waniofin);
            }

            if (wmesini != null && wmesfin != null)
            {
                //predicate = predicate.And(x => x.mes >= wmesini && x.mes <= wmesfin);
                // predicateMes = predicateMes.Or(x => x.mes >= wmesini && x.mes <= wmesfin);
            }

            #endregion

            //    predicate = predicate.And(predicateMes);
            //  var Aux_PrediMes = db.vw_cuentas_valores.Where(predicate).ToList();
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
                    //  predicate = predicate.And(x => x.clase >= ci11 && x.clase <= cf11   /* && x.cntpuc_numero <= wcuentaini*/ );
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci11 && x.clase <= cf11 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = "-11-";
                }

                if (Convert.ToString(wcuentaini).Length == 1 && Convert.ToString(wcuentafin).Length == 2)
                {
                    int ci12 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf12 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gf12 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));


                    string numi12 = Convert.ToString(wcuentaini);
                    string numf12 = Convert.ToString(wcuentafin);
                    predicate = predicate.And(
                        x => x.clase >= ci12 && x.clase <= cf12 &&
                             x.grupo <= gf12 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci12 && x.clase <= cf12 &&
                                 x.grupo <= gf12 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-12-";
                }

                if (Convert.ToString(wcuentaini).Length == 1 && Convert.ToString(wcuentafin).Length == 3)
                {
                    int ci13 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf13 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gf13 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));


                    string numi13 = Convert.ToString(wcuentaini);
                    string numf13 = Convert.ToString(wcuentafin);
                    predicate = predicate.And(
                        x => x.clase >= ci13 && x.clase <= cf13 &&
                             x.grupo <= gf13 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci13 && x.clase <= cf13 &&
                                 x.grupo <= gf13 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci14 && x.clase <= cf14 && x.grupo <= gf14 &&
                             x.xcuenta <= xcf14 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci14 && x.clase <= cf14 && x.grupo <= gf14 &&
                                 x.xcuenta <= xcf14 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci15 && x.clase <= cf15 && x.grupo <= gf15 &&
                             x.xcuenta <= xcf15 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci15 && x.clase <= cf15 && x.grupo <= gf15 &&
                                 x.xcuenta <= xcf15 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci16 && x.clase <= cf16 && x.grupo <= gf16 && x.xcuenta <= xcf16 &&
                             x.subcuenta <= scf16 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci16 && x.clase <= cf16 && x.grupo <= gf16 && x.xcuenta <= xcf16 &&
                                 x.subcuenta <= scf16 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-1>5-";
                }

                if (Convert.ToString(wcuentaini).Length == 1 && Convert.ToString(wcuentafin).Length > 6)
                {
                    #region Predicate cuentas

                    string desde11 = Convert.ToString(wcuentaini);
                    string hasta11 = Convert.ToString(wcuentafin);
                    List<string> rango11 = (from p in db.vw_mov_contables
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
                    predicate = predicate.And(
                        x => x.clase >= ci22 && x.clase <= cf22 && x.grupo >= gi22 &&
                             x.grupo <= gf22 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci22 && x.clase <= cf22 && x.grupo >= gi22 &&
                                 x.grupo <= gf22 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci23 && x.clase <= cf23 && x.grupo >= gi23 &&
                             x.grupo <= gf23 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci23 && x.clase <= cf23 && x.grupo >= gi23 &&
                                 x.grupo <= gf23 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci24 && x.clase <= cf24 && x.grupo >= gi24 && x.grupo <= gf24 &&
                             x.xcuenta <= xcf24 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci24 && x.clase <= cf24 && x.grupo >= gi24 && x.grupo <= gf24 &&
                                 x.xcuenta <= xcf24 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci25 && x.clase <= cf25 && x.grupo >= gi25 && x.grupo <= gf25 &&
                             x.xcuenta <= xcf25 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci25 && x.clase <= cf25 && x.grupo >= gi25 && x.grupo <= gf25 &&
                                 x.xcuenta <= xcf25 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci26 && x.clase <= cf26 && x.grupo >= gi26 && x.grupo <= gf26 &&
                             x.xcuenta <= xcf26 && x.subcuenta <= scf26 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci26 && x.clase <= cf26 && x.grupo >= gi26 && x.grupo <= gf26 &&
                                 x.xcuenta <= xcf26 && x.subcuenta <= scf26 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-2>5-";
                }

                if (Convert.ToString(wcuentaini).Length == 2 && Convert.ToString(wcuentafin).Length > 6)
                {
                    #region Predicate cuentas

                    string desde22 = Convert.ToString(wcuentaini);
                    string hasta22 = Convert.ToString(wcuentafin);
                    List<string> rango22 = (from p in db.vw_mov_contables
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
                    predicate = predicate.And(
                        x => x.clase >= ci33 && x.clase <= cf33 && x.grupo >= gi33 &&
                             x.grupo <= gf33 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci33 && x.clase <= cf33 && x.grupo >= gi33 &&
                                 x.grupo <= gf33 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci34 && x.clase <= cf34 && x.grupo >= gi34 && x.grupo <= gf34 &&
                             x.xcuenta <= xcf34 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci34 && x.clase <= cf34 && x.grupo >= gi34 && x.grupo <= gf34 &&
                                 x.xcuenta <= xcf34 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci35 && x.clase <= cf35 && x.grupo >= gi35 && x.grupo <= gf35 &&
                             x.xcuenta <= xcf35 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci35 && x.clase <= cf35 && x.grupo >= gi35 && x.grupo <= gf35 &&
                                 x.xcuenta <= xcf35 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci36 && x.clase <= cf36 && x.grupo >= gi36 && x.grupo <= gf36 &&
                             x.xcuenta <= xcf36 && x.subcuenta <= scf36 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci36 && x.clase <= cf36 && x.grupo >= gi36 && x.grupo <= gf36 &&
                                 x.xcuenta <= xcf36 && x.subcuenta <= scf36 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-3>5-";
                }

                if (Convert.ToString(wcuentaini).Length == 3 && Convert.ToString(wcuentafin).Length > 6)
                {
                    #region Predicate cuentas

                    string desde33 = Convert.ToString(wcuentaini);
                    string hasta33 = Convert.ToString(wcuentafin);
                    List<string> rango33 = (from p in db.vw_mov_contables
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
                    predicate = predicate.And(
                        x => x.clase >= ci44 && x.clase <= cf44 && x.grupo >= gi44 && x.grupo <= gf44 &&
                             x.xcuenta >= xci44 && x.xcuenta <= xcf44 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci44 && x.clase <= cf44 && x.grupo >= gi44 && x.grupo <= gf44 &&
                                 x.xcuenta >= xci44 && x.xcuenta <= xcf44 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci45 && x.clase <= cf45 && x.grupo >= gi45 && x.grupo <= gf45 &&
                             x.xcuenta >= xci45 && x.xcuenta <= xcf45 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci45 && x.clase <= cf45 && x.grupo >= gi45 && x.grupo <= gf45 &&
                                 x.xcuenta >= xci45 && x.xcuenta <= xcf45 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci46 && x.clase <= cf46 && x.grupo >= gi46 && x.grupo <= gf46 &&
                             x.xcuenta >= xci46 && x.xcuenta <= xcf46 &&
                             x.subcuenta <= scf46 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci46 && x.clase <= cf46 && x.grupo >= gi46 && x.grupo <= gf46 &&
                                 x.xcuenta >= xci46 && x.xcuenta <= xcf46 &&
                                 x.subcuenta <= scf46 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-4>5-";
                }

                if (Convert.ToString(wcuentaini).Length == 4 && Convert.ToString(wcuentafin).Length > 6)
                {
                    #region Predicate cuentas

                    string desde44 = Convert.ToString(wcuentaini);
                    string hasta44 = Convert.ToString(wcuentafin);
                    List<string> rango44 = (from p in db.vw_mov_contables
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
                    predicate = predicate.And(
                        x => x.clase >= ci55 && x.clase <= cf55 && x.grupo >= gi55 && x.grupo <= gf55 &&
                             x.xcuenta >= xci55 && x.xcuenta <= xcf55 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci55 && x.clase <= cf55 && x.grupo >= gi55 && x.grupo <= gf55 &&
                                 x.xcuenta >= xci55 && x.xcuenta <= xcf55 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci56 && x.clase <= cf56 && x.grupo >= gi56 && x.grupo <= gf56 &&
                             x.xcuenta >= xci56 && x.xcuenta <= xcf56 && x.subcuenta >= sci56 &&
                             x.subcuenta <= scf56 /*&& x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd = predicateMovAnd.And(
                        x => x.clase >= ci56 && x.clase <= cf56 && x.grupo >= gi56 && x.grupo <= gf56 &&
                             x.xcuenta >= xci56 && x.xcuenta <= xcf56 && x.subcuenta >= sci56 &&
                             x.subcuenta <= scf56 /*&& x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-5>5-";
                }

                if (Convert.ToString(wcuentaini).Length == 5 && Convert.ToString(wcuentafin).Length > 6)
                {
                    #region Predicate cuentas

                    string desde55 = Convert.ToString(wcuentaini);
                    string hasta55 = Convert.ToString(wcuentafin);
                    List<string> rango55 = (from p in db.vw_mov_contables
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
                    predicate = predicate.And(
                        x => x.clase >= ci66 && x.clase <= cf66 && x.grupo >= gi66 && x.grupo <= gf66 &&
                             x.xcuenta >= xci66 && x.xcuenta <= xcf66 && x.subcuenta >= sci66 &&
                             x.subcuenta <= scf66 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd = predicateMovAnd.And(
                        x => x.clase >= ci66 && x.clase <= cf66 && x.grupo >= gi66 && x.grupo <= gf66 &&
                             x.xcuenta >= xci66 && x.xcuenta <= xcf66 && x.subcuenta >= sci66 &&
                             x.subcuenta <= scf66 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-6-6-";
                }

                if (Convert.ToString(wcuentaini).Length >= 6 && Convert.ToString(wcuentafin).Length > 6)
                {
                    #region Predicate cuentas

                    string desde66 = Convert.ToString(wcuentaini);
                    string hasta66 = Convert.ToString(wcuentafin);
                    List<string> rango66 = (from p in db.vw_mov_contables
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
                predicate = predicate.And(x => x.nit == widtercero);
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
                        predicateMovOr = predicateMovOr.Or(x => x.td_num == tip);
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

            System.Linq.Expressions.Expression<Func<vw_mov_contables, bool>> anioP = PredicateBuilder.True<vw_mov_contables>();
            System.Linq.Expressions.Expression<Func<vw_mov_contables, bool>> nitP = PredicateBuilder.False<vw_mov_contables>();
            System.Linq.Expressions.Expression<Func<vw_mov_contables, bool>> centroP = PredicateBuilder.False<vw_mov_contables>();
            System.Linq.Expressions.Expression<Func<vw_mov_contables, bool>> cuentaP = PredicateBuilder.False<vw_mov_contables>();

            anioP = anioP.And(x => x.fec.Year == anio);
            anioP = anioP.And(x => x.fec.Month == mes);

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

            ViewBag.checkMovimiento = checkMovimiento;
            ViewBag.checkNiff = checkNiff;
            ViewBag.checkCentro = checkCentro;
            ViewBag.checkNit = checkNit;

            ViewAsPdf something = new ViewAsPdf("repcontablepdfmov", "");

            List<vw_mov_contables> consulta = db.vw_mov_contables.Where(anioP).ToList();

            predicateMovAnd = predicateMovAnd.And(x => x.debito + x.credito + x.debitoniif + x.creditoniif != 0);
            predicateMovAnd = predicateMovAnd.And(x => x.clase > 0);

            if (checkNiff == false)
            {
                predicateMovAnd = predicateMovAnd.And(x => x.debito + x.credito != 0);
            }

            if (checkNiff)
            {
                predicateMovAnd = predicateMovAnd.And(x => x.debitoniif + x.creditoniif != 0);
            }

            List<vw_mov_contables> consulta_mov = db.vw_mov_contables.Where(predicateMovAnd).ToList();

            string root = Server.MapPath("~/Pdf/");
            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
            string path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);

            //// ******** para Movimientos Inicio **********

            #region select de Mov

            ///*****  MOVIMIENTOS AGRUPADOS - INICIO
            List<listaclasemov> seleccion_de_Mov_01 = consulta_mov.GroupBy(lc1 => new { lc1.clase }).Select(lc1 => new listaclasemov
            {
                clase = lc1.Key.clase ?? 0,
                nom_clase = lc1.Select(f => f.nom_clase).First(),
                //  Ctotalsaldoini = lc1.Sum(f => f.saldo_ini),
                Ctotaldebito = lc1.Sum(f => f.debito),
                Ctotalcredito = lc1.Sum(f => f.credito),
                Ctotalsaldo = lc1.Sum(f => f.debito - f.credito),
                //  Ctotalsaldoininiff = lc1.Sum(f => f.saldo_ininiff),
                Ctotaldebitoniff = lc1.Sum(f => f.debitoniif),
                Ctotalcreditoniff = lc1.Sum(f => f.creditoniif),
                Ctotalsaldoniff = lc1.Sum(f => f.debitoniif - f.creditoniif),
                agrupa_grupomov = lc1.GroupBy(ag => new { ag.grupo }).Select(ag => new agrupa_grupomov
                {
                    grupo = ag.Key.grupo ?? 0,
                    nom_grupo = ag.Select(g => g.nom_grupo).First(),
                    //  grupo_t_saldoini = ag.Sum(f => f.saldo_ini),
                    grupo_t_debito = ag.Sum(f => f.debito),
                    grupo_t_credito = ag.Sum(f => f.credito),
                    grupo_t_saldo = ag.Sum(f => f.debito - f.credito),
                    //    grupo_t_saldoininiff = ag.Sum(f => f.saldo_ininiff),
                    grupo_t_debitoniff = ag.Sum(f => f.debitoniif),
                    grupo_t_creditoniff = ag.Sum(f => f.creditoniif),
                    grupo_t_saldoniff = ag.Sum(f => f.debitoniif - f.creditoniif),
                    agrupa_cuentamov = ag.GroupBy(ac => new { ac.xcuenta }).Select(ac => new agrupa_cuentamov
                    {
                        cuenta = ac.Key.xcuenta ?? 0,
                        nom_cuenta = ac.Select(g => g.nom_xcuenta).First(),
                        //cuenta_t_saldoini = ac.Sum(f => f.saldo_ini),
                        cuenta_t_debito = ac.Sum(f => f.debito),
                        cuenta_t_credito = ac.Sum(f => f.credito),
                        cuenta_t_saldo = ac.Sum(f => f.debito - f.credito),
                        //  cuenta_t_saldoininiff = ac.Sum(f => f.saldo_ininiff),
                        cuenta_t_debitoniff = ac.Sum(f => f.debitoniif),
                        cuenta_t_creditoniff = ac.Sum(f => f.creditoniif),
                        cuenta_t_saldoniff = ac.Sum(f => f.debitoniif - f.creditoniif),

                        agrupa_subcuentamov = ac.GroupBy(asc => new { asc.subcuenta }).Select(asc =>
                              new agrupa_subcuentamov
                              {
                                  subcuenta = asc.Key.subcuenta ?? 0,
                                  nom_subcuenta = asc.Select(g => g.nom_subcuenta).First(),
                                  // subcuenta_t_saldoini = asc.Sum(f => f.saldo_ini),
                                  subcuenta_t_debito = asc.Sum(f => f.debito),
                                  subcuenta_t_credito = asc.Sum(f => f.credito),
                                  subcuenta_t_saldo = asc.Sum(f => f.debito - f.credito),
                                  // subcuenta_t_saldoininiff = asc.Sum(f => f.saldo_ininiff),
                                  subcuenta_t_debitoniff = asc.Sum(f => f.debitoniif),
                                  subcuenta_t_creditoniff = asc.Sum(f => f.creditoniif),
                                  subcuenta_t_saldoniff = asc.Sum(f => f.debitoniif - f.creditoniif),
                                  detalleCuentamov = asc.GroupBy(dc => new { dc.cuenta }).Select(dc => new detalleCuentamov
                                  {
                                      id_cuenta_det = dc.Key.cuenta,
                                      numero_cuenta_det = dc.Select(g => g.cntpuc_numero).First(),
                                      //clase_cuenta_det = dc.Select(g => g.clase ?? 0).First(),
                                      //grupo_cuenta_det = dc.Select(g => g.grupo ?? 0).First(),
                                      //xcuenta_cuenta_det = dc.Select(g => g.xcuenta ?? 0).First(),
                                      //subcuenta_cuenta_det = dc.Select(g => g.subcuenta ?? 0).First(),
                                      nombre_cuenta_det = dc.Select(g => g.cntpuc_descp).First(),
                                      // saldoini_det = dc.Sum(fc => fc.saldo_ini),
                                      debito_det = dc.Sum(fc => fc.debito),
                                      credito_det = dc.Sum(fc => fc.credito),
                                      saldo_det = dc.Sum(f => f.debito - f.credito),
                                      // saldoininiff_det = dc.Sum(fc => fc.saldo_ininiff),
                                      debitoniff_det = dc.Sum(fc => fc.debitoniif),
                                      creditoniff_det = dc.Sum(fc => fc.creditoniif),
                                      saldoniff_det = dc.Sum(f => f.debitoniif - f.creditoniif),
                                      listacentromov = dc.GroupBy(lc => new { lc.centro }).Select(lc => new listacentromov
                                      {
                                          id_centro = lc.Key.centro,
                                          codigo_centro = lc.Select(f => f.pre_centcst).First(),
                                          nombre_centro = lc.Select(f => f.centcst_nombre).First(),
                                          // scentro_t_saldoini = lc.Sum(f => f.saldo_ini),
                                          scentro_t_debito = lc.Sum(f => f.debito),
                                          scentro_t_credito = lc.Sum(f => f.credito),
                                          scentro_t_saldo = lc.Sum(f => f.debito - f.credito),
                                          //  scentro_t_saldoininiff = lc.Sum(f => f.saldo_ininiff),
                                          scentro_t_debitoniff = lc.Sum(f => f.debitoniif),
                                          scentro_t_creditoniff = lc.Sum(f => f.creditoniif),
                                          scentro_t_saldoniff = lc.Sum(f => f.debitoniif - f.creditoniif),
                                          lista_mov = lc.GroupBy(lm => new { lm.numero }).Select(lm => new lista_mov
                                          {
                                              doc = lm.Key.numero ?? 0,
                                              prefijo = lm.Select(f => f.prefijo).First(),
                                              nom_docuento = lm.Select(f => f.tpdoc_nombre).First(),
                                              debito_mov = lm.Sum(fc => fc.debito),
                                              credito_mov = lm.Sum(fc => fc.credito),
                                              saldo_mov = lm.Sum(fc => fc.debito - fc.credito),
                                              debitoniff_mov = lm.Sum(fc => fc.debitoniif),
                                              creditoniff_mov = lm.Sum(fc => fc.creditoniif),
                                              saldoniff_mov = lm.Sum(fc => fc.debitoniif - fc.creditoniif)
                                          }).OrderBy(c => c.doc).ToList()
                                      }).OrderBy(c => c.id_centro).ToList()
                                  }).OrderBy(c => c.id_cuenta_det).ToList()
                              }).OrderBy(c => c.subcuenta).ToList()
                    }).OrderBy(c => c.cuenta).ToList()
                }).OrderBy(c => c.grupo).ToList()
            }).OrderBy(c => c.clase).ToList();

            //*****  MOVIMIENTOS AGRUPADOS -FIN

            #endregion

            #region Ejecuta el modelo 

            if (checkCentro && checkNit == false && checkMovimiento)
            {
                ViewAsPdf salida = new ViewAsPdf("repcontablepdfmov", "");
                ViewModelReportepdfmov reporte10 = new ViewModelReportepdfmov
                {
                    titulo = "Reporte Contable De Movimientos de Documentos por Centro",
                    fechaReporte = DateTime.Now,
                    fechaInicio = Convert.ToString(wanio),
                    fechafin = Convert.ToString(wmes),
                    cuentaIni = Convert.ToString(wcuentaini),
                    cuentaFin = Convert.ToString(wcuentafin),
                    // Grantotalsaldoini = seleccionSegundo_04.Sum((f => f.Ctotalsaldoini)),
                    Grantotaldebito = seleccion_de_Mov_01.Sum(f => f.Ctotaldebito),
                    Grantotalcredito = seleccion_de_Mov_01.Sum(f => f.Ctotalcredito),
                    Grantotalsaldo = seleccion_de_Mov_01.Sum(f => f.Ctotalsaldo),
                    //  Grantotalsaldoininiff = seleccionSegundo_04.Sum((f => f.Ctotalsaldoininiff)),
                    Grantotaldebitoniff = seleccion_de_Mov_01.Sum(f => f.Ctotaldebitoniff),
                    Grantotalcreditoniff = seleccion_de_Mov_01.Sum(f => f.Ctotalcreditoniff),
                    Grantotalsaldoniff = seleccion_de_Mov_01.Sum(f => f.Ctotalsaldoniff),
                    //seleccion_cuenta = seleccionCentro,
                    listaclasemov = seleccion_de_Mov_01
                };

                salida = new ViewAsPdf("repcontablepdfmov", reporte10);
                something = salida;
            }

            #endregion

            #region select de Mov

            ///*****  MOVIMIENTOS AGRUPADOS - INICIO
            List<listaclasemov> seleccion_de_Mov_02 = consulta_mov.GroupBy(lc1 => new { lc1.clase }).Select(lc1 => new listaclasemov
            {
                clase = lc1.Key.clase ?? 0,
                nom_clase = lc1.Select(f => f.nom_clase).First(),
                //  Ctotalsaldoini = lc1.Sum(f => f.saldo_ini),
                Ctotaldebito = lc1.Sum(f => f.debito),
                Ctotalcredito = lc1.Sum(f => f.credito),
                Ctotalsaldo = lc1.Sum(f => f.debito - f.credito),
                //  Ctotalsaldoininiff = lc1.Sum(f => f.saldo_ininiff),
                Ctotaldebitoniff = lc1.Sum(f => f.debitoniif),
                Ctotalcreditoniff = lc1.Sum(f => f.creditoniif),
                Ctotalsaldoniff = lc1.Sum(f => f.debitoniif - f.creditoniif),
                agrupa_grupomov = lc1.GroupBy(ag => new { ag.grupo }).Select(ag => new agrupa_grupomov
                {
                    grupo = ag.Key.grupo ?? 0,
                    nom_grupo = ag.Select(g => g.nom_grupo).First(),
                    //  grupo_t_saldoini = ag.Sum(f => f.saldo_ini),
                    grupo_t_debito = ag.Sum(f => f.debito),
                    grupo_t_credito = ag.Sum(f => f.credito),
                    grupo_t_saldo = ag.Sum(f => f.debito - f.credito),
                    //    grupo_t_saldoininiff = ag.Sum(f => f.saldo_ininiff),
                    grupo_t_debitoniff = ag.Sum(f => f.debitoniif),
                    grupo_t_creditoniff = ag.Sum(f => f.creditoniif),
                    grupo_t_saldoniff = ag.Sum(f => f.debitoniif - f.creditoniif),
                    agrupa_cuentamov = ag.GroupBy(ac => new { ac.xcuenta }).Select(ac => new agrupa_cuentamov
                    {
                        cuenta = ac.Key.xcuenta ?? 0,
                        nom_cuenta = ac.Select(g => g.nom_xcuenta).First(),
                        //cuenta_t_saldoini = ac.Sum(f => f.saldo_ini),
                        cuenta_t_debito = ac.Sum(f => f.debito),
                        cuenta_t_credito = ac.Sum(f => f.credito),
                        cuenta_t_saldo = ac.Sum(f => f.debito - f.credito),
                        //  cuenta_t_saldoininiff = ac.Sum(f => f.saldo_ininiff),
                        cuenta_t_debitoniff = ac.Sum(f => f.debitoniif),
                        cuenta_t_creditoniff = ac.Sum(f => f.creditoniif),
                        cuenta_t_saldoniff = ac.Sum(f => f.debitoniif - f.creditoniif),

                        agrupa_subcuentamov = ac.GroupBy(asc => new { asc.subcuenta }).Select(asc =>
                              new agrupa_subcuentamov
                              {
                                  subcuenta = asc.Key.subcuenta ?? 0,
                                  nom_subcuenta = asc.Select(g => g.nom_subcuenta).First(),
                                  // subcuenta_t_saldoini = asc.Sum(f => f.saldo_ini),
                                  subcuenta_t_debito = asc.Sum(f => f.debito),
                                  subcuenta_t_credito = asc.Sum(f => f.credito),
                                  subcuenta_t_saldo = asc.Sum(f => f.debito - f.credito),
                                  // subcuenta_t_saldoininiff = asc.Sum(f => f.saldo_ininiff),
                                  subcuenta_t_debitoniff = asc.Sum(f => f.debitoniif),
                                  subcuenta_t_creditoniff = asc.Sum(f => f.creditoniif),
                                  subcuenta_t_saldoniff = asc.Sum(f => f.debitoniif - f.creditoniif),
                                  detalleCuentamov = asc.GroupBy(dc => new { dc.cuenta }).Select(dc => new detalleCuentamov
                                  {
                                      id_cuenta_det = dc.Key.cuenta,
                                      numero_cuenta_det = dc.Select(g => g.cntpuc_numero).First(),
                                      //clase_cuenta_det = dc.Select(g => g.clase ?? 0).First(),
                                      //grupo_cuenta_det = dc.Select(g => g.grupo ?? 0).First(),
                                      //xcuenta_cuenta_det = dc.Select(g => g.xcuenta ?? 0).First(),
                                      //subcuenta_cuenta_det = dc.Select(g => g.subcuenta ?? 0).First(),
                                      nombre_cuenta_det = dc.Select(g => g.cntpuc_descp).First(),
                                      // saldoini_det = dc.Sum(fc => fc.saldo_ini),
                                      debito_det = dc.Sum(fc => fc.debito),
                                      credito_det = dc.Sum(fc => fc.credito),
                                      saldo_det = dc.Sum(f => f.debito - f.credito),
                                      // saldoininiff_det = dc.Sum(fc => fc.saldo_ininiff),
                                      debitoniff_det = dc.Sum(fc => fc.debitoniif),
                                      creditoniff_det = dc.Sum(fc => fc.creditoniif),
                                      saldoniff_det = dc.Sum(f => f.debitoniif - f.creditoniif),
                                      listacentromov = dc.GroupBy(lc => new { lc.centro }).Select(lc => new listacentromov
                                      {
                                          id_centro = lc.Key.centro,
                                          codigo_centro = lc.Select(f => f.pre_centcst).First(),
                                          nombre_centro = lc.Select(f => f.centcst_nombre).First(),
                                          // scentro_t_saldoini = lc.Sum(f => f.saldo_ini),
                                          scentro_t_debito = lc.Sum(f => f.debito),
                                          scentro_t_credito = lc.Sum(f => f.credito),
                                          scentro_t_saldo = lc.Sum(f => f.debito - f.credito),
                                          //  scentro_t_saldoininiff = lc.Sum(f => f.saldo_ininiff),
                                          scentro_t_debitoniff = lc.Sum(f => f.debitoniif),
                                          scentro_t_creditoniff = lc.Sum(f => f.creditoniif),
                                          scentro_t_saldoniff = lc.Sum(f => f.debitoniif - f.creditoniif),
                                          agrupa_nit_mov = lc.GroupBy(ni => new { ni.nit }).Select(ni => new agrupa_nit_mov
                                          {
                                              nit = ni.Key.nit,
                                              documento = ni.Select(f => f.doc_tercero).First(),
                                              nombre_nit = ni.Select(f => f.NombreX).First(),
                                              nit_t_debito = ni.Sum(fc => fc.debito),
                                              nit_t_credito = ni.Sum(fc => fc.credito),
                                              nit_t_saldo = ni.Sum(fc => fc.debito - fc.credito),
                                              nit_t_debitoniff = ni.Sum(fc => fc.debitoniif),
                                              nit_t_creditoniff = ni.Sum(fc => fc.creditoniif),
                                              nit_t_saldoniff = ni.Sum(fc => fc.debitoniif - fc.creditoniif),
                                              lista_mov = ni.GroupBy(lm => new { lm.numero }).Select(lm => new lista_mov
                                              {
                                                  doc = lm.Key.numero ?? 0,
                                                  prefijo = lm.Select(f => f.prefijo).First(),
                                                  nom_docuento = lm.Select(f => f.tpdoc_nombre).First(),
                                                  debito_mov = lm.Sum(fc => fc.debito),
                                                  credito_mov = lm.Sum(fc => fc.credito),
                                                  saldo_mov = lm.Sum(fc => fc.debito - fc.credito),
                                                  debitoniff_mov = lm.Sum(fc => fc.debitoniif),
                                                  creditoniff_mov = lm.Sum(fc => fc.creditoniif),
                                                  saldoniff_mov = lm.Sum(fc => fc.debitoniif - fc.creditoniif)
                                              }).OrderBy(c => c.doc).ToList()
                                          }).OrderBy(c => c.nit).ToList()
                                      }).OrderBy(c => c.id_centro).ToList()
                                  }).OrderBy(c => c.id_cuenta_det).ToList()
                              }).OrderBy(c => c.subcuenta).ToList()
                    }).OrderBy(c => c.cuenta).ToList()
                }).OrderBy(c => c.grupo).ToList()
            }).OrderBy(c => c.clase).ToList();

            //*****  MOVIMIENTOS AGRUPADOS -FIN

            #endregion

            #region Ejecuta el modelo 

            if (checkCentro && checkNit && checkMovimiento)
            {
                ViewAsPdf salida = new ViewAsPdf("repcontablepdfmov", "");
                ViewModelReportepdfmov reporte11 = new ViewModelReportepdfmov
                {
                    titulo = "Reporte Contable por Centro y Nit de Documentos",
                    fechaReporte = DateTime.Now,
                    fechaInicio = Convert.ToString(wanio),
                    fechafin = Convert.ToString(wmes),
                    cuentaIni = Convert.ToString(wcuentaini),
                    cuentaFin = Convert.ToString(wcuentafin),
                    // Grantotalsaldoini = seleccionSegundo_04.Sum((f => f.Ctotalsaldoini)),
                    Grantotaldebito = seleccion_de_Mov_02.Sum(f => f.Ctotaldebito),
                    Grantotalcredito = seleccion_de_Mov_02.Sum(f => f.Ctotalcredito),
                    Grantotalsaldo = seleccion_de_Mov_02.Sum(f => f.Ctotalsaldo),
                    //  Grantotalsaldoininiff = seleccionSegundo_04.Sum((f => f.Ctotalsaldoininiff)),
                    Grantotaldebitoniff = seleccion_de_Mov_02.Sum(f => f.Ctotaldebitoniff),
                    Grantotalcreditoniff = seleccion_de_Mov_02.Sum(f => f.Ctotalcreditoniff),
                    Grantotalsaldoniff = seleccion_de_Mov_02.Sum(f => f.Ctotalsaldoniff),
                    //seleccion_cuenta = seleccionCentro,
                    listaclasemov = seleccion_de_Mov_02
                };

                salida = new ViewAsPdf("repcontablepdfmov", reporte11);
                something = salida;
            }

            #endregion

            #region select de Mov

            ///*****  MOVIMIENTOS AGRUPADOS - INICIO
            List<listaclasemov> seleccion_de_Mov_03 = consulta_mov.GroupBy(lc1 => new { lc1.clase }).Select(lc1 => new listaclasemov
            {
                clase = lc1.Key.clase ?? 0,
                nom_clase = lc1.Select(f => f.nom_clase).First(),
                //  Ctotalsaldoini = lc1.Sum(f => f.saldo_ini),
                Ctotaldebito = lc1.Sum(f => f.debito),
                Ctotalcredito = lc1.Sum(f => f.credito),
                Ctotalsaldo = lc1.Sum(f => f.debito - f.credito),
                //  Ctotalsaldoininiff = lc1.Sum(f => f.saldo_ininiff),
                Ctotaldebitoniff = lc1.Sum(f => f.debitoniif),
                Ctotalcreditoniff = lc1.Sum(f => f.creditoniif),
                Ctotalsaldoniff = lc1.Sum(f => f.debitoniif - f.creditoniif),
                agrupa_grupomov = lc1.GroupBy(ag => new { ag.grupo }).Select(ag => new agrupa_grupomov
                {
                    grupo = ag.Key.grupo ?? 0,
                    nom_grupo = ag.Select(g => g.nom_grupo).First(),
                    //  grupo_t_saldoini = ag.Sum(f => f.saldo_ini),
                    grupo_t_debito = ag.Sum(f => f.debito),
                    grupo_t_credito = ag.Sum(f => f.credito),
                    grupo_t_saldo = ag.Sum(f => f.debito - f.credito),
                    //    grupo_t_saldoininiff = ag.Sum(f => f.saldo_ininiff),
                    grupo_t_debitoniff = ag.Sum(f => f.debitoniif),
                    grupo_t_creditoniff = ag.Sum(f => f.creditoniif),
                    grupo_t_saldoniff = ag.Sum(f => f.debitoniif - f.creditoniif),
                    agrupa_cuentamov = ag.GroupBy(ac => new { ac.xcuenta }).Select(ac => new agrupa_cuentamov
                    {
                        cuenta = ac.Key.xcuenta ?? 0,
                        nom_cuenta = ac.Select(g => g.nom_xcuenta).First(),
                        //cuenta_t_saldoini = ac.Sum(f => f.saldo_ini),
                        cuenta_t_debito = ac.Sum(f => f.debito),
                        cuenta_t_credito = ac.Sum(f => f.credito),
                        cuenta_t_saldo = ac.Sum(f => f.debito - f.credito),
                        //  cuenta_t_saldoininiff = ac.Sum(f => f.saldo_ininiff),
                        cuenta_t_debitoniff = ac.Sum(f => f.debitoniif),
                        cuenta_t_creditoniff = ac.Sum(f => f.creditoniif),
                        cuenta_t_saldoniff = ac.Sum(f => f.debitoniif - f.creditoniif),

                        agrupa_subcuentamov = ac.GroupBy(asc => new { asc.subcuenta }).Select(asc =>
                              new agrupa_subcuentamov
                              {
                                  subcuenta = asc.Key.subcuenta ?? 0,
                                  nom_subcuenta = asc.Select(g => g.nom_subcuenta).First(),
                                  // subcuenta_t_saldoini = asc.Sum(f => f.saldo_ini),
                                  subcuenta_t_debito = asc.Sum(f => f.debito),
                                  subcuenta_t_credito = asc.Sum(f => f.credito),
                                  subcuenta_t_saldo = asc.Sum(f => f.debito - f.credito),
                                  // subcuenta_t_saldoininiff = asc.Sum(f => f.saldo_ininiff),
                                  subcuenta_t_debitoniff = asc.Sum(f => f.debitoniif),
                                  subcuenta_t_creditoniff = asc.Sum(f => f.creditoniif),
                                  subcuenta_t_saldoniff = asc.Sum(f => f.debitoniif - f.creditoniif),
                                  detalleCuentamov = asc.GroupBy(dc => new { dc.cuenta }).Select(dc => new detalleCuentamov
                                  {
                                      id_cuenta_det = dc.Key.cuenta,
                                      numero_cuenta_det = dc.Select(g => g.cntpuc_numero).First(),
                                      //clase_cuenta_det = dc.Select(g => g.clase ?? 0).First(),
                                      //grupo_cuenta_det = dc.Select(g => g.grupo ?? 0).First(),
                                      //xcuenta_cuenta_det = dc.Select(g => g.xcuenta ?? 0).First(),
                                      //subcuenta_cuenta_det = dc.Select(g => g.subcuenta ?? 0).First(),
                                      nombre_cuenta_det = dc.Select(g => g.cntpuc_descp).First(),
                                      // saldoini_det = dc.Sum(fc => fc.saldo_ini),
                                      debito_det = dc.Sum(fc => fc.debito),
                                      credito_det = dc.Sum(fc => fc.credito),
                                      saldo_det = dc.Sum(f => f.debito - f.credito),
                                      // saldoininiff_det = dc.Sum(fc => fc.saldo_ininiff),
                                      debitoniff_det = dc.Sum(fc => fc.debitoniif),
                                      creditoniff_det = dc.Sum(fc => fc.creditoniif),
                                      saldoniff_det = dc.Sum(f => f.debitoniif - f.creditoniif),
                                      lista_mov = dc.GroupBy(lm => new { lm.numero }).Select(lm => new lista_mov
                                      {
                                          doc = lm.Key.numero ?? 0,
                                          prefijo = lm.Select(f => f.prefijo).First(),
                                          nom_docuento = lm.Select(f => f.tpdoc_nombre).First(),
                                          debito_mov = lm.Sum(fc => fc.debito),
                                          credito_mov = lm.Sum(fc => fc.credito),
                                          saldo_mov = lm.Sum(fc => fc.debito - fc.credito),
                                          debitoniff_mov = lm.Sum(fc => fc.debitoniif),
                                          creditoniff_mov = lm.Sum(fc => fc.creditoniif),
                                          saldoniff_mov = lm.Sum(fc => fc.debitoniif - fc.creditoniif)
                                      }).OrderBy(c => c.doc).ToList()
                                  }).OrderBy(c => c.id_cuenta_det).ToList()
                              }).OrderBy(c => c.subcuenta).ToList()
                    }).OrderBy(c => c.cuenta).ToList()
                }).OrderBy(c => c.grupo).ToList()
            }).OrderBy(c => c.clase).ToList();

            //*****  MOVIMIENTOS AGRUPADOS -FIN

            #endregion

            #region Ejecuta el modelo 

            if (checkCentro == false && checkNit == false && checkMovimiento)
            {
                ViewAsPdf salida = new ViewAsPdf("repcontablepdfmov", "");
                ViewModelReportepdfmov reporte14 = new ViewModelReportepdfmov
                {
                    titulo = "Reporte Contable de Documentos",
                    fechaReporte = DateTime.Now,
                    fechaInicio = Convert.ToString(wanio),
                    fechafin = Convert.ToString(wmes),
                    cuentaIni = Convert.ToString(wcuentaini),
                    cuentaFin = Convert.ToString(wcuentafin),
                    // Grantotalsaldoini = seleccionSegundo_04.Sum((f => f.Ctotalsaldoini)),
                    Grantotaldebito = seleccion_de_Mov_03.Sum(f => f.Ctotaldebito),
                    Grantotalcredito = seleccion_de_Mov_03.Sum(f => f.Ctotalcredito),
                    Grantotalsaldo = seleccion_de_Mov_03.Sum(f => f.Ctotalsaldo),
                    //  Grantotalsaldoininiff = seleccionSegundo_04.Sum((f => f.Ctotalsaldoininiff)),
                    Grantotaldebitoniff = seleccion_de_Mov_03.Sum(f => f.Ctotaldebitoniff),
                    Grantotalcreditoniff = seleccion_de_Mov_03.Sum(f => f.Ctotalcreditoniff),
                    Grantotalsaldoniff = seleccion_de_Mov_03.Sum(f => f.Ctotalsaldoniff),
                    //seleccion_cuenta = seleccionCentro,
                    listaclasemov = seleccion_de_Mov_03
                };

                salida = new ViewAsPdf("repcontablepdfmov", reporte14);
                something = salida;
            }

            #endregion

            #region select de Mov

            ///*****  MOVIMIENTOS AGRUPADOS - INICIO
            List<listaclasemov> seleccion_de_Mov_04 = consulta_mov.GroupBy(lc1 => new { lc1.clase }).Select(lc1 => new listaclasemov
            {
                clase = lc1.Key.clase ?? 0,
                nom_clase = lc1.Select(f => f.nom_clase).First(),
                //  Ctotalsaldoini = lc1.Sum(f => f.saldo_ini),
                Ctotaldebito = lc1.Sum(f => f.debito),
                Ctotalcredito = lc1.Sum(f => f.credito),
                Ctotalsaldo = lc1.Sum(f => f.debito - f.credito),
                //  Ctotalsaldoininiff = lc1.Sum(f => f.saldo_ininiff),
                Ctotaldebitoniff = lc1.Sum(f => f.debitoniif),
                Ctotalcreditoniff = lc1.Sum(f => f.creditoniif),
                Ctotalsaldoniff = lc1.Sum(f => f.debitoniif - f.creditoniif),
                agrupa_grupomov = lc1.GroupBy(ag => new { ag.grupo }).Select(ag => new agrupa_grupomov
                {
                    grupo = ag.Key.grupo ?? 0,
                    nom_grupo = ag.Select(g => g.nom_grupo).First(),
                    //  grupo_t_saldoini = ag.Sum(f => f.saldo_ini),
                    grupo_t_debito = ag.Sum(f => f.debito),
                    grupo_t_credito = ag.Sum(f => f.credito),
                    grupo_t_saldo = ag.Sum(f => f.debito - f.credito),
                    //    grupo_t_saldoininiff = ag.Sum(f => f.saldo_ininiff),
                    grupo_t_debitoniff = ag.Sum(f => f.debitoniif),
                    grupo_t_creditoniff = ag.Sum(f => f.creditoniif),
                    grupo_t_saldoniff = ag.Sum(f => f.debitoniif - f.creditoniif),
                    agrupa_cuentamov = ag.GroupBy(ac => new { ac.xcuenta }).Select(ac => new agrupa_cuentamov
                    {
                        cuenta = ac.Key.xcuenta ?? 0,
                        nom_cuenta = ac.Select(g => g.nom_xcuenta).First(),
                        //cuenta_t_saldoini = ac.Sum(f => f.saldo_ini),
                        cuenta_t_debito = ac.Sum(f => f.debito),
                        cuenta_t_credito = ac.Sum(f => f.credito),
                        cuenta_t_saldo = ac.Sum(f => f.debito - f.credito),
                        //  cuenta_t_saldoininiff = ac.Sum(f => f.saldo_ininiff),
                        cuenta_t_debitoniff = ac.Sum(f => f.debitoniif),
                        cuenta_t_creditoniff = ac.Sum(f => f.creditoniif),
                        cuenta_t_saldoniff = ac.Sum(f => f.debitoniif - f.creditoniif),

                        agrupa_subcuentamov = ac.GroupBy(asc => new { asc.subcuenta }).Select(asc =>
                              new agrupa_subcuentamov
                              {
                                  subcuenta = asc.Key.subcuenta ?? 0,
                                  nom_subcuenta = asc.Select(g => g.nom_subcuenta).First(),
                                  // subcuenta_t_saldoini = asc.Sum(f => f.saldo_ini),
                                  subcuenta_t_debito = asc.Sum(f => f.debito),
                                  subcuenta_t_credito = asc.Sum(f => f.credito),
                                  subcuenta_t_saldo = asc.Sum(f => f.debito - f.credito),
                                  // subcuenta_t_saldoininiff = asc.Sum(f => f.saldo_ininiff),
                                  subcuenta_t_debitoniff = asc.Sum(f => f.debitoniif),
                                  subcuenta_t_creditoniff = asc.Sum(f => f.creditoniif),
                                  subcuenta_t_saldoniff = asc.Sum(f => f.debitoniif - f.creditoniif),
                                  detalleCuentamov = asc.GroupBy(dc => new { dc.cuenta }).Select(dc => new detalleCuentamov
                                  {
                                      id_cuenta_det = dc.Key.cuenta,
                                      numero_cuenta_det = dc.Select(g => g.cntpuc_numero).First(),
                                      //clase_cuenta_det = dc.Select(g => g.clase ?? 0).First(),
                                      //grupo_cuenta_det = dc.Select(g => g.grupo ?? 0).First(),
                                      //xcuenta_cuenta_det = dc.Select(g => g.xcuenta ?? 0).First(),
                                      //subcuenta_cuenta_det = dc.Select(g => g.subcuenta ?? 0).First(),
                                      nombre_cuenta_det = dc.Select(g => g.cntpuc_descp).First(),
                                      // saldoini_det = dc.Sum(fc => fc.saldo_ini),
                                      debito_det = dc.Sum(fc => fc.debito),
                                      credito_det = dc.Sum(fc => fc.credito),
                                      saldo_det = dc.Sum(f => f.debito - f.credito),
                                      // saldoininiff_det = dc.Sum(fc => fc.saldo_ininiff),
                                      debitoniff_det = dc.Sum(fc => fc.debitoniif),
                                      creditoniff_det = dc.Sum(fc => fc.creditoniif),
                                      saldoniff_det = dc.Sum(f => f.debitoniif - f.creditoniif),
                                      agrupa_nit_mov = dc.GroupBy(ni => new { ni.nit }).Select(ni => new agrupa_nit_mov
                                      {
                                          nit = ni.Key.nit,
                                          documento = ni.Select(f => f.doc_tercero).First(),
                                          nombre_nit = ni.Select(f => f.NombreX).First(),
                                          nit_t_debito = ni.Sum(fc => fc.debito),
                                          nit_t_credito = ni.Sum(fc => fc.credito),
                                          nit_t_saldo = ni.Sum(fc => fc.debito - fc.credito),
                                          nit_t_debitoniff = ni.Sum(fc => fc.debitoniif),
                                          nit_t_creditoniff = ni.Sum(fc => fc.creditoniif),
                                          nit_t_saldoniff = ni.Sum(fc => fc.debitoniif - fc.creditoniif),
                                          lista_mov = ni.GroupBy(lm => new { lm.numero }).Select(lm => new lista_mov
                                          {
                                              doc = lm.Key.numero ?? 0,
                                              prefijo = lm.Select(f => f.prefijo).First(),
                                              nom_docuento = lm.Select(f => f.tpdoc_nombre).First(),
                                              debito_mov = lm.Sum(fc => fc.debito),
                                              credito_mov = lm.Sum(fc => fc.credito),
                                              saldo_mov = lm.Sum(fc => fc.debito - fc.credito),
                                              debitoniff_mov = lm.Sum(fc => fc.debitoniif),
                                              creditoniff_mov = lm.Sum(fc => fc.creditoniif),
                                              saldoniff_mov = lm.Sum(fc => fc.debitoniif - fc.creditoniif)
                                          }).OrderBy(c => c.doc).ToList()
                                      }).OrderBy(c => c.nit).ToList()
                                  }).OrderBy(c => c.id_cuenta_det).ToList()
                              }).OrderBy(c => c.subcuenta).ToList()
                    }).OrderBy(c => c.cuenta).ToList()
                }).OrderBy(c => c.grupo).ToList()
            }).OrderBy(c => c.clase).ToList();

            //*****  MOVIMIENTOS AGRUPADOS -FIN

            #endregion

            #region Ejecuta el modelo 

            if (checkCentro == false && checkNit && checkMovimiento)
            {
                ViewAsPdf salida = new ViewAsPdf("repcontablepdfmov", "");
                ViewModelReportepdfmov reporte15 = new ViewModelReportepdfmov
                {
                    titulo = "Reporte Contable por Nit de Documentos",
                    fechaReporte = DateTime.Now,
                    fechaInicio = Convert.ToString(wanio),
                    fechafin = Convert.ToString(wmes),
                    cuentaIni = Convert.ToString(wcuentaini),
                    cuentaFin = Convert.ToString(wcuentafin),
                    // Grantotalsaldoini = seleccionSegundo_04.Sum((f => f.Ctotalsaldoini)),
                    Grantotaldebito = seleccion_de_Mov_04.Sum(f => f.Ctotaldebito),
                    Grantotalcredito = seleccion_de_Mov_04.Sum(f => f.Ctotalcredito),
                    Grantotalsaldo = seleccion_de_Mov_04.Sum(f => f.Ctotalsaldo),
                    //  Grantotalsaldoininiff = seleccionSegundo_04.Sum((f => f.Ctotalsaldoininiff)),
                    Grantotaldebitoniff = seleccion_de_Mov_04.Sum(f => f.Ctotaldebitoniff),
                    Grantotalcreditoniff = seleccion_de_Mov_04.Sum(f => f.Ctotalcreditoniff),
                    Grantotalsaldoniff = seleccion_de_Mov_04.Sum(f => f.Ctotalsaldoniff),
                    //seleccion_cuenta = seleccionCentro,
                    listaclasemov = seleccion_de_Mov_04
                };

                salida = new ViewAsPdf("repcontablepdfmov", reporte15);
                something = salida;
            }

            #endregion

            //// ******** para Movimientos Fin **********

            return something;
        }

        public ActionResult repcontablepdf(
            int? anioini,
            int? mesini,
            int? aniofin,
            int? mesfin,
            int? cuentaini,
            int? cuentafin,
            int? idtercero,
            string centros,
            string tipos_doc,
            bool checkCentro,
            bool checkNit,
            bool checkMovimiento,
            bool checkNiff,
            int? mes,
            int? anio
        )
        {
            int? wanioini = anioini;
            int? wmesini = mesini;
            int? waniofin = aniofin;
            int? wmesfin = mesfin;
            int? wcuentaini = cuentaini;
            int? wcuentafin = cuentafin;
            int? widtercero = idtercero;
            int? wanio = anio;
            int? wmes = mes;

            string[] Vectorcentros = centros.Split(',');
            string[] VectorTipos = tipos_doc.Split(',');

            bool Ver = true;


            //********************

            #region Declaracion de PredicateBuilder Pdf

            //Declaramos el predicado de tipo And con el parámetro True. Si fuera de tipo Or lo declaramos con el parámetro False
            System.Linq.Expressions.Expression<Func<vw_cuentas_valores, bool>> predicate = PredicateBuilder.True<vw_cuentas_valores>();
            System.Linq.Expressions.Expression<Func<vw_cuentas_valores, bool>> predicateMes = PredicateBuilder.False<vw_cuentas_valores>();
            System.Linq.Expressions.Expression<Func<vw_cuentas_valores, bool>> predicate2 = PredicateBuilder.False<vw_cuentas_valores>();
            System.Linq.Expressions.Expression<Func<vw_cuentas_valores, bool>> predicateTemp = PredicateBuilder.True<vw_cuentas_valores>();

            #endregion

            #region Declaracion de PredicateBuilder2 Pdf

            //Declaramos el predicado de tipo And con el parámetro True. Si fuera de tipo Or lo declaramos con el parámetro False
            System.Linq.Expressions.Expression<Func<vw_mov_contables, bool>> predicateMovAnd = PredicateBuilder.True<vw_mov_contables>();
            System.Linq.Expressions.Expression<Func<vw_mov_contables, bool>> predicateMovOr = PredicateBuilder.False<vw_mov_contables>();

            System.Linq.Expressions.Expression<Func<vw_cuentas_valores, bool>> predicateMovAnd2 = PredicateBuilder.True<vw_cuentas_valores>();
            System.Linq.Expressions.Expression<Func<vw_cuentas_valores, bool>> predicateMovOr2 = PredicateBuilder.False<vw_cuentas_valores>();

            #endregion

            #region Criterios aplicados a los PredicateBuilder Pdf

            #region año mes

            if (wanio != null)
            {
                predicate = predicate.And(x => x.ano == wanio);
                predicateMovAnd = predicateMovAnd.And(x => x.ano == wanio);
                predicateMovAnd2 = predicateMovAnd2.And(x => x.ano == wanio);
            }

            if (wmes != null)
            {
                predicate = predicate.And(x => x.mes == wmes);
                predicateMovAnd = predicateMovAnd.And(x => x.mes == wmes);
                predicateMovAnd2 = predicateMovAnd2.And(x => x.mes == wmes);
                // predicateMes = predicateMes.Or(x => x.mes >= wmesini && x.mes <= wmesfin);
            }

            if (wanioini != null && waniofin != null)
            {
                // predicate = predicate.And(x => x.ano >= wanioini && x.ano <= waniofin);
            }

            if (wmesini != null && wmesfin != null)
            {
                //predicate = predicate.And(x => x.mes >= wmesini && x.mes <= wmesfin);
                // predicateMes = predicateMes.Or(x => x.mes >= wmesini && x.mes <= wmesfin);
            }

            #endregion

            //    predicate = predicate.And(predicateMes);
            //  var Aux_PrediMes = db.vw_cuentas_valores.Where(predicate).ToList();
            string paso = "";

            //#region Predicate cuentas
            //var desde1 = Convert.ToString(wcuentaini);
            //var hasta1 = Convert.ToString(wcuentafin);
            //var rango2 = (from p in db.vw_cuentas_valores
            //              where p.cntpuc_numero.Substring(0, 12).CompareTo(desde1.Substring(0, 12)) >= 0 && p.cntpuc_numero.Substring(0, 12).CompareTo(hasta1.Substring(0, 12)) <= 0 
            //              orderby p.cntpuc_numero, p.clase, p.grupo, p.cuenta, p.subcuenta
            //              select p.cntpuc_numero).ToList();
            //predicateMovAnd2 = predicateMovAnd2.And(p => 1 == 1 && rango2.Contains(p.cntpuc_numero));
            //var ConsultaRnago = db.vw_cuentas_valores.Where(predicateMovAnd2).ToList();
            //#endregion


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
                    predicate = predicate.And(
                        x => x.clase >= ci11 && x.clase <= cf11 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci11 && x.clase <= cf11 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = "-11-";
                }

                if (Convert.ToString(wcuentaini).Length == 1 && Convert.ToString(wcuentafin).Length == 2)
                {
                    int ci12 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf12 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gf12 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));


                    string numi12 = Convert.ToString(wcuentaini);
                    string numf12 = Convert.ToString(wcuentafin);
                    predicate = predicate.And(
                        x => x.clase >= ci12 && x.clase <= cf12 &&
                             x.grupo <= gf12 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci12 && x.clase <= cf12 &&
                                 x.grupo <= gf12 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-12-";
                }

                if (Convert.ToString(wcuentaini).Length == 1 && Convert.ToString(wcuentafin).Length == 3)
                {
                    int ci13 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf13 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gf13 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));


                    string numi13 = Convert.ToString(wcuentaini);
                    string numf13 = Convert.ToString(wcuentafin);
                    predicate = predicate.And(
                        x => x.clase >= ci13 && x.clase <= cf13 &&
                             x.grupo <= gf13 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci13 && x.clase <= cf13 &&
                                 x.grupo <= gf13 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci14 && x.clase <= cf14 && x.grupo <= gf14 &&
                             x.xcuenta <= xcf14 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci14 && x.clase <= cf14 && x.grupo <= gf14 &&
                                 x.xcuenta <= xcf14 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci15 && x.clase <= cf15 && x.grupo <= gf15 &&
                             x.xcuenta <= xcf15 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci15 && x.clase <= cf15 && x.grupo <= gf15 &&
                                 x.xcuenta <= xcf15 /* && x.cntpuc_numero <= wcuentaini*/);
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


                    predicate = predicate.And(
                        x => x.clase >= ci16 && x.clase <= cf16 && x.grupo <= gf16 && x.xcuenta <= xcf16 &&
                             x.subcuenta <= scf16 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci16 && x.clase <= cf16 && x.grupo <= gf16 && x.xcuenta <= xcf16 &&
                                 x.subcuenta <= scf16 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-1>5-";
                }

                if (Convert.ToString(wcuentaini).Length == 1 && Convert.ToString(wcuentafin).Length > 6)
                {
                    #region Predicate cuentas

                    string desde1 = Convert.ToString(wcuentaini);
                    string hasta1 = Convert.ToString(wcuentafin);
                    List<string> rango1 = (from p in db.vw_cuentas_valores
                                           where p.cntpuc_numero.Substring(0, 12).CompareTo(desde1.Substring(0, 12)) >= 0 &&
                                                 p.cntpuc_numero.Substring(0, 12).CompareTo(hasta1.Substring(0, 12)) <= 0
                                           orderby p.cntpuc_numero, p.clase, p.grupo, p.cuenta, p.subcuenta
                                           select p.cntpuc_numero).ToList();
                    predicate = predicate.And(p => 1 == 1 && rango1.Contains(p.cntpuc_numero));
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
                    predicate = predicate.And(
                        x => x.clase >= ci22 && x.clase <= cf22 && x.grupo >= gi22 &&
                             x.grupo <= gf22 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci22 && x.clase <= cf22 && x.grupo >= gi22 &&
                                 x.grupo <= gf22 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci23 && x.clase <= cf23 && x.grupo >= gi23 &&
                             x.grupo <= gf23 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci23 && x.clase <= cf23 && x.grupo >= gi23 &&
                                 x.grupo <= gf23 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci24 && x.clase <= cf24 && x.grupo >= gi24 && x.grupo <= gf24 &&
                             x.xcuenta <= xcf24 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci24 && x.clase <= cf24 && x.grupo >= gi24 && x.grupo <= gf24 &&
                                 x.xcuenta <= xcf24 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci25 && x.clase <= cf25 && x.grupo >= gi25 && x.grupo <= gf25 &&
                             x.xcuenta <= xcf25 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci25 && x.clase <= cf25 && x.grupo >= gi25 && x.grupo <= gf25 &&
                                 x.xcuenta <= xcf25 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci26 && x.clase <= cf26 && x.grupo >= gi26 && x.grupo <= gf26 &&
                             x.xcuenta <= xcf26 && x.subcuenta <= scf26 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci26 && x.clase <= cf26 && x.grupo >= gi26 && x.grupo <= gf26 &&
                                 x.xcuenta <= xcf26 && x.subcuenta <= scf26 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-2>5-";
                }

                if (Convert.ToString(wcuentaini).Length == 2 && Convert.ToString(wcuentafin).Length > 6)
                {
                    #region Predicate cuentas

                    string desde2 = Convert.ToString(wcuentaini);
                    string hasta2 = Convert.ToString(wcuentafin);
                    List<string> rango2 = (from p in db.vw_cuentas_valores
                                           where p.cntpuc_numero.Substring(0, 12).CompareTo(desde2.Substring(0, 12)) >= 0 &&
                                                 p.cntpuc_numero.Substring(0, 12).CompareTo(hasta2.Substring(0, 12)) <= 0
                                           orderby p.cntpuc_numero, p.clase, p.grupo, p.cuenta, p.subcuenta
                                           select p.cntpuc_numero).ToList();
                    predicate = predicate.And(p => 1 == 1 && rango2.Contains(p.cntpuc_numero));
                    //  var ConsultaRnago = db.vw_cuentas_valores.Where(predicateMovAnd2).ToList();

                    #endregion

                    paso = paso + "-1>6-";
                }

                #endregion

                #region cuenta 3

                if (Convert.ToString(wcuentaini).Length == 3 && Convert.ToString(wcuentafin).Length == 3)
                {
                    int ci33 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 1));
                    int cf33 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 1));

                    int gi33 = Convert.ToInt32(Convert.ToString(wcuentaini).Substring(0, 2));
                    int gf33 = Convert.ToInt32(Convert.ToString(wcuentafin).Substring(0, 2));

                    string numi33 = Convert.ToString(wcuentaini);
                    string numf33 = Convert.ToString(wcuentafin);
                    predicate = predicate.And(
                        x => x.clase >= ci33 && x.clase <= cf33 && x.grupo >= gi33 &&
                             x.grupo <= gf33 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci33 && x.clase <= cf33 && x.grupo >= gi33 &&
                                 x.grupo <= gf33 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci34 && x.clase <= cf34 && x.grupo >= gi34 && x.grupo <= gf34 &&
                             x.xcuenta <= xcf34 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci34 && x.clase <= cf34 && x.grupo >= gi34 && x.grupo <= gf34 &&
                                 x.xcuenta <= xcf34 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci35 && x.clase <= cf35 && x.grupo >= gi35 && x.grupo <= gf35 &&
                             x.xcuenta <= xcf35 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci35 && x.clase <= cf35 && x.grupo >= gi35 && x.grupo <= gf35 &&
                                 x.xcuenta <= xcf35 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci36 && x.clase <= cf36 && x.grupo >= gi36 && x.grupo <= gf36 &&
                             x.xcuenta <= xcf36 && x.subcuenta <= scf36 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci36 && x.clase <= cf36 && x.grupo >= gi36 && x.grupo <= gf36 &&
                                 x.xcuenta <= xcf36 && x.subcuenta <= scf36 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-3>5-";
                }

                if (Convert.ToString(wcuentaini).Length == 3 && Convert.ToString(wcuentafin).Length > 6)
                {
                    #region Predicate cuentas

                    string desde3 = Convert.ToString(wcuentaini);
                    string hasta3 = Convert.ToString(wcuentafin);
                    List<string> rango3 = (from p in db.vw_cuentas_valores
                                           where p.cntpuc_numero.Substring(0, 12).CompareTo(desde3.Substring(0, 12)) >= 0 &&
                                                 p.cntpuc_numero.Substring(0, 12).CompareTo(hasta3.Substring(0, 12)) <= 0
                                           orderby p.cntpuc_numero, p.clase, p.grupo, p.cuenta, p.subcuenta
                                           select p.cntpuc_numero).ToList();
                    predicate = predicate.And(p => 1 == 1 && rango3.Contains(p.cntpuc_numero));
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
                    predicate = predicate.And(
                        x => x.clase >= ci44 && x.clase <= cf44 && x.grupo >= gi44 && x.grupo <= gf44 &&
                             x.xcuenta >= xci44 && x.xcuenta <= xcf44 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci44 && x.clase <= cf44 && x.grupo >= gi44 && x.grupo <= gf44 &&
                                 x.xcuenta >= xci44 && x.xcuenta <= xcf44 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci45 && x.clase <= cf45 && x.grupo >= gi45 && x.grupo <= gf45 &&
                             x.xcuenta >= xci45 && x.xcuenta <= xcf45 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci45 && x.clase <= cf45 && x.grupo >= gi45 && x.grupo <= gf45 &&
                                 x.xcuenta >= xci45 && x.xcuenta <= xcf45 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci46 && x.clase <= cf46 && x.grupo >= gi46 && x.grupo <= gf46 &&
                             x.xcuenta >= xci46 && x.xcuenta <= xcf46 &&
                             x.subcuenta <= scf46 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci46 && x.clase <= cf46 && x.grupo >= gi46 && x.grupo <= gf46 &&
                                 x.xcuenta >= xci46 && x.xcuenta <= xcf46 &&
                                 x.subcuenta <= scf46 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-4>5-";
                }

                if (Convert.ToString(wcuentaini).Length == 4 && Convert.ToString(wcuentafin).Length > 6)
                {
                    #region Predicate cuentas

                    string desde4 = Convert.ToString(wcuentaini);
                    string hasta4 = Convert.ToString(wcuentafin);
                    List<string> rango4 = (from p in db.vw_cuentas_valores
                                           where p.cntpuc_numero.Substring(0, 12).CompareTo(desde4.Substring(0, 12)) >= 0 &&
                                                 p.cntpuc_numero.Substring(0, 12).CompareTo(hasta4.Substring(0, 12)) <= 0
                                           orderby p.cntpuc_numero, p.clase, p.grupo, p.cuenta, p.subcuenta
                                           select p.cntpuc_numero).ToList();
                    predicate = predicate.And(p => 1 == 1 && rango4.Contains(p.cntpuc_numero));
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
                    predicate = predicate.And(
                        x => x.clase >= ci55 && x.clase <= cf55 && x.grupo >= gi55 && x.grupo <= gf55 &&
                             x.xcuenta >= xci55 && x.xcuenta <= xcf55 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd =
                        predicateMovAnd.And(
                            x => x.clase >= ci55 && x.clase <= cf55 && x.grupo >= gi55 && x.grupo <= gf55 &&
                                 x.xcuenta >= xci55 && x.xcuenta <= xcf55 /* && x.cntpuc_numero <= wcuentaini*/);
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
                    predicate = predicate.And(
                        x => x.clase >= ci56 && x.clase <= cf56 && x.grupo >= gi56 && x.grupo <= gf56 &&
                             x.xcuenta >= xci56 && x.xcuenta <= xcf56 && x.subcuenta >= sci56 &&
                             x.subcuenta <= scf56 /*&& x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd = predicateMovAnd.And(
                        x => x.clase >= ci56 && x.clase <= cf56 && x.grupo >= gi56 && x.grupo <= gf56 &&
                             x.xcuenta >= xci56 && x.xcuenta <= xcf56 && x.subcuenta >= sci56 &&
                             x.subcuenta <= scf56 /*&& x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "-5>5-";
                }

                if (Convert.ToString(wcuentaini).Length == 5 && Convert.ToString(wcuentafin).Length > 6)
                {
                    #region Predicate cuentas

                    string desde5 = Convert.ToString(wcuentaini);
                    string hasta5 = Convert.ToString(wcuentafin);
                    List<string> rango5 = (from p in db.vw_cuentas_valores
                                           where p.cntpuc_numero.Substring(0, 12).CompareTo(desde5.Substring(0, 12)) >= 0 &&
                                                 p.cntpuc_numero.Substring(0, 12).CompareTo(hasta5.Substring(0, 12)) <= 0
                                           orderby p.cntpuc_numero, p.clase, p.grupo, p.cuenta, p.subcuenta
                                           select p.cntpuc_numero).ToList();
                    predicate = predicate.And(p => 1 == 1 && rango5.Contains(p.cntpuc_numero));
                    //  var ConsultaRnago = db.vw_cuentas_valores.Where(predicateMovAnd2).ToList();

                    #endregion

                    paso = paso + "-1>6-";
                }

                #endregion

                #region cuenta 6

                if (Convert.ToString(wcuentaini).Length == 5 && Convert.ToString(wcuentafin).Length == 6)
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
                    predicate = predicate.And(
                        x => x.clase >= ci66 && x.clase <= cf66 && x.grupo >= gi66 && x.grupo <= gf66 &&
                             x.xcuenta >= xci66 && x.xcuenta <= xcf66 && x.subcuenta >= sci66 &&
                             x.subcuenta <= scf66 /* && x.cntpuc_numero <= wcuentaini*/);
                    predicateMovAnd = predicateMovAnd.And(
                        x => x.clase >= ci66 && x.clase <= cf66 && x.grupo >= gi66 && x.grupo <= gf66 &&
                             x.xcuenta >= xci66 && x.xcuenta <= xcf66 && x.subcuenta >= sci66 &&
                             x.subcuenta <= scf66 /* && x.cntpuc_numero <= wcuentaini*/);
                    paso = paso + "->5>5-";
                }

                if (Convert.ToString(wcuentaini).Length >= 6 && Convert.ToString(wcuentafin).Length > 6)
                {
                    #region Predicate cuentas

                    string desde6 = Convert.ToString(wcuentaini);
                    string hasta6 = Convert.ToString(wcuentafin);
                    List<string> rango6 = (from p in db.vw_cuentas_valores
                                           where p.cntpuc_numero.Substring(0, 12).CompareTo(desde6.Substring(0, 12)) >= 0 &&
                                                 p.cntpuc_numero.Substring(0, 12).CompareTo(hasta6.Substring(0, 12)) <= 0
                                           orderby p.cntpuc_numero, p.clase, p.grupo, p.cuenta, p.subcuenta
                                           select p.cntpuc_numero).ToList();
                    predicate = predicate.And(p => 1 == 1 && rango6.Contains(p.cntpuc_numero));
                    //  var ConsultaRnago = db.vw_cuentas_valores.Where(predicateMovAnd2).ToList();

                    #endregion

                    paso = paso + "-1>6-";
                }

                #endregion

                // predicate = predicate.And(predicate2);
                paso = paso + "-";
            }

            #region Nit

            if (widtercero != null)
            {
                predicate = predicate.And(x => x.nit == widtercero);
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
                        predicate2 = predicate2.Or(x => x.centro == cen);
                        predicateMovOr = predicateMovOr.Or(x => x.centro == cen);
                    }

                    predicate = predicate.And(predicate2);
                    predicateMovAnd = predicateMovAnd.And(predicateMovOr);
                }
            }

            // predicate = predicate.And(predicate2);
            predicate = predicate.And(x =>
                x.saldo_ini + x.debito + x.credito + x.saldo_ininiff + x.debitoniff + x.creditoniff != 0);
            predicate = predicate.And(x => x.clase > 0);

            #endregion

            #region Tipos Documentos

            if (VectorTipos.Count() > 0)
            {
                if (VectorTipos[0] != "null")
                {
                    foreach (string j in VectorTipos)
                    {
                        int tip = Convert.ToInt32(j);
                        //predicate2 = predicate2.Or(x => x.td_num  == tip);
                        //predicateMovOr = predicateMovOr.Or(x => x.td_num == tip);
                    }
                }
            }
            //predicate = predicate.And(predicate2);
            //predicateMovAnd = predicateMovAnd.And(predicateMovOr);

            string wpaso = paso;

            #endregion

            #endregion

            //*******************

            #region Tercero

            if (widtercero != null)
            {
                var buscarDatosTercero = (from ter in db.icb_terceros
                                          where ter.tercero_id == widtercero
                                          select new
                                          {
                                              id = ter.tercero_id,
                                              docu = ter.doc_tercero,
                                              nombre = ter.prinom_tercero + " " + ter.apellido_tercero,
                                              correo = ter.email_tercero
                                              // doc.fecha.ToString(),
                                              //numeroRegistro = doc.numero,
                                              //nombreBodegas = bodegacs.bodccs_nombre,
                                              //notas = doc.notas,
                                              //tipoEntrada = t.tpdoc_nombre,
                                              //TotalTotales = 0,
                                          }).FirstOrDefault();
            }

            #endregion

            ////////////////// nuevo para Mov_contable

            #region validacion de cuentas

            string desde = Convert.ToString(wcuentaini);
            if (string.IsNullOrWhiteSpace(desde))
            {
                desde = (from c in db.mov_contable
                         join p in db.cuenta_puc
                             on c.cuenta equals p.cntpuc_id
                         where p.esafectable
                         orderby c.cuenta
                         select p.cntpuc_numero).FirstOrDefault();
            }

            string hasta = Convert.ToString(wcuentafin);
            if (string.IsNullOrWhiteSpace(hasta))
            {
                hasta = (from c in db.mov_contable
                         join p in db.cuenta_puc
                             on c.cuenta equals p.cntpuc_id
                         where p.esafectable
                         orderby c.cuenta descending
                         select p.cntpuc_numero).FirstOrDefault();
            }

            #endregion

            int ini = (from c in db.cuenta_puc
                       where c.cntpuc_numero == desde
                       orderby c.cuenta
                       select c.cntpuc_id).FirstOrDefault();

            int fin = (from c in db.cuenta_puc
                       where c.cntpuc_numero == hasta
                       orderby c.cuenta
                       select c.cntpuc_id).FirstOrDefault();


            List<int> rango = (from p in db.cuenta_puc
                               where p.cntpuc_numero.Substring(0, 12).CompareTo(desde.Substring(0, 12)) >= 0 &&
                                     p.cntpuc_numero.Substring(0, 12).CompareTo(hasta.Substring(0, 12)) <= 0 && p.esafectable
                               orderby p.clase, p.grupo, p.cuenta, p.subcuenta
                               select p.cntpuc_id).ToList();


            #region Declaro los predicate a utilizar

            System.Linq.Expressions.Expression<Func<vw_mov_contables, bool>> anioP = PredicateBuilder.True<vw_mov_contables>();
            System.Linq.Expressions.Expression<Func<vw_mov_contables, bool>> nitP = PredicateBuilder.False<vw_mov_contables>();
            System.Linq.Expressions.Expression<Func<vw_mov_contables, bool>> centroP = PredicateBuilder.False<vw_mov_contables>();
            System.Linq.Expressions.Expression<Func<vw_mov_contables, bool>> cuentaP = PredicateBuilder.False<vw_mov_contables>();

            anioP = anioP.And(x => x.fec.Year == anio);
            anioP = anioP.And(x => x.fec.Month == mes);

            #region Predicate cuentas

            anioP = anioP.And(p => 1 == 1 && rango.Contains(p.cuenta));

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

            List<vw_mov_contables> consulta = db.vw_mov_contables.Where(anioP).ToList();

            List<vw_mov_contables> consulta_mov = db.vw_mov_contables.Where(predicateMovAnd).ToList();

            ///////////////// para Mov-Contabñe
            if (checkNiff == false)
            {
                predicate = predicate.And(x => x.saldo_ini + x.debito + x.credito != 0);
            }

            if (checkNiff)
            {
                predicate = predicate.And(x => x.saldo_ininiff + x.debitoniff + x.creditoniff != 0);
            }

            string root = Server.MapPath("~/Pdf/");
            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
            string path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);


            List<vw_cuentas_valores> movt = db.vw_cuentas_valores.Where(predicate).ToList();


            #region Movimientos agrupados por cuenta 

            List<detalleCuenta> detalleTransacciones = movt.GroupBy(am => new { am.cuenta }).OrderBy(p => p.Key.cuenta).Select(f =>
                  new detalleCuenta
                  {
                      id_cuenta_det = f.Key.cuenta,
                      clase_cuenta_det = f.Select(g => g.clase ?? 0).First(),
                      grupo_cuenta_det = f.Select(g => g.grupo ?? 0).First(),
                      xcuenta_cuenta_det = f.Select(g => g.xcuenta ?? 0).First(),
                      subcuenta_cuenta_det = f.Select(g => g.subcuenta ?? 0).First(),
                      numero_cuenta_det = f.Select(g => g.cntpuc_numero).First(), //f.Key.cntpuc_numero,
                      nombre_cuenta_det = f.Select(g => g.cntpuc_descp).First(),
                      saldoini_det = f.Sum(fc => fc.saldo_ini),
                      debito_det = f.Sum(fc => fc.debito),
                      credito_det = f.Sum(fc => fc.credito),
                      saldo_det = f.Sum(fc => fc.saldo_ini - (fc.debito - fc.credito)),
                      saldoininiff_det = f.Sum(fc => fc.saldo_ininiff),
                      debitoniff_det = f.Sum(fc => fc.debitoniff),
                      creditoniff_det = f.Sum(fc => fc.creditoniff),
                      saldoniff_det = f.Sum(fc => fc.saldo_ininiff - (fc.debitoniff - fc.creditoniff))
                  }).ToList();

            #endregion

            #region Movimientos agrupados por clase 

            List<detalleCuenta> det_trans_agrupado_por_Cuenta = movt.OrderBy(f => f.clase).OrderBy(f => f.grupo).OrderBy(f => f.xcuenta)
                .OrderBy(f => f.subcuenta).GroupBy(f => new { f.cuenta }).Select(f => new detalleCuenta
                {
                    id_cuenta_det = f.Key.cuenta,
                    numero_cuenta_det = f.Select(fc => fc.cntpuc_numero).First(),
                    nombre_cuenta_det = f.Select(fc => fc.cntpuc_descp).First(),
                    clase_cuenta_det = f.Select(fc => fc.clase ?? 0).First(),
                    grupo_cuenta_det = f.Select(fc => fc.grupo ?? 0).First(),
                    xcuenta_cuenta_det = f.Select(fc => fc.xcuenta ?? 0).First(),
                    subcuenta_cuenta_det = f.Select(fc => fc.subcuenta ?? 0).First(),
                    saldoini_det = f.Sum(fc => fc.saldo_ini),
                    debito_det = f.Sum(fc => fc.debito),
                    credito_det = f.Sum(fc => fc.credito),
                    saldo_det = f.Sum(fc => fc.saldo_ini - (fc.debito - fc.credito)),
                    saldoininiff_det = f.Sum(fc => fc.saldo_ininiff),
                    debitoniff_det = f.Sum(fc => fc.debitoniff),
                    creditoniff_det = f.Sum(fc => fc.creditoniff),
                    saldoniff_det = f.Sum(fc => fc.saldo_ininiff - (fc.debitoniff - fc.creditoniff))
                }).ToList();

            #endregion

            #region Mov agrup clase2 

            List<detalleCuenta> det_trans_agrupado_por_Clase = movt.GroupBy(am => new { am.clase }).Select(f => new detalleCuenta
            {
                // id_cuenta_det = am.key.cntpuc_numero,
                numero_cuenta_det = f.Select(fc => fc.cntpuc_numero).First(),
                nombre_cuenta_det = f.Select(fc => fc.cntpuc_descp).First(),
                clase_cuenta_det = f.Select(fc => fc.clase ?? 0).First(),
                grupo_cuenta_det = f.Select(fc => fc.grupo ?? 0).First(),
                xcuenta_cuenta_det = f.Select(fc => fc.xcuenta ?? 0).First(),
                subcuenta_cuenta_det = f.Select(fc => fc.subcuenta ?? 0).First(),
                saldoini_det = f.Sum(fc => fc.saldo_ini),
                debito_det = f.Sum(fc => fc.debito),
                credito_det = f.Sum(fc => fc.credito),
                saldo_det = f.Sum(fc => fc.saldo_ini - (fc.debito - fc.credito)),
                saldoininiff_det = f.Sum(fc => fc.saldo_ininiff),
                debitoniff_det = f.Sum(fc => fc.debitoniff),
                creditoniff_det = f.Sum(fc => fc.creditoniff),
                saldoniff_det = f.Sum(fc => fc.saldo_ininiff - (fc.debitoniff - fc.creditoniff))
            }).ToList();

            #endregion

            #region Movimientos Agrupados por Nit

            List<detalleCuenta> det_trans_agrupado_por_nit = movt.GroupBy(am => new { am.nit }).Select(f => new detalleCuenta
            {
                tercero_det = f.Select(fc => fc.nit).First(),
                tercero_documento_det = f.Select(fc => fc.doc_tercero).First(),
                tercero_nombre_det = f.Select(fc =>
                    fc.prinom_tercero + " " + fc.segnom_tercero + " " + fc.apellido_tercero + " " +
                    fc.segapellido_tercero).First(),
                numero_cuenta_det = f.Select(fc => fc.cntpuc_numero).First(),
                nombre_cuenta_det = f.Select(fc => fc.cntpuc_descp).First(),
                clase_cuenta_det = f.Select(fc => fc.clase ?? 0).First(),
                grupo_cuenta_det = f.Select(fc => fc.grupo ?? 0).First(),
                xcuenta_cuenta_det = f.Select(fc => fc.xcuenta ?? 0).First(),
                subcuenta_cuenta_det = f.Select(fc => fc.subcuenta ?? 0).First(),
                saldoini_det = f.Sum(fc => fc.saldo_ini),
                debito_det = f.Sum(fc => fc.debito),
                credito_det = f.Sum(fc => fc.credito),
                saldo_det = f.Sum(fc => fc.saldo_ini - (fc.debito - fc.credito)),
                saldoininiff_det = f.Sum(fc => fc.saldo_ininiff),
                debitoniff_det = f.Sum(fc => fc.debitoniff),
                creditoniff_det = f.Sum(fc => fc.creditoniff),
                saldoniff_det = f.Sum(fc => fc.saldo_ininiff - (fc.debitoniff - fc.creditoniff))
            }).ToList();

            #endregion

            #region Mov agrup subcuenta

            List<detalleCuenta> subcuenta = movt.GroupBy(am => new { am.n_subcuenta }).Select(f => new detalleCuenta
            {
                // id_cuenta_det = am.key.cntpuc_numero,
                numero_cuenta_det = f.Select(fc => fc.cntpuc_numero).First(),
                nombre_cuenta_det = f.Select(fc => fc.cntpuc_descp).First(),
                clase_cuenta_det = f.Select(fc => fc.clase ?? 0).First(),
                grupo_cuenta_det = f.Select(fc => fc.grupo ?? 0).First(),
                xcuenta_cuenta_det = f.Select(fc => fc.xcuenta ?? 0).First(),
                subcuenta_cuenta_det = f.Select(fc => fc.subcuenta ?? 0).First(),
                saldoini_det = f.Sum(fc => fc.saldo_ini),
                debito_det = f.Sum(fc => fc.debito),
                credito_det = f.Sum(fc => fc.credito),
                saldo_det = f.Sum(fc => fc.saldo_ini - (fc.debito - fc.credito)),
                saldoininiff_det = f.Sum(fc => fc.saldo_ininiff),
                debitoniff_det = f.Sum(fc => fc.debitoniff),
                creditoniff_det = f.Sum(fc => fc.creditoniff),
                saldoniff_det = f.Sum(fc => fc.saldo_ininiff - (fc.debitoniff - fc.creditoniff))
            }).ToList();

            #endregion

            #region Mov agrup cuenta

            List<detalleCuenta> cuenta = movt.GroupBy(am => new { am.n_cuenta }).Select(f => new detalleCuenta
            {
                // id_cuenta_det = am.key.cntpuc_numero,
                numero_cuenta_det = f.Select(fc => fc.cntpuc_numero).First(),
                nombre_cuenta_det = f.Select(fc => fc.cntpuc_descp).First(),
                clase_cuenta_det = f.Select(fc => fc.clase ?? 0).First(),
                grupo_cuenta_det = f.Select(fc => fc.grupo ?? 0).First(),
                xcuenta_cuenta_det = f.Select(fc => fc.xcuenta ?? 0).First(),
                subcuenta_cuenta_det = f.Select(fc => fc.subcuenta ?? 0).First(),
                saldoini_det = f.Sum(fc => fc.saldo_ini),
                debito_det = f.Sum(fc => fc.debito),
                credito_det = f.Sum(fc => fc.credito),
                saldo_det = f.Sum(fc => fc.saldo_ini - (fc.debito - fc.credito)),
                saldoininiff_det = f.Sum(fc => fc.saldo_ininiff),
                debitoniff_det = f.Sum(fc => fc.debitoniff),
                creditoniff_det = f.Sum(fc => fc.creditoniff),
                saldoniff_det = f.Sum(fc => fc.saldo_ininiff - (fc.debitoniff - fc.creditoniff))
            }).ToList();

            #endregion

            #region Mov agrup grupo

            List<detalleCuenta> grupo = movt.GroupBy(am => new { am.n_grupo }).Select(f => new detalleCuenta
            {
                // id_cuenta_det = am.key.cntpuc_numero,
                numero_cuenta_det = f.Select(fc => fc.cntpuc_numero).First(),
                nombre_cuenta_det = f.Select(fc => fc.cntpuc_descp).First(),
                clase_cuenta_det = f.Select(fc => fc.clase ?? 0).First(),
                grupo_cuenta_det = f.Select(fc => fc.grupo ?? 0).First(),
                xcuenta_cuenta_det = f.Select(fc => fc.xcuenta ?? 0).First(),
                subcuenta_cuenta_det = f.Select(fc => fc.subcuenta ?? 0).First(),
                saldoini_det = f.Sum(fc => fc.saldo_ini),
                debito_det = f.Sum(fc => fc.debito),
                credito_det = f.Sum(fc => fc.credito),
                saldo_det = f.Sum(fc => fc.saldo_ini - (fc.debito - fc.credito)),
                saldoininiff_det = f.Sum(fc => fc.saldo_ininiff),
                debitoniff_det = f.Sum(fc => fc.debitoniff),
                creditoniff_det = f.Sum(fc => fc.creditoniff),
                saldoniff_det = f.Sum(fc => fc.saldo_ininiff - (fc.debitoniff - fc.creditoniff))
            }).ToList();

            #endregion


            #region MODELO PARA MOVIMIENTOS A PARTIR DE LA CLASE

            ///***** FUNCIONA DEFINITIVA MOVIMIENTOS AGRUPADOS - INICIO
            List<movimientoagrupado01> movimientoagrupadocuenta = movt.GroupBy(d => new { d.clase }).Select(d => new movimientoagrupado01
            {
                clase = d.Key.clase ?? 0,
                nom_clase = d.Select(q => q.nom_clase).First(),
                // cuenta = d.Key.cuenta, 
                numero_cuenta = d.Select(f => f.cntpuc_numero).First(),
                Ctotalsaldoini = d.Sum(f => f.saldo_ini),
                Ctotaldebito = d.Sum(f => f.debito),
                Ctotalcredito = d.Sum(f => f.credito),
                Ctotalsaldo = d.Sum(f => f.saldo_ini - (f.debito - f.credito)),
                Ctotalsaldoininiff = d.Sum(f => f.saldo_ininiff),
                Ctotaldebitoniff = d.Sum(f => f.debitoniff),
                Ctotalcreditoniff = d.Sum(f => f.creditoniff),
                Ctotalsaldoniff = d.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
                agrupa_grupo = d.GroupBy(gg => new { gg.grupo }).Select(gg => new agrupa_grupo
                {
                    grupo = gg.Key.grupo ?? 0,
                    nom_grupo = gg.Select(g => g.nom_grupo).First(),
                    grupo_t_saldoini = gg.Sum(f => f.saldo_ini),
                    grupo_t_debito = gg.Sum(f => f.debito),
                    grupo_t_credito = gg.Sum(f => f.credito),
                    grupo_t_saldo = gg.Sum(f => f.saldo_ini - (f.debito - f.credito)),
                    grupo_t_saldoininiff = gg.Sum(f => f.saldo_ininiff),
                    grupo_t_debitoniff = gg.Sum(f => f.debitoniff),
                    grupo_t_creditoniff = gg.Sum(f => f.creditoniff),
                    grupo_t_saldoniff = gg.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
                    agrupa_cuenta = gg.GroupBy(cc => new { cc.xcuenta }).Select(cc => new agrupa_cuenta
                    {
                        cuenta = cc.Key.xcuenta ?? 0,
                        nom_cuenta = cc.Select(g => g.nom_cuenta).First(),
                        cuenta_t_saldoini = cc.Sum(f => f.saldo_ini),
                        cuenta_t_debito = cc.Sum(f => f.debito),
                        cuenta_t_credito = cc.Sum(f => f.credito),
                        cuenta_t_saldo = cc.Sum(f => f.saldo_ini - (f.debito - f.credito)),
                        cuenta_t_saldoininiff = cc.Sum(f => f.saldo_ininiff),
                        cuenta_t_debitoniff = cc.Sum(f => f.debitoniff),
                        cuenta_t_creditoniff = cc.Sum(f => f.creditoniff),
                        cuenta_t_saldoniff = cc.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
                        agrupa_subcuenta = cc.GroupBy(sc => new { sc.subcuenta }).Select(sc => new agrupa_subcuenta
                        {
                            subcuenta = sc.Key.subcuenta ?? 0,
                            nom_subcuenta = sc.Select(g => g.nom_cuenta).First(),
                            subcuenta_t_saldoini = sc.Sum(f => f.saldo_ini),
                            subcuenta_t_debito = sc.Sum(f => f.debito),
                            subcuenta_t_credito = sc.Sum(f => f.credito),
                            subcuenta_t_saldo = sc.Sum(f => f.saldo_ini - (f.debito - f.credito)),
                            subcuenta_t_saldoininiff = sc.Sum(f => f.saldo_ininiff),
                            subcuenta_t_debitoniff = sc.Sum(f => f.debitoniff),
                            subcuenta_t_creditoniff = sc.Sum(f => f.creditoniff),
                            subcuenta_t_saldoniff = sc.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
                            detalleCuenta = sc.GroupBy(am => new { am.cuenta }).Select(f => new detalleCuenta
                            {
                                id_cuenta_det = f.Key.cuenta,
                                clase_cuenta_det = f.Select(g => g.clase ?? 0).First(),
                                grupo_cuenta_det = f.Select(g => g.grupo ?? 0).First(),
                                xcuenta_cuenta_det = f.Select(g => g.xcuenta ?? 0).First(),
                                subcuenta_cuenta_det = f.Select(g => g.subcuenta ?? 0).First(),
                                numero_cuenta_det = f.Select(g => g.cntpuc_numero).First(), //f.Key.cntpuc_numero,
                                nombre_cuenta_det = f.Select(g => g.cntpuc_descp).First(),
                                saldoini_det = f.Sum(fc => fc.saldo_ini),
                                debito_det = f.Sum(fc => fc.debito),
                                credito_det = f.Sum(fc => fc.credito),
                                saldo_det = f.Sum(fc => fc.saldo_ini - (fc.debito - fc.credito)),
                                saldoininiff_det = f.Sum(fc => fc.saldo_ininiff),
                                debitoniff_det = f.Sum(fc => fc.debitoniff),
                                creditoniff_det = f.Sum(fc => fc.creditoniff),
                                saldoniff_det = f.Sum(fc => fc.saldo_ininiff - (fc.debitoniff - fc.creditoniff))
                            }).ToList()
                        }).ToList()
                    }).ToList()
                }).ToList()
            }).ToList();

            //*****FUNCIONA DEFINITIVA MOVIMIENTOS AGRUPADOS -FIN

            #endregion

            ViewBag.checkMovimiento = checkMovimiento;
            ViewBag.checkNiff = checkNiff;
            ViewBag.checkCentro = checkCentro;
            ViewBag.checkNit = checkNit;

            ViewAsPdf something = new ViewAsPdf("repcontablepdf", "");

            #region checkCentro == false && Ver == false

            //if (checkCentro == false && checkMovimiento == false && Ver == false )
            //{
            //    var salida = new Rotativa.ViewAsPdf("repcontablepdf", "") { };
            //    ViewModelReportepdf reporte = new ViewModelReportepdf()
            //    {
            //        titulo = "Reporte Contable por cuenta pdf",
            //        fechaReporte = DateTime.Now,
            //        fechaInicio = wanioini + "/" + wmesini,
            //        fechafin = waniofin + "/" + wmesfin,
            //        cuentaIni = Convert.ToString(wcuentaini),
            //        cuentaFin = Convert.ToString(wcuentafin),
            //        Grantotalsaldoini = movimientoagrupadocuenta.Sum((f => f.Ctotalsaldoini)),
            //        Grantotaldebito = movimientoagrupadocuenta.Sum((f => f.Ctotaldebito)),
            //        Grantotalcredito = movimientoagrupadocuenta.Sum((f => f.Ctotalcredito)),
            //        Grantotalsaldo = movimientoagrupadocuenta.Sum((f => f.Ctotalsaldo)),
            //        Grantotalsaldoininiff = movimientoagrupadocuenta.Sum((f => f.Ctotalsaldoininiff)),
            //        Grantotaldebitoniff = movimientoagrupadocuenta.Sum((f => f.Ctotaldebitoniff)),
            //        Grantotalcreditoniff = movimientoagrupadocuenta.Sum((f => f.Ctotalcreditoniff)),
            //        Grantotalsaldoniff = movimientoagrupadocuenta.Sum((f => f.Ctotalsaldoniff)),
            //        // movimientosmaestro01 = detalleagrupado_01,
            //        // detalleCuenta = detalleTransacciones,
            //        movimientoagrupado01 = movimientoagrupadocuenta,
            //    };

            //    salida = new Rotativa.ViewAsPdf("repcontablepdf", reporte) { };
            //    something = salida;
            //}

            #endregion


            #region MODELO A PARTIR DE EL CENTRO DE COSTO

            ///*****  MOVIMIENTOS AGRUPADOS - INICIO
            //var movimientoCentro = movt.GroupBy(ct => new { ct.centro }).Select(ct => new centroagrupado
            //{
            //    centro = ct.Key.centro,
            //    nom_centro = ct.Select(q => q.centcst_nombre).First(),
            //    centro_totalsaldoini = ct.Sum(f => f.saldo_ini),
            //    centro_totaldebito = ct.Sum(f => f.debito),
            //    centro_totalcredito = ct.Sum(f => f.credito),
            //    centro_totalsaldo = ct.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //    centro_totalsaldoininiff = ct.Sum(f => f.saldo_ininiff),
            //    centro_totaldebitoniff = ct.Sum(f => f.debitoniff),
            //    centro_totalcreditoniff = ct.Sum(f => f.creditoniff),
            //    centro_totalsaldoniff = ct.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //    clases = ct.GroupBy(d => new { d.clase }).Select(d => new movimientoagrupado01
            //    {
            //        clase = d.Key.clase ?? 0,
            //        nom_clase = d.Select(q => q.nom_clase).First(),
            //        numero_cuenta = d.Select(f => f.cntpuc_numero).First(),
            //        Ctotalsaldoini = d.Sum(f => f.saldo_ini),
            //        Ctotaldebito = d.Sum(f => f.debito),
            //        Ctotalcredito = d.Sum(f => f.credito),
            //        Ctotalsaldo = d.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //        Ctotalsaldoininiff = d.Sum(f => f.saldo_ininiff),
            //        Ctotaldebitoniff = d.Sum(f => f.debitoniff),
            //        Ctotalcreditoniff = d.Sum(f => f.creditoniff),
            //        Ctotalsaldoniff = d.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //        agrupa_grupo = d.GroupBy(gg => new { gg.grupo }).Select(gg => new agrupa_grupo
            //        {
            //            grupo = gg.Key.grupo ?? 0,
            //            nom_grupo = gg.Select(g => g.nom_grupo).First(),
            //            grupo_t_saldoini = gg.Sum(f => f.saldo_ini),
            //            grupo_t_debito = gg.Sum(f => f.debito),
            //            grupo_t_credito = gg.Sum(f => f.credito),
            //            grupo_t_saldo = gg.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //            grupo_t_saldoininiff = gg.Sum(f => f.saldo_ininiff),
            //            grupo_t_debitoniff = gg.Sum(f => f.debitoniff),
            //            grupo_t_creditoniff = gg.Sum(f => f.creditoniff),
            //            grupo_t_saldoniff = gg.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //            agrupa_cuenta = gg.GroupBy(cc => new { cc.xcuenta }).Select(cc => new agrupa_cuenta
            //            {
            //                cuenta = cc.Key.xcuenta ?? 0,
            //                nom_cuenta = cc.Select(g => g.nom_cuenta).First(),
            //                cuenta_t_saldoini = cc.Sum(f => f.saldo_ini),
            //                cuenta_t_debito = cc.Sum(f => f.debito),
            //                cuenta_t_credito = cc.Sum(f => f.credito),
            //                cuenta_t_saldo = cc.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //                cuenta_t_saldoininiff = cc.Sum(f => f.saldo_ininiff),
            //                cuenta_t_debitoniff = cc.Sum(f => f.debitoniff),
            //                cuenta_t_creditoniff = cc.Sum(f => f.creditoniff),
            //                cuenta_t_saldoniff = cc.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //                agrupa_subcuenta = cc.GroupBy(sc => new { sc.subcuenta }).Select(sc => new agrupa_subcuenta
            //                {
            //                    subcuenta = sc.Key.subcuenta ?? 0,
            //                    nom_subcuenta = sc.Select(g => g.nom_subcuenta).First(),
            //                    subcuenta_t_saldoini = sc.Sum(f => f.saldo_ini),
            //                    subcuenta_t_debito = sc.Sum(f => f.debito),
            //                    subcuenta_t_credito = sc.Sum(f => f.credito),
            //                    subcuenta_t_saldo = sc.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //                    subcuenta_t_saldoininiff = sc.Sum(f => f.saldo_ininiff),
            //                    subcuenta_t_debitoniff = sc.Sum(f => f.debitoniff),
            //                    subcuenta_t_creditoniff = sc.Sum(f => f.creditoniff),
            //                    subcuenta_t_saldoniff = sc.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //                    detalleCuenta = sc.GroupBy(am => new { am.cuenta }).Select(f => new detalleCuenta
            //                    {
            //                        id_cuenta_det = f.Key.cuenta,
            //                        clase_cuenta_det = f.Select(g => g.clase ?? 0).First(),
            //                        grupo_cuenta_det = f.Select(g => g.grupo ?? 0).First(),
            //                        xcuenta_cuenta_det = f.Select(g => g.xcuenta ?? 0).First(),
            //                        subcuenta_cuenta_det = f.Select(g => g.subcuenta ?? 0).First(),
            //                        numero_cuenta_det = f.Select(g => g.cntpuc_numero).First(),//f.Key.cntpuc_numero,
            //                        nombre_cuenta_det = f.Select(g => g.cntpuc_descp).First(),
            //                        saldoini_det = f.Sum(fc => fc.saldo_ini),
            //                        debito_det = f.Sum(fc => fc.debito),
            //                        credito_det = f.Sum(fc => fc.credito),
            //                        saldo_det = f.Sum(fc => fc.saldo_ini - (fc.debito - fc.credito)),
            //                        saldoininiff_det = f.Sum(fc => fc.saldo_ininiff),
            //                        debitoniff_det = f.Sum(fc => fc.debitoniff),
            //                        creditoniff_det = f.Sum(fc => fc.creditoniff),
            //                        saldoniff_det = f.Sum(fc => fc.saldo_ininiff - (fc.debitoniff - fc.creditoniff)),
            //                    }).ToList(),
            //                }).ToList(),
            //            }).ToList(),
            //        }).ToList(),
            //    }).ToList(),
            //}).ToList();

            //*****  MOVIMIENTOS AGRUPADOS -FIN

            #endregion

            #region con checkCentro == true && Ver == false

            //if (checkCentro == true && checkMovimiento == false && Ver == false)
            //{
            //    var salida = new Rotativa.ViewAsPdf("repcontablepdf", "") { };
            //    ViewModelReportepdf reporte2 = new ViewModelReportepdf()
            //    {
            //        titulo = "Reporte Contable por Centro pdf",
            //        fechaReporte = DateTime.Now,
            //        fechaInicio = wanioini + "/" + wmesini,
            //        fechafin = waniofin + "/" + wmesfin,
            //        cuentaIni = Convert.ToString(wcuentaini),
            //        cuentaFin = Convert.ToString(wcuentafin),
            //        Grantotalsaldoini = movimientoCentro.Sum((f => f.centro_totalsaldoini)),
            //        Grantotaldebito = movimientoCentro.Sum((f => f.centro_totaldebito)),
            //        Grantotalcredito = movimientoCentro.Sum((f => f.centro_totalcredito)),
            //        Grantotalsaldo = movimientoCentro.Sum((f => f.centro_totalsaldo)),
            //        Grantotalsaldoininiff = movimientoCentro.Sum((f => f.centro_totalsaldoininiff)),
            //        Grantotaldebitoniff = movimientoCentro.Sum((f => f.centro_totaldebitoniff)),
            //        Grantotalcreditoniff = movimientoCentro.Sum((f => f.centro_totalcreditoniff)),
            //        Grantotalsaldoniff = movimientoCentro.Sum((f => f.centro_totalsaldoniff)),
            //        centroagrupado = movimientoCentro,
            //    };

            //    salida = new Rotativa.ViewAsPdf("repcontablepdf", reporte2) { };
            //    something = salida;
            //}

            #endregion

            #region MODELO A PARTIR DE EL NIT

            ///*****  MOVIMIENTOS AGRUPADOS - INICIO
            //var movimientoNit = movt.GroupBy(ct => new { ct.centro }).Select(ct => new centroagrupado
            //{
            //    centro = ct.Key.centro,
            //    nom_centro = ct.Select(q => q.centcst_nombre).First(),
            //    centro_totalsaldoini = ct.Sum(f => f.saldo_ini),
            //    centro_totaldebito = ct.Sum(f => f.debito),
            //    centro_totalcredito = ct.Sum(f => f.credito),
            //    centro_totalsaldo = ct.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //    centro_totalsaldoininiff = ct.Sum(f => f.saldo_ininiff),
            //    centro_totaldebitoniff = ct.Sum(f => f.debitoniff),
            //    centro_totalcreditoniff = ct.Sum(f => f.creditoniff),
            //    centro_totalsaldoniff = ct.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //    clases = ct.GroupBy(d => new { d.clase }).Select(d => new movimientoagrupado01
            //    {
            //        clase = d.Key.clase ?? 0,
            //        nom_clase = d.Select(q => q.nom_clase).First(),
            //        numero_cuenta = d.Select(f => f.cntpuc_numero).First(),
            //        Ctotalsaldoini = d.Sum(f => f.saldo_ini),
            //        Ctotaldebito = d.Sum(f => f.debito),
            //        Ctotalcredito = d.Sum(f => f.credito),
            //        Ctotalsaldo = d.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //        Ctotalsaldoininiff = d.Sum(f => f.saldo_ininiff),
            //        Ctotaldebitoniff = d.Sum(f => f.debitoniff),
            //        Ctotalcreditoniff = d.Sum(f => f.creditoniff),
            //        Ctotalsaldoniff = d.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //        agrupa_grupo = d.GroupBy(gg => new { gg.grupo }).Select(gg => new agrupa_grupo
            //        {
            //            grupo = gg.Key.grupo ?? 0,
            //            nom_grupo = gg.Select(g => g.nom_grupo).First(),
            //            grupo_t_saldoini = gg.Sum(f => f.saldo_ini),
            //            grupo_t_debito = gg.Sum(f => f.debito),
            //            grupo_t_credito = gg.Sum(f => f.credito),
            //            grupo_t_saldo = gg.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //            grupo_t_saldoininiff = gg.Sum(f => f.saldo_ininiff),
            //            grupo_t_debitoniff = gg.Sum(f => f.debitoniff),
            //            grupo_t_creditoniff = gg.Sum(f => f.creditoniff),
            //            grupo_t_saldoniff = gg.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //            agrupa_cuenta = gg.GroupBy(cc => new { cc.xcuenta }).Select(cc => new agrupa_cuenta
            //            {
            //                cuenta = cc.Key.xcuenta ?? 0,
            //                nom_cuenta = cc.Select(g => g.nom_cuenta).First(),
            //                cuenta_t_saldoini = cc.Sum(f => f.saldo_ini),
            //                cuenta_t_debito = cc.Sum(f => f.debito),
            //                cuenta_t_credito = cc.Sum(f => f.credito),
            //                cuenta_t_saldo = cc.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //                cuenta_t_saldoininiff = cc.Sum(f => f.saldo_ininiff),
            //                cuenta_t_debitoniff = cc.Sum(f => f.debitoniff),
            //                cuenta_t_creditoniff = cc.Sum(f => f.creditoniff),
            //                cuenta_t_saldoniff = cc.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //                agrupa_subcuenta = cc.GroupBy(sc => new { sc.subcuenta }).Select(sc => new agrupa_subcuenta
            //                {
            //                    subcuenta = sc.Key.subcuenta ?? 0,
            //                    nom_subcuenta = sc.Select(g => g.nom_subcuenta).First(),
            //                    subcuenta_t_saldoini = sc.Sum(f => f.saldo_ini),
            //                    subcuenta_t_debito = sc.Sum(f => f.debito),
            //                    subcuenta_t_credito = sc.Sum(f => f.credito),
            //                    subcuenta_t_saldo = sc.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //                    subcuenta_t_saldoininiff = sc.Sum(f => f.saldo_ininiff),
            //                    subcuenta_t_debitoniff = sc.Sum(f => f.debitoniff),
            //                    subcuenta_t_creditoniff = sc.Sum(f => f.creditoniff),
            //                    subcuenta_t_saldoniff = sc.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //                    agrupa_nit = sc.GroupBy(an => new { an.nit }).Select(an => new agrupa_nit
            //                    {
            //                        nit = an.Key.nit,
            //                        documento = an.Select(g => g.doc_tercero).First(),
            //                        nombre_nit = an.Select(g => g.prinom_tercero +" "+ g.segnom_tercero +" "+ g.apellido_tercero +" "+ g.segapellido_tercero ).First(),
            //                        nit_t_saldoini = an.Sum(f => f.saldo_ini),
            //                        nit_t_debito = an.Sum(f => f.debito),
            //                        nit_t_credito = an.Sum(f => f.credito),
            //                        nit_t_saldo = an.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //                        nit_t_saldoininiff = an.Sum(f => f.saldo_ininiff),
            //                        nit_t_debitoniff = an.Sum(f => f.debitoniff),
            //                        nit_t_creditoniff = an.Sum(f => f.creditoniff),
            //                        nit_t_saldoniff = an.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //                        detalleCuenta = an.GroupBy(am => new { am.cuenta }).Select(f => new detalleCuenta
            //                            {
            //                                id_cuenta_det = f.Key.cuenta,
            //                                clase_cuenta_det = f.Select(g => g.clase ?? 0).First(),
            //                                grupo_cuenta_det = f.Select(g => g.grupo ?? 0).First(),
            //                                xcuenta_cuenta_det = f.Select(g => g.xcuenta ?? 0).First(),
            //                                subcuenta_cuenta_det = f.Select(g => g.subcuenta ?? 0).First(),
            //                                numero_cuenta_det = f.Select(g => g.cntpuc_numero).First(),//f.Key.cntpuc_numero,
            //                                nombre_cuenta_det = f.Select(g => g.cntpuc_descp).First(),
            //                                saldoini_det = f.Sum(fc => fc.saldo_ini),
            //                                debito_det = f.Sum(fc => fc.debito),
            //                                credito_det = f.Sum(fc => fc.credito),
            //                                saldo_det = f.Sum(fc => fc.saldo_ini - (fc.debito - fc.credito)),
            //                                saldoininiff_det = f.Sum(fc => fc.saldo_ininiff),
            //                                debitoniff_det = f.Sum(fc => fc.debitoniff),
            //                                creditoniff_det = f.Sum(fc => fc.creditoniff),
            //                                saldoniff_det = f.Sum(fc => fc.saldo_ininiff - (fc.debitoniff - fc.creditoniff)),
            //                            }).ToList(),
            //                    }).ToList(),
            //                }).ToList(),
            //            }).ToList(),
            //        }).ToList(),
            //    }).ToList(),
            //}).ToList();

            //*****  MOVIMIENTOS AGRUPADOS -FIN

            #endregion

            #region con checkNit == true Ver == false

            //if (checkNit  == true  && checkMovimiento == false && Ver == false)
            //{
            //    var salida = new Rotativa.ViewAsPdf("repcontablepdf", "") { };
            //    ViewModelReportepdf reporte3 = new ViewModelReportepdf()
            //    {
            //        titulo = "Reporte Contable por Centro pdf",
            //        fechaReporte = DateTime.Now,
            //        fechaInicio = wanioini + "/" + wmesini,
            //        fechafin = waniofin + "/" + wmesfin,
            //        cuentaIni = Convert.ToString(wcuentaini),
            //        cuentaFin = Convert.ToString(wcuentafin),
            //        Grantotalsaldoini = movimientoCentro.Sum((f => f.centro_totalsaldoini)),
            //        Grantotaldebito = movimientoCentro.Sum((f => f.centro_totaldebito)),
            //        Grantotalcredito = movimientoCentro.Sum((f => f.centro_totalcredito)),
            //        Grantotalsaldo = movimientoCentro.Sum((f => f.centro_totalsaldo)),
            //        Grantotalsaldoininiff = movimientoCentro.Sum((f => f.centro_totalsaldoininiff)),
            //        Grantotaldebitoniff = movimientoCentro.Sum((f => f.centro_totaldebitoniff)),
            //        Grantotalcreditoniff = movimientoCentro.Sum((f => f.centro_totalcreditoniff)),
            //        Grantotalsaldoniff = movimientoCentro.Sum((f => f.centro_totalsaldoniff)),
            //        centroagrupado = movimientoNit,
            //    };

            //    salida = new Rotativa.ViewAsPdf("repcontablepdf", reporte3) { };
            //    something = salida;
            //}

            #endregion

            ///************************************************ Cambio solicitado *******************************************
            ///**************************************************************************************************************

            #region 24 de 07 2018

            ///*****  MOVIMIENTOS AGRUPADOS - INICIO
            //var seleccionCentro = movt.GroupBy(sc1 => new { sc1.cuenta }).Select(sc1 => new seleccion_cuenta
            //{
            //    id_cuenta = sc1.Key.cuenta,
            //    numero_cuenta = sc1.Select(q => q.cntpuc_numero).First(),
            //    nombre_cuenta = sc1.Select(q => q.cntpuc_descp).First(),
            //    scuenta_t_saldoini = sc1.Sum(f => f.saldo_ini),
            //    scuenta_t_debito = sc1.Sum(f => f.debito),
            //    scuenta_t_credito = sc1.Sum(f => f.credito),
            //    scuenta_t_saldo = sc1.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //    scuenta_t_saldoininiff = sc1.Sum(f => f.saldo_ininiff),
            //    scuenta_t_debitoniff = sc1.Sum(f => f.debitoniff),
            //    scuenta_t_creditoniff = sc1.Sum(f => f.creditoniff),
            //    scuenta_t_saldoniff = sc1.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //    listacentro = sc1.GroupBy(lc => new { lc.centro }).Select(lc => new listacentro
            //    {
            //        id_centro = lc.Key.centro,
            //        nombre_centro = lc.Select(f => f.centcst_nombre).First(),
            //        scentro_t_saldoini = lc.Sum(f => f.saldo_ini),
            //        scentro_t_debito = lc.Sum(f => f.debito),
            //        scentro_t_credito = lc.Sum(f => f.credito),
            //        scentro_t_saldo = lc.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //        scentro_t_saldoininiff = lc.Sum(f => f.saldo_ininiff),
            //        scentro_t_debitoniff = lc.Sum(f => f.debitoniff),
            //        scentro_t_creditoniff = lc.Sum(f => f.creditoniff),
            //        scentro_t_saldoniff = lc.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //        listaclase = lc.GroupBy(lca => new { lca.clase }).Select(lca => new listaclase
            //        {
            //            clase = lca.Key.clase ?? 0,
            //            nom_clase = lca.Select(f => f.nom_clase).First(),
            //            Ctotalsaldoini = lca.Sum(f => f.saldo_ini),
            //            Ctotaldebito = lca.Sum(f => f.debito),
            //            Ctotalcredito = lca.Sum(f => f.credito),
            //            Ctotalsaldo = lca.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //            Ctotalsaldoininiff = lca.Sum(f => f.saldo_ininiff),
            //            Ctotaldebitoniff = lca.Sum(f => f.debitoniff),
            //            Ctotalcreditoniff = lca.Sum(f => f.creditoniff),
            //            Ctotalsaldoniff = lca.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //            agrupa_grupo = lca.GroupBy(gg => new { gg.grupo }).Select(gg => new agrupa_grupo
            //            {
            //                grupo = gg.Key.grupo ?? 0,
            //                nom_grupo = gg.Select(g => g.nom_grupo).First(),
            //                grupo_t_saldoini = gg.Sum(f => f.saldo_ini),
            //                grupo_t_debito = gg.Sum(f => f.debito),
            //                grupo_t_credito = gg.Sum(f => f.credito),
            //                grupo_t_saldo = gg.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //                grupo_t_saldoininiff = gg.Sum(f => f.saldo_ininiff),
            //                grupo_t_debitoniff = gg.Sum(f => f.debitoniff),
            //                grupo_t_creditoniff = gg.Sum(f => f.creditoniff),
            //                grupo_t_saldoniff = gg.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //                agrupa_cuenta = gg.GroupBy(cc => new { cc.xcuenta }).Select(cc => new agrupa_cuenta
            //                {
            //                    cuenta = cc.Key.xcuenta ?? 0,
            //                    nom_cuenta = cc.Select(g => g.nom_cuenta).First(),
            //                    cuenta_t_saldoini = cc.Sum(f => f.saldo_ini),
            //                    cuenta_t_debito = cc.Sum(f => f.debito),
            //                    cuenta_t_credito = cc.Sum(f => f.credito),
            //                    cuenta_t_saldo = cc.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //                    cuenta_t_saldoininiff = cc.Sum(f => f.saldo_ininiff),
            //                    cuenta_t_debitoniff = cc.Sum(f => f.debitoniff),
            //                    cuenta_t_creditoniff = cc.Sum(f => f.creditoniff),
            //                    cuenta_t_saldoniff = cc.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //                    agrupa_subcuenta = cc.GroupBy(sc => new { sc.subcuenta }).Select(sc => new agrupa_subcuenta
            //                    {
            //                        subcuenta = sc.Key.subcuenta ?? 0,
            //                        nom_subcuenta = sc.Select(g => g.nom_subcuenta).First(),
            //                        subcuenta_t_saldoini = sc.Sum(f => f.saldo_ini),
            //                        subcuenta_t_debito = sc.Sum(f => f.debito),
            //                        subcuenta_t_credito = sc.Sum(f => f.credito),
            //                        subcuenta_t_saldo = sc.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //                        subcuenta_t_saldoininiff = sc.Sum(f => f.saldo_ininiff),
            //                        subcuenta_t_debitoniff = sc.Sum(f => f.debitoniff),
            //                        subcuenta_t_creditoniff = sc.Sum(f => f.creditoniff),
            //                        subcuenta_t_saldoniff = sc.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //                        detalleCuenta = sc.GroupBy(am => new { am.cuenta }).Select(f => new detalleCuenta
            //                        {
            //                            id_cuenta_det = f.Key.cuenta,
            //                            clase_cuenta_det = f.Select(g => g.clase ?? 0).First(),
            //                            grupo_cuenta_det = f.Select(g => g.grupo ?? 0).First(),
            //                            xcuenta_cuenta_det = f.Select(g => g.xcuenta ?? 0).First(),
            //                            subcuenta_cuenta_det = f.Select(g => g.subcuenta ?? 0).First(),
            //                            numero_cuenta_det = f.Select(g => g.cntpuc_numero).First(),//f.Key.cntpuc_numero,
            //                            nombre_cuenta_det = f.Select(g => g.cntpuc_descp).First(),
            //                            saldoini_det = f.Sum(fc => fc.saldo_ini),
            //                            debito_det = f.Sum(fc => fc.debito),
            //                            credito_det = f.Sum(fc => fc.credito),
            //                            saldo_det = f.Sum(fc => fc.saldo_ini - (fc.debito - fc.credito)),
            //                            saldoininiff_det = f.Sum(fc => fc.saldo_ininiff),
            //                            debitoniff_det = f.Sum(fc => fc.debitoniff),
            //                            creditoniff_det = f.Sum(fc => fc.creditoniff),
            //                            saldoniff_det = f.Sum(fc => fc.saldo_ininiff - (fc.debitoniff - fc.creditoniff)),
            //                        }).ToList(),
            //                    }).ToList(),
            //                }).ToList(),
            //            }).ToList(),
            //        }).ToList(),
            //    }).ToList(),
            //}).ToList();

            //*****  MOVIMIENTOS AGRUPADOS -FIN

            #endregion

            #region 26 de 07 2018 ok 

            ///*****  MOVIMIENTOS AGRUPADOS - INICIO
            //var seleccionPrimero = movt.GroupBy(lc1 => new { lc1.clase }).Select(lc1 => new listaclase
            //{
            //    clase = lc1.Key.clase ?? 0,
            //    nom_clase = lc1.Select(f => f.nom_clase).First(),
            //    Ctotalsaldoini = lc1.Sum(f => f.saldo_ini),
            //    Ctotaldebito = lc1.Sum(f => f.debito),
            //    Ctotalcredito = lc1.Sum(f => f.credito),
            //    Ctotalsaldo = lc1.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //    Ctotalsaldoininiff = lc1.Sum(f => f.saldo_ininiff),
            //    Ctotaldebitoniff = lc1.Sum(f => f.debitoniff),
            //    Ctotalcreditoniff = lc1.Sum(f => f.creditoniff),
            //    Ctotalsaldoniff = lc1.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //    agrupa_grupo = lc1.GroupBy(ag => new { ag.grupo }).Select(ag => new agrupa_grupo
            //    {
            //        grupo = ag.Key.grupo ?? 0,
            //        nom_grupo = ag.Select(g => g.nom_grupo).First(),
            //        grupo_t_saldoini = ag.Sum(f => f.saldo_ini),
            //        grupo_t_debito = ag.Sum(f => f.debito),
            //        grupo_t_credito = ag.Sum(f => f.credito),
            //        grupo_t_saldo = ag.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //        grupo_t_saldoininiff = ag.Sum(f => f.saldo_ininiff),
            //        grupo_t_debitoniff = ag.Sum(f => f.debitoniff),
            //        grupo_t_creditoniff = ag.Sum(f => f.creditoniff),
            //        grupo_t_saldoniff = ag.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //        agrupa_cuenta = ag.GroupBy(ac => new { ac.xcuenta }).Select(ac => new agrupa_cuenta
            //        {
            //            cuenta = ac.Key.xcuenta ?? 0,
            //            nom_cuenta = ac.Select(g => g.nom_cuenta).First(),
            //            cuenta_t_saldoini = ac.Sum(f => f.saldo_ini),
            //            cuenta_t_debito = ac.Sum(f => f.debito),
            //            cuenta_t_credito = ac.Sum(f => f.credito),
            //            cuenta_t_saldo = ac.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //            cuenta_t_saldoininiff = ac.Sum(f => f.saldo_ininiff),
            //            cuenta_t_debitoniff = ac.Sum(f => f.debitoniff),
            //            cuenta_t_creditoniff = ac.Sum(f => f.creditoniff),
            //            cuenta_t_saldoniff = ac.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //            agrupa_subcuenta = ac.GroupBy(asc => new { asc.subcuenta }).Select(asc => new agrupa_subcuenta
            //            {
            //                subcuenta = asc.Key.subcuenta ?? 0,
            //                nom_subcuenta = asc.Select(g => g.nom_subcuenta).First(),
            //                subcuenta_t_saldoini = asc.Sum(f => f.saldo_ini),
            //                subcuenta_t_debito = asc.Sum(f => f.debito),
            //                subcuenta_t_credito = asc.Sum(f => f.credito),
            //                subcuenta_t_saldo = asc.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //                subcuenta_t_saldoininiff = asc.Sum(f => f.saldo_ininiff),
            //                subcuenta_t_debitoniff = asc.Sum(f => f.debitoniff),
            //                subcuenta_t_creditoniff = asc.Sum(f => f.creditoniff),
            //                subcuenta_t_saldoniff = asc.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //                listacentro = asc.GroupBy(lc => new { lc.centro }).Select(lc => new listacentro
            //                {
            //                    id_centro = lc.Key.centro,
            //                    nombre_centro = lc.Select(f => f.centcst_nombre).First(),
            //                    scentro_t_saldoini = lc.Sum(f => f.saldo_ini),
            //                    scentro_t_debito = lc.Sum(f => f.debito),
            //                    scentro_t_credito = lc.Sum(f => f.credito),
            //                    scentro_t_saldo = lc.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //                    scentro_t_saldoininiff = lc.Sum(f => f.saldo_ininiff),
            //                    scentro_t_debitoniff = lc.Sum(f => f.debitoniff),
            //                    scentro_t_creditoniff = lc.Sum(f => f.creditoniff),
            //                    scentro_t_saldoniff = lc.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //                    detalleCuenta = lc.GroupBy(dc => new { dc.cntpuc_numero }).Select(dc => new detalleCuenta
            //                    {
            //                        //id_cuenta_det = dc.Key.cuenta,
            //                        numero_cuenta_det = dc.Key.cntpuc_numero,
            //                        clase_cuenta_det = dc.Select(g => g.clase ?? 0).First(),
            //                        grupo_cuenta_det = dc.Select(g => g.grupo ?? 0).First(),
            //                        xcuenta_cuenta_det = dc.Select(g => g.xcuenta ?? 0).First(),
            //                        subcuenta_cuenta_det = dc.Select(g => g.subcuenta ?? 0).First(),
            //                        //numero_cuenta_det = dc.Select(g => g.cntpuc_numero).First(),//f.Key.cntpuc_numero,
            //                        nombre_cuenta_det = dc.Select(g => g.cntpuc_descp).First(),
            //                        saldoini_det = dc.Sum(fc => fc.saldo_ini),
            //                        debito_det = dc.Sum(fc => fc.debito),
            //                        credito_det = dc.Sum(fc => fc.credito),
            //                        saldo_det = dc.Sum(fc => fc.saldo_ini - (fc.debito - fc.credito)),
            //                        saldoininiff_det = dc.Sum(fc => fc.saldo_ininiff),
            //                        debitoniff_det = dc.Sum(fc => fc.debitoniff),
            //                        creditoniff_det = dc.Sum(fc => fc.creditoniff),
            //                        saldoniff_det = dc.Sum(fc => fc.saldo_ininiff - (fc.debitoniff - fc.creditoniff)),
            //                    }).ToList(),
            //                }).ToList(),
            //            }).ToList(),
            //        }).ToList(),
            //    }).ToList(),
            //}).ToList();

            //*****  MOVIMIENTOS AGRUPADOS -FIN

            #endregion

            #region Ejecuta el modelo de Centro + saldo normal y saldo nif

            //if (checkCentro == true  && checkNit == false && checkMovimiento == false && Ver == true)
            //{
            //    var salida = new Rotativa.ViewAsPdf("repcontablepdf", "") { };
            //    ViewModelReportepdf reporte4 = new ViewModelReportepdf()
            //    {
            //        titulo = "xx Reporte Contable por Centro pdf",
            //        fechaReporte = DateTime.Now,
            //        fechaInicio = wanioini + "/" + wmesini,
            //        fechafin = waniofin + "/" + wmesfin,
            //        cuentaIni = Convert.ToString(wcuentaini),
            //        cuentaFin = Convert.ToString(wcuentafin),
            //        Grantotalsaldoini = seleccionPrimero.Sum((f => f.Ctotalsaldoini)),
            //        Grantotaldebito = seleccionPrimero.Sum((f => f.Ctotaldebito)),
            //        Grantotalcredito = seleccionPrimero.Sum((f => f.Ctotalcredito)),
            //        Grantotalsaldo = seleccionPrimero.Sum((f => f.Ctotalsaldo)),
            //        Grantotalsaldoininiff = seleccionPrimero.Sum((f => f.Ctotalsaldoininiff)),
            //        Grantotaldebitoniff = seleccionPrimero.Sum((f => f.Ctotaldebitoniff)),
            //        Grantotalcreditoniff = seleccionPrimero.Sum((f => f.Ctotalcreditoniff)),
            //        Grantotalsaldoniff = seleccionPrimero.Sum((f => f.Ctotalsaldoniff)),
            //        //seleccion_cuenta = seleccionCentro,
            //        listaclase = seleccionPrimero,
            //    };

            //    salida = new Rotativa.ViewAsPdf("repcontablepdf", reporte4) { };
            //    something = salida;
            //    Ver = true;
            //}

            #endregion


            #region 26 de 07 2018 orig 

            /////*****  MOVIMIENTOS AGRUPADOS - INICIO
            //var seleccionSegundo = movt.GroupBy(lc1 => new { lc1.clase }).Select(lc1 => new listaclase
            //{
            //    clase = lc1.Key.clase ?? 0,
            //    nom_clase = lc1.Select(f => f.nom_clase).First(),
            //    Ctotalsaldoini = lc1.Sum(f => f.saldo_ini),
            //    Ctotaldebito = lc1.Sum(f => f.debito),
            //    Ctotalcredito = lc1.Sum(f => f.credito),
            //    Ctotalsaldo = lc1.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //    Ctotalsaldoininiff = lc1.Sum(f => f.saldo_ininiff),
            //    Ctotaldebitoniff = lc1.Sum(f => f.debitoniff),
            //    Ctotalcreditoniff = lc1.Sum(f => f.creditoniff),
            //    Ctotalsaldoniff = lc1.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //    agrupa_grupo = lc1.GroupBy(ag => new { ag.grupo }).Select(ag => new agrupa_grupo
            //    {
            //        grupo = ag.Key.grupo ?? 0,
            //        nom_grupo = ag.Select(g => g.nom_grupo).First(),
            //        grupo_t_saldoini = ag.Sum(f => f.saldo_ini),
            //        grupo_t_debito = ag.Sum(f => f.debito),
            //        grupo_t_credito = ag.Sum(f => f.credito),
            //        grupo_t_saldo = ag.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //        grupo_t_saldoininiff = ag.Sum(f => f.saldo_ininiff),
            //        grupo_t_debitoniff = ag.Sum(f => f.debitoniff),
            //        grupo_t_creditoniff = ag.Sum(f => f.creditoniff),
            //        grupo_t_saldoniff = ag.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //        agrupa_cuenta = ag.GroupBy(ac => new { ac.xcuenta }).Select(ac => new agrupa_cuenta
            //        {
            //            cuenta = ac.Key.xcuenta ?? 0,
            //            nom_cuenta = ac.Select(g => g.nom_cuenta).First(),
            //            cuenta_t_saldoini = ac.Sum(f => f.saldo_ini),
            //            cuenta_t_debito = ac.Sum(f => f.debito),
            //            cuenta_t_credito = ac.Sum(f => f.credito),
            //            cuenta_t_saldo = ac.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //            cuenta_t_saldoininiff = ac.Sum(f => f.saldo_ininiff),
            //            cuenta_t_debitoniff = ac.Sum(f => f.debitoniff),
            //            cuenta_t_creditoniff = ac.Sum(f => f.creditoniff),
            //            cuenta_t_saldoniff = ac.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //            agrupa_subcuenta = ac.GroupBy(asc => new { asc.subcuenta }).Select(asc => new agrupa_subcuenta
            //            {
            //                subcuenta = asc.Key.subcuenta ?? 0,
            //                nom_subcuenta = asc.Select(g => g.nom_cuenta).First(),
            //                subcuenta_t_saldoini = asc.Sum(f => f.saldo_ini),
            //                subcuenta_t_debito = asc.Sum(f => f.debito),
            //                subcuenta_t_credito = asc.Sum(f => f.credito),
            //                subcuenta_t_saldo = asc.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //                subcuenta_t_saldoininiff = asc.Sum(f => f.saldo_ininiff),
            //                subcuenta_t_debitoniff = asc.Sum(f => f.debitoniff),
            //                subcuenta_t_creditoniff = asc.Sum(f => f.creditoniff),
            //                subcuenta_t_saldoniff = asc.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //                listacentro = asc.GroupBy(lc => new { lc.centro }).Select(lc => new listacentro
            //                {
            //                    id_centro = lc.Key.centro,
            //                    codigo_centro = lc.Select(f => f.pre_centcst).First(),
            //                    nombre_centro = lc.Select(f => f.centcst_nombre).First(),
            //                    scentro_t_saldoini = lc.Sum(f => f.saldo_ini),
            //                    scentro_t_debito = lc.Sum(f => f.debito),
            //                    scentro_t_credito = lc.Sum(f => f.credito),
            //                    scentro_t_saldo = lc.Sum(f => f.saldo_ini - (f.debito - f.credito)),
            //                    scentro_t_saldoininiff = lc.Sum(f => f.saldo_ininiff),
            //                    scentro_t_debitoniff = lc.Sum(f => f.debitoniff),
            //                    scentro_t_creditoniff = lc.Sum(f => f.creditoniff),
            //                    scentro_t_saldoniff = lc.Sum(f => f.saldo_ininiff - (f.debitoniff - f.creditoniff)),
            //                    agrupa_nit = lc.GroupBy(an => new { an.nit }).Select(an => new agrupa_nit
            //                    {
            //                        nit = an.Key.nit,
            //                        documento = an.Select(g => g.doc_tercero).First(),
            //                        nombre_nit = an.Select(g => g.prinom_tercero + " " + g.segnom_tercero + " " + g.apellido_tercero + " " + g.segapellido_tercero ).First(),
            //                        nit_t_saldoini = an.Sum(fc => fc.saldo_ini),
            //                        nit_t_debito = an.Sum(fc => fc.debito),
            //                        nit_t_credito = an.Sum(fc => fc.credito),
            //                        nit_t_saldo = an.Sum(fc => fc.saldo_ini - (fc.debito - fc.credito)),
            //                        nit_t_saldoininiff = an.Sum(fc => fc.saldo_ininiff),
            //                        nit_t_debitoniff = an.Sum(fc => fc.debitoniff),
            //                        nit_t_creditoniff = an.Sum(fc => fc.creditoniff),
            //                        nit_t_saldoniff = an.Sum(fc => fc.saldo_ininiff - (fc.debitoniff - fc.creditoniff)),
            //                        detalleCuenta = an.GroupBy(dc => new { dc.cntpuc_numero }).Select(dc => new detalleCuenta
            //                        {
            //                            //id_cuenta_det = dc.Key.cuenta,
            //                            numero_cuenta_det = dc.Key.cntpuc_numero,
            //                            clase_cuenta_det = dc.Select(g => g.clase ?? 0).First(),
            //                            grupo_cuenta_det = dc.Select(g => g.grupo ?? 0).First(),
            //                            xcuenta_cuenta_det = dc.Select(g => g.xcuenta ?? 0).First(),
            //                            subcuenta_cuenta_det = dc.Select(g => g.subcuenta ?? 0).First(),
            //                            //numero_cuenta_det = dc.Select(g => g.cntpuc_numero).First(),//f.Key.cntpuc_numero,
            //                            nombre_cuenta_det = dc.Select(g => g.cntpuc_descp).First(),
            //                            saldoini_det = dc.Sum(fc => fc.saldo_ini),
            //                            debito_det = dc.Sum(fc => fc.debito),
            //                            credito_det = dc.Sum(fc => fc.credito),
            //                            saldo_det = dc.Sum(fc => fc.saldo_ini - (fc.debito - fc.credito)),
            //                            saldoininiff_det = dc.Sum(fc => fc.saldo_ininiff),
            //                            debitoniff_det = dc.Sum(fc => fc.debitoniff),
            //                            creditoniff_det = dc.Sum(fc => fc.creditoniff),
            //                            saldoniff_det = dc.Sum(fc => fc.saldo_ininiff - (fc.debitoniff - fc.creditoniff)),
            //                        }).ToList(),
            //                    }).ToList(),
            //                 }).ToList(),
            //            }).ToList(),
            //        }).ToList(),
            //    }).ToList(),
            //}).ToList();

            ////*****  MOVIMIENTOS AGRUPADOS -FIN

            #endregion

            #region Ejecuta el modelo de Centro con nit (0001 - 0002 EN LA VISTA)

            ///*****  MOVIMIENTOS AGRUPADOS - INICIO
            List<listaclase> seleccionSegundo_02 = movt.GroupBy(lc1 => new { lc1.clase }).Select(lc1 => new listaclase
            {
                clase = lc1.Key.clase ?? 0,
                nom_clase = lc1.Select(f => f.nom_clase).First(),
                Ctotalsaldoini = lc1.Sum(f => f.saldo_ini),
                Ctotaldebito = lc1.Sum(f => f.debito),
                Ctotalcredito = lc1.Sum(f => f.credito),
                Ctotalsaldo = lc1.Sum(f => f.saldo_ini + f.debito - f.credito),
                Ctotalsaldoininiff = lc1.Sum(f => f.saldo_ininiff),
                Ctotaldebitoniff = lc1.Sum(f => f.debitoniff),
                Ctotalcreditoniff = lc1.Sum(f => f.creditoniff),
                Ctotalsaldoniff = lc1.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                agrupa_grupo = lc1.GroupBy(ag => new { ag.grupo }).Select(ag => new agrupa_grupo
                {
                    grupo = ag.Key.grupo ?? 0,
                    nom_grupo = ag.Select(g => g.nom_grupo).First(),
                    grupo_t_saldoini = ag.Sum(f => f.saldo_ini),
                    grupo_t_debito = ag.Sum(f => f.debito),
                    grupo_t_credito = ag.Sum(f => f.credito),
                    grupo_t_saldo = ag.Sum(f => f.saldo_ini + f.debito - f.credito),
                    grupo_t_saldoininiff = ag.Sum(f => f.saldo_ininiff),
                    grupo_t_debitoniff = ag.Sum(f => f.debitoniff),
                    grupo_t_creditoniff = ag.Sum(f => f.creditoniff),
                    grupo_t_saldoniff = ag.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                    agrupa_cuenta = ag.GroupBy(ac => new { ac.xcuenta }).Select(ac => new agrupa_cuenta
                    {
                        cuenta = ac.Key.xcuenta ?? 0,
                        nom_cuenta = ac.Select(g => g.nom_cuenta).First(),
                        cuenta_t_saldoini = ac.Sum(f => f.saldo_ini),
                        cuenta_t_debito = ac.Sum(f => f.debito),
                        cuenta_t_credito = ac.Sum(f => f.credito),
                        cuenta_t_saldo = ac.Sum(f => f.saldo_ini + f.debito - f.credito),
                        cuenta_t_saldoininiff = ac.Sum(f => f.saldo_ininiff),
                        cuenta_t_debitoniff = ac.Sum(f => f.debitoniff),
                        cuenta_t_creditoniff = ac.Sum(f => f.creditoniff),
                        cuenta_t_saldoniff = ac.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                        agrupa_subcuenta = ac.GroupBy(asc => new { asc.subcuenta }).Select(asc => new agrupa_subcuenta
                        {
                            subcuenta = asc.Key.subcuenta ?? 0,
                            nom_subcuenta = asc.Select(g => g.nom_cuenta).First(),
                            subcuenta_t_saldoini = asc.Sum(f => f.saldo_ini),
                            subcuenta_t_debito = asc.Sum(f => f.debito),
                            subcuenta_t_credito = asc.Sum(f => f.credito),
                            subcuenta_t_saldo = asc.Sum(f => f.saldo_ini + f.debito - f.credito),
                            subcuenta_t_saldoininiff = asc.Sum(f => f.saldo_ininiff),
                            subcuenta_t_debitoniff = asc.Sum(f => f.debitoniff),
                            subcuenta_t_creditoniff = asc.Sum(f => f.creditoniff),
                            subcuenta_t_saldoniff = asc.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                            detalleCuenta = asc.GroupBy(dc => new { dc.cuenta }).Select(dc => new detalleCuenta
                            {
                                id_cuenta_det = dc.Key.cuenta,
                                numero_cuenta_det = dc.Select(g => g.cntpuc_numero).First(),
                                clase_cuenta_det = dc.Select(g => g.clase ?? 0).First(),
                                grupo_cuenta_det = dc.Select(g => g.grupo ?? 0).First(),
                                xcuenta_cuenta_det = dc.Select(g => g.xcuenta ?? 0).First(),
                                subcuenta_cuenta_det = dc.Select(g => g.subcuenta ?? 0).First(),
                                nombre_cuenta_det = dc.Select(g => g.cntpuc_descp).First(),
                                saldoini_det = dc.Sum(fc => fc.saldo_ini),
                                debito_det = dc.Sum(fc => fc.debito),
                                credito_det = dc.Sum(fc => fc.credito),
                                saldo_det = dc.Sum(f => f.saldo_ini + f.debito - f.credito),
                                saldoininiff_det = dc.Sum(fc => fc.saldo_ininiff),
                                debitoniff_det = dc.Sum(fc => fc.debitoniff),
                                creditoniff_det = dc.Sum(fc => fc.creditoniff),
                                saldoniff_det = dc.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                                listacentro = dc.GroupBy(lc => new { lc.centro }).Select(lc => new listacentro
                                {
                                    id_centro = lc.Key.centro,
                                    codigo_centro = lc.Select(f => f.pre_centcst).First(),
                                    nombre_centro = lc.Select(f => f.centcst_nombre).First(),
                                    scentro_t_saldoini = lc.Sum(f => f.saldo_ini),
                                    scentro_t_debito = lc.Sum(f => f.debito),
                                    scentro_t_credito = lc.Sum(f => f.credito),
                                    scentro_t_saldo = lc.Sum(f => f.saldo_ini + f.debito - f.credito),
                                    scentro_t_saldoininiff = lc.Sum(f => f.saldo_ininiff),
                                    scentro_t_debitoniff = lc.Sum(f => f.debitoniff),
                                    scentro_t_creditoniff = lc.Sum(f => f.creditoniff),
                                    scentro_t_saldoniff = lc.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                                    agrupa_nit = lc.GroupBy(an => new { an.nit }).Select(an => new agrupa_nit
                                    {
                                        nit = an.Key.nit,
                                        documento = an.Select(g => g.doc_tercero).First(),
                                        nombre_nit = an.Select(g => g.NombreX).First(),
                                        nit_t_saldoini = an.Sum(fc => fc.saldo_ini),
                                        nit_t_debito = an.Sum(fc => fc.debito),
                                        nit_t_credito = an.Sum(fc => fc.credito),
                                        nit_t_saldo = an.Sum(f => f.saldo_ini + f.debito - f.credito),
                                        nit_t_saldoininiff = an.Sum(fc => fc.saldo_ininiff),
                                        nit_t_debitoniff = an.Sum(fc => fc.debitoniff),
                                        nit_t_creditoniff = an.Sum(fc => fc.creditoniff),
                                        nit_t_saldoniff = an.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff)
                                    }).OrderBy(c => c.nit).ToList()
                                }).OrderBy(c => c.id_centro).ToList()
                            }).OrderBy(c => c.id_cuenta_det).ToList()
                        }).OrderBy(c => c.subcuenta).ToList()
                    }).OrderBy(c => c.cuenta).ToList()
                }).OrderBy(c => c.grupo).ToList()
            }).OrderBy(c => c.clase).ToList();

            //*****  MOVIMIENTOS AGRUPADOS -FIN

            #endregion

            #region Ejecuta el modelo de Centro con nit (0001 - 0002 EN LA VISTA)

            if (checkCentro && checkNit && checkMovimiento == false && Ver)
            {
                ViewAsPdf salida = new ViewAsPdf("repcontablepdf", "");
                ViewModelReportepdf reporte4 = new ViewModelReportepdf
                {
                    titulo = "Reporte Contable (Centro y Nit )",
                    fechaReporte = DateTime.Now,
                    fechaInicio = Convert.ToString(wanio),
                    fechafin = Convert.ToString(wmes),
                    //fechaInicio = wanioini + "/" + wmesini,
                    //fechafin = waniofin + "/" + wmesfin,
                    cuentaIni = Convert.ToString(wcuentaini),
                    cuentaFin = Convert.ToString(wcuentafin),
                    Grantotalsaldoini = seleccionSegundo_02.Sum(f => f.Ctotalsaldoini),
                    Grantotaldebito = seleccionSegundo_02.Sum(f => f.Ctotaldebito),
                    Grantotalcredito = seleccionSegundo_02.Sum(f => f.Ctotalcredito),
                    Grantotalsaldo = seleccionSegundo_02.Sum(f => f.Ctotalsaldo),
                    Grantotalsaldoininiff = seleccionSegundo_02.Sum(f => f.Ctotalsaldoininiff),
                    Grantotaldebitoniff = seleccionSegundo_02.Sum(f => f.Ctotaldebitoniff),
                    Grantotalcreditoniff = seleccionSegundo_02.Sum(f => f.Ctotalcreditoniff),
                    Grantotalsaldoniff = seleccionSegundo_02.Sum(f => f.Ctotalsaldoniff),
                    //seleccion_cuenta = seleccionCentro,
                    listaclase = seleccionSegundo_02
                };

                salida = new ViewAsPdf("repcontablepdf", reporte4);
                something = salida;
                Ver = true;
            }

            #endregion

            #region Ejecuta el modelo de Centro Sin Nit (0003 - 0004 EN LA VISTA)

            ///*****  MOVIMIENTOS AGRUPADOS - INICIO
            List<listaclase> seleccionSegundo_03 = movt.GroupBy(lc1 => new { lc1.clase }).Select(lc1 => new listaclase
            {
                clase = lc1.Key.clase ?? 0,
                nom_clase = lc1.Select(f => f.nom_clase).First(),
                Ctotalsaldoini = lc1.Sum(f => f.saldo_ini),
                Ctotaldebito = lc1.Sum(f => f.debito),
                Ctotalcredito = lc1.Sum(f => f.credito),
                Ctotalsaldo = lc1.Sum(f => f.saldo_ini + f.debito - f.credito),
                Ctotalsaldoininiff = lc1.Sum(f => f.saldo_ininiff),
                Ctotaldebitoniff = lc1.Sum(f => f.debitoniff),
                Ctotalcreditoniff = lc1.Sum(f => f.creditoniff),
                Ctotalsaldoniff = lc1.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                agrupa_grupo = lc1.GroupBy(ag => new { ag.grupo }).Select(ag => new agrupa_grupo
                {
                    grupo = ag.Key.grupo ?? 0,
                    nom_grupo = ag.Select(g => g.nom_grupo).First(),
                    grupo_t_saldoini = ag.Sum(f => f.saldo_ini),
                    grupo_t_debito = ag.Sum(f => f.debito),
                    grupo_t_credito = ag.Sum(f => f.credito),
                    grupo_t_saldo = ag.Sum(f => f.saldo_ini + f.debito - f.credito),
                    grupo_t_saldoininiff = ag.Sum(f => f.saldo_ininiff),
                    grupo_t_debitoniff = ag.Sum(f => f.debitoniff),
                    grupo_t_creditoniff = ag.Sum(f => f.creditoniff),
                    grupo_t_saldoniff = ag.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                    agrupa_cuenta = ag.GroupBy(ac => new { ac.xcuenta }).Select(ac => new agrupa_cuenta
                    {
                        cuenta = ac.Key.xcuenta ?? 0,
                        nom_cuenta = ac.Select(g => g.nom_cuenta).First(),
                        cuenta_t_saldoini = ac.Sum(f => f.saldo_ini),
                        cuenta_t_debito = ac.Sum(f => f.debito),
                        cuenta_t_credito = ac.Sum(f => f.credito),
                        cuenta_t_saldo = ac.Sum(f => f.saldo_ini + f.debito - f.credito),
                        cuenta_t_saldoininiff = ac.Sum(f => f.saldo_ininiff),
                        cuenta_t_debitoniff = ac.Sum(f => f.debitoniff),
                        cuenta_t_creditoniff = ac.Sum(f => f.creditoniff),
                        cuenta_t_saldoniff = ac.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                        agrupa_subcuenta = ac.GroupBy(asc => new { asc.subcuenta }).Select(asc => new agrupa_subcuenta
                        {
                            subcuenta = asc.Key.subcuenta ?? 0,
                            nom_subcuenta = asc.Select(g => g.nom_subcuenta).First(),
                            subcuenta_t_saldoini = asc.Sum(f => f.saldo_ini),
                            subcuenta_t_debito = asc.Sum(f => f.debito),
                            subcuenta_t_credito = asc.Sum(f => f.credito),
                            subcuenta_t_saldo = asc.Sum(f => f.saldo_ini + f.debito - f.credito),
                            subcuenta_t_saldoininiff = asc.Sum(f => f.saldo_ininiff),
                            subcuenta_t_debitoniff = asc.Sum(f => f.debitoniff),
                            subcuenta_t_creditoniff = asc.Sum(f => f.creditoniff),
                            subcuenta_t_saldoniff = asc.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                            detalleCuenta = asc.GroupBy(dc => new { dc.cuenta }).Select(dc => new detalleCuenta
                            {
                                id_cuenta_det = dc.Key.cuenta,
                                numero_cuenta_det = dc.Select(g => g.cntpuc_numero).First(),
                                clase_cuenta_det = dc.Select(g => g.clase ?? 0).First(),
                                grupo_cuenta_det = dc.Select(g => g.grupo ?? 0).First(),
                                xcuenta_cuenta_det = dc.Select(g => g.xcuenta ?? 0).First(),
                                subcuenta_cuenta_det = dc.Select(g => g.subcuenta ?? 0).First(),
                                nombre_cuenta_det = dc.Select(g => g.cntpuc_descp).First(),
                                saldoini_det = dc.Sum(fc => fc.saldo_ini),
                                debito_det = dc.Sum(fc => fc.debito),
                                credito_det = dc.Sum(fc => fc.credito),
                                saldo_det = dc.Sum(f => f.saldo_ini + f.debito - f.credito),
                                saldoininiff_det = dc.Sum(fc => fc.saldo_ininiff),
                                debitoniff_det = dc.Sum(fc => fc.debitoniff),
                                creditoniff_det = dc.Sum(fc => fc.creditoniff),
                                saldoniff_det = dc.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                                listacentro = dc.GroupBy(lc => new { lc.centro }).Select(lc => new listacentro
                                {
                                    id_centro = lc.Key.centro,
                                    codigo_centro = lc.Select(f => f.pre_centcst).First(),
                                    nombre_centro = lc.Select(f => f.centcst_nombre).First(),
                                    scentro_t_saldoini = lc.Sum(f => f.saldo_ini),
                                    scentro_t_debito = lc.Sum(f => f.debito),
                                    scentro_t_credito = lc.Sum(f => f.credito),
                                    scentro_t_saldo = lc.Sum(f => f.saldo_ini + f.debito - f.credito),
                                    scentro_t_saldoininiff = lc.Sum(f => f.saldo_ininiff),
                                    scentro_t_debitoniff = lc.Sum(f => f.debitoniff),
                                    scentro_t_creditoniff = lc.Sum(f => f.creditoniff),
                                    scentro_t_saldoniff = lc.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff)
                                    //agrupa_nit = lc.GroupBy(an => new { an.nit }).Select(an => new agrupa_nit
                                    //{
                                    //    nit = an.Key.nit,
                                    //    documento = an.Select(g => g.doc_tercero).First(),
                                    //    nombre_nit = an.Select(g => g.NombreX).First(),
                                    //    nit_t_saldoini = an.Sum(fc => fc.saldo_ini),
                                    //    nit_t_debito = an.Sum(fc => fc.debito),
                                    //    nit_t_credito = an.Sum(fc => fc.credito),
                                    //    nit_t_saldo = an.Sum(f => f.saldo_ini + f.debito - f.credito),
                                    //    nit_t_saldoininiff = an.Sum(fc => fc.saldo_ininiff),
                                    //    nit_t_debitoniff = an.Sum(fc => fc.debitoniff),
                                    //    nit_t_creditoniff = an.Sum(fc => fc.creditoniff),
                                    //    nit_t_saldoniff = an.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                                    //}).OrderBy(c => c.nit).ToList(),
                                }).OrderBy(c => c.id_centro).ToList()
                            }).OrderBy(c => c.id_cuenta_det).ToList()
                        }).OrderBy(c => c.subcuenta).ToList()
                    }).OrderBy(c => c.cuenta).ToList()
                }).OrderBy(c => c.grupo).ToList()
            }).OrderBy(c => c.clase).ToList();

            //*****  MOVIMIENTOS AGRUPADOS -FIN

            #endregion

            #region Ejecuta el modelo de Centro Sin Nit (0003 - 0004 EN LA VISTA)

            if (checkCentro && checkNit == false && checkMovimiento == false && Ver)
            {
                ViewAsPdf salida = new ViewAsPdf("repcontablepdf", "");
                ViewModelReportepdf reporte5 = new ViewModelReportepdf
                {
                    titulo = "Reporte Contable (Solo Centros)",
                    fechaReporte = DateTime.Now,
                    fechaInicio = Convert.ToString(wanio),
                    fechafin = Convert.ToString(wmes),
                    cuentaIni = Convert.ToString(wcuentaini),
                    cuentaFin = Convert.ToString(wcuentafin),
                    Grantotalsaldoini = seleccionSegundo_03.Sum(f => f.Ctotalsaldoini),
                    Grantotaldebito = seleccionSegundo_03.Sum(f => f.Ctotaldebito),
                    Grantotalcredito = seleccionSegundo_03.Sum(f => f.Ctotalcredito),
                    Grantotalsaldo = seleccionSegundo_03.Sum(f => f.Ctotalsaldo),
                    Grantotalsaldoininiff = seleccionSegundo_03.Sum(f => f.Ctotalsaldoininiff),
                    Grantotaldebitoniff = seleccionSegundo_03.Sum(f => f.Ctotaldebitoniff),
                    Grantotalcreditoniff = seleccionSegundo_03.Sum(f => f.Ctotalcreditoniff),
                    Grantotalsaldoniff = seleccionSegundo_03.Sum(f => f.Ctotalsaldoniff),
                    //seleccion_cuenta = seleccionCentro,
                    listaclase = seleccionSegundo_03
                };

                salida = new ViewAsPdf("repcontablepdf", reporte5);
                something = salida;
                Ver = true;
            }

            #endregion


            #region  Ejecuta el modelo (0005 -0006 EN LA VISTA) Sin nada ckeck

            ///*****  MOVIMIENTOS AGRUPADOS - INICIO0
            List<listaclase> seleccionSegundo_04 = movt.GroupBy(lc1 => new { lc1.clase }).Select(lc1 => new listaclase
            {
                clase = lc1.Key.clase ?? 0,
                nom_clase = lc1.Select(f => f.nom_clase).First(),
                Ctotalsaldoini = lc1.Sum(f => f.saldo_ini),
                Ctotaldebito = lc1.Sum(f => f.debito),
                Ctotalcredito = lc1.Sum(f => f.credito),
                Ctotalsaldo = lc1.Sum(f => f.saldo_ini + f.debito - f.credito),
                Ctotalsaldoininiff = lc1.Sum(f => f.saldo_ininiff),
                Ctotaldebitoniff = lc1.Sum(f => f.debitoniff),
                Ctotalcreditoniff = lc1.Sum(f => f.creditoniff),
                Ctotalsaldoniff = lc1.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                agrupa_grupo = lc1.GroupBy(ag => new { ag.grupo }).Select(ag => new agrupa_grupo
                {
                    grupo = ag.Key.grupo ?? 0,
                    nom_grupo = ag.Select(g => g.nom_grupo).First(),
                    grupo_t_saldoini = ag.Sum(f => f.saldo_ini),
                    grupo_t_debito = ag.Sum(f => f.debito),
                    grupo_t_credito = ag.Sum(f => f.credito),
                    grupo_t_saldo = ag.Sum(f => f.saldo_ini + f.debito - f.credito),
                    grupo_t_saldoininiff = ag.Sum(f => f.saldo_ininiff),
                    grupo_t_debitoniff = ag.Sum(f => f.debitoniff),
                    grupo_t_creditoniff = ag.Sum(f => f.creditoniff),
                    grupo_t_saldoniff = ag.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                    agrupa_cuenta = ag.GroupBy(ac => new { ac.xcuenta }).Select(ac => new agrupa_cuenta
                    {
                        cuenta = ac.Key.xcuenta ?? 0,
                        nom_cuenta = ac.Select(g => g.nom_cuenta).First(),
                        cuenta_t_saldoini = ac.Sum(f => f.saldo_ini),
                        cuenta_t_debito = ac.Sum(f => f.debito),
                        cuenta_t_credito = ac.Sum(f => f.credito),
                        cuenta_t_saldo = ac.Sum(f => f.saldo_ini + f.debito - f.credito),
                        cuenta_t_saldoininiff = ac.Sum(f => f.saldo_ininiff),
                        cuenta_t_debitoniff = ac.Sum(f => f.debitoniff),
                        cuenta_t_creditoniff = ac.Sum(f => f.creditoniff),
                        cuenta_t_saldoniff = ac.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                        agrupa_subcuenta = ac.GroupBy(asc => new { asc.subcuenta }).Select(asc => new agrupa_subcuenta
                        {
                            subcuenta = asc.Key.subcuenta ?? 0,
                            nom_subcuenta = asc.Select(g => g.nom_subcuenta).First(),
                            subcuenta_t_saldoini = asc.Sum(f => f.saldo_ini),
                            subcuenta_t_debito = asc.Sum(f => f.debito),
                            subcuenta_t_credito = asc.Sum(f => f.credito),
                            subcuenta_t_saldo = asc.Sum(f => f.saldo_ini + f.debito - f.credito),
                            subcuenta_t_saldoininiff = asc.Sum(f => f.saldo_ininiff),
                            subcuenta_t_debitoniff = asc.Sum(f => f.debitoniff),
                            subcuenta_t_creditoniff = asc.Sum(f => f.creditoniff),
                            subcuenta_t_saldoniff = asc.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                            detalleCuenta = asc.GroupBy(dc => new { dc.cuenta }).Select(dc => new detalleCuenta
                            {
                                id_cuenta_det = dc.Key.cuenta,
                                numero_cuenta_det = dc.Select(g => g.cntpuc_numero).First(),
                                clase_cuenta_det = dc.Select(g => g.clase ?? 0).First(),
                                grupo_cuenta_det = dc.Select(g => g.grupo ?? 0).First(),
                                xcuenta_cuenta_det = dc.Select(g => g.xcuenta ?? 0).First(),
                                subcuenta_cuenta_det = dc.Select(g => g.subcuenta ?? 0).First(),
                                nombre_cuenta_det = dc.Select(g => g.cntpuc_descp).First(),
                                saldoini_det = dc.Sum(fc => fc.saldo_ini),
                                debito_det = dc.Sum(fc => fc.debito),
                                credito_det = dc.Sum(fc => fc.credito),
                                saldo_det = dc.Sum(f => f.saldo_ini + f.debito - f.credito),
                                saldoininiff_det = dc.Sum(fc => fc.saldo_ininiff),
                                debitoniff_det = dc.Sum(fc => fc.debitoniff),
                                creditoniff_det = dc.Sum(fc => fc.creditoniff),
                                saldoniff_det = dc.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff)
                                //listacentro = dc.GroupBy(lc => new { lc.centro }).Select(lc => new listacentro
                                //{
                                //    id_centro = lc.Key.centro,
                                //    codigo_centro = lc.Select(f => f.pre_centcst).First(),
                                //    nombre_centro = lc.Select(f => f.centcst_nombre).First(),
                                //    scentro_t_saldoini = lc.Sum(f => f.saldo_ini),
                                //    scentro_t_debito = lc.Sum(f => f.debito),
                                //    scentro_t_credito = lc.Sum(f => f.credito),
                                //    scentro_t_saldo = lc.Sum(f => f.saldo_ini + f.debito - f.credito),
                                //    scentro_t_saldoininiff = lc.Sum(f => f.saldo_ininiff),
                                //    scentro_t_debitoniff = lc.Sum(f => f.debitoniff),
                                //    scentro_t_creditoniff = lc.Sum(f => f.creditoniff),
                                //    scentro_t_saldoniff = lc.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                                //    //agrupa_nit = lc.GroupBy(an => new { an.nit }).Select(an => new agrupa_nit
                                //    //{
                                //    //    nit = an.Key.nit,
                                //    //    documento = an.Select(g => g.doc_tercero).First(),
                                //    //    nombre_nit = an.Select(g => g.NombreX).First(),
                                //    //    nit_t_saldoini = an.Sum(fc => fc.saldo_ini),
                                //    //    nit_t_debito = an.Sum(fc => fc.debito),
                                //    //    nit_t_credito = an.Sum(fc => fc.credito),
                                //    //    nit_t_saldo = an.Sum(f => f.saldo_ini + f.debito - f.credito),
                                //    //    nit_t_saldoininiff = an.Sum(fc => fc.saldo_ininiff),
                                //    //    nit_t_debitoniff = an.Sum(fc => fc.debitoniff),
                                //    //    nit_t_creditoniff = an.Sum(fc => fc.creditoniff),
                                //    //    nit_t_saldoniff = an.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                                //    //}).OrderBy(c => c.nit).ToList(),
                                //}).OrderBy(c => c.id_centro).ToList(),
                            }).OrderBy(c => c.id_cuenta_det).ToList()
                        }).OrderBy(c => c.subcuenta).ToList()
                    }).OrderBy(c => c.cuenta).ToList()
                }).OrderBy(c => c.grupo).ToList()
            }).OrderBy(c => c.clase).ToList();

            //*****  MOVIMIENTOS AGRUPADOS -FIN

            #endregion

            #region Ejecuta el modelo (0005 - 0006 EN LA VISTA) Sin nada ckeck

            if (checkCentro == false && checkNit == false && checkMovimiento == false && Ver)
            {
                ViewAsPdf salida = new ViewAsPdf("repcontablepdf", "");
                ViewModelReportepdf reporte6 = new ViewModelReportepdf
                {
                    titulo = "Reporte Contable (Cuentas)",
                    fechaReporte = DateTime.Now,
                    fechaInicio = Convert.ToString(wanio),
                    fechafin = Convert.ToString(wmes),
                    cuentaIni = Convert.ToString(wcuentaini),
                    cuentaFin = Convert.ToString(wcuentafin),
                    Grantotalsaldoini = seleccionSegundo_04.Sum(f => f.Ctotalsaldoini),
                    Grantotaldebito = seleccionSegundo_04.Sum(f => f.Ctotaldebito),
                    Grantotalcredito = seleccionSegundo_04.Sum(f => f.Ctotalcredito),
                    Grantotalsaldo = seleccionSegundo_04.Sum(f => f.Ctotalsaldo),
                    Grantotalsaldoininiff = seleccionSegundo_04.Sum(f => f.Ctotalsaldoininiff),
                    Grantotaldebitoniff = seleccionSegundo_04.Sum(f => f.Ctotaldebitoniff),
                    Grantotalcreditoniff = seleccionSegundo_04.Sum(f => f.Ctotalcreditoniff),
                    Grantotalsaldoniff = seleccionSegundo_04.Sum(f => f.Ctotalsaldoniff),
                    //seleccion_cuenta = seleccionCentro,
                    listaclase = seleccionSegundo_04
                };

                salida = new ViewAsPdf("repcontablepdf", reporte6);
                something = salida;
                Ver = true;
            }

            #endregion

            //************************

            #region Ejecuta el modelo Solo con Nit (0007-0008 EN LA VISTA)

            ///*****  MOVIMIENTOS AGRUPADOS - INICIO
            List<listaclase> seleccionSegundo_05 = movt.GroupBy(lc1 => new { lc1.clase }).Select(lc1 => new listaclase
            {
                clase = lc1.Key.clase ?? 0,
                nom_clase = lc1.Select(f => f.nom_clase).First(),
                Ctotalsaldoini = lc1.Sum(f => f.saldo_ini),
                Ctotaldebito = lc1.Sum(f => f.debito),
                Ctotalcredito = lc1.Sum(f => f.credito),
                Ctotalsaldo = lc1.Sum(f => f.saldo_ini + f.debito - f.credito),
                Ctotalsaldoininiff = lc1.Sum(f => f.saldo_ininiff),
                Ctotaldebitoniff = lc1.Sum(f => f.debitoniff),
                Ctotalcreditoniff = lc1.Sum(f => f.creditoniff),
                Ctotalsaldoniff = lc1.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                agrupa_grupo = lc1.GroupBy(ag => new { ag.grupo }).Select(ag => new agrupa_grupo
                {
                    grupo = ag.Key.grupo ?? 0,
                    nom_grupo = ag.Select(g => g.nom_grupo).First(),
                    grupo_t_saldoini = ag.Sum(f => f.saldo_ini),
                    grupo_t_debito = ag.Sum(f => f.debito),
                    grupo_t_credito = ag.Sum(f => f.credito),
                    grupo_t_saldo = ag.Sum(f => f.saldo_ini + f.debito - f.credito),
                    grupo_t_saldoininiff = ag.Sum(f => f.saldo_ininiff),
                    grupo_t_debitoniff = ag.Sum(f => f.debitoniff),
                    grupo_t_creditoniff = ag.Sum(f => f.creditoniff),
                    grupo_t_saldoniff = ag.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                    agrupa_cuenta = ag.GroupBy(ac => new { ac.xcuenta }).Select(ac => new agrupa_cuenta
                    {
                        cuenta = ac.Key.xcuenta ?? 0,
                        nom_cuenta = ac.Select(g => g.nom_cuenta).First(),
                        cuenta_t_saldoini = ac.Sum(f => f.saldo_ini),
                        cuenta_t_debito = ac.Sum(f => f.debito),
                        cuenta_t_credito = ac.Sum(f => f.credito),
                        cuenta_t_saldo = ac.Sum(f => f.saldo_ini + f.debito - f.credito),
                        cuenta_t_saldoininiff = ac.Sum(f => f.saldo_ininiff),
                        cuenta_t_debitoniff = ac.Sum(f => f.debitoniff),
                        cuenta_t_creditoniff = ac.Sum(f => f.creditoniff),
                        cuenta_t_saldoniff = ac.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                        agrupa_subcuenta = ac.GroupBy(asc => new { asc.subcuenta }).Select(asc => new agrupa_subcuenta
                        {
                            subcuenta = asc.Key.subcuenta ?? 0,
                            nom_subcuenta = asc.Select(g => g.nom_subcuenta).First(),
                            subcuenta_t_saldoini = asc.Sum(f => f.saldo_ini),
                            subcuenta_t_debito = asc.Sum(f => f.debito),
                            subcuenta_t_credito = asc.Sum(f => f.credito),
                            subcuenta_t_saldo = asc.Sum(f => f.saldo_ini + f.debito - f.credito),
                            subcuenta_t_saldoininiff = asc.Sum(f => f.saldo_ininiff),
                            subcuenta_t_debitoniff = asc.Sum(f => f.debitoniff),
                            subcuenta_t_creditoniff = asc.Sum(f => f.creditoniff),
                            subcuenta_t_saldoniff = asc.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                            detalleCuenta = asc.GroupBy(dc => new { dc.cuenta }).Select(dc => new detalleCuenta
                            {
                                id_cuenta_det = dc.Key.cuenta,
                                numero_cuenta_det = dc.Select(g => g.cntpuc_numero).First(),
                                clase_cuenta_det = dc.Select(g => g.clase ?? 0).First(),
                                grupo_cuenta_det = dc.Select(g => g.grupo ?? 0).First(),
                                xcuenta_cuenta_det = dc.Select(g => g.xcuenta ?? 0).First(),
                                subcuenta_cuenta_det = dc.Select(g => g.subcuenta ?? 0).First(),
                                nombre_cuenta_det = dc.Select(g => g.cntpuc_descp).First(),
                                saldoini_det = dc.Sum(fc => fc.saldo_ini),
                                debito_det = dc.Sum(fc => fc.debito),
                                credito_det = dc.Sum(fc => fc.credito),
                                saldo_det = dc.Sum(f => f.saldo_ini + f.debito - f.credito),
                                saldoininiff_det = dc.Sum(fc => fc.saldo_ininiff),
                                debitoniff_det = dc.Sum(fc => fc.debitoniff),
                                creditoniff_det = dc.Sum(fc => fc.creditoniff),
                                saldoniff_det = dc.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff),
                                agrupa_nit = dc.GroupBy(an => new { an.nit }).Select(an => new agrupa_nit
                                {
                                    nit = an.Key.nit,
                                    documento = an.Select(g => g.doc_tercero).First(),
                                    nombre_nit = an.Select(g => g.NombreX).First(),
                                    nit_t_saldoini = an.Sum(fc => fc.saldo_ini),
                                    nit_t_debito = an.Sum(fc => fc.debito),
                                    nit_t_credito = an.Sum(fc => fc.credito),
                                    nit_t_saldo = an.Sum(f => f.saldo_ini + f.debito - f.credito),
                                    nit_t_saldoininiff = an.Sum(fc => fc.saldo_ininiff),
                                    nit_t_debitoniff = an.Sum(fc => fc.debitoniff),
                                    nit_t_creditoniff = an.Sum(fc => fc.creditoniff),
                                    nit_t_saldoniff = an.Sum(f => f.saldo_ininiff + f.debitoniff - f.creditoniff)
                                }).OrderBy(c => c.nit).ToList()
                            }).OrderBy(c => c.id_cuenta_det).ToList()
                        }).OrderBy(c => c.subcuenta).ToList()
                    }).OrderBy(c => c.cuenta).ToList()
                }).OrderBy(c => c.grupo).ToList()
            }).OrderBy(c => c.clase).ToList();

            //*****  MOVIMIENTOS AGRUPADOS -FIN

            #endregion

            #region Ejecuta el modelo Solo con Nit (0007-0008 EN LA VISTA)

            if (checkCentro == false && checkNit && checkMovimiento == false && Ver)
            {
                ViewAsPdf salida = new ViewAsPdf("repcontablepdf", "");
                ViewModelReportepdf reporte7 = new ViewModelReportepdf
                {
                    titulo = "Reporte Contable por Centro pdf",
                    fechaReporte = DateTime.Now,
                    fechaInicio = Convert.ToString(wanio),
                    fechafin = Convert.ToString(wmes),
                    cuentaIni = Convert.ToString(wcuentaini),
                    cuentaFin = Convert.ToString(wcuentafin),
                    Grantotalsaldoini = seleccionSegundo_04.Sum(f => f.Ctotalsaldoini),
                    Grantotaldebito = seleccionSegundo_04.Sum(f => f.Ctotaldebito),
                    Grantotalcredito = seleccionSegundo_04.Sum(f => f.Ctotalcredito),
                    Grantotalsaldo = seleccionSegundo_04.Sum(f => f.Ctotalsaldo),
                    Grantotalsaldoininiff = seleccionSegundo_04.Sum(f => f.Ctotalsaldoininiff),
                    Grantotaldebitoniff = seleccionSegundo_04.Sum(f => f.Ctotaldebitoniff),
                    Grantotalcreditoniff = seleccionSegundo_04.Sum(f => f.Ctotalcreditoniff),
                    Grantotalsaldoniff = seleccionSegundo_04.Sum(f => f.Ctotalsaldoniff),
                    //seleccion_cuenta = seleccionCentro,
                    listaclase = seleccionSegundo_05
                };

                salida = new ViewAsPdf("repcontablepdf", reporte7);
                something = salida;
                Ver = true;
            }

            #endregion

            //*************************


            ViewBag.Ver = Ver;
            return something;
        } //final del metodo
    }
}