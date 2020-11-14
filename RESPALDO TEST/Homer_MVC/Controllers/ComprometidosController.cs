using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Homer_MVC.ViewModels.medios;
using System.Text.RegularExpressions;
using Homer_MVC.IcebergModel;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Data.Entity;
using Homer_MVC.Models;

namespace Homer_MVC.Controllers
{
    public class ComprometidosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        private readonly CultureInfo miCultura = new CultureInfo("is-IS");
        // GET: Comprometidos
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Create(int? menu)
        {

            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            //busco el rol del usuario
            int rol = Convert.ToInt32(Session["user_rolid"]);
            //busco la bodega del usuario
            int bodegaactual = Convert.ToInt32(Session["user_bodega"]);

            ViewBag.doccons_idtpdoc = context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();

            var documentos = (from doc in context.tiempos_comprometidos
                              select new
                              {
                                  doc.id_proceso,
                                  doc.documento,
                              }).ToList();

            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var item in documentos)
            {
                list.Add(new SelectListItem
                {
                    Text = item.documento,
                    Value = item.id_proceso.ToString()
                });
            }
            ViewBag.documento = list;


            var permisos = (from acceso in context.rolacceso
                            join rolPerm in context.rolpermisos
                            on acceso.idpermiso equals rolPerm.id
                            where acceso.idrol == rol //&& rolPerm.codigo == "P40" 
                            select new { rolPerm.codigo }).ToList();

            var resultado1 = permisos.Where(x => x.codigo == "P38").Count() > 0 ? "Si" : "No";
            var resultado2 = permisos.Where(x => x.codigo == "P41").Count() > 0 ? "Si" : "No";

            ViewBag.Permiso = resultado1;

