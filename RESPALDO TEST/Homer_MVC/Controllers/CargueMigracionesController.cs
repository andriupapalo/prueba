//Ultima Modificación 2019-03-13

using Homer_MVC.IcebergModel;
using Homer_MVC.ViewModels.medios;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class CargueMigracionesController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: CargueMigraciones
        private static Expression<Func<vw_historial_migraciones,
            string>> GetColumnName(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(vw_historial_migraciones), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<vw_historial_migraciones, string>> lambda = Expression.Lambda<Func<vw_historial_migraciones,
                string>>(menuProperty, menu);
            return lambda;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CrearCargue()
        {
            traerMigraciones();
            return View();
        }

        public void traerMigraciones()
        {
            ViewBag.migraciones = new SelectList(context.icb_migracion, "id", "nombre_migracion");
        }

        public JsonResult GuardarArchivo(HttpPostedFileBase txtfile, int migracion)
        {
            object usuario = Session["user_usuarioid"];
            if (usuario == null)
            {
                usuario = 5;
            }

            IDictionary<string, string> respuesta = new Dictionary<string, string>();
            var columnas = (from c in context.vw_parametrizaciones_creadas
                            orderby c.id_columna
                            where c.id_migracion == migracion && c.col_agregada
                            select new
                            {
                                c.nombre_migracion,
                                c.nombre_columna,
                                tabla_origen = c.nombre_tabla,
                                columna_origen = c.columna_tabla,
                                c.campo_null,
                                c.tipo_dato,
                                c.tabla_referencia,
                                c.columna_referencia,
                                c.tipo_constraint
                            }).ToList();
            try
            {
                string path = Server.MapPath("~/Content/Migraciones/" + DateTime.Now.Year + "_" + DateTime.Now.Month +
                                          "_" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute +
                                          "_" + DateTime.Now.Second + "_" + txtfile.FileName);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }

                txtfile.SaveAs(path);
                using (StreamReader lector = new StreamReader(path))
                {
                    int fila = 0;
                    string sql_insert = "";
                    string sql_values = "";
                    string tabla = "";
                    string nombre_migracion = "";
                    IDictionary<int, string> llaves = new Dictionary<int, string>();
                    if (lector.Peek() <= -1)
                    {
                        respuesta = insertarLog(nombre_migracion, usuario.ToString(), tabla,
                            "Cargue de migraciones -> Archivo vacio",
                            "El archivo a cargar se encuentra vacio o no corresponde con el formato especificado", fila,
                            txtfile.FileName);
                        return Json(respuesta);
                    }
                    else
                    {
                        while (lector.Peek() > -1)
                        {
                            fila++;
                            string linea = lector.ReadLine();
                            string[] datos_array = linea.Split('|');
                            if (columnas.Count == datos_array.Length)
                            {
                                int iteracion = 0;
                                foreach (var c in columnas)
                                {
                                    tabla = c.tabla_origen;
                                    nombre_migracion = c.nombre_migracion;
                                    if (iteracion == 0)
                                    {
                                        sql_insert = "SET DATEFORMAT ymd; insert into " + c.tabla_origen + "(" +
                                                     c.columna_origen;
                                    }
                                    else
                                    {
                                        sql_insert += " ," + c.columna_origen;
                                    }

                                    for (int i = iteracion; i < datos_array.Length; i++)
                                    {
                                        if (datos_array[i] == null || datos_array[i] == "")
                                        {
                                            if (c.campo_null)
                                            {
                                                if (iteracion == 0 && fila == 1)
                                                {
                                                    sql_values += "(null";
                                                }
                                                else if (iteracion == 0)
                                                {
                                                    sql_values += ",(null";
                                                }
                                                else
                                                {
                                                    sql_values += " ,null";
                                                }

                                                if (iteracion == datos_array.Length - 1)
                                                {
                                                    sql_values += ")";
                                                }

                                                iteracion++;
                                                break;
                                            }

                                            respuesta = insertarLog(nombre_migracion, usuario.ToString(), tabla,
                                                "Cargue de migraciones -> valores nulos en el archivo",
                                                "La columna: " + c.nombre_columna + " no permite valores vacios", fila,
                                                txtfile.FileName);
                                            return Json(respuesta);
                                        }
                                        else
                                        {
                                            if (c.tipo_dato == "integer")
                                            {
                                                if (int.TryParse(datos_array[i], out int numero))
                                                {
                                                    if (c.tipo_constraint != null)
                                                    {
                                                        IDictionary<string, string> constraint =
                                                            new Dictionary<string, string>();
                                                        constraint = validaConstraint(sql_values, datos_array.Length,
                                                            iteracion, fila, c.tipo_constraint, c.columna_origen,
                                                            c.tabla_origen, c.nombre_columna, c.columna_referencia,
                                                            c.tabla_referencia, datos_array[i]);
                                                        if (constraint["valor"] == "1")
                                                        {
                                                            sql_values = constraint["sql_values"];
                                                            if (c.tipo_constraint != "FOREIGN KEY")
                                                            {
                                                                bool existe = llaves.Any(b =>
                                                                    b.Value != null && b.Value ==
                                                                    c.columna_origen + "|" + datos_array[i]);
                                                                if (!existe)
                                                                {
                                                                    llaves.Add(fila,
                                                                        c.columna_origen + "|" + datos_array[i]);
                                                                    iteracion++;
                                                                    break;
                                                                }

                                                                respuesta = insertarLog(nombre_migracion,
                                                                    usuario.ToString(), tabla,
                                                                    "Cargue de migraciones -> Valores repedidos en el archivo de tipo entero",
                                                                    "El valor: " + datos_array[i] +
                                                                    " se encuentra duplicado en el archivo a cargar", fila,
                                                                    txtfile.FileName);
                                                                return Json(respuesta);
                                                            }

                                                            iteracion++;
                                                            break;
                                                        }

                                                        respuesta = insertarLog(nombre_migracion, usuario.ToString(), tabla,
                                                            "Cargue de migraciones -> Constraint tipo entero",
                                                            constraint["mensaje"], fila, txtfile.FileName);
                                                        return Json(respuesta);
                                                    }

                                                    if (iteracion == 0 && fila == 1)
                                                    {
                                                        sql_values += "('" + datos_array[i] + "'";
                                                    }
                                                    else if (iteracion == 0)
                                                    {
                                                        sql_values += ",('" + datos_array[i] + "'";
                                                    }
                                                    else
                                                    {
                                                        sql_values += " ,'" + datos_array[i] + "'";
                                                    }

                                                    if (iteracion == datos_array.Length - 1)
                                                    {
                                                        sql_values += ")";
                                                    }

                                                    iteracion++;
                                                    break;
                                                }

                                                respuesta = insertarLog(nombre_migracion, usuario.ToString(), tabla,
                                                    "Cargue de migraciones -> tipo dato entero",
                                                    "El tipo de dato de la columna: " + c.nombre_columna +
                                                    " no cohincide con el parametrizado", fila, txtfile.FileName);
                                                return Json(respuesta);
                                            }

                                            if (c.tipo_dato == "time")
                                            {
                                                if (DateTime.TryParseExact(datos_array[i], "HH:mm:ss", null,
                                                    DateTimeStyles.None, out DateTime fecha))
                                                {
                                                    if (c.tipo_constraint != null)
                                                    {
                                                        IDictionary<string, string> constraint =
                                                            new Dictionary<string, string>();
                                                        constraint = validaConstraint(sql_values, datos_array.Length,
                                                            iteracion, fila, c.tipo_constraint, c.columna_origen,
                                                            c.tabla_origen, c.nombre_columna, c.columna_referencia,
                                                            c.tabla_referencia, datos_array[i]);
                                                        if (constraint["valor"] == "1")
                                                        {
                                                            sql_values = constraint["sql_values"];
                                                            if (c.tipo_constraint != "FOREIGN KEY")
                                                            {
                                                                bool existe = llaves.Any(b =>
                                                                    b.Value != null && b.Value ==
                                                                    c.columna_origen + "|" + datos_array[i]);
                                                                if (!existe)
                                                                {
                                                                    llaves.Add(fila,
                                                                        c.columna_origen + "|" + datos_array[i]);
                                                                    iteracion++;
                                                                    break;
                                                                }

                                                                respuesta = insertarLog(nombre_migracion,
                                                                    usuario.ToString(), tabla,
                                                                    "Cargue de migraciones -> Valores repedidos en el archivo de tipo time",
                                                                    "El valor: " + datos_array[i] +
                                                                    " se encuentra duplicado en el archivo a cargar", fila,
                                                                    txtfile.FileName);
                                                                return Json(respuesta);
                                                            }

                                                            iteracion++;
                                                            break;
                                                        }

                                                        respuesta = insertarLog(nombre_migracion, usuario.ToString(), tabla,
                                                            "Cargue de migraciones -> Constraint tipo time",
                                                            constraint["mensaje"], fila, txtfile.FileName);
                                                        return Json(respuesta);
                                                    }

                                                    if (iteracion == 0 && fila == 1)
                                                    {
                                                        sql_values += "('" + datos_array[i] + "'";
                                                    }
                                                    else if (iteracion == 0)
                                                    {
                                                        sql_values += ",('" + datos_array[i] + "'";
                                                    }
                                                    else
                                                    {
                                                        sql_values += " ,'" + datos_array[i] + "'";
                                                    }

                                                    if (iteracion == datos_array.Length - 1)
                                                    {
                                                        sql_values += ")";
                                                    }

                                                    iteracion++;
                                                    break;
                                                }

                                                respuesta = insertarLog(nombre_migracion, usuario.ToString(), tabla,
                                                    "Cargue de migraciones -> tipo dato time",
                                                    "El tipo de dato de la columna: " + c.nombre_columna +
                                                    " no cohincide con el parametrizado", fila, txtfile.FileName);
                                                return Json(respuesta);
                                            }

                                            if (c.tipo_dato == "datetime")
                                            {
                                                if (DateTime.TryParseExact(datos_array[i], "yyyy-MM-dd HH:mm:ss", null,
                                                    DateTimeStyles.None, out DateTime fecha))
                                                {
                                                    if (c.tipo_constraint != null)
                                                    {
                                                        IDictionary<string, string> constraint =
                                                            new Dictionary<string, string>();
                                                        constraint = validaConstraint(sql_values, datos_array.Length,
                                                            iteracion, fila, c.tipo_constraint, c.columna_origen,
                                                            c.tabla_origen, c.nombre_columna, c.columna_referencia,
                                                            c.tabla_referencia, datos_array[i]);
                                                        if (constraint["valor"] == "1")
                                                        {
                                                            sql_values = constraint["sql_values"];
                                                            if (c.tipo_constraint != "FOREIGN KEY")
                                                            {
                                                                bool existe = llaves.Any(b =>
                                                                    b.Value != null && b.Value ==
                                                                    c.columna_origen + "|" + datos_array[i]);
                                                                if (!existe)
                                                                {
                                                                    llaves.Add(fila,
                                                                        c.columna_origen + "|" + datos_array[i]);
                                                                    iteracion++;
                                                                    break;
                                                                }

                                                                respuesta = insertarLog(nombre_migracion,
                                                                    usuario.ToString(), tabla,
                                                                    "Cargue de migraciones -> Valores repedidos en el archivo de tipo datetime",
                                                                    "El valor: " + datos_array[i] +
                                                                    " se encuentra duplicado en el archivo a cargar", fila,
                                                                    txtfile.FileName);
                                                                return Json(respuesta);
                                                            }

                                                            iteracion++;
                                                            break;
                                                        }

                                                        respuesta = insertarLog(nombre_migracion, usuario.ToString(), tabla,
                                                            "Cargue de migraciones -> Constraint tipo datetime",
                                                            constraint["mensaje"], fila, txtfile.FileName);
                                                        return Json(respuesta);
                                                    }

                                                    if (iteracion == 0 && fila == 1)
                                                    {
                                                        sql_values += "('" + datos_array[i] + "'";
                                                    }
                                                    else if (iteracion == 0)
                                                    {
                                                        sql_values += ",('" + datos_array[i] + "'";
                                                    }
                                                    else
                                                    {
                                                        sql_values += " ,'" + datos_array[i] + "'";
                                                    }

                                                    if (iteracion == datos_array.Length - 1)
                                                    {
                                                        sql_values += ")";
                                                    }

                                                    iteracion++;
                                                    break;
                                                }

                                                respuesta = insertarLog(nombre_migracion, usuario.ToString(), tabla,
                                                    "Cargue de migraciones -> tipo dato datetime",
                                                    "El tipo de dato de la columna: " + c.nombre_columna +
                                                    " no cohincide con el parametrizado", fila, txtfile.FileName);
                                                return Json(respuesta);
                                            }

                                            if (c.tipo_dato == "date")
                                            {
                                                if (DateTime.TryParseExact(datos_array[i], "yyyy-MM-dd", null,
                                                    DateTimeStyles.None, out DateTime fecha))
                                                {
                                                    if (c.tipo_constraint != null)
                                                    {
                                                        IDictionary<string, string> constraint =
                                                            new Dictionary<string, string>();
                                                        constraint = validaConstraint(sql_values, datos_array.Length,
                                                            iteracion, fila, c.tipo_constraint, c.columna_origen,
                                                            c.tabla_origen, c.nombre_columna, c.columna_referencia,
                                                            c.tabla_referencia, datos_array[i]);
                                                        if (constraint["valor"] == "1")
                                                        {
                                                            sql_values = constraint["sql_values"];
                                                            if (c.tipo_constraint != "FOREIGN KEY")
                                                            {
                                                                bool existe = llaves.Any(b =>
                                                                    b.Value != null && b.Value ==
                                                                    c.columna_origen + "|" + datos_array[i]);
                                                                if (!existe)
                                                                {
                                                                    llaves.Add(fila,
                                                                        c.columna_origen + "|" + datos_array[i]);
                                                                    iteracion++;
                                                                    break;
                                                                }

                                                                respuesta = insertarLog(nombre_migracion,
                                                                    usuario.ToString(), tabla,
                                                                    "Cargue de migraciones -> Valores repedidos en el archivo de tipo date",
                                                                    "El valor: " + datos_array[i] +
                                                                    " se encuentra duplicado en el archivo a cargar", fila,
                                                                    txtfile.FileName);
                                                                return Json(respuesta);
                                                            }

                                                            iteracion++;
                                                            break;
                                                        }

                                                        respuesta = insertarLog(nombre_migracion, usuario.ToString(), tabla,
                                                            "Cargue de migraciones -> Constraint tipo date",
                                                            constraint["mensaje"], fila, txtfile.FileName);
                                                        return Json(respuesta);
                                                    }

                                                    if (iteracion == 0 && fila == 1)
                                                    {
                                                        sql_values += "('" + datos_array[i] + "'";
                                                    }
                                                    else if (iteracion == 0)
                                                    {
                                                        sql_values += ",('" + datos_array[i] + "'";
                                                    }
                                                    else
                                                    {
                                                        sql_values += " ,'" + datos_array[i] + "'";
                                                    }

                                                    if (iteracion == datos_array.Length - 1)
                                                    {
                                                        sql_values += ")";
                                                    }

                                                    iteracion++;
                                                    break;
                                                }

                                                respuesta = insertarLog(nombre_migracion, usuario.ToString(), tabla,
                                                    "Cargue de migraciones -> tipo dato date",
                                                    "El tipo de dato de la columna: " + c.nombre_columna +
                                                    " no cohincide con el parametrizado", fila, txtfile.FileName);
                                                return Json(respuesta);
                                            }

                                            if (c.tipo_dato == "boolean")
                                            {
                                                if (bool.TryParse(datos_array[i], out bool boolean))
                                                {
                                                    if (c.tipo_constraint != null)
                                                    {
                                                        IDictionary<string, string> constraint =
                                                            new Dictionary<string, string>();
                                                        constraint = validaConstraint(sql_values, datos_array.Length,
                                                            iteracion, fila, c.tipo_constraint, c.columna_origen,
                                                            c.tabla_origen, c.nombre_columna, c.columna_referencia,
                                                            c.tabla_referencia, datos_array[i]);
                                                        if (constraint["valor"] == "1")
                                                        {
                                                            sql_values = constraint["sql_values"];
                                                            if (c.tipo_constraint != "FOREIGN KEY")
                                                            {
                                                                bool existe = llaves.Any(b =>
                                                                    b.Value != null && b.Value ==
                                                                    c.columna_origen + "|" + datos_array[i]);
                                                                if (!existe)
                                                                {
                                                                    llaves.Add(fila,
                                                                        c.columna_origen + "|" + datos_array[i]);
                                                                    iteracion++;
                                                                    break;
                                                                }

                                                                respuesta = insertarLog(nombre_migracion,
                                                                    usuario.ToString(), tabla,
                                                                    "Cargue de migraciones -> Valores repedidos en el archivo de tipo boolean",
                                                                    "El valor: " + datos_array[i] +
                                                                    " se encuentra duplicado en el archivo a cargar", fila,
                                                                    txtfile.FileName);
                                                                return Json(respuesta);
                                                            }

                                                            iteracion++;
                                                            break;
                                                        }

                                                        respuesta = insertarLog(nombre_migracion, usuario.ToString(), tabla,
                                                            "Cargue de migraciones -> Constraint tipo boolean",
                                                            constraint["mensaje"], fila, txtfile.FileName);
                                                        return Json(respuesta);
                                                    }

                                                    if (iteracion == 0 && fila == 1)
                                                    {
                                                        sql_values += "('" + datos_array[i] + "'";
                                                    }
                                                    else if (iteracion == 0)
                                                    {
                                                        sql_values += ",('" + datos_array[i] + "'";
                                                    }
                                                    else
                                                    {
                                                        sql_values += " ,'" + datos_array[i] + "'";
                                                    }

                                                    if (iteracion == datos_array.Length - 1)
                                                    {
                                                        sql_values += ")";
                                                    }

                                                    iteracion++;
                                                    break;
                                                }

                                                respuesta = insertarLog(nombre_migracion, usuario.ToString(), tabla,
                                                    "Cargue de migraciones -> tipo dato boolean",
                                                    "El tipo de dato de la columna: " + c.nombre_columna +
                                                    " no cohincide con el parametrizado", fila, txtfile.FileName);
                                                return Json(respuesta);
                                            }

                                            if (c.tipo_constraint != null)
                                            {
                                                IDictionary<string, string> constraint = new Dictionary<string, string>();
                                                constraint = validaConstraint(sql_values, datos_array.Length, iteracion,
                                                    fila, c.tipo_constraint, c.columna_origen, c.tabla_origen,
                                                    c.nombre_columna, c.columna_referencia, c.tabla_referencia,
                                                    datos_array[i]);
                                                if (constraint["valor"] == "1")
                                                {
                                                    sql_values = constraint["sql_values"];
                                                    if (c.tipo_constraint != "FOREIGN KEY")
                                                    {
                                                        bool existe = llaves.Any(b =>
                                                            b.Value != null &&
                                                            b.Value == c.columna_origen + "|" + datos_array[i]);
                                                        if (!existe)
                                                        {
                                                            llaves.Add(fila, c.columna_origen + "|" + datos_array[i]);
                                                            iteracion++;
                                                            break;
                                                        }

                                                        respuesta = insertarLog(nombre_migracion, usuario.ToString(), tabla,
                                                            "Cargue de migraciones -> Valores repedidos en el archivo de tipo otro",
                                                            "El valor: " + datos_array[i] +
                                                            " se encuentra duplicado en el archivo a cargar", fila,
                                                            txtfile.FileName);
                                                        return Json(respuesta);
                                                    }

                                                    iteracion++;
                                                    break;
                                                }

                                                respuesta = insertarLog(nombre_migracion, usuario.ToString(), tabla,
                                                    "Cargue de migraciones -> Constraint tipo otro", constraint["mensaje"],
                                                    fila, txtfile.FileName);
                                                return Json(respuesta);
                                            }

                                            if (iteracion == 0 && fila == 1)
                                            {
                                                sql_values += "('" + datos_array[i] + "'";
                                            }
                                            else if (iteracion == 0)
                                            {
                                                sql_values += ",('" + datos_array[i] + "'";
                                            }
                                            else
                                            {
                                                sql_values += " ,'" + datos_array[i] + "'";
                                            }

                                            if (iteracion == datos_array.Length - 1)
                                            {
                                                sql_values += ")";
                                            }

                                            iteracion++;
                                            break;
                                        }
                                    }
                                }

                                sql_insert += ") values" + sql_values;
                            }
                            else
                            {
                                respuesta = insertarLog(nombre_migracion, usuario.ToString(), tabla,
                                    "Cargue de migraciones -> Cantidad de columnas",
                                    "La cantidad de columnas en el archivo no cohincide con el numero de columnas parametrizadas",
                                    fila, txtfile.FileName);
                                return Json(respuesta);
                            }
                        }

                        try
                        {
                            string entityConnectionString =
                                ConfigurationManager.ConnectionStrings["Iceberg_Context"].ConnectionString;
                            string con = new EntityConnectionStringBuilder(entityConnectionString).ProviderConnectionString;
                            using (SqlConnection connection = new SqlConnection(con))
                            {
                                SqlCommand command = new SqlCommand(sql_insert, connection);
                                connection.Open();
                                int filas_afectadas = command.ExecuteNonQuery();
                                if (filas_afectadas > 0)
                                {
                                    icb_historia_migraciones historialCargue = new icb_historia_migraciones
                                    {
                                        nombre_migracion = nombre_migracion,
                                        tabla_afectada = tabla,
                                        nombre_archivo = txtfile.FileName,
                                        fecha_cargue = DateTime.Now,
                                        estado = true
                                    };
                                    context.icb_historia_migraciones.Add(historialCargue);
                                    int historial_guardado = context.SaveChanges();

                                    if (historial_guardado > 0)
                                    {
                                        respuesta.Add("valor", "1");
                                        respuesta.Add("clase", "success");
                                        respuesta.Add("fila", fila.ToString());
                                        respuesta.Add("mensaje",
                                            "Se han insertado un total de " + filas_afectadas +
                                            " registros y el archivo cuenta con un total de " + fila + " filas");
                                        return Json(respuesta);
                                    }

                                    respuesta.Add("valor", "0");
                                    respuesta.Add("clase", "danger");
                                    respuesta.Add("fila", fila.ToString());
                                    respuesta.Add("mensaje", "No fue posible insertar el historial de cargue");
                                    return Json(respuesta);
                                }

                                string sql_error = sql_insert.Replace("'", "|").Replace("\"", "|");
                                respuesta = insertarLog(nombre_migracion, usuario.ToString(), tabla,
                                    "Cargue de migraciones -> Insertar datos", sql_error, fila, txtfile.FileName);
                                respuesta["mensaje"] = "Hubo un error al insertar la infomación en la tabla: " + tabla +
                                                       " consulte el log de transacciones";
                                return Json(respuesta);
                            }
                        }
                        catch (SqlException ex)
                        {
                            string sql_error = sql_insert.Replace("'", "|").Replace("\"", "|");
                            respuesta = insertarLog(nombre_migracion, usuario.ToString(), tabla,
                                "Cargue de migraciones -> Insertar datos",
                                "Se ha presentado el error: " + ex.Message + " al ejecutar la sentencia: " + sql_error +
                                " en la table " + tabla, fila, txtfile.FileName);
                            respuesta["mensaje"] = "Se ha presentado el error: " + ex.Message +
                                                   " al insertar la infomación en la tabla: " + tabla +
                                                   " consulte el log de transacciones";
                            return Json(respuesta);
                        }
                    }

                    
                }
            }
            catch (DbEntityValidationException ex)
            {
                respuesta.Add("valor", "0");
                respuesta.Add("clase", "danger");
                respuesta.Add("mensaje", "No fue posible inicar el proceso de cargue " + ex.Message);
                return Json(respuesta);
            }
        }

        public IDictionary<string, string> insertarLog(string migracion, string usuario, string tabla,
            string origen_error, string error, int fila, string nombre_archivo)
        {
            IDictionary<string, string> respuesta = new Dictionary<string, string>();
            using (System.Data.Entity.DbContextTransaction dbTran = context.Database.BeginTransaction())
            {
                try
                {
                    log_transacciones_migraciones logTransaccion = new log_transacciones_migraciones
                    {
                        migracion = migracion,
                        usuario_creacion = usuario,
                        tabla_afectada = tabla,
                        origen_error = origen_error,
                        error = "En la fila: " + fila + " " + error,
                        fecha_creacion = DateTime.Now
                    };
                    context.log_transacciones_migraciones.Add(logTransaccion);
                    int log_guardado = context.SaveChanges();
                    if (log_guardado > 0)
                    {
                        icb_historia_migraciones historialCargue = new icb_historia_migraciones
                        {
                            nombre_migracion = migracion,
                            tabla_afectada = tabla,
                            nombre_archivo = nombre_archivo,
                            fecha_cargue = DateTime.Now,
                            estado = false,
                            id_log_transacciones = logTransaccion.id
                        };
                        context.icb_historia_migraciones.Add(historialCargue);
                        int historial_guardado = context.SaveChanges();

                        if (historial_guardado > 0)
                        {
                            respuesta.Add("valor", "0");
                            respuesta.Add("clase", "danger");
                            respuesta.Add("fila", fila.ToString());
                            respuesta.Add("mensaje", error);
                            dbTran.Commit();
                            return respuesta;
                        }

                        respuesta.Add("valor", "0");
                        respuesta.Add("clase", "danger");
                        respuesta.Add("fila", fila.ToString());
                        respuesta.Add("mensaje", "No fue posible insertar el historial de cargue");
                        return respuesta;
                    }

                    respuesta.Add("valor", "0");
                    respuesta.Add("clase", "danger");
                    respuesta.Add("fila", fila.ToString());
                    respuesta.Add("mensaje",
                        "No fue posible insertar el registro del error en el log de transacciones");
                    return respuesta;
                }
                catch (DbEntityValidationException ex)
                {
                    dbTran.Rollback();
                    respuesta.Add("valor", "0");
                    respuesta.Add("clase", "danger");
                    respuesta.Add("mensaje", "No fue iniciar la conexión con la base " + ex.Message);
                    return respuesta;
                }
            }
        }

        public IDictionary<string, string> validaConstraint(string sql_values, int cantidad_columnas, int iteracion,
            int fila, string tipo_constraint, string columna_origen, string tabla_origen, string nombre_columna,
            string columna_referencia, string tabla_referencia, string dato_buscado)
        {
            IDictionary<string, string> respuesta = new Dictionary<string, string>();
            if (tipo_constraint == "FOREIGN KEY")
            {
                List<string> procedimiento = context.pw_existe_constraint("", "", tipo_constraint, tabla_referencia,
                    nombre_columna, columna_referencia, dato_buscado).ToList();
                for (int j = 0; j < procedimiento.Count; j++)
                {
                    if (procedimiento[j] != null)
                    {
                        if (iteracion == 0 && fila == 1)
                        {
                            sql_values += "('" + procedimiento[j] + "'";
                        }
                        else if (iteracion == 0)
                        {
                            sql_values += ",('" + procedimiento[j] + "'";
                        }
                        else
                        {
                            sql_values += " ,'" + procedimiento[j] + "'";
                        }

                        if (iteracion == cantidad_columnas - 1)
                        {
                            sql_values += ")";
                        }

                        respuesta.Add("valor", "1");
                        respuesta.Add("sql_values", sql_values);
                        return respuesta;
                    }
                    else
                    {
                        respuesta.Add("valor", "0");
                        respuesta.Add("clase", "danger");
                        respuesta.Add("fila", fila.ToString());
                        respuesta.Add("mensaje",
                            "el valor: " + dato_buscado + " no esta presente en la tabla: " + tabla_referencia +
                            ". Usted puede crearlo de forma manual e intentarlo nuevamente o seleccione la migracion parametrizada para la tabla");
                        return respuesta;
                    }
                }
            }
            else
            {
                List<string> procedimiento = context
                    .pw_existe_constraint(columna_origen, tabla_origen, tipo_constraint, "", "", "", dato_buscado)
                    .ToList();
                for (int j = 0; j < procedimiento.Count; j++)
                {
                    if (procedimiento[j] != null)
                    {
                        respuesta.Add("valor", "0");
                        respuesta.Add("clase", "danger");
                        respuesta.Add("fila", fila.ToString());
                        respuesta.Add("mensaje",
                            "El valor: " + dato_buscado + " viola la restricción de unicidad de la tabla" +
                            tabla_origen);
                        return respuesta;
                    }
                    else
                    {
                        if (iteracion == 0 && fila == 1)
                        {
                            sql_values += "('" + dato_buscado + "'";
                        }
                        else if (iteracion == 0)
                        {
                            sql_values += ",('" + dato_buscado + "'";
                        }
                        else
                        {
                            sql_values += " ,'" + dato_buscado + "'";
                        }

                        if (iteracion == cantidad_columnas - 1)
                        {
                            sql_values += ")";
                        }

                        respuesta.Add("valor", "1");
                        respuesta.Add("sql_values", sql_values);
                        return respuesta;
                    }
                }
            }

            respuesta.Add("valor", "0");
            respuesta.Add("clase", "danger");
            respuesta.Add("fila", fila.ToString());
            respuesta.Add("mensaje", "No fue posible validar los datos ingresados en la linea " + fila);
            return respuesta;
        }

        public JsonResult traerColumnas(int migracion)
        {
            //var columnas = context.vw_columnas_sistema.OrderBy(d => d.COLUMN_NAME).Where(d => 1 == 1 && d.TABLE_NAME==nomTabla).ToList();
            var columnas = (from c in context.vw_parametrizaciones_creadas
                            orderby c.orden
                            where c.id_migracion == migracion && c.col_agregada
                            select new
                            {
                                c.nombre_columna,
                                c.campo_null,
                                c.tipo_dato,
                                c.nombre_constraint,
                                c.tabla_referencia,
                                c.columna_referencia,
                                c.tipo_constraint
                            }).ToList();
            string cuerpo = "";
            foreach (var c in columnas)
            {
                string label_nulo = "<span class=\"badge badge-danger\">No</span>";
                string label_constraint = "";
                if (c.campo_null)
                {
                    label_nulo = "<span class=\"badge badge-warning\">Si</span>";
                }

                if (c.tipo_constraint == "FOREIGN KEY")
                {
                    label_constraint = c.nombre_constraint + ", relacionado con la tabla " + c.tabla_referencia + "(" +
                                       c.columna_referencia + ")";
                }
                else
                {
                    label_constraint = c.nombre_constraint;
                }

                cuerpo += "<tr>" +
                          "<td>" + c.nombre_columna + "</td>" +
                          "<td>" + label_nulo + "</td>" +
                          "<td>" + c.tipo_dato + "</td>" +
                          "<td>" + label_constraint + "</td>" +
                          "<td>" + c.tipo_constraint + "</td>"
                          + "</tr>";
            }

            return Json(cuerpo);
        }

        public JsonResult generarEjemplo(int migracion, string labelMigracion)
        {
            labelMigracion = labelMigracion.Replace(" ", "_");
            string ruta = "../Content/Migraciones/EjemplosCargues/" + labelMigracion + ".txt";
            string path = Server.MapPath("~/Content/Migraciones/EjemplosCargues/" + labelMigracion + ".txt");
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            var columnas = (from c in context.vw_parametrizaciones_creadas
                            orderby c.id_columna
                            where c.id_migracion == migracion && c.col_agregada
                            select new
                            {
                                c.nombre_columna,
                                c.campo_null,
                                c.tipo_dato
                            }).ToList();
            using (StreamWriter sw = System.IO.File.CreateText(path))
            {
                string encabezados = "";
                string cuerpo = "";
                sw.WriteLine(
                    "Este es un archivo de ejemplo. Recuerde que el archivo a cargar no debe llevar encabezados");
                foreach (var c in columnas)
                {
                    encabezados += c.nombre_columna + "|";
                    if (c.tipo_dato == "integer")
                    {
                        cuerpo += "11111|";
                    }
                    else if (c.tipo_dato == "time")
                    {
                        cuerpo += "HH:MM:SS|";
                    }
                    else if (c.tipo_dato == "boolean")
                    {
                        cuerpo += "true/false|";
                    }
                    else if (c.tipo_dato == "date")
                    {
                        cuerpo += "AAAA-MM-DD|";
                    }
                    else if (c.tipo_dato == "datetime")
                    {
                        cuerpo += "AAAA-MM-DD HH:MM:SS|";
                    }
                    else
                    {
                        cuerpo += "AAAAAAAA|";
                    }
                }

                int encabezado_tamano = encabezados.Length;
                int cuerpo_tamano = cuerpo.Length;
                encabezados = encabezados.Substring(0, encabezado_tamano - 1);
                cuerpo = cuerpo.Substring(0, cuerpo_tamano - 1);

                sw.WriteLine(encabezados);
                sw.WriteLine(cuerpo);
                return Json(ruta);
            }
        }

        public JsonResult TraerCargues(string filtroGeneral)
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

            Expression<Func<vw_historial_migraciones, bool>> predicado = PredicateBuilder.True<vw_historial_migraciones>();
            Expression<Func<vw_historial_migraciones, bool>> predicado3 = PredicateBuilder.False<vw_historial_migraciones>();


            if (!string.IsNullOrWhiteSpace(filtroGeneral))
            {
                predicado3 = predicado3.Or(d => 1 == 1 && d.nombre_migracion.ToString().Contains(filtroGeneral));
                predicado3 = predicado3.Or(d => 1 == 1 && d.tabla_afectada.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.nombre_archivo.Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.error.Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.origen_error.Contains(filtroGeneral.ToUpper()));
                predicado = predicado.And(predicado3);
            }

            int registrostotales = context.vw_historial_migraciones.Where(predicado).Count();
            //si el ordenamiento es ascendente o descendente es distinto
            if (pageSize == -1)
            {
                pageSize = registrostotales;
            }

            if (sortColumnDir == "asc")
            {
                List<vw_historial_migraciones> query2 = context.vw_historial_migraciones.Where(predicado)
                    .OrderBy(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
                var query = query2.Select(d => new
                {
                    d.nombre_migracion,
                    d.tabla_afectada,
                    d.nombre_archivo,
                    d.fecha_cargue,
                    d.estado,
                    d.error
                }).ToList();
                int contador = query.Count();
                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<vw_historial_migraciones> query2 = context.vw_historial_migraciones.Where(predicado)
                    .OrderByDescending(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();

                var query = query2.Select(d => new
                {
                    d.nombre_migracion,
                    d.tabla_afectada,
                    d.nombre_archivo,
                    d.fecha_cargue,
                    d.estado,
                    d.error
                }).ToList();


                int contador = query.Count();
                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }
        }
    }
}