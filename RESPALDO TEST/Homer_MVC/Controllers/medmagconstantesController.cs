using Homer_MVC.IcebergModel;
using Homer_MVC.ViewModels.medios;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Homer_MVC.Controllers
{
    public class medmagconstantesController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: medmagcontantes
        public ActionResult Crearmmconstante()
        {
            ViewModeloMediosConstante VModelomedios = new ViewModeloMediosConstante
            {
                Viewterceros = new icb_terceros(),
                Viewmediosconstantes = new medios_constantes(),
                ViewListterceros = new List<icb_terceros>(),

                Viewcuenta_puc = new cuenta_puc(),
                Viewmedios_gen = new medios_gen(),
                Viewmedios_movtos = new medios_movtos(),
                Viewtp_doc_registros = new tp_doc_registros(),
                Viewmediostipovalor = new mediostipovalor()
            };


            //ViewBag.tpdoc_id = new SelectList(db.icb_terceros, "tercero_id", "doc_tercero");
            // ViewBag.ciu_id = new SelectList(db.icb_terceros,"tercero_id", "doc_tercero");
            //var listaTerceros2 = db.icb_terceros.ToList();
            var listaTerceros = (from ter in db.icb_terceros
                                 where ter.tercero_estado
                                 select new
                                 {
                                     ter.tercero_id,
                                     doc_tercero = ter.doc_tercero + " " + (ter.razon_social != null
                                                       ? ter.razon_social
                                                       : ter.prinom_tercero + " " + ter.segnom_tercero + " " + ter.apellido_tercero +
                                                         " " + ter.segapellido_tercero)
                                 }).ToList();
            // ViewBag.tercero_id = new SelectList(terceros, "id", "docyNombre");
            ViewBag.tercero_id = new SelectList(listaTerceros, "tercero_id", "doc_tercero");

            //*********************************************

            var listaCuentas = (from cta in db.cuenta_puc
                                select new
                                {
                                    cta.cntpuc_id,
                                    des_cuantax = cta.cntpuc_numero + " - " + cta.cntpuc_descp
                                }).ToList();
            ViewBag.cntpuc_id = new SelectList(listaCuentas, "cntpuc_id", "des_cuantax");
            /// des_cuantax = cta.cntpuc_numero + " - " + cta.cntpuc_descp + " Mov_Cta: " + cta.mov_cnt + " Clase " + cta.clase + " Grupo " + cta.grupo + " Cuenta " + cta.cuenta + " Sub-Cuenta " + cta.subcuenta,
            //*********************************************

            var listaTpDocRegi = (from tdoc in db.tp_doc_registros
                                  select new
                                  {
                                      tdoc.tpdoc_id,
                                      des_tpDocRegi = tdoc.prefijo + " - " + tdoc.tpdoc_nombre
                                  }).ToList();
            ViewBag.tpdoc_id = new SelectList(listaTpDocRegi, "tpdoc_id", "des_tpDocRegi");
            //*********************************************

            var listaTpDocRegi2 = (from tdoc2 in db.tp_doc_registros
                                   select new
                                   {
                                       tdoc2.tpdoc_id,
                                       des_tpDocRegi2 = tdoc2.prefijo + " - " + tdoc2.tpdoc_nombre
                                   }).ToList();
            ViewBag.tpdoc_id2 = new SelectList(listaTpDocRegi2, "tpdoc_id", "des_tpDocRegi2");
            //*********************************************
            var listamediostipovalor = (from medtvalor in db.mediostipovalor
                                        select new
                                        {
                                            medtvalor.id,
                                            medtvalor.descripcion
                                        }).ToList();
            ViewBag.id = new SelectList(listamediostipovalor, "id", "descripcion");

            //*********************************************
            var listaconceptos = (from medconceptos in db.medios_conceptos
                                  select new
                                  {
                                      medconceptos.id,
                                      medconceptos.concepto
                                  }).ToList();
            ViewBag.Conceptoid = new SelectList(listaconceptos, "id", "concepto");

            //*********************************************

            var listaformatos = (from medformato in db.medios_formato
                                 select new
                                 {
                                     medformato.id,
                                     medformato.formato
                                 }).ToList();
            ViewBag.Formatoid = new SelectList(listaformatos, "id", "formato");


            return View(VModelomedios);
        }

        public ActionResult MttoFormato()
        {
            return View();
            // return View(VModelomedios);
        }

        public JsonResult AgregarConstantes(int idanio, int idformato, int idconcepto, int nit, decimal? Val1,
            decimal? Val2, decimal? Val3, decimal? Val4, decimal? Val5, decimal? Val6, decimal? Val7, decimal? Val8,
            decimal? Val9, decimal? Val10, decimal? Val11, string observacion)
        {
            bool result = false;
            if (idanio > 0)
            {
                medios_constantes buscarConstante = db.medios_constantes.FirstOrDefault(c =>
                    c.ano == idanio && c.formato == idformato && c.concepto == idconcepto && c.nit == nit);
                if (buscarConstante != null)
                {
                    buscarConstante.val1 = Val1 ?? 0;
                    buscarConstante.val2 = Val2 ?? 0;
                    buscarConstante.val3 = Val3 ?? 0;
                    buscarConstante.val4 = Val4 ?? 0;
                    buscarConstante.val5 = Val5 ?? 0;
                    buscarConstante.val6 = Val6 ?? 0;
                    buscarConstante.val7 = Val7 ?? 0;
                    buscarConstante.val8 = Val8 ?? 0;
                    buscarConstante.val9 = Val9 ?? 0;
                    buscarConstante.val10 = Val10 ?? 0;
                    buscarConstante.val11 = Val11 ?? 0;
                    buscarConstante.Detalle = observacion;
                    db.Entry(buscarConstante).State = EntityState.Modified;
                    int actualizar = db.SaveChanges();
                    if (actualizar > 0)
                    {
                        result = true;
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    db.medios_constantes.Add(new medios_constantes
                    {
                        ano = idanio,
                        formato = idformato,
                        concepto = idconcepto,
                        nit = nit,
                        val1 = Val1 ?? 0,
                        val2 = Val2 ?? 0,
                        val3 = Val3 ?? 0,
                        val4 = Val4 ?? 0,
                        val5 = Val5 ?? 0,
                        val6 = Val6 ?? 0,
                        val7 = Val7 ?? 0,
                        val8 = Val8 ?? 0,
                        val9 = Val9 ?? 0,
                        val10 = Val10 ?? 0,
                        val11 = Val11 ?? 0,
                        Detalle = observacion
                    });
                    int guardar = db.SaveChanges();

                    if (guardar > 0)
                    {
                        result = true;
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            result = false;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //*****
        public JsonResult AgregarDefBD(int idanio, int idformato, int idconcepto, string Val1, string Val2, string Val3,
            string Val4, string Val5, string Val6, string Val7, string Val8, string Val9, string Val10, string Val11)
        {
            bool result = false;
            if (idanio > 0)
            {
                medios_gen buscarDefBD = db.medios_gen.FirstOrDefault(c =>
                    c.ano == idanio && c.formato == idformato && c.concepto == idconcepto);
                if (buscarDefBD != null)
                {
                    buscarDefBD.val1 = Val1;
                    buscarDefBD.val2 = Val2;
                    buscarDefBD.val3 = Val3;
                    buscarDefBD.val4 = Val4;
                    buscarDefBD.val5 = Val5;
                    buscarDefBD.val6 = Val6;
                    buscarDefBD.val7 = Val7;
                    buscarDefBD.val8 = Val8;
                    buscarDefBD.val9 = Val9;
                    buscarDefBD.val10 = Val10;
                    buscarDefBD.val11 = Val11;

                    db.Entry(buscarDefBD).State = EntityState.Modified;
                    int actualizar = db.SaveChanges();
                    if (actualizar > 0)
                    {
                        result = true;
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    db.medios_gen.Add(new medios_gen
                    {
                        ano = idanio,
                        formato = idformato,
                        concepto = idconcepto,
                        val1 = Val1,
                        val2 = Val2,
                        val3 = Val3,
                        val4 = Val4,
                        val5 = Val5,
                        val6 = Val6,
                        val7 = Val7,
                        val8 = Val8,
                        val9 = Val9,
                        val10 = Val10,
                        val11 = Val11
                    });
                    int guardar = db.SaveChanges();

                    if (guardar > 0)
                    {
                        result = true;
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            result = false;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AgregarDefMovCtaBD(int idmedios, int tipomovimiento, int cuenta, string operacion,
            int idtipodoc, int idvalor, int tipodato)
        {
            bool result = false;


            if (idmedios > 0)
            {
                medios_movtos buscarDefMovCtaBD = db.medios_movtos.FirstOrDefault(m =>
                    m.idmedios == idmedios && m.tipomovimiento == tipomovimiento && m.cuenta == cuenta &&
                    m.operacion == operacion && m.tipodato == tipodato && m.idvalor == idvalor);
                if (buscarDefMovCtaBD != null)
                {
                    result = false;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                if (idtipodoc == 0)
                {
                    db.medios_movtos.Add(new medios_movtos
                    {
                        idmedios = idmedios,
                        tipomovimiento = tipomovimiento,
                        cuenta = cuenta,
                        operacion = operacion,
                        idvalor = idvalor,
                        tipodato = tipodato
                    });
                }
                else
                {
                    db.medios_movtos.Add(new medios_movtos
                    {
                        idmedios = idmedios,
                        tipomovimiento = tipomovimiento,
                        cuenta = cuenta,
                        operacion = operacion,
                        idtipodoc = idtipodoc,
                        idvalor = idvalor,
                        tipodato = tipodato
                    });
                }
                //db.medios_movtos.Add(new medios_movtos()
                //{
                //   idmedios = idmedios,
                //   tipomovimiento = tipomovimiento,
                //    cuenta = cuenta,
                //    operacion = operacion,
                //    idtipodoc = idtipodoc,
                //    idvalor = idvalor,
                //    tipodato = tipodato,
                //});
                int guardar = db.SaveChanges();

                if (guardar > 0)
                {
                    result = true;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }

            result = false;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AgregarDocumentosBD(int idmedios, int tipomovimiento, string operacion, int idtipodoc,
            int idvalor, int tipodato)
        {
            bool result = false;


            if (idmedios > 0)
            {
                medios_movtos buscarDocumentosBD = db.medios_movtos.FirstOrDefault(m =>
                    m.idmedios == idmedios && m.tipomovimiento == tipomovimiento && m.operacion == operacion &&
                    m.tipodato == tipodato && m.idvalor == idvalor);
                if (buscarDocumentosBD != null)
                {
                    result = false;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                db.medios_movtos.Add(new medios_movtos
                {
                    idmedios = idmedios,
                    tipomovimiento = tipomovimiento,
                    //cuenta = cuenta,
                    operacion = operacion,
                    idtipodoc = idtipodoc,
                    idvalor = idvalor,
                    tipodato = tipodato
                });

                int guardar = db.SaveChanges();

                if (guardar > 0)
                {
                    result = true;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }

            result = false;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult actualizarValorinVal(int idCabecera, int idPosValor, string NuevoValor)
        {
            bool result = false;
            if (idCabecera > 0)
            {
                medios_gen buscarValor = db.medios_gen.FirstOrDefault(c => c.id == idCabecera);
                if (buscarValor != null)
                {
                    if (idPosValor == 1)
                    {
                        buscarValor.val1 = NuevoValor;
                    }

                    if (idPosValor == 2)
                    {
                        buscarValor.val2 = NuevoValor;
                    }

                    if (idPosValor == 3)
                    {
                        buscarValor.val3 = NuevoValor;
                    }

                    if (idPosValor == 4)
                    {
                        buscarValor.val4 = NuevoValor;
                    }

                    if (idPosValor == 5)
                    {
                        buscarValor.val5 = NuevoValor;
                    }

                    if (idPosValor == 6)
                    {
                        buscarValor.val6 = NuevoValor;
                    }

                    if (idPosValor == 7)
                    {
                        buscarValor.val7 = NuevoValor;
                    }

                    if (idPosValor == 8)
                    {
                        buscarValor.val8 = NuevoValor;
                    }

                    if (idPosValor == 9)
                    {
                        buscarValor.val9 = NuevoValor;
                    }

                    if (idPosValor == 10)
                    {
                        buscarValor.val10 = NuevoValor;
                    }

                    if (idPosValor == 11)
                    {
                        buscarValor.val11 = NuevoValor;
                    }

                    db.Entry(buscarValor).State = EntityState.Modified;
                    int actualizar = db.SaveChanges();
                    if (actualizar > 0)
                    {
                        result = true;
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    result = true;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }

            result = false;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //*****
        public JsonResult CopiarNuevoConf(int annio_copia)
        {
            bool result = false;
            int anio_nuevo = annio_copia + 1;
            if (annio_copia > 0)
            {
                medios_gen buscarGeneral = db.medios_gen.FirstOrDefault(m => m.ano == anio_nuevo);
                if (buscarGeneral == null)
                {
                    result = false;
                }
                else
                {
                    var dataActualGeneral = (from c in db.medios_gen
                                             where c.ano == annio_copia
                                             select new
                                             {
                                                 c.id,
                                                 c.ano,
                                                 c.formato,
                                                 c.concepto,
                                                 c.val1,
                                                 c.val2,
                                                 c.val3,
                                                 c.val4,
                                                 c.val5,
                                                 c.val6,
                                                 c.val7,
                                                 c.val8,
                                                 c.val9,
                                                 c.val10,
                                                 c.val11
                                             }).ToList();
                    if (dataActualGeneral != null)
                    {
                        for (int i = 0; i < dataActualGeneral.Count; i++)
                        {
                            db.medios_gen.Add(new medios_gen
                            {
                                ano = dataActualGeneral[i].ano + 1,
                                formato = dataActualGeneral[i].formato,
                                concepto = dataActualGeneral[i].concepto,
                                val1 = dataActualGeneral[i].val1,
                                val2 = dataActualGeneral[i].val2,
                                val3 = dataActualGeneral[i].val3,
                                val4 = dataActualGeneral[i].val4,
                                val5 = dataActualGeneral[i].val5,
                                val6 = dataActualGeneral[i].val6,
                                val7 = dataActualGeneral[i].val7,
                                val8 = dataActualGeneral[i].val8,
                                val9 = dataActualGeneral[i].val9,
                                val10 = dataActualGeneral[i].val10,
                                val11 = dataActualGeneral[i].val11
                            });
                            int idtemp = dataActualGeneral[i].id;
                            db.SaveChanges();
                            int newid = db.medios_gen.Max(x => x.id);
                            var dataActualMov = (from m in db.medios_movtos
                                                 where m.idmedios == idtemp
                                                 select new
                                                 {
                                                     m.idmedios,
                                                     m.tipomovimiento,
                                                     m.cuenta,
                                                     m.operacion,
                                                     m.idtipodoc,
                                                     m.idvalor,
                                                     m.tipodato
                                                 }).ToList();
                            if (dataActualMov != null)
                            {
                                for (int j = 0; j < dataActualMov.Count; j++)
                                {
                                    db.medios_movtos.Add(new medios_movtos
                                    {
                                        idmedios = newid,
                                        tipomovimiento = dataActualMov[j].tipomovimiento,
                                        cuenta = dataActualMov[j].cuenta,
                                        operacion = dataActualMov[j].operacion,
                                        idtipodoc = dataActualMov[j].idtipodoc,
                                        idvalor = dataActualMov[j].idvalor,
                                        tipodato = dataActualMov[j].tipodato
                                    });
                                    db.SaveChanges();
                                    result = true;
                                }
                            }
                        }
                    }
                }
            }

            //result = false;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult MediosExcelDefinitivo(int? excelanio, int? excelformato, string excelconcepto)
        {
            int? localWanio = excelanio;
            int? localWform = excelformato;
            string localWconc = excelconcepto;
            int conid = 0;

            string[] Vectorexcelconcepto = excelconcepto.Split(',');
            // string[] Vectorexcelformato = excelformato.Split(',');

            #region Declaracion de PredicateBuilder Excel

            //Declaramos el predicado de tipo And con el parámetro True. Si fuera de tipo Or lo declaramos con el parámetro False
            System.Linq.Expressions.Expression<Func<vw_medios_magneticos_excel, bool>> predicate = PredicateBuilder.True<vw_medios_magneticos_excel>();
            System.Linq.Expressions.Expression<Func<vw_medios_magneticos_excel, bool>> predicate2 = PredicateBuilder.False<vw_medios_magneticos_excel>();

            #endregion

            #region Criterios aplicados a los PredicateBuilder Excel

            if (excelanio != null)
            {
                predicate = predicate.And(x => x.ano == excelanio);
            }

            if (Vectorexcelconcepto.Count() > 0)
            {
                foreach (string j in Vectorexcelconcepto)
                {
                    int con = Convert.ToInt32(j);
                    conid = con;
                    predicate2 = predicate2.Or(x => x.idConcepto == con);
                }

                predicate = predicate.And(predicate2);
            }

            #endregion


            int newidcon = Convert.ToInt32(conid);
            var idfor = db.vw_medios_magneticos_excel.Where(an => an.ano == localWanio && an.idConcepto == newidcon)
                .Select(re => new { idformato = re.idFormato }).ToList();
            //var idfor = db.vw_medios_magneticos_excel.Where(an => an.ano == localWanio && an.idConcepto == newidcon).Select(re => new { idformato = localWform }).ToList();
            if (idfor.Count > 0)
            {
                int formato = Convert.ToInt32(idfor[0].idformato);
                var formatook = db.vw_medios_magneticos_formatos.Where(an => an.formato == formato)
                    .Select(an => new { forma = an.Des }).ToList();
                if (formatook.Count > 0)
                {
                    string deci = Convert.ToString(formatook[0].forma);
                    if (deci == "1001")
                    {
                        #region Archivo Excel 1001

                        string nomArchivo = "Dian " + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day +
                                         DateTime.Now.Minute + "_1001";
                        List<vw_medios_magneticos_excel> envioExcel = db.vw_medios_magneticos_excel.Where(predicate).ToList();
                        var data = envioExcel.Select(d => new
                        {
                            d.NomCon,
                            d.TipoDocumento,
                            d.doc_tercero,
                            d.digito_verificacion,
                            d.apellido_tercero,
                            d.segapellido_tercero,
                            d.prinom_tercero,
                            d.segnom_tercero,
                            d.razon_social,
                            d.direccion,
                            d.cod_dpto,
                            d.cod_ciudad,
                            d.cod_pais,
                            d.val1,
                            d.val2,
                            d.val3,
                            d.val4,
                            d.val5,
                            d.val6,
                            d.val7,
                            d.val8,
                            d.val9,
                            d.val10,
                            d.val11
                        }).ToArray();

                        ExcelPackage excel = new ExcelPackage();
                        ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Formato1001");

                        workSheet.Cells[1, 1].Value = "Reporte - Dian Medios Magnetico";
                        workSheet.Cells[2, 1].Value = nomArchivo;

                        workSheet.Column(1).Width = 10;
                        workSheet.Column(2).Width = 12;
                        workSheet.Column(3).Width = 14;
                        workSheet.Column(4).Width = 4;
                        workSheet.Column(5).Width = 10;
                        workSheet.Column(6).Width = 10;
                        workSheet.Column(7).Width = 10;
                        workSheet.Column(8).Width = 10;
                        workSheet.Column(9).Width = 16;
                        workSheet.Column(10).Width = 20;
                        workSheet.Column(11).Width = 7;
                        workSheet.Column(12).Width = 10;
                        workSheet.Column(13).Width = 12;
                        workSheet.Column(14).Width = 12;
                        workSheet.Column(15).Width = 12;
                        workSheet.Column(16).Width = 12;
                        workSheet.Column(17).Width = 12;
                        workSheet.Column(18).Width = 10;
                        workSheet.Column(19).Width = 10;
                        workSheet.Column(20).Width = 13;
                        workSheet.Column(21).Width = 12;
                        workSheet.Column(22).Width = 14;
                        workSheet.Column(23).Width = 11;
                        workSheet.Column(24).Width = 11;

                        workSheet.Row(3).Height = 80;

                        workSheet.Cells["A3:X3"].Style.Font.Bold = true;
                        workSheet.Cells["A3:X3"].Style.Font.Size = 10;
                        workSheet.Cells["A3:X3"].Style.Font.Color.SetColor(Color.Black);
                        workSheet.Cells["A3:X3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        workSheet.Cells["A3:X3"].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                        workSheet.Cells["A3:X3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells["A3:X3"].Style.WrapText = true;

                        workSheet.Cells[3, 1].Value = "Concepto";
                        workSheet.Cells[3, 2].Value = "Tipo de Documentos";
                        workSheet.Cells[3, 3].Value = "Numero de identificación del informado";
                        workSheet.Cells[3, 4].Value = "DV";
                        workSheet.Cells[3, 5].Value = "Primer apellido";
                        workSheet.Cells[3, 6].Value = "Segundo Apellido";
                        workSheet.Cells[3, 7].Value = "Primer Nombre";
                        workSheet.Cells[3, 8].Value = "Otros Nombres";
                        workSheet.Cells[3, 9].Value = "Razón Social";
                        workSheet.Cells[3, 10].Value = "Dirección";
                        workSheet.Cells[3, 11].Value = "Cod. Dpto.";
                        workSheet.Cells[3, 12].Value = "Cod. Mcipio.";
                        workSheet.Cells[3, 13].Value = "País de residencia o domicilio";
                        workSheet.Cells[3, 14].Value = "Pago o abono en cuenta deducible";
                        workSheet.Cells[3, 15].Value = "Pago o abono en cuenta NO deducible";
                        workSheet.Cells[3, 16].Value = "IVA mayor valor del costo o gasto deducible";
                        workSheet.Cells[3, 17].Value = "IVA mayor valor del costo o gasto NO deducible";
                        workSheet.Cells[3, 18].Value = "Retención en la fuente practicada Renta";
                        workSheet.Cells[3, 19].Value = "Retención en la fuente asumida Renta";
                        workSheet.Cells[3, 20].Value = "Retención en la fuente practicada IVA régimen común";
                        workSheet.Cells[3, 21].Value = "Retención en la fuente asumida IVA régimen Simplificado";
                        workSheet.Cells[3, 22].Value = "Retención en la fuente practicada IVA no domiciliados";
                        workSheet.Cells[3, 23].Value = "Retención en la fuente practicada por CREE";
                        workSheet.Cells[3, 24].Value = "Retención en la fuente asumida por CREE";

                        workSheet.Cells[4, 1].LoadFromCollection(data, false);
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            Response.AddHeader("content-disposition", "attachment;  filename=" + nomArchivo + ".xlsx");
                            excel.SaveAs(memoryStream);
                            memoryStream.WriteTo(Response.OutputStream);
                            Response.Flush();
                            Response.End();
                        }

                        #endregion
                    }

                    if (deci == "1003")
                    {
                        #region Archivo Excel 1003

                        string nomArchivo = "Dian " + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day +
                                         DateTime.Now.Minute + "_1003";
                        List<vw_medios_magneticos_excel> envioExcel = db.vw_medios_magneticos_excel.Where(predicate).ToList();
                        var data = envioExcel.Select(d => new
                        {
                            d.NomCon,
                            d.TipoDocumento,
                            d.doc_tercero,
                            d.digito_verificacion,
                            d.apellido_tercero,
                            d.segapellido_tercero,
                            d.prinom_tercero,
                            d.segnom_tercero,
                            d.razon_social,
                            d.direccion,
                            d.cod_dpto,
                            d.cod_ciudad,
                            // d.cod_pais,
                            d.val1,
                            d.val2
                        }).ToArray();

                        ExcelPackage excel = new ExcelPackage();
                        ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Formato1003");

                        workSheet.Cells[1, 1].Value = "Reporte - Dian Medios Magnetico";
                        workSheet.Cells[2, 1].Value = nomArchivo;

                        workSheet.Column(1).Width = 10;
                        workSheet.Column(2).Width = 12;
                        workSheet.Column(3).Width = 14;
                        workSheet.Column(4).Width = 4;
                        workSheet.Column(5).Width = 10;
                        workSheet.Column(6).Width = 10;
                        workSheet.Column(7).Width = 10;
                        workSheet.Column(8).Width = 10;
                        workSheet.Column(9).Width = 16;
                        workSheet.Column(10).Width = 20;
                        workSheet.Column(11).Width = 7;
                        workSheet.Column(12).Width = 10;
                        // workSheet.Column(13).Width = 12;
                        workSheet.Column(13).Width = 12;
                        workSheet.Column(14).Width = 12;


                        workSheet.Row(3).Height = 80;

                        workSheet.Cells["A3:N3"].Style.Font.Bold = true;
                        workSheet.Cells["A3:N3"].Style.Font.Size = 10;
                        workSheet.Cells["A3:N3"].Style.Font.Color.SetColor(Color.Black);
                        workSheet.Cells["A3:N3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        workSheet.Cells["A3:N3"].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                        workSheet.Cells["A3:N3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells["A3:N3"].Style.WrapText = true;

                        workSheet.Cells[3, 1].Value = "Concepto";
                        workSheet.Cells[3, 2].Value = "Tipo de Documentos";
                        workSheet.Cells[3, 3].Value = "Número de identificación del informado";
                        workSheet.Cells[3, 4].Value = "DV";
                        workSheet.Cells[3, 5].Value = "Primer apellido";
                        workSheet.Cells[3, 6].Value = "Segundo Apellido";
                        workSheet.Cells[3, 7].Value = "Primer Nombre";
                        workSheet.Cells[3, 8].Value = "Otros Nombres";
                        workSheet.Cells[3, 9].Value = "Razón Social";
                        workSheet.Cells[3, 10].Value = "Dirección";
                        workSheet.Cells[3, 11].Value = "Cod. Dpto.";
                        workSheet.Cells[3, 12].Value = "Cod. Mcipio.";
                        //        workSheet.Cells[3, 13].Value = "País de residencia o domicilio";
                        workSheet.Cells[3, 13].Value = "Valor acumulado del pago o abono sujeto a retención";
                        workSheet.Cells[3, 14].Value =
                            "Valor de la retención a título de renta, CREE,  o de IVA que le practicaron";

                        workSheet.Cells[4, 1].LoadFromCollection(data, false);
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            Response.AddHeader("content-disposition", "attachment;  filename=" + nomArchivo + ".xlsx");
                            excel.SaveAs(memoryStream);
                            memoryStream.WriteTo(Response.OutputStream);
                            Response.Flush();
                            Response.End();
                        }

                        #endregion
                    }
                }
            }
            else
            {
                return RedirectToAction("Crearmmconstante", new { });
            }

            return RedirectToAction("Crearmmconstante", new { });

            //return View();
        }

        public ActionResult MediosExcelDefinitivoTope(int? excelanio, int? excelformato, string excelconcepto)
        {
            int? localWanio = excelanio;
            int? localWform = excelformato;
            string localWconc = excelconcepto;
            int conid = 0;

            string[] Vectorexcelconcepto = excelconcepto.Split(',');
            // string[] Vectorexcelformato = excelformato.Split(',');

            #region Declaracion de PredicateBuilder Excel

            //Declaramos el predicado de tipo And con el parámetro True. Si fuera de tipo Or lo declaramos con el parámetro False
            System.Linq.Expressions.Expression<Func<vw_medios_magneticos_excel_tope, bool>> predicate = PredicateBuilder.True<vw_medios_magneticos_excel_tope>();
            System.Linq.Expressions.Expression<Func<vw_medios_magneticos_excel_tope, bool>> predicate2 = PredicateBuilder.False<vw_medios_magneticos_excel_tope>();

            #endregion

            #region Criterios aplicados a los PredicateBuilder Excel

            if (excelanio != null)
            {
                predicate = predicate.And(x => x.ano == excelanio);
            }

            if (Vectorexcelconcepto.Count() > 0)
            {
                foreach (string j in Vectorexcelconcepto)
                {
                    int con = Convert.ToInt32(j);
                    conid = con;
                    predicate2 = predicate2.Or(x => x.idConcepto == con);
                }

                predicate = predicate.And(predicate2);
            }

            #endregion


            int newidcon = Convert.ToInt32(conid);
            var idfor = db.vw_medios_magneticos_excel_tope
                .Where(an => an.ano == localWanio && an.idConcepto == newidcon)
                .Select(re => new { idformato = re.idFormato }).ToList();
            if (idfor.Count > 0)
            {
                int formato = Convert.ToInt32(idfor[0].idformato);
                var formatook = db.vw_medios_magneticos_formatos.Where(an => an.formato == formato)
                    .Select(an => new { forma = an.Des }).ToList();
                if (formatook.Count > 0)
                {
                    string deci = Convert.ToString(formatook[0].forma);
                    if (deci == "1001")
                    {
                        #region Archivo Excel 1001

                        string nomArchivo = "Dian " + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day +
                                         DateTime.Now.Minute + "_1001";
                        List<vw_medios_magneticos_excel_tope> envioExcel = db.vw_medios_magneticos_excel_tope.Where(predicate).ToList();
                        var data = envioExcel.Select(d => new
                        {
                            d.NomCon,
                            d.TipoDocumento,
                            d.doc_tercero,
                            d.digito_verificacion,
                            d.apellido_tercero,
                            d.segapellido_tercero,
                            d.prinom_tercero,
                            d.segnom_tercero,
                            d.razon_social,
                            d.direccion,
                            d.cod_dpto,
                            d.cod_ciudad,
                            d.cod_pais,
                            d.val1,
                            d.val2,
                            d.val3,
                            d.val4,
                            d.val5,
                            d.val6,
                            d.val7,
                            d.val8,
                            d.val9,
                            d.val10,
                            d.val11
                        }).ToArray();

                        ExcelPackage excel = new ExcelPackage();
                        ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Formato1001");

                        workSheet.Cells[1, 1].Value = "Reporte - Dian Medios Magnetico";
                        workSheet.Cells[2, 1].Value = nomArchivo;

                        workSheet.Column(1).Width = 10;
                        workSheet.Column(2).Width = 12;
                        workSheet.Column(3).Width = 14;
                        workSheet.Column(4).Width = 4;
                        workSheet.Column(5).Width = 10;
                        workSheet.Column(6).Width = 10;
                        workSheet.Column(7).Width = 10;
                        workSheet.Column(8).Width = 10;
                        workSheet.Column(9).Width = 16;
                        workSheet.Column(10).Width = 20;
                        workSheet.Column(11).Width = 7;
                        workSheet.Column(12).Width = 10;
                        workSheet.Column(13).Width = 12;
                        workSheet.Column(14).Width = 12;
                        workSheet.Column(15).Width = 12;
                        workSheet.Column(16).Width = 12;
                        workSheet.Column(17).Width = 12;
                        workSheet.Column(18).Width = 10;
                        workSheet.Column(19).Width = 10;
                        workSheet.Column(20).Width = 13;
                        workSheet.Column(21).Width = 12;
                        workSheet.Column(22).Width = 14;
                        workSheet.Column(23).Width = 11;
                        workSheet.Column(24).Width = 11;

                        workSheet.Row(3).Height = 80;

                        workSheet.Cells["A3:X3"].Style.Font.Bold = true;
                        workSheet.Cells["A3:X3"].Style.Font.Size = 10;
                        workSheet.Cells["A3:X3"].Style.Font.Color.SetColor(Color.Black);
                        workSheet.Cells["A3:X3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        workSheet.Cells["A3:X3"].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                        workSheet.Cells["A3:X3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells["A3:X3"].Style.WrapText = true;

                        workSheet.Cells[3, 1].Value = "Concepto";
                        workSheet.Cells[3, 2].Value = "Tipo de Documentos";
                        workSheet.Cells[3, 3].Value = "Numero de identificación del informado";
                        workSheet.Cells[3, 4].Value = "DV";
                        workSheet.Cells[3, 5].Value = "Primer apellido";
                        workSheet.Cells[3, 6].Value = "Segundo Apellido";
                        workSheet.Cells[3, 7].Value = "Primer Nombre";
                        workSheet.Cells[3, 8].Value = "Otros Nombres";
                        workSheet.Cells[3, 9].Value = "Razón Social";
                        workSheet.Cells[3, 10].Value = "Dirección";
                        workSheet.Cells[3, 11].Value = "Cod. Dpto.";
                        workSheet.Cells[3, 12].Value = "Cod. Mcipio.";
                        workSheet.Cells[3, 13].Value = "País de residencia o domicilio";
                        workSheet.Cells[3, 14].Value = "Pago o abono en cuenta deducible";
                        workSheet.Cells[3, 15].Value = "Pago o abono en cuenta NO deducible";
                        workSheet.Cells[3, 16].Value = "IVA mayor valor del costo o gasto deducible";
                        workSheet.Cells[3, 17].Value = "IVA mayor valor del costo o gasto NO deducible";
                        workSheet.Cells[3, 18].Value = "Retención en la fuente practicada Renta";
                        workSheet.Cells[3, 19].Value = "Retención en la fuente asumida Renta";
                        workSheet.Cells[3, 20].Value = "Retención en la fuente practicada IVA régimen común";
                        workSheet.Cells[3, 21].Value = "Retención en la fuente asumida IVA régimen Simplificado";
                        workSheet.Cells[3, 22].Value = "Retención en la fuente practicada IVA no domiciliados";
                        workSheet.Cells[3, 23].Value = "Retención en la fuente practicada por CREE";
                        workSheet.Cells[3, 24].Value = "Retención en la fuente asumida por CREE";

                        workSheet.Cells[4, 1].LoadFromCollection(data, false);
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            Response.AddHeader("content-disposition", "attachment;  filename=" + nomArchivo + ".xlsx");
                            excel.SaveAs(memoryStream);
                            memoryStream.WriteTo(Response.OutputStream);
                            Response.Flush();
                            Response.End();
                        }

                        #endregion
                    }

                    if (deci == "1003")
                    {
                        #region Archivo Excel 1003

                        string nomArchivo = "Dian " + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day +
                                         DateTime.Now.Minute + "_1003";
                        List<vw_medios_magneticos_excel_tope> envioExcel = db.vw_medios_magneticos_excel_tope.Where(predicate).ToList();
                        var data = envioExcel.Select(d => new
                        {
                            d.NomCon,
                            d.TipoDocumento,
                            d.doc_tercero,
                            d.digito_verificacion,
                            d.apellido_tercero,
                            d.segapellido_tercero,
                            d.prinom_tercero,
                            d.segnom_tercero,
                            d.razon_social,
                            d.direccion,
                            d.cod_dpto,
                            d.cod_ciudad,
                            // d.cod_pais,
                            d.val1,
                            d.val2
                        }).ToArray();

                        ExcelPackage excel = new ExcelPackage();
                        ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Formato1003");

                        workSheet.Cells[1, 1].Value = "Reporte - Dian Medios Magnetico";
                        workSheet.Cells[2, 1].Value = nomArchivo;

                        workSheet.Column(1).Width = 10;
                        workSheet.Column(2).Width = 12;
                        workSheet.Column(3).Width = 14;
                        workSheet.Column(4).Width = 4;
                        workSheet.Column(5).Width = 10;
                        workSheet.Column(6).Width = 10;
                        workSheet.Column(7).Width = 10;
                        workSheet.Column(8).Width = 10;
                        workSheet.Column(9).Width = 16;
                        workSheet.Column(10).Width = 20;
                        workSheet.Column(11).Width = 7;
                        workSheet.Column(12).Width = 10;
                        // workSheet.Column(13).Width = 12;
                        workSheet.Column(13).Width = 12;
                        workSheet.Column(14).Width = 12;


                        workSheet.Row(3).Height = 80;

                        workSheet.Cells["A3:N3"].Style.Font.Bold = true;
                        workSheet.Cells["A3:N3"].Style.Font.Size = 10;
                        workSheet.Cells["A3:N3"].Style.Font.Color.SetColor(Color.Black);
                        workSheet.Cells["A3:N3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        workSheet.Cells["A3:N3"].Style.Fill.BackgroundColor.SetColor(Color.Gray);
                        workSheet.Cells["A3:N3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells["A3:N3"].Style.WrapText = true;

                        workSheet.Cells[3, 1].Value = "Concepto";
                        workSheet.Cells[3, 2].Value = "Tipo de Documentos";
                        workSheet.Cells[3, 3].Value = "Número de identificación del informado";
                        workSheet.Cells[3, 4].Value = "DV";
                        workSheet.Cells[3, 5].Value = "Primer apellido";
                        workSheet.Cells[3, 6].Value = "Segundo Apellido";
                        workSheet.Cells[3, 7].Value = "Primer Nombre";
                        workSheet.Cells[3, 8].Value = "Otros Nombres";
                        workSheet.Cells[3, 9].Value = "Razón Social";
                        workSheet.Cells[3, 10].Value = "Dirección";
                        workSheet.Cells[3, 11].Value = "Cod. Dpto.";
                        workSheet.Cells[3, 12].Value = "Cod. Mcipio.";
                        //        workSheet.Cells[3, 13].Value = "País de residencia o domicilio";
                        workSheet.Cells[3, 13].Value = "Valor acumulado del pago o abono sujeto a retención";
                        workSheet.Cells[3, 14].Value =
                            "Valor de la retención a título de renta, CREE,  o de IVA que le practicaron";

                        workSheet.Cells[4, 1].LoadFromCollection(data, false);
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            Response.AddHeader("content-disposition", "attachment;  filename=" + nomArchivo + ".xlsx");
                            excel.SaveAs(memoryStream);
                            memoryStream.WriteTo(Response.OutputStream);
                            Response.Flush();
                            Response.End();
                        }

                        #endregion
                    }
                }
            }
            else
            {
                return RedirectToAction("Crearmmconstante", new { });
            }

            return RedirectToAction("Crearmmconstante", new { });

            //return View();
        }

        public JsonResult MediosXML(int? excelanio, string[] excelformato)
        {
            bool result = true;
            string txtCampo1 = "1000";
            string txtCampo2 = "2000";
            string txtCampo3 = "3000";
            string txtCampo4 = "4000";


            XDocument documento = new XDocument(new XDeclaration("1.0", "utf-8", null));
            XElement nodoRaiz = new XElement("XMLformato1001");
            documento.Add(nodoRaiz);
            XElement formato = new XElement("formato1001");
            formato.Add(new XElement("Campo1", txtCampo1));
            formato.Add(new XElement("Campo2", txtCampo2));
            formato.Add(new XElement("Campo3", txtCampo3));
            formato.Add(new XElement("Campo4", txtCampo4));
            nodoRaiz.Add(formato);
            documento.Save(@"c:\formatosxml\formato1001.xml");

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarContab(int? Wannio_gmm, int? Wforma_gmm, string[] Wconce_gmm, bool opcionTope)
        {
            bool opcionTopelocal = opcionTope;
            int Auxnit = 4275;

            #region Declaracion de PredicateBuilder

            //Declaramos el predicado de tipo And con el parámetro True. Si fuera de tipo Or lo declaramos con el parámetro False
            System.Linq.Expressions.Expression<Func<vw_conceptosFiltradosGroup, bool>> predicate = PredicateBuilder.True<vw_conceptosFiltradosGroup>();
            System.Linq.Expressions.Expression<Func<vw_conceptosFiltradosGroup, bool>> predicate2 = PredicateBuilder.False<vw_conceptosFiltradosGroup>();
            System.Linq.Expressions.Expression<Func<vw_medios_magneticosdb, bool>> predicateMediosMagneticosNormalAnd = PredicateBuilder.True<vw_medios_magneticosdb>();
            System.Linq.Expressions.Expression<Func<vw_medios_magneticosdb, bool>> predicateMediosMagneticosNormalOr = PredicateBuilder.False<vw_medios_magneticosdb>();

            #endregion

            #region Criterios aplicados a los PredicateBuilder

            if (Wannio_gmm != null)
            {
                predicate = predicate.And(x => x.ano == Wannio_gmm);
                predicateMediosMagneticosNormalAnd = predicateMediosMagneticosNormalAnd.And(x => x.ano == Wannio_gmm);
            }

            if (Wforma_gmm != null)
            {
                predicate = predicate.And(x => x.formato == Wforma_gmm);
                predicateMediosMagneticosNormalAnd =
                    predicateMediosMagneticosNormalAnd.And(x => x.formato == Wforma_gmm);
            }

            //if (!string.IsNullOrWhiteSpace(Wconce_gmm))
            if (Wconce_gmm.Count() > 0)

            {
                //separar lo que me traigo en una lista
                //string[] conceptosid = Wconce_gmm.Split(',');
                foreach (string j in Wconce_gmm)
                {
                    int con = Convert.ToInt32(j);
                    predicate2 = predicate2.Or(x => x.concepto == con);
                    predicateMediosMagneticosNormalOr = predicateMediosMagneticosNormalOr.Or(x => x.concepto == con);
                }

                predicate = predicate.And(predicate2);
                predicateMediosMagneticosNormalAnd =
                    predicateMediosMagneticosNormalAnd.And(predicateMediosMagneticosNormalOr);
            }

            #endregion

            #region Eliminar contenido de medios_magneticosdb

            if (Wconce_gmm.Count() > 0)
            {
                foreach (string k in Wconce_gmm)
                {
                    int con = Convert.ToInt32(k);
                    db.medios_magneticosbd.RemoveRange(db.medios_magneticosbd.Where(x =>
                        x.ano == Wannio_gmm && x.formato == Wforma_gmm && x.concepto == con));
                    db.SaveChanges();
                }
            }

            #endregion


            // predicate = predicate.And(x => x.row1 != null);
            List<vw_conceptosFiltradosGroup> registrosMediosMagneticos = db.vw_conceptosFiltradosGroup.Where(predicate).ToList();

            var data = registrosMediosMagneticos.Select(d => new
            {
                d.ano,
                d.formato,
                d.concepto,
                d.nit,
                d.tipomovimiento,
                d.SubDebito,
                d.SumaCredito,
                d.SumaBase,
                d.idvalor,
                d.operacion
            }).ToList();


            //#region Crear o modificar Medios Magneticos tabla medios_magneticosbd


            bool result = false;

            if (data != null || data.Count > 0)
            {
                if (Wconce_gmm.Count() > 0)
                {
                    foreach (string Wcon2 in Wconce_gmm)
                    {
                        int con2 = Convert.ToInt32(Wcon2);
                        //****************************************

                        //		//****************************************
                        //	}
                        //}

                        #region que recorre vw_conceptosFiltradosGroup

                        for (int i = 0; i < data.Count; i++)
                        {
                            int localannio = data[i].ano;
                            int localforma = data[i].formato;
                            int localconce = data[i].concepto;
                            int localnit = data[i].nit;
                            string localopera = data[i].operacion;
                            int localmovim = data[i].tipomovimiento;
                            /// ***********************************************************************************************************************************
                            /// 
                            /// *****************************************    sin tope 29 10 18   *****************************************************************
                            /// 
                            /// ***********************************************************************************************************************************
                            medios_magneticosbd buscarNit = db.medios_magneticosbd.FirstOrDefault(c =>
                                c.ano == localannio && c.formato == localforma && c.concepto == con2 &&
                                c.nit == localnit);
                            if (buscarNit != null)
                            {
                                #region Actualizar Datos Valor 1

                                // valor 1 
                                if (data[i].idvalor == 1)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val1 = buscarNit.val1 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val1 = buscarNit.val1 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val1 = buscarNit.val1 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val1 = buscarNit.val1 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val1 =
                                            buscarNit.val1 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val1 =
                                            buscarNit.val1 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val1 = buscarNit.val1 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val1 = buscarNit.val1 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Actualizar Datos Valor 2

                                // valor 2
                                if (data[i].idvalor == 2)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val2 = buscarNit.val2 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val2 = buscarNit.val2 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val2 = buscarNit.val2 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val2 = buscarNit.val2 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val2 =
                                            buscarNit.val2 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val2 =
                                            buscarNit.val2 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val2 = buscarNit.val2 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val2 = buscarNit.val2 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Actualizar Datos Valor 3

                                // valor 3
                                if (data[i].idvalor == 3)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val3 = buscarNit.val3 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val3 = buscarNit.val3 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val3 = buscarNit.val3 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val3 = buscarNit.val3 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val3 =
                                            buscarNit.val3 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val3 =
                                            buscarNit.val3 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val3 = buscarNit.val3 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val3 = buscarNit.val3 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Actualizar Datos Valor 4

                                // valor 4
                                if (data[i].idvalor == 4)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val4 = buscarNit.val4 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val4 = buscarNit.val4 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val4 = buscarNit.val4 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val4 = buscarNit.val4 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val4 =
                                            buscarNit.val4 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val4 =
                                            buscarNit.val4 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val4 = buscarNit.val4 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val4 = buscarNit.val4 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Actualizar Datos Valor 5

                                // valor 5
                                if (data[i].idvalor == 5)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val5 = buscarNit.val5 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val5 = buscarNit.val5 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val5 = buscarNit.val5 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val5 = buscarNit.val5 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val5 =
                                            buscarNit.val5 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val5 =
                                            buscarNit.val5 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val5 = buscarNit.val5 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val5 = buscarNit.val5 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Actualizar Datos Valor 6

                                // valor 6
                                if (data[i].idvalor == 6)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val6 = buscarNit.val6 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val6 = buscarNit.val6 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val6 = buscarNit.val6 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val6 = buscarNit.val6 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val6 =
                                            buscarNit.val6 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val6 =
                                            buscarNit.val6 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val6 = buscarNit.val6 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val6 = buscarNit.val6 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Actualizar Datos Valor 7

                                // valor 7
                                if (data[i].idvalor == 7)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val7 = buscarNit.val7 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val7 = buscarNit.val7 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val7 = buscarNit.val7 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val7 = buscarNit.val7 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val7 =
                                            buscarNit.val7 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val7 =
                                            buscarNit.val7 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val7 = buscarNit.val7 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val7 = buscarNit.val7 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Actualizar Datos Valor 8

                                // valor 8
                                if (data[i].idvalor == 8)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val8 = buscarNit.val8 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val8 = buscarNit.val8 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val8 = buscarNit.val8 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val8 = buscarNit.val8 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val8 =
                                            buscarNit.val8 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val8 =
                                            buscarNit.val8 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val8 = buscarNit.val8 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val8 = buscarNit.val8 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Actualizar Datos Valor 9

                                // valor 9
                                if (data[i].idvalor == 9)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val9 = buscarNit.val9 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val9 = buscarNit.val9 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val9 = buscarNit.val9 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val9 = buscarNit.val9 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val9 =
                                            buscarNit.val9 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val9 =
                                            buscarNit.val9 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val9 = buscarNit.val9 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val9 = buscarNit.val9 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Actualizar Datos Valor 10

                                // valor 10
                                if (data[i].idvalor == 10)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val10 = buscarNit.val10 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val10 = buscarNit.val10 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val10 = buscarNit.val10 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val10 = buscarNit.val10 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val10 =
                                            buscarNit.val10 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val10 =
                                            buscarNit.val10 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val10 = buscarNit.val10 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val10 = buscarNit.val10 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Actualizar Datos Valor 11

                                // valor 11
                                if (data[i].idvalor == 11)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val11 = buscarNit.val11 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val11 = buscarNit.val11 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val11 = buscarNit.val11 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val11 = buscarNit.val11 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val11 =
                                            buscarNit.val11 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val11 =
                                            buscarNit.val11 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val11 = buscarNit.val11 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val11 = buscarNit.val11 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                db.Entry(buscarNit).State = EntityState.Modified;
                                int actualizar = db.SaveChanges();
                                if (actualizar > 0)
                                {
                                    // result = true;
                                    //return Json(result, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                #region Iniciliazar temporales

                                decimal temp1 = 0;
                                decimal temp2 = 0;
                                decimal temp3 = 0;
                                decimal temp4 = 0;
                                decimal temp5 = 0;
                                decimal temp6 = 0;
                                decimal temp7 = 0;
                                decimal temp8 = 0;
                                decimal temp9 = 0;
                                decimal temp10 = 0;
                                decimal temp11 = 0;

                                #endregion

                                #region Calculos para los campos de Valor 1

                                //valor 1
                                if (data[i].idvalor == 1)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp1 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp1 = temp1 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp1 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp1 = temp1 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp1 = Convert.ToDecimal(data[i].SubDebito) -
                                                Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp1 = temp1 - (Convert.ToDecimal(data[i].SubDebito) -
                                                         Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp1 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp1 = temp1 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Calculos para los campos de Valor 2

                                //valor 2
                                if (data[i].idvalor == 2)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp2 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp2 = temp2 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp2 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp2 = temp2 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp2 = Convert.ToDecimal(data[i].SubDebito) -
                                                Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp2 = temp2 - (Convert.ToDecimal(data[i].SubDebito) -
                                                         Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp2 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp2 = temp2 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Calculos para los campos de Valor 3

                                //valor 3
                                if (data[i].idvalor == 3)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp3 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp3 = temp3 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp3 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp3 = temp3 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp3 = Convert.ToDecimal(data[i].SubDebito) -
                                                Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp3 = temp3 - (Convert.ToDecimal(data[i].SubDebito) -
                                                         Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp3 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp3 = temp3 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Calculos para los campos de Valor 4

                                //valor 4
                                if (data[i].idvalor == 4)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp4 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp4 = temp4 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp4 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp4 = temp4 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp4 = Convert.ToDecimal(data[i].SubDebito) -
                                                Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp4 = temp4 - (Convert.ToDecimal(data[i].SubDebito) -
                                                         Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp4 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp4 = temp4 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Calculos para los campos de Valor 5

                                // valor 5
                                if (data[i].idvalor == 5)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp5 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp5 = temp5 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp5 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp5 = temp5 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp5 = Convert.ToDecimal(data[i].SubDebito) -
                                                Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp5 = temp5 - (Convert.ToDecimal(data[i].SubDebito) -
                                                         Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp5 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp5 = temp5 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Calculos para los campos de Valor 6

                                // valor 6
                                if (data[i].idvalor == 6)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp6 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp6 = temp6 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp6 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp6 = temp6 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp6 = Convert.ToDecimal(data[i].SubDebito) -
                                                Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp6 = temp6 - (Convert.ToDecimal(data[i].SubDebito) -
                                                         Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp6 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp6 = temp6 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Calculos para los campos de Valor 7

                                //valor 7
                                if (data[i].idvalor == 7)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp7 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp7 = temp7 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp7 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp7 = temp7 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp7 = Convert.ToDecimal(data[i].SubDebito) -
                                                Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp7 = temp7 - (Convert.ToDecimal(data[i].SubDebito) -
                                                         Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp7 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp7 = temp7 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Calculos para los campos de Valor 8

                                //valor 8
                                if (data[i].idvalor == 8)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp8 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp8 = temp8 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp8 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp8 = temp8 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp8 = Convert.ToDecimal(data[i].SubDebito) -
                                                Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp8 = temp8 - (Convert.ToDecimal(data[i].SubDebito) -
                                                         Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp8 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp8 = temp8 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Calculos para los campos de Valor 9

                                //valor 9
                                if (data[i].idvalor == 9)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp9 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp9 = temp9 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp9 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp9 = temp9 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp9 = Convert.ToDecimal(data[i].SubDebito) -
                                                Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp9 = temp9 - (Convert.ToDecimal(data[i].SubDebito) -
                                                         Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp9 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp9 = temp9 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Calculos para los campos de Valor 10

                                //valor 10
                                if (data[i].idvalor == 10)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp10 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp10 = temp10 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp10 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp10 = temp10 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp10 = Convert.ToDecimal(data[i].SubDebito) -
                                                 Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp10 = temp10 - (Convert.ToDecimal(data[i].SubDebito) -
                                                           Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp10 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp10 = temp10 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Calculos para los campos de Valor 11

                                //valor 11
                                if (data[i].idvalor == 11)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp11 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp11 = temp11 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp11 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp11 = temp11 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp11 = Convert.ToDecimal(data[i].SubDebito) -
                                                 Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp11 = temp11 - (Convert.ToDecimal(data[i].SubDebito) -
                                                           Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp11 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp11 = temp11 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                db.medios_magneticosbd.Add(new medios_magneticosbd
                                {
                                    ano = data[i].ano,
                                    formato = data[i].formato,
                                    concepto = data[i].concepto,
                                    nit = data[i].nit,
                                    val1 = temp1,
                                    val2 = temp2,
                                    val3 = temp3,
                                    val4 = temp4,
                                    val5 = temp5,
                                    val6 = temp6,
                                    val7 = temp7,
                                    val8 = temp8,
                                    val9 = temp9,
                                    val10 = temp10,
                                    val11 = temp11
                                });
                                int guardar = db.SaveChanges();
                                if (guardar > 0)
                                {
                                    // result = true;
                                }
                            } // fin actualiza o crea data de medios_magneticosdb

                            localopera = "";
                        } // final del if que recorre el data[i]

                        #endregion

                        //				#endregion

                        // inicio Agregar los valores existentes en constantes a los nit correspondientes

                        #region Suma el valor de Constantes a medios_magneticosbd

                        List<medios_constantes> registrosConstantes = db.medios_constantes.Where(x =>
                            x.ano == Wannio_gmm && x.formato == Wforma_gmm && x.concepto == con2).ToList();
                        var dataConstantes = registrosConstantes.Select(d => new
                        {
                            d.ano,
                            d.formato,
                            d.concepto,
                            d.nit,
                            d.val1,
                            d.val2,
                            d.val3,
                            d.val4,
                            d.val5,
                            d.val6,
                            d.val7,
                            d.val8,
                            d.val9,
                            d.val10,
                            d.val11
                        }).ToList();

                        if (dataConstantes != null || dataConstantes.Count > 0)
                        {
                            for (int j = 0; j < dataConstantes.Count; j++)
                            {
                                List<medios_magneticosbd> registrosMediosDB = db.medios_magneticosbd.Where(x =>
                                    x.ano == Wannio_gmm && x.formato == Wforma_gmm && x.concepto == con2).ToList();
                                var dataMedios = registrosMediosDB.Select(m => new
                                {
                                    m.ano,
                                    m.formato,
                                    m.concepto,
                                    m.nit,
                                    m.val1,
                                    m.val2,
                                    m.val3,
                                    m.val4,
                                    m.val5,
                                    m.val6,
                                    m.val7,
                                    m.val8,
                                    m.val9,
                                    m.val10,
                                    m.val11
                                }).ToList();

                                #region sdsd

                                if (dataMedios != null)
                                {
                                    for (int k = 0; k < dataMedios.Count; k++)
                                    {
                                        int esteNit = dataMedios[k].nit;
                                        if (dataMedios[k].concepto == dataConstantes[j].concepto &&
                                            dataMedios[k].nit == dataConstantes[j].nit)
                                        {
                                            medios_magneticosbd ActualizaMM = db.medios_magneticosbd.FirstOrDefault(c =>
                                                c.ano == Wannio_gmm && c.formato == Wforma_gmm && c.concepto == con2 &&
                                                c.nit == esteNit);
                                            if (ActualizaMM != null)
                                            {
                                                ActualizaMM.val1 = ActualizaMM.val1 + dataConstantes[j].val1;
                                                ActualizaMM.val2 = ActualizaMM.val2 + dataConstantes[j].val2;
                                                ActualizaMM.val3 = ActualizaMM.val3 + dataConstantes[j].val3;
                                                ActualizaMM.val4 = ActualizaMM.val4 + dataConstantes[j].val4;
                                                ActualizaMM.val5 = ActualizaMM.val5 + dataConstantes[j].val5;
                                                ActualizaMM.val6 = ActualizaMM.val6 + dataConstantes[j].val6;
                                                ActualizaMM.val7 = ActualizaMM.val7 + dataConstantes[j].val7;
                                                ActualizaMM.val8 = ActualizaMM.val8 + dataConstantes[j].val8;
                                                ActualizaMM.val9 = ActualizaMM.val9 + dataConstantes[j].val9;
                                                ActualizaMM.val10 = ActualizaMM.val10 + dataConstantes[j].val10;
                                                ActualizaMM.val11 = ActualizaMM.val11 + dataConstantes[j].val11;

                                                db.Entry(ActualizaMM).State = EntityState.Modified;
                                                db.SaveChanges();
                                            }
                                        }
                                    }
                                }

                                #endregion
                            }
                        }

                        #endregion

                        // finaliza Agregar los valores existentes en constantes a los nit correspondientes


                        /// Para Borrar "medios_magneticos_topes" 

                        #region Eliminar contenido de medios_magneticos_topes

                        if (Wconce_gmm.Count() > 0)
                        {
                            foreach (string k in Wconce_gmm)
                            {
                                int con = Convert.ToInt32(k);
                                db.medios_magneticos_topes.RemoveRange(db.medios_magneticos_topes.Where(x =>
                                    x.ano == Wannio_gmm && x.formato == Wforma_gmm && x.concepto == con));
                                db.SaveChanges();
                            }
                        }

                        #endregion

                        /// Para Borrar "medios_magneticos_topes"

                        #region CON TOPE

                        if (opcionTopelocal)
                        {
                            //Declaramos el predicado de tipo And con el parámetro True. Si fuera de tipo Or lo declaramos con el parámetro False
                            System.Linq.Expressions.Expression<Func<vw_medios_magneticosdb, bool>> predicatedb = PredicateBuilder.True<vw_medios_magneticosdb>();
                            System.Linq.Expressions.Expression<Func<vw_medios_magneticosdb, bool>> predicatedb2 = PredicateBuilder.False<vw_medios_magneticosdb>();
                            System.Linq.Expressions.Expression<Func<vw_medios_magneticosdb, bool>> predicatedb3 = PredicateBuilder.True<vw_medios_magneticosdb>();

                            // var predicadoConstantes = PredicateBuilder.True<Vista_ConceptosUnicos_medios_magenticosdb>();
                            if (Wannio_gmm != null)
                            {
                                predicatedb = predicatedb.And(x => x.ano == Wannio_gmm);
                            }
                            //predicadoConcepto = predicadoConcepto.And(x => x.ano == Wannio_gmm);
                            if (Wforma_gmm != null)
                            {
                                predicatedb = predicatedb.And(x => x.formato == Wforma_gmm);
                            }
                            //predicadoConcepto = predicadoConcepto.And(x => x.formato == Wforma_gmm);
                            predicatedb = predicatedb.And(x => x.val1 < x.tope);

                            if (Wconce_gmm.Count() > 0)

                            {
                                foreach (string j in Wconce_gmm)
                                {
                                    int con = Convert.ToInt32(j);
                                    predicatedb2 = predicatedb2.Or(x => x.concepto == con);
                                    predicatedb3 = predicatedb.And(x => x.concepto == con);
                                    List<vw_medios_magneticosdb> registrosPorConcepto = db.vw_medios_magneticosdb.Where(predicatedb3).ToList();
                                    var data03 = registrosPorConcepto.Select(c => new
                                    {
                                        c.id,
                                        c.ano,
                                        c.formato,
                                        c.concepto,
                                        c.nit
                                    }).ToList();

                                    if (data03.Count > 0)
                                    {
                                        db.medios_magneticos_topes.Add(new medios_magneticos_topes
                                        {
                                            ano = data03[0].ano,
                                            formato = data03[0].formato,
                                            concepto = data03[0].concepto,
                                            nit = Auxnit
                                        });
                                        db.SaveChanges();
                                    }
                                }

                                predicatedb = predicatedb.And(predicatedb2);
                            }

                            List<vw_medios_magneticosdb> registrosMediosMagneticoscontopedb =
                                db.vw_medios_magneticosdb.Where(predicatedb).ToList();
                            var data01 = registrosMediosMagneticoscontopedb.Select(c => new
                            {
                                c.id,
                                c.ano,
                                c.formato,
                                c.concepto,
                                c.nit,
                                c.val1,
                                c.val2,
                                c.val3,
                                c.val4,
                                c.val5,
                                c.val6,
                                c.val7,
                                c.val8,
                                c.val9,
                                c.val10,
                                c.val11,
                                c.desformato,
                                c.tope
                            }).ToList();

                            if (data01.Count > 0)
                            {
                                int AuxConGlobal = 0;
                                for (int i = 0; i < data01.Count; i++)
                                {
                                    int Auxconceptos = data01[i].concepto;
                                    AuxConGlobal = Auxconceptos;
                                    medios_magneticos_topes buscar2222 = db.medios_magneticos_topes.FirstOrDefault(c =>
                                        c.ano == Wannio_gmm && c.formato == Wforma_gmm && c.concepto == Auxconceptos);
                                    if (buscar2222 != null)
                                    {
                                        #region Variables 222

                                        //var tempannio = data01[i].ano;
                                        //var tempforma = data01[i].formato;
                                        //// var tempconce = 22;
                                        //var tempval1 = data01[i].val1;
                                        //var tempval2 = data01[i].val2;
                                        //var tempval3 = data01[i].val3;
                                        //var tempval4 = data01[i].val4;
                                        //var tempval5 = data01[i].val5;
                                        //var tempval6 = data01[i].val6;
                                        //var tempval7 = data01[i].val7;
                                        //var tempval8 = data01[i].val8;
                                        //var tempval9 = data01[i].val9;
                                        //var tempval10 = data01[i].val10;
                                        //var tempval11 = data01[i].val11;

                                        #endregion

                                        #region inicializar variables

                                        //buscar2222.val1 = 0;
                                        //buscar2222.val2 = 0;
                                        //buscar2222.val3 = 0;
                                        //buscar2222.val4 = 0;
                                        //buscar2222.val5 = 0;
                                        //buscar2222.val6 = 0;
                                        //buscar2222.val7 = 0;
                                        //buscar2222.val8 = 0;
                                        //buscar2222.val9 = 0;
                                        //buscar2222.val10 = 0;
                                        //buscar2222.val11 = 0;

                                        #endregion

                                        #region Calculos 222

                                        if (data01[i].val1 < data01[i].tope)
                                        {
                                            buscar2222.val1 = buscar2222.val1 + data01[i].val1;
                                            buscar2222.val2 = buscar2222.val2 + data01[i].val2;
                                            buscar2222.val3 = buscar2222.val3 + data01[i].val3;
                                            buscar2222.val4 = buscar2222.val4 + data01[i].val4;
                                            buscar2222.val5 = buscar2222.val5 + data01[i].val5;
                                            buscar2222.val6 = buscar2222.val6 + data01[i].val6;
                                            buscar2222.val7 = buscar2222.val7 + data01[i].val7;
                                            buscar2222.val8 = buscar2222.val8 + data01[i].val8;
                                            buscar2222.val9 = buscar2222.val9 + data01[i].val9;
                                            buscar2222.val10 = buscar2222.val10 + data01[i].val10;
                                            buscar2222.val11 = buscar2222.val11 + data01[i].val11;

                                            db.Entry(buscar2222).State = EntityState.Modified;
                                            db.SaveChanges();
                                        }

                                        #endregion

                                        #region Calculos NO 2222

                                        if (data01[i].val1 > data01[i].tope)
                                        {
                                            buscar2222.val1 = buscar2222.val1 + data01[i].val1;
                                            buscar2222.val2 = buscar2222.val2 + data01[i].val2;
                                            buscar2222.val3 = buscar2222.val3 + data01[i].val3;
                                            buscar2222.val4 = buscar2222.val4 + data01[i].val4;
                                            buscar2222.val5 = buscar2222.val5 + data01[i].val5;
                                            buscar2222.val6 = buscar2222.val6 + data01[i].val6;
                                            buscar2222.val7 = buscar2222.val7 + data01[i].val7;
                                            buscar2222.val8 = buscar2222.val8 + data01[i].val8;
                                            buscar2222.val9 = buscar2222.val9 + data01[i].val9;
                                            buscar2222.val10 = buscar2222.val10 + data01[i].val10;
                                            buscar2222.val11 = buscar2222.val11 + data01[i].val11;

                                            //db.Entry(buscar2222).State = EntityState.Modified;
                                            //db.SaveChanges();
                                        }

                                        #endregion
                                    }
                                    //}


                                    List<vw_medios_magneticosdb> registrosno2222 = db.vw_medios_magneticosdb.Where(x =>
                                        x.ano == Wannio_gmm && x.formato == Wforma_gmm && x.concepto == Auxconceptos &&
                                        x.val1 > x.tope).ToList();
                                    var dataregno2222 = registrosno2222.Select(m => new
                                    {
                                        m.ano,
                                        m.formato,
                                        m.concepto,
                                        m.nit,
                                        m.val1,
                                        m.val2,
                                        m.val3,
                                        m.val4,
                                        m.val5,
                                        m.val6,
                                        m.val7,
                                        m.val8,
                                        m.val9,
                                        m.val10,
                                        m.val11
                                    }).ToList();
                                    if (dataregno2222 != null)
                                    {
                                        for (int m = 0; m < dataregno2222.Count; m++)
                                        {
                                            db.medios_magneticos_topes.Add(new medios_magneticos_topes
                                            {
                                                ano = dataregno2222[m].ano,
                                                formato = dataregno2222[m].formato,
                                                concepto = dataregno2222[m].concepto,
                                                nit = dataregno2222[m].nit,
                                                val1 = dataregno2222[m].val1,
                                                val2 = dataregno2222[m].val2,
                                                val3 = dataregno2222[m].val3,
                                                val4 = dataregno2222[m].val4,
                                                val5 = dataregno2222[m].val5,
                                                val6 = dataregno2222[m].val6,
                                                val7 = dataregno2222[m].val7,
                                                val8 = dataregno2222[m].val8,
                                                val9 = dataregno2222[m].val9,
                                                val10 = dataregno2222[m].val10,
                                                val11 = dataregno2222[m].val11
                                            });
                                            db.SaveChanges();
                                        }
                                    }

                                    result = true;
                                } // 
                            }
                        } // final con Tope

                        #endregion

                        //***************************************************** Ingresar Registros ************************************************************************

                        #region "OpcionTope == false" 

                        if (opcionTopelocal == false)
                        {
                            List<vw_medios_magneticosdb> registrosNormal = db.vw_medios_magneticosdb.Where(predicateMediosMagneticosNormalAnd)
                                .ToList();
                            var dataregNormal = registrosNormal.Select(m => new
                            {
                                m.ano,
                                m.formato,
                                m.concepto,
                                m.nit,
                                m.val1,
                                m.val2,
                                m.val3,
                                m.val4,
                                m.val5,
                                m.val6,
                                m.val7,
                                m.val8,
                                m.val9,
                                m.val10,
                                m.val11
                            }).ToList();
                            if (dataregNormal != null || dataregNormal.Count > 0)
                            {
                                for (int m = 0; m < dataregNormal.Count; m++)
                                {
                                    db.medios_magneticos_topes.Add(new medios_magneticos_topes
                                    {
                                        ano = dataregNormal[m].ano,
                                        formato = dataregNormal[m].formato,
                                        concepto = dataregNormal[m].concepto,
                                        nit = dataregNormal[m].nit,
                                        val1 = dataregNormal[m].val1,
                                        val2 = dataregNormal[m].val2,
                                        val3 = dataregNormal[m].val3,
                                        val4 = dataregNormal[m].val4,
                                        val5 = dataregNormal[m].val5,
                                        val6 = dataregNormal[m].val6,
                                        val7 = dataregNormal[m].val7,
                                        val8 = dataregNormal[m].val8,
                                        val9 = dataregNormal[m].val9,
                                        val10 = dataregNormal[m].val10,
                                        val11 = dataregNormal[m].val11
                                    });
                                    db.SaveChanges();
                                    result = true;
                                }
                            }

                            //  #endregion
                            //***************************************************** Ingresar Registros   *****************************************************************************
                        }

                        #endregion


                        //****************************************
                    }
                }

                ///ojo hasta aqui 
            }
            else
            {
                ///esto ocurre si no hay data que venga de la consulta de la vista
                result = false;
                //return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarContabTope(int? Wannio_gmm, int? Wforma_gmm, string[] Wconce_gmm, bool opcionTope)
        {
            bool opcionTopelocal = opcionTope;
            int Auxnit = 4275;

            #region Declaracion de PredicateBuilder

            //Declaramos el predicado de tipo And con el parámetro True. Si fuera de tipo Or lo declaramos con el parámetro False
            System.Linq.Expressions.Expression<Func<vw_conceptosFiltradosGroup, bool>> predicate = PredicateBuilder.True<vw_conceptosFiltradosGroup>();
            System.Linq.Expressions.Expression<Func<vw_conceptosFiltradosGroup, bool>> predicate2 = PredicateBuilder.False<vw_conceptosFiltradosGroup>();
            System.Linq.Expressions.Expression<Func<vw_medios_magneticos_tope, bool>> predicateMediosMagneticosNormalAnd = PredicateBuilder.True<vw_medios_magneticos_tope>();
            System.Linq.Expressions.Expression<Func<vw_medios_magneticos_tope, bool>> predicateMediosMagneticosNormalOr = PredicateBuilder.False<vw_medios_magneticos_tope>();

            #endregion

            #region Criterios aplicados a los PredicateBuilder

            if (Wannio_gmm != null)
            {
                predicate = predicate.And(x => x.ano == Wannio_gmm);
                predicateMediosMagneticosNormalAnd = predicateMediosMagneticosNormalAnd.And(x => x.ano == Wannio_gmm);
            }

            if (Wforma_gmm != null)
            {
                predicate = predicate.And(x => x.formato == Wforma_gmm);
                predicateMediosMagneticosNormalAnd =
                    predicateMediosMagneticosNormalAnd.And(x => x.formato == Wforma_gmm);
            }

            //if (!string.IsNullOrWhiteSpace(Wconce_gmm))
            if (Wconce_gmm.Count() > 0)

            {
                //separar lo que me traigo en una lista
                //string[] conceptosid = Wconce_gmm.Split(',');
                foreach (string j in Wconce_gmm)
                {
                    int con = Convert.ToInt32(j);
                    predicate2 = predicate2.Or(x => x.concepto == con);
                    predicateMediosMagneticosNormalOr = predicateMediosMagneticosNormalOr.Or(x => x.concepto == con);
                }

                predicate = predicate.And(predicate2);
                predicateMediosMagneticosNormalAnd =
                    predicateMediosMagneticosNormalAnd.And(predicateMediosMagneticosNormalOr);
            }

            #endregion

            #region Eliminar contenido de medios_magneticos_topes

            if (Wconce_gmm.Count() > 0)
            {
                foreach (string k in Wconce_gmm)
                {
                    int con = Convert.ToInt32(k);
                    db.medios_magneticos_topes.RemoveRange(db.medios_magneticos_topes.Where(x =>
                        x.ano == Wannio_gmm && x.formato == Wforma_gmm && x.concepto == con));
                    db.SaveChanges();
                }
            }

            #endregion


            // predicate = predicate.And(x => x.row1 != null);
            List<vw_conceptosFiltradosGroup> registrosMediosMagneticos = db.vw_conceptosFiltradosGroup.Where(predicate).ToList();

            var data = registrosMediosMagneticos.Select(d => new
            {
                d.ano,
                d.formato,
                d.concepto,
                d.nit,
                d.tipomovimiento,
                d.SubDebito,
                d.SumaCredito,
                d.SumaBase,
                d.idvalor,
                d.operacion
            }).ToList();


            //#region Crear o modificar Medios Magneticos tabla medios_magneticosbd


            bool result = false;

            if (data != null || data.Count > 0)
            {
                if (Wconce_gmm.Count() > 0)
                {
                    foreach (string Wcon2 in Wconce_gmm)
                    {
                        int con2 = Convert.ToInt32(Wcon2);
                        //****************************************

                        //		//****************************************
                        //	}
                        //}

                        #region que recorre vw_conceptosFiltradosGroup

                        for (int i = 0; i < data.Count; i++)
                        {
                            int localannio = data[i].ano;
                            int localforma = data[i].formato;
                            int localconce = data[i].concepto;
                            int localnit = data[i].nit;
                            string localopera = data[i].operacion;
                            int localmovim = data[i].tipomovimiento;
                            /// ***********************************************************************************************************************************
                            /// 
                            /// *****************************************    sin tope 29 10 18   *****************************************************************
                            /// 
                            /// ***********************************************************************************************************************************
                            medios_magneticos_topes buscarNit = db.medios_magneticos_topes.FirstOrDefault(c =>
                                c.ano == localannio && c.formato == localforma && c.concepto == con2 &&
                                c.nit == localnit);
                            if (buscarNit != null)
                            {
                                #region Actualizar Datos Valor 1

                                // valor 1 
                                if (data[i].idvalor == 1)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val1 = buscarNit.val1 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val1 = buscarNit.val1 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val1 = buscarNit.val1 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val1 = buscarNit.val1 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val1 =
                                            buscarNit.val1 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val1 =
                                            buscarNit.val1 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val1 = buscarNit.val1 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val1 = buscarNit.val1 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Actualizar Datos Valor 2

                                // valor 2
                                if (data[i].idvalor == 2)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val2 = buscarNit.val2 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val2 = buscarNit.val2 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val2 = buscarNit.val2 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val2 = buscarNit.val2 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val2 =
                                            buscarNit.val2 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val2 =
                                            buscarNit.val2 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val2 = buscarNit.val2 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val2 = buscarNit.val2 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Actualizar Datos Valor 3

                                // valor 3
                                if (data[i].idvalor == 3)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val3 = buscarNit.val3 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val3 = buscarNit.val3 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val3 = buscarNit.val3 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val3 = buscarNit.val3 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val3 =
                                            buscarNit.val3 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val3 =
                                            buscarNit.val3 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val3 = buscarNit.val3 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val3 = buscarNit.val3 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Actualizar Datos Valor 4

                                // valor 4
                                if (data[i].idvalor == 4)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val4 = buscarNit.val4 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val4 = buscarNit.val4 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val4 = buscarNit.val4 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val4 = buscarNit.val4 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val4 =
                                            buscarNit.val4 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val4 =
                                            buscarNit.val4 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val4 = buscarNit.val4 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val4 = buscarNit.val4 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Actualizar Datos Valor 5

                                // valor 5
                                if (data[i].idvalor == 5)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val5 = buscarNit.val5 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val5 = buscarNit.val5 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val5 = buscarNit.val5 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val5 = buscarNit.val5 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val5 =
                                            buscarNit.val5 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val5 =
                                            buscarNit.val5 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val5 = buscarNit.val5 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val5 = buscarNit.val5 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Actualizar Datos Valor 6

                                // valor 6
                                if (data[i].idvalor == 6)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val6 = buscarNit.val6 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val6 = buscarNit.val6 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val6 = buscarNit.val6 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val6 = buscarNit.val6 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val6 =
                                            buscarNit.val6 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val6 =
                                            buscarNit.val6 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val6 = buscarNit.val6 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val6 = buscarNit.val6 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Actualizar Datos Valor 7

                                // valor 7
                                if (data[i].idvalor == 7)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val7 = buscarNit.val7 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val7 = buscarNit.val7 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val7 = buscarNit.val7 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val7 = buscarNit.val7 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val7 =
                                            buscarNit.val7 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val7 =
                                            buscarNit.val7 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val7 = buscarNit.val7 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val7 = buscarNit.val7 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Actualizar Datos Valor 8

                                // valor 8
                                if (data[i].idvalor == 8)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val8 = buscarNit.val8 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val8 = buscarNit.val8 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val8 = buscarNit.val8 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val8 = buscarNit.val8 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val8 =
                                            buscarNit.val8 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val8 =
                                            buscarNit.val8 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val8 = buscarNit.val8 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val8 = buscarNit.val8 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Actualizar Datos Valor 9

                                // valor 9
                                if (data[i].idvalor == 9)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val9 = buscarNit.val9 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val9 = buscarNit.val9 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val9 = buscarNit.val9 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val9 = buscarNit.val9 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val9 =
                                            buscarNit.val9 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val9 =
                                            buscarNit.val9 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val9 = buscarNit.val9 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val9 = buscarNit.val9 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Actualizar Datos Valor 10

                                // valor 10
                                if (data[i].idvalor == 10)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val10 = buscarNit.val10 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val10 = buscarNit.val10 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val10 = buscarNit.val10 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val10 = buscarNit.val10 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val10 =
                                            buscarNit.val10 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val10 =
                                            buscarNit.val10 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val10 = buscarNit.val10 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val10 = buscarNit.val10 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Actualizar Datos Valor 11

                                // valor 11
                                if (data[i].idvalor == 11)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        buscarNit.val11 = buscarNit.val11 + Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        buscarNit.val11 = buscarNit.val11 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        buscarNit.val11 = buscarNit.val11 + Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        buscarNit.val11 = buscarNit.val11 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        buscarNit.val11 =
                                            buscarNit.val11 +
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        buscarNit.val11 =
                                            buscarNit.val11 -
                                            (Convert.ToDecimal(data[i].SubDebito) -
                                             Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        buscarNit.val11 = buscarNit.val11 + Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        buscarNit.val11 = buscarNit.val11 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                db.Entry(buscarNit).State = EntityState.Modified;
                                int actualizar = db.SaveChanges();
                                if (actualizar > 0)
                                {
                                    // result = true;
                                    //return Json(result, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                #region Iniciliazar temporales

                                decimal temp1 = 0;
                                decimal temp2 = 0;
                                decimal temp3 = 0;
                                decimal temp4 = 0;
                                decimal temp5 = 0;
                                decimal temp6 = 0;
                                decimal temp7 = 0;
                                decimal temp8 = 0;
                                decimal temp9 = 0;
                                decimal temp10 = 0;
                                decimal temp11 = 0;

                                #endregion

                                #region Calculos para los campos de Valor 1

                                //valor 1
                                if (data[i].idvalor == 1)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp1 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp1 = temp1 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp1 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp1 = temp1 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp1 = Convert.ToDecimal(data[i].SubDebito) -
                                                Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp1 = temp1 - (Convert.ToDecimal(data[i].SubDebito) -
                                                         Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp1 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp1 = temp1 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Calculos para los campos de Valor 2

                                //valor 2
                                if (data[i].idvalor == 2)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp2 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp2 = temp2 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp2 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp2 = temp2 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp2 = Convert.ToDecimal(data[i].SubDebito) -
                                                Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp2 = temp2 - (Convert.ToDecimal(data[i].SubDebito) -
                                                         Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp2 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp2 = temp2 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Calculos para los campos de Valor 3

                                //valor 3
                                if (data[i].idvalor == 3)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp3 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp3 = temp3 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp3 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp3 = temp3 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp3 = Convert.ToDecimal(data[i].SubDebito) -
                                                Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp3 = temp3 - (Convert.ToDecimal(data[i].SubDebito) -
                                                         Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp3 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp3 = temp3 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Calculos para los campos de Valor 4

                                //valor 4
                                if (data[i].idvalor == 4)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp4 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp4 = temp4 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp4 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp4 = temp4 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp4 = Convert.ToDecimal(data[i].SubDebito) -
                                                Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp4 = temp4 - (Convert.ToDecimal(data[i].SubDebito) -
                                                         Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp4 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp4 = temp4 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Calculos para los campos de Valor 5

                                // valor 5
                                if (data[i].idvalor == 5)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp5 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp5 = temp5 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp5 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp5 = temp5 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp5 = Convert.ToDecimal(data[i].SubDebito) -
                                                Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp5 = temp5 - (Convert.ToDecimal(data[i].SubDebito) -
                                                         Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp5 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp5 = temp5 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Calculos para los campos de Valor 6

                                // valor 6
                                if (data[i].idvalor == 6)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp6 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp6 = temp6 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp6 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp6 = temp6 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp6 = Convert.ToDecimal(data[i].SubDebito) -
                                                Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp6 = temp6 - (Convert.ToDecimal(data[i].SubDebito) -
                                                         Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp6 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp6 = temp6 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Calculos para los campos de Valor 7

                                //valor 7
                                if (data[i].idvalor == 7)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp7 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp7 = temp7 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp7 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp7 = temp7 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp7 = Convert.ToDecimal(data[i].SubDebito) -
                                                Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp7 = temp7 - (Convert.ToDecimal(data[i].SubDebito) -
                                                         Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp7 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp7 = temp7 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Calculos para los campos de Valor 8

                                //valor 8
                                if (data[i].idvalor == 8)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp8 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp8 = temp8 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp8 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp8 = temp8 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp8 = Convert.ToDecimal(data[i].SubDebito) -
                                                Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp8 = temp8 - (Convert.ToDecimal(data[i].SubDebito) -
                                                         Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp8 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp8 = temp8 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Calculos para los campos de Valor 9

                                //valor 9
                                if (data[i].idvalor == 9)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp9 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp9 = temp9 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp9 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp9 = temp9 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp9 = Convert.ToDecimal(data[i].SubDebito) -
                                                Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp9 = temp9 - (Convert.ToDecimal(data[i].SubDebito) -
                                                         Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp9 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp9 = temp9 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Calculos para los campos de Valor 10

                                //valor 10
                                if (data[i].idvalor == 10)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp10 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp10 = temp10 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp10 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp10 = temp10 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp10 = Convert.ToDecimal(data[i].SubDebito) -
                                                 Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp10 = temp10 - (Convert.ToDecimal(data[i].SubDebito) -
                                                           Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp10 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp10 = temp10 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                #region Calculos para los campos de Valor 11

                                //valor 11
                                if (data[i].idvalor == 11)
                                {
                                    if (localmovim == 1 && localopera == "+")
                                    {
                                        temp11 = Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 1 && localopera == "-")
                                    {
                                        temp11 = temp11 - Convert.ToDecimal(data[i].SubDebito);
                                    }

                                    if (localmovim == 2 && localopera == "+")
                                    {
                                        temp11 = Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 2 && localopera == "-")
                                    {
                                        temp11 = temp11 - Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "+")
                                    {
                                        temp11 = Convert.ToDecimal(data[i].SubDebito) -
                                                 Convert.ToDecimal(data[i].SumaCredito);
                                    }

                                    if (localmovim == 3 && localopera == "-")
                                    {
                                        temp11 = temp11 - (Convert.ToDecimal(data[i].SubDebito) -
                                                           Convert.ToDecimal(data[i].SumaCredito));
                                    }

                                    if (localmovim == 4 && localopera == "+")
                                    {
                                        temp11 = Convert.ToDecimal(data[i].SumaBase);
                                    }

                                    if (localmovim == 4 && localopera == "-")
                                    {
                                        temp11 = temp11 - Convert.ToDecimal(data[i].SumaBase);
                                    }
                                }

                                #endregion

                                db.medios_magneticos_topes.Add(new medios_magneticos_topes
                                {
                                    ano = data[i].ano,
                                    formato = data[i].formato,
                                    concepto = data[i].concepto,
                                    nit = data[i].nit,
                                    val1 = temp1,
                                    val2 = temp2,
                                    val3 = temp3,
                                    val4 = temp4,
                                    val5 = temp5,
                                    val6 = temp6,
                                    val7 = temp7,
                                    val8 = temp8,
                                    val9 = temp9,
                                    val10 = temp10,
                                    val11 = temp11
                                });
                                int guardar = db.SaveChanges();
                                if (guardar > 0)
                                {
                                    // result = true;
                                }
                            } // fin actualiza o crea data de medios_magneticosdb

                            localopera = "";
                        } // final del if que recorre el data[i]

                        #endregion

                        //				#endregion

                        // inicio Agregar los valores existentes en constantes a los nit correspondientes

                        #region Suma el valor de Constantes a medios_magneticosbd

                        List<medios_constantes> registrosConstantes = db.medios_constantes.Where(x =>
                            x.ano == Wannio_gmm && x.formato == Wforma_gmm && x.concepto == con2).ToList();
                        var dataConstantes = registrosConstantes.Select(d => new
                        {
                            d.ano,
                            d.formato,
                            d.concepto,
                            d.nit,
                            d.val1,
                            d.val2,
                            d.val3,
                            d.val4,
                            d.val5,
                            d.val6,
                            d.val7,
                            d.val8,
                            d.val9,
                            d.val10,
                            d.val11
                        }).ToList();

                        if (dataConstantes != null || dataConstantes.Count > 0)
                        {
                            for (int j = 0; j < dataConstantes.Count; j++)
                            {
                                List<medios_magneticos_topes> registrosMediosDB = db.medios_magneticos_topes.Where(x =>
                                    x.ano == Wannio_gmm && x.formato == Wforma_gmm && x.concepto == con2).ToList();
                                var dataMedios = registrosMediosDB.Select(m => new
                                {
                                    m.ano,
                                    m.formato,
                                    m.concepto,
                                    m.nit,
                                    m.val1,
                                    m.val2,
                                    m.val3,
                                    m.val4,
                                    m.val5,
                                    m.val6,
                                    m.val7,
                                    m.val8,
                                    m.val9,
                                    m.val10,
                                    m.val11
                                }).ToList();

                                #region sdsd

                                if (dataMedios != null)
                                {
                                    for (int k = 0; k < dataMedios.Count; k++)
                                    {
                                        int esteNit = dataMedios[k].nit;
                                        if (dataMedios[k].concepto == dataConstantes[j].concepto &&
                                            dataMedios[k].nit == dataConstantes[j].nit)
                                        {
                                            medios_magneticos_topes ActualizaMM = db.medios_magneticos_topes.FirstOrDefault(c =>
                                                c.ano == Wannio_gmm && c.formato == Wforma_gmm && c.concepto == con2 &&
                                                c.nit == esteNit);
                                            if (ActualizaMM != null)
                                            {
                                                ActualizaMM.val1 = ActualizaMM.val1 + dataConstantes[j].val1;
                                                ActualizaMM.val2 = ActualizaMM.val2 + dataConstantes[j].val2;
                                                ActualizaMM.val3 = ActualizaMM.val3 + dataConstantes[j].val3;
                                                ActualizaMM.val4 = ActualizaMM.val4 + dataConstantes[j].val4;
                                                ActualizaMM.val5 = ActualizaMM.val5 + dataConstantes[j].val5;
                                                ActualizaMM.val6 = ActualizaMM.val6 + dataConstantes[j].val6;
                                                ActualizaMM.val7 = ActualizaMM.val7 + dataConstantes[j].val7;
                                                ActualizaMM.val8 = ActualizaMM.val8 + dataConstantes[j].val8;
                                                ActualizaMM.val9 = ActualizaMM.val9 + dataConstantes[j].val9;
                                                ActualizaMM.val10 = ActualizaMM.val10 + dataConstantes[j].val10;
                                                ActualizaMM.val11 = ActualizaMM.val11 + dataConstantes[j].val11;

                                                db.Entry(ActualizaMM).State = EntityState.Modified;
                                                db.SaveChanges();
                                            }
                                        }
                                    }
                                }

                                #endregion
                            }
                        }

                        #endregion

                        // finaliza Agregar los valores existentes en constantes a los nit correspondientes


                        /// Para Borrar "medios_magneticos_topes" 
                        //#region Eliminar contenido de medios_magneticos_topes
                        //if (Wconce_gmm.Count() > 0)
                        //{
                        //	foreach (string k in Wconce_gmm)
                        //	{
                        //		var con = Convert.ToInt32(k);
                        //		db.medios_magneticos_topes.RemoveRange(db.medios_magneticos_topes.Where(x => x.ano == Wannio_gmm && x.formato == Wforma_gmm && x.concepto == con));
                        //		db.SaveChanges();
                        //	}
                        //}
                        //#endregion
                        /// Para Borrar "medios_magneticos_topes"

                        #region CON TOPE

                        if (opcionTopelocal)
                        {
                            //Declaramos el predicado de tipo And con el parámetro True. Si fuera de tipo Or lo declaramos con el parámetro False
                            System.Linq.Expressions.Expression<Func<vw_medios_magneticos_tope, bool>> predicatedb = PredicateBuilder.True<vw_medios_magneticos_tope>();
                            System.Linq.Expressions.Expression<Func<vw_medios_magneticos_tope, bool>> predicatedb2 = PredicateBuilder.False<vw_medios_magneticos_tope>();
                            System.Linq.Expressions.Expression<Func<vw_medios_magneticos_tope, bool>> predicatedb3 = PredicateBuilder.True<vw_medios_magneticos_tope>();

                            // var predicadoConstantes = PredicateBuilder.True<Vista_ConceptosUnicos_medios_magenticosdb>();
                            if (Wannio_gmm != null)
                            {
                                predicatedb = predicatedb.And(x => x.ano == Wannio_gmm);
                            }
                            //predicadoConcepto = predicadoConcepto.And(x => x.ano == Wannio_gmm);
                            if (Wforma_gmm != null)
                            {
                                predicatedb = predicatedb.And(x => x.formato == Wforma_gmm);
                            }
                            //predicadoConcepto = predicadoConcepto.And(x => x.formato == Wforma_gmm);
                            predicatedb = predicatedb.And(x => x.val1 < x.tope);

                            if (Wconce_gmm.Count() > 0)

                            {
                                foreach (string j in Wconce_gmm)
                                {
                                    int con = Convert.ToInt32(j);
                                    predicatedb2 = predicatedb2.Or(x => x.concepto == con);
                                    predicatedb3 = predicatedb.And(x => x.concepto == con);
                                    List<vw_medios_magneticos_tope> registrosPorConcepto =
                                        db.vw_medios_magneticos_tope.Where(predicatedb3).ToList();
                                    var data03 = registrosPorConcepto.Select(c => new
                                    {
                                        c.id,
                                        c.ano,
                                        c.formato,
                                        c.concepto,
                                        c.nit,
                                        c.val1,
                                        c.val2,
                                        c.val3,
                                        c.val4,
                                        c.val5,
                                        c.val6,
                                        c.val7,
                                        c.val8,
                                        c.val9,
                                        c.val10,
                                        c.val11
                                    }).ToList();

                                    if (data03.Count > 0)
                                    {
                                        for (int i = 0; i < data03.Count; i++)
                                        {
                                            db.medios_magneticos_topes.Add(new medios_magneticos_topes
                                            {
                                                ano = data03[0].ano,
                                                formato = data03[0].formato,
                                                concepto = data03[0].concepto,
                                                nit = Auxnit,
                                                val1 = data03[0].val1,
                                                val2 = data03[0].val2,
                                                val3 = data03[0].val3,
                                                val4 = data03[0].val4,
                                                val5 = data03[0].val5,
                                                val6 = data03[0].val6,
                                                val7 = data03[0].val7,
                                                val8 = data03[0].val8,
                                                val9 = data03[0].val9,
                                                val10 = data03[0].val10,
                                                val11 = data03[0].val11
                                            });
                                            db.SaveChanges();
                                            medios_magneticos_topes buscar2222 = db.medios_magneticos_topes.FirstOrDefault(c =>
                                                c.ano == Wannio_gmm && c.formato == Wforma_gmm && c.concepto == con &&
                                                c.nit == Auxnit);
                                            {
                                                if (buscar2222 != null && i > 0)
                                                {
                                                    buscar2222.val1 = buscar2222.val1 + data03[i].val1;
                                                    buscar2222.val2 = buscar2222.val2 + data03[i].val2;
                                                    buscar2222.val3 = buscar2222.val3 + data03[i].val3;
                                                    buscar2222.val4 = buscar2222.val4 + data03[i].val4;
                                                    buscar2222.val5 = buscar2222.val5 + data03[i].val5;
                                                    buscar2222.val6 = buscar2222.val6 + data03[i].val6;
                                                    buscar2222.val7 = buscar2222.val7 + data03[i].val7;
                                                    buscar2222.val8 = buscar2222.val8 + data03[i].val8;
                                                    buscar2222.val9 = buscar2222.val9 + data03[i].val9;
                                                    buscar2222.val10 = buscar2222.val10 + data03[i].val10;
                                                    buscar2222.val11 = buscar2222.val11 + data03[i].val11;

                                                    db.Entry(buscar2222).State = EntityState.Modified;
                                                    db.SaveChanges();
                                                }
                                            }
                                        }
                                    }
                                }
                                //predicatedb = predicatedb.And(predicatedb2);

                                predicatedb = predicatedb.And(x => x.nit != Auxnit);

                                List<vw_medios_magneticos_tope> regeliminar = db.vw_medios_magneticos_tope.Where(predicatedb).ToList();
                                foreach (vw_medios_magneticos_tope item in regeliminar)
                                {
                                    medios_magneticos_topes regeliminarAux = db.medios_magneticos_topes.Where(x => x.id == item.id)
                                        .FirstOrDefault();
                                    db.Entry(regeliminarAux).State = EntityState.Deleted;
                                    //	var regeliminarAux222 = db.medios_magneticos_topes.Where(x => x.nit== Auxnit && x.val1 == 0 && x.val2 ==0 && x.val3 == 0 && x.val4 == 0 && x.val5==0 && x.val6 == 0 && x.val7 == 0 && x.val8 == 0 && x.val9 == 0 && x.val10 == 0 && x.val11 == 0).FirstOrDefault();
                                    //	db.Entry(regeliminarAux222).State = EntityState.Deleted;
                                }

                                try
                                {
                                    db.SaveChanges();
                                    return Json(result, JsonRequestBehavior.AllowGet);
                                }
                                catch (Exception e)
                                {
                                    string error = e.Message;
                                    return Json(result, JsonRequestBehavior.AllowGet);

                                    //throw;
                                }
                            }
                        } // final con Tope

                        #endregion
                    }
                }

                ///ojo hasta aqui 
            }
            else
            {
                ///esto ocurre si no hay data que venga de la consulta de la vista
                result = false;
                //return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarConstantes(int? xannio, int? xformato, int? xconcepto)
        {
            if (xannio != null && xformato != null && xconcepto != null)
            {
                var data = (from mc in db.medios_constantes
                            join fo in db.medios_formato
                                on mc.formato equals fo.id
                            join co in db.medios_conceptos
                                on mc.concepto equals co.id
                            where mc.ano == xannio && mc.formato == xformato && mc.concepto == xconcepto
                            select new
                            {
                                mc.id,
                                mc.ano,
                                fo.formato,
                                co.concepto,
                                mc.nit,
                                mc.val1,
                                mc.val2,
                                mc.val3,
                                mc.val4,
                                mc.val5,
                                mc.val6,
                                mc.val7,
                                mc.val8,
                                mc.val9,
                                mc.val10,
                                mc.val11
                            }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarRegMedMag2(int? xannio, int? xformato, int? xconcepto, bool? op)
        {
            //var con = 
            if (op != null)
            {
                if (xannio != null && xformato != null && op == false)
                {
                    var data = (from mc in db.medios_magneticosbd
                                join fo in db.medios_formato
                                    on mc.formato equals fo.id
                                join co in db.medios_conceptos
                                    on mc.concepto equals co.id
                                join ter in db.icb_terceros
                                    on mc.nit equals ter.tercero_id
                                where mc.ano == xannio && mc.formato == xformato && mc.concepto == xconcepto
                                select new
                                {
                                    mc.id,
                                    mc.ano,
                                    fo.formato,
                                    co.concepto,
                                    mc.nit,
                                    mc.val1,
                                    mc.val2,
                                    mc.val3,
                                    mc.val4,
                                    mc.val5,
                                    mc.val6,
                                    mc.val7,
                                    mc.val8,
                                    mc.val9,
                                    mc.val10,
                                    mc.val11,
                                    tercero = ter.tpdoc_id != 1
                                        ? "(" + ter.doc_tercero + ")" + ter.prinom_tercero + " " + ter.segnom_tercero + " " +
                                          ter.apellido_tercero + " " + ter.segapellido_tercero
                                        : "(" + ter.doc_tercero + ")" + ter.razon_social
                                }).ToList();

                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var data = (from mc in db.medios_magneticos_topes
                                join fo in db.medios_formato
                                    on mc.formato equals fo.id
                                join co in db.medios_conceptos
                                    on mc.concepto equals co.id
                                join ter in db.icb_terceros
                                    on mc.nit equals ter.tercero_id
                                where mc.ano == xannio && mc.formato == xformato && mc.concepto == xconcepto
                                //&& mc.concepto == conceptox
                                select new
                                {
                                    mc.id,
                                    mc.ano,
                                    fo.formato,
                                    co.concepto,
                                    mc.nit,
                                    mc.val1,
                                    mc.val2,
                                    mc.val3,
                                    mc.val4,
                                    mc.val5,
                                    mc.val6,
                                    mc.val7,
                                    mc.val8,
                                    mc.val9,
                                    mc.val10,
                                    mc.val11,
                                    tercero = ter.tpdoc_id != 1
                                        ? "(" + ter.doc_tercero + ")" + ter.prinom_tercero + " " + ter.segnom_tercero + " " +
                                          ter.apellido_tercero + " " + ter.segapellido_tercero
                                        : "(" + ter.doc_tercero + ")" + ter.razon_social
                                }).ToList();

                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult BuscarRegMedMag(int? xannio, int? xformato, int? xconcepto, bool? op)
        //{
        //	//var con = 
        //	if (op != null)
        //	{
        //		if (xannio != null && xformato != null && op == false)
        //		{
        //			var data = (from mc in db.medios_magneticosbd
        //						join fo in db.medios_formato
        //						 on mc.formato equals fo.id
        //						join co in db.medios_conceptos
        //						on mc.concepto equals co.id
        //						where mc.ano == xannio && mc.formato == xformato
        //						//&& mc.concepto == conceptox
        //						select new
        //						{
        //							mc.id,
        //							mc.ano,
        //							formato = fo.formato,
        //							concepto = co.concepto,
        //							mc.nit,
        //							mc.val1,
        //							mc.val2,
        //							mc.val3,
        //							mc.val4,
        //							mc.val5,
        //							mc.val6,
        //							mc.val7,
        //							mc.val8,
        //							mc.val9,
        //							mc.val10,
        //							mc.val11
        //						}).ToList();

        //			return Json(data, JsonRequestBehavior.AllowGet);
        //		}
        //		else
        //		{
        //			var data = (from mc in db.medios_magneticos_topes
        //						join fo in db.medios_formato
        //						 on mc.formato equals fo.id
        //						join co in db.medios_conceptos
        //						on mc.concepto equals co.id
        //						where mc.ano == xannio && mc.formato == xformato
        //						//&& mc.concepto == conceptox
        //						select new
        //						{
        //							mc.id,
        //							mc.ano,
        //							formato = fo.formato,
        //							concepto = co.concepto,
        //							mc.nit,
        //							mc.val1,
        //							mc.val2,
        //							mc.val3,
        //							mc.val4,
        //							mc.val5,
        //							mc.val6,
        //							mc.val7,
        //							mc.val8,
        //							mc.val9,
        //							mc.val10,
        //							mc.val11
        //						}).ToList();

        //			return Json(data, JsonRequestBehavior.AllowGet);
        //		}
        //	}
        //	else
        //	{
        //		return Json(0, JsonRequestBehavior.AllowGet);
        //	}


        //}
        public JsonResult BuscarDefDB(int? xannio, int? xformato, int? xconcepto)
        {
            if (xannio != null && xformato != null && xconcepto != null)
            {
                var data = (from mg in db.medios_gen
                            join fo in db.medios_formato
                                on mg.formato equals fo.id
                            join co in db.medios_conceptos
                                on mg.concepto equals co.id
                            where mg.ano == xannio && mg.formato == xformato && mg.concepto == xconcepto
                            select new
                            {
                                mg.id,
                                mg.ano,
                                fo.formato,
                                co.concepto,
                                mg.val1,
                                mg.val2,
                                mg.val3,
                                mg.val4,
                                mg.val5,
                                mg.val6,
                                mg.val7,
                                mg.val8,
                                mg.val9,
                                mg.val10,
                                mg.val11
                            }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDefMovCtaDB(int? xidmedios, int? xidvalor)
        {
            if (xidmedios != null && xidvalor != null)
            {
                var data = (from mmc in db.medios_movtos
                            where mmc.idmedios == xidmedios && mmc.idvalor == xidvalor
                            select new
                            {
                                mmc.idmedios,
                                mmc.tipomovimiento,
                                mmc.cuenta,
                                mmc.operacion,
                                mmc.idtipodoc,
                                mmc.idvalor
                            }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDefMovCtaDBxx(int? xidmedios, int? xidvalor, int? xxTipoDato)
        {
            if (xidmedios != null && xidvalor != null && xxTipoDato != null)
            {
                var data = (from medmov in db.medios_movtos
                            join cuenta in db.cuenta_puc
                                on medmov.cuenta equals cuenta.cntpuc_id
                            join tipo in db.tp_doc_registros
                                on medmov.idtipodoc equals tipo.tpdoc_id into aux
                            from tipo in aux.DefaultIfEmpty()
                            where medmov.idmedios == xidmedios && medmov.idvalor == xidvalor && medmov.tipodato == xxTipoDato
                            select new
                            {
                                medmov.id,
                                WtipoCod = medmov.idtipodoc > 0 ? "" + tipo.prefijo : "No Documento",
                                WtipoDes = medmov.idtipodoc > 0 ? "" + tipo.tpdoc_nombre : "No Desc",
                                WiCuenta = cuenta.cntpuc_numero,
                                WdCuenta = cuenta.cntpuc_descp,
                                Woperaci = medmov.operacion,
                                WQue = medmov.tipomovimiento == 1 ? "Debito" :
                                    medmov.tipomovimiento == 2 ? "Credito" :
                                    medmov.tipomovimiento == 3 ? "Saldo" :
                                    medmov.tipomovimiento == 4 ? "Base" : " ",
                                WTipoDato = medmov.tipodato
                            }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
            //var data = (from mmc in db.medios_movtos
            //            where mmc.idmedios == xidmedios && mmc.idvalor == xidvalor
            //            select new
            //            {
            //                mmc.idmedios,
            //                mmc.tipomovimiento,
            //                mmc.cuenta,
            //                mmc.operacion,
            //                mmc.idtipodoc,
            //                mmc.idvalor
            ////            }).ToList();
            //     join tipo in db.tp_doc_registros
            //     on medmov.idtipodoc equals tipo.tpdoc_id

            // 
        }

        public JsonResult BuscarDefDocumentosDB(int? xidmedios, int? xidvalor, int? xxTipoDato)
        {
            if (xidmedios != null && xidvalor != null && xxTipoDato != null)
            {
                var data = (from medDoc in db.medios_movtos
                            join tipo in db.tp_doc_registros
                                on medDoc.idtipodoc equals tipo.tpdoc_id
                            join campo in db.mediostipovalor
                                on medDoc.tipomovimiento equals campo.id
                            where medDoc.idmedios == xidmedios && medDoc.idvalor == xidvalor && medDoc.tipodato == xxTipoDato
                            select new
                            {
                                wid = medDoc.id,
                                wtipo = tipo.prefijo + "-" + tipo.tpdoc_nombre,
                                wcampo = campo.descripcion,
                                woperacion = medDoc.operacion
                                //WtipoCod = (medmov.idtipodoc > 0 ? "Num: " + medmov.idtipodoc : "No Documento"),
                                //WtipoDes = (medmov.idtipodoc > 0 ? "Pref: " + tipo.prefijo + ": " + tipo.tpdoc_nombre : "No Desc"),
                                //WiCuenta = cuenta.cntpuc_numero,
                                //WdCuenta = cuenta.cntpuc_descp,
                                //Woperaci = medmov.operacion,
                                //WQue = (medmov.tipomovimiento == 1 ? "Debito" : (medmov.tipomovimiento == 2 ? "Credito" : (medmov.tipomovimiento == 3 ? "Saldo" : (medmov.tipomovimiento == 4 ? "Base" : " ")))),
                                //WTipoDato = medmov.tipodato
                            }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
            //var data = (from mmc in db.medios_movtos
            //            where mmc.idmedios == xidmedios && mmc.idvalor == xidvalor
            //            select new
            //            {
            //                mmc.idmedios,
            //                mmc.tipomovimiento,
            //                mmc.cuenta,
            //                mmc.operacion,
            //                mmc.idtipodoc,
            //                mmc.idvalor
            ////            }).ToList();
            //     join tipo in db.tp_doc_registros
            //     on medmov.idtipodoc equals tipo.tpdoc_id

            // 
        }

        public JsonResult eliminarVariable(int idDetalle)
        {
            medios_constantes buscarVariable = db.medios_constantes.FirstOrDefault(x => x.id == idDetalle);
            if (buscarVariable != null)
            {
                db.Entry(buscarVariable).State = EntityState.Deleted;
                int eliminar = db.SaveChanges();
                if (eliminar > 0)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarObservacion(int idDetalle)
        {
            var data = (from obs in db.medios_constantes
                        where obs.id == idDetalle
                        select new
                        {
                            obs.id,
                            obs.Detalle
                        }).FirstOrDefault();

            // return Json(data, JsonRequestBehavior.AllowGet);

            if (data != null)
            {
                return Json(new { descripcion = data.Detalle });
            }

            return Json(new { descripcion = data.Detalle });
        }

        public JsonResult BuscarTerVariable(int? idano, int? idfor, int? idcon, int? nit)
        {
            var data = (from me in db.medios_constantes
                        where me.ano == idano && me.formato == idfor && me.concepto == idcon && me.nit == nit
                        select new
                        {
                            me.val1,
                            me.val2,
                            me.val3,
                            me.val4,
                            me.val5,
                            me.val6,
                            me.val7,
                            me.val8,
                            me.val9,
                            me.val10,
                            me.val11
                        }).FirstOrDefault();

            if (data != null)
            {
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult eliminarCabeceraDefinicionBD(int idDetalle)
        {
            medios_gen buscarCabecera = db.medios_gen.FirstOrDefault(x => x.id == idDetalle);
            if (buscarCabecera != null)
            {
                medios_movtos buscarDefMovCtas = db.medios_movtos.FirstOrDefault(m => m.idmedios == idDetalle);
                if (buscarDefMovCtas != null)
                {
                    db.Entry(buscarDefMovCtas).State = EntityState.Deleted;
                    db.SaveChanges();
                }

                db.Entry(buscarCabecera).State = EntityState.Deleted;
                int eliminar = db.SaveChanges();
                if (eliminar > 0)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult eliminarRegistrosMediosMagneticosBD(int? Wannio_gmm, int? Wforma_gmm)
        {
            // public JsonResult eliminarRegistrosMediosMagneticosBD(int? Wannio_gmm, int? Wforma_gmm, string Wconce_gmm)
            System.Linq.Expressions.Expression<Func<vw_eliminar_regmedmag, bool>> predicate = PredicateBuilder.True<vw_eliminar_regmedmag>();
            var ok = true;
            if (Wannio_gmm != null)
            {
                predicate = predicate.And(x => x.ano == Wannio_gmm);
            }

            if (Wforma_gmm != null)
            {
                predicate = predicate.And(x => x.formato == Wforma_gmm);
            }
            //if (!string.IsNullOrWhiteSpace(Wconce_gmm))
            //{
            //    //separar lo que me traigo en una lista
            //    string[] conceptosid = Wconce_gmm.Split(',');
            //    foreach (string j in conceptosid)
            //    {
            //        var con = Convert.ToInt32(j);
            //        predicate = predicate.And(x => x.concepto == con);

            //    }

            //}
            List<vw_eliminar_regmedmag> registroseliminar = db.vw_eliminar_regmedmag.Where(predicate).ToList();
            var data = registroseliminar.Select(d => new
            {
                d.ano,
                d.formato,
                d.concepto
            }).ToList();
            if (data != null)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    int eliminaAnnio = data[i].ano;
                    int eliminaforma = data[i].formato;
                    List<medios_magneticosbd> regeliminar = (from ts in db.medios_magneticosbd
                                                             where ts.ano == eliminaAnnio && ts.formato == eliminaforma
                                                             select ts).ToList();
                    foreach (medios_magneticosbd item in regeliminar)
                    {
                        db.Entry(item).State = EntityState.Deleted;
                    }

                    try
                    {
                        int eliminar = db.SaveChanges();
                        ok = true;
                       // return Json(true, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception e)
                    {
                        string error = e.Message;
                        ok = false;
                       // return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }

                //return Json(false, JsonRequestBehavior.AllowGet);
            }

            return Json(ok, JsonRequestBehavior.AllowGet);
        }

        public JsonResult eliminarRegistrosMediosMagneticosBDTope(int? Wannio_gmm, int? Wforma_gmm)
        {
            // public JsonResult eliminarRegistrosMediosMagneticosBD(int? Wannio_gmm, int? Wforma_gmm, string Wconce_gmm)
            System.Linq.Expressions.Expression<Func<vw_eliminar_medios_magneticos_tope, bool>> predicate = PredicateBuilder.True<vw_eliminar_medios_magneticos_tope>();
            var ok = true;
            if (Wannio_gmm != null)
            {
                predicate = predicate.And(x => x.ano == Wannio_gmm);
            }

            if (Wforma_gmm != null)
            {
                predicate = predicate.And(x => x.formato == Wforma_gmm);
            }
            //if (!string.IsNullOrWhiteSpace(Wconce_gmm))
            //{
            //    //separar lo que me traigo en una lista
            //    string[] conceptosid = Wconce_gmm.Split(',');
            //    foreach (string j in conceptosid)
            //    {
            //        var con = Convert.ToInt32(j);
            //        predicate = predicate.And(x => x.concepto == con);

            //    }

            //}

            List<vw_eliminar_medios_magneticos_tope> registroseliminar = db.vw_eliminar_medios_magneticos_tope.Where(predicate).ToList();

            var data = registroseliminar.Select(d => new
            {
                d.ano,
                d.formato,
                d.concepto
            }).ToList();

            if (data != null)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    for (int i = 0; i < data.Count; i++)
                    {
                        int eliminaAnnio = data[i].ano;
                        int eliminaforma = data[i].formato;

                        List<medios_magneticos_topes> regeliminar = (from ts in db.medios_magneticos_topes
                                                                     where ts.ano == eliminaAnnio && ts.formato == eliminaforma
                                                                     select ts).ToList();

                        foreach (medios_magneticos_topes item in regeliminar)
                        {
                            db.Entry(item).State = EntityState.Deleted;
                        }

                        try
                        {
                            int eliminar = db.SaveChanges();
                            ok = true;
                            //return Json(true, JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception e)
                        {
                            string error = e.Message;
                            ok = false;
                            //return Json(false, JsonRequestBehavior.AllowGet);
                            break;
                            //throw;
                        }
                    }
                    if (ok == true) {
                        dbTran.Commit();
                    }
                    else
                    {
                        dbTran.Rollback();
                    }
                }                
                //return Json(false, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ok = false;
            }

            return Json(ok, JsonRequestBehavior.AllowGet);
        }

        public JsonResult eliminarRegistrosTope22(int? Wannio_gmm, int? Wforma_gmm, string WWconce_gmm, int Auxiliar)
        {
            //if (!string.IsNullOrWhiteSpace(WWconce_gmm))
            //{
            //	//separar lo que me traigo en una lista
            //	string[] conceptosid = WWconce_gmm.Split(',');

            //	foreach (string j in conceptosid)
            //	{
            //	var con = Convert.ToInt32(j);
            List<medios_magneticos_topes> regeliminar = (from ts in db.medios_magneticos_topes
                                                         where ts.ano == Wannio_gmm && ts.formato == Wforma_gmm /*&& ts.concepto == con*/ && ts.nit == Auxiliar
                                                               && ts.val1 == 0 && ts.val2 == 0 && ts.val3 == 0 && ts.val4 == 0 && ts.val5 == 0 && ts.val6 == 0
                                                               && ts.val7 == 0 && ts.val8 == 0 && ts.val9 == 0 && ts.val10 == 0 && ts.val11 == 0
                                                         select ts).ToList();
            foreach (medios_magneticos_topes item in regeliminar)
            {
                db.Entry(item).State = EntityState.Deleted;
            }

            try
            {
                int eliminar = db.SaveChanges();
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                string error = e.Message;
                return Json(false, JsonRequestBehavior.AllowGet);

                //throw;
            }

            //	}

            //}

            //	return Json(false, JsonRequestBehavior.AllowGet);
        }


        public JsonResult eliminarDefinicionBD(int idDetalle)
        {
            medios_movtos buscarDefMovCtas = db.medios_movtos.FirstOrDefault(m => m.id == idDetalle);
            if (buscarDefMovCtas != null)
            {
                db.Entry(buscarDefMovCtas).State = EntityState.Deleted;
                int eliminar = db.SaveChanges();
                if (eliminar > 0)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult eliminarDefDocumentoBD(int idDetalle)
        {
            medios_movtos buscarDefDocumento = db.medios_movtos.FirstOrDefault(m => m.id == idDetalle);
            if (buscarDefDocumento != null)
            {
                db.Entry(buscarDefDocumento).State = EntityState.Deleted;
                int eliminar = db.SaveChanges();
                if (eliminar > 0)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarFormatoGeneExcel(int wannio)
        {
            var data = from med in db.vw_medios_magneticos_formatos
                       where med.ano == wannio
                       select new
                       {
                           id = med.formato,
                           nombre = med.Des
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarConceptoGeneExcel(int? wannio, int? wformato)
        {
            System.Linq.Expressions.Expression<Func<vw_medios_magneticos_conceptos, bool>> predicate = PredicateBuilder.True<vw_medios_magneticos_conceptos>();
            // var predicate2 = PredicateBuilder.False<vw_medios_magneticos_conceptos>(); esto funcionaria si se requiere que reciba el venctor de wformato string[] wformato

            if (wannio != null)
            {
                predicate = predicate.And(x => x.ano == wannio);
            }

            if (wformato != null)
            {
                predicate = predicate.And(x => x.formato == wformato);
            }

            List<vw_medios_magneticos_conceptos> registrosConceptos = db.vw_medios_magneticos_conceptos.Where(predicate).ToList();

            var data = registrosConceptos.Select(d => new
            {
                d.id,
                nombre = d.concepto
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarFormato()
        {
            var data = from f in db.medios_formato
                       where f.estado
                       select new
                       {
                           f.id,
                           nombre = f.formato
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarFormato2()
        {
            var data = from f in db.medios_formato
                       where f.estado
                       select new
                       {
                           f.id,
                           nombre = f.formato
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarConceptos(int? idFormato)
        {
            var data = from c in db.medios_conceptos
                       where c.idformato == idFormato
                             && c.estado
                       select new
                       {
                           c.id,
                           nombre = c.concepto
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarConceptos2(int? idFormato)
        {
            var data = from c in db.medios_conceptos
                       where c.idformato == idFormato
                             && c.estado
                       select new
                       {
                           c.id,
                           nombre = c.concepto
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}

#region para borra 2

//var con = Convert.ToInt32(Wconce_gmm);

//var data = (from c in db.conceptosFiltradosGroup
//            where c.ano == Wannio_gmm && c.formato == Wforma_gmm && c.concepto == con
//            select new
//            {
//                c.ano,
//                c.formato,
//                c.concepto,
//                c.nit,
//                c.tipomovimiento,
//                c.SubDebito,
//                c.SumaCredito,
//                c.SumaBase,
//                c.idvalor,
//                c.operacion,
//                c.row1
//            }).ToList();

#endregion

#region Borrar esto 

//if (xannio != null && xformato != null && !string.IsNullOrWhiteSpace(xconcepto))
//{

//var conceptox = Convert.ToInt32(xconcepto);


////Declaramos el predicado de tipo And con el parámetro True. Si fuera de tipo Or lo declaramos con el parámetro False
//var predicate = PredicateBuilder.True<vV_medios_magneticosdb>();

//if (xannio != null)
//{
//    predicate = predicate.And(x => x.ano == xannio);
//}
//if (xformato != null)
//{
//    predicate = predicate.And(x => x.formato == xformato);
//}

//if (!string.IsNullOrWhiteSpace(xconcepto))
//{
//    //separar lo que me traigo en una lista
//    string[] conceptosid = xconcepto.Split(',');
//    foreach (string j in conceptosid)
//    {
//        var con = Convert.ToInt32(j);
//        predicate = predicate.And(x => x.concepto == con);
//    }

//}

//var RegMedMag = db.vV_medios_magneticosdb.Where(predicate).ToList();

//var data = RegMedMag.Select(d => new
//{
//    d.id,
//    d.ano,
//    d.formato,
//    d.concepto,
//    d.nit,
//    d.val1,
//    d.val2,
//    d.val3,
//    d.val4,
//    d.val5,
//    d.val6,
//    d.val7,
//    d.val8,
//    d.val9,
//    d.val10,
//    d.val11
//}).ToList();

#endregion

#region excel borrar

//public ActionResult MediosExcel(int excelanio, string[] excelformato, string[] excelconcepto)
//{
//    var localWanio = excelanio;
//    var localWform = excelformato;
//    var localWconc = excelconcepto;


//    if (excelconcepto.Count() > 0)

//    {
//        //separar lo que me traigo en una lista
//        //string[] conceptosid = Wconce_gmm.Split(',');
//        foreach (string j in excelconcepto)
//        {
//            var localWformato = Convert.ToInt32(j);
//            #region Formato 1001
//            if (localWformato == 8)
//            {
//                var nomArchivo = "Dian " + (DateTime.Now).Year + (DateTime.Now).Month + (DateTime.Now).Minute;
//                var EnvioExcel = db.vw_medios_magneticos_excel.OrderBy(d => d.ano).ThenByDescending(d => d.ano).Select(d => new
//                {
//                    d.NomCon,
//                    d.TipoDocumento,
//                    d.doc_tercero,
//                    d.digito_verificacion,
//                    d.apellido_tercero,
//                    d.segapellido_tercero,
//                    d.prinom_tercero,
//                    d.segnom_tercero,
//                    d.razon_social,
//                    d.direccion,
//                    d.cod_dpto,
//                    d.cod_ciudad,
//                    d.cod_pais,
//                    d.val1,
//                    d.val2,
//                    d.val3,
//                    d.val4,
//                    d.val5,
//                    d.val6,
//                    d.val7,
//                    d.val8,
//                    d.val9,
//                    d.val10,
//                    d.val11
//                }).ToArray();
//                ExcelPackage excel = new ExcelPackage();
//                var workSheet = excel.Workbook.Worksheets.Add("1001");

//                workSheet.Cells[1, 1].Value = "Reporte - Dian Medios Magnetico";
//                workSheet.Cells[2, 1].Value = nomArchivo;

//                workSheet.Column(1).Width = 10;
//                workSheet.Column(2).Width = 12;
//                workSheet.Column(3).Width = 14;
//                workSheet.Column(4).Width = 4;
//                workSheet.Column(5).Width = 10;
//                workSheet.Column(6).Width = 10;
//                workSheet.Column(7).Width = 10;
//                workSheet.Column(8).Width = 10;
//                workSheet.Column(9).Width = 16;
//                workSheet.Column(10).Width = 20;
//                workSheet.Column(11).Width = 7;
//                workSheet.Column(12).Width = 10;
//                workSheet.Column(13).Width = 12;
//                workSheet.Column(14).Width = 12;
//                workSheet.Column(15).Width = 12;
//                workSheet.Column(16).Width = 12;
//                workSheet.Column(17).Width = 12;
//                workSheet.Column(18).Width = 10;
//                workSheet.Column(19).Width = 10;
//                workSheet.Column(20).Width = 13;
//                workSheet.Column(21).Width = 12;
//                workSheet.Column(22).Width = 14;
//                workSheet.Column(23).Width = 11;
//                workSheet.Column(24).Width = 11;

//                workSheet.Row(3).Height = 80;

//                workSheet.Cells["A3:X3"].Style.Font.Bold = true;
//                workSheet.Cells["A3:X3"].Style.Font.Size = 10;
//                workSheet.Cells["A3:X3"].Style.Font.Color.SetColor(System.Drawing.Color.Black);
//                workSheet.Cells["A3:X3"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
//                workSheet.Cells["A3:X3"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
//                workSheet.Cells["A3:X3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
//                workSheet.Cells["A3:X3"].Style.WrapText = true;

//                workSheet.Cells[3, 1].Value = "Concepto";
//                workSheet.Cells[3, 2].Value = "Tipo de Documentos";
//                workSheet.Cells[3, 3].Value = "Numero de identificación del informado";
//                workSheet.Cells[3, 4].Value = "DV";
//                workSheet.Cells[3, 5].Value = "Primer apellido";
//                workSheet.Cells[3, 6].Value = "Segundo Apellido";
//                workSheet.Cells[3, 7].Value = "Primer Nombre";
//                workSheet.Cells[3, 8].Value = "Otros Nombres";
//                workSheet.Cells[3, 9].Value = "Razón Social";
//                workSheet.Cells[3, 10].Value = "Dirección";
//                workSheet.Cells[3, 11].Value = "Cod. Dpto.";
//                workSheet.Cells[3, 12].Value = "Cod. Mcipio.";
//                workSheet.Cells[3, 13].Value = "País de residencia o domicilio";
//                workSheet.Cells[3, 14].Value = "Pago o abono en cuenta deducible";
//                workSheet.Cells[3, 15].Value = "Pago o abono en cuenta NO deducible";
//                workSheet.Cells[3, 16].Value = "IVA mayor valor del costo o gasto deducible";
//                workSheet.Cells[3, 17].Value = "IVA mayor valor del costo o gasto NO deducible";
//                workSheet.Cells[3, 18].Value = "Retención en la fuente practicada Renta";
//                workSheet.Cells[3, 19].Value = "Retención en la fuente asumida Renta";
//                workSheet.Cells[3, 20].Value = "Retención en la fuente practicada IVA régimen común";
//                workSheet.Cells[3, 21].Value = "Retención en la fuente asumida IVA régimen Simplificado";
//                workSheet.Cells[3, 22].Value = "Retención en la fuente practicada IVA no domiciliados";
//                workSheet.Cells[3, 23].Value = "Retención en la fuente practicada por CREE";
//                workSheet.Cells[3, 24].Value = "Retención en la fuente asumida por CREE";

//                workSheet.Cells[4, 1].LoadFromCollection(EnvioExcel, false);
//                using (var memoryStream = new MemoryStream())
//                {
//                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
//                    Response.AddHeader("content-disposition", "attachment;  filename=" + nomArchivo + ".xlsx");
//                    excel.SaveAs(memoryStream);
//                    memoryStream.WriteTo(Response.OutputStream);
//                    Response.Flush();
//                    Response.End();
//                }
//            }
//            #endregion

//            #region Formato 1003
//            if (localWformato == 28)
//            {
//                var nomArchivo = "Dian " + (DateTime.Now).Year + (DateTime.Now).Month + (DateTime.Now).Minute;
//                var EnvioExcel = db.vw_medios_magneticos_excel.OrderBy(d => d.ano).ThenByDescending(d => d.ano).Where(e => e.ano == localWanio).Select(d => new
//                {
//                    d.NomCon,
//                    d.TipoDocumento,
//                    d.doc_tercero,
//                    d.digito_verificacion,
//                    d.apellido_tercero,
//                    d.segapellido_tercero,
//                    d.prinom_tercero,
//                    d.segnom_tercero,
//                    d.razon_social,
//                    d.direccion,
//                    d.cod_dpto,
//                    d.cod_ciudad,
//                    // d.cod_pais,
//                    d.val1,
//                    d.val2,
//                    d.val3,
//                    d.val4,
//                    d.val5,
//                    d.val6,
//                    d.val7,
//                    d.val8,
//                    d.val9,
//                    d.val10,
//                    d.val11
//                }).ToArray();
//                ExcelPackage excel = new ExcelPackage();
//                var workSheet = excel.Workbook.Worksheets.Add("1003");

//                workSheet.Cells[1, 1].Value = "Reporte - Dian Medios Magnetico";
//                workSheet.Cells[2, 1].Value = nomArchivo;

//                workSheet.Column(1).Width = 10;
//                workSheet.Column(2).Width = 12;
//                workSheet.Column(3).Width = 14;
//                workSheet.Column(4).Width = 4;
//                workSheet.Column(5).Width = 10;
//                workSheet.Column(6).Width = 10;
//                workSheet.Column(7).Width = 10;
//                workSheet.Column(8).Width = 10;
//                workSheet.Column(9).Width = 16;
//                workSheet.Column(10).Width = 20;
//                workSheet.Column(11).Width = 7;
//                workSheet.Column(12).Width = 10;
//                //workSheet.Column(13).Width = 12;
//                workSheet.Column(14).Width = 12;
//                workSheet.Column(15).Width = 12;
//                workSheet.Column(16).Width = 12;
//                workSheet.Column(17).Width = 12;
//                workSheet.Column(18).Width = 10;
//                workSheet.Column(19).Width = 10;
//                workSheet.Column(20).Width = 13;
//                workSheet.Column(21).Width = 12;
//                workSheet.Column(22).Width = 14;
//                workSheet.Column(23).Width = 11;
//                workSheet.Column(24).Width = 11;

//                workSheet.Row(3).Height = 80;

//                workSheet.Cells["A3:X3"].Style.Font.Bold = true;
//                workSheet.Cells["A3:X3"].Style.Font.Size = 10;
//                workSheet.Cells["A3:X3"].Style.Font.Color.SetColor(System.Drawing.Color.Black);
//                workSheet.Cells["A3:X3"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
//                workSheet.Cells["A3:X3"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
//                workSheet.Cells["A3:X3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
//                workSheet.Cells["A3:X3"].Style.WrapText = true;

//                workSheet.Cells[3, 1].Value = "Concepto";
//                workSheet.Cells[3, 2].Value = "Tipo de Documentos";
//                workSheet.Cells[3, 3].Value = "Numero de identificación del informado";
//                workSheet.Cells[3, 4].Value = "DV";
//                workSheet.Cells[3, 5].Value = "Primer apellido";
//                workSheet.Cells[3, 6].Value = "Segundo Apellido";
//                workSheet.Cells[3, 7].Value = "Primer Nombre";
//                workSheet.Cells[3, 8].Value = "Otros Nombres";
//                workSheet.Cells[3, 9].Value = "Razón Social";
//                workSheet.Cells[3, 10].Value = "Dirección";
//                workSheet.Cells[3, 11].Value = "Cod. Dpto.";
//                workSheet.Cells[3, 12].Value = "Cod. Mcipio.";
//                //  workSheet.Cells[3, 13].Value = "País de residencia o domicilio";
//                workSheet.Cells[3, 14].Value = "Pago o abono en cuenta deducible";
//                workSheet.Cells[3, 15].Value = "Pago o abono en cuenta NO deducible";
//                workSheet.Cells[3, 16].Value = "IVA mayor valor del costo o gasto deducible";
//                workSheet.Cells[3, 17].Value = "IVA mayor valor del costo o gasto NO deducible";
//                workSheet.Cells[3, 18].Value = "Retención en la fuente practicada Renta";
//                workSheet.Cells[3, 19].Value = "Retención en la fuente asumida Renta";
//                workSheet.Cells[3, 20].Value = "Retención en la fuente practicada IVA régimen común";
//                workSheet.Cells[3, 21].Value = "Retención en la fuente asumida IVA régimen Simplificado";
//                workSheet.Cells[3, 22].Value = "Retención en la fuente practicada IVA no domiciliados";
//                workSheet.Cells[3, 23].Value = "Retención en la fuente practicada por CREE";
//                workSheet.Cells[3, 24].Value = "Retención en la fuente asumida por CREE";

//                workSheet.Cells[4, 1].LoadFromCollection(EnvioExcel, false);
//                using (var memoryStream = new MemoryStream())
//                {
//                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
//                    Response.AddHeader("content-disposition", "attachment;  filename=" + nomArchivo + ".xlsx");
//                    excel.SaveAs(memoryStream);
//                    memoryStream.WriteTo(Response.OutputStream);
//                    Response.Flush();
//                    Response.End();
//                }
//            }
//            #endregion

//        }

//    }


//    return View();
//}

#endregion

#region para borrar

//        public JsonResult Buscar001(int? Wannio_gmm, int? Wforma_gmm, int? Wconce_gmm)
//{
//    if (Wannio_gmm != null)
//    {
//        var data01 = (from Wmg in db.medios_gen
//                      join Wmm in db.medios_movtos
//                      on Wmg.id equals Wmm.idmedios
//                      join Wcp in db.cuenta_puc
//                      on Wmm.cuenta equals Wcp.cntpuc_id into ps
//                      from Wcp in ps.DefaultIfEmpty()
//                      where Wmg.ano == Wannio_gmm && Wmg.formato == Wforma_gmm && Wmg.concepto == Wconce_gmm && Wmm.cuenta > 0
//                      select new
//                      {
//                          Wmg.ano,
//                          Wmg.formato,
//                          Wmg.concepto,
//                          Wmg.val1,
//                          Wmg.val2,
//                          Wmg.val3,
//                          Wmg.val4,
//                          Wmg.val5,
//                          Wmg.val6,
//                          Wmg.val7,
//                          Wmg.val8,
//                          Wmg.val9,
//                          Wmg.val10,
//                          Wmg.val11,
//                          Wmm.id,
//                          Wmm.idmedios,
//                          tm = (Wmm.tipomovimiento == 1 ? "Debito" : (Wmm.tipomovimiento == 2 ? "Credito" : (Wmm.tipomovimiento == 3 ? "Saldo" : (Wmm.tipomovimiento == 4 ? "Base" : " ")))),
//                          tmnum = Wmm.tipomovimiento,
//                          cuentamovtos = Wmm.cuenta,
//                          Wmm.operacion,
//                          // Wmm.idtipodoc,
//                          Wmm.idvalor,
//                          Wmm.tipodato,
//                          Wcp.cntpuc_id,
//                          Wcp.cntpuc_numero,
//                          Wcp.cntpuc_descp,
//                          Wcp.clase,
//                          Wcp.grupo,
//                          cuentapuc = Wcp.cuenta,
//                          Wcp.subcuenta
//                      }).ToList();

//        return Json(data01, JsonRequestBehavior.AllowGet);
//    }
//    else
//    {
//        return Json(0, JsonRequestBehavior.AllowGet);
//    }

//}

//public JsonResult BuscarArreglo001(int? annio, int? forma, int? conce, string numclase)
//{
//    var x = numclase;
//    if (annio != null)
//    {
//        //on Wcp.cntpuc_id equals Wmc.cuenta into ps

//        //from Wmc in ps.DefaultIfEmpty()
//        var dataArr01 = (from Wmg in db.medios_gen
//                         join Wmm in db.medios_movtos
//                         on Wmg.id equals Wmm.idmedios
//                         join Wcp in db.cuenta_puc
//                         on Wmm.cuenta equals Wcp.cntpuc_id    /// (b.cuenta = c.cntpuc_id)  (b.cuenta = c.grupo) or (b.cuenta = c.cuenta) or (b.cuenta = c.subcuenta)
//                         //join Wcp2 in db.cuenta_puc
//                         //on Wmm.cuenta equals Wcp2.clase       /// (b.cuenta = c.clase)
//                         join Wmc in db.mov_contable
//                         on Wcp.cntpuc_id equals Wmc.cuenta
//                         where Wcp.clase == numclase && Wmg.ano == annio && Wmg.formato == forma && Wmg.concepto == conce && Wmm.cuenta > 0
//                         select new
//                         {
//                             Wcp.cntpuc_id,
//                             Wcp.cntpuc_numero,
//                             Wcp.cntpuc_descp,
//                             Wcp.clase,
//                             //// Wcp.grupo,
//                             //// cuentapuc = Wcp.cuenta,
//                             //// Wcp.subcuenta,
//                             Wmc.cuenta,
//                             Wmc.nit,
//                             Wmc.fec,
//                             Wmc.debito,
//                             Wmc.credito,
//                             Wmc.basecontable
//                         }).ToList();

//        return Json(dataArr01, JsonRequestBehavior.AllowGet);
//    }
//    else
//    {
//        return Json(0, JsonRequestBehavior.AllowGet);
//    }

//}

//public JsonResult BuscarArreglo002(int? annio, int? forma, int? conce, string numgrupo)
//{
//    var x = numgrupo;
//    if (annio != null)
//    {
//        var dataArr02 = (from Wcp in db.cuenta_puc
//                         join Wmc in db.mov_contable
//                          on Wcp.cntpuc_id equals Wmc.cuenta into ps
//                         from Wmc in ps.DefaultIfEmpty()
//                         where Wcp.grupo == numgrupo
//                         select new
//                         {
//                             Wcp.cntpuc_id,
//                             Wcp.cntpuc_numero,
//                             Wcp.cntpuc_descp,
//                             Wcp.clase,
//                             Wcp.grupo,
//                             // cuentapuc = Wcp.cuenta,
//                             // Wcp.subcuenta,
//                             cuentamovcontable = Wmc.cuenta,
//                             Wmc.nit,
//                             Wmc.fec,
//                             Wmc.debito,
//                             Wmc.credito,
//                             Wmc.basecontable
//                         }).ToList();

//        return Json(dataArr02, JsonRequestBehavior.AllowGet);
//    }
//    else
//    {
//        return Json(0, JsonRequestBehavior.AllowGet);
//    }

//}

//public JsonResult BuscarArreglo035(int? annio, int? forma, int? conce, string numcuenta)
//{
//    var x = numcuenta;
//    if (annio != null)
//    {
//        var dataArr35 = (from Wcp in db.cuenta_puc
//                         join Wmc in db.mov_contable
//                          on Wcp.cntpuc_id equals Wmc.cuenta into ps
//                         from Wmc in ps.DefaultIfEmpty()
//                         where Wcp.cuenta == numcuenta
//                         select new
//                         {
//                             Wcp.cntpuc_id,
//                             Wcp.cntpuc_numero,
//                             Wcp.cntpuc_descp,
//                             Wcp.clase,
//                             Wcp.grupo,
//                             cuentapuc = Wcp.cuenta,
//                             //  Wcp.subcuenta,
//                             cuentamovcontable = Wmc.cuenta,
//                             Wmc.nit,
//                             Wmc.fec,
//                             Wmc.debito,
//                             Wmc.credito,
//                             Wmc.basecontable
//                         }).ToList();

//        return Json(dataArr35, JsonRequestBehavior.AllowGet);
//    }
//    else
//    {
//        return Json(0, JsonRequestBehavior.AllowGet);
//    }

//}

//public JsonResult BuscarArreglo006(int? annio, int? forma, int? conce, string numsubcuenta)
//{
//    var x = numsubcuenta;
//    if (annio != null)
//    {
//        var dataArr06 = (from Wcp in db.cuenta_puc
//                         join Wmc in db.mov_contable
//                          on Wcp.cntpuc_id equals Wmc.cuenta into ps
//                         from Wmc in ps.DefaultIfEmpty()
//                         where Wcp.subcuenta == numsubcuenta
//                         select new
//                         {
//                             Wcp.cntpuc_id,
//                             Wcp.cntpuc_numero,
//                             Wcp.cntpuc_descp,
//                             Wcp.clase,
//                             Wcp.grupo,
//                             cuentapuc = Wcp.cuenta,
//                             Wcp.subcuenta,
//                             cuentamovcontable = Wmc.cuenta,
//                             Wmc.nit,
//                             Wmc.fec,
//                             Wmc.debito,
//                             Wmc.credito,
//                             Wmc.basecontable
//                         }).ToList();

//        return Json(dataArr06, JsonRequestBehavior.AllowGet);
//    }
//    else
//    {
//        return Json(0, JsonRequestBehavior.AllowGet);
//    }

//}

#endregion


#region Archivo Excel 1003

//var nomArchivo = "Dian " + (DateTime.Now).Year + (DateTime.Now).Month + (DateTime.Now).Minute + "_1003";
//var envioExcel = db.vw_medios_magneticos_excel.Where(predicate).ToList();
//var data = envioExcel.Select(d => new
//{
//	d.NomCon,
//	d.TipoDocumento,
//	d.doc_tercero,
//	d.digito_verificacion,
//	d.apellido_tercero,
//	d.segapellido_tercero,
//	d.prinom_tercero,
//	d.segnom_tercero,
//	d.razon_social,
//	d.direccion,
//	d.cod_dpto,
//	d.cod_ciudad,
//	// d.cod_pais,
//	d.val1,
//	d.val2,
//	// d.val3,
//	// d.val4,
//	// d.val5,
//	// d.val6,
//	// d.val7,
//	// d.val8,
//	// d.val9,
//	// d.val10,
//	// d.val11
//}).ToArray();

//ExcelPackage excel = new ExcelPackage();
//var workSheet = excel.Workbook.Worksheets.Add("Formato1003");

//workSheet.Cells[1, 1].Value = "Reporte - Dian Medios Magnetico";
//                        workSheet.Cells[2, 1].Value = nomArchivo;

//                        workSheet.Column(1).Width = 10;
//                        workSheet.Column(2).Width = 12;
//                        workSheet.Column(3).Width = 14;
//                        workSheet.Column(4).Width = 4;
//                        workSheet.Column(5).Width = 10;
//                        workSheet.Column(6).Width = 10;
//                        workSheet.Column(7).Width = 10;
//                        workSheet.Column(8).Width = 10;
//                        workSheet.Column(9).Width = 16;
//                        workSheet.Column(10).Width = 20;
//                        workSheet.Column(11).Width = 7;
//                        workSheet.Column(12).Width = 10;
//                       // workSheet.Column(13).Width = 12;
//                        workSheet.Column(14).Width = 12;
//                        workSheet.Column(15).Width = 12;
//                    //    workSheet.Column(16).Width = 12;
//                     //   workSheet.Column(17).Width = 12;
//                     //   workSheet.Column(18).Width = 10;
//                     //   workSheet.Column(19).Width = 10;
//                     //   workSheet.Column(20).Width = 13;
//                      //  workSheet.Column(21).Width = 12;
//                     //   workSheet.Column(22).Width = 14;
//                     //   workSheet.Column(23).Width = 11;
//                     //   workSheet.Column(24).Width = 11;

//                        workSheet.Row(3).Height = 80;

//                        workSheet.Cells["A3:X3"].Style.Font.Bold = true;
//                        workSheet.Cells["A3:X3"].Style.Font.Size = 10;
//                        workSheet.Cells["A3:X3"].Style.Font.Color.SetColor(System.Drawing.Color.Black);
//                        workSheet.Cells["A3:X3"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
//                        workSheet.Cells["A3:X3"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
//                        workSheet.Cells["A3:X3"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
//                        workSheet.Cells["A3:X3"].Style.WrapText = true;

//                        workSheet.Cells[3, 1].Value = "Concepto";
//                        workSheet.Cells[3, 2].Value = "Tipo de Documentos";
//                        workSheet.Cells[3, 3].Value = "Numero de identificación del informado";
//                        workSheet.Cells[3, 4].Value = "DV";
//                        workSheet.Cells[3, 5].Value = "Primer apellido";
//                        workSheet.Cells[3, 6].Value = "Segundo Apellido";
//                        workSheet.Cells[3, 7].Value = "Primer Nombre";
//                        workSheet.Cells[3, 8].Value = "Otros Nombres";
//                        workSheet.Cells[3, 9].Value = "Razón Social";
//                        workSheet.Cells[3, 10].Value = "Dirección";
//                        workSheet.Cells[3, 11].Value = "Cod. Dpto.";
//                        workSheet.Cells[3, 12].Value = "Cod. Mcipio.";
//                //        workSheet.Cells[3, 13].Value = "País de residencia o domicilio";
//                        workSheet.Cells[3, 14].Value = "Valor acumulado del pago o abono sujeto a retención";
//                        workSheet.Cells[3, 15].Value = "Valor de la retención a título de renta, CREE,  o de IVA que le practicaron";
//                //        workSheet.Cells[3, 16].Value = "IVA mayor valor del costo o gasto deducible";
//                //        workSheet.Cells[3, 17].Value = "IVA mayor valor del costo o gasto NO deducible";
//                //        workSheet.Cells[3, 18].Value = "Retención en la fuente practicada Renta";
//                //        workSheet.Cells[3, 19].Value = "Retención en la fuente asumida Renta";
//                //        workSheet.Cells[3, 20].Value = "Retención en la fuente practicada IVA régimen común";
//                //        workSheet.Cells[3, 21].Value = "Retención en la fuente asumida IVA régimen Simplificado";
//                //        workSheet.Cells[3, 22].Value = "Retención en la fuente practicada IVA no domiciliados";
//                 //       workSheet.Cells[3, 23].Value = "Retención en la fuente practicada por CREE";
//                  //      workSheet.Cells[3, 24].Value = "Retención en la fuente asumida por CREE";

//                        workSheet.Cells[4, 1].LoadFromCollection(data, false);
//                        using (var memoryStream = new MemoryStream())
//                        {
//                            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
//                            Response.AddHeader("content-disposition", "attachment;  filename=" + nomArchivo + ".xlsx");
//                            excel.SaveAs(memoryStream);
//                            memoryStream.WriteTo(Response.OutputStream);
//                            Response.Flush();
//                            Response.End();
//                        }

#endregion