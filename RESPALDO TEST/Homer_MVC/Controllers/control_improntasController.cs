using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class control_improntasController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        private void parametrosBusqueda()
        {
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 126);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
        }

        // GET: control_improntas
        public ActionResult envio_improntas(int? menu)
        {
            parametrosBusqueda();
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult envio_improntas(IngresoVhModel modelo, int? menu)
        {
            string listVin = Request["listVin"];
            string[] exploded = listVin.Split('|');

            foreach (string item in exploded)
            {
                if (item != "")
                {
                    int id = Convert.ToInt32(item);
                    icb_vehiculo buscaVin = context.icb_vehiculo.FirstOrDefault(x => x.icbvh_id == id);
                    if (buscaVin != null)
                    {
                        buscaVin.icbvh_fec_envioimpronta = DateTime.Now;
                        context.Entry(buscaVin).State = EntityState.Modified;
                    }

                    bool result = context.SaveChanges() > 0;
                    if (result)
                    {
                        TempData["mensaje"] = "El envio de impronta fue exitoso!";

                        icb_sysparameter buscarParametroTpEvento =
                            context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P15");
                        string tpEventoParametro =
                            buscarParametroTpEvento != null ? buscarParametroTpEvento.syspar_value : "5";

                        context.icb_vehiculo_eventos.Add(new icb_vehiculo_eventos
                        {
                            eventofec_creacion = DateTime.Now,
                            eventouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            evento_nombre = "Envio Impronta",
                            evento_estado = true,
                            evento_observacion = "Se realiza envio de impronta",
                            //id_vehiculo = id,
                            vin = buscaVin.vin,
                            bodega_id = Convert.ToInt32(buscaVin.icbvh_bodpro),
                            id_tpevento = Convert.ToInt32(tpEventoParametro)
                        });
                        context.SaveChanges();
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error de conexion, intente mas tarde!";
                    }
                }
            }

            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult BuscarImprontasPaginados()
        {
            int idsuario = Convert.ToInt32(Session["user_usuarioid"]);
            var query = (from vehiculo in context.icb_vehiculo
                         join bodega in context.bodega_concesionario
                             on vehiculo.id_bod equals bodega.id
                         join usuario in context.users
                             on idsuario equals usuario.user_id
                         join bodUsuario in context.bodega_usuario
                             on usuario.user_id equals bodUsuario.id_usuario
                         join modelo in context.modelo_vehiculo
                             on vehiculo.modvh_id equals modelo.modvh_codigo
                         join tpEventos in context.icb_tpeventos
                             on vehiculo.id_evento equals tpEventos.tpevento_id
                         where vehiculo.icbvh_estatus == "6"
                               && vehiculo.icbvh_fec_impronta != null
                               && vehiculo.icbvh_fec_envioimpronta == null
                               && bodUsuario.id_bodega == vehiculo.id_bod
                         select new
                         {
                             vehiculo.icbvh_id,
                             vehiculo.vin,
                             modelo.modvh_nombre,
                             vehiculo.icbvh_estatus,
                             vehiculo.icbvh_fecinsp_ingreso,
                             vehiculo.icbvh_fec_impronta
                         }).ToList();

            var data = query.Select(x => new
            {
                x.icbvh_id,
                x.vin,
                x.modvh_nombre,
                x.icbvh_estatus,
                icbvh_fecinsp_ingreso = x.icbvh_fecinsp_ingreso != null
                    ? x.icbvh_fecinsp_ingreso.Value.ToShortDateString() + " " +
                      x.icbvh_fecinsp_ingreso.Value.ToShortTimeString()
                    : "",
                icbvh_fec_impronta = x.icbvh_fec_impronta != null
                    ? x.icbvh_fec_impronta.Value.ToShortDateString() + " " +
                      x.icbvh_fec_impronta.Value.ToShortTimeString()
                    : ""
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarImprontasPaginadosSeguimiento()
        {
            int idsuario = Convert.ToInt32(Session["user_usuarioid"]);
            var query = (from vehiculo in context.icb_vehiculo
                         join bodega in context.bodega_concesionario
                             on vehiculo.id_bod equals bodega.id
                         join usuario in context.users
                             on idsuario equals usuario.user_id
                         join bodUsuario in context.bodega_usuario
                             on usuario.user_id equals bodUsuario.id_usuario
                         join modelo in context.modelo_vehiculo
                             on vehiculo.modvh_id equals modelo.modvh_codigo
                         join tpEvento in context.icb_tpeventos
                             on vehiculo.id_evento equals tpEvento.tpevento_id
                         where vehiculo.icbvh_estatus == "6"
                               && vehiculo.icbvh_fec_impronta != null
                               && vehiculo.icbvh_fec_envioimpronta != null
                               && bodUsuario.id_bodega == vehiculo.id_bod
                         select new
                         {
                             vehiculo.icbvh_id,
                             vehiculo.vin,
                             modelo.modvh_nombre,
                             bodega.bodccs_nombre,
                             vehiculo.icbvh_estatus,
                             vehiculo.icbvh_fecinsp_ingreso,
                             vehiculo.icbvh_fec_impronta,
                             vehiculo.icbvh_fec_envioimpronta,
                             vehiculo.icbvh_fec_recepcionimpronta,
                             tiempo_tomaImpronta = SqlFunctions.DateDiff("hour", vehiculo.icbvh_fecinsp_ingreso,
                                 vehiculo.icbvh_fec_impronta),
                             tiempo_envioImpronta = SqlFunctions.DateDiff("hour", vehiculo.icbvh_fec_impronta,
                                 vehiculo.icbvh_fec_envioimpronta),
                             tiempo_recepcionImpronta = SqlFunctions.DateDiff("hour", vehiculo.icbvh_fec_envioimpronta,
                                 vehiculo.icbvh_fec_recepcionimpronta)
                         }).ToList();

            var data = query.Select(x => new
            {
                x.icbvh_id,
                x.vin,
                x.modvh_nombre,
                x.bodccs_nombre,
                x.icbvh_estatus,
                icbvh_fecinsp_ingreso = x.icbvh_fecinsp_ingreso != null
                    ? x.icbvh_fecinsp_ingreso.Value.ToShortDateString() + " " +
                      x.icbvh_fecinsp_ingreso.Value.ToShortTimeString()
                    : "",
                min_tomaImpronta = x.tiempo_tomaImpronta > 24 ? x.tiempo_tomaImpronta / 24 : x.tiempo_tomaImpronta,
                valorTiempo_tomaImpronta =
                    x.tiempo_tomaImpronta > 24 ? "Dias" : x.tiempo_tomaImpronta != null ? "Horas" : " ",
                icbvh_fec_impronta = x.icbvh_fec_impronta != null
                    ? x.icbvh_fec_impronta.Value.ToShortDateString() + " " +
                      x.icbvh_fec_impronta.Value.ToShortTimeString()
                    : "",
                min_envioImpronta = x.tiempo_envioImpronta > 24 ? x.tiempo_envioImpronta / 24 : x.tiempo_envioImpronta,
                valorTiempo_envioImpronta =
                    x.tiempo_envioImpronta > 24 ? "Dias" : x.tiempo_envioImpronta != null ? "Horas" : " ",
                icbvh_fec_envioimpronta = x.icbvh_fec_envioimpronta != null
                    ? x.icbvh_fec_envioimpronta.Value.ToShortDateString() + " " +
                      x.icbvh_fec_envioimpronta.Value.ToShortTimeString()
                    : "",
                min_recepcionImpronta = x.tiempo_recepcionImpronta > 24
                    ? x.tiempo_recepcionImpronta / 24
                    : x.tiempo_recepcionImpronta,
                valorTiempo_recepcionImpronta = x.tiempo_recepcionImpronta > 24 ? "Dias" :
                    x.tiempo_recepcionImpronta != null ? "Horas" : " ",
                icbvh_fec_recepcionimpronta = x.icbvh_fec_recepcionimpronta != null
                    ? x.icbvh_fec_recepcionimpronta.Value.ToShortDateString() + " " +
                      x.icbvh_fec_recepcionimpronta.Value.ToShortTimeString()
                    : ""
            });

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