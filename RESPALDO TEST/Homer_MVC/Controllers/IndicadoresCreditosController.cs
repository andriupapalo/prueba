using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class IndicadoresCreditosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");
        private readonly CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

        // GET: InformesCreditos
        public ActionResult Index(int? menu)
        {
            var asesores = (from u in context.users
                            where u.rol_id == 4
                            select new
                            {
                                u.user_nombre,
                                u.user_apellido,
                                u.user_id
                            }).ToList();
            List<SelectListItem> lista_asesores = new List<SelectListItem>();
            foreach (var item in asesores)
            {
                lista_asesores.Add(new SelectListItem
                {
                    Text = item.user_nombre + ' ' + item.user_apellido,
                    Value = item.user_id.ToString()
                });
            }

            ViewBag.asesor = lista_asesores;
            ViewBag.financiera = new SelectList(context.icb_unidad_financiera, "financiera_id", "financiera_nombre");
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult Indicadores()
        {
            return View();
        }

        public JsonResult buscarDatos(DateTime? fechaDesde, DateTime? fechaHasta, int? financiera, int? asesor)
        {
            int bodega = Convert.ToInt32(Session["user_bodega"]);

            if (fechaDesde == null)
            {
                fechaDesde = DateTime.Now.AddMonths(-1);
            }

            if (fechaHasta == null)
            {
                fechaHasta = DateTime.Now.AddDays(1);
            }
            else
            {
                fechaHasta = fechaHasta.Value.AddDays(1);
            }

            List<vw_estado_creditos> consulta = context.vw_estado_creditos.Where(x =>
                x.fec_creacion >= fechaDesde && x.fec_creacion <= fechaHasta && x.bodega_id == bodega).ToList();

            if (financiera != null)
            {
                consulta = consulta.Where(x => x.financiera_id == financiera).ToList();
            }

            if (asesor != null)
            {
                consulta = consulta.Where(x => x.asesor_id == asesor).ToList();
            }

            var solicitados = consulta.Where(x => x.fec_solicitud != null).Select(d => new
            {
                pedido = d.pedido != null ? d.pedido.ToString() : "",
                fec_solicitud = d.fec_solicitud != null
                    ? d.fec_solicitud.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                valor_solicitado = d.valor_solicitado != null
                    ? d.valor_solicitado.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "",
                d.estado, //new id 
                d.plazo,
                financiera_nombre = d.financiera_nombre != null ? d.financiera_nombre : "",
                cliente = d.cliente != null ? d.cliente : "",
                asesor = d.asesor != null ? d.asesor : "",
                plan_fin = d.plan_fin != null ? d.plan_fin : "",
                bodega = d.bodega != null ? d.bodega : "",
                cuota = d.cuotainicial != null ? d.cuotainicial.Value.ToString("N0", new CultureInfo("is-IS")) : "",
                d.responsable
            }).ToList();
            var aprobados = consulta.Where(x => x.fec_aprobacion != null).Select(d => new
            {
                pedido = d.pedido != null ? d.pedido.ToString() : "",
                //dateReq = d.fec_solicitud != null ? d.fec_solicitud.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")): "",
                dateApr = d.fec_aprobacion != null
                    ? d.fec_aprobacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                estado = d.estado != null ? d.estado : "",
                num_aprobacion = d.num_aprobacion != null ? d.num_aprobacion : "",
                valueApp = d.valor_aprobado != null
                    ? d.valor_aprobado.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "",
                reqvalue = d.valor_solicitado != null
                    ? d.valor_solicitado.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "",
                plazo = d.plazo != null ? d.plazo.ToString() : "",
                dateReq = d.fec_solicitud != null
                    ? d.fec_solicitud.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                d.financiera_nombre,
                cliente = d.cliente != null ? d.cliente : "",
                asesor = d.asesor != null ? d.asesor : "",
                plan_fin = d.plan_fin != null ? d.plan_fin : "",
                bodega = d.bodega != null ? d.bodega : "",
                feeInit = d.cuotainicial != null ? d.cuotainicial.Value.ToString("N0", new CultureInfo("is-IS")) : "",
                responsable = d.responsable != null ? d.responsable : "",
                comission = d.valor_comision != null
                    ? d.valor_comision.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "",
                comison = d.comison != true ? d.comison.ToString() : "",
            }).ToList();
            var negados = consulta.Where(x => x.fec_negacion != null).Select(d => new
            {
                pedido = d.pedido != null ? d.pedido.ToString() : "",
                dateReq = d.fec_solicitud != null
                    ? d.fec_solicitud.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                reqvalue = d.valor_solicitado != null
                    ? d.valor_solicitado.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                estado = d.estado != null ? d.estado : "",
                dateDen = d.fec_negacion != null
                    ? d.fec_negacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                detalle = d.detalle != null ? d.detalle : "",
                financiera_nombre = d.financiera_nombre != null ? d.financiera_nombre : "",
                cliente = d.cliente != null ? d.cliente : "",
                asesor = d.asesor != null ? d.asesor : "",
                d.plan_fin,
                d.bodega,
                feeInit = d.cuotainicial != null ? d.cuotainicial.Value.ToString("N0", new CultureInfo("is-IS")) : "",
                d.responsable
            }).ToList();
            var desistidos = consulta.Where(x => x.fec_desistimiento != null).Select(d => new
            {
                pedido = d.pedido != null ? d.pedido.ToString() : "",
                dateApr = d.fec_aprobacion != null
                    ? d.fec_aprobacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                estado = d.estado != null ? d.estado : "",
                num_aprobacion = d.num_aprobacion != null ? d.num_aprobacion.ToString() : "",
                valueApp = d.valor_aprobado != null
                    ? d.valor_aprobado.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "",
                reqvalue = d.valor_solicitado != null
                    ? d.valor_solicitado.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "",
                dateReq = d.fec_solicitud != null
                    ? d.fec_solicitud.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                plazo = d.plazo != null ? d.plazo.ToString() : "",
                financiera_nombre = d.financiera_nombre != null ? d.financiera_nombre : "",
                cliente = d.cliente != null ? d.cliente : "",
                asesor = d.asesor != null ? d.asesor : "",
                plan_fin = d.plan_fin != null ? d.plan_fin : "",
                bodega = d.bodega != null ? d.bodega : "",
                feeInit = d.cuotainicial != null ? d.cuotainicial.Value.ToString("N0", new CultureInfo("is-IS")) : "",
                responsable = d.responsable != null ? d.responsable : "",
                desistdate = d.fec_desistimiento != null
                    ? d.fec_desistimiento.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                detalle = d.detalle != null ? d.detalle : "",
            }).ToList();
            var comisiones = consulta.Where(x => x.comison).Select(d => new
            {
                pedido = d.pedido != null ? d.pedido.ToString() : "",
                dateCon = d.fec_confirmacion != null
                    ? d.fec_confirmacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                valueApp = d.valor_aprobado != null
                    ? d.valor_aprobado.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "",
                dateRece = d.fec_facturacomision != null
                    ? d.fec_facturacomision.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                commisionVal = d.valor_comision != null
                    ? d.valor_comision.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "",
                estado = d.estado != null ? d.estado : "",
                plazo = d.plazo != null ? d.plazo.Value.ToString() : "",
                d.financiera_nombre,
                asesor = d.asesor != null ? d.asesor : "",
                cliente = d.cliente != null ? d.cliente : "",
                plan_fin = d.plan_fin != null ? d.plan_fin : "",
                bodega = d.bodega != null ? d.bodega : "",
                feeInit = d.cuotainicial != null ? d.cuotainicial.Value.ToString("N0", new CultureInfo("is-Is")) : "",
                d.responsable
            }).ToList();
            var desembolsados = consulta.Where(x => x.fec_desembolso != null).Select(d => new
            {
                pedido = d.pedido != null ? d.pedido.Value.ToString() : " ",
                dateReq = d.fec_solicitud != null ? d.fec_solicitud.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                reqvalue = d.valor_solicitado != null
                    ? d.valor_solicitado.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "",
                dateApp = d.fec_aprobacion != null
                    ? d.fec_aprobacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-Us"))
                    : "",
                estado = d.estado != null ? d.estado : "",
                numfactura = d.numfactura != null ? d.numfactura.Value.ToString() : "",
                valueApp = d.valor_aprobado != null
                    ? d.valor_aprobado.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "",
                plazo = d.plazo != null ? d.plazo.Value.ToString() : "",
                dateOut = d.fec_desembolso != null
                    ? d.fec_desembolso.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-Us"))
                    : "",
                financiera_nombre = d.financiera_nombre != null ? d.financiera_nombre : "",
                cliente = d.cliente != null ? d.cliente : "",
                asesor = d.asesor != null ? d.asesor : "",
                plan_fin = d.plan_fin != null ? d.plan_fin : "",
                d.bodega,
                feeInit = d.cuotainicial != null ? d.cuotainicial.Value.ToString("N0", new CultureInfo("is-IS")) : "",
                d.responsable
            }).ToList();
            var asesores = consulta.GroupBy(x => x.asesor_id).Select(grp => new
            {
                id = grp.Key,
                asesor = grp.Select(x => x.asesor).FirstOrDefault(),
                cantidad = grp.Count()
            }).OrderByDescending(x => x.cantidad).Distinct().ToList();

            int total = context.vw_estado_creditos
                .Where(x => x.fec_solicitud >= fechaDesde && x.fec_solicitud <= fechaHasta && x.bodega_id == bodega)
                .Select(x => x.financiera_id).Count();

            //var nameFinancial = context.vw_estado_creditos.

            var financieras = consulta.GroupBy(x => x.financiera_id).Select(grp => new
            {
                id = grp.Key,
                financiera = grp.Select(x => x.financiera_nombre).FirstOrDefault(),
                cantidad = grp.Count(),
                porcentaje = grp.Count() * 100 / total,
                total
            }).OrderByDescending(x => x.cantidad).ToList();

            int total_desembolsados = consulta.Where(x => x.valor_solicitado != null).Count();
            int total_comisiones = consulta.Where(x => x.valor_comision != null).Count();
            var data = new
            {
                solicitados,
                aprobados,
                negados,
                desistidos,
                comisiones,
                desembolsados,
                asesores,
                financieras,
                total_desembolsados,
                total_comisiones
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarVehiculoFacturacion(DateTime fechaDesde, DateTime fechaHasta, int? asesor,
            int? financiera)
        {
            // todos los filtros
            fechaHasta = fechaHasta.AddDays(1);
            if (fechaDesde != null && fechaHasta != null && asesor != null && financiera != null)
            {
                #region Vehiculos Facturados Todos los Filtros

                var vehiculos2 = (from a in context.v_creditos
                                  join b in context.vpedido
                                      on a.pedido equals b.id
                                  join c in context.icb_vehiculo
                                      on b.planmayor equals c.plan_mayor
                                  join d in context.encab_documento
                                      on a.pedido equals d.id_pedido_vehiculo
                                  join e in context.tp_doc_registros
                                      on d.tipo equals e.tpdoc_id
                                  join f in context.icb_terceros
                                      on d.nit equals f.tercero_id
                                  join g in context.modelo_vehiculo
                                      on b.modelo equals g.modvh_codigo
                                  join h in context.color_vehiculo
                                      on b.Color_Deseado equals h.colvh_id
                                  join j in context.icb_unidad_financiera
                                      on a.financiera_id equals j.financiera_id
                                  join k in context.icb_plan_financiero
                                      on a.plan_id equals k.plan_id into sor
                                  from k in sor.DefaultIfEmpty()
                                  where d.fecha >= fechaDesde && d.fecha <= fechaHasta
                                                              && a.financiera_id == financiera
                                                              && a.asesor_id == asesor
                                  select new
                                  {
                                      e.prefijo,
                                      d.numero,
                                      d.fecha,
                                      d.nit,
                                      f.tpdoc_id,
                                      f.doc_tercero,
                                      f.prinom_tercero,
                                      f.segnom_tercero,
                                      f.apellido_tercero,
                                      f.segapellido_tercero,
                                      f.razon_social,
                                      pedido = b.numero,
                                      b.planmayor,
                                      g.modvh_nombre,
                                      b.id_anio_modelo,
                                      h.colvh_nombre,
                                      c.fecha_venta,
                                      a.vsolicitado,
                                      j.financiera_nombre,
                                      k.plan_nombre,
                                      a.plazo
                                  }).ToList();

                var vehiculos = vehiculos2.Select(c => new
                {
                    tdoc = c.prefijo,
                    docu = c.numero,
                    fecha = c.fecha != null ? c.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                    c.nit,
                    cliente = c.tpdoc_id != 1
                        ? "(" + c.doc_tercero + ") " + c.prinom_tercero + " " + c.segnom_tercero + " " +
                          c.apellido_tercero + " " + c.segapellido_tercero
                        : "(" + c.doc_tercero + ") " + c.razon_social,
                    c.pedido,
                    planM = c.planmayor,
                    modelovh = c.modvh_nombre,
                    anniovh = c.id_anio_modelo,
                    colorvh = c.colvh_nombre,
                    fechaVen = c.fecha_venta != null
                        ? c.fecha_venta.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    valosoli = c.vsolicitado != null ? c.vsolicitado.Value.ToString("N0") : "",
                    valors = c.vsolicitado,
                    financiera = c.financiera_nombre != null ? c.financiera_nombre : "",
                    plan = c.plan_nombre != null ? c.plan_nombre : "",
                    plazo = c.plazo != null ? c.plazo.Value.ToString("N0") : ""
                }).ToList();

                decimal? total_Vehiculosfacturados2 = vehiculos.Sum(x => x.valors);
                string total_Vehiculosfacturados = total_Vehiculosfacturados2.Value.ToString("0,0", elGR);
                int total_VehiculosfacturadosCount = vehiculos.Count;

                #endregion

                var data = new
                {
                    vehiculos,
                    total_Vehiculosfacturados,
                    total_VehiculosfacturadosCount
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            // filtros fechas y financiera
            if (fechaDesde != null && fechaHasta != null && asesor == null && financiera != null)
            {
                #region Vehiculos Facturados filtros fechas y financiera

                var vehiculos2 = (from a in context.v_creditos
                                  join b in context.vpedido
                                      on a.pedido equals b.id
                                  join c in context.icb_vehiculo
                                      on b.planmayor equals c.plan_mayor
                                  join d in context.encab_documento
                                      on a.pedido equals d.id_pedido_vehiculo
                                  join e in context.tp_doc_registros
                                      on d.tipo equals e.tpdoc_id
                                  join f in context.icb_terceros
                                      on d.nit equals f.tercero_id
                                  join g in context.modelo_vehiculo
                                      on b.modelo equals g.modvh_codigo
                                  join h in context.color_vehiculo
                                      on b.Color_Deseado equals h.colvh_id
                                  join j in context.icb_unidad_financiera
                                      on a.financiera_id equals j.financiera_id
                                  join k in context.icb_plan_financiero
                                      on a.plan_id equals k.plan_id into sor
                                  from k in sor.DefaultIfEmpty()
                                  where d.fecha >= fechaDesde && d.fecha <= fechaHasta
                                                              && a.financiera_id == financiera
                                  select new
                                  {
                                      e.prefijo,
                                      d.numero,
                                      d.fecha,
                                      d.nit,
                                      f.tpdoc_id,
                                      f.doc_tercero,
                                      f.prinom_tercero,
                                      f.segnom_tercero,
                                      f.apellido_tercero,
                                      f.segapellido_tercero,
                                      f.razon_social,
                                      pedido = b.numero,
                                      b.planmayor,
                                      g.modvh_nombre,
                                      b.id_anio_modelo,
                                      h.colvh_nombre,
                                      c.fecha_venta,
                                      a.vsolicitado,
                                      j.financiera_nombre,
                                      k.plan_nombre,
                                      a.plazo
                                  }).ToList();

                var vehiculos = vehiculos2.Select(c => new
                {
                    tdoc = c.prefijo,
                    docu = c.numero,
                    fecha = c.fecha != null ? c.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                    c.nit,
                    cliente = c.tpdoc_id != 1
                        ? "(" + c.doc_tercero + ") " + c.prinom_tercero + " " + c.segnom_tercero + " " +
                          c.apellido_tercero + " " + c.segapellido_tercero
                        : "(" + c.doc_tercero + ") " + c.razon_social,
                    c.pedido,
                    planM = c.planmayor,
                    modelovh = c.modvh_nombre,
                    anniovh = c.id_anio_modelo,
                    colorvh = c.colvh_nombre,
                    fechaVen = c.fecha_venta != null
                        ? c.fecha_venta.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    valosoli = c.vsolicitado != null ? c.vsolicitado.Value.ToString("N0") : "",
                    valors = c.vsolicitado,
                    financiera = c.financiera_nombre != null ? c.financiera_nombre : "",
                    plan = c.plan_nombre != null ? c.plan_nombre : "",
                    plazo = c.plazo != null ? c.plazo.Value.ToString("N0") : ""
                }).ToList();

                decimal? total_Vehiculosfacturados2 = vehiculos.Sum(x => x.valors);
                string total_Vehiculosfacturados = total_Vehiculosfacturados2.Value.ToString("0,0", elGR);
                int total_VehiculosfacturadosCount = vehiculos.Count;

                #endregion

                var data = new
                {
                    vehiculos,
                    total_Vehiculosfacturados,
                    total_VehiculosfacturadosCount
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            // filtros fechas y financiera
            if (fechaDesde != null && fechaHasta != null && asesor == null && financiera == null)
            {
                #region Vehiculos Facturados filtros fechas y financiera

                var vehiculos2 = (from a in context.v_creditos
                                  join b in context.vpedido
                                      on a.pedido equals b.id
                                  join c in context.icb_vehiculo
                                      on b.planmayor equals c.plan_mayor
                                  join d in context.encab_documento
                                      on a.pedido equals d.id_pedido_vehiculo
                                  join e in context.tp_doc_registros
                                      on d.tipo equals e.tpdoc_id
                                  join f in context.icb_terceros
                                      on d.nit equals f.tercero_id
                                  join g in context.modelo_vehiculo
                                      on b.modelo equals g.modvh_codigo
                                  join h in context.color_vehiculo
                                      on b.Color_Deseado equals h.colvh_id
                                  join j in context.icb_unidad_financiera
                                      on a.financiera_id equals j.financiera_id
                                  join k in context.icb_plan_financiero
                                      on a.plan_id equals k.plan_id into sor
                                  from k in sor.DefaultIfEmpty()
                                  where d.fecha >= fechaDesde && d.fecha <= fechaHasta
                                  select new
                                  {
                                      e.prefijo,
                                      d.numero,
                                      d.fecha,
                                      d.nit,
                                      f.tpdoc_id,
                                      f.doc_tercero,
                                      f.prinom_tercero,
                                      f.segnom_tercero,
                                      f.apellido_tercero,
                                      f.segapellido_tercero,
                                      f.razon_social,
                                      pedido = b.numero,
                                      b.planmayor,
                                      g.modvh_nombre,
                                      b.id_anio_modelo,
                                      h.colvh_nombre,
                                      c.fecha_venta,
                                      a.vsolicitado,
                                      j.financiera_nombre,
                                      k.plan_nombre,
                                      a.plazo
                                  }).ToList();

                var vehiculos = vehiculos2.Select(c => new
                {
                    tdoc = c.prefijo,
                    docu = c.numero,
                    fecha = c.fecha != null ? c.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                    c.nit,
                    cliente = c.tpdoc_id != 1
                        ? "(" + c.doc_tercero + ") " + c.prinom_tercero + " " + c.segnom_tercero + " " +
                          c.apellido_tercero + " " + c.segapellido_tercero
                        : "(" + c.doc_tercero + ") " + c.razon_social,
                    c.pedido,
                    planM = c.planmayor,
                    modelovh = c.modvh_nombre,
                    anniovh = c.id_anio_modelo,
                    colorvh = c.colvh_nombre,
                    fechaVen = c.fecha_venta != null
                        ? c.fecha_venta.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    valosoli = c.vsolicitado != null ? c.vsolicitado.Value.ToString("N0") : "",
                    valors = c.vsolicitado,
                    financiera = c.financiera_nombre != null ? c.financiera_nombre : "",
                    plan = c.plan_nombre != null ? c.plan_nombre : "",
                    plazo = c.plazo != null ? c.plazo.Value.ToString("N0") : ""
                }).ToList();

                decimal? total_Vehiculosfacturados2 = vehiculos.Sum(x => x.valors);
                string total_Vehiculosfacturados = total_Vehiculosfacturados2.Value.ToString("0,0", elGR);
                int total_VehiculosfacturadosCount = vehiculos.Count;

                #endregion

                var data = new
                {
                    vehiculos,
                    total_Vehiculosfacturados,
                    total_VehiculosfacturadosCount
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            if (fechaDesde != null && fechaHasta != null && asesor != null && financiera == null)
            {
                #region Vehiculos Facturados Todos los Filtros

                var vehiculos2 = (from a in context.v_creditos
                                  join b in context.vpedido
                                      on a.pedido equals b.id
                                  join c in context.icb_vehiculo
                                      on b.planmayor equals c.plan_mayor
                                  join d in context.encab_documento
                                      on a.pedido equals d.id_pedido_vehiculo
                                  join e in context.tp_doc_registros
                                      on d.tipo equals e.tpdoc_id
                                  join f in context.icb_terceros
                                      on d.nit equals f.tercero_id
                                  join g in context.modelo_vehiculo
                                      on b.modelo equals g.modvh_codigo
                                  join h in context.color_vehiculo
                                      on b.Color_Deseado equals h.colvh_id
                                  join j in context.icb_unidad_financiera
                                      on a.financiera_id equals j.financiera_id
                                  join k in context.icb_plan_financiero
                                      on a.plan_id equals k.plan_id into sor
                                  from k in sor.DefaultIfEmpty()
                                  where d.fecha >= fechaDesde && d.fecha <= fechaHasta
                                                              && a.asesor_id == asesor
                                  select new
                                  {
                                      e.prefijo,
                                      d.numero,
                                      d.fecha,
                                      d.nit,
                                      f.tpdoc_id,
                                      f.doc_tercero,
                                      f.prinom_tercero,
                                      f.segnom_tercero,
                                      f.apellido_tercero,
                                      f.segapellido_tercero,
                                      f.razon_social,
                                      pedido = b.numero,
                                      b.planmayor,
                                      g.modvh_nombre,
                                      b.id_anio_modelo,
                                      h.colvh_nombre,
                                      c.fecha_venta,
                                      a.vsolicitado,
                                      j.financiera_nombre,
                                      k.plan_nombre,
                                      a.plazo
                                  }).ToList();

                var vehiculos = vehiculos2.Select(c => new
                {
                    tdoc = c.prefijo,
                    docu = c.numero,
                    fecha = c.fecha != null ? c.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                    c.nit,
                    cliente = c.tpdoc_id != 1
                        ? "(" + c.doc_tercero + ") " + c.prinom_tercero + " " + c.segnom_tercero + " " +
                          c.apellido_tercero + " " + c.segapellido_tercero
                        : "(" + c.doc_tercero + ") " + c.razon_social,
                    c.pedido,
                    planM = c.planmayor,
                    modelovh = c.modvh_nombre,
                    anniovh = c.id_anio_modelo,
                    colorvh = c.colvh_nombre,
                    fechaVen = c.fecha_venta != null
                        ? c.fecha_venta.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    valosoli = c.vsolicitado != null ? c.vsolicitado.Value.ToString("N0") : "",
                    valors = c.vsolicitado,
                    financiera = c.financiera_nombre != null ? c.financiera_nombre : "",
                    plan = c.plan_nombre != null ? c.plan_nombre : "",
                    plazo = c.plazo != null ? c.plazo.Value.ToString("N0") : ""
                }).ToList();

                decimal? total_Vehiculosfacturados2 = vehiculos.Sum(x => x.valors);
                string total_Vehiculosfacturados = total_Vehiculosfacturados2.Value.ToString("0,0", elGR);
                int total_VehiculosfacturadosCount = vehiculos.Count;

                #endregion

                var data = new
                {
                    vehiculos,
                    total_Vehiculosfacturados,
                    total_VehiculosfacturadosCount
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }


            return Json(JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarDetallesFinancieras(DateTime fechaDesde, DateTime fechaHasta, int? asesor,
            int financiera)
        {
            // todos los filtros
            fechaHasta = fechaHasta.AddDays(1);
            if (fechaDesde != null && fechaHasta != null && asesor != null)
            {
                var datos = (from c in context.v_creditos
                             join pl in context.icb_plan_financiero
                                 on c.plan_id equals pl.plan_id into cpl
                             from pl in cpl.DefaultIfEmpty()
                             join bo in context.bodega_concesionario
                                 on c.bodegaid equals bo.id into boc
                             from bo in boc.DefaultIfEmpty()
                             join us in context.users
                                 on c.userid_creacion equals us.user_id into cus
                             from us in cus.DefaultIfEmpty()
                             join u_asesor in context.users
                                 on c.asesor_id equals u_asesor.user_id into sor
                             from u_asesor in sor.DefaultIfEmpty()
                             join vw in context.vw_estado_creditos
                                 on c.financiera_id equals vw.financiera_id
                             where c.fec_solicitud >= fechaDesde && c.fec_solicitud <= fechaHasta
                                                                 && c.financiera_id == financiera
                                                                 && c.asesor_id == asesor
                             select new
                             {
                                 pedido = c.vpedido.numero != null ? c.vpedido.numero.ToString() : "",
                                 fec_solicitud = c.fec_solicitud,
                                 //c.fec_aprobacion,
                                 c.fec_confirmacion,
                                 c.fec_desembolso,
                                 c.fec_negacion,
                                 c.fec_desistimiento,
                                 financiera = c.icb_unidad_financiera.financiera_nombre,
                                 c.vsolicitado,
                                 c.vaprobado,
                                 c.num_aprobacion,
                                 c.estadoc,
                                 c.vcredmotdesistio.motivo,
                                 c.plazo,
                                 vw.fec_aprobacion,
                                 vw.financiera_nombre,
                                 asesor = u_asesor.user_nombre + " " + u_asesor.user_apellido,
                                 //asesor = u_asesor.user_nombre + " " + u_asesor.user_apellido, //c.users.user_nombre + " " + c.users.user_apellido,
                                 cliente = "(" + c.vinfcredito.icb_terceros.doc_tercero + ") " +
                                           c.vinfcredito.icb_terceros.prinom_tercero + " " +
                                           c.vinfcredito.icb_terceros.segnom_tercero + " " +
                                           c.vinfcredito.icb_terceros.apellido_tercero + " " +
                                           c.vinfcredito.icb_terceros.segapellido_tercero,
                                 pl.plan_nombre,
                                 bodega = c.bodegaid != null ? "(" + bo.bodccs_cod + ") " + bo.bodccs_nombre : "",
                                 c.cuota_inicial,
                                 responsable = us.user_nombre + " " + us.user_apellido,
                                 c.fec_envdocumentos
                                 //cumplimiento = d.fec_aprobacion != null ? calcularcumplimiento(d.id, d.tiempo_aprobacion, d.fec_solicitud, d.fec_aprobacion) : d.fec_negado != null ? calcularcumplimiento(d.id, d.tiempo_aprobacion, d.fec_solicitud, d.fec_negado) : d.fec_desistimiento != null ? calcularcumplimiento(d.id, d.tiempo_aprobacion, d.fec_solicitud, d.fec_negado) : "",
                             }).ToList();
                var data = datos.Select(d => new
                {
                    pedido = d.pedido != null ? d.pedido : "",
                    fecha_solicitud = d.fec_solicitud != null
                        ? d.fec_solicitud.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    fecha_aprobacion = d.fec_aprobacion != null
                        ? d.fec_aprobacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    fecha_confirmacion = d.fec_confirmacion != null
                        ? d.fec_confirmacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    fecha_desembolso = d.fec_desembolso != null
                        ? d.fec_desembolso.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    fecha_negacion = d.fec_negacion != null
                        ? d.fec_negacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    fecha_desistimiento = d.fec_desistimiento != null
                        ? d.fec_desistimiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    d.financiera,
                    nombreFinanciera = d.financiera_nombre != null,
                    valor_solicitado = d.vsolicitado != null ? d.vsolicitado : 0,
                    valor_aprobado = d.vaprobado != null ? d.vaprobado : 0,
                    numero = d.num_aprobacion != null ? d.num_aprobacion.ToString() : "",
                    estado = d.estadoc != null ? d.estadoc : "",
                    motivo = d.motivo != null ? d.motivo : "",
                    plazo = d.plazo.Value.ToString(),
                    asesor = d.asesor != null ? d.asesor : "",
                    cliente = d.cliente != null ? d.cliente : "",
                    plan = d.plan_nombre != null ? d.plan_nombre : "",
                    bodega = d.bodega != null ? d.bodega : "",
                    cuotainicial = d.cuota_inicial != null ? d.cuota_inicial : 0,
                    responsable = d.responsable != null ? d.responsable : "",
                    d.fec_envdocumentos,
                    cumplimiento = d.fec_envdocumentos != null
                        ? calcularcumplimiento(d.fec_envdocumentos, d.fec_aprobacion)
                        : "" ///? calcularcumplimiento(d.id, d.tiempo_aprobacion, d.fec_solicitud, d.fec_negado) : d.fec_desistimiento != null ? calcularcumplimiento(d.id, d.tiempo_aprobacion, d.fec_solicitud, d.fec_negado) : "",
				}).ToList();


                return Json(data, JsonRequestBehavior.AllowGet);
            }

            // filtros fechas y financiera
            if (fechaDesde != null && fechaHasta != null && asesor == null)
            {
                #region 1

                var datos = (from c in context.v_creditos
                             join d in context.vinfcredito
                                 on c.infocredito_id equals d.id
                             join pl in context.icb_plan_financiero
                                 on c.plan_id equals pl.plan_id into cpl
                             from pl in cpl.DefaultIfEmpty()
                             join bo in context.bodega_concesionario
                                 on c.bodegaid equals bo.id into boc
                             from bo in boc.DefaultIfEmpty()
                             join us in context.users
                                 on c.userid_creacion equals us.user_id into cus
                             from us in cus.DefaultIfEmpty()
                             join u_asesor in context.users
                                 on c.asesor_id equals u_asesor.user_id into sor
                             from u_asesor in sor.DefaultIfEmpty()
                             where c.fec_solicitud >= fechaDesde && c.fec_solicitud <= fechaHasta
                                                                 && c.financiera_id == financiera
                             select new
                             {
                                 pedido = c.vpedido.numero,
                                 c.fec_solicitud,
                                 c.fec_aprobacion,
                                 c.fec_confirmacion,
                                 c.fec_desembolso,
                                 c.fec_negacion,
                                 c.fec_desistimiento,
                                 financiera = c.icb_unidad_financiera.financiera_nombre,
                                 c.vsolicitado,
                                 c.vaprobado,
                                 c.num_aprobacion,
                                 c.estadoc,
                                 c.vcredmotdesistio.motivo,
                                 c.plazo,
                                 asesor = u_asesor.user_nombre + " " +
                                          u_asesor.user_apellido, //asesor = c.users.user_nombre + " " + c.users.user_apellido,
                                 cliente = "(" + c.vinfcredito.icb_terceros.doc_tercero + ") " +
                                           c.vinfcredito.icb_terceros.prinom_tercero + " " +
                                           c.vinfcredito.icb_terceros.segnom_tercero + " " +
                                           c.vinfcredito.icb_terceros.apellido_tercero + " " +
                                           c.vinfcredito.icb_terceros.segapellido_tercero,
                                 pl.plan_nombre,
                                 bodega = c.bodegaid != null ? "(" + bo.bodccs_cod + ") " + bo.bodccs_nombre : "",
                                 c.cuota_inicial,
                                 responsable = us.user_nombre + " " + us.user_apellido,
                                 c.fec_envdocumentos
                             }).ToList();

                #endregion

                var data = datos.Select(d => new
                {
                    pedido = d.pedido != null ? d.pedido.ToString() : "",
                    fecha_solicitud = d.fec_solicitud != null
                        ? d.fec_solicitud.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    fecha_aprobacion = d.fec_aprobacion != null
                        ? d.fec_aprobacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    fecha_confirmacion = d.fec_confirmacion != null
                        ? d.fec_confirmacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    fecha_desembolso = d.fec_desembolso != null
                        ? d.fec_desembolso.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    fecha_negacion = d.fec_negacion != null
                        ? d.fec_negacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    fecha_desistimiento = d.fec_desistimiento != null
                        ? d.fec_desistimiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    d.financiera,
                    valor_solicitado = d.vsolicitado != null ? d.vsolicitado : 0,
                    valor_aprobado = d.vaprobado != null ? d.vaprobado : 0,
                    numero = d.num_aprobacion != null ? d.num_aprobacion.ToString() : "",
                    estado = d.estadoc != null ? d.estadoc : "",
                    motivo = d.motivo != null ? d.motivo : "",
                    plazo = d.plazo.Value.ToString(),
                    asesor = d.asesor != null ? d.asesor : "",
                    cliente = d.cliente != null ? d.cliente : "",
                    plan = d.plan_nombre != null ? d.plan_nombre : "",
                    bodega = d.bodega != null ? d.bodega : "",
                    cuotainicial = d.cuota_inicial != null ? d.cuota_inicial : 0,
                    responsable = d.responsable != null ? d.responsable : "",
                    d.fec_envdocumentos,
                    cumplimiento = d.fec_envdocumentos != null
                        ? calcularcumplimiento(d.fec_envdocumentos, d.fec_aprobacion)
                        : "" ///? calcularcumplimiento(d.id, d.tiempo_aprobacion, d.fec_solicitud, d.fec_negado) : d.fec_desistimiento != null ? calcularcumplimiento(d.id, d.tiempo_aprobacion, d.fec_solicitud, d.fec_negado) : "",
				}).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(JsonRequestBehavior.AllowGet);
        }

        //public string calcularcumplimiento(int id, int tiempo_aprobacion, DateTime? fec_solicitud, DateTime? fec_dos)
        public string calcularcumplimiento(DateTime? fec_envdocumentos, DateTime? fec_aprobacion)
        {
            string cumplimiento = "";
            if (fec_aprobacion == null)
            {
                fec_aprobacion = DateTime.Now;
            }
            //if (tiempo_aprobacion != 0 && fec_solicitud != null & fec_dos != null)
            if ((fec_envdocumentos != null) & (fec_aprobacion != null))
            {
                TimeSpan ts = Convert.ToDateTime(fec_aprobacion) - Convert.ToDateTime(fec_envdocumentos);
                double dias = ts.TotalSeconds / 86400;

                if (dias <= 1) // (tiempo.Value.TotalDays <= 1)
                {
                    cumplimiento = "success";
                }
                //cumplimiento = "danger";  /*///cumplimiento = "<span class='badge badge-danger'>&nbsp;&nbsp;&nbsp;&nbsp;</span>";*/
                //else if (dias <= 2)//(tiempo.Value.TotalDays > 1 || tiempo.Value.TotalDays <=2)   //if (tiempo.Value.TotalSeconds >= (tiempo_aprobacion * 3600 * 0.75) && tiempo.Value.Seconds <= tiempo_aprobacion * 3600)
                //{
                //	cumplimiento = "warning"; // cumplimiento = "<span class='badge badge-warning'>&nbsp;&nbsp;&nbsp;&nbsp;</span>";
                //}
                else if (dias <= 3
                ) //(tiempo.Value.TotalDays >= 3)   //if (tiempo.Value.TotalSeconds >= (tiempo_aprobacion * 3600 * 0.75) && tiempo.Value.Seconds <= tiempo_aprobacion * 3600)
                {
                    cumplimiento =
                        "warning"; // cumplimiento = "<span class='badge badge-warning'>&nbsp;&nbsp;&nbsp;&nbsp;</span>";
                }
                else
                {
                    cumplimiento = "danger";
                }

                return cumplimiento;
            }

            return cumplimiento;
        }

        public JsonResult buscarDetallesAsesores(DateTime fechaDesde, DateTime fechaHasta, int? financiera, int asesor)
        {
            fechaHasta = fechaHasta.AddDays(1);
            // todos los filtros
            if (fechaDesde != null && fechaHasta != null && financiera != null)
            {
                var data = (from c in context.v_creditos
                            join pl in context.icb_plan_financiero
                                on c.plan_id equals pl.plan_id into cpl
                            from pl in cpl.DefaultIfEmpty()
                            join bo in context.bodega_concesionario
                                on c.bodegaid equals bo.id into boc
                            from bo in boc.DefaultIfEmpty()
                            join us in context.users
                                on c.userid_creacion equals us.user_id into cus
                            from us in cus.DefaultIfEmpty()
                            join u_asesor in context.users
                                on c.asesor_id equals u_asesor.user_id into sor
                            from u_asesor in sor.DefaultIfEmpty()
                            where c.fec_solicitud >= fechaDesde && c.fec_solicitud <= fechaHasta
                                                                && c.financiera_id == financiera
                                                                && c.asesor_id == asesor
                            select new
                            {
                                pedido = c.vpedido.numero != null ? c.vpedido.numero.ToString() : "",
                                fecha_solicitud = c.fec_solicitud.Value.ToString(),
                                fecha_aprobacion = c.fec_aprobacion.Value.ToString(),
                                fecha_confirmacion = c.fec_confirmacion.Value.ToString(),
                                fecha_desembolso = c.fec_desembolso.Value.ToString(),
                                fecha_negacion = c.fec_negacion.Value.ToString(),
                                fecha_desistimiento = c.fec_desistimiento.Value.ToString(),
                                financiera = c.icb_unidad_financiera.financiera_nombre,
                                valor_solicitado = c.vsolicitado,
                                valor_aprobado = c.vaprobado != null ? c.vaprobado : 0,
                                numero = c.num_aprobacion ?? "Sin número de aprobación",
                                estado = c.estadoc,
                                c.vcredmotdesistio.motivo,
                                plazo = c.plazo.Value.ToString(),
                                asesor = u_asesor.user_nombre + " " +
                                         u_asesor.user_apellido, //asesor = c.users.user_nombre + " " + c.users.user_apellido,
                                cliente = "(" + c.vinfcredito.icb_terceros.doc_tercero + ") " +
                                          c.vinfcredito.icb_terceros.prinom_tercero + " " +
                                          c.vinfcredito.icb_terceros.segnom_tercero + " " +
                                          c.vinfcredito.icb_terceros.apellido_tercero + " " +
                                          c.vinfcredito.icb_terceros.segapellido_tercero,
                                plan = pl.plan_nombre != null ? pl.plan_nombre : "",
                                bodega = c.bodegaid != null ? "(" + bo.bodccs_cod + ") " + bo.bodccs_nombre : "",
                                cuotainicial = c.cuota_inicial,
                                responsable = us.user_nombre + " " + us.user_apellido
                            }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            // filtros fechas y financiera
            if (fechaDesde != null && fechaHasta != null && financiera == null)
            {
                var data = (from c in context.v_creditos
                            join pl in context.icb_plan_financiero
                                on c.plan_id equals pl.plan_id into cpl
                            from pl in cpl.DefaultIfEmpty()
                            join bo in context.bodega_concesionario
                                on c.bodegaid equals bo.id into boc
                            from bo in boc.DefaultIfEmpty()
                            join us in context.users
                                on c.userid_creacion equals us.user_id into cus
                            from us in cus.DefaultIfEmpty()
                            join u_asesor in context.users
                                on c.asesor_id equals u_asesor.user_id into sor
                            from u_asesor in sor.DefaultIfEmpty()
                            where c.fec_solicitud >= fechaDesde && c.fec_solicitud <= fechaHasta
                                                                && c.asesor_id == asesor
                            //&& c.userid_creacion == asesor
                            select new
                            {
                                pedido = c.vpedido.numero != null ? c.vpedido.numero.ToString() : "",
                                fecha_solicitud = c.fec_solicitud.Value.ToString(),
                                fecha_aprobacion = c.fec_aprobacion.Value.ToString(),
                                fecha_confirmacion = c.fec_confirmacion.Value.ToString(),
                                fecha_desembolso = c.fec_desembolso.Value.ToString(),
                                fecha_negacion = c.fec_negacion.Value.ToString(),
                                fecha_desistimiento = c.fec_desistimiento.Value.ToString(),
                                financiera = c.icb_unidad_financiera.financiera_nombre,
                                valor_solicitado = c.vsolicitado,
                                valor_aprobado = c.vaprobado != null ? c.vaprobado : 0,
                                numero = c.num_aprobacion ?? "Sin número de aprobación",
                                estado = c.estadoc,
                                motivo = c.motivodesiste.Value.ToString(),
                                plazo = c.plazo.Value.ToString(),
                                asesor = u_asesor.user_nombre + " " +
                                         u_asesor.user_apellido, //asesor = c.users.user_nombre + " " + c.users.user_apellido,
                                cliente = "(" + c.vinfcredito.icb_terceros.doc_tercero + ") " +
                                          c.vinfcredito.icb_terceros.prinom_tercero + " " +
                                          c.vinfcredito.icb_terceros.segnom_tercero + " " +
                                          c.vinfcredito.icb_terceros.apellido_tercero + " " +
                                          c.vinfcredito.icb_terceros.segapellido_tercero,
                                plan = pl.plan_nombre != null ? pl.plan_nombre : "",
                                bodega = c.bodegaid != null ? "(" + bo.bodccs_cod + ") " + bo.bodccs_nombre : "",
                                cuotainicial = c.cuota_inicial,
                                responsable = us.user_nombre + " " + us.user_apellido
                            }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarComparativos(DateTime? fecha1, DateTime? fecha2, string tipo)
        {
            #region tipo1

            if (tipo == "1")
            {
                DateTime fechaFinal = fecha2.Value.AddDays(1);
                decimal porcentaje = 0;
                List<v_creditos> buscarSolicitados = (from a in context.v_creditos
                                                      where a.fec_solicitud >= fecha1 && a.fec_solicitud <= fechaFinal
                                                      select a).ToList();

                int contarSolicitados = buscarSolicitados.Count();
                int contarAprobados = buscarSolicitados.Where(x => x.fec_aprobacion != null).Count();
                if (contarSolicitados > 0)
                {
                    porcentaje = Convert.ToDecimal(contarAprobados, miCultura) / Convert.ToDecimal(contarSolicitados, miCultura) * 100;
                }

                var data = new
                {
                    contarAprobados,
                    contarSolicitados,
                    porcentaje,
                    tipo
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            #endregion

            #region tipo2

            if (tipo == "2")
            {
                DateTime fechaFinal = fecha2.Value.AddDays(1);
                decimal porcentaje = 0;
                var buscaraprobados = (from a in context.v_creditos
                                       join b in context.vpedido
                                           on a.pedido equals b.id
                                       where a.fec_aprobacion >= fecha1 && a.fec_aprobacion <= fechaFinal
                                       select new { a, b }).ToList();

                int contarAprobados = buscaraprobados.Count();
                int contarFacturados = buscaraprobados.Where(x => x.b.facturado).Count();
                if (contarAprobados > 0)
                {
                    porcentaje = Convert.ToDecimal(contarFacturados, miCultura) / Convert.ToDecimal(contarAprobados, miCultura) * 100;
                }

                var data = new
                {
                    contarAprobados,
                    contarFacturados,
                    porcentaje,
                    tipo
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            #endregion

            #region tipo3

            if (tipo == "3")
            {
                DateTime fechaFinal = fecha2.Value.AddDays(1);
                decimal porcentaje = 0;
                var buscaraprobados = (from a in context.v_creditos
                                       where a.fec_aprobacion >= fecha1 && a.fec_aprobacion <= fechaFinal
                                       select new { a }).ToList();

                int contarAprobados = buscaraprobados.Count();
                int contarDesistidos = buscaraprobados.Where(x => x.a.fec_desistimiento != null).Count();
                if (contarAprobados > 0)
                {
                    porcentaje = Convert.ToDecimal(contarDesistidos, miCultura) / Convert.ToDecimal(contarAprobados, miCultura) * 100;
                }

                var data = new
                {
                    contarAprobados,
                    contarDesistidos,
                    porcentaje,
                    tipo
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            #endregion

            #region tipo4

            if (tipo == "4")
            {
                DateTime fechaFinal = fecha2.Value.AddDays(1);
                decimal porcentaje = 0;
                var buscarsolicitados = (from a in context.v_creditos
                                         where a.fec_solicitud >= fecha1 && a.fec_solicitud <= fechaFinal
                                         select new { a }).ToList();

                int contarSolicitados = buscarsolicitados.Count();
                int contarRechazados = buscarsolicitados.Where(x => x.a.fec_negacion != null).Count();
                if (contarSolicitados > 0)
                {
                    porcentaje = Convert.ToDecimal(contarRechazados, miCultura) / Convert.ToDecimal(contarSolicitados, miCultura) * 100;
                }

                var data = new
                {
                    contarSolicitados,
                    contarRechazados,
                    porcentaje,
                    tipo
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            #endregion

            #region tipo5

            if (tipo == "5")
            {
                DateTime fechaFinal = fecha2.Value.AddDays(1);
                decimal porcentaje = 0;
                var buscaraprobados = (from a in context.v_creditos
                                       where a.fec_aprobacion >= fecha1 && a.fec_aprobacion <= fechaFinal
                                       select new { a }).ToList();

                int contarSolicitados = buscaraprobados.Count();
                int contarSincerrar = buscaraprobados.Where(x => x.a.fec_confirmacion == null).Count();
                if (contarSolicitados > 0)
                {
                    porcentaje = Convert.ToDecimal(contarSincerrar, miCultura) / Convert.ToDecimal(contarSolicitados, miCultura) * 100;
                }

                var data = new
                {
                    contarSolicitados,
                    contarSincerrar,
                    porcentaje,
                    tipo
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            #endregion

            #region tipo6

            if (tipo == "6")
            {
                DateTime fechaFinal = fecha2.Value.AddDays(1);
                var financieras = (from c in context.v_creditos
                                   where c.fec_solicitud >= fecha1 && c.fec_solicitud <= fechaFinal
                                   group c by new { c.icb_unidad_financiera }
                    into grupo
                                   select new
                                   {
                                       financiera = grupo.Key.icb_unidad_financiera.financiera_nombre,
                                       id = grupo.Key.icb_unidad_financiera.financiera_id,
                                       cantidad = grupo.Count()
                                   }).OrderByDescending(x => x.cantidad).ToList();
                return Json(new { financieras, tipo = "6" }, JsonRequestBehavior.AllowGet);
            }

            #endregion

            #region tipo7

            if (tipo == "7")
            {
                DateTime fechaFinal = fecha2.Value.AddDays(1);
                var financieras = (from c in context.v_creditos
                                   where c.fec_desembolso >= fecha1 && c.fec_desembolso <= fechaFinal
                                   group c by new { c.icb_unidad_financiera }
                    into grupo
                                   select new
                                   {
                                       financiera = grupo.Key.icb_unidad_financiera.financiera_nombre,
                                       id = grupo.Key.icb_unidad_financiera.financiera_id,
                                       cantidad = grupo.Count()
                                   }).OrderByDescending(x => x.cantidad).ToList();
                return Json(new { financieras, tipo = "7" }, JsonRequestBehavior.AllowGet);
            }

            #endregion

            return Json(0, JsonRequestBehavior.AllowGet);
        }


        public void BuscarFavoritos(int? menu)
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

            var buscarFavoritosSeleccionados = (from favoritos in context.favoritos
                                                join menu2 in context.Menus
                                                    on favoritos.idmenu equals menu2.idMenu
                                                where favoritos.idusuario == usuarioActual && favoritos.seleccionado
                                                select new
                                                {
                                                    favoritos.seleccionado,
                                                    favoritos.cantidad,
                                                    menu2.idMenu,
                                                    menu2.nombreMenu,
                                                    menu2.url
                                                }).OrderByDescending(x => x.cantidad).ToList();

            bool esFavorito = false;

            foreach (var favoritosSeleccionados in buscarFavoritosSeleccionados)
            {
                if (favoritosSeleccionados.idMenu == menu)
                {
                    esFavorito = true;
                    break;
                }
            }

            if (esFavorito)
            {
                ViewBag.Favoritos =
                    "<div id='areaFavoritos'><i class='fa fa-close'></i>&nbsp;&nbsp;<a href='#' onclick='AgregarQuitarFavorito();return false;'>Quitar de Favoritos</a><div>";
            }
            else
            {
                ViewBag.Favoritos =
                    "<div id='areaFavoritos'><i class='fa fa-check'></i>&nbsp;&nbsp;<a href='#' onclick='AgregarQuitarFavorito();return false;'>Agregar a Favoritos</a></div>";
            }

            ViewBag.id_menu = menu != null ? menu : 0;
        }
    }
}