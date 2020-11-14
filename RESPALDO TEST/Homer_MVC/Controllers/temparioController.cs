using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using OfficeOpenXml;
using System;
using System.Collections;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class temparioController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        private readonly CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");
        CultureInfo miCultura = new CultureInfo("is-IS");
        // GET: tempario
        public ActionResult Create(int? menu)
        {
            ViewBag.tipooperacion = new SelectList(context.ttipooperacion, "id", "Descripcion");
            var consultaIva = (from codigoIva in context.codigo_iva
                               select new
                               {
                                   codigoIva.id,
                                   Descripcion = codigoIva.Descripcion + " (" + codigoIva.porcentaje + "%)"
                               }).ToList();
            ViewBag.iva = new SelectList(consultaIva, "id", "Descripcion");

            ViewBag.tipooperacion = new SelectList(context.ttipooperacion, "id", "Descripcion");
            var modelo = new ModeloTempario() {
                aplica_costo = true,
            };

            ViewBag.idplanm = new SelectList(context.tplanmantenimiento, "id", "Descripcion", modelo.idplanm);


            BuscarFavoritos(menu);
            return View(modelo);
        }


        // POST: tempario
        [HttpPost]
        public ActionResult Create(ModeloTempario modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                ttempario buscarPorCodigo = context.ttempario.FirstOrDefault(x => x.codigo == modelo.codigo);
                if (buscarPorCodigo != null)
                {
                    TempData["mensaje_error"] =
                        "El codigo de la operacion ya se encuentra registrado, por favor valide...";
                }
                else
                {
                    var horac = Convert.ToDecimal(modelo.HoraCliente, new CultureInfo("is-IS"));
                    var horasc = horac.ToString("N2", new CultureInfo("en-US"));
                    var horao = Convert.ToDecimal(modelo.HoraOperario, new CultureInfo("is-IS"));
                    var horaso = horao.ToString("N2", new CultureInfo("en-US"));
                    var temparionuevo = new ttempario
                    {
                        fec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        estado = true,
                        iva = 4,
                        costo = 0,
                        tiempo = modelo.tiempo,
                        categoria = modelo.categoria,
                        tipooperacion = modelo.tipooperacion.Value,
                        codigo = modelo.codigo,
                        esmatriz = modelo.esmatriz,
                        operacion = modelo.operacion,
                        HoraCliente = horasc,
                        HoraOperario = horao,
                        idplanm = modelo.idplanm,
                        aplica_costo=modelo.aplica_costo,
                    };



                    context.ttempario.Add(temparionuevo);
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "El tempario se ha creado exitosamente.";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error en la base de datos, por favor verifique su conexion...";
                    }
                }
            }
            else
            {
                TempData["mensaje_error"] ="Errores de validación, por favor verifique";
            }                          
            ViewBag.tipooperacion = new SelectList(context.ttipooperacion, "id", "Descripcion", modelo.tipooperacion);
            var consultaIva = (from codigoIva in context.codigo_iva
                               select new
                               {
                                   codigoIva.id,
                                   Descripcion = codigoIva.Descripcion + " (" + codigoIva.porcentaje + "%)"
                               }).ToList();

            ViewBag.idplanm = new SelectList(context.tplanmantenimiento, "id", "Descripcion", modelo.idplanm);
            
            ViewBag.iva = new SelectList(consultaIva, "id", "Descripcion", modelo.iva);

            BuscarFavoritos(menu);
            return View();
        }


        public ActionResult Edit(string id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ttempario tempario = context.ttempario.FirstOrDefault(x => x.codigo == id);
            if (tempario == null)
            {
                TempData["mensaje_error"] = "No se encontró la operación especificada";
                return RedirectToAction("Create", "tempario", new { menu = menu });
            }
            else
            {
                var horac = Convert.ToDecimal(tempario.HoraCliente, new CultureInfo("en-US"));
                var horasc = horac.ToString("N2", new CultureInfo("is-IS"));
                var horao = Convert.ToDecimal(tempario.HoraOperario, new CultureInfo("en-US"));
                var horaso = horao.ToString("N2", new CultureInfo("is-IS"));

                var modelo = new ModeloTempario
                {
                    categoria=tempario.categoria,
                    codigo=tempario.codigo,
                    costo=tempario.costo,
                    esmatriz=tempario.esmatriz,
                    estado=tempario.estado,
                    fec_creacion=tempario.fec_creacion,
                    fec_actualizacion=tempario.fec_actualizacion,
                    HoraCliente= horasc,
                    HoraOperario= horaso,
                    idplanm=tempario.idplanm,
                    operacion=tempario.operacion,
                    razon_inactivo=tempario.razon_inactivo,
                    tipooperacion=tempario.tipooperacion,
                    userid_creacion=tempario.userid_creacion,
                    user_idactualizacion=tempario.user_idactualizacion,
                    aplica_costo=tempario.aplica_costo,
                };
            

            ViewBag.tipooperacion = new SelectList(context.ttipooperacion, "id", "Descripcion", modelo.tipooperacion);
            var consultaIva = (from codigoIva in context.codigo_iva
                               select new
                               {
                                   codigoIva.id,
                                   Descripcion = codigoIva.Descripcion + " (" + codigoIva.porcentaje + "%)"
                               }).ToList();
            ViewBag.iva = new SelectList(consultaIva, "id", "Descripcion", modelo.iva);
                ViewBag.idplanm = new SelectList(context.tplanmantenimiento, "id", "Descripcion", modelo.idplanm);
                string precio_matriz = tempario.preciomatriz.ToString("0,0", elGR);

            ViewBag.preciomatriz = precio_matriz;
                BuscarFavoritos(menu);
                return View(modelo);
            }
        }


        [HttpPost]
        public ActionResult Edit(ModeloTempario modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //busco el tempario
                var horac = Convert.ToDecimal(modelo.HoraCliente, new CultureInfo("is-IS"));
                var horasc = horac.ToString("N2", new CultureInfo("en-US"));
                var horao = Convert.ToDecimal(modelo.HoraOperario, new CultureInfo("is-IS"));
                var horaso = horao.ToString("N2", new CultureInfo("en-US"));

                var tempario = context.ttempario.Where(d => d.codigo == modelo.codigo).FirstOrDefault();
                if (tempario != null)
                {
                    
                    tempario.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    tempario.categoria = modelo.categoria;
                    tempario.operacion = modelo.operacion;
                    tempario.esmatriz = modelo.esmatriz;
                    tempario.estado = modelo.estado;
                    tempario.tipooperacion = modelo.tipooperacion.Value;
                    tempario.HoraCliente = horasc;
                    tempario.HoraOperario = horao;
                    tempario.aplica_costo = modelo.aplica_costo;
                    if (modelo.esmatriz == true)
                    {
                        tempario.idplanm = modelo.idplanm;
                    }
                    else
                    {
                        tempario.idplanm = null;
                    }
                    tempario.fec_actualizacion = DateTime.Now;
                    tempario.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(tempario).State = EntityState.Modified;
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "El tempario se ha actualizado exitosamente.";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error en la base de datos, por favor verifique su conexion...";
                    }
                }
                else
                {
                    var temparionuevo = new ttempario
                    {
                        fec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        estado = true,
                        iva = 4,
                        costo = 0,
                        tiempo = modelo.tiempo,
                        categoria = modelo.categoria,
                        tipooperacion = modelo.tipooperacion.Value,
                        codigo = modelo.codigo,
                        esmatriz = modelo.esmatriz,
                        operacion = modelo.operacion,
                        HoraCliente = horasc,
                        HoraOperario = horao,
                        idplanm = modelo.idplanm,
                    };



                    context.ttempario.Add(temparionuevo);
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "El tempario se ha creado exitosamente.";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error en la base de datos, por favor verifique su conexion...";
                    }
                }
            }
            else
            {
                TempData["mensaje_error"] = "Errores de validación, por favor verifique";

            }


            //}
            //else {
            //    TempData["mensaje_error"] = "El codigo de la operacion ya se encuentra registrado, por favor valide...";
            //}
            //}  
            ViewBag.idplanm = new SelectList(context.tplanmantenimiento, "id", "Descripcion", modelo.idplanm);
            ViewBag.tipooperacion = new SelectList(context.ttipooperacion, "id", "Descripcion", modelo.tipooperacion);
            var consultaIva = (from codigoIva in context.codigo_iva
                               select new
                               {
                                   codigoIva.id,
                                   Descripcion = codigoIva.Descripcion + " (" + codigoIva.porcentaje + "%)"
                               }).ToList();
            ViewBag.iva = new SelectList(consultaIva, "id", "Descripcion", modelo.iva);
            BuscarFavoritos(menu);
            return View(modelo);
        }


        public JsonResult BuscarTemparios()
        {
            var buscarTemparios = (from tempario in context.ttempario
                                   join tipoOperacion in context.ttipooperacion
                                   on tempario.tipooperacion equals tipoOperacion.id              
                                   select new
                                   {
                                       tempario.codigo,
                                       tempario.operacion,
                                       tiempocliente = tempario.HoraCliente != null ? tempario.HoraCliente : "0",
                                       tiempooperario = tempario.HoraOperario != null ? tempario.HoraOperario : 0,
                                       tipoOperacion.Descripcion,
                                       categoria = tempario.categoria 
                                       }).ToList();

            return Json(buscarTemparios, JsonRequestBehavior.AllowGet);
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

        
        public ActionResult preCargue(HttpPostedFileBase txtPreCargue, int? menu)
            {
            if (txtPreCargue != null)
                {

                try
                    {
                    string path = Server.MapPath("~/Content/" + txtPreCargue.FileName);
                    // Validacion para cuando el archivo esta en uso y no puede ser usado desde visual 

                    if (System.IO.File.Exists(path))
                        {
                        System.IO.File.Delete(path);
                        }

                    txtPreCargue.SaveAs(path);
                    FileInfo fileInfo = new FileInfo(path);
                    ExcelPackage package = new ExcelPackage(fileInfo);
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();


                    int rows = worksheet.Dimension.Rows;
                    int columns = worksheet.Dimension.Columns;

                    if (columns == 6)
                        {

                        for (int i = 2; i < rows; i++)
                            {


                            if (!string.IsNullOrEmpty(worksheet.Cells[i, 1].Value.ToString()))

                                {
                                string categoria = worksheet.Cells[i, 1].Value.ToString();
                                string tipooperacion = worksheet.Cells[i, 2].Value.ToString();
                                string codigo = worksheet.Cells[i, 3].Value.ToString();
                                string operacion = worksheet.Cells[i, 4].Value.ToString();
                                string horacliente = worksheet.Cells[i, 5].Value.ToString();
                                string horaoperario = worksheet.Cells[i, 6].Value.ToString();


                                ttempario tempario = context.ttempario.Where(x => x.codigo == codigo).FirstOrDefault();
                                if (tempario != null)
                                    {
                                    tempario.categoria = categoria;
                                    tempario.HoraCliente = horacliente;
                                    tempario.HoraOperario = Convert.ToDecimal(horaoperario, miCultura);
                                    context.Entry(tempario).State = EntityState.Modified;
                                    context.SaveChanges();
                                    }
                                else
                                    {
                                    ttipooperacion ttipooperacion = context.ttipooperacion.Where(x => x.Descripcion == tipooperacion).FirstOrDefault();
                                    if (ttipooperacion != null)
                                        {

                                        ttempario temparionew = new ttempario();
                                        temparionew.categoria = categoria;
                                        temparionew.codigo = codigo;
                                        temparionew.operacion = operacion;
                                        temparionew.tipooperacion = ttipooperacion.id;
                                        temparionew.iva = 1;
                                        temparionew.costo = 0;
                                        temparionew.precio = 0;
                                        temparionew.fec_creacion = DateTime.Now;
                                        temparionew.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                        temparionew.estado = true;
                                        temparionew.HoraCliente = horacliente;
                                        temparionew.HoraOperario = Convert.ToDecimal(horaoperario, miCultura);
                                        context.ttempario.Add(temparionew);
                                        context.SaveChanges();
                                        }
                                    else
                                        {
                                        ttipooperacion ttipooper = new ttipooperacion();
                                        ttipooper.Descripcion = tipooperacion;
                                        ttipooper.fec_creacion = DateTime.Now;
                                        ttipooper.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                        ttipooper.estado = true;
                                        context.ttipooperacion.Add(ttipooper);

                                        ttempario temparionew = new ttempario();
                                        temparionew.codigo = codigo;
                                        temparionew.operacion = operacion;
                                        temparionew.tipooperacion = ttipooper.id;
                                        temparionew.iva = 1;
                                        temparionew.costo = 0;
                                        temparionew.precio = 0;
                                        temparionew.fec_creacion = DateTime.Now;
                                        temparionew.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                        temparionew.estado = true;
                                        temparionew.HoraCliente = horacliente;
                                        temparionew.HoraOperario = Convert.ToDecimal(horaoperario, miCultura);
                                        context.ttempario.Add(temparionew);
                                        context.SaveChanges();
                                        }
                                    }

                                }

                            }


                        TempData["mensaje"] = "Archivo cargado con exito";



                        }
                    else
                        {

                        TempData["mensaje_error"] = "El numero de columnas excede al permitido";
                        }





           
                    }
                catch (DbEntityValidationException dbEx)
                    {


                    TempData["mensajeError"] = "se a presentado un error" + dbEx;
                    return RedirectToAction("Create", "tempario", new { menu });
                    }
                }
            else {

                TempData["mensaje_error"] = "Debe seleccionar algun archivo";
                }
            return RedirectToAction("Create", "tempario");
            }


        }
    }