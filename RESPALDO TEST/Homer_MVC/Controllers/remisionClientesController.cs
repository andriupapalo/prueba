using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Globalization;
using System.IO;
using Rotativa;
using Rotativa.Options;


namespace Homer_MVC.Controllers
{
    public class remisionClientesController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        private CultureInfo culturaInfo = new CultureInfo("is-IS");

        // GET: remisionClientes
        public ActionResult Create(int? menu)
        {
            ListasDesplegables();
            BuscarFavoritos(menu);
            return View();
        }


        // POST: remisionClientes
        [HttpPost]
        public ActionResult Create(RemisionClienteModel modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //int bodegaActual = Convert.ToInt32(Session["user_bodega"]);

                int bodega = Convert.ToInt32(Request["bodega"]);

                long numeroConsecutivo = 0;
                grupoconsecutivos buscarGrupoConsecutivos = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == modelo.TipoDocumento && x.bodega_id == bodega);
                int numeroGrupo = buscarGrupoConsecutivos != null ? buscarGrupoConsecutivos.grupo : 0;

                ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
                icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
                tp_doc_registros buscarTipoDocRegistro =
                    context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == modelo.TipoDocumento);
                numeroConsecutivoAux =
                    gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro, modelo.TipoDocumento, bodega);

                if (numeroConsecutivoAux != null)
                {
                    numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                }
                else
                {
                    TempData["mensaje_error"] = "No existe un numero consecutivo asignado para este tipo de documento";
                    ListasDesplegables();
                    ViewBag.documentoSeleccionado = modelo.TipoDocumento;
                    ViewBag.bodegaSeleccionada = bodega;
                    //ViewBag.usuarioSeleccionado = modelo.UsuarioRecepcion;
                    ViewBag.perfilSeleccionado = modelo.TipoDocumento;
                    BuscarFavoritos(menu);
                    return View(modelo);
                }
                // Si llega aqui significa que si hay un numero de documento consecutivo 

                encab_documento crearEncabezado = new encab_documento
                {
                    tipo = modelo.TipoDocumento,
                    bodega = bodega,
                    numero = numeroConsecutivo,
                    documento = modelo.Referencia,
                    fecha = DateTime.Now,
                    fec_creacion = DateTime.Now,
                    impoconsumo = 0,
                    vendedor = modelo.asesor,
                    notas = modelo.Notas,
                    nit = modelo.cliente,
                    //crearEncabezado.destinatario = modelo.UsuarioRecepcion;
                    bodega_destino = modelo.bodega,
                    perfilcontable = modelo.TipoDocumento,
                };
                context.encab_documento.Add(crearEncabezado);

                bool guardar = context.SaveChanges() > 0;
                encab_documento buscarUltimoEncabezado =
                    context.encab_documento.OrderByDescending(x => x.idencabezado).Where(x => x.tipo == 3037).FirstOrDefault();

                icb_sysparameter buscarNit = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P33");
                int nitTraslado = buscarNit != null ? Convert.ToInt32(buscarNit.syspar_value) : 0;

                List<string> listaReferencias = new List<string>();
                int numero_repuestos = Convert.ToInt32(Request["lista_repuestos"]);
                int sequ = 1;
                for (int i = 1; i <= numero_repuestos; i++)
                {
                    string referencia_codigo = Request["cod_referencia" + i];
                    string referencia_cantidad = Request["cantidad_referencia" + i];
                    string referencia_precio = Request["precio_referencia" + i];
                    string referencia_iva = Request["iva_referencia" + i];
                    string referencia_descuento = Request["descuento_referencia" + i];
                    string estado = Request["estado_referencia" + i];

                    int idestado = context.EstadosRemision.Where(d => d.estado.Contains(estado)).Select(d => d.id).FirstOrDefault();

                    if (string.IsNullOrEmpty(referencia_codigo) || string.IsNullOrEmpty(referencia_cantidad))
                    {
                        // Significa que la agregaron y la eliminaron
                    }
                    else
                    {
                        listaReferencias.Add(referencia_codigo);
                        vw_promedio buscarPromedio = context.vw_promedio.FirstOrDefault(x =>
                            x.codigo == modelo.Referencia && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month);
                        decimal promedio = buscarPromedio != null ? buscarPromedio.Promedio ?? 0 : 0;

                        //referencias_inven buscarReferenciasInvenOrigen = context.referencias_inven.FirstOrDefault(x =>
                        //    x.codigo == modelo.Referencia && x.ano == DateTime.Now.Year &&
                        //    x.mes == DateTime.Now.Month && x.bodega == bodega);

                        //if (buscarReferenciasInvenOrigen != null)
                        //{
                        //    buscarReferenciasInvenOrigen.can_sal =
                        //        buscarReferenciasInvenOrigen.can_sal + Convert.ToInt32(referencia_cantidad);
                        //    buscarReferenciasInvenOrigen.cos_sal = promedio;
                        //    buscarReferenciasInvenOrigen.can_tra =
                        //        buscarReferenciasInvenOrigen.can_tra + Convert.ToInt32(referencia_cantidad);
                        //    buscarReferenciasInvenOrigen.cos_tra = promedio;
                        //    context.Entry(buscarReferenciasInvenOrigen).State = EntityState.Modified;

                        //    // La bodega de origen debe existir
                        //    referencias_inven buscarReferenciasInvenDestino = context.referencias_inven.FirstOrDefault(x =>
                        //        x.codigo == modelo.Referencia && x.ano == DateTime.Now.Year &&
                        //        x.mes == DateTime.Now.Month && x.bodega == modelo.BodegaDestino);
                        //    if (buscarReferenciasInvenDestino != null)
                        //    {
                        //        buscarReferenciasInvenDestino.can_ent =
                        //            buscarReferenciasInvenDestino.can_ent + Convert.ToInt32(referencia_cantidad);
                        //        buscarReferenciasInvenDestino.cos_ent = promedio;
                        //        buscarReferenciasInvenDestino.can_tra =
                        //            buscarReferenciasInvenDestino.can_tra + Convert.ToInt32(referencia_cantidad);
                        //        buscarReferenciasInvenDestino.cos_tra = promedio;
                        //        context.Entry(buscarReferenciasInvenDestino).State = EntityState.Modified;
                        //    }
                        //    else
                        //    {
                        //        referencias_inven crearReferencia = new referencias_inven
                        //        {
                        //            bodega = modelo.BodegaDestino,
                        //            codigo = modelo.Referencia,
                        //            ano = (short)DateTime.Now.Year,
                        //            mes = (short)DateTime.Now.Month,
                        //            can_ini = 0,
                        //            can_ent = Convert.ToInt32(referencia_cantidad),
                        //            can_sal = 0,
                        //            cos_ent = promedio,
                        //            can_tra = Convert.ToInt32(referencia_cantidad),
                        //            cos_tra = promedio,
                        //            modulo = "R"
                        //        };
                        //        context.referencias_inven.Add(crearReferencia);
                        //    }
                        //}


                        lineas_documento crearLineas = new lineas_documento
                        {
                            codigo = referencia_codigo,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            nit = nitTraslado,
                            idestadoremision = idestado,
                            cantidad = !string.IsNullOrEmpty(referencia_cantidad)
                                ? Convert.ToDecimal(referencia_cantidad, culturaInfo)
                                : 0,
                            bodega = bodega,
                            seq = sequ,
                            estado = true,
                            fec = DateTime.Now,
                            costo_unitario = !string.IsNullOrEmpty(referencia_precio)
                                ? Convert.ToDecimal(referencia_precio, culturaInfo)
                                : 0,
                            id_encabezado = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0,
                            porcentaje_descuento = !string.IsNullOrEmpty(referencia_descuento)
                                ? float.Parse(referencia_descuento)
                                : 0,
                            porcentaje_iva = !string.IsNullOrEmpty(referencia_iva) ? float.Parse(referencia_iva) : 0
                        };
                        context.lineas_documento.Add(crearLineas);

                        sequ++;
                    }
                }


                decimal? promedioReferencias = (from referencia in context.icb_referencia
                                                join promedioAux in context.vw_promedio
                                                    on referencia.ref_codigo equals promedioAux.codigo into pro
                                                from promedioAux in pro.DefaultIfEmpty()
                                                where listaReferencias.Contains(promedioAux.codigo)
                                                select promedioAux.Promedio).Sum();


                List<perfil_cuentas_documento> parametrosCuentas = context.perfil_cuentas_documento
                    .Where(x => x.id_perfil == modelo.TipoDocumento).ToList();
                int secuencia = 1;
                centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
                icb_terceros terceroValorCero = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
                int idTerceroCero = centroValorCero != null ? Convert.ToInt32(terceroValorCero.tercero_id) : 0;

                foreach (perfil_cuentas_documento parametro in parametrosCuentas)
                {
                    cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
                    if (buscarCuenta != null)
                    {
                        mov_contable movNuevo = new mov_contable
                        {
                            id_encab = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            idparametronombre = parametro.id_nombre_parametro,
                            cuenta = parametro.cuenta,
                            centro = parametro.centro,
                            nit = 0,
                            fec = DateTime.Now,
                            seq = secuencia,
                            debito = promedioReferencias ?? 0
                        };
                        //movNuevo.credito = modelo.Costo;                
                        if (buscarCuenta.tercero)
                        {
                            movNuevo.nit = nitTraslado;
                        }

                        movNuevo.detalle = "Remision de repuesto " + modelo.Referencia;
                        secuencia++;

                        AgregarRegistroCuentasValores(movNuevo, parametro.centro, parametro.cuenta, idCentroCero,
                            idTerceroCero);

                        context.mov_contable.Add(movNuevo);

                        mov_contable movNuevo2 = new mov_contable
                        {
                            id_encab = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            idparametronombre = parametro.id_nombre_parametro,
                            cuenta = parametro.cuenta,
                            centro = parametro.centro,
                            nit = 0,
                            fec = DateTime.Now,
                            seq = secuencia,
                            //movNuevo2.debito = modelo.Costo;
                            credito = promedioReferencias ?? 0
                        };
                        if (buscarCuenta.tercero)
                        {
                            movNuevo2.nit = nitTraslado;
                        }

                        movNuevo2.detalle = "Remision de repuesto " + modelo.Referencia;
                        secuencia++;

                        AgregarRegistroCuentasValores(movNuevo2, parametro.centro, parametro.cuenta, idCentroCero,
                            idTerceroCero);

                        context.mov_contable.Add(movNuevo2);
                    }
                }

                int guardarLineasYMovimientos = context.SaveChanges();

                if (guardarLineasYMovimientos > 0)
                {
                    if (buscarGrupoConsecutivos != null)
                    {
                        List<icb_doc_consecutivos> numerosConsecutivos = context.icb_doc_consecutivos
                            .Where(x => x.doccons_grupoconsecutivo == numeroGrupo).ToList();
                        foreach (icb_doc_consecutivos item in numerosConsecutivos)
                        {
                            item.doccons_siguiente = item.doccons_siguiente + 1;
                            context.Entry(item).State = EntityState.Modified;
                        }

                        context.SaveChanges();
                    }

                    TempData["mensaje"] = "La Remisión de Clientes se ha realizado correctamente";
                }
            }

            ListasDesplegables();
            BuscarFavoritos(menu);
            return View();
        }


        public ActionResult Edit(int id) {

            var remisioncliente = context.encab_documento.Where(x=>x.idencabezado== id).FirstOrDefault();

            RemisionClienteModel remimodelo = new RemisionClienteModel();
            remimodelo.encabezado_id = remisioncliente.idencabezado;
         
            remimodelo.bodega = remisioncliente.bodega;
            remimodelo.cliente = remisioncliente.nit;
            remimodelo.Notas = remisioncliente.notas;
            remimodelo.asesor = Convert.ToInt32(remisioncliente.vendedor);
            remimodelo.TipoDocumento = remisioncliente.tipo;

            var tercerosdir = context.terceros_direcciones.Where(x => x.idtercero == remisioncliente.nit).Select(x=>new { x.id, x.direccion}).ToList();
            icb_terceros tercero = context.icb_terceros.Where(x => x.tercero_id == remisioncliente.nit).FirstOrDefault();



            ViewBag.fecha = remisioncliente.fec_creacion;
            if (tercerosdir!=null)
                {
                ViewBag.cbDireccion = new SelectList(tercerosdir, "id", "direccion");
                }
            if (tercero!=null)
                {
                ViewBag.cbTelefono = tercero.celular_tercero;
                ViewBag.txtCorreo = tercero.email_tercero;
                }
          

            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);


            var buscarClientes = (from cli in context.tercero_cliente
                                  join ter in context.icb_terceros
                                      on cli.tercero_id equals ter.tercero_id
                                  select new
                                      {
                                      idTercero = ter.tercero_id,
                                      ter.doc_tercero,
                                      nombreTercero = ter.razon_social + ter.prinom_tercero + " " + ter.segnom_tercero + " " +
                                                 ter.apellido_tercero + " " + ter.segapellido_tercero
                                      }).ToList();

            ViewBag.cliente = new SelectList(buscarClientes, "idTercero", "nombreTercero", remisioncliente.nit);


            var buscarEstados = (from estado in context.EstadosRemision
                                 select new
                                     {
                                     id = estado.id,
                                     value = estado.estado
                                     }).ToList();

            ViewBag.Estado = new SelectList(buscarEstados, "id", "value");

         
            icb_sysparameter buscarRolsAsesorRepuestos = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P51");
            int idRolAsesorRepuestos = buscarRolsAsesorRepuestos != null
                ? Convert.ToInt32(buscarRolsAsesorRepuestos.syspar_value)
                : 0;
            var buscarAsesoresRepuestos = (from usuario in context.users
                                           join bodegaUsuario in context.bodega_usuario
                                               on usuario.user_id equals bodegaUsuario.id_usuario
                                           where usuario.rol_id == idRolAsesorRepuestos && bodegaUsuario.id_bodega == bodegaActual
                                           select new
                                               {
                                               usuario.user_id,
                                               nombre = usuario.user_nombre + " " + usuario.user_apellido
                                               }).ToList();
            ViewBag.asesor = new SelectList(buscarAsesoresRepuestos, "user_id", "nombre", remisioncliente.vendedor);

            var listT = (from t in context.tp_doc_registros
                         where t.tpdoc_id == 3037
                         select new
                             {
                             id = t.tpdoc_id,
                             tipo = "(" + t.prefijo + ")" + t.tpdoc_nombre
                             }).ToList();

          
            ViewBag.TipoDocumento = new SelectList(listT, "id", "tipo", remisioncliente.tipo); 


            var listB = (from t in context.icb_doc_consecutivos
                         where t.doccons_idtpdoc == 3037
                         select new
                             {
                             id = t.bodega_concesionario.id,
                             tipo = t.bodega_concesionario.bodccs_nombre
                             }).ToList();


            ViewBag.bodega = new SelectList(listB, "id", "tipo", remisioncliente.bodega);

            var idb = Convert.ToInt32(Session["user_usuarioid"]);
            var listT1 = (from t in context.users
                          where t.user_id == idb
                          select new
                              {
                              id = t.user_id,
                              tipo = t.user_nombre
                              }).ToList();

        
            ViewBag.Asesor = new SelectList(listT1, "id", "tipo", remisioncliente.vendedor);
            icb_sysparameter buscarperfiles = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P102");
            int idperfil = buscarperfiles != null
                ? Convert.ToInt32(buscarperfiles.syspar_value)
                : 0;

            var buscarPerfilContable = context.perfil_contable_documento.Where(x => x.tipo == idperfil).Select(x => new
                {
                value = x.id,
                text = x.descripcion
                }).ToList();

         
            ViewBag.perfilcontable = new SelectList(buscarPerfilContable, "value", "text");


            return View(remimodelo);
            }

        [HttpPost]
        public ActionResult Edit(RemisionClienteModel modelo) {



            using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                {
                try
                    {
                    icb_sysparameter buscarperfiles = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P102");
                    int idperfil = buscarperfiles != null ? Convert.ToInt32(buscarperfiles.syspar_value) : 0;
                    long numeroConsecutivo = 0;
                    grupoconsecutivos buscarGrupoConsecutivos = context.grupoconsecutivos.FirstOrDefault(x =>
                        x.documento_id == modelo.TipoDocumento && x.bodega_id == modelo.bodega);
                    int numeroGrupo = buscarGrupoConsecutivos != null ? buscarGrupoConsecutivos.grupo : 0;
                    ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
                    icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
                    tp_doc_registros buscarTipoDocRegistro =
                        context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == idperfil);
                    numeroConsecutivoAux =
                        gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro, idperfil, modelo.bodega);


                    encab_documento crearEncabezado = new encab_documento();

                    crearEncabezado.tipo = idperfil;
                    crearEncabezado.bodega = modelo.bodega;
                    crearEncabezado.numero = numeroConsecutivo;
                    crearEncabezado.documento = modelo.Referencia;
                    crearEncabezado.fecha = DateTime.Now;
                    crearEncabezado.fec_creacion = DateTime.Now;
                    crearEncabezado.impoconsumo = 0;
                    crearEncabezado.vendedor = modelo.asesor;
                    crearEncabezado.notas = modelo.Notas;
                    crearEncabezado.nit = modelo.cliente;
                    crearEncabezado.perfilcontable = Convert.ToInt32(Request["perfilcontable"]);
                    crearEncabezado.valor_total = Convert.ToInt32(Request["txtTotalPrecio"]);
                    crearEncabezado.prefactura = false;
                    crearEncabezado.estado_factura = 1;
                    context.encab_documento.Add(crearEncabezado);
                    context.SaveChanges();
                    icb_sysparameter buscarNit = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P33");
                    var lineas = context.lineas_documento.Where(x => x.id_encabezado == modelo.encabezado_id).ToList();
                    int nitTraslado = buscarNit != null ? Convert.ToInt32(buscarNit.syspar_value) : 0;
                    int sequ = 1;
                    foreach (var item in lineas)
                        {
                        lineas_documento linea = new lineas_documento();
                        linea.codigo = item.codigo;
                        linea.fec_creacion = DateTime.Now;
                        linea.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                        linea.id_encabezado = crearEncabezado.idencabezado;
                        linea.nit = nitTraslado;
                        linea.idestadoremision = item.idestadoremision;
                        linea.cantidad = item.cantidad;
                        linea.bodega = item.bodega;
                        linea.seq = sequ;
                        linea.estado = true;
                        linea.fec = DateTime.Now;
                        linea.costo_unitario = item.costo_unitario;
                        linea.valor_unitario = item.valor_unitario;
                        linea.porcentaje_iva = item.porcentaje_iva;
                        linea.porcentaje_descuento = item.porcentaje_descuento;

                        context.lineas_documento.Add(linea);

                        //esta tabla hasta la fecha 2020/08/22 no se habia utilizado
                        referencias_comprometidas comprometido = new referencias_comprometidas();
                        comprometido.cantidad =  Convert.ToInt32(item.cantidad);
                        comprometido.doc_cliente = crearEncabezado.nit;
                        comprometido.fecha_comprometido = DateTime.Now;
                        // comprometo con el  id del documento que estoy creando
                        comprometido.numero_remision = crearEncabezado.idencabezado;
                        context.referencias_comprometidas.Add(comprometido);


                        sequ++;

                        }


                    context.SaveChanges();
                    dbTran.Commit();
                    }
                catch (Exception)
                    {
                    dbTran.Rollback();
                    throw;
                    }

                }
            return RedirectToAction("BrowserCajas", "CentralAtencion");
            }
        public JsonResult Buscarlineas(int? idencabezado){


            var data2 = context.lineas_documento.Where(x => x.id_encabezado == idencabezado).Select(s => new
            {
                s.id,
                s.codigo,
                s.icb_referencia.ref_descripcion,
                s.cantidad,
                s.EstadosRemision.estado,
                s.valor_unitario,
                s.porcentaje_descuento,
                s.porcentaje_iva,
            }).ToList();
                var data = data2.Select(s => new
                {
                s.id,
                s.codigo,
                s.ref_descripcion,
                s.cantidad,
                s.estado,
                s.valor_unitario,
                s.porcentaje_descuento,
                s.porcentaje_iva,
                totaldes = calculardescuento(s.cantidad, s.valor_unitario, Convert.ToDecimal(s.porcentaje_descuento, culturaInfo)),
                totaliva = calculardescuentoIva(s.cantidad, s.valor_unitario, Convert.ToDecimal(s.porcentaje_descuento, culturaInfo), Convert.ToDecimal(s.porcentaje_iva, culturaInfo)),
                total = calcularTotal(s.cantidad, s.valor_unitario, Convert.ToDecimal(s.porcentaje_descuento, culturaInfo), Convert.ToDecimal(s.porcentaje_iva, culturaInfo))
                }).ToList();


            decimal totaldesc = data.Select(x => x.totaldes).Sum();
            decimal totaliva = data.Select(x => x.totaliva).Sum();
            decimal total = data.Select(x => x.total).Sum();


            return Json( new{ data, totaldesc , totaliva , total }, JsonRequestBehavior.AllowGet);
            }


        public decimal calculardescuento(decimal? cantidad, decimal? precio, decimal? descuento) {
            decimal resultado = 0;
            if ( descuento != 0)
                {
                decimal des = Convert.ToDecimal(descuento, culturaInfo) / 100;
                resultado = Convert.ToDecimal(cantidad, culturaInfo) * Convert.ToInt32(precio, culturaInfo) * des;
                }
            else{
                resultado = 0;
                }

            

            return resultado;
            }

        public decimal calculardescuentoIva(decimal? cantidad, decimal? precio, decimal? descuento, decimal? iva)
            {           
            decimal resultado = 0, tdesc=0;

            if (descuento != 0)
                {
                decimal des = Convert.ToDecimal(descuento, culturaInfo) / 100;
                tdesc = Convert.ToDecimal(cantidad, culturaInfo) * Convert.ToDecimal(precio, culturaInfo) * des;
                }
            else
                {
                tdesc = Convert.ToDecimal(cantidad, culturaInfo) * Convert.ToDecimal(precio, culturaInfo);
                }
            if (iva != 0)
                {
                decimal ivapor = Convert.ToDecimal(iva, culturaInfo) / 100;
                resultado = tdesc * ivapor;
                }
            else {
                resultado = 0;
                }                 

            return resultado;
            }

        public decimal calcularTotal(decimal? cantidad, decimal? precio, decimal? descuento, decimal? iva)
            {
            decimal resultado = 0, tdesc, tiva = 0;

            if ( descuento != 0)
                {
                decimal des = Convert.ToInt32(descuento, culturaInfo) / 100;
                tdesc = Convert.ToDecimal(cantidad, culturaInfo) * Convert.ToDecimal(precio, culturaInfo) * des;
                }
            else
                {
                tdesc = Convert.ToDecimal(cantidad, culturaInfo) * Convert.ToDecimal(precio, culturaInfo);
                }
            if ( iva != 0)
                {
                decimal ivapor = Convert.ToDecimal(iva, culturaInfo) / 100;
                tiva = tdesc * ivapor;
                resultado = tdesc + tiva;
                }
            else {
                resultado = tdesc;


                }

            return resultado;         
            }

        public JsonResult agregar(string codigo, int idencab, int idestado, int cantidad, decimal precio, int bodega, decimal? descuento, decimal? iva) {

            string[] referencia = codigo.Split('|');
            string rrefcod = referencia[0].Trim();


            var linea = context.lineas_documento.Where(x => x.id_encabezado == idencab && x.codigo == rrefcod).FirstOrDefault();
            if (linea == null)
                {
                icb_sysparameter buscarNit = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P33");
                int nitTraslado = buscarNit != null ? Convert.ToInt32(buscarNit.syspar_value) : 0;

                lineas_documento lineanueva = new lineas_documento();

                lineanueva.codigo = referencia[0];
                lineanueva.fec_creacion = DateTime.Now;
                lineanueva.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                lineanueva.nit = nitTraslado;
                lineanueva.idestadoremision = idestado;
                lineanueva.cantidad = cantidad;
                lineanueva.bodega = bodega;
                lineanueva.seq = 1;
                lineanueva.estado = true;
                lineanueva.fec = DateTime.Now;
                lineanueva.costo_unitario = precio;
                lineanueva.valor_unitario = precio;
                lineanueva.id_encabezado = idencab;
                lineanueva.porcentaje_descuento = descuento != null ? float.Parse(descuento.Value.ToString()) : 0;
                lineanueva.porcentaje_iva = iva != null ? float.Parse(iva.Value.ToString()) : 0;
                context.lineas_documento.Add(lineanueva);

                }
            else {

                linea.costo_unitario = precio;
                linea.valor_unitario = precio;
                linea.cantidad = cantidad;
                linea.porcentaje_descuento = descuento != null ? float.Parse(descuento.Value.ToString()) : 0;
                linea.porcentaje_iva = iva != null ? float.Parse(iva.Value.ToString()) : 0;
                linea.idestadoremision = idestado;
                context.Entry(linea).State = EntityState.Modified;
                }
            context.SaveChanges();

            return Json(true, JsonRequestBehavior.AllowGet);
            }

        public JsonResult eliminarlinea(int lineadoc) {

            lineas_documento linea = context.lineas_documento.Where(x => x.id == lineadoc).FirstOrDefault();
            context.lineas_documento.Remove(linea);
            context.SaveChanges();

            return Json(true, JsonRequestBehavior.AllowGet);
            }


        public ActionResult crearPDFRemisionCliente(int? id)
        {


            if (id != null)
            {

                encab_documento encab = context.encab_documento.Where(d => d.idencabezado == id && d.tipo == 3037).OrderByDescending(d => d.idencabezado).FirstOrDefault();

                var prefijo = context.tp_doc_registros.Where(d => d.tpdoc_id == 3037).Select(d => d.prefijo).FirstOrDefault();

                var lineas = context.lineas_documento.Where(d => d.id_encabezado == id).ToList();

                grupoconsecutivos grupoint = context.grupoconsecutivos.FirstOrDefault(x => x.documento_id == 3037 && x.bodega_id == encab.bodega);

                DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                var bsconcecutivo = doc.BuscarConsecutivo(grupoint.grupo);


                string asesor = "";
                if (encab.vendedor != null)
                {
                    users tecnico = context.users.Where(d => d.user_id == encab.vendedor).FirstOrDefault();
                    if (tecnico != null)
                    {
                        asesor = tecnico.user_nombre + " " + tecnico.user_apellido;
                    }
                }

                string cliente = "";
                string doc_cliente = "";
                if (encab.nit > 0)
                {
                    icb_terceros clie = context.icb_terceros.Where(d => d.tercero_id == encab.nit).FirstOrDefault();
                    if (clie != null)
                    {
                        cliente = clie.prinom_tercero != null ? clie.prinom_tercero + " " + clie.apellido_tercero : clie.razon_social;
                        doc_cliente = clie.doc_tercero;
                    }
                }

                decimal totaldescuentorepuestos = 0;
                decimal repuestosbruto = 0;
                decimal totalivarepuestos = 0;
                decimal totalrepuestos = 0;
                decimal totalrepuestosiniva = 0;
                decimal totalrepuestosbaseiva = 0;
                //decimal totalflete = 0;

                decimal totaltotal = 0;

                PdfRemision informe = new PdfRemision
                {
                    //idRemision, numero de remision, fecha, referencias, cantidades, documento cliente, asesor Seccion de totales: Subtotal, iva, descuento, valor total.
                    idRemision = encab.idencabezado,
                    numRemision = encab.numero.ToString(),
                    fecha = encab.fecha.ToString("dd/MM/yyyy", new CultureInfo("en-US")),
                    prefijo = prefijo != null ? prefijo : "",
                    consecutivo = bsconcecutivo,
                    documento = doc_cliente,
                    cliente = cliente,
                    asesor = asesor != null ? asesor : ""

                };

                informe.repuestos = lineas.GroupBy(e => e.codigo).Select(e => new repuestos_remision
                {

                    //x.Select(e => e.nombre_cliente).FirstOrDefault()
                    id = e.Select(x => x.id).FirstOrDefault(),
                    codigo = !string.IsNullOrWhiteSpace(e.Select(x => x.codigo).FirstOrDefault()) ? e.Select(x => x.codigo).FirstOrDefault() : e.Select(x => x.codigo).FirstOrDefault(),
                    nombre = !string.IsNullOrWhiteSpace(e.Select(x => x.codigo).FirstOrDefault())
                             ? e.Select(x => x.codigo).FirstOrDefault()
                             : e.Select(x => x.codigo).FirstOrDefault(),
                    cantidad = e.Select(x => x.cantidad).FirstOrDefault().ToString(),
                    precio_unitario = e.Select(x => x.costo_unitario).FirstOrDefault().ToString("N0", new CultureInfo("is-IS")),
                    valorBruto = Math.Round(calcularbruto(e.Select(x => x.costo_unitario).FirstOrDefault(), Convert.ToDecimal(e.Select(x => x.porcentaje_descuento).FirstOrDefault()), Convert.ToDecimal(e.Select(x => x.porcentaje_iva).FirstOrDefault()))).ToString(),
                    porcentaje_iva = Convert.ToDecimal(e.Select(x => x.porcentaje_iva).FirstOrDefault()).ToString("N2", new CultureInfo("is-IS")),
                    porcentaje_descuento = Convert.ToDecimal(e.Select(x => x.porcentaje_descuento).FirstOrDefault()).ToString("N2", new CultureInfo("is-IS")),
                    totaldescuento = Math.Round(calculardescuentore(e.Select(x => x.costo_unitario).FirstOrDefault(), e.Select(x => x.cantidad).FirstOrDefault(), Convert.ToDecimal(e.Select(x => x.porcentaje_descuento).FirstOrDefault()))).ToString(),
                    totaliva = Math.Round(calcularivare(e.Select(x => x.costo_unitario).FirstOrDefault(), e.Select(x => x.cantidad).FirstOrDefault(), Convert.ToDecimal(e.Select(x => x.porcentaje_descuento).FirstOrDefault()), Convert.ToDecimal(e.Select(x => x.porcentaje_iva).FirstOrDefault()))).ToString(),
                    precio_total = Math.Round((e.Select(x => x.costo_unitario).FirstOrDefault() * e.Select(x => x.cantidad).FirstOrDefault()) + calcularivare(e.Select(x => x.costo_unitario).FirstOrDefault(), e.Select(x => x.cantidad).FirstOrDefault(), Convert.ToDecimal(e.Select(x => x.porcentaje_descuento).FirstOrDefault()), Convert.ToDecimal(e.Select(x => x.porcentaje_iva).FirstOrDefault())) - calculardescuentore(e.Select(x => x.costo_unitario).FirstOrDefault(), e.Select(x => x.cantidad).FirstOrDefault(), Convert.ToDecimal(e.Select(x => x.porcentaje_descuento).FirstOrDefault()))).ToString(),
                    totalsiniva = Math.Round(Convert.ToDecimal(e.Select(x => x.costo_unitario).FirstOrDefault() * e.Select(x => x.cantidad).FirstOrDefault(), new CultureInfo("is-IS")) - calculardescuentore(e.Select(x => x.costo_unitario).FirstOrDefault(), e.Select(x => x.cantidad).FirstOrDefault(), Convert.ToDecimal(e.Select(x => x.porcentaje_descuento).FirstOrDefault())) - calcularivare(e.Select(x => x.costo_unitario).FirstOrDefault(), e.Select(x => x.cantidad).FirstOrDefault(), Convert.ToDecimal(e.Select(x => x.porcentaje_descuento).FirstOrDefault()), Convert.ToDecimal(e.Select(x => x.porcentaje_iva).FirstOrDefault()))).ToString()

                }).ToList();


                totalrepuestos = informe.repuestos.Sum(d => Convert.ToInt32(d.precio_total));
                totalivarepuestos = informe.repuestos.Sum(d => Convert.ToInt32(d.totaliva));
                totaldescuentorepuestos = informe.repuestos.Sum(d => Convert.ToInt32(d.totaldescuento));
                totalrepuestosiniva = informe.repuestos.Sum(d => Convert.ToInt32(d.totalsiniva));
                repuestosbruto = informe.repuestos.Sum(d => Convert.ToInt32(d.valorBruto));
                totalrepuestosbaseiva = informe.repuestos.Sum(d => Convert.ToInt32(d.valorBaseiva));

                decimal subtotal = Math.Round(totalrepuestosiniva);
                decimal valorBruto = Math.Round(repuestosbruto);
                decimal totaliva = Math.Round(totalivarepuestos);
                decimal valorBaseiva = Math.Round(totalrepuestosbaseiva);
                decimal totalDescuentos = Math.Round(totaldescuentorepuestos);
                totaltotal = Math.Round(totalrepuestos);

                informe.subtotal = Math.Round(subtotal).ToString();
                informe.valoriva = Math.Round(totaliva).ToString();
                informe.totaldescuento = Math.Round(totalDescuentos).ToString();
                informe.totalFactura = Math.Round(totaltotal).ToString();

                DocumentoPorBodegaController docu = new DocumentoPorBodegaController();
                var concecutivoNuevo = docu.ActualizarConsecutivo(grupoint.grupo, informe.consecutivo);

                //string nombre = "Remision";
                //string nombre2 = nombre;
                //nombre = nombre + "file.pdf";

                string root = Server.MapPath("~/Pdf/");
                string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
                string path = Path.Combine(root, pdfname);
                path = Path.GetFullPath(path);

                string customSwitches = string.Format("--print-media-type --allow {0} --header-html {0} --header-spacing 5 --footer-html {1} --footer-spacing 0",
                    Url.Action("CabeceraPdfRemision", "remisionClientes", new { prefijo = informe.prefijo, consecutivo = informe.consecutivo, numRemision = informe.numRemision, fecha = informe.fecha }, Request.Url.Scheme), Url.Action("PiePdfRemision", "remisionClientes", new { area = "" }, Request.Url.Scheme));

                ViewAsPdf something = new ViewAsPdf("crearPDFRemisionCliente", informe)
                {
                    PageOrientation = Orientation.Landscape,
                    //FileName = nombre,
                    CustomSwitches = customSwitches,
                    PageSize = Size.Letter,
                    PageMargins = new Margins { Top = 40, Bottom = 20 }
                };
                return something;


            }
            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public ActionResult crearPDFRemision()
        {

            int? buscarUltimoEncabezado = context.encab_documento.OrderByDescending(x => x.idencabezado).Where(x => x.tipo == 3037).Select(x => x.idencabezado).FirstOrDefault();


            if (buscarUltimoEncabezado != null)
            {

                encab_documento encab = context.encab_documento.Where(d => d.idencabezado == buscarUltimoEncabezado && d.tipo == 3037).OrderByDescending(d => d.idencabezado).FirstOrDefault();

                var prefijo = context.tp_doc_registros.Where(d => d.tpdoc_id == 3037).Select(d => d.prefijo).FirstOrDefault();

                var lineas = context.lineas_documento.Where(d => d.id_encabezado == buscarUltimoEncabezado).ToList();

                grupoconsecutivos grupoint = context.grupoconsecutivos.FirstOrDefault(x => x.documento_id == 3037 && x.bodega_id == encab.bodega);

                DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                var bsconcecutivo = doc.BuscarConsecutivo(grupoint.grupo);


                string asesor = "";
                if (encab.vendedor != null)
                {
                    users tecnico = context.users.Where(d => d.user_id == encab.vendedor).FirstOrDefault();
                    if (tecnico != null)
                    {
                        asesor = tecnico.user_nombre + " " + tecnico.user_apellido;
                    }
                }

                string cliente = "";
                string doc_cliente = "";
                if (encab.nit > 0)
                {
                    icb_terceros clie = context.icb_terceros.Where(d => d.tercero_id == encab.nit).FirstOrDefault();
                    if (clie != null)
                    {
                        cliente = clie.prinom_tercero != null ? clie.prinom_tercero + " " + clie.apellido_tercero : clie.razon_social;
                        doc_cliente = clie.doc_tercero;
                    }
                }

                decimal totaldescuentorepuestos = 0;
                decimal repuestosbruto = 0;
                decimal totalivarepuestos = 0;
                decimal totalrepuestos = 0;
                decimal totalrepuestosiniva = 0;
                decimal totalrepuestosbaseiva = 0;
                //decimal totalflete = 0;

                decimal totaltotal = 0;

                PdfRemision informe = new PdfRemision
                {
                    //idRemision, numero de remision, fecha, referencias, cantidades, documento cliente, asesor Seccion de totales: Subtotal, iva, descuento, valor total.
                    idRemision = encab.idencabezado,
                    numRemision = encab.numero.ToString(),
                    fecha = encab.fecha.ToString("dd/MM/yyyy", new CultureInfo("en-US")),
                    prefijo = prefijo != null ? prefijo : "",
                    consecutivo = bsconcecutivo,
                    documento = doc_cliente,
                    cliente = cliente,
                    asesor = asesor != null ? asesor : ""

                };

                informe.repuestos = lineas.GroupBy(e => e.codigo).Select(e => new repuestos_remision
                {

                    //x.Select(e => e.nombre_cliente).FirstOrDefault()
                    id = e.Select(x => x.id).FirstOrDefault(),
                    codigo = !string.IsNullOrWhiteSpace(e.Select(x => x.codigo).FirstOrDefault()) ? e.Select(x => x.codigo).FirstOrDefault() : e.Select(x => x.codigo).FirstOrDefault(),
                    nombre = !string.IsNullOrWhiteSpace(e.Select(x => x.codigo).FirstOrDefault())
                             ? e.Select(x => x.codigo).FirstOrDefault()
                             : e.Select(x => x.codigo).FirstOrDefault(),
                    cantidad = e.Select(x => x.cantidad).FirstOrDefault().ToString(),
                    precio_unitario = e.Select(x => x.costo_unitario).FirstOrDefault().ToString("N0", new CultureInfo("is-IS")),
                    valorBruto = Math.Round(calcularbruto(e.Select(x => x.costo_unitario).FirstOrDefault(), Convert.ToDecimal(e.Select(x => x.porcentaje_descuento).FirstOrDefault()), Convert.ToDecimal(e.Select(x => x.porcentaje_iva).FirstOrDefault()))).ToString(),
                    porcentaje_iva = Convert.ToDecimal(e.Select(x => x.porcentaje_iva).FirstOrDefault()).ToString("N2", new CultureInfo("is-IS")),
                    porcentaje_descuento = Convert.ToDecimal(e.Select(x => x.porcentaje_descuento).FirstOrDefault()).ToString("N2", new CultureInfo("is-IS")),
                    totaldescuento = Math.Round(calculardescuentore(e.Select(x => x.costo_unitario).FirstOrDefault(), e.Select(x => x.cantidad).FirstOrDefault(), Convert.ToDecimal(e.Select(x => x.porcentaje_descuento).FirstOrDefault()))).ToString(),
                    totaliva = Math.Round(calcularivare(e.Select(x => x.costo_unitario).FirstOrDefault(), e.Select(x => x.cantidad).FirstOrDefault(), Convert.ToDecimal(e.Select(x => x.porcentaje_descuento).FirstOrDefault()), Convert.ToDecimal(e.Select(x => x.porcentaje_iva).FirstOrDefault()))).ToString(),
                    precio_total = Math.Round((e.Select(x => x.costo_unitario).FirstOrDefault() * e.Select(x => x.cantidad).FirstOrDefault()) + calcularivare(e.Select(x => x.costo_unitario).FirstOrDefault(), e.Select(x => x.cantidad).FirstOrDefault(), Convert.ToDecimal(e.Select(x => x.porcentaje_descuento).FirstOrDefault()), Convert.ToDecimal(e.Select(x => x.porcentaje_iva).FirstOrDefault())) - calculardescuentore(e.Select(x => x.costo_unitario).FirstOrDefault(), e.Select(x => x.cantidad).FirstOrDefault(), Convert.ToDecimal(e.Select(x => x.porcentaje_descuento).FirstOrDefault()))).ToString(),
                    totalsiniva = Math.Round(Convert.ToDecimal(e.Select(x => x.costo_unitario).FirstOrDefault() * e.Select(x => x.cantidad).FirstOrDefault(), new CultureInfo("is-IS")) - calculardescuentore(e.Select(x => x.costo_unitario).FirstOrDefault(), e.Select(x => x.cantidad).FirstOrDefault(), Convert.ToDecimal(e.Select(x => x.porcentaje_descuento).FirstOrDefault())) - calcularivare(e.Select(x => x.costo_unitario).FirstOrDefault(), e.Select(x => x.cantidad).FirstOrDefault(), Convert.ToDecimal(e.Select(x => x.porcentaje_descuento).FirstOrDefault()), Convert.ToDecimal(e.Select(x => x.porcentaje_iva).FirstOrDefault()))).ToString()

                }).ToList();


                totalrepuestos = informe.repuestos.Sum(d => Convert.ToInt32(d.precio_total));
                totalivarepuestos = informe.repuestos.Sum(d => Convert.ToInt32(d.totaliva));
                totaldescuentorepuestos = informe.repuestos.Sum(d => Convert.ToInt32(d.totaldescuento));
                totalrepuestosiniva = informe.repuestos.Sum(d => Convert.ToInt32(d.totalsiniva));
                repuestosbruto = informe.repuestos.Sum(d => Convert.ToInt32(d.valorBruto));
                totalrepuestosbaseiva = informe.repuestos.Sum(d => Convert.ToInt32(d.valorBaseiva));

                decimal subtotal = Math.Round(totalrepuestosiniva);
                decimal valorBruto = Math.Round(repuestosbruto);
                decimal totaliva = Math.Round(totalivarepuestos);
                decimal valorBaseiva = Math.Round(totalrepuestosbaseiva);
                decimal totalDescuentos = Math.Round(totaldescuentorepuestos);
                totaltotal = Math.Round(totalrepuestos);

                informe.subtotal = Math.Round(subtotal).ToString();
                informe.valoriva = Math.Round(totaliva).ToString();
                informe.totaldescuento = Math.Round(totalDescuentos).ToString();
                informe.totalFactura = Math.Round(totaltotal).ToString();

                DocumentoPorBodegaController docu = new DocumentoPorBodegaController();
                var concecutivoNuevo = docu.ActualizarConsecutivo(grupoint.grupo, informe.consecutivo);

                //string nombre = "Remision";
                //string nombre2 = nombre;
                //nombre = nombre + "file.pdf";

                string root = Server.MapPath("~/Pdf/");
                string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
                string path = Path.Combine(root, pdfname);
                path = Path.GetFullPath(path);

                string customSwitches = string.Format("--print-media-type --allow {0} --header-html {0} --header-spacing 5 --footer-html {1} --footer-spacing 0",
                    Url.Action("CabeceraPdfRemision", "remisionClientes", new { prefijo = informe.prefijo, consecutivo = informe.consecutivo, numRemision = informe.numRemision, fecha = informe.fecha }, Request.Url.Scheme), Url.Action("PiePdfRemision", "remisionClientes", new { area = "" }, Request.Url.Scheme));

                ViewAsPdf something = new ViewAsPdf("crearPDFRemision", informe)
                {
                    PageOrientation = Orientation.Landscape,
                    //FileName = nombre,
                    CustomSwitches = customSwitches,
                    PageSize = Size.Letter,
                    PageMargins = new Margins { Top = 40, Bottom = 20 }
                };
                return something;


            }
            return Json(0, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult CabeceraPdfRemision(string prefijo, long consecutivo, string numRemision, string fecha)
        {
            var recibido = Request;
            var modelo2 = new PdfRemision
            {
                prefijo = prefijo,
                consecutivo = consecutivo,
                numRemision = numRemision,
                fecha = fecha,
            };

            return View(modelo2);
        }

        [AllowAnonymous]
        public ActionResult PiePdfRemision()
        {
            return View();
        }

        public JsonResult buscarEstado(int id)
        {

            var data = context.EstadosRemision.Where(x => x.id == id).Select(x =>
                    new { id = x.id, estado = (x.estado != null && x.estado!=string.Empty)? x.estado : "" })
                .FirstOrDefault();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public decimal calcularbruto(decimal? valor, decimal? pordescuento, decimal? poriva)
        {
            decimal respuesta = 0;
            if (valor != null && pordescuento != null && poriva != null)
            {
                respuesta = valor.Value - pordescuento.Value - poriva.Value;
            }

            return respuesta;
        }

        public decimal calculardescuentore(decimal? valor, decimal? cantidad, decimal? pordescuento)
        {
            decimal respuesta = 0;
            if (valor != null && cantidad != null && pordescuento != null)
            {
                respuesta = (valor.Value * cantidad.Value * pordescuento.Value) / 100;
            }

            return respuesta;
        }

        public decimal calcularivare(decimal? valor, decimal? cantidad, decimal? pordescuento, decimal? poriva)
        {
            decimal respuesta = 0;
            decimal total = 0;
            if (valor != null && poriva != null)
            {
                if (pordescuento != null)
                {
                    total = valor.Value * cantidad.Value;
                    respuesta = (total * poriva.Value) / 100;
                }
                else
                {
                    respuesta = valor.Value * cantidad.Value * poriva.Value / 100;
                }
            }

            return respuesta;
        }

        public void AgregarRegistroCuentasValores(mov_contable movNuevo, int centro, int cuenta, int idCentroCero,
            int idTerceroCero)
        {
            cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                x.centro == centro && x.cuenta == cuenta && x.nit == movNuevo.nit);
            DateTime fechaHoy = DateTime.Now;
            if (buscar_cuentas_valores != null)
            {
                buscar_cuentas_valores.ano = fechaHoy.Year;
                buscar_cuentas_valores.mes = fechaHoy.Month;
                buscar_cuentas_valores.cuenta = movNuevo.cuenta;
                buscar_cuentas_valores.centro = movNuevo.centro;
                //buscar_cuentas_valores.nit = movNuevo.nit ?? idTerceroCero;
                buscar_cuentas_valores.nit = movNuevo.nit;
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
                    //crearCuentaValor.nit = movNuevo.nit ?? idTerceroCero;
                    nit = movNuevo.nit,
                    debito = movNuevo.debito,
                    credito = movNuevo.credito,
                    debitoniff = movNuevo.debitoniif,
                    creditoniff = movNuevo.creditoniif
                };
                context.cuentas_valores.Add(crearCuentaValor);
            }
            context.SaveChanges();
        }


        public JsonResult BuscarBodegasYPerfiles(int tp_documento)
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            var buscarBodegasDelDocumento = (from doc_consecutivos in context.icb_doc_consecutivos
                                             join bodega in context.bodega_concesionario
                                                 on doc_consecutivos.doccons_bodega equals bodega.id
                                             where doc_consecutivos.doccons_idtpdoc == tp_documento &&
                                                   doc_consecutivos.doccons_bodega == bodegaActual && bodega.es_repuestos
                                             select new
                                             {
                                                 bodega.id,
                                                 bodega.bodccs_nombre
                                             }).ToList();

            var buscarPerfiles = context.perfil_contable_documento.Where(x => x.tipo == tp_documento).Select(x => new
            {
                x.id,
                x.descripcion
            }).ToList();

            return Json(new { buscarBodegasDelDocumento, buscarPerfiles }, JsonRequestBehavior.AllowGet);
        }


        public void ListasDesplegables()
        {
            //ViewBag.tp_doc_registros = context.tp_doc_registros.Where(x => x.sw == 3 && x.tipo == 1007).ToList();
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            //ViewBag.TipoDocumento = new SelectList(context.tp_doc_registros, "tpdoc_id", "tpdoc_nombre");

            var buscarClientes = (from pro in context.tercero_proveedor
                                  join ter in context.icb_terceros
                                      on pro.tercero_id equals ter.tercero_id
                                  select new
                                  {
                                      idTercero = ter.tercero_id,
                                      ter.doc_tercero,
                                      nombreTercero = ter.razon_social + ter.prinom_tercero + " " + ter.segnom_tercero + " " +
                                                 ter.apellido_tercero + " " + ter.segapellido_tercero
                                  }).ToList();

            ViewBag.cliente = new SelectList(buscarClientes, "idTercero", "nombreTercero");


            var buscarEstados = (from estado in context.EstadosRemision
                                 select new
                                 {
                                     id = estado.id,
                                     value = estado.estado
                                 }).ToList();

            ViewBag.Estado = new SelectList(buscarEstados, "id", "value");

            //ViewBag.BodegaOrigen = new SelectList(context.bodega_concesionario.Where(x=>x.id == bodegaActual && x.es_repuestos == true), "id", "bodccs_nombre");
            //ViewBag.BodegaDestino = new SelectList(context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).Where(x => x.es_repuestos == true), "id", "bodccs_nombre");

            /*var buscarReferencias = context.icb_referencia.Where(x => x.modulo == "R").ToList();
            var items = new List<SelectListItem>();
            foreach (var item in buscarReferencias)
            {
                var nombre = "(" + item.ref_codigo + ") " + item.ref_descripcion;
                items.Add(new SelectListItem { Text = nombre, Value = item.ref_codigo });
            }

            ViewBag.Referencia = new SelectList(items, "Value", "Text");*/
            ViewBag.Referencia = "";
            icb_sysparameter buscarRolsAsesorRepuestos = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P51");
            int idRolAsesorRepuestos = buscarRolsAsesorRepuestos != null
                ? Convert.ToInt32(buscarRolsAsesorRepuestos.syspar_value)
                : 0;
            var buscarAsesoresRepuestos = (from usuario in context.users
                                           join bodegaUsuario in context.bodega_usuario
                                               on usuario.user_id equals bodegaUsuario.id_usuario
                                           where usuario.rol_id == idRolAsesorRepuestos && bodegaUsuario.id_bodega == bodegaActual
                                           select new
                                           {
                                               usuario.user_id,
                                               nombre = usuario.user_nombre + " " + usuario.user_apellido
                                           }).ToList();
            ViewBag.asesor = new SelectList(buscarAsesoresRepuestos, "user_id", "nombre");

            var listT = (from t in context.tp_doc_registros
                         where t.tpdoc_id == 3037
                         select new
                         {
                             id = t.tpdoc_id,
                             tipo = "(" + t.prefijo + ")" + t.tpdoc_nombre
                         }).ToList();

            List<SelectListItem> lista2 = new List<SelectListItem>();
            foreach (var item in listT)
            {
                lista2.Add(new SelectListItem
                {
                    Text = item.tipo,
                    Value = item.id.ToString()
                });
            }
            ViewBag.TipoDocumento = lista2;


            var listB = (from t in context.icb_doc_consecutivos
                         where t.doccons_idtpdoc == 3037
                         select new
                         {
                             id = t.bodega_concesionario.id,
                             tipo = t.bodega_concesionario.bodccs_nombre
                         }).ToList();

            List<SelectListItem> lista3 = new List<SelectListItem>();
            foreach (var item in listB)
            {
                lista3.Add(new SelectListItem
                {
                    Text = item.tipo,
                    Value = item.id.ToString()
                });
            }

            ViewBag.bodega = lista3;

            var id = Convert.ToInt32(Session["user_usuarioid"]);
            var listT1 = (from t in context.users
                          where t.user_id == id
                          select new
                          {
                              id = t.user_id,
                              tipo = t.user_nombre
                          }).ToList();

            List<SelectListItem> lista1 = new List<SelectListItem>();
            foreach (var item in listT1)
            {
                lista1.Add(new SelectListItem
                {
                    Text = item.tipo,
                    Value = item.id.ToString()
                });
            }
            ViewBag.Asesor = lista1;

        }

        public JsonResult BuscarRemisiones()
        {

            //validarComprometido();

            var buscar = (from remision in context.vw_remisionClientes
                          select new
                          {

                              remision.idencabezado,
                              remision.numero,
                              remision.fecha2,
                              remision.nombreReferencias,
                              remision.doc_tercero,
                              remision.proveedor,
                              remision.asesor

                          }).ToList();

            var data = buscar.Select(d => new {

                id = d.idencabezado,
                remision = d.numero,
                fecha = d.fecha2,
                referencias = d.nombreReferencias,
                documento = d.doc_tercero,
                cliente = d.proveedor,
                asesor = d.asesor

            }).ToList();


            return Json(data, JsonRequestBehavior.AllowGet);

        }

        public JsonResult BuscarDatosReferencia(string cod_referencia)
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            var buscarReferencia = (from referencia in context.icb_referencia
                                    join promedio in context.vw_promedio
                                        on referencia.ref_codigo equals promedio.codigo into pro
                                    from promedio in pro.DefaultIfEmpty()
                                    join inventarioHoy in context.vw_inventario_hoy
                                        on referencia.ref_codigo equals inventarioHoy.ref_codigo into hoy
                                    from inventarioHoy in hoy.DefaultIfEmpty()
                                    where referencia.ref_codigo == cod_referencia && inventarioHoy.bodega == bodegaActual
                                    select new
                                    {
                                        referencia.ref_codigo,
                                        referencia.ref_descripcion,
                                        promedio.Promedio,
                                        inventarioHoy.stock,
                                        referencia.por_dscto,
                                        referencia.por_iva
                                    }).FirstOrDefault();
            if (buscarReferencia == null)
            {
                return Json(new { encontrado = false, buscarReferencia }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { encontrado = true, buscarReferencia }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarDatosCliente(int id_tercero)
        {
            var buscarTercero2 = (from tercero in context.icb_terceros
                                  join cliente in context.tercero_cliente
                                      on tercero.tercero_id equals cliente.tercero_id
                                  where tercero.tercero_id == id_tercero
                                  select new
                                  {
                                      tercero.telf_tercero,
                                      tercero.celular_tercero,
                                      cliente.telefono,
                                      tercero.email_tercero
                                  }).FirstOrDefault();

            var buscarTercero = (from tercero in context.icb_terceros
                                 join cliente in context.tercero_cliente
                                     on tercero.tercero_id equals cliente.tercero_id
                                 where tercero.tercero_id == id_tercero
                                 select new
                                 {
                                     tercero.tercero_id,
                                     tercero.telf_tercero,
                                     tercero.celular_tercero,
                                     cliente.telefono,
                                     direcciones = (from tercero2 in context.icb_terceros
                                                    join direccion in context.terceros_direcciones
                                                        on tercero2.tercero_id equals direccion.idtercero //into ps
                                                                                                          //from direccion in ps.DefaultIfEmpty()
                                                    where tercero2.tercero_id == tercero.tercero_id
                                                    select new
                                                    {
                                                        direccion.direccion,
                                                        direccion.ciudad
                                                    }).ToList(),
                                     tercero.email_tercero
                                 }).FirstOrDefault();

            List<string> listaTelefonos = new List<string>();
            if (buscarTercero != null)
            {
                if (!string.IsNullOrEmpty(buscarTercero.telf_tercero))
                {
                    listaTelefonos.Add(buscarTercero.telf_tercero);
                }

                if (!string.IsNullOrEmpty(buscarTercero.celular_tercero))
                {
                    listaTelefonos.Add(buscarTercero.celular_tercero);
                }

                if (!string.IsNullOrEmpty(buscarTercero.telefono))
                {
                    listaTelefonos.Add(buscarTercero.telefono);
                }
            }

            return Json(new { buscarTercero, listaTelefonos }, JsonRequestBehavior.AllowGet);
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

        public JsonResult traerReferencias(string referencia /*cadena de la referencia a buscar*/)
        {
            var referencias = (from r in context.icb_referencia
                               where r.ref_descripcion.Contains(referencia) || r.ref_codigo.Contains(referencia)
                               select new
                               {
                                   referencia = r.ref_codigo + " | " + r.ref_descripcion
                               }).ToList();
            List<string> referencias_data = referencias.Select(d => d.referencia).ToList();
            return Json(referencias_data);
        }


        public JsonResult buscarEstados()
        {

            var buscar = context.EstadosRemision.ToList();

            var data = buscar.Select(d => new {

                id = d.id,
                estado = d.estado,
                motivo = d.motivo,
                habilitado = d.habilitado,

            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult EstadosRemision(int? menu)
        {

            return View();

        }

        public JsonResult AgregarEstado(string estado, bool? habilitado, string motivo)
        {

            int result = 0;

            if (!string.IsNullOrWhiteSpace(estado) && habilitado != null)
            {

                EstadosRemision e = new EstadosRemision();

                e.estado = estado;
                e.habilitado = Convert.ToBoolean(habilitado);
                e.motivo = motivo;

                context.EstadosRemision.Add(e);
                result = context.SaveChanges();

            }
            else
            {
                result = 0;
            }

            return Json(result, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult actualizar_EstadosRemision(int? menu, int? id)
        {

            var buscar = context.EstadosRemision.Where(d => d.id == id).FirstOrDefault();

            ViewBag.id = buscar.id;
            ViewBag.Estado = buscar.estado;
            ViewBag.motivo = buscar.motivo;
            ViewBag.habilitado = buscar.habilitado;

            return View();

        }


        public JsonResult editarEstados(int? id, string estado, bool? habilitado, string motivo)
        {

            var result = 0;

            if (id != null && !string.IsNullOrWhiteSpace(estado) && habilitado != null)
            {

                var e = context.EstadosRemision.Find(id);

                e.estado = estado;
                e.habilitado = Convert.ToBoolean(habilitado);
                e.motivo = motivo;


                context.Entry(e).State = EntityState.Modified;
                result = context.SaveChanges();

            }
            else
            {
                result = 0;
            }

            return Json(result, JsonRequestBehavior.AllowGet);

        }


    }
}