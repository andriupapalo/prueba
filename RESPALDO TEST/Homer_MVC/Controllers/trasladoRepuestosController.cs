using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class trasladoRepuestosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        private readonly CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
        private CultureInfo Cultura = new CultureInfo("is-IS");
        //private readonly object predicateBuilder;
      

        public class listadoRecibidos
        {
            public int idRecibido { get; set; }
            public int cantidad { get; set; }
            //id de la bodega recibida
            public int bodega { get; set; }
            public decimal costo { get; set; }
        }

        public class documentoscreados
        {
            public int encabezado { get; set; }
            public long consecutivo { get; set; }
            public int grupo { get; set; }

        }

        private static Expression<Func<vw_solicitudes_traslados, string>> GetColumnName(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(vw_solicitudes_traslados), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<vw_solicitudes_traslados, string>> lambda = Expression.Lambda<Func<vw_solicitudes_traslados, string>>(menuProperty, menu);

            return lambda;
        }
        // GET: trasladoRepuestos
        public ActionResult Create(int? menu, string idSolicitudtraslado = "")
        {
            var modelo = new TrasladoModel();
            if (!string.IsNullOrWhiteSpace(idSolicitudtraslado))
            {
                modelo.solicitudtranslado = Convert.ToInt32(idSolicitudtraslado);
                //busco la solicitud de traslado
                //provisionalmente quemado.
                modelo.TipoDocumento=1028;
            }
            ListasDesplegables(modelo);

            ViewBag.SolicitudTraslado = idSolicitudtraslado;
            BuscarFavoritos(menu);
            //agregarEstado(idSolicitudtraslado);
            return View(modelo);
        }

        // POST: trasladoRepuestos
        [HttpPost]
        public ActionResult Create(TrasladoModel modelo, int? menu)
        {
                        
            //*****/////////
            int documentointerno = 0;
            grupoconsecutivos grupo2 = new grupoconsecutivos();
            long consecutivo2 = 0;
            if (ModelState.IsValid)
            {

                using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                {
                    try
                    {
                        #region MyRegion

                        int totalAccesoriosAgregados = Convert.ToInt32(Request["total_repuestos"]);

                        if (totalAccesoriosAgregados == 0)
                        {
                            TempData["mensaje_error"] = "Debe agregar minimo una referencia para este proceso";
                            dbTran.Rollback();
                            ListasDesplegables(modelo);
                            ViewBag.documentoSeleccionado = modelo.TipoDocumento;
                            ViewBag.bodegaSeleccionada = modelo.BodegaOrigen;
                            ViewBag.usuarioSeleccionado = modelo.UsuarioRecepcion;
                            ViewBag.perfilSeleccionado = modelo.PerfilContable;
                            BuscarFavoritos(menu);
                            return View(modelo);
                        }

                        long numeroConsecutivo = 0;
                        grupoconsecutivos buscarGrupoConsecutivos = context.grupoconsecutivos.FirstOrDefault(x =>
                            x.documento_id == modelo.TipoDocumento && x.bodega_id == modelo.BodegaOrigen);
                        int numeroGrupo = buscarGrupoConsecutivos != null ? buscarGrupoConsecutivos.grupo : 0;
                        DocumentoPorBodegaController doc = new DocumentoPorBodegaController();

                        ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
                        icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
                        tp_doc_registros buscarTipoDocRegistro =
                            context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == modelo.TipoDocumento);
                        numeroConsecutivoAux = gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro, modelo.TipoDocumento,
                            modelo.BodegaOrigen);

                        if (numeroConsecutivoAux != null)
                        {
                            numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                        }
                        else
                        {
                            TempData["mensaje_error"] = "No existe un numero consecutivo asignado para este tipo de documento";
                            dbTran.Rollback();
                            ListasDesplegables(modelo);
                            ViewBag.documentoSeleccionado = modelo.TipoDocumento;
                            ViewBag.bodegaSeleccionada = modelo.BodegaOrigen;
                            ViewBag.usuarioSeleccionado = modelo.UsuarioRecepcion;
                            ViewBag.perfilSeleccionado = modelo.PerfilContable;
                            BuscarFavoritos(menu);
                            return View(modelo);
                        }

                        // Si llega aqui significa que si hay un numero de documento consecutivo
                        List<string> listaReferencias = new List<string>();
                        int numero_repuestos = Convert.ToInt32(Request["lista_repuestos"]);
                        int sequ = 1;
                        List<string> codigosReferencias = new List<string>();
                        for (int i = 1; i <= numero_repuestos; i++)
                        {
                            string referencia_codigo = Request["codigo_referencia" + i];
                            if (!string.IsNullOrEmpty(referencia_codigo))
                            {
                                codigosReferencias.Add(referencia_codigo);
                            }
                        }

                        int buscarCantidadRefEnOrigen = (from referencia in context.referencias_inven
                                                         where referencia.ano == DateTime.Now.Year
                                                               && referencia.mes == DateTime.Now.Month && referencia.bodega == modelo.BodegaOrigen
                                                               && codigosReferencias.Contains(referencia.codigo)
                                                         select referencia.codigo).Count();

                        // Significa que al menos una referencia se encuentra en referecias inven en la bodega de origen
                        // Se supone que si existe al menos una que este en el origen se hace el proceso
                        if (buscarCantidadRefEnOrigen > 0)
                        {
                            icb_sysparameter buscarNit = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P33");
                            int nitTraslado = buscarNit != null ? Convert.ToInt32(buscarNit.syspar_value) : 0;
                            string referenciasNoProcesadas = ", los siguientes codigos no se trasladaron por error en inventario";

                            encab_documento crearEncabezado = new encab_documento
                            {
                                notas = modelo.Notas,
                                tipo = modelo.TipoDocumento,
                                bodega = modelo.BodegaOrigen,
                                numero = numeroConsecutivo,
                                ///////crearEncabezado.documento = modelo.Referencia;
                                fecha = DateTime.Now,
                                fec_creacion = DateTime.Now,
                                impoconsumo = 0,
                                destinatario = modelo.UsuarioRecepcion,
                                bodega_destino = modelo.BodegaDestino,
                                perfilcontable = modelo.PerfilContable,
                                nit = nitTraslado,
                                requiere_mensajeria = modelo.requiere_mensajeria,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                            };
                            if (crearEncabezado.requiere_mensajeria == null || crearEncabezado.requiere_mensajeria == false || crearEncabezado.requiere_mensajeria == true)
                            {
                                crearEncabezado.mensajeria_atendido = false;
                            }
                            context.encab_documento.Add(crearEncabezado);

                            bool guardar = context.SaveChanges() > 0;


                            encab_documento buscarUltimoEncabezado =
                                context.encab_documento.OrderByDescending(x => x.idencabezado).Where(d => d.idencabezado == crearEncabezado.idencabezado).FirstOrDefault();

                            //var idsolitra = 0;
                            //si viene desde la solicitud de repuestos marcar
                            if (modelo.solicitudtranslado != null)
                            {
                                //busco la solicitud de traslado
                                var trasladox = context.Solicitud_traslado.Where(d => d.Id == modelo.solicitudtranslado.Value).FirstOrDefault();
                                if (trasladox != null)
                                {
                                    if (modelo.requiere_mensajeria == true)
                                    {
                                        trasladox.Estado_atendido = 2;
                                    }
                                    else
                                    {
                                        trasladox.Estado_atendido = 4;
                                    }
                                    trasladox.idEncabezado = crearEncabezado.idencabezado;
                                    buscarUltimoEncabezado.id_solicitud_traslado = trasladox.Id;
                                    context.Entry(trasladox).State = EntityState.Modified;
                                    context.Entry(buscarUltimoEncabezado).State = EntityState.Modified;
                                    context.SaveChanges();
                                }
                            }

                            //veo si el documento externo tiene documento interno asociado
                            tp_doc_registros doc_interno = context.tp_doc_registros.Where(d => d.tpdoc_id == modelo.TipoDocumento).FirstOrDefault();
                            //guardado de documento interno
                            if (doc_interno.doc_interno_asociado != null)
                            {
                                //se consulta consecutivo de documento interno
                                grupo2 = context.grupoconsecutivos.FirstOrDefault(x => x.documento_id == doc_interno.doc_interno_asociado && x.bodega_id == modelo.BodegaOrigen);
                                if (grupo2 != null)
                                {
                                    consecutivo2 = doc.BuscarConsecutivo(grupo2.grupo);
                                    //calculo y guardo el encabezado del movimiento interno
                                    encab_documento encabezado2 = new encab_documento
                                    {
                                        tipo = doc_interno.doc_interno_asociado.Value,
                                        numero = consecutivo2,
                                        nit = nitTraslado,
                                        fecha = DateTime.Now,
                                        fpago_id = crearEncabezado.fpago_id,
                                        vencimiento = crearEncabezado.vencimiento,
                                        valor_total = crearEncabezado.valor_total,
                                        iva = crearEncabezado.iva,
                                        porcen_retencion = crearEncabezado.porcen_retencion,
                                        retencion = crearEncabezado.retencion,
                                        porcen_reteiva = crearEncabezado.porcen_reteiva,
                                        retencion_iva = crearEncabezado.retencion_iva,
                                        porcen_retica = crearEncabezado.porcen_retica,
                                        retencion_ica = crearEncabezado.retencion_ica,
                                        fletes = crearEncabezado.fletes,
                                        iva_fletes = crearEncabezado.iva_fletes,
                                        costo = crearEncabezado.costo,
                                        vendedor = crearEncabezado.vendedor,
                                        documento = crearEncabezado.documento,
                                        remision = crearEncabezado.remision,
                                        bodega = crearEncabezado.bodega,
                                        concepto = crearEncabezado.concepto,
                                        moneda = crearEncabezado.moneda,
                                        perfilcontable = crearEncabezado.perfilcontable,
                                        valor_mercancia = crearEncabezado.valor_mercancia,
                                        fec_creacion = crearEncabezado.fec_creacion,
                                        userid_creacion = crearEncabezado.userid_creacion,
                                        estado = true,
                                        concepto2 = crearEncabezado.concepto2,
                                        id_movimiento_interno = crearEncabezado.idencabezado,
                                    };
                                    context.encab_documento.Add(encabezado2);
                                    context.SaveChanges();
                                    documentointerno = encabezado2.idencabezado;
                                }
                            }

                            for (int i = 1; i <= numero_repuestos; i++)
                            {
                                string referencia_codigo = Request["codigo_referencia" + i];
                                string referencia_cantidad = Request["cantidad_referencia" + i];
                                string referencia_costo = Request["costo_referencia" + i];
                                if (string.IsNullOrEmpty(referencia_codigo) || string.IsNullOrEmpty(referencia_cantidad))
                                {
                                    // Significa que la agregaron y la eliminaron
                                }
                                else
                                {
                                    listaReferencias.Add(referencia_codigo);

                                    var entrada = false;
                                    if (doc_interno.doc_interno_asociado != null)
                                    {//calculo el comportamiento del documento interno asociado

                                        var docinternoaso = context.tp_doc_registros.Where(d => d.tpdoc_id == doc_interno.doc_interno_asociado.Value).FirstOrDefault();
                                        if (docinternoaso.entrada_salida != null)
                                        {
                                            entrada = docinternoaso.entrada_salida.Value;
                                        }
                                    }

                                    vw_promedio buscarPromedio = context.vw_promedio.FirstOrDefault(x =>
                                        x.codigo == referencia_codigo && x.ano == DateTime.Now.Year &&
                                        x.mes == DateTime.Now.Month);
                                    decimal promedio = buscarPromedio != null ? buscarPromedio.Promedio ?? 0 : 0;

                                    referencias_inven buscarReferenciasInvenOrigen = context.referencias_inven.FirstOrDefault(x =>
                                        x.codigo == referencia_codigo && x.ano == DateTime.Now.Year &&
                                        x.mes == DateTime.Now.Month && x.bodega == modelo.BodegaOrigen);
                                    if (buscarReferenciasInvenOrigen != null)
                                    {
                                        if (entrada == false)
                                        {
                                            buscarReferenciasInvenOrigen.can_sal =
                                            buscarReferenciasInvenOrigen.can_sal + Convert.ToInt32(referencia_cantidad);
                                            buscarReferenciasInvenOrigen.cos_sal =
                                                buscarReferenciasInvenOrigen.cos_sal +
                                                promedio * Convert.ToInt32(referencia_cantidad);
                                            buscarReferenciasInvenOrigen.can_tra =
                                                buscarReferenciasInvenOrigen.can_tra + Convert.ToInt32(referencia_cantidad);
                                            buscarReferenciasInvenOrigen.cos_tra =
                                                buscarReferenciasInvenOrigen.cos_tra +
                                                promedio * Convert.ToInt32(referencia_cantidad);
                                        }
                                        else
                                        {
                                            buscarReferenciasInvenOrigen.can_ent =
                                            buscarReferenciasInvenOrigen.can_ent + Convert.ToInt32(referencia_cantidad);
                                            buscarReferenciasInvenOrigen.cos_ent =
                                                buscarReferenciasInvenOrigen.cos_ent +
                                                promedio * Convert.ToInt32(referencia_cantidad);
                                            buscarReferenciasInvenOrigen.can_tra =
                                                buscarReferenciasInvenOrigen.can_tra + Convert.ToInt32(referencia_cantidad);
                                            buscarReferenciasInvenOrigen.cos_tra =
                                                buscarReferenciasInvenOrigen.cos_tra +
                                                promedio * Convert.ToInt32(referencia_cantidad);
                                        }

                                        context.Entry(buscarReferenciasInvenOrigen).State = EntityState.Modified;

                                        // La bodega de origen debe existir
                                        /*var buscarReferenciasInvenDestino = context.referencias_inven.FirstOrDefault(x => x.codigo == referencia_codigo && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month && x.bodega == modelo.BodegaDestino);
                                            if (buscarReferenciasInvenDestino != null)
                                            {
                                                buscarReferenciasInvenDestino.can_ent = buscarReferenciasInvenDestino.can_ent + Convert.ToInt32(referencia_cantidad);
                                                buscarReferenciasInvenDestino.cos_ent = (buscarReferenciasInvenDestino.cos_ent) + (promedio * Convert.ToInt32(referencia_cantidad));
                                                buscarReferenciasInvenDestino.can_tra = buscarReferenciasInvenDestino.can_tra + Convert.ToInt32(referencia_cantidad);
                                                buscarReferenciasInvenDestino.cos_tra = (buscarReferenciasInvenDestino.cos_tra) + (promedio * Convert.ToInt32(referencia_cantidad));
                                                context.Entry(buscarReferenciasInvenDestino).State = EntityState.Modified;
                                            }
                                            else
                                            {
                                                referencias_inven crearReferencia = new referencias_inven();

                                                crearReferencia.bodega = modelo.BodegaDestino;
                                                crearReferencia.codigo = referencia_codigo;
                                                crearReferencia.ano = (short)DateTime.Now.Year;
                                                crearReferencia.mes = (short)DateTime.Now.Month;
                                                crearReferencia.can_ini = 0;
                                                crearReferencia.can_ent = Convert.ToInt32(referencia_cantidad);
                                                crearReferencia.can_sal = 0;
                                                crearReferencia.cos_ent = (buscarReferenciasInvenOrigen.cos_ent) + (promedio * Convert.ToInt32(referencia_cantidad));
                                                crearReferencia.can_tra = Convert.ToInt32(referencia_cantidad);
                                                crearReferencia.cos_tra = (buscarReferenciasInvenOrigen.cos_tra) + (promedio * Convert.ToInt32(referencia_cantidad));
                                                crearReferencia.modulo = "R";

                                                context.referencias_inven.Add(crearReferencia);
                                                context.SaveChanges();
                                            }*/
                                    }
                                    else
                                    {
                                        // Validado arriba porque debe haber referencias inven en la bodega de origen
                                        referenciasNoProcesadas += ", " + referencia_codigo;
                                    }

                                    lineas_documento crearLineasOrigen = new lineas_documento
                                    {
                                        codigo = referencia_codigo,
                                        fec_creacion = DateTime.Now,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                        nit = nitTraslado,
                                        cantidad = !string.IsNullOrEmpty(referencia_cantidad)
                                            ? Convert.ToDecimal(referencia_cantidad)
                                            : 0,
                                        bodega = modelo.BodegaOrigen,
                                        seq = sequ,
                                        estado = true,
                                        fec = DateTime.Now,
                                        costo_unitario = !string.IsNullOrEmpty(referencia_costo)
                                            ? Convert.ToDecimal(referencia_costo)
                                            : 0,
                                        valor_unitario = !string.IsNullOrEmpty(referencia_costo)
                                            ? Convert.ToDecimal(referencia_costo)
                                            : 0,
                                        id_encabezado = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0
                                    };
                                    context.lineas_documento.Add(crearLineasOrigen);
                                    context.SaveChanges();
                                    #endregion

                                    if (doc_interno.doc_interno_asociado != null && documentointerno != 0)
                                    {
                                        lineas_documento lineas2 = new lineas_documento
                                        {
                                            id_encabezado = documentointerno,
                                            codigo = crearLineasOrigen.codigo,
                                            seq = sequ,
                                            fec = DateTime.Now,
                                            nit = crearLineasOrigen.nit,
                                            cantidad = crearLineasOrigen.cantidad,
                                            porcentaje_iva = crearLineasOrigen.porcentaje_iva,
                                            valor_unitario = crearLineasOrigen.valor_unitario,
                                            porcentaje_descuento = crearLineasOrigen.porcentaje_descuento,
                                            costo_unitario = crearLineasOrigen.costo_unitario,
                                            bodega = crearLineasOrigen.bodega,
                                            fec_creacion = crearLineasOrigen.fec_creacion,
                                            userid_creacion = crearLineasOrigen.userid_creacion,
                                            tasa = crearLineasOrigen.tasa,
                                        };
                                        
                                        context.lineas_documento.Add(lineas2);
                                    }
                                    sequ++;

                                    recibidotraslados recibidos = new recibidotraslados
                                    {
                                        idtraslado = buscarUltimoEncabezado.idencabezado,
                                        refcodigo = referencia_codigo,
                                        cantidad = Convert.ToInt32(referencia_cantidad),
                                        recibido = false,
                                        fecharecibido = null,
                                        userrecibido = null,
                                        fechatraslado = DateTime.Now,
                                        usertraslado = Convert.ToInt32(Session["user_usuarioid"]),
                                        costo = Convert.ToDecimal(referencia_costo),
                                        idorigen = modelo.BodegaOrigen,
                                        iddestino = modelo.BodegaDestino,
                                        tipo = "R",
                                        idlinea= crearLineasOrigen.id,
                                    };
                                    if (modelo.requiere_mensajeria == true)
                                    {
                                        recibidos.estado_traslado = 2;
                                    }
                                    else
                                    {
                                        recibidos.estado_traslado = 4;
                                    }
                                    context.recibidotraslados.Add(recibidos);
                                }
                            }


                            decimal? promedioReferencias = (from referencia in context.icb_referencia
                                                            join promedioAux in context.vw_promedio
                                                                on referencia.ref_codigo equals promedioAux.codigo into pro
                                                            from promedioAux in pro.DefaultIfEmpty()
                                                            where listaReferencias.Contains(promedioAux.codigo)
                                                            select promedioAux.Promedio).Sum();


                            List<perfil_cuentas_documento> parametrosCuentas = context.perfil_cuentas_documento
                                .Where(x => x.id_perfil == modelo.PerfilContable).ToList();
                            int secuencia = 1;
                            centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                            int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
                            //icb_terceros terceroValorCero = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
                            //int idTerceroCero = centroValorCero != null ? Convert.ToInt32(terceroValorCero.tercero_id) : 0;

                            var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                                              join nombreParametro in context.paramcontablenombres
                                                                  on perfil.id_nombre_parametro equals nombreParametro.id
                                                              join cuenta in context.cuenta_puc
                                                                  on perfil.cuenta equals cuenta.cntpuc_id
                                                              where perfil.id_perfil == modelo.PerfilContable
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

                            foreach (perfil_cuentas_documento parametro in parametrosCuentas)
                            {
                                cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
                                if (buscarCuenta != null)
                                {
                                    //foreach (var cuenta in parametrosCuentasVerificar)
                                    //{
                                    if (parametro.id_nombre_parametro == 9 || parametro.id_nombre_parametro == 11)
                                    {
                                        mov_contable movNuevo = new mov_contable
                                        {
                                            id_encab = buscarUltimoEncabezado != null
                                            ? buscarUltimoEncabezado.idencabezado
                                            : 0,
                                            fec_creacion = DateTime.Now,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            idparametronombre = parametro.id_nombre_parametro,
                                            cuenta = parametro.cuenta,
                                            centro = parametro.centro,
                                            nit = buscarUltimoEncabezado.nit,
                                            fec = DateTime.Now,
                                            seq = secuencia
                                        };
                                        //movNuevo.credito = modelo.Costo;                
                                        if (buscarCuenta.tercero)
                                        {
                                            movNuevo.nit = nitTraslado;
                                        }

                                        if (buscarCuenta.concepniff == 1)
                                        {
                                            movNuevo.debitoniif = promedioReferencias ?? 0;
                                            movNuevo.debito = promedioReferencias ?? 0;
                                        }

                                        if (buscarCuenta.concepniff == 4)
                                        {
                                            movNuevo.debitoniif = promedioReferencias ?? 0;
                                        }

                                        if (buscarCuenta.concepniff == 5)
                                        {
                                            movNuevo.debito = promedioReferencias ?? 0;
                                        }

                                        movNuevo.detalle = "Traslado de repuesto ";
                                        secuencia++;
                                        AgregarRegistroCuentasValores(movNuevo, parametro.centro, parametro.cuenta,
                                            idCentroCero, buscarUltimoEncabezado.nit);
                                        context.mov_contable.Add(movNuevo);
                                        context.SaveChanges();
                                    }

                                    if (parametro.id_nombre_parametro == 20)
                                    {
                                        mov_contable movNuevo2 = new mov_contable
                                        {
                                            id_encab = buscarUltimoEncabezado != null
                                            ? buscarUltimoEncabezado.idencabezado
                                            : 0,
                                            fec_creacion = DateTime.Now,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            idparametronombre = parametro.id_nombre_parametro,
                                            cuenta = parametro.cuenta,
                                            centro = parametro.centro,
                                            nit = buscarUltimoEncabezado.nit,
                                            fec = DateTime.Now,
                                            seq = secuencia,
                                            //movNuevo2.debito = modelo.Costo;
                                            credito = promedioReferencias ?? 0
                                        };
                                        if (buscarCuenta.tercero)
                                        {
                                            movNuevo2.nit = nitTraslado;
                                        }

                                        if (buscarCuenta.concepniff == 1)
                                        {
                                            movNuevo2.credito = promedioReferencias ?? 0;
                                            movNuevo2.creditoniif = promedioReferencias ?? 0;
                                        }

                                        if (buscarCuenta.concepniff == 4)
                                        {
                                            movNuevo2.creditoniif = promedioReferencias ?? 0;
                                        }

                                        if (buscarCuenta.concepniff == 5)
                                        {
                                            movNuevo2.credito = promedioReferencias ?? 0;
                                        }

                                        movNuevo2.detalle = "Traslado de repuesto ";
                                        secuencia++;
                                        AgregarRegistroCuentasValores(movNuevo2, parametro.centro, parametro.cuenta,
                                            idCentroCero, buscarUltimoEncabezado.nit);
                                        context.mov_contable.Add(movNuevo2);
                                    }

                                    //}
                                }
                            }

                            int guardarLineasYMovimientos = context.SaveChanges();

                            if (guardarLineasYMovimientos > 0)
                            {
                                //creo los eventos de cada traslado

                                List<recibidotraslados> listatraslados = context.recibidotraslados
                                    .Where(d => d.idtraslado == buscarUltimoEncabezado.idencabezado).ToList();
                                foreach (recibidotraslados item in listatraslados)
                                {
                                    //busco el item exacto
                                    recibidotraslados trasladoexacto = context.recibidotraslados.FirstOrDefault(x => x.id == item.id);
                                    icb_referencia referencia2 = context.icb_referencia
                                        .Where(d => d.ref_codigo == trasladoexacto.refcodigo).FirstOrDefault();
                                    bodega_concesionario bodega1 = context.bodega_concesionario.Where(d => d.id == trasladoexacto.idorigen)
                                        .FirstOrDefault();
                                    bodega_concesionario bodega2 = context.bodega_concesionario.Where(d => d.id == trasladoexacto.iddestino)
                                        .FirstOrDefault();

                                    //creo el tracking de ese traslado
                                    tracking_traslado track = new tracking_traslado
                                    {
                                        id_recibotraslado = trasladoexacto.id,
                                        estado_evento = "ENVIADO",
                                        descripcion_evento = "Envío de " + trasladoexacto.cantidad +
                                                             " unidades de la referencia " + referencia2.ref_descripcion +
                                                             " (" + trasladoexacto.refcodigo + ") desde la bodega " +
                                                             bodega1.bodccs_nombre + " a la bodega " + bodega2.bodccs_nombre,
                                        fecha_evento = DateTime.Now,
                                        id_usuario_creacion = trasladoexacto.usertraslado
                                    };
                                    context.tracking_traslado.Add(track);
                                }
                                //actualizar el costo total del traslado saliente
                                var lineasdocumentox = context.lineas_documento.Where(d => d.id_encabezado == crearEncabezado.idencabezado).ToList();
                                decimal totalencabezado = 0;
                                foreach (var item in lineasdocumentox)
                                {
                                    var descuento = (item.costo_unitario * Convert.ToDecimal(item.porcentaje_descuento, Cultura)) / 100;
                                    var valorref = item.cantidad * (item.costo_unitario - descuento);
                                    var iva = valorref * Convert.ToDecimal(item.porcentaje_iva, Cultura) / 100;
                                    totalencabezado = totalencabezado + valorref + iva;
                                }
                                crearEncabezado.valor_total = totalencabezado;
                                crearEncabezado.valor_mercancia = totalencabezado;

                                context.Entry(crearEncabezado).State = EntityState.Modified;
                                context.SaveChanges();
                                if (buscarGrupoConsecutivos != null)
                                {
                                    List<icb_doc_consecutivos> numerosConsecutivos = context.icb_doc_consecutivos
                                        .Where(x => x.doccons_grupoconsecutivo == numeroGrupo).ToList();
                                    foreach (icb_doc_consecutivos item in numerosConsecutivos)
                                    {
                                        item.doccons_siguiente = item.doccons_siguiente + 1;
                                        context.Entry(item).State = EntityState.Modified;
                                    }

                                    if (documentointerno != 0)
                                    {
                                        var docinterno = context.encab_documento.Where(d => d.idencabezado == documentointerno).FirstOrDefault();
                                        if (docinterno != null)
                                        {
                                            docinterno.valor_total = totalencabezado;
                                            docinterno.valor_mercancia = totalencabezado;

                                            context.Entry(docinterno).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                        doc.ActualizarConsecutivo(grupo2.grupo, consecutivo2);

                                    }
                                    context.SaveChanges();
                                }
                                dbTran.Commit();

                                TempData["mensaje"] = "El traslado se ha realizado correctamente" + referenciasNoProcesadas;
                                TempData["numeroConsecutivo"] = numeroConsecutivo;
                                ViewBag.documentoSeleccionado = modelo.TipoDocumento;
                                ViewBag.bodegaSeleccionada = modelo.BodegaOrigen;
                                ViewBag.usuarioSeleccionado = modelo.UsuarioRecepcion;
                                ViewBag.perfilSeleccionado = modelo.PerfilContable;
                                ListasDesplegables(modelo);
                                BuscarFavoritos(menu);
                                return RedirectToAction("Create");
                            }
                        }
                        else
                        {
                            // Significa que ninguna referencia de las agregadas se encuentra en la tabla referencias inven en la bodega de origen, al menos debe haber una
                            // pero por no haber ninguna entonces nse guarda ni encabezado ni nada de lo demas.
                            TempData["mensaje_error"] =
                                "Referencia(s) no valida(s) en inventario, realice recalculo de la(s) misma(s)";
                            dbTran.Rollback();

                            ListasDesplegables(modelo);
                            ViewBag.documentoSeleccionado = modelo.TipoDocumento;
                            ViewBag.bodegaSeleccionada = modelo.BodegaOrigen;
                            ViewBag.usuarioSeleccionado = modelo.UsuarioRecepcion;
                            ViewBag.perfilSeleccionado = modelo.PerfilContable;
                            BuscarFavoritos(menu);
                            return View(modelo);
                        }
                    }
                    catch (DbEntityValidationException ex)
                    {
                        var mensaje = ex;
                        dbTran.Rollback();
                    }

                }
            }
            else
            {
                TempData["mensaje_error"] =
                                "Errores de guardado, por favor verifique campos obligatorios";
            }
            //ViewBag.documentoSeleccionado = modelo.TipoDocumento;
            ViewBag.bodegaSeleccionada = modelo.BodegaOrigen;
            ViewBag.usuarioSeleccionado = modelo.UsuarioRecepcion;
            ViewBag.perfilSeleccionado = modelo.PerfilContable;
            ListasDesplegables(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }
        /**/

        public JsonResult trackingtraslado(int? id)
        {
            int valor = 0;
            string respuesta = "";

            if (id != null)
            {
                //busco el id recibido traslado
                recibidotraslados existe = context.recibidotraslados.Where(d => d.id == id).FirstOrDefault();
                if (existe != null)
                {
                    //proceso la lista de trackings
                    List<tracking_traslado> tracking1 = context.tracking_traslado.Where(d => d.id_recibotraslado == existe.id).ToList();
                    var listatracking = tracking1.Select(d => new
                    {
                        d.id_tracking_traslado,
                        usuario = d.users.user_nombre + " " + d.users.user_apellido,
                        d.descripcion_evento,
                        d.estado_evento,
                        fecha_evento = d.fecha_evento.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    });
                    //me traigo el historial de ese traslado
                    var historial = new
                    {
                        numero_traslado = existe.encab_documento.numero,
                        bodega_origen = existe.bodega_concesionario.bodccs_nombre,
                        bodega_destino = existe.bodega_concesionario1.bodccs_nombre,
                        asesor = existe.users.user_nombre + " " + existe.users.user_apellido,
                        FechasPedidoSugerido =
                            existe.fechatraslado.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")),
                        estado = existe.recibido ? existe.recibo_completo ? "Recibido" : "Incompleto" : "Sin Recibir",
                        listatracking
                    };

                    valor = 1;
                    respuesta = "No se seleccionó un id de traslado válido";
                    var data = new { valor, respuesta, historial };
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    respuesta = "No se seleccionó un id de traslado válido";
                    var data = new { valor, respuesta };
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }

            {
                respuesta = "No se seleccionó un id de traslado válido";
                var data = new { valor, respuesta };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        public void AgregarRegistroCuentasValores(mov_contable movNuevo, int? centro, int cuenta, int idCentroCero,
            int idTerceroCero)
        {
            DateTime fechaHoy = DateTime.Now;
            cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                x.centro == centro && x.cuenta == cuenta && x.nit == movNuevo.nit && x.ano == fechaHoy.Year &&
                x.mes == fechaHoy.Month);

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
        }

        public JsonResult BuscarTrasladosRepuestos()
        {
            icb_sysparameter tipoDocTrasladoRepuestos = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P37");
            int id_tipo_documento = tipoDocTrasladoRepuestos != null
                ? Convert.ToInt32(tipoDocTrasladoRepuestos.syspar_value)
                : 0;

            var buscarTraslados = (from encabezado in context.encab_documento
                                   join origen in context.bodega_concesionario
                                       on encabezado.bodega equals origen.id into ori
                                   from origen in ori.DefaultIfEmpty()
                                   join destino in context.bodega_concesionario
                                       on encabezado.bodega_destino equals destino.id into dest
                                   from destino in dest.DefaultIfEmpty()
                                   where encabezado.tipo == id_tipo_documento 
                                   select new
                                   {
                                       encabezado.idencabezado,
                                       encabezado.numero,
                                       encabezado.fecha,
                                       origen = origen.bodccs_nombre,
                                       destino = destino.bodccs_nombre,
                                       encabezado.id_solicitud_traslado,
                                       mensajeria= encabezado.requiere_mensajeria == false || encabezado.requiere_mensajeria == null ? "No" : "Si",
                                       referencias = (from lineas in context.lineas_documento
                                                      join referencia in context.icb_referencia
                                                          on lineas.codigo equals referencia.ref_codigo
                                                      where lineas.id_encabezado == encabezado.idencabezado
                                                      select new
                                                      {
                                                          lineas.seq,
                                                          lineas.cantidad,
                                                          referencia.ref_descripcion
                                                      }).Distinct().ToList()
                                   }).ToList();

            var data = buscarTraslados.Select(x => new
            {
                x.idencabezado,
                x.numero,
                fecha = x.fecha.ToShortDateString(),
                x.origen,
                x.destino,
                x.mensajeria,
                numero_solicitud=x.id_solicitud_traslado!=null?x.id_solicitud_traslado.Value.ToString():"",
                x.referencias
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public void ListasDesplegables(TrasladoModel modelo)
        {

            //ViewBag.BodegaOrigen = new SelectList(context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).Where(x => x.es_repuestos==true),
            //    "id", "bodccs_nombre", modelo.BodegaOrigen);
            var id1 = 0;
            var id2 = 0;
            if (modelo.solicitudtranslado != null)
            {

                //buscamos las bodegas origen y destino en la solicitud de traslado. Osea. Punto.
                var listaB1 = (from a in context.bodega_concesionario
                               join c in context.Solicitud_traslado
                                 on a.id equals c.Id_bodega_origen
                               select new
                               {
                                   idtemp = c.Id,
                                   id = c.Id_bodega_origen,
                               }).Where(x => x.idtemp==modelo.solicitudtranslado).FirstOrDefault();

                id1 = listaB1 != null ? listaB1.id : 0;
                if (modelo.BodegaOrigen == 0)
                {
                    if (id1 != 0)
                    {
                        modelo.BodegaOrigen = id1;
                    }
                }
            }

            if (modelo.solicitudtranslado != null)
            {
                var listaB2 = (from a in context.bodega_concesionario
                               join c in context.Solicitud_traslado
                                 on a.id equals c.Id_bodega_destino
                               select new
                               {
                                   idsoli = c.Id,
                                   id = c.Id_bodega_destino,
                               }).Where(x => x.idsoli == modelo.solicitudtranslado).FirstOrDefault();

                id2 = listaB2 != null ? listaB2.id : 0;
                if (modelo.BodegaDestino == 0)
                {
                    if (id2 != 0)
                    {
                        modelo.BodegaDestino = id2;
                    }
                }
            }

            var predicado = PredicateBuilder.True<bodega_concesionario>();
            if (id1 != 0)
            {
                predicado = predicado.And(d => d.id == id1);
            }
            var consultabodegas = context.bodega_concesionario.Where(predicado).ToList();
            var listaBodega = consultabodegas.Select(d=> new
                              {
                                  id = d.id,
                                  nombre = d.bodccs_nombre

                              }).OrderBy(d =>d.id).ToList();


            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in listaBodega)
            {
                lista.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }


            ViewBag.BodegaOrigen =new SelectList(lista,"Value","Text",modelo.BodegaOrigen);

            var predicado2 = PredicateBuilder.True<bodega_concesionario>();
            if (id2 != 0)
            {
                predicado2 = predicado2.And(d => d.id == id2);
            }
            var consultabodegas2 = context.bodega_concesionario.Where(predicado2).ToList();
            var listBodega2 = consultabodegas2.Select(d=> new
                               {
                                   id = d.id,
                                   nombre = d.bodccs_nombre

                               }).OrderBy(d => d.id).ToList();


            List<SelectListItem> lista2 = new List<SelectListItem>();
            foreach (var item in listBodega2)
            {
                lista2.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }

            ViewBag.BodegaDestino = new SelectList(lista2, "Value", "Text", modelo.BodegaDestino);

            //ViewBag.BodegaDestino = lista2;

            int value = Convert.ToInt32(
                context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P37").syspar_value);
            var buscarTipoDocumentos = (from tipoDocumentos in context.tp_doc_registros
                                        where tipoDocumentos.tpdoc_id == value
                                        select new
                                        {
                                            tipoDocumentos.tpdoc_id,
                                            nombre = "(" + tipoDocumentos.prefijo + ") " + tipoDocumentos.tpdoc_nombre
                                        }).ToList();
            ViewBag.TipoDocumento = new SelectList(buscarTipoDocumentos, "tpdoc_id", "nombre", modelo.TipoDocumento);
            

            //bodega
            var listBodega = (from grupo in context.grupoconsecutivos
                              join documento in context.tp_doc_registros
                                  on grupo.documento_id equals documento.tpdoc_id
                              join bodega in context.bodega_concesionario
                                  on grupo.bodega_id equals bodega.id
                              where documento.prefijo == "TRB"
                              select new
                              {
                                  grupo.grupo,
                                  documento.prefijo,
                                  id = bodega.bodccs_cod,
                                  nombre = bodega.bodccs_nombre
                              }).OrderBy(x => x.grupo).ToList();


            List<SelectListItem> lista1 = new List<SelectListItem>();
            foreach (var item in listBodega)
            {
                lista1.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }
            //ViewBag.DestinoBodega = lista1;//DestinoBodega
            //bodega
            List<icb_referencia> buscarReferencias = context.icb_referencia.Where(x => x.modulo == "R").ToList();
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (icb_referencia item in buscarReferencias)
            {
                string nombre = "(" + item.ref_codigo + ") " + item.ref_descripcion;
                items.Add(new SelectListItem { Text = nombre, Value = item.ref_codigo });
            }

            ViewBag.Referencia = new SelectList(items, "Value", "Text");

            /*Estado*/
            var listEstado = (from e in context.Estado
                         select new
                         {
                             id = e.id,
                             nombre = e.Tipo
                         }).ToList();

            List<SelectListItem> listaEstado = new List<SelectListItem>();
            foreach (var item in listEstado)
            {
                listaEstado.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }
            ViewBag.Estado = listaEstado;

        }

        public JsonResult createSolicitud(tempSolicitud tempSolicitud)
        {
            var data = 0;
            //object[] data = {
            //    bodega,
            //    cliente,
            //    referencias,
            //    precios
            //};
            tempSolicitud.dataBogeda = tempSolicitud.dataBogeda;
            tempSolicitud.dataBogeda2 = tempSolicitud.dataBogeda2;
            tempSolicitud.dataCliente = tempSolicitud.dataCliente;
            tempSolicitud.idreferencia = tempSolicitud.idreferencia;
            tempSolicitud.idkitaccesorios = tempSolicitud.idkitaccesorios;

            context.tempSolicitud.Add(tempSolicitud);
            int result = context.SaveChanges();

            if (result == 1)
            {
                data = 1;
            }
            else
            {
                data = 0;
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Ver(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            encab_documento encabezado = context.encab_documento.Find(id);
            if (encabezado == null)
            {
                return HttpNotFound();
            }

            TrasladoModel traslado = new TrasladoModel
            {
                TipoDocumento = encabezado.tipo,
                BodegaOrigen = encabezado.bodega,
                BodegaDestino = encabezado.bodega_destino ?? 0,
                PerfilContable = encabezado.perfilcontable ?? 0,
                UsuarioRecepcion = encabezado.nit,
                encabezado_id = encabezado.idencabezado,
                Notas = encabezado.notas
            };

            ViewBag.documentoSeleccionado = encabezado.tipo;
            ViewBag.bodegaSeleccionada = encabezado.bodega;
            ViewBag.usuarioSeleccionado = encabezado.destinatario;
            ViewBag.perfilSeleccionado = encabezado.perfilcontable;
            ViewBag.tp_doc_registros = context.tp_doc_registros.ToList();
            ViewBag.BodegaDestino =
                new SelectList(context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).Where(x => x.es_repuestos),
                    "id", "bodccs_nombre", encabezado.bodega_destino);
            BuscarFavoritos(menu);
            return View(traslado);
        }

        public ActionResult Traslado(int? id)
        {
            var buscarTraslado = (from doc in context.encab_documento
                                  join origen in context.bodega_concesionario
                                      on doc.bodega equals origen.id
                                  join t in context.tp_doc_registros
                                      on doc.tipo equals t.tpdoc_id
                                  join destino in context.bodega_concesionario
                                      on doc.bodega_destino equals destino.id
                                  where doc.idencabezado == id
                                  select new
                                  {
                                      fecha = doc.fecha.ToString(),
                                      numeroRegistro = doc.numero,
                                      origenBodega = origen.bodccs_nombre,
                                      destinoBodegaID = destino.id,
                                      destinoBodega = destino.bodccs_nombre,
                                      doc.notas,
                                      tipoEntrada = t.tpdoc_nombre,
                                      TotalTotales = 0
                                  }).FirstOrDefault();


            var buscarUbicacion = (from a in context.encab_documento
                                   join b in context.lineas_documento
                                       on a.idencabezado equals b.id_encabezado
                                   join c in context.icb_referencia
                                       on b.codigo equals c.ref_codigo
                                   join d in context.ubicacion_repuesto
                                       on new { x1 = a.bodega, x2 = b.codigo } equals new { x1 = d.bodega, x2 = d.codigo } into zz
                                   from d in zz.DefaultIfEmpty()
                                   join e in context.ubicacion_repuestobod
                                       on d.ubicacion equals e.id into xx
                                   from e in xx.DefaultIfEmpty()
                                   where a.idencabezado == id && a.bodega_destino == buscarTraslado.destinoBodegaID
                                   select new
                                   {
                                       e.descripcion
                                   }).Distinct().ToList();
            string root = Server.MapPath("~/Pdf/");
            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
            string path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);
            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");


            PDFmodel modeloTraslado = new PDFmodel
            {
                tipoEntrada = buscarTraslado.tipoEntrada,
                numeroRegistro = buscarTraslado.numeroRegistro,
                fechaRegistro = buscarTraslado.fecha,
                bodegaOrigen = buscarTraslado.origenBodega,
                destinoID = buscarTraslado.destinoBodegaID,
                BodegaDestino = buscarTraslado.destinoBodega,
                notas = buscarTraslado.notas,
                referencias = (from a in context.encab_documento
                               join b in context.lineas_documento
                                   on a.idencabezado equals b.id_encabezado
                               join c in context.icb_referencia
                                   on b.codigo equals c.ref_codigo
                               where a.idencabezado == id && a.bodega_destino == buscarTraslado.destinoBodegaID
                               select new referenciasPDF
                               {
                                   codigo = b.codigo,
                                   cantidad = b.cantidad,
                                   descripcion = c.ref_descripcion,
                                   costo = b.costo_unitario,
                                   total = b.costo_unitario * b.cantidad,
                                   secuencia = b.seq
                               }).Distinct().ToList()
            };

            modeloTraslado.referencias.ForEach(item2 => item2.total = Convert.ToDecimal(item2.total));
            modeloTraslado.referencias.ForEach(item2 => item2.total = Math.Truncate(item2.total));
            modeloTraslado.referencias.ForEach(item2 => item2.costo = Math.Truncate(item2.costo));
            modeloTraslado.referencias.ForEach(item2 => item2.cantidad = Convert.ToInt32(item2.cantidad));

            decimal totales = modeloTraslado.referencias.Sum(d => d.total);
            modeloTraslado.TotalTotales = Convert.ToDecimal(totales);


            ViewAsPdf something = new ViewAsPdf("traslado", modeloTraslado);
            return something;
        }

        public JsonResult BuscarReferenciasAgregadas(int? encabezado_id)
        {
            var buscarReferencias = (from lineas in context.lineas_documento
                                     join referencia in context.icb_referencia
                                         on lineas.codigo equals referencia.ref_codigo
                                     where lineas.id_encabezado == encabezado_id
                                     select new
                                     {
                                         lineas.seq,
                                         lineas.cantidad,
                                         lineas.codigo,
                                         lineas.costo_unitario,
                                         referencia.ref_descripcion
                                     }).Distinct().ToList();

            return Json(buscarReferencias, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarReferencias(string referencia_id, int bodega_id)
        {
            DateTime fechaHoy = DateTime.Now;
            //var bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            var buscarReferencia = (from referencia in context.icb_referencia
                                    join promedio in context.vw_promedio
                                        on referencia.ref_codigo equals promedio.codigo into pro
                                    from promedio in pro.DefaultIfEmpty()
                                    join inventarioHoy in context.vw_inventario_hoy
                                        on referencia.ref_codigo equals inventarioHoy.ref_codigo into hoy
                                    from inventarioHoy in hoy.DefaultIfEmpty()
                                    where referencia.ref_codigo == referencia_id && inventarioHoy.bodega == bodega_id &&
                                          promedio.ano == fechaHoy.Year && promedio.mes == fechaHoy.Month
                                    select new
                                    {
                                        referencia.ref_codigo,
                                        referencia.ref_descripcion,
                                        inventarioHoy.costo_prom.Value,
                                        prom = promedio.Promedio.Value ,
                                        prominvent =inventarioHoy.costo_prom.Value,
                                        promref = referencia.costo_promedio.Value,                                        
                                        Promedio = inventarioHoy.costo_prom.Value  > 0 ? inventarioHoy.costo_prom.Value:promedio.Promedio!=null || promedio.Promedio.Value > 0 ? promedio.Promedio.Value:referencia.costo_promedio!=null?referencia.costo_promedio.Value:0,
                                        inventarioHoy.stock
                                    }).FirstOrDefault();
            if (buscarReferencia == null)
            {
                return Json(new { encontrado = false, buscarReferencia }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { encontrado = true, buscarReferencia }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarBodegasPorDocumento(int? idTpDoc)
        {
            
            if (idTpDoc != null)
            {
                int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                //idTpDoc = 1028;
                var buscarBodegasDelDocumento = (from doc_consecutivos in context.icb_doc_consecutivos
                                                 join bodega in context.bodega_concesionario
                                                     on doc_consecutivos.doccons_bodega equals bodega.id
                                                 where doc_consecutivos.doccons_idtpdoc == idTpDoc && doc_consecutivos.doccons_bodega == bodegaActual &&
                                                       bodega.es_repuestos
                                                 select new
                                                 {
                                                     bodega.id,
                                                     bodega.bodccs_nombre
                                                 }).ToList();

                return Json(buscarBodegasDelDocumento, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(0, JsonRequestBehavior.AllowGet);

            }

        }

  
        public JsonResult BuscarBodegasSol(int? idTpDoc)
        {
           
            var buscarBodegasDelDocumento = (from  bodega in context.bodega_concesionario                                            
                                             select new
                                             {
                                                 bodega.id,
                                                 bodega.bodccs_nombre
                                             }).ToList();

            return Json(buscarBodegasDelDocumento, JsonRequestBehavior.AllowGet);
        }


        public JsonResult traerReferencias(string codigo /*codigo de la referencia a buscar*/)
        {
            var referencias = (from r in context.icb_referencia                              
                               select new
                               {
                                   referencia = r.ref_codigo,
                                   nombre = r.ref_codigo + " | " + r.ref_descripcion
                               }).ToList();
      
            //referencias_array = new string[referencias.Count];
            return Json(referencias);
        }


        //GET
        public ActionResult destinoTraslado(int? menu)
        {
            DateTime fechainicial = DateTime.Now.AddMonths(-1);
            DateTime hoy = DateTime.Now;
            ViewBag.fechaini = fechainicial.ToString("yyyy/MM/dd");
            ViewBag.fechafin = hoy.ToString("yyyy/MM/dd");

            return View();
        }

        [HttpPost]
        public ActionResult destinoTraslado(recibidoTrasladosModel recibido, int? menu)
        {
            string lista = Request["listaReferencias"];
            int anio = DateTime.Now.Year;
            int mes = DateTime.Now.Month;
            int bodega = Convert.ToInt32(Session["user_bodega"]);
            int num = Convert.ToInt32(lista);
            //int documentointerno = 0;
            grupoconsecutivos grupo2 = new grupoconsecutivos();
            long consecutivo2 = 0;
            //tipo de documento recepcion de traslado taller
            icb_sysparameter tipotra = context.icb_sysparameter.Where(d => d.syspar_cod == "P108")
                .FirstOrDefault();
            int tipotraslado = tipotra != null ? Convert.ToInt32(tipotra.syspar_value) : 3076;

            if (!string.IsNullOrEmpty(lista))
            {
                var listadoreci = new List<listadoRecibidos>();
                for (int i = 0; i <= num; i++)
                {
                    int chequeado = Convert.ToInt32(Request["checkbox" + i]);
                    if (chequeado == 1)
                    {
                        int idRecibido = Convert.ToInt32(Request["oculto" + i]);
                        string cantidaditem = Request["cantidad_" + i];
                        bool convertir = int.TryParse(cantidaditem, out int cantidad2);
                        recibidotraslados busqueda = context.recibidotraslados.FirstOrDefault(x => x.id == idRecibido);

                        if (busqueda != null && convertir)
                        {
                            //añado la referencia en el arreglo a guardar
                            listadoreci.Add(new listadoRecibidos {
                            idRecibido=idRecibido,
                            cantidad=cantidad2,
                            costo= Convert.ToDecimal(Request["costo_" + i],new CultureInfo("is-IS")),
                            bodega =busqueda.iddestino
                            });
                        }
                        else
                        {
                            if (busqueda == null)
                            {
                                TempData["mensaje_error"] +=
                                    " <br /> Error en guardado de la referencia " + idRecibido +
                                    " La referencia no existe para recepción";
                            }

                            if (convertir == false)
                            {
                                TempData["mensaje_error"] +=
                                    " <br /> Error en guardado de la referencia " + busqueda.refcodigo +
                                    " Debe especificar una cantidad válida";
                            }
                        }
                    }
                }

                using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                {
                    try
                    {
                        //guardado de los traslados
                        var cantidadaguardar = listadoreci.Count();
                        var listadoconsecutivos = new List<documentoscreados>();
                        if (cantidadaguardar > 0)
                        {
                            var numerobodega = 0;
                            //var idencabezado = 0;
                            //proceso la lista por bodega
                            listadoreci = listadoreci.OrderBy(x => x.bodega).ToList();
                            DocumentoPorBodegaController conse = new DocumentoPorBodegaController();

                            var nuevoencabezado = 0;
                            var nuevodocinterno = 0;
                            foreach (var item in listadoreci)
                            {
                               
                                //busco la información del traslado
                                var infotraslado = context.recibidotraslados.Where(d => d.id == item.idRecibido).FirstOrDefault();
                                var nit = infotraslado.encab_documento.nit;
                                //voy bodega por bodega creando el documento de recepcion de traslado
                                if (item.bodega != numerobodega)
                                {
                                    numerobodega = item.bodega;
                                    //si ya hay consecutivos registrados (es decir hay una segunda iteracion, actualizo y borro del arreglo
                                    if (listadoconsecutivos.Count > 0)
                                    {

                                        foreach (var item2 in listadoconsecutivos)
                                        {
                                            //actualizar el costo total del traslado saliente
                                            var lineasdocumentox = context.lineas_documento.Where(d => d.id_encabezado == item2.encabezado).ToList();
                                            decimal totalencabezado = 0;
                                            foreach (var item33 in lineasdocumentox)
                                            {
                                                var descuento = (item33.costo_unitario * Convert.ToDecimal(item33.porcentaje_descuento, Cultura)) / 100;
                                                var valorref = item33.cantidad * (item33.costo_unitario - descuento);
                                                var iva = valorref * Convert.ToDecimal(item33.porcentaje_iva, Cultura) / 100;
                                                totalencabezado = totalencabezado + valorref + iva;
                                            }

                                            //busco el encabezado respectivo
                                            var encabresp = context.encab_documento.Where(d => d.idencabezado == item2.encabezado).FirstOrDefault();
                                            if (encabresp != null)
                                            {
                                                encabresp.valor_total = totalencabezado;
                                                encabresp.valor_mercancia = totalencabezado;

                                                context.Entry(encabresp).State = EntityState.Modified;
                                                context.SaveChanges();
                                            }
                                            conse.ActualizarConsecutivo(item2.grupo, item2.consecutivo);
                                        }
                                        listadoconsecutivos.RemoveRange(0, 2);
                                    }
                                    //consecutivo
                                    grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                                        x.documento_id == tipotraslado && x.bodega_id == item.bodega);
                                    if (grupo != null)
                                    {
                                        DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                                        long consecutivo = doc.BuscarConsecutivo(grupo.grupo);
                                        //creo un nuevo encabezado
                                        encab_documento encabezado = new encab_documento
                                        {
                                            tipo = tipotraslado,
                                            numero = consecutivo,
                                            nit = nit,
                                            fecha = DateTime.Now,
                                            bodega = item.bodega,
                                            impoconsumo = 0,
                                            destinatario = infotraslado.encab_documento.destinatario,
                                            perfilcontable = infotraslado.encab_documento.perfilcontable,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            fec_creacion = DateTime.Now,
                                        };
                                        context.encab_documento.Add(encabezado);

                                        bool guardar = context.SaveChanges() > 0;
                                        nuevoencabezado = encabezado.idencabezado;
                                        listadoconsecutivos.Add(new documentoscreados
                                        {
                                            encabezado = nuevoencabezado,
                                            consecutivo = consecutivo,
                                            grupo = grupo.grupo,
                                        });

                                        tp_doc_registros doc_interno = context.tp_doc_registros.Where(d => d.tpdoc_id == tipotraslado).FirstOrDefault();
                                        //guardado de documento interno
                                        if (doc_interno.doc_interno_asociado != null)
                                        {
                                            //se consulta consecutivo de documento interno
                                            grupo2 = context.grupoconsecutivos.FirstOrDefault(x => x.documento_id == doc_interno.doc_interno_asociado && x.bodega_id == item.bodega);
                                            if (grupo2 != null)
                                            {
                                                consecutivo2 = doc.BuscarConsecutivo(grupo2.grupo);
                                                //calculo y guardo el encabezado del movimiento interno
                                                encab_documento encabezado2 = new encab_documento
                                                {
                                                    tipo = doc_interno.doc_interno_asociado.Value,
                                                    numero = consecutivo2,
                                                    nit = encabezado.nit,
                                                    fecha = DateTime.Now,
                                                    fpago_id = encabezado.fpago_id,
                                                    vencimiento = encabezado.vencimiento,
                                                    valor_total = encabezado.valor_total,
                                                    iva = encabezado.iva,
                                                    porcen_retencion = encabezado.porcen_retencion,
                                                    retencion = encabezado.retencion,
                                                    porcen_reteiva = encabezado.porcen_reteiva,
                                                    retencion_iva = encabezado.retencion_iva,
                                                    porcen_retica = encabezado.porcen_retica,
                                                    retencion_ica = encabezado.retencion_ica,
                                                    fletes = encabezado.fletes,
                                                    iva_fletes = encabezado.iva_fletes,
                                                    costo = encabezado.costo,
                                                    vendedor = encabezado.vendedor,
                                                    documento = encabezado.documento,
                                                    remision = encabezado.remision,
                                                    bodega = encabezado.bodega,
                                                    concepto = encabezado.concepto,
                                                    moneda = encabezado.moneda,
                                                    perfilcontable = encabezado.perfilcontable,
                                                    valor_mercancia = encabezado.valor_mercancia,
                                                    fec_creacion = encabezado.fec_creacion,
                                                    userid_creacion = encabezado.userid_creacion,
                                                    estado = true,
                                                    concepto2 = encabezado.concepto2,
                                                    id_movimiento_interno = encabezado.idencabezado,
                                                };
                                                context.encab_documento.Add(encabezado2);
                                                context.SaveChanges();
                                                nuevodocinterno = encabezado2.idencabezado;

                                                listadoconsecutivos.Add(new documentoscreados
                                                {
                                                    encabezado = nuevodocinterno,
                                                    consecutivo = consecutivo2,
                                                    grupo = grupo2.grupo,
                                                });
                                            }
                                        }
                                        //la parte del documento interno
                                    }
                                    else
                                    {
                                        dbTran.Rollback();
                                        TempData["mensaje_error"] = "No se puede guardar traslado porque no existe consecutivo";
                                        break;
                                    }
                                }
                                //busco la referencia saliente
                                var esptraslado = context.recibidotraslados.Where(d => d.id == item.idRecibido).First();
                                var lineaorigen = context.lineas_documento.Where(d => d.id_encabezado == esptraslado.encab_documento.idencabezado && d.codigo == esptraslado.refcodigo && d.cantidad == esptraslado.cantidad).FirstOrDefault();
                                //creo la linea documento del encabezado
                                lineas_documento lineas2 = new lineas_documento
                                {
                                    id_encabezado = nuevoencabezado,
                                    codigo = esptraslado.refcodigo,
                                    seq = 1,
                                    fec = DateTime.Now,
                                    nit = lineaorigen.nit,
                                    cantidad = item.cantidad,
                                    porcentaje_iva = lineaorigen.porcentaje_iva,
                                    valor_unitario = lineaorigen.valor_unitario,
                                    porcentaje_descuento = lineaorigen.porcentaje_descuento,
                                    costo_unitario = lineaorigen.costo_unitario,
                                    bodega = item.bodega,
                                    fec_creacion = DateTime.Now,
                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                    tasa = lineaorigen.tasa,
                                    estado = true,

                                };

                                context.lineas_documento.Add(lineas2);
                                context.SaveChanges();

                                //Referencias Inven
                                //registro de referencias del documento interno (si existe)
                                if (nuevodocinterno != 0)
                                {
                                    lineas_documento lineas3 = new lineas_documento
                                    {
                                        id_encabezado = nuevodocinterno,
                                        codigo = lineas2.codigo,
                                        seq = 1,
                                        fec = DateTime.Now,
                                        nit = lineas2.nit,
                                        cantidad = lineas2.cantidad,
                                        porcentaje_iva = lineas2.porcentaje_iva,
                                        valor_unitario = lineas2.valor_unitario,
                                        porcentaje_descuento = lineas2.porcentaje_descuento,
                                        costo_unitario = lineas2.costo_unitario,
                                        bodega = lineas2.bodega,
                                        fec_creacion = lineas2.fec_creacion,
                                        userid_creacion = lineas2.userid_creacion,
                                        tasa = lineas2.tasa,
                                    };

                                    context.lineas_documento.Add(lineas3);
                                    context.SaveChanges();

                                }

                                //region para actualizar inventario
                                int cantidad = item.cantidad;
                                //busco la referencia
                                var busqueda2 = context.recibidotraslados.Where(d => d.id == item.idRecibido).FirstOrDefault();
                                int cantidadcompleta = (busqueda2.cant_recibida != null ? busqueda2.cant_recibida.Value : 0) +
                                                       cantidad;
                                busqueda2.cant_recibida =
                                    (busqueda2.cant_recibida != null ? busqueda2.cant_recibida.Value : 0) + cantidad;
                                busqueda2.recibido = true;
                                busqueda2.notas = recibido.notas;
                                busqueda2.fecharecibido = DateTime.Now;
                                busqueda2.userrecibido = Convert.ToInt32(Session["user_usuarioid"]);
                                string estado = "RECIBIDO";
                                string descripcionevento = "Recepcion de " + cantidad + " unidades de la referencia " +
                                                        busqueda2.icb_referencia.ref_descripcion + " (" +
                                                        busqueda2.refcodigo + ") en la bodega " +
                                                        busqueda2.bodega_concesionario1.bodccs_nombre + " desde la bodega " +
                                                        busqueda2.bodega_concesionario.bodccs_nombre;
                                if (cantidad == cantidadcompleta)
                                {
                                    busqueda2.recibo_completo = true;
                                    busqueda2.estado_traslado = 5;
                                    descripcionevento = descripcionevento + " Recibido completo";
                                }
                                else
                                {
                                    estado = "FALTANTE";
                                    int cantidadfaltante =
                                        busqueda2.cantidad -
                                        ((busqueda2.cant_recibida != null ? busqueda2.cant_recibida.Value : 0) + cantidad);
                                    descripcionevento = descripcionevento + " Faltan " + cantidadfaltante + " unidades.";
                                    busqueda2.recibo_completo = false;
                                }

                                context.Entry(busqueda2).State = EntityState.Modified;

                                //creo el tracking de ese traslado
                                tracking_traslado track = new tracking_traslado
                                {
                                    id_recibotraslado = busqueda2.id,
                                    estado_evento = estado,
                                    descripcion_evento = descripcionevento,
                                    fecha_evento = DateTime.Now,
                                    id_usuario_creacion = busqueda2.userrecibido != null
                                        ? busqueda2.userrecibido.Value
                                        : busqueda2.usertraslado
                                };
                                context.tracking_traslado.Add(track);

                                referencias_inven inven = context.referencias_inven.FirstOrDefault(x =>
                                    x.ano == anio && x.mes == mes && x.codigo == busqueda2.refcodigo && x.bodega == bodega);
                                vw_promedio buscarPromedio = context.vw_promedio.FirstOrDefault(x =>
                                    x.codigo == busqueda2.refcodigo && x.ano == DateTime.Now.Year &&
                                    x.mes == DateTime.Now.Month);
                                decimal promedio = buscarPromedio != null ? buscarPromedio.Promedio ?? 0 : 0;

                                //verifico la naturaleza del documento interno asociado
                                var entrada = true;
                                if (nuevodocinterno != 0)
                                {//calculo el comportamiento del documento interno asociado
                                    var docinterno2 = context.encab_documento.Where(d => d.idencabezado == nuevodocinterno).FirstOrDefault();
                                    var docinternoaso = context.tp_doc_registros.Where(d => d.tpdoc_id == docinterno2.tipo).FirstOrDefault();
                                    if (docinternoaso.entrada_salida != null)
                                    {
                                        entrada = docinternoaso.entrada_salida.Value;
                                    }
                                }
                                if (inven != null)
                                {

                                    CsCalcularCostoPromedio calcular = new CsCalcularCostoPromedio();

                                    decimal Costo_prom = calcular.calcularCostoPromedio(Convert.ToDecimal(item.cantidad, Cultura), Convert.ToDecimal(lineaorigen.valor_unitario, Cultura), lineaorigen.codigo, bodega);
                                    if (entrada == true)
                                    {
                                        inven.can_ent = inven.can_ent + item.cantidad;
                                        inven.can_tra = inven.can_tra + item.cantidad;
                                        inven.cos_ent = inven.cos_ent + promedio * item.cantidad;
                                        inven.cos_tra = inven.cos_tra + promedio * item.cantidad;
                                        inven.costo_prom = Costo_prom;
                                    }
                                    else
                                    {
                                        inven.can_sal = inven.can_sal + item.cantidad;
                                        inven.can_tra = inven.can_tra + item.cantidad;
                                        inven.cos_sal = inven.cos_sal + promedio * item.cantidad;
                                        inven.cos_tra = inven.cos_tra + promedio * item.cantidad;
                                    }

                                    context.Entry(inven).State = EntityState.Modified;
                                }
                                else
                                {
                                    referencias_inven nuevo = new referencias_inven
                                    {
                                        bodega = bodega,
                                        codigo = busqueda2.refcodigo,
                                        ano = (short)DateTime.Now.Year,
                                        mes = (short)DateTime.Now.Month,
                                        can_ini = 0,
                                        cos_ini = 0,
                                        can_dev_vta = 0,
                                        cos_dev_vta = 0,
                                        val_dev = 0,
                                        can_com = 0,
                                        can_dev_com = 0,
                                        can_otr_ent = 0,
                                        cos_otr_ent = 0,
                                        can_otr_sal = 0,
                                        can_tra = item.cantidad,
                                        cos_tra = item.costo,
                                        sub_cos = 0,
                                        baj_cos = 0,
                                        nro_vta = 0,
                                        nro_dev_vta = 0,
                                        nro_com = 0,
                                        nro_dev_com = 0,
                                        nro_ped = null,
                                        can_ped = 0,
                                        modulo = "R",
                                        cos_ini_aju = 0,
                                        cos_ent_aju = 0,
                                        cos_sal_aju = 0,
                                        cos_ini_niif = 0,
                                        cos_ent_niif = 0,
                                        cos_sal_niif = 0
                                    };
                                    if (entrada == true)
                                    {
                                        nuevo.cos_ent = item.costo;
                                        nuevo.can_ent = item.cantidad;
                                        nuevo.cos_sal = item.costo;
                                        nuevo.costo_prom = item.costo;
                                        nuevo.can_vta = 0;
                                        nuevo.val_vta = 0;
                                    }
                                    else
                                    {
                                        nuevo.cos_ent = item.costo;
                                        nuevo.cos_sal = item.costo;
                                        nuevo.costo_prom = item.costo;
                                        nuevo.can_vta = 0;
                                        nuevo.val_vta = 0;
                                        nuevo.can_sal = item.cantidad;

                                    }


                                    context.referencias_inven.Add(nuevo);
                                }
                                context.SaveChanges();
                                //en este segmento actualizo el estado del recibidotraslado y veo si proviene de un agendamiento y le pongo pues, su estado.
                                var reci = context.recibidotraslados.Where(d => d.id == item.idRecibido).FirstOrDefault();
                                var cantidadrecibir = item.cantidad;
                                //var cantidadactual = reci.cant_recibida!=null?reci.cant_recibida.Value:0;
                                //var cantidad_pedida = reci.cantidad;
                                //var diferencia = cantidad_pedida - cantidadactual;
                                //var cantidadrecibir = 0;
                                //if (item.cantidad >= diferencia)
                                //{
                                //    reci.estado_traslado = 5;
                                //    reci.recibo_completo = true;
                                //    cantidadrecibir = diferencia;
                                //}
                                //else
                                //{
                                //    cantidadrecibir = item.cantidad;
                                //}
                                //reci.cant_recibida = cantidadactual + cantidadrecibir;
                                //context.Entry(reci).State = EntityState.Modified;
                                //context.SaveChanges();

                                //var completo = 0;
                                
                                //verifico si hay otros recibidotraslados que compartan el mismo encabezado y cuyo estado sea 5;
                                var otrosreci = context.recibidotraslados.Where(d => d.estado_traslado < 5 && d.idtraslado == reci.idtraslado).Count();
                                /*if (otrosreci == 0)
                                {
                                    completo = 1;
                                }*/
                                //veo si este documento tiene una linea relacionada.
                                if (reci.idlinea != null)
                                {
                                    var existelinea = context.lineas_documento.Where(d => d.id == reci.idlinea).FirstOrDefault();
                                    if (existelinea != null)
                                    {
                                        //luego determino si ese recibidotraslado viene de un documento de traslado, que proviene a su vez de una solicitud de traslado.
                                        var solicitudtrasladox = existelinea.encab_documento.id_solicitud_traslado;
                                        if (solicitudtrasladox != null)
                                        {
                                            var trals = context.Detalle_Solicitud_Traslado.Where(d => d.Id_Solicitud_Traslado == solicitudtrasladox && d.Cod_referencia == existelinea.codigo).FirstOrDefault();

                                            //si proviene de una solicitud de traslado, busco la rseparacionmercancia y adjudico
                                            if (trals != null)
                                            {
                                                if (trals.idseparacion != null)
                                                {
                                                    var separx = context.rseparacionmercancia.Where(d => d.id == trals.idseparacion).FirstOrDefault();
                                                    if (separx != null)
                                                    {
                                                        //var completosepa = 0;
                                                        var cantidadsepa = separx.cantidad;
                                                        var cantidadrecisepa = separx.cantidad_recibida!=null?separx.cantidad_recibida.Value:0;
                                                        var diferenciasepa = cantidadsepa - cantidadrecisepa;
                                                        var cantidadadjudicarsepa = 0;
                                                        if (cantidadrecibir >= diferenciasepa)
                                                        {
                                                            //completosepa = 1;
                                                            cantidadadjudicarsepa = diferenciasepa;
                                                        }
                                                        else
                                                        {
                                                            cantidadadjudicarsepa = cantidadrecibir;
                                                        }
                                                        //le adjudico la cantidad a la separacion
                                                        separx.cantidad_recibida = cantidadrecisepa + cantidadadjudicarsepa;
                                                        context.Entry(separx).State = EntityState.Modified;
                                                        context.SaveChanges();
                                                        //si dicha rseparacionmercancia viene de un pedido, actualizo la cantidad.
                                                        if (separx.idpedido != null)
                                                        {
                                                            //busco el detalle de ese pedido
                                                            var detallepe = context.icb_referencia_movdetalle.Where(d => d.refmov_id == separx.idpedido && d.ref_codigo == separx.codigo).FirstOrDefault();
                                                            if (detallepe != null)
                                                            {
                                                                var cantsol = Convert.ToInt32(detallepe.refdet_cantidad);
                                                                var cantreci = detallepe.cantidad_recibida;
                                                                var dife = cantsol - cantreci;
                                                                var cantidadagregar = 0;
                                                                if (cantidadrecibir >= dife)
                                                                {
                                                                    detallepe.tiene_stock = true;
                                                                    cantidadagregar = dife;
                                                                }
                                                                else
                                                                {
                                                                    cantidadagregar = cantidadrecibir;
                                                                }
                                                                detallepe.cantidad_recibida = cantreci + cantidadagregar;
                                                                context.Entry(detallepe).State = EntityState.Modified;
                                                                context.SaveChanges();
                                                            }


                                                        }

                                                    }
                                                }
                                            }
                                        }

                                    }

                                }


                                var existeenagenda = context.detalle_agendamiento.Where(d => d.fk_recibidotraslado == item.idRecibido).FirstOrDefault();
                                if (existeenagenda != null)
                                {
                                    if (reci.estado_traslado == 5)
                                    {
                                        var agen = context.agenda_mensajero.Where(d => d.id == existeenagenda.fk_agendamiento).FirstOrDefault();
                                        if (agen != null)
                                        {
                                            agen.idestado = 5;
                                            context.Entry(agen).State = EntityState.Modified;
                                            context.SaveChanges();
                                        }
                                    }
                                }
                                



                            }

                            //actualizo consecutivos finales
                            if (listadoconsecutivos.Count > 0)
                            {


                                foreach (var item2 in listadoconsecutivos)
                                {
                                    //actualizar el costo total del traslado saliente
                                    var lineasdocumentox = context.lineas_documento.Where(d => d.id_encabezado == item2.encabezado).ToList();
                                    decimal totalencabezado = 0;
                                    foreach (var item in lineasdocumentox)
                                    {
                                        var descuento = (item.costo_unitario * Convert.ToDecimal(item.porcentaje_descuento, Cultura)) / 100;
                                        var valorref = item.cantidad * (item.costo_unitario - descuento);
                                        var iva = valorref * Convert.ToDecimal(item.porcentaje_iva, Cultura) / 100;
                                        totalencabezado = totalencabezado + valorref + iva;
                                    }

                                    //busco el encabezado respectivo
                                    var encabresp = context.encab_documento.Where(d => d.idencabezado == item2.encabezado).FirstOrDefault();
                                    if (encabresp != null)
                                    {
                                        encabresp.valor_total = totalencabezado;
                                        encabresp.valor_mercancia = totalencabezado;

                                        context.Entry(encabresp).State = EntityState.Modified;
                                        context.SaveChanges();
                                    }
                                    
                                    conse.ActualizarConsecutivo(item2.grupo, item2.consecutivo);
                                }
                                listadoconsecutivos.RemoveRange(0, 2);
                            }

                            dbTran.Commit();
                        }


                    }
                    catch (DbEntityValidationException)
                    {
                        dbTran.Rollback();
                        throw;
                    }
                }
                   

                //context.SaveChanges();
            }
            else
            {
                TempData["mensaje_error"] += " No se ha marcado ningun repuesto a ser recibido";
            }

            return View();
        }

        //public JsonResult cargarTrasladosDestino()
        //{
        //    int bodegaActual = Convert.ToInt32(Session["user_bodega"]);

        //    var buscar = (from a in context.recibidotraslados
        //                  join b in context.encab_documento
        //                      on a.idtraslado equals b.idencabezado
        //                  join c in context.icb_referencia
        //                      on a.refcodigo equals c.ref_codigo
        //                  join d in context.bodega_concesionario
        //                      on a.idorigen equals d.id
        //                  join dd in context.bodega_concesionario
        //                      on a.iddestino equals dd.id
        //                  join e in context.users
        //                      on a.usertraslado equals e.user_id
        //                  where a.iddestino == bodegaActual && a.recibo_completo == false && a.tipo == "R" && b.requiere_mensajeria == false
        //                  select new
        //                  {
        //                      a.id,
        //                      b.numero,
        //                      a.refcodigo,
        //                      c.ref_descripcion,
        //                      a.cantidad,
        //                      a.recibido,
        //                      a.fechatraslado,
        //                      a.costo,
        //                      origen = d.bodccs_nombre,
        //                      destino = dd.bodccs_nombre,
        //                      cantidad_pendiente = a.recibido == true ? a.cant_recibida != null ? a.cantidad - a.cant_recibida : a.cantidad : a.cantidad,
        //                      esfaltante = a.recibido && a.recibo_completo == false ? 1 : 0,
        //                      destinatario = b.destinatario != null ? b.users2.user_nombre.ToString() + " " + b.users2.user_apellido.ToString() : "",
        //                  }).ToList();

        //    var data = buscar.Select(x => new
        //    {
        //        x.id,
        //        x.numero,
        //        cod_referencia = x.refcodigo,
        //        referencia = x.refcodigo + " - " + x.ref_descripcion,
        //        x.cantidad,
        //        x.cantidad_pendiente,
        //        x.esfaltante,
        //        x.recibido,
        //        costo = x.costo.ToString("0,0", elGR),
        //        fechatraslado = x.fechatraslado.ToString("yyyy/MM/dd HH:mm"),
        //        x.destino,
        //        x.origen,
        //        x.destinatario
        //    });


        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}
        public JsonResult cargarTrasladosDestino()
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);

            var buscar = (from a in context.recibidotraslados
                          join c in context.icb_referencia
                              on a.refcodigo equals c.ref_codigo
                          join b in context.encab_documento
                          on a.idtraslado equals b.idencabezado
                          join d in context.bodega_concesionario
                              on a.idorigen equals d.id
                          join dd in context.bodega_concesionario
                              on a.iddestino equals dd.id
                          join e in context.users
                              on a.usertraslado equals e.user_id
                          where a.iddestino == bodegaActual && a.recibo_completo == false && a.tipo == "R" && a.estado_traslado == 4 && a.devolucion == false
                          select new
                          {
                              a.id,
                              a.encab_documento.numero,
                              a.refcodigo,
                              c.ref_descripcion,
                              a.cantidad,
                              a.recibido,
                              a.fechatraslado,
                              a.costo,
                              origen = d.bodccs_nombre,
                              destino = dd.bodccs_nombre,
                              cantidad_pendiente = a.recibido == true ? a.cant_recibida != null ? (a.cantidad - a.cant_recibida) : a.cantidad : a.cantidad,
                              esfaltante = (a.recibido && a.recibo_completo == false) ? 1 : 0,
                              destinatario = a.encab_documento.destinatario != null ? a.encab_documento.users2.user_nombre.ToString() + " " + a.encab_documento.users2.user_apellido.ToString() : "",
                          }).ToList();

            var data = buscar.Select(x => new
            {
                x.id,
                x.numero,
                cod_referencia = x.refcodigo,
                referencia = x.refcodigo + " - " + x.ref_descripcion,
                x.cantidad,
                x.cantidad_pendiente,
                x.esfaltante,
                x.recibido,
                costo = x.costo.ToString("0,0", elGR),
                fechatraslado = x.fechatraslado.ToString("yyyy/MM/dd HH:mm"),

                x.destino,
                x.origen,
                x.destinatario
            });


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult devolverSolicitud(int? id, int? bodega_origen, int? bodega_destino)
        {
            grupoconsecutivos grupo2 = new grupoconsecutivos();

            var buscar = context.recibidotraslados.Where(d => d.id == id).FirstOrDefault();
            var cantidad = Convert.ToInt32(buscar.cantidad) - Convert.ToInt32(buscar.recibido);
            var referencia = buscar.refcodigo;

            var recibidotraslados = context.recibidotraslados.Find(id);

            recibidotraslados.devolucion = true;
            recibidotraslados.recibo_completo = false;

            context.Entry(recibidotraslados).State = EntityState.Modified;
            context.SaveChanges();



            if (buscar != null)
            {

                using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                {
                    try
                    {
                        #region MyRegion

                        long numeroConsecutivo = 0;
                        grupoconsecutivos buscarGrupoConsecutivos = context.grupoconsecutivos.FirstOrDefault(x =>
                            x.documento_id == 3099 && x.bodega_id == bodega_origen);
                        int numeroGrupo = buscarGrupoConsecutivos != null ? buscarGrupoConsecutivos.grupo : 0;
                        DocumentoPorBodegaController doc = new DocumentoPorBodegaController();

                        ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
                        icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
                        tp_doc_registros buscarTipoDocRegistro =
                            context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == 3099);
                        numeroConsecutivoAux = gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro, 3099, Convert.ToInt32(bodega_origen));


                        if (numeroConsecutivoAux != null)
                        {
                            numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                        }
                        else
                        {
                            TempData["mensaje_error"] = "No existe un numero consecutivo asignado para este tipo de documento";
                            dbTran.Rollback();
                            return Json(0, JsonRequestBehavior.AllowGet);
                        }

                        // Si llega aqui significa que si hay un numero de documento consecutivo

                        // Significa que al menos una referencia se encuentra en referecias inven en la bodega de origen
                        // Se supone que si existe al menos una que este en el origen se hace el proceso

                        icb_sysparameter buscarNit = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P33");
                        int nitTraslado = buscarNit != null ? Convert.ToInt32(buscarNit.syspar_value) : 0;

                        encab_documento crearEncabezado = new encab_documento
                        {
                            tipo = 3099,
                            bodega = Convert.ToInt32(bodega_origen),
                            numero = numeroConsecutivo,
                            ///////crearEncabezado.documento = modelo.Referencia;
                            fecha = DateTime.Now,
                            fec_creacion = DateTime.Now,
                            impoconsumo = 0,
                            destinatario = buscar.encab_documento.destinatario,
                            bodega_destino = Convert.ToInt32(bodega_destino),
                            perfilcontable = buscar.encab_documento.perfilcontable,
                            nit = nitTraslado,
                            requiere_mensajeria = null,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                        };
                        if (crearEncabezado.requiere_mensajeria == null || crearEncabezado.requiere_mensajeria == false || crearEncabezado.requiere_mensajeria == true)
                        {
                            crearEncabezado.mensajeria_atendido = false;
                        }
                        context.encab_documento.Add(crearEncabezado);

                        bool guardar = context.SaveChanges() > 0;


                        encab_documento buscarUltimoEncabezado =
                            context.encab_documento.OrderByDescending(x => x.idencabezado).Where(d => d.idencabezado == crearEncabezado.idencabezado).FirstOrDefault();

                        //veo si el documento externo tiene documento interno asociado

                        tp_doc_registros doc_interno = context.tp_doc_registros.Where(d => d.tpdoc_id == 3099).FirstOrDefault();

                        int sequ = 1;

                        vw_promedio buscarPromedio = context.vw_promedio.FirstOrDefault(x =>
                            x.codigo == referencia && x.ano == DateTime.Now.Year &&
                            x.mes == DateTime.Now.Month);
                        decimal promedio = buscarPromedio != null ? buscarPromedio.Promedio ?? 0 : 0;

                        referencias_inven buscarReferenciasInvenOrigen = context.referencias_inven.FirstOrDefault(x =>
                            x.codigo == referencia && x.ano == DateTime.Now.Year &&
                            x.mes == DateTime.Now.Month && x.bodega == bodega_origen);


                        if (buscarReferenciasInvenOrigen != null)
                        {

                            buscarReferenciasInvenOrigen.can_sal = buscarReferenciasInvenOrigen.can_sal + Convert.ToInt32(cantidad);
                            buscarReferenciasInvenOrigen.cos_sal = buscarReferenciasInvenOrigen.cos_sal + promedio * Convert.ToInt32(cantidad);

                            buscarReferenciasInvenOrigen.can_tra = buscarReferenciasInvenOrigen.can_tra + Convert.ToInt32(cantidad);
                            buscarReferenciasInvenOrigen.cos_tra = buscarReferenciasInvenOrigen.cos_tra + promedio * Convert.ToInt32(cantidad);

                            context.Entry(buscarReferenciasInvenOrigen).State = EntityState.Modified;

                            bool guardar2 = context.SaveChanges() > 0;

                        }

                        lineas_documento crearLineasOrigen = new lineas_documento
                        {
                            codigo = referencia,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            nit = nitTraslado,
                            cantidad = cantidad > 0
                                ? Convert.ToDecimal(cantidad)
                                : 0,
                            bodega = Convert.ToInt32(bodega_origen),
                            seq = sequ,
                            estado = true,
                            fec = DateTime.Now,
                            costo_unitario = buscar.costo > 0
                                ? Convert.ToDecimal(buscar.costo)
                                : 0,
                            valor_unitario = buscar.costo > 0
                                ? Convert.ToDecimal(buscar.costo)
                                : 0,
                            id_encabezado = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0
                        };
                        context.lineas_documento.Add(crearLineasOrigen);

                        bool guardar3 = context.SaveChanges() > 0;
                        #endregion

                        dbTran.Commit();
                        return Json(1, JsonRequestBehavior.AllowGet);
                    }
                    catch (DbEntityValidationException ex)
                    {
                        var mensaje = ex;
                        dbTran.Rollback();
                        return Json(0, JsonRequestBehavior.AllowGet);
                    }

                }
            }
            else
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }

            
        }

        public JsonResult validarcantidad(int? id, int? cantidad)
        {
            int valor = 0;
            int? result = 0;
            string respuesta = "";

            if (id != null && cantidad != null)
            {
                //verifico si id corresponde con un recibidotraslados valido
                recibidotraslados recibo = context.recibidotraslados.Where(d => d.id == id).FirstOrDefault();
                if (recibo != null)
                {
                    int cantidadenviada = recibo.cantidad;
                    int cantidadrecibida = recibo.cant_recibida != null ? recibo.cant_recibida.Value : 0;
                    int diferencia = cantidadenviada - cantidadrecibida;
                    if (diferencia >= cantidad)
                    {
                        valor = 1;
                        result = (diferencia - cantidad);
                        respuesta = "Se puede asignar.";
                    }
                    else 
                    {
                        respuesta =
                            "La cantidad a recibir es mayor a la cantidad total enviada y a la cantidad pendiente por recibir.";

                    }
                }
                else
                {
                    respuesta = "No se ingresó un id de traslado válido";
                }
            }
            else
            {
                respuesta = "Faltan datos";
            }

            var data = new
            {
                valor,
                result,
                respuesta
            };
            return Json(data);
        }

        public JsonResult buscartraslado(string numero)
        {
            int numeroTraslado = Convert.ToInt32(numero);

            int bodegaLog = Convert.ToInt32(Session["user_bodega"]);


            var buscar = (from a in context.recibidotraslados
                          join b in context.icb_referencia
                              on a.refcodigo equals b.ref_codigo
                          join c in context.bodega_concesionario
                              on a.idorigen equals c.id
                          join cc in context.bodega_concesionario
                              on a.iddestino equals cc.id
                          join d in context.users
                              on a.userrecibido equals d.user_id into xx
                          from d in xx.DefaultIfEmpty()
                          join e in context.encab_documento
                              on a.idtraslado equals e.idencabezado
                          where e.numero == numeroTraslado
                          select new
                          {
                              e.numero,
                              a.refcodigo,
                              b.ref_descripcion,
                              a.cantidad,
                              a.fecharecibido,
                              a.fechatraslado,
                              a.costo,
                              origen = c.bodccs_nombre,
                              destino = cc.bodccs_nombre,
                              d.user_nombre,
                              d.user_apellido,
                              a.notas,
                              a.recibido,
                              a.recibo_completo,
                              a.cant_recibida
                          }).ToList();
            if (buscar.Count != 0)
            {
                var data = buscar.Select(x => new
                {
                    x.numero,
                    referencia = x.refcodigo + " - " + x.ref_descripcion,
                    x.cantidad,
                    fecharecibido = x.fecharecibido != null ? x.fecharecibido.Value.ToString("yyyy/MM/dd HH:mm") : "",
                    fechatraslado = x.fechatraslado.ToString("yyyy/MM/dd HH:mm"),
                    costo = x.costo.ToString("0,0", elGR),
                    x.origen,
                    x.destino,
                    asesor = x.user_nombre != null ? x.user_nombre + " " + x.user_apellido : "",
                    notas = x.notas != null ? x.notas : "",
                    x.recibido,
                    x.recibo_completo,
                    cant_recibida = x.cant_recibida != null ? x.cant_recibida : 0
                });

                int buscarPermiso = (from a in context.recibidotraslados
                                     join b in context.encab_documento
                                         on a.idtraslado equals b.idencabezado
                                     where numeroTraslado == b.numero
                                     select b.idencabezado).FirstOrDefault();

                int comparar = context.recibidotraslados.Where(x =>
                    x.idtraslado == buscarPermiso && (x.idorigen == bodegaLog || x.iddestino == bodegaLog)).Count();

                if (comparar >= 1)
                {
                    return Json(new { data, info = true, error = false }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { info = false, error = true }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { info = false, error = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult trasladoPendiente(int? id, string fechaini, string fechafin)
        {
            int bodegalog = Convert.ToInt32(Session["user_bodega"]);

            System.Linq.Expressions.Expression<Func<recibidotraslados, bool>> predicado = PredicateBuilder.True<recibidotraslados>();
            System.Linq.Expressions.Expression<Func<recibidotraslados, bool>> predicado2 = PredicateBuilder.False<recibidotraslados>();

            if (id != null)
            {
                if (id == 1)
                {
                    predicado = predicado.And(a => a.recibido == false);//por recibir
                }
                else if (id == 2)
                {
                    predicado = predicado.And(a => a.recibido == true && a.cant_recibida != null && a.recibo_completo == false);//incompleto
                }
                else if (id == 3)
                {
                    predicado = predicado.And(a => a.recibido==true && a.recibo_completo == true);//recibido
                }
                if (!string.IsNullOrEmpty(fechaini))
                    {

                    DateTime fechain = Convert.ToDateTime(fechaini);
                    predicado = predicado.And(a => a.fechatraslado >= fechain);
                    }
                else {
                    DateTime fecha = DateTime.Now.AddMonths(-1);
                    predicado = predicado.And(a => a.fechatraslado >= fecha);
                    }
                if (!string.IsNullOrEmpty(fechafin))
                    {

                    DateTime fechafn = Convert.ToDateTime(fechafin);
                    predicado = predicado.And(a => a.fechatraslado <= fechafn);
                    }
                else
                    {
                    DateTime fecha2 = DateTime.Now;
                    predicado = predicado.And(a => a.fechatraslado <= fecha2);
                    }




                }

            predicado = predicado.And(a => a.tipo == "R");
            predicado2 = predicado2.Or(a => a.idorigen == bodegalog);
            predicado2 = predicado2.Or(a => a.iddestino == bodegalog);
            predicado = predicado.And(predicado2);

            var buscar2 = context.recibidotraslados.Where(predicado).ToList();

            var buscar = buscar2.Select(a => new
            {
                a.id,
                a.encab_documento.idencabezado,
                a.encab_documento.numero,
                a.refcodigo,
                a.icb_referencia.ref_descripcion,
                a.cantidad,
                a.fecharecibido,
                a.fechatraslado,
                a.costo,
                a.bodega_concesionario.bodccs_nombre,
                id_origen= a.bodega_concesionario.id,
                origen = a.bodega_concesionario.bodccs_nombre,
                id_destino = a.bodega_concesionario1.id,
                destino = a.bodega_concesionario1.bodccs_nombre,
                user_nombre = a.userrecibido != null ? a.users1.user_nombre : "",
                user_apellido = a.userrecibido != null ? a.users1.user_apellido : "",
                a.notas,
                a.recibido,
                a.recibo_completo,
                a.cant_recibida,
                cantidad_faltante = a.cantidad - (a.cant_recibida != null ? a.cant_recibida.Value : 0),
                mensajeria = a.detalle_agendamiento.Where(x=> x.fk_encab_documento== a.encab_documento.idencabezado).Select(x=> x.fk_agendamiento).FirstOrDefault(),
            }).ToList();

            if (buscar.Count != 0)
            {
                var data = buscar.Select(x => new
                {
                    x.id,
                    x.numero,
                    referencia = x.refcodigo + " - " + x.ref_descripcion,
                    x.cantidad,
                    fecharecibido = x.fecharecibido != null ? x.fecharecibido.Value.ToString("yyyy/MM/dd HH:mm") : "",
                    fechatraslado = x.fechatraslado.ToString("yyyy/MM/dd HH:mm"),
                    costo = x.costo.ToString("0,0", elGR),
                    x.id_origen,
                    x.id_destino,
                    x.origen,
                    x.destino,
                    asesor = x.user_nombre != null ? x.user_nombre + " " + x.user_apellido : "",
                    notas = x.notas != null ? x.notas : "",
                    x.recibido,
                    cant_recibida = x.cant_recibida != null ? x.cant_recibida : 0,
                    mensajeria= x.mensajeria != null ? x.mensajeria.ToString() : "",

                });

                return Json(new { data, info = true }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { info = false }, JsonRequestBehavior.AllowGet);
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


        public ActionResult SolicitudTraslado(TrasladoModel modelo,string cadena = "")
        {

            var listRef = (from b in context.tempSolicitud
                           select new { idtemp = b.idtemp, id = b.idreferencia }
               ).OrderByDescending(x => x.idtemp).FirstOrDefault();
            if (listRef.id != null)
            {
                var listaR1 = (
                    from b in context.icb_referencia
                    join c in context.tempSolicitud
                        on b.ref_codigo equals c.idreferencia
                    select new
                    {
                        id = c.idreferencia,
                        idtemp = c.idtemp
                    }).OrderByDescending(x => x.idtemp).FirstOrDefault();
                ViewBag.Referencia1 = listaR1.id;
            }

            /*Estado*/
            var listEstado = (from e in context.Estado
                              select new
                              {
                                  id = e.id,
                                  nombre = e.Tipo
                              }).ToList();

            List<SelectListItem> listaEstado = new List<SelectListItem>();
            foreach (var item in listEstado)
            {
                listaEstado.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }
            ViewBag.Estado_atendido = listaEstado;

            var listaStock = (
                from b in context.icb_referencia
                join c in context.tempSolicitud
                    on b.ref_codigo equals c.idreferencia
                select new
                {
                    id = c.Stock,
                    idtemp = c.idtemp
                }).OrderByDescending(x => x.idtemp).FirstOrDefault();
                ViewBag.Stock_ = listaStock.id;


            var listaB = (
                from b in context.icb_referencia
                join c in context.tempSolicitud
                    on b.ref_codigo equals c.idreferencia
                select new
                {
                    id = c.dataBogeda,
                    idtemp = c.idtemp
                }).OrderByDescending(x => x.idtemp).FirstOrDefault();
            ViewBag.Bodega1 = listaB.id;


            var lista_B2 = (
            from b in context.icb_referencia
            join c in context.tempSolicitud
                on b.ref_codigo equals c.idreferencia
            select new
            {
                id = c.dataBogeda2,
                idtemp = c.idtemp
            }).OrderByDescending(x => x.idtemp).FirstOrDefault();
            ViewBag.Bodega2 = lista_B2.id;

            var listaB1 = (from a in context.bodega_concesionario
                          join c in context.tempSolicitud
                            on a.id equals c.dataBogeda
                          select new
                          {
                              idtemp = c.idtemp,
                              id = c.dataBogeda,
                          }).OrderByDescending(x => x.idtemp).FirstOrDefault();


            var id1 = listaB1.id;

            var listaB2 = (from a in context.bodega_concesionario
                           join c in context.tempSolicitud
                             on a.id equals c.dataBogeda2
                           select new
                           {
                               idtemp = c.idtemp,
                               id = c.dataBogeda2,
                           }).OrderByDescending(x => x.idtemp).FirstOrDefault();
            //ViewBag.Bodega2 = listaB2.id;

            var id2 = listaB2.id;

            var listBodega = (from bodega in context.bodega_concesionario
                              where bodega.id == id1
                              select new
                              {
                                  id = bodega.id,
                                  nombre = bodega.bodccs_nombre

                              }).OrderBy(x => x.id).ToList();


            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in listBodega)
            {
                lista.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }


            ViewBag.BodegaOrigen = lista;

            var listBodega2 = (from bodega in context.bodega_concesionario
                               where bodega.id == id2
                               select new
                               {
                                   id = bodega.id,
                                   nombre = bodega.bodccs_nombre

                               }).OrderBy(x => x.id).ToList();


            List<SelectListItem> lista2 = new List<SelectListItem>();
            foreach (var item in listBodega2)
            {
                lista2.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }

            ViewBag.BodegaDestino = lista2;


            var listReferencias = (from b in context.tempSolicitud
                           select new { idtemp = b.idtemp, id = b.idreferencia }
               ).OrderByDescending(x => x.idtemp).FirstOrDefault();
            if (listReferencias.id != null)
            {
                var listaR1 = (
                    from b in context.icb_referencia
                    join c in context.tempSolicitud
                        on b.ref_codigo equals c.idreferencia
                    select new
                    {
                        id = c.idreferencia,
                        idtemp = c.idtemp
                    }).OrderByDescending(x => x.idtemp).FirstOrDefault();
                ViewBag.Referencia1 = listaR1.id;
            }

            if (cadena != "")
            {
                ViewBag.cadena = cadena;
                var Subcadena = cadena.Split(',');
                int dataBogeda = Convert.ToInt32(Subcadena[0]);
                int dataBogeda2 = Convert.ToInt32(Subcadena[1]);
                int dataCliente = Convert.ToInt32(Subcadena[2]);
                string idreferencia = Subcadena[3];
                int Stock = Convert.ToInt32(Subcadena[4]);

                Solicitud_traslado Solicitudtraslado = new Solicitud_traslado();

                ViewBag.Id_bodega_origen = dataBogeda2;
                ViewBag.Id_bodega_destino = dataBogeda;
                ViewBag.CodReferencia = idreferencia;
                ViewBag.Stock = Stock;
                ViewBag.Cliente = dataCliente;
                ViewBag.Estado_atendido = new SelectList(listEstado, "id","nombre",1);
                return View(Solicitudtraslado);
            }

            var listaC = (from a in context.icb_terceros
                          join c in context.tempSolicitud
                            on a.tercero_id equals c.dataCliente
                          where c.idtemp > 0
                          select new
                          {
                              id = c.dataCliente,
                          }).OrderByDescending(x => x.id).FirstOrDefault();
            ViewBag.Cliente1 = listaC.id;

            return View();
        }

        [HttpPost]
        public ActionResult SolicitudTraslado(Solicitud_traslado Solicitudtraslado)
        {
            string valor = Request["lista_repuestos"];


            /*Estado*/
            var listEstado = (from e in context.Estado
                              select new
                              {
                                  id = e.id,
                                  nombre = e.Tipo
                              }).ToList();

            List<SelectListItem> listaEstado = new List<SelectListItem>();
            foreach (var item in listEstado)
            {
                listaEstado.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }
            ViewBag.Estado_atendido = listaEstado;


            var listaR = (from a in context.referenciaskits
                          join b in context.icb_referencia
                             on a.codigo equals b.ref_codigo
                          join c in context.tempSolicitud
                             on a.idkitaccesorios equals c.idkitaccesorios
                          where c.idtemp > 0
                          select new
                          {
                              id = a.codigo,
                              //nombre = b.ref_descripcion
                          }).OrderByDescending(x => x.id).FirstOrDefault();
            if (listaR!=null)
                {
                ViewBag.Referencia = listaR.id;
                }
           
            //viewBag referencia desde facturacio repuestos
            var listaC = (from a in context.icb_terceros
                          join c in context.tempSolicitud
                            on a.tercero_id equals c.dataCliente
                          where c.idtemp > 0
                          select new
                          {
                              id = c.dataCliente,
                              //nombre = b.ref_descripcion
                          }).OrderByDescending(x => x.id).FirstOrDefault();

            if (listaC!=null)
                {
                ViewBag.Cliente1 = listaC.id;
                }
          

            var listaB1 = (from a in context.bodega_concesionario
                           join c in context.tempSolicitud
                             on a.id equals c.dataBogeda
                           select new
                           {
                               idtemp = c.idtemp,
                               id = c.dataBogeda,
                           }).OrderByDescending(x => x.idtemp).FirstOrDefault();
            
            ViewBag.Bodega1 = listaB1.id;

            var id1 = listaB1.id;

            var listaB2 = (from a in context.bodega_concesionario
                           join c in context.tempSolicitud
                             on a.id equals c.dataBogeda2
                           select new
                           {
                               idtemp = c.idtemp,
                               id = c.dataBogeda2,
                           }).OrderByDescending(x => x.idtemp).FirstOrDefault();
            ViewBag.Bodega2 = listaB2.id;

            var id2 = listaB2.id;

            var listBodega = (from bodega in context.bodega_concesionario
                              where bodega.id == id1
                              select new
                              {
                                  id = bodega.id,
                                  nombre = bodega.bodccs_nombre

                              }).OrderBy(x => x.id).ToList();


            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in listBodega)
            {
                lista.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }


            ViewBag.BodegaOrigen = lista;

            var listBodega2 = (from bodega in context.bodega_concesionario
                               where bodega.id == id2
                               select new
                               {
                                   id = bodega.id,
                                   nombre = bodega.bodccs_nombre

                               }).OrderBy(x => x.id).ToList();


            List<SelectListItem> lista2 = new List<SelectListItem>();
            foreach (var item in listBodega2)
            {
                lista2.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }

            ViewBag.BodegaDestino = lista2;


            if (valor == null || valor != "")
            {
                try
                {
                    Solicitudtraslado.Id_bodega_origen = Convert.ToInt32(Request["Id_bodega_origen_re"]);
                    Solicitudtraslado.Id_bodega_destino = Convert.ToInt32(Request["Id_bodega_destino_re"]);
                    Solicitudtraslado.Fecha_creacion = DateTime.Now;
                    Solicitudtraslado.Estado_atendido = Solicitudtraslado.Estado_atendido;///revisar//////////////////////////////////////////
                    Solicitudtraslado.Id_solicitante = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Solicitud_traslado.Add(Solicitudtraslado);
                    context.SaveChanges();
                    List<string> listaReferencias = new List<string>();
                    int numero_repuestos = Convert.ToInt32(Request["lista_repuestos"]);
                    //int sequ = 1;
                    List<string> codigosReferencias = new List<string>();
                    List<int> CantidadReferencias = new List<int>();
                    for (int i = 1; i <= numero_repuestos; i++)
                    {
                        string referencia_codigo = Request["codigo_referencia" + i];
                        int referencia_cantidad = Convert.ToInt32(Request["cantidad_referencia" + i]);
                        if (!string.IsNullOrEmpty(referencia_codigo))
                        {
                            codigosReferencias.Add(referencia_codigo);

                            CantidadReferencias.Add(referencia_cantidad);
                        }
                    }
                    var id_solicitud_traslado = context.Solicitud_traslado.OrderByDescending(x => x.Id).FirstOrDefault();

                    for (int i = 0; i < numero_repuestos; i++)
                    {
                        string referencia = codigosReferencias[i];
                        int cantidad = Convert.ToInt32(CantidadReferencias[i]);
                        Detalle_Solicitud_Traslado Detalle = new Detalle_Solicitud_Traslado();
                        Detalle.Id_Solicitud_Traslado = Convert.ToInt32(id_solicitud_traslado.Id);
                        Detalle.Cod_referencia =referencia;
                        Detalle.Cantidad = cantidad;
                        context.Detalle_Solicitud_Traslado.Add(Detalle);
                        context.SaveChanges();
                    }


                    TempData["mensaje"] = "La solicitud se ha realizado correctamente";

                  string  cadena6 = Request["cadenast"];

                    return View(Solicitudtraslado);

                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
       

            TempData["mensaje_error"] = "Debe adicionar algun elemento ";

            return View(Solicitudtraslado);

        }


        public ActionResult VerSolicitudesTraslado() {
            if (Session["user_usuarioid"] != null)
            {
                var bodegausuario = Convert.ToInt32(Session["user_bodega"]);
                var bodegas = context.bodega_concesionario.Where(d => d.bodccs_estado == true).Select(d => new { d.id, nombre = d.bodccs_nombre }).ToList();
                var estados = context.Estado.Where(d => d.habilitado == true && d.id>1).Select(d => new { d.id, nombre = d.Tipo }).ToList();
                var bodegaorigen = bodegas.Where(d => d.id == bodegausuario).ToList();
                ViewBag.bodega = new SelectList(bodegaorigen, "id", "nombre");
                ViewBag.bodega2 = new SelectList(bodegas, "id", "nombre");
                ViewBag.bodega3 = new SelectList(bodegaorigen, "id", "nombre");
                ViewBag.bodega4 = new SelectList(bodegas, "id", "nombre");
                ViewBag.estado = new SelectList(estados, "id", "nombre");

                return View();

            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }

        public JsonResult BuscarSolicitudesTrasladosPorGestionar(int? bodega,int? bodega2, string buscar,string fecha_desde,string fecha_hasta,int? estado)
        {
            string draw = Request.Form.GetValues("draw").FirstOrDefault();
            string start = Request.Form.GetValues("start").FirstOrDefault();
            string length = Request.Form.GetValues("length").FirstOrDefault();
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();
            //esto me sirve para reiniciar la consulta cuando ordeno las columnas de menor a mayor y que no me vuelva a recalcular todo
            //ES IMPORTANTE QUE LA COLUMNA EN EL DATATABLE TENGA EL NOMBRE DE LA TABLA O VISTA A CONSULTAR, porque vamos a usarla para ordenar.
            string sortColumn = Request.Form
                .GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]")
                .FirstOrDefault();
            string sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            search = search.Replace(" ", "");
            int pagina = Convert.ToInt32(start);
            int pageSize = Convert.ToInt32(length);

            int skip = 0;
            if (pagina == 0)
            {
                skip = 0;
            }
            else
            {
                skip = pagina;
            }

            Expression<Func<vw_solicitudes_traslados, bool>> predicado = PredicateBuilder.True<vw_solicitudes_traslados>();
            Expression<Func<vw_solicitudes_traslados, bool>> predicado2 = PredicateBuilder.False<vw_solicitudes_traslados>();
            if (estado != null)
            {
                predicado = predicado.And(d => d.Estado_atendido == estado);
            }
            if (bodega != null)
            {
                predicado = predicado.And(d => d.Id_bodega_origen == bodega);
            }
            if (bodega2 != null)
            {
                predicado = predicado.And(d => d.Id_bodega_destino == bodega);
            }
            if (!string.IsNullOrWhiteSpace(fecha_desde))
            {
                var fechax = DateTime.Now;
                var convertir = DateTime.TryParse(fecha_desde, out fechax);
                if (convertir == true)
                {
                    predicado = predicado.And(d => d.Fecha_creacion >= fechax);
                }
            }
            if (!string.IsNullOrWhiteSpace(fecha_hasta))
            {
                var fechax2 = DateTime.Now;
                var convertir = DateTime.TryParse(fecha_hasta, out fechax2);
                if (convertir == true)
                {
                    fechax2 = fechax2.AddDays(1);
                    predicado = predicado.And(d => d.Fecha_creacion <= fechax2);
                }
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                predicado2 = predicado2.Or(d => 1 == 1 && d.numero.ToString().Contains(buscar));
                predicado2 = predicado2.Or(d => 1 == 1 && d.fecha2.ToUpper().Contains(buscar.ToUpper()));
                predicado2 = predicado2.Or(d => 1 == 1 && d.bodegaorigen.Contains(buscar.ToUpper()));
                predicado2 = predicado2.Or(d => 1 == 1 && d.bodegadestino.Contains(buscar.ToUpper()));
                //predicado2 = predicado2.Or(d => 1 == 1 && d.nombre_estado.Contains(buscar.ToUpper()));
                predicado = predicado.And(predicado2);
            }

            int registrostotales = context.vw_solicitudes_traslados.Where(predicado).Count();
            //si el ordenamiento es ascendente o descendente es distinto
            if (pageSize == -1)
            {
                pageSize = registrostotales;
            }

            List<vw_solicitudes_traslados> lista2 = new List<vw_solicitudes_traslados>();


            if (sortColumnDir == "asc")
            {
                lista2 = context.vw_solicitudes_traslados.Where(predicado)
                    .OrderBy(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                lista2 = context.vw_solicitudes_traslados.Where(predicado)
                  .OrderByDescending(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
            }

            var lista = lista2.GroupBy(d=> d.Id).Select(d => new {
            numero= d.Select(x=> x.numero),
            Id= d.Select(x => x.Id).FirstOrDefault(),
            fecha2= d.Select(x => x.fecha2),
            bodegaorigen= d.Select(x => x.bodegaorigen),
            bodegadestino= d.Select(x => x.bodegadestino),
            listaReferencias= d.Select(x => x.listaReferencias),
            nombre_estado= d.Select(x => x.nombre_estado),
            mensajeria_atentido= d.Select(x => x.mensajeria_atentido),
            fk_agendamiento= d.Select(x => x.fk_agendamiento)
            }).ToList();

            return Json(
                        new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = lista },
                        JsonRequestBehavior.AllowGet);
        
        }

        public JsonResult agregarEstado(string id)
        {
            var result = 0;
            Solicitud_traslado e = new Solicitud_traslado();

            int idSolicitud = Convert.ToInt32(id);

            var state = context.Solicitud_traslado.Find(idSolicitud);

            state.Estado_atendido = 3;

            context.Entry(state).State = EntityState.Modified;
            int resultado = context.SaveChanges();

            if (resultado > 0)
            {
                result = 1;
            }
            else
            {
                result = 0;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarSolicitudesTraslados() {
            var data = (from solicitud in context.Solicitud_traslado
                        join bodegaorigen in context.bodega_concesionario on solicitud.Id_bodega_origen equals bodegaorigen.id
                        join bodegadestino in context.bodega_concesionario on solicitud.Id_bodega_destino equals bodegadestino.id
                        join state in context.Estado on solicitud.Estado_atendido equals state.id
                        join doc in context.encab_documento on solicitud.idEncabezado equals doc.idencabezado into left
                        from e in left.DefaultIfEmpty()
                        join b in context.recibidotraslados on e.idencabezado equals b.idtraslado into left3
                        from f in left3.DefaultIfEmpty()
                        join detalle in context.detalle_agendamiento on e.idencabezado equals detalle.fk_encab_documento into left2
                        from d in left2.DefaultIfEmpty()
                            //fk_encab_documento
                        where solicitud.Estado_atendido >1
                        select new
                        {
                            solicitud.Id,
                            solicitud.Fecha_creacion,
                            origen = bodegaorigen.bodccs_nombre,
                            destino = bodegadestino.bodccs_nombre,
                            state.Tipo,
                            e.mensajeria_atendido,
                            d.fk_agendamiento,
                            referencias = (from Detalle in context.Detalle_Solicitud_Traslado
                                           join referencia in context.icb_referencia
                                           on Detalle.Cod_referencia.ToString() equals referencia.ref_codigo
                                           where Detalle.Id_Solicitud_Traslado == solicitud.Id
                                           select new
                                           {
                                               referencia.ref_descripcion
                                           }).Distinct().ToList(),
                                           
                        }).ToList();


            var data2 = data.Select(x => new
            {
                x.Id,
                fecha_creacion = x.Fecha_creacion.ToShortDateString(),
                x.origen,
                x.destino,
                x.referencias,
                x.Tipo,
                mensajeria = x.mensajeria_atendido != true ? "No" : "Si",
                x.fk_agendamiento
            });

            return Json(new { data2 }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarSolicitudTrasladoId(int id) {
           
            var data2 = (from solicitud in context.Solicitud_traslado
                          where solicitud.Id == id
                         select new
                         {
                             solicitud.Id,
                             solicitud.Fecha_creacion,
                             solicitud.Id_bodega_origen,
                             solicitud.Id_bodega_destino
                         }).ToList();

            if (data2.Count > 0)
            {
                return Json(new { data2 }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }

        }


        public JsonResult BuscarDetalleSolicitudTrasladoId(int id) {


            var data2 = (from DetalleSolTra in context.Detalle_Solicitud_Traslado
                         join referencia in context.referencias_inven on DetalleSolTra.Cod_referencia.ToString() equals referencia.codigo
                          where DetalleSolTra.Id_Solicitud_Traslado == id && referencia.bodega==DetalleSolTra.Solicitud_traslado.Id_bodega_origen && referencia.ano==DateTime.Now.Year && referencia.mes==DateTime.Now.Month
                         select new
                         {
                             DetalleSolTra.Cod_referencia,
                             DetalleSolTra.Cantidad,
                             referencia.costo_prom,
                             Total = DetalleSolTra.Cantidad * referencia.costo_prom,
                             referencia.icb_referencia.ref_descripcion,
                             Promedio = referencia.costo_prom
                         }).ToList();
            decimal costototal = 0;
            costototal = data2.Sum(d => d.Total);
            return Json(new { data2,costototal=costototal.ToString("N0",new CultureInfo("is-IS")) }, JsonRequestBehavior.AllowGet);
        }


        public void ActualizarInventario() {
            string modulo = "V";

            try
            {
                var Referencias = (from referen in context.icb_referencia
                                   where referen.modulo != modulo
                                   select
                                   new
                                   {
                                       referen.ref_codigo,
                                       referen.costo_promedio
                                   }).ToList();

                foreach (var item in Referencias)
                {
                    
                    var AnoYMesReferencia = context.referencias_inven.Where(e => e.codigo == item.ref_codigo).OrderByDescending(m => new { m.ano, m.mes }).FirstOrDefault();
                    if (AnoYMesReferencia!= null)
                    {
                        AnoYMesReferencia.costo_prom = Convert.ToDecimal(item.costo_promedio, Cultura);
                        context.SaveChanges();

                    }

                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
                            
        
        }

        public ActionResult infotraslados() {




            return View();
        }



        public JsonResult buscarSolicitudes() {


            var data = (from sol in context.Solicitud_traslado
                        join bod in context.bodega_concesionario on sol.Id_bodega_origen equals bod.id
                        join bod2 in context.bodega_concesionario on sol.Id_bodega_destino equals bod2.id
                        join use in context.users on sol.Id_solicitante equals use.user_id
                        select new { 
                        sol.Id,
                        bod.bodccs_nombre,
                        destino = bod2.bodccs_nombre,
                        nombre = use.user_nombre+" "+use.user_apellido,
                        sol.Fecha_creacion,
                        referencia = (from det in context.Detalle_Solicitud_Traslado join
                                      refe in context.icb_referencia on det.Cod_referencia equals refe.ref_codigo
                                      where det.Id_Solicitud_Traslado == sol.Id
                                      select 
                                      refe.ref_descripcion
                                      )
                        }).ToList();


            var data2 = data.Select(x => new
            {
                x.Id,
                x.bodccs_nombre,
                x.destino,
                x.nombre,
                fecha = x.Fecha_creacion.ToString("yyyy/MM/dd", new CultureInfo("en-US")),
                x.referencia
            }).ToList();


            return Json(  data2 , JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarAgendados() {

            var data = (from rt in context.recibidotraslados
                        join bod in context.bodega_concesionario on rt.idorigen equals bod.id
                        join bod2 in context.bodega_concesionario on rt.iddestino equals bod2.id
                        join use in context.users on rt.usertraslado equals use.user_id
                        where rt.cant_recibida == null
                        select new
                        {
                            rt.id,
                            bod.bodccs_nombre,
                            destino = bod2.bodccs_nombre,
                            nombre = use.user_nombre + " " + use.user_apellido,
                            rt.fecharecibido,
                            referencia = (from refe in context.icb_referencia 
                                          where refe.ref_codigo == rt.refcodigo
                                          select
                                          refe.ref_descripcion
                                      )
                        }).ToList();


            var data2 = data.Select(x => new
            {
                x.id,
                x.bodccs_nombre,
                x.destino,
                x.nombre,
                fecha = x.fecharecibido != null ? x.fecharecibido.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")): "",
                x.referencia
            }).ToList();




            return Json( data2 , JsonRequestBehavior.AllowGet);
        }



        public JsonResult buscarRecepcionados()
        {

            var data = (from rt in context.recibidotraslados
                        join bod in context.bodega_concesionario on rt.idorigen equals bod.id
                        join bod2 in context.bodega_concesionario on rt.iddestino equals bod2.id
                        join use in context.users on rt.usertraslado equals use.user_id
                        where rt.cant_recibida != null
                        select new
                        {
                            rt.id,
                            bod.bodccs_nombre,
                            destino = bod2.bodccs_nombre,
                            nombre = use.user_nombre + " " + use.user_apellido,
                            rt.fecharecibido,
                            referencia = (from refe in context.icb_referencia
                                          where refe.ref_codigo == rt.refcodigo
                                          select
                                          refe.ref_descripcion
                                      )
                        }).ToList();


            var data2 = data.Select(x => new
            {
                x.id,
                x.bodccs_nombre,
                x.destino,
                x.nombre,
                fecha = x.fecharecibido != null ? x.fecharecibido.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                x.referencia
            }).ToList();




            return Json(data2, JsonRequestBehavior.AllowGet);
        }



        public JsonResult buscarRecibidos()
        {

            int bodegalog = Convert.ToInt32(Session["user_bodega"]);

            var data = (from rt in context.recibidotraslados
                        join bod in context.bodega_concesionario on rt.idorigen equals bod.id
                        join bod2 in context.bodega_concesionario on rt.iddestino equals bod2.id
                        join use in context.users on rt.usertraslado equals use.user_id
                        where rt.recibido==true && rt.recibo_completo == true && rt.tipo=="R" 
                        select new
                        {
                            rt.id,
                            rt.idorigen,
                            rt.iddestino,
                            rt.fechatraslado,
                            bod.bodccs_nombre,
                            destino = bod2.bodccs_nombre,
                            nombre = use.user_nombre + " " + use.user_apellido,
                            rt.fecharecibido,
                            referencia = (from refe in context.icb_referencia
                                          where refe.ref_codigo == rt.refcodigo
                                          select
                                          refe.ref_descripcion
                                      )
                        }).ToList();


            var data2 = data.Where(d=> d.idorigen== bodegalog || d.iddestino== bodegalog).Select(x => new
            {
                x.id,
                x.bodccs_nombre,
                x.destino,
                x.nombre,
                fecha = x.fechatraslado != null ? x.fechatraslado.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                x.referencia
            }).ToList();


            return Json(data2, JsonRequestBehavior.AllowGet);
        }


        public JsonResult buscarPendientePorRecibir()
        {

            int bodegalog = Convert.ToInt32(Session["user_bodega"]);

            var data = (from rt in context.recibidotraslados
                        join bod in context.bodega_concesionario on rt.idorigen equals bod.id
                        join bod2 in context.bodega_concesionario on rt.iddestino equals bod2.id
                        join use in context.users on rt.usertraslado equals use.user_id 
                        where rt.recibido == false && rt.tipo == "R" 
                        select new
                        {
                            rt.id,
                            rt.idorigen,
                            rt.iddestino,
                            rt.fechatraslado,
                            bod.bodccs_nombre,
                            destino = bod2.bodccs_nombre,
                            nombre = use.user_nombre + " " + use.user_apellido,
                            rt.fecharecibido,
                            referencia = (from refe in context.icb_referencia
                                          where refe.ref_codigo == rt.refcodigo
                                          select
                                          refe.ref_descripcion
                                      )
                        }).ToList();


            var data2 = data.Where(d => d.idorigen == bodegalog || d.iddestino == bodegalog).Select(x => new
            {
                x.id,
                x.bodccs_nombre,
                x.destino,
                x.nombre,
                fecha = x.fechatraslado != null ? x.fechatraslado.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                x.referencia
            }).ToList();


            return Json(data2, JsonRequestBehavior.AllowGet);
        }


        public JsonResult buscarIncompletos()
        {

            int bodegalog = Convert.ToInt32(Session["user_bodega"]);

            var data = (from rt in context.recibidotraslados
                        join bod in context.bodega_concesionario on rt.idorigen equals bod.id
                        join bod2 in context.bodega_concesionario on rt.iddestino equals bod2.id
                        join use in context.users on rt.usertraslado equals use.user_id
                        where rt.recibido==true && rt.cant_recibida != null && rt.recibo_completo == false && rt.tipo == "R" 
                        select new
                        {
                            rt.id,
                            rt.idorigen,
                            rt.iddestino,
                            rt.fechatraslado,
                            bod.bodccs_nombre,
                            destino = bod2.bodccs_nombre,
                            nombre = use.user_nombre + " " + use.user_apellido,
                            rt.fecharecibido,
                            referencia = (from refe in context.icb_referencia
                                          where refe.ref_codigo == rt.refcodigo
                                          select
                                          refe.ref_descripcion
                                      )
                        }).ToList();


            var data2 = data.Where(d => d.idorigen == bodegalog || d.iddestino == bodegalog).Select(x => new
            {
                x.id,
                x.bodccs_nombre,
                x.destino,
                x.nombre,
                fecha = x.fechatraslado != null ? x.fechatraslado.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                x.referencia
            }).ToList();


            return Json(data2, JsonRequestBehavior.AllowGet);
        }




    }
}