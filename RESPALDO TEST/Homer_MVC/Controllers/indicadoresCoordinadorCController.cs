using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class indicadoresCoordinadorCController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: indicadoresCoordinadorC
        public ActionResult Index()
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
            return View();
        }

        // GET: indicadoresCoordinadorC
        public ActionResult IndexUsados()
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
            return View();
        }

        public JsonResult listarAsesor()
        {
            var listar_asesor = from a in context.users
                                where a.rol_id == 4
                                select new
                                {
                                    a.user_id,
                                    nombre = a.user_nombre + " " + a.user_apellido
                                };
            return Json(listar_asesor, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarDatos(DateTime fechaDesde, DateTime fechaHasta, int? asesor)
        {
            int bodega_actual = Convert.ToInt32(Session["user_bodega"]);

            // todos los filtros
            if (fechaDesde != null && fechaHasta != null && asesor != null)
            {
                var solicitados = (from c in context.v_creditos
                                   where c.fec_solicitud >= fechaDesde && c.fec_solicitud <= fechaHasta
                                                                       && c.asesor_id == asesor
                                                                       && c.vpedido.nuevo
                                   //&& c.bodegaid == bodega_actual
                                   select new
                                   {
                                       fecha_solicitud = c.fec_solicitud.Value.ToString(),
                                       financiera = c.icb_unidad_financiera.financiera_nombre,
                                       valor_solicitado = c.vsolicitado,
                                       estado = c.estadoc,
                                       plazo = c.plazo.Value.ToString(),
                                       pedido = c.vpedido.numero,
                                       asesor = c.users.user_nombre + " " + c.users.user_apellido,
                                       cliente = c.vinfcredito.icb_terceros.prinom_tercero + " " +
                                                 c.vinfcredito.icb_terceros.segnom_tercero + " " +
                                                 c.vinfcredito.icb_terceros.apellido_tercero + " " +
                                                 c.vinfcredito.icb_terceros.segapellido_tercero
                                   }).ToList();

                List<string> estadosx = new List<string>
                {
                    "a",
                    "c"
                };
                var aprobados = (from c in context.v_creditos
                                 join u in context.icb_unidad_financiera
                                     on c.financiera_id equals u.financiera_id
                                 join v in context.vpedido
                                     on c.pedido equals v.id
                                 join s in context.users
                                     on c.userid_creacion equals s.user_id
                                 join i in context.vinfcredito
                                     on c.infocredito_id equals i.id
                                 join t in context.icb_terceros
                                     on i.tercero equals t.tercero_id
                                 where v.nuevo && estadosx.Contains(c.estadoc) && c.bodegaid == 26
                                 select new
                                 {
                                     c.fec_solicitud,
                                     u.financiera_nombre,
                                     c.vsolicitado,
                                     c.estadoc,
                                     c.plazo,
                                     v.numero,
                                     s.user_nombre,
                                     s.user_apellido,
                                     nombre = t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero + " " +
                                              t.segapellido_tercero
                                 }).ToList();

                var demos = (from a in context.agenda_demos
                             join v in context.vdemos
                                 on a.demo_id equals v.id
                             join i in context.icb_referencia
                                 on v.planmayor equals i.ref_codigo
                             join s in context.users
                                 on a.asesor_id equals s.user_id
                             select new
                             {
                                 i.ref_codigo,
                                 i.ref_descripcion,
                                 a.desde,
                                 a.ncliente,
                                 a.nombre,
                                 a.celular,
                                 a.correo,
                                 a.telefono,
                                 asesor = s.user_nombre + " " + s.user_apellido
                             }).ToList();


                var cotizaciones = (from c in context.icb_cotizacion
                                    join a in context.users
                                        on c.asesor equals a.user_id
                                    join d in context.vcotdetallevehiculo
                                        on c.cot_idserial equals d.idcotizacion
                                    where c.cot_feccreacion >= fechaDesde && c.cot_feccreacion <= fechaHasta
                                                                          && c.asesor == asesor
                                                                          && d.nuevo == true
                                    //&& c.bodegaid == bodega_actual
                                    select new
                                    {
                                        numero = c.cot_numcotizacion.ToString(),
                                        fecha = c.cot_feccreacion.ToString(),
                                        asesor = a.user_nombre + " " + a.user_apellido,
                                        cliente = c.icb_terceros.prinom_tercero + " " + c.icb_terceros.segnom_tercero + " " +
                                                  c.icb_terceros.apellido_tercero + " " + c.icb_terceros.segapellido_tercero,
                                        origen = c.tp_origen.tporigen_nombre
                                    }).ToList();


                var prospectos = (from c in context.icb_terceros
                                  where c.tercerofec_creacion >= fechaDesde && c.tercerofec_creacion <= fechaHasta
                                                                            //&& c.bodega == bodega_actual
                                                                            && c.asesor_id == asesor
                                                                            && c.icb_tptramite_prospecto.tptrapros_id == 1
                                                                            && c.prospecto
                                  group c by new { c.tp_origen }
                    into grupo
                                  select new
                                  {
                                      origen = grupo.Key.tp_origen.tporigen_nombre,
                                      id = grupo.Key.tp_origen.tporigen_id,
                                      cantidad = grupo.Count()
                                  }).ToList();

                List<int> cantidadProspectos = (from c in context.icb_terceros
                                                where c.tercerofec_creacion >= fechaDesde && c.tercerofec_creacion <= fechaHasta
                                                                                          //&& c.bodega == bodega_actual
                                                                                          && c.asesor_id == asesor
                                                                                          && c.icb_tptramite_prospecto.tptrapros_id == 1
                                                                                          && c.prospecto
                                                select
                                                    c.tercero_id
                    ).ToList();


                var pedidos = (from c in context.vpedido
                               join cl in context.color_vehiculo
                                   on c.Color_Deseado equals cl.colvh_id into ps
                               from cl in ps.DefaultIfEmpty()
                               where c.fecha >= fechaDesde && c.fecha <= fechaHasta
                                                           //&& c.bodega == bodega_actual
                                                           && c.vendedor == asesor
                                                           && c.nuevo
                               select new
                               {
                                   numero = c.numero.ToString(),
                                   fecha = c.fecha.ToString(),
                                   modelo = c.modelo_vehiculo.modvh_nombre,
                                   anio_modelo = c.id_anio_modelo.Value.ToString(),
                                   color = cl.colvh_nombre,
                                   placa = c.numeroplaca,
                                   plan_venta = c.plan_venta.Value.ToString(),
                                   plan_mayor = c.planmayor,
                                   fecha_plan_mayor = c.fecha_asignacion_planmayor.Value.ToString(),
                                   valor_total = c.vrtotal,
                                   asesor = c.users.user_nombre + " " + c.users.user_apellido,
                                   cliente = c.icb_terceros.prinom_tercero + " " + c.icb_terceros.segnom_tercero + " " +
                                             c.icb_terceros.apellido_tercero + " " + c.icb_terceros.segapellido_tercero
                               }).ToList();

                var facturas = (from f in context.encab_documento
                                join t in context.tp_doc_registros
                                    on f.tipo equals t.tpdoc_id
                                join tr in context.icb_terceros
                                    on f.nit equals tr.tercero_id into ps
                                from tr in ps.DefaultIfEmpty()
                                join fin in context.icb_unidad_financiera
                                    on f.facturado_a equals fin.financiera_id into jf
                                from fin in jf.DefaultIfEmpty()
                                join l in context.lineas_documento
                                    on f.idencabezado equals l.id_encabezado
                                join vh in context.icb_vehiculo
                                    on l.codigo equals vh.plan_mayor
                                where t.sw == 3
                                      && f.fecha >= fechaDesde && f.fecha <= fechaHasta
                                      && f.vendedor == asesor
                                      && vh.nuevo == true
                                //&& f.bodega == bodega_actual
                                select new
                                {
                                    numero = f.numero.ToString(),
                                    tipo = t.tpdoc_nombre,
                                    fecha = f.fecha.ToString(),
                                    tercero = tr.prinom_tercero + " " + tr.segnom_tercero + " " + tr.apellido_tercero + " " +
                                              tr.segapellido_tercero,
                                    financiera = fin.financiera_nombre,
                                    forma_pago = f.fpago_tercero.fpago_nombre,
                                    fecha_vencimiento = f.vencimiento.Value.ToString(),
                                    f.valor_total,
                                    vendedor = f.users.user_nombre + " " + f.users.user_apellido
                                }).ToList();

                var matriculas = from m in context.icb_vehiculo_eventos
                                 where m.id_tpevento == 13
                                       && m.fechaevento >= fechaDesde && m.fechaevento <= fechaHasta
                                       && m.icb_vehiculo.nuevo == true
                                 select new
                                 {
                                     fecha = m.fechaevento.ToString(),
                                     plan_mayor = m.planmayor.ToString()
                                 };

                var data = new
                {
                    solicitados,
                    cotizaciones,
                    aprobados,
                    demos,
                    prospectos,
                    cantidadProspectos,
                    pedidos,
                    facturas,
                    matriculas
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            // filtros solo fechas
            if (fechaDesde != null && fechaHasta != null && asesor == null)
            {
                var solicitados = (from c in context.v_creditos
                                   where c.fec_solicitud >= fechaDesde && c.fec_solicitud <= fechaHasta
                                                                       && c.vpedido.nuevo
                                   //&& c.bodegaid == bodega_actual
                                   select new
                                   {
                                       fecha_solicitud = c.fec_solicitud.Value.ToString(),
                                       financiera = c.icb_unidad_financiera.financiera_nombre,
                                       valor_solicitado = c.vsolicitado,
                                       estado = c.estadoc,
                                       plazo = c.plazo.Value.ToString(),
                                       pedido = c.vpedido.numero,
                                       asesor = c.users.user_nombre + " " + c.users.user_apellido,
                                       cliente = c.vinfcredito.icb_terceros.prinom_tercero + " " +
                                                 c.vinfcredito.icb_terceros.segnom_tercero + " " +
                                                 c.vinfcredito.icb_terceros.apellido_tercero + " " +
                                                 c.vinfcredito.icb_terceros.segapellido_tercero
                                   }).ToList();

                List<string> estadosx = new List<string>
                {
                    "A",
                    "C"
                };
                var aprobados = (from c in context.v_creditos
                                 join u in context.icb_unidad_financiera
                                     on c.financiera_id equals u.financiera_id
                                 join v in context.vpedido
                                     on c.pedido equals v.id
                                 join s in context.users
                                     on c.userid_creacion equals s.user_id
                                 join i in context.vinfcredito
                                     on c.infocredito_id equals i.id
                                 join t in context.icb_terceros
                                     on i.tercero equals t.tercero_id
                                 where v.nuevo && estadosx.Contains(c.estadoc) && c.bodegaid == 26
                                 select new
                                 {
                                     v.numero,
                                     fec_solicitud = c.fec_solicitud.ToString(),
                                     c.vsolicitado,
                                     c.plazo,
                                     u.financiera_nombre,
                                     nombre = t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero + " " +
                                              t.segapellido_tercero,
                                     nombreAsesor = s.user_nombre + " " + s.user_apellido
                                 }).ToList();

                var demos = (from a in context.agenda_demos
                             join v in context.vdemos
                                 on a.demo_id equals v.id
                             join i in context.icb_referencia
                                 on v.planmayor equals i.ref_codigo
                             join s in context.users
                                 on a.asesor_id equals s.user_id
                             where a.desde >= fechaDesde && a.hasta <= fechaHasta
                             select new
                             {
                                 i.ref_codigo,
                                 i.ref_descripcion,
                                 desde = a.desde.ToString(),
                                 a.ncliente,
                                 a.nombre,
                                 a.celular,
                                 a.correo,
                                 a.telefono,
                                 asesor = s.user_nombre + " " + s.user_apellido
                             }).ToList();


                var cotizaciones = (from c in context.icb_cotizacion
                                    join a in context.users
                                        on c.asesor equals a.user_id
                                    join d in context.vcotdetallevehiculo
                                        on c.cot_idserial equals d.idcotizacion
                                    where c.cot_feccreacion >= fechaDesde && c.cot_feccreacion <= fechaHasta
                                                                          && d.nuevo == true
                                    //&& c.bodegaid == bodega_actual
                                    select new
                                    {
                                        numero = c.cot_numcotizacion.ToString(),
                                        fecha = c.cot_feccreacion.ToString(),
                                        asesor = a.user_nombre + " " + a.user_apellido,
                                        cliente = c.icb_terceros.prinom_tercero + " " + c.icb_terceros.segnom_tercero + " " +
                                                  c.icb_terceros.apellido_tercero + " " + c.icb_terceros.segapellido_tercero,
                                        origen = c.tp_origen.tporigen_nombre
                                    }).ToList();


                var prospectos = (from c in context.icb_terceros
                                  where c.tercerofec_creacion >= fechaDesde && c.tercerofec_creacion <= fechaHasta
                                                                            //&& c.bodega == bodega_actual
                                                                            && c.icb_tptramite_prospecto.tptrapros_id == 1
                                                                            && c.prospecto
                                  group c by new { c.tp_origen }
                    into grupo
                                  select new
                                  {
                                      origen = grupo.Key.tp_origen.tporigen_nombre,
                                      id = grupo.Key.tp_origen.tporigen_id,
                                      cantidad = grupo.Count()
                                  }).ToList();

                List<int> cantidadProspectos = (from c in context.icb_terceros
                                                where c.tercerofec_creacion >= fechaDesde && c.tercerofec_creacion <= fechaHasta
                                                                                          //&& c.bodega == bodega_actual
                                                                                          && c.icb_tptramite_prospecto.tptrapros_id == 1
                                                                                          && c.prospecto
                                                select
                                                    c.tercero_id
                    ).ToList();

                var pedidos = (from c in context.vpedido
                               join cl in context.color_vehiculo
                                   on c.Color_Deseado equals cl.colvh_id into ps
                               from cl in ps.DefaultIfEmpty()
                               where c.fecha >= fechaDesde && c.fecha <= fechaHasta
                                                           //&& c.bodega == bodega_actual
                                                           && c.nuevo
                               select new
                               {
                                   numero = c.numero.ToString(),
                                   fecha = c.fecha.ToString(),
                                   modelo = c.modelo_vehiculo.modvh_nombre,
                                   anio_modelo = c.id_anio_modelo.Value.ToString(),
                                   color = cl.colvh_nombre,
                                   placa = c.numeroplaca,
                                   plan_venta = c.plan_venta.Value.ToString(),
                                   plan_mayor = c.planmayor,
                                   fecha_plan_mayor = c.fecha_asignacion_planmayor.Value.ToString(),
                                   valor_total = c.vrtotal,
                                   asesor = c.users.user_nombre + " " + c.users.user_apellido,
                                   cliente = c.icb_terceros.prinom_tercero + " " + c.icb_terceros.segnom_tercero + " " +
                                             c.icb_terceros.apellido_tercero + " " + c.icb_terceros.segapellido_tercero
                               }).ToList();

                var facturas = (from f in context.encab_documento
                                join t in context.tp_doc_registros
                                    on f.tipo equals t.tpdoc_id
                                join tr in context.icb_terceros
                                    on f.nit equals tr.tercero_id into ps
                                from tr in ps.DefaultIfEmpty()
                                join fin in context.icb_unidad_financiera
                                    on f.facturado_a equals fin.financiera_id into jf
                                from fin in jf.DefaultIfEmpty()
                                join l in context.lineas_documento
                                    on f.idencabezado equals l.id_encabezado
                                join vh in context.icb_vehiculo
                                    on l.codigo equals vh.plan_mayor
                                where t.sw == 3
                                      && f.fecha >= fechaDesde && f.fecha <= fechaHasta
                                      && vh.nuevo == true
                                //&& f.bodega == bodega_actual
                                select new
                                {
                                    numero = f.numero.ToString(),
                                    tipo = t.tpdoc_nombre,
                                    fecha = f.fecha.ToString(),
                                    tercero = tr.prinom_tercero + " " + tr.segnom_tercero + " " + tr.apellido_tercero + " " +
                                              tr.segapellido_tercero,
                                    financiera = fin.financiera_nombre,
                                    forma_pago = f.fpago_tercero.fpago_nombre,
                                    fecha_vencimiento = f.vencimiento.Value.ToString(),
                                    f.valor_total,
                                    vendedor = f.users.user_nombre + " " + f.users.user_apellido
                                }).ToList();

                var matriculas = from m in context.icb_vehiculo_eventos
                                 where m.id_tpevento == 13
                                       && m.fechaevento >= fechaDesde && m.fechaevento <= fechaHasta
                                       && m.icb_vehiculo.nuevo == true
                                 select new
                                 {
                                     fecha = m.fechaevento.ToString(),
                                     plan_mayor = m.planmayor.ToString()
                                 };

                var data = new
                {
                    solicitados,
                    aprobados,
                    cotizaciones,
                    demos,
                    cantidadProspectos,
                    prospectos,
                    pedidos,
                    facturas,
                    matriculas
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }


            return Json(JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarDetallesProspectos(DateTime fechaDesde, DateTime fechaHasta, int? asesor, int origen)
        {
            // todos los filtros
            if (fechaDesde != null && fechaHasta != null && asesor != null)
            {
                int bodega_actual = Convert.ToInt32(Session["user_bodega"]);

                var data = (from c in context.icb_terceros
                            where c.tercerofec_creacion >= fechaDesde && c.tercerofec_creacion <= fechaHasta
                                                                      && c.asesor_id == asesor
                                                                      && c.origen_id == origen
                                                                      && c.icb_tptramite_prospecto.tptrapros_id == 1
                                                                      && c.prospecto
                            //&& c.bodega == bodega_actual
                            select new
                            {
                                cliente = c.prinom_tercero + " " + c.segnom_tercero + " " + c.apellido_tercero + " " +
                                          c.segapellido_tercero,
                                razon_social = c.razon_social.ToString(),
                                documento = c.doc_tercero.ToString(),
                                telefono = c.telf_tercero.ToString(),
                                celular = c.celular_tercero.ToString(),
                                correo = c.email_tercero.ToString(),
                                //direccion = c.direc_tercero.ToString(),
                                fecha = c.tercerofec_creacion.ToString(),
                                asesor = c.users.user_nombre + " " + c.users.user_apellido,
                                tramite = c.icb_tptramite_prospecto.tptrapros_descripcion.ToString(),
                                origen = c.tp_origen.tporigen_nombre.ToString()
                            }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            // filtros fechas
            if (fechaDesde != null && fechaHasta != null && asesor == null)
            {
                var data = (from c in context.icb_terceros
                            where c.tercerofec_creacion >= fechaDesde && c.tercerofec_creacion <= fechaHasta
                                                                      && c.origen_id == origen
                                                                      && c.icb_tptramite_prospecto.tptrapros_id == 1
                                                                      && c.prospecto
                            //&& c.bodega == bodega_actual
                            select new
                            {
                                cliente = c.prinom_tercero + " " + c.segnom_tercero + " " + c.apellido_tercero + " " +
                                          c.segapellido_tercero,
                                razon_social = c.razon_social.ToString(),
                                documento = c.doc_tercero.ToString(),
                                telefono = c.telf_tercero.ToString(),
                                celular = c.celular_tercero.ToString(),
                                correo = c.email_tercero.ToString(),
                                //direccion = c.direc_tercero.ToString(),
                                fecha = c.tercerofec_creacion.ToString(),
                                asesor = c.users.user_nombre + " " + c.users.user_apellido,
                                tramite = c.icb_tptramite_prospecto.tptrapros_descripcion.ToString(),
                                origen = c.tp_origen.tporigen_nombre.ToString()
                            }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarDatosUsados(DateTime fechaDesde, DateTime fechaHasta, int? asesor)
        {
            int bodega_actual = Convert.ToInt32(Session["user_bodega"]);

            // todos los filtros
            if (fechaDesde != null && fechaHasta != null && asesor != null)
            {
                var solicitados = (from c in context.v_creditos
                                   where c.fec_solicitud >= fechaDesde && c.fec_solicitud <= fechaHasta
                                                                       && c.asesor_id == asesor
                                                                       && c.vpedido.usado == true
                                   //&& c.bodegaid == bodega_actual
                                   select new
                                   {
                                       fecha_solicitud = c.fec_solicitud.Value.ToString(),
                                       financiera = c.icb_unidad_financiera.financiera_nombre,
                                       valor_solicitado = c.vsolicitado,
                                       estado = c.estadoc,
                                       plazo = c.plazo.Value.ToString(),
                                       pedido = c.vpedido.numero,
                                       asesor = c.users.user_nombre + " " + c.users.user_apellido,
                                       cliente = c.vinfcredito.icb_terceros.prinom_tercero + " " +
                                                 c.vinfcredito.icb_terceros.segnom_tercero + " " +
                                                 c.vinfcredito.icb_terceros.apellido_tercero + " " +
                                                 c.vinfcredito.icb_terceros.segapellido_tercero
                                   }).ToList();


                var cotizaciones = (from c in context.icb_cotizacion
                                    join a in context.users
                                        on c.asesor equals a.user_id
                                    join d in context.vcotdetallevehiculo
                                        on c.cot_idserial equals d.idcotizacion
                                    where c.cot_feccreacion >= fechaDesde && c.cot_feccreacion <= fechaHasta
                                                                          && c.asesor == asesor
                                                                          && d.usado == true
                                    //&& c.bodegaid == bodega_actual
                                    select new
                                    {
                                        numero = c.cot_numcotizacion.ToString(),
                                        fecha = c.cot_feccreacion.ToString(),
                                        asesor = a.user_nombre + " " + a.user_apellido,
                                        cliente = c.icb_terceros.prinom_tercero + " " + c.icb_terceros.segnom_tercero + " " +
                                                  c.icb_terceros.apellido_tercero + " " + c.icb_terceros.segapellido_tercero,
                                        origen = c.tp_origen.tporigen_nombre
                                    }).ToList();


                var prospectos = (from c in context.icb_terceros
                                  where c.tercerofec_creacion >= fechaDesde && c.tercerofec_creacion <= fechaHasta
                                                                            //&& c.bodega == bodega_actual
                                                                            && c.asesor_id == asesor
                                                                            && c.icb_tptramite_prospecto.tptrapros_id == 2
                                                                            && c.prospecto
                                  group c by new { c.tp_origen }
                    into grupo
                                  select new
                                  {
                                      origen = grupo.Key.tp_origen.tporigen_nombre,
                                      id = grupo.Key.tp_origen.tporigen_id,
                                      cantidad = grupo.Count()
                                  }).ToList();


                var pedidos = (from c in context.vpedido
                               join cl in context.color_vehiculo
                                   on c.Color_Deseado equals cl.colvh_id into ps
                               from cl in ps.DefaultIfEmpty()
                               where c.fecha >= fechaDesde && c.fecha <= fechaHasta
                                                           //&& c.bodega == bodega_actual
                                                           && c.vendedor == asesor
                                                           && c.usado == true
                               select new
                               {
                                   numero = c.numero.ToString(),
                                   fecha = c.fecha.ToString(),
                                   modelo = c.modelo_vehiculo.modvh_nombre,
                                   anio_modelo = c.id_anio_modelo.Value.ToString(),
                                   color = cl.colvh_nombre,
                                   placa = c.numeroplaca,
                                   plan_venta = c.plan_venta.Value.ToString(),
                                   plan_mayor = c.planmayor,
                                   fecha_plan_mayor = c.fecha_asignacion_planmayor.Value.ToString(),
                                   valor_total = c.vrtotal,
                                   asesor = c.users.user_nombre + " " + c.users.user_apellido,
                                   cliente = c.icb_terceros.prinom_tercero + " " + c.icb_terceros.segnom_tercero + " " +
                                             c.icb_terceros.apellido_tercero + " " + c.icb_terceros.segapellido_tercero
                               }).ToList();

                var facturas = (from f in context.encab_documento
                                join t in context.tp_doc_registros
                                    on f.tipo equals t.tpdoc_id
                                join tr in context.icb_terceros
                                    on f.nit equals tr.tercero_id into ps
                                from tr in ps.DefaultIfEmpty()
                                join fin in context.icb_unidad_financiera
                                    on f.facturado_a equals fin.financiera_id into jf
                                from fin in jf.DefaultIfEmpty()
                                join l in context.lineas_documento
                                    on f.idencabezado equals l.id_encabezado
                                join vh in context.icb_vehiculo
                                    on l.codigo equals vh.plan_mayor
                                where t.sw == 3
                                      && f.fecha >= fechaDesde && f.fecha <= fechaHasta
                                      && f.vendedor == asesor
                                      && vh.usado == true
                                //&& f.bodega == bodega_actual
                                select new
                                {
                                    numero = f.numero.ToString(),
                                    tipo = t.tpdoc_nombre,
                                    fecha = f.fecha.ToString(),
                                    tercero = tr.prinom_tercero + " " + tr.segnom_tercero + " " + tr.apellido_tercero + " " +
                                              tr.segapellido_tercero,
                                    financiera = fin.financiera_nombre,
                                    forma_pago = f.fpago_tercero.fpago_nombre,
                                    fecha_vencimiento = f.vencimiento.Value.ToString(),
                                    f.valor_total,
                                    vendedor = f.users.user_nombre + " " + f.users.user_apellido
                                }).ToList();

                var matriculas = from m in context.icb_vehiculo_eventos
                                 where m.id_tpevento == 13
                                       && m.fechaevento >= fechaDesde && m.fechaevento <= fechaHasta
                                       && m.icb_vehiculo.usado == true
                                 select new
                                 {
                                     fecha = m.fechaevento.ToString(),
                                     plan_mayor = m.planmayor.ToString()
                                 };

                var data = new
                {
                    solicitados,
                    cotizaciones,
                    prospectos,
                    pedidos,
                    facturas,
                    matriculas
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            // filtros solo fechas
            if (fechaDesde != null && fechaHasta != null && asesor == null)
            {
                var solicitados = (from c in context.v_creditos
                                   where c.fec_solicitud >= fechaDesde && c.fec_solicitud <= fechaHasta
                                                                       && c.vpedido.usado == true
                                   //&& c.bodegaid == bodega_actual
                                   select new
                                   {
                                       fecha_solicitud = c.fec_solicitud.Value.ToString(),
                                       financiera = c.icb_unidad_financiera.financiera_nombre,
                                       valor_solicitado = c.vsolicitado,
                                       estado = c.estadoc,
                                       plazo = c.plazo.Value.ToString(),
                                       pedido = c.vpedido.numero,
                                       asesor = c.users.user_nombre + " " + c.users.user_apellido,
                                       cliente = c.vinfcredito.icb_terceros.prinom_tercero + " " +
                                                 c.vinfcredito.icb_terceros.segnom_tercero + " " +
                                                 c.vinfcredito.icb_terceros.apellido_tercero + " " +
                                                 c.vinfcredito.icb_terceros.segapellido_tercero
                                   }).ToList();


                var cotizaciones = (from c in context.icb_cotizacion
                                    join a in context.users
                                        on c.asesor equals a.user_id
                                    join d in context.vcotdetallevehiculo
                                        on c.cot_idserial equals d.idcotizacion
                                    where c.cot_feccreacion >= fechaDesde && c.cot_feccreacion <= fechaHasta
                                                                          && d.usado == true
                                    //&& c.bodegaid == bodega_actual
                                    select new
                                    {
                                        numero = c.cot_numcotizacion.ToString(),
                                        fecha = c.cot_feccreacion.ToString(),
                                        asesor = a.user_nombre + " " + a.user_apellido,
                                        cliente = c.icb_terceros.prinom_tercero + " " + c.icb_terceros.segnom_tercero + " " +
                                                  c.icb_terceros.apellido_tercero + " " + c.icb_terceros.segapellido_tercero,
                                        origen = c.tp_origen.tporigen_nombre
                                    }).ToList();


                var prospectos = (from c in context.icb_terceros
                                  where c.tercerofec_creacion >= fechaDesde && c.tercerofec_creacion <= fechaHasta
                                                                            //&& c.bodega == bodega_actual
                                                                            && c.tptramite == 2
                                                                            && c.prospecto
                                  group c by new { c.tp_origen }
                    into grupo
                                  select new
                                  {
                                      origen = grupo.Key.tp_origen.tporigen_nombre,
                                      id = grupo.Key.tp_origen.tporigen_id,
                                      cantidad = grupo.Count()
                                  }).ToList();


                var pedidos = (from c in context.vpedido
                               join cl in context.color_vehiculo
                                   on c.Color_Deseado equals cl.colvh_id into ps
                               from cl in ps.DefaultIfEmpty()
                               where c.fecha >= fechaDesde && c.fecha <= fechaHasta
                                                           //&& c.bodega == bodega_actual
                                                           && c.usado == true
                               select new
                               {
                                   numero = c.numero.ToString(),
                                   fecha = c.fecha.ToString(),
                                   modelo = c.modelo_vehiculo.modvh_nombre,
                                   anio_modelo = c.id_anio_modelo.Value.ToString(),
                                   color = cl.colvh_nombre,
                                   placa = c.numeroplaca,
                                   plan_venta = c.plan_venta.Value.ToString(),
                                   plan_mayor = c.planmayor,
                                   fecha_plan_mayor = c.fecha_asignacion_planmayor.Value.ToString(),
                                   valor_total = c.vrtotal,
                                   asesor = c.users.user_nombre + " " + c.users.user_apellido,
                                   cliente = c.icb_terceros.prinom_tercero + " " + c.icb_terceros.segnom_tercero + " " +
                                             c.icb_terceros.apellido_tercero + " " + c.icb_terceros.segapellido_tercero
                               }).ToList();

                var facturas = (from f in context.encab_documento
                                join t in context.tp_doc_registros
                                    on f.tipo equals t.tpdoc_id
                                join tr in context.icb_terceros
                                    on f.nit equals tr.tercero_id into ps
                                from tr in ps.DefaultIfEmpty()
                                join fin in context.icb_unidad_financiera
                                    on f.facturado_a equals fin.financiera_id into jf
                                from fin in jf.DefaultIfEmpty()
                                join l in context.lineas_documento
                                    on f.idencabezado equals l.id_encabezado
                                join vh in context.icb_vehiculo
                                    on l.codigo equals vh.plan_mayor
                                where t.sw == 3
                                      && f.fecha >= fechaDesde && f.fecha <= fechaHasta
                                      && vh.usado == true
                                //&& f.bodega == bodega_actual
                                select new
                                {
                                    numero = f.numero.ToString(),
                                    tipo = t.tpdoc_nombre,
                                    fecha = f.fecha.ToString(),
                                    tercero = tr.prinom_tercero + " " + tr.segnom_tercero + " " + tr.apellido_tercero + " " +
                                              tr.segapellido_tercero,
                                    financiera = fin.financiera_nombre,
                                    forma_pago = f.fpago_tercero.fpago_nombre,
                                    fecha_vencimiento = f.vencimiento.Value.ToString(),
                                    f.valor_total,
                                    vendedor = f.users.user_nombre + " " + f.users.user_apellido
                                }).ToList();

                var matriculas = from m in context.icb_vehiculo_eventos
                                 where m.id_tpevento == 13
                                       && m.fechaevento >= fechaDesde && m.fechaevento <= fechaHasta
                                       && m.icb_vehiculo.usado == true
                                 select new
                                 {
                                     fecha = m.fechaevento.ToString(),
                                     plan_mayor = m.planmayor.ToString()
                                 };

                var data = new
                {
                    solicitados,
                    cotizaciones,
                    prospectos,
                    pedidos,
                    facturas,
                    matriculas
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }


            return Json(JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarDetallesProspectosUsados(DateTime fechaDesde, DateTime fechaHasta, int? asesor,
            int origen)
        {
            // todos los filtros
            if (fechaDesde != null && fechaHasta != null && asesor != null)
            {
                int bodega_actual = Convert.ToInt32(Session["user_bodega"]);

                var data = (from c in context.icb_terceros
                            where c.tercerofec_creacion >= fechaDesde && c.tercerofec_creacion <= fechaHasta
                                                                      && c.asesor_id == asesor
                                                                      && c.origen_id == origen
                                                                      && c.icb_tptramite_prospecto.tptrapros_id == 2
                                                                      && c.prospecto
                            //&& c.bodega == bodega_actual
                            select new
                            {
                                cliente = c.prinom_tercero + " " + c.segnom_tercero + " " + c.apellido_tercero + " " +
                                          c.segapellido_tercero,
                                razon_social = c.razon_social.ToString(),
                                documento = c.doc_tercero.ToString(),
                                telefono = c.telf_tercero.ToString(),
                                celular = c.celular_tercero.ToString(),
                                correo = c.email_tercero.ToString(),
                                //direccion = c.direc_tercero.ToString(),
                                fecha = c.tercerofec_creacion.ToString(),
                                asesor = c.users.user_nombre + " " + c.users.user_apellido,
                                tramite = c.icb_tptramite_prospecto.tptrapros_descripcion.ToString(),
                                origen = c.tp_origen.tporigen_nombre.ToString()
                            }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            // filtros fechas
            if (fechaDesde != null && fechaHasta != null && asesor == null)
            {
                var data = (from c in context.icb_terceros
                            where c.tercerofec_creacion >= fechaDesde && c.tercerofec_creacion <= fechaHasta
                                                                      && c.origen_id == origen
                                                                      && c.icb_tptramite_prospecto.tptrapros_id == 2
                                                                      && c.prospecto
                            //&& c.bodega == bodega_actual
                            select new
                            {
                                cliente = c.prinom_tercero + " " + c.segnom_tercero + " " + c.apellido_tercero + " " +
                                          c.segapellido_tercero,
                                razon_social = c.razon_social.ToString(),
                                documento = c.doc_tercero.ToString(),
                                telefono = c.telf_tercero.ToString(),
                                celular = c.celular_tercero.ToString(),
                                correo = c.email_tercero.ToString(),
                                //direccion = c.direc_tercero.ToString(),
                                fecha = c.tercerofec_creacion.ToString(),
                                asesor = c.users.user_nombre + " " + c.users.user_apellido,
                                tramite = c.icb_tptramite_prospecto.tptrapros_descripcion.ToString(),
                                origen = c.tp_origen.tporigen_nombre.ToString()
                            }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(JsonRequestBehavior.AllowGet);
        }
    }
}