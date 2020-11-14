using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class depreciacionActivosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");
        // GET: depreciacionActivos
        public ActionResult Create(int? menu)
        {
            ListasDesplegables();
            //BuscarFavoritos(menu);
            return View();
        }


        [HttpPost]
        public ActionResult Create(depreciacionActivosModelo modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                {
                    try
                    {
                        //	}
                        //	catch (DbEntityValidationException ex)
                        //	{
                        //		dbTran.Rollback();
                        //		throw;
                        //	}
                        //}


                        int bodega = Convert.ToInt32(Request["bodega"]);
                        int docu = Convert.ToInt32(Request["tipoDoc"]);
                        DateTime Wfecha = Convert.ToDateTime(Request["fecha_dep"]);

                        int annio = Wfecha.Year;
                        int mes = Wfecha.Month;
                        int sigue = 0;

                        List<meses_cierre> buscarCerrado = context.meses_cierre.ToList();
                        foreach (meses_cierre item in buscarCerrado)
                        {
                            if (annio == item.ano && mes == item.mes)
                            {
                                sigue = 1;
                            }
                        }

                        if (sigue == 1)
                        {
                            TempData["mensaje_error"] = "El Periodo a Actualizar ya se encuentra CERRADO ";
                            ListasDesplegables();

                            //ViewBag.documentoSeleccionado = docu;

                            //ViewBag.bodegaSeleccionada = bodega;
                            //ViewBag.perfilSeleccionado = modelo.PerfilContable;
                            //BuscarFavoritos(menu);
                            return View(modelo);
                        }

                        int guardarLineasYMovimientos = 0;

                        long numeroConsecutivo = 0;
                        grupoconsecutivos buscarGrupoConsecutivos =
                            context.grupoconsecutivos.FirstOrDefault(x =>
                                x.documento_id == docu && x.bodega_id == bodega);
                        int numeroGrupo = buscarGrupoConsecutivos != null ? buscarGrupoConsecutivos.grupo : 0;

                        ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
                        icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
                        tp_doc_registros buscarTipoDocRegistro = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == docu);

                        numeroConsecutivoAux =
                            gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro, docu, bodega);

                        if (numeroConsecutivoAux != null)
                        {
                            numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                        }
                        else
                        {
                            TempData["mensaje_error"] =
                                "No existe un numero consecutivo asignado para este tipo de documento";
                            ListasDesplegables();

                            //ViewBag.documentoSeleccionado = docu;

                            //ViewBag.bodegaSeleccionada = bodega;
                            //ViewBag.perfilSeleccionado = modelo.PerfilContable;
                            //BuscarFavoritos(menu);
                            return View(modelo);
                        }

                        // Si llega aqui significa que si hay un numero de documento consecutivo 


                        var conjuntoActivos = (from activos in context.activosfijos
                                               join centro in context.centro_costo
                                                   on activos.idcentro equals centro.centcst_id
                                               join ubicacion in context.activosfubicacion
                                                   on activos.ubicacion equals ubicacion.id
                                               join clasif1 in context.activoclasificacion
                                                   on activos.clasificacion equals clasif1.id
                                               join clasif2 in context.activoclasificacion
                                                   on activos.clasificacionniff equals clasif2.id
                                               where activos.estado &&
                                                     activos.depreciable &&
                                                     activos.vendido == false &&
                                                     activos.valorresidual > 0 &&
                                                     (activos.fechaactualizacion == null &&
                                                      DbFunctions.DiffMonths(activos.fecha_activacion, Wfecha) == 1 ||
                                                      DbFunctions.DiffMonths(activos.fechaactualizacion, Wfecha) == 1) // &&
                                                                                                                       // activos.fecha_findepreciacion <= Wfecha
                                               select new
                                               {
                                                   activos.id,
                                                   activos.placa,
                                                   activos.descripcion,
                                                   centro.centcst_id,
                                                   centro.centcst_nombre,
                                                   ubicacionid = ubicacion.id,
                                                   ubicacion_des = ubicacion.descripcion,

                                                   activos.clasificacion,
                                                   depre = clasif1.Descripcion,
                                                   cuenta_dep = clasif1.cuentadepre,
                                                   cuanta_act = clasif1.cuentaactivo,

                                                   activos.clasificacionniff,
                                                   depreniif = clasif2.Descripcion,
                                                   cuenta_depniif = clasif2.cuentadepre,
                                                   cuenta_actniff = clasif2.cuentaactivo,

                                                   activos.valoractivo,
                                                   activos.constantedepre,
                                                   activos.constantedepreniif,

                                                   activos.meses_depreciacion,
                                                   activos.mesesfaltantes,

                                                   activos.meses_depreniff,
                                                   activos.mesesfaltantesniif,

                                                   activos.fecha_activacion,
                                                   activos.fecha_activacionniif,
                                                   activos.fechaactualizacion,

                                                   activos.valorresidual,
                                                   activos.valorresidualniif,
                                                   activos.valordepre,
                                                   activos.valordepreniif
                                               }).ToList();


                        if (conjuntoActivos != null && conjuntoActivos.Count != 0)
                        {
                            //*******
                            List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();
                            decimal calcularDebito = 0;
                            decimal calcularCredito = 0;
                            decimal sumaCostosDepreciacion = 0;
                            decimal sumaCostosActivos = 0;
                            foreach (var parametro in conjuntoActivos)
                            {
                                #region Cuenta_dep

                                cuenta_puc buscarCuenta =
                                    context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta_dep);
                                if (buscarCuenta != null)
                                {
                                    //sumaCostosReferenciasAux = 0;
                                    int WparametroDepreciacion = 21;
                                    string WNombreparametroDepreciacion = "Depreciación";
                                    sumaCostosDepreciacion = Convert.ToDecimal(parametro.constantedepre, miCultura);

                                    if (WparametroDepreciacion == 21)
                                    {
                                        #region Parametro 21 

                                        if (buscarCuenta.concepniff == 1 || buscarCuenta.concepniff == 5)
                                        {
                                            //	if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                            if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                            {
                                                calcularCredito += sumaCostosDepreciacion;
                                                listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                {
                                                    NumeroCuenta = Convert.ToString(parametro.cuenta_dep),
                                                    DescripcionParametro = WNombreparametroDepreciacion,
                                                    ValorDebito = 0,
                                                    ValorCredito = sumaCostosDepreciacion
                                                });
                                            }

                                            //if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                            if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                            {
                                                calcularDebito += sumaCostosDepreciacion;
                                                listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                {
                                                    NumeroCuenta = Convert.ToString(parametro.cuenta_dep),
                                                    DescripcionParametro = WNombreparametroDepreciacion,
                                                    ValorDebito = sumaCostosDepreciacion,
                                                    ValorCredito = 0
                                                });
                                            }
                                        }
                                        else
                                        {
                                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                                            {
                                                NumeroCuenta = Convert.ToString(parametro.cuenta_dep),
                                                DescripcionParametro = WNombreparametroDepreciacion,
                                                ValorDebito = 0,
                                                ValorCredito = 0
                                            });
                                        }

                                        #endregion
                                    }

                                    #region ???

                                    //else
                                    //{

                                    //	if (buscarCuenta.concepniff == 1 || buscarCuenta.concepniff == 5)
                                    //	{
                                    //		if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                    //		{
                                    //			calcularDebito += sumaCostosReferenciasAux;
                                    //			listaDescuadrados.Add(new DocumentoDescuadradoModel()
                                    //			{
                                    //				NumeroCuenta = Convert.ToString(parametro.cuenta_dep),
                                    //				DescripcionParametro = WNombreparametroDepreciacion,
                                    //				ValorDebito = sumaCostosReferenciasAux,
                                    //				ValorCredito = 0
                                    //			});
                                    //		}
                                    //		if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                    //		{
                                    //			calcularCredito += sumaCostosReferenciasAux;
                                    //			listaDescuadrados.Add(new DocumentoDescuadradoModel()
                                    //			{
                                    //				NumeroCuenta = Convert.ToString(parametro.cuenta_dep),
                                    //				DescripcionParametro = WNombreparametroDepreciacion,
                                    //				ValorDebito = 0,
                                    //				ValorCredito = sumaCostosReferenciasAux
                                    //			});
                                    //		}
                                    //	}
                                    //	else
                                    //	{
                                    //		listaDescuadrados.Add(new DocumentoDescuadradoModel()
                                    //		{
                                    //			NumeroCuenta = Convert.ToString(parametro.cuenta_dep),
                                    //			DescripcionParametro = WNombreparametroDepreciacion,
                                    //			ValorDebito = 0,
                                    //			ValorCredito = 0
                                    //		});
                                    //	}
                                    //}

                                    #endregion
                                }

                                #endregion


                                cuenta_puc buscarCuenta2 =
                                    context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuanta_act);
                                if (buscarCuenta2 != null)
                                {
                                    //sumaCostosReferenciasAux = 0;

                                    int WparametroActivo = 22;
                                    string WNombreparametroActivo = "Activos Fijos";
                                    sumaCostosActivos = Convert.ToDecimal(parametro.constantedepre, miCultura);


                                    if (WparametroActivo == 22)
                                    {
                                        #region Parametro 22

                                        if (buscarCuenta2.concepniff == 1 || buscarCuenta2.concepniff == 5)
                                        {
                                            //if (buscarCuenta2.mov_cnt.ToUpper().Contains("DEBITO"))
                                            if (buscarCuenta2.mov_cnt.ToUpper().Contains("CREDITO"))
                                            {
                                                calcularCredito += sumaCostosActivos;
                                                listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                {
                                                    NumeroCuenta = Convert.ToString(parametro.cuanta_act),
                                                    DescripcionParametro = WNombreparametroActivo,
                                                    ValorDebito = 0,
                                                    ValorCredito = sumaCostosActivos
                                                });
                                            }

                                            //if (buscarCuenta2.mov_cnt.ToUpper().Contains("CREDITO"))
                                            if (buscarCuenta2.mov_cnt.ToUpper().Contains("DEBITO"))
                                            {
                                                calcularDebito += sumaCostosActivos;
                                                listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                {
                                                    NumeroCuenta = Convert.ToString(parametro.cuanta_act),
                                                    DescripcionParametro = WNombreparametroActivo,
                                                    ValorDebito = sumaCostosActivos,
                                                    ValorCredito = 0
                                                });
                                            }
                                        }
                                        else
                                        {
                                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                                            {
                                                NumeroCuenta = Convert.ToString(parametro.cuanta_act),
                                                DescripcionParametro = WNombreparametroActivo,
                                                ValorDebito = 0,
                                                ValorCredito = 0
                                            });
                                        }

                                        #endregion
                                    }

                                    #region ????

                                    //else
                                    //{
                                    //	if (buscarCuenta2.concepniff == 1 || buscarCuenta2.concepniff == 5)
                                    //	{
                                    //		if (buscarCuenta2.mov_cnt.ToUpper().Contains("DEBITO"))
                                    //		{
                                    //			calcularDebito += sumaCostosReferenciasAux;
                                    //			listaDescuadrados.Add(new DocumentoDescuadradoModel()
                                    //			{
                                    //				NumeroCuenta = Convert.ToString(parametro.cuanta_act),
                                    //				DescripcionParametro = WNombreparametroActivo,
                                    //				ValorDebito = sumaCostosReferenciasAux,
                                    //				ValorCredito = 0
                                    //			});
                                    //		}
                                    //		if (buscarCuenta2.mov_cnt.ToUpper().Contains("CREDITO"))
                                    //		{
                                    //			calcularCredito += sumaCostosReferenciasAux;
                                    //			listaDescuadrados.Add(new DocumentoDescuadradoModel()
                                    //			{
                                    //				NumeroCuenta = Convert.ToString(parametro.cuanta_act),
                                    //				DescripcionParametro = WNombreparametroActivo,
                                    //				ValorDebito = 0,
                                    //				ValorCredito = sumaCostosReferenciasAux
                                    //			});
                                    //		}
                                    //	}
                                    //	else
                                    //	{
                                    //		listaDescuadrados.Add(new DocumentoDescuadradoModel()
                                    //		{
                                    //			NumeroCuenta = Convert.ToString(parametro.cuenta_dep),
                                    //			DescripcionParametro = WNombreparametroActivo,
                                    //			ValorDebito = 0,
                                    //			ValorCredito = 0
                                    //		});
                                    //	}
                                    //}

                                    #endregion
                                }
                            }

                            //	TOT = calcularCredito;
                            if (calcularCredito != calcularDebito)
                            {
                                TempData["documento_descuadrado"] =
                                    "El documento no tiene los movimientos calculados correctamente";
                                ViewBag.documentoSeleccionado = docu;
                                ViewBag.bodegaSeleccionada = bodega;
                                ViewBag.perfilSeleccionado = modelo.PerfilContable;

                                ViewBag.documentoDescuadrado = listaDescuadrados;
                                ViewBag.calculoDebito = calcularDebito;
                                ViewBag.calculoCredito = calcularCredito;
                                ListasDesplegables();
                                //BuscarFavoritos(menu);
                                return View(modelo);
                            }


                            icb_sysparameter buscarNit =
                                context.icb_sysparameter.FirstOrDefault(x =>
                                    x.syspar_cod == "P33"); // ojo con este nit de traslado
                            int nitTraslado =
                                buscarNit != null
                                    ? Convert.ToInt32(buscarNit.syspar_value)
                                    : 0; // ojo con este nit de traslado

                            encab_documento crearEncabezado = new encab_documento
                            {
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                tipo = docu,
                                bodega = bodega,
                                numero = numeroConsecutivo,
                                nit = nitTraslado, // ojo con este nit de traslado
                                                   //crearEncabezado.documento = modelo.Referencia; // Puede haber varias referencias
                                valor_total = calcularCredito,
                                fecha = Wfecha, // DateTime.Now;
                                fec_creacion = DateTime.Now,
                                impoconsumo = 0,
                                perfilcontable = modelo.PerfilContable
                            };
                            context.encab_documento.Add(crearEncabezado);

                            bool guardar = context.SaveChanges() > 0;

                            encab_documento buscarUltimoEncabezado = context.encab_documento.OrderByDescending(x => x.idencabezado)
                                .FirstOrDefault();
                            //***********////******************/////**********************************/////********************************************************

                            int secuencia = 1;

                            foreach (var parametro in conjuntoActivos)
                            {
                                cuenta_puc buscarCuenta =
                                    context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuanta_act);
                                if (buscarCuenta != null)
                                {
                                    int WparametroActivo = 22;
                                    sumaCostosActivos = Convert.ToDecimal(parametro.constantedepre, miCultura);


                                    if (WparametroActivo == 22)
                                    {
                                        int Witem = buscarUltimoEncabezado != null
                                            ? buscarUltimoEncabezado.idencabezado
                                            : 0;
                                        mov_contable SiExiste = context.mov_contable.FirstOrDefault(x =>
                                            x.id_encab == Witem && x.cuenta == parametro.cuanta_act &&
                                            x.centro == parametro.centcst_id);
                                        if (SiExiste != null)
                                        {
                                            SiExiste.credito = SiExiste.credito + sumaCostosActivos;
                                            SiExiste.creditoniif = SiExiste.creditoniif + sumaCostosActivos;
                                            context.Entry(SiExiste).State = EntityState.Modified;
                                            //context.SaveChanges();

                                            //Cuentas Valores
                                            DateTime fechaHoy = Wfecha; //DateTime.Now;
                                            cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                                x.centro == SiExiste.centro && x.cuenta == SiExiste.cuenta &&
                                                x.nit == SiExiste.nit && x.ano == fechaHoy.Year &&
                                                x.mes == fechaHoy.Month);
                                            if (buscar_cuentas_valores != null)
                                            {
                                                buscar_cuentas_valores.ano = fechaHoy.Year;
                                                buscar_cuentas_valores.mes = fechaHoy.Month;
                                                buscar_cuentas_valores.cuenta = SiExiste.cuenta;
                                                buscar_cuentas_valores.centro = SiExiste.centro;
                                                //buscar_cuentas_valores.nit = movNuevo.nit ?? idTerceroCero;
                                                buscar_cuentas_valores.nit = SiExiste.nit;
                                                //buscar_cuentas_valores.debito += SiExiste.debito;
                                                buscar_cuentas_valores.credito += SiExiste.credito;
                                                //buscar_cuentas_valores.debitoniff += SiExiste.debitoniif;
                                                buscar_cuentas_valores.creditoniff += SiExiste.creditoniif;
                                                context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                            }
                                            else
                                            {
                                                cuentas_valores crearCuentaValor = new cuentas_valores
                                                {
                                                    ano = fechaHoy.Year,
                                                    mes = fechaHoy.Month,
                                                    cuenta = SiExiste.cuenta,
                                                    centro = SiExiste.centro,
                                                    //crearCuentaValor.nit = movNuevo.nit ?? idTerceroCero;
                                                    nit = SiExiste.nit,
                                                    //crearCuentaValor.debito = SiExiste.debito;
                                                    credito = SiExiste.credito,
                                                    //crearCuentaValor.debitoniff = SiExiste.debitoniif;
                                                    creditoniff = SiExiste.creditoniif
                                                };
                                                context.cuentas_valores.Add(crearCuentaValor);
                                            }

                                            //Cuentas Valores

                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            mov_contable movNuevo = new mov_contable
                                            {
                                                id_encab = buscarUltimoEncabezado != null
                                                ? buscarUltimoEncabezado.idencabezado
                                                : 0,
                                                fec_creacion = DateTime.Now,
                                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                                idparametronombre = WparametroActivo,
                                                cuenta = parametro.cuanta_act,
                                                centro = parametro.centcst_id,
                                                fec = Wfecha, //DateTime.Now;
                                                seq = secuencia
                                            };

                                            // La validacion se hace ya que cuando el parametro es 9 (es decir inventario) se invierte el credito con debito
                                            if (WparametroActivo == 22)
                                            {
                                                if (buscarCuenta.concepniff == 1)
                                                {
                                                    //if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                                    if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                                    {
                                                        movNuevo.credito = sumaCostosActivos;
                                                        movNuevo.creditoniif = sumaCostosActivos;
                                                    }

                                                    //if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                                    if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                                    {
                                                        movNuevo.debitoniif = sumaCostosActivos;
                                                        movNuevo.debito = sumaCostosActivos;
                                                    }
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    //if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                                    if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                                    {
                                                        movNuevo.creditoniif = sumaCostosActivos;
                                                    }
                                                    //if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                                    if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                                    {
                                                        movNuevo.debitoniif = sumaCostosActivos;
                                                    }
                                                }

                                                if (buscarCuenta.concepniff == 5)
                                                {
                                                    //if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                                    if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                                    {
                                                        movNuevo.credito = sumaCostosActivos;
                                                    }
                                                    //if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                                    if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                                    {
                                                        movNuevo.debito = sumaCostosActivos;
                                                    }
                                                }
                                            }

                                            #region no se usa

                                            //else
                                            //{
                                            //	if (buscarCuenta.concepniff == 1)
                                            //	{
                                            //		//if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                            //		if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                            //		{
                                            //			movNuevo.debitoniif = sumaCostosActivos;
                                            //			movNuevo.debito = sumaCostosActivos;
                                            //		}
                                            //		//if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                            //		if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                            //		{
                                            //			movNuevo.credito = sumaCostosActivos;
                                            //			movNuevo.creditoniif = sumaCostosActivos;
                                            //		}
                                            //	}

                                            //	if (buscarCuenta.concepniff == 4)
                                            //	{
                                            //		// if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                            //		if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                            //		{
                                            //			movNuevo.debitoniif = sumaCostosActivos;
                                            //		}
                                            //		// if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                            //		if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                            //		{
                                            //			movNuevo.creditoniif = sumaCostosActivos;
                                            //		}
                                            //	}

                                            //	if (buscarCuenta.concepniff == 5)
                                            //	{
                                            //		//if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                            //		if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                            //		{
                                            //			movNuevo.debito = sumaCostosActivos;
                                            //		}
                                            //		// if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                            //		if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                            //		{
                                            //			movNuevo.credito = sumaCostosActivos;
                                            //		}
                                            //	}
                                            //}

                                            #endregion

                                            if (buscarCuenta.manejabase)
                                            {
                                                movNuevo.basecontable = sumaCostosActivos;
                                            }

                                            if (buscarCuenta.documeto)
                                            {
                                                movNuevo.documento = "1";
                                            }

                                            if (buscarCuenta.tercero)
                                            {
                                                movNuevo.nit = nitTraslado;
                                            }
                                            else
                                            {
                                                icb_terceros buscarNitCero =
                                                    context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
                                                movNuevo.nit = buscarNitCero != null ? buscarNitCero.tercero_id : 0;
                                            }

                                            movNuevo.detalle = "Depreciación de Activo";
                                            secuencia++;

                                            //Cuentas Valores
                                            DateTime fechaHoy = Wfecha; //DateTime.Now;
                                            cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                                x.centro == movNuevo.centro && x.cuenta == movNuevo.cuenta &&
                                                x.nit == movNuevo.nit && x.ano == fechaHoy.Year &&
                                                x.mes == fechaHoy.Month);
                                            if (buscar_cuentas_valores != null)
                                            {
                                                buscar_cuentas_valores.ano = fechaHoy.Year;
                                                buscar_cuentas_valores.mes = fechaHoy.Month;
                                                buscar_cuentas_valores.cuenta = movNuevo.cuenta;
                                                buscar_cuentas_valores.centro = movNuevo.centro;
                                                //buscar_cuentas_valores.nit = movNuevo.nit ?? idTerceroCero;
                                                buscar_cuentas_valores.nit = movNuevo.nit;
                                                //buscar_cuentas_valores.debito += movNuevo.debito;
                                                buscar_cuentas_valores.credito += movNuevo.credito;
                                                //buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                                                buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                                                context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                            }
                                            else
                                            {
                                                cuentas_valores crearCuentaValor = new cuentas_valores
                                                {
                                                    ano = fechaHoy.Year,
                                                    mes = fechaHoy.Month,
                                                    cuenta = movNuevo.cuenta,
                                                    centro = movNuevo.centro,
                                                    //crearCuentaValor.nit = movNuevo.nit ?? idTerceroCero;
                                                    nit = movNuevo.nit,
                                                    //crearCuentaValor.debito = movNuevo.debito;
                                                    credito = movNuevo.credito,
                                                    //crearCuentaValor.debitoniff = movNuevo.debitoniif;
                                                    creditoniff = movNuevo.creditoniif
                                                };
                                                context.cuentas_valores.Add(crearCuentaValor);
                                            }
                                            //Cuentas Valores

                                            context.mov_contable.Add(movNuevo);

                                            guardarLineasYMovimientos = context.SaveChanges();
                                        }
                                    }
                                }

                                cuenta_puc buscarCuenta2 =
                                    context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta_dep);
                                if (buscarCuenta2 != null)
                                {
                                    int WparametroDeprecicion = 21;
                                    sumaCostosDepreciacion = Convert.ToDecimal(parametro.constantedepre, miCultura);


                                    if (WparametroDeprecicion == 21)
                                    {
                                        int Witem = buscarUltimoEncabezado != null
                                            ? buscarUltimoEncabezado.idencabezado
                                            : 0;
                                        mov_contable SiExiste = context.mov_contable.FirstOrDefault(x =>
                                            x.id_encab == Witem && x.cuenta == parametro.cuenta_dep &&
                                            x.centro == parametro.centcst_id);
                                        if (SiExiste != null)
                                        {
                                            SiExiste.debito = SiExiste.debito + sumaCostosDepreciacion;
                                            SiExiste.debitoniif = SiExiste.debitoniif + sumaCostosDepreciacion;
                                            context.Entry(SiExiste).State = EntityState.Modified;

                                            //Cuentas Valores
                                            DateTime fechaHoy = Wfecha; // DateTime.Now;
                                            cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                                x.centro == SiExiste.centro && x.cuenta == SiExiste.cuenta &&
                                                x.nit == SiExiste.nit && x.ano == fechaHoy.Year &&
                                                x.mes == fechaHoy.Month);
                                            if (buscar_cuentas_valores != null)
                                            {
                                                buscar_cuentas_valores.ano = fechaHoy.Year;
                                                buscar_cuentas_valores.mes = fechaHoy.Month;
                                                buscar_cuentas_valores.cuenta = SiExiste.cuenta;
                                                buscar_cuentas_valores.centro = SiExiste.centro;
                                                //buscar_cuentas_valores.nit = movNuevo.nit ?? idTerceroCero;
                                                buscar_cuentas_valores.nit = SiExiste.nit;
                                                buscar_cuentas_valores.debito += SiExiste.debito;
                                                //buscar_cuentas_valores.credito += SiExiste.credito;
                                                buscar_cuentas_valores.debitoniff += SiExiste.debitoniif;
                                                //buscar_cuentas_valores.creditoniff += SiExiste.creditoniif;
                                                context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                            }
                                            else
                                            {
                                                cuentas_valores crearCuentaValor = new cuentas_valores
                                                {
                                                    ano = fechaHoy.Year,
                                                    mes = fechaHoy.Month,
                                                    cuenta = SiExiste.cuenta,
                                                    centro = SiExiste.centro,
                                                    //crearCuentaValor.nit = movNuevo.nit ?? idTerceroCero;
                                                    nit = SiExiste.nit,
                                                    debito = SiExiste.debito,
                                                    //crearCuentaValor.credito = SiExiste.credito;
                                                    debitoniff = SiExiste.debitoniif
                                                };
                                                //crearCuentaValor.creditoniff = SiExiste.creditoniif;
                                                context.cuentas_valores.Add(crearCuentaValor);
                                            }
                                            //Cuentas Valores

                                            context.SaveChanges();
                                        }
                                        else
                                        {
                                            mov_contable movNuevo = new mov_contable
                                            {
                                                id_encab = buscarUltimoEncabezado != null
                                                ? buscarUltimoEncabezado.idencabezado
                                                : 0,
                                                fec_creacion = DateTime.Now,
                                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                                idparametronombre = WparametroDeprecicion,
                                                cuenta = parametro.cuenta_dep,
                                                centro = parametro.centcst_id,
                                                fec = DateTime.Now,
                                                seq = secuencia
                                            };

                                            // La validacion se hace ya que cuando el parametro es 9 (es decir inventario) se invierte el credito con debito
                                            if (WparametroDeprecicion == 21)
                                            {
                                                if (buscarCuenta2.concepniff == 1)
                                                {
                                                    //if (buscarCuenta2.mov_cnt.ToUpper().Contains("DEBITO"))
                                                    if (buscarCuenta2.mov_cnt.ToUpper().Contains("CREDITO"))
                                                    {
                                                        movNuevo.credito = sumaCostosDepreciacion;
                                                        movNuevo.creditoniif = sumaCostosDepreciacion;
                                                    }

                                                    // if (buscarCuenta2.mov_cnt.ToUpper().Contains("CREDITO"))
                                                    if (buscarCuenta2.mov_cnt.ToUpper().Contains("DEBITO"))
                                                    {
                                                        movNuevo.debitoniif = sumaCostosDepreciacion;
                                                        movNuevo.debito = sumaCostosDepreciacion;
                                                    }
                                                }

                                                if (buscarCuenta.concepniff == 4)
                                                {
                                                    // if (buscarCuenta2.mov_cnt.ToUpper().Contains("DEBITO"))
                                                    if (buscarCuenta2.mov_cnt.ToUpper().Contains("CREDITO"))
                                                    {
                                                        movNuevo.creditoniif = sumaCostosDepreciacion;
                                                    }
                                                    // if (buscarCuenta2.mov_cnt.ToUpper().Contains("CREDITO"))
                                                    if (buscarCuenta2.mov_cnt.ToUpper().Contains("DEBITO"))
                                                    {
                                                        movNuevo.debitoniif = sumaCostosDepreciacion;
                                                    }
                                                }

                                                if (buscarCuenta2.concepniff == 5)
                                                {
                                                    //if (buscarCuenta2.mov_cnt.ToUpper().Contains("DEBITO"))
                                                    if (buscarCuenta2.mov_cnt.ToUpper().Contains("CREDITO"))
                                                    {
                                                        movNuevo.credito = sumaCostosDepreciacion;
                                                    }
                                                    // if (buscarCuenta2.mov_cnt.ToUpper().Contains("CREDITO"))
                                                    if (buscarCuenta2.mov_cnt.ToUpper().Contains("DEBITO"))
                                                    {
                                                        movNuevo.debito = sumaCostosDepreciacion;
                                                    }
                                                }
                                            }

                                            #region no se usa

                                            //else
                                            //{
                                            //	if (buscarCuenta2.concepniff == 1)
                                            //	{
                                            //		if (buscarCuenta2.mov_cnt.ToUpper().Contains("DEBITO"))
                                            //		{
                                            //			movNuevo.debitoniif = sumaCostosDepreciacion;
                                            //			movNuevo.debito = sumaCostosDepreciacion;
                                            //		}
                                            //		if (buscarCuenta2.mov_cnt.ToUpper().Contains("CREDITO"))
                                            //		{
                                            //			movNuevo.credito = sumaCostosDepreciacion;
                                            //			movNuevo.creditoniif = sumaCostosDepreciacion;
                                            //		}
                                            //	}

                                            //	if (buscarCuenta2.concepniff == 4)
                                            //	{
                                            //		if (buscarCuenta2.mov_cnt.ToUpper().Contains("DEBITO"))
                                            //		{
                                            //			movNuevo.debitoniif = sumaCostosDepreciacion;
                                            //		}
                                            //		if (buscarCuenta2.mov_cnt.ToUpper().Contains("CREDITO"))
                                            //		{
                                            //			movNuevo.creditoniif = sumaCostosDepreciacion;
                                            //		}
                                            //	}

                                            //	if (buscarCuenta2.concepniff == 5)
                                            //	{
                                            //		if (buscarCuenta2.mov_cnt.ToUpper().Contains("DEBITO"))
                                            //		{
                                            //			movNuevo.debito = sumaCostosDepreciacion;
                                            //		}
                                            //		if (buscarCuenta2.mov_cnt.ToUpper().Contains("CREDITO"))
                                            //		{
                                            //			movNuevo.credito = sumaCostosDepreciacion;
                                            //		}
                                            //	}
                                            //}

                                            #endregion

                                            if (buscarCuenta2.manejabase)
                                            {
                                                movNuevo.basecontable = sumaCostosDepreciacion;
                                            }

                                            if (buscarCuenta2.documeto)
                                            {
                                                movNuevo.documento = "1";
                                            }

                                            if (buscarCuenta2.tercero)
                                            {
                                                movNuevo.nit = nitTraslado;
                                            }
                                            else
                                            {
                                                icb_terceros buscarNitCero =
                                                    context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
                                                movNuevo.nit = buscarNitCero != null ? buscarNitCero.tercero_id : 0;
                                            }

                                            movNuevo.detalle = "Depreciación de Activo";
                                            secuencia++;

                                            //Cuentas Valores
                                            DateTime fechaHoy = Wfecha; //  DateTime.Now;
                                            cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                                x.centro == movNuevo.centro && x.cuenta == movNuevo.cuenta &&
                                                x.nit == movNuevo.nit && x.ano == fechaHoy.Year &&
                                                x.mes == fechaHoy.Month);
                                            if (buscar_cuentas_valores != null)
                                            {
                                                buscar_cuentas_valores.ano = fechaHoy.Year;
                                                buscar_cuentas_valores.mes = fechaHoy.Month;
                                                buscar_cuentas_valores.cuenta = movNuevo.cuenta;
                                                buscar_cuentas_valores.centro = movNuevo.centro;
                                                //buscar_cuentas_valores.nit = movNuevo.nit ?? idTerceroCero;
                                                buscar_cuentas_valores.nit = movNuevo.nit;
                                                buscar_cuentas_valores.debito += movNuevo.debito;
                                                //buscar_cuentas_valores.credito += movNuevo.credito;
                                                buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                                                //buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                                                context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                            }
                                            else
                                            {
                                                cuentas_valores crearCuentaValor = new cuentas_valores
                                                {
                                                    ano = fechaHoy.Year,
                                                    mes = fechaHoy.Month,
                                                    cuenta = movNuevo.cuenta,
                                                    centro = movNuevo.centro,
                                                    //crearCuentaValor.nit = movNuevo.nit ?? idTerceroCero;
                                                    nit = movNuevo.nit,
                                                    debito = movNuevo.debito,
                                                    //crearCuentaValor.credito = movNuevo.credito;
                                                    debitoniff = movNuevo.debitoniif
                                                };
                                                //crearCuentaValor.creditoniff = movNuevo.creditoniif;
                                                context.cuentas_valores.Add(crearCuentaValor);
                                            }
                                            //Cuentas Valores

                                            context.mov_contable.Add(movNuevo);
                                            guardarLineasYMovimientos = context.SaveChanges();
                                        }
                                    }
                                }

                                //******
                            }
                        }
                        else
                        {
                            TempData["mensaje_error"] = "No existen activos para depreciar en la fecha Seleccionada";
                            ListasDesplegables();
                            dbTran.Rollback();

                            //ViewBag.documentoSeleccionado = docu;

                            //ViewBag.bodegaSeleccionada = bodega;
                            //ViewBag.perfilSeleccionado = modelo.PerfilContable;
                            //BuscarFavoritos(menu);
                            return View(modelo);
                        }

                        try
                        {
                            //var guardarLineasYMovimientos = context.SaveChanges();

                            if (guardarLineasYMovimientos > 0)
                            {
                                ActualizaActivos(Wfecha);
                                long nu = 0;
                                if (buscarGrupoConsecutivos != null)
                                {
                                    List<icb_doc_consecutivos> numerosConsecutivos = context.icb_doc_consecutivos
                                        .Where(x => x.doccons_grupoconsecutivo == numeroGrupo).ToList();
                                    foreach (icb_doc_consecutivos item in numerosConsecutivos)
                                    {
                                        nu = item.doccons_siguiente;
                                        item.doccons_siguiente = item.doccons_siguiente + 1;
                                        context.Entry(item).State = EntityState.Modified;
                                    }

                                    context.SaveChanges();
                                    dbTran.Commit();
                                }

                                TempData["mensaje"] =
                                    "La Depreciacion de los Activos Fijos se ha realizado correctamente";
                                //	ViewBag.numDocumentoCreado = numeroConsecutivo;


                                encab_documento buscarUltimaDepre = context.encab_documento.Where(x => x.tipo == docu)
                                    .OrderByDescending(x => x.idencabezado).FirstOrDefault();
                                ViewBag.numDocumentoCreado = buscarUltimaDepre != null ? buscarUltimaDepre.numero : 0;

                                ListasDesplegables();
                                //BuscarFavoritos(menu);
                                return RedirectToAction("create");
                            }
                        }
                        catch (Exception ex)
                        {
                            TempData["mensaje_error"] = ex.InnerException.InnerException.Message;
                        }
                    }
                    catch (DbEntityValidationException)
                    {
                        //TempData["mensaje_error"] = ex.InnerException.InnerException.Message;
                        //TempData["mensaje_error"] = "No existen activos para depreciar en la fecha Seleccionada !!! ";
                        dbTran.Rollback();
                        throw;
                    }
                }
            }

            ListasDesplegables();
            //BuscarFavoritos(menu);
            return View();
        }

        public void ListasDesplegables()
        {
            //var buscarTipoDoc = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P41");
            //var id_tipo_doc = buscarTipoDoc != null ? Convert.ToInt32(buscarTipoDoc.syspar_value) : 0;

            ViewBag.TipoDocumento = context.tp_doc_registros.Where(x => x.tipo == 39).ToList();

            var documentos = from t in context.tp_doc_registros
                             where t.tpdoc_estado && t.tipo == 39
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

            List<activosfubicacion> buscarUbicaciones = context.activosfubicacion.ToList();
            ViewBag.ubicacion = new SelectList(buscarUbicaciones, "id", "descripcion");
            var centros = (from u in context.centro_costo
                           select new
                           {
                               u.centcst_id,
                               u.pre_centcst,
                               u.centcst_nombre,
                               nombre = "(" + u.pre_centcst + ") " + u.centcst_nombre
                           }).ToList();
            ViewBag.Centros = new SelectList(centros, "centcst_id", "nombre");

            var buscarPerfiles = context.perfil_contable_documento.Where(x => x.tipo == 39).Select(x => new
            {
                x.id,
                x.descripcion
            }).ToList();
            ViewBag.PerfilContable = new SelectList(buscarPerfiles, "id", "descripcion");

            //var numero = (from u in context.tp_doc_registros
            //			  where u.tipo == 39 
            //			   select new
            //			   {
            //				   u.tpdoc_id,

            //			   }).FirstOrDefault();
            //ViewBag.Centros = new SelectList(centros, "centcst_id", "nombre");

            //var buscarUltimaDepre = context.encab_documento.Where(x => x.tipo == 3066).OrderByDescending(x => x.idencabezado).FirstOrDefault();
            //ViewBag.numDocumentoCreado = buscarUltimaDepre != null ? buscarUltimaDepre.numero : 0;
        }

        public void ActualizaActivos(DateTime Wfecha)
        {
            List<activosfijos> Activos = context.activosfijos.Where(x => x.estado &&
                                                          x.depreciable &&
                                                          x.vendido == false &&
                                                          x.valorresidual > 0 &&
                                                          (x.fechaactualizacion == null &&
                                                           DbFunctions.DiffMonths(x.fecha_activacion, Wfecha) == 1 ||
                                                           DbFunctions.DiffMonths(x.fechaactualizacion, Wfecha) == 1))
                .ToList();
            if (Activos != null)
            {
                foreach (activosfijos item in Activos)
                {
                    item.fechaactualizacion = Wfecha != null ? Convert.ToDateTime(Wfecha) : DateTime.Now;
                    item.mesesfaltantes =
                        item.mesesfaltantes == 0 ? item.meses_depreciacion - 1 : item.mesesfaltantes - 1;
                    item.valorresidual = item.valorresidual == 0
                        ? item.valoractivo - item.constantedepre
                        : item.valorresidual - item.constantedepre;
                    item.valordepre = item.valordepre == 0
                        ? Convert.ToDecimal(item.constantedepre, miCultura)
                        : Convert.ToDecimal(item.valordepre, miCultura) + Convert.ToDecimal(item.constantedepre, miCultura);
                    item.mesesfaltantesniif =
                        item.mesesfaltantes == 0
                            ? item.meses_depreciacion - 1
                            : item.mesesfaltantes -
                              1; //item.mesesfaltantesniif == 0 ? item.meses_depreciacion  - 1 : item.meses_depreciacion - 1;
                    item.valorresidualniif = item.valorresidual == 0
                        ? item.valoractivo - item.constantedepre
                        : item.valorresidual -
                          item.constantedepre; //item.valorresidualniif == 0 ? item.valoractivo - item.constantedepre : item.valorresidualniif - item.constantedepreniif;
                    item.valordepreniif = item.valordepre == 0
                        ? Convert.ToDecimal(item.constantedepre, miCultura)
                        : Convert.ToDecimal(item.valordepre, miCultura) +
                          Convert.ToDecimal(item
                              .constantedepre, miCultura); //item.valordepreniif == 0 ? Convert.ToDecimal(item.constantedepre) : Convert.ToDecimal(item.valordepreniif) + Convert.ToDecimal(item.constantedepreniif);
                    context.Entry(item).State = EntityState.Modified;
                }

                context.SaveChanges();
            }
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


        public JsonResult BuscarPerfilesContables(int? idTpDoc, int? bod)
        {
            //var bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            var buscarPerfiles = (from perfil_contable in context.perfil_contable_documento
                                  join perfil_bodega in context.perfil_contable_bodega
                                      on perfil_contable.id equals perfil_bodega.idperfil
                                  where perfil_contable.tipo == idTpDoc && perfil_bodega.idbodega == bod
                                  select new
                                  {
                                      perfil_contable.id,
                                      perfil_contable.descripcion
                                  }).ToList();

            return Json(buscarPerfiles, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarActivosDepreciados()
        {
            var buscarDepreciados = (from activos in context.activosfijos
                                     join centros in context.centro_costo
                                         on activos.idcentro equals centros.centcst_id
                                     join ubica in context.activosfubicacion
                                         on activos.ubicacion equals ubica.id
                                     join clasif in context.activoclasificacion
                                         on activos.clasificacion equals clasif.id
                                     // where activos.fechaactualizacion != null
                                     select new
                                     {
                                         activos.id,
                                         activos.descripcion,
                                         activos.placa,
                                         centro = "(" + centros.pre_centcst + ") " + centros.centcst_nombre,
                                         ubicacion = ubica.descripcion,
                                         activos.meses_depreciacion,
                                         activos.mesesfaltantes,
                                         activos.constantedepre,
                                         activos.valoractivo,
                                         activos.valorresidual,
                                         activos.fecha_compra,
                                         activos.fecha_activacion,
                                         activos.fechaactualizacion,
                                         clasif.Descripcion
                                     }).ToList();
            var ActivosDepreciados = buscarDepreciados.Select(c => new
            {
                c.id,
                c.descripcion,
                c.placa,
                c.centro,
                clasif = c.Descripcion,
                c.ubicacion,
                mesdep = c.meses_depreciacion.ToString("N0"),
                mesfal = c.mesesfaltantes.ToString("N0"),
                constante = c.constantedepre != null ? c.constantedepre.Value.ToString("N0") : "",
                valoractivo = c.valoractivo != null ? c.valoractivo.ToString("N0") : "",
                valorresid = c.valorresidual != null ? c.valorresidual.Value.ToString("N0") : "",
                fecha = c.fechaactualizacion != null
                    ? c.fechaactualizacion.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : "",
                fechaC = c.fecha_compra != null ? c.fecha_compra.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                fechaA = c.fecha_activacion != null
                    ? c.fecha_activacion.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : ""
                //estadoUsuario = c.user_estado == true ? "Activo" : "Inactivo"
            }).ToList();

            return Json(ActivosDepreciados, JsonRequestBehavior.AllowGet);
        }
    }
}

#region Cuentas a verificar anteriores No deberia utilizarce esto

//var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
//								  join nombreParametro in context.paramcontablenombres
//								  on perfil.id_nombre_parametro equals nombreParametro.id
//								  join cuenta in context.cuenta_puc
//								  on perfil.cuenta equals cuenta.cntpuc_id
//								  where perfil.id_perfil == modelo.PerfilContable
//								  select new
//								  {
//									  perfil.id,
//									  perfil.id_nombre_parametro,
//									  perfil.cuenta,
//									  perfil.centro,
//									  perfil.id_perfil,
//									  nombreParametro.descripcion_parametro,
//									  cuenta.cntpuc_numero
//								  }).ToList();

#endregion