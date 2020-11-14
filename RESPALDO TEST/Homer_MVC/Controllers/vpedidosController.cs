using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
using OfficeOpenXml;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class vpedidosController : Controller
    {
        // laura actualizacion
        //jairo 001
        //Aca esta el comentario del 22 04 19 1:23 pm

        private readonly Iceberg_Context db = new Iceberg_Context();
        private readonly CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
        private readonly int isNoExiste = 0;
        private int bahia;
        private int bodegaActual;
        private string codigoIOT = "";
        private int iCreateOt;
        private int idDocOtBodega;
        private int idtpbahia_alis;
        private int iEnvAlis;
        private int iEnvRAlis;
        private int iExecutionOt;
        private int iFinAlis;
        private int iFinishOt;
        private int iFinRAlis;
        private int iIdOperacion;
        private string sFinAlis;
        public string Nota { get; set; }
        public string Motivo { get; set; }

        public string NumeroEnLetras(string num)
        {
            string res, dec = "";
            long entero;
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
                dec = " CON " + decimales + "/100";
            }

            res = toText(Convert.ToDouble(entero)) + dec;
            return res;
        }

        public JsonResult buscarCreditosmodal(string cedula)
        {
            int buscar = db.icb_terceros.FirstOrDefault(x => x.doc_tercero == cedula).tercero_id;

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
            if (value == 0)
            {
                Num2Text = "CERO";
            }
            else if (value == 1)
            {
                Num2Text = "UNO";
            }
            else if (value == 2)
            {
                Num2Text = "DOS";
            }
            else if (value == 3)
            {
                Num2Text = "TRES";
            }
            else if (value == 4)
            {
                Num2Text = "CUATRO";
            }
            else if (value == 5)
            {
                Num2Text = "CINCO";
            }
            else if (value == 6)
            {
                Num2Text = "SEIS";
            }
            else if (value == 7)
            {
                Num2Text = "SIETE";
            }
            else if (value == 8)
            {
                Num2Text = "OCHO";
            }
            else if (value == 9)
            {
                Num2Text = "NUEVE";
            }
            else if (value == 10)
            {
                Num2Text = "DIEZ";
            }
            else if (value == 11)
            {
                Num2Text = "ONCE";
            }
            else if (value == 12)
            {
                Num2Text = "DOCE";
            }
            else if (value == 13)
            {
                Num2Text = "TRECE";
            }
            else if (value == 14)
            {
                Num2Text = "CATORCE";
            }
            else if (value == 15)
            {
                Num2Text = "QUINCE";
            }
            else if (value < 20)
            {
                Num2Text = "DIECI" + toText(value - 10);
            }
            else if (value == 20)
            {
                Num2Text = "VEINTE";
            }
            else if (value < 30)
            {
                Num2Text = "VEINTI" + toText(value - 20);
            }
            else if (value == 30)
            {
                Num2Text = "TREINTA";
            }
            else if (value == 40)
            {
                Num2Text = "CUARENTA";
            }
            else if (value == 50)
            {
                Num2Text = "CINCUENTA";
            }
            else if (value == 60)
            {
                Num2Text = "SESENTA";
            }
            else if (value == 70)
            {
                Num2Text = "SETENTA";
            }
            else if (value == 80)
            {
                Num2Text = "OCHENTA";
            }
            else if (value == 90)
            {
                Num2Text = "NOVENTA";
            }
            else if (value < 100)
            {
                Num2Text = toText(Math.Truncate(value / 10) * 10) + " Y " + toText(value % 10);
            }
            else if (value == 100)
            {
                Num2Text = "CIEN";
            }
            else if (value < 200)
            {
                Num2Text = "CIENTO " + toText(value - 100);
            }
            else if (value == 200 || value == 300 || value == 400 || value == 600 || value == 800)
            {
                Num2Text = toText(Math.Truncate(value / 100)) + "CIENTOS";
            }
            else if (value == 500)
            {
                Num2Text = "QUINIENTOS";
            }
            else if (value == 700)
            {
                Num2Text = "SETECIENTOS";
            }
            else if (value == 900)
            {
                Num2Text = "NOVECIENTOS";
            }
            else if (value < 1000)
            {
                Num2Text = toText(Math.Truncate(value / 100) * 100) + " " + toText(value % 100);
            }
            else if (value == 1000)
            {
                Num2Text = "MIL";
            }
            else if (value < 2000)
            {
                Num2Text = "MIL " + toText(value % 1000);
            }
            else if (value < 1000000)
            {
                Num2Text = toText(Math.Truncate(value / 1000)) + " MIL";
                if (value % 1000 > 0)
                {
                    Num2Text = Num2Text + " " + toText(value % 1000);
                }
            }

            else if (value == 1000000)
            {
                Num2Text = "UN MILLON";
            }
            else if (value < 2000000)
            {
                Num2Text = "UN MILLON " + toText(value % 1000000);
            }
            else if (value < 1000000000000)
            {
                Num2Text = toText(Math.Truncate(value / 1000000)) + " MILLONES ";
                if (value - Math.Truncate(value / 1000000) * 1000000 > 0)
                {
                    Num2Text = Num2Text + " " + toText(value - Math.Truncate(value / 1000000) * 1000000);
                }
            }

            else if (value == 1000000000000)
            {
                Num2Text = "UN BILLON";
            }
            else if (value < 2000000000000)
            {
                Num2Text = "UN BILLON " + toText(value - Math.Truncate(value / 1000000000000) * 1000000000000);
            }

            else
            {
                Num2Text = toText(Math.Truncate(value / 1000000000000)) + " BILLONES";
                if (value - Math.Truncate(value / 1000000000000) * 1000000000000 > 0)
                {
                    Num2Text = Num2Text + " " + toText(value - Math.Truncate(value / 1000000000000) * 1000000000000);
                }
            }

            return Num2Text;
        }

        public JsonResult BuscarNitsFacturaProforma()
        {
            var buscarNits = db.icb_terceros.Where(x => x.tpdoc_id == 1).Select(x => new
            {
                razon_social = x.razon_social != null
                    ? x.doc_tercero + " - " + x.razon_social
                    : x.doc_tercero + " - " + x.prinom_tercero + " - " + x.segnom_tercero + " " + x.apellido_tercero +
                      " " + x.segapellido_tercero,
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
                                 select new
                                 {
                                     /*tercero.direc_tercero,*/
                                     j.ciu_nombre
                                 }).FirstOrDefault();

            return Json(buscarTercero, JsonRequestBehavior.AllowGet);
        }

        public ActionResult
            FacturaProforma(
                string info) //int? id,int? idFacPro, int dirigido, string direccion, string ciudad, string observacion, decimal valorSolicitado
        {
            if (info != null)
            {
                string[] array;
                int id = 0;
                int idFacPro = 0;
                int dirigido = 0;
                string direccion = "";
                string ciudad = "";
                string observacion = "";
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
                    valorSolicitado = Convert.ToDecimal(array[6],new CultureInfo("is-IS"));
                }

                string direcOK = "";
                for (int i = 0; i < direccion.Count(); i++)
                {
                    direcOK = direccion.Replace("|N|", "#");
                }

                string obsOK = "";
                for (int i = 0; i < observacion.Count(); i++)
                {
                    obsOK = observacion.Replace("|N|", "#");
                }

                icb_terceros t = db.icb_terceros.FirstOrDefault(x => x.tercero_id == dirigido);
                string buscardirigido = t.razon_social != null
                    ? t.doc_tercero + " - " + t.razon_social
                    : t.doc_tercero + " - " + t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero +
                      " " + t.segapellido_tercero;
                string dirigidoA = buscardirigido;

                facturaProforma buscar = db.facturaProforma.Where(x => x.id == idFacPro).FirstOrDefault();
                if (buscar == null)
                {
                    facturaProforma facpro = new facturaProforma
                    {
                        idPedido = Convert.ToInt32(id),
                        direccion = direcOK,
                        ciudad = ciudad,
                        valorSolicitado = valorSolicitado,
                        observaciones = observacion,
                        dirigido = dirigido,
                        fecha_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                    };
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

                string root = Server.MapPath("~/Pdf/");
                string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
                string path = Path.Combine(root, pdfname);
                path = Path.GetFullPath(path);

                decimal total = valorSolicitado;
                CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
                string formatoNumericoVrTotal = total.ToString("0,0", elGR);
                FacturaProformaModel modelo = new FacturaProformaModel
                {
                    cliente = buscaPedido.prinom_tercero + " " + buscaPedido.segnom_tercero + " " +
                              buscaPedido.apellido_tercero + " " + buscaPedido.segapellido_tercero,
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

                ViewAsPdf something = new ViewAsPdf("FacturaProforma", modelo);
                return something;
            }

            return View();
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
                            cliente = h.razon_social != null
                                ? h.razon_social
                                : h.prinom_tercero + " " + h.segnom_tercero + " " + h.apellido_tercero + " " +
                                  h.segapellido_tercero
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
                                       plac_vh = pedido.planmayor != null && pedido.planmayor != string.Empty
                                           ? pedido.icb_vehiculo.plac_vh
                                           : "",
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
                                       vin = pedido.planmayor != null && pedido.planmayor != string.Empty
                                           ? pedido.icb_vehiculo.vin
                                           : "",
                                       pedido.bodega_concesionario.bodccs_cod,
                                       pedido.bodega_concesionario.bodccs_direccion,
                                       pedido.bodega_concesionario.bodccs_nombre,

                                       ciu_concesionario = n.ciu_nombre,
                                       b.dpto_nombre,
                                       ocu.tpocupacion_nombre,
                                       edoCivil.edocivil_nombre,
                                       vendedor = pedido.vendedor != null ? vend.user_nombre + " " + vend.user_apellido : "",
                                       user_numIdent = pedido.vendedor != null ? vend.user_numIdent.ToString() : "",
                                       pedido.id,
                                       pedido.aprobado_por
                                   }).FirstOrDefault();


                users aprobadoPor = db.users.Where(x => x.user_id == buscaPedido.aprobado_por).FirstOrDefault();

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

                    string buscarFinanciera = (from a in db.icb_unidad_financiera
                                               join b in db.vpedpago
                                                   on a.financiera_id equals b.banco
                                               join c in db.vpedido
                                                   on b.idpedido equals c.id
                                               where c.id == id
                                               select a.financiera_nombre).FirstOrDefault();


                    decimal? buscarCredito = (from a in db.vpedpago
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
                    var docCliente = (from pedido in db.vw_pedidos
                                      join pedidos in db.vpedido
                                          on pedido.numero equals pedidos.numero
                                      where pedidos.id == id
                                      select new
                                      {
                                          pedido.doc_tercero
                                      }).ToList();
                    string documentoTercero = "";
                    for (int i = 0; i < docCliente.Count; i++)
                    {
                        documentoTercero = docCliente[i].doc_tercero;
                    }

                    var terceroid = (from cliente in db.icb_terceros
                                     where cliente.doc_tercero == documentoTercero
                                     select new
                                     {
                                         tercero = cliente.tercero_id
                                     }).ToList();
                    var info = terceroid.Select(x => new
                    {
                        idtercero = x.tercero
                    }).ToList();
                    int idtercero = 0;
                    for (int i = 0; i < terceroid.Count; i++)
                    {
                        idtercero = terceroid[i].tercero;
                    }

                    var buscarContactoC = (from tercero in db.icb_terceros
                                           join contacto in db.icb_contacto_tercero
                                               on tercero.tercero_id equals contacto.tercero_id
                                           join pedidos in db.vw_pedidos
                                               on tercero.doc_tercero equals pedidos.doc_tercero
                                           join pedido in db.vpedido
                                               on pedidos.numero equals pedido.numero
                                           where contacto.tipocontacto == 3 && contacto.tercero_id == idtercero
                                           select new
                                           {
                                               nombreContacto = contacto.con_tercero_nombre,
                                               documento = contacto.cedula,
                                               telefono = contacto.con_tercero_telefono
                                           }).ToList();

                    var Contacto = buscarContactoC.Select(x => new
                    {
                        nombreC = x.nombreContacto,
                        documentoC = x.documento,
                        telefonoC = x.telefono
                    }).ToList();
                    string nomAsegurados = "";
                    string docAsegurados = "";
                    for (int i = 0; i < Contacto.Count; i++)
                    {
                        nomAsegurados = Contacto[i].nombreC;
                        docAsegurados = Contacto[i].documentoC;
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

                    string root = Server.MapPath("~/Pdf/");
                    string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
                    string path = Path.Combine(root, pdfname);
                    path = Path.GetFullPath(path);

                    CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
                    decimal? valor = buscarCredito;

                    decimal total = buscaPedido.vrtotal ?? 0;
                    decimal descuento = buscaPedido.vrdescuento ?? 0;
                    decimal calcularTotal = total - descuento;
                    decimal totalReferencias = 0;
                    string accesorios = "";
                    if (buscarReferencias != null)
                    {
                        foreach (var item in buscarReferencias)
                        {
                            totalReferencias += item.vrtotal ?? 0;
                            accesorios = accesorios == ""
                                ? item.ref_descripcion
                                : accesorios + "," + item.ref_descripcion;
                        }
                    }

                    try
                    {
                        PedidoPDFModel modelo = new PedidoPDFModel
                        {
                            NumeroPedido = buscaPedido.numero ?? 0,
                            NombreBodega = buscaPedido.bodccs_nombre,
                            DireccionBodega = buscaPedido.bodccs_direccion,
                            CiudadBodega = buscaPedido.ciu_concesionario,
                            DepartamentoBodega = buscaPedido.dpto_nombre,
                            NombresCliente = buscaPedido.prinom_tercero + " " + buscaPedido.segnom_tercero + " " +
                                             buscaPedido.apellido_tercero + " " + buscaPedido.segapellido_tercero,
                            CedulaNit = buscaPedido.doc_tercero,
                            DireccionCliente = buscarDireccion.direccion,
                            CiudadCliente = buscaPedido.ciu_nombre,
                            TelefonoCliente = buscaPedido.telf_tercero,
                            CelularCliente = buscaPedido.celular_tercero,
                            ProfesionCliente = buscaPedido.tpocupacion_nombre,
                            EstadoCivil = buscaPedido.edocivil_nombre,
                            FechaNacimientoCliente = buscaPedido.fec_nacimiento != null
                                ? buscaPedido.fec_nacimiento.Value.ToShortDateString()
                                : "",
                            EmailCliente = buscaPedido.email_tercero,
                            DiaPedido = buscaPedido.fecha != null ? buscaPedido.fecha.Day.ToString() : "",
                            MesPedido = buscaPedido.fecha != null ? buscaPedido.fecha.Month.ToString() : "",
                            AnioPedido = buscaPedido.fecha != null ? buscaPedido.fecha.Year.ToString() : "",
                            ModeloVehiculo = buscaPedido.modvh_nombre,
                            AnioVehiculo = buscaPedido.anio,
                            //AnioVehiculo = buscaPedido.anio != null ? buscaPedido.anio : 0,
                            ColorVehiculo = buscaPedido.colvh_nombre,
                            PlacaVehiculo = buscaPedido.plac_vh,
                            PlanMayor = buscaPedido.planmayor,
                            ServicioVehiculo = buscaPedido.tpserv_nombre,
                            TipoVehiculo = buscaPedido.segvh_nombre,
                            Descuento = buscaPedido.vrdescuento != null
                                ? buscaPedido.vrdescuento.Value.ToString("0,0", elGR)
                                : "0",
                            PrecioAlPublico = buscaPedido.vrtotal != null
                                ? buscaPedido.vrtotal.Value.ToString("0,0", elGR)
                                : "0",
                            PrecioVenta = calcularTotal.ToString("0,0", elGR),
                            cantCuotas = buscaPedido.plazo != null ? buscaPedido.plazo.ToString() : "",
                            saldoFinanciar = buscaPedido.vaprobado != null
                                ? buscaPedido.vaprobado.Value.ToString("N0", new CultureInfo("is-IS"))
                                : "",
                            cuoInicial = buscaPedido.cuota_inicial != null
                                ? buscaPedido.cuota_inicial.Value.ToString("N0", new CultureInfo("is-IS"))
                                : "",
                            poliza = buscaPedido.poliza != null ? buscaPedido.poliza : "",
                            Accesorios = accesorios,
                            TotalAccesorios = totalReferencias.ToString("0,0", elGR),
                            PlacaVhRetoma = buscarVehiculoRetoma != null ? buscarVehiculoRetoma.placa : "",
                            ValorRetoma = buscarVehiculoRetoma != null
                                ? buscarVehiculoRetoma.valor != null
                                    ? buscarVehiculoRetoma.valor.Value.ToString("0,0", elGR)
                                    : ""
                                : "",
                            ModeloVhRetoma = buscarVehiculoRetoma != null ? buscarVehiculoRetoma.modelo : "",
                            CuotaInicial = contado.ToString("0,0", elGR),
                            financiera = buscarFinanciera != null ? buscarFinanciera : "",
                            ValorRetomaPago = buscarVehiculoRetoma != null
                                ? buscarVehiculoRetoma.valor != null
                                    ? buscarVehiculoRetoma.valor.Value.ToString("0,0", elGR)
                                    : ""
                                : "",
                            SaldoFinanciar = credito.ToString("0,0", elGR),
                            Total = totalAPagar.ToString("0,0", elGR),
                            CedulaVendedor = buscaPedido.user_numIdent,
                            valorCredito = valor != null ? valor.Value.ToString("0,0", elGR) : "",
                            Vendedor = buscaPedido.vendedor,
                            //caso 2056
                            NombresAsegurados = nomAsegurados,
                            DocumentosAsegurados = docAsegurados,
                            CedulaAprobado = aprobadoPor != null ? aprobadoPor.user_numIdent.ToString() : "",
                            NombreAprobado = aprobadoPor != null
                                ? aprobadoPor.user_nombre + " " + aprobadoPor.user_apellido
                                : ""
                            //fin caso 2056
                        };

                        ViewAsPdf something = new ViewAsPdf("ContratoCompraventa", modelo);
                        return something;
                    }
                    catch (Exception es)
                    {
                        Exception mensaje = es.InnerException;
                        throw;
                    }

                    //return View();
                }

                TempData["mensaje_error"] = "No se puede generar el pdf";
                return RedirectToAction("Edit", "vpedidos", new { id, menu = "5278" });
            }

            TempData["mensaje_error"] = "No se ha seleccionado un número de pedido válido";
            return RedirectToAction("Create", "vpedidos");
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
                               cedula = t.prinom_tercero != null
                                   ? t.doc_tercero + " - " + t.prinom_tercero + " " + t.apellido_tercero + " " +
                                     t.segapellido_tercero
                                   : t.doc_tercero + " - " + t.razon_social,
                               t.tercero_id
                           };
            ViewBag.rol = Session["user_rolid"];
            int usuarioId = Convert.ToInt32(Session["user_usuarioid"]);
            users usuarioDatos = db.users.FirstOrDefault();
            if (vpedidos.vendedor == null)
            {
                usuarioDatos = db.users.Where(d => d.user_id == usuarioId).FirstOrDefault();
            }
            else
            {
                usuarioDatos = db.users.Where(d => d.user_id == vpedidos.vendedor).FirstOrDefault();
            }

            int rol = Convert.ToInt32(Session["user_rolid"]);
            //busco si el rol tiene permiso de modificar impuesto de consumo
            icb_sysparameter paramimpu = db.icb_sysparameter.Where(d => d.syspar_cod == "P121").FirstOrDefault();
            int permisorol1 = paramimpu != null ? Convert.ToInt32(paramimpu.syspar_value) : 1029;
            //veo si el permiso está asignado a este rol
            int permisos = db.rolacceso.Where(d => d.idrol == rol && d.idpermiso == permisorol1).Count();
            if (permisos > 0)
            {
                ViewBag.permiso = 1;
            }
            else
            {
                ViewBag.permiso = 0;
            }
            //busco si el rol tiene permiso de modificar nombre de asesor
            icb_sysparameter perModAse = db.icb_sysparameter.Where(d => d.syspar_cod == "P122").FirstOrDefault();
            int permisorol2 = perModAse != null ? Convert.ToInt32(perModAse.syspar_value) : 1030;
            //veo si el permiso está asignado a este rol

            List<SelectListItem> nit = new List<SelectListItem>();
            List<SelectListItem> nit2 = new List<SelectListItem>();
            List<SelectListItem> nit3 = new List<SelectListItem>();
            List<SelectListItem> nit4 = new List<SelectListItem>();
            List<SelectListItem> nit5 = new List<SelectListItem>();
            List<SelectListItem> nit_asegurado = new List<SelectListItem>();
            List<SelectListItem> nitPrenda = new List<SelectListItem>();

            foreach (var item in clientes)
            {
                nit.Add(new SelectListItem
                {
                    Text = item.cedula,
                    Value = item.tercero_id.ToString(),
                    Selected = item.tercero_id == vpedidos.nit ? true : false
                });
                nit2.Add(new SelectListItem
                {
                    Text = item.cedula,
                    Value = item.tercero_id.ToString(),
                    Selected = item.tercero_id == vpedidos.nit2 ? true : false
                });
                nit3.Add(new SelectListItem
                {
                    Text = item.cedula,
                    Value = item.tercero_id.ToString(),
                    Selected = item.tercero_id == vpedidos.nit3 ? true : false
                });
                nit4.Add(new SelectListItem
                {
                    Text = item.cedula,
                    Value = item.tercero_id.ToString(),
                    Selected = item.tercero_id == vpedidos.nit4 ? true : false
                });
                nit5.Add(new SelectListItem
                {
                    Text = item.cedula,
                    Value = item.tercero_id.ToString(),
                    Selected = item.tercero_id == vpedidos.nit5 ? true : false
                });
                nit_asegurado.Add(new SelectListItem
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
            List<SelectListItem> listFinancieras = new List<SelectListItem>();
            foreach (var item in nitPrendas)
            {
                listFinancieras.Add(new SelectListItem
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
            List<SelectListItem> lisAsesores = new List<SelectListItem>();
            foreach (var item in asesores)
            {
                lisAsesores.Add(new SelectListItem
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
                            f.id,
                            nombre = f.codigo + " - " + f.Descripcion
                        };
            List<SelectListItem> lisflotas = new List<SelectListItem>();
            foreach (var item in flota)
            {
                lisflotas.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString(),
                    Selected = item.id == vpedidos.codflota ? true : false
                });
            }

            var data = from v in db.vflota
                       orderby v.numero
                       select new
                       {
                           id = v.idflota,
                           nombre = v.icb_terceros.prinom_tercero != null
                               ? v.numero + " - " + v.icb_terceros.prinom_tercero + " " + v.icb_terceros.segnom_tercero + " " +
                                 v.icb_terceros.apellido_tercero + " " + v.icb_terceros.segapellido_tercero
                               : v.numero + " - " + v.icb_terceros.razon_social
                       };
            List<SelectListItem> lflota = new List<SelectListItem>();
            foreach (var item in data)
            {
                lflota.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString(),
                    Selected = item.id == vpedidos.flota ? true : false
                });
            }

            ViewBag.nit = nit;
            ViewBag.nit2 = nit2;
            ViewBag.nit3 = nit3;
            ViewBag.nit4 = nit4;
            ViewBag.nit5 = nit5;
            ViewBag.nit_asegurado = nit_asegurado;
            ViewBag.nit_prenda = listFinancieras;
            int permisos2 = db.rolacceso.Where(d => d.idrol == rol && d.idpermiso == permisorol2).Count();
            if (permisos2 > 0)
            {
                ViewBag.permiso2 = 1;
                ViewBag.vendedor = lisAsesores;
            }
            else
            {
                ViewBag.permiso2 = 0;
                ViewBag.vendedor = vpedidos.vendedor;
                ViewBag.nombreAsesor = usuarioDatos.user_nombre + " " + usuarioDatos.user_apellido;
            }

            ViewBag.Color_Deseado =
                new SelectList(db.color_vehiculo.Where(x => x.colvh_estado).OrderBy(x => x.colvh_nombre), "colvh_id",
                    "colvh_nombre", vpedidos.Color_Deseado);
            ViewBag.color_opcional =
                new SelectList(db.color_vehiculo.Where(x => x.colvh_estado).OrderBy(x => x.colvh_nombre), "colvh_id",
                    "colvh_nombre", vpedidos.color_opcional);
            int?[] pedidoscot = db.vpedido.Where(d => d.anulado == false /*|| d.anulado == true*/)
                .Select(d => d.idcotizacion).ToArray();

            List<icb_cotizacion> cotizaciones = db.icb_cotizacion.Where(d => !pedidoscot.Contains(d.cot_idserial)).ToList();
            ViewBag.idcotizacion = cotizaciones;

            ViewBag.flota = lflota;
            ViewBag.codflota = lisflotas;

            ViewBag.marcvh_id = new SelectList(db.marca_vehiculo.OrderBy(x => x.marcvh_nombre), "marcvh_id",
                "marcvh_nombre", vpedidos.marcvh_id);
            ViewBag.modelo = new SelectList(db.modelo_vehiculo.OrderBy(x => x.modvh_nombre), "modvh_codigo",
                "modvh_nombre", vpedidos.modelo);
            ViewBag.id_anio_modelo = new SelectList(db.anio_modelo.OrderBy(x => x.anio), "anio_modelo_id", "anio",
                vpedidos.id_anio_modelo);

            ViewBag.marcas = new SelectList(db.marca_vehiculo.OrderBy(x => x.marcvh_nombre), "marcvh_id",
                "marcvh_nombre", vpedidos.marcvh_id);
            ViewBag.modelosMarca = new SelectList(db.modelo_vehiculo.OrderBy(x => x.modvh_nombre), "modvh_codigo",
                "modvh_nombre", vpedidos.modelo);
            ViewBag.idAnioModelo =
                new SelectList(db.anio_modelo.Where(x => x.codigo_modelo == vpedidos.modelo).OrderBy(x => x.anio),
                    "anio_modelo_id", "anio", vpedidos.id_anio_modelo);

            var motivoCambio = (from c in db.vcambiovehiculo
                                join p in db.vpedido
                                    on c.id equals p.id
                                where c.idpedido == vpedidos.id
                                orderby c.id descending
                                select new { c.motivo }).FirstOrDefault();

            ViewBag.motivoCambio = motivoCambio;

            ViewBag.plan_venta = new SelectList(db.icb_plan_financiero, "plan_id", "plan_nombre", vpedidos.plan_venta);
            ViewBag.numpedido = vpedidos.numero;
            ViewBag.tipovh = vpedidos.nuevo ? "Nuevo" : "Usado";
            ViewBag.servicio =
                new SelectList(db.tpservicio_vehiculo.Where(x => x.tpserv_estado).OrderBy(x => x.tpserv_nombre),
                    "tpserv_id", "tpserv_nombre", vpedidos.servicio);
            ViewBag.cargomatricula = new SelectList(db.vcargomatricula.Where(x => x.estado).OrderBy(x => x.descripcion),
                "id", "descripcion", vpedidos.cargomatricula);
            ViewBag.tipo_carroceria =
                new SelectList(db.tipo_carroceria.Where(x => x.estado).OrderBy(x => x.descripcion), "idcarroceria",
                    "descripcion", vpedidos.tipo_carroceria);

            ViewBag.iddepartamento =
                new SelectList(db.nom_departamento.OrderBy(x => x.dpto_nombre).Where(x => x.dpto_estado), "dpto_id",
                    "dpto_nombre", vpedidos != null ? vpedidos.iddepartamento : 0);
            ViewBag.idciudad = new SelectList(db.nom_ciudad.OrderBy(x => x.ciu_nombre).Where(x => x.ciu_estado),
                "ciu_id", "ciu_nombre", vpedidos != null ? vpedidos.idciudad : 0);

            vpedido buscarSerialUltimoPed = db.vpedido.OrderByDescending(x => x.id).FirstOrDefault();
            ViewBag.numPedidoCreado = buscarSerialUltimoPed != null ? buscarSerialUltimoPed.numero : 0;
            if (vpedidos.id > 0)
            {
                vpedido pedidorevisar = db.vpedido.Where(d => d.id == vpedidos.id).FirstOrDefault();
                ViewBag.no_disponible = pedidorevisar.no_disponible;
                ViewBag.solicitado = pedidorevisar.solicitado;
                ViewBag.facturar = pedidorevisar.para_facturar;
                ViewBag.facturado = pedidorevisar.facturado;
                ViewBag.asignado = !string.IsNullOrWhiteSpace(pedidorevisar.planmayor) ? 1 : 0;
            }
        }


        // GET: vpedidos/Create
        public ActionResult Create(int? menu)
        {
            VehiculoPedidoModel vpedidos = new VehiculoPedidoModel();
            listas(vpedidos);
            BuscarFavoritos(menu);
            ViewBag.parametro = DateTime.Now.ToString("HH:mm:ss", new CultureInfo("en-US"));
            ViewBag.rol = Convert.ToInt32(Session["user_rolid"]);
            return View();
        }

        // POST: vpedidos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create(VehiculoPedidoModel vpedido, int? menu)
        {
            if (!string.IsNullOrWhiteSpace(Request["nombreAsesor"]))
            {
                vpedido.vendedor = Convert.ToInt32(Session["user_usuarioid"]);
            }
            string camposObligatorios = "<label>Los siguientes campos son obligatorios:<label><ul>";
            int vacio = 0;

            if (vpedido.nit == null) { camposObligatorios += "<li>Cliente / Nit</li>"; vacio = 1; }
            if (vpedido.vendedor == null) { camposObligatorios += "<li>Asesor</li>"; vacio = 1; }
            if (string.IsNullOrWhiteSpace(Request["marcvh_id"])) { camposObligatorios += "<li>Marca Vehiculo</li>"; vacio = 1; }

            if (string.IsNullOrWhiteSpace(Request["modelo"]))
            {
                camposObligatorios += "<li>Modelo Vehiculo</li>";
                vacio = 1;
            }

            if (string.IsNullOrWhiteSpace(Request["id_anio_modelo"]))
            {
                camposObligatorios += "<li>Año Vehiculo</li>";
                vacio = 1;
            }

            if (string.IsNullOrWhiteSpace(Request["servicio"]))
            {
                camposObligatorios += "<li>Servicio Vehiculo</li>";
                vacio = 1;
            }

            camposObligatorios += "</ul>";
            if (vacio == 1)
            {
                TempData["mensaje_obligatorios"] = camposObligatorios;
            }

            ViewBag.rol = Convert.ToInt32(Session["user_rolid"]);
            ViewBag.parametro = DateTime.Now.ToString("HH:mm:ss", new CultureInfo("en-US"));
            if (ModelState.IsValid || vacio == 0)
            {
                using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (vpedido.vendedor == null)
                        {
                            vpedido.vendedor = Convert.ToInt32(Session["user_usuarioid"]);
                        }

                        vpedido num_pedido = db.vpedido.OrderByDescending(x => x.numero).FirstOrDefault();

                        if (num_pedido != null)
                        {
                            vpedido.numero = num_pedido.numero + 1;
                        }
                        else
                        {
                            vpedido.numero = 1;
                        }

                        string tpvh = Request["tipoVh"];

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
                        object bodega = Session["user_bodega"];
                        vpedido.bodega = Convert.ToInt32(Session["user_bodega"]);
                        vpedido.anulado = false;
                        vpedido.fecha = DateTime.Now;
                        vpedido.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                        vpedido.fec_creacion = DateTime.Now;

                        #region creas objeto pedido de la clase vpedido

                        vpedido pedido = new vpedido
                        {
                            numero = vpedido.numero,
                            impfactura2 = vpedido.impfactura2,
                            impfactura3 = vpedido.impfactura3,
                            impfactura4 = vpedido.impfactura4,
                            bodega = vpedido.bodega,
                            anulado = vpedido.anulado,
                            fecha = Convert.ToDateTime(vpedido.fecha),
                            idcotizacion = vpedido.idcotizacion,
                            nit = vpedido.nit,
                            nit_asegurado = vpedido.nit_asegurado,
                            nit2 = vpedido.nit2,
                            nit3 = vpedido.nit3,
                            nit4 = vpedido.nit4,
                            nit5 = vpedido.nit5,
                            vendedor = vpedido.vendedor,
                            modelo = vpedido.modelo,
                            id_anio_modelo = vpedido.id_anio_modelo,
                            plan_venta = vpedido.plan_venta,
                            planmayor = vpedido.planmayor,
                            fecha_asignacion_planmayor = vpedido.fecha_asignacion_planmayor,
                            asignado_por = vpedido.asignado_por,
                            condicion = vpedido.condicion,
                            dias_validez = vpedido.dias_validez,
                            valor_unitario = Convert.ToDecimal(vpedido.valor_unitario,new CultureInfo("is-IS")),
                            porcentaje_iva = vpedido.porcentaje_iva,
                             pordscto = vpedido.pordscto,
                            vrdescuento = Convert.ToDecimal(vpedido.vrdescuento, new CultureInfo("is-IS")),
                            cantidad = vpedido.cantidad,
                            tipo_carroceria = vpedido.tipo_carroceria,
                            vrtotal = Convert.ToDecimal(vpedido.vrtotal, new CultureInfo("is-IS")),
                            moneda = vpedido.moneda,
                            id_aseguradora = vpedido.id_aseguradora,
                            notas1 = vpedido.notas1,
                            notas2 = vpedido.notas2,
                            escanje = vpedido.escanje,
                            eschevyplan = vpedido.eschevyplan,
                            esLeasing = vpedido.esLeasing,
                            esreposicion = vpedido.esreposicion,
                            nit_prenda = vpedido.nit_prenda,
                            flota = vpedido.flota,
                            codigoflota = vpedido.codflota,
                            facturado = vpedido.facturado,
                            numfactura = vpedido.numfactura,
                            porcentaje_impoconsumo = vpedido.porcentaje_impoconsumo,
                            numeroplaca = vpedido.numeroplaca,
                            motivo_anulacion = vpedido.motivo_anulacion,
                            venta_gerencia = vpedido.venta_gerencia,
                            Color_Deseado = vpedido.Color_Deseado,
                            terminacionplaca = vpedido.terminacionplaca,
                            bono = vpedido.bono,
                            marca = Convert.ToInt32(vpedido.marcvh_id),
                            idmodelo = vpedido.idmodelo,
                            nuevo = vpedido.nuevo,
                            usado = vpedido.usado,
                            servicio = vpedido.servicio,
                            placapar = vpedido.placapar,
                            placaimpar = vpedido.placaimpar,
                            color_opcional = vpedido.color_opcional,
                            cargomatricula = vpedido.cargomatricula,
                            obsequioporcen = vpedido.obsequioporcen,
                            rango_placa = vpedido.rango_placa,
                            userid_creacion = vpedido.userid_creacion,
                            fec_creacion = vpedido.fec_creacion,
                            iddepartamento = vpedido.iddepartamento,
                            idciudad = vpedido.idciudad,
                            //idunitario=vpedido.valormatricula
                        };
                        decimal valorpoliz = 0;
                        decimal valormatr = 0;
                        decimal vrcarroceria = 0;
                        decimal otrosvalores = 0;
                        decimal valorsoat = 0;
                        if (!string.IsNullOrWhiteSpace(vpedido.valorPoliza))
                        {
                            var convertir = Decimal.TryParse(vpedido.valorPoliza, NumberStyles.Number, new CultureInfo("is-IS"), out valorpoliz);
                        }
                        if (!string.IsNullOrWhiteSpace(vpedido.valormatricula))
                        {
                            var convertir = Decimal.TryParse(vpedido.valormatricula, NumberStyles.Number, new CultureInfo("is-IS"), out valormatr);
                        }
                        if (!string.IsNullOrWhiteSpace(vpedido.vrcarroceria))
                        {
                            var convertir = Decimal.TryParse(vpedido.vrcarroceria, NumberStyles.Number, new CultureInfo("is-IS"), out vrcarroceria);
                        }
                        if (!string.IsNullOrWhiteSpace(vpedido.valorsoat))
                        {
                            var convertir = Decimal.TryParse(vpedido.valorsoat, NumberStyles.Number, new CultureInfo("is-IS"), out valorsoat);
                        }
                        if (!string.IsNullOrWhiteSpace(vpedido.otrosValores))
                        {
                            var convertir = Decimal.TryParse(vpedido.otrosValores, NumberStyles.Number, new CultureInfo("is-IS"), out otrosvalores);
                        }

                        pedido.valorPoliza = valorpoliz;
                        pedido.valormatricula = valormatr;
                        pedido.vrcarroceria = vrcarroceria;
                        pedido.valorsoat = valorsoat;
                        pedido.otrosValores = otrosvalores;

                        #endregion

                        db.vpedido.Add(pedido);

                        db.SaveChanges();
                        int result = db.SaveChanges();
                        int pedido_id = db.vpedido.OrderByDescending(x => x.id).FirstOrDefault().id;

                        #region pagos

                        //se capturan las formas de pago que que se guardan en la vista con el formato ~ para separar cada forma de pago y | para separar los datos
                        string[] formas_pago = Request["formas_depago_json"].Split('~');

                        for (int i = 0; i < formas_pago.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(formas_pago[i]))
                            {
                                string[] forma_pago = formas_pago[i].Split('|');
                                vpedpago vpago = new vpedpago
                                {
                                    idpedido = pedido_id,
                                    seq = i + 1,
                                    condicion = Convert.ToInt32(forma_pago[1]), //forma de pago
                                    valor = Convert.ToDecimal(forma_pago[3], new CultureInfo("is-IS")), //Valor de la forma de pago
                                    fecpago = Convert.ToDateTime(forma_pago[4]), //Fecha de pago
                                    observaciones = forma_pago[7] //observaciones
                                };
                                string banco = forma_pago[5];
                                if (!string.IsNullOrEmpty(banco))
                                {
                                    vpago.banco = Convert.ToInt32(banco);
                                }

                                db.vpedpago.Add(vpago);
                                if (Convert.ToInt32(forma_pago[1]) == 1)
                                {
                                    bool convertir = int.TryParse(forma_pago[8], out int fpago);
                                    if (convertir)
                                    {
                                        v_creditos credito = (from c in db.v_creditos
                                                              where c.Id == fpago
                                                              select c).FirstOrDefault();
                                        if (credito != null)
                                        {
                                            credito.pedido = pedido_id;
                                            credito.estadoc = "D";
                                            credito.fec_desembolso = DateTime.Now;
                                            ;
                                            db.Entry(credito).State = EntityState.Modified;
                                        }
                                    }


                                    db.SaveChanges();
                                }
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
                                vpedrepuestos vrepuesto = new vpedrepuestos
                                {
                                    pedido_id = pedido_id,
                                    referencia = accesorio_item[1],
                                    vrunitario = Convert.ToDecimal(accesorio_item[2], new CultureInfo("is-IS")),
                                    vrtotal = Convert.ToDecimal(accesorio_item[4], new CultureInfo("is-IS")),
                                    obsequio = Convert.ToBoolean(accesorio_item[3], new CultureInfo("is-IS")),
                                    cantidad = Convert.ToInt32(accesorio_item[6], new CultureInfo("is-IS"))
                                };
                                db.vpedrepuestos.Add(vrepuesto);
                            }
                        }

                        #endregion

                        #region retomas

                        string lr = Request["lista_retomas"];
                        if (!string.IsNullOrEmpty(lr))
                        {
                            int lista_retomas = Convert.ToInt32(Request["lista_retomas"]);
                            for (int i = 1; i <= lista_retomas; i++)
                            {
                                if (!string.IsNullOrEmpty(Request["valor_retoma" + i]))
                                {
                                    vpedretoma retoma = new vpedretoma
                                    {
                                        pedido_id = pedido_id,
                                        placa = Request["placa_retoma" + i],
                                        valor = Convert.ToDecimal(Request["valor_retoma" + i], new CultureInfo("is-IS")),
                                        modelo = Request["modelo_retoma" + i]
                                    };
                                    if (!string.IsNullOrEmpty(Request["kl_retoma" + i]))
                                    {
                                        retoma.kilometraje = Convert.ToDecimal(Request["kl_retoma" + i], new CultureInfo("is-IS"));
                                    }

                                    if (!string.IsNullOrEmpty(Request["obligacion_retoma" + i]))
                                    {
                                        retoma.obligaciones = Convert.ToBoolean(Request["obligacion_retoma" + i]);
                                        if (!string.IsNullOrEmpty(Request["valor_obligacion" + i]))
                                        {
                                            retoma.valor_obligacion =
                                                Convert.ToDecimal(Request["valor_obligacion" + i], new CultureInfo("is-IS"));
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
                                vpedcostos_adicionales costos = new vpedcostos_adicionales
                                {
                                    pedido_id = pedido_id,
                                    descripcion = costo_item[1]
                                };
                                if (!string.IsNullOrEmpty(costo_item[3]))
                                {
                                    costos.valor = Convert.ToDecimal(costo_item[3], new CultureInfo("is-IS"));
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

                        vflota codflota = db.vflota.Find(vpedido.flota);
                        if (codflota != null)
                        {
                            IQueryable<vdocrequeridosflota> docrequeridosflota = db.vdocrequeridosflota.Where(x => x.codflota == codflota.flota);
                            foreach (vdocrequeridosflota item in docrequeridosflota)
                            {
                                vvalidacionpeddoc validacion = new vvalidacionpeddoc
                                {
                                    idpedido = pedido_id,
                                    idflota = vpedido.flota ?? 0,
                                    codflota = codflota.flota,
                                    iddocrequerido = item.id,
                                    estado = Convert.ToBoolean(Request["documento_" + item.id])
                                };
                                db.vvalidacionpeddoc.Add(validacion);
                            }
                        }
                        else
                        {
                            string tercero = (from p in db.vpedido
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
                                                                iddocumento = d.iddocumento
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
                                                                iddocumento = d.iddocumento
                                                            }).ToList();
                            }

                            foreach (docNecesarios item in Lista.ListaDocNecesarios)
                            {
                                vvalidacionpeddoc validacion = new vvalidacionpeddoc
                                {
                                    idpedido = pedido_id,
                                    iddocrequerido = item.id,
                                    estado = false
                                };
                                db.vvalidacionpeddoc.Add(validacion);
                            }
                        }

                        #endregion

                        #region Seguimiento

                        db.seguimientotercero.Add(new seguimientotercero
                        {
                            idtercero = vpedido.nit,
                            tipo = 4,
                            nota = "El tercero realizo un pedido con numero " + vpedido.numero,
                            fecha = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                        });

                        if (vpedido.idcotizacion != null)
                        {
                            vcotseguimiento seguimiento = new vcotseguimiento
                            {
                                cot_id = Convert.ToInt32(vpedido.idcotizacion),
                                fecha = DateTime.Now,
                                responsable = Convert.ToInt32(Session["user_usuarioid"]),
                                Notas = "Se genero pedido de vehiculo numero " + vpedido.numero,
                                Motivo = null,
                                fec_creacion = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                estado = true,
                                tipo_seguimiento = 4
                            };
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
                                vpedido ultimoAgregado = db.vpedido.OrderByDescending(x => x.numero).FirstOrDefault();
                                ViewBag.pedidoId = ultimoAgregado.numero;
                                //-----
                                TempData["mensaje"] = "Pedido registrado correctamente";

                                listas(vpedido);
                                BuscarFavoritos(menu);

                                return RedirectToAction("Create", new { vpedido.id, menu });
                            }

                            dbTran.Rollback();
                            TempData["mensaje_error"] = "Error al registrar el pedido, por favor intente nuevamente";
                            List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                                .Where(y => y.Count > 0)
                                .ToList();
                        }
                        catch (DbEntityValidationException dbEx)
                        {
                            Exception raise = dbEx;
                            foreach (DbEntityValidationResult validationErrors in dbEx.EntityValidationErrors)
                            {
                                foreach (DbValidationError validationError in validationErrors.ValidationErrors)
                                {
                                    string message = string.Format("{0}:{1}",
                                    validationErrors.Entry.Entity,
                                    validationError.ErrorMessage);
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
                    catch (DbEntityValidationException)
                    {
                        dbTran.Rollback();
                        throw;
                    }
                }
            }
            else
            {
                //TempData["mensaje_error"] = "Errores en la creación del pedido, por favor valide";
                List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
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
            ViewBag.rol = Convert.ToInt32(Session["user_rolid"]);
            int roluser = Convert.ToInt32(Session["user_rolid"]);
            string nombrerol = db.rols.Where(x => x.rol_id == roluser).Select(x => x.rol_nombre).FirstOrDefault();
            ViewBag.nombrerol = nombrerol;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            vpedido vpedido = db.vpedido.Find(id);
            if (vpedido == null)
            {
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

            string valorIva =
                (Convert.ToDecimal(vpedido.valor_unitario, new CultureInfo("is-IS")) * Convert.ToDecimal(vpedido.porcentaje_iva, new CultureInfo("is-IS")) / 100).ToString(
                    "N2", new CultureInfo("is-IS"));
            string valorImpConsumo =
                (Convert.ToDecimal(vpedido.valor_unitario, new CultureInfo("is-IS")) * Convert.ToDecimal(vpedido.porcentaje_impoconsumo, new CultureInfo("is-IS")) / 100)
                .ToString("N2", new CultureInfo("is-IS"));
            ViewBag.valorIva = valorIva;
            ViewBag.valorImpConsumo = valorImpConsumo;

            #region obj pedido

            VehiculoPedidoModel pedido = new VehiculoPedidoModel
            {
                color = buscarColor.colvh_nombre,
                serie = buscarColor.vin,
                numero = vpedido.numero,
                impfactura2 = vpedido.impfactura2,
                impfactura3 = vpedido.impfactura3,
                impfactura4 = vpedido.impfactura4,
                bodega = vpedido.bodega,
                anulado = vpedido.anulado,
                fecha = vpedido.fecha,
                idcotizacion = vpedido.idcotizacion,
                numerocotizacion = Convert.ToInt32(db.icb_cotizacion
                    .Where(d => d.cot_idserial == vpedido.idcotizacion).Select(d => d.cot_numcotizacion)
                    .FirstOrDefault()),
                nit = vpedido.nit,
                numeroIdentificacion = Convert.ToString(db.icb_terceros.Where(d => d.tercero_id == vpedido.nit)
                    .Select(d => d.doc_tercero).FirstOrDefault()),
                nit_asegurado = vpedido.nit_asegurado,
                nit2 = vpedido.nit2,
                nit3 = vpedido.nit3,
                nit4 = vpedido.nit4,
                nit5 = vpedido.nit5,
                vendedor = vpedido.vendedor,
                modelo = vpedido.modelo,
                id_anio_modelo = vpedido.id_anio_modelo,
                plan_venta = vpedido.plan_venta,
                planmayor = vpedido.planmayor,
                fecha_asignacion_planmayor = vpedido.fecha_asignacion_planmayor,
                asignado_por = vpedido.asignado_por,
                condicion = vpedido.condicion,
                dias_validez = vpedido.dias_validez,
                valor_unitario = vpedido.valor_unitario != null
                    ? vpedido.valor_unitario.Value.ToString("N2", new CultureInfo("is-IS"))
                    : "0",
                porcentaje_iva = vpedido.porcentaje_iva,
                valorPoliza = vpedido.valorPoliza != null
                    ? vpedido.valorPoliza.Value.ToString("N2", new CultureInfo("is-IS"))
                    : "0",
                pordscto = vpedido.pordscto,
                vrdescuento = vpedido.vrdescuento != null
                    ? vpedido.vrdescuento.Value.ToString("N2", new CultureInfo("is-IS"))
                    : "0",
                cantidad = vpedido.cantidad,
                tipo_carroceria = vpedido.tipo_carroceria,
                vrcarroceria = vpedido.vrcarroceria != null
                    ? vpedido.vrcarroceria.Value.ToString("N2", new CultureInfo("is-IS"))
                    : "0",
                vrtotal = vpedido.vrtotal != null
                    ? vpedido.vrtotal.Value.ToString("N2", new CultureInfo("is-IS"))
                    : "0",
                moneda = vpedido.moneda,
                id_aseguradora = vpedido.id_aseguradora,
                notas1 = vpedido.notas1,
                notas2 = vpedido.notas2,
                escanje = vpedido.escanje,
                eschevyplan = vpedido.eschevyplan,
                esLeasing = vpedido.esLeasing,
                esreposicion = vpedido.esreposicion,
                nit_prenda = vpedido.nit_prenda,
                flota = vpedido.flota,
                codflota = vpedido.codigoflota,
                facturado = vpedido.facturado,
                numfactura = vpedido.numfactura,
                porcentaje_impoconsumo = vpedido.porcentaje_impoconsumo,
                numeroplaca = vpedido.numeroplaca,
                motivo_anulacion = vpedido.motivo_anulacion,
                venta_gerencia = vpedido.venta_gerencia,
                Color_Deseado = vpedido.Color_Deseado,
                terminacionplaca = vpedido.terminacionplaca,
                bono = vpedido.bono,
                idmodelo = vpedido.idmodelo,
                nuevo = vpedido.nuevo,
                usado = vpedido.usado,
                servicio = vpedido.servicio,
                placapar = vpedido.placapar,
                placaimpar = vpedido.placaimpar,
                color_opcional = vpedido.color_opcional,
                cargomatricula = vpedido.cargomatricula,
                obsequioporcen = vpedido.obsequioporcen,
                valormatricula = vpedido.valormatricula != null
                    ? vpedido.valormatricula.Value.ToString("N2", new CultureInfo("is-IS"))
                    : "0",
                rango_placa = vpedido.rango_placa,
                marcvh_id = Convert.ToString(vpedido.marca),
                fec_creacion = vpedido.fec_creacion,
                userid_creacion = vpedido.userid_creacion,
                iddepartamento = vpedido.iddepartamento,
                idciudad = vpedido.idciudad,
                id = vpedido.id,
                valorsoat = vpedido.valorsoat != null
                    ? vpedido.valorsoat.Value.ToString("N2", new CultureInfo("is-IS"))
                    : "0",
                otrosValores = vpedido.otrosValores != null
                    ? vpedido.otrosValores.Value.ToString("N2", new CultureInfo("is-IS"))
                    : "0"
            };

            #endregion

            listas(pedido);
            BuscarFavoritos(menu);
            return View(pedido);
        }

        public JsonResult calcularValores(string valor_poliza, string soat, string valor_carroceria,
            string valor_unitario, string por_descuento, string porcentaje_iva, string por_imp_consumo)
        {
            if (string.IsNullOrWhiteSpace(por_descuento))
            {
                por_descuento = "0";
            }

            decimal valoru = Convert.ToDecimal(valor_unitario, new CultureInfo("is-IS"));
            decimal poliza = Convert.ToDecimal(valor_poliza, new CultureInfo("is-IS"));
            decimal valor_soat = Convert.ToDecimal(soat, new CultureInfo("is-IS"));
            decimal carroceria = Convert.ToDecimal(valor_carroceria, new CultureInfo("is-IS"));
            decimal valor_descuento = Convert.ToDecimal(valor_unitario, new CultureInfo("is-IS")) * Convert.ToDecimal(por_descuento, new CultureInfo("is-IS")) / 100;
            decimal valor_unitario_desc = Convert.ToDecimal(valor_unitario, new CultureInfo("is-IS")) - valor_descuento;
            decimal valor_iva = valor_unitario_desc * Convert.ToDecimal(porcentaje_iva, new CultureInfo("is-IS")) / 100;
            decimal valor_impuesto = valor_unitario_desc * Convert.ToDecimal(por_imp_consumo, new CultureInfo("is-IS")) / 100;
            decimal valor_total = valor_unitario_desc + valor_iva + valor_impuesto + poliza + valor_soat + carroceria;

            string[] valores =
            {
                valoru.ToString("N2", new CultureInfo("is-IS")),
                poliza.ToString("N2", new CultureInfo("is-IS")),
                valor_soat.ToString("N2", new CultureInfo("is-IS")),
                carroceria.ToString("N2", new CultureInfo("is-IS")),
                valor_descuento.ToString("N2", new CultureInfo("is-IS")),
                valor_iva.ToString("N2", new CultureInfo("is-IS")),
                valor_impuesto.ToString("N2", new CultureInfo("is-IS")),
                valor_total.ToString("N2", new CultureInfo("is-IS"))
            };

            return Json(valores);
        }

        public JsonResult validarFacAccesorios(string planmayor)
        {
            vpedido pedido = db.vpedido.Where(x => x.planmayor == planmayor).FirstOrDefault();
            if (pedido != null)
            {
                List<vpedrepuestos> pedrepuestos = db.vpedrepuestos.Where(x => x.pedido_id == pedido.id).ToList();
                if (pedrepuestos != null)
                {
                    for (int i = 0; i < pedrepuestos.Count; i++)
                    {
                        if (pedrepuestos[i].facturado == false)
                        {
                            return Json(new { resultado = 0, mensaje = "Los accesorios no se han facturado." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    return Json(new { resultado = 1, mensaje = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { resultado = 0, mensaje = "No hay accesorios para realizar el agendamiento." }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { resultado = 0, mensaje = "El plan mayor no tiene un pedido asociado." }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult modalesBackOffice()
        {
            ViewBag.tp_doc_registros = db.tp_doc_registros.Where(x => x.tpdoc_estado && x.sw == 3 && x.tipo == 2)
                .OrderBy(x => x.tpdoc_nombre).ToList();
            ViewBag.tp_doc_registrosFR = db.tp_doc_registros.Where(x => x.tpdoc_estado && x.sw == 3 && x.tipo == 4)
                .OrderBy(x => x.tpdoc_nombre).ToList();
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
                    planmayor = data.planmayor,
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
            icb_vehiculo buscarVehiculo = db.icb_vehiculo.FirstOrDefault(x => x.asignado == idPedido);
            if (buscarVehiculo != null)
            {
                buscarVehiculo.asignado = null;
                db.Entry(buscarVehiculo).State = EntityState.Modified;
            }

            int result = db.SaveChanges();
            if (result > 0)
            {
                return Json(new { exito = true }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { exito = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Edit(VehiculoPedidoModel vpedido, int? menu)
        {
            ViewBag.rol = Convert.ToInt32(Session["user_rolid"]);
            string valorIva =
                (Convert.ToDecimal(vpedido.valor_unitario, new CultureInfo("is-IS")) * Convert.ToDecimal(vpedido.porcentaje_iva, new CultureInfo("is-IS")) / 100).ToString(
                    "N2", new CultureInfo("is-IS"));
            string valorImpConsumo =
                (Convert.ToDecimal(vpedido.valor_unitario, new CultureInfo("is-IS")) * Convert.ToDecimal(vpedido.porcentaje_impoconsumo, new CultureInfo("is-IS")) / 100)
                .ToString("N2", new CultureInfo("is-IS"));
            ViewBag.valorIva = valorIva;
            ViewBag.valorImpConsumo = valorImpConsumo;
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
                            icb_vehiculo vehiculo = db.icb_vehiculo.Find(vpedido.planmayor);
                            vehiculo.asignado = vpedido.id;

                            db.Entry(vehiculo).State = EntityState.Modified;
                            vpedido.fecha_asignacion_planmayor = DateTime.Now;
                        }

                        //creas objeto pedido de la clase vpedido

                        #region variables pedido

                        vpedido pedidos = new vpedido
                        {
                            id = vpedido.id,
                            numero = vpedido.numero,
                            impfactura2 = vpedido.impfactura2,
                            impfactura3 = vpedido.impfactura3,
                            impfactura4 = vpedido.impfactura4,
                            bodega = vpedido.bodega,
                            anulado = vpedido.anulado,
                            fecha = Convert.ToDateTime(vpedido.fecha),
                            idcotizacion = vpedido.idcotizacion,
                            nit = vpedido.nit,
                            nit_asegurado = vpedido.nit_asegurado,
                            nit2 = vpedido.nit2,
                            nit3 = vpedido.nit3,
                            nit4 = vpedido.nit4,
                            nit5 = vpedido.nit5,
                            vendedor = vpedido.vendedor,
                            marca = Convert.ToInt32(vpedido.marcvh_id),
                            modelo = vpedido.modelo,
                            id_anio_modelo = vpedido.id_anio_modelo,
                            plan_venta = vpedido.plan_venta,
                            planmayor = vpedido.planmayor
                        };
                        if (vpedido.planmayor != null)
                        {
                            pedidos.no_disponible = false;
                        }

                        pedidos.fecha_asignacion_planmayor = vpedido.fecha_asignacion_planmayor;
                        pedidos.asignado_por = vpedido.asignado_por;
                        pedidos.condicion = vpedido.condicion;
                        pedidos.dias_validez = vpedido.dias_validez;
                        pedidos.valor_unitario = Convert.ToDecimal(vpedido.valor_unitario, new CultureInfo("is-IS"));
                        pedidos.porcentaje_iva = vpedido.porcentaje_iva;
                        pedidos.valorPoliza = Convert.ToDecimal(vpedido.valorPoliza, new CultureInfo("is-IS"));
                        pedidos.pordscto = vpedido.pordscto;
                        pedidos.vrdescuento = Convert.ToDecimal(vpedido.vrdescuento, new CultureInfo("is-IS"));
                        pedidos.cantidad = vpedido.cantidad;
                        pedidos.tipo_carroceria = vpedido.tipo_carroceria;
                        pedidos.vrcarroceria = Convert.ToDecimal(vpedido.vrcarroceria, new CultureInfo("is-IS"));
                        pedidos.vrtotal = Convert.ToDecimal(vpedido.vrtotal, new CultureInfo("is-IS"));
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
                        pedidos.valormatricula = Convert.ToDecimal(vpedido.valormatricula, new CultureInfo("is-IS"));
                        pedidos.rango_placa = vpedido.rango_placa;
                        pedidos.user_idactualizacion = vpedido.user_idactualizacion;
                        pedidos.fec_actualizacion = vpedido.fec_actualizacion;
                        pedidos.userid_creacion = vpedido.userid_creacion;
                        pedidos.fec_creacion = vpedido.fec_creacion;
                        pedidos.iddepartamento = vpedido.iddepartamento;
                        pedidos.idciudad = vpedido.idciudad;
                        pedidos.valorsoat = Convert.ToDecimal(vpedido.valorsoat, new CultureInfo("is-IS"));
                        pedidos.otrosValores = Convert.ToDecimal(vpedido.otrosValores, new CultureInfo("is-IS"));

                        #endregion

                        db.Entry(pedidos).State = EntityState.Modified;

                        int pedido_id = pedidos.id;

                        #region pagos

                        string pagos = Request["lista_pagos"];
                        if (!string.IsNullOrEmpty(pagos))
                        {
                            int lista_pagos = Convert.ToInt32(Request["lista_pagos"]);
                            for (int i = 1; i <= lista_pagos; i++)
                            {
                                if (!string.IsNullOrEmpty(Request["condicion" + i]))
                                {
                                    vpedpago vpago = new vpedpago
                                    {
                                        idpedido = pedido_id,
                                        seq = i,
                                        condicion = Convert.ToInt32(Request["condicion" + i]),
                                        valor = Convert.ToDecimal(Request["valor" + i], new CultureInfo("is-IS")),
                                        fecpago = Convert.ToDateTime(Request["fecpago" + i]),
                                        observaciones = Request["observaciones" + i]
                                    };
                                    string banco = Request["banco" + i];
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

                        string lrp = Request["lista_repuestos"];
                        if (!string.IsNullOrEmpty(lrp))
                        {
                            int lista_repuestos = Convert.ToInt32(Request["lista_repuestos"]);
                            for (int j = 1; j <= lista_repuestos; j++)
                            {
                                if (!string.IsNullOrEmpty(Request["repuestos" + j]))
                                {
                                    vpedrepuestos vrepuesto = new vpedrepuestos
                                    {
                                        pedido_id = pedido_id,
                                        referencia = Request["repuestos" + j],
                                        vrunitario = Convert.ToDecimal(Request["costo" + j], new CultureInfo("is-IS")),
                                        vrtotal = Convert.ToDecimal(Request["totalRespuesto" + j], new CultureInfo("is-IS")),
                                        obsequio = Convert.ToBoolean(Request["obsequio" + j]),
                                        cantidad = Convert.ToInt32(Request["cantidadRespuesto" + j])
                                    };
                                    db.vpedrepuestos.Add(vrepuesto);
                                }
                            }
                        }

                        #endregion

                        #region retomas

                        string lr = Request["lista_retomas"];
                        if (!string.IsNullOrEmpty(lr))
                        {
                            int lista_retomas = Convert.ToInt32(Request["lista_retomas"]);
                            for (int i = 1; i <= lista_retomas; i++)
                            {
                                if (!string.IsNullOrEmpty(Request["valor_retoma" + i]))
                                {
                                    vpedretoma retoma = new vpedretoma
                                    {
                                        pedido_id = pedido_id,
                                        placa = Request["placa_retoma" + i],
                                        valor = Convert.ToDecimal(Request["valor_retoma" + i], new CultureInfo("is-IS")),
                                        modelo = Request["modelo_retoma" + i]
                                    };
                                    if (!string.IsNullOrEmpty(Request["kl_retoma" + i]))
                                    {
                                        retoma.kilometraje = Convert.ToDecimal(Request["kl_retoma" + i], new CultureInfo("is-IS"));
                                    }

                                    if (!string.IsNullOrEmpty(Request["obligacion_retoma" + i]))
                                    {
                                        retoma.obligaciones = Convert.ToBoolean(Request["obligacion_retoma" + i]);
                                        if (!string.IsNullOrEmpty(Request["valor_obligacion" + i]))
                                        {
                                            retoma.valor_obligacion =
                                                Convert.ToDecimal(Request["valor_obligacion" + i], new CultureInfo("is-IS"));
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

                        string costo = Request["lista_costos"];
                        if (!string.IsNullOrEmpty(costo))
                        {
                            int lista_costos = Convert.ToInt32(Request["lista_costos"]);
                            for (int j = 1; j <= lista_costos; j++)
                            {
                                if (!string.IsNullOrEmpty(Request["descripcion_costo" + j]))
                                {
                                    vpedcostos_adicionales costos = new vpedcostos_adicionales
                                    {
                                        pedido_id = pedido_id,
                                        descripcion = Request["descripcion_costo" + j]
                                    };
                                    if (!string.IsNullOrEmpty(Request["valor_costo" + j]))
                                    {
                                        costos.valor = Convert.ToDecimal(Request["valor_costo" + j], new CultureInfo("is-IS"));
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

                        int cambio = Convert.ToInt32(Request["esCambio"]);
                        if (cambio == 1)
                        {
                            IQueryable<v_creditos> creditoExiste = db.v_creditos.Where(x => x.pedido == pedido_id);
                            if (creditoExiste != null)
                            {
                                foreach (v_creditos item in creditoExiste)
                                {
                                    item.vehiculo = vpedido.modelosMarca;
                                    db.Entry(item).State = EntityState.Modified;
                                }
                            }

                            pedidos.marca = Convert.ToInt32(vpedido.marcas);
                            pedidos.modelo = vpedido.modelosMarca;
                            pedidos.id_anio_modelo = vpedido.idAnioModelo;
                            db.Entry(pedidos).State = EntityState.Modified;

                            vcambiovehiculo cambioVH = new vcambiovehiculo
                            {
                                idpedido = vpedido.id,
                                idmarca = Convert.ToInt32(vpedido.marcvh_id),
                                idanomodelo = Convert.ToInt32(vpedido.id_anio_modelo),
                                motivo = vpedido.motivoCambio,
                                iduser = Convert.ToInt32(Session["user_usuarioid"]),
                                feccreacion = DateTime.Now
                            };

                            db.vcambiovehiculo.Add(cambioVH);
                            //db.SaveChanges();
                        }

                        #endregion

                        int result = db.SaveChanges();
                        if (result > 0)
                        {
                            TempData["mensaje"] = "Pedido editado correctamente";
                            dbTran.Commit();
                            return RedirectToAction("Edit", new { id = vpedido.id, menu = menu });
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error al editar el pedido, por favor intente nuevamente";
                            dbTran.Rollback();
                        }
                    }
                    catch (DbEntityValidationException)
                    {
                        dbTran.Rollback();
                        throw;
                    }
                }
            }

            #region variables pedido

            VehiculoPedidoModel pedido = new VehiculoPedidoModel
            {
                id = vpedido.id,
                numero = vpedido.numero,
                impfactura2 = vpedido.impfactura2,
                impfactura3 = vpedido.impfactura3,
                impfactura4 = vpedido.impfactura4,
                bodega = vpedido.bodega,
                anulado = vpedido.anulado,
                fecha = Convert.ToDateTime(vpedido.fecha),
                idcotizacion = vpedido.idcotizacion,
                nit = vpedido.nit,
                nit_asegurado = vpedido.nit_asegurado,
                nit2 = vpedido.nit2,
                nit3 = vpedido.nit3,
                nit4 = vpedido.nit4,
                nit5 = vpedido.nit5,
                vendedor = vpedido.vendedor,
                modelo = vpedido.modelo,
                id_anio_modelo = vpedido.id_anio_modelo,
                plan_venta = vpedido.plan_venta,
                planmayor = vpedido.planmayor,
                fecha_asignacion_planmayor = vpedido.fecha_asignacion_planmayor,
                asignado_por = vpedido.asignado_por,
                condicion = vpedido.condicion,
                dias_validez = vpedido.dias_validez,
                valor_unitario = !string.IsNullOrWhiteSpace(vpedido.valor_unitario)
                    ? Convert.ToDecimal(vpedido.valor_unitario).ToString("N2", new CultureInfo("is-IS"))
                    : "0",
                porcentaje_iva = vpedido.porcentaje_iva,
                valorPoliza = !string.IsNullOrWhiteSpace(vpedido.valorPoliza)
                    ? Convert.ToDecimal(vpedido.valorPoliza).ToString("N2", new CultureInfo("is-IS"))
                    : "0",
                pordscto = vpedido.pordscto,
                vrdescuento = !string.IsNullOrWhiteSpace(vpedido.vrdescuento)
                    ? Convert.ToDecimal(vpedido.vrdescuento).ToString("N2", new CultureInfo("is-IS"))
                    : "0",
                cantidad = vpedido.cantidad,
                tipo_carroceria = vpedido.tipo_carroceria,
                vrcarroceria = !string.IsNullOrWhiteSpace(vpedido.vrcarroceria)
                    ? Convert.ToDecimal(vpedido.vrcarroceria).ToString("N2", new CultureInfo("is-IS"))
                    : "0",
                vrtotal = !string.IsNullOrWhiteSpace(vpedido.vrtotal)
                    ? Convert.ToDecimal(vpedido.vrtotal).ToString("N2", new CultureInfo("is-IS"))
                    : "0",
                moneda = vpedido.moneda,
                id_aseguradora = vpedido.id_aseguradora,
                notas1 = vpedido.notas1,
                notas2 = vpedido.notas2,
                escanje = vpedido.escanje,
                eschevyplan = vpedido.eschevyplan,
                esLeasing = vpedido.esLeasing,
                esreposicion = vpedido.esreposicion,
                nit_prenda = vpedido.nit_prenda,
                flota = vpedido.flota,
                codflota = vpedido.codflota,
                facturado = vpedido.facturado,
                numfactura = vpedido.numfactura,
                porcentaje_impoconsumo = vpedido.porcentaje_impoconsumo,
                numeroplaca = vpedido.numeroplaca,
                motivo_anulacion = vpedido.motivo_anulacion,
                venta_gerencia = vpedido.venta_gerencia,
                Color_Deseado = vpedido.Color_Deseado,
                terminacionplaca = vpedido.terminacionplaca,
                bono = vpedido.bono,
                marcvh_id = Convert.ToString(vpedido.marcvh_id),
                idmodelo = vpedido.idmodelo,
                nuevo = vpedido.nuevo,
                usado = vpedido.usado,
                servicio = vpedido.servicio,
                placapar = vpedido.placapar,
                placaimpar = vpedido.placaimpar,
                color_opcional = vpedido.color_opcional,
                cargomatricula = vpedido.cargomatricula,
                obsequioporcen = vpedido.obsequioporcen,
                valormatricula = Convert.ToString(vpedido.valormatricula),
                rango_placa = vpedido.rango_placa,
                user_idactualizacion = vpedido.user_idactualizacion,
                fec_actualizacion = vpedido.fec_actualizacion,
                userid_creacion = vpedido.userid_creacion,
                fec_creacion = vpedido.fec_creacion
            };
            int cambio2 = Convert.ToInt32(Request["esCambio"]);
            if (cambio2 == 1)
            {
                pedido.modelo = vpedido.modelosMarca;
                pedido.id_anio_modelo = vpedido.idAnioModelo;
            }
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
                List<anio_modelo> anios = db.anio_modelo.Where(x => x.codigo_modelo == codigoModelo).ToList();
                var result = anios.Select(x => new
                {
                    text = x.anio,
                    value = x.anio_modelo_id
                });
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                DateTime fechaActual = DateTime.Now;
                var anios = db.vw_referencias_total.Where(x =>
                    x.modvh_id == codigoModelo && x.ano == fechaActual.Year && x.mes == fechaActual.Month).Select(x =>
                    new
                    {
                        anios = x.anio_vh,
                        x.codigo
                    }).Distinct();
                return Json(anios, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult BuscarPrecioXAniosModelos(int anioModelo)
        {
            anio_modelo precio = db.anio_modelo.FirstOrDefault(x => x.anio_modelo_id == anioModelo);
            vlistanuevos precioVh = db.vlistanuevos.Where(x => x.anomodelo == anioModelo).OrderByDescending(x => x.ano)
                .ThenByDescending(x => x.mes).FirstOrDefault();
            string data = precioVh != null ? precioVh.precioespecial.ToString("N2", new CultureInfo("is-IS")) : "0,00";
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Seguimiento(int id, int? menu)
        {
            infoPedido(id);

            int pedido_id = db.vpedido.Find(id).id;
            vpedseguimiento seguimiento = new vpedseguimiento
            {
                pedidoid = pedido_id
            };

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
                                  tercero = t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero + " " +
                                            t.segapellido_tercero,
                                  t.telf_tercero,
                                  t.email_tercero,
                                  t.celular_tercero
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
            int result = db.SaveChanges();
            if (result > 0)
            {
                TempData["mensaje"] = "Nota agregada correctamente";
                return RedirectToAction("Seguimiento", new { id = seguimiento.pedidoid, menu });
            }

            TempData["mensaje_error"] = "Error al agregar la nota, por favor intente nuevamente";


            ViewBag.tipo = new SelectList(db.vtiposeguimientocot, "id_tipo_seguimiento", "nombre_seguimiento");
            BuscarFavoritos(menu);
            return View(seguimiento);
        }

        public ActionResult Delete(int id, int? menu, int motivo, string observacion)
        {
            vpedido vpedidos = db.vpedido.Find(id);
            vpedidos.anulado = true;
            vpedidos.idanulacion = motivo;
            vpedidos.motivo_anulacion = observacion;
            db.Entry(vpedidos).State = EntityState.Modified;

            db.SaveChanges();

            TempData["mensaje"] = "Pedido anulado Correctamente";
            return RedirectToAction("Edit", new { id, menu });
        }

        public JsonResult BuscarSeguimientos(int id)
        {
            var data = from seguimiento in db.vpedseguimiento
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
                       };

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
                          c.asesor
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
                          alias = rf.alias,
                          r.cantidad
                      };

            var ret = (from r in db.vcotretoma
                       where r.idcot == cotizacion
                       select new
                       {
                           valor_retoma = r.valor,
                           placa_retoma = r.placa,
                           modelo_retoma = r.modelo,
                           kl_retoma = r.Kilometraje
                       }).ToList();

            var data = new
            {
                cot,
                vh,
                rpt,
                ret
            };
            ViewBag.parametro = DateTime.Now.ToString("HH:mm:ss", new CultureInfo("en-US"));
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
                        where i.tercero == idTercero && c.estadoc == "A"
                        select new
                        {
                            c.Id,
                            c.financiera_id,
                            c.fec_solicitud,
                            tercero = t.prinom_tercero != null
                                ? "(" + t.doc_tercero + ") " + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                  t.apellido_tercero + " " + t.segnom_tercero
                                : "(" + t.doc_tercero + ") " + t.razon_social,
                            e.codigo,
                            e.descripcion,
                            c.vsolicitado,
                            c.vaprobado
                        }).ToList();

            var data = info.Select(x => new
            {
                x.Id,
                x.financiera_id,
                x.tercero,
                x.codigo,
                fecha = x.fec_solicitud.Value.ToString("yyyy/MM/dd"),
                x.descripcion,
                vsolicitado = string.IsNullOrEmpty(x.vsolicitado.ToString())
                    ? "0"
                    : x.vsolicitado.Value.ToString("0,0", elGR),
                vaprobado = string.IsNullOrEmpty(x.vaprobado.ToString()) ? "0" : x.vaprobado.Value.ToString("0,0", elGR)
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
                                     cedula = t.prinom_tercero != null
                                         ? t.doc_tercero + " - " + t.prinom_tercero + " " + t.apellido_tercero + " " +
                                           t.segapellido_tercero
                                         : t.doc_tercero + " - " + t.razon_social,
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
                               nombre = t.prinom_tercero != null
                                   ? t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero + " " +
                                     t.segapellido_tercero
                                   : t.razon_social,
                               telefono = t.telf_tercero != null ? t.telf_tercero : "",
                               celular = t.celular_tercero != null ? t.celular_tercero : "",
                               correo = t.email_tercero != null ? t.email_tercero : "",
                               ciudad = ci.ciu_nombre,
                               db.terceros_direcciones.OrderByDescending(x => x.id)
                                   .FirstOrDefault(x => x.idtercero == t.tercero_id).direccion
                           };

                return Json(new { info = data, cliente = true }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { info = "", cliente = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarClienteCedula(string cliente)
        {
            int idTercero = db.icb_terceros.FirstOrDefault(x => x.doc_tercero == cliente).tercero_id;

            var info = from t in db.icb_terceros
                       join c in db.tercero_cliente
                           on t.tercero_id equals c.tercero_id
                       where t.doc_tercero == cliente
                       select new
                       {
                           idTercero = t.tercero_id,
                           tipo = t.tpdoc_id,
                           nombre = t.prinom_tercero != null
                               ? t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero + " " +
                                 t.segapellido_tercero
                               : t.razon_social,
                           telefono = t.telf_tercero != null ? t.telf_tercero : "",
                           celular = t.celular_tercero != null ? t.celular_tercero : "",
                           correo = t.email_tercero != null ? t.email_tercero : "",
                           db.terceros_direcciones.OrderByDescending(x => x.id)
                               .FirstOrDefault(x => x.idtercero == t.tercero_id).direccion,
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
                tipoPer,
                idTercero
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
                           porcentaje_iva = vh.porcentaje_iva,
                           impuesto_consumo =vh.impuesto_consumo,
                           mar.marcvh_id,
                           soat = v.soat != null ? v.soat : 0
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPagos()
        {
            string parametro = db.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P62").syspar_value;

            var pagos = from p in db.vformapago
                        where p.estado
                        orderby p.descripcion
                        select new
                        {
                            p.id,
                            p.descripcion
                        };

            var bancos = from b in db.icb_unidad_financiera
                         where b.financiera_estado
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

        /// <summary>
        /// Se modifica la busqueda de datos de la tabla paginada que se utiliza en registro pedido
        /// se puede ver que se realizarón modificaciones en el LINQ de tal forma que no aparezca quemado
        /// el número de parametro, a su vez se ha utilizado la variable permcod que hace referencia a permiso de código
        /// para traer el parametro P04, que a su vez, hace referencia al rolpermiso ID= 4 con descripción modificar
        /// valor del permiso.
        /// </summary>
        /// <returns></returns>
        public JsonResult BuscarDatos()
        {
            int user = Convert.ToInt32(Session["user_usuarioid"]);

            int rolUsurario = (from u in db.users
                               join r in db.rolacceso
                                   on u.rol_id equals r.idrol
                               join permiso in db.rolpermisos
                               on r.idpermiso equals permiso.id
                               where u.user_id == user
                               select
                                   r.idrol
                ).FirstOrDefault();

            rolpermisos permcod = db.rolpermisos.FirstOrDefault(x => x.codigo == "P04");
            int codac = Convert.ToInt32(permcod.id);

            if (rolUsurario == codac)
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
                               doc_tercero = "(" + t.doc_tercero + ") " + t.prinom_tercero + " " + t.apellido_tercero + " " +
                                             t.razon_social,
                               fecha = p.fecha.ToString(),
                               facturado = p.facturado ? "Facturado" : "Sin facturar",
                               anulado = p.anulado ? "Anulado" : "No",
                               numfactura = p.numfactura != null ? p.numfactura.ToString() : " ",
                               planmayor = p.planmayor != null ? p.planmayor : "No asignado",
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
                               doc_tercero = "(" + t.doc_tercero + ") " + t.prinom_tercero + " " + t.apellido_tercero + " " +
                                             t.razon_social,
                               fecha = p.fecha.ToString(),
                               facturado = p.facturado ? "Facturado" : "Sin facturar",
                               anulado = p.anulado ? "Anulado" : "No",
                               numfactura = p.numfactura != null ? p.numfactura.ToString() : " ",
                               planmayor = p.planmayor != null ? p.planmayor : "No asignado",
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
                             p.id
                         }).ToList();

            var rpt = (from r in db.vpedrepuestos
                       join a in db.icb_referencia
                           on r.referencia equals a.ref_codigo
                       where r.pedido_id == pedido
                       select new
                       {
                           codigo = a.ref_codigo,
                           referencia = a.ref_descripcion,
                           alias = a.alias ?? "",
                           valor = r.vrtotal,
                           r.cantidad,
                           obsequio = r.obsequio == true ? "Si" : "No",
                           r.id
                       }).ToList();

            var rt = (from r in db.vpedretoma
                      where r.pedido_id == pedido
                      select new
                      {
                          r.modelo,
                          r.placa,
                          r.kilometraje,
                          r.valor,
                          // = r.obligaciones != null ? r.obligaciones == true ? "Si" : "No" : "No",
                          obligaciones = r.obligaciones == true ? "Si" : "No",
                          valor_obligacion = r.valor_obligacion != null ? r.valor_obligacion : 0,
                          r.id
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

        public JsonResult BuscarVhDisponible(string modelo_id, string usado, int? anio)  ///------
        {

            if (!string.IsNullOrWhiteSpace(modelo_id) && !string.IsNullOrWhiteSpace(usado) && anio != null)
            {

                string par = "P34";
                string valorParametro = db.icb_sysparameter.Where(x => x.syspar_cod == par).Select(x => x.syspar_value)
                    .FirstOrDefault();

                icb_sysparameter codigoeventorecepcion1 = db.icb_sysparameter.Where(x => x.syspar_cod == "P8").FirstOrDefault();
                int codigoeventorecepcion = codigoeventorecepcion1 != null ? Convert.ToInt32(codigoeventorecepcion1.syspar_value) : 5;
                int eventorecepcion = db.icb_tpeventos.Where(d => d.codigoevento == codigoeventorecepcion).Select(d => d.tpevento_id).FirstOrDefault();

                int valorPar = Convert.ToInt32(valorParametro);

                int usuario = Convert.ToInt32(Session["user_usuarioid"]);
                int anioFinal = db.anio_modelo.FirstOrDefault(x => x.anio_modelo_id == anio).anio;
                //Validamos que el usuario loguado tenga el rol y el permiso para ver toda la informacion
                int permiso = (from u in db.users
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
                                        on new { pm = v.plan_mayor, sv = valorPar } equals new
                                        { pm = ev.planmayor, sv = ev.id_tpevento } into je
                                    from ev in je.DefaultIfEmpty()
                                    join ub in db.ubicacion_bodega
                                        on v.ubicacionactual equals ub.id into xx
                                    from ub in xx.DefaultIfEmpty()
                                    join vped in db.vpedido
                                        on v.plan_mayor equals vped.planmayor into vpedi
                                    from vped in vpedi.DefaultIfEmpty()
                                    where v.modvh_id == modelo_id
                                          && vh.stock > 0
                                          && v.usado == true
                                          && v.anio_vh == anioFinal
                                          && (vped == null || vped != null && vped.anulado)
                                    //&& ev.id_tpevento == 15
                                    select new
                                    {
                                        v.vin,
                                        s.syspar_value,
                                        v.plan_mayor,
                                        color = v.color_vehiculo.colvh_nombre,
                                        bodega_id = vh.bodega,
                                        bodega = b.bodccs_nombre,
                                        modelo = m.modvh_nombre,
                                        anio_modelo = v.anio_vh,
                                        fecha_compra = v.icbvhfec_creacion.ToString(),
                                        fechacompra = v.icbvhfec_creacion,

                                        //averias = ev.id_tpevento == 15 ? ev.evento_observacion : "",
                                        estado = db.icb_vehiculo_eventos.Where(x => x.id_tpevento == eventorecepcion && x.planmayor == v.plan_mayor).FirstOrDefault() != null
                                            ? "Recepcionado"
                                            : "En tránsito",
                                        evento = db.icb_vehiculo_eventos.Where(x => x.id_tpevento == eventorecepcion && x.planmayor == v.plan_mayor).FirstOrDefault() != null
                                            ? "2"
                                            : "1",
                                        ev.fechaevento,
                                        ub.descripcion,
                                        averias = ev.evento_observacion != null ? ev.evento_observacion : "",
                                        cantidadAverias = db.icb_inspeccionvehiculos
                                            .Where(x => x.planmayor == v.plan_mayor).Count()
                                    }).OrderByDescending(x => x.fecha_compra).ToList();

                        var data2 = data.Select(x => new
                        {
                            x.vin,
                            x.syspar_value,
                            x.plan_mayor,
                            x.color,
                            x.bodega_id,
                            x.bodega,
                            x.modelo,
                            x.anio_modelo,
                            x.fecha_compra,
                            x.estado,
                            //dias = x.evento == "2" ? (x.fechacompra - DateTime.Now).Days.ToString() : "En tránsito",
                            dias = (DateTime.Now - x.fechacompra).Days.ToString(),

                            ubicacion = x.descripcion != null ? x.descripcion : "",
                            cantidadAverias = x.cantidadAverias.ToString()
                        }).ToList();

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
                                        on new { pm = v.plan_mayor, sv = valorPar } equals new
                                        { pm = ev.planmayor, sv = ev.id_tpevento } into je
                                    from ev in je.DefaultIfEmpty()
                                    join ub in db.ubicacion_bodega
                                        on ev.ubicacion equals ub.id into xx
                                    from ub in xx.DefaultIfEmpty()
                                    join vped in db.vpedido
                                        on v.plan_mayor equals vped.planmayor into vpedi
                                    from vped in vpedi.DefaultIfEmpty()
                                    where v.modvh_id == modelo_id
                                          && vh.stock > 0
                                          && v.nuevo == true
                                          && v.anio_vh == anioFinal
                                          && (vped == null || vped != null && vped.anulado)
                                    //&& ev.id_tpevento == 15
                                    select new
                                    {
                                        v.vin,
                                        s.syspar_value,
                                        v.plan_mayor,
                                        color = v.color_vehiculo.colvh_nombre,
                                        bodega_id = vh.bodega,
                                        bodega = b.bodccs_nombre,
                                        modelo = m.modvh_nombre,
                                        anio_modelo = v.anio_vh,
                                        fecha_compra = v.icbvhfec_creacion.ToString(),
                                        fechacompra = v.icbvhfec_creacion,
                                        //averias = ev.id_tpevento == 15 ? ev.evento_observacion : ""
                                        estado = db.icb_vehiculo_eventos.Where(x => x.id_tpevento == eventorecepcion && x.planmayor == v.plan_mayor).FirstOrDefault() != null
                                            ? "Recepcionado"
                                            : "En tránsito",
                                        evento = db.icb_vehiculo_eventos.Where(x => x.id_tpevento == eventorecepcion && x.planmayor == v.plan_mayor).FirstOrDefault() != null
                                            ? "2"
                                            : "1",
                                        fechaevento = db.icb_vehiculo_eventos.Where(x => x.id_tpevento == eventorecepcion && x.planmayor == v.plan_mayor)
                                            .Select(x => x.fechaevento).FirstOrDefault(),
                                        ub.descripcion,
                                        averias = ev.evento_observacion != null ? ev.evento_observacion : "",
                                        cantidadAverias = db.icb_inspeccionvehiculos
                                            .Where(x => x.planmayor == v.plan_mayor).Count()
                                    }).OrderByDescending(x => x.fecha_compra).ToList();

                        var data2 = data.Select(x => new
                        {
                            x.vin,
                            x.syspar_value,
                            x.plan_mayor,
                            x.color,
                            x.bodega_id,
                            x.bodega,
                            x.modelo,
                            x.anio_modelo,
                            x.fecha_compra,
                            x.estado,
                            fechaevento = x.fechaevento != null ? x.fechaevento.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "Proceso de llegada a bodega",
                            //dias = x.evento == "2" ? (x.fechacompra - DateTime.Now).Days.ToString() : "En tránsito",
                            dias = (DateTime.Now - x.fechacompra).Days.ToString(),
                            ubicacion = x.descripcion != null ? x.descripcion : "",
                            x.averias,
                            cantidadAverias = x.cantidadAverias.ToString()
                        }).ToList();

                        return Json(data2, JsonRequestBehavior.AllowGet);
                    }
                }

                int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
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
                                    on new { pm = v.plan_mayor, sv = valorPar } equals new { pm = ev.planmayor, sv = ev.id_tpevento }
                                    into je
                                from ev in je.DefaultIfEmpty()
                                join ub in db.ubicacion_bodega
                                    on v.ubicacionactual equals ub.id into xx
                                from ub in xx.DefaultIfEmpty()
                                where v.modvh_id == modelo_id
                                      && vh.stock > 0
                                      && v.usado == true
                                      && vh.bodega == bodegaActual
                                      && v.anio_vh == anioFinal
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
                                    fechacompra = v.icbvhfec_creacion,
                                    estado = db.icb_vehiculo_eventos.Where(x => x.id_tpevento == eventorecepcion).FirstOrDefault() != null
                                        ? "Recepcionado"
                                        : "En tránsito",
                                    evento = db.icb_vehiculo_eventos.Where(x => x.id_tpevento == eventorecepcion).FirstOrDefault() != null
                                        ? "2"
                                        : "1",
                                    ev.fechaevento,
                                    ub.descripcion,
                                    averias = ev.evento_observacion != null ? ev.evento_observacion : ""
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
                        //dias = x.evento == "2" ? (x.fechacompra - DateTime.Now).Days.ToString() : "En tránsito",
                        dias = (DateTime.Now - x.fechacompra).Days.ToString(),
                        ubicacion = x.descripcion != null ? x.descripcion : "",
                        x.averias,
                        exhibicion = x.plan_mayor != null ? buscarEventoExh(x.plan_mayor) : 2
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
                                    on new { pm = v.plan_mayor, sv = valorPar } equals new { pm = ev.planmayor, sv = ev.id_tpevento }
                                    into je
                                from ev in je.DefaultIfEmpty()
                                join ub in db.ubicacion_bodega
                                    on v.ubicacionactual equals ub.id into xx
                                from ub in xx.DefaultIfEmpty()
                                where v.modvh_id == modelo_id
                                      && vh.stock > 0
                                      && v.nuevo == true
                                      && vh.bodega == bodegaActual
                                      && v.anio_vh == anioFinal
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
                                    fechacompra = v.icbvhfec_creacion,
                                    estado = db.icb_vehiculo_eventos.Where(x => x.id_tpevento == 2).FirstOrDefault() != null
                                        ? "Recepcionado"
                                        : "En tránsito",
                                    evento = db.icb_vehiculo_eventos.Where(x => x.id_tpevento == 2).FirstOrDefault() != null
                                        ? "2"
                                        : "1",
                                    ev.fechaevento,
                                    ub.descripcion,
                                    averias = ev.evento_observacion != null ? ev.evento_observacion : ""
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
                        //dias = x.evento == "2" ? (x.fechacompra - DateTime.Now).Days.ToString() : "En tránsito",
                        dias = (DateTime.Now - x.fechacompra).Days.ToString(),
                        ubicacion = x.descripcion != null ? x.descripcion : "",
                        x.averias,
                        exhibicion = x.plan_mayor != null ? buscarEventoExh(x.plan_mayor) : 2
                    });

                    return Json(data2, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                return Json(0, JsonRequestBehavior.AllowGet);//return 0
            }



        }
        public int buscarEventoExh(string planmayor)
        {
            int Exh = 0;
            icb_vehiculo_eventos buscarEvento = db.icb_vehiculo_eventos
                .Where(x => x.planmayor == planmayor && x.evento_estado && x.evento_nombre == "Envio a Exibicion")
                .FirstOrDefault();
            if (buscarEvento != null)
            {
                Exh = 1;
            }
            return Exh;
        }

        public JsonResult validarPlanMayor(string plan)
        {
            vpedido data = db.vpedido.FirstOrDefault(x => x.planmayor == plan);
            if (data == null)
            {
                return Json(new { permitir = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { permitir = false, data.numero }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MarcarNoDisponible(int? id)
        {
            int valor = 0;
            string respuesta = "Debe suministrar un Id de pedido válido";

            //valido si el id no es nulo
            if (id != null)
            {
                //verifico si existe
                vpedido vpedidox = db.vpedido.Where(d => d.id == id).FirstOrDefault();
                if (vpedidox != null)
                {
                    //veo si ya está marcado como no disponible y si aún no tiene plan mayr
                    if (string.IsNullOrWhiteSpace(vpedidox.planmayor))
                    {
                        //si ya fue marcado como no disponible
                        if (vpedidox.no_disponible == false)
                        {
                            vpedidox.no_disponible = true;
                            db.Entry(vpedidox).State = EntityState.Modified;
                            int guardar = db.SaveChanges();
                            if (guardar > 0)
                            {
                                respuesta = "El pedido ha sido marcado como no disponible exitosamente";
                                valor = 1;
                            }
                            else
                            {
                                respuesta = "Error en guardado";
                            }
                        }
                        else
                        {
                            respuesta = "El pedido ya fue marcado como no disponible";
                        }
                    }
                    else
                    {
                        respuesta = "El pedido ya tiene un plan mayor asignado";
                    }
                }
                else
                {
                    respuesta = "No existe un pedido con el id suministrado";
                }
            }

            var data = new
            {
                valor,
                respuesta
            };

            return Json(data);
        }

        public JsonResult MarcarFacturar(int? id)
        {
            int valor = 0;
            string respuesta = "Debe suministrar un Id de pedido válido";

            //valido si el id no es nulo
            if (id != null)
            {
                //verifico si existe
                vpedido vpedidox = db.vpedido.Where(d => d.id == id).FirstOrDefault();
                if (vpedidox != null)
                {
                    //veo si ya está marcado como no disponible y si aún no tiene plan mayr
                    if (!string.IsNullOrWhiteSpace(vpedidox.planmayor))
                    {
                        //si ya fue marcado como no disponible
                        if (vpedidox.para_facturar == false)
                        {
                            vpedidox.para_facturar = true;
                            vpedidox.aprobado_por = Convert.ToInt32(Session["user_usuarioid"]);
                            db.Entry(vpedidox).State = EntityState.Modified;
                            int guardar = db.SaveChanges();
                            if (guardar > 0)
                            {
                                respuesta = "El pedido ha sido marcado para su facturacion exitosamente";
                                valor = 1;
                            }
                            else
                            {
                                respuesta = "Error en guardado";
                            }
                        }
                        else
                        {
                            respuesta = "El pedido ya fue marcado para facturar";
                        }
                    }
                    else
                    {
                        respuesta = "El pedido no tiene un plan mayor asignado";
                    }
                }
                else
                {
                    respuesta = "No existe un pedido con el id suministrado";
                }
            }

            var data = new
            {
                valor,
                respuesta
            };

            return Json(data);
        }

        public JsonResult ValidarAverias(string plan_mayor)
        {
            //P34 parametro en sysparameter para averias
            string valor_parametro = db.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P34").syspar_value;
            int result = 0;

            if (valor_parametro != "0")
            {
                int valor = Convert.ToInt32(valor_parametro);
                icb_vehiculo_eventos averias =
                    db.icb_vehiculo_eventos.FirstOrDefault(x => x.id_tpevento == valor && x.planmayor == plan_mayor);

                if (averias != null)
                {
                    autorizaciones autorizacion = db.autorizaciones.FirstOrDefault(x =>
                        x.plan_mayor == plan_mayor && x.autorizado && x.tipo_autorizacion == 1);
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
                        result
                    };
                }
            }
            else
            {
                result = 0;
                var data = new
                {
                    result
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            return Json(JsonRequestBehavior.AllowGet);
        }

        public JsonResult EnviarNotificacionAverias(string plan_mayor)
        {
            int usuario_actual = Convert.ToInt32(Session["user_usuarioid"]);
            int bodega_actual = Convert.ToInt32(Session["user_bodega"]);
            icb_vehiculo vh = db.icb_vehiculo.Find(plan_mayor);
            int result = 0;
            configuracion_envio_correos correoconfig = db.configuracion_envio_correos.Where(d => d.activo).FirstOrDefault();

            usuarios_autorizaciones usuarios_autorizacion = db.usuarios_autorizaciones.FirstOrDefault(x => x.bodega_id == vh.id_bod);
            if (usuarios_autorizacion != null)
            {
                autorizaciones existe = db.autorizaciones.FirstOrDefault(x =>
                    x.plan_mayor == plan_mayor && x.user_autorizacion == usuarios_autorizacion.user_id &&
                    x.tipo_autorizacion == 1);

                if (existe == null)
                {
                    int usuario_autorizacion = usuarios_autorizacion.user_id;

                    autorizaciones autorizacion = new autorizaciones
                    {
                        plan_mayor = plan_mayor,
                        user_autorizacion = usuario_autorizacion,
                        user_creacion = usuario_actual,
                        fecha_creacion = DateTime.Now,
                        tipo_autorizacion = 1,
                        bodega = bodega_actual
                    };
                    db.autorizaciones.Add(autorizacion);
                    db.SaveChanges();
                    int autorizacion_id = db.autorizaciones.OrderByDescending(x => x.id)
                        .FirstOrDefault(x => x.tipo_autorizacion == 1).id;
                    result = 1;

                    try
                    {
                        notificaciones correo_enviado = db.notificaciones.FirstOrDefault(x =>
                            x.user_destinatario == usuario_autorizacion && x.enviado != true &&
                            x.autorizacion_id == autorizacion_id);
                        if (correo_enviado == null)
                        {
                            users user_destinatario = db.users.Find(usuario_autorizacion);
                            users user_remitente = db.users.Find(usuario_actual);

                            MailAddress de = new MailAddress(correoconfig.correo, "Notificación Iceberg");
                            MailAddress para = new MailAddress(user_destinatario.user_email,
                                user_destinatario.user_nombre + " " + user_destinatario.user_apellido);
                            MailMessage mensaje = new MailMessage(de, para);
                            mensaje.ReplyToList.Add(new MailAddress(user_remitente.user_email,
                                user_remitente.user_nombre + " " + user_remitente.user_apellido));
                            mensaje.Subject = "Solicitud Autorización plan mayor " + plan_mayor;
                            mensaje.BodyEncoding = Encoding.Default;
                            mensaje.IsBodyHtml = true;
                            string html = "";
                            html += "<h4>Cordial Saludo</h4><br>";
                            html += "<p>El usuario " + user_remitente.user_nombre + " " + user_remitente.user_apellido +
                                    " solicita autorización para la asignación del "
                                    + " vehículo con plan mayor " + plan_mayor + " por averia </p><br /><br />";
                            html += "Por favor ingrese a la plataforma para dar autorización.";
                            mensaje.Body = html;

                            SmtpClient cliente = new SmtpClient(correoconfig.smtp_server)
                            {
                                Port = correoconfig.puerto,
                                UseDefaultCredentials = false,
                                Credentials = new NetworkCredential(correoconfig.usuario, correoconfig.password),
                                EnableSsl = true
                            };
                            cliente.Send(mensaje);

                            notificaciones envio = new notificaciones
                            {
                                user_remitente = usuario_actual,
                                asunto = "Notificación solicitud autorización por averia",
                                fecha_envio = DateTime.Now,
                                enviado = true,
                                user_destinatario = usuario_autorizacion,
                                autorizacion_id = autorizacion_id
                            };
                            db.notificaciones.Add(envio);
                            db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        notificaciones envio = new notificaciones
                        {
                            user_remitente = usuario_actual,
                            asunto = "Notificación solicitud autorización por averia",
                            fecha_envio = DateTime.Now,
                            user_destinatario = usuario_autorizacion,
                            autorizacion_id = autorizacion_id,
                            enviado = false,
                            razon_no_envio = ex.Message
                        };
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
            int usuario_actual = Convert.ToInt32(Session["user_usuarioid"]);
            int bodega_actual = Convert.ToInt32(Session["user_bodega"]);
            icb_vehiculo vh = db.icb_vehiculo.Find(plan_mayor);
            int result = 0;
            configuracion_envio_correos correoconfig = db.configuracion_envio_correos.Where(d => d.activo).FirstOrDefault();

            usuarios_autorizaciones usuarios_autorizacion = db.usuarios_autorizaciones.FirstOrDefault(x => x.bodega_id == vh.id_bod);
            if (usuarios_autorizacion != null)
            {
                autorizaciones existe = db.autorizaciones.FirstOrDefault(x =>
                    x.plan_mayor == plan_mayor && x.bodega == bodega_actual && x.tipo_autorizacion == 2);

                if (existe == null)
                {
                    int usuario_autorizacion = usuarios_autorizacion.user_id;

                    autorizaciones autorizacion = new autorizaciones
                    {
                        plan_mayor = plan_mayor,
                        user_autorizacion = usuario_autorizacion,
                        user_creacion = usuario_actual,
                        fecha_creacion = DateTime.Now,
                        tipo_autorizacion = 2,
                        bodega = bodega_actual
                    };
                    db.autorizaciones.Add(autorizacion);
                    db.SaveChanges();
                    int autorizacion_id = db.autorizaciones.OrderByDescending(x => x.id)
                        .FirstOrDefault(x => x.tipo_autorizacion == 2).id;
                    result = 1;

                    try
                    {
                        notificaciones correo_enviado = db.notificaciones.FirstOrDefault(x =>
                            x.user_destinatario == usuario_autorizacion && x.enviado != true &&
                            x.autorizacion_id == autorizacion_id);
                        if (correo_enviado == null)
                        {
                            users user_destinatario = db.users.Find(usuario_autorizacion);
                            users user_remitente = db.users.Find(usuario_actual);

                            MailAddress de = new MailAddress(correoconfig.correo, "Notificación Iceberg");
                            MailAddress para = new MailAddress(user_destinatario.user_email,
                                user_destinatario.user_nombre + " " + user_destinatario.user_apellido);
                            MailMessage mensaje = new MailMessage(de, para);
                            mensaje.ReplyToList.Add(new MailAddress(user_remitente.user_email,
                                user_remitente.user_nombre + " " + user_remitente.user_apellido));
                            mensaje.Bcc.Add("jairo.mateus@exiware.com");
                            mensaje.Bcc.Add("correospruebaexi2019@gmail.com");
                            mensaje.Subject = "Solicitud facturación plan mayor " + plan_mayor;
                            mensaje.BodyEncoding = Encoding.Default;
                            mensaje.IsBodyHtml = true;
                            string html = "";
                            html += "<h4>Cordial Saludo</h4><br>";
                            html += "<p>El usuario " + user_remitente.user_nombre + " " + user_remitente.user_apellido +
                                    " solicita autorización para la facturación del vehículo " + plan_mayor + " - " +
                                    modelo + "</p><br /><br />";
                            html += "Por favor ingrese a la plataforma para dar autorización.";
                            mensaje.Body = html;

                            SmtpClient cliente = new SmtpClient(correoconfig.smtp_server)
                            {
                                Port = correoconfig.puerto,
                                UseDefaultCredentials = false,
                                Credentials = new NetworkCredential(correoconfig.usuario, correoconfig.password),
                                EnableSsl = true
                            };
                            cliente.Send(mensaje);

                            notificaciones envio = new notificaciones
                            {
                                user_remitente = usuario_actual,
                                asunto = "Notificación solicitud facturacion de vehiculo",
                                fecha_envio = DateTime.Now,
                                enviado = true,
                                user_destinatario = usuario_autorizacion,
                                autorizacion_id = autorizacion_id
                            };
                            db.notificaciones.Add(envio);
                            db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        notificaciones envio = new notificaciones
                        {
                            user_remitente = usuario_actual,
                            asunto = "Notificación solicitud facturacion de vehiculo",
                            fecha_envio = DateTime.Now,
                            user_destinatario = usuario_autorizacion,
                            autorizacion_id = autorizacion_id,
                            enviado = false,
                            razon_no_envio = ex.Message
                        };
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
                           nombre = v.icb_terceros.prinom_tercero != null
                               ? v.numero + " - " + v.icb_terceros.prinom_tercero + " " + v.icb_terceros.segnom_tercero + " " +
                                 v.icb_terceros.apellido_tercero + " " + v.icb_terceros.segapellido_tercero
                               : v.numero + " - " + v.icb_terceros.razon_social
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDocumentosFlota(int flotaid, int pedidoid)
        {
            vflota flota = db.vflota.Find(flotaid);

            if (flota != null)
            {
                var data = (from d in db.vdocrequeridosflota
                            where d.codflota == flota.flota
                            select new
                            {
                                d.id,
                                d.vdocumentosflota.documento,
                                d.vdocumentosflota.iddocumento,
                                cargado =
                                    db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x =>
                                        x.idpedido == pedidoid && x.iddocumento == d.iddocumento) != null
                                        ? db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x =>
                                            x.idpedido == pedidoid && x.iddocumento == d.iddocumento).id
                                        : 0
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
                            cargado =
                                db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion)
                                    .FirstOrDefault(x => x.idpedido == pedido && x.iddocumento == d.iddocumento) != null
                                    ? db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion)
                                        .FirstOrDefault(x => x.idpedido == pedido && x.iddocumento == d.iddocumento).id
                                    : 0
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
                            cargado =
                                db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion)
                                    .FirstOrDefault(x => x.idpedido == pedido && x.iddocumento == d.iddocumento) != null
                                    ? db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion)
                                        .FirstOrDefault(x => x.idpedido == pedido && x.iddocumento == d.iddocumento).id
                                    : 0
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDocumentosSeleccionados(int? flotaid, int pedidoid)
        {
            var data = (from d in db.vvalidacionpeddoc
                        where d.idflota == flotaid
                              && d.idpedido == pedidoid
                        select new
                        {
                            d.estado,
                            id = d.iddocrequerido
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult LiberarPedido()
        {
            List<vpedido> pedidos = db.vpedido
                .Where(x => x.planmayor != null && x.anulado == false && x.fecha_asignacion_planmayor != null).ToList();

            foreach (vpedido item in pedidos)
            {
                int dias = db.pedido_tliberacion.FirstOrDefault(x => x.id == item.icb_vehiculo.clasificacion_id)
                    .dias_para_liberar;
                if (item.fecha_asignacion_planmayor.Value.AddDays(dias).CompareTo(DateTime.Now) > 0)
                {
                    icb_vehiculo vehiculo = db.icb_vehiculo.Find(item.planmayor);
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
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            //Validamos que el usuario loguado tenga el rol y el permiso para modificar los valores del pedido
            int permiso = (from u in db.users
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
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            //Validamos que el usuario loguado tenga el rol y el permiso para modificar los valores del pedido
            int permiso = (from u in db.users
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
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            //Validamos que el usuario loguado tenga el rol y el permiso para modificar los valores del pedido
            int permiso = (from u in db.users
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
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            int permiso = (from u in db.users
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
            int info = (from a in db.vpedpago
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

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BrowserFacturasProforma(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult validarEstadoPedido(int pedidoId)
        {
            int estado = (from v in db.vpedido
                          where v.anulado && v.id == pedidoId
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
                            dp.valor,
                            fecha = dp.fecha.ToString()
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BrowserBackOffice(int? menu, string en)
        {
            // se realiza una decodificación de la variable "en" para el id encabezado que será usado para usarlo en el metodo facturación PDF
            int idencabezado = 0;
            if (!string.IsNullOrWhiteSpace(en))
            {
                //descodifico la variable en

                //la busco en encab documento. Si existe

                //Viewbag.idEncabezado=

                try
                {
                    byte[] decoEn = Convert.FromBase64String(en);
                    string en2 = Encoding.UTF8.GetString(decoEn);
                    bool convertir = int.TryParse(en2, out idencabezado);
                }
                catch (Exception)
                {
                    throw;
                }
            }

            if (idencabezado != 0)
            {
                ViewBag.encabezado = idencabezado;
            }
            else
            {
                idencabezado = 0;
                ViewBag.encabezado = idencabezado;
            }
            BuscarFavoritos(menu);
            ViewBag.tp_doc_registros = db.tp_doc_registros.Where(x => x.tpdoc_estado && x.sw == 3 && x.tipo == 2)
                .OrderBy(x => x.tpdoc_nombre).ToList();
            ViewBag.tp_doc_registrosFR = db.tp_doc_registros.Where(x => x.tpdoc_estado && x.sw == 3 && x.tipo == 4)
                .OrderBy(x => x.tpdoc_nombre).ToList();
            return View();
        }

        public ActionResult BrowserFlotas(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult BrowserBackOfficePendientes(int? menu)
        {
            BuscarFavoritos(menu);
            ViewBag.tp_doc_registros = db.tp_doc_registros.Where(x => x.tpdoc_estado && x.sw == 3 && x.tipo == 2)
                .OrderBy(x => x.tpdoc_nombre).ToList();
            ViewBag.tp_doc_registrosFR = db.tp_doc_registros.Where(x => x.tpdoc_estado && x.sw == 3 && x.tipo == 4)
                .OrderBy(x => x.tpdoc_nombre).ToList();
            return View();
        }

        public ActionResult BrowserUsados(int? menu)
        {
            BuscarFavoritos(menu);
            ViewBag.tp_doc_registros = db.tp_doc_registros.Where(x => x.tpdoc_estado && x.sw == 3 && x.tipo == 2)
                .OrderBy(x => x.tpdoc_nombre).ToList();
            ViewBag.tp_doc_registrosFR = db.tp_doc_registros.Where(x => x.tpdoc_estado && x.sw == 3 && x.tipo == 4)
                .OrderBy(x => x.tpdoc_nombre).ToList();
            return View();
        }

        public ActionResult VerificarDocumentosFlotas(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
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
                    planmayor = data.planmayor,
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
            vpedido vpedido = db.vpedido.Find(id);
            if (vpedido == null)
            {
                return HttpNotFound();
            }

            #region obj pedido

            VehiculoPedidoModel pedido = new VehiculoPedidoModel
            {
                numero = vpedido.numero,
                impfactura2 = vpedido.impfactura2,
                impfactura3 = vpedido.impfactura3,
                impfactura4 = vpedido.impfactura4,
                bodega = vpedido.bodega,
                anulado = vpedido.anulado,
                fecha = vpedido.fecha,
                idcotizacion = vpedido.idcotizacion,
                numerocotizacion = Convert.ToInt32(db.icb_cotizacion
                .Where(d => d.cot_idserial == vpedido.idcotizacion).Select(d => d.cot_numcotizacion).FirstOrDefault()),
                nit = vpedido.nit,
                numeroIdentificacion = Convert.ToString(db.icb_terceros.Where(d => d.tercero_id == vpedido.nit)
                .Select(d => d.doc_tercero).FirstOrDefault()),
                nit_asegurado = vpedido.nit_asegurado,
                nit2 = vpedido.nit2,
                nit3 = vpedido.nit3,
                nit4 = vpedido.nit4,
                nit5 = vpedido.nit5,
                vendedor = vpedido.vendedor,
                modelo = vpedido.modelo,
                id_anio_modelo = vpedido.id_anio_modelo,
                plan_venta = vpedido.plan_venta,
                planmayor = vpedido.planmayor,
                fecha_asignacion_planmayor = vpedido.fecha_asignacion_planmayor,
                asignado_por = vpedido.asignado_por,
                condicion = vpedido.condicion,
                dias_validez = vpedido.dias_validez,
                valor_unitario = vpedido.valor_unitario != null
                ? vpedido.valor_unitario.Value.ToString("N0", new CultureInfo("is-IS"))
                : "0",
                porcentaje_iva = vpedido.porcentaje_iva,
                valorPoliza = vpedido.valorPoliza != null
                ? vpedido.valorPoliza.Value.ToString("N0", new CultureInfo("is-IS"))
                : "0",
                pordscto = vpedido.pordscto,
                vrdescuento = vpedido.vrdescuento != null
                ? vpedido.vrdescuento.Value.ToString("N0", new CultureInfo("is-IS"))
                : "0",
                cantidad = vpedido.cantidad,
                tipo_carroceria = vpedido.tipo_carroceria,
                vrcarroceria = vpedido.vrcarroceria != null
                ? vpedido.vrcarroceria.Value.ToString("N0", new CultureInfo("is-IS"))
                : "0",
                vrtotal = vpedido.vrtotal != null
                ? vpedido.vrtotal.Value.ToString("N0", new CultureInfo("is-IS"))
                : "0",
                moneda = vpedido.moneda,
                id_aseguradora = vpedido.id_aseguradora,
                notas1 = vpedido.notas1,
                notas2 = vpedido.notas2,
                escanje = vpedido.escanje,
                eschevyplan = vpedido.eschevyplan,
                esLeasing = vpedido.esLeasing,
                esreposicion = vpedido.esreposicion,
                nit_prenda = vpedido.nit_prenda,
                flota = vpedido.flota,
                codflota = vpedido.codigoflota,
                facturado = vpedido.facturado,
                numfactura = vpedido.numfactura,
                porcentaje_impoconsumo = vpedido.porcentaje_impoconsumo,
                numeroplaca = vpedido.numeroplaca,
                motivo_anulacion = vpedido.motivo_anulacion,
                venta_gerencia = vpedido.venta_gerencia,
                Color_Deseado = vpedido.Color_Deseado,
                terminacionplaca = vpedido.terminacionplaca,
                bono = vpedido.bono,
                idmodelo = vpedido.idmodelo,
                nuevo = vpedido.nuevo,
                usado = vpedido.usado,
                servicio = vpedido.servicio,
                placapar = vpedido.placapar,
                placaimpar = vpedido.placaimpar,
                color_opcional = vpedido.color_opcional,
                cargomatricula = vpedido.cargomatricula,
                obsequioporcen = vpedido.obsequioporcen,
                valormatricula = Convert.ToString(vpedido.valormatricula),
                rango_placa = vpedido.rango_placa,
                marcvh_id = Convert.ToString(vpedido.marca),
                fec_creacion = vpedido.fec_creacion,
                userid_creacion = vpedido.userid_creacion
            };

            #endregion

            listas(pedido);
            BuscarFavoritos(menu);
            return View(pedido);
        }

        [HttpPost]
        public ActionResult VerificarDocumentosFlotas(VehiculoPedidoModel vpedido, int? menu)
        {
            //Se verifica los documentos que son anexados para facturar

            #region Documentos

            List<vvalidacionpeddoc> docsValidacion = db.vvalidacionpeddoc.Where(x => x.idflota == vpedido.flota && x.idpedido == vpedido.id)
                .ToList();
            if (docsValidacion.Count > 0)
            {
                foreach (vvalidacionpeddoc item in docsValidacion)
                {
                    if (vpedido.flota != null)
                    {
                        vdocrequeridosflota docrequeridos = db.vdocrequeridosflota.Find(item.iddocrequerido);
                        item.estado = Convert.ToBoolean(Request["documento_" + docrequeridos.id]);
                        db.Entry(item).State = EntityState.Modified;
                    }
                    else
                    {
                        //En la acciión de verificar se toma cada documento con respecto a lo solucionado
                        string asd = Request["documento_" + item.iddocrequerido];
                        item.estado = Convert.ToBoolean(Request["documento_" + item.iddocrequerido]);
                        db.Entry(item).State = EntityState.Modified;
                    }
                }
            }
            else
            {
                vflota codflota = db.vflota.Find(vpedido.flota);
                if (codflota != null)
                {
                    IQueryable<vdocrequeridosflota> docrequeridosflota = db.vdocrequeridosflota.Where(x => x.codflota == codflota.flota);
                    foreach (vdocrequeridosflota item in docrequeridosflota)
                    {
                        vvalidacionpeddoc validacion = new vvalidacionpeddoc
                        {
                            idpedido = vpedido.id,
                            idflota = vpedido.flota ?? 0,
                            codflota = codflota.flota,
                            iddocrequerido = item.id,
                            estado = Convert.ToBoolean(Request["documento_" + item.id])
                        };
                        db.vvalidacionpeddoc.Add(validacion);
                    }
                }
                else
                {
                    int docPer = 0;
                    string tipoPer = Request["tipoPer"].Trim();
                    if (tipoPer == "N")
                    {
                        docPer = 3;
                    }
                    else
                    {
                        docPer = 2;
                    }

                    IQueryable<vdocumentosflota> docrequeridos = db.vdocumentosflota.Where(x => x.id_tipo_documento == docPer);
                    foreach (vdocumentosflota item in docrequeridos)
                    {
                        vvalidacionpeddoc validacion = new vvalidacionpeddoc
                        {
                            idpedido = vpedido.id,
                            iddocrequerido = item.id,
                            estado = Convert.ToBoolean(Request["documento_" + item.id])
                        };
                        db.vvalidacionpeddoc.Add(validacion);
                    }
                }
            }

            #endregion

            int result = db.SaveChanges();
            if (result > 0)
            {
                //----- Tomar el ultimo pedido agregado para mostrar la factura proforma
                vpedido ultimoAgregado = db.vpedido.OrderByDescending(x => x.numero).FirstOrDefault();
                ViewBag.pedidoId = ultimoAgregado.numero;

                TempData["mensaje"] = "Documentos actualizados correctamente";
                return RedirectToAction("BrowserFlotas", new { vpedido.id, menu });
            }

            TempData["mensaje_error"] = "Error al registrar los documentos del pedido, por favor intente nuevamente";

            listas(vpedido);
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

            VehiculoPedidoModel pedido = new VehiculoPedidoModel
            {
                numero = vpedido.numero,
                impfactura2 = vpedido.impfactura2,
                impfactura3 = vpedido.impfactura3,
                impfactura4 = vpedido.impfactura4,
                bodega = vpedido.bodega,
                anulado = vpedido.anulado,
                fecha = vpedido.fecha,
                idcotizacion = vpedido.idcotizacion,
                numerocotizacion = Convert.ToInt32(db.icb_cotizacion
                    .Where(d => d.cot_idserial == vpedido.idcotizacion).Select(d => d.cot_numcotizacion)
                    .FirstOrDefault()),
                nit = vpedido.nit,
                numeroIdentificacion = Convert.ToString(db.icb_terceros.Where(d => d.tercero_id == vpedido.nit)
                    .Select(d => d.doc_tercero).FirstOrDefault()),
                nit_asegurado = vpedido.nit_asegurado,
                nit2 = vpedido.nit2,
                nit3 = vpedido.nit3,
                nit4 = vpedido.nit4,
                nit5 = vpedido.nit5,
                vendedor = vpedido.vendedor,
                modelo = vpedido.modelo,
                id_anio_modelo = vpedido.id_anio_modelo,
                plan_venta = vpedido.plan_venta,
                planmayor = vpedido.planmayor,
                fecha_asignacion_planmayor = vpedido.fecha_asignacion_planmayor,
                asignado_por = vpedido.asignado_por,
                condicion = vpedido.condicion,
                dias_validez = vpedido.dias_validez,
                valor_unitario = vpedido.valor_unitario != null
                    ? vpedido.valor_unitario.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "0",
                porcentaje_iva = vpedido.porcentaje_iva,
                valorPoliza = vpedido.valorPoliza != null
                    ? vpedido.valorPoliza.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "0",
                pordscto = vpedido.pordscto,
                vrdescuento = vpedido.vrdescuento != null
                    ? vpedido.vrdescuento.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "0",
                cantidad = vpedido.cantidad,
                tipo_carroceria = vpedido.tipo_carroceria,
                vrcarroceria = vpedido.vrcarroceria != null
                    ? vpedido.vrcarroceria.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "0",
                vrtotal = vpedido.vrtotal != null
                    ? vpedido.vrtotal.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "0",
                moneda = vpedido.moneda,
                id_aseguradora = vpedido.id_aseguradora,
                notas1 = vpedido.notas1,
                notas2 = vpedido.notas2,
                escanje = vpedido.escanje,
                eschevyplan = vpedido.eschevyplan,
                esLeasing = vpedido.esLeasing,
                esreposicion = vpedido.esreposicion,
                nit_prenda = vpedido.nit_prenda,
                flota = vpedido.flota,
                codflota = vpedido.codigoflota,
                facturado = vpedido.facturado,
                numfactura = vpedido.numfactura,
                porcentaje_impoconsumo = vpedido.porcentaje_impoconsumo,
                numeroplaca = vpedido.numeroplaca,
                motivo_anulacion = vpedido.motivo_anulacion,
                venta_gerencia = vpedido.venta_gerencia,
                Color_Deseado = vpedido.Color_Deseado,
                terminacionplaca = vpedido.terminacionplaca,
                bono = vpedido.bono,
                idmodelo = vpedido.idmodelo,
                nuevo = vpedido.nuevo,
                usado = vpedido.usado,
                servicio = vpedido.servicio,
                placapar = vpedido.placapar,
                placaimpar = vpedido.placaimpar,
                color_opcional = vpedido.color_opcional,
                cargomatricula = vpedido.cargomatricula,
                obsequioporcen = vpedido.obsequioporcen,
                valormatricula = Convert.ToString(vpedido.valormatricula),
                rango_placa = vpedido.rango_placa,
                marcvh_id = Convert.ToString(vpedido.marca),
                fec_creacion = vpedido.fec_creacion,
                userid_creacion = vpedido.userid_creacion
            };

            #endregion

            listas(pedido);
            BuscarFavoritos(menu);
            return View(pedido);
        }

        [HttpPost]
        public ActionResult Verificar(VehiculoPedidoModel vpedido, int? menu)
        {
            //Se verifica los documentos que son anexados para facturar

            #region Documentos

            List<vvalidacionpeddoc> docsValidacion = db.vvalidacionpeddoc.Where(x => x.idflota == vpedido.flota && x.idpedido == vpedido.id)
                .ToList();
            if (docsValidacion.Count > 0)
            {
                foreach (vvalidacionpeddoc item in docsValidacion)
                {
                    if (vpedido.flota != null)
                    {
                        vdocrequeridosflota docrequeridos = db.vdocrequeridosflota.Find(item.iddocrequerido);
                        item.estado = Convert.ToBoolean(Request["documento_" + docrequeridos.id]);
                        db.Entry(item).State = EntityState.Modified;
                    }
                    else
                    {
                        //En la acciión de verificar se toma cada documento con respecto a lo solucionado
                        string asd = Request["documento_" + item.iddocrequerido];
                        item.estado = Convert.ToBoolean(Request["documento_" + item.iddocrequerido]);
                        db.Entry(item).State = EntityState.Modified;
                    }
                }
            }
            else
            {
                vflota codflota = db.vflota.Find(vpedido.flota);
                if (codflota != null)
                {
                    IQueryable<vdocrequeridosflota> docrequeridosflota = db.vdocrequeridosflota.Where(x => x.codflota == codflota.flota);
                    foreach (vdocrequeridosflota item in docrequeridosflota)
                    {
                        vvalidacionpeddoc validacion = new vvalidacionpeddoc
                        {
                            idpedido = vpedido.id,
                            idflota = vpedido.flota ?? 0,
                            codflota = codflota.flota,
                            iddocrequerido = item.id,
                            estado = Convert.ToBoolean(Request["documento_" + item.id])
                        };
                        db.vvalidacionpeddoc.Add(validacion);
                    }
                }
                else
                {
                    int docPer = 0;
                    string tipoPer = Request["tipoPer"].Trim();
                    if (tipoPer == "N")
                    {
                        docPer = 3;
                    }
                    else
                    {
                        docPer = 2;
                    }

                    IQueryable<vdocumentosflota> docrequeridos = db.vdocumentosflota.Where(x => x.id_tipo_documento == docPer);
                    foreach (vdocumentosflota item in docrequeridos)
                    {
                        vvalidacionpeddoc validacion = new vvalidacionpeddoc
                        {
                            idpedido = vpedido.id,
                            iddocrequerido = item.id,
                            estado = Convert.ToBoolean(Request["documento_" + item.id])
                        };
                        db.vvalidacionpeddoc.Add(validacion);
                    }
                }
            }

            #endregion

            int result = db.SaveChanges();
            if (result > 0)
            {
                //----- Tomar el ultimo pedido agregado para mostrar la factura proforma
                vpedido ultimoAgregado = db.vpedido.OrderByDescending(x => x.numero).FirstOrDefault();
                ViewBag.pedidoId = ultimoAgregado.numero;

                TempData["mensaje"] = "Documentos actualizados correctamente";
                return RedirectToAction("BrowserBackOffice", new { vpedido.id, menu });
            }

            TempData["mensaje_error"] = "Error al registrar los documentos del pedido, por favor intente nuevamente";

            listas(vpedido);
            return View();
        }

        private void enableFlot(object vpedido, EventArgs e)
        {
            int flag = 0;
            string chain = "'Update vflota set idflota = '" + vpedido + "''";
            SqlCommand command = new SqlCommand(chain);

            if (flag == 1)
            {
                //se recibe una conexión
            }
        }

        public JsonResult actualizardocumentos()
        {
            string id = Request.Form["id"];
            int pedido = Convert.ToInt32(Request.Form["pedido"]);
            System.Web.HttpPostedFileBase documento = Request.Files["documento"];
            string ruta = "";

            if (!string.IsNullOrWhiteSpace(id) && documento.ContentLength > 0)
            {
                //busco la consulta
                bool conver = int.TryParse(id, out int idconsulta);

                int? docFacturacion = db.vdocumentosflota.FirstOrDefault(x => x.id == idconsulta).iddocumento;
                vpedido consulta = db.vpedido.Where(d => d.id == pedido).FirstOrDefault();
                if (consulta != null)
                {
                    //guardo el archivo
                    string archivo = documento.FileName;
                    if (CheckFileType(documento.FileName) == false)
                    {
                        TempData["mensaje_error"] = TempData["mensaje_error"] +
                                                    "error. extensión no permitida. las extensiones permitidas son .pdf, .doc, .docx, .jpg , .jpeg  y .png";
                    }
                    else
                    {
                        //guardo el archivo
                        ruta = "documentosVehiculo/" + idconsulta + "_" + consulta.planmayor + "_" + archivo;
                        string path = Server.MapPath("~/Content/documentosVehiculo/" + idconsulta + "_" +
                                                  consulta.planmayor + "_" + archivo);
                        documento.SaveAs(path);
                    }

                    List<vvalidacionpeddoc> buscarDocPedido = db.vvalidacionpeddoc.Where(x => x.idpedido == pedido).ToList();
                    if (buscarDocPedido.Count > 0)
                    {
                        foreach (vvalidacionpeddoc item in buscarDocPedido)
                        {
                            if (item.iddocrequerido == Convert.ToInt32(id))
                            {
                                item.estado = true;
                                db.Entry(item).State = EntityState.Modified;
                            }
                        }
                    }

                    documentos_vehiculo docVH = new documentos_vehiculo
                    {
                        iddocumento = docFacturacion,
                        idvehiculo = consulta.planmayor,
                        rutadocumento = ruta,
                        fec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        estado = true,
                        idtercero = Convert.ToInt32(consulta.nit),
                        idpedido = pedido
                    };

                    db.documentos_vehiculo.Add(docVH);
                    db.SaveChanges();
                    return Json(1, JsonRequestBehavior.AllowGet);
                }

                return Json(0, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult verArchivo(int id)
        {
            string info = db.documentos_vehiculo.FirstOrDefault(x => x.id == id).rutadocumento;

            string urlActual0 = Request.Url.Scheme + "://" + Request.Url.Authority;
            //var lastFolder = Path.GetDirectoryName(urlActual0);
            //var pathWithoutLastFolder = Path.GetDirectoryName(lastFolder);
            string url = urlActual0 += @"/Content/" + info;

            return Json(url, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPedidosFlotas()
        {
            var data = (from p in db.vw_browserBackOffice
                        join ped in db.vpedido
                        on p.numero equals ped.numero
                        where ped.flota != null && p.facturado == false && p.planmayor != null && p.autorizado != true
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
                            cliente = p.razon_social != null
                                ? p.doc_tercero + "-" + p.razon_social
                                : p.doc_tercero + "-" + p.prinom_tercero + " " + p.segnom_tercero + " " + p.apellido_tercero +
                                  " " + p.segapellido_tercero,
                            p.fecha,
                            fechaA = p.fecha_asignacion_planmayor,
                            saldo = p.saldo != null ? p.saldo.ToString() : " ",
                            planmayor = p.planmayor.ToString(),
                            p.fecha_asignacion_planmayor,
                            p.autorizado,
                            p.autorizados,
                            p.facturado
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
                fechaA = x.fechaA != null ? x.fechaA.Value.ToString("yyyy/MM/dd") : "",
                x.saldo,
                x.planmayor,
                fecha_asignacion_planmayor = x.fecha_asignacion_planmayor != null
                    ? x.fecha_asignacion_planmayor.Value.ToString("yyyy/MM/dd")
                    : "",
                x.autorizado,
                x.autorizados,
                x.facturado,
                documentos = verificarPendientes(x.id),
                facturaAcc = !string.IsNullOrWhiteSpace(x.planmayor) ? facturaAcc(x.planmayor) : ""
            });

            return Json(data2, JsonRequestBehavior.AllowGet);

        }

        public JsonResult BuscarPedidosPaginadosBackOffice()
        {
            var data = (from p in db.vw_browserBackOffice
                        join evento in db.vw_referencias_total
                            on p.planmayor equals evento.codigo 
                        where /*p.facturado == false &&*/ p.planmayor != null && p.para_facturar 
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
                            cliente = p.razon_social != null
                                ? p.doc_tercero + "-" + p.razon_social
                                : p.doc_tercero + "-" + p.prinom_tercero + " " + p.segnom_tercero + " " + p.apellido_tercero +
                                  " " + p.segapellido_tercero,
                            p.fecha,
                            fechaA = p.fecha_asignacion_planmayor,
                            saldo = p.saldo != null ? p.saldo.ToString() : " ",
                            planmayor = p.planmayor.ToString(),
                            p.fecha_asignacion_planmayor,
                            p.autorizado,
                            p.autorizados,
                            p.facturado,
                            p.codigo
                        }).ToList().Distinct();

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
                fechaA = x.fechaA != null ? x.fechaA.Value.ToString("yyyy/MM/dd") : "",
                x.saldo,
                x.planmayor,
                fecha_asignacion_planmayor = x.fecha_asignacion_planmayor != null
                    ? x.fecha_asignacion_planmayor.Value.ToString("yyyy/MM/dd")
                    : "",
                x.autorizado,
                x.autorizados,
                x.facturado,
                documentos = verificarPendientes(x.id),
                x.codigo,
                facturaAcc = !string.IsNullOrWhiteSpace(x.planmayor) ? facturaAcc(x.planmayor) : "",
                fRecepcion=verfechaRecepcion(x.planmayor),
            });

            return Json(data2, JsonRequestBehavior.AllowGet);
        }

        public string verfechaRecepcion(string planmayor)
        {
            var respuesta = "";
            var idparametro = db.icb_sysparameter.Where(d => d.syspar_cod == "P8").FirstOrDefault();
            var param = idparametro != null ? Convert.ToInt32(idparametro.syspar_value) : 5;

            icb_vehiculo_eventos data = db.icb_vehiculo_eventos
                .Where(x => x.planmayor == planmayor && x.icb_tpeventos.codigoevento == param).FirstOrDefault();
            if (data != null)
            {
                respuesta = data.fechaevento.ToString("yyyy/MM/dd HH:mm",new CultureInfo("en-US"));
            }
            return respuesta;
        }
        public JsonResult BuscarPedidosPaginadosBackOfficePendientes()
        {
            //parametro evento 
            icb_sysparameter param = db.icb_sysparameter.Where(d => d.syspar_cod == "P86").FirstOrDefault();
            int evento = param != null ? Convert.ToInt32(param.syspar_value) : 1028;
            var info = (from p in db.vw_browserBackOffice
                        join vev in db.icb_vehiculo_eventos
                            on p.planmayor equals vev.planmayor
                        join tpev in db.icb_tpeventos
                            on vev.id_tpevento equals tpev.tpevento_id
                        join pe in db.vw_pendientesAlistamiento
                            on p.planmayor equals pe.planmayor
                        where p.facturado && p.planmayor != null && tpev.tpevento_id == evento && pe.numerosoat != null &&
                              pe.FechaPedido != null && pe.fec_desembolso != null
                              && pe.FechaVenta != null && pe.FechaTramite != null && pe.FechaMatricula != null &&
                              pe.FechaManifiesto != null && pe.FechaAlistamiento != null
                              && pe.FechaRecepcionBodega != null && pe.FechaFinAlistamiento != null &&
                              pe.FechaEntrega != null && pe.FechaProgramacioEntrega != null && pe.pazysalvo
                              && pe.numfactura != null
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
                            pe.FechaMatricula,
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
                            p.fecha_entrega,
                            p.planmayor,
                            p.autorizado,
                            p.numfactura,
                            p.fecha_venta,
                            p.idCliente,
                            pe.FechaRecepcionBodega
                        }).ToList();

            var data = info.Select(p => new
            {
                p.id,
                numero = p.numero != null ? p.numero.Value.ToString() : "",
                p.bodega,
                p.bodccs_nombre,
                p.proceso,
                p.modelo,
                vrtotal = p.vrtotal != null ? p.vrtotal.Value.ToString("N0", new CultureInfo("is-Is")) : "",
                placa = p.plac_vh,
                color = p.colvh_nombre != null ? p.colvh_nombre : "",
                vin = p.vin != null ? p.vin : "",
                ubicacion = p.ubivh_nombre != null ? p.ubivh_nombre : "",
                anio = p.anio_vh != null ? p.anio_vh : null,
                fechaMatricula = p.FechaMatricula != null ? p.FechaMatricula.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                valor = p.valor != null ? p.valor.ToString() : "0",
                p.asesor,
                cliente = p.razon_social != null
                    ? p.doc_tercero + "-" + p.razon_social
                    : p.doc_tercero + "-" + p.prinom_tercero + " " + p.segnom_tercero + " " + p.apellido_tercero + " " +
                      p.segapellido_tercero,
                fecha = p.fecha != null ? p.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                fechaA = p.fecha_asignacion_planmayor.ToString(),
                saldo = p.saldo != null ? p.saldo.ToString() : " ",
                planmayor = p.planmayor.ToString(),
                fecha_asignacion_planmayor =
                    p.fecha_asignacion_planmayor.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                autorizado = p.autorizado != null ? p.autorizado.Value : false,
                fechaEntrega = p.fecha_entrega != null
                    ? p.fecha_entrega.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : "",
                factura = p.numfactura != null ? p.numfactura : 0,
                facturaFecha = p.fecha_venta != null
                    ? p.fecha_venta.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                p.idCliente,
                verificarAlistamiento =
                    !string.IsNullOrWhiteSpace(p.planmayor) ? verificarAlistamientoAcc(p.planmayor) : "",
                fRecepcion=p.FechaRecepcionBodega!=null?p.FechaRecepcionBodega.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")):""
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
                            p.tipo_compra
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
                fechaA = p.fecha_asignacion_planmayor != null
                    ? p.fecha_asignacion_planmayor.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                saldo = p.saldo != null ? p.saldo : 0,
                planmayor = !string.IsNullOrWhiteSpace(p.plan_mayor) ? p.plan_mayor.ToString() : "",
                fecha_asignacion_planmayor = p.fecha_asignacion_planmayor != null
                    ? p.fecha_asignacion_planmayor.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                autorizado = p.autorizado != null ? p.autorizado : false,
                factura = p.numfactura != null ? p.numfactura.ToString() : "",
                facturaFecha = p.fecha_venta != null
                    ? p.fecha_venta.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                tipoCompra = !string.IsNullOrWhiteSpace(p.tipo_compra) ? p.tipo_compra : ""
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult verificarRolUsuario()
        {
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            //Validamos que el usuario loguado tenga el rol y el permiso para modificar los valores del pedido
            int data = (from u in db.users
                        join r in db.rols
                            on u.rol_id equals r.rol_id
                        where u.user_id == usuario
                        select
                            u.rol_id
                ).FirstOrDefault();

            return Json(data, JsonRequestBehavior.AllowGet);
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
                                   cargado =
                                       db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x =>
                                           x.idvehiculo == planmayor && x.iddocumento == te.iddocasociado) != null
                                           ? db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x =>
                                               x.idvehiculo == planmayor && x.iddocumento == te.iddocasociado).id
                                           : 0
                               }).ToList();

                List<int> listaEventos = new List<int>();
                foreach (var item in eventos)
                {
                    listaEventos.Add(item.tpevento_id);
                }

                var buscarFaltantes = (from a in db.icb_tpeventos
                                       where !listaEventos.Contains(a.tpevento_id) && a.tpevento_estado
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
                    x.cargado
                }).ToList();

                return Json(new { info = true, data2, buscarFaltantes }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { info = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarMotivosAnulacion()
        {
            var data = (from m in db.motivoanulacion
                        where m.estado
                        select new
                        {
                            m.id,
                            m.motivo
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BrowserPendientesAlistamiento(int? menu, string planmayor)
        {
            if (planmayor != null)
            {
                ViewBag.planmayor = planmayor;
            }
            else
            {
                ViewBag.planmayor = "-1";
            }
            ViewBag.Motivo = Motivo;
            ViewBag.Nota = Nota;
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult traerVhExhibicion()
        {
            var buscarEvento = (from vh in db.icb_vehiculo
                                join ev in db.icb_vehiculo_eventos
                                    on vh.plan_mayor equals ev.planmayor
                                join mv in db.modelo_vehiculo
                                    on vh.modvh_id equals mv.modvh_codigo
                                join cv in db.color_vehiculo
                                    on vh.colvh_id equals cv.colvh_id
                                join bod in db.bodega_concesionario
                                    on vh.id_bod equals bod.id
                                where ev.evento_estado && ev.evento_nombre == "Envio a Exibicion"
                                select new
                                {
                                    ev.evento_id,
                                    ev.planmayor,
                                    ev.evento_nombre,
                                    ev.evento_estado,
                                    anio = vh.anio_vh,
                                    modelo = mv.modvh_nombre,
                                    color = cv.colvh_nombre,
                                    vh.vin,
                                    bodega = bod.bodccs_nombre
                                }).ToList();
            var data = buscarEvento.Select(p => new
            {
                p.planmayor,
                p.anio,
                p.modelo,
                p.color,
                p.vin,
                p.bodega,
                botonalismec = !string.IsNullOrWhiteSpace(p.planmayor) ? recepcionBodega(p.planmayor) : "",
                botonalisacce = !string.IsNullOrWhiteSpace(p.planmayor) ? botonalistamientoa(p.planmayor) : "",
                botonalisembe = !string.IsNullOrWhiteSpace(p.planmayor) ? alistamiento(p.planmayor) : ""
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public string recepcionBodega(string planmayor)
        {
            var idparametro = db.icb_sysparameter.Where(d => d.syspar_cod == "P8").FirstOrDefault();
            var param = idparametro != null ? Convert.ToInt32(idparametro.syspar_value) : 5;

            icb_vehiculo_eventos data = db.icb_vehiculo_eventos
                .Where(x => x.planmayor == planmayor && x.icb_tpeventos.codigoevento == param).FirstOrDefault();


            string resultado =
                "<div class='col-xs-12 col-sm-12 col-md-12 col-lg-12' style='white-space:nowrap;width:100%;heigth:100%'>";
            resultado = resultado + "<div class='col-xs-3 col-sm-3 col-md-3 col-lg-3' style='display:inline-block;'>";
            string clase = "btn-sm ";
            string fechax = "";
            if (data == null)
            {
                clase = clase + "btn-danger";
            }

            else
            {
                string fechax_fin = data.fechaevento.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
                fechax = fechax +
                         "<br/><span class='text-success' style='font-size:9px;'><i class='text-success fa fa-check-circle'></i>" +
                         fechax_fin + "</span>";

                clase = clase + "btn-success";
            }

            string icono = "<button type='button' class='" + clase + "'><i class='fa fa-cogs'></i>";
            resultado = resultado + icono + "</div>";
            resultado = resultado + "<div class='col-xs-9 col-sm-9 col-md-9 col-lg-9' style='display:inline-block;'>";
            resultado = resultado + fechax;
            resultado = resultado + "</div>";
            resultado = resultado + "</div>";

            return resultado;
        }

        public string alistamiento(string planmayor)
        {
            icb_vehiculo_eventos data = db.icb_vehiculo_eventos
                .Where(x => x.planmayor == planmayor && x.evento_nombre == "Envió Alistamiento").FirstOrDefault();
            icb_vehiculo_eventos data2 = db.icb_vehiculo_eventos
                .Where(x => x.planmayor == planmayor && x.evento_nombre == "Fin Alistamiento").FirstOrDefault();

            string resultado =
                "<div class='col-xs-12 col-sm-12 col-md-12 col-lg-12' style='white-space:nowrap;width:100%;heigth:100%'>";
            resultado = resultado + "<div class='col-xs-3 col-sm-3 col-md-3 col-lg-3' style='display:inline-block;'>";
            string clase = "btn-sm ";
            string fechax = "";
            if (data == null)
            {
                clase = clase + "btn-danger";
            }
            else if (data != null && data2 == null)
            {
                clase = clase + "btn-warning";
                string fechax_inicio = data.fechaevento.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
                fechax = fechax +
                         "<span class='text-warning' style='font-size:9px;><i class='text-warning fa fa-check-circle'></i>" +
                         fechax_inicio + "</span>";
            }
            else
            {
                string fechax_inicio = data.fechaevento.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
                fechax = fechax +
                         "<span class='text-warning' style='font-size:9px;'><i class='text-warning fa fa-check-circle'></i>" +
                         fechax_inicio + "</span>";
                string fechax_fin = data2.fechaevento.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
                fechax = fechax +
                         "<br/><span class='text-success' style='font-size:9px;'><i class='text-success fa fa-check-circle'></i>" +
                         fechax_fin + "</span>";

                clase = clase + "btn-success";
            }

            string icono = "<button type='button' class='" + clase + "'><i class='fa fa-car'></i>";
            resultado = resultado + icono + "</div>";
            resultado = resultado + "<div class='col-xs-9 col-sm-9 col-md-9 col-lg-9' style='display:inline-block;'>";
            resultado = resultado + fechax;
            resultado = resultado + "</div>";
            resultado = resultado + "</div>";


            return resultado;
        }

        //inicio caso 2299
        public JsonResult traerDatosBusqueda(bool? facturado, bool? facturadoDes, bool? facturadoMat,
            bool? facturadoMatDes)
        {
            System.Linq.Expressions.Expression<Func<vw_pendientesAlistamiento, bool>> predicate = PredicateBuilder.True<vw_pendientesAlistamiento>();
            System.Linq.Expressions.Expression<Func<vw_pendientesAlistamiento, bool>> vh_Facturados = PredicateBuilder.False<vw_pendientesAlistamiento>();
            System.Linq.Expressions.Expression<Func<vw_pendientesAlistamiento, bool>> vh_FacturadosDes = PredicateBuilder.False<vw_pendientesAlistamiento>();
            System.Linq.Expressions.Expression<Func<vw_pendientesAlistamiento, bool>> vh_FacturadosMat = PredicateBuilder.False<vw_pendientesAlistamiento>();
            System.Linq.Expressions.Expression<Func<vw_pendientesAlistamiento, bool>> vh_FacturadosDesMat = PredicateBuilder.False<vw_pendientesAlistamiento>();

            if (facturado == true)
            {
                predicate = predicate.And(x => x.FechaVenta != null);
            }

            if (facturadoDes == true)
            {
                predicate = predicate.And(x => x.FechaVenta != null);
                predicate = predicate.And(x => x.fec_desembolso != null);
            }

            if (facturadoMat == true)
            {
                predicate = predicate.And(x => x.FechaVenta != null);
                predicate = predicate.And(x => x.FechaMatricula != null);
            }

            if (facturadoMatDes == true)
            {
                predicate = predicate.And(x => x.FechaVenta != null);
                predicate = predicate.And(x => x.fec_desembolso != null);
                predicate = predicate.And(x => x.FechaMatricula != null);
            }

            List<vw_pendientesAlistamiento> buscar2 = db.vw_pendientesAlistamiento.Where(predicate).ToList();
            Dictionary<string, string> estilosAlist = new Dictionary<string, string>
            {
                ["est_" + iCreateOt] = "info",
                ["est_" + iExecutionOt] = "warning",
                ["est_" + iFinishOt] = "success",
                ["est_" + isNoExiste] = "success"
            };
            operacionAlistamiento();
            var data = buscar2.Select(p => new
            {
                p.id,
                placa= p.plac_vh != null ? p.plac_vh : "",
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
                pedidoFecha = p.FechaPedido != null
                    ? p.FechaPedido.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                desmbolsoFecha = p.fec_desembolso != null
                    ? p.fec_desembolso.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                facturaFecha = p.FechaVenta != null
                    ? p.FechaVenta.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                facturaAcc = !string.IsNullOrWhiteSpace(p.planmayor) ? facturaAcc(p.planmayor) : "",
                tramitesFecha = p.FechaTramite != null
                    ? p.FechaTramite.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                matriculaFecha = p.FechaMatricula != null
                    ? p.FechaMatricula.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                manifiestoFecha = p.FechaManifiesto != null
                    ? p.FechaManifiesto.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : !string.IsNullOrWhiteSpace(p.plan_mayor)
                        ? p.fecentman_vh != null
                            ? p.fecentman_vh.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                            : ""
                        : "",
                inicioFecha = p.FechaAlistamiento != null
                    ? p.FechaAlistamiento.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                botonalisembe = !string.IsNullOrWhiteSpace(p.planmayor)
                    ? botonalistamientoe(p.planmayor, p.FechaAlistamiento, p.FechaFinAlistamiento)
                    : "",
                finFechaRecepcion = p.FechaRecepcionBodega != null
                    ? p.FechaRecepcionBodega.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                botonalismec = !string.IsNullOrWhiteSpace(p.planmayor)
                    ? botonalistamientoem(p.planmayor, p.FechaRecepcionBodega)
                    : "",
                botonalisacce = !string.IsNullOrWhiteSpace(p.planmayor) ? botonalistamientoa(p.planmayor) : "",
                finFecha = p.FechaFinAlistamiento != null
                    ? p.FechaFinAlistamiento.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                entregaFecha = p.FechaEntrega != null
                    ? p.FechaEntrega.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                programacionFecha = p.FechaProgramacioEntrega != null
                    ? p.FechaProgramacioEntrega.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                p.pazysalvo,
                usupazysalvo = p.usupazysalvo != null ? p.usupazysalvo : "",
                numFactura = p.numfactura != null ? p.numfactura.ToString() : "",
                estadoAlistamiento = p.estadoorden != null ? p.estadoorden.Value : 0,
                //estadoAlistaminetoEstilo = (p.estadoorden != null) ? estilosAlist["est_" + p.estadoorden.Value] : "",
                estadoAlistaminetoEstilo = "success",

                poliza = p.poliza != null ? p.poliza : ""
            }).Distinct();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        //fin caso 2299

        /// <summary>
        ///     Este es el llamado de la vista de pendientes matricula que será usado en otro controlador llamado
        ///     NoteDebitScheduleController
        /// </summary>
        /// <returns></returns>
        public ActionResult BrowserPendientesMatricula(int? menu, string tramitados)
        {
            ViewBag.tramite = tramitados;
            BuscarFavoritos(menu);
            return View();
        }
        //Leo

        public string facturaAcc(string planmayor)
        {
            string resultado = "";

            int pedido = db.vpedido.Where(x => x.planmayor == planmayor).Select(z => z.id).FirstOrDefault();
            List<vpedrepuestos> insaccesorios = db.vpedrepuestos.Where(x => x.pedido_id == pedido).ToList();
            icb_sysparameter acce1 = db.icb_sysparameter.Where(d => d.syspar_cod == "P136").FirstOrDefault();
            int accesori = acce1 != null ? Convert.ToInt32(acce1.syspar_value) : 1062;


            if (insaccesorios.Count > 0)
            {
                icb_vehiculo_eventos eventoAcc = db.icb_vehiculo_eventos
                    .Where(x => x.planmayor == planmayor && x.id_tpevento == accesori).FirstOrDefault();
                if (eventoAcc != null)
                {
                    resultado = eventoAcc.fechaevento.ToString("yyyy/MM/dd", new CultureInfo("is-IS"));
                }
            }
            else
            {
                resultado = "No tiene accesorios";
            }


            return resultado;
        }

        public JsonResult verificarPermisoPazysalvo()
        {
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            //Validamos que el usuario loguado tenga el rol y el permiso para ver toda la informacion
            int data = (from u in db.users
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
            string numero2 = "";
            if (numero > 100000)
            {
                numero2 = numero.ToString();
            }
            else if (numero > 10000)
            {
                numero2 = "0" + numero;
            }
            else if (numero > 1000)
            {
                numero2 = "00" + numero;
            }
            else if (numero > 100)
            {
                numero2 = "000" + numero;
            }
            else if (numero > 10)
            {
                numero2 = "0000" + numero;
            }
            else if (numero >= 0)
            {
                numero2 = "00000" + numero;
            }

            return numero2;
        }

        public string botonalistamientoe(string planmayor, DateTime? fecha_inicio, DateTime? fecha_fin)
        {
            string resultado =
                "<div class='col-xs-12 col-sm-12 col-md-12 col-lg-12' style='white-space:nowrap;width:100%;heigth:100%'>";
            resultado = resultado + "<div class='col-xs-3 col-sm-3 col-md-3 col-lg-3' style='display:inline-block;'>";
            string clase = "btn-sm ";
            string fechax = "";
            if (fecha_inicio == null)
            {
                clase = clase + "btn-danger";
            }
            else if (fecha_inicio != null && fecha_fin == null)
            {
                clase = clase + "btn-warning";
                string fechax_inicio = fecha_inicio.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
                fechax = fechax +
                         "<span class='text-warning' style='font-size:9px;><i class='text-warning fa fa-check-circle'></i>" +
                         fechax_inicio + "</span>";
            }
            else
            {
                string fechax_inicio = fecha_inicio.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
                fechax = fechax +
                         "<span class='text-warning' style='font-size:9px;'><i class='text-warning fa fa-check-circle'></i>" +
                         fechax_inicio + "</span>";
                string fechax_fin = fecha_fin.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
                fechax = fechax +
                         "<br/><span class='text-success' style='font-size:9px;'><i class='text-success fa fa-check-circle'></i>" +
                         fechax_fin + "</span>";

                clase = clase + "btn-success";
            }

            string icono = "<button type='button' class='" + clase + "'><i class='fa fa-car'></i>";
            //busco si el vehículo ha tenido alistamiento de embellecimiento
            resultado = resultado + icono + "</div>";
            resultado = resultado + "<div class='col-xs-9 col-sm-9 col-md-9 col-lg-9' style='display:inline-block;'>";
            resultado = resultado + fechax;
            resultado = resultado + "</div>";
            resultado = resultado + "</div>";

            return resultado;
        }

        public string botonalistamientoem(string planmayor, DateTime? fecha_fin)
        {
            string resultado =
                "<div class='col-xs-12 col-sm-12 col-md-12 col-lg-12' style='white-space:nowrap;width:100%;heigth:100%'>";
            resultado = resultado + "<div class='col-xs-3 col-sm-3 col-md-3 col-lg-3' style='display:inline-block;'>";
            string clase = "btn-sm ";
            string fechax = "";
            if (fecha_fin == null)
            {
                clase = clase + "btn-danger";
            }

            else
            {
                string fechax_fin = fecha_fin.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
                fechax = fechax +
                         "<br/><span class='text-success' style='font-size:9px;'><i class='text-success fa fa-check-circle'></i>" +
                         fechax_fin + "</span>";

                clase = clase + "btn-success";
            }

            string icono = "<button type='button' class='" + clase + "'><i class='fa fa-cogs'></i>";
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
            int pedido = db.vpedido.Where(x => x.planmayor == planmayor).Select(z => z.id).FirstOrDefault();
            List<vpedrepuestos> insaccesorios = db.vpedrepuestos.Where(x => x.pedido_id == pedido).ToList();
            int instalado = 0;
            int enproceso = 0;
            //parametro tipo de orden de taller accesorios (razon de ingreso accesorios)
            icb_sysparameter acce1 = db.icb_sysparameter.Where(d => d.syspar_cod == "P116").FirstOrDefault();
            int accesori = acce1 != null ? Convert.ToInt32(acce1.syspar_value) : 6;

            //veo si el plan_mayor tiene orden de taller en accesorios
            tencabezaorden ordenaccesorios = db.tencabezaorden
                .Where(d => d.razoningreso == accesori && d.estado && d.placa == planmayor)
                .OrderByDescending(x => x.fecha).FirstOrDefault();


            string resultado =
                "<div class='col-xs-12 col-sm-12 col-md-12 col-lg-12' style='white-space:nowrap;width:100%;heigth:100%'>";
            resultado = resultado + "<div class='col-xs-3 col-sm-3 col-md-3 col-lg-3' style='display:inline-block;'>";
            string clase = "btn-sm ";
            string fechax = "";
            string verOT = "";
            if (ordenaccesorios != null)
            {
                if (ordenaccesorios.fecha_fin_operacion == null)
                {
                    clase = clase + "btn-warning";
                    verOT = "estadoOT('" + planmayor + "')";
                    string fechax_inicio =
                        ordenaccesorios.fec_creacion.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
                    fechax = fechax +
                             "<span class='text-warning' style='font-size:9px;><i class='text-warning fa fa-check-circle'></i>" +
                             fechax_inicio + "</span>";
                    enproceso = 1;
                }
            }

            if (insaccesorios.Count == 0)
            {
                clase = clase + "btn-success";
            }
            else if (insaccesorios.Count > 0)
            {
                for (int i = 0; i < insaccesorios.Count; i++)
                {
                    if (insaccesorios[i].instalado == false)
                    {
                        instalado = 0;
                        break;
                    }
                    else
                    {
                        instalado = 1;
                    }
                }

                if (instalado == 0 && enproceso != 1)
                {
                    clase = clase + "btn-danger";
                }
                else if (ordenaccesorios != null)
                {
                    if (instalado == 1 && ordenaccesorios.fecha_fin_operacion != null)
                    {
                        string fechax_inicio =
                            ordenaccesorios.fec_creacion.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
                        fechax = fechax +
                                 "<span class='text-warning' style='font-size:9px;'><i class='text-warning fa fa-check-circle'></i>" +
                                 fechax_inicio + "</span>";
                        string fechax_fin =
                            ordenaccesorios.fecha_fin_operacion.Value.ToString("yyyy/MM/dd HH:mm",
                                new CultureInfo("en-US"));
                        fechax = fechax +
                                 "<br/><span class='text-success' style='font-size:9px;'><i class='text-success fa fa-check-circle'></i>" +
                                 fechax_fin + "</span>";
                        verOT = "estadoOT('" + planmayor + "')";
                        clase = clase + "btn-success";
                    }
                }
            }

            string icono = "<button type='button' class='" + clase + "' onclick=" + verOT + "><i class='fa fa-sliders'></i>";
            //busco si el vehículo ha tenido alistamiento de embellecimiento
            resultado = resultado + icono + "</div>";
            resultado = resultado + "<div class='col-xs-9 col-sm-9 col-md-9 col-lg-9' style='display:inline-block;'>";
            resultado = resultado + fechax;
            resultado = resultado + "</div>";
            resultado = resultado + "</div>";

            return resultado;
        }

        public string verificarAlistamientoAcc(string planmayor)
        {
            int pedido = db.vpedido.Where(x => x.planmayor == planmayor).Select(z => z.id).FirstOrDefault();
            List<vpedrepuestos> insaccesorios = db.vpedrepuestos.Where(x => x.pedido_id == pedido).ToList();
            int instalado = 0;
            //parametro tipo de orden de taller accesorios (razon de ingreso accesorios)
            icb_sysparameter acce1 = db.icb_sysparameter.Where(d => d.syspar_cod == "P116").FirstOrDefault();
            int accesori = acce1 != null ? Convert.ToInt32(acce1.syspar_value) : 6;

            //veo si el plan_mayor tiene orden de taller en accesorios
            tencabezaorden ordenaccesorios = db.tencabezaorden
                .Where(d => d.razoningreso == accesori && d.estado && d.placa == planmayor)
                .OrderByDescending(x => x.fecha).FirstOrDefault();


            int resultado = 0;
            if (insaccesorios.Count == 0)
            {
                resultado = 1;
            }
            else if (insaccesorios.Count > 0)
            {
                for (int i = 0; i < insaccesorios.Count; i++)
                {
                    if (insaccesorios[i].instalado == false)
                    {
                        instalado = 0;
                        break;
                    }
                    else
                    {
                        instalado = 1;
                    }
                }

                if (instalado == 0)
                {
                    resultado = 0;
                }
                else if (ordenaccesorios != null)
                {
                    if (instalado == 1 && ordenaccesorios.fecha_fin_operacion != null)
                    {
                        resultado = 1;
                    }
                }
            }
            return resultado.ToString();
        }

        public string verificarAlistamientoMec(string planmayor, DateTime? fecha_fin)
        {
            string resultado = "0";
            if (fecha_fin == null)
            {
                resultado = "0";
            }
            else
            {
                resultado = "1";
            }
            return resultado;
        }

        public JsonResult buscarOTPedido(string planmayor)
        {
            trazonesingreso razoningreso = db.trazonesingreso.Where(x => x.razoz_ingreso.Contains("accesorio")).FirstOrDefault();
            if (razoningreso != null)
            {
                List<tencabezaorden> ot = db.tencabezaorden.Where(x => x.placa == planmayor && x.razoningreso == razoningreso.id).ToList();
                if (ot != null)
                {
                    var data = ot.Select(x => new
                    {
                        responsable = db.users.Where(s => s.user_id == x.idtecnico).Select(p => new { nombre = p.user_nombre + " " + p.user_apellido }).FirstOrDefault(),
                        fecha = x.fecha != null ? x.fecha.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")) : "",
                        estado = x.estadoorden != null ? x.tcitasestados.Descripcion : ""
                    });

                    return Json(new { resultado = 1, data }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { resultado = 0, mensaje = "El pedido no tiene una orden de taller creada de tipo accesorios." }, JsonRequestBehavior.AllowGet);
        }

        public void operacionAlistamiento()
        {
            string sIdOperacion = db.icb_sysparameter.Where(x => x.syspar_cod.Contains("P81")).Select(x => x.syspar_value)
                .FirstOrDefault();
            iIdOperacion = Convert.ToInt32(sIdOperacion); // id Operacion Alistamiento
            string sCreateOt = db.icb_sysparameter.Where(x => x.syspar_cod.Contains("P78")).Select(x => x.syspar_value)
                .FirstOrDefault();
            iCreateOt = Convert.ToInt32(sCreateOt); // id estado creacion Ot
            string sFinishOt = db.icb_sysparameter.Where(x => x.syspar_cod.Contains("P89")).Select(x => x.syspar_value)
                .FirstOrDefault();
            iFinishOt = Convert.ToInt16(sFinishOt); // id estado finalizacion Ot TEMPORAL
            string sExecutionOt = db.icb_sysparameter.Where(x => x.syspar_cod.Contains("P84")).Select(x => x.syspar_value)
                .FirstOrDefault();
            iExecutionOt = Convert.ToInt16(sExecutionOt); // id estado ejecucion Ot TEMPORAL
            string tpbahia_alis = db.icb_sysparameter.Where(x => x.syspar_cod.Contains("P82")).Select(x => x.syspar_value)
                .FirstOrDefault();
            idtpbahia_alis = Convert.ToInt32(tpbahia_alis); // id bahia alistamiento
            bahia = db.tbahias.Where(x => x.bodega == bodegaActual && x.tipo_bahia == idtpbahia_alis).Select(x => x.id)
                .FirstOrDefault(); // Id bahia alistamiento
            string sEnvAlis = db.icb_sysparameter.Where(x => x.syspar_cod == "P85").Select(x => x.syspar_value)
                .FirstOrDefault(); // Envio Alistamiento
            iEnvAlis = Convert.ToInt32(sEnvAlis); // Envio Alistamiento
            sFinAlis = db.icb_sysparameter.Where(x => x.syspar_cod == "P86").Select(x => x.syspar_value)
                .FirstOrDefault(); // Fin Alistamiento
            iFinAlis = Convert.ToInt32(sFinAlis); // Fin Alistamiento
            string sEnvRAlis = db.icb_sysparameter.Where(x => x.syspar_cod == "P92").Select(x => x.syspar_value)
                .FirstOrDefault(); // Envio Alistamiento
            iEnvRAlis = Convert.ToInt32(sEnvRAlis); // Envio Realistamiento
            string sFinRAlis = db.icb_sysparameter.Where(x => x.syspar_cod == "P93").Select(x => x.syspar_value)
                .FirstOrDefault(); // Fin Alistamiento
            iFinRAlis = Convert.ToInt32(sFinRAlis); // Fin Realistamiento
        }

        public void variablesAlistamiento(icb_consecutivo_ot buscatCodigo)
        {
            icb_sysparameter tipoorden = db.icb_sysparameter.Where(d => d.syspar_cod == "P76").FirstOrDefault();
            int tipodocorden = tipoorden != null ? Convert.ToInt32(tipoorden.syspar_value) : 26;
            // Tipo Doc
            idDocOtBodega = (from consecutivos in db.icb_doc_consecutivos
                             join bodega in db.bodega_concesionario
                                 on consecutivos.doccons_bodega equals bodega.id
                             join tipoDocumento in db.tp_doc_registros
                                 on consecutivos.doccons_idtpdoc equals tipoDocumento.tpdoc_id
                             where consecutivos.doccons_bodega == bodegaActual && tipoDocumento.tipo == tipodocorden
                             select tipoDocumento.tpdoc_id).FirstOrDefault();
            // Consecutivo
            string anio = DateTime.Now.Year.ToString();
            string consecutivonum = cerosconsecutivo(buscatCodigo.otcon_consecutivo.Value);
            codigoIOT = anio.Substring(anio.Length - 2) + buscatCodigo.otcon_prefijo + "-" + consecutivonum;
            operacionAlistamiento();
        }


        public ActionResult modalAlistamientoExhibicion(string idn)
        {
            operacionAlistamiento();
            var x = (from vh in db.icb_vehiculo
                     join mvh in db.modelo_vehiculo
                         on vh.modvh_id equals mvh.modvh_codigo
                     join cvh in db.color_vehiculo
                         on vh.colvh_id equals cvh.colvh_id into a
                     from color in a.DefaultIfEmpty()
                     join ub in db.ubicacion_bodega
                         on vh.ubicacionactual equals ub.id into b
                     from ubica in b.DefaultIfEmpty()
                     where vh.plan_mayor == idn
                     select new
                     {
                         modelo = mvh.modvh_nombre,
                         vh.vin,
                         placa = vh.plac_vh,
                         anio = vh.anio_vh,
                         color = color.colvh_nombre,
                         vh.plan_mayor,
                         ubicacion_id = vh.ubicacionactual,
                         ubicacion = ubica.descripcion
                     }).FirstOrDefault();
            agendaAlistamientoModel alist = new agendaAlistamientoModel
            {
                idpedido = 0,
                modeloVh = x.modelo,
                serieVh = x.vin,
                placaVh = x.placa,
                anioModeloVh = x.anio,
                colorVh = x.color,
                planMayorVh = x.plan_mayor,
                ubivh_id = x.ubicacion_id,
                ubicacionVh = x.ubicacion
            };
            //alist.carroceriaVh = x.tipo_carroceria != null ? true : false;
            //alist.fcEnvioCarrocVh = x.fec_carroceria_envio != null ? x.fec_carroceria_envio.Value.ToString("yyyy/MM/dd") : "";
            //alist.fcLlegadaCarrocVh = x.fec_carroceria_llegada != null ? x.fec_carroceria_llegada.Value.ToString("yyyy/MM/dd") : "";

            bahia_alistamiento_exh bhAls = db.bahia_alistamiento_exh.Where(t =>
                    t.icb_vehiculo.plan_mayor == idn && (t.tencabezaorden_exh.tcitasestados.id == iCreateOt ||
                                                         t.tencabezaorden_exh.tcitasestados.id == iExecutionOt))
                .FirstOrDefault();
            alist.estadoAlis = false;
            if (bhAls != null)
            {
                alist.id = bhAls.bahia_als_id;
                alist.estadoAlis = true;
                alist.fcPreentregaVh =
                    bhAls.bahia_als_fecha != null ? bhAls.bahia_als_fecha.Value.ToString("yyyy/MM/dd") : "";
                alist.motivo = bhAls.tp_movimiento;
                alist.bodegaListVh = bodegas();
            }

            ViewBag.iCreateOt = iCreateOt;
            ViewBag.iExecutionOt = iExecutionOt;
            ViewBag.estadoVh = db.bahia_alistamiento_exh
                .Where(t => t.icb_vehiculo.plan_mayor == idn && (t.tencabezaorden_exh.tcitasestados.id == iCreateOt ||
                                                                 t.tencabezaorden_exh.tcitasestados.id == iExecutionOt))
                .Select(t => t.tencabezaorden_exh.tcitasestados.id).FirstOrDefault();
            //parametro de sistema razon de ingreso 
            icb_sysparameter param1 = db.icb_sysparameter.Where(d => d.syspar_cod == "P116").FirstOrDefault();
            int accesorios = param1 != null ? Convert.ToInt32(param1.syspar_value) : 6;
            //verifico si tiene alistamiento de accesorios
            tencabezaorden_exh otAccesorios = db.tencabezaorden_exh
                .Where(d => d.icb_vehiculo.plan_mayor == x.plan_mayor && d.trazonesingreso.id == accesorios)
                .FirstOrDefault();
            //verifico si vehiculo está en algún pedido
            vpedido existepedido = db.vpedido.Where(d => d.planmayor == x.plan_mayor).FirstOrDefault();

            int agendaralistamiento = 0;
            int mostraraccesorios = 0;
            int accesoriosAlistamiento = 0;
            //busco si el vehículo viene de pedido (es este caso) 

            if (otAccesorios == null)
            {
                agendaralistamiento = 1;
            }
            else
            {
                accesoriosAlistamiento = 1;
            }
            //if (existepedido != null)
            //{
            //	//me traigo los accesorios de ese pedido
            //	var acce = db.vpedrepuestos
            //		.Where(d => d.pedido_id == existepedido.id && d.cantidad != null && d.instalado == false).ToList();
            //	var listaaccesorios = acce.Select(d => new accesoriospedido
            //	{
            //		cantidad = d.cantidad != null ? d.cantidad.Value.ToString("N0", new CultureInfo("is-IS")) : "0",
            //		codigo = d.icb_referencia.ref_codigo,
            //		nombre = d.icb_referencia.ref_descripcion,
            //		porcentaje_descuento = "0",
            //		porcentaje_iva = "0",
            //		precio_unitario = d.vrunitario != null
            //			? d.vrunitario.Value.ToString("N0", new CultureInfo("is-IS"))
            //			: "0",
            //		total_repuesto = d.vrtotal != null ? d.vrtotal.Value.ToString("N0", new CultureInfo("is-IS")) : "0",
            //		valor_descuento = "0",
            //		valor_iva = "0"
            //	}).ToList();
            //	mostraraccesorios = 1;
            //	ViewBag.listaaccesorios = listaaccesorios;
            //}

            ViewBag.agendaralistamiento = agendaralistamiento;
            ViewBag.mostraraccesorios = mostraraccesorios;
            ViewBag.accesoriosAlistamiento = accesoriosAlistamiento;


            return PartialView("formAlistamiento", alist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult modalAlistamientoExhibicion(agendaAlistamientoModel modAls)
        {
            bool resl = false;
            if (ModelState.IsValid)
            {
                int id_usuario_actual = Convert.ToInt32(Session["user_usuarioid"]);
                bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                //ahora quiero ver si hay un código de orden de taller para esa bodega
                icb_consecutivo_ot buscatCodigo = db.icb_consecutivo_ot.Where(x => x.otcon_bodega == bodegaActual).FirstOrDefault();
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

                                bahia_alistamiento_exh bhAlis =
                                    db.bahia_alistamiento_exh.FirstOrDefault    (x => x.bahia_als_id == modAls.id);
                                bhAlis.tp_movimiento = Convert.ToInt16(modAls.motivo);
                                bhAlis.bahia_als_fecha = DateTime.Parse(modAls.fcPreentregaVh,
                                    CultureInfo.CreateSpecificCulture("en-US"));
                                bhAlis.users.user_id = id_usuario_actual;
                                bhAlis.bahia_fechamod = DateTime.Now;
                                db.Entry(bhAlis).State = EntityState.Modified;
                                resp_bah = db.SaveChanges();
                                // Actualizar evento si existe
                                // Validar si evento es realistaminto o alistamiento
                                icb_vehiculo_eventos icbvh_even = db.icb_vehiculo_eventos.Where(x =>
                                        x.planmayor.Contains(modAls.planMayorVh) && x.id_tpevento == iEnvRAlis)
                                    .FirstOrDefault();
                                if (icbvh_even == null)
                                {
                                    icbvh_even = db.icb_vehiculo_eventos.Where(x =>
                                            x.planmayor.Contains(modAls.planMayorVh) && x.id_tpevento == iEnvAlis)
                                        .FirstOrDefault();
                                }

                                if (icbvh_even != null)
                                {
                                    // Validar si existe Evento
                                    icbvh_even.fechaevento = DateTime.Parse(modAls.fcPreentregaVh,
                                        CultureInfo.CreateSpecificCulture("en-US"));
                                    icbvh_even.eventofec_actualizacion = DateTime.Now;
                                    icbvh_even.eventouserid_actualizacion = id_usuario_actual;
                                    db.Entry(icbvh_even).State = EntityState.Modified;
                                    resp_even = db.SaveChanges();
                                }
                            }
                            else
                            {
                                int idn_exist = db.bahia_alistamiento_exh
                                    .Where(x => x.icb_vehiculo.plan_mayor == modAls.planMayorVh &&
                                                (x.tencabezaorden_exh.tcitasestados.id == iCreateOt ||
                                                 x.tencabezaorden_exh.tcitasestados.id == iExecutionOt))
                                    .Select(x => x.bahia_als_id)
                                    .FirstOrDefault();
                                if (idn_exist == 0)
                                {
                                    tencabezaorden_exh nuevaOT = new tencabezaorden_exh
                                    {
                                        numero = 1,
                                        idtipodoc = idDocOtBodega,
                                        tipooperacion = iIdOperacion,
                                        codigoentrada = codigoIOT,
                                        bodega = bodegaActual,
                                        planmayor = modAls.planMayorVh,
                                        fecha = DateTime.Now,
                                        fec_creacion = DateTime.Now,
                                        userid_creacion = id_usuario_actual,
                                        entrega = DateTime.Parse(modAls.fcPreentregaVh,
                                            CultureInfo.CreateSpecificCulture("en-US")),
                                        kilometraje = kilometraje,
                                        razoningreso = razoning,
                                        estado = true,
                                        estadoorden = iCreateOt
                                    };

                                    db.tencabezaorden_exh.Add(nuevaOT);
                                    resp_ot = db.SaveChanges();
                                    int ot_id = nuevaOT.id;
                                    // Si tiene fecha de envio carroceria se el actualiza en la tabla
                                    resp_pedi = updFecCarroceria(modAls);
                                    //le aumento el consecutivo en 1 al consecutivo de orden
                                    buscatCodigo.otcon_consecutivo = buscatCodigo.otcon_consecutivo + 1;
                                    db.Entry(buscatCodigo).State = EntityState.Modified;
                                    resp_consec = db.SaveChanges();
                                    // Tabla que asocia el alistamiento y la ot. 
                                    bahia_alistamiento_exh bhAlis = new bahia_alistamiento_exh
                                    {
                                        bahia_id = bahia, // Bahia 
                                        ot_id = ot_id,
                                        tp_movimiento = Convert.ToInt16(modAls.motivo),
                                        plan_mayor = modAls.planMayorVh,
                                        bahia_als_fecha = DateTime.Parse(modAls.fcPreentregaVh,
                                            CultureInfo.CreateSpecificCulture("en-US")),
                                        bahia_usercre = id_usuario_actual,
                                        bahia_fechacre = DateTime.Now
                                    };

                                    db.bahia_alistamiento_exh.Add(bhAlis);
                                    resp_bah = db.SaveChanges();
                                    // Adicionar evento de alistamiento  
                                    icb_vehiculo_eventos icbvh_even = db.icb_vehiculo_eventos.Where(x =>
                                            x.planmayor.Contains(modAls.planMayorVh) && x.id_tpevento == iEnvAlis)
                                        .FirstOrDefault();
                                    if (icbvh_even != null)
                                    {
                                        // Validar si existe alistamiento.
                                        // Validar si existe un evento de realistamiento
                                        icb_vehiculo_eventos icbvh_even_r = db.icb_vehiculo_eventos.Where(x =>
                                                x.planmayor.Contains(modAls.planMayorVh) && x.id_tpevento == iEnvRAlis)
                                            .FirstOrDefault(); //Validar si existe realistamiento o crearlo
                                        if (icbvh_even_r != null)
                                        {
                                            // Re-iniciar los valores
                                            icbvh_even.fechaevento = DateTime.Parse(modAls.fcPreentregaVh,
                                                CultureInfo.CreateSpecificCulture("en-US"));
                                            icbvh_even.eventofec_actualizacion = DateTime.Now;
                                            icbvh_even.eventouserid_actualizacion = id_usuario_actual;
                                            db.Entry(icbvh_even).State = EntityState.Modified;
                                            resp_even = db.SaveChanges();
                                            icb_vehiculo_eventos icbvh_evenFin = db.icb_vehiculo_eventos.Where(x =>
                                                    x.planmayor.Contains(modAls.planMayorVh) &&
                                                    x.id_tpevento == iFinRAlis)
                                                .FirstOrDefault();
                                            if (icbvh_evenFin != null)
                                            {
                                                db.icb_vehiculo_eventos.Remove(icbvh_evenFin);
                                                db.SaveChanges();
                                            }
                                        }
                                        else
                                        {
                                            // Crear evento re-alistamiento
                                            addEvento(modAls, id_usuario_actual, iEnvRAlis);
                                        }
                                    }
                                    else
                                    {
                                        // Crear Evento Alistamiento
                                        addEvento(modAls, id_usuario_actual, iEnvAlis);
                                    }
                                }
                            }

                            dbTrans.Commit();
                            resl = true;
                        }
                        catch (Exception ex)
                        {
                            Exception exp = ex;
                            resl = false;
                            dbTrans.Rollback();
                        }
                    }
                }

                return Json(new { resp_ot, resp_consec, resp_bah, resl }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { resl, resp = "Valide los datos ingresados" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult modalAlistamiento(int idn)
        {
            operacionAlistamiento();
            vw_pendientesEntrega x = db.vw_pendientesEntrega.FirstOrDefault(t => t.id == idn);
            agendaAlistamientoModel alist = new agendaAlistamientoModel
            {
                idpedido = x.id,
                modeloVh = x.modelo,
                serieVh = x.vin,
                placaVh = x.plac_vh,
                anioModeloVh = x.anio_vh != null ? x.anio_vh : 0,
                colorVh = x.colvh_nombre,
                planMayorVh = x.planmayor,
                asesor = x.asesor,
                cedulaVh = x.doc_tercero,
                clienteIdVh = x.idCliente,
                clienteVh = x.cliente,
                ubivh_id = x.ubivh_id,
                ubicacionVh = x.ubivh_nombre,
                carroceriaVh = x.tipo_carroceria != null ? true : false,
                fcEnvioCarrocVh =
                    x.fec_carroceria_envio != null ? x.fec_carroceria_envio.Value.ToString("yyyy/MM/dd") : "",
                fcLlegadaCarrocVh = x.fec_carroceria_llegada != null
                    ? x.fec_carroceria_llegada.Value.ToString("yyyy/MM/dd")
                    : ""
            };

            //alist.fcEnvioCarrocVh = (alist.fcEnvioCarrocVh != "") ? alist.fcEnvioCarrocVh.ToString("dd/mm/yyyy") : "",
            //alist.fcLlegadaCarrocVh = (alist.fcLlegadaCarrocVh != "") ? alist.fcLlegadaCarrocVh.ToString("dd/mm/yyyy") : ""

            ///string sCreateOt = db.icb_sysparameter.Where(t => t.syspar_cod.Contains("P78")).Select(t => t.syspar_value).FirstOrDefault();
            ///int iCreateOt = Convert.ToInt32(sCreateOt);
            icb_bahia_alistamiento bhAls = db.icb_bahia_alistamiento.Where(t =>
                t.id_pedido == idn && (t.tencabezaorden.estadoorden == iCreateOt ||
                                       t.tencabezaorden.estadoorden == iExecutionOt)).FirstOrDefault();
            alist.estadoAlis = false;
            if (bhAls != null)
            {
                alist.id = bhAls.bh_als_id;
                alist.estadoAlis = true;
                alist.fcPreentregaVh =
                    bhAls.bh_als_fecha != null ? bhAls.bh_als_fecha.Value.ToString("yyyy/MM/dd") : "";
                alist.motivo = bhAls.tp_movimiento;
                alist.bodegaListVh = bodegas();
            }

            ViewBag.iCreateOt = iCreateOt;
            ViewBag.iExecutionOt = iExecutionOt;
            ViewBag.estadoVh = db.icb_bahia_alistamiento
                .Where(t => t.id_pedido == idn && (t.tencabezaorden.estadoorden == iCreateOt ||
                                                   t.tencabezaorden.estadoorden == iExecutionOt))
                .Select(t => t.tencabezaorden.estadoorden).FirstOrDefault();
            //parametro de sistema razon de ingreso 
            icb_sysparameter param1 = db.icb_sysparameter.Where(d => d.syspar_cod == "P116").FirstOrDefault();
            int accesorios = param1 != null ? Convert.ToInt32(param1.syspar_value) : 6;
            //verifico si tiene alistamiento de accesorios
            tencabezaorden otAccesorios = db.tencabezaorden
                .Where(d => d.placa == x.planmayor && d.razoningreso == accesorios && d.idpedido != null)
                .FirstOrDefault();
            //verifico si vehiculo está en algún pedido
            vpedido existepedido = db.vpedido.Where(d => d.planmayor == x.planmayor).FirstOrDefault();

            int agendaralistamiento = 0;
            int mostraraccesorios = 0;
            int accesoriosAlistamiento = 0;
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
                List<vpedrepuestos> acce = db.vpedrepuestos
                    .Where(d => d.pedido_id == existepedido.id && d.cantidad != null && d.instalado == false).ToList();
                List<accesoriospedido> listaaccesorios = acce.Select(d => new accesoriospedido
                {
                    cantidad = d.cantidad != null ? d.cantidad.Value.ToString("N0", new CultureInfo("is-IS")) : "0",
                    codigo = d.icb_referencia.ref_codigo,
                    nombre = d.icb_referencia.ref_descripcion,
                    porcentaje_descuento = "0",
                    porcentaje_iva = "0",
                    precio_unitario = d.vrunitario != null
                        ? d.vrunitario.Value.ToString("N0", new CultureInfo("is-IS"))
                        : "0",
                    total_repuesto = d.vrtotal != null ? d.vrtotal.Value.ToString("N0", new CultureInfo("is-IS")) : "0",
                    valor_descuento = "0",
                    valor_iva = "0"
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
            icb_tpeventos buscarEvento =
                db.icb_tpeventos.FirstOrDefault(x => x.tpevento_id == idevent); // Alistamiento - Realistamiento
            int resp_even = 0;
            icb_vehiculo_eventos crearEvento = new icb_vehiculo_eventos();
            int? propietario_vh = db.icb_vehiculo.Find(modAls.planMayorVh).propietario;
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
            int resp_pedi = 0;
            if (modAls.fcEnvioCarrocVh != null && modAls.fcLlegadaCarrocVh != null)
            {
                vpedido tPedido = db.vpedido.FirstOrDefault(x => x.id == modAls.idpedido);
                tPedido.fec_carroceria_envio =
                    DateTime.Parse(modAls.fcEnvioCarrocVh, CultureInfo.CreateSpecificCulture("en-US"));
                tPedido.fec_carroceria_llegada =
                    DateTime.Parse(modAls.fcLlegadaCarrocVh, CultureInfo.CreateSpecificCulture("en-US"));
                db.Entry(tPedido).State = EntityState.Modified;
                resp_pedi = db.SaveChanges();
            }

            return resp_pedi;
        }

        //Creando un objeto de json Result
        [HttpPost]
        public JsonResult checkCarExhibition(string id)
        {
            icb_sysparameter parameter = db.icb_sysparameter.Where(d => d.syspar_cod == "P116").FirstOrDefault();
            int razoningreso = parameter != null ? Convert.ToInt32(parameter.syspar_value) : 6;
            tencabezaorden existe = db.tencabezaorden.Where(o => o.placa == id && o.razoningreso == razoningreso).FirstOrDefault();
            if (existe != null)
            {
                //veo si tiene repuestos
                List<tsolicitudrepuestosot> repsolicitados = db.tsolicitudrepuestosot.Where(d => d.idorden == existe.id && d.recibido).ToList();
                if (repsolicitados.Count > 0)
                {
                    var listasolicitados = repsolicitados.Select(d => new
                    {
                        id = d.tdetallerepuestosot.idrepuesto,
                        nombre = d.tdetallerepuestosot.icb_referencia.ref_descripcion,
                        valor_unitario = d.tdetallerepuestosot.valorunitario,
                        valor_unitario_string =
                            d.tdetallerepuestosot.valorunitario.ToString("N0", new CultureInfo("is-IS")),

                        cantidad = d.canttraslado,
                        iva = d.tdetallerepuestosot.poriva,
                        desc = d.tdetallerepuestosot.pordescto,
                        tipo_tarifa = d.tdetallerepuestosot.tipotarifa,
                        d.tdetallerepuestosot.centro_costo
                    }).ToList();
                }
            }

            return Json(existe);
        }

        [HttpPost]
        public JsonResult debitNote(int nit, DateTime? desde, DateTime? hasta, int? id_documento, int? factura)
        {
            if (desde == null)
            {
                desde = db.encab_documento.OrderBy(x => x.fecha).Select(x => x.fecha).FirstOrDefault();
            }

            if (hasta == null)
            {
                hasta = db.encab_documento.OrderByDescending(x => x.fecha).Select(x => x.fecha).FirstOrDefault();
            }
            else
            {
                hasta = hasta.GetValueOrDefault();
            }

            List<int> listaDocumentos = new List<int>();
            if (id_documento != null)
            {
                listaDocumentos.Add(id_documento ?? 0);
            }
            else
            {
                listaDocumentos = db.tp_doc_registros.Where(x => x.sw == 3).Select(x => x.tpdoc_id).ToList();
            }

            if (factura != null)
            {
                var buscarFacturasConSaldo = (from e in db.encab_documento
                                              join t in db.tp_doc_registros
                                                  on e.tipo equals t.tpdoc_id
                                              join tp in db.tp_doc_registros_tipo
                                                  on t.tipo equals tp.id
                                              where listaDocumentos.Contains(t.tpdoc_id)
                                                    && e.valor_total - e.valor_aplicado != 0
                                                    && e.fecha >= desde && e.fecha <= hasta
                                                    && e.numero == factura
                                                    && e.nit == nit
                                              select new
                                              {
                                                  id = e.idencabezado,
                                                  fecha = e.fecha.ToString(),
                                                  valor_aplicado = e.valor_aplicado,
                                                  e.valor_total,
                                                  e.numero,
                                                  vencimiento = e.vencimiento.Value.ToString(),
                                                  tipo = "(" + t.prefijo + ") " + t.tpdoc_nombre,
                                                  saldo = e.valor_total - e.valor_aplicado,
                                                  retencion = e.porcen_retencion,
                                                  reteIca = e.porcen_retica,
                                                  reteIva = e.porcen_reteiva,
                                                  tipodoc = t.tpdoc_id,
                                                  descripcion = t.tpdoc_nombre,
                                                  numeroFactura = e.numero,
                                                  prefijo = e.idencabezado,
                                                  tp = e.tipo
                                              }).ToList();

                var data = buscarFacturasConSaldo.Select(x => new
                {
                    x.id,
                    x.fecha,
                    x.valor_aplicado,
                    x.valor_total,
                    x.numero,
                    x.vencimiento,
                    x.tipo,
                    x.saldo,
                    x.retencion,
                    x.reteIca,
                    x.reteIva,
                    x.tipodoc,
                    x.descripcion,
                    x.numeroFactura,
                    x.prefijo,
                    x.tp
                });

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var buscarFacturasConSaldo = (from e in db.encab_documento
                                              join t in db.tp_doc_registros
                                                  on e.tipo equals t.tpdoc_id
                                              join tp in db.tp_doc_registros_tipo
                                                  on t.tipo equals tp.id
                                              where listaDocumentos.Contains(t.tpdoc_id)
                                                    && e.valor_total - e.valor_aplicado != 0
                                                    && e.fecha >= desde && e.fecha <= hasta
                                                    && e.nit == nit
                                              select new
                                              {
                                                  id = e.idencabezado,
                                                  fecha = e.fecha.ToString(),
                                                  valor_aplicado = e.valor_aplicado,
                                                  e.valor_total,
                                                  e.numero,
                                                  vencimiento = e.vencimiento.Value.ToString(),
                                                  tipo = "(" + t.prefijo + ") " + t.tpdoc_nombre,
                                                  //saldo = e.valor_total - (e.valor_aplicado != null ? e.valor_aplicado : 0),
                                                  saldo = e.valor_total -e.valor_aplicado,

                                                  retencion = e.porcen_retencion,
                                                  reteIca = e.porcen_retica,
                                                  reteIva = e.porcen_reteiva,
                                                  tipodoc = t.tpdoc_id,
                                                  descripcion = t.tpdoc_nombre,
                                                  numeroFactura = e.numero,
                                                  t.prefijo,
                                                  tp = e.tipo
                                              }).ToList();

                var data = buscarFacturasConSaldo.Select(x => new
                {
                    x.id,
                    x.fecha,
                    x.valor_aplicado,
                    x.valor_total,
                    x.numero,
                    x.vencimiento,
                    x.tipo,
                    x.saldo,
                    x.retencion,
                    x.reteIca,
                    x.reteIva,
                    x.tipodoc,
                    x.descripcion,
                    x.numeroFactura,
                    x.prefijo,
                    x.tp
                });

                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult BuscarBodegasPorDocumento(int? id)
        {
            var buscarBodega = (from consecutivos in db.icb_doc_consecutivos
                                join bodega in db.bodega_concesionario
                                    on consecutivos.doccons_bodega equals bodega.id
                                where consecutivos.doccons_idtpdoc == id
                                select new
                                {
                                    bodega.bodccs_nombre,
                                    bodega.id
                                }).Distinct().OrderBy(bn => bn.bodccs_nombre).ToList();

            var buscarConceptos1 = (from concepto1 in db.tpdocconceptos
                                    where concepto1.tipodocid == id
                                    select new
                                    {
                                        concepto1.id,
                                        concepto1.Descripcion
                                    }).ToList();

            var buscarConceptos2 = (from concepto2 in db.tpdocconceptos2
                                    where concepto2.tipodocid == id
                                    select new
                                    {
                                        concepto2.id,
                                        concepto2.Descripcion
                                    }).ToList();

            var buscarPerfilContable = db.perfil_contable_documento.Where(x => x.tipo == id).Select(x => new
            {
                value = x.id,
                text = x.descripcion
            }).ToList();

            var data = new
            {
                buscarBodega,
                buscarConceptos1,
                buscarConceptos2,
                buscarPerfilContable
            };

            return Json(data, JsonRequestBehavior.AllowGet);
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
                icb_consecutivo_ot buscatCodigo = db.icb_consecutivo_ot.Where(x => x.otcon_bodega == bodegaActual).FirstOrDefault();
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

                                icb_bahia_alistamiento bhAlis = db.icb_bahia_alistamiento.FirstOrDefault(x => x.bh_als_id == modAls.id);
                                bhAlis.tp_movimiento = Convert.ToInt16(modAls.motivo);
                                bhAlis.bh_als_fecha = DateTime.Parse(modAls.fcPreentregaVh,
                                    CultureInfo.CreateSpecificCulture("en-US"));
                                bhAlis.bh_als_usumod = id_usuario_actual;
                                bhAlis.bh_als_fecmod = DateTime.Now;
                                db.Entry(bhAlis).State = EntityState.Modified;
                                resp_bah = db.SaveChanges();
                                // Actualizar evento si existe
                                // Validar si evento es realistaminto o alistamiento
                                icb_vehiculo_eventos icbvh_even = db.icb_vehiculo_eventos.Where(x =>
                                        x.planmayor.Contains(modAls.planMayorVh) && x.id_tpevento == iEnvRAlis)
                                    .FirstOrDefault();
                                if (icbvh_even == null)
                                {
                                    icbvh_even = db.icb_vehiculo_eventos.Where(x =>
                                            x.planmayor.Contains(modAls.planMayorVh) && x.id_tpevento == iEnvAlis)
                                        .FirstOrDefault();
                                }

                                if (icbvh_even != null)
                                {
                                    // Validar si existe Evento
                                    icbvh_even.fechaevento = DateTime.Parse(modAls.fcPreentregaVh,
                                        CultureInfo.CreateSpecificCulture("en-US"));
                                    icbvh_even.eventofec_actualizacion = DateTime.Now;
                                    icbvh_even.eventouserid_actualizacion = id_usuario_actual;
                                    db.Entry(icbvh_even).State = EntityState.Modified;
                                    resp_even = db.SaveChanges();
                                }
                            }
                            else
                            {
                                int? idn_exist = db.icb_bahia_alistamiento
                                    .Where(x => x.id_pedido == modAls.idpedido &&
                                                (x.tencabezaorden.estadoorden == iCreateOt ||
                                                 x.tencabezaorden.estadoorden == iExecutionOt)).Select(x => x.id_pedido)
                                    .FirstOrDefault();
                                if (idn_exist == null)
                                {
                                    tencabezaorden nuevaOT = new tencabezaorden
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
                                        entrega = DateTime.Parse(modAls.fcPreentregaVh,
                                            CultureInfo.CreateSpecificCulture("en-US")),
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
                                        bh_als_fecha = DateTime.Parse(modAls.fcPreentregaVh,
                                            CultureInfo.CreateSpecificCulture("en-US")),
                                        bh_als_usuela = id_usuario_actual,
                                        bh_als_fecela = DateTime.Now
                                    };
                                    db.icb_bahia_alistamiento.Add(bhAlis);
                                    resp_bah = db.SaveChanges();
                                    // Adicionar evento de alistamiento  
                                    icb_vehiculo_eventos icbvh_even = db.icb_vehiculo_eventos.Where(x =>
                                            x.planmayor.Contains(modAls.planMayorVh) && x.id_tpevento == iEnvAlis)
                                        .FirstOrDefault();
                                    if (icbvh_even != null)
                                    {
                                        // Validar si existe alistamiento.
                                        // Validar si existe un evento de realistamiento
                                        icb_vehiculo_eventos icbvh_even_r = db.icb_vehiculo_eventos.Where(x =>
                                                x.planmayor.Contains(modAls.planMayorVh) && x.id_tpevento == iEnvRAlis)
                                            .FirstOrDefault(); //Validar si existe realistamiento o crearlo
                                        if (icbvh_even_r != null)
                                        {
                                            // Re-iniciar los valores
                                            icbvh_even.fechaevento = DateTime.Parse(modAls.fcPreentregaVh,
                                                CultureInfo.CreateSpecificCulture("en-US"));
                                            icbvh_even.eventofec_actualizacion = DateTime.Now;
                                            icbvh_even.eventouserid_actualizacion = id_usuario_actual;
                                            db.Entry(icbvh_even).State = EntityState.Modified;
                                            resp_even = db.SaveChanges();
                                            icb_vehiculo_eventos icbvh_evenFin = db.icb_vehiculo_eventos.Where(x =>
                                                    x.planmayor.Contains(modAls.planMayorVh) &&
                                                    x.id_tpevento == iFinRAlis)
                                                .FirstOrDefault();
                                            if (icbvh_evenFin != null)
                                            {
                                                db.icb_vehiculo_eventos.Remove(icbvh_evenFin);
                                                db.SaveChanges();
                                            }
                                        }
                                        else
                                        {
                                            // Crear evento re-alistamiento
                                            addEvento(modAls, id_usuario_actual, iEnvRAlis);
                                        }
                                    }
                                    else
                                    {
                                        // Crear Evento Alistamiento
                                        addEvento(modAls, id_usuario_actual, iEnvAlis);
                                    }
                                }
                            }

                            dbTrans.Commit();
                            resl = true;
                        }
                        catch (Exception ex)
                        {
                            Exception exp = ex;
                            resl = false;
                            dbTrans.Rollback();
                        }
                    }
                }

                return Json(new { resp_ot, resp_consec, resp_bah, resl }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { resl, resp = "Valide los datos ingresados" }, JsonRequestBehavior.AllowGet);
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
            int resp = 0;
            string respuesta = "";
            int idpedido = int.Parse(Request["idpedido"]);
            string planmayor = Request["planmayor"];

            if (idpedido == 0)
            {
                bahia_alistamiento_exh bhAlis = db.bahia_alistamiento_exh.FirstOrDefault(x =>
                    x.plan_mayor == planmayor && (x.tencabezaorden_exh.estadoorden == iCreateOt ||
                                                  x.tencabezaorden_exh.estadoorden == iExecutionOt));
                if (bhAlis != null)
                {
                    using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            tencabezaorden_exh ot = db.tencabezaorden_exh.FirstOrDefault(x => x.id == bhAlis.ot_id);
                            if (ot != null)
                            {
                                //verifico si ese vehiculo tuvo alistamiento mecanico
                                icb_vehiculo vh = db.icb_vehiculo.Where(x => x.plan_mayor == planmayor).FirstOrDefault();
                                icb_vehiculo_eventos pendAlis = db.icb_vehiculo_eventos
                                    .Where(x => x.planmayor == planmayor && x.id_tpevento == 1034).FirstOrDefault();

                                if (pendAlis != null)
                                {
                                    ot.estadoorden = ot.estadoorden == iCreateOt ? iExecutionOt : iFinishOt;
                                    ot.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                                    ot.fec_actualizacion = DateTime.Now;
                                    db.Entry(ot).State = EntityState.Modified;
                                    resp = db.SaveChanges();


                                    // Validar si evento es realistaminto o alistamiento
                                    icb_vehiculo_eventos icbvh_even = db.icb_vehiculo_eventos.Where(x =>
                                            x.planmayor.Contains(pendAlis.planmayor) && x.id_tpevento == iFinAlis)
                                        .FirstOrDefault();
                                    int idevent = icbvh_even == null ? iFinAlis : iFinRAlis;
                                    // Registrar evento fin alistamiento
                                    icb_tpeventos buscarEvento =
                                        db.icb_tpeventos.FirstOrDefault(x =>
                                            x.tpevento_id == idevent); // Fin (Alistamiento - Re Alistamiento)
                                    // Condicionar evento 
                                    if (ot.estadoorden == iFinishOt)
                                    {
                                        // Validar qué se debe finalizar (alistamiento - re-alistamiento)
                                        icb_vehiculo_eventos crearEvento = new icb_vehiculo_eventos
                                        {
                                            eventofec_creacion = DateTime.Now,
                                            eventouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            evento_estado = true,
                                            bodega_id = Convert.ToInt32(Session["user_bodega"]),
                                            evento_nombre = buscarEvento.tpevento_nombre,
                                            planmayor = pendAlis.planmayor,
                                            id_tpevento = idevent,
                                            fechaevento = DateTime.Now,
                                            placa = vh.plac_vh,
                                            ubicacion = vh.ubicacionactual
                                        };
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
                            else
                            {
                                respuesta = "No existe una OT de alistamiento para este vehículo";
                            }
                        }
                        catch (Exception ex)
                        {
                            Exception exp = ex;
                            respuesta =
                                "Se ha presentado un error en guardado de alistamiento. Favor contactar con personal de Exiware";

                            dbTran.Rollback();
                        }
                    }
                }
                else
                {
                    respuesta = "No existe bahía de alistamiento para la bodega en curso";
                }
            }
            else
            {
                icb_bahia_alistamiento bhAlis = db.icb_bahia_alistamiento.FirstOrDefault(x =>
                    x.id_pedido == idpedido && (x.tencabezaorden.estadoorden == iCreateOt ||
                                                x.tencabezaorden.estadoorden == iExecutionOt));
                if (bhAlis != null)
                {
                    using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            tencabezaorden ot = db.tencabezaorden.FirstOrDefault(x => x.id == bhAlis.ot_id);
                            if (ot != null)
                            {
                                //verifico si ese vehiculo tuvo alistamiento mecanico
                                vw_pendientesEntrega pendAlis = db.vw_pendientesEntrega.Where(x => x.id == idpedido).FirstOrDefault();

                                if (pendAlis.FechaMecanico != null)
                                {
                                    ot.estadoorden = ot.estadoorden == iCreateOt ? iExecutionOt : iFinishOt;
                                    ot.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                                    ot.fec_actualizacion = DateTime.Now;
                                    db.Entry(ot).State = EntityState.Modified;
                                    resp = db.SaveChanges();


                                    // Validar si evento es realistaminto o alistamiento
                                    icb_vehiculo_eventos icbvh_even = db.icb_vehiculo_eventos.Where(x =>
                                            x.planmayor.Contains(pendAlis.planmayor) && x.id_tpevento == iFinAlis)
                                        .FirstOrDefault();
                                    int idevent = icbvh_even == null ? iFinAlis : iFinRAlis;
                                    // Registrar evento fin alistamiento
                                    icb_tpeventos buscarEvento =
                                        db.icb_tpeventos.FirstOrDefault(x =>
                                            x.tpevento_id == idevent); // Fin (Alistamiento - Re Alistamiento)
                                    // Condicionar evento 
                                    if (ot.estadoorden == iFinishOt)
                                    {
                                        // Validar qué se debe finalizar (alistamiento - re-alistamiento)
                                        icb_vehiculo_eventos crearEvento = new icb_vehiculo_eventos();
                                        int? propietario_vh = db.icb_vehiculo.Find(pendAlis.planmayor).propietario;
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
                            else
                            {
                                respuesta = "No existe una OT de alistamiento para este vehículo";
                            }
                        }
                        catch (Exception ex)
                        {
                            Exception exp = ex;
                            respuesta =
                                "Se ha presentado un error en guardado de alistamiento. Favor contactar con personal de Exiware";

                            dbTran.Rollback();
                        }
                    }
                }
                else
                {
                    respuesta = "No existe bahía de alistamiento para la bodega en curso";
                }
            }

            var data = new
            {
                valor = resp,
                respuesta
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult TerminarAlistamientoMecanico(string planmayor)
        {
            string respuesta = "";
            int valor = 0;
            if (!string.IsNullOrWhiteSpace(planmayor))
            {
                //busco parametro fin de alistamiento
                icb_sysparameter param1 = db.icb_sysparameter.Where(d => d.syspar_cod == "P119").FirstOrDefault();
                int parametro = param1 != null ? Convert.ToInt32(param1.syspar_value) : 23;
                //parametro evento de fin de alistamiento
                icb_tpeventos eve = db.icb_tpeventos.Where(d => d.codigoevento == parametro).FirstOrDefault();


                //busco en vehiculo
                icb_vehiculo vehi = db.icb_vehiculo.Where(d => d.plan_mayor == planmayor).FirstOrDefault();
                if (vehi != null)
                {
                    //veo si ya se le realizó alistamiento mecánico
                    int alistaexiste = db.icb_vehiculo_eventos
                        .Where(d => d.planmayor == planmayor && d.id_tpevento == eve.tpevento_id).Count();
                    if (alistaexiste == 0)
                    {
                        icb_vehiculo_eventos crearEvento = new icb_vehiculo_eventos();
                        int? propietario_vh = db.icb_vehiculo.Find(planmayor).propietario;
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
                    respuesta =
                        "Debe ingresar un plan mayor válido. El plan mayor ingresado no existe en base de datos";
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
                            where (t.estadoorden == iCreateOt || t.estadoorden == iExecutionOt) && v.FechaRecepcionBodega != null
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
                asesor = v.v.asesor != null ? v.v.asesor : "",
                pedidoFecha = v.v.FechaPedido != null
                    ? v.v.FechaPedido.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                fecha = v.a.bh_als_fecha != null
                    ? v.a.bh_als_fecha.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                nombreBodega = v.v.bodccs_nombre != null ? v.v.bodccs_nombre : "",
                icon = v.estadoorden == iCreateOt ? "fa-play" : "fa-check",
                info = v.estadoorden == iCreateOt ? "primary" : "success",
                estadoorden = v.estadoorden != null ? v.estadoorden.Value.ToString() : "",
                placa = v.v.plac_vh != null ? v.v.plac_vh : "",
                tiene_alistamiento = v.v.FechaMecanico != null ? 1 : 0,
                fecha_alistamiento_mecanico = v.v.FechaMecanico != null
                    ? v.v.FechaMecanico.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : ""
            }).Distinct();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //Se crea una función para realiar el comentario de observación en alistamiento 

        public JsonResult jsonResult(string planmayor, string observacion)
        {
            int value = 0;
            string answer;
            if (!string.IsNullOrWhiteSpace(planmayor) && !string.IsNullOrWhiteSpace(observacion))
            {
                bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                icb_vehiculo alistamiento = db.icb_vehiculo.Where(d => d.plan_mayor == planmayor).FirstOrDefault();
                if (alistamiento != null)
                {
                    alistamiento.observacion_alistamiento = observacion;
                    db.Entry(alistamiento).State = EntityState.Modified;
                    int guardar = db.SaveChanges();

                    if (guardar > 0)
                    {
                        answer = "observación Guardada";
                        value = 1;
                    }
                    else
                    {
                        answer = "debe digitar una observación";
                    }
                }
                else
                {
                    answer =
                        "El numero de plan mayor suministrado no corresponde a un vehículo registrado en el sistema";
                }
            }
            else
            {
                answer = "debe digitar una observación y suministrar un numero de plan mayor";
            }


            var respuesta = new
            {
                value,
                answer
            };

            return Json(respuesta);
        }

        public JsonResult validate(string planmayor)
        {
            int value = 1;
            string respuesta = "";
            icb_vehiculo data = db.icb_vehiculo.Where(d => d.plan_mayor == planmayor).FirstOrDefault();
            if (data != null)
            {
                respuesta = data.observacion_alistamiento;
                value = 0;
            }

            var answer = new
            {
                value,
                respuesta
            };

            return Json(answer);
        }

        public JsonResult BuscarPendientesAlistamientoExh()
        {
            bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            operacionAlistamiento();
            var pendAlis = (from a in db.bahia_alistamiento_exh
                            join t in db.tencabezaorden_exh on a.ot_id equals t.id
                            join vh in db.icb_vehiculo on a.plan_mayor equals vh.plan_mayor
                            where t.estadoorden == iCreateOt || t.estadoorden == iExecutionOt /*&& v.FechaRecepcionBodega != null*/
                            select new
                            {
                                /*v,*/
                                a,
                                t.estadoorden,
                                vh
                            }).ToList();
            var data = pendAlis.Select(v => new
            {
                id = 0,
                v.a.bahia_als_id,
                planmayor = v.vh.plan_mayor != null ? v.vh.plan_mayor : "",
                vin = v.vh.vin != null ? v.vh.vin : "",
                modelo = v.vh.modelo_vehiculo.modvh_nombre,
                anio = v.vh.anio_vh,
                color = v.vh.colvh_id != null ? v.vh.color_vehiculo.colvh_nombre : "",
                ubicacion = v.vh.ubicacionactual != null ? v.vh.ubicacion_bodega.descripcion : "",
                fecha = v.a.bahia_als_fecha != null
                    ? v.a.bahia_als_fecha.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                nombreBodega =v.vh.bodega_concesionario.bodccs_nombre,
                icon = v.estadoorden == iCreateOt ? "fa-play" : "fa-check",
                info = v.estadoorden == iCreateOt ? "primary" : "success",
                v.estadoorden,
                tiene_alistamiento = v.vh.plan_mayor != null ? verificarAlistMecanico(v.vh.plan_mayor) : 0,
                fecha_alistamiento_mecanico = v.vh.plan_mayor != null
                    ? fechaAlistMecanico(v.vh.plan_mayor)
                    : "",
                recepcion = v.vh.plan_mayor != null ? verificarRecepcion(v.vh.plan_mayor) : 0
            }).Distinct();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public int verificarAlistMecanico(string planmayor)
        {
            int resultado = 0;

            icb_vehiculo_eventos evento = db.icb_vehiculo_eventos.Where(x => x.id_tpevento == 1034 && x.planmayor == planmayor)
                .FirstOrDefault();
            if (evento != null)
            {
                resultado = 1;
            }

            return resultado;
        }

        public string fechaAlistMecanico(string planmayor)
        {
            string resultado = "";

            icb_vehiculo_eventos evento = db.icb_vehiculo_eventos.Where(x => x.id_tpevento == 1034 && x.planmayor == planmayor)
                .FirstOrDefault();
            if (evento != null)
            {
                resultado = evento.fechaevento.ToString("yyyy/MM/dd", new CultureInfo("en-US"));
            }

            return resultado;
        }

        public int verificarRecepcion(string planmayor)
        {
            int resultado = 0;

            icb_vehiculo_eventos evento = db.icb_vehiculo_eventos.Where(x => x.id_tpevento == 2 && x.planmayor == planmayor)
                .FirstOrDefault();
            if (evento != null)
            {
                resultado = 1;
            }

            return resultado;
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
                pedidoFecha = v.a.bh_als_fecha != null
                    ? v.a.bh_als_fecha.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                nombreBodega = v.item.bodccs_nombre != null ? v.item.bodccs_nombre : ""
            }).Distinct();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BuscarAlistamientoRealizadosExh(string planmayor)
        {
            bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            operacionAlistamiento();
            var pendAlis = (from a in db.bahia_alistamiento_exh
                            join v in db.icb_vehiculo
                                on a.plan_mayor equals v.plan_mayor
                            where a.tencabezaorden_exh.estadoorden == iFinishOt && a.plan_mayor == planmayor
                            select new { v, a }).ToList();

            var data = pendAlis.Select(v => new
            {
                v.a.bahia_als_id,
                v.v.icbvh_id,
                planmayor = v.v.plan_mayor != null ? v.v.plan_mayor : "",
                vin = v.v.vin != null ? v.v.vin : "",
                modelo = v.v.modvh_id != null ? v.v.modelo_vehiculo.modvh_nombre : "",
                anio = v.v.anio_vh,
                color = v.v.colvh_id != null ? v.v.color_vehiculo.colvh_nombre : "",
                ubicacion = v.v.ubicacionactual != null ? v.v.ubicacion_bodega.descripcion : "",
                pedidoFecha = v.a.bahia_als_fecha != null
                    ? v.a.bahia_als_fecha.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                nombreBodega = v.v.bodega_concesionario.bodccs_nombre
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
                        where vpe.planmayor != null && vpe.pazysalvo == false && vpe.FechaVenta != null &&
                              vpe.FechaMatricula == null // && vpe.FechaRecepcionBodega != null
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
                asesor = p.vpe.asesor,
                soat = p.vpe.numerosoat != null ? p.vpe.numerosoat : "",
                pedidoFecha = p.vpe.FechaPedido != null
                    ? p.vpe.FechaPedido.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                desmbolsoFecha = p.fec_desembolso != null
                    ? p.fec_desembolso.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                facturaFecha = p.vpe.FechaVenta != null
                    ? p.vpe.FechaVenta.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                tramitesFecha = p.vpe.FechaTramite != null
                    ? p.vpe.FechaTramite.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                matriculaFecha = p.vpe.FechaMatricula != null
                    ? p.vpe.FechaMatricula.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                manifiestoFecha = p.vpe.FechaManifiesto != null
                    ? p.vpe.FechaManifiesto.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : !string.IsNullOrWhiteSpace(p.vehi.plan_mayor)
                        ? p.vehi.fecentman_vh != null
                            ? p.vehi.fecentman_vh.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                            : ""
                        : "",
                inicioFecha = p.vpe.FechaAlistamiento != null
                    ? p.vpe.FechaAlistamiento.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                botonalisembe = !string.IsNullOrWhiteSpace(p.vpe.planmayor)
                    ? botonalistamientoe(p.vpe.planmayor, p.vpe.FechaAlistamiento, p.vpe.FechaFinAlistamiento)
                    : "",
                finFechaRecepcion = p.vpe.FechaRecepcionBodega != null
                    ? p.vpe.FechaRecepcionBodega.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                plac_vh = p.vpe.plac_vh != null ? p.vpe.plac_vh : "",
                serie = p.vpe.vin != null ? p.vpe.vin : "",
                botonalismec = !string.IsNullOrWhiteSpace(p.vpe.planmayor)
                    ? botonalistamientoem(p.vpe.planmayor, p.vpe.FechaRecepcionBodega)
                    : "",
                botonalisacce = !string.IsNullOrWhiteSpace(p.vpe.planmayor) ? botonalistamientoa(p.vpe.planmayor) : "",
                finFecha = p.vpe.FechaFinAlistamiento != null
                    ? p.vpe.FechaFinAlistamiento.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                entregaFecha = p.vpe.FechaEntrega != null
                    ? p.vpe.FechaEntrega.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                programacionFecha = p.vpe.FechaProgramacioEntrega != null
                    ? p.vpe.FechaProgramacioEntrega.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                p.vpe.pazysalvo,
                usupazysalvo = p.vpe.usupazysalvo != null ? p.vpe.usupazysalvo : "",
                numFactura = p.vpe.numfactura != null ? p.vpe.numfactura.ToString() : "0",
                estadoAlistamiento = p.estadoorden,
                estadoAlistaminetoEstilo = p.estadoorden != null ? estilosAlist["est_" + p.estadoorden] : "",
                poliza = p.poliza != null ? p.poliza : ""
            }).Distinct();

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonResult pazysalvo(int pedido, bool autorizado)
        {
            vpedido data = db.vpedido.FirstOrDefault(x => x.id == pedido);

            if (data != null)
            {
                data.pazysalvo = autorizado;
                data.usupazysalvo = Convert.ToInt32(Session["user_usuarioid"]);
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { ok = true, mensaje = "Se actualizo el pedido exitosamente" },
                    JsonRequestBehavior.AllowGet);
            }

            return Json(new { ok = false, mensaje = "No fue posible actualizar el pedido, por favor verifique" },
                JsonRequestBehavior.AllowGet);
        }

        public JsonResult boletoSalida(int pedido)
        {
            encab_documento data = db.encab_documento.FirstOrDefault(x => x.id_pedido_vehiculo == pedido);

            if (data != null)
            {
                int tipoDocumento = data.tipo;
                int idEncabezado = data.idencabezado;
                int bodega = data.bodega;
                int nit = data.nit;

                return Json(new { ok = true, tipoDocumento, idEncabezado, bodega, nit }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { ok = false }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BrowserVehiculosEntregados()
        {
            return View();
        }


        public JsonResult BuscarVehiculosEntregados()
        {
            List<vw_pendientesEntrega> info = (from vpe in db.vw_pendientesEntrega
                                               where vpe.planmayor != null && vpe.pazysalvo
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
                pedidoFecha = p.FechaPedido != null
                    ? p.FechaPedido.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                facturaFecha = p.FechaVenta != null
                    ? p.FechaVenta.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                tramitesFecha = p.FechaTramite != null
                    ? p.FechaTramite.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                matriculaFecha = p.FechaMatricula != null
                    ? p.FechaMatricula.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                manifiestoFecha = p.FechaManifiesto != null
                    ? p.FechaManifiesto.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                inicioFecha = p.FechaAlistamiento != null
                    ? p.FechaAlistamiento.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                finFecha = p.FechaFinAlistamiento != null
                    ? p.FechaFinAlistamiento.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                entregaFecha = p.FechaEntrega != null
                    ? p.FechaEntrega.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                programacionFecha = p.FechaProgramacioEntrega != null
                    ? p.FechaProgramacioEntrega.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
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
                int rolActual = Convert.ToInt32(Session["user_rolid"]);
                rolacceso buscarAccesoAlValor = db.rolacceso.FirstOrDefault(x => x.idrol == rolActual && x.idpermiso == 12);
                bool puedeActualizar = buscarAccesoAlValor != null ? true : false;

                int anioModeloAux = Convert.ToInt32(anioModelo);
                anio_modelo buscaPrecio = db.anio_modelo.FirstOrDefault(x =>
                    x.codigo_modelo == codigoModelo && x.anio_modelo_id == anioModeloAux);
                vlistanuevos precioVh = db.vlistanuevos.Where(x => x.anomodelo == anioModeloAux).OrderByDescending(x => x.ano)
                    .ThenByDescending(x => x.mes).FirstOrDefault();
                var buscaColor = (from color in db.color_vehiculo select new { color.colvh_id, color.colvh_nombre })
                    .ToList();
                var result = new
                {
                    buscaPrecio.descripcion,
                    //aqui estaba colocado buscaPrecio.precio
                    valor = precioVh != null ? precioVh.precioespecial.ToString():"0",
                    poliza = buscaPrecio!= null ? buscaPrecio.poliza.ToString() : "",
                    matricula = buscaPrecio != null ? buscaPrecio.matricula.ToString() : "",
                    /*
                    valor = precioVh != null
                        ? precioVh.precioespecial != null ? precioVh.precioespecial.ToString() : "0"
                        : "0",
                    poliza = buscaPrecio.poliza != null ? buscaPrecio.poliza.ToString() : "",
                    matricula = buscaPrecio.matricula != null ? buscaPrecio.matricula.ToString() : "",
                    */
                    esNuevo = true,
                    buscaColor,
                    puedeActualizar,
                    //Se actualiza el código para que sólo muestre el IVA e 
                    codigo_iva = buscaPrecio.porcentaje_iva.ToString("N0", new CultureInfo("is-IS")),
                    porcentaje_impoconsumo = buscaPrecio.impuesto_consumo.ToString("N0", new CultureInfo("is-IS"))
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var buscaColor = (from referencias in db.vw_referencias_total
                                  where referencias.codigo == codigoModelo && referencias.anio_vh == anioModelo &&
                                        referencias.usado == true
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

            return RedirectToAction("Login", "Login");
        }

        public JsonResult BuscarVHConAlistamiento()
        {
            if (Session["user_usuarioid"] != null)
            {
                bool bodega2 = int.TryParse(Session["user_bodega"].ToString(), out int bodega);
                if (bodega2)
                {
                    int usuariox = Convert.ToInt32(Session["user_usuarioid"].ToString());

                    List<int> listabodega = db.bodega_usuario.Where(d => d.id_usuario == usuariox).Select(d => d.id_bodega)
                        .ToList();
                    var info = (from vpe in db.vw_pendientesEntrega
                                join vehi in db.icb_vehiculo on vpe.planmayor equals vehi.plan_mayor into vu
                                from vehi in vu.DefaultIfEmpty()
                                where vpe.planmayor != null && vpe.pazysalvo == false && vpe.FechaFinAlistamiento != null &&
                                      listabodega.Contains(vpe.bodega) && vpe.FechaRecepcionBodega != null &&
                                      vpe.FechaProgramacioEntrega != null
                                select new { vpe, vehi }).ToList();

                    var data = info.Select(p => new
                    {
                        p.vpe.id,
                        verificarAlistamientoAcc = !string.IsNullOrWhiteSpace(p.vpe.planmayor)
                            ? verificarAlistamientoAcc(p.vpe.planmayor)
                            : "",
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
                        pedidoFecha = p.vpe.FechaPedido != null
                            ? p.vpe.FechaPedido.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                            : "",
                        facturaFecha = p.vpe.FechaVenta != null
                            ? p.vpe.FechaVenta.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                            : "",
                        tramitesFecha = p.vpe.FechaTramite != null
                            ? p.vpe.FechaTramite.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                            : "",
                        matriculaFecha = p.vpe.FechaMatricula != null
                            ? p.vpe.FechaMatricula.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                            : "",
                        manifiestoFecha = p.vpe.FechaManifiesto != null
                            ? p.vpe.FechaManifiesto.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                            : !string.IsNullOrWhiteSpace(p.vehi.plan_mayor)
                                ? p.vehi.fecentman_vh != null
                                    ? p.vehi.fecentman_vh.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                                    : ""
                                : "",
                        inicioFecha = p.vpe.FechaAlistamiento != null
                            ? p.vpe.FechaAlistamiento.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                            : "",
                        finFecha = p.vpe.FechaFinAlistamiento != null
                            ? p.vpe.FechaFinAlistamiento.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                            : "",
                        p.vpe.IdTecnicoFinAlistamiento,
                        p.vpe.NombreTecnicoFinAlistamiento,
                        entregaFecha = p.vpe.FechaEntrega != null
                            ? p.vpe.FechaEntrega.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                            : "",
                        programacionFecha = p.vpe.FechaProgramacioEntrega != null
                            ? p.vpe.FechaProgramacioEntrega.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                            : "",
                        p.vpe.pazysalvo,
                        usupazysalvo = p.vpe.usupazysalvo != null ? p.vpe.usupazysalvo : "",
                        numFactura = p.vpe.numfactura != null ? p.vpe.numfactura.ToString() : ""
                    });

                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                return Json(0, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckListAlistamiento(string planM, int? tecnico, int? menu)
        {
            int? tercero = db.icb_vehiculo.FirstOrDefault(x => x.plan_mayor == planM).propietario;
            ViewBag.tercero = tercero;
            ViewBag.tecnico = tecnico;
            ViewBag.placa = planM;
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult CheckListAlistamiento(vencabingresovehiculo modelo, int? menu)
        {
            operacionAlistamiento();

            int preguntas = int.Parse(Request["numeroParametros"]);
            //var tecnico = int.Parse(Request["idTecnico"]);
            modelo.entrega = true;
            modelo.recepcion = false;
            modelo.fecha = DateTime.Now;
            modelo.fec_creacion = DateTime.Now;
            modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);

            if (modelo.tercero == null || modelo.tercero == 0)
            {
                int sinDoc = db.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0").tercero_id;
                modelo.tercero = sinDoc;
            }

            db.vencabingresovehiculo.Add(modelo);

            //busco el parámetro de sistema de evento fin alistamiento
            icb_sysparameter ali1 = db.icb_sysparameter.Where(d => d.syspar_cod == "P66").FirstOrDefault();
            int eventoalistamiento = ali1 != null ? Convert.ToInt32(ali1.syspar_value) : 22;
            //busco en la lista de eventos el usuario que registró el evento de fin de alistamiento para este vehículo
            icb_vehiculo_eventos evento = db.icb_vehiculo_eventos.OrderByDescending(d => d.fechaevento).Where(d =>
                d.icb_tpeventos.codigoevento == eventoalistamiento &&
                (d.icb_vehiculo.plac_vh == modelo.placa || d.icb_vehiculo.plan_mayor == modelo.placa)).FirstOrDefault();

            if (modelo.aceptaalistamiento == false)
            {
                int buscarUsuario = evento != null ? evento.eventouserid_creacion : modelo.userid_creacion;
                tareasasignadas tarea = new tareasasignadas
                {
                    fec_creacion = DateTime.Now,
                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    estado = true,
                    idusuarioasignado = buscarUsuario,
                    notas = "Devolución por no conformidad de alistamiento",
                    observaciones = modelo.Observacion
                };
                db.tareasasignadas.Add(tarea);

                //busco si el vehiculo está en un pedido activo
                vpedido pedidoexiste = db.vpedido.Where(d => d.planmayor == modelo.placa && d.anulado == false).FirstOrDefault();
                if (pedidoexiste != null)
                {
                    //reverso el alistamiento de embellecimiento (si es de pedido)
                    icb_bahia_alistamiento bhAlis = db.icb_bahia_alistamiento.FirstOrDefault(x =>
                       x.id_pedido == pedidoexiste.id && (x.tencabezaorden.estadoorden == iFinishOt ||
                                                x.tencabezaorden.estadoorden == iExecutionOt));
                    if (bhAlis != null)
                    {
                        tencabezaorden ot = db.tencabezaorden.FirstOrDefault(x => x.id == bhAlis.ot_id);
                        ot.estadoorden = iCreateOt;
                        ot.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        ot.fec_actualizacion = DateTime.Now;
                        db.Entry(ot).State = EntityState.Modified;

                        //le quito el evento de fin de alistamiento
                        int finalis = Convert.ToInt32(sFinAlis);
                        icb_vehiculo_eventos eventos = db.icb_vehiculo_eventos.OrderByDescending(d => d.fechaevento).Where(d => d.planmayor == modelo.placa && d.id_tpevento == finalis).FirstOrDefault();
                        if (eventos != null)
                        {
                            db.Entry(eventos).State = EntityState.Deleted;
                        }
                    }
                }
            }

            int guardar = db.SaveChanges();
            if (guardar > 0)
            {
                int ultimoEncabezado = modelo.id;
                for (int i = 0; i <= preguntas; i++)
                {
                    string parametros = Request["parametros" + i];
                    string respuestas = Request["respuestas" + i];
                    respuestas = respuestas != null ? respuestas : "";
                    if (parametros != null && respuestas != null)
                    {
                        if (respuestas == "on")
                        {
                            db.vdetalleingresovehiculo.Add(new vdetalleingresovehiculo
                            {
                                idencabezado = ultimoEncabezado,
                                iditeminspeccion = Convert.ToInt32(parametros),
                                respuesta = "true"
                            });
                        }
                        else
                        {
                            db.vdetalleingresovehiculo.Add(new vdetalleingresovehiculo
                            {
                                idencabezado = ultimoEncabezado,
                                iditeminspeccion = Convert.ToInt32(parametros),
                                respuesta = respuestas
                            });
                        }
                    }
                }

                int guardarRespuestas = db.SaveChanges();
                if (guardarRespuestas > 0)
                {
                    TempData["mensaje"] = "El check list de alistamiento fue registrado de manera exitosa";
                }
                else
                {
                    TempData["mensaje_error"] =
                        "Error registrar el check list de alistamiento, por favor intente nuevamente";
                }
            }

            return RedirectToAction("BrowserVHConAlistamiento");
        }

        public JsonResult pintarChackListAlistamiento()
        {
            int parametro =
                Convert.ToInt32(db.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P74").syspar_value);

            var buscarCheck = (from checks in db.vingresovehiculo
                               where checks.tipoCheckid == parametro
                               select new
                               {
                                   id_descripcion = checks.id,
                                   checks.descripcion,
                                   checks.tiporespuesta,
                                   opciones = db.vingresovehiculoopcion.Where(x => x.id_ingreso == checks.id)
                                       .Select(x => new { x.id, x.descripcion }).ToList()
                               }).ToList();

            return Json(buscarCheck, JsonRequestBehavior.AllowGet);
        }

        public JsonResult verificarPendientes(int idPedido)
        {
            // se eliminaron las validaciones del saldo el dia 28/02/2019 por tarea 3055 - 543
            int docPendiente = 0;
            vpedido infoPedido = db.vpedido.FirstOrDefault(x => x.id == idPedido);
            int? flotaAux = infoPedido.flota;
            int? codFlota = infoPedido.codigoflota;
            string tercero = (from p in db.vpedido
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
                vflota flota = db.vflota.Find(flotaAux);

                if (flota != null)
                {
                    Lista.ListaDocNecesarios = (from v in db.vvalidacionpeddoc
                                                join r in db.vdocrequeridosflota
                                                    on v.iddocrequerido equals r.id
                                                join d in db.vdocumentosflota
                                                    on r.iddocumento equals d.id
                                                where r.codflota == codFlota && v.idpedido == infoPedido.id
                                                select new docNecesarios
                                                {
                                                    id = d.id,
                                                    documento = d.documento,
                                                    iddocumento = d.iddocumento,
                                                    estado = v.estado,
                                                    cargado =
                                                        db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x =>
                                                            x.idpedido == infoPedido.id && x.iddocumento == d.iddocumento) != null
                                                            ? db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x =>
                                                                x.idpedido == infoPedido.id && x.iddocumento == d.iddocumento).id
                                                            : 0
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
                                                cargado =
                                                    db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x =>
                                                        x.idpedido == infoPedido.id && x.iddocumento == d.iddocumento) != null
                                                        ? db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x =>
                                                            x.idpedido == infoPedido.id && x.iddocumento == d.iddocumento).id
                                                        : 0
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
                                                cargado =
                                                    db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x =>
                                                        x.idpedido == infoPedido.id && x.iddocumento == d.iddocumento) != null
                                                        ? db.documentos_vehiculo.OrderByDescending(x => x.fec_creacion).FirstOrDefault(x =>
                                                            x.idpedido == infoPedido.id && x.iddocumento == d.iddocumento).id
                                                        : 0
                                            }).ToList();
            }

            foreach (docNecesarios item in Lista.ListaDocNecesarios)
            {
                if (item.estado == false)
                {
                    docPendiente = 1;
                }
            }

            if (docPendiente == 1)
            {
                //La excepcion de documentos solo se le realiza a las flotas.
                if (flotaAux != null)
                {
                    return Json(new { solicitar = true }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { solicitar = false }, JsonRequestBehavior.AllowGet);
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

        public ActionResult BrowserModelosNoDisponibles()
        {
            if (Session["user_usuarioid"] != null)
            {
                int bodega = Convert.ToInt32(Session["user_bodega"]);
                ViewBag.bodega = bodega;
                List<vpedido> listanodisponibles = db.vpedido.Where(d =>
                    d.anulado == false && (d.planmayor == null || d.planmayor == string.Empty) && d.no_disponible &&
                    d.solicitado == false && d.bodega == bodega).ToList();

                var marcasdisponibles = listanodisponibles.GroupBy(d => d.modelo).Select(d => new
                {
                    id = d.Select(e => e.modelo_vehiculo.marca_vehiculo.marcvh_id).FirstOrDefault(),
                    nombre = d.Select(e => e.modelo_vehiculo.marca_vehiculo.marcvh_nombre).FirstOrDefault()
                }).ToList();

                ViewBag.marcasnodisponibles = new SelectList(marcasdisponibles, "id", "nombre");
                var modelosnodisponibles = listanodisponibles.GroupBy(d => d.modelo).Select(d => new
                {
                    id = d.Select(e => e.modelo_vehiculo.modvh_codigo).FirstOrDefault(),
                    nombre = d.Select(e => e.modelo_vehiculo.modvh_nombre).FirstOrDefault()
                }).ToList();
                ViewBag.modelosnodisponibles = new SelectList(modelosnodisponibles, "id", "nombre");

                List<vpedido> listasolicitados = db.vpedido.Where(d =>
                    d.anulado == false && (d.planmayor == null || d.planmayor == string.Empty) && d.no_disponible &&
                    d.solicitado && d.bodega == bodega).ToList();
                var marcassolicitados = listasolicitados.GroupBy(d => d.modelo).Select(d => new
                {
                    id = d.Select(e => e.modelo_vehiculo.marca_vehiculo.marcvh_id).FirstOrDefault(),
                    nombre = d.Select(e => e.modelo_vehiculo.marca_vehiculo.marcvh_nombre).FirstOrDefault()
                }).ToList();
                ViewBag.marcassolicitados = new SelectList(marcassolicitados, "id", "nombre");

                var modelossolicitados = listasolicitados.GroupBy(d => d.modelo).Select(d => new
                {
                    id = d.Select(e => e.modelo_vehiculo.modvh_codigo).FirstOrDefault(),
                    nombre = d.Select(e => e.modelo_vehiculo.modvh_nombre).FirstOrDefault()
                }).ToList();
                ViewBag.modelossolicitados = new SelectList(modelossolicitados, "id", "nombre");

                return View();
            }

            return RedirectToAction("Login", "Login");
        }

        public JsonResult listarModelos(int[] marca, string[] modelos, int? bodega, int sw = 0)
        {
            System.Linq.Expressions.Expression<Func<vpedido, bool>> predicado = PredicateBuilder.True<vpedido>();
            System.Linq.Expressions.Expression<Func<vpedido, bool>> predicadomarcas = PredicateBuilder.False<vpedido>();
            System.Linq.Expressions.Expression<Func<vpedido, bool>> predicadomodelos = PredicateBuilder.False<vpedido>();

            if (marca != null)
            {
                if (marca[0] != 0)
                {
                    for (int i = 0; i < marca.Length; i++)
                    {
                        predicadomarcas = predicadomarcas.Or(d => d.modelo_vehiculo.mar_vh_id == marca[i]);
                    }

                    predicado = predicado.And(predicadomarcas);
                }
            }

            if (modelos != null)
            {
                if (modelos[0] != "")
                {
                    for (int j = 0; j < modelos.Length; j++)
                    {
                        string stringmodelo = modelos[j];
                        predicadomodelos = predicadomodelos.Or(d => d.modelo == stringmodelo);
                    }

                    predicado = predicado.And(predicadomodelos);
                }
            }

            predicado = predicado.And(d => d.no_disponible /*&& d.anulado == false*/);
            if (sw == 0)
            {
                predicado = predicado.And(d => d.solicitado == false);
            }
            else
            {
                predicado = predicado.And(d => d.solicitado);
            }

            List<vpedido> listanodisponibles = db.vpedido.Where(predicado).ToList();
            var data = listanodisponibles.Select(d => new
            {
                d.id,
                d.numero,
                d.modelo,
                nombremodelo = d.modelo_vehiculo.modvh_nombre,
                db.anio_modelo.Where(x => x.anio_modelo_id == d.id_anio_modelo).FirstOrDefault().anio,
                color = !string.IsNullOrWhiteSpace(d.Color_Deseado) ? vercolor(d.Color_Deseado) : "",
                cliente = d.icb_terceros.prinom_tercero + "  " + d.icb_terceros.apellido_tercero,
                asesor = d.users.user_nombre + " " + d.users.user_apellido,
                bodega = d.bodega_concesionario.bodccs_nombre,
                d.solicitado,
                ubicacion = d.icb_vehiculo != null ? d.icb_vehiculo.ubicacion_bodega.descripcion : "",
                segmento = d.modelo_vehiculo.seg_vh_id != null ? d.modelo_vehiculo.segmento_vehiculo.segvh_nombre : "",
                estado = d.anulado != true ? "Activo" : "Anulado",
                fechaPedido = d.fecha != null ? d.fecha.ToString("yyyy-MM-dd", new CultureInfo("en-US")) : ""
            }).ToList();

            return Json(data);
        }


        [HttpPost]
        public ActionResult solicitarModelos(string lista)
        {
            int valor = 0;
            string resultado = "";
            if (!string.IsNullOrWhiteSpace(lista))
            {
                string vehiculos = lista;
                vehiculos = vehiculos.TrimEnd(',', ' ');

                string[] autos = vehiculos.Split(',');
                //var i = 0;
                List<int> idpedidos = new List<int>();

                //los marco como solicitados
                foreach (string s in autos)
                {
                    //busco esos ids
                    int id = Convert.ToInt32(s);
                    vpedido ovj = db.vpedido.Where(h => h.id == id).FirstOrDefault();
                    ovj.solicitado = true;
                    db.Entry(ovj).State = EntityState.Modified;
                    db.SaveChanges();
                    idpedidos.Add(id);
                }

                //genero el excel con la información
                List<vpedido> listadopedidos = db.vpedido.Where(d => d.anulado == false && idpedidos.Contains(d.id)).ToList();
                string nombre = "informe_solicitud_modelos_" +
                             DateTime.Now.ToString("yyyy-MM-dd", new CultureInfo("en-US"));
                string nombre2 = nombre;
                var datos = listadopedidos.Select(d => new
                {
                    d.numero,
                    d.modelo,
                    nombremodelo = d.modelo_vehiculo.modvh_nombre,
                    anio = d.id_anio_modelo,
                    color = !string.IsNullOrWhiteSpace(d.Color_Deseado) ? vercolor(d.Color_Deseado) : "",
                    cliente = d.icb_terceros.prinom_tercero + "  " + d.icb_terceros.apellido_tercero,
                    asesor = d.users.user_nombre + " " + d.users.user_apellido,
                    bodega = d.bodega_concesionario.bodccs_nombre,
                    d.solicitado
                }).ToList();

                ExcelPackage excel = new ExcelPackage();
                ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Sheet1");
                workSheet.Cells[1, 1].Value = "Iceberg ERP - SOLICITUD de MODELOS";

                workSheet.Cells[2, 1].Value = "N° Pedido";
                workSheet.Cells[2, 2].Value = "Modelo";
                workSheet.Cells[2, 3].Value = "Descripcion";
                workSheet.Cells[2, 4].Value = "Año";
                workSheet.Cells[2, 5].Value = "Color";
                workSheet.Cells[2, 6].Value = "Cliente que Solicita";
                workSheet.Cells[2, 7].Value = "Asesor";
                workSheet.Cells[2, 8].Value = "Bodega";
                workSheet.Cells[3, 1].LoadFromCollection(datos, false);

                string server = HttpContext.Server.MapPath("~/Content/DocumentosVehiculo" + "/" + nombre2 + ".xlsx");
                string dire = Request.Url.Scheme + "://" + Request.Url.Authority;
                string path = server;
                using (FileStream aFile = new FileStream(server, FileMode.Create))
                {
                    aFile.Seek(0, SeekOrigin.Begin);
                    excel.SaveAs(aFile);
                    aFile.Close();
                }

                resultado = dire + "/Content/DocumentosVehiculo" + "/" + nombre2 + ".xlsx";
                valor = 1;
            }
            else
            {
                resultado = "NO se han seleccionado modelos para solicitar";
            }

            var data = new
            {
                valor,
                resultado
            };

            return Json(data);
        }

        public string vercolor(string codigo)
        {
            string resultado = "";

            if (!string.IsNullOrWhiteSpace(codigo))
            {
                color_vehiculo color = db.color_vehiculo.Where(d => d.colvh_id == codigo).FirstOrDefault();
                if (color != null)
                {
                    resultado = color.colvh_nombre;
                }
            }

            return resultado;
        }

        public JsonResult verRepuestosExhibicion(string planmayor)
        {
            int valor = 0;
            string resultado = "";
            if (!string.IsNullOrWhiteSpace(planmayor))
            {
                icb_sysparameter ejecu = db.icb_sysparameter.Where(d => d.syspar_cod == "P116").FirstOrDefault();
                int razoningreso = ejecu != null ? Convert.ToInt32(ejecu.syspar_value) : 6;
                //veo si ese plan mayor tiene una ot de accesorios sin pedido
                tencabezaorden existeot = db.tencabezaorden.Where(d =>
                    d.placa == planmayor && d.razoningreso == razoningreso && d.fecha_fin_operacion != null &&
                    d.idpedido == null).FirstOrDefault();
                if (existeot != null)
                {
                    //veo si hay accesorios instalados
                    var buscarCliente = db.tercero_cliente.Where(d => d.tercero_id == existeot.tercero)
                        .Select(d => new { d.lprecios_repuestos, d.dscto_rep }).FirstOrDefault();

                    List<tsolicitudrepuestosot> acce = db.tsolicitudrepuestosot.Where(d =>
                        d.recibido && d.canttraslado > 0 && d.tdetallerepuestosot.idorden == existeot.id).ToList();
                    var listarepuestos = acce.Select(d => new
                    {
                        d.tdetallerepuestosot.id,
                        codigo = d.icb_referencia.ref_codigo,
                        nombre = d.icb_referencia.ref_descripcion,
                        cantidad = d.canttraslado,
                        iva = d.tdetallerepuestosot.poriva,
                        descuento = d.tdetallerepuestosot.pordescto,
                        d.valor,
                        reemplazar = d.icb_referencia.permite_retirar ? 1 : 0,
                        valor_descuento = calculardescuentore(d.valor.Value, d.canttraslado,
                            d.tdetallerepuestosot.pordescto),
                        valor_iva = calcularivare(d.valor, d.canttraslado, d.tdetallerepuestosot.pordescto,
                            d.tdetallerepuestosot.poriva),
                        valorTotal = Math.Round(d.valor.Value * d.canttraslado -
                                                calculardescuentore(d.valor.Value, d.canttraslado,
                                                    d.tdetallerepuestosot.pordescto) + calcularivare(d.valor,
                                                    d.canttraslado, d.tdetallerepuestosot.pordescto,
                                                    d.tdetallerepuestosot.poriva))
                    }).ToList();

                    valor = 1;
                    var data = new
                    {
                        valor,
                        listarepuestos,
                        resultado
                    };

                    return Json(data);
                }
                else
                {
                    var data = new
                    {
                        valor,
                        resultado
                    };

                    return Json(data);
                }
            }

            {
                var data = new
                {
                    valor,
                    resultado
                };

                return Json(data);
            }
        }

        public JsonResult confirmarRepuestos(string planmayor, string ordenesretiradas, int? idpedido)
        {
            int valor = 0;
            string resultado = "";

            if (!string.IsNullOrWhiteSpace(planmayor) && idpedido != null)
            {
                //veo si existe como vehículo
                icb_vehiculo vehi = db.icb_vehiculo.Where(d => d.plan_mayor == planmayor).FirstOrDefault();
                if (vehi != null)
                {
                    //busco la ot de accesorios que venga de exhibicion
                    icb_sysparameter ejecu = db.icb_sysparameter.Where(d => d.syspar_cod == "P116").FirstOrDefault();
                    int razoningreso = ejecu != null ? Convert.ToInt32(ejecu.syspar_value) : 6;
                    //veo si ese plan mayor tiene una ot de accesorios sin pedido
                    tencabezaorden existeot = db.tencabezaorden.Where(d =>
                        d.placa == planmayor && d.razoningreso == razoningreso && d.fecha_fin_operacion != null &&
                        d.idpedido == null).FirstOrDefault();
                    if (existeot != null)
                    {
                        //veo si hay repuestos por eliminar 
                        if (!string.IsNullOrWhiteSpace(ordenesretiradas))
                        {
                            string acceretirar = ordenesretiradas.TrimEnd(',', ' ');

                            string[] acce = acceretirar.Split(',');
                            //procedo a verificar si para ese plan mayor hay esos repuestos a retirar
                            foreach (string item in acce)
                            {
                                int idrep = Convert.ToInt32(item);
                                icb_repuestos_retirar existeretirar = db.icb_repuestos_retirar
                                    .Where(d => d.planmayor == planmayor && d.id_repuesto == idrep).FirstOrDefault();
                                if (existeretirar == null)
                                {
                                    icb_repuestos_retirar nuevoreti = new icb_repuestos_retirar
                                    {
                                        id_repuesto = idrep,
                                        planmayor = planmayor
                                    };
                                    db.icb_repuestos_retirar.Add(nuevoreti);
                                }
                            }

                            db.SaveChanges();
                        }

                        //traslado todas las referencias que no estén en lista de retiradas.
                        List<int> listaretiradas = db.icb_repuestos_retirar.Where(d => d.planmayor == planmayor)
                            .Select(d => d.id_repuesto).ToList();

                        List<tsolicitudrepuestosot> listatraslado = db.tsolicitudrepuestosot.Where(d =>
                            d.idorden == existeot.id && !listaretiradas.Contains(d.tdetallerepuestosot.id)).ToList();
                        vpedido pedidox = db.vpedido.Where(d => d.id == idpedido).FirstOrDefault();
                        pedidox.planmayor = planmayor;
                        db.Entry(pedidox).State = EntityState.Modified;
                        //verifico que por cada una de ellas, no exista ya en vpedido la misma referencia y cantidad
                        foreach (tsolicitudrepuestosot item in listatraslado)
                        {
                            vpedrepuestos existeenpedido = db.vpedrepuestos.Where(d =>
                                d.pedido_id == idpedido && d.referencia == item.idrepuesto &&
                                d.cantidad == item.canttraslado).FirstOrDefault();
                            if (existeenpedido != null)
                            {
                                existeenpedido.instalado = true;
                                db.Entry(existeenpedido).State = EntityState.Modified;
                            }
                            else
                            {
                                vpedrepuestos nuevoaccesorio = new vpedrepuestos
                                {
                                    referencia = item.idrepuesto,
                                    cantidad = item.canttraslado,
                                    facturado = false,
                                    fec_creacion = DateTime.Now,
                                    instalado = true,
                                    obsequio = false,
                                    vrunitario = item.tdetallerepuestosot.valorunitario,
                                    pedido_id = idpedido.Value,
                                    vrtotal = item.tdetallerepuestosot.valorunitario * item.canttraslado
                                };
                                db.vpedrepuestos.Add(nuevoaccesorio);
                            }
                        }

                        int guardar = db.SaveChanges();
                        if (guardar > 0)
                        {
                            valor = 1;
                            resultado = "Accesorios cargados a pedido";
                        }
                    }
                    else
                    {
                        valor = 1;
                        resultado = "El plan mayor no tiene repuestos desde exhibicion";
                    }
                }
                else
                {
                    resultado = "El plan mayor no corresponde a un vehículo registrado";
                }
            }
            else
            {
                resultado = "No se suministró un plan mayor";
            }

            var data = new
            {
                valor,
                resultado
            };

            return Json(data);
        }

        public decimal calculardescuentore(decimal? valor, int? cantidad, decimal? pordescuento)
        {
            decimal respuesta = 0;
            if (valor != null && cantidad != null && pordescuento != null)
            {
                respuesta = valor.Value * cantidad.Value * pordescuento.Value / 100;
            }

            return respuesta;
        }

        public decimal calcularivare(decimal? valor, int? cantidad, decimal? pordescuento, decimal? poriva)
        {
            decimal respuesta = 0;
            decimal respuesta2 = 0;
            if (valor != null && poriva != null)
            {
                if (pordescuento != null)
                {
                    respuesta2 = valor.Value * cantidad.Value * pordescuento.Value / 100;
                    respuesta = (valor.Value * cantidad.Value - respuesta2) * poriva.Value / 100;
                }
                else
                {
                    respuesta = valor.Value * cantidad.Value * poriva.Value / 100;
                }
            }

            return respuesta;
        }

        public void EliminarRepuestos(int id)
        {
            vpedrepuestos dato = db.vpedrepuestos.Find(id);
            db.Entry(dato).State = EntityState.Deleted;
            db.SaveChanges();
        }

        public void EliminarRetomas(int id)
        {
            vpedretoma dato = db.vpedretoma.Find(id);
            db.Entry(dato).State = EntityState.Deleted;
            db.SaveChanges();
        }

        public void EliminarFpagos(int id)
        {
            vpedpago dato = db.vpedpago.Find(id);
            string condicion = db.vformapago.Where(x => x.id == dato.condicion).FirstOrDefault().descripcion;
            if (condicion == "Credito")
            {
                v_creditos credito = db.v_creditos.Where(x => x.financiera_id == dato.banco && x.pedido == dato.idpedido && x.estadoc == "D" && x.vaprobado == dato.valor).FirstOrDefault();
                if (credito != null)
                {
                    credito.estadoc = "A";
                    credito.pedido = null;
                    db.Entry(credito).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            db.Entry(dato).State = EntityState.Deleted;
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

        private bool CheckFileType(string fileName)
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
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

            var buscarFavoritosSeleccionados = (from favoritos in db.favoritos
                                                join menu2 in db.Menus
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

        /// <summary>
        ///     Se crea una función para envio a tramites
        /// </summary>
        /// <param name="planmayor"></param>
        /// <returns></returns>
        public JsonResult eventVehi(string planmayor)
        {
            icb_vehiculo vehiculo = db.icb_vehiculo.Where(d => d.plan_mayor != null).FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(planmayor))
            {
                icb_sysparameter eTramite = db.icb_sysparameter.Where(d => d.syspar_cod == "P63").FirstOrDefault();
                int enviarTramite = eTramite != null ? Convert.ToInt32(eTramite.syspar_value) : 19;

                //se captura el plan mayor del vehicullo
                string plan_mayor = vehiculo.plan_mayor;
                //vehiculo.plan_mayor = enviarTramite;
            }

            return Json(planmayor);
        }

        [HttpPost]
        public JsonResult matriculados()
        {
            var info = (from mat in db.vw_Matriculas_Gestionadas select new { mat }).ToList();
            var data = info.Select(x => new
            {
                x.mat.numero,
                //year = x.mat.anio_vh != null ? x.mat.anio_vh.ToString() : "",
                year = x.mat.anio_vh.ToString(),
                model = x.mat.modelo != null ? x.mat.modelo : "",
                color = x.mat.colvh_nombre != null ? x.mat.colvh_nombre : "",
                plan = x.mat.plan_mayor != null ? x.mat.plan_mayor : "",
                placa = x.mat.plac_vh != null ? x.mat.plac_vh : "",
                vin = x.mat.vin != null ? x.mat.vin : "",
                tramitador = x.mat.tramitador_id != null ? x.mat.tramitadorpri_nombre + " " + x.mat.tramitador_apellidos : "",
                cartera = x.mat.cartera != null ? x.mat.cartera.ToString() : "",

                

                fecha = x.mat.fecmatricula != null
                    ? x.mat.fecmatricula.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                    : "",
                city = x.mat.ciudadplaca != null ? x.mat.ciudadplaca.ToString() : "",
                observation = x.mat.evento_observacion != null ? x.mat.evento_observacion.ToString() : "",
                nd = x.mat.idencabezado != null ? x.mat.idencabezado : 0
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public class eventosFaltantes
        {
            public int id { get; set; }
            public string nombreEvento { get; set; }
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