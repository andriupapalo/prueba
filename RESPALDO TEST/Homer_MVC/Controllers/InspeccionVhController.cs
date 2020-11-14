using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class InspeccionVhController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");
        // GET: InspeccionVh
        public ActionResult Index(int? menu, string id)
        {
            ViewBag.ave_id = new SelectList(context.icb_averias.Where(x => x.ave_estado), "ave_id", "ave_descripcion");
            ViewBag.grave_id = new SelectList(context.icb_gravedad_averias.Where(x => x.grave_estado), "grave_id", "grave_descripcion");
            ViewBag.taller_averia_id = new SelectList(context.tallerAveria.Where(x => x.estado == true && !(x.nombre.Contains("solucion"))), "id", "nombre");
            ViewBag.comb_id = new SelectList(context.icb_combustible_averia.Where(x => x.comb_estado), "comb_id", "comb_descripcion");
            ViewBag.medida_id = new SelectList(context.icb_medida_averia.Where(x => x.medida_estado), "medida_id", "medida_descripcion");
            ViewBag.zona_id = new SelectList(context.icb_zona_averia.Where(x => x.zona_estado), "zona_id", "zona_descripcion");
            ViewBag.impacto_id = new SelectList(context.icb_impacto_averia.Where(x => x.impacto_estado), "impacto_id", "impacto_descripcion");
            //busco el vin del vehículo, de existir)
            string vin = "";
            icb_vehiculo existevehi = context.icb_vehiculo.Where(d => d.plan_mayor == id).FirstOrDefault();
            if (existevehi != null)
            {
                vin = existevehi.vin;
            }

            ViewBag.vin = vin;
            BuscarFavoritos(menu);
            return View();
        }

        public ActionResult TareasInspeccion(int? menu)
        {
            ViewBag.iditeminspeccion = new SelectList(context.titemsinspeccion, "id", "Descripcion");
            BuscarFavoritos(menu);
            return View(new ttareasinspeccion { estado = true });
        }

        [HttpPost]
        public ActionResult TareasInspeccion(ttareasinspeccion modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                ttareasinspeccion buscarNombre = context.ttareasinspeccion.FirstOrDefault(x =>
                    x.descripcion == modelo.descripcion && x.iditeminspeccion == modelo.iditeminspeccion);
                if (buscarNombre != null)
                {
                    TempData["mensaje_error"] =
                        "El nombre de la tarea ya se encuentra registrado para el item seleccionado!";
                }
                else
                {
                    modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    modelo.fec_creacion = DateTime.Now;
                    context.ttareasinspeccion.Add(modelo);
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "El registro de la tarea se guardo exitosamente!";
                        BuscarFavoritos(menu);
                        return RedirectToAction("TareasInspeccion", new { menu });
                    }

                    TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                }
            }

            BuscarFavoritos(menu);
            ViewBag.iditeminspeccion =
                new SelectList(context.titemsinspeccion, "id", "Descripcion", modelo.iditeminspeccion);
            return View(modelo);
        }

        public ActionResult ActualizarTareaInspeccion(int? menu, int? id)
        {
            //valida si el id es null
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ttareasinspeccion tarea = context.ttareasinspeccion.Find(id);
            if (tarea == null)
            {
                return HttpNotFound();
            }

            ViewBag.iditeminspeccion =
                new SelectList(context.titemsinspeccion, "id", "Descripcion", tarea.iditeminspeccion);
            BuscarFavoritos(menu);
            ConsultaDatosCreacionTareas(tarea);
            return View(tarea);
        }

        [HttpPost]
        public ActionResult ActualizarTareaInspeccion(ttareasinspeccion modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                //consulta si el registro esta en BD teniendo en cuenta el id de consulta
                ttareasinspeccion buscarNombre = context.ttareasinspeccion.FirstOrDefault(x =>
                    x.descripcion == modelo.descripcion && x.iditeminspeccion == modelo.iditeminspeccion);
                if (buscarNombre != null)
                {
                    if (buscarNombre.id != modelo.id)
                    {
                        TempData["mensaje_error"] = "El registro que ingreso ya se encuentra, por favor valide!";
                    }
                    else
                    {
                        modelo.fec_actualizacion = DateTime.Now;
                        modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarNombre.fec_actualizacion = DateTime.Now;
                        buscarNombre.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                        buscarNombre.estado = modelo.estado;
                        buscarNombre.iditeminspeccion = modelo.iditeminspeccion;
                        buscarNombre.descripcion = modelo.descripcion;
                        buscarNombre.razon_inactivo = modelo.razon_inactivo;
                        context.Entry(buscarNombre).State = EntityState.Modified;
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                        {
                            TempData["mensaje"] = "La actualización de la tarea fue exitosa!";
                        }
                        else
                        {
                            TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                        }
                    }
                }
                else
                {
                    modelo.fec_actualizacion = DateTime.Now;
                    modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                    context.Entry(modelo).State = EntityState.Modified;
                    int guardar = context.SaveChanges();
                    if (guardar > 0)
                    {
                        TempData["mensaje"] = "La actualización de la tarea fue exitosa!";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error con base de datos, revise su conexion!";
                    }
                }
            }

            ViewBag.iditeminspeccion =
                new SelectList(context.titemsinspeccion, "id", "Descripcion", modelo.iditeminspeccion);
            BuscarFavoritos(menu);
            ConsultaDatosCreacionTareas(modelo);
            return View(modelo);
        }

        public void ConsultaDatosCreacionTareas(ttareasinspeccion modelo)
        {
            //consulta el nombre de usuario creacion con el id, lo envia a la vista a traves de ViewBag
            users creador = (from c in context.users
                             join b in context.ttareasinspeccion on c.user_id equals b.userid_creacion
                             where b.id == modelo.id
                             select c).FirstOrDefault();

            ViewBag.user_nombre_cre = creador != null ? creador.user_nombre + " " + creador.user_apellido : null;

            //consulta el nombre de usuario actualizacion con el id, lo envia a la vista a traves de ViewBag
            users actualizador = (from c in context.users
                                  join b in context.ttareasinspeccion on c.user_id equals b.user_idactualizacion
                                  where b.id == modelo.id
                                  select c).FirstOrDefault();

            ViewBag.user_nombre_act =
                actualizador != null ? actualizador.user_nombre + " " + actualizador.user_apellido : null;
        }

        public JsonResult BuscarTareasInspeccion()
        {
            var buscarTareas = (from tarea in context.ttareasinspeccion
                                join item in context.titemsinspeccion
                                    on tarea.iditeminspeccion equals item.id
                                select new
                                {
                                    tarea.id,
                                    tarea.descripcion,
                                    estado = tarea.estado ? "Activo" : "Inactivo",
                                    nombre_item = item.Descripcion
                                }).ToList();
            return Json(buscarTareas, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarListaInspeccion(int id_cita)
        {
            tcitastaller buscarCita = context.tcitastaller.FirstOrDefault(x => x.id == id_cita);
            if (buscarCita != null)
            {
                if (buscarCita.fecfin_inspeccion != null && buscarCita.fecini_inspeccion != null)
                {
                    return Json(
                        new
                        {
                            citaYaAtendida = true,
                            mensajeAlerta = "La cita ya se atendió el dia " +
                                            buscarCita.fecini_inspeccion.Value.ToShortDateString() + " a las " +
                                            buscarCita.fecini_inspeccion.Value.ToShortTimeString()
                        }, JsonRequestBehavior.AllowGet);
                }

                buscarCita.fecini_inspeccion = DateTime.Now;
                context.Entry(buscarCita).State = EntityState.Modified;
                context.SaveChanges();
            }

            var buscarConceptos = (from concepto in context.tconceptoinspeccion
                                   select new
                                   {
                                       concepto.Descripcion,
                                       categorias = (from categoria in context.tcategoriainspeccion
                                                     where categoria.idconcepto == concepto.id
                                                     select new
                                                     {
                                                         categoria.descripcion,
                                                         items = (from item in context.titemsinspeccion
                                                                  where item.idcategoria == categoria.id
                                                                  select new
                                                                  {
                                                                      item.id,
                                                                      item.Descripcion,
                                                                      item.tiporespuesta,
                                                                      respuestasSelect = (from opcion in context.titemopcioninspeccion
                                                                                          where opcion.iditem == item.id
                                                                                          select new
                                                                                          {
                                                                                              opcion.id,
                                                                                              opcion.descripcion
                                                                                          }).ToList()
                                                                  }).ToList()
                                                     }).ToList()
                                   }).ToList();
            var listaEstados = from estado in context.titemcolorinspeccion
                               select new
                               {
                                   estado.id,
                                   estado.descripcion,
                                   estado.color
                               };
            return Json(new { buscarConceptos, listaEstados, citaYaAtendida = false }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarTareasItem(int id_item, int id_color)
        {
            if (id_color == 3 || id_color == 4)
            {
                var buscarTarea = (from tarea in context.ttareasinspeccion
                                   join item in context.titemsinspeccion
                                       on tarea.iditeminspeccion equals item.id
                                   join categoria in context.tcategoriainspeccion
                                       on item.idcategoria equals categoria.id
                                   where tarea.iditeminspeccion == id_item
                                   select new
                                   {
                                       tarea.id,
                                       tarea.descripcion,
                                       nombre_item = item.Descripcion,
                                       nombre_categoria = categoria.descripcion
                                   }).ToList();
                // Si el estado es amarillo se debe solicitar fecha
                bool requiereFecha = id_color == 3 ? true : false;
                return Json(new { buscarTarea, requiereFecha }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { buscarTarea = new List<int>() }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarTodasLasTareas()
        {
            var buscarTarea = (from tarea in context.ttareasinspeccion
                               join item in context.titemsinspeccion
                                   on tarea.iditeminspeccion equals item.id
                               join categoria in context.tcategoriainspeccion
                                   on item.idcategoria equals categoria.id
                               select new
                               {
                                   tarea.id,
                                   tarea.descripcion,
                                   nombre_item = item.Descripcion,
                                   nombre_categoria = categoria.descripcion
                               }).ToList();
            return Json(buscarTarea, JsonRequestBehavior.AllowGet);
        }

        public bool ValidarKilometraje(long km1, long km2)
        {

            bool result = false;
            long kilometraje1 = 0;

            kilometraje1 = km1 * 3;

            if (kilometraje1 < km2)
            {
                result = true;
                return result;
            }
            else
            {
                return result;
            }


        }


        [HttpGet]
        public ActionResult AtenderCita(int id_cita, int? menu)
        {
            var buscarCita = (from cita in context.tcitastaller
                              join cliente in context.icb_terceros
                                  on cita.cliente equals cliente.tercero_id
                              where cita.id == id_cita
                              select new
                              {
                                  cita.placa,
                                  cita.id,
                                  cita.bahia,
                                  cliente.doc_tercero,
                                  cliente.tercero_id,
                                  cliente.digito_verificacion,
                                  cliente = cliente.razon_social + cliente.prinom_tercero + " " + cliente.segnom_tercero + " " +
                                            cliente.apellido_tercero + " " + cliente.segapellido_tercero,
                                  cliente.email_tercero,
                                  cliente.celular_tercero,
                                  cliente.telf_tercero,
                                  direccion = (from direcciones in context.terceros_direcciones
                                               join ciudad in context.nom_ciudad
                                                   on direcciones.ciudad equals ciudad.ciu_id
                                               where direcciones.idtercero == cliente.tercero_id
                                               select new
                                               {
                                                   direcciones.id,
                                                   direcciones.direccion,
                                                   direcciones.ciudad,
                                                   ciudad.ciu_nombre
                                               }).OrderByDescending(x => x.id).FirstOrDefault()
                              }).FirstOrDefault();

            //veo si hay alguna OT que tenga asociada esta cita
            tencabezaorden orden = context.tencabezaorden.Where(d => d.idcita == id_cita).FirstOrDefault();
            if (orden != null)
            {
                return RedirectToAction("Update", "OrdenTaller", new { orden.id, menu = 1234 });
            }

            var buscarVehiculo = (from vehiculo in context.icb_vehiculo
                                  join modelo in context.modelo_vehiculo
                                      on vehiculo.modvh_id equals modelo.modvh_codigo
                                  join tipo in context.vtipo
                                      on modelo.tipo equals tipo.tipo_id into ps
                                  from tipo in ps.DefaultIfEmpty()
                                  join color in context.color_vehiculo
                                      on vehiculo.colvh_id equals color.colvh_id
                                  join marca in context.marca_vehiculo
                                      on modelo.mar_vh_id equals marca.marcvh_id
                                  where vehiculo.plac_vh == buscarCita.placa || vehiculo.plan_mayor == buscarCita.placa
                                  select new
                                  {
                                      vehiculo.kilometraje,
                                      vehiculo.kilometraje_nuevo,
                                      modelo.modvh_nombre,
                                      marca.marcvh_nombre,
                                      color.colvh_nombre,
                                      vehiculo.plac_vh,
                                      vehiculo.plan_mayor,
                                      vehiculo.vin,
                                      vehiculo.nummot_vh,
                                      nombreTipo = tipo.nombre != null ? tipo.nombre : ""
                                  }).FirstOrDefault();
            if (buscarVehiculo == null)
            {
                TempData["mensaje_error"] =
                    "El vehículo no se encuentra registrado, por favor dirijase al centro de atención para ingresar los datos del vehículo...";
                return RedirectToAction("InicioTecnico", "Inicio");
            }

            ClienteVehiculoModel modeloVista = new ClienteVehiculoModel
            {
                BahiaId = buscarCita.bahia,
                PlanMayor = buscarVehiculo.plan_mayor,
                TerceroId = buscarCita.tercero_id,
                Placa = buscarCita.placa,
                Marca = buscarVehiculo.marcvh_nombre,
                Modelo = buscarVehiculo.modvh_nombre,
                Color = buscarVehiculo.colvh_nombre,
                Serie = buscarVehiculo.vin,
                Motor = buscarVehiculo.nummot_vh,
                TipoVehiculo = buscarVehiculo.nombreTipo,
                Documento = buscarCita.doc_tercero,
                DigitoVerificacion = buscarCita.digito_verificacion,
                Direccion = buscarCita.direccion != null ? buscarCita.direccion.direccion : "",
                Nombres = buscarCita.cliente,
                Correo = buscarCita.email_tercero,
                Telefono = buscarCita.telf_tercero,
                Celular = buscarCita.celular_tercero,
                Kilometraje = buscarVehiculo.kilometraje_nuevo,
                KilometrajeAnterior = buscarVehiculo.kilometraje,

                CitaId = buscarCita.id
            };
            //ViewBag.CombustibleId = new SelectList(context.tcantcombustible, "id", "descripcion");
            //var centros=context.centro_costo.Where(d=>d.bodega==)

            ViewBag.validacionkm = ValidarKilometraje(buscarVehiculo.kilometraje, buscarVehiculo.kilometraje_nuevo);

            ViewBag.IdCita = id_cita;
            BuscarFavoritos(menu);
            return View(modeloVista);
        }

        [HttpPost]
        public ActionResult AtenderCita(ClienteVehiculoModel modeloVista, int? menu)
        {
            if (Session["user_usuarioid"] != null)
            {
                int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                int id_usuario_actual = Convert.ToInt32(Session["user_usuarioid"]);
                int buscarIdTecnico = (from tecnico in context.ttecnicos
                                       where tecnico.idusuario == id_usuario_actual
                                       select tecnico.id).FirstOrDefault();
                int id_cita = Convert.ToInt32(Request["CitaId"]);
                int kilometraje = 0;
                int kilometrajeanterior = 0;
                if (Request["Kilometraje"] != null)
                {
                    kilometraje = Convert.ToInt32(Request["Kilometraje"]);
                }

                if (Request["KilometrajeAnterior"] != null)
                {
                    kilometrajeanterior = Convert.ToInt32(modeloVista.KilometrajeAnterior);
                }

                ViewBag.IdCita = id_cita;

                // Verifico que la cita existe y que no tiene una OT asociada
                tencabezaorden ordent = context.tencabezaorden.Where(d => d.idcita == id_cita).FirstOrDefault();
                if (ordent != null)
                {
                    TempData["mensaje_error"] = "La cita ya ha sido asignada a la orden de trabajo con el código " +
                                                ordent.codigoentrada;
                    return RedirectToAction("Update", "OrdenTaller", new { ordent.id, menu = 1234 });
                }

                if (kilometraje <= kilometrajeanterior)
                {
                    TempData["mensaje_error"] = "El kilometraje actual no puede ser menor al kilometraje anterior";
                    return View(modeloVista);
                }

                tcitastaller buscarCita = context.tcitastaller.FirstOrDefault(x => x.id == id_cita);
                //Creo la OT desde aqui
                icb_sysparameter tipoorden = context.icb_sysparameter.Where(d => d.syspar_cod == "P76").FirstOrDefault();
                int tipodocorden = tipoorden != null ? Convert.ToInt32(tipoorden.syspar_value) : 26;
                //

                int buscarDocumentos = (from consecutivos in context.icb_doc_consecutivos
                                        join bodega in context.bodega_concesionario
                                            on consecutivos.doccons_bodega equals bodega.id
                                        join tipoDocumento in context.tp_doc_registros
                                            on consecutivos.doccons_idtpdoc equals tipoDocumento.tpdoc_id
                                        where consecutivos.doccons_bodega == bodegaActual && tipoDocumento.tipo == tipodocorden
                                        select tipoDocumento.tpdoc_id).FirstOrDefault();

                //ahora quiero ver si hay un código de orden de taller para esa bodega
                icb_consecutivo_ot buscatCodigo = context.icb_consecutivo_ot.Where(x => x.otcon_bodega == bodegaActual)
                    .FirstOrDefault();
                string codigoIOT = "";

                int pedido = 0;
                //busco si es de accesorios 
                icb_sysparameter paramaacce = context.icb_sysparameter.Where(d => d.syspar_cod == "P116").FirstOrDefault();
                int razonaccesorio = paramaacce != null ? Convert.ToInt32(paramaacce.syspar_value) : 6;
                if (buscarCita.razon_ingreso == razonaccesorio)
                {
                    //veo si el vehiculo tiene pedido
                    vpedido existepedido = context.vpedido.Where(d =>
                        (d.planmayor == buscarCita.placa || d.icb_vehiculo.plac_vh == buscarCita.placa) &&
                        d.anulado == false).FirstOrDefault();
                    if (existepedido != null)
                    {
                        pedido = existepedido.id;
                    }
                }

                //razon ingreso por garantia 
                icb_sysparameter paramgarane = context.icb_sysparameter.Where(d => d.syspar_cod == "P87").FirstOrDefault();
                int razongarantia = paramgarane != null ? Convert.ToInt32(paramgarane.syspar_value) : 7;
                int? EstadoOTdos = null;
                if (buscarCita.razon_ingreso == razongarantia)
                    {
                    EstadoOTdos = 1;
                    }



                if (buscatCodigo != null)
                {
                    string planmayor = context.icb_vehiculo
                        .Where(d => d.plac_vh == buscarCita.placa || d.plan_mayor == buscarCita.placa)
                        .Select(d => d.plan_mayor).FirstOrDefault();
                    string anio = DateTime.Now.Year.ToString();
                    string consecutivonum = cerosconsecutivo(buscatCodigo.otcon_consecutivo.Value);
                    var numerox = Convert.ToInt32(consecutivonum);
                    codigoIOT = anio.Substring(anio.Length - 2) + buscatCodigo.otcon_prefijo + "-" + consecutivonum;
                    //busco el id del tecnico
                    ttecnicos idtecnico = context.ttecnicos.Where(d => d.id == buscarCita.tecnico).FirstOrDefault();
                    int tecnico = idtecnico != null ? idtecnico.idusuario : 1;
                    //creo la OT
                    double tiempocita = (buscarCita.hasta - buscarCita.desde).TotalHours;
                    tencabezaorden nuevaOT = new tencabezaorden
                        {
                        //aseguradora=buscarCita.
                        idtipodoc = buscarDocumentos,
                        numero = numerox,
                        asesor = buscarCita.userid_creacion,
                        tipooperacion = buscarCita.tipocita,
                        codigoentrada = codigoIOT,
                        bodega = buscarCita.tbahias.bodega,
                        tercero = buscarCita.cliente,
                        placa = planmayor,
                        fecha = DateTime.Now,
                        fec_creacion = DateTime.Now,
                        entrega = buscarCita.hasta,
                        kilometraje = kilometraje,
                        razoningreso = buscarCita.razon_ingreso ?? 1,
                        userid_creacion = tecnico,
                        idtecnico = tecnico,
                        estado = true,
                        idcita = buscarCita.id,
                        centrocosto = buscarCita.centrocosto,
                        id_plan_mantenimiento = buscarCita.id_tipo_inspeccion,
                        estadoorden_dos = EstadoOTdos
                        //tecnico
                        };
                    context.tencabezaorden.Add(nuevaOT);
                    int guardarTareas = context.SaveChanges();
                    if (guardarTareas > 0)
                    {
                        //me traigo las operaciones de la cita y las añado a las operaciones de la OT
                        List<tcitasoperacion> operaciones = context.tcitasoperacion.Where(d => d.idcita == nuevaOT.idcita && d.inspeccion)
                            .ToList();
                        if (operaciones.Count > 0)
                        {
                            foreach (tcitasoperacion item in operaciones)
                            {
                                //busco la operacion en este tempario
                                ttempario datosope = context.ttempario.Where(d => d.codigo == item.operacion)
                                    .FirstOrDefault();
                                if (datosope != null)
                                {
                                    //busco dicha operacion en el tempario de acuerdo a la referencia y la 
                                    string operacion = item.operacion;
                                    int descuento = 0;
                                    decimal? ivaOperacion = datosope.iva != null ? datosope.codigo_iva.porcentaje : 0;
                                    decimal tiempo = datosope.tiempo != null ? Convert.ToDecimal(datosope.tiempo, miCultura) : 0;
                                    decimal precio = datosope.precio != null ? datosope.precio.Value : 0;
                                    if (!string.IsNullOrEmpty(operacion))
                                    {
                                        context.tdetallemanoobraot.Add(new tdetallemanoobraot
                                        {
                                            idorden = nuevaOT.id,
                                            idtempario = operacion,
                                            idtecnico = nuevaOT.tcitastaller1.tecnico,
                                            tiempo = tiempo,
                                            valorunitario = precio,
                                            poriva = ivaOperacion,
                                            //tipotarifa = Convert.ToInt32(tipoTarifa),
                                            pordescuento = descuento,
                                            fecha = DateTime.Now,
                                            id_plan_mantenimiento = item.id_plan_mantenimiento,
                                            estado = "1",
                                            respuesta_sms = null
                                        });
                                    }
                                }
                            }

                            context.SaveChanges();
                        }

                        //me traigo los repuestos de la cita (los que devienen de las operaciones relacionadas al plan de mantenimiento
                        if (nuevaOT.tcitastaller1.id_tipo_inspeccion != null)
                        {
                            List<string> repuestoscita = context.tcitarepuestos
                                .Where(d => d.idcita == buscarCita.id && d.id_plan_mantenimiento != null)
                                .Select(d => d.idrepuesto).ToList();
                            int? buscarModeloGeneral = (from modelox in context.modelo_vehiculo
                                                        where modelox.modvh_codigo == nuevaOT.tcitastaller1.modelo
                                                        select modelox.modelogkit).FirstOrDefault();

                            var buscarReferenciasSQL = (from plan in context.tplanrepuestosmodelo
                                                        join referencia in context.icb_referencia
                                                            on plan.referencia equals referencia.ref_codigo into ps
                                                        from referencia in ps.DefaultIfEmpty()
                                                        join precios in context.rprecios
                                                            on referencia.ref_codigo equals precios.codigo into ps2
                                                        from precios in ps2.DefaultIfEmpty()
                                                        where plan.inspeccion == nuevaOT.tcitastaller1.id_tipo_inspeccion &&
                                                              plan.modgeneral == buscarModeloGeneral && plan.tipo == "R" &&
                                                              plan.referencia != null
                                                              && repuestoscita.Contains(plan.referencia)
                                                        select new
                                                        {
                                                            referencia.ref_codigo,
                                                            referencia.ref_descripcion,
                                                            precio = plan.listaprecios.Contains("precio1") ? precios != null ? precios.precio1 :
                                                                0 :
                                                                plan.listaprecios.Contains("precio2") ? precios != null ? precios.precio1 : 0 :
                                                                plan.listaprecios.Contains("precio3") ? precios != null ? precios.precio1 : 0 :
                                                                plan.listaprecios.Contains("precio4") ? precios != null ? precios.precio1 : 0 :
                                                                plan.listaprecios.Contains("precio5") ? precios != null ? precios.precio1 : 0 :
                                                                plan.listaprecios.Contains("precio6") ? precios != null ? precios.precio1 : 0 :
                                                                plan.listaprecios.Contains("precio7") ? precios != null ? precios.precio1 : 0 :
                                                                plan.listaprecios.Contains("precio8") ? precios != null ? precios.precio1 : 0 :
                                                                plan.listaprecios.Contains("precio9") ? precios != null ? precios.precio1 : 0 :
                                                                0,
                                                            referencia.por_iva,
                                                            cantidad = plan.cantidad != null ? plan.cantidad.Value : 0
                                                        }).ToList();
                            var buscarReferencias = buscarReferenciasSQL.Select(x => new
                            {
                                x.ref_codigo,
                                x.ref_descripcion,
                                x.precio,
                                x.por_iva,
                                x.cantidad,
                                valorIva = Math.Round(x.precio * (decimal)x.por_iva / 100),
                                valorTotal = Math.Round((x.precio + x.precio * (decimal)x.por_iva / 100) * x.cantidad)
                            });

                            foreach (var item4 in buscarReferencias)
                            {

                                var cantidad = Math.Round(item4.cantidad);
                                decimal ivadecimal = Convert.ToDecimal(item4.por_iva, miCultura);
                                context.tdetallerepuestosot.Add(new tdetallerepuestosot
                                {
                                    idorden = nuevaOT.id,
                                    idrepuesto = item4.ref_codigo,
                                    valorunitario = item4.precio,
                                    pordescto = 0,
                                    poriva = ivadecimal,
                                    idtercero = nuevaOT.tercero,
                                    //cantidad = Convert.ToInt32(item4.cantidad),
                                    cantidad = Convert.ToInt32(cantidad),
                                    solicitado = false,
                                    fecha_solicitud = null,
                                    id_plan_mantenimiento = nuevaOT.tcitastaller1.id_tipo_inspeccion
                                });
                            }

                            context.SaveChanges();
                        }

                        //me traigo los repuestos que no forman parte del plan de mantenimiento (aplica para cita de accesorios)
                        List<tcitarepuestos> buscar = context.tcitarepuestos
                            .Where(d => d.idcita == id_cita && d.id_plan_mantenimiento == null).ToList();

                        if (buscar.Count > 0)
                        {
                            //busco si existe un pedido para esos repuestos
                            tcitarepuestos numeropedido = buscar.Where(d => d.idpedidorepuesto != null).FirstOrDefault();
                            if (numeropedido != null)
                            {
                                nuevaOT.idpedido = numeropedido.vpedrepuestos.pedido_id;
                                nuevaOT.tercero = numeropedido.vpedrepuestos.vpedido.nit.Value;
                                foreach (tcitarepuestos item in buscar)
                                {
                                    vpedrepuestos existerepuesto = context.vpedrepuestos.Where(d =>
                                        d.pedido_id == numeropedido.vpedrepuestos.pedido_id &&
                                        d.referencia == item.idrepuesto).FirstOrDefault();
                                    if (existerepuesto != null)
                                    {
                                        context.tdetallerepuestosot.Add(new tdetallerepuestosot
                                        {
                                            idorden = nuevaOT.id,
                                            idrepuesto = existerepuesto.referencia,
                                            valorunitario = existerepuesto.vrunitario ?? 0,
                                            pordescto = 0,
                                            poriva = 0,
                                            idtercero = nuevaOT.tercero,
                                            cantidad = existerepuesto.cantidad ?? 0,
                                            solicitado = false,
                                            fecha_solicitud = null
                                        });
                                    }
                                }
                            }
                        }

                        //me traigo las solicitudes de la cita (los síntomas que reporta el cliente o lo que solicita
                        List<tcitasintomas> numeroSolicitudes = context.tcitasintomas.Where(d => d.idcita == nuevaOT.idcita).ToList();
                        if (numeroSolicitudes.Count > 0)
                        {
                            foreach (tcitasintomas item2 in numeroSolicitudes)
                            {
                                context.tsolicitudorden.Add(new tsolicitudorden
                                {
                                    idorden = nuevaOT.id,
                                    bodega = nuevaOT.bodega,
                                    usuariosolicitud = nuevaOT.userid_creacion,
                                    fecsolicitud = DateTime.Now,
                                    solicitud = item2.sintomas
                                });
                            }

                            context.SaveChanges();
                        }

                        //parametro de sistema 
                        icb_sysparameter estadoOT = context.icb_sysparameter.Where(d => d.syspar_cod == "P78").FirstOrDefault();
                        int estadodeOT = estadoOT != null ? Convert.ToInt32(estadoOT.syspar_value) : 16;
                        //le cambio el estado a la cita (crear parametro de sistema)
                        if (buscarCita != null)
                        {
                            buscarCita.fecfin_inspeccion = DateTime.Now;
                            buscarCita.estadocita = estadodeOT;
                            context.Entry(buscarCita).State = EntityState.Modified;

                            context.SaveChanges();
                        }

                        nuevaOT.estadoorden = estadodeOT;
                        //le aumento el consecutivo en 1 al consecutivo de orden
                        buscatCodigo.otcon_consecutivo = buscatCodigo.otcon_consecutivo + 1;
                        context.Entry(buscatCodigo).State = EntityState.Modified;
                        context.SaveChanges();

                        vpedido pedidoPlan = context.vpedido.Where(x => x.planmayor == planmayor).FirstOrDefault();
                        if (pedidoPlan != null)
                        {
                            List<vpedrepuestos> pedrepuestos = context.vpedrepuestos.Where(x => x.pedido_id == pedidoPlan.id && x.facturado && x.instalado == false).ToList();
                            if (pedrepuestos != null)
                            {
                                for (int i = 0; i < pedrepuestos.Count; i++)
                                {
                                    string codigoref = pedrepuestos[i].referencia;
                                    icb_referencia referencia = context.icb_referencia.Where(x => x.ref_codigo == codigoref).FirstOrDefault();
                                    tdetallerepuestosot repuestosot = new tdetallerepuestosot
                                    {
                                        cantidad = pedrepuestos[i].cantidad != null ? pedrepuestos[i].cantidad.Value : 0,
                                        idrepuesto = codigoref,
                                        valorunitario = pedrepuestos[i].vrunitario != null ? pedrepuestos[i].vrunitario.Value : 0,
                                        pordescto = 0,
                                        costopromedio = 0,
                                        idtercero = pedidoPlan.nit != null ? pedidoPlan.nit.Value : 0,
                                        poriva = Convert.ToDecimal(referencia.por_iva, miCultura),
                                        idorden = nuevaOT.id
                                    };
                                    context.tdetallerepuestosot.Add(repuestosot);
                                    context.SaveChanges();
                                }
                            }
                        }

                        // Se envia un mensaje al cliente con un enlace para que este de autorizacion de las reparaciones de su vehiculo.                   
                        // Fin del envio de mensaje de texto al cliente

                        TempData["mensaje"] = "La Orden de trabajo N° " + nuevaOT.codigoentrada +
                                              " ha sido creada exitosamente";
                        //el id de la OT creada.
                        return RedirectToAction("Update", "OrdenTaller", new { nuevaOT.id });
                    }

                    TempData["mensaje_error"] = "La inspección no se ha guardado, verifique su conexión...";

                    ViewBag.CombustibleId = new SelectList(context.tcantcombustible, "id", "descripcion");
                    BuscarFavoritos(menu);
                    return View(modeloVista);
                }

                TempData["mensaje_error"] =
                    "No existe un código de OT configurado para la bodega actual que permita crear la OT";
                ViewBag.CombustibleId = new SelectList(context.tcantcombustible, "id", "descripcion");
                BuscarFavoritos(menu);
                return View(modeloVista);
            }

            return RedirectToAction("Login", "Login");
        }

        public JsonResult BuscarEstadoCita(int? cita)
        {
            if (cita != null)
            {
                //estado OT creada

                //estado OT llegada
                icb_sysparameter otlle = context.icb_sysparameter.Where(d => d.syspar_cod == "P80").FirstOrDefault();
                int estadollegada = otlle != null ? Convert.ToInt32(otlle.syspar_value) : 15;
                //busco la cita
                tcitastaller citax = context.tcitastaller.Where(d => d.id == cita).FirstOrDefault();
                if (citax != null)
                {
                    if (citax.estadocita == estadollegada || citax.fechallegada != null)
                    {
                        return Json(1);
                    }

                    return Json(0);
                }

                return Json(-1);
            }

            return Json(-2);
        }

        public JsonResult buscarKilometrajeAnterior(string placa)
        {
            var data = (from v in context.icb_vehiculo
                        where v.plac_vh == placa || v.plan_mayor == placa
                        orderby v.icbvh_id descending
                        select new
                        {
                            v.kilometraje
                        }).FirstOrDefault();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult actualizarKilometraje(string placa,int kilometraje) {

            var result = 0;

            var data = (from vh in context.icb_vehiculo
                        where vh.plac_vh == placa || vh.plan_mayor == placa
                        orderby vh.icbvh_id descending
                        select new
                        {
                            vh.plan_mayor
                        }).FirstOrDefault();

            var vehiculo = context.icb_vehiculo.Find(data.plan_mayor);
            vehiculo.kilometraje_nuevo = kilometraje;
            context.Entry(vehiculo).State = EntityState.Modified;
            int resultado = context.SaveChanges();

            if (resultado > 0)
            {
                result = 1;
            }
            else
            {
                result = 0;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public JsonResult actualizarKilometrajes(string placa, int kilometraje)
        {

            var result = 0;

            var data = (from vh in context.icb_vehiculo
                        where vh.plac_vh == placa || vh.plan_mayor == placa
                        orderby vh.icbvh_id descending
                        select new
                        {
                            vh.plan_mayor
                        }).FirstOrDefault();

            var vehiculo = context.icb_vehiculo.Find(data.plan_mayor);

            vehiculo.kilometraje = kilometraje;
            vehiculo.kilometraje_nuevo = kilometraje;

            context.Entry(vehiculo).State = EntityState.Modified;
            int resultado = context.SaveChanges();

            if (resultado > 0)
            {
                result = 1;
            }
            else
            {
                result = 0;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Autorizar(string id)
        {
            var buscarTareas = (from mensaje in context.tinspeccionmensaje
                                where mensaje.llave == id
                                select new
                                {
                                    mensaje.llave,
                                    mensaje.id_inspeccion,
                                    mensaje.fecha_envio,
                                    tareas = (from tareaPrioridad in context.ttareainspeccionprioridad
                                              join tarea in context.ttareasinspeccion
                                                  on tareaPrioridad.idtareainspeccion equals tarea.id
                                              where tareaPrioridad.idencabinspeccion == mensaje.id_inspeccion &&
                                                    tareaPrioridad.autorizado == false
                                              select new
                                              {
                                                  tarea.id,
                                                  tarea.descripcion,
                                                  tareaPrioridad.valor_unitario,
                                                  tareaPrioridad.cantidad
                                              }).ToList()
                                }).FirstOrDefault();

            if (buscarTareas != null)
            {
                if (buscarTareas.tareas.Count == 0)
                {
                    TempData["mensaje_error"] =
                        "El enlace de autorización ya no se encuentra disponible porque ya se realizó el proceso correspondiente.";
                    return RedirectToAction("InspeccionTerminada", "InspeccionVh");
                }

                if (buscarTareas.fecha_envio.Year == DateTime.Now.Year &&
                    buscarTareas.fecha_envio.Month == DateTime.Now.Month &&
                    buscarTareas.fecha_envio.Day == DateTime.Now.Day)
                {
                    AutorizacionTallerModel modelo = new AutorizacionTallerModel
                    {
                        id_encabezado = buscarTareas.id_inspeccion,
                        id_encabezado_encryptado = buscarTareas.llave
                    };
                    foreach (var item2 in buscarTareas.tareas)
                    {
                        modelo.listaItems.Add(new ItemAutorizado
                        {
                            id_item = item2.id,
                            nombre_item = item2.descripcion,
                            valor_item = item2.valor_unitario,
                            cantidad_item = item2.cantidad
                        });
                    }

                    return View(modelo);
                }

                TempData["mensaje_error"] =
                    "El enlace de autorización ya no se encuentra disponible porque ya transcurrio demasiado tiempo.";
                return RedirectToAction("InspeccionTerminada", "InspeccionVh");
            }

            TempData["mensaje_error"] =
                "El enlace de autorización ya no se encuentra disponible porque no es un enlace valido";
            return RedirectToAction("InspeccionTerminada", "InspeccionVh");
        }

        [HttpPost]
        public ActionResult Autorizar(AutorizacionTallerModel modelo)
        {
            List<ttareainspeccionprioridad> buscarTareas = context.ttareainspeccionprioridad.Where(x => x.idencabinspeccion == modelo.id_encabezado)
                .ToList();
            foreach (ttareainspeccionprioridad tarea in buscarTareas)
            {
                string item_seleccionado = Request["" + tarea.idtareainspeccion];
                if (item_seleccionado == "on")
                {
                    tarea.autorizado = true;
                }

                context.Entry(tarea).State = EntityState.Modified;
            }

            int guardar = context.SaveChanges();
            if (guardar > 0)
            {
                TempData["mensaje"] = "La autorización se ha guardado exitosamente. ";
                return RedirectToAction("Autorizar", "InspeccionVh", new { id = modelo.id_encabezado_encryptado });
            }

            TempData["mensaje_error"] = "No se ha guardado ningun cambio, por favor revise su conexión...";
            return View(modelo);
        }

        public ActionResult InspeccionTerminada()
        {
            return View();
        }

        public string EncriptarLlave(string entrada)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            md5.ComputeHash(Encoding.ASCII.GetBytes(entrada));
            byte[] result = md5.Hash;
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                str.Append(result[i].ToString("x2"));
            }

            Random rdm = new Random();
            int aleatorio = rdm.Next(1, 1000);
            string llave = DateTime.Now.ToString() + aleatorio;
            md5.ComputeHash(Encoding.ASCII.GetBytes(llave));
            byte[] resultLlave = md5.Hash;
            StringBuilder strLlave = new StringBuilder();
            for (int i = 0; i < resultLlave.Length; i++)
            {
                strLlave.Append(resultLlave[i].ToString("x2"));
            }

            return strLlave.ToString();
        }

        public string cerosconsecutivo(int numero)
        {
            string numero2 = "";
            if (numero >= 100000)
            {
                numero2 = numero.ToString();
            }
            else if (numero >= 10000)
            {
                numero2 = "0" + numero;
            }
            else if (numero >= 1000)
            {
                numero2 = "00" + numero;
            }
            else if (numero >= 100)
            {
                numero2 = "000" + numero;
            }
            else if (numero >= 10)
            {
                numero2 = "0000" + numero;
            }
            else if (numero >= 0)
            {
                numero2 = "00000" + numero;
            }

            return numero2;
        }

        [HttpPost]
        public ActionResult Index(InspeccionVhModel inspeccion, int? menu)
        {
            //HttpPostedFileBase imagen = Request.Files["imagefile"];
            //byte[] image = new byte[imagen.ContentLength];
            //imagen.InputStream.Read(image, 0, image.Length);
            //context.icb_inspeccionvehiculos.Add(new icb_inspeccionvehiculos() {
            //    insp_imagen=image
            //});
            //context.SaveChanges();
            if (ModelState.IsValid)
            {
            }

            ViewBag.ave_id = new SelectList(context.icb_averias.Where(x => x.ave_estado), "ave_id", "ave_descripcion");
            ViewBag.grave_id = new SelectList(context.icb_gravedad_averias.Where(x => x.grave_estado), "grave_id", "grave_descripcion");
            ViewBag.taller_averia_id = new SelectList(context.tallerAveria.Where(x => x.estado == true && !(x.nombre.Contains("solucion"))), "id", "nombre");
            ViewBag.comb_id = new SelectList(context.icb_combustible_averia.Where(x => x.comb_estado), "comb_id", "comb_descripcion");
            ViewBag.medida_id = new SelectList(context.icb_medida_averia.Where(x => x.medida_estado), "medida_id", "medida_descripcion");
            ViewBag.zona_id = new SelectList(context.icb_zona_averia.Where(x => x.zona_estado), "zona_id", "zona_descripcion");
            ViewBag.impacto_id = new SelectList(context.icb_impacto_averia.Where(x => x.impacto_estado), "impacto_id", "impacto_descripcion");
            BuscarFavoritos(menu);
            return View(inspeccion);
        }

        public JsonResult BuscarVehiculoVin(string vin)
        {
            /////
            ////Se comentarea el siguiente código por fallos encontrados, junto con el código correspondiente la vista 'index' en cuanto a consultas y cargue de datos
            /////

            #region Comentado

            //var buscarParametroStatus = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P7");
            //var statusAlmacenamientoParametro = buscarParametroStatus != null ? buscarParametroStatus.syspar_value : "6";

            //var idUsuario = Convert.ToInt32(Session["user_usuarioid"]);
            //var bodegaUsuarioActual = context.bodega_usuario.Where(x=>x.id_usuario==idUsuario).ToList();
            //List<int> bodegasDelUsuario = new List<int>();
            //foreach (var item in bodegaUsuarioActual) {
            //    bodegasDelUsuario.Add(item.id_bodega);
            //}

            //var estadoVin = 0;
            //var vehiculo = (from icb_vh in context.icb_vehiculo
            //               join color in context.color_vehiculo
            //               on icb_vh.colvh_id equals color.colvh_id
            //               join bodega in context.bodega_concesionario
            //               on icb_vh.icbvh_bodpro equals bodega.id
            //               join modelo in context.modelo_vehiculo
            //               on icb_vh.modvh_id equals modelo.modvh_codigo
            //               join evento in context.icb_tpeventos
            //               on icb_vh.id_evento equals evento.tpevento_id
            //                /*where icb_vh.vin == vin && bodegasDelUsuario.Contains(icb_vh.icbvh_bodpro??0) && icb_vh.icbvh_estatus == statusAlmacenamientoParametro && icb_vh.icbvh_fecinsp_ingreso==null*/
            //                select new {
            //                   icb_vh.icbvh_id,
            //                   icb_vh.vin,
            //                   modelo.modvh_nombre,
            //                   color.colvh_nombre,
            //                   bodega.bodccs_nombre,
            //                   evento.tpevento_nombre
            //               }).ToList();

            //if (vehiculo.Count > 0)
            //{
            //    estadoVin = 1;
            //}
            //else
            //{
            //    var buscaSinBodega = context.icb_vehiculo.FirstOrDefault(x=>x.vin==vin && !bodegasDelUsuario.Contains(x.icbvh_bodpro??0));
            //    if (buscaSinBodega != null)
            //    {
            //        estadoVin = -1;
            //    }
            //    else {
            //        var buscaVin = context.icb_vehiculo.FirstOrDefault(x => x.vin == vin);
            //        if (buscaVin != null)
            //        {
            //            if (buscaVin.icbvh_estatus != statusAlmacenamientoParametro)
            //            {
            //                estadoVin = -2;
            //            }
            //            if (buscaVin.icbvh_fecinsp_ingreso != null)
            //            {
            //                estadoVin = -3;
            //            }
            //        }
            //        else {
            //            estadoVin = -4;
            //        }
            //    }
            //}
            //var data = new
            //{
            //    estadoVin,
            //    vehiculo
            //};

            #endregion

            var data = (from a in context.icb_vehiculo
                        join b in context.modelo_vehiculo
                            on a.modvh_id equals b.modvh_codigo into aa
                        from b in aa.DefaultIfEmpty()
                        join c in context.bodega_concesionario
                            on a.icbvh_bodpro equals c.id into xx
                        from c in xx.DefaultIfEmpty()
                        join e in context.color_vehiculo
                            on a.colvh_id equals e.colvh_id into zz
                        from e in zz.DefaultIfEmpty()
                        join f in context.icb_tpeventos
                            on a.id_evento equals f.tpevento_id into ee
                        from f in ee.DefaultIfEmpty()
                        where a.vin == vin
                        select new
                        {
                            a.icbvh_id,
                            a.vin,
                            b.modvh_nombre,
                            e.colvh_nombre,
                            c.bodccs_nombre,
                            f.tpevento_nombre,
                            a.plan_mayor,
                        }).FirstOrDefault();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AgregarInspeccion(string planmayor, string vin, int? averiaId, int? gravedadId, int medidaId, int? zonaId,
            int? impactoId, string observaciones, string recibidoDe, string recibidoEn,
            int combustible, bool sinManual, bool sinManualGarantia, bool sinLlaves, bool sinControl, bool sucio,
            bool sinCintas, decimal kms, int? taller, DateTime? fecha)
        {
            DateTime insfecha = DateTime.Now;
            if (fecha != null)
            {
                insfecha = fecha.Value;
            }

            context.icb_inspeccionvehiculos.Add(new icb_inspeccionvehiculos
            {
                planmayor = planmayor,
                insp_vin = vin,
                insp_idaveria = Convert.ToInt32(averiaId),
                insp_idgravedadaveria = Convert.ToInt32(gravedadId),
                insp_recibidode = recibidoDe,
                insp_recibidoen = recibidoEn,
                insp_combustible = combustible,
                insp_medida = medidaId,
                insp_observacion = observaciones,
                insp_fechains = insfecha,
                insp_usuela = Convert.ToInt32(Session["user_usuarioid"]),
                insp_sincontrol = sinControl,
                insp_sincintas = sinCintas,
                insp_sucio = sucio,
                insp_sinllaves = sinLlaves,
                insp_sinmanual = sinManual,
                insp_sinmanualgarantia = sinManualGarantia,
                insp_km = kms,
                insp_zona = zonaId,
                insp_impacto = impactoId,
                taller_averia_id = taller,
                estado_averia_id = 1
            });
            bool result = context.SaveChanges() > 0;
            var data = new
            {
                result = true
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult eliminarInsp(int id)
        {
            icb_inspeccionvehiculos insp = context.icb_inspeccionvehiculos.Find(id);
            context.Entry(insp).State = EntityState.Deleted;
            int result = context.SaveChanges();
            if (result > 0)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult buscarInspecciones(string planmayor)
        {

            var inspeccionesActuales = (from inspeccion in context.icb_inspeccionvehiculos
                                        join averia in context.icb_averias
                                            on inspeccion.insp_idaveria equals averia.ave_id
                                        join impacto in context.icb_impacto_averia
                                            on inspeccion.insp_impacto equals impacto.impacto_id
                                        join zona in context.icb_zona_averia
                                            on inspeccion.insp_zona equals zona.zona_id
                                        where inspeccion.planmayor == planmayor
                                        select new
                                        {
                                            id_inspeccion = inspeccion.insp_id,
                                            averia = averia.ave_descripcion,
                                            medida = inspeccion.insp_medida,
                                            impacto = impacto.impacto_descripcion,
                                            zona = zona.zona_descripcion,
                                            observacion = inspeccion.insp_observacion,
                                            estadoa = inspeccion.estado_averia_id != null ? inspeccion.estadoAverias.nombre : "",
                                            estadoid = inspeccion.estado_averia_id != null ? context.estadoAverias.Where(x => x.nombre.Contains("creada") && x.id == inspeccion.estado_averia_id).FirstOrDefault().id != null ? true : false : false,
                                        }).ToList();
            var data = new
            {
                inspeccionesActuales
            };
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarInspeccionesActuales(string planmayor)
        {
            var inspeccionesActuales = (from inspeccion in context.icb_inspeccionvehiculos
                                        join averia in context.icb_averias
                                            on inspeccion.insp_idaveria equals averia.ave_id
                                        //join gravedad in context.icb_gravedad_averias
                                        //on inspeccion.insp_idgravedadaveria equals gravedad.grave_id
                                        join zona in context.icb_zona_averia
                                            on inspeccion.insp_zona equals zona.zona_id
                                        where inspeccion.planmayor == planmayor
                                        select new
                                        {
                                            id_inspeccion = inspeccion.insp_id,
                                            averia = averia.ave_descripcion,
                                            medida = inspeccion.insp_medida,
                                            //gravedad = gravedad.grave_descripcion,
                                            zona = zona.zona_descripcion
                                        }).ToList();
            return Json(inspeccionesActuales, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FinalizarInspeccion(string vin)
        {
            icb_vehiculo buscaVin = context.icb_vehiculo.FirstOrDefault(x => x.vin == vin);
            bool result = false;
            if (buscaVin != null)
            {
                buscaVin.icbvh_fecinsp_ingreso = DateTime.Now;
                context.Entry(buscaVin).State = EntityState.Modified;
                result = context.SaveChanges() > 0;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
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



        public JsonResult ValidarCampania(string placa) {
            var resultado = 0;

             icb_vehiculo vehiculocita = context.icb_vehiculo.Where(x => x.plac_vh == placa).FirstOrDefault();

            if (vehiculocita!=null)
                {
                var campania = (from campana in context.tcamptaller
                                join campanavin in context.tcamptallervin on
                                 campana.id equals campanavin.id_camp

                                where campanavin.vin == vehiculocita.vin
                                select new
                                    {
                                    campana.nombre,
                                    campana.referencia,
                                    campana.Descripcion,
                                    campana.numcircular,
                                    numdwn = context.tcamptallernumgwn.Where(x => x.idcampa == campana.id && x.estado == true).Select(o => o.numgwn).ToList()
                                    }).FirstOrDefault();
                if (campania != null)
                    {
                    return Json(campania, JsonRequestBehavior.AllowGet);
                    }
                else {
                 var   campan = 0;

                    return Json(campan, JsonRequestBehavior.AllowGet);
                    }
       

               
                }



            return Json(resultado, JsonRequestBehavior.AllowGet);

            }



    }
}