            return View();
        }

        [HttpPost]
        public ActionResult Create()
        {
            //string documentosSeleccionados = Request["doccons_idtpdoc"];
            int? id = Convert.ToInt32(Request["documento"]);
            string tiempo = Request["txtTiempo"];

            int guardar = 0;

            if (string.IsNullOrWhiteSpace(tiempo))
            {
                TempData["mensaje_error"] = "Debe asignar tiempo!";
            }

            if (id == null)
            {
                TempData["mensaje_error"] = "Debe asignar minimo un documento!";
            }

            if (id != null && !string.IsNullOrWhiteSpace(tiempo))
            {

                //string[] documentosId = documentosSeleccionados.Split(',');

                var t = context.tiempos_comprometidos.Find(id);
                t.tiempo = Convert.ToInt32(tiempo);

                context.Entry(t).State = EntityState.Modified;
                guardar = context.SaveChanges();


                if (guardar > 0)
                {
                    TempData["mensaje"] = "El registro fue exitoso!";
                }
                else
                {
                    TempData["mensaje_error"] = "El registro fue exitoso!";
                }

            }


            return RedirectToAction("Create", "Comprometidos");
        }


        public ActionResult BrowserComprometidos()
        {

            return View();

        }


        public JsonResult buscarComprometidos()
        {

            var buscarTiempos = (from t in context.tiempos_comprometidos
                                 select new
                                 {
                                     t.id_proceso,
                                     t.cod_proceso,
                                     t.tiempo,
                                 }).ToList();
            List<ListaComprometidos> listacompromet  = new List<ListaComprometidos>();
            var buscarSeparacion = buscarTiempos.Where(d => d.cod_proceso == "TC1").FirstOrDefault();
            var buscarAnticipo = buscarTiempos.Where(d => d.cod_proceso == "TC2").FirstOrDefault();
            var buscarOT = buscarTiempos.Where(d => d.cod_proceso == "TC3").FirstOrDefault();
            var buscarTraslado = buscarTiempos.Where(d => d.cod_proceso == "TC4").FirstOrDefault();
            var buscarSolicitud = buscarTiempos.Where(d => d.cod_proceso == "TC5").FirstOrDefault();
            var buscarDespacho = buscarTiempos.Where(d => d.cod_proceso == "TC6").FirstOrDefault();
            var buscarRemision = buscarTiempos.Where(d => d.cod_proceso == "TC7").FirstOrDefault();

            var tiempoSeparacion = buscarSeparacion.tiempo;
            var tiempoAnticipo = buscarAnticipo.tiempo;
            var tiempoOT = buscarOT.tiempo;
            var tiempoTraslado = buscarTraslado.tiempo;
            var tiempoSolicitud = buscarSolicitud.tiempo;
            var tiempoDespacho = buscarDespacho.tiempo;
            var tiempoRemision = buscarRemision.tiempo;


            //Separacion
            var buscardatosSeparacion = (from s in context.vw_separacionComprometidos
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



            var datosSeparacion = buscardatosSeparacion.Where(d => d.diasComprometidos < tiempoSeparacion).Select(d => new
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


            var datosOT = buscardatosOT.Where(d => d.diasComprometidos < tiempoOT).Select(d => new
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
                                       where s.tipo == 1028
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





            var datosTraslado = buscardatosTraslado.Where(d => d.diasComprometidos < tiempoTraslado).GroupBy(d => new { d.idencabezado, d.codigo }).Select(d => new
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

            var datosSolicitud = buscardatosSolicitud.Where(d => d.diasComprometidos < tiempoSolicitud).Select(d => new
            {
                numero = d.numero,
                id = d.id,
                d.codigo,
                bodega = d.bodccs_nombre,
                d.cliente,
                fecha = d.fecha2,
                d.responsable,
                d.diasComprometidos,
                d.ref_descripcion

            }).ToList();

            foreach (var item in datosSolicitud)
                {
                ListaComprometidos listacom = new ListaComprometidos();
                listacom.Numero = item.numero;
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
                                       where s.tipo == 3074
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

            var datosDespacho = buscardatosDespacho.Where(d => d.diasComprometidos < tiempoDespacho).GroupBy(d => new { d.idencabezado, d.codigo }).Select(d => new
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
                                       where s.tipo == 3037
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

            var datosRemision = buscardatosRemision.Where(d => d.diasComprometidos < tiempoRemision).GroupBy(d => new { d.idencabezado, d.codigo }).Select(d => new
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


            //var data = new { datosSeparacion, datosOT, datosTraslado, datosSolicitud, datosDespacho, datosRemision };

            return Json(listacompromet, JsonRequestBehavior.AllowGet);

        }

        public JsonResult buscarDescomprometidos()
        {

            var buscarTiempos = (from t in context.tiempos_comprometidos
                                 select new
                                 {
                                     t.id_proceso,
                                     t.cod_proceso,
                                     t.tiempo,
                                 }).ToList();


            var buscarSeparacion = buscarTiempos.Where(d => d.cod_proceso == "TC1").FirstOrDefault();
            var buscarAnticipo = buscarTiempos.Where(d => d.cod_proceso == "TC2").FirstOrDefault();
            var buscarOT = buscarTiempos.Where(d => d.cod_proceso == "TC3").FirstOrDefault();
            var buscarTraslado = buscarTiempos.Where(d => d.cod_proceso == "TC4").FirstOrDefault();
            var buscarSolicitud = buscarTiempos.Where(d => d.cod_proceso == "TC5").FirstOrDefault();
            var buscarDespacho = buscarTiempos.Where(d => d.cod_proceso == "TC6").FirstOrDefault();
            var buscarRemision = buscarTiempos.Where(d => d.cod_proceso == "TC7").FirstOrDefault();

            var tiempoSeparacion = buscarSeparacion.tiempo;
            var tiempoAnticipo = buscarAnticipo.tiempo;
            var tiempoOT = buscarOT.tiempo;
            var tiempoTraslado = buscarTraslado.tiempo;
            var tiempoSolicitud = buscarSolicitud.tiempo;
            var tiempoDespacho = buscarDespacho.tiempo;
            var tiempoRemision = buscarRemision.tiempo;

            double db_tiempoSeparacion = Convert.ToDouble(buscarSeparacion.tiempo);
            double db_tiempoAnticipo = Convert.ToDouble(buscarAnticipo.tiempo);
            double db_tiempoOT = Convert.ToDouble(buscarOT.tiempo);
            double db_tiempoTraslado = Convert.ToDouble(buscarTraslado.tiempo);
            double db_tiempoSolicitud = Convert.ToDouble(buscarSolicitud.tiempo);
            double db_tiempoDespacho = Convert.ToDouble(buscarDespacho.tiempo);
            double db_tiempoRemision = Convert.ToDouble(buscarRemision.tiempo);


            //Separacion

            var buscardatosSeparacion = (from s in context.vw_separacionComprometidos
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

                                         }).ToList();

            var datosSeparacion = buscardatosSeparacion.Where(d => d.diasComprometidos > tiempoSeparacion).Select(d => new
            {

                numero = d.id,
                d.codigo,
                bodega = d.bodccs_nombre,
                d.cliente,
                fecha = d.fecha.AddDays(db_tiempoSeparacion).ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")),
                d.responsable,
                d.diasComprometidos,

            }).ToList();

            //OT
            var buscardatosOT = (from s in context.vw_otComprometidos
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

                                 }).ToList();


            var datosOT = buscardatosOT.Where(d => d.diasComprometidos > tiempoOT).Select(d => new
            {

                numero = d.id,
                d.codigo,
                bodega = d.bodccs_nombre,
                d.cliente,
                fecha = Convert.ToDateTime(d.fecha).AddDays(db_tiempoSeparacion).ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")),
                d.responsable,
                d.diasComprometidos,

            }).ToList();

            //Traslado

            var buscardatosTraslado = (from s in context.vw_documentoComprometidos
                                       where s.tipo == 1028
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

                                       }).ToList();

            var datosTraslado = buscardatosTraslado.Where(d => d.diasComprometidos > tiempoTraslado).GroupBy(d => new { d.idencabezado, d.codigo }).Select(d => new
            {

                numero = d.Select(e => e.idencabezado).FirstOrDefault(),
                codigo = d.Select(e => e.codigo).FirstOrDefault(),
                bodega = d.Select(e => e.bodccs_nombre).FirstOrDefault(),
                cliente = d.Select(e => e.cliente).FirstOrDefault(),
                fecha = Convert.ToDateTime(d.Select(e => e.fecha).FirstOrDefault()).AddDays(db_tiempoSolicitud).ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")),
                responsable = d.Select(e => e.responsable).FirstOrDefault(),
                diasComprometidos = d.Select(e => e.diasComprometidos).FirstOrDefault(),

            }).ToList();

            //Solicitud de despacho

            var buscardatosSolicitud = (from s in context.vw_solicitudComprometidos
                                        select new
                                        {
                                            s.id,
                                            s.codigo,
                                            s.bodccs_nombre,
                                            s.cliente,                                           
                                            fecha = s.fecha2,
                                            s.responsable,
                                            s.diasComprometidos,

                                        }).ToList();


            var datosSolicitud = buscardatosSolicitud.Where(d => d.diasComprometidos > tiempoSolicitud).Select(d => new
            {

                numero = d.id,
                d.codigo,
                bodega = d.bodccs_nombre,
                d.cliente,
                fecha = Convert.ToDateTime(d.fecha).AddDays(db_tiempoSolicitud).ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")),
                d.responsable,
                d.diasComprometidos,

            }).ToList();


            //Despacho

            var buscardatosDespacho = (from s in context.vw_documentoComprometidos
                                       where s.tipo == 3074
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

                                       }).ToList();

            var datosDespacho = buscardatosDespacho.Where(d => d.diasComprometidos > tiempoDespacho).GroupBy(d => new { d.idencabezado, d.codigo }).Select(d => new
            {

                numero = d.Select(e => e.idencabezado).FirstOrDefault(),
                codigo = d.Select(e => e.codigo).FirstOrDefault(),
                bodega = d.Select(e => e.bodccs_nombre).FirstOrDefault(),
                cliente = d.Select(e => e.cliente).FirstOrDefault(),
                fecha = Convert.ToDateTime(d.Select(e => e.fecha).FirstOrDefault()).AddDays(db_tiempoDespacho).ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")),
                responsable = d.Select(e => e.responsable).FirstOrDefault(),
                diasComprometidos = d.Select(e => e.diasComprometidos).FirstOrDefault(),

            }).ToList();


            //Remision

            var buscardatosRemision = (from s in context.vw_documentoComprometidos
                                       where s.tipo == 3037
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

                                       }).ToList();

            var datosRemision = buscardatosRemision.Where(d => d.diasComprometidos > tiempoRemision).GroupBy(d => new { d.idencabezado, d.codigo }).Select(d => new
            {

                numero = d.Select(e => e.idencabezado).FirstOrDefault(),
                codigo = d.Select(e => e.codigo).FirstOrDefault(),
                bodega = d.Select(e => e.bodccs_nombre).FirstOrDefault(),
                cliente = d.Select(e => e.cliente).FirstOrDefault(),
                fecha = Convert.ToDateTime(d.Select(e => e.fecha).FirstOrDefault()).AddDays(db_tiempoRemision).ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")),
                responsable = d.Select(e => e.responsable).FirstOrDefault(),
                diasComprometidos = d.Select(e => e.diasComprometidos).FirstOrDefault(),

            }).ToList();

            var data = new { datosSeparacion, datosOT, datosTraslado, datosSolicitud, datosDespacho, datosRemision };


            return Json(data, JsonRequestBehavior.AllowGet);

        }

        public JsonResult buscarTiempoIncumplido()
        {

            return Json(0, JsonRequestBehavior.AllowGet);

        }

        public JsonResult buscarTiempos()
        {



            //ComprometidoSeperacion();
            //ComprometidoAnticipo();
            //ComprometidoOT();
            //ComprometidoTraslado();
            //ComprometidoSolicitudDespacho();
            //ComprometidoDespachoTaller();
            //ComprometidoRemision();

            var buscar = (from tiempo in context.tiempos_comprometidos
                          select new
                          {
                              tiempo.id_proceso,
                              tiempo.cod_proceso,
                              tiempo.documento,
                              tiempo.tiempo,

                          }).ToList();


            var data = buscar.Select(d => new
            {

                id = d.id_proceso,
                codigo = d.cod_proceso,
                documento = d.documento,
                tiempo = d.tiempo,

            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);

        }


        public void ComprometidoSeperacion()
        {


            var buscar = (from tiempo in context.tiempos_comprometidos
                          where tiempo.cod_proceso == "TC1"
                          select new
                          {
                              tiempo.id_proceso,
                              tiempo.tiempo,
                          }).FirstOrDefault();


            var buscar2 = (from separacion in context.rseparacionmercancia
                           select new
                           {
                               separacion.id,
                               separacion.codigo,
                               separacion.diasComprometidos,
                               separacion.comprometido,

                           }).ToList();


            foreach (var item in buscar2)
            {

                var id = item.id;
                var tiempo = buscar.tiempo;
                var dias = item.diasComprometidos;


                if (Convert.ToInt32(dias) > Convert.ToInt32(tiempo))
                {

                    var l = context.rseparacionmercancia.Find(id);

                    l.comprometido = false;
                    context.Entry(l).State = EntityState.Modified;
                    context.SaveChanges();

                }
                else if (Convert.ToInt32(dias) < Convert.ToInt32(tiempo))
                {
                    var l = context.rseparacionmercancia.Find(id);

                    l.comprometido = true;
                    context.Entry(l).State = EntityState.Modified;
                    context.SaveChanges();
                }


            }

        }

        public void ComprometidoAnticipo()
        {

            var buscar = (from tiempo in context.tiempos_comprometidos
                          where tiempo.cod_proceso == "TC2"
                          select new
                          {
                              tiempo.id_proceso,
                              tiempo.tiempo,
                          }).FirstOrDefault();


            var buscar2 = (from separacion in context.rseparacionmercancia
                           select new
                           {
                               separacion.id,
                               separacion.codigo,
                               separacion.diasComprometidos,
                               separacion.comprometido,

                           }).ToList();


            foreach (var item in buscar2)
            {

                var id = item.id;
                var tiempo = buscar.tiempo;
                var dias = item.diasComprometidos;


                if (Convert.ToInt32(dias) > Convert.ToInt32(tiempo))
                {

                    var l = context.rseparacionmercancia.Find(id);

                    l.comprometido = false;
                    context.Entry(l).State = EntityState.Modified;
                    context.SaveChanges();

                }
                else if (Convert.ToInt32(dias) < Convert.ToInt32(tiempo))
                {
                    var l = context.rseparacionmercancia.Find(id);

                    l.comprometido = true;
                    context.Entry(l).State = EntityState.Modified;
                    context.SaveChanges();
                }


            }

        }

        public void ComprometidoOT()
        {

            var buscar = (from tiempo in context.tiempos_comprometidos
                          where tiempo.cod_proceso == "TC3"
                          select new
                          {
                              tiempo.id_proceso,
                              tiempo.tiempo,
                          }).FirstOrDefault();


            var buscar2 = (from repuestosot in context.tdetallerepuestosot
                           select new
                           {
                               repuestosot.id,
                               repuestosot.idrepuesto,
                               repuestosot.diasComprometidos,
                               repuestosot.comprometido,

                           }).ToList();


            foreach (var item in buscar2)
            {

                var id = item.id;
                var tiempo = buscar.tiempo;
                var dias = item.diasComprometidos;


                if (Convert.ToInt32(dias) > Convert.ToInt32(tiempo))
                {

                    var l = context.tdetallerepuestosot.Find(id);

                    l.comprometido = false;
                    context.Entry(l).State = EntityState.Modified;
                    context.SaveChanges();

                }
                else if (Convert.ToInt32(dias) < Convert.ToInt32(tiempo))
                {
                    var l = context.tdetallerepuestosot.Find(id);

                    l.comprometido = true;
                    context.Entry(l).State = EntityState.Modified;
                    context.SaveChanges();
                }


            }

        }

        public void ComprometidoTraslado()//crear vista 1028
        {

            var buscar = (from tiempo in context.tiempos_comprometidos
                          where tiempo.cod_proceso == "TC4"
                          select new
                          {
                              tiempo.id_proceso,
                              tiempo.tiempo,
                          }).FirstOrDefault();


            var buscar2 = (from lineas in context.vw_lineasComprometidas
                           where lineas.tipo == 1028
                           select new
                           {
                               lineas.idencabezado,
                               lineas.codigo,
                               lineas.diasComprometidos,
                               lineas.comprometido,

                           }).ToList();


            foreach (var item in buscar2)
            {

                var id = item.idencabezado;
                var codigo = item.codigo;
                var tiempo = buscar.tiempo;
                var dias = item.diasComprometidos;


                if (Convert.ToInt32(dias) > Convert.ToInt32(tiempo))
                {

                    var lineas = context.lineas_documento.Where(d => d.id_encabezado == id && d.codigo == codigo).ToList();

                    foreach (var item2 in lineas)
                    {
                        item2.comprometido = false;
                        context.Entry(item2).State = EntityState.Modified;
                        context.SaveChanges();
                    }

                }
                else if (Convert.ToInt32(dias) < Convert.ToInt32(tiempo))
                {
                    var lineas = context.lineas_documento.Where(d => d.id_encabezado == id && d.codigo == codigo).ToList();

                    foreach (var item2 in lineas)
                    {
                        item2.comprometido = true;
                        context.Entry(item2).State = EntityState.Modified;
                        context.SaveChanges();
                    }


                }

            }

        }

        public void ComprometidoSolicitudDespacho()
        {

            var buscar = (from tiempo in context.tiempos_comprometidos
                          where tiempo.cod_proceso == "TC5"
                          select new
                          {
                              tiempo.id_proceso,
                              tiempo.tiempo,
                          }).FirstOrDefault();


            var buscar2 = (from repuestosot in context.tsolicitudrepuestosot
                           select new
                           {
                               repuestosot.id,
                               repuestosot.idrepuesto,
                               repuestosot.diasComprometidos,
                               repuestosot.comprometido,

                           }).ToList();


            foreach (var item in buscar2)
            {

                var id = item.id;
                var tiempo = buscar.tiempo;
                var dias = item.diasComprometidos;


                if (Convert.ToInt32(dias) > Convert.ToInt32(tiempo))
                {

                    var l = context.tsolicitudrepuestosot.Find(id);

                    l.comprometido = false;
                    context.Entry(l).State = EntityState.Modified;
                    context.SaveChanges();

                }
                else if (Convert.ToInt32(dias) < Convert.ToInt32(tiempo))
                {
                    var l = context.tsolicitudrepuestosot.Find(id);

                    l.comprometido = true;
                    context.Entry(l).State = EntityState.Modified;
                    context.SaveChanges();
                }


            }

        }

        public void ComprometidoDespachoTaller() //crear vista 3074
        {

            var buscar = (from tiempo in context.tiempos_comprometidos
                          where tiempo.cod_proceso == "TC6"
                          select new
                          {
                              tiempo.id_proceso,
                              tiempo.tiempo,

                          }).FirstOrDefault();


            var buscar2 = (from lineas in context.vw_lineasComprometidas
                           where lineas.tipo == 3074
                           select new
                           {
                               lineas.idencabezado,
                               lineas.codigo,
                               lineas.diasComprometidos,
                               lineas.comprometido,

                           }).ToList();


            foreach (var item in buscar2)
            {

                var id = item.idencabezado;
                var codigo = item.codigo;
                var tiempo = buscar.tiempo;
                var dias = item.diasComprometidos;


                if (Convert.ToInt32(dias) > Convert.ToInt32(tiempo))
                {
                    var lineas = context.lineas_documento.Where(d => d.id_encabezado == id && d.codigo == codigo).ToList();

                    foreach (var item2 in lineas)
                    {
                        item2.comprometido = false;
                        context.Entry(item2).State = EntityState.Modified;
                        context.SaveChanges();
                    }


                }
                else if (Convert.ToInt32(dias) < Convert.ToInt32(tiempo))
                {
                    var lineas = context.lineas_documento.Where(d => d.id_encabezado == id && d.codigo == codigo).ToList();

                    foreach (var item2 in lineas)
                    {
                        item2.comprometido = true;
                        context.Entry(item2).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                }

            }



        }

        public void ComprometidoRemision()//crear vista 3037
        {

            var buscar = (from tiempo in context.tiempos_comprometidos
                          where tiempo.cod_proceso == "TC7"
                          select new
                          {
                              tiempo.id_proceso,
                              tiempo.tiempo,
                          }).FirstOrDefault();


            var buscar2 = (from lineas in context.vw_lineasComprometidas
                           where lineas.tipo == 3037
                           select new
                           {
                               lineas.idencabezado,
                               lineas.codigo,
                               lineas.diasComprometidos,
                               lineas.comprometido,

                           }).ToList();


            foreach (var item in buscar2)
            {

                var id = item.idencabezado;
                var codigo = item.codigo;
                var tiempo = buscar.tiempo;
                var dias = item.diasComprometidos;


                if (Convert.ToInt32(dias) > Convert.ToInt32(tiempo))
                {
                    var lineas = context.lineas_documento.Where(d => d.id_encabezado == id && d.codigo == codigo).ToList();

                    foreach (var item2 in lineas)
                    {
                        item2.comprometido = false;
                        context.Entry(item2).State = EntityState.Modified;
                        context.SaveChanges();
                    }


                }
                else if (Convert.ToInt32(dias) < Convert.ToInt32(tiempo))
                {
                    var lineas = context.lineas_documento.Where(d => d.id_encabezado == id && d.codigo == codigo).ToList();

                    foreach (var item2 in lineas)
                    {
                        item2.comprometido = true;
                        context.Entry(item2).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                }

            }

        }

    }
}