using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class perfilTipoCompraController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

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
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Create(perfiltipocompra modelo, int? menu)
        {
            string bodegasSeleccionadas = Request["bodccs_cod"];
            if (ModelState.IsValid)
            {
                perfiltipocompra buscarPorCodigo = context.perfiltipocompra.FirstOrDefault(x => x.idTipo == modelo.idTipo);
                if (buscarPorCodigo != null)
                {
                    TempData["mensaje_error"] =
                        "El codigo ingresado ya se encuentra registrado para ese tipo de compra!";
                }
                else
                {
                    if (string.IsNullOrEmpty(bodegasSeleccionadas))
                    {
                        TempData["mensaje_error"] = "Debe asignar minimo una bodega!";
                        ViewBag.paramcontablenombres =
                            new SelectList(context.paramcontablenombres.OrderBy(x => x.descripcion_parametro), "id",
                                "descripcion_parametro");
                        ViewBag.cuentas_puc = context.cuenta_puc.OrderBy(x => x.cntpuc_descp).Where(x => x.esafectable)
                            .ToList();
                        ViewBag.centroCosto = context.centro_costo.OrderBy(x => x.centcst_nombre).ToList();
                        ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                        BuscarFavoritos(menu);
                        return View(modelo);
                    }

                    // Primero se crea el registro en la tabla perfiltipocompra para luego asignar las bodegas a ese tipo de documento
                    context.perfiltipocompra.Add(new perfiltipocompra
                    {
                        idTipo = modelo.idTipo,
                        descripcion = modelo.descripcion
                    });
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        perfiltipocompra buscarUltimoPerfil = context.perfiltipocompra.OrderByDescending(x => x.id).FirstOrDefault();
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
                                context.perfilbodegastipocompra.Add(new perfilbodegastipocompra
                                {
                                    idTipoCompra = buscarUltimoPerfil.idTipo, // cambio jm id a idTipo
                                    idBodega = Convert.ToInt32(substring)
                                });
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
                                        context.perfilcuentastipocompra.Add(new perfilcuentastipocompra
                                        {
                                            idTipoCompra = buscarUltimoPerfil.idTipo,
                                            idParametro = Convert.ToInt32(parametro),
                                            cuenta = Convert.ToInt32(cuenta),
                                            centro = idCentroCero
                                        });
                                    }
                                    else
                                    {
                                        context.perfilcuentastipocompra.Add(new perfilcuentastipocompra
                                        {
                                            idTipoCompra = buscarUltimoPerfil.idTipo,
                                            idParametro = Convert.ToInt32(parametro),
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
                                TempData["mensaje"] = "El perfil del tipo de compra se ha guardado con exito";
                                ViewBag.tp_doc_registros =
                                    context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();
                                ViewBag.paramcontablenombres = new SelectList(
                                    context.paramcontablenombres.OrderBy(x => x.descripcion_parametro), "id",
                                    "descripcion_parametro");
                                ViewBag.cuentas_puc = context.cuenta_puc.OrderBy(x => x.cntpuc_descp)
                                    .Where(x => x.esafectable).ToList();
                                ViewBag.vconceptocompra = context.vconceptocompra.OrderBy(x => x.descripcion).ToList();
                                ViewBag.centroCosto = context.centro_costo.OrderBy(x => x.centcst_nombre).ToList();
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
            ViewBag.vconceptocompra = context.vconceptocompra.OrderBy(x => x.descripcion).ToList();
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

            perfiltipocompra perfilBuscado = context.perfiltipocompra.FirstOrDefault(x => x.id == id);
            if (perfilBuscado == null)
            {
                return HttpNotFound();
            }

            System.Collections.Generic.List<perfilbodegastipocompra> bodegas = context.perfilbodegastipocompra.Where(x => x.idTipoCompra == perfilBuscado.id).ToList();
            string bodegasString = "";
            bool primera = true;
            foreach (perfilbodegastipocompra item in bodegas)
            {
                if (primera)
                {
                    bodegasString += item.idBodega;
                    primera = !primera;
                }
                else
                {
                    bodegasString += "," + item.idBodega;
                }
            }

            ViewBag.paramcontablenombres =
                new SelectList(
                    context.paramcontablenombres.OrderBy(x => x.descripcion_parametro).Where(x => x.id != 19), "id",
                    "descripcion_parametro");
            ViewBag.cuentas_puc = context.cuenta_puc.OrderBy(x => x.cntpuc_descp).Where(x => x.esafectable).ToList();
            ViewBag.centroCosto = context.centro_costo.OrderBy(x => x.centcst_nombre).ToList();
            ViewBag.vconceptocompra = context.vconceptocompra.OrderBy(x => x.descripcion).ToList();
            ViewBag.tipoCompra = perfilBuscado.idTipo;
            ViewBag.bodegasSeleccionadas = bodegasString;
            BuscarFavoritos(menu);
            return View(perfilBuscado);
        }

        [HttpPost]
        public ActionResult update(perfiltipocompra modelo, int? menu)
        {
            // Seleccion de bodegassdfsdf
            string bodegasSeleccionadas = Request["bodccs_cod"];
            string tipoConcepto = Request["tipoConceptoCompra"];
            if (ModelState.IsValid)
            {
                perfiltipocompra buscarPorCodigo = context.perfiltipocompra.FirstOrDefault(x => x.idTipo == modelo.idTipo);
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
                        ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                        ViewBag.conceptosCompraSeleccionadas = tipoConcepto;
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
                    BuscarFavoritos(menu);
                    return View(modelo);
                }

                string[] bodegasId = bodegasSeleccionadas.Split(',');
                // Primero se actualiza el registro en la tabla perfiltipocompra para luego asignar las bodegas a ese tipo de documento
                perfiltipocompra bucarPerfilId = context.perfiltipocompra.FirstOrDefault(x => x.id == modelo.id);
                bucarPerfilId.idTipo = modelo.idTipo;
                bucarPerfilId.descripcion = modelo.descripcion;
                context.Entry(bucarPerfilId).State = EntityState.Modified;
                int guardar = context.SaveChanges();
                if (guardar > 0)
                {
                    // Aqui se empieza a actualizar el ultimo perfil contable asignado a una o varias bodegas
                    const string query = "DELETE FROM [dbo].[perfilbodegastipocompra] WHERE [idTipoCompra]={0}";
                    int rows = context.Database.ExecuteSqlCommand(query, modelo.idTipo);

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
                        context.perfilbodegastipocompra.Add(new perfilbodegastipocompra
                        {
                            idTipoCompra = modelo.idTipo,
                            idBodega = Convert.ToInt32(substring)
                        });
                    }

                    context.SaveChanges();

                    // Por ultimo se actualiza el perfilcuentastipocompra
                    const string query2 = "DELETE FROM [dbo].[perfilcuentastipocompra] WHERE [idTipoCompra]={0}";
                    int rows2 = context.Database.ExecuteSqlCommand(query2, modelo.idTipo);
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
                                context.perfilcuentastipocompra.Add(new perfilcuentastipocompra
                                {
                                    idTipoCompra = modelo.idTipo, //tenia el id ya corregido
                                    idParametro = Convert.ToInt32(parametro),
                                    cuenta = Convert.ToInt32(cuenta),
                                    centro = Convert.ToInt32(centroCosto)
                                });
                            }
                            else
                            {
                                context.perfilcuentastipocompra.Add(new perfilcuentastipocompra
                                {
                                    idTipoCompra = modelo.idTipo, //tenia el id ya corregido
                                    idParametro = Convert.ToInt32(parametro),
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
                        TempData["mensaje"] = "El perfil del tipo de compra se ha guardado con exito";
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
            ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
            ViewBag.conceptosCompraSeleccionadas = tipoConcepto;
            BuscarFavoritos(menu);
            return View(modelo);
        }

        public JsonResult BuscarBodegas()
        {
            var data = (from b in context.bodega_concesionario
                        where b.bodccs_estado
                        orderby b.bodccs_nombre
                        select new
                        {
                            b.id,
                            b.bodccs_nombre
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarParametrosJson(int id)
        {
            var buscarParametros = (from perfil in context.perfilcuentastipocompra
                                    join parametro in context.paramcontablenombres
                                        on perfil.idParametro equals parametro.id
                                    join cuenta in context.cuenta_puc
                                        on perfil.cuenta equals cuenta.cntpuc_id
                                    join centro in context.centro_costo
                                        on perfil.centro equals centro.centcst_id into temp
                                    from j in temp.DefaultIfEmpty()
                                    where perfil.idTipoCompra == id
                                    select new
                                    {
                                        perfil.id,
                                        parametroId = parametro.id,
                                        parametro.descripcion_parametro,
                                        cuenta.cntpuc_id,
                                        cuenta.cntpuc_numero,
                                        cuenta.cntpuc_descp,
                                        centroCosto = context.centro_costo.FirstOrDefault(x => x.centcst_id == perfil.centro),
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

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarPerfilesPaginados()
        {
            var buscar = (from perfilDocumento in context.perfiltipocompra
                          join concepto in context.vconceptocompra
                              on perfilDocumento.idTipo equals concepto.id
                          select new
                          {
                              nombre = "(" + concepto.codigo + ") " + concepto.descripcion,
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