using Homer_MVC.IcebergModel;
using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class indicadoresAsesorController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: indicadoresAsesor
        public ActionResult Index(int? menu)
        {
            string[] meses =
            {
                "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre",
                "Noviembre", "Diciembre"
            };
            int asesor_logeado = Convert.ToInt32(Session["user_usuarioid"]);
            ViewBag.mes_actual = meses[DateTime.Now.Month];

            System.Collections.Generic.List<vpedido> cant_pedidos = context.vpedido.Where(x =>
                x.vendedor == asesor_logeado && x.fecha.Month == DateTime.Now.Month &&
                x.fecha.Year == DateTime.Now.Year && x.anulado == false).ToList();
            ViewBag.cant_pedidos = cant_pedidos.Count();
            ViewBag.datos_pedidos = cant_pedidos;

            //var vh_entregados = (from e in context.icb_vehiculo_eventos
            //                     join l in context.lineas_documento
            //                     on e.planmayor equals l.codigo
            //                     where e.id_tpevento == 14
            //                     && e.fechaevento.Month == DateTime.Now.Month
            //                     select new {
            //                         e.planmayor,
            //                         e.placa,
            //                         nombre =  e.icb_terceros.prinom_tercero
            //                     }).ToList();

            System.Collections.Generic.List<icb_vehiculo_eventos> vh_entregados = context.icb_vehiculo_eventos.Where(x =>
                x.fechaevento.Month == DateTime.Now.Month && x.fechaevento.Year == DateTime.Now.Year &&
                x.id_tpevento == 14).ToList();
            ViewBag.vh_entregados = vh_entregados.Count();
            ViewBag.datos_vh_entregados = vh_entregados;


            System.Collections.Generic.List<v_creditos> cred_aprobados = context.v_creditos.Where(x => x.fec_aprobacion.Value.Month == DateTime.Now.Month
                                                               && x.fec_aprobacion.Value.Year == DateTime.Now.Year
                                                               && x.asesor_id == asesor_logeado).ToList();
            ViewBag.cred_aprobados = cred_aprobados.Count();
            ViewBag.datos_cred_aprobados = cred_aprobados;


            System.Collections.Generic.List<v_creditos> comisiones = context.v_creditos.Where(c => c.fec_facturacomision.Value.Month == DateTime.Now.Month
                                                           && c.fec_facturacomision.Value.Year == DateTime.Now.Year
                                                           && c.asesor_id == asesor_logeado).ToList();
            ViewBag.comisiones_hoy = comisiones.Sum(x => x.valor_comision);
            ViewBag.datos_comisiones = comisiones;
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult Seguimiento(int? menu)
        {
            int asesor_logeado = Convert.ToInt32(Session["user_usuarioid"]);
            ViewBag.datos = context.v_creditos.Where(x => x.asesor_id == asesor_logeado).ToList();
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult GetVentasPorMes()
        {
            int asesor_logeado = Convert.ToInt32(Session["user_usuarioid"]);
            DateTimeFormatInfo formatoFecha = CultureInfo.CurrentCulture.DateTimeFormat;
            var datos = (from d in context.encab_documento
                         where d.vendedor == asesor_logeado
                         group d by new { d.fecha.Month }
                into grupo
                         select new
                         {
                             mes = grupo.Key.Month,
                             cantidad = grupo.Count()
                         }).ToList();

            string[] meses =
            {
                "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre",
                "Noviembre", "Diciembre"
            };
            int[] ventas = new int[12];
            int[] cotizaciones = new int[12];

            foreach (var item1 in datos)
            {
                ventas[item1.mes - 1] = item1.cantidad;
            }

            var datosCotizaciones = (from cotizacion in context.icb_cotizacion
                                     where cotizacion.asesor == asesor_logeado
                                     group cotizacion by new { cotizacion.cot_feccreacion.Month }
                into grupo
                                     select new
                                     {
                                         mes = grupo.Key.Month,
                                         cantidad = grupo.Count()
                                     }).ToList();

            foreach (var item1 in datosCotizaciones)
            {
                cotizaciones[item1.mes - 1] = item1.cantidad;
            }

            var datosPorMes = new
            {
                meses,
                ventas,
                cotizaciones
            };
            return Json(datosPorMes, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarCitas()
        {
            int asesor_logeado = Convert.ToInt32(Session["user_usuarioid"]);
            var citas = (from t in context.icb_terceros
                         join o in context.tp_origen
                             on t.origen_id equals o.tporigen_id
                         where t.asesor_id == asesor_logeado
                               && t.tercerofec_creacion.Month == DateTime.Now.Month
                               && t.tercerofec_creacion.Year == DateTime.Now.Year
                         group t by new { t.tp_origen }
                into grupo
                         select new
                         {
                             origen = grupo.Key.tp_origen.tporigen_nombre,
                             id = grupo.Key.tp_origen.tporigen_id,
                             cantidad = grupo.Count()
                         }).ToList();

            var total = (from t in context.icb_terceros
                         join o in context.tp_origen
                             on t.origen_id equals o.tporigen_id
                         where t.asesor_id == asesor_logeado
                               && t.tercerofec_creacion.Month == DateTime.Now.Month
                               && t.tercerofec_creacion.Year == DateTime.Now.Year
                         select new
                         {
                         }).ToList();

            var data = new
            {
                citas,
                total
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarCitasDetalle(int origen_id)
        {
            int asesor_logeado = Convert.ToInt32(Session["user_usuarioid"]);

            var data = (from t in context.icb_terceros
                        join o in context.tp_origen
                            on t.origen_id equals o.tporigen_id
                        join tt in context.icb_tptramite_prospecto
                            on t.tptramite equals tt.tptrapros_id
                        where t.asesor_id == asesor_logeado
                              && t.tercerofec_creacion.Month == DateTime.Now.Month
                              && t.tercerofec_creacion.Year == DateTime.Now.Year
                              && t.origen_id == origen_id
                        select new
                        {
                            cliente = t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero + " " +
                                      t.segapellido_tercero,
                            fecha = t.tercerofec_creacion.ToString(),
                            tramite = tt.tptrapros_descripcion
                        }).ToList();


            return Json(data, JsonRequestBehavior.AllowGet);
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