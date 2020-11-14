using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class SignosVitalesController : Controller
    {
        // GET: SignosVitales
        private readonly Iceberg_Context db = new Iceberg_Context();

        public List<listaGeneral> sistemas(int idot)
        {
            List<listaGeneral> sistemasV = (from v in db.vw_operaciones_vh
                                            where v.tsis_id != null && v.ot_id == idot
                                            group v by new { v.tsis_id, v.tsis_sistema, v.tsis_estadovh }
                into vg
                                            select new listaGeneral
                                            {
                                                id = vg.Key.tsis_id.Value,
                                                descripcion = vg.Key.tsis_sistema,
                                                codigo = vg.Key.tsis_estadovh.Trim()
                                            }).ToList();
            return sistemasV;
        }

        public ActionResult signos(int idot, int autorz)
        {
            //
            icb_sysparameter ejecu = db.icb_sysparameter.Where(d => d.syspar_cod == "P79").FirstOrDefault();
            int inspeccion = ejecu != null ? Convert.ToInt32(ejecu.syspar_value) : 14;
            //ver ot existente
            tencabezaorden ot = db.tencabezaorden.Where(d => d.id == idot).FirstOrDefault();
             if (ot != null)
            {
                if (ot.fecha_inicio_inspeccion == null)
                {
                    ot.fecha_inicio_inspeccion = DateTime.Now;
                    db.Entry(ot).State = EntityState.Modified;
                    if (ot.idcita != null)
                    {
                        //busco la cita
                        tcitastaller cita = db.tcitastaller.Where(d => d.id == ot.idcita.Value).FirstOrDefault();
                        cita.estadocita = inspeccion;
                        db.Entry(cita).State = EntityState.Modified;
                    }

                    db.SaveChanges();
                }
            }

            ViewBag.sistemasV = sistemas(idot);
            ViewBag.otid = idot;
            ViewBag.autorz = autorz;
            return PartialView("InspeccionSistemas");
        }

        public int tecnicoOt(int idot)
        {
            int idtecnico = db.tencabezaorden.Where(x => x.id == idot).Select(x => x.idtecnico.Value).FirstOrDefault();
            int idtec = db.ttecnicos.Where(x => x.idusuario == idtecnico).Select(x => x.id).FirstOrDefault();
            return idtec;
        }

        /// <summary>0
        ///     Función para insertar la operación que proviene del formulario signos vitales
        /// </summary>
        /// <param name="oprinsp"></param>
        /// <param name="estado_opr"></param>
        /// <param name="autorz"></param>
        public void inst_operacion_insp(vw_operaciones_vh oprinsp, string estado_opr, bool autorz)
        {
            icb_tinspeccionsistemasvh inspsis = new icb_tinspeccionsistemasvh
            {
                ot_id = oprinsp.ot_id,
                tsope_id = oprinsp.tsope_id,
                tinsvh_estado = estado_opr,
                tinsvh_autorizar = autorz
            };
            db.icb_tinspeccionsistemasvh.Add(inspsis);
            db.SaveChanges();
        }

        public void inst_manodeobra(vw_operaciones_vh opr, int idtec)
        {
            tdetallemanoobraot oprOt = db.tdetallemanoobraot.FirstOrDefault(x => x.idorden == opr.ot_id && x.idtempario == opr.codigo);
            if (oprOt == null)
            {
                //busco la informacion específica del tempario
                ttempario operacion = db.ttempario.Where(d => d.codigo == opr.codigo).FirstOrDefault();

                tdetallemanoobraot operacionOt = new tdetallemanoobraot
                {
                    costopromedio = 0,
                    fecha = DateTime.Now,
                    idorden = opr.ot_id,
                    idtempario = opr.codigo,
                    tiempo = Convert.ToDecimal(opr.tiempo),
                    idtecnico = idtec,
                    valorunitario = opr.precio_tempario != null ? opr.precio_tempario.Value : 0,
                    pordescuento = 0,
                    poriva = operacion.iva,
                    estado = "1"
                };
                db.tdetallemanoobraot.Add(operacionOt);
                if (!string.IsNullOrWhiteSpace(opr.referencia))
                {
                    icb_referencia referencia = db.icb_referencia.Where(d => d.ref_codigo == opr.referencia).FirstOrDefault();
                    tencabezaorden orden = db.tencabezaorden.Where(d => d.id == opr.ot_id).FirstOrDefault();
                    tdetallerepuestosot referenciaOT = new tdetallerepuestosot
                    {
                        costopromedio = 0,
                        idorden = opr.ot_id,
                        idrepuesto = opr.referencia,
                        valorunitario = opr.precio_repuesto != null ? opr.precio_repuesto.Value : 0,
                        pordescto = referencia != null ? Convert.ToDecimal(referencia.por_dscto) : 0,
                        poriva = referencia != null ? Convert.ToDecimal(referencia.por_iva) : 0,
                        cantidad = opr.cant_sistema,
                        idtercero = orden.tercero,
                        solicitado = false
                    };
                    db.tdetallerepuestosot.Add(referenciaOT);
                }

                db.SaveChanges();
            }
        }

        /// <summary>
        ///     Permite adicionar operaciones a la lista de operaciones de la OT, si el parámetro autorz es true. Si es false, no
        ///     la envía pero la marca como opcionada (para enviar mensajes de texto)
        /// </summary>
        /// <param name="oprinsp"></param>
        /// <param name="estado_opr"></param>
        /// <param name="autorz"></param>
        /// <param name="idtec"></param>
        public void inst_operaciones(List<vw_operaciones_vh> oprinsp, string estado_opr, bool autorz, int idtec)
        {
           var  listprinsp= oprinsp.GroupBy(d => d.codigo).Select(d => new vw_operaciones_vh { ot_id = d.Select(e => e.ot_id).FirstOrDefault(), codigo = d.Key, tsope_id=d.Select(e => e.tsope_id).FirstOrDefault() }).ToList();

            foreach (vw_operaciones_vh item in listprinsp)
            {
                inst_operacion_insp(item, estado_opr, autorz);              
            }

            foreach (vw_operaciones_vh item in oprinsp)
            {
                if (autorz)
                {
                    inst_manodeobra(item, idtec);
                }
            }
        }

        [HttpPost]
        public ActionResult tablas()
        {
            int idot = Convert.ToInt32(Request["ot_id"]);
            bool autorz = Convert.ToInt32(Request["autorz"])==1? true: false;
            List<listaGeneral> sist = sistemas(idot);
            List<int> sist_B = new List<int>();
            List<int> sist_C = new List<int>();
            using (DbContextTransaction dbTrn = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (listaGeneral item in sist)
                    {
                        icb_tsistemas_ot otsis = new icb_tsistemas_ot
                        {
                            ot_id = idot,
                            tsis_id = item.id,
                            tsis_estadovh = Request["sist_" + item.id]
                        };
                        db.icb_tsistemas_ot.Add(otsis);
                        db.SaveChanges();
                        if (Request["sist_" + item.id] == "B")
                        {
                            sist_B.Add(item.id);
                        }
                        else if (Request["sist_" + item.id] == "C")
                        {
                            sist_C.Add(item.id);
                        }
                    }

                    List<vw_operaciones_vh> opr_B = db.vw_operaciones_vh.Where(x => x.ot_id == idot && x.tsis_estadovh == "B").ToList();
                    List<vw_operaciones_vh> opr_C = db.vw_operaciones_vh.Where(x => x.ot_id == idot && x.tsis_estadovh == "C").ToList();
                    int idtec = tecnicoOt(idot);
                    if (opr_B!=null)
                        {
                        inst_operaciones(opr_B, "B", false, idtec);
                        }
                    if (opr_C!=null)
                        {
                        inst_operaciones(opr_C, "C", false, idtec);
                        }                  
                    dbTrn.Commit();
                    List<vw_operaciones_vh> ls_opr_B = new List<vw_operaciones_vh>();
                    List<vw_operaciones_vh> ls_opr_C = new List<vw_operaciones_vh>();
                    var list_opr_B = (from a in db.vw_operaciones_vh
                                      join b in db.icb_tinspeccionsistemasvh on new { v1 = a.ot_id, v2 = a.tsope_id } equals new
                                      { v1 = b.ot_id.Value, v2 = b.tsope_id }
                                      where a.ot_id == idot && a.tsis_estadovh == "B"
                                      select new
                                      {
                                          a.tiempo,
                                          mnobra_id = autorz ? 1 : 0,
                                          b.tinsvh_id,
                                          a.operacion,
                                          a.referencia,
                                          a.tsis_sistema,
                                          a.precio_repuesto,
                                          a.tinsvh_resp_sms,
                                          a.tinsvh_fecenviosms,
                                          a.precio_total_string
                                      }).ToList(); // db.vw_operaciones_vh.Where(x => x.ot_id == idot && x.tsis_estadovh == "B").ToList(); //(autorz == true) ? opr_B.Select(x => { x.mnobra_id = 1; return x; }).ToList(): opr_B;
                    var list_opr_C = (from a in db.vw_operaciones_vh
                                      join b in db.icb_tinspeccionsistemasvh on new { v1 = a.ot_id, v2 = a.tsope_id } equals new
                                      { v1 = b.ot_id.Value, v2 = b.tsope_id }
                                      where a.ot_id == idot && a.tsis_estadovh == "C"
                                      select new
                                      {
                                          a.tiempo,
                                          mnobra_id = autorz ? 1 : 0,
                                          b.tinsvh_id,
                                          a.operacion,
                                          a.referencia,
                                          a.tsis_sistema,
                                          a.precio_repuesto,
                                          a.tinsvh_resp_sms,
                                          a.tinsvh_fecenviosms,
                                          a.precio_total_string
                                      }).ToList(); // db.vw_operaciones_vh.Where(x => x.ot_id == idot && x.tsis_estadovh == "C").ToList(); //(autorz == true) ? opr_C.Select(x => { x.mnobra_id = 1; return x; }).ToList(): opr_C;
                    foreach (var item in list_opr_B)
                    {
                        ls_opr_B.Add(new vw_operaciones_vh
                        {
                            tiempo = item.tiempo,
                            mnobra_id = item.mnobra_id,
                            tinsvh_id = item.tinsvh_id,
                            operacion = item.operacion,
                            referencia = item.referencia,
                            tsis_sistema = item.tsis_sistema,
                            precio_repuesto = item.precio_repuesto,
                            tinsvh_resp_sms = item.tinsvh_resp_sms,
                            tinsvh_fecenviosms = item.tinsvh_fecenviosms,
                            precio_repuesto_string = item.precio_repuesto != null
                                ? item.precio_repuesto.Value.ToString("N2", new CultureInfo("is-IS"))
                                : "0",
                            precio_total_string = item.precio_total_string
                        });
                    }

                    foreach (var item in list_opr_C)
                    {
                        ls_opr_C.Add(new vw_operaciones_vh
                        {
                            tiempo = item.tiempo,
                            mnobra_id = item.mnobra_id,
                            tinsvh_id = item.tinsvh_id,
                            operacion = item.operacion,
                            referencia = item.referencia,
                            tsis_sistema = item.tsis_sistema,
                            precio_repuesto = item.precio_repuesto,
                            tinsvh_resp_sms = item.tinsvh_resp_sms,
                            tinsvh_fecenviosms = item.tinsvh_fecenviosms,
                            precio_repuesto_string = item.precio_repuesto != null
                                ? item.precio_repuesto.Value.ToString("N2", new CultureInfo("is-IS"))
                                : "0",
                            precio_total_string = item.precio_total_string
                        });
                    }

                    ViewBag.opr_B = ls_opr_B.ToList();
                    ViewBag.opr_C = ls_opr_C.ToList();
                    //ver ot existente
                    tencabezaorden ot = db.tencabezaorden.Where(d => d.id == idot).FirstOrDefault();

                    //veo el estado de inspeccion
                    var idestado = db.tcitasestados.Where(d => d.tipoestado == "I").FirstOrDefault();
                    if (ot != null)
                    {
                        if (ot.fecha_inspeccion == null)
                        {
                            ot.estadoorden = idestado.id;
                            ot.fecha_inspeccion = DateTime.Now;
                            db.Entry(ot).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Exception exp = ex;
                    dbTrn.Rollback();
                }
            }

            ViewBag.accion = "tabla";
            ViewBag.operaciones = operacionesTemp(idot);
            return PartialView("tablasOperacion");
        }

        public ActionResult tablasVw(int ot_id)
        {
            List<vw_operaciones_vh> opr_B = db.vw_operaciones_vh
                .Where(x => x.ot_id == ot_id && x.tinsvh_estado == "B" && x.tinsvh_id != null).ToList();
            ViewBag.opr_B = opr_B; // Operaciones para cotizar
            List<vw_operaciones_vh> opr_C = db.vw_operaciones_vh
                .Where(x => x.ot_id == ot_id && x.tinsvh_estado == "C" && x.tinsvh_id != null).ToList();
            ViewBag.opr_C = opr_C; // Operaciones para hacer en la ot
            ViewBag.accion = "tabla";
            ViewBag.operaciones = operacionesTemp(ot_id);
            return PartialView("tablasOperacion");
        }

        private List<listaGeneral> operacionesTemp(int idot)
        {
            List<listaGeneral> operaciones = (from a in db.vw_operaciones_vh
                                              where a.tsope_id != null && a.tinsvh_id == null && a.mnobra_id == null && a.ot_id == idot
                                              select new listaGeneral
                                              {
                                                  descripcion = a.operacion,
                                                  id = a.tsope_id.Value
                                              }).ToList();
            return operaciones;
        }

        public ActionResult operacionesSlc(int idot)
        {
            ViewBag.operaciones = operacionesTemp(idot);
            ViewBag.accion = "combo";
            return PartialView("tablasOperacion");
        }

        [HttpPost]
        public ActionResult filaVw()
        {
            // int ot_id, int op_id, bool autorz, string estadoOp
            int ot_id = Convert.ToInt32(Request["ot_id"]);
            int op_id = Convert.ToInt32(Request["op_id"]);
            bool autorz = Convert.ToInt32(Request["autorz"])==1?true: false;
            string estadoOp = Request["estadoOp"];
            int resp = 0;
            vw_operaciones_vh opr_vh = db.vw_operaciones_vh.FirstOrDefault(x => x.ot_id == ot_id && x.tsope_id == op_id);
            using (DbContextTransaction dbTrn = db.Database.BeginTransaction())
            {
                try
                {
                    int idtec = tecnicoOt(ot_id);
                    inst_operacion_insp(opr_vh, estadoOp, autorz);
                    if (autorz && estadoOp.Trim() != "B")
                    {
                        inst_manodeobra(opr_vh, idtec);
                    }

                    dbTrn.Commit();
                    opr_vh.mnobra_id = autorz ? 1 : 0;

                    resp = 1;
                }
                catch (Exception ex)
                {
                    Exception exp = ex;
                    dbTrn.Rollback();
                }
            }

            opr_vh.tinsvh_id = resp == 1
                ? db.icb_tinspeccionsistemasvh.Where(x => x.ot_id == ot_id && x.tsope_id == op_id).FirstOrDefault()
                    .tinsvh_id
                : 0;
            ViewBag.accion = "fila";
            ViewBag.resp = resp;
            ViewBag.estadoOp = estadoOp;
            return PartialView("tablasOperacion", opr_vh);
        }

        public ActionResult notificacionOperacion(int idins_op)
        {
            vw_operaciones_vh oprins = db.vw_operaciones_vh.FirstOrDefault(x => x.tinsvh_id == idins_op);
            return View(oprins);
        }

        public JsonResult confirmarRespuesto()
        {
            DateTime fec = DateTime.Now;
            string txt = Request["txt_confirmar"];
            bool autoriza = Request["txt_confirmar"].Trim() == "1";
            int insp_op = Convert.ToInt32(Request["txt_insp_op"]);
            string observacion = Request["txt_observacion"];
            bool error = false;
            icb_tinspeccionsistemasvh insp = db.icb_tinspeccionsistemasvh.FirstOrDefault(x => x.tinsvh_id == insp_op);
            if (insp != null)
            {
                using (DbContextTransaction dbTrn = db.Database.BeginTransaction())
                {
                    try
                    {
                        insp.tinsvh_observacion = observacion;
                        insp.tinsvh_resp_sms = autoriza;
                        insp.tinsvh_autorizar = autoriza;
                        insp.tinsvh_fecenviosms = DateTime.Now;
                        db.Entry(insp).State = EntityState.Modified;
                        db.SaveChanges();
                        if (autoriza)
                        {
                            vw_operaciones_vh opr = db.vw_operaciones_vh.FirstOrDefault(x => x.tinsvh_id == insp_op);
                            int idtec = tecnicoOt(opr.ot_id);
                            inst_manodeobra(opr, idtec);
                        }

                        fec = insp.tinsvh_fecenviosms.Value;
                        dbTrn.Commit();
                    }
                    catch (Exception ex)
                    {
                        Exception exp = ex;
                        dbTrn.Rollback();
                        error = true;
                    }
                }
            }

            return Json(new { error, resl = autoriza, fec = fec.ToString(), obs = observacion },
                JsonRequestBehavior.AllowGet);
        }

        public JsonResult confirmarRespuesto2()
        {
            DateTime fec = DateTime.Now;
            string txt = Request["confirmar"];
            bool autoriza = Request["confirmar"].Trim() == "1";
            int insp_op = Convert.ToInt32(Request["idope"]);
            string observacion = Request["observacion"];
            bool error = false;
            icb_tinspeccionsistemasvh insp = db.icb_tinspeccionsistemasvh.FirstOrDefault(x => x.tinsvh_id == insp_op);
            if (insp != null)
            {
                using (DbContextTransaction dbTrn = db.Database.BeginTransaction())
                {
                    try
                    {
                        insp.tinsvh_observacion = observacion;
                        insp.tinsvh_resp_sms = autoriza;
                        insp.tinsvh_autorizar = autoriza;
                        insp.tinsvh_fecenviosms = DateTime.Now;
                        db.Entry(insp).State = EntityState.Modified;
                        db.SaveChanges();
                        if (autoriza)
                        {
                            vw_operaciones_vh opr = db.vw_operaciones_vh.FirstOrDefault(x => x.tinsvh_id == insp_op);
                            int idtec = tecnicoOt(opr.ot_id);
                            inst_manodeobra(opr, idtec);
                        }

                        fec = insp.tinsvh_fecenviosms.Value;
                        dbTrn.Commit();
                    }
                    catch (Exception ex)
                    {
                        Exception exp = ex;
                        dbTrn.Rollback();
                        error = true;
                    }
                }
            }

            return Json(new { error, resl = autoriza, fec = fec.ToString(), obs = observacion },
                JsonRequestBehavior.AllowGet);
        }


        public void eliminarOperacionSignoVitales(int id, bool origen = true)
        {
            var opr = (from o in db.tdetallemanoobraot
                       join vw in db.vw_operaciones_vh on
                           new { temp = o.idtempario, ot = o.idorden } equals new { temp = vw.codigo, ot = vw.ot_id }
                       where o.id == id
                       select new
                       {
                           vw.tinsvh_id,
                           vw.ot_id
                       }).FirstOrDefault();
            if (opr != null)
            {
                eliminarOperacion(opr.tinsvh_id.Value, opr.ot_id, origen);
            }
        }

        public JsonResult eliminarOperacion(int oprid, int idot, bool origen = true)
        {
            int resl = 0;
            using (DbContextTransaction dbTr = db.Database.BeginTransaction())
            {
                try
                {
                    vw_operaciones_vh opr_ot = db.vw_operaciones_vh.Where(x => x.tinsvh_id == oprid && x.ot_id == idot)
                        .FirstOrDefault();
                    if (opr_ot != null)
                    {
                        icb_tinspeccionsistemasvh opr_ins = db.icb_tinspeccionsistemasvh.Where(x => x.ot_id == idot && x.tinsvh_id == oprid)
                            .FirstOrDefault();
                        if (opr_ins != null)
                        {
                            db.icb_tinspeccionsistemasvh.Remove(opr_ins);
                            db.SaveChanges();
                        }

                        tdetallemanoobraot mno_obra = db.tdetallemanoobraot
                            .Where(x => x.idtempario == opr_ot.codigo && x.idorden == idot).FirstOrDefault();
                        if (mno_obra != null && origen)
                        {
                            db.tdetallemanoobraot.Remove(mno_obra);
                            db.SaveChanges();
                        }

                        resl = 1;
                    }

                    dbTr.Commit();
                }
                catch (Exception ex)
                {
                    Exception exp = ex;
                    resl = 0;
                    dbTr.Rollback();
                }
            }

            return Json(new { resl });
        }

        public JsonResult envioMensajeTexto(int idins_op)
        {
            listaGeneral dat = (from o in db.vw_operaciones_vh
                                join v in db.tencabezaorden on o.ot_id equals v.id
                                join tr in db.icb_terceros on v.tercero equals tr.tercero_id
                                where o.tinsvh_id == idins_op
                                select new listaGeneral
                                {
                                    descripcion = o.placa,
                                    codigo = tr.celular_tercero
                                }).FirstOrDefault();

            HttpClient client = new HttpClient();
            string celu = "57" + dat.codigo;
            int resp = 0;
            string dire = Request.Url.Scheme + "://" + Request.Url.Authority;
            string urlApp = dire + "/enviosmsnotf";
            string mensaje = "Iceberg le informa: al realizar una inspeccion general de su vehiculo " + dat.descripcion +
                          ", encontramos una reparacion pendiente. Por favor ingresar " + urlApp + "?idins_op=" +
                          idins_op;
            string msm = mensaje.Replace(" ", "%20");
            MensajesDeTexto enviar2 = new MensajesDeTexto();
            string status = enviar2.enviarmensaje(celu, msm);

            HttpStatusCode statuscode = HttpStatusCode.OK;

            if (status == "OK")
            {
                icb_tinspeccionsistemasvh tinspvh = db.icb_tinspeccionsistemasvh.FirstOrDefault(x => x.tinsvh_id == idins_op);
                if (tinspvh != null)
                {
                    tinspvh.tinsvh_fecenviosms = DateTime.Now;
                    db.Entry(tinspvh).State = EntityState.Modified;
                    resp = db.SaveChanges();
                }
            }
            else
            {
                statuscode = HttpStatusCode.InternalServerError;
            }

            return Json(new { status, statuscode, resp }, JsonRequestBehavior.AllowGet);
        }

        public string observacionMensajeTexto(int idins_op)
        {
            string observacion = db.icb_tinspeccionsistemasvh.FirstOrDefault(x => x.tinsvh_id == idins_op)
                .tinsvh_observacion;
            return observacion;
        }

        public JsonResult operacionesSinConfirmacion()
        {
            List<int> operaciones = new List<int>();
            int ot_id = Convert.ToInt32(Request["ot_id"]);
            List<int> oprSinConfirmacion = db.vw_operaciones_vh
                .Where(x => x.mnobra_id == null && x.tinsvh_id != null && x.tinsvh_resp_sms == null &&
                            x.ot_id == ot_id && x.tinsvh_estado == "C")
                .Select(x => x.tinsvh_id.Value).ToList();
            oprSinConfirmacion.ForEach(x => { operaciones.Add(x); });

            return Json(operaciones, JsonRequestBehavior.AllowGet);
        }

        public JsonResult operacionesConfirmadas()
        {
            List<int> operaciones = new List<int>();
            int ot_id = Convert.ToInt32(Request["ot_id"]);
            string oopp = Request["operaciones"];
            List<int> opr = Request["operaciones"].Split(',').Select(int.Parse).ToList();
            var oprConfirmadas = (from o in opr
                                  join v in db.vw_operaciones_vh on o equals v.tinsvh_id
                                  where v.tinsvh_id != null && (v.mnobra_id != null || v.tinsvh_resp_sms != null)
                                  select new
                                  {
                                      v.tinsvh_id,
                                      v.mnobra_id,
                                      v.tinsvh_resp_sms
                                  }).ToList();
            return Json(oprConfirmadas, JsonRequestBehavior.AllowGet);
        }
    }
}