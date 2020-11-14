using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class chequePosfechadoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        // GET: chequePosfechado
        public ActionResult Create(int? menu)
        {
            ViewBag.banco = new SelectList(context.bancos, "id", "Descripcion");

            icb_sysparameter buscarParametroTpDoc = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P49");
            string idTipoDocPosfechados = buscarParametroTpDoc != null ? buscarParametroTpDoc.syspar_value : "0";
            int idDocPosfechados = Convert.ToInt32(idTipoDocPosfechados);
            ViewBag.idtipodoc = new SelectList(context.tp_doc_registros.Where(x => x.tipo == idDocPosfechados),
                "tpdoc_id", "tpdoc_nombre");
            ViewBag.perfilSeleccionado = 0;
            BuscarFavoritos(menu);
            return View();
        }


        // POST: chequePosfechado
        [HttpPost]
        public ActionResult Create(documentos_posfechados modelo, int? menu)
        {
            int idtipodoc = Convert.ToInt32(Request["idtipodoc"]);
            ViewBag.txtNitCliente = Request["txtNitCliente"];
            ViewBag.perfilSeleccionado = Convert.ToInt32(Request["idperfil"]);

            icb_sysparameter buscarParametroTpDoc = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P49");
            string idTipoDocPosfechados = buscarParametroTpDoc != null ? buscarParametroTpDoc.syspar_value : "0";
            int idDocPosfechados = Convert.ToInt32(idTipoDocPosfechados);

            if (ModelState.IsValid)
            {
                long numeroConsecutivo = 0;
                int anioActual = DateTime.Now.Year;
                int mesActual = DateTime.Now.Month;
                int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                icb_doc_consecutivos numeroConsecutivoAux = context.icb_doc_consecutivos.OrderByDescending(x => x.doccons_ano)
                    .FirstOrDefault(x => x.doccons_idtpdoc == idtipodoc && x.doccons_bodega == bodegaActual);
                grupoconsecutivos grupoConsecutivo =
                    context.grupoconsecutivos.FirstOrDefault(x =>
                        x.documento_id == idtipodoc && x.bodega_id == bodegaActual);
                if (numeroConsecutivoAux != null)
                {
                    if (numeroConsecutivoAux.doccons_requiere_mes)
                    {
                        if (numeroConsecutivoAux.doccons_mes != mesActual)
                        {
                            // Requiere mes pero no hay consecutivo para el anio actual
                            TempData["mensaje_error"] =
                                "No existe un numero consecutivo asignado para este tipo de documento en el mes actual que es requerido";
                            ViewBag.banco = new SelectList(context.bancos, "id", "Descripcion", modelo.banco);

                            ViewBag.idtipodoc =
                                new SelectList(context.tp_doc_registros.Where(x => x.tipo == idDocPosfechados),
                                    "tpdoc_id", "tpdoc_nombre", idtipodoc);
                            TempData["modelo_invalido"] = "modelo_invalido";
                            BuscarFavoritos(menu);
                            return View(modelo);
                        }

                        //requiereAnio = true;
                        numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                    }
                    else if (numeroConsecutivoAux.doccons_requiere_anio)
                    {
                        if (numeroConsecutivoAux.doccons_ano != anioActual)
                        {
                            // Requiere anio pero no hay consecutivo para el anio actual
                            TempData["mensaje_error"] =
                                "No existe un numero consecutivo asignado para este tipo de documento en el año actual que es requerido";
                            ViewBag.banco = new SelectList(context.bancos, "id", "Descripcion", modelo.banco);
                            ViewBag.idtipodoc =
                                new SelectList(context.tp_doc_registros.Where(x => x.tipo == idDocPosfechados),
                                    "tpdoc_id", "tpdoc_nombre", idtipodoc);
                            TempData["modelo_invalido"] = "modelo_invalido";
                            BuscarFavoritos(menu);
                            return View(modelo);
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
                    ViewBag.banco = new SelectList(context.bancos, "id", "Descripcion", modelo.banco);
                    ViewBag.idtipodoc = new SelectList(context.tp_doc_registros.Where(x => x.tipo == idDocPosfechados),
                        "tpdoc_id", "tpdoc_nombre", idtipodoc);
                    TempData["modelo_invalido"] = "modelo_invalido";
                    BuscarFavoritos(menu);
                    return View(modelo);
                }

                // Si llega hasta aqui significa que si existe un numero consecutivo para el documento en la bodega actual.

                var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                                  join nombreParametro in context.paramcontablenombres
                                                      on perfil.id_nombre_parametro equals nombreParametro.id
                                                  join cuenta in context.cuenta_puc
                                                      on perfil.cuenta equals cuenta.cntpuc_id
                                                  where perfil.id_perfil == modelo.idperfil
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
                decimal calcularDebito = 0;
                decimal calcularCredito = 0;

                foreach (var parametro in parametrosCuentasVerificar)
                {
                    //var tipoParametro = 0;

                    cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
                    if (buscarCuenta != null)
                    {
                        if (parametro.id_nombre_parametro == 15)
                        {
                            if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                            {
                                calcularDebito += modelo.Valor;
                                listaDescuadrados.Add(new DocumentoDescuadradoModel
                                {
                                    NumeroCuenta = parametro.cntpuc_numero,
                                    DescripcionParametro = parametro.descripcion_parametro,
                                    ValorDebito = modelo.Valor,
                                    ValorCredito = 0
                                });
                            }

                            if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                            {
                                calcularCredito += modelo.Valor;
                                listaDescuadrados.Add(new DocumentoDescuadradoModel
                                {
                                    NumeroCuenta = parametro.cntpuc_numero,
                                    DescripcionParametro = parametro.descripcion_parametro,
                                    ValorDebito = 0,
                                    ValorCredito = modelo.Valor
                                });
                            }
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
                            TempData["documento_descuadrado"] =
                                "El documento no tiene los movimientos calculados correctamente";
                            ViewBag.documentoDescuadrado = listaDescuadrados;
                            ViewBag.calculoDebito = calcularDebito;
                            ViewBag.calculoCredito = calcularCredito;
                            ViewBag.banco = new SelectList(context.bancos, "id", "Descripcion", modelo.banco);
                            ViewBag.idtipodoc =
                                new SelectList(context.tp_doc_registros.Where(x => x.tipo == idDocPosfechados),
                                    "tpdoc_id", "tpdoc_nombre", idtipodoc);
                            TempData["modelo_invalido"] = "modelo_invalido";
                            BuscarFavoritos(menu);
                            return View(modelo);
                        }
                    }
                }

                if (calcularCredito != calcularDebito)
                {
                    TempData["documento_descuadrado"] =
                        "El documento no tiene los movimientos calculados correctamente";
                    ViewBag.documentoDescuadrado = listaDescuadrados;
                    ViewBag.calculoDebito = calcularDebito;
                    ViewBag.calculoCredito = calcularCredito;
                    ViewBag.banco = new SelectList(context.bancos, "id", "Descripcion", modelo.banco);
                    ViewBag.idtipodoc = new SelectList(context.tp_doc_registros.Where(x => x.tipo == idDocPosfechados),
                        "tpdoc_id", "tpdoc_nombre", idtipodoc);
                    TempData["modelo_invalido"] = "modelo_invalido";
                    BuscarFavoritos(menu);
                    return View(modelo);
                }
                // Fin de la validacion para el calculo del debito y credito del movimiento contable

                encab_documento crearEncabezado = new encab_documento
                {
                    tipo = idtipodoc,
                    bodega = bodegaActual,
                    nit = modelo.idtercero,
                    numero = numeroConsecutivo,
                    fecha = DateTime.Now,
                    fec_creacion = DateTime.Now,
                    valor_total = modelo.Valor,
                    impoconsumo = 0
                };
                context.encab_documento.Add(crearEncabezado);
                int save = context.SaveChanges();

                if (save > 0)
                {
                    encab_documento buscarUltimoEncabezado =
                        context.encab_documento.OrderByDescending(x => x.idencabezado).FirstOrDefault();


                    modelo.idencabdoc = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0;
                    context.documentos_posfechados.Add(modelo);

                    List<perfil_cuentas_documento> parametrosCuentas = context.perfil_cuentas_documento.Where(x => x.id_perfil == modelo.idperfil)
                        .ToList();
                    centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                    int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
                    icb_terceros terceroValorCero = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
                    int idTerceroCero = centroValorCero != null ? Convert.ToInt32(terceroValorCero.tercero_id) : 0;

                    int secuencia = 1;
                    foreach (perfil_cuentas_documento parametro in parametrosCuentas)
                    {
                        decimal valorCredito = 0;
                        decimal valorDebito = 0;
                        cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
                        if (buscarCuenta != null)
                        {
                            if (parametro.id_nombre_parametro == 15)
                            {
                                valorCredito = modelo.Valor;
                                valorDebito = modelo.Valor;
                            }

                            mov_contable movNuevo = new mov_contable
                            {
                                id_encab =
                                buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0,
                                fec_creacion = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                idparametronombre = parametro.id_nombre_parametro,
                                cuenta = parametro.cuenta,
                                centro = parametro.centro,
                                fec = DateTime.Now,
                                seq = secuencia
                            };

                            if (buscarCuenta.concepniff == 1)
                            {
                                if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                {
                                    movNuevo.debitoniif = valorDebito;
                                    movNuevo.debito = valorDebito;
                                }

                                if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                {
                                    movNuevo.credito = valorCredito;
                                    movNuevo.creditoniif = valorCredito;
                                }
                            }

                            if (buscarCuenta.concepniff == 4)
                            {
                                if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                {
                                    movNuevo.debitoniif = valorDebito;
                                }

                                if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                {
                                    movNuevo.creditoniif = valorCredito;
                                }
                            }

                            if (buscarCuenta.concepniff == 5)
                            {
                                if (buscarCuenta.mov_cnt.ToUpper().Contains("DEBITO"))
                                {
                                    movNuevo.debito = valorDebito;
                                }

                                if (buscarCuenta.mov_cnt.ToUpper().Contains("CREDITO"))
                                {
                                    movNuevo.credito = valorCredito;
                                }
                            }

                            if (buscarCuenta.manejabase)
                            {
                                movNuevo.basecontable = modelo.Valor;
                            }

                            if (buscarCuenta.tercero)
                            {
                                movNuevo.nit = modelo.idtercero;
                            }

                            if (buscarCuenta.documeto)
                            {
                                movNuevo.documento = buscarUltimoEncabezado.numero.ToString();
                            }

                            movNuevo.detalle = "Cheque posfechado " + buscarUltimoEncabezado.documento;
                            secuencia++;


                            cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                x.centro == parametro.centro && x.cuenta == parametro.cuenta && x.nit == movNuevo.nit);
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

                            context.mov_contable.Add(movNuevo);
                        }
                    }

                    bool guardarCuenta = context.SaveChanges() > 0;
                    if (guardarCuenta)
                    {
                        int grupoId = grupoConsecutivo != null ? grupoConsecutivo.grupo : 0;
                        List<icb_doc_consecutivos> gruposConsecutivos = context.icb_doc_consecutivos
                            .Where(x => x.doccons_grupoconsecutivo == grupoId).ToList();
                        foreach (icb_doc_consecutivos grupo in gruposConsecutivos)
                        {
                            grupo.doccons_siguiente = grupo.doccons_siguiente + 1;
                            context.Entry(grupo).State = EntityState.Modified;
                        }

                        context.SaveChanges();
                        TempData["mensaje"] = "La creacion del cheque posfechado se ha guardado exitosamente.";
                        ViewBag.banco = new SelectList(context.bancos, "id", "Descripcion", modelo.banco);
                        ViewBag.idtipodoc =
                            new SelectList(context.tp_doc_registros.Where(x => x.tipo == idDocPosfechados), "tpdoc_id",
                                "tpdoc_nombre", idtipodoc);
                        return RedirectToAction("Create", "chequePosfechado", new { menu });
                    }
                }
            }

            ViewBag.banco = new SelectList(context.bancos, "id", "Descripcion", modelo.banco);
            ViewBag.idtipodoc = new SelectList(context.tp_doc_registros.Where(x => x.tipo == idDocPosfechados),
                "tpdoc_id", "tpdoc_nombre", idtipodoc);
            TempData["modelo_invalido"] = "modelo_invalido";
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            documentos_posfechados documento = context.documentos_posfechados.Find(id);
            if (documento == null)
            {
                return HttpNotFound();
            }

            var buscarEncabezado = (from documentoAux in context.documentos_posfechados
                                    join encabezado in context.encab_documento
                                        on documentoAux.idencabdoc equals encabezado.idencabezado
                                    join tercero in context.icb_terceros
                                        on documentoAux.idtercero equals tercero.tercero_id
                                    where documentoAux.id == id
                                    select new
                                    {
                                        encabezado.tipo,
                                        tercero.doc_tercero
                                    }).FirstOrDefault();
            ViewBag.banco = new SelectList(context.bancos, "id", "Descripcion", documento.banco);
            ViewBag.idtipodoc =
                new SelectList(context.tp_doc_registros, "tpdoc_id", "tpdoc_nombre", buscarEncabezado.tipo);
            ViewBag.txtNitCliente = buscarEncabezado.doc_tercero;
            ViewBag.perfilSeleccionado = documento.idperfil;
            TempData["modelo_invalido"] = "modelo_invalido";
            BuscarFavoritos(menu);
            return View(documento);
        }


        [HttpPost]
        public ActionResult Edit(documentos_posfechados modelo, int? menu)
        {
            int idtipodoc = Convert.ToInt32(Request["idtipodoc"]);
            ViewBag.txtNitCliente = Request["txtNitCliente"];
            ViewBag.perfilSeleccionado = Convert.ToInt32(Request["idperfil"]);
            if (ModelState.IsValid)
            {
                context.Entry(modelo).State = EntityState.Modified;
                int guardar = context.SaveChanges();
                if (guardar > 0)
                {
                    TempData["mensaje"] = "La actualizacion del cheque posfechado se ha guardado exitosamente.";
                }
                else
                {
                    TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                }
            }

            var buscarEncabezado = (from documentoAux in context.documentos_posfechados
                                    join encabezado in context.encab_documento
                                        on documentoAux.idencabdoc equals encabezado.idencabezado
                                    join tercero in context.icb_terceros
                                        on documentoAux.idtercero equals tercero.tercero_id
                                    where documentoAux.id == modelo.id
                                    select new
                                    {
                                        encabezado.tipo,
                                        tercero.doc_tercero
                                    }).FirstOrDefault();
            ViewBag.banco = new SelectList(context.bancos, "id", "Descripcion", modelo.banco);
            ViewBag.idtipodoc =
                new SelectList(context.tp_doc_registros, "tpdoc_id", "tpdoc_nombre", buscarEncabezado.tipo);
            ViewBag.perfilSeleccionado = modelo.idperfil;
            TempData["modelo_invalido"] = "modelo_invalido";
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public JsonResult BuscarDocumentoTercero(string documento)
        {
            var buscarTercero = (from tercero in context.icb_terceros
                                 where tercero.doc_tercero == documento
                                 select new
                                 {
                                     tercero.tercero_id,
                                     tercero.tpdoc_id,
                                     nombre = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " + tercero.apellido_tercero +
                                              " " + tercero.segapellido_tercero,
                                     tercero.razon_social,
                                     tercero.celular_tercero,
                                     tercero.email_tercero
                                 }).FirstOrDefault();

            if (buscarTercero != null)
            {
                bool cedula = buscarTercero.tpdoc_id == 2 ? true : false;
                bool nit = buscarTercero.tpdoc_id == 1 ? true : false;
                return Json(new { encontrado = true, buscarTercero, tieneCedula = cedula, tieneNit = nit },
                    JsonRequestBehavior.AllowGet);
            }

            return Json(new { encontrado = false }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarPerfilesPorDocumento(int? id_tipo_documento)
        {
            var buscarPerfiles = context.perfil_contable_documento.Where(x => x.tipo == id_tipo_documento).Select(x =>
                new
                {
                    x.id,
                    x.descripcion
                }).ToList();

            return Json(buscarPerfiles, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarChequesPosfechados()
        {
            var buscarCheques = (from cheques in context.documentos_posfechados
                                 join tercero in context.icb_terceros
                                     on cheques.idtercero equals tercero.tercero_id
                                 join banco in context.bancos
                                     on cheques.banco equals banco.id
                                 select new
                                 {
                                     cheques.id,
                                     cheques.fecrecibido,
                                     nombreTercero = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                     tercero.apellido_tercero + " " + tercero.segapellido_tercero + " " +
                                                     tercero.razon_social,
                                     banco.Descripcion,
                                     cheques.numero_cheque
                                 }).ToList();

            var data = buscarCheques.Select(x => new
            {
                x.id,
                fecrecibido = x.fecrecibido.ToShortDateString(),
                x.nombreTercero,
                x.Descripcion,
                x.numero_cheque
            });

            return Json(data, JsonRequestBehavior.AllowGet);
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