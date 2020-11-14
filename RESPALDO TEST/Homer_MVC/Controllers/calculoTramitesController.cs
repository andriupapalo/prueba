using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class calculoTramitesController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: calculoTramites
        public ActionResult Index(int? menu)
        {
            var list = (from t in context.users
                        where t.rol_id == 4
                        select new
                        {
                            id = t.user_id,
                            nombre = t.user_nombre + " - " + t.user_apellido
                        }).ToList();

            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in list)
            {
                lista.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }

            ViewBag.asesor = lista;
            ViewBag.vehiculo = new SelectList(context.modelo_vehiculo, "modvh_codigo", "modvh_nombre");
            ViewBag.ciudadp = new SelectList(context.nom_ciudad, "ciu_id", "ciu_nombre");
            ViewBag.ciudadt = new SelectList(context.nom_ciudad, "ciu_id", "ciu_nombre");
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult BuscarValores(int ciudad)

        {
            int bodega = Convert.ToInt32(Session["user_bodega"]);

            var data = (from a in context.valores_trasmites
                        join b in context.valortramitebodega
                            on a.idvalor equals b.idvalortramite
                        where a.ciudad_id == ciudad && b.idbodega == bodega
                        select new
                        {
                            a.antec_pazysalvo,
                            a.bodega,
                            a.cert_transito,
                            a.ciudad_id,
                            a.consig_minist_tte,
                            a.copia_factura,
                            a.derechos_transito,
                            a.estampillas,
                            a.idvalor,
                            a.prenda,
                            a.radicacion_cuenta,
                            a.runt,
                            a.semaforizacion,
                            a.serv_tramitador,
                            a.sijin,
                            a.sistematizacion,
                            a.sistematizacion_impuestos,
                            a.tarjeta,
                            a.tradicion,
                            a.traslado_cuenta,
                            a.traspaso
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