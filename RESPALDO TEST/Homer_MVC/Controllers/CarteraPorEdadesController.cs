using Homer_MVC.IcebergModel;
using Homer_MVC.ViewModels.medios;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

//laura
namespace Homer_MVC.Controllers
{
    public class CarteraPorEdadesController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        private readonly CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
        CultureInfo miCultura = new CultureInfo("is-IS");

        // GET: CarteraPorEdades
        public ActionResult CarteraPorEdades()
        {
            return View();
        }

        public JsonResult CuentasCartera()
        {
            var cuentas = (from a in context.cuenta_puc
                           where a.cuentacartera
                           select new
                           {
                               a.cntpuc_id,
                               a.cntpuc_numero,
                               a.cntpuc_descp
                           }).ToList();

            return Json(cuentas, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ListarCuentasCarteraResumen(string[] param_3, string[] vh)
        {
            System.Linq.Expressions.Expression<Func<vw_cuenta_cartera_fechasV, bool>> predicate = PredicateBuilder.True<vw_cuenta_cartera_fechasV>(); //and
            System.Linq.Expressions.Expression<Func<vw_cuenta_cartera_fechasV, bool>> predicate2 = PredicateBuilder.False<vw_cuenta_cartera_fechasV>(); //or

            if (param_3.Count() > 0 && param_3[0] != "")
            {
                //separar lo que me traigo en una lista
                foreach (string j in param_3)
                {
                    int con = Convert.ToInt32(j);
                    //buscar numero de cuenta
                    cuenta_puc cuenta = context.cuenta_puc.Where(d => d.cntpuc_id == con).FirstOrDefault();
                    if (cuenta != null)
                    {
                        predicate2 = predicate2.Or(x => x.cntpuc_numero.StartsWith(cuenta.cntpuc_numero));
                    }
                }
                predicate = predicate.And(predicate2);


            }

            if (vh.Count() > 0 && vh[0] != "")
            {
                //separar lo que me traigo en una lista
                foreach (string i in vh)
                {
                    int con = Convert.ToInt32(i);
                    if (con == 1)
                    {
                        predicate2 = predicate2.Or(x => x.entrega_vehiculo == null);
                    }
                    else
                    {
                        predicate2 = predicate2.Or(x => x.entrega_vehiculo != null);
                    }
                }

                predicate = predicate.And(predicate2);
            }

            List<vw_cuenta_cartera_fechasV> cuentaCartera = context.vw_cuenta_cartera_fechasV.Where(predicate).ToList();

            var data = cuentaCartera.GroupBy(d => new { d.cuenta, d.cntpuc_numero, V = d.entrega_vehiculo != null ? 1 : 0 }).Select(d => new
            {
                d.Key.cuenta,
                facturado = d.Key.V,
                d.Key.cntpuc_numero,
                sin_vencer = d.Sum(e => e.sin_vencer),
                mayor_cientoveinte = d.Sum(e => e.mayor_cientoveinte),
                valor_aplicado = d.Sum(e => e.valor_aplicado),
                vencido_sesentauno_a_noventa = d.Sum(e => e.vencido_sesentauno_a_noventa),
                vencido_treintauno_a_sesenta = d.Sum(e => e.vencido_treintauno_a_sesenta),
                vencido_noventauno_a_cientoveinte = d.Sum(e => e.vencido_noventauno_a_cientoveinte),
                vencido_uno_a_treinta = d.Sum(e => e.vencido_uno_a_treinta),
                //--FechaEntrega = d.entrega_vehiculo != null ? d.entrega_vehiculo.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                //d.vencimiento
            }).ToList();

            if (data != null)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    decimal? Contador = data[i].sin_vencer;
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonResult ListarCuentasCartera(string[] param_3, string[] vh)
        {
            System.Linq.Expressions.Expression<Func<vw_cuenta_cartera_fechasV, bool>> predicate = PredicateBuilder.True<vw_cuenta_cartera_fechasV>(); //and
            System.Linq.Expressions.Expression<Func<vw_cuenta_cartera_fechasV, bool>> predicate2 = PredicateBuilder.False<vw_cuenta_cartera_fechasV>(); //or

            if (param_3.Count() > 0 && param_3[0] != "")
            {
                //separar lo que me traigo en una lista
                foreach (string j in param_3)
                {
                    int con = Convert.ToInt32(j);
                    predicate2 = predicate2.Or(x => x.cuenta == con);
                }

                predicate = predicate.And(predicate2);
            }

            if (vh.Count() > 0 && vh[0] != "")
            {
                //separar lo que me traigo en una lista
                foreach (string i in vh)
                {
                    int con = Convert.ToInt32(i);
                    if (con == 1)
                    {
                        predicate2 = predicate2.Or(x => x.factura == con);
                    }
                    else
                    {
                        predicate2 = predicate2.Or(x => x.entrega_vehiculo != null);
                    }
                }

                predicate = predicate.And(predicate2);
            }

            List<vw_cuenta_cartera_fechasV> cuentaCartera = context.vw_cuenta_cartera_fechasV.Where(predicate).ToList();

            var data = cuentaCartera.Select(d => new
            {
                d.cuenta,
                d.cntpuc_numero,
                d.sin_vencer,
                d.mayor_cientoveinte,
                d.valor_aplicado,
                d.vencido_sesentauno_a_noventa,
                d.vencido_treintauno_a_sesenta,
                d.vencido_noventauno_a_cientoveinte,
                d.vencido_uno_a_treinta,
                FechaEntrega = d.entrega_vehiculo != null ? d.entrega_vehiculo.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                d.vencimiento
            }).ToList();

            if (data != null)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    decimal? Contador = data[i].sin_vencer;
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult listarTerceros(string cuenta, int? facturado)
        {
            System.Linq.Expressions.Expression<Func<vw_cuenta_cartera_fechasV, bool>> predicate = PredicateBuilder.True<vw_cuenta_cartera_fechasV>(); //and

            predicate = predicate.And(d => d.cntpuc_numero == cuenta);
            if (facturado == 1)
            {
                predicate = predicate.And(x => x.entrega_vehiculo != null);
            }
            else
            {
                predicate = predicate.And(x => x.entrega_vehiculo == null);
            }
            List<vw_cuenta_cartera_fechasV> lista = context.vw_cuenta_cartera_fechasV.Where(predicate).ToList();
            var data = lista.Select(d => new
            {
                d.cuenta,
                d.cntpuc_numero,
                d.sin_vencer,
                d.mayor_cientoveinte,
                d.valor_aplicado,
                d.vencido_sesentauno_a_noventa,
                d.vencido_treintauno_a_sesenta,
                d.vencido_noventauno_a_cientoveinte,
                d.vencido_uno_a_treinta,
                d.vencimiento,
                d.doc_tercero,
                d.nombreTercero,
                d.numPedido
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult detalleTercero(string tercero, string cuenta)
        {
            var data = (from d in context.vw_cuenta_cartera_fechasV
                        where d.doc_tercero == tercero && d.cntpuc_numero == cuenta
                        select new
                        {
                            d.cuenta,
                            d.prefijo,
                            d.tpdoc_nombre,
                            d.numero,
                            d.valor_aplicado,
                            d.valor_total,
                            d.doc_tercero,
                            d.nombreTercero,
                            total = d.valor_total - d.valor_aplicado
                        }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Cartera Facturacion
        public ActionResult CarteraFacturacion(int? id_cliente, long? cedula)
        {
            ViewBag.cedula = cedula;
            ViewBag.documentoCliente = cedula;
            ViewBag.id_cliente = id_cliente;
            if (cedula != null)
            {
                string numcedula = cedula.ToString();
                var buscarCliente = (from t in context.icb_terceros
                                     join c in context.tercero_cliente
                                         on t.tercero_id equals c.tercero_id
                                     where t.doc_tercero == numcedula
                                     select new
                                     {
                                         nombre = t.prinom_tercero != null
                                             ? t.doc_tercero + " - " + t.prinom_tercero + " " + t.apellido_tercero + " " +
                                               t.segapellido_tercero
                                             : t.doc_tercero + " - " + t.razon_social,
                                         id = t.tercero_id,
                                         t.doc_tercero,
                                     }).FirstOrDefault();
                if (buscarCliente != null)
                {
                    ViewBag.nombreCliente = buscarCliente.nombre;
                }
                else
                {
                    ViewBag.nombreCliente = "";
                }
            }
            else
            {
                ViewBag.nombreCliente = "";

            }
            return View();
        }

        public JsonResult listaTercerosFacturas()
        {
            var data = (from d in context.vw_cartera_factura_inicio
                        select new
                        {
                            d.nit,
                            d.doc_tercero,
                            nombreTercero = d.tipotercero != 1
                                ? "(" + d.doc_tercero + ") " + d.prinom_tercero + " " + d.segnom_tercero + " " +
                                  d.apellido_tercero + " " + d.segapellido_tercero
                                : "(" + d.doc_tercero + ") " + d.razon_social
                        }).Distinct().OrderBy(x => x.nombreTercero).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // Browser Inicial
        public JsonResult ListarTodosCartera()
        {
            var data2 = (from a in context.vw_cartera_factura_inicio
                             //where a.tercero_estado == true
                         select new
                         {
                             a.nit,
                             a.doc_tercero,
                             nombre = a.tipotercero == 1
                                 ? "(" + a.doc_tercero + ") " + a.razon_social
                                 : "(" + a.doc_tercero + ") " + a.prinom_tercero + " " + a.segnom_tercero + " " +
                                   a.apellido_tercero + " " + a.segapellido_tercero,
                             a.valoraplicado,
                             a.valortotal,
                             a.saldo
                         }).ToList();
            var data = data2.Select(d => new
            {
                d.nit,
                d.doc_tercero,
                d.nombre,
                valoraplicado = d.valoraplicado != null ? d.valoraplicado.Value.ToString("N2") : "",
                valortotal = d.valortotal != null ? d.valortotal.Value.ToString("N2") : "",
                saldo = d.saldo != null ? d.saldo.Value.ToString("N2") : "0"
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult listarTerceros2(int nit, int? encab_id)
        {
            if (nit > 0)
            {
                List<valores> lista = new List<valores>();
                var buscarFacturas = (from a in context.encab_documento
                                      join b in context.tp_doc_registros
                                          on a.tipo equals b.tpdoc_id
                                      where a.nit == nit && b.sw == 3
                                      select new
                                      {
                                          a.idencabezado,
                                          b.prefijo,
                                          b.tpdoc_nombre,
                                          a.numero,
                                          a.fecha,
                                          a.vencimiento,
                                          a.nit,
                                          a.valor_total,
                                          b.sw
                                      }).ToList();

                foreach (var item in buscarFacturas)
                {
                    var buscarNotas = (from a in context.encab_documento
                                       join b in context.tp_doc_registros
                                           on a.tipo equals b.tpdoc_id
                                       where a.prefijo == item.idencabezado && (b.sw == 1013 || b.sw == 1014)
                                       select new
                                       {
                                           id = a.prefijo,
                                           b.sw,
                                           valor = a.valor_total,
                                           a.nit
                                       }).ToList();
                    if (buscarNotas.Count == 0)
                    {
                        lista.Add(new valores
                        {
                            nit = item.nit,
                            id = item.idencabezado,
                            valor = item.valor_total,
                            sw = item.sw,
                            prefijo = context.encab_documento.FirstOrDefault(x => x.idencabezado == item.idencabezado)
                                .tp_doc_registros.prefijo,
                            documento = context.encab_documento.FirstOrDefault(x => x.idencabezado == item.idencabezado)
                                .tp_doc_registros.tpdoc_nombre,
                            numero = Convert.ToString(context.encab_documento
                                .FirstOrDefault(x => x.idencabezado == item.idencabezado).numero),
                            fecha = context.encab_documento.FirstOrDefault(x => x.idencabezado == item.idencabezado)
                                .fecha,
                            vencimiento = context.encab_documento
                                .FirstOrDefault(x => x.idencabezado == item.idencabezado).vencimiento,
                            valorFactura = Convert.ToDecimal(context.encab_documento
                                .FirstOrDefault(x => x.idencabezado == item.idencabezado).valor_total,miCultura)
                        });
                    }

                    foreach (var item2 in buscarNotas)
                    {
                        lista.Add(new valores
                        {
                            nit = item2.nit,
                            id = item2.id,
                            valor = item2.valor,
                            sw = item2.sw,
                            prefijo = context.encab_documento.FirstOrDefault(x => x.idencabezado == item2.id)
                                .tp_doc_registros.prefijo,
                            documento = context.encab_documento.FirstOrDefault(x => x.idencabezado == item2.id)
                                .tp_doc_registros.tpdoc_nombre,
                            numero = Convert.ToString(context.encab_documento
                                .FirstOrDefault(x => x.idencabezado == item2.id).numero),
                            fecha = context.encab_documento.FirstOrDefault(x => x.idencabezado == item2.id).fecha,
                            vencimiento = context.encab_documento.FirstOrDefault(x => x.idencabezado == item2.id)
                                .vencimiento,
                            valorFactura = Convert.ToDecimal(context.encab_documento
                                .FirstOrDefault(x => x.idencabezado == item2.id).valor_total, miCultura)
                        });
                    }
                }

                var agrupar = lista.GroupBy(x => new { x.id, x.documento }).Select(grp => new
                {
                    id = grp.Select(x => x.id).FirstOrDefault(),
                    nit = grp.Select(x => x.nit).FirstOrDefault(),
                    documento = "(" + grp.Select(a => a.prefijo).FirstOrDefault() + ") " +
                                  grp.Select(a => a.documento).FirstOrDefault(),
                    numero = grp.Select(a => a.numero).FirstOrDefault(),
                    fecha = grp.Select(a => a.fecha).FirstOrDefault().Value.ToString("yyyy/MM/dd HH:mm"),
                    vencimiento = grp.Select(a => a.vencimiento).FirstOrDefault().Value.ToString("yyyy/MM/dd HH:mm"),
                    valorT = grp.Select(a => a.valorFactura).FirstOrDefault().ToString("0,0", elGR),
                    valorA = (grp.Where(v => v.sw == 1013).Sum(a => a.valor) -
                                grp.Where(v => v.sw == 1014).Sum(a => a.valor)).ToString("0,0", elGR),
                    saldo = (grp.Select(a => a.valorFactura).FirstOrDefault() +
                               grp.Where(v => v.sw == 1013).Sum(a => a.valor) -
                               grp.Where(v => v.sw == 1014).Sum(a => a.valor)).ToString("0,0", elGR)
                }).ToList();

                return Json(agrupar, JsonRequestBehavior.AllowGet);
            }

            return Json(JsonRequestBehavior.AllowGet);
        }

        public JsonResult ListartercerosCartera(string param_3)
        {
            if (string.IsNullOrEmpty(param_3) != true)
            //if (param_3 != null)/*(param_3 > 0)*/
            {
                int nit = Convert.ToInt32(param_3);
                var data2 = (from a in context.vw_cartera_factura_inicio
                             where a.nit == nit
                             select new
                             {
                                 a.nit,
                                 a.doc_tercero,
                                 nombre = a.tipotercero == 1
                                     ? "(" + a.doc_tercero + ")" + a.razon_social
                                     : "(" + a.doc_tercero + ")" + a.prinom_tercero + " " + a.segnom_tercero + " " +
                                       a.apellido_tercero + " " + a.segapellido_tercero,
                                 a.valoraplicado,
                                 a.valortotal,
                                 a.saldo
                             }).ToList();
                var data = data2.Select(d => new
                {
                    d.nit,
                    d.doc_tercero,
                    d.nombre,
                    valoraplicado = d.valoraplicado != null ? d.valoraplicado.Value.ToString("N2") : "",
                    valortotal = d.valortotal != null ? d.valortotal.Value.ToString("N2") : "",
                    saldo = d.saldo != null ? d.saldo.Value.ToString("N2") : "0"
                }).ToList();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var data2 = (from a in context.vw_cartera_factura_inicio
                             select new
                             {
                                 a.nit,
                                 a.doc_tercero,
                                 nombre = a.tipotercero == 1
                                     ? "(" + a.doc_tercero + ")" + a.razon_social
                                     : "(" + a.doc_tercero + ")" + a.prinom_tercero + " " + a.segnom_tercero + " " +
                                       a.apellido_tercero + " " + a.segapellido_tercero,
                                 a.valoraplicado,
                                 a.valortotal,
                                 a.saldo
                             }).ToList();
                var data = data2.Select(d => new
                {
                    d.nit,
                    d.doc_tercero,
                    d.nombre,
                    valoraplicado = d.valoraplicado != null ? d.valoraplicado.Value.ToString("N2") : "",
                    valortotal = d.valortotal != null ? d.valortotal.Value.ToString("N2") : "",
                    saldo = d.saldo != null ? d.saldo.Value.ToString("N2") : "0"
                }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult detalleDocumentos(int nit, int numero)
        {
            var buscar = (from a in context.encab_documento
                          join b in context.tp_doc_registros
                              on a.tipo equals b.tpdoc_id
                          where a.prefijo == numero
                          select new
                          {
                              documento = "(" + b.prefijo + ") " + b.tpdoc_nombre,
                              factura = context.encab_documento.FirstOrDefault(x => x.idencabezado == numero).numero,
                              descripcion = a.nota1,
                              fechaDoc = a.fecha,
                              valorDoc = a.valor_total,
                              a.numero
                          }).ToList();

            var data = buscar.Select(x => new
            {
                tdoc = x.documento,
                numeroaplicaCruce = x.factura,
                nota1 = x.descripcion != null ? x.descripcion : "",
                fechaCruce = x.fechaDoc.ToString("yyyy/MM/dd HH:mm"),
                valorCruce = x.valorDoc.ToString("0,0", elGR),
                x.numero
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ModalCruce(int id_encabezado)
        {
            vw_movimiento_clientes rc = context.vw_movimiento_clientes.Where(x => x.idencabezado == id_encabezado).FirstOrDefault();

            return Json(rc, JsonRequestBehavior.AllowGet);
        }

        public JsonResult nuevoBuscador(int? id_tercero)
        {
            var buscar = (from a in context.encab_documento
                          join b in context.tp_doc_registros
                              on a.tipo equals b.tpdoc_id
                          join c in context.icb_terceros
                              on a.nit equals c.tercero_id
                          //where b.sw == 3
                          select new
                          {
                              c.tercero_id,
                              c.prinom_tercero,
                              c.segnom_tercero,
                              c.apellido_tercero,
                              c.segapellido_tercero,
                              c.razon_social,
                              c.doc_tercero,
                              b.sw,
                              a.idencabezado,
                              a.tipo,
                              a.valor_total,
                              a.prefijo
                          }).ToList();


            if (id_tercero == null)
            {
                var continua = buscar.GroupBy(x => x.tercero_id).Select(grp => new
                {
                    grp.Key,
                    id_encabezado = grp.Where(x => x.sw == 3).Select(x => x.idencabezado).FirstOrDefault(),
                    id_tercero = grp.Where(x => x.sw == 3).Select(x => x.tercero_id).FirstOrDefault(),
                    nombre = "(" + grp.Where(x => x.sw == 3).Select(x => x.doc_tercero).FirstOrDefault() + ") " +
                             grp.Where(x => x.sw == 3).Select(x => x.razon_social).FirstOrDefault() +
                             grp.Where(x => x.sw == 3).Select(x => x.prinom_tercero).FirstOrDefault() + " " +
                             grp.Where(x => x.sw == 3).Select(x => x.segnom_tercero).FirstOrDefault() + " " +
                             grp.Where(x => x.sw == 3).Select(x => x.apellido_tercero).FirstOrDefault() + " " +
                             grp.Where(x => x.sw == 3).Select(x => x.segapellido_tercero).FirstOrDefault(),
                    valorTotalFactura = grp.Where(x => x.sw == 3).Select(x => x.valor_total).Sum() +
                                        grp.Where(a => a.sw == 1013 && a.prefijo != null).Select(x => x.valor_total)
                                            .Sum() - grp.Where(a => a.sw == 1014 && a.prefijo != null)
                                            .Select(x => x.valor_total).Sum(),
                    cantidadFacturas = grp.Where(x => x.sw == 3).Select(x => x.valor_total).Count(),
                    tipo = grp.Where(x => x.sw == 3).Select(x => x.tipo).FirstOrDefault()
                }).Where(x => x.cantidadFacturas > 0).ToList();
                var data = continua.Select(x => new
                {
                    x.id_tercero,
                    x.nombre,
                    valorTotalFactura = x.valorTotalFactura.ToString("0,0", elGR),
                    x.tipo,
                    encab = x.id_encabezado,
                    x.cantidadFacturas
                });

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            if (id_tercero != null)
            {
                decimal nd = context.encab_documento
                    .Where(x => x.nit == id_tercero && x.tp_doc_registros.sw == 1013 && x.prefijo != null)
                    .Sum(a => a.valor_total);
                decimal nc = context.encab_documento
                    .Where(x => x.nit == id_tercero && x.tp_doc_registros.sw == 1014 && x.prefijo != null)
                    .Sum(a => a.valor_total);

                var continua = buscar.Where(x => x.tercero_id == id_tercero).GroupBy(x => x.tercero_id).Select(grp =>
                    new
                    {
                        grp.Key,
                        id_encabezado = grp.Where(x => x.sw == 3).Where(x => x.sw == 3).Select(x => x.idencabezado)
                            .FirstOrDefault(),
                        id_tercero = grp.Where(x => x.sw == 3).Where(x => x.sw == 3).Select(x => x.tercero_id)
                            .FirstOrDefault(),
                        nombre = "(" + grp.Where(x => x.sw == 3).Select(x => x.doc_tercero).FirstOrDefault() + ") " +
                                 grp.Where(x => x.sw == 3).Select(x => x.razon_social).FirstOrDefault() +
                                 grp.Where(x => x.sw == 3).Select(x => x.prinom_tercero).FirstOrDefault() + " " +
                                 grp.Where(x => x.sw == 3).Select(x => x.segnom_tercero).FirstOrDefault() + " " +
                                 grp.Where(x => x.sw == 3).Select(x => x.apellido_tercero).FirstOrDefault() + " " +
                                 grp.Where(x => x.sw == 3).Select(x => x.segapellido_tercero).FirstOrDefault(),
                        valorTotalFactura = grp.Where(x => x.sw == 3).Select(x => x.valor_total).Sum() - nc + nd,
                        cantidadFacturas = grp.Where(x => x.sw == 3).Select(x => x.valor_total).Count(),
                        tipo = grp.Where(x => x.sw == 3).Select(x => x.tipo).FirstOrDefault()
                    }).ToList();
                var data = continua.Select(x => new
                {
                    x.id_tercero,
                    x.nombre,
                    valorTotalFactura = x.valorTotalFactura.ToString("0,0", elGR),
                    x.tipo,
                    encab = x.id_encabezado,
                    x.cantidadFacturas
                });

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(JsonRequestBehavior.AllowGet);
        }

        public JsonResult validarSaldo(string cedula_cliente)
        {
            //id_tipo_doc
            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> predicate = PredicateBuilder.True<vw_movimiento_clientes>();

            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> cliente = PredicateBuilder.False<vw_movimiento_clientes>();

            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> id_tipo_doc = PredicateBuilder.False<vw_movimiento_clientes>();

            if (cedula_cliente != "")
            {
                cliente = cliente.Or(x => x.doc_tercero == cedula_cliente);
                predicate = predicate.And(cliente);
            }

            if (id_tipo_doc != null)
            {
                id_tipo_doc = id_tipo_doc.Or(x => x.id_tipo_doc == 16);
                predicate = predicate.And(id_tipo_doc);
            }

            List<vw_movimiento_clientes> buscar = context.vw_movimiento_clientes.Where(predicate).ToList();

            var datos = buscar.Select(x => new
            {
                x.bodega_id,
                id_enc = x.idencabezado,
                bodega = !string.IsNullOrWhiteSpace(x.bodega) ? x.bodega : "",
                pre = !string.IsNullOrWhiteSpace(x.prefijo) ? x.prefijo : "",
                nro_doc = x.nro_documento.ToString() ?? "",
                td = x.id_tipo_doc,
                cliente = !string.IsNullOrWhiteSpace(x.cliente) ? x.cliente : "",
                saldo = x.valor_total - x.valor_aplicado,
                ValorAplicado = x.valor_aplicado,
                valorTotal = x.valor_total,
                tipo_recibo=x.tipo_recibo,
                fecha = x.fecha.ToString("yyyy/MM/dd"),
            }).ToList();

            var saldoDisponible = datos.Where(d => d.saldo > 0 ).Select(d =>d.id_enc).Count() > 0 ? "Si" : "No";

            var datos2 = datos.Where(d => d.saldo > 0 ).Select(x => new
            {
                x.bodega_id,
                x.id_enc,
                x.bodega,
                x.pre,
                x.nro_doc,
                x.td,
                x.cliente,
                x.saldo,
                x.ValorAplicado,
                x.valorTotal,
                tipo_recibo = x.tipo_recibo,
                x.fecha,
            }).OrderByDescending(x => x.fecha).FirstOrDefault();

            var data = new
            {
                datos2,
                saldoDisponible
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult traerDatosBusqueda(int?[] tipo_cartera, int? id_cliente, string cedula_cliente, int?[] bodega,
            bool? vehiculosFacturados,
            bool? caja_general, DateTime? fechaDesde, DateTime? fechaHasta, int? inhabilitar)
        {
            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> predicate = PredicateBuilder.True<vw_movimiento_clientes>();
            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> cliente = PredicateBuilder.False<vw_movimiento_clientes>();
            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> tp_cartera = PredicateBuilder.False<vw_movimiento_clientes>();
            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> tp_doc = PredicateBuilder.False<vw_movimiento_clientes>();
            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> clasif = PredicateBuilder.False<vw_movimiento_clientes>();
            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> predicate_bodega = PredicateBuilder.False<vw_movimiento_clientes>();
            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> vh_Facturados = PredicateBuilder.False<vw_movimiento_clientes>();
            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> cajaGeneral = PredicateBuilder.False<vw_movimiento_clientes>();


            if (id_cliente != null || cedula_cliente != "")
            {
                cliente = cliente.Or(x => x.tercero_id == id_cliente || x.doc_tercero == cedula_cliente);
                predicate = predicate.And(cliente);
            }

            if (tipo_cartera[0] != null)
            {
                foreach (int? item in tipo_cartera)
                {
                    tp_cartera = tp_cartera.Or(x => x.id_tipo_cartera == item);
                }

                predicate = predicate.And(tp_cartera);
            }

            if (bodega[0] != null)
            {
                foreach (int? item in bodega)
                {
                    predicate_bodega = predicate_bodega.Or(x => x.bodega_id == item);
                }

                predicate = predicate.And(predicate_bodega);
            }

            if (fechaDesde != null && fechaHasta != null)
            {
                fechaHasta = fechaHasta.Value.AddDays(1);
                predicate = predicate.And(x => x.fecha >= fechaDesde && x.fecha <= fechaHasta);
            }
            else
            {
                fechaHasta = DateTime.Now.Date.AddDays(1);
                fechaDesde = DateTime.Now.Date.AddYears(-1);
                predicate = predicate.And(x => x.fecha >= fechaDesde && x.fecha <= fechaHasta);
            }

            if (vehiculosFacturados == true)
            {
                vh_Facturados = vh_Facturados.Or(x => x.id_forma_pago == 1);
                predicate = predicate.And(vh_Facturados);
            }

            if (caja_general == true)
            {
                cajaGeneral = cajaGeneral.Or(x => x.id_tipo_recibo == false);
                predicate = predicate.And(cajaGeneral);
            }

            List<vw_movimiento_clientes> buscar = context.vw_movimiento_clientes.Where(predicate).ToList();

            List<vw_movimiento_clientes> sincruzar = buscar.Where(d => d.cruzado == 0).ToList();
            icb_sysparameter paranc = context.icb_sysparameter.Where(d => d.syspar_cod == "P125").FirstOrDefault();
            int idNc = paranc != null ? Convert.ToInt32(paranc.syspar_value) : 16;
            icb_sysparameter pararc = context.icb_sysparameter.Where(d => d.syspar_cod == "P126").FirstOrDefault();
            int idRc = pararc != null ? Convert.ToInt32(pararc.syspar_value) : 20;
            icb_sysparameter swND = context.icb_sysparameter.Where(d => d.syspar_cod == "P128").FirstOrDefault();
            int swND2 = swND != null ? Convert.ToInt32(swND.syspar_value) : 1013;
            icb_sysparameter swF = context.icb_sysparameter.Where(d => d.syspar_cod == "P129").FirstOrDefault();
            int swF2 = swF != null ? Convert.ToInt32(swF.syspar_value) : 3;
            var datos = buscar.Select(x => new
            {
                x.bodega_id,
                bodega = !string.IsNullOrWhiteSpace(x.bodega) ? x.bodega : "",
                pre = !string.IsNullOrWhiteSpace(x.prefijo) ? x.prefijo : "",
                nro_doc = x.nro_documento.ToString() ?? "",
                placa = !string.IsNullOrWhiteSpace(x.placa) ? x.placa : "",
                fecha = x.fecha.ToString("yyyy/MM/dd"),
                cliente = !string.IsNullOrWhiteSpace(x.cliente) ? x.cliente : "",
                valor_suma = x.debito > 0 ? x.debito : x.credito,
                debito = x.debito > 0 ? "$" + x.debito.ToString("0,0", elGR) : "",
                debitos = x.debito,
                creditos = x.credito,
                factura = x.sw != null ? x.sw : 0,
                credito = x.credito > 0 ? "$" + x.credito.ToString("0,0", elGR) : "",
                valor_sin_aplicar = (x.referencia == null || x.referencia == "") && (x.tipo == idNc || x.tipo == idRc)
                    ? x.credito
                    : 0,
                vr_factura = x.valor_factura > 0 ? "$" + x.valor_factura.ToString("0,0", elGR) : "",
                vr_saldo = x.sw == swND2 || x.sw == swF2
                    ? x.debito - x.credito < 0 ? -1 * (x.debito - x.credito) : x.debito - x.credito
                    : 0,
                btnCruzar = x.tipo == idNc || x.tipo == idRc && x.valor_total - x.valor_aplicado > 0 ? 1 : 0,
                aplicado = x.valor_aplicado >= x.valor_total || x.credito >= x.valor_total ? true : false,
                //saldo = x.tipo == 2 || x.tipo == 4 ? "$" + (x.debito - x.credito).ToString("0,0", elGR) : "",
                //saldo = x.sw == swND2 || x.sw == swF2 ? "$" + calcularsaldo(x.idencabezado, x.sw, Convert.ToInt32(x.debito)) : "",
                saldo = x.valor_total - x.valor_aplicado,
                x.cruzado,
                id_enc = x.idencabezado,
                concepto = !string.IsNullOrWhiteSpace(x.observacion) ? x.observacion : "",
                cartera = !string.IsNullOrWhiteSpace(x.tipo_cartera) ? x.tipo_cartera : "",
                //referencia = !string.IsNullOrWhiteSpace(x.referencia) ? x.referencia : "",
                referencia = buscarReferenciasCruzadas(x.idencabezado),
                forma_pago = !string.IsNullOrWhiteSpace(x.forma_pago) ? x.forma_pago : "",
                td = x.id_tipo_doc,
                recibo_caja = !string.IsNullOrWhiteSpace(x.tipo_recibo) ? x.tipo_recibo : "",
                medioPago=x.mediopago
            }).OrderByDescending(x => x.fecha).Distinct();

            string fechahasta = DateTime.Now.Date.ToString("dd/MM/yyyy");
            string fechadesde = DateTime.Now.AddYears(-1).Date.ToString("dd/MM/yyyy");
            var data = new
            {
                datos,
                fechadesde,
                fechahasta,
                inhabilitar
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public string buscarReferenciasCruzadas(int? idencabezado)
        {
            //busco los tipos de switch 3,13 y 18 en psarámetros de sistema -.-
            icb_sysparameter tipofa = context.icb_sysparameter.Where(d => d.syspar_cod == "P129").FirstOrDefault();
            int facturacion = tipofa != null ? Convert.ToInt32(tipofa.syspar_value) : 3;
            icb_sysparameter debitox = context.icb_sysparameter.Where(d => d.syspar_cod == "P139").FirstOrDefault();
            int debito = debitox != null ? Convert.ToInt32(debitox.syspar_value) : 13;

            icb_sysparameter creditox = context.icb_sysparameter.Where(d => d.syspar_cod == "P140").FirstOrDefault();
            int debitopro = creditox != null ? Convert.ToInt32(creditox.syspar_value) : 18;

            List<int> tipota = new List<int>
            {
                facturacion,
                debito,
                debitopro
            };
            string resultado = "";
            if (idencabezado != null)
            {
                //si el encabezado enviado es válido
                encab_documento busqueda = context.encab_documento.Where(d => d.idencabezado == idencabezado).FirstOrDefault();
                if (busqueda != null)
                {

                    if (!tipota.Contains(busqueda.tp_doc_registros.tp_doc_sw.sw))
                    {

                        //veo con quien lo he cruzado
                        List<cruce_documentos> cruc = context.cruce_documentos.Where(d => d.id_encabezado == busqueda.idencabezado).ToList();
                        if (cruc.Count > 0)
                        {
                            resultado = resultado + "<ul>";
                            foreach (cruce_documentos item in cruc)
                            {
                                if (item.id_encab_aplica != null)
                                {
                                    resultado = resultado + "<li>" + item.tp_doc_registros1.prefijo + " - " + item.numeroaplica + "</li>";
                                }
                            }
                            resultado = resultado + "</ul>";
                        }
                    }
                }
            }
            return resultado;
        }
        public ActionResult excelCartera(string tipo_cartera, int? id_cliente, string cedula_cliente, string bodega,
            bool? vehiculosFacturados,
            bool? caja_general, DateTime? fechaDesde, DateTime? fechaHasta, int? inhabilitar)
        {
            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> predicate = PredicateBuilder.True<vw_movimiento_clientes>();
            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> cliente = PredicateBuilder.False<vw_movimiento_clientes>();
            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> tp_cartera = PredicateBuilder.False<vw_movimiento_clientes>();
            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> tp_doc = PredicateBuilder.False<vw_movimiento_clientes>();
            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> clasif = PredicateBuilder.False<vw_movimiento_clientes>();
            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> predicate_bodega = PredicateBuilder.False<vw_movimiento_clientes>();
            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> vh_Facturados = PredicateBuilder.False<vw_movimiento_clientes>();
            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> cajaGeneral = PredicateBuilder.False<vw_movimiento_clientes>();


            if (id_cliente != null || cedula_cliente != "")
            {
                cliente = cliente.Or(x => x.tercero_id == id_cliente || x.doc_tercero == cedula_cliente);
                predicate = predicate.And(cliente);
            }

            if (!string.IsNullOrWhiteSpace(tipo_cartera) && tipo_cartera != "null")
            {
                string[] cartera = tipo_cartera.Split(',');
                foreach (string item in cartera)
                {
                    int carterita = Convert.ToInt32(item);
                    tp_cartera = tp_cartera.Or(x => x.id_tipo_cartera == carterita);
                }

                predicate = predicate.And(tp_cartera);
            }

            if (!string.IsNullOrWhiteSpace(bodega) && bodega != "null")
            {
                string[] bodega1 = bodega.Split(',');
                foreach (string item in bodega1)
                {
                    int bodeguita = Convert.ToInt32(item);
                    predicate_bodega = predicate_bodega.Or(x => x.bodega_id == bodeguita);
                }

                predicate = predicate.And(predicate_bodega);
            }

            if (fechaDesde != null && fechaHasta != null)
            {
                fechaHasta = fechaHasta.Value.AddDays(1);
                predicate = predicate.And(x => x.fecha >= fechaDesde && x.fecha <= fechaHasta);
            }
            else
            {
                fechaHasta = DateTime.Now.Date.AddDays(1);
                fechaDesde = DateTime.Now.Date.AddYears(-1);
                predicate = predicate.And(x => x.fecha >= fechaDesde && x.fecha <= fechaHasta);
            }

            if (vehiculosFacturados == true)
            {
                vh_Facturados = vh_Facturados.Or(x => x.id_forma_pago == 1);
                predicate = predicate.And(vh_Facturados);
            }

            if (caja_general == true)
            {
                cajaGeneral = cajaGeneral.Or(x => x.id_tipo_recibo == false);
                predicate = predicate.And(cajaGeneral);
            }

            List<vw_movimiento_clientes> buscar = context.vw_movimiento_clientes.Where(predicate).ToList();

            List<vw_movimiento_clientes> sincruzar = buscar.Where(d => d.cruzado == 0).ToList();
            icb_sysparameter paranc = context.icb_sysparameter.Where(d => d.syspar_cod == "P125").FirstOrDefault();
            int idNc = paranc != null ? Convert.ToInt32(paranc.syspar_value) : 16;
            icb_sysparameter pararc = context.icb_sysparameter.Where(d => d.syspar_cod == "P126").FirstOrDefault();
            int idRc = pararc != null ? Convert.ToInt32(pararc.syspar_value) : 20;
            icb_sysparameter swND = context.icb_sysparameter.Where(d => d.syspar_cod == "P128").FirstOrDefault();
            int swND2 = swND != null ? Convert.ToInt32(swND.syspar_value) : 1013;
            icb_sysparameter swF = context.icb_sysparameter.Where(d => d.syspar_cod == "P129").FirstOrDefault();
            int swF2 = swF != null ? Convert.ToInt32(swF.syspar_value) : 3;
            var dataexcel = buscar.Select(x => new
            {
                fecha = x.fecha.ToString("yyyy/MM/dd"),
                x.bodega,
                pre = x.prefijo,
                nro_doc = x.nro_documento.ToString() ?? "",
                debito = x.debito > 0 ? "$" + x.debito.ToString("0,0", elGR) : "",
                credito = x.credito > 0 ? "$" + x.credito.ToString("0,0", elGR) : "",
                vr_factura = x.valor_factura > 0 ? "$" + x.valor_factura.ToString("0,0", elGR) : "",
                //saldo = x.sw == swND2 || x.sw == swF2 ? "$" + calcularsaldo(x.idencabezado, x.sw, Convert.ToInt32(x.debito)) : "",
                saldo = x.valor_total - x.valor_aplicado,
                referencia = x.referencia != null ? x.referencia : "",
                placa = x.placa != null ? x.placa : "",
                x.forma_pago,
                concepto = x.observacion,
                cartera = x.tipo_cartera ?? "",
                recibo_caja = x.tipo_recibo
            }).OrderByDescending(x => x.fecha);

            ExcelPackage excel = new ExcelPackage();
            ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Cartera");
            workSheet.Cells[1, 1].Value = "Cartera Cliente(s)";

            workSheet.Cells[2, 1].Value = "Fecha";
            workSheet.Cells[2, 2].Value = "Bodega";
            workSheet.Cells[2, 3].Value = "Pre";
            workSheet.Cells[2, 4].Value = "Número Dcto";
            workSheet.Cells[2, 5].Value = "Débito";
            workSheet.Cells[2, 6].Value = "Crédito";
            workSheet.Cells[2, 7].Value = "Valor Total";
            workSheet.Cells[2, 8].Value = "Saldo";
            workSheet.Cells[2, 9].Value = "Referencia";
            workSheet.Cells[2, 10].Value = "Placa";
            workSheet.Cells[2, 11].Value = "Forma de Pago";
            workSheet.Cells[2, 12].Value = "Concepto";
            workSheet.Cells[2, 13].Value = "Tipo RC";

            string nombreExcel = "cartera" + DateTime.Now;
            workSheet.Cells[3, 1].LoadFromCollection(dataexcel, false);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;  filename=" + nombreExcel + ".xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }

            return View("informe");
        }

        public class valores
        {
            public int? id { get; set; }
            public decimal valor { get; set; }
            public decimal valorFactura { get; set; }
            public int? sw { get; set; }
            public string documento { get; set; }
            public string prefijo { get; set; }

            public string numero { get; set; }
            public DateTime? fecha { get; set; }
            public DateTime? vencimiento { get; set; }
            public int nit { get; set; }
        }

        /*    public string calcularsaldo(long? id,int? sw,int saldo)
	   {
		    var swND = context.icb_sysparameter.Where(d => d.syspar_cod == "P128").FirstOrDefault();
		    var swND2 = swND != null ? Convert.ToInt32(swND.syspar_value) : 1013;
		    var swF = context.icb_sysparameter.Where(d => d.syspar_cod == "P129").FirstOrDefault();
		    var swF2 = swF != null ? Convert.ToInt32(swF.syspar_value) : 3;
		    var totalFactura = 0;
		    if (id != null && sw != null)
		    {
		        if (sw == swF2 || sw == swND2)
		        {
		            var resultadoFactura2 = (from cd in context.cruce_documentos
		                                     where cd.id_encab_aplica == id
		                                     select new
		                                     {
		                                         cd.valor
		                                     }).ToList();
		            var resultadoFactura = (from  ed in context.encab_documento 
		                                    join cd in context.cruce_documentos 
		                                    on ed.idencabezado equals cd.id_encab_aplica
		                                    join dp in context.documentos_pago
		                                    on cd.id_encabezado equals dp.idtencabezado
		                                    where cd.id_encab_aplica == id 
		                             select new
		                             {
		                                 dp.valor
		                             }).ToList();

		            for (var i = 0; i < resultadoFactura2.Count; i++)
		            {
		                totalFactura += Convert.ToInt32(resultadoFactura2[i].valor);
		            }
		            saldo = saldo - totalFactura;
		            return saldo.ToString();
		        }
		        else
		        {
		            return totalFactura.ToString();

		        }
		    }
		    else {
		        return totalFactura.ToString();
		    }
		    
		}*/
    }
}