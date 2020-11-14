using Homer_MVC.IcebergModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class anfitrionController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult CambiarEstadoCita(int id, int id_estado, int? id_motivo_cancela)
        {
            tcitastaller buscarCita = context.tcitastaller.FirstOrDefault(x => x.id == id);
            if (buscarCita != null)
            {
                buscarCita.estadocita = id_estado;
                buscarCita.motivoestado = id_motivo_cancela;
                if (id_estado == 15)
                {
                    buscarCita.fechallegada = DateTime.Now;
                }

                context.Entry(buscarCita).State = EntityState.Modified;
                int guardar = context.SaveChanges();
                if (guardar > 0)
                {
                    return Json(new { mensaje = "Estado modificado correctamente" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { mensaje = "Cita no modificada" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { mensaje = "Cita no encontrada" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarAgendasHoy()
        {
            DateTime fechaHoy = DateTime.Now;
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            var buscarAgendasHoySQL = (from citas in context.tcitastaller
                                       join estado in context.tcitasestados
                                           on citas.estadocita equals estado.id
                                       join tercero in context.icb_terceros
                                           on citas.cliente equals tercero.tercero_id
                                       join bahias in context.tbahias
                                           on citas.bahia equals bahias.id
                                       join tipb in context.ttipobahia
                                           on bahias.tipo_bahia equals tipb.id
                                       join tecs in context.ttecnicos
                                           on bahias.idtecnico equals tecs.id
                                       join tiptec in context.ttipotecnico
                                           on tecs.tipo_tecnico equals tiptec.id
                                       join u in context.users
                                           on tecs.idusuario equals u.user_id
                                       join bodega in context.bodega_concesionario
                                           on bahias.bodega equals bodega.id
                                       join vehiculo in context.icb_vehiculo
                                           on citas.placa equals vehiculo.plac_vh into ps
                                            from vehiculo in ps.DefaultIfEmpty()
                                       join clr in context.color_vehiculo 
                                           on vehiculo.colvh_id equals clr.colvh_id into cls
                                            from clr in cls.DefaultIfEmpty()
                                       join modelo in context.modelo_vehiculo
                                           on vehiculo.modvh_id equals modelo.modvh_codigo into ps2
                                       from modelo in ps2.DefaultIfEmpty()
                                       where bodega.id == bodegaActual && u.rol_id == 1014 && citas.desde.Year == fechaHoy.Year &&
                                             citas.desde.Month == fechaHoy.Month && citas.desde.Day == fechaHoy.Day
                                       select new
                                       {
                                           tercero.doc_tercero,
                                           cliente = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " + tercero.apellido_tercero +
                                                     " " + tercero.segapellido_tercero,
                                           estado.color_estado,
                                           estado_id = estado.id,
                                           citas.placa,
                                           modelo = modelo.modvh_nombre != null ? modelo.modvh_nombre : "",
                                           hora = citas.desde,
                                           citas.id,
                                           area = tipb.descripcion,
                                           especialidad = tiptec.Especializacion,
                                           tecnico = u.user_nombre + " " + u.user_apellido,
                                           color = clr.colvh_nombre != null ? clr.colvh_nombre : "",
                                       }).OrderBy(x => x.hora).ToList();
            var buscarAgendasHoy = buscarAgendasHoySQL.Select(x => new
            {
                x.id,
                x.area,
                x.especialidad,
                x.tecnico,
                x.color,
                color_estado =
                    (x.estado_id == 1 || x.estado_id == 2 || x.estado_id == 3) && x.hora < DateTime.Now.AddMinutes(-1)
                        ? "#ddbadb"
                        : x.color_estado,
                x.doc_tercero,
                x.modelo,
                x.cliente,
                x.placa,
                hora = x.hora.ToShortTimeString()
            });
            return Json(buscarAgendasHoy, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarNotificacionAnfitrion()
        {
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            DateTime fechaHoy = DateTime.Now;
            string buscarTiempoEspera =
                (from parametro in context.icb_sysparameter
                 where parametro.syspar_cod == "P53"
                 select parametro.syspar_value).FirstOrDefault();
            int segundos = Convert.ToInt32(buscarTiempoEspera);
            var buscarNotificacion = (from alerta in context.alertaasesor
                                      join tercero in context.icb_terceros
                                          on alerta.propecto equals tercero.tercero_id
                                      where alerta.anfitrion == usuario &&
                                      alerta.aprobado == false &&
                                      alerta.recibido == true &&
                                      alerta.cancelado == false &&
                                      alerta.finalizado == false &&
                                      alerta.rechazado == true &&
                                      alerta.aprobado != null
                                            /*&& alerta.finalizado == false
                                            && (alerta.recibido == true && alerta.cancelado == false && alerta.aprobado == false &&
                                                alerta.reasignado == false
                                                || alerta.recibido == true && alerta.cancelado == false && alerta.aprobado == false &&
                                                alerta.reasignado)*/
                                            && alerta.fecha.Year == fechaHoy.Year && alerta.fecha.Month == fechaHoy.Month &&
                                            alerta.fecha.Day == fechaHoy.Day
                                            && DbFunctions.DiffSeconds(alerta.fecha, fechaHoy) >= segundos
                                      orderby alerta.fecha descending
                                      select new
                                      {
                                          alerta.reasignado,
                                          alerta.cancelado,
                                          alerta.aprobado,
                                          alerta.id,
                                          alerta.propecto,
                                          tercero.tercero_id,
                                          nombreTercero = tercero.razon_social + tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                          tercero.apellido_tercero + " " + tercero.segapellido_tercero,
                                          diferencia = DbFunctions.DiffSeconds(alerta.fecha, fechaHoy)
                                      }).FirstOrDefault();
            if (buscarNotificacion != null)
            {
                if (buscarNotificacion.aprobado == false)
                {
                    alertaasesor buscaAlerta = context.alertaasesor.FirstOrDefault(x => x.id == buscarNotificacion.id);
                    buscaAlerta.recibido = true;
                    buscaAlerta.aprobado = false;
                    buscaAlerta.finalizado = true;
                    buscaAlerta.rechazado = true;
                    buscaAlerta.reasignado = true;
                    context.Entry(buscaAlerta).State = EntityState.Modified;
                    int guardar = context.SaveChanges();
                    var mensaje = new
                    {
                        notificacion = true,
                        buscarNotificacion.tercero_id,
                        alerta_id = buscarNotificacion.id,
                        alerta = "El prospecto " + buscarNotificacion.nombreTercero + " no tiene asesor asignado"
                    };
                    return Json(mensaje, JsonRequestBehavior.AllowGet);
                }

                return Json(new { notificacion = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { notificacion = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ReasignarAsesor(int id)
        {
            alertaasesor buscarTercero = context.alertaasesor.OrderByDescending(x => x.id).FirstOrDefault(x => x.id == id);
            if (buscarTercero != null)
            {
                buscarTercero.recibido = true;
                buscarTercero.reasignado = true;
                buscarTercero.rechazado = true;
                context.Entry(buscarTercero).State = EntityState.Modified;
                int guardar = context.SaveChanges();
                if (guardar > 0)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarTiposTramiteAsesor()
        {
            var buscarTipoTramite = (from tipoTramite in context.icb_tptramite_prospecto
                                     select new
                                     {
                                         tipoTramite.tptrapros_id,
                                         tipoTramite.tptrapros_descripcion
                                     }).ToList();
            return Json(buscarTipoTramite, JsonRequestBehavior.AllowGet);
        }

        public JsonResult tramiteAsesorProspecto(int id)
        {
            var data = (from p in context.prospectos
                        join t in context.icb_tptramite_prospecto
                            on p.tptramite equals t.tptrapros_id
                        where p.idtercero == id
                        orderby p.fec_creacion descending
                        select new
                        {
                            p.tptramite,
                            t.tptrapros_descripcion
                        }).FirstOrDefault();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult cancelarProspectoAnfitrion(int? idAlerta)
        {
            //var buscarTercero = context.alertaasesor.OrderByDescending(x=>x.id).FirstOrDefault(x=>x.propecto==id);
            alertaasesor buscarTercero = context.alertaasesor.FirstOrDefault(x => x.id == idAlerta);
            if (buscarTercero != null)
            {
                int usuario = Convert.ToInt32(Session["user_usuarioid"]);
                int rol = Convert.ToInt32(Session["user_rolid"]);

                if (buscarTercero.recibido == true)
                {
                    // Se cambia la alerta para que al rol anfitriona le aparezca un alerta para que asigne otro asesor al prospecto
                    buscarTercero.recibido = true;
                    if (rol == 7)
                    {
                        buscarTercero.cancelado = true;
                    }

                    context.Entry(buscarTercero).State = EntityState.Modified;

                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        return Json(JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(JsonRequestBehavior.AllowGet);
                }
            }

            return Json(JsonRequestBehavior.AllowGet);
        }
    }
}