//using CrystalDecisions.CrystalReports.Engine;

using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class inventario_repuestoController : Controller
    {
        // ticket 4475 fabian vanegas fecha 23-09-2020 10:20 am para filtrar documentos coti repuestos y pedidos se traen de la tabla de parametros
        public int PCotRep=0, PPedRep=0;
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");

        DateTimeFormatInfo usDtfi = new CultureInfo("en-US").DateTimeFormat;

        // GET: inventario_repuesto
        public ActionResult Index(int? menu)
        {
            BuscarFavoritos(menu);
            BuscarParametros();
            return View();
        }

        public void listParametros()
        {
            ReferenciasModel model = new ReferenciasModel();

            var referencia = (from r in context.icb_referencia
                              select new
                              {
                                  r.ref_codigo,
                                  r.ref_descripcion
                              }).ToList();

            List<SelectListItem> listRef = new List<SelectListItem>();
            foreach (var item in referencia)
            {
                listRef.Add(new SelectListItem
                {
                    Text = item.ref_descripcion,
                    Value = item.ref_codigo
                });
            }

            var id1 = Convert.ToInt32(Session["user_usuarioid"]);

            var list = (from u in context.users
                        where u.user_id == id1 //&& u.rol_id == 6
                        select new
                        {
                            u.user_id,
                            u.user_nombre,
                            u.user_apellido
                        }).ToList();

            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in list)
            {
                lista.Add(new SelectListItem
                {
                    Text = item.user_nombre + ' ' + item.user_apellido,
                    Value = item.user_id.ToString()
                });
            }

            var listCotizaciones = (from r in context.icb_referencia_mov
                                    join t in context.icb_terceros
                                        on r.cliente equals t.tercero_id
                                    join tp in context.tp_doc_registros
                                        on r.tpdocid equals tp.tpdoc_id
                                    where tp.tipo == PCotRep && r.habilitar == true
                                    //where tp.tipo == 25 && r.habilitar == true

                                    select new
                                    {
                                        numero = "(" + r.refmov_numero + ")" + " " + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                                 t.apellido_tercero + " " + t.segapellido_tercero,
                                        id = r.refmov_id
                                        //nombre = t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero + " " + t.segapellido_tercero 
                                    }).ToList();

            List<SelectListItem> listaCotizaciones = new List<SelectListItem>();
            foreach (var item in listCotizaciones)
            {
                listaCotizaciones.Add(new SelectListItem
                {
                    Text = item.numero,
                    Value = item.id.ToString()
                });
            }

            var clientes = (from c in context.tercero_cliente
                            join t in context.icb_terceros
                                on c.tercero_id equals t.tercero_id
                            select new
                            {
                                id = c.tercero_id,
                                nombre = "(" + t.doc_tercero + ") " + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                         t.apellido_tercero + " " + t.segapellido_tercero
                            }).ToList();
            List<SelectListItem> listClientes = new List<SelectListItem>();
            foreach (var item in clientes)
            {
                listClientes.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }

            ViewBag.cargarCotizacion = listaCotizaciones;
            ViewBag.proveedor = lista;

            //solo se listan las bodegas donde el usuario logueado tenga login
            var bods = context.bodega_usuario.Where(d => d.id_usuario == id1).GroupBy(d => d.id_bodega).Select(d =>d.Key).ToList();
            ViewBag.bodega_id = new SelectList(context.bodega_concesionario.Where(d=>bods.Contains(d.id)), "id", "bodccs_nombre");
            //pendiente cambio
            //ViewBag.tipoCargue = new SelectList(context.tp_doc_registros.Where(x => x.tipo == 25 || x.tipo == 27),
            ViewBag.tipoCargue = new SelectList(context.tp_doc_registros.Where(x => x.tipo == PCotRep || x.tipo == PPedRep),
                "tpdoc_id", "tpdoc_nombre");

            ViewBag.nit = listClientes;
            ViewBag.tpmovimiento = new SelectList(context.icb_tpmovimiento, "mov_id", "mov_nombre");
            ViewBag.ref_codigo = listRef /*new SelectList(context.icb_referencia, "ref_codigo", "ref_codigo")*/;
            ViewBag.vendedor = new SelectList(context.users, "user_id", "user_nombre");
            ViewBag.condicion = new SelectList(context.fpago_tercero, "fpago_id", "fpago_nombre");

            icb_referencia_mov buscarUltimaCotizacion =
                context.icb_referencia_mov.OrderByDescending(x => x.refmov_id).FirstOrDefault();
            ViewBag.numCotCreado = buscarUltimaCotizacion != null ? buscarUltimaCotizacion.refmov_numero : 0;

            icb_referencia_mov buscarUltimaCotizacionBodega =
                context.icb_referencia_mov.OrderByDescending(x => x.refmov_id).FirstOrDefault();
            ViewBag.bodega_id1 = buscarUltimaCotizacionBodega != null ? buscarUltimaCotizacionBodega.bodega_id : 0;
        }
        public JsonResult QuitarCotizacion(icb_referencia_mov model, int cotizacion)//1
        {
            var result = false;
            //context//1
            var referencia = context.icb_referencia_mov.Find(cotizacion);
            referencia.habilitar = model.habilitar = false;

            context.Entry(referencia).State = EntityState.Modified;
            int resultado = context.SaveChanges();

            if (resultado == 1)
            {
                result = true;
            }
            else {
                result = false;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult crearPDFPedidos(int? id, int? bod)
        {
            //bodega
            //var bod = Convert.ToInt32(Request["selectBodegas"]);

            if (id != null)
            {
                    var buscaPedido = (from pedido in context.icb_referencia_mov
                                       join pedidoDetalle in context.icb_referencia_movdetalle
                                       on pedido.refmov_id equals pedidoDetalle.refmov_id
                                       join bodega in context.bodega_concesionario
                                       on pedido.bodega_id equals bodega.id
                                       join tercero in context.icb_terceros 
                                       on pedido.cliente equals tercero.tercero_id
                                        join ciudadCliente in context.nom_ciudad
                                        on tercero.ciu_id equals ciudadCliente.ciu_id into temp
                                       from j in temp.DefaultIfEmpty()
                                       join ciudad in context.nom_ciudad
                                       on bodega.ciudad_id equals ciudad.ciu_id into temp4
                                       from n in temp4.DefaultIfEmpty()
                                       join departamento in context.nom_departamento
                                       on n.dpto_id equals departamento.dpto_id into temp5
                                       from b in temp5.DefaultIfEmpty()
                                       join cliente in context.tercero_cliente
                                       on tercero.tercero_id equals cliente.tercero_id into temp9
                                       from cli in temp9.DefaultIfEmpty()
                                       join ocupacion in context.tp_ocupacion
                                       on cli.tpocupacion_id equals ocupacion.tpocupacion_id into temp10
                                       from ocu in temp10.DefaultIfEmpty()
                                       join estadoCivil in context.estado_civil
                                       on cli.edocivil_id equals estadoCivil.edocivil_id into temp11
                                       from edoCivil in temp11.DefaultIfEmpty()
                                       join usuario in context.users
                                       on pedido.vendedor equals usuario.user_id into temp12
                                       from vend in temp12.DefaultIfEmpty()
                                       where pedido.refmov_id == id// revisar
                                            select new
                                            {
                                                tercero.prinom_tercero,
                                                tercero.segnom_tercero,
                                                tercero.apellido_tercero,
                                                tercero.segapellido_tercero,
                                                tercero.doc_tercero,
                                                tercero.telf_tercero,
                                                tercero.celular_tercero,
                                                tercero.fec_nacimiento,
                                                tercero.email_tercero,
                                                j.ciu_nombre,
                                                bodega.bodccs_nombre,
                                                pedido.refmov_numero,
                                                //pedido.valor_total,
                                                pedido.valordescuento,
                                                pedido.refmov_fecela,
                                                bodega.bodccs_cod,
                                                bodega.bodccs_direccion,
                                                bodegaNombre = bodega.bodccs_nombre,
                                                ciu_concesionario = n.ciu_nombre,
                                                b.dpto_nombre,
                                                ocu.tpocupacion_nombre,
                                                edoCivil.edocivil_nombre,
                                                vendedor = pedido.vendedor != 0 ? vend.user_nombre + " " + vend.user_apellido : "",
                                                user_numIdent = pedido.vendedor != 0 ? vend.user_numIdent.ToString() : "",
                                                pedido.refmov_id,
                                                pedidoDetalle.ref_codigo,
                                                pedidoDetalle.valor_unitario,
                                                pedidoDetalle.valor_total,
                                                pedidoDetalle.refdet_cantidad,
                                                pedidoDetalle.poriva,
                                                //pedido.cliente,
                                                clienteuser = tercero.tercero_id
                                            }).FirstOrDefault();

                users aprobadoPor = context.users.Where(x => x.user_id == buscaPedido.clienteuser).FirstOrDefault();

                var buscarDireccion = (from pedido in context.vpedido
                                       join tercero in context.icb_terceros
                                           on pedido.nit equals tercero.tercero_id
                                       join direcciones in context.terceros_direcciones
                                           on tercero.tercero_id equals direcciones.idtercero
                                       orderby direcciones.id descending
                                       where pedido.id == id
                                       select new { direcciones.direccion }).FirstOrDefault();

                if (buscaPedido != null)
                {
                    var buscarReferencias = (from pedidoRepuesto in context.vpedrepuestos
                                             join pedido in context.vpedido
                                                 on pedidoRepuesto.pedido_id equals pedido.id
                                             join referencia in context.icb_referencia
                                                 on pedidoRepuesto.referencia equals referencia.ref_codigo into temp1
                                             from refer in temp1.DefaultIfEmpty()
                                             where pedido.id == id
                                             select new
                                             {
                                                 refer.ref_descripcion,
                                                 pedidoRepuesto.vrtotal
                                             }).ToList();

                    var buscarFormaPago = (from pago in context.vpedpago
                                           join formaPago in context.vformapago
                                               on pago.condicion equals formaPago.id
                                           join a in context.vpedido
                                               on pago.idpedido equals a.id
                                           where a.id == id
                                           select new
                                           {
                                               pago.condicion,
                                               pago.valor,
                                               formaPago.tipo
                                           }).ToList();

                    string buscarFinanciera = (from a in context.icb_unidad_financiera
                                               join b in context.vpedpago
                                                   on a.financiera_id equals b.banco
                                               join c in context.vpedido
                                                   on b.idpedido equals c.id
                                               where c.id == id
                                               select a.financiera_nombre).FirstOrDefault();


                    decimal? buscarCredito = (from a in context.vpedpago
                                              join b in context.vpedido
                                                  on a.idpedido equals b.id
                                              where b.id == id && a.condicion == 1
                                              select a.valor).FirstOrDefault();

                    var docCliente = (from pedido in context.vw_pedidos
                                      join pedidos in context.vpedido
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

                    var terceroid = (from cliente in context.icb_terceros
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

                    //tarea 2056
                    //var buscarAsegurados = (from terc in context.icb_terceros
                    //                        where terc.tercero_id == buscaPedido.nit_asegurado ||
                    //                              terc.tercero_id == buscaPedido.nit2 ||
                    //                              terc.tercero_id == buscaPedido.nit3 ||
                    //                              terc.tercero_id == buscaPedido.nit4 ||
                    //                              terc.tercero_id == buscaPedido.nit5
                    //                        select new
                    //                        {
                    //                            documentoAsegurado = terc.doc_tercero,
                    //                            terc.prinom_tercero,
                    //                            terc.apellido_tercero
                    //                        }).ToList();

                    //var buscarVehiculoRetoma = (from retoma in context.vpedretoma
                    //                            where retoma.vpedido.id == id
                    //                            select new
                    //                            {
                    //                                retoma.modelo,
                    //                                retoma.placa,
                    //                                retoma.valor
                    //                            }).FirstOrDefault();

                    var buscarContactoC = (from tercero in context.icb_terceros
                                           join contacto in context.icb_contacto_tercero
                                               on tercero.tercero_id equals contacto.tercero_id
                                           join pedidos in context.vw_pedidos
                                               on tercero.doc_tercero equals pedidos.doc_tercero
                                           join pedido in context.vpedido
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



                    //decimal totalAPagar = buscarVehiculoRetoma != null ? buscarVehiculoRetoma.valor ?? 0 : 0;
                    //totalAPagar += credito + contado;

                    string root = Server.MapPath("~/Pdf/");
                    string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
                    string path = Path.Combine(root, pdfname);
                    path = Path.GetFullPath(path);

                    CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
                    decimal? valor = buscarCredito;

                    decimal total = buscaPedido.valor_total ?? 0;
                    decimal descuento = buscaPedido.valordescuento;
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
                        PedidoCotizacionPDFModel modelo = new PedidoCotizacionPDFModel
                        {

                            //Bodega
                            NombreBodega = buscaPedido.bodccs_nombre,
                            DireccionBodega = buscaPedido.bodccs_direccion,
                            CiudadBodega = buscaPedido.ciu_concesionario,
                            DepartamentoBodega = buscaPedido.dpto_nombre,
                            //Bodega

                            //Cliente
                            NumeroPedido = buscaPedido.refmov_numero,
                            DiaPedido = buscaPedido.refmov_fecela != null ? buscaPedido.refmov_fecela.Day.ToString() : "",
                            MesPedido = buscaPedido.refmov_fecela != null ? buscaPedido.refmov_fecela.Month.ToString() : "",
                            AnioPedido = buscaPedido.refmov_fecela != null ? buscaPedido.refmov_fecela.Year.ToString() : "",
                            NombresCliente = buscaPedido.prinom_tercero + " " + buscaPedido.segnom_tercero + " " +
                                                    buscaPedido.apellido_tercero + " " + buscaPedido.segapellido_tercero,
                            CedulaNit = buscaPedido.doc_tercero,
                            //DireccionCliente = buscarDireccion.direccion,
                            CiudadCliente = buscaPedido.ciu_nombre,
                            TelefonoCliente = buscaPedido.telf_tercero,
                            CelularCliente = buscaPedido.celular_tercero,
                            ProfesionCliente = buscaPedido.tpocupacion_nombre,
                            EstadoCivil = buscaPedido.edocivil_nombre,
                            FechaNacimientoCliente = buscaPedido.fec_nacimiento != null
                                ? buscaPedido.fec_nacimiento.Value.ToShortDateString()
                                : "",
                            EmailCliente = buscaPedido.email_tercero,
                            //NombresAsegurados = nomAsegurados,
                            //DocumentosAsegurados = docAsegurados,
                            //Cliente

                            //Repuestos
                            ModeloVehiculo =buscaPedido.ref_codigo,
                            //Repuestos

                            //Precio
                            PrecioAlPublico = buscaPedido.valor_total != null
                                ? buscaPedido.valor_total.Value.ToString("0,0", elGR)
                                : "0",
                            /*Descuento = buscaPedido.valordescuento != null
                                ? buscaPedido.valordescuento.ToString("0,0", elGR)
                                : "0",*/
                            Descuento = buscaPedido.valordescuento.ToString("0,0", elGR),

                            PrecioVenta = calcularTotal.ToString("0,0", elGR),

                            //Accesorios = accesorios,
                            CuotaInicial = buscaPedido.valor_total.Value.ToString("0,0", elGR),
                            Total = buscaPedido.valor_total.Value.ToString("0,0", elGR),
                            //Precio

                            //Vendedor
                            CedulaVendedor = buscaPedido.user_numIdent,
                            Vendedor = buscaPedido.vendedor,
                            CedulaAprobado = aprobadoPor != null ? aprobadoPor.user_numIdent.ToString() : "",
                            NombreAprobado = aprobadoPor != null
                                ? aprobadoPor.user_nombre + " " + aprobadoPor.user_apellido
                                : "",
                            //Vendedor

                        };

                        ViewAsPdf something = new ViewAsPdf("crearPDFfacturacionrepuestos", modelo); //crearPDFfacturacionotros
                        return something;
                    }
                    catch (Exception es)
                    {
                        Exception mensaje = es.InnerException;
                        throw;
                    }


                }

                TempData["mensaje_error"] = "No se puede generar el pdf";
            }

            return View();

        }

        public ActionResult Create(int? id, int? menu,string cadena = "")
        {
            BuscarFavoritos(menu);
            BuscarParametros();
            listParametros();


            if (cadena != "")
            {
                ViewBag.cadena = cadena;
                var Subcadena = cadena.Split(',');
                int dataBogeda = Convert.ToInt32(Subcadena[0]);
                string idreferencia = Subcadena[1];

                ViewBag.Id_bodega_origen = dataBogeda;
                ViewBag.CodReferencia = idreferencia;

                return View();
            }
            if (id != null)
            {
                //busco el registro
                var regi = context.icb_referencia_mov.Where(d => d.refmov_id == id).FirstOrDefault();
                if (regi != null)
                {
                    ViewBag.numCotCreado = regi.refmov_numero;
                    ViewBag.encabezado = regi.refmov_id;
                }
            }
            else
            {
                ViewBag.encabezado = 0;
            }
            ViewBag.fec_creacion = DateTime.Now.ToString("yyyy/MM/dd", new CultureInfo("en-US"));
            //fabian
            ReferenciasModel referenciasmodel = new ReferenciasModel();
            referenciasmodel.fec_creacion = DateTime.Now;
            return View(referenciasmodel);

            //return View();
        }

        [HttpPost]
        public ActionResult Create(ReferenciasModel datos, int? menu)
        {
            if (ModelState.IsValid)
            {
                int tipoCargue = Convert.ToInt32(Request["tipoCargue"]);
                int bodega = Convert.ToInt32(Request["bodega"]);

                tp_doc_registros tipoDocumento = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == tipoCargue);
                int final = Convert.ToInt32(tipoDocumento.tipo);

                //Consecutivo
                grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == tipoCargue && x.bodega_id == bodega);
                if (grupo != null)
                {
                    DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                    long consecutivo = doc.BuscarConsecutivo(grupo.grupo);
                    if (consecutivo == -1)
                    {
                        TempData["mensaje_error"] =
                            "No existe un consecutivo parametrizado para la bodega seleccionada";
                    }
                    else
                    {

                        //ICB_REFERENCIAS_MOV
                        icb_referencia_mov irm = new icb_referencia_mov();
                        //if (final == 27)
                        if (final == PPedRep)
                        {
                            irm.idcotizacion = Request["cargarCotizacion"] != ""
                                ? Convert.ToInt32(Request["cargarCotizacion"])
                                : 0;
                        }


                        decimal TotalMargenUtilidad = 0;
                        decimal margen = 0;
                        //calculando el margen de utilidad 
                       
                        int Listareferencia = Convert.ToInt32(Request["lista_referencias"]);
                        for (int i = 0; i < Listareferencia; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(Request["referencia" + i]) &&
                                                               !string.IsNullOrWhiteSpace(Request["cantidadReferencia" + i]) &&
                                                               !string.IsNullOrWhiteSpace(Request["valorUnitarioReferencia" + i]))
                            {



                                decimal valoriva = Convert.ToDecimal(Request["ivaTotalReferencia" + i], miCultura);
                                decimal valorTotal = Convert.ToDecimal(Request["valorTotalReferencia"+i], miCultura);
                                string idreferencia = Request["referencia" + i];

                                int cantidad = Convert.ToInt32(Request["cantidadReferencia" + i]);
                                int existecosto= Convert.ToInt32(Request["stockReferencia" + i]);
                                //se crea clase para calcular el margen de utilidad
                                CsCalcularMargenUtilidad calcularmargen = new CsCalcularMargenUtilidad();
                                if (existecosto == 1)
                                {
                                    margen = calcularmargen.MargenUtilidad(bodega, idreferencia, cantidad, valorTotal, valoriva);
                                    TotalMargenUtilidad = TotalMargenUtilidad + margen;
                                }                            
                            }
                        
                        }




                        irm.tpdocid = tipoCargue;
                        irm.bodega_id = bodega;
                        irm.refmov_numero = consecutivo;
                        //tiket 4469 realizado por fabian vanegas 22/09/2020 a las 11:15
                        irm.refmov_fecela = DateTime.Now; //Convert.ToDateTime(datos.fec_creacion, usDtfi);
                        var diasvalidezguardar = datos.dias_validez != null ? Convert.ToDouble(datos.dias_validez.Value) : 0;
                        irm.fecha_vencimiento = irm.refmov_fecela.AddDays(diasvalidezguardar);
                        irm.cliente = Convert.ToInt32(Request["cliente_id"]);
                        irm.vendedor = Convert.ToInt32(Request["proveedor"]);
                        irm.condicion = datos.condicion;
                        irm.dias_validez = (short)datos.dias_validez;
                        irm.valor_total = Convert.ToDecimal(Request["valorFinal"], miCultura);
                        irm.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                        irm.descuento_pie = Request["numDescuentoPIE"] != null && Request["numDescuentoPIE"] != "" ? Convert.ToDecimal(Request["numDescuentoPIE"], miCultura) : 0;
                        irm.estado = true;
                        irm.valoriva = Convert.ToDecimal(Request["valorIVA"], miCultura);
                        irm.valordescuento = Convert.ToDecimal(Request["valorDes"], miCultura);
                        irm.margen_utilidad = TotalMargenUtilidad;
                        irm.habilitar = true;
                        context.icb_referencia_mov.Add(irm);
                        int result = context.SaveChanges();
                        int idref_mov = irm.refmov_id;
                       

                        //ICB_REFERENCIAS_MOVDETALLE
                        string referencias = Request["lista_referencias"];
                        if (!string.IsNullOrEmpty(referencias))
                        {
                            int listaReferencias = Convert.ToInt32(Request["lista_referencias"]);
                            for (int i = 0; i < listaReferencias; i++)
                            {
                                if (!string.IsNullOrWhiteSpace(Request["referencia" + i]) &&
                                    !string.IsNullOrWhiteSpace(Request["cantidadReferencia" + i]) &&
                                    !string.IsNullOrWhiteSpace(Request["valorUnitarioReferencia" + i]))
                                {
                                    var existestock = Convert.ToInt32(Request["stockReferencia" + i]);
                                    icb_referencia_movdetalle irmd = new icb_referencia_movdetalle
                                        {
                                        refmov_id = idref_mov,
                                        ref_codigo = Request["referencia" + i],
                                        refdet_cantidad = Convert.ToInt32(Request["cantidadReferencia" + i]),
                                        valor_unitario = Convert.ToDecimal(Request["valorUnitarioReferencia" + i], miCultura),
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                        fec_creacion = DateTime.Now,
                                        estado = true,
                                        poriva = Convert.ToInt32(Request["ivaReferencia" + i]),
                                        pordscto = Convert.ToInt32(Request["descuentoReferencia" + i]),
                                        valor_total = Convert.ToInt32(Request["valorTotalReferencia" + i]),
                                        tiene_stock = existestock == 1 ? true : false,
                                        pedido = existestock == 0 ? true : false,
                                        traslado = existestock == 2 ? true : false,
                                        desdepedido = 1,
                                    };

                                    context.icb_referencia_movdetalle.Add(irmd);
                                }
                            }
                        }

                        result = context.SaveChanges();
                        if (result > 0)
                        {
                            //if (final == 25)
                            if (final == PCotRep)
                            {
                                DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
                                doc.ActualizarConsecutivo(grupo.grupo, consecutivo);
                                TempData["mensajeCotizacion"] = "Cotizacion registrado correctamente";
                                return RedirectToAction("Create",new {id= idref_mov });
                            }
                            else
                            {
                                DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
                                doc.ActualizarConsecutivo(grupo.grupo, consecutivo);
                                TempData["mensajePedido"] = "Pedido registrado correctamente";
                                return RedirectToAction("Create", new { id = idref_mov });
                            }
                        }

                        TempData["mensaje_error"] = "Error al registrar la Cotizacion, por favor intente nuevamente";
                        List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                            .Where(y => y.Count > 0)
                            .ToList();
                    }
                }
                else
                {
                    TempData["mensaje_error"] = "Error, no hay numero consecutivo";
                }
            }
            else
            {
                TempData["mensaje_error"] = "No fue posible guardar el registro, por favor valide";
                List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();
            }

            listParametros();
            BuscarFavoritos(menu);
            return View(datos);
        }

        public JsonResult agregarReferenciaMov(string codigo, string precio, string descuento, string iva,
            string cantidad, string descuentoPie, int idref_mov)
        {
            if (!string.IsNullOrWhiteSpace(codigo) && !string.IsNullOrWhiteSpace(precio) &&
                !string.IsNullOrWhiteSpace(descuento)
                && !string.IsNullOrWhiteSpace(iva) && !string.IsNullOrWhiteSpace(cantidad) &&
                !string.IsNullOrWhiteSpace(descuentoPie))
            {
                decimal precioU = Convert.ToDecimal(precio, miCultura);
                decimal vrdescuento = Convert.ToInt32(descuento) * precioU / 100;
                decimal vrdescuentopie = Convert.ToInt32(descuentoPie) * precioU / 100;
                decimal vriva = (precioU - vrdescuento - vrdescuentopie) * Convert.ToInt32(iva) / 100;
                decimal valorTotal = precioU + vriva - vrdescuento - vrdescuentopie;

                icb_referencia_movdetalle irmd = new icb_referencia_movdetalle
                {
                    refmov_id = idref_mov,
                    ref_codigo = codigo,
                    refdet_cantidad = Convert.ToDecimal(cantidad, miCultura),
                    valor_unitario = precioU,
                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    fec_creacion = DateTime.Now,
                    estado = true,
                    poriva = Convert.ToInt32(iva),
                    pordscto = Convert.ToInt32(descuento),
                    valor_total = valorTotal
                };

                context.icb_referencia_movdetalle.Add(irmd);
                context.SaveChanges();
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }


        public JsonResult CalcularMargenU(int Bodega, string codigoref, int cantidad, decimal valorTotal, decimal valoriva) {

            decimal resultado = 0;
            CsCalcularMargenUtilidad calcularmargen = new CsCalcularMargenUtilidad();

            resultado = calcularmargen.MargenUtilidad(Bodega, codigoref, cantidad, valorTotal, valoriva);

            return Json(resultado, JsonRequestBehavior.AllowGet);
        }

        public JsonResult eliminarRefBD(int detalle)
        {
            icb_referencia_movdetalle movdetalle = context.icb_referencia_movdetalle.Where(x => x.refdet_id == detalle).FirstOrDefault();
            if (movdetalle != null)
            {
                context.Entry(movdetalle).State = EntityState.Deleted;
                context.SaveChanges();
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Ver(int id, int? menu)
        {
            ReferenciasModel model = new ReferenciasModel();
            var datos = (from rm in context.icb_referencia_mov
                         join tp in context.tp_doc_registros
                             on rm.tpdocid equals tp.tpdoc_id
                         join tpt in context.tp_doc_registros_tipo
                             on tp.tipo equals tpt.id
                         join t in context.icb_terceros
                             on rm.cliente equals t.tercero_id
                         join b in context.bodega_concesionario
                             on rm.bodega_id equals b.id
                         join u in context.users
                             on rm.userid_creacion equals u.user_id
                         join uv in context.users
                             on rm.vendedor equals uv.user_id
                         join fp in context.fpago_tercero
                             on rm.condicion equals fp.fpago_id
                         where rm.refmov_id == id
                         select new
                         {
                             usuario = u.user_nombre + " " + u.user_apellido,
                             fecha = rm.refmov_fecela,
                             pedido = rm.refmov_numero,
                             nombre = t.prinom_tercero,
                             apellido = t.apellido_tercero,
                             bodega_id = b.id,
                             estado = rm.estado ? "Activo" : "Anulado",
                             rm.tpdocid,
                             condicion = fp.fpago_nombre,
                             rm.descuento_pie,
                             rm.dias_validez,
                             fecela = rm.refmov_fecela,
                             rm.valor_total,
                             vendedor = uv.user_nombre + " " + uv.user_apellido,
                             nit = t.tercero_id,
                             cliente = "(" + t.doc_tercero + ") " + t.prinom_tercero + " " + t.apellido_tercero,
                             cotizacion = rm.idcotizacion,
                             id = rm.refmov_id,
                             tpdoc_id = tpt.id
                         }).ToList();

            var nit = datos.Select(d => d.nit).FirstOrDefault();
            foreach (var item in datos)
            {
                //ViewBag.movimiento = item.tpmov;
                ViewBag.tpdoc_id = item.tpdoc_id;
                ViewBag.usuario = item.usuario;
                ViewBag.proveedor = item.nombre + " " + item.apellido;
                ViewBag.estado = item.estado;
                ViewBag.tpdocid = item.tpdocid;
                ViewBag.id = item.id;
                ViewBag.condicion = item.condicion;
                ViewBag.descuento_pie = item.descuento_pie;
                ViewBag.fecela = item.fecela;
                ViewBag.pedido = item.pedido;
                ViewBag.valor_total = item.valor_total;
                ViewBag.vendedor = item.vendedor;
                ViewBag.nit = item.nit;
                //ViewBag.cliente = item.cliente;
                ViewBag.cotizacion = item.cotizacion;
                ViewBag.dias_validez = item.dias_validez;
                ViewBag.bodega_id = new SelectList(context.bodega_concesionario, "id", "bodccs_nombre", item.bodega_id);
            }

            var listM = (from m in context.icb_terceros
                         join rm in context.icb_referencia_mov
                         on m.tercero_id  equals rm.cliente

                         select new
                         {
                             id = m.tercero_id,
                             cliente = "(" + m.doc_tercero + ") " + m.prinom_tercero + " " + m.apellido_tercero
                         }).ToList();

            ViewBag.cliente = new SelectList(listM, "id","cliente", nit);

            icb_referencia_mov buscarAnuladas = context.icb_referencia_mov.Where(x => x.refmov_id == id).FirstOrDefault();
            if (buscarAnuladas.estado == false)
            {
                int tipoCotizacion =
                    Convert.ToInt32(context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P25").syspar_value);
                int tipoPedido = Convert.ToInt32(context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P26")
                    .syspar_value);
                if (buscarAnuladas.tpdocid == tipoCotizacion)
                {
                    TempData["mensaje_error"] = "Esta cotización ha sido anulada";
                }

                if (buscarAnuladas.tpdocid == tipoPedido)
                {
                    TempData["mensaje_error"] = "Este Pedido ha sido anulado";
                }

                ViewBag.anulado = 1;
                BuscarFavoritos(menu);
                return View(model);
            }

            model.refmov_id = id;
            model.bodega_id = datos.Select(x => x.bodega_id).FirstOrDefault();
            ViewBag.anulado = 0;

            BuscarFavoritos(menu);
            return View(model);
        }

        [HttpPost]
        public ActionResult Ver(ReferenciasModel datos, int? menu)
        {
            icb_referencia_mov refmov = context.icb_referencia_mov.FirstOrDefault(x => x.refmov_id == datos.refmov_id);

            //refmov.bodega_id = datos.bodega_id;
            //context.Entry(refmov).State = System.Data.Entity.EntityState.Modified;
            //var result = context.SaveChanges();

            //ICB_REFERENCIAS_MOVDETALLE
            if (!string.IsNullOrEmpty(Request["lista_referencias"]))
            {
                int lista_referencias = Convert.ToInt32(Request["lista_referencias"]);
                int valor_total = 0;

                for (int i = 0; i < lista_referencias; i++)
                {
                    string refe = Request["referencia" + i];
                    icb_referencia referencia = context.icb_referencia.FirstOrDefault(x => x.ref_codigo == refe);
                    int cantidad = Convert.ToInt32(Request["cantidadReferencia" + i]);
                    int iva = Convert.ToInt32(Request["ivaReferencia" + i]);
                    int descuento = Convert.ToInt32(Request["descuentoReferencia" + i]);
                    decimal valor = Convert.ToDecimal(Request["valorUnitarioReferencia" + i], miCultura);
                    decimal total = Convert.ToDecimal(Request["valorTotalReferencia" + i], miCultura);
                    if (refe != null)
                    {
                        refmov.valor_total = refmov.valor_total + total;

                        refmov.valoriva = refmov.valoriva +
                                          iva * (valor * cantidad - descuento * valor * cantidad / 100) / 100;
                        refmov.valordescuento = refmov.valordescuento + descuento * valor * cantidad / 100;


                        context.Entry(refmov).State = EntityState.Modified;

                        context.icb_referencia_movdetalle.Add(new icb_referencia_movdetalle
                        {
                            refmov_id = refmov.refmov_id,
                            ref_codigo = referencia.ref_codigo,
                            refdet_cantidad = cantidad,
                            poriva = iva,
                            pordscto = descuento,
                            valor_unitario = valor,
                            valor_total = total,

                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            fec_creacion = DateTime.Now,
                            estado = true
                        });
                    }


                    int result = context.SaveChanges();
                    if (result > 0)
                    {
                        refmov.valor_total = refmov.valor_total + valor_total;
                        context.Entry(refmov).State = EntityState.Modified;
                        result = context.SaveChanges();
                        TempData["mensaje"] = "Registro actualizado correctamente";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error al actualizar el registro";
                    }
                }
            }

            ViewBag.bodega_id = new SelectList(context.bodega_concesionario, "id", "bodccs_nombre", datos.bodega_id);
            Ver(refmov.refmov_id, menu);
            return View(datos);
        }

        public JsonResult BuscarHistorico(int? id)
        {
            var data = from d in context.icb_referencia_movdetalle
                       join m in context.icb_referencia_mov
                           on d.refmov_id equals m.refmov_id
                       join r in context.icb_referencia
                           on d.ref_codigo equals r.ref_codigo
                       join g in context.grupo_repuesto
                           on r.tipo_id equals g.grupo_id
                       where d.refmov_id == id
                       select new
                       {
                           id = d.refdet_id,
                           codigo = d.ref_codigo,
                           descripcion = r.ref_descripcion,
                           cantidad = d.refdet_cantidad,
                           stock = r.ref_stock,
                           valor = d.valor_unitario,
                           grupo = g.grupo_nombre,
                           d.valor_total,
                           r.por_iva
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarReferenciasHistorico(int? id)
        {
            var buscar = (from d in context.icb_referencia_movdetalle
                          join m in context.icb_referencia_mov
                              on d.refmov_id equals m.refmov_id
                          join r in context.icb_referencia
                              on d.ref_codigo equals r.ref_codigo
                          join g in context.grupo_repuesto
                              on r.tipo_id equals g.grupo_id
                          where d.refmov_id == id
                          select new
                          {
                              d.refdet_id,
                              d.ref_codigo,
                              r.ref_descripcion,
                              d.refdet_cantidad,
                              d.valor_unitario,
                              d.pordscto,
                              r.por_iva,
                              m.descuento_pie
                          }).ToList();
            var data = buscar.Select(x => new
            {
                id = x.refdet_id,
                codigo = x.ref_codigo,
                descripcion = x.ref_descripcion,
                cantidad = x.refdet_cantidad,
                valorUnitario = Math.Round(x.valor_unitario, 0),
                valorAntesIva = Math.Round(x.refdet_cantidad * x.valor_unitario, 0),
                porDescuento = x.pordscto,
                totalDescuento = Math.Round(x.refdet_cantidad * x.valor_unitario * Convert.ToDecimal(x.pordscto, miCultura) / 100,
                    0),
                porDescuentoPie = x.descuento_pie,
                totalDescuentoPie = Math.Round(x.refdet_cantidad * x.valor_unitario * x.descuento_pie) / 100,
                porIva = x.por_iva,
                totalIva = Math.Round(
                    (x.refdet_cantidad * x.valor_unitario -
                     x.refdet_cantidad * x.valor_unitario * Convert.ToDecimal(x.pordscto, miCultura) / 100) *
                    Convert.ToDecimal(x.por_iva, miCultura) / 100, 0)
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDocumentos(int? id)
            {

            List<ListaDocumentos> listadoc = new  List<ListaDocumentos>();
            var data = (from a in context.Solicitud_traslado 
                       join d in context.Estado
                       on a.Estado_atendido equals  d.id
                       join b in context.Detalle_Solicitud_Traslado
                       on a.Id equals b.Id_Solicitud_Traslado
                       join c in context.rseparacionmercancia
                       on b.idseparacion equals c.id into cseparacion
                       from c in cseparacion.DefaultIfEmpty()
                       where c.idpedido == id
                       select new
                           {
                           tipodoc = "Solicitud de Translado",
                           a.Id,
                           fecha = a.Fecha_creacion,
                           d.Tipo

                           }).ToList();

            var soltrans =  data.GroupBy(x => new { x.Id, x.tipodoc, x.fecha, x.Tipo});

            foreach (var item in soltrans)
                {
                ListaDocumentos lista = new ListaDocumentos();
                lista.id = item.Key.Id;
                lista.tipodoc = item.Key.tipodoc;
                lista.fecha = item.Key.fecha.ToString("yyyy-MM-dd");
                lista.estado = item.Key.Tipo;
                listadoc.Add(lista);
                }

            var datoscom = (from a in context.rsolicitudesrepuestos
                            join b in context.rseparacionmercancia
                            on a.separacion_consecutivo equals b.separacion
                            join c in context.restado_solicitud_Repuestos
                            on a.estado_solicitud equals c.id_estado_solicitud
                            where b.idpedido == id 
                            select new
                                {
                                tipodoc = "Solicitud de Compra",
                                a.id,
                                fecha = a.fecha,
                                c.Descripcion
                                }).ToList();

            var datoscomlis = datoscom.GroupBy(x => new { x.id, x.tipodoc, x.fecha, x.Descripcion });


            foreach (var item in datoscomlis)
                {
                ListaDocumentos lista = new ListaDocumentos();
                lista.id = item.Key.id;
                lista.tipodoc = item.Key.tipodoc;
                lista.fecha = item.Key.fecha.ToString("yyyy-MM-dd");
                lista.estado = item.Key.Descripcion;
                listadoc.Add(lista);
                }

            int swfactura = Convert.ToInt32(context.icb_sysparameter.Where(z => z.syspar_cod == "P129").Select(x => x.syspar_value).FirstOrDefault());
            var facturas = (from a in context.encab_documento
                            join b in context.tp_doc_registros
                            on a.tipo equals b.tpdoc_id
                            join c in context.tp_doc_sw
                            on b.sw equals c.tpdoc_id
                            where a.pedido == id && c.sw == swfactura
                            select new
                                {
                                id = a.numero,
                                documento = b.tpdoc_nombre,
                                fecha = a.fec_creacion,
                                estado = ""

                                }).ToList();

            foreach (var item in facturas)
                {
                ListaDocumentos lista = new ListaDocumentos();
                lista.id = Convert.ToInt32(item.id);
                lista.tipodoc = item.documento;
                lista.fecha = item.fecha.ToString("yyyy-MM-dd");
                lista.estado = item.estado;
                listadoc.Add(lista);
                }


            return Json(listadoc, JsonRequestBehavior.AllowGet);
            }

        public JsonResult BuscarRepuestos()
        {
            var data = (from rm in context.icb_referencia_mov
                        join b in context.bodega_concesionario
                            on rm.bodega_id equals b.id
                        join tp in context.tp_doc_registros
                            on rm.tpdocid equals tp.tpdoc_id
                        join t in context.icb_terceros
                            on rm.cliente equals t.tercero_id into xx
                        from t in xx.DefaultIfEmpty()
                        join u in context.users
                            on rm.vendedor equals u.user_id into zz
                        from u in zz.DefaultIfEmpty()
                        select new
                        {
                            pedido = rm.refmov_numero,
                            bodega = b.bodccs_nombre,
                            id = rm.refmov_id,
                            //tipocargue = tp.tipo == 25 ? "Cotización" : "Pedido",
                            tipocargue = tp.tipo == PCotRep ? "Cotización" : "Pedido",
                            fechaRegistro = rm.refmov_fecela,
                            priNom = t.prinom_tercero,
                            segNom = t.segnom_tercero,
                            apellido = t.apellido_tercero,
                            segApe = t.segapellido_tercero,
                            t.doc_tercero,
                            u.user_nombre,
                            u.user_apellido,
                            rm.estado
                        }).ToList();

            var data2 = data.Select(x => new
            {
                x.pedido,
                x.bodega,
                x.id,
                x.tipocargue,
                fechaRegistro = x.fechaRegistro.ToString("yyyy/MM/dd hh:mm:ss", new CultureInfo("en-US")),
                documento = x.doc_tercero,
                nombreCliente = x.priNom + " " + x.segNom + " " + x.apellido + " " + x.segApe,
                nombreAsesor = x.user_nombre + " " + x.user_apellido,
                estado = x.estado ? "Activo" : "Anulado"
            });
            return Json(data2, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarRepuestosAnulados()
        {
            var data = (from rm in context.icb_referencia_mov
                        join b in context.bodega_concesionario
                            on rm.bodega_id equals b.id
                        join d in context.icb_referencia_movdetalle
                            on rm.refmov_id equals d.refmov_id
                        join r in context.icb_referencia
                            on d.ref_codigo equals r.ref_codigo
                        join tp in context.tp_doc_registros
                            on rm.tpdocid equals tp.tpdoc_id
                        join t in context.icb_terceros
                            on rm.cliente equals t.tercero_id into xx
                        from t in xx.DefaultIfEmpty()
                        join u in context.users
                            on rm.vendedor equals u.user_id into zz
                        from u in zz.DefaultIfEmpty()
                        where rm.estado==false
                        select new
                        {
                            pedido = rm.refmov_numero,
                            bodega = b.bodccs_nombre,
                            id = rm.refmov_id,
                            //tipocargue = tp.tipo == 25 ? "Cotización" : "Pedido",
                            tipocargue = tp.tipo == PCotRep ? "Cotización" : "Pedido",
                            fechaRegistro = rm.refmov_fecela,
                            priNom = t.prinom_tercero,
                            segNom = t.segnom_tercero,
                            apellido = t.apellido_tercero,
                            segApe = t.segapellido_tercero,
                            t.doc_tercero,
                            u.user_nombre,
                            u.user_apellido,
                            rm.estado,
                            motivo= rm.idanulacion != null ? rm.motivo_anulacion.motivo : "",
                            d.refdet_id,
                            d.ref_codigo,
                            d.refdet_cantidad,
                            d.valor_unitario,
                            d.pordscto,
                            r.por_iva,
                        }).ToList();

            var data2 = data.Select(x => new
            {
                x.pedido,
                x.bodega,
                x.id,
                x.tipocargue,
                fechaRegistro = x.fechaRegistro.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                documento = x.doc_tercero,
                nombreCliente = x.priNom + " " + x.segNom + " " + x.apellido + " " + x.segApe,
                nombreAsesor = x.user_nombre + " " + x.user_apellido,
                estado = x.estado ? "Activo" : "Anulado",
                x.motivo,
                valorAntesIva = Math.Round(x.refdet_cantidad * x.valor_unitario, 0),
                totalDescuento = Math.Round(x.refdet_cantidad * x.valor_unitario * Convert.ToDecimal(x.pordscto, miCultura) / 100,
                    0),
                totalIva = Math.Round(
                    (x.refdet_cantidad * x.valor_unitario -
                     x.refdet_cantidad * x.valor_unitario * Convert.ToDecimal(x.pordscto, miCultura) / 100) *
                    Convert.ToDecimal(x.por_iva, miCultura) / 100, 0)
            });
            return Json(data2, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarRepuestosXFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            var data = (from rm in context.icb_referencia_mov
                        join b in context.bodega_concesionario
                            on rm.bodega_id equals b.id
                        join tp in context.tp_doc_registros
                            on rm.tpdocid equals tp.tpdoc_id
                        join t in context.icb_terceros
                            on rm.cliente equals t.tercero_id into xx
                        from t in xx.DefaultIfEmpty()
                        join u in context.users
                            on rm.vendedor equals u.user_id into zz
                        from u in zz.DefaultIfEmpty()
                        where (rm.refmov_fecela >= fechaInicio && rm.refmov_fecela <= fechaFin)
                        select new
                        {
                            pedido = rm.refmov_numero,
                            bodega = b.bodccs_nombre,
                            id = rm.refmov_id,
                            //tipocargue = tp.tipo == 25 ? "Cotización" : "Pedido",
                            tipocargue = tp.tipo == PCotRep ? "Cotización" : "Pedido",
                            fechaRegistro = rm.refmov_fecela,
                            priNom = t.prinom_tercero,
                            segNom = t.segnom_tercero,
                            apellido = t.apellido_tercero,
                            segApe = t.segapellido_tercero,
                            t.doc_tercero,
                            u.user_nombre,
                            u.user_apellido,
                            rm.estado
                        }).ToList().OrderBy(s => s.fechaRegistro); 

            var data2 = data.Select(x => new
            {
                x.pedido,
                x.bodega,
                x.id,
                x.tipocargue,
                fechaRegistro = x.fechaRegistro.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                documento = x.doc_tercero,
                nombreCliente = x.priNom + " " + x.segNom + " " + x.apellido + " " + x.segApe,
                nombreAsesor = x.user_nombre + " " + x.user_apellido,
                estado = x.estado ? "Activo" : "Anulado"
            });
            return Json(data2, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarRepuestosXFechaAnulados(DateTime fechaInicio, DateTime fechaFin)
        {
            var data = (from rm in context.icb_referencia_mov
                        join b in context.bodega_concesionario
                            on rm.bodega_id equals b.id
                        join tp in context.tp_doc_registros
                            on rm.tpdocid equals tp.tpdoc_id
                        join t in context.icb_terceros
                            on rm.cliente equals t.tercero_id into xx
                        from t in xx.DefaultIfEmpty()
                        join u in context.users
                            on rm.vendedor equals u.user_id into zz
                        from u in zz.DefaultIfEmpty()
                        where (rm.refmov_fecela >= fechaInicio && rm.refmov_fecela <= fechaFin) && rm.estado == false
                        select new
                        {
                            pedido = rm.refmov_numero,
                            bodega = b.bodccs_nombre,
                            id = rm.refmov_id,
                            //tipocargue = tp.tipo == 25 ? "Cotización" : "Pedido",
                            tipocargue = tp.tipo == PCotRep ? "Cotización" : "Pedido",
                            fechaRegistro = rm.refmov_fecela,
                            priNom = t.prinom_tercero,
                            segNom = t.segnom_tercero,
                            apellido = t.apellido_tercero,
                            segApe = t.segapellido_tercero,
                            t.doc_tercero,
                            u.user_nombre,
                            u.user_apellido,
                            rm.estado,
                            motivo = rm.idanulacion != null ? rm.motivo_anulacion.motivo : "",

                        }).ToList().OrderBy(s => s.fechaRegistro);

            var data2 = data.Select(x => new
            {
                x.pedido,
                x.bodega,
                x.id,
                x.tipocargue,
                fechaRegistro = x.fechaRegistro.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                documento = x.doc_tercero,
                nombreCliente = x.priNom + " " + x.segNom + " " + x.apellido + " " + x.segApe,
                nombreAsesor = x.user_nombre + " " + x.user_apellido,
                estado = x.estado ? "Activo" : "Anulado",
                x.motivo,
            });
            return Json(data2, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarReferencias()
        {
            var data = from r in context.icb_referencia
                       where r.modulo == "R"
                       select new
                       {
                           codigo = r.ref_codigo,
                           nombre = "(" + r.ref_codigo + ") " + r.ref_descripcion
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarDescuento(string codigo, int cliente)
        {
            float data = (from r in context.icb_referencia
                          where r.ref_codigo == codigo
                          select
                              r.por_dscto_max
                ).FirstOrDefault();


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult validezPedido(string fecha, int dias)
        {
            DateTime fechaCreacion = Convert.ToDateTime(fecha, usDtfi);
            DateTime fechaActual = DateTime.Now;
            int data = 0;
            DateTime fechaFin = fechaCreacion.AddDays(dias);

            if (fechaActual > fechaFin)
            {
                data = 1;
            }
            else
            {
                data = 2;
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarCostoReferenciaCompras(string codigo, int id_cliente,int? bodega)
        {
            var bodegabuscar = 0;
             var convertir= Int32.TryParse(Session["user_bodega"].ToString(),out bodegabuscar);
            if (bodega != null)
            {
                bodegabuscar = bodega.Value;
            }
            int idRegSimp = 0;
            //  decimal iva1= 0;

            var buscarSimplificado = (from des in context.icb_sysparameter
                                      where des.syspar_cod == "P130" //id regimen simplificado
                                      select new
                                      {
                                          regimenSimplificado = des.syspar_value
                                      }).FirstOrDefault();
            if (buscarSimplificado != null)
            {
                idRegSimp = Convert.ToInt32(buscarSimplificado.regimenSimplificado);
            }

            var buscarPerfilUsuario = (from des in context.icb_terceros
                                       where des.tercero_id == id_cliente //perfil tributario usuairo
                                       select new
                                       {
                                           des.tpregimen_id
                                       }).FirstOrDefault();

            icb_referencia buscarReferencia = context.icb_referencia.Where(x => x.ref_codigo == codigo).FirstOrDefault();

            decimal iva1 = (decimal)buscarReferencia.por_iva;
            if (buscarPerfilUsuario.tpregimen_id == idRegSimp)
            {
                iva1 = 0;
            }
            else
            {
                iva1 = (decimal)buscarReferencia.por_iva;
            }

            decimal precio = 0;
            decimal iva = 0;
            decimal descuento = 0;
            decimal costo = 0;
            decimal descuento_maximo = 0;


            if (buscarReferencia != null)
            {
                descuento = (decimal)buscarReferencia.por_dscto;
                descuento_maximo = (decimal)buscarReferencia.por_dscto_max;
                if (descuento_maximo < descuento)
                {
                    descuento = descuento_maximo;
                }

                precio = buscarReferencia.precio_venta;
                iva = iva1;
                //busco el costo promedio de la bodega en curso
                var promediobodega = context.vw_inventario_hoy.Where(d => d.ref_codigo == buscarReferencia.ref_codigo && d.bodega == bodegabuscar).FirstOrDefault();
                if (promediobodega != null)
                {


                }
                costo = buscarReferencia.costo_promedio != null ? buscarReferencia.costo_promedio.Value : 0;
            }

            return Json(new { empleado = false, precio, iva, descuento, costo, descuento_maximo },
                JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarCostoReferencia(string codigo, int? id_cliente)
        {
            //
            int idRegSimp = 0;
            //  decimal iva1= 0;
            var buscarSimplificado = (from des in context.icb_sysparameter
                                      where des.syspar_cod == "P130" //id regimen simplificado
                                      select new
                                      {
                                          regimenSimplificado = des.syspar_value
                                      }).FirstOrDefault();
            if (buscarSimplificado != null)
            {
                idRegSimp = Convert.ToInt32(buscarSimplificado.regimenSimplificado);
            }

            var buscarPerfilUsuario = (from des in context.icb_terceros
                                       where des.tercero_id == id_cliente //perfil tributario usuairo
                                       select new
                                       {
                                           des.tpregimen_id
                                       }).FirstOrDefault();

            var buscarReferencia = (from referencia in context.icb_referencia
                                    join precios in context.rprecios
                                        on referencia.ref_codigo equals precios.codigo into pre
                                    from precios in pre.DefaultIfEmpty()
                                    where referencia.ref_codigo == codigo
                                    select new
                                    {
                                        referencia.ref_codigo,
                                        referencia.ref_descripcion,
                                        referencia.por_iva,
                                        referencia.por_dscto,
                                        referencia.por_dscto_max,
                                        referencia.costo_unitario,
                                        precio1 = precios != null ? precios.precio1 : 0,
                                        precio2 = precios != null ? precios.precio2 : 0,
                                        precio3 = precios != null ? precios.precio3 : 0,
                                        precio4 = precios != null ? precios.precio4 : 0,
                                        precio5 = precios != null ? precios.precio5 : 0,
                                        precio6 = precios != null ? precios.precio6 : 0,
                                        precio7 = precios != null ? precios.precio7 : 0,
                                        precio8 = precios != null ? precios.precio8 : 0,
                                        precio9 = precios != null ? precios.precio9 : 0
                                    }).FirstOrDefault();
            decimal iva1 = (decimal)buscarReferencia.por_iva;
            if (buscarPerfilUsuario?.tpregimen_id == idRegSimp)
            {
                iva1 = 0;
            }
            else
            {
                iva1 = (decimal)buscarReferencia.por_iva;
            }

            var buscarPrecioEmpleado = (from empleado in context.tercero_empleado
                                        where empleado.tercero_id == id_cliente
                                        select new
                                        {
                                            empleado.tercero_id
                                        }).FirstOrDefault();

            var buscarPrecioCliente = (from cliente in context.tercero_cliente
                                       where cliente.tercero_id == id_cliente
                                       select new
                                       {
                                           cliente.lprecios_repuestos,
                                           cliente.dscto_rep
                                       }).FirstOrDefault();

            decimal precio = 0;
            decimal iva = 0;
            decimal descuento = 0;
            decimal costo = 0;
            if (buscarReferencia != null)
            {
                costo = buscarReferencia.costo_unitario;
            }

            if (buscarPrecioEmpleado != null)
            {
                var descuentoEmpleado = (from des in context.icb_sysparameter
                                         where des.syspar_cod == "P110" //Codigo de descuento de empleado
                                         select new
                                         {
                                             descuento = des.syspar_value
                                         }).FirstOrDefault();
                iva = iva1;
                int descuento_maximo = descuentoEmpleado.descuento != null ? int.Parse(descuentoEmpleado.descuento) : 0;
                descuento = descuento_maximo;
                precio = costo; // asi debera quedar
                precio = buscarReferencia
                    .precio1; // linea temporal mientras se cargar un valor valido para el costo de las referencias ya que esta en 0
                return Json(new { empleado = true, precio, iva, descuento, costo }, JsonRequestBehavior.AllowGet);
            }

            if (buscarPrecioCliente != null && buscarReferencia != null)
            {
                iva = iva1;
                decimal descuento_maximo = (decimal)buscarReferencia.por_dscto_max;
                descuento = (decimal)buscarReferencia.por_dscto;
                decimal descuentoCliente = (decimal)buscarPrecioCliente.dscto_rep;
                descuento = descuento < descuentoCliente ? descuentoCliente : descuento;
                if (descuento_maximo < descuento)
                {
                    descuento = descuento_maximo;
                }

                #region Selecciona el precio de acuerdo a la lista del cliente

                if (buscarPrecioCliente.lprecios_repuestos == "precio1")
                {
                    precio = buscarReferencia.precio1;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio2")
                {
                    precio = buscarReferencia.precio2;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio3")
                {
                    precio = buscarReferencia.precio3;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio4")
                {
                    precio = buscarReferencia.precio4;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio5")
                {
                    precio = buscarReferencia.precio5;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio6")
                {
                    precio = buscarReferencia.precio6;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio7")
                {
                    precio = buscarReferencia.precio7;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio8")
                {
                    precio = buscarReferencia.precio8;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio9")
                {
                    precio = buscarReferencia.precio9;
                }

                if (buscarPrecioCliente.lprecios_repuestos == null)
                {
                    precio = buscarReferencia.precio1;
                }

                #endregion

                return Json(new { empleado = false, precio, iva, descuento, costo }, JsonRequestBehavior.AllowGet);
            }

            if (buscarReferencia != null)
            {
                descuento = (decimal)buscarReferencia.por_dscto;
                decimal descuento_maximoAux = (decimal)buscarReferencia.por_dscto_max;
                if (descuento_maximoAux < descuento)
                {
                    descuento = descuento_maximoAux;
                }
            }

            return Json(new { empleado = false, precio, iva, descuento, costo }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult permisoDescuento()
        {
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);

            int permiso = (from u in context.users
                           join r in context.rols
                               on u.rol_id equals r.rol_id
                           join ra in context.rolacceso
                               on r.rol_id equals ra.idrol
                           where u.user_id == usuario && ra.idpermiso == 10
                           select new
                           {
                               u.user_id,
                               u.rol_id,
                               r.rol_nombre,
                               ra.idpermiso
                           }).Count();
            return Json(permiso, JsonRequestBehavior.AllowGet);
        }

        public JsonResult permisoPrecio()
        {
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);

            int permiso = (from u in context.users
                           join r in context.rols
                               on u.rol_id equals r.rol_id
                           join ra in context.rolacceso
                               on r.rol_id equals ra.idrol
                           where u.user_id == usuario && ra.idpermiso == 11
                           select new
                           {
                               u.user_id,
                               u.rol_id,
                               r.rol_nombre,
                               ra.idpermiso
                           }).Count();
            return Json(permiso, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Anular(int id, int? menu, int? motivo)
        {
            icb_referencia_mov refmov = context.icb_referencia_mov.FirstOrDefault(x => x.refmov_id == id);
            refmov.estado = false;
            refmov.idanulacion = motivo;
            context.Entry(refmov).State = EntityState.Modified;

            int result = context.SaveChanges();

            if (result > 0)
            {
                TempData["mensaje"] = "Anulado correctamente";
            }
            else
            {
                TempData["mensaje_error"] = "Error al anular";
            }

            Ver(id, menu);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult cargarCotizacion(int id)
        {
            var datos = (from rm in context.icb_referencia_mov
                         join rd in context.icb_referencia_movdetalle
                             on rm.refmov_id equals rd.refmov_id
                         join c in context.tercero_cliente
                             on rm.cliente equals c.tercero_id
                         join f in context.fpago_tercero
                             on c.cod_pago_id equals f.fpago_id
                         join rf in context.icb_referencia
                         on rd.ref_codigo equals rf.ref_codigo
                         where rm.refmov_id == id && rm.estado
                         select new
                         {
                             nit = rm.cliente,
                             bodega = rm.bodega_id,
                             rm.vendedor,
                             fecela = rm.refmov_fecela,
                             id = f.fpago_id,
                             condicion = f.fpago_nombre,
                             dias = rm.dias_validez,
                             pie = rm.descuento_pie,
                             codigo = rd.ref_codigo,
                             valor = rd.valor_unitario,
                             descuento = rd.pordscto,
                             iva = rd.poriva,
                             cantidad = rd.refdet_cantidad,
                             movdetalle = rd.refdet_id,
                             costo = rf.costo_unitario,
                         }).ToList();

            AlmacenController metodosstock = new AlmacenController();

            var consulta = datos.Select(x => new
            {
                x.nit,
                x.bodega,
                x.vendedor,
                fecela = x.fecela != null ? x.fecela.ToString("yyyy/MM/dd",new CultureInfo("es-US")) : "",
                x.id,
                x.condicion,
                x.dias,
                x.pie,
                x.codigo,
                x.valor,
                x.descuento,
                x.iva,
                x.cantidad,
                x.movdetalle,
                x.costo,
                stock_bodega = metodosstock.buscarStockBodega(x.codigo, x.bodega)>=x.cantidad?1:0,
                stock_otrasbodegas = metodosstock.buscarStockOtras(x.codigo, x.bodega) >= x.cantidad ? 1 : 0,
            });

            return Json(consulta, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarStockRef(string codigo, int bodega, int? cantidad)
        {
            vw_inventario_hoy referencia = context.vw_inventario_hoy.Where(x => x.ref_codigo == codigo && x.bodega == bodega)
                .FirstOrDefault();

            if (referencia != null)
            {
                decimal stock = referencia.stock;
                if (cantidad != null)
                {
                    if (cantidad <= stock)
                    {
                        return Json(new { cantidad = true }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { cantidad = false, stock }, JsonRequestBehavior.AllowGet);
                }

                if (stock > 0)
                {
                    return Json(new { stock = true }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                List<vw_inventario_hoy> stockGeneral = context.vw_inventario_hoy.Where(x => x.ref_codigo == codigo && x.stock > 0).ToList();
                if (stockGeneral != null)
                {
                    var info = stockGeneral.Select(x => new
                    {
                        x.ref_codigo,
                        referencia = x.ref_codigo + " - " + x.ref_descripcion,
                        x.nombreBodega,
                        x.stock,
                        x.bodega
                    });
                    return Json(new { stock = false, inventario = true, info }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { stock = false, inventario = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { stock = false, inventario = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarFormaPago(int id)
        {
            //var data = (from c in context.tercero_cliente
            //            join f in context.fpago_tercero
            //                on c.cod_pago_id equals f.fpago_id
            //            where c.tercero_id == id && c.fec_cupo_limite >= DateTime.Now
            //            select new
            //            {
            //                id = f.fpago_id,
            //                fpago = f.fpago_nombre
            //            }).FirstOrDefault();

            var data = context.tercero_cliente.Where(x => x.tercero_id == id).Select(x =>
                    new { id = x.cod_pago_id, fpago = x.cod_pago_id != null ? x.fpago_tercero.fpago_nombre : "" })
                .FirstOrDefault();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarFormasPagos()
        {
            var data = (from f in context.fpago_tercero
                        select new
                        {
                            id = f.fpago_id,
                            fpago = f.fpago_nombre
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult formaPagoContado()
        {
            var data = (from f in context.icb_sysparameter
                        where f.syspar_cod == "P124"
                        select new
                        {
                            f.syspar_value
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult tipoCargue(int? id)
        {
            var data2 = 0;
            if (id != null)
            {
                //examino parametros de sistema a ver si es cotizacion o pedido
                var param1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P152").FirstOrDefault();
                var coti = param1 != null ? Convert.ToInt32(param1.syspar_value) : 5;

                var param2 = context.icb_sysparameter.Where(d => d.syspar_cod == "P153").FirstOrDefault();
                var pedi = param2 != null ? Convert.ToInt32(param2.syspar_value) : 5;
                var data = (from tp in context.tp_doc_registros
                            where tp.tpdoc_id == id
                            select new
                            {
                                tpCargue = tp.tipo,
                                sw = tp.tp_doc_sw.sw,
                            }).FirstOrDefault();
                if (data != null)
                {
                    data2=data.sw==coti?1:(data.sw==pedi?2:0);
                }
            }
            
            return Json(data2, JsonRequestBehavior.AllowGet);
        }

        public void EliminarPedido(int id, int mov)
        {
            icb_referencia_movdetalle dato = context.icb_referencia_movdetalle.Find(id);

            icb_referencia_mov movi = context.icb_referencia_mov.Find(mov);
            movi.valor_total = movi.valor_total - Convert.ToDecimal(dato.valor_total, miCultura);

            decimal iva = Convert.ToDecimal(dato.poriva, miCultura) *
                      (dato.valor_unitario * dato.refdet_cantidad - Convert.ToDecimal(dato.pordscto, miCultura) *
                       dato.valor_unitario * dato.refdet_cantidad / 100) / 100;
            decimal descuento = Convert.ToDecimal(dato.pordscto, miCultura) * dato.valor_unitario * dato.refdet_cantidad / 100;

            movi.valoriva = movi.valoriva - iva;
            movi.valordescuento = movi.valordescuento - descuento;

            context.Entry(movi).State = EntityState.Modified;
            context.Entry(dato).State = EntityState.Deleted;
            context.SaveChanges();
        }

        public ActionResult crearPDFcotizacion(int? id)
        {
            var direcciontercero = (from rm in context.icb_referencia_mov
                                    join ter in context.terceros_direcciones
                                        on rm.cliente equals ter.idtercero
                                    where rm.refmov_id == id
                                    select new
                                    {
                                        direccion_cliente = ter.direccion
                                    }).ToList();
            string direc = direcciontercero.Max(c => c.direccion_cliente);


            var data2 = (from rm in context.icb_referencia_mov
                         join rdm in context.icb_referencia_movdetalle
                             on rm.refmov_id equals rdm.refmov_id
                         join re in context.icb_referencia
                             on rdm.ref_codigo equals re.ref_codigo
                         where rm.refmov_id == id
                         select new
                         {
                             ref_mov = rdm.refmov_id,
                             id_Item = rdm.refdet_id,
                             codItem = rdm.ref_codigo,
                             numitem = rdm.refdet_id,
                             descripcionItem = re.ref_descripcion,
                             canItem = rdm.refdet_cantidad,
                             dctItem = rdm.pordscto ?? 0,

                             ivaItem = rdm.poriva ?? 0,
                             pre_unit_Item = rdm.valor_unitario,
                             valorItem = rdm.valor_total
                         }).ToList();

            List<detalleCotizacion> detalleCotizacion001 = data2.Select(c => new detalleCotizacion
            {
                ref_mov = c.ref_mov,
                id_Item = c.id_Item,
                codItem = c.codItem,
                numitem = c.numitem,
                descripcionItem = c.descripcionItem,
                canItem = c.canItem.ToString("N0"),
                dctItem = Convert.ToString(c.dctItem),
                dctval = (c.canItem * c.pre_unit_Item * (Convert.ToDecimal(c.dctItem, miCultura) / 100)).ToString("N0"),
                valiva = (c.canItem * c.pre_unit_Item * (Convert.ToDecimal(c.ivaItem, miCultura) / 100)).ToString("N0"),
                ivaItem = Convert.ToString(c.ivaItem),
                pre_unit_Item = c.pre_unit_Item.ToString("N0"),
                valorItem = (c.canItem * c.pre_unit_Item).ToString("N0"), //c.valorItem.Value.ToString("N2"),
                valorItem2 = (c.canItem * c.pre_unit_Item), //c.valorItem.Value.ToString("N2"),
                pre_unit_dcto_Item =
                    (c.canItem * (c.pre_unit_Item - c.pre_unit_Item * (Convert.ToDecimal(c.dctItem, miCultura) / 100)))
                    .ToString("N0"),
                valor_dcto_Item =
                    (c.valorItem ?? 0 - (c.valorItem ?? 0 * (Convert.ToDecimal(c.dctItem, miCultura) / 100))).ToString("N0")
            }).ToList();


            var buscar_Cotizacion = (from rm in context.icb_referencia_mov
                                         //join rd in context.icb_referencia_movdetalle
                                         //on rm.refmov_id equals rd.refmov_id
                                     join c in context.tercero_cliente
                                         on rm.cliente equals c.tercero_id
                                     join f in context.fpago_tercero
                                         on c.cod_pago_id equals f.fpago_id
                                     join ter in context.icb_terceros
                                         on rm.cliente equals ter.tercero_id
                                     join bc in context.bodega_concesionario
                                         on rm.bodega_id equals bc.id
                                     join empr in context.tablaempresa
                                         on bc.concesionarioid equals empr.id

                                     //join dire in context.terceros_direcciones
                                     //on ter.tercero_id equals dire.idtercero
                                     where rm.refmov_id == id
                                     select new
                                     {
                                         id_empre = empr.id,
                                         desempre = empr.nombre_empresa,
                                         dirempre = empr.direccion,
                                         rifempre = empr.nit,
                                         id_usuario=rm.vendedor,
                                         ref_mov = rm.refmov_id,
                                         id_coti = rm.idcotizacion,
                                         numcoti = rm.refmov_numero,
                                         feccoti = rm.refmov_fecela,
                                         fvencoti = rm.fecha_vencimiento,


                                         nota = "",

                                         #region valores de montos totales

                                         //valorbruto =   
                                         //valordescuento = 
                                         //valorfletes =
                                         //baseiva =
                                         //valoriva =
                                         //valorneto =

                                         #endregion

                                         id_clie = c.tercero_id,
                                         nitclie = ter.doc_tercero,
                                         docclie = ter.doc_tercero,
                                         nomclie = ter.tpdoc_id != 1
                                             ? ter.prinom_tercero + " " + ter.segnom_tercero + " " + ter.apellido_tercero + " " +
                                               ter.segapellido_tercero
                                             : ter.razon_social,
                                         // dirclie = "-", //dire.direccion,
                                         telclie = ter.telf_tercero + " " + ter.celular_tercero,
                                         nit = rm.cliente,

                                         valorflete = rm.fletes,
                                         valorneto = rm.valor_total,
                                         rm.valoriva,
                                         valordcto = rm.valordescuento,
                                         baseiva = rm.valor_total - rm.valoriva,

                                         bodega = rm.bodega_id,
                                         rm.vendedor,
                                         rm.dias_validez,
                                         fecela = rm.refmov_fecela,
                                         id = f.codigo,
                                         condicion = f.fpago_nombre,
                                         dias = rm.dias_validez,
                                         pie = rm.descuento_pie,

                                         #region otros campos

                                         //codigo = rd.ref_codigo,
                                         //valor = rd.valor_unitario,
                                         //descuento = rd.pordscto,
                                         //iva = rd.poriva,
                                         //cantidad = rd.refdet_cantidad

                                         #endregion
                                     }).FirstOrDefault();

            string root = Server.MapPath("~/Pdf/");
            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
            string path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);
            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");


            DateTime fechavencimientoA = Convert.ToDateTime(buscar_Cotizacion.fvencoti, usDtfi);
  

            var numerosaletras = new NumerosALetras();
            //datos del asesor
            var idusuario = buscar_Cotizacion.id_usuario;
            var vendedor = context.users.Where(d => d.user_id == idusuario).FirstOrDefault();
            ModeloPdfCotizacion obj = new ModeloPdfCotizacion
            {
                idEmpresa = buscar_Cotizacion.id_empre,
                descripcionEmpresa = buscar_Cotizacion.desempre,
                direccionEmpresa = buscar_Cotizacion.dirempre,
                rifEmpresa = buscar_Cotizacion.rifempre,

                idCotizacion = buscar_Cotizacion.id_coti ?? 0,
                numeroCotizacion = buscar_Cotizacion.numcoti,
                fechaCotizacion = buscar_Cotizacion.feccoti.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                //fechaVencimiento = buscar_Cotizacion.fecela.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                fechaVencimiento = fechavencimientoA.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                nota = buscar_Cotizacion.nota,

                idusuario= vendedor.user_id,
                nombreusuario=vendedor.user_nombre+" "+vendedor.user_apellido,
                idCliente = buscar_Cotizacion.id_clie,
                nitCliente = ": " + Convert.ToString(buscar_Cotizacion.nit),
                docCliente = ": " + buscar_Cotizacion.docclie,
                nomCliente = ": " + buscar_Cotizacion.nomclie,
                dirCliente = ": " + direc,
                telCliente = ": " + buscar_Cotizacion.telclie,
                vigencianum= buscar_Cotizacion.dias_validez,
                vigenciatext= numerosaletras.enletras(buscar_Cotizacion.dias_validez.ToString()),
                valorbruto = detalleCotizacion001.Sum(f => f.valorItem2).ToString("N0"),
                valordescuento =
                    buscar_Cotizacion.valordcto
                        .ToString("N0"), // (detalleCotizacion001.Sum(f=> Convert.ToDecimal(f.dctval))).ToString("N2"),
                valorfletes = buscar_Cotizacion.valorflete.ToString("N0"),
                baseiva = buscar_Cotizacion.baseiva
                    .ToString("N0"), //(detalleCotizacion001.Sum(f => Convert.ToDecimal(f.valorItem))).ToString("N2"),
                valoriva = buscar_Cotizacion.valoriva
                    .ToString("N0"), //(detalleCotizacion001.Sum(f => Convert.ToDecimal(f.valiva))).ToString("N2"),
                valorneto = buscar_Cotizacion.valorneto.ToString("N0"),

                detalleCotizacion = detalleCotizacion001,

                #region pilas

                //detalleCotizacion = (from rm in context.icb_referencia_mov
                //                     join rdm in context.icb_referencia_movdetalle
                //                     on rm.refmov_id equals rdm.refmov_id
                //                     join re in context.icb_referencia
                //                     on rdm.ref_codigo equals re.ref_codigo
                //                     where rm.refmov_id == id
                //                     select new detalleCotizacion
                //                     {
                //                         ref_mov = rdm.refmov_id,

                //                         id_Item = rdm.refdet_id,
                //                         codItem = rdm.ref_codigo,
                //                         numitem = rdm.refdet_id,
                //                         descripcionItem = re.ref_descripcion,
                //                         canItem = rdm.refdet_cantidad,
                //                         dctItem = rdm.pordscto ?? 0,

                //                         ivaItem = rdm.poriva ?? 0,
                //                         pre_unit_Item = rdm.valor_unitario.ToString(),
                //                         valorItem = rdm.valor_total.Value.ToString(),
                //                         pre_unit_dcto_Item = rdm.valor_unitario - rdm.valor_unitario * 100,
                //                         // valor_dcto_Item =
                //                     }).ToList(),

                #endregion
            };


            ViewAsPdf something = new ViewAsPdf("crearPDFcotizacion", obj);
            return something;
            //return View(); // solo mientras esta en espera 
        }

        public ActionResult crearPDFpedido(int? id)
        {
            // tiket 4469 fabian vanegas 22-09-2020 a las 12:30 se trae nombre del vendedor
            var nomvendedor= (from rm in context.icb_referencia_mov
                             join u in context.users
                             on rm.vendedor equals u.user_id
                             where rm.refmov_id == id
                             select new
                             {
                                 nom_vendedor = u.user_nombre.Trim(),
                                 ape_vendedor = u.user_apellido.Trim()
                             }).ToList();
            ViewBag.Nomvendedor = nomvendedor.Max(c => c.nom_vendedor);
            ViewBag.Apevendedor = nomvendedor.Max(c => c.ape_vendedor);
            // = nomvende;

            var direcciontercero = (from rm in context.icb_referencia_mov
                                    join ter in context.terceros_direcciones
                                    on rm.cliente equals ter.idtercero
                                    join u in context.users
                                    on rm.vendedor equals u.user_id 
                                    where rm.refmov_id == id
                                    select new
                                    {
                                        direccion_cliente = ter.direccion
                                    }).ToList();
            string direc = direcciontercero.Max(c => c.direccion_cliente);
             //= direcciontercero.nombrevendedor;

            var data2 = (from rm in context.icb_referencia_mov
                         join rdm in context.icb_referencia_movdetalle
                             on rm.refmov_id equals rdm.refmov_id
                         join re in context.icb_referencia
                         on rdm.ref_codigo equals re.ref_codigo
                         where rm.refmov_id == id
                         select new
                         {
                             ref_mov = rdm.refmov_id,
                             id_Item = rdm.refdet_id,
                             codItem = rdm.ref_codigo,
                             numitem = rdm.refdet_id,
                             descripcionItem = re.ref_descripcion,
                             canItem = rdm.refdet_cantidad,
                             dctItem = rdm.pordscto ?? 0,
                             ivaItem = rdm.poriva ?? 0,
                             pre_unit_Item = rdm.valor_unitario,
                             valorItem = rdm.valor_total
                         }).ToList();

            List<detallePDF_Pedido> detallePedido002 = data2.Select(c => new detallePDF_Pedido
            {
                Pref_mov = c.ref_mov,
                Pid_Item = c.id_Item,
                PcodItem = c.codItem,
                Pnumitem = c.numitem,
                PdescripcionItem = c.descripcionItem,
                PcanItem = c.canItem.ToString("N0"),
                PdctItem = Convert.ToString(c.dctItem),
                Pdctval = (c.canItem * c.pre_unit_Item * (Convert.ToDecimal(c.dctItem, miCultura) / 100)).ToString("N0"),
                Pvaliva = (c.canItem * c.pre_unit_Item * (Convert.ToDecimal(c.ivaItem, miCultura) / 100)).ToString("N0"),
                PivaItem = Convert.ToString(c.ivaItem),
                Ppre_unit_Item = c.pre_unit_Item.ToString("N0"),
                PvalorItem = (c.canItem * c.pre_unit_Item).ToString("N0"), //c.valorItem.Value.ToString("N2"),
                PvalorItem2 = c.canItem * c.pre_unit_Item, //c.valorItem.Value.ToString("N2"),

                Ppre_unit_dcto_Item =
                    (c.canItem * (c.pre_unit_Item - c.pre_unit_Item * (Convert.ToDecimal(c.dctItem, miCultura) / 100)))
                    .ToString("N0"),
                Pvalor_dcto_Item =
                    (c.valorItem ?? 0 - (c.valorItem ?? 0 * (Convert.ToDecimal(c.dctItem, miCultura) / 100))).ToString("N0")
            }).ToList();

            var buscar_Pedido = (from rm in context.icb_referencia_mov
                                     //join rd in context.icb_referencia_movdetalle
                                     //on rm.refmov_id equals rd.refmov_id
                                 join c in context.tercero_cliente
                                     on rm.cliente equals c.tercero_id
                                 join f in context.fpago_tercero
                                     on c.cod_pago_id equals f.fpago_id
                                 join ter in context.icb_terceros
                                     on rm.cliente equals ter.tercero_id
                                 join bc in context.bodega_concesionario
                                     on rm.bodega_id equals bc.id
                                 join empr in context.tablaempresa
                                     on bc.concesionarioid equals empr.id
                                 //join dire in context.terceros_direcciones
                                 //on ter.tercero_id equals dire.idtercero
                                 where rm.refmov_id == id
                                 select new
                                 {
                                     Pid_empre = empr.id,
                                     Pdesempre = empr.nombre_empresa,
                                     Pdirempre = empr.direccion,
                                     Prifempre = empr.nit,

                                     Pref_mov = rm.refmov_id,
                                     Pid_ped = rm.refmov_id,
                                     Pnumped = rm.refmov_numero,
                                     Pfecped = rm.refmov_fecela,
                                     Pfvenped = rm.fecha_vencimiento,

                                     Pnota = "",

                                     Pid_clie = c.tercero_id,
                                     Pnitclie = ter.doc_tercero,
                                     Pdocclie = ter.doc_tercero,
                                     Pnomclie = ter.tpdoc_id != 1
                                         ? ter.prinom_tercero + " " + ter.segnom_tercero + " " + ter.apellido_tercero + " " +
                                           ter.segapellido_tercero
                                         : ter.razon_social,
                                     Ptelclie = ter.telf_tercero + " " + ter.celular_tercero,
                                     Pnit = rm.cliente,
                                     rm.dias_validez,
                                     Pvalorflete = rm.fletes,
                                     Pvalorneto = rm.valor_total,
                                     Pvaloriva = rm.valoriva,
                                     Pvalordcto = rm.valordescuento,
                                     Pbaseiva = rm.valor_total - rm.valoriva,

                                     Pbodega = rm.bodega_id,
                                     Pvendedor = rm.vendedor,
                                     Pfecela = rm.refmov_fecela,
                                     Pid = f.codigo,
                                     Pcondicion = f.fpago_nombre,
                                     Pdias = rm.dias_validez,
                                     Ppie = rm.descuento_pie
                                 }).FirstOrDefault();

            string root = Server.MapPath("~/Pdf/");
            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
            string path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);
            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");


            DateTime PfechavencimientoA = Convert.ToDateTime(buscar_Pedido.Pfvenped, usDtfi);
         
            var numerosaletras = new NumerosALetras();

            ModeloPdfPedido objPedido = new ModeloPdfPedido
            {
                PidEmpresa = buscar_Pedido.Pid_empre,
                PdescripcionEmpresa = buscar_Pedido.Pdesempre,
                PdireccionEmpresa = buscar_Pedido.Pdirempre,
                PrifEmpresa = buscar_Pedido.Prifempre,

                PidPedido = buscar_Pedido.Pid_ped,
                PnumeroPedido = buscar_Pedido.Pnumped,
                PfechaPedido = buscar_Pedido.Pfecped.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                //fechaVencimiento = buscar_Cotizacion.fecela.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                PfechaVencimiento = PfechavencimientoA.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                Pnota = buscar_Pedido.Pnota,


                PidCliente = buscar_Pedido.Pid_clie,
                PnitCliente = ": " + Convert.ToString(buscar_Pedido.Pnit),
                PdocCliente = ": " + buscar_Pedido.Pdocclie,
                PnomCliente = ": " + buscar_Pedido.Pnomclie,
                PdirCliente = ": " + direc,
                PtelCliente = ": " + buscar_Pedido.Ptelclie,
                vigencianum = buscar_Pedido.dias_validez,
                vigenciatext = numerosaletras.enletras(buscar_Pedido.dias_validez.ToString()),
                Pvalorbruto = detallePedido002.Sum(f => f.PvalorItem2).ToString("N0"),
                Pvalordescuento =
                    buscar_Pedido.Pvalordcto
                        .ToString("N0"), // (detalleCotizacion001.Sum(f=> Convert.ToDecimal(f.dctval))).ToString("N2"),
                Pvalorfletes = buscar_Pedido.Pvalorflete.ToString("N0"),
                Pbaseiva = buscar_Pedido.Pbaseiva
                    .ToString("N0"), //(detalleCotizacion001.Sum(f => Convert.ToDecimal(f.valorItem))).ToString("N2"),
                Pvaloriva = buscar_Pedido.Pvaloriva
                    .ToString("N0"), //(detalleCotizacion001.Sum(f => Convert.ToDecimal(f.valiva))).ToString("N2"),
                Pvalorneto = buscar_Pedido.Pvalorneto.ToString("N0"),

                detallePDF_Pedido = detallePedido002
            };

            ViewAsPdf something = new ViewAsPdf("crearPDFpedido", objPedido);
            return something;
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
        // ticket 4475 fabian vanegas fecha 23-09-2020 10:20 am para filtrar documentos coti repuestos y pedidos se traen de la tabla de parametros
        public void BuscarParametros()
        {
            var BuscaPara = (from param in context.icb_sysparameter
                             where param.syspar_cod == "P162" || param.syspar_cod == "P163"
                             select new
                             {
                                 parcod = param.syspar_cod,
                                 parvalue = param.syspar_value
                             }).ToList();
            foreach (var parametro in BuscaPara)
            {
                PCotRep = parametro.parcod == "P162" ? Convert.ToInt32(parametro.parvalue) : PCotRep;
                PPedRep = parametro.parcod == "P163" ? Convert.ToInt32(parametro.parvalue) : PPedRep;
            }
        }

        #region Para Crystal No funciona aun

        //public ActionResult exportReport(int? id)
        //{
        //    var buscar_Cotizacion = (from rm in context.icb_referencia_mov
        //                                 //join rd in context.icb_referencia_movdetalle
        //                                 //on rm.refmov_id equals rd.refmov_id
        //                             join c in context.tercero_cliente
        //                             on rm.cliente equals c.tercero_id
        //                             join f in context.fpago_tercero
        //                             on c.cod_pago_id equals f.fpago_id
        //                             join ter in context.icb_terceros
        //                             on rm.cliente equals ter.tercero_id
        //                             join bc in context.bodega_concesionario
        //                             on rm.bodega_id equals bc.id
        //                             join empr in context.tablaempresa
        //                             on bc.concesionarioid equals empr.id
        //                             where rm.refmov_id == id
        //                             select new
        //                             {
        //                                 id_empre = empr.id,
        //                                 desempre = empr.nombre_empresa,
        //                                 dirempre = empr.direccion,
        //                                 rifempre = empr.nit,

        //                                 ref_mov = rm.refmov_id,
        //                                 id_coti = rm.idcotizacion,
        //                                 numcoti = rm.refmov_numero,
        //                                 feccoti = rm.refmov_fecela,
        //                                 fvecoti = rm.refmov_fecela,


        //                                 nota = "",
        //                                 #region valores de montos totales
        //                                 //valorbruto =   
        //                                 //valordescuento = 
        //                                 //valorfletes =
        //                                 //baseiva =
        //                                 //valoriva =
        //                                 //valorneto =
        //                                 #endregion
        //                                 id_clie = c.tercero_id,
        //                                 nitclie = ter.doc_tercero,
        //                                 docclie = ter.doc_tercero,
        //                                 nomclie = ter.tpdoc_id != 1 ? ter.prinom_tercero + " " + ter.segnom_tercero + " " + ter.apellido_tercero + " " + ter.segapellido_tercero : ter.razon_social,
        //                                 dirclie = "",
        //                                 telclie = ter.telf_tercero + "-- " + ter.celular_tercero,
        //                                 nit = rm.cliente,

        //                                 bodega = rm.bodega_id,
        //                                 vendedor = rm.vendedor,
        //                                 fecela = rm.refmov_fecela,
        //                                 id = f.codigo,
        //                                 condicion = f.fpago_nombre,
        //                                 dias = rm.dias_validez,
        //                                 pie = rm.descuento_pie,
        //                                 #region otros campos
        //                                 //codigo = rd.ref_codigo,
        //                                 //valor = rd.valor_unitario,
        //                                 //descuento = rd.pordscto,
        //                                 //iva = rd.poriva,
        //                                 //cantidad = rd.refdet_cantidad
        //                                 #endregion
        //                             }).FirstOrDefault();

        //    ModeloPdfCotizacion obj = new ModeloPdfCotizacion()
        //    {
        //        idEmpresa = buscar_Cotizacion.id_empre,
        //        descripcionEmpresa = buscar_Cotizacion.desempre,
        //        direccionEmpresa = buscar_Cotizacion.dirempre,
        //        rifEmpresa = buscar_Cotizacion.rifempre,

        //        idCotizacion = buscar_Cotizacion.id_coti ?? 0,
        //        numeroCotizacion = buscar_Cotizacion.numcoti,
        //        fechaCotizacion = buscar_Cotizacion.feccoti.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
        //        fechaVencimiento = buscar_Cotizacion.fecela.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
        //        nota = buscar_Cotizacion.nota,


        //        idCliente = buscar_Cotizacion.id_clie,
        //        nitCliente = buscar_Cotizacion.nit,
        //        docCliente = buscar_Cotizacion.docclie,
        //        nomCliente = buscar_Cotizacion.nomclie,
        //        dirCliente = "-" + buscar_Cotizacion.dirclie,
        //        telCliente = buscar_Cotizacion.telclie,


        //        detalleCotizacion = (from rm in context.icb_referencia_mov
        //                             join rdm in context.icb_referencia_movdetalle
        //                             on rm.refmov_id equals rdm.refmov_id
        //                             join re in context.icb_referencia
        //                             on rdm.ref_codigo equals re.ref_codigo
        //                             where rm.refmov_id == id
        //                             select new detalleCotizacion
        //                             {
        //                                 ref_mov = rdm.refmov_id,

        //                                 id_Item = rdm.refdet_id,
        //                                 codItem = rdm.ref_codigo,
        //                                 numitem = rdm.refdet_id,
        //                                 descripcionItem = re.ref_descripcion,
        //                                 //canItem = rdm.refdet_cantidad,
        //                                 //dctItem = rdm.pordscto ?? 0,

        //                                 //ivaItem = rdm.poriva ?? 0,
        //                                 //pre_unit_Item = rdm.valor_unitario.ToString(),
        //                                 //valorItem = rdm.valor_total.Value.ToString(),
        //                                 //pre_unit_dcto_Item = rdm.valor_unitario - rdm.valor_unitario * 100,
        //                                 // valor_dcto_Item =
        //                             }).ToList(),
        //    };


        //    ReportDocument rd = new ReportDocument();
        //    rd.Load(Path.Combine(Server.MapPath("../ReportesCrystal"), "CrystalReport.rpt"));
        //    rd.SetDataSource(obj.ToString());
        //    //crystalReportViewer1.ReportSource = rpt;
        //    //rd.SetDataSource(o);
        //    Response.Buffer = false;
        //    Response.ClearContent();
        //    Response.ClearHeaders();
        //    try
        //    {
        //        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
        //        stream.Seek(0, SeekOrigin.Begin);
        //        return File(stream, "application/pdf", "Cotizacion.pdf" );
        //    }
        //    catch
        //    {
        //        throw;
        //    }

        //}

        #endregion
    }
}