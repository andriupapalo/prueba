using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class consultaDocumentosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: consultaDocumentos
        public ActionResult Index(int? menu)
        {
            var documentos = from t in context.tp_doc_registros
                             where t.tpdoc_estado
                             orderby t.tpdoc_nombre
                             select new
                             {
                                 documento = "(" + t.prefijo + ") " + t.tpdoc_nombre,
                                 idDoc = t.tpdoc_id
                             };
            List<SelectListItem> tipoDoc = new List<SelectListItem>();
            foreach (var item in documentos)
            {
                tipoDoc.Add(new SelectListItem
                {
                    Text = item.documento,
                    Value = item.idDoc.ToString()
                });
            }

            ViewBag.tipoDoc = tipoDoc;
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult BuscarDocumentos(int? tipo_documento, int? bodega_id)
        {
            if (tipo_documento != null && bodega_id != null)
            {
                var buscarDocumentos = (from encabezado in context.encab_documento
                                        join tercer in context.icb_terceros
                                            on encabezado.nit equals tercer.tercero_id
                                        where encabezado.tipo == tipo_documento && encabezado.bodega == bodega_id
                                        orderby encabezado.numero descending
                                        select new
                                        {
                                            encabezado.numero,
                                            encabezado.fecha,
                                            cliente = tercer.razon_social + "" + tercer.prinom_tercero + " " + tercer.segnom_tercero + " ",
                                            tercer.razon_social
                                        }).OrderByDescending(x => x.numero).Take(50).ToList();
                return Json(buscarDocumentos, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDocumento(int? numero, int? tpDoc, int? bodega, int? otro, string plan_mayor)
        {
            int numero2 = 0;
            if (otro != null)
            {
                numero = otro;
            }

            if (plan_mayor != "")
            {
                var documento = (from t in context.encab_documento
                                 where t.documento == plan_mayor
                                 select new
                                 {
                                     t.numero
                                 }).FirstOrDefault();
                if (documento != null)
                {
                    numero2 = Convert.ToInt32(documento.numero);
                }
                //bu
            }

            var buscarDocumento = (from encabezado in context.encab_documento
                                   join tercero in context.icb_terceros
                                       on encabezado.nit equals tercero.tercero_id into ter
                                   from tercero in ter.DefaultIfEmpty()
                                   join tt in context.tp_doc_registros
                                       on encabezado.tipo equals tt.tpdoc_id into ptt
                                   from tt in ptt.DefaultIfEmpty()
                                   where (encabezado.numero == numero || encabezado.numero == otro || encabezado.numero == numero2) &&
                                         (encabezado.tipo == tpDoc) & (encabezado.bodega == bodega)
                                   select new
                                   {
                                       encabezado.idencabezado,
                                       encabezado.fecha,
                                       encabezado.vencimiento,
                                       encabezado.numero,
                                       tercero.tercero_id,
                                       tercero.doc_tercero, // Se utilizo  en Actualizacion realizada el 04 /07 /2018 
                                       prinom_tercero = tercero.prinom_tercero != null ? tercero.prinom_tercero : "",
                                       segnom_tercero = tercero.segnom_tercero != null ? tercero.segnom_tercero : "",
                                       apellido_tercero = tercero.apellido_tercero != null ? tercero.apellido_tercero : "",
                                       segapellido_tercero = tercero.segapellido_tercero != null ? tercero.segapellido_tercero : "",
                                       razon_social = tercero.razon_social != null ? tercero.razon_social : "",
                                       direccion = (from direcciones in context.terceros_direcciones
                                                    join ciudad in context.nom_ciudad
                                                        on direcciones.ciudad equals ciudad.ciu_id
                                                    where direcciones.idtercero == tercero.tercero_id
                                                    select new
                                                    {
                                                        direcciones.id,
                                                        direccion = direcciones.direccion + " (" + ciudad.ciu_nombre + ")"
                                                    }).OrderByDescending(x => x.id).FirstOrDefault(),
                                       tercero.celular_tercero,
                                       tercero.email_tercero,
                                       nomCompleto = tercero.tpdoc_id == 1
                                           ? "(" + tercero.doc_tercero + ") " + tercero.razon_social
                                           : "(" + tercero.doc_tercero + ") " + tercero.prinom_tercero + " " + tercero.segnom_tercero +
                                             " " + tercero.apellido_tercero + " " + tercero.segapellido_tercero,
                                       facturacompra = encabezado.documento != null ? encabezado.documento : "",
                                       tipoCompra = tt.sw != null ? tt.sw : 0
                                   }).FirstOrDefault();

            if (buscarDocumento != null)
            {
                var movimientos = (from movimiento in context.mov_contable
                                   join cuenta in context.cuenta_puc
                                       on movimiento.cuenta equals cuenta.cntpuc_id
                                   join centro in context.centro_costo
                                       on movimiento.centro equals centro.centcst_id into cc
                                   from centro in cc.DefaultIfEmpty()
                                   where movimiento.id_encab == buscarDocumento.idencabezado
                                   select new
                                   {
                                       movimiento.cuenta,
                                       cuenta.cntpuc_numero,
                                       cuenta.cntpuc_descp,
                                       movimiento.debito,
                                       movimiento.credito,
                                       movimiento.basecontable,
                                       movimiento.creditoniif,
                                       movimiento.debitoniif,
                                       nomcentro = " (" + centro.pre_centcst + ") " + centro.centcst_nombre
                                   }).ToList();

                var calculoMovimientos = new
                {
                    creditoTotal = movimientos.Sum(x => x.credito),
                    debitoTotal = movimientos.Sum(x => x.debito),
                    baseTotal = movimientos.Sum(x => x.basecontable),
                    creditoNiffTotal = movimientos.Sum(x => x.creditoniif),
                    debitoNiffTotal = movimientos.Sum(x => x.debitoniif)
                };

                var buscarFactura = new
                {
                    //    fecha = buscarDocumento.fecha.ToShortDateString(),
                    fecha = buscarDocumento.fecha.ToString("yyyy/MM/dd"), /// Actualizacion realizada el 04 /07 /2018 
					vencimiento = buscarDocumento.vencimiento != null
                        ? buscarDocumento.vencimiento.Value.ToString("yyyy/MM/dd")
                        : "", /// Actualizacion realizada el 25 /09 /2018 
					buscarDocumento.numero,
                    buscarDocumento.doc_tercero, // se utilizo en Actualizacion realizada el 04 /07 /2018 
                    buscarDocumento.prinom_tercero,
                    buscarDocumento.segnom_tercero,
                    buscarDocumento.apellido_tercero,
                    buscarDocumento.segapellido_tercero,
                    direc_tercero = buscarDocumento.direccion,
                    buscarDocumento.celular_tercero,
                    buscarDocumento.razon_social,
                    buscarDocumento.email_tercero, // Actualizacion realizada el 04 /07 /2018 
                    nombrecliente = buscarDocumento.nomCompleto,
                    buscarDocumento.facturacompra,
                    buscarDocumento.tipoCompra
                };

                var buscarLineas = (from lineas in context.lineas_documento
                                    join referencia in context.icb_referencia
                                        on lineas.codigo equals referencia.ref_codigo
                                    join bodegaaux in context.bodega_concesionario
                                        on lineas.bodega equals bodegaaux.id into bod
                                    from bodegaaux in bod.DefaultIfEmpty()
                                    where lineas.id_encabezado == buscarDocumento.idencabezado
                                    select new
                                    {
                                        lineas.codigo,
                                        referencia.ref_descripcion,
                                        lineas.cantidad,
                                        porcentaje_iva = lineas.porcentaje_iva != null ? lineas.porcentaje_iva : 0,
                                        porcentaje_descuento = lineas.porcentaje_descuento != null ? lineas.porcentaje_descuento : 0,
                                        //costo_unitario = lineas.costo_unitario != null ? lineas.costo_unitario : 0,
                                        costo_unitario = lineas != null ? lineas.costo_unitario : 0,
                                        bodega1 = "(" + bodegaaux.bodccs_cod + ") " + bodegaaux.bodccs_nombre
                                    }).ToList();

                var detalles = buscarLineas.Select(x => new
                {
                    x.bodega1,
                    x.codigo,
                    x.ref_descripcion,
                    x.cantidad,
                    x.porcentaje_descuento,
                    x.porcentaje_iva,
                    x.costo_unitario,
                    valorIva = (decimal)x.porcentaje_iva * x.costo_unitario / 100 * x.cantidad,
                    valorDescuento = (decimal)x.porcentaje_descuento * x.costo_unitario / 100 * x.cantidad,
                    valorTotal = x.costo_unitario * x.cantidad +
                                 (decimal)x.porcentaje_iva * x.costo_unitario / 100 * x.cantidad -
                                 (decimal)x.porcentaje_descuento * x.costo_unitario / 100 * x.cantidad
                }).ToList();

                //aqui agregare la consulta y filtrado de documentos aplicados
                var crucedocumentos = (from docu in context.cruce_documentos
                                       where docu.numeroaplica == numero && docu.idtipo == tpDoc
                                       select new
                                       {
                                           docu.idtipo,
                                           docu.tp_doc_registros.prefijo,
                                           //nombretipo = docu.tp_doc_registros.tpdoc_nombre,
                                           descripcion = docu.tp_doc_registros.tpdoc_nombre,
                                           valorap = docu.numero,
                                           docu.valor,
                                           // fecha = docu.fecha.ToString("yyyy/MM/dd"),
                                           fecha = docu.fecha.ToString()
                                       }
                    ).ToList();

                var documentosaplicados = crucedocumentos.ToList();
                //

                //
                return Json(
                    new
                    {
                        encontrado = true,
                        buscarFactura,
                        movimientos,
                        calculoMovimientos,
                        detalles,
                        documentosaplicados
                    }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { encontrado = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarBodegasPorDocumento(int? tpDoc)
        {
            var buscarBodegas = (from consecutivos in context.icb_doc_consecutivos
                                 join bodega in context.bodega_concesionario
                                     on consecutivos.doccons_bodega equals bodega.id
                                 where consecutivos.doccons_idtpdoc == tpDoc
                                 select new
                                 {
                                     bodega.bodccs_nombre,
                                     bodega.id
                                 }).ToList();
            return Json(buscarBodegas, JsonRequestBehavior.AllowGet);
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