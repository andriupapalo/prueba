using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class boleto_salidaController : Controller
    {
        //lauraa
        private readonly Iceberg_Context db = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");

        // GET: boleto_salida lauraa
        public ActionResult Index()
        {
            var buscarTipoDocumento = (from tipoDocumento in db.tp_doc_registros
                                       where tipoDocumento.tpdoc_id == 4
                                       select new
                                       {
                                           tipoDocumento.tpdoc_id,
                                           nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                           tipoDocumento.tipo
                                       }).ToList();
            //ViewBag.tipo_documentoFiltro = new SelectList(buscarTipoDocumento.Where(x => x.tipo == 1), "tpdoc_id", "nombre");
            ViewBag.tipo_documento = new SelectList(buscarTipoDocumento, "tpdoc_id", "nombre");
            //ViewBag.tipo_documentoDevAuto = new SelectList(buscarTipoDocumento.Where(x => x.tipo == 7), "tpdoc_id", "nombre");
            //   BuscarFavoritos(menu);
            int  rol = Convert.ToInt32(Session["user_rolid"]);
            var Permiso = (from acceso in db.rolacceso
                           join Rolperm  in db.rolpermisos on acceso.idpermiso equals Rolperm.id
                           where Rolperm.codigo =="P36" && acceso.idrol == rol
                           select Rolperm.id);

            ViewBag.Permiso = Permiso != null ? "Si" : "No";

            return View();
        }

        public JsonResult BuscarFacturaciones()
        {
            int rolActual = Convert.ToInt32(Session["user_rolid"]);
            rolacceso buscarSiTodasBodegas = db.rolacceso.FirstOrDefault(x => x.idrol == rolActual && x.idpermiso == 1);
            if (buscarSiTodasBodegas != null)
            {
                var buscarFacturaciones = (from encabezado in db.encab_documento
                                           join tpDoc in db.tp_doc_registros
                                               on encabezado.tipo equals tpDoc.tpdoc_id
                                           join bodega in db.bodega_concesionario
                                               on encabezado.bodega equals bodega.id
                                           join tpDocRegistros in db.tp_doc_registros
                                               on encabezado.tipo equals tpDocRegistros.tpdoc_id
                                           join t in db.icb_terceros
                                               on encabezado.nit equals t.tercero_id
                                           where tpDocRegistros.tipo == 2
                                           select new
                                           {
                                               tpDoc.tpdoc_id,
                                               tpDoc.tpdoc_nombre,
                                               encabezado.numero,
                                               bodega.id,
                                               bodega.bodccs_cod,
                                               bodega.bodccs_nombre,
                                               encabezado.fecha,
                                               encabezado.nit,
                                               encabezado.idencabezado,
                                               t.prinom_tercero,
                                               t.segnom_tercero,
                                               t.apellido_tercero,
                                               t.segapellido_tercero,
                                               t.razon_social,
                                               t.doc_tercero,
                                               encabezado.documento
                                           }).ToList();

                var data = buscarFacturaciones.Select(x => new
                {
                    x.tpdoc_id,
                    x.tpdoc_nombre,
                    x.numero,
                    bodegaId = x.id,
                    x.bodccs_cod,
                    x.bodccs_nombre,
                    fecha = x.fecha.ToString("yyyy/MM/dd"),
                    x.nit,
                    x.idencabezado,
                    cliente = x.razon_social != null
                        ? x.doc_tercero + " - " + x.razon_social
                        : x.doc_tercero + " - " + x.prinom_tercero + " " + x.segnom_tercero + " " + x.apellido_tercero +
                          " " + x.segapellido_tercero,
                    x.documento
                });
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                // Significa que el rol no tiene permiso de ver todas las facturaciones de todas las bodegas, por tantos se busca las bodegas que el usuario actual tiene asignadas
                List<int> lista = new List<int>();
                int usuarioId = Convert.ToInt32(Session["user_usuarioid"]);
                List<bodega_usuario> buscarBodegasUsuario = db.bodega_usuario.Where(x => x.id_usuario == usuarioId).ToList();
                foreach (bodega_usuario item in buscarBodegasUsuario)
                {
                    lista.Add(item.id_bodega);
                }

                var buscarFacturasEnBodegas = (from encabezado in db.encab_documento
                                               join tpDoc in db.tp_doc_registros
                                                   on encabezado.tipo equals tpDoc.tpdoc_id
                                               join bodega in db.bodega_concesionario
                                                   on encabezado.bodega equals bodega.id
                                               join tpDocRegistros in db.tp_doc_registros
                                                   on encabezado.tipo equals tpDocRegistros.tpdoc_id
                                               join t in db.icb_terceros
                                                   on encabezado.nit equals t.tercero_id
                                               where tpDocRegistros.tipo == 2 && lista.Contains(encabezado.bodega)
                                               select new
                                               {
                                                   tpDoc.tpdoc_id,
                                                   tpDoc.tpdoc_nombre,
                                                   encabezado.numero,
                                                   encabezado.fecha,
                                                   bodega.id,
                                                   bodega.bodccs_cod,
                                                   bodega.bodccs_nombre,
                                                   encabezado.nit,
                                                   encabezado.idencabezado,
                                                   t.prinom_tercero,
                                                   t.segnom_tercero,
                                                   t.apellido_tercero,
                                                   t.segapellido_tercero,
                                                   t.razon_social,
                                                   t.doc_tercero,
                                                   encabezado.documento
                                               }).ToList();
                var data = buscarFacturasEnBodegas.Select(x => new
                {
                    x.tpdoc_id,
                    x.tpdoc_nombre,
                    x.numero,
                    bodegaId = x.id,
                    x.bodccs_cod,
                    x.bodccs_nombre,
                    fecha = x.fecha.ToString("yyyy/MM/dd"),
                    x.nit,
                    x.idencabezado,
                    cliente = x.razon_social != null
                        ? x.doc_tercero + " - " + x.razon_social
                        : x.doc_tercero + " - " + x.prinom_tercero + " " + x.segnom_tercero + " " + x.apellido_tercero +
                          " " + x.segapellido_tercero,
                    x.documento
                });
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult generaPdf_boleto(int tipodoc, int? numero, int? bodega, int nit, int idencab)
        {
            #region Empresa

            var empresa = (from e in db.encab_documento
                           join b in db.bodega_concesionario
                               on e.bodega equals b.id
                           join emp in db.tablaempresa
                               on b.concesionarioid equals emp.id
                           where e.idencabezado == idencab
                           select new
                           {
                               emp.nombre_empresa,
                               emp.nit,
                               emp.direccion,
                               emp.estado,
                               emp.telefono
                           }).FirstOrDefault();

            #endregion

            #region Asesor

            int usuario = Convert.ToInt32(Session["user_usuarioid"]);

            var asesor = (from u in db.users
                          where u.user_id == usuario
                          select new
                          {
                              nombre = u.user_nombre + " " + u.user_apellido
                          }).FirstOrDefault();

            #endregion

            #region Cabecera

            var encab = (from enc in db.encab_documento
                         where enc.idencabezado == idencab
                         select new
                         {
                             enc.numero,
                             enc.fecha,
                             enc.vencimiento,
                             enc.valor_total,
                             enc.valor_aplicado,
                             enc.documento
                         }).FirstOrDefault();

            #endregion

            #region Cliente

            var tercero = (from ter in db.icb_terceros
                           where ter.tercero_id == nit
                           select new
                           {
                               nombreCliente = ter.tpdoc_id == 2
                                   ? "(" + ter.doc_tercero + ") " + ter.prinom_tercero + " " + ter.segnom_tercero + " " +
                                     ter.apellido_tercero + " " + ter.segapellido_tercero
                                   : "(" + ter.doc_tercero + ") " + ter.razon_social,
                               ter.telf_tercero,
                               ter.celular_tercero,
                               ter.email_tercero
                           }).FirstOrDefault();
            var dircliente = (from te in db.terceros_direcciones
                              where te.idtercero == nit
                              select new
                              {
                                  te.direccion
                              }).ToList();
            string dircli = dircliente.Max(c => c.direccion);

            #endregion

            var abonos = (from a in db.encab_documento
                          join b in db.tp_doc_registros
                              on a.tipo equals b.tpdoc_id
                          join c in db.cruce_documentos
                              on new { x1 = a.tipo, x2 = a.numero } equals new { x1 = c.idtipoaplica, x2 = c.numeroaplica } into cs
                          from c in cs.DefaultIfEmpty()
                          join d in db.tp_doc_registros
                              on c.idtipo equals d.tpdoc_id
                          where a.nit == nit && a.idencabezado == idencab
                          select new
                          {
                              a.nit,
                              tdoc = "(" + d.prefijo + ") " + d.tpdoc_nombre,
                              a.numero,
                              c.numeroaplica,
                              a.tipo,
                              c.idtipoaplica,
                              c.fecha,
                              c.fechacruce,
                              c.valor,
                              a.valor_aplicado,
                              a.valor_total
                          }).ToList();
            List<AbonosBoleto> listaAbonos = abonos.Select(c => new AbonosBoleto
            {
                descripcion_abono = c.tdoc,
                monto_abono = c.valor.ToString("N2",miCultura),
                monto_abonodeci=c.valor

                //descripcion_accesorio = c.referencia,
                //cant_acccesorio = c.cantidad != null ? c.cantidad.Value.ToString("N0") : "",
                //val_unitario_accesorio = c.vrunitario != null ? c.vrunitario.Value.ToString("N2") : "",
                //val_monto_accesorio = c.vrtotal != null ? c.vrtotal.Value.ToString("N2") : ""
            }).ToList();

            var infopedido = (from enc in db.encab_documento
                              join ped in db.vpedido
                                  on enc.id_pedido_vehiculo equals ped.id
                              join vehm in db.modelo_vehiculo
                                  on ped.modelo equals vehm.modvh_codigo
                              join vehi in db.icb_vehiculo
                                  on ped.planmayor equals vehi.plan_mayor into ep
                              from vehi in ep.DefaultIfEmpty()
                              where enc.idencabezado == idencab
                              select new
                              {
                                  ped.cargomatricula,
                                  ped.planmayor,
                                  ped.valorPoliza,
                                  ped.valormatricula,
                                  vehm.modvh_nombre,
                                  vehi.plac_vh,
                                  vehi.vin,
                              }).FirstOrDefault();

            var infpago = (from enc in db.encab_documento
                           join pedfp in db.vpedpago
                               on enc.id_pedido_vehiculo equals pedfp.idpedido into fp
                           from pedfp in fp.DefaultIfEmpty()
                           where enc.idencabezado == idencab && pedfp.condicion == 9
                           select new
                           {
                               pedfp.valor
                           }).FirstOrDefault();

            #region Lista de Accesorios

            var data2 = (from enc2 in db.encab_documento
                         join ped2 in db.vpedido
                             on enc2.id_pedido_vehiculo equals ped2.id
                         join acce in db.vpedrepuestos
                             on ped2.id equals acce.pedido_id
                         where enc2.tipo == 4 && enc2.idencabezado == idencab
                         select new
                         {
                             acce.numfactura,
                             acce.referencia,
                             acce.cantidad,
                             acce.vrunitario,
                             acce.vrtotal
                         }).ToList();
            List<AccesoriosBoleto> listaAccesorio = data2.Select(c => new AccesoriosBoleto
            {
                id_accesorio_modelo = c.numfactura,
                descripcion_accesorio = c.referencia,
                cant_acccesorio = c.cantidad != null ? c.cantidad.Value.ToString("N0") : "0",
                val_unitario_accesorio = c.vrunitario != null ? c.vrunitario.Value.ToString("N2") : "0",
                val_monto_accesorio = c.vrtotal != null ? c.vrtotal.Value.ToString("N2") : "0",
                val_monto_accesoriodeci = c.vrtotal != null ? c.vrtotal.Value:0,
            }).ToList();

            #endregion

            var financiera = (from enc2 in db.encab_documento
                              join ped2 in db.vpedpago
                                  on enc2.id_pedido_vehiculo equals ped2.idpedido
                              join fin in db.icb_unidad_financiera
                                  on ped2.banco equals fin.financiera_id
                              where ped2.condicion == 4 && enc2.idencabezado == idencab
                              select new
                              {
                                  ped2.valor,
                                  fin.financiera_nombre
                              }).FirstOrDefault();

            var retoma = (from enc2 in db.encab_documento
                          join reto in db.vpedretoma
                              on enc2.id_pedido_vehiculo equals reto.pedido_id
                          where enc2.idencabezado == idencab
                          select new
                          {
                              reto.valor,
                              retoma = "(" + reto.placa + ") " + reto.modelo + " (" + reto.kilometraje + ")"
                          }).FirstOrDefault();

            string root = Server.MapPath("~/Pdf/");
            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
            string path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);
            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");


            string finan = financiera != null
                ? financiera.financiera_nombre != null ? financiera.financiera_nombre : ""
                : "";
            decimal? finanmonto = financiera != null ? financiera.valor != null ? financiera.valor : 0 : 0;

            string nomretom = retoma != null ? retoma.retoma != null ? retoma.retoma : "" : "";
            decimal? valretoma = retoma != null ? retoma.valor != null ? retoma.valor : 0 : 0;

            decimal? valipoliza = infopedido != null ? infopedido.valorPoliza != null ? infopedido.valorPoliza : 0 : 0;

            decimal? valimatricula =
                infopedido != null ? infopedido.valormatricula != null ? infopedido.valormatricula : 0 : 0;

            string modelo = infopedido != null ? infopedido.modvh_nombre != null ? infopedido.modvh_nombre : "" : "";

            string placa = infopedido != null ? infopedido.plac_vh != null ? infopedido.plac_vh : "" : "";

            string vin = infopedido != null ? infopedido.vin != null ? infopedido.vin : "" : "";

            decimal? valCuotaIni = infpago != null ? infpago.valor != null ? infpago.valor : 0 : 0;
            decimal sumaAccesorios = 0;
            if (listaAccesorio.Count > 0)
            {
                sumaAccesorios =
                   listaAccesorio.Sum(vb => vb.val_monto_accesoriodeci)> 0
                       ? listaAccesorio.Sum(vb => vb.val_monto_accesoriodeci)
                       : 0;
            }
            

            decimal SumaAbono = 0;
            if (listaAbonos.Count > 0)
            {
                SumaAbono =listaAbonos.Sum(vb =>vb.monto_abonodeci) > 0
                ? listaAbonos.Sum(vb => vb.monto_abonodeci)
                : 0;
            }

            //var tcargos = (encab.valor_total + infopedido.valormatricula + infopedido.valorPoliza + Convert.ToDecimal(listaAccesorio.Sum(vb => (Convert.ToDecimal(vb.val_monto_accesorio)))));
            decimal? tcargos = encab.valor_total + valimatricula + valipoliza + sumaAccesorios;
            //var tabonos = (infopedido.valor + finanmonto + valretoma + Convert.ToDecimal(listaAbonos.Sum(vb => (Convert.ToDecimal(vb.monto_abono)))));
            //var tabonos = (valCuotaIni + finanmonto + valretoma + SumaAbono);
            decimal tabonos = SumaAbono;
            decimal? saldopediente = tcargos - tabonos;
            decimal? devolucion = tabonos > tcargos ? tabonos - tcargos : 0;




            icb_sysparameter paranc = db.icb_sysparameter.Where(d => d.syspar_cod == "P125").FirstOrDefault();
            int idNc = paranc != null ? Convert.ToInt32(paranc.syspar_value) : 16;
            icb_sysparameter pararc = db.icb_sysparameter.Where(d => d.syspar_cod == "P126").FirstOrDefault();
            int idRc = pararc != null ? Convert.ToInt32(pararc.syspar_value) : 20;
            icb_sysparameter swND = db.icb_sysparameter.Where(d => d.syspar_cod == "P128").FirstOrDefault();
            int swND2 = swND != null ? Convert.ToInt32(swND.syspar_value) : 1013;
            icb_sysparameter swF = db.icb_sysparameter.Where(d => d.syspar_cod == "P129").FirstOrDefault();
            int swF2 = swF != null ? Convert.ToInt32(swF.syspar_value) : 3;



            PDF_ModeloBoletosalida obj = new PDF_ModeloBoletosalida
            {
                nombreEmpre = empresa.nombre_empresa,
                nitEmpre = empresa.nit,
                dirEmpre = empresa.direccion,
                titulo = "Boleta de Salida",
                fecha = DateTime.Now.ToString("yyyy/MM/dd"),
                factura = encab.numero.ToString(),
                cliente = tercero.nombreCliente,
                asesor = asesor.nombre,
                vehiculo_frand = encab.documento,
                vehiculo_valor = encab.valor_total.ToString("N2",miCultura),
                accesoriosboleto = listaAccesorio,
                veh_modelo = modelo,
                placa = placa,
                serVeh = vin,
                matricula_frand = infopedido.cargomatricula != null
                    ? infopedido.cargomatricula.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "",
                matricula_valor = infopedido.valormatricula != null
                    ? infopedido.valormatricula.Value.ToString("N2", new CultureInfo("is-IS"))
                    : "",
                //soat_frand = 
                //soat_valor =
                poliza_frand = "",
                poliza_valor = infopedido.valorPoliza != null
                    ? infopedido.valorPoliza.Value.ToString("N2", new CultureInfo("is-IS"))
                    : "",

                abonosboleto = listaAbonos,

                cuota_inicial_valor =
                    valCuotaIni != null ? valCuotaIni.Value.ToString("N2", new CultureInfo("is-IS")) : "",

                financiera = finan,
                finanaciera_valor = finanmonto.Value.ToString("N2", new CultureInfo("is-IS")),

                retoma = nomretom,
                retoma_valor = valretoma.Value.ToString("N2", new CultureInfo("is-IS")),


                //finanaciera_valor =      

                total_cargos = tcargos.Value.ToString("N2", new CultureInfo("is-IS")),
                total_abonos = tabonos.ToString("N2", new CultureInfo("is-IS")),


                saldo_pendiente = saldopediente.Value.ToString("N2", new CultureInfo("is-IS")),
                saldo_devolucion = devolucion.Value.ToString("N2", new CultureInfo("is-IS"))
            };


            ViewAsPdf something = new ViewAsPdf("generaPdf_boleto", obj);
            return something;
            // return View(); // solo mientras esta en espera 
        }

        //var data2 = (from df in context.lineas_documento
        //             join re in context.icb_referencia
        //             on df.codigo equals re.ref_codigo
        //             where df.id_encabezado == id
        //             select new
        //             {
        //                 id_Factura = df.id_encabezado,
        //                 id_detalleFactura = df.id,
        //                 referenciaFactura = df.codigo,
        //                 descripcionFactura = re.ref_descripcion,
        //                 cantFactura = df.cantidad,
        //                 pordescuentoFactura = df.porcentaje_descuento,
        //                 porivaFactura = df.porcentaje_iva,
        //                 preciounitarioFactura = df.valor_unitario,
        //                 valorFactura = df.valor_unitario * df.cantidad,
        //             }).ToList();

        //var detalleFacturacion001 = data2.Select(c => new detalleFacturacion
        //{
        //    id_Factura = c.id_Factura,
        //    id_detalleFactura = c.id_detalleFactura,
        //    referenciaFactura = c.referenciaFactura,
        //    descripcionFactura = c.descripcionFactura,
        //    cantFactura = c.cantFactura.ToString("N0"),
        //    pordescuentoFactura = c.pordescuentoFactura.Value.ToString("N0"),
        //    porivaFactura = c.porivaFactura.Value.ToString("N0"),
        //    // preciounitarioFactura = c.preciounitarioFactura.ToString("N0"),
        //    preciounitarioFactura = (c.preciounitarioFactura / ((100 - Convert.ToDecimal(c.pordescuentoFactura)) / 100)).ToString("N0"),
        //    valorFactura = (c.cantFactura * (c.preciounitarioFactura / ((100 - Convert.ToDecimal(c.pordescuentoFactura)) / 100))).ToString("N0"),
        //    montoivaFactura = ((c.cantFactura * (c.preciounitarioFactura / ((100 - Convert.ToDecimal(c.pordescuentoFactura)) / 100))) * (Convert.ToDecimal(c.porivaFactura) / 100)).ToString("N0"),
        //}).ToList();
    }
}