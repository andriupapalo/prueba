using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class kardexController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        private readonly CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
        CultureInfo miCultura = new CultureInfo("is-IS");

        public class listaMovimientos
        {
            public string bodega { get; set; }
            public int? sw { get; set; }
            public int? idencabezado { get; set; }
            public string prefijo { get; set; }
            public string tpdoc_nombre { get; set; }
            public long numero { get; set; }
            public string prefijoope { get; set; }
            public string tpdoc_nombreope { get; set; }
            public long numeroope { get; set; }
            public DateTime? fec_creacion { get; set; }
            public string fecha { get; set; }
            public string cantEntrada { get; set; }
            public decimal? cant_entrada { get; set; }
            public string cantSalida { get; set; }
            public decimal? cant_salida { get; set; }
            public string costoEntrada { get; set; }
            public decimal? costo_entrada { get; set; }
            public string costoSalida { get; set; }
            public decimal? costo_salida { get; set; }
            public string costoTotal { get; set; }
            public decimal? costo_total { get; set; }
            public int? documento_interno { get; set; }
            public string documentoExterno { get; set; }
            public decimal? saldo { get; set; }
            public string saldo2 { get; set; }

        }
        public ActionResult KardexRepuestos(string id)
        {
            //ViewBag.codigo = context.icb_referencia.Where(x => x.modulo == "R").ToList();
            if (!string.IsNullOrWhiteSpace(id))
            {
                //busco si el codigo corresponde a una referencia
                icb_referencia refer = context.icb_referencia.Where(d => d.ref_codigo == id).FirstOrDefault();
                if (refer != null)
                {
                    ViewBag.codigo = refer.ref_codigo + " | " + refer.ref_descripcion;
                }
            }
            else
            {
                ViewBag.codigo = "";
            }

            int rol = Convert.ToInt32(Session["user_rolid"]);

            var permisos = (from acceso in context.rolacceso
                           join rolPerm in context.rolpermisos
                           on acceso.idpermiso equals rolPerm.id
                           where acceso.idrol == rol //&& rolPerm.codigo == "P40" 
                           select new { rolPerm.codigo }).ToList();

            var resultado = permisos.Where(x => x.codigo == "P40").Count() > 0 ? "Si" : "No";

            ViewBag.Permiso = resultado;
            return View();
        }

        public ActionResult KardexVehiculos()
        {
            ViewBag.codigo = context.icb_referencia.Where(x => x.modulo == "V").ToList();
            return View();
        }

        public JsonResult BuscarBodegas()
        {
            //me traigo la bodega de la sesión si existe. Esta variable es de tipo OBJETO
            object bodega = Session["user_bodega"];
            int idbodega = 0;
            //si esa variable existe
            if (bodega != null)
            {
                //trato de convertirla a entero. Para ello primero coloco la variable bodega a string y luego uso el método int.TryParse para intentar hacer la conversion

                bool convertir = int.TryParse(bodega.ToString(), out idbodega);
            }
            var listarBodegas = (from b in context.bodega_concesionario
                                 select new
                                 {
                                     b.id,
                                     b.bodccs_nombre,
                                     selected = b.id == idbodega ? 1 : 0,
                                 }).OrderBy(x => x.bodccs_nombre).ToList();

            return Json(listarBodegas, JsonRequestBehavior.AllowGet);
        }

        //Json anterior que se solicito cambiar completo el dia 9 de abril con el ticket 3432-999
        //public JsonResult BuscarKardexRepuestos(int anio, int mes, int?[] bodega, string codigo)
        //{
        //	var anioP = PredicateBuilder.True<vw_kardex2>();

        //	var bodegaP = PredicateBuilder.False<vw_kardex2>();
        //	var codigoP = PredicateBuilder.False<vw_kardex2>();

        //	anioP = anioP.And(x => x.ano == anio && x.mes == mes && x.modulo == "R");

        //	#region Predicate bodega
        //	if (bodega.Count() > 0 && bodega[0] != null)
        //	{
        //		foreach (var item in bodega)
        //		{
        //			if (item != null)
        //			{
        //				bodegaP = bodegaP.Or(d => d.bodega == item);
        //			}
        //		}
        //		anioP = anioP.And(bodegaP);
        //	}
        //	#endregion
        //	#region Predicate codigo
        //	if (!String.IsNullOrEmpty(codigo))
        //	{
        //		codigoP = codigoP.Or(d => d.codigo == codigo);
        //		anioP = anioP.And(codigoP);
        //	}
        //	#endregion

        //	var consultaOrig = context.vw_kardex2.Where(anioP).ToList();
        //	var consulta = consultaOrig.Select(c => new
        //	{
        //		c.bodega,
        //		c.bodccs_nombre,
        //		c.codigo,
        //		c.ref_descripcion,
        //		c.ano,
        //		c.mes,
        //		c.can_ini,
        //		c.can_ent,
        //		c.can_sal,
        //		c.cos_ini, // = c.cos_ini.ToString("N0", new CultureInfo("is-IS")),
        //		c.cos_ent, //= c.cos_ent.ToString("N0", new CultureInfo("is-IS")),
        //		c.cos_sal, //= c.cos_sal.ToString("N0", new CultureInfo("is-IS")),
        //		c.stock,
        //		separado = c.separado != null ? c.separado : 0,
        //		c.CostoStock,
        //		c.modulo,
        //		costoProm = c.costoProm != null ? Math.Truncate(Convert.ToDecimal(c.costoProm)) : 0, // Math.Truncate(Convert.ToDecimal(c.costoProm) * 100) / 100:0,
        //																							 //costoProm = c.costoProm != null ? c.costoProm :0 ,

        //		ClasifABC = c.ClasifABC != null ? c.ClasifABC : "",
        //		precioVenta = c.precioVenta != null ? Math.Truncate(Convert.ToDecimal(c.precioVenta)) : 0,// c.precioVenta.Value.ToString("N0", new CultureInfo("is-IS")) : "",
        //		fechaUltimacompra = c.fechaUltimacompra != null ? c.fechaUltimacompra.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : ""

        //	}).ToList();
        //	var consultaB = context.vw_kardex2.Where(anioP).Select(x => new { x.bodega, x.bodccs_nombre }).Distinct().ToList();

        //	if (consulta.Count > 0 && consultaB.Count > 0)
        //	{
        //		var data = new
        //		{
        //			consulta,
        //			consultaB
        //		};

        //		return Json(new { info = true, data }, JsonRequestBehavior.AllowGet);
        //	}
        //	else
        //	{
        //		return Json(new { info = false }, JsonRequestBehavior.AllowGet);
        //	}

        //}


        public JsonResult BuscarKardexRepuestos(int? anioDesde, int? mesDesde, int? anioHasta, int? mesHasta, int? bodega, string codigo)
        {
            decimal stockInicial = 0;
            decimal stockFinal = 0;
            int anio_act = DateTime.Now.Year;
            int mes_act = DateTime.Now.Month;

            

            if (anioDesde == null)
            {
                //busco el ultimo anio de la referencia en referencias inven
                var ultimoanio = context.referencias_inven.OrderBy(d => d.ano).ThenBy(d => d.mes).Where(x => x.codigo == codigo && x.bodega== bodega).FirstOrDefault();
                if (ultimoanio != null)
                {
                    anioDesde = ultimoanio.ano;
                }
                else
                {
                    anioDesde = DateTime.Now.Year;
                }

            }
            if (mesDesde == null)
            {
                //busco el ultimo anio de la referencia en referencias inven
                var ultimoanio = context.referencias_inven.OrderBy(d => d.ano).ThenBy(d => d.mes).Where(x => x.codigo == codigo && x.bodega == bodega).FirstOrDefault();
                if (ultimoanio != null)
                {
                    mesDesde = ultimoanio.mes;
                }
                else
                {
                    mesDesde = DateTime.Now.Month;
                }

            }

            vw_kardex2 stockInicial2 = context.vw_kardex2.OrderBy(x => x.mes).FirstOrDefault(x => x.codigo == codigo && x.ano==anioDesde && x.mes==mesDesde && x.bodega == bodega);


            if (anioHasta == null)
            {
                //busco el ultimo anio de la referencia en referencias inven
                var ultimoanio = context.referencias_inven.OrderByDescending(d => d.ano).ThenByDescending(d => d.mes).Where(x => x.codigo == codigo && x.bodega == bodega).FirstOrDefault();
                if (ultimoanio != null)
                {
                    anioHasta = ultimoanio.ano;                 
                }
                else
                {
                    anioHasta = DateTime.Now.Year;
                }
                
            }
            if (mesHasta == null)
            {
                //busco el ultimo anio de la referencia en referencias inven
                var ultimoanio = context.referencias_inven.OrderByDescending(d => d.ano).ThenByDescending(d => d.mes).Where(x => x.codigo == codigo && x.bodega == bodega).FirstOrDefault();
                if (ultimoanio != null)
                {
                    mesHasta = ultimoanio.mes;
                }
                else
                {
                    mesHasta = DateTime.Now.Month;
                }

            }

            var stockFinal2 = (from x in context.vw_kardex2
                               where x.codigo == codigo && x.ano==anioHasta && x.mes==mesHasta && x.bodega==bodega
                               select new
                               {
                                   x.stock
                               }).ToList();
            // Se cambia por ticket 4221-2192
            //if (anioHasta == anio_act)
            //{
            //    if (mesHasta >= mes_act)
            //    {
            //        stockInicial2 = context.vw_kardex2.OrderBy(x => x.mes).FirstOrDefault(x => x.ano >= anioDesde && bodega.Contains(x.bodega) && x.codigo == codigo);
            //    }
            //    else
            //    {
            //        stockInicial2 = context.vw_kardex2.OrderBy(x => x.mes).FirstOrDefault(x => x.ano >= anioDesde && x.mes >= mesDesde && bodega.Contains(x.bodega) && x.codigo == codigo);
            //    }
            //}
            //else
            //{
            //    stockInicial2 = context.vw_kardex2.OrderBy(x => x.mes).FirstOrDefault(x => x.ano >= anioDesde && x.mes >= mesDesde && bodega.Contains(x.bodega) && x.codigo == codigo);
            //}
            var infoEncabezado = (from x in context.vw_kardex2
                                  where x.ano == anioHasta && x.mes == mesHasta && x.bodega == bodega && x.codigo == codigo
                                  select new
                                  {
                                      x.costoProm,
                                      x.fechaUltimacompra,
                                      x.ClasifABC,
                                      x.unidad_medida,
                                      x.ref_cantidad_min,
                                      x.precio_alterno,
                                      x.precio_diesel,
                                      x.precio_garantia
                                  }).FirstOrDefault();

            if (stockFinal2 != null && infoEncabezado != null)
            {
                stockFinal = stockFinal2.Sum(x => x.stock) != null
                    ? Convert.ToDecimal(stockFinal2.Sum(x => x.stock), miCultura)
                    : 0;
                stockInicial = stockInicial2!=null? Convert.ToDecimal(stockInicial2.can_ini, miCultura):0;
                System.Collections.Generic.List<rseparacionmercancia> separados = context.rseparacionmercancia
                    .Where(x => x.bodega == bodega && x.codigo == codigo && x.estado).ToList();
                int separa = 0;
                if (separados != null)
                {
                    foreach (rseparacionmercancia item in separados)
                    {
                        separa += item.cantidad;
                    }
                }

                var ultimoPrecio = (from k in context.vw_kardex2
                                    where k.bodega == bodega && k.codigo == codigo
                                    select new
                                    {
                                        k.fechaUltimacompra,
                                        uPrecio = from l in context.lineas_documento
                                                  where k.fechaUltimacompra.Value.Year == l.fec.Year
                                                        && k.fechaUltimacompra.Value.Month == l.fec.Month
                                                        && k.fechaUltimacompra.Value.Day == l.fec.Day
                                                        && k.fechaUltimacompra.Value.Hour == l.fec.Hour
                                                        && k.fechaUltimacompra.Value.Minute == l.fec.Minute
                                                  select l.valor_unitario
                                    }).FirstOrDefault();
                decimal precioU = 0;
                foreach (decimal item in ultimoPrecio.uPrecio)
                {
                    precioU = item;
                };

                decimal precioVenta = (from a in context.rprecios
                                       where a.codigo == codigo
                                       select a.precio1
                    ).FirstOrDefault();


                var data = new
                {
                    stockInicial = stockInicial.ToString("0,0", elGR),
                    stockFinal = stockFinal.ToString("0,0", elGR),
                    costoProm = infoEncabezado.costoProm !=null ? Convert.ToDecimal(infoEncabezado.costoProm).ToString("0,0", elGR) : "",
                    infoEncabezado.fechaUltimacompra,
                    infoEncabezado.ClasifABC,
                    unidad_medida = infoEncabezado.unidad_medida != null ? infoEncabezado.unidad_medida : "",
                    referencia = codigo,
                    ref_cantidad_min = infoEncabezado.ref_cantidad_min.ToString("0,0", elGR),
                    separa = separa.ToString("0,0", elGR),
                    fechaUltimacompras = ultimoPrecio.fechaUltimacompra != null
                        ? ultimoPrecio.fechaUltimacompra.Value.ToString("yyyy/MM/dd")
                        : "",
                    precioU = precioU.ToString("0,0", elGR),
                    precioVenta = precioVenta != null ? precioVenta : 0,
                    infoEncabezado.precio_alterno,
                    infoEncabezado.precio_garantia,
                    infoEncabezado.precio_diesel,
                };

                return Json(new { info = true, data }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { info = false }, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult listarUbicacion(string codigo, int bodega)
        //{
        //    var data = (from a in context.ubicacion_repuesto
        //                join b in context.ubicacion_repuestobod
        //                    on a.ubicacion equals b.id
        //                where a.codigo == codigo && a.bodega == bodega
        //                select new
        //                {
        //                    b.id,
        //                    b.descripcion
        //                }).ToList();

        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}
        public JsonResult listarUbicacion(string codigo,int bodega)
        {
            var ubicacion = (from ubicaciones in context.ubicacion_repuesto
                             join bodegax in context.bodega_concesionario on ubicaciones.bodega equals bodegax.id
                             join ubiBod in context.ubicacion_repuestobod on ubicaciones.ubicacion equals ubiBod.id
                             join referencia in context.icb_referencia on ubicaciones.codigo equals referencia.ref_codigo
                             join area in context.area_bodega on ubicaciones.idarea equals area.areabod_id into zz
                             from area in zz.DefaultIfEmpty()
                             where referencia.ref_codigo == codigo && bodegax.id == bodega
                             select new
                             {
                                 ubicaciones.id,
                                 bodegax.bodccs_nombre,
                                 bodega=bodegax.id,
                                 referencia.ref_codigo,
                                 referencia.ref_descripcion,
                                 ubicacion = ubiBod.descripcion,
                                 area = area.areabod_nombre != null ? area.areabod_nombre : "",
                                 ubicaciones.notaUbicacion
                             }).ToList();
                return Json(ubicacion, JsonRequestBehavior.AllowGet);

        }



        public JsonResult Datoscomprometidosmodal(string codigo,  int bodega) {

            string nombodega = context.bodega_concesionario.Where(x => x.id == bodega).Select(a => a.bodccs_nombre).FirstOrDefault();




            List<ListaComprometidos> listacompromet = new List<ListaComprometidos>();

            var buscardatosSeparacion = (from s in context.vw_separacionComprometidos
                                         where s.codigo== codigo && s.bodccs_nombre == nombodega
                                         select new
                                             {
                                             s.id,
                                             s.codigo,
                                             s.bodccs_nombre,
                                             s.cliente,
                                             s.fecha2,
                                             s.responsable,
                                             s.diasComprometidos,
                                             s.ref_descripcion,
                                             s.numero
                                             }).ToList();



            var datosSeparacion = buscardatosSeparacion.Select(d => new
                {

                id = d.id,
                d.codigo,
                bodega = d.bodccs_nombre,
                d.cliente,
                fecha = d.fecha2,
                d.responsable,
                d.diasComprometidos,
                d.ref_descripcion,
                numero = d.numero
                }).ToList();


            foreach (var item in datosSeparacion)
                {
                ListaComprometidos listacom = new ListaComprometidos();
                listacom.id = item.id;
                listacom.Numero = Convert.ToInt32(item.numero);
                listacom.Codigo = item.codigo;
                listacom.Ref_descripcion = item.ref_descripcion;
                listacom.Bodega = item.bodega;
                listacom.Cliente = item.cliente;
                listacom.Fecha = item.fecha;
                listacom.Dias_comprometidos = Convert.ToInt32(item.diasComprometidos);
                listacom.Responsable = item.responsable;
                listacom.Tipoproceso = "Separacion de Mercancia";
                listacom.Tp = "Sep";
                listacompromet.Add(listacom);
                }




            //Anticipo


            //OT
            var buscardatosOT = (from s in context.vw_otComprometidos
                                 where s.codigo == codigo && s.bodccs_nombre == nombodega
                                 select new
                                     {
                                     s.id,
                                     s.codigo,
                                     s.bodccs_nombre,
                                     s.cliente,
                                     s.fecha,
                                     s.fecha2,
                                     s.responsable,
                                     s.diasComprometidos,
                                     s.ref_descripcion,
                                     s.numero
                                     }).ToList();


            var datosOT = buscardatosOT.Select(d => new
                {

                id = d.id,
                d.codigo,
                bodega = d.bodccs_nombre,
                d.cliente,
                fecha = d.fecha2,
                d.responsable,
                d.diasComprometidos,
                d.ref_descripcion,
                numero = d.numero
                }).ToList();


            foreach (var item in datosOT)
                {
                ListaComprometidos listacom = new ListaComprometidos();
                listacom.id = item.id;
                listacom.Numero = Convert.ToInt32(item.numero);
                listacom.Codigo = item.codigo;
                listacom.Ref_descripcion = item.ref_descripcion;
                listacom.Bodega = item.bodega;
                listacom.Cliente = item.cliente;
                listacom.Fecha = item.fecha;
                listacom.Dias_comprometidos = Convert.ToInt32(item.diasComprometidos);
                listacom.Responsable = item.responsable;
                listacom.Tipoproceso = "Orden de  Trabajo";
                listacom.Tp = "Ot";
                listacompromet.Add(listacom);
                }



            //Traslado

            var buscardatosTraslado = (from s in context.vw_documentoComprometidos
                                       where s.tipo == 1028 &&  s.codigo == codigo && s.bodccs_nombre == nombodega
                                       select new
                                           {
                                           s.idencabezado,
                                           s.codigo,
                                           s.bodccs_nombre,
                                           s.cliente,
                                           s.fecha,
                                           s.fecha2,
                                           s.responsable,
                                           s.diasComprometidos,
                                           s.ref_descripcion,
                                           s.numero

                                           }).ToList();





            var datosTraslado = buscardatosTraslado.GroupBy(d => new { d.idencabezado, d.codigo }).Select(d => new
                {

                id = d.Select(e => e.idencabezado).FirstOrDefault(),
                numero = d.Select(e => e.numero).FirstOrDefault(),
                codigo = d.Select(e => e.codigo).FirstOrDefault(),
                bodega = d.Select(e => e.bodccs_nombre).FirstOrDefault(),
                cliente = d.Select(e => e.cliente).FirstOrDefault(),
                fecha = d.Select(e => e.fecha2).FirstOrDefault(),
                responsable = d.Select(e => e.responsable).FirstOrDefault(),
                diasComprometidos = d.Select(e => e.diasComprometidos).FirstOrDefault(),
                ref_descripcion = d.Select(e => e.ref_descripcion).FirstOrDefault(),
                }).ToList();


            foreach (var item in datosTraslado)
                {
                ListaComprometidos listacom = new ListaComprometidos();
                listacom.id = item.id;
                listacom.Numero = Convert.ToInt32(item.numero);
                listacom.Codigo = item.codigo;
                listacom.Ref_descripcion = item.ref_descripcion;
                listacom.Bodega = item.bodega;
                listacom.Cliente = item.cliente;
                listacom.Fecha = item.fecha;
                listacom.Dias_comprometidos = Convert.ToInt32(item.diasComprometidos);
                listacom.Responsable = item.responsable;
                listacom.Tipoproceso = "Traslado de Repuestos";
                listacom.Tp = "Tr";
                listacompromet.Add(listacom);
                }




            //Solicitud de despacho

            var buscardatosSolicitud = (from s in context.vw_solicitudComprometidos
                                        where s.codigo == codigo && s.bodccs_nombre == nombodega
                                        select new
                                            {
                                            s.id,
                                            s.codigo,
                                            s.bodccs_nombre,
                                            s.cliente,
                                            s.fecha2,
                                            s.responsable,
                                            s.diasComprometidos,
                                            s.ref_descripcion,
                                            s.numero

                                            }).ToList();

            var datosSolicitud = buscardatosSolicitud.Select(d => new
                {

                id = d.id,
                d.codigo,
                bodega = d.bodccs_nombre,
                d.cliente,
                fecha = d.fecha2,
                d.responsable,
                d.diasComprometidos,
                d.ref_descripcion,
                numero =  d.numero

                }).ToList();

            foreach (var item in datosSolicitud)
                {
                ListaComprometidos listacom = new ListaComprometidos();
                listacom.id = item.id;
                listacom.Numero = Convert.ToInt32(item.numero);
                listacom.Codigo = item.codigo;
                listacom.Ref_descripcion = item.ref_descripcion;
                listacom.Bodega = item.bodega;
                listacom.Cliente = item.cliente;
                listacom.Fecha = item.fecha;
                listacom.Dias_comprometidos = Convert.ToInt32(item.diasComprometidos);
                listacom.Responsable = item.responsable;
                listacom.Tipoproceso = "Solicitud de Despacho";
                listacom.Tp = "Sd";
                listacompromet.Add(listacom);
                }




            //Despacho

            var buscardatosDespacho = (from s in context.vw_documentoComprometidos
                                       where s.tipo == 3074 && s.codigo == codigo && s.bodccs_nombre == nombodega
                                       select new
                                           {
                                           s.idencabezado,
                                           s.codigo,
                                           s.bodccs_nombre,
                                           s.cliente,
                                           s.fecha,
                                           s.fecha2,
                                           s.responsable,
                                           s.diasComprometidos,
                                           s.ref_descripcion,
                                           s.numero
                                           }).ToList();

            var datosDespacho = buscardatosDespacho.GroupBy(d => new { d.idencabezado, d.codigo }).Select(d => new
                {

                id = d.Select(e => e.idencabezado).FirstOrDefault(),
                numero = d.Select(e => e.numero).FirstOrDefault(),
                codigo = d.Select(e => e.codigo).FirstOrDefault(),
                bodega = d.Select(e => e.bodccs_nombre).FirstOrDefault(),
                cliente = d.Select(e => e.cliente).FirstOrDefault(),
                fecha = d.Select(e => e.fecha2).FirstOrDefault(),
                responsable = d.Select(e => e.responsable).FirstOrDefault(),
                diasComprometidos = d.Select(e => e.diasComprometidos).FirstOrDefault(),
                ref_descripcion = d.Select(e => e.ref_descripcion).FirstOrDefault()

                }).ToList();


            foreach (var item in datosDespacho)
                {
                ListaComprometidos listacom = new ListaComprometidos();
                listacom.id = item.id;
                listacom.Numero = Convert.ToInt32(item.numero);
                listacom.Codigo = item.codigo;
                listacom.Ref_descripcion = item.ref_descripcion;
                listacom.Bodega = item.bodega;
                listacom.Cliente = item.cliente;
                listacom.Fecha = item.fecha;
                listacom.Dias_comprometidos = Convert.ToInt32(item.diasComprometidos);
                listacom.Responsable = item.responsable;
                listacom.Tipoproceso = "Despacho";
                listacom.Tp = "De";
                listacompromet.Add(listacom);
                }





            //Remision

            var buscardatosRemision = (from s in context.vw_documentoComprometidos
                                       where s.tipo == 3037 && s.codigo == codigo && s.bodccs_nombre == nombodega
                                       select new
                                           {
                                           s.idencabezado,
                                           s.codigo,
                                           s.bodccs_nombre,
                                           s.cliente,
                                           s.fecha,
                                           s.fecha2,
                                           s.responsable,
                                           s.diasComprometidos,
                                           s.ref_descripcion,
                                           s.numero

                                           }).ToList();

            var datosRemision = buscardatosRemision.GroupBy(d => new { d.idencabezado, d.codigo }).Select(d => new
                {

                id = d.Select(e => e.idencabezado).FirstOrDefault(),
                numero = d.Select(e => e.numero).FirstOrDefault(),
                codigo = d.Select(e => e.codigo).FirstOrDefault(),
                bodega = d.Select(e => e.bodccs_nombre).FirstOrDefault(),
                cliente = d.Select(e => e.cliente).FirstOrDefault(),
                fecha = d.Select(e => e.fecha2).FirstOrDefault(),
                responsable = d.Select(e => e.responsable).FirstOrDefault(),
                diasComprometidos = d.Select(e => e.diasComprometidos).FirstOrDefault(),
                ref_descripcion = d.Select(e => e.ref_descripcion).FirstOrDefault()
                }).ToList();


            foreach (var item in datosRemision)
                {
                ListaComprometidos listacom = new ListaComprometidos();
                listacom.id = item.id;
                listacom.Numero = Convert.ToInt32(item.numero);
                listacom.Codigo = item.codigo;
                listacom.Ref_descripcion = item.ref_descripcion;
                listacom.Bodega = item.bodega;
                listacom.Cliente = item.cliente;
                listacom.Fecha = item.fecha;
                listacom.Dias_comprometidos = Convert.ToInt32(item.diasComprometidos);
                listacom.Responsable = item.responsable;
                listacom.Tipoproceso = "Remision";
                listacom.Tp = "Rem";
                listacompromet.Add(listacom);
                }






            return Json(listacompromet, JsonRequestBehavior.AllowGet);
            }
        public JsonResult listarMovimientos(string codigo, int[] bodega, int anioDesde, int mesDesde, int anioHasta,
            int mesHasta)
        {
            decimal saldox = 0;

            var stockini = context.referencias_inven.OrderBy(a=>a.ano).ThenBy(a=>a.mes).Where(a => a.codigo == codigo && bodega.Contains(a.bodega)
                                                  && a.ano >= anioDesde && a.mes >= mesDesde).FirstOrDefault();
            if (stockini != null)
            {

                //veo si hay stock en el mes anterior
                int anoanterior = stockini.ano;
                int resta = 1;
                int mesanterior = Convert.ToInt32(stockini.mes) - resta;
                if (mesanterior == 0)
                {
                    anoanterior = Convert.ToInt32(stockini.ano)- resta;
                    mesanterior = 12;
                }
                var existestockanterior= context.referencias_inven.OrderBy(a => a.ano).ThenBy(a => a.mes).Where(a => a.codigo == codigo && bodega.Contains(a.bodega)
                                                       && a.ano == anoanterior && a.mes == mesanterior).FirstOrDefault();
                if (existestockanterior != null)
                {
                    saldox = existestockanterior.can_ini + existestockanterior.can_ent - existestockanterior.can_sal;
                }
                else
                {
                    saldox = stockini.can_ini;
                }
            }
            var datos = (from a in context.vw_kardex_movimientos
                         where a.codigo == codigo && bodega.Contains(a.bodega)
                                                  && a.fec_creacion.Year >= anioDesde && a.fec_creacion.Month >= mesDesde
                                                  && a.fec_creacion.Year <= anioHasta && a.fec_creacion.Month <= mesHasta
                         select new
                         {
                             a.sw,
                             a.idencabezado,
                             a.id_movimiento_interno,
                             a.prefijo,
                             a.tpdoc_nombre,
                             a.fec_creacion,
                             a.numero,
                             a.cantentrada,
                             a.cantsalida,
                             a.costentrada,
                             a.costsalida,
                             a.CostoTotal,
                             a.prefijoext,
                             a.numeroext,
                             a.nombredocex,
                             bodega = (from bod in context.bodega_concesionario
                                       where a.bodega == bod.id
                                       select bod.bodccs_nombre).FirstOrDefault(),
                             documentoExterno = (from doc in context.encab_documento
                                                 where a.id_encabezado == doc.idencabezado
                                                 select new { doc.documento, doc.remision }).FirstOrDefault()
                         }).ToList();
            var data = datos.Select(c => new listaMovimientos
            {
                bodega=c.bodega,
               sw= c.sw,
                idencabezado= c.idencabezado,
                prefijo=c.prefijo,
                tpdoc_nombre=c.tpdoc_nombre,
                fecha = c.fec_creacion != null
                    ? c.fec_creacion.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                fec_creacion=c.fec_creacion,
                numero=c.numero,
                cantEntrada =
                    c.cantentrada != null ? c.cantentrada.Value.ToString("N0", new CultureInfo("is-IS")) : "0",
                cant_entrada=c.cantentrada!=null?c.cantentrada.Value:0,
                cantSalida = c.cantsalida != null ? c.cantsalida.Value.ToString("N0", new CultureInfo("is-IS")) : "0",
                cant_salida=c.cantsalida!=null?c.cantsalida.Value:0,
                costoEntrada = c.costentrada != null
                    ? c.costentrada.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "0",
                costo_entrada=c.costentrada,
                costoSalida = c.costsalida != null ? c.costsalida.Value.ToString("N0", new CultureInfo("is-IS")) : "0",
                costo_salida=c.costsalida,
                costoTotal = c.CostoTotal != null ? c.CostoTotal.Value.ToString("N0", new CultureInfo("is-IS")) : "0",
                costo_total=c.CostoTotal,
                documento_interno=c.id_movimiento_interno!=null?c.id_movimiento_interno.Value:0,
                documentoExterno = c.documentoExterno.documento != null
                    ? c.documentoExterno.documento != "" ? c.documentoExterno.documento :
                    c.documentoExterno.remision != null ? "(R) " + c.documentoExterno.remision : ""
                    : "",
                tpdoc_nombreope=c.nombredocex,
                numeroope=c.numeroext,
                prefijoope=c.prefijoext
            }).OrderBy(x => x.fecha).ToList();
            var inicio = 0;

            foreach (var item in data)
            {
                saldox = saldox + item.cant_entrada.Value - item.cant_salida.Value;
                item.saldo = saldox;
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarKardexVehiculos(int anio, int mes, int?[] bodega, string codigo)
        {
            System.Linq.Expressions.Expression<Func<vw_kardex, bool>> anioP = PredicateBuilder.True<vw_kardex>();

            System.Linq.Expressions.Expression<Func<vw_kardex, bool>> bodegaP = PredicateBuilder.False<vw_kardex>();
            System.Linq.Expressions.Expression<Func<vw_kardex, bool>> codigoP = PredicateBuilder.False<vw_kardex>();

            anioP = anioP.And(x => x.ano == anio);
            anioP = anioP.And(x => x.mes == mes);
            anioP = anioP.And(x => x.modulo == "V");

            #region Predicate bodega

            if (bodega.Count() > 0 && bodega[0] != null)
            {
                foreach (int? item in bodega)
                {
                    if (item != null)
                    {
                        bodegaP = bodegaP.Or(d => d.bodega == item);
                    }
                }

                anioP = anioP.And(bodegaP);
            }

            #endregion

            #region Predicate codigo

            if (!string.IsNullOrEmpty(codigo))
            {
                codigoP = codigoP.Or(d => d.codigo == codigo);
                anioP = anioP.And(codigoP);
            }

            #endregion

            System.Collections.Generic.List<vw_kardex> consulta = context.vw_kardex.Where(anioP).ToList();
            var consultaB = context.vw_kardex.Where(anioP).Select(x => new { x.bodega, x.bodccs_nombre }).Distinct()
                .ToList();

            if (consulta.Count > 0 && consultaB.Count > 0)
            {
                var data = new
                {
                    consulta,
                    consultaB
                };

                return Json(new { info = true, data }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { info = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult traerReferencias(string referencia /*cadena de la referencia a buscar*/)
        {
            var referencias = (from r in context.icb_referencia
                               where (r.ref_descripcion.Contains(referencia) || r.ref_codigo.Contains(referencia)) && r.modulo=="R"
                               select new
                               {
                                   referencia = r.ref_codigo + " | " + r.ref_descripcion
                               }).ToList();
            System.Collections.Generic.List<string> referencias_data = referencias.Select(d => d.referencia).ToList();
            
            return Json(referencias_data);
        }

        public JsonResult traerOperaciones(string operacion)
        {
            var operaciones = (from t in context.ttempario
                               where (t.operacion.Contains(operacion) || t.codigo.Contains(operacion))
                               select new
                               {
                                   operacion = t.codigo + " | " + t.operacion
                               }).ToList();
            System.Collections.Generic.List<string> operaciones_data = operaciones.Select(d => d.operacion).ToList();

            return Json(operaciones_data);
        }

        public ActionResult graficoCoordinador()
        {
            ViewBag.codigo = context.icb_referencia.Where(x => x.modulo == "R").ToList();
            ViewBag.horas = DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second;
            return View();
        }
    }
}