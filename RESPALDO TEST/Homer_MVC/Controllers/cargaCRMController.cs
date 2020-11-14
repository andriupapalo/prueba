using Homer_MVC.IcebergModel;
using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class cargaCRMController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: cargaCRM
        public ActionResult Index(int? menu)
        {
            ViewBag.bodega_concesionario = context.bodega_concesionario.ToList();
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult BuscarDatosCargueExportados(int? bodega, DateTime? desde, DateTime? hasta)
        {
            //consulto parametro de exportación de información vehículos
            icb_sysparameter paraEx = context.icb_sysparameter.Where(d => d.syspar_cod == "P133").FirstOrDefault();
            int idEvento = paraEx != null ? Convert.ToInt32(paraEx.syspar_value) : 1061;
            int idEv = context.icb_tpeventos.Where(x => x.codigoevento == idEvento).FirstOrDefault().tpevento_id;

            //cambiar para que únicamente se muestren los registros que tengas eventos.
            if (desde == null)
            {
                desde = context.vw_cargacrmgm.OrderBy(x => x.fecha).Select(x => x.fecha).FirstOrDefault();
            }

            if (hasta == null)
            {
                hasta = context.vw_cargacrmgm.OrderByDescending(x => x.fecha).Select(x => x.fecha).FirstOrDefault();
            }
            else
            {
                hasta = hasta.GetValueOrDefault().AddDays(1);
            }

            if (bodega == null)
            {
                System.Collections.Generic.List<vw_cargacrmgm> vistaCRM = (from vista in context.vw_cargacrmgm
                                                                           where vista.fecha >= desde
                                                                                 && vista.fecha <= hasta && vista.IdEventoExportado == idEv
                                                                           select vista).ToList();

                var consultarVista = vistaCRM.Select(x => new
                {
                    concesionario = x.concesionario != null ? x.concesionario : "",
                    puntoventa = x.puntoventa != null ? x.puntoventa + " - " + x.bodccs_nombre : "",
                    serie = x.serie != null ? x.serie : "",
                    chevystar = x.chevystar != null ? x.chevystar : "",
                    nit_flota = x.nit_flota != null ? x.nit_flota : "",
                    nomflota = x.nomflota != null ? x.nomflota : "",
                    dirflota = x.dirflota != null ? x.dirflota : "",
                    ciuflota = x.ciuflota != null ? x.ciuflota : "",
                    depflota = x.depflota != null ? x.depflota : "",
                    paisflota = x.paisflota != null ? x.paisflota : "",
                    apartflota = x.apartflota != null ? x.apartflota : "",
                    telflota = x.telflota != null ? x.telflota : "",
                    nitleasing = x.nitleasing != null ? x.nitleasing : "",
                    nomleasing = x.nomleasing != null ? x.nomleasing : "",
                    dirleasing = x.dirleasing != null ? x.dirleasing : "",
                    ciuleasinf = x.ciuleasing != null ? x.ciuleasing : "",
                    depleasing = x.depleasing != null ? x.depleasing : "",
                    paisleasing = x.paisleasing != null ? x.paisleasing : "",
                    aparleasing = x.aparleasing != null ? x.aparleasing : "",
                    telleasing = x.telleasing != null ? x.telleasing : "",
                    nit_cliente = x.nit_cliente != null ? x.nit_cliente : "",
                    tipo = x.tipo != null ? x.tipo : "",
                    nomcliente = x.nomcliente != null ? x.nomcliente : "",
                    apellidocliente = x.apellidocliente != null ? x.apellidocliente : "",
                    gentercero_nombre = x.gentercero_nombre != null ? x.gentercero_nombre : "",
                    email_tercero = x.email_tercero != null ? x.email_tercero : "",
                    dircliente = x.dircliente != null ? x.dircliente : "",
                    ciucliente = x.ciucliente != null ? x.ciucliente : "",
                    depcliente = x.depcliente != null ? x.depcliente : "",
                    paiscliente = x.paiscliente != null ? x.paiscliente : "",
                    telcliente = x.telcliente != null ? x.telcliente : "",
                    aparcliente = x.aparcliente != null ? x.aparcliente : "",
                    celular = x.celular != null ? x.celular : "",
                    fec_nacimiento = x.fec_nacimiento != null
                        ? x.fec_nacimiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    vendedor = x.vendedor != null ? x.vendedor : 0,
                    tipoventa = x.tipoventa != null ? x.tipoventa : "",
                    formapago = x.formapago != null ? x.formapago : "",
                    numero = x.numero,
                    fecha = x.fecha != null ? x.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                    valor_mercancia = x.valor_mercancia,
                    valor_total = x.valor_total,
                    tpserv_nombre = x.tpserv_nombre != null ? x.tpserv_nombre : "",
                    x.fec_entrega /*!= null ? (Convert.ToDateTime(x.fec_entrega)).ToString("yyyy/MM/dd", new CultureInfo("en-US")) : ""*/,
                    planmayor = x.planmayor != null ? x.planmayor : "",
                    fec_alistamiento =
                        context.icb_vehiculo_eventos
                            .Where(z => (z.planmayor == x.planmayor || z.vin == x.serie) && z.id_tpevento == 2)
                            .FirstOrDefault() != null
                            ? context.icb_vehiculo_eventos
                                .Where(z => (z.planmayor == x.planmayor || z.vin == x.serie) && z.id_tpevento == 2)
                                .Select(c => c.fechaevento).FirstOrDefault()
                                .ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                            : "",
                    marca_anterior = x.marca_anterior != null ? x.marca_anterior : "",
                    modeloaterior = x.modeloaterior != null ? x.modeloaterior : "",
                    anoanterior = x.anoanterior != null ? x.anoanterior : "",
                    companiaseguros = x.companiaseguros != null ? x.companiaseguros : ""
                }).ToList();

                return Json(consultarVista, JsonRequestBehavior.AllowGet);
            }
            else
            {
                System.Collections.Generic.List<vw_cargacrmgm> vistaCRM = (from vista in context.vw_cargacrmgm
                                                                           where vista.fecha >= desde
                                                                                 && vista.fecha <= hasta && vista.IdEventoExportado == idEv
                                                                           select vista).ToList();

                var consultarVista = vistaCRM.Select(x => new
                {
                    concesionario = x.concesionario != null ? x.concesionario : "",
                    puntoventa = x.puntoventa != null ? x.puntoventa + " - " + x.bodccs_nombre : "",
                    serie = x.serie != null ? x.serie : "",
                    chevystar = x.chevystar != null ? x.chevystar : "",
                    nit_flota = x.nit_flota != null ? x.nit_flota : "",
                    nomflota = x.nomflota != null ? x.nomflota : "",
                    dirflota = x.dirflota != null ? x.dirflota : "",
                    ciuflota = x.ciuflota != null ? x.ciuflota : "",
                    depflota = x.depflota != null ? x.depflota : "",
                    paisflota = x.paisflota != null ? x.paisflota : "",
                    apartflota = x.apartflota != null ? x.apartflota : "",
                    telflota = x.telflota != null ? x.telflota : "",
                    nitleasing = x.nitleasing != null ? x.nitleasing : "",
                    nomleasing = x.nomleasing != null ? x.nomleasing : "",
                    dirleasing = x.dirleasing != null ? x.dirleasing : "",
                    ciuleasinf = x.ciuleasing != null ? x.ciuleasing : "",
                    depleasing = x.depleasing != null ? x.depleasing : "",
                    paisleasing = x.paisleasing != null ? x.paisleasing : "",
                    aparleasing = x.aparleasing != null ? x.aparleasing : "",
                    telleasing = x.telleasing != null ? x.telleasing : "",
                    nit_cliente = x.nit_cliente != null ? x.nit_cliente : "",
                    tipo = x.tipo != null ? x.tipo : "",
                    nomcliente = x.nomcliente != null ? x.nomcliente : "",
                    apellidocliente = x.apellidocliente != null ? x.apellidocliente : "",
                    gentercero_nombre = x.gentercero_nombre != null ? x.gentercero_nombre : "",
                    email_tercero = x.email_tercero != null ? x.email_tercero : "",
                    dircliente = x.dircliente != null ? x.dircliente : "",
                    ciucliente = x.ciucliente != null ? x.ciucliente : "",
                    depcliente = x.depcliente != null ? x.depcliente : "",
                    paiscliente = x.paiscliente != null ? x.paiscliente : "",
                    telcliente = x.telcliente != null ? x.telcliente : "",
                    aparcliente = x.aparcliente != null ? x.aparcliente : "",
                    celular = x.celular != null ? x.celular : "",
                    fec_nacimiento = x.fec_nacimiento != null
                        ? x.fec_nacimiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    vendedor = x.vendedor != null ? x.vendedor : 0,
                    tipoventa = x.tipoventa != null ? x.tipoventa : "",
                    formapago = x.formapago != null ? x.formapago : "",
                    x.numero,
                    fecha = x.fecha != null ? x.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                    x.valor_mercancia,
                    x.valor_total,
                    tpserv_nombre = x.tpserv_nombre != null ? x.tpserv_nombre : "",
                    x.fec_entrega /*!= null ? (Convert.ToDateTime(x.fec_entrega)).ToString("yyyy/MM/dd", new CultureInfo("en-US")) : ""*/,
                    planmayor = x.planmayor != null ? x.planmayor : "",
                    fec_alistamiento =
                        context.icb_vehiculo_eventos
                            .Where(z => (z.planmayor == x.planmayor || z.vin == x.serie) && z.id_tpevento == 2)
                            .FirstOrDefault() != null
                            ? context.icb_vehiculo_eventos
                                .Where(z => (z.planmayor == x.planmayor || z.vin == x.serie) && z.id_tpevento == 2)
                                .Select(c => c.fechaevento).FirstOrDefault()
                                .ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                            : "",
                    marca_anterior = x.marca_anterior != null ? x.marca_anterior : "",
                    modeloaterior = x.modeloaterior != null ? x.modeloaterior : "",
                    anoanterior = x.anoanterior != null ? x.anoanterior : "",
                    companiaseguros = x.companiaseguros != null ? x.companiaseguros : ""
                }).ToList();

                return Json(consultarVista, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult BuscarDatosCargue(int? bodega, DateTime? desde, DateTime? hasta)
        {
            //consulto parametro de exportación de información vehículos
            icb_sysparameter paraEx = context.icb_sysparameter.Where(d => d.syspar_cod == "P133").FirstOrDefault();
            int idEv = paraEx != null ? Convert.ToInt32(paraEx.syspar_value) : 1061;
            int idEvento = context.icb_tpeventos.Where(x => x.codigoevento == idEv).FirstOrDefault().tpevento_id;
            if (desde == null)
            {
                desde = context.vw_cargacrmgm.OrderBy(x => x.fecha).Select(x => x.fecha).FirstOrDefault();
            }

            if (hasta == null)
            {
                hasta = context.vw_cargacrmgm.OrderByDescending(x => x.fecha).Select(x => x.fecha).FirstOrDefault();
            }
            else
            {
                hasta = hasta.GetValueOrDefault().AddDays(1);
            }

            if (bodega == null)
            {
                //    var   vistaCRM2 = (from vista in context.vw_cargacrmgm
                //                          where vista.fecha >= desde
                //                           && vista.fecha <= hasta && vista.IdEventoExportado == idEv
                //                      select vista).ToList();

                System.Collections.Generic.List<vw_cargacrmgm> vistaCRM = context.vw_cargacrmgm
                    .Where(d => d.fecha >= desde && d.fecha <= hasta && d.IdEventoExportado != idEvento).ToList();
                var consultarRecepcion = vistaCRM.Select(x => new
                {
                    fec_alistamiento = context.icb_vehiculo_eventos
                        .Where(z => z.planmayor == x.planmayor || z.vin == x.serie && z.id_tpevento == 2)
                        .Select(c => c.fechaevento).FirstOrDefault()
                }).FirstOrDefault();

                var consultarVista = vistaCRM.Select(x => new
                {
                    concesionario = x.concesionario != null ? x.concesionario : "",
                    puntoventa = x.puntoventa != null ? x.puntoventa + " - " + x.bodccs_nombre : "",
                    serie = x.serie != null ? x.serie : "",
                    chevystar = x.chevystar != null ? x.chevystar : "",
                    nit_flota = x.nit_flota != null ? x.nit_flota : "",
                    nomflota = x.nomflota != null ? x.nomflota : "",
                    dirflota = x.dirflota != null ? x.dirflota : "",
                    ciuflota = x.ciuflota != null ? x.ciuflota : "",
                    depflota = x.depflota != null ? x.depflota : "",
                    paisflota = x.paisflota != null ? x.paisflota : "",
                    apartflota = x.apartflota != null ? x.apartflota : "",
                    telflota = x.telflota != null ? x.telflota : "",
                    nitleasing = x.nitleasing != null ? x.nitleasing : "",
                    nomleasing = x.nomleasing != null ? x.nomleasing : "",
                    dirleasing = x.dirleasing != null ? x.dirleasing : "",
                    ciuleasinf = x.ciuleasing != null ? x.ciuleasing : "",
                    depleasing = x.depleasing != null ? x.depleasing : "",
                    paisleasing = x.paisleasing != null ? x.paisleasing : "",
                    aparleasing = x.aparleasing != null ? x.aparleasing : "",
                    telleasing = x.telleasing != null ? x.telleasing : "",
                    nit_cliente = x.nit_cliente != null ? x.nit_cliente : "",
                    tipo = x.tipo != null ? x.tipo : "",
                    nomcliente = x.nomcliente != null ? x.nomcliente : "",
                    apellidocliente = x.apellidocliente != null ? x.apellidocliente : "",
                    gentercero_nombre = x.gentercero_nombre != null ? x.gentercero_nombre : "",
                    email_tercero = x.email_tercero != null ? x.email_tercero : "",
                    dircliente = x.dircliente != null ? x.dircliente : "",
                    ciucliente = x.ciucliente != null ? x.ciucliente : "",
                    depcliente = x.depcliente != null ? x.depcliente : "",
                    paiscliente = x.paiscliente != null ? x.paiscliente : "",
                    telcliente = x.telcliente != null ? x.telcliente : "",
                    aparcliente = x.aparcliente != null ? x.aparcliente : "",
                    celular = x.celular != null ? x.celular : "",
                    fec_nacimiento = x.fec_nacimiento != null
                        ? x.fec_nacimiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    vendedor = x.vendedor != null ? x.vendedor : 0,
                    tipoventa = x.tipoventa != null ? x.tipoventa : "",
                    formapago = x.formapago != null ? x.formapago : "",
                    x.numero,
                    fecha = x.fecha != null ? x.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                    x.valor_mercancia,
                    x.valor_total,
                    tpserv_nombre = x.tpserv_nombre != null ? x.tpserv_nombre : "",
                    x.fec_entrega /*!= null ? (Convert.ToDateTime(x.fec_entrega)).ToString("yyyy/MM/dd", new CultureInfo("en-US")) : ""*/,
                    planmayor = x.planmayor != null ? x.planmayor : "",
                    //fec_alistamiento = x.fec_alistamiento != null ? x.fec_alistamiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                    fec_alistamiento =
                        context.icb_vehiculo_eventos
                            .Where(z => (z.planmayor == x.planmayor || z.vin == x.serie) && z.id_tpevento == 2)
                            .FirstOrDefault() != null
                            ? context.icb_vehiculo_eventos
                                .Where(z => (z.planmayor == x.planmayor || z.vin == x.serie) && z.id_tpevento == 2)
                                .Select(c => c.fechaevento).FirstOrDefault()
                                .ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                            : "",
                    marca_anterior = x.marca_anterior != null ? x.marca_anterior : "",
                    modeloaterior = x.modeloaterior != null ? x.modeloaterior : "",
                    anoanterior = x.anoanterior != null ? x.anoanterior : "",
                    companiaseguros = x.companiaseguros != null ? x.companiaseguros : ""
                }).ToList();

                return Json(consultarVista, JsonRequestBehavior.AllowGet);
            }
            else
            {
                System.Collections.Generic.List<vw_cargacrmgm> vistaCRM = (from vista in context.vw_cargacrmgm
                                                                           where vista.bodcss_id == bodega
                                                                                 && vista.fecha >= desde
                                                                                 && vista.fecha <= hasta
                                                                           select vista).ToList();

                var consultarVista = vistaCRM.Select(x => new
                {
                    concesionario = x.concesionario != null ? x.concesionario : "",
                    puntoventa = x.puntoventa != null ? x.puntoventa + " - " + x.bodccs_nombre : "",
                    serie = x.serie != null ? x.serie : "",
                    chevystar = x.chevystar != null ? x.chevystar : "",
                    nit_flota = x.nit_flota != null ? x.nit_flota : "",
                    nomflota = x.nomflota != null ? x.nomflota : "",
                    dirflota = x.dirflota != null ? x.dirflota : "",
                    ciuflota = x.ciuflota != null ? x.ciuflota : "",
                    depflota = x.depflota != null ? x.depflota : "",
                    paisflota = x.paisflota != null ? x.paisflota : "",
                    apartflota = x.apartflota != null ? x.apartflota : "",
                    telflota = x.telflota != null ? x.telflota : "",
                    nitleasing = x.nitleasing != null ? x.nitleasing : "",
                    nomleasing = x.nomleasing != null ? x.nomleasing : "",
                    dirleasing = x.dirleasing != null ? x.dirleasing : "",
                    ciuleasinf = x.ciuleasing != null ? x.ciuleasing : "",
                    depleasing = x.depleasing != null ? x.depleasing : "",
                    paisleasing = x.paisleasing != null ? x.paisleasing : "",
                    aparleasing = x.aparleasing != null ? x.aparleasing : "",
                    telleasing = x.telleasing != null ? x.telleasing : "",
                    nit_cliente = x.nit_cliente != null ? x.nit_cliente : "",
                    tipo = x.tipo != null ? x.tipo : "",
                    nomcliente = x.nomcliente != null ? x.nomcliente : "",
                    apellidocliente = x.apellidocliente != null ? x.apellidocliente : "",
                    gentercero_nombre = x.gentercero_nombre != null ? x.gentercero_nombre : "",
                    email_tercero = x.email_tercero != null ? x.email_tercero : "",
                    dircliente = x.dircliente != null ? x.dircliente : "",
                    ciucliente = x.ciucliente != null ? x.ciucliente : "",
                    depcliente = x.depcliente != null ? x.depcliente : "",
                    paiscliente = x.paiscliente != null ? x.paiscliente : "",
                    telcliente = x.telcliente != null ? x.telcliente : "",
                    aparcliente = x.aparcliente != null ? x.aparcliente : "",
                    celular = x.celular != null ? x.celular : "",
                    fec_nacimiento = x.fec_nacimiento != null
                        ? x.fec_nacimiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    vendedor = x.vendedor != null ? x.vendedor : 0,
                    tipoventa = x.tipoventa != null ? x.tipoventa : "",
                    formapago = x.formapago != null ? x.formapago : "",
                    //numero = x.numero != null ? x.numero : 0,
                    numero = x.numero,
                    fecha = x.fecha != null ? x.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                    /*valor_mercancia = x.valor_mercancia != null ? x.valor_mercancia : 0,
                    valor_total = x.valor_total != null ? x.valor_total : 0,*/
                    valor_mercancia = x.valor_mercancia,
                    valor_total = x.valor_total,
                    tpserv_nombre = x.tpserv_nombre != null ? x.tpserv_nombre : "",
                    x.fec_entrega /*!= null ? (Convert.ToDateTime(x.fec_entrega)).ToString("yyyy/MM/dd", new CultureInfo("en-US")) : ""*/,
                    planmayor = x.planmayor != null ? x.planmayor : "",
                    fec_alistamiento =
                        context.icb_vehiculo_eventos
                            .Where(z => (z.planmayor == x.planmayor || z.vin == x.serie) && z.id_tpevento == 2)
                            .FirstOrDefault() != null
                            ? context.icb_vehiculo_eventos
                                .Where(z => (z.planmayor == x.planmayor || z.vin == x.serie) && z.id_tpevento == 2)
                                .Select(c => c.fechaevento).FirstOrDefault()
                                .ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                            : "",
                    //fec_alistamiento = x.fec_alistamiento != null ? x.fec_alistamiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                    marca_anterior = x.marca_anterior != null ? x.marca_anterior : "",
                    modeloaterior = x.modeloaterior != null ? x.modeloaterior : "",
                    anoanterior = x.anoanterior != null ? x.anoanterior : "",
                    companiaseguros = x.companiaseguros != null ? x.companiaseguros : ""
                }).ToList();

                return Json(consultarVista, JsonRequestBehavior.AllowGet);
            }
        }

        public static implicit operator cargaCRMController(string v)
        {
            throw new NotImplementedException();
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