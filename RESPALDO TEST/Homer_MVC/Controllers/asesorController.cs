using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class asesorController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();


        public ActionResult Listar()
        {
            return View();
        }

        public JsonResult BuscarAsesoresPaginados()
        {
            #region PrimeraConsulta

            var buscaAsesores = (from usuarios in context.users
                                 where usuarios.rol_id == 4 || usuarios.rol_id == 3020
                                 select new
                                 {
                                     ultimaConexion = context.sesion_logasesor.OrderByDescending(x => x.fecha_termina)
                                         .FirstOrDefault(x => x.user_id == usuarios.user_id),
                                     usuarios.sesion,
                                     usuarios.user_id,
                                     usuarios.user_nombre,
                                     usuarios.user_apellido,
                                     buscarProspecto = (from a in context.sesion_logasesor
                                                        join b in context.prospectos
                                                            on a.idprospecto equals b.id into xx
                                                        from b in xx.DefaultIfEmpty()
                                                        join c in context.users
                                                            on a.user_id equals c.user_id
                                                        group a by new { a.user_id }
                                         into asesor
                                                        select new
                                                        {
                                                            asesor.Key.user_id,
                                                            fecha = asesor.Max(x => x.fecha_inicia),
                                                            prospecto = asesor.Where(x => x.estado == 2).Select(x => x.idprospecto).FirstOrDefault()
                                                        }).FirstOrDefault()
                                 }).OrderBy(x => x.ultimaConexion.fecha_inicia).ToList();


            var buscarFechaAsesor = (from a in context.sesion_logasesor
                                     group a by new { a.user_id }
                into asesor
                                     select new
                                     {
                                         asesor.Key.user_id,
                                         fecha = asesor.Max(x => x.fecha_inicia)
                                     }).ToList();

            //var buscarProspecto = (from a in context.sesion_logasesor
            //                       join b in context.prospectos
            //                       on a.idprospecto equals b.id into xx
            //                       from b in xx.DefaultIfEmpty()
            //                       join c in context.users
            //                       on a.user_id equals c.user_id
            //                       group a by new { a.user_id } into asesor

            //                       select new
            //                       {
            //                           asesor.Key.user_id,
            //                           fecha = asesor.Max(x => x.fecha_inicia),
            //                           prospecto = asesor.Where(x => x.estado == 2).Select(x => x.idprospecto).FirstOrDefault()

            //                       }).ToList();

            List<ListarAsesoresModel> listaAsesores = new List<ListarAsesoresModel>();
            int conectados = 0;
            int ocupados = 0;
            int ausentes = 0;
            int desconectados = 0;
            int esperando = 0;
            List<int> estados = context.estados_sesiones.OrderBy(x => x.id).Select(x => x.id).ToList();


            foreach (int estado in estados)
            {
                foreach (var asesor in buscaAsesores)
                {
                    int estadoUltimo = asesor.ultimaConexion != null ? asesor.ultimaConexion.estado : 0;
                    if (estado == estadoUltimo)
                    {
                        // Si el usuario olvida cerrar la sesion, se valida que la conexion sea del dia actual para prevenir que salga conectado por dejar sesion activa dias anteriores
                        // Si la deja activa dias anteriores se pone en estado 4 que es desconectado
                        if (asesor.ultimaConexion != null)
                        {
                            foreach (var item in buscarFechaAsesor)
                            {
                                var buscar = (from a in context.prospectos
                                              where a.id == asesor.buscarProspecto.prospecto
                                              select new
                                              {
                                                  a.prinom_tercero,
                                                  a.segnom_tercero,
                                                  a.apellido_tercero,
                                                  a.segapellido_tercero
                                              }).FirstOrDefault();

                                //if (buscar != null)
                                //{
                                //    var nombre = buscar.prinom_tercero + ' ' + buscar.segnom_tercero + ' ' + buscar.apellido_tercero + ' ' + buscar.segapellido_tercero;
                                //}


                                if (item.user_id == asesor.user_id)
                                {
                                    DateTime fecha = Convert.ToDateTime(item.fecha);
                                    DateTime Ahora = DateTime.Now;

                                    TimeSpan tiempo_transcurrido = Ahora - fecha;
                                    string duracion = "";
                                    int dias = tiempo_transcurrido.Days;
                                    if (dias > 0)
                                    {
                                        DateTime fecha2 = DateTime.Now.Date;
                                        DateTime fechaHoy = new DateTime(fecha2.Year, fecha2.Month, fecha2.Day, 07, 00, 00);
                                        TimeSpan tiempo_transcurrido2 = Ahora - fecha2;
                                        duracion = tiempo_transcurrido.Hours + ":" + tiempo_transcurrido.Minutes + ":" +
                                                   tiempo_transcurrido.Seconds;
                                    }
                                    else
                                    {
                                        duracion = tiempo_transcurrido.Hours + ":" + tiempo_transcurrido.Minutes + ":" +
                                                   tiempo_transcurrido.Seconds;
                                    }

                                    if (asesor.ultimaConexion.fecha_inicia.Year !=
                                        DateTime.Now.Year || asesor.ultimaConexion.fecha_inicia.Month != DateTime.Now.Month
                                                          || asesor.ultimaConexion.fecha_inicia.Day != DateTime.Now.Day)
                                    {
                                        desconectados++;

                                        listaAsesores.Add(new ListarAsesoresModel
                                        {
                                            nombres = asesor.user_nombre + " " + asesor.user_apellido,
                                            UltimaConexion = asesor.ultimaConexion != null
                                                ? asesor.ultimaConexion.fecha_termina.Value.ToShortDateString() + " " +
                                                  asesor.ultimaConexion.fecha_termina.Value.ToShortTimeString()
                                                : "",
                                            InicioConexion = duracion,
                                            estado = context.estados_sesiones.FirstOrDefault(x => x.id == 4).descripcion,
                                            estado_id = 4,
                                            detalle = asesor.ultimaConexion != null
                                                ? asesor.ultimaConexion.Observacon ?? ""
                                                : ""
                                        });
                                    }
                                    else if (asesor.ultimaConexion.estado != 2)
                                    {
                                        if (asesor.ultimaConexion.estado == 1)
                                        {
                                            conectados++;
                                        }

                                        if (asesor.ultimaConexion.estado == 2)
                                        {
                                            ocupados++;
                                        }

                                        if (asesor.ultimaConexion.estado == 3)
                                        {
                                            ausentes++;
                                        }

                                        if (asesor.ultimaConexion.estado == 4)
                                        {
                                            desconectados++;
                                        }

                                        if (asesor.ultimaConexion.estado == 5)
                                        {
                                            esperando++;
                                        }

                                        listaAsesores.Add(new ListarAsesoresModel
                                        {
                                            nombres = asesor.user_nombre + " " + asesor.user_apellido,
                                            UltimaConexion = asesor.ultimaConexion != null
                                                ? asesor.ultimaConexion.fecha_termina.Value.ToShortDateString() + " " +
                                                  asesor.ultimaConexion.fecha_termina.Value.ToShortTimeString()
                                                : "",
                                            InicioConexion = duracion,
                                            estado = asesor.ultimaConexion != null
                                                ? context.estados_sesiones
                                                    .FirstOrDefault(y => y.id == asesor.ultimaConexion.estado).descripcion
                                                : "",
                                            estado_id = asesor.ultimaConexion != null ? asesor.ultimaConexion.estado : 0,
                                            detalle = asesor.ultimaConexion != null
                                                ? asesor.ultimaConexion.Observacon ?? ""
                                                : ""
                                        });
                                    }

                                    else if (buscar != null)
                                    {
                                        listaAsesores.Add(new ListarAsesoresModel
                                        {
                                            nombres = asesor.user_nombre + " " + asesor.user_apellido,
                                            UltimaConexion = asesor.ultimaConexion != null
                                                ? asesor.ultimaConexion.fecha_termina.Value.ToShortDateString() + " " +
                                                  asesor.ultimaConexion.fecha_termina.Value.ToShortTimeString()
                                                : "",
                                            InicioConexion = duracion,
                                            estado = asesor.ultimaConexion != null
                                                ? context.estados_sesiones
                                                    .FirstOrDefault(y => y.id == asesor.ultimaConexion.estado).descripcion
                                                : "",
                                            estado_id = asesor.ultimaConexion != null ? asesor.ultimaConexion.estado : 0,
                                            detalle = asesor.ultimaConexion != null
                                                ? asesor.ultimaConexion.Observacon ?? ""
                                                : "",
                                            nombreProspecto =
                                                buscar.prinom_tercero + ' ' + buscar.segnom_tercero + ' ' +
                                                buscar.apellido_tercero + ' ' + buscar.segapellido_tercero
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }

            List<ListarAsesoresModel> listaAsesoresOrdenada = listaAsesores.OrderBy(x => x.estado_id).ToList();
            var data = new
            {
                listaAsesoresOrdenada,
                conectados,
                ocupados,
                ausentes,
                desconectados,
                esperando
            };

            #region comentariado

            //double diferenciaSegundos = 9;
            //if (asesor.ultimaConexion!=null) {
            //    DateTime ultimaConexion = asesor.ultimaConexion.fecha_termina ?? DateTime.Now;
            //    TimeSpan differenciaSegundos = DateTime.Now - ultimaConexion;
            //    diferenciaSegundos = differenciaSegundos.TotalHours;
            //    if (asesor.ultimaConexion.estado==1)
            //    {
            //        conectados++;
            //    }
            //    if (asesor.ultimaConexion.estado == 2)
            //    {
            //        ocupados++;
            //    }
            //    if (asesor.ultimaConexion.estado == 3)
            //    {
            //        ausentes++;
            //    }
            //}

            //if (diferenciaSegundos < 8)
            //{
            //    listaAsesores.Add(new ListarAsesoresModel()
            //    {
            //        nombres = asesor.user_nombre + " " + asesor.user_apellido,
            //        UltimaConexion = asesor.ultimaConexion != null ? asesor.ultimaConexion.fecha_termina.Value.ToShortDateString() +" "+ asesor.ultimaConexion.fecha_termina.Value.ToShortTimeString() : "",
            //        InicioConexion = asesor.ultimaConexion.fecha_inicia != null ? asesor.ultimaConexion.fecha_inicia.ToShortDateString() + " " + asesor.ultimaConexion.fecha_inicia.ToShortTimeString() : "",
            //        estado = asesor.ultimaConexion != null ? context.estados_sesiones.FirstOrDefault(y => y.id == asesor.ultimaConexion.estado).descripcion : "",
            //        detalle = asesor.ultimaConexion != null ? asesor.ultimaConexion.Observacon??"" : "",
            //    });
            //}
            //else {
            //    listaAsesores.Add(new ListarAsesoresModel()
            //    {
            //        nombres = asesor.user_nombre + " " + asesor.user_apellido,
            //        UltimaConexion = asesor.ultimaConexion != null ? asesor.ultimaConexion.fecha_termina.Value.ToShortDateString() + " " + asesor.ultimaConexion.fecha_termina.Value.ToShortTimeString() : "",
            //        InicioConexion = asesor.ultimaConexion != null ? asesor.ultimaConexion.fecha_inicia.ToShortDateString() + " " + asesor.ultimaConexion.fecha_inicia.ToShortTimeString() : "",
            //        estado = "Desconectado",
            //        detalle = asesor.ultimaConexion != null ? asesor.ultimaConexion.Observacon??"" : "",
            //    });
            //}

            #endregion

            #region comentariado 

            //var data = buscaAsesores.Select(x => new {
            //    ultimaConexion = x.ultimaConexion != null ? x.ultimaConexion.fecha_termina.ToString() : "",
            //    x.user_nombre,
            //    x.user_apellido,
            //    estado = x.ultimaConexion!=null? context.estados_sesiones.FirstOrDefault(y => y.id == x.ultimaConexion.estado).descripcion:""
            //});

            #endregion

            #endregion

            var buscarAsesores = (from a in context.sesion_logasesor
                                  join b in context.users
                                      on a.user_id equals b.user_id
                                  join c in context.prospectos
                                      on a.idprospecto equals c.id into cc
                                  from c in cc.DefaultIfEmpty()
                                  orderby a.fecha_inicia descending
                                  where b.rol_id == 4
                                  select new
                                  {
                                      a.user_id,
                                      a.fecha_inicia,
                                      a.fecha_termina,
                                      a.idprospecto,
                                      b.user_nombre,
                                      b.user_apellido
                                  }).GroupBy(x => x.user_id).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarAsesores()
        {
            int bodega = Convert.ToInt32(Session["user_bodega"]);
            DateTime ayer = DateTime.Now.AddDays(-1);
            DateTime hoy = DateTime.Now.AddDays(1);
            List<ListarAsesoresModel> listaAsesores = new List<ListarAsesoresModel>();

            var buscarprospecto = (from a in context.sesion_logasesor
                                   join b in context.prospectos
                                       on a.idprospecto equals b.id into xx
                                   from b in xx.DefaultIfEmpty()
                                   join c in context.users
                                       on a.user_id equals c.user_id into zz
                                   from c in zz.DefaultIfEmpty()
                                   join d in context.bodega_usuario
                                       on c.user_id equals d.id_usuario
                                   where (c.rol_id == 4 || c.rol_id == 2030) && d.id_bodega == bodega
                                   group a by new { a.user_id }
                into asesor
                                   select new
                                   {
                                       id = asesor.Select(x => x.id),
                                       asesor.Key.user_id,
                                       fechaInicia = asesor.Max(x => x.fecha_inicia),
                                       fechatermina = asesor.Max(x => x.fecha_termina),
                                       prospecto = asesor.Select(x => x.idprospecto).FirstOrDefault()
                                   }).ToList();

            foreach (var item in buscarprospecto)
            {
                var buscarDatos = (from a in context.sesion_logasesor
                                   join b in context.users
                                       on a.user_id equals b.user_id
                                   join c in context.prospectos
                                       on a.idprospecto equals c.id into tmp
                                   from c in tmp.DefaultIfEmpty()
                                   join d in context.icb_medio_comunicacion
                                       on c.medcomun_id equals d.medcomun_id into tmp1
                                   from d in tmp1.DefaultIfEmpty()
                                   join e in context.icb_tptramite_prospecto
                                       on c.tptramite equals e.tptrapros_id into tmp2
                                   from e in tmp2.DefaultIfEmpty()
                                   join f in context.tp_origen
                                       on c.origen_id equals f.tporigen_id into tmp3
                                   from f in tmp3.DefaultIfEmpty()
                                   join g in context.modelo_vehiculo
                                       on c.modelo equals g.modvh_codigo into tmp4
                                   from g in tmp4.DefaultIfEmpty()
                                   orderby a.fecha_inicia descending
                                   where a.user_id == item.user_id
                                   select new
                                   {
                                       a.fecha_inicia,
                                       b.user_nombre,
                                       b.user_apellido,
                                       a.estado,
                                       c.prinom_tercero,
                                       c.segnom_tercero,
                                       c.apellido_tercero,
                                       c.segapellido_tercero,
                                       d.medcomun_descripcion,
                                       e.tptrapros_descripcion,
                                       f.tporigen_nombre,
                                       g.modvh_nombre
                                   }).OrderByDescending(x => x.fecha_inicia).FirstOrDefault();
                if (buscarDatos != null)
                {
                    DateTime fechaInicia = Convert.ToDateTime(item.fechaInicia);
                    DateTime Ahora = DateTime.Now;
                    DateTime InicioDia = DateTime.Now.Date;
                    DateTime fechaHoy = new DateTime(InicioDia.Year, InicioDia.Month, InicioDia.Day, 07, 00, 00);
                    string duracion = "";
                    string nombre = buscarDatos.prinom_tercero + ' ' + buscarDatos.segnom_tercero + ' ' +
                                 buscarDatos.apellido_tercero + ' ' + buscarDatos.segapellido_tercero;
                    if (buscarDatos.estado == 1)
                    {
                        TimeSpan tiempo_transcurrido = Ahora - fechaInicia;
                        duracion = tiempo_transcurrido.Hours.ToString("00") + ":" +
                                   tiempo_transcurrido.Minutes.ToString("00") + ":" +
                                   tiempo_transcurrido.Seconds.ToString("00");


                        listaAsesores.Add(new ListarAsesoresModel
                        {
                            nombres = buscarDatos.user_nombre + ' ' + buscarDatos.user_apellido,
                            InicioConexion = duracion,
                            estado = Convert.ToString(buscarDatos.estado),
                            nombreProspecto = nombre,
                            medioComunicacion = buscarDatos.medcomun_descripcion != null
                                ? buscarDatos.medcomun_descripcion
                                : "",
                            tramite =
                                buscarDatos.tptrapros_descripcion != null ? buscarDatos.tptrapros_descripcion : "",
                            fuente = buscarDatos.tporigen_nombre != null ? buscarDatos.tporigen_nombre : "",
                            detalle = buscarDatos.modvh_nombre != null ? buscarDatos.modvh_nombre : "",
                            horaConexion = buscarDatos.fecha_inicia.ToString("yyyy/MM/dd HH:mm")
                        });
                    }
                    else if (buscarDatos.estado == 2)
                    {
                        TimeSpan tiempo_transcurrido = Ahora - fechaInicia;
                        duracion = tiempo_transcurrido.Hours.ToString("00") + ":" +
                                   tiempo_transcurrido.Minutes.ToString("00") + ":" +
                                   tiempo_transcurrido.Seconds.ToString("00");


                        listaAsesores.Add(new ListarAsesoresModel
                        {
                            nombres = buscarDatos.user_nombre + ' ' + buscarDatos.user_apellido,
                            InicioConexion = duracion,
                            estado = Convert.ToString(buscarDatos.estado),
                            nombreProspecto = nombre,
                            medioComunicacion = buscarDatos.medcomun_descripcion != null
                                ? buscarDatos.medcomun_descripcion
                                : "",
                            tramite =
                                buscarDatos.tptrapros_descripcion != null ? buscarDatos.tptrapros_descripcion : "",
                            fuente = buscarDatos.tporigen_nombre != null ? buscarDatos.tporigen_nombre : "",
                            detalle = buscarDatos.modvh_nombre != null ? buscarDatos.modvh_nombre : "",
                            horaConexion = buscarDatos.fecha_inicia.ToString("yyyy/MM/dd HH:mm")
                        });
                    }
                    else if (buscarDatos.estado == 3)
                    {
                        TimeSpan tiempo_transcurrido = Ahora - fechaInicia;
                        duracion = tiempo_transcurrido.Hours.ToString("00") + ":" +
                                   tiempo_transcurrido.Minutes.ToString("00") + ":" +
                                   tiempo_transcurrido.Seconds.ToString("00");


                        listaAsesores.Add(new ListarAsesoresModel
                        {
                            nombres = buscarDatos.user_nombre + ' ' + buscarDatos.user_apellido,
                            InicioConexion = duracion,
                            estado = Convert.ToString(buscarDatos.estado),
                            nombreProspecto = nombre,
                            medioComunicacion = buscarDatos.medcomun_descripcion != null
                                ? buscarDatos.medcomun_descripcion
                                : "",
                            tramite =
                                buscarDatos.tptrapros_descripcion != null ? buscarDatos.tptrapros_descripcion : "",
                            fuente = buscarDatos.tporigen_nombre != null ? buscarDatos.tporigen_nombre : "",
                            detalle = buscarDatos.modvh_nombre != null ? buscarDatos.modvh_nombre : "",
                            horaConexion = buscarDatos.fecha_inicia.ToString("yyyy/MM/dd HH:mm")
                        });
                    }
                    else if (buscarDatos.estado == 4)
                    {
                        if (fechaInicia < InicioDia)
                        {
                            TimeSpan tiempo_transcurrido2 = Ahora - fechaHoy;
                            duracion = tiempo_transcurrido2.Hours.ToString("00") + ":" +
                                       tiempo_transcurrido2.Minutes.ToString("00") + ":" +
                                       tiempo_transcurrido2.Seconds.ToString("00");

                            listaAsesores.Add(new ListarAsesoresModel
                            {
                                nombres = buscarDatos.user_nombre + ' ' + buscarDatos.user_apellido,
                                InicioConexion = duracion,
                                estado = Convert.ToString(buscarDatos.estado),
                                nombreProspecto = nombre,
                                medioComunicacion = buscarDatos.medcomun_descripcion != null
                                    ? buscarDatos.medcomun_descripcion
                                    : "",
                                tramite = buscarDatos.tptrapros_descripcion != null
                                    ? buscarDatos.tptrapros_descripcion
                                    : "",
                                fuente = buscarDatos.tporigen_nombre != null ? buscarDatos.tporigen_nombre : "",
                                detalle = buscarDatos.modvh_nombre != null ? buscarDatos.modvh_nombre : "",
                                horaConexion = fechaHoy.ToString("yyyy/MM/dd HH:mm")
                            });
                        }
                        else if (fechaInicia > InicioDia)
                        {
                            TimeSpan tiempo_transcurrido = Ahora - fechaInicia;
                            duracion = tiempo_transcurrido.Hours.ToString("00") + ":" +
                                       tiempo_transcurrido.Minutes.ToString("00") + ":" +
                                       tiempo_transcurrido.Seconds.ToString("00");

                            listaAsesores.Add(new ListarAsesoresModel
                            {
                                nombres = buscarDatos.user_nombre + ' ' + buscarDatos.user_apellido,
                                InicioConexion = duracion,
                                estado = Convert.ToString(buscarDatos.estado),
                                nombreProspecto = nombre,
                                medioComunicacion = buscarDatos.medcomun_descripcion != null
                                    ? buscarDatos.medcomun_descripcion
                                    : "",
                                tramite = buscarDatos.tptrapros_descripcion != null
                                    ? buscarDatos.tptrapros_descripcion
                                    : "",
                                fuente = buscarDatos.tporigen_nombre != null ? buscarDatos.tporigen_nombre : "",
                                detalle = buscarDatos.modvh_nombre != null ? buscarDatos.modvh_nombre : "",
                                horaConexion = buscarDatos.fecha_inicia.ToString("yyyy/MM/dd HH:mm")
                            });
                        }
                    }
                }
            }

            List<ListarAsesoresModel> listaAsesoresOrdenada =
                listaAsesores.OrderByDescending(x => x.InicioConexion).OrderBy(x => x.estado).ToList();
            var data = new
            {
                listaAsesoresOrdenada
            };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarNotificacionPropspecto()
        {
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            var buscarNotificacion = (from alerta in context.alertaasesor
                                      join tercero in context.icb_terceros
                                          on alerta.propecto equals tercero.tercero_id
                                      where alerta.aprobado == null && alerta.recibido == false && alerta.asesor == usuario
                                      select new
                                      {
                                          alerta.id,
                                          tercero_nombre = tercero.razon_social + tercero.prinom_tercero + " " + tercero.segnom_tercero +
                                                           " " + tercero.apellido_tercero + " " + tercero.segapellido_tercero,
                                          tercero.tercero_id
                                      }).FirstOrDefault();
            if (buscarNotificacion != null)
            {
                //var ultimaSesionAsesor = context.sesion_logasesor.OrderByDescending(x => x.id).FirstOrDefault(x => x.user_id == usuario);
                //if (ultimaSesionAsesor != null)
                //{
                //    ultimaSesionAsesor.fecha_inicia = DateTime.Now;
                //    ultimaSesionAsesor.fecha_termina = DateTime.Now;
                //    ultimaSesionAsesor.estado = 2;
                //    context.Entry(ultimaSesionAsesor).State = System.Data.Entity.EntityState.Modified;
                //    context.SaveChanges();
                //}
                var mensaje = new
                {
                    notificacion = true,
                    buscarNotificacion.tercero_id,
                    alertaId = buscarNotificacion.id,
                    alerta = "Se ha asignado el prospecto " + buscarNotificacion.tercero_nombre
                };
                return Json(mensaje, JsonRequestBehavior.AllowGet);
            }

            var data = new
            {
                notificacion = false
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AtenderUsuario(int? id, bool? atendido, int? idAlerta)
        {
            alertaasesor buscarAlerta = context.alertaasesor.FirstOrDefault(x => x.id == idAlerta);
            icb_terceros buscarTercero = context.icb_terceros.FirstOrDefault(x => x.tercero_id == buscarAlerta.propecto);
            if (buscarAlerta != null)
            {
                int usuario = Convert.ToInt32(Session["user_usuarioid"]);
                //para atender a un nuevo usuario todas las demas sesiones que tengan prospecto deben cerrarse
                sesion_logasesor ultimaSesionAsesor = context.sesion_logasesor.OrderByDescending(x => x.id)
                    .FirstOrDefault(x => x.user_id == usuario);
                if (ultimaSesionAsesor != null)
                {
                    ultimaSesionAsesor.fecha_inicia = DateTime.Now;
                    ultimaSesionAsesor.fecha_termina = DateTime.Now;
                    if (ultimaSesionAsesor.estado != 4)
                    {
                        ultimaSesionAsesor.estado = 4;
                    }
                }

                #region atendido false

                if (atendido == false)
                {
                    //creo una nueva sesion
                    sesion_logasesor nuevaSesionAsesor = new sesion_logasesor
                    {
                        bodega = ultimaSesionAsesor.bodega,
                        estado = 1,
                        fecha_inicia = DateTime.Now,
                        fecha_termina = DateTime.Now.AddMinutes(5),
                        user_id = ultimaSesionAsesor.user_id,
                    };
                    context.sesion_logasesor.Add(nuevaSesionAsesor);
                    buscarAlerta.aprobado = atendido;
                    context.Entry(ultimaSesionAsesor).State = EntityState.Modified;

                    if (buscarAlerta.recibido == false)
                    {
                        int guardar = 0;
                        // Se cambia la alerta para que al rol anfitriona le aparezca un alerta para que asigne otro asesor al prospecto
                        buscarAlerta.recibido = true;
                        buscarAlerta.reasignado = true;
                        buscarAlerta.rechazado = true;
                        context.Entry(buscarAlerta).State = EntityState.Modified;
                        guardar = context.SaveChanges();

                        prospectos prospectoID = context.prospectos.FirstOrDefault(x => x.idtercero == buscarAlerta.propecto);
                        int prospecto_ID = Convert.ToInt32(prospectoID.id);
                        asignacion asignado = context.asignacion.OrderByDescending(x => x.id)
                            .FirstOrDefault(x => x.idProspecto == prospecto_ID && x.idAsesor == usuario);

                        asignado.estado = false;
                        asignado.fechaFin = DateTime.Now;
                        context.Entry(asignado).State = EntityState.Modified;
                        guardar = context.SaveChanges();

                        if (guardar > 0)
                        {
                            return Json(
                                new { success = false, error_message = "El usuario sera atendido por otro asesor" },
                                JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(
                            new
                            {
                                success = false,
                                error_message =
                                    "El usuario ya fue asignado a otro asesor por superar limite de tiempo ausente"
                            }, JsonRequestBehavior.AllowGet);
                    }
                }

                #endregion

                #region atendido true

                if (atendido == true)
                {
                    if (buscarAlerta.recibido == false)
                    {
                        //
                        if (ultimaSesionAsesor != null)
                        {


                            int idProspecto = (from p in context.prospectos
                                               join t in context.icb_terceros
                                                   on p.idtercero equals t.tercero_id
                                               where p.idtercero == id
                                               select p.id).FirstOrDefault();

                            sesion_logasesor nuevaSesionAsesor = new sesion_logasesor
                            {
                                bodega = ultimaSesionAsesor.bodega,
                                estado = 2,
                                fecha_inicia = DateTime.Now,
                                fecha_termina = DateTime.Now.AddMinutes(5),
                                user_id = ultimaSesionAsesor.user_id,
                                idprospecto = idProspecto,
                            };
                            context.sesion_logasesor.Add(nuevaSesionAsesor);
                            context.Entry(ultimaSesionAsesor).State = EntityState.Modified;

                            prospectos prospecto =
                                context.prospectos.FirstOrDefault(x => x.idtercero == buscarAlerta.propecto);
                            int prospecto_ID = Convert.ToInt32(prospecto.id);
                            asignacion asignado = context.asignacion.OrderByDescending(x => x.id)
                                .FirstOrDefault(x => x.idProspecto == prospecto_ID && x.idAsesor == usuario);

                            asignado.estado = true;
                            //asignado.fechaFin = DateTime.Now;
                            context.Entry(asignado).State = EntityState.Modified;

                            buscarTercero.asesor_id = usuario;
                            context.Entry(buscarTercero).State = EntityState.Modified;

                            prospecto.asesor_id = usuario;
                            context.Entry(prospecto).State = EntityState.Modified;

                            context.SaveChanges();
                        }

                        buscarAlerta.aprobado = atendido;
                        buscarAlerta.recibido = atendido;

                        context.Entry(buscarAlerta).State = EntityState.Modified;
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        if (ultimaSesionAsesor != null)
                        {
                            sesion_logasesor nuevaSesionAsesor = new sesion_logasesor
                            {
                                bodega = ultimaSesionAsesor.bodega,
                                estado = 1,
                                fecha_inicia = DateTime.Now,
                                fecha_termina = DateTime.Now.AddMinutes(5),
                                user_id = ultimaSesionAsesor.user_id,
                            };
                            context.sesion_logasesor.Add(nuevaSesionAsesor);
                            context.Entry(ultimaSesionAsesor).State = EntityState.Modified;
                            context.SaveChanges();
                        }

                        return Json(
                            new
                            {
                                success = false,
                                error_message =
                                    "El usuario ya fue asignado a otro asesor por superar limite de tiempo ausente"
                            }, JsonRequestBehavior.AllowGet);
                    }
                }

                #endregion
            }

            return Json(new { success = false, error_message = "La notificacion no fue encontrada" },
                JsonRequestBehavior.AllowGet);
        }
         
        public JsonResult ultimaAlerta()
        {
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            int data = (from a in context.alertaasesor
                        orderby a.id descending
                        where a.anfitrion == usuario
                        select a.id).FirstOrDefault();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult cancelarProspecto(bool atendido, int? idAlerta, bool? recibioNoti)
        {
            //var buscarTercero = context.alertaasesor.OrderByDescending(x=>x.id).FirstOrDefault(x=>x.propecto==id);
            alertaasesor buscarTercero = context.alertaasesor.FirstOrDefault(x => x.id == idAlerta);
            if (buscarTercero != null)
            {
                //int usuario = Convert.ToInt32(Session["user_usuarioid"]);
                //int rol = Convert.ToInt32(Session["user_rolid"]);
                sesion_logasesor ultimaSesionAsesor = context.sesion_logasesor.OrderByDescending(x => x.id)
                    .FirstOrDefault(x => x.user_id == buscarTercero.asesor);
                if (ultimaSesionAsesor != null)
                {
                    ultimaSesionAsesor.fecha_inicia = DateTime.Now;
                    ultimaSesionAsesor.fecha_termina = DateTime.Now;
                }

                if (atendido == false)
                {
                    // Se cambia el turno del asesor para que no se asigne a la siguiente persona prospecto porque perdio el turno por no atender al que se le asigno
                    //if (ultimaSesionAsesor != null-------)
                    //{
                    ultimaSesionAsesor.estado = 1;

                    context.Entry(ultimaSesionAsesor).State = EntityState.Modified;
                    //buscarTercero.aprobado = atendido;
                    //}

                    if (buscarTercero.recibido == true || buscarTercero.recibido == false && recibioNoti == true)
                    {
                        // Se cambia la alerta para que al rol anfitriona le aparezca un alerta para que asigne otro asesor al prospecto
                        buscarTercero.recibido = true;
                        buscarTercero.aprobado = false;
                        buscarTercero.rechazado = true;
                        buscarTercero.reasignado = true;
                        //if (rol == 7)
                        //{
                        //    buscarTercero.cancelado = true;
                        //}
                        context.Entry(buscarTercero).State = EntityState.Modified;

                        int terceroProspecto = buscarTercero.propecto;
                        prospectos prospectoID = context.prospectos.FirstOrDefault(x => x.idtercero == terceroProspecto);
                        int prospecto_ID = Convert.ToInt32(prospectoID.id);
                        asignacion asignado = context.asignacion.OrderByDescending(x => x.id)
                            .FirstOrDefault(x => x.idProspecto == prospecto_ID);

                        asignado.estado = false;
                        asignado.fechaFin = DateTime.Now;
                        context.Entry(asignado).State = EntityState.Modified;

                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            return Json(
                                new { success = false, error_message = "El usuario sera atendido por otro asesor" },
                                JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(
                            new
                            {
                                success = false,
                                error_message =
                                    "El usuario ya fue asignado a otro asesor por superar limite de tiempo ausente"
                            }, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            return Json(new { success = false, error_message = "La notificacion no fue encontrada" },
                JsonRequestBehavior.AllowGet);
        }

        public JsonResult tiempoAlerta()
        {
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            string buscarTiempoEspera =
                (from parametro in context.icb_sysparameter
                 where parametro.syspar_cod == "P53"
                 select parametro.syspar_value).FirstOrDefault();
            int segundos = Convert.ToInt32(buscarTiempoEspera);

            return Json(segundos, JsonRequestBehavior.AllowGet);
        }
    }
}