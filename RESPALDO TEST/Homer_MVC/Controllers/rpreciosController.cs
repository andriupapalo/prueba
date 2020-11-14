using Homer_MVC.IcebergModel;
using Homer_MVC.ViewModels.medios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class rpreciosController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        // GET: rprecios
        private static Expression<Func<vw_archListaPrecios,
            string>> GetColumnName(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(vw_archListaPrecios), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<vw_archListaPrecios, string>> lambda = Expression.Lambda<Func<vw_archListaPrecios,
                string>>(menuProperty, menu);
            return lambda;
        }

        public ActionResult Index(int? menu)
        {
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult GuardarArchivo(HttpPostedFileBase txtfile, string precioLabel, string precioSelect)
        {
            IDictionary<string, string> respuesta = new Dictionary<string, string>();
            object usuario = Session["user_usuarioid"];
            if (usuario == null)
            {
                respuesta.Add("valor", "-1");
                respuesta.Add("clase", "danger");
                respuesta.Add("mensaje", "Su sesión ha expirado, no es posible hacer el cargue del archivo");
                return Json(respuesta);
            }

            //Consulta de parametros por defecto en la creacion de la referencia
            icb_sysparameter tipo_id_consulta = db.icb_sysparameter.Where(d => d.syspar_cod == "P112").FirstOrDefault();
            int tipo_id =
                tipo_id_consulta != null
                    ? Convert.ToInt32(tipo_id_consulta.syspar_value)
                    : 3; //valor por defecto desarrollo

            icb_sysparameter linea_id_consulta = db.icb_sysparameter.Where(d => d.syspar_cod == "P113").FirstOrDefault();
            string linea_id =
                linea_id_consulta != null ? linea_id_consulta.syspar_value : "01"; //valor por defecto desarrollo

            icb_sysparameter proveedor_consulta = db.icb_sysparameter.Where(d => d.syspar_cod == "P114").FirstOrDefault();
            string proveedor =
                proveedor_consulta != null ? proveedor_consulta.syspar_value : "48"; //valor por defecto desarrollo
            try
            {
                string path = Server.MapPath("~/Content/" + txtfile.FileName);
                // Validacion para cuando el archivo esta en uso y no puede ser usado desde visual 

                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }

                txtfile.SaveAs(path);

                using (StreamReader objReader = new StreamReader(path))
                {
                    string lineasPrecio = "";
                    int contadorReferenciaNok = 0;
                    string lineasNok = "";
                    int contadorPrecioNok = 0;
                    int registros = 0;
                    if (objReader.Peek() <= -1)
                    {
                        respuesta.Add("valor", "0");
                        respuesta.Add("clase", "danger");
                        respuesta.Add("mensaje",
                            "El archivo se encuentra vacio o no cumple con el formato establecido");
                        return Json(respuesta);
                    }

                    archListaPrecios archivoTransaccion = new archListaPrecios
                    {
                        nombre = txtfile.FileName,
                        listaPrecio = precioLabel,
                        fecha = DateTime.Now,
                        items = registros,
                        userid_creacion = Convert.ToInt32(usuario)
                    };
                    db.archListaPrecios.Add(archivoTransaccion);
                    db.SaveChanges();
                    int ultimoArchivo = archivoTransaccion.id;
                    bool encabezado = true;
                    while (objReader.Peek() > -1)
                    {
                        string linea = objReader.ReadLine();
                        if (encabezado)
                        {
                            encabezado = false;
                            continue;
                        }

                        registros++;
                        //Codigo de la referencia
                        int i = 0;
                        string codigo = linea.Substring(0, 18);
                        while (linea.Substring(i, 1) == "0")
                        {
                            i++;
                            codigo = linea.Substring(i, 18 - i);
                        }

                        //Descripción de la referencia 
                        string descripcion = linea.Substring(28, 25).Trim();

                        //Precio de venta
                        string c = linea.Substring(54, 12).Trim();
                        decimal precio = c != null
                            ? Convert.ToDecimal(linea.Substring(54, 12).Trim(), CultureInfo.InvariantCulture)
                            : 0;

                        //Precio compra
                        string r = linea.Substring(150, 15).Trim();
                        decimal costo = !string.IsNullOrEmpty(linea.Substring(150, 15).Trim())
                            ? Convert.ToDecimal(linea.Substring(150, 15).Trim(), CultureInfo.InvariantCulture)
                            : 0;

                        //costo de emergencia
                        decimal costo_emergencia = linea.Substring(165, 13).Trim() != null
                            ? Convert.ToDecimal(linea.Substring(165, 13).Trim(), CultureInfo.InvariantCulture)
                            : 0;

                        //Unidad de medida
                        string unidad_medida = linea.Substring(147, 3).Trim();
                        if (unidad_medida.ToUpper() == "PZA")
                        {
                            unidad_medida = "UND";
                        }
                        else
                        {
                            unidad_medida = "OTRO";
                        }

                        //Valor IVA
                        string valor_iva = linea.Substring(179, 2).Trim();
                        if (valor_iva.ToUpper() == "UD" || valor_iva.ToUpper() == "XD")
                        {
                            valor_iva = "19";
                        }
                        else
                        {
                            valor_iva = "0";
                        }

                        icb_referencia buscarReferencia = db.icb_referencia.FirstOrDefault(x => x.ref_codigo == codigo);
                        bool referencia_creada = false;
                        if (buscarReferencia == null)
                        {
                            referencia_creada = true;
                            //insertarLog(ultimoArchivo, codigo, precio, costo, costo_emergencia, "La referencia no se encuentra en la base de datos");
                            //lineasNok += codigo + ", ";
                            //contadorReferenciaNok++;
                            icb_referencia referencia = new icb_referencia
                            {
                                ref_codigo = codigo,
                                ref_descripcion = descripcion,
                                ref_stock = 0, //Se deja este valor en 0 solititud 2019-08-26 jairo mateus
                                ref_estado = true,
                                ref_usuario_creacion = Convert.ToInt32(usuario),
                                ref_fecha_creacion = DateTime.Now,
                                ref_valor_unitario = precio,
                                ref_cantidad_min = 0, //Se deja este valor en 0 solititud 2019-08-26 jairo mateus
                                ref_cantidad_max = 0, //Se deja este valor en 0 solititud 2019-08-26 jairo mateus
                                ref_valor_total = 0, //Se deja este valor en 0 solititud 2019-08-26 jairo mateus
                                por_iva = Convert.ToInt32(valor_iva),
                                por_iva_compra = Convert.ToInt32(valor_iva),
                                costo_unitario = costo,
                                manejo_inv = true,
                                unidad_medida = unidad_medida,
                                costo_anterior = 0, //Se deja este valor en 0 solititud 2019-08-26 jairo mateus
                                costo_emergencia = costo_emergencia,
                                por_dscto = 0, //Se deja este valor en 0 solititud 2019-08-26 jairo mateus
                                por_dscto_max = 0, //Se deja este valor en 0 solititud 2019-08-26 jairo mateus
                                precio_venta = precio,
                                precio_alterno = precio, //Se deja este valor en 0 solititud 2019-08-26 jairo mateus
                                precio_garantia = costo, //Se deja este valor en 0 solititud 2019-08-26 jairo mateus
                                precio_diesel = 0, //Se deja este valor en 0 solititud 2019-08-26 jairo mateus
                                modulo = "R", // Este codigo corresponde a las referencias de respuestos
                                tipo_id = tipo_id,
                                linea_id = linea_id,
                                proveedor_ppal = Convert.ToInt32(proveedor),
                                clasificacion_ABC = "C" //valor por defecto a solicitud del 2019-08-28 jairo mateus
                            };
                            db.icb_referencia.Add(referencia);
                            db.SaveChanges();
                        }

                        if (precio != 0 || referencia_creada)
                        {
                            int lista_precios = db.cargarListaPrecios(precio, codigo, costo, costo_emergencia,
                                Convert.ToInt32(usuario), Convert.ToInt32(precioSelect));
                            detalleArchListaPrecios detalleTransaccion = new detalleArchListaPrecios
                            {
                                idArch = ultimoArchivo,
                                referencia = codigo,
                                precio = precio,
                                costo = costo,
                                costoEmergencia = costo_emergencia
                            };
                            db.detalleArchListaPrecios.Add(detalleTransaccion);
                        }
                        else
                        {
                            insertarLog(ultimoArchivo, codigo, precio, costo, costo_emergencia,
                                "La referencia no tiene precio");
                            lineasPrecio += codigo + ", ";
                            contadorPrecioNok++;
                        }
                    }

                    archListaPrecios ObjArchivo = db.archListaPrecios.FirstOrDefault(x => x.id == ultimoArchivo);
                    ObjArchivo.items = registros;
                    ObjArchivo.registros_erroneos = contadorReferenciaNok + contadorPrecioNok;
                    ObjArchivo.registros_ingresados = registros - (contadorReferenciaNok + contadorPrecioNok);
                    db.Entry(ObjArchivo).State = EntityState.Modified;
                    if (lineasNok != "")
                    {
                        int a = lineasNok.Length;
                        lineasNok = lineasNok.Substring(0, a - 2);
                        respuesta.Add("sinReferencia", lineasNok);
                        respuesta.Add("mensajeRerencia",
                            "Las siguientes referencias no fueron registradas porque no se encuentran en la base de datos: " +
                            lineasNok);
                    }

                    if (lineasPrecio != "")
                    {
                        int b = lineasPrecio.Length;
                        lineasPrecio = lineasPrecio.Substring(0, b - 2);
                        respuesta.Add("sinPrecio", lineasPrecio);
                        respuesta.Add("mensajePrecio",
                            "Las siguientes referencias no fueron registradas porque no tienen precio: " +
                            lineasPrecio);
                    }

                    respuesta.Add("valor", "1");
                    respuesta.Add("clase", "success");
                    respuesta.Add("mensaje", "El archivo Fue procesado correctamente!");
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception dbEx)
                    {
                        Exception raise = dbEx;
                    }

                    return Json(respuesta);
                }
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
                        // raise a new exception nesting
                        // the current instance as InnerException
                        raise = new InvalidOperationException(message, raise);
                    }
                }

                respuesta.Add("valor", "1");
                respuesta.Add("clase", "success");
                respuesta.Add("mensaje", "Error: " + raise);
                return Json(respuesta);
            }
        }

        private void insertarLog(int ultimoArchivo, string codigo, decimal precio, decimal costo,
            decimal costo_emergencia, string mensaje_error)
        {
            log_lista_precios logTransaccion = new log_lista_precios
            {
                idArch = ultimoArchivo,
                referencia = codigo,
                precio = precio,
                costo = costo,
                costoEmergencia = costo_emergencia,
                error = mensaje_error
            };
            db.log_lista_precios.Add(logTransaccion);
        }

        // POST: rprecios/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public JsonResult BuscarDatos(string filtroGeneral)
        {
            string draw = Request.Form.GetValues("draw").FirstOrDefault();
            string start = Request.Form.GetValues("start").FirstOrDefault();
            string length = Request.Form.GetValues("length").FirstOrDefault();
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

            Expression<Func<vw_archListaPrecios, bool>> predicado = PredicateBuilder.True<vw_archListaPrecios>();
            Expression<Func<vw_archListaPrecios, bool>> predicado3 = PredicateBuilder.False<vw_archListaPrecios>();


            if (!string.IsNullOrWhiteSpace(filtroGeneral))
            {
                predicado3 = predicado3.Or(d => 1 == 1 && d.nombre.ToString().Contains(filtroGeneral));
                predicado3 = predicado3.Or(d => 1 == 1 && d.fecha.ToString().Contains(filtroGeneral));
                predicado3 = predicado3.Or(d => 1 == 1 && d.user_nombre.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.registros_ingresados.ToString().Contains(filtroGeneral));
                predicado3 = predicado3.Or(d => 1 == 1 && d.registros_erroneos.ToString().Contains(filtroGeneral));
                predicado3 = predicado3.Or(d => 1 == 1 && d.items.ToString().Contains(filtroGeneral));
                predicado = predicado.And(predicado3);
            }

            int registrostotales = db.vw_archListaPrecios.Where(predicado).Count();
            //si el ordenamiento es ascendente o descendente es distinto
            if (pageSize == -1)
            {
                pageSize = registrostotales;
            }

            if (sortColumnDir == "asc")
            {
                List<vw_archListaPrecios> query2 = db.vw_archListaPrecios.Where(predicado).OrderBy(GetColumnName(sortColumn).Compile())
                    .Skip(skip).Take(pageSize).ToList();
                var query = query2.Select(d => new
                {
                    d.id,
                    d.nombre,
                    d.fecha,
                    d.user_nombre,
                    d.registros_ingresados,
                    d.registros_erroneos,
                    d.items
                }).ToList();
                int contador = query.Count();
                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<vw_archListaPrecios> query2 = db.vw_archListaPrecios.Where(predicado)
                    .OrderByDescending(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();

                var query = query2.Select(d => new
                {
                    d.id,
                    d.nombre,
                    d.fecha,
                    d.user_nombre,
                    d.registros_ingresados,
                    d.registros_erroneos,
                    d.items
                }).ToList();


                int contador = query.Count();
                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult buscarDetalle(int id)
        {
            var data = (from a in db.log_lista_precios
                        where a.idArch == id
                        select new
                        {
                            nombre = a.referencia,
                            a.precio,
                            a.costo,
                            a.costoEmergencia,
                            a.error
                        }).ToList();

            JsonResult jsonResult = Json(data, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
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