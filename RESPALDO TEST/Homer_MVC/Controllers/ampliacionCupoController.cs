using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class ampliacionCupoController : Controller
    {
        private readonly Iceberg_Context db = new Iceberg_Context();

        private readonly CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

        // GET: ampliacionCupo
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult CargarTabla()
        {
            System.Collections.Generic.List<solicitud_ampliacion_cupo> solicitudes2 = db.solicitud_ampliacion_cupo.Where(x => x.atendido_check == false).ToList();
            var solicitudes = solicitudes2.Select(x => new
            {
                idSolicitud = x.id,
                solicitante = ( from a in db.icb_terceros where a.tercero_id == x.tercero_id select new {Nombre = a.prinom_tercero+" "+a.segnom_tercero+" "+a.apellido_tercero+" "+a.segapellido_tercero}).FirstOrDefault().Nombre,
                montoSolicitado = x.monto_aplicado != null
                    ? x.monto_aplicado.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "0",
                motivoSolicitud = x.razon != null ? x.razon : "",
                fechaSolicitud = x.fecha_solicitud.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")),
                empleadoSolicitud = ( from b in db.users where b.user_id == x.user_solicitud select new { nombre = b.user_nombre+" "+b.user_apellido}).FirstOrDefault().nombre
            }).ToList();
            return Json(solicitudes, JsonRequestBehavior.AllowGet);
        }

        public JsonResult crearSolicitud(int tercero_id, decimal monto_aplicado, string razon)
        {
            var idsolicitud = 0;
            System.Collections.Generic.List<solicitud_ampliacion_cupo> estado = db.solicitud_ampliacion_cupo
                .Where(x => x.tercero_id == tercero_id && x.atendido_check == false).ToList();
            if (estado.Count == 0)
            {
                try
                {
                    string motivoSolicitud = razon != null ? razon : "";
                    solicitud_ampliacion_cupo solicitud = new solicitud_ampliacion_cupo
                    {
                        tercero_id = tercero_id,
                        monto_aplicado = -1 * monto_aplicado,
                        razon = motivoSolicitud,
                        user_solicitud = Convert.ToInt32(Session["user_usuarioid"]),
                        fecha_solicitud = DateTime.Now
                    };
                    db.solicitud_ampliacion_cupo.Add(solicitud);
                    int guardar = db.SaveChanges();




                     idsolicitud = db.solicitud_ampliacion_cupo.OrderByDescending(x => x.id).FirstOrDefault().id;
                    return Json(new { proceso = true,  solicitud = idsolicitud }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception es)
                {
                    Exception mensaje = es.InnerException;
                    throw;
                }
            }

            return Json(new { proceso = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult confirmarSolicitud(int? idSolicitud)
        {
            solicitud_ampliacion_cupo query = (from a in db.solicitud_ampliacion_cupo
                                               where a.id == idSolicitud
                                               select a).FirstOrDefault();
            query.estado_check = true;
            query.atendido_check = true;
            query.fecha_respuesta = DateTime.Now;
            db.Entry(query).State = EntityState.Modified;
            int respuesta = db.SaveChanges();
            tercero_cliente query2 = (from b in db.tercero_cliente where b.tercero_id == query.tercero_id select b)
                .FirstOrDefault();
            query2.cupocredito = Convert.ToInt32(query2.cupocredito) + query.monto_aplicado;
            query2.terclifec_actualizacion = DateTime.Now;
            query2.tercliuserid_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
            db.Entry(query2).State = EntityState.Modified;
            int respuesta2 = db.SaveChanges();
            return Json(respuesta, JsonRequestBehavior.AllowGet);
        }

        public JsonResult denegarSolicitud(int? idSolicitud, string Observacion)
        {
            solicitud_ampliacion_cupo query = (from a in db.solicitud_ampliacion_cupo
                                               where a.id == idSolicitud
                                               select a).FirstOrDefault();
            query.estado_check = false;
            query.atendido_check = true;
            query.Observacion = Observacion;

            query.fecha_respuesta = DateTime.Now;
            db.Entry(query).State = EntityState.Modified;
            int respuesta = db.SaveChanges();

            return Json(respuesta, JsonRequestBehavior.AllowGet);
        }


        public JsonResult VerificarestadoSolicitud(int id) {

            var datos = (from a in db.solicitud_ampliacion_cupo
                        where a.id == id
                        select new { a.Observacion, a.estado_check, a.atendido_check }).ToList();

            var data = datos.Select(x => new { x.atendido_check, x.estado_check, x.Observacion }).ToList();



            return Json(data, JsonRequestBehavior.AllowGet);
        }


    }
}