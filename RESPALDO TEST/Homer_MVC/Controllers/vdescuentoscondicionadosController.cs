using Homer_MVC.IcebergModel;
using OfficeOpenXml;
using System;
using System.Collections;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class vdescuentoscondicionadosController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        public void listas(vdescuentoscondicionados descuentos)
        {
            ViewBag.modelo = new SelectList(db.modelo_vehiculo, "modvh_codigo", "modvh_nombre", descuentos.modelo);
            ViewBag.tipo = new SelectList(db.vtipobono, "id", "tipobono", descuentos.tipo);
        }

        // GET: vdescuentoscondicionados/Create
        public ActionResult Create(int? menu)
        {
            vdescuentoscondicionados descuento = new vdescuentoscondicionados();
            listas(descuento);
            BuscarFavoritos(menu);
            return View();
        }

        // POST: vdescuentoscondicionados/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(vdescuentoscondicionados vdescuentoscondicionados, int? menu)
        {
            if (ModelState.IsValid)
            {
                vdescuentoscondicionados.fec_creacion = DateTime.Now;
                vdescuentoscondicionados.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                db.vdescuentoscondicionados.Add(vdescuentoscondicionados);
                db.SaveChanges();
                TempData["mensaje"] = "Registro creado correctamente";

                listas(vdescuentoscondicionados);
                int id = db.vdescuentoscondicionados.OrderByDescending(x => x.id).FirstOrDefault().id;
                return RedirectToAction("Edit", new { id, menu });
            }

            listas(vdescuentoscondicionados);
            BuscarFavoritos(menu);
            return View(vdescuentoscondicionados);
        }

        // GET: vdescuentoscondicionados/Edit/5
        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            vdescuentoscondicionados vdescuentoscondicionados = db.vdescuentoscondicionados.Find(id);
            if (vdescuentoscondicionados == null)
            {
                return HttpNotFound();
            }

            ConsultaDatosCreacion(vdescuentoscondicionados);
            listas(vdescuentoscondicionados);
            BuscarFavoritos(menu);
            return View(vdescuentoscondicionados);
        }

        // POST: vdescuentoscondicionados/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(vdescuentoscondicionados vdescuentoscondicionados, int? menu)
        {
            if (ModelState.IsValid)
            {
                vdescuentoscondicionados.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                vdescuentoscondicionados.fec_actualizacion = DateTime.Now;
                db.Entry(vdescuentoscondicionados).State = EntityState.Modified;
                db.SaveChanges();
                TempData["mensaje"] = "Registro editado correctamente";
            }

            ConsultaDatosCreacion(vdescuentoscondicionados);
            listas(vdescuentoscondicionados);
            BuscarFavoritos(menu);
            return View(vdescuentoscondicionados);
        }

        public ActionResult Browser(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult Import(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        [HttpPost]
        public ActionResult Import(HttpPostedFileBase excelfile, int? menu)
        {
            if (excelfile == null || excelfile.ContentLength == 0)
            {
                TempData["mensaje_error"] = "El archivo esta vacio o no es un archivo valido!";
                return RedirectToAction("Index", "vlisnuevos", new { menu });
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

                    excelfile.SaveAs(path);
                }
                catch (IOException)
                {
                    TempData["mensaje_error"] =
                        "El archivo esta siendo usado por otro proceso, asegurece de cerrarlo o cree una copia del archivo e intente de nuevo!";
                    BuscarFavoritos(menu);
                    return View();
                }

                // Read data from excel file
                ExcelPackage Excel = new ExcelPackage(new FileInfo(path));
                ExcelWorksheet range = Excel.Workbook.Worksheets[1];

                ArrayList arrText = new ArrayList();

                int itemsCorrectos = 0;
                int itemsFallidos = 0;
                string nombreArchivo = excelfile.FileName;

                for (int row = range.Dimension.Start.Row + 1; row <= range.Dimension.End.Row; row++)
                {
                    string circular = range.Cells[row, 1].Value != null ? range.Cells[row, 1].Value.ToString() : "";
                    string ano = range.Cells[row, 2].Value != null ? range.Cells[row, 2].Value.ToString() : "";
                    string mes = range.Cells[row, 3].Value != null ? range.Cells[row, 3].Value.ToString() : "";
                    string modelo = range.Cells[row, 4].Value != null ? range.Cells[row, 4].Value.ToString() : "";
                    string anomodelo = range.Cells[row, 5].Value != null ? range.Cells[row, 5].Value.ToString() : "";
                    string feccompradesde = range.Cells[row, 6].Value != null ? range.Cells[row, 6].Value.ToString() : "";
                    string feccomprahasta = range.Cells[row, 7].Value != null ? range.Cells[row, 7].Value.ToString() : "";
                    string descuentoAdicional =
                        range.Cells[row, 8].Value != null ? range.Cells[row, 8].Value.ToString() : "";
                    string fecadicional = range.Cells[row, 9].Value != null ? range.Cells[row, 9].Value.ToString() : "";
                    string descuentos = range.Cells[row, 10].Value != null ? range.Cells[row, 10].Value.ToString() : "";
                    string descuentopie = range.Cells[row, 11].Value != null ? range.Cells[row, 11].Value.ToString() : "";
                    string bonoretoma = range.Cells[row, 12].Value != null ? range.Cells[row, 12].Value.ToString() : "";
                    string fecventaini = range.Cells[row, 13].Value != null ? range.Cells[row, 13].Value.ToString() : "";
                    string fecventafin = range.Cells[row, 14].Value != null ? range.Cells[row, 14].Value.ToString() : "";
                    string aplicafota = range.Cells[row, 15].Value != null ? range.Cells[row, 15].Value.ToString() : "";
                    string esdetal = range.Cells[row, 16].Value != null ? range.Cells[row, 16].Value.ToString() : "";
                    string Notas = range.Cells[row, 17].Value != null ? range.Cells[row, 17].Value.ToString() : "";

                    vdescuentoscondicionados descuento = new vdescuentoscondicionados
                    {
                        fec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        circular = circular,
                        ano = Convert.ToInt32(ano),
                        mes = Convert.ToInt32(mes),
                        modelo = modelo,
                        anomodelo = Convert.ToInt32(anomodelo),
                        feccompradesde = Convert.ToDateTime(feccompradesde),
                        feccomprahasta = Convert.ToDateTime(feccomprahasta)
                    };
                    descuento.descuento = Convert.ToDecimal(descuento);
                    descuento.fecadicional = Convert.ToDateTime(fecadicional);
                    descuento.descuentopie = Convert.ToDecimal(descuentopie);
                    descuento.bonoretoma = Convert.ToDecimal(bonoretoma);
                    descuento.fecventaini = Convert.ToDateTime(fecventaini);
                    descuento.fecventafin = Convert.ToDateTime(fecventafin);
                    descuento.aplicafota = Convert.ToBoolean(aplicafota);
                    descuento.esdetal = Convert.ToBoolean(esdetal);
                    descuento.descuentoadicional = Convert.ToDecimal(descuentoAdicional);
                    descuento.Notas = Notas;
                    db.vdescuentoscondicionados.Add(descuento);

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
                        return View();
                    }
                } // Fin del ciclo FOR
                  //}

                try
                {
                    db.SaveChanges();
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
                        BuscarFavoritos(menu);
                        return View();
                    }

                    throw raise;
                }
                catch (Exception ex)
                {
                    //workbook.Close(0);
                    //application.Quit();
                    System.IO.File.Delete(path);
                    TempData["mensaje_error"] = ex;
                    BuscarFavoritos(menu);
                    return View();
                }

                TempData["correctos"] = itemsCorrectos;
                TempData["fallidos"] = itemsFallidos;
                TempData["mensaje"] = "La lectura del archivo se realizo correctamente!";
                int id = db.vdescuentoscondicionados.OrderByDescending(x => x.id).FirstOrDefault().id;
                return RedirectToAction("Edit", new { id, menu });
            }

            TempData["mensaje_error"] =
                "La lectura del archivo no fue correcta, verifique que selecciono un archivo valido.";
            BuscarFavoritos(menu);
            return View();
        }

        public void ConsultaDatosCreacion(vdescuentoscondicionados descuentos)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creator = db.users.Find(descuentos.userid_creacion);
            if (creator != null)
            {
                ViewBag.user_nombre_cre = creator.user_nombre + " " + creator.user_apellido;
            }

            users modificator = db.users.Find(descuentos.user_idactualizacion);
            if (modificator != null)
            {
                ViewBag.user_nombre_act = modificator.user_nombre + " " + modificator.user_apellido;
                ViewBag.user_fec_act = modificator.userfec_actualizacion.ToString();
            }
        }

        public JsonResult BuscarDatos()
        {
            var data = from x in db.vdescuentoscondicionados
                       select new
                       {
                           ano_mes = x.ano + " / " + x.mes,
                           x.circular,
                           x.vtipobono.tipobono,
                           x.modelo_vehiculo.modvh_nombre,
                           x.anomodelo,
                           x.descuento,
                           x.id
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }


        public void BuscarFavoritos(int? menu)
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

            var buscarFavoritosSeleccionados = (from favoritos in db.favoritos
                                                join menu2 in db.Menus
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