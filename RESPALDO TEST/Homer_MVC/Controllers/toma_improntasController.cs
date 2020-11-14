using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class toma_improntasController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: ingresoVehiculos
        public ActionResult toma_improntas(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }


        [HttpPost]
        public ActionResult toma_improntas(IngresoVhModel modelo, int? menu)
        {
            icb_vehiculo buscaVin = context.icb_vehiculo.FirstOrDefault(x => x.vin == modelo.vin);
            if (buscaVin != null)
            {
                icb_sysparameter buscarParametroStatus = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P7");
                string statusParametro = buscarParametroStatus != null ? buscarParametroStatus.syspar_value : "6";

                icb_sysparameter buscarParametroTpEvento = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P13");
                string tpEventoParametro = buscarParametroTpEvento != null ? buscarParametroTpEvento.syspar_value : "3";

                buscaVin.icbvh_estatus = statusParametro;
                buscaVin.id_evento = Convert.ToInt32(tpEventoParametro);
                //buscaVin.id_bod = buscaVin.icbvh_bodpro;
                buscaVin.icbvh_fec_impronta = DateTime.Now;
                context.Entry(buscaVin).State = EntityState.Modified;
                bool result = context.SaveChanges() > 0;
                if (result)
                {
                    TempData["mensaje"] = "La toma de impronta fue exitoso!";
                    context.icb_vehiculo_eventos.Add(new icb_vehiculo_eventos
                    {
                        eventofec_creacion = DateTime.Now,
                        eventouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        evento_nombre = "Toma Impronta",
                        evento_estado = true,
                        //id_vehiculo = modelo.icbvh_id,
                        vin = modelo.vin,
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
            else
            {
                TempData["mensaje_error"] = "No se encontro el numero de vin, verifique o intente mas tarde!";
            }

            BuscarFavoritos(menu);
            return View();
        }


        public JsonResult BuscarVehiculoVin(string vin)
        {
            int idUsuario = Convert.ToInt32(Session["user_usuarioid"]);
            List<bodega_usuario> bodegaUsuarioActual = context.bodega_usuario.Where(x => x.id_usuario == idUsuario).ToList();
            List<int> bodegasDelUsuario = new List<int>();
            foreach (bodega_usuario item in bodegaUsuarioActual)
            {
                bodegasDelUsuario.Add(item.id_bodega);
            }

            int estadoVin = 0;

            icb_sysparameter buscarParametroStatus = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P7");
            string statusParametro = buscarParametroStatus != null ? buscarParametroStatus.syspar_value : "6";

            var vehiculo = (from icb_vh in context.icb_vehiculo
                            join color in context.color_vehiculo
                                on icb_vh.colvh_id equals color.colvh_id
                            join bodega in context.bodega_concesionario
                                on icb_vh.id_bod equals bodega.id
                            join modelo in context.modelo_vehiculo
                                on icb_vh.modvh_id equals modelo.modvh_codigo
                            join evento in context.icb_tpeventos
                                on icb_vh.id_evento equals evento.tpevento_id
                            where icb_vh.vin == vin && bodegasDelUsuario.Contains(icb_vh.id_bod) &&
                                  icb_vh.icbvh_estatus == statusParametro && icb_vh.icbvh_fec_impronta == null
                            select new
                            {
                                icb_vh.icbvh_id,
                                icb_vh.vin,
                                modelo.modvh_nombre,
                                color.colvh_nombre,
                                bodega.bodccs_nombre,
                                evento.tpevento_nombre
                            }).ToList();

            if (vehiculo.Count > 0)
            {
                estadoVin = 1;
            }
            else
            {
                icb_vehiculo buscaSinBodega = context.icb_vehiculo.FirstOrDefault(x =>
                    x.vin == vin && !bodegasDelUsuario.Contains(x.icbvh_bodpro ?? 0));
                if (buscaSinBodega != null)
                {
                    estadoVin = -1; // Significa usuario no tiene permiso de ver el vin
                }
                else
                {
                    icb_vehiculo buscaVin = context.icb_vehiculo.FirstOrDefault(x => x.vin == vin);
                    if (buscaVin != null)
                    {
                        if (buscaVin.icbvh_estatus != statusParametro)
                        {
                            estadoVin = -2;
                        }

                        if (buscaVin.icbvh_fec_impronta != null)
                        {
                            estadoVin = -3;
                        }
                    }
                    else
                    {
                        estadoVin = -4; // Sginifica que el vin no existe en db
                    }
                }
            }

            var data = new
            {
                estadoVin,
                vehiculo
            };
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