using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Rotativa;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class checkListEntregasController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: checkListEntregas
        public ActionResult Index(int? menu, string planmayor)
        {
            if (!string.IsNullOrEmpty(planmayor))
            {
                ViewBag.txtPlanMayorBuscar = planmayor;
            }

            BuscarFavoritos(menu);
            return View();
        }


        public JsonResult BuscarPlanMayor(string planMayor)
        {
            var buscarVehiculo = (from vehiculo in context.icb_vehiculo
                                  join modelo in context.modelo_vehiculo
                                      on vehiculo.modvh_id equals modelo.modvh_codigo
                                  join color in context.color_vehiculo
                                      on vehiculo.colvh_id equals color.colvh_id
                                  join propietario in context.icb_terceros
                                      on vehiculo.propietario equals propietario.tercero_id into ps
                                  from propietario in ps.DefaultIfEmpty()
                                  join tpDocumento in context.tp_documento
                                      on propietario.tpdoc_id equals tpDocumento.tpdoc_id into tpDoc
                                  from tpDocumento in tpDoc.DefaultIfEmpty()
                                  where vehiculo.plan_mayor == planMayor
                                  select new
                                  {
                                      vehiculo.plan_mayor,
                                      vehiculo.vin,
                                      vehiculo.nummot_vh,
                                      vehiculo.plac_vh,
                                      modelo.modvh_nombre,
                                      color.colvh_nombre,
                                      tpDocumento.tpdoc_nombre,
                                      propietario.doc_tercero,
                                      propietario.prinom_tercero,
                                      propietario.segnom_tercero,
                                      propietario.apellido_tercero,
                                      propietario.segapellido_tercero
                                  }).FirstOrDefault();

            if (buscarVehiculo != null)
            {
                bool sinPropietario = buscarVehiculo.doc_tercero == null ? true : false;
                return Json(new { encontrado = true, sinPropietario, buscarVehiculo }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { encontrado = false }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult checkEntregas()
        {
            string root = Server.MapPath("~/Pdf/");
            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
            string path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);

            ViewAsPdf something = new ViewAsPdf("checkEntregas");

            return something;
        }


        public ActionResult inspeccionPreEntrega(string id)
        {
            var buscarVehiculo = (from vehiculo in context.icb_vehiculo
                                  join modelo in context.modelo_vehiculo
                                      on vehiculo.modvh_id equals modelo.modvh_codigo
                                  join color in context.color_vehiculo
                                      on vehiculo.colvh_id equals color.colvh_id
                                  join propietario in context.icb_terceros
                                      on vehiculo.propietario equals propietario.tercero_id into ps
                                  from propietario in ps.DefaultIfEmpty()
                                  join tpDocumento in context.tp_documento
                                      on propietario.tpdoc_id equals tpDocumento.tpdoc_id into tpDoc
                                  from tpDocumento in tpDoc.DefaultIfEmpty()
                                  where vehiculo.plan_mayor == id
                                  select new
                                  {
                                      vehiculo.plan_mayor,
                                      vehiculo.vin,
                                      vehiculo.nummot_vh,
                                      vehiculo.plac_vh,
                                      vehiculo.anio_vh,
                                      modelo.modvh_nombre,
                                      color.colvh_nombre,
                                      tpDocumento.tpdoc_id,
                                      propietario.doc_tercero,
                                      propietario.prinom_tercero,
                                      propietario.segnom_tercero,
                                      propietario.apellido_tercero,
                                      propietario.segapellido_tercero,
                                      //propietario.direc_tercero,
                                      propietario.razon_social
                                  }).FirstOrDefault();

            CheckEntregasModel modeloCheck = new CheckEntregasModel();
            if (buscarVehiculo != null)
            {
                if (buscarVehiculo.tpdoc_id == 1)
                {
                    modeloCheck.NombreCompleto = buscarVehiculo.razon_social;
                }

                if (buscarVehiculo.tpdoc_id == 2)
                {
                    modeloCheck.NombreCompleto = buscarVehiculo.prinom_tercero + " " + buscarVehiculo.segnom_tercero +
                                                 " " + buscarVehiculo.apellido_tercero + " " +
                                                 buscarVehiculo.segapellido_tercero;
                }
                //modeloCheck.Direccion = buscarVehiculo.direc_tercero;
                modeloCheck.NumeroVin = buscarVehiculo.vin;
                modeloCheck.NumeroMotor = buscarVehiculo.nummot_vh;
                modeloCheck.placa = buscarVehiculo.plac_vh;
                modeloCheck.AnioModelo = buscarVehiculo.anio_vh;
                modeloCheck.Modelo = buscarVehiculo.modvh_nombre;
                modeloCheck.NombreColor = buscarVehiculo.colvh_nombre;
            }

            string root = Server.MapPath("~/Pdf/");
            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
            string path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);

            ViewAsPdf something = new ViewAsPdf("inspeccionPreEntrega", modeloCheck);

            return something;
            //return View();
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