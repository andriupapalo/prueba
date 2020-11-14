using Homer_MVC.IcebergModel;
using Homer_MVC.ModeloVehiculos;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class cargueBateriasController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: cargueBaterias
        public ActionResult Index(int? menu)
        {
            ViewBag.tomaBateria = context.icb_toma_baterias;
            ViewBag.bodegas = context.bodega_concesionario;
            BuscarFavoritos(menu);
            return View();
        }


        [HttpPost]
        public ActionResult Index(HttpPostedFileBase excelfile, int? menu)
        {
            int items = 0;
            string log = "";
            int itemsCorrectos = 0;
            int itemsFallidos = 0;
            string nombreArchivo = excelfile.FileName;
            bool agregarArchivo = false;
            int idUltimoArchTomBat = 0;

            int tomaBateriaId = 0;
            try
            {
                tomaBateriaId = Convert.ToInt32(Request["tomaBateriaExcel"]);
            }
            catch (FormatException)
            {
                //Validacion para cuando no se asigna una toma de bateria al importar el archivo excel
            }

            int codigoBodega = 0;
            try
            {
                codigoBodega = Convert.ToInt32(Request["bodegaExcel"]);
            }
            catch (FormatException)
            {
                //Validacion para cuando no se asigna una bodega al importar el archivo excel
            }


            if (excelfile == null || excelfile.ContentLength == 0)
            {
                TempData["mensajeCargue"] = "El archivo esta vacio o no es un archivo valido!";
                ViewBag.tomaBateria = context.icb_toma_baterias;
                ViewBag.bodegas = context.bodega_concesionario;
                BuscarFavoritos(menu);
                return View();
            }

            icb_arch_tombat buscaNombre = context.icb_arch_tombat.FirstOrDefault(x => x.arch_tombat_nombre == excelfile.FileName);

            if (buscaNombre != null)
            {
                TempData["mensajeCargue"] =
                    "El nombre del archivo ya se encuentra registrado, verifique que el archivo no ha sido cargado antes o cambie de nombre!";
                ViewBag.tomaBateria = context.icb_toma_baterias;
                ViewBag.bodegas = context.bodega_concesionario;
                BuscarFavoritos(menu);
                return View();
            }

            if (excelfile.FileName.EndsWith("xls") || excelfile.FileName.EndsWith("xlsx"))
            {
                string path = Server.MapPath("~/Content/" + excelfile.FileName);
                // Validacion para cuando el archivo esta en uso y no puede ser usado desde visual 
                try
                {
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }
                catch (IOException)
                {
                    TempData["mensajeCargue"] =
                        "El archivo esta siendo usado por otro proceso, asegurece de cerrarlo o cree una copia del archivo e intente de nuevo!";
                    ViewBag.tomaBateria = context.icb_toma_baterias;
                    ViewBag.bodegas = context.bodega_concesionario;
                    BuscarFavoritos(menu);
                    return View();
                }

                excelfile.SaveAs(path);

                // Read data from excel file
                Application application = new Application();
                Workbook workbook = application.Workbooks.Open(path);
                Worksheet worksheet = workbook.ActiveSheet;
                Range range = worksheet.UsedRange;

                items = range.Rows.Count;

                // Se hace la validacion para agregar solo una vez el archivo de facturacion con el total de registros que existen
                if (!agregarArchivo)
                {
                    icb_arch_tombat archivoTomBat = new icb_arch_tombat
                    {
                        arch_tombat_nombre = nombreArchivo,
                        arch_tombat_fecha = DateTime.Now,
                        items = items - 1,
                        toma_bateria_id = tomaBateriaId
                    };
                    context.icb_arch_tombat.Add(archivoTomBat);
                    bool resultArchTombat = context.SaveChanges() > 0;
                    if (resultArchTombat)
                    {
                        idUltimoArchTomBat = context.icb_arch_tombat.OrderByDescending(x => x.arch_tombat_id).First()
                            .arch_tombat_id;
                    }

                    agregarArchivo = true;
                }


                byte[] fileBytes = new byte[excelfile.ContentLength];
                int data = excelfile.InputStream.Read(fileBytes, 0, Convert.ToInt32(excelfile.ContentLength));

                for (int row = 2; row <= range.Rows.Count; row++)
                {
                    icb_vehiculo_eventos nuevoRegistroTomaBateria = new icb_vehiculo_eventos();

                    dynamic capturaFecha = ((Range)range.Cells[row, 12]).Text;
                    //DateTime resultFecha = DateTime.Parse(capturaFecha);

                    DateTime parseo = new DateTime();
                    try
                    {
                        parseo = DateTime.Parse(capturaFecha);
                        nuevoRegistroTomaBateria.eventofec_creacion = parseo;

                        nuevoRegistroTomaBateria.eventouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);

                        string VoltajeBateria = ((Range)range.Cells[row, 2]).Text;
                        ;
                        string CCATomado = ((Range)range.Cells[row, 3]).Text;
                        ;
                        string CCABateria = ((Range)range.Cells[row, 4]).Text;
                        string observacion = "CCA Bateria = " + CCABateria + " , Voltaje Bateria = " + VoltajeBateria +
                                          " ,CCA Tomado = " + CCATomado;

                        string decision = ((Range)range.Cells[row, 7]).Text;

                        string vin = ((Range)range.Cells[row, 14]).Text;

                        icb_vehiculo buscarVin = context.icb_vehiculo.FirstOrDefault(x => x.vin == vin);
                        if (buscarVin != null)
                        {
                            icb_vehiculo_bateria buscarTomaExiste = context.icb_vehiculo_bateria.FirstOrDefault(x =>
                                x.veh_bat_vhid == buscarVin.icbvh_id && x.veh_bat_toma_id == tomaBateriaId);

                            if (buscarTomaExiste != null)
                            {
                                itemsFallidos++;
                                if (log.Length < 1)
                                {
                                    log += vin + "*El VIN ya registro inspeccion de bateria";
                                }
                                else
                                {
                                    log += "|" + vin + "*El VIN ya registro inspeccion de bateria";
                                }
                            }
                            else
                            {
                                if (buscarVin.icbvh_estatus == "0")
                                {
                                    itemsFallidos++;
                                    if (log.Length < 1)
                                    {
                                        log += vin + "*El VIN se encuentra en estado 0";
                                    }
                                    else
                                    {
                                        log += "|" + vin + "*El VIN se encuentra en estado 0";
                                    }
                                }
                                else
                                {
                                    itemsCorrectos++;
                                    nuevoRegistroTomaBateria.vin = vin;
                                    nuevoRegistroTomaBateria.evento_nombre = "Toma Bateria";
                                    nuevoRegistroTomaBateria.evento_estado = true;
                                    //nuevoRegistroTomaBateria.id_vehiculo = buscarVin.icbvh_id;
                                    nuevoRegistroTomaBateria.bodega_id = codigoBodega;

                                    icb_sysparameter buscarParametroTpEvento =
                                        context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P12");
                                    string tpEventoParametro = buscarParametroTpEvento != null
                                        ? buscarParametroTpEvento.syspar_value
                                        : "6";

                                    nuevoRegistroTomaBateria.id_tpevento = Convert.ToInt32(tpEventoParametro);
                                    nuevoRegistroTomaBateria.evento_observacion = observacion;
                                    context.icb_vehiculo_eventos.Add(nuevoRegistroTomaBateria);
                                    bool guardarEventoVehiculo = context.SaveChanges() > 0;

                                    context.icb_vehiculo_bateria.Add(new icb_vehiculo_bateria
                                    {
                                        veh_bat_fecha = parseo,
                                        veh_bat_toma_id = tomaBateriaId,
                                        veh_bat_vhid = buscarVin.icbvh_id,
                                        veh_bat_vin = buscarVin.vin,
                                        veh_bat_archid = idUltimoArchTomBat,
                                        veh_bat_decision = decision
                                    });
                                    bool guardarTipoTomaBateria = context.SaveChanges() > 0;

                                    if (guardarEventoVehiculo && guardarTipoTomaBateria)
                                    {
                                        buscarVin.icbvh_fecing_bateria = DateTime.Now;
                                        context.Entry(buscarVin).State = EntityState.Modified;
                                        context.SaveChanges();
                                    }
                                }
                            }
                        }
                        else
                        {
                            itemsFallidos++;
                            if (log.Length < 1)
                            {
                                log += vin + "*El VIN no esta registrado en base de datos";
                            }
                            else
                            {
                                log += "|" + vin + "*El VIN no esta registrado en base de datos";
                            }
                        }
                    }
                    catch (FormatException)
                    {
                        //excelfile.InputStream.Close();
                        //excelfile.InputStream.Dispose();
                        //System.IO.File.Delete(path);
                        //TempData["mensajeError"] = "Verifique que los campos de excel no tengan valores vacios o mal escritos!";
                        //return View();
                    }
                } // Fin del ciclo FOR

                workbook.Close(0);
                application.Quit();
            }

            icb_arch_tombat_log archLog = new icb_arch_tombat_log
            {
                tombat_log_fecha = DateTime.Now,
                tombat_log_itemscorrecto = itemsCorrectos,
                tombat_log_itemserror = itemsFallidos,
                tombat_log_nombrearchivo = nombreArchivo,
                tombat_log_items = items - 1,
                tombat_log_log = log,
                id_arch_tombat = idUltimoArchTomBat
            };
            context.icb_arch_tombat_log.Add(archLog);

            context.SaveChanges();
            ViewBag.tomaBateria = context.icb_toma_baterias;
            ViewBag.bodegas = context.bodega_concesionario;
            BuscarFavoritos(menu);
            return View();
        }


        public JsonResult VehiculosSinCarguePaginados()
        {
            var query = (from vehiculo in context.icb_vehiculo
                         join bodega in context.bodega_concesionario
                             on vehiculo.id_bod equals bodega.id
                         join modelo in context.modelo_vehiculo
                             on vehiculo.modvh_id equals modelo.modvh_codigo
                         where vehiculo.icbvh_estatus == "6" && vehiculo.icbvh_fecing_bateria == null
                         select new
                         {
                             vehiculo.icbvh_id,
                             vehiculo.vin,
                             vehiculo.icbvh_fecinsp_ingreso,
                             bodega.bodccs_nombre,
                             modelo.modvh_nombre
                         }).ToList();

            var data = query.Select(x => new
            {
                x.icbvh_id,
                x.vin,
                icbvh_fecinsp_ingreso = x.icbvh_fecinsp_ingreso != null
                    ? x.icbvh_fecinsp_ingreso.Value.ToShortDateString()
                    : "",
                x.bodccs_nombre,
                x.modvh_nombre
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonResult AgregarInspeccionIndividual(int idVehiculo, decimal ccaBateria, decimal voltajeBateria,
            decimal ccaTomado, int idTomaBateria, int idBodega)
        {
            string observacion = "CCA Bateria = " + ccaBateria + " , Voltaje Bateria = " + voltajeBateria +
                              " ,CCA Tomado = " + ccaTomado;
            icb_vehiculo buscaVehiculo = context.icb_vehiculo.FirstOrDefault(x => x.icbvh_id == idVehiculo);
            bool result = false;
            if (buscaVehiculo != null)
            {
                icb_sysparameter buscarParametroTpEvento = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P12");
                string tpEventoParametro = buscarParametroTpEvento != null ? buscarParametroTpEvento.syspar_value : "6";

                context.icb_vehiculo_eventos.Add(new icb_vehiculo_eventos
                {
                    eventofec_creacion = DateTime.Now,
                    eventouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    evento_nombre = "Toma Bateria",
                    evento_estado = true,
                    //id_vehiculo = idVehiculo,
                    vin = buscaVehiculo.vin,
                    bodega_id = idBodega,
                    id_tpevento = Convert.ToInt32(tpEventoParametro),
                    evento_observacion = observacion
                });
                result = context.SaveChanges() > 0;
                if (result)
                {
                    buscaVehiculo.icbvh_fecing_bateria = DateTime.Now;
                    context.Entry(buscaVehiculo).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetArchivosCarguePaginados()
        {
            var query = (from archivoLog in context.icb_arch_tombat_log
                         join archivo in context.icb_arch_tombat
                             on archivoLog.id_arch_tombat equals archivo.arch_tombat_id
                         join tomaBateria in context.icb_toma_baterias
                             on archivo.toma_bateria_id equals tomaBateria.tombat_id
                         select new
                         {
                             archivoLog.id_arch_tombat,
                             archivoLog.tombat_log_nombrearchivo,
                             archivoLog.tombat_log_items,
                             archivoLog.tombat_log_itemscorrecto,
                             archivoLog.tombat_log_itemserror,
                             archivoLog.tombat_log_fecha,
                             tomaBateria.tombat_nombre
                         }).ToList();

            var data = query.Select(x => new
            {
                x.id_arch_tombat,
                x.tombat_log_nombrearchivo,
                x.tombat_log_items,
                x.tombat_log_itemscorrecto,
                x.tombat_log_itemserror,
                tombat_log_fecha = x.tombat_log_fecha != null ? x.tombat_log_fecha.Value.ToShortDateString() : "",
                x.tombat_nombre
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetDetallesCargueBaterias(int id)
        {
            var detallesJson = from vehiculo in context.icb_vehiculo
                               join vhBateria in context.icb_vehiculo_bateria
                                   on vehiculo.icbvh_id equals vhBateria.veh_bat_vhid
                               join modelo in context.modelo_vehiculo
                                   on vehiculo.modvh_id equals modelo.modvh_codigo
                               where vhBateria.veh_bat_archid == id
                               select new
                               {
                                   vehiculo.icbvh_id,
                                   modelo.modvh_nombre,
                                   vehiculo.vin,
                                   vhBateria.veh_bat_decision
                               };

            string logs = context.icb_arch_tombat_log.FirstOrDefault(x => x.id_arch_tombat == id).tombat_log_log;
            List<ListaLogs> listaLogsExcel = new List<ListaLogs>();
            string[] substring = logs.Split('|');
            try
            {
                foreach (string item in substring)
                {
                    string vinAuxiliar = item.Substring(0, item.IndexOf('*'));

                    //var consultaModelo = (from vehiculo in context.icb_vehiculo
                    //                      join modelo in context.modelo_vehiculo
                    //                      on vehiculo.modvh_id equals modelo.modvh_codigo
                    //                      where vehiculo.vin == vinAuxiliar
                    //                      select modelo.modvh_nombre).FirstOrDefault();

                    listaLogsExcel.Add(new ListaLogs
                    {
                        Vin = item.Substring(0, item.IndexOf('*')),
                        Excepcion = item.Substring(item.IndexOf('*') + 1, item.Length - (item.IndexOf('*') + 1))
                    });
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // Excepcion cuando no hay logs para mostrar, por tanto no hay substrings
            }


            var data = new
            {
                detallesJson,
                listaLogsExcel
            };
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