using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using OfficeOpenXml;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class bajarCostoReferenciaController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");

        // GET: bajarCostoReferencia
        public ActionResult Create(int? menu)
        {
            ListasDesplegables();
            BuscarFavoritos(menu);
            return View();
        }


        [HttpPost]
        public ContentResult UploadFiles()
        {
            List<UploadFilesResult> r = new List<UploadFilesResult>();

            foreach (string file in Request.Files)
            {
                System.Web.HttpPostedFileBase hpf = Request.Files[file];
                if (hpf.ContentLength == 0)
                {
                    continue;
                }

                string savedFileName = Path.Combine(Server.MapPath("~/Content"), Path.GetFileName(hpf.FileName));
                hpf.SaveAs(savedFileName);

                r.Add(new UploadFilesResult
                {
                    Name = hpf.FileName,
                    Length = hpf.ContentLength,
                    Type = hpf.ContentType
                });
            }

            return Content(
                "{\"name\":\"" + r[0].Name + "\",\"type\":\"" + r[0].Type + "\",\"size\":\"" +
                string.Format("{0} bytes", r[0].Length) + "\"}", "application/json");
        }


        public JsonResult CargarExcel(string nombre_archivo)
        {
            //var bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            int bodegaActual = 1;
            string path = Server.MapPath("~/Content/" + nombre_archivo);
            if (nombre_archivo.Contains(".xls") || nombre_archivo.Contains(".xlsx"))
            {
                ExcelPackage package = new ExcelPackage(new FileInfo(path));
                ExcelWorksheet workSheet = package.Workbook.Worksheets[1];
                List<ModificaCostosRefereciaAsync> listaReferencias = new List<ModificaCostosRefereciaAsync>();
                for (int i = workSheet.Dimension.Start.Row + 1; i <= workSheet.Dimension.End.Row; i++)
                {
                    try
                    {
                        string codigoReferencia = workSheet.Cells[i, 1].Value.ToString();
                        //var buscarReferencia = context.icb_referencia.FirstOrDefault(x=>x.ref_codigo == codigoReferencia);
                        var buscarReferencia = (from referencia in context.icb_referencia
                                                join promedio in context.vw_promedio
                                                    on referencia.ref_codigo equals promedio.codigo into pro
                                                from promedio in pro.DefaultIfEmpty()
                                                join inventarioHoy in context.vw_inventario_hoy
                                                    on referencia.ref_codigo equals inventarioHoy.ref_codigo into hoy
                                                from inventarioHoy in hoy.DefaultIfEmpty()
                                                where referencia.ref_codigo == codigoReferencia && inventarioHoy.bodega == bodegaActual
                                                select new
                                                {
                                                    descripcion = referencia.ref_descripcion,
                                                    promedio = promedio.Promedio != null ? promedio.Promedio : 0
                                                }).FirstOrDefault();
                        if (buscarReferencia != null)
                        {
                            listaReferencias.Add(new ModificaCostosRefereciaAsync
                            {
                                codigo = workSheet.Cells[i, 1].Value.ToString(),
                                costo = Convert.ToDecimal(workSheet.Cells[i, 2].Value.ToString(), miCultura),
                                descripcion = buscarReferencia.descripcion,
                                valor = buscarReferencia.promedio ?? 0
                            });
                        }
                        else
                        {
                            System.IO.File.Delete(path);
                            return Json(
                                new
                                {
                                    error = true,
                                    error_message = "La referencia con codigo " + codigoReferencia + " en la linea " +
                                                    i + " no existe para la bodega actual, por favor valide."
                                }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception)
                    {
                        System.IO.File.Delete(path);
                        return Json(
                            new
                            {
                                error = true,
                                error_message = "El archivo contiene errores en linea " + i + ", por favor valide."
                            }, JsonRequestBehavior.AllowGet);
                    }
                }

                System.IO.File.Delete(path);
                return Json(new { error = false, listaReferencias }, JsonRequestBehavior.AllowGet);
            }

            System.IO.File.Delete(path);
            return Json(
                new { error = true, error_message = "El archivo no es un archivo valido o se encuentra abierto." },
                JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult Create(EntradaSalidaModel modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                long numeroConsecutivo = 0;
                ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
                icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
                tp_doc_registros buscarTipoDocRegistro =
                    context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == modelo.TipoDocumento);
                numeroConsecutivoAux = gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro, modelo.TipoDocumento,
                    modelo.BodegaOrigen);


                grupoconsecutivos buscarGrupoConsecutivos = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == modelo.TipoDocumento && x.bodega_id == modelo.BodegaOrigen);
                int numeroGrupo = buscarGrupoConsecutivos != null ? buscarGrupoConsecutivos.grupo : 0;

                //var numeroConsecutivoAux = context.icb_doc_consecutivos.OrderByDescending(x => x.doccons_ano).FirstOrDefault(x => x.doccons_idtpdoc == modelo.TipoDocumento && x.doccons_bodega == modelo.BodegaOrigen);
                if (numeroConsecutivoAux != null)
                {
                    numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                }
                else
                {
                    TempData["mensaje_error"] = "No existe un numero consecutivo asignado para este tipo de documento";
                    ListasDesplegables();
                    ViewBag.documentoSeleccionado = modelo.TipoDocumento;
                    ViewBag.bodegaSeleccionada = modelo.BodegaOrigen;
                    ViewBag.perfilSeleccionado = modelo.PerfilContable;
                    BuscarFavoritos(menu);
                    return View(modelo);
                }

                // Si llega aqui significa que si hay un numero de documento consecutivo 
                decimal sumaCostosReferenciasAux = 0;
                int numero_repuestosAux = Convert.ToInt32(Request["lista_repuestos"]);
                int cantidad_total = Convert.ToInt32(Request["cantidad_total"]);

                if (cantidad_total <= 0)
                {
                    TempData["mensaje_error"] = "No ha agregado ninguna referencia para realizar el proceso";
                    ListasDesplegables();
                    ViewBag.documentoSeleccionado = modelo.TipoDocumento;
                    ViewBag.bodegaSeleccionada = modelo.BodegaOrigen;
                    ViewBag.perfilSeleccionado = modelo.PerfilContable;
                    BuscarFavoritos(menu);
                    return View(modelo);
                }

                for (int i = 1; i <= numero_repuestosAux; i++)
                {
                    string referencia_codigo = Request["codigo_referencia" + i];
                    string referencia_valor = Request["valor_referencia" + i];
                    if (string.IsNullOrEmpty(referencia_codigo) || string.IsNullOrEmpty(referencia_valor))
                    {
                        // Significa que la agregaron y la eliminaron
                    }
                    else
                    {
                        sumaCostosReferenciasAux += Convert.ToDecimal(referencia_valor, miCultura);
                    }
                }


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
                List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();
                centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
                icb_terceros terceroValorCero = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
                int idTerceroCero = centroValorCero != null ? Convert.ToInt32(terceroValorCero.tercero_id) : 0;
                decimal calcularDebito = 0;
                decimal calcularCredito = 0;

                foreach (var parametro in parametrosCuentasVerificar)
                {
                    cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
                    if (buscarCuenta != null)
                    {
                        if (parametro.id_nombre_parametro == 9 || parametro.id_nombre_parametro == 12)
                        {
                            if (parametro.id_nombre_parametro == 9)
                            {
                                if (buscarCuenta.concepniff == 1 || buscarCuenta.concepniff == 5)
                                {
                                    calcularCredito += sumaCostosReferenciasAux;
                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                    {
                                        NumeroCuenta = parametro.cntpuc_numero,
                                        DescripcionParametro = parametro.descripcion_parametro,
                                        ValorDebito = 0,
                                        ValorCredito = sumaCostosReferenciasAux
                                    });
                                }
                                else
                                {
                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                    {
                                        NumeroCuenta = parametro.cntpuc_numero,
                                        DescripcionParametro = parametro.descripcion_parametro,
                                        ValorDebito = 0,
                                        ValorCredito = 0
                                    });
                                }
                            }
                            else
                            {
                                if (buscarCuenta.concepniff == 1 || buscarCuenta.concepniff == 5)
                                {
                                    calcularDebito += sumaCostosReferenciasAux;
                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                    {
                                        NumeroCuenta = parametro.cntpuc_numero,
                                        DescripcionParametro = parametro.descripcion_parametro,
                                        ValorDebito = sumaCostosReferenciasAux,
                                        ValorCredito = 0
                                    });
                                }
                                else
                                {
                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                    {
                                        NumeroCuenta = parametro.cntpuc_numero,
                                        DescripcionParametro = parametro.descripcion_parametro,
                                        ValorDebito = 0,
                                        ValorCredito = 0
                                    });
                                }
                            }
                        }
                    }
                }

                if (calcularCredito != calcularDebito)
                {
                    TempData["documento_descuadrado"] =
                        "El documento no tiene los movimientos calculados correctamente";
                    ViewBag.documentoSeleccionado = modelo.TipoDocumento;
                    ViewBag.bodegaSeleccionada = modelo.BodegaOrigen;
                    ViewBag.perfilSeleccionado = modelo.PerfilContable;

                    ViewBag.documentoDescuadrado = listaDescuadrados;
                    ViewBag.calculoDebito = calcularDebito;
                    ViewBag.calculoCredito = calcularCredito;
                    ListasDesplegables();
                    BuscarFavoritos(menu);
                    return View(modelo);
                }

                icb_sysparameter buscarNit = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P33");
                int nitTraslado = buscarNit != null ? Convert.ToInt32(buscarNit.syspar_value) : 0;

                encab_documento crearEncabezado = new encab_documento
                {
                    tipo = modelo.TipoDocumento,
                    bodega = modelo.BodegaOrigen,
                    numero = numeroConsecutivo,
                    nit = nitTraslado,
                    //crearEncabezado.documento = modelo.Referencia; // Puede haber varias referencias
                    fecha = DateTime.Now,
                    fec_creacion = DateTime.Now,
                    vencimiento = DateTime.Now,
                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    impoconsumo = 0,
                    perfilcontable = modelo.PerfilContable,
                    notas = modelo.Notas
                };
                context.encab_documento.Add(crearEncabezado);

                bool guardar = context.SaveChanges() > 0;
                encab_documento buscarUltimoEncabezado =
                    context.encab_documento.OrderByDescending(x => x.idencabezado).FirstOrDefault();


                decimal sumaCostosReferencias = 0;
                int numero_repuestos = Convert.ToInt32(Request["lista_repuestos"]);
                int sequ = 1;
                for (int i = 1; i <= numero_repuestos; i++)
                {
                    string referencia_codigo = Request["codigo_referencia" + i];
                    string referencia_valor = Request["valor_referencia" + i];
                    if (string.IsNullOrEmpty(referencia_codigo) || string.IsNullOrEmpty(referencia_valor))
                    {
                        // Significa que la agregaron y la eliminaron
                    }
                    else
                    {
                        sumaCostosReferencias += Convert.ToDecimal(referencia_valor, miCultura);

                        referencias_inven buscarReferenciasInvenOrigen = context.referencias_inven.FirstOrDefault(x =>
                            x.codigo == referencia_codigo && x.ano == DateTime.Now.Year &&
                            x.mes == DateTime.Now.Month && x.bodega == modelo.BodegaOrigen);
                        if (buscarReferenciasInvenOrigen != null)
                        {
                            buscarReferenciasInvenOrigen.cos_sal =
                                (!string.IsNullOrEmpty(buscarReferenciasInvenOrigen.cos_sal.ToString())
                                    ? Convert.ToDecimal(buscarReferenciasInvenOrigen.cos_sal, miCultura)
                                    : 0) + (!string.IsNullOrEmpty(referencia_valor)
                                    ? Convert.ToDecimal(referencia_valor, miCultura)
                                    : 0);
                            buscarReferenciasInvenOrigen.baj_cos =
                                (!string.IsNullOrEmpty(buscarReferenciasInvenOrigen.baj_cos.ToString())
                                    ? Convert.ToDecimal(buscarReferenciasInvenOrigen.baj_cos, miCultura)
                                    : 0) + (!string.IsNullOrEmpty(referencia_valor)
                                    ? Convert.ToDecimal(referencia_valor, miCultura)
                                    : 0);
                            context.Entry(buscarReferenciasInvenOrigen).State = EntityState.Modified;
                        }
                        else
                        {
                            referencias_inven crearReferencia = new referencias_inven
                            {
                                bodega = modelo.BodegaOrigen,
                                codigo = referencia_codigo,
                                ano = (short)DateTime.Now.Year,
                                mes = (short)DateTime.Now.Month,
                                can_ini = 0,
                                cos_sal = !string.IsNullOrEmpty(referencia_valor)
                                    ? Convert.ToDecimal(referencia_valor, miCultura)
                                    : 0,
                                baj_cos = !string.IsNullOrEmpty(referencia_valor)
                                    ? Convert.ToDecimal(referencia_valor, miCultura)
                                    : 0,
                                modulo = "R"
                            };
                            context.referencias_inven.Add(crearReferencia);
                        }


                        lineas_documento crearLineasOrigen = new lineas_documento
                        {
                            codigo = referencia_codigo,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            nit = nitTraslado,
                            bodega = modelo.BodegaOrigen,
                            seq = sequ,
                            estado = true,
                            fec = DateTime.Now,
                            costo_unitario = !string.IsNullOrEmpty(referencia_valor)
                                ? Convert.ToDecimal(referencia_valor, miCultura)
                                : 0,
                            id_encabezado = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0
                        };
                        context.lineas_documento.Add(crearLineasOrigen);
                        sequ++;
                    }
                }

                int secuencia = 1;
                foreach (var parametro in parametrosCuentasVerificar)
                {
                    cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
                    if (buscarCuenta != null)
                    {
                        if (parametro.id_nombre_parametro == 9 || parametro.id_nombre_parametro == 12)
                        {
                            mov_contable movNuevo = new mov_contable
                            {
                                id_encab =
                                buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0,
                                fec_creacion = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                idparametronombre = parametro.id_nombre_parametro,
                                cuenta = parametro.cuenta,
                                centro = parametro.centro,
                                nit = nitTraslado,
                                fec = DateTime.Now,
                                seq = secuencia
                            };

                            // La validacion se hace ya que cuando el parametro es 9 (es decir inventario) se invierte el credito con debito
                            if (parametro.id_nombre_parametro == 9)
                            {
                                if (buscarCuenta.concepniff == 1)
                                {
                                    //if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                    //{
                                    //movNuevo.credito = sumaCostosReferencias;
                                    //movNuevo.creditoniif = sumaCostosReferencias;
                                    //}
                                    //if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                    //{
                                    movNuevo.debitoniif = sumaCostosReferencias;
                                    movNuevo.debito = sumaCostosReferencias;
                                    //}
                                }

                                if (buscarCuenta.concepniff == 4)
                                {
                                    //if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                    //{
                                    //    movNuevo.creditoniif = sumaCostosReferencias;
                                    //}
                                    //if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                    //{
                                    movNuevo.debitoniif = sumaCostosReferencias;
                                }
                                //}

                                if (buscarCuenta.concepniff == 5)
                                {
                                    //if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                    //{
                                    //    movNuevo.credito = sumaCostosReferencias;
                                    //}
                                    //if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                    //{
                                    movNuevo.debito = sumaCostosReferencias;
                                }
                                //}
                            }
                            else
                            {
                                if (buscarCuenta.concepniff == 1)
                                {
                                    //if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                    //{
                                    movNuevo.debitoniif = sumaCostosReferencias;
                                    movNuevo.debito = sumaCostosReferencias;
                                    //}
                                    //if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                    //{
                                    //    movNuevo.credito = sumaCostosReferencias;
                                    //    movNuevo.creditoniif = sumaCostosReferencias;
                                    //}
                                }

                                if (buscarCuenta.concepniff == 4)
                                {
                                    //if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                    //{
                                    movNuevo.debitoniif = sumaCostosReferencias;
                                }
                                //}
                                //if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                //{
                                //    movNuevo.creditoniif = sumaCostosReferencias;
                                //}

                                if (buscarCuenta.concepniff == 5)
                                {
                                    //if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                    //{
                                    movNuevo.debito = sumaCostosReferencias;
                                }
                                //}
                                //if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                //{
                                //    movNuevo.credito = sumaCostosReferencias;
                                //}
                            }

                            if (buscarCuenta.manejabase)
                            {
                                movNuevo.basecontable = sumaCostosReferencias;
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
                                movNuevo.nit = idTerceroCero;
                            }

                            movNuevo.detalle = "Baja costo de referencias";
                            secuencia++;

                            DateTime fechaHoy = DateTime.Now;
                            cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                x.centro == parametro.centro && x.cuenta == parametro.cuenta && x.nit == movNuevo.nit &&
                                x.ano == fechaHoy.Year && x.mes == fechaHoy.Month);

                            if (buscar_cuentas_valores != null)
                            {
                                //buscar_cuentas_valores.ano = fechaHoy.Year;
                                //buscar_cuentas_valores.mes = fechaHoy.Month;
                                //buscar_cuentas_valores.cuenta = movNuevo.cuenta;
                                //buscar_cuentas_valores.centro = movNuevo.centro;
                                //buscar_cuentas_valores.nit = movNuevo.nit;
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

                            context.mov_contable.Add(movNuevo);
                        }
                    }
                }

                try
                {
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

                        ViewBag.numDocumentoCreado = numeroConsecutivo;
                        TempData["mensaje"] =
                            "El cambio de costo de la(s) referencia(s) se ha realizado correctamente con numero " +
                            numeroConsecutivo;
                    }
                }
                catch (Exception ex)
                {
                    Exception sss = ex.InnerException;
                }
            }

            ListasDesplegables();
            BuscarFavoritos(menu);
            return View();
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

            EntradaSalidaModel modelo = new EntradaSalidaModel
            {
                TipoDocumento = encabezado.tipo,
                BodegaOrigen = encabezado.bodega,
                PerfilContable = encabezado.perfilcontable ?? 0,
                encabezado_id = encabezado.idencabezado,
                Notas = encabezado.notas
            };

            ViewBag.documentoSeleccionado = encabezado.tipo;
            ViewBag.bodegaSeleccionada = encabezado.bodega;
            ViewBag.perfilSeleccionado = encabezado.perfilcontable;

            icb_sysparameter buscarTipoDoc = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P45");
            int id_tipo_doc = buscarTipoDoc != null ? Convert.ToInt32(buscarTipoDoc.syspar_value) : 0;

            ViewBag.tp_doc_registros =
                context.tp_doc_registros.Where(x => x.tipo == id_tipo_doc && x.tipo == 13).ToList();
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public ActionResult bajarCosto(int? id)
        {
            var costoBajada = (from doc in context.encab_documento
                               join bodegacs in context.bodega_concesionario
                                   on doc.bodega equals bodegacs.id
                               join t in context.tp_doc_registros
                                   on doc.tipo equals t.tpdoc_id
                               where doc.idencabezado == id
                               select new
                               {
                                   fecha = doc.fecha.ToString(),
                                   numeroRegistro = doc.numero,
                                   nombreBodegas = bodegacs.bodccs_nombre,
                                   doc.notas,
                                   tipoEntrada = t.tpdoc_nombre,
                                   TotalTotales = 0
                               }).FirstOrDefault();

            string root = Server.MapPath("~/Pdf/");
            string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
            string path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path);

            modificarCosto modeloCostoBajada = new modificarCosto
            {
                tipoEntrada = costoBajada.tipoEntrada,
                numeroRegistro = costoBajada.numeroRegistro,
                fechaRegistro = costoBajada.fecha,
                nombreBodega = costoBajada.nombreBodegas,


                referencias = (from doc in context.encab_documento
                               join l in context.lineas_documento
                                   on doc.idencabezado equals l.id_encabezado
                               join r in context.icb_referencia
                                   on l.codigo equals r.ref_codigo
                               where doc.idencabezado == id
                               select new referencias
                               {
                                   codigo = l.codigo,
                                   descripcion = r.ref_descripcion,
                                   secuencia = l.seq,
                                   costoModificacionBajada = l.costo_unitario
                               }).ToList()
            };

            modeloCostoBajada.referencias.ForEach(item =>
                item.costoModificacionBajada = Math.Truncate(item.costoModificacionBajada));


            decimal totales = modeloCostoBajada.referencias.Sum(d => d.costoModificacionBajada);
            modeloCostoBajada.TotalTotales = Convert.ToDecimal(totales, miCultura);
            modeloCostoBajada.TotalTotales = Math.Truncate(totales);


            ViewAsPdf something = new ViewAsPdf("bajarCosto", modeloCostoBajada);
            return something;
        }


        public JsonResult BuscarBajaCostoReferencias()
        {
            icb_sysparameter tipoDocSalidasRepuestos = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P46");
            int id_tipo_documento = tipoDocSalidasRepuestos != null
                ? Convert.ToInt32(tipoDocSalidasRepuestos.syspar_value)
                : 0;

            var buscarTraslados = (from encabezado in context.encab_documento
                                   join origen in context.bodega_concesionario
                                       on encabezado.bodega equals origen.id into ori
                                   from origen in ori.DefaultIfEmpty()
                                   where encabezado.tipo == id_tipo_documento
                                   select new
                                   {
                                       encabezado.idencabezado,
                                       encabezado.numero,
                                       encabezado.fecha,
                                       origen = origen.bodccs_nombre,
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
                x.referencias
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public void ListasDesplegables()
        {
            icb_sysparameter buscarTipoDoc = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P45");
            int id_tipo_doc = buscarTipoDoc != null ? Convert.ToInt32(buscarTipoDoc.syspar_value) : 13;

            ViewBag.tp_doc_registros = context.tp_doc_registros.Where(x => x.tipo == id_tipo_doc).ToList();

            //var buscarReferencias = context.icb_referencia.Where(x => x.modulo == "R").ToList();
            //List<SelectListItem> items = new List<SelectListItem>();
            //foreach (var item in buscarReferencias)
            //{
            //    var nombre = "(" + item.ref_codigo + ") " + item.ref_descripcion;
            //    items.Add(new SelectListItem() { Text = nombre, Value = item.ref_codigo });
            //}
            //ViewBag.Referencia = new SelectList(items, "Value", "Text");
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