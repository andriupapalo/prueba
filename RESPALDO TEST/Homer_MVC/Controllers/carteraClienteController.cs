using Homer_MVC.IcebergModel;
using Homer_MVC.ViewModels.medios;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class carteraClienteController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");
        private readonly CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

        // GET: carteraCliente
        public ActionResult Index()
        {
            var clientes = from t in context.icb_terceros
                           join c in context.tercero_cliente
                               on t.tercero_id equals c.tercero_id
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

            List<SelectListItem> nit = new List<SelectListItem>();

            foreach (var item in clientes)
            {
                nit.Add(new SelectListItem
                {
                    Text = item.cedula,
                    Value = item.tercero_id.ToString()
                    //	Selected = item.tercero_id == vpedidos.nit ? true : false
                });
            }

            ViewBag.nit = nit;

            return View();
        }

        public JsonResult BuscarCliente2(int? cliente)
        {
            if (cliente != null)
            {
                var buscarCliente = (from t in context.icb_terceros
                                     join c in context.tercero_cliente
                                         on t.tercero_id equals c.tercero_id
                                     where t.tercero_id == cliente
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
                    var buscarCupo = context.tercero_cliente.Where(x => x.tercero_id == buscarCliente.id).Select(x => new
                    {
                        cupo = x.cupocredito,
                        fecha_limite = x.fec_cupo_limite
                    }).FirstOrDefault();

                    if (buscarCupo.cupo != null && buscarCupo.fecha_limite != null)
                    {
                        icb_sysparameter swND = context.icb_sysparameter.Where(d => d.syspar_cod == "P102").FirstOrDefault();
                        int swND2 = swND != null ? Convert.ToInt32(swND.syspar_value) : 5;
                        icb_sysparameter swF = context.icb_sysparameter.Where(d => d.syspar_cod == "P103").FirstOrDefault();
                        int swF2 = swF != null ? Convert.ToInt32(swF.syspar_value) : 17;
                        decimal totalFactura = 0;
                        decimal? saldoCupo = 0;
                        fpago_tercero fpcontado = context.fpago_tercero.Where(x => x.dvencimiento == 0).FirstOrDefault();

                        var totalfactura2 = context.encab_documento
                            .Where(x => (x.tipo == swND2 || x.tipo == swF2) && x.fpago_id != fpcontado.fpago_id && x.nit == buscarCliente.id
                             && (x.usa_cupo == true || x.detalle_formas_pago_orden.Where(d => d.idformas_pago == 7).Count() > 0)
                             ).Select(d => new { valor_total = d.valor_cupo, valor_aplicado = d.valor_cupo_aplicado }).ToList();
                        if (totalfactura2.Count() > 0)
                        {
                            totalFactura = totalfactura2.Select(d => d.valor_total - d.valor_aplicado).Sum();
                        }

                        saldoCupo = buscarCupo.cupo - totalFactura;
                        //var facturado2 = context.vw_cartera_general.Where(x => x.tercero_id == buscarCliente.id).ToList();
                        //decimal facturado = 0;
                        //decimal facturadoRepuestos = 0;
                        //decimal nd = 0;
                        //decimal nc = 0;
                        //decimal rc = 0;

                        //if (facturado2.Where(x => x.tercero_id == buscarCliente.id && (x.id_tipo_doc == 2 || x.id_tipo_doc == 4)).Count() > 0)
                        //{
                        //	facturado = facturado2.Where(x => x.tercero_id == buscarCliente.id && (x.id_tipo_doc == 2 || x.id_tipo_doc == 4)).Select(x => x.vr_factura).Sum();
                        //    facturadoRepuestos = facturado2.Where(x => x.tercero_id == buscarCliente.id && (x.id_tipo_doc == 4)).Select(x => x.vr_factura).Sum();
                        //   }
                        //else
                        //{
                        //	facturado = 0;
                        //	facturadoRepuestos = 0;
                        //}
                        //if (facturado2.Where(x => x.tercero_id == buscarCliente.id && (x.id_tipo_doc == 20)).Count() > 0)
                        //{
                        //	nc = facturado2.Where(x => x.tercero_id == buscarCliente.id && (x.id_tipo_doc == 20)).Select(x => x.vr_factura).Sum();
                        //}
                        //else
                        //{
                        //	nc = 0;
                        //}
                        //if (facturado2.Where(x => x.tercero_id == buscarCliente.id && (x.id_tipo_doc == 21)).Count() > 0)
                        //{
                        //	nd = context.vw_cartera_general.Where(x => x.tercero_id == buscarCliente.id && (x.id_tipo_doc == 21)).Select(x => x.vr_factura).Sum();
                        //}
                        //else
                        //{
                        //	nd = 0;
                        //}

                        //if (facturado2.Where(x => x.tercero_id == buscarCliente.id && (x.id_tipo_doc == 16)).Count() > 0)
                        //{
                        //	rc = context.vw_cartera_general.Where(x => x.tercero_id == buscarCliente.id && (x.id_tipo_doc == 16)).Select(x => x.vr_factura).Sum();
                        //}
                        //else
                        //{
                        //	nc = 0;
                        //}
                        //var cupo = buscarCupo.Select(x => new
                        //{
                        //	cupo = x.cupo != null ? x.cupo.Value.ToString("0,0", elGR) : "0",
                        //	fecha_vence = x.fecha_limite != null ? x.fecha_limite.Value.ToString("yyyy/MM/dd") : "Sin fecha",
                        //	dias = (x.fecha_limite - DateTime.Now.Date).Value.TotalDays,
                        //	//saldo = x.cupo != null ? (x.cupo + nd ).Value.ToString("0,0", elGR) : "0",
                        //	//saldo = x.cupo != null ? (x.cupo + nd - nc - rc).Value.ToString("0,0", elGR) : "0",
                        //	saldo = x.cupo != null ? (x.cupo - nd - facturadoRepuestos + rc + nc).Value.ToString("0,0", elGR) : "0",
                        //});
                        var cupo = new
                        {
                            cupo = buscarCupo.cupo != null ? buscarCupo.cupo.Value.ToString("0,0", elGR) : "0",
                            fecha_vence = buscarCupo.fecha_limite != null
                                ? buscarCupo.fecha_limite.Value.ToString("yyyy/MM/dd")
                                : "Sin fecha",
                            dias = (buscarCupo.fecha_limite - DateTime.Now.Date).Value.TotalDays,
                            saldo = saldoCupo.Value.ToString("0,0", elGR)
                        };

                        return Json(new { buscarCliente, cupo }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { buscarCliente, cupo = "" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { buscarCliente, cupo = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult buscarTiposCartera()
        {
            var buscar = context.Tipos_Cartera.Select(x => new
            {
                value = x.id,
                text = x.descripcion
            }).ToList();

            return Json(buscar, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarBodegas()
        {
            var buscar = context.bodega_concesionario.Where(x => x.bodccs_estado).Select(x => new
            {
                value = x.id,
                text = x.bodccs_nombre
            }).OrderBy(x => x.text).ToList();

            return Json(buscar, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarClasificaccion()
        {
            var buscar = context.rtipocliente.Select(x => new
            {
                value = x.id,
                text = x.descripcion
            }).OrderBy(x => x.text).ToList();

            return Json(buscar, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarTercero(int? doc_tercero)
        {
            if (doc_tercero != null)
            {
                var buscar = context.icb_terceros.Where(x => x.doc_tercero == doc_tercero.ToString()).Select(d => new
                {
                    nombre = d.prinom_tercero + " " + d.segnom_tercero + " " + d.apellido_tercero + " " +
                             d.segapellido_tercero,
                    id = d.tercero_id,
                    telefono = d.telf_tercero != null ? d.telf_tercero : "No registrado",
                    celular = d.celular_tercero != null ? d.celular_tercero : "No registrado"
                }).FirstOrDefault();
                if (buscar == null)
                {
                    return Json(0, JsonRequestBehavior.AllowGet);
                }

                if (buscar != null)
                {
                    var buscarCupo = context.tercero_cliente.Where(x => x.tercero_id == buscar.id).Select(x => new
                    {
                        cupo = x.cupocredito,
                        fecha_limite = x.fec_cupo_limite
                    }).FirstOrDefault();

                    if (buscarCupo.cupo != null && buscarCupo.fecha_limite != null)
                    {
                        //
                        icb_sysparameter swND = context.icb_sysparameter.Where(d => d.syspar_cod == "P102").FirstOrDefault();
                        int swND2 = swND != null ? Convert.ToInt32(swND.syspar_value) : 5;
                        icb_sysparameter swF = context.icb_sysparameter.Where(d => d.syspar_cod == "P103").FirstOrDefault();
                        int swF2 = swF != null ? Convert.ToInt32(swF.syspar_value) : 17;
                        decimal totalFactura = 0;
                        decimal? saldoCupo = 0;
                        fpago_tercero fpcontado = context.fpago_tercero.Where(x => x.dvencimiento == 0).FirstOrDefault();

                        totalFactura = context.encab_documento
                            .Where(x => x.tipo == swND2 || x.tipo == swF2 && x.fpago_id != 0)
                            .Select(x => x.valor_total - x.valor_aplicado).Sum();
                        saldoCupo = buscarCupo.cupo - totalFactura;

                        /*	var facturado2 = context.vw_cartera_general.Where(x => x.tercero_id == buscar.id).ToList();
						decimal facturado = 0;
						decimal nd = 0;
						decimal nc = 0;
						decimal rc = 0;

						if (facturado2.Where(x => x.tercero_id == buscar.id && (x.id_tipo_doc == 2 || x.id_tipo_doc == 4)).Count() > 0)
						{
						    facturado = facturado2.Where(x => x.tercero_id == buscar.id && (x.id_tipo_doc == 2 || x.id_tipo_doc == 4)).Select(x => x.vr_factura).Sum();
						}
						else { facturado = 0; }
						if (facturado2.Where(x => x.tercero_id == buscar.id && (x.id_tipo_doc == 20)).Count() > 0)
						{
						    nc = facturado2.Where(x => x.tercero_id == buscar.id && (x.id_tipo_doc == 20)).Select(x => x.vr_factura).Sum();
						}
						else { nc = 0; }
						if (facturado2.Where(x => x.tercero_id == buscar.id && (x.id_tipo_doc == 21)).Count() > 0)
						{
						    nd = context.vw_cartera_general.Where(x => x.tercero_id == buscar.id && (x.id_tipo_doc == 21)).Select(x => x.vr_factura).Sum();
						}
						else { nd = 0; }

						if (facturado2.Where(x => x.tercero_id == buscar.id && (x.id_tipo_doc == 16)).Count() > 0)
						{
						    rc = context.vw_cartera_general.Where(x => x.tercero_id == buscar.id && (x.id_tipo_doc == 16)).Select(x => x.vr_factura).Sum();
						}
						else { nc = 0; }
						
						var cupo = new 
							{
								cupo = buscarCupo.cupo != null ? buscarCupo.cupo.Value.ToString("0,0", elGR) : "0",
								fecha_vence = buscarCupo.fecha_limite != null ? buscarCupo.fecha_limite.Value.ToString("yyyy/MM/dd") : "Sin fecha",
								dias = (buscarCupo.fecha_limite - DateTime.Now.Date).Value.TotalDays,
								saldo = buscarCupo.cupo != null ? (buscarCupo.cupo - nd - facturado + rc + nc).Value.ToString("0,0", elGR) : "0",
							};*/
                        var cupo = new
                        {
                            cupo = buscarCupo.cupo != null ? buscarCupo.cupo.Value.ToString("0,0", elGR) : "0",
                            fecha_vence = buscarCupo.fecha_limite != null
                                ? buscarCupo.fecha_limite.Value.ToString("yyyy/MM/dd")
                                : "Sin fecha",
                            dias = (buscarCupo.fecha_limite - DateTime.Now.Date).Value.TotalDays,
                            saldo = saldoCupo.Value.ToString("0,0", elGR)
                        };

                        return Json(new { buscar, cupo }, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(new { buscar, cupo = "" }, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarCuantos(string doc_tercero)
        {
            if (!string.IsNullOrWhiteSpace(doc_tercero))
            {
                System.Linq.Expressions.Expression<Func<icb_terceros, bool>> predicado = PredicateBuilder.True<icb_terceros>();
                System.Linq.Expressions.Expression<Func<icb_terceros, bool>> predicado2 = PredicateBuilder.False<icb_terceros>();

                predicado2 = predicado2.Or(d => 1 == 1 && d.doc_tercero.Contains(doc_tercero));
                predicado2 = predicado2.Or(d => 1 == 1 && d.razon_social.Contains(doc_tercero));
                predicado2 = predicado2.Or(d => 1 == 1 && d.prinom_tercero.Contains(doc_tercero));
                predicado2 = predicado2.Or(d => 1 == 1 && d.segnom_tercero.Contains(doc_tercero));
                predicado2 = predicado2.Or(d => 1 == 1 && d.apellido_tercero.Contains(doc_tercero));
                predicado2 = predicado2.Or(d => 1 == 1 && d.segapellido_tercero.Contains(doc_tercero));
                predicado2 = predicado2.Or(d => 1 == 1 && d.telf_tercero.Contains(doc_tercero));
                predicado2 = predicado2.Or(d => 1 == 1 && d.celular_tercero.Contains(doc_tercero));
                predicado2 = predicado2.Or(d => 1 == 1 && d.email_tercero.Contains(doc_tercero));
                predicado = predicado.And(predicado2);

                var terceros = context.icb_terceros.Where(predicado).Select(x => new
                {
                    id = x.tercero_id
                }).ToList();

                int Cuantos = context.icb_terceros.Where(predicado).Count();

                var data = new
                {
                    Cuantos,
                    terceros
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarMuchosClientes(string campo)
        {
            if (campo != null)
            {
                System.Linq.Expressions.Expression<Func<icb_terceros, bool>> predicado = PredicateBuilder.True<icb_terceros>();
                System.Linq.Expressions.Expression<Func<icb_terceros, bool>> predicado2 = PredicateBuilder.False<icb_terceros>();

                predicado2 = predicado2.Or(d => 1 == 1 && d.doc_tercero.Contains(campo));
                predicado2 = predicado2.Or(d => 1 == 1 && d.razon_social.Contains(campo));
                predicado2 = predicado2.Or(d => 1 == 1 && d.prinom_tercero.Contains(campo));
                predicado2 = predicado2.Or(d => 1 == 1 && d.segnom_tercero.Contains(campo));
                predicado2 = predicado2.Or(d => 1 == 1 && d.apellido_tercero.Contains(campo));
                predicado2 = predicado2.Or(d => 1 == 1 && d.segapellido_tercero.Contains(campo));
                predicado2 = predicado2.Or(d => 1 == 1 && d.telf_tercero.Contains(campo));
                predicado2 = predicado2.Or(d => 1 == 1 && d.celular_tercero.Contains(campo));
                predicado2 = predicado2.Or(d => 1 == 1 && d.email_tercero.Contains(campo));
                predicado = predicado.And(predicado2);
                var buscar = context.icb_terceros.Where(predicado).Select(d => new
                {
                    nombre = d.prinom_tercero + " " + d.segnom_tercero + " " + d.apellido_tercero + " " +
                             d.segapellido_tercero,
                    id = d.tercero_id,
                    docu = d.doc_tercero,
                    tel = d.telf_tercero,
                    cel = d.celular_tercero,
                    email = d.email_tercero
                }).ToList();

                var datos = buscar.Select(x => new
                {
                    x.id,
                    x.docu,
                    x.nombre,
                    x.tel,
                    x.cel,
                    x.email
                }).OrderBy(x => x.nombre);

                var muchosclientes = datos.ToList();

                return Json(muchosclientes, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }


        public JsonResult tipoDocumento()
        {
            var buscar = context.vw_cartera_general.Select(x => new
            {
                value = x.id_tipo_doc,
                text = x.pre
            }).OrderBy(x => x.text).ToList().Distinct();

            return Json(buscar, JsonRequestBehavior.AllowGet);
        }

        public JsonResult traerDatosBusqueda(int?[] tipo_cartera, int? id_cliente,
            string cedula_cliente, /*int? tipo_documento, string indicador_documento,*/ int? clasificacion,
            int?[] bodega, bool? vehiculosEntregados,
            bool? sys, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            System.Linq.Expressions.Expression<Func<vw_cartera_general, bool>> predicate = PredicateBuilder.True<vw_cartera_general>();
            System.Linq.Expressions.Expression<Func<vw_cartera_general, bool>> cliente = PredicateBuilder.False<vw_cartera_general>();
            System.Linq.Expressions.Expression<Func<vw_cartera_general, bool>> tp_cartera = PredicateBuilder.False<vw_cartera_general>();
            System.Linq.Expressions.Expression<Func<vw_cartera_general, bool>> tp_doc = PredicateBuilder.False<vw_cartera_general>();
            System.Linq.Expressions.Expression<Func<vw_cartera_general, bool>> clasif = PredicateBuilder.False<vw_cartera_general>();
            System.Linq.Expressions.Expression<Func<vw_cartera_general, bool>> predicate_bodega = PredicateBuilder.False<vw_cartera_general>();
            System.Linq.Expressions.Expression<Func<vw_cartera_general, bool>> vh_entregado = PredicateBuilder.False<vw_cartera_general>();
            System.Linq.Expressions.Expression<Func<vw_cartera_general, bool>> sano_salvo = PredicateBuilder.False<vw_cartera_general>();
            System.Linq.Expressions.Expression<Func<vw_cartera_general, bool>> cartera_null = PredicateBuilder.False<vw_cartera_general>();

            //var predicate = PredicateBuilder.True<vw_cartera_general2>();
            //var cliente = PredicateBuilder.False<vw_cartera_general2>();
            //var tp_cartera = PredicateBuilder.False<vw_cartera_general2>();
            //var tp_doc = PredicateBuilder.False<vw_cartera_general2>();
            //var clasif = PredicateBuilder.False<vw_cartera_general2>();
            //var predicate_bodega = PredicateBuilder.False<vw_cartera_general2>();
            //var vh_entregado = PredicateBuilder.False<vw_cartera_general2>();
            //var sano_salvo = PredicateBuilder.False<vw_cartera_general2>();
            //var cartera_null = PredicateBuilder.False<vw_cartera_general2>();

            if (id_cliente != null || cedula_cliente != "")
            {
                cliente = cliente.Or(x => x.tercero_id == id_cliente || x.doc_tercero == cedula_cliente);
                predicate = predicate.And(cliente);
            }

            if (tipo_cartera[0] != null)
            {
                foreach (int? item in tipo_cartera)
                {
                    tp_cartera = tp_cartera.Or(x => x.id_cartera == item);
                }

                if (id_cliente != null || cedula_cliente != "")
                {
                    tp_cartera = tp_cartera.Or(x => x.id_cartera == null);
                }

                predicate = predicate.And(tp_cartera);
            }

            if (bodega[0] != null)
            {
                foreach (int? item in bodega)
                {
                    predicate_bodega = predicate_bodega.Or(x => x.id_bodega == item);
                }

                predicate = predicate.And(predicate_bodega);
            }

            if (fechaDesde != null && fechaHasta != null)
            {
                DateTime fecha2 = fechaHasta.Value.AddDays(1);
                predicate = predicate.And(x => x.fecha >= fechaDesde && x.fecha <= fecha2);
            }
            else
            {
                fechaHasta = DateTime.Now;
                DateTime fecha2 = fechaHasta.Value.AddDays(1);
                fechaDesde = DateTime.Now.AddYears(-1);
                predicate = predicate.And(x => x.fecha >= fechaDesde && x.fecha <= fecha2);
            }

            List<vw_cartera_general> buscar = context.vw_cartera_general.Where(predicate).OrderBy(x => x.mora).ToList();

            var datos = buscar.Select(x => new
            {
                mora = Convert.ToDecimal(x.nota_debito_aplicada, miCultura) + Convert.ToDecimal(x.vr_factura, miCultura) -
                       Convert.ToDecimal(x.nota_credito_aplicada, miCultura) != 0
                    ? x.mora.ToString()
                    : "",
                bodega_id = x.id_bodega,
                bodega = x.bodccs_nombre,
                pre = x.pre ?? "0",
                nro_doc = x.numero.ToString() ?? "0",
                placa = x.placa != null ? verificarplaca(x.placa) : "",
                fecha = x.fecha.ToString("yyyy/MM/dd"),
                cliente = x.cliente ?? "",
                valor_fac = x.vr_factura,
                vr_factura = x.vr_factura.ToString("0,0", elGR),
                vr_aplicado =
                    (Convert.ToDecimal(x.nota_debito_aplicada, miCultura) - Convert.ToDecimal(x.nota_credito_aplicada, miCultura) -
                     Convert.ToDecimal(x.recibo_caja, miCultura) < 0
                        ? (-1 * (Convert.ToDecimal(x.nota_debito_aplicada, miCultura) -
                                 Convert.ToDecimal(x.nota_credito_aplicada, miCultura) - Convert.ToDecimal(x.recibo_caja, miCultura)))
                        .ToString("0,0", elGR)
                        : (Convert.ToDecimal(x.nota_debito_aplicada, miCultura) - Convert.ToDecimal(x.nota_credito_aplicada, miCultura) -
                           Convert.ToDecimal(x.recibo_caja, miCultura)).ToString("0,0", elGR)) ?? "0",
                //saldo = ((Convert.ToInt32(x.nota_debito_aplicada) + Convert.ToInt32(x.vr_factura) - Convert.ToInt32(x.nota_credito_aplicada) - Convert.ToDecimal(x.recibo_caja)).ToString("0,0", elGR)) ?? "0",
                saldo = x.vr_factura - x.vr_aplicado,
                saldo2 = Convert.ToInt32(x.nota_debito_aplicada) + Convert.ToInt32(x.vr_factura) -
                         Convert.ToInt32(x.nota_credito_aplicada) - Convert.ToDecimal(x.recibo_caja, miCultura),
                debito = x.nota_debito_aplicada,
                credito = x.nota_credito_aplicada,
                f_vence = x.f_vence != null ? x.f_vence.Value.ToString("yyyy/MM/dd") : "Sin fecha Vence",
                pedido = x.numero_pedido != null ? x.numero_pedido.ToString() : "",
                x.observaciones,
                x.vendedor,
                vehiculo = x.vehiculo != null ? x.vehiculo : "",
                cartera = x.cartera ?? "",
                td = x.id_tipo_doc,
                cabeza = x.idencabezado,
                idcliente = x.tercero_id
            }).OrderBy(x => x.mora);

            //var datos = datosS.Select(x => x.td != 20 && x.td != 16 );
            //lista de conceptos que son cartera
            List<int> listacartera = new List<int>
            {
                16, 20
            };

            var datoscartera = datos.Where(d => 1==1 && !listacartera.Contains(d.td)).ToList();
            //}).OrderByDescending(x => x.mora);

            //var datos = new
            //	 {
            //	datosS,
            //	datosT
            //	};

            //return Json(datos, JsonRequestBehavior.AllowGet);
            return Json(datoscartera, JsonRequestBehavior.AllowGet);
        }

        public string verificarplaca(string placa)
        {
            string resultado = "";
            //verifico que placa no venga vacio
            if (!string.IsNullOrWhiteSpace(placa))
            {
                //verifico que sea de un vehículo válido
                icb_vehiculo vei = context.icb_vehiculo.Where(d => d.plan_mayor == placa).FirstOrDefault();
                if (vei != null)
                {
                    if (!string.IsNullOrWhiteSpace(vei.plac_vh))
                    {
                        resultado = vei.plac_vh;
                    }
                    else
                    {
                        resultado = vei.plan_mayor;
                    }
                }
            }

            return resultado;
        }

        public ActionResult buscarSeguimientoCartera(int encabezado)
        {
            var data2 = (from seg in context.seguicarteratercero
                         join user in context.users
                             on seg.user_creacion_id equals user.user_id
                         join tipo in context.vtiposeguimientocot
                             on seg.tipo_id equals tipo.id_tipo_seguimiento
                         join encab in context.encab_documento
                             on seg.encabezado_id equals encab.idencabezado
                         where encab.idencabezado == encabezado
                         select new
                         {
                             tipo.nombre_seguimiento,
                             seg.nota,
                             seg.fecha,
                             nombreUsuario = user.user_nombre + " " + user.user_apellido
                         }
                ).ToList();
            var data = data2.Select(d => new
            {
                d.nombre_seguimiento,
                d.nombreUsuario,
                nota = !string.IsNullOrWhiteSpace(d.nota) ? d.nota : "",
                fecha = d.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
            }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult agregarSeguimiento(int encabezado, int tercero, int tipo, string nota,
            seguicarteratercero entrada)
        {
            object user_id = Session["user_usuarioid"];
            int tipo_seguimiento = tipo;
            string notas = nota;
            bool guardar = false;

            if (!string.IsNullOrWhiteSpace(nota))
            {
                guardar = true;
                entrada.nota = notas;
                entrada.tercero_id = tercero;
                entrada.tipo_id = Convert.ToInt32(tipo_seguimiento);
                entrada.user_creacion_id = Convert.ToInt32(user_id);
                entrada.encabezado_id = encabezado;
                entrada.fecha = DateTime.Now;
                context.seguicarteratercero.Add(entrada);
                context.SaveChanges();
            }
            else
            {
                guardar = false;
            }

            return Json(guardar, JsonRequestBehavior.AllowGet);
        }
    }
}