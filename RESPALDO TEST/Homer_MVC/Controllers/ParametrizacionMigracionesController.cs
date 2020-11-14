using Homer_MVC.IcebergModel;
using Homer_MVC.ViewModels.medios;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class ParametrizacionMigracionesController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: ParametrizacionMigraciones
        private static Expression<Func<vw_migraciones_parametrizadas,
            string>> GetColumnName(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(vw_migraciones_parametrizadas), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<vw_migraciones_parametrizadas, string>> lambda = Expression.Lambda<Func<vw_migraciones_parametrizadas,
                string>>(menuProperty, menu);
            return lambda;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CrearMigracion()
        {
            traerMigraciones();
            traerTablas();
            traerTablasReferencia();
            return View();
        }

        public void traerMigraciones()
        {
            ViewBag.migraciones = new SelectList(context.icb_migracion, "id", "nombre_migracion");
        }

        public void traerTablas()
        {
            //Tabla con los nombres que no queremos
            List<string> nombres = context.icb_tabla_migracion.Select(d => d.nombre_tabla).ToList();
            //consulta para traerme de otra tabla esos nombres que no queremos
            List<vw_tablas_sistema> tablas = context.vw_tablas_sistema.OrderBy(d => d.TABLE_NAME)
                .Where(d => 1 == 1 && !nombres.Contains(d.TABLE_NAME)).ToList();
            ViewBag.tablas = new SelectList(tablas, "object_id", "TABLE_NAME");
        }

        public JsonResult traerTablasDetallado(string nomTabla)
        {
            IDictionary<string, string> respuesta = new Dictionary<string, string>();
            var columnas = (from c in context.icb_tabla_migracion
                            where c.nombre_tabla == nomTabla
                            select new
                            {
                                c.nombre_tabla,
                                c.oid
                            }).ToList();
            string columnas_select = "<option value=\"\"></option>";
            string oid = "";
            foreach (var c in columnas)
            {
                columnas_select += "<option selected value=" + c.oid + ">" + c.nombre_tabla + "</option>";
                oid = c.oid.ToString();
            }

            respuesta.Add("cuerpo", columnas_select);
            respuesta.Add("val", oid);
            return Json(respuesta);
        }

        public void traerTablasReferencia()
        {
            List<vw_tablas_sistema> tablasReferencia = context.vw_tablas_sistema.OrderBy(d => d.TABLE_NAME).ToList();
            ViewBag.tablasReferencia = new SelectList(tablasReferencia, "object_id", "TABLE_NAME");
        }

        public JsonResult traerColumnas(string nomTabla)
        {
            string[] respuesta;
            //var columnas = context.vw_columnas_sistema.OrderBy(d => d.COLUMN_NAME).Where(d => 1 == 1 && d.TABLE_NAME==nomTabla).ToList();
            var columnas = (from c in context.vw_columnas_sistema
                            where c.TABLE_NAME == nomTabla
                            select new
                            {
                                columna = c.COLUMN_NAME,
                                campo_null = c.IS_NULLABLE,
                                tipo_dato = c.DATA_TYPE
                            }).ToList();
            string cuerpo = "";
            string columnas_select = "<option value=\"\"></option>";
            foreach (var c in columnas)
            {
                string label_nulo = "<span class=\"badge badge-danger\">No</span>";
                if (c.campo_null == "YES")
                {
                    label_nulo = "<span class=\"badge badge-warning\">Si</span>";
                }

                cuerpo += "<tr>" +
                          "<td>" + c.columna + "</td>" +
                          "<td>" + label_nulo + "</td>" +
                          "<td>" +
                          "<input class=\"check_toggle columnas\" type=\"checkbox\" columnOrden=\"\" name=\"chk_columnas\" id=\"chk_" +
                          c.columna + "\" value=\"" + c.columna + "|" + c.campo_null + "|" + c.tipo_dato +
                          "\" data_toggle=\"toggle\" data_size=\"mini\" checked />" +
                          "</td>"
                          + "</tr>";
                columnas_select += "<option value=" + c.columna + ">" + c.columna + "</option>";
            }

            respuesta = new string[2] { cuerpo, columnas_select };
            return Json(respuesta);
        }

        public JsonResult traerColumnasDetallado(string nomTabla)
        {
            //var columnas = context.vw_columnas_sistema.OrderBy(d => d.COLUMN_NAME).Where(d => 1 == 1 && d.TABLE_NAME==nomTabla).ToList();
            var columnas = (from c in context.vw_parametrizaciones_creadas
                            orderby c.orden
                            where c.nombre_tabla == nomTabla
                            select new
                            {
                                columna = c.columna_tabla,
                                c.campo_null,
                                tipo_dato = c.tipo_dato_original,
                                c.col_agregada,
                                c.orden
                            }).ToList();
            string cuerpo = "";
            foreach (var c in columnas)
            {
                string label_nulo = "<span class=\"badge badge-danger\">No</span>";
                string es_nulo = "NO";
                if (c.campo_null)
                {
                    label_nulo = "<span class=\"badge badge-warning\">Si</span>";
                    es_nulo = "YES";
                }

                string check = "";
                if (c.col_agregada)
                {
                    check = "checked";
                }

                cuerpo += "<tr>" +
                          "<td>" + c.columna + "</td>" +
                          "<td>" + label_nulo + "</td>" +
                          "<td>" +
                          "<input class=\"check_toggle columnas\" type=\"checkbox\" name=\"chk_columnas\"  id=\"chk_" +
                          c.columna + "\" value=\"" + c.columna + "|" + es_nulo + "|" + c.tipo_dato +
                          "\" data_toggle=\"toggle\" data_size=\"mini\" " + check + " />" +
                          "</td>"
                          + "</tr>";
            }

            return Json(cuerpo);
        }

        public JsonResult traerConstraintsDetallado(string nomTabla)
        {
            var constraints = (from cn in context.icb_constaint
                               join co in context.icb_columnas_tabla on cn.id_columna_tabla equals co.id
                               join t in context.icb_tabla_migracion on co.id_tabla_migracion equals t.id
                               where t.nombre_tabla == nomTabla
                               select new
                               {
                                   cn.nombre,
                                   cn.tipo_constraint,
                                   columna_origen = co.nombre_campo,
                                   cn.tabla_referencia,
                                   cn.columna_referencia,
                                   cn.columna_resultado
                               }).ToList();
            string cuerpo = "";
            foreach (var con in constraints)
            {
                string constraint = con.nombre + "|" + con.tipo_constraint + "|" + con.columna_origen + "|" +
                                 con.tabla_referencia + "|" + con.columna_referencia + "|" + con.columna_resultado;
                cuerpo += "<tr id=\"tr_" + con.nombre + "\">" +
                          "<td>" + con.nombre + "</td>" +
                          "<td>" + con.tipo_constraint + "</td>" +
                          "<td>" + con.columna_origen + "</td>" +
                          "<td>" + con.tabla_referencia + "</td>" +
                          "<td>" + con.columna_referencia + "</td>" +
                          "<td>" + con.columna_resultado + "</td>" +
                          "<td><a onclick=\"retirar_constraint('tr_" + con.nombre +
                          "')\" class=\"btn btn-danger\" name=\"btnAgregarFK\" id=\"btnAgregarFK\"><i class=\"fa fa-trash\"></i></a></td>" +
                          "<td class=\"td_constraint\" style=\"display:none;\">" + constraint + "</td>" +
                          "</tr>";
            }

            return Json(cuerpo);
        }

        public JsonResult traerColumnasReferencia(string nomTabla)
        {
            var columnas = (from c in context.vw_columnas_sistema
                            where c.TABLE_NAME == nomTabla
                            select new
                            {
                                columna = c.COLUMN_NAME
                            }).ToList();
            string columnas_select = "<option value=\"\"></option>";
            foreach (var c in columnas)
            {
                columnas_select += "<option value=" + c.columna + ">" + c.columna + "</option>";
            }

            return Json(columnas_select);
        }

        public string EliminarMigracion(string migracion)
        {
            int constraintsContador = 0;
            int columnasContador = 0;
            int tablasContador = 0;
            int migracionesContador = 0;

            var detalladoConstraint = (from m in context.icb_migracion
                                       join t in context.icb_tabla_migracion on m.id equals t.id_migracion
                                       join co in context.icb_columnas_tabla on t.id equals co.id_tabla_migracion
                                       join cn in context.icb_constaint on co.id equals cn.id_columna_tabla
                                       where m.nombre_migracion == migracion
                                       select new
                                       {
                                           id_constraint = cn.id
                                       }).ToList();

            var detalladoColumnas = (from m in context.icb_migracion
                                     join t in context.icb_tabla_migracion on m.id equals t.id_migracion
                                     join co in context.icb_columnas_tabla on t.id equals co.id_tabla_migracion
                                     where m.nombre_migracion == migracion
                                     select new
                                     {
                                         id_columna = co.id
                                     }).ToList();

            var detalladoTabla = (from m in context.icb_migracion
                                  join t in context.icb_tabla_migracion on m.id equals t.id_migracion
                                  where m.nombre_migracion == migracion
                                  select new
                                  {
                                      id_tabla = t.id
                                  }).ToList();
            var detalladoMigracion = (from m in context.icb_migracion
                                      where m.nombre_migracion == migracion
                                      select new
                                      {
                                          id_migracion = m.id
                                      }).ToList();

            using (System.Data.Entity.DbContextTransaction dbTran = context.Database.BeginTransaction())
            {
                try
                {
                    int contador = 0;
                    foreach (var constraints in detalladoConstraint)
                    {
                        contador++;
                        context.icb_constaint.RemoveRange(
                            context.icb_constaint.Where(c => c.id == constraints.id_constraint));
                        int constraintEdliminado = context.SaveChanges();
                        if (constraintEdliminado > 0)
                        {
                            constraintsContador++;
                        }
                    }

                    if (constraintsContador == contador)
                    {
                        contador = 0;
                        foreach (var columnas in detalladoColumnas)
                        {
                            contador++;
                            context.icb_columnas_tabla.RemoveRange(
                                context.icb_columnas_tabla.Where(c => c.id == columnas.id_columna));
                            int columnaEdliminada = context.SaveChanges();
                            if (columnaEdliminada > 0)
                            {
                                columnasContador++;
                            }
                        }

                        if (columnasContador == contador)
                        {
                            contador = 0;
                            foreach (var tabla in detalladoTabla)
                            {
                                contador++;
                                context.icb_tabla_migracion.RemoveRange(
                                    context.icb_tabla_migracion.Where(c => c.id == tabla.id_tabla));
                                int tablaEdliminada = context.SaveChanges();
                                if (tablaEdliminada > 0)
                                {
                                    tablasContador++;
                                }
                            }

                            if (tablasContador == contador)
                            {
                                contador = 0;
                                foreach (var migraciones in detalladoMigracion)
                                {
                                    contador++;
                                    context.icb_migracion.RemoveRange(
                                        context.icb_migracion.Where(c => c.id == migraciones.id_migracion));
                                    int tablaEdliminada = context.SaveChanges();
                                    if (tablaEdliminada > 0)
                                    {
                                        migracionesContador++;
                                    }
                                }

                                if (contador == migracionesContador)
                                {
                                    dbTran.Commit();
                                    return "1";
                                }

                                dbTran.Rollback();
                                return "Error al eliminar la migración";
                            }

                            dbTran.Rollback();
                            return "Error al eliminar la tabla";
                        }

                        dbTran.Rollback();
                        return "Error al eliminar las columnas";
                    }

                    dbTran.Rollback();
                    return "Error al eliminar los constraints";
                }
                catch (DbEntityValidationException)
                {
                    dbTran.Rollback();
                    return "Error al iniciar la eliminación";
                }
            }
        }

        public JsonResult GuardarMigracion(string migracion, string oid, string nombre_tabla, string[] columnas,
            string[] constraint)
        {
            string respuesta_eliminar = EliminarMigracion(migracion);
            int columnas_guardadas = 0;
            int constraint_guardados = 0;
            string[] respuesta;
            if (respuesta_eliminar == "1")
            {
                using (System.Data.Entity.DbContextTransaction dbTran = context.Database.BeginTransaction())
                {
                    try
                    {
                        //creas tus objetos y los vas guardando
                        icb_migracion icbMigracion = new icb_migracion
                        {
                            nombre_migracion = migracion
                        };
                        context.icb_migracion.Add(icbMigracion);
                        int migracion_guardada = context.SaveChanges();

                        if (migracion_guardada > 0)
                        {
                            int id_migracion = icbMigracion.id;
                            icb_tabla_migracion icbTablaMigracion = new icb_tabla_migracion
                            {
                                id_migracion = id_migracion,
                                oid = int.Parse(oid),
                                nombre_tabla = nombre_tabla,
                                tipo_elemento = "BASE TABLE"
                            };
                            context.icb_tabla_migracion.Add(icbTablaMigracion);
                            int tabla_migracion_guardada = context.SaveChanges();

                            if (tabla_migracion_guardada > 0)
                            {
                                int id_tabla_migracion = icbTablaMigracion.id;
                                for (int i = 0; i < columnas.Length; i++)
                                {
                                    string[] columnas_array = columnas[i].Split('|');
                                    string nombre_campo = columnas_array[0];
                                    string es_null = columnas_array[1];
                                    string es_agregada = columnas_array[3];
                                    string tipo_dato = columnas_array[2];
                                    bool campo_null = false;
                                    bool col_agregada = true;
                                    string orden = "";

                                    if (es_null == "YES")
                                    {
                                        campo_null = true;
                                    }

                                    if (es_agregada == "0")
                                    {
                                        col_agregada = false;
                                    }

                                    if (!string.IsNullOrEmpty(columnas_array[4]))
                                    {
                                        orden = columnas_array[4];
                                    }

                                    icb_columnas_tabla icbColumnasTabla = new icb_columnas_tabla
                                    {
                                        id_tabla_migracion = id_tabla_migracion,
                                        nombre_campo = nombre_campo,
                                        tipo_dato = tipo_dato,
                                        campo_null = campo_null,
                                        col_agregada = col_agregada,
                                        orden = orden
                                    };
                                    context.icb_columnas_tabla.Add(icbColumnasTabla);
                                    int columnas_tabla_guardada = context.SaveChanges();
                                    if (columnas_tabla_guardada > 0)
                                    {
                                        columnas_guardadas++;
                                        if (constraint != null)
                                        {
                                            for (int j = 0; j < constraint.Length; j++)
                                            {
                                                string[] constraint_array = constraint[j].Split('|');
                                                string nombre_columna_constraint = constraint_array[2];
                                                if (icbColumnasTabla.nombre_campo == nombre_columna_constraint)
                                                {
                                                    //campos obligatorios
                                                    string nombre_constraint = constraint_array[0];
                                                    int id_columna = icbColumnasTabla.id;
                                                    string tipo_constraint = constraint_array[1];
                                                    //campos opcionales
                                                    string tabla_referencia = constraint_array[3];
                                                    string columna_referencia = constraint_array[4];
                                                    string columna_resultado = constraint_array[5];
                                                    if (tabla_referencia == "" || columna_referencia == "" ||
                                                        columna_resultado == "")
                                                    {
                                                        tabla_referencia = null;
                                                        columna_referencia = null;
                                                        columna_resultado = null;
                                                    }

                                                    icb_constaint icbConstraint = new icb_constaint
                                                    {
                                                        nombre = nombre_constraint,
                                                        id_columna_tabla = id_columna,
                                                        tipo_constraint = tipo_constraint,
                                                        tabla_referencia = tabla_referencia,
                                                        columna_referencia = columna_referencia,
                                                        columna_resultado = columna_resultado
                                                    };
                                                    context.icb_constaint.Add(icbConstraint);
                                                    int constraint_guardada = context.SaveChanges();
                                                    if (constraint_guardada > 0)
                                                    {
                                                        constraint_guardados++;
                                                    }
                                                    else
                                                    {
                                                        dbTran.Rollback();
                                                        respuesta = new string[2]
                                                        {
                                                            "danger",
                                                            "Error al insertar los constraints de la tabla, fallo en la constraint " +
                                                            nombre_constraint + " de la columna " +
                                                            icbColumnasTabla.nombre_campo
                                                        };
                                                        return Json(respuesta);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        dbTran.Rollback();
                                        respuesta = new string[2]
                                        {
                                            "danger",
                                            "Error al insertar las columnas de la tabla, fallo en la columna " +
                                            nombre_campo
                                        };
                                        return Json(respuesta);
                                    }
                                }

                                if (constraint != null)
                                {
                                    if (columnas_guardadas == columnas.Length &&
                                        constraint_guardados == constraint.Length)
                                    {
                                        dbTran.Commit();
                                        respuesta = new string[2]
                                            {"success", "Se ha insertado la migración corectamente"};
                                    }
                                    else
                                    {
                                        dbTran.Rollback();
                                        respuesta = new string[2] { "danger", "Error al completar la transacción " };
                                    }
                                }
                                else
                                {
                                    if (columnas_guardadas == columnas.Length)
                                    {
                                        dbTran.Commit();
                                        respuesta = new string[2]
                                            {"success", "Se ha insertado la migración corectamente"};
                                    }
                                    else
                                    {
                                        dbTran.Rollback();
                                        respuesta = new string[2] { "danger", "Error al completar la transacción " };
                                    }
                                }
                            }
                            else
                            {
                                dbTran.Rollback();
                                respuesta = new string[2] { "danger", "Error al insertar la tabla de origen" };
                            }
                        }
                        else
                        {
                            dbTran.Rollback();
                            respuesta = new string[2] { "danger", "Error al insertar la migración" };
                        }

                        ////si todos los guardados van bien
                        //dbTran.Commit();
                        ////si no
                        //dbTran.Rollback();
                    }
                    catch (DbEntityValidationException)
                    {
                        dbTran.Rollback();
                        respuesta = new string[2] { "danger", "Error al iniciar la transacción" };
                    }
                }
            }
            else
            {
                respuesta = new string[2] { "danger", respuesta_eliminar };
            }

            return Json(respuesta);
        }

        public JsonResult cargarMigraciones(string filtroGeneral)
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

            Expression<Func<vw_migraciones_parametrizadas, bool>> predicado = PredicateBuilder.True<vw_migraciones_parametrizadas>();
            Expression<Func<vw_migraciones_parametrizadas, bool>> predicado3 = PredicateBuilder.False<vw_migraciones_parametrizadas>();


            if (!string.IsNullOrWhiteSpace(filtroGeneral))
            {
                predicado3 = predicado3.Or(d => 1 == 1 && d.nombre_migracion.ToString().Contains(filtroGeneral));
                predicado3 = predicado3.Or(d => 1 == 1 && d.nombre_tabla.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.columnas.Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.constraints.Contains(filtroGeneral.ToUpper()));
                predicado = predicado.And(predicado3);
            }

            int registrostotales = context.vw_migraciones_parametrizadas.Where(predicado).Count();
            //si el ordenamiento es ascendente o descendente es distinto
            if (pageSize == -1)
            {
                pageSize = registrostotales;
            }

            if (sortColumnDir == "asc")
            {
                List<vw_migraciones_parametrizadas> query2 = context.vw_migraciones_parametrizadas.Where(predicado)
                    .OrderBy(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
                var query = query2.Select(d => new
                {
                    d.nombre_migracion,
                    d.nombre_tabla,
                    d.columnas,
                    d.constraints,
                    d.id
                }).ToList();


                int contador = query.Count();
                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<vw_migraciones_parametrizadas> query2 = context.vw_migraciones_parametrizadas.Where(predicado)
                    .OrderByDescending(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();

                var query = query2.Select(d => new
                {
                    d.nombre_migracion,
                    d.nombre_tabla,
                    d.columnas,
                    d.constraints,
                    d.id
                }).ToList();


                int contador = query.Count();
                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }
        }
    }
}