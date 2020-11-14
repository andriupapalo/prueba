using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class comprasGeneralesController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");

        // GET: comprasGenerales
        public void listas()
        {
            var buscarTipoDocumento = (from tipoDocumento in context.tp_doc_registros
                                       select new
                                       {
                                           tipoDocumento.sw,
                                           tipoDocumento.tpdoc_id,
                                           nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                           tipoDocumento.tipo
                                       }).ToList();
            ViewBag.tipo_documentoFiltro =
                new SelectList(buscarTipoDocumento.Where(x => x.sw == 3), "tpdoc_id", "nombre");

            ViewBag.tipo = new SelectList(context.tp_doc_registros.Where(x => x.tipo == 30), "tpdoc_id",
                "tpdoc_nombre");

            var users = from u in context.users
                        select new
                        {
                            idTercero = u.user_id,
                            nombre = u.user_nombre,
                            apellidos = u.user_apellido,
                            u.user_numIdent
                        };
            List<SelectListItem> itemsU = new List<SelectListItem>();
            foreach (var item in users)
            {
                string nombre = "(" + item.user_numIdent + ") - " + item.nombre + " " + item.apellidos;
                itemsU.Add(new SelectListItem { Text = nombre, Value = item.idTercero.ToString() });
            }

            ViewBag.vendedor = itemsU;


            ViewBag.moneda = new SelectList(context.monedas, "moneda", "descripcion");
            ViewBag.tasa = new SelectList(context.moneda_conversion, "id", "conversion");
            ViewBag.motivocompra = new SelectList(context.motcompra, "id", "Motivo");

            ViewBag.condicion_pago = context.fpago_tercero;

            var provedores = (from pro in context.tercero_proveedor
                              join ter in context.icb_terceros
                                  on pro.tercero_id equals ter.tercero_id
                              select new
                              {
                                  idTercero = ter.tercero_id,
                                  nombreTErcero = ter.prinom_tercero,
                                  apellidosTercero = ter.apellido_tercero,
                                  razonSocial = ter.razon_social,
                                  ter.doc_tercero
                              }).ToList();
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in provedores)
            {
                string nombre = item.doc_tercero + " - " + item.nombreTErcero + " " + item.apellidosTercero + " " +
                             item.razonSocial;
                items.Add(new SelectListItem { Text = nombre, Value = item.idTercero.ToString() });
            }

            ViewBag.nit = items;

            encab_documento buscarSerialUltimaNota = context.encab_documento.Where(x => x.tipo == 3065)
                .OrderByDescending(x => x.idencabezado).FirstOrDefault();
            ViewBag.numNotaCreado = buscarSerialUltimaNota != null ? buscarSerialUltimaNota.numero : 0;

            //var retenciones = from u in context.tablaretenciones
            //				  select new
            //				  {
            //					  u.id,
            //					  u.concepto
            //				  };
            //List<SelectListItem> itemsRet = new List<SelectListItem>();
            //foreach (var item in retenciones)
            //{
            //	var nombre = item.concepto;
            //	itemsRet.Add(new SelectListItem() { Text = nombre, Value = item.id.ToString() });
            //}
            //ViewBag.retencion_id = itemsRet;


            var retenciones = from u in context.tparamretenciones
                              select new
                              {
                                  u.id,
                                  u.concepto
                              };
            List<SelectListItem> itemsRet = new List<SelectListItem>();
            foreach (var item in retenciones)
            {
                string nombre = item.concepto;
                itemsRet.Add(new SelectListItem { Text = nombre, Value = item.id.ToString() });
            }

            ViewBag.retencion_id = itemsRet;

            var centros = from u in context.centro_costo
                          select new
                          {
                              u.centcst_id,
                              u.pre_centcst,
                              u.centcst_nombre
                          };
            List<SelectListItem> itemsCen = new List<SelectListItem>();
            foreach (var item in centros)
            {
                string nombre = "(" + item.pre_centcst + ") " + item.centcst_nombre;
                itemsCen.Add(new SelectListItem { Text = nombre, Value = item.centcst_id.ToString() });
            }

            ViewBag.centcst_id = itemsCen;
        }

        public ActionResult Create()
        {
            listas();
            return View();
        }

        [HttpPost]
        public ActionResult Create(ComprasGeneralesModel cgm)
        {
            if (ModelState.IsValid)
            {
                //using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                //{
                //	try
                //	{
                int anniohoy = cgm.fecha.Year;
                int meshoy = cgm.fecha.Month;


                meses_cierre Si_esta_Cerrado = context.meses_cierre.FirstOrDefault(x => x.ano == anniohoy && x.mes == meshoy);
                if (Si_esta_Cerrado == null)
                {
                    //consecutivo
                    grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                        x.documento_id == cgm.tipo && x.bodega_id == cgm.bodega);
                    if (grupo != null)
                    {
                        DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                        long consecutivo = doc.BuscarConsecutivo(grupo.grupo);

                        //Encabezado documento
                        encab_documento encabezado = new encab_documento
                        {
                            tipo = cgm.tipo,
                            numero = consecutivo,
                            nit = cgm.nit,
                            fecha = Convert.ToDateTime(cgm.fecha), // DateTime.Now;
                            fpago_id = cgm.fpago_id,
                            vencimiento = Convert.ToDateTime(cgm.vencimiento),
                            valor_total = cgm.valor_total != null ? Convert.ToDecimal(cgm.valor_total, miCultura) : 0,
                            iva = cgm.iva != null ? Convert.ToDecimal(cgm.iva, miCultura) : 0,
                            retencion = cgm.retencion != null ? Convert.ToDecimal(cgm.retencion, miCultura) : 0,
                            retencion_ica = cgm.retencion_ica != null ? Convert.ToDecimal(cgm.retencion_ica, miCultura) : 0,
                            retencion_iva = cgm.retencion_iva != null ? Convert.ToDecimal(cgm.retencion_iva, miCultura) : 0,
                            costo = 0,
                            vendedor = Convert.ToInt32(cgm.vendedor),
                            valor_aplicado = 0,
                            anulado = false,
                            documento = cgm.numFactura,
                            //encabezado.//prefijo = ncm.prefijo;
                            valor_mercancia = Convert.ToDecimal(cgm.valorFactura, miCultura)
                        };
                        if (!string.IsNullOrEmpty(cgm.notas))
                        {
                            encabezado.notas = cgm.nota1;
                        }

                        if (!string.IsNullOrEmpty(cgm.nota1))
                        {
                            encabezado.nota1 = cgm.nota1;
                        }

                        encabezado.impoconsumo = 0;
                        encabezado.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                        encabezado.fec_creacion = DateTime.Now;
                        encabezado.porcen_retencion = cgm.por_retencion != null
                            ? float.Parse(cgm.por_retencion, CultureInfo.InvariantCulture)
                            : 0; //esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal
                        encabezado.porcen_reteiva = cgm.por_retencion_iva != null
                            ? float.Parse(cgm.por_retencion_iva, CultureInfo.InvariantCulture)
                            : 0; //esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal
                        encabezado.porcen_retica = cgm.por_retencion_iva != null
                            ? float.Parse(cgm.por_retencion_ica, CultureInfo.InvariantCulture)
                            : 0; //esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal
                        if (cgm.concepto != null)
                        {
                            encabezado.concepto = cgm.concepto;
                        }

                        if (cgm.concepto2 != null)
                        {
                            encabezado.concepto2 = cgm.concepto2;
                        }

                        encabezado.perfilcontable = Convert.ToInt32(cgm.perfilcontable);
                        encabezado.bodega = Convert.ToInt32(cgm.bodega);
                        encabezado.estado = true;
                        encabezado.tipodocprov = Convert.ToString(cgm.prefijoFac);
                        encabezado.idconcepretencion = Convert.ToInt32(cgm.retencion_id);
                        // Agrega a Encabezado encab_documentos
                        context.encab_documento.Add(encabezado);
                        //movimiento contable
                        //buscamos en perfil cuenta documento, por medio del perfil seleccionado
                        var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                                          join nombreParametro in context.paramcontablenombres
                                                              on perfil.id_nombre_parametro equals nombreParametro.id
                                                          join cuenta in context.cuenta_puc
                                                              on perfil.cuenta equals cuenta.cntpuc_id
                                                          join dis in context.detallesComprasDistri
                                                              on perfil.centro equals dis.idCentro into al
                                                          from dis in al.DefaultIfEmpty()
                                                          where perfil.id_perfil == cgm.perfilcontable &&
                                                                !((perfil.id_nombre_parametro == 12 || perfil.id_nombre_parametro == 13) &&
                                                                  dis.monto == null)
                                                          select new
                                                          {
                                                              perfil.id,
                                                              perfil.id_nombre_parametro,
                                                              perfil.cuenta,
                                                              perfil.centro,
                                                              perfil.id_perfil,
                                                              nombreParametro.descripcion_parametro,
                                                              cuenta.cntpuc_numero,
                                                              dis.monto
                                                          }).ToList();

                        int secuencia = 1;
                        decimal totalDebitos = 0;
                        decimal totalCreditos = 0;

                        List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();
                        List<cuentas_valores> ids_cuentas_valores = new List<cuentas_valores>();
                        centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                        int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
                        foreach (var parametro in parametrosCuentasVerificar)
                        {
                            string descripcionParametro = context.paramcontablenombres
                                .FirstOrDefault(x => x.id == parametro.id_nombre_parametro).descripcion_parametro;
                            cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);

                            if (buscarCuenta != null)
                            {
                                if (parametro.id_nombre_parametro == 1
                                    || parametro.id_nombre_parametro == 2 && Convert.ToDecimal(cgm.iva, miCultura) != 0
                                    || parametro.id_nombre_parametro == 3 && Convert.ToDecimal(cgm.retencion, miCultura) != 0
                                    || parametro.id_nombre_parametro == 4 &&
                                    Convert.ToDecimal(cgm.por_retencion_iva, miCultura) != 0
                                    || parametro.id_nombre_parametro == 5 && Convert.ToDecimal(cgm.retencion_ica, miCultura) != 0
                                    || parametro.id_nombre_parametro == 12
                                    || parametro.id_nombre_parametro == 13)
                                {
                                    mov_contable movNuevo = new mov_contable
                                    {
                                        id_encab = cgm.id_encab,
                                        seq = secuencia,
                                        idparametronombre = parametro.id_nombre_parametro,
                                        cuenta = parametro.cuenta,
                                        centro = parametro.centro,
                                        fec = DateTime.Now,
                                        fec_creacion = DateTime.Now,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                        detalle = cgm.nota1 != null ? cgm.nota1 : ""
                                    };

                                    cuenta_puc info = context.cuenta_puc.Where(i => i.cntpuc_id == parametro.cuenta)
                                        .FirstOrDefault();

                                    if (info.tercero)
                                    {
                                        movNuevo.nit = cgm.nit;
                                    }
                                    else
                                    {
                                        icb_terceros tercero = context.icb_terceros.Where(t => t.doc_tercero == "0")
                                            .FirstOrDefault();
                                        movNuevo.nit = tercero.tercero_id;
                                    }

                                    // las siguientes validaciones se hacen dependiendo de la cuenta que esta moviendo la Compra General, para guardar la informacion acorde

                                    #region Parametro 1

                                    if (parametro.id_nombre_parametro == 1)
                                    {
                                        //movNuevo.cuenta = cgm.ctaxpagar;
                                        movNuevo.cuenta = parametro.cuenta;

                                        if (info.manejabase)
                                        {
                                            movNuevo.basecontable = Convert.ToDecimal(cgm.valorFactura, miCultura);
                                        }
                                        else
                                        {
                                            movNuevo.basecontable = 0;
                                        }

                                        if (info.documeto)
                                        {
                                            movNuevo.documento = cgm.numFactura;
                                        }

                                        if (buscarCuenta.concepniff == 1)
                                        {
                                            movNuevo.credito = Convert.ToDecimal(cgm.valor_total, miCultura);
                                            movNuevo.debito = 0;

                                            movNuevo.creditoniif = Convert.ToDecimal(cgm.valor_total, miCultura);
                                            movNuevo.debitoniif = 0;
                                        }

                                        if (buscarCuenta.concepniff == 4)
                                        {
                                            movNuevo.creditoniif = Convert.ToDecimal(cgm.valor_total, miCultura);
                                            movNuevo.debitoniif = 0;
                                        }

                                        if (buscarCuenta.concepniff == 5)
                                        {
                                            movNuevo.credito = Convert.ToDecimal(cgm.valor_total, miCultura);
                                            movNuevo.debito = 0;
                                        }
                                    }

                                    #endregion

                                    #region Parametro 2 IVA

                                    decimal TempExisteIva = Convert.ToDecimal(cgm.iva, miCultura);

                                    if (TempExisteIva > 0)
                                    {
                                        if (parametro.id_nombre_parametro == 2)
                                        {
                                            //movNuevo.cuenta = cgm.ctaimpuesto;
                                            movNuevo.cuenta = parametro.cuenta;
                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(cgm.valorFactura, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = cgm.numFactura;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(cgm.iva, miCultura);
                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(cgm.iva, miCultura);
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(cgm.iva, miCultura);
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(cgm.iva, miCultura);
                                            }
                                        }
                                    }

                                    #endregion

                                    #region Parametro 3 Retencion

                                    decimal TempExisteRetencion = Convert.ToDecimal(cgm.retencion, miCultura);
                                    if (TempExisteRetencion != 0)
                                    {
                                        if (parametro.id_nombre_parametro == 3)
                                        {
                                            //movNuevo.cuenta = cgm.ctaretencion;
                                            movNuevo.cuenta = parametro.cuenta;
                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(cgm.valorFactura, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = cgm.numFactura;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(cgm.retencion, miCultura);
                                                movNuevo.debito = 0;

                                                movNuevo.creditoniif = Convert.ToDecimal(cgm.retencion, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(cgm.retencion, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(cgm.retencion, miCultura);
                                                movNuevo.debito = 0;
                                            }
                                        }
                                    }

                                    #endregion

                                    #region Parametro 4 Retencion IVA

                                    decimal TempExisteRetencionIVA = Convert.ToDecimal(cgm.retencion_iva, miCultura);
                                    if (TempExisteRetencionIVA != 0)
                                    {
                                        if (parametro.id_nombre_parametro == 4)
                                        {
                                            //movNuevo.cuenta = cgm.ctareteiva;
                                            movNuevo.cuenta = parametro.cuenta;
                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(cgm.iva, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = cgm.numFactura;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(cgm.retencion_iva, miCultura);
                                                movNuevo.debito = 0;

                                                movNuevo.creditoniif = Convert.ToDecimal(cgm.retencion_iva, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(cgm.retencion_iva, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(cgm.retencion_iva, miCultura);
                                                movNuevo.debito = 0;
                                            }
                                        }
                                    }

                                    #endregion

                                    #region Parametro 5

                                    decimal TempExisteRetencionICA = Convert.ToDecimal(cgm.retencion_ica, miCultura);
                                    if (TempExisteRetencionICA != 0)
                                    {
                                        if (parametro.id_nombre_parametro == 5)
                                        {
                                            //movNuevo.cuenta = cgm.ctaica;
                                            movNuevo.cuenta = parametro.cuenta;
                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(cgm.valorFactura, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = cgm.numFactura;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(cgm.retencion_ica, miCultura);
                                                movNuevo.debito = 0;
                                                movNuevo.creditoniif = Convert.ToDecimal(cgm.retencion_ica, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(cgm.retencion_ica, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(cgm.retencion_ica, miCultura);
                                                movNuevo.debito = 0;
                                            }
                                        }
                                    }

                                    #endregion

                                    //	var cont = 0;

                                    #region  Parametro 13

                                    //if (parametro.id_nombre_parametro == 12 || parametro.id_nombre_parametro == 13)
                                    //{

                                    //	foreach (var centr in distribucion)
                                    //	{
                                    //		movNuevo.id_encab = cgm.id_encab;
                                    //		movNuevo.seq = secuencia;
                                    //		movNuevo.idparametronombre = parametro.id_nombre_parametro;
                                    //		movNuevo.cuenta = parametro.cuenta;
                                    //		movNuevo.centro = Convert.ToInt32(centr.idCentro);
                                    //		movNuevo.fec = DateTime.Now;
                                    //		movNuevo.fec_creacion = DateTime.Now;
                                    //		movNuevo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                    //		movNuevo.detalle = cgm.nota1;

                                    //		var info2 = context.cuenta_puc.Where(i => i.cntpuc_id == parametro.cuenta).FirstOrDefault();

                                    //		if (info2.tercero == true)
                                    //		{
                                    //			movNuevo.nit = cgm.nit;
                                    //		}
                                    //		else
                                    //		{
                                    //			var tercero = context.icb_terceros.Where(t => t.doc_tercero == "0").FirstOrDefault();
                                    //			movNuevo.nit = tercero.tercero_id;
                                    //		}


                                    //		if (info.manejabase == true)
                                    //		{
                                    //			movNuevo.basecontable = Convert.ToDecimal(centr.monto);
                                    //		}
                                    //		else
                                    //		{
                                    //			movNuevo.basecontable = 0;
                                    //		}

                                    //		if (info.documeto == true)
                                    //		{
                                    //			movNuevo.documento = cgm.numFactura;
                                    //		}

                                    //		if (buscarCuenta.concepniff == 1)
                                    //		{
                                    //			movNuevo.credito = 0;
                                    //			movNuevo.debito = Convert.ToDecimal(centr.monto);

                                    //			movNuevo.creditoniif = 0;
                                    //			movNuevo.debitoniif = Convert.ToDecimal(centr.monto);
                                    //		}

                                    //		if (buscarCuenta.concepniff == 4)
                                    //		{
                                    //			movNuevo.creditoniif = 0;
                                    //			movNuevo.debitoniif = Convert.ToDecimal(centr.monto);
                                    //		}

                                    //		if (buscarCuenta.concepniff == 5)
                                    //		{
                                    //			movNuevo.credito = 0;
                                    //			movNuevo.debito = Convert.ToDecimal(centr.monto);
                                    //		}
                                    //	}
                                    //	secuencia++;
                                    //	cont++;
                                    //}

                                    #endregion

                                    #region  Parametro 12 - 13 

                                    if (parametro.id_nombre_parametro == 12 || parametro.id_nombre_parametro == 13)
                                    {
                                        if (info.manejabase)
                                        {
                                            movNuevo.basecontable = Convert.ToDecimal(parametro.monto, miCultura);
                                        }
                                        else
                                        {
                                            movNuevo.basecontable = 0;
                                        }

                                        if (info.documeto)
                                        {
                                            movNuevo.documento = cgm.numFactura;
                                        }

                                        if (buscarCuenta.concepniff == 1)
                                        {
                                            movNuevo.credito = 0;
                                            movNuevo.debito = Convert.ToDecimal(parametro.monto, miCultura);

                                            movNuevo.creditoniif = 0;
                                            movNuevo.debitoniif = Convert.ToDecimal(parametro.monto, miCultura);
                                        }

                                        if (buscarCuenta.concepniff == 4)
                                        {
                                            movNuevo.creditoniif = 0;
                                            movNuevo.debitoniif = Convert.ToDecimal(parametro.monto, miCultura);
                                        }

                                        if (buscarCuenta.concepniff == 5)
                                        {
                                            movNuevo.credito = 0;
                                            movNuevo.debito = Convert.ToDecimal(parametro.monto, miCultura);
                                        }
                                    }

                                    #endregion


                                    #region  Parametro 11

                                    //if (parametro.id_nombre_parametro == 11)
                                    //{
                                    //	if (info.manejabase == true)
                                    //	{
                                    //		movNuevo.basecontable = Convert.ToDecimal(cgm.valorFactura);
                                    //	}
                                    //	else
                                    //	{
                                    //		movNuevo.basecontable = 0;
                                    //	}

                                    //	if (info.documeto == true)
                                    //	{
                                    //		movNuevo.documento = cgm.numFactura;
                                    //	}

                                    //	if (buscarCuenta.concepniff == 1)
                                    //	{
                                    //		movNuevo.credito = 0;
                                    //		movNuevo.debito = Convert.ToDecimal(cgm.valorFactura);

                                    //		movNuevo.creditoniif = 0;
                                    //		movNuevo.debitoniif = Convert.ToDecimal(cgm.valorFactura);
                                    //	}

                                    //	if (buscarCuenta.concepniff == 4)
                                    //	{
                                    //		movNuevo.creditoniif = 0;
                                    //		movNuevo.debitoniif = Convert.ToDecimal(cgm.valorFactura);
                                    //	}

                                    //	if (buscarCuenta.concepniff == 5)
                                    //	{
                                    //		movNuevo.credito = 0;
                                    //		movNuevo.debito = Convert.ToDecimal(cgm.valorFactura);
                                    //	}
                                    //}

                                    #endregion

                                    secuencia++;

                                    //Cuentas valores
                                    cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                        x.ano == anniohoy && x.mes == meshoy && x.centro == parametro.centro &&
                                        x.cuenta == parametro.cuenta && x.nit == movNuevo.nit);
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
                                    }

                                    /// Agraga a Cuentas Valores
                                    context.mov_contable.Add(movNuevo);

                                    totalCreditos += movNuevo.credito;
                                    totalDebitos += movNuevo.debito;
                                    // Agrega a lista Descuadrados
                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                    {
                                        NumeroCuenta = "(" + info.cntpuc_numero + ")" + info.cntpuc_descp,
                                        DescripcionParametro = descripcionParametro,
                                        ValorDebito = movNuevo.debito,
                                        ValorCredito = movNuevo.credito
                                    });
                                }
                            }
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

                            //	dbTran.Rollback();

                            listas();
                            return View(cgm);
                        }

                        context.SaveChanges();
                        //	dbTran.Commit();
                        TempData["mensaje"] = "registro creado correctamente";
                        DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
                        // Agrega a Consecutivos						
                        doc.ActualizarConsecutivo(grupo.grupo, consecutivo);

                        return RedirectToAction("Create");
                    }

                    TempData["mensaje_error"] = "no hay consecutivo";

                    //	}
                    //	catch (DbEntityValidationException ex)
                    //	{
                    //		dbTran.Rollback();
                    //		throw;
                    //	}
                    //}
                }
                else
                {
                    TempData["mensaje_error"] = "No fue posible guardar el registro, por favor valide";
                    List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                        .Where(y => y.Count > 0)
                        .ToList();
                }
            }
            else
            {
                TempData["mensaje_error"] = "El periodo para el que esta realizando la operacion esta Cerrado";
            }


            listas();
            return View(cgm);
        }

        public ActionResult Edit(int? id, int? menu)
        {
            //listas();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            encab_documento encabezado = context.encab_documento.Find(id);
            if (encabezado == null)
            {
                return HttpNotFound();
            }

            ViewBag.idencabezado = encabezado.idencabezado;

            var datosTipo = (from a in context.tp_doc_registros
                             where a.tpdoc_id == encabezado.tipo
                             select new
                             {
                                 a.tpdoc_id,
                                 nombre = "(" + a.prefijo + ") " + a.tpdoc_nombre
                             }).FirstOrDefault();
            ViewBag.tipo = datosTipo != null ? datosTipo.nombre : null;
            //	ViewBag.tipo = new SelectList(context.tp_doc_registros.Where(x => x.tpdoc_id == encabezado.tipo), "tpdoc_id", "tpdoc_nombre");
            var datosBodega = (from a in context.bodega_concesionario
                               where a.id == encabezado.bodega
                               select new
                               {
                                   a.id,
                                   nombre = "(" + a.bodccs_cod + ") " + a.bodccs_nombre
                               }).FirstOrDefault();
            ViewBag.bodega = datosBodega != null ? datosBodega.nombre : null;
            //	ViewBag.bodega = new SelectList(context.bodega_concesionario.Where(x => x.id == encabezado.bodega), "id", "bodccs_nombre");
            var datosNit = (from a in context.icb_terceros
                            where a.tercero_id == encabezado.nit
                            select new
                            {
                                a.tercero_id,
                                nombre = "(" + a.doc_tercero + ") " + a.prinom_tercero + " " + a.segnom_tercero + " " +
                                         a.apellido_tercero + " " + a.segapellido_tercero
                            }).FirstOrDefault();
            ViewBag.nit = datosNit != null ? datosNit.nombre : null;

            var datosNit2 = (from a in context.tercero_proveedor
                             join b in context.fpago_tercero
                                 on a.fpago_id equals b.fpago_id
                             where a.tercero_id == encabezado.nit
                             select new
                             {
                                 b.fpago_id,
                                 b.dvencimiento,
                                 b.fpago_nombre
                             }).FirstOrDefault();
            ViewBag.fpago = datosNit2 != null ? datosNit2.fpago_nombre : null;
            ViewBag.fecha = encabezado.fecha != null
                ? encabezado.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                : null;
            ViewBag.vence = encabezado.vencimiento != null
                ? encabezado.vencimiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                : null;

            var datosVendedor = (from a in context.users
                                 where a.user_id == encabezado.vendedor
                                 select new
                                 {
                                     nombre = a.user_nombre,
                                     apellidos = a.user_apellido
                                 }).FirstOrDefault();
            ViewBag.vendedor = datosVendedor != null ? datosVendedor.nombre + " " + datosVendedor.apellidos : null;


            var datosPerfil = (from b in context.perfil_contable_bodega
                               join t in context.perfil_contable_documento
                                   on b.idperfil equals t.id
                               where b.idbodega == encabezado.bodega && t.tipo == encabezado.tipo
                               select new
                               {
                                   b.idperfil,
                                   t.descripcion
                               }).FirstOrDefault();

            ViewBag.perfil = datosPerfil != null ? datosPerfil.descripcion : null;


            var datosConcepto = (from t1 in context.tpdocconceptos
                                 where t1.id == encabezado.concepto
                                 select new
                                 {
                                     t1.id,
                                     t1.Descripcion
                                 }).FirstOrDefault();
            ViewBag.concepto = datosConcepto != null ? datosConcepto.Descripcion : null;

            var datosConcepto2 = (from t2 in context.tpdocconceptos2
                                  where t2.id == encabezado.concepto2
                                  select new
                                  {
                                      t2.id,
                                      t2.Descripcion
                                  }).FirstOrDefault();
            ViewBag.concepto2 = datosConcepto2 != null ? datosConcepto2.Descripcion : null;

            ViewBag.nota1 = encabezado.notas != null ? encabezado.notas : null;
            ViewBag.prefijoFac = encabezado.tipodocprov != null ? encabezado.tipodocprov : null;
            ViewBag.numFactura = encabezado.documento != null ? encabezado.documento : null;
            ViewBag.valorFactura = encabezado.valor_mercancia.ToString("N0");

            //var datosRet = (from a in context.tablaretenciones
            //				where a.id == encabezado.idconcepretencion
            //				select new
            //				{
            //					a.id,
            //					nombre = a.concepto
            //				}).FirstOrDefault();
            //ViewBag.retencion_id = datosRet != null ? datosRet.nombre : null;

            var datosRet = (from a in context.tparamretenciones
                            where a.id == encabezado.idconcepretencion
                            select new
                            {
                                a.id,
                                nombre = a.concepto
                            }).FirstOrDefault();
            ViewBag.retencion_id = datosRet != null ? datosRet.nombre : null;

            //ViewBag.por_iva = encabezado.;
            ViewBag.iva = encabezado.iva.ToString("N0");
            ViewBag.por_retencion = encabezado.porcen_retencion;
            ViewBag.retencion = encabezado.retencion.ToString("N0");
            ViewBag.por_retencion_ica = encabezado.porcen_retica;
            ViewBag.retencion_ica = encabezado.retencion_ica.ToString("N0");
            ViewBag.por_retencion_iva = encabezado.porcen_reteiva;
            ViewBag.retencion_iva = encabezado.retencion_iva.ToString("N0");
            ViewBag.valor_total = encabezado.valor_total.ToString("N0");


            //var pre_movimientos = (from mov in context.mov_contable
            //				   join cta in context.cuenta_puc
            //				   on mov.cuenta equals cta.cntpuc_id
            //				   join ccs in context.centro_costo
            //				   on mov.centro equals ccs.centcst_id
            //				   where mov.id_encab == encabezado.idencabezado && (mov.idparametronombre == 12 || mov.idparametronombre == 13)
            //				   select new
            //				   {
            //					   mov.id_encab,
            //					   centro = "(" + ccs.centcst_id + ") " + ccs.centcst_nombre,
            //					   cuenta = "(" + cta.cntpuc_numero + ") " + cta.cntpuc_descp,
            //					   cta.mov_cnt,
            //					   mov.debito,
            //					   mov.credito,
            //					   mov.fec
            //				   }).ToList();
            //var movimientos = pre_movimientos.Select(c => new
            //{
            //	c.id_encab,
            //	c.centro,
            //	c.cuenta,
            //	debito = c.debito != null ? c.debito.ToString("N0") : "",
            //	credito = c.credito != null ? c.credito.ToString("N0") : "",
            //	fecha = c.fec != null ? c.fec.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")):""
            //}).ToList();


            return View();
        }

        [HttpPost]
        public ActionResult Edit()
        {
            return View();
        }

        public JsonResult BuscarDocumentosFiltro(int nit, DateTime? desde, DateTime? hasta, int? id_documento,
            int? factura)
        {
            if (desde == null)
            {
                desde = context.encab_documento.OrderBy(x => x.fecha).Select(x => x.fecha).FirstOrDefault();
            }

            if (hasta == null)
            {
                hasta = context.encab_documento.OrderByDescending(x => x.fecha).Select(x => x.fecha).FirstOrDefault();
            }
            else
            {
                hasta = hasta.GetValueOrDefault();
            }

            List<int> listaDocumentos = new List<int>();
            if (id_documento != null)
            {
                listaDocumentos.Add(id_documento ?? 0);
            }
            else
            {
                listaDocumentos = context.tp_doc_registros.Where(x => x.sw == 3).Select(x => x.tpdoc_id).ToList();
            }

            if (desde != null && hasta != null && desde < hasta)
            {
                //hago todo el resto de funcion
                if (factura != null)
                {
                    var buscarFacturasConSaldo = (from e in context.encab_documento
                                                  join t in context.tp_doc_registros
                                                      on e.tipo equals t.tpdoc_id
                                                  join tp in context.tp_doc_registros_tipo
                                                      on t.tipo equals tp.id
                                                  where listaDocumentos.Contains(t.tpdoc_id)
                                                        && e.valor_total - e.valor_aplicado != 0
                                                        && e.fecha >= desde && e.fecha <= hasta
                                                        && e.numero == factura
                                                        && e.nit == nit
                                                  select new
                                                  {
                                                      id = e.idencabezado,
                                                      fecha = e.fecha.ToString(),
                                                      e.valor_aplicado,
                                                      //valor_aplicado = e.valor_aplicado != null ? e.valor_aplicado : 0,
                                                      e.valor_total,
                                                      e.numero,
                                                      vencimiento = e.vencimiento.Value.ToString(),
                                                      tipo = "(" + t.prefijo + ") " + t.tpdoc_nombre,
                                                      saldo = e.valor_total - e.valor_aplicado,
                                                      retencion = e.porcen_retencion,
                                                      reteIca = e.porcen_retica,
                                                      reteIva = e.porcen_reteiva,
                                                      tipodoc = t.tpdoc_id,
                                                      descripcion = t.tpdoc_nombre,
                                                      numeroFactura = e.numero,
                                                      prefijo = e.idencabezado,
                                                      tp = e.tipo
                                                  }).ToList();

                    var data = buscarFacturasConSaldo.Select(x => new
                    {
                        x.id,
                        x.fecha,
                        x.valor_aplicado,
                        x.valor_total,
                        x.numero,
                        x.vencimiento,
                        x.tipo,
                        x.saldo,
                        x.retencion,
                        x.reteIca,
                        x.reteIva,
                        x.tipodoc,
                        x.descripcion,
                        x.numeroFactura,
                        x.prefijo,
                        x.tp
                    });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var buscarFacturasConSaldo = (from e in context.encab_documento
                                                  join t in context.tp_doc_registros
                                                      on e.tipo equals t.tpdoc_id
                                                  join tp in context.tp_doc_registros_tipo
                                                      on t.tipo equals tp.id
                                                  where listaDocumentos.Contains(t.tpdoc_id)
                                                        && e.valor_total - e.valor_aplicado != 0
                                                        && e.fecha >= desde && e.fecha <= hasta
                                                        && e.nit == nit
                                                  select new
                                                  {
                                                      id = e.idencabezado,
                                                      fecha = e.fecha.ToString(),
                                                      valor_aplicado = e.valor_aplicado,
                                                      //valor_aplicado = e.valor_aplicado != null ? e.valor_aplicado : 0,
                                                      e.valor_total,
                                                      e.numero,
                                                      vencimiento = e.vencimiento.Value.ToString(),
                                                      tipo = "(" + t.prefijo + ") " + t.tpdoc_nombre,
                                                      saldo = e.valor_total - e.valor_aplicado,
                                                      //saldo = e.valor_total - (e.valor_aplicado != null ? e.valor_aplicado : 0),
                                                      retencion = e.porcen_retencion,
                                                      reteIca = e.porcen_retica,
                                                      reteIva = e.porcen_reteiva,
                                                      tipodoc = t.tpdoc_id,
                                                      descripcion = t.tpdoc_nombre,
                                                      numeroFactura = e.numero,
                                                      t.prefijo,
                                                      tp = e.tipo
                                                  }).ToList();

                    var data = buscarFacturasConSaldo.Select(x => new
                    {
                        x.id,
                        x.fecha,
                        x.valor_aplicado,
                        x.valor_total,
                        x.numero,
                        x.vencimiento,
                        x.tipo,
                        x.saldo,
                        x.retencion,
                        x.reteIca,
                        x.reteIva,
                        x.tipodoc,
                        x.descripcion,
                        x.numeroFactura,
                        x.prefijo,
                        x.tp
                    });
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }
        //public JsonResult DetalleCompraDist(int? idUsuario, int idCentro, float? por, decimal? monto)
        //{
        //	var WidUsuario = Request[ ]; 
        //	var result = false;
        //	if (idCentro > 0)
        //	{

        //		var buscarDefBD = db.medios_gen.FirstOrDefault(c => c.ano == idUsuario && c.formato == idUsuario && c.concepto == idUsuario);
        //		if (buscarDefBD != null)
        //		{
        //			//buscarDefBD.val1 = Val1;
        //			//buscarDefBD.val2 = Val2;
        //			//buscarDefBD.val3 = Val3;
        //			//buscarDefBD.val4 = Val4;
        //			//buscarDefBD.val5 = Val5;
        //			//buscarDefBD.val6 = Val6;
        //			//buscarDefBD.val7 = Val7;
        //			//buscarDefBD.val8 = Val8;
        //			//buscarDefBD.val9 = Val9;
        //			//buscarDefBD.val10 = Val10;
        //			//buscarDefBD.val11 = Val11;

        //			//db.Entry(buscarDefBD).State = EntityState.Modified;
        //			//var actualizar = db.SaveChanges();
        //			//if (actualizar > 0)
        //			//{
        //			//	result = true;
        //			//	return Json(result, JsonRequestBehavior.AllowGet);
        //			//}
        //		}
        //		else
        //		{
        //			//db.medios_gen.Add(new medios_gen()
        //			//{
        //			//	ano = idanio,
        //			//	formato = idformato,
        //			//	concepto = idconcepto,
        //			//	val1 = Val1,
        //			//	val2 = Val2,
        //			//	val3 = Val3,
        //			//	val4 = Val4,
        //			//	val5 = Val5,
        //			//	val6 = Val6,
        //			//	val7 = Val7,
        //			//	val8 = Val8,
        //			//	val9 = Val9,
        //			//	val10 = Val10,
        //			//	val11 = Val11,
        //			//});
        //			//var guardar = db.SaveChanges();

        //			//if (guardar > 0)
        //			//{
        //			//	result = true;
        //			//	return Json(result, JsonRequestBehavior.AllowGet);
        //			//}
        //		}
        //	}
        //	result = false;
        //	return Json(result, JsonRequestBehavior.AllowGet);
        //}
        public JsonResult BuscarConceptosPorDocumento(int? tipoP)
        {
            if (tipoP != null)
            {
                var buscarConcepto = (from e1 in context.tp_doc_registros
                                      join t1 in context.tpdocconceptos
                                          on e1.tpdoc_id equals t1.tipodocid
                                      where e1.tpdoc_id == tipoP
                                      select new
                                      {
                                          t1.id,
                                          t1.Descripcion
                                      }).ToList();


                var buscarConcepto2 = (from e2 in context.tp_doc_registros
                                       join t2 in context.tpdocconceptos2
                                           on e2.tpdoc_id equals t2.tipodocid
                                       where e2.tpdoc_id == tipoP
                                       select new
                                       {
                                           t2.id,
                                           t2.Descripcion
                                       }).ToList();
                var data = new
                {
                    buscarConcepto,
                    buscarConcepto2
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDatos()
        {
            var datos = (from e in context.encab_documento
                         join b in context.bodega_concesionario
                             on e.bodega equals b.id
                         join t in context.icb_terceros
                             on e.nit equals t.tercero_id
                         join tp in context.tp_doc_registros
                             on e.tipo equals tp.tpdoc_id
                         join tpt in context.tp_doc_registros_tipo
                             on tp.tipo equals tpt.id
                         where tp.tipo == 30
                         select new
                         {
                             e.numero,
                             nit = t.prinom_tercero != null
                                 ? "(" + t.doc_tercero + ") " + t.prinom_tercero + " " + t.segnom_tercero + " " +
                                   t.apellido_tercero + " " + t.segapellido_tercero
                                 : "(" + t.doc_tercero + ") " + t.razon_social,
                             e.fecha,
                             e.valor_total,
                             id = e.idencabezado,
                             bodega = b.bodccs_nombre,
                             estado=e.devolucion==true ? "Devuelta" : "Comprado",
                             //estado = e.valor_aplicado != null ? "Devuelta" : "Comprado",
                             e.fec_actualizacion
                         }).ToList();
            var data = datos.Select(c => new
            {
                c.numero,
                c.nit,
                fecha = c.fecha != null ? c.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")) :
                    c.fecha != null ? c.fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                valor_total = c.valor_total.ToString("N0"),
                c.id,
                c.bodega,
                c.estado,
                fecha_devolucion = c.fec_actualizacion != null
                    ? c.fec_actualizacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : ""
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Browser()
        {
            return View();
        }

        public ActionResult BuscarDatosNitCompra(int? nit, int? doc, int? bod)
        {
            if (nit != null && doc != null && bod != null)
            {
                var nits = (from a in context.tercero_proveedor
                            join b in context.fpago_tercero
                                on a.fpago_id equals b.fpago_id
                            where a.tercero_id == nit
                            select new
                            {
                                b.fpago_id,
                                b.dvencimiento,
                                b.fpago_nombre
                                //	a.exentoiva,
                            }).FirstOrDefault();

                var deldoc = (from a in context.tp_doc_registros
                              where a.tpdoc_id == doc
                              select new
                              {
                                  a.sw,
                                  valorretencion = a.retencion,
                                  valorretiva = a.retiva,
                                  valorretica = a.retica
                              }).FirstOrDefault();

                var data2 = (from a in context.tercero_proveedor
                             join b in context.fpago_tercero
                                 on a.fpago_id equals b.fpago_id
                             join d in context.icb_terceros
                                      on a.tercero_id equals d.tercero_id
                             join c in context.perfiltributario
                        on d.tpregimen_id equals c.tipo_regimenid
                             where a.tercero_id == nit && c.bodega == bod && c.sw == deldoc.sw
                             select new
                             {
                                 a.tercero_id,
                                 //a.exentoiva,
                                 a.tpregimen_id,
                                 //b.fpago_id,
                                 //b.dvencimiento,
                                 //b.fpago_nombre,
                                 c.bodega,
                                 c.sw,
                                 c.retfuente,
                                 c.retiva,
                                 c.retica,
                                 pretfuente = a.retfuente,
                                 pretiva = a.retiva,
                                 pretica = a.retica
                             }).FirstOrDefault();
                var data3 = new
                {
                    data2.tercero_id,
                    data2.tpregimen_id,
                    data2.bodega,
                    data2.sw,
                    retfuente = !string.IsNullOrWhiteSpace(data2.pretfuente) ? data2.pretfuente : data2.retfuente,
                    retiva = !string.IsNullOrWhiteSpace(data2.pretiva) ? data2.pretiva : data2.retiva,
                    retica = !string.IsNullOrWhiteSpace(data2.pretica) ? data2.pretica : data2.retica
                };

                var data = new
                {
                    nits,
                    data3,
                    deldoc
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AgregaDetalleCompraDist(int idCentro, float? por, decimal monto, int op, decimal tm)
        {
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            decimal localtot = tm;
            bool result = false;
            if (op == 1)
            {
                if (por > 0) // 5
                {
                    float? porlocal = por;
                    double suma = 0;

                    var data = (from a in context.detallesComprasDistri
                                where a.idUsuario == usuario
                                select new
                                {
                                    a.por,
                                    a.monto
                                }).ToList();
                    double sumaexiste = data.Sum(item => item.por ?? 0);
                    if (por > 0)
                    {
                        suma = Convert.ToInt32(sumaexiste) + Convert.ToInt32(por);
                    }
                    else
                    {
                        suma = Convert.ToInt32(sumaexiste);
                    }

                    if (suma <= 100) // 4
                    {
                        if (idCentro > 0) // 3
                        {
                            //var buscarDefBD = context.detallesComprasDistri.FirstOrDefault(c => c.idUsuario == usuario && c.idCentro == idCentro && c.por == por && c.monto == monto);
                            detallesComprasDistri buscarDetComDes =
                                context.detallesComprasDistri.FirstOrDefault(c =>
                                    c.idUsuario == usuario && c.idCentro == idCentro);
                            if (buscarDetComDes != null) // 2
                            {
                                int Wsum = 0;
                                Wsum = Convert.ToInt32(suma) - Convert.ToInt32(buscarDetComDes.por);

                                if (por <= Wsum) /// 1
								{
                                    buscarDetComDes.por = por;
                                    buscarDetComDes.monto = monto;
                                    context.Entry(buscarDetComDes).State = EntityState.Modified;
                                    int actualizar = context.SaveChanges();
                                    if (actualizar > 0)
                                    {
                                        result = true;
                                        TempData["mensaje"] = "La actualizacion del registro temporal fue exitoso";
                                        return Json(result, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                else
                                {
                                    int ValorSI = Convert.ToInt32(Request["tvalorFactura"]);
                                    int recalmonto = ValorSI * (Wsum / 100);
                                    buscarDetComDes.por = Wsum;
                                    buscarDetComDes.monto = recalmonto;
                                    context.Entry(buscarDetComDes).State = EntityState.Modified;
                                    int actualizar = context.SaveChanges();
                                    if (actualizar > 0)
                                    {
                                        result = true;
                                        TempData["mensaje"] = "La actualizacion del registro temporal fue exitoso";
                                        return Json(result, JsonRequestBehavior.AllowGet);
                                    }
                                } /// 1
							} // 2
                            else
                            {
                                //2
                                decimal newMonto = Convert.ToDecimal(por * Convert.ToDouble(localtot) / 100, miCultura);
                                context.detallesComprasDistri.Add(new detallesComprasDistri
                                {
                                    idUsuario = usuario,
                                    idCentro = idCentro,
                                    por = por,
                                    monto = newMonto
                                });
                                int guardar = context.SaveChanges();

                                if (guardar > 0)
                                {
                                    result = true;
                                    TempData["mensaje"] = "La creación del registro temporal fue exitoso";
                                    return Json(result, JsonRequestBehavior.AllowGet);
                                }
                            } // 2
                        } //3
                    }
                } // 5
            }

            if (op == 2)
            {
                if (monto > 0) // 5
                {
                    decimal monlocal = monto;
                    decimal suma2 = 0;

                    var data22 = (from a in context.detallesComprasDistri
                                  where a.idUsuario == usuario
                                  select new
                                  {
                                      a.por,
                                      a.monto
                                  }).ToList();
                    decimal sumaexiste2 = data22.Sum(item => item.monto ?? 0);
                    if (monto > 0)
                    {
                        suma2 = Convert.ToDecimal(sumaexiste2, miCultura) + Convert.ToDecimal(monto, miCultura);
                    }
                    else
                    {
                        suma2 = Convert.ToDecimal(sumaexiste2, miCultura);
                    }

                    if (suma2 <= localtot) // 4
                    {
                        if (idCentro > 0) // 3
                        {
                            //var buscarDefBD = context.detallesComprasDistri.FirstOrDefault(c => c.idUsuario == usuario && c.idCentro == idCentro && c.por == por && c.monto == monto);
                            detallesComprasDistri buscarDetComDes =
                                context.detallesComprasDistri.FirstOrDefault(c =>
                                    c.idUsuario == usuario && c.idCentro == idCentro);
                            if (buscarDetComDes != null) // 2
                            {
                                decimal Wsum = 0;
                                Wsum = Convert.ToDecimal(suma2, miCultura) - Convert.ToDecimal(buscarDetComDes.monto, miCultura);
                                double Nuevopor = Convert.ToDouble(monlocal * 100 / localtot);


                                if (monto <= Wsum) /// 1
								{
                                    buscarDetComDes.por = Nuevopor;
                                    buscarDetComDes.monto = monto;
                                    context.Entry(buscarDetComDes).State = EntityState.Modified;
                                    int actualizar = context.SaveChanges();
                                    if (actualizar > 0)
                                    {
                                        result = true;
                                        TempData["mensaje"] = "La actualizacion del registro temporal fue exitoso";
                                        return Json(result, JsonRequestBehavior.AllowGet);
                                    }
                                }

                                //else
                                //{
                                //	var ValorSI = Convert.ToDecimal(Request["tvalorFactura"]);
                                //	var recalmonto = ValorSI * (Wsum / 100);
                                //	buscarDetComDes.por = por;
                                //	buscarDetComDes.monto = recalmonto;
                                //	context.Entry(buscarDetComDes).State = EntityState.Modified;
                                //	var actualizar = context.SaveChanges();
                                //	if (actualizar > 0)
                                //	{
                                //		result = true;
                                //		TempData["mensaje"] = "La actualizacion del registro temporal fue exitoso";
                                //		return Json(result, JsonRequestBehavior.AllowGet);
                                //	}
                                //} /// 1
                            } // 2
                            else
                            {
                                //2
                                double Nuevopor2 = Convert.ToDouble(monlocal * 100 / localtot);
                                context.detallesComprasDistri.Add(new detallesComprasDistri
                                {
                                    idUsuario = usuario,
                                    idCentro = idCentro,
                                    por = Nuevopor2,
                                    monto = monto
                                });
                                int guardar = context.SaveChanges();

                                if (guardar > 0)
                                {
                                    result = true;
                                    TempData["mensaje"] = "La creación del registro temporal fue exitoso";
                                    return Json(result, JsonRequestBehavior.AllowGet);
                                }
                            } // 2
                        } //3
                    }
                } // 5
            }

            //	result = false;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult MuestraDetalleCompraDist()
        {
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            var datos = (from a in context.detallesComprasDistri
                         join b in context.centro_costo
                             on a.idCentro equals b.centcst_id
                         where a.idUsuario == usuario
                         select new
                         {
                             a.id,
                             a.idCentro,
                             nomccs = "(" + b.pre_centcst + ") " + b.centcst_nombre,
                             por = a.por != null ? a.por : 0,
                             a.monto
                         }).ToList();
            double sumapor = datos.Sum(item => item.por ?? 0);
            decimal sumamonto = datos.Sum(item => item.monto ?? 0);
            var data = new
            {
                datos,
                sumapor,
                sumamonto
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Movimientos(int idencab)
        {
            var pre_movimientos = (from mov in context.mov_contable
                                   join cta in context.cuenta_puc
                                       on mov.cuenta equals cta.cntpuc_id
                                   join ccs in context.centro_costo
                                       on mov.centro equals ccs.centcst_id
                                   where mov.id_encab == idencab && (mov.idparametronombre == 12 || mov.idparametronombre == 13)
                                   select new
                                   {
                                       mov.id_encab,
                                       centro = "(" + ccs.centcst_id + ") " + ccs.centcst_nombre,
                                       cuenta = "(" + cta.cntpuc_numero + ") " + cta.cntpuc_descp,
                                       cta.mov_cnt,
                                       mov.debito,
                                       mov.credito,
                                       mov.fec
                                   }).ToList();

            decimal sumaDe = pre_movimientos.Sum(item => item.debito);
            decimal sumaCr = pre_movimientos.Sum(item => item.credito);
            var movimientos = pre_movimientos.Select(c => new
            {
                c.id_encab,
                c.centro,
                c.cuenta,
                porcentaje = c.debito > 0 ? Convert.ToDouble(c.debito * 100 / sumaDe) :
                    c.credito > 0 ? Convert.ToDouble(c.credito * 100 / sumaCr) : 0,
                debito = c.debito > 0 ? "$ " + c.debito.ToString("N0") : "0",
                credito = c.credito > 0 ? "$ " + c.credito.ToString("N0") : "0",
                fecha = c.fec != null ? c.fec.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : ""
            }).ToList();

            return Json(movimientos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult eliminarItem(int iditem)
        {
            int eliminar = 0;
            detallesComprasDistri dist1 = context.detallesComprasDistri.FirstOrDefault(x => x.id == iditem);
            if (dist1 != null)
            {
                detallesComprasDistri itemEliminar = context.detallesComprasDistri.FirstOrDefault(m => m.id == iditem);
                if (itemEliminar != null)
                {
                    context.Entry(itemEliminar).State = EntityState.Deleted;
                    context.SaveChanges();
                    eliminar = context.SaveChanges();
                }

                //	context.Entry(itemEliminar).State = EntityState.Deleted;
                //	var eliminar = context.SaveChanges();
                if (eliminar > 0)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult eliminarTodo()
        {
            var respuesta = true;
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            detallesComprasDistri dist1 = context.detallesComprasDistri.FirstOrDefault(x => x.idUsuario == usuario);
            if (dist1 != null)
            {
                List<detallesComprasDistri> itemEliminar = context.detallesComprasDistri.Where(m => m.idUsuario == usuario).ToList();
                if (itemEliminar != null)
                {
                    for (int i = 0; i < itemEliminar.Count; i++)
                    {
                        foreach (detallesComprasDistri item in itemEliminar)
                        {
                            context.Entry(item).State = EntityState.Deleted;
                        }

                        try
                        {
                            int eliminar = context.SaveChanges();
                            respuesta = true;
                        }
                        catch (Exception e)
                        {
                            string error = e.Message;
                            respuesta = false;
                        }
                    }
                }
                //return Json(false, JsonRequestBehavior.AllowGet);
            }

            return Json(respuesta, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SumarPorcentaje(int? porce)
        {
            int? porlocal = porce;
            double suma = 0;
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            var data = (from a in context.detallesComprasDistri
                        where a.idUsuario == usuario
                        select new
                        {
                            a.id,
                            a.idCentro,
                            a.por,
                            a.monto
                        }).ToList();
            double sumaexiste = data.Sum(item => item.por ?? 0);
            if (porlocal != null)
            {
                suma = sumaexiste + porce ?? 0;
            }
            else
            {
                suma = sumaexiste;
            }

            return Json(suma, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SumarPorcentajeAcum()
        {
            double suma = 0;
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            var data = (from a in context.detallesComprasDistri
                        where a.idUsuario == usuario
                        select new
                        {
                            a.id,
                            a.idCentro,
                            a.por,
                            a.monto
                        }).ToList();
            double sumaexiste = data.Sum(item => item.por ?? 0);
            suma = sumaexiste;


            return Json(suma, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SumarMonto(decimal? monto)
        {
            decimal montolocal = monto ?? 0;
            decimal suma = 0;
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            var data = (from a in context.detallesComprasDistri
                        where a.idUsuario == usuario
                        select new
                        {
                            a.id,
                            a.idCentro,
                            a.por,
                            a.monto
                        }).ToList();
            decimal sumaexiste = data.Sum(item => item.monto ?? 0);
            if (montolocal > 0)
            {
                suma = sumaexiste + monto ?? 0;
            }
            else
            {
                suma = sumaexiste;
            }

            return Json(suma, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SumarMontoActual()
        {
            decimal suma = 0;
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            var data = (from a in context.detallesComprasDistri
                        where a.idUsuario == usuario
                        select new
                        {
                            a.id,
                            a.idCentro,
                            a.por,
                            a.monto
                        }).ToList();
            decimal sumaexiste = data.Sum(item => item.monto ?? 0);
            suma = sumaexiste;

            return Json(suma, JsonRequestBehavior.AllowGet);
        }

        public JsonResult TraeIva(int? nit)
        {

            icb_sysparameter siPerfileTributarioSimplificado = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P130");
            int PerfilSimplificado = Convert.ToInt32(siPerfileTributarioSimplificado.syspar_value);

            int PerfilTercero = 0;

            icb_terceros PerfileTributarioTercero = context.icb_terceros.FirstOrDefault(x => x.tercero_id == nit);
            if (PerfileTributarioTercero != null)
            {
                PerfilTercero = Convert.ToInt32(PerfileTributarioTercero.tpregimen_id);
            }

            if (PerfilSimplificado == PerfilTercero)
            {
                //Iva = 0;
                //RetIva = 0;
                //PorRetIva = 0;
                int data = 0;
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                icb_sysparameter siaplicaiva = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P70");
                decimal Iva = Convert.ToDecimal(siaplicaiva.syspar_value, miCultura);

                decimal data = Iva;
                return Json(data, JsonRequestBehavior.AllowGet);
            }


            //         var trae = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P70");
            //var SiIva = context.tercero_proveedor.Where(x => x.tercero_id == nit && x.exentoiva == true).FirstOrDefault();
            //if (SiIva != null)
            //{

            //	return Json(0, JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
            //	return Json(trae, JsonRequestBehavior.AllowGet);
            //}
            //var trae = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P70");
            //return Json(trae, JsonRequestBehavior.AllowGet);
        }

        public JsonResult TraeRet(int? ret_id, int? bod, int? nit, int? doc)
        {
            if (nit != null && doc != null && bod != null)
            {
                var deldoc = (from a in context.tp_doc_registros
                              where a.tpdoc_id == doc
                              select new
                              {
                                  a.sw,
                                  a.retencion
                              }).FirstOrDefault();
                var datater = (from a in context.tercero_proveedor
                               join c in context.perfiltributario
                                   on a.tpregimen_id equals c.tipo_regimenid
                               where a.tercero_id == nit && c.bodega == bod && c.sw == deldoc.sw
                               select new
                               {
                                   a.tercero_id,
                                   c.bodega,
                                   c.sw,
                                   a.retfuente
                                   //c.retfuente,
                               }).FirstOrDefault();


                if (datater.retfuente == "A")
                {
                    if (ret_id > 0 && bod > 0)
                    {
                        var Wtparamretenciones = (from u in context.tparamretenciones
                                                  where u.id == ret_id
                                                  select new
                                                  {
                                                      u.id,
                                                      u.tarifas,
                                                      u.basepesos
                                                  }).FirstOrDefault(); //var esRet = Convert.ToInt32(Request["retencion_id"]);

                        var Wtablaretenciones = (from u in context.tablaretenciones
                                                 where u.concepto == Wtparamretenciones.id //	where u.id == ret_id
                                                 select new
                                                 {
                                                     u.id,
                                                     u.concepto
                                                     //u.ctaimpuesto,
                                                     //u.ctaretencion,
                                                     //u.ctareteiva, 
                                                     //u.ctaica,
                                                     //u.ctaxpagar
                                                 }).FirstOrDefault();

                        var Wretencionesbodega = (from u in context.retencionesbodega
                                                  where u.idretencion == Wtablaretenciones.id && u.idbodega == bod
                                                  select new
                                                  {
                                                      u.id,
                                                      u.idretencion,
                                                      u.idbodega,
                                                      u.ctaiva,
                                                      u.ctareteiva,
                                                      u.ctaretencion,
                                                      u.ctareteica,
                                                      u.cuentaxpagar
                                                  }).FirstOrDefault();


                        var data = new
                        {
                            Wtablaretenciones,
                            Wtparamretenciones,
                            Wretencionesbodega,
                            deldoc
                        };

                        return Json(data, JsonRequestBehavior.AllowGet);
                    }

                    return Json(0, JsonRequestBehavior.AllowGet);
                }

                return Json(0, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult TraeRetIca(int? bodega, int? tercero, int? docu)
        {
            if (bodega > 0 && tercero > 0 && docu > 0)
            {
                var opcion1 = (from a in context.terceros_bod_ica
                               join b in context.acteco_tercero
                                   on a.idcodica equals b.acteco_id
                               join c in context.tercero_proveedor
                                   on b.acteco_id equals c.acteco_id
                               where a.bodega == bodega && b.acteco_estado && c.tercero_id == tercero
                               select new
                               {
                                   a.porcentaje
                               }).FirstOrDefault();

                if (opcion1 != null && opcion1.porcentaje > 0)
                {
                    decimal data = opcion1.porcentaje;
                    return Json(data, JsonRequestBehavior.AllowGet);
                }


                {
                    var opcion2 = (from a in context.tercero_proveedor
                                   join b in context.acteco_tercero
                                       on a.acteco_id equals b.acteco_id
                                   where a.tercero_id == tercero && b.acteco_estado
                                   select new
                                   {
                                       b.tarifa
                                   }).FirstOrDefault();
                    if (opcion2 != null && opcion2.tarifa > 0)
                    {
                        decimal data = opcion2.tarifa;
                        return Json(data, JsonRequestBehavior.AllowGet);
                    }

                    {
                        var opcion3 = (from a in context.tp_doc_registros
                                       where a.tpdoc_id == docu
                                       select new
                                       {
                                           a.retica
                                       }).FirstOrDefault();
                        if (opcion3 != null && opcion3.retica > 0)
                        {
                            float data = opcion3.retica;
                            return Json(data, JsonRequestBehavior.AllowGet);
                        }

                        return Json(0, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult TraeRetIva(int? docu)
        {
            if (docu > 0)
            {
                var opcion3 = (from a in context.tp_doc_registros
                               where a.tpdoc_id == docu
                               select new
                               {
                                   a.retiva
                               }).FirstOrDefault();
                if (opcion3 != null && opcion3.retiva > 0)
                {
                    float data = opcion3.retiva;
                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                return Json(0, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BuscarConf_de_Retencion(int? id)
        {
            var data = (from a in context.perfil_cuentas_documento
                        join b in context.centro_costo
                            on a.centro equals b.centcst_id
                        where a.id_perfil == id
                        select new
                        {
                            b.centcst_id,
                            b.centcst_nombre,
                            b.pre_centcst,
                            nombre = "(" + b.pre_centcst + ") " + b.centcst_nombre
                        }).Distinct().OrderBy(bn => bn.nombre).ToList();


            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public ActionResult sihaydis()
        {
            int Wuser = Convert.ToInt32(Session["user_usuarioid"]);
            //var data = (from a in context.detallesComprasDistri
            //			where a.idUsuario == Wuser
            //			select new
            //			{
            //				a.id
            //			}).ToList();
            List<detallesComprasDistri> data = context.detallesComprasDistri.Where(x => x.idUsuario == Wuser).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonResult validafacturaproveedor(int ter, string fac)
        {
            icb_sysparameter pardoc = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P131");
            int doc = Convert.ToInt32(pardoc.syspar_value);
            bool dataok = true;

            encab_documento data = context.encab_documento.FirstOrDefault(x =>
                x.nit == ter && x.tipo == doc && x.documento.Contains(fac));
            if (data == null)
            {
                dataok = false;
                return Json(dataok, JsonRequestBehavior.AllowGet);
            }

            dataok = true;
            return Json(dataok, JsonRequestBehavior.AllowGet);
        }


        public JsonResult CalcularRetencionesPorItemNew(decimal MontoAntesIva, int PerfilTributario, int ter)
        {
            icb_sysparameter pardoc = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P131");
            int doc = Convert.ToInt32(pardoc.syspar_value);

            icb_sysparameter siPerfileTributarioSimplificado = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P130");
            int PerfilSimplificado = Convert.ToInt32(siPerfileTributarioSimplificado.syspar_value);
            decimal Iva = 0;
            decimal PorRetIva = 0;

            decimal basecalculoIva = 0;
            decimal baseretiva = 0;
            decimal calculoRetIva = 0;


            if (PerfilSimplificado == PerfilTributario)
            {
                Iva = 0;
                PorRetIva = 0;
            }
            else
            {
                icb_sysparameter siaplicaiva = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P70");
                Iva = Convert.ToDecimal(siaplicaiva.syspar_value, miCultura);
                basecalculoIva = MontoAntesIva * (Iva / 100);

                tercero_proveedor CondicionesTributariasTercero = context.tercero_proveedor.FirstOrDefault(x => x.tercero_id == ter);
                //AplicaFuente = Convert.ToDecimal(CondicionesTributariasTercero.);

                tp_doc_registros basesiaplicaRetiva = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == doc);
                baseretiva = Convert.ToDecimal(basesiaplicaRetiva.baseiva, miCultura);
                if (baseretiva >= MontoAntesIva)
                {
                    tp_doc_registros siaplicaRetiva = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == doc);
                    PorRetIva = Convert.ToDecimal(siaplicaRetiva.retiva, miCultura);
                    calculoRetIva = MontoAntesIva * (PorRetIva / 100);
                }
            }


            return Json(0, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarPerfilPorBodegaProveedor(int bodega, int tipoD, int proveedor_id)
        {
            icb_terceros proveedorIva = context.icb_terceros.Where(d => d.tercero_id == proveedor_id).FirstOrDefault();
            //(from proveedor in context.icb_terceros
            //	where proveedor.tercero_id == proveedor_id
            //	select new {proveedor.tpregimen_id}).FirstOrDefault();
            string proveedorIva2 = "";
            int tpregimen = 0;
            if (proveedorIva != null)
            {
                tpregimen = proveedorIva.tpregimen_id != null ? proveedorIva.tpregimen_id.Value : 0;
                proveedorIva2 = "A";
                // if (proveedorIva.exentoiva)
                //	proveedorIva2 = "N";
                //else if (proveedorIva.exentoiva == false) proveedorIva2 = "A";
            }

            var data = (from b in context.perfil_contable_bodega
                        join t in context.perfil_contable_documento
                            on b.idperfil equals t.id
                        where b.idbodega == bodega && t.tipo == tipoD
                        select new
                        {
                            id = b.idperfil,
                            perfil = t.descripcion,
                            t.iva,
                            t.regimen_tercero
                        }).ToList();
            var data2 = data;
            int existeiva = data.Where(d => d.iva != null).Count();
            if (existeiva > 0)
            {
                data2 = data.Where(d => d.iva == proveedorIva2 && d.regimen_tercero == tpregimen).ToList();
            }

            if (data2.Count == 0)
            {
                data2 = data;
            }

            return Json(data2, JsonRequestBehavior.AllowGet);
        }

        public JsonResult NuevoBuscarPerfiladoTributario(int bodega, int tipoD, int proveedor_id)
        {
            var tercero_regimen_tributario_mas = (from tercero in context.icb_terceros
                                                  where tercero.tercero_id == proveedor_id
                                                  select new
                                                  {
                                                      tercero.tpregimen_id
                                                      //tercero.retica,
                                                      //tercero.retfuente,
                                                      //tercero.retiva
                                                  }).FirstOrDefault();

            var retenciones_regimen_tributario = (from retenciones in context.parametrizacion_retenciones
                                                  where retenciones.id_Bodega == bodega &&
                                                        retenciones.id == tipoD &&
                                                        retenciones.id_RegimenTributario == tercero_regimen_tributario_mas.tpregimen_id
                                                  select new
                                                  {
                                                      retenciones.id_Retencion
                                                  }).ToList();

            //var proveedorIva = (from proveedor in context.tercero_proveedor where proveedor.tercero_id == proveedor_id select new { proveedor.exentoiva, proveedor.tpregimen_id }).FirstOrDefault();
            //var proveedorIva2 = "";
            //var tpregimen = 0;
            //if (proveedorIva != null)
            //{
            //    tpregimen = proveedorIva.tpregimen_id;
            //    if (proveedorIva.exentoiva == true)
            //    {
            //        proveedorIva2 = "N";
            //    }
            //    else if (proveedorIva.exentoiva == false)
            //    {
            //        proveedorIva2 = "A";
            //    }
            //}
            var perfileContable = (from b in context.perfil_contable_bodega
                                   join t in context.perfil_contable_documento
                                       on b.idperfil equals t.id
                                   where b.idbodega == bodega && t.tipo == tipoD
                                   select new
                                   {
                                       id = b.idperfil,
                                       perfil = t.descripcion,
                                       t.iva,
                                       t.regimen_tercero
                                   }).ToList();


            var data = new
            {
                perfileContable,
                retenciones_regimen_tributario
            };
            //var existeiva = data.Where(d => d.iva != null).Count();
            //if (existeiva > 0)
            //{
            //    data2 = data.Where(d => d.iva == proveedorIva2 && d.regimen_tercero == tpregimen).ToList();
            //}

            //if (data2.Count == 0)
            //{
            //    data2 = data;
            //}

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}


//valor aplicado tengo que actualizarlo en la factura a la que estoy generando la nota
//buscas la factura
////////////if (!string.IsNullOrWhiteSpace(ncm.documento) && ncm.prefijo != null)
////////////{
////////////	var numerodocumento = Convert.ToInt32(ncm.documento);
////////////	var factura = context.encab_documento.Where(d => d.idencabezado == ncm.prefijo && d.numero == numerodocumento).FirstOrDefault();

////////////	if (factura != null)
////////////	{
////////////		var valoraplicado = Convert.ToInt32(factura.valor_aplicado) != null ? Convert.ToInt32(factura.valor_aplicado) : 0;

////////////		var nuevovalor = Convert.ToDecimal(valoraplicado) + Convert.ToDecimal(ncm.valor_total);
////////////		factura.valor_aplicado = Convert.ToDecimal(nuevovalor);
////////////		context.Entry(factura).State = EntityState.Modified;
////////////		// si al crear la nota credito se selecciono factura se debe guardar el registro tambien en cruce documentos, de lo contrario NO
////////////		//Cruce documentos
////////////		//cruce_documentos cd = new cruce_documentos();
////////////		//cd.idtipo = ncm.tipo;
////////////		//cd.numero = consecutivo;
////////////		//cd.idtipoaplica = Convert.ToInt32(ncm.tipofactura);
////////////		//cd.numeroaplica = Convert.ToInt32(ncm.documento);
////////////		//cd.valor = Convert.ToDecimal(ncm.valor_total);
////////////		//cd.fecha = DateTime.Now;
////////////		//cd.fechacruce = DateTime.Now;
////////////		//cd.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
////////////		//context.cruce_documentos.Add(cd);
////////////	}
////////////}


#region para borrar

//			//consecutivo
//			var grupo = context.grupoconsecutivos.FirstOrDefault(x => x.documento_id == cgm.tipo && x.bodega_id == cgm.bodega);
//			if (grupo != null)
//			{
//				DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
//				var consecutivo = doc.BuscarConsecutivo(grupo.grupo);

//				//Encabezado documento
//				encab_documento encabezado = new encab_documento();
//				encabezado.tipo = cgm.tipo;
//				encabezado.numero = consecutivo;
//				encabezado.nit = cgm.nit;
//				encabezado.fecha = Convert.ToDateTime(cgm.fecha);// DateTime.Now;
//				encabezado.fpago_id = cgm.fpago_id;
//				encabezado.vencimiento = Convert.ToDateTime(cgm.vencimiento);
//				encabezado.valor_total = Convert.ToDecimal(cgm.valor_total);
//				encabezado.iva = Convert.ToDecimal(cgm.iva);
//				encabezado.retencion = Convert.ToDecimal(cgm.retencion);
//				encabezado.retencion_ica = Convert.ToDecimal(cgm.retencion_ica);
//				encabezado.retencion_iva = Convert.ToDecimal(cgm.retencion_iva);
//				encabezado.costo = 0;
//				encabezado.vendedor = Convert.ToInt32(cgm.vendedor);
//				encabezado.valor_aplicado = 0;
//				encabezado.anulado = false;
//				encabezado.documento = cgm.numFactura;
//				//encabezado.//prefijo = ncm.prefijo;
//				encabezado.valor_mercancia = Convert.ToDecimal(cgm.valorFactura);
//				encabezado.notas = cgm.nota1;
//				encabezado.nota1 = cgm.nota1;
//				encabezado.impoconsumo = 0;
//				encabezado.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
//				encabezado.fec_creacion = DateTime.Now;
//				encabezado.porcen_retencion = float.Parse(cgm.por_retencion, System.Globalization.CultureInfo.InvariantCulture);//esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal
//				encabezado.porcen_reteiva = float.Parse(cgm.por_retencion_iva, System.Globalization.CultureInfo.InvariantCulture);//esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal
//				encabezado.porcen_retica = float.Parse(cgm.por_retencion_ica, System.Globalization.CultureInfo.InvariantCulture);//esto se utiliza para que al guardar el dato en la BD me quede con la "," del decimal
//				encabezado.concepto = cgm.concepto;
//				encabezado.concepto2 = cgm.concepto2;
//				encabezado.perfilcontable = Convert.ToInt32(cgm.perfilcontable);
//				encabezado.bodega = Convert.ToInt32(cgm.bodega);
//				encabezado.estado = true;
//				encabezado.tipodocprov = Convert.ToString(cgm.prefijoFac);
//				encabezado.idconcepretencion = Convert.ToInt32(cgm.retencion_id);
//				// Agrega a Encabezado encab_documentos
//				context.encab_documento.Add(encabezado);
//				//movimiento contable
//				//buscamos en perfil cuenta documento, por medio del perfil seleccionado
//				var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
//												  join nombreParametro in context.paramcontablenombres
//												  on perfil.id_nombre_parametro equals nombreParametro.id
//												  join cuenta in context.cuenta_puc
//												  on perfil.cuenta equals cuenta.cntpuc_id
//												  join dis in context.detallesComprasDistri
//												  on perfil.centro equals dis.idCentro into al
//												  from dis in al.DefaultIfEmpty()
//												  where perfil.id_perfil == cgm.perfilcontable && !((perfil.id_nombre_parametro == 12 || perfil.id_nombre_parametro == 13) && dis.monto == null)
//												  select new
//												  {
//													  perfil.id,
//													  perfil.id_nombre_parametro,
//													  perfil.cuenta,
//													  perfil.centro,
//													  perfil.id_perfil,
//													  nombreParametro.descripcion_parametro,
//													  cuenta.cntpuc_numero,
//													  dis.monto
//												  }).ToList();

//				var secuencia = 1;
//				decimal totalDebitos = 0;
//				decimal totalCreditos = 0;
//				var cont = 0;

//				List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();
//				List<cuentas_valores> ids_cuentas_valores = new List<cuentas_valores>();
//				var centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
//				var idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
//				foreach (var parametro in parametrosCuentasVerificar)
//				{
//					var descripcionParametro = context.paramcontablenombres.FirstOrDefault(x => x.id == parametro.id_nombre_parametro).descripcion_parametro;
//					var buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);

//					if (buscarCuenta != null)
//					{
//						if (parametro.id_nombre_parametro == 1
//							|| (parametro.id_nombre_parametro == 2 && Convert.ToDecimal(cgm.iva) != 0)
//							|| (parametro.id_nombre_parametro == 3 && Convert.ToDecimal(cgm.retencion) != 0)
//							|| (parametro.id_nombre_parametro == 4 && Convert.ToDecimal(cgm.por_retencion_iva) != 0)
//							|| (parametro.id_nombre_parametro == 5 && Convert.ToDecimal(cgm.retencion_ica) != 0)
//							|| parametro.id_nombre_parametro == 12
//							|| parametro.id_nombre_parametro == 13)
//						{
//							mov_contable movNuevo = new mov_contable();
//							movNuevo.id_encab = cgm.id_encab;
//							movNuevo.seq = secuencia;
//							movNuevo.idparametronombre = parametro.id_nombre_parametro;
//							movNuevo.cuenta = parametro.cuenta;
//							movNuevo.centro = parametro.centro;
//							movNuevo.fec = DateTime.Now;
//							movNuevo.fec_creacion = DateTime.Now;
//							movNuevo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
//							movNuevo.detalle = cgm.nota1;

//							var info = context.cuenta_puc.Where(i => i.cntpuc_id == parametro.cuenta).FirstOrDefault();

//							if (info.tercero == true)
//							{
//								movNuevo.nit = cgm.nit;
//							}
//							else
//							{
//								var tercero = context.icb_terceros.Where(t => t.doc_tercero == "0").FirstOrDefault();
//								movNuevo.nit = tercero.tercero_id;
//							}

//							// las siguientes validaciones se hacen dependiendo de la cuenta que esta moviendo la Compra General, para guardar la informacion acorde
//							#region Parametro 1
//							if (parametro.id_nombre_parametro == 1)
//							{
//								movNuevo.cuenta = cgm.ctaxpagar;
//								if (info.manejabase == true)
//								{
//									movNuevo.basecontable = Convert.ToDecimal(cgm.valorFactura);
//								}
//								else
//								{
//									movNuevo.basecontable = 0;
//								}

//								if (info.documeto == true)
//								{
//									movNuevo.documento = cgm.numFactura;
//								}

//								if (buscarCuenta.concepniff == 1)
//								{
//									movNuevo.credito = Convert.ToDecimal(cgm.valor_total);
//									movNuevo.debito = 0;

//									movNuevo.creditoniif = Convert.ToDecimal(cgm.valor_total);
//									movNuevo.debitoniif = 0;
//								}

//								if (buscarCuenta.concepniff == 4)
//								{
//									movNuevo.creditoniif = Convert.ToDecimal(cgm.valor_total);
//									movNuevo.debitoniif = 0;
//								}

//								if (buscarCuenta.concepniff == 5)
//								{
//									movNuevo.credito = Convert.ToDecimal(cgm.valor_total);
//									movNuevo.debito = 0;
//								}
//							}
//							#endregion

//							#region Parametro 2
//							if (parametro.id_nombre_parametro == 2)
//							{
//								movNuevo.cuenta = cgm.ctaimpuesto;
//								if (info.manejabase == true)
//								{
//									movNuevo.basecontable = Convert.ToDecimal(cgm.valorFactura);
//								}
//								else
//								{
//									movNuevo.basecontable = 0;
//								}

//								if (info.documeto == true)
//								{
//									movNuevo.documento = cgm.numFactura;
//								}

//								if (buscarCuenta.concepniff == 1)
//								{
//									movNuevo.credito = 0;
//									movNuevo.debito = Convert.ToDecimal(cgm.iva);
//									movNuevo.creditoniif = 0;
//									movNuevo.debitoniif = Convert.ToDecimal(cgm.iva);
//								}

//								if (buscarCuenta.concepniff == 4)
//								{
//									movNuevo.creditoniif = 0;
//									movNuevo.debitoniif = Convert.ToDecimal(cgm.iva);
//								}

//								if (buscarCuenta.concepniff == 5)
//								{
//									movNuevo.credito = 0;
//									movNuevo.debito = Convert.ToDecimal(cgm.iva);
//								}
//							}
//							#endregion

//							#region Parametro 3
//							if (parametro.id_nombre_parametro == 3)
//							{
//								movNuevo.cuenta = cgm.ctaretencion;
//								if (info.manejabase == true)
//								{
//									movNuevo.basecontable = Convert.ToDecimal(cgm.valorFactura);
//								}
//								else
//								{
//									movNuevo.basecontable = 0;
//								}

//								if (info.documeto == true)
//								{
//									movNuevo.documento = cgm.numFactura;
//								}

//								if (buscarCuenta.concepniff == 1)
//								{
//									movNuevo.credito = Convert.ToDecimal(cgm.retencion);
//									movNuevo.debito = 0;

//									movNuevo.creditoniif = Convert.ToDecimal(cgm.retencion);
//									movNuevo.debitoniif = 0;
//								}

//								if (buscarCuenta.concepniff == 4)
//								{
//									movNuevo.creditoniif = Convert.ToDecimal(cgm.retencion);
//									movNuevo.debitoniif = 0;
//								}

//								if (buscarCuenta.concepniff == 5)
//								{
//									movNuevo.credito = Convert.ToDecimal(cgm.retencion);
//									movNuevo.debito = 0;
//								}
//							}
//							#endregion

//							#region Parametro 4
//							if (parametro.id_nombre_parametro == 4)
//							{
//								movNuevo.cuenta = cgm.ctareteiva;
//								if (info.manejabase == true)
//								{
//									movNuevo.basecontable = Convert.ToDecimal(cgm.iva);
//								}
//								else
//								{
//									movNuevo.basecontable = 0;
//								}

//								if (info.documeto == true)
//								{
//									movNuevo.documento = cgm.numFactura;
//								}

//								if (buscarCuenta.concepniff == 1)
//								{
//									movNuevo.credito = Convert.ToDecimal(cgm.retencion_iva);
//									movNuevo.debito = 0;

//									movNuevo.creditoniif = Convert.ToDecimal(cgm.retencion_iva);
//									movNuevo.debitoniif = 0;
//								}

//								if (buscarCuenta.concepniff == 4)
//								{
//									movNuevo.creditoniif = Convert.ToDecimal(cgm.retencion_iva);
//									movNuevo.debitoniif = 0;
//								}

//								if (buscarCuenta.concepniff == 5)
//								{
//									movNuevo.credito = Convert.ToDecimal(cgm.retencion_iva);
//									movNuevo.debito = 0;
//								}
//							}
//							#endregion

//							#region Parametro 5
//							if (parametro.id_nombre_parametro == 5)
//							{
//								movNuevo.cuenta = cgm.ctaica;
//								if (info.manejabase == true)
//								{
//									movNuevo.basecontable = Convert.ToDecimal(cgm.valorFactura);
//								}
//								else
//								{
//									movNuevo.basecontable = 0;
//								}

//								if (info.documeto == true)
//								{
//									movNuevo.documento = cgm.numFactura;
//								}

//								if (buscarCuenta.concepniff == 1)
//								{
//									movNuevo.credito = Convert.ToDecimal(cgm.retencion_ica);
//									movNuevo.debito = 0;
//									movNuevo.creditoniif = Convert.ToDecimal(cgm.retencion_ica);
//									movNuevo.debitoniif = 0;
//								}

//								if (buscarCuenta.concepniff == 4)
//								{
//									movNuevo.creditoniif = Convert.ToDecimal(cgm.retencion_ica);
//									movNuevo.debitoniif = 0;
//								}

//								if (buscarCuenta.concepniff == 5)
//								{
//									movNuevo.credito = Convert.ToDecimal(cgm.retencion_ica);
//									movNuevo.debito = 0;
//								}
//							}
//							#endregion
//							//	var cont = 0;
//							#region  Parametro 13
//							//if (parametro.id_nombre_parametro == 12 || parametro.id_nombre_parametro == 13)
//							//{

//							//	foreach (var centr in distribucion)
//							//	{
//							//		movNuevo.id_encab = cgm.id_encab;
//							//		movNuevo.seq = secuencia;
//							//		movNuevo.idparametronombre = parametro.id_nombre_parametro;
//							//		movNuevo.cuenta = parametro.cuenta;
//							//		movNuevo.centro = Convert.ToInt32(centr.idCentro);
//							//		movNuevo.fec = DateTime.Now;
//							//		movNuevo.fec_creacion = DateTime.Now;
//							//		movNuevo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
//							//		movNuevo.detalle = cgm.nota1;

//							//		var info2 = context.cuenta_puc.Where(i => i.cntpuc_id == parametro.cuenta).FirstOrDefault();

//							//		if (info2.tercero == true)
//							//		{
//							//			movNuevo.nit = cgm.nit;
//							//		}
//							//		else
//							//		{
//							//			var tercero = context.icb_terceros.Where(t => t.doc_tercero == "0").FirstOrDefault();
//							//			movNuevo.nit = tercero.tercero_id;
//							//		}


//							//		if (info.manejabase == true)
//							//		{
//							//			movNuevo.basecontable = Convert.ToDecimal(centr.monto);
//							//		}
//							//		else
//							//		{
//							//			movNuevo.basecontable = 0;
//							//		}

//							//		if (info.documeto == true)
//							//		{
//							//			movNuevo.documento = cgm.numFactura;
//							//		}

//							//		if (buscarCuenta.concepniff == 1)
//							//		{
//							//			movNuevo.credito = 0;
//							//			movNuevo.debito = Convert.ToDecimal(centr.monto);

//							//			movNuevo.creditoniif = 0;
//							//			movNuevo.debitoniif = Convert.ToDecimal(centr.monto);
//							//		}

//							//		if (buscarCuenta.concepniff == 4)
//							//		{
//							//			movNuevo.creditoniif = 0;
//							//			movNuevo.debitoniif = Convert.ToDecimal(centr.monto);
//							//		}

//							//		if (buscarCuenta.concepniff == 5)
//							//		{
//							//			movNuevo.credito = 0;
//							//			movNuevo.debito = Convert.ToDecimal(centr.monto);
//							//		}
//							//	}
//							//	secuencia++;
//							//	cont++;
//							//}
//							#endregion

//							#region  Parametro 12 - 13 
//							if (parametro.id_nombre_parametro == 12 || parametro.id_nombre_parametro == 13)
//							{
//								if (info.manejabase == true)
//								{
//									movNuevo.basecontable = Convert.ToDecimal(parametro.monto);
//								}
//								else
//								{
//									movNuevo.basecontable = 0;
//								}

//								if (info.documeto == true)
//								{
//									movNuevo.documento = cgm.numFactura;
//								}

//								if (buscarCuenta.concepniff == 1)
//								{
//									movNuevo.credito = 0;
//									movNuevo.debito = Convert.ToDecimal(parametro.monto);

//									movNuevo.creditoniif = 0;
//									movNuevo.debitoniif = Convert.ToDecimal(parametro.monto);
//								}

//								if (buscarCuenta.concepniff == 4)
//								{
//									movNuevo.creditoniif = 0;
//									movNuevo.debitoniif = Convert.ToDecimal(parametro.monto);
//								}

//								if (buscarCuenta.concepniff == 5)
//								{
//									movNuevo.credito = 0;
//									movNuevo.debito = Convert.ToDecimal(parametro.monto);
//								}
//							}
//							#endregion


//							#region  Parametro 11
//							//if (parametro.id_nombre_parametro == 11)
//							//{
//							//	if (info.manejabase == true)
//							//	{
//							//		movNuevo.basecontable = Convert.ToDecimal(cgm.valorFactura);
//							//	}
//							//	else
//							//	{
//							//		movNuevo.basecontable = 0;
//							//	}

//							//	if (info.documeto == true)
//							//	{
//							//		movNuevo.documento = cgm.numFactura;
//							//	}

//							//	if (buscarCuenta.concepniff == 1)
//							//	{
//							//		movNuevo.credito = 0;
//							//		movNuevo.debito = Convert.ToDecimal(cgm.valorFactura);

//							//		movNuevo.creditoniif = 0;
//							//		movNuevo.debitoniif = Convert.ToDecimal(cgm.valorFactura);
//							//	}

//							//	if (buscarCuenta.concepniff == 4)
//							//	{
//							//		movNuevo.creditoniif = 0;
//							//		movNuevo.debitoniif = Convert.ToDecimal(cgm.valorFactura);
//							//	}

//							//	if (buscarCuenta.concepniff == 5)
//							//	{
//							//		movNuevo.credito = 0;
//							//		movNuevo.debito = Convert.ToDecimal(cgm.valorFactura);
//							//	}
//							//}
//							#endregion

//							secuencia++;

//							//Cuentas valores
//							var buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x => x.centro == parametro.centro && x.cuenta == parametro.cuenta && x.nit == movNuevo.nit);
//							if (buscar_cuentas_valores != null)
//							{
//								buscar_cuentas_valores.debito += movNuevo.debito;
//								buscar_cuentas_valores.credito += movNuevo.credito;
//								buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
//								buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
//								context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
//							}
//							else
//							{
//								var fechaHoy = DateTime.Now;
//								var crearCuentaValor = new cuentas_valores();
//								crearCuentaValor.ano = fechaHoy.Year;
//								crearCuentaValor.mes = fechaHoy.Month;
//								crearCuentaValor.cuenta = movNuevo.cuenta;
//								crearCuentaValor.centro = movNuevo.centro;
//								crearCuentaValor.nit = movNuevo.nit;
//								crearCuentaValor.debito = movNuevo.debito;
//								crearCuentaValor.credito = movNuevo.credito;
//								crearCuentaValor.debitoniff = movNuevo.debitoniif;
//								crearCuentaValor.creditoniff = movNuevo.creditoniif;
//								context.cuentas_valores.Add(crearCuentaValor);
//							}
//							/// Agraga a Cuentas Valores
//							context.mov_contable.Add(movNuevo);

//							totalCreditos += movNuevo.credito;
//							totalDebitos += movNuevo.debito;
//							// Agrega a lista Descuadrados
//							listaDescuadrados.Add(new DocumentoDescuadradoModel()
//							{
//								NumeroCuenta = "(" + info.cntpuc_numero + ")" + info.cntpuc_descp,
//								DescripcionParametro = descripcionParametro,
//								ValorDebito = movNuevo.debito,
//								ValorCredito = movNuevo.credito
//							});
//						}
//					}
//				}


//				if (totalDebitos != totalCreditos)
//				{
//					TempData["mensaje_error"] = "El documento no tiene los movimientos calculados correctamente, verifique el perfil del documento";

//					ViewBag.documentoSeleccionado = encabezado.tipo;
//					ViewBag.bodegaSeleccionado = encabezado.bodega;
//					ViewBag.perfilSeleccionado = encabezado.perfilcontable;

//					ViewBag.documentoDescuadrado = listaDescuadrados;
//					ViewBag.calculoDebito = totalDebitos;
//					ViewBag.calculoCredito = totalCreditos;

//				//	dbTran.Rollback();

//					listas();
//					return View(cgm);
//				}
//				else
//				{
//					context.SaveChanges();
//				//	dbTran.Commit();
//					TempData["mensaje"] = "registro creado correctamente";
//					DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
//					// Agrega a Consecutivos						
//					doc.ActualizarConsecutivo(grupo.grupo, consecutivo);

//					return RedirectToAction("Create");
//				}

//			}
//			else
//			{
//				TempData["mensaje_error"] = "no hay consecutivo";
//			}

//	//	}
//	//	catch (DbEntityValidationException ex)
//	//	{
//	//		dbTran.Rollback();
//	//		throw;
//	//	}
//	//}
//}
//else
//{
//	TempData["mensaje_error"] = "No fue posible guardar el registro, por favor valide";
//	var errors = ModelState.Select(x => x.Value.Errors)
//						   .Where(y => y.Count > 0)
//						   .ToList();
//}

#endregion