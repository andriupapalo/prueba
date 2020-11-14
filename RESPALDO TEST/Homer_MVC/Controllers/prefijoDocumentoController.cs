using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Homer_MVC.ViewModels.medios;

namespace Homer_MVC.Controllers
{
    public class prefijoDocumentoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public void ParametrosVista()
        {
            System.Collections.Generic.List<menu_busqueda> parametrosVista = context.menu_busqueda.Where(x => x.menu_busqueda_id_menu == 265).ToList();
            ViewBag.parametrosBusqueda = parametrosVista;
            IQueryable<icb_modulo_enlaces> enlacesBuscar = context.icb_modulo_enlaces.Where(x => x.enl_modulo == 265);
            string enlaces = "";
            foreach (icb_modulo_enlaces item in enlacesBuscar)
            {
                Menus buscarEnlace = context.Menus.FirstOrDefault(x => x.idMenu == item.id_modulo_destino);
                enlaces += "<li><a href='" + buscarEnlace.url + "'>" + buscarEnlace.nombreMenu + "</a></li>";
            }

            ViewBag.nombreEnlaces = enlaces;
        }

        // GET: tipoModelo
        public ActionResult Crear(int? menu)
        {
            TpDocRegistroModel crearTpDocRegistros = new TpDocRegistroModel
            {
                baseretencion = Convert.ToInt32(0),
                baseica = Convert.ToInt32(0),
                baseiva = Convert.ToInt32(0),
                tpdoc_estado = true,
                tpdocrazoninactivo = "No aplica",
                interno=false,
            };
            ParametrosVista();
            ViewBag.sw = new SelectList(context.tp_doc_sw.OrderBy(x => x.Descripcion), "tpdoc_id", "Descripcion");
            ViewBag.tipo = new SelectList(context.tp_doc_registros_tipo.OrderBy(x => x.descripcion), "id",
                "descripcion");
            ViewBag.doc_interno_asociado = new SelectList(context.tp_doc_registros.Take(0), "tpdoc_id", "tpdoc_nombre");
            ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
            BuscarFavoritos(menu);
            return View(crearTpDocRegistros);
        }

