using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
//using Excel = Microsoft.Office.Interop.Excel;

namespace Homer_MVC.Controllers
{
    public class vlisnuevosController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        //cambio laura
        // GET: vlisnuevos
        public ActionResult Index(int? menu)
        {
            List<listaCargaPreciosNuevosModel> things = (List<listaCargaPreciosNuevosModel>)TempData["listaNoCargados"];
            List<listaCargaPreciosNuevosModel> correctos = (List<listaCargaPreciosNuevosModel>)TempData["listaCargados"];
            ViewBag.noCargados = things;
            ViewBag.Cargados = correctos;
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult browserAsesorNuevos(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        // leer archivo
        public ActionResult Import(HttpPostedFileBase excelfile, int? menu)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                TempData["mensaje_error"] = "El archivo esta vacio o no es un archivo valido!";
                return RedirectToAction("Index", "vlisnuevos", new { menu });
            }
            //var buscaNombre = context.icb_arch_facturacion.FirstOrDefault(x => x.arch_fac_nombre == excelfile.FileName);

            //if (buscaNombre != null)
            //{
            //    TempData["mensajeError"] = "El nombre del archivo ya se encuentra registrado, verifique que el archivo no ha sido cargado antes o cambie de nombre!";
            //    return RedirectToAction("FacturacionGM", "gestionVhNuevo");
            //}

            //else
            //{
            if (excelfile.FileName.EndsWith("xls") || excelfile.FileName.EndsWith("xlsx") ||
                excelfile.FileName.EndsWith("xlsm"))
            {
                string path = Server.MapPath("~/Content/" + excelfile.FileName);
                // Validacion para cuando el archivo esta en uso y no puede ser usado desde visual 
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
                    return RedirectToAction("Index", "vlisnuevos", new { menu });
                }


                ExcelPackage Excel = new ExcelPackage(new FileInfo(path));
                int nows = Excel.Workbook.Worksheets.Count;
                ExcelWorksheet range1 = Excel.Workbook.Worksheets["IMP ESPECIALES"];
                ExcelWorksheet range = Excel.Workbook.Worksheets[1];


                // Read data from excel file
                //Excel.Application application = new Excel.Application();
                //Excel.Workbook workbook = application.Workbooks.Open(path);
                //Excel.Worksheet worksheet = workbook.ActiveSheet;
                //Excel.Range range = worksheet.UsedRange;


                //ArrayList arrText = new ArrayList();

                //var items = 0;
                int itemsCorrectos = 0;
                int itemsFallidos = 0;
                string nombreArchivo = excelfile.FileName;
                //items = range.Cells.[];
                //var num_porcentaje = Cells.Rows.Count / 10;

                //const string Query = "Delete from vlistanuevos";
                //context.Database.ExecuteSqlCommand(Query);
                List<listaCargaPreciosNuevosModel> listaCorrectos = new List<listaCargaPreciosNuevosModel>();
                List<listaCargaPreciosNuevosModel> listaErrado = new List<listaCargaPreciosNuevosModel>();
                //var listaErrados = new List<vlistanuevos>();
                for (int row = 10; row <= 72; row++)
                {
                    try
                    {
                        decimal Porcentajedescuento = 0;
                        vlistanuevos listanuevos = new vlistanuevos();
                        int lista = Convert.ToInt32(range.Cells[row, 1].Value.ToString());
                        string concepto = range.Cells[row, 2].Text;
                        int ano = Convert.ToInt32(range.Cells[row, 3].Value.ToString());
                        int mes = Convert.ToInt32(range.Cells[row, 4].Value.ToString());
                        string modelo = range.Cells[row, 5].Text;
                        string descripcion = range.Cells[row, 6].Text;
                        int anomodelo = Convert.ToInt32(range.Cells[row, 7].Value.ToString());
                        //var planmayor = range.Cells[row, 7].Text;
                        decimal preciolista = Convert.ToDecimal(range.Cells[row, 8].Text.Replace("$", ""));
                        decimal precioespecial = Convert.ToDecimal(range.Cells[row, 9].Text.Replace("$", ""));
                        decimal Preciodescuento = Convert.ToDecimal(range.Cells[row, 10].Text.Replace("$", ""));
                        try
                        {
                            Porcentajedescuento = Convert.ToDecimal(range.Cells[row, 11].Text.Replace("%", ""));
                        }
                        catch
                        {
                            Porcentajedescuento = Convert.ToDecimal(range.Cells[row, 11].Text.Replace("$", ""));
                        }

                        ObjectParameter output = new ObjectParameter("modeloInexistente", typeof(bool));

                        context.cargarListaNuevos(lista, concepto, ano, mes, modelo, descripcion, "", anomodelo,
                            preciolista, precioespecial, Preciodescuento, Porcentajedescuento, output);

                        bool salida = Convert.ToBoolean(output.Value);
                        if (!salida)
                        {
                            itemsCorrectos++;
                            listaCorrectos.Add(new listaCargaPreciosNuevosModel { anio = anomodelo, modelo = modelo });
                        }
                        else
                        {
                            itemsFallidos++;
                            //listaErrados.Add(new vlistanuevos { ano = anomodelo, descripcion = modelo });
                            listaErrado.Add(new listaCargaPreciosNuevosModel { anio = anomodelo, modelo = modelo, mensaje = "El año modelo no existe." });
                        }
                    }
                    catch (Exception ex)
                    {
                        itemsFallidos++;
                        TempData["mensaje_error"] =
                            "Error al leer archivo, revise que los campos no esten vacios o mal escritos, linea " +
                            (row + 1) + ex;
                        return RedirectToAction("Index", "vlisnuevos", new { menu });
                    }
                }
                //}

                try
                {
                    context.SaveChanges();
                    //    Excel.Close(0);
                    //Excel.Quit();
                    System.IO.File.Delete(path);
                }
                catch (DbEntityValidationException dbEx)
                {
                    Exception raise = dbEx;
                    foreach (DbEntityValidationResult validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (DbValidationError validationError in validationErrors.ValidationErrors)
                        {
                            string message = string.Format("{0}:{1}",
                                validationErrors.Entry.Entity,
                                validationError.ErrorMessage);
                            raise = new InvalidOperationException(message, raise);
                        }

                        TempData["mensaje_error"] = raise;
                        return RedirectToAction("Index", "vlisnuevos", new { menu });
                    }

                    throw raise;
                }
                catch (Exception ex)
                {
                    //workbook.Close(0);
                    //application.Quit();
                    System.IO.File.Delete(path);
                    TempData["mensaje_error"] = ex;
                    return RedirectToAction("Index", "vlisnuevos", new { menu });
                }
                TempData["correctos"] = itemsCorrectos;
                TempData["fallidos"] = itemsFallidos;
                TempData["listaNoCargados"] = listaErrado;
                TempData["listaCargados"] = listaCorrectos;
                TempData["mensaje"] = "La lectura del archivo se realizo correctamente!";
                return RedirectToAction("Index", "vlisnuevos", new { menu });
            }

