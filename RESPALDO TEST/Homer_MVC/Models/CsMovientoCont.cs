using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Homer_MVC.Models
    {
    public class CsMovientoCont
        {
        private readonly Iceberg_Context context = new Iceberg_Context();

        private CultureInfo Cultureinfo = new CultureInfo("is-IS");

        public ListaMovContable MovContableOperacionIn(int perfilcont, int encabezado,long numero,int nit, int numOT, int user_id) {

            ListaMovContable resultado = new ListaMovContable();
            resultado.movimientos = new List<moviContable>();
            resultado.valido = false;           
            var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                              join nombreParametro in context.paramcontablenombres
                                                  on perfil.id_nombre_parametro equals nombreParametro.id
                                              join cuenta in context.cuenta_puc
                                                  on perfil.cuenta equals cuenta.cntpuc_id
                                           where perfil.id_perfil == perfilcont
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
            decimal ivaEncabezado = 0;
            decimal costoPromedioTotal = 0;
            decimal costoEncabezado = 0;
            decimal totalCreditos = 0, totalDebitos = 0;
            var modelo = context.tencabezaorden.Where(x => x.id == numOT).FirstOrDefault();

            int tipotarifa = Convert.ToInt32(context.icb_sysparameter.Where(s => s.syspar_cod == "P147").Select(z => z.syspar_value).FirstOrDefault());
            var operacionesinternas = context.tdetallemanoobraot.Where(x => x.tipotarifa == tipotarifa && x.respuestainterno == true && x.idorden == modelo.id).ToList();
            int secuencia = 0;

            foreach (var item in operacionesinternas)
                {

                decimal cr = Convert.ToDecimal(item.tiempo, Cultureinfo) * item.valorunitario;
                decimal valor_totalenca = Convert.ToDecimal(item.tiempo, Cultureinfo) * item.valorunitario;
                 ivaEncabezado = 1;
                 costoPromedioTotal = 1;
                 costoEncabezado = 1;
                List<DocumentoDescuadradoModel>  listaDescuadrados =  new List<DocumentoDescuadradoModel> ();
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
                        Convert.ToDecimal(ivaEncabezado, Cultureinfo) != 0
                        || parametro.id_nombre_parametro == 9 &&
                        Convert.ToDecimal(costoPromedioTotal, Cultureinfo) != 0 //costo promedio
                        || parametro.id_nombre_parametro == 20 &&
                        Convert.ToDecimal(costoPromedioTotal, Cultureinfo) != 0 //costo promedio
                        || parametro.id_nombre_parametro == 11 &&
                        Convert.ToDecimal(costoEncabezado, Cultureinfo) != 0
                        || parametro.id_nombre_parametro == 12 &&
                        Convert.ToDecimal(costoPromedioTotal, Cultureinfo) != 0) //costo promedio
                        {
                            moviContable movNuevo = new moviContable();

                            movNuevo.id_encab = encabezado;
                            movNuevo.seq = secuencia;
                            movNuevo.idparametronombre = parametro.id_nombre_parametro;
                            movNuevo.cuenta = parametro.cuenta;
                            movNuevo.tipotarifa = item.tipotarifa;
                            movNuevo.centro = item.tipotarifa == 2
                                ? parametro.id_nombre_parametro == 11
                                    ? Convert.ToInt32(item.idcentro)
                                    : parametro.id_nombre_parametro == 12
                                        ? Convert.ToInt32(item.idcentro)
                                        : parametro.centro
                             : parametro.centro;
                            movNuevo.fec = DateTime.Now;
                            movNuevo.fec_creacion = DateTime.Now;
                            movNuevo.tipo_tarifa =  Convert.ToInt32(item.tipotarifa);
                            movNuevo.userid_creacion =  Convert.ToInt32(user_id);
                            movNuevo.documento = Convert.ToString(modelo.id);

                        cuenta_puc info = context.cuenta_puc
                            .Where(a => a.cntpuc_id == parametro.cuenta).FirstOrDefault();

                        if (info.tercero)
                            {

                            movNuevo.nit = modelo.tercero;
                            }
                        else
                            {
                            icb_terceros tercero = context.icb_terceros
                                .Where(t => t.doc_tercero == "0").FirstOrDefault();
                            movNuevo.nit = tercero.tercero_id;
                            }

                            #region Inventario

                            if (parametro.id_nombre_parametro == 9 ||
                                parametro.id_nombre_parametro == 20)
                            {
                                movNuevo.id_encab = encabezado;
                                movNuevo.seq = secuencia;
                                movNuevo.idparametronombre = parametro.id_nombre_parametro;

                                movNuevo.cuenta = parametro.cuenta;
                                movNuevo.centro = Convert.ToInt32(item.idcentro);
                                movNuevo.fec = DateTime.Now;
                                movNuevo.fec_creacion = DateTime.Now;
                                movNuevo.userid_creacion =
                                    Convert.ToInt32(user_id);
                                movNuevo.documento = Convert.ToString(numero);

                                cuenta_puc infoReferencia = context.cuenta_puc
                                    .Where(a => a.cntpuc_id == parametro.cuenta)
                                    .FirstOrDefault();
                                if (infoReferencia.manejabase)
                                {
                                    movNuevo.basecontable =
                                        Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                }
                                else
                                {
                                    movNuevo.basecontable = 0;
                                }

                                if (infoReferencia.documeto)
                                {
                                    movNuevo.documento = Convert.ToString(numero);
                                }

                                if (infoReferencia.concepniff == 1)
                                {
                                    movNuevo.credito = Convert.ToDecimal(cr, Cultureinfo);
                                    movNuevo.debito = 0;

                                    movNuevo.creditoniif = Convert.ToDecimal(cr, Cultureinfo);
                                    movNuevo.debitoniif = 0;
                                }

                                if (infoReferencia.concepniff == 4)
                                {
                                    movNuevo.creditoniif = Convert.ToDecimal(cr, Cultureinfo);
                                    movNuevo.debitoniif = 0;
                                }

                                if (infoReferencia.concepniff == 5)
                                {
                                    movNuevo.credito = Convert.ToDecimal(cr, Cultureinfo);
                                    movNuevo.debito = 0;
                                }
                                movNuevo.detalle =
                                       "Facturacion de repuestos con consecutivo " +
                                       numero;
                                movNuevo.estado = true;
                            }
                            #endregion
                            #region Costo

                            if (parametro.id_nombre_parametro == 12)
                            {
                                movNuevo.id_encab = encabezado;
                                movNuevo.seq = secuencia;
                                movNuevo.idparametronombre = parametro.id_nombre_parametro;
                                movNuevo.cuenta = parametro.cuenta;
                                movNuevo.centro = item.tipotarifa == 2
                                    ? parametro.id_nombre_parametro == 12
                                        ? Convert.ToInt32(item.idcentro)
                                        : parametro.centro
                                    : parametro.centro;

                                movNuevo.fec = DateTime.Now;
                                movNuevo.fec_creacion = DateTime.Now;
                                movNuevo.userid_creacion =
                                    Convert.ToInt32(user_id);

                                if (info.manejabase)
                                {
                                    movNuevo.basecontable =
                                        Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                }
                                else
                                {
                                    movNuevo.basecontable = 0;
                                }

                                if (info.documeto)
                                {
                                    movNuevo.documento = Convert.ToString(encabezado);
                                }

                                if (buscarCuenta.concepniff == 1)
                                {
                                    movNuevo.credito = 0;
                                    movNuevo.debito = Convert.ToDecimal(cr, Cultureinfo);

                                    movNuevo.creditoniif = 0;
                                    movNuevo.debitoniif = Convert.ToDecimal(cr, Cultureinfo);
                                }

                                if (buscarCuenta.concepniff == 4)
                                {
                                    movNuevo.creditoniif = 0;
                                    movNuevo.debitoniif = Convert.ToDecimal(cr, Cultureinfo);
                                }

                                if (buscarCuenta.concepniff == 5)
                                {
                                    movNuevo.credito = 0;
                                    movNuevo.debito = Convert.ToDecimal(cr, Cultureinfo);
                                }
                                movNuevo.detalle =
                                        "Liquidacion de Orden de taller Operacion tarifa interna " + numero;
                                movNuevo.estado = true;
                            }
                            #endregion
                            
                            secuencia++;
                            totalCreditos += Math.Round(movNuevo.credito);
                            totalDebitos += Math.Round(movNuevo.debito);
                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                                {
                                NumeroCuenta =
                                    "(" + buscarCuenta.cntpuc_numero + ")" +
                                    buscarCuenta.cntpuc_descp,
                                DescripcionParametro = descripcionParametro,
                                ValorDebito = movNuevo.debito,
                                ValorCredito = movNuevo.credito
                                });
                            resultado.movimientos.Add(movNuevo);
                            resultado.cantidad = resultado.cantidad + 1;
                            }
                        resultado.descuadrados=listaDescuadrados;

                    }
                }


                }
          

            if (Math.Round(totalDebitos) != Math.Round(totalCreditos))
                {
                resultado.valido= false;
                }
            else
                {
                resultado.valido = true;
                }

            return resultado;

            }

        public ListaMovContable MovContableRepuestoIn(int perfilcont, int encabezado, long numero, int nit, int numOT, int user_id) {
            
        //    perfilcont = 5087;
            var resultado = new ListaMovContable();
            resultado.movimientos = new List<moviContable>();
            resultado.valido = false;
            var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                              join nombreParametro in context.paramcontablenombres
                                                  on perfil.id_nombre_parametro equals nombreParametro.id
                                              join cuenta in context.cuenta_puc
                                                  on perfil.cuenta equals cuenta.cntpuc_id
                                              where perfil.id_perfil == perfilcont
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

            var modelo = context.tencabezaorden.Where(x => x.id == numOT).FirstOrDefault();
            int tipotarifa = Convert.ToInt32(context.icb_sysparameter.Where(s => s.syspar_cod == "P147").Select(z => z.syspar_value).FirstOrDefault());
            
            var repuestos = context.tdetallerepuestosot.Where(x => x.idorden == modelo.id && x.tipotarifa == tipotarifa).ToList();

            decimal ivaEncabezado = 0;
            decimal costoPromedioTotal = 0;
            decimal costoEncabezado = 0;
            decimal totalCreditos = 0, totalDebitos = 0;
            int secuencia = 0;
            List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();
            foreach (var item in repuestos)
                {

                decimal vrto = item.cantidad * item.valorunitario;
                decimal dcto = (vrto * item.pordescto) / 100;
                decimal vrgen = vrto - dcto;
  
       

                vw_promedio vwPromedio = context.vw_promedio.FirstOrDefault(x =>
                                       x.codigo == item.idrepuesto && x.ano == DateTime.Now.Year &&
                                       x.mes == DateTime.Now.Month);
                decimal? costoReferencia = vwPromedio.Promedio;
                decimal baseUnitario = Convert.ToDecimal(costoReferencia, Cultureinfo);
                decimal? cr = costoReferencia * Convert.ToDecimal(item.cantidad, Cultureinfo);
                decimal ivaReferencia = (Convert.ToDecimal(cr, Cultureinfo) * item.poriva) / 100;
                decimal cr2 = Convert.ToDecimal(cr, Cultureinfo) +  ivaReferencia;
                decimal valor_totalenca = vrgen;
                 ivaEncabezado = 1;
                 costoPromedioTotal = 1;
                 costoEncabezado = 1;
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
                        Convert.ToDecimal(ivaEncabezado, Cultureinfo) != 0
                        || parametro.id_nombre_parametro == 9 &&
                        Convert.ToDecimal(costoPromedioTotal, Cultureinfo) != 0 //costo promedio
                        || parametro.id_nombre_parametro == 20 &&
                        Convert.ToDecimal(costoPromedioTotal, Cultureinfo) != 0 //costo promedio
                        || parametro.id_nombre_parametro == 11 &&
                        Convert.ToDecimal(costoEncabezado, Cultureinfo) != 0
                        || parametro.id_nombre_parametro == 12 &&
                        Convert.ToDecimal(costoPromedioTotal, Cultureinfo) != 0) //costo promedio
                        {
                            moviContable movNuevo = new moviContable();

                            movNuevo.id_encab = encabezado;
                            movNuevo.seq = secuencia;
                            movNuevo.idparametronombre = parametro.id_nombre_parametro;
                            movNuevo.cuenta = parametro.cuenta;
                            movNuevo.centro = item.tipotarifa == 2
                                ? parametro.id_nombre_parametro == 11
                                    ? Convert.ToInt32(item.idcentro)
                                    : parametro.id_nombre_parametro == 12
                                        ? Convert.ToInt32(item.idcentro)
                                        : parametro.centro
                                : parametro.centro;
                            movNuevo.fec = DateTime.Now;
                            movNuevo.fec_creacion = DateTime.Now;
                            movNuevo.tipo_tarifa =  Convert.ToInt32(item.tipotarifa);
                            movNuevo.userid_creacion = Convert.ToInt32(user_id);
                           movNuevo.documento = Convert.ToString(modelo.id);

                        cuenta_puc info = context.cuenta_puc
                            .Where(a => a.cntpuc_id == parametro.cuenta).FirstOrDefault();

                        if (info.tercero)
                            {
                            movNuevo.nit = modelo.tercero;
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
                                    x.ref_codigo == item.idrepuesto);
                            int perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                            perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(r =>
                                r.id == perfilBuscar);

                            #region Tiene perfil la referencia

                            if (pcr != null)
                                {
                                int? cuentaIva = pcr.cuenta_dev_iva_compras;

                                movNuevo.id_encab = encabezado;
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
                                        Convert.ToInt32(user_id);
                                    movNuevo.documento = Convert.ToString(numero);

                                    cuenta_puc infoReferencia = context.cuenta_puc
                                        .Where(a => a.cntpuc_id == cuentaIva)
                                        .FirstOrDefault();
                                    if (infoReferencia.manejabase)
                                        {
                                        movNuevo.basecontable =
                                            Convert.ToDecimal(baseUnitario, Cultureinfo);
                                        }
                                    else
                                        {
                                        movNuevo.basecontable = 0;
                                        }

                                    if (infoReferencia.documeto)
                                        {
                                        movNuevo.documento = Convert.ToString(numero);
                                        }

                                    if (infoReferencia.concepniff == 1)
                                        {
                                        movNuevo.credito = Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                        movNuevo.debito = 0;

                                        movNuevo.creditoniif =
                                            Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                        movNuevo.debitoniif = 0;
                                        }

                                    if (infoReferencia.concepniff == 4)
                                        {
                                        movNuevo.creditoniif =
                                            Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                        movNuevo.debitoniif = 0;
                                        }

                                    if (infoReferencia.concepniff == 5)
                                        {
                                        movNuevo.credito = Convert.ToDecimal(ivaReferencia, Cultureinfo);
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
                                        Convert.ToInt32(user_id);
                                    movNuevo.documento = Convert.ToString(numero);

                                    cuenta_puc infoReferencia = context.cuenta_puc
                                        .Where(a => a.cntpuc_id == parametro.cuenta)
                                        .FirstOrDefault();
                                    if (infoReferencia.manejabase)
                                        {
                                        movNuevo.basecontable =
                                            Convert.ToDecimal(baseUnitario, Cultureinfo);
                                        }
                                    else
                                        {
                                        movNuevo.basecontable = 0;
                                        }

                                    if (infoReferencia.documeto)
                                        {
                                        movNuevo.documento = Convert.ToString(numero);
                                        }

                                    if (infoReferencia.concepniff == 1)
                                        {
                                        movNuevo.credito = Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                        movNuevo.debito = 0;

                                        movNuevo.creditoniif =
                                            Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                        movNuevo.debitoniif = 0;
                                        }

                                    if (infoReferencia.concepniff == 4)
                                        {
                                        movNuevo.creditoniif =
                                            Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                        movNuevo.debitoniif = 0;
                                        }

                                    if (infoReferencia.concepniff == 5)
                                        {
                                        movNuevo.credito = Convert.ToDecimal(ivaReferencia, Cultureinfo);
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
                                movNuevo.id_encab = encabezado;
                                movNuevo.seq = secuencia;
                                movNuevo.idparametronombre = parametro.id_nombre_parametro;
                                movNuevo.cuenta = parametro.cuenta;
                                movNuevo.centro = parametro.centro;
                                movNuevo.fec = DateTime.Now;
                                movNuevo.fec_creacion = DateTime.Now;
                                movNuevo.userid_creacion =
                                    Convert.ToInt32(user_id);
                          
                                if (info.manejabase)
                                    {
                                    movNuevo.basecontable = Convert.ToDecimal(baseUnitario, Cultureinfo);
                                    }
                                else
                                    {
                                    movNuevo.basecontable = 0;
                                    }

                                if (info.documeto)
                                    {
                                    movNuevo.documento = Convert.ToString(numero);
                                    }

                                if (buscarCuenta.concepniff == 1)
                                    {
                                    movNuevo.credito = Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                    movNuevo.debito = 0;

                                    movNuevo.creditoniif = Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                    movNuevo.debitoniif = 0;
                                    }

                                if (buscarCuenta.concepniff == 4)
                                    {
                                    movNuevo.creditoniif = Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                    movNuevo.debitoniif = 0;
                                    }

                                if (buscarCuenta.concepniff == 5)
                                    {
                                    movNuevo.credito = Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                    movNuevo.debito = 0;
                                    }

                                }

                            #endregion

                      
                            }

                        #endregion

                        #region Inventario
                        if (parametro.id_nombre_parametro == 9 ||
                            parametro.id_nombre_parametro == 20)
                            {
                            icb_referencia perfilReferencia =
                                context.icb_referencia.FirstOrDefault(x =>
                                    x.ref_codigo == item.idrepuesto);
                            int perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                            perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(r =>
                                r.id == perfilBuscar);

                            #region Tiene perfil la referencia

                            if (pcr != null)
                                {
                                int? cuentaInven = pcr.cta_inventario;

                                movNuevo.id_encab = encabezado;
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
                                        Convert.ToInt32(user_id);
                                    movNuevo.documento = Convert.ToString(numero);

                                    cuenta_puc infoReferencia = context.cuenta_puc
                                        .Where(a => a.cntpuc_id == cuentaInven)
                                        .FirstOrDefault();
                                    if (infoReferencia.manejabase)
                                        {
                                        movNuevo.basecontable =
                                            Convert.ToDecimal(baseUnitario, Cultureinfo);
                                        }
                                    else
                                        {
                                        movNuevo.basecontable = 0;
                                        }

                                    if (infoReferencia.documeto)
                                        {
                                        movNuevo.documento = Convert.ToString(numero);
                                        }

                                    if (infoReferencia.concepniff == 1)
                                        {
                                        movNuevo.credito = Convert.ToDecimal(cr, Cultureinfo);
                                        movNuevo.debito = 0;

                                        movNuevo.creditoniif = Convert.ToDecimal(cr, Cultureinfo);
                                        movNuevo.debitoniif = 0;
                                        }

                                    if (infoReferencia.concepniff == 4)
                                        {
                                        movNuevo.creditoniif = Convert.ToDecimal(cr, Cultureinfo);
                                        movNuevo.debitoniif = 0;
                                        }

                                    if (infoReferencia.concepniff == 5)
                                        {
                                        movNuevo.credito = Convert.ToDecimal(cr, Cultureinfo);
                                        movNuevo.debito = 0;
                                        }

                                 
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
                                        Convert.ToInt32(user_id);
                                    movNuevo.documento = Convert.ToString(numero);

                                    cuenta_puc infoReferencia = context.cuenta_puc
                                        .Where(a => a.cntpuc_id == parametro.cuenta)
                                        .FirstOrDefault();
                                    if (infoReferencia.manejabase)
                                        {
                                        movNuevo.basecontable =
                                            Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                        }
                                    else
                                        {
                                        movNuevo.basecontable = 0;
                                        }

                                    if (infoReferencia.documeto)
                                        {
                                        movNuevo.documento = Convert.ToString(numero);
                                        }

                                    if (infoReferencia.concepniff == 1)
                                        {
                                        movNuevo.credito = Convert.ToDecimal(cr, Cultureinfo);
                                        movNuevo.debito = 0;

                                        movNuevo.creditoniif = Convert.ToDecimal(cr, Cultureinfo);
                                        movNuevo.debitoniif = 0;
                                        }

                                    if (infoReferencia.concepniff == 4)
                                        {
                                        movNuevo.creditoniif = Convert.ToDecimal(cr, Cultureinfo);
                                        movNuevo.debitoniif = 0;
                                        }

                                    if (infoReferencia.concepniff == 5)
                                        {
                                        movNuevo.credito = Convert.ToDecimal(cr, Cultureinfo);
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
                                movNuevo.id_encab = encabezado;
                                movNuevo.seq = secuencia;
                                movNuevo.idparametronombre = parametro.id_nombre_parametro;
                                movNuevo.cuenta = parametro.cuenta;
                                movNuevo.centro = parametro.centro;
                                movNuevo.fec = DateTime.Now;
                                movNuevo.fec_creacion = DateTime.Now;
                                movNuevo.userid_creacion =
                                    Convert.ToInt32(user_id);
                           
                                if (info.manejabase)
                                    {
                                    movNuevo.basecontable =
                                        Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                    }
                                else
                                    {
                                    movNuevo.basecontable = 0;
                                    }

                                if (info.documeto)
                                    {
                                    movNuevo.documento = Convert.ToString(numero);
                                    }

                                if (buscarCuenta.concepniff == 1)
                                    {
                                    movNuevo.credito = Convert.ToDecimal(cr, Cultureinfo);
                                    movNuevo.debito = 0;

                                    movNuevo.creditoniif = Convert.ToDecimal(cr, Cultureinfo);
                                    movNuevo.debitoniif = 0;
                                    }

                                if (buscarCuenta.concepniff == 4)
                                    {
                                    movNuevo.creditoniif = Convert.ToDecimal(cr, Cultureinfo);
                                    movNuevo.debitoniif = 0;
                                    }

                                if (buscarCuenta.concepniff == 5)
                                    {
                                    movNuevo.credito = Convert.ToDecimal(cr, Cultureinfo);
                                    movNuevo.debito = 0;
                                    }

                                //context.mov_contable.Add(movNuevo);
                                }

                            #endregion

                     
                                }
                            

                        #endregion

                        #region Costo

                        if (parametro.id_nombre_parametro == 12)
                            {
                            icb_referencia perfilReferencia =
                                context.icb_referencia.FirstOrDefault(x =>
                                    x.ref_codigo == item.idrepuesto);
                            int perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                            perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(r =>
                                r.id == perfilBuscar);

                            #region Tiene perfil la referencia

                            if (pcr != null)
                                {
                                int? cuentaCosto = pcr.cuenta_costo;

                                movNuevo.id_encab = encabezado ;
                                movNuevo.seq = secuencia;
                                movNuevo.idparametronombre = parametro.id_nombre_parametro;

                                #region tiene perfil y cuenta asignada al perfil

                                if (cuentaCosto != null)
                                    {
                                    movNuevo.cuenta = Convert.ToInt32(cuentaCosto);
                                    movNuevo.centro =
                                        item.tipotarifa == 2
                                            ? parametro.id_nombre_parametro == 12
                                                ? Convert.ToInt32(
                                                   item.idcentro)
                                                : parametro.centro
                                            : parametro.centro;
                                    movNuevo.fec = DateTime.Now;
                                    movNuevo.fec_creacion = DateTime.Now;
                                    movNuevo.userid_creacion =
                                        Convert.ToInt32(user_id);
                                    movNuevo.documento = Convert.ToString(numero);

                                    cuenta_puc infoReferencia = context.cuenta_puc
                                        .Where(a => a.cntpuc_id == cuentaCosto)
                                        .FirstOrDefault();
                                    if (infoReferencia.manejabase)
                                        {
                                        movNuevo.basecontable =
                                            Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                        }
                                    else
                                        {
                                        movNuevo.basecontable = 0;
                                        }

                                    if (infoReferencia.documeto)
                                        {
                                        movNuevo.documento = Convert.ToString(numero);
                                        }

                                    if (infoReferencia.concepniff == 1)
                                        {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(cr2, Cultureinfo);

                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(cr2, Cultureinfo);
                                        }

                                    if (infoReferencia.concepniff == 4)
                                        {
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(cr2, Cultureinfo);
                                        }

                                    if (infoReferencia.concepniff == 5)
                                        {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(cr2, Cultureinfo);
                                        }

                                    //context.mov_contable.Add(movNuevo);
                                    }

                                #endregion

                                #region tiene perfil pero no tiene cuenta asignada

                                else
                                    {
                                    movNuevo.cuenta = parametro.cuenta;
                                    movNuevo.centro =
                                      item.tipotarifa == 2
                                            ? parametro.id_nombre_parametro == 12
                                                ? Convert.ToInt32(
                                               item.idcentro)
                                                : parametro.centro
                                            : parametro.centro;
                                    movNuevo.fec = DateTime.Now;
                                    movNuevo.fec_creacion = DateTime.Now;
                                    movNuevo.userid_creacion =
                                        Convert.ToInt32(user_id);
                                    movNuevo.documento = Convert.ToString(numero);

                                    cuenta_puc infoReferencia = context.cuenta_puc
                                        .Where(a => a.cntpuc_id == parametro.cuenta)
                                        .FirstOrDefault();
                                    if (infoReferencia.manejabase)
                                        {
                                        movNuevo.basecontable =
                                            Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                        }
                                    else
                                        {
                                        movNuevo.basecontable = 0;
                                        }

                                    if (infoReferencia.documeto)
                                        {
                                        movNuevo.documento = Convert.ToString(numero);
                                        }

                                    if (infoReferencia.concepniff == 1)
                                        {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(cr2, Cultureinfo);

                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(cr2, Cultureinfo);
                                        }

                                    if (infoReferencia.concepniff == 4)
                                        {
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(cr2, Cultureinfo);
                                        }

                                    if (infoReferencia.concepniff == 5)
                                        {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(cr2, Cultureinfo);
                                        }

                                    //context.mov_contable.Add(movNuevo);
                                    }

                                #endregion
                                }

                            #endregion

                            #region La referencia no tiene perfil

                            else
                                {
                                movNuevo.id_encab = encabezado;
                                movNuevo.seq = secuencia;
                                movNuevo.idparametronombre = parametro.id_nombre_parametro;
                                movNuevo.cuenta = parametro.cuenta;
                                movNuevo.centro = item.tipotarifa == 2
                                    ? parametro.id_nombre_parametro == 12
                                        ? Convert.ToInt32(item.idcentro)
                                        : parametro.centro
                                    : parametro.centro;
                                ;
                                movNuevo.fec = DateTime.Now;
                                movNuevo.fec_creacion = DateTime.Now;
                                movNuevo.userid_creacion =
                                    Convert.ToInt32(user_id);
                             

                                if (info.manejabase)
                                    {
                                    movNuevo.basecontable =
                                        Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                    }
                                else
                                    {
                                    movNuevo.basecontable = 0;
                                    }

                                if (info.documeto)
                                    {
                                    movNuevo.documento = Convert.ToString(numero);
                                    }

                                if (buscarCuenta.concepniff == 1)
                                    {
                                    movNuevo.credito = 0;
                                    movNuevo.debito = Convert.ToDecimal(cr2, Cultureinfo);

                                    movNuevo.creditoniif = 0;
                                    movNuevo.debitoniif = Convert.ToDecimal(cr2, Cultureinfo);
                                    }

                                if (buscarCuenta.concepniff == 4)
                                    {
                                    movNuevo.creditoniif = 0;
                                    movNuevo.debitoniif = Convert.ToDecimal(cr2, Cultureinfo);
                                    }

                                if (buscarCuenta.concepniff == 5)
                                    {
                                    movNuevo.credito = 0;
                                    movNuevo.debito = Convert.ToDecimal(cr2, Cultureinfo);
                                    }

                                //context.mov_contable.Add(movNuevo);
                                }

                            #endregion

                          
                            }

                        #endregion

                                                  

                            secuencia++;
                            totalCreditos += Math.Round(movNuevo.credito);
                            totalDebitos += Math.Round(movNuevo.debito);
                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                                {
                                NumeroCuenta =
                                    "(" + buscarCuenta.cntpuc_numero + ")" +
                                    buscarCuenta.cntpuc_descp,
                                DescripcionParametro = descripcionParametro,
                                ValorDebito = movNuevo.debito,
                                ValorCredito = movNuevo.credito
                                });
                            resultado.movimientos.Add(movNuevo);
                            resultado.cantidad = resultado.cantidad + 1;
                            }
                        resultado.descuadrados = listaDescuadrados;
                        }
                    }
                }



            if (Math.Round(totalDebitos) != Math.Round(totalCreditos))
                {
                resultado.valido = false;
                }
            else
                {
                resultado.valido = true;
                }

            return resultado;


            }

        public ListaMovContable MovContableRepuestoIntFactBach(int perfilcont, int encabezado, long numero, int nit, int numPed, int user_id) {
      
            ListaMovContable listaMov = new ListaMovContable();
            listaMov.movimientos = new List<moviContable>();
            listaMov.valido = false;
            var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                              join nombreParametro in context.paramcontablenombres
                                                  on perfil.id_nombre_parametro equals nombreParametro.id
                                              join cuenta in context.cuenta_puc
                                                  on perfil.cuenta equals cuenta.cntpuc_id
                                              where perfil.id_perfil == perfilcont
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

            int tipotarifa = Convert.ToInt32(context.icb_sysparameter.Where(s => s.syspar_cod == "P147").Select(z => z.syspar_value).FirstOrDefault());
            var modelo = context.vpedido.Where(x => x.numero == numPed).FirstOrDefault();
           var repuestos = context.vpedrepuestos.Where(x => x.pedido_id == modelo.id && x.estado == true  && x.tipotarifa == tipotarifa ).ToList();

            decimal ivaEncabezado = 0;
            decimal costoPromedioTotal = 0;
            decimal costoEncabezado = 0;
            decimal totalCreditos = 0, totalDebitos = 0;
            int secuencia = 0;
            List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();
            foreach (var item in repuestos)
                {

                decimal vrto = Convert.ToDecimal(item.cantidad, Cultureinfo) * Convert.ToDecimal(item.vrunitario, Cultureinfo);
                decimal dcto = (vrto * Convert.ToDecimal(item.descuento, Cultureinfo)) / 100;
                decimal vrgen = vrto - dcto;            

                vw_promedio vwPromedio = context.vw_promedio.FirstOrDefault(x =>
                                       x.codigo == item.referencia && x.ano == DateTime.Now.Year &&
                                       x.mes == DateTime.Now.Month);


              

                decimal ? costoReferencia = vwPromedio.Promedio;
                decimal baseUnitario = Convert.ToDecimal(costoReferencia, Cultureinfo); 
              decimal ? cr = costoReferencia * Convert.ToDecimal(item.cantidad, Cultureinfo);
                decimal ivaReferencia = (Convert.ToDecimal(cr, Cultureinfo) * Convert.ToDecimal(item.iva, Cultureinfo)) / 100;
                decimal cr2 = Convert.ToDecimal(cr, Cultureinfo) + ivaReferencia;
                decimal valor_totalenca = 1;
                           ivaEncabezado = 1;
                costoPromedioTotal = 1;
                costoEncabezado = 1;
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
                            Convert.ToDecimal(ivaEncabezado, Cultureinfo) != 0
                            || parametro.id_nombre_parametro == 9 &&
                            Convert.ToDecimal(costoPromedioTotal, Cultureinfo) != 0 //costo promedio
                            || parametro.id_nombre_parametro == 20 &&
                            Convert.ToDecimal(costoPromedioTotal, Cultureinfo) != 0 //costo promedio
                            || parametro.id_nombre_parametro == 11 &&
                            Convert.ToDecimal(costoEncabezado, Cultureinfo) != 0
                            || parametro.id_nombre_parametro == 12 &&
                            Convert.ToDecimal(costoPromedioTotal, Cultureinfo) != 0) //costo promedio
                            {
                            moviContable movNuevo = new moviContable();

                            movNuevo.id_encab = encabezado;
                            movNuevo.seq = secuencia;
                            movNuevo.idparametronombre = parametro.id_nombre_parametro;
                            movNuevo.cuenta = parametro.cuenta;
                            movNuevo.centro = item.tipotarifa == 2
                                ? parametro.id_nombre_parametro == 11
                                    ? Convert.ToInt32(item.idcentro)
                                    : parametro.id_nombre_parametro == 12
                                        ? Convert.ToInt32(item.idcentro)
                                        : parametro.centro
                                : parametro.centro;
                            movNuevo.fec = DateTime.Now;
                            movNuevo.fec_creacion = DateTime.Now;
                            movNuevo.tipo_tarifa = Convert.ToInt32(item.tipotarifa);
                            movNuevo.userid_creacion = Convert.ToInt32(user_id);
                            movNuevo.documento = Convert.ToString(modelo.id);

                            cuenta_puc info = context.cuenta_puc
                                .Where(a => a.cntpuc_id == parametro.cuenta).FirstOrDefault();

                            if (info.tercero)
                                {
                                movNuevo.nit = nit;
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
                                        x.ref_codigo == item.referencia);
                                int perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(r =>
                                    r.id == perfilBuscar);

                                #region Tiene perfil la referencia

                                if (pcr != null)
                                    {
                                    int? cuentaIva = pcr.cuenta_dev_iva_compras;

                                    movNuevo.id_encab = encabezado;
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
                                            Convert.ToInt32(user_id);
                                        movNuevo.documento = Convert.ToString(numero);

                                        cuenta_puc infoReferencia = context.cuenta_puc
                                            .Where(a => a.cntpuc_id == cuentaIva)
                                            .FirstOrDefault();
                                        if (infoReferencia.manejabase)
                                            {
                                            movNuevo.basecontable =
                                                Convert.ToDecimal(baseUnitario, Cultureinfo);
                                            }
                                        else
                                            {
                                            movNuevo.basecontable = 0;
                                            }

                                        if (infoReferencia.documeto)
                                            {
                                            movNuevo.documento = Convert.ToString(numero);
                                            }

                                        if (infoReferencia.concepniff == 1)
                                            {
                                            movNuevo.credito = Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                            movNuevo.debito = 0;

                                            movNuevo.creditoniif =
                                                Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                            movNuevo.debitoniif = 0;
                                            }

                                        if (infoReferencia.concepniff == 4)
                                            {
                                            movNuevo.creditoniif =
                                                Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                            movNuevo.debitoniif = 0;
                                            }

                                        if (infoReferencia.concepniff == 5)
                                            {
                                            movNuevo.credito = Convert.ToDecimal(ivaReferencia, Cultureinfo);
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
                                            Convert.ToInt32(user_id);
                                        movNuevo.documento = Convert.ToString(numero);

                                        cuenta_puc infoReferencia = context.cuenta_puc
                                            .Where(a => a.cntpuc_id == parametro.cuenta)
                                            .FirstOrDefault();
                                        if (infoReferencia.manejabase)
                                            {
                                            movNuevo.basecontable =
                                                Convert.ToDecimal(baseUnitario, Cultureinfo);
                                            }
                                        else
                                            {
                                            movNuevo.basecontable = 0;
                                            }

                                        if (infoReferencia.documeto)
                                            {
                                            movNuevo.documento = Convert.ToString(numero);
                                            }

                                        if (infoReferencia.concepniff == 1)
                                            {
                                            movNuevo.credito = Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                            movNuevo.debito = 0;

                                            movNuevo.creditoniif =
                                                Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                            movNuevo.debitoniif = 0;
                                            }

                                        if (infoReferencia.concepniff == 4)
                                            {
                                            movNuevo.creditoniif =
                                                Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                            movNuevo.debitoniif = 0;
                                            }

                                        if (infoReferencia.concepniff == 5)
                                            {
                                            movNuevo.credito = Convert.ToDecimal(ivaReferencia, Cultureinfo);
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
                                    movNuevo.id_encab = encabezado;
                                    movNuevo.seq = secuencia;
                                    movNuevo.idparametronombre = parametro.id_nombre_parametro;
                                    movNuevo.cuenta = parametro.cuenta;
                                    movNuevo.centro = parametro.centro;
                                    movNuevo.fec = DateTime.Now;
                                    movNuevo.fec_creacion = DateTime.Now;
                                    movNuevo.userid_creacion =
                                        Convert.ToInt32(user_id);

                                    if (info.manejabase)
                                        {
                                        movNuevo.basecontable = Convert.ToDecimal(baseUnitario, Cultureinfo);
                                        }
                                    else
                                        {
                                        movNuevo.basecontable = 0;
                                        }

                                    if (info.documeto)
                                        {
                                        movNuevo.documento = Convert.ToString(numero);
                                        }

                                    if (buscarCuenta.concepniff == 1)
                                        {
                                        movNuevo.credito = Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                        movNuevo.debito = 0;

                                        movNuevo.creditoniif = Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                        movNuevo.debitoniif = 0;
                                        }

                                    if (buscarCuenta.concepniff == 4)
                                        {
                                        movNuevo.creditoniif = Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                        movNuevo.debitoniif = 0;
                                        }

                                    if (buscarCuenta.concepniff == 5)
                                        {
                                        movNuevo.credito = Convert.ToDecimal(ivaReferencia, Cultureinfo);
                                        movNuevo.debito = 0;
                                        }

                                    }

                                #endregion


                                }

                            #endregion

                            #region Inventario
                            if (parametro.id_nombre_parametro == 9 ||
                                parametro.id_nombre_parametro == 20)
                                {
                                icb_referencia perfilReferencia =
                                    context.icb_referencia.FirstOrDefault(x =>
                                        x.ref_codigo == item.referencia);
                                int perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(r =>
                                    r.id == perfilBuscar);

                                #region Tiene perfil la referencia

                                if (pcr != null)
                                    {
                                    int? cuentaInven = pcr.cta_inventario;

                                    movNuevo.id_encab = encabezado;
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
                                            Convert.ToInt32(user_id);
                                        movNuevo.documento = Convert.ToString(numero);

                                        cuenta_puc infoReferencia = context.cuenta_puc
                                            .Where(a => a.cntpuc_id == cuentaInven)
                                            .FirstOrDefault();
                                        if (infoReferencia.manejabase)
                                            {
                                            movNuevo.basecontable =
                                                Convert.ToDecimal(baseUnitario, Cultureinfo);
                                            }
                                        else
                                            {
                                            movNuevo.basecontable = 0;
                                            }

                                        if (infoReferencia.documeto)
                                            {
                                            movNuevo.documento = Convert.ToString(numero);
                                            }

                                        if (infoReferencia.concepniff == 1)
                                            {
                                            movNuevo.credito = Convert.ToDecimal(cr, Cultureinfo);
                                            movNuevo.debito = 0;

                                            movNuevo.creditoniif = Convert.ToDecimal(cr, Cultureinfo);
                                            movNuevo.debitoniif = 0;
                                            }

                                        if (infoReferencia.concepniff == 4)
                                            {
                                            movNuevo.creditoniif = Convert.ToDecimal(cr, Cultureinfo);
                                            movNuevo.debitoniif = 0;
                                            }

                                        if (infoReferencia.concepniff == 5)
                                            {
                                            movNuevo.credito = Convert.ToDecimal(cr, Cultureinfo);
                                            movNuevo.debito = 0;
                                            }


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
                                            Convert.ToInt32(user_id);
                                        movNuevo.documento = Convert.ToString(numero);

                                        cuenta_puc infoReferencia = context.cuenta_puc
                                            .Where(a => a.cntpuc_id == parametro.cuenta)
                                            .FirstOrDefault();
                                        if (infoReferencia.manejabase)
                                            {
                                            movNuevo.basecontable =
                                                Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                            }
                                        else
                                            {
                                            movNuevo.basecontable = 0;
                                            }

                                        if (infoReferencia.documeto)
                                            {
                                            movNuevo.documento = Convert.ToString(numero);
                                            }

                                        if (infoReferencia.concepniff == 1)
                                            {
                                            movNuevo.credito = Convert.ToDecimal(cr, Cultureinfo);
                                            movNuevo.debito = 0;

                                            movNuevo.creditoniif = Convert.ToDecimal(cr, Cultureinfo);
                                            movNuevo.debitoniif = 0;
                                            }

                                        if (infoReferencia.concepniff == 4)
                                            {
                                            movNuevo.creditoniif = Convert.ToDecimal(cr, Cultureinfo);
                                            movNuevo.debitoniif = 0;
                                            }

                                        if (infoReferencia.concepniff == 5)
                                            {
                                            movNuevo.credito = Convert.ToDecimal(cr, Cultureinfo);
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
                                    movNuevo.id_encab = encabezado;
                                    movNuevo.seq = secuencia;
                                    movNuevo.idparametronombre = parametro.id_nombre_parametro;
                                    movNuevo.cuenta = parametro.cuenta;
                                    movNuevo.centro = parametro.centro;
                                    movNuevo.fec = DateTime.Now;
                                    movNuevo.fec_creacion = DateTime.Now;
                                    movNuevo.userid_creacion =
                                        Convert.ToInt32(user_id);

                                    if (info.manejabase)
                                        {
                                        movNuevo.basecontable =
                                            Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                        }
                                    else
                                        {
                                        movNuevo.basecontable = 0;
                                        }

                                    if (info.documeto)
                                        {
                                        movNuevo.documento = Convert.ToString(numero);
                                        }

                                    if (buscarCuenta.concepniff == 1)
                                        {
                                        movNuevo.credito = Convert.ToDecimal(cr, Cultureinfo);
                                        movNuevo.debito = 0;

                                        movNuevo.creditoniif = Convert.ToDecimal(cr, Cultureinfo);
                                        movNuevo.debitoniif = 0;
                                        }

                                    if (buscarCuenta.concepniff == 4)
                                        {
                                        movNuevo.creditoniif = Convert.ToDecimal(cr, Cultureinfo);
                                        movNuevo.debitoniif = 0;
                                        }

                                    if (buscarCuenta.concepniff == 5)
                                        {
                                        movNuevo.credito = Convert.ToDecimal(cr, Cultureinfo);
                                        movNuevo.debito = 0;
                                        }

                                    //context.mov_contable.Add(movNuevo);
                                    }

                                #endregion


                                }


                            #endregion

                            #region Costo

                            if (parametro.id_nombre_parametro == 12)
                                {
                                icb_referencia perfilReferencia =
                                    context.icb_referencia.FirstOrDefault(x =>
                                        x.ref_codigo == item.referencia);
                                int perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(r =>
                                    r.id == perfilBuscar);

                                #region Tiene perfil la referencia

                                if (pcr != null)
                                    {
                                    int? cuentaCosto = pcr.cuenta_costo;

                                    movNuevo.id_encab = encabezado;
                                    movNuevo.seq = secuencia;
                                    movNuevo.idparametronombre = parametro.id_nombre_parametro;

                                    #region tiene perfil y cuenta asignada al perfil

                                    if (cuentaCosto != null)
                                        {
                                        movNuevo.cuenta = Convert.ToInt32(cuentaCosto);
                                        movNuevo.centro =
                                            item.tipotarifa == 2
                                                ? parametro.id_nombre_parametro == 12
                                                    ? Convert.ToInt32(
                                                       item.idcentro)
                                                    : parametro.centro
                                                : parametro.centro;
                                        movNuevo.fec = DateTime.Now;
                                        movNuevo.fec_creacion = DateTime.Now;
                                        movNuevo.userid_creacion =
                                            Convert.ToInt32(user_id);
                                        movNuevo.documento = Convert.ToString(numero);

                                        cuenta_puc infoReferencia = context.cuenta_puc
                                            .Where(a => a.cntpuc_id == cuentaCosto)
                                            .FirstOrDefault();
                                        if (infoReferencia.manejabase)
                                            {
                                            movNuevo.basecontable =
                                                Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                            }
                                        else
                                            {
                                            movNuevo.basecontable = 0;
                                            }

                                        if (infoReferencia.documeto)
                                            {
                                            movNuevo.documento = Convert.ToString(numero);
                                            }

                                        if (infoReferencia.concepniff == 1)
                                            {
                                            movNuevo.credito = 0;
                                            movNuevo.debito = Convert.ToDecimal(cr2, Cultureinfo);

                                            movNuevo.creditoniif = 0;
                                            movNuevo.debitoniif = Convert.ToDecimal(cr2, Cultureinfo);
                                            }

                                        if (infoReferencia.concepniff == 4)
                                            {
                                            movNuevo.creditoniif = 0;
                                            movNuevo.debitoniif = Convert.ToDecimal(cr2, Cultureinfo);
                                            }

                                        if (infoReferencia.concepniff == 5)
                                            {
                                            movNuevo.credito = 0;
                                            movNuevo.debito = Convert.ToDecimal(cr2, Cultureinfo);
                                            }

                                        //context.mov_contable.Add(movNuevo);
                                        }

                                    #endregion

                                    #region tiene perfil pero no tiene cuenta asignada

                                    else
                                        {
                                        movNuevo.cuenta = parametro.cuenta;
                                        movNuevo.centro =
                                          item.tipotarifa == 2
                                                ? parametro.id_nombre_parametro == 12
                                                    ? Convert.ToInt32(
                                                   item.idcentro)
                                                    : parametro.centro
                                                : parametro.centro;
                                        movNuevo.fec = DateTime.Now;
                                        movNuevo.fec_creacion = DateTime.Now;
                                        movNuevo.userid_creacion =
                                            Convert.ToInt32(user_id);
                                        movNuevo.documento = Convert.ToString(numero);

                                        cuenta_puc infoReferencia = context.cuenta_puc
                                            .Where(a => a.cntpuc_id == parametro.cuenta)
                                            .FirstOrDefault();
                                        if (infoReferencia.manejabase)
                                            {
                                            movNuevo.basecontable =
                                                Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                            }
                                        else
                                            {
                                            movNuevo.basecontable = 0;
                                            }

                                        if (infoReferencia.documeto)
                                            {
                                            movNuevo.documento = Convert.ToString(numero);
                                            }

                                        if (infoReferencia.concepniff == 1)
                                            {
                                            movNuevo.credito = 0;
                                            movNuevo.debito = Convert.ToDecimal(cr2, Cultureinfo);

                                            movNuevo.creditoniif = 0;
                                            movNuevo.debitoniif = Convert.ToDecimal(cr2, Cultureinfo);
                                            }

                                        if (infoReferencia.concepniff == 4)
                                            {
                                            movNuevo.creditoniif = 0;
                                            movNuevo.debitoniif = Convert.ToDecimal(cr2, Cultureinfo);
                                            }

                                        if (infoReferencia.concepniff == 5)
                                            {
                                            movNuevo.credito = 0;
                                            movNuevo.debito = Convert.ToDecimal(cr2, Cultureinfo);
                                            }

                                        //context.mov_contable.Add(movNuevo);
                                        }

                                    #endregion
                                    }

                                #endregion

                                #region La referencia no tiene perfil

                                else
                                    {
                                    movNuevo.id_encab = encabezado;
                                    movNuevo.seq = secuencia;
                                    movNuevo.idparametronombre = parametro.id_nombre_parametro;
                                    movNuevo.cuenta = parametro.cuenta;
                                    movNuevo.centro = item.tipotarifa == 2
                                        ? parametro.id_nombre_parametro == 12
                                            ? Convert.ToInt32(item.idcentro)
                                            : parametro.centro
                                        : parametro.centro;
                                    ;
                                    movNuevo.fec = DateTime.Now;
                                    movNuevo.fec_creacion = DateTime.Now;
                                    movNuevo.userid_creacion =
                                        Convert.ToInt32(user_id);


                                    if (info.manejabase)
                                        {
                                        movNuevo.basecontable =
                                            Convert.ToDecimal(valor_totalenca, Cultureinfo);
                                        }
                                    else
                                        {
                                        movNuevo.basecontable = 0;
                                        }

                                    if (info.documeto)
                                        {
                                        movNuevo.documento = Convert.ToString(numero);
                                        }

                                    if (buscarCuenta.concepniff == 1)
                                        {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(cr2, Cultureinfo);

                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(cr2, Cultureinfo);
                                        }

                                    if (buscarCuenta.concepniff == 4)
                                        {
                                        movNuevo.creditoniif = 0;
                                        movNuevo.debitoniif = Convert.ToDecimal(cr2, Cultureinfo);
                                        }

                                    if (buscarCuenta.concepniff == 5)
                                        {
                                        movNuevo.credito = 0;
                                        movNuevo.debito = Convert.ToDecimal(cr2, Cultureinfo);
                                        }

                                    //context.mov_contable.Add(movNuevo);
                                    }

                                #endregion


                                }

                            #endregion



                            secuencia++;
                            totalCreditos += Math.Round(movNuevo.credito);
                            totalDebitos += Math.Round(movNuevo.debito);
                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                                {
                                NumeroCuenta =
                                    "(" + buscarCuenta.cntpuc_numero + ")" +
                                    buscarCuenta.cntpuc_descp,
                                DescripcionParametro = descripcionParametro,
                                ValorDebito = movNuevo.debito,
                                ValorCredito = movNuevo.credito
                                });
                            listaMov.movimientos.Add(movNuevo);
                            listaMov.cantidad = listaMov.cantidad + 1;
                            }
                        listaMov.descuadrados = listaDescuadrados;
                        }
                    }
                }



            if (Math.Round(totalDebitos) != Math.Round(totalCreditos))
                {
                listaMov.valido = false;
                }
            else
                {
                listaMov.valido = true;
                }





            return listaMov;
            }





        }



    }


    
    