using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class matricesMantenimientoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
     
        // GET: matricesMantenimiento
        public ActionResult Create(int? menu)
        {
            ViewBag.modeloGeneral = new SelectList(context.vmodelog, "id", "Descripcion");
            ViewBag.modeloGeneral2 = new SelectList(context.vmodelog, "id", "Descripcion");

            BuscarFavoritos(menu);
            return View();
        }


        [HttpPost]
        public ActionResult Create(HttpPostedFileBase excelfileManoObra, HttpPostedFileBase excelfileRepuestos, int? menu)
        {

            string path = "";
            string tipo = Request["tipo"];
            string modeloGeneral = Request["modeloGeneral"];
            int guardar = 0;

            if (tipo == "R")
            {
                if (excelfileRepuestos == null || excelfileRepuestos.ContentLength == 0)
                {
                    TempData["mensaje_error"] = "El archivo esta vacio o no es un archivo valido!";
                    ViewBag.modeloGeneral = new SelectList(context.vmodelog, "id", "Descripcion");
                    BuscarFavoritos(menu);
                    return View();
                }

                if (excelfileRepuestos.FileName.EndsWith("xls") || excelfileRepuestos.FileName.EndsWith("xlsx"))
                {
                    path = Server.MapPath("~/Content/" + excelfileRepuestos.FileName);
                    // Validacion para cuando el archivo esta en uso y no puede ser usado desde visual 
                    try
                    {
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }

                        excelfileRepuestos.SaveAs(path);
                    }
                    catch (IOException)
                    {
                        TempData["mensaje_error"] =
                            "El archivo esta siendo usado por otro proceso, asegurece de cerrarlo o cree una copia del archivo e intente de nuevo!";
                        BuscarFavoritos(menu);
                        return RedirectToAction("cargamasiva", new { menu });
                    }

                    ExcelPackage package = new ExcelPackage(new FileInfo(path));
                    ExcelWorksheet workSheet = package.Workbook.Worksheets[1];

                    int kitSeleccionado = Convert.ToInt32(Request["modeloGeneral"]);
                    string tipoSeleccionado = Request["tipo"];
                    //string listaPrecios = Request["lista_precios"];
                    System.Collections.Generic.List<tplanmantenimiento> buscarPlanMantenimiento = context.tplanmantenimiento.ToList();
                    int totalFilas = workSheet.Dimension.End.Row;
                    System.Collections.Generic.List<tplanmantenimiento> buscarPlanes = context.tplanmantenimiento.ToList();

                    for (int i = workSheet.Dimension.Start.Row + 1; i <= workSheet.Dimension.End.Row; i++)
                    {
                        try
                        {

                            string codigo = workSheet.Cells[i, 1].Value.ToString();
                            string descripcion = workSheet.Cells[i, 2].Value.ToString();
                            string cantidad = workSheet.Cells[i, 3].Value.ToString();

                            var buscarReferencia = context.icb_referencia.FirstOrDefault(x => x.ref_codigo == codigo);
                            var buscarTempario = context.ttempario.FirstOrDefault(x => x.codigo == codigo);

                            if (buscarReferencia != null)
                            {
                                
                                tipoSeleccionado = "R";

                            }
                            else if (buscarTempario != null) {

                                tipoSeleccionado = "M";

                            }


                            //if (buscarReferencia == null && buscarTempario == null)
                            //{
                            //    TempData["mensaje_error"] =
                            //        "No se pudo cargar esa referencia" + codigo + " no existe, linea " + i;
                            //}

                            for (int kms = 4; kms <= 23; kms++)
                            {
                                int kilometrosTitulo = Convert.ToInt32(workSheet.Cells[1, kms].Value.ToString());

                                kilometrosTitulo = kilometrosTitulo * 1000;

                                tplanmantenimiento buscarIdPlanKms = buscarPlanes.Where(x => x.kilometraje == kilometrosTitulo).FirstOrDefault();
                                //busco la operacion vinculada a dicho plan
                                decimal horac = 0;
                                var oper = context.ttempario.Where(d => d.idplanm == buscarIdPlanKms.id).FirstOrDefault();
                                string kilometros = "";
                                if (oper != null)
                                {
                                    horac = Convert.ToDecimal(oper.HoraCliente,new CultureInfo("en-US"));
                                }
                                decimal numero;


                                if (workSheet.Cells[i, kms].Value != null)
                                {
                                    kilometros = workSheet.Cells[i, kms].Value.ToString();

                                    if (decimal.TryParse(kilometros, out numero))
                                    {
                                        kilometros = "AC";
                                    }

                                }
                                else {
                                    kilometros = "N/A";
                                }

                                tplanrepuestosmodelo buscarSiModeloExiste = context.tplanrepuestosmodelo.FirstOrDefault(x =>
                                    x.modgeneral == kitSeleccionado && x.referencia == codigo &&
                                    x.inspeccion == buscarIdPlanKms.id);

                                if (buscarSiModeloExiste != null)
                                {
                                    if (!string.IsNullOrWhiteSpace(kilometros))
                                    {
                                        buscarSiModeloExiste.cantidad = Convert.ToInt32(cantidad);
                                        //buscarSiModeloExiste.listaprecios = listaPrecios;
                                    }
                                    else
                                    {
                                        buscarSiModeloExiste.cantidad = 0;
                                        //buscarSiModeloExiste.listaprecios = listaPrecios;
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrWhiteSpace(kilometros) && tipoSeleccionado == "R")
                                    {

                                        var buscar = context.tplanrepuestosmodelo.Where(d => d.modgeneral == kitSeleccionado && d.referencia == codigo).Select(d => d.id).ToList();
                                        //var listaprecion = 0;

                                        context.tplanrepuestosmodelo.Add(new tplanrepuestosmodelo
                                        {
                                            modgeneral = kitSeleccionado,
                                            referencia = codigo,
                                            cantidad = Convert.ToDecimal(cantidad),
                                            inspeccion = buscarIdPlanKms.id,
                                            listaprecios = kilometros,
                                            tipo = tipoSeleccionado,
                                            estado = true,
                                        });

                                        guardar = context.SaveChanges();


                                        //if (buscar.Count > 0)
                                        //{
                                        //    foreach (var item in buscar)//id
                                        //    {

                                        //        var b = context.tplanrepuestosmodelo.Find(item);

                                        //        b.modgeneral = Convert.ToInt32(modeloGeneral);
                                        //        b.referencia = codigo;
                                        //        b.cantidad = Convert.ToDecimal(cantidad);
                                        //        b.listaprecios = kilometros;
                                        //        listaprecion += 1;

                                        //        context.Entry(b).State = EntityState.Modified;

                                        //        guardar = context.SaveChanges();

                                        //    }
                                        //}
                                        //else {

                                        //    context.tplanrepuestosmodelo.Add(new tplanrepuestosmodelo
                                        //    {
                                        //        modgeneral = kitSeleccionado,
                                        //        referencia = codigo,
                                        //        cantidad = Convert.ToDecimal(cantidad),
                                        //        inspeccion = buscarIdPlanKms.id,
                                        //        listaprecios = kilometros,
                                        //        tipo = tipoSeleccionado,
                                        //        estado = true,
                                        //    });

                                        //    guardar = context.SaveChanges();

                                        //}
                                    }
                                    else if (!string.IsNullOrWhiteSpace(kilometros) && tipoSeleccionado == "M") {

                                        var buscar = context.tplanrepuestosmodelo.Where(d => d.modgeneral == kitSeleccionado && d.tempario == codigo).Select(d => d.id).ToList();
                                        //var listaprecion = 0;

                                        context.tplanrepuestosmodelo.Add(new tplanrepuestosmodelo
                                        {
                                            modgeneral = kitSeleccionado,
                                            tempario = codigo,
                                            cantidad = Convert.ToDecimal(cantidad),
                                            inspeccion = buscarIdPlanKms.id,
                                            listaprecios = kilometros,
                                            tipo = tipoSeleccionado,
                                            estado = true,
                                        });

                                        guardar = context.SaveChanges();

                                        //if (buscar.Count > 0)
                                        //{
                                        //    foreach (var item in buscar)//id
                                        //    {

                                        //        var b = context.tplanrepuestosmodelo.Find(item);

                                        //        b.modgeneral = Convert.ToInt32(modeloGeneral);
                                        //        b.tempario = codigo;
                                        //        b.cantidad = Convert.ToDecimal(cantidad);
                                        //        b.listaprecios = kilometros;
                                        //        listaprecion += 1;

                                        //        context.Entry(b).State = EntityState.Modified;

                                        //        guardar = context.SaveChanges();

                                        //    }

                                        //}
                                        //else {

                                        //    context.tplanrepuestosmodelo.Add(new tplanrepuestosmodelo
                                        //    {
                                        //        modgeneral = kitSeleccionado,
                                        //        tempario = codigo,
                                        //        cantidad = Convert.ToDecimal(cantidad),
                                        //        inspeccion = buscarIdPlanKms.id,
                                        //        listaprecios = kilometros,
                                        //        tipo = tipoSeleccionado,
                                        //        estado = true,
                                        //    });

                                        //    guardar = context.SaveChanges();
                                        //}
                                        // Si el tipo de cargue es para mano de obra

                                    }
                                }
                            }

                            //}
                        }
                        catch (Exception ex)
                        {
                            if (ex is ArgumentOutOfRangeException || ex is FormatException)
                            {

                                excelfileRepuestos.InputStream.Close();
                                excelfileRepuestos.InputStream.Dispose();
                                System.IO.File.Delete(path);
                                TempData["mensaje_error"] =
                                    "Error al leer el archivo, verifique que los datos estan bien escritos,no se encontro el codigo en la base de datos, linea " + i;
                                ViewBag.modeloGeneral = new SelectList(context.vmodelog, "id", "Descripcion");
                                BuscarFavoritos(menu);
                                return View();
                            }
                        }
                    }

                    
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "El archivo se ha cargado correctamente";
                        return RedirectToAction("Create", "matricesMantenimiento", new { menu });
                    }

                    TempData["mensaje_error"] = "Error se encontro una o mas referencias que no estan registradas, verifique base de datos";
                }
            }



            ViewBag.modeloGeneral = new SelectList(context.vmodelog, "id", "Descripcion");
            ViewBag.modeloGeneral2 = new SelectList(context.vmodelog, "id", "Descripcion");
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult agregarMatriz(int? modeloGeneral, string referencia, string operacion, string cantidad, string[] kilometros)
        {

            var idrefencia = "";
            var idoperacion = "";
            var result = 0;
            decimal totalRepuestos = 0;
            decimal totalOperaciones = 0;
            decimal mano_obra = 0;

            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            var data = context.ttarifastaller.Where(d => d.bodega == bodegaActual && d.tipotarifa == 1).Select(d=> d.total_tarifa).FirstOrDefault();
            data = data != null ? data : 0;

            if (modeloGeneral != null || !string.IsNullOrWhiteSpace(referencia) || !string.IsNullOrWhiteSpace(operacion))
            {

                if (!string.IsNullOrWhiteSpace(referencia))
                {
                    string[] codigo = referencia.Split('|');
                    idrefencia = codigo[0];
                    var buscar = context.tplanrepuestosmodelo.Where(d => d.modgeneral == modeloGeneral && d.referencia == idrefencia).Select(d => d.id).ToList();


                    if (buscar.Count == 0)
                    {
                        int guardar = 0;
                        string tipoSeleccionado = "R";

                        for (int i = 1; i <= kilometros.Length; i++)
                        {
                            var j = i - 1;

                             totalRepuestos = context.icb_referencia.Where(d=> d.ref_codigo== idrefencia).Select(d=> d.precio_venta).FirstOrDefault();
                             totalRepuestos =totalRepuestos* Convert.ToDecimal(cantidad);
                             totalOperaciones = 0;
                             totalOperaciones= totalOperaciones* Convert.ToDecimal(cantidad);
                             mano_obra = 0;

                            context.tplanrepuestosmodelo.Add(new tplanrepuestosmodelo
                            {
                                modgeneral = Convert.ToInt32(modeloGeneral),
                                referencia = idrefencia,
                                cantidad = Convert.ToDecimal(cantidad),
                                //tiempo_mano_obra = TMO[j].ToString(),
                                tiempo_insumos = "0",
                                mano_obra = mano_obra,
                                insumos= 0,
                                valor_total= totalRepuestos + totalOperaciones + mano_obra+ 0,
                                inspeccion = i,
                                listaprecios = kilometros[j],
                                tipo = tipoSeleccionado,
                                estado = true,
                            });

                            guardar = context.SaveChanges();
                        }


                        if (guardar > 0)
                        {
                            result = 1;
                        }
                        else {

                            result = 0;

                        }

                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        int guardar = 0;
                        var listaprecion = 0;


                        foreach (var item in buscar)//id
                        {

                            totalRepuestos = context.icb_referencia.Where(d => d.ref_codigo == idrefencia).Select(d => d.precio_venta).FirstOrDefault();
                            totalRepuestos = totalRepuestos * Convert.ToDecimal(cantidad);
                            totalOperaciones = 0;
                            totalOperaciones = totalOperaciones * Convert.ToDecimal(cantidad);
                            mano_obra = 0;

                            var b = context.tplanrepuestosmodelo.Find(item);

                            b.modgeneral = Convert.ToInt32(modeloGeneral);
                            b.referencia = idrefencia;
                            b.cantidad = Convert.ToDecimal(cantidad);
                       //     b.tiempo_mano_obra = TMO[listaprecion].ToString();
                            b.mano_obra = mano_obra;
                            b.valor_total = totalRepuestos + totalOperaciones + mano_obra;
                            b.listaprecios = kilometros[listaprecion];
                            listaprecion += 1;
                            context.Entry(b).State = EntityState.Modified;

                            guardar = context.SaveChanges();                           
                        }



                        if (guardar > 0)
                        {
                            result = 1;
                        }
                        else
                        {

                            result = 0;

                        }

                        return Json(result, JsonRequestBehavior.AllowGet);
                        //actualizar
                    }


                }


                if (!string.IsNullOrWhiteSpace(operacion))
                {
                    string[] codigo = operacion.Split('|');
                    idoperacion = codigo[0];
                    var buscar = context.tplanrepuestosmodelo.Where(d => d.modgeneral == modeloGeneral && d.tempario == idoperacion).Select(d => d.id).ToList();

                    if (buscar.Count == 0)
                    {
                        int guardar = 0;
                        string tipoSeleccionado = "M";

                        for (int i = 1; i <= kilometros.Length; i++)
                        {

                            var j = i - 1;

                            totalRepuestos = 0;
                            var cant = cantidad.Replace(".", ",");
                            totalRepuestos = totalRepuestos * Convert.ToDecimal(cant);
                            totalOperaciones = Convert.ToDecimal(data);
                            totalOperaciones = totalOperaciones * Convert.ToDecimal(cant);
                            //mano_obra = (Convert.ToDecimal(data) * Convert.ToDecimal(TMO[j])) + Convert.ToDecimal(data);
                            mano_obra =Convert.ToDecimal(traerManoObra(idoperacion));

                            context.tplanrepuestosmodelo.Add(new tplanrepuestosmodelo
                            {
                                modgeneral = Convert.ToInt32(modeloGeneral),
                                tempario = idoperacion,
                                cantidad = Convert.ToDecimal(cant),
                                //tiempo_mano_obra = TMO[j].ToString(),
                                tiempo_insumos = "0",
                                mano_obra = mano_obra,
                                insumos = 0,
                                valor_total = totalRepuestos + totalOperaciones + mano_obra,
                                inspeccion = i,
                                listaprecios = kilometros[j],
                                tipo = tipoSeleccionado,
                                estado = true,
                            });

                            guardar = context.SaveChanges();

                        }


                        if (guardar > 0)
                        {
                            result = 1;
                        }
                        else
                        {

                            result = 0;

                        }

                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        int guardar = 0;
                        var listaprecion = 0;

                        foreach (var item in buscar)
                        {

                            totalRepuestos = 0;

                            var cant = cantidad.Replace(".", ",");
                            totalRepuestos = totalRepuestos * Convert.ToDecimal(cant);
                            totalOperaciones = Convert.ToDecimal(data);
                            totalOperaciones = totalOperaciones * Convert.ToDecimal(cant);
                            mano_obra = Convert.ToDecimal(traerManoObra(idoperacion));

                            var b = context.tplanrepuestosmodelo.Find(item);

                            b.tempario = idoperacion;
                            b.cantidad = Convert.ToInt32(cant);
                            //b.tiempo_mano_obra = TMO[listaprecion].ToString();
                            b.tiempo_insumos = "0";
                            b.mano_obra = mano_obra;
                            b.insumos = 0;
                            b.valor_total = totalRepuestos + totalOperaciones + mano_obra + 0;
                            b.listaprecios = kilometros[listaprecion];
                            listaprecion += 1;
                            context.Entry(b).State = EntityState.Modified;

                            guardar = context.SaveChanges();


                        }

                        if (guardar > 0)
                        {
                            result = 1;
                        }
                        else
                        {

                            result = 0;

                        }

                        return Json(result, JsonRequestBehavior.AllowGet);

                    }


                        
                    
                }

            }


            return Json(0, JsonRequestBehavior.AllowGet);


        }

        public JsonResult traerMatriz(string id) {

            var cod = Convert.ToInt32(id);
            var buscar = context.tplanrepuestosmodelo.Where(d => d.id == cod && d.referencia != null).Select(d => d.referencia).FirstOrDefault();
            var buscar2 = context.tplanrepuestosmodelo.Where(d => d.id == cod && d.tempario != null).Select(d => d.tempario).FirstOrDefault();

            if (buscar!= null)
            {
                var referencias = context.tplanrepuestosmodelo.Where(d => d.referencia == buscar).ToList();

                var items = referencias.GroupBy(d => d.referencia).Select(d => new itemPlan
                {

                    iditem = d.Select(e => e.id).FirstOrDefault(),
                    codigo = d.Select(e => e.referencia).FirstOrDefault(),
                    cantidad = d.Select(e => e.cantidad).FirstOrDefault(),
                }).ToList();

                foreach (var item in items)
                {

                    item.detalle = referencias.Select(d => new detalleitem
                    {
                        id = d.inspeccion,
                        valor = d.listaprecios,
                    }).ToList();

                }

                return Json(items, JsonRequestBehavior.AllowGet);

            }
            else
            if (buscar2 != null)
            {
                var referencias = context.tplanrepuestosmodelo.Where(d => d.tempario == buscar2).ToList();


                var items = referencias.GroupBy(d => d.referencia).Select(d => new itemPlan
                {

                    iditem = d.Select(e => e.id).FirstOrDefault(),
                    codigo = d.Select(e => e.tempario).FirstOrDefault(),
                    cantidad = d.Select(e => e.cantidad).FirstOrDefault(),
                }).ToList();

                foreach (var item in items)
                {

                    item.detalle = referencias.Select(d => new detalleitem
                    {
                        id = d.inspeccion,
                        valor = d.listaprecios,
                    }).ToList();

                }

                return Json(items, JsonRequestBehavior.AllowGet);

            }
            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult eliminarMatriz(string id) {

            if (!string.IsNullOrWhiteSpace(id))
            {
                var cod = Convert.ToInt32(id);

                var buscar=context.tplanrepuestosmodelo.Where(d => d.id == cod && d.referencia!=null).Select(d=> d.referencia).FirstOrDefault();
                var buscar2 =context.tplanrepuestosmodelo.Where(d => d.id == cod && d.tempario != null).Select(d => d.tempario).FirstOrDefault();
                int result = 0;


                if (buscar!= null)
                {
                    var referencias = context.tplanrepuestosmodelo.Where(d => d.referencia == buscar).Select(d => d.id).ToList();

                    foreach (var item in referencias)
                    {
                        tplanrepuestosmodelo dato = context.tplanrepuestosmodelo.Find(item);

                        context.Entry(dato).State = EntityState.Deleted;
                        int guardar = context.SaveChanges();

                        if (guardar > 0)
                        {
                            result = 1;
                        }
                        else
                        {
                            result = 0;
                        }
                    }
                    return Json(result, JsonRequestBehavior.AllowGet);

                }

                if (buscar2 != null)
                {
                    var operaciones = context.tplanrepuestosmodelo.Where(d => d.tempario == buscar2).Select(d=> d.id).ToList();

                    foreach (var item in operaciones)
                    {
                        tplanrepuestosmodelo dato = context.tplanrepuestosmodelo.Find(item);

                        context.Entry(dato).State = EntityState.Deleted;
                        int guardar = context.SaveChanges();

                        if (guardar > 0)
                        {
                            result = 1;
                        }
                        else
                        {
                            result = 0;
                        }
                    }
                    return Json(result, JsonRequestBehavior.AllowGet);

                }


            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public decimal traerPrecioReferencias(string codigo, decimal? cantidad)
        {
            decimal? precio = 0;
            decimal result = 0;

            var buscar = context.icb_referencia.Where(d => d.ref_codigo == codigo).FirstOrDefault();

            if (buscar != null)
            {
                precio = (buscar.precio_venta * cantidad);
                var iva = Convert.ToDecimal(buscar.por_iva, new CultureInfo("is-IS"));
                var iva2 = (precio.Value * iva) / 100;
                result = precio.Value+iva2;

            }
            else
            {
                result = 0;
            }
            return result;

        }

        public decimal traerPrecioOperaciones(string codigo, decimal? cantidad)
        {
            decimal precio = 0;
            decimal result = 0;

            var bodega = Convert.ToInt32(Session["user_bodega"]);
            decimal? buscar = context.ttarifastaller.Where(d => d.bodega == bodega && d.tipotarifa == 1).Select(d => d.total_tarifa).FirstOrDefault();
            //string buscar2 = context.ttempario.Where(d => d.codigo == codigo).Select(d => d.HoraCliente).FirstOrDefault();

            if (buscar != null)
            {
                precio = Convert.ToDecimal(buscar) * Convert.ToDecimal(cantidad);
                result = precio;

            }
            else
            {
                result = 0;
            }
            return result;

        }

        public double traerManoObra(string codigo)
        {
            decimal precio = 0;
            double result = 0;

            var bodega = Convert.ToInt32(Session["user_bodega"]);
            decimal? buscar = context.ttarifastaller.Where(d => d.bodega == bodega &&  d.tipotarifa==1).Select(d => d.total_tarifa).FirstOrDefault();
            string buscar2 = context.ttempario.Where(d => d.codigo == codigo).Select(d => d.HoraCliente).FirstOrDefault();

            var valorOperacion = Math.Round(Convert.ToDecimal(buscar));
            var horaCliente = buscar2.Replace(".", ",");

            if (buscar != null && buscar2 != null)
            {

                precio = valorOperacion * decimal.Parse(horaCliente);
                result = Math.Round(Convert.ToDouble(precio));

            }
            else {
                result = 0;
            }
            return result;

        }

        //public JsonResult buscarMatriz2(string codigo,string placa)
        //{

        //    var resultado = buscarMatriz(id);
        //}

        public JsonResult buscarMatriz(int? id) {
            var bodega = 0;
            if (Session["user_usuarioid"] != null)
            {
                bodega = Convert.ToInt32(Session["user_bodega"]);
            }
            if (id != null && bodega>0)
            {
                var buscar = (from p in context.tplanrepuestosmodelo
                              where p.estado == true && p.modgeneral == id //&& p.referencia!= null
                              select new Detalleplanmantenimiento
                              {
                                  id=p.id,
                                  modgeneral=p.modgeneral,
                                  Descripcion=p.vmodelog.Descripcion,
                                  referencia=p.referencia,
                                  ref_descripcion=p.icb_referencia.ref_descripcion,
                                  tempario=p.tempario,
                                  operacion=p.ttempario.operacion,
                                  cantidad=p.cantidad,
                                  inspeccion=p.inspeccion,
                                  listaprecios=p.listaprecios,
                                  tipo=p.tipo,                                
                                  //p.tiempo_mano_obra,
                                  //p.tiempo_mano_obra2,
                                  insumos=p.insumos,
                                  tiempo_insumos=p.tiempo_insumos,
                                  tiempo_insumos2=p.tiempo_insumos2,
                                  valor_total=p.valor_total,
                              }).ToList();

                
                //var total_mano_obra = context.vw_matriz_mantenimiento_1.Where(d => d.modgeneral == id).ToList();
                var totalmanoobra2 = new List<manoobraporplan>();
                var totalinsumos2 = new List<insumosporplan>();
                var totaltotal2 = new List<totalporplan>();

                //busco los planes de mantenimiento
                var planesmantenimiento = context.tplanmantenimiento.ToList();
                //calculo la tarifa del taller tarifataller
                decimal totaltarifa = 0;
                //calculo las horas de cada plan
                decimal horastarifa = 0;
                var tarifa = context.ttarifastaller.Where(d => d.bodega == bodega && d.tipotarifa == 1).FirstOrDefault();
                if (tarifa != null)
                {
                    totaltarifa = tarifa.total_tarifa != null ? tarifa.total_tarifa.Value : 0;
                }                
                //var total_insumos = context.vw_matriz_mantenimiento_2.Where(d => d.modgeneral == id).ToList();               
                var referencias=buscar.Where(d=> d.referencia != null).GroupBy(d=> d.referencia).Select(d=> new itemPlan {

                    modgeneral= d.Select(e => e.modgeneral).FirstOrDefault(),
                    inspeccion = d.Select(e => e.inspeccion).FirstOrDefault(),
                    iditem =d.Select(e=> e.id).FirstOrDefault(),
                    codigo= d.Key,
                    nombre= d.Select(e => e.ref_descripcion).FirstOrDefault(),
                    cantidad= d.Select(e => e.cantidad).FirstOrDefault(),
                   // mano_obra=d.Sum(e=> e.mano_obra),
                 //   tiempo_mano_obra = d.Sum(e => e.tiempo_mano_obra2),
                    //insumos = d.Sum(e => e.insumos),
                    //tiempo_insumos = d.Sum(e => e.tiempo_insumos2),
                    //valor_total = d.Sum(e => e.valor_total),
                }).ToList();

                var operaciones = buscar.Where(d => d.tempario != null).GroupBy(d => d.tempario).Select(d => new itemPlan
                {
                    modgeneral = d.Select(e => e.modgeneral).FirstOrDefault(),
                    inspeccion = d.Select(e => e.inspeccion).FirstOrDefault(),
                    iditem = d.Select(e => e.id).FirstOrDefault(),
                    codigo = d.Key,
                    nombre = d.Select(e => e.operacion).FirstOrDefault(),
                    cantidad = d.Select(e => e.cantidad).FirstOrDefault(),

                }).ToList();
                               
                foreach (var item in referencias)
                {

                    item.detalle = buscar.Where(d => d.referencia == item.codigo).Select(d => new detalleitem
                    {
                        id = d.inspeccion,
                        valor = d.listaprecios == "AC" ?  traerPrecioReferencias(d.referencia,d.cantidad).ToString("N2", new CultureInfo("is-IS")) : d.listaprecios,
                        valor2 = d.listaprecios == "AC" ? traerPrecioReferencias(d.referencia, d.cantidad): 0,
                    }).ToList();

                }

                foreach (var item2 in operaciones)
                {
                    item2.detalle = buscar.Where(d => d.tempario == item2.codigo).Select(d => new detalleitem
                    {
                        id = d.inspeccion,
                        valor = d.listaprecios == "AC" ? traerPrecioOperaciones(d.tempario,d.cantidad).ToString("N2",new CultureInfo("is-IS")) : d.listaprecios,
                        valor2 = d.listaprecios == "AC" ? traerPrecioOperaciones(d.tempario, d.cantidad) : 0,
                    }).ToList();

                }

                foreach (var item in planesmantenimiento)
                {
                    decimal precio = 0;

                    //de momento quemado el tipo de operacion 10.
                    var tempa = context.ttempario.Where(d => d.idplanm == item.id && d.tipooperacion == 10).FirstOrDefault();
                    if (tempa != null)
                    {
                        horastarifa = Convert.ToDecimal(tempa.HoraCliente, new CultureInfo("en-US"));
                        if (tempa.aplica_costo == true)
                        {
                            precio = horastarifa * totaltarifa;
                        }
                    }
                    

                    totalmanoobra2.Add(new manoobraporplan
                    {
                        idplan = item.id,
                        horas = horastarifa,
                        precio = precio,
                        precio2 = precio.ToString("N2", new CultureInfo("is-IS")),
                    });

                    //busco el porcentaje de insumo de ese plan
                    decimal porcentajeinsu = 0;
                    var insu = context.insumosplanmantenimiento.Where(d => d.idplan == item.id && d.modelogeneral == id).FirstOrDefault();
                    if (insu != null)
                    {
                        porcentajeinsu = insu.porcentaje;
                    }
                    var precioinsumo = precio * porcentajeinsu;
                    totalinsumos2.Add(new insumosporplan
                    {
                        idplan = item.id,
                        porcentaje = porcentajeinsu,
                        precio = precioinsumo,
                        precio2 = precioinsumo.ToString("N2", new CultureInfo("is-IS"))
                    });

                    //calculo el total de referencias para ese plan
                    var refer = referencias.Select(d =>new
                    {
                        plan=item.id,
                        precio=d.detalle.Where(e=>e.id==item.id && e.valor2>0).Sum(e=>e.valor2),
                    }).ToList();
                    var preciorefer = refer.Sum(e => e.precio);
                    //calculo el total de operaciones para ese plan
                    var oper = operaciones.Select(d => new
                    {
                        plan = d.detalle.Where(e => e.id == item.id).FirstOrDefault(),
                        precio = d.detalle.Where(e => e.id == item.id && e.valor2 > 0).Sum(e => e.valor2),
                    }).ToList();
                    var preciooper = oper.Sum(e => e.precio);

                    var totaltotalplan = precio + precioinsumo + preciorefer + preciooper;
                    totaltotal2.Add(new totalporplan
                    {
                        idplan = item.id,
                        precio = totaltotalplan,
                        precio2 = totaltotalplan.ToString("N2", new CultureInfo("is-IS"))
                    });
                }
                var valor_total_total = totaltotal2;
                var valor_total_mano_obra = totalmanoobra2.OrderBy(d => d.idplan).ToList();
                var valor_total_insumos = totalinsumos2.OrderBy(d => d.idplan).ToList();
                var data = new { referencias, operaciones, valor_total_mano_obra, valor_total_insumos, valor_total_total };

                return Json(data, JsonRequestBehavior.AllowGet);

            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public JsonResult guardarInsumos(listainsumos insumo)
        {
            if (ModelState.IsValid)
            {
                //busco el registro de insumos
                var existe = context.insumosplanmantenimiento.Where(d => d.modelogeneral == insumo.modelogeneral).ToList();
                if (existe.Count > 0)
                {
                    foreach (var item in existe)
                    {
                        context.Entry(item).State = EntityState.Deleted;
                    }
                    context.SaveChanges();
                }
                //selecciono los planes de mantenimiento
                var planes = context.tplanmantenimiento.ToList();
                var indice = 1;
                foreach (var item in planes)
                {
                    var porcen = "0";
                    switch (indice)
                    {
                        case 1:
                            porcen = insumo.insumo1;
                            break;
                        case 2:
                            porcen = insumo.insumo2;
                            break;
                        case 3:
                            porcen = insumo.insumo3;
                            break;
                        case 4:
                            porcen = insumo.insumo4;
                            break;
                        case 5:
                            porcen = insumo.insumo5;
                            break;
                        case 6:
                            porcen = insumo.insumo6;
                            break;
                        case 7:
                            porcen = insumo.insumo7;
                            break;
                        case 8:
                            porcen = insumo.insumo8;
                            break;
                        case 9:
                            porcen = insumo.insumo9;
                            break;
                        case 10:
                            porcen = insumo.insumo10;
                            break;
                        case 11:
                            porcen = insumo.insumo11;
                            break;
                        case 12:
                            porcen = insumo.insumo12;
                            break;
                        case 13:
                            porcen = insumo.insumo13;
                            break;
                        case 14:
                            porcen = insumo.insumo14;
                            break;
                        case 15:
                            porcen = insumo.insumo15;
                            break;
                        case 16:
                            porcen = insumo.insumo16;
                            break;
                        case 17:
                            porcen = insumo.insumo17;
                            break;
                        case 18:
                            porcen = insumo.insumo18;
                            break;
                        case 19:
                            porcen = insumo.insumo19;
                            break;
                        case 20:
                            porcen = insumo.insumo20;
                            break;
                        default:
                            break;
                    }
                    var conversionporcentaje = Convert.ToDecimal(porcen, new CultureInfo("is-IS"));
                    var planinsumo = new insumosplanmantenimiento
                    {
                        modelogeneral=insumo.modelogeneral.Value,
                        idplan=item.id,
                        porcentaje=conversionporcentaje,
                    };
                    context.insumosplanmantenimiento.Add(planinsumo);
                    context.SaveChanges();
                    indice = indice + 1;
                }
                return Json(1);
            }
            else
            {
                return Json(0);
            }
        }

        public JsonResult buscarMatrices() {

            var matriz = (from p in context.tplanrepuestosmodelo
                          where p.estado == true
                          select new
                          {
                              p.id,
                              p.tipo,
                              p.inspeccion,
                              p.referencia,
                              p.tempario,
                              p.cantidad,
                              p.listaprecios,
                              p.estado,
                          }).ToList();

            var data = matriz.Select(d => new
            {
                d.id,
                tipo = d.tipo,
                inspeccion = d.inspeccion,
                referencia = d.referencia,
                tempario = d.tempario,
                cantidad = d.cantidad,
                precio = d.listaprecios,
                estado = d.estado,
            });

            return Json(data, JsonRequestBehavior.AllowGet);

        }

        public JsonResult cambiarEstado(int? id) {

            int data = 0;

            if (id != null)
            {
                var b = context.tplanrepuestosmodelo.Find(id);

                var plan = context.tplanrepuestosmodelo.Where(d => d.id == id).FirstOrDefault();

                if (plan.estado == false)
                {
                    b.estado = true;
                    context.Entry(b).State = EntityState.Modified;
                }
                else {
                    b.estado = false;
                    context.Entry(b).State = EntityState.Modified;
                }

                int resultado = context.SaveChanges();

                if (resultado > 0)
                {
                    data = 1;
                }
                else
                {
                    data = 0;
                }

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else {

                return Json(0, JsonRequestBehavior.AllowGet);

            }

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

    public class itemPlan
    {

        public int? iditem { get; set; }
        public int modgeneral { get; set; }
        public int? inspeccion { get; set; }
        public string codigo { get; set; }
        public string nombre { get; set; }
        public decimal? cantidad { get; set; }
        public List<detalleitem> detalle { get; set; }
    }
    public class detalleitem
    {
        public int? id { get; set; }
        public string valor { get; set; }
        public decimal valor2 { get; set; }
    }
    public class totales_item
    {
        public int? inspeccion { get; set; }
        public int modelo { get; set; }
        public decimal? mano_obra { get; set; }
        public decimal? tiempo_mano_obra { get; set; }
        public decimal? insumos { get; set; }
        public decimal? tiempo_insumos { get; set; }
        public decimal? valor_total { get; set; }

    }

}
