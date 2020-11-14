using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class consultaEventosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: consultaEventos
        public ActionResult Index(int? menu)
        {
            ViewBag.icb_tpeventos = context.icb_tpeventos.ToList();
            ViewBag.ubicacion_vehiculo = new SelectList(context.ubicacion_vehiculo, "ubivh_id", "ubivh_nombre");
            BuscarFavoritos(menu);
            return View();
        }


        public JsonResult BuscarEventos(int? tpEvento, DateTime? desde, DateTime? hasta, string nit, string placa)
        {
            List<int> listaEventos = new List<int>();
            List<int> listaUbicaciones = new List<int>();
            if (tpEvento == null)
            {
                listaEventos = context.icb_tpeventos.Select(x => x.tpevento_id).ToList();
            }
            else
            {
                listaEventos.Add(tpEvento ?? 0);
            }

            if (desde == null)
            {
                desde = context.icb_vehiculo_eventos.OrderBy(x => x.fechaevento).Select(x => x.fechaevento)
                    .FirstOrDefault();
            }

            if (hasta == null)
            {
                hasta = context.icb_vehiculo_eventos.OrderByDescending(x => x.fechaevento).Select(x => x.fechaevento)
                    .FirstOrDefault();
            }
            else
            {
                hasta = hasta.GetValueOrDefault().AddDays(1);
            }

            if (string.IsNullOrEmpty(nit) && string.IsNullOrEmpty(placa))
            {
                var buscarEventos1 = (from eventos in context.icb_vehiculo_eventos
                                      join tipoEvento in context.icb_tpeventos
                                          on eventos.id_tpevento equals tipoEvento.tpevento_id
                                      join vehiculo in context.icb_vehiculo
                                          on eventos.planmayor equals vehiculo.plan_mayor
                                      join modelo in context.modelo_vehiculo
                                          on vehiculo.modvh_id equals modelo.modvh_codigo
                                      join bodega in context.bodega_concesionario
                                          on eventos.bodega_id equals bodega.id
                                      join ubicacionVh in context.ubicacion_vehiculo
                                          on eventos.ubicacion equals ubicacionVh.ubivh_id into ps
                                      from ubicacionVh in ps.DefaultIfEmpty()
                                      join tercero in context.icb_terceros
                                          on eventos.terceroid equals tercero.tercero_id into ter
                                      from tercero in ter.DefaultIfEmpty()
                                      where listaEventos.Contains(eventos.id_tpevento)
                                            && eventos.fechaevento >= desde
                                            && eventos.fechaevento <= hasta
                                      select new
                                      {
                                          eventos.evento_nombre,
                                          tipoEvento.tpevento_nombre,
                                          vehiculo.plan_mayor,
                                          vehiculo.vin,
                                          modelo.modvh_nombre,
                                          vehiculo.anio_vh,
                                          eventos.fechaevento,
                                          eventos.evento_observacion,
                                          vehiculo.plac_vh,
                                          bodega.bodccs_cod,
                                          bodega.bodccs_nombre,
                                          nit = tercero.doc_tercero,
                                          cliente = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                    tercero.apellido_tercero + " " + tercero.segapellido_tercero + tercero.razon_social,
                                          ubicacionVh.ubivh_nombre
                                      }).ToList();

                var eventos1 = buscarEventos1.Select(x => new
                {
                    x.evento_nombre,
                    x.tpevento_nombre,
                    x.plan_mayor,
                    x.vin,
                    x.modvh_nombre,
                    fechaevento = x.fechaevento.ToShortDateString(),
                    evento_observacion = x.evento_observacion != null ? x.evento_observacion : "",
                    plac_vh = x.plac_vh != null ? x.plac_vh : "",
                    x.bodccs_cod,
                    x.bodccs_nombre,
                    x.anio_vh,
                    ubivh_nombre = x.ubivh_nombre != null ? x.ubivh_nombre : "",
                    cliente = x.cliente.Trim()
                });

                return Json(eventos1, JsonRequestBehavior.AllowGet);
            }

            if (!string.IsNullOrEmpty(nit) && string.IsNullOrEmpty(placa))
            {
                var buscarEventos2 = (from eventos in context.icb_vehiculo_eventos
                                      join tipoEvento in context.icb_tpeventos
                                          on eventos.id_tpevento equals tipoEvento.tpevento_id
                                      join vehiculo in context.icb_vehiculo
                                          on eventos.planmayor equals vehiculo.plan_mayor
                                      join modelo in context.modelo_vehiculo
                                          on vehiculo.modvh_id equals modelo.modvh_codigo
                                      join bodega in context.bodega_concesionario
                                          on eventos.bodega_id equals bodega.id
                                      join ubicacionVh in context.ubicacion_vehiculo
                                          on eventos.ubicacion equals ubicacionVh.ubivh_id into ps
                                      from ubicacionVh in ps.DefaultIfEmpty()
                                      join tercero in context.icb_terceros
                                          on eventos.terceroid equals tercero.tercero_id into ter
                                      from tercero in ter.DefaultIfEmpty()
                                      where listaEventos.Contains(eventos.id_tpevento)
                                            && eventos.fechaevento >= desde
                                            && eventos.fechaevento <= hasta
                                            && tercero.doc_tercero.Contains(nit)
                                      //&& eventos.nit.Contains(nit)
                                      select new
                                      {
                                          eventos.evento_nombre,
                                          tipoEvento.tpevento_nombre,
                                          vehiculo.plan_mayor,
                                          vehiculo.vin,
                                          modelo.modvh_nombre,
                                          vehiculo.anio_vh,
                                          eventos.fechaevento,
                                          eventos.evento_observacion,
                                          vehiculo.plac_vh,
                                          bodega.bodccs_cod,
                                          bodega.bodccs_nombre,
                                          nit = tercero.doc_tercero,
                                          cliente = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                    tercero.apellido_tercero + " " + tercero.segapellido_tercero + tercero.razon_social,
                                          //eventos.nit,
                                          ubicacionVh.ubivh_nombre
                                      }).ToList();
                var eventos2 = buscarEventos2.Select(x => new
                {
                    x.evento_nombre,
                    x.tpevento_nombre,
                    x.plan_mayor,
                    x.vin,
                    x.modvh_nombre,
                    fechaevento = x.fechaevento.ToShortDateString(),
                    evento_observacion = x.evento_observacion != null ? x.evento_observacion : "",
                    plac_vh = x.plac_vh != null ? x.plac_vh : "",
                    x.bodccs_cod,
                    x.bodccs_nombre,
                    x.anio_vh,
                    ubivh_nombre = x.ubivh_nombre != null ? x.ubivh_nombre : "",
                    cliente = x.cliente.Trim()
                });
                return Json(eventos2, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrEmpty(nit) && !string.IsNullOrEmpty(placa))
            {
                var buscarEventos3 = (from eventos in context.icb_vehiculo_eventos
                                      join tipoEvento in context.icb_tpeventos
                                          on eventos.id_tpevento equals tipoEvento.tpevento_id
                                      join vehiculo in context.icb_vehiculo
                                          on eventos.planmayor equals vehiculo.plan_mayor
                                      join modelo in context.modelo_vehiculo
                                          on vehiculo.modvh_id equals modelo.modvh_codigo
                                      join bodega in context.bodega_concesionario
                                          on eventos.bodega_id equals bodega.id
                                      join ubicacionVh in context.ubicacion_vehiculo
                                          on eventos.ubicacion equals ubicacionVh.ubivh_id into ps
                                      from ubicacionVh in ps.DefaultIfEmpty()
                                      join tercero in context.icb_terceros
                                          on eventos.terceroid equals tercero.tercero_id into ter
                                      from tercero in ter.DefaultIfEmpty()
                                      where listaEventos.Contains(eventos.id_tpevento)
                                            && eventos.fechaevento >= desde
                                            && eventos.fechaevento <= hasta
                                            && vehiculo.plac_vh.Contains(placa)
                                      select new
                                      {
                                          eventos.evento_nombre,
                                          tipoEvento.tpevento_nombre,
                                          vehiculo.plan_mayor,
                                          vehiculo.vin,
                                          modelo.modvh_nombre,
                                          vehiculo.anio_vh,
                                          eventos.fechaevento,
                                          eventos.evento_observacion,
                                          vehiculo.plac_vh,
                                          bodega.bodccs_cod,
                                          bodega.bodccs_nombre,
                                          nit = tercero.doc_tercero,
                                          cliente = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                    tercero.apellido_tercero + " " + tercero.segapellido_tercero + tercero.razon_social,
                                          //eventos.nit,
                                          ubicacionVh.ubivh_nombre
                                      }).ToList();
                var eventos3 = buscarEventos3.Select(x => new
                {
                    x.evento_nombre,
                    x.tpevento_nombre,
                    x.plan_mayor,
                    x.vin,
                    x.modvh_nombre,
                    fechaevento = x.fechaevento.ToShortDateString(),
                    evento_observacion = x.evento_observacion != null ? x.evento_observacion : "",
                    plac_vh = x.plac_vh != null ? x.plac_vh : "",
                    x.bodccs_cod,
                    x.bodccs_nombre,
                    x.anio_vh,
                    ubivh_nombre = x.ubivh_nombre != null ? x.ubivh_nombre : "",
                    cliente = x.cliente.Trim()
                });
                return Json(eventos3, JsonRequestBehavior.AllowGet);
            }

            {
                var buscarEventos4 = (from eventos in context.icb_vehiculo_eventos
                                      join tipoEvento in context.icb_tpeventos
                                          on eventos.id_tpevento equals tipoEvento.tpevento_id
                                      join vehiculo in context.icb_vehiculo
                                          on eventos.planmayor equals vehiculo.plan_mayor
                                      join modelo in context.modelo_vehiculo
                                          on vehiculo.modvh_id equals modelo.modvh_codigo
                                      join bodega in context.bodega_concesionario
                                          on eventos.bodega_id equals bodega.id
                                      join ubicacionVh in context.ubicacion_vehiculo
                                          on eventos.ubicacion equals ubicacionVh.ubivh_id into ps
                                      from ubicacionVh in ps.DefaultIfEmpty()
                                      join tercero in context.icb_terceros
                                          on eventos.terceroid equals tercero.tercero_id into ter
                                      from tercero in ter.DefaultIfEmpty()
                                      where listaEventos.Contains(eventos.id_tpevento)
                                            && eventos.fechaevento >= desde
                                            && eventos.fechaevento <= hasta
                                            && tercero.doc_tercero.Contains(nit)
                                            //&& eventos.nit.Contains(nit)
                                            && vehiculo.plac_vh.Contains(placa)
                                      select new
                                      {
                                          eventos.evento_nombre,
                                          tipoEvento.tpevento_nombre,
                                          vehiculo.plan_mayor,
                                          vehiculo.vin,
                                          modelo.modvh_nombre,
                                          vehiculo.anio_vh,
                                          eventos.fechaevento,
                                          eventos.evento_observacion,
                                          vehiculo.plac_vh,
                                          bodega.bodccs_cod,
                                          bodega.bodccs_nombre,
                                          nit = tercero.doc_tercero,
                                          cliente = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                    tercero.apellido_tercero + " " + tercero.segapellido_tercero + tercero.razon_social,
                                          //eventos.nit,
                                          ubicacionVh.ubivh_nombre
                                      }).ToList();
                var eventos4 = buscarEventos4.Select(x => new
                {
                    x.evento_nombre,
                    x.tpevento_nombre,
                    x.plan_mayor,
                    x.vin,
                    x.modvh_nombre,
                    fechaevento = x.fechaevento.ToShortDateString(),
                    evento_observacion = x.evento_observacion != null ? x.evento_observacion : "",
                    plac_vh = x.plac_vh != null ? x.plac_vh : "",
                    x.bodccs_cod,
                    x.bodccs_nombre,
                    x.anio_vh,
                    ubivh_nombre = x.ubivh_nombre != null ? x.ubivh_nombre : "",
                    cliente = x.cliente.Trim()
                });
                return Json(eventos4, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult BrowserDelima(int? menu)
        {
            ViewBag.datos = context.vw_cargadelima.ToList();
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult BuscarDatosDelima(string fechaDesde, string fechaHasta)
        {
            if (!string.IsNullOrEmpty(fechaDesde) && !string.IsNullOrEmpty(fechaHasta))
            {
                DateTime desde = Convert.ToDateTime(fechaDesde);
                DateTime hasta = Convert.ToDateTime(fechaHasta);

                var datos = (from d in context.vw_cargadelima
                             where d.fechaevento >= desde
                                   && d.fechaevento <= hasta
                             select new
                             {
                                 d.vin,
                                 d.vrtotal,
                                 d.propietario,
                                 d.plac_vh,
                                 d.numfactura,
                                 d.nombrecliente,
                                 d.fecha_factura_css,
                                 d.fechaevento
                             }).ToList();

                return Json(datos, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var datos = (from d in context.vw_cargadelima
                             select new
                             {
                                 vin = d.vin != null ? d.vin : "",
                                 vrtotal = d.vrtotal != null ? d.vrtotal.ToString() : "",
                                 propietario = d.propietario != null ? d.propietario.ToString() : "",
                                 plac_vh = d.plac_vh != null ? d.plac_vh : "",
                                 numfactura = d.numfactura != null ? d.numfactura.ToString() : "",
                                 nombrecliente = d.nombrecliente != null ? d.nombrecliente : "",
                                 fecha_factura_css = d.fecha_factura_css != null ? d.fecha_factura_css.ToString() : "",
                                 fechaevento = d.fechaevento != null ? d.fechaevento.ToString() : ""
                             }).ToList();

                return Json(datos, JsonRequestBehavior.AllowGet);
            }
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