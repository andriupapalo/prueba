using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class InicioController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");
        public ActionResult inicio(int? menu)
        {
            if (Session["user_usuarioid"] == null)
            {
                return RedirectToAction("Login", "login");
            }
            else
            {
                int rol = Convert.ToInt32(Session["user_rolid"]);

                var Permiso = (from acceso in context.rolacceso
                               join RolPerm in context.rolpermisos on acceso.idpermiso equals RolPerm.id
                               where RolPerm.codigo == "P38" && acceso.idrol == rol
                               select RolPerm.id);

                ViewBag.Permiso = Permiso != null ? "Si" : "No";

                //calculo parametro de sistema rol alistamientof
                icb_sysparameter alista = context.icb_sysparameter.Where(d => d.syspar_cod == "P135").FirstOrDefault();
                int alistamiento = alista != null ? Convert.ToInt32(alista.syspar_value) : 9;
                var persona = Convert.ToInt32(Session["user_usuarioid"]);
                var usuariox = context.users.Where(d => d.user_id == persona).FirstOrDefault();
                if (usuariox == null)
                {
                    return RedirectToAction("Login", "login");

                }
                else
                {
                    if (usuariox.rols.dashboard_inicial != null && ((usuariox.rols.dashboard_rol.accion != "Inicio" && usuariox.rols.dashboard_rol.controlador != "Inicio") || (usuariox.rols.dashboard_rol.accion != "Inicio" && usuariox.rols.dashboard_rol.controlador == "Inicio")))
                    {
                        return RedirectToAction(usuariox.rols.dashboard_rol.accion,usuariox.rols.dashboard_rol.controlador);
                    }
                    else
                    {
                        ViewBag.totalVehiculos = context.icb_vehiculo.Count();
                        ViewBag.totalPedidos = context.detallePedido_GM.Count();

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

                        return View();
                    }
                }
                
               
            }
            
        }

        public ActionResult inicioAsesor()
        {
            int asesorLog = Convert.ToInt32(Session["user_usuarioid"]);
            int data = (from tareas in context.vw_tareasPendientes
                        where tareas.idusuarioasignado == asesorLog
                        select
                            tareas
                ).Count();

            ViewBag.cantidad = data;
            return View();
        }

        public ActionResult inicioBackoffice()
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);

            var listM = (from a in context.recibidotraslados
                         join b in context.encab_documento
                             on a.idtraslado equals b.idencabezado
                         join c in context.icb_referencia
                             on a.refcodigo equals c.ref_codigo
                         join d in context.bodega_concesionario
                             on a.idorigen equals d.id
                         join dd in context.bodega_concesionario
                             on a.iddestino equals dd.id
                         join e in context.users
                             on a.usertraslado equals e.user_id
                         where a.recibo_completo == false && a.tipo == "R" && b.requiere_mensajeria == true && b.mensajeria_atendido == false && a.idorigen == bodegaActual //|| a.iddestino == bodegaActual
                         select new
                         {
                             a.id,
                             b.numero,
                             a.refcodigo,
                             c.ref_descripcion,
                             a.cantidad,
                             a.recibido,
                             a.fechatraslado,
                             a.costo,
                             origen = d.bodccs_nombre,
                             destino = dd.bodccs_nombre,
                             cantidad_pendiente = a.recibido == true ? a.cant_recibida != null ? a.cantidad - a.cant_recibida : a.cantidad : a.cantidad,
                             esfaltante = a.recibido && a.recibo_completo == false ? 1 : 0,
                             destinatario = b.destinatario != null ? b.users2.user_nombre.ToString() + " " + b.users2.user_apellido.ToString() : "",
                         }).Count();

            //List<SelectListItem> lista = new List<SelectListItem>();
            //foreach (var item in listM)
            //{
            //    lista.Add(new SelectListItem
            //    {
            //        Text = item.nombre,
            //        Value = item.id.ToString()
            //    });
            //}
            ViewBag.Mensajeria = listM;

            return View();
        }

        public ActionResult inicioAlmacenista()
        {
            //--//
            return View();
        }

        public ActionResult inicioAsesorFlota()
        {
            return View();
        }

        public ActionResult inicioAnalistaRepuestos()
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);

            var listM = (from a in context.recibidotraslados
                         join b in context.encab_documento
                             on a.idtraslado equals b.idencabezado
                         join c in context.icb_referencia
                             on a.refcodigo equals c.ref_codigo
                         join d in context.bodega_concesionario
                             on a.idorigen equals d.id
                         join dd in context.bodega_concesionario
                             on a.iddestino equals dd.id
                         join e in context.users
                             on a.usertraslado equals e.user_id
                         where a.recibo_completo == false && a.tipo == "R" && b.requiere_mensajeria == true && b.mensajeria_atendido == false //&& a.idorigen == bodegaActual || a.iddestino == bodegaActual
                         select new
                         {
                             a.id,
                             b.numero,
                             a.refcodigo,
                             c.ref_descripcion,
                             a.cantidad,
                             a.recibido,
                             a.fechatraslado,
                             a.costo,
                             origen = d.bodccs_nombre,
                             destino = dd.bodccs_nombre,
                             cantidad_pendiente = a.recibido == true ? a.cant_recibida != null ? a.cantidad - a.cant_recibida : a.cantidad : a.cantidad,
                             esfaltante = a.recibido && a.recibo_completo == false ? 1 : 0,
                             destinatario = b.destinatario != null ? b.users2.user_nombre.ToString() + " " + b.users2.user_apellido.ToString() : "",
                         }).Count();


            ViewBag.Mensajeria = listM;

            ViewBag.CantSolicitada =  context.vw_solicitudes_traslados.Where(x => x.Id_bodega_origen == bodegaActual).GroupBy(x=>x.Id).Count();


    


         //  ViewBag.CantSolicitada = (from so in context.Solicitud_traslado select so).Count();

            ViewBag.CantAgendados = (from rt in context.recibidotraslados where rt.cant_recibida == null group rt by rt.id into recibido select recibido).Count();

            ViewBag.CantRecepcionados = (from rtt in context.recibidotraslados where rtt.cant_recibida != null select rtt).Count();

            var CantRecibidos = (from rtt in context.recibidotraslados where rtt.recibido==true && rtt.recibo_completo == true select rtt).ToList();
            ViewBag.CantRecibidos = CantRecibidos.Where(d => d.idorigen == bodegaActual || d.iddestino == bodegaActual).Count();

            var CantPendiente = (from rtt in context.recibidotraslados where rtt.recibido == false  select rtt).ToList();
            ViewBag.CantPendiente = CantPendiente.Where(d => d.idorigen == bodegaActual || d.iddestino == bodegaActual).Count();

            var CantIncompleto = (from rtt in context.recibidotraslados where rtt.recibido == true && rtt.cant_recibida != null && rtt.recibo_completo == false  select rtt).ToList();
            ViewBag.CantIncompleto = CantIncompleto.Where(d => d.idorigen == bodegaActual || d.iddestino == bodegaActual).Count();

            ViewBag.CantSolicompra = (from sol in context.rsolicitudesrepuestos
                                      where sol.estado_solicitud == 1
                                      select sol).Count();



            return View();
        }

        public ActionResult inicioCoordinadorR()
        {
            return View();
        }

        public ActionResult inicioAsesorRepuestos()
        {
            return View();
        }

        public ActionResult inicioCaja()
        {
            return View();
        }

        public ActionResult inicioCreditos()
        {
            return View();
        }

        private static Expression<Func<vw_agenda_tecnicos, string>> GetColumnName(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(vw_agenda_tecnicos), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<vw_agenda_tecnicos, string>> lambda = Expression.Lambda<Func<vw_agenda_tecnicos, string>>(menuProperty, menu);

            return lambda;
        }

        public ActionResult inicioTecnico()
        {
            var buscarEstados = (from estados in context.tcitasestados
                                 select new
                                 {
                                     estados.id,
                                     descripcion = "(" + estados.tipoestado + ") " + estados.Descripcion,
                                     estados.color_estado
                                 }).ToList();
            ViewBag.colorEstados = new SelectList(buscarEstados, "color_estado", "descripcion");
            return View();
        }

        public JsonResult BuscarCitasTecnico()
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);
            var buscarCitas = (from citas in context.tcitastaller
                               join tecnico in context.ttecnicos
                               on citas.tecnico equals tecnico.id
                               join usuario in context.users
                               on tecnico.idusuario equals usuario.user_id
                               join cliente in context.vw_terceros
                               on citas.cliente equals cliente.tercero_id
                               join ot in context.tencabezaorden
                               on citas.id equals ot.idcita into otcita
                               from ot in otcita.DefaultIfEmpty()
                               where usuario.user_id == usuarioActual && citas.estadocita != 11
                               select new
                               {
                                   citas.desde,
                                   citas.hasta,
                                   citas.id,
                                   citas.placa,
                                   citas.modelo,
                                   cliente.nombre_completo,
                                   numeroOT = ot.codigoentrada,
                                   estadoOT = citas.tcitasestados.Descripcion,
                                   color = citas.tcitasestados.color_estado,
                               }).ToList();
            var citasActuales = buscarCitas.Select(x => new
            {
                title = x.placa,
                modelo = x.modelo,
                cliente = !string.IsNullOrWhiteSpace(x.nombre_completo) ? x.nombre_completo : "No hay cliente",
                numeroOT = x.numeroOT != null ? x.numeroOT : "No Existe OT",
                estadoOT = x.estadoOT != null ? x.estadoOT : "No Existe OT",
                start = x.desde.ToString("MM/dd/yyyy") + " " + x.desde.Hour.ToString("00") + ":" + x.desde.Minute.ToString("00") + ":00",
                end = x.hasta.ToString("MM/dd/yyyy") + " " + x.hasta.Hour.ToString("00") + ":" + x.hasta.Minute.ToString("00") + ":00",
                id = x.id,
                color = x.color,
            });
            return Json(citasActuales, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarPaginados(string filtroGeneral, string fecha_desde, string fecha_hasta, int? tecnico1)
        {
            int tecnico = Convert.ToInt32(Session["user_usuarioid"]);
            if (Session["user_usuarioid"] != null)
            {
                //cantidad de registros a buscar
                string draw = Request.Form.GetValues("draw").FirstOrDefault();
                //indice desde el cual comienza a buscar(por ejemplo desde el 10° registro)
                string start = Request.Form.GetValues("start").FirstOrDefault();
                //cantidad de registros a parsear
                string length = Request.Form.GetValues("length").FirstOrDefault();
                //si hay un campo de busqueda en el datatable(no hay) busca
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

                int idusuario = Convert.ToInt32(Session["user_usuarioid"]);

                //por si necesito el rol del usuario
                //comentario para hacer diferencia
                icb_sysparameter admin1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P109").FirstOrDefault();
                int admin = admin1 != null ? Convert.ToInt32(admin1.syspar_value) : 1;

                Expression<Func<vw_agenda_tecnicos, bool>> predicado = PredicateBuilder.True<vw_agenda_tecnicos>();
                Expression<Func<vw_agenda_tecnicos, bool>> predicado2 = PredicateBuilder.False<vw_agenda_tecnicos>();
                Expression<Func<vw_agenda_tecnicos, bool>> predicado3 = PredicateBuilder.False<vw_agenda_tecnicos>();
                //comienzo a hacer mis verificaciones
                //valido si el tecnico está seleccionado (depende del rol va a ver lo suyo solamente o el de todos los tecnicos)
                if (tecnico1 != null)
                {
                    predicado = predicado.And(d => d.idtecnico == tecnico1);
                }


                if (!string.IsNullOrWhiteSpace(fecha_desde))
                {
                    //transformo la fecha y de ser válida hago la condicion and (osea uso predicado) con fecha >=a fechadesde
                    DateTime fecha = DateTime.Now;
                    bool convertir = DateTime.TryParse(fecha_desde, out fecha);
                    if (convertir)
                    {
                        predicado = predicado.And(d => d.desde >= fecha);
                    }
                }

                if (!string.IsNullOrWhiteSpace(fecha_hasta))
                {
                    //transformo la fecha y de ser válida hago la condicion and (osea uso predicado) con fecha <=a fechahasta
                    DateTime fecha = DateTime.Now;
                    bool convertir = DateTime.TryParse(fecha_hasta, out fecha);
                    if (convertir)
                    {
                        fecha = fecha.AddDays(1);
                        predicado = predicado.And(d => d.desde <= fecha);
                    }
                }

                if (!string.IsNullOrEmpty(filtroGeneral))
                {
                    predicado2 = predicado2.Or(d => 1 == 1 && d.placa.ToString().Contains(filtroGeneral));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.modelo.ToUpper().Contains(filtroGeneral.ToUpper()));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.nombre_completo.Contains(filtroGeneral.ToUpper()));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.codigoentrada.Contains(filtroGeneral.ToUpper()));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.Descripcion.Contains(filtroGeneral.ToUpper()));
                    predicado2 = predicado2.Or(d =>
                        1 == 1 && d.fechadesde2.ToUpper().Contains(filtroGeneral.ToUpper()));
                    predicado2 = predicado2.Or(d =>
                        1 == 1 && d.fechahasta2.ToUpper().Contains(filtroGeneral.ToUpper()));
                    predicado = predicado.And(predicado2);
                }

                //contamos el TOTAL de registros que da la consulta
                int registrostotales = context.vw_agenda_tecnicos.Where(predicado).Count();
                //si el ordenamiento es ascendente o descendente es distinto
                if (pageSize == -1)
                {
                    pageSize = registrostotales;
                }
                //creo un objeto de tipo lista para recibir las variables. Le pongo take 0 porque necesito que sea VACIO
                System.Collections.Generic.List<vw_agenda_tecnicos> query2 = context.vw_agenda_tecnicos.Where(predicado).Take(0).ToList();
                if (sortColumnDir == "asc")
                {
                    query2 = context.vw_agenda_tecnicos.Where(predicado).OrderBy(GetColumnName(sortColumn).Compile())
                        .Skip(skip).Take(pageSize).ToList();
                }
                else
                {
                    query2 = context.vw_agenda_tecnicos.Where(predicado)
                        .OrderByDescending(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
                }

                var query = query2.Select(d => new
                {
                    d.placa,
                    d.modelo,
                    nombre_completo = !string.IsNullOrWhiteSpace(d.nombre_completo)
                        ? d.nombre_completo
                        : "No hay cliente",
                    d.codigoentrada,
                    d.Descripcion,
                    sintomas = context.tsolicitudorden.Where(e => e.idorden == d.idorden).Select(e => new
                    {
                        idsintoma = e.idsolicitud,
                        descripcion = e.solicitud,
                        e.respuesta
                    }).ToList(),
                    operaciones = context.tdetallemanoobraot
                        .Where(e => e.idorden == d.idorden && !string.IsNullOrEmpty(e.estado)).Select(e => new
                        {
                            idoperacion = e.id,
                            descripcion = e.ttempario.operacion,
                            e.tiempo
                        }).ToList(),
                    repuestos = context.tdetallerepuestosot.Where(e => e.idorden == d.idorden).Select(e => new
                    {
                        idrepuesto = e.id,
                        codigo = e.idrepuesto,
                        descripcion = e.icb_referencia.ref_descripcion,
                        e.cantidad
                    }).ToList(),

                    d.fechadesde2,
                    d.fechahasta2,

                    d.id
                }).ToList();
                int contador = query.Count();
                return Json(
                    new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                    JsonRequestBehavior.AllowGet);
            }

            return Json(0);
        }

        public ActionResult inicioAnfitrionTaller()
        {
            ViewBag.id_estado_cita =
                new SelectList(context.tcitasestados.Where(x => x.id == 11 || x.id == 15).OrderBy(x => x.id), "id",
                    "Descripcion");
            ViewBag.motivoEstadoModal = new SelectList(context.tcitamotivocancela, "id", "Descripcion");
            var buscarEstados = (from estados in context.tcitasestados
                                 select new
                                 {
                                     estados.id,
                                     descripcion = "(" + estados.tipoestado + ") " + estados.Descripcion,
                                     estados.color_estado,
                                     estados.posicion
                                 }).OrderBy(x => x.posicion).ToList();
            ViewBag.colorEstados = new SelectList(buscarEstados, "color_estado", "descripcion");
            return View();
        }

        public JsonResult CambiarEstadosAsesor(int id)
        {
            int idUsuarioActual = Convert.ToInt32(Session["user_usuarioid"]);
            if (idUsuarioActual != 0)
            {
                sesion_logasesor lastConection = context.sesion_logasesor.OrderByDescending(x => x.fecha_inicia)
                    .FirstOrDefault(x => x.user_id == idUsuarioActual);
                if (lastConection != null)
                {
                    // Si el estado es ausente no cambio de posicion en la lista de asesores disponibles, por tanto no cambio la fecha de inicio
                    if (id == 1 && lastConection.estado != 3)
                    {
                        lastConection.fecha_inicia = DateTime.Now;
                        lastConection.fecha_termina = DateTime.Now;
                    }

                    if (id != 3 && lastConection.estado != 3)
                    {
                        if (lastConection.estado == 2 && lastConection.idprospecto != null)
                        {
                            //valido si el prospecto fue atendido
                            asignacion asigna = context.asignacion.OrderByDescending(d => d.id).Where(d => d.idProspecto == lastConection.idprospecto && d.estado == true).FirstOrDefault();
                            if (asigna != null)
                            {
                                asigna.fechaFin = DateTime.Now;
                                context.Entry(asigna).State = EntityState.Modified;
                            }

                        }
                        lastConection.fecha_inicia = DateTime.Now;
                        lastConection.fecha_termina = DateTime.Now;
                    }

                    if (id == 3 && lastConection.estado != 1)
                    {
                        lastConection.fecha_inicia = DateTime.Now;
                        lastConection.fecha_termina = DateTime.Now;
                    }

                    lastConection.estado = 4;
                    context.Entry(lastConection).State = EntityState.Modified;
                    //busco el estado de los ultimos prospectos atendidos
                    int bodega = Convert.ToInt32(Session["user_bodega"]);

                    int guardar = context.SaveChanges();
                    //crear nuevo estado de conexion
                    sesion_logasesor conexionnue = new sesion_logasesor
                    {
                        user_id = idUsuarioActual,
                        bodega = bodega,
                        estado = id,
                        fecha_inicia = DateTime.Now,
                        fecha_termina = DateTime.Now,
                    };
                    context.sesion_logasesor.Add(conexionnue);
                    context.SaveChanges();
                    if (guardar > 0)
                    {
                        return Json(id, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            return Json(new { id = -1 }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MantenerConexion()
        {
            int idUsuarioActual = Convert.ToInt32(Session["user_usuarioid"]);
            object usuarioAsesor = Session["user_rol"];
            if (idUsuarioActual != 0 && usuarioAsesor != null && usuarioAsesor.ToString() == "asesor")
            {
                sesion_logasesor lastConection = context.sesion_logasesor.OrderByDescending(x => x.fecha_inicia)
                    .FirstOrDefault(x => x.user_id == idUsuarioActual);
                if (lastConection != null)
                {
                    lastConection.fecha_termina = DateTime.Now;
                    context.Entry(lastConection).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarEstadosAsesor()
        {
            var buscarEstados = (from estados in context.estados_sesiones
                                 where estados.id != 5
                                 select new
                                 {
                                     estados.id,
                                     estados.descripcion
                                 }).ToList();
            int estadoActual = 0;
            int idUsuarioActual = Convert.ToInt32(Session["user_usuarioid"]);
            if (idUsuarioActual != 0)
            {
                sesion_logasesor lastConection = context.sesion_logasesor.OrderByDescending(x => x.id)
                    .FirstOrDefault(x => x.user_id == idUsuarioActual);
                if (lastConection != null)
                {
                    estadoActual = lastConection.estado;
                }
            }

            var data = new
            {
                buscarEstados,
                estadoActual
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLineasDocFinanciera(string mesAnio)
        {
            int mes = 0;
            int anio = 0;
            if (string.IsNullOrEmpty(mesAnio))
            {
                mes = DateTime.Now.Month;
                anio = DateTime.Now.Year;
            }
            else
            {
                mes = Convert.ToInt32(mesAnio.Substring(0, 2));
                anio = Convert.ToInt32(mesAnio.Substring(3, 4));
            }

            var buscaLineas = context.lineas_documento.Where(x => x.fec.Month == mes && x.fec.Year == anio).Select(x =>
                new
                {
                    x.valor_unitario,
                    x.porcentaje_iva,
                    x.impoconsumo
                });
            decimal valorBruto = 0;
            decimal valorIva = 0;
            decimal valorImpuestoConsumo = 0;

            foreach (var item in buscaLineas)
            {
                valorBruto += item.valor_unitario;
                valorIva += Convert.ToDecimal(item.porcentaje_iva, miCultura) * item.valor_unitario / 100;
                valorImpuestoConsumo += Convert.ToDecimal(item.impoconsumo, miCultura) * item.valor_unitario / 100;
            }

            DateTimeFormatInfo formatoFecha = CultureInfo.CurrentCulture.DateTimeFormat;
            var data = new
            {
                valorBruto = string.Format(CultureInfo.CreateSpecificCulture("el-GR"), "{0:0,0}", valorBruto),
                valorIva = string.Format(CultureInfo.CreateSpecificCulture("el-GR"), "{0:0,0}", valorIva),
                valorImpuestoConsumo = string.Format(CultureInfo.CreateSpecificCulture("el-GR"), "{0:0,0}",
                    valorImpuestoConsumo),
                valorTotal = string.Format(CultureInfo.CreateSpecificCulture("el-GR"), "{0:0,0}",
                    valorBruto + valorIva + valorImpuestoConsumo),
                nombreMes = formatoFecha.GetMonthName(mes),
                anio
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRepuestosPorClasificacion()
        {
            var buscarRepuestos = (from repuestos in context.icb_referencia
                                   group repuestos by new { repuestos.clasificacion_ABC }
                into clasificaciones
                                   select new
                                   {
                                       clasificacion = clasificaciones.Key.clasificacion_ABC,
                                       cantidad = clasificaciones.Count()
                                   }).ToList();
            return Json(buscarRepuestos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetVehiculosPorModelo()
        {
            var cantModelos = (from vehiculos in context.icb_vehiculo
                               join modelo in context.modelo_vehiculo
                                   on vehiculos.modvh_id equals modelo.modvh_codigo
                               group modelo by new { modelo.modvh_codigo, modelo.modvh_nombre }
                into modeloGrupo
                               select new
                               {
                                   modeloGrupo.Key.modvh_codigo,
                                   modeloGrupo.Key.modvh_nombre,
                                   modvh_cantidad = modeloGrupo.Count()
                               }).ToList();
            var listaPorModelo1 = cantModelos.Take(cantModelos.Count / 2).ToList();
            var listaPorModelo2 = cantModelos.Skip(cantModelos.Count / 2)
                .Take(cantModelos.Count - cantModelos.Count / 2).ToList();
            var data = new
            {
                listaPorModelo1,
                listaPorModelo2
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetVehiculosCotizados()
        {
            var modelos = (from cotizacion in context.icb_cotizacion
                               //join anioModelo in context.anio_modelo
                               //on cotizacion.id_anio_modelo equals anioModelo.anio_modelo_id
                               //join modelo in context.modelo_vehiculo
                               //on anioModelo.codigo_modelo equals modelo.modvh_codigo
                               //group modelo by new { modelo.modvh_nombre } into modeloGrupo
                           select new
                           {
                               //descripcion = modeloGrupo.Key.modvh_nombre,
                               //cantidad = modeloGrupo.Count()
                           }).ToList();
            return Json(modelos, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProspectosPorMes()
        {
            DateTimeFormatInfo formatoFecha = CultureInfo.CurrentCulture.DateTimeFormat;
            var datos = (from prospecto in context.icb_terceros
                         group prospecto by new { prospecto.tercerofec_creacion.Month }
                into grupo
                         select new
                         {
                             mes = grupo.Key.Month,
                             cantidad = grupo.Count()
                         }).ToList();

            string[] meses =
            {
                "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre",
                "Noviembre", "Diciembre"
            };
            int[] prospectos = new int[12];
            int[] cotizaciones = new int[12];

            foreach (var item1 in datos)
            {
                prospectos[item1.mes - 1] = item1.cantidad;
            }

            var datosCotizaciones = (from cotizacion in context.icb_cotizacion
                                     group cotizacion by new { cotizacion.cot_feccreacion.Month }
                into grupo
                                     select new
                                     {
                                         mes = grupo.Key.Month,
                                         cantidad = grupo.Count()
                                     }).ToList();

            foreach (var item1 in datosCotizaciones)
            {
                cotizaciones[item1.mes - 1] = item1.cantidad;
            }

            var datosPorMes = new
            {
                meses,
                prospectos,
                cotizaciones
            };
            return Json(datosPorMes, JsonRequestBehavior.AllowGet);
        }

        public JsonResult NotificacionesLlegada()
        {
            List<ListaNotificaciones> lista = new List<ListaNotificaciones>();
            int asesor_id = Convert.ToInt32(Session["user_usuarioid"]);
            var buscarNotificaciones = (from n in context.notificacion_llegadacliente
                                        join tc in context.icb_terceros
                                        on n.cliente_id equals tc.tercero_id
                                        join vh in context.icb_vehiculo
                                        on n.planmayor equals vh.plan_mayor
                                        where n.asesor_id == asesor_id && n.leido==false
                                        select new 
                                        {
                                            leido = n.leido!=null?n.leido.Value:false,
                                            id = n.id,
                                            nombre = tc.prinom_tercero + " " + tc.segnom_tercero + " " + tc.apellido_tercero + " " + tc.segapellido_tercero,
                                            placa = vh.plac_vh,
                                        }).OrderBy(d => d.leido).Take(6);

            var buscarNotificacionesProspecto = (from n in context.notificacion_asesor_prospecto
                                        join tc in context.prospectos
                                        on n.prospecto_id equals tc.id

                                        where n.asesor_id == asesor_id && n.leido == false
                                        select new
                                        {
                                            leido = n.leido != null ? n.leido.Value : false,
                                            id = n.id,
                                            nombre = tc.prinom_tercero + " " + tc.segnom_tercero + " " + tc.apellido_tercero + " " + tc.segapellido_tercero,
                                        }).OrderBy(d => d.leido).Take(6);
           
            foreach (var item in buscarNotificaciones)
            {
                lista.Add(new ListaNotificaciones
                {
                    id=item.id,
                    leido=item.leido,
                    nombre=item.nombre,
                    placa=item.placa,
                });
            }

            foreach (var item in buscarNotificacionesProspecto)
            {
                lista.Add(new ListaNotificaciones
                {
                    id = item.id,
                    leido = item.leido,
                    nombre = item.nombre,
                    placa = "",
                });
            }
            return Json(lista, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult NotificacionesLeidas(int id)
        {
            notificacion_llegadacliente notificacion = context.notificacion_llegadacliente.Find(id);
            if (notificacion != null)
            {
                notificacion.leido = true;
                context.Entry(notificacion).State = EntityState.Modified;
            }
          
            notificacion_asesor_prospecto notificaciones = context.notificacion_asesor_prospecto.Find(id);
            if (notificaciones != null)
            {
                notificaciones.leido = true;
                context.Entry(notificaciones).State = EntityState.Modified;
            }
            if(notificacion!=null || notificaciones != null)
            {
                context.SaveChanges();
            }

            return Redirect("../agenda_asesor/Index");
        }

                                 

       




        public JsonResult CumpleanosClientes()
        {
            configuracion_envio_correos correoconfig = context.configuracion_envio_correos.Where(d => d.activo).FirstOrDefault();

            int asesor_id = Convert.ToInt32(Session["user_usuarioid"]);

            var data = (from t in context.icb_terceros
                        join c in context.tercero_cliente
                            on t.tercero_id equals c.tercero_id
                        where t.asesor_id == asesor_id
                              && t.fec_nacimiento.Value.Day == DateTime.Now.Day
                              && t.fec_nacimiento.Value.Month == DateTime.Now.Month
                        select new
                        {
                            nombre = t.prinom_tercero + " " + t.segnom_tercero + " " + t.apellido_tercero + " " +
                                     t.segapellido_tercero,
                            telefono = t.telf_tercero + " - " + t.celular_tercero,
                            t.tercero_id,
                            correo = t.email_tercero != null ? t.email_tercero : " "
                        }).ToList();

            if (data.Count() > 0 && Convert.ToInt32(Session["user_rolid"]) == 4)
            {
                try
                {
                    enviocorreo_cumpleanos correo_enviado = context.enviocorreo_cumpleanos.FirstOrDefault(x =>
                        x.asesor_id == asesor_id && x.fecha_envio.Value.Day == DateTime.Now.Day
                                                 && x.fecha_envio.Value.Month == DateTime.Now.Month);

                    if (correo_enviado == null)
                    {
                        users asesor = context.users.Find(asesor_id);
                        string nombre_asesor = asesor.user_nombre + " " + asesor.user_apellido;

                        MailAddress de = new MailAddress(correoconfig.correo, correoconfig.nombre_remitente);
                        MailAddress para = new MailAddress(asesor.user_email, nombre_asesor);
                        MailMessage mensaje = new MailMessage(de, para)
                        {
                            //mensaje.Bcc.Add("liliana.avila@exiware.com");
                            //mensaje.Bcc.Add("marley.vargas@exiware.com");
                            Subject = "Cumpleaños clientes ",
                            BodyEncoding = Encoding.Default
                        };
                        mensaje.ReplyToList.Add(new MailAddress(asesor.user_email, nombre_asesor));
                        mensaje.IsBodyHtml = true;
                        string html = "";
                        html += "<h2>Hola <b>" + nombre_asesor + "</b>:</h2><br />";
                        html += "<p>Hoy estan cumpliendo años tus clientes, felicitalos en su día.</p><br />";
                        html += "<table style='text-align: center; border: 1px solid #3498db; width: 100%'>";
                        html += "<thead style='line-height: 30px; background-color: #3498db; color: white'>";
                        html += "<tr>";
                        html += "<th style='text-align: center; border: 1px solid #3498db;'>NOMBRE</th>";
                        html += "<th style='text-align: center; border: 1px solid #3498db;'>TELEFONO</th>";
                        html += "<th style='text-align: center; border: 1px solid #3498db;'>CORREO</th>";
                        html += "</tr>";
                        html += "</thead>";
                        html += "<tbody style='line-height: 23px'>";

                        foreach (var item in data)
                        {
                            html += "<tr>";
                            html += "<td style='text-align: center; border: 1px solid #3498db;'>" + item.nombre +
                                    "</td>";
                            html += "<td style='text-align: center; border: 1px solid #3498db;'>" + item.telefono +
                                    "</td>";
                            html += "<td style='text-align: center; border: 1px solid #3498db;'>" + item.correo +
                                    "</td>";
                            html += "</tr>";
                        }

                        html += "</tbody>";
                        html += "</table>";

                        mensaje.Body = html;

                        SmtpClient cliente = new SmtpClient(correoconfig.smtp_server)
                        {
                            Port = correoconfig.puerto,
                            UseDefaultCredentials = false,
                            Credentials = new NetworkCredential(correoconfig.usuario, correoconfig.password),
                            EnableSsl = true
                        };

                        cliente.Send(mensaje);

                        enviocorreo_cumpleanos envio = new enviocorreo_cumpleanos
                        {
                            asesor_id = asesor_id,
                            asunto = "Notificación cumpleaños clientes",
                            fecha_envio = DateTime.Now,
                            enviado = true
                        };
                        context.enviocorreo_cumpleanos.Add(envio);
                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    enviocorreo_cumpleanos envio = new enviocorreo_cumpleanos
                    {
                        asesor_id = asesor_id,
                        asunto = "Notificación cumpleaños clientes",
                        fecha_envio = DateTime.Now,
                        enviado = false,
                        razon_noenvio = ex.Message
                    };
                    context.enviocorreo_cumpleanos.Add(envio);
                    context.SaveChanges();
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarFavoritosUsuarioActual()
        {
            int usuarioActual = Convert.ToInt32(Session["user_usuarioid"]);

            var buscarFavoritosMasVistos = (from favoritos in context.favoritos
                                            join menu in context.Menus
                                                on favoritos.idmenu equals menu.idMenu
                                            where favoritos.idusuario == usuarioActual
                                            select new
                                            {
                                                favoritos.seleccionado,
                                                favoritos.cantidad,
                                                menu.nombreMenu,
                                                menu.url,
                                                menu.idMenu
                                            }).Take(2).ToList();

            var buscarFavoritosSeleccionados = (from favoritos in context.favoritos
                                                join menu in context.Menus
                                                    on favoritos.idmenu equals menu.idMenu
                                                where favoritos.idusuario == usuarioActual && favoritos.seleccionado
                                                select new
                                                {
                                                    favoritos.seleccionado,
                                                    favoritos.cantidad,
                                                    menu.nombreMenu,
                                                    menu.url,
                                                    menu.idMenu
                                                }).ToList();

            foreach (var favoritos in buscarFavoritosMasVistos)
            {
                bool existe = false;
                var favoritoAgregar = favoritos;
                foreach (var favoritos2 in buscarFavoritosSeleccionados)
                {
                    if (favoritos.nombreMenu == favoritos2.nombreMenu)
                    {
                        existe = true;
                        break;
                    }
                }

                if (!existe)
                {
                    buscarFavoritosSeleccionados.Add(favoritoAgregar);
                }
            }

            return Json(buscarFavoritosSeleccionados, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult buscarAccesos() {
        //    var rolLogueado = Convert.ToInt32(Session["user_rolid"]);

        //    var data = (from o in context.rolopcionAcceso
        //                where o.idRol == rolLogueado
        //                select new {
        //                    o.id,
        //                    o.idopcionAcceso
        //                }).ToList();

        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult buscarAccesos()
        {
            int rolLogueado = Convert.ToInt32(Session["user_rolid"]);

            var data = (from o in context.opcion_acceso_rol
                        where o.id_rol == rolLogueado
                        select new
                        {
                            o.id_rol,
                            o.opcion_acceso.codigo,
                            o.id_opcion_acceso
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarAperturaMes(string url)
        {
            int anoActual = DateTime.Now.Year;
            int mesActual = DateTime.Now.Month;
            meses_cierre mesesCierre = context.meses_cierre.FirstOrDefault(x => x.ano == anoActual && x.mes == mesActual);

            if (mesesCierre != null)
            {
                string quitarUrl = Request.Url.Scheme + "://" + Request.Url.Authority;
                int contarLetras = quitarUrl.Length;

                string input = url.Remove(0, contarLetras + 1);
                int index = input.IndexOf("?");
                if (index > 0)
                {
                    input = input.Substring(0, index);
                }

                //var urlActual = Request.Url.LocalPath;
                int paginas = context.bloqueomescerrado.Where(d => input.Contains(d.url)).Count();

                if (paginas > 0)
                {
                    bloqueomescerrado nivelBloqueo = context.bloqueomescerrado.FirstOrDefault(d => input.Contains(d.url));
                    if (nivelBloqueo.protegido == 1)
                    {
                        return Json(new { bloqueado = true, restringido = false }, JsonRequestBehavior.AllowGet);
                    }

                    if (nivelBloqueo.protegido == 2)
                    {
                        return Json(new { bloqueado = false, restringido = true }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { bloqueado = false, restringido = false }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { bloqueado = false, restringido = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AgregarQuitarFavorito(int? id_menu)
        {
            favoritos buscarMenu = context.favoritos.FirstOrDefault(x => x.idmenu == id_menu);
            if (buscarMenu != null)
            {
                if (buscarMenu.seleccionado == false)
                {
                    buscarMenu.seleccionado = true;
                }
                else
                {
                    buscarMenu.seleccionado = false;
                }

                context.Entry(buscarMenu).State = EntityState.Modified;
                int guardar = context.SaveChanges();
                if (guardar > 0)
                {
                    if (buscarMenu.seleccionado)
                    {
                        return Json(new { esFavorito = true }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { esFavorito = false }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { esFavorito = false }, JsonRequestBehavior.AllowGet);
        }
    }
}