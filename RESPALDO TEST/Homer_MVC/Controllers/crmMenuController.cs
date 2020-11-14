using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class crmMenuController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");

        private static Expression<Func<datatablex, string>> GetColumnName(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(datatablex), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<datatablex, string>> lambda = Expression.Lambda<Func<datatablex, string>>(menuProperty, menu);

            return lambda;
        }

        // GET: crmMenu
        public ActionResult Index(int? menu, int? id)
        {
            DateTime fecha = DateTime.Now.Date;
            ViewBag.fecha_hasta = fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US"));
            ViewBag.fecha_desde = fecha.AddDays(-30).ToString("yyyy/MM/dd", new CultureInfo("en-US"));

            return View();
        }

        public ActionResult DasboardContact()
        {
            return View();
        }

        public JsonResult buscarDashboard(string fecha_desde, string fecha_hasta)
        {
            return Json(0);
        }

        public ActionResult ConsultaIndividual(int? menu, int? id)
        {
            BuscarFavoritos(menu);
            if (id != null)
            {
                icb_terceros cliente = context.icb_terceros.Where(d => d.tercero_id == id).FirstOrDefault();
                if (cliente != null)
                {
                    ViewBag.txtDocumentoTercero = cliente.doc_tercero;
                }
            }

            return View();
        }

        public ActionResult ConsultaClientes(int? menu, int? id)
        {
            BuscarFavoritos(menu);
            //si existe el id busco el documento de ese cliente y lo mando al formulario
            if (id != null)
            {
                icb_terceros cliente = context.icb_terceros.Where(d => d.tercero_id == id).FirstOrDefault();
                if (cliente != null)
                {
                    ViewBag.txtDocumentoTercero = cliente.doc_tercero;
                }
            }

            DateTime fecha = DateTime.Now.Date;
            ViewBag.fecha_hasta = fecha.ToString("yyyy/MM/dd", new CultureInfo("en-US"));
            ViewBag.fecha_desde = fecha.AddDays(-30).ToString("yyyy/MM/dd", new CultureInfo("en-US"));
            return View();
        }

        public JsonResult BuscarFiltrosPestanaDos(int id_menu)
        {
            var buscarFiltros = (from menuBusqueda in context.menu_busqueda
                                 where menuBusqueda.menu_busqueda_id_menu == id_menu && menuBusqueda.menu_busqueda_id_pestana == 1
                                 select new
                                 {
                                     menuBusqueda.menu_busqueda_nombre,
                                     menuBusqueda.menu_busqueda_tipo_campo,
                                     menuBusqueda.menu_busqueda_campo,
                                     menuBusqueda.menu_busqueda_consulta,
                                     menuBusqueda.menu_busqueda_multiple
                                 }).ToList();

            List<ListaFiltradaModel> listas = new List<ListaFiltradaModel>();
            foreach (var item in buscarFiltros)
            {
                if (item.menu_busqueda_tipo_campo == "select")
                {
                    //con esto guardo las listas agregadas en la tabla
                    string queryString = item.menu_busqueda_consulta;
                    //string connectionString = @"Data Source=WIN-DESARROLLO\DEVSQLSERVER;Initial Catalog=Iceberg_Project2;User ID=simplexwebuser;Password=Iceberg05;MultipleActiveResultSets=True;Application Name=EntityFramework";
                    string entityConnectionString =
                        ConfigurationManager.ConnectionStrings["Iceberg_Context"].ConnectionString;
                    string con = new EntityConnectionStringBuilder(entityConnectionString).ProviderConnectionString;
                    ListaFiltradaModel nuevaLista = new ListaFiltradaModel
                    {
                        NombreAMostrar = item.menu_busqueda_nombre,
                        NombreCampo = item.menu_busqueda_campo
                    };
                    if (item.menu_busqueda_multiple)
                    {
                        nuevaLista.multiple = 1;
                    }
                    else
                    {
                        nuevaLista.multiple = 0;
                    }

                    using (SqlConnection connection = new SqlConnection(con))
                    {
                        SqlCommand command = new SqlCommand(queryString, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        try
                        {
                            while (reader.Read())
                            {
                                string id = reader[0].ToString();
                                object valor = reader[1];
                                nuevaLista.items.Add(new SelectListItem { Text = (string)valor, Value = id });
                            }

                            listas.Add(nuevaLista);
                        }
                        finally
                        {
                            // Always call Close when done reading.
                            reader.Close();
                        }
                    }
                }
            }
            //aqui agrego las listas armadas por mi mismo (rangos de edades, numero de hijos)

            return Json(new { buscarFiltros, listas }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDevolucionesPaginadas(string filtros, string valorFiltros, string filtroGeneral)
        {
            try
            {
                string draw = Request.Form.GetValues("draw").FirstOrDefault();
                string start = Request.Form.GetValues("start").FirstOrDefault();
                string length = Request.Form.GetValues("length").FirstOrDefault();
                string search = Request.Form.GetValues("search[value]").FirstOrDefault();
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

                int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                string busquedadoc_tercero = "";
                string busquedaterceroo = "";
                string busquedatpdoc_id = "";
                string busquedaedad = "";
                string busquedagenero = "";
                string busquedaemail = "";
                string busquedasegmento = "";
                string busquedamarca = "";
                string busquedamodelo = "";
                string busquedacolor = "";
                //var busquedafecha = "";
                string[] vectorNombresFiltro = !string.IsNullOrEmpty(filtros) ? filtros.Split(',') : new string[1];
                string[] vectorValoresFiltro = !string.IsNullOrEmpty(valorFiltros) ? valorFiltros.Split('~') : new string[1];
                for (int i = 0; i < vectorNombresFiltro.Length; i++)
                {
                    busquedadoc_tercero = busquedadoc_tercero == ""
                        ? vectorNombresFiltro[i] == "doc_tercero" ? vectorValoresFiltro[i] : ""
                        : busquedadoc_tercero;
                    busquedaterceroo = busquedaterceroo == ""
                        ? vectorNombresFiltro[i] == "tercero" ? vectorValoresFiltro[i] : ""
                        : busquedaterceroo;
                    busquedatpdoc_id = busquedatpdoc_id == ""
                        ? vectorNombresFiltro[i] == "tpdoc_id" ? vectorValoresFiltro[i] : ""
                        : busquedatpdoc_id;
                    busquedaedad = busquedaedad == ""
                        ? vectorNombresFiltro[i] == "edad" ? vectorValoresFiltro[i] : ""
                        : busquedaedad;
                    busquedagenero = busquedagenero == ""
                        ? vectorNombresFiltro[i] == "gentercero_id" ? vectorValoresFiltro[i] : ""
                        : busquedagenero;
                    busquedaemail = busquedaemail == ""
                        ? vectorNombresFiltro[i] == "email_tercero" ? vectorValoresFiltro[i] : ""
                        : busquedaemail;
                    busquedasegmento = busquedasegmento == ""
                        ? vectorNombresFiltro[i] == "segmentacion" ? vectorValoresFiltro[i] : ""
                        : busquedasegmento;
                    busquedamarca = busquedamarca == ""
                        ? vectorNombresFiltro[i] == "marca" ? vectorValoresFiltro[i] : ""
                        : busquedamarca;
                    busquedamodelo = busquedamodelo == ""
                        ? vectorNombresFiltro[i] == "modelo" ? vectorValoresFiltro[i] : ""
                        : busquedamodelo;
                    busquedacolor = busquedacolor == ""
                        ? vectorNombresFiltro[i] == "color" ? vectorValoresFiltro[i] : ""
                        : busquedacolor;
                }

                Expression<Func<vw_CRMgeneral, bool>> predicado = PredicateBuilder.True<vw_CRMgeneral>();
                Expression<Func<vw_CRMgeneral, bool>> predicado2 = PredicateBuilder.False<vw_CRMgeneral>();
                if (!string.IsNullOrWhiteSpace(filtroGeneral))
                {
                    predicado2 = predicado2.Or(d => 1 == 1 && d.doc_tercero.Contains(filtroGeneral));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.tercero.Contains(filtroGeneral));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.tpdoc_nombre.Contains(filtroGeneral));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.gentercero_nombre.Contains(filtroGeneral));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.email_tercero.Contains(filtroGeneral));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.Segmentacion.Contains(filtroGeneral));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.colvh_nombre.Contains(filtroGeneral));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.ref_descripcion.Contains(filtroGeneral));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.marcvh_nombre.Contains(filtroGeneral));
                    //predicado2 = predicado2.Or(d => 1 == 1 && filtroGeneral.Contains(d.doc_tercero.ToString()));
                    predicado = predicado.And(predicado2);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(busquedadoc_tercero))
                    {
                        predicado = predicado.And(d => d.doc_tercero == busquedadoc_tercero);
                    }

                    if (!string.IsNullOrWhiteSpace(busquedaterceroo))
                    {
                        predicado = predicado.And(d => d.tercero.Contains(busquedaterceroo));
                    }

                    if (!string.IsNullOrWhiteSpace(busquedatpdoc_id))
                    {
                        bool convertir = int.TryParse(busquedatpdoc_id, out int documento);
                        predicado = predicado.And(d => d.tpdoc_id == documento);
                    }

                    if (!string.IsNullOrWhiteSpace(busquedagenero))
                    {
                        bool convertir = int.TryParse(busquedagenero, out int generox);
                        predicado = predicado.And(d => d.gentercero_id == generox);
                    }

                    if (!string.IsNullOrWhiteSpace(busquedaemail))
                    {
                        predicado = predicado.And(d => d.email_tercero.Contains(busquedaemail));
                    }

                    if (!string.IsNullOrWhiteSpace(busquedamarca))
                    {
                        //separo las marcas por comas
                        Expression<Func<vw_CRMgeneral, bool>> predicado3 = PredicateBuilder.False<vw_CRMgeneral>();

                        string[] idmarcas = busquedamarca.Split(',');
                        foreach (string item2 in idmarcas)
                        {
                            predicado3 = predicado3.Or(d => d.marcvh_id.ToString() == item2);
                        }

                        predicado = predicado.And(predicado3);
                    }


                    if (!string.IsNullOrWhiteSpace(busquedamodelo))
                    {
                        //separo las marcas por comas
                        Expression<Func<vw_CRMgeneral, bool>> predicado5 = PredicateBuilder.False<vw_CRMgeneral>();

                        string[] modelox = busquedamodelo.Split(',');

                        foreach (string item33 in modelox)
                        {
                            predicado5 = predicado5.Or(d => 1 == 1 && d.ref_descripcion.Contains(item33));
                        }

                        predicado = predicado.And(predicado5);
                    }


                    if (!string.IsNullOrWhiteSpace(busquedacolor))
                    {
                        //separo las marcas por comas
                        Expression<Func<vw_CRMgeneral, bool>> predicado4 = PredicateBuilder.False<vw_CRMgeneral>();

                        string[] colores = busquedacolor.Split(',');

                        foreach (string item in colores)
                        {
                            predicado4 = predicado4.Or(d => 1 == 1 && d.colvh_nombre.Contains(item));
                        }

                        predicado = predicado.And(predicado4);
                    }

                    //if (!string.IsNullOrWhiteSpace(busquedaemail))
                    //{
                    //    predicado = predicado.And(d => d.email_tercero.Contains(busquedaemail));
                    //}
                    if (!string.IsNullOrWhiteSpace(busquedasegmento))
                    {
                        predicado = predicado.And(d => d.Segmentacion.Contains(busquedasegmento));
                    }
                }

                //script para CONTAR los registros totales             


                //var query = context.vw_CRMgeneral.Where(predicado).Skip(skip).Take(pageSize).Select(d => new

                List<datatablex> query = context.vw_CRMgeneral.Where(predicado).Select(d => new datatablex
                {
                    id = d.tercero_id,
                    Columna0 = d.tpdoc_nombre,
                    Columna1 = d.doc_tercero,
                    Columna2 = d.tercero,
                    Columna3 = d.edad,
                    Columna4 = d.gentercero_nombre,
                    Columna5 = d.celular_tercero,
                    Columna6 = d.email_tercero,
                    Columna7 = d.Segmentacion,
                    Columna8 = d.plan_mayor
                    //Columna9 = context.icb_contacto_tercero.Where(f => f.tercero_id == d.tercero_id).Count(),
                }).ToList();

                int contador = query.GroupBy(d => d.id).Count();
                if (sortColumnDir == "asc")
                {
                    query = query.OrderBy(GetColumnName(sortColumn).Compile()).ToList();
                }
                else if (sortColumnDir == "desc")
                {
                    query = query.OrderByDescending(GetColumnName(sortColumn).Compile()).ToList();
                }

                var lista = query.GroupBy(d => d.id).Skip(skip).Take(pageSize).Select(d => new
                {
                    Columna0 = d.Select(f => f.Columna0).First(),
                    Columna1 = d.Select(f => f.Columna1).First(),
                    Columna2 = d.Select(f => f.Columna2).First(),
                    Columna3 = d.Select(f => f.Columna3).First(),
                    Columna4 = d.Select(f => f.Columna4).First(),
                    Columna5 = d.Select(f => f.Columna5).First(),
                    Columna6 = d.Select(f => f.Columna6).First(),
                    Columna7 = d.Select(f => f.Columna7).First(),
                    Columna8 = context.icb_vehiculo.Where(f => f.propietario == d.Key).Count(),
                    Columna9 = "<button type='button' class='btn btn-sm btn-info' onclick='verfichero(" + d.Key +
                               ")'>&nbsp;Ver&nbsp;</button>"
                }).ToList();

                int count = lista.Count();
                //lista.Add(new ConsultaFiltradaModel()
                //{
                //    Columna0 = "(" + (string)reader["prefijo"] + ") " + (string)reader["tpdoc_nombre"],
                //    Columna1 = numeroDevolucion.ToString(),
                //    Columna2 = (string)reader["planMayor"],
                //    Columna3 = "(" + (string)reader["bodccs_cod"] + ") " + (string)reader["bodccs_nombre"],
                //    Columna4 = (string)reader["doc_tercero"],
                //    Columna5 = nombre,
                //    Columna6 = fecha.ToString("yyyy/MM/dd"),
                //    Columna7 = "$ " + Math.Round(valor).ToString("0,0", elGR),
                //    Columna8 = numeroCompra.ToString(),
                //    Columna9 = idDevolucion.ToString()
                //});

                return Json(new { draw, recordsFiltered = contador, recordsTotal = contador, data = lista },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { errorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
                // Exception
            }
        }

        public JsonResult BuscarDevolucionesPaginadasxx(string filtros, string valorFiltros, string filtroGeneral)
        {
            try
            {
                string draw = Request.Form.GetValues("draw").FirstOrDefault();
                string start = Request.Form.GetValues("start").FirstOrDefault();
                string length = Request.Form.GetValues("length").FirstOrDefault();
                string search = Request.Form.GetValues("search[value]").FirstOrDefault();
                string sortColumn = Request.Form
                    .GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]")
                    .FirstOrDefault();
                string sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                search = search.Replace(" ", "");
                int pageSize = Convert.ToInt32(length);
                int skip = Convert.ToInt32(start) / pageSize;

                CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

                int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                string busquedadoc_tercero = "";
                string busquedaprinom_tercero = "";
                string busquedatpdoc_id = "";
                string busquedanumero = "";
                string busquedadocumento = "";
                string busquedabodccs_nombre = "";
                string busquedavalor_total = "";
                string busquedanumeroCompra = "";
                string busquedafecha = "";
                string[] vectorNombresFiltro = !string.IsNullOrEmpty(filtros) ? filtros.Split(',') : new string[1];
                string[] vectorValoresFiltro = !string.IsNullOrEmpty(valorFiltros) ? valorFiltros.Split(',') : new string[1];
                for (int i = 0; i < vectorNombresFiltro.Length; i++)
                {
                    busquedadoc_tercero = busquedadoc_tercero == ""
                        ? vectorNombresFiltro[i] == "doc_tercero" ? vectorValoresFiltro[i] : ""
                        : busquedadoc_tercero;
                    busquedaprinom_tercero = busquedaprinom_tercero == ""
                        ? vectorNombresFiltro[i] == "prinom_tercero" ? vectorValoresFiltro[i] : ""
                        : busquedaprinom_tercero;
                    busquedatpdoc_id = busquedatpdoc_id == ""
                        ? vectorNombresFiltro[i] == "tpdoc_id" ? vectorValoresFiltro[i] : ""
                        : busquedatpdoc_id;
                    busquedanumero = busquedanumero == ""
                        ? vectorNombresFiltro[i] == "numero" ? vectorValoresFiltro[i] : ""
                        : busquedanumero;
                    busquedadocumento = busquedadocumento == ""
                        ? vectorNombresFiltro[i] == "documento" ? vectorValoresFiltro[i] : ""
                        : busquedadocumento;
                    busquedabodccs_nombre = busquedabodccs_nombre == ""
                        ? vectorNombresFiltro[i] == "bodccs_nombre" ? vectorValoresFiltro[i] : ""
                        : busquedabodccs_nombre;
                    busquedavalor_total = busquedavalor_total == ""
                        ? vectorNombresFiltro[i] == "valor_total" ? vectorValoresFiltro[i] : ""
                        : busquedavalor_total;
                    busquedanumeroCompra = busquedanumeroCompra == ""
                        ? vectorNombresFiltro[i] == "numeroCompra" ? vectorValoresFiltro[i] : ""
                        : busquedanumeroCompra;
                    busquedafecha = busquedafecha == ""
                        ? vectorNombresFiltro[i] == "fecha" ? vectorValoresFiltro[i] : ""
                        : busquedafecha;
                }

                string fechaFiltroFormateada = !string.IsNullOrEmpty(busquedafecha)
                    ? Convert.ToDateTime(busquedafecha).ToString("dd/MM/yyyy")
                    : "";
                fechaFiltroFormateada = fechaFiltroFormateada != ""
                    ? "and encabezado.fecha between '" + fechaFiltroFormateada + "' and dateadd(day,1,('" +
                      fechaFiltroFormateada + "'))"
                    : "";
                string consultaDocumentoFiltro = busquedatpdoc_id != "" ? "and encabezado.tipo = " + busquedatpdoc_id : "";

                string queryString =
                    @"select encabezado.idencabezado, tp_doc_registros.prefijo, tp_doc_registros.tpdoc_nombre
                                            , encabezado.documento ,encabezado.numero, bodega_concesionario.bodccs_cod,
                                            bodega_concesionario.bodccs_nombre,
                                            icb_terceros.razon_social,icb_terceros.prinom_tercero,
                                            icb_terceros.segnom_tercero,icb_terceros.apellido_tercero,
                                            icb_terceros.segapellido_tercero,icb_terceros.doc_tercero,
                                            encabezado.fecha,encabezado.valor_total,
                                            compra.numero as numeroCompra,compra.documento as planMayor,
                                            CONCAT(YEAR(encabezado.fecha),'/',MONTH(encabezado.fecha),'/',DAY(encabezado.fecha))
                                            from encab_documento as encabezado join tp_doc_registros on encabezado.tipo = tp_doc_registros.tpdoc_id
                                            join bodega_concesionario on encabezado.bodega = bodega_concesionario.id
                                            join icb_terceros on encabezado.nit = icb_terceros.tercero_id
                                            join encab_documento as compra on encabezado.documento = LTRIM(STR(compra.numero))
                                            where tp_doc_registros.tipo = 7 and bodega_concesionario.id = @bodegaActual " +
                    fechaFiltroFormateada + consultaDocumentoFiltro + @" and(
                                            icb_terceros.doc_tercero LIKE ISNULL('%' + @doc_tercero + '%', icb_terceros.doc_tercero) AND
                                            compra.documento LIKE ISNULL('%' + @documento + '%', compra.documento) AND
                                            encabezado.numero LIKE ISNULL('%' + @numero + '%', encabezado.numero) AND
                                            CONCAT(icb_terceros.prinom_tercero,icb_terceros.segnom_tercero,icb_terceros.apellido_tercero
                                            ,icb_terceros.segapellido_tercero) LIKE ISNULL('%' + '' + '%', icb_terceros.doc_tercero) AND
                                            bodega_concesionario.bodccs_nombre LIKE ISNULL('%' + '' + '%', bodega_concesionario.bodccs_nombre)
                                            ) and (
		 icb_terceros.doc_tercero LIKE ISNULL('%' + @parametroGeneral + '%', icb_terceros.doc_tercero) OR
		 tp_doc_registros.prefijo LIKE ISNULL('%' + @parametroGeneral + '%', tp_doc_registros.prefijo) OR
		 tp_doc_registros.tpdoc_nombre LIKE ISNULL('%' + @parametroGeneral + '%', tp_doc_registros.tpdoc_nombre) OR
		 encabezado.numero LIKE ISNULL('%' + @parametroGeneral + '%', encabezado.numero) OR
		 compra.documento LIKE ISNULL('%' + @parametroGeneral + '%', compra.documento) OR
		 bodega_concesionario.bodccs_cod LIKE ISNULL('%' + @parametroGeneral + '%', bodega_concesionario.bodccs_cod) OR
		 bodega_concesionario.bodccs_nombre LIKE ISNULL('%' + @parametroGeneral + '%', bodega_concesionario.bodccs_nombre) OR
         CONCAT(icb_terceros.razon_social,icb_terceros.prinom_tercero,' ',icb_terceros.segnom_tercero,' ', 
		 icb_terceros.apellido_tercero,' ',icb_terceros.segapellido_tercero) LIKE ISNULL('%' + @parametroGeneral + '%', 
		 CONCAT(icb_terceros.razon_social,icb_terceros.prinom_tercero,' ',icb_terceros.segnom_tercero,' ',
		 icb_terceros.apellido_tercero,' ',icb_terceros.segapellido_tercero)) OR
		 encabezado.fecha LIKE ISNULL('%' + @parametroGeneral + '%', encabezado.fecha)
		 ) ORDER BY encabezado.idencabezado
                                            OFFSET(@pagina) * @pageSize ROWS FETCH NEXT @pageSize ROWS ONLY";

                string queryStringCount = @"select count(*)
                                            from encab_documento as encabezado join tp_doc_registros on encabezado.tipo = tp_doc_registros.tpdoc_id
                                            join bodega_concesionario on encabezado.bodega = bodega_concesionario.id
                                            join icb_terceros on encabezado.nit = icb_terceros.tercero_id
                                            join encab_documento as compra on encabezado.documento = LTRIM(STR(compra.numero))
                                            where tp_doc_registros.tipo = 7 and bodega_concesionario.id = @bodegaActual " +
                                       fechaFiltroFormateada + consultaDocumentoFiltro + @" and(
                                            icb_terceros.doc_tercero LIKE ISNULL('%' + @doc_tercero + '%', icb_terceros.doc_tercero) AND
                                            compra.documento LIKE ISNULL('%' + @documento + '%', compra.documento) AND
                                            encabezado.numero LIKE ISNULL('%' + @numero + '%', encabezado.numero) AND
                                            CONCAT(icb_terceros.prinom_tercero,icb_terceros.segnom_tercero,icb_terceros.apellido_tercero
                                            ,icb_terceros.segapellido_tercero) LIKE ISNULL('%' + '' + '%', icb_terceros.doc_tercero) AND
                                            bodega_concesionario.bodccs_nombre LIKE ISNULL('%' + '' + '%', bodega_concesionario.bodccs_nombre)
                                            ) and (
		 icb_terceros.doc_tercero LIKE ISNULL('%' + @parametroGeneral + '%', icb_terceros.doc_tercero) OR
		 tp_doc_registros.prefijo LIKE ISNULL('%' + @parametroGeneral + '%', tp_doc_registros.prefijo) OR
		 tp_doc_registros.tpdoc_nombre LIKE ISNULL('%' + @parametroGeneral + '%', tp_doc_registros.tpdoc_nombre) OR
		 encabezado.numero LIKE ISNULL('%' + @parametroGeneral + '%', encabezado.numero) OR
		 compra.documento LIKE ISNULL('%' + @parametroGeneral + '%', compra.documento) OR
		 bodega_concesionario.bodccs_cod LIKE ISNULL('%' + @parametroGeneral + '%', bodega_concesionario.bodccs_cod) OR
		 bodega_concesionario.bodccs_nombre LIKE ISNULL('%' + @parametroGeneral + '%', bodega_concesionario.bodccs_nombre) OR
         CONCAT(icb_terceros.razon_social,icb_terceros.prinom_tercero,' ',icb_terceros.segnom_tercero,' ', 
		 icb_terceros.apellido_tercero,' ',icb_terceros.segapellido_tercero) LIKE ISNULL('%' + @parametroGeneral + '%', 
		 CONCAT(icb_terceros.razon_social,icb_terceros.prinom_tercero,' ',icb_terceros.segnom_tercero,' ',
		 icb_terceros.apellido_tercero,' ',icb_terceros.segapellido_tercero)) OR
		 encabezado.fecha LIKE ISNULL('%' + @parametroGeneral + '%', encabezado.fecha)
		 )";

                string connectionString =
                    @"Data Source=WIN-DESARROLLO\DEVSQLSERVER;Initial Catalog=Iceberg_Project2;User ID=simplexwebuser;Password=Iceberg05;MultipleActiveResultSets=True;Application Name=EntityFramework";

                List<ConsultaFiltradaModel> lista = new List<ConsultaFiltradaModel>();
                int count = 0;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    SqlCommand commandCount = new SqlCommand(queryStringCount, connection);
                    command.Parameters.AddWithValue("@pagina", skip);
                    command.Parameters.AddWithValue("@pageSize", pageSize);
                    command.Parameters.AddWithValue("@parametroGeneral", filtroGeneral);
                    command.Parameters.AddWithValue("@doc_tercero",
                        busquedadoc_tercero != null ? busquedadoc_tercero : "");
                    command.Parameters.AddWithValue("@documento", busquedadocumento != null ? busquedadocumento : "");
                    command.Parameters.AddWithValue("@fecha", busquedafecha);
                    command.Parameters.AddWithValue("@bodegaActual", bodegaActual);
                    command.Parameters.AddWithValue("@numero", busquedanumero);
                    commandCount.Parameters.AddWithValue("@parametroGeneral", filtroGeneral);
                    commandCount.Parameters.AddWithValue("@documento",
                        busquedadocumento != null ? busquedadocumento : "");
                    commandCount.Parameters.AddWithValue("@doc_tercero",
                        busquedadoc_tercero != null ? busquedadoc_tercero : "");
                    commandCount.Parameters.AddWithValue("@numero", busquedanumero);
                    commandCount.Parameters.AddWithValue("@bodegaActual", bodegaActual);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    SqlDataReader readerCount = commandCount.ExecuteReader();

                    try
                    {
                        if (readerCount.Read())
                        {
                            count = readerCount.GetInt32(0);
                        }

                        while (reader.Read())
                        {
                            int idDevolucion = Convert.ToInt32(reader["idencabezado"]);
                            int numeroDevolucion = Convert.ToInt32(reader["numero"]);
                            DateTime fecha = Convert.ToDateTime(reader["fecha"]);
                            decimal valor = Convert.ToDecimal(reader["valor_total"], miCultura);
                            int numeroCompra = Convert.ToInt32(reader["numeroCompra"]);
                            string nombre = "";
                            nombre += reader["razon_social"] != DBNull.Value ? (string)reader["razon_social"] : "";
                            nombre += reader["prinom_tercero"] != DBNull.Value
                                ? (string)reader["prinom_tercero"] + " "
                                : "";
                            nombre += reader["segnom_tercero"] != DBNull.Value
                                ? (string)reader["segnom_tercero"] + " "
                                : "";
                            nombre += reader["apellido_tercero"] != DBNull.Value
                                ? (string)reader["apellido_tercero"] + " "
                                : "";
                            nombre += reader["segapellido_tercero"] != DBNull.Value
                                ? (string)reader["segapellido_tercero"]
                                : "";

                            lista.Add(new ConsultaFiltradaModel
                            {
                                Columna0 = "(" + (string)reader["prefijo"] + ") " + (string)reader["tpdoc_nombre"],
                                Columna1 = numeroDevolucion.ToString(),
                                Columna2 = (string)reader["planMayor"],
                                Columna3 =
                                    "(" + (string)reader["bodccs_cod"] + ") " + (string)reader["bodccs_nombre"],
                                Columna4 = (string)reader["doc_tercero"],
                                Columna5 = nombre,
                                Columna6 = fecha.ToString("yyyy/MM/dd"),
                                Columna7 = "$ " + Math.Round(valor).ToString("0,0", elGR),
                                Columna8 = numeroCompra.ToString(),
                                Columna9 = idDevolucion.ToString()
                            });
                        }
                    }
                    finally
                    {
                        // Always call Close when done reading.
                        reader.Close();
                    }
                }
                //var dataSQL = (from encabezado in context.encab_documento
                //            join tipoDocumento in context.tp_doc_registros
                //            on encabezado.tipo equals tipoDocumento.tpdoc_id
                //            join bodega in context.bodega_concesionario
                //            on encabezado.bodega equals bodega.id
                //            join terceros in context.icb_terceros
                //            on encabezado.nit equals terceros.tercero_id
                //            join compra in context.encab_documento
                //            on encabezado.documento equals SqlFunctions.StringConvert((double)compra.numero).Trim()
                //            where tipoDocumento.tipo == 7 && encabezado.bodega == bodegaActual && (terceros.doc_tercero.Contains(search) || (terceros.prinom_tercero+terceros.segnom_tercero+terceros.apellido_tercero+terceros.segapellido_tercero).Contains(search)
                //            || tipoDocumento.tpdoc_nombre.Contains(search) || SqlFunctions.StringConvert((double)encabezado.numero).Trim().Contains(search) || compra.documento.Contains(search)
                //            || bodega.bodccs_nombre.Trim().Replace(" ","").Contains(search) || SqlFunctions.StringConvert((double)encabezado.valor_total).Trim().Contains(search)
                //            || SqlFunctions.StringConvert((double)compra.numero).Trim().Contains(search)
                //            || (encabezado.fecha.Year + "/" + encabezado.fecha.Month + "/" + encabezado.fecha.Day).Contains(search))
                //            select new {
                //                encabezado.idencabezado,
                //                tpdoc_nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                //                numero_devolucion = encabezado.numero,
                //                encabezado.documento,
                //                bodccs_nombre = "(" + bodega.bodccs_cod + ") " + bodega.bodccs_nombre,
                //                nombreCompleto = terceros.razon_social + terceros.prinom_tercero + " " + terceros.segnom_tercero + " " + terceros.apellido_tercero + " " + terceros.segapellido_tercero,
                //                terceros.doc_tercero,
                //                fecha = encabezado.fecha.Year + "/" + encabezado.fecha.Month +"/" + encabezado.fecha.Day,
                //                valor_total = encabezado.valor_total,
                //                numero_compra = compra.numero,
                //                plan_mayor_compra = compra.documento
                //            }).OrderBy(x => x.idencabezado).Skip(skip).Take(pageSize).ToList();

                //var data = dataSQL.Select(x=>new {
                //    x.idencabezado,
                //    x.tpdoc_nombre,
                //    x.numero_devolucion,
                //    x.documento,
                //    x.bodccs_nombre,
                //    x.nombreCompleto,
                //    x.doc_tercero,
                //    x.fecha,
                //    valor_total = "$ " + x.valor_total.ToString("0,0",elGR),
                //    x.numero_compra,
                //    x.plan_mayor_compra 
                //});


                //var totalRecords = (from encabezado in context.encab_documento
                //                   join tipoDocumento in context.tp_doc_registros
                //                   on encabezado.tipo equals tipoDocumento.tpdoc_id
                //                   join bodega in context.bodega_concesionario
                //                   on encabezado.bodega equals bodega.id
                //                   join terceros in context.icb_terceros
                //                   on encabezado.nit equals terceros.tercero_id
                //                   join compra in context.encab_documento
                //                   on encabezado.documento equals SqlFunctions.StringConvert((double)compra.numero).Trim()
                //                    where tipoDocumento.tipo == 7 && encabezado.bodega == bodegaActual && (terceros.doc_tercero.Contains(search) || (terceros.prinom_tercero + terceros.segnom_tercero + terceros.apellido_tercero + terceros.segapellido_tercero).Contains(search)
                //                    || tipoDocumento.tpdoc_nombre.Contains(search) || SqlFunctions.StringConvert((double)encabezado.numero).Trim().Contains(search) || compra.documento.Contains(search)
                //                    || bodega.bodccs_nombre.Trim().Replace(" ", "").Contains(search) || SqlFunctions.StringConvert((double)encabezado.valor_total).Trim().Contains(search)
                //                    || SqlFunctions.StringConvert((double)compra.numero).Trim().Contains(search)
                //                    || (encabezado.fecha.Year + "/" + encabezado.fecha.Month + "/" + encabezado.fecha.Day).Contains(search))
                //                    select encabezado.idencabezado).Count();

                return Json(new { draw, recordsFiltered = count, recordsTotal = count, data = lista },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { errorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
                // Exception
            }
        }

        public JsonResult BuscarTerceroPorDocumento(string documento)
        {
            icb_terceros buscarTercero = context.icb_terceros.Where(x => x.doc_tercero == documento).FirstOrDefault();
            DateTime fecha = DateTime.Now;
            if (buscarTercero != null)
            {
                var data = new
                {
                    idtipodoc = buscarTercero.tpdoc_id,
                    tipodoc = context.tp_documento.Where(g => g.tpdoc_id == buscarTercero.tpdoc_id).FirstOrDefault()
                        .tpdoc_nombre,
                    buscarTercero.doc_tercero,
                    buscarTercero.prinom_tercero,
                    buscarTercero.segnom_tercero,
                    buscarTercero.apellido_tercero,
                    buscarTercero.segapellido_tercero,
                    buscarTercero.telf_tercero,
                    buscarTercero.celular_tercero,
                    buscarTercero.email_tercero,
                    buscarTercero.razon_social,
                    habdtautor_celular = buscarTercero.habdtautor_celular ? "SI" : "NO",
                    habdtautor_correo = buscarTercero.habdtautor_correo ? "SI" : "NO",
                    habdtautor_msm = buscarTercero.habdtautor_msm ? "SI" : "NO",
                    habdtautor_watsap = buscarTercero.habdtautor_watsap ? "SI" : "NO",
                    genero_tecero = buscarTercero.genero_tercero == 1 ? "Masculino" : "Femenino",
                    //buscarTercero.direc_tercero,
                    fec_nacimiento = buscarTercero.fec_nacimiento != null
                        ? buscarTercero.fec_nacimiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-Us"))
                        : "",
                    edad = buscarTercero.fec_nacimiento != null
                        ? ((fecha - buscarTercero.fec_nacimiento.Value).Days / 365).ToString("N0",
                            new CultureInfo("is-IS"))
                        : ""
                };
                //obtengo las direcciones del tercero
                var direcciones = context.terceros_direcciones.Where(d => d.idtercero == buscarTercero.tercero_id)
                    .Select(s => new
                    {
                        ciudad = s.nom_ciudad.ciu_nombre,
                        sector = s.sector != null ? s.nom_sector.sec_nombre : "",
                        s.direccion
                    }).ToList();
                //obtengo los contactos del tercero
                List<icb_contacto_tercero> contactos_tercero = context.icb_contacto_tercero
                    .Where(d => d.tercero_id == buscarTercero.tercero_id).ToList();
                var contactox = contactos_tercero.Select(d => new
                {
                    nombre = d.con_tercero_nombre,
                    direccion = !string.IsNullOrWhiteSpace(d.con_tercero_direccion) ? d.con_tercero_direccion : "",
                    tipo_documento = d.tipo_documento != null
                        ? context.tp_documento.Where(g => g.tpdoc_id == d.tipo_documento).FirstOrDefault().tpdoc_nombre
                        : "",
                    tipo_contacto = d.tipocontacto,
                    fechacumpleanos = d.fecha_cumpleaños != null
                        ? d.fecha_cumpleaños.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                        : "",
                    cedula = !string.IsNullOrWhiteSpace(d.cedula) ? d.cedula : "",
                    correo = !string.IsNullOrWhiteSpace(d.mail) ? d.mail : "",
                    telefono = !string.IsNullOrWhiteSpace(d.con_tercero_telefono) ? d.con_tercero_telefono : "",
                    estado_contacto = d.con_terceroestado ? "ACTIVO" : "INACTIVO"
                }).ToList();


                var buscarCotizaciones = (from cotizacion in context.icb_cotizacion
                                          join tercero in context.icb_terceros
                                              on cotizacion.id_tercero equals tercero.tercero_id
                                          join detalle in context.vcotdetallevehiculo
                                              on cotizacion.cot_numcotizacion equals detalle.idcotizacion
                                          join modelo in context.modelo_vehiculo
                                              on detalle.idmodelo equals modelo.modvh_codigo
                                          join color in context.color_vehiculo
                                              on detalle.color equals color.colvh_id
                                          where tercero.doc_tercero == documento
                                          select new
                                          {
                                              detalle.idcotizacion,
                                              modelo.modvh_nombre,
                                              detalle.anomodelo,
                                              color.colvh_nombre,
                                              detalle.precio,
                                              cotizacion.cot_feccreacion
                                          }).OrderBy(x => x.cot_feccreacion).ToList();

                var dataCotizacion = buscarCotizaciones.Select(x => new
                {
                    x.idcotizacion,
                    x.modvh_nombre,
                    x.anomodelo,
                    x.colvh_nombre,
                    x.precio,
                    cot_feccreacion = x.cot_feccreacion != null ? x.cot_feccreacion.ToShortDateString() : ""
                });


                var buscarPedidos = (from pedido in context.vpedido
                                     join tercero in context.icb_terceros
                                         on pedido.nit equals tercero.tercero_id
                                     join modelo in context.modelo_vehiculo
                                         on pedido.modelo equals modelo.modvh_codigo
                                     join color in context.color_vehiculo
                                         on pedido.Color_Deseado equals color.colvh_id
                                     where tercero.doc_tercero == documento
                                     select new
                                     {
                                         pedido.fecha,
                                         modelo.modvh_nombre,
                                         pedido.id_anio_modelo,
                                         color.colvh_nombre,
                                         pedido.valor_unitario
                                     }).ToList();

                var dataPedidos = buscarPedidos.Select(x => new
                {
                    fecha = x.fecha.ToShortDateString(),
                    x.modvh_nombre,
                    x.id_anio_modelo,
                    x.colvh_nombre,
                    valor_unitario = x.valor_unitario != null ? x.valor_unitario : 0
                });
                return Json(
                    new
                    {
                        encontrado = true,
                        tercero = data,
                        direcciones,
                        contactox,
                        vehiculos = dataCotizacion,
                        pedidos = dataPedidos
                    }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { encontrado = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult TraerOrdenes(string tercero)
        {
            var ordenes = (from c in context.vw_ordenes_taller
                           orderby c.orden
                           where c.tercero == tercero
                           select new
                           {
                               c.orden,
                               bodega = c.bodccs_nombre,
                               c.kilometraje,
                               c.user_nombre,
                               c.fecha_entrega,
                               c.estado,
                               c.tipo_orden
                           }
                ).Distinct().ToList();
            string cuerpo = "";
            if (ordenes.Count != 0)
            {
                foreach (var orden in ordenes)
                {
                    cuerpo += @"
                <tr>
                    <td style=""text-align:right"">" + orden.orden + @"</td>
                    <td>" + orden.bodega + @"</td>
                    <td style=""text-align:right"">" + orden.kilometraje + @"</td>
                    <td>" + orden.user_nombre + @"</td>
                    <td>" + orden.fecha_entrega.ToString("yyyy/MM/dd", new CultureInfo("en-US")) + @"</td>
                    <td>" + orden.estado + @"</td>
                    <td>" + orden.tipo_orden + @"</td>
                    <td><a class=""btn btn-info"" data-toggle=""collapse"" data-target=""#collapse_" + orden.orden +
                              @"""><i class=""fa fa-list""></i></a></td>
                 </tr>";
                    var detallado_ordenes = (from d in context.vw_ordenes_taller
                                             orderby d.orden
                                             where d.orden == orden.orden
                                             select new
                                             {
                                                 d.tipo_operacion,
                                                 d.operacion,
                                                 d.cantidad,
                                                 d.valorunitario,
                                                 d.pordescuento,
                                                 d.poriva,
                                                 d.plan_mantenimiento
                                             }
                        ).ToList();
                    cuerpo += @"
                <tr>
                    <td colspan=""8"">
                        <div id=""collapse_" + orden.orden + @""" class=""collapse"">
                            <table class=""table table-striped table-bordered table-hover table-responsive"" id = ""tdetallado_" +
                              orden.orden + @""" >
                                <thead>
                                    <tr role=""row"" class=""heading"">
                                        <th width=""10%"">Tipo operación</th>
                                        <th width = ""10%"" >operación</th>
                                        <th width = ""10%"" >Cantidad/Tiempo</th>
                                        <th width = ""10%"" >Valor unitario</th>
                                        <th width = ""10%"" >Porcetanje descuento</th>
                                        <th width = ""10%"" >Porcetanje IVA</th>
                                        <th width = ""10%"" >Plan mantenimiento</th>
                                    </tr>
                                </thead>
                                <tbody>";
                    foreach (var detallado in detallado_ordenes)
                    {
                        cuerpo += @"
                                    <tr>
                                        <td>" + detallado.tipo_operacion + @"</td>
                                        <td>" + detallado.operacion + @"</td>
                                        <td style=""text-align:right"">" + detallado.cantidad + @"</td>
                                        <td style=""text-align:right"">" +
                                  detallado.valorunitario.ToString("N0", new CultureInfo("is-IS")) + @"</td>
                                        <td style=""text-align:right"">" + detallado.pordescuento + @"</td>
                                        <td style=""text-align:right"">" + detallado.poriva + @"</td>
                                        <td>" + detallado.plan_mantenimiento + @"</td>
                                    </tr>";
                    }

                    cuerpo += @"
                                </tbody>
                            </table>
                        </div>    
                    </td>
                </tr>";
                }
            }
            else
            {
                cuerpo = "<tr><td colspan =\"8\" style=\"text-align:center\">No se han registrado ordenes</td>";
            }

            return Json(cuerpo);
        }

        public JsonResult BuscarTerceroPorId(int? documento)
        {
            if (documento != null)
            {
                icb_terceros buscarTercero = context.icb_terceros.Where(x => x.tercero_id == documento).FirstOrDefault();
                DateTime fecha = DateTime.Now;
                if (buscarTercero != null)
                {
                    var data = new
                    {
                        idtipodoc = buscarTercero.tpdoc_id,
                        tipodoc = context.tp_documento.Where(g => g.tpdoc_id == buscarTercero.tpdoc_id).FirstOrDefault()
                            .tpdoc_nombre,
                        buscarTercero.doc_tercero,
                        buscarTercero.prinom_tercero,
                        buscarTercero.segnom_tercero,
                        buscarTercero.apellido_tercero,
                        buscarTercero.segapellido_tercero,
                        buscarTercero.telf_tercero,
                        buscarTercero.celular_tercero,
                        buscarTercero.email_tercero,
                        buscarTercero.razon_social,
                        habdtautor_celular = buscarTercero.habdtautor_celular ? "SI" : "NO",
                        habdtautor_correo = buscarTercero.habdtautor_correo ? "SI" : "NO",
                        habdtautor_msm = buscarTercero.habdtautor_msm ? "SI" : "NO",
                        habdtautor_watsap = buscarTercero.habdtautor_watsap ? "SI" : "NO",
                        //buscarTercero.direc_tercero,
                        fec_nacimiento = buscarTercero.fec_nacimiento != null
                            ? buscarTercero.fec_nacimiento.Value.ToString("yyyy/MM/dd", new CultureInfo("en-Us"))
                            : "",
                        edad = buscarTercero.fec_nacimiento != null
                            ? ((fecha - buscarTercero.fec_nacimiento.Value).Days / 365).ToString("N0",
                                new CultureInfo("is-IS"))
                            : ""
                    };
                    //obtengo las direcciones del tercero
                    var direcciones = context.terceros_direcciones.Where(d => d.idtercero == buscarTercero.tercero_id)
                        .Select(s => new
                        {
                            ciudad = s.nom_ciudad.ciu_nombre,
                            sector = s.sector != null ? s.nom_sector.sec_nombre : "",
                            s.direccion
                        }).ToList();
                    //obtengo los contactos del tercero
                    List<icb_contacto_tercero> contactos_tercero = context.icb_contacto_tercero
                        .Where(d => d.tercero_id == buscarTercero.tercero_id).ToList();
                    var contactox = contactos_tercero.Select(d => new
                    {
                        nombre = d.con_tercero_nombre,
                        direccion = !string.IsNullOrWhiteSpace(d.con_tercero_direccion) ? d.con_tercero_direccion : "",
                        tipo_documento = d.tipo_documento != null
                            ? context.tp_documento.Where(g => g.tpdoc_id == d.tipo_documento).FirstOrDefault()
                                .tpdoc_nombre
                            : "",
                        tipo_contacto = d.tipocontacto,
                        fechacumpleanos = d.fecha_cumpleaños != null
                            ? d.fecha_cumpleaños.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                            : "",
                        cedula = !string.IsNullOrWhiteSpace(d.cedula) ? d.cedula : "",
                        correo = !string.IsNullOrWhiteSpace(d.mail) ? d.mail : "",
                        telefono = !string.IsNullOrWhiteSpace(d.con_tercero_telefono) ? d.con_tercero_telefono : "",
                        estado_contacto = d.con_terceroestado ? "ACTIVO" : "INACTIVO"
                    }).ToList();


                    var buscarCotizaciones = (from cotizacion in context.icb_cotizacion
                                              join tercero in context.icb_terceros
                                                  on cotizacion.id_tercero equals tercero.tercero_id
                                              join detalle in context.vcotdetallevehiculo
                                                  on cotizacion.cot_numcotizacion equals detalle.idcotizacion
                                              join modelo in context.modelo_vehiculo
                                                  on detalle.idmodelo equals modelo.modvh_codigo
                                              join color in context.color_vehiculo
                                                  on detalle.color equals color.colvh_id
                                              where tercero.tercero_id == documento
                                              select new
                                              {
                                                  detalle.idcotizacion,
                                                  modelo.modvh_nombre,
                                                  detalle.anomodelo,
                                                  color.colvh_nombre,
                                                  detalle.precio,
                                                  cotizacion.cot_feccreacion
                                              }).OrderBy(x => x.cot_feccreacion).ToList();

                    var dataCotizacion = buscarCotizaciones.Select(x => new
                    {
                        x.idcotizacion,
                        x.modvh_nombre,
                        x.anomodelo,
                        x.colvh_nombre,
                        x.precio,
                        cot_feccreacion = x.cot_feccreacion != null ? x.cot_feccreacion.ToShortDateString() : ""
                    });


                    var buscarPedidos = (from pedido in context.vpedido
                                         join tercero in context.icb_terceros
                                             on pedido.nit equals tercero.tercero_id
                                         join modelo in context.modelo_vehiculo
                                             on pedido.modelo equals modelo.modvh_codigo
                                         join color in context.color_vehiculo
                                             on pedido.Color_Deseado equals color.colvh_id
                                         where tercero.tercero_id == documento
                                         select new
                                         {
                                             pedido.fecha,
                                             modelo.modvh_nombre,
                                             pedido.id_anio_modelo,
                                             color.colvh_nombre,
                                             pedido.valor_unitario
                                         }).ToList();

                    var dataPedidos = buscarPedidos.Select(x => new
                    {
                        fecha = x.fecha.ToShortDateString(),
                        x.modvh_nombre,
                        x.id_anio_modelo,
                        x.colvh_nombre,
                        valor_unitario = x.valor_unitario != null ? x.valor_unitario : 0
                    });
                    return Json(
                        new
                        {
                            encontrado = true,
                            tercero = data,
                            direcciones,
                            contactox,
                            vehiculos = dataCotizacion,
                            pedidos = dataPedidos
                        }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { encontrado = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { encontrado = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult modelospormarca(string ids)
        {
            if (!string.IsNullOrWhiteSpace(ids))
            {
                string[] modelos = ids.Split(',');
                Expression<Func<modelo_vehiculo, bool>> predicado = PredicateBuilder.True<modelo_vehiculo>();
                Expression<Func<modelo_vehiculo, bool>> predicado2 = PredicateBuilder.False<modelo_vehiculo>();

                foreach (string item in modelos)
                {
                    bool conv = int.TryParse(item, out int numero);
                    predicado2 = predicado2.Or(c => c.mar_vh_id == numero);
                }

                predicado = predicado.And(predicado2);
                List<modelo_vehiculo> modelos2 = context.modelo_vehiculo.Where(predicado).ToList();
                //var modelos2 = context.vw_modelos_nombre.Where(d=>d.mar_vh_id==1 || d.mar_vh_id==2).ToList();


                var modelos3 = modelos2.Select(d => new
                {
                    value = word(d.modvh_nombre)
                }).ToList();
                var modelos4 = modelos3.GroupBy(d => d.value).Select(d => new { value = d.Key, text = d.Key.ToUpper() })
                    .ToList();
                return Json(modelos4, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);
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

        public string word(string original)
        {
            string palabra = "";
            palabra = original.IndexOf(" ") > -1
                ? original.Substring(0, original.IndexOf(" "))
                : original;
            return palabra;
        }

        [HttpPost]
        public ActionResult nuevaCampana(browsercampana datos)
        {
            return View();
        }


        public class browsercampana
        {
            public string filtros { get; set; }
            public string valorFiltros { get; set; }
            public string filtroGeneral { get; set; }
        }

        public class datatablex
        {
            public int id { get; set; }
            public string Columna0 { get; set; }
            public string Columna1 { get; set; }
            public string Columna2 { get; set; }
            public int? Columna3 { get; set; }
            public string Columna4 { get; set; }
            public string Columna5 { get; set; }
            public string Columna6 { get; set; }
            public string Columna7 { get; set; }
            public string Columna8 { get; set; }
        }
    }
}