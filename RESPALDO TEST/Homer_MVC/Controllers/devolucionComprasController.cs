using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class devolucionComprasController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");

        // GET: devolucionCompras
        public ActionResult Index(int? menu)
        {
            var buscarTipoDocumento = (from tipoDocumento in context.tp_doc_registros
                                       select new
                                       {
                                           tipoDocumento.tpdoc_id,
                                           nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                           tipoDocumento.tipo
                                       }).ToList();
            ViewBag.tipo_documentoFiltro =
                new SelectList(buscarTipoDocumento.Where(x => x.tipo == 1), "tpdoc_id", "nombre");
            ViewBag.tipo_documento = new SelectList(buscarTipoDocumento.Where(x => x.tipo == 7), "tpdoc_id", "nombre");
            ViewBag.tipo_documentoDevAuto =
                new SelectList(buscarTipoDocumento.Where(x => x.tipo == 7), "tpdoc_id", "nombre");
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult BuscarDatosFiltradosPestanaDos(string filtros, string valorFiltros)
        {
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarFiltrosPestanaDos(int id_menu)
        {
            var buscarFiltros = (from menuBusqueda in context.menu_busqueda
                                 where menuBusqueda.menu_busqueda_id_menu == id_menu && menuBusqueda.menu_busqueda_id_pestana == 2
                                 select new
                                 {
                                     menuBusqueda.menu_busqueda_nombre,
                                     menuBusqueda.menu_busqueda_tipo_campo,
                                     menuBusqueda.menu_busqueda_campo,
                                     menuBusqueda.menu_busqueda_consulta
                                 }).ToList();

            List<ListaFiltradaModel> listas = new List<ListaFiltradaModel>();
            foreach (var item in buscarFiltros)
            {
                if (item.menu_busqueda_tipo_campo == "select")
                {
                    string queryString = item.menu_busqueda_consulta;
                    string connectionString =
                        @"Data Source=WIN-DESARROLLO\DEVSQLSERVER;Initial Catalog=Iceberg_Project2;User ID=simplexwebuser;Password=Iceberg05;MultipleActiveResultSets=True;Application Name=EntityFramework";
                    ListaFiltradaModel nuevaLista = new ListaFiltradaModel
                    {
                        NombreAMostrar = item.menu_busqueda_nombre,
                        NombreCampo = item.menu_busqueda_campo
                    };

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand command = new SqlCommand(queryString, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        try
                        {
                            while (reader.Read())
                            {
                                int id = Convert.ToInt32(reader[0]);
                                object valor = reader[1];
                                nuevaLista.items.Add(new SelectListItem
                                { Text = (string)valor, Value = Convert.ToString(id) });
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

            return Json(new { buscarFiltros, listas }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDocumentosFiltro(DateTime? desde, DateTime? hasta, int? id_documento, int? factura)
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);

            if (desde == null)
            {
                desde = DateTime.Now.AddDays(-60);
            }

            if (hasta == null)
            {
                hasta = DateTime.Now.AddDays(1);
            }

            int userLog = Convert.ToInt32(Session["user_usuarioid"]);
            //Validamos que el usuario loguado tenga el rol y el permiso para ver toda la informacion
            int permiso = (from u in context.users
                           join r in context.rols
                               on u.rol_id equals r.rol_id
                           join ra in context.rolacceso
                               on r.rol_id equals ra.idrol
                           where u.user_id == userLog && ra.idpermiso == 2
                           select new
                           {
                               u.user_id,
                               u.rol_id,
                               r.rol_nombre,
                               ra.idpermiso
                           }).Count();
            if (permiso > 0)
            {
                if (factura != null && id_documento == null)
                {
                    #region factura            

                    var buscarVehiculosComprados = (from encabezado in context.encab_documento
                                                    join usuario in context.users
                                                        on encabezado.userid_creacion equals usuario.user_id
                                                    join tipoDocumento in context.tp_doc_registros
                                                        on encabezado.tipo equals tipoDocumento.tpdoc_id
                                                    join vehiculo in context.icb_vehiculo
                                                        on encabezado.documento equals vehiculo.plan_mayor
                                                    join modelo in context.modelo_vehiculo
                                                        on vehiculo.modvh_id equals modelo.modvh_codigo
                                                    join creador in context.users
                                                        on encabezado.userid_creacion equals creador.user_id
                                                    join bodega in context.bodega_concesionario
                                                        on encabezado.bodega equals bodega.id
                                                    join tercero in context.icb_terceros
                                                        on encabezado.nit equals tercero.tercero_id
                                                    where encabezado.numero == factura && tipoDocumento.tipo == 1
                                                    select new
                                                    {
                                                        encabezado.idencabezado,
                                                        tpdoc_nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                                        encabezado.numero,
                                                        vehiculo.plan_mayor,
                                                        modelo.modvh_nombre,
                                                        creador = creador.user_nombre + " " + creador.user_apellido,
                                                        vehiculo.icbvh_id,
                                                        bodega.bodccs_cod,
                                                        bodega.bodccs_nombre,
                                                        tercero.doc_tercero,
                                                        proveedor = tercero.razon_social + tercero.prinom_tercero + " " + tercero.segnom_tercero +
                                                                    " " + tercero.apellido_tercero + " " + tercero.segapellido_tercero,
                                                        encabezado.fecha,
                                                        factura = encabezado.documento,
                                                        usuario = usuario.user_nombre + " " + usuario.user_apellido
                                                    }).ToList();

                    var data = buscarVehiculosComprados.Select(x => new
                    {
                        x.idencabezado,
                        x.tpdoc_nombre,
                        x.numero,
                        x.plan_mayor,
                        x.modvh_nombre,
                        x.creador,
                        x.icbvh_id,
                        x.bodccs_cod,
                        x.bodccs_nombre,
                        x.doc_tercero,
                        x.proveedor,
                        x.usuario,
                        fecha = x.fecha.ToString("yyyy/MM/dd"),
                        x.factura
                    });
                    return Json(data, JsonRequestBehavior.AllowGet);

                    #endregion
                }

                if (id_documento != null && factura == null)
                {
                    #region id_documento

                    var buscarVehiculosComprados = (from encabezado in context.encab_documento
                                                    join usuario in context.users
                                                        on encabezado.userid_creacion equals usuario.user_id
                                                    join tipoDocumento in context.tp_doc_registros
                                                        on encabezado.tipo equals tipoDocumento.tpdoc_id
                                                    join vehiculo in context.icb_vehiculo
                                                        on encabezado.documento equals vehiculo.plan_mayor
                                                    join modelo in context.modelo_vehiculo
                                                        on vehiculo.modvh_id equals modelo.modvh_codigo
                                                    join creador in context.users
                                                        on encabezado.userid_creacion equals creador.user_id
                                                    join bodega in context.bodega_concesionario
                                                        on encabezado.bodega equals bodega.id
                                                    join tercero in context.icb_terceros
                                                        on encabezado.nit equals tercero.tercero_id
                                                    where encabezado.tipo == id_documento && encabezado.fecha >= desde && encabezado.fecha <= hasta
                                                    select new
                                                    {
                                                        encabezado.idencabezado,
                                                        tpdoc_nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                                        encabezado.numero,
                                                        vehiculo.plan_mayor,
                                                        modelo.modvh_nombre,
                                                        creador = creador.user_nombre + " " + creador.user_apellido,
                                                        vehiculo.icbvh_id,
                                                        bodega.bodccs_cod,
                                                        bodega.bodccs_nombre,
                                                        tercero.doc_tercero,
                                                        proveedor = tercero.razon_social + tercero.prinom_tercero + " " + tercero.segnom_tercero +
                                                                    " " + tercero.apellido_tercero + " " + tercero.segapellido_tercero,
                                                        encabezado.fecha,
                                                        usuario = usuario.user_nombre + " " + usuario.user_apellido,
                                                        factura = encabezado.documento
                                                    }).ToList();
                    var data = buscarVehiculosComprados.Select(x => new
                    {
                        x.idencabezado,
                        x.tpdoc_nombre,
                        x.numero,
                        x.plan_mayor,
                        x.modvh_nombre,
                        x.creador,
                        x.icbvh_id,
                        x.bodccs_cod,
                        x.bodccs_nombre,
                        x.doc_tercero,
                        x.proveedor,
                        x.usuario,
                        fecha = x.fecha.ToString("yyyy/MM/dd"),
                        x.factura
                    });
                    return Json(data, JsonRequestBehavior.AllowGet);

                    #endregion
                }

                if (id_documento != null && factura != null)
                {
                    #region id_documento y factura

                    var buscarVehiculosComprados = (from encabezado in context.encab_documento
                                                    join usuario in context.users
                                                        on encabezado.userid_creacion equals usuario.user_id
                                                    join tipoDocumento in context.tp_doc_registros
                                                        on encabezado.tipo equals tipoDocumento.tpdoc_id
                                                    join vehiculo in context.icb_vehiculo
                                                        on encabezado.documento equals vehiculo.plan_mayor
                                                    join modelo in context.modelo_vehiculo
                                                        on vehiculo.modvh_id equals modelo.modvh_codigo
                                                    join creador in context.users
                                                        on encabezado.userid_creacion equals creador.user_id
                                                    join bodega in context.bodega_concesionario
                                                        on encabezado.bodega equals bodega.id
                                                    join tercero in context.icb_terceros
                                                        on encabezado.nit equals tercero.tercero_id
                                                    where encabezado.tipo == id_documento && encabezado.numero == factura
                                                    select new
                                                    {
                                                        encabezado.idencabezado,
                                                        tpdoc_nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                                        encabezado.numero,
                                                        vehiculo.plan_mayor,
                                                        modelo.modvh_nombre,
                                                        creador = creador.user_nombre + " " + creador.user_apellido,
                                                        vehiculo.icbvh_id,
                                                        bodega.bodccs_cod,
                                                        bodega.bodccs_nombre,
                                                        tercero.doc_tercero,
                                                        proveedor = tercero.razon_social + tercero.prinom_tercero + " " + tercero.segnom_tercero +
                                                                    " " + tercero.apellido_tercero + " " + tercero.segapellido_tercero,
                                                        encabezado.fecha,
                                                        usuario = usuario.user_nombre + " " + usuario.user_apellido,
                                                        factura = encabezado.documento
                                                    }).ToList();
                    var data = buscarVehiculosComprados.Select(x => new
                    {
                        x.idencabezado,
                        x.tpdoc_nombre,
                        x.numero,
                        x.plan_mayor,
                        x.modvh_nombre,
                        x.creador,
                        x.icbvh_id,
                        x.bodccs_cod,
                        x.bodccs_nombre,
                        x.doc_tercero,
                        x.proveedor,
                        x.usuario,
                        fecha = x.fecha.ToString("yyyy/MM/dd"),
                        x.factura
                    });
                    return Json(data, JsonRequestBehavior.AllowGet);

                    #endregion
                }
                else
                {
                    #region por Fechas               

                    var buscarVehiculosComprados = (from encabezado in context.encab_documento
                                                    join usuario in context.users
                                                        on encabezado.userid_creacion equals usuario.user_id
                                                    join tipoDocumento in context.tp_doc_registros
                                                        on encabezado.tipo equals tipoDocumento.tpdoc_id
                                                    join vehiculo in context.icb_vehiculo
                                                        on encabezado.documento equals vehiculo.plan_mayor
                                                    join modelo in context.modelo_vehiculo
                                                        on vehiculo.modvh_id equals modelo.modvh_codigo
                                                    join creador in context.users
                                                        on encabezado.userid_creacion equals creador.user_id
                                                    join bodega in context.bodega_concesionario
                                                        on encabezado.bodega equals bodega.id
                                                    join tercero in context.icb_terceros
                                                        on encabezado.nit equals tercero.tercero_id
                                                    where encabezado.fecha >= desde && encabezado.fecha <= hasta && tipoDocumento.tipo == 1
                                                    select new
                                                    {
                                                        encabezado.idencabezado,
                                                        tpdoc_nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                                        encabezado.numero,
                                                        vehiculo.plan_mayor,
                                                        modelo.modvh_nombre,
                                                        creador = creador.user_nombre + " " + creador.user_apellido,
                                                        vehiculo.icbvh_id,
                                                        bodega.bodccs_cod,
                                                        bodega.bodccs_nombre,
                                                        tercero.doc_tercero,
                                                        proveedor = tercero.razon_social + tercero.prinom_tercero + " " + tercero.segnom_tercero +
                                                                    " " + tercero.apellido_tercero + " " + tercero.segapellido_tercero,
                                                        encabezado.fecha,
                                                        usuario = usuario.user_nombre + " " + usuario.user_apellido,
                                                        factura = encabezado.documento
                                                    }).ToList();

                    var data = buscarVehiculosComprados.Select(x => new
                    {
                        x.idencabezado,
                        x.tpdoc_nombre,
                        x.numero,
                        x.plan_mayor,
                        x.modvh_nombre,
                        x.creador,
                        x.icbvh_id,
                        x.bodccs_cod,
                        x.bodccs_nombre,
                        x.doc_tercero,
                        x.proveedor,
                        x.usuario,
                        x.factura,
                        fecha = x.fecha.ToString("yyyy/MM/dd")
                    });
                    return Json(data, JsonRequestBehavior.AllowGet);

                    #endregion
                }
            }

            if (factura != null && id_documento == null)
            {
                #region factura            

                var buscarVehiculosComprados = (from encabezado in context.encab_documento
                                                join usuario in context.users
                                                    on encabezado.userid_creacion equals usuario.user_id
                                                join tipoDocumento in context.tp_doc_registros
                                                    on encabezado.tipo equals tipoDocumento.tpdoc_id
                                                join vehiculo in context.icb_vehiculo
                                                    on encabezado.documento equals vehiculo.plan_mayor
                                                join modelo in context.modelo_vehiculo
                                                    on vehiculo.modvh_id equals modelo.modvh_codigo
                                                join creador in context.users
                                                    on encabezado.userid_creacion equals creador.user_id
                                                join bodega in context.bodega_concesionario
                                                    on encabezado.bodega equals bodega.id
                                                join tercero in context.icb_terceros
                                                    on encabezado.nit equals tercero.tercero_id
                                                where encabezado.numero == factura && tipoDocumento.tipo == 1
                                                select new
                                                {
                                                    encabezado.idencabezado,
                                                    tpdoc_nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                                    encabezado.numero,
                                                    vehiculo.plan_mayor,
                                                    modelo.modvh_nombre,
                                                    creador = creador.user_nombre + " " + creador.user_apellido,
                                                    vehiculo.icbvh_id,
                                                    bodega.bodccs_cod,
                                                    bodega.bodccs_nombre,
                                                    tercero.doc_tercero,
                                                    proveedor = tercero.razon_social + tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                                tercero.apellido_tercero + " " + tercero.segapellido_tercero,
                                                    encabezado.fecha,
                                                    factura = encabezado.documento,
                                                    usuario = usuario.user_nombre + " " + usuario.user_apellido
                                                }).ToList();

                var data = buscarVehiculosComprados.Select(x => new
                {
                    x.idencabezado,
                    x.tpdoc_nombre,
                    x.numero,
                    x.plan_mayor,
                    x.modvh_nombre,
                    x.creador,
                    x.icbvh_id,
                    x.bodccs_cod,
                    x.bodccs_nombre,
                    x.doc_tercero,
                    x.proveedor,
                    x.usuario,
                    fecha = x.fecha.ToString("yyyy/MM/dd"),
                    x.factura
                });
                return Json(data, JsonRequestBehavior.AllowGet);

                #endregion
            }

            if (id_documento != null && factura == null)
            {
                #region id_documento

                var buscarVehiculosComprados = (from encabezado in context.encab_documento
                                                join usuario in context.users
                                                    on encabezado.userid_creacion equals usuario.user_id
                                                join tipoDocumento in context.tp_doc_registros
                                                    on encabezado.tipo equals tipoDocumento.tpdoc_id
                                                join vehiculo in context.icb_vehiculo
                                                    on encabezado.documento equals vehiculo.plan_mayor
                                                join modelo in context.modelo_vehiculo
                                                    on vehiculo.modvh_id equals modelo.modvh_codigo
                                                join creador in context.users
                                                    on encabezado.userid_creacion equals creador.user_id
                                                join bodega in context.bodega_concesionario
                                                    on encabezado.bodega equals bodega.id
                                                join tercero in context.icb_terceros
                                                    on encabezado.nit equals tercero.tercero_id
                                                where encabezado.bodega == bodegaActual && encabezado.tipo == id_documento &&
                                                      encabezado.fecha >= desde && encabezado.fecha <= hasta
                                                select new
                                                {
                                                    encabezado.idencabezado,
                                                    tpdoc_nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                                    encabezado.numero,
                                                    vehiculo.plan_mayor,
                                                    modelo.modvh_nombre,
                                                    creador = creador.user_nombre + " " + creador.user_apellido,
                                                    vehiculo.icbvh_id,
                                                    bodega.bodccs_cod,
                                                    bodega.bodccs_nombre,
                                                    tercero.doc_tercero,
                                                    proveedor = tercero.razon_social + tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                                tercero.apellido_tercero + " " + tercero.segapellido_tercero,
                                                    encabezado.fecha,
                                                    usuario = usuario.user_nombre + " " + usuario.user_apellido,
                                                    factura = encabezado.documento
                                                }).ToList();
                var data = buscarVehiculosComprados.Select(x => new
                {
                    x.idencabezado,
                    x.tpdoc_nombre,
                    x.numero,
                    x.plan_mayor,
                    x.modvh_nombre,
                    x.creador,
                    x.icbvh_id,
                    x.bodccs_cod,
                    x.bodccs_nombre,
                    x.doc_tercero,
                    x.proveedor,
                    x.usuario,
                    fecha = x.fecha.ToString("yyyy/MM/dd"),
                    x.factura
                });
                return Json(data, JsonRequestBehavior.AllowGet);

                #endregion
            }

            if (id_documento != null && factura != null)
            {
                #region id_documento y factura

                var buscarVehiculosComprados = (from encabezado in context.encab_documento
                                                join usuario in context.users
                                                    on encabezado.userid_creacion equals usuario.user_id
                                                join tipoDocumento in context.tp_doc_registros
                                                    on encabezado.tipo equals tipoDocumento.tpdoc_id
                                                join vehiculo in context.icb_vehiculo
                                                    on encabezado.documento equals vehiculo.plan_mayor
                                                join modelo in context.modelo_vehiculo
                                                    on vehiculo.modvh_id equals modelo.modvh_codigo
                                                join creador in context.users
                                                    on encabezado.userid_creacion equals creador.user_id
                                                join bodega in context.bodega_concesionario
                                                    on encabezado.bodega equals bodega.id
                                                join tercero in context.icb_terceros
                                                    on encabezado.nit equals tercero.tercero_id
                                                where encabezado.bodega == bodegaActual && encabezado.tipo == id_documento &&
                                                      encabezado.numero == factura
                                                select new
                                                {
                                                    encabezado.idencabezado,
                                                    tpdoc_nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                                    encabezado.numero,
                                                    vehiculo.plan_mayor,
                                                    modelo.modvh_nombre,
                                                    creador = creador.user_nombre + " " + creador.user_apellido,
                                                    vehiculo.icbvh_id,
                                                    bodega.bodccs_cod,
                                                    bodega.bodccs_nombre,
                                                    tercero.doc_tercero,
                                                    proveedor = tercero.razon_social + tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                                tercero.apellido_tercero + " " + tercero.segapellido_tercero,
                                                    encabezado.fecha,
                                                    usuario = usuario.user_nombre + " " + usuario.user_apellido,
                                                    factura = encabezado.documento
                                                }).ToList();
                var data = buscarVehiculosComprados.Select(x => new
                {
                    x.idencabezado,
                    x.tpdoc_nombre,
                    x.numero,
                    x.plan_mayor,
                    x.modvh_nombre,
                    x.creador,
                    x.icbvh_id,
                    x.bodccs_cod,
                    x.bodccs_nombre,
                    x.doc_tercero,
                    x.proveedor,
                    x.usuario,
                    fecha = x.fecha.ToString("yyyy/MM/dd"),
                    x.factura
                });
                return Json(data, JsonRequestBehavior.AllowGet);

                #endregion
            }
            else
            {
                #region por Fechas               

                var buscarVehiculosComprados = (from encabezado in context.encab_documento
                                                join usuario in context.users
                                                    on encabezado.userid_creacion equals usuario.user_id
                                                join tipoDocumento in context.tp_doc_registros
                                                    on encabezado.tipo equals tipoDocumento.tpdoc_id
                                                join vehiculo in context.icb_vehiculo
                                                    on encabezado.documento equals vehiculo.plan_mayor
                                                join modelo in context.modelo_vehiculo
                                                    on vehiculo.modvh_id equals modelo.modvh_codigo
                                                join creador in context.users
                                                    on encabezado.userid_creacion equals creador.user_id
                                                join bodega in context.bodega_concesionario
                                                    on encabezado.bodega equals bodega.id
                                                join tercero in context.icb_terceros
                                                    on encabezado.nit equals tercero.tercero_id
                                                where encabezado.fecha >= desde && encabezado.fecha <= hasta && encabezado.bodega == bodegaActual &&
                                                      tipoDocumento.tipo == 1
                                                select new
                                                {
                                                    encabezado.idencabezado,
                                                    tpdoc_nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                                    encabezado.numero,
                                                    vehiculo.plan_mayor,
                                                    modelo.modvh_nombre,
                                                    creador = creador.user_nombre + " " + creador.user_apellido,
                                                    vehiculo.icbvh_id,
                                                    bodega.bodccs_cod,
                                                    bodega.bodccs_nombre,
                                                    tercero.doc_tercero,
                                                    proveedor = tercero.razon_social + tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                                tercero.apellido_tercero + " " + tercero.segapellido_tercero,
                                                    encabezado.fecha,
                                                    usuario = usuario.user_nombre + " " + usuario.user_apellido,
                                                    factura = encabezado.documento
                                                }).ToList();

                var data = buscarVehiculosComprados.Select(x => new
                {
                    x.idencabezado,
                    x.tpdoc_nombre,
                    x.numero,
                    x.plan_mayor,
                    x.modvh_nombre,
                    x.creador,
                    x.icbvh_id,
                    x.bodccs_cod,
                    x.bodccs_nombre,
                    x.doc_tercero,
                    x.proveedor,
                    x.usuario,
                    x.factura,
                    fecha = x.fecha.ToString("yyyy/MM/dd")
                });
                return Json(data, JsonRequestBehavior.AllowGet);

                #endregion
            }
        }

        public JsonResult DevolverCompraManual(int? id_encabezado, int? id_tp_documento, int? idperfil, decimal? fletes,
            decimal? ivaFletes, string nota)
        {
            if (id_encabezado == null || idperfil == null)
            {
                string mensaje = "No existe un numero de encabezado o perfil en esta compra, asegurese que exista";
                return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
            }

            encab_documento buscarEncabezado = context.encab_documento.FirstOrDefault(x => x.idencabezado == id_encabezado);
            if (buscarEncabezado != null)
            {
                // Validacion para saber si el documento ya se le realizo una devolucion
                int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                string castString = buscarEncabezado.numero.ToString();

                // Se valida que el vehiculo no se encuentre en ningun pedido
                vpedido buscarPedido = context.vpedido.FirstOrDefault(x => x.planmayor == buscarEncabezado.documento);
                if (buscarPedido != null)
                {
                    string mensaje =
                        "Este vehiculo se encuentra asignado en el pedido <span class='label label-default'>" +
                        buscarPedido.numero + "</span>";
                    return Json(new { mensaje, valorGuardado = false, facturaDevuelta = true, buscarPedido.numero },
                        JsonRequestBehavior.AllowGet);
                }

                encab_documento buscarDevolucion = context.encab_documento.Where(x =>
                        x.prefijo == buscarEncabezado.idencabezado && x.bodega == bodegaActual &&
                        x.documento == castString)
                    .FirstOrDefault();
                if (buscarDevolucion != null)
                {
                    string mensaje =
                        "La factura ya fue devuelta, el numero de devolucion es <span class=\"label label-default\">" +
                        buscarDevolucion.numero + "</span>";
                    return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
                }
                // Fin validacion para saber si el documento ya se le realizo una devolucion

                long numeroConsecutivo = 0;
                ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
                icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
                tp_doc_registros buscarTipoDocRegistro = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == id_tp_documento);
                numeroConsecutivoAux = gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro, id_tp_documento ?? 0,
                    buscarEncabezado.bodega);

                //var numeroConsecutivoAux = context.icb_doc_consecutivos.OrderByDescending(x => x.doccons_ano).FirstOrDefault(x => x.doccons_idtpdoc == id_tp_documento && x.doccons_bodega == buscarEncabezado.bodega);
                grupoconsecutivos grupoConsecutivo = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == id_tp_documento && x.bodega_id == buscarEncabezado.bodega);
                //var alertasFechaOConsecutivo = "";
                if (numeroConsecutivoAux != null)
                {
                    numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                }
                else
                {
                    string mensaje = "No existe un numero consecutivo asignado para este tipo de documento";
                    return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
                }

                // Se valida que el valor de credito sea igual al valor de debito para que la ecuacion quede balanceada
                //var parametrosCuentas = context.perfil_cuentas_documento.Where(x => x.id_perfil == modelo.perfilContable).ToList();
                var parametrosCuentasVerificar = (from perfil_cuenta in context.perfil_cuentas_documento
                                                  join nombreParametro in context.paramcontablenombres
                                                      on perfil_cuenta.id_nombre_parametro equals nombreParametro.id
                                                  join cuenta in context.cuenta_puc
                                                      on perfil_cuenta.cuenta equals cuenta.cntpuc_id
                                                  where perfil_cuenta.id_perfil == idperfil
                                                  select new
                                                  {
                                                      perfil_cuenta.id,
                                                      perfil_cuenta.id_nombre_parametro,
                                                      perfil_cuenta.cuenta,
                                                      perfil_cuenta.centro,
                                                      perfil_cuenta.id_perfil,
                                                      nombreParametro.descripcion_parametro,
                                                      cuenta.cntpuc_numero
                                                  }).ToList();

                var buscarTerceroProveedor = (from tercero in context.icb_terceros
                                              join proveedor in context.tercero_proveedor
                                                  on tercero.tercero_id equals proveedor.tercero_id
                                              join tributario in context.perfiltributario
                                                  on proveedor.tpregimen_id equals tributario.tipo_regimenid
                                              join acteco in context.acteco_tercero
                                                  on proveedor.acteco_id equals acteco.acteco_id into ps
                                              from acteco in ps.DefaultIfEmpty()
                                              where tercero.tercero_id == buscarEncabezado.nit
                                              select new
                                              {
                                                  tercero.tercero_id,
                                                  proveedor.tpregimen_id,
                                                  tributario.retfuente,
                                                  tributario.retiva,
                                                  tributario.retica,
                                                  proveedor.acteco_id,
                                                  acteco.tarifa
                                              }).FirstOrDefault();

                decimal? retencionAux = 0;
                decimal? reteIvaAux = 0;
                decimal? reteIcaAux = 0;
                tp_doc_registros buscarPrefijoDocumento =
                    context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == id_tp_documento);
                decimal buscarIvaVehiculo = (from vehiculo in context.icb_vehiculo
                                             where vehiculo.plan_mayor == buscarEncabezado.documento
                                             select vehiculo.iva_vh).FirstOrDefault();
                if (buscarTerceroProveedor != null)
                {
                    if (buscarPrefijoDocumento != null)
                    {
                        decimal baseRetencion = buscarPrefijoDocumento.baseretencion;
                        if (baseRetencion < buscarEncabezado.costo)
                        {
                            if (buscarTerceroProveedor.retfuente == "A")
                            {
                                float porcentajeRetencion = buscarPrefijoDocumento.retencion;
                                retencionAux = Math.Round(buscarEncabezado.costo * (decimal)porcentajeRetencion / 100);
                            }
                        }

                        decimal baseIva = buscarPrefijoDocumento.baseiva;
                        float porcentajeIva = buscarPrefijoDocumento.retiva;
                        decimal calcularIva = Math.Round(buscarEncabezado.costo * buscarIvaVehiculo / 100 *
                                                     (decimal)(porcentajeIva / 100));
                        if (baseIva < calcularIva)
                        {
                            if (buscarTerceroProveedor.retfuente == "A")
                            {
                                reteIvaAux = calcularIva;
                            }
                        }

                        decimal baseIca = buscarPrefijoDocumento.baseica;
                        if (baseIca < buscarEncabezado.costo)
                        {
                            if (buscarTerceroProveedor.retfuente == "A")
                            {
                                var buscarActecoPorBodega = (from actecoBodega in context.terceros_bod_ica
                                                             where actecoBodega.bodega == bodegaActual
                                                             select new
                                                             {
                                                                 actecoBodega.porcentaje
                                                             }).FirstOrDefault();
                                decimal tarifaActeco = 0;
                                if (buscarActecoPorBodega != null)
                                {
                                    tarifaActeco = buscarActecoPorBodega.porcentaje;
                                }
                                else
                                {
                                    if (buscarTerceroProveedor.tarifa != null)
                                    {
                                        tarifaActeco = buscarTerceroProveedor.tarifa;
                                    }
                                    else
                                    {
                                        tarifaActeco = (decimal)buscarPrefijoDocumento.retica;
                                    }
                                }

                                reteIcaAux = Math.Round(buscarEncabezado.costo * tarifaActeco / 1000);
                            }
                        }
                    }
                }

                decimal retenciones = (retencionAux ?? 0) + (reteIvaAux ?? 0) + (reteIcaAux ?? 0);

                List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();
                centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
                icb_terceros terceroValorCero = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
                int idTerceroCero = centroValorCero != null ? Convert.ToInt32(terceroValorCero.tercero_id) : 0;
                decimal calcularDebito = 0;
                decimal calcularCredito = 0;

                foreach (var parametro in parametrosCuentasVerificar)
                {
                    //var tipoParametro = 0;

                    cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
                    decimal valorCredito = 0;
                    decimal valorDebito = 0;
                    string CreditoDebito = "";
                    if (buscarCuenta != null)
                    {
                        if (parametro.id_nombre_parametro == 1)
                        {
                            valorDebito = buscarEncabezado.costo + buscarEncabezado.iva - retenciones;
                            CreditoDebito = "Debito";
                        }

                        if (parametro.id_nombre_parametro == 2)
                        {
                            valorCredito = buscarEncabezado.iva;
                            CreditoDebito = "Credito";
                        }

                        if (parametro.id_nombre_parametro == 3)
                        {
                            valorDebito = retencionAux ?? 0;
                            CreditoDebito = "Debito";
                        }

                        if (parametro.id_nombre_parametro == 4)
                        {
                            valorDebito = reteIvaAux ?? 0;
                            CreditoDebito = "Debito";
                        }

                        if (parametro.id_nombre_parametro == 5)
                        {
                            valorDebito = reteIcaAux ?? 0;
                            CreditoDebito = "Debito";
                        }

                        if (parametro.id_nombre_parametro == 6)
                        {
                            valorCredito = fletes ?? 0;
                            CreditoDebito = "Credito";
                        }

                        if (parametro.id_nombre_parametro == 7)
                        {
                            valorDebito = buscarEncabezado.descuento_pie;
                            CreditoDebito = "Debito";
                        }

                        if (parametro.id_nombre_parametro == 9)
                        {
                            valorCredito = buscarEncabezado.costo;
                            CreditoDebito = "Credito";
                        }

                        if (parametro.id_nombre_parametro == 14)
                        {
                            valorCredito = (ivaFletes ?? 0 * fletes ?? 0) / 100;
                            CreditoDebito = "Credito";
                        }

                        if (CreditoDebito.ToUpper().Contains("DEBITO"))
                        {
                            calcularDebito += valorDebito;
                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                            {
                                NumeroCuenta = parametro.cntpuc_numero,
                                DescripcionParametro = parametro.descripcion_parametro,
                                ValorDebito = valorDebito,
                                ValorCredito = 0
                            });
                        }

                        if (CreditoDebito.ToUpper().Contains("CREDITO"))
                        {
                            calcularCredito += valorCredito;
                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                            {
                                NumeroCuenta = parametro.cntpuc_numero,
                                DescripcionParametro = parametro.descripcion_parametro,
                                ValorDebito = 0,
                                ValorCredito = valorCredito
                            });
                        }
                    }
                }

                if (calcularCredito != calcularDebito)
                {
                    string mensaje = "El documento se encuentra mal validado";
                    return Json(new { mensaje, valorGuardado = false, docDescuadrado = true, listaDescuadrados },
                        JsonRequestBehavior.AllowGet);
                }
                // Fin de la validacion para el calculo del debito y credito del movimiento contable

                referencias_inven buscarReferenciasInven = context.referencias_inven.FirstOrDefault(x =>
                    x.codigo == buscarEncabezado.documento && x.ano == DateTime.Now.Year &&
                    x.mes == DateTime.Now.Month && x.can_ini + x.can_ent - x.can_sal > 0);
                if (buscarReferenciasInven != null)
                {
                    buscarReferenciasInven.can_sal = 1;
                    buscarReferenciasInven.can_dev_com = 1;
                    buscarReferenciasInven.cos_dev_com = buscarReferenciasInven.cos_dev_com + buscarEncabezado.costo;
                    buscarReferenciasInven.cos_sal = buscarReferenciasInven.cos_ent + buscarEncabezado.costo;
                    context.Entry(buscarReferenciasInven).State = EntityState.Modified;

                    // Se busca el vehiculo  de la tabla icb_vehiculo, para actualizarle el campo propietario
                    icb_vehiculo buscarVehiculo =
                        context.icb_vehiculo.FirstOrDefault(x => x.plan_mayor == buscarEncabezado.documento);
                    if (buscarVehiculo != null)
                    {
                        buscarVehiculo.propietario = null;
                        buscarVehiculo.fecha_venta = null;
                        context.Entry(buscarVehiculo).State = EntityState.Modified;
                    }

                    bool guardarReferenciasInven = context.SaveChanges() > 0;
                }
                else
                {
                    string mensaje = "El vehiculo buscado con plan mayor " + buscarEncabezado.documento +
                                  " no tiene stock disponible para devolución";
                    return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
                }

                encab_documento crearEncabezado = new encab_documento
                {
                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    tipo = id_tp_documento ?? 0,
                    pedido = buscarEncabezado.pedido,
                    bodega = buscarEncabezado.bodega,
                    fpago_id = buscarEncabezado.fpago_id,
                    nit = buscarEncabezado.nit,
                    numero = numeroConsecutivo,
                    documento = buscarEncabezado.numero.ToString(),
                    fecha = DateTime.Now,
                    fec_creacion = DateTime.Now,
                    vendedor = buscarEncabezado.vendedor,
                    vencimiento = DateTime.Now,
                    id_pedido_vehiculo = buscarEncabezado.id_pedido_vehiculo,
                    valor_total = buscarEncabezado.valor_total,
                    iva = buscarEncabezado.iva,
                    valor_mercancia = buscarEncabezado.costo,
                    impoconsumo = buscarEncabezado.impoconsumo,
                    retencion = buscarEncabezado.retencion,
                    retencion_ica = buscarEncabezado.retencion_ica,
                    retencion_iva = buscarEncabezado.retencion_iva,
                    costo = buscarEncabezado.costo,
                    perfilcontable = idperfil,
                    estado = true,
                    notas = nota,
                    prefijo = buscarEncabezado.idencabezado,
                    nit_pago = buscarEncabezado.nit_pago,
                    valor_aplicado = buscarEncabezado.valor_total
                };

                context.encab_documento.Add(crearEncabezado);
                bool guardarEncabezado = context.SaveChanges() > 0;
                encab_documento buscarUltimoEncabezado =
                    context.encab_documento.OrderByDescending(x => x.idencabezado).FirstOrDefault();

                List<lineas_documento> buscarLinea = context.lineas_documento.Where(x => x.id_encabezado == buscarEncabezado.idencabezado)
                    .ToList();
                foreach (lineas_documento linea in buscarLinea)
                {
                    lineas_documento crearLineas = new lineas_documento
                    {
                        codigo = buscarEncabezado.documento,
                        fec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        nit = buscarEncabezado.nit,
                        cantidad = 1,
                        bodega = buscarEncabezado.bodega,
                        seq = 1,
                        porcentaje_iva = linea.porcentaje_iva,
                        valor_unitario = buscarEncabezado.costo,
                        impoconsumo = (float)buscarEncabezado.impoconsumo,
                        estado = true,
                        fec = DateTime.Now,
                        costo_unitario = buscarEncabezado.costo,
                        id_encabezado = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0
                    };
                    context.lineas_documento.Add(crearLineas);
                }


                List<perfil_cuentas_documento> perfil = context.perfil_cuentas_documento.Where(x => x.id_perfil == idperfil).ToList();
                foreach (perfil_cuentas_documento parametro in perfil)
                {
                    decimal valorCredito = 0;
                    decimal valorDebito = 0;
                    int secuencia = 1;
                    string CreditoDebito = "";
                    cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);
                    if (buscarCuenta != null)
                    {
                        if (parametro.id_nombre_parametro == 1)
                        {
                            valorDebito = buscarEncabezado.costo + buscarEncabezado.iva - retenciones;
                            CreditoDebito = "Debito";
                        }

                        if (parametro.id_nombre_parametro == 2)
                        {
                            valorCredito = buscarEncabezado.iva;
                            CreditoDebito = "Credito";
                        }

                        if (parametro.id_nombre_parametro == 3)
                        {
                            valorDebito = retencionAux ?? 0;
                            CreditoDebito = "Debito";
                        }

                        if (parametro.id_nombre_parametro == 4)
                        {
                            valorDebito = reteIvaAux ?? 0;
                            CreditoDebito = "Debito";
                        }

                        if (parametro.id_nombre_parametro == 5)
                        {
                            valorDebito = reteIcaAux ?? 0;
                            CreditoDebito = "Debito";
                        }

                        if (parametro.id_nombre_parametro == 6)
                        {
                            valorCredito = fletes ?? 0;
                            CreditoDebito = "Credito";
                        }

                        if (parametro.id_nombre_parametro == 7)
                        {
                            valorDebito = buscarEncabezado.descuento_pie;
                            CreditoDebito = "Credito";
                        }

                        if (parametro.id_nombre_parametro == 9)
                        {
                            valorCredito = buscarEncabezado.costo;
                            CreditoDebito = "Credito";
                        }

                        if (parametro.id_nombre_parametro == 14)
                        {
                            valorCredito = (ivaFletes ?? 0 * buscarEncabezado.costo) / 100;
                            CreditoDebito = "Credito";
                        }


                        mov_contable movNuevo = new mov_contable
                        {
                            id_encab = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            idparametronombre = parametro.id_nombre_parametro,
                            cuenta = parametro.cuenta,
                            centro = parametro.centro,
                            nit = buscarEncabezado.nit,
                            fec = DateTime.Now,
                            seq = secuencia,
                            debito = valorDebito,
                            credito = valorCredito
                        };

                        if (buscarPrefijoDocumento.aplicaniff)
                        {
                            if (buscarCuenta.concepniff == 1)
                            {
                                if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                {
                                    movNuevo.debitoniif = valorDebito;
                                    movNuevo.debito = valorDebito;
                                }

                                if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                {
                                    movNuevo.credito = valorCredito;
                                    movNuevo.creditoniif = valorCredito;
                                }
                            }

                            if (buscarCuenta.concepniff == 4)
                            {
                                if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                {
                                    movNuevo.debitoniif = valorDebito;
                                }

                                if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                {
                                    movNuevo.creditoniif = valorCredito;
                                }
                            }

                            if (buscarCuenta.concepniff == 5)
                            {
                                if (CreditoDebito.ToUpper().Contains("DEBITO"))
                                {
                                    movNuevo.debito = valorDebito;
                                }

                                if (CreditoDebito.ToUpper().Contains("CREDITO"))
                                {
                                    movNuevo.credito = valorCredito;
                                }
                            }
                        }


                        if (buscarCuenta.manejabase)
                        {
                            if (parametro.id_nombre_parametro == 4)
                            {
                                movNuevo.basecontable = buscarEncabezado.iva;
                            }
                            else
                            {
                                movNuevo.basecontable = buscarEncabezado.costo;
                            }
                        }

                        if (buscarCuenta.tercero)
                        {
                            movNuevo.nit = buscarEncabezado.nit;
                        }

                        if (buscarCuenta.documeto)
                        {
                            movNuevo.documento = buscarUltimoEncabezado.numero.ToString();
                        }

                        if (string.IsNullOrEmpty(buscarCuenta.ctareversion))
                        {
                            movNuevo.cuenta = parametro.cuenta;
                        }
                        else
                        {
                            cuenta_puc buscarCuentaReversion =
                                context.cuenta_puc.FirstOrDefault(x => x.cntpuc_numero == buscarCuenta.ctareversion);
                            if (buscarCuentaReversion != null)
                            {
                                movNuevo.cuenta = buscarCuentaReversion.cntpuc_id;
                            }
                        }

                        movNuevo.detalle = "Devolucion Facturacion vehiculos fact " + buscarUltimoEncabezado.documento;
                        secuencia++;

                        DateTime fechaHoy = DateTime.Now;
                        cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                            x.centro == parametro.centro && x.cuenta == parametro.cuenta && x.nit == movNuevo.nit &&
                            x.ano == fechaHoy.Year && x.mes == fechaHoy.Month);

                        if (buscar_cuentas_valores != null)

                        {
                            buscar_cuentas_valores.debito += movNuevo.debito;
                            buscar_cuentas_valores.credito += movNuevo.credito;
                            buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                            buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                            context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                        }
                        else
                        {
                            cuentas_valores crearCuentaValor = new cuentas_valores
                            {
                                ano = fechaHoy.Year,
                                mes = fechaHoy.Month,
                                cuenta = movNuevo.cuenta,
                                centro = movNuevo.centro,
                                nit = movNuevo.nit,
                                debito = movNuevo.debito,
                                credito = movNuevo.credito,
                                debitoniff = movNuevo.debitoniif,
                                creditoniff = movNuevo.creditoniif
                            };
                            context.cuentas_valores.Add(crearCuentaValor);
                        }

                        context.mov_contable.Add(movNuevo);

                        cruce_documentos nuevoCruce = new cruce_documentos
                        {
                            idtipo = buscarUltimoEncabezado.tipo,
                            numero = buscarUltimoEncabezado.numero,
                            fecha = DateTime.Now,
                            fechacruce = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            idtipoaplica = buscarEncabezado.tipo,
                            numeroaplica = buscarEncabezado.numero,
                            valor = buscarEncabezado.valor_total
                        };
                        context.cruce_documentos.Add(nuevoCruce);
                    }
                }

                bool guardarCuenta = context.SaveChanges() > 0;
                if (guardarEncabezado && guardarCuenta)
                {
                    vpedido buscarPedidoParaActualizar =
                        context.vpedido.FirstOrDefault(x => x.numero == buscarEncabezado.id_pedido_vehiculo);
                    if (buscarPedidoParaActualizar != null)
                    {
                        buscarPedidoParaActualizar.facturado = false;
                        buscarPedidoParaActualizar.numfactura = null;
                        context.Entry(buscarPedidoParaActualizar).State = EntityState.Modified;
                    }

                    // Actualiza los numeros consecutivos por documento
                    int grupoId = grupoConsecutivo != null ? grupoConsecutivo.grupo : 0;
                    List<grupoconsecutivos> gruposConsecutivos = context.grupoconsecutivos.Where(x => x.grupo == grupoId).ToList();
                    foreach (grupoconsecutivos grupo in gruposConsecutivos)
                    {
                        icb_doc_consecutivos buscarElemento = context.icb_doc_consecutivos.FirstOrDefault(x =>
                            x.doccons_idtpdoc == id_tp_documento && x.doccons_bodega == grupo.bodega_id);
                        buscarElemento.doccons_siguiente = buscarElemento.doccons_siguiente + 1;
                        context.Entry(buscarElemento).State = EntityState.Modified;
                    }

                    context.SaveChanges();
                    string mensaje = "La devolución se ha guardado exitosamente con numero de documento " +
                                  numeroConsecutivo;
                    return Json(new { mensaje, valorGuardado = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string mensaje = "Error de conexion con base de datos";
                    return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
                }
            }

            {
                string mensaje = "No se encontro el encabezado de este documento.";
                return Json(new { mensaje, valorGuardado = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DevolverCompraAutomatica(int? id_encabezado, int id_tp_documento, string nota)
        {
            encab_documento buscarEncabezado = context.encab_documento.FirstOrDefault(x => x.idencabezado == id_encabezado);
            if (buscarEncabezado != null)
            {
                // Validacion para saber si el documento ya se le realizo una devolucion
                tp_doc_registros buscarPrefijoDocumento =
                    context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == id_tp_documento);
                int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                string castString = buscarEncabezado.numero.ToString();

                // Se valida que el vehiculo no se encuentre en ningun pedido
                vpedido buscarPedido = context.vpedido.FirstOrDefault(x => x.planmayor == buscarEncabezado.documento);
                if (buscarPedido != null)
                {
                    string mensaje =
                        "Este vehiculo se encuentra asignado en el pedido <span class='label label-default'>" +
                        buscarPedido.numero + "</span>";
                    return Json(new { mensaje, valorGuardado = false, facturaDevuelta = true, buscarPedido.numero },
                        JsonRequestBehavior.AllowGet);
                }


                encab_documento buscarDevolucion = context.encab_documento.Where(x =>
                        x.prefijo == buscarEncabezado.idencabezado && x.bodega == bodegaActual &&
                        x.documento == castString)
                    .FirstOrDefault();
                if (buscarDevolucion != null)
                {
                    string mensaje =
                        "La factura ya fue devuelta, el numero de devolucion es <span class='label label-default'>" +
                        buscarDevolucion.numero + "</span>";
                    return Json(new { mensaje, valorGuardado = false, facturaDevuelta = true, buscarDevolucion.numero },
                        JsonRequestBehavior.AllowGet);
                }
                // Fin validacion para saber si el documento ya se le realizo una devolucion


                long numeroConsecutivo = 0;
                int anioActual = DateTime.Now.Year;
                int mesActual = DateTime.Now.Month;
                //var buscarTipoDocRegistro = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == id_tp_documento);
                icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
                if (buscarPrefijoDocumento.consecano)
                {
                    numeroConsecutivoAux = context.icb_doc_consecutivos.FirstOrDefault(x =>
                        x.doccons_idtpdoc == id_tp_documento && x.doccons_bodega == buscarEncabezado.bodega &&
                        x.doccons_ano == anioActual);
                }
                else if (buscarPrefijoDocumento.consecmes)
                {
                    numeroConsecutivoAux = context.icb_doc_consecutivos.FirstOrDefault(x =>
                        x.doccons_idtpdoc == id_tp_documento && x.doccons_bodega == buscarEncabezado.bodega &&
                        x.doccons_ano == anioActual && x.doccons_mes == mesActual);
                }
                else
                {
                    numeroConsecutivoAux = context.icb_doc_consecutivos.FirstOrDefault(x =>
                        x.doccons_idtpdoc == id_tp_documento && x.doccons_bodega == buscarEncabezado.bodega);
                }

                //var numeroConsecutivoAux = context.icb_doc_consecutivos.OrderByDescending(x => x.doccons_ano).FirstOrDefault(x => x.doccons_idtpdoc == id_tp_documento && x.doccons_bodega == buscarEncabezado.bodega);
                grupoconsecutivos grupoConsecutivo = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == id_tp_documento && x.bodega_id == buscarEncabezado.bodega);
                //var alertasFechaOConsecutivo = "";
                if (numeroConsecutivoAux != null)
                {
                    numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                }
                else
                {
                    string mensaje = "No existe un numero consecutivo asignado para este tipo de documento";
                    return Json(new { mensaje, valorGuardado = false, facturaDevuelta = false },
                        JsonRequestBehavior.AllowGet);
                }


                // Aqui se termino de validar el consecutivo
                referencias_inven buscarReferenciasInven = context.referencias_inven.FirstOrDefault(x =>
                    x.codigo == buscarEncabezado.documento && x.ano == DateTime.Now.Year &&
                    x.mes == DateTime.Now.Month);
                if (buscarReferenciasInven != null)
                {
                    buscarReferenciasInven.can_sal = 1;
                    buscarReferenciasInven.can_dev_com = 1;
                    buscarReferenciasInven.cos_dev_com = buscarReferenciasInven.cos_dev_com + buscarEncabezado.costo;
                    buscarReferenciasInven.cos_sal = buscarReferenciasInven.cos_ent + buscarEncabezado.costo;
                    context.Entry(buscarReferenciasInven).State = EntityState.Modified;

                    // Se busca el vehiculo de la tabla icb_vehiculo, para actualizar el campo propietario
                    icb_vehiculo buscarVehiculo =
                        context.icb_vehiculo.FirstOrDefault(x => x.plan_mayor == buscarEncabezado.documento);
                    if (buscarVehiculo != null)
                    {
                        buscarVehiculo.propietario = null;
                        buscarVehiculo.fecha_venta = null;
                        context.Entry(buscarVehiculo).State = EntityState.Modified;
                    }

                    bool guardarReferenciasInven = context.SaveChanges() > 0;
                }
                else
                {
                    string mensaje = "El vehiculo buscado con plan mayor " + buscarEncabezado.documento +
                                  " no tiene stock disponible para devolución";
                    return Json(new { mensaje, valorGuardado = false, facturaDevuelta = false },
                        JsonRequestBehavior.AllowGet);
                }


                encab_documento crearEncabezado = new encab_documento
                {
                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    tipo = id_tp_documento,
                    pedido = buscarEncabezado.pedido,
                    bodega = buscarEncabezado.bodega,
                    fpago_id = buscarEncabezado.fpago_id,
                    nit = buscarEncabezado.nit,
                    numero = numeroConsecutivo,
                    documento = buscarEncabezado.numero.ToString(),
                    fecha = DateTime.Now,
                    fec_creacion = DateTime.Now,
                    vendedor = buscarEncabezado.vendedor,
                    vencimiento = DateTime.Now,
                    id_pedido_vehiculo = buscarEncabezado.id_pedido_vehiculo,
                    valor_total = buscarEncabezado.valor_total,
                    iva = buscarEncabezado.iva,
                    valor_mercancia = buscarEncabezado.valor_mercancia,
                    impoconsumo = buscarEncabezado.impoconsumo,
                    retencion = buscarEncabezado.retencion,
                    retencion_ica = buscarEncabezado.retencion_ica,
                    retencion_iva = buscarEncabezado.retencion_iva,
                    costo = buscarEncabezado.costo,
                    estado = true,
                    notas = nota,
                    perfilcontable = buscarEncabezado.perfilcontable,
                    prefijo = buscarEncabezado.idencabezado,
                    nit_pago = buscarEncabezado.nit_pago,
                    valor_aplicado = buscarEncabezado.valor_total
                };

                // Se actualiza el campo valor_aplicado en el encabezado viejo (es decir el encabezado de la compra)
                buscarEncabezado.valor_aplicado = buscarEncabezado.valor_total;
                context.Entry(buscarEncabezado).State = EntityState.Modified;

                context.encab_documento.Add(crearEncabezado);
                bool guardarEncabezado = context.SaveChanges() > 0;
                encab_documento buscarUltimoEncabezado =
                    context.encab_documento.OrderByDescending(x => x.idencabezado).FirstOrDefault();

                List<lineas_documento> buscarLinea = context.lineas_documento.Where(x => x.id_encabezado == buscarEncabezado.idencabezado)
                    .ToList();
                foreach (lineas_documento linea in buscarLinea)
                {
                    lineas_documento crearLineas = new lineas_documento
                    {
                        codigo = buscarEncabezado.documento,
                        fec_creacion = DateTime.Now,
                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                        nit = buscarEncabezado.nit,
                        cantidad = 1,
                        bodega = buscarEncabezado.bodega,
                        seq = 1,
                        porcentaje_iva = linea.porcentaje_iva,
                        valor_unitario = buscarEncabezado.valor_mercancia,
                        impoconsumo = (float)buscarEncabezado.impoconsumo,
                        estado = true,
                        fec = DateTime.Now,
                        costo_unitario = buscarEncabezado.costo,
                        id_encabezado = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.idencabezado : 0
                    };
                    context.lineas_documento.Add(crearLineas);
                }


                int secuencia = 1;
                List<mov_contable> buscarMovimientos = context.mov_contable.Where(x => x.id_encab == buscarEncabezado.idencabezado)
                    .ToList();
                centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
                icb_terceros terceroValorCero = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == "0");
                int idTerceroCero = centroValorCero != null ? Convert.ToInt32(terceroValorCero.tercero_id) : 0;

                foreach (mov_contable movimiento in buscarMovimientos)
                {
                    cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == movimiento.cuenta);
                    if (buscarCuenta != null)
                    {
                        mov_contable movNuevo = new mov_contable
                        {
                            id_encab = buscarUltimoEncabezado.idencabezado,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            idparametronombre = movimiento.idparametronombre,
                            centro = movimiento.centro,
                            nit = movimiento.nit,
                            fec = DateTime.Now,
                            seq = secuencia,
                            debito = movimiento.credito,
                            credito = movimiento.debito,
                            documento = buscarEncabezado.numero.ToString(),
                            basecontable = movimiento.basecontable
                        };

                        if (buscarPrefijoDocumento.aplicaniff)
                        {
                            movNuevo.debitoniif = movimiento.debitoniif;
                            movNuevo.creditoniif = movimiento.creditoniif;
                        }

                        if (string.IsNullOrEmpty(buscarCuenta.ctareversion))
                        {
                            movNuevo.cuenta = movimiento.cuenta;
                        }
                        else
                        {
                            cuenta_puc buscarCuentaReversion =
                                context.cuenta_puc.FirstOrDefault(x => x.cntpuc_numero == buscarCuenta.ctareversion);
                            if (buscarCuentaReversion != null)
                            {
                                movNuevo.cuenta = buscarCuentaReversion.cntpuc_id;
                            }
                        }

                        movNuevo.debitoniif = movimiento.creditoniif;
                        movNuevo.creditoniif = movimiento.debitoniif;

                        DateTime fechaHoy = DateTime.Now;
                        cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                            x.centro == movimiento.centro && x.cuenta == movimiento.cuenta && x.nit == movNuevo.nit &&
                            x.ano == fechaHoy.Year && x.mes == fechaHoy.Month);

                        if (buscar_cuentas_valores != null)
                        {
                            buscar_cuentas_valores.debito += movNuevo.debito;
                            buscar_cuentas_valores.credito += movNuevo.credito;
                            buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                            buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                            context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                        }
                        else
                        {
                            cuentas_valores crearCuentaValor = new cuentas_valores
                            {
                                ano = fechaHoy.Year,
                                mes = fechaHoy.Month,
                                cuenta = movNuevo.cuenta,
                                centro = movNuevo.centro,
                                nit = movNuevo.nit,
                                debito = movNuevo.debito,
                                credito = movNuevo.credito,
                                debitoniff = movNuevo.debitoniif,
                                creditoniff = movNuevo.creditoniif
                            };
                            context.cuentas_valores.Add(crearCuentaValor);
                        }

                        context.mov_contable.Add(movNuevo);
                        secuencia++;
                    }
                }

                cruce_documentos nuevoCruce = new cruce_documentos
                {
                    idtipo = buscarUltimoEncabezado.tipo,
                    numero = buscarUltimoEncabezado.numero,
                    fecha = DateTime.Now,
                    fechacruce = DateTime.Now,
                    userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    idtipoaplica = buscarEncabezado.tipo,
                    numeroaplica = buscarEncabezado.numero,
                    valor = buscarEncabezado.valor_total
                };
                context.cruce_documentos.Add(nuevoCruce);

                int guardarCuenta = context.SaveChanges();
                if (guardarEncabezado && guardarCuenta > 0)
                {
                    vpedido buscarPedidoParaActualizar =
                        context.vpedido.FirstOrDefault(x => x.numero == buscarEncabezado.id_pedido_vehiculo);
                    if (buscarPedidoParaActualizar != null)
                    {
                        buscarPedidoParaActualizar.facturado = false;
                        buscarPedidoParaActualizar.numfactura = null;
                        context.Entry(buscarPedidoParaActualizar).State = EntityState.Modified;
                    }

                    // Actualiza los numeros consecutivos por documento
                    int grupoId = grupoConsecutivo != null ? grupoConsecutivo.grupo : 0;
                    List<grupoconsecutivos> gruposConsecutivos = context.grupoconsecutivos.Where(x => x.grupo == grupoId).ToList();
                    foreach (grupoconsecutivos grupo in gruposConsecutivos)
                    {
                        icb_doc_consecutivos buscarElemento = context.icb_doc_consecutivos.FirstOrDefault(x =>
                            x.doccons_idtpdoc == id_tp_documento && x.doccons_bodega == grupo.bodega_id);
                        buscarElemento.doccons_siguiente = buscarElemento.doccons_siguiente + 1;
                        context.Entry(buscarElemento).State = EntityState.Modified;
                    }

                    context.SaveChanges();
                    string mensaje =
                        "La devolución se ha guardado exitosamente con numero de documento <span class='label label-default'>" +
                        numeroConsecutivo + "</span>";
                    return Json(new { mensaje, valorGuardado = true, facturaDevuelta = false },
                        JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string mensaje = "Error de conexion con base de datos";
                    return Json(new { mensaje, valorGuardado = false, facturaDevuelta = false },
                        JsonRequestBehavior.AllowGet);
                }
            }

            {
                string mensaje = "No se encontro el encabezado de este documento.";
                return Json(new { mensaje, valorGuardado = false, facturaDevuelta = false },
                    JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult BuscarPerfilesContables(int? idTpDoc)
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            var buscarPerfiles = (from perfil_contable in context.perfil_contable_documento
                                  join perfil_bodega in context.perfil_contable_bodega
                                      on perfil_contable.id equals perfil_bodega.idperfil
                                  where perfil_contable.tipo == idTpDoc && perfil_bodega.idbodega == bodegaActual
                                  select new
                                  {
                                      perfil_contable.id,
                                      perfil_contable.descripcion
                                  }).ToList();

            return Json(buscarPerfiles, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDetalleDevolucion(int id_devolucion)
        {
            var buscarDevolucion = (from encabezado in context.encab_documento
                                    join lineas in context.lineas_documento
                                        on encabezado.idencabezado equals lineas.id_encabezado into tmp1
                                    from lineas in tmp1.DefaultIfEmpty()
                                    join compra in context.encab_documento
                                        on encabezado.documento equals SqlFunctions.StringConvert((double)compra.idencabezado).Trim() into
                                        tmp2
                                    from compra in tmp2.DefaultIfEmpty()
                                    join vehiculo in context.icb_vehiculo
                                        on lineas.codigo equals vehiculo.plan_mayor into tmp3
                                    from vehiculo in tmp3.DefaultIfEmpty()
                                    join modelo in context.modelo_vehiculo
                                        on vehiculo.modvh_id equals modelo.modvh_codigo into tmp4
                                    from modelo in tmp4.DefaultIfEmpty()
                                    join color in context.color_vehiculo
                                        on vehiculo.colvh_id equals color.colvh_id into tmp5
                                    from color in tmp5.DefaultIfEmpty()
                                    where encabezado.idencabezado == id_devolucion
                                    select new
                                    {
                                        modelo.modvh_nombre,
                                        vehiculo.anio_vh,
                                        color.colvh_nombre,
                                        vehiculo.vin,
                                        vehiculo.nummot_vh,
                                        vehiculo.plan_mayor,
                                        encabezado.valor_total,
                                        lineas.porcentaje_iva,
                                        lineas.valor_unitario,
                                        impoconsumo = lineas.impoconsumo != null ? lineas.impoconsumo : 0,
                                        valor_iva = encabezado.iva
                                    }).FirstOrDefault();
            return Json(buscarDevolucion, JsonRequestBehavior.AllowGet);
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