            TempData["mensaje_error"] =
                "La lectura del archivo no fue correcta, verifique que selecciono un archivo valido.";
            return RedirectToAction("Index", "vlisnuevos", new { menu });
        }
        private static Expression<Func<vw_lista_vh_nuevos, string>> GetColumnName(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(vw_lista_vh_nuevos), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<vw_lista_vh_nuevos, string>> lambda = Expression.Lambda<Func<vw_lista_vh_nuevos, string>>(menuProperty, menu);

            return lambda;
        }
        [HttpPost]
        public JsonResult buscarPaginados(string filtroGeneral, string concepto, string ano, string mes)
        {
            if (Session["user_usuarioid"] != null)
            {
                //cantidad de registros a buscar
                string draw = Request.Form.GetValues("draw").FirstOrDefault();
                //indice desde el cual comienza a buscar(por ejemplo desde el 10° registro)
                string start = Request.Form.GetValues("start").FirstOrDefault();
                //cantidad de registros a parsear
                string length = Request.Form.GetValues("length").FirstOrDefault();
                //si hay un campo de busqueda en el datatable(no hay) busca
                string search = Request.Form.GetValues("search[value]").FirstOrDefault();
                //esto me sirve para reiniciar la consulta cuando ordeno las columnas de menor a mayor y que no me vuelva a recalcular todo
                //ES IMPORTANTE QUE LA COLUMNA EN EL DATATABLE TENGA EL NOMBRE DE LA TABLA O VISTA A CONSULTAR, porque vamos a usarla para ordenar.
                string sortColumn = Request.Form
                    .GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]")
                    .FirstOrDefault();
                string sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                search = search.Replace(" ", "");
                int pagina = Convert.ToInt32(start);
                int pageSize = Convert.ToInt32(length);

                int skip = 0;
                if (pagina == 0)
                {
                    skip = 0;
                }
                else
                {
                    skip = pagina;
                }

                CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

                int idusuario = Convert.ToInt32(Session["user_usuarioid"]);

                //por si necesito el rol del usuario
                //comentario para hacer diferencia
                icb_sysparameter admin1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P109").FirstOrDefault();
                int admin = admin1 != null ? Convert.ToInt32(admin1.syspar_value) : 1;

                Expression<Func<vw_lista_vh_nuevos, bool>> predicado = PredicateBuilder.True<vw_lista_vh_nuevos>();
                Expression<Func<vw_lista_vh_nuevos, bool>> predicado2 = PredicateBuilder.False<vw_lista_vh_nuevos>();
                Expression<Func<vw_lista_vh_nuevos, bool>> predicado3 = PredicateBuilder.False<vw_lista_vh_nuevos>();
                //comienzo a hacer mis verificaciones
                //valido si el tecnico está seleccionado (depende del rol va a ver lo suyo solamente o el de todos los tecnicos)
                if (!string.IsNullOrEmpty(concepto))
                {
                    predicado = predicado.And(d => d.concepto.Contains(concepto.ToUpper()));
                }
                if (!string.IsNullOrEmpty(ano))
                {
                    predicado = predicado.And(d => d.ano.ToString().Contains(ano));
                }
                if (!string.IsNullOrEmpty(mes))
                {
                    predicado = predicado.And(d => d.mes.ToString().Contains(mes));
                }
                if (!string.IsNullOrEmpty(filtroGeneral))
                {
                    predicado2 = predicado2.Or(d => 1 == 1 && d.ano.Contains(filtroGeneral));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.mes.Contains(filtroGeneral));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.concepto.Contains(filtroGeneral.ToUpper()));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.descripcion.Contains(filtroGeneral.ToUpper()));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.precioespecial.Contains(filtroGeneral));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.preciolista.Contains(filtroGeneral));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.anio_modelo.Contains(filtroGeneral));

                    predicado = predicado.And(predicado2);
                }

                //contamos el TOTAL de registros que da la consulta
                int registrostotales = context.vw_lista_vh_nuevos.Where(predicado).Count();
                //si el ordenamiento es ascendente o descendente es distinto
                if (pageSize == -1)
                {
                    pageSize = registrostotales;
                }
                //creo un objeto de tipo lista para recibir las variables. Le pongo take 0 porque necesito que sea VACIO
                List<vw_lista_vh_nuevos> query2 = context.vw_lista_vh_nuevos.Where(predicado).Take(0).ToList();
                if (sortColumnDir == "asc")
                {
                    query2 = context.vw_lista_vh_nuevos.Where(predicado).OrderBy(GetColumnName(sortColumn).Compile())
                        .Skip(skip).Take(pageSize).ToList();
                }
                else
                {
                    query2 = context.vw_lista_vh_nuevos.Where(predicado)
                        .OrderByDescending(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
                }
                var query = query2.Select(d => new
                {
                    d.lista,
                    d.concepto,
                    d.ano,
                    d.mes,
                    d.codigo_modelo,
                    d.descripcion,
                    d.planmayor,
                    d.anio_modelo,
                    d.preciolista,
                    d.precioespecial,
                }).ToList();
                int contador = query.Count();
                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }

            return Json(0);
        }
        public JsonResult BuscarDatos()
        {
            var data = from v in context.vlistanuevos
                       join anioModelo in context.anio_modelo
                           on v.anomodelo equals anioModelo.anio_modelo_id
                       join modelo in context.modelo_vehiculo
                           on anioModelo.codigo_modelo equals modelo.modvh_codigo
                       select new
                       {
                           v.lista,
                           v.concepto,
                           v.ano,
                           v.mes,
                           v.descripcion,
                           v.planmayor,
                           modelo = modelo.modvh_codigo,
                           anomodelo = anioModelo.anio,
                           v.preciolista,
                           v.precioespecial,
                           v.descuento,
                           porcendescuento = v.porcendescuento != null ? v.porcendescuento : 0
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CantidadVehiculos()
        {
            int rol = Convert.ToInt32(Session["user_rolid"]);
            int mes = DateTime.Now.Month;
            if (rol == 4)
            {
                var cantModelos = (from v in context.vlistanuevos
                                   group v by new { v.concepto, v.lista, v.ano, v.mes }
                    into modeloGrupo
                                   where modeloGrupo.Key.mes == mes
                                   select new
                                   {
                                       modeloGrupo.Key.concepto,
                                       modeloGrupo.Key.lista,
                                       modeloGrupo.Key.ano,
                                       modeloGrupo.Key.mes,
                                       cantidad = modeloGrupo.Count()
                                   }).ToList();
                return Json(cantModelos, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var cantModelos = (from v in context.vlistanuevos
                                   group v by new { v.concepto, v.lista, v.ano, v.mes }
                    into modeloGrupo
                                   select new
                                   {
                                       modeloGrupo.Key.concepto,
                                       modeloGrupo.Key.lista,
                                       modeloGrupo.Key.ano,
                                       modeloGrupo.Key.mes,
                                       cantidad = modeloGrupo.Count()
                                   }).ToList();
                return Json(cantModelos, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetCantidadModelo(int lista, string concepto)
        {
            var cantModelos = from v in context.vlistanuevos
                              join anioModelo in context.anio_modelo
                                  on v.anomodelo equals anioModelo.anio_modelo_id
                              join modelo in context.modelo_vehiculo
                                  on anioModelo.codigo_modelo equals modelo.modvh_codigo
                              where v.lista == lista && v.concepto == concepto
                              select new
                              {
                                  v.lista,
                                  v.concepto,
                                  v.ano,
                                  v.mes,
                                  modelo.modvh_codigo,
                                  v.descripcion,
                                  v.planmayor,
                                  anomodelo = anioModelo.anio,
                                  v.preciolista,
                                  v.precioespecial,
                                  v.descuento,
                                  v.porcendescuento
                              };
            return Json(cantModelos, JsonRequestBehavior.AllowGet);
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