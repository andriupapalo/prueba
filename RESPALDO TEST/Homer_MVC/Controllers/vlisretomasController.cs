using Homer_MVC.IcebergModel;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class vlisretomasController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: vlisretomas
        public ActionResult Index(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult browserAsesor(int? menu)
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
                return RedirectToAction("Index", "vlisretomas", new { menu });
            }
            //var buscaNombre = context.icb_arch_facturacion.FirstOrDefault(x => x.arch_fac_nombre == excelfile.FileName);

            //if (buscaNombre != null)
            //{
            //    TempData["mensajeError"] = "El nombre del archivo ya se encuentra registrado, verifique que el archivo no ha sido cargado antes o cambie de nombre!";
            //    return RedirectToAction("FacturacionGM", "gestionVhNuevo");
            //}

            //else
            //{
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

                    excelfile.SaveAs(path);
                }
                catch (IOException)
                {
                    TempData["mensaje_error"] =
                        "El archivo esta siendo usado por otro proceso, asegurece de cerrarlo o cree una copia del archivo e intente de nuevo!";
                    return RedirectToAction("Index", "vlisretomas", new { menu });
                }


                // Read data from excel file
                Application application = new Application();
                Workbook workbook = application.Workbooks.Open(path);
                Worksheet worksheet = workbook.ActiveSheet;
                Range range = worksheet.UsedRange;


                ArrayList arrText = new ArrayList();

                int items = 0;
                int itemsCorrectos = 0;
                int itemsFallidos = 0;
                string nombreArchivo = excelfile.FileName;
                items = range.Rows.Count - 1;
                int num_porcentaje = range.Rows.Count / 10;

                const string Query = "Delete from vlisretomas";
                context.Database.ExecuteSqlCommand(Query);

                for (int row = 2; row <= range.Rows.Count; row++)
                {
                    vlisretomas listaretomas = new vlisretomas
                    {
                        marca = ((Range)range.Cells[row, 1]).Text,
                        estilo = ((Range)range.Cells[row, 2]).Text,
                        anoModelo = ((Range)range.Cells[row, 3]).Text,
                        precio = ((Range)range.Cells[row, 4]).Text,
                        fecha = DateTime.Now,

                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                    };
                    context.vlisretomas.Add(listaretomas);

                    try
                    {
                        itemsCorrectos++;
                    }
                    catch (Exception ex)
                    {
                        itemsFallidos++;
                        TempData["mensaje_error"] =
                            "Error al leer archivo, revise que los campos no esten vacios o mal escritos, linea " +
                            (row + 1) + ex;
                        return RedirectToAction("Index", "vlisretomas", new { menu });
                    }
                } // Fin del ciclo FOR
                  //}

                try
                {
                    context.SaveChanges();
                    workbook.Close(0);
                    application.Quit();
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
                        return RedirectToAction("Index", "vlisretomas", new { menu });
                    }

                    throw raise;
                }
                catch (Exception ex)
                {
                    workbook.Close(0);
                    application.Quit();
                    System.IO.File.Delete(path);
                    TempData["mensaje_error"] = ex;
                    return RedirectToAction("Index", "vlisretomas", new { menu });
                }

                TempData["correctos"] = itemsCorrectos;
                TempData["fallidos"] = itemsFallidos;
                TempData["mensaje"] = "La lectura del archivo se realizo correctamente!";
                return RedirectToAction("Index", "vlisretomas", new { menu });
            }

            TempData["mensaje_error"] =
                "La lectura del archivo no fue correcta, verifique que selecciono un archivo valido.";
            return RedirectToAction("Index", "vlisretomas", new { menu });
        }

        public JsonResult BuscarDatos()
        {
            var data = from v in context.vlisretomas
                       join u in context.users
                           on v.userid_creacion equals u.user_id
                       select new
                       {
                           v.marca,
                           v.estilo,
                           v.anoModelo,
                           fecha = v.fecha != null ? v.fecha.ToString() : "",
                           v.precio,
                           usuario = u.user_nombre + " " + u.user_apellido
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CantidadVehiculos()
        {
            var cantModelos = (from v in context.vlisretomas
                               group v by new { v.marca, v.estilo }
                into modeloGrupo
                               select new
                               {
                                   modeloGrupo.Key.marca,
                                   modeloGrupo.Key.estilo,

                                   cantidad = modeloGrupo.Count()
                               }).ToList();
            return Json(cantModelos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCantidadModelo(string estilo)
        {
            var cantModelos = (from v in context.vlisretomas
                               where v.estilo == estilo
                               select new
                               {
                                   v.precio,
                                   v.marca,
                                   v.estilo,
                                   v.anoModelo
                               }).ToList();
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