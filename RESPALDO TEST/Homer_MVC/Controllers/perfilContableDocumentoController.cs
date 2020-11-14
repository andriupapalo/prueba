using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class perfilContableDocumentoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: perfilContableDocumento
        public ActionResult Create(int? menu)
        {
            ViewBag.tp_doc_registros = context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();
            ViewBag.paramcontablenombres =
                new SelectList(
                    context.paramcontablenombres.OrderBy(x => x.descripcion_parametro).Where(x => x.id != 19), "id",
                    "descripcion_parametro");
            ViewBag.cuentas_puc = context.cuenta_puc.OrderBy(x => x.cntpuc_descp).Where(x => x.esafectable).ToList();
            ViewBag.vconceptocompra = context.vconceptocompra.OrderBy(x => x.descripcion).ToList();
            ViewBag.centroCosto = context.centro_costo.OrderBy(x => x.centcst_nombre).ToList();
            ViewBag.iva = new SelectList(context.contributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            //ViewBag.bodega= new SelectList(context.contributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            ViewBag.regimen_tercero = new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre),
                "tpregimen_id", "tpregimen_nombre");
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Create(perfil_contable_documento modelo, int? menu)
        {
            string bodegasSeleccionadas = Request["bodccs_cod"];
            string tipoConcepto = Request["tipoConceptoCompra"];
            if (ModelState.IsValid)
            {
                if (modelo.compraautomatica)
                {
                    if (string.IsNullOrEmpty(tipoConcepto))
                    {
                        TempData["mensaje_error"] = "Si la compra es automatica, debe asignar minimo un tipo!";
                        ViewBag.tp_doc_registros = context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();
                        ViewBag.paramcontablenombres =
                            new SelectList(context.paramcontablenombres.OrderBy(x => x.descripcion_parametro), "id",
                                "descripcion_parametro");
                        ViewBag.cuentas_puc = context.cuenta_puc.OrderBy(x => x.cntpuc_descp).Where(x => x.esafectable)
                            .ToList();
                        ViewBag.centroCosto = context.centro_costo.OrderBy(x => x.centcst_nombre).ToList();
                        ViewBag.iva = new SelectList(context.contributario.OrderBy(x => x.descripcion), "codigo",
                            "descripcion");
                        ViewBag.regimen_tercero =
                            new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre), "tpregimen_id",
                                "tpregimen_nombre");
                        ViewBag.vconceptocompra = context.vconceptocompra.OrderBy(x => x.descripcion).ToList();
                        ViewBag.tipoDocumento = modelo.tipo;
                        ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                        BuscarFavoritos(menu);
                        return View(modelo);
                    }
                }
                else
                {
                    tipoConcepto = "";
                }

                perfil_contable_documento buscarPorCodigo =
                    context.perfil_contable_documento.FirstOrDefault(x =>
                        x.codigo == modelo.codigo && x.tipo == modelo.tipo);
                if (buscarPorCodigo != null)
                {
                    TempData["mensaje_error"] =
                        "El codigo ingresado ya se encuentra registrado para ese tipo de documento!";
                }
                else
                {
                    if (string.IsNullOrEmpty(bodegasSeleccionadas))
                    {
                        TempData["mensaje_error"] = "Debe asignar minimo una bodega!";
                        ViewBag.tp_doc_registros = context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();
                        ViewBag.paramcontablenombres =
                            new SelectList(context.paramcontablenombres.OrderBy(x => x.descripcion_parametro), "id",
                                "descripcion_parametro");
                        ViewBag.cuentas_puc = context.cuenta_puc.OrderBy(x => x.cntpuc_descp).Where(x => x.esafectable)
                            .ToList();
                        ViewBag.centroCosto = context.centro_costo.OrderBy(x => x.centcst_nombre).ToList();
                        ViewBag.iva = new SelectList(context.contributario.OrderBy(x => x.descripcion), "codigo",
                            "descripcion");
                        ViewBag.regimen_tercero =
                            new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre), "tpregimen_id",
                                "tpregimen_nombre");
                        ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                        ViewBag.tipoDocumento = modelo.tipo;
                        BuscarFavoritos(menu);
                        return View(modelo);
                    }

                    // Primero se crea el registro en la tabla perfilcontabledocumento para luego asignar las bodegas a ese tipo de documento
                    context.perfil_contable_documento.Add(new perfil_contable_documento
                    {
                        tipo = modelo.tipo,
                        codigo = modelo.codigo,
                        descripcion = modelo.descripcion,
                        compraautomatica = modelo.compraautomatica,
                        iva = modelo.iva,
                        regimen_tercero = modelo.regimen_tercero
                    });
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        perfil_contable_documento buscarUltimoPerfil = context.perfil_contable_documento.OrderByDescending(x => x.id)
                            .FirstOrDefault();
                        if (buscarUltimoPerfil != null)
                        {
                            // Aqui se empieza a agregar el ultimo perfil contable asignado a una o varias bodegas

                            // Se busca el centro de costo cero, por si en caso que una cuenta no requiera centro, se asigna el centro cero por defecto.
                            centro_costo buscarCentroCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                            int idCentroCero = 0;
                            if (buscarCentroCero == null)
                            {
                                //idCentroCero = null;
                            }
                            else
                            {
                                idCentroCero = buscarCentroCero.centcst_id;
                            }

                            string[] bodegasId = bodegasSeleccionadas.Split(',');
                            foreach (string substring in bodegasId)
                            {
                                context.perfil_contable_bodega.Add(new perfil_contable_bodega
                                {
                                    idperfil = buscarUltimoPerfil.id,
                                    idbodega = Convert.ToInt32(substring)
                                });
                                if (!string.IsNullOrEmpty(tipoConcepto))
                                {
                                    string[] tipoConceptoId = tipoConcepto.Split(',');
                                    foreach (string item in tipoConceptoId)
                                    {
                                        context.vtipo_compra_vehiculos.Add(new vtipo_compra_vehiculos
                                        {
                                            idperfilcontable = buscarUltimoPerfil.id,
                                            idbodega = Convert.ToInt32(substring),
                                            idtipocomprav = Convert.ToInt32(item),
                                            iddoc = modelo.tipo
                                        });
                                    }
                                }
                            }

                            // Por ultimo se agrega el perfil cuentas documento
                            int parametros = Convert.ToInt32(Request["lista_parametros"]);
                            for (int i = 1; i <= parametros; i++)
                            {
                                string parametro = Request["parametro" + i];
                                string cuenta = Request["cuenta" + i];
                                string centroCosto = Request["centroCosto" + i];
                                if (!string.IsNullOrEmpty(parametro) && !string.IsNullOrEmpty(cuenta))
                                {
                                    if (string.IsNullOrEmpty(centroCosto))
                                    {
                                        context.perfil_cuentas_documento.Add(new perfil_cuentas_documento
                                        {
                                            id_perfil = buscarUltimoPerfil.id,
                                            id_nombre_parametro = Convert.ToInt32(parametro),
                                            cuenta = Convert.ToInt32(cuenta),
                                            centro = idCentroCero
                                        });
                                    }
                                    else
                                    {
                                        context.perfil_cuentas_documento.Add(new perfil_cuentas_documento
                                        {
                                            id_perfil = buscarUltimoPerfil.id,
                                            id_nombre_parametro = Convert.ToInt32(parametro),
                                            cuenta = Convert.ToInt32(cuenta),
                                            centro = Convert.ToInt32(centroCosto)
                                        });
                                    }
                                }
                            }

                            // Se guarda las bodegas y los parametros asignadso
                            int guardarBodYParametros = context.SaveChanges();
                            if (guardarBodYParametros > 0)
                            {
                                TempData["mensaje"] = "El perfil contable se ha guardado con exito";
                                ViewBag.tp_doc_registros =
                                    context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();
                                ViewBag.paramcontablenombres = new SelectList(
                                    context.paramcontablenombres.OrderBy(x => x.descripcion_parametro), "id",
                                    "descripcion_parametro");
                                ViewBag.cuentas_puc = context.cuenta_puc.OrderBy(x => x.cntpuc_descp)
                                    .Where(x => x.esafectable).ToList();
                                ViewBag.vconceptocompra = context.vconceptocompra.OrderBy(x => x.descripcion).ToList();
                                ViewBag.centroCosto = context.centro_costo.OrderBy(x => x.centcst_nombre).ToList();
                                ViewBag.iva = new SelectList(context.contributario.OrderBy(x => x.descripcion),
                                    "codigo", "descripcion");
                                ViewBag.regimen_tercero =
                                    new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre),
                                        "tpregimen_id", "tpregimen_nombre");
                                BuscarFavoritos(menu);
                                return View();
                            }
                        }
                    }
                }
            }

            ViewBag.tp_doc_registros = context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();
            ViewBag.paramcontablenombres =
                new SelectList(context.paramcontablenombres.OrderBy(x => x.descripcion_parametro), "id",
                    "descripcion_parametro");
            ViewBag.cuentas_puc = context.cuenta_puc.OrderBy(x => x.cntpuc_descp).Where(x => x.esafectable).ToList();
            ViewBag.centroCosto = context.centro_costo.OrderBy(x => x.centcst_nombre).ToList();
            ViewBag.iva = new SelectList(context.contributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            ViewBag.regimen_tercero = new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre),
                "tpregimen_id", "tpregimen_nombre");
            ViewBag.vconceptocompra = context.vconceptocompra.OrderBy(x => x.descripcion).ToList();
            ViewBag.tipoDocumento = modelo.tipo;
            ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public ActionResult update(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            perfil_contable_documento perfilBuscado = context.perfil_contable_documento.FirstOrDefault(x => x.id == id);
            if (perfilBuscado == null)
            {
                return HttpNotFound();
            }

            System.Collections.Generic.List<perfil_contable_bodega> bodegas = context.perfil_contable_bodega.Where(x => x.idperfil == perfilBuscado.id).ToList();
            string bodegasString = "";
            bool primera = true;
            foreach (perfil_contable_bodega item in bodegas)
            {
                if (primera)
                {
                    bodegasString += item.idbodega;
                    primera = !primera;
                }
                else
                {
                    bodegasString += "," + item.idbodega;
                }
            }

            System.Collections.Generic.List<vtipo_compra_vehiculos> conceptosCompra = context.vtipo_compra_vehiculos.Where(x => x.idperfilcontable == perfilBuscado.id)
                .ToList();
            string conceptosCompraString = "";
            bool primerId = true;
            foreach (vtipo_compra_vehiculos item in conceptosCompra)
            {
                if (primerId)
                {
                    conceptosCompraString += item.idtipocomprav;
                    primerId = !primerId;
                }
                else
                {
                    conceptosCompraString += "," + item.idtipocomprav;
                }
            }

            ViewBag.tp_doc_registros = context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();
            ViewBag.paramcontablenombres =
                new SelectList(
                    context.paramcontablenombres.OrderBy(x => x.descripcion_parametro).Where(x => x.id != 19), "id",
                    "descripcion_parametro");
            ViewBag.cuentas_puc = context.cuenta_puc.OrderBy(x => x.cntpuc_descp).Where(x => x.esafectable).ToList();
            ViewBag.centroCosto = context.centro_costo.OrderBy(x => x.centcst_nombre).ToList();
            ViewBag.vconceptocompra = context.vconceptocompra.OrderBy(x => x.descripcion).ToList();
            ViewBag.bodegasSeleccionadas = bodegasString;
            ViewBag.conceptosCompraSeleccionadas = conceptosCompraString;
            ViewBag.iva = new SelectList(context.contributario.OrderBy(x => x.descripcion), "codigo", "descripcion",
                perfilBuscado.iva);
            ViewBag.regimen_tercero = new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre),
                "tpregimen_id", "tpregimen_nombre", perfilBuscado.regimen_tercero);
            ViewBag.tipoDocumento = perfilBuscado.tipo;

            BuscarFavoritos(menu);
            return View(perfilBuscado);
        }


        [HttpPost]
        public ActionResult update(perfil_contable_documento modelo, int? menu)
        {
            // Seleccion de bodegassdfsdf
            string bodegasSeleccionadas = Request["bodccs_cod"];
            string tipoConcepto = Request["tipoConceptoCompra"];
            if (ModelState.IsValid)
            {
                if (modelo.compraautomatica)
                {
                    if (string.IsNullOrEmpty(tipoConcepto))
                    {
                        TempData["mensaje_error"] = "Si la compra es automatica, debe asignar minimo un tipo!";
                        ViewBag.tp_doc_registros = context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();
                        ViewBag.paramcontablenombres =
                            new SelectList(context.paramcontablenombres.OrderBy(x => x.descripcion_parametro), "id",
                                "descripcion_parametro");
                        ViewBag.cuentas_puc = context.cuenta_puc.OrderBy(x => x.cntpuc_descp).Where(x => x.esafectable)
                            .ToList();
                        ViewBag.centroCosto = context.centro_costo.OrderBy(x => x.centcst_nombre).ToList();
                        ViewBag.vconceptocompra = context.vconceptocompra.OrderBy(x => x.descripcion).ToList();
                        ViewBag.tipoDocumento = modelo.tipo;
                        ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                        ViewBag.conceptosCompraSeleccionadas = tipoConcepto;
                        ViewBag.iva = new SelectList(context.contributario.OrderBy(x => x.descripcion), "codigo",
                            "descripcion");
                        //ViewBag.regimen_tercero = new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre), "tpregimen_id", "tpregimen_nombre");
                        ViewBag.regimen_tercero =
                            new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre), "tpregimen_id",
                                "tpregimen_nombre", modelo.regimen_tercero);


                        BuscarFavoritos(menu);
                        return View(modelo);
                    }
                }
                else
                {
                    tipoConcepto = "";
                }

                perfil_contable_documento buscarPorCodigo =
                    context.perfil_contable_documento.FirstOrDefault(x =>
                        x.tipo == modelo.tipo && x.codigo == modelo.codigo);
                int idActual = buscarPorCodigo != null ? buscarPorCodigo.id : 0;
                if (buscarPorCodigo != null)
                {
                    if (buscarPorCodigo.id != modelo.id)
                    {
                        TempData["mensaje_error"] = "El perfil contable por documento ya se encuentra registrado!";
                        ViewBag.tp_doc_registros = context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();
                        ViewBag.paramcontablenombres =
                            new SelectList(context.paramcontablenombres.OrderBy(x => x.descripcion_parametro), "id",
                                "descripcion_parametro");
                        ViewBag.cuentas_puc = context.cuenta_puc.OrderBy(x => x.cntpuc_descp).Where(x => x.esafectable)
                            .ToList();
                        ViewBag.centroCosto = context.centro_costo.OrderBy(x => x.centcst_nombre).ToList();
                        ViewBag.vconceptocompra = context.vconceptocompra.OrderBy(x => x.descripcion).ToList();
                        ViewBag.tipoDocumento = modelo.tipo;
                        ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                        ViewBag.conceptosCompraSeleccionadas = tipoConcepto;
                        ViewBag.iva = new SelectList(context.contributario.OrderBy(x => x.descripcion), "codigo",
                            "descripcion");
                        //ViewBag.regimen_tercero = new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre), "tpregimen_id", "tpregimen_nombre");
                        ViewBag.regimen_tercero =
                            new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre), "tpregimen_id",
                                "tpregimen_nombre", modelo.regimen_tercero);

                        BuscarFavoritos(menu);
                        return View(modelo);
                    }
                }

                //else {
                if (string.IsNullOrEmpty(bodegasSeleccionadas))
                {
                    TempData["mensaje_error"] = "Debe asignar minimo una bodega!";
                    ViewBag.tp_doc_registros = context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();
                    ViewBag.paramcontablenombres =
                        new SelectList(context.paramcontablenombres.OrderBy(x => x.descripcion_parametro), "id",
                            "descripcion_parametro");
                    ViewBag.cuentas_puc = context.cuenta_puc.OrderBy(x => x.cntpuc_descp).Where(x => x.esafectable)
                        .ToList();
                    ViewBag.centroCosto = context.centro_costo.OrderBy(x => x.centcst_nombre).ToList();
                    ViewBag.vconceptocompra = context.vconceptocompra.OrderBy(x => x.descripcion).ToList();
                    ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                    ViewBag.conceptosCompraSeleccionadas = tipoConcepto;
                    ViewBag.tipoDocumento = modelo.tipo;
                    ViewBag.iva = new SelectList(context.contributario.OrderBy(x => x.descripcion), "codigo",
                        "descripcion");
                    //ViewBag.regimen_tercero = new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre), "tpregimen_id", "tpregimen_nombre");
                    ViewBag.regimen_tercero = new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre),
                        "tpregimen_id", "tpregimen_nombre", modelo.regimen_tercero);


                    BuscarFavoritos(menu);
                    return View(modelo);
                }

                // Validacion para que no se repitan los campos tipo documento, bodega y tipo, ya que no puede haber un registro con estos valores repetidos simultaneamente
                string[] bodegasId = bodegasSeleccionadas.Split(',');
                bool errorPorTipoConcepto = false;
                foreach (string substring in bodegasId)
                {
                    if (!string.IsNullOrEmpty(tipoConcepto))
                    {
                        string[] tipoConceptoId = tipoConcepto.Split(',');
                        foreach (string item in tipoConceptoId)
                        {
                            int idBodega = Convert.ToInt32(substring);
                            int idTipoConcepto = Convert.ToInt32(item);
                            vtipo_compra_vehiculos buscarTipoCompra = context.vtipo_compra_vehiculos.FirstOrDefault(x =>
                                x.iddoc == modelo.tipo && x.idbodega == idBodega && x.idtipocomprav == idTipoConcepto);
                            if (buscarTipoCompra != null)
                            {
                                if (buscarTipoCompra.idperfilcontable != modelo.id)
                                {
                                    errorPorTipoConcepto = true;
                                }
                            }
                        }
                    }
                }

                if (errorPorTipoConcepto)
                {
                    TempData["mensaje_error"] =
                        "El tipo de documento ya esta registrado en alguna bodega y tipo de los seleccionados!";
                    ViewBag.tp_doc_registros = context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();
                    ViewBag.paramcontablenombres =
                        new SelectList(context.paramcontablenombres.OrderBy(x => x.descripcion_parametro), "id",
                            "descripcion_parametro");
                    ViewBag.cuentas_puc = context.cuenta_puc.OrderBy(x => x.cntpuc_descp).Where(x => x.esafectable)
                        .ToList();
                    ViewBag.centroCosto = context.centro_costo.OrderBy(x => x.centcst_nombre).ToList();
                    ViewBag.vconceptocompra = context.vconceptocompra.OrderBy(x => x.descripcion).ToList();
                    ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                    ViewBag.conceptosCompraSeleccionadas = tipoConcepto;
                    ViewBag.tipoDocumento = modelo.tipo;
                    ViewBag.iva = new SelectList(context.contributario.OrderBy(x => x.descripcion), "codigo",
                        "descripcion");
                    //ViewBag.regimen_tercero = new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre), "tpregimen_id", "tpregimen_nombre");
                    ViewBag.regimen_tercero = new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre),
                        "tpregimen_id", "tpregimen_nombre", modelo.regimen_tercero);


                    BuscarFavoritos(menu);
                    return View(modelo);
                }


                // Primero se actualiza el registro en la tabla perfilcontabledocumento para luego asignar las bodegas a ese tipo de documento
                perfil_contable_documento bucarPerfilId = context.perfil_contable_documento.FirstOrDefault(x => x.id == modelo.id);
                bucarPerfilId.tipo = modelo.tipo;
                bucarPerfilId.codigo = modelo.codigo;
                bucarPerfilId.descripcion = modelo.descripcion;
                bucarPerfilId.compraautomatica = modelo.compraautomatica;
                bucarPerfilId.iva = modelo.iva;
                bucarPerfilId.regimen_tercero = modelo.regimen_tercero;
                context.Entry(bucarPerfilId).State = EntityState.Modified;
                int guardar = context.SaveChanges();
                if (guardar > 0)
                {
                    // Aqui se empieza a actualizar el ultimo perfil contable asignado a una o varias bodegas

                    const string query = "DELETE FROM [dbo].[perfil_contable_bodega] WHERE [idperfil]={0}";
                    int rows = context.Database.ExecuteSqlCommand(query, modelo.id);

                    const string query3 = "DELETE FROM [dbo].[vtipo_compra_vehiculos] WHERE [idperfilcontable]={0}";
                    int rows3 = context.Database.ExecuteSqlCommand(query3, modelo.id);

                    // Se busca el centro de costo cero, por si en caso que una cuenta no requiera centro, se asigna el centro cero por defecto.
                    centro_costo buscarCentroCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                    int idCentroCero = 0;
                    if (buscarCentroCero == null)
                    {
                        //idCentroCero = null;
                    }
                    else
                    {
                        idCentroCero = buscarCentroCero.centcst_id;
                    }


                    foreach (string substring in bodegasId)
                    {
                        context.perfil_contable_bodega.Add(new perfil_contable_bodega
                        {
                            idperfil = modelo.id,
                            idbodega = Convert.ToInt32(substring)
                        });
                        // Agregar datos a la tabla VTIPO_COMPRA_VEHICULO
                        if (!string.IsNullOrEmpty(tipoConcepto))
                        {
                            string[] tipoConceptoId = tipoConcepto.Split(',');
                            foreach (string item in tipoConceptoId)
                            {
                                context.vtipo_compra_vehiculos.Add(new vtipo_compra_vehiculos
                                {
                                    idperfilcontable = modelo.id,
                                    idbodega = Convert.ToInt32(substring),
                                    idtipocomprav = Convert.ToInt32(item),
                                    iddoc = modelo.tipo
                                });
                            }
                        }
                    }

                    context.SaveChanges();

                    // Por ultimo se actualiza el perfil cuentas documento
                    const string query2 = "DELETE FROM [dbo].[perfil_cuentas_documento] WHERE [id_perfil]={0}";
                    int rows2 = context.Database.ExecuteSqlCommand(query2, modelo.id);
                    int parametros = Convert.ToInt32(Request["lista_parametros"]);
                    for (int i = 1; i <= parametros; i++)
                    {
                        string parametro = Request["parametro" + i];
                        string cuenta = Request["cuenta" + i];
                        string centroCosto = Request["centroCosto" + i];
                        if (!string.IsNullOrEmpty(parametro) && !string.IsNullOrEmpty(cuenta))
                        {
                            if (!string.IsNullOrEmpty(centroCosto))
                            {
                                context.perfil_cuentas_documento.Add(new perfil_cuentas_documento
                                {
                                    id_perfil = modelo.id,
                                    id_nombre_parametro = Convert.ToInt32(parametro),
                                    cuenta = Convert.ToInt32(cuenta),
                                    centro = Convert.ToInt32(centroCosto)
                                });
                            }
                            else
                            {
                                context.perfil_cuentas_documento.Add(new perfil_cuentas_documento
                                {
                                    id_perfil = modelo.id,
                                    id_nombre_parametro = Convert.ToInt32(parametro),
                                    cuenta = Convert.ToInt32(cuenta),
                                    centro = idCentroCero
                                });
                            }
                        }
                    }

                    // Se guarda las bodegas y los parametros asignadso
                    int guardarBodYParametros = context.SaveChanges();
                    if (guardarBodYParametros > 0)
                    {
                        TempData["mensaje"] = "El perfil contable se ha guardado con exito";
                        ViewBag.tp_doc_registros = context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();
                        ViewBag.paramcontablenombres =
                            new SelectList(context.paramcontablenombres.OrderBy(x => x.descripcion_parametro), "id",
                                "descripcion_parametro");
                        ViewBag.cuentas_puc = context.cuenta_puc.OrderBy(x => x.cntpuc_descp).Where(x => x.esafectable)
                            .ToList();
                        ViewBag.centroCosto = context.centro_costo.OrderBy(x => x.centcst_nombre).ToList();
                        ViewBag.vconceptocompra = context.vconceptocompra.OrderBy(x => x.descripcion).ToList();
                        ViewBag.tipoDocumento = modelo.tipo;
                        ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                        ViewBag.conceptosCompraSeleccionadas = tipoConcepto;
                        ViewBag.iva = new SelectList(context.contributario.OrderBy(x => x.descripcion), "codigo",
                            "descripcion");
                        //ViewBag.regimen_tercero = new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre), "tpregimen_id", "tpregimen_nombre");
                        ViewBag.regimen_tercero =
                            new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre), "tpregimen_id",
                                "tpregimen_nombre", modelo.regimen_tercero);

                        BuscarFavoritos(menu);
                        return View(modelo);
                    }
                }

                //}
            }

            ViewBag.tp_doc_registros = context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();
            ViewBag.paramcontablenombres =
                new SelectList(
                    context.paramcontablenombres.OrderBy(x => x.descripcion_parametro).Where(x => x.id != 19), "id",
                    "descripcion_parametro");
            ViewBag.cuentas_puc = context.cuenta_puc.OrderBy(x => x.cntpuc_descp).Where(x => x.esafectable).ToList();
            ViewBag.centroCosto = context.centro_costo.OrderBy(x => x.centcst_nombre).ToList();
            ViewBag.vconceptocompra = context.vconceptocompra.OrderBy(x => x.descripcion).ToList();
            ViewBag.tipoDocumento = modelo.tipo;
            ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
            ViewBag.conceptosCompraSeleccionadas = tipoConcepto;
            ViewBag.iva = new SelectList(context.contributario.OrderBy(x => x.descripcion), "codigo", "descripcion");
            //ViewBag.regimen_tercero = new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre), "tpregimen_id", "tpregimen_nombre");
            ViewBag.regimen_tercero = new SelectList(context.tpregimen_tercero.OrderBy(x => x.tpregimen_nombre),
                "tpregimen_id", "tpregimen_nombre", modelo.regimen_tercero);

            BuscarFavoritos(menu);
            return View(modelo);
        }


        public JsonResult BuscarParametrosJson(int id)
        {
            var buscarParametros = (from perfil in context.perfil_cuentas_documento
                                    join parametro in context.paramcontablenombres
                                        on perfil.id_nombre_parametro equals parametro.id
                                    join cuenta in context.cuenta_puc
                                        on perfil.cuenta equals cuenta.cntpuc_id
                                    join centro in context.centro_costo
                                        on perfil.centro equals centro.centcst_id into temp
                                    from j in temp.DefaultIfEmpty()
                                    where perfil.id_perfil == id
                                    select new
                                    {
                                        perfil.id,
                                        parametroId = parametro.id,
                                        parametro.descripcion_parametro,
                                        cuenta.cntpuc_id,
                                        cuenta.cntpuc_numero,
                                        cuenta.cntpuc_descp,
                                        centroCosto = context.centro_costo.FirstOrDefault(x => x.centcst_id == perfil.centro),
                                        //j.centcst_id,
                                        j.pre_centcst
                                    }).ToList();
            var data = buscarParametros.Select(x => new
            {
                x.id,
                x.parametroId,
                x.descripcion_parametro,
                x.cntpuc_id,
                x.cntpuc_numero,
                x.cntpuc_descp,
                centcst_id = x.centroCosto != null ? x.centroCosto.centcst_id.ToString() : "",
                centcst_nombre = x.centroCosto != null ? "(" + x.pre_centcst + ")" + x.centroCosto.centcst_nombre : ""
            });
            // "(" + x.cntpuc_numero + ")" + 
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonResult CuentaRequiereCentroCosto(int? id)
        {
            cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == id);
            if (buscarCuenta != null)
            {
                return Json(buscarCuenta.ccostos, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarBodegasPorDocumento(int? id)
        {
            var buscarBodegas = (from grupoDocumentos in context.grupoconsecutivos
                                 join bodegas in context.bodega_concesionario
                                     on grupoDocumentos.bodega_id equals bodegas.id
                                 where grupoDocumentos.documento_id == id
                                 select new
                                 {
                                     bodegas.id,
                                     bodegas.bodccs_nombre
                                 }).ToList();
            return Json(buscarBodegas, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarPerfilesDocPaginados()
        {
            var buscar = (from perfilDocumento in context.perfil_contable_documento
                          join tipoDocumento in context.tp_doc_registros
                              on perfilDocumento.tipo equals tipoDocumento.tpdoc_id
                          select new
                          {
                              tpdoc_nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                              perfilDocumento.codigo,
                              perfilDocumento.descripcion,
                              perfilDocumento.id
                          }).ToList();
            return Json(buscar, JsonRequestBehavior.AllowGet);
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