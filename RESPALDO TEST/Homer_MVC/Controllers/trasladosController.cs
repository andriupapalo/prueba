using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class trasladosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        private readonly CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

        public void ListasDesplegables(TrasladoModel traslado)
        {
            //ViewBag.tp_doc_registros = context.tp_doc_registros.Where(x => x.sw == 3 && x.tipo == 1007).ToList();
            int value = Convert.ToInt32(
                context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P38").syspar_value);
            var buscarTipoDocumentos = (from tipoDocumentos in context.tp_doc_registros
                                        where tipoDocumentos.tpdoc_id == value
                                        select new
                                        {
                                            tipoDocumentos.tpdoc_id,
                                            nombre = "(" + tipoDocumentos.prefijo + ") " + tipoDocumentos.tpdoc_nombre
                                        }).ToList();
            ViewBag.TipoDocumento = new SelectList(buscarTipoDocumentos, "tpdoc_id", "nombre");
            //leo las bodegas que comparten el tipo de documento TRASLADO ENTRE BODEGAS
            System.Collections.Generic.List<int> bodegas = context.grupoconsecutivos.Where(d => d.documento_id == value).Select(d => d.bodega_id)
                .ToList();
            ViewBag.BodegaDestino =
                new SelectList(
                    context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).Where(x =>
                        x.es_vehiculos && x.id != traslado.BodegaOrigen && bodegas.Contains(x.id)), "id",
                    "bodccs_nombre");
            ViewBag.MotivosTraslado = new SelectList(context.motivos_traslado.OrderBy(x => x.motivo).ToList(), "id", "motivo");
        }

        // GET: traslados
        public ActionResult Create(int? menu)
        {
            TrasladoModel traslado = new TrasladoModel();
            ListasDesplegables(traslado);
            BuscarFavoritos(menu);
            return View(traslado);
        }
        /// <summary>
        /// Este método sirve para TRASLADAR VEHICULOS
        /// </summary>
        /// <param name="modelo"></param>
        /// <param name="menu"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(TrasladoModel modelo, int? menu)
        {
            //modelo.Costo = Convert.ToDecimal(Request["costoValor"]);
                    string referencia = Request["codigo"];
                    string codigo2 = "";
                    string code = referencia;
                    if (code != "")
                    {
                        string[] codigo_arr = referencia.Split('|', ' ');
                        codigo2 = codigo_arr[0];
                    }
                    icb_vehiculo id_referencia = context.icb_vehiculo.Where(x => x.plan_mayor.Contains(codigo2) || x.plac_vh.Contains(codigo2) || x.vin.Contains(codigo2)).FirstOrDefault();
                    if (id_referencia != null)
                    {
                        modelo.Referencia = id_referencia.plan_mayor;
                    }
                    int lista = Convert.ToInt32(Request["listaTrasladoVH"]);

                    if (ModelState.IsValid && lista > 0)
                    {
                        for (int i = 0; i < lista; i++)
                        {
                            if (!string.IsNullOrWhiteSpace(Request["txtPlanMayor" + i]))
                    {

                        using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                        {
                            try
                            {
                                string pm = "";
                                int bo = 0;
                                int bd = 0;
                                string cl = "";
                                string mod = "";
                                decimal cos = 0;
                                int motivo = 0;
                                pm = Request["txtPlanMayor" + i];
                                bo = Convert.ToInt32(Request["txtOrigen" + i]);
                                bd = Convert.ToInt32(Request["txtDestino" + i]);
                                cl = Request["txtColor" + i];
                                mod = Request["txtModelo" + i];
                                cos = Convert.ToDecimal(Request["txtCosto" + i], new CultureInfo("is-Is"));
                                motivo = Convert.ToInt32(Request["txtMotivo" + i]);
                                var buscarPlanMayor = (from inventario in context.vw_inventario_hoy
                                                       join vwpromedio in context.vw_promedio
                                                           on inventario.ref_codigo equals vwpromedio.codigo into ps
                                                       from vwpromedio in ps.DefaultIfEmpty()
                                                       where inventario.modulo == "V"
                                                             && vwpromedio.ano == DateTime.Now.Year
                                                             && vwpromedio.mes == DateTime.Now.Month
                                                             && inventario.stock > 0
                                                             && inventario.ref_codigo == pm
                                                             && inventario.bodega == modelo.BodegaOrigen
                                                       select new
                                                       {
                                                           inventario.bodega,
                                                           inventario.stock
                                                       }).FirstOrDefault();
                                if (buscarPlanMayor == null)
                                {
                                    TempData["mensaje_error"] += "El vehiculo " + pm + " no se encuentra en el inventario";
                                    //BuscarFavoritos(menu);
                                    //return View(modelo);
                                }
                                else if (buscarPlanMayor.stock <= 0)
                                {
                                    TempData["mensaje_error"] +=
                                        "El vehiculo " + pm + " se encuentra en stock cero dentro del inventario";
                                }
                                else if (buscarPlanMayor.bodega != bo)
                                {
                                    TempData["mensaje_error"] += "El vehiculo " + pm + " no se encuentra en la bodega de origen seleccionada";
                                }
                                else
                                {
                                    int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                                    var buscarBodega = (from doc_consecutivos in context.icb_doc_consecutivos
                                                            join bodega in context.bodega_concesionario
                                                                on doc_consecutivos.doccons_bodega equals bodega.id
                                                            where doc_consecutivos.doccons_idtpdoc == modelo.TipoDocumento &&
                                                                  doc_consecutivos.doccons_bodega == bodegaActual && bodega.es_vehiculos
                                                            select new
                                                            {
                                                                bodega.id,
                                                                bodega.bodccs_nombre
                                                            }).FirstOrDefault();

                                    long numeroConsecutivo = 0;
                                    if (buscarBodega == null)
                                    {
                                        TempData["mensaje_error"] += "El consecutivo para traslados deLa bodega seleccionada de origen no está configurada";
                                    }
                                    else
                                    {
                                        ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
                                        icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
                                        tp_doc_registros buscarTipoDocRegistro =
                                            context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == modelo.TipoDocumento);
                                        numeroConsecutivoAux = gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro,
                                            modelo.TipoDocumento, modelo.BodegaOrigen);
                                        //var numeroConsecutivoAux = context.icb_doc_consecutivos.OrderByDescending(x => x.doccons_ano).FirstOrDefault(x => x.doccons_idtpdoc == modelo.TipoDocumento && x.doccons_bodega == modelo.BodegaOrigen);
                                        if (numeroConsecutivoAux == null)
                                        {
                                            TempData["mensaje_error"] =
                                               "No existe un numero consecutivo asignado para este tipo de documento";
                                            ListasDesplegables(modelo);
                                            ViewBag.documentoSeleccionado = modelo.TipoDocumento;
                                            ViewBag.bodegaSeleccionada = modelo.BodegaOrigen;
                                            ViewBag.usuarioSeleccionado = modelo.UsuarioRecepcion;
                                            ViewBag.perfilSeleccionado = modelo.PerfilContable;
                                            BuscarFavoritos(menu);
                                            //return View(modelo);
                                        }
                                        else
                                        {
                                            numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                                            // Si llega aqui significa que SI existe un consecutivo y la referencia si cuenta con stock
                                            vw_promedio buscarPromedio = context.vw_promedio.FirstOrDefault(x =>
                                                x.codigo == pm && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month);
                                            decimal promedio = buscarPromedio != null ? buscarPromedio.Promedio ?? 0 : 0;

                                            referencias_inven buscarReferenciasInvenOrigen = context.referencias_inven.FirstOrDefault(x =>
                                                x.codigo == pm && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month &&
                                                x.bodega == bo);
                                            if (buscarReferenciasInvenOrigen == null)
                                            {
                                                TempData["mensaje_error"] +=
                                            "El plan mayor con la bodega de origen no se encontro en el inventario de referencias para el mes actual";
                                            }
                                            else
                                            {
                                                buscarReferenciasInvenOrigen.can_sal = buscarReferenciasInvenOrigen.can_sal + 1;
                                                buscarReferenciasInvenOrigen.cos_sal = promedio;
                                                buscarReferenciasInvenOrigen.can_tra = buscarReferenciasInvenOrigen.can_tra + 1;
                                                buscarReferenciasInvenOrigen.cos_tra = promedio;
                                                context.Entry(buscarReferenciasInvenOrigen).State = EntityState.Modified;

                                                // La bodega de origen debe existir
                                                referencias_inven buscarReferenciasInvenDestino = context.referencias_inven.FirstOrDefault(x =>
                                                    x.codigo == pm && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month &&
                                                    x.bodega == bd);
                                                if (buscarReferenciasInvenDestino != null)
                                                {
                                                    buscarReferenciasInvenDestino.can_ent = buscarReferenciasInvenDestino.can_ent + 1;
                                                    buscarReferenciasInvenDestino.cos_ent = promedio;
                                                    buscarReferenciasInvenDestino.can_tra = buscarReferenciasInvenDestino.can_tra + 1;
                                                    buscarReferenciasInvenDestino.cos_tra = promedio;
                                                    context.Entry(buscarReferenciasInvenDestino).State = EntityState.Modified;
                                                }
                                                else
                                                {
                                                    referencias_inven crearReferencia = new referencias_inven
                                                    {
                                                        bodega = bd,
                                                        codigo = pm,
                                                        ano = (short)DateTime.Now.Year,
                                                        mes = (short)DateTime.Now.Month,
                                                        can_ini = 0,
                                                        can_ent = 1,
                                                        can_sal = 0,
                                                        cos_ent = promedio,
                                                        can_tra = 1,
                                                        cos_tra = promedio,
                                                        modulo = "V",
                                                        costo_prom = promedio,
                                                    };
                                                    context.referencias_inven.Add(crearReferencia);
                                                }
                                                icb_sysparameter buscarNit = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P33");
                                                int nitTraslado = buscarNit != null ? Convert.ToInt32(buscarNit.syspar_value) : 0;

                                                encab_documento crearEncabezado = new encab_documento
                                                {
                                                    nit = nitTraslado,
                                                    tipo = modelo.TipoDocumento,
                                                    bodega = bo,
                                                    numero = numeroConsecutivo,
                                                    documento = pm,
                                                    fecha = DateTime.Now,
                                                    fec_creacion = DateTime.Now,
                                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                                    impoconsumo = 0,
                                                    perfilcontable = modelo.PerfilContable,
                                                    bodega_destino = bd,
                                                    destinatario = modelo.UsuarioRecepcion,
                                                    mot_traslado = motivo
                                                };
                                                context.encab_documento.Add(crearEncabezado);

                                                bool guardar = context.SaveChanges() > 0;
                                                encab_documento buscarUltimoEncabezado = context.encab_documento.Where(d => d.idencabezado == crearEncabezado.idencabezado).FirstOrDefault();

                                                lineas_documento crearLineasOrigen = new lineas_documento
                                                {
                                                    codigo = pm,
                                                    fec_creacion = DateTime.Now,
                                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                                    nit = nitTraslado,
                                                    cantidad = 1,
                                                    bodega = bo,
                                                    seq = 1,
                                                    estado = true,
                                                    fec = DateTime.Now,
                                                    costo_unitario = Convert.ToDecimal(cos),
                                                    id_encabezado = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0
                                                };
                                                context.lineas_documento.Add(crearLineasOrigen);


                                                lineas_documento crearLineasDestino = new lineas_documento
                                                {
                                                    codigo = pm,
                                                    fec_creacion = DateTime.Now,
                                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                                    nit = nitTraslado,
                                                    cantidad = 1,
                                                    bodega = bd,
                                                    seq = 1,
                                                    estado = true,
                                                    fec = DateTime.Now,
                                                    costo_unitario = cos,
                                                    id_encabezado = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0
                                                };

                                                context.lineas_documento.Add(crearLineasDestino);

                                                recibidotraslados recibido = new recibidotraslados
                                                {
                                                    idtraslado = crearEncabezado.idencabezado,
                                                    refcodigo = pm,
                                                    cantidad = 1,
                                                    recibido = false,
                                                    fechatraslado = DateTime.Now,
                                                    usertraslado = Convert.ToInt32(Session["user_usuarioid"]),
                                                    costo = cos,
                                                    idorigen = bo,
                                                    iddestino = bd,
                                                    tipo = "V"
                                                };
                                                context.recibidotraslados.Add(recibido);

                                                System.Collections.Generic.List<perfil_cuentas_documento> parametrosCuentas = context.perfil_cuentas_documento
                                                    .Where(x => x.id_perfil == modelo.PerfilContable).ToList();
                                                int secuencia = 1;
                                                /*centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                                                int idCentroCero = centroValorCero != null
                                                    ? Convert.ToInt32(centroValorCero.centcst_id)
                                                    : 0;
                                                icb_terceros terceroValorCero = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
                                                int idTerceroCero = centroValorCero != null
                                                    ? Convert.ToInt32(terceroValorCero.tercero_id)
                                                    : 0;*/

                                                foreach (perfil_cuentas_documento parametro in parametrosCuentas)
                                                {
                                                    cuenta_puc buscarCuenta =
                                                        context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
                                                    if (buscarCuenta != null)
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
                                                            nit = nitTraslado,
                                                            fec = DateTime.Now,
                                                            seq = secuencia,
                                                            debito = cos
                                                        };
                                                        //movNuevo.credito = modelo.Costo;                
                                                        if (buscarCuenta.tercero)
                                                        {
                                                            movNuevo.nit = nitTraslado;
                                                        }

                                                        movNuevo.detalle = "Traslado de vehiculo " + pm;
                                                        secuencia++;
                                                        AgregarRegistroCuentasValores(movNuevo, parametro.centro, parametro.cuenta);
                                                        context.mov_contable.Add(movNuevo);

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
                                                            nit = nitTraslado,
                                                            fec = DateTime.Now,
                                                            seq = secuencia,
                                                            //movNuevo2.debito = modelo.Costo;
                                                            credito = cos
                                                        };
                                                        if (buscarCuenta.tercero)
                                                        {
                                                            movNuevo2.nit = nitTraslado;
                                                        }

                                                        movNuevo2.detalle = "Traslado de vehiculo " + pm;
                                                        secuencia++;
                                                        AgregarRegistroCuentasValores(movNuevo2, parametro.centro, parametro.cuenta);
                                                        context.mov_contable.Add(movNuevo2);
                                                    }
                                                }

                                                int guardarLineasYMovimientos = context.SaveChanges();
                                                if (guardarLineasYMovimientos > 0)
                                                {
                                                    // Se actualiza la bodega actual del vehiculo
                                                    icb_vehiculo buscarVehiculo = context.icb_vehiculo.FirstOrDefault(x => x.plan_mayor == pm);
                                                    if (buscarVehiculo != null)
                                                    {
                                                        buscarVehiculo.id_bod = bd;
                                                        context.Entry(buscarVehiculo).State = EntityState.Modified;
                                                        context.SaveChanges();
                                                    }

                                                    grupoconsecutivos buscarGrupoConsecutivos = context.grupoconsecutivos.FirstOrDefault(x =>
                                                        x.documento_id == modelo.TipoDocumento && x.bodega_id == bo);
                                                    int numeroGrupo = buscarGrupoConsecutivos != null ? buscarGrupoConsecutivos.grupo : 0;
                                                    if (buscarGrupoConsecutivos != null)
                                                    {
                                                        IQueryable<icb_doc_consecutivos> numerosConsecutivos =
                                                            context.icb_doc_consecutivos.Where(x =>
                                                                x.doccons_grupoconsecutivo == numeroGrupo);
                                                        foreach (icb_doc_consecutivos item in numerosConsecutivos)
                                                        {
                                                            item.doccons_siguiente = item.doccons_siguiente + 1;
                                                            context.Entry(item).State = EntityState.Modified;
                                                        }

                                                        context.SaveChanges();
                                                    }

                                                    TempData["mensaje"] += "El traslado se ha realizado exitosamente";
                                                    ListasDesplegables(modelo);                                                   
                                                    dbTran.Commit();
                                                    //return RedirectToAction("Create", new { menu });
                                                }
                                                else
                                                {
                                                    TempData["mensaje_error"] += "Error en movimientos contables";
                                                    ListasDesplegables(modelo);
                                                    dbTran.Rollback();
                                                }
                                                ////
                                            }
                                        }
                                    }                                                                       
                                }
                                
                            }
                            catch (Exception ex)
                            {
                                var error = ex.InnerException.Message;
                                dbTran.Rollback();
                            }
                        }                                          
                    }                                                       
                        }
                    }
                    else if (ModelState.IsValid && lista == 0)
                    {
                        TempData["mensaje_error"] = "Debe seleccionar y agregar al menos un vehículo para poder efectuar el traslado";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Debe ingresar información en los campos obligatorios y seleccionar al menos un vehículo";
                    }                           

            ListasDesplegables(modelo);
            ViewBag.documentoSeleccionado = modelo.TipoDocumento;
            ViewBag.bodegaSeleccionada = modelo.BodegaOrigen;
            ViewBag.usuarioSeleccionado = modelo.UsuarioRecepcion;
            ViewBag.perfilSeleccionado = modelo.PerfilContable;
            BuscarFavoritos(menu);
            return RedirectToAction("Create", new { menu });
        }

        public void AgregarRegistroCuentasValores(mov_contable movNuevo, int? centro, int cuenta)
        {
            DateTime fechaHoy = DateTime.Now;
            cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                x.centro == centro && x.cuenta == cuenta && x.nit == movNuevo.nit && x.ano == fechaHoy.Year &&
                x.mes == fechaHoy.Month);
            if (buscar_cuentas_valores != null)
            {
                buscar_cuentas_valores.ano = fechaHoy.Year;
                buscar_cuentas_valores.mes = fechaHoy.Month;
                buscar_cuentas_valores.cuenta = movNuevo.cuenta;
                buscar_cuentas_valores.centro = movNuevo.centro;
                buscar_cuentas_valores.nit = movNuevo.nit;
                buscar_cuentas_valores.debito += movNuevo.debito;
                buscar_cuentas_valores.credito += movNuevo.credito;
                buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                context.SaveChanges();
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
                    credito = movNuevo.credito,
                    debitoniff = movNuevo.debitoniif,
                    creditoniff = movNuevo.creditoniif
                };
                context.cuentas_valores.Add(crearCuentaValor);
                context.SaveChanges();
            }
        }

        public JsonResult BuscarTrasladosVehiculos()
        {
            icb_sysparameter tipoDocTrasladoRepuestos = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P38");
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
                                       motivo = encabezado.mot_traslado != null ? encabezado.motivos_traslado.motivo : "",
                                       encabezado.idencabezado,
                                       encabezado.numero,
                                       encabezado.fecha,
                                       origen = origen.bodccs_nombre,
                                       destino = destino.bodccs_nombre,
                                       referencias = (from lineas in context.lineas_documento
                                                      join referencia in context.icb_referencia
                                                          on lineas.codigo equals referencia.ref_codigo
                                                      where lineas.id_encabezado == encabezado.idencabezado
                                                      select new
                                                      {
                                                          lineas.seq,
                                                          lineas.cantidad,
                                                          referencia.ref_codigo,
                                                          referencia.ref_descripcion
                                                      }).Distinct().ToList()
                                   }).ToList();

            var data = buscarTraslados.Select(x => new
            {
                x.idencabezado,
                x.numero,
                fecha = x.fecha.ToShortDateString(),
                x.origen,
                destino = x.destino != null ? x.destino : "",
                x.referencias,
                x.motivo
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPerfilesContables(int? idTpDoc)
        {
            var buscarPerfiles = context.perfil_contable_documento.Where(x => x.tipo == idTpDoc).Select(x => new
            {
                x.id,
                x.descripcion
            }).ToList();
            return Json(buscarPerfiles, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarBodegasPorDocumento(int? idTpDoc)
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            var buscarBodegasDelDocumento = (from doc_consecutivos in context.icb_doc_consecutivos
                                             join bodega in context.bodega_concesionario
                                                 on doc_consecutivos.doccons_bodega equals bodega.id
                                             where doc_consecutivos.doccons_idtpdoc == idTpDoc && doc_consecutivos.doccons_bodega == bodegaActual &&
                                                   bodega.es_vehiculos
                                             select new
                                             {
                                                 bodega.id,
                                                 bodega.bodccs_nombre
                                             }).ToList();

            return Json(buscarBodegasDelDocumento, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPerfilPorBodega(int bodega, int idTpDoc)
        {
            var data = (from b in context.perfil_contable_bodega
                        join t in context.perfil_contable_documento
                            on b.idperfil equals t.id
                        where b.idbodega == bodega && t.tipo == idTpDoc
                        select new
                        {
                            id = b.idperfil,
                            descripcion = t.descripcion
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarBodegasPorDocumentoDestino(int? idTpDoc, int? idBodega)
        {
            System.Collections.Generic.List<int> bodegas = context.grupoconsecutivos.Where(d => d.documento_id == idTpDoc).Select(d => d.bodega_id)
                .ToList();
            System.Collections.Generic.List<bodega_concesionario> bodegas2 = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre)
                .Where(x => x.es_vehiculos && x.id != idBodega && bodegas.Contains(x.id)).ToList();
            var bodegas3 = bodegas2.Select(d => new { d.id, nombre = d.bodccs_nombre }).ToList();

            return Json(bodegas3, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarUsuariosPorBodega(int? idBodega)
        {
            if (idBodega != null)
            {
                var buscarUsuarios = (from bodegaUsuario in context.bodega_usuario
                                      join usuario in context.users
                                          on bodegaUsuario.id_usuario equals usuario.user_id
                                      where bodegaUsuario.id_bodega == idBodega && usuario.aut_repuestos == true
                                      select new
                                      {
                                          usuario.user_id,
                                          usuario.user_nombre,
                                          usuario.user_apellido
                                      }).ToList();

                return Json(buscarUsuarios, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult BuscarReferencia(string referencia, int? bodega_origen)
        {
            string id_referencia = context.icb_vehiculo.Where(x => x.plan_mayor.Contains(referencia) || x.plac_vh.Contains(referencia) || x.vin.Contains(referencia)).FirstOrDefault().plan_mayor;

            if (!string.IsNullOrWhiteSpace(id_referencia) && bodega_origen != null)
            {
                var buscarReferencia = (from inventario in context.vw_inventario_hoy
                                        join promedio in context.vw_promedio
                                            on inventario.ref_codigo equals promedio.codigo into ps
                                        from promedio in ps.DefaultIfEmpty()
                                        join vehiculo in context.icb_vehiculo
                                            on inventario.ref_codigo equals vehiculo.plan_mayor into veh
                                        from vehiculo in veh.DefaultIfEmpty()
                                        join modelo in context.modelo_vehiculo
                                            on vehiculo.modvh_id equals modelo.modvh_codigo into mod
                                        from modelo in mod.DefaultIfEmpty()
                                        join color in context.color_vehiculo
                                            on vehiculo.colvh_id equals color.colvh_id into col
                                        from color in col.DefaultIfEmpty()
                                        where inventario.modulo == "V"
                                              && promedio.ano == DateTime.Now.Year
                                              && promedio.mes == DateTime.Now.Month
                                              && inventario.stock > 0
                                              && inventario.ref_codigo == id_referencia
                                              && inventario.bodega == bodega_origen
                                        select new
                                        {
                                            costo = promedio != null ? promedio.Promedio != null ? promedio.Promedio : 0 : 0,
                                            modelo = modelo.modvh_nombre != null ? modelo.modvh_nombre : "",
                                            color = color.colvh_nombre != null ? color.colvh_nombre : "",
                                            planmayor = inventario.ref_codigo
                                        }).FirstOrDefault();

                if (buscarReferencia != null)
                {
                    return Json(new { buscarReferencia, encontrado = true }, JsonRequestBehavior.AllowGet);
                }

                return Json(
                    new
                    {
                        encontrado = false,
                        alerta =
                            "La referencia buscada no se encontro en el inventario del mes actual para la bodega de origen seleccionada"
                    }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { encontrado = false, alerta = "Debe indicar una referencia y una bodega de origen" },
                JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarReferenciasVh(string serie, int? bodega_origen)
        {
            var planmayor = (from r in context.icb_vehiculo
                             join inv in context.vw_inventario_hoy
                             on r.plan_mayor equals inv.ref_codigo
                             where r.plan_mayor.Contains(serie) && inv.bodega == bodega_origen
                             && inv.mes == DateTime.Now.Month && inv.ano == DateTime.Now.Year && inv.stock > 0 && inv.modulo == "V"
                             select new
                             {
                                 series = r.plan_mayor + " | " + r.modelo_vehiculo.modvh_nombre
                             }).ToList();
            System.Collections.Generic.List<string> planmayor_data = planmayor.Select(d => d.series).ToList();

            if (planmayor.Count == 0)
            {
                var placa = (from r in context.icb_vehiculo
                             join inv in context.vw_inventario_hoy
                             on r.plan_mayor equals inv.ref_codigo
                             where r.plac_vh.Contains(serie) && inv.bodega == bodega_origen
                             && inv.mes == DateTime.Now.Month && inv.ano == DateTime.Now.Year && inv.stock > 0 && inv.modulo == "V"
                             select new
                             {
                                 series = r.plac_vh + " | " + r.modelo_vehiculo.modvh_nombre
                             }).ToList();
                System.Collections.Generic.List<string> placa_data = placa.Select(d => d.series).ToList();

                if (placa.Count == 0)
                {
                    var series = (from r in context.icb_vehiculo
                                  join inv in context.vw_inventario_hoy
                                  on r.plan_mayor equals inv.ref_codigo
                                  where r.vin.Contains(serie) && inv.bodega == bodega_origen
                                  && inv.mes == DateTime.Now.Month && inv.ano == DateTime.Now.Year && inv.stock > 0 && inv.modulo == "V"
                                  select new
                                  {
                                      series = r.vin + " | " + r.modelo_vehiculo.modvh_nombre
                                  }).ToList();
                    System.Collections.Generic.List<string> series_data = series.Select(d => d.series).ToList();

                    return Json(series_data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(placa_data, JsonRequestBehavior.AllowGet);
                }

            }
            return Json(planmayor_data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarReferenciaSerie(string serie, int? bodega_origen)
        {
            var series = (from r in context.icb_vehiculo
                          join inv in context.vw_inventario_hoy
                          on r.plan_mayor equals inv.ref_codigo
                          where r.vin.Contains(serie) && inv.bodega == bodega_origen
                          select new
                          {
                              series = r.vin + " | " + r.modelo_vehiculo.modvh_nombre
                          }).ToList();
            System.Collections.Generic.List<string> series_data = series.Select(d => d.series).ToList();

            return Json(series_data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarReferenciasPlaca(string serie, int? bodega_origen)
        {
            var series = (from r in context.icb_vehiculo
                          join inv in context.vw_inventario_hoy
                          on r.plan_mayor equals inv.ref_codigo
                          where r.plac_vh.Contains(serie) && inv.bodega == bodega_origen
                          select new
                          {
                              series = r.plac_vh + " | " + r.modelo_vehiculo.modvh_nombre
                          }).ToList();
            System.Collections.Generic.List<string> series_data = series.Select(d => d.series).ToList();

            return Json(series, JsonRequestBehavior.AllowGet);
        }

        public ActionResult destinoTraslado(int? menu)
        {
            return View();
        }

        public JsonResult cargarTrasladosDestino()
        {
            string parametro = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P68").syspar_value;

            int syspar = Convert.ToInt32(parametro);

            int tipoCheck = context.tipo_Checklist.FirstOrDefault(x => x.id == syspar).id;

            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);

            var buscar = (from a in context.recibidotraslados
                          join b in context.encab_documento
                              on a.idtraslado equals b.idencabezado
                          join c in context.icb_referencia
                              on a.refcodigo equals c.ref_codigo
                          join d in context.bodega_concesionario
                              on a.idorigen equals d.id
                          join dd in context.bodega_concesionario
                              on a.iddestino equals dd.id
                          join e in context.users
                              on a.usertraslado equals e.user_id
                          where a.iddestino == bodegaActual && a.recibido == false && a.tipo == "V"
                          select new
                          {
                              a.id,
                              b.numero,
                              b.idencabezado,
                              a.refcodigo,
                              c.ref_descripcion,
                              a.cantidad,
                              a.recibido,
                              a.fechatraslado,
                              a.costo,
                              origen = d.bodccs_nombre,
                              destino = dd.bodccs_nombre,
                              motivo = b.mot_traslado != null ? b.motivos_traslado.motivo : ""
                          }).ToList();

            var buscar2 = buscar.Select(x => new
            {
                x.id,
                x.numero,
                x.refcodigo,
                cod_referencia = x.refcodigo,
                referencia = x.refcodigo + " - " + x.ref_descripcion,
                x.cantidad,
                x.recibido,
                fechatraslado = x.fechatraslado.ToString("yyyy/MM/dd HH:mm"),
                costo = x.costo.ToString("0,0", elGR),
                x.destino,
                x.origen,
                x.idencabezado,
                x.motivo
            });

            var data = new
            {
                buscar2,
                tipoCheck
            };


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscartraslado(string numero)
        {
            long numeroTraslado = Convert.ToInt64(numero);

            int bodegaLog = Convert.ToInt32(Session["user_bodega"]);

            var buscar = context.vw_seguimiento_traslados
                .Where(x => x.numero == numeroTraslado || x.refcodigo == numero).Select(x => new
                {
                    x.numero,
                    x.refcodigo,
                    x.ref_descripcion,
                    x.cantidad,
                    x.fecharecibido,
                    x.fechatraslado,
                    x.costo,
                    x.origen,
                    x.destino,
                    x.user_nombre,
                    x.user_apellido,
                    x.notas,
                    x.recibido,
                    x.id,
                    x.idEncabezado
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
                    x.idEncabezado
                });

                int permiso = context.vw_seguimiento_traslados.Where(x =>
                    x.id_origen == bodegaLog && (x.numero == numeroTraslado || x.refcodigo == numero)).Count();

                if (permiso > 0)
                {
                    return Json(new { data, info = true, error = false }, JsonRequestBehavior.AllowGet);
                }

                if (permiso == 0)
                {
                    return Json(new { info = false, error = true }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { info = false, error = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult trasladoPendiente()
        {
            int bodegalog = Convert.ToInt32(Session["user_bodega"]);

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
                          where a.idorigen == bodegalog && a.recibido == false && a.tipo == "V"
                          select new
                          {
                              e.numero,
                              b.ref_codigo,
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
                              a.recibido
                          }).ToList();

            if (buscar.Count != 0)
            {
                var data = buscar.Select(x => new
                {
                    x.numero,
                    referencia = x.ref_codigo + " - " + x.ref_descripcion,
                    x.cantidad,
                    fecharecibido = x.fecharecibido != null ? x.fecharecibido.Value.ToString("yyyy/MM/dd HH:mm") : "",
                    fechatraslado = x.fechatraslado.ToString("yyyy/MM/dd HH:mm"),
                    costo = x.costo.ToString("0,0", elGR),
                    x.origen,
                    x.destino,
                    asesor = x.user_nombre != null ? x.user_nombre + " " + x.user_apellido : "",
                    notas = x.notas != null ? x.notas : "",
                    x.recibido
                });

                return Json(new { data, info = true }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { info = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult recibirVehiculo(int id)
        {
            return Json(JsonRequestBehavior.AllowGet);
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