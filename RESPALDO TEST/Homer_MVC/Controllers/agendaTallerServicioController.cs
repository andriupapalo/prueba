using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
    {
    public class agendaTallerServicioController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");
        CultureInfo mCultura = new CultureInfo("en-US");
        // GET: agendaTallerServicioo
        public ActionResult Create(int? menu, string plan_mayor, int? acce)
        {
            if (Session["user_usuarioid"] != null)
            {
                //razon de ingreso accesorios
                icb_sysparameter acce1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P116").FirstOrDefault();
                int accesori = acce1 != null ? Convert.ToInt32(acce1.syspar_value) : 6;
                //tipo de cita accesorios
                icb_sysparameter acce2 = context.icb_sysparameter.Where(d => d.syspar_cod == "P117").FirstOrDefault();
                int accesori2 = acce2 != null ? Convert.ToInt32(acce2.syspar_value) : 5;
                //tipo de operacion accesorios
                icb_sysparameter acce3 = context.icb_sysparameter.Where(d => d.syspar_cod == "P118").FirstOrDefault();
                int accesori3 = acce3 != null ? Convert.ToInt32(acce3.syspar_value) : 4;

                ViewBag.bodega = new SelectList(context.bodega_concesionario.Where(x => x.es_taller), "id",
                    "bodccs_nombre");
                ViewBag.accesorio = 0;
                //if (acce == 1)
                //{
                //	var bodegaactual = Convert.ToInt32(Session["user_bodega"]);
                //	ViewBag.bodega =
                //		new SelectList(context.bodega_concesionario.Where(x => x.es_taller && x.id == bodegaactual),
                //			"id", "bodccs_nombre", bodegaactual);
                //	ViewBag.accesorio = 1;
                //}

                if (acce == 1)
                {
                    ViewBag.averia = 1;
                }
                else
                {
                    ViewBag.averia = 0;
                }

                ViewBag.plan_mayor = "";
                if (!string.IsNullOrWhiteSpace(plan_mayor))
                {
                    ViewBag.plan_mayor = plan_mayor;
                }

                List<ttipobahia> areas = context.ttipobahia.ToList();
                //if (acce != 1)
                //{
                areas.Add(new ttipobahia { id = 0, descripcion = "Todas" });
                ViewBag.cbAreas = new SelectList(areas.OrderBy(x => x.id), "id", "descripcion");
                /*}
				else
				{
					areas = context.ttipobahia.Where(d => d.descripcion.ToUpper() == "ACCESORIOS").ToList();
					ViewBag.cbAreas = new SelectList(areas.OrderBy(x => x.id), "id", "descripcion",
						areas.Select(d => d.id).First());
				}*/

                ViewBag.operacionModal = new SelectList(context.ttempario, "codigo", "operacionhr");
                if (acce == 1)
                {
                    ViewBag.operacionModal = new SelectList(context.ttempario.Where(d => d.tipooperacion == accesori3),
                        "codigo", "operacionhr");
                }

                ViewBag.fecha_agenda = DateTime.Now.ToString("yyyy/MM/dd", new CultureInfo("en-US"));
                var buscarEstados = (from estados in context.tcitasestados
                                     select new
                                     {
                                         estados.id,
                                         descripcion = "(" + estados.tipoestado + ") " + estados.Descripcion,
                                         estados.color_estado,
                                         estados.posicion
                                     }).OrderBy(x => x.posicion).ToList();
                ViewBag.vehiculoModal = new SelectList(context.modelo_vehiculo, "modvh_codigo", "modvh_nombre");
                ViewBag.colorEstados = new SelectList(buscarEstados, "color_estado", "descripcion");
                ViewBag.estadoModal = new SelectList(buscarEstados, "id", "descripcion");
                ViewBag.motivoEstadoModal = new SelectList(context.tcitamotivocancela, "id", "Descripcion");

                ViewBag.tipoCitaModal = new SelectList(context.ttipocita, "id", "descripcion");
                //if (acce == 1)
                //	ViewBag.tipoCitaModal = new SelectList(context.ttipocita.Where(d => d.id == accesori2), "id",
                //		"descripcion", accesori2);
                ViewBag.fuenteModal = new SelectList(context.tcitamotivoentrada, "id", "descripcion");
                icb_sysparameter revisio = context.icb_sysparameter.Where(d => d.syspar_cod == "P158").FirstOrDefault();
                int revisionper = revisio != null ? Convert.ToInt32(revisio.syspar_value) : 10;

                ViewBag.planModal = new SelectList(context.ttempario.Where(x=>x.tipooperacion== revisionper && x.idplanm!=null).Select(x=>new { x.codigo, Descripcion = x.operacion}), "codigo", "Descripcion");
                ViewBag.tipoTarifa = new SelectList(context.ttipostarifa, "id", "Descripcion");
                ViewBag.razonIngresoModal = new SelectList(context.trazonesingreso, "id", "razoz_ingreso");

                if (acce == 1)
                {
                    ViewBag.razonIngresoModal =
                        new SelectList(
                            context.trazonesingreso.Where(d => d.id == accesori)
                                .Select(d => new { d.id, d.razoz_ingreso }).ToList(), "id", "razoz_ingreso", accesori);
                }

                var buscarAgenteTaller = (from usuario in context.users
                                          where usuario.rol_id == 1015
                                          select new
                                          {
                                              usuario.user_id,
                                              nombres = usuario.user_nombre + " " + usuario.user_apellido
                                          }).ToList();
                //ViewBag.agenteModal = new SelectList(buscarAgenteTaller, "user_id", "nombres");

                var centroCosotos = (from cc in context.centro_costo
                                     where cc.centcst_estado
                                     orderby cc.pre_centcst
                                     select new
                                     {
                                         value = cc.centcst_id,
                                         text = "(" + cc.pre_centcst + ") " + cc.centcst_nombre
                                     }).ToList();

                ViewBag.centroCostoModal = new SelectList(centroCosotos, "value", "text");

                //var buscarFavoritos = context.favoritos.FirstOrDefault(x=>x.idmenu == menu);
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

            return RedirectToAction("Login", "Login");
        }

        public JsonResult validarEventos(string tipoCita, string placa)
        {
            string parametro = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P67").syspar_value;
            if (parametro == tipoCita)
            {
                var data = (from v in context.vw_pendientesEntrega
                            where v.planmayor == placa
                            select new
                            {
                                v.FechaPedido,
                                v.FechaVenta,
                                v.FechaTramite,
                                v.FechaMatricula,
                                v.numerosoat,
                                v.FechaManifiesto
                            }).FirstOrDefault();

                if (data != null)
                {
                    if (data.FechaPedido != null && data.FechaVenta != null && data.FechaTramite != null &&
                        data.FechaMatricula != null && data.numerosoat != null && data.FechaManifiesto != null)
                    {
                        return Json(new { ok = true, permitido = true }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { ok = true, permitido = false }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { ok = false, permitido = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { ok = false, permitido = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult validarObligatorios(string tipoCita)
        {
            string parametro = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P67").syspar_value;

            string parametro2 = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P117").syspar_value;

            if (parametro == tipoCita || parametro2 == tipoCita)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarOperacionPorPlan(string id_modelo, string codigo, int? bodega)
        {
            int? buscarModeloGeneral = (from modelo in context.modelo_vehiculo
                                        where modelo.modvh_codigo == id_modelo
                                        select modelo.modelogkit).FirstOrDefault();

            //calculo de tarifas del taller
            var tarifastaller = context.ttarifastaller.Where(d => d.bodega == bodega && d.tipotarifa == 1).FirstOrDefault();
            var ope= context.ttempario.Where(d => d.codigo == codigo).FirstOrDefault();
            var id_plan =ope.idplanm.Value;
            var buscarOperaciones2 = (from plan in context.tplanrepuestosmodelo
                                      join tempario in context.ttempario
                                          on plan.tempario equals tempario.codigo into ps
                                      from tempario in ps.DefaultIfEmpty()
                                     
                                      where plan.inspeccion == id_plan && plan.modgeneral == buscarModeloGeneral && plan.tipo == "M" &&
                                            tempario != null && plan.listaprecios!="N/A"
                                      select new
                                          {
                                          tempario.codigo,
                                          tempario.operacion,
                                          tiempo = plan.cantidad != null ? plan.cantidad : 0,
                                          tipooperacion=plan.listaprecios,
                                          preciototalhora=tarifastaller.total_tarifa,
                                          }).ToList();
            var buscarOperaciones = new List<detalleoperacionesPlan>();

            foreach (var item in buscarOperaciones2)
            {
                decimal preciocliente = 0;
                decimal iva = 0;
                decimal iva2 = 0;
                var convertir = Decimal.TryParse(tarifastaller.iva, NumberStyles.Number, new CultureInfo("en-US"), out iva);

                if (item.tipooperacion == "AC")
                {
                    preciocliente = (tarifastaller.total_tarifa??0) * (item.tiempo??0);
                    iva2 = (tarifastaller.valorhora.Value * item.tiempo.Value) * iva;
                }
                buscarOperaciones.Add(new detalleoperacionesPlan
                {
                    codigo=item.codigo,
                    iva= iva2.ToString("N2", new CultureInfo("is-IS")),
                    operacion=item.operacion,
                    porcentajeiva=iva,
                    preciomatriz=item.tipooperacion=="AC"?tarifastaller.valorhora!=null?tarifastaller.valorhora.Value:0:0,
                    preciomatriz2= tarifastaller.valorhora!=null?tarifastaller.valorhora.Value.ToString("N2",new CultureInfo("is-IS")):"0",
                    preciototal=preciocliente,
                   tiempo=item.tiempo??0,
                });
            }


            float tiempo = 0;
            decimal tiempo3 = 0;
            decimal costomanoobraa = 0;
            if (id_plan != null)
                {
                    var tempa = context.ttempario.Where(x => x.codigo == codigo).Select(s => s.HoraCliente).FirstOrDefault();
                    tiempo = float.Parse(tempa,new CultureInfo("en-US"));
                    var tiempo2 = Decimal.TryParse(tempa, NumberStyles.Number, new CultureInfo("en-US"), out tiempo3);
                    if (ope.aplica_costo == true)
                    {
                        costomanoobraa = tiempo3 * tarifastaller.total_tarifa ?? 0;

                    }
                }
            else {
                tiempo = float.Parse(buscarOperaciones.Sum(d => d.tiempo).ToString());
                }

            decimal preciooperaciones = buscarOperaciones.Sum(d =>
               Convert.ToDecimal(d.preciototal, miCultura));

            var buscarReferenciasSQL = (from plan in context.tplanrepuestosmodelo
                                        join referencia in context.icb_referencia
                                            on plan.referencia equals referencia.ref_codigo into ps
                                        from referencia in ps.DefaultIfEmpty()
                                        join precios in context.rprecios
                                            on referencia.ref_codigo equals precios.codigo into ps2
                                        from precios in ps2.DefaultIfEmpty()
                                        where plan.inspeccion == id_plan && plan.modgeneral == buscarModeloGeneral && plan.tipo == "R" &&
                                              plan.referencia != null && plan.listaprecios!="N/A"
                                        select new
                                        {
                                            referencia.ref_codigo,
                                            referencia.ref_descripcion,
                                            tiporeferencia=plan.listaprecios,
                                          //  precio = plan.listaprecios.Contains("precio1") ? precios != null ? precios.precio1 : 0 :
                                                //plan.listaprecios.Contains("precio2") ? precios != null ? precios.precio1 : 0 :
                                                //plan.listaprecios.Contains("precio3") ? precios != null ? precios.precio1 : 0 :
                                                //plan.listaprecios.Contains("precio4") ? precios != null ? precios.precio1 : 0 :
                                                //plan.listaprecios.Contains("precio5") ? precios != null ? precios.precio1 : 0 :
                                                //plan.listaprecios.Contains("precio6") ? precios != null ? precios.precio1 : 0 :
                                                //plan.listaprecios.Contains("precio7") ? precios != null ? precios.precio1 : 0 :
                                                //plan.listaprecios.Contains("precio8") ? precios != null ? precios.precio1 : 0 :
                                                //plan.listaprecios.Contains("precio9") ? precios != null ? precios.precio1 : 0 : 0,

                                            precio = plan.listaprecios=="AC"?context.icb_referencia.Where(d => d.ref_codigo == referencia.ref_codigo).Select(d => d.precio_venta).FirstOrDefault():0,
                                            referencia.por_iva,                                            
                                            cantidad = plan.cantidad != null ? plan.cantidad : 0
                                        }).ToList();

            var buscarReferencias = buscarReferenciasSQL.Select(x => new
            {
                x.ref_codigo,
                x.ref_descripcion,
                x.tiporeferencia,
                precio=x.precio,
                x.por_iva,
                x.cantidad,
                valorIva = Math.Round(x.precio * (decimal)x.por_iva / 100),
                valorTotal = Math.Round((x.precio + x.precio * (decimal)x.por_iva / 100) * x.cantidad ?? 0)
            });


            decimal preciorepuestos = buscarReferencias.Sum(d => d.valorTotal);
            //falta el cálculo de la mano de obra y de los insumos del plan en cuestión
            
            var insumos = context.insumosplanmantenimiento.Where(d => d.modelogeneral == buscarModeloGeneral && d.idplan == id_plan).FirstOrDefault();
            decimal costoinsumos = 0;
            if (insumos != null)
            {
                costoinsumos = costomanoobraa * insumos.porcentaje;
            }
            string valortotal = (preciorepuestos+ preciooperaciones+ costomanoobraa+costoinsumos).ToString();

            return Json(new { buscarOperaciones, buscarReferencias, tiempo, valortotal }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarFiltro(int? id_bodega, DateTime? fecha, int? id_area)
        {
            
            List<int> listaBahiasFiltro = new List<int>();
            if (id_area == null || id_area == 0)
            {
                listaBahiasFiltro = context.ttipobahia.Select(x => x.id).ToList();
            }
            else
            {
                listaBahiasFiltro.Add(id_area ?? 0);
            }

            DateTime fechaBuscar = fecha ?? DateTime.Now;

            icb_sysparameter buscarRolTecnico = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P48");
            int idRolTecnico = buscarRolTecnico != null ? Convert.ToInt32(buscarRolTecnico.syspar_value) : 0;

            #region Consulta de los tecnicos de la bodega y del area selecionada


            var query = context.vw_bahiaxtecnico.Where(x => x.bodega == id_bodega && listaBahiasFiltro.Contains(x.tipo_bahia)).ToList();


            var buscarTecnicos = query.Select(x => new {
                x.id,
                x.bodega,
                x.codigo_bahia,
                x.idtecnico,
                x.descripcion,
                 x.user_id ,
                x.usuario
                }).ToList();
                
              

            #endregion

            #region Consulta de las bahias y las citas

            var buscarBahias = query.Select(x =>
                                new
                                {
                                    x.id,
                                    x.codigo_bahia,
                                    x.idtecnico,
                                    x.descripcion,
                                    x.user_id,
                                   x. usuario ,
                                    citas = (from citas in context.tcitastaller
                                             join estado in context.tcitasestados
                                                 on citas.estadocita equals estado.id
                                             join tercero in context.icb_terceros
                                                 on citas.cliente equals tercero.tercero_id
                                             where citas.bahia == x.id && citas.desde.Year == fechaBuscar.Year &&
                                                   citas.desde.Month == fechaBuscar.Month && citas.desde.Day == fechaBuscar.Day &&
                                                   citas.estadocita != 11
                                             select new
                                             {
                                                 citas.id,
                                                 citas.tecnico,
                                                 citas.desde,
                                                 citas.hasta,
                                                 citas.placa,
                                                 cliente = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                           tercero.apellido_tercero + " " + tercero.segapellido_tercero + " " +
                                                           tercero.razon_social,
                                                 idEstado = estado.id,
                                                 estado.color_estado,
                                                 estado.tipoestado
                                             }).ToList()
                                }).ToList();

            #endregion

            List<string> listaTitulos = new List<string>();
            List<FilaAgentaTallerModel> listaDatos = new List<FilaAgentaTallerModel>();
            List<FilaAgentaTallerModel> listaPrimeraFila = new List<FilaAgentaTallerModel>();
            FilaAgentaTallerModel Fila = new FilaAgentaTallerModel();
            FilaAgentaTallerModel listaTitulos2 = new FilaAgentaTallerModel();

            #region asigno los encabezados de la tabla (nombre del tecnigo y bahia)

            listaTitulos2.ListaTitulosAgenda.Add(new CeldaAgentaTallerModel { NombreCelda = "Hora / Puesto" });
            if (buscarTecnicos != null)
            {
                foreach (var item in buscarTecnicos)
                {
                    listaTitulos2.ListaTitulosAgenda.Add(new CeldaAgentaTallerModel
                    { NombreCelda = item.codigo_bahia + "<br>" + item.descripcion + "<br>" + item.usuario });
                }

                listaPrimeraFila.Add(listaTitulos2);
            }

            #endregion

            #region asigno los tiempos y los intervalos que tiene la bodega

            var buscarTiempos = (from bodega in context.bodega_concesionario
                                 where bodega.id == id_bodega
                                 select new
                                 {
                                     bodega.hora_inicial,
                                     bodega.hora_final,
                                     bodega.lapso_tiempo
                                 }).FirstOrDefault();

            if (buscarTiempos != null)
            {
                TimeSpan horaActual = buscarTiempos.hora_inicial ?? new TimeSpan();
                while (horaActual < buscarTiempos.hora_final)
                {
                    listaTitulos.Add(horaActual.ToString());
                    horaActual = horaActual.Add(new TimeSpan(0, buscarTiempos.lapso_tiempo ?? 0, 0));
                }

                if (horaActual > buscarTiempos.hora_final)
                {
                    listaTitulos.Add(buscarTiempos.hora_final.ToString());
                }
            }

            #endregion

            DateTime inicioCita = DateTime.Now;
            DateTime finCita = DateTime.Now;

            foreach (string item in listaTitulos)
            {
                Fila.ListaCeldas.Add(new CeldaAgentaTallerModel { HoraInicio = item, NombreCelda = item, hora = true });
                foreach (var bahias in buscarBahias)
                {
                    parametrizacion_horario buscarHorarioPerito =
                        context.parametrizacion_horario.FirstOrDefault(x => x.usuario_id == bahias.user_id);

                    TimeSpan horaInicioMedioDia = TimeSpan.Zero, horaFinMedioDia = TimeSpan.Zero,
                        horaInicioDiaTecnico = TimeSpan.Zero, horaFinDiaTecnico = TimeSpan.Zero, horaInicioNovedad = TimeSpan.Zero,
                        horaFinNovedad = TimeSpan.Zero, horaInicioDescanso = TimeSpan.Zero, horaFinDescanso = TimeSpan.Zero;
                    vhorarionovedad buscarNovedadPerito = new vhorarionovedad();
                    if (buscarHorarioPerito !=null)
                        {

                      
                     horaInicioMedioDia = validarInicioMedioDia(buscarHorarioPerito, fechaBuscar);
                     horaFinMedioDia = validarFinalMedioDia(buscarHorarioPerito, fechaBuscar);
                     horaInicioDiaTecnico = InicioDiaTecnico(buscarHorarioPerito, fechaBuscar);
                     horaFinDiaTecnico = FinDiaTecnico(buscarHorarioPerito, fechaBuscar);

                                       
                     buscarNovedadPerito = context.vhorarionovedad.FirstOrDefault(x =>
                        x.horarioid == buscarHorarioPerito.horario_id && x.fechaini.Year == fechaBuscar.Year &&
                        x.fechaini.Month == fechaBuscar.Month && x.fechaini.Day == fechaBuscar.Day);
                   
                     horaInicioNovedad = validarInicioNovedad(buscarNovedadPerito, fechaBuscar);
                     horaFinNovedad = validarFinalNovedad(buscarNovedadPerito, fechaBuscar);
                    

                    ttecnicos buscarDescansoPerito = context.ttecnicos.FirstOrDefault(x => x.idusuario == bahias.user_id);
                     horaInicioDescanso = validarInicioDescanso(buscarDescansoPerito, fechaBuscar);
                     horaFinDescanso = validarFinalDescanso(buscarDescansoPerito, fechaBuscar);

                        }

                    // Se valida que en la hora exista alguna cita
                    bool horaOcupada = /*""*/ false;
                    int calculoSegmentos = 0;
                    bool horaMedioDia = false;
                    bool horaInicioTecnico = false;
                    bool horaFinTecnico = false;
                    bool horanovedad = false;
                    bool horaDescanso = false;
                    string mensajeHora = "";
                    int? id_cita = null;
                    int idTecnico = 0;
                    string colorCita = "";
                    int horas = Convert.ToInt32(item.Substring(0, 2));
                    int minutos = Convert.ToInt32(item.Substring(3, 2));
                    idTecnico = Convert.ToInt32(bahias.idtecnico);


                    if (buscarHorarioPerito != null)
                        {

                        foreach (var cita in bahias.citas)
                            {
                            inicioCita = cita.desde;
                            finCita = cita.hasta;
                            idTecnico = cita.tecnico;
                            horaOcupada = /*horaConCita*/ExisteCitaEnElHorario(new TimeSpan(horas, minutos, 0),
                                cita.desde.TimeOfDay, cita.hasta.TimeOfDay);
                            string colorEstado =
                                (cita.idEstado == 1 || cita.idEstado == 2 || cita.idEstado == 3) &&
                                cita.desde < DateTime.Now.AddMinutes(-1)
                                    ? "#ddbadb"
                                    : cita.color_estado;
                            if (horaOcupada /*!="libre" || horaOcupada!=""*/)
                                {
                                mensajeHora = cita.placa + "<br>" + cita.cliente + "<br>(" + cita.tipoestado + ")";
                                id_cita = cita.id;
                                colorCita = colorEstado;
                                break;
                                }
                            }

                        // Se valida la supuesta hora de almuerzo de medio dia que es libre
                        horaMedioDia = ExisteCitaEnElHorario(new TimeSpan(horas, minutos, 0), horaInicioMedioDia,
                            horaFinMedioDia);
                        horaInicioTecnico = ExisteCitaEnElHorario(new TimeSpan(horas, minutos, 0), new TimeSpan(),
                            horaInicioDiaTecnico);
                        horaFinTecnico = ExisteCitaEnElHorario(new TimeSpan(horas, minutos, 0), horaFinDiaTecnico,
                            new TimeSpan(23, 59, 59));

                        horanovedad = ExisteCitaEnElHorario(new TimeSpan(horas, minutos, 0), horaInicioNovedad,
                            horaFinNovedad);
                        horaDescanso = ExisteCitaEnElHorario(new TimeSpan(horas, minutos, 0), horaInicioDescanso,
                            horaFinDescanso);

                        string fechaActual = fecha != null
                            ? fecha.Value.ToString("yyyy/MM/dd")
                            : DateTime.Now.ToString("yyyy/MM/dd");
                        DateTime hora = Convert.ToDateTime(fechaActual + " " + item);
                        if (horaInicioTecnico || horaFinTecnico)
                            {
                            if (horaFinTecnico)
                                {
                                string hft = horaFinDiaTecnico.ToString();
                                string hfb = buscarTiempos.hora_final.ToString();
                                DateTime concat1 = Convert.ToDateTime(fechaActual + " " + hft); //Fin tecnivo
                                DateTime concat2 = Convert.ToDateTime(fechaActual + " " + hfb); //fin bodega
                                if (concat1 == hora && bahias.idtecnico == idTecnico)
                                    {
                                    calculoSegmentos =
                                        calcularSegmentos(concat1, concat2, buscarTiempos.lapso_tiempo ?? 30);
                                    Fila.ListaCeldas.Add(new CeldaAgentaTallerModel
                                        {
                                        HoraInicio = item,
                                        IdBahia = bahias.id,
                                        NombreCelda = "",
                                        CeldaInactiva = true,
                                        valRowSpan = calculoSegmentos
                                        });
                                    }
                                }
                            else
                                {
                                Fila.ListaCeldas.Add(new CeldaAgentaTallerModel
                                    { HoraInicio = item, IdBahia = bahias.id, NombreCelda = "", CeldaInactiva = true });
                                }
                            }
                        else
                            {
                            if (horanovedad)
                                {
                                string hin = horaInicioNovedad.ToString();
                                string hfn = horaFinNovedad.ToString();
                                DateTime concat1 = Convert.ToDateTime(fechaActual + " " + hin); //inicio novedad
                                DateTime concat2 = Convert.ToDateTime(fechaActual + " " + hfn); //fin novedad
                                if (concat1 == hora && bahias.idtecnico == idTecnico)
                                    {
                                    calculoSegmentos =
                                        calcularSegmentos(concat1, concat2, buscarTiempos.lapso_tiempo ?? 30);
                                    Fila.ListaCeldas.Add(new CeldaAgentaTallerModel
                                        {
                                        HoraInicio = item,
                                        IdBahia = bahias.id,
                                        NombreCelda = "Novedad: " + buscarNovedadPerito.motivo,
                                        CeldaInactiva = true,
                                        valRowSpan = calculoSegmentos
                                        });
                                    }
                                }
                            else if (horaMedioDia)
                                {
                                string hia = horaInicioMedioDia.ToString();
                                string hfa = horaFinMedioDia.ToString();
                                DateTime concat2 = Convert.ToDateTime(fechaActual + " " + hia); //inicio del almuerzo
                                DateTime concat3 = Convert.ToDateTime(fechaActual + " " + hfa); //fin del almuerzo
                                if (concat2 == hora && bahias.idtecnico == idTecnico)
                                    {
                                    calculoSegmentos =
                                        calcularSegmentos(concat2, concat3, buscarTiempos.lapso_tiempo ?? 30);
                                    Fila.ListaCeldas.Add(new CeldaAgentaTallerModel
                                        {
                                        HoraInicio = item,
                                        IdBahia = bahias.id,
                                        NombreCelda = "<i class='fa fa-coffee'>Almuerzo</i>",
                                        CeldaInactiva = true,
                                        valRowSpan = calculoSegmentos
                                        });
                                    }
                                }
                            else if (horaDescanso)
                                {
                                string hid = horaInicioDescanso.ToString();
                                string hfd = horaFinDescanso.ToString();
                                DateTime concatI = Convert.ToDateTime(fechaActual + " " + hid);
                                DateTime concatF = Convert.ToDateTime(fechaActual + " " + hfd);
                                if (concatI == hora && bahias.idtecnico == idTecnico)
                                    {
                                    calculoSegmentos =
                                        calcularSegmentos(concatI, concatF, buscarTiempos.lapso_tiempo ?? 30);
                                    Fila.ListaCeldas.Add(new CeldaAgentaTallerModel
                                        {
                                        HoraInicio = item,
                                        IdBahia = bahias.id,
                                        NombreCelda = "Descanso",
                                        CeldaInactiva = true,
                                        valRowSpan = calculoSegmentos
                                        });
                                    }
                                }
                            else if (horaOcupada /*=="inicio"*/)
                                {
                                string hid = horaInicioDescanso.ToString();
                                string hfd = horaFinDescanso.ToString();
                                string hia = horaInicioMedioDia.ToString();
                                string hfa = horaFinMedioDia.ToString();
                                string hft = horaFinDiaTecnico.ToString();
                                string hin = horaInicioNovedad.ToString();
                                string hfn = horaFinNovedad.ToString();
                                DateTime concat0 = Convert.ToDateTime(fechaActual + " " + hid); //inicio descanso
                                DateTime concat1 = Convert.ToDateTime(fechaActual + " " + hfd); //fin del descanso
                                DateTime concat2 = Convert.ToDateTime(fechaActual + " " + hia); //inicio del almuerzo
                                DateTime concat3 = Convert.ToDateTime(fechaActual + " " + hfa); //fin del almuerzo
                                DateTime concat4 = Convert.ToDateTime(fechaActual + " " + hft); //fin del tecnico
                                DateTime concat5 = Convert.ToDateTime(fechaActual + " " + hin); //inicio novedad
                                DateTime concat6 = Convert.ToDateTime(fechaActual + " " + hfn); //fin novedad

                                #region Si la cita inicia antes del break y finaliza antes del almuerzo

                                if (inicioCita < concat0 && finCita > concat1 && finCita <= concat2)
                                    {
                                    //primer Segmento
                                    if (inicioCita == hora && bahias.idtecnico == idTecnico)
                                        {
                                        int segmentos = calcularSegmentos(inicioCita, concat0,
                                            buscarTiempos.lapso_tiempo ?? 30);
                                        Fila.ListaCeldas.Add(new CeldaAgentaTallerModel
                                            {
                                            HoraInicio = item,
                                            IdBahia = bahias.id,
                                            NombreCelda = mensajeHora,
                                            CeldaInactiva = false,
                                            ColorCelda = colorCita,
                                            IdCita = id_cita,
                                            valRowSpan = segmentos
                                            });
                                        }

                                    //Segundo Segmento
                                    if (concat1 == hora && bahias.idtecnico == idTecnico)
                                        {
                                        int segmentos2 = calcularSegmentos(concat1, finCita,
                                            buscarTiempos.lapso_tiempo ?? 30);
                                        Fila.ListaCeldas.Add(new CeldaAgentaTallerModel
                                            {
                                            HoraInicio = item,
                                            IdBahia = bahias.id,
                                            NombreCelda = "",
                                            CeldaInactiva = false,
                                            ColorCelda = colorCita,
                                            IdCita = id_cita,
                                            valRowSpan = segmentos2
                                            });
                                        }
                                    }

                                #endregion

                                #region Si la cita inicia antes del break y finaliza despues del almuerzo

                                if (inicioCita < concat0 && finCita >= concat3)
                                    {
                                    //primer Segmento
                                    if (inicioCita == hora && bahias.idtecnico == idTecnico)
                                        {
                                        int segmentos = calcularSegmentos(inicioCita, concat0,
                                            buscarTiempos.lapso_tiempo ?? 30);
                                        Fila.ListaCeldas.Add(new CeldaAgentaTallerModel
                                            {
                                            HoraInicio = item,
                                            IdBahia = bahias.id,
                                            NombreCelda = mensajeHora,
                                            CeldaInactiva = false,
                                            ColorCelda = colorCita,
                                            IdCita = id_cita,
                                            valRowSpan = segmentos
                                            });
                                        }

                                    //Segundo Segmento
                                    if (concat1 == hora && bahias.idtecnico == idTecnico)
                                        {
                                        int segmentos2 = calcularSegmentos(concat1, concat2,
                                            buscarTiempos.lapso_tiempo ?? 30);
                                        Fila.ListaCeldas.Add(new CeldaAgentaTallerModel
                                            {
                                            HoraInicio = item,
                                            IdBahia = bahias.id,
                                            NombreCelda = "",
                                            CeldaInactiva = false,
                                            ColorCelda = colorCita,
                                            IdCita = id_cita,
                                            valRowSpan = segmentos2
                                            });
                                        }

                                    //Tercer Segmento
                                    if (concat3 == hora && bahias.idtecnico == idTecnico)
                                        {
                                        int segmentos3 = calcularSegmentos(concat3, finCita,
                                            buscarTiempos.lapso_tiempo ?? 30);
                                        Fila.ListaCeldas.Add(new CeldaAgentaTallerModel
                                            {
                                            HoraInicio = item,
                                            IdBahia = bahias.id,
                                            NombreCelda = "",
                                            CeldaInactiva = false,
                                            ColorCelda = colorCita,
                                            IdCita = id_cita,
                                            valRowSpan = segmentos3
                                            });
                                        }
                                    }

                                #endregion

                                #region Si la cita inicia despues del break y finaliza despues del almuerzo

                                if (inicioCita >= concat1 && inicioCita < concat2 && finCita >= concat3)
                                    {
                                    int segmentos = calcularSegmentos(inicioCita, concat2,
                                        buscarTiempos.lapso_tiempo ?? 30);
                                    int segmentos2 = calcularSegmentos(concat3, finCita, buscarTiempos.lapso_tiempo ?? 30);
                                    //primer Segmento
                                    if (inicioCita == hora && bahias.idtecnico == idTecnico)
                                        {
                                        Fila.ListaCeldas.Add(new CeldaAgentaTallerModel
                                            {
                                            HoraInicio = item,
                                            IdBahia = bahias.id,
                                            NombreCelda = mensajeHora,
                                            CeldaInactiva = false,
                                            ColorCelda = colorCita,
                                            IdCita = id_cita,
                                            valRowSpan = segmentos
                                            });
                                        }
                                    //Segundo Segmento
                                    if (concat3 == hora && bahias.idtecnico == idTecnico)
                                        {
                                        Fila.ListaCeldas.Add(new CeldaAgentaTallerModel
                                            {
                                            HoraInicio = item,
                                            IdBahia = bahias.id,
                                            NombreCelda = "",
                                            CeldaInactiva = false,
                                            ColorCelda = colorCita,
                                            IdCita = id_cita,
                                            valRowSpan = segmentos2
                                            });
                                        }
                                    }

                                #endregion

                                #region ***COMENTADO*** Si la cita inicia antes de una novedad y finaliza despues de esta

                                //if (TimeSpan.Compare(horaInicioNovedad, new TimeSpan()) != 0 && TimeSpan.Compare(horaFinNovedad, new TimeSpan()) != 0)
                                //{
                                //	if (inicioCita < concat5 && finCita >= concat6)
                                //	{
                                //		var segmentos = calcularSegmentos(inicioCita, concat5, buscarTiempos.lapso_tiempo ?? 30);
                                //		var segmentos2 = calcularSegmentos(concat6, finCita, buscarTiempos.lapso_tiempo ?? 30);
                                //		//primer Segmento
                                //		if (inicioCita == hora && bahias.idtecnico == idTecnico)
                                //		{
                                //			Fila.ListaCeldas.Add(new CeldaAgentaTallerModel() { HoraInicio = item, IdBahia = bahias.id, NombreCelda = mensajeHora, CeldaInactiva = false, ColorCelda = colorCita, IdCita = id_cita, valRowSpan = segmentos });
                                //		}
                                //		//Segundo Segmento
                                //		if (concat6 == hora && bahias.idtecnico == idTecnico)
                                //		{
                                //			Fila.ListaCeldas.Add(new CeldaAgentaTallerModel() { HoraInicio = item, IdBahia = bahias.id, NombreCelda = "", CeldaInactiva = false, ColorCelda = colorCita, IdCita = id_cita, valRowSpan = segmentos2 });
                                //		}
                                //	}
                                //}							

                                #endregion

                                #region Si la cita no se cruza con nada

                                else if (inicioCita <= concat0 && finCita <= concat0 ||
                                         inicioCita >= concat1 && finCita <= concat2 ||
                                         inicioCita >= concat3 && finCita <= concat4)
                                    {
                                    if (inicioCita == hora && bahias.idtecnico == idTecnico)
                                        {
                                        calculoSegmentos = calcularSegmentos(inicioCita, finCita,
                                            buscarTiempos.lapso_tiempo ?? 30);
                                        Fila.ListaCeldas.Add(new CeldaAgentaTallerModel
                                            {
                                            HoraInicio = item,
                                            IdBahia = bahias.id,
                                            NombreCelda = mensajeHora,
                                            CeldaInactiva = false,
                                            ColorCelda = colorCita,
                                            IdCita = id_cita,
                                            valRowSpan = calculoSegmentos
                                            });
                                        }
                                    }

                                #endregion
                                }
                            else
                                {
                                if (!horaOcupada /* == "libre" || horaOcupada == ""*/)
                                    {
                                    Fila.ListaCeldas.Add(new CeldaAgentaTallerModel
                                        {
                                        HoraInicio = item,
                                        IdBahia = bahias.id,
                                        NombreCelda = "",
                                        CeldaInactiva = false,
                                        ColorCelda = "#"
                                        });
                                    }
                                }
                            }
                        }
                    else {
                            {
                            Fila.ListaCeldas.Add(new CeldaAgentaTallerModel
                                { HoraInicio = item, IdBahia = bahias.id, NombreCelda = "", CeldaInactiva = false });
                            }
                        }




                }
            }

            listaDatos.Add(Fila);
            return Json(new { listaDatos, listaTitulos, listaPrimeraFila }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarAveriasSolicitadas(string planmayor)
        {
            long tallerOrden = context.tallerAveria.Where(x => x.nombre.Contains("orden")).FirstOrDefault().id;
            long estadoAveria = context.estadoAverias.Where(x => x.nombre.Contains("asignada")).FirstOrDefault().id;
            var buscar = context.icb_inspeccionvehiculos.Where(x => x.planmayor == planmayor && x.insp_solicitar == true && x.estado_averia_id == estadoAveria && x.idcitataller == null && x.taller_averia_id == tallerOrden)
                .Select(x => new
                {
                    x.insp_id,
                    x.planmayor,
                    fechaevento = x.insp_fechains,
                    evento_observacion = x.insp_observacion,
                    estadoA = x.estado_averia_id != null ? x.estadoAverias.nombre : "",
                    taller = x.taller_averia_id != null ? x.tallerAveria.nombre : "",
                    solicitar = x.insp_solicitar,
                    averia = x.insp_idaveria != 0 ? x.icb_averias.ave_descripcion : "",
                    impacto = x.insp_impacto != null ? x.icb_impacto_averia.impacto_descripcion : "",
                    zona = x.insp_zona != null ? x.icb_zona_averia.zona_descripcion : "",
                }).OrderBy(x => x.fechaevento).ToList();

            var data = buscar.Select(x => new
            {
                x.insp_id,
                x.planmayor,
                fecha = x.fechaevento.Value.ToString("yyyy/MM/dd HH:mm"),
                observacion = x.evento_observacion,
                x.estadoA,
                x.taller,
                x.solicitar,
                x.averia,
                x.impacto,
                x.zona
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public int calcularSegmentos(DateTime hi, DateTime hf, int? intervalo)
        {
            TimeSpan diferencia = hf - hi;

            int duracion = diferencia.Hours * 60;
            int minutos = diferencia.Minutes;
            duracion += minutos;

            int numero_segmentos = duracion / intervalo ?? 30;
            return numero_segmentos;
        }

        public JsonResult BuscarFiltroSemanal(int? id_bodega, DateTime? fecha, int? id_area)
        {
            List<int> listaBahiasFiltro = new List<int>();
            if (id_area == null || id_area == 0)
            {
                listaBahiasFiltro = context.ttipobahia.Select(x => x.id).ToList();
            }
            else
            {
                listaBahiasFiltro.Add(id_area ?? 0);
            }

            DateTime fechaBuscar = fecha ?? DateTime.Now;

            int diaDeLaSemana = (int)fechaBuscar.DayOfWeek - 1;
            diaDeLaSemana = diaDeLaSemana >= 0 ? diaDeLaSemana : 6;
            DateTime fechaLunes = fechaBuscar.AddDays(-diaDeLaSemana);
            DateTime fechaDomingo = fechaBuscar.AddDays(6);

            var buscarBahias = (from bahia in context.tbahias
                                join tipoBahia in context.ttipobahia
                                    on bahia.tipo_bahia equals tipoBahia.id
                                join tecnico in context.ttecnicos
                                    on bahia.idtecnico equals tecnico.id
                                join usuario in context.users
                                    on tecnico.idusuario equals usuario.user_id
                                where bahia.bodega == id_bodega && listaBahiasFiltro.Contains(bahia.tipo_bahia)
                                select new
                                {
                                    bahia.id,
                                    bahia.codigo_bahia,
                                    tipoBahia.descripcion,
                                    usuario.user_id,
                                    usuario = usuario.user_nombre + " " + usuario.user_apellido,
                                    citas = (from citas in context.tcitastaller
                                             join tercero in context.icb_terceros
                                                 on citas.cliente equals tercero.tercero_id
                                             join estados in context.tcitasestados
                                                 on citas.estadocita equals estados.id
                                             where citas.bahia == bahia.id &&
                                                   DbFunctions.TruncateTime(citas.desde) >= DbFunctions.TruncateTime(fechaLunes) &&
                                                   DbFunctions.TruncateTime(citas.desde) <= DbFunctions.TruncateTime(fechaDomingo) &&
                                                   citas.estadocita != 11
                                             select new
                                             {
                                                 citas.id,
                                                 citas.desde,
                                                 citas.hasta,
                                                 citas.placa,
                                                 citas.tcitasestados.tipoestado,
                                                 colorhx=estados.color_estado,
                                                 cliente = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                           tercero.apellido_tercero + " " + tercero.segapellido_tercero + " " +
                                                           tercero.razon_social
                                             }).OrderBy(x => x.desde).ToList()
                                }).OrderBy(x => x.codigo_bahia).ToList();

            string[] diasEnLetras = { "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado", "Domingo" };
            List<string> listaTitulos = new List<string>();
            List<FilaAgentaTallerModel> listaDatos = new List<FilaAgentaTallerModel>();
            listaTitulos.Add("Puesto / Dia");
            for (int dia = 0; dia <= 6; dia++)
            {
                listaTitulos.Add(string.Format("{0:00}", fechaLunes.AddDays(dia).Day) + "/" +
                                 string.Format("{0:00}", fechaLunes.AddDays(dia).Month) + "/" +
                                 string.Format("{0:0000}", fechaLunes.AddDays(dia).Year) + "<br>" + diasEnLetras[dia]);
            }

            foreach (var bahias in buscarBahias)
            {
                FilaAgentaTallerModel Fila = new FilaAgentaTallerModel();
                Fila.ListaCeldas.Add(new CeldaAgentaTallerModel
                { NombreCelda = bahias.codigo_bahia + "<br>" + bahias.descripcion + "<br>" + bahias.usuario });

                //var buscarHorarioPerito = context.parametrizacion_horario.FirstOrDefault(x => x.usuario_id == bahias.user_id);
                //var horaInicioMedioDia = validarInicioMedioDia(buscarHorarioPerito, fecha);
                //var horaFinMedioDia = validarFinalMedioDia(buscarHorarioPerito, fecha);

                for (int i = 1; i < listaTitulos.Count; i++)
                {
                    // Se valida que en la hora exista alguna cita
                    //var horaOcupada = false;
                    //var horaMedioDia = false;
                    string mensajeDia = "";
                    string hxcolor = "";
                    DateTime? fechaCelda = null;
                    int dia = Convert.ToInt32(listaTitulos[i].Substring(0, 2));
                    int mes = Convert.ToInt32(listaTitulos[i].Substring(3, 2));
                    int anio = Convert.ToInt32(listaTitulos[i].Substring(6, 4));
                    foreach (var cita in bahias.citas)
                    {
                        if (cita.desde.Day == dia && cita.desde.Month == mes && cita.desde.Year == anio)
                        {
                            mensajeDia += cita.desde.ToShortTimeString() + " - " + cita.hasta.ToShortTimeString() +
                                          "<br>" + cita.placa.ToUpper() + "<br>" + cita.cliente.ToUpper() + "<br>(" + cita.tipoestado + ")" + "<br><br>";

                            hxcolor = cita.colorhx;
                            fechaCelda = cita.desde;
                        }
                    }

                    Fila.ListaCeldas.Add(new CeldaAgentaTallerModel
                    {
                        HoraInicio = listaTitulos[i],
                        IdBahia = bahias.id,
                        NombreCelda = mensajeDia,
                        CeldaInactiva = false,
                        ColorCelda = hxcolor//"#"
                    });
                }

                listaDatos.Add(Fila);
            }

            return Json(new { listaDatos, listaTitulos }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarFiltroMensual(int? id_bodega, DateTime? fecha, int? id_area)
        {
            List<int> listaBahiasFiltro = new List<int>();
            if (id_area == null || id_area == 0)
            {
                listaBahiasFiltro = context.ttipobahia.Select(x => x.id).ToList();
            }
            else
            {
                listaBahiasFiltro.Add(id_area ?? 0);
            }

            DateTime fechaBuscar = fecha ?? DateTime.Now;

            int diasDelMes = CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(fechaBuscar.Year, fechaBuscar.Month);
            DateTime fechaInicio = new DateTime(fechaBuscar.Year, fechaBuscar.Month, 1);
            DateTime fechaFin = new DateTime(fechaBuscar.Year, fechaBuscar.Month, diasDelMes);

            var buscarBahias = (from bahia in context.tbahias
                                join tipoBahia in context.ttipobahia
                                    on bahia.tipo_bahia equals tipoBahia.id
                                join tecnico in context.ttecnicos
                                    on bahia.idtecnico equals tecnico.id
                                join usuario in context.users
                                    on tecnico.idusuario equals usuario.user_id
                                where bahia.bodega == id_bodega && listaBahiasFiltro.Contains(bahia.tipo_bahia)
                                select new
                                {
                                    bahia.id,
                                    bahia.codigo_bahia,
                                    tipoBahia.descripcion,
                                    usuario.user_id,
                                    usuario = usuario.user_nombre + " " + usuario.user_apellido,
                                    citas = (from citas in context.tcitastaller
                                             join tercero in context.icb_terceros
                                                 on citas.cliente equals tercero.tercero_id
                                             join estados in context.tcitasestados
                                                on citas.estadocita equals estados.id
                                             where citas.bahia == bahia.id &&
                                                   DbFunctions.TruncateTime(citas.desde) >= DbFunctions.TruncateTime(fechaInicio) &&
                                                   DbFunctions.TruncateTime(citas.desde) <= DbFunctions.TruncateTime(fechaFin) &&
                                                   citas.estadocita != 11
                                             select new
                                             {
                                                 citas.id,
                                                 citas.desde,
                                                 citas.hasta,
                                                 citas.placa,
                                                 citas.tcitasestados.tipoestado,
                                                 colorhx=estados.color_estado,
                                                 cliente = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                           tercero.apellido_tercero + " " + tercero.segapellido_tercero + " " +
                                                           tercero.razon_social
                                             }).OrderBy(x => x.desde).ToList()
                                }).OrderBy(x => x.codigo_bahia).ToList();

            string[] diasEnLetras = { "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado", "Domingo" };
            List<string> listaTitulos = new List<string>();
            List<FilaAgentaTallerModel> listaDatos = new List<FilaAgentaTallerModel>();
            listaTitulos.Add("Puesto / Mes");
            int primerDia = (int)fechaInicio.DayOfWeek - 1;
            primerDia = primerDia >= 0 ? primerDia : 6;
            for (int dia = 0; dia < diasDelMes; dia++)
            {
                listaTitulos.Add(string.Format("{0:00}", fechaInicio.AddDays(dia).Day) + "/" +
                                 string.Format("{0:00}", fechaInicio.AddDays(dia).Month) + "/" +
                                 string.Format("{0:0000}", fechaInicio.AddDays(dia).Year) + "<br>" +
                                 diasEnLetras[primerDia]);
                if (primerDia == 6)
                {
                    primerDia = 0;
                }
                else
                {
                    primerDia++;
                }
            }

            foreach (var bahias in buscarBahias)
            {
                FilaAgentaTallerModel Fila = new FilaAgentaTallerModel();
                Fila.ListaCeldas.Add(new CeldaAgentaTallerModel
                { NombreCelda = bahias.codigo_bahia + "<br>" + bahias.descripcion + "<br>" + bahias.usuario });

                //var buscarHorarioPerito = context.parametrizacion_horario.FirstOrDefault(x => x.usuario_id == bahias.user_id);
                //var horaInicioMedioDia = validarInicioMedioDia(buscarHorarioPerito, fecha);
                //var horaFinMedioDia = validarFinalMedioDia(buscarHorarioPerito, fecha);

                for (int i = 1; i < listaTitulos.Count; i++)
                {
                    // Se valida que en la hora exista alguna cita
                    //var horaOcupada = false;
                    //var horaMedioDia = false;
                    string mensajeDia = "";
                    int cantidadCitas = 0;
                    string hxcolor = "";
                    DateTime? fechaCelda = null;
                    int dia = Convert.ToInt32(listaTitulos[i].Substring(0, 2));
                    int mes = Convert.ToInt32(listaTitulos[i].Substring(3, 2));
                    int anio = Convert.ToInt32(listaTitulos[i].Substring(6, 4));
                    foreach (var cita in bahias.citas)
                    {
                        if (cita.desde.Day == dia && cita.desde.Month == mes && cita.desde.Year == anio)
                        {
                            cantidadCitas++;
                            mensajeDia += cita.desde.ToShortTimeString() + " - " + cita.hasta.ToShortTimeString() +
                                          "<br>" + cita.placa.ToUpper() + "<br>" + cita.cliente.ToUpper() + "<br>";
                            hxcolor = cita.colorhx;
                            fechaCelda = cita.desde;

                        }
                    }

                    string cantidades = cantidadCitas != 0 ? cantidadCitas.ToString() : "";
                    Fila.ListaCeldas.Add(new CeldaAgentaTallerModel
                    {
                        HoraInicio = listaTitulos[i],
                        IdBahia = bahias.id,
                        NombreCelda = mensajeDia,
                        CeldaInactiva = false,
                        ColorCelda = hxcolor,
                    });
                }

                listaDatos.Add(Fila);
            }

            return Json(new { listaDatos, listaTitulos }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ValidarSiExisteCita(int? id_cita, string plan_mayor, int? accesorio)
        {
            DateTime fechaHoy = DateTime.Now;
            var buscarEstadoProgramado = (from parametro in context.icb_sysparameter
                                          where parametro.syspar_cod == "P50"
                                          select new
                                          {
                                              parametro.syspar_value
                                          }).FirstOrDefault();
            List<bodega_concesionario> talleres = context.bodega_concesionario.Where(x => x.es_taller == true && x.es_repuestos == true && x.bodccs_estado == true).ToList();
            var listataller = talleres.Select(d => new
            {
                d.id,
                nombre = d.bodccs_nombre
            });
            int idEstado = buscarEstadoProgramado != null ? Convert.ToInt32(buscarEstadoProgramado.syspar_value) : 0;
            if (Session["user_usuarioid"] == null)
            {
                return Json(new { encontrado = false, estadoCita = idEstado }, JsonRequestBehavior.AllowGet);
            }

            if (id_cita == null)
            {
                int aseor = Convert.ToInt32(Session["user_usuarioid"]);
                users ase = context.users.Where(d => d.user_id == aseor).FirstOrDefault();
                string nombreasesor = "";
                if (ase != null)
                {
                    nombreasesor = ase.user_nombre + " " + ase.user_apellido;
                }

                int tieneaccesorios = 0;

                if (accesorio != null && !string.IsNullOrWhiteSpace(plan_mayor))
                {
                    //busco si plan mayor viene de un pedido
                    vpedido pedido = context.vpedido.OrderByDescending(d => d.fec_creacion)
                        .Where(d => d.planmayor == plan_mayor && d.anulado == false).FirstOrDefault();
                    if (pedido != null)
                    {
                        //busco la lista de accesorios que vienen del pedido
                        List<vpedrepuestos> accesorios = context.vpedrepuestos
                            .Where(d => d.pedido_id == pedido.id && d.instalado == false).ToList();
                        var listaaccesorios = accesorios.Select(d => new
                        {
                            pedido = d.id,
                            id = d.referencia,
                            nombre = d.icb_referencia.ref_descripcion,
                            d.cantidad,
                            valorcifra = d.vrunitario != null ? d.vrunitario.Value : 0,
                            valor_uni = d.vrunitario != null
                                ? d.vrunitario.Value.ToString("N0", new CultureInfo("is-IS"))
                                : "0",
                            totalcifra = d.vrtotal != null ? d.vrtotal.Value : 0,
                            valor_total = d.vrtotal != null
                                ? d.vrtotal.Value.ToString("N0", new CultureInfo("is-IS"))
                                : "0"
                        });

                        tieneaccesorios = 1;
                        return Json(
                            new
                            {
                                encontrado = false,
                                estadoCita = idEstado,
                                nombreasesor,
                                tieneaccesorios,
                                listaaccesorios,
                                listataller
                            }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { encontrado = false, estadoCita = idEstado, nombreasesor, tieneaccesorios, listataller },
                        JsonRequestBehavior.AllowGet);
                }

                return Json(new { encontrado = false, estadoCita = idEstado, nombreasesor, tieneaccesorios, listataller },
                    JsonRequestBehavior.AllowGet);
            }


            var buscarCliente = (from cita in context.tcitastaller
                                 join bahia in context.tbahias
                                     on cita.bahia equals bahia.id
                                 join bodega in context.bodega_concesionario
                                     on bahia.bodega equals bodega.id
                                 join tercero in context.icb_terceros
                                     on cita.cliente equals tercero.tercero_id
                                 join cliente in context.tercero_cliente
                                     on tercero.tercero_id equals cliente.tercero_id
                                 where cita.id == id_cita
                                 select new
                                 {
                                     cliente.cltercero_id,
                                     bodega.id,
                                     cita.tipotarifa
                                 }).FirstOrDefault();
            int id_cliente = buscarCliente != null ? buscarCliente.cltercero_id : 0;
            int id_bodega = buscarCliente != null ? buscarCliente.id : 0;
            int id_tarifa = buscarCliente != null ? buscarCliente.tipotarifa : 0;
            //ttarifaclientebodega buscarTarifaClienteBodega = (from tarifaCliente in context.ttarifaclientebodega
            //                                                  where tarifaCliente.idtcliente == id_cliente
            //                                                  select tarifaCliente).FirstOrDefault();

            ttarifastaller buscarTarifaBodega = (from tarifaTaller in context.ttarifastaller
                                                 where tarifaTaller.bodega == id_bodega && tarifaTaller.tipotarifa == id_tarifa
                                                 select tarifaTaller).FirstOrDefault();
            var ivabodega = Convert.ToDecimal(buscarTarifaBodega.iva);
            var buscarDatosCita = (from cita in context.tcitastaller
                                   join placaBuscar in context.icb_vehiculo
                                       on cita.placa equals placaBuscar.plac_vh into plac
                                   from placaBuscar in plac.DefaultIfEmpty()
                                   join bahia in context.tbahias
                                       on cita.bahia equals bahia.id into bah
                                   from bahia in bah.DefaultIfEmpty()
                                   join tipoBahia in context.ttipobahia
                                       on bahia.tipo_bahia equals tipoBahia.id into tip
                                   from tipoBahia in tip.DefaultIfEmpty()
                                   join taller in context.bodega_concesionario
                                       on bahia.bodega equals taller.id into tal
                                   from taller in tal.DefaultIfEmpty()
                                   join agente in context.users
                                       on cita.agente equals agente.user_id into age
                                   from agente in age.DefaultIfEmpty()
                                   join tecnico in context.ttecnicos
                                       on cita.tecnico equals tecnico.id into tec
                                   from tecnico in tec.DefaultIfEmpty()
                                   join usuario in context.users
                                       on tecnico.idusuario equals usuario.user_id
                                   join tercero in context.icb_terceros
                                       on cita.cliente equals tercero.tercero_id into ter
                                   from tercero in ter.DefaultIfEmpty()
                                   join cliente in context.tercero_cliente
                                       on tercero.tercero_id equals cliente.tercero_id into cli
                                   from cliente in cli.DefaultIfEmpty()
                                   join centro in context.centro_costo
                                       on cita.centrocosto equals centro.centcst_id into cc
                                   from centro in cc.DefaultIfEmpty()
                                   where cita.id == id_cita && cita.estadocita != 11
                                   select new
                                   {
                                       cita.id,
                                       cita.tipocita,
                                       cita.id_tipo_inspeccion,
                                       cita.tipotarifa,
                                       bahia_id = bahia.id,
                                       bahia.codigo_bahia,
                                       tipoBahia_id = tipoBahia.id,
                                       tipoBahia.descripcion,
                                       bodega_id = taller.id,
                                       taller.bodccs_nombre,
                                       cita.centrocosto,
                                       centro.centcst_nombre,
                                       sintomas = (from sintomas in context.tcitasintomas
                                                   where sintomas.idcita == cita.id
                                                   select new
                                                   {
                                                       sintomas.idcita,
                                                       sintomas.sintomas
                                                   }).ToList(),
                                       operacionesPlan = (from operaciones in context.tcitasoperacion
                                                          join tempario in context.ttempario
                                                              on operaciones.operacion equals tempario.codigo
                                                          join plan in context.tplanrepuestosmodelo
                                                              on operaciones.operacion equals plan.tempario
                                                          join iva in context.codigo_iva
                                                              on tempario.iva equals iva.id
                                                          where operaciones.idcita == cita.id && plan.inspeccion == cita.id_tipo_inspeccion && operaciones.id_plan_mantenimiento!=null
                                                          select new
                                                          {
                                                              operaciones.idcita,
                                                              operaciones.id,
                                                              tempario.operacion,
                                                              tempario.codigo,
                                                              iva = iva.porcentaje??0,
                                                              tempario.precio,
                                                              tiempo = tempario.HoraCliente != null ? tempario.HoraCliente : "0"
                                                          }).ToList(),
                                       operacionesCompletas = (from operaciones in context.tcitasoperacion
                                                               join tempario in context.ttempario
                                                                   on operaciones.operacion equals tempario.codigo
                                                               join iva in context.codigo_iva
                                                                   on tempario.iva equals iva.id
                                                               where operaciones.idcita == cita.id && operaciones.id_plan_mantenimiento == null
                                                               select new
                                                               {
                                                                   operaciones.idcita,
                                                                   operaciones.id,
                                                                   tempario.operacion,
                                                                   tempario.codigo,
                                                                   iva = ivabodega,
                                                                   precio = context.ttarifastaller.Where(d => d.bodega == id_bodega && d.tipotarifa == 1).Select(d => d.valorhora).FirstOrDefault(),
                                                                   tiempo = tempario.HoraCliente != null ? tempario.HoraCliente : "0"
                                                               }).ToList(),
                                       tecnico_id = tecnico.id,
                                       tecnicoNombre = usuario.user_nombre + " " + usuario.user_apellido,
                                       agente_id = agente.user_id,
                                       agenteNombre = agente.user_nombre + " " + agente.user_apellido,
                                       fecha_desde = cita.desde,
                                       hora_desde = cita.desde,
                                       cita.placa,
                                       cita.modelo,
                                       cita.cliente,
                                       tercero.doc_tercero,
                                       nombre_cliente = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                        tercero.apellido_tercero + " " + tercero.segapellido_tercero + " " +
                                                        tercero.razon_social,
                                       celular = tercero.celular_tercero,
                                       fijo = tercero.telf_tercero,
                                       telefonoCliente = cliente.telefono,
                                       cita.fuente,
                                       cita.tipollamada,
                                       cita.estadocita,
                                       cita.motivoestado,
                                       cita.notas
                                   }).FirstOrDefault();

            int? buscarModeloGeneral = (from vehiculo in context.icb_vehiculo
                                        join modelo in context.modelo_vehiculo
                                            on vehiculo.modvh_id equals modelo.modvh_codigo
                                        where vehiculo.plan_mayor == buscarDatosCita.placa || vehiculo.plac_vh == buscarDatosCita.placa
                                        select modelo.modelogkit).FirstOrDefault();


            var buscarOperacionesSQL = (from plan in context.tplanrepuestosmodelo
                                        join tempario in context.ttempario
                                            on plan.tempario equals tempario.codigo into ps
                                        from tempario in ps.DefaultIfEmpty()
                                        where plan.inspeccion == buscarDatosCita.id_tipo_inspeccion && plan.modgeneral == buscarModeloGeneral &&
                                              plan.tipo == "M" && plan.tempario != null
                                        select new
                                        {
                                            tempario.codigo,
                                            tempario.operacion,
                                            tempario.tiempo,
                                            manoObraSeleccionada = (from citaTempario in context.tcitasoperacion
                                                                    join temparioAux in context.ttempario
                                                                        on citaTempario.operacion equals temparioAux.codigo
                                                                    where citaTempario.idcita == buscarDatosCita.id && citaTempario.operacion == tempario.codigo
                                                                    select temparioAux).FirstOrDefault(),
                                            ivaManoObra = (from citaTempario in context.tcitasoperacion
                                                           join temparioAux in context.ttempario
                                                               on citaTempario.operacion equals temparioAux.codigo
                                                           join iva in context.codigo_iva
                                                               on temparioAux.iva equals iva.id
                                                           where citaTempario.idcita == buscarDatosCita.id && citaTempario.operacion == tempario.codigo
                                                           select iva).FirstOrDefault()
                                        }).ToList();

            var buscarOperaciones = buscarOperacionesSQL.Select(x => new
            {
                x.codigo,
                x.operacion,
                x.tiempo,
                operacionSeleccionada = x.manoObraSeleccionada != null ? true : false,
                x.manoObraSeleccionada.preciomatriz,
                x.manoObraSeleccionada.tiempomatriz,
                x.ivaManoObra.porcentaje
            });
            float tiempo = buscarOperaciones.Sum(d => d.tiempo != null ? d.tiempo.Value : 0);
            decimal valoroperaciones = buscarOperaciones.Sum(d => d.preciomatriz);


            var referenciasPlanSQL = (from plan in context.tplanrepuestosmodelo
                                      join referencia in context.icb_referencia
                                          on plan.referencia equals referencia.ref_codigo into ps
                                      from referencia in ps.DefaultIfEmpty()
                                      join precios in context.rprecios
                                          on referencia.ref_codigo equals precios.codigo into ps2
                                      from precios in ps2.DefaultIfEmpty()
                                      where plan.inspeccion == buscarDatosCita.id_tipo_inspeccion && plan.modgeneral == buscarModeloGeneral &&
                                            plan.tipo == "R" && plan.referencia != null
                                      select new
                                      {
                                          referencia.ref_codigo,
                                          referencia.ref_descripcion,
                                          precio = plan.listaprecios.Contains("precio1") ? precios != null ? precios.precio1 : 0 :
                                              plan.listaprecios.Contains("precio2") ? precios != null ? precios.precio1 : 0 :
                                              plan.listaprecios.Contains("precio3") ? precios != null ? precios.precio1 : 0 :
                                              plan.listaprecios.Contains("precio4") ? precios != null ? precios.precio1 : 0 :
                                              plan.listaprecios.Contains("precio5") ? precios != null ? precios.precio1 : 0 :
                                              plan.listaprecios.Contains("precio6") ? precios != null ? precios.precio1 : 0 :
                                              plan.listaprecios.Contains("precio7") ? precios != null ? precios.precio1 : 0 :
                                              plan.listaprecios.Contains("precio8") ? precios != null ? precios.precio1 : 0 :
                                              plan.listaprecios.Contains("precio9") ? precios != null ? precios.precio1 : 0 : 0,
                                          referencia.por_iva,
                                          cantidad = plan.cantidad != null ? plan.cantidad : 0,
                                          referenciaSeleccionada = (from citaRepuesto in context.tcitarepuestos
                                                                    join referenciaAux in context.icb_referencia
                                                                        on citaRepuesto.idrepuesto equals referenciaAux.ref_codigo
                                                                    where citaRepuesto.idcita == buscarDatosCita.id &&
                                                                          citaRepuesto.idrepuesto == referencia.ref_codigo
                                                                    select referenciaAux).FirstOrDefault()
                                      }).ToList();

            var buscarReferencias = referenciasPlanSQL.Select(x => new
            {
                x.ref_codigo,
                x.ref_descripcion,
                x.precio,
                x.cantidad,
                x.por_iva,
                valorIva = Math.Round(x.precio * (decimal)x.por_iva / 100),
                valorTotal = Math.Round((x.precio + x.precio * (decimal)x.por_iva / 100) * x.cantidad ?? 0),
                referenciaSeleccionada = x.referenciaSeleccionada != null ? true : false
            });

            decimal valorrepuestos = buscarReferencias.Sum(d => d.valorTotal);

            var operacionesSinPlanSolamente =
                buscarDatosCita.operacionesCompletas.Except(buscarDatosCita.operacionesPlan).ToList();
            var operacionesSinPlan = new List<listaOperacionesSinPlan>();

            foreach (var item in operacionesSinPlanSolamente)
            {
                var valorunahora = context.ttarifastaller.Where(d => d.bodega == id_bodega && d.tipotarifa == 1).Select(d => d.valorhora).FirstOrDefault();
                var tiempooperacion = Convert.ToDecimal(item.tiempo, new CultureInfo("en-US"));
                var precio = valorunahora * tiempooperacion;
                var ivaoperacion = item.iva;
                var valoriva = ((precio??0) * ivaoperacion) / 100;
                operacionesSinPlan.Add(new listaOperacionesSinPlan
                {
                    codigo=item.codigo,
                operacion=item.operacion,
                tiempo=tiempooperacion,
                precio=precio??0,
                valorIva= valoriva,
                    valorTotal = (precio??0)+valoriva
                });
            }

            var buscarCita = new
            {
                buscarDatosCita.id,
                buscarDatosCita.id_tipo_inspeccion,
                buscarDatosCita.tipotarifa,
                buscarDatosCita.bahia_id,
                buscarDatosCita.tipoBahia_id,
                buscarDatosCita.descripcion,
                buscarDatosCita.tipocita,
                buscarDatosCita.bodega_id,
                buscarDatosCita.bodccs_nombre,
                buscarDatosCita.agente_id,
                buscarDatosCita.agenteNombre,
                buscarDatosCita.tecnico_id,
                tecnicoNombre = "(" + buscarDatosCita.codigo_bahia + ") " + buscarDatosCita.tecnicoNombre,
                buscarDatosCita.sintomas,
                buscarDatosCita.operacionesPlan,
                operacionesSinPlan,
                buscarReferencias,
                buscarOperaciones,
                fecha_desde = buscarDatosCita.fecha_desde.ToShortDateString(),
                hora_desde = buscarDatosCita.fecha_desde.ToString("HH:mm:ss"),
                buscarDatosCita.placa,
                buscarDatosCita.modelo,
                buscarDatosCita.cliente,
                buscarDatosCita.doc_tercero,
                buscarDatosCita.nombre_cliente,
                buscarDatosCita.celular,
                buscarDatosCita.fijo,
                buscarDatosCita.telefonoCliente,
                buscarDatosCita.fuente,
                buscarDatosCita.tipollamada,
                buscarDatosCita.estadocita,
                buscarDatosCita.motivoestado,
                buscarDatosCita.notas,
                buscarDatosCita.centrocosto,
                buscarDatosCita.centcst_nombre
            };

            if (buscarCita != null)
            {
                string valortotal = (valoroperaciones + valorrepuestos).ToString("N0", new CultureInfo("is-IS"));
                return Json(new { encontrado = true, buscarCita, tiempo, valortotal, listataller }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { encontrado = false, estadoCita = idEstado, listataller }, JsonRequestBehavior.AllowGet);
        }

        public TimeSpan InicioDiaTecnico(parametrizacion_horario buscarHorarioPerito, DateTime fecha)
        {
            if (buscarHorarioPerito == null)
            {
                buscarHorarioPerito = new parametrizacion_horario();
            }

            if (fecha.DayOfWeek == DayOfWeek.Monday)
            {
                if (buscarHorarioPerito.lunes_desde != null && buscarHorarioPerito.lunes_hasta != null)
                {
                    return buscarHorarioPerito.lunes_desde ?? new TimeSpan();
                }

                return buscarHorarioPerito.lunes_desde2 ?? new TimeSpan(23, 59, 59);
            }

            if (fecha.DayOfWeek == DayOfWeek.Tuesday)
            {
                if (buscarHorarioPerito.martes_desde != null && buscarHorarioPerito.martes_hasta != null)
                {
                    return buscarHorarioPerito.martes_desde ?? new TimeSpan();
                }

                return buscarHorarioPerito.martes_desde2 ?? new TimeSpan(23, 59, 59);
            }

            if (fecha.DayOfWeek == DayOfWeek.Wednesday)
            {
                if (buscarHorarioPerito.miercoles_desde != null && buscarHorarioPerito.miercoles_hasta != null)
                {
                    return buscarHorarioPerito.miercoles_desde ?? new TimeSpan();
                }

                return buscarHorarioPerito.miercoles_desde2 ?? new TimeSpan(23, 59, 59);
            }

            if (fecha.DayOfWeek == DayOfWeek.Thursday)
            {
                if (buscarHorarioPerito.jueves_desde != null && buscarHorarioPerito.jueves_hasta != null)
                {
                    return buscarHorarioPerito.jueves_desde ?? new TimeSpan();
                }

                return buscarHorarioPerito.jueves_desde2 ?? new TimeSpan(23, 59, 59);
            }

            if (fecha.DayOfWeek == DayOfWeek.Friday)
            {
                if (buscarHorarioPerito.viernes_desde != null && buscarHorarioPerito.viernes_hasta != null)
                {
                    return buscarHorarioPerito.viernes_desde ?? new TimeSpan();
                }

                return buscarHorarioPerito.viernes_desde2 ?? new TimeSpan(23, 59, 59);
            }

            if (fecha.DayOfWeek == DayOfWeek.Saturday)
            {
                if (buscarHorarioPerito.sabado_desde != null && buscarHorarioPerito.sabado_hasta != null)
                {
                    return buscarHorarioPerito.sabado_desde ?? new TimeSpan();
                }

                return buscarHorarioPerito.sabado_desde2 ?? new TimeSpan(23, 59, 59);
            }

            if (fecha.DayOfWeek == DayOfWeek.Sunday)
            {
                if (buscarHorarioPerito.domingo_desde != null && buscarHorarioPerito.domingo_hasta != null)
                {
                    return buscarHorarioPerito.domingo_desde ?? new TimeSpan();
                }

                return buscarHorarioPerito.domingo_desde2 ?? new TimeSpan(23, 59, 59);
            }

            return new TimeSpan();
        }

        public TimeSpan FinDiaTecnico(parametrizacion_horario buscarHorarioPerito, DateTime fecha)
        {
            if (buscarHorarioPerito == null)
            {
                buscarHorarioPerito = new parametrizacion_horario();
            }

            if (fecha.DayOfWeek == DayOfWeek.Monday)
            {
                if (buscarHorarioPerito.lunes_desde2 != null && buscarHorarioPerito.lunes_hasta2 != null)
                {
                    return buscarHorarioPerito.lunes_hasta2 ?? new TimeSpan();
                }

                return buscarHorarioPerito.lunes_hasta ?? new TimeSpan();
            }

            if (fecha.DayOfWeek == DayOfWeek.Tuesday)
            {
                if (buscarHorarioPerito.martes_desde2 != null && buscarHorarioPerito.martes_hasta2 != null)
                {
                    return buscarHorarioPerito.martes_hasta2 ?? new TimeSpan();
                }

                return buscarHorarioPerito.martes_hasta ?? new TimeSpan();
            }

            if (fecha.DayOfWeek == DayOfWeek.Wednesday)
            {
                if (buscarHorarioPerito.miercoles_desde2 != null && buscarHorarioPerito.miercoles_hasta2 != null)
                {
                    return buscarHorarioPerito.miercoles_hasta2 ?? new TimeSpan();
                }

                return buscarHorarioPerito.miercoles_hasta ?? new TimeSpan();
            }

            if (fecha.DayOfWeek == DayOfWeek.Thursday)
            {
                if (buscarHorarioPerito.jueves_desde2 != null && buscarHorarioPerito.jueves_hasta2 != null)
                {
                    return buscarHorarioPerito.jueves_hasta2 ?? new TimeSpan();
                }

                return buscarHorarioPerito.jueves_hasta ?? new TimeSpan();
            }

            if (fecha.DayOfWeek == DayOfWeek.Friday)
            {
                if (buscarHorarioPerito.viernes_desde2 != null && buscarHorarioPerito.viernes_hasta2 != null)
                {
                    return buscarHorarioPerito.viernes_hasta2 ?? new TimeSpan();
                }

                return buscarHorarioPerito.viernes_hasta ?? new TimeSpan();
            }

            if (fecha.DayOfWeek == DayOfWeek.Saturday)
            {
                if (buscarHorarioPerito.sabado_desde2 != null && buscarHorarioPerito.sabado_hasta2 != null)
                {
                    return buscarHorarioPerito.sabado_hasta2 ?? new TimeSpan();
                }

                return buscarHorarioPerito.sabado_hasta ?? new TimeSpan();
            }

            if (fecha.DayOfWeek == DayOfWeek.Sunday)
            {
                if (buscarHorarioPerito.domingo_desde2 != null && buscarHorarioPerito.domingo_hasta2 != null)
                {
                    return buscarHorarioPerito.domingo_hasta2 ?? new TimeSpan();
                }

                return buscarHorarioPerito.domingo_hasta ?? new TimeSpan();
            }

            return new TimeSpan();
        }

        public TimeSpan validarInicioMedioDia(parametrizacion_horario buscarHorarioPerito, DateTime fecha)
        {
            if (buscarHorarioPerito == null)
            {
                buscarHorarioPerito = new parametrizacion_horario();
            }

            if (fecha.DayOfWeek == DayOfWeek.Monday)
            {
                if (buscarHorarioPerito.lunes_desde != null && buscarHorarioPerito.lunes_hasta != null)
                {
                    return buscarHorarioPerito.lunes_hasta ?? new TimeSpan();
                }
            }

            if (fecha.DayOfWeek == DayOfWeek.Tuesday)
            {
                if (buscarHorarioPerito.martes_desde != null && buscarHorarioPerito.martes_hasta != null)
                {
                    return buscarHorarioPerito.martes_hasta ?? new TimeSpan();
                }
            }

            if (fecha.DayOfWeek == DayOfWeek.Wednesday)
            {
                if (buscarHorarioPerito.miercoles_desde != null && buscarHorarioPerito.miercoles_hasta != null)
                {
                    return buscarHorarioPerito.miercoles_hasta ?? new TimeSpan();
                }
            }

            if (fecha.DayOfWeek == DayOfWeek.Thursday)
            {
                if (buscarHorarioPerito.jueves_desde != null && buscarHorarioPerito.jueves_hasta != null)
                {
                    return buscarHorarioPerito.jueves_hasta ?? new TimeSpan();
                }
            }

            if (fecha.DayOfWeek == DayOfWeek.Friday)
            {
                if (buscarHorarioPerito.viernes_desde != null && buscarHorarioPerito.viernes_hasta != null)
                {
                    return buscarHorarioPerito.viernes_hasta ?? new TimeSpan();
                }
            }

            if (fecha.DayOfWeek == DayOfWeek.Saturday)
            {
                if (buscarHorarioPerito.sabado_desde != null && buscarHorarioPerito.sabado_hasta != null)
                {
                    return buscarHorarioPerito.sabado_hasta ?? new TimeSpan();
                }
            }

            if (fecha.DayOfWeek == DayOfWeek.Sunday)
            {
                if (buscarHorarioPerito.domingo_desde != null && buscarHorarioPerito.domingo_hasta != null)
                {
                    return buscarHorarioPerito.domingo_hasta ?? new TimeSpan();
                }
            }

            return new TimeSpan();
        }

        public TimeSpan validarFinalMedioDia(parametrizacion_horario buscarHorarioPerito, DateTime fecha)
        {
            if (buscarHorarioPerito == null)
            {
                buscarHorarioPerito = new parametrizacion_horario();
            }

            if (fecha.DayOfWeek == DayOfWeek.Monday)
            {
                if (buscarHorarioPerito.lunes_desde2 != null && buscarHorarioPerito.lunes_hasta2 != null)
                {
                    return buscarHorarioPerito.lunes_desde2 ?? new TimeSpan();
                }
            }

            if (fecha.DayOfWeek == DayOfWeek.Tuesday)
            {
                if (buscarHorarioPerito.martes_desde2 != null && buscarHorarioPerito.martes_hasta2 != null)
                {
                    return buscarHorarioPerito.martes_desde2 ?? new TimeSpan();
                }
            }

            if (fecha.DayOfWeek == DayOfWeek.Wednesday)
            {
                if (buscarHorarioPerito.miercoles_desde2 != null && buscarHorarioPerito.miercoles_hasta2 != null)
                {
                    return buscarHorarioPerito.miercoles_desde2 ?? new TimeSpan();
                }
            }

            if (fecha.DayOfWeek == DayOfWeek.Thursday)
            {
                if (buscarHorarioPerito.jueves_desde2 != null && buscarHorarioPerito.jueves_hasta2 != null)
                {
                    return buscarHorarioPerito.jueves_desde2 ?? new TimeSpan();
                }
            }

            if (fecha.DayOfWeek == DayOfWeek.Friday)
            {
                if (buscarHorarioPerito.viernes_desde2 != null && buscarHorarioPerito.viernes_hasta2 != null)
                {
                    return buscarHorarioPerito.viernes_desde2 ?? new TimeSpan();
                }
            }

            if (fecha.DayOfWeek == DayOfWeek.Saturday)
            {
                if (buscarHorarioPerito.sabado_desde2 != null && buscarHorarioPerito.sabado_hasta2 != null)
                {
                    return buscarHorarioPerito.sabado_desde2 ?? new TimeSpan();
                }
            }

            if (fecha.DayOfWeek == DayOfWeek.Sunday)
            {
                if (buscarHorarioPerito.domingo_desde2 != null && buscarHorarioPerito.domingo_hasta2 != null)
                {
                    return buscarHorarioPerito.domingo_desde2 ?? new TimeSpan();
                }
            }

            return new TimeSpan(23, 59, 59);
        }

        /****************************************************************************************************************************************************************************************************************/

        public TimeSpan validarInicioNovedad(vhorarionovedad buscarNovedadPerito, DateTime fecha)
        {
            if (buscarNovedadPerito == null)
            {
                buscarNovedadPerito = new vhorarionovedad();
            }

            if (buscarNovedadPerito.fechaini != null && buscarNovedadPerito.fechafin != null)
            {
                if (buscarNovedadPerito.fechaini.Date == fecha.Date)
                {
                    TimeSpan hora = new TimeSpan(buscarNovedadPerito.fechaini.Hour, buscarNovedadPerito.fechaini.Minute, 0);

                    return hora;
                }

                return new TimeSpan();
            }

            return new TimeSpan();
        }

        public TimeSpan validarFinalNovedad(vhorarionovedad buscarNovedadPerito, DateTime fecha)
        {
            if (buscarNovedadPerito == null)
            {
                buscarNovedadPerito = new vhorarionovedad();
            }

            if (buscarNovedadPerito.fechaini != null && buscarNovedadPerito.fechafin != null)
            {
                if (buscarNovedadPerito.fechaini.Date == fecha.Date)
                {
                    TimeSpan hora = new TimeSpan(buscarNovedadPerito.fechafin.Hour, buscarNovedadPerito.fechafin.Minute, 0);

                    return hora;
                }

                return new TimeSpan();
            }

            return new TimeSpan(23, 59, 59);
        }


        public TimeSpan validarInicioDescanso(ttecnicos buscarDescansoPerito, DateTime fecha)
        {
            if (buscarDescansoPerito == null)
            {
                buscarDescansoPerito = new ttecnicos();
            }

            if (buscarDescansoPerito.iniciodescanso != null && buscarDescansoPerito.findescanso != null)
            {
                TimeSpan hora = new TimeSpan(buscarDescansoPerito.iniciodescanso.Value.Hours,
                    buscarDescansoPerito.iniciodescanso.Value.Minutes, 0);

                return hora;
            }

            return new TimeSpan();
        }

        public TimeSpan validarFinalDescanso(ttecnicos buscarDescansoPerito, DateTime fecha)
        {
            if (buscarDescansoPerito == null)
            {
                buscarDescansoPerito = new ttecnicos();
            }

            if (buscarDescansoPerito.iniciodescanso != null && buscarDescansoPerito.findescanso != null)
            {
                TimeSpan hora = new TimeSpan(buscarDescansoPerito.findescanso.Value.Hours,
                    buscarDescansoPerito.findescanso.Value.Minutes, 0);

                return hora;
            }

            return new TimeSpan(23, 59, 59);
        }

        /****************************************************************************************************************************************************************************************************************/

        public string horaConCita(TimeSpan horaTabla, TimeSpan HoraInicioCita, TimeSpan HoraFinCita)
        {
            if (TimeSpan.Compare(horaTabla, HoraInicioCita) == 0)
            {
                return "inicio";
            }

            if (TimeSpan.Compare(horaTabla, HoraInicioCita) >= 0 && TimeSpan.Compare(horaTabla, HoraFinCita) < 0)
            {
                return "medio";
            }

            if (TimeSpan.Compare(horaTabla, HoraInicioCita) <= 0 &&
                TimeSpan.Compare(horaTabla, HoraFinCita) >= 0)
            {
                return "fin";
            }

            return "libre";
        }

        public bool ExisteCitaEnElHorario(TimeSpan horaTabla, TimeSpan HoraInicioCita, TimeSpan HoraFinCita)
        {
            if (TimeSpan.Compare(horaTabla, HoraInicioCita) == 0)
            {
                return true;
            }

            if (TimeSpan.Compare(horaTabla, HoraInicioCita) >= 0 &&
                TimeSpan.Compare(horaTabla, HoraFinCita) < 0)
            {
                return true;
            }

            if (TimeSpan.Compare(horaTabla, HoraInicioCita) <= 0 &&
                TimeSpan.Compare(horaTabla, HoraFinCita) >= 0)
            {
                return true;
            }
  
            return false;
        }

        public bool ExisteCitaEntreDosHoras(List<tcitastaller> citasDelDia, DateTime inicio, DateTime fin)
        {
            foreach (tcitastaller cita in citasDelDia)
            {
                if (DateTime.Compare(inicio, cita.desde) == 0)
                {
                    return true;
                }
                else if (DateTime.Compare(inicio, cita.desde) < 0 && DateTime.Compare(fin, cita.desde) > 0)
                {
                    return true;
                }
                else if (DateTime.Compare(inicio, cita.desde) > 0 && DateTime.Compare(inicio, cita.hasta) < 0)
                {
                    return true;
                }
            }

            return false;
        }

        public JsonResult citaVSnovedad(DateTime desde, DateTime hasta, int usuario_id)
        {
            int buscarTecnico = context.ttecnicos.FirstOrDefault(x => x.idusuario == usuario_id).id;
            List<tcitastaller> citasDelDia = context.tcitastaller.Where(x =>
                x.tecnico == buscarTecnico && x.desde.Year == desde.Year && x.desde.Month == desde.Month &&
                x.desde.Day == desde.Day).ToList();
            if (citasDelDia != null)
            {
                foreach (tcitastaller item in citasDelDia)
                {
                    if (ExisteCitaEntreDosHoras(citasDelDia, desde, hasta))
                    {
                        //la novedad se cruza con una cita y por tal motivo no se puede registrar
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        //la novedad no se cruza con una cita y por tal motivo se puede registrar
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            //el tecnico no tiene citas, por tal motivo la novedad no se cruza con nada y se puede registrar
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public bool CalcularHorarioDisponible(int usuarioId, int bahia, int bodega, DateTime fecha, DateTime inicio,
            DateTime fin, tcitastaller cita, bool actualizarCita, DateTime horaInicialAct, DateTime horaFinalAct)
        {
            List<int> buscarBahiasTecnico = context.tbahias.Where(x => x.idtecnico == usuarioId).Select(x => x.id).ToList();
            List<tcitastaller> citasDelDia = context.tcitastaller.Where(x =>
                x.tecnico == usuarioId && buscarBahiasTecnico.Contains(x.bahia) && x.desde.Year == fecha.Year &&
                x.desde.Month == fecha.Month && x.desde.Day == fecha.Day && x.estadocita != 11).ToList();
            //var buscarTTecnico = context.ttecnicos.FirstOrDefault(x=>x.idusuario == cita.tecnico);

            if (actualizarCita)
            {
                List<tcitastaller> citasRemover = citasDelDia.Where(x =>
                    x.desde.Hour == horaInicialAct.Hour && x.desde.Minute == horaInicialAct.Minute &&
                    x.hasta.Hour == horaFinalAct.Hour && x.hasta.Minute == horaFinalAct.Minute).ToList();
                foreach (tcitastaller citaEliminar in citasRemover)
                {
                    citasDelDia.Remove(citaEliminar);
                }
            }

            //var buscarBodega = context.tbahias.FirstOrDefault(x => x.tipo_bahia == bahia && x.bodega == bodega && x.idtecnico == usuarioId);
            //var id_bodega = buscarBodega != null ? buscarBodega.bodega : 0;
            var buscarTiemposBodega = (from bodegas in context.bodega_concesionario
                                       where bodegas.id == bodega
                                       select new
                                       {
                                           bodegas.hora_inicial,
                                           bodegas.hora_final,
                                           bodegas.lapso_tiempo
                                       }).FirstOrDefault();

            if (buscarTiemposBodega == null)
            {
                return false;
            }

            // Se busca que el horario se encuentre disponible segun el horario que el ha elegido
            ttecnicos buscarTecnico = context.ttecnicos.FirstOrDefault(x => x.id == usuarioId);
            int tecnicoIdTablaUsers = 0;
            if (buscarTecnico != null)
            {
                tecnicoIdTablaUsers = buscarTecnico != null ? buscarTecnico.idusuario : 0;
            }

            parametrizacion_horario buscarHorarioPerito =
                context.parametrizacion_horario.FirstOrDefault(x => x.usuario_id == tecnicoIdTablaUsers);
            ttecnicos buscarDescansosPerito = context.ttecnicos.FirstOrDefault(x => x.idusuario == tecnicoIdTablaUsers);
            if (buscarHorarioPerito != null)
            {
                TimeSpan horaInicio = new TimeSpan(inicio.Hour, inicio.Minute, inicio.Second);
                TimeSpan horaFin = new TimeSpan(fin.Hour, fin.Minute, fin.Second);

                TimeSpan horaSalidaDescanso = buscarDescansosPerito.iniciodescanso ?? new TimeSpan();
                TimeSpan horaEntradaDescanso = buscarDescansosPerito.findescanso ?? new TimeSpan();
                DateTime fechaActualSalidaDescanso = fecha.Add(horaSalidaDescanso);
                DateTime fechaActualEntradaDescanso = fecha.Add(horaEntradaDescanso);

                // Valido si la cita inicia antes del descanso
                TimeSpan descansito = new TimeSpan();
                TimeSpan horasDuracionTotal = new TimeSpan();
                TimeSpan diferenciaDeHoras = new TimeSpan();
                TimeSpan timeSpanAAgregarATarde = new TimeSpan();
                TimeSpan horaFinalDescanso = new TimeSpan();
                /*
				 var concat0 = Convert.ToDateTime(fechaActual + " " + hid);//inicio descanso
				 var concat1 = Convert.ToDateTime(fechaActual + " " + hfd);//fin del descanso
				 var concat2 = Convert.ToDateTime(fechaActual + " " + hia);//inicio del almuerzo
				 if (inicioCita < concat0 && (finCita > concat1 && finCita <= concat2))
				 */
                if (horaInicio < horaSalidaDescanso && horaFin >= horaEntradaDescanso)
                {
                    descansito = horaEntradaDescanso.Subtract(horaSalidaDescanso);
                    horasDuracionTotal = horaFin.Subtract(horaInicio);
                    diferenciaDeHoras = horaSalidaDescanso.Subtract(horaInicio);
                    timeSpanAAgregarATarde = new TimeSpan(horasDuracionTotal.Hours - diferenciaDeHoras.Hours,
                        horasDuracionTotal.Minutes - diferenciaDeHoras.Minutes, 0);
                    horaFinalDescanso = new TimeSpan(
                        buscarDescansosPerito.findescanso != null
                            ? buscarDescansosPerito.findescanso.Value.Hours + timeSpanAAgregarATarde.Hours
                            : 0,
                        buscarDescansosPerito.findescanso != null
                            ? buscarDescansosPerito.findescanso.Value.Minutes + timeSpanAAgregarATarde.Minutes
                            : 0, 0);
                    cita.desde = fecha.Add(horaInicio);
                    cita.hasta = fecha.Add(horaFinalDescanso);
                    horaFin = horaFinalDescanso;
                }
                else if (horaInicio < horaSalidaDescanso && horaFin <= horaEntradaDescanso &&
                         horaFin >= horaSalidaDescanso)
                {
                    descansito = horaEntradaDescanso.Subtract(horaSalidaDescanso);
                    horasDuracionTotal = horaFin.Subtract(horaInicio);
                    diferenciaDeHoras = horaSalidaDescanso.Subtract(horaInicio);
                    timeSpanAAgregarATarde = new TimeSpan(horasDuracionTotal.Hours - diferenciaDeHoras.Hours,
                        horasDuracionTotal.Minutes - diferenciaDeHoras.Minutes, 0);
                    horaFinalDescanso = new TimeSpan(
                        buscarDescansosPerito.findescanso != null
                            ? buscarDescansosPerito.findescanso.Value.Hours + timeSpanAAgregarATarde.Hours
                            : 0,
                        buscarDescansosPerito.findescanso != null
                            ? buscarDescansosPerito.findescanso.Value.Minutes + timeSpanAAgregarATarde.Minutes
                            : 0, 0);
                    cita.desde = fecha.Add(horaInicio);
                    cita.hasta = fecha.Add(horaFinalDescanso);
                    horaFin = horaFinalDescanso;
                }
                else if (horaInicio < horaSalidaDescanso && horaFin <= horaEntradaDescanso)
                {
                    cita.desde = fecha.Add(horaInicio);
                    cita.hasta = fecha.Add(horaFin);
                }
                else
                {
                    cita.desde = fecha.Add(horaInicio);
                    cita.hasta = fecha.Add(horaFin);
                }

                // Valido si el tecnico tiene novedad
                vhorarionovedad buscarNovedadPerito = context.vhorarionovedad.FirstOrDefault(x =>
                    x.horarioid == buscarHorarioPerito.horario_id && x.fechaini.Year == cita.desde.Year &&
                    x.fechaini.Month == cita.desde.Month && x.fechaini.Day == cita.desde.Day);
                TimeSpan horaInicioNovedad = validarInicioNovedad(buscarNovedadPerito, fecha);
                TimeSpan horaFinNovedad = validarFinalNovedad(buscarNovedadPerito, fecha);
                //TimeSpan novedad = new TimeSpan();
                //TimeSpan horasDuracionCita = new TimeSpan();
                //TimeSpan diferenciaDeHorasNovedad = new TimeSpan();
                //TimeSpan timeSpanAAgregarANovedad = new TimeSpan();
                //TimeSpan horaFinalNovedad = new TimeSpan();

                if (TimeSpan.Compare(horaInicioNovedad, new TimeSpan()) != 0 &&
                    TimeSpan.Compare(horaFinNovedad, new TimeSpan()) != 0)
                {
                    DateTime concat5 = Convert.ToDateTime(fecha.Add(horaInicioNovedad)); //inicio novedad
                    DateTime concat6 = Convert.ToDateTime(fecha.Add(horaFinNovedad)); //fin novedad

                    citasDelDia.Add(new tcitastaller { desde = concat5, hasta = concat6 });
                    if (ExisteCitaEntreDosHoras(citasDelDia, inicio, fin))
                    {
                        return false;
                    }
                    //novedad = horaFinNovedad.Subtract(horaInicioNovedad);
                    //horasDuracionCita = horaFin.Subtract(horaInicio);
                    //diferenciaDeHorasNovedad = horaInicioNovedad.Subtract(horaInicio);
                    //timeSpanAAgregarANovedad = new TimeSpan((horasDuracionCita.Hours - diferenciaDeHorasNovedad.Hours), (horasDuracionCita.Minutes - diferenciaDeHorasNovedad.Minutes), 0);
                    //horaFinalNovedad = new TimeSpan(horaFinNovedad != null ? horaFinNovedad.Hours + (timeSpanAAgregarANovedad.Hours) : 0, horaFinNovedad != null ? horaFinNovedad.Minutes + (timeSpanAAgregarANovedad.Minutes) : 0, 0);
                    //
                    //cita.desde = fecha.Add(horaInicio);
                    //cita.hasta = fecha.Add(horaFinalNovedad);
                    //horaFin = horaFinalNovedad;				
                }

                // Si la cita es para el dia "LUNES" se valida si el horario funciona por la mañana o por la tarde
                if (fecha.DayOfWeek == DayOfWeek.Monday)
                {
                    #region Lunes

                    if (buscarHorarioPerito.lunes_desde != null && buscarHorarioPerito.lunes_hasta != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.lunes_desde ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.lunes_hasta ?? new TimeSpan()) <= 0)
                        {
                            if (!ExisteCitaEntreDosHoras(citasDelDia, inicio, fin))
                            {
                                if (actualizarCita)
                                {
                                    int bodegaBahia = context.tbahias.FirstOrDefault(x =>
                                        x.id == bahia && x.bodega == bodega && x.idtecnico == usuarioId).id;
                                    cita.bahia = bodegaBahia;
                                    context.Entry(cita).State = EntityState.Modified;
                                }
                                else
                                {
                                    if (TimeSpan.Compare(descansito, new TimeSpan()) == 0)
                                    {
                                        if (buscarTiemposBodega.hora_final < horaFinalDescanso ||
                                            buscarDescansosPerito.findescanso == null)
                                        {
                                            return false;
                                        }

                                        if (!ExisteCitaEntreDosHoras(citasDelDia, cita.desde, cita.hasta))
                                        {
                                            context.tcitastaller.Add(cita);
                                        }
                                    }
                                    else
                                    {
                                        citasDelDia.Add(new tcitastaller
                                        { desde = fechaActualSalidaDescanso, hasta = fechaActualEntradaDescanso });
                                        if (!ExisteCitaEntreDosHoras(citasDelDia, fecha.Add(horaInicio),
                                            fecha.Add(buscarDescansosPerito.iniciodescanso ?? new TimeSpan())))
                                        {
                                            if (buscarTiemposBodega.hora_final < horaFinalDescanso ||
                                                buscarDescansosPerito.findescanso == null)
                                            {
                                                return false;
                                            }

                                            if (!ExisteCitaEntreDosHoras(citasDelDia,
                                                fecha.Add(buscarDescansosPerito.findescanso ?? new TimeSpan()),
                                                fecha.Add(horaFinalDescanso)))
                                            {
                                                context.tcitastaller.Add(cita);
                                            }
                                        }
                                    }
                                }

                                int guardar = context.SaveChanges();
                                if (guardar > 0)
                                {
                                    return true;
                                }

                                return false;
                            }

                            return false;
                        }

                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.lunes_hasta ?? new TimeSpan()) <= 0)
                        {
                            if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.lunes_desde ?? new TimeSpan()) >= 0)
                            {
                                // Se busca si el usuario puede atender luego del descanso de medio dia y se divide la cita en una parte por la mañana y una por la tarde
                                TimeSpan horaEntradaMedioDia = buscarHorarioPerito.lunes_desde2 ?? new TimeSpan();
                                TimeSpan horaSalidaMedioDia = buscarHorarioPerito.lunes_hasta ?? new TimeSpan();
                                DateTime fechaActualSalida = fecha.Add(horaSalidaMedioDia);
                                DateTime fechaActualEntrada = fecha.Add(horaEntradaMedioDia);

                                citasDelDia.Add(
                                    new tcitastaller { desde = fechaActualSalida, hasta = fechaActualEntrada });

                                // Se valida la parte de por la mañana haber si esta libre
                                if (!ExisteCitaEntreDosHoras(citasDelDia, fecha.Add(horaInicio),
                                    fecha.Add(buscarHorarioPerito.lunes_hasta ?? new TimeSpan())))
                                {
                                    // Se valida la parte de por la tarde haber si esta libre
                                    horasDuracionTotal = horaFin.Subtract(horaInicio);
                                    diferenciaDeHoras = horaSalidaMedioDia.Subtract(horaInicio);
                                    timeSpanAAgregarATarde =
                                        new TimeSpan(horasDuracionTotal.Hours - diferenciaDeHoras.Hours,
                                            horasDuracionTotal.Minutes - diferenciaDeHoras.Minutes, 0);
                                    TimeSpan horaFinalTarde = new TimeSpan(
                                        buscarHorarioPerito.lunes_desde2 != null
                                            ? buscarHorarioPerito.lunes_desde2.Value.Hours +
                                              timeSpanAAgregarATarde.Hours
                                            : 0,
                                        buscarHorarioPerito.lunes_desde2 != null
                                            ? buscarHorarioPerito.lunes_desde2.Value.Minutes +
                                              timeSpanAAgregarATarde.Minutes
                                            : 0, 0);

                                    if (buscarTiemposBodega.hora_final < horaFinalTarde ||
                                        buscarHorarioPerito.lunes_desde2 == null)
                                    {
                                        return false;
                                    }

                                    if (!ExisteCitaEntreDosHoras(citasDelDia,
                                        fecha.Add(buscarHorarioPerito.lunes_desde2 ?? new TimeSpan()),
                                        fecha.Add(horaFinalTarde)))
                                    {
                                        cita.desde = fecha.Add(horaInicio);
                                        cita.hasta = fecha.Add(horaFinalTarde);
                                        if (actualizarCita)
                                        {
                                            int bodegaBahia = context.tbahias.FirstOrDefault(x =>
                                                x.id == bahia && x.bodega == bodega && x.idtecnico == usuarioId).id;
                                            cita.bahia = bodegaBahia;
                                            context.Entry(cita).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            context.tcitastaller.Add(cita);
                                        }

                                        int guardar = context.SaveChanges();
                                        if (guardar > 0)
                                        {
                                            return true;
                                        }

                                        return false;
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    if (buscarHorarioPerito.lunes_desde2 != null && buscarHorarioPerito.lunes_hasta2 != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.lunes_desde2 ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.lunes_hasta2 ?? new TimeSpan()) <= 0)
                        {
                            if (buscarTiemposBodega.hora_final < horaFin)
                            {
                                return false;
                            }

                            if (actualizarCita)
                            {
                                int bodegaBahia = context.tbahias.FirstOrDefault(x =>
                                    x.id == bahia && x.bodega == bodega && x.idtecnico == usuarioId).id;
                                cita.bahia = bodegaBahia;
                                context.Entry(cita).State = EntityState.Modified;
                            }
                            else
                            {
                                context.tcitastaller.Add(cita);
                            }

                            int guardar = context.SaveChanges();
                            if (guardar > 0)
                            {
                                return true;
                            }

                            return false;
                        }
                    }

                    return false;

                    #endregion
                }

                // Si la cita es para el dia "MARTES" se valida si el horario funciona por la mañana o por la tarde
                if (fecha.DayOfWeek == DayOfWeek.Tuesday)
                {
                    #region Martes

                    if (buscarHorarioPerito.martes_desde != null && buscarHorarioPerito.martes_hasta != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.martes_desde ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.martes_hasta ?? new TimeSpan()) <= 0)
                        {
                            if (!ExisteCitaEntreDosHoras(citasDelDia, inicio, fin))
                            {
                                if (actualizarCita)
                                {
                                    int bodegaBahia = context.tbahias.FirstOrDefault(x =>
                                        x.id == bahia && x.bodega == bodega && x.idtecnico == usuarioId).id;
                                    cita.bahia = bodegaBahia;
                                    context.Entry(cita).State = EntityState.Modified;
                                }
                                else
                                {
                                    if (TimeSpan.Compare(descansito, new TimeSpan()) == 0)
                                    {
                                        if (buscarTiemposBodega.hora_final < horaFinalDescanso ||
                                            buscarDescansosPerito.findescanso == null)
                                        {
                                            return false;
                                        }

                                        if (!ExisteCitaEntreDosHoras(citasDelDia, cita.desde, cita.hasta))
                                        {
                                            context.tcitastaller.Add(cita);
                                        }
                                    }
                                    else
                                    {
                                        citasDelDia.Add(new tcitastaller
                                        { desde = fechaActualSalidaDescanso, hasta = fechaActualEntradaDescanso });
                                        if (!ExisteCitaEntreDosHoras(citasDelDia, fecha.Add(horaInicio),
                                            fecha.Add(buscarDescansosPerito.iniciodescanso ?? new TimeSpan())))
                                        {
                                            if (buscarTiemposBodega.hora_final < horaFinalDescanso ||
                                                buscarDescansosPerito.findescanso == null)
                                            {
                                                return false;
                                            }

                                            if (!ExisteCitaEntreDosHoras(citasDelDia,
                                                fecha.Add(buscarDescansosPerito.findescanso ?? new TimeSpan()),
                                                fecha.Add(horaFinalDescanso)))
                                            {
                                                context.tcitastaller.Add(cita);
                                            }
                                        }
                                    }
                                }

                                int guardar = context.SaveChanges();
                                if (guardar > 0)
                                {
                                    return true;
                                }

                                return false;
                            }

                            return false;
                        }

                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.martes_hasta ?? new TimeSpan()) <= 0)
                        {
                            if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.martes_desde ?? new TimeSpan()) >= 0)
                            {
                                // Se busca si el usuario puede atender luego del descanso de medio dia y se divide la cita en una parte por la mañana y una por la tarde
                                TimeSpan horaEntradaMedioDia = buscarHorarioPerito.martes_desde2 ?? new TimeSpan();
                                TimeSpan horaSalidaMedioDia = buscarHorarioPerito.martes_hasta ?? new TimeSpan();
                                DateTime fechaActualSalida = fecha.Add(horaSalidaMedioDia);
                                DateTime fechaActualEntrada = fecha.Add(horaEntradaMedioDia);

                                citasDelDia.Add(
                                    new tcitastaller { desde = fechaActualSalida, hasta = fechaActualEntrada });

                                // Se valida la parte de por la mañana haber si esta libre
                                if (!ExisteCitaEntreDosHoras(citasDelDia, fecha.Add(horaInicio),
                                    fecha.Add(buscarHorarioPerito.martes_hasta ?? new TimeSpan())))
                                {
                                    // Se valida la parte de por la tarde haber si esta libre
                                    horasDuracionTotal = horaFin.Subtract(horaInicio);
                                    diferenciaDeHoras = horaSalidaMedioDia.Subtract(horaInicio);
                                    timeSpanAAgregarATarde =
                                        new TimeSpan(horasDuracionTotal.Hours - diferenciaDeHoras.Hours,
                                            horasDuracionTotal.Minutes - diferenciaDeHoras.Minutes, 0);
                                    TimeSpan horaFinalTarde = new TimeSpan(
                                        buscarHorarioPerito.martes_desde2 != null
                                            ? buscarHorarioPerito.martes_desde2.Value.Hours +
                                              timeSpanAAgregarATarde.Hours
                                            : 0,
                                        buscarHorarioPerito.martes_desde2 != null
                                            ? buscarHorarioPerito.martes_desde2.Value.Minutes +
                                              timeSpanAAgregarATarde.Minutes
                                            : 0, 0);

                                    if (buscarTiemposBodega.hora_final < horaFinalTarde ||
                                        buscarHorarioPerito.miercoles_desde2 == null)
                                    {
                                        return false;
                                    }

                                    if (!ExisteCitaEntreDosHoras(citasDelDia,
                                        fecha.Add(buscarHorarioPerito.martes_desde2 ?? new TimeSpan()),
                                        fecha.Add(horaFinalTarde)))
                                    {
                                        cita.desde = fecha.Add(horaInicio);
                                        cita.hasta = fecha.Add(horaFinalTarde);
                                        if (actualizarCita)
                                        {
                                            int bodegaBahia = context.tbahias.FirstOrDefault(x =>
                                                x.id == bahia && x.bodega == bodega && x.idtecnico == usuarioId).id;
                                            cita.bahia = bodegaBahia;
                                            context.Entry(cita).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            context.tcitastaller.Add(cita);
                                        }

                                        int guardar = context.SaveChanges();
                                        if (guardar > 0)
                                        {
                                            return true;
                                        }

                                        return false;
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    if (buscarHorarioPerito.martes_desde2 != null && buscarHorarioPerito.martes_hasta2 != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.martes_desde2 ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.martes_hasta2 ?? new TimeSpan()) <= 0)
                        {
                            if (buscarTiemposBodega.hora_final < horaFin)
                            {
                                return false;
                            }

                            if (actualizarCita)
                            {
                                int bodegaBahia = context.tbahias.FirstOrDefault(x =>
                                    x.id == bahia && x.bodega == bodega && x.idtecnico == usuarioId).id;
                                cita.bahia = bodegaBahia;
                                context.Entry(cita).State = EntityState.Modified;
                            }
                            else
                            {
                                context.tcitastaller.Add(cita);
                            }

                            int guardar = context.SaveChanges();
                            if (guardar > 0)
                            {
                                return true;
                            }

                            return false;
                        }
                    }

                    return false;

                    #endregion
                }

                // Si la cita es para el dia "MIERCOLES" se valida si el horario funciona por la mañana o por la tarde
                if (fecha.DayOfWeek == DayOfWeek.Wednesday)
                {
                    #region Miercoles

                    if (buscarHorarioPerito.miercoles_desde != null && buscarHorarioPerito.miercoles_hasta != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.miercoles_desde ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.miercoles_hasta ?? new TimeSpan()) <= 0)
                        {
                            if (!ExisteCitaEntreDosHoras(citasDelDia, inicio, fin))
                            {
                                if (actualizarCita)
                                {
                                    int bodegaBahia = context.tbahias.FirstOrDefault(x =>
                                        x.id == bahia && x.bodega == bodega && x.idtecnico == usuarioId).id;
                                    cita.bahia = bodegaBahia;
                                    context.Entry(cita).State = EntityState.Modified;
                                }
                                else
                                {
                                    if (TimeSpan.Compare(descansito, new TimeSpan()) == 0)
                                    {
                                        if (buscarTiemposBodega.hora_final < horaFinalDescanso ||
                                            buscarDescansosPerito.findescanso == null)
                                        {
                                            return false;
                                        }

                                        if (!ExisteCitaEntreDosHoras(citasDelDia, cita.desde, cita.hasta))
                                        {
                                            context.tcitastaller.Add(cita);
                                        }
                                    }
                                    else
                                    {
                                        citasDelDia.Add(new tcitastaller
                                        { desde = fechaActualSalidaDescanso, hasta = fechaActualEntradaDescanso });
                                        if (!ExisteCitaEntreDosHoras(citasDelDia, fecha.Add(horaInicio),
                                            fecha.Add(buscarDescansosPerito.iniciodescanso ?? new TimeSpan())))
                                        {
                                            if (buscarTiemposBodega.hora_final < horaFinalDescanso ||
                                                buscarDescansosPerito.findescanso == null)
                                            {
                                                return false;
                                            }

                                            if (!ExisteCitaEntreDosHoras(citasDelDia,
                                                fecha.Add(buscarDescansosPerito.findescanso ?? new TimeSpan()),
                                                fecha.Add(horaFinalDescanso)))
                                            {
                                                context.tcitastaller.Add(cita);
                                            }
                                        }
                                    }
                                }

                                int guardar = context.SaveChanges();
                                if (guardar > 0)
                                {
                                    return true;
                                }

                                return false;
                            }

                            return false;
                        }

                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.lunes_hasta ?? new TimeSpan()) <= 0)
                        {
                            if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.miercoles_desde ?? new TimeSpan()) >=
                                0)
                            {
                                // Se busca si el usuario puede atender luego del descanso de medio dia y se divide la cita en una parte por la mañana y una por la tarde
                                TimeSpan horaEntradaMedioDia = buscarHorarioPerito.miercoles_desde2 ?? new TimeSpan();
                                TimeSpan horaSalidaMedioDia = buscarHorarioPerito.miercoles_hasta ?? new TimeSpan();
                                DateTime fechaActualSalida = fecha.Add(horaSalidaMedioDia);
                                DateTime fechaActualEntrada = fecha.Add(horaEntradaMedioDia);

                                citasDelDia.Add(
                                    new tcitastaller { desde = fechaActualSalida, hasta = fechaActualEntrada });

                                // Se valida la parte de por la mañana haber si esta libre
                                if (!ExisteCitaEntreDosHoras(citasDelDia, fecha.Add(horaInicio),
                                    fecha.Add(buscarHorarioPerito.miercoles_hasta ?? new TimeSpan())))
                                {
                                    // Se valida la parte de por la tarde haber si esta libre
                                    horasDuracionTotal = horaFin.Subtract(horaInicio);
                                    diferenciaDeHoras = horaSalidaMedioDia.Subtract(horaInicio);
                                    timeSpanAAgregarATarde =
                                        new TimeSpan(horasDuracionTotal.Hours - diferenciaDeHoras.Hours,
                                            horasDuracionTotal.Minutes - diferenciaDeHoras.Minutes, 0);
                                    TimeSpan horaFinalTarde = new TimeSpan(
                                        buscarHorarioPerito.miercoles_desde2 != null
                                            ? buscarHorarioPerito.miercoles_desde2.Value.Hours +
                                              timeSpanAAgregarATarde.Hours
                                            : 0,
                                        buscarHorarioPerito.miercoles_desde2 != null
                                            ? buscarHorarioPerito.miercoles_desde2.Value.Minutes +
                                              timeSpanAAgregarATarde.Minutes
                                            : 0, 0);

                                    if (buscarTiemposBodega.hora_final < horaFinalTarde ||
                                        buscarHorarioPerito.miercoles_desde2 == null)
                                    {
                                        return false;
                                    }

                                    if (!ExisteCitaEntreDosHoras(citasDelDia,
                                        fecha.Add(buscarHorarioPerito.miercoles_desde2 ?? new TimeSpan()),
                                        fecha.Add(horaFinalTarde)))
                                    {
                                        cita.desde = fecha.Add(horaInicio);
                                        cita.hasta = fecha.Add(horaFinalTarde);
                                        if (actualizarCita)
                                        {
                                            int bodegaBahia = context.tbahias.FirstOrDefault(x =>
                                                x.id == bahia && x.bodega == bodega && x.idtecnico == usuarioId).id;
                                            cita.bahia = bodegaBahia;
                                            context.Entry(cita).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            context.tcitastaller.Add(cita);
                                        }

                                        int guardar = context.SaveChanges();
                                        if (guardar > 0)
                                        {
                                            return true;
                                        }

                                        return false;
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    if (buscarHorarioPerito.miercoles_desde2 != null && buscarHorarioPerito.miercoles_hasta2 != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.miercoles_desde2 ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.miercoles_hasta2 ?? new TimeSpan()) <= 0)
                        {
                            if (buscarTiemposBodega.hora_final < horaFin)
                            {
                                return false;
                            }

                            if (actualizarCita)
                            {
                                int bodegaBahia = context.tbahias.FirstOrDefault(x =>
                                    x.id == bahia && x.bodega == bodega && x.idtecnico == usuarioId).id;
                                cita.bahia = bodegaBahia;
                                context.Entry(cita).State = EntityState.Modified;
                            }
                            else
                            {
                                context.tcitastaller.Add(cita);
                            }

                            int guardar = context.SaveChanges();
                            if (guardar > 0)
                            {
                                return true;
                            }

                            return false;
                        }
                    }

                    return false;

                    #endregion
                }

                // Si la cita es para el dia "JUEVES" se valida si el horario funciona por la mañana o por la tarde
                if (fecha.DayOfWeek == DayOfWeek.Thursday)
                {
                    #region Jueves

                    if (buscarHorarioPerito.jueves_desde != null && buscarHorarioPerito.jueves_hasta != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.jueves_desde ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.jueves_hasta ?? new TimeSpan()) <= 0)
                        {
                            if (!ExisteCitaEntreDosHoras(citasDelDia, inicio, fin))
                            {
                                if (actualizarCita)
                                {
                                    int bodegaBahia = context.tbahias.FirstOrDefault(x =>
                                        x.id == bahia && x.bodega == bodega && x.idtecnico == usuarioId).id;
                                    cita.bahia = bodegaBahia;
                                    context.Entry(cita).State = EntityState.Modified;
                                }
                                else
                                {
                                    if (TimeSpan.Compare(descansito, new TimeSpan()) == 0)
                                    {
                                        if (buscarTiemposBodega.hora_final < horaFinalDescanso ||
                                            buscarDescansosPerito.findescanso == null)
                                        {
                                            return false;
                                        }

                                        if (!ExisteCitaEntreDosHoras(citasDelDia, cita.desde, cita.hasta))
                                        {
                                            context.tcitastaller.Add(cita);
                                        }
                                    }
                                    else
                                    {
                                        citasDelDia.Add(new tcitastaller
                                        { desde = fechaActualSalidaDescanso, hasta = fechaActualEntradaDescanso });
                                        if (!ExisteCitaEntreDosHoras(citasDelDia, fecha.Add(horaInicio),
                                            fecha.Add(buscarDescansosPerito.iniciodescanso ?? new TimeSpan())))
                                        {
                                            if (buscarTiemposBodega.hora_final < horaFinalDescanso ||
                                                buscarDescansosPerito.findescanso == null)
                                            {
                                                return false;
                                            }

                                            if (!ExisteCitaEntreDosHoras(citasDelDia,
                                                fecha.Add(buscarDescansosPerito.findescanso ?? new TimeSpan()),
                                                fecha.Add(horaFinalDescanso)))
                                            {
                                                context.tcitastaller.Add(cita);
                                            }
                                        }
                                    }
                                }

                                int guardar = context.SaveChanges();
                                if (guardar > 0)
                                {
                                    return true;
                                }

                                return false;
                            }

                            return false;
                        }

                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.lunes_hasta ?? new TimeSpan()) <= 0)
                        {
                            if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.jueves_desde ?? new TimeSpan()) >= 0)
                            {
                                // Se busca si el usuario puede atender luego del descanso de medio dia y se divide la cita en una parte por la mañana y una por la tarde
                                TimeSpan horaEntradaMedioDia = buscarHorarioPerito.jueves_desde2 ?? new TimeSpan();
                                TimeSpan horaSalidaMedioDia = buscarHorarioPerito.jueves_hasta ?? new TimeSpan();
                                DateTime fechaActualSalida = fecha.Add(horaSalidaMedioDia);
                                DateTime fechaActualEntrada = fecha.Add(horaEntradaMedioDia);

                                citasDelDia.Add(
                                    new tcitastaller { desde = fechaActualSalida, hasta = fechaActualEntrada });

                                // Se valida la parte de por la mañana haber si esta libre
                                if (!ExisteCitaEntreDosHoras(citasDelDia, fecha.Add(horaInicio),
                                    fecha.Add(buscarHorarioPerito.jueves_hasta ?? new TimeSpan())))
                                {
                                    // Se valida la parte de por la tarde haber si esta libre
                                    horasDuracionTotal = horaFin.Subtract(horaInicio);
                                    diferenciaDeHoras = horaSalidaMedioDia.Subtract(horaInicio);
                                    timeSpanAAgregarATarde =
                                        new TimeSpan(horasDuracionTotal.Hours - diferenciaDeHoras.Hours,
                                            horasDuracionTotal.Minutes - diferenciaDeHoras.Minutes, 0);
                                    TimeSpan horaFinalTarde = new TimeSpan(
                                        buscarHorarioPerito.jueves_desde2 != null
                                            ? buscarHorarioPerito.jueves_desde2.Value.Hours +
                                              timeSpanAAgregarATarde.Hours
                                            : 0,
                                        buscarHorarioPerito.jueves_desde2 != null
                                            ? buscarHorarioPerito.jueves_desde2.Value.Minutes +
                                              timeSpanAAgregarATarde.Minutes
                                            : 0, 0);

                                    if (buscarTiemposBodega.hora_final < horaFinalTarde ||
                                        buscarHorarioPerito.jueves_desde2 == null)
                                    {
                                        return false;
                                    }

                                    if (!ExisteCitaEntreDosHoras(citasDelDia,
                                        fecha.Add(buscarHorarioPerito.jueves_desde2 ?? new TimeSpan()),
                                        fecha.Add(horaFinalTarde)))
                                    {
                                        cita.desde = fecha.Add(horaInicio);
                                        cita.hasta = fecha.Add(horaFinalTarde);
                                        if (actualizarCita)
                                        {
                                            int bodegaBahia = context.tbahias.FirstOrDefault(x =>
                                                x.id == bahia && x.bodega == bodega && x.idtecnico == usuarioId).id;
                                            cita.bahia = bodegaBahia;
                                            context.Entry(cita).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            context.tcitastaller.Add(cita);
                                        }

                                        int guardar = context.SaveChanges();
                                        if (guardar > 0)
                                        {
                                            return true;
                                        }

                                        return false;
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    if (buscarHorarioPerito.jueves_desde2 != null && buscarHorarioPerito.jueves_hasta2 != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.jueves_desde2 ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.jueves_hasta2 ?? new TimeSpan()) <= 0)
                        {
                            if (buscarTiemposBodega.hora_final < horaFin)
                            {
                                return false;
                            }

                            if (actualizarCita)
                            {
                                int bodegaBahia = context.tbahias.FirstOrDefault(x =>
                                    x.id == bahia && x.bodega == bodega && x.idtecnico == usuarioId).id;
                                cita.bahia = bodegaBahia;
                                context.Entry(cita).State = EntityState.Modified;
                            }
                            else
                            {
                                context.tcitastaller.Add(cita);
                            }

                            int guardar = context.SaveChanges();
                            if (guardar > 0)
                            {
                                return true;
                            }

                            return false;
                        }
                    }

                    return false;

                    #endregion
                }

                // Si la cita es para el dia "VIERNES" se valida si el horario funciona por la mañana o por la tarde
                if (fecha.DayOfWeek == DayOfWeek.Friday)
                {
                    #region Viernes

                    if (buscarHorarioPerito.viernes_desde != null && buscarHorarioPerito.viernes_hasta != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.viernes_desde ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.viernes_hasta ?? new TimeSpan()) <= 0)
                        {
                            if (!ExisteCitaEntreDosHoras(citasDelDia, inicio, fin))
                            {
                                if (actualizarCita)
                                {
                                    int bodegaBahia = context.tbahias.FirstOrDefault(x =>
                                        x.id == bahia && x.bodega == bodega && x.idtecnico == usuarioId).id;
                                    cita.bahia = bodegaBahia;
                                    context.Entry(cita).State = EntityState.Modified;
                                }
                                else
                                {
                                    if (TimeSpan.Compare(descansito, new TimeSpan()) == 0)
                                    {
                                        if (buscarTiemposBodega.hora_final < horaFinalDescanso ||
                                            buscarDescansosPerito.findescanso == null)
                                        {
                                            return false;
                                        }

                                        if (!ExisteCitaEntreDosHoras(citasDelDia, cita.desde, cita.hasta))
                                        {
                                            context.tcitastaller.Add(cita);
                                        }
                                    }
                                    else
                                    {
                                        citasDelDia.Add(new tcitastaller
                                        { desde = fechaActualSalidaDescanso, hasta = fechaActualEntradaDescanso });
                                        if (!ExisteCitaEntreDosHoras(citasDelDia, fecha.Add(horaInicio),
                                            fecha.Add(buscarDescansosPerito.iniciodescanso ?? new TimeSpan())))
                                        {
                                            if (buscarTiemposBodega.hora_final < horaFinalDescanso ||
                                                buscarDescansosPerito.findescanso == null)
                                            {
                                                return false;
                                            }

                                            if (!ExisteCitaEntreDosHoras(citasDelDia,
                                                fecha.Add(buscarDescansosPerito.findescanso ?? new TimeSpan()),
                                                fecha.Add(horaFinalDescanso)))
                                            {
                                                context.tcitastaller.Add(cita);
                                            }
                                        }
                                    }
                                }

                                int guardar = context.SaveChanges();
                                if (guardar > 0)
                                {
                                    return true;
                                }

                                return false;
                            }

                            return false;
                        }

                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.lunes_hasta ?? new TimeSpan()) <= 0)
                        {
                            if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.viernes_desde ?? new TimeSpan()) >= 0)
                            {
                                // Se busca si el usuario puede atender luego del descanso de medio dia y se divide la cita en una parte por la mañana y una por la tarde
                                TimeSpan horaEntradaMedioDia = buscarHorarioPerito.viernes_desde2 ?? new TimeSpan();
                                TimeSpan horaSalidaMedioDia = buscarHorarioPerito.viernes_hasta ?? new TimeSpan();
                                DateTime fechaActualSalida = fecha.Add(horaSalidaMedioDia);
                                DateTime fechaActualEntrada = fecha.Add(horaEntradaMedioDia);

                                citasDelDia.Add(
                                    new tcitastaller { desde = fechaActualSalida, hasta = fechaActualEntrada });

                                // Se valida la parte de por la mañana haber si esta libre
                                if (!ExisteCitaEntreDosHoras(citasDelDia, fecha.Add(horaInicio),
                                    fecha.Add(buscarHorarioPerito.viernes_hasta ?? new TimeSpan())))
                                {
                                    // Se valida la parte de por la tarde haber si esta libre
                                    horasDuracionTotal = horaFin.Subtract(horaInicio);
                                    diferenciaDeHoras = horaSalidaMedioDia.Subtract(horaInicio);
                                    timeSpanAAgregarATarde =
                                        new TimeSpan(horasDuracionTotal.Hours - diferenciaDeHoras.Hours,
                                            horasDuracionTotal.Minutes - diferenciaDeHoras.Minutes, 0);
                                    TimeSpan horaFinalTarde = new TimeSpan(
                                        buscarHorarioPerito.viernes_desde2 != null
                                            ? buscarHorarioPerito.viernes_desde2.Value.Hours +
                                              timeSpanAAgregarATarde.Hours
                                            : 0,
                                        buscarHorarioPerito.viernes_desde2 != null
                                            ? buscarHorarioPerito.viernes_desde2.Value.Minutes +
                                              timeSpanAAgregarATarde.Minutes
                                            : 0, 0);

                                    if (buscarTiemposBodega.hora_final < horaFinalTarde ||
                                        buscarHorarioPerito.viernes_desde2 == null)
                                    {
                                        return false;
                                    }

                                    if (!ExisteCitaEntreDosHoras(citasDelDia,
                                        fecha.Add(buscarHorarioPerito.viernes_desde2 ?? new TimeSpan()),
                                        fecha.Add(horaFinalTarde)))
                                    {
                                        cita.desde = fecha.Add(horaInicio);
                                        cita.hasta = fecha.Add(horaFinalTarde);
                                        if (actualizarCita)
                                        {
                                            int bodegaBahia = context.tbahias.FirstOrDefault(x =>
                                                x.id == bahia && x.bodega == bodega && x.idtecnico == usuarioId).id;
                                            cita.bahia = bodegaBahia;
                                            context.Entry(cita).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            context.tcitastaller.Add(cita);
                                        }

                                        int guardar = context.SaveChanges();
                                        if (guardar > 0)
                                        {
                                            return true;
                                        }

                                        return false;
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    if (buscarHorarioPerito.viernes_desde2 != null && buscarHorarioPerito.viernes_hasta2 != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.viernes_desde2 ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.viernes_hasta2 ?? new TimeSpan()) <= 0)
                        {
                            if (buscarTiemposBodega.hora_final < horaFin)
                            {
                                return false;
                            }

                            if (actualizarCita)
                            {
                                int bodegaBahia = context.tbahias.FirstOrDefault(x =>
                                    x.id == bahia && x.bodega == bodega && x.idtecnico == usuarioId).id;
                                cita.bahia = bodegaBahia;
                                context.Entry(cita).State = EntityState.Modified;
                            }
                            else
                            {
                                context.tcitastaller.Add(cita);
                            }

                            int guardar = context.SaveChanges();
                            if (guardar > 0)
                            {
                                return true;
                            }

                            return false;
                        }
                    }

                    return false;

                    #endregion
                }

                // Si la cita es para el dia "SABADO" se valida si el horario funciona por la mañana o por la tarde
                if (fecha.DayOfWeek == DayOfWeek.Saturday)
                {
                    #region Sabado

                    if (buscarHorarioPerito.sabado_desde != null && buscarHorarioPerito.sabado_hasta != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.sabado_desde ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.sabado_hasta ?? new TimeSpan()) <= 0)
                        {
                            if (!ExisteCitaEntreDosHoras(citasDelDia, inicio, fin))
                            {
                                if (actualizarCita)
                                {
                                    int bodegaBahia = context.tbahias.FirstOrDefault(x =>
                                        x.id == bahia && x.bodega == bodega && x.idtecnico == usuarioId).id;
                                    cita.bahia = bodegaBahia;
                                    context.Entry(cita).State = EntityState.Modified;
                                }
                                else
                                {
                                    if (TimeSpan.Compare(descansito, new TimeSpan()) == 0)
                                    {
                                        if (buscarTiemposBodega.hora_final < horaFinalDescanso ||
                                            buscarDescansosPerito.findescanso == null)
                                        {
                                            return false;
                                        }

                                        if (!ExisteCitaEntreDosHoras(citasDelDia, cita.desde, cita.hasta))
                                        {
                                            context.tcitastaller.Add(cita);
                                        }
                                    }
                                    else
                                    {
                                        citasDelDia.Add(new tcitastaller
                                        { desde = fechaActualSalidaDescanso, hasta = fechaActualEntradaDescanso });
                                        if (!ExisteCitaEntreDosHoras(citasDelDia, fecha.Add(horaInicio),
                                            fecha.Add(buscarDescansosPerito.iniciodescanso ?? new TimeSpan())))
                                        {
                                            if (buscarTiemposBodega.hora_final < horaFinalDescanso ||
                                                buscarDescansosPerito.findescanso == null)
                                            {
                                                return false;
                                            }

                                            if (!ExisteCitaEntreDosHoras(citasDelDia,
                                                fecha.Add(buscarDescansosPerito.findescanso ?? new TimeSpan()),
                                                fecha.Add(horaFinalDescanso)))
                                            {
                                                context.tcitastaller.Add(cita);
                                            }
                                        }
                                    }
                                }

                                int guardar = context.SaveChanges();
                                if (guardar > 0)
                                {
                                    return true;
                                }

                                return false;
                            }

                            return false;
                        }

                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.lunes_hasta ?? new TimeSpan()) <= 0)
                        {
                            if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.sabado_desde ?? new TimeSpan()) >= 0)
                            {
                                // Se busca si el usuario puede atender luego del descanso de medio dia y se divide la cita en una parte por la mañana y una por la tarde
                                TimeSpan horaEntradaMedioDia = buscarHorarioPerito.sabado_desde2 ?? new TimeSpan();
                                TimeSpan horaSalidaMedioDia = buscarHorarioPerito.sabado_hasta ?? new TimeSpan();
                                DateTime fechaActualSalida = fecha.Add(horaSalidaMedioDia);
                                DateTime fechaActualEntrada = fecha.Add(horaEntradaMedioDia);

                                citasDelDia.Add(
                                    new tcitastaller { desde = fechaActualSalida, hasta = fechaActualEntrada });

                                // Se valida la parte de por la mañana haber si esta libre
                                if (!ExisteCitaEntreDosHoras(citasDelDia, fecha.Add(horaInicio),
                                    fecha.Add(buscarHorarioPerito.sabado_hasta ?? new TimeSpan())))
                                {
                                    // Se valida la parte de por la tarde haber si esta libre
                                    horasDuracionTotal = horaFin.Subtract(horaInicio);
                                    diferenciaDeHoras = horaSalidaMedioDia.Subtract(horaInicio);
                                    timeSpanAAgregarATarde =
                                        new TimeSpan(horasDuracionTotal.Hours - diferenciaDeHoras.Hours,
                                            horasDuracionTotal.Minutes - diferenciaDeHoras.Minutes, 0);
                                    TimeSpan horaFinalTarde = new TimeSpan(
                                        buscarHorarioPerito.sabado_desde2 != null
                                            ? buscarHorarioPerito.sabado_desde2.Value.Hours +
                                              timeSpanAAgregarATarde.Hours
                                            : 0,
                                        buscarHorarioPerito.sabado_desde2 != null
                                            ? buscarHorarioPerito.sabado_desde2.Value.Minutes +
                                              timeSpanAAgregarATarde.Minutes
                                            : 0, 0);

                                    if (buscarTiemposBodega.hora_final < horaFinalTarde ||
                                        buscarHorarioPerito.sabado_desde2 == null)
                                    {
                                        return false;
                                    }

                                    if (!ExisteCitaEntreDosHoras(citasDelDia,
                                        fecha.Add(buscarHorarioPerito.sabado_desde2 ?? new TimeSpan()),
                                        fecha.Add(horaFinalTarde)))
                                    {
                                        cita.desde = fecha.Add(horaInicio);
                                        cita.hasta = fecha.Add(horaFinalTarde);
                                        if (actualizarCita)
                                        {
                                            int bodegaBahia = context.tbahias.FirstOrDefault(x =>
                                                x.id == bahia && x.bodega == bodega && x.idtecnico == usuarioId).id;
                                            cita.bahia = bodegaBahia;
                                            context.Entry(cita).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            context.tcitastaller.Add(cita);
                                        }

                                        int guardar = context.SaveChanges();
                                        if (guardar > 0)
                                        {
                                            return true;
                                        }

                                        return false;
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    if (buscarHorarioPerito.sabado_desde2 != null && buscarHorarioPerito.sabado_hasta2 != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.sabado_desde2 ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.sabado_hasta2 ?? new TimeSpan()) <= 0)
                        {
                            if (buscarTiemposBodega.hora_final < horaFin)
                            {
                                return false;
                            }

                            if (actualizarCita)
                            {
                                int bodegaBahia = context.tbahias.FirstOrDefault(x =>
                                    x.id == bahia && x.bodega == bodega && x.idtecnico == usuarioId).id;
                                cita.bahia = bodegaBahia;
                                context.Entry(cita).State = EntityState.Modified;
                            }
                            else
                            {
                                context.tcitastaller.Add(cita);
                            }

                            int guardar = context.SaveChanges();
                            if (guardar > 0)
                            {
                                return true;
                            }

                            return false;
                        }
                    }

                    return false;

                    #endregion
                }

                // Si la cita es para el dia "DOMINGO" se valida si el horario funciona por la mañana o por la tarde
                if (fecha.DayOfWeek == DayOfWeek.Sunday)
                {
                    #region Domingo

                    if (buscarHorarioPerito.domingo_desde != null && buscarHorarioPerito.domingo_hasta != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.domingo_desde ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.domingo_hasta ?? new TimeSpan()) <= 0)
                        {
                            if (!ExisteCitaEntreDosHoras(citasDelDia, inicio, fin))
                            {
                                if (actualizarCita)
                                {
                                    int bodegaBahia = context.tbahias.FirstOrDefault(x =>
                                        x.id == bahia && x.bodega == bodega && x.idtecnico == usuarioId).id;
                                    cita.bahia = bodegaBahia;
                                    context.Entry(cita).State = EntityState.Modified;
                                }
                                else
                                {
                                    if (TimeSpan.Compare(descansito, new TimeSpan()) == 0)
                                    {
                                        if (buscarTiemposBodega.hora_final < horaFinalDescanso ||
                                            buscarDescansosPerito.findescanso == null)
                                        {
                                            return false;
                                        }

                                        if (!ExisteCitaEntreDosHoras(citasDelDia, cita.desde, cita.hasta))
                                        {
                                            context.tcitastaller.Add(cita);
                                        }
                                    }
                                    else
                                    {
                                        citasDelDia.Add(new tcitastaller
                                        { desde = fechaActualSalidaDescanso, hasta = fechaActualEntradaDescanso });
                                        if (!ExisteCitaEntreDosHoras(citasDelDia, fecha.Add(horaInicio),
                                            fecha.Add(buscarDescansosPerito.iniciodescanso ?? new TimeSpan())))
                                        {
                                            if (buscarTiemposBodega.hora_final < horaFinalDescanso ||
                                                buscarDescansosPerito.findescanso == null)
                                            {
                                                return false;
                                            }

                                            if (!ExisteCitaEntreDosHoras(citasDelDia,
                                                fecha.Add(buscarDescansosPerito.findescanso ?? new TimeSpan()),
                                                fecha.Add(horaFinalDescanso)))
                                            {
                                                context.tcitastaller.Add(cita);
                                            }
                                        }
                                    }
                                }

                                int guardar = context.SaveChanges();
                                if (guardar > 0)
                                {
                                    return true;
                                }

                                return false;
                            }

                            return false;
                        }

                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.lunes_hasta ?? new TimeSpan()) <= 0)
                        {
                            if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.domingo_desde ?? new TimeSpan()) >= 0)
                            {
                                // Se busca si el usuario puede atender luego del descanso de medio dia y se divide la cita en una parte por la mañana y una por la tarde
                                TimeSpan horaEntradaMedioDia = buscarHorarioPerito.domingo_desde2 ?? new TimeSpan();
                                TimeSpan horaSalidaMedioDia = buscarHorarioPerito.domingo_hasta ?? new TimeSpan();
                                DateTime fechaActualSalida = fecha.Add(horaSalidaMedioDia);
                                DateTime fechaActualEntrada = fecha.Add(horaEntradaMedioDia);

                                citasDelDia.Add(
                                    new tcitastaller { desde = fechaActualSalida, hasta = fechaActualEntrada });

                                // Se valida la parte de por la mañana haber si esta libre
                                if (!ExisteCitaEntreDosHoras(citasDelDia, fecha.Add(horaInicio),
                                    fecha.Add(buscarHorarioPerito.domingo_hasta ?? new TimeSpan())))
                                {
                                    // Se valida la parte de por la tarde haber si esta libre
                                    horasDuracionTotal = horaFin.Subtract(horaInicio);
                                    diferenciaDeHoras = horaSalidaMedioDia.Subtract(horaInicio);
                                    timeSpanAAgregarATarde =
                                        new TimeSpan(horasDuracionTotal.Hours - diferenciaDeHoras.Hours,
                                            horasDuracionTotal.Minutes - diferenciaDeHoras.Minutes, 0);
                                    TimeSpan horaFinalTarde = new TimeSpan(
                                        buscarHorarioPerito.domingo_desde2 != null
                                            ? buscarHorarioPerito.domingo_desde2.Value.Hours +
                                              timeSpanAAgregarATarde.Hours
                                            : 0,
                                        buscarHorarioPerito.domingo_desde2 != null
                                            ? buscarHorarioPerito.domingo_desde2.Value.Minutes +
                                              timeSpanAAgregarATarde.Minutes
                                            : 0, 0);

                                    if (buscarTiemposBodega.hora_final < horaFinalTarde ||
                                        buscarHorarioPerito.domingo_desde2 == null)
                                    {
                                        return false;
                                    }

                                    if (!ExisteCitaEntreDosHoras(citasDelDia,
                                        fecha.Add(buscarHorarioPerito.domingo_desde2 ?? new TimeSpan()),
                                        fecha.Add(horaFinalTarde)))
                                    {
                                        cita.desde = fecha.Add(horaInicio);
                                        cita.hasta = fecha.Add(horaFinalTarde);
                                        if (actualizarCita)
                                        {
                                            int bodegaBahia = context.tbahias.FirstOrDefault(x =>
                                                x.id == bahia && x.bodega == bodega && x.idtecnico == usuarioId).id;
                                            cita.bahia = bodegaBahia;
                                            context.Entry(cita).State = EntityState.Modified;
                                        }
                                        else
                                        {
                                            context.tcitastaller.Add(cita);
                                        }

                                        int guardar = context.SaveChanges();
                                        if (guardar > 0)
                                        {
                                            return true;
                                        }

                                        return false;
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    if (buscarHorarioPerito.domingo_desde2 != null && buscarHorarioPerito.domingo_hasta2 != null)
                    {
                        if (TimeSpan.Compare(horaInicio, buscarHorarioPerito.domingo_desde2 ?? new TimeSpan()) >= 0 &&
                            TimeSpan.Compare(horaFin, buscarHorarioPerito.domingo_hasta2 ?? new TimeSpan()) <= 0)
                        {
                            if (buscarTiemposBodega.hora_final < horaFin)
                            {
                                return false;
                            }

                            if (actualizarCita)
                            {
                                int bodegaBahia = context.tbahias.FirstOrDefault(x =>
                                    x.id == bahia && x.bodega == bodega && x.idtecnico == usuarioId).id;
                                cita.bahia = bodegaBahia;
                                context.Entry(cita).State = EntityState.Modified;
                            }
                            else
                            {
                                context.tcitastaller.Add(cita);
                            }

                            int guardar = context.SaveChanges();
                            if (guardar > 0)
                            {
                                return true;
                            }

                            return false;
                        }
                    }

                    return false;

                    #endregion
                }
            }
            else
            {
                // Significa que el perito no tiene horario guardado en base de datos
                return false;
            }

            //foreach (var cita in citasDelDia)
            //{
            //    if (DateTime.Compare(inicio, cita.desde) == 0)
            //    {
            //        return false;
            //    }
            //    else if (DateTime.Compare(inicio, cita.desde) < 0 && DateTime.Compare(fin, cita.desde) > 0)
            //    {
            //        return false;
            //    }
            //    else if (DateTime.Compare(inicio, cita.desde) > 0 && DateTime.Compare(inicio, cita.hasta) < 0)
            //    {
            //        return false;
            //    }
            //}

            return true;
        }

        public JsonResult ValidarEstadoSeleccionado(int? id_estado)
        {
            // Si el estado es 2 quiere decir cancelado en base de datos de la tabla tcitaestados
            if (id_estado == 11)
            {
                return Json(new { estado = "Cancelado" }, JsonRequestBehavior.AllowGet);
            }

            if (id_estado == 2)
            {
                return Json(new { estado = "Reprogramado" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { estado = "Sin estado" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ValidarPlacaConCitaAsignada(string placa)
        {
            DateTime fechaHoy = DateTime.Now;
            var buscarPlacaConCita = (from citas in context.tcitastaller
                                      join bahia in context.tbahias
                                          on citas.bahia equals bahia.id
                                      join bodega in context.bodega_concesionario
                                          on bahia.bodega equals bodega.id
                                      where DbFunctions.TruncateTime(citas.desde) >= DbFunctions.TruncateTime(fechaHoy) &&
                                            citas.placa == placa && citas.estadocita != 11
                                      select new
                                      {
                                          citas.desde,
                                          citas.hasta,
                                          bodega.bodccs_nombre
                                      }).FirstOrDefault();

            if (buscarPlacaConCita != null)
            {
                return Json(
                    new
                    {
                        placaConCita = true,
                        fecha = buscarPlacaConCita.desde.ToShortDateString(),
                        bodega = buscarPlacaConCita.bodccs_nombre
                    }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { placaConCita = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AgregarCita(int? id_cita, int? tipo_cita, int bahia, int bodega, int tecnico, string agente,
            int cliente, string placa, DateTime fecha, TimeSpan horaInicio, string tiempo, string tipoLlamada, int fuente,
            string operaciones,
            string sintomas, string notas, int estado, int? motivoEstado, int? id_tipo_inspeccion,
            string referenciasPlan, string operacionesPlan, /*int tipoTarifa,*/ string modelo_codigo, int? centroCosto,
            int? razonIngreso, int? averia, int? idmovprog, string observacionreprog)
        {
            if (Session["user_usuarioid"] != null)
            {
                DateTime fechaDesde = new DateTime(fecha.Year, fecha.Month, fecha.Day, horaInicio.Hours, horaInicio.Minutes,
                    horaInicio.Seconds);

                var horas = Math.Round(Convert.ToDecimal(tiempo));
                //conversion
                double hr = Convert.ToDouble(horas);

                DateTime fechaHasta = fechaDesde.AddHours(hr);

                ttecnicos buscarTecnico = context.ttecnicos.FirstOrDefault(x => x.id == tecnico);
                int tecnicoIdTablaUsers = 0;
                if (buscarTecnico != null)
                {
                    tecnicoIdTablaUsers = buscarTecnico != null ? buscarTecnico.idusuario : 0;
                }

                tcitastaller buscarCita = context.tcitastaller.FirstOrDefault(x => x.id == id_cita);
                if (buscarCita != null)
                {
                    //var guardar = context.SaveChanges();
                    //if (guardar > 0) {
                    //if (estado == 3)
                    //{
                    //    buscarCita.estadocita = estado;
                    //    context.Entry(buscarCita).State = EntityState.Modified;
                    //    var guardarCambios = context.SaveChanges();
                    //    if (guardarCambios > 0)
                    //    {
                    //        return Json(new { success = true, successMessage = "La cita se ha actualizado con exito" }, JsonRequestBehavior.AllowGet);
                    //    }
                    //    else
                    //    {
                    //        return Json(new { success = false, successMessage = "La cita no se ha actualizado" }, JsonRequestBehavior.AllowGet);
                    //    }
                    //}
                    buscarCita.tipocita =Convert.ToInt32(tipo_cita);
                    buscarCita.tecnico = tecnico;
                    //buscarCita.agente = Convert.ToInt32(Session["user_usuarioid"]);
                    buscarCita.cliente = cliente;
                    buscarCita.placa = placa;
                    buscarCita.tipollamada = tipoLlamada;
                    buscarCita.fuente = fuente;
                    buscarCita.tipotarifa = 1;
                    buscarCita.notas = notas;
                    buscarCita.estadocita = estado;
                    buscarCita.motivoestado = motivoEstado;
                    buscarCita.centrocosto = centroCosto;
                    buscarCita.razon_ingreso = razonIngreso;
                    if (estado == 2) //REPROGRAMADA
                    {
                        if (buscarCita.desde.Day == fechaDesde.Day && buscarCita.desde.Month == fechaDesde.Month &&
                            buscarCita.desde.Year == fechaDesde.Year && buscarCita.desde.Hour == fechaDesde.Hour &&
                            buscarCita.desde.Minute == fechaDesde.Minute && buscarCita.bahia == bahia)
                        {
                            return Json(
                                new
                                {
                                    success = false,
                                    errorMessage =
                                        "No se puede reprogramar una cita con la misma fecha y hora que tenia asignada."
                                }, JsonRequestBehavior.AllowGet);
                        }

                        buscarCita.bahia = bahia;
                        buscarCita.fecantiordesde = buscarCita.desde;
                        buscarCita.desde = fechaDesde;
                        buscarCita.hasta = fechaHasta;
                        if (idmovprog > 0)
                            {
                            buscarCita.idmovtoreprog = idmovprog;
                            buscarCita.observacionrprog = observacionreprog;
                            }

                        logCitastallereepprog logcitas = new logCitastallereepprog();
                        logcitas.idcita = id_cita;
                        logcitas.idmovreprog = idmovprog;
                        logcitas.fecha = DateTime.Now;
                        logcitas.iduser = Convert.ToInt32(Session["user_usuarioid"]);
                        context.logCitastallereepprog.Add(logcitas);
                        context.SaveChanges();


                        }
                    //se comentarea el siguiente else por que no se encuentra motivo logico para actualizar la hora al momento de cambiar el estado de la cita, el cambio de horario solo se debe dar cuando se reprograme la cita
                    //else
                    //{
                    //	buscarCita.bahia = bahia;
                    //	buscarCita.desde = fechaDesde;
                    //	buscarCita.hasta = fechaHasta;
                    //}

                    //if (estado == 3)//CONFIRMADA
                    //{
                    //	context.Entry(buscarCita).State = EntityState.Modified;
                    //
                    //	var guardar = context.SaveChanges();
                    //	if (guardar > 0)
                    //	{
                    //		return Json(new { success = true, successMessage = "La cita se ha actualizado con exito" }, JsonRequestBehavior.AllowGet);
                    //	}
                    //}

                    bool horarioDisponible = CalcularHorarioDisponible( /*tecnicoIdTablaUsers*/ tecnico, bahia, bodega,
                        fecha, fechaDesde, fechaHasta, buscarCita, true, buscarCita.desde, buscarCita.hasta);

                    if (horarioDisponible)
                    {
                        //context.tcitastaller.Add(busca);

                        //context.Entry(buscarCita).State = System.Data.Entity.EntityState.Modified;
                        //var gurdarCita = context.SaveChanges();
                        //if (gurdarCita > 0)
                        //{
                        //var buscarUltimaCita = context.tcitastaller.OrderByDescending(x => x.id).FirstOrDefault();
                        //if (buscarUltimaCita != null)
                        //{
                        operaciones = operaciones.Replace("[", "");
                        operaciones = operaciones.Replace("]", "");
                        operaciones = operaciones.Replace(@"\", "");
                        operaciones = operaciones.Replace(@"""", "");
                        string[] operacionesId = operaciones.Split(',');

                        sintomas = sintomas.Replace("[", "");
                        sintomas = sintomas.Replace("]", "");
                        sintomas = sintomas.Replace(@"\", "");
                        sintomas = sintomas.Replace(@"""", "");
                        string[] sintomasId = sintomas.Split(',');

                        const string query = "DELETE FROM [dbo].[tcitasoperacion] WHERE [idcita]={0}";
                        int rows = context.Database.ExecuteSqlCommand(query, buscarCita.id);
                        for (int i = 0; i < operacionesId.Length; i++)
                        {
                            string idOperacion = operacionesId[i];
                            if (!string.IsNullOrEmpty(idOperacion))
                            {
                                var oper = context.ttempario.Where(d => d.codigo == idOperacion).FirstOrDefault();
                                var tiempo2 = Convert.ToDecimal(oper.HoraCliente, new CultureInfo("en-US"));
                                context.tcitasoperacion.Add(new tcitasoperacion
                                {
                                    idcita = buscarCita.id,
                                    operacion = idOperacion,
                                    idcentro = centroCosto,
                                    tiempo= tiempo2,
                                });
                            }
                        }

                        const string query2 = "DELETE FROM [dbo].[tcitasintomas] WHERE [idcita]={0}";
                        int rows2 = context.Database.ExecuteSqlCommand(query2, buscarCita.id);
                        for (int j = 0; j < sintomasId.Length; j++)
                        {
                            string sintoma = sintomasId[j];
                            if (!string.IsNullOrEmpty(sintoma))
                            {
                                context.tcitasintomas.Add(new tcitasintomas
                                {
                                    idcita = buscarCita.id,
                                    sintomas = sintoma
                                });
                            }
                        }

                        string[] operacionesPlanVector = operacionesPlan.Split(',');
                        for (int j = 0; j < operacionesPlanVector.Length; j++)
                        {
                            string operacion = operacionesPlanVector[j];
                            if (!string.IsNullOrEmpty(operacion))
                            {
                                var oper2 = context.ttempario.Where(d => d.codigo == operacion).FirstOrDefault();
                                var tiempo22 = Convert.ToDecimal(oper2.HoraCliente, new CultureInfo("en-US"));
                                context.tcitasoperacion.Add(new tcitasoperacion
                                {
                                    idcita = buscarCita.id,
                                    operacion = operacion,
                                    idcentro = centroCosto,
                                    inspeccion = true,
                                    id_plan_mantenimiento = id_tipo_inspeccion,
                                    tiempo=tiempo22
                                });
                            }
                        }

                        const string query3 = "DELETE FROM [dbo].[tcitarepuestos] WHERE [idcita]={0}";
                        int rows3 = context.Database.ExecuteSqlCommand(query3, buscarCita.id);
                        string[] referenciasPlanVector = referenciasPlan.Split(',');
                        for (int j = 0; j < referenciasPlanVector.Length; j++)
                        {
                            string referencia = referenciasPlanVector[j];
                            if (!string.IsNullOrEmpty(referencia))
                            {
                                context.tcitarepuestos.Add(new tcitarepuestos
                                {
                                    idcita = buscarCita.id,
                                    idrepuesto = referencia,
                                    idcentro = centroCosto,
                                    id_plan_mantenimiento = id_tipo_inspeccion
                                });
                            }
                        }

                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            return Json(new { success = true, successMessage = "La cita se ha actualizado con exito" },
                                JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(
                            new
                            {
                                success = false,
                                errorMessage =
                                    "El horario de la cita ya esta asignado o el tecnico no trabaja en la hora establecida"
                            }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    //var nuevaCita = new tcitastaller()
                    //{
                    //	tipocita = tipo_cita,
                    //	bahia = bahia,
                    //	tecnico = tecnico,
                    //	agente = agente,
                    //	cliente = cliente,
                    //	placa = placa,
                    //	modelo = modelo_codigo,
                    //	desde = fechaDesde,
                    //	hasta = fechaHasta,
                    //	tipollamada = tipoLlamada,
                    //	fuente = fuente,
                    //	fec_creacion = DateTime.Now,
                    //	userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                    //	notas = notas,
                    //	estadocita = estado,
                    //	tipotarifa = tipoTarifa,
                    //	motivoestado = motivoEstado,
                    //	centrocosto = centroCosto,
                    //	id_tipo_inspeccion = id_tipo_inspeccion
                    //};
                    tcitastaller nuevaCita = new tcitastaller
                    {
                        tipocita = Convert.ToInt32(tipo_cita),
                        bahia = bahia,
                        tecnico = tecnico,
                        agente = Convert.ToInt32(Session["user_usuarioid"]),
                        cliente = cliente,
                        placa = placa,
                        modelo = modelo_codigo,
                        desde = fechaDesde,
                        hasta = fechaHasta
                    };
                    if (tipo_cita == 4)
                    {
                        //nuevaCita.tipotarifa = 6;
                        nuevaCita.fuente = 5;
                    }
                    else
                    {
                        //nuevaCita.tipotarifa = tipoTarifa;
                        nuevaCita.fuente = fuente;
                    }

                    nuevaCita.tipotarifa = 1;

                    nuevaCita.tipollamada = tipoLlamada;
                    nuevaCita.fec_creacion = DateTime.Now;
                    nuevaCita.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    nuevaCita.notas = notas;
                    nuevaCita.estadocita = estado;
                    nuevaCita.motivoestado = motivoEstado;
                    nuevaCita.centrocosto = centroCosto;
                    nuevaCita.id_tipo_inspeccion = id_tipo_inspeccion;
                    nuevaCita.razon_ingreso = razonIngreso;

                    bool horarioDisponible = CalcularHorarioDisponible( /*tecnicoIdTablaUsers*/ tecnico, bahia, bodega,
                        fecha, fechaDesde, fechaHasta, nuevaCita, false, fechaDesde, fechaHasta);

                    if (horarioDisponible)
                    {
                        //context.tcitastaller.Add(nuevaCita);
                        //var gurdarCita = context.SaveChanges();
                        //if (gurdarCita > 0)
                        //{
                        tcitastaller buscarUltimaCita = context.tcitastaller.OrderByDescending(x => x.id).FirstOrDefault();
                        if (buscarUltimaCita != null)
                        {
                            operaciones = operaciones.Replace("[", "");
                            operaciones = operaciones.Replace("]", "");
                            operaciones = operaciones.Replace(@"\", "");
                            operaciones = operaciones.Replace(@"""", "");
                            string[] operacionesId = operaciones.Split(',');

                            sintomas = sintomas.Replace("[", "");
                            sintomas = sintomas.Replace("]", "");
                            sintomas = sintomas.Replace(@"\", "");
                            sintomas = sintomas.Replace(@"""", "");
                            string[] sintomasId = sintomas.Split(',');

                            for (int i = 0; i < operacionesId.Length; i++)
                            {
                                string idOperacion = operacionesId[i];
                                if (!string.IsNullOrEmpty(idOperacion))
                                {
                                    context.tcitasoperacion.Add(new tcitasoperacion
                                    {
                                        idcita = buscarUltimaCita.id,
                                        operacion = idOperacion,
                                        idcentro = centroCosto,
                                        inspeccion = true
                                    });
                                }
                            }

                            for (int j = 0; j < sintomasId.Length; j++)
                            {
                                string sintoma = sintomasId[j];
                                if (!string.IsNullOrEmpty(sintoma))
                                {
                                    context.tcitasintomas.Add(new tcitasintomas
                                    {
                                        idcita = buscarUltimaCita.id,
                                        sintomas = sintoma
                                    });
                                }
                            }

                            string[] operacionesPlanVector = operacionesPlan.Split(',');
                            for (int j = 0; j < operacionesPlanVector.Length; j++)
                            {
                                string operacion = operacionesPlanVector[j];
                                if (!string.IsNullOrEmpty(operacion))
                                {
                                    context.tcitasoperacion.Add(new tcitasoperacion
                                    {
                                        idcita = buscarUltimaCita.id,
                                        operacion = operacion,
                                        idcentro = centroCosto,
                                        inspeccion = true,
                                        id_plan_mantenimiento = id_tipo_inspeccion
                                    });
                                }
                            }

                            //busco si es de accesorios 
                            icb_sysparameter paramaacce = context.icb_sysparameter.Where(d => d.syspar_cod == "P116")
                                .FirstOrDefault();
                            int razonaccesorio = paramaacce != null ? Convert.ToInt32(paramaacce.syspar_value) : 6;

                            int pedido = 0;
                            if (razonIngreso == razonaccesorio)
                            {
                                //veo si el vehiculo tiene pedido
                                vpedido existepedido = context.vpedido.Where(d =>
                                        (d.planmayor == placa || d.icb_vehiculo.plac_vh == placa) && d.anulado == false)
                                    .FirstOrDefault();
                                if (existepedido != null)
                                {
                                    pedido = existepedido.id;
                                    nuevaCita.cliente = existepedido.nit.Value;
                                    context.Entry(nuevaCita).State = EntityState.Modified;
                                }
                            }

                            string[] referenciasPlanVector = referenciasPlan.Split(',');
                            for (int j = 0; j < referenciasPlanVector.Length; j++)
                            {
                                string referencia = referenciasPlanVector[j];
                                if (!string.IsNullOrEmpty(referencia))
                                {
                                    tcitarepuestos nuevorepuesto = new tcitarepuestos
                                    {
                                        idcita = buscarUltimaCita.id,
                                        idrepuesto = referencia,
                                        idcentro = centroCosto,
                                        id_plan_mantenimiento = id_tipo_inspeccion
                                    };
                                    if (pedido != 0)
                                    {
                                        vpedrepuestos existerepuesto = context.vpedrepuestos.Where(d =>
                                                d.pedido_id == pedido && d.instalado == false &&
                                                d.referencia == referencia)
                                            .FirstOrDefault();
                                        if (existerepuesto != null)
                                        {
                                            nuevorepuesto.idpedidorepuesto = existerepuesto.id;
                                        }
                                    }

                                    context.tcitarepuestos.Add(nuevorepuesto);
                                }
                            }

                            //si es una agenda de tipo alistamiento debemos crear tambien el evento
                            string parametroCita = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P67")
                                .syspar_value;
                            if (parametroCita == tipo_cita.ToString())
                            {
                                int parametroEvento = Convert.ToInt32(context.icb_sysparameter
                                    .FirstOrDefault(x => x.syspar_cod == "P65").syspar_value);
                                icb_tpeventos evento =
                                    context.icb_tpeventos.FirstOrDefault(x => x.codigoevento == parametroEvento);
                                context.icb_vehiculo_eventos.Add(new icb_vehiculo_eventos
                                {
                                    planmayor = placa,
                                    eventofec_creacion = DateTime.Now,
                                    eventouserid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                    evento_nombre = evento.tpevento_nombre,
                                    evento_estado = evento.tpevento_estado,
                                    bodega_id = Convert.ToInt32(Session["user_bodega"]),
                                    id_tpevento = evento.tpevento_id,
                                    fechaevento = DateTime.Now
                                });
                            }

                            int guardar = context.SaveChanges();

                            //Los siguientes condicionales se realizan para validar si la cita va por averias y si es en ese caso 
                            //modificar las averias y actualizarles el idcita.
                            string planmayor = "";
                            long tallerOrden = context.tallerAveria.Where(x => x.nombre.Contains("orden")).FirstOrDefault().id;
                            if (averia != null && averia == 1)
                            {
                                icb_vehiculo vehiculoPlaca = context.icb_vehiculo.Where(x => x.plac_vh == nuevaCita.placa).FirstOrDefault();

                                if (vehiculoPlaca == null)
                                {
                                    icb_vehiculo vehiculoPlan = context.icb_vehiculo.Where(x => x.plan_mayor == nuevaCita.placa).FirstOrDefault();
                                    if (vehiculoPlan != null)
                                    {
                                        planmayor = vehiculoPlan.plan_mayor;
                                    }
                                }
                                else
                                {
                                    planmayor = vehiculoPlaca.plan_mayor;
                                }

                                if (planmayor != "")
                                {
                                    List<icb_inspeccionvehiculos> averias = context.icb_inspeccionvehiculos.Where(x => x.planmayor == planmayor && x.insp_solicitar == true && x.idcitataller == null && x.taller_averia_id == tallerOrden).ToList();

                                    for (int i = 0; i < averias.Count; i++)
                                    {
                                        int id = averias[i].insp_id;
                                        icb_inspeccionvehiculos buscarAveria = context.icb_inspeccionvehiculos.Find(id);
                                        buscarAveria.idcitataller = nuevaCita.id;
                                        context.Entry(buscarAveria).State = EntityState.Modified;
                                        context.SaveChanges();
                                    }
                                }

                            }

                            if (guardar > 0)
                            {
                                return Json(new { success = true, successMessage = "La cita se ha agregado con exito" },
                                    JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            return Json(
                                new
                                {
                                    success = false,
                                    errorMessage = "Error en la base de datos, por favor verifique la conexion"
                                }, JsonRequestBehavior.AllowGet);
                        }

                        //}
                    }
                    else
                    {
                        return Json(
                            new
                            {
                                success = false,
                                errorMessage =
                                    "El horario de la cita ya esta asignado, o el tiempo de la cita sobrepasa el horario del tecnico o de la bodega"
                            }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            else
            {
                return Json(
                    new
                    {
                        success = false,
                        errorMessage = "La cita no pudo ser asignada, la sesión del usuario ha finalizado"
                    }, JsonRequestBehavior.AllowGet);
            }

            return Json(
                new
                {
                    success = false,
                    errorMessage =
                        "La cita no pudo ser asignada, revise que los datos esten completos y la hora este disponible"
                }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarOperacionParaAgregar(string id, int id_bodega, int id_tercero)
        {
            tercero_cliente buscarCliente = context.tercero_cliente.FirstOrDefault(x => x.tercero_id == id_tercero);
            int id_cliente = buscarCliente != null ? buscarCliente.cltercero_id : 0;


            //busca las operaciones de ese plan de mantenimiento



            var tarifax = context.ttarifastaller.Where(d => d.bodega == id_bodega && d.tipotarifa == 1).FirstOrDefault();
            var ivan = Convert.ToDecimal(tarifax.iva, new CultureInfo("is-IS"));

            var buscarOperacionSQL = (from tempario in context.ttempario
                                      join iva in context.codigo_iva
                                          on tempario.iva equals iva.id
                                      where tempario.codigo == id
                                      select new
                                      {
                                          tempario.codigo,
                                          tempario.operacion,
                                          tiempo = tempario.HoraCliente != null ? tempario.HoraCliente :"0",
                                          precio =tarifax.valorhora,
                                          iva = ivan
                                      }).FirstOrDefault();


            if (buscarOperacionSQL != null)
            {
                var preciototalSinIva = (buscarOperacionSQL.precio??0)* Convert.ToDecimal(buscarOperacionSQL.tiempo, mCultura);
                var iva = buscarOperacionSQL.iva;
                var precioiva = (preciototalSinIva * iva) / 100;
                var total = preciototalSinIva + precioiva;
                var buscarOperacion = new
                {
                    buscarOperacionSQL.codigo,
                    buscarOperacionSQL.operacion,
                   tiempo = Convert.ToDecimal(buscarOperacionSQL.tiempo, mCultura),
                    preciohora = buscarOperacionSQL.precio != null ? buscarOperacionSQL.precio : 0,

                    precio =preciototalSinIva,
                    ivaOperacion = buscarOperacionSQL.iva,
                    valorIva = iva,
                    valorTotal =total,
                    valorTotalString=total.ToString("N2",new CultureInfo("is-IS"))
                };
                return Json(new { encontrado = true, buscarOperacion }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { encontrado = false }, JsonRequestBehavior.AllowGet);
        }

        /*public JsonResult BuscarOperacionParaAgregar(string id, int id_bodega, int id_tercero, int id_tipo_tarifa)
        {
            var buscarCliente = context.tercero_cliente.FirstOrDefault(x => x.tercero_id == id_tercero);
            var id_cliente = buscarCliente != null ? buscarCliente.cltercero_id : 0;
            var buscarOperacionSQL = (from tempario in context.ttempario
                                      join iva in context.codigo_iva
                                      on tempario.iva equals iva.id
                                      where tempario.codigo == id
                                      select new
                                      {
                                          tempario.codigo,
                                          tempario.operacion,
                                          tiempo = tempario.tiempo != null ? tempario.tiempo : 0,
                                          precio = tempario.precio,
                                          iva = iva.porcentaje
                                      }).FirstOrDefault();


            if (buscarOperacionSQL != null)
            {
                if (id_tipo_tarifa == 1)
                {
                    var buscarTarifaClienteBodega = (from tarifaCliente in context.ttarifaclientebodega
                                                     where tarifaCliente.idtcliente == id_cliente
                                                     select tarifaCliente).FirstOrDefault();
                    if (buscarTarifaClienteBodega != null)
                    {
                        var buscarOperacion = new
                        {
                            buscarOperacionSQL.codigo,
                            buscarOperacionSQL.operacion,
                            buscarOperacionSQL.tiempo,
                            precio = buscarOperacionSQL.precio != null ? buscarOperacionSQL.precio : buscarTarifaClienteBodega != null ? buscarTarifaClienteBodega.valor : 0,
                            ivaOperacion = buscarOperacionSQL.iva != null ? buscarOperacionSQL.iva : 0,
                            valorIva = buscarOperacionSQL.precio != null ? ((buscarOperacionSQL.precio * buscarOperacionSQL.iva) / 100) * (decimal)buscarOperacionSQL.tiempo
                                    : buscarTarifaClienteBodega != null ? ((buscarTarifaClienteBodega.valor) * buscarOperacionSQL.iva / 100) * (decimal)buscarOperacionSQL.tiempo : 0,
                            valorTotal = (buscarOperacionSQL.precio != null ? buscarOperacionSQL.precio * (decimal)buscarOperacionSQL.tiempo
                                    : buscarTarifaClienteBodega != null ? (decimal)buscarOperacionSQL.tiempo * buscarTarifaClienteBodega.valor : 0)
                                        +
                                        (buscarOperacionSQL.precio != null ? ((buscarOperacionSQL.precio * buscarOperacionSQL.iva) / 100) * (decimal)buscarOperacionSQL.tiempo
                                    : buscarTarifaClienteBodega != null ? ((buscarTarifaClienteBodega.valor * buscarOperacionSQL.iva) / 100) * (decimal)buscarOperacionSQL.tiempo : 0)
                        };
                        return Json(new { encontrado = true, buscarOperacion }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var buscarTarifaBodega = (from tarifaTaller in context.ttarifastaller
                                                  where tarifaTaller.bodega == id_bodega && tarifaTaller.tipotarifa == id_tipo_tarifa
                                                  select tarifaTaller).FirstOrDefault();

                        var buscarOperacion = new
                        {
                            buscarOperacionSQL.codigo,
                            buscarOperacionSQL.operacion,
                            buscarOperacionSQL.tiempo,
                            precio = buscarOperacionSQL.precio != null ? buscarOperacionSQL.precio : buscarTarifaBodega != null ? buscarTarifaBodega.valorhora : 0,
                            ivaOperacion = buscarOperacionSQL.iva != null ? buscarOperacionSQL.iva : 0,
                            valorIva = buscarOperacionSQL.precio != null ? ((buscarOperacionSQL.precio * buscarOperacionSQL.iva) / 100) * (decimal)buscarOperacionSQL.tiempo
                                        : buscarTarifaBodega != null ? ((buscarTarifaBodega.valorhora * buscarOperacionSQL.iva) / 100) * (decimal)buscarOperacionSQL.tiempo : 0,
                            valorTotal = (buscarOperacionSQL.precio != null ? buscarOperacionSQL.precio * (decimal)buscarOperacionSQL.tiempo
                                            : buscarTarifaBodega != null ? buscarTarifaBodega.valorhora * (decimal)buscarOperacionSQL.tiempo : 0)
                                            +
                                            (buscarOperacionSQL.precio != null ? ((buscarOperacionSQL.precio * buscarOperacionSQL.iva) / 100) * (decimal)buscarOperacionSQL.tiempo
                                            : buscarTarifaBodega != null ? ((buscarTarifaBodega.valorhora * buscarOperacionSQL.iva) / 100) * (decimal)buscarOperacionSQL.tiempo : 0)
                        };
                        return Json(new { encontrado = true, buscarOperacion }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    var buscarTarifaBodega = (from tarifaTaller in context.ttarifastaller
                                              where tarifaTaller.bodega == id_bodega && tarifaTaller.tipotarifa == id_tipo_tarifa
                                              select tarifaTaller).FirstOrDefault();

                    var buscarOperacion = new
                    {
                        buscarOperacionSQL.codigo,
                        buscarOperacionSQL.operacion,
                        buscarOperacionSQL.tiempo,
                        precio = buscarOperacionSQL.precio != null ? buscarOperacionSQL.precio : buscarTarifaBodega != null ? buscarTarifaBodega.valorhora : 0,
                        ivaOperacion = buscarOperacionSQL.iva != null ? buscarOperacionSQL.iva : 0,
                        valorIva = buscarOperacionSQL.precio != null ? ((buscarOperacionSQL.precio * buscarOperacionSQL.iva) / 100) * (decimal)buscarOperacionSQL.tiempo
                                    : buscarTarifaBodega != null ? ((buscarTarifaBodega.valorhora * buscarOperacionSQL.iva) / 100) * (decimal)buscarOperacionSQL.tiempo : 0,
                        valorTotal = (buscarOperacionSQL.precio != null ? buscarOperacionSQL.precio * (decimal)buscarOperacionSQL.tiempo
                                        : buscarTarifaBodega != null ? buscarTarifaBodega.valorhora * (decimal)buscarOperacionSQL.tiempo : 0)
                                        +
                                        (buscarOperacionSQL.precio != null ? ((buscarOperacionSQL.precio * buscarOperacionSQL.iva) / 100) * (decimal)buscarOperacionSQL.tiempo
                                        : buscarTarifaBodega != null ? ((buscarTarifaBodega.valorhora * buscarOperacionSQL.iva) / 100) * (decimal)buscarOperacionSQL.tiempo : 0)
                    };
                    return Json(new { encontrado = true, buscarOperacion }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { encontrado = false }, JsonRequestBehavior.AllowGet);
            }
        }*/

        public JsonResult BuscarCliente(string cedula)
        {
            var buscarCliente = (from tercero in context.icb_terceros
                                 join cliente in context.tercero_cliente
                                     on tercero.tercero_id equals cliente.tercero_id into cli
                                 from cliente in cli.DefaultIfEmpty()
                                 where tercero.doc_tercero == cedula
                                 select new
                                 {
                                     tercero.tercero_id,
                                     nombres = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " + tercero.apellido_tercero +
                                               " " + tercero.segapellido_tercero + " " + tercero.razon_social,
                                     celular = tercero.celular_tercero,
                                     fijo = tercero.telf_tercero,
                                     telefonoCliente = cliente.telefono
                                 }).FirstOrDefault();

            if (buscarCliente != null)
            {
                var direcciones = (from direccion in context.terceros_direcciones
                                   where direccion.idtercero == buscarCliente.tercero_id
                                   select new
                                   {
                                       direccion.direccion
                                   }).ToList();

                List<string> telefonos = new List<string>();
                string Nombrecliente = buscarCliente != null ? buscarCliente.nombres : "";
                if (buscarCliente.celular != null)
                {
                    telefonos.Add(buscarCliente.celular);
                }

                if (buscarCliente.fijo != null)
                {
                    telefonos.Add(buscarCliente.fijo);
                }

                if (buscarCliente.telefonoCliente != null)
                {
                    telefonos.Add(buscarCliente.telefonoCliente);
                }

                return Json(new { encontrado = true, buscarCliente.tercero_id, direcciones, Nombrecliente, telefonos },
                    JsonRequestBehavior.AllowGet);
            }

            return Json(new { encontrado = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarTerceroPorCedula(string cedula)
        {
            icb_terceros buscarTercero = context.icb_terceros.FirstOrDefault(x => x.doc_tercero == cedula);
            int id_tercero = buscarTercero != null ? buscarTercero.tercero_id : 0;

            return Json(id_tercero, JsonRequestBehavior.AllowGet);
        }

        public JsonResult buscarXPlanMayor(string planmayor)
        {
            var buscarPlan = (from vehiculo in context.icb_vehiculo
                              join modelo in context.modelo_vehiculo
                                  on vehiculo.modvh_id equals modelo.modvh_codigo
                              join t in context.icb_terceros
                                  on vehiculo.propietario equals t.tercero_id into temp
                              from t in temp.DefaultIfEmpty()
                              where vehiculo.plan_mayor == planmayor
                              select new
                              {
                                  vehiculo.plan_mayor,
                                  modelo.modvh_nombre,
                                  modelo.modvh_codigo,
                                  vehiculo.propietario,
                                  t.doc_tercero,
                                  vehiculo.plac_vh,
                              }).FirstOrDefault();
            if (buscarPlan != null)
            {
                return Json(new { existe = true, buscarPlan }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { existe = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult BuscarPlaca(string placa)
        {
            var buscarPlaca = (from vehiculo in context.icb_vehiculo
                               join modelo in context.modelo_vehiculo
                                   on vehiculo.modvh_id equals modelo.modvh_codigo
                               join t in context.icb_terceros
                                   on vehiculo.propietario equals t.tercero_id into temp
                               from t in temp.DefaultIfEmpty()
                               where vehiculo.plac_vh == placa || vehiculo.plan_mayor == placa
                               select new
                               {
                                   vehiculo.plan_mayor,
                                   modelo.modvh_nombre,
                                   modelo.modvh_codigo,
                                   vehiculo.propietario,
                                   t.doc_tercero
                               }).FirstOrDefault();
            string cedulacliente = "";
            bool placaExiste = buscarPlaca != null ? true : false;
            if (buscarPlaca != null)
            {
                if (!string.IsNullOrWhiteSpace(buscarPlaca.doc_tercero))
                {
                    cedulacliente = buscarPlaca.doc_tercero;
                }
                else
                {
                    icb_vehiculo vehi = context.icb_vehiculo.Where(d => d.plan_mayor == buscarPlaca.plan_mayor).FirstOrDefault();
                    if (vehi.propietario != null)
                    {
                        cedulacliente = vehi.icb_terceros.doc_tercero;
                    }
                    else
                    {
                        //nit de comercializadora HOMAZ
                        icb_sysparameter param1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P33").FirstOrDefault();
                        int nitcliente = param1 != null ? Convert.ToInt32(param1.syspar_value) : 10;
                        icb_terceros tercerox = context.icb_terceros.Where(d => d.tercero_id == nitcliente).FirstOrDefault();
                        cedulacliente = tercerox.doc_tercero;
                    }
                }
            }


            return Json(new { placaExiste, buscarPlaca, cedulacliente }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarTecnicoPorBahia(int? id_bahia)
        {
            var buscarTecnico = (from bahia in context.tbahias
                                 join tecnico in context.ttecnicos
                                     on bahia.idtecnico equals tecnico.id
                                 join usuario in context.users
                                     on tecnico.idusuario equals usuario.user_id
                                 where bahia.id == id_bahia
                                 select new
                                 {
                                     //user_id = usuario.user_id,
                                     user_id = tecnico.id,
                                     usuario = "(" + bahia.codigo_bahia + ") " + usuario.user_nombre + " " + usuario.user_apellido
                                 }).FirstOrDefault();

            if (buscarTecnico == null)
                {
                return Json(0, JsonRequestBehavior.AllowGet);
                }
            else {

                return Json(buscarTecnico, JsonRequestBehavior.AllowGet);
                }
                               
        }


        public JsonResult TecnicosSinBahia() {

            var bahias = context.tbahias.Where(d => d.estado == true && d.idtecnico != null).Select(d => d.idtecnico).ToList();
            var buscarTecnico = (from tecnico in context.ttecnicos
                                 join bahia in context.tbahias
                                     on tecnico.id equals bahia.idtecnico into bar
                                 from bahia in bar.DefaultIfEmpty()
                                 join usuario in context.users
                                     on tecnico.idusuario equals usuario.user_id
                                 where !bahias.Contains(tecnico.id)
                                 select new
                                 {
                                     //user_id = usuario.user_id,
                                     user_id = tecnico.id,
                                     usuario = usuario.user_nombre + " " + usuario.user_apellido
                                 }).ToList();
            /*var buscarTecnico = (from tecnico in context.ttecnicos
                                 join bahia in context.tbahias
                                     on tecnico.id equals bahia.idtecnico into bar
                                 from bahia in bar.DefaultIfEmpty()
                                 join usuario in context.users
                                     on tecnico.idusuario equals usuario.user_id
                                 where bahia.id == null
                                 select new
                                     {
                                     //user_id = usuario.user_id,
                                     user_id = tecnico.id,
                                     usuario =  usuario.user_nombre + " " + usuario.user_apellido
                                     }).ToList();*/




            return Json(buscarTecnico, JsonRequestBehavior.AllowGet);
            }

        public JsonResult BuscarArea(int? id)
        {
            var buscarArea = (from bahia in context.tbahias
                              join tipoBahia in context.ttipobahia
                                  on bahia.tipo_bahia equals tipoBahia.id
                              where bahia.id == id
                              select new
                              {
                                  bahia.id,
                                  tipoBahia.descripcion
                              }).FirstOrDefault();

            return Json(buscarArea, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CargarAreas(int bodega)
        {
            var data = (from t in context.ttipobahia
                        join tb in context.tbahias
                            on t.id equals tb.tipo_bahia
                        where tb.bodega == bodega
                        select new
                        {
                            t.id,
                            t.descripcion
                        }).Distinct().ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CargarPuestoArea(int idBodega, int area)
        {
            icb_sysparameter buscarRolTecnico = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P48");
            int idRolTecnico = buscarRolTecnico != null ? Convert.ToInt32(buscarRolTecnico.syspar_value) : 0;
            var buscarTecnicoBahia = (from bahia in context.tbahias
                                      join tipoBahia in context.ttipobahia
                                          on bahia.tipo_bahia equals tipoBahia.id
                                      join tecnico in context.ttecnicos
                                          on bahia.idtecnico equals tecnico.id
                                      join usuario in context.users
                                          on tecnico.idusuario equals usuario.user_id
                                      where bahia.bodega == idBodega && tipoBahia.id == area && usuario.rol_id == idRolTecnico
                                      select new
                                      {
                                          usuario.user_id,
                                          idTecnico = tecnico.id,
                                          usuario = "(" + bahia.codigo_bahia + ") " + usuario.user_nombre + " " + usuario.user_apellido
                                      }).ToList();

            return Json(buscarTecnicoBahia, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarCentrosCosto(int? id)
        {
            if (id != null)
            {
                var centroCosotos = (from cc in context.centro_costo
                                     where cc.centcst_estado && cc.bodega == id
                                     orderby cc.pre_centcst
                                     select new
                                     {
                                         id = cc.centcst_id,
                                         nombre = "(" + cc.pre_centcst + ") " + cc.centcst_nombre
                                     }).ToList();
                return Json(centroCosotos);
            }

            return Json(0);
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

        public JsonResult ConsultarAgendasClientes() {

            var agendaclientes = context.vw_buscar_cliente_agenda.Select(x => new
                {
                x.bodega_nombre,
                x.idbodega,
                x.fecha,
                x.asesor_nombre,
                x.placa,
                x.cedula,
                x.cliente,
                x.estadoorden_nombre,
                x.ringresodescripcion,
                x.tipo_bahia
                }).ToList();


        return Json(agendaclientes, JsonRequestBehavior.AllowGet);
            }


        public JsonResult ConsultarAgClienteFiltro(string cedula, string placa, DateTime? fechaini, DateTime? fechafin)
            {


            Expression<Func<vw_buscar_cliente_agenda, bool>> predicado = PredicateBuilder.True<vw_buscar_cliente_agenda>();
           

            if (cedula != "")
                {
                predicado = predicado.And(d => d.cedula == cedula);
                }
            if (placa != "")
                {
                predicado = predicado.And(d => d.placa == placa);
                }

            if (fechaini != null)
                {
                predicado = predicado.And(d => d.fec_creacion >= fechaini);
                }
            if (fechafin != null)
                {
                predicado = predicado.And(d => d.fec_creacion <= fechafin);
                }

            var agendaclientes = context.vw_buscar_cliente_agenda.Where(predicado).Select(x => new
                {
                x.bodega_nombre,
                x.idbodega,
                x.fecha,
                x.asesor_nombre,
                x.placa,
                x.cedula,
                x.cliente,
                x.estadoorden_nombre,
                x.ringresodescripcion,
                x.tipo_bahia
                }).ToList();


            return Json(agendaclientes, JsonRequestBehavior.AllowGet);
            }

        public ActionResult CrearVehiculocita(string placa) {

            ListasVehiculos();

            if (placa != null)
                {
                CrearVehiculoModel vehiculo = new CrearVehiculoModel();
                vehiculo.plac_vh = placa;
                
                return View(vehiculo);
                }
            else {
                return View();
                }

            }

        [HttpPost]
        public ActionResult CrearVehiculocita(CrearVehiculoModel modelo)
            {
            try
                {
                icb_vehiculo vehiculo = new icb_vehiculo();
                vehiculo.propietario = Convert.ToInt32(modelo.propietario);
                vehiculo.plan_mayor = modelo.plan_mayor;
                vehiculo.vin = modelo.vin;
                vehiculo.plac_vh = modelo.plac_vh;
                vehiculo.nummot_vh = modelo.nummot_vh;
                vehiculo.marcvh_id = modelo.marcvh_id;
                vehiculo.modvh_id = modelo.modvh_id;
                vehiculo.colvh_id = modelo.colvh_id;
                decimal kilometrajhe = Convert.ToDecimal(modelo.kilometraje);
                vehiculo.kilometraje = Convert.ToInt32(kilometrajhe);
                vehiculo.anio_vh = modelo.anio_vh;
                vehiculo.tpvh_id = modelo.tpvh_id;
                vehiculo.proveedor_id = Convert.ToInt32(Request["proveedor_id"]);
                vehiculo.icbvhfec_creacion = DateTime.Now;
                vehiculo.icbvhuserid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                vehiculo.numerosoat = modelo.numerosoat;
                DateTime fechasoat = Convert.ToDateTime(modelo.fecha_soat, miCultura);
                vehiculo.fecha_soat = fechasoat;
                DateTime fechatecnomenica = Convert.ToDateTime(modelo.fecha_tecnomecanica, miCultura);
                vehiculo.fecha_tecnomecanica = fechatecnomenica;
                vehiculo.propietario = Convert.ToInt32(modelo.propietario);
                vehiculo.icbvh_estado = true;
                vehiculo.id_bod = Convert.ToInt32(Session["user_bodega"]);
                vehiculo.costototal_vh = 0;
                vehiculo.costosiniva_vh = 0;
                vehiculo.iva_vh = 0;
                vehiculo.kmgarantia = 0;
                vehiculo.kilometraje = 0;
                vehiculo.garantia = false;
                vehiculo.kilometraje_nuevo = 0;
                vehiculo.fecha_entrega = Convert.ToDateTime(modelo.fechaentrega);
                vehiculo.concesionariovh = modelo.nombreconcesionario;

                context.icb_vehiculo.Add(vehiculo);
                context.SaveChanges();
                TempData["mensaje"] = "Vehiculo creado con exito";
                ListasVehiculos();


                return View();
                }
            catch (Exception e)
                {

                throw e;
                }
   
            }

        public void ListasVehiculos() {

            ViewBag.marcvh_id = new SelectList(context.marca_vehiculo, "marcvh_id", "marcvh_nombre");
            ViewBag.modvh_id = new SelectList(context.modelo_vehiculo.ToList(), "modvh_codigo", "modvh_nombre");
            ViewBag.colvh_id = new SelectList(context.color_vehiculo, "colvh_id", "colvh_nombre");
            ViewBag.tpvh_id = new SelectList(context.tipo_vehiculo, "tpvh_id", "tpvh_nombre");
            var provedores = (from  ter in context.icb_terceros                                
                              select new
                                  {
                                  idTercero = ter.tercero_id,
                                  nombre = "(" + ter.doc_tercero + ") - " + ter.prinom_tercero + " " + ter.apellido_tercero + " " +
                                           ter.razon_social
                                  }).ToList();
            ViewBag.proveedor_id = new SelectList(provedores, "idTercero", "nombre");
            ViewBag.propietario = new SelectList(provedores, "idTercero", "nombre");

            }


        public JsonResult cambiarPropietarioVehiculo(string placa, string cedula) {

            icb_vehiculo vehiculo = context.icb_vehiculo.Where(x => x.plac_vh == placa).FirstOrDefault();
            icb_terceros tercero = context.icb_terceros.Where(d => d.doc_tercero == cedula).FirstOrDefault();

            vehiculo.propietario = tercero.tercero_id;
            context.Entry(vehiculo).State = EntityState.Modified;
            context.SaveChanges();



            return Json(true, JsonRequestBehavior.AllowGet);
            }


        public JsonResult ConsultarAverias(string  placa)
            {
            var resultado = false;
            var listaverias = context.icb_vehiculo_eventos.Where(x => x.planmayor == placa).Select(d => new { d.planmayor, d.evento_id }).ToList();

            if (listaverias.Count > 0)
                {
                resultado = true;
                }


            return Json(resultado, JsonRequestBehavior.AllowGet);
            }

        public ActionResult MtivoReprogramacionCita() {

            return View();
            }

        [HttpPost]
        public ActionResult MtivoReprogramacionCita(formMtvoReprogCita modelo)
            {
            try
                {
                tmotivosreprogcita reprogcita = new tmotivosreprogcita();
                reprogcita.descripcion = modelo.descripcion;
                reprogcita.estado = modelo.estado;
                reprogcita.fecha_creacion = DateTime.Now;
                reprogcita.razoninactivo = modelo.razoninactivo;
                reprogcita.user_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                context.tmotivosreprogcita.Add(reprogcita);
                context.SaveChanges();
                TempData["mensaje"] = "Motivo de reprogramacion creado con exito";
                }
            catch (Exception)
                {

                throw;
                }
            return View();
            }


        public JsonResult BuscarMtivoreprogcita()
            {

            var datos = context.tmotivosreprogcita.Select(x => new
                {
                x.descripcion,
                x.id,
                estado = x.estado != false ? "Activo" : "inactivo",
                }).ToList();


            return Json(datos, JsonRequestBehavior.AllowGet);
            }

        public ActionResult MotivoReprogcitaEditar(int id)
            {
            formMtvoReprogCita modelo = new formMtvoReprogCita();
            tmotivosreprogcita motivo = context.tmotivosreprogcita.Where(x => x.id == id).FirstOrDefault();
            modelo.id = motivo.id;
            modelo.descripcion = motivo.descripcion;
            modelo.estado = Convert.ToBoolean(motivo.estado);
            modelo.razoninactivo = motivo.razoninactivo;                            
            return View(modelo);
            }

        [HttpPost]
        public ActionResult MotivoReprogcitaEditar(formMtvoReprogCita modelo)
            {
            tmotivosreprogcita motivo = context.tmotivosreprogcita.Where(x => x.id == modelo.id).FirstOrDefault();

            motivo.descripcion = modelo.descripcion;
            motivo.estado = Convert.ToBoolean(modelo.estado);
            motivo.razoninactivo = modelo.razoninactivo;           
            context.Entry(motivo).State = EntityState.Modified;
            context.SaveChanges();
            TempData["mensaje"] = "Motivo de reprogramacion actualizado con exito";
            return View(modelo);
            }

        public JsonResult ConsultaMotivosReprog(){

            var datos = context.tmotivosreprogcita.Where(x => x.estado == true).Select(b => new { b.id, b.descripcion }).ToList();

            return Json(datos, JsonRequestBehavior.AllowGet);
            }

        }
    }