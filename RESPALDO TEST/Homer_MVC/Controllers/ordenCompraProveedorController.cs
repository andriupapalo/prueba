using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Rotativa;
using Rotativa.Options;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class ordenCompraProveedorController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        private readonly CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

        private CultureInfo Culture = new CultureInfo("is-IS");

        public void ListasDesplegables(rordencompra modelo)
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            int rolActual = Convert.ToInt32(Session["user_rolid"]);

            List<tp_doc_registros> tipoDocumentos = context.tp_doc_registros.Where(x => x.tipo == 9 && x.tpdoc_estado).ToList();
            List<SelectListItem> listaDocumentos = new List<SelectListItem>();
            foreach (tp_doc_registros item in tipoDocumentos)
            {
                listaDocumentos.Add(new SelectListItem
                { Value = item.tpdoc_id.ToString(), Text = "(" + item.prefijo + ") " + item.tpdoc_nombre });
            }

            ViewBag.idtipodoc = new SelectList(listaDocumentos, "Value", "Text", modelo.idtipodoc);
            ViewBag.tipoorden = new SelectList(context.rtipocompra, "id", "descripcion", modelo.tipoorden);
            ViewBag.condicion_pago =
                new SelectList(context.fpago_tercero, "fpago_id", "fpago_nombre", modelo.condicion_pago);

            var provedores = from pro in context.tercero_proveedor
                             join ter in context.icb_terceros
                                 on pro.tercero_id equals ter.tercero_id
                             select new
                             {
                                 idTercero = ter.tercero_id,
                                 ter.doc_tercero,
                                 nombreTercero = ter.razon_social + ter.prinom_tercero + " " + ter.segnom_tercero + " " +
                                                 ter.apellido_tercero + " " + ter.segapellido_tercero
                             };
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in provedores)
            {
                string nombre = item.doc_tercero + " - " + item.nombreTercero;
                items.Add(new SelectListItem { Text = nombre, Value = item.idTercero.ToString() });
            }

            ViewBag.proveedor = new SelectList(items, "Value", "Text", modelo.proveedor);

            var clientes = from cli in context.tercero_cliente
                           join ter in context.icb_terceros
                               on cli.tercero_id equals ter.tercero_id
                           select new
                           {
                               idTercero = ter.tercero_id,
                               documento = ter.doc_tercero,
                               nombreTErcero = ter.prinom_tercero,
                               segNombre = ter.segnom_tercero,
                               apellidosTercero = ter.apellido_tercero,
                               segApellido = ter.segapellido_tercero,
                               razonSocial = ter.razon_social
                           };

            // Rol 6 significa asesor de repuestos de la tabla rols en DB
            List<users> asesores = context.users.Where(x => x.rols.rol_nombre.Contains("Repuestos")).ToList();
            List<SelectListItem> itemsAsesores = new List<SelectListItem>();
            foreach (users item in asesores)
            {
                string nombre = item.user_nombre + " " + item.user_apellido;
                itemsAsesores.Add(new SelectListItem { Text = nombre, Value = item.user_id.ToString() });
            }

            ViewBag.vendedor = new SelectList(itemsAsesores, "Value", "Text", modelo.vendedor);
            var referencias = (from referencia in context.icb_referencia
                               where referencia.modulo == "R"
                               select new
                               {
                                   id = referencia.ref_codigo,
                                   nombre = referencia.ref_codigo + " - " + referencia.ref_descripcion
                               }).ToList();
            ViewBag.id_referencia = new SelectList(referencias, "id", "nombre");

            var buscarResponsable = (from usuario in context.users
                                           where usuario.user_id == usuarioActual //&& bodegaUsuario.id_bodega == bodegaActual
                                           select new
                                           {
                                               usuario.user_id,
                                               nombre = usuario.user_nombre + " " + usuario.user_apellido
                                           }).ToList();

            ViewBag.responsable = new SelectList(buscarResponsable, "user_id", "nombre");



        }

        public JsonResult BuscarBodegasPorDocumento(int? idTpDoc)
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);
            List<int> bodegasUsuario = context.bodega_usuario.Where(x => x.id_usuario == usuarioActual)
                .Select(x => x.id_bodega).ToList();
            var buscarBodegasDelDocumento = (from doc_consecutivos in context.icb_doc_consecutivos
                                             join bodega in context.bodega_concesionario
                                                 on doc_consecutivos.doccons_bodega equals bodega.id
                                             where doc_consecutivos.doccons_idtpdoc == idTpDoc &&
                                                   bodegasUsuario.Contains(doc_consecutivos.doccons_bodega)
                                             select new
                                             {
                                                 bodega.id,
                                                 bodega.bodccs_nombre
                                             }).ToList();
            return Json(buscarBodegasDelDocumento, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarReferencias()
        {
            var data = from r in context.icb_referencia
                       where r.modulo == "R"
                       select new
                       {
                           codigo = r.ref_codigo
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // GET: ordenCompraProveedor
        public ActionResult Create(int? menu)
        {
            ListasDesplegables(new rordencompra());
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult OrdenCompraPDF(int? id)
        {
            OrdenRepuestoModel modelo = new OrdenRepuestoModel();

            string root = Server.MapPath("~/Pdf/");
            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
            string path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);

            #region Consultas

            var buscarOrden = (from encabezado in context.rordencompra
                               join tipoDocumento in context.tp_doc_registros
                                   on encabezado.idtipodoc equals tipoDocumento.tpdoc_id into aa
                               from tipoDocumento in aa.DefaultIfEmpty()
                               join direccion in context.terceros_direcciones
                                   on encabezado.direccion equals direccion.id into bb
                               from direccion in bb.DefaultIfEmpty()
                               join ciudad in context.nom_ciudad
                                   on direccion.ciudad equals ciudad.ciu_id into xx
                               from ciudad in xx.DefaultIfEmpty()
                               join bodega in context.bodega_concesionario
                                   on encabezado.bodega equals bodega.id into cc
                               from bodega in cc.DefaultIfEmpty()
                                   //join cliente in context.icb_terceros
                                   //on encabezado.destinatario equals cliente.tercero_id
                               join tipoCompra in context.rtipocompra
                                   on encabezado.tipoorden equals tipoCompra.id into dd
                               from tipoCompra in dd.DefaultIfEmpty()
                               join proveedor in context.icb_terceros
                                   on encabezado.proveedor equals proveedor.tercero_id into ee
                               from proveedor in ee.DefaultIfEmpty()
                               join ciudadProveedor in context.nom_ciudad
                                   on proveedor.ciu_id equals ciudadProveedor.ciu_id into ciuPro
                               from ciudadProveedor in ciuPro.DefaultIfEmpty()
                               join departamento in context.nom_departamento
                                   on ciudadProveedor.dpto_id equals departamento.dpto_id into deptoPro
                               from departamento in deptoPro.DefaultIfEmpty()
                               join pais in context.nom_pais
                                   on departamento.pais_id equals pais.pais_id into paisPro
                               from pais in paisPro.DefaultIfEmpty()
                               join formaPago in context.fpago_tercero
                                   on encabezado.condicion_pago equals formaPago.fpago_id into ff
                               from formaPago in ff.DefaultIfEmpty()
                               join asesor in context.users
                                   on encabezado.vendedor equals asesor.user_id into gg
                               from asesor in gg.DefaultIfEmpty()
                               where encabezado.idorden == id
                               select new
                               {
                                   encabezado.idorden,
                                   encabezado.numero,
                                   encabezado.diasvalidez,
                                   encabezado.fec_creacion,
                                   encabezado.descuentopie,
                                   encabezado.iva,
                                   encabezado.fletes,
                                   encabezado.ivafletes,
                                   encabezado.valor,
                                   encabezado.notas,
                                   ciudad.ciu_nombre,
                                   direccion.direccion,
                                   encabezado.destinatario,
                                   //cliente.doc_tercero,
                                   //cliente.prinom_tercero,
                                   //cliente.segnom_tercero,
                                   //cliente.apellido_tercero,
                                   //cliente.segapellido_tercero,
                                   //cliente.celular_tercero,
                                   //cliente.email_tercero,
                                   tipoDocumento.tpdoc_nombre,
                                   tipoDocumento.prefijo,
                                   bodega.bodccs_cod,
                                   bodega.bodccs_nombre,
                                   tipoCompra.descripcion,
                                   nombre_proveedor = proveedor.prinom_tercero,
                                   segNombre_proveedor = proveedor.segnom_tercero,
                                   apellido_proveedor = proveedor.apellido_tercero,
                                   segApellidoProveedor = proveedor.segapellido_tercero,
                                   emailProveedor = proveedor.email_tercero,
                                   proveedor.razon_social,
                                   DocumentoProveedor = proveedor.doc_tercero,
                                   proveedor.telf_tercero,
                                   //proveedor.direc_tercero,
                                   ciudadProveedor = ciudadProveedor.ciu_nombre,
                                   paisProveedor = pais.pais_nombre,
                                   formaPago.fpago_nombre,
                                   asesor.user_nombre,
                                   asesor.user_apellido
                               }).OrderByDescending(x => x.fec_creacion).FirstOrDefault();


            var buscarDetalles = (from detalles in context.rdetalleordencompra
                                  join referencia in context.icb_referencia
                                      on detalles.codigo_referencia equals referencia.ref_codigo
                                  where detalles.idencaborden == id
                                  select new
                                  {
                                      detalles.codigo_referencia,
                                      referencia.ref_descripcion,
                                      detalles.cantidad,
                                      detalles.porceniva,
                                      detalles.porcendescto,
                                      detalles.valorunitario
                                  }).ToList();

            int user = Convert.ToInt32(Session["user_usuarioid"]);

            var buscarUsuario = (from a in context.users
                                 where a.user_id == user
                                 select new
                                 {
                                     a.user_nombre,
                                     a.user_apellido
                                 }).FirstOrDefault();

            #endregion

            #region Armado de informacion a mostrar

            if (buscarOrden != null)
            {
                modelo.NumeroOrden = buscarOrden.numero;
                modelo.Notas = buscarOrden.notas;
                //modelo.DireccionDestinatario = buscarOrden.direccion + " (" + buscarOrden.ciu_nombre + ")";
                modelo.DiasValidez = buscarOrden.diasvalidez;
                modelo.Fecha = buscarOrden.fec_creacion.ToShortDateString();
                modelo.Bodega = buscarOrden.bodccs_nombre;
                modelo.TipoCompra = buscarOrden.descripcion;
                modelo.Proveedor = buscarOrden.nombre_proveedor + " " + buscarOrden.segNombre_proveedor + " " +
                                   buscarOrden.apellido_proveedor + " " + buscarOrden.segApellidoProveedor + " " +
                                   buscarOrden.razon_social;
                modelo.FormaPago = buscarOrden.fpago_nombre;
                modelo.Descuento = buscarOrden.descuentopie ?? 0;
                modelo.Iva = buscarOrden.iva ?? 0;
                modelo.Fletes = buscarOrden.fletes ?? 0;
                modelo.IvaFletes = buscarOrden.ivafletes ?? 0;
                modelo.Total = buscarOrden.valor ?? 0;
                modelo.Asesor = buscarOrden.user_nombre + " " + buscarOrden.user_apellido;
                modelo.DocumentoProveedor = buscarOrden.DocumentoProveedor;
                //modelo.DireccionProveedor = buscarOrden.direc_tercero;
                modelo.TelefonoProveedor = buscarOrden.telf_tercero;
                modelo.CiudadProveedor = buscarOrden.ciudadProveedor;
                modelo.PaisProveedor = buscarOrden.paisProveedor;
                modelo.usuario_login = buscarUsuario.user_nombre + " " + buscarUsuario.user_apellido;
            }

            decimal subTotal = 0;
            foreach (var item in buscarDetalles)
            {
                decimal valorUnitario = item.valorunitario ?? 0;
                decimal valorIva = item.porceniva ?? 0;
                decimal valorDescuento = item.porcendescto ?? 0;
                decimal iva = valorUnitario * valorIva / 100;
                decimal descuento = valorUnitario * valorDescuento / 100;
                decimal cantidad = item.cantidad;
                subTotal += (valorUnitario - valorUnitario * valorDescuento / 100) * cantidad;
                modelo.ListaDetalles.Add(new DetalleRepuestoOrdenModel
                {
                    CodigoReferencia = item.codigo_referencia,
                    NombreReferencia = item.ref_descripcion,
                    DescuentoReferencia = item.porcendescto ?? 0,
                    IvaReferencia = item.porceniva ?? 0,
                    PrecioReferencia = item.valorunitario ?? 0,
                    CantidadReferencia = item.cantidad,
                    PrecioTotal = (valorUnitario - valorUnitario * valorDescuento / 100) * cantidad
                });
            }


            int Numordencomra = buscarOrden.numero;

            modelo.SubTotal = subTotal;

            #endregion

            //modelo.Iva = (modelo.Iva * subTotal) / 100; 
            //creo el PDF
            string fecha = DateTime.Now.ToString("dd-MM-yyyy");

            ViewAsPdf something = new ViewAsPdf("OrdenCompraPDF", modelo)
            {
                PageSize = Size.A4,
                PageOrientation = Orientation.Landscape,
                PageMargins = new Margins(0, 0, 0, 0),
                PageWidth = 210,
                PageHeight = 297
                /*FileName = "cv.pdf" SaveOnServerPath = path*/
            };

            byte[] applicationPDFData = something.BuildPdf(ControllerContext);
            FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            fileStream.Write(applicationPDFData, 0, applicationPDFData.Length);
            fileStream.Close();

            if (buscarOrden.emailProveedor != null)
            {
                enviarcorreo(id.Value, applicationPDFData, "Orden_de_compra" + Numordencomra + "_" + fecha + ".pdf",
                    buscarOrden.emailProveedor);
            }

            return something;
        }

        [HttpPost]
        public ActionResult Create(rordencompra modelo, int? menu)
        {
            ViewBag.bodegaSeleccionado = modelo.bodega;
            ViewBag.destinatarioSeleccionado = Request["txtCedulaDestinatario"];
            if (ModelState.IsValid)
            {
                long numeroConsecutivo = 0;
                DateTime fechaActual = DateTime.Now;
                icb_doc_consecutivos numeroConsecutivoAux = context.icb_doc_consecutivos.OrderByDescending(x => x.doccons_ano)
                    .FirstOrDefault(x => x.doccons_idtpdoc == modelo.idtipodoc && x.doccons_bodega == modelo.bodega);
                grupoconsecutivos grupoConsecutivo = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == modelo.idtipodoc && x.bodega_id == modelo.bodega);
                if (numeroConsecutivoAux != null)
                {
                    if (numeroConsecutivoAux.doccons_requiere_mes)
                    {
                        if (numeroConsecutivoAux.doccons_mes != fechaActual.Month)
                        {
                            TempData["mensaje_error"] =
                                "No existe un numero consecutivo asignado para este tipo de documento en el mes actual que es requerido";
                            ListasDesplegables(modelo);
                            BuscarFavoritos(menu);
                            return View();
                        }

                        numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                    }
                    else if (numeroConsecutivoAux.doccons_requiere_anio)
                    {
                        if (numeroConsecutivoAux.doccons_ano != fechaActual.Year)
                        {
                            TempData["mensaje_error"] =
                                "No existe un numero consecutivo asignado para este tipo de documento en el año actual que es requerido";
                            ListasDesplegables(modelo);
                            BuscarFavoritos(menu);
                            return View();
                        }

                        numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                    }
                    else
                    {
                        numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                    }
                }
                else
                {
                    TempData["mensaje_error"] = "No existe un numero consecutivo asignado para este tipo de documento";
                    ListasDesplegables(modelo);
                    BuscarFavoritos(menu);
                    return View();
                }

                // Si llega hasta aqui significa que si existe un numero consecutivo para este tipo de documento
                modelo.numero = (int)numeroConsecutivo;
                string valorTotal = Request["txtValor"];
                string valorIvaTotal = Request["txtTotalIva"];
                string valorFletes = Request["fletes"];
                modelo.valor = !string.IsNullOrEmpty(valorTotal) ? Convert.ToDecimal(valorTotal, Culture) : 0;
                modelo.iva = !string.IsNullOrEmpty(valorIvaTotal) ? Convert.ToDecimal(valorIvaTotal, Culture) : 0;
                modelo.fletes = !string.IsNullOrEmpty(valorFletes) ? Convert.ToDecimal(valorFletes, Culture) : 0;

                modelo.fec_creacion = DateTime.Now;
                modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                modelo.responsable= Convert.ToInt32(Session["user_usuarioid"]);
                context.rordencompra.Add(modelo);
                int guardarOrden = context.SaveChanges();

                if (guardarOrden > 0)
                {
                    rordencompra consultarUltimaOrden = context.rordencompra.OrderByDescending(x => x.idorden).FirstOrDefault();
                    int lista_referencias = Convert.ToInt32(Request["lista_referencias"]);
                    int secuencia = 1;
                    for (int i = 0; i < lista_referencias; i++)
                    {
                        string refe = Request["id_referenciaTable" + i];
                        if (!string.IsNullOrEmpty(refe))
                        {
                            decimal iva = !string.IsNullOrEmpty(Request["por_iva_referenciaTable" + i])
                                ? Convert.ToDecimal(Request["por_iva_referenciaTable" + i], Culture)
                                : 0;
                            decimal valor = !string.IsNullOrEmpty(Request["precio_referenciaTable" + i])
                                ? Convert.ToDecimal(Request["precio_referenciaTable" + i], Culture)
                                : 0;
                            decimal descuento = !string.IsNullOrEmpty(Request["por_descuento_referenciaTable" + i])
                                ? Convert.ToDecimal(Request["por_descuento_referenciaTable" + i], Culture)
                                : 0;
                            decimal cantidad = !string.IsNullOrEmpty(Request["cantidad_referenciaTable" + i])
                                ? Convert.ToDecimal(Request["cantidad_referenciaTable" + i], Culture)
                                : 0;

                            context.rdetalleordencompra.Add(new rdetalleordencompra
                            {
                                idencaborden = consultarUltimaOrden.idorden,
                                codigo_referencia = refe,
                                cantidad = cantidad,
                                porcendescto = descuento,
                                porceniva = iva,
                                valorunitario = valor,
                                seq = secuencia
                            });
                            secuencia++;
                        }
                    }

                    int result = context.SaveChanges();
                    if (result > 0)
                    {
                        numeroConsecutivoAux.doccons_siguiente = numeroConsecutivoAux.doccons_siguiente + 1;
                        context.Entry(numeroConsecutivoAux).State = EntityState.Modified;
                        context.SaveChanges();
                        TempData["mensaje"] = "La orden se ha realizado exitosamente";
                        TempData["numOrdenCreada"] = numeroConsecutivo;
                        ListasDesplegables(modelo);
                        BuscarFavoritos(menu);
                        return RedirectToAction("Create");
                    }
                }
            }
            else
            {
                List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();
            }

            ListasDesplegables(modelo);
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult Editar(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            rordencompra orden = context.rordencompra.Find(id);
            if (orden == null)
            {
                return HttpNotFound();
            }

            ListasDesplegables(orden);
            ViewBag.bodegaSeleccionado = orden.bodega;
            ViewBag.destinatarioSeleccionado = orden.destinatario;
            ViewBag.numOC = orden.numero;
            ConsultaDatosCreacion(orden);
            BuscarFavoritos(menu);
            if (orden.descuentopie == null)
            {
                orden.descuentopie = 0;
            }

            return View(orden);
        }

        [HttpPost]
        public ActionResult Editar(rordencompra modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                string valorTotal = Request["txtValor"];
                string valorIvaTotal = Request["txtTotalIva"];
                string valorFletes = Request["txtFletes"];
                string descuentoPie = Request["txtDescuentopie"];
                modelo.valor = !string.IsNullOrEmpty(valorTotal) ? Convert.ToDecimal(valorTotal, Culture) : 0;
                modelo.iva = !string.IsNullOrEmpty(valorIvaTotal) ? Convert.ToDecimal(valorIvaTotal, Culture) : 0;
                modelo.fletes = !string.IsNullOrEmpty(valorFletes) ? Convert.ToDecimal(valorFletes, Culture) : 0;
                modelo.descuentopie = !string.IsNullOrEmpty(descuentoPie) ? Convert.ToDecimal(descuentoPie, Culture) : 0;
                modelo.fec_actualizacion = DateTime.Now;
                modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                context.Entry(modelo).State = EntityState.Modified;
                int guardarOrden = context.SaveChanges();

                if (guardarOrden > 0)
                {
                    //var consultarUltimaOrden = context.rordencompra.OrderByDescending(x => x.idorden).FirstOrDefault();
                    const string query = "DELETE FROM [dbo].[rdetalleordencompra] WHERE [idencaborden]={0}";
                    int rows = context.Database.ExecuteSqlCommand(query, modelo.idorden);

                    int lista_referencias = Convert.ToInt32(Request["lista_referencias"]);
                    int secuencia = 1;
                    for (int i = 0; i < lista_referencias; i++)
                    {
                        string refe = Request["id_referenciaTable" + i];
                        if (!string.IsNullOrEmpty(refe))
                        {
                            decimal iva = !string.IsNullOrEmpty(Request["por_iva_referenciaTable" + i])
                                ? Convert.ToDecimal(Request["por_iva_referenciaTable" + i], Culture)
                                : 0;
                            decimal valor = !string.IsNullOrEmpty(Request["precio_referenciaTable" + i])
                                ? Convert.ToDecimal(Request["precio_referenciaTable" + i], Culture)
                                : 0;
                            decimal descuento = !string.IsNullOrEmpty(Request["por_descuento_referenciaTable" + i])
                                ? Convert.ToDecimal(Request["por_descuento_referenciaTable" + i], Culture)
                                : 0;
                            decimal cantidad = !string.IsNullOrEmpty(Request["cantidad_referenciaTable" + i])
                                ? Convert.ToDecimal(Request["cantidad_referenciaTable" + i], Culture)
                                : 0;

                            context.rdetalleordencompra.Add(new rdetalleordencompra
                            {
                                idencaborden = modelo.idorden,
                                codigo_referencia = refe,
                                cantidad = cantidad,
                                porcendescto = descuento,
                                porceniva = iva,
                                valorunitario = valor,
                                seq = secuencia
                            });
                            secuencia++;
                        }
                    }

                    int result = context.SaveChanges();
                    if (result > 0)
                    {
                        TempData["mensaje"] = "La orden se ha actualizado exitosamente";
                    }
                }
            }
            else
            {
                List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();
            }

            ListasDesplegables(modelo);
            ViewBag.bodegaSeleccionado = modelo.bodega;
            ViewBag.destinatarioSeleccionado = modelo.destinatario;
            ViewBag.numOC = modelo.numero;
            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }

        public JsonResult BuscarDetallesOrden(int? id)
        {
            var buscarDetalles = (from detalles in context.rdetalleordencompra
                                  join referencia in context.icb_referencia
                                      on detalles.codigo_referencia equals referencia.ref_codigo
                                  where detalles.idencaborden == id
                                  select new
                                  {
                                      detalles.codigo_referencia,
                                      referencia.ref_descripcion,
                                      referencia.ref_codigo,
                                      detalles.cantidad,
                                      detalles.porceniva,
                                      detalles.porcendescto,
                                      detalles.valorunitario,
                                      valorIva = detalles.valorunitario * detalles.porceniva / 100 * detalles.cantidad,
                                      subtotal = detalles.valorunitario * detalles.cantidad -
                                                 detalles.valorunitario * detalles.porcendescto / 100 * detalles.cantidad +
                                                 detalles.valorunitario * detalles.porceniva / 100 * detalles.cantidad
                                  }).ToList();

            //var referencias = (from r in context.icb_referencia
            //            where r.modulo == "R"
            //            select new
            //            {
            //                codigo = r.ref_codigo,
            //            });

            return Json(buscarDetalles, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult Ver(int ? id, int ? menu) {

        //    var buscarOrden = (from encabezado in context.rordencompra
        //                         join tipoDocumento in context.tp_doc_registros
        //                         on encabezado.idtipodoc equals tipoDocumento.tpdoc_id
        //                         join direccion in context.terceros_direcciones
        //                         on encabezado.direccion equals direccion.id
        //                         join ciudad in context.nom_ciudad
        //                         on direccion.ciudad equals ciudad.ciu_id
        //                         join bodega in context.bodega_concesionario
        //                         on encabezado.bodega equals bodega.id
        //                         join tipoCompra in context.rtipocompra
        //                         on encabezado.tipoorden equals tipoCompra.id
        //                         join proveedor in context.icb_terceros
        //                         on encabezado.proveedor equals proveedor.tercero_id
        //                         join formaPago in context.fpago_tercero
        //                         on encabezado.condicion_pago equals formaPago.fpago_id
        //                         where encabezado.idorden == id
        //                         select new
        //                         {
        //                             encabezado.idorden,
        //                             encabezado.numero,
        //                             encabezado.diasvalidez,
        //                             encabezado.fec_creacion,
        //                             encabezado.descuentopie,
        //                             encabezado.iva,
        //                             encabezado.fletes,
        //                             encabezado.ivafletes,
        //                             encabezado.valor,
        //                             ciudad.ciu_nombre,
        //                             direccion.direccion,
        //                             encabezado.destinatario,
        //                             tipoDocumento.tpdoc_nombre,
        //                             tipoDocumento.prefijo,
        //                             bodega.bodccs_cod,
        //                             bodega.bodccs_nombre,
        //                             tipoCompra.descripcion,
        //                             nombre_proveedor = proveedor.prinom_tercero,
        //                             segNombre_proveedor = proveedor.segnom_tercero,
        //                             apellido_proveedor = proveedor.apellido_tercero,
        //                             segApellidoProveedor = proveedor.segapellido_tercero,
        //                             proveedor.razon_social,
        //                             formaPago.fpago_nombre
        //                         }).OrderByDescending(x => x.fec_creacion).FirstOrDefault();

        //    var buscarDetalles = (from detalles in  context.rdetalleordencompra
        //                          join referencia in context.icb_referencia
        //                          on detalles.codigo_referencia equals referencia.ref_codigo
        //                          where detalles.idencaborden == id
        //                          select new
        //                          {
        //                              detalles.codigo_referencia,
        //                              referencia.ref_descripcion,
        //                              detalles.cantidad,
        //                              detalles.porceniva,
        //                              detalles.porcendescto,
        //                              detalles.valorunitario
        //                          }).ToList();

        //    OrdenRepuestoModel modelo = new OrdenRepuestoModel();
        //    if (buscarOrden != null) {

        //        modelo.DiasValidez = buscarOrden.diasvalidez;
        //        modelo.Fecha = buscarOrden.fec_creacion.ToShortDateString();
        //        modelo.Bodega = "(" + buscarOrden.bodccs_cod + ") " + buscarOrden.bodccs_nombre;
        //        modelo.TipoCompra = buscarOrden.descripcion;
        //        modelo.Proveedor = buscarOrden.nombre_proveedor + " " + buscarOrden.segNombre_proveedor + " " + buscarOrden.apellido_proveedor + " " + buscarOrden.segApellidoProveedor + " " + buscarOrden.razon_social;
        //        modelo.FormaPago = buscarOrden.fpago_nombre;
        //        modelo.Descuento = buscarOrden.descuentopie??0;
        //        modelo.Iva = buscarOrden.iva??0;
        //        modelo.Fletes = buscarOrden.fletes??0;
        //        modelo.IvaFletes = buscarOrden.ivafletes??0;
        //        modelo.Total = buscarOrden.valor??0;
        //    }

        //    foreach (var item in buscarDetalles) {
        //        modelo.ListaDetalles.Add(new DetalleRepuestoOrdenModel() {
        //            CodigoReferencia = item.codigo_referencia,
        //            NombreReferencia = item.ref_descripcion,
        //            DescuentoReferencia= item.porcendescto??0,
        //            IvaReferencia = item.porceniva??0,
        //            PrecioReferencia = item.valorunitario??0,
        //            CantidadReferencia = item.cantidad
        //        });
        //    }
        //    BuscarFavoritos(menu);
        //    return View(modelo);
        //}

        public void ConsultaDatosCreacion(rordencompra orden)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(orden.userid_creacion);
            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            users modificator = context.users.Find(orden.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult BuscarOrdenesCompra()
        {
            var buscarOrdenes = (from encabezado in context.rordencompra
                                 join tipoDocumento in context.tp_doc_registros
                                     on encabezado.idtipodoc equals tipoDocumento.tpdoc_id
                                 join bodega in context.bodega_concesionario
                                     on encabezado.bodega equals bodega.id
                                 join tp in context.rtipocompra
                                     on encabezado.tipoorden equals tp.id
                                 join pro in  context.icb_terceros 
                                    on encabezado.proveedor equals pro.tercero_id

                                 select new
                                 {
                                     encabezado.idorden,
                                     encabezado.numero,
                                     tipoDocumento.tpdoc_nombre,
                                     tipoDocumento.prefijo,
                                     bodega.bodccs_cod,
                                     bodega.bodccs_nombre,
                                     encabezado.fec_creacion,
                                     tipo_compra = tp.descripcion,
                                     encabezado.valor,
                                     proveedor = pro.doc_tercero != null  ? pro.doc_tercero + " - "+pro.prinom_tercero+" "+pro.segnom_tercero+" "+pro.apellido_tercero+" "+pro.segapellido_tercero: " "
                                 }).OrderByDescending(x => x.fec_creacion).ToList();

            var ordenes = buscarOrdenes.Select(x => new
            {
                x.idorden,
                x.numero,
                x.tpdoc_nombre,
                x.prefijo,
                x.bodccs_cod,
                x.tipo_compra,
                valor = x.valor != null ? x.valor.Value.ToString("0,0", elGR) : "0",
                x.proveedor,
                x.bodccs_nombre,
                fec_creacion = x.fec_creacion.ToShortDateString() + " " + x.fec_creacion.ToShortTimeString()
            }).ToList();

            return Json(ordenes, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarCedulaProveedor(int id_tercero, int? id_orden)
        {
            var buscarDestinatario = context.icb_terceros.Select(x => new
            {
                x.tercero_id,
                x.doc_tercero,
                x.prinom_tercero,
                x.segnom_tercero,
                x.apellido_tercero,
                x.segapellido_tercero,
                x.celular_tercero,
                x.email_tercero,
                direcciones = (from direccions in context.terceros_direcciones
                               join ciudad in context.nom_ciudad
                                   on direccions.ciudad equals ciudad.ciu_id
                               where direccions.idtercero == x.tercero_id
                               select new
                               {
                                   direccions.id,
                                   direccions.direccion,
                                   ciudad.ciu_nombre
                               }).ToList()
            }).FirstOrDefault(x => x.tercero_id == id_tercero);

            var buscarDireccion = (from orden in context.rordencompra
                                   where orden.idorden == id_orden
                                   select new
                                   {
                                       id_direccion = orden.direccion != null ? orden.direccion.ToString() : ""
                                   }).FirstOrDefault();

            var buscarFormaPago = (from c in context.tercero_proveedor
                                   join f in context.fpago_tercero
                                       on c.fpago_id equals f.fpago_id
                                   where c.tercero_id == id_tercero
                                   select new
                                   {
                                       id = f.fpago_id,
                                       fpago = f.fpago_nombre
                                   }).FirstOrDefault();

            if (buscarDestinatario != null)
            {
                return Json(new { encontrado = true, buscarDestinatario, buscarDireccion, buscarFormaPago },
                    JsonRequestBehavior.AllowGet);
            }

            return Json(new { encontrado = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarDatosReferencia(string idReferencia)
        {
            float buscar = context.icb_referencia.Where(x => x.ref_codigo == idReferencia).Select(x => x.por_iva_compra)
                .FirstOrDefault();

            return Json(buscar, JsonRequestBehavior.AllowGet);
        }

        private static int enviarcorreo(int id, byte[] pdf, string nombre, string correo)
        {
            Iceberg_Context context2 = new Iceberg_Context();
            int exito = 0;
            //objeto
            configuracion_envio_correos correoconfig = context2.configuracion_envio_correos.Where(d => d.activo).FirstOrDefault();

            rordencompra ordenEnviar = context2.rordencompra.Where(a => a.idorden == id).FirstOrDefault();
            if (ordenEnviar != null)
            {
                string emailtercero = ordenEnviar.icb_terceros.email_tercero;
                int numeroOC = ordenEnviar.numero;
                int aniocorreo = DateTime.Now.Year;
                //formateo del correo (convertir en parámetros del sistema)
                string titulocorreo = "Orden de compra número: " + numeroOC;
                string asuntocorreo = "Su orden de compra ha sido registrada exitosamente";
                string textoinicial =
                    "A continuación le informamos que se ha aprobado la adquisición de sus productos y/o Servicios, por lo que hacemos llegar Orden de Compra  N° " +
                    numeroOC + ", el detalle se encuentra en documento adjunto al final de este correo.";
                string subject = "Envio de la orden de compra número: " + numeroOC;

                string html2 =
                    @"<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
                <html xmlns='http://www.w3.org/1999/xhtml'>
                <head>
                    <meta http-equiv='Content-Type' content='text/html; charset=utf-8' />
                    <title>Notificacion Iceberg Email</title>
                    <style type='text/css'>
                        body {margin: 0; padding: 0; min-width: 100%!important;}
						img {height: auto;}
						.content {width: 100%; max-width: 600px;}
						.header {padding: 40px 30px 20px 30px;}
						.innerpadding {padding: 30px 30px 30px 30px;}
						.borderbottom {border-bottom: 1px solid #f2eeed;}
						.subhead {font-size: 15px; color: #ffffff; font-family: sans-serif; letter-spacing: 10px;}
						.h1, .h2, .bodycopy {color: #153643; font-family: sans-serif;}
						.h1 {font-size: 33px; line-height: 38px; font-weight: bold;}
						.h2 {padding: 0 0 15px 0; font-size: 24px; line-height: 28px; font-weight: bold;}
						.bodycopy {font-size: 16px; line-height: 22px;}
						.button {text-align: center; font-size: 18px; font-family: sans-serif; font-weight: bold; padding: 0 30px 0 30px;}
						.button a {color: #ffffff; text-decoration: none;}
						.footer {padding: 20px 30px 15px 30px;}
						.footercopy {font-family: sans-serif; font-size: 14px; color: #ffffff;}
						.footercopy a {color: #ffffff; text-decoration: underline;}
						.unsubscribe {display: block; margin-top: 20px; padding: 10px 50px; background: #2f3942; border-radius: 5px; text-decoration: none!important; font-weight: bold;}
						.hide {display: none!important;}
						.imagenmail{
						       width: 100%;
						       max-width: 600px;
						      }
						@media only screen and (max-width: 550px), screen and (max-device-width: 550px) {
							.imagenmail {
							  width: 100%;
							  max-width: 530px;
							}
							body[yahoo] .hide {display: none!important;}
							body[yahoo] .buttonwrapper {background-color: transparent!important;}
							body[yahoo] .button {padding: 0px!important;}
							body[yahoo] .button a {background-color: #e05443; padding: 15px 15px 13px!important;}
							body[yahoo] .unsubscribe {display: block; margin-top: 20px; padding: 10px 50px; background: #2f3942; border-radius: 5px; text-decoration: none!important; font-weight: bold;}
						}
                    </style>
                </head>
				<body yahoo=yahoo bgcolor='#ffffff'>
					<table width='100%' bgcolor='#ffffff' border='0' cellpadding='0' cellspacing='0'>
						<tr>
							<td>
								<!--[if (gte mso 9)|(IE)]>
								<table width='600' align='center' cellpadding='0' cellspacing='0' border='0'>
									<tr>
										<td>
                        					<![endif]-->

                        					<table class='content' align='center' cellpadding='0' cellspacing='0' border='0'>
                        						<tr>
                        							<td class='header' bgcolor='#ffffff' >
                        								<img class='imagenmail' src='cid:logoiceberg' /><br>
                        								<table width='70' align='left' border='0' cellpadding='0' cellspacing='0'>
                        									<tr>
                        										<td height='auto' style='padding: 0 20px 20px 0;'>

                        										</td>
                        									</tr>
                        								</table>

											<!--[if (gte mso 9)|(IE)]>
						  <table width='425' align='left' cellpadding='0' cellspacing='0' border='0'>
							<tr>
							  <td>
              					<![endif]-->
              					<table class='col425' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%; max-width: 425px;'>
              						<tr>
              							<td height='70'>
              								<table width='100%' border='0' cellspacing='0' cellpadding='0'>

              								</table>
              							</td>
              						</tr>
              					</table>
											<!--[if (gte mso 9)|(IE)]>
							  </td>
							</tr>
						</table>
						<![endif]-->

					</td>
				</tr>
				<tr>
					<td class='innerpadding borderbottom'>
						<table width='100%' border='0' cellspacing='0' cellpadding='0'>
							<tr>
								<td class='h2'>" + titulocorreo + @"
								</td>
							</tr>
							<tr>
								<td class='bodycopy'>
									Saludos! Sr(a) " + ordenEnviar.icb_terceros.prinom_tercero + " " +
                    ordenEnviar.icb_terceros.apellido_tercero + @"
								</td>
							</tr>
						</table>
					</td>
				</tr>

				<tr>
					<td class='innerpadding borderbottom'>
						<table width='115' align='left' border='0' cellpadding='0' cellspacing='0'>

						</table>
											<!--[if (gte mso 9)|(IE)]>
							  <table width='380' align='left' cellpadding='0' cellspacing='0' border='0'>
								<tr>
								  <td>
                  					<![endif]-->
                  					<table class='col380' align='left' border='0' cellpadding='0' cellspacing='0' style='width: 100%; max-width: 380px;'>
                  						<tr>
                  							<td>
                  								<table width='100%' border='0' cellspacing='0' cellpadding='0'>
                  									<tr>
                  										<td class='bodycopy'>
                  											<p>" + asuntocorreo + @"</p>
                  											<p>" + textoinicial + @"</p>
                  										</td>
                  									</tr>
                  								</table>
                  							</td>
                  						</tr>
                  					</table>
											<!--[if (gte mso 9)|(IE)]>
								  </td>
								</tr>
							</table>
							<![endif]-->
						</td>
					</tr>

					<tr>
    					<td class='footer' bgcolor='#09599C'>
    						<table width='100%' border='0' cellspacing='0' cellpadding='0'>
    							<tr>
    								<td align='center' class='footercopy'>
    									&reg; ICEBERG, " + aniocorreo + @"<br />                                       
    								</td>
    							</tr>                                
    						</table>
    					</td>
					</tr>
				</table>
								<!--[if (gte mso 9)|(IE)]>
										</td>
									</tr>
							</table>
							<![endif]-->

						</td>
					</tr>
				</table>
			</body>
	</html>
";

                //create an instance of new mail message
                MailAddress de = new MailAddress(correoconfig.correo, "Notificación Iceberg");
                //MailAddress para = new MailAddress("erion23@gmail.com", "Paulo Romero");
                MailAddress para = new MailAddress(ordenEnviar.icb_terceros.email_tercero,
                    ordenEnviar.icb_terceros.prinom_tercero + " " + ordenEnviar.icb_terceros.apellido_tercero);
                MailAddress replicar = new MailAddress(ordenEnviar.users1.user_email,
                    ordenEnviar.users1.user_nombre + " " + ordenEnviar.users1.user_apellido);
                MailMessage mensaje = new MailMessage(de, para);
                mensaje.Bcc.Add("correospruebaexi2019@gmail.com");

                mensaje.BodyEncoding = Encoding.Default;
                //mensaje.CC.Add()
                //set the HTML format to true
                mensaje.IsBodyHtml = true;

                LinkedResource theEmailImage6 =
                    new LinkedResource(HostingEnvironment.MapPath("~/Images/icebergerp03.png"), "image/png")
                    {
                        ContentId = "logoiceberg",
                        TransferEncoding = TransferEncoding.Base64
                    };
                theEmailImage6.ContentType.Name = theEmailImage6.ContentId;
                theEmailImage6.ContentLink = new Uri("cid:" + theEmailImage6.ContentId);

                //create Alrternative HTML view
                //string body = string.Format(html2, theEmailImage.ContentId, theEmailImage2.ContentId, theEmailImage3.ContentId, theEmailImage4.ContentId);
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(html2, null, "text/html");

                Attachment att = new Attachment(new MemoryStream(pdf), nombre);
                mensaje.Attachments.Add(att);
                mensaje.AlternateViews.Add(htmlView);
                //mensaje.Body = html2;
                //set the Email subject
                mensaje.Subject = subject;

                SmtpClient cliente = new SmtpClient(correoconfig.smtp_server)
                {
                    Port = correoconfig.puerto,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(correoconfig.usuario, correoconfig.password),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };
                cliente.Send(mensaje);
            }

            return exito;
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