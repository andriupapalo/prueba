using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Homer_MVC.IcebergModel;
using System.IO;
using System.Globalization;
using System.Net.Mail;
using System.Data.Entity.Validation;
using Homer_MVC.Models;
using System.Data.SqlClient;
using Homer_MVC.ViewModels.medios;

namespace Homer_MVC.Controllers
{
    public class vpedidosController : Controller
    {
        // laura 2056
		//jairo 001
        //Aca esta el comentario del 22 04 19 1:23 pm

        private Iceberg_Context db = new Iceberg_Context();
        CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
        private int idDocOtBodega = 0;
        private string codigoIOT = "";
        private int iIdOperacion = 0;
        private int iCreateOt = 0;
        private int iExecutionOt = 0;
        private int iFinishOt = 0;
        private int idtpbahia_alis = 0;
        private int bahia = 0;
        private int bodegaActual = 0;
        private int iEnvAlis = 0;
        private int iFinAlis = 0;
        private int iEnvRAlis = 0;
        private int iFinRAlis = 0;

        public string NumeroEnLetras(string num)
        {
            string res, dec = "";
            Int64 entero;
            int decimales;
            double nro;

            try

            {
                nro = Convert.ToDouble(num);
            }
            catch
            {
                return "";
            }

            entero = Convert.ToInt64(Math.Truncate(nro));
            decimales = Convert.ToInt32(Math.Round((nro - entero) * 100, 2));
            if (decimales > 0)
            {
                dec = " CON " + decimales.ToString() + "/100";
            }

            res = toText(Convert.ToDouble(entero)) + dec;
            return res;
        }
        public JsonResult buscarCreditosmodal(string cedula)
        {

            var buscar = db.icb_terceros.FirstOrDefault(x => x.doc_tercero == cedula).tercero_id;

            var buscar_creditos = db.vinfcredito.Where(x => x.tercero == buscar).Select(x => new
            {
                estado = x.v_creditos.Select(a => a.estadoc).FirstOrDefault(),
                valor = x.v_creditos.Select(b => b.vsolicitado).FirstOrDefault(),
                aprobado = x.v_creditos.Select(c => c.vaprobado).FirstOrDefault()
            }).ToList();
            return Json(buscar_creditos, JsonRequestBehavior.AllowGet);
        }


        private string toText(double value)
        {
            string Num2Text = "";
            value = Math.Truncate(value);
            if (value == 0) Num2Text = "CERO";
            else if (value == 1) Num2Text = "UNO";
            else if (value == 2) Num2Text = "DOS";
            else if (value == 3) Num2Text = "TRES";
            else if (value == 4) Num2Text = "CUATRO";
            else if (value == 5) Num2Text = "CINCO";
            else if (value == 6) Num2Text = "SEIS";
            else if (value == 7) Num2Text = "SIETE";
            else if (value == 8) Num2Text = "OCHO";
            else if (value == 9) Num2Text = "NUEVE";
            else if (value == 10) Num2Text = "DIEZ";
            else if (value == 11) Num2Text = "ONCE";
            else if (value == 12) Num2Text = "DOCE";
            else if (value == 13) Num2Text = "TRECE";
            else if (value == 14) Num2Text = "CATORCE";
            else if (value == 15) Num2Text = "QUINCE";
            else if (value < 20) Num2Text = "DIECI" + toText(value - 10);
            else if (value == 20) Num2Text = "VEINTE";
            else if (value < 30) Num2Text = "VEINTI" + toText(value - 20);
            else if (value == 30) Num2Text = "TREINTA";
            else if (value == 40) Num2Text = "CUARENTA";
            else if (value == 50) Num2Text = "CINCUENTA";
            else if (value == 60) Num2Text = "SESENTA";
            else if (value == 70) Num2Text = "SETENTA";
            else if (value == 80) Num2Text = "OCHENTA";
            else if (value == 90) Num2Text = "NOVENTA";
            else if (value < 100) Num2Text = toText(Math.Truncate(value / 10) * 10) + " Y " + toText(value % 10);
            else if (value == 100) Num2Text = "CIEN";
            else if (value < 200) Num2Text = "CIENTO " + toText(value - 100);
            else if ((value == 200) || (value == 300) || (value == 400) || (value == 600) || (value == 800)) Num2Text = toText(Math.Truncate(value / 100)) + "CIENTOS";
            else if (value == 500) Num2Text = "QUINIENTOS";
            else if (value == 700) Num2Text = "SETECIENTOS";
            else if (value == 900) Num2Text = "NOVECIENTOS";
            else if (value < 1000) Num2Text = toText(Math.Truncate(value / 100) * 100) + " " + toText(value % 100);
            else if (value == 1000) Num2Text = "MIL";
            else if (value < 2000) Num2Text = "MIL " + toText(value % 1000);
            else if (value < 1000000)
            {
                Num2Text = toText(Math.Truncate(value / 1000)) + " MIL";
                if ((value % 1000) > 0) Num2Text = Num2Text + " " + toText(value % 1000);
            }

            else if (value == 1000000) Num2Text = "UN MILLON";
            else if (value < 2000000) Num2Text = "UN MILLON " + toText(value % 1000000);
            else if (value < 1000000000000)
            {
                Num2Text = toText(Math.Truncate(value / 1000000)) + " MILLONES ";
                if ((value - Math.Truncate(value / 1000000) * 1000000) > 0) Num2Text = Num2Text + " " + toText(value - Math.Truncate(value / 1000000) * 1000000);
            }

            else if (value == 1000000000000) Num2Text = "UN BILLON";
            else if (value < 2000000000000) Num2Text = "UN BILLON " + toText(value - Math.Truncate(value / 1000000000000) * 1000000000000);

            else
            {
                Num2Text = toText(Math.Truncate(value / 1000000000000)) + " BILLONES";
                if ((value - Math.Truncate(value / 1000000000000) * 1000000000000) > 0) Num2Text = Num2Text + " " + toText(value - Math.Truncate(value / 1000000000000) * 1000000000000);
            }
            return Num2Text;
        }

