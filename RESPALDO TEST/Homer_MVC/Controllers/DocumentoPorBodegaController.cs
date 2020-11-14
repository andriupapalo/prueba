using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class DocumentoPorBodegaController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        // GET: DocumentoPorBodeda
        public ActionResult Create(int? menu)
        {
            ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
            ViewBag.doccons_idtpdoc = context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Create(icb_doc_consecutivos modelo, int? menu)
        {
            string bodegasSeleccionadas = Request["bodccs_cod"];
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(bodegasSeleccionadas))
                {
                    TempData["mensaje_error"] = "Debe asignar minimo una bodega!";
                    ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
                    ViewBag.doccons_idtpdoc = context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();
                    ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                    ViewBag.tipoDocumentoElegido = modelo.doccons_idtpdoc;
                    BuscarFavoritos(menu);
                    return View(modelo);
                }

                if (!string.IsNullOrEmpty(bodegasSeleccionadas))
                {
                    // Verifica que alguna bodega no se encuentre ya asignada a este tipo de documento                   
                    string[] bodegasId = bodegasSeleccionadas.Split(',');
                    string bodegasYaASignadas = "";
                    int numeroBodegasAsignadas = 0;
                    foreach (string substring in bodegasId)
                    {
                        int bodegaEntero = Convert.ToInt32(substring);
                        grupoconsecutivos buscar = context.grupoconsecutivos.FirstOrDefault(x =>
                            x.bodega_id == bodegaEntero && x.documento_id == modelo.doccons_idtpdoc);
                        if (buscar != null)
                        {
                            bodega_concesionario buscarBodega =
                                context.bodega_concesionario.FirstOrDefault(x => x.id == buscar.bodega_id);
                            bodegasYaASignadas += buscarBodega != null ? " , " + buscarBodega.bodccs_nombre : "";
                            numeroBodegasAsignadas++;
                        }
                    }

                    if (numeroBodegasAsignadas > 0)
                    {
                        TempData["mensaje_error"] = "La(s) siguientes bodega(s) ya esta asignada a este documento" +
                                                    bodegasYaASignadas;
                        ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
                        ViewBag.doccons_idtpdoc = context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();
                        ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                        ViewBag.tipoDocumentoElegido = modelo.doccons_idtpdoc;
                        BuscarFavoritos(menu);
                        return View(modelo);
                    }

                    grupoconsecutivos grupo = context.grupoconsecutivos.OrderByDescending(x => x.grupo).FirstOrDefault();
                    int numeroGrupo = grupo != null ? grupo.grupo + 1 : 1;
                    //string[] bodegasId = bodegasSeleccionadas.Split(',');
                    foreach (string substring in bodegasId)
                    {
                        context.icb_doc_consecutivos.Add(new icb_doc_consecutivos
                        {
                            doccons_bodega = Convert.ToInt32(substring),
                            doccons_idtpdoc = modelo.doccons_idtpdoc,
                            doccons_siguiente = modelo.doccons_siguiente,
                            doccons_feccreacion = DateTime.Now,
                            doccons_usucreacion = Convert.ToInt32(Session["user_usuarioid"]),
                            doccons_ano = modelo.doccons_ano,
                            doccons_requiere_anio = modelo.doccons_requiere_anio,
                            doccons_requiere_mes = modelo.doccons_requiere_mes,
                            doccons_mes = modelo.doccons_mes,
                            doccons_grupoconsecutivo = numeroGrupo
                        });

                        context.grupoconsecutivos.Add(new grupoconsecutivos
                        {
                            bodega_id = Convert.ToInt32(substring),
                            documento_id = modelo.doccons_idtpdoc,
                            grupo = numeroGrupo
                        });
                    }

                    int guardarBodegas = context.SaveChanges();
                    if (guardarBodegas > 0)
                    {
                        TempData["mensaje"] = "El registro del nuevo tipo de documento fue exitoso!";
                        return RedirectToAction("Create", new { menu });
                    }
                }
            }

            ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
            ViewBag.doccons_idtpdoc = context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();
            ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
            ViewBag.tipoDocumentoElegido = modelo.doccons_idtpdoc;
            BuscarFavoritos(menu);
            return View();
        }


        public ActionResult update(int? id, int? menu)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            List<grupoconsecutivos> grupo = context.grupoconsecutivos.Where(x => x.grupo == id).ToList();
            if (grupo.Count <= 0)
            {
                return HttpNotFound();
            }

            ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();

            string bodegasString = "";
            bool primera = true;
            foreach (grupoconsecutivos item in grupo)
            {
                if (primera)
                {
                    bodegasString += item.bodega_id;
                    primera = !primera;
                }
                else
                {
                    bodegasString += "," + item.bodega_id;
                }
            }

            ViewBag.bodegasSeleccionadas = bodegasString;
            ViewBag.grupo = grupo.FirstOrDefault().grupo;
            ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
            ViewBag.doccons_idtpdoc = context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();
            int tipoDocumento = grupo.FirstOrDefault().documento_id;
            int bodega = grupo.FirstOrDefault().bodega_id;
            var buscaConsecutivo = (from documento in context.tp_doc_registros
                                    join consecutivo in context.icb_doc_consecutivos
                                        on documento.tpdoc_id equals consecutivo.doccons_idtpdoc
                                    where consecutivo.doccons_idtpdoc == tipoDocumento
                                          && consecutivo.doccons_bodega == bodega
                                    select new
                                    {
                                        consecutivo.doccons_siguiente,
                                        consecutivo.doccons_idtpdoc,
                                        consecutivo.doccons_ano,
                                        consecutivo.doccons_requiere_anio,
                                        consecutivo.doccons_mes,
                                        consecutivo.doccons_requiere_mes
                                    }).FirstOrDefault();
            icb_doc_consecutivos modelo = new icb_doc_consecutivos
            {
                doccons_siguiente = buscaConsecutivo != null ? buscaConsecutivo.doccons_siguiente : 0,
                doccons_idtpdoc = buscaConsecutivo != null ? buscaConsecutivo.doccons_idtpdoc : 0,
                doccons_ano = buscaConsecutivo != null ? buscaConsecutivo.doccons_ano : 0,
                doccons_requiere_anio = buscaConsecutivo != null ? buscaConsecutivo.doccons_requiere_anio : false,
                doccons_mes = buscaConsecutivo != null ? buscaConsecutivo.doccons_mes : 0,
                doccons_requiere_mes = buscaConsecutivo != null ? buscaConsecutivo.doccons_requiere_mes : false
            };
            ViewBag.tipoDocumentoElegido = buscaConsecutivo != null ? buscaConsecutivo.doccons_idtpdoc : 0;
            ViewBag.doccons_mes = buscaConsecutivo != null ? buscaConsecutivo.doccons_mes : 0;
            BuscarFavoritos(menu);
            return View(modelo);
        }


        [HttpPost]
        public ActionResult update(icb_doc_consecutivos modelo, int? menu)
        {
            string bodegasSeleccionadas = Request["bodccs_cod"];
            int grupo = Convert.ToInt32(Request["grupo"]);
            ViewBag.doccons_mes = modelo.doccons_mes;
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(bodegasSeleccionadas))
                {
                    TempData["mensaje_error"] = "Debe asignar minimo una bodega!";
                    ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
                    ViewBag.doccons_idtpdoc = context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();
                    ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                    ViewBag.tipoDocumentoElegido = modelo.doccons_idtpdoc;
                    ViewBag.grupo = grupo;
                    BuscarFavoritos(menu);
                    return View(modelo);
                }


                if (!string.IsNullOrEmpty(bodegasSeleccionadas))
                {
                    // Verifica que alguna bodega no se encuentre ya asignada a este tipo de documento                   
                    string[] bodegasId = bodegasSeleccionadas.Split(',');
                    string bodegasYaASignadas = "";
                    int numeroBodegasAsignadas = 0;
                    List<grupoconsecutivos> bodegasGrupo = context.grupoconsecutivos.Where(x => x.grupo == grupo).ToList();
                    foreach (string substring in bodegasId)
                    {
                        int bodegaEntero = Convert.ToInt32(substring);
                        grupoconsecutivos buscar = context.grupoconsecutivos.FirstOrDefault(x =>
                            x.bodega_id == bodegaEntero && x.documento_id == modelo.doccons_idtpdoc);
                        if (buscar != null && buscar.grupo != grupo)
                        {
                            bodega_concesionario buscarBodega =
                                context.bodega_concesionario.FirstOrDefault(x => x.id == buscar.bodega_id);
                            bodegasYaASignadas += buscarBodega != null ? " , " + buscarBodega.bodccs_nombre : "";
                            numeroBodegasAsignadas++;
                        }
                    }

                    if (numeroBodegasAsignadas > 0)
                    {
                        TempData["mensaje_error"] = "La(s) siguientes bodega(s) ya esta asignada a este documento" +
                                                    bodegasYaASignadas;
                        ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
                        ViewBag.doccons_idtpdoc = context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();
                        ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
                        ViewBag.tipoDocumentoElegido = modelo.doccons_idtpdoc;
                        ViewBag.grupo = grupo;
                        BuscarFavoritos(menu);
                        return View(modelo);
                    }


                    foreach (grupoconsecutivos substring in bodegasGrupo)
                    {
                        const string query3 =
                            "DELETE FROM [dbo].[icb_doc_consecutivos] WHERE [doccons_idtpdoc]={0} AND [doccons_bodega]={1}";
                        int rows3 = context.Database.ExecuteSqlCommand(query3, modelo.doccons_idtpdoc,
                            substring.bodega_id);
                    }


                    //var Ultimogrupo = context.grupoconsecutivos.OrderByDescending(x => x.grupo).FirstOrDefault();
                    //var numeroGrupo = Ultimogrupo != null ? Ultimogrupo.grupo : 1;
                    const string query4 = "DELETE FROM [dbo].[grupoconsecutivos] WHERE [grupo]={0}";
                    int rows4 = context.Database.ExecuteSqlCommand(query4, grupo);


                    foreach (string substring in bodegasId)
                    {
                        context.icb_doc_consecutivos.Add(new icb_doc_consecutivos
                        {
                            doccons_bodega = Convert.ToInt32(substring),
                            doccons_idtpdoc = modelo.doccons_idtpdoc,
                            doccons_siguiente = modelo.doccons_siguiente,
                            doccons_feccreacion = DateTime.Now,
                            doccons_usucreacion = Convert.ToInt32(Session["user_usuarioid"]),
                            doccons_ano = modelo.doccons_ano,
                            doccons_requiere_anio = modelo.doccons_requiere_anio,
                            doccons_requiere_mes = modelo.doccons_requiere_mes,
                            doccons_mes = modelo.doccons_mes,
                            doccons_grupoconsecutivo = grupo
                        });

                        context.grupoconsecutivos.Add(new grupoconsecutivos
                        {
                            bodega_id = Convert.ToInt32(substring),
                            documento_id = modelo.doccons_idtpdoc,
                            grupo = grupo
                        });
                    }

                    int guardarBodegas = context.SaveChanges();
                    if (guardarBodegas > 0)
                    {
                        TempData["mensaje"] = "El registro se ha actualizado con exito";
                    }
                }
            }

            ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
            ViewBag.doccons_idtpdoc = context.tp_doc_registros.OrderBy(x => x.tpdoc_nombre).ToList();
            ViewBag.bodegasSeleccionadas = bodegasSeleccionadas;
            ViewBag.tipoDocumentoElegido = modelo.doccons_idtpdoc;
            ViewBag.grupo = grupo;
            BuscarFavoritos(menu);
            return View();
        }


        public JsonResult DocumentoAnioMesRequerido(int id_tipo_documento)
        {
            var buscarDocumento = (from documento in context.tp_doc_registros
                                   where documento.tpdoc_id == id_tipo_documento
                                   select new
                                   {
                                       documento.consecano,
                                       documento.consecmes
                                   }).FirstOrDefault();

            return Json(buscarDocumento, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarDocumentosPorBodega()
        {
            var buscarDocumentosPorGrupo = (from grupo in context.grupoconsecutivos
                                            join documento in context.tp_doc_registros
                                                on grupo.documento_id equals documento.tpdoc_id
                                            join bodega in context.bodega_concesionario
                                                on grupo.bodega_id equals bodega.id
                                            select new
                                            {
                                                grupo.grupo,
                                                documento.prefijo,
                                                documento.tpdoc_nombre,
                                                documento.tpdoc_id,
                                                bodega.bodccs_cod,
                                                bodega.bodccs_nombre
                                            }).OrderBy(x => x.grupo).ToList();

            List<DocumentoPorModelo> lista = new List<DocumentoPorModelo>();
            IEnumerable<int> grupos = buscarDocumentosPorGrupo.Select(x => x.grupo).Distinct();
            string bodegas = "";
            //bool cambio = false;
            //var grupoActual = 0;

            foreach (int grupo in grupos)
            {
                bodegas = "";
                string docNombreActual = "";
                string prefijoActual = "";
                int tpdocumento_id = 0;
                //cambio = false;
                //grupoActual = grupo;
                foreach (var item in buscarDocumentosPorGrupo)
                {
                    if (grupo == item.grupo)
                    {
                        docNombreActual = item.tpdoc_nombre;
                        prefijoActual = item.prefijo;
                        bodegas += "&nbsp;&nbsp;&nbsp;<strong>(" + item.bodccs_cod + ")</strong>&nbsp;" +
                                   item.bodccs_nombre + "&nbsp;&nbsp;&nbsp;";
                        tpdocumento_id = item.tpdoc_id;
                        //cambio = true;
                    }
                }
                //else {                     
                //    if (cambio) {

                //        break;
                //    }  
                //}

                lista.Add(new DocumentoPorModelo
                {
                    grupo = grupo,
                    bodccs_nombre = bodegas,
                    tpdoc_nombre = docNombreActual,
                    tpdoc_prefijo = prefijoActual,
                    tpdoc_id = tpdocumento_id
                });
            }

            //lista.Add(new DocumentoPorModelo() { grupo = grupoActual, bodccs_nombre = bodegas, tpdoc_nombre = docNombreActual, tpdoc_prefijo = prefijoActual });
            return Json(lista, JsonRequestBehavior.AllowGet);
        }

        public long BuscarConsecutivo(int grupo)
        {
            long consecutivo = -1;

            icb_doc_consecutivos doc_consecutivos =
                context.icb_doc_consecutivos.FirstOrDefault(x => x.doccons_grupoconsecutivo == grupo);
            if (doc_consecutivos == null)
            {
                TempData["mensaje_error"] = "No existe un consecutivo parametrizado para la bodega seleccionada";
            }
            else
            {
                // para año y mes
                if (doc_consecutivos.doccons_requiere_anio && doc_consecutivos.doccons_requiere_mes)
                {
                    doc_consecutivos = context.icb_doc_consecutivos.FirstOrDefault(x =>
                        x.doccons_grupoconsecutivo == grupo && x.doccons_ano == DateTime.Now.Year &&
                        x.doccons_mes == DateTime.Now.Month);
                    if (doc_consecutivos != null)
                    {
                        consecutivo = doc_consecutivos.doccons_siguiente;
                    }
                }
                //para solo año
                else if (doc_consecutivos.doccons_requiere_anio && doc_consecutivos.doccons_requiere_mes == false)
                {
                    doc_consecutivos = context.icb_doc_consecutivos.FirstOrDefault(x =>
                        x.doccons_grupoconsecutivo == grupo && x.doccons_ano == DateTime.Now.Year);
                    if (doc_consecutivos != null)
                    {
                        consecutivo = doc_consecutivos.doccons_siguiente;
                    }
                    else
                    {
                        TempData["mensaje_error"] =
                            "No existe un consecutivo parametrizado para la bodega seleccionada";
                    }
                }
                //para ninguno
                else
                {
                    consecutivo = doc_consecutivos.doccons_siguiente;
                }
            }

            return consecutivo;
        }

        public int ActualizarConsecutivo(int grupo, long consecutivo)
        {
            icb_doc_consecutivos doc_consecutivos =
                context.icb_doc_consecutivos.FirstOrDefault(x => x.doccons_grupoconsecutivo == grupo);

            // para año y mes
            if (doc_consecutivos.doccons_requiere_anio && doc_consecutivos.doccons_requiere_mes)
            {
                IQueryable<icb_doc_consecutivos> gconsecutivos = context.icb_doc_consecutivos.Where(x =>
                    x.doccons_grupoconsecutivo == grupo && x.doccons_ano == DateTime.Now.Year &&
                    x.doccons_mes == DateTime.Now.Month);
                foreach (icb_doc_consecutivos item in gconsecutivos)
                {
                    item.doccons_siguiente = consecutivo + 1;
                    context.Entry(item).State = EntityState.Modified;
                }
            }
            //para solo año
            else if (doc_consecutivos.doccons_requiere_anio && doc_consecutivos.doccons_requiere_mes == false)
            {
                IQueryable<icb_doc_consecutivos> gconsecutivos = context.icb_doc_consecutivos.Where(x =>
                    x.doccons_grupoconsecutivo == grupo && x.doccons_ano == DateTime.Now.Year);
                foreach (icb_doc_consecutivos item in gconsecutivos)
                {
                    item.doccons_siguiente = consecutivo + 1;
                    context.Entry(item).State = EntityState.Modified;
                }
            }
            //para ninguno
            else
            {
                IQueryable<icb_doc_consecutivos> gconsecutivos = context.icb_doc_consecutivos.Where(x => x.doccons_grupoconsecutivo == grupo);
                foreach (icb_doc_consecutivos item in gconsecutivos)
                {
                    item.doccons_siguiente = consecutivo + 1;
                    context.Entry(item).State = EntityState.Modified;
                }
            }

            int result = context.SaveChanges();
            return result;
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