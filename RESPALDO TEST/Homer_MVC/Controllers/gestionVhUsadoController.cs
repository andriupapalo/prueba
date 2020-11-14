using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class gestionVhUsadoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: gestionVhUsado
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetCantidadVehiculos()
        {
            int rolLogin = Convert.ToInt32(Session["user_rolid"]);
            //Validamos que el usuario loguado tenga el rol y el permiso para ver toda la informacion
            int permiso = (from u in context.users
                           join r in context.rols
                               on u.rol_id equals r.rol_id
                           join ra in context.rolacceso
                               on r.rol_id equals ra.idrol
                           where u.rol_id == rolLogin && ra.idpermiso == 3
                           select new
                           {
                               u.user_id,
                               u.rol_id,
                               r.rol_nombre,
                               ra.idpermiso
                           }).Count();
            if (permiso > 0)
            {
                string sDate = DateTime.Now.ToString();
                DateTime datevalue = Convert.ToDateTime(sDate);
                int mn = datevalue.Month;
                int yy = datevalue.Year;
                var cantModelos = (from vehiculos in context.vw_referencias_total
                                   where vehiculos.usado == true && vehiculos.stock > 0 && vehiculos.ano == yy && vehiculos.mes == mn
                                   group vehiculos by new { vehiculos.modvh_id, vehiculos.modvh_nombre }
                    into modeloGrupo
                                   select new
                                   {
                                       modvh_codigo = modeloGrupo.Key.modvh_id,
                                       modeloGrupo.Key.modvh_nombre,
                                       modvh_cantidad = modeloGrupo.Count()
                                   }).ToList();
                //
                int totalModelos = (from vehiculos in context.vw_referencias_total
                                    where vehiculos.usado == true && vehiculos.stock > 0 && vehiculos.ano == yy && vehiculos.mes == mn
                                    select new
                                    {
                                        vehiculos.modvh_id,
                                        vehiculos.modvh_nombre
                                    }).Count();

                var datos = new
                {
                    cantModelos,
                    totalModelos
                };

                return Json(datos, JsonRequestBehavior.AllowGet);


                //var data = context.vw_referencias_total.ToList();
                //return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                string sDate = DateTime.Now.ToString();
                DateTime datevalue = Convert.ToDateTime(sDate);
                int mn = datevalue.Month;
                int yy = datevalue.Year;
                var cantModelos = (from vehiculos in context.vw_referencias_total
                                   where vehiculos.usado == true && vehiculos.stock > 0 && vehiculos.ano == yy &&
                                         vehiculos.mes == mn && vehiculos.bodega == bodegaActual
                                   group vehiculos by new { vehiculos.modvh_id, vehiculos.modvh_nombre }
                    into modeloGrupo
                                   select new
                                   {
                                       modvh_codigo = modeloGrupo.Key.modvh_id,
                                       modeloGrupo.Key.modvh_nombre,
                                       modvh_cantidad = modeloGrupo.Count()
                                   }).ToList();

                int totalModelos = (from vehiculos in context.vw_referencias_total
                                    where vehiculos.usado == true && vehiculos.stock > 0 && vehiculos.ano == yy &&
                                          vehiculos.mes == mn && vehiculos.bodega == bodegaActual
                                    select new
                                    {
                                        vehiculos.modvh_id,
                                        vehiculos.modvh_nombre
                                    }).Count();

                var datos = new
                {
                    cantModelos,
                    totalModelos
                };
                return Json(datos, JsonRequestBehavior.AllowGet);

                //return Json(permiso, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult JsonBrowserAsesorUsados(string idModelo)
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            string sDate = DateTime.Now.ToString();
            DateTime datevalue = Convert.ToDateTime(sDate);
            int mn = datevalue.Month;
            int yy = datevalue.Year;
            var data = from v in context.vw_referencias_total
                       where v.modvh_nombre == idModelo && v.ano == yy && v.mes == mn /*&& v.bodega == bodegaActual*/ &&
                             v.usado == true && v.stock > 0
                       select new
                       {
                           planmayor = v.codigo,
                           serie = v.vin,
                           descripcion = v.modvh_nombre,
                           anomodelo = v.anio_vh,
                           color = v.colvh_nombre,
                           fec_compra = v.fecfact_fabrica.ToString(),
                           dias_inventario = SqlFunctions.DateDiff("day", v.fecfact_fabrica, DateTime.Now),
                           asignado = v.asignado != null ? v.asignado.ToString() : "",
                           v.stock,
                           bodega = v.bodccs_nombre,
                           ubicacion = v.descripcion,
                           v.Estado,
                           cantidadAverias = context.icb_vehiculo_eventos
                               .Where(x => x.planmayor == v.codigo && x.id_tpevento == 15).Count()
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}