        public JsonResult BuscarNitsFacturaProforma()
        {
            var buscarNits = db.icb_terceros.Where(x => x.tpdoc_id == 1).Select(x => new
            {
                razon_social = x.razon_social != null ? x.doc_tercero + " - " + x.razon_social : x.doc_tercero + " - " + x.prinom_tercero + " - " + x.segnom_tercero + " " + x.apellido_tercero + " " + x.segapellido_tercero,
                x.tercero_id
            }).ToList();
            return Json(buscarNits, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDatosNit(int id)
        {
            var buscarTercero = (from tercero in db.icb_terceros
                                 join ciudad in db.nom_ciudad
                                 on tercero.ciu_id equals ciudad.ciu_id into temp
                                 from j in temp.DefaultIfEmpty()
                                 where tercero.tercero_id == id
                                 select new { /*tercero.direc_tercero,*/ j.ciu_nombre }).FirstOrDefault();
            return Json(buscarTercero, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FacturaProforma(string info)//int? id,int? idFacPro, int dirigido, string direccion, string ciudad, string observacion, decimal valorSolicitado
        {
            if (info != null)
            {
                string[] array;
                var id = 0;
                var idFacPro = 0;
                var dirigido = 0;
                var direccion = "";
                var ciudad = "";
                var observacion = "";
                decimal valorSolicitado = 0;

                array = info.Split(',');
                for (int i = 0; i < array.Count(); i++)
                {
                    id = Convert.ToInt32(array[0]);
                    idFacPro = !string.IsNullOrWhiteSpace(array[1]) ? 0 : Convert.ToInt32(array[1]);
                    dirigido = Convert.ToInt32(array[2]);
                    direccion = Convert.ToString(array[3]);
                    ciudad = Convert.ToString(array[4]);
                    observacion = Convert.ToString(array[5]);
                    valorSolicitado = Convert.ToDecimal(array[6]);
                }

                var direcOK = "";
                for (int i = 0; i < direccion.Count(); i++)
                {
                    direcOK = direccion.Replace("|N|", "#");
                }
                var obsOK = "";
                for (int i = 0; i < observacion.Count(); i++)
                {
                    obsOK = observacion.Replace("|N|", "#");
                }

                var t = db.icb_terceros.FirstOrDefault(x => x.tercero_id == dirigido);
                var buscardirigido = t.razon_social != null ? t.doc_tercero + " - " + t.razon_social : t.doc_tercero + " - " + t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero + " " + t.segapellido_tercero;
                var dirigidoA = buscardirigido;

                var buscar = db.facturaProforma.Where(x => x.id == idFacPro).FirstOrDefault();
                if (buscar == null)
                {
                    facturaProforma facpro = new facturaProforma();
                    facpro.idPedido = Convert.ToInt32(id);
                    facpro.direccion = direcOK;
                    facpro.ciudad = ciudad;
                    facpro.valorSolicitado = valorSolicitado;
                    facpro.observaciones = observacion;
                    facpro.dirigido = dirigido;
                    facpro.fecha_creacion = DateTime.Now;
                    facpro.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    db.facturaProforma.Add(facpro);
                    db.SaveChanges();
                }

                var buscaPedido = (from pedido in db.vpedido
                                   join tercero in db.icb_terceros
                                   on pedido.nit equals tercero.tercero_id
                                   join modeloVh in db.modelo_vehiculo
                                   on pedido.modelo equals modeloVh.modvh_codigo
                                   join marca in db.marca_vehiculo
                                   on modeloVh.mar_vh_id equals marca.marcvh_id
                                   join ciudadCliente in db.nom_ciudad
                                   on tercero.ciu_id equals ciudadCliente.ciu_id into temp
                                   from j in temp.DefaultIfEmpty()
                                   join vehiculo in db.icb_vehiculo
                                   on pedido.planmayor equals vehiculo.plan_mayor into temp2
                                   from k in temp2.DefaultIfEmpty()
                                   join bodegaCss in db.bodega_concesionario
                                   on pedido.bodega equals bodegaCss.id into temp3
                                   from m in temp3.DefaultIfEmpty()
                                   where pedido.id == id
                                   select new
                                   {
                                       tercero.prinom_tercero,
                                       tercero.segnom_tercero,
                                       tercero.apellido_tercero,
                                       tercero.segapellido_tercero,
                                       tercero.doc_tercero,
                                       //tercero.direc_tercero,
                                       modeloVh.modvh_nombre,
                                       marca.marcvh_nombre,
                                       j.ciu_nombre,
                                       pedido.vrtotal,
                                       pedido.bodega,
                                       pedido.numero,
                                       pedido.numeroplaca,
                                       k.vin,
                                       m.bodccs_cod
                                   }).FirstOrDefault();

                var buscarDireccion = (from pedido in db.vpedido
                                       join tercero in db.icb_terceros
                                       on pedido.nit equals tercero.tercero_id
                                       join direcciones in db.terceros_direcciones
                                       on tercero.tercero_id equals direcciones.idtercero
                                       orderby direcciones.id descending
                                       where pedido.id == id
                                       select new { direcciones.direccion }).FirstOrDefault();

                var root = Server.MapPath("~/Pdf/");
                var pdfname = String.Format("{0}.pdf", Guid.NewGuid().ToString());
                var path = Path.Combine(root, pdfname);
                path = Path.GetFullPath(path);

                var total = valorSolicitado;
                CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
                var formatoNumericoVrTotal = total.ToString("0,0", elGR);
                FacturaProformaModel modelo = new FacturaProformaModel()
                {
                    cliente = buscaPedido.prinom_tercero + " " + buscaPedido.segnom_tercero + " " + buscaPedido.apellido_tercero + " " + buscaPedido.segapellido_tercero,
                    cedula = buscaPedido.doc_tercero,
                    direccionCliente = buscarDireccion.direccion,
                    estilo = buscaPedido.modvh_nombre,
                    marca = buscaPedido.marcvh_nombre,
                    ciudadCliente = buscaPedido.ciu_nombre,
                    valor = formatoNumericoVrTotal,
                    valorLetras = NumeroEnLetras(Convert.ToString(total)),
                    senores = dirigidoA,
                    direccionSenores = direcOK,
                    ciudadSenores = ciudad,
                    bodega = buscaPedido.bodccs_cod,
                    numeroPedido = buscaPedido.numero != null ? buscaPedido.numero.ToString() : "",
                    placa = buscaPedido.numeroplaca,
                    serie = buscaPedido.vin,
                    observaciones = obsOK
                };

                var something = new Rotativa.ViewAsPdf("FacturaProforma", modelo) { /*FileName = "cv.pdf" SaveOnServerPath = path*/ };
                return something;
            }
            else
            {
                return View();
            }
        }

        public JsonResult buscarFacturaPRO(int id)
        {
            var data = (from f in db.facturaProforma
                        join p in db.vpedido
                        on f.idPedido equals p.id
                        join t in db.icb_terceros
                        on p.nit equals t.tercero_id
                        where f.id == id
                        select new
                        {
                            f.idPedido,
                            f.dirigido,
                            f.direccion,
                            f.ciudad,
                            f.observaciones,
                            f.valorSolicitado
                        }).FirstOrDefault();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarHistoricoFacPRo(DateTime? desde, DateTime? hasta)
        {
            if (desde == null)
            {
                desde = DateTime.Now.AddMonths(-1);
            }

            if (hasta == null)
            {
                hasta = DateTime.Now.AddDays(1);
            }
            else
            {
                hasta = hasta.GetValueOrDefault().AddDays(1);
            }

            var data = (from h in db.vw_historicoFacturaProforma
                        where h.fecha_creacion >= desde && h.fecha_creacion <= hasta
                        select new
                        {
                            fechaCompra = h.fec.Value.ToString(),
                            h.plan_mayor,
                            placa = h.plac_vh != null ? h.plac_vh : h.plan_mayor,
                            h.modelo,
                            h.vin,
                            h.modvh_nombre,
                            h.colvh_nombre,
                            h.anio,
                            fecFacPro = h.fecha_creacion != null ? h.fecha_creacion.ToString() : "",
                            nFacPro = h.id,
                            h.valorSolicitado,
                            fecFacVenta = h.fec_creacion != null ? h.fec_creacion.Value.ToString() : "",
                            nFacVenta = h.numero != null ? h.numero.ToString() : "",
                            h.doc_tercero,
                            cliente = h.razon_social != null ? h.razon_social : h.prinom_tercero + " " + h.segnom_tercero + " " + h.apellido_tercero + " " + h.segapellido_tercero
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ContratoCompraventa(int? id)
        {
            if (id != null)
            {

            var buscaPedido = (from pedido in db.vpedido
                               join anioM in db.anio_modelo
                               on pedido.id_anio_modelo equals anioM.anio_modelo_id
                               join v_credito in db.v_creditos
                               on pedido.id equals v_credito.pedido into cred
                               from cr in cred.DefaultIfEmpty()
                               join ciudadCliente in db.nom_ciudad
                               on pedido.icb_terceros.ciu_id equals ciudadCliente.ciu_id into temp
                               from j in temp.DefaultIfEmpty()
                               join ciudad in db.nom_ciudad
                               on pedido.bodega_concesionario.ciudad_id equals ciudad.ciu_id into temp4
                               from n in temp4.DefaultIfEmpty()
                               join departamento in db.nom_departamento
                               on n.dpto_id equals departamento.dpto_id into temp5
                               from b in temp5.DefaultIfEmpty()
                               join color in db.color_vehiculo
                               on pedido.Color_Deseado equals color.colvh_id into temp6
                               from c in temp6.DefaultIfEmpty()
                               join servicio in db.tpservicio_vehiculo
                               on pedido.servicio equals servicio.tpserv_id into temp7
                               from s in temp7.DefaultIfEmpty()
                               join segmento in db.segmento_vehiculo
                               on pedido.modelo_vehiculo.seg_vh_id equals segmento.segvh_id into temp8
                               from seg in temp8.DefaultIfEmpty()
                               join cliente in db.tercero_cliente
                               on pedido.icb_terceros.tercero_id equals cliente.tercero_id into temp9
                               from cli in temp9.DefaultIfEmpty()
                               join ocupacion in db.tp_ocupacion
                               on cli.tpocupacion_id equals ocupacion.tpocupacion_id into temp10
                               from ocu in temp10.DefaultIfEmpty()
                               join estadoCivil in db.estado_civil
                               on cli.edocivil_id equals estadoCivil.edocivil_id into temp11
                               from edoCivil in temp11.DefaultIfEmpty()
                               join usuario in db.users
                               on pedido.vendedor equals usuario.user_id into temp12
                               from vend in temp12.DefaultIfEmpty()

                               where pedido.id == id     
                               select new
                               {
                                   pedido.icb_terceros.prinom_tercero,
                                   pedido.icb_terceros.segnom_tercero,
                                   pedido.icb_terceros.apellido_tercero,
                                   pedido.icb_terceros.segapellido_tercero,
                                   pedido.icb_terceros.doc_tercero,
                                   //tercero.direc_tercero,
                                   pedido.icb_terceros.telf_tercero,
                                   pedido.icb_terceros.celular_tercero,
                                   pedido.icb_terceros.fec_nacimiento,
                                   pedido.icb_terceros.email_tercero,
                                   pedido.modelo_vehiculo.modvh_nombre,

                                   c.colvh_nombre,
                                   plac_vh = (pedido.planmayor != null && pedido.planmayor != string.Empty) ? pedido.icb_vehiculo.plac_vh : "",
                                   pedido.modelo_vehiculo.marca_vehiculo.marcvh_nombre,
                                   j.ciu_nombre,
                                   pedido.vrtotal,
                                   anioM.anio,
                                   // . caso 2256
                                   cr.fec_aprobacion,
                                   cr.fec_desembolso,
                                   cr.poliza,
                                   cr.plazo,
                                   cr.vaprobado,
                                   cr.cuota_inicial,
                                   // fin caso 2256
                                   pedido.bodega,
                                   pedido.numero,
                                   pedido.vrdescuento,
                                   pedido.numeroplaca,
                                   pedido.fecha,
                                   pedido.planmayor,
                                   //caso 2056
                                   pedido.nit_asegurado,
                                    pedido.nit2,
                                    pedido.nit3,
                                    pedido.nit4,
                                    pedido.nit5,
                                    //fin caso 2056
                                   s.tpserv_nombre,
                                   seg.segvh_nombre,
                                   vin = (pedido.planmayor != null && pedido.planmayor != string.Empty) ? pedido.icb_vehiculo.vin : "",
                                   pedido.bodega_concesionario.bodccs_cod,
                                   pedido.bodega_concesionario.bodccs_direccion,
                                   pedido.bodega_concesionario.bodccs_nombre,

                                   ciu_concesionario = n.ciu_nombre,
                                   b.dpto_nombre,
                                   ocu.tpocupacion_nombre,
                                   edoCivil.edocivil_nombre,
                                   vendedor = pedido.vendedor != null ? vend.user_nombre + " " + vend.user_apellido : "",
                                   user_numIdent = pedido.vendedor != null ? vend.user_numIdent.ToString() : "",                                       
                                   pedido.id
                               }).FirstOrDefault();


                                   var buscarDireccion = (from pedido in db.vpedido
                                   join tercero in db.icb_terceros
                                   on pedido.nit equals tercero.tercero_id
                                   join direcciones in db.terceros_direcciones
                                   on tercero.tercero_id equals direcciones.idtercero
                                   orderby direcciones.id descending
                                   where pedido.id == id
                                   select new { direcciones.direccion }).FirstOrDefault();

        if (buscaPedido != null)
        {


            var buscarReferencias = (from pedidoRepuesto in db.vpedrepuestos
                                     join pedido in db.vpedido
                                     on pedidoRepuesto.pedido_id equals pedido.id
                                     join referencia in db.icb_referencia
                                     on pedidoRepuesto.referencia equals referencia.ref_codigo into temp1
                                     from refer in temp1.DefaultIfEmpty()
                                     where pedido.id == id
                                     select new
                                     {
                                         refer.ref_descripcion,
                                         pedidoRepuesto.vrtotal
                                     }).ToList();


            var buscarVehiculoRetoma = (from retoma in db.vpedretoma
                                        where retoma.vpedido.id == id
                                        select new
                                        {
                                            retoma.modelo,
                                            retoma.placa,
                                            retoma.valor
                                        }).FirstOrDefault();

            var buscarFormaPago = (from pago in db.vpedpago
                                   join formaPago in db.vformapago
                                   on pago.condicion equals formaPago.id
                                   join a in db.vpedido
                                   on pago.idpedido equals a.id
                                   where a.id == id
                                   select new
                                   {
                                       pago.condicion,
                                       pago.valor,
                                       formaPago.tipo
                                   }).ToList();

            var buscarFinanciera = (from a in db.icb_unidad_financiera
                                    join b in db.vpedpago
                                    on a.financiera_id equals b.banco
                                    join c in db.vpedido
                                    on b.idpedido equals c.id
                                    where c.id == id
                                    select a.financiera_nombre).FirstOrDefault();


            var buscarCredito = (from a in db.vpedpago
                                 join b in db.vpedido
                                 on a.idpedido equals b.id
                                 where b.id == id && a.condicion == 1
                                 select a.valor).FirstOrDefault();
//tarea 2056
                    var buscarAsegurados = (from terc in db.icb_terceros
                                            where terc.tercero_id == buscaPedido.nit_asegurado ||
                                            terc.tercero_id == buscaPedido.nit2 ||
                                            terc.tercero_id == buscaPedido.nit3 ||
                                            terc.tercero_id == buscaPedido.nit4 ||
                                            terc.tercero_id == buscaPedido.nit5
                                            select new
                                            {
                                                documentoAsegurado = terc.doc_tercero,
                                                terc.prinom_tercero,
                                                terc.apellido_tercero
                                            }).ToList();
                    var nomAsegurados = "";
                    var docAsegurados = "";
                    for (var i = 0; i < buscarAsegurados.Count; i++)
                    {
                        nomAsegurados += buscarAsegurados[i].prinom_tercero + " " + buscarAsegurados[i].apellido_tercero + "/";
                        docAsegurados += '/' + buscarAsegurados[i].documentoAsegurado;
                    }
                    //fin tarea 2056
            decimal credito = 0;
            decimal contado = 0;
            foreach (var item in buscarFormaPago)
            {
                credito = item.tipo == 1 ? credito + item.valor ?? 0 : credito;
                contado = item.tipo == 2 ? contado + item.valor ?? 0 : contado;
            }
            decimal totalAPagar = buscarVehiculoRetoma != null ? buscarVehiculoRetoma.valor ?? 0 : 0;
            totalAPagar += credito + contado;

            var root = Server.MapPath("~/Pdf/");
            var pdfname = String.Format("{0}.pdf", Guid.NewGuid().ToString());
            var path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);

            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
            var valor = buscarCredito;

            var total = buscaPedido.vrtotal ?? 0;
            var descuento = buscaPedido.vrdescuento ?? 0;
            var calcularTotal = total - descuento;
            decimal totalReferencias = 0;
            string accesorios = "";
            if (buscarReferencias != null)
            {
                foreach (var item in buscarReferencias)
                {
                    totalReferencias += item.vrtotal ?? 0;
                    accesorios = accesorios == "" ? item.ref_descripcion : accesorios + "," + item.ref_descripcion;
                }
            }

        try
        {

            PedidoPDFModel modelo = new PedidoPDFModel()
            {
                NumeroPedido = buscaPedido.numero ?? 0,
                NombreBodega = buscaPedido.bodccs_nombre,
                DireccionBodega = buscaPedido.bodccs_direccion,
                CiudadBodega = buscaPedido.ciu_concesionario,
                DepartamentoBodega = buscaPedido.dpto_nombre,
                NombresCliente = buscaPedido.prinom_tercero + " " + buscaPedido.segnom_tercero + " " + buscaPedido.apellido_tercero + " " + buscaPedido.segapellido_tercero,
                CedulaNit = buscaPedido.doc_tercero,
                DireccionCliente = buscarDireccion.direccion,
                CiudadCliente = buscaPedido.ciu_nombre,
                TelefonoCliente = buscaPedido.telf_tercero,
                CelularCliente = buscaPedido.celular_tercero,
                ProfesionCliente = buscaPedido.tpocupacion_nombre,
                EstadoCivil = buscaPedido.edocivil_nombre,
                FechaNacimientoCliente = buscaPedido.fec_nacimiento != null ? buscaPedido.fec_nacimiento.Value.ToShortDateString() : "",
                EmailCliente = buscaPedido.email_tercero,
                DiaPedido = buscaPedido.fecha != null ? buscaPedido.fecha.Day.ToString() : "",
                MesPedido = buscaPedido.fecha != null ? buscaPedido.fecha.Month.ToString() : "",
                AnioPedido = buscaPedido.fecha != null ? buscaPedido.fecha.Year.ToString() : "",
                ModeloVehiculo = buscaPedido.modvh_nombre,
                AnioVehiculo = buscaPedido.anio != null ? buscaPedido.anio : 0,
                ColorVehiculo = buscaPedido.colvh_nombre,
                PlacaVehiculo = buscaPedido.plac_vh,
                PlanMayor = buscaPedido.planmayor,
                ServicioVehiculo = buscaPedido.tpserv_nombre,
                TipoVehiculo = buscaPedido.segvh_nombre,
                Descuento = buscaPedido.vrdescuento != null ? buscaPedido.vrdescuento.Value.ToString("0,0", elGR) : "0",
                PrecioAlPublico = buscaPedido.vrtotal != null ? buscaPedido.vrtotal.Value.ToString("0,0", elGR) : "0",
                PrecioVenta = calcularTotal.ToString("0,0", elGR),
                cantCuotas = buscaPedido.plazo != null ? buscaPedido.plazo.ToString() : "",
                saldoFinanciar = buscaPedido.vaprobado != null ? buscaPedido.vaprobado.ToString() : "",
                cuoInicial = buscaPedido.cuota_inicial != null ? buscaPedido.cuota_inicial.ToString() : "",
                poliza = buscaPedido.poliza != null ? buscaPedido.poliza.ToString() : "",
                Accesorios = accesorios,
                TotalAccesorios = totalReferencias.ToString("0,0", elGR),
                PlacaVhRetoma = buscarVehiculoRetoma != null ? buscarVehiculoRetoma.placa : "",
                ValorRetoma = buscarVehiculoRetoma != null ? buscarVehiculoRetoma.valor != null ? buscarVehiculoRetoma.valor.Value.ToString("0,0", elGR) : "" : "",
                ModeloVhRetoma = buscarVehiculoRetoma != null ? buscarVehiculoRetoma.modelo : "",
                CuotaInicial = contado.ToString("0,0", elGR),
                financiera = buscarFinanciera != null ? buscarFinanciera : "",
                ValorRetomaPago = buscarVehiculoRetoma != null ? buscarVehiculoRetoma.valor != null ? buscarVehiculoRetoma.valor.Value.ToString("0,0", elGR) : "" : "",
                SaldoFinanciar = credito.ToString("0,0", elGR),
                Total = totalAPagar.ToString("0,0", elGR),
                CedulaVendedor = buscaPedido.user_numIdent,
                valorCredito = valor != null ? valor.Value.ToString("0,0", elGR) : "",
                Vendedor = buscaPedido.vendedor,
                            //caso 2056
                            NombresAsegurados = nomAsegurados,
                            DocumentosAsegurados = docAsegurados,
                            //fin caso 2056

            };
            var something = new Rotativa.ViewAsPdf("ContratoCompraventa", modelo) { /*FileName = "cv.pdf" SaveOnServerPath = path*/ };
            return something;
            }
            catch (Exception es)
            {
            var mensaje = es.InnerException;
            throw;
           }

            //return View();
        }
        else
                {
                    TempData["mensaje_error"] = "No se puede generar el pdf";
                    return RedirectToAction("Edit", "vpedidos", new { id = id, menu = "5278" });
                }
            }
            else
            {
                TempData["mensaje_error"] = "No se ha seleccionado un número de pedido válido";
                return RedirectToAction("Create", "vpedidos");
            }
        }

        public void listas(VehiculoPedidoModel vpedidos)
        {
            var clientes = from t in db.icb_terceros
                               //join c in db.tercero_cliente
                               //on t.tercero_id equals c.tercero_id //se quita la validacion de mostrar solo terceros clientes el dia 28/02/19 a peticion de caminos y Jairo -> solicitaron mostrar todos los terceros
                           where t.doc_tercero != null
                           orderby t.prinom_tercero
                           select new
                           {
                               cedula = t.prinom_tercero != null ? t.doc_tercero + " - " + t.prinom_tercero + " " + t.apellido_tercero + " " + t.segapellido_tercero : t.doc_tercero + " - " + t.razon_social,
                               t.tercero_id
                           };
            ViewBag.rol = Session["user_rolid"];
            var rol = Convert.ToInt32(Session["user_rolid"]);
            //busco si el rol tiene permiso de modificar impuesto de consumo
            var paramimpu = db.icb_sysparameter.Where(d => d.syspar_cod == "P121").FirstOrDefault();
            var permisorol = paramimpu != null ? Convert.ToInt32(paramimpu.syspar_value) : 1029;
            //veo si el permiso está asignado a este rol
            var permisos = db.rolacceso.Where(d => d.idrol == rol && d.idpermiso == permisorol).Count();
            if (permisos > 0)
            {
                ViewBag.permiso = 1;
            }
            else
            {
                ViewBag.permiso = 0;
            }
            //busco si el rol tiene permiso de modificar nombre de Asesor en Pedido
            var paramAsesor= db.icb_sysparameter.Where(d => d.syspar_cod == "P122").FirstOrDefault();
            var permisorol2 = paramAsesor != null ? Convert.ToInt32(paramAsesor.syspar_value) : 1030;
            //veo si el permiso está asignado a este rol
            var permisos2 = db.rolacceso.Where(d => d.idrol == rol && d.idpermiso == permisorol2).Count();
            if (permisos2 > 0)
            {
                ViewBag.permiso2 = 1;
            }
            else
            {
                ViewBag.permiso2 = 0;
            }
            var nit = new List<SelectListItem>();
            var nit2 = new List<SelectListItem>();
            var nit3 = new List<SelectListItem>();
            var nit4 = new List<SelectListItem>();
            var nit5 = new List<SelectListItem>();
            var nit_asegurado = new List<SelectListItem>();
            var nitPrenda = new List<SelectListItem>();

            foreach (var item in clientes)
            {
                nit.Add(new SelectListItem()
                {
                    Text = item.cedula,
                    Value = item.tercero_id.ToString(),
                    Selected = item.tercero_id == vpedidos.nit ? true : false
                });
                nit2.Add(new SelectListItem()
                {
                    Text = item.cedula,
                    Value = item.tercero_id.ToString(),
                    Selected = item.tercero_id == vpedidos.nit2 ? true : false

                });
                nit3.Add(new SelectListItem()
                {
                    Text = item.cedula,
                    Value = item.tercero_id.ToString(),
                    Selected = item.tercero_id == vpedidos.nit3 ? true : false

                });
                nit4.Add(new SelectListItem()
                {
                    Text = item.cedula,
                    Value = item.tercero_id.ToString(),
                    Selected = item.tercero_id == vpedidos.nit4 ? true : false
                });
                nit5.Add(new SelectListItem()
                {
                    Text = item.cedula,
                    Value = item.tercero_id.ToString(),
                    Selected = item.tercero_id == vpedidos.nit5 ? true : false
                });
                nit_asegurado.Add(new SelectListItem()
                {
                    Text = item.cedula,
                    Value = item.tercero_id.ToString(),
                    Selected = item.tercero_id == vpedidos.nit_asegurado ? true : false
                });
            }
            var nitPrendas = from f in db.icb_unidad_financiera
                             orderby f.financiera_nombre
                             select new
                             {
                                 id = f.financiera_id,
                                 nombre = f.financiera_nombre
                             };
            var listFinancieras = new List<SelectListItem>();
            foreach (var item in nitPrendas)
            {
                listFinancieras.Add(new SelectListItem()
                {
                    Value = item.id.ToString(),
                    Text = item.nombre,
                    Selected = item.id == vpedidos.nit_prenda ? true : false
                });
            }

            var asesores = from u in db.users
                           where u.rol_id == 4
                           orderby u.user_nombre
                           select new
                           {
                               nombre = u.user_nombre + " " + u.user_apellido,
                               u.user_id
                           };
            var lisAsesores = new List<SelectListItem>();
            foreach (var item in asesores)
            {
                lisAsesores.Add(new SelectListItem()
                {
                    Text = item.nombre,
                    Value = item.user_id.ToString(),
                    Selected = item.user_id == vpedidos.vendedor ? true : false
                });
            }

            var flota = from f in db.vcodflota
                        orderby f.codigo
                        select new
                        {
                            id = f.id,
                            nombre = f.codigo + " - " + f.Descripcion
                        };
            var lisflotas = new List<SelectListItem>();
            foreach (var item in flota)
            {
                lisflotas.Add(new SelectListItem()
                {
                    Text = item.nombre,
                    Value = item.id.ToString(),
                    Selected = item.id == vpedidos.codflota ? true : false,
                });
            }

            var data = from v in db.vflota
                       orderby v.numero
                       select new
                       {
                           id = v.idflota,
                           nombre = v.icb_terceros.prinom_tercero != null ? v.numero + " - " + v.icb_terceros.prinom_tercero + " " + v.icb_terceros.segnom_tercero + " " + v.icb_terceros.apellido_tercero + " " + v.icb_terceros.segapellido_tercero : v.numero + " - " + v.icb_terceros.razon_social
                       };
            var lflota = new List<SelectListItem>();
            foreach (var item in data)
            {
                lflota.Add(new SelectListItem()
                {
                    Text = item.nombre,
                    Value = item.id.ToString(),
                    Selected = item.id == vpedidos.flota ? true : false,
                });
            }
            ViewBag.nit = nit;
            ViewBag.nit2 = nit2;
            ViewBag.nit3 = nit3;
            ViewBag.nit4 = nit4;
            ViewBag.nit5 = nit5;
            ViewBag.nit_asegurado = nit_asegurado;
            ViewBag.nit_prenda = listFinancieras;
            ViewBag.vendedor = lisAsesores;
            //ViewBag.idcotizacion = db.icb_cotizacion.ToList();
            ViewBag.Color_Deseado = new SelectList(db.color_vehiculo.Where(x => x.colvh_estado == true).OrderBy(x => x.colvh_nombre), "colvh_id", "colvh_nombre", vpedidos.Color_Deseado);
            ViewBag.color_opcional = new SelectList(db.color_vehiculo.Where(x => x.colvh_estado == true).OrderBy(x => x.colvh_nombre), "colvh_id", "colvh_nombre", vpedidos.color_opcional);
            //ViewBag.flota = new SelectList(db.vflota, "idflota", "numero", vpedidos.flota);

            var pedidoscot = db.vpedido.Where(d => d.anulado == false /*|| d.anulado == true*/).Select(d => d.idcotizacion).ToArray();

            var cotizaciones = db.icb_cotizacion.Where(d => !(pedidoscot).Contains(d.cot_idserial)).ToList();
            ViewBag.idcotizacion = cotizaciones;

            ViewBag.flota = lflota;
            ViewBag.codflota = lisflotas;

            ViewBag.marcvh_id = new SelectList(db.marca_vehiculo.OrderBy(x => x.marcvh_nombre), "marcvh_id", "marcvh_nombre", vpedidos.marcvh_id);
            ViewBag.modelo = new SelectList(db.modelo_vehiculo.OrderBy(x => x.modvh_nombre), "modvh_codigo", "modvh_nombre", vpedidos.modelo);
            ViewBag.id_anio_modelo = new SelectList(db.anio_modelo.OrderBy(x => x.anio), "anio_modelo_id", "anio", vpedidos.id_anio_modelo);

            ViewBag.marcas = new SelectList(db.marca_vehiculo.OrderBy(x => x.marcvh_nombre), "marcvh_id", "marcvh_nombre", vpedidos.marcvh_id);
            ViewBag.modelosMarca = new SelectList(db.modelo_vehiculo.OrderBy(x => x.modvh_nombre), "modvh_codigo", "modvh_nombre", vpedidos.modelo);
            ViewBag.idAnioModelo = new SelectList(db.anio_modelo.Where(x => x.codigo_modelo == vpedidos.modelo).OrderBy(x => x.anio), "anio_modelo_id", "anio", vpedidos.id_anio_modelo);

            var motivoCambio = (from c in db.vcambiovehiculo
                                join p in db.vpedido
                                on c.id equals p.id
                                where c.idpedido == vpedidos.id
                                orderby c.id descending
                                select new { c.motivo }).FirstOrDefault();

            ViewBag.motivoCambio = motivoCambio;

            ViewBag.plan_venta = new SelectList(db.icb_plan_financiero, "plan_id", "plan_nombre", vpedidos.plan_venta);
            ViewBag.numpedido = vpedidos.numero;
            ViewBag.tipovh = vpedidos.nuevo == true ? "Nuevo" : "Usado";
            ViewBag.servicio = new SelectList(db.tpservicio_vehiculo.Where(x => x.tpserv_estado == true).OrderBy(x => x.tpserv_nombre), "tpserv_id", "tpserv_nombre", vpedidos.servicio);
            ViewBag.cargomatricula = new SelectList(db.vcargomatricula.Where(x => x.estado == true).OrderBy(x => x.descripcion), "id", "descripcion", vpedidos.cargomatricula);
            ViewBag.tipo_carroceria = new SelectList(db.tipo_carroceria.Where(x => x.estado == true).OrderBy(x => x.descripcion), "idcarroceria", "descripcion", vpedidos.tipo_carroceria);

            ViewBag.iddepartamento = new SelectList(db.nom_departamento.OrderBy(x => x.dpto_nombre).Where(x => x.dpto_estado != false), "dpto_id", "dpto_nombre", vpedidos != null ? vpedidos.iddepartamento : 0);
            ViewBag.idciudad = new SelectList(db.nom_ciudad.OrderBy(x => x.ciu_nombre).Where(x => x.ciu_estado != false), "ciu_id", "ciu_nombre", vpedidos != null ? vpedidos.idciudad : 0);

            var buscarSerialUltimoPed = db.vpedido.OrderByDescending(x => x.id).FirstOrDefault();
            ViewBag.numPedidoCreado = buscarSerialUltimoPed != null ? buscarSerialUltimoPed.numero : 0;
        }


        // GET: vpedidos/Create
        public ActionResult Create(int? menu)
        {
            VehiculoPedidoModel vpedidos = new VehiculoPedidoModel();
            listas(vpedidos);
            BuscarFavoritos(menu);
            return View();
        }

        // POST: vpedidos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create(VehiculoPedidoModel vpedido, int? menu)
        {
            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var num_pedido = db.vpedido.OrderByDescending(x => x.numero).FirstOrDefault();

                        if (num_pedido != null)
                        {
                            vpedido.numero = num_pedido.numero + 1;
                        }
                        else
                        {
                            vpedido.numero = 1;
                        }

                        var tpvh = Request["tipoVh"];

                        if (tpvh == "Nuevo" || tpvh == "true")
                        {
                            vpedido.nuevo = true;
                            vpedido.usado = false;
                        }
                        else
                        {
                            vpedido.nuevo = false;
                            vpedido.usado = true;
                        }

                        vpedido.impfactura2 = false;
                        vpedido.impfactura3 = false;
                        vpedido.impfactura4 = false;
                        var bodega = Session["user_bodega"];
                        vpedido.bodega = Convert.ToInt32(Session["user_bodega"]);
                        vpedido.anulado = false;
                        vpedido.fecha = DateTime.Now;
                        vpedido.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                        vpedido.fec_creacion = DateTime.Now;
                        #region creas objeto pedido de la clase vpedido
                        vpedido pedido = new IcebergModel.vpedido();
                        pedido.numero = vpedido.numero;
                        pedido.impfactura2 = vpedido.impfactura2;
                        pedido.impfactura3 = vpedido.impfactura3;
                        pedido.impfactura4 = vpedido.impfactura4;
                        pedido.bodega = vpedido.bodega;
                        pedido.anulado = vpedido.anulado;
                        pedido.fecha = Convert.ToDateTime(vpedido.fecha);
                        pedido.idcotizacion = vpedido.idcotizacion;
                        pedido.nit = vpedido.nit;
                        pedido.nit_asegurado = vpedido.nit_asegurado;
                        pedido.nit2 = vpedido.nit2;
                        pedido.nit3 = vpedido.nit3;
                        pedido.nit4 = vpedido.nit4;
                        pedido.nit5 = vpedido.nit5;
                        pedido.vendedor = vpedido.vendedor;
                        pedido.modelo = vpedido.modelo;
                        pedido.id_anio_modelo = vpedido.id_anio_modelo;
                        pedido.plan_venta = vpedido.plan_venta;
                        pedido.planmayor = vpedido.planmayor;
                        pedido.fecha_asignacion_planmayor = vpedido.fecha_asignacion_planmayor;
                        pedido.asignado_por = vpedido.asignado_por;
                        pedido.condicion = vpedido.condicion;
                        pedido.dias_validez = vpedido.dias_validez;
                        pedido.valor_unitario = Convert.ToDecimal(vpedido.valor_unitario);
                        pedido.porcentaje_iva = vpedido.porcentaje_iva;
                        pedido.valorPoliza = Convert.ToDecimal(vpedido.valorPoliza);
                        pedido.pordscto = vpedido.pordscto;
                        pedido.vrdescuento = Convert.ToDecimal(vpedido.vrdescuento);
                        pedido.cantidad = vpedido.cantidad;
                        pedido.tipo_carroceria = vpedido.tipo_carroceria;
                        pedido.vrcarroceria = Convert.ToDecimal(vpedido.vrcarroceria);
                        pedido.vrtotal = Convert.ToDecimal(vpedido.vrtotal);
                        pedido.moneda = vpedido.moneda;
                        pedido.id_aseguradora = vpedido.id_aseguradora;
                        pedido.notas1 = vpedido.notas1;
                        pedido.notas2 = vpedido.notas2;
                        pedido.escanje = vpedido.escanje;
                        pedido.eschevyplan = vpedido.eschevyplan;
                        pedido.esLeasing = vpedido.esLeasing;
                        pedido.esreposicion = vpedido.esreposicion;
                        pedido.nit_prenda = vpedido.nit_prenda;
                        pedido.flota = vpedido.flota;
                        pedido.codigoflota = vpedido.codflota;
                        pedido.facturado = vpedido.facturado;
                        pedido.numfactura = vpedido.numfactura;
                        pedido.porcentaje_impoconsumo = vpedido.porcentaje_impoconsumo;
                        pedido.numeroplaca = vpedido.numeroplaca;
                        pedido.motivo_anulacion = vpedido.motivo_anulacion;
                        pedido.venta_gerencia = vpedido.venta_gerencia;
                        pedido.Color_Deseado = vpedido.Color_Deseado;
                        pedido.terminacionplaca = vpedido.terminacionplaca;
                        pedido.bono = vpedido.bono;
                        pedido.marca = Convert.ToInt32(vpedido.marcvh_id);
                        pedido.idmodelo = vpedido.idmodelo;
                        pedido.nuevo = vpedido.nuevo;
                        pedido.usado = vpedido.usado;
                        pedido.servicio = vpedido.servicio;
                        pedido.placapar = vpedido.placapar;
                        pedido.placaimpar = vpedido.placaimpar;
                        pedido.color_opcional = vpedido.color_opcional;
                        pedido.cargomatricula = vpedido.cargomatricula;
                        pedido.obsequioporcen = vpedido.obsequioporcen;
                        pedido.valormatricula = Convert.ToDecimal(vpedido.valormatricula);
                        pedido.rango_placa = vpedido.rango_placa;
                        pedido.userid_creacion = vpedido.userid_creacion;
                        pedido.fec_creacion = vpedido.fec_creacion;
                        pedido.iddepartamento = vpedido.iddepartamento;
                        pedido.idciudad = vpedido.idciudad;
                        pedido.valorsoat = Convert.ToDecimal(vpedido.valorsoat);
                        pedido.otrosValores = Convert.ToDecimal(vpedido.otrosValores);
                        #endregion
                        db.vpedido.Add(pedido);

                        db.SaveChanges();
                        var result = db.SaveChanges();
                        var pedido_id = db.vpedido.OrderByDescending(x => x.id).FirstOrDefault().id;

                        #region pagos
                        //se capturan las formas de pago que que se guardan en la vista con el formato ~ para separar cada forma de pago y | para separar los datos
                        string[] formas_pago = Request["formas_depago_json"].Split('~');

                        for (int i = 0; i < formas_pago.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(formas_pago[i])){
                                string[] forma_pago = formas_pago[i].Split('|');
                                vpedpago vpago = new vpedpago();
                                vpago.idpedido = pedido_id;
                                vpago.seq = i+1;
                                vpago.condicion = Convert.ToInt32(forma_pago[1]);//forma de pago
                                vpago.valor = Convert.ToDecimal(forma_pago[3]); //Valor de la forma de pago
                                vpago.fecpago = Convert.ToDateTime(forma_pago[4]); //Fecha de pago
                                vpago.observaciones = forma_pago[7]; //observaciones
                                var banco = forma_pago[5];
                                if (!string.IsNullOrEmpty(banco))
                                {
                                    vpago.banco = Convert.ToInt32(banco);
                                }
                                db.vpedpago.Add(vpago);
                            }
                            
                        }


                        #endregion
                        #region repuestos
                        string[] accesorios = Request["accesorios_json"].Split('~');

                        for (int i = 0; i < accesorios.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(accesorios[i]))
                            {
                                string[] accesorio_item = accesorios[i].Split('|');
                                vpedrepuestos vrepuesto = new vpedrepuestos();
                                vrepuesto.pedido_id = pedido_id;
                                vrepuesto.referencia = accesorio_item[1];
                                vrepuesto.vrunitario = Convert.ToDecimal(accesorio_item[2]);
                                vrepuesto.vrtotal = Convert.ToDecimal(accesorio_item[4]);
                                vrepuesto.obsequio = Convert.ToBoolean(accesorio_item[3]);
                                vrepuesto.cantidad = Convert.ToInt32(accesorio_item[6]);
                                //vrepuesto.obsequio = Convert.ToBoolean(Request["obsequio" + j]);
                                //vrepuesto.obsequio = Convert.ToBoolean(vrepuesto.obsequio == true ? "Si" : "No");
                                db.vpedrepuestos.Add(vrepuesto);
                            }

                        }
                        #endregion
                        #region retomas
                        var lr = Request["lista_retomas"];
                        if (!String.IsNullOrEmpty(lr))
                        {
                            var lista_retomas = Convert.ToInt32(Request["lista_retomas"]);
                            for (int i = 1; i <= lista_retomas; i++)
                            {
                                if (!string.IsNullOrEmpty(Request["valor_retoma" + i]))
                                {
                                    vpedretoma retoma = new vpedretoma();
                                    retoma.pedido_id = pedido_id;
                                    retoma.placa = Request["placa_retoma" + i];
                                    retoma.valor = Convert.ToDecimal(Request["valor_retoma" + i]);
                                    retoma.modelo = Request["modelo_retoma" + i];
                                    if (!string.IsNullOrEmpty(Request["kl_retoma" + i]))
                                    {
                                        retoma.kilometraje = Convert.ToDecimal(Request["kl_retoma" + i]);
                                    }
                                    if (!string.IsNullOrEmpty(Request["obligacion_retoma" + i]))
                                    {
                                        retoma.obligaciones = Convert.ToBoolean(Request["obligacion_retoma" + i]);
                                        if (!string.IsNullOrEmpty(Request["valor_obligacion" + i]))
                                        {
                                            retoma.valor_obligacion = Convert.ToDecimal(Request["valor_obligacion" + i]);
                                        }
                                    }
                                    else
                                    {
                                        retoma.obligaciones = false;
                                    }

                                    db.vpedretoma.Add(retoma);
                                }
                            }
                        }
                        #endregion
                        #region costos

                        string[] costos_array = Request["costos_json"].Split('~');

                        for (int i = 0; i < costos_array.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(costos_array[i]))
                            {
                                string[] costo_item = costos_array[i].Split('|');
                                vpedcostos_adicionales costos = new vpedcostos_adicionales();
                                costos.pedido_id = pedido_id;
                                costos.descripcion = costo_item[1];
                                if (!string.IsNullOrEmpty(costo_item[3]))
                                {
                                    costos.valor = Convert.ToDecimal(costo_item[3]);
                                }
                                if (!string.IsNullOrEmpty(costo_item[2]))
                                {
                                    costos.obsequio = Convert.ToBoolean(costo_item[2]);
                                }
                                else
                                {
                                    costos.obsequio = false;
                                }
                                db.vpedcostos_adicionales.Add(costos);
                            }

                        }

                        #endregion
                        #region Documentos
                        var codflota = db.vflota.Find(vpedido.flota);
                        if (codflota != null)
                        {
                            var docrequeridosflota = db.vdocrequeridosflota.Where(x => x.codflota == codflota.flota);
                            foreach (var item in docrequeridosflota)
                            {
                                vvalidacionpeddoc validacion = new vvalidacionpeddoc();
                                validacion.idpedido = pedido_id;
                                validacion.idflota = vpedido.flota ?? 0;
                                validacion.codflota = codflota.flota;
                                validacion.iddocrequerido = item.id;
                                validacion.estado = Convert.ToBoolean(Request["documento_" + item.id]);
                                db.vvalidacionpeddoc.Add(validacion);
                            }

                        }
                        else
                        {
                            var tercero = (from p in db.vpedido
                                           join t in db.icb_terceros
                                           on p.nit equals t.tercero_id
                                           where p.nit == vpedido.nit
                                           select t.doc_tercero).FirstOrDefault();

                            var tipoPer = from t in db.icb_terceros
                                          join d in db.tp_documento
                                          on t.tpdoc_id equals d.tpdoc_id
                                          where t.doc_tercero == tercero
                                          select new
                                          {
                                              tipo = d.tipo.Trim()
                                          };
                            ListaPersonas Lista = new ListaPersonas();
                            if (tipoPer.ToString() == "N")
                            {
                                Lista.ListaDocNecesarios = (from d in db.vdocumentosflota
                                                            where d.id_tipo_documento == 3
                                                            select new docNecesarios
                                                            {
                                                                id = d.id,
                                                                documento = d.documento,
                                                                iddocumento = d.iddocumento,
                                                            }).ToList();
                            }
                            else if (tipoPer.ToString() != "N")
                            {
                                Lista.ListaDocNecesarios = (from d in db.vdocumentosflota
                                                            where d.id_tipo_documento == 2
                                                            select new docNecesarios
                                                            {
                                                                id = d.id,
                                                                documento = d.documento,
                                                                iddocumento = d.iddocumento,
                                                            }).ToList();
                            }

                            foreach (var item in Lista.ListaDocNecesarios)
                            {
                                vvalidacionpeddoc validacion = new vvalidacionpeddoc();
                                validacion.idpedido = pedido_id;
                                validacion.iddocrequerido = item.id;
                                validacion.estado = false;
                                db.vvalidacionpeddoc.Add(validacion);
                            }
                        }
                        #endregion
                        #region Seguimiento
                        db.seguimientotercero.Add(new seguimientotercero()
                        {
                            idtercero = vpedido.nit,
                            tipo = 4,
                            nota = "El tercero realizo un pedido con numero " + vpedido.numero,
                            fecha = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                        });

                        if (vpedido.idcotizacion != null)
                        {
                            vcotseguimiento seguimiento = new vcotseguimiento();
                            seguimiento.cot_id = Convert.ToInt32(vpedido.idcotizacion);
                            seguimiento.fecha = DateTime.Now;
                            seguimiento.responsable = Convert.ToInt32(Session["user_usuarioid"]);
                            seguimiento.Notas = "Se genero pedido de vehiculo numero " + vpedido.numero;
                            seguimiento.Motivo = "";
                            seguimiento.fec_creacion = DateTime.Now;
                            seguimiento.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                            seguimiento.estado = true;
                            seguimiento.tipo_seguimiento = 4;
                            db.vcotseguimiento.Add(seguimiento);
                        }
                        #endregion

                        try
                        {
                            result = db.SaveChanges();
                            if (result > 0)
                            {
                                dbTran.Commit();
                                //----- Tomar el ultimo pedido agregado para mostrar la factura proforma
                                var ultimoAgregado = db.vpedido.OrderByDescending(x => x.numero).FirstOrDefault();
                                ViewBag.pedidoId = ultimoAgregado.numero;
                                //-----
                                TempData["mensaje"] = "Pedido registrado correctamente";

                                listas(vpedido);
                                BuscarFavoritos(menu);

                                return RedirectToAction("Create", new { id = vpedido.id, menu });
                            }
                            else
                            {
                                dbTran.Rollback();
                                TempData["mensaje_error"] = "Error al registrar el pedido, por favor intente nuevamente";
                                var errors = ModelState.Select(x => x.Value.Errors)
                                  .Where(y => y.Count > 0)
                                  .ToList();
                            }
                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            Exception raise = dbEx;
                            foreach (var validationErrors in dbEx.EntityValidationErrors)
                            {
                                foreach (var validationError in validationErrors.ValidationErrors)
                                {
                                    string message = string.Format("{0}:{1}",
                                        validationErrors.Entry.Entity.ToString(),
                                        validationError.ErrorMessage);
                                    // raise a new exception nesting
                                    // the current instance as InnerException
                                    raise = new InvalidOperationException(message, raise);
                                    TempData["mensaje_error"] = message;
                                    dbTran.Rollback();
                                    listas(vpedido);
                                    BuscarFavoritos(menu);
                                    return View(vpedido);
                                }
                            }
                        }
                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        throw;
                    }
                }
            }
            else
            {
                //TempData["mensaje_error"] = "Errores en la creación del pedido, por favor valide";
                var errors = ModelState.Select(x => x.Value.Errors)
                            .Where(y => y.Count > 0)
                            .ToList();
            }
            listas(vpedido);
            BuscarFavoritos(menu);
            return View(vpedido);
        }

        // GET: vpedidos/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            vpedido vpedido = db.vpedido.Find(id);
            if (vpedido == null)
            {
                // TempData["mensaje"] = "Nota no existe pedido, por tanto no hay seguimiento";
                //// return RedirectToAction("Seguimiento", new { id = id, menu = menu });
                // return RedirectToAction("Create", "v_creditos", new { menu = menu });
                return HttpNotFound();
            }
            var buscarColor = (from a in db.vpedido
                               join b in db.icb_vehiculo
                               on a.planmayor equals b.plan_mayor into xx
                               from b in xx.DefaultIfEmpty()
                               join c in db.color_vehiculo
                               on b.colvh_id equals c.colvh_id into zz
                               from c in zz.DefaultIfEmpty()
                               where a.id == id
                               select new { b.vin, c.colvh_nombre }).FirstOrDefault();
            #region obj pedido
            VehiculoPedidoModel pedido = new VehiculoPedidoModel();

            pedido.color = buscarColor.colvh_nombre;
            pedido.serie = buscarColor.vin;
            pedido.numero = vpedido.numero;
            pedido.impfactura2 = vpedido.impfactura2;
            pedido.impfactura3 = vpedido.impfactura3;
            pedido.impfactura4 = vpedido.impfactura4;
            pedido.bodega = vpedido.bodega;
            pedido.anulado = vpedido.anulado;
            pedido.fecha = vpedido.fecha;
            pedido.idcotizacion = vpedido.idcotizacion;
            pedido.numerocotizacion = Convert.ToInt32(db.icb_cotizacion.Where(d => d.cot_idserial == vpedido.idcotizacion).Select(d => d.cot_numcotizacion).FirstOrDefault());
            pedido.nit = vpedido.nit;
            pedido.numeroIdentificacion = Convert.ToString(db.icb_terceros.Where(d => d.tercero_id == vpedido.nit).Select(d => d.doc_tercero).FirstOrDefault());
            pedido.nit_asegurado = vpedido.nit_asegurado;
            pedido.nit2 = vpedido.nit2;
            pedido.nit3 = vpedido.nit3;
            pedido.nit4 = vpedido.nit4;
            pedido.nit5 = vpedido.nit5;
            pedido.vendedor = vpedido.vendedor;
            pedido.modelo = vpedido.modelo;
            pedido.id_anio_modelo = vpedido.id_anio_modelo;
            pedido.plan_venta = vpedido.plan_venta;
            pedido.planmayor = vpedido.planmayor;
            pedido.fecha_asignacion_planmayor = vpedido.fecha_asignacion_planmayor;
            pedido.asignado_por = vpedido.asignado_por;
            pedido.condicion = vpedido.condicion;
            pedido.dias_validez = vpedido.dias_validez;
            pedido.valor_unitario = vpedido.valor_unitario != null ? vpedido.valor_unitario.Value.ToString("N0", new CultureInfo("is-IS")) : "0";
            pedido.porcentaje_iva = vpedido.porcentaje_iva;
            pedido.valorPoliza = vpedido.valorPoliza != null ? vpedido.valorPoliza.Value.ToString("N0", new CultureInfo("is-IS")) : "0";
            pedido.pordscto = vpedido.pordscto;
            pedido.vrdescuento = vpedido.vrdescuento != null ? vpedido.vrdescuento.Value.ToString("N0", new CultureInfo("is-IS")) : "0";
            pedido.cantidad = vpedido.cantidad;
            pedido.tipo_carroceria = vpedido.tipo_carroceria;
            pedido.vrcarroceria = vpedido.vrcarroceria != null ? vpedido.vrcarroceria.Value.ToString("N0", new CultureInfo("is-IS")) : "0";
            pedido.vrtotal = vpedido.vrtotal != null ? vpedido.vrtotal.Value.ToString("N0", new CultureInfo("is-IS")) : "0";
            pedido.moneda = vpedido.moneda;
            pedido.id_aseguradora = vpedido.id_aseguradora;
            pedido.notas1 = vpedido.notas1;
            pedido.notas2 = vpedido.notas2;
            pedido.escanje = vpedido.escanje;
            pedido.eschevyplan = vpedido.eschevyplan;
            pedido.esLeasing = vpedido.esLeasing;
            pedido.esreposicion = vpedido.esreposicion;
            pedido.nit_prenda = vpedido.nit_prenda;
            pedido.flota = vpedido.flota;
            pedido.codflota = vpedido.codigoflota;
            pedido.facturado = vpedido.facturado;
            pedido.numfactura = vpedido.numfactura;
            pedido.porcentaje_impoconsumo = vpedido.porcentaje_impoconsumo;
            pedido.numeroplaca = vpedido.numeroplaca;
            pedido.motivo_anulacion = vpedido.motivo_anulacion;
            pedido.venta_gerencia = vpedido.venta_gerencia;
            pedido.Color_Deseado = vpedido.Color_Deseado;
            pedido.terminacionplaca = vpedido.terminacionplaca;
            pedido.bono = vpedido.bono;
            pedido.idmodelo = vpedido.idmodelo;
            pedido.nuevo = vpedido.nuevo;
            pedido.usado = vpedido.usado;
            pedido.servicio = vpedido.servicio;
            pedido.placapar = vpedido.placapar;
            pedido.placaimpar = vpedido.placaimpar;
            pedido.color_opcional = vpedido.color_opcional;
            pedido.cargomatricula = vpedido.cargomatricula;
            pedido.obsequioporcen = vpedido.obsequioporcen;
            pedido.valormatricula = vpedido.valormatricula != null ? vpedido.valormatricula.Value.ToString("N0", new CultureInfo("is-IS")) : "0";
            pedido.rango_placa = vpedido.rango_placa;
            pedido.marcvh_id = Convert.ToString(vpedido.marca);
            pedido.fec_creacion = vpedido.fec_creacion;
            pedido.userid_creacion = vpedido.userid_creacion;
            pedido.iddepartamento = vpedido.iddepartamento;
            pedido.idciudad = vpedido.idciudad;
            pedido.valorsoat = vpedido.valorsoat != null ? vpedido.valorsoat.Value.ToString("N0", new CultureInfo("is-IS")) : "0";
            pedido.otrosValores = vpedido.otrosValores != null ? vpedido.otrosValores.Value.ToString("N0", new CultureInfo("is-IS")) : "0";
            #endregion

            listas(pedido);
            BuscarFavoritos(menu);
            return View(pedido);
        }

