using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class DevolucionComprasRepuestosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");

        // GET: DevolucionComprasRepuestos
        public ActionResult Index(int? menu)
        {
            ViewBag.doc_registros = context.tp_doc_registros.Where(x => x.tipo == 14);
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult BrowserDevoluciones(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public void listas(int id)
        {
            ViewBag.doc_registros = context.tp_doc_registros.Where(x => x.tipo == 14);
            encab_documento encab = context.encab_documento.Find(id);

            ViewBag.idDevolucion = encab.idencabezado;

            var proveedores = (from pro in context.tercero_proveedor
                               join ter in context.icb_terceros
                                   on pro.tercero_id equals ter.tercero_id
                               where pro.tercero_id == encab.nit
                               select new
                               {
                                   idTercero = ter.tercero_id,
                                   nombreTErcero = ter.prinom_tercero,
                                   apellidosTercero = ter.apellido_tercero,
                                   razonSocial = ter.razon_social,
                                   ter.doc_tercero
                               }).FirstOrDefault();
            int idProveedor = proveedores.idTercero;
            ViewBag.idProveedor = idProveedor;
            ViewBag.proveedor = proveedores.doc_tercero + " - " + proveedores.nombreTErcero + " " +
                                proveedores.apellidosTercero + " " + proveedores.razonSocial;

            var infoCompra = (from e in context.encab_documento
                              join m in context.monedas
                                  on e.moneda equals m.moneda
                              join b in context.bodega_concesionario
                                  on e.bodega equals b.id
                              join t in context.tp_doc_registros
                                  on e.tipo equals t.tpdoc_id
                              where e.idencabezado == encab.idencabezado
                              select new
                              {
                                  e.fletes,
                                  e.iva_fletes,
                                  e.moneda,
                                  m.descripcion,
                                  e.tasa,
                                  bodega = b.bodccs_nombre,
                                  idTipo = e.tipo,
                                  tipoDes = t.prefijo + " - " + t.tpdoc_nombre
                              }).FirstOrDefault();

            ViewBag.tasa = infoCompra.tasa != null ? infoCompra.tasa : 0;
            ViewBag.moneda = infoCompra.moneda;
            ViewBag.monedaDes = infoCompra.descripcion;
            ViewBag.bodega = infoCompra.bodega;
            ViewBag.tipoID = infoCompra.idTipo;
            ViewBag.tipoDes = infoCompra.tipoDes;
            ViewBag.fletes = infoCompra.fletes;
            ViewBag.ivaFletes = infoCompra.iva_fletes;
            ViewBag.nit = encab.nit;
            ViewBag.numero = encab.numero;
            ViewBag.valor_total = encab.valor_total;
            ViewBag.fecha = encab.fec_creacion;
        }

        public ActionResult DetalleDevoluciones(int id, int? menu)
        {
            lineas_documento lineas = context.lineas_documento.FirstOrDefault(x => x.id_encabezado == id);
            ViewBag.numero = lineas.encab_documento.numero;
            ViewBag.valor_total = lineas.encab_documento.valor_total;
            ViewBag.fecha = lineas.encab_documento.fecha;
            ViewBag.facid = lineas.encab_documento.idencabezado;
            ViewBag.idEncabezado = id;
            ViewBag.factura = context.encab_documento.Where(x => x.idencabezado == id).Select(x => x.numero)
                .FirstOrDefault();
            ViewBag.fecha = context.encab_documento.Where(x => x.idencabezado == id).Select(x => x.fec_creacion)
                .FirstOrDefault();
            ViewBag.fechaVencimiento = context.encab_documento.Where(x => x.idencabezado == id)
                .Select(x => x.vencimiento).FirstOrDefault();
            ViewBag.tipoDocumento = (from a in context.encab_documento
                                     join b in context.tp_doc_registros
                                         on a.tipo equals b.tpdoc_id
                                     where a.idencabezado == id
                                     select b.tpdoc_nombre).FirstOrDefault();

            ViewBag.bodega = (from a in context.encab_documento
                              join b in context.bodega_concesionario
                                  on a.bodega equals b.id
                              where a.idencabezado == id
                              select b.bodccs_nombre).FirstOrDefault();

            var proveedor = (from a in context.encab_documento
                             join b in context.icb_terceros
                                 on a.nit equals b.tercero_id
                             where a.idencabezado == id
                             select new
                             {
                                 b.prinom_tercero,
                                 b.segnom_tercero,
                                 b.apellido_tercero,
                                 b.segapellido_tercero,
                                 b.razon_social
                             }).FirstOrDefault();
            ViewBag.proveedor = proveedor.razon_social + " " + proveedor.prinom_tercero + " " +
                                proveedor.segnom_tercero + " " + proveedor.apellido_tercero + " " +
                                proveedor.segapellido_tercero;

            ViewBag.tipoPago = (from c in context.encab_documento
                                join f in context.fpago_tercero
                                    on c.fpago_id equals f.fpago_id
                                where c.idencabezado == id
                                select f.fpago_nombre).FirstOrDefault();

            ViewBag.moneda = (from c in context.encab_documento
                              join f in context.monedas
                                  on c.moneda equals f.moneda
                              where c.idencabezado == id
                              select f.descripcion).FirstOrDefault();

            ViewBag.perfil = (from a in context.encab_documento
                              join b in context.perfil_contable_documento
                                  on a.perfilcontable equals b.id
                              where a.idencabezado == id
                              select b.descripcion).FirstOrDefault();

            ViewBag.fletes = (from a in context.encab_documento
                              where a.idencabezado == id
                              select a.fletes).FirstOrDefault();

            ViewBag.ivafletes = (from a in context.encab_documento
                                 where a.idencabezado == id
                                 select a.iva_fletes).FirstOrDefault();

            var asesor = (from a in context.encab_documento
                          join b in context.users
                              on a.solicitadopor equals b.user_id
                          where a.idencabezado == id
                          select new
                          {
                              b.user_nombre,
                              b.user_apellido,
                              a.solicitadopor
                          }).FirstOrDefault();

            ViewBag.asesor = asesor != null ? asesor.user_nombre + " " + asesor.user_apellido : "";

            ViewBag.concepto1 = (from a in context.encab_documento
                                 join b in context.tpdocconceptos
                                     on a.concepto equals b.id
                                 where a.idencabezado == id
                                 select b.Descripcion).FirstOrDefault();

            ViewBag.concepto2 = (from a in context.encab_documento
                                 join b in context.tpdocconceptos2
                                     on a.concepto equals b.id
                                 where a.idencabezado == id
                                 select b.Descripcion).FirstOrDefault();

            ViewBag.observaciones = (from a in context.encab_documento
                                     where a.idencabezado == id
                                     select a.notas != null ? a.notas : "").FirstOrDefault();

            ViewBag.pedido = (from a in context.encab_documento
                              join b in context.icb_referencia_mov
                                  on a.pedido equals b.refmov_id
                              where a.idencabezado == id
                              select b.refmov_numero).FirstOrDefault();


            List<lineas_documento> datos = context.lineas_documento.Where(x => x.id_encabezado == id).ToList();
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult DetalleComprasRespuestos(int id, int? menu)
        {
            lineas_documento lineas = context.lineas_documento.FirstOrDefault(x => x.id_encabezado == id);
            ViewBag.numero = lineas.encab_documento.numero;
            ViewBag.valor_total = lineas.encab_documento.valor_total;
            ViewBag.fecha = lineas.encab_documento.fecha;
            ViewBag.facid = lineas.encab_documento.idencabezado;
            ViewBag.idEncabezado = id;
            ViewBag.factura = context.encab_documento.Where(x => x.idencabezado == id).Select(x => x.numero)
                .FirstOrDefault();
            ViewBag.fecha = context.encab_documento.Where(x => x.idencabezado == id).Select(x => x.fec_creacion)
                .FirstOrDefault();
            ViewBag.fechaVencimiento = context.encab_documento.Where(x => x.idencabezado == id)
                .Select(x => x.vencimiento).FirstOrDefault();
            ViewBag.tipoDocumento = (from a in context.encab_documento
                                     join b in context.tp_doc_registros
                                         on a.tipo equals b.tpdoc_id
                                     where a.idencabezado == id
                                     select b.tpdoc_nombre).FirstOrDefault();

            ViewBag.bodega = (from a in context.encab_documento
                              join b in context.bodega_concesionario
                                  on a.bodega equals b.id
                              where a.idencabezado == id
                              select b.bodccs_nombre).FirstOrDefault();

            var proveedor = (from a in context.encab_documento
                             join b in context.icb_terceros
                                 on a.nit equals b.tercero_id
                             where a.idencabezado == id
                             select new
                             {
                                 b.prinom_tercero,
                                 b.segnom_tercero,
                                 b.apellido_tercero,
                                 b.segapellido_tercero,
                                 b.razon_social
                             }).FirstOrDefault();
            ViewBag.proveedor = proveedor.razon_social + " " + proveedor.prinom_tercero + " " +
                                proveedor.segnom_tercero + " " + proveedor.apellido_tercero + " " +
                                proveedor.segapellido_tercero;

            ViewBag.tipoPago = (from c in context.encab_documento
                                join f in context.fpago_tercero
                                    on c.fpago_id equals f.fpago_id
                                where c.idencabezado == id
                                select f.fpago_nombre).FirstOrDefault();

            ViewBag.moneda = (from c in context.encab_documento
                              join f in context.monedas
                                  on c.moneda equals f.moneda
                              where c.idencabezado == id
                              select f.descripcion).FirstOrDefault();

            ViewBag.perfil = (from a in context.encab_documento
                              join b in context.perfil_contable_documento
                                  on a.perfilcontable equals b.id
                              where a.idencabezado == id
                              select b.descripcion).FirstOrDefault();

            ViewBag.fletes = (from a in context.encab_documento
                              where a.idencabezado == id
                              select a.fletes).FirstOrDefault();

            ViewBag.ivafletes = (from a in context.encab_documento
                                 where a.idencabezado == id
                                 select a.iva_fletes).FirstOrDefault();

            var asesor = (from a in context.encab_documento
                          join b in context.users
                              on a.solicitadopor equals b.user_id
                          where a.idencabezado == id
                          select new
                          {
                              b.user_nombre,
                              b.user_apellido,
                              a.solicitadopor
                          }).FirstOrDefault();

            ViewBag.asesor = asesor != null ? asesor.user_nombre + " " + asesor.user_apellido : "";

            ViewBag.concepto1 = (from a in context.encab_documento
                                 join b in context.tpdocconceptos
                                     on a.concepto equals b.id
                                 where a.idencabezado == id
                                 select b.Descripcion).FirstOrDefault();

            ViewBag.concepto2 = (from a in context.encab_documento
                                 join b in context.tpdocconceptos2
                                     on a.concepto equals b.id
                                 where a.idencabezado == id
                                 select b.Descripcion).FirstOrDefault();

            ViewBag.observaciones = (from a in context.encab_documento
                                     where a.idencabezado == id
                                     select a.notas != null ? a.notas : "").FirstOrDefault();

            ViewBag.pedido = (from a in context.encab_documento
                              join b in context.icb_referencia_mov
                                  on a.pedido equals b.refmov_id
                              where a.idencabezado == id
                              select b.refmov_numero).FirstOrDefault();

            ViewBag.facturaProveedor = (from a in context.encab_documento
                                        where a.idencabezado == id
                                        select a.documento).FirstOrDefault();

            List<lineas_documento> datos = context.lineas_documento.Where(x => x.id_encabezado == id).ToList();
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult DevolucionManual(int id, int? menu)
        {
            listas(id);
            //se cargan los datos del mismo numero que aun no se han comprado
            List<lineas_documento> datos = context.lineas_documento.Where(x => x.id_encabezado == id).ToList();
            ViewBag.datos = datos;
            ViewBag.facturaProveedor = context.encab_documento.FirstOrDefault(x => x.idencabezado == id).documento;
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult DevolucionManual(NotasContablesModel modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                {
                    try
                    {
                        int funciono = 0;
                        decimal totalCreditos = 0;
                        decimal totalDebitos = 0;
                        int perfilcontable = Convert.ToInt32(Request["perfil"]);
                        int tipo = Convert.ToInt32(Request["selectTipoDocumento"]);
                        int bodega = Convert.ToInt32(Request["idBodega"]);
                        int nit = Convert.ToInt32(Request["nit"]);
                        int idDevol = Convert.ToInt32(Request["idDevol"]);
                        decimal fletes = Convert.ToDecimal(Request["fletes"], miCultura);
                        decimal ivaFletes = Convert.ToDecimal(Request["iva_fletes"], miCultura);

                        //variable que contiene la informacion de la comra que estoy devolviendo
                        encab_documento compraDevol = context.encab_documento.FirstOrDefault(x => x.idencabezado == idDevol);

                        var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                                          join nombreParametro in context.paramcontablenombres
                                                              on perfil.id_nombre_parametro equals nombreParametro.id
                                                          join cuenta in context.cuenta_puc
                                                              on perfil.cuenta equals cuenta.cntpuc_id
                                                          where perfil.id_perfil == perfilcontable
                                                          select new
                                                          {
                                                              perfil.id,
                                                              perfil.id_nombre_parametro,
                                                              perfil.cuenta,
                                                              perfil.centro,
                                                              perfil.id_perfil,
                                                              nombreParametro.descripcion_parametro,
                                                              cuenta.cntpuc_numero
                                                          }).ToList();

                        int secuencia = 1;
                        //decimal totalDebitos = 0;
                        //decimal totalCreditos = 0;


                        List<cuentas_valores> ids_cuentas_valores = new List<cuentas_valores>();
                        centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                        int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;

                        List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();

                        string lista = Request["lista"];
                        if (!string.IsNullOrEmpty(lista))
                        {
                            int datos = Convert.ToInt32(lista);
                            decimal costoTotal =
                                Convert.ToDecimal(Request["valor_proveedor"], miCultura); //costo con retenciones y fletes
                            decimal ivaEncabezado = Convert.ToDecimal(Request["valorIVA"], miCultura); //valor total del iva
                            decimal descuentoEncabezado =
                                Convert.ToDecimal(Request["valorDes"], miCultura); //valor total del descuento
                            decimal costoEncabezado = Convert.ToDecimal(Request["valorSub"], miCultura); //valor antes de impuestos

                            decimal valor_totalenca = costoEncabezado - descuentoEncabezado;

                            //consecutivo
                            grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                                x.documento_id == tipo && x.bodega_id == bodega);
                            if (grupo != null)
                            {
                                DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                                long consecutivo = doc.BuscarConsecutivo(grupo.grupo);

                                //Encabezado documento

                                #region encabezado

                                encab_documento encabezado = new encab_documento
                                {
                                    tipo = tipo,
                                    numero = consecutivo,
                                    nit = nit,
                                    fecha = DateTime.Now
                                };
                                int? condicion = compraDevol.fpago_id;
                                encabezado.fpago_id = condicion;
                                int dias = context.fpago_tercero.Find(condicion).dvencimiento ?? 0;
                                DateTime vencimiento = DateTime.Now.AddDays(dias);
                                encabezado.vencimiento = vencimiento;
                                encabezado.valor_total = costoTotal;
                                encabezado.iva = ivaEncabezado;
                                encabezado.notas = Request["notaDevolucion"];
                                // Validacion para reteIVA, reteICA y retencion dependiendo del proveedor seleccionado
                                tp_doc_registros buscarTipoDocRegistro =
                                    context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == tipo);
                                tercero_proveedor buscarProveedor =
                                    context.tercero_proveedor.FirstOrDefault(x => x.tercero_id == nit);
                                int regimen_proveedor = buscarProveedor != null ? buscarProveedor.tpregimen_id : 0;
                                perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                                    x.bodega == bodega && x.sw == buscarTipoDocRegistro.sw &&
                                    x.tipo_regimenid == regimen_proveedor);
                                decimal costoModelo = Convert.ToDecimal(costoEncabezado, miCultura);
                                decimal retenciones = 0;

                                if (buscarPerfilTributario != null)
                                {
                                    if (buscarPerfilTributario.retfuente == "A" &&
                                        valor_totalenca >= buscarTipoDocRegistro.baseretencion)
                                    {
                                        encabezado.porcen_retencion = buscarTipoDocRegistro.retencion;
                                        encabezado.retencion =
                                            Math.Round(valor_totalenca *
                                                       Convert.ToDecimal(buscarTipoDocRegistro.retencion / 100, miCultura));
                                        retenciones += encabezado.retencion;
                                    }

                                    if (buscarPerfilTributario.retiva == "A" &&
                                        ivaEncabezado >= buscarTipoDocRegistro.baseiva)
                                    {
                                        encabezado.porcen_reteiva = buscarTipoDocRegistro.retiva;
                                        encabezado.retencion_iva =
                                            Math.Round(encabezado.iva *
                                                       Convert.ToDecimal(buscarTipoDocRegistro.retiva / 100, miCultura));
                                        retenciones += encabezado.retencion_iva;
                                    }

                                    if (buscarPerfilTributario.retica == "A" &&
                                        valor_totalenca >= buscarTipoDocRegistro.baseica)
                                    {
                                        terceros_bod_ica bodega_acteco = context.terceros_bod_ica.FirstOrDefault(x =>
                                            x.idcodica == buscarProveedor.acteco_id && x.bodega == bodega);
                                        decimal tercero_acteco = buscarProveedor.acteco_tercero.tarifa;
                                        if (bodega_acteco != null)
                                        {
                                            encabezado.porcen_retica = (float)bodega_acteco.porcentaje;
                                            encabezado.retencion_ica =
                                                Math.Round(valor_totalenca *
                                                           Convert.ToDecimal(bodega_acteco.porcentaje / 1000, miCultura));
                                            retenciones += encabezado.retencion_ica;
                                        }

                                        if (tercero_acteco != 0)
                                        {
                                            encabezado.porcen_retica = (float)buscarProveedor.acteco_tercero.tarifa;
                                            encabezado.retencion_ica =
                                                Math.Round(valor_totalenca *
                                                           Convert.ToDecimal(
                                                               buscarProveedor.acteco_tercero.tarifa / 1000, miCultura));
                                            retenciones += encabezado.retencion_ica;
                                        }
                                        else
                                        {
                                            encabezado.porcen_retica = buscarTipoDocRegistro.retica;
                                            encabezado.retencion_ica =
                                                Math.Round(valor_totalenca *
                                                           Convert.ToDecimal(buscarTipoDocRegistro.retica / 1000, miCultura));
                                            retenciones += encabezado.retencion_ica;
                                        }
                                    }
                                }

                                if (modelo.fletes != null)
                                {
                                    encabezado.fletes = Convert.ToDecimal(modelo.fletes, miCultura);
                                    encabezado.iva_fletes = Convert.ToDecimal(modelo.iva_fletes, miCultura);
                                }

                                encabezado.costo = valor_totalenca;
                                encabezado.vendedor = compraDevol.vendedor; //mismo de la compra
                                encabezado.documento = Convert.ToString(compraDevol.numero); //consecutivo de la compra
                                encabezado.bodega = bodega;
                                encabezado.concepto = modelo.concepto;
                                encabezado.perfilcontable = perfilcontable;
                                encabezado.solicitadopor = compraDevol.solicitadopor;
                                encabezado.moneda = compraDevol.moneda;
                                encabezado.tasa = Convert.ToInt32(Request["tasa"]);
                                encabezado.valor_mercancia = valor_totalenca;
                                encabezado.fec_creacion = DateTime.Now;
                                encabezado.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                encabezado.estado = true;
                                encabezado.concepto2 = modelo.concepto2;

                                context.encab_documento.Add(encabezado);
                                context.SaveChanges();

                                #endregion

                                int id_encabezado = context.encab_documento.OrderByDescending(x => x.idencabezado)
                                    .FirstOrDefault().idencabezado;

                                //Cruce documentos

                                #region cruce documentos

                                cruce_documentos cruce = new cruce_documentos
                                {
                                    idtipo = tipo,
                                    numero = consecutivo,
                                    idtipoaplica = compraDevol.tipo,
                                    numeroaplica = compraDevol.numero,
                                    valor = compraDevol.valor_total,
                                    fecha = DateTime.Now,
                                    fechacruce = DateTime.Now,
                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                                };

                                context.cruce_documentos.Add(cruce);

                                #endregion

                                //actualizar el encabezado de la compra

                                #region actualizar la compra devuelta

                                compraDevol.documento = Convert.ToString(consecutivo);
                                compraDevol.valor_aplicado += costoTotal;
                                compraDevol.fec_actualizacion = DateTime.Now;
                                context.Entry(compraDevol).State = EntityState.Modified;

                                #endregion

                                //Mov Contable

                                #region movimientos contables

                                //buscamos en perfil cuenta documento, por medio del perfil seleccionado

                                foreach (var parametro in parametrosCuentasVerificar)
                                {
                                    string descripcionParametro = context.paramcontablenombres
                                        .FirstOrDefault(x => x.id == parametro.id_nombre_parametro)
                                        .descripcion_parametro;
                                    cuenta_puc buscarCuenta =
                                        context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);

                                    if (buscarCuenta != null)
                                    {
                                        if (parametro.id_nombre_parametro == 1 &&
                                            Convert.ToDecimal(valor_totalenca, miCultura) != 0
                                            || parametro.id_nombre_parametro == 3 &&
                                            Convert.ToDecimal(encabezado.retencion, miCultura) != 0
                                            || parametro.id_nombre_parametro == 4 &&
                                            Convert.ToDecimal(encabezado.retencion_iva, miCultura) != 0
                                            || parametro.id_nombre_parametro == 5 &&
                                            Convert.ToDecimal(encabezado.retencion_ica, miCultura) != 0
                                            || parametro.id_nombre_parametro == 6 &&
                                            Convert.ToDecimal(encabezado.fletes, miCultura) != 0
                                            || parametro.id_nombre_parametro == 14 &&
                                            Convert.ToDecimal(encabezado.iva_fletes, miCultura) != 0)
                                        {
                                            mov_contable movNuevo = new mov_contable
                                            {
                                                id_encab = encabezado.idencabezado,
                                                seq = secuencia,
                                                idparametronombre = parametro.id_nombre_parametro,
                                                cuenta = parametro.cuenta,
                                                centro = parametro.centro,
                                                fec = DateTime.Now,
                                                fec_creacion = DateTime.Now,
                                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                                documento = Convert.ToString(compraDevol.numero),
                                                detalle =
                                                "Devolucion manual de la compra con consecutivo " + compraDevol.numero,
                                                estado = true
                                            };

                                            cuenta_puc info = context.cuenta_puc.Where(a => a.cntpuc_id == parametro.cuenta)
                                                .FirstOrDefault();

                                            if (info.tercero)
                                            {
                                                movNuevo.nit = nit;
                                            }
                                            else
                                            {
                                                icb_terceros tercero = context.icb_terceros.Where(t => t.doc_tercero == "0")
                                                    .FirstOrDefault();
                                                movNuevo.nit = tercero.tercero_id;
                                            }

                                            // las siguientes validaciones se hacen dependiendo de la cuenta que esta moviendo la compra manual, para guardar la informacion acorde

                                            #region Cuentas X Pagar

                                            if (parametro.id_nombre_parametro == 1)
                                            {
                                                /*if (info.aplicaniff==true)
												{

												}*/

                                                if (info.manejabase)
                                                {
                                                    movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                                }
                                                else
                                                {
                                                    movNuevo.basecontable = 0;
                                                }

                                                if (info.documeto)
                                                {
                                                    movNuevo.documento = Convert.ToString(compraDevol.numero);
                                                }

                                                if (buscarCuenta.concepniff == 1)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = Convert.ToDecimal(costoTotal, miCultura);

                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif = Convert.ToDecimal(costoTotal, miCultura);
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif = Convert.ToDecimal(costoTotal, miCultura);
                                                }

                                                if (buscarCuenta.concepniff == 5)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = Convert.ToDecimal(costoTotal, miCultura);
                                                }
                                            }

                                            #endregion

                                            #region Retencion

                                            if (parametro.id_nombre_parametro == 3)
                                            {
                                                /*if (info.aplicaniff==true)
												{

												}*/

                                                if (info.manejabase)
                                                {
                                                    movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                                }
                                                else
                                                {
                                                    movNuevo.basecontable = 0;
                                                }

                                                if (info.documeto)
                                                {
                                                    movNuevo.documento = Convert.ToString(compraDevol.numero);
                                                }

                                                if (buscarCuenta.concepniff == 1)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = Convert.ToDecimal(
                                                        Math.Round(valor_totalenca *
                                                                   Convert.ToDecimal(
                                                                       buscarTipoDocRegistro.retencion / 100, miCultura)), miCultura);

                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif =
                                                        Convert.ToDecimal(Math.Round(
                                                            valor_totalenca *
                                                            Convert.ToDecimal(buscarTipoDocRegistro.retencion / 100, miCultura)), miCultura);
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif =
                                                        Convert.ToDecimal(Math.Round(
                                                            valor_totalenca *
                                                            Convert.ToDecimal(buscarTipoDocRegistro.retencion / 100, miCultura)), miCultura);
                                                }

                                                if (buscarCuenta.concepniff == 5)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = Convert.ToDecimal(
                                                        Math.Round(valor_totalenca *
                                                                   Convert.ToDecimal(
                                                                       buscarTipoDocRegistro.retencion / 100, miCultura)), miCultura);
                                                }
                                            }

                                            #endregion

                                            #region ReteIVA

                                            if (parametro.id_nombre_parametro == 4)
                                            {
                                                /*if (info.aplicaniff==true)
												{

												}*/

                                                if (info.manejabase)
                                                {
                                                    movNuevo.basecontable = Convert.ToDecimal(ivaEncabezado, miCultura);
                                                }
                                                else
                                                {
                                                    movNuevo.basecontable = 0;
                                                }

                                                if (info.documeto)
                                                {
                                                    movNuevo.documento = Convert.ToString(compraDevol.numero);
                                                }

                                                if (buscarCuenta.concepniff == 1)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = Convert.ToDecimal(
                                                        Math.Round(encabezado.iva *
                                                                   Convert.ToDecimal(
                                                                       buscarTipoDocRegistro.retiva / 100, miCultura)), miCultura);

                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif =
                                                        Convert.ToDecimal(Math.Round(
                                                            encabezado.iva *
                                                            Convert.ToDecimal(buscarTipoDocRegistro.retiva / 100, miCultura)), miCultura);
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif =
                                                        Convert.ToDecimal(Math.Round(
                                                            encabezado.iva *
                                                            Convert.ToDecimal(buscarTipoDocRegistro.retiva / 100, miCultura)), miCultura);
                                                }

                                                if (buscarCuenta.concepniff == 5)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = Convert.ToDecimal(
                                                        Math.Round(encabezado.iva *
                                                                   Convert.ToDecimal(
                                                                       buscarTipoDocRegistro.retiva / 100, miCultura)), miCultura);
                                                }
                                            }

                                            #endregion

                                            #region ReteICA

                                            if (parametro.id_nombre_parametro == 5)
                                            {
                                                /*if (info.aplicaniff==true)
												{

												}*/

                                                if (info.manejabase)
                                                {
                                                    movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                                }
                                                else
                                                {
                                                    movNuevo.basecontable = 0;
                                                }

                                                if (info.documeto)
                                                {
                                                    movNuevo.documento = Convert.ToString(compraDevol.numero);
                                                }

                                                if (buscarCuenta.concepniff == 1)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = Convert.ToDecimal(encabezado.retencion_ica, miCultura);

                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif = Convert.ToDecimal(encabezado.retencion_ica, miCultura);
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    movNuevo.creditoniif = 0;
                                                    movNuevo.debitoniif = Convert.ToDecimal(encabezado.retencion_ica, miCultura);
                                                }

                                                if (buscarCuenta.concepniff == 5)
                                                {
                                                    movNuevo.credito = 0;
                                                    movNuevo.debito = Convert.ToDecimal(encabezado.retencion_ica, miCultura);
                                                }
                                            }

                                            #endregion

                                            #region Fletes

                                            if (parametro.id_nombre_parametro == 6)
                                            {
                                                /*if (info.aplicaniff==true)
												{

												}*/

                                                if (info.manejabase)
                                                {
                                                    movNuevo.basecontable = Convert.ToDecimal(fletes, miCultura);
                                                }
                                                else
                                                {
                                                    movNuevo.basecontable = 0;
                                                }

                                                if (info.documeto)
                                                {
                                                    movNuevo.documento = Convert.ToString(compraDevol.numero);
                                                }

                                                if (buscarCuenta.concepniff == 1)
                                                {
                                                    movNuevo.credito = Convert.ToDecimal(fletes, miCultura);
                                                    movNuevo.debito = 0;

                                                    movNuevo.creditoniif = Convert.ToDecimal(fletes, miCultura);
                                                    movNuevo.debitoniif = 0;
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    movNuevo.creditoniif = Convert.ToDecimal(fletes, miCultura);
                                                    ;
                                                    movNuevo.debitoniif = 0;
                                                }

                                                if (buscarCuenta.concepniff == 5)
                                                {
                                                    movNuevo.credito = Convert.ToDecimal(fletes, miCultura);
                                                    movNuevo.debito = 0;
                                                }
                                            }

                                            #endregion

                                            #region IVA fletes

                                            if (parametro.id_nombre_parametro == 14)
                                            {
                                                /*if (info.aplicaniff==true)
												{

												}*/

                                                if (info.manejabase)
                                                {
                                                    movNuevo.basecontable = Convert.ToDecimal(ivaFletes, miCultura);
                                                }
                                                else
                                                {
                                                    movNuevo.basecontable = 0;
                                                }

                                                if (info.documeto)
                                                {
                                                    movNuevo.documento = Convert.ToString(compraDevol.numero);
                                                }

                                                if (buscarCuenta.concepniff == 1)
                                                {
                                                    movNuevo.credito = Convert.ToDecimal(ivaFletes, miCultura);
                                                    movNuevo.debito = 0;

                                                    movNuevo.creditoniif = Convert.ToDecimal(ivaFletes, miCultura);
                                                    movNuevo.debitoniif = 0;
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    movNuevo.creditoniif = Convert.ToDecimal(ivaFletes, miCultura);
                                                    movNuevo.debitoniif = 0;
                                                }

                                                if (buscarCuenta.concepniff == 5)
                                                {
                                                    movNuevo.credito = Convert.ToDecimal(ivaFletes, miCultura);
                                                    movNuevo.debito = 0;
                                                }
                                            }

                                            #endregion

                                            context.mov_contable.Add(movNuevo);
                                            //context.SaveChanges();

                                            secuencia++;
                                            //Cuentas valores

                                            #region Cuentas valores

                                            cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                                x.centro == parametro.centro && x.cuenta == parametro.cuenta &&
                                                x.nit == nit);
                                            if (buscar_cuentas_valores != null)
                                            {
                                                buscar_cuentas_valores.debito += movNuevo.debito;
                                                buscar_cuentas_valores.credito += movNuevo.credito;
                                                buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                                                buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                                                context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                            }
                                            else
                                            {
                                                DateTime fechaHoy = DateTime.Now;
                                                cuentas_valores crearCuentaValor = new cuentas_valores
                                                {
                                                    ano = fechaHoy.Year,
                                                    mes = fechaHoy.Month,
                                                    cuenta = movNuevo.cuenta,
                                                    centro = movNuevo.centro,
                                                    nit = movNuevo.nit,
                                                    debito = movNuevo.debito,
                                                    credito = movNuevo.credito,
                                                    debitoniff = movNuevo.debitoniif,
                                                    creditoniff = movNuevo.creditoniif
                                                };
                                                context.cuentas_valores.Add(crearCuentaValor);
                                                //context.SaveChanges();
                                            }

                                            #endregion

                                            totalCreditos += movNuevo.credito;
                                            totalDebitos += movNuevo.debito;
                                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                                            {
                                                NumeroCuenta =
                                                    "(" + buscarCuenta.cntpuc_numero + ")" + buscarCuenta.cntpuc_descp,
                                                DescripcionParametro = descripcionParametro,
                                                ValorDebito = movNuevo.debito,
                                                ValorCredito = movNuevo.credito
                                            });
                                        }
                                    }
                                }

                                #endregion

                                //Lineas documento

                                #region lineasDocumento

                                List<mov_contable> listaMov = new List<mov_contable>();
                                int listaLineas = Convert.ToInt32(Request["lista"]);
                                for (int i = 0; i <= listaLineas; i++)
                                    if (!string.IsNullOrEmpty(Request["codigo" + i]))
                                    {
                                        decimal porDescuento = !string.IsNullOrEmpty(Request["porcentaje_descuento" + i])
                                            ? Convert.ToDecimal(Request["porcentaje_descuento" + i], miCultura)
                                            : 0;

                                        string codigo = Request["codigo" + i];
                                        decimal valorReferencia = Convert.ToDecimal(Request["valor_unitario" + i], miCultura);
                                        decimal descontar = porDescuento / 100;
                                        decimal porIVAReferencia = Convert.ToDecimal(Request["porcentaje_iva" + i], miCultura) / 100;
                                        decimal final = valorReferencia - valorReferencia * descontar;

                                        decimal fnl = final % 1;
                                        if (fnl < Convert.ToDecimal(0.5, miCultura))
                                        {
                                            final = Math.Round(final);
                                        }
                                        else if (fnl >= Convert.ToDecimal(0.5, miCultura))
                                        {
                                            final = Math.Ceiling(final);
                                        }

                                        decimal baseUnitario = final * Convert.ToDecimal(Request["cantidadDevuelta" + i], miCultura);
                                        decimal ivaReferencia =
                                            final * porIVAReferencia *
                                            Convert.ToDecimal(Request["cantidadDevuelta" + i], miCultura);
                                        icb_referencia unidadCodigo =
                                            context.icb_referencia.FirstOrDefault(x => x.ref_codigo == codigo);
                                        string und = unidadCodigo.unidad_medida;

                                        decimal bu = baseUnitario % 1;
                                        if (bu < Convert.ToDecimal(0.5, miCultura))
                                        {
                                            baseUnitario = Math.Round(baseUnitario);
                                        }
                                        else if (bu >= Convert.ToDecimal(0.5, miCultura))
                                        {
                                            baseUnitario = Math.Ceiling(baseUnitario);
                                        }

                                        decimal ir = ivaReferencia % 1;
                                        if (ir < Convert.ToDecimal(0.5, miCultura))
                                        {
                                            ivaReferencia = Math.Round(ivaReferencia);
                                        }
                                        else if (ir >= Convert.ToDecimal(0.5, miCultura))
                                        {
                                            ivaReferencia = Math.Ceiling(ivaReferencia);
                                        }

                                        lineas_documento lineas = new lineas_documento
                                        {
                                            id_encabezado = id_encabezado,
                                            codigo = Request["codigo" + i],
                                            seq = i + 1,
                                            fec = DateTime.Now,
                                            nit = nit,
                                            und = Convert.ToString(und),
                                            cantidad = Convert.ToDecimal(Request["cantidadDevuelta" + i], miCultura)
                                        };
                                        decimal ivaLista = Convert.ToDecimal(Request["porcentaje_iva" + i], miCultura);
                                        lineas.porcentaje_iva = (float)ivaLista;
                                        lineas.valor_unitario = final;
                                        decimal descuento = porDescuento;
                                        lineas.porcentaje_descuento = (float)descuento;
                                        lineas.costo_unitario = final;
                                        lineas.bodega = bodega;
                                        lineas.fec_creacion = DateTime.Now;
                                        lineas.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                        lineas.estado = true;
                                        lineas.moneda = Convert.ToInt32(Request["moneda"]);
                                        lineas.tasa = Convert.ToInt32(Request["tasa"]);
                                        context.lineas_documento.Add(lineas);

                                        #endregion

                                        //actualizamos las lineas de la compra, ponemos la cantidad devuelta (todas)
                                        lineas_documento lineaDevuelta = context.lineas_documento.FirstOrDefault(x =>
                                            x.id_encabezado == compraDevol.idencabezado && x.codigo == lineas.codigo);

                                        #region actualizar las lineas de la compra devuelta

                                        lineaDevuelta.cantidad_devuelta += lineas.cantidad;
                                        lineaDevuelta.fec_actualizacion = DateTime.Now;
                                        lineaDevuelta.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                                        context.Entry(lineaDevuelta).State = EntityState.Modified;

                                        #endregion

                                        //Referencias Inven

                                        #region referencias inven

                                        string referencia = Request["codigo" + i];
                                        int anio = DateTime.Now.Year;
                                        int mes = DateTime.Now.Month;

                                        referencias_inven refin = new referencias_inven();

                                        referencias_inven existencia = context.referencias_inven.FirstOrDefault(x =>
                                            x.ano == anio && x.mes == mes && x.codigo == referencia &&
                                            x.bodega == bodega);

                                        if (existencia != null)
                                        {
                                            existencia.codigo = referencia;
                                            existencia.can_sal += Convert.ToDecimal(Request["cantidadDevuelta" + i], miCultura);
                                            existencia.cos_sal +=
                                                final * Convert.ToDecimal(Request["cantidadDevuelta" + i], miCultura);
                                            existencia.can_dev_com +=
                                                Convert.ToDecimal(Request["cantidadDevuelta" + i], miCultura);
                                            existencia.cos_dev_com +=
                                                final * Convert.ToDecimal(Request["cantidadDevuelta" + i], miCultura);
                                            context.Entry(existencia).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            refin.bodega = bodega;
                                            refin.codigo = referencia;
                                            refin.ano = Convert.ToInt16(DateTime.Now.Year);
                                            refin.mes = Convert.ToInt16(DateTime.Now.Month);
                                            refin.can_sal = Convert.ToDecimal(Request["cantidadDevuelta" + i], miCultura);
                                            refin.cos_sal = final;
                                            refin.can_dev_com = Convert.ToDecimal(Request["cantidadDevuelta" + i], miCultura);
                                            refin.cos_dev_com = final;
                                            refin.modulo = "R";
                                            context.referencias_inven.Add(refin);
                                        }

                                        #endregion

                                        //Mov Contable (IVA e Inventario)

                                        #region Mov Contable (IVA e Inventario)

                                        foreach (var parametro in parametrosCuentasVerificar)
                                        {
                                            string descripcionParametro = context.paramcontablenombres
                                                .FirstOrDefault(x => x.id == parametro.id_nombre_parametro)
                                                .descripcion_parametro;
                                            cuenta_puc buscarCuenta =
                                                context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);

                                            if (buscarCuenta != null)
                                            {
                                                if (parametro.id_nombre_parametro == 2 &&
                                                    Convert.ToDecimal(ivaReferencia, miCultura) != 0
                                                    || parametro.id_nombre_parametro == 9 &&
                                                    Convert.ToDecimal(valor_totalenca, miCultura) != 0
                                                    || parametro.id_nombre_parametro == 20 &&
                                                    Convert.ToDecimal(valor_totalenca, miCultura) != 0)
                                                {
                                                    mov_contable movNuevo = new mov_contable
                                                    {
                                                        id_encab = encabezado.idencabezado,
                                                        seq = secuencia,
                                                        idparametronombre = parametro.id_nombre_parametro,
                                                        cuenta = parametro.cuenta,
                                                        centro = parametro.centro,
                                                        fec = DateTime.Now,
                                                        fec_creacion = DateTime.Now,
                                                        userid_creacion =
                                                        Convert.ToInt32(Session["user_usuarioid"]),
                                                        documento = Convert.ToString(compraDevol.numero),
                                                        estado = true
                                                    };

                                                    cuenta_puc info = context.cuenta_puc
                                                        .Where(a => a.cntpuc_id == parametro.cuenta).FirstOrDefault();

                                                    if (info.tercero)
                                                    {
                                                        movNuevo.nit = modelo.nit;
                                                    }
                                                    else
                                                    {
                                                        icb_terceros tercero = context.icb_terceros
                                                            .Where(t => t.doc_tercero == "0").FirstOrDefault();
                                                        movNuevo.nit = tercero.tercero_id;
                                                    }

                                                    #region IVA

                                                    if (parametro.id_nombre_parametro == 2)
                                                    {
                                                        icb_referencia perfilReferencia =
                                                            context.icb_referencia.FirstOrDefault(x =>
                                                                x.ref_codigo == lineas.codigo);
                                                        int perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                                        perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(r =>
                                                            r.id == perfilBuscar);

                                                        #region Tiene perfil la referencia

                                                        if (pcr != null)
                                                        {
                                                            int? cuentaIva = pcr.cuenta_dev_iva_compras;

                                                            movNuevo.id_encab = encabezado.idencabezado;
                                                            movNuevo.seq = secuencia;
                                                            movNuevo.idparametronombre = parametro.id_nombre_parametro;

                                                            #region si tiene perfil y cuenta asignada a ese perfil

                                                            if (cuentaIva != null)
                                                            {
                                                                movNuevo.cuenta = Convert.ToInt32(cuentaIva);
                                                                movNuevo.centro = parametro.centro;
                                                                movNuevo.fec = DateTime.Now;
                                                                movNuevo.fec_creacion = DateTime.Now;
                                                                movNuevo.userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]);
                                                                movNuevo.documento =
                                                                    Convert.ToString(compraDevol.numero);

                                                                cuenta_puc infoReferencia = context.cuenta_puc
                                                                    .Where(a => a.cntpuc_id == cuentaIva)
                                                                    .FirstOrDefault();
                                                                if (infoReferencia.manejabase)
                                                                {
                                                                    movNuevo.basecontable =
                                                                        Convert.ToDecimal(baseUnitario, miCultura);
                                                                }
                                                                else
                                                                {
                                                                    movNuevo.basecontable = 0;
                                                                }

                                                                if (infoReferencia.documeto)
                                                                {
                                                                    movNuevo.documento =
                                                                        Convert.ToString(compraDevol.numero);
                                                                }

                                                                if (infoReferencia.concepniff == 1)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(ivaReferencia, miCultura);
                                                                    movNuevo.debito = 0;

                                                                    movNuevo.creditoniif =
                                                                        Convert.ToDecimal(ivaReferencia, miCultura);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (infoReferencia.concepniff == 4)
                                                                {
                                                                    movNuevo.creditoniif =
                                                                        Convert.ToDecimal(ivaReferencia, miCultura);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (infoReferencia.concepniff == 5)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(ivaReferencia, miCultura);
                                                                    movNuevo.debito = 0;
                                                                }

                                                                // context.mov_contable.Add(movNuevo);
                                                            }

                                                            #endregion

                                                            #region si tiene perfil pero no tiene cuenta asignada

                                                            else
                                                            {
                                                                movNuevo.cuenta = parametro.cuenta;
                                                                movNuevo.centro = parametro.centro;
                                                                movNuevo.fec = DateTime.Now;
                                                                movNuevo.fec_creacion = DateTime.Now;
                                                                movNuevo.userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]);
                                                                movNuevo.documento =
                                                                    Convert.ToString(compraDevol.numero);

                                                                cuenta_puc infoReferencia = context.cuenta_puc
                                                                    .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                    .FirstOrDefault();
                                                                if (infoReferencia.manejabase)
                                                                {
                                                                    movNuevo.basecontable =
                                                                        Convert.ToDecimal(baseUnitario, miCultura);
                                                                }
                                                                else
                                                                {
                                                                    movNuevo.basecontable = 0;
                                                                }

                                                                if (infoReferencia.documeto)
                                                                {
                                                                    movNuevo.documento =
                                                                        Convert.ToString(compraDevol.numero);
                                                                }

                                                                if (infoReferencia.concepniff == 1)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(ivaReferencia, miCultura);
                                                                    movNuevo.debito = 0;

                                                                    movNuevo.creditoniif =
                                                                        Convert.ToDecimal(ivaReferencia, miCultura);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (infoReferencia.concepniff == 4)
                                                                {
                                                                    movNuevo.creditoniif =
                                                                        Convert.ToDecimal(ivaReferencia, miCultura);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (infoReferencia.concepniff == 5)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(ivaReferencia, miCultura);
                                                                    movNuevo.debito = 0;
                                                                }

                                                                //context.mov_contable.Add(movNuevo);
                                                            }

                                                            #endregion
                                                        }

                                                        #endregion

                                                        #region La referencia no tiene perfil

                                                        else
                                                        {
                                                            movNuevo.id_encab = encabezado.idencabezado;
                                                            movNuevo.seq = secuencia;
                                                            movNuevo.idparametronombre = parametro.id_nombre_parametro;
                                                            movNuevo.cuenta = parametro.cuenta;
                                                            movNuevo.centro = parametro.centro;
                                                            movNuevo.fec = DateTime.Now;
                                                            movNuevo.fec_creacion = DateTime.Now;
                                                            movNuevo.userid_creacion =
                                                                Convert.ToInt32(Session["user_usuarioid"]);
                                                            /*if (info.aplicaniff==true)
															{

															}*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(baseUnitario, miCultura);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento =
                                                                    Convert.ToString(compraDevol.numero);
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = Convert.ToDecimal(ivaReferencia, miCultura);
                                                                movNuevo.debito = 0;

                                                                movNuevo.creditoniif = Convert.ToDecimal(ivaReferencia, miCultura);
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = Convert.ToDecimal(ivaReferencia, miCultura);
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = Convert.ToDecimal(ivaReferencia, miCultura);
                                                                movNuevo.debito = 0;
                                                            }

                                                            //context.mov_contable.Add(movNuevo);
                                                        }

                                                        #endregion

                                                        mov_contable buscarIVA = context.mov_contable.FirstOrDefault(x =>
                                                            x.id_encab == id_encabezado &&
                                                            x.cuenta == movNuevo.cuenta &&
                                                            x.idparametronombre == parametro.id_nombre_parametro);
                                                        if (buscarIVA != null)
                                                        {
                                                            buscarIVA.debito += movNuevo.debito;
                                                            buscarIVA.debitoniif += movNuevo.debitoniif;
                                                            buscarIVA.credito += movNuevo.credito;
                                                            buscarIVA.creditoniif += movNuevo.creditoniif;
                                                            context.Entry(buscarIVA).State = EntityState.Modified;
                                                        }
                                                        else
                                                        {
                                                            mov_contable crearMovContable = new mov_contable
                                                            {
                                                                id_encab = encabezado.idencabezado,
                                                                seq = secuencia,
                                                                idparametronombre =
                                                                parametro.id_nombre_parametro,
                                                                cuenta = Convert.ToInt32(movNuevo.cuenta),
                                                                centro = parametro.centro,
                                                                nit = encabezado.nit,
                                                                fec = DateTime.Now,
                                                                debito = movNuevo.debito,
                                                                debitoniif = movNuevo.debitoniif,
                                                                basecontable = movNuevo.basecontable,
                                                                credito = movNuevo.credito,
                                                                creditoniif = movNuevo.creditoniif,
                                                                fec_creacion = DateTime.Now,
                                                                userid_creacion =
                                                                Convert.ToInt32(Session["user_usuarioid"]),
                                                                detalle =
                                                                "Devolucion manual de la compra con consecutivo " +
                                                                compraDevol.numero,
                                                                estado = true
                                                            };
                                                            context.mov_contable.Add(crearMovContable);
                                                        }
                                                    }

                                                    #endregion

                                                    #region Inventario				

                                                    if (parametro.id_nombre_parametro == 9 ||
                                                        parametro.id_nombre_parametro == 20)
                                                    {
                                                        icb_referencia perfilReferencia =
                                                            context.icb_referencia.FirstOrDefault(x =>
                                                                x.ref_codigo == lineas.codigo);
                                                        int perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                                        perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(r =>
                                                            r.id == perfilBuscar);

                                                        #region Tiene perfil la referencia

                                                        if (pcr != null)
                                                        {
                                                            int? cuentaInven = pcr.cuenta_dev_inv_compra;

                                                            movNuevo.id_encab = encabezado.idencabezado;
                                                            movNuevo.seq = secuencia;
                                                            movNuevo.idparametronombre = parametro.id_nombre_parametro;

                                                            #region tiene perfil y cuenta asignada al perfil

                                                            if (cuentaInven != null)
                                                            {
                                                                movNuevo.cuenta = Convert.ToInt32(cuentaInven);
                                                                movNuevo.centro = parametro.centro;
                                                                movNuevo.fec = DateTime.Now;
                                                                movNuevo.fec_creacion = DateTime.Now;
                                                                movNuevo.userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]);
                                                                movNuevo.documento =
                                                                    Convert.ToString(compraDevol.numero);

                                                                cuenta_puc infoReferencia = context.cuenta_puc
                                                                    .Where(a => a.cntpuc_id == cuentaInven)
                                                                    .FirstOrDefault();
                                                                if (infoReferencia.manejabase)
                                                                {
                                                                    movNuevo.basecontable =
                                                                        Convert.ToDecimal(baseUnitario, miCultura);
                                                                }
                                                                else
                                                                {
                                                                    movNuevo.basecontable = 0;
                                                                }

                                                                if (infoReferencia.documeto)
                                                                {
                                                                    movNuevo.documento =
                                                                        Convert.ToString(compraDevol.numero);
                                                                }

                                                                if (infoReferencia.concepniff == 1)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(baseUnitario, miCultura);
                                                                    movNuevo.debito = 0;

                                                                    movNuevo.creditoniif =
                                                                        Convert.ToDecimal(baseUnitario, miCultura);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (infoReferencia.concepniff == 4)
                                                                {
                                                                    movNuevo.creditoniif =
                                                                        Convert.ToDecimal(baseUnitario, miCultura);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (infoReferencia.concepniff == 5)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(baseUnitario, miCultura);
                                                                    movNuevo.debito = 0;
                                                                }

                                                                //context.mov_contable.Add(movNuevo);
                                                            }

                                                            #endregion

                                                            #region tiene perfil pero no tiene cuenta asignada

                                                            else
                                                            {
                                                                movNuevo.cuenta = parametro.cuenta;
                                                                movNuevo.centro = parametro.centro;
                                                                movNuevo.fec = DateTime.Now;
                                                                movNuevo.fec_creacion = DateTime.Now;
                                                                movNuevo.userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]);
                                                                movNuevo.documento =
                                                                    Convert.ToString(compraDevol.numero);

                                                                cuenta_puc infoReferencia = context.cuenta_puc
                                                                    .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                    .FirstOrDefault();
                                                                if (infoReferencia.manejabase)
                                                                {
                                                                    movNuevo.basecontable =
                                                                        Convert.ToDecimal(baseUnitario, miCultura);
                                                                }
                                                                else
                                                                {
                                                                    movNuevo.basecontable = 0;
                                                                }

                                                                if (infoReferencia.documeto)
                                                                {
                                                                    movNuevo.documento =
                                                                        Convert.ToString(compraDevol.numero);
                                                                }

                                                                if (infoReferencia.concepniff == 1)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(baseUnitario, miCultura);
                                                                    movNuevo.debito = 0;

                                                                    movNuevo.creditoniif =
                                                                        Convert.ToDecimal(baseUnitario, miCultura);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (infoReferencia.concepniff == 4)
                                                                {
                                                                    movNuevo.creditoniif =
                                                                        Convert.ToDecimal(baseUnitario, miCultura);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (infoReferencia.concepniff == 5)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(baseUnitario, miCultura);
                                                                    movNuevo.debito = 0;
                                                                }

                                                                //context.mov_contable.Add(movNuevo);
                                                            }

                                                            #endregion
                                                        }

                                                        #endregion

                                                        #region La referencia no tiene perfil

                                                        else
                                                        {
                                                            movNuevo.id_encab = encabezado.idencabezado;
                                                            movNuevo.seq = secuencia;
                                                            movNuevo.idparametronombre = parametro.id_nombre_parametro;
                                                            movNuevo.cuenta = parametro.cuenta;
                                                            movNuevo.centro = parametro.centro;
                                                            movNuevo.fec = DateTime.Now;
                                                            movNuevo.fec_creacion = DateTime.Now;
                                                            movNuevo.userid_creacion =
                                                                Convert.ToInt32(Session["user_usuarioid"]);
                                                            /*if (info.aplicaniff==true)
															{

															}*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(baseUnitario, miCultura);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento =
                                                                    Convert.ToString(compraDevol.numero);
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = Convert.ToDecimal(baseUnitario, miCultura);
                                                                movNuevo.debito = 0;

                                                                movNuevo.creditoniif = Convert.ToDecimal(baseUnitario, miCultura);
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = Convert.ToDecimal(baseUnitario, miCultura);
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = Convert.ToDecimal(baseUnitario, miCultura);
                                                                movNuevo.debito = 0;
                                                            }

                                                            //context.mov_contable.Add(movNuevo);
                                                        }

                                                        #endregion

                                                        mov_contable buscarInventario = context.mov_contable.FirstOrDefault(x =>
                                                            x.id_encab == id_encabezado &&
                                                            x.cuenta == movNuevo.cuenta &&
                                                            x.idparametronombre == parametro.id_nombre_parametro);
                                                        if (buscarInventario != null)
                                                        {
                                                            buscarInventario.basecontable += movNuevo.basecontable;
                                                            buscarInventario.debito += movNuevo.debito;
                                                            buscarInventario.debitoniif += movNuevo.debitoniif;
                                                            buscarInventario.credito += movNuevo.credito;
                                                            buscarInventario.creditoniif += movNuevo.creditoniif;
                                                            context.Entry(buscarInventario).State =
                                                                EntityState.Modified;
                                                        }
                                                        else
                                                        {
                                                            mov_contable crearMovContable = new mov_contable
                                                            {
                                                                id_encab = encabezado.idencabezado,
                                                                seq = secuencia,
                                                                idparametronombre =
                                                                parametro.id_nombre_parametro,
                                                                cuenta = Convert.ToInt32(movNuevo.cuenta),
                                                                centro = parametro.centro,
                                                                nit = encabezado.nit,
                                                                fec = DateTime.Now,
                                                                debito = movNuevo.debito,
                                                                debitoniif = movNuevo.debitoniif,
                                                                basecontable = movNuevo.basecontable,
                                                                credito = movNuevo.credito,
                                                                creditoniif = movNuevo.creditoniif,
                                                                fec_creacion = DateTime.Now,
                                                                userid_creacion =
                                                                Convert.ToInt32(Session["user_usuarioid"]),
                                                                detalle =
                                                                "Devolucion manual de la compra con consecutivo" +
                                                                compraDevol.numero,
                                                                estado = true
                                                            };
                                                            context.mov_contable.Add(crearMovContable);
                                                        }
                                                    }

                                                    #endregion

                                                    secuencia++;
                                                    //Cuentas valores

                                                    #region Cuentas valores

                                                    cuentas_valores buscar_cuentas_valores =
                                                        context.cuentas_valores.FirstOrDefault(x =>
                                                            x.centro == parametro.centro &&
                                                            x.cuenta == movNuevo.cuenta && x.nit == nit);
                                                    if (buscar_cuentas_valores != null)
                                                    {
                                                        buscar_cuentas_valores.debito += movNuevo.debito;
                                                        buscar_cuentas_valores.credito += movNuevo.credito;
                                                        buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                                                        buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                                                        context.Entry(buscar_cuentas_valores).State =
                                                            EntityState.Modified;
                                                    }
                                                    else
                                                    {
                                                        DateTime fechaHoy = DateTime.Now;
                                                        cuentas_valores crearCuentaValor = new cuentas_valores
                                                        {
                                                            ano = fechaHoy.Year,
                                                            mes = fechaHoy.Month,
                                                            cuenta = movNuevo.cuenta,
                                                            centro = movNuevo.centro,
                                                            nit = movNuevo.nit,
                                                            debito = movNuevo.debito,
                                                            credito = movNuevo.credito,
                                                            debitoniff = movNuevo.debitoniif,
                                                            creditoniff = movNuevo.creditoniif
                                                        };
                                                        context.cuentas_valores.Add(crearCuentaValor);
                                                        context.SaveChanges();
                                                    }

                                                    #endregion

                                                    totalCreditos += movNuevo.credito;
                                                    totalDebitos += movNuevo.debito;
                                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                    {
                                                        NumeroCuenta =
                                                            "(" + buscarCuenta.cntpuc_numero + ")" +
                                                            buscarCuenta.cntpuc_descp,
                                                        DescripcionParametro = descripcionParametro,
                                                        ValorDebito = movNuevo.debito,
                                                        ValorCredito = movNuevo.credito
                                                    });
                                                }
                                            }
                                        }

                                        #endregion
                                    }

                                #region validaciones para guardar

                                decimal td = totalDebitos % 1;
                                if (td < Convert.ToDecimal(0.5, miCultura))
                                {
                                    totalDebitos = Math.Round(totalDebitos);
                                }
                                else if (td >= Convert.ToDecimal(0.5, miCultura))
                                {
                                    totalDebitos = Math.Ceiling(totalDebitos);
                                }

                                decimal tc = totalCreditos % 1;
                                if (tc < Convert.ToDecimal(0.5, miCultura))
                                {
                                    totalCreditos = Math.Round(totalCreditos);
                                }
                                else if (tc >= Convert.ToDecimal(0.5, miCultura))
                                {
                                    totalCreditos = Math.Ceiling(totalCreditos);
                                }

                                if (totalDebitos != totalCreditos)
                                {
                                    TempData["mensaje_error"] =
                                        "El documento no tiene los movimientos calculados correctamente, verifique el perfil del documento";

                                    ViewBag.documentoSeleccionado = encabezado.tipo;
                                    ViewBag.bodegaSeleccionado = encabezado.bodega;
                                    ViewBag.perfilSeleccionado = encabezado.perfilcontable;

                                    ViewBag.documentoDescuadrado = listaDescuadrados;
                                    ViewBag.calculoDebito = totalDebitos;
                                    ViewBag.calculoCredito = totalCreditos;
                                    dbTran.Rollback();
                                    listas(compraDevol.idencabezado);
                                    BuscarFavoritos(menu);
                                    return View(modelo);
                                }

                                funciono = 1;

                                #endregion

                                if (funciono > 0)
                                {
                                    context.SaveChanges();
                                    dbTran.Commit();
                                    TempData["mensaje"] = "registro creado correctamente";
                                    DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
                                    doc.ActualizarConsecutivo(grupo.grupo, consecutivo);

                                    return RedirectToAction("DetalleDevoluciones",
                                        new { id = id_encabezado, menu = ViewBag.id_menu });
                                }
                            }
                            else
                            {
                                TempData["mensaje_error"] = "no hay consecutivo";
                            }
                        }
                        //cierre
                        else
                        {
                            TempData["mensaje_error"] = "Lista vacia";
                        }
                    }
                    catch (DbEntityValidationException)
                    {
                        dbTran.Rollback();
                        throw;
                    }
                }
            }
            else
            {
                TempData["mensaje_error"] = "No fue posible guardar el registro, por favor valide";
                List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();
            }

            //listas(.id_encabezado);
            BuscarFavoritos(menu);
            return RedirectToAction("DevolucionManual", "devolucionComprasRepuestos", new { menu });
        }

        public JsonResult buscarCompraDevolver(int idDevol)
        {
            var data = from e in context.encab_documento
                       join b in context.bodega_concesionario
                           on e.bodega equals b.id
                       join t in context.icb_terceros
                           on e.nit equals t.tercero_id
                       join tp in context.tp_doc_registros
                           on e.tipo equals tp.tpdoc_id
                       join l in context.lineas_documento
                           on e.idencabezado equals l.id_encabezado
                       where tp.tipo == 3 && e.idencabezado == idDevol
                       select new
                       {
                           tipoDocumento = "(" + tp.prefijo + ") " + tp.tpdoc_nombre,
                           e.numero,
                           idNit = t.tercero_id,
                           nit = t.prinom_tercero != null
                               ? "(" + t.doc_tercero + ")" + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                 t.apellido_tercero + " " + t.segapellido_tercero
                               : "(" + t.doc_tercero + ") " + t.razon_social,
                           fecha = e.fecha.ToString(),
                           e.valor_total,
                           id = e.idencabezado,
                           bodega = b.bodccs_nombre,
                           estado = e.valor_aplicado != 0 ? "Devuelta" : "Comprado",
                           fecha_devolucion = e.fec_actualizacion.Value.ToString(),
                           facturaProveedor = e.documento
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarBodegasPorDocumento(int? id, string bodegaDevol)
        {
            var data = (from consecutivos in context.icb_doc_consecutivos
                        join bodega in context.bodega_concesionario
                            on consecutivos.doccons_bodega equals bodega.id
                        where consecutivos.doccons_idtpdoc == id && bodega.bodccs_nombre == bodegaDevol
                        select new
                        {
                            bodega.bodccs_nombre,
                            bodega.id
                        }).ToList();


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DevolucionAutomatica(int bodega, int nit, int tipo_doc, int id_devolucion, string motivo)
        {
            #region variables

            tp_doc_registros buscarTipoDocRegistro = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == tipo_doc);
            tercero_proveedor buscarProveedor = context.tercero_proveedor.FirstOrDefault(x => x.tercero_id == nit);
            int regimen_proveedor = buscarProveedor != null ? buscarProveedor.tpregimen_id : 0;
            perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                x.bodega == bodega && x.sw == buscarTipoDocRegistro.sw && x.tipo_regimenid == regimen_proveedor);
            //var costoModelo = Convert.ToDecimal(modelo.costo);

            encab_documento devolucion = context.encab_documento.FirstOrDefault(x => x.idencabezado == id_devolucion);

            if (devolucion != null)
            {
                // Validacion para saber si el documento ya se le realizo una devolucion
                int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                string castString = devolucion.numero.ToString();
                encab_documento buscarDevolucion = context.encab_documento.Where(x =>
                        x.prefijo == devolucion.idencabezado && x.bodega == bodegaActual && x.documento == castString)
                    .FirstOrDefault();
                if (buscarDevolucion != null)
                {
                    string mensaje = "La factura ya fue devuelta, el numero de devolucion es : " + buscarDevolucion.numero;
                    return Json(new { devuelto = true, mensaje }, JsonRequestBehavior.AllowGet);
                }

                // Fin validacion para saber si el documento ya se le realizo una devolucion
                int? perfilContable = devolucion.perfilcontable;

                decimal totalCreditos = 0;
                decimal totalDebitos = 0;

                int secuencia = 1;

                List<cuentas_valores> ids_cuentas_valores = new List<cuentas_valores>();
                centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
                List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();

                #endregion

                #region validar referencias compradas

                List<string> referenciasLineasCompradas = context.lineas_documento.Where(x => x.id_encabezado == id_devolucion)
                    .Select(x => x.codigo).ToList();

                DateTime fechaActual = DateTime.Now;

                List<lineas_documento> lineasReferencias = context.lineas_documento.Where(x => x.id_encabezado == id_devolucion).ToList();
                List<mov_contable> movimientosContables = context.mov_contable.Where(x => x.id_encab == id_devolucion).ToList();

                #endregion

                //consecutivo
                grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == tipo_doc && x.bodega_id == bodega);
                if (grupo != null)
                {
                    DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                    long consecutivo = doc.BuscarConsecutivo(grupo.grupo);

                    using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                    {
                        try
                        {
                            //Encabezado documento

                            #region encabezado

                            encab_documento encabezado = new encab_documento
                            {
                                tipo = tipo_doc,
                                numero = consecutivo,
                                nit = nit,
                                fecha = DateTime.Now
                            };
                            int? condicion = devolucion.fpago_id;
                            encabezado.fpago_id = condicion;
                            int dias = context.fpago_tercero.Find(condicion).dvencimiento ?? 0;
                            DateTime vencimiento = DateTime.Now.AddDays(dias);
                            encabezado.vencimiento = vencimiento;
                            encabezado.valor_total = devolucion.valor_total;
                            encabezado.retencion = devolucion.retencion;
                            encabezado.retencion_ica = devolucion.retencion_ica;
                            encabezado.retencion_iva = devolucion.retencion_iva;
                            encabezado.porcen_retencion = devolucion.porcen_retencion;
                            encabezado.porcen_retica = devolucion.porcen_retica;
                            encabezado.porcen_reteiva = devolucion.porcen_reteiva;
                            encabezado.iva = devolucion.iva;
                            encabezado.fletes = devolucion.fletes;
                            encabezado.iva_fletes = devolucion.iva_fletes;
                            encabezado.costo = devolucion.costo;
                            encabezado.vendedor = devolucion.vendedor;
                            encabezado.documento = Convert.ToString(devolucion.numero);
                            encabezado.bodega = bodega;
                            encabezado.concepto = devolucion.concepto;
                            encabezado.moneda = devolucion.moneda;
                            encabezado.tasa = devolucion.tasa;
                            encabezado.valor_mercancia = devolucion.valor_mercancia;
                            encabezado.fec_creacion = DateTime.Now;
                            encabezado.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                            encabezado.estado = true;
                            encabezado.perfilcontable = Convert.ToInt32(Request["perfil"]);
                            encabezado.concepto2 = devolucion.concepto2;
                            encabezado.prefijo = devolucion.idencabezado;
                            encabezado.notas = motivo;

                            context.encab_documento.Add(encabezado);
                            context.SaveChanges();

                            #endregion

                            int id_encabezado = context.encab_documento.OrderByDescending(x => x.idencabezado)
                                .FirstOrDefault().idencabezado;

                            //actualizar el encabezado de la compra

                            #region actualizar la compra devuelta

                            devolucion.documento = Convert.ToString(consecutivo);
                            devolucion.valor_aplicado = devolucion.valor_total;
                            devolucion.fec_actualizacion = fechaActual;
                            context.Entry(devolucion).State = EntityState.Modified;

                            #endregion

                            //Cruce documentos

                            #region cruce documentos

                            cruce_documentos cruce = new cruce_documentos
                            {
                                idtipo = tipo_doc,
                                numero = consecutivo,
                                idtipoaplica = devolucion.tipo,
                                numeroaplica = devolucion.numero,
                                valor = devolucion.valor_total,
                                fecha = fechaActual,
                                fechacruce = fechaActual,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                            };

                            context.cruce_documentos.Add(cruce);

                            #endregion

                            //Mov Contable

                            #region movimientos contables

                            //buscamos en perfil cuenta documento, por medio del perfil seleccionado

                            foreach (mov_contable movimientos in movimientosContables)
                            {
                                string descripcionParametro = context.paramcontablenombres
                                    .FirstOrDefault(x => x.id == movimientos.idparametronombre).descripcion_parametro;
                                cuenta_puc buscarCuenta =
                                    context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == movimientos.cuenta);

                                if (buscarCuenta != null)
                                {
                                    mov_contable movNuevo = new mov_contable
                                    {
                                        id_encab = encabezado.idencabezado,
                                        seq = secuencia,
                                        idparametronombre = movimientos.idparametronombre,
                                        cuenta = movimientos.cuenta,
                                        centro = movimientos.centro,
                                        fec = DateTime.Now,
                                        fec_creacion = DateTime.Now,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                        documento = devolucion.documento
                                    };

                                    cuenta_puc info = context.cuenta_puc.Where(a => a.cntpuc_id == movimientos.cuenta)
                                        .FirstOrDefault();

                                    if (info.tercero)
                                    {
                                        movNuevo.nit = nit;
                                    }
                                    else
                                    {
                                        icb_terceros tercero = context.icb_terceros.Where(t => t.doc_tercero == "0")
                                            .FirstOrDefault();
                                        movNuevo.nit = tercero.tercero_id;
                                    }

                                    // las siguientes validaciones se hacen dependiendo de las cuentas que movió la compra manual, para guardar la informacion acorde

                                    #region Cuentas X Pagar

                                    if (movimientos.idparametronombre == 1)
                                    {
                                        /*if (info.aplicaniff==true)
										{

										}*/

                                        if (info.manejabase)
                                        {
                                            movNuevo.basecontable = movimientos.basecontable;
                                        }
                                        else
                                        {
                                            movNuevo.basecontable = 0;
                                        }

                                        if (info.documeto)
                                        {
                                            movNuevo.documento = movimientos.documento;
                                        }

                                        if (buscarCuenta.concepniff == 1)
                                        {
                                            movNuevo.credito = 0;
                                            movNuevo.debito = movimientos.credito;

                                            movNuevo.creditoniif = 0;
                                            movNuevo.debitoniif = movimientos.creditoniif;
                                        }

                                        if (buscarCuenta.concepniff == 4)
                                        {
                                            movNuevo.creditoniif = 0;
                                            movNuevo.debitoniif = movimientos.creditoniif;
                                        }

                                        if (buscarCuenta.concepniff == 5)
                                        {
                                            movNuevo.credito = 0;
                                            movNuevo.debito = movimientos.credito;
                                        }
                                    }

                                    #endregion

                                    #region Retencion

                                    if (movimientos.idparametronombre == 3)
                                    {
                                        /*if (info.aplicaniff==true)
										{

										}*/

                                        if (info.manejabase)
                                        {
                                            movNuevo.basecontable = movimientos.basecontable;
                                        }
                                        else
                                        {
                                            movNuevo.basecontable = 0;
                                        }

                                        if (info.documeto)
                                        {
                                            movNuevo.documento = movimientos.documento;
                                        }

                                        if (buscarCuenta.concepniff == 1)
                                        {
                                            movNuevo.credito = 0;
                                            movNuevo.debito = movimientos.credito;

                                            movNuevo.creditoniif = 0;
                                            movNuevo.debitoniif = movimientos.creditoniif;
                                        }

                                        if (buscarCuenta.concepniff == 4)
                                        {
                                            movNuevo.creditoniif = 0;
                                            movNuevo.debitoniif = movimientos.creditoniif;
                                        }

                                        if (buscarCuenta.concepniff == 5)
                                        {
                                            movNuevo.credito = 0;
                                            movNuevo.debito = movimientos.credito;
                                        }
                                    }

                                    #endregion

                                    #region ReteIVA

                                    if (movimientos.idparametronombre == 4)
                                    {
                                        /*if (info.aplicaniff==true)
										{

										}*/

                                        if (info.manejabase)
                                        {
                                            movNuevo.basecontable = movimientos.basecontable;
                                        }
                                        else
                                        {
                                            movNuevo.basecontable = 0;
                                        }

                                        if (info.documeto)
                                        {
                                            movNuevo.documento = movimientos.documento;
                                        }

                                        if (buscarCuenta.concepniff == 1)
                                        {
                                            movNuevo.credito = 0;
                                            movNuevo.debito = movimientos.credito;

                                            movNuevo.creditoniif = 0;
                                            movNuevo.debitoniif = movimientos.creditoniif;
                                        }

                                        if (buscarCuenta.concepniff == 4)
                                        {
                                            movNuevo.creditoniif = 0;
                                            movNuevo.debitoniif = movimientos.creditoniif;
                                        }

                                        if (buscarCuenta.concepniff == 5)
                                        {
                                            movNuevo.credito = 0;
                                            movNuevo.debito = movimientos.credito;
                                        }
                                    }

                                    #endregion

                                    #region ReteICA

                                    if (movimientos.idparametronombre == 5)
                                    {
                                        /*if (info.aplicaniff==true)
										{

										}*/

                                        if (info.manejabase)
                                        {
                                            movNuevo.basecontable = movimientos.basecontable;
                                        }
                                        else
                                        {
                                            movNuevo.basecontable = 0;
                                        }

                                        if (info.documeto)
                                        {
                                            movNuevo.documento = movimientos.documento;
                                        }

                                        if (buscarCuenta.concepniff == 1)
                                        {
                                            movNuevo.credito = 0;
                                            movNuevo.debito = movimientos.credito;

                                            movNuevo.creditoniif = 0;
                                            movNuevo.debitoniif = movimientos.creditoniif;
                                        }

                                        if (buscarCuenta.concepniff == 4)
                                        {
                                            movNuevo.creditoniif = 0;
                                            movNuevo.debitoniif = movimientos.creditoniif;
                                        }

                                        if (buscarCuenta.concepniff == 5)
                                        {
                                            movNuevo.credito = 0;
                                            movNuevo.debito = movimientos.credito;
                                        }
                                    }

                                    #endregion

                                    #region Fletes

                                    if (movimientos.idparametronombre == 6)
                                    {
                                        /*if (info.aplicaniff==true)
										{

										}*/

                                        if (info.manejabase)
                                        {
                                            movNuevo.basecontable = movimientos.basecontable;
                                        }
                                        else
                                        {
                                            movNuevo.basecontable = 0;
                                        }

                                        if (info.documeto)
                                        {
                                            movNuevo.documento = movimientos.documento;
                                        }

                                        if (buscarCuenta.concepniff == 1)
                                        {
                                            movNuevo.credito = movimientos.debito;
                                            movNuevo.debito = 0;

                                            movNuevo.creditoniif = movimientos.debitoniif;
                                            movNuevo.debitoniif = 0;
                                        }

                                        if (buscarCuenta.concepniff == 4)
                                        {
                                            movNuevo.creditoniif = movimientos.debitoniif;
                                            movNuevo.debitoniif = 0;
                                        }

                                        if (buscarCuenta.concepniff == 5)
                                        {
                                            movNuevo.credito = movimientos.debito;
                                            movNuevo.debito = 0;
                                        }
                                    }

                                    #endregion

                                    #region IVA fletes

                                    if (movimientos.idparametronombre == 14)
                                    {
                                        /*if (info.aplicaniff==true)
										{

										}*/

                                        if (info.manejabase)
                                        {
                                            movNuevo.basecontable = movimientos.basecontable;
                                        }
                                        else
                                        {
                                            movNuevo.basecontable = 0;
                                        }

                                        if (info.documeto)
                                        {
                                            movNuevo.documento = movimientos.documento;
                                        }

                                        if (buscarCuenta.concepniff == 1)
                                        {
                                            movNuevo.credito = movimientos.debito;
                                            movNuevo.debito = 0;

                                            movNuevo.creditoniif = movimientos.debitoniif;
                                            movNuevo.debitoniif = 0;
                                        }

                                        if (buscarCuenta.concepniff == 4)
                                        {
                                            movNuevo.creditoniif = movimientos.debitoniif;
                                            movNuevo.debitoniif = 0;
                                        }

                                        if (buscarCuenta.concepniff == 5)
                                        {
                                            movNuevo.credito = movimientos.debito;
                                            movNuevo.debito = 0;
                                        }
                                    }

                                    #endregion

                                    #region IVA

                                    if (movimientos.idparametronombre == 2)
                                    {
                                        /*if (info.aplicaniff==true)
										{

										}*/

                                        if (info.manejabase)
                                        {
                                            movNuevo.basecontable = movimientos.basecontable;
                                        }
                                        else
                                        {
                                            movNuevo.basecontable = 0;
                                        }

                                        if (info.documeto)
                                        {
                                            movNuevo.documento = movimientos.documento;
                                        }

                                        if (buscarCuenta.concepniff == 1)
                                        {
                                            movNuevo.credito = movimientos.debito;
                                            movNuevo.debito = 0;

                                            movNuevo.creditoniif = movimientos.debitoniif;
                                            movNuevo.debitoniif = 0;
                                        }

                                        if (buscarCuenta.concepniff == 4)
                                        {
                                            movNuevo.creditoniif = movimientos.debitoniif;
                                            movNuevo.debitoniif = 0;
                                        }

                                        if (buscarCuenta.concepniff == 5)
                                        {
                                            movNuevo.credito = movimientos.debito;
                                            movNuevo.debito = 0;
                                        }
                                    }

                                    #endregion

                                    #region Inventario

                                    if (movimientos.idparametronombre == 9)
                                    {
                                        /*if (info.aplicaniff==true)
										{

										}*/

                                        if (info.manejabase)
                                        {
                                            movNuevo.basecontable = movimientos.basecontable;
                                        }
                                        else
                                        {
                                            movNuevo.basecontable = 0;
                                        }

                                        if (info.documeto)
                                        {
                                            movNuevo.documento = movimientos.documento;
                                        }

                                        if (buscarCuenta.concepniff == 1)
                                        {
                                            movNuevo.credito = movimientos.debito;
                                            movNuevo.debito = 0;

                                            movNuevo.creditoniif = movimientos.debitoniif;
                                            movNuevo.debitoniif = 0;
                                        }

                                        if (buscarCuenta.concepniff == 4)
                                        {
                                            movNuevo.creditoniif = movimientos.debitoniif;
                                            movNuevo.debitoniif = 0;
                                        }

                                        if (buscarCuenta.concepniff == 5)
                                        {
                                            movNuevo.credito = movimientos.debito;
                                            movNuevo.debito = 0;
                                        }
                                    }

                                    #endregion

                                    context.mov_contable.Add(movNuevo);
                                    //context.SaveChanges();

                                    secuencia++;
                                    //Cuentas valores

                                    #region Cuentas valores

                                    cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                        x.centro == movimientos.centro && x.cuenta == movimientos.cuenta &&
                                        x.nit == movNuevo.nit && x.ano == fechaActual.Year &&
                                        x.mes == fechaActual.Month);
                                    if (buscar_cuentas_valores != null)
                                    {
                                        buscar_cuentas_valores.debito += movNuevo.debito;
                                        buscar_cuentas_valores.credito += movNuevo.credito;
                                        buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                                        buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                                        context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        cuentas_valores crearCuentaValor = new cuentas_valores
                                        {
                                            ano = fechaActual.Year,
                                            mes = fechaActual.Month,
                                            cuenta = movNuevo.cuenta,
                                            centro = movNuevo.centro,
                                            nit = movNuevo.nit,
                                            debito = movNuevo.debito,
                                            credito = movNuevo.credito,
                                            debitoniff = movNuevo.debitoniif,
                                            creditoniff = movNuevo.creditoniif
                                        };
                                        context.cuentas_valores.Add(crearCuentaValor);
                                        //context.SaveChanges();
                                    }

                                    #endregion

                                    totalCreditos += movNuevo.credito;
                                    totalDebitos += movNuevo.debito;
                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                    {
                                        NumeroCuenta =
                                            "(" + buscarCuenta.cntpuc_numero + ")" + buscarCuenta.cntpuc_descp,
                                        DescripcionParametro = descripcionParametro,
                                        ValorDebito = movNuevo.debito,
                                        ValorCredito = movNuevo.credito
                                    });
                                }
                            }

                            #endregion

                            //Lineas documento y referencias invem

                            #region lineasDocumento y referencias invem

                            int i = 0;
                            foreach (lineas_documento item in lineasReferencias)
                            {
                                if (item.codigo != null)
                                {
                                    i++;
                                    lineas_documento linea = new lineas_documento
                                    {
                                        id_encabezado = id_encabezado,
                                        codigo = item.codigo,
                                        seq = i,
                                        fec = DateTime.Now,
                                        nit = nit,
                                        cantidad = item.cantidad,
                                        valor_unitario = item.valor_unitario,
                                        costo_unitario = item.valor_unitario,
                                        bodega = bodega,
                                        cantidad_pedida = item.cantidad,
                                        fec_creacion = DateTime.Now,
                                        estado = true,
                                        moneda = devolucion.moneda,
                                        tasa = devolucion.tasa,
                                        pedido = Convert.ToInt32(item.pedido)
                                    };
                                    icb_referencia por_iva = context.icb_referencia.Find(item.codigo);
                                    if (por_iva != null)
                                    {
                                        linea.porcentaje_iva = por_iva.por_iva_compra;
                                    }

                                    context.lineas_documento.Add(linea);

                                    //actualizamos las lineas de la compra, ponemos la cantidad devuelta (todas)
                                    lineas_documento lineaDevuelta = context.lineas_documento.FirstOrDefault(x =>
                                        x.id_encabezado == id_devolucion && x.codigo == item.codigo);

                                    #region actualizar las lineas de la compra devuelta

                                    lineaDevuelta.cantidad_devuelta += item.cantidad;
                                    lineaDevuelta.fec_actualizacion = fechaActual;
                                    context.Entry(lineaDevuelta).State = EntityState.Modified;

                                    #endregion
                                }

                                //Referencias Inven

                                #region referencias inven

                                referencias_inven referencia = context.referencias_inven.FirstOrDefault(x =>
                                    x.codigo == item.codigo && x.ano == DateTime.Now.Year &&
                                    x.mes == DateTime.Now.Month && x.bodega == bodega);
                                if (referencia != null)
                                {
                                    referencia.can_sal += item.cantidad;
                                    referencia.cos_sal += item.valor_unitario * item.cantidad;
                                    referencia.can_dev_com += item.cantidad;
                                    referencia.cos_dev_com += item.valor_unitario * item.cantidad;
                                    context.Entry(referencia).State = EntityState.Modified;
                                }
                                else
                                {
                                    referencias_inven refe = new referencias_inven
                                    {
                                        codigo = item.codigo,
                                        bodega = bodega,
                                        ano = Convert.ToInt16(DateTime.Now.Year),
                                        mes = Convert.ToInt16(DateTime.Now.Month),
                                        can_sal = item.cantidad,
                                        cos_sal = item.valor_unitario * item.cantidad,
                                        can_dev_com = item.cantidad,
                                        cos_dev_com = item.valor_unitario * item.cantidad,
                                        modulo = "R"
                                    };
                                    context.referencias_inven.Add(refe);
                                }

                                #endregion
                            }

                            #endregion

                            #region validaciones para guardar

                            if (totalDebitos != totalCreditos)
                            {
                                //TempData["mensaje_error"] = "El documento no tiene los movimientos calculados correctamente, verifique el perfil del documento";

                                ViewBag.documentoSeleccionado = encabezado.tipo;
                                ViewBag.bodegaSeleccionado = encabezado.bodega;
                                ViewBag.perfilSeleccionado = encabezado.perfilcontable;

                                ViewBag.documentoDescuadrado = listaDescuadrados;
                                ViewBag.calculoDebito = totalDebitos;
                                ViewBag.calculoCredito = totalCreditos;
                                dbTran.Rollback();
                                return Json(
                                    new
                                    {
                                        descuadrado = true,
                                        deb = totalDebitos,
                                        cred = totalCreditos,
                                        lista = listaDescuadrados
                                    }, JsonRequestBehavior.AllowGet);
                            }

                            context.SaveChanges();
                            dbTran.Commit();
                            DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
                            doc.ActualizarConsecutivo(grupo.grupo, consecutivo);
                            return Json(new { exito = true, numeroConsecutivo = encabezado.numero },
                                JsonRequestBehavior.AllowGet);

                            #endregion
                        }

                        catch (DbEntityValidationException)
                        {
                            dbTran.Rollback();
                            throw;
                        }
                    }
                }

                {
                    string mensaje = "No se encontro el grupo para este documento.";
                    return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
                }
            }

            {
                string mensaje = "No se encontro el encabezado de este documento.";
                return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult BuscarReferenciasADevolver(int idDevol)
        {
            int data = (from e in context.encab_documento
                        join b in context.bodega_concesionario
                            on e.bodega equals b.id
                        join l in context.lineas_documento
                            on e.idencabezado equals l.id_encabezado
                        join t in context.icb_terceros
                            on e.nit equals t.tercero_id
                        join tp in context.tp_doc_registros
                            on e.tipo equals tp.tpdoc_id
                        where tp.tipo == 3 && l.cantidad_devuelta < l.cantidad && l.id_encabezado == idDevol
                        select new
                        {
                            tipoDocumento = "(" + tp.prefijo + ") " + tp.tpdoc_nombre,
                            e.numero,
                            idNit = t.tercero_id,
                            nit = t.prinom_tercero != null
                                ? "(" + t.doc_tercero + ")" + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                  t.apellido_tercero + " " + t.segapellido_tercero
                                : "(" + t.doc_tercero + ") " + t.razon_social,
                            fecha = e.fecha.ToString(),
                            e.valor_total,
                            id = e.idencabezado,
                            bodega = b.bodccs_nombre,
                            estado = e.valor_aplicado != 0 ? "Devuelta" : "Comprado",
                            fecha_devolucion = e.fec_actualizacion.Value.ToString()
                        }).Count();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarLineasADevolver(int consecutivo, int tipo)
        {
            int encabezado = (from e in context.encab_documento
                              where e.numero == consecutivo && e.tipo == tipo
                              select e.idencabezado).FirstOrDefault();

            var data = (from l in context.lineas_documento
                        join r in context.icb_referencia
                            on l.codigo equals r.ref_codigo
                        where l.id_encabezado == encabezado
                        select new
                        {
                            l.codigo,
                            descripcion = r.ref_descripcion,
                            valor = l.valor_unitario,
                            descuento = l.porcentaje_descuento != null ? l.porcentaje_descuento : 0,
                            iva = l.porcentaje_iva,
                            cantidad = l.cantidad - l.cantidad_devuelta
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult devolucionComprasPDF(int? id)
        {
            if (id != null)
            {
                string buscarEmpresa = context.tablaempresa.Select(x => x.nombre_empresa).FirstOrDefault();

                var buscarEncabezado = (from a in context.encab_documento
                                        join b in context.bodega_concesionario
                                            on a.bodega equals b.id
                                        join t in context.tp_doc_registros
                                            on a.tipo equals t.tpdoc_id
                                        join p in context.icb_terceros
                                            on a.nit equals p.tercero_id
                                        where a.idencabezado == id
                                        select new { a, b, t, p }).FirstOrDefault();

                string root = Server.MapPath("~/Pdf/");
                string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
                string path = Path.Combine(root, pdfname);
                path = Path.GetFullPath(path);
                CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");


                PDFmodel devolucion = new PDFmodel
                {
                    empresa = buscarEmpresa,
                    tipoDocumento = buscarEncabezado.t.tpdoc_nombre,
                    numeroRegistro = buscarEncabezado.a.numero,
                    bodega = buscarEncabezado.b.bodccs_nombre,
                    fechaEncabezado = buscarEncabezado.a.fecha.ToString("yyyy-MM-dd"),
                    numeroFactura = buscarEncabezado.a.documento,
                    proveedor = buscarEncabezado.p.razon_social,
                    nit = buscarEncabezado.p.doc_tercero,
                    digitoVerificacion = buscarEncabezado.p.digito_verificacion,
                    referencias = (from doc in context.encab_documento
                                   join l in context.lineas_documento
                                       on doc.idencabezado equals l.id_encabezado
                                   join r in context.icb_referencia
                                       on l.codigo equals r.ref_codigo
                                   where doc.idencabezado == id
                                   select new referenciasPDF
                                   {
                                       cantidad = l.cantidad,
                                       codigo = l.codigo,
                                       descripcion = r.ref_descripcion,
                                       costo = l.costo_unitario,
                                       secuencia = l.seq,
                                       iva = l.porcentaje_iva,
                                       descuento = l.porcentaje_descuento,
                                       costoTotal = l.costo_unitario * l.cantidad,
                                       notas = doc.notas,
                                       fletes = doc.fletes
                                   }).ToList()
                };

                decimal total = devolucion.referencias.Sum(x => x.costo);
                devolucion.TotalTotales = total;
                float? desc = devolucion.referencias.Sum(x => x.descuento);
                devolucion.totalDescuento = desc;
                decimal fletes = devolucion.referencias.Sum(x => x.fletes);
                devolucion.fletes = fletes;
                decimal baseIva = devolucion.referencias.Sum(x => Convert.ToDecimal(x.iva, miCultura) * x.costo / 100);
                devolucion.iva = baseIva.ToString("0,0", elGR);
                decimal totalFinal = total - Convert.ToDecimal(desc, miCultura) + fletes + baseIva;
                devolucion.totalfinal = totalFinal.ToString("0,0", elGR);


                ViewAsPdf something = new ViewAsPdf("devolucionComprasPDF", devolucion);
                return something;
            }

            return null;
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