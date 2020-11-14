using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class campanasTallerController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: campanasTaller
        public ActionResult Create(int? menu)
        {
            BuscarFavoritos(menu);
            return View(new CampanaTallerModel { estado = true });
        }


        // POST: campanasTaller
        [HttpPost]
        public ActionResult Create(CampanaTallerModel modelo2, HttpPostedFileBase excelfile, int? menu)
        {
            if (ModelState.IsValid)
            {
                try
                    {                                     
                var modelo = new tcamptaller
                    {
                    Descripcion = modelo2.Descripcion,
                    nombre = modelo2.Descripcion,
                    estado = modelo2.estado,
                    id_licencia = 1,
                    referencia = modelo2.referencia,
                    numerogwm = "0",
                    numcircular = modelo2.numerocircular
                };
                var fechaini = DateTime.Now;
                var fechafin = DateTime.Now;

                var convertir = DateTime.TryParse(modelo2.fecha_inicio, out fechaini);
                if (convertir == true)
                {
                    modelo.fecha_inicio = fechaini;
                }
                var convertir2 = DateTime.TryParse(modelo2.fecha_fin, out fechafin);

                if (convertir2 == true)
                {
                    modelo.fecha_fin = fechafin;
                }
                List<string> listaVinesAgregados = new List<string>();
                string cargueExcel = Request["checkCargueExcel"];
                if (cargueExcel == "on")
                {
                    //Aqui va si se carga un archivo de excel
                   
                    modelo.fecha_creacion = DateTime.Now;
                    modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.tcamptaller.Add(modelo);
                    int guardarCampana = context.SaveChanges();

                    if (guardarCampana > 0)
                    {
                        tcamptaller buscarUltimaCampana = context.tcamptaller.OrderByDescending(x => x.id).FirstOrDefault();
                        string path = "";
                        if (excelfile == null || excelfile.ContentLength == 0)
                        {
                            TempData["mensaje_error"] = "El archivo esta vacio o no es un archivo valido!";
                            return View();
                        }

                        if (excelfile.FileName.EndsWith("xls") || excelfile.FileName.EndsWith("xlsx"))
                        {
                            path = Server.MapPath("~/Content/" + excelfile.FileName);
                            //Validacion para cuando el archivo esta en uso y no puede ser usado desde visual
                            try
                            {
                                if (System.IO.File.Exists(path))
                                {
                                    System.IO.File.Delete(path);
                                }

                                excelfile.SaveAs(path);
                            }
                            catch (IOException)
                            {
                                TempData["mensaje_error"] =
                                    "El archivo esta siendo usado por otro proceso, asegurece de cerrarlo o cree una copia del archivo e intente de nuevo!";
                                BuscarFavoritos(menu);
                                return View();
                            }


                            string vinesNoCargados = "";

                            ExcelPackage package = new ExcelPackage(new FileInfo(path));
                            ExcelWorksheet workSheet = package.Workbook.Worksheets[1];

                            int totalFilas = workSheet.Dimension.End.Row;
                            for (int i = workSheet.Dimension.Start.Row + 2; i <= workSheet.Dimension.End.Row; i++)
                            {
                                try
                                {
                                    string vin = workSheet.Cells[i, 2].Value.ToString();
                                    if (vin.Length < 13 || vin.Length > 17)
                                    {
                                        vinesNoCargados += vin + ", ";
                                    }
                                    else
                                    {
                                        context.tcamptallervin.Add(new tcamptallervin
                                        {
                                            id_camp = buscarUltimaCampana != null ? buscarUltimaCampana.id : 0,
                                            vin = vin.Trim()
                                        });
                                        listaVinesAgregados.Add(vin.Trim());
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (ex is ArgumentOutOfRangeException || ex is FormatException)
                                    {
                                        excelfile.InputStream.Close();
                                        excelfile.InputStream.Dispose();
                                        System.IO.File.Delete(path);
                                        TempData["mensaje_error"] =
                                            "Error al leer el archivo, verifique que los datos estan bien escritos, linea " +
                                            i;
                                        return RedirectToAction("PedidoEnFirme", "gestionVhNuevo", new { menu });
                                    }
                                }
                            }

                            var vinesSiTieneCampana = (from vehiculo in context.icb_vehiculo
                                                       where listaVinesAgregados.Contains(vehiculo.vin)
                                                       select new
                                                       {
                                                           vehiculo.plan_mayor,
                                                           vehiculo.propietario
                                                       }).ToList();
                            icb_sysparameter buscarNit = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P33");
                            int nitHomaz = buscarNit != null ? Convert.ToInt32(buscarNit.syspar_value) : 0;
                            foreach (var vehiculo in vinesSiTieneCampana)
                            {
                                context.crm_campvintaller.Add(new crm_campvintaller
                                {
                                    idcamp = buscarUltimaCampana.id,
                                    planmayor = vehiculo.plan_mayor,
                                    idtercero = vehiculo.propietario ?? nitHomaz,
                                    fec_creacion = DateTime.Now,
                                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                                });
                            }

                            int guardarVines = context.SaveChanges();
                            if (guardarVines > 0)
                            {
                                if (vinesNoCargados.Length > 5)
                                {
                                    TempData["mensaje_error"] =
                                        "El registro de la nueva campaña se agrego exitosamente, pero algunos vines no se cargaron: " +
                                        vinesNoCargados;
                                }

                                TempData["mensaje"] = "El registro de la nueva campaña se agrego exitosamente!";
                            }

                            excelfile.InputStream.Close();
                            excelfile.InputStream.Dispose();
                        }
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                    }
                }
                else
                {
                    //Aqui va si se hace un cargue de vines manualmente
                    modelo.fecha_creacion = DateTime.Now;
                    modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.tcamptaller.Add(modelo);
                    int guardarCampana = context.SaveChanges();

                    if (guardarCampana > 0)
                    {
                        tcamptaller buscarUltimaCampana = context.tcamptaller.OrderByDescending(x => x.id).FirstOrDefault();
                        int cantidadVines = Convert.ToInt32(Request["cantidadVines"]);
                        for (int i = 1; i <= cantidadVines; i++)
                        {
                            string vinEncontrado = Request["vin" + i];
                            if (!string.IsNullOrEmpty(vinEncontrado))
                            {
                                context.tcamptallervin.Add(new tcamptallervin
                                {
                                    id_camp = buscarUltimaCampana != null ? buscarUltimaCampana.id : 0,
                                    vin = vinEncontrado.Trim()
                                });
                                listaVinesAgregados.Add(vinEncontrado.Trim());
                            }
                        }

                        var vinesSiTieneCampana = (from vehiculo in context.icb_vehiculo
                                                   where listaVinesAgregados.Contains(vehiculo.vin)
                                                   select new
                                                   {
                                                       vehiculo.plan_mayor,
                                                       vehiculo.propietario
                                                   }).ToList();
                        icb_sysparameter buscarNit = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P33");
                        int nitHomaz = buscarNit != null ? Convert.ToInt32(buscarNit.syspar_value) : 0;
                        foreach (var vehiculo in vinesSiTieneCampana)
                        {
                            context.crm_campvintaller.Add(new crm_campvintaller
                            {
                                idcamp = buscarUltimaCampana.id,
                                planmayor = vehiculo.plan_mayor,
                                idtercero = vehiculo.propietario ?? nitHomaz,
                                fec_creacion = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                            });
                        }

                        int guardarVines = context.SaveChanges();
                        if (guardarVines > 0)
                        {
                            TempData["mensaje"] = "El registro de la nueva campaña se agrego exitosamente!";
                        }
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                    }
                }

                int cantnumgwn = Convert.ToInt32(Request["cantnum"]);
                if (cantnumgwn !=0)
                    {
                    for (int i = 0; i < cantnumgwn; i++)
                        {
                        if (Request["Cant"+i] != null)
                            {
                            tcamptallernumgwn campaña = new tcamptallernumgwn();
                            campaña.idcampa = modelo.id;
                            campaña.numgwn = Request["Cant" + i];
                            campaña.estado = true;

                            context.tcamptallernumgwn.Add(campaña);
                            context.SaveChanges();

                            }


                        }



                    }

                    }
                catch (Exception ex)
                    {
                    throw ex;
                    }

                }//aqui

            BuscarFavoritos(menu);
            return View(modelo2);
        }


        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tcamptaller campana2 = context.tcamptaller.Find(id);

            if (campana2 == null)
            {
                TempData["mensaje_error"] = "No se introdujo un número de campaña Válido";
                return RedirectToAction("Create");
            }
            else
            {

                var campana = new CampanaTallerModel
                {
                    id=id,
                    numerogwm=campana2.numerogwm,
                    Descripcion=campana2.Descripcion,
                    estado=campana2.estado,
                    fecha_creacion=campana2.fecha_creacion,
                    fecha_inicio=campana2.fecha_inicio.ToString("yyyy/MM/dd",new CultureInfo("en-US")),
                    fecha_fin=campana2.fecha_fin!=null?campana2.fecha_fin.Value.ToString("yyyy/MM/dd",new CultureInfo("en-US")):"",
                    fec_actualizacion=campana2.fec_actualizacion,
                    id_licencia=campana2.id_licencia,
                    nombre=campana2.nombre,
                    razon_inactivo=campana2.razon_inactivo,
                    referencia=campana2.referencia,
                    userid_creacion=campana2.userid_creacion,
                    user_idactualizacion=campana2.user_idactualizacion,
                    numerocircular = campana2.numcircular
                    };

                ConsultaDatosCreacion(campana);
                BuscarFavoritos(menu);
                return View(campana);
            }
        }


        [HttpPost]
        public ActionResult Edit(CampanaTallerModel modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                List<string> listaVinesAgregados = new List<string>();
                //busco la campana
                var modelo2 = context.tcamptaller.Where(d => d.id == modelo.id).FirstOrDefault();

                modelo2.numerogwm = modelo.numerogwm;
                    modelo2.Descripcion = modelo.Descripcion;
                    modelo2.estado = modelo.estado;

                if (!string.IsNullOrEmpty(modelo.fecha_inicio)){
                    modelo2.fecha_inicio = Convert.ToDateTime(modelo.fecha_inicio, new CultureInfo("en-US"));
                }
                if (!string.IsNullOrEmpty(modelo.fecha_fin))
                {
                    modelo2.fecha_fin = Convert.ToDateTime(modelo.fecha_fin, new CultureInfo("en-US"));
                }
                    modelo2.fec_actualizacion = DateTime.Now;
                    modelo2.nombre = modelo.nombre;
                    modelo2.razon_inactivo = modelo.razon_inactivo;
                    modelo2.referencia = modelo.referencia;
                    modelo2.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modelo2.numcircular = modelo.numerocircular;

                context.Entry(modelo2).State = EntityState.Modified;
                int guardarCampana = context.SaveChanges();

                if (guardarCampana > 0)
                {
                    int cantidadVines = Convert.ToInt32(Request["cantidadVines"]);
                    const string query = "DELETE FROM [dbo].[tcamptallervin] WHERE [id_camp]={0}";
                    int rows = context.Database.ExecuteSqlCommand(query, modelo2.id);
                    for (int i = 1; i <= cantidadVines; i++)
                    {
                        string vinEncontrado = Request["vin" + i];
                        if (!string.IsNullOrEmpty(vinEncontrado))
                        {
                            context.tcamptallervin.Add(new tcamptallervin
                            {
                                id_camp = modelo2.id,
                                vin = vinEncontrado.Trim()
                            });
                            listaVinesAgregados.Add(vinEncontrado.Trim());
                        }
                    }

                    // Se agregan los vines que se encuentren dentro de icb_vehiuclo en la tabla crm_campvintaller ya que son carros que tiene el concesionario.
                    var vinesSiTieneCampana = (from vehiculo in context.icb_vehiculo
                                               where listaVinesAgregados.Contains(vehiculo.vin)
                                               select new
                                               {
                                                   vehiculo.plan_mayor,
                                                   vehiculo.propietario
                                               }).ToList();

                    // Se elimina algun vehiculo que se encuentra en la tabla crm_campvintaller para actualizar en caso que lo hayan quitado en la edicion.
                    var vinesSiYaTEstabanEnCampana = (from vinCampana in context.crm_campvintaller
                                                      where vinCampana.idcamp == modelo2.id
                                                      select new
                                                      {
                                                          vinCampana.id
                                                      }).ToList();

                    foreach (var vinesABorrar in vinesSiYaTEstabanEnCampana)
                    {
                        const string queryEliminar = "DELETE FROM [dbo].[crm_campvintaller] WHERE [id]={0}";
                        int rowsEliminar = context.Database.ExecuteSqlCommand(queryEliminar, vinesABorrar.id);
                    }

                    icb_sysparameter buscarNit = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P33");
                    int nitHomaz = buscarNit != null ? Convert.ToInt32(buscarNit.syspar_value) : 0;
                    foreach (var vehiculo in vinesSiTieneCampana)
                    {
                        context.crm_campvintaller.Add(new crm_campvintaller
                        {
                            idcamp = modelo2.id,
                            planmayor = vehiculo.plan_mayor,
                            idtercero = vehiculo.propietario ?? nitHomaz,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                        });
                    }

                    int guardarVines = context.SaveChanges();
                    if (guardarVines > 0)
                    {
                        TempData["mensaje"] = "El actualización de la nueva campaña se agrego exitosamente!";
                    }
                }
                else
                {
                    TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                }
            }

            ConsultaDatosCreacion(modelo);
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public JsonResult BuscarCampanasDeVin(string vin)
        {
            var buscarVin = (from campanas in context.tcamptaller
                             join vines in context.tcamptallervin
                                 on campanas.id equals vines.id_camp
                             where vines.vin == vin
                             select new
                             {
                                 campanas.nombre,
                                 campanas.fecha_inicio,
                                 campanas.fecha_fin,
                                 campanas.Descripcion
                             }).ToList();

            var data = buscarVin.Select(x => new
            {
                x.nombre,
                fecha_inicio = x.fecha_inicio.ToShortDateString(),
                fecha_fin = x.fecha_fin != null ? x.fecha_fin.Value.ToShortDateString() : "",
                x.Descripcion
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarVinesAgregados(int? idCampana)
        {
            var buscarVines = (from campana in context.tcamptaller
                               join vines in context.tcamptallervin
                                   on campana.id equals vines.id_camp
                               where campana.id == idCampana
                               select new
                               {
                                   vines.vin
                               }).ToList();

            return Json(buscarVines, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Buscarnumgwn(int? idCampana) {

            var datos = context.tcamptallernumgwn.Where(x => x.idcampa == idCampana && x.estado==true).Select(t => new { 
                t.id, t.numgwn }).ToList();
            return Json(datos, JsonRequestBehavior.AllowGet);
            }

        public JsonResult EliminarNumGwn(int id) {


            tcamptallernumgwn campañadetall = context.tcamptallernumgwn.Where(x => x.id == id).FirstOrDefault();
            campañadetall.estado = false;
            context.Entry(campañadetall).State = EntityState.Modified;
            context.SaveChanges();

            return Json(true, JsonRequestBehavior.AllowGet);
            }

        public JsonResult Agregarnumgwnedit(int idcampaña, string numgwn) {

            tcamptallernumgwn campañadetall = context.tcamptallernumgwn.Where(x => x.numgwn == numgwn && x.idcampa == idcampaña
            ).FirstOrDefault();

            if (campañadetall== null)
                {
                tcamptallernumgwn campañadetalle = new tcamptallernumgwn();
                campañadetalle.idcampa = idcampaña;
                campañadetalle.numgwn = numgwn;
                campañadetalle.estado = true;
                context.tcamptallernumgwn.Add(campañadetalle);
                context.SaveChanges();

                }
            return Json(true, JsonRequestBehavior.AllowGet);
            }


        public void ConsultaDatosCreacion(CampanaTallerModel campana)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = context.users.Find(campana.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = context.users.Find(campana.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }


        public JsonResult BuscarCampanasTaller()
        {
            var buscarCampanas = (from campanas in context.tcamptaller
                                  select new
                                  {
                                      campanas.id,
                                      campanas.nombre,
                                      campanas.fecha_inicio,
                                      campanas.fecha_fin,
                                      campanas.Descripcion
                                  }).ToList();

            var data = buscarCampanas.Select(x => new
            {
                x.id,
                fecha_inicio = x.fecha_inicio.ToShortDateString(),
                fecha_fin = x.fecha_fin.Value.ToShortDateString(),
                x.nombre,
                x.Descripcion
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