        public ActionResult modalesBackOffice()
        {
            ViewBag.tp_doc_registros = db.tp_doc_registros.Where(x => x.tpdoc_estado == true && x.sw == 3 && x.tipo == 2).OrderBy(x => x.tpdoc_nombre).ToList();
            ViewBag.tp_doc_registrosFR = db.tp_doc_registros.Where(x => x.tpdoc_estado == true && x.sw == 3 && x.tipo == 4).OrderBy(x => x.tpdoc_nombre).ToList();
            return PartialView("modalesBackOffice");
        }

        public ActionResult submenuBackOffice(int? id)
        {
            if (id != null)
            {
                datosSubmenu data = (from p in db.vw_browserBackOffice
                                     where p.id == id
                                     select new datosSubmenu
                                     {
                                         id = p.id,
                                         bodega = p.bodega,
                                         numero = p.numero,
                                         planmayor = p.planmayor.ToString(),
                                         modelo = p.modelo,
                                         idCliente = p.idCliente,
                                         autorizado = p.autorizado
                                     }).FirstOrDefault();
                ViewBag.datos = new datosSubmenu
                {
                    id = data.id,
                    bodega = data.bodega,
                    numero = data.numero,
                    planmayor = data.planmayor.ToString(),
                    modelo = data.modelo,
                    idCliente = data.idCliente,
                    autorizado = data.autorizado
                };
            }
            else
            {
                id = 0;
            }
            ViewBag.idn = id;
            return PartialView("submenuBackOffice");
        }