        // POST: col_vh/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(TpDocRegistroModel tipoDoc, int? menu)
        {
            //var bodegasSeleccionadas = Request["bodccs_cod"];
            if (ModelState.IsValid)
            {
                //if (string.IsNullOrEmpty(bodegasSeleccionadas))
                //{
                //    TempData["mensaje_error"] = "Debe asignar minimo una bodega!";
                //    ViewBag.sw = new SelectList(context.tp_doc_sw, "tpdoc_id", "Descripcion", tipo.sw);
                //    ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
                //    ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                //    return View(tipo);
                //}

                //var numeroSiguiente = Request["numeroConsecutivo"] != "" ? Convert.ToInt32(Request["numeroConsecutivo"]) : 0;
                //consulta si el registro esta en BD
                int nom = (from a in context.tp_doc_registros
                           where a.prefijo == tipoDoc.prefijo || a.tpdoc_id == tipoDoc.tpdoc_id
                           select a.tpdoc_nombre).Count();

                if (nom == 0)
                {
                    var tipoDoc2 = new tp_doc_registros
                    {
                        aplicaniff=tipoDoc.aplicaniff,
                        baseica=tipoDoc.baseica,
                        baseiva= tipoDoc.baseiva,
                        baseret1=tipoDoc.baseret1,
                        baseret2=tipoDoc.baseret2,
                        baseretencion=tipoDoc.baseretencion,
                        bodega=tipoDoc.bodega,
                        concepto1=tipoDoc.concepto1,
                        concepto2=tipoDoc.concepto2,
                        consecano=tipoDoc.consecano,
                        consecmes=tipoDoc.consecmes,
                        prefijo=tipoDoc.prefijo,
                        ret1=tipoDoc.ret1,
                        ret2=tipoDoc.ret2,
                        retencion=tipoDoc.retencion,
                        retica=tipoDoc.retica,
                        retiva=tipoDoc.retiva,
                        sw=tipoDoc.sw,
                        texto1=tipoDoc.texto1,
                        texto2=tipoDoc.texto2,
                        texto3=tipoDoc.texto3,
                        texto4=tipoDoc.texto4,
                        tipo=tipoDoc.tipo,
                        tpdocid_licencia=tipoDoc.tpdocid_licencia,
                        tpdocrazoninactivo=tipoDoc.tpdocrazoninactivo,
                        tpdoc_estado=tipoDoc.tpdoc_estado,
                        tpdoc_nombre=tipoDoc.tpdoc_nombre,                       
                    };
                    if (tipoDoc.interno == true)
                    {
                        tipoDoc2.interno = true;
                        tipoDoc2.doc_interno_asociado = tipoDoc.doc_interno_asociado;
                        tipoDoc2.entrada_salida = tipoDoc.entrada_salida;
                    }
                    else
                    {
                        tipoDoc2.interno = false;
                    }
                    tipoDoc2.tpdocfec_creacion = DateTime.Now;
                    tipoDoc2.tpdocuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.tp_doc_registros.Add(tipoDoc2);
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        tp_doc_registros ultimoPrefijo = context.tp_doc_registros.Where(x => x.tpdoc_id==tipoDoc2.tpdoc_id)
                            .FirstOrDefault();
                        // Primero se agregan los conceptos en caso de que existan
                        int conceptos1 = Convert.ToInt32(Request["numeroConcepto1"]);
                        int conceptos2 = Convert.ToInt32(Request["numeroConcepto2"]);

                        for (int i = 1; i <= conceptos1; i++)
                        {
                            string concepto = Request["conceptoUno" + i];
                            if (!string.IsNullOrEmpty(concepto))
                            {
                                context.tpdocconceptos.Add(new tpdocconceptos
                                {
                                    tipodocid = ultimoPrefijo.tpdoc_id,
                                    Descripcion = concepto,
                                    fec_creacion = DateTime.Now,
                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                                });
                            }
                        }

                        for (int i = 1; i <= conceptos2; i++)
                        {
                            string concepto = Request["conceptoDos" + i];
                            if (!string.IsNullOrEmpty(concepto))
                            {
                                context.tpdocconceptos2.Add(new tpdocconceptos2
                                {
                                    tipodocid = ultimoPrefijo.tpdoc_id,
                                    Descripcion = concepto,
                                    fec_creacion = DateTime.Now,
                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                                });
                            }
                        }

                        //Se agregan los datos del usuario, una vez agregado se agregan las bodegas de ese usuario en la tabla bodega_usuario
                        //if (!string.IsNullOrEmpty(bodegasSeleccionadas))
                        //{
                        //    var grupo = context.grupoconsecutivos.OrderByDescending(x => x.id).FirstOrDefault();
                        //    var numeroGrupo = grupo != null ? grupo.grupo + 1 : 1;
                        //    string[] bodegasId = bodegasSeleccionadas.Split(',');
                        //    foreach (var substring in bodegasId)
                        //    {
                        //        context.icb_doc_consecutivos.Add(new icb_doc_consecutivos
                        //        {
                        //            doccons_bodega = Convert.ToInt32(substring),
                        //            doccons_idtpdoc = ultimoPrefijo.tpdoc_id,
                        //            doccons_siguiente = numeroSiguiente,
                        //            doccons_feccreacion = DateTime.Now,
                        //            doccons_usucreacion = Convert.ToInt32(Session["user_usuarioid"])
                        //        });
                        //        context.grupoconsecutivos.Add(new grupoconsecutivos
                        //        {
                        //            bodega_id = Convert.ToInt32(substring),
                        //            documento_id = ultimoPrefijo.tpdoc_id,
                        //            grupo = numeroGrupo
                        //        });
                        //    }
                        //    var guardarBodegas = context.SaveChanges();
                        //}

                        context.SaveChanges();

                        TempData["mensaje"] = "El registro del nuevo prefijo de documento fue exitoso!";
                        return RedirectToAction("Crear", new { id = tipoDoc.tpdoc_id, menu });
                    }
                }
                else
                {
                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                }
            }

            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";
            ParametrosVista();
            ViewBag.sw = new SelectList(context.tp_doc_sw.OrderBy(x => x.Descripcion), "tpdoc_id", "Descripcion",
                tipoDoc.sw);
            ViewBag.tipo = new SelectList(context.tp_doc_registros_tipo.OrderBy(x => x.descripcion), "id",
                "descripcion");
            ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
            ViewBag.doc_interno_asociado = new SelectList(context.tp_doc_registros.Where(d=>d.interno==true && d.tpdoc_estado==true), "tpdoc_id", "tpdoc_nombre",tipoDoc.doc_interno_asociado);

