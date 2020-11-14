using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class agenda_demosController : Controller
    {
        int idubica=0;
        private readonly Iceberg_Context db = new Iceberg_Context();


        public ActionResult Busquedas(int? menu)
        {
            //BuscarFavoritos(menu);
            //BuscarParametros();
            return View();
        }

        // GET: agenda_demos
        public ActionResult Index()
        {

              var lisdemos = (from v in db.vdemos
                            join vh in db.icb_vehiculo
                                on v.planmayor equals vh.plan_mayor
                            join mh in db.modelo_vehiculo
                                on vh.modvh_id equals mh.modvh_codigo
                            join uvh in db.ubicacion_vehiculo 
                            on v.ubicacion equals uvh.ubivh_id
                            where v.estado
                            select new
                            {
                                v.placa,
                                v.id,
                                mh.modvh_nombre,
                                v.planmayor,
                                uvh.ubivh_nombre
                            }).ToList();
            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in lisdemos)
            {
                lista.Add(new SelectListItem
                {
                    Text = item.placa != null
                        ? item.placa + " (" + item.modvh_nombre + ")"
                        : item.planmayor + " (" + item.modvh_nombre + ")",
                    Value = item.id.ToString()
                });
            }

            ViewBag.demo_id = lista;


            int asesor_actual = Convert.ToInt32(Session["user_usuarioid"]);
            int rol_actual = Convert.ToInt32(Session["user_rolid"]);
            if (rol_actual == 4 || rol_actual == 7)
            {
                ViewBag.tipo = new SelectList(db.vtiposdemos.Where(x => x.id == 2), "id", "descripcion");
            }
            else
            {
                ViewBag.tipo = new SelectList(db.vtiposdemos, "id", "descripcion");
            }

            ViewBag.asesor_actual = asesor_actual;

            var listAsesores = (from u in db.users
                                where u.user_estado && u.rol_id == 4
                                select new
                                {
                                    id = u.user_id,
                                    nombre = u.user_nombre + " " + u.user_apellido
                                }).ToList();

            ViewBag.asesor_id = new SelectList(listAsesores, "id", "nombre", asesor_actual);
            ViewBag.rol_log = Convert.ToInt32(Session["user_rolid"]);
            ViewBag.ruta = new SelectList(db.vrutasdemo, "id", "ruta");
            ViewBag.ncliente = new SelectList(db.icb_terceros, "tercero_id", "doc_tercero");
            ViewBag.id_encuesta = new SelectList(db.crm_encuestas, "id", "Descripcion");
            ViewBag.lisdemos = lisdemos;
            return View();
        }

        public JsonResult BuscarUbicacion(int[] param)
        {
            idubica = param[0];
                var data = (from v in db.vdemos
                            join vh in db.icb_vehiculo
                                on v.planmayor equals vh.plan_mayor
                            join mh in db.modelo_vehiculo
                                on vh.modvh_id equals mh.modvh_codigo
                            join uvh in db.ubicacion_vehiculo
                            on v.ubicacion equals uvh.ubivh_id
                            where v.estado && v.id == idubica
                            select new
                            {
                                v.placa,
                                v.id,
                                mh.modvh_nombre,
                                v.planmayor,
                                uvh.ubivh_nombre
                            }).ToList();
            ViewBag.Ubicacion = data[0].ubivh_nombre;

            return Json(data, JsonRequestBehavior.AllowGet);
        }
            //foreach (var item in ViewBag.lisdemos)
            //{
            //    var nombre = item.id == Convert.ToInt32(iden) ? item.ubivh_nombre : "";
            //}



        // POST: agenda_demos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(agenda_demos agenda_demos)
        {
            int bodega = Convert.ToInt32(Session["user_bodega"]);
            if (ModelState.IsValid)
            {
                bool citas = CalcularDisponible(agenda_demos.demo_id, DateTime.Now, agenda_demos.desde,
                    agenda_demos.hasta);
                bool citasAsesor = CalcularDisponibleAsesor(Convert.ToInt32(agenda_demos.asesor_id), DateTime.Now,
                    agenda_demos.desde, agenda_demos.hasta);
                if (citas && citasAsesor)
                {
                    if (agenda_demos.hasta > agenda_demos.desde)
                    {
                        agenda_demos.idbodega = bodega;
                        agenda_demos.estado = "Activa";
                        db.agenda_demos.Add(agenda_demos);

                        string idDemo = db.vdemos.FirstOrDefault(x => x.id == agenda_demos.demo_id).planmayor;

                        agenda_asesor agenda = new agenda_asesor
                        {
                            desde = agenda_demos.desde,
                            hasta = agenda_demos.hasta,
                            estado = "Activa",
                            asesor_id = Convert.ToInt32(agenda_demos.asesor_id),
                            descripcion = "Agenda demo del vehículo " + idDemo,
                            cliente = agenda_demos.ncliente,
                            placa = idDemo,
                            tipoorigen = 2,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                        };
                        db.agenda_asesor.Add(agenda);

                        int result = db.SaveChanges();
                        if (result > 0)
                        {
                            TempData["mensaje"] = " Cita creada correctamente";
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error al crear la Cita, por favor valide los datos";
                        }
                    }
                    else
                    {
                        TempData["mensaje_error"] =
                            "La fecha final de la cita debe ser mayor a la inicial, por favor valide";
                    }
                }
                else
                {
                    TempData["mensaje_error"] =
                        "Ya tiene una cita agendada para el rango de fecha y hora ingresados, por favor valide";
                }
            }

            return RedirectToAction("Index");
        }

        // POST: agenda_demos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(agenda_demos agenda_demos)
        {
            if (ModelState.IsValid)
            {
                bool citas = true;
                if (agenda_demos.estado == "Reprogramada")
                {
                    citas = CalcularDisponible(agenda_demos.demo_id, DateTime.Now, agenda_demos.desde,
                        agenda_demos.hasta);
                }

                if (citas)
                {
                    agenda_demos existe = db.agenda_demos.FirstOrDefault(x => x.id == agenda_demos.id);
                    if (existe != null)
                    {
                        existe.demo_id = agenda_demos.demo_id;
                        existe.asesor_id = agenda_demos.asesor_id;
                        existe.tercero_id = agenda_demos.tercero_id;
                        existe.desde = agenda_demos.desde;
                        existe.hasta = agenda_demos.hasta;
                        existe.titulo = agenda_demos.titulo;
                        existe.descripcion = agenda_demos.descripcion;
                        existe.estado = agenda_demos.estado;
                        existe.motivo = agenda_demos.motivo;
                        existe.ruta = agenda_demos.ruta;
                        existe.ncliente = agenda_demos.ncliente;
                        existe.nombre = agenda_demos.nombre;
                        existe.celular = agenda_demos.celular;
                        existe.correo = agenda_demos.correo;
                        existe.telefono = agenda_demos.telefono;
                        existe.tipo = agenda_demos.tipo;
                        existe.idbodega = agenda_demos.idbodega;
                        db.Entry(existe).State = EntityState.Modified;
                    }

                    int result = db.SaveChanges();

                    if (result == 1)
                    {
                        TempData["mensaje"] = " Cita editada correctamente";
                    }
                    else
                    {
                        TempData["mensaje_error"] = " Error al editar la Cita, por favor valide los datos";
                    }
                }
            }

            return RedirectToAction("Index");
        }

        public JsonResult BuscarCitas(int demo_id)
        {
            //var citas = context.icb_cita_perito.Where(x=>x.perito_id==peritoId).ToList();
            var citas = (from c in db.agenda_demos
                         where c.demo_id == demo_id
                         select new
                         {
                             c.desde,
                             c.hasta,
                             c.titulo,
                             c.id,
                             c.motivo,
                             c.descripcion,
                             c.asesor_id,
                             c.estado,
                             c.demo_id,
                             c.vdemos.placa,
                             c.ncliente,
                             c.nombre,
                             c.tercero_id,
                             c.tipo,
                             c.ruta,
                             c.telefono,
                             c.correo,
                             c.celular,
                             c.idbodega
                         }).ToList();

            var events = citas.Select(x => new
            {
                title = x.titulo + " " + x.nombre,
                start = x.desde.ToString("yyyy/MM/dd") + " " + x.desde.Hour.ToString("00") + ":" +
                        x.desde.Minute.ToString("00") + ":00",
                end = x.hasta.ToString("yyyy/MM/dd") + " " + x.hasta.Hour.ToString("00") + ":" +
                      x.hasta.Minute.ToString("00") + ":00",
                x.id,
                x.descripcion,
                x.asesor_id,
                x.motivo,
                x.estado,
                x.demo_id,
                x.placa,
                cliente = x.ncliente,
                x.tercero_id,
                x.nombre,
                x.celular,
                x.correo,
                x.telefono,
                x.tipo,
                x.ruta,
                bodega = x.idbodega
            });

            return Json(events, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarCliente(string cliente)
        {
            var data = from t in db.icb_terceros
                           //join tc in db.tercero_cliente
                           //on t.tercero_id equals tc.tercero_id
                       where t.doc_tercero == cliente
                       select new
                       {
                           nombre = t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero + " " +
                                    t.segapellido_tercero,
                           telefono = t.telf_tercero,
                           celular = t.celular_tercero,
                           correo = t.email_tercero,
                           idtercero = t.tercero_id
                       };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public bool CalcularDisponible(int? demo_id, DateTime fecha, DateTime inicio, DateTime fin)
        {
            List<agenda_demos> citasDelMes = db.agenda_demos.Where(x =>
                x.desde.Year == fecha.Year && x.desde.Month == fecha.Month && x.demo_id == demo_id).ToList();

            foreach (agenda_demos cita in citasDelMes)
            {
                int compare = DateTime.Compare(inicio, cita.desde);
                if (DateTime.Compare(inicio, cita.desde) == 0)
                {
                    return false;
                }

                if (DateTime.Compare(inicio, cita.desde) > 0 && DateTime.Compare(inicio, cita.hasta) < 0)
                {
                    return false;
                }

                if (DateTime.Compare(inicio, cita.desde) > 0 && DateTime.Compare(fin, cita.hasta) < 0)
                {
                    return false;
                }

                if (DateTime.Compare(fin, cita.desde) > 0 && DateTime.Compare(fin, cita.hasta) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        public bool CalcularDisponibleAsesor(int asesor_id, DateTime fecha, DateTime inicio, DateTime fin)
        {
            List<agenda_asesor> citasDelMes = db.agenda_asesor.Where(x =>
                x.asesor_id == asesor_id && x.desde.Year == inicio.Year && x.desde.Month == inicio.Month).ToList();

            foreach (agenda_asesor cita in citasDelMes)
            {
                if (DateTime.Compare(inicio, cita.desde) == 0)
                {
                    return false;
                }
                else if (DateTime.Compare(inicio, cita.desde) > 0 && DateTime.Compare(inicio, cita.hasta) < 0)
                {
                    return false;
                }
                else if (DateTime.Compare(inicio, cita.desde) > 0 && DateTime.Compare(fin, cita.hasta) < 0)
                {
                    return false;
                }
                else if (DateTime.Compare(fin, cita.desde) > 0 && DateTime.Compare(fin, cita.hasta) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        public JsonResult BuscarHorario(int demo_id)
        {
            var data = from h in db.parametrizacion_horario
                       where h.demo_id == demo_id
                       select new
                       {
                           lunes = h.lunes_desde.ToString().Substring(0, 5) + " - " +
                                   h.lunes_hasta.ToString().Substring(0, 5) + "  -  " +
                                   h.lunes_desde2.ToString().Substring(0, 5) + " - " +
                                   h.lunes_hasta2.ToString().Substring(0, 5),
                           martes = h.martes_desde.ToString().Substring(0, 5) + " - " +
                                    h.martes_hasta.ToString().Substring(0, 5) + "  -  " +
                                    h.martes_desde2.ToString().Substring(0, 5) + " - " +
                                    h.martes_hasta2.ToString().Substring(0, 5),
                           miercoles = h.miercoles_desde.ToString().Substring(0, 5) + " - " +
                                       h.miercoles_hasta.ToString().Substring(0, 5) + " - " +
                                       h.miercoles_desde2.ToString().Substring(0, 5) + " - " +
                                       h.miercoles_hasta2.ToString().Substring(0, 5),
                           jueves = h.jueves_desde.ToString().Substring(0, 5) + " - " +
                                    h.jueves_hasta.ToString().Substring(0, 5) + "  -  " +
                                    h.jueves_desde2.ToString().Substring(0, 5) + " - " +
                                    h.jueves_hasta2.ToString().Substring(0, 5),
                           viernes = h.viernes_desde.ToString().Substring(0, 5) + " - " +
                                     h.viernes_hasta.ToString().Substring(0, 5) + "  -  " +
                                     h.viernes_desde2.ToString().Substring(0, 5) + " - " +
                                     h.viernes_hasta2.ToString().Substring(0, 5),
                           sabado = h.sabado_desde.ToString().Substring(0, 5) + " - " +
                                    h.sabado_hasta.ToString().Substring(0, 5) + "  -  " +
                                    h.sabado_desde2.ToString().Substring(0, 5) + " - " +
                                    h.sabado_hasta2.ToString().Substring(0, 5),
                           domingo = h.domingo_desde.ToString().Substring(0, 5) + " - " +
                                     h.domingo_hasta.ToString().Substring(0, 5) + "  -  " +
                                     h.domingo_desde2.ToString().Substring(0, 5) + " - " +
                                     h.domingo_hasta2.ToString().Substring(0, 5),
                           no_disponible = h.ndispo_fecha_inicio != null ? h.ndispo_fecha_inicio + " a " + h.ndispo_fecha_fin :
                               h.no_disponible ? "No disponible en toda la semana" : " ",
                           fecha_inicio = h.ndispo_fecha_inicio,
                           fecha_fin = h.ndispo_fecha_fin,
                           motivo = h.observaciones != null ? h.observaciones : " ",
                           nd = h.no_disponible,
                           d_lunes = h.lunes_disponible,
                           d_martes = h.martes_disponible,
                           d_miercoles = h.miercoles_disponible,
                           d_jueves = h.jueves_disponible,
                           d_viernes = h.viernes_disponible,
                           d_sabado = h.sabado_disponible,
                           d_domingo = h.domingo_disponible
                       };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarDisponibilidadHorario(int demo_id, string fecha)
        {
            if (fecha != "")
            {
                DateTime fec = DateTime.Parse(fecha);
                DayOfWeek dia = fec.DayOfWeek;
                string hour = Convert.ToString(fec.Hour + ":" + fec.Minute);
                TimeSpan hora = TimeSpan.Parse(hour);
                int data = 0;
                parametrizacion_horario horario = db.parametrizacion_horario.FirstOrDefault(x => x.demo_id == demo_id);

                if (horario.no_disponible)
                {
                    data = 0;
                }
                else
                {
                    if (dia == DayOfWeek.Monday)
                    {
                        if (horario.lunes_disponible)
                        {
                            if (horario.lunes_desde != null)
                            {
                                if (horario.lunes_desde < hora && horario.lunes_hasta > hora)
                                {
                                    data = 1;
                                }
                            }

                            if (horario.lunes_desde2 != null)
                            {
                                if (horario.lunes_desde2 < hora && horario.lunes_hasta2 > hora)
                                {
                                    data = 1;
                                }
                            }
                        }
                    }

                    if (dia == DayOfWeek.Tuesday)
                    {
                        if (horario.martes_disponible)
                        {
                            if (horario.martes_desde != null)
                            {
                                if (horario.martes_desde < hora && horario.martes_hasta > hora)
                                {
                                    data = 1;
                                }
                            }

                            if (horario.martes_desde2 != null)
                            {
                                if (horario.martes_desde2 < hora && horario.martes_hasta2 > hora)
                                {
                                    data = 1;
                                }
                            }
                        }
                    }

                    if (dia == DayOfWeek.Wednesday)
                    {
                        if (horario.miercoles_disponible)
                        {
                            if (horario.miercoles_desde != null)
                            {
                                if (horario.miercoles_desde < hora && horario.miercoles_hasta > hora)
                                {
                                    data = 1;
                                }
                            }

                            if (horario.miercoles_desde2 != null)
                            {
                                if (horario.miercoles_desde2 < hora && horario.miercoles_hasta2 > hora)
                                {
                                    data = 1;
                                }
                            }
                        }
                    }

                    if (dia == DayOfWeek.Thursday)
                    {
                        if (horario.jueves_disponible)
                        {
                            if (horario.jueves_desde != null)
                            {
                                if (horario.jueves_desde < hora && horario.jueves_hasta > hora)
                                {
                                    data = 1;
                                }
                            }

                            if (horario.jueves_desde2 != null)
                            {
                                if (horario.jueves_desde2 < hora && horario.jueves_hasta2 > hora)
                                {
                                    data = 1;
                                }
                            }
                        }
                    }

                    if (dia == DayOfWeek.Friday)
                    {
                        if (horario.viernes_disponible)
                        {
                            if (horario.viernes_desde != null)
                            {
                                if (horario.viernes_desde < hora && horario.viernes_hasta > hora)
                                {
                                    data = 1;
                                }
                            }

                            if (horario.viernes_desde2 != null)
                            {
                                if (horario.viernes_desde2 < hora && horario.viernes_hasta2 > hora)
                                {
                                    data = 1;
                                }
                            }
                        }
                    }

                    if (dia == DayOfWeek.Saturday)
                    {
                        if (horario.sabado_disponible)
                        {
                            if (horario.sabado_desde != null)
                            {
                                if (horario.sabado_desde < hora && horario.sabado_hasta > hora)
                                {
                                    data = 1;
                                }
                            }

                            if (horario.sabado_desde2 != null)
                            {
                                if (horario.sabado_desde2 < hora && horario.sabado_hasta2 > hora)
                                {
                                    data = 1;
                                }
                            }
                        }
                    }

                    if (dia == DayOfWeek.Sunday)
                    {
                        if (horario.domingo_disponible)
                        {
                            if (horario.domingo_desde != null)
                            {
                                if (horario.domingo_desde < hora && horario.domingo_hasta > hora)
                                {
                                    data = 1;
                                }
                            }

                            if (horario.domingo_desde2 != null)
                            {
                                if (horario.domingo_desde2 < hora && horario.domingo_hasta2 > hora)
                                {
                                    data = 1;
                                }
                            }
                        }
                    }
                }

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}