        public JsonResult eliminarPlanMayorPedido(int idPedido)
        {
            var buscarVehiculo = db.icb_vehiculo.FirstOrDefault(x => x.asignado == idPedido);
            if (buscarVehiculo != null)
            {
                buscarVehiculo.asignado = null;
                db.Entry(buscarVehiculo).State = EntityState.Modified;
            }
            var result = db.SaveChanges();
            if (result > 0)
            {
                return Json(new { exito = true }, JsonRequestBehavior.AllowGet);

            }
            else
            {
                return Json(new { exito = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Edit(VehiculoPedidoModel vpedido, int? menu)
        {
            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        vpedido.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        vpedido.fec_actualizacion = DateTime.Now;

                        if (vpedido.planmayor != null)
                        {
                            var vehiculo = db.icb_vehiculo.Find(vpedido.planmayor);
                            vehiculo.asignado = vpedido.id;
                            db.Entry(vehiculo).State = EntityState.Modified;
                            vpedido.fecha_asignacion_planmayor = DateTime.Now;
                        }
                        //creas objeto pedido de la clase vpedido
                        #region variables pedido
                        vpedido pedidos = new vpedido();
                        pedidos.id = vpedido.id;
                        pedidos.numero = vpedido.numero;
                        pedidos.impfactura2 = vpedido.impfactura2;
                        pedidos.impfactura3 = vpedido.impfactura3;
                        pedidos.impfactura4 = vpedido.impfactura4;
                        pedidos.bodega = vpedido.bodega;
                        pedidos.anulado = vpedido.anulado;
                        pedidos.fecha = Convert.ToDateTime(vpedido.fecha);
                        pedidos.idcotizacion = vpedido.idcotizacion;
                        pedidos.nit = vpedido.nit;
                        pedidos.nit_asegurado = vpedido.nit_asegurado;
                        pedidos.nit2 = vpedido.nit2;
                        pedidos.nit3 = vpedido.nit3;
                        pedidos.nit4 = vpedido.nit4;
                        pedidos.nit5 = vpedido.nit5;
                        pedidos.vendedor = vpedido.vendedor;
                        pedidos.marca = Convert.ToInt32(vpedido.marcvh_id);
                        pedidos.modelo = vpedido.modelo;
                        pedidos.id_anio_modelo = vpedido.id_anio_modelo;
                        pedidos.plan_venta = vpedido.plan_venta;
                        pedidos.planmayor = vpedido.planmayor;
                        pedidos.fecha_asignacion_planmayor = vpedido.fecha_asignacion_planmayor;
                        pedidos.asignado_por = vpedido.asignado_por;
                        pedidos.condicion = vpedido.condicion;
                        pedidos.dias_validez = vpedido.dias_validez;
                        pedidos.valor_unitario = Convert.ToDecimal(vpedido.valor_unitario);
                        pedidos.porcentaje_iva = vpedido.porcentaje_iva;
                        pedidos.valorPoliza = Convert.ToDecimal(vpedido.valorPoliza);
                        pedidos.pordscto = vpedido.pordscto;
                        pedidos.vrdescuento = Convert.ToDecimal(vpedido.vrdescuento);
                        pedidos.cantidad = vpedido.cantidad;
                        pedidos.tipo_carroceria = vpedido.tipo_carroceria;
                        pedidos.vrcarroceria = Convert.ToDecimal(vpedido.vrcarroceria);
                        pedidos.vrtotal = Convert.ToDecimal(vpedido.vrtotal);
                        pedidos.moneda = vpedido.moneda;
                        pedidos.id_aseguradora = vpedido.id_aseguradora;
                        pedidos.notas1 = vpedido.notas1;
                        pedidos.notas2 = vpedido.notas2;
                        pedidos.escanje = vpedido.escanje;
                        pedidos.eschevyplan = vpedido.eschevyplan;
                        pedidos.esLeasing = vpedido.esLeasing;
                        pedidos.esreposicion = vpedido.esreposicion;
                        pedidos.nit_prenda = vpedido.nit_prenda;
                        pedidos.flota = vpedido.flota;
                        pedidos.codigoflota = vpedido.codflota;
                        pedidos.facturado = vpedido.facturado;
                        pedidos.numfactura = vpedido.numfactura;
                        pedidos.porcentaje_impoconsumo = vpedido.porcentaje_impoconsumo;
                        pedidos.numeroplaca = vpedido.numeroplaca;
                        pedidos.motivo_anulacion = vpedido.motivo_anulacion;
                        pedidos.venta_gerencia = vpedido.venta_gerencia;
                        pedidos.Color_Deseado = vpedido.Color_Deseado;
                        pedidos.terminacionplaca = vpedido.terminacionplaca;
                        pedidos.bono = vpedido.bono;
                        pedidos.idmodelo = vpedido.idmodelo;
                        pedidos.nuevo = vpedido.nuevo;
                        pedidos.usado = vpedido.usado;
                        pedidos.servicio = vpedido.servicio;
                        pedidos.placapar = vpedido.placapar;
                        pedidos.placaimpar = vpedido.placaimpar;
                        pedidos.color_opcional = vpedido.color_opcional;
                        pedidos.cargomatricula = vpedido.cargomatricula;
                        pedidos.obsequioporcen = vpedido.obsequioporcen;
                        pedidos.valormatricula = Convert.ToDecimal(vpedido.valormatricula);
                        pedidos.rango_placa = vpedido.rango_placa;
                        pedidos.user_idactualizacion = vpedido.user_idactualizacion;
                        pedidos.fec_actualizacion = vpedido.fec_actualizacion;
                        pedidos.userid_creacion = vpedido.userid_creacion;
                        pedidos.fec_creacion = vpedido.fec_creacion;
                        pedidos.iddepartamento = vpedido.iddepartamento;
                        pedidos.idciudad = vpedido.idciudad;
                        pedidos.valorsoat = Convert.ToDecimal(vpedido.valorsoat);
                        pedidos.otrosValores = Convert.ToDecimal(vpedido.otrosValores);
                        #endregion

                        db.Entry(pedidos).State = EntityState.Modified;

                        var pedido_id = pedidos.id;

                        #region pagos
                        var pagos = Request["lista_pagos"];
                        if (!String.IsNullOrEmpty(pagos))
                        {
                            var lista_pagos = Convert.ToInt32(Request["lista_pagos"]);
                            for (int i = 1; i <= lista_pagos; i++)
                            {
                                if (!string.IsNullOrEmpty(Request["condicion" + i]))
                                {
                                    vpedpago vpago = new vpedpago();
                                    vpago.idpedido = pedido_id;
                                    vpago.seq = i;
                                    vpago.condicion = Convert.ToInt32(Request["condicion" + i]);
                                    vpago.valor = Convert.ToDecimal(Request["valor" + i]);
                                    vpago.fecpago = Convert.ToDateTime(Request["fecpago" + i]);
                                    vpago.observaciones = Request["observaciones" + i];
                                    var banco = Request["banco" + i];
                                    if (!string.IsNullOrEmpty(Request["banco" + i]))
                                    {
                                        vpago.banco = Convert.ToInt32(Request["banco" + i]);
                                    }
                                    db.vpedpago.Add(vpago);
                                }
                            }
                        }
                        #endregion
                        #region repuestos
                        var lrp = Request["lista_repuestos"];
                        if (!String.IsNullOrEmpty(lrp))
                        {
                            var lista_repuestos = Convert.ToInt32(Request["lista_repuestos"]);
                            for (int j = 1; j <= lista_repuestos; j++)
                            {
                                if (!String.IsNullOrEmpty(Request["repuestos" + j]))
                                {

                                    vpedrepuestos vrepuesto = new vpedrepuestos();
                                    vrepuesto.pedido_id = pedido_id;
                                    vrepuesto.referencia = Request["repuestos" + j];
                                    vrepuesto.vrunitario = Convert.ToDecimal(Request["costo" + j]);
                                    vrepuesto.vrtotal = Convert.ToDecimal(Request["totalRespuesto" + j]);
                                    vrepuesto.obsequio = Convert.ToBoolean(Request["obsequio" + j]);
                                    vrepuesto.cantidad = Convert.ToInt32(Request["cantidadRespuesto" + j]);
                                    //vrepuesto.obsequio = Convert.ToBoolean(Request["obsequio" + j]);
                                    //vrepuesto.obsequio = Convert.ToBoolean(vrepuesto.obsequio == true ? "Si" : "No");
                                    db.vpedrepuestos.Add(vrepuesto);
                                }
                            }
                        }
                        #endregion
                        #region retomas
                        var lr = Request["lista_retomas"];
                        if (!String.IsNullOrEmpty(lr))
                        {
                            var lista_retomas = Convert.ToInt32(Request["lista_retomas"]);
                            for (int i = 1; i <= lista_retomas; i++)
                            {
                                if (!string.IsNullOrEmpty(Request["valor_retoma" + i]))
                                {
                                    vpedretoma retoma = new vpedretoma();
                                    retoma.pedido_id = pedido_id;
                                    retoma.placa = Request["placa_retoma" + i];
                                    retoma.valor = Convert.ToDecimal(Request["valor_retoma" + i]);
                                    retoma.modelo = Request["modelo_retoma" + i];
                                    if (!string.IsNullOrEmpty(Request["kl_retoma" + i]))
                                    {
                                        retoma.kilometraje = Convert.ToDecimal(Request["kl_retoma" + i]);
                                    }
                                    if (!string.IsNullOrEmpty(Request["obligacion_retoma" + i]))
                                    {
                                        retoma.obligaciones = Convert.ToBoolean(Request["obligacion_retoma" + i]);
                                        if (!string.IsNullOrEmpty(Request["valor_obligacion" + i]))
                                        {
                                            retoma.valor_obligacion = Convert.ToDecimal(Request["valor_obligacion" + i]);
                                        }
                                    }
                                    else
                                    {
                                        retoma.obligaciones = false;
                                    }

                                    db.vpedretoma.Add(retoma);
                                }
                            }
                        }
                        #endregion
                        #region costos
                        var costo = Request["lista_costos"];
                        if (!String.IsNullOrEmpty(costo))
                        {
                            var lista_costos = Convert.ToInt32(Request["lista_costos"]);
                            for (int j = 1; j <= lista_costos; j++)
                            {
                                if (!String.IsNullOrEmpty(Request["descripcion_costo" + j]))
                                {
                                    vpedcostos_adicionales costos = new vpedcostos_adicionales();
                                    costos.pedido_id = pedido_id;
                                    costos.descripcion = Request["descripcion_costo" + j];
                                    if (!string.IsNullOrEmpty(Request["valor_costo" + j]))
                                    {
                                        costos.valor = Convert.ToDecimal(Request["valor_costo" + j]);
                                    }
                                    if (!string.IsNullOrEmpty(Request["obsequio_costo" + j]))
                                    {
                                        costos.obsequio = Convert.ToBoolean(Request["obsequio_costo" + j]);
                                    }
                                    else
                                    {
                                        costos.obsequio = false;
                                    }
                                    db.vpedcostos_adicionales.Add(costos);
                                }
                            }
                        }
                        #endregion

                        #region Cambio Vehiculo
                        var cambio = Convert.ToInt32(Request["esCambio"]);
                        if (cambio == 1)
                        {
                            var creditoExiste = db.v_creditos.Where(x => x.pedido == pedido_id);
                            if (creditoExiste != null)
                            {
                                foreach (var item in creditoExiste)
                                {
                                    item.vehiculo = vpedido.modelosMarca;
                                    db.Entry(item).State = EntityState.Modified;
                                }
                            }

                            pedidos.marca = Convert.ToInt32(vpedido.marcas);
                            pedidos.modelo = vpedido.modelosMarca;
                            pedidos.id_anio_modelo = vpedido.idAnioModelo;
                            db.Entry(pedidos).State = EntityState.Modified;

                            vcambiovehiculo cambioVH = new vcambiovehiculo();
                            cambioVH.idpedido = vpedido.id;
                            cambioVH.idmarca = Convert.ToInt32(vpedido.marcvh_id);
                            cambioVH.idanomodelo = Convert.ToInt32(vpedido.id_anio_modelo);
                            cambioVH.motivo = vpedido.motivoCambio;
                            cambioVH.iduser = Convert.ToInt32(Session["user_usuarioid"]);
                            cambioVH.feccreacion = DateTime.Now;

                            db.vcambiovehiculo.Add(cambioVH);
                            //db.SaveChanges();
                        }
                        #endregion

                        var result = db.SaveChanges();
                        if (result > 0)
                        {
                            TempData["mensaje"] = "Pedido editado correctamente";
                            dbTran.Commit();
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error al editar el pedido, por favor intente nuevamente";
                            dbTran.Rollback();
                        }
                    }
                    catch (DbEntityValidationException ex)
                    {
                        dbTran.Rollback();
                        throw;
                    }
                }
            }
            #region variables pedido
            VehiculoPedidoModel pedido = new VehiculoPedidoModel();
            pedido.id = vpedido.id;
            pedido.numero = vpedido.numero;
            pedido.impfactura2 = vpedido.impfactura2;
            pedido.impfactura3 = vpedido.impfactura3;
            pedido.impfactura4 = vpedido.impfactura4;
            pedido.bodega = vpedido.bodega;
            pedido.anulado = vpedido.anulado;
            pedido.fecha = Convert.ToDateTime(vpedido.fecha);
            pedido.idcotizacion = vpedido.idcotizacion;
            pedido.nit = vpedido.nit;
            pedido.nit_asegurado = vpedido.nit_asegurado;
            pedido.nit2 = vpedido.nit2;
            pedido.nit3 = vpedido.nit3;
            pedido.nit4 = vpedido.nit4;
            pedido.nit5 = vpedido.nit5;
            pedido.vendedor = vpedido.vendedor;
            pedido.modelo = vpedido.modelo;
            pedido.id_anio_modelo = vpedido.id_anio_modelo;
            pedido.plan_venta = vpedido.plan_venta;
            pedido.planmayor = vpedido.planmayor;
            pedido.fecha_asignacion_planmayor = vpedido.fecha_asignacion_planmayor;
            pedido.asignado_por = vpedido.asignado_por;
            pedido.condicion = vpedido.condicion;
            pedido.dias_validez = vpedido.dias_validez;
            pedido.valor_unitario = !string.IsNullOrWhiteSpace(vpedido.valor_unitario) ? Convert.ToDecimal(vpedido.valor_unitario).ToString("N0", new CultureInfo("is-IS")) : "0";
            pedido.porcentaje_iva = vpedido.porcentaje_iva;
            pedido.valorPoliza = !string.IsNullOrWhiteSpace(vpedido.valorPoliza) ? Convert.ToDecimal(vpedido.valorPoliza).ToString("N0", new CultureInfo("is-IS")) : "0";
            pedido.pordscto = vpedido.pordscto;
            pedido.vrdescuento = !string.IsNullOrWhiteSpace(vpedido.vrdescuento) ? Convert.ToDecimal(vpedido.vrdescuento).ToString("N0", new CultureInfo("is-IS")) : "0";
            pedido.cantidad = vpedido.cantidad;
            pedido.tipo_carroceria = vpedido.tipo_carroceria;
            pedido.vrcarroceria = !string.IsNullOrWhiteSpace(vpedido.vrcarroceria) ? Convert.ToDecimal(vpedido.vrcarroceria).ToString("N0", new CultureInfo("is-IS")) : "0";
            pedido.vrtotal = !string.IsNullOrWhiteSpace(vpedido.vrtotal) ? Convert.ToDecimal(vpedido.vrtotal).ToString("N0", new CultureInfo("is-IS")) : "0";
            pedido.moneda = vpedido.moneda;
            pedido.id_aseguradora = vpedido.id_aseguradora;
            pedido.notas1 = vpedido.notas1;
            pedido.notas2 = vpedido.notas2;
            pedido.escanje = vpedido.escanje;
            pedido.eschevyplan = vpedido.eschevyplan;
            pedido.esLeasing = vpedido.esLeasing;
            pedido.esreposicion = vpedido.esreposicion;
            pedido.nit_prenda = vpedido.nit_prenda;
            pedido.flota = vpedido.flota;
            pedido.codflota = vpedido.codflota;
            pedido.facturado = vpedido.facturado;
            pedido.numfactura = vpedido.numfactura;
            pedido.porcentaje_impoconsumo = vpedido.porcentaje_impoconsumo;
            pedido.numeroplaca = vpedido.numeroplaca;
            pedido.motivo_anulacion = vpedido.motivo_anulacion;
            pedido.venta_gerencia = vpedido.venta_gerencia;
            pedido.Color_Deseado = vpedido.Color_Deseado;
            pedido.terminacionplaca = vpedido.terminacionplaca;
            pedido.bono = vpedido.bono;
            pedido.marcvh_id = Convert.ToString(vpedido.marcvh_id);
            pedido.idmodelo = vpedido.idmodelo;
            pedido.nuevo = vpedido.nuevo;
            pedido.usado = vpedido.usado;
            pedido.servicio = vpedido.servicio;
            pedido.placapar = vpedido.placapar;
            pedido.placaimpar = vpedido.placaimpar;
            pedido.color_opcional = vpedido.color_opcional;
            pedido.cargomatricula = vpedido.cargomatricula;
            pedido.obsequioporcen = vpedido.obsequioporcen;
            pedido.valormatricula = Convert.ToString(vpedido.valormatricula);
            pedido.rango_placa = vpedido.rango_placa;
            pedido.user_idactualizacion = vpedido.user_idactualizacion;
            pedido.fec_actualizacion = vpedido.fec_actualizacion;
            pedido.userid_creacion = vpedido.userid_creacion;
            pedido.fec_creacion = vpedido.fec_creacion;
            #endregion

            listas(pedido);
            BuscarFavoritos(menu);
            //return RedirectToAction("Edit", new { menu , vpedido.id });
            return View(pedido);
        }

        public JsonResult BuscarAniosModelos(string codigoModelo, bool esNuevo)
        {
            if (esNuevo)
            {
                var anios = db.anio_modelo.Where(x => x.codigo_modelo == codigoModelo).ToList();
                var result = anios.Select(x => new
                {
                    text = x.anio,
                    value = x.anio_modelo_id
                });
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var fechaActual = DateTime.Now;
                var anios = db.vw_referencias_total.Where(x => x.modvh_id == codigoModelo && x.ano == fechaActual.Year && x.mes == fechaActual.Month).Select(x => new
                {
                    anios = x.anio_vh,
                    codigo = x.codigo
                }).Distinct();
                return Json(anios, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult BuscarPrecioXAniosModelos(int anioModelo)
        {
            var data = db.anio_modelo.FirstOrDefault(x => x.anio_modelo_id == anioModelo).precio;
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Seguimiento(int id, int? menu)
        {
            infoPedido(id);

            var pedido_id = db.vpedido.Find(id).id;
            vpedseguimiento seguimiento = new vpedseguimiento();
            seguimiento.pedidoid = pedido_id;

            ViewBag.tipo = new SelectList(db.vtiposeguimientocot, "id_tipo_seguimiento", "nombre_seguimiento");
            BuscarFavoritos(menu);
            return View(seguimiento);
        }

        public void infoPedido(int id)
        {
            var infoPedido = (from p in db.vpedido
                              join c in db.icb_cotizacion
                              on p.idcotizacion equals c.cot_idserial into tmp
                              from c in tmp.DefaultIfEmpty()
                              join t in db.icb_terceros
                              on p.nit equals t.tercero_id into temp
                              from t in temp.DefaultIfEmpty()
                              where p.id == id
                              select new
                              {
                                  p.numero,
                                  c.cot_numcotizacion,
                                  t.doc_tercero,
                                  tercero = t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero + " " + t.segapellido_tercero,
                                  t.telf_tercero,
                                  t.email_tercero,
                                  t.celular_tercero,
                              }).FirstOrDefault();

            if (infoPedido != null)
            {
                ViewBag.numPedido = infoPedido.numero != null ? infoPedido.numero : 0;
                ViewBag.numCotizacion = infoPedido.cot_numcotizacion != null ? infoPedido.cot_numcotizacion : 0;
                ViewBag.idTercero = infoPedido.doc_tercero != null ? infoPedido.doc_tercero : "";
                ViewBag.tercero = infoPedido.tercero != null ? infoPedido.tercero : "";
                ViewBag.telefono = infoPedido.telf_tercero != null ? infoPedido.telf_tercero : "";
                ViewBag.email = infoPedido.email_tercero != null ? infoPedido.email_tercero : "";
                ViewBag.celular = infoPedido.celular_tercero != null ? infoPedido.celular_tercero : "";
            }
        }

        [HttpPost]
        public ActionResult Seguimiento(vpedseguimiento seguimiento, int? menu)
        {
            seguimiento.fecha = DateTime.Now;
            seguimiento.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
            seguimiento.responsable = Convert.ToInt32(Session["user_usuarioid"]);
            db.vpedseguimiento.Add(seguimiento);
            var result = db.SaveChanges();
            if (result > 0)
            {
                TempData["mensaje"] = "Nota agregada correctamente";
                return RedirectToAction("Seguimiento", new { id = seguimiento.pedidoid, menu = menu });
            }
            else
            {
                TempData["mensaje_error"] = "Error al agregar la nota, por favor intente nuevamente";
            }


            ViewBag.tipo = new SelectList(db.vtiposeguimientocot, "id_tipo_seguimiento", "nombre_seguimiento");
            BuscarFavoritos(menu);
            return View(seguimiento);
        }

        public ActionResult Delete(int id, int? menu, int motivo, string observacion)
        {
            //string QueryRepuestos = "Delete from vpedrepuestos where pedido_id ="+id;
            //db.Database.ExecuteSqlCommand(QueryRepuestos);

            //string QueryPagos = "Delete from vpedpago where idpedido =" + id;
            //db.Database.ExecuteSqlCommand(QueryPagos);

            //string QueryRetomas = "Delete from vpedretoma where pedido_id =" + id;
            //db.Database.ExecuteSqlCommand(QueryRetomas);

            //string QuerySeguimiento = "Delete from vpedseguimiento where pedidoid =" + id;
            //db.Database.ExecuteSqlCommand(QuerySeguimiento);
            //db.SaveChanges();

            vpedido vpedidos = db.vpedido.Find(id);
            vpedidos.anulado = true;
            vpedidos.idanulacion = motivo;
            vpedidos.motivo_anulacion = observacion;
            //db.vpedido.Remove(vpedido);
            db.Entry(vpedidos).State = EntityState.Modified;

            db.SaveChanges();

            TempData["mensaje"] = "Pedido anulado Correctamente";
            return RedirectToAction("Edit", new { id = id, menu });

        }

        public JsonResult BuscarSeguimientos(int id)
        {
            var data = (from seguimiento in db.vpedseguimiento
                        join tipo in db.vtiposeguimientocot
                        on seguimiento.tipo equals tipo.id_tipo_seguimiento
                        join usuario in db.users
                        on seguimiento.responsable equals usuario.user_id
                        where seguimiento.pedidoid == id
                        select new
                        {
                            tipo.nombre_seguimiento,
                            fec_creacion = seguimiento.fecha.ToString(),
                            responsable = usuario.user_nombre + " " + usuario.user_apellido,
                            seguimiento.motivo,
                            seguimiento.nota
                        });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarCotizacion(int cotizacion)
        {
            var cot = from c in db.icb_cotizacion
                      where c.cot_idserial == cotizacion
                      select new
                      {
                          c.cot_numcotizacion,
                          c.id_tercero,
                          notas1 = c.cot_observacion,
                          asesor = c.asesor
                      };

            var vh = (from v in db.vcotdetallevehiculo
                      join m in db.modelo_vehiculo
                      on v.idmodelo equals m.modvh_codigo
                      join c in db.color_vehiculo
                      on v.color equals c.colvh_id
                      join a in db.anio_modelo
                      on v.anomodelo equals a.anio_modelo_id
                      where v.idcotizacion == cotizacion
                      select new
                      {
                          idmodelo = m.modvh_codigo,
                          modelo = m.modvh_nombre,
                          v.precio,
                          a.anio,
                          color = c.colvh_nombre,
                          v.soat,
                          v.matricula
                      }).ToList();

            var rpt = from r in db.vcotrepuestos
                      join rf in db.icb_referencia
                      on r.referencia equals rf.ref_codigo
                      where r.cot_id == cotizacion
                      select new
                      {

                          codigo = rf.ref_codigo,
                          nombre = rf.ref_descripcion,
                          valor = r.vrunitario,
                          cantidad = r.cantidad
                      };

            var ret = (from r in db.vcotretoma
                       where r.idcot == cotizacion
                       select new
                       {
                           valor_retoma = r.valor,
                           placa_retoma = r.placa,
                           modelo_retoma = r.modelo,
                           kl_retoma = r.Kilometraje,
                       }).ToList();

            var data = new
            {
                cot,
                vh,
                rpt,
                ret
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarCreditos(int idTercero)
        {
            var info = (from c in db.v_creditos
                        join i in db.vinfcredito
                        on c.infocredito_id equals i.id
                        join t in db.icb_terceros
                        on i.tercero equals t.tercero_id
                        join e in db.estados_credito
                        on c.estadoc equals e.codigo
                        where i.tercero == idTercero && (c.estadoc == "T" || c.estadoc == "A" || c.estadoc == "D" || c.estadoc == "C")
                        select new
                        {
                            c.Id,
                            c.financiera_id,
                            c.fec_solicitud,
                            tercero = t.prinom_tercero != null ? "(" + t.doc_tercero + ") " + t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero + " " + t.segnom_tercero : "(" + t.doc_tercero + ") " + t.razon_social,
                            e.codigo,
                            e.descripcion,
                            vsolicitado = c.vsolicitado,
                            vaprobado = c.vaprobado,
                        }).ToList();

            var data = info.Select(x => new
            {
                x.Id,
                x.financiera_id,
                x.tercero,
                x.codigo,
                fecha = x.fec_solicitud.Value.ToString("yyyy/MM/dd"),
                x.descripcion,
                vsolicitado = String.IsNullOrEmpty(x.vsolicitado.ToString()) ? "0" : x.vsolicitado.Value.ToString("0,0", elGR),
                vaprobado = String.IsNullOrEmpty(x.vaprobado.ToString()) ? "0" : x.vaprobado.Value.ToString("0,0", elGR),
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarCliente(int cliente)
        {
            var buscarCliente = (from t in db.icb_terceros
                                 join c in db.tercero_cliente
                                 on t.tercero_id equals c.tercero_id
                                 where t.tercero_id == cliente
                                 select new
                                 {
                                     cedula = t.prinom_tercero != null ? t.doc_tercero + " - " + t.prinom_tercero + " " + t.apellido_tercero + " " + t.segapellido_tercero : t.doc_tercero + " - " + t.razon_social,
                                     t.tercero_id
                                 }).FirstOrDefault();

            if (buscarCliente != null)
            {
                var data = from t in db.icb_terceros
                           join c in db.tercero_cliente
                           on t.tercero_id equals c.tercero_id
                           join ci in db.nom_ciudad
                           on t.ciu_id equals ci.ciu_id into tmp
                           from ci in tmp.DefaultIfEmpty()
                           where t.tercero_id == cliente
                           select new
                           {
                               idTercero = t.tercero_id,
                               nombre = t.prinom_tercero != null ? t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero + " " + t.segapellido_tercero : t.razon_social,
                               telefono = t.telf_tercero != null ? t.telf_tercero : "",
                               celular = t.celular_tercero != null ? t.celular_tercero : "",
                               correo = t.email_tercero != null ? t.email_tercero : "",
                               ciudad = ci.ciu_nombre,
                               direccion = db.terceros_direcciones.OrderByDescending(x => x.id).FirstOrDefault(x => x.idtercero == t.tercero_id).direccion
                           };
                return Json(new { info = data, cliente = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { info = "", cliente = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult BuscarClienteCedula(string cliente)
        {
            var idTercero = db.icb_terceros.FirstOrDefault(x => x.doc_tercero == cliente).tercero_id;

            var info = from t in db.icb_terceros
                       join c in db.tercero_cliente
                       on t.tercero_id equals c.tercero_id
                       where t.doc_tercero == cliente
                       select new
                       {
                           tipo = t.tpdoc_id,
                           nombre = t.prinom_tercero != null ? t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero + " " + t.segapellido_tercero : t.razon_social,
                           telefono = t.telf_tercero != null ? t.telf_tercero : "",
                           celular = t.celular_tercero != null ? t.celular_tercero : "",
                           correo = t.email_tercero != null ? t.email_tercero : "",
                           direccion = db.terceros_direcciones.OrderByDescending(x => x.id).FirstOrDefault(x => x.idtercero == t.tercero_id).direccion,
                           ciudad = (from d in db.terceros_direcciones
                                     join c in db.nom_ciudad
                                     on d.ciudad equals c.ciu_id
                                     where d.idtercero == idTercero
                                     orderby d.id descending
                                     select
                                         c.ciu_nombre
                                    ).FirstOrDefault()
                       };

            var tipoPer = from t in db.icb_terceros
                          join d in db.tp_documento
                          on t.tpdoc_id equals d.tpdoc_id
                          where t.doc_tercero == cliente
                          select new
                          {
                              d.tipo
                          };

            var data = new
            {
                info,
                tipoPer
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarVh(string modelo, int cotizacion)
        {
            var data = from v in db.vcotdetallevehiculo
                       join m in db.modelo_vehiculo
                       on v.idmodelo equals m.modvh_codigo
                       join mar in db.marca_vehiculo
                       on m.mar_vh_id equals mar.marcvh_id
                       join c in db.color_vehiculo
                       on v.color equals c.colvh_id
                       join vh in db.anio_modelo
                       on v.idmodelo equals vh.codigo_modelo
                       where m.modvh_codigo == modelo
                       && v.idcotizacion == cotizacion
                       select new
                       {
                           v.nuevo,
                           v.usado,
                           v.color,
                           v.precio,
                           tipo_vh = v.nuevo != true ? "Usado" : "Nuevo",
                           v.anomodelo,
                           m.modvh_codigo,
                           poliza = v.poliza != null ? v.poliza : 0,
                           matricula = v.matricula != null ? v.matricula : 0,
                           porcentaje_iva = vh.porcentaje_iva != null ? vh.porcentaje_iva : 0,
                           impuesto_consumo = vh.impuesto_consumo != null ? vh.impuesto_consumo : 0,
                           mar.marcvh_id,
                           soat = v.soat != null ? v.soat : 0
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPagos()
        {
            var parametro = db.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P62").syspar_value;

            var pagos = from p in db.vformapago
                        where p.estado == true
                        orderby p.descripcion
                        select new
                        {
                            p.id,
                            p.descripcion
                        };

            var bancos = from b in db.icb_unidad_financiera
                         where b.financiera_estado == true
                         orderby b.financiera_nombre
                         select new
                         {
                             id = b.financiera_id,
                             b.financiera_nombre
                         };

            var data = new
            {
                parametro,
                pagos,
                bancos
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDatos()
        {
            var user = Convert.ToInt32(Session["user_usuarioid"]);

            var rolUsurario = (from u in db.users
                               join r in db.rols
                               on u.rol_id equals r.rol_id
                               where u.user_id == user
                               select
                                   r.rol_id
                               ).FirstOrDefault();

            if (rolUsurario == 4)
            {
                var data = from p in db.vpedido
                           join b in db.bodega_concesionario
                           on p.bodega equals b.id
                           join u in db.users
                           on p.vendedor equals u.user_id
                           join m in db.modelo_vehiculo
                           on p.modelo equals m.modvh_codigo
                           join t in db.icb_terceros
                           on p.nit equals t.tercero_id
                           join c in db.icb_cotizacion
                           on p.idcotizacion equals c.cot_idserial into ps3
                           from c in ps3.DefaultIfEmpty()
                           join tpOrigen in db.tp_origen
                           on c.cot_origen equals tpOrigen.tporigen_id into ps1
                           from tpOrigen in ps1.DefaultIfEmpty()
                           join tpSubFuente in db.tp_subfuente
                           on c.id_subfuente equals tpSubFuente.id into ps2
                           from tpSubFuente in ps2.DefaultIfEmpty()
                           where p.vendedor == user
                           select new
                           {
                               p.id,
                               p.numero,
                               modelo = m.modvh_nombre,
                               bodega = b.bodccs_nombre,
                               asesor = u.user_nombre + " " + u.user_apellido,
                               //p.vrtotal,
                               p.nit,
                               fuente = tpOrigen.tporigen_nombre != null ? tpOrigen.tporigen_nombre : "",
                               subfuente = tpSubFuente.subfuente != null ? tpSubFuente.subfuente : "",
                               doc_tercero = "(" + t.doc_tercero + ") " + t.prinom_tercero + " " + t.apellido_tercero + " " + t.razon_social,
                               fecha = p.fecha.ToString(),
                               facturado = p.facturado == true ? "Si" : "No",
                               anulado = p.anulado == true ? "Si" : "No",
                               numfactura = p.numfactura != null ? p.numfactura.ToString() : " ",
                               planmayor = p.planmayor != null ? p.planmayor.ToString() : "No asignado",
                               fecha_asignacion_planmayor = p.fecha_asignacion_planmayor.Value.ToString()

                           };

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var data = from p in db.vpedido
                           join b in db.bodega_concesionario
                           on p.bodega equals b.id
                           join u in db.users
                           on p.vendedor equals u.user_id
                           join m in db.modelo_vehiculo
                           on p.modelo equals m.modvh_codigo
                           join t in db.icb_terceros
                           on p.nit equals t.tercero_id
                           join c in db.icb_cotizacion
                           on p.idcotizacion equals c.cot_idserial into ps3
                           from c in ps3.DefaultIfEmpty()
                           join tpOrigen in db.tp_origen
                           on c.cot_origen equals tpOrigen.tporigen_id into ps1
                           from tpOrigen in ps1.DefaultIfEmpty()
                           join tpSubFuente in db.tp_subfuente
                           on c.id_subfuente equals tpSubFuente.id into ps2
                           from tpSubFuente in ps2.DefaultIfEmpty()
                           select new
                           {
                               p.id,
                               p.numero,
                               modelo = m.modvh_nombre,
                               bodega = b.bodccs_nombre,
                               asesor = u.user_nombre + " " + u.user_apellido,
                               //p.vrtotal,
                               p.nit,
                               fuente = tpOrigen.tporigen_nombre != null ? tpOrigen.tporigen_nombre : "",
                               subfuente = tpSubFuente.subfuente != null ? tpSubFuente.subfuente : "",
                               doc_tercero = "(" + t.doc_tercero + ") " + t.prinom_tercero + " " + t.apellido_tercero + " " + t.razon_social,
                               fecha = p.fecha.ToString(),
                               facturado = p.facturado == true ? "Si" : "No",
                               anulado = p.anulado == true ? "Si" : "No",
                               numfactura = p.numfactura != null ? p.numfactura.ToString() : " ",
                               planmayor = p.planmayor != null ? p.planmayor.ToString() : "No asignado",
                               fecha_asignacion_planmayor = p.fecha_asignacion_planmayor.Value.ToString()

                           };

                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult BuscarPagosyAccesorios(int pedido)
        {
            var pagos = (from p in db.vpedpago
                         join f in db.vformapago
                         on p.condicion equals f.id
                         join b in db.icb_unidad_financiera
                         on p.banco equals b.financiera_id
                         into ps
                         from b in ps.DefaultIfEmpty()
                         where p.idpedido == pedido
                         select new
                         {
                             condicion = f.descripcion,
                             p.valor,
                             fecpago = p.fecpago.ToString(),
                             financiera_nombre = b.financiera_nombre != null ? b.financiera_nombre : "",
                             p.observaciones,
                             id = p.id
                         }).ToList();

            var rpt = (from r in db.vpedrepuestos
                       join a in db.icb_referencia
                       on r.referencia equals a.ref_codigo
                       where r.pedido_id == pedido
                       select new
                       {
                           codigo = a.ref_codigo,
                           referencia = a.ref_descripcion,
                           valor = r.vrtotal,
                           cantidad = r.cantidad,
                           obsequio = r.obsequio == true ? "Si" : "No",
                           id = r.id
                       }).ToList();

            var rt = (from r in db.vpedretoma
                      where r.pedido_id == pedido
                      select new
                      {
                          modelo = r.modelo,
                          placa = r.placa,
                          kilometraje = r.kilometraje,
                          r.valor,
                          // = r.obligaciones != null ? r.obligaciones == true ? "Si" : "No" : "No",
                          obligaciones = r.obligaciones == true ? "Si" : "No",
                          valor_obligacion = r.valor_obligacion != null ? r.valor_obligacion : 0,
                          id = r.id
                      }).ToList();

            var cad = (from c in db.vpedcostos_adicionales
                       where c.pedido_id == pedido
                       select new
                       {
                           cadDes = c.descripcion,
                           //cadObsequio = c.obsequio != null ? c.obsequio == true ? "Si":"No":"No",
                           cadObsequio = c.obsequio == true ? "Si" : "No",
                           cadValor = c.valor,
                           cadID = c.idcosto
                       }).ToList();

            var pagosr = (from p in db.crucedoc_pagos_recibidos
                          join f in db.tipopagorecibido
                          on p.fpago_recibido equals f.id
                          join c in db.cruce_documentos
                          on p.crucedoc_id equals c.id
                          where p.pedido_id == pedido
                          select new
                          {
                              f.pago,
                              p.valor,
                              c.fechacruce
                          }).ToList();

            var data = new
            {
                pagos,
                rpt,
                rt,
                cad,
                pagosr
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarVhDisponible(string modelo_id, string usado, int anio)
        {
            var par = "P34";
            var valorParametro = db.icb_sysparameter.Where(x => x.syspar_cod == par).Select(x => x.syspar_value).FirstOrDefault();

            var valorPar = Convert.ToInt32(valorParametro);

            var usuario = Convert.ToInt32(Session["user_usuarioid"]);
            var anioFinal = db.anio_modelo.FirstOrDefault(x => x.anio_modelo_id == anio).anio;
            //Validamos que el usuario loguado tenga el rol y el permiso para ver toda la informacion
            var permiso = (from u in db.users
                           join r in db.rols
                                on u.rol_id equals r.rol_id
                           join ra in db.rolacceso
                                on r.rol_id equals ra.idrol
                           where u.user_id == usuario && ra.idpermiso == 3
                           select new
                           {
                               u.user_id,
                               u.rol_id,
                               r.rol_nombre,
                               ra.idpermiso
                           }).Count();
            if (permiso > 0)
            {
                if (usado == "True")
                {
                    var data = (from vh in db.vw_inventario_hoy
                                join v in db.icb_vehiculo
                                on vh.ref_codigo equals v.plan_mayor
                                join b in db.bodega_concesionario
                                on vh.bodega equals b.id
                                join m in db.modelo_vehiculo
                                on v.modvh_id equals m.modvh_codigo into ps
                                from m in ps.DefaultIfEmpty()
                                join s in db.icb_sysparameter
                                on par equals s.syspar_cod
                                join ev in db.icb_vehiculo_eventos
                                on new { pm = v.plan_mayor, sv = valorPar } equals new { pm = ev.planmayor, sv = ev.id_tpevento } into je
                                from ev in je.DefaultIfEmpty()
                                join ub in db.ubicacion_bodega
                                on v.ubicacionactual equals ub.id into xx
                                from ub in xx.DefaultIfEmpty()
                                where v.modvh_id == modelo_id
                                && vh.stock > 0
                                && v.usado == true
                                && v.anio_vh == anioFinal
                                //&& ev.id_tpevento == 15
                                select new
                                {
                                    s.syspar_value,
                                    v.plan_mayor,
                                    color = v.color_vehiculo.colvh_nombre,
                                    bodega_id = vh.bodega,
                                    bodega = b.bodccs_nombre,
                                    modelo = m.modvh_nombre,
                                    anio_modelo = v.anio_vh,
                                    fecha_compra = v.icbvhfec_creacion.ToString(),
                                    //averias = ev.id_tpevento == 15 ? ev.evento_observacion : ""
                                    estado = db.icb_vehiculo_eventos.Where(x => x.id_tpevento == 2).FirstOrDefault() != null ? "Recepcionado" : "En tránsito",
                                    evento = db.icb_vehiculo_eventos.Where(x => x.id_tpevento == 2).FirstOrDefault() != null ? "2" : "1",
                                    ev.fechaevento,
                                    ub.descripcion,
                                    averias = ev.evento_observacion != null ? ev.evento_observacion : "",
                                }).OrderByDescending(x => x.fecha_compra).ToList();

                    var data2 = data.Select(x => new
                    {
                        x.syspar_value,
                        x.plan_mayor,
                        x.color,
                        x.bodega_id,
                        x.bodega,
                        x.modelo,
                        x.anio_modelo,
                        x.fecha_compra,
                        x.estado,
                        dias = x.evento == "2" ? (x.fechaevento - DateTime.Now).Days.ToString() : "En tránsito",
                        ubicacion = x.descripcion != null ? x.descripcion : ""

                    });

                    return Json(data2, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var data = (from vh in db.vw_inventario_hoy
                                join v in db.icb_vehiculo
                                on vh.ref_codigo equals v.plan_mayor
                                join b in db.bodega_concesionario
                                on vh.bodega equals b.id
                                join m in db.modelo_vehiculo
                                on v.modvh_id equals m.modvh_codigo into ps
                                from m in ps.DefaultIfEmpty()
                                join s in db.icb_sysparameter
                                on par equals s.syspar_cod
                                join ev in db.icb_vehiculo_eventos
                                on new { pm = v.plan_mayor, sv = valorPar } equals new { pm = ev.planmayor, sv = ev.id_tpevento } into je
                                from ev in je.DefaultIfEmpty()
                                join ub in db.ubicacion_bodega
                                on ev.ubicacion equals ub.id into xx
                                from ub in xx.DefaultIfEmpty()
                                where v.modvh_id == modelo_id
                                && vh.stock > 0
                                && v.nuevo == true
                                && v.anio_vh == anioFinal
                                //&& ev.id_tpevento == 15
                                select new
                                {
                                    s.syspar_value,
                                    v.plan_mayor,
                                    color = v.color_vehiculo.colvh_nombre,
                                    bodega_id = vh.bodega,
                                    bodega = b.bodccs_nombre,
                                    modelo = m.modvh_nombre,
                                    anio_modelo = v.anio_vh,
                                    fecha_compra = v.icbvhfec_creacion.ToString(),
                                    //averias = ev.id_tpevento == 15 ? ev.evento_observacion : ""
                                    estado = db.icb_vehiculo_eventos.Where(x => x.id_tpevento == 2).FirstOrDefault() != null ? "Recepcionado" : "En tránsito",
                                    evento = db.icb_vehiculo_eventos.Where(x => x.id_tpevento == 2).FirstOrDefault() != null ? "2" : "1",
                                    fechaevento = db.icb_vehiculo_eventos.Where(x => x.id_tpevento == 2).Select(x => x.fechaevento).FirstOrDefault(),
                                    ub.descripcion,
                                    averias = ev.evento_observacion != null ? ev.evento_observacion : "",
                                }).OrderByDescending(x => x.fecha_compra).ToList();

                    var data2 = data.Select(x => new
                    {
                        x.syspar_value,
                        x.plan_mayor,
                        x.color,
                        x.bodega_id,
                        x.bodega,
                        x.modelo,
                        x.anio_modelo,
                        x.fecha_compra,
                        x.estado,
                        dias = x.evento == "2" ? (x.fechaevento - DateTime.Now).Days.ToString() : "En tránsito",
                        ubicacion = x.descripcion != null ? x.descripcion : ""

                    });

                    return Json(data2, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                if (usado == "True")
                {
                    var data = (from vh in db.vw_inventario_hoy
                                join v in db.icb_vehiculo
                                on vh.ref_codigo equals v.plan_mayor
                                join b in db.bodega_concesionario
                                on vh.bodega equals b.id
                                join m in db.modelo_vehiculo
                                on v.modvh_id equals m.modvh_codigo into ps
                                from m in ps.DefaultIfEmpty()
                                join s in db.icb_sysparameter
                                on par equals s.syspar_cod
                                join ev in db.icb_vehiculo_eventos
                               on new { pm = v.plan_mayor, sv = valorPar } equals new { pm = ev.planmayor, sv = ev.id_tpevento } into je
                                from ev in je.DefaultIfEmpty()
                                join ub in db.ubicacion_bodega
                                on v.ubicacionactual equals ub.id into xx
                                from ub in xx.DefaultIfEmpty()
                                where v.modvh_id == modelo_id
                               && vh.stock > 0
                               && v.usado == true
                               && vh.bodega == bodegaActual
                               && v.anio_vh == anioFinal
                                //&& ev.id_tpevento == 15
                                select new
                                {
                                    s.syspar_value,
                                    v.plan_mayor,
                                    color = v.color_vehiculo.colvh_nombre,
                                    bodega_id = vh.bodega,
                                    bodega = b.bodccs_nombre,
                                    modelo = m.modvh_nombre,
                                    anio_modelo = v.anio_vh,
                                    fecha_compra = v.icbvhfec_creacion.ToString(),
                                    //averias = ev.id_tpevento == 15 ? ev.evento_observacion : ""
                                    estado = db.icb_vehiculo_eventos.Where(x => x.id_tpevento == 2).FirstOrDefault() != null ? "Recepcionado" : "En tránsito",
                                    evento = db.icb_vehiculo_eventos.Where(x => x.id_tpevento == 2).FirstOrDefault() != null ? "2" : "1",
                                    ev.fechaevento,
                                    ub.descripcion,
                                    averias = ev.evento_observacion != null ? ev.evento_observacion : "",
                                }).OrderByDescending(x => x.fecha_compra).ToList();

                    var data2 = data.Select(x => new
                    {
                        x.syspar_value,
                        x.plan_mayor,
                        x.color,
                        x.bodega_id,
                        x.bodega,
                        x.modelo,
                        x.anio_modelo,
                        x.fecha_compra,
                        x.estado,
                        dias = x.evento == "2" ? (x.fechaevento - DateTime.Now).Days.ToString() : "En tránsito",
                        ubicacion = x.descripcion != null ? x.descripcion : ""

                    });

                    return Json(data2, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    var data = (from vh in db.vw_inventario_hoy
                                join v in db.icb_vehiculo
                                on vh.ref_codigo equals v.plan_mayor
                                join b in db.bodega_concesionario
                                on vh.bodega equals b.id
                                join m in db.modelo_vehiculo
                                on v.modvh_id equals m.modvh_codigo into ps
                                from m in ps.DefaultIfEmpty()
                                join s in db.icb_sysparameter
                                on par equals s.syspar_cod
                                join ev in db.icb_vehiculo_eventos
                                on new { pm = v.plan_mayor, sv = valorPar } equals new { pm = ev.planmayor, sv = ev.id_tpevento } into je
                                from ev in je.DefaultIfEmpty()
                                join ub in db.ubicacion_bodega
                                on v.ubicacionactual equals ub.id into xx
                                from ub in xx.DefaultIfEmpty()
                                where v.modvh_id == modelo_id
                                && vh.stock > 0
                                && v.nuevo == true
                                && vh.bodega == bodegaActual
                                && v.anio_vh == anioFinal
                                //&& ev.id_tpevento == 15
                                select new
                                {
                                    s.syspar_value,
                                    //ev.id_tpevento,
                                    //v.plan_mayor,
                                    //color = v.color_vehiculo.colvh_nombre,
                                    //bodega_id = vh.bodega,
                                    //bodega = b.bodccs_nombre,
                                    //modelo = m.modvh_nombre,
                                    //anio_modelo = v.anio_vh,
                                    //fecha_compra = v.icbvhfec_creacion.ToString(),
                                    ////averias = ev.id_tpevento == 15 ? ev.evento_observacion : "",
                                    ////averias = v.icb_vehiculo_eventos.FirstOrDefault(x=> x.id_tpevento == 15),
                                    //averias = ev.evento_observacion != null ? ev.evento_observacion : "",

                                    v.plan_mayor,
                                    color = v.color_vehiculo.colvh_nombre,
                                    bodega_id = vh.bodega,
                                    bodega = b.bodccs_nombre,
                                    modelo = m.modvh_nombre,
                                    anio_modelo = v.anio_vh,
                                    fecha_compra = v.icbvhfec_creacion.ToString(),
                                    //averias = ev.id_tpevento == 15 ? ev.evento_observacion : "",
                                    //averias = v.icb_vehiculo_eventos.FirstOrDefault(x=> x.id_tpevento == 15),
                                    //averias = ev.id_tpevento == 15 ? ev.evento_observacion : ""
                                    estado = db.icb_vehiculo_eventos.Where(x => x.id_tpevento == 2).FirstOrDefault() != null ? "Recepcionado" : "En tránsito",
                                    evento = db.icb_vehiculo_eventos.Where(x => x.id_tpevento == 2).FirstOrDefault() != null ? "2" : "1",
                                    ev.fechaevento,
                                    ub.descripcion,
                                    averias = ev.evento_observacion != null ? ev.evento_observacion : "",
                                }).OrderByDescending(x => x.fecha_compra).ToList();

                    var data2 = data.Select(x => new
                    {
                        x.syspar_value,
                        x.plan_mayor,
                        x.color,
                        x.bodega_id,
                        x.bodega,
                        x.modelo,
                        x.anio_modelo,
                        x.fecha_compra,
                        x.estado,
                        dias = x.evento == "2" ? (x.fechaevento - DateTime.Now).Days.ToString() : "En tránsito",
                        ubicacion = x.descripcion != null ? x.descripcion : ""

                    });

                    return Json(data2, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult validarPlanMayor(string plan)
        {
            var data = db.vpedido.FirstOrDefault(x => x.planmayor == plan);
            if (data == null)
            {
                return Json(new { permitir = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { permitir = false, numero = data.numero }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult ValidarAverias(string plan_mayor)
        {
            //P34 parametro en sysparameter para averias
            var valor_parametro = db.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P34").syspar_value;
            var result = 0;

            if (valor_parametro != "0")
            {
                var valor = Convert.ToInt32(valor_parametro);
                var averias = db.icb_vehiculo_eventos.FirstOrDefault(x => x.id_tpevento == valor && x.planmayor == plan_mayor);

                if (averias != null)
                {
                    var autorizacion = db.autorizaciones.FirstOrDefault(x => x.plan_mayor == plan_mayor && x.autorizado == true && x.tipo_autorizacion == 1);
                    if (autorizacion != null)
                    {
                        result = 0;
                    }
                    else
                    {
                        result = 1;
                    }

                    var data = new
                    {
                        result,
                        averias.evento_observacion
                    };

                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    result = 0;
                    var data = new
                    {
                        result,
                    };
                }

            }
            else
            {
                result = 0;
                var data = new
                {
                    result,
                };
                return Json(data, JsonRequestBehavior.AllowGet);

            }


            return Json(JsonRequestBehavior.AllowGet);
        }

        public JsonResult EnviarNotificacionAverias(string plan_mayor)
        {
            var usuario_actual = Convert.ToInt32(Session["user_usuarioid"]);
            var bodega_actual = Convert.ToInt32(Session["user_bodega"]);
            var vh = db.icb_vehiculo.Find(plan_mayor);
            var result = 0;
            var correoconfig = db.configuracion_envio_correos.Where(d => d.activo == true).FirstOrDefault();

            var usuarios_autorizacion = db.usuarios_autorizaciones.FirstOrDefault(x => x.bodega_id == vh.id_bod);
            if (usuarios_autorizacion != null)
            {
                var existe = db.autorizaciones.FirstOrDefault(x => x.plan_mayor == plan_mayor && x.user_autorizacion == usuarios_autorizacion.user_id && x.tipo_autorizacion == 1);

                if (existe == null)
                {
                    var usuario_autorizacion = usuarios_autorizacion.user_id;

                    autorizaciones autorizacion = new autorizaciones();
                    autorizacion.plan_mayor = plan_mayor;
                    autorizacion.user_autorizacion = usuario_autorizacion;
                    autorizacion.user_creacion = usuario_actual;
                    autorizacion.fecha_creacion = DateTime.Now;
                    autorizacion.tipo_autorizacion = 1;
                    autorizacion.bodega = bodega_actual;
                    db.autorizaciones.Add(autorizacion);
                    db.SaveChanges();
                    var autorizacion_id = db.autorizaciones.OrderByDescending(x => x.id).FirstOrDefault(x => x.tipo_autorizacion == 1).id;
                    result = 1;

                    try
                    {
                        var correo_enviado = db.notificaciones.FirstOrDefault(x => x.user_destinatario == usuario_autorizacion && x.enviado != true && x.autorizacion_id == autorizacion_id);
                        if (correo_enviado == null)
                        {
                            var user_destinatario = db.users.Find(usuario_autorizacion);
                            var user_remitente = db.users.Find(usuario_actual);

                            MailAddress de = new MailAddress(correoconfig.correo, "Notificación Iceberg");
                            MailAddress para = new MailAddress(user_destinatario.user_email, user_destinatario.user_nombre + " " + user_destinatario.user_apellido);
                            MailMessage mensaje = new MailMessage(de, para);
                            /*mensaje.Bcc.Add("liliana.avila@exiware.com");
							mensaje.Bcc.Add("marley.vargas@exiware.com");*/
                            mensaje.ReplyToList.Add(new MailAddress(user_remitente.user_email, user_remitente.user_nombre + " " + user_remitente.user_apellido));
                            mensaje.Subject = "Solicitud Autorización plan mayor " + plan_mayor;
                            mensaje.BodyEncoding = System.Text.Encoding.Default;
                            mensaje.IsBodyHtml = true;
                            var html = "";
                            html += "<h4>Cordial Saludo</h4><br>";
                            html += "<p>El usuario " + user_remitente.user_nombre + " " + user_remitente.user_apellido + " solicita autorización para la asignación del "
                                    + " vehículo con plan mayor " + plan_mayor + " por averia </p><br /><br />";
                            html += "Por favor ingrese a la plataforma para dar autorización.";
                            mensaje.Body = html;

                            SmtpClient cliente = new SmtpClient(correoconfig.smtp_server);
                            cliente.Port = correoconfig.puerto;
                            cliente.UseDefaultCredentials = false;
                            cliente.Credentials = new System.Net.NetworkCredential(correoconfig.usuario, correoconfig.password);
                            cliente.EnableSsl = true;
                            cliente.Send(mensaje);

                            notificaciones envio = new notificaciones();
                            envio.user_remitente = usuario_actual;
                            envio.asunto = "Notificación solicitud autorización por averia";
                            envio.fecha_envio = DateTime.Now;
                            envio.enviado = true;
                            envio.user_destinatario = usuario_autorizacion;
                            envio.autorizacion_id = autorizacion_id;
                            db.notificaciones.Add(envio);
                            db.SaveChanges();
                        }

                    }
                    catch (Exception ex)
                    {
                        notificaciones envio = new notificaciones();
                        envio.user_remitente = usuario_actual;
                        envio.asunto = "Notificación solicitud autorización por averia";
                        envio.fecha_envio = DateTime.Now;
                        envio.user_destinatario = usuario_autorizacion;
                        envio.autorizacion_id = autorizacion_id;
                        envio.enviado = false;
                        envio.razon_no_envio = ex.Message;
                        db.notificaciones.Add(envio);
                        db.SaveChanges();
                        //notificacion no enviada
                        result = -1;
                    }
                }
                else
                {
                    // ya existe
                    result = 2;
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EnviarSolicitudFacturacion(string plan_mayor, string modelo)
        {
            var usuario_actual = Convert.ToInt32(Session["user_usuarioid"]);
            var bodega_actual = Convert.ToInt32(Session["user_bodega"]);
            var vh = db.icb_vehiculo.Find(plan_mayor);
            var result = 0;
            var correoconfig = db.configuracion_envio_correos.Where(d => d.activo == true).FirstOrDefault();

            var usuarios_autorizacion = db.usuarios_autorizaciones.FirstOrDefault(x => x.bodega_id == vh.id_bod);
            if (usuarios_autorizacion != null)
            {
                var existe = db.autorizaciones.FirstOrDefault(x => x.plan_mayor == plan_mayor && x.bodega == bodega_actual && x.tipo_autorizacion == 2);

                if (existe == null)
                {
                    var usuario_autorizacion = usuarios_autorizacion.user_id;

                    autorizaciones autorizacion = new autorizaciones();
                    autorizacion.plan_mayor = plan_mayor;
                    autorizacion.user_autorizacion = usuario_autorizacion;
                    autorizacion.user_creacion = usuario_actual;
                    autorizacion.fecha_creacion = DateTime.Now;
                    autorizacion.tipo_autorizacion = 2;
                    autorizacion.bodega = bodega_actual;
                    db.autorizaciones.Add(autorizacion);
                    db.SaveChanges();
                    var autorizacion_id = db.autorizaciones.OrderByDescending(x => x.id).FirstOrDefault(x => x.tipo_autorizacion == 2).id;
                    result = 1;

                    try
                    {
                        var correo_enviado = db.notificaciones.FirstOrDefault(x => x.user_destinatario == usuario_autorizacion && x.enviado != true && x.autorizacion_id == autorizacion_id);
                        if (correo_enviado == null)
                        {
                            var user_destinatario = db.users.Find(usuario_autorizacion);
                            var user_remitente = db.users.Find(usuario_actual);

                            MailAddress de = new MailAddress(correoconfig.correo, "Notificación Iceberg");
                            MailAddress para = new MailAddress(user_destinatario.user_email, user_destinatario.user_nombre + " " + user_destinatario.user_apellido);
                            MailMessage mensaje = new MailMessage(de, para);
                            mensaje.ReplyToList.Add(new MailAddress(user_remitente.user_email, user_remitente.user_nombre + " " + user_remitente.user_apellido));
                            mensaje.Bcc.Add("jairo.mateus@exiware.com");
                            mensaje.Bcc.Add("correospruebaexi2019@gmail.com");
                            mensaje.Subject = "Solicitud facturación plan mayor " + plan_mayor;
                            mensaje.BodyEncoding = System.Text.Encoding.Default;
                            mensaje.IsBodyHtml = true;
                            var html = "";
                            html += "<h4>Cordial Saludo</h4><br>";
                            html += "<p>El usuario " + user_remitente.user_nombre + " " + user_remitente.user_apellido + " solicita autorización para la facturación del vehículo " + plan_mayor + " - " + modelo + "</p><br /><br />";
                            html += "Por favor ingrese a la plataforma para dar autorización.";
                            mensaje.Body = html;

                            SmtpClient cliente = new SmtpClient(correoconfig.smtp_server);
                            cliente.Port = correoconfig.puerto;
                            cliente.UseDefaultCredentials = false;
                            cliente.Credentials = new System.Net.NetworkCredential(correoconfig.usuario, correoconfig.password);
                            cliente.EnableSsl = true;
                            cliente.Send(mensaje);

                            notificaciones envio = new notificaciones();
                            envio.user_remitente = usuario_actual;
                            envio.asunto = "Notificación solicitud facturacion de vehiculo";
                            envio.fecha_envio = DateTime.Now;
                            envio.enviado = true;
                            envio.user_destinatario = usuario_autorizacion;
                            envio.autorizacion_id = autorizacion_id;
                            db.notificaciones.Add(envio);
                            db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        notificaciones envio = new notificaciones();
                        envio.user_remitente = usuario_actual;
                        envio.asunto = "Notificación solicitud facturacion de vehiculo";
                        envio.fecha_envio = DateTime.Now;
                        envio.user_destinatario = usuario_autorizacion;
                        envio.autorizacion_id = autorizacion_id;
                        envio.enviado = false;
                        envio.razon_no_envio = ex.Message;
                        db.notificaciones.Add(envio);
                        db.SaveChanges();
                        //notificacion no enviada
                        result = -1;
                    }
                }
                else
                {
                    // ya existe
                    result = 2;
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarFlota(int flota)
        {
            var data = from v in db.vflota
                       where v.flota == flota
                       select new
                       {
                           id = v.idflota,
                           nombre = v.icb_terceros.prinom_tercero != null ? v.numero + " - " + v.icb_terceros.prinom_tercero + " " + v.icb_terceros.segnom_tercero + " " + v.icb_terceros.apellido_tercero + " " + v.icb_terceros.segapellido_tercero : v.numero + " - " + v.icb_terceros.razon_social
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDocumentosFlota(int flotaid, int pedidoid)
        {
            var flota = db.vflota.Find(flotaid);

            if (flota != null)
            {
                var data = (from d in db.vdocrequeridosflota
                            where d.codflota == flota.flota
                            select new
                            {
                                d.id,
                                d.vdocumentosflota.documento,
                                d.vdocumentosflota.iddocumento,
                                cargado = db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x => x.idpedido == pedidoid && x.iddocumento == d.iddocumento) != null ?
                                          db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x => x.idpedido == pedidoid && x.iddocumento == d.iddocumento).id : 0
                            }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            return Json(JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDocumentosPersonaNatural(int pedido)
        {
            var data = (from d in db.vdocumentosflota
                        where d.id_tipo_documento == 2
                        select new
                        {
                            d.id,
                            d.documento,
                            d.iddocumento,
                            cargado = db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x => x.idpedido == pedido && x.iddocumento == d.iddocumento) != null ?
                                          db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x => x.idpedido == pedido && x.iddocumento == d.iddocumento).id : 0
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDocumentosPersonJuridica(int pedido)
        {
            var data = (from d in db.vdocumentosflota
                        where d.id_tipo_documento == 3
                        select new
                        {
                            d.id,
                            d.documento,
                            d.iddocumento,
                            cargado = db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x => x.idpedido == pedido && x.iddocumento == d.iddocumento) != null ?
                                          db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x => x.idpedido == pedido && x.iddocumento == d.iddocumento).id : 0
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDocumentosSeleccionados(int? flotaid, int pedidoid)
        {
            var data = (from d in db.vvalidacionpeddoc
                        where (d.idflota == flotaid
                        && d.idpedido == pedidoid)
                        select new
                        {
                            d.estado,
                            id = d.iddocrequerido
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LiberarPedido()
        {
            var pedidos = db.vpedido.Where(x => x.planmayor != null && x.anulado == false && x.fecha_asignacion_planmayor != null).ToList();

            foreach (var item in pedidos)
            {
                var dias = db.pedido_tliberacion.FirstOrDefault(x => x.id == item.icb_vehiculo.clasificacion_id).dias_para_liberar;
                if (item.fecha_asignacion_planmayor.Value.AddDays(dias).CompareTo(DateTime.Now) > 0)
                {
                    var vehiculo = db.icb_vehiculo.Find(item.planmayor);
                    vehiculo.asignado = null;
                    db.Entry(vehiculo).State = EntityState.Modified;

                    item.planmayor = null;
                    //item.fecha_asignacion_planmayor = null;
                    db.Entry(item).State = EntityState.Modified;
                }
            }
            db.SaveChanges();
            return Json(pedidos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PermisoModificarValores()
        {
            var usuario = Convert.ToInt32(Session["user_usuarioid"]);
            //Validamos que el usuario loguado tenga el rol y el permiso para modificar los valores del pedido
            var permiso = (from u in db.users
                           join r in db.rols
                           on u.rol_id equals r.rol_id
                           join ra in db.rolacceso
                           on r.rol_id equals ra.idrol
                           where u.user_id == usuario /*&& u.rol_id == 4*/ && ra.idpermiso == 7
                           select new
                           {
                               u.user_id,
                               u.rol_id,
                               r.rol_nombre,
                               ra.idpermiso
                           }).Count();

            return Json(permiso, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PermisoPlanMayor()
        {
            var usuario = Convert.ToInt32(Session["user_usuarioid"]);
            //Validamos que el usuario loguado tenga el rol y el permiso para modificar los valores del pedido
            var permiso = (from u in db.users
                           join r in db.rols
                           on u.rol_id equals r.rol_id
                           join ra in db.rolacceso
                           on r.rol_id equals ra.idrol
                           where u.user_id == usuario /*&& u.rol_id == 4*/ && ra.idpermiso == 8
                           select new
                           {
                               u.user_id,
                               u.rol_id,
                               r.rol_nombre,
                               ra.idpermiso
                           }).Count();

            return Json(permiso, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PermisoModificarVH()
        {
            var usuario = Convert.ToInt32(Session["user_usuarioid"]);
            //Validamos que el usuario loguado tenga el rol y el permiso para modificar los valores del pedido
            var permiso = (from u in db.users
                           join r in db.rols
                           on u.rol_id equals r.rol_id
                           join ra in db.rolacceso
                           on r.rol_id equals ra.idrol
                           where u.user_id == usuario /*&& u.rol_id == 4*/ && ra.idpermiso == 27
                           select new
                           {
                               u.user_id,
                               u.rol_id,
                               r.rol_nombre,
                               ra.idpermiso
                           }).Count();

            return Json(permiso, JsonRequestBehavior.AllowGet);
        }

        public JsonResult PermisoGenerarFacturaProforma(int idPedido)
        {
            //Validamos que el usuario loguado tenga el rol y el permiso para generar factura proforma
            var usuario = Convert.ToInt32(Session["user_usuarioid"]);
            var permiso = (from u in db.users
                           join r in db.rols
                           on u.rol_id equals r.rol_id
                           join ra in db.rolacceso
                           on r.rol_id equals ra.idrol
                           where u.user_id == usuario /*&& u.rol_id == 4*/ && ra.idpermiso == 29
                           select new
                           {
                               u.user_id,
                               u.rol_id,
                               r.rol_nombre,
                               ra.idpermiso
                           }).Count();

            //Validamos que el pedido tenga como forma de pago credito 
            var info = (from a in db.vpedpago
                        join b in db.vformapago
                        on a.condicion equals b.id
                        where a.idpedido == idPedido && a.condicion == 1
                        select new
                        {
                            a.valor
                        }).Count();

            if (permiso > 0 && info > 0)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult BrowserFacturasProforma(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult validarEstadoPedido(int pedidoId)
        {
            var estado = (from v in db.vpedido
                          where v.anulado == true && v.id == pedidoId
                          select new
                          {
                              v.anulado
                          }).Count();

            return Json(estado, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPagosRecibidos(int pedido)
        {
            var data = (from dp in db.documentos_pago
                        join fp in db.tipopagorecibido
                        on dp.forma_pago equals fp.id
                        where dp.pedido == pedido
                        select new
                        {
                            fpago = fp.pago,
                            valor = dp.valor,
                            fecha = dp.fecha.ToString()
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BrowserBackOffice(int? menu)
        {
            BuscarFavoritos(menu);
            ViewBag.tp_doc_registros = db.tp_doc_registros.Where(x => x.tpdoc_estado == true && x.sw == 3 && x.tipo == 2).OrderBy(x => x.tpdoc_nombre).ToList();
            ViewBag.tp_doc_registrosFR = db.tp_doc_registros.Where(x => x.tpdoc_estado == true && x.sw == 3 && x.tipo == 4).OrderBy(x => x.tpdoc_nombre).ToList();
            return View();
        }

        public ActionResult BrowserBackOfficePendientes(int? menu)
        {
            BuscarFavoritos(menu);
            ViewBag.tp_doc_registros = db.tp_doc_registros.Where(x => x.tpdoc_estado == true && x.sw == 3 && x.tipo == 2).OrderBy(x => x.tpdoc_nombre).ToList();
            ViewBag.tp_doc_registrosFR = db.tp_doc_registros.Where(x => x.tpdoc_estado == true && x.sw == 3 && x.tipo == 4).OrderBy(x => x.tpdoc_nombre).ToList();
            return View();
        }

        public ActionResult BrowserUsados(int? menu)
        {
            BuscarFavoritos(menu);
            ViewBag.tp_doc_registros = db.tp_doc_registros.Where(x => x.tpdoc_estado == true && x.sw == 3 && x.tipo == 2).OrderBy(x => x.tpdoc_nombre).ToList();
            ViewBag.tp_doc_registrosFR = db.tp_doc_registros.Where(x => x.tpdoc_estado == true && x.sw == 3 && x.tipo == 4).OrderBy(x => x.tpdoc_nombre).ToList();
            return View();
        }

        public ActionResult Verificar(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            vpedido vpedido = db.vpedido.Find(id);
            if (vpedido == null)
            {
                return HttpNotFound();
            }
            #region obj pedido
            VehiculoPedidoModel pedido = new VehiculoPedidoModel();
            pedido.numero = vpedido.numero;
            pedido.impfactura2 = vpedido.impfactura2;
            pedido.impfactura3 = vpedido.impfactura3;
            pedido.impfactura4 = vpedido.impfactura4;
            pedido.bodega = vpedido.bodega;
            pedido.anulado = vpedido.anulado;
            pedido.fecha = vpedido.fecha;
            pedido.idcotizacion = vpedido.idcotizacion;
            pedido.numerocotizacion = Convert.ToInt32(db.icb_cotizacion.Where(d => d.cot_idserial == vpedido.idcotizacion).Select(d => d.cot_numcotizacion).FirstOrDefault());
            pedido.nit = vpedido.nit;
            pedido.numeroIdentificacion = Convert.ToString(db.icb_terceros.Where(d => d.tercero_id == vpedido.nit).Select(d => d.doc_tercero).FirstOrDefault());
            pedido.nit_asegurado = vpedido.nit_asegurado;
            pedido.nit2 = vpedido.nit2;
            pedido.nit3 = vpedido.nit3;
            pedido.nit4 = vpedido.nit4;
            pedido.nit5 = vpedido.nit5;
            pedido.vendedor = vpedido.vendedor;
            pedido.modelo = vpedido.modelo;
            pedido.id_anio_modelo = vpedido.id_anio_modelo;
            pedido.plan_venta = vpedido.plan_venta;
            pedido.planmayor = vpedido.planmayor;
            pedido.fecha_asignacion_planmayor = vpedido.fecha_asignacion_planmayor;
            pedido.asignado_por = vpedido.asignado_por;
            pedido.condicion = vpedido.condicion;
            pedido.dias_validez = vpedido.dias_validez;
            pedido.valor_unitario = vpedido.valor_unitario != null ? vpedido.valor_unitario.Value.ToString("N0", new CultureInfo("is-IS")) : "0";
            pedido.porcentaje_iva = vpedido.porcentaje_iva;
            pedido.valorPoliza = vpedido.valorPoliza != null ? vpedido.valorPoliza.Value.ToString("N0", new CultureInfo("is-IS")) : "0";
            pedido.pordscto = vpedido.pordscto;
            pedido.vrdescuento = vpedido.vrdescuento != null ? vpedido.vrdescuento.Value.ToString("N0", new CultureInfo("is-IS")) : "0";
            pedido.cantidad = vpedido.cantidad;
            pedido.tipo_carroceria = vpedido.tipo_carroceria;
            pedido.vrcarroceria = vpedido.vrcarroceria != null ? vpedido.vrcarroceria.Value.ToString("N0", new CultureInfo("is-IS")) : "0";
            pedido.vrtotal = vpedido.vrtotal != null ? vpedido.vrtotal.Value.ToString("N0", new CultureInfo("is-IS")) : "0";
            pedido.moneda = vpedido.moneda;
            pedido.id_aseguradora = vpedido.id_aseguradora;
            pedido.notas1 = vpedido.notas1;
            pedido.notas2 = vpedido.notas2;
            pedido.escanje = vpedido.escanje;
            pedido.eschevyplan = vpedido.eschevyplan;
            pedido.esLeasing = vpedido.esLeasing;
            pedido.esreposicion = vpedido.esreposicion;
            pedido.nit_prenda = vpedido.nit_prenda;
            pedido.flota = vpedido.flota;
            pedido.codflota = vpedido.codigoflota;
            pedido.facturado = vpedido.facturado;
            pedido.numfactura = vpedido.numfactura;
            pedido.porcentaje_impoconsumo = vpedido.porcentaje_impoconsumo;
            pedido.numeroplaca = vpedido.numeroplaca;
            pedido.motivo_anulacion = vpedido.motivo_anulacion;
            pedido.venta_gerencia = vpedido.venta_gerencia;
            pedido.Color_Deseado = vpedido.Color_Deseado;
            pedido.terminacionplaca = vpedido.terminacionplaca;
            pedido.bono = vpedido.bono;
            pedido.idmodelo = vpedido.idmodelo;
            pedido.nuevo = vpedido.nuevo;
            pedido.usado = vpedido.usado;
            pedido.servicio = vpedido.servicio;
            pedido.placapar = vpedido.placapar;
            pedido.placaimpar = vpedido.placaimpar;
            pedido.color_opcional = vpedido.color_opcional;
            pedido.cargomatricula = vpedido.cargomatricula;
            pedido.obsequioporcen = vpedido.obsequioporcen;
            pedido.valormatricula = Convert.ToString(vpedido.valormatricula);
            pedido.rango_placa = vpedido.rango_placa;
            pedido.marcvh_id = Convert.ToString(vpedido.marca);
            pedido.fec_creacion = vpedido.fec_creacion;
            pedido.userid_creacion = vpedido.userid_creacion;
            #endregion

            listas(pedido);
            BuscarFavoritos(menu);
            return View(pedido);
        }

        [HttpPost]
        public ActionResult Verificar(VehiculoPedidoModel vpedido, int? menu)
        {
            #region  Documentos
            var docsValidacion = db.vvalidacionpeddoc.Where(x => x.idflota == vpedido.flota && x.idpedido == vpedido.id).ToList();
            if (docsValidacion.Count > 0)
            {
                foreach (var item in docsValidacion)
                {
                    if (vpedido.flota != null)
                    {
                        var docrequeridos = db.vdocrequeridosflota.Find(item.iddocrequerido);
                        item.estado = Convert.ToBoolean(Request["documento_" + docrequeridos.id]);
                        db.Entry(item).State = EntityState.Modified;
                    }
                    else
                    {
                        var asd = Request["documento_" + item.iddocrequerido];
                        item.estado = Convert.ToBoolean(Request["documento_" + item.iddocrequerido]);
                        db.Entry(item).State = EntityState.Modified;
                    }
                }
            }
            else
            {
                var codflota = db.vflota.Find(vpedido.flota);
                if (codflota != null)
                {
                    var docrequeridosflota = db.vdocrequeridosflota.Where(x => x.codflota == codflota.flota);
                    foreach (var item in docrequeridosflota)
                    {
                        vvalidacionpeddoc validacion = new vvalidacionpeddoc();
                        validacion.idpedido = vpedido.id;
                        validacion.idflota = vpedido.flota ?? 0;
                        validacion.codflota = codflota.flota;
                        validacion.iddocrequerido = item.id;
                        validacion.estado = Convert.ToBoolean(Request["documento_" + item.id]);
                        db.vvalidacionpeddoc.Add(validacion);
                    }
                }
                else
                {
                    var docPer = 0;
                    var tipoPer = Request["tipoPer"].Trim();
                    if (tipoPer == "N")
                    {
                        docPer = 3;
                    }
                    else
                    {
                        docPer = 2;
                    }
                    var docrequeridos = db.vdocumentosflota.Where(x => x.id_tipo_documento == docPer);
                    foreach (var item in docrequeridos)
                    {
                        vvalidacionpeddoc validacion = new vvalidacionpeddoc();
                        validacion.idpedido = vpedido.id;
                        validacion.iddocrequerido = item.id;
                        validacion.estado = Convert.ToBoolean(Request["documento_" + item.id]);
                        db.vvalidacionpeddoc.Add(validacion);
                    }
                }
            }
            #endregion
            var result = db.SaveChanges();
            if (result > 0)
            {
                //----- Tomar el ultimo pedido agregado para mostrar la factura proforma
                var ultimoAgregado = db.vpedido.OrderByDescending(x => x.numero).FirstOrDefault();
                ViewBag.pedidoId = ultimoAgregado.numero;

                TempData["mensaje"] = "Documentos actualizados correctamente";
                return RedirectToAction("BrowserBackOffice", new { id = vpedido.id, menu });
            }
            else
            {
                TempData["mensaje_error"] = "Error al registrar los documentos del pedido, por favor intente nuevamente";
            }

            listas(vpedido);
            return View();
        }

        public JsonResult actualizardocumentos()
        {
            var id = Request.Form["id"];
            var pedido = Convert.ToInt32(Request.Form["pedido"]);
            var documento = Request.Files["documento"];
            var error = 0;
            var ruta = "";

            if (!string.IsNullOrWhiteSpace(id) && documento.ContentLength > 0)
            {
                //busco la consulta
                var idconsulta = 0;
                var conver = int.TryParse(id, out idconsulta);

                var docFacturacion = db.vdocumentosflota.FirstOrDefault(x => x.id == idconsulta).iddocumento;
                var consulta = db.vpedido.Where(d => d.id == pedido).FirstOrDefault();
                if (consulta != null)
                {
                    //guardo el archivo
                    var archivo = documento.FileName;
                    if (CheckFileType(documento.FileName) == false)
                    {
                        //..........
                        error = 1;
                        TempData["mensaje_error"] = TempData["mensaje_error"] + "error. extensión no permitida. las extensiones permitidas son .pdf, .doc, .docx, .jpg , .jpeg  y .png";
                    }
                    else
                    {
                        //guardo el archivo
                        ruta = "documentosVehiculo/" + idconsulta + "_" + consulta.planmayor + "_" + archivo;
                        string path = Server.MapPath("~/Content/documentosVehiculo/" + idconsulta + "_" + consulta.planmayor + "_" + archivo);
                        documento.SaveAs(path);
                    }

                    var buscarDocPedido = db.vvalidacionpeddoc.Where(x => x.idpedido == pedido).ToList();
                    if (buscarDocPedido.Count > 0)
                    {
                        foreach (var item in buscarDocPedido)
                        {
                            if (item.iddocrequerido == Convert.ToInt32(id))
                            {
                                item.estado = true;
                                db.Entry(item).State = EntityState.Modified;
                            }
                        }
                    }

                    documentos_vehiculo docVH = new documentos_vehiculo();
                    docVH.iddocumento = docFacturacion;
                    docVH.idvehiculo = consulta.planmayor;
                    docVH.rutadocumento = ruta;
                    docVH.fec_creacion = DateTime.Now;
                    docVH.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    docVH.estado = true;
                    docVH.idtercero = Convert.ToInt32(consulta.nit);
                    docVH.idpedido = pedido;

                    db.documentos_vehiculo.Add(docVH);
                    db.SaveChanges();
                    return Json(1, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(0, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult verArchivo(int id)
        {
            var info = db.documentos_vehiculo.FirstOrDefault(x => x.id == id).rutadocumento;

            var urlActual0 = Request.Url.Scheme + "://" + Request.Url.Authority;
            //var lastFolder = Path.GetDirectoryName(urlActual0);
            //var pathWithoutLastFolder = Path.GetDirectoryName(lastFolder);
            var url = urlActual0 += @"/Content/" + info;

            return Json(url, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPedidosPaginadosBackOffice()
        {

            var data = (from p in db.vw_browserBackOffice
                        where p.facturado == false && p.planmayor != null
                        select new
                        {
                            p.id,
                            p.numero,
                            p.bodega,
                            p.bodccs_nombre,
                            p.proceso,
                            p.modelo,
                            p.vrtotal,
                            placa = p.plac_vh != null ? p.plac_vh : "",
                            color = p.colvh_nombre != null ? p.colvh_nombre : "",
                            vin = p.vin != null ? p.vin : "",
                            ubicacion = p.ubivh_nombre != null ? p.ubivh_nombre : "",
                            anio = p.anio_vh != null ? p.anio_vh : null,
                            fechaMatricula = p.fecmatricula,
                            valor = p.valor != null ? p.valor.ToString() : "0",
                            p.asesor,
                            p.idCliente,
                            cliente = p.razon_social != null ? p.doc_tercero + "-" + p.razon_social : p.doc_tercero + "-" + p.prinom_tercero + " " + p.segnom_tercero + " " + p.apellido_tercero + " " + p.segapellido_tercero,
                            fecha = p.fecha,
                            fechaA = p.fecha_asignacion_planmayor,
                            saldo = p.saldo != null ? p.saldo.ToString() : " ",
                            planmayor = p.planmayor.ToString(),
                            fecha_asignacion_planmayor = p.fecha_asignacion_planmayor,
                            p.autorizado,
                            p.autorizados
                        }).ToList();

            var data2 = data.Select(x => new
            {
                x.id,
                x.numero,
				x.bodega,
                x.bodccs_nombre,
                x.proceso,
                x.modelo,
                x.vrtotal,
                x.placa,
                x.color,
                x.vin,
                x.ubicacion,
                x.anio,
                fechaMatricula = x.fechaMatricula != null ? x.fechaMatricula.Value.ToString("yyyy/MM/dd") : "",
                x.valor,
                x.asesor,
                x.idCliente,
                x.cliente,
                fecha = x.fecha.ToString("yyyy/MM/dd"),
                fechaA = x.fechaA != null ?  x.fechaA.Value.ToString("yyyy/MM/dd") : "",
                x.saldo,
                x.planmayor,
                fecha_asignacion_planmayor = x.fecha_asignacion_planmayor != null ? x.fecha_asignacion_planmayor.Value.ToString("yyyy/MM/dd"): "",
                x.autorizado,
                x.autorizados,
            });

            return Json(data2, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPedidosPaginadosBackOfficePendientes()
        {
            var info = (from p in db.vw_browserBackOffice
                        where p.facturado == true && p.fecha_entrega == null && p.planmayor != null
                        select new
                        {
                            p.id,
                            p.numero,
                            p.bodega,
                            p.bodccs_nombre,
                            p.proceso,
                            p.modelo,
                            p.vrtotal,
                            p.plac_vh,
                            p.colvh_nombre,
                            p.vin,
                            p.ubivh_nombre,
                            p.anio_vh,
                            p.fecmatricula,
                            p.valor,
                            p.asesor,
                            p.doc_tercero,
                            p.prinom_tercero,
                            p.segnom_tercero,
                            p.apellido_tercero,
                            p.segapellido_tercero,
                            p.razon_social,
                            p.fecha,
                            p.fecha_asignacion_planmayor,
                            p.saldo,
                            p.planmayor,
                            p.autorizado,
                            p.numfactura,
                            p.fecha_venta
                        }).ToList();

            var data = info.Select(p => new
            {
                p.id,
                p.numero,
                p.bodega,
                p.bodccs_nombre,
                p.proceso,
                p.modelo,
                p.vrtotal,
                placa = p.plac_vh,
                color = p.colvh_nombre != null ? p.colvh_nombre : "",
                vin = p.vin != null ? p.vin : "",
                ubicacion = p.ubivh_nombre != null ? p.ubivh_nombre : "",
                anio = p.anio_vh != null ? p.anio_vh : null,
                fechaMatricula = p.fecmatricula != null ? p.fecmatricula.Value.ToString() : "",
                valor = p.valor != null ? p.valor.ToString() : "0",
                p.asesor,
                cliente = p.razon_social != null ? p.doc_tercero + "-" + p.razon_social : p.doc_tercero + "-" + p.prinom_tercero + " " + p.segnom_tercero + " " + p.apellido_tercero + " " + p.segapellido_tercero,
                fecha = p.fecha != null ? p.fecha.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                fechaA = p.fecha_asignacion_planmayor.ToString(),
                saldo = p.saldo != null ? p.saldo.ToString() : " ",
                planmayor = p.planmayor.ToString(),
                fecha_asignacion_planmayor = p.fecha_asignacion_planmayor.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")),
                p.autorizado,
                factura = p.numfactura != null ? p.numfactura : 0,
                facturaFecha = p.fecha_venta != null ? p.fecha_venta.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : ""
            });


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarUsadosSinFacturar()
        {
            var info = (from p in db.vw_usadosEnProceso
                        select new
                        {
                            p.id,
                            p.numero,
                            p.id_bod,
                            p.bodccs_nombre,
                            p.proceso,
                            p.modelo,
                            p.vrtotal,
                            p.plac_vh,
                            p.colvh_nombre,
                            p.vin,
                            p.ubivh_nombre,
                            p.anio_vh,
                            p.fecmatricula,
                            p.valor,
                            p.asesor,
                            p.doc_tercero,
                            p.cliente,
                            p.fecha,
                            p.fecha_asignacion_planmayor,
                            p.saldo,
                            p.plan_mayor,
                            p.autorizado,
                            p.numfactura,
                            p.fecha_venta,
                            p.tipo_compra,
                        }).ToList();

            var data = info.Select(p => new
            {
                id = p.id != null ? p.id.ToString() : "",
                numero = p.numero != null ? p.numero.ToString() : "",
                bodega = p.id_bod != null ? p.id_bod.ToString() : "",
                bodccs_nombre = !string.IsNullOrWhiteSpace(p.bodccs_nombre) ? p.bodccs_nombre : "",
                proceso = p.proceso != null ? p.proceso : "",
                modelo = p.modelo != null ? p.modelo : "",
                placa = !string.IsNullOrWhiteSpace(p.plac_vh) ? p.plac_vh : "",
                color = !string.IsNullOrWhiteSpace(p.colvh_nombre) ? p.colvh_nombre : "",
                vin = p.vin != null ? p.vin : "",
                ubicacion = p.ubivh_nombre != null ? p.ubivh_nombre : "",
                anio = p.anio_vh != null ? p.anio_vh.ToString() : "",
                fechaMatricula = p.fecmatricula != null ? p.fecmatricula.Value.ToString() : "",
                vrtotal = p.vrtotal != null ? p.vrtotal : 0,
                valor = p.valor != null ? p.valor : 0,
                asesor = !string.IsNullOrWhiteSpace(p.asesor) ? p.asesor : "",
                cliente = p.doc_tercero + "-" + p.cliente,
                fecha = p.fecha != null ? p.fecha.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                fechaA = p.fecha_asignacion_planmayor != null ? p.fecha_asignacion_planmayor.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                saldo = p.saldo != null ? p.saldo : 0,
                planmayor = !string.IsNullOrWhiteSpace(p.plan_mayor) ? p.plan_mayor.ToString() : "",
                fecha_asignacion_planmayor = p.fecha_asignacion_planmayor != null ? p.fecha_asignacion_planmayor.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                autorizado = p.autorizado != null ? p.autorizado : false,
                factura = p.numfactura != null ? p.numfactura.ToString() : "",
                facturaFecha = p.fecha_venta != null ? p.fecha_venta.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                tipoCompra = !string.IsNullOrWhiteSpace(p.tipo_compra) ? p.tipo_compra : "",
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult verificarRolUsuario()
        {
            var usuario = Convert.ToInt32(Session["user_usuarioid"]);
            //Validamos que el usuario loguado tenga el rol y el permiso para modificar los valores del pedido
            var data = (from u in db.users
                        join r in db.rols
                        on u.rol_id equals r.rol_id
                        where u.user_id == usuario
                        select
                            u.rol_id
                        ).FirstOrDefault();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public class eventosFaltantes
        {
            public int id { get; set; }
            public string nombreEvento { get; set; }
        }

        public JsonResult buscarTracking(string planmayor)
        {
            if (planmayor != null || planmayor != "")
            {
                var eventos = (from ve in db.icb_vehiculo_eventos
                               join te in db.icb_tpeventos
                               on ve.id_tpevento equals te.tpevento_id
                               join b in db.bodega_concesionario
                               on ve.bodega_id equals b.id
                               join u in db.users
                               on ve.eventouserid_creacion equals u.user_id
                               join df in db.documento_facturacion
                               on te.iddocasociado equals df.docfac_id into temp
                               from df in temp.DefaultIfEmpty()
                               where ve.planmayor == planmayor
                               select new
                               {
                                   te.tpevento_id,
                                   te.codigoevento,
                                   te.tpevento_nombre,
                                   b.bodccs_nombre,
                                   ve.fechaevento,
                                   u.user_nombre,
                                   u.user_apellido,
                                   te.iddocasociado,
                                   cargado = db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x => x.idvehiculo == planmayor && x.iddocumento == te.iddocasociado) != null ?
                                          db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x => x.idvehiculo == planmayor && x.iddocumento == te.iddocasociado).id : 0
                               }).ToList();

                List<int> listaEventos = new List<int>();
                foreach (var item in eventos)
                {
                    listaEventos.Add(item.tpevento_id);
                }

                var buscarFaltantes = (from a in db.icb_tpeventos
                                       where !listaEventos.Contains(a.tpevento_id) && a.tpevento_estado == true
                                       select new
                                       {
                                           a.tpevento_id,
                                           a.tpevento_nombre
                                       }).ToList();


                var data2 = eventos.Select(x => new
                {
                    codigo = x.codigoevento,
                    nombre = x.tpevento_nombre,
                    bodega = x.bodccs_nombre,
                    fecha = x.fechaevento.ToString("yyyy/MM/dd HH:mm", new CultureInfo("is-IS")),
                    usuario = x.user_nombre + ' ' + x.user_apellido,
                    documento = x.iddocasociado != null ? x.iddocasociado : 0,
                    cargado = x.cargado
                }).ToList();

                return Json(new { info = true, data2, buscarFaltantes }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { info = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarMotivosAnulacion()
        {
            var data = (from m in db.motivoanulacion
                        where m.estado == true
                        select new
                        {
                            m.id,
                            m.motivo
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BrowserPendientesAlistamiento(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult verificarPermisoPazysalvo()
        {
            var usuario = Convert.ToInt32(Session["user_usuarioid"]);
            //Validamos que el usuario loguado tenga el rol y el permiso para ver toda la informacion
            var data = (from u in db.users
                        join r in db.rols
                             on u.rol_id equals r.rol_id
                        join ra in db.rolacceso
                             on r.rol_id equals ra.idrol
                        where u.user_id == usuario && ra.idpermiso == 22
                        select new
                        {
                            u.user_id,
                            u.rol_id,
                            r.rol_nombre,
                            ra.idpermiso
                        }).Count();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public List<listaGeneral> bodegas()
        {
            List<listaGeneral> bodegas = db.bodega_concesionario.Select(
                    x => new listaGeneral
                    {
                        id = x.id,
                        descripcion = x.bodccs_nombre
                    }
                ).OrderBy(x => x.descripcion).ToList();
            return bodegas;
        }

        public string cerosconsecutivo(int numero)
        {
            var numero2 = "";
            if (numero > 100000)
            {
                numero2 = numero.ToString();
            }
            else if (numero > 10000)
            {
                numero2 = "0" + numero.ToString();
            }
            else if (numero > 1000)
            {
                numero2 = "00" + numero.ToString();

            }
            else if (numero > 100)
            {
                numero2 = "000" + numero.ToString();
            }
            else if (numero > 10)
            {
                numero2 = "0000" + numero.ToString();
            }
            else if (numero >= 0)
            {
                numero2 = "00000" + numero.ToString();
            }
            return numero2;
        }

        public string botonalistamientoe(string planmayor,DateTime? fecha_inicio,DateTime? fecha_fin)
        {
            
            var resultado = "<div class='col-xs-12 col-sm-12 col-md-12 col-lg-12' style='white-space:nowrap;width:100%;heigth:100%'>";
            resultado = resultado + "<div class='col-xs-3 col-sm-3 col-md-3 col-lg-3' style='display:inline-block;'>";
            var clase = "btn-sm ";
            var fechax = "";
            if (fecha_inicio == null)
            {
                clase = clase + "btn-danger";
            }
            else if(fecha_inicio !=null && fecha_fin == null)
            {
                clase = clase + "btn-warning";
                var fechax_inicio = fecha_inicio.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
                fechax = fechax + "<span class='text-warning' style='font-size:9px;><i class='text-warning fa fa-check-circle'></i>" + fechax_inicio+"</span>";
            }
            else
            {
                var fechax_inicio = fecha_inicio.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
                fechax = fechax + "<span class='text-warning' style='font-size:9px;'><i class='text-warning fa fa-check-circle'></i>" + fechax_inicio + "</span>";
                var fechax_fin = fecha_fin.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
                fechax = fechax + "<br/><span class='text-success' style='font-size:9px;'><i class='text-success fa fa-check-circle'></i>" + fechax_fin + "</span>";

                clase = clase + "btn-success";
            }
            var icono = "<button type='button' class='"+clase+"'><i class='fa fa-car'></i>";
            //busco si el vehículo ha tenido alistamiento de embellecimiento
            resultado = resultado +icono+ "</div>";
            resultado = resultado + "<div class='col-xs-9 col-sm-9 col-md-9 col-lg-9' style='display:inline-block;'>";
            resultado = resultado +fechax;
            resultado = resultado + "</div>";
            resultado = resultado + "</div>";

            return resultado;
        }

        public string botonalistamientoem(string planmayor, DateTime? fecha_fin)
        {

            var resultado = "<div class='col-xs-12 col-sm-12 col-md-12 col-lg-12' style='white-space:nowrap;width:100%;heigth:100%'>";
            resultado = resultado + "<div class='col-xs-3 col-sm-3 col-md-3 col-lg-3' style='display:inline-block;'>";
            var clase = "btn-sm ";
            var fechax = "";
            if (fecha_fin == null)
            {
                clase = clase + "btn-danger";
            }
            
            else
            {
                var fechax_fin = fecha_fin.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
                fechax = fechax + "<br/><span class='text-success' style='font-size:9px;'><i class='text-success fa fa-check-circle'></i>" + fechax_fin + "</span>";

                clase = clase + "btn-success";
            }
            var icono = "<button type='button' class='" + clase + "'><i class='fa fa-cogs'></i>";
            //busco si el vehículo ha tenido alistamiento de embellecimiento
            resultado = resultado + icono + "</div>";
            resultado = resultado + "<div class='col-xs-9 col-sm-9 col-md-9 col-lg-9' style='display:inline-block;'>";
            resultado = resultado + fechax;
            resultado = resultado + "</div>";
            resultado = resultado + "</div>";

            return resultado;
        }

        public string botonalistamientoa(string planmayor)
        {
            //parametro tipo de orden de taller accesorios (razon de ingreso accesorios)
            var acce1 = db.icb_sysparameter.Where(d => d.syspar_cod == "P116").FirstOrDefault();
            var accesori = acce1 != null ? Convert.ToInt32(acce1.syspar_value) : 6;

            //veo si el plan_mayor tiene orden de taller en accesorios
            var ordenaccesorios = db.tencabezaorden.Where(d => d.razoningreso == accesori && d.estado==true).FirstOrDefault();

            var resultado = "<div class='col-xs-12 col-sm-12 col-md-12 col-lg-12' style='white-space:nowrap;width:100%;heigth:100%'>";
            resultado = resultado + "<div class='col-xs-3 col-sm-3 col-md-3 col-lg-3' style='display:inline-block;'>";
            var clase = "btn-sm ";
            var fechax = "";
            if (ordenaccesorios == null)
            {
                clase = clase + "btn-danger";
            }
            else if (ordenaccesorios.fecha_fin_operacion == null)
            {
                clase = clase + "btn-warning";
                var fechax_inicio = ordenaccesorios.fec_creacion.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
                fechax = fechax + "<span class='text-warning' style='font-size:9px;><i class='text-warning fa fa-check-circle'></i>" + fechax_inicio + "</span>";
            }
            else
            {
                var fechax_inicio = ordenaccesorios.fec_creacion.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
                fechax = fechax + "<span class='text-warning' style='font-size:9px;'><i class='text-warning fa fa-check-circle'></i>" + fechax_inicio + "</span>";
                var fechax_fin = ordenaccesorios.fecha_fin_operacion.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
                fechax = fechax + "<br/><span class='text-success' style='font-size:9px;'><i class='text-success fa fa-check-circle'></i>" + fechax_fin + "</span>";

                clase = clase + "btn-success";
            }
            var icono = "<button type='button' class='" + clase + "'><i class='fa fa-sliders'></i>";
            //busco si el vehículo ha tenido alistamiento de embellecimiento
            resultado = resultado + icono + "</div>";
            resultado = resultado + "<div class='col-xs-9 col-sm-9 col-md-9 col-lg-9' style='display:inline-block;'>";
            resultado = resultado + fechax;
            resultado = resultado + "</div>";
            resultado = resultado + "</div>";

            return resultado;
        }

        public void operacionAlistamiento()
        {
            string sIdOperacion = db.icb_sysparameter.Where(x => x.syspar_cod.Contains("P81")).Select(x => x.syspar_value).FirstOrDefault();
            iIdOperacion = Convert.ToInt32(sIdOperacion); // id Operacion Alistamiento
            string sCreateOt = db.icb_sysparameter.Where(x => x.syspar_cod.Contains("P78")).Select(x => x.syspar_value).FirstOrDefault();
            iCreateOt = Convert.ToInt32(sCreateOt); // id estado creacion Ot
            string sFinishOt = db.icb_sysparameter.Where(x => x.syspar_cod.Contains("P89")).Select(x => x.syspar_value).FirstOrDefault();
            iFinishOt = Convert.ToInt16(sFinishOt); // id estado finalizacion Ot TEMPORAL
            string sExecutionOt = db.icb_sysparameter.Where(x => x.syspar_cod.Contains("P84")).Select(x => x.syspar_value).FirstOrDefault();
            iExecutionOt = Convert.ToInt16(sExecutionOt); // id estado ejecucion Ot TEMPORAL
            string tpbahia_alis = db.icb_sysparameter.Where(x => x.syspar_cod.Contains("P82")).Select(x => x.syspar_value).FirstOrDefault();
            idtpbahia_alis = Convert.ToInt32(tpbahia_alis); // id bahia alistamiento
            bahia = db.tbahias.Where(x => x.bodega == bodegaActual && x.tipo_bahia == idtpbahia_alis).Select(x => x.id).FirstOrDefault(); // Id bahia alistamiento
            string sEnvAlis = db.icb_sysparameter.Where(x => x.syspar_cod == "P85").Select(x => x.syspar_value).FirstOrDefault(); // Envio Alistamiento
            iEnvAlis = Convert.ToInt32(sEnvAlis);// Envio Alistamiento
            string sFinAlis = db.icb_sysparameter.Where(x => x.syspar_cod == "P86").Select(x => x.syspar_value).FirstOrDefault(); // Fin Alistamiento
            iFinAlis = Convert.ToInt32(sFinAlis);// Fin Alistamiento
            string sEnvRAlis = db.icb_sysparameter.Where(x => x.syspar_cod == "P92").Select(x => x.syspar_value).FirstOrDefault(); // Envio Alistamiento
            iEnvRAlis = Convert.ToInt32(sEnvRAlis);// Envio Realistamiento
            string sFinRAlis = db.icb_sysparameter.Where(x => x.syspar_cod == "P93").Select(x => x.syspar_value).FirstOrDefault(); // Fin Alistamiento
            iFinRAlis = Convert.ToInt32(sFinRAlis);// Fin Realistamiento
        }

        public void variablesAlistamiento(icb_consecutivo_ot buscatCodigo)
        {
            var tipoorden = db.icb_sysparameter.Where(d => d.syspar_cod == "P76").FirstOrDefault();
            var tipodocorden = tipoorden != null ? Convert.ToInt32(tipoorden.syspar_value) : 26;
            // Tipo Doc
            idDocOtBodega = (from consecutivos in db.icb_doc_consecutivos
                             join bodega in db.bodega_concesionario
                             on consecutivos.doccons_bodega equals bodega.id
                             join tipoDocumento in db.tp_doc_registros
                             on consecutivos.doccons_idtpdoc equals tipoDocumento.tpdoc_id
                             where consecutivos.doccons_bodega == bodegaActual && tipoDocumento.tipo == tipodocorden
                             select tipoDocumento.tpdoc_id).FirstOrDefault();
            // Consecutivo
            var anio = DateTime.Now.Year.ToString();
            var consecutivonum = cerosconsecutivo(buscatCodigo.otcon_consecutivo.Value);
            codigoIOT = anio.Substring(anio.Length - 2) + buscatCodigo.otcon_prefijo + "-" + consecutivonum;
            operacionAlistamiento();
        }

        public class accesoriospedido
        {
            public string codigo { get; set; }
            public string nombre { get; set; }
            public string cantidad { get; set; }
            public string precio_unitario { get; set; }
            public string porcentaje_descuento { get; set; }
            public string valor_descuento { get; set; }
            public string porcentaje_iva { get; set; }
            public string valor_iva { get; set; }
            public string total_repuesto { get; set; }
        }


        public ActionResult modalAlistamiento(int idn)
        {
            operacionAlistamiento();
            vw_pendientesEntrega x = db.vw_pendientesEntrega.SingleOrDefault(t => t.id == idn);
            agendaAlistamientoModel alist = new agendaAlistamientoModel();
            alist.idpedido = x.id;
            alist.modeloVh = x.modelo;
            alist.serieVh = x.vin;
            alist.placaVh = x.plac_vh;
            alist.anioModeloVh = (x.anio_vh != null) ? x.anio_vh : 0;
            alist.colorVh = x.colvh_nombre;
            alist.planMayorVh = x.planmayor;
            alist.asesor = x.asesor;
            alist.cedulaVh = x.doc_tercero;
            alist.clienteIdVh = x.idCliente;
            alist.clienteVh = x.cliente;
            alist.ubivh_id = x.ubivh_id;
            alist.ubicacionVh = x.ubivh_nombre;
            alist.carroceriaVh = (x.tipo_carroceria != null) ? true : false;
            alist.fcEnvioCarrocVh = (x.fec_carroceria_envio != null) ? x.fec_carroceria_envio.Value.ToString("dd/MM/yyyy") : "";
            alist.fcLlegadaCarrocVh = (x.fec_carroceria_llegada != null) ? x.fec_carroceria_llegada.Value.ToString("dd/MM/yyyy") : "";

            //alist.fcEnvioCarrocVh = (alist.fcEnvioCarrocVh != "") ? alist.fcEnvioCarrocVh.ToString("dd/mm/yyyy") : "",
            //alist.fcLlegadaCarrocVh = (alist.fcLlegadaCarrocVh != "") ? alist.fcLlegadaCarrocVh.ToString("dd/mm/yyyy") : ""

            ///string sCreateOt = db.icb_sysparameter.Where(t => t.syspar_cod.Contains("P78")).Select(t => t.syspar_value).FirstOrDefault();
            ///int iCreateOt = Convert.ToInt32(sCreateOt);
            icb_bahia_alistamiento bhAls = db.icb_bahia_alistamiento.Where(t => t.id_pedido == idn && (t.tencabezaorden.estadoorden == iCreateOt || t.tencabezaorden.estadoorden == iExecutionOt)).FirstOrDefault();
            alist.estadoAlis = false;
            if (bhAls != null)
            {
                alist.id = bhAls.bh_als_id;
                alist.estadoAlis = true;
                alist.fcPreentregaVh = (bhAls.bh_als_fecha != null) ? bhAls.bh_als_fecha.Value.ToString("dd/MM/yyyy") : "";
                alist.motivo = bhAls.tp_movimiento;
                alist.bodegaListVh = bodegas();
            }
            ViewBag.iCreateOt = iCreateOt;
            ViewBag.iExecutionOt = iExecutionOt;
            ViewBag.estadoVh = db.icb_bahia_alistamiento.Where(t => t.id_pedido == idn && (t.tencabezaorden.estadoorden == iCreateOt || t.tencabezaorden.estadoorden == iExecutionOt)).Select(t => t.tencabezaorden.estadoorden).FirstOrDefault();
            //parametro de sistema razon de ingreso 
            var param1 = db.icb_sysparameter.Where(d => d.syspar_cod == "P116").FirstOrDefault();
            var accesorios = param1 != null ? Convert.ToInt32(param1.syspar_value) : 6;
            //verifico si tiene alistamiento de accesorios
            var otAccesorios = db.tencabezaorden.Where(d => d.placa == x.planmayor && d.razoningreso == accesorios).FirstOrDefault();
            //verifico si vehiculo está en algún pedido
            var existepedido = db.vpedido.Where(d => d.planmayor == x.planmayor).FirstOrDefault();

            var agendaralistamiento = 0;
            var mostraraccesorios = 0;
            var accesoriosAlistamiento = 0;
            //busco si el vehículo viene de pedido (es este caso) 

            if (otAccesorios == null)
            {
                agendaralistamiento = 1;
            }
            else
            {
                accesoriosAlistamiento = 1;
            }
            if (existepedido != null)
            {
                //me traigo los accesorios de ese pedido
                var acce = db.vpedrepuestos.Where(d => d.pedido_id == existepedido.id && d.cantidad!=null).ToList();
                var listaaccesorios = acce.Select(d => new accesoriospedido {
                    cantidad=d.cantidad!=null?d.cantidad.Value.ToString("N0",new CultureInfo("is-IS")):"0",
                    codigo=d.icb_referencia.ref_codigo,
                    nombre= d.icb_referencia.ref_descripcion,
                    porcentaje_descuento ="0",
                    porcentaje_iva="0",
                    precio_unitario=d.vrunitario!=null?d.vrunitario.Value.ToString("N0",new CultureInfo("is-IS")):"0",
                    total_repuesto=d.vrtotal!=null?d.vrtotal.Value.ToString("N0",new CultureInfo("is-IS")):"0",
                    valor_descuento="0",
                    valor_iva="0",
                }).ToList();
                mostraraccesorios = 1;
                ViewBag.listaaccesorios = listaaccesorios;
            }

            ViewBag.agendaralistamiento = agendaralistamiento;
            ViewBag.mostraraccesorios = mostraraccesorios;
            ViewBag.accesoriosAlistamiento = accesoriosAlistamiento;


            return PartialView("formAlistamiento", alist);
        }



        public int addEvento(agendaAlistamientoModel modAls, int id_usuario_actual, int idevent)
        {
           
            var buscarEvento = db.icb_tpeventos.FirstOrDefault(x => x.tpevento_id == idevent);// Alistamiento - Realistamiento
            int resp_even = 0;
            icb_vehiculo_eventos crearEvento = new icb_vehiculo_eventos();
            var propietario_vh = db.icb_vehiculo.Find(modAls.planMayorVh).propietario;
            if (propietario_vh != null || propietario_vh > 0)
            {
                crearEvento.terceroid = propietario_vh;
            }
            crearEvento.eventofec_creacion = DateTime.Now;
            crearEvento.eventouserid_creacion = id_usuario_actual;
            crearEvento.evento_estado = true;
            crearEvento.bodega_id = Convert.ToInt32(Session["user_bodega"]);
            crearEvento.evento_nombre = buscarEvento.tpevento_nombre;
            crearEvento.planmayor = modAls.planMayorVh;
            crearEvento.id_tpevento = idevent;
            crearEvento.fechaevento = DateTime.Parse(modAls.fcPreentregaVh, CultureInfo.CreateSpecificCulture("en-US"));
            crearEvento.placa = modAls.placaVh;
            crearEvento.ubicacion = modAls.ubivh_id;
            db.icb_vehiculo_eventos.Add(crearEvento);
            resp_even = db.SaveChanges();
            return resp_even;
        }
        public int updFecCarroceria(agendaAlistamientoModel modAls)
        {
            var resp_pedi = 0;
            if (modAls.fcEnvioCarrocVh != null && modAls.fcLlegadaCarrocVh != null)
            {
                vpedido tPedido = db.vpedido.SingleOrDefault(x => x.id == modAls.idpedido);
                tPedido.fec_carroceria_envio = DateTime.Parse(modAls.fcEnvioCarrocVh, CultureInfo.CreateSpecificCulture("en-US"));
                tPedido.fec_carroceria_llegada = DateTime.Parse(modAls.fcLlegadaCarrocVh, CultureInfo.CreateSpecificCulture("en-US"));
                db.Entry(tPedido).State = EntityState.Modified;
                resp_pedi = db.SaveChanges();
            }
            return resp_pedi;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult modalAlistamiento(agendaAlistamientoModel modAls)
        {
            bool resl = false;
            if (ModelState.IsValid)
            {
                int id_usuario_actual = Convert.ToInt32(Session["user_usuarioid"]);
                bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                //ahora quiero ver si hay un código de orden de taller para esa bodega
                var buscatCodigo = db.icb_consecutivo_ot.Where(x => x.otcon_bodega == bodegaActual).FirstOrDefault();
                int kilometraje = 0;
                int razoning = 1;
                int resp_ot = 0;
                int resp_consec = 0;
                int resp_bah = 0;
                int resp_pedi = 0;
                int resp_even = 0;
                if (buscatCodigo != null)
                {
                    variablesAlistamiento(buscatCodigo);
                    /// Sacar
                    using (DbContextTransaction dbTrans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            if (modAls.id > 0)
                            {
                                // Si tiene fecha de envio carroceria se el actualiza en la tabla
                                resp_pedi = updFecCarroceria(modAls);

                                icb_bahia_alistamiento bhAlis = db.icb_bahia_alistamiento.SingleOrDefault(x => x.bh_als_id == modAls.id);
                                bhAlis.tp_movimiento = Convert.ToInt16(modAls.motivo);
                                bhAlis.bh_als_fecha = DateTime.Parse(modAls.fcPreentregaVh, CultureInfo.CreateSpecificCulture("en-US"));
                                bhAlis.bh_als_usumod = id_usuario_actual;
                                bhAlis.bh_als_fecmod = DateTime.Now;
                                db.Entry(bhAlis).State = EntityState.Modified;
                                resp_bah = db.SaveChanges();
                                // Actualizar evento si existe
                                // Validar si evento es realistaminto o alistamiento
                                var icbvh_even = db.icb_vehiculo_eventos.Where(x => x.planmayor.Contains(modAls.planMayorVh) && x.id_tpevento == iEnvRAlis).FirstOrDefault();
                                if (icbvh_even == null)
                                    icbvh_even = db.icb_vehiculo_eventos.Where(x => x.planmayor.Contains(modAls.planMayorVh) && x.id_tpevento == iEnvAlis).FirstOrDefault();

                                if (icbvh_even != null)
                                { // Validar si existe Evento
                                    icbvh_even.fechaevento = DateTime.Parse(modAls.fcPreentregaVh, CultureInfo.CreateSpecificCulture("en-US"));
                                    icbvh_even.eventofec_actualizacion = DateTime.Now;
                                    icbvh_even.eventouserid_actualizacion = id_usuario_actual;
                                    db.Entry(icbvh_even).State = EntityState.Modified;
                                    resp_even = db.SaveChanges();
                                }
                            }
                            else
                            {
                                var idn_exist = db.icb_bahia_alistamiento.Where(x => x.id_pedido == modAls.idpedido && (x.tencabezaorden.estadoorden == iCreateOt || x.tencabezaorden.estadoorden == iExecutionOt)).Select(x => x.id_pedido).FirstOrDefault();
                                if (idn_exist == null)
                                {
                                    var nuevaOT = new tencabezaorden
                                    {
                                        idtipodoc = idDocOtBodega,
                                        numero = 1,
                                        asesor = id_usuario_actual,
                                        tipooperacion = iIdOperacion,
                                        codigoentrada = codigoIOT,
                                        bodega = bodegaActual,
                                        tercero = Convert.ToInt32(modAls.clienteIdVh),
                                        placa = modAls.planMayorVh,
                                        fecha = DateTime.Now,
                                        fec_creacion = DateTime.Now,
                                        userid_creacion = id_usuario_actual,
                                        entrega = DateTime.Parse(modAls.fcPreentregaVh, CultureInfo.CreateSpecificCulture("en-US")),
                                        kilometraje = kilometraje,
                                        razoningreso = razoning,
                                        estado = true,
                                        estadoorden = iCreateOt
                                    };
                                    db.tencabezaorden.Add(nuevaOT);
                                    resp_ot = db.SaveChanges();
                                    int ot_id = nuevaOT.id;
                                    // Si tiene fecha de envio carroceria se el actualiza en la tabla
                                    resp_pedi = updFecCarroceria(modAls);
                                    //le aumento el consecutivo en 1 al consecutivo de orden
                                    buscatCodigo.otcon_consecutivo = buscatCodigo.otcon_consecutivo + 1;
                                    db.Entry(buscatCodigo).State = EntityState.Modified;
                                    resp_consec = db.SaveChanges();
                                    // Tabla que asocia el alistamiento y la ot. 
                                    icb_bahia_alistamiento bhAlis = new icb_bahia_alistamiento
                                    {
                                        bahia_id = bahia, // Bahia 
                                        ot_id = ot_id,
                                        tp_movimiento = Convert.ToInt16(modAls.motivo),
                                        id_pedido = modAls.idpedido,
                                        bh_als_fecha = DateTime.Parse(modAls.fcPreentregaVh, CultureInfo.CreateSpecificCulture("en-US")),
                                        bh_als_usuela = id_usuario_actual,
                                        bh_als_fecela = DateTime.Now
                                    };
                                    db.icb_bahia_alistamiento.Add(bhAlis);
                                    resp_bah = db.SaveChanges();
                                    // Adicionar evento de alistamiento  
                                    var icbvh_even = db.icb_vehiculo_eventos.Where(x => x.planmayor.Contains(modAls.planMayorVh) && x.id_tpevento == iEnvAlis).FirstOrDefault();
                                    if (icbvh_even != null)
                                    { // Validar si existe alistamiento.
                                      // Validar si existe un evento de realistamiento
                                        var icbvh_even_r = db.icb_vehiculo_eventos.Where(x => x.planmayor.Contains(modAls.planMayorVh) && x.id_tpevento == iEnvRAlis).FirstOrDefault(); //Validar si existe realistamiento o crearlo
                                        if (icbvh_even_r != null)
                                        { // Re-iniciar los valores
                                            icbvh_even.fechaevento = DateTime.Parse(modAls.fcPreentregaVh, CultureInfo.CreateSpecificCulture("en-US"));
                                            icbvh_even.eventofec_actualizacion = DateTime.Now;
                                            icbvh_even.eventouserid_actualizacion = id_usuario_actual;
                                            db.Entry(icbvh_even).State = EntityState.Modified;
                                            resp_even = db.SaveChanges();
                                            var icbvh_evenFin = db.icb_vehiculo_eventos.Where(x => x.planmayor.Contains(modAls.planMayorVh) && x.id_tpevento == iFinRAlis).FirstOrDefault();
                                            if (icbvh_evenFin != null)
                                            {
                                                db.icb_vehiculo_eventos.Remove(icbvh_evenFin);
                                                db.SaveChanges();
                                            }
                                        }
                                        else
                                        { // Crear evento re-alistamiento
                                            addEvento(modAls, id_usuario_actual, iEnvRAlis);
                                        }
                                    }
                                    else
                                    { // Crear Evento Alistamiento
                                        addEvento(modAls, id_usuario_actual, iEnvAlis);
                                    }


                                }
                            }
                            dbTrans.Commit();
                            resl = true;
                        }
                        catch (Exception ex)
                        {
                            var exp = ex;
                            resl = false;
                            dbTrans.Rollback();
                        }
                    }
                }
                return Json(new { resp_ot = resp_ot, resp_consec = resp_consec, resp_bah = resp_bah, resl = resl }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { resl = resl, resp = "Valide los datos ingresados" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult browserAlistamiento()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult terminarAlistamiento()
        {
            operacionAlistamiento();
            var resp = 0;
            var respuesta = "";
            int idpedido = int.Parse(Request["idpedido"]);


            icb_bahia_alistamiento bhAlis = db.icb_bahia_alistamiento.SingleOrDefault(x => x.id_pedido == idpedido && (x.tencabezaorden.estadoorden == iCreateOt || x.tencabezaorden.estadoorden == iExecutionOt));
            if (bhAlis != null)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        tencabezaorden ot = db.tencabezaorden.SingleOrDefault(x => x.id == bhAlis.ot_id);
                        if (ot != null)
                        {
                            //verifico si ese vehiculo tuvo alistamiento mecanico
                            var pendAlis = db.vw_pendientesEntrega.Where(x => x.id == idpedido).FirstOrDefault();

                            if (pendAlis.FechaMecanico != null)
                            {
                                ot.estadoorden = (ot.estadoorden == iCreateOt) ? iExecutionOt : iFinishOt;
                                ot.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                                ot.fec_actualizacion = DateTime.Now;
                                db.Entry(ot).State = EntityState.Modified;
                                resp = db.SaveChanges();



                                // Validar si evento es realistaminto o alistamiento
                                var icbvh_even = db.icb_vehiculo_eventos.Where(x => x.planmayor.Contains(pendAlis.planmayor) && x.id_tpevento == iFinAlis).FirstOrDefault();
                                int idevent = (icbvh_even == null) ? iFinAlis : iFinRAlis;
                                // Registrar evento fin alistamiento
                                var buscarEvento = db.icb_tpeventos.FirstOrDefault(x => x.tpevento_id == idevent);// Fin (Alistamiento - Re Alistamiento)
                                                                                                                  // Condicionar evento 
                                if (ot.estadoorden == iFinishOt)
                                {
                                    // Validar qué se debe finalizar (alistamiento - re-alistamiento)
                                    icb_vehiculo_eventos crearEvento = new icb_vehiculo_eventos();
                                    var propietario_vh = db.icb_vehiculo.Find(pendAlis.planmayor).propietario;
                                    if (propietario_vh != null || propietario_vh > 0)
                                    {
                                        crearEvento.terceroid = propietario_vh;
                                    }
                                    crearEvento.eventofec_creacion = DateTime.Now;
                                    crearEvento.eventouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                    crearEvento.evento_estado = true;
                                    crearEvento.bodega_id = Convert.ToInt32(Session["user_bodega"]);
                                    crearEvento.evento_nombre = buscarEvento.tpevento_nombre;
                                    crearEvento.planmayor = pendAlis.planmayor;
                                    crearEvento.id_tpevento = idevent;
                                    crearEvento.fechaevento = DateTime.Now;
                                    crearEvento.placa = pendAlis.plac_vh;
                                    crearEvento.ubicacion = pendAlis.ubivh_id;
                                    db.icb_vehiculo_eventos.Add(crearEvento);
                                    db.SaveChanges();
                                }
                                dbTran.Commit();
                                resp = 1;
                                respuesta = "Alistamiento finalizado exitosamente";

                            }
                            else
                            {
                                respuesta = "Debe finalizar alistamiento mecánico primero";
                            }

                        }
                        else {
                            respuesta = "No existe una OT de alistamiento para este vehículo";
                        }
                    }
                    catch (Exception ex)
                    {
                        var exp = ex;
                        respuesta = "Se ha presentado un error en guardado de alistamiento. Favor contactar con personal de Exiware";

                        dbTran.Rollback();
                    }
                }
            }
            else
            {
                respuesta = "No existe bahía de alistamiento para la bodega en curso";
            }
            var data = new
            {
                valor=resp,
                respuesta=respuesta
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult TerminarAlistamientoMecanico(string planmayor)
        {
            var respuesta = "";
            var valor = 0;
            if (!string.IsNullOrWhiteSpace(planmayor))
            {
                //busco parametro fin de alistamiento
                var param1 = db.icb_sysparameter.Where(d => d.syspar_cod == "P119").FirstOrDefault();
                var parametro = param1 != null ? Convert.ToInt32(param1.syspar_value) : 23;
                //parametro evento de fin de alistamiento
                var eve = db.icb_tpeventos.Where(d => d.codigoevento == parametro).FirstOrDefault();


                //busco en vehiculo
                var vehi = db.icb_vehiculo.Where(d => d.plan_mayor == planmayor).FirstOrDefault();
                if (vehi != null)
                {
                    //veo si ya se le realizó alistamiento mecánico
                    var alistaexiste = db.icb_vehiculo_eventos.Where(d => d.planmayor == planmayor && d.id_tpevento == eve.tpevento_id).Count();
                    if (alistaexiste == 0)
                    {
                        icb_vehiculo_eventos crearEvento = new icb_vehiculo_eventos();
                        var propietario_vh = db.icb_vehiculo.Find(planmayor).propietario;
                        if (propietario_vh != null || propietario_vh > 0)
                        {
                            crearEvento.terceroid = propietario_vh;
                        }
                        crearEvento.eventofec_creacion = DateTime.Now;
                        crearEvento.eventouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                        crearEvento.evento_estado = true;
                        crearEvento.bodega_id = Convert.ToInt32(Session["user_bodega"]);
                        crearEvento.evento_nombre = eve.tpevento_nombre;
                        crearEvento.planmayor = planmayor;
                        crearEvento.id_tpevento = eve.tpevento_id;
                        crearEvento.fechaevento = DateTime.Now;
                        //crearEvento.placa = pendAlis.plac_vh;
                        //crearEvento.ubicacion = pendAlis.ubivh_id;
                        db.icb_vehiculo_eventos.Add(crearEvento);
                        db.SaveChanges();
                        valor = 1;
                        respuesta = "Plan de alistamiento mecánico finalizado exitosamente";

                    }
                    else
                    {
                        respuesta = "El vehículo ya tiene plan de alistamiento mecánico";
                    }
                }
                else
                {
                    respuesta = "Debe ingresar un plan mayor válido. El plan mayor ingresado no existe en base de datos";
                }
            }
            else
            {
                respuesta = "Debe ingresar un plan mayor válido";
            }


            var data = new
            {
                respuesta,
                valor
            };
            return Json(data);
        }
        public JsonResult BuscarPendientesAlistamiento()
        {
            bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            operacionAlistamiento();
            var pendAlis = (from a in db.icb_bahia_alistamiento
                            join v in db.vw_pendientesEntrega on a.id_pedido equals v.id
                            join t in db.tencabezaorden on a.ot_id equals t.id
                            where (t.estadoorden == iCreateOt || t.estadoorden == iExecutionOt) && v.FechaRecepcionBodega!=null
                            select new { v, a, t.estadoorden }).ToList();
            var data = pendAlis.Select(v => new
            {
                v.v.id,
                v.v.numero,
                planmayor = v.v.planmayor != null ? v.v.planmayor : "",
                vin = v.v.vin != null ? v.v.vin : "",
                v.v.modelo,
                anio = v.v.anio_vh,
                color = v.v.colvh_nombre != null ? v.v.colvh_nombre : "",
                cliente = v.v.doc_tercero + "-" + v.v.cliente,
                ubicacion = v.v.ubivh_nombre != null ? v.v.ubivh_nombre : "",
                v.v.asesor,
                pedidoFecha = v.v.FechaPedido != null ? v.v.FechaPedido.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                fecha = v.a.bh_als_fecha != null ? v.a.bh_als_fecha.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                nombreBodega = v.v.bodccs_nombre != null ? v.v.bodccs_nombre : "",
                icon = (v.estadoorden == iCreateOt) ? "fa-play" : "fa-check",
                info = (v.estadoorden == iCreateOt) ? "primary" : "success",
                v.estadoorden,
                tiene_alistamiento = v.v.FechaMecanico!=null?1:0,
                fecha_alistamiento_mecanico = v.v.FechaMecanico!=null?v.v.FechaMecanico.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")):"",
            }).Distinct();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarAlistamientoRealizados()
        {
            bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            operacionAlistamiento();
            int id_pedido = int.Parse(Request["idpedido"]);
            var pendAlis = (from a in db.icb_bahia_alistamiento
                            join v in db.vw_pendientesEntrega on a.id_pedido equals v.id into c
                            from item in c.DefaultIfEmpty()
                            where a.tencabezaorden.estadoorden == iFinishOt && a.id_pedido == id_pedido
                            select new { item, a }).ToList();

            var data = pendAlis.Select(v => new
            {
                v.a.bh_als_id,
                v.item.id,
                v.item.numero,
                planmayor = v.item.planmayor != null ? v.item.planmayor : "",
                vin = v.item.vin != null ? v.item.vin : "",
                v.item.modelo,
                anio = v.item.anio_vh,
                color = v.item.colvh_nombre != null ? v.item.colvh_nombre : "",
                cliente = v.item.doc_tercero + "-" + v.item.cliente,
                ubicacion = v.item.ubivh_nombre != null ? v.item.ubivh_nombre : "",
                v.item.asesor,
                pedidoFecha = v.a.bh_als_fecha != null ? v.a.bh_als_fecha.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                nombreBodega = v.item.bodccs_nombre != null ? v.item.bodccs_nombre : ""
            }).Distinct();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPedidosPendientesEntrega()
        {

            operacionAlistamiento();
            var info = (from vpe in db.vw_pendientesEntrega
                        join al in db.vw_estado_alistamiento_nuevos on vpe.id equals al.id_pedido into vpe_al
                        from al in vpe_al.DefaultIfEmpty()
                        join vehi in db.icb_vehiculo on vpe.planmayor equals vehi.plan_mayor into vu
                        from vehi in vu.DefaultIfEmpty()
                        join cred in db.v_creditos
                        on vpe.id equals cred.pedido into xx
                        from cred in xx.DefaultIfEmpty()
                        join pol in db.icb_vehiculo_eventos
                        on vpe.planmayor equals pol.planmayor into zz
                        from pol in zz.DefaultIfEmpty()
                        where vpe.planmayor != null && vpe.pazysalvo == false && vpe.FechaRecepcionBodega!=null
                        select //vpe, al).ToList();
                        new { vpe, al.estadoorden, vehi, cred.fec_desembolso, pol.poliza }).ToList();

            Dictionary<string, string> estilosAlist = new Dictionary<string, string>
            {
                ["est_" + iCreateOt] = "info",
                ["est_" + iExecutionOt] = "warning",
                ["est_" + iFinishOt] = "success"
            };
            var data = info.Select(p => new
            {
                p.vpe.id,
                p.vpe.numero,
                planmayor = p.vpe.planmayor != null ? p.vpe.planmayor : "",
                anio = p.vpe.anio_vh != null ? p.vpe.anio_vh.ToString() : "",
                modelo = p.vpe.modelo != null ? p.vpe.modelo : "",
                color = p.vpe.colvh_nombre != null ? p.vpe.colvh_nombre : "",
                vin = p.vpe.vin != null ? p.vpe.vin : "",
                p.vpe.bodega,
                nombreBodega = p.vpe.bodccs_nombre != null ? p.vpe.bodccs_nombre : "",
                ubicacion = p.vpe.ubivh_nombre != null ? p.vpe.ubivh_nombre : "",
                cliente = p.vpe.doc_tercero + "-" + p.vpe.cliente,
                p.vpe.asesor,
                soat = p.vpe.numerosoat != null ? p.vpe.numerosoat : "",
                pedidoFecha = p.vpe.FechaPedido != null ? p.vpe.FechaPedido.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                desmbolsoFecha = p.fec_desembolso != null ? p.fec_desembolso.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                facturaFecha = p.vpe.FechaVenta != null ? p.vpe.FechaVenta.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                tramitesFecha = p.vpe.FechaTramite != null ? p.vpe.FechaTramite.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                matriculaFecha = p.vpe.FechaMatricula != null ? p.vpe.FechaMatricula.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                manifiestoFecha = p.vpe.FechaManifiesto != null ? p.vpe.FechaManifiesto.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : !string.IsNullOrWhiteSpace(p.vehi.plan_mayor) ? p.vehi.fecentman_vh != null ? p.vehi.fecentman_vh.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "" : "",
                inicioFecha = p.vpe.FechaAlistamiento != null ? p.vpe.FechaAlistamiento.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                botonalisembe=!string.IsNullOrWhiteSpace(p.vpe.planmayor)?botonalistamientoe(p.vpe.planmayor,p.vpe.FechaAlistamiento,p.vpe.FechaFinAlistamiento):"",
                finFechaRecepcion = p.vpe.FechaRecepcionBodega != null ? p.vpe.FechaRecepcionBodega.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",

                botonalismec= !string.IsNullOrWhiteSpace(p.vpe.planmayor) ? botonalistamientoem(p.vpe.planmayor,p.vpe.FechaRecepcionBodega) : "",
                botonalisacce = !string.IsNullOrWhiteSpace(p.vpe.planmayor) ? botonalistamientoa(p.vpe.planmayor) : "",
                finFecha = p.vpe.FechaFinAlistamiento != null ? p.vpe.FechaFinAlistamiento.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                entregaFecha = p.vpe.FechaEntrega != null ? p.vpe.FechaEntrega.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                programacionFecha = p.vpe.FechaProgramacioEntrega != null ? p.vpe.FechaProgramacioEntrega.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                p.vpe.pazysalvo,
                usupazysalvo = p.vpe.usupazysalvo != null ? p.vpe.usupazysalvo : "",
                numFactura = p.vpe.numfactura != null ? p.vpe.numfactura.ToString() : "",
                estadoAlistamiento = p.estadoorden,
                estadoAlistaminetoEstilo = (p.estadoorden != null) ? estilosAlist["est_" + p.estadoorden] : "",
                poliza = p.poliza != null ? p.poliza : "",
            }).Distinct();

            return Json(data, JsonRequestBehavior.AllowGet);
        }



        public JsonResult pazysalvo(int pedido, bool autorizado)
        {
            var data = db.vpedido.FirstOrDefault(x => x.id == pedido);

            if (data != null)
            {
                data.pazysalvo = autorizado;
                data.usupazysalvo = Convert.ToInt32(Session["user_usuarioid"]);
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { ok = true, mensaje = "Se actualizo el pedido exitosamente" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { ok = false, mensaje = "No fue posible actualizar el pedido, por favor verifique" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult boletoSalida(int pedido)
        {
            var data = db.encab_documento.FirstOrDefault(x => x.id_pedido_vehiculo == pedido);

            if (data != null)
            {
                var tipoDocumento = data.tipo;
                var idEncabezado = data.idencabezado;
                var bodega = data.bodega;
                var nit = data.nit;

                return Json(new { ok = true, tipoDocumento, idEncabezado, bodega, nit }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { ok = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult BrowserVehiculosEntregados()
        {
            return View();
        }


        public JsonResult BuscarVehiculosEntregados()
        {
            var info = (from vpe in db.vw_pendientesEntrega
                        where vpe.planmayor != null && vpe.pazysalvo == true
                        select vpe).ToList();

            var data = info.Select(p => new
            {
                p.id,
                p.numero,
                planmayor = p.planmayor != null ? p.planmayor : "",
                anio = p.anio_vh != null ? p.anio_vh.ToString() : "",
                modelo = p.modelo != null ? p.modelo : "",
                color = p.colvh_nombre != null ? p.colvh_nombre : "",
                vin = p.vin != null ? p.vin : "",
                p.bodega,
                nombreBodega = p.bodccs_nombre != null ? p.bodccs_nombre : "",
                ubicacion = p.ubivh_nombre != null ? p.ubivh_nombre : "",
                cliente = p.doc_tercero + "-" + p.cliente,
                p.asesor,
                soat = p.numerosoat != null ? p.numerosoat : "",
                pedidoFecha = p.FechaPedido != null ? p.FechaPedido.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                facturaFecha = p.FechaVenta != null ? p.FechaVenta.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                tramitesFecha = p.FechaTramite != null ? p.FechaTramite.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                matriculaFecha = p.FechaMatricula != null ? p.FechaMatricula.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                manifiestoFecha = p.FechaManifiesto != null ? p.FechaManifiesto.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                inicioFecha = p.FechaAlistamiento != null ? p.FechaAlistamiento.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                finFecha = p.FechaFinAlistamiento != null ? p.FechaFinAlistamiento.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                entregaFecha = p.FechaEntrega != null ? p.FechaEntrega.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                programacionFecha = p.FechaProgramacioEntrega != null ? p.FechaProgramacioEntrega.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                p.pazysalvo,
                usupazysalvo = p.usupazysalvo != null ? p.usupazysalvo : "",
                numFactura = p.numfactura != null ? p.numfactura.ToString() : ""
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPreciosPorAnio(string codigoModelo, int anioModelo, bool esNuevo)
        {
            if (esNuevo)
            {
                var rolActual = Convert.ToInt32(Session["user_rolid"]);
                var buscarAccesoAlValor = db.rolacceso.FirstOrDefault(x => x.idrol == rolActual && x.idpermiso == 12);
                var puedeActualizar = buscarAccesoAlValor != null ? true : false;

                var anioModeloAux = Convert.ToInt32(anioModelo);
                var buscaPrecio = db.anio_modelo.FirstOrDefault(x => x.codigo_modelo == codigoModelo && x.anio_modelo_id == anioModeloAux);
                var buscaColor = (from color in db.color_vehiculo select new { color.colvh_id, color.colvh_nombre }).ToList();
                var result = new
                {
                    buscaPrecio.descripcion,
                    //aqui estaba colocado buscaPrecio.precio
                    valor = buscaPrecio.valor != null ? buscaPrecio.valor.ToString() : "",
                    poliza = buscaPrecio.poliza != null ? buscaPrecio.poliza.ToString() : "",
                    matricula = buscaPrecio.matricula != null ? buscaPrecio.matricula.ToString() : "",
                    esNuevo = true,
                    buscaColor,
                    puedeActualizar,
                    //Se actualiza el código para que sólo muestre el IVA e 
                    codigo_iva = buscaPrecio.porcentaje_iva.ToString("N0",new CultureInfo("is-IS")),
                    porcentaje_impoconsumo =buscaPrecio.impuesto_consumo.ToString("N0",new CultureInfo("is-IS"))
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var buscaColor = (from referencias in db.vw_referencias_total
                                  where referencias.codigo == codigoModelo && referencias.anio_vh == anioModelo && referencias.usado == true
                                  select new
                                  {
                                      referencias.colvh_id,
                                      referencias.colvh_nombre
                                  }).ToList();
                //var buscaColor = context.vw_referencias_total.Where(x=>x.codigo==anioModelo).ToList();
                var result = new
                {
                    buscaColor,
                    esNuevo = false
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult BrowserVHConAlistamiento(int? menu)
        {
            if (Session["user_usuarioid"] != null)
            {
                BuscarFavoritos(menu);
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }

        }

        public JsonResult BuscarVHConAlistamiento()
        {

            if (Session["user_usuarioid"] != null)
            {
                var bodega = 1;
                var bodega2 = int.TryParse(Session["user_bodega"].ToString(), out bodega);
                if (bodega2 == true)
                {
                    var usuariox = Convert.ToInt32(Session["user_usuarioid"].ToString());

                    var listabodega = db.bodega_usuario.Where(d => d.id_usuario == usuariox).Select(d => d.id_bodega).ToList();
                    var info = (from vpe in db.vw_pendientesEntrega
                                join vehi in db.icb_vehiculo on vpe.planmayor equals vehi.plan_mayor into vu
                                from vehi in vu.DefaultIfEmpty()
                                where vpe.planmayor != null && vpe.pazysalvo == false && vpe.FechaFinAlistamiento != null && listabodega.Contains(vpe.bodega)
                                select new { vpe, vehi }).ToList();

                    var data = info.Select(p => new
                    {
                        p.vpe.id,
                        p.vpe.numero,
                        planmayor = p.vpe.planmayor != null ? p.vpe.planmayor : "",
                        anio = p.vpe.anio_vh != null ? p.vpe.anio_vh.ToString() : "",
                        modelo = p.vpe.modelo != null ? p.vpe.modelo : "",
                        color = p.vpe.colvh_nombre != null ? p.vpe.colvh_nombre : "",
                        vin = p.vpe.vin != null ? p.vpe.vin : "",
                        p.vpe.bodega,
                        nombreBodega = p.vpe.bodccs_nombre != null ? p.vpe.bodccs_nombre : "",
                        ubicacion = p.vpe.ubivh_nombre != null ? p.vpe.ubivh_nombre : "",
                        cliente = p.vpe.doc_tercero + "-" + p.vpe.cliente,
                        p.vpe.asesor,
                        soat = p.vpe.numerosoat != null ? p.vpe.numerosoat : "",
                        pedidoFecha = p.vpe.FechaPedido != null ? p.vpe.FechaPedido.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                        facturaFecha = p.vpe.FechaVenta != null ? p.vpe.FechaVenta.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                        tramitesFecha = p.vpe.FechaTramite != null ? p.vpe.FechaTramite.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                        matriculaFecha = p.vpe.FechaMatricula != null ? p.vpe.FechaMatricula.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                        manifiestoFecha = p.vpe.FechaManifiesto != null ? p.vpe.FechaManifiesto.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : !string.IsNullOrWhiteSpace(p.vehi.plan_mayor) ? p.vehi.fecentman_vh != null ? p.vehi.fecentman_vh.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "" : "",
                        inicioFecha = p.vpe.FechaAlistamiento != null ? p.vpe.FechaAlistamiento.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                        finFecha = p.vpe.FechaFinAlistamiento != null ? p.vpe.FechaFinAlistamiento.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                        p.vpe.IdTecnicoFinAlistamiento,
                        p.vpe.NombreTecnicoFinAlistamiento,
                        entregaFecha = p.vpe.FechaEntrega != null ? p.vpe.FechaEntrega.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                        programacionFecha = p.vpe.FechaProgramacioEntrega != null ? p.vpe.FechaProgramacioEntrega.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                        p.vpe.pazysalvo,
                        usupazysalvo = p.vpe.usupazysalvo != null ? p.vpe.usupazysalvo : "",
                        numFactura = p.vpe.numfactura != null ? p.vpe.numfactura.ToString() : ""
                    });

                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(0, JsonRequestBehavior.AllowGet);

                }

            }
            else
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CheckListAlistamiento(string planM, int? tecnico, int? menu)
        {
            var tercero = db.icb_vehiculo.FirstOrDefault(x => x.plan_mayor == planM).propietario;
            ViewBag.tercero = tercero;
            ViewBag.tecnico = tecnico;
            ViewBag.placa = planM;
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult CheckListAlistamiento(vencabingresovehiculo modelo, int? menu)
        {
            var preguntas = int.Parse(Request["numeroParametros"]);
            //var tecnico = int.Parse(Request["idTecnico"]);
            modelo.entrega = true;
            modelo.recepcion = false;
            modelo.fecha = DateTime.Now;
            modelo.fec_creacion = DateTime.Now;
            modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
            if (modelo.tercero == null || modelo.tercero == 0)
            {
                var sinDoc = db.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0").tercero_id;
                modelo.tercero = sinDoc;
            }

            db.vencabingresovehiculo.Add(modelo);


            //busco el parámetro de sistema de evento fin alistamiento
            var ali1 = db.icb_sysparameter.Where(d => d.syspar_cod == "P66").FirstOrDefault();
            var eventoalistamiento = ali1 != null ? Convert.ToInt32(ali1.syspar_value) : 22;
            //busco en la lista de eventos el usuario que registró el evento de fin de alistamiento para este vehículo
            var evento = db.icb_vehiculo_eventos.OrderByDescending(d => d.fechaevento).Where(d => d.icb_tpeventos.codigoevento == eventoalistamiento && (d.icb_vehiculo.plac_vh == modelo.placa || d.icb_vehiculo.plan_mayor == modelo.placa)).FirstOrDefault();

            if (modelo.aceptaalistamiento == false)
            {
                var buscarUsuario = evento != null ? evento.eventouserid_creacion : modelo.userid_creacion;
                tareasasignadas tarea = new tareasasignadas();
                tarea.fec_creacion = DateTime.Now;
                tarea.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                tarea.estado = true;
                tarea.idusuarioasignado = buscarUsuario;
                tarea.notas = "Devolución por no conformidad de alistamiento";
                tarea.observaciones = modelo.Observacion;
                db.tareasasignadas.Add(tarea);
            }

            var guardar = db.SaveChanges();
            if (guardar > 0)
            {
                var ultimoEncabezado = modelo.id;
                for (var i = 0; i <= preguntas; i++)
                {
                    var parametros = Request["parametros" + i];
                    var respuestas = Request["respuestas" + i];
                    respuestas = respuestas != null ? respuestas : "";
                    if (parametros != null && respuestas != null)
                    {
                        if (respuestas == "on")
                        {
                            db.vdetalleingresovehiculo.Add(new vdetalleingresovehiculo()
                            {
                                idencabezado = ultimoEncabezado,
                                iditeminspeccion = Convert.ToInt32(parametros),
                                respuesta = "true"
                            });
                        }
                        else
                        {
                            db.vdetalleingresovehiculo.Add(new vdetalleingresovehiculo()
                            {
                                idencabezado = ultimoEncabezado,
                                iditeminspeccion = Convert.ToInt32(parametros),
                                respuesta = respuestas
                            });
                        }
                    }
                }
                var guardarRespuestas = db.SaveChanges();
                if (guardarRespuestas > 0)
                {
                    TempData["mensaje"] = "El check list de alistamiento fue registrado de manera exitosa";
                }
                else
                {
                    TempData["mensaje_error"] = "Error registrar el check list de alistamiento, por favor intente nuevamente";
                }
            }
            return RedirectToAction("BrowserVHConAlistamiento");
        }

        public JsonResult pintarChackListAlistamiento()
        {
            var parametro = Convert.ToInt32(db.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P74").syspar_value);

            var buscarCheck = (from checks in db.vingresovehiculo
                               where checks.tipoCheckid == parametro
                               select new
                               {
                                   id_descripcion = checks.id,
                                   checks.descripcion,
                                   checks.tiporespuesta,
                                   opciones = db.vingresovehiculoopcion.Where(x => x.id_ingreso == checks.id).Select(x => new { x.id, x.descripcion }).ToList()
                               }).ToList();

            return Json(buscarCheck, JsonRequestBehavior.AllowGet);
        }

        public class docNecesarios
        {
            public int id { get; set; }
            public string documento { get; set; }
            public int? iddocumento { get; set; }
            public int? cargado { get; set; }
            public bool estado { get; set; }
        }

        public class ListaPersonas
        {
            public List<docNecesarios> ListaDocNecesarios { get; set; }
        }

        public JsonResult verificarPendientes(int idPedido)
        {
            // se eliminaron las validaciones del saldo el dia 28/02/2019 por tarea 3055 - 543
            var docPendiente = 0;
            var infoPedido = db.vpedido.FirstOrDefault(x => x.id == idPedido);
            var flotaAux = infoPedido.flota;
            var tercero = (from p in db.vpedido
                           join t in db.icb_terceros
                           on p.nit equals t.tercero_id
                           select t.doc_tercero).FirstOrDefault();

            var tipoPer = from t in db.icb_terceros
                          join d in db.tp_documento
                          on t.tpdoc_id equals d.tpdoc_id
                          where t.doc_tercero == tercero
                          select new
                          {
                              tipo = d.tipo.Trim()
                          };
            ListaPersonas Lista = new ListaPersonas();
            if (flotaAux != null)
            {
                var flota = db.vflota.Find(flotaAux);

                if (flota != null)
                {
                    Lista.ListaDocNecesarios = (from v in db.vvalidacionpeddoc
                                                join r in db.vdocrequeridosflota
                                                on v.iddocrequerido equals r.id
                                                join d in db.vdocumentosflota
                                                on r.iddocumento equals d.id
                                                where r.codflota == flotaAux && v.idpedido == infoPedido.id
                                                select new docNecesarios
                                                {
                                                    id = d.id,
                                                    documento = d.documento,
                                                    iddocumento = d.iddocumento,
                                                    estado = v.estado,
                                                    cargado = db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x => x.idpedido == infoPedido.id && x.iddocumento == d.iddocumento) != null ?
                                                              db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x => x.idpedido == infoPedido.id && x.iddocumento == d.iddocumento).id : 0
                                                }).ToList();
                }
            }
            else if (tipoPer.ToString() == "N")
            {
                Lista.ListaDocNecesarios = (from v in db.vvalidacionpeddoc
                                            join d in db.vdocumentosflota
                                            on v.iddocrequerido equals d.id
                                            where d.id_tipo_documento == 3 && v.idpedido == infoPedido.id
                                            select new docNecesarios
                                            {
                                                id = d.id,
                                                documento = d.documento,
                                                iddocumento = d.iddocumento,
                                                estado = v.estado,
                                                cargado = db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x => x.idpedido == infoPedido.id && x.iddocumento == d.iddocumento) != null ?
                                                          db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x => x.idpedido == infoPedido.id && x.iddocumento == d.iddocumento).id : 0
                                            }).ToList();
            }
            else if (tipoPer.ToString() != "N")
            {
                Lista.ListaDocNecesarios = (from v in db.vvalidacionpeddoc
                                            join d in db.vdocumentosflota
                                            on v.iddocrequerido equals d.id
                                            where d.id_tipo_documento == 2 && v.idpedido == infoPedido.id
                                            select new docNecesarios
                                            {
                                                id = d.id,
                                                documento = d.documento,
                                                iddocumento = d.iddocumento,
                                                estado = v.estado,
                                                cargado = db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x => x.idpedido == infoPedido.id && x.iddocumento == d.iddocumento) != null ?
                                                          db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x => x.idpedido == infoPedido.id && x.iddocumento == d.iddocumento).id : 0
                                            }).ToList();
            }

            foreach (var item in Lista.ListaDocNecesarios)
            {
                if (item.estado == false)
                {
                    docPendiente = 1;
                }
            }

            if (docPendiente == 1)
            {
                return Json(new { solicitar = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { solicitar = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult buscarCreditoSolicitado(int idPedido)
        {
            var info = (from a in db.vpedpago
                        join b in db.vformapago
                        on a.condicion equals b.id
                        where a.idpedido == idPedido && a.condicion == 1
                        select new
                        {
                            a.valor
                        }).ToList();

            return Json(info, JsonRequestBehavior.AllowGet);
        }

        //nueva paginacion
        /*public JsonResult BuscarPedidosPaginados(string filtros, string valorFiltros, string filtroGeneral)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            var search = Request.Form.GetValues("search[value]").FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            search = search.Replace(" ", "");
            int pageSize = Convert.ToInt32(length);
            int skip = Convert.ToInt32(start) / pageSize;

            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

            var bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            var busquedaDocCliente = "";
            var busquedaNombreCliente = "";
            var busquedaNombreAsesor = "";
            var busquedaModelo = "";
            var busquedaPlanMayor = "";
            var busquedaFecha = "";
            var busquedaNombreBodega = "";
            var busquedaValor = "";
            var busquedaNumeroPedido = "";
            string[] vectorNombresFiltro = !string.IsNullOrEmpty(filtros) ? filtros.Split(',') : new string[1];
            string[] vectorValoresFiltro = !string.IsNullOrEmpty(valorFiltros) ? valorFiltros.Split(',') : new string[1];
            for (int i = 0; i < vectorNombresFiltro.Length; i++)
            {
                busquedaDocCliente = busquedaDocCliente == "" ? vectorNombresFiltro[i] == "doc_tercero" ? vectorValoresFiltro[i] : "" : busquedaDocCliente;
                busquedaNombreCliente = busquedaNombreCliente == "" ? vectorNombresFiltro[i] == "prinom_tercero" ? vectorValoresFiltro[i] : "" : busquedaNombreCliente;
                busquedaNombreAsesor = busquedaNombreAsesor == "" ? vectorNombresFiltro[i] == "tpdoc_id" ? vectorValoresFiltro[i] : "" : busquedaNombreAsesor;
                busquedaModelo = busquedaModelo == "" ? vectorNombresFiltro[i] == "numero" ? vectorValoresFiltro[i] : "" : busquedaModelo;
                busquedaPlanMayor = busquedaPlanMayor == "" ? vectorNombresFiltro[i] == "documento" ? vectorValoresFiltro[i] : "" : busquedaPlanMayor;
                busquedaNombreBodega = busquedaNombreBodega == "" ? vectorNombresFiltro[i] == "bodccs_nombre" ? vectorValoresFiltro[i] : "" : busquedaNombreBodega;
                busquedaValor = busquedaValor == "" ? vectorNombresFiltro[i] == "valor_total" ? vectorValoresFiltro[i] : "" : busquedaValor;
                busquedaNumeroPedido = busquedaNumeroPedido == "" ? vectorNombresFiltro[i] == "numeroCompra" ? vectorValoresFiltro[i] : "" : busquedaNumeroPedido;
                busquedaFecha = busquedaFecha == "" ? vectorNombresFiltro[i] == "fecha" ? vectorValoresFiltro[i] : "" : busquedaFecha;
            }
            var fechaFiltroFormateada = !string.IsNullOrEmpty(busquedaFecha) ? Convert.ToDateTime(busquedaFecha).ToString("dd/MM/yyyy") : "";
            fechaFiltroFormateada = fechaFiltroFormateada != "" ? "and encabezado.fecha between '" + fechaFiltroFormateada + "' and dateadd(day,1,('" + fechaFiltroFormateada + "'))" : "";
            //var consultaDocumentoFiltro = busquedatpdoc_id != "" ? "and encabezado.tipo = " + busquedatpdoc_id : "";

            var predicado = PredicateBuilder.True<vw_browserBackOffice>();
            if (!string.IsNullOrWhiteSpace(busquedaDocCliente))
            {
                predicado = predicado.And(f => f.doc_tercero == busquedaDocCliente);
            }
            if (!string.IsNullOrWhiteSpace(busquedaNombreCliente))
            {
                predicado = predicado.And(f => f.cliente.Contains(busquedaNombreCliente));
            }

            if (!string.IsNullOrWhiteSpace(busquedaNombreCliente))
            {
                predicado = predicado.And(f => f.cliente == busquedaNombreCliente);
            }
            var consulta = db.vw_browserBackOffice.OrderBy(f => f.id).Where(predicado).Skip(skip).Take(pageSize).ToList();

            return Json(consulta, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarFiltrosPestanaDos(int id_menu)
        {
            var buscarFiltros = (from menuBusqueda in db.menu_busqueda
                                 where menuBusqueda.menu_busqueda_id_menu == id_menu && menuBusqueda.menu_busqueda_id_pestana == 2
                                 select new
                                 {
                                     menuBusqueda.menu_busqueda_nombre,
                                     menuBusqueda.menu_busqueda_tipo_campo,
                                     menuBusqueda.menu_busqueda_campo,
                                     menuBusqueda.menu_busqueda_consulta
                                 }).ToList();

            List<ListaFiltradaModel> listas = new List<ListaFiltradaModel>();
            foreach (var item in buscarFiltros)
            {
                if (item.menu_busqueda_tipo_campo == "select")
                {
                    string queryString = item.menu_busqueda_consulta;
                    string connectionString = @"Data Source=WIN-DESARROLLO\DEVSQLSERVER;Initial Catalog=Iceberg_Project2;User ID=simplexwebuser;Password=Iceberg05;MultipleActiveResultSets=True;Application Name=EntityFramework";
                    ListaFiltradaModel nuevaLista = new ListaFiltradaModel();
                    nuevaLista.NombreAMostrar = item.menu_busqueda_nombre;
                    nuevaLista.NombreCampo = item.menu_busqueda_campo;
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand command = new SqlCommand(queryString, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        try
                        {
                            while (reader.Read())
                            {
                                int id = Convert.ToInt32(reader[0]);
                                var valor = (reader[1]);
                                nuevaLista.items.Add(new SelectListItem() { Text = (string)valor, Value = Convert.ToString(id) });
                            }
                            listas.Add(nuevaLista);
                        }
                        finally
                        {
                            // Always call Close when done reading.
                            reader.Close();
                        }
                    }
                }
            }
            return Json(new { buscarFiltros, listas }, JsonRequestBehavior.AllowGet);
        }*/

        public void EliminarRepuestos(int id)
        {
            var dato = db.vpedrepuestos.Find(id);
            db.Entry(dato).State = System.Data.Entity.EntityState.Deleted;
            db.SaveChanges();

        }

        public void EliminarRetomas(int id)
        {
            var dato = db.vpedretoma.Find(id);
            db.Entry(dato).State = System.Data.Entity.EntityState.Deleted;
            db.SaveChanges();

        }

        public void EliminarFpagos(int id)
        {
            var dato = db.vpedpago.Find(id);
            db.Entry(dato).State = System.Data.Entity.EntityState.Deleted;
            db.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        bool CheckFileType(string fileName)
        {
            string ext = Path.GetExtension(fileName);
            switch (ext.ToLower())
            {
                case ".pdf":
                    return true;
                case ".doc":
                    return true;
                case ".docx":
                    return true;
                case ".odf":
                    return true;
                case ".jpg":
                    return true;
                case ".gif":
                    return true;
                case ".png":
                    return true;
                case ".jpeg":
                    return true;
                default:
                    return false;
            }
        }

        public void BuscarFavoritos(int? menu)
        {
            var usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

            var buscarFavoritosSeleccionados = (from favoritos in db.favoritos
                                                join menu2 in db.Menus
                                                on favoritos.idmenu equals menu2.idMenu
                                                where favoritos.idusuario == usuarioActual && favoritos.seleccionado == true
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
                ViewBag.Favoritos = "<div id='areaFavoritos'><i class='fa fa-close'></i>&nbsp;&nbsp;<a href='#' onclick='AgregarQuitarFavorito();return false;'>Quitar de Favoritos</a><div>";
            }
            else
            {
                ViewBag.Favoritos = "<div id='areaFavoritos'><i class='fa fa-check'></i>&nbsp;&nbsp;<a href='#' onclick='AgregarQuitarFavorito();return false;'>Agregar a Favoritos</a></div>";
            }
            ViewBag.id_menu = menu != null ? menu : 0;
        }

    }
}



public class datosSubmenu
{
    public int id { get; set; }
    public int bodega { get; set; }
    public int? numero { get; set; }
    public string modelo { get; set; }
    public string planmayor { get; set; }
    public int? idCliente { get; set; }
    public bool? autorizado { get; set; }

}