            //ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
            BuscarFavoritos(menu);
            return View(tipoDoc);
        }

        public JsonResult traerDocumentosInternos(int? documento)
        {
            var predicado = PredicateBuilder.True<tp_doc_registros>();
            if (documento != null)
            {
                predicado = predicado.And(d => d.tpdoc_id != documento);
            }
            predicado = predicado.And(d => d.tpdoc_estado==true);
            predicado = predicado.And(d => d.interno==true);
            var documentos = context.tp_doc_registros.Where(d => d.tpdoc_estado == true && d.interno == true).Select(d => new {id=d.tpdoc_id,nombre=d.prefijo+" "+d.tpdoc_nombre }).ToList();
            return Json(documentos);

        }

        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tp_doc_registros tipo2 = context.tp_doc_registros.Find(id);
            if (tipo2 == null)
            {
                return HttpNotFound();
            }

            var tipo = new TpDocRegistroModel
            {
                aplicaniff = tipo2.aplicaniff,
                baseica = tipo2.baseica,
                baseiva = tipo2.baseiva,
                baseret1 = tipo2.baseret1,
                baseret2 = tipo2.baseret2,
                baseretencion = tipo2.baseretencion,
                bodega = tipo2.bodega,
                concepto1 = tipo2.concepto1,
                concepto2 = tipo2.concepto2,
                consecano = tipo2.consecano,
                consecmes = tipo2.consecmes,
                doc_interno_asociado = tipo2.doc_interno_asociado,
                entrada_salida = tipo2.entrada_salida!=null?tipo2.entrada_salida.Value:false,
                interno = tipo2.interno,
                prefijo = tipo2.prefijo,
                ret1 = tipo2.ret1,
                ret2 = tipo2.ret2,
                retencion = tipo2.retencion,
                retica = tipo2.retica,
                retiva = tipo2.retiva,
                sw = tipo2.sw,
                texto1 = tipo2.texto1,
                texto2 = tipo2.texto2,
                texto3=tipo2.texto3,
                texto4=tipo2.texto4,
                tipo=tipo2.tipo,
                tpdocfec_actualizacion=tipo2.tpdocfec_actualizacion,
                tpdocfec_creacion=tipo2.tpdocfec_creacion,
                tpdocid_licencia=tipo2.tpdocid_licencia,
                tpdocrazoninactivo=tipo2.tpdocrazoninactivo,
                tpdocuserid_actualizacion=tipo2.tpdocuserid_actualizacion,
                tpdocuserid_creacion=tipo2.tpdocuserid_creacion,
                tpdoc_estado=tipo2.tpdoc_estado,
                tpdoc_id=tipo2.tpdoc_id,
                tpdoc_nombre=tipo2.tpdoc_nombre,
            };
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tipo.tpdocuserid_creacion);

            ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users modificator = context.users.Find(tipo.tpdocuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }

            ParametrosVista();
            ViewBag.sw = new SelectList(context.tp_doc_sw.OrderBy(x => x.Descripcion), "tpdoc_id", "Descripcion",
                tipo.sw);
            ViewBag.tipo = new SelectList(context.tp_doc_registros_tipo.OrderBy(x => x.descripcion), "id",
                "descripcion");
            ViewBag.doc_interno_asociado = new SelectList(context.tp_doc_registros.Where(d => d.interno == true && d.tpdoc_estado == true && d.tpdoc_id!=tipo.tpdoc_id), "tpdoc_id", "tpdoc_nombre", tipo.doc_interno_asociado);

            //ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();

            //var buscarBodegas = from bodegas in context.icb_doc_consecutivos
            //                    where bodegas.doccons_idtpdoc == id
            //                    select new { bodegas.doccons_bodega };
            //var bodegasString = "";
            //var primera = true;
            //foreach (var item in buscarBodegas)
            //{
            //    if (primera)
            //    {
            //        bodegasString += item.doccons_bodega;
            //        primera = !primera;
            //    }
            //    else
            //    {
            //        bodegasString += "," + item.doccons_bodega;
            //    }
            //}
            //ViewBag.bodegasSeleccionadas = bodegasString;
            //var buscarConsecutivo =  context.icb_doc_consecutivos.FirstOrDefault(x=>x.doccons_idtpdoc==id);
            //ViewBag.numeroConsecutivo = buscarConsecutivo != null ? buscarConsecutivo.doccons_siguiente : 0;
            BuscarFavoritos(menu);
            return View(tipo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult update(TpDocRegistroModel tipoDoc, int? menu)
        {
            //var bodegasSeleccionadas = Request["bodccs_cod"];
            if (ModelState.IsValid)
            {
                //if (string.IsNullOrEmpty(bodegasSeleccionadas))
                //{
                //    TempData["mensaje_error"] = "Debe asignar minimo una bodega!";
                //    ViewBag.sw = new SelectList(context.tp_doc_sw, "tpdoc_id", "Descripcion", tipo.sw);
                //    ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
                //    ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                //    return View(tipo);
                //}

                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                int nom = (from a in context.tp_doc_registros
                           where a.prefijo == tipoDoc.prefijo && a.tpdoc_id == tipoDoc.tpdoc_id
                           select a.tpdoc_nombre).Count();

                if (nom == 1)
                {
                    //selecciono el documento
                    var docguardar = context.tp_doc_registros.Where(d => d.tpdoc_id == tipoDoc.tpdoc_id).FirstOrDefault();
                    docguardar.aplicaniff = tipoDoc.aplicaniff;
                    docguardar.baseica = tipoDoc.baseica;
                    docguardar.baseiva = tipoDoc.baseiva;
                    docguardar.baseret1 = tipoDoc.baseret1;
                    docguardar.baseret2 = tipoDoc.baseret2;
                    docguardar.baseretencion = tipoDoc.baseretencion;
                    docguardar.bodega = tipoDoc.bodega;
                    docguardar.concepto1 = tipoDoc.concepto1;
                    docguardar.concepto2 = tipoDoc.concepto2;
                    docguardar.consecano = tipoDoc.consecano;
                    docguardar.consecmes = tipoDoc.consecmes;
                    docguardar.doc_interno_asociado = tipoDoc.doc_interno_asociado;
                    docguardar.entrada_salida = tipoDoc.entrada_salida;
                    docguardar.interno = tipoDoc.interno;
                    docguardar.prefijo = tipoDoc.prefijo;
                    docguardar.ret1 = tipoDoc.ret1;
                    docguardar.ret2 = tipoDoc.ret2;
                    docguardar.retencion = tipoDoc.retencion;
                    docguardar.retica = tipoDoc.retica;
                    docguardar.retiva = tipoDoc.retiva;
                    docguardar.sw = tipoDoc.sw;
                    docguardar.texto1 = tipoDoc.texto1;
                    docguardar.texto2 = tipoDoc.texto2;
                    docguardar.texto3 = tipoDoc.texto3;
                    docguardar.texto4 = tipoDoc.texto4;
                    docguardar.tipo = tipoDoc.tipo;
                    docguardar.tpdocid_licencia = tipoDoc.tpdocid_licencia;
                    docguardar.tpdocrazoninactivo = tipoDoc.tpdocrazoninactivo;
                    docguardar.tpdoc_estado = tipoDoc.tpdoc_estado;
                    docguardar.tpdoc_nombre = tipoDoc.tpdoc_nombre;
                    docguardar.tpdocfec_actualizacion = DateTime.Now;
                    docguardar.tpdocuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(docguardar).State = EntityState.Modified;
                    context.SaveChanges();

                    // GuardarDatosConceptosYBodegas(tipoDoc/*,bodegasSeleccionadas*/);
                    bool result1 = GuardarDatosConceptosYBodegas(docguardar);
                    if (result1 == false)
                    {
                        TempData["mensaje_error"] =
                            "No es posible modificar el concepto por que ya tiene un movimiento asociado, por favor valide!";
                    }

                    ConsultaDatosCreacion(tipoDoc);
                    TempData["mensaje"] = "La actualización del tipo de documento fue exitoso!";
                    ViewBag.sw = new SelectList(context.tp_doc_sw, "tpdoc_id", "Descripcion", tipoDoc.sw);
                    ViewBag.tipo = new SelectList(context.tp_doc_registros_tipo, "id", "descripcion");
                    ViewBag.doc_interno_asociado = new SelectList(context.tp_doc_registros.Where(d => d.interno == true && d.tpdoc_estado == true && d.tpdoc_id != tipoDoc.tpdoc_id), "tpdoc_id", "tpdoc_nombre", tipoDoc.doc_interno_asociado);

                    //ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
                    //ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                    BuscarFavoritos(menu);
                    return View(tipoDoc);
                }

                {
                    int nom2 = (from a in context.tp_doc_registros
                                where a.prefijo == tipoDoc.prefijo
                                select a.tpdoc_nombre).Count();
                    if (nom2 == 0)
                    {
                        var docguardar2 = context.tp_doc_registros.Where(d => d.tpdoc_id == tipoDoc.tpdoc_id).FirstOrDefault();

                        docguardar2.tpdocfec_actualizacion = DateTime.Now;
                        docguardar2.tpdocuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        context.Entry(docguardar2).State = EntityState.Modified;
                        context.SaveChanges();

                        //GuardarDatosConceptosYBodegas(tipoDoc/*, bodegasSeleccionadas*/);
                        bool result2 = GuardarDatosConceptosYBodegas(docguardar2);
                        if (result2 == false)
                        {
                            TempData["mensaje_error"] =
                                "No es posible modificar el concepto por que ya tiene un movimiento asociado, por favor valide!";
                        }

                        ConsultaDatosCreacion(tipoDoc);
                        ViewBag.sw = new SelectList(context.tp_doc_sw, "tpdoc_id", "Descripcion", tipoDoc.sw);
                        ViewBag.tipo = new SelectList(context.tp_doc_registros_tipo, "id", "descripcion");
                        ViewBag.doc_interno_asociado = new SelectList(context.tp_doc_registros.Where(d => d.interno == true && d.tpdoc_estado == true && d.tpdoc_id != tipoDoc.tpdoc_id), "tpdoc_id", "tpdoc_nombre", tipoDoc.doc_interno_asociado);

                        //ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
                        //ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                        TempData["mensaje"] = "La actualización del tipo de documento fue exitoso!";
                        BuscarFavoritos(menu);
                        return View(tipoDoc);
                    }

                    TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                }
            }

            ConsultaDatosCreacion(tipoDoc);
            TempData["mensaje_vacio"] = "Campos vacios, por favor valide!";
            ViewBag.sw = new SelectList(context.tp_doc_sw, "tpdoc_id", "Descripcion", tipoDoc.sw);
            ViewBag.tipo = new SelectList(context.tp_doc_registros_tipo, "id", "descripcion");
            ViewBag.doc_interno_asociado = new SelectList(context.tp_doc_registros.Where(d => d.interno == true && d.tpdoc_estado == true && d.tpdoc_id != tipoDoc.tpdoc_id), "tpdoc_id", "tpdoc_nombre", tipoDoc.doc_interno_asociado);

            //ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
            //ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
            BuscarFavoritos(menu);
            return View(tipoDoc);
        }

        public JsonResult BuscarConceptosJson(int? id)
        {
            var conceptosActualesUno = context.tpdocconceptos.Where(x => x.tipodocid == id).Select(x => new
            {
                x.Descripcion,
                x.id
            }).ToList();
            var conceptosActualesDos = context.tpdocconceptos2.Where(x => x.tipodocid == id).Select(x => new
            {
                x.Descripcion,
                x.id
            }).ToList();
            var data = new
            {
                conceptosActualesUno,
                conceptosActualesDos
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public bool GuardarDatosConceptosYBodegas(tp_doc_registros tipo /*, string bodegasSeleccionadas*/)
        {
            // Primero se agregan los conceptos en caso de que existan
            int conceptos1 = Convert.ToInt32(Request["numeroConcepto1"]);
            int conceptos2 = Convert.ToInt32(Request["numeroConcepto2"]);

            //const string query = "DELETE FROM [dbo].[tpdocconceptos] WHERE [tipodocid]={0}"; //se comentarea la sentencia por que ya no se va a borrar la tabla 
            //var rows = context.Database.ExecuteSqlCommand(query, tipo.tpdoc_id);//se comentarea la sentencia por que ya no se va a borrar la tabla 
            try
            {
                for (int j = 1; j <= conceptos1; j++)
                {
                    int idConcepto = Convert.ToInt32(Request["idconceptoUno" + j]);
                    string concepto = Request["conceptoUno" + j];
                    int existe = context.encab_documento.Where(x => x.concepto == idConcepto).Count();
                    if (existe == 0)
                    {
                        tpdocconceptos coincidencia = context.tpdocconceptos.FirstOrDefault(x =>
                            x.tipodocid == tipo.tpdoc_id && x.Descripcion == concepto);
                        if (coincidencia == null)
                        {
                            context.tpdocconceptos.Add(new tpdocconceptos
                            {
                                tipodocid = tipo.tpdoc_id,
                                Descripcion = concepto,
                                fec_creacion = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                            });
                        }

                        context.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {
                return false;
                throw;
            }

            //const string query2 = "DELETE FROM [dbo].[tpdocconceptos2] WHERE [tipodocid]={0}";
            //var rows2 = context.Database.ExecuteSqlCommand(query2, tipo.tpdoc_id);
            try
            {
                for (int j = 1; j <= conceptos2; j++)
                {
                    int idConcepto = Convert.ToInt32(Request["idconceptoDos" + j]);
                    string concepto = Request["conceptoDos" + j];
                    int existe = context.encab_documento.Where(x => x.concepto == idConcepto).Count();
                    if (existe == 0)
                    {
                        tpdocconceptos coincidencia = context.tpdocconceptos.FirstOrDefault(x =>
                            x.tipodocid == tipo.tpdoc_id && x.Descripcion == concepto);
                        if (coincidencia == null)
                        {
                            context.tpdocconceptos2.Add(new tpdocconceptos2
                            {
                                tipodocid = tipo.tpdoc_id,
                                Descripcion = concepto,
                                fec_creacion = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                            });
                        }

                        context.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {
                return false;
                throw;
            }

            return true;

            #region Codigo comentado

            //Se agregan los datos del usuario, una vez agregado se agregan las bodegas de ese usuario en la tabla bodega_usuario
            //var numeroSiguiente = Request["numeroConsecutivo"] != "" ? Convert.ToInt32(Request["numeroConsecutivo"]) : 0;
            //if (!string.IsNullOrEmpty(bodegasSeleccionadas))
            //{
            //    const string query3 = "DELETE FROM [dbo].[icb_doc_consecutivos] WHERE [doccons_idtpdoc]={0}";
            //    var rows3 = context.Database.ExecuteSqlCommand(query3, tipo.tpdoc_id);
            //    var grupo = context.grupoconsecutivos.OrderByDescending(x => x.grupo).FirstOrDefault();
            //    var numeroGrupo = grupo != null ? grupo.grupo + 1 : 1;
            //    const string query4 = "DELETE FROM [dbo].[grupoconsecutivos] WHERE [documento_id]={0}";
            //    var rows4 = context.Database.ExecuteSqlCommand(query4, tipo.tpdoc_id);
            //    string[] bodegasId = bodegasSeleccionadas.Split(',');
            //    foreach (var substring in bodegasId)
            //    {
            //        context.icb_doc_consecutivos.Add(new icb_doc_consecutivos
            //        {
            //            doccons_bodega = Convert.ToInt32(substring),
            //            doccons_idtpdoc = tipo.tpdoc_id,
            //            doccons_siguiente = numeroSiguiente,
            //            doccons_feccreacion = DateTime.Now,
            //            doccons_usucreacion = Convert.ToInt32(Session["user_usuarioid"])
            //        });
            //        context.grupoconsecutivos.Add(new grupoconsecutivos
            //        {
            //            bodega_id = Convert.ToInt32(substring),
            //            documento_id = tipo.tpdoc_id,
            //            grupo = numeroGrupo
            //        });
            //    }
            //    var guardarBodegas = context.SaveChanges();
            //}

            #endregion
        }

        public JsonResult EliminarConceptos(int id)
        {
            int existe = context.encab_documento.Where(x => x.concepto == id).Count();
            if (existe == 0)
            {
                tpdocconceptos dato = context.tpdocconceptos.Find(id);
                context.Entry(dato).State = EntityState.Deleted;
                context.SaveChanges();
                return Json(new { exito = true }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { exito = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EliminarConceptos2(int id)
        {
            int existe = context.encab_documento.Where(x => x.concepto == id).Count();
            if (existe == 0)
            {
                tpdocconceptos2 dato = context.tpdocconceptos2.Find(id);
                context.Entry(dato).State = EntityState.Deleted;
                context.SaveChanges();
                return Json(new { exito = true }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { exito = false }, JsonRequestBehavior.AllowGet);
        }

        public void ConsultaDatosCreacion(TpDocRegistroModel tipo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(tipo.tpdocuserid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(tipo.tpdocuserid_actualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult GetTiposJson()
        {
            var buscarTipos = context.tp_doc_registros.ToList().Select(x => new
            {
                x.tpdoc_id,
                x.tpdoc_nombre,
                x.prefijo,
                tpdoc_estado = x.tpdoc_estado ? "Activo" : "Inactivo"
            });
            return Json(buscarTipos, JsonRequestBehavior.AllowGet);
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