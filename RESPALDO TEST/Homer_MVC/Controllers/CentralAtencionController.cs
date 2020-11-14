using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
using Rotativa;
using Rotativa.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using OfficeOpenXml;

namespace Homer_MVC.Controllers
{
    public class CentralAtencionController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");

        private static Expression<Func<vw_browser_central_atencion, string>> GetColumnName(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(vw_browser_central_atencion), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<vw_browser_central_atencion, string>> lambda = Expression.Lambda<Func<vw_browser_central_atencion, string>>(menuProperty, menu);

            return lambda;
        }

        private static Expression<Func<datatablex, string>> GetColumnName2(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(datatablex), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<datatablex, string>> lambda = Expression.Lambda<Func<datatablex, string>>(menuProperty, menu);

            return lambda;
        }

        private static Expression<Func<vw_browser_cuadre_cajas, string>> GetColumnName3(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(vw_browser_cuadre_cajas), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<vw_browser_cuadre_cajas, string>> lambda = Expression.Lambda<Func<vw_browser_cuadre_cajas, string>>(menuProperty, menu);

            return lambda;
        }

        private static Expression<Func<datatablex, string>> GetColumnName4(string property)
        {
            ParameterExpression menu = Expression.Parameter(typeof(datatablex), "menu");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<datatablex, string>> lambda = Expression.Lambda<Func<datatablex, string>>(menuProperty, menu);

            return lambda;
        }


        // GET: CentralAtencion
        public ActionResult Index(int? menu)
        {
            if (Session["user_usuarioid"] != null)
            {
                var bodegas = context.bodega_concesionario.Where(d => d.bodccs_estado)
                    .Select(d => new { d.id, nombre = d.bodccs_cod + "-" + d.bodccs_nombre }).ToList();
                int rol = Convert.ToInt32(Session["user_rolid"]);
                int usuario = Convert.ToInt32(Session["user_usuarioid"]);
                icb_sysparameter admin1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P109").FirstOrDefault();
                int admin = admin1 != null ? Convert.ToInt32(admin1.syspar_value) : 1;
                if (rol != admin)
                {
                    //bodegasusuario
                    List<int> bode = context.bodega_usuario.Where(d => d.id_usuario == usuario).Select(d => d.id_bodega)
                        .ToList();
                    bodegas = context.bodega_concesionario.Where(d => d.bodccs_estado && bode.Contains(d.id))
                        .Select(d => new { d.id, nombre = d.bodccs_cod + "-" + d.bodccs_nombre }).ToList();
                }

                var permisos = (from acceso in context.rolacceso
                                join rolPerm in context.rolpermisos
                                on acceso.idpermiso equals rolPerm.id
                                where acceso.idrol == rol //&& rolPerm.codigo == "P40" 
                                select new { rolPerm.codigo }).ToList();

                var resultado1 = permisos.Where(x => x.codigo == "P45").Count() > 0 ? "Si" : "No";
                var resultado2 = permisos.Where(x => x.codigo == "P46").Count() > 0 ? "Si" : "No";


                ViewBag.bodega = new MultiSelectList(bodegas, "id", "nombre");
                ViewBag.fechaini = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy/MM/dd", new CultureInfo("en-US"));
                ViewBag.fechafin = DateTime.Now.ToString("yyyy/MM/dd", new CultureInfo("en-US"));

                ViewBag.liquidarot = resultado2;
                ViewBag.modificarot = resultado1;
                BuscarFavoritos(menu);
                return View();
            }

            return RedirectToAction("Login", "Login");
        }

        public JsonResult agregarOperaciones(int? cliente, int tecnico, string operacion, string codigoentrada, decimal valor)
        {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            bool existeTarifa = true;
            string mensajeExisteTarifa = "";
            decimal? valorCalculado = 0;
            decimal? ivaCalculado = 0;
            decimal? descuentoCalculado = 0;
            var resultado = 0;

            int orden = (from m in context.tdetallemanoobraot
                         join o in context.tencabezaorden
                         on m.idorden equals o.id
                         where o.codigoentrada == codigoentrada
                         select o.id).FirstOrDefault();

            tencabezaorden ordentrabajo = context.tencabezaorden.Where(d => d.id == orden).FirstOrDefault();
            if (ordentrabajo != null)
            {
                int idTecnico = (from tempario in context.ttecnicos
                                 where tempario.idusuario == tecnico
                                 select tempario.id).FirstOrDefault();

                var buscarTiempo = (from tempario in context.ttempario
                                    join codigoIva in context.codigo_iva
                                        on tempario.iva equals codigoIva.id
                                    where tempario.codigo == operacion
                                    select new
                                    {
                                        tiempo = tempario.tiempo != null ? tempario.tiempo : 0,
                                        tempario.precio,
                                        iva = codigoIva.porcentaje
                                    }).FirstOrDefault();

                //ttarifaclientebodega buscarTarifaClienteBodega = (from tarifaCliente in context.ttarifaclientebodega
                //                                                  where tarifaCliente.idtcliente == cliente
                //                                                  select tarifaCliente).FirstOrDefault();

                //if (buscarTiempo.tiempo == 0)
                //{
                //    ivaCalculado = buscarTiempo.precio != null
                //        ?
                //        (buscarTiempo.precio * 1 - descuentoCalculado) * buscarTiempo.iva / 100
                //        :
                //        buscarTarifaClienteBodega != null
                //            ? (buscarTarifaClienteBodega.valor * 1 - descuentoCalculado) * buscarTiempo.iva / 100
                //            : 0;

                //    valorCalculado = buscarTiempo.precio != null
                //        ?
                //        buscarTiempo.precio * 1 - descuentoCalculado + ivaCalculado
                //        :
                //        buscarTarifaClienteBodega != null
                //            ? 1 * buscarTarifaClienteBodega.valor - descuentoCalculado + ivaCalculado
                //            : 0;
                //}
                //else
                //{
                //    ivaCalculado = buscarTiempo.precio != null
                //        ?
                //        (buscarTiempo.precio * (decimal)buscarTiempo.tiempo - descuentoCalculado) *
                //        buscarTiempo.iva / 100
                //        :
                //        buscarTarifaClienteBodega != null
                //            ? (buscarTarifaClienteBodega.valor * (decimal)buscarTiempo.tiempo -
                //               descuentoCalculado) * buscarTiempo.iva / 100
                //            : 0;

                //    valorCalculado = buscarTiempo.precio != null
                //        ?
                //        buscarTiempo.precio * (decimal)buscarTiempo.tiempo - descuentoCalculado + ivaCalculado
                //        :
                //        buscarTarifaClienteBodega != null
                //            ? (decimal)buscarTiempo.tiempo * buscarTarifaClienteBodega.valor - descuentoCalculado +
                //              ivaCalculado
                //            : 0;
                //}

                tdetallemanoobraot operacionOt = new tdetallemanoobraot
                {
                    costopromedio = 0,
                    fecha = DateTime.Now,
                    idorden = orden,
                    idtecnico = idTecnico,
                    idtempario = operacion,
                    pordescuento = descuentoCalculado,
                    poriva = buscarTiempo.iva,
                    tiempo = Convert.ToDecimal(buscarTiempo.tiempo),
                    //valorunitario = buscarTiempo.precio != null ? Convert.ToDecimal(buscarTiempo.precio) : 0,
                    valorunitario = valor,
                    estado = "1",
                };
                context.tdetallemanoobraot.Add(operacionOt);
                int guardar = context.SaveChanges();

                if (guardar > 0)
                {
                    if (buscarTiempo.tiempo != null)
                    {
                        ordentrabajo.entrega = ordentrabajo.entrega.AddHours(Convert.ToDouble(buscarTiempo.tiempo));
                        context.Entry(ordentrabajo).State = EntityState.Modified;
                        context.SaveChanges();
                    }

                    var buscarOperacion = new
                    {
                        idope = operacionOt.id,
                        buscarTiempo.tiempo,
                       // precio = buscarTiempo.precio != null ? buscarTiempo.precio : buscarTarifaClienteBodega != null ? buscarTarifaClienteBodega.valor : 0,
                        ivaOperacion = buscarTiempo.iva != null ? buscarTiempo.iva : 0,
                        valorDescuento = descuentoCalculado,
                        valorIva = ivaCalculado,
                        valorTotal = valorCalculado,
                        existeTarifa,
                        idTecnico,
                        mensajeExisteTarifa
                    };
                    resultado = 1;
                    return Json(resultado, JsonRequestBehavior.AllowGet);
                }
                return Json(resultado, JsonRequestBehavior.AllowGet);
            }
            return Json(resultado, JsonRequestBehavior.AllowGet);

        }

        public JsonResult traerTiempo(int tecnico, int hr)
        {
            decimal? total;

            decimal? tiempo = (from tempario in context.ttecnicos
                               where tempario.idusuario == tecnico
                               select tempario.valorhora).FirstOrDefault();

            total = tiempo * hr;

            return Json(total, JsonRequestBehavior.AllowGet);
        }

        public JsonResult traerPrecio(string codigoOperacion)
        {

            var buscarTiempo = (from tempario in context.ttempario
                                join codigoIva in context.codigo_iva
                                    on tempario.iva equals codigoIva.id
                                where tempario.codigo == codigoOperacion
                                select new
                                {
                                    tiempo = tempario.tiempo != null ? tempario.tiempo : 0,
                                    tempario.precio,
                                    iva = codigoIva.porcentaje
                                }).FirstOrDefault();

            return Json(buscarTiempo, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BrowserCajas()
        {

            var listaClientes = (from t in context.vw_usuarios_central_atencion
                                 where t.idestado == 19
                                 select new
                                   {
                                       id=t.doc_tercero,
                                       cliente =t.nombreCliente,

                                   }).ToList();

            ViewBag.cliente = new SelectList(listaClientes, "id", "cliente");

            var listaTransporte = (from t in context.Transporte
                                   where t.habilitado == true
                               select new
                               {
                                   t.id,
                                   tipo = t.tipo_transporte,

                               }).ToList();

            ViewBag.transporte = new SelectList(listaTransporte, "id", "tipo");

            var listaPrioridad = (from t in context.Prioridad
                                  where t.habilitado == true
                                  select new
                                  {
                                      t.id,
                                      tipo = t.tipo_prioridad,

                                  }).ToList();

            ViewBag.prioridad = new SelectList(listaPrioridad, "id", "tipo");

            var listaMensajero = (from t in context.tercero_empleado
                                  join e in context.icb_terceros
                                      on t.tercero_id equals e.tercero_id
                                  where t.teremp_cargo == 4
                                  select new
                                  {
                                      id=t.emp_tercero_id,
                                      nombre = e.prinom_tercero + " " + e.apellido_tercero,

                                  }).ToList();

            ViewBag.mensajero = new SelectList(listaMensajero, "id", "nombre");



            return View();
        }

        public JsonResult CrearMensajeria(AgendaMensajeria model) {

            if (ModelState.IsValid)
            {

                try
                {
                    bool citas = CalcularDisponible(model.mensajero, DateTime.Now, model.desde, model.hasta);

                    if (citas)
                    {
                        if (model.hasta > model.desde)
                        {

                            agenda_mensajero agenda = new agenda_mensajero();

                            agenda.hasta = model.hasta;
                            agenda.desde = model.desde;
                            agenda.idmensajero = model.mensajero;
                            agenda.descripcion = model.descripcion;
                            agenda.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                            agenda.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                            agenda.fec_creacion = DateTime.Now;
                            agenda.fec_actualizacion = DateTime.Now;

                            context.agenda_mensajero.Add(agenda);
                            bool result = context.SaveChanges() > 0;

                            if (result)
                            {
                                return Json(1, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {

                                return Json(0, JsonRequestBehavior.AllowGet);

                            }


                        }
                        else
                        {

                            return Json(0, JsonRequestBehavior.AllowGet);

                        }
                    }
                    else {

                        return Json(0, JsonRequestBehavior.AllowGet);

                    }


                }
                catch (Exception)
                {
                    return Json(0, JsonRequestBehavior.AllowGet);
                    throw;
                }

                

                
            }
            else {

                return Json(0, JsonRequestBehavior.AllowGet);

            }

            

        }

        public bool CalcularDisponible(int idmensajero, DateTime fecha, DateTime hasta, DateTime desde)
        {
              List<agenda_mensajero> citasDelMes = context.agenda_mensajero.Where(x =>
                x.idmensajero == idmensajero && x.desde.Year == fecha.Year && x.desde.Month == fecha.Month).ToList();

            foreach (agenda_mensajero cita in citasDelMes)
            {
                if (DateTime.Compare(hasta, cita.desde) == 0)
                {
                    return false;
                }
                else if (DateTime.Compare(hasta, cita.desde) > 0 && DateTime.Compare(hasta, cita.hasta) < 0)
                {
                    return false;
                }
                else if (DateTime.Compare(hasta, cita.desde) > 0 && DateTime.Compare(desde, cita.hasta) < 0)
                {
                    return false;
                }
                else if (DateTime.Compare(desde, cita.desde) > 0 && DateTime.Compare(desde, cita.hasta) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        public JsonResult traerTotales(int caja)
        {
            var cajaNombre = context.icb_caja_detalle.Where(d => d.nombre == caja).Select(d => d.caja).FirstOrDefault();

            var buscar = (from d in context.detalle_formas_pago_orden
                          where d.caja == cajaNombre
                          select new
                          {
                              id = d.idorden,
                              idmedio = d.idformas_pago,
                              medio = d.formas_pago.formapago,
                              valor = d.valorRecibido,
                              caja = d.caja,

                          }).ToList();

            var efectivo = buscar.Where(d => d.medio.Contains("Efectivo")).Select(x => x.valor).Sum();

            var tarjetas = buscar.Where(d => d.medio.Contains("Tarjeta")).Select(x => x.valor).Sum();

            var cheques = buscar.Where(d => d.medio.Contains("Cheque")).Select(x => x.valor).Sum();

            var recibos = buscar.Where(d => d.medio.Contains("Cupo")).Select(x => x.valor).Sum();


            var datos = new { efectivo, tarjetas, cheques, recibos };

            return Json(datos, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult  CuadreDiario(int? menu)
        {
            //busco el usuario
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            //busco el rol del usuario
            int rol = Convert.ToInt32(Session["user_rolid"]);
            //busco la bodega del usuario
            int bodegaactual = Convert.ToInt32(Session["user_bodega"]);

            var permisos = (from acceso in context.rolacceso
                            join rolPerm in context.rolpermisos
                            on acceso.idpermiso equals rolPerm.id
                            where acceso.idrol == rol
                            select new { rolPerm.codigo }).ToList();

            var resultado = permisos.Where(x => x.codigo == "P48").Count() > 0 ? "Si" : "No";

            ViewBag.Permiso = resultado;

            //ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();

            var list3 = (from bod in context.bodega_concesionario
                             //where bod.id == bodegaactual
                         select new
                         {
                             id = bod.id,
                             bodega = bod.bodccs_nombre,
                         }).ToList();

            List<SelectListItem> lista3 = new List<SelectListItem>();
            foreach (var item in list3)
            {
                lista3.Add(new SelectListItem
                {
                    Text = item.bodega,
                    Value = item.id.ToString()
                });
            }
            ViewBag.bodega = lista3;
            ViewBag.bodccs_cod = lista3;
            ViewBag.bodegaActual = bodegaactual;

            var list4 = (from c in context.icb_caja_nombre
                         select new
                         {
                             id = c.id,
                             nombre = c.nombre,
                         }).ToList();

            List<SelectListItem> lista4 = new List<SelectListItem>();
            foreach (var item in list4)
            {
                lista4.Add(new SelectListItem
                {
                    Text = item.nombre.ToString(),
                    Value = item.id.ToString()
                });
            }
            ViewBag.nombre = lista4;

            return View();
        }

        [HttpPost]
        public ActionResult CuadreDiario()
        {
            //var result = 0;
            icb_cuadre_caja c = new icb_cuadre_caja();

            var total = Request["total"];
            var caja = Request["nombre"];

            var nombre = Convert.ToInt32(caja);

            var buscar = context.icb_caja_detalle.Where(d => d.nombre == nombre).Select(d => d.caja).FirstOrDefault();

            c.bodega = Convert.ToInt32(Request["bodega"]);
            c.caja = buscar;
            c.fecha = Convert.ToDateTime(Request["fecha"]);

            c.efectivo = Convert.ToDecimal(Request["efectivo"]);
            c.sistema_efectivo = Convert.ToDecimal(Request["sistema_efectivo"]);
            c.diferencia_efectivo = Convert.ToDecimal(Request["diferencia_efectivo"]);

            c.tarjetas = Convert.ToDecimal(Request["tarjetas"]);
            c.sistema_tarjetas = Convert.ToDecimal(Request["sistema_tarjetas"]);
            c.diferencia_tarjetas = Convert.ToDecimal(Request["diferencia_tarjetas"]);

            c.cheques = Convert.ToDecimal(Request["cheques"]);
            c.sistema_cheques = Convert.ToDecimal(Request["sistema_cheques"]);
            c.diferencia_cheques = Convert.ToDecimal(Request["diferencia_cheques"]);

            c.recibos = Convert.ToDecimal(Request["recibos"]);
            c.sistema_recibos = Convert.ToDecimal(Request["sistema_recibos"]);
            c.diferencia_recibos = Convert.ToDecimal(Request["diferencia_recibos"]);

            c.total_ingresos = Convert.ToDecimal(Request["total_ingresos"]);
            c.total_egresos = Convert.ToDecimal(Request["total_egresos"]);
            c.total_sistema = Convert.ToDecimal(Request["total_sistema"]);
            c.total = Convert.ToDecimal(Request["total"]);

            if (Convert.ToDecimal(total) == 0)
            {
                c.estado = 2;
                c.cerrada = true;
            }
            else {
                c.estado = 1;
                c.cerrada = false;
            }

            context.icb_cuadre_caja.Add(c);
            int resultado = context.SaveChanges();

            if (resultado > 0)
            {
                TempData["mensaje"] = "El registro fue exitoso!";
            }
            else
            {
                TempData["mensaje_error"] = "Error, intente de nuevo";
            }

            return RedirectToAction("CuadreDiario", "CentralAtencion");

        }

        public JsonResult LiquidarOT(string codigoentrada)
        {
            var result = 0;

            var OT = (from orden in context.tencabezaorden
                      where orden.codigoentrada == codigoentrada
                      select new
                      {
                          orden.id
                      }).FirstOrDefault();


            tencabezaorden e = new tencabezaorden();

            var state = context.tencabezaorden.Find(OT.id);

            state.estadoorden = 24;

            context.Entry(state).State = EntityState.Modified;
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

        public JsonResult traerCupoCredito(string codigoentrada)
        {
            var OT = (from orden in context.tencabezaorden
                      where orden.codigoentrada == codigoentrada
                      select new
                      {
                          orden.id,
                          orden.tercero
                      }).FirstOrDefault();

            var tercero = (from t in context.tercero_cliente
                           where t.tercero_id == OT.tercero
                           select new
                           {
                               t.cltercero_id,
                               cupocredito = t.cupocredito != null ? t.cupocredito : 0,
                           }).ToList();

            var data = tercero.Select(x => new
            {
                x.cltercero_id,
                x.cupocredito
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult datosOrden(int idorden)
        {

            var OT = (from o in context.tencabezaorden
                      join t in context.icb_terceros
                      on o.tercero equals t.tercero_id
                      where o.id == idorden
                      select new
                      {
                          o.id,
                          t.doc_tercero,
                          t.prinom_tercero,
                          t.apellido_tercero,
                          nombre= t.prinom_tercero+" "+ t.apellido_tercero

                      }).FirstOrDefault();

            return Json(OT, JsonRequestBehavior.AllowGet);
        }

        public JsonResult TraerMediosPago(int? idorden, int? idEncabezado)
        {

            if (idorden!= null)
            {
                var data = (from f in context.formas_pago
                            join d in context.detalle_formas_pago_orden
                            on f.id equals d.idformas_pago
                            where d.idorden == idorden
                            select new
                            {
                                f.formapago,
                                d.valorRecibido,
                                d.vaucher,
                                d.cheque,
                                d.pendiente,
                               // pendiente2=d.pendiente!=null?d.pendiente.Value.ToString("N0",new CultureInfo("is-IS")):"",
                            }).ToList();
                var data2 = data.Select(d => new {
                    d.formapago,
                    d.valorRecibido,
                    valorRecibido2 = d.valorRecibido != null ? d.valorRecibido.Value.ToString("N0", new CultureInfo("is-IS")) : "",
                    d.vaucher,
                    d.cheque,
                    d.pendiente,
                    pendiente2=d.pendiente!=null?d.pendiente.Value.ToString("N0",new CultureInfo("is-IS")):"",
                }).ToList();
                return Json(data2, JsonRequestBehavior.AllowGet);
            }
            else if (idEncabezado != null)
            {
                var data = (from f in context.formas_pago
                            join d in context.detalle_formas_pago_orden
                            on f.id equals d.idformas_pago
                            where d.idencabezado == idEncabezado
                            select new
                            {
                                f.formapago,
                                d.valorRecibido,
                                d.vaucher,
                                d.cheque,
                                d.pendiente,
                               // pendiente2 = d.pendiente != null ? d.pendiente.Value.ToString("N0", new CultureInfo("is-IS")) : "",
                            }).ToList();
                var data2 = data.Select(d => new {
                    d.formapago,
                    d.valorRecibido,
                    valorRecibido2 = d.valorRecibido != null ? d.valorRecibido.Value.ToString("N0", new CultureInfo("is-IS")) : "",
                    d.vaucher,
                    d.cheque,
                    d.pendiente,
                    pendiente2 = d.pendiente != null ? d.pendiente.Value.ToString("N0", new CultureInfo("is-IS")) : "",
                }).ToList();
                return Json(data2, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);

        }

        public JsonResult cuadreSaldo(string[] idEncabezado)
        {

            var result = 0;
            decimal encab = 0;
            int resultado = 0;

            foreach (var item in idEncabezado)
            {
                encab = context.encab_documento.Where(d => d.idencabezado == Convert.ToInt32(item)).Select(d => d.valor_total).FirstOrDefault();
                var b = context.encab_documento.Find(Convert.ToInt32(item));
                b.valor_aplicado = encab;
                context.Entry(b).State = EntityState.Modified;
                resultado = context.SaveChanges();
            }
           
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

        public JsonResult asignarMedioPago(int? idorden, int? formaPago, string valor,int? idencabezado)
        {

            var result = 0;
            int resultado = 0;
            List<decimal> list = new List<decimal>();
            decimal recibido = 0;

            var encab = context.encab_documento.Where(d => d.tipo == 3106 && d.orden_taller == idorden).FirstOrDefault();
            var num = encab.valor_total.ToString().Split(',');


            if (!string.IsNullOrWhiteSpace(valor) && idorden != null && formaPago != null && idencabezado != null)
            {
                var v = Convert.ToDecimal(valor);

                valor = Convert.ToInt32(v).ToString();

                var data1 = (from f in context.formas_pago
                             join d in context.detalle_formas_pago_orden
                             on f.id equals d.idformas_pago
                             where d.idorden == idorden
                             select new
                             {
                                 f.formapago,
                                 d.valor,
                                 d.vaucher,
                                 d.cheque,
                                 d.pendiente,
                                 d.valorRecibido,
                             }).ToList();



                if (data1.Count == 0)
                {
                    detalle_formas_pago_orden e = new detalle_formas_pago_orden();

                    var pendiente = Convert.ToInt32(num[0]) - Convert.ToInt32(v);

                    recibido = Convert.ToDecimal(valor);


                    e.idorden = idorden;
                    e.idformas_pago = formaPago;
                    e.valor = Convert.ToDecimal(valor);
                    e.pendiente = pendiente;
                    e.valorRecibido = recibido;

                    context.detalle_formas_pago_orden.Add(e);
                    resultado = context.SaveChanges();
                }
                else
                {
                    detalle_formas_pago_orden e = new detalle_formas_pago_orden();
                    var df = context.detalle_formas_pago_orden.Where(d => d.idorden == idorden).OrderByDescending(d => d.id_detalle).FirstOrDefault();
                    var pendiente = Convert.ToInt32(df.pendiente) - Convert.ToInt32(v);
                    recibido = Convert.ToInt32(df.valorRecibido) + Convert.ToInt32(v);

                    e.idorden = idorden;
                    e.idformas_pago = formaPago;
                    e.valor = Convert.ToDecimal(valor);
                    e.pendiente = pendiente;
                    e.valorRecibido = recibido;

                    context.detalle_formas_pago_orden.Add(e);
                    resultado = context.SaveChanges();

                }

                var data = (from f in context.formas_pago
                            join d in context.detalle_formas_pago_orden
                            on f.id equals d.idformas_pago
                            where d.idorden == idorden
                            select new
                            {
                                f.formapago,
                                d.valor,
                                d.vaucher,
                                d.cheque,
                                d.pendiente,

                            }).ToList();



                if (resultado > 0)
                {
                    result = 1;
                    //cuadreSaldo(idencabezado, valor);
                }
                else
                {
                    result = 0;
                }
                //detalle_formas_pago_orden dfp = context.detalle_formas_pago_orden.Where(d => d.idorden == idorden).LastOrDefault();
                detalle_formas_pago_orden dfp = context.detalle_formas_pago_orden.Where(d => d.idorden == idorden).OrderByDescending(d => d.id_detalle).FirstOrDefault();
                var cantPendiente = dfp.pendiente != null ? dfp.pendiente : 0;
                var cantidadRecibida = dfp.valorRecibido != null ? dfp.valorRecibido : 0;


                var datos = new { result, data, cantPendiente, cantidadRecibida };
                return Json(datos, JsonRequestBehavior.AllowGet);
            }
            else {
                var datos = "Datos incorrectos";
                return Json(datos, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult asignarMedio(int? idorden, int? medioPago, string valor, string total, string vaucher, string Cheque)
        {

            if (ModelState.IsValid)
            {
                int result = 0;
                var valorConvertido = valor.Replace(".", "");
                var totalConvertido = total.Replace(".", "");

                try
                {

                    if (Convert.ToInt32(valorConvertido) <= Convert.ToInt32(totalConvertido))
                    {
                        detalle_formas_pago_orden medios = new detalle_formas_pago_orden
                        {

                            idorden = idorden,
                            idformas_pago = medioPago,
                            valor = Convert.ToDecimal(totalConvertido),
                            valorRecibido = Convert.ToDecimal(valorConvertido),
                        };

                        if (!string.IsNullOrWhiteSpace(vaucher))
                        {
                            medios.vaucher = vaucher;
                        }
                        if (!string.IsNullOrWhiteSpace(Cheque))
                        {
                            medios.cheque = Convert.ToInt64(Cheque);
                        }

                        context.detalle_formas_pago_orden.Add(medios);
                        bool resultado = context.SaveChanges() > 0;

                        var buscar = context.detalle_formas_pago_orden.Where(d => d.idorden == idorden).OrderByDescending(d => d.id_detalle).FirstOrDefault();
                        var buscar2 = context.detalle_formas_pago_orden.Where(d => d.idorden == idorden).Select(d=> d.valorRecibido).ToList();
                        var cantPendiente = buscar.pendiente != null ? buscar.pendiente : 0;
                        var cantidadRecibida = buscar2.Sum();


                        if (resultado)
                        {
                            result = 1;
                            var datos = new { result, cantPendiente, cantidadRecibida };
                            return Json(datos, JsonRequestBehavior.AllowGet);

                        }
                        else
                        {
                            return Json(0, JsonRequestBehavior.AllowGet);
                        }

                    }

                    return Json(0, JsonRequestBehavior.AllowGet);

                }
                catch (Exception)
                {

                    throw;
                }
            }

            return Json(0, JsonRequestBehavior.AllowGet);

        }

        public JsonResult asignarCupo(string codigoentrada, int valor_cupon)
        {
            var result = 0;
            //revisar validacion
            var OT = (from orden in context.tencabezaorden
                      where orden.codigoentrada == codigoentrada
                      select new
                      {
                          orden.id
                      }).FirstOrDefault();


            tencabezaorden e = new tencabezaorden();

            var state = context.tencabezaorden.Find(OT.id);

            state.valor_cupon = valor_cupon;

            context.Entry(state).State = EntityState.Modified;
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

        public JsonResult buscarOTLiquidadas()
        {

            var OT = (from central in context.vw_browser_central_atencion
                      where central.idestado == 19
                      select new
                      {
                          central.id,
                          central.codigoentrada,
                          central.placa_plan,
                          central.modvh_nombre,
                          central.nombrecliente,
                          central.asesor_tecnico,
                          central.idestado,
                          central.Descripcion,
                          central.color_estado,
                          central.fecha2,
                          central.razon_ingreso,
                          central.razon_ingreso2,
                      }).ToList();

            var data = OT.Select(d => new
            {
                d.id,
                d.codigoentrada,
                d.placa_plan,
                d.modvh_nombre,
                d.nombrecliente,
                d.asesor_tecnico,
                d.idestado,
                d.Descripcion,
                d.color_estado,
                d.fecha2,
                //fecha2=d.fecha2.Tostring("yyyy/MM/dd", new CultureInfo("is-IS")),
                razon_ingreso = d.razon_ingreso != null ? d.razon_ingreso : "",
                razon_ingreso2 = d.razon_ingreso2 != null ? d.razon_ingreso2 : "",
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult validarFormaPago(int forma)
        {
            int resultado = 0;

            var fpago = context.formas_pago.Where(d => d.id == forma).FirstOrDefault();



            if (fpago.formapago.Contains("Efectivo"))
            {
                resultado = 1;
            }
            else if (fpago.formapago.Contains("Cheque"))
            {
                resultado = 2;
            }
            else if (fpago.formapago.Contains("Cupo"))
            {
                resultado = 3;
            } else if (fpago.formapago.Contains("Tarjeta")) {
                
                resultado = 4;
            }


            return Json(resultado, JsonRequestBehavior.AllowGet);
        }

        public ActionResult parametroTipoBolsa()
        {
            return View();
        }

        public ActionResult editarTipoBolsa(string menu,int id) {

            icb_bolsa b = context.icb_bolsa.Where(d => d.id == id).FirstOrDefault();

            ViewBag.id = b.id;
            ViewBag.bolsa = b.bolsa;
            ViewBag.valor = Math.Round(Convert.ToDecimal(b.valor));

            return View();
        }

        public JsonResult buscarBolsas()
        {

            var data = (from e in context.icb_bolsa
                        select new
                        {
                            id = e.id,
                            bolsa = e.bolsa,
                            valor = e.valor,
                            estado = e.estado
                        }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult agregarBolsa(string bolsa, string valor, bool estado)
        {
            var result = 0;
            icb_bolsa b = new icb_bolsa();

            b.bolsa = bolsa;
            b.valor = Convert.ToDecimal(valor);
            b.estado = estado;

            context.icb_bolsa.Add(b);
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

       
        [HttpGet]
        public ActionResult creacionCaja(int? menu) {

            ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
            ViewBag.doccons_idtpdoc = context.tp_doc_registros_tipo.OrderBy(x => x.descripcion).ToList();
            ViewBag.responsable = context.users.OrderBy(x => x.user_nombre).ToList();
            ViewBag.mediosPago = context.formas_pago.OrderBy(x => x.formapago).ToList();

            //busco el usuario
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            //busco el rol del usuario
            int rol = Convert.ToInt32(Session["user_rolid"]);
            //busco la bodega del usuario
            int bodegaactual = Convert.ToInt32(Session["user_bodega"]);


            var list = (from e in context.icb_caja_estados
                        select new
                        {
                            id = e.id,
                            value = e.estado
                        }).ToList();

            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in list)
            {
                lista.Add(new SelectListItem
                {
                    Text = item.value,
                    Value = item.id.ToString()
                });
            }
            ViewBag.estado = lista;

            var list2 = (from e in context.users
                             //where e.user_id== usuario
                         select new
                         {
                             id = e.user_id,
                             value = e.user_nombre + " " + e.user_apellido
                         }).ToList();

            List<SelectListItem> lista2 = new List<SelectListItem>();
            foreach (var item in list2)
            {
                lista2.Add(new SelectListItem
                {
                    Text = item.value,
                    Value = item.id.ToString()
                });
            }
            ViewBag.responsable1 = lista2;
            ViewBag.usuarioActual = usuario;

            var list3 = (from bod in context.bodega_concesionario
                             //where bod.id == bodegaactual
                         select new
                         {
                             id = bod.id,
                             value = bod.bodccs_nombre,
                         }).ToList();

            List<SelectListItem> lista3 = new List<SelectListItem>();
            foreach (var item in list3)
            {
                lista3.Add(new SelectListItem
                {
                    Text = item.value,
                    Value = item.id.ToString()
                });
            }
            ViewBag.bodega = lista3;
            ViewBag.bodegaActual = bodegaactual;


            var list4 = (from a in context.area_bodega
                             where a.id_bodega == bodegaactual
                         select new
                         {
                             id = a.areabod_id,
                             value = a.areabod_nombre,
                         }).ToList();

            List<SelectListItem> lista4 = new List<SelectListItem>();
            foreach (var item in list4)
            {
                lista4.Add(new SelectListItem
                {
                    Text = item.value,
                    Value = item.id.ToString()
                });
            }
            ViewBag.area = lista4;

            var list5 = (from f in context.formas_pago
                         select new
                         {
                             id = f.id,
                             value = f.formapago+"("+ f.bancos.Descripcion + ")",
                         }).ToList();

            List<SelectListItem> lista5 = new List<SelectListItem>();
            foreach (var item in list5)
            {
                lista5.Add(new SelectListItem
                {
                    Text = item.value,
                    Value = item.id.ToString()
                });
            }
            ViewBag.mediosPago1 = lista5;

            return View();
        
        }

        [HttpPost]
        public ActionResult creacionCaja()
        {

            string bodegaSeleccionada = Request["bodega"];
            string numero = Request["numero"];
            string cajaNombre = Request["nombre"];
            var fechaSeleccionada = DateTime.Now;
            string documentosSeleccionadas = Request["doccons_idtpdoc"];
            string responsablesSeleccionadas = Request["responsable"];
            string mediosPagosSeleccionadas = Request["mediosPago"];

            var caja = Convert.ToInt32(numero);

            var tablacaja = context.icb_caja.Where(d => d.numero == caja).Select(d => d.numero).Count();

            int idcaja = 0;
            int idcajaNombre = 0;

            int resultado = 0;

            if (tablacaja > 0)
            {
                TempData["mensaje_error"] = "El numero ingresado ya esta registrado!";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(bodegaSeleccionada))
                {
                    TempData["mensaje_error"] = "Debe asignar una bodega!";
                }

                if (string.IsNullOrWhiteSpace(documentosSeleccionadas))
                {
                    TempData["mensaje_error"] = "Debe asignar minimo un documento!";
                }

                if (string.IsNullOrWhiteSpace(responsablesSeleccionadas))
                {
                    TempData["mensaje_error"] = "Debe asignar minimo un responsable!";
                }

                if (string.IsNullOrWhiteSpace(mediosPagosSeleccionadas))
                {
                    TempData["mensaje_error"] = "Debe asignar minimo un medio de pago!";
                }

                if (!string.IsNullOrWhiteSpace(bodegaSeleccionada) && !string.IsNullOrWhiteSpace(documentosSeleccionadas) && !string.IsNullOrWhiteSpace(responsablesSeleccionadas) && !string.IsNullOrWhiteSpace(mediosPagosSeleccionadas) && !string.IsNullOrWhiteSpace(cajaNombre))
                {

                    string[] documentosId = documentosSeleccionadas.Split(',');
                    string[] responsablesId = responsablesSeleccionadas.Split(',');
                    string[] mediosPagoSId = mediosPagosSeleccionadas.Split(',');

                    icb_caja_nombre n = new icb_caja_nombre();
                    n.nombre= cajaNombre;
                    context.icb_caja_nombre.Add(n);
                    context.SaveChanges();
                    idcajaNombre=n.id;

                    icb_caja c = new icb_caja();

                    c.numero = Convert.ToInt32(Request["numero"]);
                    c.grupo = Request["grupo"];
                    c.area = Convert.ToInt32(Request["area"]);
                    c.bodega = Convert.ToInt32(Request["bodega"]);
                    c.estado = Convert.ToInt32(Request["estado"]);

                    context.icb_caja.Add(c);
                    context.SaveChanges();
                    idcaja= c.id;

                    icb_caja_detalle d = new icb_caja_detalle();

                    d.nombre = idcajaNombre;
                    d.caja = idcaja;

                    context.icb_caja_detalle.Add(d);
                    context.SaveChanges();

                    foreach (string substring in documentosId)
                    {

                        transaciones_caja t = new transaciones_caja();

                        t.caja = idcaja;
                        t.transacion = Convert.ToInt32(substring);
                        context.transaciones_caja.Add(t);
                        context.SaveChanges();

                    }//transaciones

                    foreach (var item2 in responsablesId)
                    {
                        usuarios_caja u = new usuarios_caja();

                        u.usuario = Convert.ToInt32(item2);
                        u.fecha = Convert.ToDateTime(fechaSeleccionada);
                        u.caja = idcaja;

                        context.usuarios_caja.Add(u);
                        context.SaveChanges();
                    }

                    foreach (var item2 in mediosPagoSId)
                    {
                        medios_caja m = new medios_caja();

                        m.medio = Convert.ToInt32(item2);
                        m.caja = idcaja;

                        context.medios_caja.Add(m);
                        context.SaveChanges();
                    }

                    resultado = +1;

                    if (resultado > 0)
                    {
                        TempData["mensaje"] = "El registro fue exitoso!";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "El registro fue exitoso!";
                    }
                }


            }

            return RedirectToAction("creacionCaja", "CentralAtencion");

        }

        public JsonResult actualizarBolsa(int id, string bolsa, string valor, bool? estado)
        {

            var result = 0;
            var b = context.icb_bolsa.Find(id);

            b.bolsa = bolsa;
            b.valor = Convert.ToDecimal(valor);
            b.estado = Convert.ToBoolean(estado);

            context.Entry(b).State = EntityState.Modified;
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

        public ActionResult BrowserCuadreCaja(int? menu,int id) {

            ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();

            var list3 = (from bod in context.bodega_concesionario
                         select new
                         {
                             id = bod.id,
                             bodega = bod.bodccs_nombre,
                         }).ToList();

            List<SelectListItem> lista3 = new List<SelectListItem>();
            foreach (var item in list3)
            {
                lista3.Add(new SelectListItem
                {
                    Text = item.bodega,
                    Value = item.id.ToString()
                });
            }
            ViewBag.bodega = lista3;


            var list4 = (from c in context.icb_caja_nombre
                         select new
                         {
                             id = c.id,
                             nombre = c.nombre,
                         }).ToList();

            List<SelectListItem> lista4 = new List<SelectListItem>();
            foreach (var item in list4)
            {
                lista4.Add(new SelectListItem
                {
                    Text = item.nombre.ToString(),
                    Value = item.id.ToString()
                });
            }
            ViewBag.nombre = lista4;

            icb_cuadre_caja b = context.icb_cuadre_caja.Where(d => d.id == id).FirstOrDefault();

            var detalle = context.icb_caja_detalle.Where(d => d.caja == b.caja).Select(d => d.nombre).FirstOrDefault();

            ViewBag.id = b.id;
            ViewBag.bodega1 = b.icb_caja.bodega_concesionario.id;
            ViewBag.fecha1 = b.fecha.Value.ToString("yyyy-MM-dd");
            ViewBag.nombre1 = detalle;

            ViewBag.efectivo = Math.Round(Convert.ToDecimal(b.efectivo));
            ViewBag.sistema_efectivo = Math.Round(Convert.ToDecimal(b.sistema_efectivo));
            ViewBag.diferencia_efectivo = Math.Round(Convert.ToDecimal(b.diferencia_efectivo));

            ViewBag.tarjetas = Math.Round(Convert.ToDecimal(b.tarjetas));
            ViewBag.sistema_tarjetas = Math.Round(Convert.ToDecimal(b.sistema_tarjetas));
            ViewBag.diferencia_tarjetas = Math.Round(Convert.ToDecimal(b.diferencia_tarjetas));

            ViewBag.cheques = Math.Round(Convert.ToDecimal(b.cheques));
            ViewBag.sistema_cheques = Math.Round(Convert.ToDecimal(b.sistema_cheques));
            ViewBag.diferencia_cheques = Math.Round(Convert.ToDecimal(b.diferencia_cheques));

            ViewBag.recibos = Math.Round(Convert.ToDecimal(b.recibos));
            ViewBag.sistema_recibos = Math.Round(Convert.ToDecimal(b.sistema_recibos));
            ViewBag.diferencia_recibos = Math.Round(Convert.ToDecimal(b.diferencia_recibos));

            ViewBag.total_ingresos = Math.Round(Convert.ToDecimal(b.total_ingresos));
            ViewBag.total_egresos = Math.Round(Convert.ToDecimal(b.total_egresos));
            ViewBag.total_sistema = Math.Round(Convert.ToDecimal(b.total_sistema));
            ViewBag.total1 = Math.Round(Convert.ToDecimal(b.total));

            return View();
        }

        public ActionResult CuadreCaja(int? menu)
        {
            //busco el usuario
            int usuario = Convert.ToInt32(Session["user_usuarioid"]);
            //busco el rol del usuario
            int rol = Convert.ToInt32(Session["user_rolid"]);
            //busco la bodega del usuario
            int bodegaactual = Convert.ToInt32(Session["user_bodega"]);

            var permisos = (from acceso in context.rolacceso
                            join rolPerm in context.rolpermisos
                            on acceso.idpermiso equals rolPerm.id
                            where acceso.idrol == rol
                            select new { rolPerm.codigo }).ToList();

            var resultado = permisos.Where(x => x.codigo == "P48").Count() > 0 ? "Si" : "No";

            ViewBag.Permiso = resultado;

            //ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();

            var list = (from bod in context.bodega_concesionario
                         select new
                         {
                             id = bod.id,
                             bodega = bod.bodccs_nombre,
                         }).ToList();

            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in list)
            {
                lista.Add(new SelectListItem
                {
                    Text = item.bodega,
                    Value = item.id.ToString()
                });
            }
            ViewBag.bodccs_cod = lista;
            ViewBag.bodegaActual = bodegaactual;

            //var list2 = (from c in context.icb_caja
            //            select new
            //            {
            //                id = c.id,
            //                num = c.numero,
            //            }).ToList();

            //List<SelectListItem> lista2 = new List<SelectListItem>();
            //foreach (var item in list2)
            //{
            //    lista2.Add(new SelectListItem
            //    {
            //        Text = item.num.ToString(),
            //        Value = item.id.ToString()
            //    });
            //}
            //ViewBag.numero = lista2;

            return View();
        }

        [HttpGet]
        public ActionResult CuadreCajaUpdate(int? menu,int id)
        {
            ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();

            var list3 = (from bod in context.bodega_concesionario
                         select new
                         {
                             id = bod.id,
                             bodega = bod.bodccs_nombre,
                         }).ToList();

            List<SelectListItem> lista3 = new List<SelectListItem>();
            foreach (var item in list3)
            {
                lista3.Add(new SelectListItem
                {
                    Text = item.bodega,
                    Value = item.id.ToString()
                });
            }
            ViewBag.bodega = lista3;


            var list4 = (from c in context.icb_caja_nombre
                         select new
                         {
                             id = c.id,
                             nombre = c.nombre,
                         }).ToList();

            List<SelectListItem> lista4 = new List<SelectListItem>();
            foreach (var item in list4)
            {
                lista4.Add(new SelectListItem
                {
                    Text = item.nombre.ToString(),
                    Value = item.id.ToString()
                });
            }
            ViewBag.nombre = lista4;

            icb_cuadre_caja b = context.icb_cuadre_caja.Where(d => d.id == id).FirstOrDefault();

            var detalle = context.icb_caja_detalle.Where(d => d.caja == b.caja).Select(d=> d.nombre).FirstOrDefault();

            ViewBag.id = b.id;
            ViewBag.bodega1 = b.icb_caja.bodega_concesionario.id;
            ViewBag.fecha1 = b.fecha.Value.ToString("yyyy-MM-dd");
            ViewBag.nombre1 = detalle;

            ViewBag.efectivo = Math.Round(Convert.ToDecimal(b.efectivo));
            ViewBag.sistema_efectivo = Math.Round(Convert.ToDecimal(b.sistema_efectivo));
            ViewBag.diferencia_efectivo = Math.Round(Convert.ToDecimal(b.diferencia_efectivo));

            ViewBag.tarjetas = Math.Round(Convert.ToDecimal(b.tarjetas));
            ViewBag.sistema_tarjetas = Math.Round(Convert.ToDecimal(b.sistema_tarjetas));
            ViewBag.diferencia_tarjetas = Math.Round(Convert.ToDecimal(b.diferencia_tarjetas));

            ViewBag.cheques = Math.Round(Convert.ToDecimal(b.cheques));
            ViewBag.sistema_cheques = Math.Round(Convert.ToDecimal(b.sistema_cheques));
            ViewBag.diferencia_cheques = Math.Round(Convert.ToDecimal(b.diferencia_cheques));

            ViewBag.recibos = Math.Round(Convert.ToDecimal(b.recibos));
            ViewBag.sistema_recibos = Math.Round(Convert.ToDecimal(b.sistema_recibos));
            ViewBag.diferencia_recibos = Math.Round(Convert.ToDecimal(b.diferencia_recibos));

            ViewBag.total_ingresos = Math.Round(Convert.ToDecimal(b.total_ingresos));
            ViewBag.total_egresos = Math.Round(Convert.ToDecimal(b.total_egresos));
            ViewBag.total_sistema = Math.Round(Convert.ToDecimal(b.total_sistema));
            ViewBag.total1 = Math.Round(Convert.ToDecimal(b.total));

            return View();
        }

        [HttpPost]
        public ActionResult CuadreCajaUpdate()
        {
            var total = Request["total"];
            var caja = Request["nombre"];

            var c = context.icb_cuadre_caja.Find(Convert.ToInt32(Request["id"]));

            var nombre = Convert.ToInt32(caja);

            var buscar = context.icb_caja_detalle.Where(d => d.nombre == nombre).Select(d => d.caja).FirstOrDefault();

            c.bodega = Convert.ToInt32(Request["bodega"]);
            c.caja = buscar;
            c.fecha = Convert.ToDateTime(Request["fecha"]);

            c.efectivo = Convert.ToDecimal(Request["efectivo"]);
            c.sistema_efectivo = Convert.ToDecimal(Request["sistema_efectivo"]);
            c.diferencia_efectivo = Convert.ToDecimal(Request["diferencia_efectivo"]);

            c.tarjetas = Convert.ToDecimal(Request["tarjetas"]);
            c.sistema_tarjetas = Convert.ToDecimal(Request["sistema_tarjetas"]);
            c.diferencia_tarjetas = Convert.ToDecimal(Request["diferencia_tarjetas"]);

            c.cheques = Convert.ToDecimal(Request["cheques"]);
            c.sistema_cheques = Convert.ToDecimal(Request["sistema_cheques"]);
            c.diferencia_cheques = Convert.ToDecimal(Request["diferencia_cheques"]);

            c.recibos = Convert.ToDecimal(Request["recibos"]);
            c.sistema_recibos = Convert.ToDecimal(Request["sistema_recibos"]);
            c.diferencia_recibos = Convert.ToDecimal(Request["diferencia_recibos"]);

            c.total_ingresos = Convert.ToDecimal(Request["total_ingresos"]);
            c.total_egresos = Convert.ToDecimal(Request["total_egresos"]);
            c.total_sistema = Convert.ToDecimal(Request["total_sistema"]);
            c.total = Convert.ToDecimal(Request["total"]);

            if (Convert.ToInt32(total) == 0)
            {
                c.estado = 2;
                c.cerrada = true;
            }

            else
            {
                c.estado = 1;
                c.cerrada = false;
            }

            context.Entry(c).State = EntityState.Modified;
            int resultado = context.SaveChanges();

            if (resultado > 0)
            {
                TempData["mensaje"] = "El registro fue exitoso!";
            }
            else
            {
                TempData["mensaje_error"] = "Error, intente de nuevo";
            }

            return RedirectToAction("CuadreCaja", "CentralAtencion");

        }

        public JsonResult busquedaCaja(int? menu)
        {
            string draw = Request.Form.GetValues("draw").FirstOrDefault();
            string start = Request.Form.GetValues("start").FirstOrDefault();
            string length = Request.Form.GetValues("length").FirstOrDefault();
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

            int registrostotales = context.vw_browser_cajas.Count();

            if (pageSize == -1)
            {
                pageSize = registrostotales;
            }

            List<vw_browser_cajas> lista2 = new List<vw_browser_cajas>();


            if (sortColumnDir == "asc")
            {
                lista2 = context.vw_browser_cajas.ToList();
                //.OrderBy(GetColumnName3(sortColumn).Compile()).Skip(skip).Take(pageSize)
            }
            else
            {
                lista2 = context.vw_browser_cajas.ToList();
                //.OrderByDescending(GetColumnName3(sortColumn).Compile()).Skip(skip).Take(pageSize)
            }

            var lista = lista2.Select(x => new {

                x.id,
                //fecha = x.fecha.Value.ToString("yyyy/MM/dd"),
                bodega = x.bodccs_nombre,
                caja = x.numero,
                estado = x.estado,
                nombre=x.nombreCaja,
                responsables = x.listausuarios,
                transacciones = x.listatransacciones,

            }).ToList();

            return Json(
                        new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = lista },
                        JsonRequestBehavior.AllowGet);

        }
        public JsonResult BuscarCajas(int? idbodegas, int? numero, string desde, string hasta)
        {
            string draw = Request.Form.GetValues("draw").FirstOrDefault();
            string start = Request.Form.GetValues("start").FirstOrDefault();
            string length = Request.Form.GetValues("length").FirstOrDefault();
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

            Expression<Func<vw_browser_cuadre_cajas, bool>> predicado = PredicateBuilder.True<vw_browser_cuadre_cajas>();
            Expression<Func<vw_browser_cuadre_cajas, bool>> predicado2 = PredicateBuilder.False<vw_browser_cuadre_cajas>();

            if (idbodegas != null)
            {
                predicado = predicado.And(d => d.bodega == idbodegas);
            }
            if (numero != null)
            {
                predicado = predicado.And(d => d.numerocaja == numero);
            }

            if (!string.IsNullOrWhiteSpace(desde))
            {
                var fecha = DateTime.Now;
                var convertir = DateTime.TryParse(desde, out fecha);
                if (convertir == true)
                {
                    predicado = predicado.And(d => d.fecha >= fecha);
                }
            }
            if (!string.IsNullOrWhiteSpace(hasta))
            {
                var fecha = DateTime.Now;
                var convertir = DateTime.TryParse(hasta, out fecha);
                if (convertir == true)
                {
                    fecha = fecha.AddDays(1);
                    predicado = predicado.And(d => d.fecha <= fecha);
                }
            }

            int registrostotales = context.vw_browser_cuadre_cajas.Where(predicado).Count();

            if (pageSize == -1)
            {
                pageSize = registrostotales;
            }

            List<vw_browser_cuadre_cajas> lista2 = new List<vw_browser_cuadre_cajas>();


            if (sortColumnDir == "asc")
            {
                lista2 = context.vw_browser_cuadre_cajas.Where(predicado).ToList();
                //.OrderBy(GetColumnName3(sortColumn).Compile()).Skip(skip).Take(pageSize)
            }
            else
            {
                lista2 = context.vw_browser_cuadre_cajas.Where(predicado).ToList();
                //.OrderByDescending(GetColumnName3(sortColumn).Compile()).Skip(skip).Take(pageSize)
            }

            var lista = lista2.Select(x => new {

                x.id,
                fecha = x.fecha.Value.ToString("yyyy/MM/dd"),
                bodega = x.bodccs_nombre,
                caja = x.numerocaja,
                nombre = x.nombreCaja,
                estado = x.nombreestado,
                valorTotal = x.total,
                responsables = x.listausuarios,

            }).ToList();

            return Json(
                        new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = lista },
                        JsonRequestBehavior.AllowGet);

        }


        public JsonResult diferenciaArqueo(string valor1, string valor2)
        {
            decimal resultado = 0;
            decimal result = 0;

            if (string.IsNullOrWhiteSpace(valor1))
            {
                valor1 = "0";
            }
            if (string.IsNullOrWhiteSpace(valor2))
            {
                valor2 = "0";
            }

            result = Convert.ToDecimal(valor2) - Convert.ToDecimal(valor1);

            resultado = Math.Round(result);

            return Json(resultado, JsonRequestBehavior.AllowGet);

        }

        public JsonResult total_ingresos(string efectivo,string tarjetas, string cheques, string recibos) {

            decimal resultado = 0;
            decimal result = 0;

            if (string.IsNullOrWhiteSpace(efectivo))
            {
                efectivo = "0";
            }

            if (string.IsNullOrWhiteSpace(tarjetas))
            {
                tarjetas = "0";
            }

            if (string.IsNullOrWhiteSpace(cheques))
            {
                cheques = "0";
            }

            if (string.IsNullOrWhiteSpace(recibos))
            {
                recibos = "0";
            }

            result = Convert.ToDecimal(efectivo) + Convert.ToDecimal(tarjetas) + Convert.ToDecimal(cheques) + Convert.ToDecimal(recibos);

            resultado = Math.Round(result);

            return Json(resultado, JsonRequestBehavior.AllowGet);
        }

        public JsonResult total_egresos(string efectivo, string tarjetas, string cheques, string recibos)
        {

            decimal resultado = 0;
            decimal result = 0;

            if (string.IsNullOrWhiteSpace(efectivo))
            {
                efectivo = "0";
            }

            if (string.IsNullOrWhiteSpace(tarjetas))
            {
                tarjetas = "0";
            }

            if (string.IsNullOrWhiteSpace(cheques))
            {
                cheques = "0";
            }

            if (string.IsNullOrWhiteSpace(recibos))
            {
                recibos = "0";
            }

            result = Convert.ToDecimal(efectivo) + Convert.ToDecimal(tarjetas) + Convert.ToDecimal(cheques) + Convert.ToDecimal(recibos);

            resultado = Math.Round(result);

            return Json(resultado, JsonRequestBehavior.AllowGet);
        }

        public JsonResult total_sistema(string total_ingresos, string total_egresos)
        {
            decimal resultado = 0;
            decimal result = 0;

            if (string.IsNullOrWhiteSpace(total_ingresos))
            {
                total_ingresos = "0";
            }
            if (string.IsNullOrWhiteSpace(total_egresos))
            {
                total_egresos = "0";
            }

            result = Convert.ToDecimal(total_egresos) - Convert.ToDecimal(total_ingresos);

            resultado = Math.Round(result);

            return Json(resultado, JsonRequestBehavior.AllowGet);

        }

        public JsonResult buscarAnticipos(string cedula_cliente) {

            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> predicate = PredicateBuilder.True<vw_movimiento_clientes>();

            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> cliente = PredicateBuilder.False<vw_movimiento_clientes>();

            System.Linq.Expressions.Expression<Func<vw_movimiento_clientes, bool>> id_tipo_doc = PredicateBuilder.False<vw_movimiento_clientes>();

            if (cedula_cliente != "")
            {
                cliente = cliente.Or(x => x.doc_tercero == cedula_cliente);
                predicate = predicate.And(cliente);
            }

            if (id_tipo_doc != null)
            {
                id_tipo_doc = id_tipo_doc.Or(x => x.id_tipo_doc == 16);
                predicate = predicate.And(id_tipo_doc);
            }

            List<vw_movimiento_clientes> buscar = context.vw_movimiento_clientes.Where(predicate).ToList();

            var datos = buscar.Select(x => new
            {
                x.bodega_id,
                id_enc = x.idencabezado,
                bodega = !string.IsNullOrWhiteSpace(x.bodega) ? x.bodega : "",
                pre = !string.IsNullOrWhiteSpace(x.prefijo) ? x.prefijo : "",
                nro_doc = x.nro_documento.ToString() ?? "",
                td = x.id_tipo_doc,
                cliente = !string.IsNullOrWhiteSpace(x.cliente) ? x.cliente : "",
                saldo = x.valor_total - x.valor_aplicado,
                ValorAplicado= x.valor_aplicado,
                valorTotal = x.valor_total,
                tipo_Recibo = x.tipo_recibo,
                fecha = x.fecha.ToString("yyyy/MM/dd"),
            }).ToList();

            var saldoDisponible = datos.Where(d => d.saldo > 0).Select(d => d.id_enc).Count() > 0 ? "Si" : "No";

            var datos2 = datos.Where(d => d.saldo > 0).Select(x => new
            {
                x.bodega_id,
                x.id_enc,
                x.bodega,
                x.pre,
                x.nro_doc,
                x.td,
                x.cliente,
                x.saldo,
                x.valorTotal,
                x.ValorAplicado,
                x.tipo_Recibo,
                x.fecha,
            }).OrderByDescending(x => x.fecha).ToList();


            return Json(datos2, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult LiquidacionCajaOT(int? id, int? menu)
        {

            if (id != null)
            {
                var ot = context.tencabezaorden.Where(d => d.id == id).ToList();
                var resultado = ot.Where(x => x.tp_doc_registros.prefijo == "PRE").Count() > 0 ? "Si" : "No";

                var otaller = context.tencabezaorden.Where(d => d.id == id).FirstOrDefault();
                var operaciones = context.tdetallemanoobraot.Where(d => d.idorden == otaller.id && d.tipotarifa == 4).Count() > 0 ? "Si" : "No";

                //var encab= context.encab_documento.Where(d => d.tipo== 3103 /*|| d.tipo == 3080*/ && d.orden_taller == id).OrderByDescending(d=> d.idencabezado).FirstOrDefault();
                var total = traerTotalesporIdOrden(id);


                var tot = context.vw_tot.Where(d => d.numot == id).ToList();
                var total_tot = tot.Sum(d => Convert.ToDecimal(d.valor_total));

                var buscar = context.detalle_formas_pago_orden.Where(d => d.idorden == id).OrderByDescending(d => d.id_detalle).FirstOrDefault();
                var buscar2 = context.detalle_formas_pago_orden.Where(d => d.idorden == id).Select(d => d.valorRecibido).ToList();

                if (buscar == null)
                {
                    ViewBag.Pendiente = Math.Round(total);
                    ViewBag.Recibido = 0;
                }
                else
                {
                    ViewBag.Pendiente = Math.Round(Convert.ToDecimal(buscar.pendiente));
                    ViewBag.Recibido = Math.Round(Convert.ToDecimal(buscar2.Sum()));

                }

                ViewBag.Permiso = resultado;
                ViewBag.Permiso2 = operaciones;

            }
            if (Session["user_usuarioid"] != null)
            {
                //valido si la orden existe
                tencabezaorden ordentaller = context.tencabezaorden.Where(d => d.id == id).FirstOrDefault();
                //de momento y hasta que se parametrice, la forma de pago CUPO queda quemada
                var listaFP = (from f in context.formas_pago
                               join b in context.bancos
                               on f.idbanco equals b.id into formabando
                               from fb in formabando.DefaultIfEmpty()
                               where f.id!=7
                               orderby fb.Descripcion
                               select new
                               {
                                   f.id,
                                   nombre = fb.Descripcion + " " + f.formapago + " (" + fb.numero_cuenta + ")"
                               }).ToList();

                List<SelectListItem> list = new List<SelectListItem>();
                foreach (var item in listaFP)
                {
                    list.Add(new SelectListItem
                    {
                        Text = item.nombre,
                        Value = item.id.ToString()
                    });
                }
                ViewBag.FormaPago = list;


                var data = (from vh in context.icb_vehiculo
                            join t in context.tencabezaorden
                            on vh.plan_mayor equals t.placa
                            where vh.plac_vh == ordentaller.placa || vh.plan_mayor == ordentaller.placa && t.codigoentrada == ordentaller.codigoentrada
                            select new
                            {
                                vh.garantia
                            }).FirstOrDefault();

                var garantia = data.garantia;

                ViewBag.Garantia = garantia;


                var listaTipoTarifa = (from e in context.rtipocliente
                                       select new
                                       {
                                           id = e.id,
                                           nombre = e.descripcion
                                       }).ToList();

                List<SelectListItem> lista = new List<SelectListItem>();
                foreach (var item in listaTipoTarifa)
                {
                    lista.Add(new SelectListItem
                    {
                        Text = item.nombre,
                        Value = item.id.ToString()
                    });
                }
                ViewBag.tarifa = lista;

                if (ordentaller != null)
                {
                    //parametro de sistema liquidacion
                    icb_sysparameter liqui = context.icb_sysparameter.Where(d => d.syspar_cod == "P111").FirstOrDefault();
                    int liquidada = liqui != null ? Convert.ToInt32(liqui.syspar_value) : 8;

                        string formapago = "NO TIENE, DEBE ACTUALIZAR DATOS DEL CLIENTE";
                        tercero_cliente tercero_cli = context.tercero_cliente.Where(d => d.tercero_id == ordentaller.tercero)
                            .FirstOrDefault();
                        if (tercero_cli != null)
                        {
                            formapago = (from c in context.tercero_cliente
                                         join f in context.fpago_tercero
                                             on c.cod_pago_id equals f.fpago_id
                                         where c.tercero_id == ordentaller.tercero
                                         select
                                             f.fpago_nombre
                                ).FirstOrDefault();
                        }
                        //llenado de objeto formulario de orden de taller
                        //datos del formulario
                        formularioliquidacionOT formuorden = new formularioliquidacionOT
                        {
                            nombre = !string.IsNullOrWhiteSpace(ordentaller.icb_terceros.razon_social)
                                ? ordentaller.icb_terceros.razon_social
                                : ordentaller.icb_terceros.prinom_tercero +
                                  (string.IsNullOrWhiteSpace(ordentaller.icb_terceros.segnom_tercero)
                                      ? " " + ordentaller.icb_terceros.segnom_tercero
                                      : "") + " " + ordentaller.icb_terceros.apellido_tercero +
                                  (string.IsNullOrWhiteSpace(ordentaller.icb_terceros.segapellido_tercero)
                                      ? " " + ordentaller.icb_terceros.segapellido_tercero
                                      : ""),
                            documento = ordentaller.icb_terceros.doc_tercero,
                            telefono = ordentaller.icb_terceros.telf_tercero,
                            celular = !string.IsNullOrWhiteSpace(ordentaller.icb_terceros.celular_tercero)
                                ? ordentaller.icb_terceros.celular_tercero
                                : "",
                            correo = ordentaller.icb_terceros.email_tercero,
                            direccion = ordentaller.icb_terceros.terceros_direcciones.Count() > 0
                                ? ordentaller.icb_terceros.terceros_direcciones.Select(e => e.direccion)
                                    .FirstOrDefault()
                                : "",
                            //datos del vehiculo
                            placa = !string.IsNullOrWhiteSpace(ordentaller.icb_vehiculo.plac_vh)
                                ? ordentaller.icb_vehiculo.plac_vh
                                : ordentaller.icb_vehiculo.plan_mayor,
                            marca = ordentaller.icb_vehiculo.marca_vehiculo.marcvh_nombre,
                            modelo = ordentaller.icb_vehiculo.modelo_vehiculo.modvh_nombre,
                            asesor = ordentaller.users.user_nombre + " " + ordentaller.users.user_apellido,
                            forma_pago = formapago,
                            garantia = ordentaller.icb_vehiculo.garantia,
                            //campos generales de la OT
                            codigoentrada = ordentaller.codigoentrada,
                            idorden = ordentaller.id,
                            razon_ingreso = ordentaller.razoningreso,
                            razon_ingreso2 = ordentaller.razoningreso2,

                            tecnico = ordentaller.idtecnico,
                            tipoDocumento= ordentaller.idtipodoc,
                            cartera= ordentaller.centrocosto,
                            id_cliente = ordentaller.tercero,
                            bodega = ordentaller.bodega,
                            kilometraje = ordentaller.kilometraje.ToString("N0", new CultureInfo("is-IS")),
                            serie = ordentaller.icb_vehiculo.vin,
                            motor = ordentaller.icb_vehiculo.nummot_vh,
                            color = ordentaller.icb_vehiculo.color_vehiculo.colvh_nombre,
                            fecha_fin_garantia = ordentaller.icb_vehiculo.fecha_fin_garantia != null
                                ? ordentaller.icb_vehiculo.fecha_fin_garantia.Value.ToString("yyyy/MM/dd",
                                    new CultureInfo("en-US"))
                                : "",
                            fecha_venta = ordentaller.icb_vehiculo.fecha_venta != null
                                ? ordentaller.icb_vehiculo.fecha_venta.Value.ToString("yyyy/MM/dd",
                                    new CultureInfo("en-US"))
                                : "",
                            aseguradora = ordentaller.aseguradora,
                            garantia_falla = ordentaller.garantia_falla,
                            garantia_causa = ordentaller.garantia_causa,
                            garantia_solucion = ordentaller.garantia_solucion,
                            poliza = ordentaller.poliza,
                            deducible = ordentaller.deducible != null
                                ? ordentaller.deducible.Value.ToString("N0", new CultureInfo("is-IS"))
                                : "",
                            siniestro = ordentaller.siniestro,
                            minimo = ordentaller.minimo != null
                                ? ordentaller.minimo.Value.ToString("N0", new CultureInfo("is-IS"))
                                : "",
                            fecha_soat = ordentaller.fecha_soat != null
                                ? ordentaller.fecha_soat.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                                : "",
                            numero_soat = !string.IsNullOrWhiteSpace(ordentaller.numero_soat)
                                ? ordentaller.numero_soat
                                : ""

                        };
                        listasliquidacion(formuorden);
                        BuscarFavoritos(menu);
                        return View(formuorden);
                }
                return View();
            }

            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        public ActionResult LiquidacionCajaOT(formularioliquidacionOT modelo, int? menu)
        {
            int id_encabezado = 0;
            long id_numero = 0;

            if (Session["user_usuarioid"] != null)
            {
                /*int cajas = 0;
                int guardar = 0;*/
                //valido si el modelo es válido
                if (ModelState.IsValid)
                {
                    //parametro orden de taller facturada
                    icb_sysparameter param1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P111").FirstOrDefault();
                    int estadofacturada = param1 != null ? Convert.ToInt32(param1.syspar_value) : 8;

                    //parametro de sistema accesorios
                    icb_sysparameter param2 = context.icb_sysparameter.Where(d => d.syspar_cod == "P116").FirstOrDefault();
                    int accesorios = param2 != null ? Convert.ToInt32(param2.syspar_value) : 6;

                    var tecnicos = context.ttecnicos.Where(d => d.estado && d.users.user_estado).Select(d =>
                    new { id = d.users.user_id, nombre = d.users.user_nombre + " " + d.users.user_apellido }).ToList();
                    ViewBag.tecnico = new SelectList(tecnicos, "id", "nombre", modelo.tecnico);

                    var razones = context.trazonesingreso.Where(d => d.estado).Select(d => new { d.id, nombre = d.razoz_ingreso }).ToList();

                    ViewBag.razon_ingreso = new SelectList(razones, "id", "nombre", modelo.razon_ingreso);
                    ViewBag.razon_ingreso2 = new SelectList(razones, "id", "nombre", modelo.razon_ingreso2);

                    icb_sysparameter parametro1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P115").FirstOrDefault();
                    int tipodocumento = parametro1 != null ? Convert.ToInt32(parametro1.syspar_value) : 28;

                    icb_sysparameter parametro11 = context.icb_sysparameter.Where(d => d.syspar_cod == "P129").FirstOrDefault();
                    int swiche = parametro11 != null ? Convert.ToInt32(parametro11.syspar_value) : 3;
                    //parametro orden de taller prefactura
                    icb_sysparameter parametro2 = context.icb_sysparameter.Where(d => d.syspar_cod == "P150").FirstOrDefault();
                    int estadoprefacturada = parametro2 != null ? Convert.ToInt32(parametro2.syspar_value) : 25;

                    var documentos = context.tp_doc_registros.Where(d => d.tpdoc_estado && d.tipo == tipodocumento && d.sw==swiche)
                    .Select(d => new { id = d.tpdoc_id, nombre = d.tpdoc_nombre }).ToList();
                    ViewBag.tipoDocumento = new SelectList(documentos, "id", "nombre", modelo.tipoDocumento);

                    ViewBag.operacionesTempario = new SelectList(context.ttempario, "codigo", "operacion");

                    var buscarCentro = context.centro_costo.Where(x => x.bodega == modelo.bodega).Select(x => new
                    {
                        id = x.centcst_id,
                        nombre = x.pre_centcst + " - " + x.Tipos_Cartera.descripcion
                    }).ToList();
                    ViewBag.cartera = new SelectList(buscarCentro, "id", "nombre", modelo.cartera);

                    ViewBag.aseguradora = new SelectList(context.icb_aseguradoras, "aseg_id", "nombre", modelo.aseguradora);


                    var listaFP = (from f in context.formas_pago
                                   join b in context.bancos
                                   on f.idbanco equals b.id into formabando
                                   from fb in formabando.DefaultIfEmpty()
                                   orderby fb.Descripcion
                                   select new
                                   {
                                       f.id,
                                       nombre = fb.Descripcion + " " + f.formapago + " (" + fb.numero_cuenta + ")"
                                   }).ToList();
                    //si la forma de pago seleccionada en el post es CONTADO
                    if (modelo.forma_pago_id == 1)
                    {
                        listaFP = listaFP.Where(d => d.id != 7).ToList();
                    }
                    List<SelectListItem> list = new List<SelectListItem>();
                    foreach (var item in listaFP)
                    {
                        list.Add(new SelectListItem
                        {
                            Text = item.nombre,
                            Value = item.id.ToString()
                        });
                    }
                    ViewBag.FormaPago = list;


                    using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                    {
                        try
                        {
                            bodega_concesionario bodegaconce = context.bodega_concesionario.Where(d => d.id == modelo.bodega)
                                .FirstOrDefault();
                            //validaciones preliminares
                            int funciono = 0;
                            decimal totalCreditos = 0;
                            decimal totalDebitos = 0;
                            decimal costoPromedioTotal = 0;
                            //busco el perfil contable del tipo de documento y bodega

                            perfil_contable_documento perfilcontable = context.perfil_contable_documento.Where(d =>
                                d.perfil_contable_bodega.Where(e => e.idbodega == modelo.bodega).Count() > 0 &&
                                d.tipo == modelo.tipoDocumento).FirstOrDefault();
                            if (perfilcontable != null)
                            {
                                var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                                                  join nombreParametro in context.paramcontablenombres
                                                                      on perfil.id_nombre_parametro equals nombreParametro.id
                                                                  join cuenta in context.cuenta_puc
                                                                      on perfil.cuenta equals cuenta.cntpuc_id
                                                                  where perfil.id_perfil == perfilcontable.id
                                                                  select new
                                                                  {
                                                                      perfil.id,
                                                                      perfil.id_nombre_parametro,
                                                                      perfil.cuenta,
                                                                      perfil.centro,
                                                                      perfil.id_perfil,
                                                                      nombreParametro.descripcion_parametro,
                                                                      cuenta.cntpuc_numero
                                                                  }).ToList();

                                int secuencia = 1;
                                //informacion de la orden de taller
                                tencabezaorden ordentaller = context.tencabezaorden.Where(d => d.id == modelo.idorden)
                                    .FirstOrDefault();

                                int exhibicion = 0;
                                if (ordentaller.razoningreso == accesorios)
                                {
                                    //busco si el vehiculo es de exhibicion
                                    vpedido existepedido = context.vpedido.Where(d => d.planmayor == ordentaller.placa)
                                        .FirstOrDefault();
                                    if (existepedido == null)
                                    {
                                        exhibicion = 1;
                                    }
                                }



                                //centro de costo cero
                                List<cuentas_valores> ids_cuentas_valores = new List<cuentas_valores>();
                                centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                                int idCentroCero = centroValorCero != null
                                    ? Convert.ToInt32(centroValorCero.centcst_id)
                                    : 0;

                                List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();
                                List<ElementosFacturacion> listaelementos = new List<ElementosFacturacion>();
                                List<tsolicitudrepuestosot> cantidadreferencias = context.tsolicitudrepuestosot
                                    .Where(d => d.idorden == modelo.idorden && d.recibido).ToList();

                                //sumo el costo total de los repuestos si el vehiculo no es de exhibicion. Si lo es, ni los tomo en cuenta.
                                if (exhibicion == 1)
                                {
                                    cantidadreferencias = cantidadreferencias.Take(0).ToList();
                                }

                                int costoLineas = cantidadreferencias.Count();
                                for (int i = 0; i < costoLineas; i++)
                                {
                                    string referencia = !string.IsNullOrWhiteSpace(cantidadreferencias[i].reemplazo)
                                        ? cantidadreferencias[i].reemplazo
                                        : cantidadreferencias[i].tdetallerepuestosot.idrepuesto;
                                    decimal costoReferencia = cantidadreferencias[i].tdetallerepuestosot.valorunitario;
                                    costoPromedioTotal +=
                                        Convert.ToDecimal(costoReferencia, miCultura) *
                                        cantidadreferencias[i].tdetallerepuestosot.cantidad;

                                    listaelementos.Add(new ElementosFacturacion
                                    {
                                        tipo = "R",
                                        cantidad = cantidadreferencias[i].tdetallerepuestosot.cantidad,
                                        centro_costo = cantidadreferencias[i].tdetallerepuestosot.idcentro ?? 0,
                                        codigo = cantidadreferencias[i].tdetallerepuestosot.idrepuesto,
                                        porcentaje_descuento = cantidadreferencias[i].tdetallerepuestosot.pordescto,
                                        porcentaje_iva = cantidadreferencias[i].tdetallerepuestosot.poriva,
                                        tipo_tarifa = cantidadreferencias[i].tdetallerepuestosot.tipotarifa ?? 1,
                                        valor_descuento = calculardescuentore(
                                            cantidadreferencias[i].tdetallerepuestosot.valorunitario,
                                            cantidadreferencias[i].tdetallerepuestosot.cantidad,
                                            cantidadreferencias[i].tdetallerepuestosot.pordescto),
                                        valor_iva = calcularivare(
                                            cantidadreferencias[i].tdetallerepuestosot.valorunitario,
                                            cantidadreferencias[i].tdetallerepuestosot.cantidad,
                                            cantidadreferencias[i].tdetallerepuestosot.pordescto,
                                            cantidadreferencias[i].tdetallerepuestosot.poriva),
                                        valor_unitario =
                                            cantidadreferencias[i].tdetallerepuestosot.valorunitario
                                    });
                                }

                                var mano_obra = context.ttarifastaller.Where(d => d.bodega == modelo.bodega && d.tipotarifa == 1).FirstOrDefault();

                                //sumo el costo total de las operaciones
                                List<tdetallemanoobraot> cantidadoperaciones = context.tdetallemanoobraot
                                    .Where(d => d.idorden == modelo.idorden && d.estado == "1" && d.tipotarifa != 2).ToList();
                                int costooperaciones = cantidadoperaciones.Count();
                                //OPERACIONES
                                for (int i = 0; i < costooperaciones; i++)
                                    {
                                    string operacion = cantidadoperaciones[i].idtempario;
                                    decimal costooperacion = cantidadoperaciones[i].valorunitario;
                                    costoPromedioTotal += costooperacion;

                                    listaelementos.Add(new ElementosFacturacion
                                        {
                                        tipo = "M",
                                        cantidad = 1,
                                        tiempo = cantidadoperaciones[i].tiempo ?? 0,
                                        centro_costo = cantidadoperaciones[i].idcentro ?? 0,
                                        codigo = cantidadoperaciones[i].idtempario,
                                        porcentaje_descuento = cantidadoperaciones[i].pordescuento ?? 0,
                                        porcentaje_iva = !string.IsNullOrWhiteSpace(mano_obra.iva) ? Convert.ToDecimal(mano_obra.iva, miCultura) : 0,
                                        tipo_tarifa = cantidadoperaciones[i].tipotarifa ?? 1,
                                        valor_descuento = calculardescuento(cantidadoperaciones[i].valorunitario,
                                            cantidadoperaciones[i].pordescuento),
                                        valor_iva = calculariva(cantidadoperaciones[i].valorunitario,
                                            cantidadoperaciones[i].pordescuento, Convert.ToDecimal(mano_obra.iva, miCultura)),
                                        valor_unitario = cantidadoperaciones[i].valorunitario
                                        });
                                    }

                                string ot = modelo.idorden.ToString();
                                var cantidadtot = context.vw_tot.Where(r => r.numot2 == ot).Select(x => new
                                    {
                                    x.fecha2,
                                    x.numerofactura,
                                    x.proveedor,
                                    x.valor_total,
                                    x.valor_total2,
                                    x.id2,
                                    x.numot,
                                    x.codigoentrada
                                    }).ToList();


                                int costotot = cantidadtot.Count();
                                //TOT
                                for (int i = 0; i < costotot; i++)
                                    {
                                    string operacion = cantidadoperaciones[i].idtempario;
                                    decimal costooperacion = Convert.ToDecimal(cantidadtot[i].valor_total, miCultura);
                                    costoPromedioTotal += costooperacion;

                                    listaelementos.Add(new ElementosFacturacion
                                        {
                                        tipo = "TOT",
                                        cantidad = 1,
                                        tipo_tarifa = 1,
                                        centro_costo = 0,
                                        porcentaje_descuento = 0,
                                        porcentaje_iva = 0,
                                        valor_descuento = 0,
                                        valor_iva = 0,
                                        codigo = cantidadtot[i].numerofactura.ToString(),
                                        valor_unitario = Convert.ToDecimal(cantidadtot[i].valor_total, miCultura)
                                        });
                                    }

                                var listainsumos = context.tinsumooperaciones.Where(x => x.numot == modelo.idorden && x.valort_horas > 0).Select(e => new
                                    {
                                    e.id,
                                    e.codigo_insumo,
                                    Descripcion_insumo = e.Tinsumo.descripcion,
                                    tarifa = e.tipotarifa,
                                    porcentaje = e.porcentaje_total_,
                                    valor_tarifa = e.valortarifa,
                                    e.valort_horas
                                    }).ToList();

                                int tinsumos = listainsumos.Count();
                                //INSUMOS
                                for (int i = 0; i < tinsumos; i++)
                                    {

                                    decimal costooperacion = Convert.ToDecimal(listainsumos[i].valort_horas, miCultura);
                                    costoPromedioTotal += costooperacion;

                                    ElementosFacturacion elemento = new ElementosFacturacion();
                                    elemento.tipo = "I";
                                    elemento.cantidad = 1;
                                    elemento.tiempo = Convert.ToDecimal(listainsumos[i].porcentaje, miCultura);
                                    elemento.codigo = listainsumos[i].codigo_insumo.ToString();
                                    elemento.tipo_tarifa = Convert.ToInt32(listainsumos[i].tarifa);
                                    elemento.centro_costo = 0;
                                    elemento.porcentaje_descuento = 0;
                                    elemento.porcentaje_iva = 0;
                                    elemento.valor_descuento = 0;
                                    elemento.valor_iva = 0;
                                    elemento.valor_unitario = Convert.ToDecimal(listainsumos[i].valort_horas, miCultura);


                                    listaelementos.Add(elemento);
                                    }

                                var listacombo = context.tot_combo.Where(x => x.numot == modelo.idorden && x.estado == true).Select(c => new
                                    {
                                    c.id,
                                    codigo = c.ttempario.codigo,
                                    operacion = c.ttempario.operacion,
                                    c.totalHorasCliente,
                                    c.totalhorasoperario,
                                    ttipostarifa = context.rtipocliente.Where(d => d.estado).Select(x => new { x.id, x.descripcion }).ToList(),
                                    c.tipotarifa,
                                    c.idtempario,
                                    valortotal = c.valortotal != null ? c.valortotal : 0
                                    }).ToList();
                                int tcombo = listacombo.Count();
                                //COMBOS 
                                for (int i = 0; i < tcombo; i++)
                                    {

                                    decimal costooperacion = Convert.ToDecimal(listacombo[i].valortotal, miCultura);
                                    costoPromedioTotal += costooperacion;

                                    listaelementos.Add(new ElementosFacturacion
                                        {
                                        tipo = "CO",
                                        cantidad = 1,
                                        tiempo = listacombo[i].totalHorasCliente ?? 0,
                                        codigo = listacombo[i].idtempario,
                                        tipo_tarifa = listacombo[i].tipotarifa ?? 1,
                                        centro_costo = 0,
                                        porcentaje_descuento = 0,
                                        porcentaje_iva = 0,
                                        valor_descuento = 0,
                                        valor_iva = 0,
                                        valor_unitario = Convert.ToDecimal(listacombo[i].valortotal, miCultura)
                                        });
                                    }



                                //seleccionamos los que son diferentes de precio interno o garantias
                                listaelementos = listaelementos.Where(x => x.tipo_tarifa == 1 || x.tipo_tarifa == 3).ToList();
                                NotasContablesOT oT = new NotasContablesOT();

                                int? bodega = modelo.bodega;


                                int lista = cantidadreferencias.Where(x => x.tdetallerepuestosot.tipotarifa == 1 || x.tdetallerepuestosot.tipotarifa == 3).Count();
                                int lista2 = cantidadoperaciones.Where(x => x.tipotarifa == 1 || x.tipotarifa == 3).Count();
                                int lista7 = cantidadtot.Count();
                                int list8 = listainsumos.Where(x => x.tarifa == 1 || x.tarifa == 3).Count();
                                int lista9 = listacombo.Where(x => x.tipotarifa == 1 || x.tipotarifa == 3).Count();

                                int lista3 = cantidadreferencias.Where(x => x.tdetallerepuestosot.tipotarifa == 2).Count();
                                int lista4 = cantidadoperaciones.Where(x => x.tipotarifa == 2).Count();

                                int lista5 = cantidadreferencias.Where(x => x.tdetallerepuestosot.tipotarifa == 4).Count();
                                int lista6 = cantidadoperaciones.Where(x => x.tipotarifa == 4).Count();

                                if (lista > 0 || lista2 > 0 || lista3 > 0 || lista > 4 || lista > 5 || lista > 6)
                                {
                                    if (lista > 0 || lista2 > 0)
                                    {
                                        int datos = Convert.ToInt32(lista);
                                        decimal  costodocliq = 0;


                                        decimal costoTotal = 0;
                                        //    Convert.ToDecimal(Request["totaltotal"], miCultura); //costo con retenciones y fletes

                                        tercero_cliente tercero_cliente = context.tercero_cliente
                                 .Where(d => d.icb_terceros.tercero_id == modelo.id_cliente).FirstOrDefault();
                                        decimal desto_rep = 0, desto_mo = 0;
                                        
                                            desto_rep = tercero_cliente!=null?Convert.ToDecimal(tercero_cliente.dscto_rep, miCultura):0;                                      
                                            desto_mo = tercero_cliente!=null?Convert.ToDecimal(tercero_cliente.dscto_mo, miCultura):0;
                                        
                                        foreach (var item in listaelementos)
                                            {
                                            //decimal valorcantidad = 0,valdesc =0 , valiva= 0, iva=0,  desc = 0, valoconivauni = 0, descliente = 0, valor = 0;
                                            decimal valorcantidad = 0, valdesc = 0, valiva = 0, desc = 0, valoconivauni = 0, descliente = 0, valor = 0;
                                            if (item.tipo == "M")
                                                {
                                                valorcantidad = item.valor_unitario;
                                                valdesc = (item.porcentaje_descuento / 100) * valorcantidad;
                                                desc = valorcantidad - valdesc;
                                                valiva = (item.porcentaje_iva / 100) * desc;
                                                valoconivauni = desc + valiva;
                                                costoTotal = costoTotal + valoconivauni;

                                                if (desto_mo != 0)
                                                    {
                                                    descliente = (desto_mo / 100) * desc;
                                                    desc = desc - descliente;
                                                    }
                                                valor = desc + item.valor_iva;
                                                costodocliq = costodocliq + valor;
                                                }
                                            else if (item.tipo == "R")
                                                {
                                                valorcantidad = item.cantidad * item.valor_unitario;
                                                valdesc = (item.porcentaje_descuento / 100) * valorcantidad;
                                                desc = valorcantidad - valdesc;
                                                valiva = (item.porcentaje_iva / 100) * desc;                                
                                                valoconivauni = desc + valiva;
                                                costoTotal = costoTotal + valoconivauni;
                                                if (desto_rep != 0)
                                                    {
                                                    descliente = (desto_rep / 100) * desc;
                                                    desc = desc - descliente;
                                                    }

                                                valor = desc + item.valor_iva;
                                                costodocliq = costodocliq + valor;
                                                }

                                            if (item.tipo == "TOT")
                                                {
                                                valorcantidad = item.valor_unitario;
                                                costoTotal = costoTotal + valorcantidad;
                                                costodocliq = costodocliq + valorcantidad;
                                                }
                                            if (item.tipo == "I")
                                                {
                                                valorcantidad = item.valor_unitario;
                                                costoTotal = costoTotal + valorcantidad;
                                                costodocliq = costodocliq + valorcantidad;
                                                }
                                            if (item.tipo == "CO")
                                                {
                                                valorcantidad = item.valor_unitario;
                                                costoTotal = costoTotal + valorcantidad;
                                                costodocliq = costodocliq + valorcantidad;
                                                }


                                            }

                                        decimal ivaEncabezado = Convert.ToDecimal(Request["totaliva"], miCultura); //valor total del iva
                                        decimal descuentoEncabezado =
                                            Convert.ToDecimal(Request["totaldescuentos"], miCultura); //valor total del descuento
                                        decimal costoEncabezado =
                                            Convert.ToDecimal(Request["subtotal"], miCultura); //valor antes de impuestos
                                                                                               //esto es verdad? 
                                        decimal valor_totalenca = costoEncabezado - descuentoEncabezado;
                                        //comentado la forma de pago por defecto del cliente, se debe dejar abierta a que se  seleccione en el formulario
                                        //int? condicion = tercero_cliente.cod_pago_id;
                                        int? condicion = modelo.forma_pago_id;

                                        //consecutivo
                                        grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                                            x.documento_id == modelo.tipoDocumento && x.bodega_id == bodega);
                                        if (grupo != null)
                                        {
                                            DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                                            long consecutivo = doc.BuscarConsecutivo(grupo.grupo);

                                            //Encabezado documento

                                            #region encabezado

                                            encab_documento encabezado = new encab_documento
                                            {
                                                //tipo = modelo.tipoDocumento.Value,
                                                tipo = Convert.ToInt32(Request["tipoDocumento"]),
                                                prefijo = Convert.ToInt32(Request["tipoDocumento"]),
                                                numero = consecutivo,
                                                nit = modelo.id_cliente.Value,
                                                fecha = DateTime.Now,
                                                fpago_id = condicion,
                                                centro_doc = Convert.ToInt32(modelo.cartera),
                                                estado_factura = 4,
                                            };
                                            int dias = context.fpago_tercero.Find(condicion).dvencimiento ?? 0;
                                            DateTime vencimiento = DateTime.Now.AddDays(dias);
                                            encabezado.vencimiento = vencimiento;
                                            encabezado.valor_total = costodocliq;
                                            encabezado.iva = ivaEncabezado;
                                            encabezado.orden_taller = modelo.idorden;
                                            // Validacion para reteIVA, reteICA y retencion dependiendo del proveedor seleccionado

                                            //de acuerdo a los medios de pago asigno valor_aplicado y valor_cupo


                                            #region calculo de retenciones

                                            tp_doc_registros buscarTipoDocRegistro =
                                                context.tp_doc_registros.FirstOrDefault(x =>
                                                    x.tpdoc_id == modelo.tipoDocumento.Value);
                                            tercero_cliente buscarCliente =
                                                context.tercero_cliente.FirstOrDefault(x =>
                                                    x.tercero_id == modelo.id_cliente);
                                            int? regimen_cliente = buscarCliente != null ? buscarCliente.tpregimen_id : 0;
                                            perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                                                x.bodega == bodega && x.sw == buscarTipoDocRegistro.sw &&
                                                x.tipo_regimenid == regimen_cliente);

                                            decimal retenciones = 0;

                                            if (buscarPerfilTributario != null)
                                            {
                                                if (buscarPerfilTributario.retfuente == "A" &&
                                                    valor_totalenca >= buscarTipoDocRegistro.baseretencion)
                                                {
                                                    encabezado.porcen_retencion = buscarTipoDocRegistro.retencion;
                                                    encabezado.retencion =
                                                        Math.Round(valor_totalenca *
                                                                   Convert.ToDecimal(
                                                                       buscarTipoDocRegistro.retencion / 100, miCultura));
                                                    retenciones += encabezado.retencion;
                                                }

                                                if (buscarPerfilTributario.retiva == "A" &&
                                                    ivaEncabezado >= buscarTipoDocRegistro.baseiva)
                                                {
                                                    encabezado.porcen_reteiva = buscarTipoDocRegistro.retiva;
                                                    encabezado.retencion_iva =
                                                        Math.Round(encabezado.iva *
                                                                   Convert.ToDecimal(buscarTipoDocRegistro.retiva / 100, miCultura));
                                                    retenciones += encabezado.retencion_iva;
                                                }

                                                if (buscarPerfilTributario.autorretencion == "A")
                                                {
                                                    decimal tercero_acteco = buscarCliente.acteco_tercero.autorretencion;
                                                    encabezado.porcen_autorretencion = (float)tercero_acteco;
                                                    encabezado.retencion_causada =
                                                        Math.Round(
                                                            valor_totalenca * Convert.ToDecimal(tercero_acteco / 100, miCultura));
                                                    retenciones += encabezado.retencion_causada;
                                                }

                                                if (buscarPerfilTributario.retica == "A" &&
                                                    valor_totalenca >= buscarTipoDocRegistro.baseica)
                                                {
                                                    terceros_bod_ica bodega_acteco = context.terceros_bod_ica.FirstOrDefault(x =>
                                                        x.idcodica == buscarCliente.actividadEconomica_id &&
                                                        x.bodega == bodega);
                                                    decimal tercero_acteco = buscarCliente.acteco_tercero.tarifa;
                                                    if (bodega_acteco != null)
                                                    {
                                                        encabezado.porcen_retica = (float)bodega_acteco.porcentaje;
                                                        encabezado.retencion_ica =
                                                            Math.Round(valor_totalenca *
                                                                       Convert.ToDecimal(bodega_acteco.porcentaje / 1000, miCultura));
                                                        retenciones += encabezado.retencion_ica;
                                                    }

                                                    if (tercero_acteco != 0)
                                                    {
                                                        encabezado.porcen_retica =
                                                            (float)buscarCliente.acteco_tercero.tarifa;
                                                        encabezado.retencion_ica =
                                                            Math.Round(valor_totalenca *
                                                                       Convert.ToDecimal(
                                                                           buscarCliente.acteco_tercero.tarifa / 1000, miCultura));
                                                        retenciones += encabezado.retencion_ica;
                                                    }
                                                    else
                                                    {
                                                        encabezado.porcen_retica = buscarTipoDocRegistro.retica;
                                                        encabezado.retencion_ica =
                                                            Math.Round(valor_totalenca *
                                                                       Convert.ToDecimal(
                                                                           buscarTipoDocRegistro.retica / 1000, miCultura));
                                                        retenciones += encabezado.retencion_ica;
                                                    }
                                                }
                                            }

                                            #endregion

                                            encabezado.costo = costoPromedioTotal;
                                            //para efectos de la facturacion, quien es el vendedor?
                                            encabezado.vendedor = Convert.ToInt32(Request["vendedor"]);
                                            encabezado.perfilcontable = perfilcontable.id;
                                            encabezado.bodega = modelo.bodega.Value;
                                            monedas moneda2 = context.monedas.Where(d => d.estado).FirstOrDefault();
                                            int moneda = 1;
                                            if (moneda2 != null)
                                            {
                                                moneda = moneda2.moneda;
                                            }

                                            encabezado.moneda = moneda;
                                            encabezado.valor_mercancia = valor_totalenca;
                                            encabezado.fec_creacion = DateTime.Now;
                                            encabezado.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                            encabezado.estado = true;
                                            encabezado.prefactura = false;
                                            encabezado.orden_taller = modelo.idorden;
                                            context.encab_documento.Add(encabezado);
                                            context.SaveChanges();

                                            #endregion

                                            id_encabezado = encabezado.idencabezado;
                                            id_numero = encabezado.numero;

                                            encab_documento eg = context.encab_documento.FirstOrDefault(x =>
                                                x.idencabezado == id_encabezado);

                                            //Mov Contable

                                            #region movimientos contables

                                            //buscamos en perfil cuenta documento, por medio del perfil seleccionado
                                            foreach (var parametro in parametrosCuentasVerificar)
                                            {
                                                string descripcionParametro = context.paramcontablenombres
                                                    .FirstOrDefault(x => x.id == parametro.id_nombre_parametro)
                                                    .descripcion_parametro;
                                                cuenta_puc buscarCuenta =
                                                    context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);

                                                if (buscarCuenta != null)
                                                {
                                                    if (parametro.id_nombre_parametro == 10 &&
                                                        Convert.ToDecimal(valor_totalenca, miCultura) != 0
                                                        || parametro.id_nombre_parametro == 3 &&
                                                        Convert.ToDecimal(eg.retencion, miCultura) != 0
                                                        || parametro.id_nombre_parametro == 4 &&
                                                        Convert.ToDecimal(eg.retencion_iva, miCultura) != 0
                                                        || parametro.id_nombre_parametro == 5 &&
                                                        Convert.ToDecimal(eg.retencion_ica, miCultura) != 0
                                                        || parametro.id_nombre_parametro == 6 &&
                                                        Convert.ToDecimal(eg.fletes, miCultura) != 0
                                                        || parametro.id_nombre_parametro == 14 &&
                                                        Convert.ToDecimal(eg.iva_fletes, miCultura) != 0
                                                        || parametro.id_nombre_parametro == 17 &&
                                                        Convert.ToDecimal(eg.retencion_causada, miCultura) != 0
                                                        || parametro.id_nombre_parametro == 18 &&
                                                        Convert.ToDecimal(eg.retencion_causada, miCultura) != 0)
                                                    {
                                                        mov_contable movNuevo = new mov_contable
                                                        {
                                                            id_encab = eg.idencabezado,
                                                            seq = secuencia,
                                                            idparametronombre = parametro.id_nombre_parametro,
                                                            cuenta = parametro.cuenta,
                                                            centro = parametro.centro,
                                                            fec = DateTime.Now,
                                                            fec_creacion = DateTime.Now,
                                                            userid_creacion =
                                                            Convert.ToInt32(Session["user_usuarioid"]),
                                                            documento = Convert.ToString(modelo.idorden),
                                                            detalle =
                                                            "Facturacion de orden de talle N° " +
                                                            ordentaller.codigoentrada + " con consecutivo " + eg.numero,
                                                            estado = true
                                                        };

                                                        cuenta_puc info = context.cuenta_puc
                                                            .Where(a => a.cntpuc_id == parametro.cuenta).FirstOrDefault();

                                                        if (info.tercero)
                                                        {
                                                            movNuevo.nit = ordentaller.tercero;
                                                        }
                                                        else
                                                        {
                                                            icb_terceros tercero = context.icb_terceros
                                                                .Where(t => t.doc_tercero == "0").FirstOrDefault();
                                                            movNuevo.nit = tercero.tercero_id;
                                                        }

                                                        // las siguientes validaciones se hacen dependiendo de la cuenta que esta moviendo la compra manual, para guardar la informacion acorde

                                                        #region Cuentas X Cobrar

                                                        if (parametro.id_nombre_parametro == 10)
                                                        {
                                                            /*if (info.aplicaniff==true)
                                                            {

                                                            }*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                //no estoy seguro de que aquí tenga que ir el id de la orden. Usualmente este campo, si se llena, está para recibir el id del pedido
                                                                movNuevo.documento = Convert.ToString(modelo.idorden);
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = Convert.ToDecimal(costoTotal, miCultura);

                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = Convert.ToDecimal(costoTotal, miCultura);
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = Convert.ToDecimal(costoTotal, miCultura);
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = Convert.ToDecimal(costoTotal, miCultura);
                                                            }
                                                        }

                                                        #endregion

                                                        #region Retencion

                                                        if (parametro.id_nombre_parametro == 3)
                                                        {
                                                            /*if (info.aplicaniff==true)
                                                            {

                                                            }*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = modelo.documento;
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = eg.retencion;

                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = eg.retencion;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = eg.retencion;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = eg.retencion;
                                                            }
                                                        }

                                                        #endregion

                                                        #region ReteIVA

                                                        if (parametro.id_nombre_parametro == 4)
                                                        {
                                                            /*if (info.aplicaniff==true)
                                                            {

                                                            }*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(ivaEncabezado, miCultura);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = modelo.documento;
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = eg.retencion_iva;

                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = eg.retencion_iva;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = eg.retencion_iva;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = eg.retencion_iva;
                                                            }
                                                        }

                                                        #endregion

                                                        #region ReteICA

                                                        if (parametro.id_nombre_parametro == 5)
                                                        {
                                                            /*if (info.aplicaniff==true)
                                                            {

                                                            }*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = modelo.documento;
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = eg.retencion_ica;

                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = eg.retencion_ica;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = eg.retencion_ica;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = eg.retencion_ica;
                                                            }
                                                        }

                                                        #endregion

                                                        #region Fletes

                                                        if (parametro.id_nombre_parametro == 6)
                                                        {
                                                            /*if (info.aplicaniff==true)
                                                            {

                                                            }*/

                                                            /*if (info.manejabase == true)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(modelo.fletes);
                                                            }*/
                                                            //else
                                                            //{
                                                            movNuevo.basecontable = 0;
                                                            //}

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = modelo.documento;
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = eg.fletes;
                                                                movNuevo.debito = 0;

                                                                movNuevo.creditoniif = eg.fletes;
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = eg.fletes;
                                                                ;
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = eg.fletes;
                                                                movNuevo.debito = 0;
                                                            }
                                                        }

                                                        #endregion

                                                        #region IVA fletes

                                                        if (parametro.id_nombre_parametro == 14)
                                                        {
                                                            /*if (info.aplicaniff==true)
                                                            {

                                                            }*/

                                                            /*if (info.manejabase == true)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(modelo.fletes);
                                                            }
                                                            else
                                                            {*/
                                                            movNuevo.basecontable = 0;
                                                            //}

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = modelo.documento;
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = eg.iva_fletes;
                                                                movNuevo.debito = 0;

                                                                movNuevo.creditoniif = eg.iva_fletes;
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = eg.iva_fletes;
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = eg.iva_fletes;
                                                                movNuevo.debito = 0;
                                                            }
                                                        }

                                                        #endregion

                                                        #region AutoRetencion Debito

                                                        if (parametro.id_nombre_parametro == 17)
                                                        {
                                                            /*if (info.aplicaniff==true)
                                                            {

                                                            }*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = modelo.documento;
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = eg.retencion_causada;

                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = eg.retencion_causada;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = eg.retencion_causada;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = eg.retencion_causada;
                                                            }
                                                        }

                                                        #endregion

                                                        #region AutoRetencion Credito

                                                        if (parametro.id_nombre_parametro == 18)
                                                        {
                                                            /*if (info.aplicaniff==true)
                                                            {

                                                            }*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = modelo.documento;
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = eg.retencion_causada;
                                                                movNuevo.debito = 0;

                                                                movNuevo.creditoniif = eg.retencion_causada;
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = eg.retencion_causada;
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = eg.retencion_causada;
                                                                movNuevo.debito = 0;
                                                            }
                                                        }

                                                        #endregion

                                                        context.mov_contable.Add(movNuevo);
                                                        context.SaveChanges();

                                                        secuencia++;
                                                        //Cuentas valores

                                                        #region Cuentas valores

                                                        cuentas_valores buscar_cuentas_valores =
                                                            context.cuentas_valores.FirstOrDefault(x =>
                                                                x.centro == parametro.centro &&
                                                                x.cuenta == parametro.cuenta && x.nit == movNuevo.nit);
                                                        if (buscar_cuentas_valores != null)
                                                        {
                                                            buscar_cuentas_valores.debito += movNuevo.debito;
                                                            buscar_cuentas_valores.credito += movNuevo.credito;
                                                            buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                                                            buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                                                            //context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                                        }
                                                        else
                                                        {
                                                            DateTime fechaHoy = DateTime.Now;
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
                                                            context.SaveChanges();
                                                        }

                                                        #endregion

                                                        totalCreditos += movNuevo.credito;
                                                        totalDebitos += movNuevo.debito;
                                                        listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                        {
                                                            NumeroCuenta =
                                                                "(" + buscarCuenta.cntpuc_numero + ")" +
                                                                buscarCuenta.cntpuc_descp,
                                                            DescripcionParametro = descripcionParametro,
                                                            ValorDebito = movNuevo.debito,
                                                            ValorCredito = movNuevo.credito
                                                        });
                                                    }
                                                }
                                            }

                                            #endregion

                                            //Documentos a cruzar

                                            #region Documentos a cruzar

                                            string listaAnticipo = Request["listaAnticipo"];
                                            if (!string.IsNullOrEmpty(listaAnticipo))
                                            {
                                                int la = Convert.ToInt32(listaAnticipo);
                                                for (int i = 1; i <= la; i++)
                                                {
                                                    int encabAnti = Convert.ToInt32(Request["encabAnticipo" + i]);
                                                    if (encabAnti != 0)
                                                    {
                                                        encab_documento encabezadoAnticipo =
                                                            context.encab_documento.FirstOrDefault(x =>
                                                                x.idencabezado == encabAnti);

                                                        documentosacruzar dac = new documentosacruzar
                                                        {
                                                            idencabrecibo = encabAnti,
                                                            valorrecibo = encabezadoAnticipo.valor_total,
                                                            idencabfactura = id_encabezado,
                                                            valorfactura = eg.valor_total,
                                                            saldo = encabezadoAnticipo.valor_total - eg.valor_total
                                                        };

                                                        context.documentosacruzar.Add(dac);
                                                        context.SaveChanges();
                                                    }
                                                }
                                            }

                                            #endregion

                                            //Lineas documento

                                            #region lineasDocumento

                                            List<mov_contable> listaMov = new List<mov_contable>();
                                            int listaLineas = listaelementos.Count();
                                            for (int i = 0; i < listaLineas; i++)
                                            {
                                                //if (!string.IsNullOrEmpty(Request["referencia" + i]))
                                                //{
                                                //var porDescuento = (!string.IsNullOrEmpty(Request["descuentoReferencia" + i])) ? Convert.ToDecimal(Request["descuentoReferencia" + i]) : 0;
                                                decimal porDescuento = listaelementos[i].porcentaje_descuento;

                                                //var codigo = Request["referencia" + i];
                                                string codigo = listaelementos[i].codigo;

                                                //var cantidadFacturada = Convert.ToDecimal(Request["cantidadReferencia" + i]);
                                                decimal cantidadFacturada = listaelementos[i].cantidad;
                                                //var valorReferencia = Convert.ToDecimal(Request["valorUnitarioReferencia" + i]);
                                                decimal valorReferencia = listaelementos[i].valor_unitario;
                                                decimal descontar = porDescuento / 100;
                                                //var porIVAReferencia = Convert.ToDecimal(Request["ivaReferencia" + i]) / 100;
                                                decimal porIVAReferencia = listaelementos[i].porcentaje_iva / 100;

                                                decimal final = Math.Round(valorReferencia - valorReferencia * descontar);
                                                //var baseUnitario = final * Convert.ToDecimal(Request["cantidadReferencia" + i]);
                                                decimal baseUnitario = final * listaelementos[i].cantidad;

                                                decimal ivaReferencia =
                                                    Math.Round(final * porIVAReferencia * listaelementos[i].cantidad);
                                                string und = "0";
                                                if (listaelementos[i].tipo == "R")
                                                {
                                                    icb_referencia unidadCodigo =
                                                        context.icb_referencia.FirstOrDefault(x => x.ref_codigo == codigo);
                                                    und = unidadCodigo.unidad_medida;
                                                }

                                                decimal costoReferencia = 0;
                                                decimal cr = 0;
                                                if (listaelementos[i].tipo == "R")
                                                {
                                                    //var vwPromedio = context.vw_promedio.FirstOrDefault(x => x.codigo == codigo && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month);
                                                    icb_referencia vwPromedio =
                                                        context.icb_referencia.FirstOrDefault(x => x.ref_codigo == codigo);
                                                    costoReferencia = vwPromedio.costo_promedio ?? 0;
                                                    cr = costoReferencia * listaelementos[i].cantidad;
                                                }
                                                else if(listaelementos[i].tipo == "M")
                                                    {
                                                    ttempario vwPromedio =
                                                        context.ttempario.FirstOrDefault(x => x.codigo == codigo);
                                                    costoReferencia = vwPromedio.costo ?? 0;
                                                    cr = costoReferencia * listaelementos[i].cantidad;
                                                }

                                                if (listaelementos[i].tipo == "R")
                                                {
                                                    lineas_documento lineas = new lineas_documento
                                                    {
                                                        id_encabezado = id_encabezado,
                                                        seq = i + 1,
                                                        fec = DateTime.Now,
                                                        nit = ordentaller.tercero
                                                    };
                                                    if (listaelementos[i].tipo == "R")
                                                    {
                                                        lineas.und = Convert.ToString(und);
                                                        lineas.codigo = codigo;
                                                    }

                                                    lineas.cantidad = listaelementos[i].cantidad;
                                                    decimal ivaLista = listaelementos[i].porcentaje_iva;
                                                    lineas.porcentaje_iva = (float)ivaLista;
                                                    lineas.valor_unitario = final;
                                                    decimal descuento = porDescuento;
                                                    lineas.porcentaje_descuento = (float)descuento;
                                                    lineas.costo_unitario = Convert.ToDecimal(costoReferencia, miCultura);
                                                    lineas.bodega = ordentaller.bodega;
                                                    lineas.fec_creacion = DateTime.Now;
                                                    lineas.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                                    lineas.estado = true;
                                                    lineas.id_tarifa_cliente = listaelementos[i].tipo_tarifa;
                                                    lineas.moneda = moneda;
                                                    lineas.vendedor = eg.vendedor;

                                                    context.lineas_documento.Add(lineas);
                                                }
                                                else if (listaelementos[i].tipo == "M")
                                                    {
                                                    lineas_documento_operaciones lineasoperaciones = new lineas_documento_operaciones();
                                                    lineasoperaciones.id_encabezado = id_encabezado;
                                                    lineasoperaciones.seq = i + 1;
                                                    lineasoperaciones.fec = DateTime.Now;
                                                    lineasoperaciones.nit = ordentaller.tercero;
                                                    lineasoperaciones.codigo_operacion = codigo;
                                                    lineasoperaciones.porcentaje_iva = float.Parse(listaelementos[i].porcentaje_iva.ToString());
                                                    //lineasoperaciones.porcentaje_iva = listaelementos[i].porcentaje_iva != null ? float.Parse(listaelementos[i].porcentaje_iva.ToString()) : 0;
                                                    lineasoperaciones.costo_unitario = listaelementos[i].valor_unitario;
                                                    lineasoperaciones.bodega = ordentaller.bodega;
                                                    lineasoperaciones.fec_creacion = DateTime.Now;
                                                    lineasoperaciones.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                                    lineasoperaciones.estado = true;
                                                    lineasoperaciones.moneda = moneda;
                                                    lineasoperaciones.vendedor = eg.vendedor;
                                                    context.lineas_documento_operaciones.Add(lineasoperaciones);
                                                }

                                                #endregion

                                                #region Mov Contable (IVA, Inventario, Costo, Ingreso)

                                                foreach (var parametro in parametrosCuentasVerificar)
                                                {
                                                    string descripcionParametro = context.paramcontablenombres
                                                        .FirstOrDefault(x => x.id == parametro.id_nombre_parametro)
                                                        .descripcion_parametro;
                                                    cuenta_puc buscarCuenta =
                                                        context.cuenta_puc.FirstOrDefault(x =>
                                                            x.cntpuc_id == parametro.cuenta);

                                                    if (buscarCuenta != null)
                                                    {
                                                        if (parametro.id_nombre_parametro == 2 &&
                                                            Convert.ToDecimal(ivaEncabezado, miCultura) != 0
                                                            || parametro.id_nombre_parametro == 9 &&
                                                            Convert.ToDecimal(costoPromedioTotal, miCultura) != 0 //costo promedio
                                                            || parametro.id_nombre_parametro == 20 &&
                                                            Convert.ToDecimal(costoPromedioTotal, miCultura) != 0 //costo promedio
                                                            || parametro.id_nombre_parametro == 11 &&
                                                            Convert.ToDecimal(costoEncabezado, miCultura) != 0
                                                            || parametro.id_nombre_parametro == 12 &&
                                                            Convert.ToDecimal(costoPromedioTotal, miCultura) != 0) //costo promedio
                                                        {
                                                            mov_contable movNuevo = new mov_contable
                                                            {
                                                                id_encab = encabezado.idencabezado,
                                                                seq = secuencia,
                                                                idparametronombre = parametro.id_nombre_parametro,
                                                                cuenta = parametro.cuenta,
                                                                centro = listaelementos[i].tipo_tarifa == 2
                                                                ? parametro.id_nombre_parametro == 11
                                                                    ?
                                                                    listaelementos[i].centro_costo
                                                                    : parametro.id_nombre_parametro == 12
                                                                        ? listaelementos[i].centro_costo
                                                                        : parametro.centro
                                                                : parametro.centro,
                                                                fec = DateTime.Now,
                                                                fec_creacion = DateTime.Now,
                                                                tipo_tarifa = listaelementos[i].tipo_tarifa,
                                                                userid_creacion =
                                                                Convert.ToInt32(Session["user_usuarioid"]),
                                                                documento = Convert.ToString(ordentaller.id)
                                                            };

                                                            cuenta_puc info = context.cuenta_puc
                                                                .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                .FirstOrDefault();

                                                            if (info.tercero)
                                                            {
                                                                movNuevo.nit = ordentaller.tercero;
                                                            }
                                                            else
                                                            {
                                                                icb_terceros tercero = context.icb_terceros
                                                                    .Where(t => t.doc_tercero == "0").FirstOrDefault();
                                                                movNuevo.nit = tercero.tercero_id;
                                                            }

                                                            #region IVA

                                                            if (parametro.id_nombre_parametro == 2)
                                                            {
                                                                icb_referencia perfilReferencia =
                                                                    context.icb_referencia.FirstOrDefault(x =>
                                                                        x.ref_codigo == codigo);
                                                                int perfilBuscar = 0;

                                                                if (perfilReferencia != null)
                                                                {
                                                                    perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                                                }

                                                                perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(
                                                                    r => r.id == perfilBuscar);

                                                                #region Tiene perfil la referencia

                                                                if (pcr != null)
                                                                {
                                                                    int? cuentaIva = pcr.cuenta_dev_iva_compras;

                                                                    movNuevo.id_encab = encabezado.idencabezado;
                                                                    movNuevo.seq = secuencia;
                                                                    movNuevo.idparametronombre =
                                                                        parametro.id_nombre_parametro;

                                                                    #region si tiene perfil y cuenta asignada a ese perfil

                                                                    if (cuentaIva != null)
                                                                    {
                                                                        movNuevo.cuenta = Convert.ToInt32(cuentaIva);
                                                                        movNuevo.centro = parametro.centro;
                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        movNuevo.documento = Convert.ToString(eg.numero);

                                                                        cuenta_puc infoReferencia = context.cuenta_puc
                                                                            .Where(a => a.cntpuc_id == cuentaIva)
                                                                            .FirstOrDefault();
                                                                        if (infoReferencia.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (infoReferencia.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (infoReferencia.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito =
                                                                                Convert.ToDecimal(ivaReferencia, miCultura);
                                                                            movNuevo.debito = 0;

                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(ivaReferencia, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(ivaReferencia, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito =
                                                                                Convert.ToDecimal(ivaReferencia, miCultura);
                                                                            movNuevo.debito = 0;
                                                                        }

                                                                        // context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion

                                                                    #region si tiene perfil pero no tiene cuenta asignada

                                                                    else
                                                                    {
                                                                        movNuevo.cuenta = parametro.cuenta;
                                                                        movNuevo.centro = parametro.centro;
                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        movNuevo.documento = Convert.ToString(eg.numero);

                                                                        cuenta_puc infoReferencia = context.cuenta_puc
                                                                            .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                            .FirstOrDefault();
                                                                        if (infoReferencia.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (infoReferencia.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (infoReferencia.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito =
                                                                                Convert.ToDecimal(ivaReferencia, miCultura);
                                                                            movNuevo.debito = 0;

                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(ivaReferencia, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(ivaReferencia, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito =
                                                                                Convert.ToDecimal(ivaReferencia, miCultura);
                                                                            movNuevo.debito = 0;
                                                                        }

                                                                        //context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion
                                                                }

                                                                #endregion

                                                                #region La referencia no tiene perfil

                                                                else
                                                                {
                                                                    movNuevo.id_encab = encabezado.idencabezado;
                                                                    movNuevo.seq = secuencia;
                                                                    movNuevo.idparametronombre =
                                                                        parametro.id_nombre_parametro;
                                                                    movNuevo.cuenta = parametro.cuenta;
                                                                    movNuevo.centro = parametro.centro;
                                                                    movNuevo.fec = DateTime.Now;
                                                                    movNuevo.fec_creacion = DateTime.Now;
                                                                    movNuevo.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);

                                                                    if (info.manejabase)
                                                                    {
                                                                        movNuevo.basecontable =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                    }
                                                                    else
                                                                    {
                                                                        movNuevo.basecontable = 0;
                                                                    }

                                                                    if (info.documeto)
                                                                    {
                                                                        movNuevo.documento = Convert.ToString(eg.numero);
                                                                    }

                                                                    if (buscarCuenta.concepniff == 1)
                                                                    {
                                                                        movNuevo.credito = Convert.ToDecimal(ivaReferencia, miCultura);
                                                                        movNuevo.debito = 0;

                                                                        movNuevo.creditoniif =
                                                                            Convert.ToDecimal(ivaReferencia, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (buscarCuenta.concepniff == 4)
                                                                    {
                                                                        movNuevo.creditoniif =
                                                                            Convert.ToDecimal(ivaReferencia, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (buscarCuenta.concepniff == 5)
                                                                    {
                                                                        movNuevo.credito = Convert.ToDecimal(ivaReferencia, miCultura);
                                                                        movNuevo.debito = 0;
                                                                    }

                                                                    //context.mov_contable.Add(movNuevo);
                                                                }

                                                                #endregion

                                                                mov_contable buscarIVA = context.mov_contable.FirstOrDefault(x =>
                                                                    x.id_encab == id_encabezado &&
                                                                    x.cuenta == movNuevo.cuenta &&
                                                                    x.idparametronombre == parametro.id_nombre_parametro);
                                                                if (buscarIVA != null)
                                                                {
                                                                    buscarIVA.debito += movNuevo.debito;
                                                                    buscarIVA.debitoniif += movNuevo.debitoniif;
                                                                    buscarIVA.credito += movNuevo.credito;
                                                                    buscarIVA.creditoniif += movNuevo.creditoniif;
                                                                    //context.Entry(buscarIVA).State = EntityState.Modified;
                                                                }
                                                                else
                                                                {
                                                                    mov_contable crearMovContable = new mov_contable
                                                                    {
                                                                        id_encab = encabezado.idencabezado,
                                                                        seq = secuencia,
                                                                        idparametronombre =
                                                                        parametro.id_nombre_parametro,
                                                                        cuenta =
                                                                        Convert.ToInt32(movNuevo.cuenta),
                                                                        centro = parametro.centro,
                                                                        nit = encabezado.nit,
                                                                        fec = DateTime.Now,
                                                                        debito = movNuevo.debito,
                                                                        debitoniif = movNuevo.debitoniif,
                                                                        basecontable = movNuevo.basecontable,
                                                                        credito = movNuevo.credito,
                                                                        creditoniif = movNuevo.creditoniif,
                                                                        fec_creacion = DateTime.Now,
                                                                        userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]),
                                                                        detalle =
                                                                        "Facturacion de orden taller N° " +
                                                                        ordentaller.codigoentrada + " con consecutivo " +
                                                                        eg.numero,
                                                                        estado = true
                                                                    };
                                                                    context.mov_contable.Add(crearMovContable);
                                                                    context.SaveChanges();
                                                                }
                                                            }

                                                            #endregion

                                                            #region Inventario

                                                            //si el tipo de elemento que estoy analizando en este momento es de inventario
                                                            if (listaelementos[i].tipo == "R")
                                                            {
                                                                if (parametro.id_nombre_parametro == 9 ||
                                                                    parametro.id_nombre_parametro == 20)
                                                                {
                                                                    icb_referencia perfilReferencia =
                                                                        context.icb_referencia.FirstOrDefault(x =>
                                                                            x.ref_codigo == codigo);
                                                                    int perfilBuscar =
                                                                        Convert.ToInt32(perfilReferencia.perfil);
                                                                    perfilcontable_referencia pcr = context.perfilcontable_referencia
                                                                        .FirstOrDefault(r => r.id == perfilBuscar);

                                                                    #region Tiene perfil la referencia

                                                                    if (pcr != null)
                                                                    {
                                                                        int? cuentaInven = pcr.cta_inventario;

                                                                        movNuevo.id_encab = encabezado.idencabezado;
                                                                        movNuevo.seq = secuencia;
                                                                        movNuevo.idparametronombre =
                                                                            parametro.id_nombre_parametro;

                                                                        #region tiene perfil y cuenta asignada al perfil

                                                                        if (cuentaInven != null)
                                                                        {
                                                                            movNuevo.cuenta = Convert.ToInt32(cuentaInven);
                                                                            movNuevo.centro = parametro.centro;
                                                                            movNuevo.fec = DateTime.Now;
                                                                            movNuevo.fec_creacion = DateTime.Now;
                                                                            movNuevo.userid_creacion =
                                                                                Convert.ToInt32(Session["user_usuarioid"]);
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);

                                                                            cuenta_puc infoReferencia = context.cuenta_puc
                                                                                .Where(a => a.cntpuc_id == cuentaInven)
                                                                                .FirstOrDefault();
                                                                            if (infoReferencia.manejabase)
                                                                            {
                                                                                movNuevo.basecontable =
                                                                                    Convert.ToDecimal(baseUnitario, miCultura);
                                                                            }
                                                                            else
                                                                            {
                                                                                movNuevo.basecontable = 0;
                                                                            }

                                                                            if (infoReferencia.documeto)
                                                                            {
                                                                                movNuevo.documento =
                                                                                    Convert.ToString(eg.numero);
                                                                            }

                                                                            if (infoReferencia.concepniff == 1)
                                                                            {
                                                                                movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                                movNuevo.debito = 0;

                                                                                movNuevo.creditoniif =
                                                                                    Convert.ToDecimal(cr, miCultura);
                                                                                movNuevo.debitoniif = 0;
                                                                            }

                                                                            if (infoReferencia.concepniff == 4)
                                                                            {
                                                                                movNuevo.creditoniif =
                                                                                    Convert.ToDecimal(cr, miCultura);
                                                                                movNuevo.debitoniif = 0;
                                                                            }

                                                                            if (infoReferencia.concepniff == 5)
                                                                            {
                                                                                movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                                movNuevo.debito = 0;
                                                                            }

                                                                            //context.mov_contable.Add(movNuevo);
                                                                        }

                                                                        #endregion

                                                                        #region tiene perfil pero no tiene cuenta asignada

                                                                        else
                                                                        {
                                                                            movNuevo.cuenta = parametro.cuenta;
                                                                            movNuevo.centro = parametro.centro;
                                                                            movNuevo.fec = DateTime.Now;
                                                                            movNuevo.fec_creacion = DateTime.Now;
                                                                            movNuevo.userid_creacion =
                                                                                Convert.ToInt32(Session["user_usuarioid"]);
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);

                                                                            cuenta_puc infoReferencia = context.cuenta_puc
                                                                                .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                                .FirstOrDefault();
                                                                            if (infoReferencia.manejabase)
                                                                            {
                                                                                movNuevo.basecontable =
                                                                                    Convert.ToDecimal(valor_totalenca, miCultura);
                                                                            }
                                                                            else
                                                                            {
                                                                                movNuevo.basecontable = 0;
                                                                            }

                                                                            if (infoReferencia.documeto)
                                                                            {
                                                                                movNuevo.documento =
                                                                                    Convert.ToString(eg.numero);
                                                                            }

                                                                            if (infoReferencia.concepniff == 1)
                                                                            {
                                                                                movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                                movNuevo.debito = 0;

                                                                                movNuevo.creditoniif =
                                                                                    Convert.ToDecimal(cr, miCultura);
                                                                                movNuevo.debitoniif = 0;
                                                                            }

                                                                            if (infoReferencia.concepniff == 4)
                                                                            {
                                                                                movNuevo.creditoniif =
                                                                                    Convert.ToDecimal(cr, miCultura);
                                                                                movNuevo.debitoniif = 0;
                                                                            }

                                                                            if (infoReferencia.concepniff == 5)
                                                                            {
                                                                                movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                                movNuevo.debito = 0;
                                                                            }

                                                                            //context.mov_contable.Add(movNuevo);
                                                                        }

                                                                        #endregion
                                                                    }

                                                                    #endregion

                                                                    #region La referencia no tiene perfil

                                                                    else
                                                                    {
                                                                        movNuevo.id_encab = encabezado.idencabezado;
                                                                        movNuevo.seq = secuencia;
                                                                        movNuevo.idparametronombre =
                                                                            parametro.id_nombre_parametro;
                                                                        movNuevo.cuenta = parametro.cuenta;
                                                                        movNuevo.centro = parametro.centro;
                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        /*if (info.aplicaniff==true)
                                                                        {

                                                                        }*/

                                                                        if (info.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(valor_totalenca, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (info.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (buscarCuenta.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debito = 0;

                                                                            movNuevo.creditoniif = Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (buscarCuenta.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif = Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (buscarCuenta.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debito = 0;
                                                                        }

                                                                        //context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion

                                                                    mov_contable buscarInventario =
                                                                        context.mov_contable.FirstOrDefault(x =>
                                                                            x.id_encab == id_encabezado &&
                                                                            x.cuenta == movNuevo.cuenta &&
                                                                            x.idparametronombre ==
                                                                            parametro.id_nombre_parametro);
                                                                    if (buscarInventario != null)
                                                                    {
                                                                        buscarInventario.basecontable +=
                                                                            movNuevo.basecontable;
                                                                        buscarInventario.debito += movNuevo.debito;
                                                                        buscarInventario.debitoniif += movNuevo.debitoniif;
                                                                        buscarInventario.credito += movNuevo.credito;
                                                                        buscarInventario.creditoniif +=
                                                                            movNuevo.creditoniif;
                                                                        //context.Entry(buscarInventario).State =
                                                                        //  EntityState.Modified;
                                                                    }
                                                                    else
                                                                    {
                                                                        mov_contable crearMovContable = new mov_contable
                                                                        {
                                                                            id_encab = encabezado.idencabezado,
                                                                            seq = secuencia,
                                                                            idparametronombre =
                                                                            parametro.id_nombre_parametro,
                                                                            cuenta =
                                                                            Convert.ToInt32(movNuevo.cuenta),
                                                                            centro = parametro.centro,
                                                                            nit = encabezado.nit,
                                                                            fec = DateTime.Now,
                                                                            debito = movNuevo.debito,
                                                                            debitoniif = movNuevo.debitoniif,
                                                                            basecontable =
                                                                            movNuevo.basecontable,
                                                                            credito = movNuevo.credito,
                                                                            creditoniif = movNuevo.creditoniif,
                                                                            fec_creacion = DateTime.Now,
                                                                            userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]),
                                                                            detalle =
                                                                            "Facturacion de orden de taller N° " +
                                                                            ordentaller.codigoentrada +
                                                                            " con consecutivo " + eg.numero,
                                                                            estado = true
                                                                        };
                                                                        context.mov_contable.Add(crearMovContable);
                                                                        context.SaveChanges();
                                                                    }
                                                                }
                                                            }

                                                            #endregion

                                                            #region Ingreso	

                                                            //var siva = Request["tipo_tarifa_hidden_" + i] == "2";
                                                            bool siva = listaelementos[i].tipo_tarifa == 2;
                                                            if (parametro.id_nombre_parametro == 11 && siva != true)
                                                            {
                                                                icb_referencia perfilReferencia =
                                                                    context.icb_referencia.FirstOrDefault(x =>
                                                                        x.ref_codigo == codigo);
                                                                int perfilBuscar = 0;
                                                                if (perfilReferencia != null &&
                                                                    listaelementos[i].tipo == "R")
                                                                {
                                                                    perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                                                }

                                                                perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(
                                                                    r => r.id == perfilBuscar);

                                                                #region Tiene perfil la referencia

                                                                if (pcr != null)
                                                                {
                                                                    int? cuentaVenta = pcr.cuenta_ventas;

                                                                    movNuevo.id_encab = encabezado.idencabezado;
                                                                    movNuevo.seq = secuencia;
                                                                    movNuevo.idparametronombre =
                                                                        parametro.id_nombre_parametro;

                                                                    #region tiene perfil y cuenta asignada al perfil

                                                                    if (cuentaVenta != null)
                                                                    {
                                                                        movNuevo.cuenta = Convert.ToInt32(cuentaVenta);
                                                                        //movNuevo.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 11 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro; ;
                                                                        movNuevo.centro = listaelementos[i].tipo_tarifa == 2
                                                                            ? parametro.id_nombre_parametro == 11
                                                                                ?
                                                                                listaelementos[i].centro_costo
                                                                                : parametro.centro
                                                                            : parametro.centro;
                                                                        ;

                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        movNuevo.documento = Convert.ToString(eg.numero);

                                                                        cuenta_puc infoReferencia = context.cuenta_puc
                                                                            .Where(a => a.cntpuc_id == cuentaVenta)
                                                                            .FirstOrDefault();
                                                                        if (infoReferencia.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (infoReferencia.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (infoReferencia.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debito = 0;

                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debito = 0;
                                                                        }

                                                                        //context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion

                                                                    #region tiene perfil pero no tiene cuenta asignada

                                                                    else
                                                                    {
                                                                        movNuevo.cuenta = parametro.cuenta;
                                                                        //movNuevo.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 11 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro; ;
                                                                        movNuevo.centro = listaelementos[i].tipo_tarifa == 2
                                                                            ? parametro.id_nombre_parametro == 11
                                                                                ?
                                                                                listaelementos[i].centro_costo
                                                                                : parametro.centro
                                                                            : parametro.centro;
                                                                        ;

                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        movNuevo.documento = Convert.ToString(eg.numero);

                                                                        cuenta_puc infoReferencia = context.cuenta_puc
                                                                            .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                            .FirstOrDefault();
                                                                        if (infoReferencia.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(valor_totalenca, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (infoReferencia.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (infoReferencia.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debito = 0;

                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debito = 0;
                                                                        }

                                                                        //context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion
                                                                }

                                                                #endregion

                                                                #region La referencia no tiene perfil

                                                                else
                                                                {
                                                                    movNuevo.id_encab = encabezado.idencabezado;
                                                                    movNuevo.seq = secuencia;
                                                                    movNuevo.idparametronombre =
                                                                        parametro.id_nombre_parametro;
                                                                    movNuevo.cuenta = parametro.cuenta;
                                                                    //movNuevo.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 11 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro;
                                                                    movNuevo.centro = listaelementos[i].tipo_tarifa == 2
                                                                        ? parametro.id_nombre_parametro == 11
                                                                            ?
                                                                            listaelementos[i].centro_costo
                                                                            : parametro.centro
                                                                        : parametro.centro;

                                                                    movNuevo.fec = DateTime.Now;
                                                                    movNuevo.fec_creacion = DateTime.Now;
                                                                    movNuevo.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);
                                                                    /*if (info.aplicaniff==true)
                                                                    {

                                                                    }*/

                                                                    if (info.manejabase)
                                                                    {
                                                                        movNuevo.basecontable =
                                                                            Convert.ToDecimal(valor_totalenca, miCultura);
                                                                    }
                                                                    else
                                                                    {
                                                                        movNuevo.basecontable = 0;
                                                                    }

                                                                    if (info.documeto)
                                                                    {
                                                                        movNuevo.documento = Convert.ToString(eg.numero);
                                                                    }

                                                                    if (buscarCuenta.concepniff == 1)
                                                                    {
                                                                        movNuevo.credito = Convert.ToDecimal(baseUnitario, miCultura);
                                                                        movNuevo.debito = 0;

                                                                        movNuevo.creditoniif =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (buscarCuenta.concepniff == 4)
                                                                    {
                                                                        movNuevo.creditoniif =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (buscarCuenta.concepniff == 5)
                                                                    {
                                                                        movNuevo.credito = Convert.ToDecimal(baseUnitario, miCultura);
                                                                        movNuevo.debito = 0;
                                                                    }

                                                                    //context.mov_contable.Add(movNuevo);
                                                                }

                                                                #endregion

                                                                mov_contable buscarVenta = context.mov_contable.FirstOrDefault(x =>
                                                                    x.id_encab == id_encabezado &&
                                                                    x.cuenta == movNuevo.cuenta &&
                                                                    x.idparametronombre == parametro.id_nombre_parametro);
                                                                if (buscarVenta != null)
                                                                {
                                                                    buscarVenta.basecontable += movNuevo.basecontable;
                                                                    buscarVenta.debito += movNuevo.debito;
                                                                    buscarVenta.debitoniif += movNuevo.debitoniif;
                                                                    buscarVenta.credito += movNuevo.credito;
                                                                    buscarVenta.creditoniif += movNuevo.creditoniif;
                                                                    //context.Entry(buscarVenta).State = EntityState.Modified;
                                                                }
                                                                else
                                                                {
                                                                    mov_contable crearMovContable = new mov_contable
                                                                    {
                                                                        id_encab = encabezado.idencabezado,
                                                                        seq = secuencia,
                                                                        idparametronombre =
                                                                        parametro.id_nombre_parametro,
                                                                        cuenta =
                                                                        Convert.ToInt32(movNuevo.cuenta),
                                                                        //crearMovContable.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 11 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro;
                                                                        centro =
                                                                        listaelementos[i].tipo_tarifa == 2
                                                                            ? parametro.id_nombre_parametro == 11
                                                                                ?
                                                                                listaelementos[i].centro_costo
                                                                                : parametro.centro
                                                                            : parametro.centro,

                                                                        nit = encabezado.nit,
                                                                        fec = DateTime.Now,
                                                                        debito = movNuevo.debito,
                                                                        debitoniif = movNuevo.debitoniif,
                                                                        basecontable = movNuevo.basecontable,
                                                                        credito = movNuevo.credito,
                                                                        creditoniif = movNuevo.creditoniif,
                                                                        fec_creacion = DateTime.Now,
                                                                        userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]),
                                                                        detalle =
                                                                        "Facturacion de Orden de Taller N° " +
                                                                        ordentaller.codigoentrada + " con consecutivo " +
                                                                        eg.numero,
                                                                        estado = true
                                                                    };
                                                                    context.mov_contable.Add(crearMovContable);
                                                                    context.SaveChanges();
                                                                }
                                                            }

                                                            #endregion

                                                            #region Costo

                                                            if (listaelementos[i].tipo == "R")
                                                            {
                                                                if (parametro.id_nombre_parametro == 12)
                                                                {
                                                                    icb_referencia perfilReferencia =
                                                                        context.icb_referencia.FirstOrDefault(x =>
                                                                            x.ref_codigo == codigo);
                                                                    int perfilBuscar = 0;
                                                                    if (listaelementos[i].tipo == "R")
                                                                    {
                                                                        perfilBuscar =
                                                                            Convert.ToInt32(perfilReferencia.perfil);
                                                                    }

                                                                    perfilcontable_referencia pcr = context.perfilcontable_referencia
                                                                        .FirstOrDefault(r => r.id == perfilBuscar);

                                                                    #region Tiene perfil la referencia

                                                                    if (pcr != null)
                                                                    {
                                                                        int? cuentaCosto = pcr.cuenta_costo;

                                                                        movNuevo.id_encab = encabezado.idencabezado;
                                                                        movNuevo.seq = secuencia;
                                                                        movNuevo.idparametronombre =
                                                                            parametro.id_nombre_parametro;

                                                                        #region tiene perfil y cuenta asignada al perfil

                                                                        if (cuentaCosto != null)
                                                                        {
                                                                            movNuevo.cuenta = Convert.ToInt32(cuentaCosto);
                                                                            //movNuevo.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 12 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro;
                                                                            movNuevo.centro =
                                                                                listaelementos[i].tipo_tarifa == 2
                                                                                    ? parametro.id_nombre_parametro == 12
                                                                                        ?
                                                                                        listaelementos[i].centro_costo
                                                                                        : parametro.centro
                                                                                    : parametro.centro;

                                                                            movNuevo.fec = DateTime.Now;
                                                                            movNuevo.fec_creacion = DateTime.Now;
                                                                            movNuevo.userid_creacion =
                                                                                Convert.ToInt32(Session["user_usuarioid"]);
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);

                                                                            cuenta_puc infoReferencia = context.cuenta_puc
                                                                                .Where(a => a.cntpuc_id == cuentaCosto)
                                                                                .FirstOrDefault();
                                                                            if (infoReferencia.manejabase)
                                                                            {
                                                                                movNuevo.basecontable =
                                                                                    Convert.ToDecimal(valor_totalenca, miCultura);
                                                                            }
                                                                            else
                                                                            {
                                                                                movNuevo.basecontable = 0;
                                                                            }

                                                                            if (infoReferencia.documeto)
                                                                            {
                                                                                movNuevo.documento =
                                                                                    Convert.ToString(eg.numero);
                                                                            }

                                                                            if (infoReferencia.concepniff == 1)
                                                                            {
                                                                                movNuevo.credito = 0;
                                                                                movNuevo.debito = Convert.ToDecimal(cr, miCultura);

                                                                                movNuevo.creditoniif = 0;
                                                                                movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                            }

                                                                            if (infoReferencia.concepniff == 4)
                                                                            {
                                                                                movNuevo.creditoniif = 0;
                                                                                movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                            }

                                                                            if (infoReferencia.concepniff == 5)
                                                                            {
                                                                                movNuevo.credito = 0;
                                                                                movNuevo.debito = Convert.ToDecimal(cr, miCultura);
                                                                            }

                                                                            //context.mov_contable.Add(movNuevo);
                                                                        }

                                                                        #endregion

                                                                        #region tiene perfil pero no tiene cuenta asignada

                                                                        else
                                                                        {
                                                                            movNuevo.cuenta = parametro.cuenta;
                                                                            //movNuevo.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 12 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro; ;
                                                                            movNuevo.centro =
                                                                                listaelementos[i].tipo_tarifa == 2
                                                                                    ? parametro.id_nombre_parametro == 12
                                                                                        ?
                                                                                        listaelementos[i].centro_costo
                                                                                        : parametro.centro
                                                                                    : parametro.centro;

                                                                            movNuevo.fec = DateTime.Now;
                                                                            movNuevo.fec_creacion = DateTime.Now;
                                                                            movNuevo.userid_creacion =
                                                                                Convert.ToInt32(Session["user_usuarioid"]);
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);

                                                                            cuenta_puc infoReferencia = context.cuenta_puc
                                                                                .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                                .FirstOrDefault();
                                                                            if (infoReferencia.manejabase)
                                                                            {
                                                                                movNuevo.basecontable =
                                                                                    Convert.ToDecimal(valor_totalenca, miCultura);
                                                                            }
                                                                            else
                                                                            {
                                                                                movNuevo.basecontable = 0;
                                                                            }

                                                                            if (infoReferencia.documeto)
                                                                            {
                                                                                movNuevo.documento =
                                                                                    Convert.ToString(eg.numero);
                                                                            }

                                                                            if (infoReferencia.concepniff == 1)
                                                                            {
                                                                                movNuevo.credito = 0;
                                                                                movNuevo.debito = Convert.ToDecimal(cr, miCultura);

                                                                                movNuevo.creditoniif = 0;
                                                                                movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                            }

                                                                            if (infoReferencia.concepniff == 4)
                                                                            {
                                                                                movNuevo.creditoniif = 0;
                                                                                movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                            }

                                                                            if (infoReferencia.concepniff == 5)
                                                                            {
                                                                                movNuevo.credito = 0;
                                                                                movNuevo.debito = Convert.ToDecimal(cr, miCultura);
                                                                            }

                                                                            //context.mov_contable.Add(movNuevo);
                                                                        }

                                                                        #endregion
                                                                    }

                                                                    #endregion

                                                                    #region La referencia no tiene perfil

                                                                    else
                                                                    {
                                                                        movNuevo.id_encab = encabezado.idencabezado;
                                                                        movNuevo.seq = secuencia;
                                                                        movNuevo.idparametronombre =
                                                                            parametro.id_nombre_parametro;
                                                                        movNuevo.cuenta = parametro.cuenta;
                                                                        //movNuevo.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 12 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro;
                                                                        movNuevo.centro = listaelementos[i].tipo_tarifa == 2
                                                                            ? parametro.id_nombre_parametro == 12
                                                                                ?
                                                                                listaelementos[i].centro_costo
                                                                                : parametro.centro
                                                                            : parametro.centro;

                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        /*if (info.aplicaniff==true)
                                                                        {

                                                                        }*/

                                                                        if (info.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(valor_totalenca, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (info.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (buscarCuenta.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito = 0;
                                                                            movNuevo.debito = Convert.ToDecimal(cr, miCultura);

                                                                            movNuevo.creditoniif = 0;
                                                                            movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                        }

                                                                        if (buscarCuenta.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif = 0;
                                                                            movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                        }

                                                                        if (buscarCuenta.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito = 0;
                                                                            movNuevo.debito = Convert.ToDecimal(cr, miCultura);
                                                                        }

                                                                        //context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion

                                                                    mov_contable buscarCosto =
                                                                        context.mov_contable.FirstOrDefault(x =>
                                                                            x.id_encab == id_encabezado &&
                                                                            x.cuenta == movNuevo.cuenta &&
                                                                            x.idparametronombre ==
                                                                            parametro.id_nombre_parametro);
                                                                    if (buscarCosto != null)
                                                                    {
                                                                        buscarCosto.basecontable += movNuevo.basecontable;
                                                                        buscarCosto.debito += movNuevo.debito;
                                                                        buscarCosto.debitoniif += movNuevo.debitoniif;
                                                                        buscarCosto.credito += movNuevo.credito;
                                                                        buscarCosto.creditoniif += movNuevo.creditoniif;
                                                                        //context.Entry(buscarCosto).State = EntityState.Modified;

                                                                    }
                                                                    else
                                                                    {
                                                                        mov_contable crearMovContable = new mov_contable
                                                                        {
                                                                            id_encab = encabezado.idencabezado,
                                                                            seq = secuencia,
                                                                            idparametronombre =
                                                                            parametro.id_nombre_parametro,
                                                                            cuenta =
                                                                            Convert.ToInt32(movNuevo.cuenta),
                                                                            //crearMovContable.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 12 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro;
                                                                            centro =
                                                                            listaelementos[i].tipo_tarifa == 2
                                                                                ? parametro.id_nombre_parametro == 12
                                                                                    ?
                                                                                    listaelementos[i].centro_costo
                                                                                    : parametro.centro
                                                                                : parametro.centro,

                                                                            nit = encabezado.nit,
                                                                            fec = DateTime.Now,
                                                                            debito = movNuevo.debito,
                                                                            debitoniif = movNuevo.debitoniif,
                                                                            basecontable =
                                                                            movNuevo.basecontable,
                                                                            credito = movNuevo.credito,
                                                                            creditoniif = movNuevo.creditoniif,
                                                                            fec_creacion = DateTime.Now,
                                                                            userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]),
                                                                            detalle =
                                                                            "Facturacion de Orden de Taller N° " +
                                                                            ordentaller.codigoentrada +
                                                                            " con consecutivo " + eg.numero,
                                                                            estado = true
                                                                        };
                                                                        context.mov_contable.Add(crearMovContable);
                                                                        context.SaveChanges();
                                                                    }
                                                                }
                                                            }

                                                            #endregion

                                                            secuencia++;
                                                            //Cuentas valores

                                                            #region Cuentas valores

                                                            cuentas_valores buscar_cuentas_valores =
                                                                context.cuentas_valores.FirstOrDefault(x =>
                                                                    x.centro == parametro.centro &&
                                                                    x.cuenta == movNuevo.cuenta && x.nit == movNuevo.nit);
                                                            if (buscar_cuentas_valores != null)
                                                            {
                                                                buscar_cuentas_valores.debito +=
                                                                    Math.Round(movNuevo.debito);
                                                                buscar_cuentas_valores.credito +=
                                                                    Math.Round(movNuevo.credito);
                                                                buscar_cuentas_valores.debitoniff +=
                                                                    Math.Round(movNuevo.debitoniif);
                                                                buscar_cuentas_valores.creditoniff +=
                                                                    Math.Round(movNuevo.creditoniif);
                                                                context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                                                context.SaveChanges();
                                                            }
                                                            else
                                                            {
                                                                DateTime fechaHoy = DateTime.Now;
                                                                cuentas_valores crearCuentaValor = new cuentas_valores
                                                                {
                                                                    ano = fechaHoy.Year,
                                                                    mes = fechaHoy.Month,
                                                                    cuenta = movNuevo.cuenta,
                                                                    //crearCuentaValor.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 11 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.id_nombre_parametro == 12 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : movNuevo.centro : movNuevo.centro; ;
                                                                    centro = listaelementos[i].tipo_tarifa == 2
                                                                    ? parametro.id_nombre_parametro == 12
                                                                        ?
                                                                        listaelementos[i].centro_costo
                                                                        : parametro.centro
                                                                    : parametro.centro,

                                                                    nit = movNuevo.nit,
                                                                    debito = Math.Round(movNuevo.debito),
                                                                    credito = Math.Round(movNuevo.credito),
                                                                    debitoniff =
                                                                    Math.Round(movNuevo.debitoniif),
                                                                    creditoniff =
                                                                    Math.Round(movNuevo.creditoniif)
                                                                };
                                                                context.cuentas_valores.Add(crearCuentaValor);
                                                                context.SaveChanges();
                                                            }

                                                            #endregion

                                                            totalCreditos += Math.Round(movNuevo.credito);
                                                            totalDebitos += Math.Round(movNuevo.debito);
                                                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                            {
                                                                NumeroCuenta =
                                                                    "(" + buscarCuenta.cntpuc_numero + ")" +
                                                                    buscarCuenta.cntpuc_descp,
                                                                DescripcionParametro = descripcionParametro,
                                                                ValorDebito = movNuevo.debito,
                                                                ValorCredito = movNuevo.credito
                                                            });
                                                        }
                                                    }
                                                }

                                                #endregion
                                            } //fin de recorrer linea por linea

                                            #region validaciones para guardar

                                            if (Math.Round(totalDebitos) != Math.Round(totalCreditos))
                                            {
                                                TempData["mensaje_error"] =
                                                    "El documento no tiene los movimientos calculados correctamente, verifique el perfil del documento";

                                                dbTran.Rollback();
                                                listasliquidacion(modelo);
                                                BuscarFavoritos(menu);
                                                return View(modelo);
                                                //return RedirectToAction("Facturar", "FacturacionRepuestos", new { menu });
                                            }

                                            funciono = 1;

                                            #endregion

                                            if (funciono > 0)
                                            {
                                                context.SaveChanges();
                                                dbTran.Commit();
                                                ordentaller.estadoorden = estadofacturada;
                                                ordentaller.fecha_facturacion = DateTime.Now;
                                                context.Entry(ordentaller).State = EntityState.Modified;
                                                context.SaveChanges();
                                                TempData["mensaje"] = "Registro creado correctamente";
                                                DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
                                                doc.ActualizarConsecutivo(grupo.grupo, consecutivo);

                                                //return RedirectToAction("Facturar", "FacturacionRepuestos", new { menu });
                                                //detalle_formas_pago_orden dfp = context.detalle_formas_pago_orden.Where(d => d.idorden == modelo.idorden).OrderByDescending(d => d.id_detalle).FirstOrDefault();
                                                //if (dfp.pendiente == 0)
                                                //{
                                                //    ReciboCaja(modelo.idorden, id_encabezado);
                                                //    //return RedirectToAction("ReciboCaja", "CentralAtencion", new { menu });
                                                //}

                                            }

                                            //#endregions


                                        }
                                        else
                                        {
                                            TempData["mensaje_error"] = "no hay consecutivo";
                                        }
                                    }
                                    //cierre
                                    if (lista3 > 0 || lista4 > 0)
                                    {
                                        if (lista3 > 0)
                                        {
                                            RIrepuestos(modelo);
                                        }
                                        if (lista4 > 0)
                                        {
                                            RIoperaciones(modelo);
                                        }



                                    }
                                    if (lista5 > 0 || lista6 > 0)
                                    {
                                        PreFactura(modelo);
                                    }
                                }
                                else
                                {
                                    TempData["mensaje_error"] = "Lista vacia";
                                }


                            }
                            else
                            {
                                TempData["mensaje_error"] =
                                    "Error en liquidación de orden " + modelo.codigoentrada +
                                    " Mensaje: No existe perfil contable para el documento seleccionado y la bodega: " +
                                    bodegaconce.bodccs_nombre;
                            }
                        }
                        catch (DbEntityValidationException ex)
                        {
                            string mensaje = ex.Message;
                            dbTran.Rollback();
                            TempData["mensaje_error"] =
                                "Error en liquidación de orden " + modelo.codigoentrada + " Mensaje:" + mensaje;
                        }
                    }

                }
                else {
                    TempData["mensaje_error"] = "Error en liquidación de orden " + modelo.codigoentrada +
                                                " : Debe llenar todos los campos obligatorios, así como seleccionar los tipos de tarifa de las operaciones y los repuestos.";
                }

                listasliquidacion(modelo);
                BuscarFavoritos(menu);

                return RedirectToAction("Recibo", "CentralAtencion", new { modelo.idorden });
                //return RedirectToAction("crearFactura", "CentralAtencion", new { modelo.idorden });


            }
            return RedirectToAction("Login", "Login");

        }
        public ActionResult Recibo(int? idorden)
        {
            encab_documento encab = context.encab_documento.Where(d => d.orden_taller == idorden && d.tipo == 2034).OrderByDescending(d => d.idencabezado).FirstOrDefault();
            encab_documento encab2 = context.encab_documento.Where(d => d.orden_taller == idorden && d.tipo == 3080).OrderByDescending(d => d.idencabezado).FirstOrDefault();
            //TempData["factura_recibo"] = "Se registro exitosamente,  No. Factura" + encab2.numero + "  No. Recibo Caja" + encab.numero;
            return RedirectToAction("BrowserCajas", "CentralAtencion");
        }

        public void listas(int? idpedido)
        {
            int idclientepedido = 0;
            int bodegapedido = 0;
            int idvendedor = 0;

            //verifico si el pedido no es nulo si corresponde a un pedido
            vpedido existepedido = context.vpedido.Where(d => d.id == idpedido).FirstOrDefault();

            if (existepedido != null)
            {
                idclientepedido = existepedido.nit != null ? existepedido.nit.Value : 0;
                bodegapedido = existepedido.bodega;
                idvendedor = existepedido.vendedor != null ? existepedido.vendedor.Value : 0;
                var pedido = (from p in context.vpedido
                              join m in context.modelo_vehiculo
                                  on p.modelo equals m.modvh_codigo
                              where p.nit == existepedido.nit
                              select new
                              {
                                  p.id,
                                  p.numero,
                                  carro = m.modvh_nombre
                              }).ToList();
                var pedidos = pedido.Select(d => new
                {
                    d.id,
                    numero = "(" + d.numero + ") - " + d.carro
                }).ToList();
                ViewBag.id_pedido_vehiculo = new SelectList(pedidos, "id", "numero", existepedido.id);
            }
            else
            {
                ViewBag.id_pedido_vehiculo = new SelectList(context.vpedido.Take(0).ToList(), "id", "numero");
            }

            ViewBag.tipo = new SelectList(context.tp_doc_registros.Where(x => x.tipo == 16), "tpdoc_id",
                "tpdoc_nombre");

            var list = (from t in context.icb_terceros
                        join tp in context.tercero_cliente
                            on t.tercero_id equals tp.tercero_id
                        select new
                        {
                            t.tercero_id,
                            t.prinom_tercero,
                            t.apellido_tercero,
                            t.segnom_tercero,
                            t.segapellido_tercero,
                            t.doc_tercero,
                            t.razon_social
                        }).ToList();

            List<SelectListItem> lista = new List<SelectListItem>();
            foreach (var item in list)
            {
                lista.Add(new SelectListItem
                {
                    Text = item.doc_tercero + " " + item.prinom_tercero + ' ' + item.segnom_tercero + ' ' +
                           item.apellido_tercero + ' ' + item.segnom_tercero + ' ' + item.razon_social,
                    Value = item.tercero_id.ToString()
                });
            }

            ViewBag.nit = new SelectList(lista, "Value", "Text", idclientepedido);
            ViewBag.fpago = new SelectList(context.tipopagorecibido, "id", "pago");

            var buscarTipoDocumento = (from tipoDocumento in context.tp_doc_registros
                                       select new
                                       {
                                           tipoDocumento.sw,
                                           tipoDocumento.tpdoc_id,
                                           nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                           tipoDocumento.tipo
                                       }).ToList();
            icb_sysparameter swND = context.icb_sysparameter.Where(d => d.syspar_cod == "P128").FirstOrDefault();
            int swND2 = swND != null ? Convert.ToInt32(swND.syspar_value) : 1013;
            icb_sysparameter swF = context.icb_sysparameter.Where(d => d.syspar_cod == "P129").FirstOrDefault();
            int swF2 = swF != null ? Convert.ToInt32(swF.syspar_value) : 3;
            ViewBag.tipo_documentoFiltro = new SelectList(buscarTipoDocumento.Where(x => x.sw == swF2 || x.sw == swND2),
                "tpdoc_id", "nombre",
                buscarTipoDocumento.Where(x => x.sw == swF2 || x.sw == swND2).Select(d => d.tpdoc_id).FirstOrDefault());

            ViewBag.moneda = new SelectList(context.monedas, "moneda", "descripcion");
            ViewBag.tasa = new SelectList(context.moneda_conversion, "id", "conversion");
            ViewBag.motivocompra = new SelectList(context.motcompra, "id", "Motivo");

            ViewBag.condicion_pago = context.fpago_tercero;

            List<users> asesores = context.users.Where(x => x.rol_id == 4 || x.rol_id == 6).ToList();
            List<SelectListItem> listaAsesores = new List<SelectListItem>();
            foreach (users asesor in asesores)
            {
                listaAsesores.Add(new SelectListItem
                { Value = asesor.user_id.ToString(), Text = asesor.user_nombre + " " + asesor.user_apellido });
            }

            ViewBag.vendedor = new SelectList(listaAsesores, "Value", "Text", idvendedor);

            ViewBag.parametroEfectivo =
                context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P71").syspar_value;
            ViewBag.parametroCredito = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P72").syspar_value;

            encab_documento buscarUltimoRecibo = context.encab_documento.OrderByDescending(x => x.idencabezado).FirstOrDefault();
            ViewBag.numReciboCreado = buscarUltimoRecibo != null ? buscarUltimoRecibo.numero : 0;
        }

        public ActionResult ReciboCaja(int? idorden, int id_encabezado)
        {
            encab_documento encab = context.encab_documento.Where(d => d.orden_taller == idorden && d.idencabezado == id_encabezado).OrderByDescending(d => d.idencabezado).FirstOrDefault();
            if (ModelState.IsValid)
            {
                
                using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                {
                    //tipo de factura factura vehiculo
                    icb_sysparameter fac = context.icb_sysparameter.Where(d => d.syspar_cod == "P101").FirstOrDefault();
                    int ventas = fac != null ? Convert.ToInt32(fac.syspar_value) : 4;

                    

                    var lista_pagos2 = context.detalle_formas_pago_orden.Where(d => d.idorden == idorden).ToList();

                    grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == encab.tipo && x.bodega_id == encab.bodega);
                    if (grupo != null)
                    {
                        try
                        {
                            DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                            long consecutivo = doc.BuscarConsecutivo(grupo.grupo);
                            decimal total = encab.valor_total;//revisar
                            //Encabezado documento
                            encab_documento encabezado = new encab_documento
                            {
                                tipo = 2034,
                                numero = consecutivo,
                                nit = encab.nit,
                                fecha = DateTime.Now,
                                valor_total = Convert.ToDecimal(total, miCultura),
                                vendedor = encab.vendedor,
                                documento = encab.documento,
                                prefijo = encab.prefijo,
                                valor_mercancia = Convert.ToDecimal(encab.costo, miCultura),
                                nota1 = encab.nota1,
                                valor_aplicado = 0,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                fec_creacion = DateTime.Now,
                                perfilcontable = 2033,
                                bodega = encab.bodega,
                                orden_taller = encab.orden_taller,
                                anticipo = false,
                            };

                            context.encab_documento.Add(encabezado);
                            context.SaveChanges();
                            int cruce = encabezado.idencabezado;
                            long numcruce = encabezado.numero;

                            var buscar = lista_pagos2.Select(x => new
                            {

                                idtencabezado = cruce,
                                tercero = encabezado.nit,
                                fecha = DateTime.Now,
                                forma_pago = x.idformas_pago,
                                valor = Convert.ToDecimal(x.valor, miCultura),
                                documento = x.vaucher,
                                //cuenta_banco = x.numero_cuenta,
                                //banco=x.banco,
                                //notas = Request["observaciones" + i]

                            });

                            foreach (var item in buscar)
                            {
                                documentos_pago dpago = new documentos_pago
                                {

                                    idtencabezado = item.idtencabezado,
                                    tercero = item.tercero,
                                    fecha = item.fecha,
                                    forma_pago = Convert.ToInt32(item.forma_pago),
                                    valor = item.valor,
                                    documento = item.documento,
                                    //cuenta_banco=item.cuenta_banco,

                                };
                                //string banco = item.banco.ToString();
                                //if (!string.IsNullOrEmpty(banco))
                                //{
                                //    dpago.banco = Convert.ToInt32(banco);
                                //}
                                string id_pedido_vehiculo = encab.id_pedido_vehiculo.ToString();
                                if (!string.IsNullOrEmpty(id_pedido_vehiculo))//revisar
                                {
                                    dpago.pedido = Convert.ToInt32(id_pedido_vehiculo);//revisar
                                }
                                context.documentos_pago.Add(dpago);
                                context.SaveChanges();
                            }

                            string pedido = encab.id_pedido_vehiculo.ToString();//revisar
                            Convert.ToString(pedido);
                            decimal valorapli = 0;

                            /************************************************************ Guardo informacion si selecciona factura ************************************************************/
                            if (pedido != "" && pedido != null)
                            {
                                int tipo = encab.tipo;
                                long numero = encab.numero;
                                int id = encab.idencabezado;
                                decimal valor = encab.valor_total;


                                encab_documento factura = context.encab_documento.Where(d => d.idencabezado == id && d.tipo == tipo)
                                    .FirstOrDefault();

                                if (valor != 0)
                                {

                                    int valoraplicado = Convert.ToInt32(factura.valor_aplicado);

                                    decimal nuevovalor = Convert.ToDecimal(valoraplicado, miCultura) + valor;
                                    valorapli = valorapli + valor;
                                    factura.valor_aplicado = Convert.ToDecimal(nuevovalor, miCultura);
                                    context.Entry(factura).State = EntityState.Modified;

                                    cruce_documentos cd = new cruce_documentos
                                    {
                                        idtipo = 2034,
                                        numero = numcruce,
                                        id_encab_aplica = id,
                                        id_encabezado = cruce,
                                        idtipoaplica = Convert.ToInt32(tipo),
                                        numeroaplica = Convert.ToInt32(numero),
                                        valor = Convert.ToDecimal(valor, miCultura),
                                        fecha = DateTime.Now,
                                        fechacruce = DateTime.Now,
                                        userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                                    };
                                    context.cruce_documentos.Add(cd);
                                }

                                int guardar = context.SaveChanges();
                            }
                            else
                            {
                                if (pedido == "")
                                {
                                    //valor aplicado tengo que actualizarlo en la(s) factura(s) a la(s) que estoy generando el recibo de caja
                                    //buscas la factura
                                    int tipo = encab.tipo;
                                    long numero = encab.numero;
                                    int id = encab.idencabezado;
                                    decimal valor = encab.valor_total;

                                    encab_documento factura = context.encab_documento.Where(d => d.idencabezado == id && d.tipo == tipo)
                                        .FirstOrDefault();

                                    if (valor != 0)
                                    {
                                        int valoraplicado = Convert.ToInt32(factura.valor_aplicado);

                                        decimal nuevovalor = Convert.ToDecimal(valoraplicado, miCultura) + valor;
                                        valorapli = valorapli + valor;

                                        factura.valor_aplicado = Convert.ToDecimal(nuevovalor, miCultura);
                                        context.Entry(factura).State = EntityState.Modified;

                                        // si al crear la nota credito se selecciono factura se debe guardar el registro tambien en cruce documentos, de lo contrario NO
                                        //Cruce documentos
                                        cruce_documentos cd = new cruce_documentos
                                        {
                                            idtipo = 2034,
                                            numero = numcruce,
                                            id_encab_aplica = id,
                                            id_encabezado = cruce,
                                            idtipoaplica = Convert.ToInt32(tipo),
                                            numeroaplica = Convert.ToInt32(numero),
                                            valor = Convert.ToDecimal(valor, miCultura),
                                            fecha = DateTime.Now,
                                            fechacruce = DateTime.Now,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"])
                                        };
                                        context.cruce_documentos.Add(cd);
                                    }

                                    int guardar = context.SaveChanges();
                                }
                                else
                                {

                                }
                            }
                            encabezado.valor_aplicado = valorapli;
                            context.SaveChanges();

                            //movimiento contable
                            //buscamos en perfil cuenta documento, por medio del perfil seleccionado

                            var parametrosCuentasVerificarRecibo = (from perfil in context.perfil_cuentas_documento
                                                              join nombreParametro in context.paramcontablenombres
                                                                  on perfil.id_nombre_parametro equals nombreParametro.id
                                                              join cuenta in context.cuenta_puc
                                                                  on perfil.cuenta equals cuenta.cntpuc_id
                                                              where perfil.id_perfil == 2033 && perfil.id_nombre_parametro == 16
                                                                    select new
                                                              {
                                                                  perfil.id,
                                                                  perfil.id_nombre_parametro,
                                                                  perfil.cuenta,
                                                                  perfil.centro,
                                                                  perfil.id_perfil,
                                                                  nombreParametro.descripcion_parametro,
                                                                  cuenta.cntpuc_numero
                                                              }).ToList();
                            //

                            var parametrosCuentasVerificarFactura = (from perfil in context.perfil_cuentas_documento
                                                              join nombreParametro in context.paramcontablenombres
                                                                  on perfil.id_nombre_parametro equals nombreParametro.id
                                                              join cuenta in context.cuenta_puc
                                                                  on perfil.cuenta equals cuenta.cntpuc_id
                                                              where perfil.id_perfil == encab.perfilcontable && perfil.id_nombre_parametro==10
                                                                     select new
                                                              {
                                                                  perfil.id,
                                                                  perfil.id_nombre_parametro,
                                                                  perfil.cuenta,
                                                                  perfil.centro,
                                                                  perfil.id_perfil,
                                                                  nombreParametro.descripcion_parametro,
                                                                  cuenta.cntpuc_numero
                                                              }).ToList();

                            int secuencia = 1;
                            decimal totalDebitos = 0;
                            decimal totalCreditos = 0;

                            List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();
                            List<cuentas_valores> ids_cuentas_valores = new List<cuentas_valores>();
                            centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                            int idCentroCero = centroValorCero != null ? Convert.ToInt32(centroValorCero.centcst_id) : 0;
                            foreach (var parametro in parametrosCuentasVerificarRecibo)
                            {
                                string descripcionParametro = context.paramcontablenombres
                                    .FirstOrDefault(x => x.id == parametro.id_nombre_parametro).descripcion_parametro;
                                cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);

                                if (buscarCuenta != null)
                                {
                                    if (parametro.id_nombre_parametro == 2 && Convert.ToDecimal(encab.iva, miCultura) != 0
                                        || parametro.id_nombre_parametro == 3 && Convert.ToDecimal(encab.retencion, miCultura) != 0
                                        || parametro.id_nombre_parametro == 4 && Convert.ToDecimal(encab.retencion_iva, miCultura) != 0
                                        || parametro.id_nombre_parametro == 5 && Convert.ToDecimal(encab.retencion_ica, miCultura) != 0
                                        || parametro.id_nombre_parametro == 10 || parametro.id_nombre_parametro == 11
                                        || parametro.id_nombre_parametro == 12 || parametro.id_nombre_parametro == 16
                                        || parametro.id_nombre_parametro == 20
                                        )
                                    {
                                        mov_contable movNuevo = new mov_contable
                                        {
                                            id_encab = cruce,
                                            seq = secuencia,
                                            idparametronombre = parametro.id_nombre_parametro,
                                            cuenta = parametro.cuenta,
                                            centro = parametro.centro,
                                            fec = DateTime.Now,
                                            fec_creacion = DateTime.Now,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            detalle = encab.nota1
                                        };

                                        cuenta_puc info = context.cuenta_puc.Where(i => i.cntpuc_id == parametro.cuenta)
                                            .FirstOrDefault();

                                        if (info.tercero)
                                        {
                                            movNuevo.nit = encab.nit;
                                        }
                                        else
                                        {
                                            icb_terceros tercero = context.icb_terceros.Where(t => t.doc_tercero == "0")
                                                .FirstOrDefault();
                                            movNuevo.nit = tercero.tercero_id;
                                        }

                                        // las siguientes validaciones se hacen dependiendo de la cuenta que esta moviendo la nota credito, para guardar la informacion acorde

                                        if (parametro.id_nombre_parametro == 10)
                                        {

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(total, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = encab.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(total, miCultura);
                                                movNuevo.debito = 0;

                                                movNuevo.creditoniif = Convert.ToDecimal(total, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(total, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(total, miCultura);
                                                movNuevo.debito = 0;
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 16)
                                        {

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(total, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = encab.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(total, miCultura);

                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(total, miCultura);
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(total, miCultura);
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(total, miCultura);
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 2)
                                        {

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(total, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = encab.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(encab.iva, miCultura);
                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(encab.iva, miCultura);
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(encab.iva, miCultura);
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(encab.iva, miCultura);
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 20)
                                        {

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(total, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = encab.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(encab.iva, miCultura);
                                                movNuevo.credito += movNuevo.credito = Convert.ToDecimal(total, miCultura);


                                                movNuevo.creditoniif = Convert.ToDecimal(encab.iva, miCultura);
                                                movNuevo.creditoniif += movNuevo.creditoniif = Convert.ToDecimal(total, miCultura);
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(encab.iva, miCultura);
                                                movNuevo.creditoniif += movNuevo.creditoniif = Convert.ToDecimal(total, miCultura);
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(encab.iva, miCultura);
                                                movNuevo.creditoniif += movNuevo.creditoniif = Convert.ToDecimal(total, miCultura);
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 11)
                                        {
                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(total, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = encab.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(total, miCultura);

                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(total, miCultura);
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(total, miCultura);
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(total, miCultura);
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 12)
                                        {
                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(total, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = encab.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(total, miCultura);
                                                movNuevo.debito = 0;

                                                movNuevo.creditoniif = Convert.ToDecimal(total, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(total, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(total, miCultura);
                                                movNuevo.debito = 0;
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 3)
                                        {

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(total, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = encab.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(encab.retencion, miCultura);
                                                movNuevo.debito = 0;

                                                movNuevo.creditoniif = Convert.ToDecimal(encab.retencion, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(encab.retencion, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(encab.retencion, miCultura);
                                                movNuevo.debito = 0;
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 4)
                                        {

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(encab.iva, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = encab.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(encab.retencion_iva, miCultura);
                                                movNuevo.debito = 0;

                                                movNuevo.creditoniif = Convert.ToDecimal(encab.retencion_iva, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(encab.retencion_iva, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(encab.retencion_iva, miCultura);
                                                movNuevo.debito = 0;
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 5)
                                        {

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(total, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = encab.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(encab.retencion_ica, miCultura);
                                                movNuevo.debito = 0;
                                                movNuevo.creditoniif = Convert.ToDecimal(encab.retencion_ica, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(encab.retencion_ica, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(encab.retencion_ica, miCultura);
                                                movNuevo.debito = 0;
                                            }
                                        }

                                        secuencia++;

                                        //Cuentas valores
                                        cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                            x.centro == parametro.centro && x.cuenta == parametro.cuenta &&
                                            x.nit == movNuevo.nit);
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
                                            DateTime fechaHoy = DateTime.Now;
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

                                        totalCreditos += movNuevo.credito;
                                        totalDebitos += movNuevo.debito;

                                        listaDescuadrados.Add(new DocumentoDescuadradoModel
                                        {
                                            NumeroCuenta = "(" + info.cntpuc_numero + ")" + info.cntpuc_descp,
                                            DescripcionParametro = descripcionParametro,
                                            ValorDebito = movNuevo.debito,
                                            ValorCredito = movNuevo.credito
                                        });
                                    }
                                }
                            }

                            foreach (var parametro in parametrosCuentasVerificarFactura)
                            {
                                string descripcionParametro = context.paramcontablenombres
                                    .FirstOrDefault(x => x.id == parametro.id_nombre_parametro).descripcion_parametro;
                                cuenta_puc buscarCuenta = context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);

                                if (buscarCuenta != null)
                                {
                                    if (parametro.id_nombre_parametro == 2 && Convert.ToDecimal(encab.iva, miCultura) != 0
                                        || parametro.id_nombre_parametro == 3 && Convert.ToDecimal(encab.retencion, miCultura) != 0
                                        || parametro.id_nombre_parametro == 4 && Convert.ToDecimal(encab.retencion_iva, miCultura) != 0
                                        || parametro.id_nombre_parametro == 5 && Convert.ToDecimal(encab.retencion_ica, miCultura) != 0
                                        || parametro.id_nombre_parametro == 10 || parametro.id_nombre_parametro == 11
                                        || parametro.id_nombre_parametro == 12 || parametro.id_nombre_parametro == 16
                                        || parametro.id_nombre_parametro == 20
                                        )
                                    {
                                        mov_contable movNuevo = new mov_contable
                                        {
                                            id_encab = cruce,
                                            seq = secuencia,
                                            idparametronombre = parametro.id_nombre_parametro,
                                            cuenta = parametro.cuenta,
                                            centro = parametro.centro,
                                            fec = DateTime.Now,
                                            fec_creacion = DateTime.Now,
                                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                            detalle = encab.nota1
                                        };

                                        cuenta_puc info = context.cuenta_puc.Where(i => i.cntpuc_id == parametro.cuenta)
                                            .FirstOrDefault();

                                        if (info.tercero)
                                        {
                                            movNuevo.nit = encab.nit;
                                        }
                                        else
                                        {
                                            icb_terceros tercero = context.icb_terceros.Where(t => t.doc_tercero == "0")
                                                .FirstOrDefault();
                                            movNuevo.nit = tercero.tercero_id;
                                        }

                                        // las siguientes validaciones se hacen dependiendo de la cuenta que esta moviendo la nota credito, para guardar la informacion acorde

                                        if (parametro.id_nombre_parametro == 10)
                                        {

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(total, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = encab.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(total, miCultura);
                                                movNuevo.debito = 0;

                                                movNuevo.creditoniif = Convert.ToDecimal(total, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(total, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(total, miCultura);
                                                movNuevo.debito = 0;
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 16)
                                        {

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(total, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = encab.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(total, miCultura);

                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(total, miCultura);
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(total, miCultura);
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(total, miCultura);
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 2)
                                        {

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(total, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = encab.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(encab.iva, miCultura);
                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(encab.iva, miCultura);
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(encab.iva, miCultura);
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(encab.iva, miCultura);
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 20)
                                        {

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(total, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = encab.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(encab.iva, miCultura);
                                                movNuevo.credito += movNuevo.credito = Convert.ToDecimal(total, miCultura);


                                                movNuevo.creditoniif = Convert.ToDecimal(encab.iva, miCultura);
                                                movNuevo.creditoniif += movNuevo.creditoniif = Convert.ToDecimal(total, miCultura);
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(encab.iva, miCultura);
                                                movNuevo.creditoniif += movNuevo.creditoniif = Convert.ToDecimal(total, miCultura);
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(encab.iva, miCultura);
                                                movNuevo.creditoniif += movNuevo.creditoniif = Convert.ToDecimal(total, miCultura);
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 11)
                                        {
                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(total, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = encab.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(total, miCultura);

                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(total, miCultura);
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = 0;
                                                movNuevo.debitoniif = Convert.ToDecimal(total, miCultura);
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = 0;
                                                movNuevo.debito = Convert.ToDecimal(total, miCultura);
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 12)
                                        {
                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(total, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = encab.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(total, miCultura);
                                                movNuevo.debito = 0;

                                                movNuevo.creditoniif = Convert.ToDecimal(total, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(total, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(total, miCultura);
                                                movNuevo.debito = 0;
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 3)
                                        {

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(total, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = encab.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(encab.retencion, miCultura);
                                                movNuevo.debito = 0;

                                                movNuevo.creditoniif = Convert.ToDecimal(encab.retencion, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(encab.retencion, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(encab.retencion, miCultura);
                                                movNuevo.debito = 0;
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 4)
                                        {

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(encab.iva, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = encab.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(encab.retencion_iva, miCultura);
                                                movNuevo.debito = 0;

                                                movNuevo.creditoniif = Convert.ToDecimal(encab.retencion_iva, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(encab.retencion_iva, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(encab.retencion_iva, miCultura);
                                                movNuevo.debito = 0;
                                            }
                                        }

                                        if (parametro.id_nombre_parametro == 5)
                                        {

                                            if (info.manejabase)
                                            {
                                                movNuevo.basecontable = Convert.ToDecimal(total, miCultura);
                                            }
                                            else
                                            {
                                                movNuevo.basecontable = 0;
                                            }

                                            if (info.documeto)
                                            {
                                                movNuevo.documento = encab.documento;
                                            }

                                            if (buscarCuenta.concepniff == 1)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(encab.retencion_ica, miCultura);
                                                movNuevo.debito = 0;
                                                movNuevo.creditoniif = Convert.ToDecimal(encab.retencion_ica, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 4)
                                            {
                                                movNuevo.creditoniif = Convert.ToDecimal(encab.retencion_ica, miCultura);
                                                movNuevo.debitoniif = 0;
                                            }

                                            if (buscarCuenta.concepniff == 5)
                                            {
                                                movNuevo.credito = Convert.ToDecimal(encab.retencion_ica, miCultura);
                                                movNuevo.debito = 0;
                                            }
                                        }

                                        secuencia++;

                                        //Cuentas valores
                                        cuentas_valores buscar_cuentas_valores = context.cuentas_valores.FirstOrDefault(x =>
                                            x.centro == parametro.centro && x.cuenta == parametro.cuenta &&
                                            x.nit == movNuevo.nit);
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
                                            DateTime fechaHoy = DateTime.Now;
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

                                        totalCreditos += movNuevo.credito;
                                        totalDebitos += movNuevo.debito;

                                        listaDescuadrados.Add(new DocumentoDescuadradoModel
                                        {
                                            NumeroCuenta = "(" + info.cntpuc_numero + ")" + info.cntpuc_descp,
                                            DescripcionParametro = descripcionParametro,
                                            ValorDebito = movNuevo.debito,
                                            ValorCredito = movNuevo.credito
                                        });
                                    }
                                }
                            }

                            if (totalDebitos != totalCreditos)
                            {
                                TempData["mensaje_error"] =
                                    "El documento no tiene los movimientos calculados correctamente, verifique el perfil del documento";

                                ViewBag.documentoSeleccionado = encabezado.tipo;
                                ViewBag.bodegaSeleccionado = encabezado.bodega;
                                ViewBag.perfilSeleccionado = encabezado.perfilcontable;

                                ViewBag.documentoDescuadrado = listaDescuadrados;
                                ViewBag.calculoDebito = totalDebitos;
                                ViewBag.calculoCredito = totalCreditos;
                                if (!string.IsNullOrWhiteSpace(pedido))
                                {
                                    int idpedidoint = Convert.ToInt32(pedido);
                                    listas(idpedidoint);
                                }
                                else
                                {
                                    listas(null);
                                }

                                return RedirectToAction("LiquidacionCajaOT", "CentralAtencion");
                            }

                            try
                            {
                                context.SaveChanges();
                                TempData["mensaje"] = "registro creado correctamente";
                                DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
                                doc.ActualizarConsecutivo(grupo.grupo, consecutivo);
                                dbTran.Commit();
                                return RedirectToAction("CarteraFacturacion", "CarteraPorEdades");
                            }
                            catch (DbEntityValidationException dbEx)
                            {
                                Exception raise = dbEx;
                                foreach (DbEntityValidationResult validationErrors in dbEx.EntityValidationErrors)
                                {
                                    foreach (DbValidationError validationError in validationErrors.ValidationErrors)
                                    {
                                        string message = string.Format("{0}:{1}",
                                        validationErrors.Entry.Entity,
                                        validationError.ErrorMessage);
                                        raise = new InvalidOperationException(message, raise);
                                    }
                                }

                                throw raise;
                            }
                        }
                        catch (Exception ex)
                        {
                            Exception mensaje = ex;
                            dbTran.Rollback();
                        }
                    }
                    else {
                        TempData["mensaje_error"] = "no hay consecutivo";
                    }
                    
                }
            }
            else
            {
                TempData["mensaje_error"] = "No fue posible guardar el registro, por favor valide";
                List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();

             }

            return RedirectToAction("BrowserCajas", "CentralAtencion");
        }

        [HttpGet]
        public ActionResult LiquidacionOT(int? id, int? menu)
        {

            if (id != null)
            {
                var otaller = context.tencabezaorden.Where(d => d.id == id).FirstOrDefault();
                var operaciones = context.tdetallemanoobraot.Where(d => d.idorden == otaller.id && d.tipotarifa == null).Count() > 0 ? "Si" : "No";

                var repuestos = context.tsolicitudrepuestosot.Where(d => d.idorden == id).FirstOrDefault();
                var tiporepuestos = context.tsolicitudrepuestosot.Where(d => d.idorden == otaller.id && d.tdetallerepuestosot.tipotarifa == null).Count() > 0 ? "Si" : "No";

                //var tipofactura=context.perfil_contable_documento.Where

                var cantidadRepuestos = context.tsolicitudrepuestosot.Where(e => e.idorden == id ).ToList();
                var resultado = cantidadRepuestos.Where(x => x.canttraslado == 0).Count() > 0 ? "Si" : "No";

                ViewBag.insumo =
           new SelectList(context.Tinsumo.Select(x => new { x.id, descripcion = x.codigo + " | " + x.descripcion }), "id", "descripcion");

                ViewBag.tarifainsumo =
                new SelectList(context.ttipostarifa.Select(x => new { x.Descripcion, x.id }), "id", "descripcion");


                ViewBag.tarifacombo =
               new SelectList(context.ttipostarifa.Select(x => new { x.Descripcion, x.id }), "id", "descripcion");

                icb_sysparameter idplanmparameter = context.icb_sysparameter.Where(d => d.syspar_cod == "P156").FirstOrDefault();
                int idplanm = idplanmparameter != null ? Convert.ToInt32(idplanmparameter.syspar_value) : 8;



                ViewBag.comboot = new SelectList(context.ttempario.Where(x => x.tipooperacion == idplanm).Select(x => new { x.operacion, x.codigo }), "codigo", "operacion");



                ViewBag.Permiso = operaciones;
                ViewBag.Permiso2 = tiporepuestos;
            }
                if (Session["user_usuarioid"] != null)
            {
                //valido si la orden existe
                tencabezaorden ordentaller = context.tencabezaorden.Where(d => d.id == id).FirstOrDefault();

                var listaFP = (from f in context.formas_pago
                               join b in context.bancos
                               on f.idbanco equals b.id
                               orderby b.Descripcion
                               select new
                               {
                                   f.id,
                                   nombre = b.Descripcion + " " + f.formapago + " (" + b.numero_cuenta + ")"
                               }).ToList();

                List<SelectListItem> list = new List<SelectListItem>();
                foreach (var item in listaFP)
                {
                    list.Add(new SelectListItem
                    {
                        Text = item.nombre,
                        Value = item.id.ToString()
                    });
                }
                ViewBag.FormaPago = list;


                var data = (from vh in context.icb_vehiculo
                            join t in context.tencabezaorden
                            on vh.plan_mayor equals t.placa
                            where vh.plac_vh == ordentaller.placa || vh.plan_mayor == ordentaller.placa && t.codigoentrada == ordentaller.codigoentrada
                            select new
                            {
                                vh.garantia
                            }).FirstOrDefault();

                var garantia = data.garantia;

                ViewBag.Garantia = garantia;


                var listaTipoTarifa = (from e in context.rtipocliente
                                       select new
                                       {
                                           id = e.id,
                                           nombre = e.descripcion
                                       }).ToList();

                List<SelectListItem> lista = new List<SelectListItem>();
                foreach (var item in listaTipoTarifa)
                {
                    lista.Add(new SelectListItem
                    {
                        Text = item.nombre,
                        Value = item.id.ToString()
                    });
                }
                ViewBag.tarifa = lista;

                if (ordentaller != null)
                {
                    //parametro de sistema liquidacion
                    icb_sysparameter liqui = context.icb_sysparameter.Where(d => d.syspar_cod == "P111").FirstOrDefault();
                    int liquidada = liqui != null ? Convert.ToInt32(liqui.syspar_value) : 8;
                    //busco si la orden de taller ya fue liquidada
                    if (ordentaller.estadoorden != 8 && ordentaller.fecha_liquidacion == null)
                    {
                        string formapago = "NO TIENE, DEBE ACTUALIZAR DATOS DEL CLIENTE";
                        tercero_cliente tercero_cli = context.tercero_cliente.Where(d => d.tercero_id == ordentaller.tercero)
                            .FirstOrDefault();
                        if (tercero_cli != null)
                        {
                            formapago = (from c in context.tercero_cliente
                                         join f in context.fpago_tercero
                                             on c.cod_pago_id equals f.fpago_id
                                         where c.tercero_id == ordentaller.tercero
                                         select
                                             f.fpago_nombre
                                ).FirstOrDefault();
                        }
                        //llenado de objeto formulario de orden de taller
                        //datos del formulario
                        formularioliquidacionOT formuorden = new formularioliquidacionOT
                        {
                            nombre = !string.IsNullOrWhiteSpace(ordentaller.icb_terceros.razon_social)
                                ? ordentaller.icb_terceros.razon_social
                                : ordentaller.icb_terceros.prinom_tercero +
                                  (string.IsNullOrWhiteSpace(ordentaller.icb_terceros.segnom_tercero)
                                      ? " " + ordentaller.icb_terceros.segnom_tercero
                                      : "") + " " + ordentaller.icb_terceros.apellido_tercero +
                                  (string.IsNullOrWhiteSpace(ordentaller.icb_terceros.segapellido_tercero)
                                      ? " " + ordentaller.icb_terceros.segapellido_tercero
                                      : ""),
                            documento = ordentaller.icb_terceros.doc_tercero,
                            telefono = ordentaller.icb_terceros.telf_tercero,
                            celular = !string.IsNullOrWhiteSpace(ordentaller.icb_terceros.celular_tercero)
                                ? ordentaller.icb_terceros.celular_tercero
                                : "",
                            correo = ordentaller.icb_terceros.email_tercero,
                            direccion = ordentaller.icb_terceros.terceros_direcciones.Count() > 0
                                ? ordentaller.icb_terceros.terceros_direcciones.Select(e => e.direccion)
                                    .FirstOrDefault()
                                : "",
                            //datos del vehiculo
                            placa = !string.IsNullOrWhiteSpace(ordentaller.icb_vehiculo.plac_vh)
                                ? ordentaller.icb_vehiculo.plac_vh
                                : ordentaller.icb_vehiculo.plan_mayor,
                            marca = ordentaller.icb_vehiculo.marca_vehiculo.marcvh_nombre,
                            modelo = ordentaller.icb_vehiculo.modelo_vehiculo.modvh_nombre,
                            asesor = ordentaller.users.user_nombre + " " + ordentaller.users.user_apellido,
                            forma_pago = formapago,
                            garantia = ordentaller.icb_vehiculo.garantia,
                            //campos generales de la OT
                            codigoentrada = ordentaller.codigoentrada,
                            idorden = ordentaller.id,
                            razon_ingreso = ordentaller.razoningreso,
                            razon_ingreso2 = ordentaller.razoningreso2,
                            tecnico = ordentaller.idtecnico,
                            id_cliente = ordentaller.tercero,
                            bodega = ordentaller.bodega,
                            kilometraje = ordentaller.kilometraje.ToString("N0", new CultureInfo("is-IS")),
                            serie = ordentaller.icb_vehiculo.vin,
                            motor = ordentaller.icb_vehiculo.nummot_vh,
                            color = ordentaller.icb_vehiculo.color_vehiculo.colvh_nombre,
                            fecha_fin_garantia = ordentaller.icb_vehiculo.fecha_fin_garantia != null
                                ? ordentaller.icb_vehiculo.fecha_fin_garantia.Value.ToString("yyyy/MM/dd",
                                    new CultureInfo("en-US"))
                                : "",
                            fecha_venta = ordentaller.icb_vehiculo.fecha_venta != null
                                ? ordentaller.icb_vehiculo.fecha_venta.Value.ToString("yyyy/MM/dd",
                                    new CultureInfo("en-US"))
                                : "",
                            aseguradora = ordentaller.aseguradora,
                            garantia_falla = ordentaller.garantia_falla,
                            garantia_causa = ordentaller.garantia_causa,
                            garantia_solucion = ordentaller.garantia_solucion,
                            poliza = ordentaller.poliza,
                            deducible = ordentaller.deducible != null
                                ? ordentaller.deducible.Value.ToString("N0", new CultureInfo("is-IS"))
                                : "",
                            siniestro = ordentaller.siniestro,
                            minimo = ordentaller.minimo != null
                                ? ordentaller.minimo.Value.ToString("N0", new CultureInfo("is-IS"))
                                : "",
                            fecha_soat = ordentaller.fecha_soat != null
                                ? ordentaller.fecha_soat.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                                : "",
                            numero_soat = !string.IsNullOrWhiteSpace(ordentaller.numero_soat)
                                ? ordentaller.numero_soat
                                : ""

                        };
                        listasliquidacion(formuorden);
                        BuscarFavoritos(menu);
                        return View(formuorden);
                    }

                    TempData["mensaje_error"] =
                        "La orden de taller N° " + ordentaller.codigoentrada + " ya ha sido liquidada";
                    return RedirectToAction("index", new { menu });
                }

                return RedirectToAction("index", "CentralAtencion", new { menu });
            }

            return RedirectToAction("Login", "Login");

        }

        [HttpPost]
        public ActionResult LiquidacionOT(formularioliquidacionOT modelo, int? menu)
        {

            if (Session["user_usuarioid"] != null)
            {
                int cajas = 0;
                int guardar = 0;
                //valido si el modelo es válido
                if (ModelState.IsValid)
                {
                    //parametro orden de taller liquidada
                    icb_sysparameter param1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P151").FirstOrDefault();
                    int estadofacturada = param1 != null ? Convert.ToInt32(param1.syspar_value) : 31;

                    //parametro de sistema accesorios
                    icb_sysparameter param2 = context.icb_sysparameter.Where(d => d.syspar_cod == "P116").FirstOrDefault();
                    int accesorios = param2 != null ? Convert.ToInt32(param2.syspar_value) : 6;


                    //razon ingreso por garantia 
                    icb_sysparameter paramgarane = context.icb_sysparameter.Where(d => d.syspar_cod == "P87").FirstOrDefault();
                    int razongarantia = paramgarane != null ? Convert.ToInt32(paramgarane.syspar_value) : 7;
                    //int? EstadoOTdos = null;
               

                    var tecnicos = context.ttecnicos.Where(d => d.estado && d.users.user_estado).Select(d =>
                    new { id = d.users.user_id, nombre = d.users.user_nombre + " " + d.users.user_apellido }).ToList();
                    ViewBag.tecnico = new SelectList(tecnicos, "id", "nombre", modelo.tecnico);

                    var razones = context.trazonesingreso.Where(d => d.estado).Select(d => new { d.id, nombre = d.razoz_ingreso }).ToList();

                    ViewBag.razon_ingreso = new SelectList(razones, "id", "nombre", modelo.razon_ingreso);
                    ViewBag.razon_ingreso2 = new SelectList(razones, "id", "nombre", modelo.razon_ingreso2);

                    icb_sysparameter parametro1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P115").FirstOrDefault();
                    int tipodocumento = parametro1 != null ? Convert.ToInt32(parametro1.syspar_value) : 28;

                    //parametro orden de taller prefactura
                    icb_sysparameter parametro2 = context.icb_sysparameter.Where(d => d.syspar_cod == "P150").FirstOrDefault();
                    int estadoprefacturada = parametro2 != null ? Convert.ToInt32(parametro2.syspar_value) : 25;

                    var documentos = context.tp_doc_registros.Where(d => d.tpdoc_estado && d.tipo == tipodocumento && d.perfil_contable_documento.Count()>0)
                    .Select(d => new { id = d.tpdoc_id, nombre = d.tpdoc_nombre }).ToList();
                    ViewBag.tipoDocumento = new SelectList(documentos, "id", "nombre", modelo.tipoDocumento);

                    ViewBag.operacionesTempario = new SelectList(context.ttempario, "codigo", "operacion");

                    var buscarCentro = context.centro_costo.Where(x => x.bodega == modelo.bodega).Select(x => new
                    {
                        id = x.centcst_id,
                        nombre = x.pre_centcst + " - " + x.Tipos_Cartera.descripcion
                    }).ToList();
                    ViewBag.cartera = new SelectList(buscarCentro, "id", "nombre", modelo.cartera);

                    ViewBag.aseguradora = new SelectList(context.icb_aseguradoras, "aseg_id", "nombre", modelo.aseguradora);


                    ViewBag.insumo =
                    new SelectList(context.Tinsumo.Select(x => new { x.id, descripcion = x.codigo + " | " + x.descripcion }), "id", "descripcion");

                    ViewBag.tarifainsumo =
                    new SelectList(context.ttipostarifa.Select(x => new { x.Descripcion, x.id }), "id", "descripcion");


                    ViewBag.tarifacombo =
                   new SelectList(context.ttipostarifa.Select(x => new { x.Descripcion, x.id }), "id", "descripcion");

                    icb_sysparameter idplanmparameter = context.icb_sysparameter.Where(d => d.syspar_cod == "P156").FirstOrDefault();
                    int idplanm = idplanmparameter != null ? Convert.ToInt32(idplanmparameter.syspar_value) : 8;



                    ViewBag.comboot = new SelectList(context.ttempario.Where(x => x.tipooperacion == idplanm).Select(x => new { x.operacion, x.codigo }), "codigo", "operacion");

                    using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                    {
                        try
                        {
                            bodega_concesionario bodegaconce = context.bodega_concesionario.Where(d => d.id == modelo.bodega)
                                .FirstOrDefault();
                            //validaciones preliminares
                            int funciono = 0;
                            decimal totalCreditos = 0;
                            decimal totalDebitos = 0;
                            decimal costoPromedioTotal = 0;
                            //busco el perfil contable del tipo de documento y bodega

                            perfil_contable_documento perfilcontable = context.perfil_contable_documento.Where(d =>
                                d.perfil_contable_bodega.Where(e => e.idbodega == modelo.bodega).Count() > 0 &&
                                d.tipo == modelo.tipoDocumento).FirstOrDefault();
                            if (perfilcontable != null)
                            {
                                var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                                                  join nombreParametro in context.paramcontablenombres
                                                                      on perfil.id_nombre_parametro equals nombreParametro.id
                                                                  join cuenta in context.cuenta_puc
                                                                      on perfil.cuenta equals cuenta.cntpuc_id
                                                                  where perfil.id_perfil == perfilcontable.id
                                                                  select new
                                                                  {
                                                                      perfil.id,
                                                                      perfil.id_nombre_parametro,
                                                                      perfil.cuenta,
                                                                      perfil.centro,
                                                                      perfil.id_perfil,
                                                                      nombreParametro.descripcion_parametro,
                                                                      cuenta.cntpuc_numero
                                                                  }).ToList();

                                int secuencia = 1;
                                //informacion de la orden de taller
                                tencabezaorden ordentaller = context.tencabezaorden.Where(d => d.id == modelo.idorden)
                                    .FirstOrDefault();

                                int exhibicion = 0;
                                if (ordentaller.razoningreso == accesorios)
                                {
                                    //busco si el vehiculo es de exhibicion
                                    vpedido existepedido = context.vpedido.Where(d => d.planmayor == ordentaller.placa)
                                        .FirstOrDefault();
                                    if (existepedido == null)
                                    {
                                        exhibicion = 1;
                                    }
                                }

                                if (modelo.razon_ingreso2 == razongarantia)
                                    {
                                    ordentaller.estadoorden_dos = 1;
                                    context.Entry(ordentaller).State = EntityState.Modified;
                                    context.SaveChanges();
                                    }



                                //centro de costo cero
                                List<cuentas_valores> ids_cuentas_valores = new List<cuentas_valores>();
                                centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                                int idCentroCero = centroValorCero != null
                                    ? Convert.ToInt32(centroValorCero.centcst_id)
                                    : 0;

                                List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();
                                List<ElementosFacturacion> listaelementos = new List<ElementosFacturacion>();
                                List<tsolicitudrepuestosot> cantidadreferencias = context.tsolicitudrepuestosot
                                    .Where(d => d.idorden == modelo.idorden && d.recibido).ToList();

                                //sumo el costo total de los repuestos si el vehiculo no es de exhibicion. Si lo es, ni los tomo en cuenta.
                                if (exhibicion == 1)
                                {
                                    cantidadreferencias = cantidadreferencias.Take(0).ToList();
                                }

                                int costoLineas = cantidadreferencias.Count();
                                //REPUESTOS 
                                for (int i = 0; i < costoLineas; i++)
                                {
                                    string referencia = !string.IsNullOrWhiteSpace(cantidadreferencias[i].reemplazo)
                                        ? cantidadreferencias[i].reemplazo
                                        : cantidadreferencias[i].tdetallerepuestosot.idrepuesto;
                                    decimal costoReferencia = cantidadreferencias[i].tdetallerepuestosot.valorunitario;
                                    costoPromedioTotal +=
                                        Convert.ToDecimal(costoReferencia, miCultura) *
                                        cantidadreferencias[i].tdetallerepuestosot.cantidad;

                                    listaelementos.Add(new ElementosFacturacion
                                    {
                                        tipo = "R",
                                        cantidad = cantidadreferencias[i].tdetallerepuestosot.cantidad,
                                        centro_costo = cantidadreferencias[i].tdetallerepuestosot.idcentro ?? 0,
                                        codigo = cantidadreferencias[i].tdetallerepuestosot.idrepuesto,
                                        porcentaje_descuento = cantidadreferencias[i].tdetallerepuestosot.pordescto,
                                        porcentaje_iva = cantidadreferencias[i].tdetallerepuestosot.poriva,
                                        tipo_tarifa = cantidadreferencias[i].tdetallerepuestosot.tipotarifa ?? 1,
                                        valor_descuento = calculardescuentore(
                                            cantidadreferencias[i].tdetallerepuestosot.valorunitario,
                                            cantidadreferencias[i].tdetallerepuestosot.cantidad,
                                            cantidadreferencias[i].tdetallerepuestosot.pordescto),
                                        valor_iva = calcularivare(
                                            cantidadreferencias[i].tdetallerepuestosot.valorunitario,
                                            cantidadreferencias[i].tdetallerepuestosot.cantidad,
                                            cantidadreferencias[i].tdetallerepuestosot.pordescto,
                                            cantidadreferencias[i].tdetallerepuestosot.poriva),
                                        valor_unitario =
                                            cantidadreferencias[i].tdetallerepuestosot.valorunitario 
                                           
                                    });
                                }
                                var mano_obra = context.ttarifastaller.Where(d => d.bodega == modelo.bodega && d.tipotarifa==1).FirstOrDefault();

                                //sumo el costo total de las operaciones
                                List<tdetallemanoobraot> cantidadoperaciones = context.tdetallemanoobraot
                                    .Where(d => d.idorden == modelo.idorden && d.estado == "1" && d.tipotarifa!=2).ToList();
                                int costooperaciones = cantidadoperaciones.Count();
                                //OPERACIONES
                                for (int i = 0; i < costooperaciones; i++)
                                {
                                    string operacion = cantidadoperaciones[i].idtempario;
                                    decimal costooperacion = cantidadoperaciones[i].valorunitario;
                                    costoPromedioTotal += costooperacion;

                                    listaelementos.Add(new ElementosFacturacion
                                    {
                                        tipo = "M",
                                        cantidad = 1,
                                        tiempo = cantidadoperaciones[i].tiempo ?? 0,
                                        centro_costo = cantidadoperaciones[i].idcentro ?? 0,
                                        codigo = cantidadoperaciones[i].idtempario,
                                        porcentaje_descuento = cantidadoperaciones[i].pordescuento ?? 0,
                                        porcentaje_iva = !string.IsNullOrWhiteSpace(mano_obra.iva) ? Convert.ToDecimal(mano_obra.iva, miCultura) : 0,
                                        tipo_tarifa = cantidadoperaciones[i].tipotarifa ?? 1,
                                        valor_descuento = calculardescuento(cantidadoperaciones[i].valorunitario,
                                            cantidadoperaciones[i].pordescuento),
                                        valor_iva = calculariva(cantidadoperaciones[i].valorunitario,
                                            cantidadoperaciones[i].pordescuento, Convert.ToDecimal(mano_obra.iva, miCultura)),
                                        valor_unitario = cantidadoperaciones[i].valorunitario
                                    });
                                }
                                string ot = modelo.idorden.ToString();
                               var cantidadtot  = context.vw_tot.Where(r => r.numot2 == ot).Select(x => new
                                    {
                                    x.fecha2,
                                    x.numerofactura,
                                    x.proveedor,
                                    x.valor_total,
                                    x.valor_total2,
                                    x.id2,
                                    x.numot,
                                    x.codigoentrada
                                    }).ToList();

                              
                                int costotot = cantidadtot.Count();
                                //TOT
                                for (int i = 0; i < costotot; i++)
                                    {
                                    string operacion = cantidadoperaciones[i].idtempario;
                                    decimal costooperacion = Convert.ToDecimal(cantidadtot[i].valor_total, miCultura);
                                    costoPromedioTotal += costooperacion;

                                    listaelementos.Add(new ElementosFacturacion
                                        {
                                        tipo = "TOT",
                                        cantidad = 1,  
                                        tipo_tarifa = 1,
                                        centro_costo = 0,
                                        porcentaje_descuento = 0,
                                        porcentaje_iva =  0,
                                        valor_descuento = 0,
                                        valor_iva = 0,
                                        codigo = cantidadtot[i].numerofactura.ToString(),  
                                        valor_unitario = Convert.ToDecimal(cantidadtot[i].valor_total, miCultura)
                                        });
                                    }

                                var listainsumos = context.tinsumooperaciones.Where(x => x.numot == modelo.idorden && x.valort_horas > 0).Select(e => new
                                    {
                                    e.id,
                                    e.codigo_insumo,
                                    Descripcion_insumo = e.Tinsumo.descripcion,
                                    tarifa = e.tipotarifa,
                                    porcentaje = e.porcentaje_total_,
                                    valor_tarifa = e.valortarifa,
                                    e.valort_horas
                                    }).ToList();

                                int tinsumos = listainsumos.Count();
                                //INSUMOS
                                for (int i = 0; i < tinsumos; i++)
                                    {
                              
                                    decimal costooperacion = Convert.ToDecimal(listainsumos[i].valort_horas, miCultura);
                                    costoPromedioTotal += costooperacion;

                                    ElementosFacturacion elemento = new ElementosFacturacion();
                                        elemento.tipo = "I";
                                    elemento.cantidad = 1;
                                    elemento.tiempo = Convert.ToDecimal(listainsumos[i].porcentaje, miCultura);
                                    elemento.codigo = listainsumos[i].codigo_insumo.ToString();
                                    elemento.tipo_tarifa = Convert.ToInt32(listainsumos[i].tarifa);
                                    elemento.centro_costo = 0;
                                    elemento.porcentaje_descuento = 0;
                                    elemento.porcentaje_iva = 0;
                                    elemento.valor_descuento = 0;
                                    elemento.valor_iva = 0;
                                    elemento.valor_unitario = Convert.ToDecimal(listainsumos[i].valort_horas, miCultura);


                                listaelementos.Add(elemento);
                                    }

                           
                                var listacombo = context.tot_combo.Where(x => x.numot == modelo.idorden && x.estado == true).Select(c => new
                                    {
                                    c.id,
                                    codigo = c.ttempario.codigo,
                                    operacion = c.ttempario.operacion,
                                    c.totalHorasCliente,
                                    c.totalhorasoperario,
                                    ttipostarifa = context.rtipocliente.Where(d => d.estado).Select(x => new { x.id, x.descripcion }).ToList(),
                                    c.tipotarifa,
                                    c.idtempario,
                                    valortotal = c.valortotal != null ? c.valortotal : 0
                                    }).ToList();
                                int tcombo = listacombo.Count();
                                //COMBOS 
                                for (int i = 0; i < tcombo; i++)
                                    {
                                
                                    decimal costooperacion = Convert.ToDecimal(listacombo[i].valortotal, miCultura);
                                    costoPromedioTotal += costooperacion;

                                    listaelementos.Add(new ElementosFacturacion
                                        {
                                        tipo = "CO",
                                        cantidad = 1,
                                        tiempo = listacombo[i].totalHorasCliente ?? 0,                                  
                                        codigo = listacombo[i].idtempario,                                 
                                        tipo_tarifa = listacombo[i].tipotarifa ?? 1,
                                        centro_costo = 0,
                                        porcentaje_descuento = 0,
                                        porcentaje_iva = 0,
                                        valor_descuento = 0,
                                        valor_iva = 0,
                                        valor_unitario = Convert.ToDecimal(listacombo[i].valortotal, miCultura)
                                        });
                                    }


                                //seleccionamos los que son diferentes de precio interno o garantias
                                listaelementos = listaelementos.Where(x => x.tipo_tarifa == 1 || x.tipo_tarifa == 3).ToList();
                                int? bodega = modelo.bodega;
                                int lista = cantidadreferencias.Where(x => x.tdetallerepuestosot.tipotarifa == 1 || x.tdetallerepuestosot.tipotarifa == 3).Count();
                                int lista2 = cantidadoperaciones.Where(x => x.tipotarifa == 1 || x.tipotarifa == 3).Count();
                                int lista7 = cantidadtot.Count();
                                int list8 = listainsumos.Where(x => x.tarifa == 1  || x.tarifa == 3).Count();
                                int lista9 = listacombo.Where(x => x.tipotarifa == 1 || x.tipotarifa == 3).Count();

                                int lista3 = cantidadreferencias.Where(x => x.tdetallerepuestosot.tipotarifa == 2).Count();
                                int lista4 = cantidadoperaciones.Where(x => x.tipotarifa == 2).Count();

                                int lista5 = cantidadreferencias.Where(x => x.tdetallerepuestosot.tipotarifa == 4).Count();
                                int lista6 = cantidadoperaciones.Where(x => x.tipotarifa == 4).Count();



                                if (lista > 0 || lista2 > 0 || lista3 > 0 || lista4 > 0 || lista5 > 0 || lista6 > 0 || lista7 > 0 || list8 > 0 || lista9 > 0)
                                {
                                    if (lista > 0 || lista2 > 0 || lista7 > 0 || list8 > 0 || lista9 >0)
                                    {
                                        int datos = Convert.ToInt32(lista);
                                        decimal costoTotal = 0, costodocliq=0;

                                        tercero_cliente tercero_cliente = context.tercero_cliente
                                         .Where(d => d.icb_terceros.tercero_id == modelo.id_cliente).FirstOrDefault();
                                        decimal desto_rep = 0, desto_mo=0;
                                        if (tercero_cliente!= null)
                                            {
                                            desto_rep = Convert.ToDecimal(tercero_cliente.dscto_rep, miCultura);
                                            }
                                        if (tercero_cliente != null)
                                            {
                                            desto_mo = Convert.ToDecimal(tercero_cliente.dscto_mo, miCultura);
                                            }



                                        foreach (var item in listaelementos)
                                            {
                                            decimal valorcantidad = 0, desc = 0, valoconivauni = 0, descliente = 0 ,valor=0;
                                            if (item.tipo == "M")
                                                {
                                                valorcantidad = item.valor_unitario;
                                                desc = valorcantidad - item.valor_descuento;

                                                valoconivauni = desc + item.valor_iva;
                                                costoTotal = costoTotal + valoconivauni;

                                                if (desto_mo != 0)
                                                    {
                                                    descliente = (desto_mo / 100) * desc;
                                                    desc = desc - descliente;
                                                    }
                                                valor = desc + item.valor_iva;
                                                costodocliq = costodocliq + valor;
                                                }
                                            else if (item.tipo == "R")
                                                {
                                                valorcantidad = item.cantidad * item.valor_unitario;
                                                desc = valorcantidad - item.valor_descuento;
                                                valoconivauni = desc + item.valor_iva;
                                                costoTotal = costoTotal + valoconivauni;
                                                if (desto_rep!=0)
                                                    {
                                                    descliente = (desto_rep / 100) * desc;
                                                    desc = desc - descliente;
                                                    }

                                                valor = desc + item.valor_iva;
                                                costodocliq = costodocliq + valor;
                                                }

                                            if (item.tipo == "TOT")
                                                {
                                                valorcantidad = item.valor_unitario;    
                                                costoTotal = costoTotal + valorcantidad;
                                                costodocliq = costodocliq + valorcantidad;
                                                }
                                            if (item.tipo == "I")
                                                {
                                                valorcantidad = item.valor_unitario;
                                                costoTotal = costoTotal + valorcantidad;
                                                costodocliq = costodocliq + valorcantidad;
                                                }
                                            if (item.tipo == "CO")
                                                {
                                                valorcantidad = item.valor_unitario;
                                                costoTotal = costoTotal + valorcantidad;
                                                costodocliq = costodocliq + valorcantidad;
                                                }


                                            }







                                        decimal ivaEncabezado = Convert.ToDecimal(Request["totaliva"], miCultura); //valor total del iva
                                        decimal descuentoEncabezado =
                                            Convert.ToDecimal(Request["totaldescuentos"], miCultura); //valor total del descuento
                                        decimal costoEncabezado =
                                            Convert.ToDecimal(Request["subtotal"], miCultura); //valor antes de impuestos
                                                                                               //esto es verdad? 
                                        decimal valor_totalenca = costoEncabezado - descuentoEncabezado;

                                     
                                        int? condicion = tercero_cliente.cod_pago_id;
                                        //consecutivo
                                        grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                                            x.documento_id == 3106 && x.bodega_id == bodega);
                                        if (grupo != null)
                                        {
                                            DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                                            long consecutivo = doc.BuscarConsecutivo(grupo.grupo);

                                            //Encabezado documento

                                            #region encabezado

                                            encab_documento encabezado = new encab_documento
                                            {
                                                //tipo = modelo.tipoDocumento.Value,
                                                //tipo = Convert.ToInt32(Request["tipoDocumento"]),
                                                tipo = 3106,
                                                numero = consecutivo,
                                                nit = modelo.id_cliente.Value,
                                                fecha = DateTime.Now,
                                                fpago_id = condicion,
                                                centro_doc = Convert.ToInt32(modelo.cartera),
                                                estado_factura= 1,
                                            };
                                            int dias = context.fpago_tercero.Find(condicion).dvencimiento ?? 0;
                                            DateTime vencimiento = DateTime.Now.AddDays(dias);
                                            encabezado.vencimiento = vencimiento;
                                            encabezado.valor_total = costodocliq;
                                            encabezado.iva = ivaEncabezado;
                                            encabezado.orden_taller = modelo.idorden;
                                            // Validacion para reteIVA, reteICA y retencion dependiendo del proveedor seleccionado

                                            #region calculo de retenciones

                                            tp_doc_registros buscarTipoDocRegistro =
                                                context.tp_doc_registros.FirstOrDefault(x =>
                                                    x.tpdoc_id == modelo.tipoDocumento.Value);
                                            tercero_cliente buscarCliente =
                                                context.tercero_cliente.FirstOrDefault(x =>
                                                    x.tercero_id == modelo.id_cliente);
                                            int? regimen_cliente = buscarCliente != null ? buscarCliente.tpregimen_id : 0;
                                            perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                                                x.bodega == bodega && x.sw == buscarTipoDocRegistro.sw &&
                                                x.tipo_regimenid == regimen_cliente);

                                            decimal retenciones = 0;

                                            if (buscarPerfilTributario != null)
                                            {
                                                if (buscarPerfilTributario.retfuente == "A" &&
                                                    valor_totalenca >= buscarTipoDocRegistro.baseretencion)
                                                {
                                                    encabezado.porcen_retencion = buscarTipoDocRegistro.retencion;
                                                    encabezado.retencion =
                                                        Math.Round(valor_totalenca *
                                                                   Convert.ToDecimal(
                                                                       buscarTipoDocRegistro.retencion / 100, miCultura));
                                                    retenciones += encabezado.retencion;
                                                }

                                                if (buscarPerfilTributario.retiva == "A" &&
                                                    ivaEncabezado >= buscarTipoDocRegistro.baseiva)
                                                {
                                                    encabezado.porcen_reteiva = buscarTipoDocRegistro.retiva;
                                                    encabezado.retencion_iva =
                                                        Math.Round(encabezado.iva *
                                                                   Convert.ToDecimal(buscarTipoDocRegistro.retiva / 100, miCultura));
                                                    retenciones += encabezado.retencion_iva;
                                                }

                                                if (buscarPerfilTributario.autorretencion == "A")
                                                {
                                                    decimal tercero_acteco = buscarCliente.acteco_tercero.autorretencion;
                                                    encabezado.porcen_autorretencion = (float)tercero_acteco;
                                                    encabezado.retencion_causada =
                                                        Math.Round(
                                                            valor_totalenca * Convert.ToDecimal(tercero_acteco / 100, miCultura));
                                                    retenciones += encabezado.retencion_causada;
                                                }

                                                if (buscarPerfilTributario.retica == "A" &&
                                                    valor_totalenca >= buscarTipoDocRegistro.baseica)
                                                {
                                                    terceros_bod_ica bodega_acteco = context.terceros_bod_ica.FirstOrDefault(x =>
                                                        x.idcodica == buscarCliente.actividadEconomica_id &&
                                                        x.bodega == bodega);
                                                    decimal tercero_acteco = buscarCliente.acteco_tercero.tarifa;
                                                    if (bodega_acteco != null)
                                                    {
                                                        encabezado.porcen_retica = (float)bodega_acteco.porcentaje;
                                                        encabezado.retencion_ica =
                                                            Math.Round(valor_totalenca *
                                                                       Convert.ToDecimal(bodega_acteco.porcentaje / 1000, miCultura));
                                                        retenciones += encabezado.retencion_ica;
                                                    }

                                                    if (tercero_acteco != 0)
                                                    {
                                                        encabezado.porcen_retica =
                                                            (float)buscarCliente.acteco_tercero.tarifa;
                                                        encabezado.retencion_ica =
                                                            Math.Round(valor_totalenca *
                                                                       Convert.ToDecimal(
                                                                           buscarCliente.acteco_tercero.tarifa / 1000, miCultura));
                                                        retenciones += encabezado.retencion_ica;
                                                    }
                                                    else
                                                    {
                                                        encabezado.porcen_retica = buscarTipoDocRegistro.retica;
                                                        encabezado.retencion_ica =
                                                            Math.Round(valor_totalenca *
                                                                       Convert.ToDecimal(
                                                                           buscarTipoDocRegistro.retica / 1000, miCultura));
                                                        retenciones += encabezado.retencion_ica;
                                                    }
                                                }
                                            }

                                            #endregion

                                            encabezado.costo = costoPromedioTotal;
                                            //para efectos de la facturacion, quien es el vendedor?
                                            encabezado.vendedor = Convert.ToInt32(Request["vendedor"]);
                                            encabezado.perfilcontable = perfilcontable.id;
                                            encabezado.bodega = modelo.bodega.Value;
                                            monedas moneda2 = context.monedas.Where(d => d.estado).FirstOrDefault();
                                            int moneda = 1;
                                            if (moneda2 != null)
                                            {
                                                moneda = moneda2.moneda;
                                            }

                                            encabezado.moneda = moneda;
                                            encabezado.valor_mercancia = valor_totalenca;
                                            encabezado.fec_creacion = DateTime.Now;
                                            encabezado.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                            encabezado.estado = true;
                                            encabezado.prefactura = true;
                                            encabezado.orden_taller = modelo.idorden;
                                            context.encab_documento.Add(encabezado);
                                            context.SaveChanges();

                                            #endregion

                                            int id_encabezado = context.encab_documento
                                                .OrderByDescending(x => x.idencabezado).FirstOrDefault().idencabezado;
                                            encab_documento eg = context.encab_documento.FirstOrDefault(x =>
                                                x.idencabezado == id_encabezado);

                                            //Mov Contable

                                            #region movimientos contables

                                            //buscamos en perfil cuenta documento, por medio del perfil seleccionado
                                            foreach (var parametro in parametrosCuentasVerificar)
                                            {
                                                string descripcionParametro = context.paramcontablenombres
                                                    .FirstOrDefault(x => x.id == parametro.id_nombre_parametro)
                                                    .descripcion_parametro;
                                                cuenta_puc buscarCuenta =
                                                    context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);

                                                if (buscarCuenta != null)
                                                {
                                                    if (parametro.id_nombre_parametro == 10 &&
                                                        Convert.ToDecimal(valor_totalenca, miCultura) != 0
                                                        || parametro.id_nombre_parametro == 3 &&
                                                        Convert.ToDecimal(eg.retencion, miCultura) != 0
                                                        || parametro.id_nombre_parametro == 4 &&
                                                        Convert.ToDecimal(eg.retencion_iva, miCultura) != 0
                                                        || parametro.id_nombre_parametro == 5 &&
                                                        Convert.ToDecimal(eg.retencion_ica, miCultura) != 0
                                                        || parametro.id_nombre_parametro == 6 &&
                                                        Convert.ToDecimal(eg.fletes, miCultura) != 0
                                                        || parametro.id_nombre_parametro == 14 &&
                                                        Convert.ToDecimal(eg.iva_fletes, miCultura) != 0
                                                        || parametro.id_nombre_parametro == 17 &&
                                                        Convert.ToDecimal(eg.retencion_causada, miCultura) != 0
                                                        || parametro.id_nombre_parametro == 18 &&
                                                        Convert.ToDecimal(eg.retencion_causada, miCultura) != 0)
                                                    {
                                                        mov_contable movNuevo = new mov_contable
                                                        {
                                                            id_encab = eg.idencabezado,
                                                            seq = secuencia,
                                                            idparametronombre = parametro.id_nombre_parametro,
                                                            cuenta = parametro.cuenta,
                                                            centro = parametro.centro,
                                                            fec = DateTime.Now,
                                                            fec_creacion = DateTime.Now,
                                                            userid_creacion =
                                                            Convert.ToInt32(Session["user_usuarioid"]),
                                                            documento = Convert.ToString(modelo.idorden),
                                                            detalle =
                                                            "Facturacion de orden de talle N° " +
                                                            ordentaller.codigoentrada + " con consecutivo " + eg.numero,
                                                            estado = true
                                                        };

                                                        cuenta_puc info = context.cuenta_puc
                                                            .Where(a => a.cntpuc_id == parametro.cuenta).FirstOrDefault();

                                                        if (info.tercero)
                                                        {
                                                            movNuevo.nit = ordentaller.tercero;
                                                        }
                                                        else
                                                        {
                                                            icb_terceros tercero = context.icb_terceros
                                                                .Where(t => t.doc_tercero == "0").FirstOrDefault();
                                                            movNuevo.nit = tercero.tercero_id;
                                                        }

                                                        // las siguientes validaciones se hacen dependiendo de la cuenta que esta moviendo la compra manual, para guardar la informacion acorde

                                                        #region Cuentas X Cobrar

                                                        if (parametro.id_nombre_parametro == 10)
                                                        {
                                                            /*if (info.aplicaniff==true)
                                                            {

                                                            }*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                //no estoy seguro de que aquí tenga que ir el id de la orden. Usualmente este campo, si se llena, está para recibir el id del pedido
                                                                movNuevo.documento = Convert.ToString(modelo.idorden);
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = Convert.ToDecimal(costoTotal, miCultura);

                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = Convert.ToDecimal(costoTotal, miCultura);
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = Convert.ToDecimal(costoTotal, miCultura);
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = Convert.ToDecimal(costoTotal, miCultura);
                                                            }
                                                        }

                                                        #endregion

                                                        #region Retencion

                                                        if (parametro.id_nombre_parametro == 3)
                                                        {
                                                            /*if (info.aplicaniff==true)
                                                            {

                                                            }*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = modelo.documento;
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = eg.retencion;

                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = eg.retencion;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = eg.retencion;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = eg.retencion;
                                                            }
                                                        }

                                                        #endregion

                                                        #region ReteIVA

                                                        if (parametro.id_nombre_parametro == 4)
                                                        {
                                                            /*if (info.aplicaniff==true)
                                                            {

                                                            }*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(ivaEncabezado, miCultura);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = modelo.documento;
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = eg.retencion_iva;

                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = eg.retencion_iva;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = eg.retencion_iva;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = eg.retencion_iva;
                                                            }
                                                        }

                                                        #endregion

                                                        #region ReteICA

                                                        if (parametro.id_nombre_parametro == 5)
                                                        {
                                                            /*if (info.aplicaniff==true)
                                                            {

                                                            }*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = modelo.documento;
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = eg.retencion_ica;

                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = eg.retencion_ica;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = eg.retencion_ica;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = eg.retencion_ica;
                                                            }
                                                        }

                                                        #endregion

                                                        #region Fletes

                                                        if (parametro.id_nombre_parametro == 6)
                                                        {
                                                            /*if (info.aplicaniff==true)
                                                            {

                                                            }*/

                                                            /*if (info.manejabase == true)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(modelo.fletes);
                                                            }*/
                                                            //else
                                                            //{
                                                            movNuevo.basecontable = 0;
                                                            //}

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = modelo.documento;
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = eg.fletes;
                                                                movNuevo.debito = 0;

                                                                movNuevo.creditoniif = eg.fletes;
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = eg.fletes;
                                                                ;
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = eg.fletes;
                                                                movNuevo.debito = 0;
                                                            }
                                                        }

                                                        #endregion

                                                        #region IVA fletes

                                                        if (parametro.id_nombre_parametro == 14)
                                                        {
                                                            /*if (info.aplicaniff==true)
                                                            {

                                                            }*/

                                                            /*if (info.manejabase == true)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(modelo.fletes);
                                                            }
                                                            else
                                                            {*/
                                                            movNuevo.basecontable = 0;
                                                            //}

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = modelo.documento;
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = eg.iva_fletes;
                                                                movNuevo.debito = 0;

                                                                movNuevo.creditoniif = eg.iva_fletes;
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = eg.iva_fletes;
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = eg.iva_fletes;
                                                                movNuevo.debito = 0;
                                                            }
                                                        }

                                                        #endregion

                                                        #region AutoRetencion Debito

                                                        if (parametro.id_nombre_parametro == 17)
                                                        {
                                                            /*if (info.aplicaniff==true)
                                                            {

                                                            }*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = modelo.documento;
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = eg.retencion_causada;

                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = eg.retencion_causada;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = 0;
                                                                movNuevo.debitoniif = eg.retencion_causada;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = 0;
                                                                movNuevo.debito = eg.retencion_causada;
                                                            }
                                                        }

                                                        #endregion

                                                        #region AutoRetencion Credito

                                                        if (parametro.id_nombre_parametro == 18)
                                                        {
                                                            /*if (info.aplicaniff==true)
                                                            {

                                                            }*/

                                                            if (info.manejabase)
                                                            {
                                                                movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                                            }
                                                            else
                                                            {
                                                                movNuevo.basecontable = 0;
                                                            }

                                                            if (info.documeto)
                                                            {
                                                                movNuevo.documento = modelo.documento;
                                                            }

                                                            if (buscarCuenta.concepniff == 1)
                                                            {
                                                                movNuevo.credito = eg.retencion_causada;
                                                                movNuevo.debito = 0;

                                                                movNuevo.creditoniif = eg.retencion_causada;
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 4)
                                                            {
                                                                movNuevo.creditoniif = eg.retencion_causada;
                                                                movNuevo.debitoniif = 0;
                                                            }

                                                            if (buscarCuenta.concepniff == 5)
                                                            {
                                                                movNuevo.credito = eg.retencion_causada;
                                                                movNuevo.debito = 0;
                                                            }
                                                        }

                                                        #endregion

                                                        //context.mov_contable.Add(movNuevo);
                                                        //context.SaveChanges();

                                                        secuencia++;
                                                        //Cuentas valores

                                                        #region Cuentas valores

                                                        cuentas_valores buscar_cuentas_valores =
                                                            context.cuentas_valores.FirstOrDefault(x =>
                                                                x.centro == parametro.centro &&
                                                                x.cuenta == parametro.cuenta && x.nit == movNuevo.nit);
                                                        if (buscar_cuentas_valores != null)
                                                        {
                                                            buscar_cuentas_valores.debito += movNuevo.debito;
                                                            buscar_cuentas_valores.credito += movNuevo.credito;
                                                            buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                                                            buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                                                            //context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                                        }
                                                        else
                                                        {
                                                            DateTime fechaHoy = DateTime.Now;
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
                                                            //context.cuentas_valores.Add(crearCuentaValor);
                                                            //context.SaveChanges();
                                                        }

                                                        #endregion

                                                        totalCreditos += movNuevo.credito;
                                                        totalDebitos += movNuevo.debito;
                                                        listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                        {
                                                            NumeroCuenta =
                                                                "(" + buscarCuenta.cntpuc_numero + ")" +
                                                                buscarCuenta.cntpuc_descp,
                                                            DescripcionParametro = descripcionParametro,
                                                            ValorDebito = movNuevo.debito,
                                                            ValorCredito = movNuevo.credito
                                                        });
                                                    }
                                                }
                                            }

                                            #endregion

                                            //Documentos a cruzar

                                            #region Documentos a cruzar

                                            string listaAnticipo = Request["listaAnticipo"];
                                            if (!string.IsNullOrEmpty(listaAnticipo))
                                            {
                                                int la = Convert.ToInt32(listaAnticipo);
                                                for (int i = 1; i <= la; i++)
                                                {
                                                    int encabAnti = Convert.ToInt32(Request["encabAnticipo" + i]);
                                                    if (encabAnti != 0)
                                                    {
                                                        encab_documento encabezadoAnticipo =
                                                            context.encab_documento.FirstOrDefault(x =>
                                                                x.idencabezado == encabAnti);

                                                        documentosacruzar dac = new documentosacruzar
                                                        {
                                                            idencabrecibo = encabAnti,
                                                            valorrecibo = encabezadoAnticipo.valor_total,
                                                            idencabfactura = id_encabezado,
                                                            valorfactura = eg.valor_total,
                                                            saldo = encabezadoAnticipo.valor_total - eg.valor_total
                                                        };

                                                        //context.documentosacruzar.Add(dac);
                                                        //context.SaveChanges();
                                                    }
                                                }
                                            }

                                            #endregion




                                            //Lineas documento

                                            #region lineasDocumento

                                            List<mov_contable> listaMov = new List<mov_contable>();
                                            int listaLineas = listaelementos.Count();
                                            for (int i = 0; i < listaLineas; i++)
                                            {
                                                //if (!string.IsNullOrEmpty(Request["referencia" + i]))
                                                //{
                                                //var porDescuento = (!string.IsNullOrEmpty(Request["descuentoReferencia" + i])) ? Convert.ToDecimal(Request["descuentoReferencia" + i]) : 0;
                                                decimal porDescuento = listaelementos[i].porcentaje_descuento;

                                                //var codigo = Request["referencia" + i];
                                                string codigo = listaelementos[i].codigo;

                                                //var cantidadFacturada = Convert.ToDecimal(Request["cantidadReferencia" + i]);
                                                decimal cantidadFacturada = listaelementos[i].cantidad;
                                                //var valorReferencia = Convert.ToDecimal(Request["valorUnitarioReferencia" + i]);
                                                decimal valorReferencia = listaelementos[i].valor_unitario;
                                                decimal descontar = porDescuento / 100;
                                                //var porIVAReferencia = Convert.ToDecimal(Request["ivaReferencia" + i]) / 100;
                                                decimal porIVAReferencia = listaelementos[i].porcentaje_iva / 100;

                                                decimal decontarcliente = desto_rep / 100;


                                                decimal final = Math.Round(valorReferencia - valorReferencia * descontar);

                                                //final = Math.Round(final - final * decontarcliente);

                                                //var baseUnitario = final * Convert.ToDecimal(Request["cantidadReferencia" + i]);
                                                decimal baseUnitario = final * listaelementos[i].cantidad;

                                                decimal ivaReferencia =
                                                    Math.Round(final * porIVAReferencia * listaelementos[i].cantidad);
                                                string und = "0";
                                                if (listaelementos[i].tipo == "R")
                                                {
                                                    icb_referencia unidadCodigo =
                                                        context.icb_referencia.FirstOrDefault(x => x.ref_codigo == codigo);
                                                    und = unidadCodigo.unidad_medida;
                                                }

                                                decimal costoReferencia = 0;
                                                decimal cr = 0;
                                                if (listaelementos[i].tipo == "R")
                                                {
                                                    //var vwPromedio = context.vw_promedio.FirstOrDefault(x => x.codigo == codigo && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month);
                                                    icb_referencia vwPromedio =
                                                        context.icb_referencia.FirstOrDefault(x => x.ref_codigo == codigo);
                                                    costoReferencia = vwPromedio.costo_promedio ?? 0;
                                                    cr = costoReferencia * listaelementos[i].cantidad;
                                                }
                                                else if(listaelementos[i].tipo == "M")
                                                {
                                                    ttempario vwPromedio =
                                                        context.ttempario.FirstOrDefault(x => x.codigo == codigo);
                                                    costoReferencia = vwPromedio.costo ?? 0;
                                                    cr = costoReferencia * listaelementos[i].cantidad;
                                                }
                                                /*
                                                if (!string.IsNullOrEmpty(Request["pedidoID" + i]))
                                                {
                                                    var pedidoSeleccionado = Convert.ToInt32(Request["pedidoID" + i]);

                                                    var buscar_movimientoPedido = context.icb_referencia_movdetalle.FirstOrDefault(x => x.refmov_id == pedidoSeleccionado && x.ref_codigo == codigo);
                                                    if (buscar_movimientoPedido != null)
                                                    {
                                                        if (buscar_movimientoPedido.refdet_saldo != null)
                                                        {
                                                            buscar_movimientoPedido.refdet_saldo += cantidadFacturada;
                                                        }
                                                        else
                                                        {
                                                            buscar_movimientoPedido.refdet_saldo = cantidadFacturada;
                                                        }

                                                        context.Entry(buscar_movimientoPedido).State = EntityState.Modified;
                                                    }
                                                }*/

                                                if (listaelementos[i].tipo == "R")
                                                {
                                                    lineas_documento lineas = new lineas_documento
                                                    {
                                                        id_encabezado = id_encabezado,
                                                        seq = i + 1,
                                                        fec = DateTime.Now,
                                                        nit = ordentaller.tercero
                                                    };
                                                    if (listaelementos[i].tipo == "R")
                                                    {
                                                        lineas.und = Convert.ToString(und);
                                                        lineas.codigo = codigo;
                                                    }

                                                    lineas.cantidad = listaelementos[i].cantidad;
                                                    decimal ivaLista = listaelementos[i].porcentaje_iva;
                                                    lineas.porcentaje_iva = (float)ivaLista;
                                                    lineas.valor_unitario = final;
                                                    decimal descuento = porDescuento;
                                                    lineas.porcentaje_descuento = (float)descuento;
                                                    lineas.costo_unitario = Convert.ToDecimal(costoReferencia, miCultura);
                                                    lineas.bodega = ordentaller.bodega;
                                                    lineas.fec_creacion = DateTime.Now;
                                                    lineas.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                                    lineas.estado = true;
                                                    lineas.id_tarifa_cliente = listaelementos[i].tipo_tarifa;
                                                    lineas.moneda = moneda;
                                                    lineas.vendedor = eg.vendedor;

                                                    context.lineas_documento.Add(lineas);
                                                }
                                                else if(listaelementos[i].tipo == "M")
                                                {
                                                lineas_documento_operaciones lineasoperaciones = new lineas_documento_operaciones();  
                                                lineasoperaciones.id_encabezado = id_encabezado;
                                                lineasoperaciones.seq = i + 1;
                                                lineasoperaciones.fec = DateTime.Now;
                                                lineasoperaciones.nit = ordentaller.tercero;
                                                lineasoperaciones.codigo_operacion = codigo;
                                                //lineasoperaciones.porcentaje_iva = listaelementos[i].porcentaje_iva != null ? float.Parse(listaelementos[i].porcentaje_iva.ToString()):0;
                                                lineasoperaciones.porcentaje_iva =float.Parse(listaelementos[i].porcentaje_iva.ToString());
                                                lineasoperaciones.costo_unitario = listaelementos[i].valor_unitario;
                                                lineasoperaciones.bodega = ordentaller.bodega;
                                                lineasoperaciones.fec_creacion = DateTime.Now;
                                                lineasoperaciones.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                                lineasoperaciones.estado = true;
                                                lineasoperaciones.moneda = moneda;
                                                lineasoperaciones.vendedor = eg.vendedor;
                                                context.lineas_documento_operaciones.Add(lineasoperaciones);
                                                }

                                                #endregion

                                                #region Mov Contable (IVA, Inventario, Costo, Ingreso)

                                                foreach (var parametro in parametrosCuentasVerificar)
                                                {
                                                    string descripcionParametro = context.paramcontablenombres
                                                        .FirstOrDefault(x => x.id == parametro.id_nombre_parametro)
                                                        .descripcion_parametro;
                                                    cuenta_puc buscarCuenta =
                                                        context.cuenta_puc.FirstOrDefault(x =>
                                                            x.cntpuc_id == parametro.cuenta);

                                                    if (buscarCuenta != null)
                                                    {
                                                        if (parametro.id_nombre_parametro == 2 &&
                                                            Convert.ToDecimal(ivaEncabezado, miCultura) != 0
                                                            || parametro.id_nombre_parametro == 9 &&
                                                            Convert.ToDecimal(costoPromedioTotal, miCultura) != 0 //costo promedio
                                                            || parametro.id_nombre_parametro == 20 &&
                                                            Convert.ToDecimal(costoPromedioTotal, miCultura) != 0 //costo promedio
                                                            || parametro.id_nombre_parametro == 11 &&
                                                            Convert.ToDecimal(costoEncabezado, miCultura) != 0
                                                            || parametro.id_nombre_parametro == 12 &&
                                                            Convert.ToDecimal(costoPromedioTotal, miCultura) != 0) //costo promedio
                                                        {
                                                            mov_contable movNuevo = new mov_contable
                                                            {
                                                                id_encab = encabezado.idencabezado,
                                                                seq = secuencia,
                                                                idparametronombre = parametro.id_nombre_parametro,
                                                                cuenta = parametro.cuenta,
                                                                centro = listaelementos[i].tipo_tarifa == 2
                                                                ? parametro.id_nombre_parametro == 11
                                                                    ?
                                                                    listaelementos[i].centro_costo
                                                                    : parametro.id_nombre_parametro == 12
                                                                        ? listaelementos[i].centro_costo
                                                                        : parametro.centro
                                                                : parametro.centro,
                                                                fec = DateTime.Now,
                                                                fec_creacion = DateTime.Now,
                                                                tipo_tarifa = listaelementos[i].tipo_tarifa,
                                                                userid_creacion =
                                                                Convert.ToInt32(Session["user_usuarioid"]),
                                                                documento = Convert.ToString(ordentaller.id)
                                                            };

                                                            cuenta_puc info = context.cuenta_puc
                                                                .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                .FirstOrDefault();

                                                            if (info.tercero)
                                                            {
                                                                movNuevo.nit = ordentaller.tercero;
                                                            }
                                                            else
                                                            {
                                                                icb_terceros tercero = context.icb_terceros
                                                                    .Where(t => t.doc_tercero == "0").FirstOrDefault();
                                                                movNuevo.nit = tercero.tercero_id;
                                                            }

                                                            #region IVA

                                                            if (parametro.id_nombre_parametro == 2)
                                                            {
                                                                icb_referencia perfilReferencia =
                                                                    context.icb_referencia.FirstOrDefault(x =>
                                                                        x.ref_codigo == codigo);
                                                                int perfilBuscar = 0;

                                                                if (perfilReferencia != null)
                                                                {
                                                                    perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                                                }

                                                                perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(
                                                                    r => r.id == perfilBuscar);

                                                                #region Tiene perfil la referencia

                                                                if (pcr != null)
                                                                {
                                                                    int? cuentaIva = pcr.cuenta_dev_iva_compras;

                                                                    movNuevo.id_encab = encabezado.idencabezado;
                                                                    movNuevo.seq = secuencia;
                                                                    movNuevo.idparametronombre =
                                                                        parametro.id_nombre_parametro;

                                                                    #region si tiene perfil y cuenta asignada a ese perfil

                                                                    if (cuentaIva != null)
                                                                    {
                                                                        movNuevo.cuenta = Convert.ToInt32(cuentaIva);
                                                                        movNuevo.centro = parametro.centro;
                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        movNuevo.documento = Convert.ToString(eg.numero);

                                                                        cuenta_puc infoReferencia = context.cuenta_puc
                                                                            .Where(a => a.cntpuc_id == cuentaIva)
                                                                            .FirstOrDefault();
                                                                        if (infoReferencia.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (infoReferencia.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (infoReferencia.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito =
                                                                                Convert.ToDecimal(ivaReferencia, miCultura);
                                                                            movNuevo.debito = 0;

                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(ivaReferencia, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(ivaReferencia, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito =
                                                                                Convert.ToDecimal(ivaReferencia, miCultura);
                                                                            movNuevo.debito = 0;
                                                                        }

                                                                        // context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion

                                                                    #region si tiene perfil pero no tiene cuenta asignada

                                                                    else
                                                                    {
                                                                        movNuevo.cuenta = parametro.cuenta;
                                                                        movNuevo.centro = parametro.centro;
                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        movNuevo.documento = Convert.ToString(eg.numero);

                                                                        cuenta_puc infoReferencia = context.cuenta_puc
                                                                            .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                            .FirstOrDefault();
                                                                        if (infoReferencia.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (infoReferencia.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (infoReferencia.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito =
                                                                                Convert.ToDecimal(ivaReferencia, miCultura);
                                                                            movNuevo.debito = 0;

                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(ivaReferencia, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(ivaReferencia, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito =
                                                                                Convert.ToDecimal(ivaReferencia, miCultura);
                                                                            movNuevo.debito = 0;
                                                                        }

                                                                        //context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion
                                                                }

                                                                #endregion

                                                                #region La referencia no tiene perfil

                                                                else
                                                                {
                                                                    movNuevo.id_encab = encabezado.idencabezado;
                                                                    movNuevo.seq = secuencia;
                                                                    movNuevo.idparametronombre =
                                                                        parametro.id_nombre_parametro;
                                                                    movNuevo.cuenta = parametro.cuenta;
                                                                    movNuevo.centro = parametro.centro;
                                                                    movNuevo.fec = DateTime.Now;
                                                                    movNuevo.fec_creacion = DateTime.Now;
                                                                    movNuevo.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);
                                                                    /*if (info.aplicaniff==true)
                                                                    {

                                                                    }*/

                                                                    if (info.manejabase)
                                                                    {
                                                                        movNuevo.basecontable =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                    }
                                                                    else
                                                                    {
                                                                        movNuevo.basecontable = 0;
                                                                    }

                                                                    if (info.documeto)
                                                                    {
                                                                        movNuevo.documento = Convert.ToString(eg.numero);
                                                                    }

                                                                    if (buscarCuenta.concepniff == 1)
                                                                    {
                                                                        movNuevo.credito = Convert.ToDecimal(ivaReferencia, miCultura);
                                                                        movNuevo.debito = 0;

                                                                        movNuevo.creditoniif =
                                                                            Convert.ToDecimal(ivaReferencia, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (buscarCuenta.concepniff == 4)
                                                                    {
                                                                        movNuevo.creditoniif =
                                                                            Convert.ToDecimal(ivaReferencia, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (buscarCuenta.concepniff == 5)
                                                                    {
                                                                        movNuevo.credito = Convert.ToDecimal(ivaReferencia, miCultura);
                                                                        movNuevo.debito = 0;
                                                                    }

                                                                    //context.mov_contable.Add(movNuevo);
                                                                }

                                                                #endregion

                                                                mov_contable buscarIVA = context.mov_contable.FirstOrDefault(x =>
                                                                    x.id_encab == id_encabezado &&
                                                                    x.cuenta == movNuevo.cuenta &&
                                                                    x.idparametronombre == parametro.id_nombre_parametro);
                                                                if (buscarIVA != null)
                                                                {
                                                                    buscarIVA.debito += movNuevo.debito;
                                                                    buscarIVA.debitoniif += movNuevo.debitoniif;
                                                                    buscarIVA.credito += movNuevo.credito;
                                                                    buscarIVA.creditoniif += movNuevo.creditoniif;
                                                                    //context.Entry(buscarIVA).State = EntityState.Modified;
                                                                }
                                                                else
                                                                {
                                                                    mov_contable crearMovContable = new mov_contable
                                                                    {
                                                                        id_encab = encabezado.idencabezado,
                                                                        seq = secuencia,
                                                                        idparametronombre =
                                                                        parametro.id_nombre_parametro,
                                                                        cuenta =
                                                                        Convert.ToInt32(movNuevo.cuenta),
                                                                        centro = parametro.centro,
                                                                        nit = encabezado.nit,
                                                                        fec = DateTime.Now,
                                                                        debito = movNuevo.debito,
                                                                        debitoniif = movNuevo.debitoniif,
                                                                        basecontable = movNuevo.basecontable,
                                                                        credito = movNuevo.credito,
                                                                        creditoniif = movNuevo.creditoniif,
                                                                        fec_creacion = DateTime.Now,
                                                                        userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]),
                                                                        detalle =
                                                                        "Facturacion de orden taller N° " +
                                                                        ordentaller.codigoentrada + " con consecutivo " +
                                                                        eg.numero,
                                                                        estado = true
                                                                    };
                                                                    //context.mov_contable.Add(crearMovContable);
                                                                    //context.SaveChanges();
                                                                }
                                                            }

                                                            #endregion

                                                            #region Inventario

                                                            //si el tipo de elemento que estoy analizando en este momento es de inventario
                                                            if (listaelementos[i].tipo == "R")
                                                            {
                                                                if (parametro.id_nombre_parametro == 9 ||
                                                                    parametro.id_nombre_parametro == 20)
                                                                {
                                                                    icb_referencia perfilReferencia =
                                                                        context.icb_referencia.FirstOrDefault(x =>
                                                                            x.ref_codigo == codigo);
                                                                    int perfilBuscar =
                                                                        Convert.ToInt32(perfilReferencia.perfil);
                                                                    perfilcontable_referencia pcr = context.perfilcontable_referencia
                                                                        .FirstOrDefault(r => r.id == perfilBuscar);

                                                                    #region Tiene perfil la referencia

                                                                    if (pcr != null)
                                                                    {
                                                                        int? cuentaInven = pcr.cta_inventario;

                                                                        movNuevo.id_encab = encabezado.idencabezado;
                                                                        movNuevo.seq = secuencia;
                                                                        movNuevo.idparametronombre =
                                                                            parametro.id_nombre_parametro;

                                                                        #region tiene perfil y cuenta asignada al perfil

                                                                        if (cuentaInven != null)
                                                                        {
                                                                            movNuevo.cuenta = Convert.ToInt32(cuentaInven);
                                                                            movNuevo.centro = parametro.centro;
                                                                            movNuevo.fec = DateTime.Now;
                                                                            movNuevo.fec_creacion = DateTime.Now;
                                                                            movNuevo.userid_creacion =
                                                                                Convert.ToInt32(Session["user_usuarioid"]);
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);

                                                                            cuenta_puc infoReferencia = context.cuenta_puc
                                                                                .Where(a => a.cntpuc_id == cuentaInven)
                                                                                .FirstOrDefault();
                                                                            if (infoReferencia.manejabase)
                                                                            {
                                                                                movNuevo.basecontable =
                                                                                    Convert.ToDecimal(baseUnitario, miCultura);
                                                                            }
                                                                            else
                                                                            {
                                                                                movNuevo.basecontable = 0;
                                                                            }

                                                                            if (infoReferencia.documeto)
                                                                            {
                                                                                movNuevo.documento =
                                                                                    Convert.ToString(eg.numero);
                                                                            }

                                                                            if (infoReferencia.concepniff == 1)
                                                                            {
                                                                                movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                                movNuevo.debito = 0;

                                                                                movNuevo.creditoniif =
                                                                                    Convert.ToDecimal(cr, miCultura);
                                                                                movNuevo.debitoniif = 0;
                                                                            }

                                                                            if (infoReferencia.concepniff == 4)
                                                                            {
                                                                                movNuevo.creditoniif =
                                                                                    Convert.ToDecimal(cr, miCultura);
                                                                                movNuevo.debitoniif = 0;
                                                                            }

                                                                            if (infoReferencia.concepniff == 5)
                                                                            {
                                                                                movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                                movNuevo.debito = 0;
                                                                            }

                                                                            //context.mov_contable.Add(movNuevo);
                                                                        }

                                                                        #endregion

                                                                        #region tiene perfil pero no tiene cuenta asignada

                                                                        else
                                                                        {
                                                                            movNuevo.cuenta = parametro.cuenta;
                                                                            movNuevo.centro = parametro.centro;
                                                                            movNuevo.fec = DateTime.Now;
                                                                            movNuevo.fec_creacion = DateTime.Now;
                                                                            movNuevo.userid_creacion =
                                                                                Convert.ToInt32(Session["user_usuarioid"]);
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);

                                                                            cuenta_puc infoReferencia = context.cuenta_puc
                                                                                .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                                .FirstOrDefault();
                                                                            if (infoReferencia.manejabase)
                                                                            {
                                                                                movNuevo.basecontable =
                                                                                    Convert.ToDecimal(valor_totalenca, miCultura);
                                                                            }
                                                                            else
                                                                            {
                                                                                movNuevo.basecontable = 0;
                                                                            }

                                                                            if (infoReferencia.documeto)
                                                                            {
                                                                                movNuevo.documento =
                                                                                    Convert.ToString(eg.numero);
                                                                            }

                                                                            if (infoReferencia.concepniff == 1)
                                                                            {
                                                                                movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                                movNuevo.debito = 0;

                                                                                movNuevo.creditoniif =
                                                                                    Convert.ToDecimal(cr, miCultura);
                                                                                movNuevo.debitoniif = 0;
                                                                            }

                                                                            if (infoReferencia.concepniff == 4)
                                                                            {
                                                                                movNuevo.creditoniif =
                                                                                    Convert.ToDecimal(cr, miCultura);
                                                                                movNuevo.debitoniif = 0;
                                                                            }

                                                                            if (infoReferencia.concepniff == 5)
                                                                            {
                                                                                movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                                movNuevo.debito = 0;
                                                                            }

                                                                            //context.mov_contable.Add(movNuevo);
                                                                        }

                                                                        #endregion
                                                                    }

                                                                    #endregion

                                                                    #region La referencia no tiene perfil

                                                                    else
                                                                    {
                                                                        movNuevo.id_encab = encabezado.idencabezado;
                                                                        movNuevo.seq = secuencia;
                                                                        movNuevo.idparametronombre =
                                                                            parametro.id_nombre_parametro;
                                                                        movNuevo.cuenta = parametro.cuenta;
                                                                        movNuevo.centro = parametro.centro;
                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        /*if (info.aplicaniff==true)
                                                                        {

                                                                        }*/

                                                                        if (info.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(valor_totalenca, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (info.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (buscarCuenta.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debito = 0;

                                                                            movNuevo.creditoniif = Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (buscarCuenta.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif = Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (buscarCuenta.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debito = 0;
                                                                        }

                                                                        //context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion

                                                                    mov_contable buscarInventario =
                                                                        context.mov_contable.FirstOrDefault(x =>
                                                                            x.id_encab == id_encabezado &&
                                                                            x.cuenta == movNuevo.cuenta &&
                                                                            x.idparametronombre ==
                                                                            parametro.id_nombre_parametro);
                                                                    if (buscarInventario != null)
                                                                    {
                                                                        buscarInventario.basecontable +=
                                                                            movNuevo.basecontable;
                                                                        buscarInventario.debito += movNuevo.debito;
                                                                        buscarInventario.debitoniif += movNuevo.debitoniif;
                                                                        buscarInventario.credito += movNuevo.credito;
                                                                        buscarInventario.creditoniif +=
                                                                            movNuevo.creditoniif;
                                                                        //context.Entry(buscarInventario).State =
                                                                          //  EntityState.Modified;
                                                                    }
                                                                    else
                                                                    {
                                                                        mov_contable crearMovContable = new mov_contable
                                                                        {
                                                                            id_encab = encabezado.idencabezado,
                                                                            seq = secuencia,
                                                                            idparametronombre =
                                                                            parametro.id_nombre_parametro,
                                                                            cuenta =
                                                                            Convert.ToInt32(movNuevo.cuenta),
                                                                            centro = parametro.centro,
                                                                            nit = encabezado.nit,
                                                                            fec = DateTime.Now,
                                                                            debito = movNuevo.debito,
                                                                            debitoniif = movNuevo.debitoniif,
                                                                            basecontable =
                                                                            movNuevo.basecontable,
                                                                            credito = movNuevo.credito,
                                                                            creditoniif = movNuevo.creditoniif,
                                                                            fec_creacion = DateTime.Now,
                                                                            userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]),
                                                                            detalle =
                                                                            "Facturacion de orden de taller N° " +
                                                                            ordentaller.codigoentrada +
                                                                            " con consecutivo " + eg.numero,
                                                                            estado = true
                                                                        };
                                                                        /*context.mov_contable.Add(crearMovContable);
                                                                        context.SaveChanges();*/
                                                                    }
                                                                }
                                                            }

                                                            #endregion

                                                            #region Ingreso	

                                                            //var siva = Request["tipo_tarifa_hidden_" + i] == "2";
                                                            bool siva = listaelementos[i].tipo_tarifa == 2;
                                                            if (parametro.id_nombre_parametro == 11 && siva != true)
                                                            {
                                                                icb_referencia perfilReferencia =
                                                                    context.icb_referencia.FirstOrDefault(x =>
                                                                        x.ref_codigo == codigo);
                                                                int perfilBuscar = 0;
                                                                if (perfilReferencia != null &&
                                                                    listaelementos[i].tipo == "R")
                                                                {
                                                                    perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                                                }

                                                                perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(
                                                                    r => r.id == perfilBuscar);

                                                                #region Tiene perfil la referencia

                                                                if (pcr != null)
                                                                {
                                                                    int? cuentaVenta = pcr.cuenta_ventas;

                                                                    movNuevo.id_encab = encabezado.idencabezado;
                                                                    movNuevo.seq = secuencia;
                                                                    movNuevo.idparametronombre =
                                                                        parametro.id_nombre_parametro;

                                                                    #region tiene perfil y cuenta asignada al perfil

                                                                    if (cuentaVenta != null)
                                                                    {
                                                                        movNuevo.cuenta = Convert.ToInt32(cuentaVenta);
                                                                        //movNuevo.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 11 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro; ;
                                                                        movNuevo.centro = listaelementos[i].tipo_tarifa == 2
                                                                            ? parametro.id_nombre_parametro == 11
                                                                                ?
                                                                                listaelementos[i].centro_costo
                                                                                : parametro.centro
                                                                            : parametro.centro;
                                                                        ;

                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        movNuevo.documento = Convert.ToString(eg.numero);

                                                                        cuenta_puc infoReferencia = context.cuenta_puc
                                                                            .Where(a => a.cntpuc_id == cuentaVenta)
                                                                            .FirstOrDefault();
                                                                        if (infoReferencia.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (infoReferencia.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (infoReferencia.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debito = 0;

                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debito = 0;
                                                                        }

                                                                        //context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion

                                                                    #region tiene perfil pero no tiene cuenta asignada

                                                                    else
                                                                    {
                                                                        movNuevo.cuenta = parametro.cuenta;
                                                                        //movNuevo.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 11 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro; ;
                                                                        movNuevo.centro = listaelementos[i].tipo_tarifa == 2
                                                                            ? parametro.id_nombre_parametro == 11
                                                                                ?
                                                                                listaelementos[i].centro_costo
                                                                                : parametro.centro
                                                                            : parametro.centro;
                                                                        ;

                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        movNuevo.documento = Convert.ToString(eg.numero);

                                                                        cuenta_puc infoReferencia = context.cuenta_puc
                                                                            .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                            .FirstOrDefault();
                                                                        if (infoReferencia.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(valor_totalenca, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (infoReferencia.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (infoReferencia.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debito = 0;

                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                            movNuevo.debito = 0;
                                                                        }

                                                                        //context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion
                                                                }

                                                                #endregion

                                                                #region La referencia no tiene perfil

                                                                else
                                                                {
                                                                    movNuevo.id_encab = encabezado.idencabezado;
                                                                    movNuevo.seq = secuencia;
                                                                    movNuevo.idparametronombre =
                                                                        parametro.id_nombre_parametro;
                                                                    movNuevo.cuenta = parametro.cuenta;
                                                                    //movNuevo.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 11 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro;
                                                                    movNuevo.centro = listaelementos[i].tipo_tarifa == 2
                                                                        ? parametro.id_nombre_parametro == 11
                                                                            ?
                                                                            listaelementos[i].centro_costo
                                                                            : parametro.centro
                                                                        : parametro.centro;

                                                                    movNuevo.fec = DateTime.Now;
                                                                    movNuevo.fec_creacion = DateTime.Now;
                                                                    movNuevo.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);
                                                                    /*if (info.aplicaniff==true)
                                                                    {

                                                                    }*/

                                                                    if (info.manejabase)
                                                                    {
                                                                        movNuevo.basecontable =
                                                                            Convert.ToDecimal(valor_totalenca, miCultura);
                                                                    }
                                                                    else
                                                                    {
                                                                        movNuevo.basecontable = 0;
                                                                    }

                                                                    if (info.documeto)
                                                                    {
                                                                        movNuevo.documento = Convert.ToString(eg.numero);
                                                                    }

                                                                    if (buscarCuenta.concepniff == 1)
                                                                    {
                                                                        movNuevo.credito = Convert.ToDecimal(baseUnitario, miCultura);
                                                                        movNuevo.debito = 0;

                                                                        movNuevo.creditoniif =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (buscarCuenta.concepniff == 4)
                                                                    {
                                                                        movNuevo.creditoniif =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (buscarCuenta.concepniff == 5)
                                                                    {
                                                                        movNuevo.credito = Convert.ToDecimal(baseUnitario, miCultura);
                                                                        movNuevo.debito = 0;
                                                                    }

                                                                    //context.mov_contable.Add(movNuevo);
                                                                }

                                                                #endregion

                                                                mov_contable buscarVenta = context.mov_contable.FirstOrDefault(x =>
                                                                    x.id_encab == id_encabezado &&
                                                                    x.cuenta == movNuevo.cuenta &&
                                                                    x.idparametronombre == parametro.id_nombre_parametro);
                                                                if (buscarVenta != null)
                                                                {
                                                                    buscarVenta.basecontable += movNuevo.basecontable;
                                                                    buscarVenta.debito += movNuevo.debito;
                                                                    buscarVenta.debitoniif += movNuevo.debitoniif;
                                                                    buscarVenta.credito += movNuevo.credito;
                                                                    buscarVenta.creditoniif += movNuevo.creditoniif;
                                                                    //context.Entry(buscarVenta).State = EntityState.Modified;
                                                                }
                                                                else
                                                                {
                                                                    mov_contable crearMovContable = new mov_contable
                                                                    {
                                                                        id_encab = encabezado.idencabezado,
                                                                        seq = secuencia,
                                                                        idparametronombre =
                                                                        parametro.id_nombre_parametro,
                                                                        cuenta =
                                                                        Convert.ToInt32(movNuevo.cuenta),
                                                                        //crearMovContable.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 11 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro;
                                                                        centro =
                                                                        listaelementos[i].tipo_tarifa == 2
                                                                            ? parametro.id_nombre_parametro == 11
                                                                                ?
                                                                                listaelementos[i].centro_costo
                                                                                : parametro.centro
                                                                            : parametro.centro,

                                                                        nit = encabezado.nit,
                                                                        fec = DateTime.Now,
                                                                        debito = movNuevo.debito,
                                                                        debitoniif = movNuevo.debitoniif,
                                                                        basecontable = movNuevo.basecontable,
                                                                        credito = movNuevo.credito,
                                                                        creditoniif = movNuevo.creditoniif,
                                                                        fec_creacion = DateTime.Now,
                                                                        userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]),
                                                                        detalle =
                                                                        "Facturacion de Orden de Taller N° " +
                                                                        ordentaller.codigoentrada + " con consecutivo " +
                                                                        eg.numero,
                                                                        estado = true
                                                                    };
                                                                    //context.mov_contable.Add(crearMovContable);
                                                                    //context.SaveChanges();
                                                                }
                                                            }

                                                            #endregion

                                                            #region Costo

                                                            if (listaelementos[i].tipo == "R")
                                                            {
                                                                if (parametro.id_nombre_parametro == 12)
                                                                {
                                                                    icb_referencia perfilReferencia =
                                                                        context.icb_referencia.FirstOrDefault(x =>
                                                                            x.ref_codigo == codigo);
                                                                    int perfilBuscar = 0;
                                                                    if (listaelementos[i].tipo == "R")
                                                                    {
                                                                        perfilBuscar =
                                                                            Convert.ToInt32(perfilReferencia.perfil);
                                                                    }

                                                                    perfilcontable_referencia pcr = context.perfilcontable_referencia
                                                                        .FirstOrDefault(r => r.id == perfilBuscar);

                                                                    #region Tiene perfil la referencia

                                                                    if (pcr != null)
                                                                    {
                                                                        int? cuentaCosto = pcr.cuenta_costo;

                                                                        movNuevo.id_encab = encabezado.idencabezado;
                                                                        movNuevo.seq = secuencia;
                                                                        movNuevo.idparametronombre =
                                                                            parametro.id_nombre_parametro;

                                                                        #region tiene perfil y cuenta asignada al perfil

                                                                        if (cuentaCosto != null)
                                                                        {
                                                                            movNuevo.cuenta = Convert.ToInt32(cuentaCosto);
                                                                            //movNuevo.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 12 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro;
                                                                            movNuevo.centro =
                                                                                listaelementos[i].tipo_tarifa == 2
                                                                                    ? parametro.id_nombre_parametro == 12
                                                                                        ?
                                                                                        listaelementos[i].centro_costo
                                                                                        : parametro.centro
                                                                                    : parametro.centro;

                                                                            movNuevo.fec = DateTime.Now;
                                                                            movNuevo.fec_creacion = DateTime.Now;
                                                                            movNuevo.userid_creacion =
                                                                                Convert.ToInt32(Session["user_usuarioid"]);
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);

                                                                            cuenta_puc infoReferencia = context.cuenta_puc
                                                                                .Where(a => a.cntpuc_id == cuentaCosto)
                                                                                .FirstOrDefault();
                                                                            if (infoReferencia.manejabase)
                                                                            {
                                                                                movNuevo.basecontable =
                                                                                    Convert.ToDecimal(valor_totalenca, miCultura);
                                                                            }
                                                                            else
                                                                            {
                                                                                movNuevo.basecontable = 0;
                                                                            }

                                                                            if (infoReferencia.documeto)
                                                                            {
                                                                                movNuevo.documento =
                                                                                    Convert.ToString(eg.numero);
                                                                            }

                                                                            if (infoReferencia.concepniff == 1)
                                                                            {
                                                                                movNuevo.credito = 0;
                                                                                movNuevo.debito = Convert.ToDecimal(cr, miCultura);

                                                                                movNuevo.creditoniif = 0;
                                                                                movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                            }

                                                                            if (infoReferencia.concepniff == 4)
                                                                            {
                                                                                movNuevo.creditoniif = 0;
                                                                                movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                            }

                                                                            if (infoReferencia.concepniff == 5)
                                                                            {
                                                                                movNuevo.credito = 0;
                                                                                movNuevo.debito = Convert.ToDecimal(cr, miCultura);
                                                                            }

                                                                            //context.mov_contable.Add(movNuevo);
                                                                        }

                                                                        #endregion

                                                                        #region tiene perfil pero no tiene cuenta asignada

                                                                        else
                                                                        {
                                                                            movNuevo.cuenta = parametro.cuenta;
                                                                            //movNuevo.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 12 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro; ;
                                                                            movNuevo.centro =
                                                                                listaelementos[i].tipo_tarifa == 2
                                                                                    ? parametro.id_nombre_parametro == 12
                                                                                        ?
                                                                                        listaelementos[i].centro_costo
                                                                                        : parametro.centro
                                                                                    : parametro.centro;

                                                                            movNuevo.fec = DateTime.Now;
                                                                            movNuevo.fec_creacion = DateTime.Now;
                                                                            movNuevo.userid_creacion =
                                                                                Convert.ToInt32(Session["user_usuarioid"]);
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);

                                                                            cuenta_puc infoReferencia = context.cuenta_puc
                                                                                .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                                .FirstOrDefault();
                                                                            if (infoReferencia.manejabase)
                                                                            {
                                                                                movNuevo.basecontable =
                                                                                    Convert.ToDecimal(valor_totalenca, miCultura);
                                                                            }
                                                                            else
                                                                            {
                                                                                movNuevo.basecontable = 0;
                                                                            }

                                                                            if (infoReferencia.documeto)
                                                                            {
                                                                                movNuevo.documento =
                                                                                    Convert.ToString(eg.numero);
                                                                            }

                                                                            if (infoReferencia.concepniff == 1)
                                                                            {
                                                                                movNuevo.credito = 0;
                                                                                movNuevo.debito = Convert.ToDecimal(cr, miCultura);

                                                                                movNuevo.creditoniif = 0;
                                                                                movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                            }

                                                                            if (infoReferencia.concepniff == 4)
                                                                            {
                                                                                movNuevo.creditoniif = 0;
                                                                                movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                            }

                                                                            if (infoReferencia.concepniff == 5)
                                                                            {
                                                                                movNuevo.credito = 0;
                                                                                movNuevo.debito = Convert.ToDecimal(cr, miCultura);
                                                                            }

                                                                            //context.mov_contable.Add(movNuevo);
                                                                        }

                                                                        #endregion
                                                                    }

                                                                    #endregion

                                                                    #region La referencia no tiene perfil

                                                                    else
                                                                    {
                                                                        movNuevo.id_encab = encabezado.idencabezado;
                                                                        movNuevo.seq = secuencia;
                                                                        movNuevo.idparametronombre =
                                                                            parametro.id_nombre_parametro;
                                                                        movNuevo.cuenta = parametro.cuenta;
                                                                        //movNuevo.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 12 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro;
                                                                        movNuevo.centro = listaelementos[i].tipo_tarifa == 2
                                                                            ? parametro.id_nombre_parametro == 12
                                                                                ?
                                                                                listaelementos[i].centro_costo
                                                                                : parametro.centro
                                                                            : parametro.centro;

                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        /*if (info.aplicaniff==true)
                                                                        {

                                                                        }*/

                                                                        if (info.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(valor_totalenca, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (info.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (buscarCuenta.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito = 0;
                                                                            movNuevo.debito = Convert.ToDecimal(cr, miCultura);

                                                                            movNuevo.creditoniif = 0;
                                                                            movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                        }

                                                                        if (buscarCuenta.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif = 0;
                                                                            movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                        }

                                                                        if (buscarCuenta.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito = 0;
                                                                            movNuevo.debito = Convert.ToDecimal(cr, miCultura);
                                                                        }

                                                                        //context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion

                                                                    mov_contable buscarCosto =
                                                                        context.mov_contable.FirstOrDefault(x =>
                                                                            x.id_encab == id_encabezado &&
                                                                            x.cuenta == movNuevo.cuenta &&
                                                                            x.idparametronombre ==
                                                                            parametro.id_nombre_parametro);
                                                                    if (buscarCosto != null)
                                                                    {
                                                                        buscarCosto.basecontable += movNuevo.basecontable;
                                                                        buscarCosto.debito += movNuevo.debito;
                                                                        buscarCosto.debitoniif += movNuevo.debitoniif;
                                                                        buscarCosto.credito += movNuevo.credito;
                                                                        buscarCosto.creditoniif += movNuevo.creditoniif;
                                                                        //context.Entry(buscarCosto).State = EntityState.Modified;

                                                                    }
                                                                    else
                                                                    {
                                                                        mov_contable crearMovContable = new mov_contable
                                                                        {
                                                                            id_encab = encabezado.idencabezado,
                                                                            seq = secuencia,
                                                                            idparametronombre =
                                                                            parametro.id_nombre_parametro,
                                                                            cuenta =
                                                                            Convert.ToInt32(movNuevo.cuenta),
                                                                            //crearMovContable.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 12 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro;
                                                                            centro =
                                                                            listaelementos[i].tipo_tarifa == 2
                                                                                ? parametro.id_nombre_parametro == 12
                                                                                    ?
                                                                                    listaelementos[i].centro_costo
                                                                                    : parametro.centro
                                                                                : parametro.centro,

                                                                            nit = encabezado.nit,
                                                                            fec = DateTime.Now,
                                                                            debito = movNuevo.debito,
                                                                            debitoniif = movNuevo.debitoniif,
                                                                            basecontable =
                                                                            movNuevo.basecontable,
                                                                            credito = movNuevo.credito,
                                                                            creditoniif = movNuevo.creditoniif,
                                                                            fec_creacion = DateTime.Now,
                                                                            userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]),
                                                                            detalle =
                                                                            "Facturacion de Orden de Taller N° " +
                                                                            ordentaller.codigoentrada +
                                                                            " con consecutivo " + eg.numero,
                                                                            estado = true
                                                                        };
                                                                        //context.mov_contable.Add(crearMovContable);
                                                                        //context.SaveChanges();
                                                                    }
                                                                }
                                                            }

                                                            #endregion

                                                            secuencia++;
                                                            //Cuentas valores

                                                            #region Cuentas valores

                                                            cuentas_valores buscar_cuentas_valores =
                                                                context.cuentas_valores.FirstOrDefault(x =>
                                                                    x.centro == parametro.centro &&
                                                                    x.cuenta == movNuevo.cuenta && x.nit == movNuevo.nit);
                                                            if (buscar_cuentas_valores != null)
                                                            {
                                                                buscar_cuentas_valores.debito +=
                                                                    Math.Round(movNuevo.debito);
                                                                buscar_cuentas_valores.credito +=
                                                                    Math.Round(movNuevo.credito);
                                                                buscar_cuentas_valores.debitoniff +=
                                                                    Math.Round(movNuevo.debitoniif);
                                                                buscar_cuentas_valores.creditoniff +=
                                                                    Math.Round(movNuevo.creditoniif);
                                                                //context.Entry(buscar_cuentas_valores).State = EntityState.Modified;

                                                                //context.SaveChanges();
                                                            }
                                                            else
                                                            {
                                                                DateTime fechaHoy = DateTime.Now;
                                                                cuentas_valores crearCuentaValor = new cuentas_valores
                                                                {
                                                                    ano = fechaHoy.Year,
                                                                    mes = fechaHoy.Month,
                                                                    cuenta = movNuevo.cuenta,
                                                                    //crearCuentaValor.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 11 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.id_nombre_parametro == 12 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : movNuevo.centro : movNuevo.centro; ;
                                                                    centro = listaelementos[i].tipo_tarifa == 2
                                                                    ? parametro.id_nombre_parametro == 12
                                                                        ?
                                                                        listaelementos[i].centro_costo
                                                                        : parametro.centro
                                                                    : parametro.centro,

                                                                    nit = movNuevo.nit,
                                                                    debito = Math.Round(movNuevo.debito),
                                                                    credito = Math.Round(movNuevo.credito),
                                                                    debitoniff =
                                                                    Math.Round(movNuevo.debitoniif),
                                                                    creditoniff =
                                                                    Math.Round(movNuevo.creditoniif)
                                                                };
                                                                //context.cuentas_valores.Add(crearCuentaValor);
                                                                //context.SaveChanges();
                                                            }

                                                            #endregion

                                                            totalCreditos += Math.Round(movNuevo.credito);
                                                            totalDebitos += Math.Round(movNuevo.debito);
                                                            listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                            {
                                                                NumeroCuenta =
                                                                    "(" + buscarCuenta.cntpuc_numero + ")" +
                                                                    buscarCuenta.cntpuc_descp,
                                                                DescripcionParametro = descripcionParametro,
                                                                ValorDebito = movNuevo.debito,
                                                                ValorCredito = movNuevo.credito
                                                            });
                                                        }
                                                    }
                                                }




                                                #endregion
                                            } //fin de recorrer linea por linea

                                            #region validaciones para guardar

                                            if (Math.Round(totalDebitos) != Math.Round(totalCreditos))
                                            {
                                                TempData["mensaje_error"] =
                                                    "El documento no tiene los movimientos calculados correctamente, verifique el perfil del documento";
                                                guardar = 0;
                                                dbTran.Rollback();
                                                listasliquidacion(modelo);
                                                BuscarFavoritos(menu);
                                                return View(modelo);
                                                //return RedirectToAction("Facturar", "FacturacionRepuestos", new { menu });
                                            }

                                            funciono = 1;

                                            #endregion

                                            if (funciono > 0)
                                            {
                                                cajas = 1;
                                                guardar = 1;
                                                context.SaveChanges();
                                           
                                                ordentaller.estadoorden = estadofacturada;

                                                ordentaller.fecha_liquidacion = DateTime.Now;
                                                context.Entry(ordentaller).State = EntityState.Modified;
                                                context.SaveChanges();
                                                TempData["mensaje"] = "Registro creado correctamente";
                                                DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
                                                doc.ActualizarConsecutivo(grupo.grupo, consecutivo);                                                
                                            }

                                            //#endregions


                                        }
                                        else
                                        {
                                            TempData["mensaje_error"] = "no hay consecutivo";
                                        }
                                    }                                   
                                    if (lista3 > 0 || lista4 > 0)
                                    {
                                        bool resultadoop = false, resultadorep=false;

                                        if (lista3 > 0)
                                            {
                                            resultadorep = RIrepuestos(modelo);
                                            }
                                        else {
                                            resultadorep =  true;
                                            }
                                        if (lista4 > 0)
                                            {
                                            resultadoop = RIoperaciones(modelo);
                                            }
                                        else {

                                            resultadoop = true;
                                            }
                                        if (resultadoop != true || resultadorep != true)
                                            {
                                            dbTran.Rollback();
                                            TempData["mensaje_error"] = "Error en los movimientos contables de las operaciones";

                                            }
                                        else {
                                            TempData["mensaje"] = "Registro creado correctamente";
                                            guardar = 1;
                                            if (cajas != 1)
                                                {
                                                var estado = context.tcitasestados.Where(x => x.tipoestado == "L").Select(x => x.id).FirstOrDefault();
                                                ordentaller.estadoorden = estado;
                                                }
                                          

                                            }


                                    }
                                    if (lista5 > 0 || lista6 > 0)
                                    {
                                      bool respuesta =  PreFactura(modelo);
                                        if (respuesta)
                                            {
                                            guardar = 1;
                                            }

                                    }
                                    if (guardar==1)
                                        {
                                        dbTran.Commit();
                                        cajas = 1;
                                        }

                                }
                                else
                                {
                                    TempData["mensaje_error"] = "Lista vacia";
                                }


                            }
                            else
                            {
                                TempData["mensaje_error"] =
                                    "Error en liquidación de orden " + modelo.codigoentrada +
                                    " Mensaje: No existe perfil contable para el documento seleccionado y la bodega: " +
                                    bodegaconce.bodccs_nombre;
                            }
                        }
                        catch (DbEntityValidationException ex)
                        {
                            string mensaje = ex.Message;
                            dbTran.Rollback();
                            TempData["mensaje_error"] =
                                "Error en liquidación de orden " + modelo.codigoentrada + " Mensaje:" + mensaje;
                        }



                    }
                    if (cajas == 1)
                        {
                        return RedirectToAction("BrowserCajas", "CentralAtencion");
                        }
                    else
                        {
                        return View(modelo);
                        }
                 
                }
                else
                {
                    TempData["mensaje_error"] = "Error en liquidación de orden " + modelo.codigoentrada +
                                                " : Debe llenar todos los campos obligatorios, así como seleccionar los tipos de tarifa de las operaciones y los repuestos.";
                }
                listasliquidacion(modelo);
                BuscarFavoritos(menu);

              

               
            }


            return RedirectToAction("Login", "Login");
        }

        public bool BuscarTipoOperacion(string idtempario, string tecnico) {

            bool result = false;

            var buscar = context.ttempario.Where(d => d.codigo == idtempario).Select(d => d.categoria).FirstOrDefault();

            if (buscar== tecnico)
            {
               return result = true;
            }

            return result;
        }

        public bool RIoperaciones(formularioliquidacionOT modelo) {

          
                int tipotarifa = Convert.ToInt32(context.icb_sysparameter.Where(s => s.syspar_cod == "P147").Select(z => z.syspar_value).FirstOrDefault());
                var operacionesinternas = context.tdetallemanoobraot.Where(x => x.tipotarifa == tipotarifa && x.respuestainterno == true && x.idorden == modelo.idorden).ToList();
                int valortransac = 0;
                bool respuesta = false;
                int empresa = Convert.ToInt32(context.icb_sysparameter.Where(s => s.syspar_cod == "P33").Select(z => z.syspar_value).FirstOrDefault());
                var tecnico = context.ttecnicos.Where(d => d.idusuario == modelo.tecnico).Select(d => d.ttipotecnico.tipo).FirstOrDefault();

            try
                    {


                    if (operacionesinternas != null)
                        {
                        int swclasifica = Convert.ToInt32(context.icb_sysparameter.Where(z => z.syspar_cod == "P148").Select(x => x.syspar_value).FirstOrDefault());
                        int documento = context.tp_doc_registros.Where(x => x.tp_doc_sw.sw == swclasifica).Select(e => e.tpdoc_id).FirstOrDefault();
                        grupoconsecutivos grupointer = context.grupoconsecutivos.FirstOrDefault(x => x.documento_id == documento && x.bodega_id == modelo.bodega);
                        DocumentoPorBodegaController docinter = new DocumentoPorBodegaController();
                        long consecutivointer = docinter.BuscarConsecutivo(grupointer.grupo);

                        encab_documento encabezadoretiro = new encab_documento();

                        encabezadoretiro.tipo = documento;
                        encabezadoretiro.numero = consecutivointer;
                        encabezadoretiro.nit = empresa;
                        encabezadoretiro.fecha = DateTime.Now;
                        encabezadoretiro.bodega = Convert.ToInt32(modelo.bodega);
                        encabezadoretiro.fec_creacion = DateTime.Now;
                        encabezadoretiro.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                        encabezadoretiro.valor_total = Convert.ToDecimal(Request["totaltotal"]);
                        encabezadoretiro.impoconsumo = 0;
                        encabezadoretiro.anulado = false;
                        encabezadoretiro.estado = true;
                        encabezadoretiro.orden_taller = modelo.idorden;
                        context.encab_documento.Add(encabezadoretiro);
                        context.SaveChanges();

                        foreach (var item in operacionesinternas)
                            {
                        lineas_documento_operaciones linea_operacion = new lineas_documento_operaciones
                        {
                            id_encabezado = encabezadoretiro.idencabezado,
                            codigo_operacion = item.idtempario,
                            fec = DateTime.Now,
                            nit = item.tencabezaorden.tercero,
                            tiempo = float.Parse(item.tiempo.ToString()),
                            porcentaje_iva = float.Parse(item.poriva.ToString()),
                            valor_unitario = item.valorunitario,
                            porcentaje_descuento = float.Parse(item.pordescuento.ToString()),
                            costo_unitario = Convert.ToDecimal(item.ttempario.costo),
                            bodega = item.tencabezaorden.bodega,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            estado = true,
                            iddetallemanoobra = item.id,
                            centro_costo = item.idcentro,
                            tecnico_pagar = BuscarTipoOperacion(item.idtempario, tecnico) != false ? Convert.ToDecimal(Convert.ToDecimal(item.ttempario.costo) * Convert.ToDecimal(item.ttempario.HoraCliente)) : 0,

                        };



                        context.lineas_documento_operaciones.Add(linea_operacion);
                            context.SaveChanges();
                            }
                        valortransac = 1;
                        if (valortransac > 0)
                            {
                            docinter.ActualizarConsecutivo(grupointer.grupo, consecutivointer);
                            }

                        CsMovientoCont MovConttOperaciones = new CsMovientoCont();
                        int user_id = Convert.ToInt32(Session["user_usuarioid"]);
                        int numot = Convert.ToInt32(modelo.idorden);
                        int bodega = Convert.ToInt32(modelo.bodega);

                        var perfilcont = context.perfil_contable_bodega.Where(x => x.perfil_contable_documento.tp_doc_registros.tp_doc_sw.sw == swclasifica && x.idbodega == bodega).FirstOrDefault();
                        int perfilcontable = perfilcont != null ? Convert.ToInt32(perfilcont.idperfil) : 0;
                        var respuesta2 = MovConttOperaciones.MovContableOperacionIn(perfilcontable, encabezadoretiro.idencabezado, encabezadoretiro.numero, encabezadoretiro.nit, numot, user_id);
                        if (respuesta2.valido == true)
                            {
                            //guardo cada uno de los movimientos contables en un foreach
                            foreach (var item in respuesta2.movimientos)
                                {
                                var movimiento = new mov_contable();
                                movimiento.id_encab = item.id_encab;
                                movimiento.seq = item.seq;
                                movimiento.idparametronombre = item.idparametronombre;
                                movimiento.cuenta = item.cuenta;
                                movimiento.tipo_tarifa = item.tipo_tarifa;
                                movimiento.centro = item.centro;
                                movimiento.basecontable = item.basecontable;
                                movimiento.fec = item.fec;
                                movimiento.fec_creacion = item.fec_creacion;
                                movimiento.tipo_tarifa = item.tipo_tarifa;
                                movimiento.userid_creacion = item.userid_creacion;
                                movimiento.documento = item.documento;
                                movimiento.nit = item.nit;
                                movimiento.basecontable = item.basecontable;
                                movimiento.credito = item.credito;
                                movimiento.creditoniif = item.creditoniif;
                                movimiento.debito = item.debito;
                                movimiento.debitoniif = item.debitoniif;
                                movimiento.estado = item.estado;
                                context.mov_contable.Add(movimiento);
                                context.SaveChanges();


                                #region Cuentas valores
                                DateTime fechaHoy = DateTime.Now;
                                var centrocosto = Convert.ToInt32(item.centro);
                                cuentas_valores buscar_cuentas_valores =
                                    context.cuentas_valores.FirstOrDefault(x =>
                                        x.centro == item.centro &&
                                        x.cuenta == item.cuenta && x.nit == item.nit && x.ano == fechaHoy.Year && x.mes == fechaHoy.Month);
                                if (buscar_cuentas_valores != null)
                                    {
                                    buscar_cuentas_valores.debito += Math.Round(item.debito);
                                    buscar_cuentas_valores.credito += Math.Round(item.credito);
                                    buscar_cuentas_valores.debitoniff +=
                                        Math.Round(item.debitoniif);
                                    buscar_cuentas_valores.creditoniff +=
                                        Math.Round(item.creditoniif);
                                    context.Entry(buscar_cuentas_valores).State =
                                        EntityState.Modified;
                                    }
                                else
                                    {
                                    cuentas_valores crearCuentaValor = new cuentas_valores
                                        {
                                        ano = fechaHoy.Year,
                                        mes = fechaHoy.Month,
                                        cuenta = item.cuenta,
                                        centro = item.centro,
                                        nit = item.nit,
                                        debito = Math.Round(item.debito),
                                        credito = Math.Round(item.credito),
                                        debitoniff = Math.Round(item.debitoniif),
                                        creditoniff = Math.Round(item.creditoniif)
                                        };


                                    context.cuentas_valores.Add(crearCuentaValor);
                                    context.SaveChanges();
                                    }





                                #endregion


                                }
                            //  verifico cuentas valores y los guardo
    
                            //guardo los movimientos contables y commit
                            respuesta = true;
                            }
                        else
                            {
                         
                            respuesta = false;

                            }




                        }
                    else
                        {
                        respuesta = true;
                        }
                    }
                catch (Exception ex)
                    {
            
                    throw ex;
                    }

                return respuesta;
                
            }

        public bool RIrepuestos(formularioliquidacionOT modelo) {
            bool respuesta2 = false;

             int tipotarifa = Convert.ToInt32(context.icb_sysparameter.Where(s => s.syspar_cod == "P147").Select(z => z.syspar_value).FirstOrDefault());
                var repuestos = context.tdetallerepuestosot.Where(x => x.idorden == modelo.idorden && x.tipotarifa == tipotarifa).ToList();
                var repuest = context.tsolicitudrepuestosot.Where(x => x.idorden == modelo.idorden && x.recibido == true).ToList();
                //int valreptransac = 0;
             
                int empresa = Convert.ToInt32(context.icb_sysparameter.Where(s => s.syspar_cod == "P33").Select(z => z.syspar_value).FirstOrDefault());
                try
                {
                    if (repuest != null && repuestos.Count > 0)
                    {
                        int swclasifica = Convert.ToInt32(context.icb_sysparameter.Where(z => z.syspar_cod == "P149").Select(x => x.syspar_value).FirstOrDefault());
                        int documento = context.tp_doc_registros.Where(x => x.tp_doc_sw.sw == swclasifica).Select(e => e.tpdoc_id).FirstOrDefault();
                        grupoconsecutivos grupointerrep = context.grupoconsecutivos.FirstOrDefault(x => x.documento_id == documento && x.bodega_id == modelo.bodega);
                        DocumentoPorBodegaController docinterrep = new DocumentoPorBodegaController();
                        long consecutivointerrep = docinterrep.BuscarConsecutivo(grupointerrep.grupo);

                        encab_documento docencabezado = new encab_documento();

                        docencabezado.tipo = documento;
                        docencabezado.numero = consecutivointerrep;
                        docencabezado.fecha = DateTime.Now;
                        docencabezado.nit = empresa;
                        docencabezado.bodega = Convert.ToInt32(modelo.bodega);
                        docencabezado.fec_creacion = DateTime.Now;
                        docencabezado.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                        docencabezado.valor_total = Convert.ToDecimal(Request["totaltotal"]);
                        docencabezado.impoconsumo = 0;
                        docencabezado.anulado = false;
                        docencabezado.orden_taller = modelo.idorden;
                        docencabezado.estado = true;

                        context.encab_documento.Add(docencabezado);
                        context.SaveChanges();

                        foreach (var item in repuestos)
                        {

                            lineas_documento linea_doc_rep = new lineas_documento
                            {
                                id_encabezado = docencabezado.idencabezado,
                                codigo = item.idrepuesto,

                                fec = DateTime.Now,
                                nit = item.tencabezaorden.tercero,
                                cantidad = item.cantidad,
                                porcentaje_iva = float.Parse(item.poriva.ToString()),
                                valor_unitario = item.valorunitario,
                                porcentaje_descuento = float.Parse(item.pordescto.ToString()),
                                costo_unitario = item.icb_referencia.costo_unitario,
                                bodega = item.tencabezaorden.bodega,
                                cantidad_und = 0,
                                cantidad_pedida = 0,
                                costo_unitario_sin = 0,
                                cantidad_devuelta = 0,
                                fec_creacion = DateTime.Now,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                estado = true,
                                costo_niff = 0,
                                id_tarifa_cliente= tipotarifa
                                };
                            context.lineas_documento.Add(linea_doc_rep);
                            context.SaveChanges();
                        }
                        //valreptransac = 1;

                        CsMovientoCont MovConttOperaciones = new CsMovientoCont();
                        int user_id = Convert.ToInt32(Session["user_usuarioid"]);
                        int numot = Convert.ToInt32(modelo.idorden);
                        int bodega = Convert.ToInt32(modelo.bodega);

                        var perfilcont = context.perfil_contable_bodega.Where(x => x.perfil_contable_documento.tp_doc_registros.tp_doc_sw.sw == swclasifica && x.idbodega == bodega).FirstOrDefault();   //5086;
                        int perfilcontable = perfilcont != null ? Convert.ToInt32(perfilcont.idperfil) : 0;
                       var  respuesta = MovConttOperaciones.MovContableRepuestoIn(perfilcontable, docencabezado.idencabezado, docencabezado.numero, docencabezado.nit, numot, user_id);
                        if (respuesta.valido == true)
                            {
                            //guardo cada uno de los movimientos contables en un foreach
                            foreach (var item in respuesta.movimientos)
                                {
                                var movimiento = new mov_contable();
                                movimiento.id_encab = item.id_encab;
                                movimiento.seq = item.seq;
                                movimiento.idparametronombre = item.idparametronombre;
                                movimiento.cuenta = item.cuenta;
                                movimiento.tipo_tarifa = item.tipo_tarifa;
                                movimiento.centro = item.centro;
                                movimiento.basecontable = item.basecontable;
                                movimiento.fec = item.fec;
                                movimiento.fec_creacion = item.fec_creacion;
                                movimiento.tipo_tarifa = item.tipo_tarifa;
                                movimiento.userid_creacion = item.userid_creacion;
                                movimiento.documento = item.documento;
                                movimiento.nit = item.nit;
                                movimiento.basecontable = item.basecontable;
                                movimiento.credito = item.credito;
                                movimiento.creditoniif = item.creditoniif;
                                movimiento.debito = item.debito;
                                movimiento.debitoniif = item.debitoniif;
                                movimiento.estado = item.estado;
                                context.mov_contable.Add(movimiento);
                                context.SaveChanges();


                                #region Cuentas valores
                                DateTime fechaHoy = DateTime.Now;
                                var centrocosto = Convert.ToInt32(item.centro);
                                cuentas_valores buscar_cuentas_valores =
                                    context.cuentas_valores.FirstOrDefault(x =>
                                        x.centro == item.centro &&
                                        x.cuenta == item.cuenta && x.nit == item.nit && x.ano == fechaHoy.Year && x.mes == fechaHoy.Month);
                                if (buscar_cuentas_valores != null)
                                    {
                                    buscar_cuentas_valores.debito += Math.Round(item.debito);
                                    buscar_cuentas_valores.credito += Math.Round(item.credito);
                                    buscar_cuentas_valores.debitoniff +=
                                        Math.Round(item.debitoniif);
                                    buscar_cuentas_valores.creditoniff +=
                                        Math.Round(item.creditoniif);
                                    context.Entry(buscar_cuentas_valores).State =
                                        EntityState.Modified;
                                    }
                                else
                                    {
                                    cuentas_valores crearCuentaValor = new cuentas_valores
                                        {
                                        ano = fechaHoy.Year,
                                        mes = fechaHoy.Month,
                                        cuenta = item.cuenta,
                                        centro = item.centro,
                                        nit = item.nit,
                                        debito = Math.Round(item.debito),
                                        credito = Math.Round(item.credito),
                                        debitoniff = Math.Round(item.debitoniif),
                                        creditoniff = Math.Round(item.creditoniif)
                                        };


                                    context.cuentas_valores.Add(crearCuentaValor);
                                    context.SaveChanges();
                                    }





                                #endregion


                                }
                   
                            respuesta2 = true;
                            }
                        else
                            {
                    
                            respuesta2 = false;

                            }


                        }
                }
                catch (Exception ex)
                {

                    throw ex;
              
                }
            

            return respuesta2;
            }

        public bool PreFactura(formularioliquidacionOT modelo)
        {
            bool respuesta = false;
            var repuestos = context.tdetallerepuestosot.Where(x => x.idorden == modelo.idorden && x.tipotarifa == 4).ToList();
            var repuest = context.tsolicitudrepuestosot.Where(x => x.idorden == modelo.idorden && x.recibido == true).ToList();

            var tecnico = context.ttecnicos.Where(d => d.idusuario == modelo.tecnico).Select(d => d.ttipotecnico.tipo).FirstOrDefault();

            var operacionesinternas = context.tdetallemanoobraot.Where(x => x.tipotarifa == 4 && x.respuestainterno == true && x.idorden == modelo.idorden).ToList();

            int valreptransac = 0;
            int empresa = Convert.ToInt32(context.icb_sysparameter.Where(s => s.syspar_cod == "P33").Select(z => z.syspar_value).FirstOrDefault());
            try
            {
                if (repuest != null && repuestos.Count > 0 && operacionesinternas != null)
                    {
                    int swclasifica = Convert.ToInt32(context.icb_sysparameter.Where(z => z.syspar_cod == "P150").Select(x => x.syspar_value).FirstOrDefault());
                    int documento = context.tp_doc_registros.Where(x => x.tp_doc_sw.sw == swclasifica).Select(e => e.tpdoc_id).FirstOrDefault();
                    grupoconsecutivos grupointerrep = context.grupoconsecutivos.FirstOrDefault(x => x.documento_id == documento && x.bodega_id == modelo.bodega);
                    DocumentoPorBodegaController docinterrep = new DocumentoPorBodegaController();
                    long consecutivointerrep = docinterrep.BuscarConsecutivo(grupointerrep.grupo);

                    encab_documento docencabezado = new encab_documento();

                    docencabezado.tipo = documento;
                    docencabezado.numero = consecutivointerrep;
                    docencabezado.fecha = DateTime.Now;
                    docencabezado.nit = empresa;
                    docencabezado.bodega = Convert.ToInt32(modelo.bodega);
                    docencabezado.fec_creacion = DateTime.Now;
                    docencabezado.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                    docencabezado.valor_total = Convert.ToDecimal(Request["totaltotal"]);
                    docencabezado.impoconsumo = 0;
                    docencabezado.anulado = false;
                    docencabezado.orden_taller = modelo.idorden;
                    docencabezado.estado = true;

                    context.encab_documento.Add(docencabezado);
                    context.SaveChanges();

                    foreach (var item in repuestos)
                        {

                        lineas_documento linea_doc_rep = new lineas_documento
                            {
                            id_encabezado = docencabezado.idencabezado,
                            codigo = item.idrepuesto,

                            fec = DateTime.Now,
                            nit = item.tencabezaorden.tercero,
                            cantidad = item.cantidad,
                            porcentaje_iva = float.Parse(item.poriva.ToString()),
                            valor_unitario = item.valorunitario,
                            porcentaje_descuento = float.Parse(item.pordescto.ToString()),
                            costo_unitario = item.icb_referencia.costo_unitario,
                            bodega = item.tencabezaorden.bodega,
                            cantidad_und = 0,
                            cantidad_pedida = 0,
                            costo_unitario_sin = 0,
                            cantidad_devuelta = 0,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            estado = true,
                            costo_niff = 0
                            };
                        context.lineas_documento.Add(linea_doc_rep);
                        context.SaveChanges();
                        }

                    foreach (var item in operacionesinternas)
                        {
                        lineas_documento_operaciones linea_operacion = new lineas_documento_operaciones
                            {
                            id_encabezado = docencabezado.idencabezado,
                            codigo_operacion = item.idtempario,
                            fec = DateTime.Now,
                            nit = item.tencabezaorden.tercero,
                            tiempo = float.Parse(item.tiempo.ToString()),
                            porcentaje_iva = float.Parse(item.poriva.ToString()),
                            valor_unitario = item.valorunitario,
                            porcentaje_descuento = float.Parse(item.pordescuento.ToString()),
                            costo_unitario = Convert.ToDecimal(item.ttempario.costo),
                            bodega = item.tencabezaorden.bodega,
                            fec_creacion = DateTime.Now,
                            userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                            estado = true,
                            iddetallemanoobra = item.id,
                            centro_costo = item.idcentro,
                            tecnico_pagar = BuscarTipoOperacion(item.idtempario, tecnico) != false ? Convert.ToDecimal(Convert.ToDecimal(item.ttempario.costo) * Convert.ToDecimal(item.ttempario.HoraCliente)) : 0,
                        };

                        context.lineas_documento_operaciones.Add(linea_operacion);
                        context.SaveChanges();


                        }

                    valreptransac = 1;
                    if (valreptransac > 0)
                        {
                        docinterrep.ActualizarConsecutivo(grupointerrep.grupo, consecutivointerrep);
                        respuesta = true;
                        }


                    }
                else {
                    respuesta = false;
                    }

            }
            catch (Exception ex)
            {

                throw ex;
            }


            return respuesta;
        }

        public ActionResult LiquidacionOTViejo(formularioliquidacionOT modelo, int? menu)
        {
            if (Session["user_usuarioid"] != null)
            {
                //valido si el modelo es válido
                if (ModelState.IsValid)
                {
                    //parametro orden de taller facturada
                    icb_sysparameter param1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P111").FirstOrDefault();
                    int estadofacturada = param1 != null ? Convert.ToInt32(param1.syspar_value) : 8;

                    //parametro de sistema accesorios
                    icb_sysparameter param2 = context.icb_sysparameter.Where(d => d.syspar_cod == "P116").FirstOrDefault();
                    int accesorios = param2 != null ? Convert.ToInt32(param2.syspar_value) : 6;

                    using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                    {
                        try
                        {
                            bodega_concesionario bodegaconce = context.bodega_concesionario.Where(d => d.id == modelo.bodega)
                                .FirstOrDefault();
                            //validaciones preliminares
                            int funciono = 0;
                            decimal totalCreditos = 0;
                            decimal totalDebitos = 0;
                            decimal costoPromedioTotal = 0;
                            //busco el perfil contable del tipo de documento y bodega

                            perfil_contable_documento perfilcontable = context.perfil_contable_documento.Where(d =>
                                d.perfil_contable_bodega.Where(e => e.idbodega == modelo.bodega).Count() > 0 &&
                                d.tipo == modelo.tipoDocumento).FirstOrDefault();
                            if (perfilcontable != null)
                            {
                                var parametrosCuentasVerificar = (from perfil in context.perfil_cuentas_documento
                                                                  join nombreParametro in context.paramcontablenombres
                                                                      on perfil.id_nombre_parametro equals nombreParametro.id
                                                                  join cuenta in context.cuenta_puc
                                                                      on perfil.cuenta equals cuenta.cntpuc_id
                                                                  where perfil.id_perfil == perfilcontable.id
                                                                  select new
                                                                  {
                                                                      perfil.id,
                                                                      perfil.id_nombre_parametro,
                                                                      perfil.cuenta,
                                                                      perfil.centro,
                                                                      perfil.id_perfil,
                                                                      nombreParametro.descripcion_parametro,
                                                                      cuenta.cntpuc_numero
                                                                  }).ToList();

                                int secuencia = 1;
                                //informacion de la orden de taller
                                tencabezaorden ordentaller = context.tencabezaorden.Where(d => d.id == modelo.idorden)
                                    .FirstOrDefault();

                                int exhibicion = 0;
                                if (ordentaller.razoningreso == accesorios)
                                {
                                    //busco si el vehiculo es de exhibicion
                                    vpedido existepedido = context.vpedido.Where(d => d.planmayor == ordentaller.placa)
                                        .FirstOrDefault();
                                    if (existepedido == null)
                                    {
                                        exhibicion = 1;
                                    }
                                }

                                //centro de costo cero
                                List<cuentas_valores> ids_cuentas_valores = new List<cuentas_valores>();
                                centro_costo centroValorCero = context.centro_costo.FirstOrDefault(x => x.pre_centcst == "0");
                                int idCentroCero = centroValorCero != null
                                    ? Convert.ToInt32(centroValorCero.centcst_id)
                                    : 0;

                                List<DocumentoDescuadradoModel> listaDescuadrados = new List<DocumentoDescuadradoModel>();
                                List<ElementosFacturacion> listaelementos = new List<ElementosFacturacion>();
                                List<tsolicitudrepuestosot> cantidadreferencias = context.tsolicitudrepuestosot
                                    .Where(d => d.idorden == modelo.idorden && d.recibido).ToList();

                                //sumo el costo total de los repuestos si el vehiculo no es de exhibicion. Si lo es, ni los tomo en cuenta.
                                if (exhibicion == 1)
                                {
                                    cantidadreferencias = cantidadreferencias.Take(0).ToList();
                                }

                                int costoLineas = cantidadreferencias.Count();
                                for (int i = 0; i < costoLineas; i++)
                                {
                                    string referencia = !string.IsNullOrWhiteSpace(cantidadreferencias[i].reemplazo)
                                        ? cantidadreferencias[i].reemplazo
                                        : cantidadreferencias[i].tdetallerepuestosot.idrepuesto;
                                    decimal costoReferencia = cantidadreferencias[i].tdetallerepuestosot.valorunitario;
                                    costoPromedioTotal +=
                                        Convert.ToDecimal(costoReferencia, miCultura) *
                                        cantidadreferencias[i].tdetallerepuestosot.cantidad;

                                    listaelementos.Add(new ElementosFacturacion
                                    {
                                        tipo = "R",
                                        cantidad = cantidadreferencias[i].tdetallerepuestosot.cantidad,
                                        centro_costo = cantidadreferencias[i].tdetallerepuestosot.idcentro ?? 0,
                                        codigo = cantidadreferencias[i].tdetallerepuestosot.idrepuesto,
                                        porcentaje_descuento = cantidadreferencias[i].tdetallerepuestosot.pordescto,
                                        porcentaje_iva = cantidadreferencias[i].tdetallerepuestosot.poriva,
                                        tipo_tarifa = cantidadreferencias[i].tdetallerepuestosot.tipotarifa ?? 1,
                                        valor_descuento = calculardescuentore(
                                            cantidadreferencias[i].tdetallerepuestosot.valorunitario,
                                            cantidadreferencias[i].tdetallerepuestosot.cantidad,
                                            cantidadreferencias[i].tdetallerepuestosot.pordescto),
                                        valor_iva = calcularivare(
                                            cantidadreferencias[i].tdetallerepuestosot.valorunitario,
                                            cantidadreferencias[i].tdetallerepuestosot.cantidad,
                                            cantidadreferencias[i].tdetallerepuestosot.pordescto,
                                            cantidadreferencias[i].tdetallerepuestosot.poriva),
                                        valor_unitario =
                                            cantidadreferencias[i].tdetallerepuestosot.valorunitario *
                                            cantidadreferencias[i].tdetallerepuestosot.cantidad
                                    });
                                }

                                //sumo el costo total de las operaciones
                                List<tdetallemanoobraot> cantidadoperaciones = context.tdetallemanoobraot
                                    .Where(d => d.idorden == modelo.idorden && d.estado == "1").ToList();
                                int costooperaciones = cantidadoperaciones.Count();
                                for (int i = 0; i < costooperaciones; i++)
                                {
                                    string operacion = cantidadoperaciones[i].idtempario;
                                    decimal costooperacion = cantidadoperaciones[i].valorunitario;
                                    costoPromedioTotal += costooperacion;

                                    listaelementos.Add(new ElementosFacturacion
                                    {
                                        tipo = "M",
                                        cantidad = 1,
                                        centro_costo = cantidadoperaciones[i].idcentro ?? 0,
                                        codigo = cantidadoperaciones[i].idtempario,
                                        porcentaje_descuento = cantidadoperaciones[i].pordescuento ?? 0,
                                        porcentaje_iva = cantidadoperaciones[i].poriva ?? 0,
                                        tipo_tarifa = cantidadoperaciones[i].tipotarifa ?? 1,
                                        valor_descuento = calculardescuento(cantidadoperaciones[i].valorunitario,
                                            cantidadoperaciones[i].pordescuento),
                                        valor_iva = calculariva(cantidadoperaciones[i].valorunitario,
                                            cantidadoperaciones[i].pordescuento, cantidadoperaciones[i].poriva),
                                        valor_unitario = cantidadoperaciones[i].valorunitario
                                    });
                                }


                                int? bodega = modelo.bodega;
                                int lista = cantidadreferencias.Count();
                                int lista2 = cantidadoperaciones.Count();
                                if (lista > 0 || lista2 > 0)
                                {
                                    int datos = Convert.ToInt32(lista);
                                    decimal costoTotal =
                                        Convert.ToDecimal(Request["totaltotal"], miCultura); //costo con retenciones y fletes
                                    decimal ivaEncabezado = Convert.ToDecimal(Request["totaliva"], miCultura); //valor total del iva
                                    decimal descuentoEncabezado =
                                        Convert.ToDecimal(Request["totaldescuentos"], miCultura); //valor total del descuento
                                    decimal costoEncabezado =
                                        Convert.ToDecimal(Request["subtotal"], miCultura); //valor antes de impuestos
                                                                                           //esto es verdad? 
                                    decimal valor_totalenca = costoEncabezado - descuentoEncabezado;

                                    tercero_cliente tercero_cliente = context.tercero_cliente
                                        .Where(d => d.icb_terceros.tercero_id == modelo.id_cliente).FirstOrDefault();
                                    int? condicion = tercero_cliente.cod_pago_id;
                                    //consecutivo
                                    grupoconsecutivos grupo = context.grupoconsecutivos.FirstOrDefault(x =>
                                        x.documento_id == modelo.tipoDocumento && x.bodega_id == bodega);
                                    if (grupo != null)
                                    {
                                        DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                                        long consecutivo = doc.BuscarConsecutivo(grupo.grupo);

                                        //Encabezado documento

                                        #region encabezado

                                        encab_documento encabezado = new encab_documento
                                        {
                                            tipo = modelo.tipoDocumento.Value,
                                            numero = consecutivo,
                                            nit = modelo.id_cliente.Value,
                                            fecha = DateTime.Now,
                                            fpago_id = condicion,
                                            centro_doc = Convert.ToInt32(modelo.cartera)
                                        };
                                        int dias = context.fpago_tercero.Find(condicion).dvencimiento ?? 0;
                                        DateTime vencimiento = DateTime.Now.AddDays(dias);
                                        encabezado.vencimiento = vencimiento;
                                        encabezado.valor_total = costoTotal;
                                        encabezado.iva = ivaEncabezado;
                                        encabezado.orden_taller = modelo.idorden;
                                        // Validacion para reteIVA, reteICA y retencion dependiendo del proveedor seleccionado

                                        #region calculo de retenciones

                                        tp_doc_registros buscarTipoDocRegistro =
                                            context.tp_doc_registros.FirstOrDefault(x =>
                                                x.tpdoc_id == modelo.tipoDocumento.Value);
                                        tercero_cliente buscarCliente =
                                            context.tercero_cliente.FirstOrDefault(x =>
                                                x.tercero_id == modelo.id_cliente);
                                        int? regimen_cliente = buscarCliente != null ? buscarCliente.tpregimen_id : 0;
                                        perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                                            x.bodega == bodega && x.sw == buscarTipoDocRegistro.sw &&
                                            x.tipo_regimenid == regimen_cliente);

                                        decimal retenciones = 0;

                                        if (buscarPerfilTributario != null)
                                        {
                                            if (buscarPerfilTributario.retfuente == "A" &&
                                                valor_totalenca >= buscarTipoDocRegistro.baseretencion)
                                            {
                                                encabezado.porcen_retencion = buscarTipoDocRegistro.retencion;
                                                encabezado.retencion =
                                                    Math.Round(valor_totalenca *
                                                               Convert.ToDecimal(
                                                                   buscarTipoDocRegistro.retencion / 100, miCultura));
                                                retenciones += encabezado.retencion;
                                            }

                                            if (buscarPerfilTributario.retiva == "A" &&
                                                ivaEncabezado >= buscarTipoDocRegistro.baseiva)
                                            {
                                                encabezado.porcen_reteiva = buscarTipoDocRegistro.retiva;
                                                encabezado.retencion_iva =
                                                    Math.Round(encabezado.iva *
                                                               Convert.ToDecimal(buscarTipoDocRegistro.retiva / 100, miCultura));
                                                retenciones += encabezado.retencion_iva;
                                            }

                                            if (buscarPerfilTributario.autorretencion == "A")
                                            {
                                                decimal tercero_acteco = buscarCliente.acteco_tercero.autorretencion;
                                                encabezado.porcen_autorretencion = (float)tercero_acteco;
                                                encabezado.retencion_causada =
                                                    Math.Round(
                                                        valor_totalenca * Convert.ToDecimal(tercero_acteco / 100, miCultura));
                                                retenciones += encabezado.retencion_causada;
                                            }

                                            if (buscarPerfilTributario.retica == "A" &&
                                                valor_totalenca >= buscarTipoDocRegistro.baseica)
                                            {
                                                terceros_bod_ica bodega_acteco = context.terceros_bod_ica.FirstOrDefault(x =>
                                                    x.idcodica == buscarCliente.actividadEconomica_id &&
                                                    x.bodega == bodega);
                                                decimal tercero_acteco = buscarCliente.acteco_tercero.tarifa;
                                                if (bodega_acteco != null)
                                                {
                                                    encabezado.porcen_retica = (float)bodega_acteco.porcentaje;
                                                    encabezado.retencion_ica =
                                                        Math.Round(valor_totalenca *
                                                                   Convert.ToDecimal(bodega_acteco.porcentaje / 1000, miCultura));
                                                    retenciones += encabezado.retencion_ica;
                                                }

                                                if (tercero_acteco != 0)
                                                {
                                                    encabezado.porcen_retica =
                                                        (float)buscarCliente.acteco_tercero.tarifa;
                                                    encabezado.retencion_ica =
                                                        Math.Round(valor_totalenca *
                                                                   Convert.ToDecimal(
                                                                       buscarCliente.acteco_tercero.tarifa / 1000, miCultura));
                                                    retenciones += encabezado.retencion_ica;
                                                }
                                                else
                                                {
                                                    encabezado.porcen_retica = buscarTipoDocRegistro.retica;
                                                    encabezado.retencion_ica =
                                                        Math.Round(valor_totalenca *
                                                                   Convert.ToDecimal(
                                                                       buscarTipoDocRegistro.retica / 1000, miCultura));
                                                    retenciones += encabezado.retencion_ica;
                                                }
                                            }
                                        }

                                        #endregion

                                        encabezado.costo = costoPromedioTotal;
                                        //para efectos de la facturacion, quien es el vendedor?
                                        encabezado.vendedor = Convert.ToInt32(Request["vendedor"]);
                                        encabezado.perfilcontable = perfilcontable.id;
                                        encabezado.bodega = modelo.bodega.Value;
                                        monedas moneda2 = context.monedas.Where(d => d.estado).FirstOrDefault();
                                        int moneda = 1;
                                        if (moneda2 != null)
                                        {
                                            moneda = moneda2.moneda;
                                        }

                                        encabezado.moneda = moneda;
                                        encabezado.valor_mercancia = valor_totalenca;
                                        encabezado.fec_creacion = DateTime.Now;
                                        encabezado.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                        encabezado.estado = true;
                                        context.encab_documento.Add(encabezado);
                                        context.SaveChanges();

                                        #endregion

                                        int id_encabezado = context.encab_documento
                                            .OrderByDescending(x => x.idencabezado).FirstOrDefault().idencabezado;
                                        encab_documento eg = context.encab_documento.FirstOrDefault(x =>
                                            x.idencabezado == id_encabezado);

                                        //Mov Contable

                                        #region movimientos contables

                                        //buscamos en perfil cuenta documento, por medio del perfil seleccionado
                                        foreach (var parametro in parametrosCuentasVerificar)
                                        {
                                            string descripcionParametro = context.paramcontablenombres
                                                .FirstOrDefault(x => x.id == parametro.id_nombre_parametro)
                                                .descripcion_parametro;
                                            cuenta_puc buscarCuenta =
                                                context.cuenta_puc.FirstOrDefault(x => x.cntpuc_id == parametro.cuenta);

                                            if (buscarCuenta != null)
                                            {
                                                if (parametro.id_nombre_parametro == 10 &&
                                                    Convert.ToDecimal(valor_totalenca, miCultura) != 0
                                                    || parametro.id_nombre_parametro == 3 &&
                                                    Convert.ToDecimal(eg.retencion, miCultura) != 0
                                                    || parametro.id_nombre_parametro == 4 &&
                                                    Convert.ToDecimal(eg.retencion_iva, miCultura) != 0
                                                    || parametro.id_nombre_parametro == 5 &&
                                                    Convert.ToDecimal(eg.retencion_ica, miCultura) != 0
                                                    || parametro.id_nombre_parametro == 6 &&
                                                    Convert.ToDecimal(eg.fletes, miCultura) != 0
                                                    || parametro.id_nombre_parametro == 14 &&
                                                    Convert.ToDecimal(eg.iva_fletes, miCultura) != 0
                                                    || parametro.id_nombre_parametro == 17 &&
                                                    Convert.ToDecimal(eg.retencion_causada, miCultura) != 0
                                                    || parametro.id_nombre_parametro == 18 &&
                                                    Convert.ToDecimal(eg.retencion_causada, miCultura) != 0)
                                                {
                                                    mov_contable movNuevo = new mov_contable
                                                    {
                                                        id_encab = eg.idencabezado,
                                                        seq = secuencia,
                                                        idparametronombre = parametro.id_nombre_parametro,
                                                        cuenta = parametro.cuenta,
                                                        centro = parametro.centro,
                                                        fec = DateTime.Now,
                                                        fec_creacion = DateTime.Now,
                                                        userid_creacion =
                                                        Convert.ToInt32(Session["user_usuarioid"]),
                                                        documento = Convert.ToString(modelo.idorden),
                                                        detalle =
                                                        "Facturacion de orden de talle N° " +
                                                        ordentaller.codigoentrada + " con consecutivo " + eg.numero,
                                                        estado = true
                                                    };

                                                    cuenta_puc info = context.cuenta_puc
                                                        .Where(a => a.cntpuc_id == parametro.cuenta).FirstOrDefault();

                                                    if (info.tercero)
                                                    {
                                                        movNuevo.nit = ordentaller.tercero;
                                                    }
                                                    else
                                                    {
                                                        icb_terceros tercero = context.icb_terceros
                                                            .Where(t => t.doc_tercero == "0").FirstOrDefault();
                                                        movNuevo.nit = tercero.tercero_id;
                                                    }

                                                    // las siguientes validaciones se hacen dependiendo de la cuenta que esta moviendo la compra manual, para guardar la informacion acorde

                                                    #region Cuentas X Cobrar

                                                    if (parametro.id_nombre_parametro == 10)
                                                    {
                                                        /*if (info.aplicaniff==true)
														{

														}*/

                                                        if (info.manejabase)
                                                        {
                                                            movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                                        }
                                                        else
                                                        {
                                                            movNuevo.basecontable = 0;
                                                        }

                                                        if (info.documeto)
                                                        {
                                                            //no estoy seguro de que aquí tenga que ir el id de la orden. Usualmente este campo, si se llena, está para recibir el id del pedido
                                                            movNuevo.documento = Convert.ToString(modelo.idorden);
                                                        }

                                                        if (buscarCuenta.concepniff == 1)
                                                        {
                                                            movNuevo.credito = 0;
                                                            movNuevo.debito = Convert.ToDecimal(costoTotal, miCultura);

                                                            movNuevo.creditoniif = 0;
                                                            movNuevo.debitoniif = Convert.ToDecimal(costoTotal, miCultura);
                                                        }

                                                        if (buscarCuenta.concepniff == 4)
                                                        {
                                                            movNuevo.creditoniif = 0;
                                                            movNuevo.debitoniif = Convert.ToDecimal(costoTotal, miCultura);
                                                        }

                                                        if (buscarCuenta.concepniff == 5)
                                                        {
                                                            movNuevo.credito = 0;
                                                            movNuevo.debito = Convert.ToDecimal(costoTotal, miCultura);
                                                        }
                                                    }

                                                    #endregion

                                                    #region Retencion

                                                    if (parametro.id_nombre_parametro == 3)
                                                    {
                                                        /*if (info.aplicaniff==true)
														{

														}*/

                                                        if (info.manejabase)
                                                        {
                                                            movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                                        }
                                                        else
                                                        {
                                                            movNuevo.basecontable = 0;
                                                        }

                                                        if (info.documeto)
                                                        {
                                                            movNuevo.documento = modelo.documento;
                                                        }

                                                        if (buscarCuenta.concepniff == 1)
                                                        {
                                                            movNuevo.credito = 0;
                                                            movNuevo.debito = eg.retencion;

                                                            movNuevo.creditoniif = 0;
                                                            movNuevo.debitoniif = eg.retencion;
                                                        }

                                                        if (buscarCuenta.concepniff == 4)
                                                        {
                                                            movNuevo.creditoniif = 0;
                                                            movNuevo.debitoniif = eg.retencion;
                                                        }

                                                        if (buscarCuenta.concepniff == 5)
                                                        {
                                                            movNuevo.credito = 0;
                                                            movNuevo.debito = eg.retencion;
                                                        }
                                                    }

                                                    #endregion

                                                    #region ReteIVA

                                                    if (parametro.id_nombre_parametro == 4)
                                                    {
                                                        /*if (info.aplicaniff==true)
														{

														}*/

                                                        if (info.manejabase)
                                                        {
                                                            movNuevo.basecontable = Convert.ToDecimal(ivaEncabezado, miCultura);
                                                        }
                                                        else
                                                        {
                                                            movNuevo.basecontable = 0;
                                                        }

                                                        if (info.documeto)
                                                        {
                                                            movNuevo.documento = modelo.documento;
                                                        }

                                                        if (buscarCuenta.concepniff == 1)
                                                        {
                                                            movNuevo.credito = 0;
                                                            movNuevo.debito = eg.retencion_iva;

                                                            movNuevo.creditoniif = 0;
                                                            movNuevo.debitoniif = eg.retencion_iva;
                                                        }

                                                        if (buscarCuenta.concepniff == 4)
                                                        {
                                                            movNuevo.creditoniif = 0;
                                                            movNuevo.debitoniif = eg.retencion_iva;
                                                        }

                                                        if (buscarCuenta.concepniff == 5)
                                                        {
                                                            movNuevo.credito = 0;
                                                            movNuevo.debito = eg.retencion_iva;
                                                        }
                                                    }

                                                    #endregion

                                                    #region ReteICA

                                                    if (parametro.id_nombre_parametro == 5)
                                                    {
                                                        /*if (info.aplicaniff==true)
														{

														}*/

                                                        if (info.manejabase)
                                                        {
                                                            movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                                        }
                                                        else
                                                        {
                                                            movNuevo.basecontable = 0;
                                                        }

                                                        if (info.documeto)
                                                        {
                                                            movNuevo.documento = modelo.documento;
                                                        }

                                                        if (buscarCuenta.concepniff == 1)
                                                        {
                                                            movNuevo.credito = 0;
                                                            movNuevo.debito = eg.retencion_ica;

                                                            movNuevo.creditoniif = 0;
                                                            movNuevo.debitoniif = eg.retencion_ica;
                                                        }

                                                        if (buscarCuenta.concepniff == 4)
                                                        {
                                                            movNuevo.creditoniif = 0;
                                                            movNuevo.debitoniif = eg.retencion_ica;
                                                        }

                                                        if (buscarCuenta.concepniff == 5)
                                                        {
                                                            movNuevo.credito = 0;
                                                            movNuevo.debito = eg.retencion_ica;
                                                        }
                                                    }

                                                    #endregion

                                                    #region Fletes

                                                    if (parametro.id_nombre_parametro == 6)
                                                    {
                                                        /*if (info.aplicaniff==true)
														{

														}*/

                                                        /*if (info.manejabase == true)
														{
														    movNuevo.basecontable = Convert.ToDecimal(modelo.fletes);
														}*/
                                                        //else
                                                        //{
                                                        movNuevo.basecontable = 0;
                                                        //}

                                                        if (info.documeto)
                                                        {
                                                            movNuevo.documento = modelo.documento;
                                                        }

                                                        if (buscarCuenta.concepniff == 1)
                                                        {
                                                            movNuevo.credito = eg.fletes;
                                                            movNuevo.debito = 0;

                                                            movNuevo.creditoniif = eg.fletes;
                                                            movNuevo.debitoniif = 0;
                                                        }

                                                        if (buscarCuenta.concepniff == 4)
                                                        {
                                                            movNuevo.creditoniif = eg.fletes;
                                                            ;
                                                            movNuevo.debitoniif = 0;
                                                        }

                                                        if (buscarCuenta.concepniff == 5)
                                                        {
                                                            movNuevo.credito = eg.fletes;
                                                            movNuevo.debito = 0;
                                                        }
                                                    }

                                                    #endregion

                                                    #region IVA fletes

                                                    if (parametro.id_nombre_parametro == 14)
                                                    {
                                                        /*if (info.aplicaniff==true)
														{

														}*/

                                                        /*if (info.manejabase == true)
														{
														    movNuevo.basecontable = Convert.ToDecimal(modelo.fletes);
														}
														else
														{*/
                                                        movNuevo.basecontable = 0;
                                                        //}

                                                        if (info.documeto)
                                                        {
                                                            movNuevo.documento = modelo.documento;
                                                        }

                                                        if (buscarCuenta.concepniff == 1)
                                                        {
                                                            movNuevo.credito = eg.iva_fletes;
                                                            movNuevo.debito = 0;

                                                            movNuevo.creditoniif = eg.iva_fletes;
                                                            movNuevo.debitoniif = 0;
                                                        }

                                                        if (buscarCuenta.concepniff == 4)
                                                        {
                                                            movNuevo.creditoniif = eg.iva_fletes;
                                                            movNuevo.debitoniif = 0;
                                                        }

                                                        if (buscarCuenta.concepniff == 5)
                                                        {
                                                            movNuevo.credito = eg.iva_fletes;
                                                            movNuevo.debito = 0;
                                                        }
                                                    }

                                                    #endregion

                                                    #region AutoRetencion Debito

                                                    if (parametro.id_nombre_parametro == 17)
                                                    {
                                                        /*if (info.aplicaniff==true)
														{

														}*/

                                                        if (info.manejabase)
                                                        {
                                                            movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                                        }
                                                        else
                                                        {
                                                            movNuevo.basecontable = 0;
                                                        }

                                                        if (info.documeto)
                                                        {
                                                            movNuevo.documento = modelo.documento;
                                                        }

                                                        if (buscarCuenta.concepniff == 1)
                                                        {
                                                            movNuevo.credito = 0;
                                                            movNuevo.debito = eg.retencion_causada;

                                                            movNuevo.creditoniif = 0;
                                                            movNuevo.debitoniif = eg.retencion_causada;
                                                        }

                                                        if (buscarCuenta.concepniff == 4)
                                                        {
                                                            movNuevo.creditoniif = 0;
                                                            movNuevo.debitoniif = eg.retencion_causada;
                                                        }

                                                        if (buscarCuenta.concepniff == 5)
                                                        {
                                                            movNuevo.credito = 0;
                                                            movNuevo.debito = eg.retencion_causada;
                                                        }
                                                    }

                                                    #endregion

                                                    #region AutoRetencion Credito

                                                    if (parametro.id_nombre_parametro == 18)
                                                    {
                                                        /*if (info.aplicaniff==true)
														{

														}*/

                                                        if (info.manejabase)
                                                        {
                                                            movNuevo.basecontable = Convert.ToDecimal(valor_totalenca, miCultura);
                                                        }
                                                        else
                                                        {
                                                            movNuevo.basecontable = 0;
                                                        }

                                                        if (info.documeto)
                                                        {
                                                            movNuevo.documento = modelo.documento;
                                                        }

                                                        if (buscarCuenta.concepniff == 1)
                                                        {
                                                            movNuevo.credito = eg.retencion_causada;
                                                            movNuevo.debito = 0;

                                                            movNuevo.creditoniif = eg.retencion_causada;
                                                            movNuevo.debitoniif = 0;
                                                        }

                                                        if (buscarCuenta.concepniff == 4)
                                                        {
                                                            movNuevo.creditoniif = eg.retencion_causada;
                                                            movNuevo.debitoniif = 0;
                                                        }

                                                        if (buscarCuenta.concepniff == 5)
                                                        {
                                                            movNuevo.credito = eg.retencion_causada;
                                                            movNuevo.debito = 0;
                                                        }
                                                    }

                                                    #endregion

                                                    context.mov_contable.Add(movNuevo);
                                                    //context.SaveChanges();

                                                    secuencia++;
                                                    //Cuentas valores

                                                    #region Cuentas valores

                                                    cuentas_valores buscar_cuentas_valores =
                                                        context.cuentas_valores.FirstOrDefault(x =>
                                                            x.centro == parametro.centro &&
                                                            x.cuenta == parametro.cuenta && x.nit == movNuevo.nit);
                                                    if (buscar_cuentas_valores != null)
                                                    {
                                                        buscar_cuentas_valores.debito += movNuevo.debito;
                                                        buscar_cuentas_valores.credito += movNuevo.credito;
                                                        buscar_cuentas_valores.debitoniff += movNuevo.debitoniif;
                                                        buscar_cuentas_valores.creditoniff += movNuevo.creditoniif;
                                                        context.Entry(buscar_cuentas_valores).State =
                                                            EntityState.Modified;
                                                    }
                                                    else
                                                    {
                                                        DateTime fechaHoy = DateTime.Now;
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
                                                        //context.SaveChanges();
                                                    }

                                                    #endregion

                                                    totalCreditos += movNuevo.credito;
                                                    totalDebitos += movNuevo.debito;
                                                    listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                    {
                                                        NumeroCuenta =
                                                            "(" + buscarCuenta.cntpuc_numero + ")" +
                                                            buscarCuenta.cntpuc_descp,
                                                        DescripcionParametro = descripcionParametro,
                                                        ValorDebito = movNuevo.debito,
                                                        ValorCredito = movNuevo.credito
                                                    });
                                                }
                                            }
                                        }

                                        #endregion

                                        //Documentos a cruzar

                                        #region Documentos a cruzar

                                        string listaAnticipo = Request["listaAnticipo"];
                                        if (!string.IsNullOrEmpty(listaAnticipo))
                                        {
                                            int la = Convert.ToInt32(listaAnticipo);
                                            for (int i = 1; i <= la; i++)
                                            {
                                                int encabAnti = Convert.ToInt32(Request["encabAnticipo" + i]);
                                                if (encabAnti != 0)
                                                {
                                                    encab_documento encabezadoAnticipo =
                                                        context.encab_documento.FirstOrDefault(x =>
                                                            x.idencabezado == encabAnti);

                                                    documentosacruzar dac = new documentosacruzar
                                                    {
                                                        idencabrecibo = encabAnti,
                                                        valorrecibo = encabezadoAnticipo.valor_total,
                                                        idencabfactura = id_encabezado,
                                                        valorfactura = eg.valor_total,
                                                        saldo = encabezadoAnticipo.valor_total - eg.valor_total
                                                    };

                                                    context.documentosacruzar.Add(dac);
                                                    context.SaveChanges();
                                                }
                                            }
                                        }

                                        #endregion

                                        //Lineas documento

                                        #region lineasDocumento

                                        List<mov_contable> listaMov = new List<mov_contable>();
                                        int listaLineas = listaelementos.Count();
                                        for (int i = 0; i < listaLineas; i++)
                                        {
                                            //if (!string.IsNullOrEmpty(Request["referencia" + i]))
                                            //{
                                            //var porDescuento = (!string.IsNullOrEmpty(Request["descuentoReferencia" + i])) ? Convert.ToDecimal(Request["descuentoReferencia" + i]) : 0;
                                            decimal porDescuento = listaelementos[i].porcentaje_descuento;

                                            //var codigo = Request["referencia" + i];
                                            string codigo = listaelementos[i].codigo;

                                            //var cantidadFacturada = Convert.ToDecimal(Request["cantidadReferencia" + i]);
                                            decimal cantidadFacturada = listaelementos[i].cantidad;
                                            //var valorReferencia = Convert.ToDecimal(Request["valorUnitarioReferencia" + i]);
                                            decimal valorReferencia = listaelementos[i].valor_unitario;
                                            decimal descontar = porDescuento / 100;
                                            //var porIVAReferencia = Convert.ToDecimal(Request["ivaReferencia" + i]) / 100;
                                            decimal porIVAReferencia = listaelementos[i].porcentaje_iva / 100;

                                            decimal final = Math.Round(valorReferencia - valorReferencia * descontar);
                                            //var baseUnitario = final * Convert.ToDecimal(Request["cantidadReferencia" + i]);
                                            decimal baseUnitario = final * listaelementos[i].cantidad;

                                            decimal ivaReferencia =
                                                Math.Round(final * porIVAReferencia * listaelementos[i].cantidad);
                                            string und = "0";
                                            if (listaelementos[i].tipo == "R")
                                            {
                                                icb_referencia unidadCodigo =
                                                    context.icb_referencia.FirstOrDefault(x => x.ref_codigo == codigo);
                                                und = unidadCodigo.unidad_medida;
                                            }

                                            decimal costoReferencia = 0;
                                            decimal cr = 0;
                                            if (listaelementos[i].tipo == "R")
                                            {
                                                //var vwPromedio = context.vw_promedio.FirstOrDefault(x => x.codigo == codigo && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month);
                                                icb_referencia vwPromedio =
                                                    context.icb_referencia.FirstOrDefault(x => x.ref_codigo == codigo);
                                                costoReferencia = vwPromedio.costo_promedio ?? 0;
                                                cr = costoReferencia * listaelementos[i].cantidad;
                                            }
                                            else
                                            {
                                                ttempario vwPromedio =
                                                    context.ttempario.FirstOrDefault(x => x.codigo == codigo);
                                                costoReferencia = vwPromedio.costo ?? 0;
                                                cr = costoReferencia * listaelementos[i].cantidad;
                                            }
                                            /*
											if (!string.IsNullOrEmpty(Request["pedidoID" + i]))
											{
											    var pedidoSeleccionado = Convert.ToInt32(Request["pedidoID" + i]);

											    var buscar_movimientoPedido = context.icb_referencia_movdetalle.FirstOrDefault(x => x.refmov_id == pedidoSeleccionado && x.ref_codigo == codigo);
											    if (buscar_movimientoPedido != null)
											    {
											        if (buscar_movimientoPedido.refdet_saldo != null)
											        {
											            buscar_movimientoPedido.refdet_saldo += cantidadFacturada;
											        }
											        else
											        {
											            buscar_movimientoPedido.refdet_saldo = cantidadFacturada;
											        }

											        context.Entry(buscar_movimientoPedido).State = EntityState.Modified;
											    }
											}*/

                                            if (listaelementos[i].tipo == "R")
                                            {
                                                lineas_documento lineas = new lineas_documento
                                                {
                                                    id_encabezado = id_encabezado,
                                                    seq = i + 1,
                                                    fec = DateTime.Now,
                                                    nit = ordentaller.tercero
                                                };
                                                if (listaelementos[i].tipo == "R")
                                                {
                                                    lineas.und = Convert.ToString(und);
                                                    lineas.codigo = codigo;
                                                }

                                                lineas.cantidad = listaelementos[i].cantidad;
                                                decimal ivaLista = listaelementos[i].porcentaje_iva;
                                                lineas.porcentaje_iva = (float)ivaLista;
                                                lineas.valor_unitario = final;
                                                decimal descuento = porDescuento;
                                                lineas.porcentaje_descuento = (float)descuento;
                                                lineas.costo_unitario = Convert.ToDecimal(costoReferencia, miCultura);
                                                lineas.bodega = ordentaller.bodega;
                                                lineas.fec_creacion = DateTime.Now;
                                                lineas.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                                lineas.estado = true;
                                                lineas.id_tarifa_cliente = listaelementos[i].tipo_tarifa;
                                                lineas.moneda = moneda;
                                                lineas.vendedor = eg.vendedor;

                                                context.lineas_documento.Add(lineas);
                                            }

                                            #endregion

                                            /*if (listaelementos[i].tipo == "R")
											{
											    #region referencias inven
											    var anio = DateTime.Now.Year;
											    var mes = DateTime.Now.Month;

											    referencias_inven refin = new referencias_inven();

											    var existencia = context.referencias_inven.FirstOrDefault(x => x.ano == anio && x.mes == mes && x.codigo == codigo && x.bodega == bodega);

											    if (existencia != null)
											    {
											        existencia.codigo = codigo;
											        existencia.can_sal += listaelementos[i].cantidad;
											        existencia.cos_sal += Convert.ToDecimal(cr);//(final * Convert.ToDecimal(Request["cantidadReferencia" + i])); cambio solicitado por la ingeniera liliana el dia 10/09/18
											        existencia.can_vta += listaelementos[i].cantidad;
											        existencia.cos_vta += Convert.ToDecimal(cr);//(final * Convert.ToDecimal(Request["cantidadReferencia" + i])); cambio solicitado por la ingeniera liliana el dia 10/09/18
											        existencia.val_vta += baseUnitario;
											        context.Entry(existencia).State = EntityState.Modified;
											    }
											    else
											    {
											        refin.bodega = ordentaller.bodega;
											        refin.codigo = codigo;
											        refin.ano = Convert.ToInt16(DateTime.Now.Year);
											        refin.mes = Convert.ToInt16(DateTime.Now.Month);
											        refin.can_sal = listaelementos[i].cantidad; 
											        refin.cos_sal = Convert.ToDecimal(cr); //final; cambio solicitado por la ingeniera liliana el dia 10/09/18
											        refin.can_vta = listaelementos[i].cantidad;
											        refin.cos_vta = Convert.ToDecimal(cr); //final; cambio solicitado por la ingeniera liliana el dia 10/09/18
											        refin.val_vta = baseUnitario;
											        refin.modulo = "R";
											        context.referencias_inven.Add(refin);
											    }
											    #endregion
											}*/

                                            #region Mov Contable (IVA, Inventario, Costo, Ingreso)

                                            foreach (var parametro in parametrosCuentasVerificar)
                                            {
                                                string descripcionParametro = context.paramcontablenombres
                                                    .FirstOrDefault(x => x.id == parametro.id_nombre_parametro)
                                                    .descripcion_parametro;
                                                cuenta_puc buscarCuenta =
                                                    context.cuenta_puc.FirstOrDefault(x =>
                                                        x.cntpuc_id == parametro.cuenta);

                                                if (buscarCuenta != null)
                                                {
                                                    if (parametro.id_nombre_parametro == 2 &&
                                                        Convert.ToDecimal(ivaEncabezado, miCultura) != 0
                                                        || parametro.id_nombre_parametro == 9 &&
                                                        Convert.ToDecimal(costoPromedioTotal, miCultura) != 0 //costo promedio
                                                        || parametro.id_nombre_parametro == 20 &&
                                                        Convert.ToDecimal(costoPromedioTotal, miCultura) != 0 //costo promedio
                                                        || parametro.id_nombre_parametro == 11 &&
                                                        Convert.ToDecimal(costoEncabezado, miCultura) != 0
                                                        || parametro.id_nombre_parametro == 12 &&
                                                        Convert.ToDecimal(costoPromedioTotal, miCultura) != 0) //costo promedio
                                                    {
                                                        mov_contable movNuevo = new mov_contable
                                                        {
                                                            id_encab = encabezado.idencabezado,
                                                            seq = secuencia,
                                                            idparametronombre = parametro.id_nombre_parametro,
                                                            cuenta = parametro.cuenta,
                                                            centro = listaelementos[i].tipo_tarifa == 2
                                                            ? parametro.id_nombre_parametro == 11
                                                                ?
                                                                listaelementos[i].centro_costo
                                                                : parametro.id_nombre_parametro == 12
                                                                    ? listaelementos[i].centro_costo
                                                                    : parametro.centro
                                                            : parametro.centro,
                                                            fec = DateTime.Now,
                                                            fec_creacion = DateTime.Now,
                                                            tipo_tarifa = listaelementos[i].tipo_tarifa,
                                                            userid_creacion =
                                                            Convert.ToInt32(Session["user_usuarioid"]),
                                                            documento = Convert.ToString(ordentaller.id)
                                                        };

                                                        cuenta_puc info = context.cuenta_puc
                                                            .Where(a => a.cntpuc_id == parametro.cuenta)
                                                            .FirstOrDefault();

                                                        if (info.tercero)
                                                        {
                                                            movNuevo.nit = ordentaller.tercero;
                                                        }
                                                        else
                                                        {
                                                            icb_terceros tercero = context.icb_terceros
                                                                .Where(t => t.doc_tercero == "0").FirstOrDefault();
                                                            movNuevo.nit = tercero.tercero_id;
                                                        }

                                                        #region IVA

                                                        if (parametro.id_nombre_parametro == 2)
                                                        {
                                                            icb_referencia perfilReferencia =
                                                                context.icb_referencia.FirstOrDefault(x =>
                                                                    x.ref_codigo == codigo);
                                                            int perfilBuscar = 0;

                                                            if (perfilReferencia != null)
                                                            {
                                                                perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                                            }

                                                            perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(
                                                                r => r.id == perfilBuscar);

                                                            #region Tiene perfil la referencia

                                                            if (pcr != null)
                                                            {
                                                                int? cuentaIva = pcr.cuenta_dev_iva_compras;

                                                                movNuevo.id_encab = encabezado.idencabezado;
                                                                movNuevo.seq = secuencia;
                                                                movNuevo.idparametronombre =
                                                                    parametro.id_nombre_parametro;

                                                                #region si tiene perfil y cuenta asignada a ese perfil

                                                                if (cuentaIva != null)
                                                                {
                                                                    movNuevo.cuenta = Convert.ToInt32(cuentaIva);
                                                                    movNuevo.centro = parametro.centro;
                                                                    movNuevo.fec = DateTime.Now;
                                                                    movNuevo.fec_creacion = DateTime.Now;
                                                                    movNuevo.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);
                                                                    movNuevo.documento = Convert.ToString(eg.numero);

                                                                    cuenta_puc infoReferencia = context.cuenta_puc
                                                                        .Where(a => a.cntpuc_id == cuentaIva)
                                                                        .FirstOrDefault();
                                                                    if (infoReferencia.manejabase)
                                                                    {
                                                                        movNuevo.basecontable =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                    }
                                                                    else
                                                                    {
                                                                        movNuevo.basecontable = 0;
                                                                    }

                                                                    if (infoReferencia.documeto)
                                                                    {
                                                                        movNuevo.documento =
                                                                            Convert.ToString(eg.numero);
                                                                    }

                                                                    if (infoReferencia.concepniff == 1)
                                                                    {
                                                                        movNuevo.credito =
                                                                            Convert.ToDecimal(ivaReferencia, miCultura);
                                                                        movNuevo.debito = 0;

                                                                        movNuevo.creditoniif =
                                                                            Convert.ToDecimal(ivaReferencia, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (infoReferencia.concepniff == 4)
                                                                    {
                                                                        movNuevo.creditoniif =
                                                                            Convert.ToDecimal(ivaReferencia, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (infoReferencia.concepniff == 5)
                                                                    {
                                                                        movNuevo.credito =
                                                                            Convert.ToDecimal(ivaReferencia, miCultura);
                                                                        movNuevo.debito = 0;
                                                                    }

                                                                    // context.mov_contable.Add(movNuevo);
                                                                }

                                                                #endregion

                                                                #region si tiene perfil pero no tiene cuenta asignada

                                                                else
                                                                {
                                                                    movNuevo.cuenta = parametro.cuenta;
                                                                    movNuevo.centro = parametro.centro;
                                                                    movNuevo.fec = DateTime.Now;
                                                                    movNuevo.fec_creacion = DateTime.Now;
                                                                    movNuevo.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);
                                                                    movNuevo.documento = Convert.ToString(eg.numero);

                                                                    cuenta_puc infoReferencia = context.cuenta_puc
                                                                        .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                        .FirstOrDefault();
                                                                    if (infoReferencia.manejabase)
                                                                    {
                                                                        movNuevo.basecontable =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                    }
                                                                    else
                                                                    {
                                                                        movNuevo.basecontable = 0;
                                                                    }

                                                                    if (infoReferencia.documeto)
                                                                    {
                                                                        movNuevo.documento =
                                                                            Convert.ToString(eg.numero);
                                                                    }

                                                                    if (infoReferencia.concepniff == 1)
                                                                    {
                                                                        movNuevo.credito =
                                                                            Convert.ToDecimal(ivaReferencia, miCultura);
                                                                        movNuevo.debito = 0;

                                                                        movNuevo.creditoniif =
                                                                            Convert.ToDecimal(ivaReferencia, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (infoReferencia.concepniff == 4)
                                                                    {
                                                                        movNuevo.creditoniif =
                                                                            Convert.ToDecimal(ivaReferencia, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (infoReferencia.concepniff == 5)
                                                                    {
                                                                        movNuevo.credito =
                                                                            Convert.ToDecimal(ivaReferencia, miCultura);
                                                                        movNuevo.debito = 0;
                                                                    }

                                                                    //context.mov_contable.Add(movNuevo);
                                                                }

                                                                #endregion
                                                            }

                                                            #endregion

                                                            #region La referencia no tiene perfil

                                                            else
                                                            {
                                                                movNuevo.id_encab = encabezado.idencabezado;
                                                                movNuevo.seq = secuencia;
                                                                movNuevo.idparametronombre =
                                                                    parametro.id_nombre_parametro;
                                                                movNuevo.cuenta = parametro.cuenta;
                                                                movNuevo.centro = parametro.centro;
                                                                movNuevo.fec = DateTime.Now;
                                                                movNuevo.fec_creacion = DateTime.Now;
                                                                movNuevo.userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]);
                                                                /*if (info.aplicaniff==true)
																{

																}*/

                                                                if (info.manejabase)
                                                                {
                                                                    movNuevo.basecontable =
                                                                        Convert.ToDecimal(baseUnitario, miCultura);
                                                                }
                                                                else
                                                                {
                                                                    movNuevo.basecontable = 0;
                                                                }

                                                                if (info.documeto)
                                                                {
                                                                    movNuevo.documento = Convert.ToString(eg.numero);
                                                                }

                                                                if (buscarCuenta.concepniff == 1)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(ivaReferencia, miCultura);
                                                                    movNuevo.debito = 0;

                                                                    movNuevo.creditoniif =
                                                                        Convert.ToDecimal(ivaReferencia, miCultura);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (buscarCuenta.concepniff == 4)
                                                                {
                                                                    movNuevo.creditoniif =
                                                                        Convert.ToDecimal(ivaReferencia, miCultura);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (buscarCuenta.concepniff == 5)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(ivaReferencia, miCultura);
                                                                    movNuevo.debito = 0;
                                                                }

                                                                //context.mov_contable.Add(movNuevo);
                                                            }

                                                            #endregion

                                                            mov_contable buscarIVA = context.mov_contable.FirstOrDefault(x =>
                                                                x.id_encab == id_encabezado &&
                                                                x.cuenta == movNuevo.cuenta &&
                                                                x.idparametronombre == parametro.id_nombre_parametro);
                                                            if (buscarIVA != null)
                                                            {
                                                                buscarIVA.debito += movNuevo.debito;
                                                                buscarIVA.debitoniif += movNuevo.debitoniif;
                                                                buscarIVA.credito += movNuevo.credito;
                                                                buscarIVA.creditoniif += movNuevo.creditoniif;
                                                                context.Entry(buscarIVA).State = EntityState.Modified;
                                                            }
                                                            else
                                                            {
                                                                mov_contable crearMovContable = new mov_contable
                                                                {
                                                                    id_encab = encabezado.idencabezado,
                                                                    seq = secuencia,
                                                                    idparametronombre =
                                                                    parametro.id_nombre_parametro,
                                                                    cuenta =
                                                                    Convert.ToInt32(movNuevo.cuenta),
                                                                    centro = parametro.centro,
                                                                    nit = encabezado.nit,
                                                                    fec = DateTime.Now,
                                                                    debito = movNuevo.debito,
                                                                    debitoniif = movNuevo.debitoniif,
                                                                    basecontable = movNuevo.basecontable,
                                                                    credito = movNuevo.credito,
                                                                    creditoniif = movNuevo.creditoniif,
                                                                    fec_creacion = DateTime.Now,
                                                                    userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]),
                                                                    detalle =
                                                                    "Facturacion de orden taller N° " +
                                                                    ordentaller.codigoentrada + " con consecutivo " +
                                                                    eg.numero,
                                                                    estado = true
                                                                };
                                                                context.mov_contable.Add(crearMovContable);
                                                                context.SaveChanges();
                                                            }
                                                        }

                                                        #endregion

                                                        #region Inventario

                                                        //si el tipo de elemento que estoy analizando en este momento es de inventario
                                                        if (listaelementos[i].tipo == "R")
                                                        {
                                                            if (parametro.id_nombre_parametro == 9 ||
                                                                parametro.id_nombre_parametro == 20)
                                                            {
                                                                icb_referencia perfilReferencia =
                                                                    context.icb_referencia.FirstOrDefault(x =>
                                                                        x.ref_codigo == codigo);
                                                                int perfilBuscar =
                                                                    Convert.ToInt32(perfilReferencia.perfil);
                                                                perfilcontable_referencia pcr = context.perfilcontable_referencia
                                                                    .FirstOrDefault(r => r.id == perfilBuscar);

                                                                #region Tiene perfil la referencia

                                                                if (pcr != null)
                                                                {
                                                                    int? cuentaInven = pcr.cta_inventario;

                                                                    movNuevo.id_encab = encabezado.idencabezado;
                                                                    movNuevo.seq = secuencia;
                                                                    movNuevo.idparametronombre =
                                                                        parametro.id_nombre_parametro;

                                                                    #region tiene perfil y cuenta asignada al perfil

                                                                    if (cuentaInven != null)
                                                                    {
                                                                        movNuevo.cuenta = Convert.ToInt32(cuentaInven);
                                                                        movNuevo.centro = parametro.centro;
                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        movNuevo.documento =
                                                                            Convert.ToString(eg.numero);

                                                                        cuenta_puc infoReferencia = context.cuenta_puc
                                                                            .Where(a => a.cntpuc_id == cuentaInven)
                                                                            .FirstOrDefault();
                                                                        if (infoReferencia.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(baseUnitario, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (infoReferencia.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (infoReferencia.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debito = 0;

                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debito = 0;
                                                                        }

                                                                        //context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion

                                                                    #region tiene perfil pero no tiene cuenta asignada

                                                                    else
                                                                    {
                                                                        movNuevo.cuenta = parametro.cuenta;
                                                                        movNuevo.centro = parametro.centro;
                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        movNuevo.documento =
                                                                            Convert.ToString(eg.numero);

                                                                        cuenta_puc infoReferencia = context.cuenta_puc
                                                                            .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                            .FirstOrDefault();
                                                                        if (infoReferencia.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(valor_totalenca, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (infoReferencia.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (infoReferencia.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debito = 0;

                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif =
                                                                                Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debitoniif = 0;
                                                                        }

                                                                        if (infoReferencia.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                            movNuevo.debito = 0;
                                                                        }

                                                                        //context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion
                                                                }

                                                                #endregion

                                                                #region La referencia no tiene perfil

                                                                else
                                                                {
                                                                    movNuevo.id_encab = encabezado.idencabezado;
                                                                    movNuevo.seq = secuencia;
                                                                    movNuevo.idparametronombre =
                                                                        parametro.id_nombre_parametro;
                                                                    movNuevo.cuenta = parametro.cuenta;
                                                                    movNuevo.centro = parametro.centro;
                                                                    movNuevo.fec = DateTime.Now;
                                                                    movNuevo.fec_creacion = DateTime.Now;
                                                                    movNuevo.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);
                                                                    /*if (info.aplicaniff==true)
																	{

																	}*/

                                                                    if (info.manejabase)
                                                                    {
                                                                        movNuevo.basecontable =
                                                                            Convert.ToDecimal(valor_totalenca, miCultura);
                                                                    }
                                                                    else
                                                                    {
                                                                        movNuevo.basecontable = 0;
                                                                    }

                                                                    if (info.documeto)
                                                                    {
                                                                        movNuevo.documento =
                                                                            Convert.ToString(eg.numero);
                                                                    }

                                                                    if (buscarCuenta.concepniff == 1)
                                                                    {
                                                                        movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                        movNuevo.debito = 0;

                                                                        movNuevo.creditoniif = Convert.ToDecimal(cr, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (buscarCuenta.concepniff == 4)
                                                                    {
                                                                        movNuevo.creditoniif = Convert.ToDecimal(cr, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (buscarCuenta.concepniff == 5)
                                                                    {
                                                                        movNuevo.credito = Convert.ToDecimal(cr, miCultura);
                                                                        movNuevo.debito = 0;
                                                                    }

                                                                    //context.mov_contable.Add(movNuevo);
                                                                }

                                                                #endregion

                                                                mov_contable buscarInventario =
                                                                    context.mov_contable.FirstOrDefault(x =>
                                                                        x.id_encab == id_encabezado &&
                                                                        x.cuenta == movNuevo.cuenta &&
                                                                        x.idparametronombre ==
                                                                        parametro.id_nombre_parametro);
                                                                if (buscarInventario != null)
                                                                {
                                                                    buscarInventario.basecontable +=
                                                                        movNuevo.basecontable;
                                                                    buscarInventario.debito += movNuevo.debito;
                                                                    buscarInventario.debitoniif += movNuevo.debitoniif;
                                                                    buscarInventario.credito += movNuevo.credito;
                                                                    buscarInventario.creditoniif +=
                                                                        movNuevo.creditoniif;
                                                                    context.Entry(buscarInventario).State =
                                                                        EntityState.Modified;
                                                                }
                                                                else
                                                                {
                                                                    mov_contable crearMovContable = new mov_contable
                                                                    {
                                                                        id_encab = encabezado.idencabezado,
                                                                        seq = secuencia,
                                                                        idparametronombre =
                                                                        parametro.id_nombre_parametro,
                                                                        cuenta =
                                                                        Convert.ToInt32(movNuevo.cuenta),
                                                                        centro = parametro.centro,
                                                                        nit = encabezado.nit,
                                                                        fec = DateTime.Now,
                                                                        debito = movNuevo.debito,
                                                                        debitoniif = movNuevo.debitoniif,
                                                                        basecontable =
                                                                        movNuevo.basecontable,
                                                                        credito = movNuevo.credito,
                                                                        creditoniif = movNuevo.creditoniif,
                                                                        fec_creacion = DateTime.Now,
                                                                        userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]),
                                                                        detalle =
                                                                        "Facturacion de orden de taller N° " +
                                                                        ordentaller.codigoentrada +
                                                                        " con consecutivo " + eg.numero,
                                                                        estado = true
                                                                    };
                                                                    context.mov_contable.Add(crearMovContable);
                                                                    context.SaveChanges();
                                                                }
                                                            }
                                                        }

                                                        #endregion

                                                        #region Ingreso	

                                                        //var siva = Request["tipo_tarifa_hidden_" + i] == "2";
                                                        bool siva = listaelementos[i].tipo_tarifa == 2;
                                                        if (parametro.id_nombre_parametro == 11 && siva != true)
                                                        {
                                                            icb_referencia perfilReferencia =
                                                                context.icb_referencia.FirstOrDefault(x =>
                                                                    x.ref_codigo == codigo);
                                                            int perfilBuscar = 0;
                                                            if (perfilReferencia != null &&
                                                                listaelementos[i].tipo == "R")
                                                            {
                                                                perfilBuscar = Convert.ToInt32(perfilReferencia.perfil);
                                                            }

                                                            perfilcontable_referencia pcr = context.perfilcontable_referencia.FirstOrDefault(
                                                                r => r.id == perfilBuscar);

                                                            #region Tiene perfil la referencia

                                                            if (pcr != null)
                                                            {
                                                                int? cuentaVenta = pcr.cuenta_ventas;

                                                                movNuevo.id_encab = encabezado.idencabezado;
                                                                movNuevo.seq = secuencia;
                                                                movNuevo.idparametronombre =
                                                                    parametro.id_nombre_parametro;

                                                                #region tiene perfil y cuenta asignada al perfil

                                                                if (cuentaVenta != null)
                                                                {
                                                                    movNuevo.cuenta = Convert.ToInt32(cuentaVenta);
                                                                    //movNuevo.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 11 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro; ;
                                                                    movNuevo.centro = listaelementos[i].tipo_tarifa == 2
                                                                        ? parametro.id_nombre_parametro == 11
                                                                            ?
                                                                            listaelementos[i].centro_costo
                                                                            : parametro.centro
                                                                        : parametro.centro;
                                                                    ;

                                                                    movNuevo.fec = DateTime.Now;
                                                                    movNuevo.fec_creacion = DateTime.Now;
                                                                    movNuevo.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);
                                                                    movNuevo.documento = Convert.ToString(eg.numero);

                                                                    cuenta_puc infoReferencia = context.cuenta_puc
                                                                        .Where(a => a.cntpuc_id == cuentaVenta)
                                                                        .FirstOrDefault();
                                                                    if (infoReferencia.manejabase)
                                                                    {
                                                                        movNuevo.basecontable =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                    }
                                                                    else
                                                                    {
                                                                        movNuevo.basecontable = 0;
                                                                    }

                                                                    if (infoReferencia.documeto)
                                                                    {
                                                                        movNuevo.documento =
                                                                            Convert.ToString(eg.numero);
                                                                    }

                                                                    if (infoReferencia.concepniff == 1)
                                                                    {
                                                                        movNuevo.credito =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                        movNuevo.debito = 0;

                                                                        movNuevo.creditoniif =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (infoReferencia.concepniff == 4)
                                                                    {
                                                                        movNuevo.creditoniif =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (infoReferencia.concepniff == 5)
                                                                    {
                                                                        movNuevo.credito =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                        movNuevo.debito = 0;
                                                                    }

                                                                    //context.mov_contable.Add(movNuevo);
                                                                }

                                                                #endregion

                                                                #region tiene perfil pero no tiene cuenta asignada

                                                                else
                                                                {
                                                                    movNuevo.cuenta = parametro.cuenta;
                                                                    //movNuevo.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 11 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro; ;
                                                                    movNuevo.centro = listaelementos[i].tipo_tarifa == 2
                                                                        ? parametro.id_nombre_parametro == 11
                                                                            ?
                                                                            listaelementos[i].centro_costo
                                                                            : parametro.centro
                                                                        : parametro.centro;
                                                                    ;

                                                                    movNuevo.fec = DateTime.Now;
                                                                    movNuevo.fec_creacion = DateTime.Now;
                                                                    movNuevo.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);
                                                                    movNuevo.documento = Convert.ToString(eg.numero);

                                                                    cuenta_puc infoReferencia = context.cuenta_puc
                                                                        .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                        .FirstOrDefault();
                                                                    if (infoReferencia.manejabase)
                                                                    {
                                                                        movNuevo.basecontable =
                                                                            Convert.ToDecimal(valor_totalenca, miCultura);
                                                                    }
                                                                    else
                                                                    {
                                                                        movNuevo.basecontable = 0;
                                                                    }

                                                                    if (infoReferencia.documeto)
                                                                    {
                                                                        movNuevo.documento =
                                                                            Convert.ToString(eg.numero);
                                                                    }

                                                                    if (infoReferencia.concepniff == 1)
                                                                    {
                                                                        movNuevo.credito =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                        movNuevo.debito = 0;

                                                                        movNuevo.creditoniif =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (infoReferencia.concepniff == 4)
                                                                    {
                                                                        movNuevo.creditoniif =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                        movNuevo.debitoniif = 0;
                                                                    }

                                                                    if (infoReferencia.concepniff == 5)
                                                                    {
                                                                        movNuevo.credito =
                                                                            Convert.ToDecimal(baseUnitario, miCultura);
                                                                        movNuevo.debito = 0;
                                                                    }

                                                                    //context.mov_contable.Add(movNuevo);
                                                                }

                                                                #endregion
                                                            }

                                                            #endregion

                                                            #region La referencia no tiene perfil

                                                            else
                                                            {
                                                                movNuevo.id_encab = encabezado.idencabezado;
                                                                movNuevo.seq = secuencia;
                                                                movNuevo.idparametronombre =
                                                                    parametro.id_nombre_parametro;
                                                                movNuevo.cuenta = parametro.cuenta;
                                                                //movNuevo.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 11 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro;
                                                                movNuevo.centro = listaelementos[i].tipo_tarifa == 2
                                                                    ? parametro.id_nombre_parametro == 11
                                                                        ?
                                                                        listaelementos[i].centro_costo
                                                                        : parametro.centro
                                                                    : parametro.centro;

                                                                movNuevo.fec = DateTime.Now;
                                                                movNuevo.fec_creacion = DateTime.Now;
                                                                movNuevo.userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]);
                                                                /*if (info.aplicaniff==true)
																{

																}*/

                                                                if (info.manejabase)
                                                                {
                                                                    movNuevo.basecontable =
                                                                        Convert.ToDecimal(valor_totalenca, miCultura);
                                                                }
                                                                else
                                                                {
                                                                    movNuevo.basecontable = 0;
                                                                }

                                                                if (info.documeto)
                                                                {
                                                                    movNuevo.documento = Convert.ToString(eg.numero);
                                                                }

                                                                if (buscarCuenta.concepniff == 1)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(baseUnitario, miCultura);
                                                                    movNuevo.debito = 0;

                                                                    movNuevo.creditoniif =
                                                                        Convert.ToDecimal(baseUnitario, miCultura);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (buscarCuenta.concepniff == 4)
                                                                {
                                                                    movNuevo.creditoniif =
                                                                        Convert.ToDecimal(baseUnitario, miCultura);
                                                                    movNuevo.debitoniif = 0;
                                                                }

                                                                if (buscarCuenta.concepniff == 5)
                                                                {
                                                                    movNuevo.credito = Convert.ToDecimal(baseUnitario, miCultura);
                                                                    movNuevo.debito = 0;
                                                                }

                                                                //context.mov_contable.Add(movNuevo);
                                                            }

                                                            #endregion

                                                            mov_contable buscarVenta = context.mov_contable.FirstOrDefault(x =>
                                                                x.id_encab == id_encabezado &&
                                                                x.cuenta == movNuevo.cuenta &&
                                                                x.idparametronombre == parametro.id_nombre_parametro);
                                                            if (buscarVenta != null)
                                                            {
                                                                buscarVenta.basecontable += movNuevo.basecontable;
                                                                buscarVenta.debito += movNuevo.debito;
                                                                buscarVenta.debitoniif += movNuevo.debitoniif;
                                                                buscarVenta.credito += movNuevo.credito;
                                                                buscarVenta.creditoniif += movNuevo.creditoniif;
                                                                context.Entry(buscarVenta).State = EntityState.Modified;
                                                            }
                                                            else
                                                            {
                                                                mov_contable crearMovContable = new mov_contable
                                                                {
                                                                    id_encab = encabezado.idencabezado,
                                                                    seq = secuencia,
                                                                    idparametronombre =
                                                                    parametro.id_nombre_parametro,
                                                                    cuenta =
                                                                    Convert.ToInt32(movNuevo.cuenta),
                                                                    //crearMovContable.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 11 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro;
                                                                    centro =
                                                                    listaelementos[i].tipo_tarifa == 2
                                                                        ? parametro.id_nombre_parametro == 11
                                                                            ?
                                                                            listaelementos[i].centro_costo
                                                                            : parametro.centro
                                                                        : parametro.centro,

                                                                    nit = encabezado.nit,
                                                                    fec = DateTime.Now,
                                                                    debito = movNuevo.debito,
                                                                    debitoniif = movNuevo.debitoniif,
                                                                    basecontable = movNuevo.basecontable,
                                                                    credito = movNuevo.credito,
                                                                    creditoniif = movNuevo.creditoniif,
                                                                    fec_creacion = DateTime.Now,
                                                                    userid_creacion =
                                                                    Convert.ToInt32(Session["user_usuarioid"]),
                                                                    detalle =
                                                                    "Facturacion de Orden de Taller N° " +
                                                                    ordentaller.codigoentrada + " con consecutivo " +
                                                                    eg.numero,
                                                                    estado = true
                                                                };
                                                                context.mov_contable.Add(crearMovContable);
                                                                context.SaveChanges();
                                                            }
                                                        }

                                                        #endregion

                                                        #region Costo

                                                        if (listaelementos[i].tipo == "R")
                                                        {
                                                            if (parametro.id_nombre_parametro == 12)
                                                            {
                                                                icb_referencia perfilReferencia =
                                                                    context.icb_referencia.FirstOrDefault(x =>
                                                                        x.ref_codigo == codigo);
                                                                int perfilBuscar = 0;
                                                                if (listaelementos[i].tipo == "R")
                                                                {
                                                                    perfilBuscar =
                                                                        Convert.ToInt32(perfilReferencia.perfil);
                                                                }

                                                                perfilcontable_referencia pcr = context.perfilcontable_referencia
                                                                    .FirstOrDefault(r => r.id == perfilBuscar);

                                                                #region Tiene perfil la referencia

                                                                if (pcr != null)
                                                                {
                                                                    int? cuentaCosto = pcr.cuenta_costo;

                                                                    movNuevo.id_encab = encabezado.idencabezado;
                                                                    movNuevo.seq = secuencia;
                                                                    movNuevo.idparametronombre =
                                                                        parametro.id_nombre_parametro;

                                                                    #region tiene perfil y cuenta asignada al perfil

                                                                    if (cuentaCosto != null)
                                                                    {
                                                                        movNuevo.cuenta = Convert.ToInt32(cuentaCosto);
                                                                        //movNuevo.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 12 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro;
                                                                        movNuevo.centro =
                                                                            listaelementos[i].tipo_tarifa == 2
                                                                                ? parametro.id_nombre_parametro == 12
                                                                                    ?
                                                                                    listaelementos[i].centro_costo
                                                                                    : parametro.centro
                                                                                : parametro.centro;

                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        movNuevo.documento =
                                                                            Convert.ToString(eg.numero);

                                                                        cuenta_puc infoReferencia = context.cuenta_puc
                                                                            .Where(a => a.cntpuc_id == cuentaCosto)
                                                                            .FirstOrDefault();
                                                                        if (infoReferencia.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(valor_totalenca, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (infoReferencia.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (infoReferencia.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito = 0;
                                                                            movNuevo.debito = Convert.ToDecimal(cr, miCultura);

                                                                            movNuevo.creditoniif = 0;
                                                                            movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                        }

                                                                        if (infoReferencia.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif = 0;
                                                                            movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                        }

                                                                        if (infoReferencia.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito = 0;
                                                                            movNuevo.debito = Convert.ToDecimal(cr, miCultura);
                                                                        }

                                                                        //context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion

                                                                    #region tiene perfil pero no tiene cuenta asignada

                                                                    else
                                                                    {
                                                                        movNuevo.cuenta = parametro.cuenta;
                                                                        //movNuevo.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 12 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro; ;
                                                                        movNuevo.centro =
                                                                            listaelementos[i].tipo_tarifa == 2
                                                                                ? parametro.id_nombre_parametro == 12
                                                                                    ?
                                                                                    listaelementos[i].centro_costo
                                                                                    : parametro.centro
                                                                                : parametro.centro;

                                                                        movNuevo.fec = DateTime.Now;
                                                                        movNuevo.fec_creacion = DateTime.Now;
                                                                        movNuevo.userid_creacion =
                                                                            Convert.ToInt32(Session["user_usuarioid"]);
                                                                        movNuevo.documento =
                                                                            Convert.ToString(eg.numero);

                                                                        cuenta_puc infoReferencia = context.cuenta_puc
                                                                            .Where(a => a.cntpuc_id == parametro.cuenta)
                                                                            .FirstOrDefault();
                                                                        if (infoReferencia.manejabase)
                                                                        {
                                                                            movNuevo.basecontable =
                                                                                Convert.ToDecimal(valor_totalenca, miCultura);
                                                                        }
                                                                        else
                                                                        {
                                                                            movNuevo.basecontable = 0;
                                                                        }

                                                                        if (infoReferencia.documeto)
                                                                        {
                                                                            movNuevo.documento =
                                                                                Convert.ToString(eg.numero);
                                                                        }

                                                                        if (infoReferencia.concepniff == 1)
                                                                        {
                                                                            movNuevo.credito = 0;
                                                                            movNuevo.debito = Convert.ToDecimal(cr, miCultura);

                                                                            movNuevo.creditoniif = 0;
                                                                            movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                        }

                                                                        if (infoReferencia.concepniff == 4)
                                                                        {
                                                                            movNuevo.creditoniif = 0;
                                                                            movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                        }

                                                                        if (infoReferencia.concepniff == 5)
                                                                        {
                                                                            movNuevo.credito = 0;
                                                                            movNuevo.debito = Convert.ToDecimal(cr, miCultura);
                                                                        }

                                                                        //context.mov_contable.Add(movNuevo);
                                                                    }

                                                                    #endregion
                                                                }

                                                                #endregion

                                                                #region La referencia no tiene perfil

                                                                else
                                                                {
                                                                    movNuevo.id_encab = encabezado.idencabezado;
                                                                    movNuevo.seq = secuencia;
                                                                    movNuevo.idparametronombre =
                                                                        parametro.id_nombre_parametro;
                                                                    movNuevo.cuenta = parametro.cuenta;
                                                                    //movNuevo.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 12 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro;
                                                                    movNuevo.centro = listaelementos[i].tipo_tarifa == 2
                                                                        ? parametro.id_nombre_parametro == 12
                                                                            ?
                                                                            listaelementos[i].centro_costo
                                                                            : parametro.centro
                                                                        : parametro.centro;

                                                                    movNuevo.fec = DateTime.Now;
                                                                    movNuevo.fec_creacion = DateTime.Now;
                                                                    movNuevo.userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]);
                                                                    /*if (info.aplicaniff==true)
																	{

																	}*/

                                                                    if (info.manejabase)
                                                                    {
                                                                        movNuevo.basecontable =
                                                                            Convert.ToDecimal(valor_totalenca, miCultura);
                                                                    }
                                                                    else
                                                                    {
                                                                        movNuevo.basecontable = 0;
                                                                    }

                                                                    if (info.documeto)
                                                                    {
                                                                        movNuevo.documento =
                                                                            Convert.ToString(eg.numero);
                                                                    }

                                                                    if (buscarCuenta.concepniff == 1)
                                                                    {
                                                                        movNuevo.credito = 0;
                                                                        movNuevo.debito = Convert.ToDecimal(cr, miCultura);

                                                                        movNuevo.creditoniif = 0;
                                                                        movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                    }

                                                                    if (buscarCuenta.concepniff == 4)
                                                                    {
                                                                        movNuevo.creditoniif = 0;
                                                                        movNuevo.debitoniif = Convert.ToDecimal(cr, miCultura);
                                                                    }

                                                                    if (buscarCuenta.concepniff == 5)
                                                                    {
                                                                        movNuevo.credito = 0;
                                                                        movNuevo.debito = Convert.ToDecimal(cr, miCultura);
                                                                    }

                                                                    //context.mov_contable.Add(movNuevo);
                                                                }

                                                                #endregion

                                                                mov_contable buscarCosto =
                                                                    context.mov_contable.FirstOrDefault(x =>
                                                                        x.id_encab == id_encabezado &&
                                                                        x.cuenta == movNuevo.cuenta &&
                                                                        x.idparametronombre ==
                                                                        parametro.id_nombre_parametro);
                                                                if (buscarCosto != null)
                                                                {
                                                                    buscarCosto.basecontable += movNuevo.basecontable;
                                                                    buscarCosto.debito += movNuevo.debito;
                                                                    buscarCosto.debitoniif += movNuevo.debitoniif;
                                                                    buscarCosto.credito += movNuevo.credito;
                                                                    buscarCosto.creditoniif += movNuevo.creditoniif;
                                                                    context.Entry(buscarCosto).State =
                                                                        EntityState.Modified;
                                                                }
                                                                else
                                                                {
                                                                    mov_contable crearMovContable = new mov_contable
                                                                    {
                                                                        id_encab = encabezado.idencabezado,
                                                                        seq = secuencia,
                                                                        idparametronombre =
                                                                        parametro.id_nombre_parametro,
                                                                        cuenta =
                                                                        Convert.ToInt32(movNuevo.cuenta),
                                                                        //crearMovContable.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 12 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.centro : parametro.centro;
                                                                        centro =
                                                                        listaelementos[i].tipo_tarifa == 2
                                                                            ? parametro.id_nombre_parametro == 12
                                                                                ?
                                                                                listaelementos[i].centro_costo
                                                                                : parametro.centro
                                                                            : parametro.centro,

                                                                        nit = encabezado.nit,
                                                                        fec = DateTime.Now,
                                                                        debito = movNuevo.debito,
                                                                        debitoniif = movNuevo.debitoniif,
                                                                        basecontable =
                                                                        movNuevo.basecontable,
                                                                        credito = movNuevo.credito,
                                                                        creditoniif = movNuevo.creditoniif,
                                                                        fec_creacion = DateTime.Now,
                                                                        userid_creacion =
                                                                        Convert.ToInt32(Session["user_usuarioid"]),
                                                                        detalle =
                                                                        "Facturacion de Orden de Taller N° " +
                                                                        ordentaller.codigoentrada +
                                                                        " con consecutivo " + eg.numero,
                                                                        estado = true
                                                                    };
                                                                    context.mov_contable.Add(crearMovContable);
                                                                    context.SaveChanges();
                                                                }
                                                            }
                                                        }

                                                        #endregion

                                                        secuencia++;
                                                        //Cuentas valores

                                                        #region Cuentas valores

                                                        cuentas_valores buscar_cuentas_valores =
                                                            context.cuentas_valores.FirstOrDefault(x =>
                                                                x.centro == parametro.centro &&
                                                                x.cuenta == movNuevo.cuenta && x.nit == movNuevo.nit);
                                                        if (buscar_cuentas_valores != null)
                                                        {
                                                            buscar_cuentas_valores.debito +=
                                                                Math.Round(movNuevo.debito);
                                                            buscar_cuentas_valores.credito +=
                                                                Math.Round(movNuevo.credito);
                                                            buscar_cuentas_valores.debitoniff +=
                                                                Math.Round(movNuevo.debitoniif);
                                                            buscar_cuentas_valores.creditoniff +=
                                                                Math.Round(movNuevo.creditoniif);
                                                            //context.Entry(buscar_cuentas_valores).State = EntityState.Modified;
                                                            //context.SaveChanges();
                                                        }
                                                        else
                                                        {
                                                            DateTime fechaHoy = DateTime.Now;
                                                            cuentas_valores crearCuentaValor = new cuentas_valores
                                                            {
                                                                ano = fechaHoy.Year,
                                                                mes = fechaHoy.Month,
                                                                cuenta = movNuevo.cuenta,
                                                                //crearCuentaValor.centro = Request["tipo_tarifa_hidden_" + i] == "2" ? parametro.id_nombre_parametro == 11 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : parametro.id_nombre_parametro == 12 ? Convert.ToInt32(Request["centro_costo_tf" + i]) : movNuevo.centro : movNuevo.centro; ;
                                                                centro = listaelementos[i].tipo_tarifa == 2
                                                                ? parametro.id_nombre_parametro == 12
                                                                    ?
                                                                    listaelementos[i].centro_costo
                                                                    : parametro.centro
                                                                : parametro.centro,

                                                                nit = movNuevo.nit,
                                                                debito = Math.Round(movNuevo.debito),
                                                                credito = Math.Round(movNuevo.credito),
                                                                debitoniff =
                                                                Math.Round(movNuevo.debitoniif),
                                                                creditoniff =
                                                                Math.Round(movNuevo.creditoniif)
                                                            };
                                                            //context.cuentas_valores.Add(crearCuentaValor);
                                                            //context.SaveChanges();
                                                        }

                                                        #endregion

                                                        totalCreditos += Math.Round(movNuevo.credito);
                                                        totalDebitos += Math.Round(movNuevo.debito);
                                                        listaDescuadrados.Add(new DocumentoDescuadradoModel
                                                        {
                                                            NumeroCuenta =
                                                                "(" + buscarCuenta.cntpuc_numero + ")" +
                                                                buscarCuenta.cntpuc_descp,
                                                            DescripcionParametro = descripcionParametro,
                                                            ValorDebito = movNuevo.debito,
                                                            ValorCredito = movNuevo.credito
                                                        });
                                                    }
                                                }
                                            }

                                            #endregion
                                        } //fin de recorrer linea por linea

                                        #region validaciones para guardar

                                        if (Math.Round(totalDebitos) != Math.Round(totalCreditos))
                                        {
                                            TempData["mensaje_error"] =
                                                "El documento no tiene los movimientos calculados correctamente, verifique el perfil del documento";

                                            dbTran.Rollback();
                                            listasliquidacion(modelo);
                                            BuscarFavoritos(menu);
                                            return View(modelo);
                                            //return RedirectToAction("Facturar", "FacturacionRepuestos", new { menu });
                                        }

                                        funciono = 1;

                                        #endregion

                                        if (funciono > 0)
                                        {
                                            context.SaveChanges();
                                            dbTran.Commit();
                                            ordentaller.estadoorden = estadofacturada;
                                            ordentaller.fecha_liquidacion = DateTime.Now;
                                            context.Entry(ordentaller).State = EntityState.Modified;
                                            context.SaveChanges();
                                            TempData["mensaje"] = "Registro creado correctamente";
                                            DocumentoPorBodegaController conse = new DocumentoPorBodegaController();
                                            doc.ActualizarConsecutivo(grupo.grupo, consecutivo);

                                            return RedirectToAction("Facturar", "FacturacionRepuestos", new { menu });
                                        }

                                        //#endregions
                                    }
                                    else
                                    {
                                        TempData["mensaje_error"] = "no hay consecutivo";
                                    }
                                }
                                //cierre
                                else
                                {
                                    TempData["mensaje_error"] = "Lista vacia";
                                }
                            }
                            else
                            {
                                TempData["mensaje_error"] =
                                    "Error en liquidación de orden " + modelo.codigoentrada +
                                    " Mensaje: No existe perfil contable para el documento seleccionado y la bodega: " +
                                    bodegaconce.bodccs_nombre;
                            }
                        }
                        catch (DbEntityValidationException ex)
                        {
                            string mensaje = ex.Message;
                            dbTran.Rollback();
                            TempData["mensaje_error"] =
                                "Error en liquidación de orden " + modelo.codigoentrada + " Mensaje:" + mensaje;
                        }
                    }

                    return View(modelo);
                }

                TempData["mensaje_error"] = "Error en liquidación de orden " + modelo.codigoentrada +
                                            " : Debe llenar todos los campos obligatorios, así como seleccionar los tipos de tarifa de las operaciones y los repuestos.";
                listasliquidacion(modelo);
                BuscarFavoritos(menu);
                return View(modelo);
            }

            return RedirectToAction("Login", "Login");
        }

        public ActionResult crearPDFConstancia(int? id)
        {

            if (id != null)
            {
                encab_documento encab = context.encab_documento.Where(d => d.orden_taller == id && d.tipo == 3106).OrderByDescending(d => d.idencabezado).FirstOrDefault();
                tencabezaorden ot = context.tencabezaorden.Where(d => d.id == encab.orden_taller).FirstOrDefault();

                var cabeceraFactura = (from e in context.tencabezaorden
                                       join tp in context.tp_doc_registros
                                           on e.idtipodoc equals tp.tpdoc_id
                                       join b in context.bodega_concesionario
                                           on e.bodega equals b.id
                                       join t in context.icb_terceros
                                           on e.tercero equals t.tercero_id
                                       join emp in context.tablaempresa
                                           on b.concesionarioid equals emp.id
                                       where e.id == ot.id
                                       select new
                                       {
                                           id_emp = emp.id,
                                           nomemp = emp.nombre_empresa,
                                           diremp = emp.direccion,
                                           nitemp = emp.nit,
                                           tp.prefijo
                                       }).FirstOrDefault();

                var formapagoter = (from a in context.tencabezaorden
                                    join b in context.detalle_formas_pago_orden
                                        on a.id equals b.idorden
                                    join c in context.formas_pago
                                        on b.idformas_pago equals c.id
                                    where a.id == ot.id
                                    select new
                                    {
                                        c.id,
                                        c.formapago
                                    }).FirstOrDefault();

                var TipoFactura = (from a in context.tencabezaorden
                                   join b in context.tp_doc_registros
                                   on a.idtipodoc equals b.tpdoc_id
                                   where a.id == ot.id
                                   select new
                                   {
                                       b.prefijo,
                                       b.tpdoc_nombre
                                   }).FirstOrDefault();



                decimal total_descuentos_operaciones_internas = 0;
                decimal total_operaciones_internas_bruto = 0;
                decimal total_iva_operaciones_internas = 0;
                decimal total_operaciones_internas = 0;
                decimal total_operaciones_internas_siniva = 0;
                decimal total_operaciones_internas_baseiva = 0;


                decimal total_descuentos_operaciones_garantia = 0;
                decimal total_operaciones_garantia_bruto = 0;
                decimal total_iva_operaciones_garantia = 0;
                decimal total_operaciones_garantia = 0;
                decimal total_operaciones_garantia_siniva = 0;
                decimal total_operaciones_garantia_baseiva = 0;

                decimal totaltotal = 0;

                string nombretecnico = "";
                if (ot.idtecnico != null)
                {
                    users tecnico = context.users.Where(d => d.user_id == ot.idtecnico).FirstOrDefault();
                    if (tecnico != null)
                    {
                        nombretecnico = tecnico.user_nombre + " " + tecnico.user_apellido;
                    }
                }

                grupoconsecutivos grupoint = context.grupoconsecutivos.FirstOrDefault(x => x.documento_id == ot.tp_doc_registros.tpdoc_id && x.bodega_id == ot.bodega);

                DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                var bsconcecutivo = doc.BuscarConsecutivo(grupoint.grupo);

                PdfFacturaOT1 informe = new PdfFacturaOT1
                {
                    //Empresa
                    id_Empresa =cabeceraFactura.id_emp,
                    nomEmpresa = cabeceraFactura.nomemp != null ? cabeceraFactura.nomemp : "",
                    dirEmpresa = cabeceraFactura.diremp != null ? cabeceraFactura.diremp : "",
                    nitEmpresa = cabeceraFactura.nitemp != null ? cabeceraFactura.nitemp : "",
                    bodega = ot.bodega_concesionario.bodccs_nombre,

                    //Factura
                    numFactura = ot.codigoentrada != null ? ot.codigoentrada : "",
                    formapagoFactura = formapagoter != null ? formapagoter.formapago.Contains("Cupo") ? "CREDITO" : "CONTADO" : "CREDITO",
                    prefijo = cabeceraFactura.prefijo != null ? cabeceraFactura.prefijo : "",
                    concecutivo = bsconcecutivo,
                    codigoentrada = ot.codigoentrada != null ? ot.codigoentrada : "",
                    asesor = ot.users.user_nombre + " " + ot.users.user_apellido,
                    tecnico = nombretecnico != null ? nombretecnico : "",
                    fechavenceFactura = formapagoter != null ? formapagoter.formapago.Contains("Cupo") ? DateTime.Now.AddMonths(1).ToString() : DateTime.Now.AddMonths(1).ToString() : DateTime.Now.ToString("dd/MM/yyyy", new CultureInfo("en-US")),
                    tipo_factura = TipoFactura.prefijo != null ? TipoFactura.prefijo : "",
                    //Vehiculo
                    placa = ot.placa != null ? ot.placa : "",
                    vehiculo = ot.icb_vehiculo.marca_vehiculo.marcvh_nombre != null ? ot.icb_vehiculo.marca_vehiculo.marcvh_nombre : "",
                    modelo = ot.icb_vehiculo.modelo_vehiculo.modvh_nombre != null ? ot.icb_vehiculo.modelo_vehiculo.modvh_nombre : "",
                    serie = ot.icb_vehiculo.vin != null ? ot.icb_vehiculo.vin : "",

                    //Cliente
                    txtDocumentoCliente = ot.icb_terceros.doc_tercero != null ? ot.icb_terceros.doc_tercero : "",
                    nombrecliente = !string.IsNullOrWhiteSpace(ot.icb_terceros.razon_social)
                    ? ot.icb_terceros.razon_social
                    : ot.icb_terceros.prinom_tercero + " " + ot.icb_terceros.segnom_tercero + " " +
                      ot.icb_terceros.apellido_tercero + " " + ot.icb_terceros.segapellido_tercero,
                    telefonocliente = !string.IsNullOrWhiteSpace(ot.icb_terceros.telf_tercero)
                    ? ot.icb_terceros.telf_tercero
                    : "",
                    celularcliente = !string.IsNullOrWhiteSpace(ot.icb_terceros.celular_tercero)
                    ? ot.icb_terceros.celular_tercero
                    : "",
                    correocliente = !string.IsNullOrWhiteSpace(ot.icb_terceros.email_tercero)
                    ? ot.icb_terceros.email_tercero
                    : "",
                    ciudadcliente = ot.icb_terceros.ciu_id != null
                    ? context.nom_ciudad.Where(d => d.ciu_id == ot.icb_terceros.ciu_id.Value)
                        .Select(d => d.ciu_nombre).FirstOrDefault() : "",

                    aseguradora = ot.aseguradora != null ? ot.icb_aseguradoras.nombre : "",
                    poliza = ot.aseguradora != null ? !string.IsNullOrWhiteSpace(ot.poliza) ? ot.poliza : "" : "",

                };

                //listado de operaciones tipo interno y garantia

                List<tdetallemanoobraot> operaciones_internas = context.tdetallemanoobraot.Where(e => e.idorden == encab.orden_taller && e.estado == "1" && e.tipotarifa==2).ToList();
                informe.operaciones_interno = operaciones_internas.Select(x => new operaciones
                {
                    codigo = x.ttempario.codigo,
                    operacion = x.ttempario.operacion,
                    tecnico = x.ttecnicos.users.user_nombre + " " + x.ttecnicos.users.user_apellido,
                    tiempo = x.tiempo != null ? Math.Round(x.tiempo.Value).ToString() : "",
                    valorUnitario = Math.Round(Convert.ToDecimal(x.costopromedio)).ToString(),
                    valorBruto = Math.Round(calcularbruto(x.costopromedio, x.pordescuento, x.poriva)).ToString(),
                    valorBaseiva = Math.Round(calcularbruto(x.costopromedio, x.pordescuento, x.poriva) - calculardescuento(x.costopromedio, x.pordescuento)).ToString(),
                    porcentaje_iva = x.poriva != null ? x.poriva.Value.ToString("N2", new CultureInfo("is-IS")) : "",
                    porcentaje_descuento = x.pordescuento != null ? x.pordescuento.Value.ToString("N2", new CultureInfo("is-IS")) : "",
                    totaldescuento = Math.Round(calculardescuento(x.costopromedio, x.pordescuento)).ToString(),
                    totaliva = Math.Round(calculariva(x.costopromedio, x.pordescuento, x.poriva)).ToString(),
                    valor_total = x.costopromedio != null ? Math.Round(Convert.ToDecimal(x.costopromedio - calculardescuento(x.costopromedio, x.pordescuento) + calculariva(x.costopromedio, x.pordescuento, x.poriva))).ToString() : "0",

                }).ToList();

                List<tdetallemanoobraot> operaciones_garantia = context.tdetallemanoobraot.Where(e => e.idorden == encab.orden_taller && e.estado == "1" && e.tipotarifa == 4).ToList();
                informe.operaciones_garantia = operaciones_garantia.Select(x => new operaciones
                {
                    codigo = x.ttempario.codigo,
                    operacion = x.ttempario.operacion,
                    tecnico = x.ttecnicos.users.user_nombre + " " + x.ttecnicos.users.user_apellido,
                    tiempo = x.tiempo!= null ? Math.Round(x.tiempo.Value).ToString() : "",
                    valorUnitario = Math.Round(Convert.ToDecimal(x.costopromedio)).ToString(),
                    valorBruto = Math.Round(calcularbruto(x.costopromedio, x.pordescuento, x.poriva)).ToString(),
                    valorBaseiva = Math.Round(calcularbruto(x.costopromedio, x.pordescuento, x.poriva) - calculardescuento(x.costopromedio, x.pordescuento)).ToString(),
                    porcentaje_iva = x.poriva != null ? x.poriva.Value.ToString("N2", new CultureInfo("is-IS")) : "",
                    porcentaje_descuento = x.pordescuento != null ? x.pordescuento.Value.ToString("N2", new CultureInfo("is-IS")) : "",
                    totaldescuento = Math.Round(calculardescuento(x.costopromedio, x.pordescuento)).ToString(),
                    totaliva = Math.Round(calculariva(x.costopromedio, x.pordescuento, x.poriva)).ToString(),
                    valor_total = x.costopromedio != null ? Math.Round(Convert.ToDecimal(x.costopromedio - calculardescuento(x.costopromedio, x.pordescuento) + calculariva(x.costopromedio, x.pordescuento, x.poriva))).ToString() : "0",

                }).ToList();

                //totales operaciones
                total_operaciones_internas = informe.operaciones_interno.Sum(d => Convert.ToDecimal(d.valor_total));
                total_operaciones_internas_siniva = informe.operaciones_interno.Sum(d => Convert.ToInt32(d.valorUnitario));
                total_iva_operaciones_internas = informe.operaciones_interno.Sum(d => Convert.ToInt32(d.totaliva));
                total_descuentos_operaciones_internas = informe.operaciones_interno.Sum(d => Convert.ToInt32(d.totaldescuento));
                total_operaciones_internas_bruto = informe.operaciones_interno.Sum(d => Convert.ToInt32(d.valorBruto));
                total_operaciones_internas_baseiva = informe.operaciones_interno.Sum(d => Convert.ToInt32(d.valorBaseiva));

                total_operaciones_garantia = informe.operaciones_garantia.Sum(d => Convert.ToDecimal(d.valor_total));
                total_operaciones_garantia_siniva = informe.operaciones_garantia.Sum(d => Convert.ToInt32(d.valorUnitario));
                total_iva_operaciones_garantia = informe.operaciones_garantia.Sum(d => Convert.ToInt32(d.totaliva));
                total_descuentos_operaciones_garantia = informe.operaciones_garantia.Sum(d => Convert.ToInt32(d.totaldescuento));
                total_operaciones_garantia_bruto = informe.operaciones_garantia.Sum(d => Convert.ToInt32(d.valorBruto));
                total_operaciones_garantia_baseiva = informe.operaciones_garantia.Sum(d => Convert.ToInt32(d.valorBaseiva));

                decimal subtotal = total_operaciones_internas_siniva+ total_operaciones_garantia_siniva;
                decimal valorBruto = total_operaciones_internas_bruto+ total_operaciones_garantia_bruto;
                decimal totaliva = total_iva_operaciones_internas+ total_iva_operaciones_garantia;
                decimal valorBaseiva = total_operaciones_internas_baseiva+ total_operaciones_garantia_baseiva;
                decimal totalDescuentos = total_descuentos_operaciones_internas+ total_descuentos_operaciones_garantia;
                totaltotal = total_operaciones_internas+ total_operaciones_garantia;

                informe.subtotal = Math.Round(subtotal).ToString();
                informe.valorbruto = Math.Round(valorBruto).ToString();
                informe.totaldescuento = Math.Round(totalDescuentos).ToString();
                informe.totaliva = Math.Round(totaliva).ToString();
                informe.baseiva = Math.Round(valorBaseiva).ToString();
                informe.totalFactura = Math.Round(totaltotal).ToString();


                DocumentoPorBodegaController docu = new DocumentoPorBodegaController();
                var concecutivoNuevo = docu.ActualizarConsecutivo(grupoint.grupo, informe.concecutivo);

                //string nombre = "Constancia de Reparación";
                //string nombre2 = nombre;
                //nombre = nombre + "file.pdf";

                string root = Server.MapPath("~/Pdf/");
                string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
                string path = Path.Combine(root, pdfname);
                path = Path.GetFullPath(path);

                if (ot.fecha_impresion_cotizacion == null)
                {
                    ot.fecha_impresion_cotizacion = DateTime.Now;
                    context.Entry(ot).State = EntityState.Modified;
                    context.SaveChanges();
                }
                var codigo2 = informe.codigoentrada;

                //int swclasifica = Convert.ToInt32(context.icb_sysparameter.Where(z => z.syspar_cod == "P111").Select(x => x.syspar_value).FirstOrDefault());
                int swclasifica = Convert.ToInt32(context.icb_sysparameter.Where(z => z.syspar_cod == "P129").Select(x => x.syspar_value).FirstOrDefault());

                var dicodigo = context.encab_documento.Where(x => x.orden_taller == encab.orden_taller && x.tp_doc_registros.tp_doc_sw.sw == swclasifica).Select(s => new { codigo = s.tp_doc_registros.prefijo + "-" + s.numero }).FirstOrDefault();

                string customSwitches = string.Format("--print-media-type --allow {0} --header-html {0} --header-spacing 5 --footer-html {1} --footer-spacing 0",
                Url.Action("CabeceraPdfConstancia", "CentralAtencion", new { area = "", bodega = informe.bodega, codigoentrada = informe.codigoentrada,  fechavenceFactura = informe.fechavenceFactura, tipo_factura = informe.tipo_factura }, Request.Url.Scheme), Url.Action("PiePdfConstancia", "CentralAtencion", new { area = "" }, Request.Url.Scheme));

                ViewAsPdf something = new ViewAsPdf("crearPDFConstancia", informe)
                {
                    PageOrientation = Orientation.Landscape,
                    //FileName = nombre,
                    CustomSwitches = customSwitches,
                    PageSize = Size.Letter,
                    PageMargins = new Margins { Top = 40, Bottom = 20 }
                };
                return something;
            }
            return Json(0, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CabeceraPdfConstancia(string bodega, string fechavenceFactura, string codigoentrada)
        {
            var recibido = Request;
            var modelo2 = new PdfFacturaOT1
            {
                bodega = bodega,
                fechavenceFactura = fechavenceFactura,
                codigoentrada = codigoentrada
            };

            return View(modelo2);
        }

        public ActionResult PiePdfConstancia()
        {
            return View();
        }

        public ActionResult crearPDFCuadreDiario(int? id)
        {

            if (id != null)
            {
                var buscar = context.icb_cuadre_caja.Where(d => d.id == id).Select(d => d.caja).FirstOrDefault();


                decimal totalefectivo = 0;
                decimal totaltarjeta = 0;
                decimal totalCheque = 0;
                decimal totalcupo = 0;
                decimal totaltotal = 0;

                var medios_Pago = (from de in context.detalle_formas_pago_orden
                                  join te in context.tencabezaorden
                                      on de.idorden equals te.id
                                  join en in context.encab_documento
                                      on te.id equals en.orden_taller 
                                where de.caja == buscar //&& de.formas_pago.formapago.Contains("Efectivo")
                                   select new
                                  {
                                      en.idencabezado,
                                      en.orden_taller,
                                      en.numero,
                                      en.valor_total,
                                      de.idformas_pago,
                                      de.formas_pago.formapago,
                                      de.caja,
                                      de.valor,
                                  }).ToList();


                PdfCuadre informe = new PdfCuadre { };

                informe.efectivo = medios_Pago.Where(x=> x.formapago.Contains("Efectivo")).Select(x => new mediosPago
                {

                 id_Medio= Convert.ToInt32(x.idformas_pago),
                 nomMedio= x.formapago,
                 id_Factura = x.idencabezado,
                 numFactura = x.numero,
                 ValorFactura = x.valor_total,
                 ValorMedio = Math.Round(Convert.ToDecimal(x.valor)),
                }).ToList();

                informe.tarjetas = medios_Pago.Where(x => x.formapago.Contains("Tarjeta")).Select(x => new mediosPago
                {

                    id_Medio = Convert.ToInt32(x.idformas_pago),
                    nomMedio = x.formapago,
                    id_Factura = x.idencabezado,
                    numFactura = x.numero,
                    ValorFactura = x.valor_total,
                    ValorMedio = Math.Round(Convert.ToDecimal(x.valor)),

                }).ToList();

                informe.cheque = medios_Pago.Where(x => x.formapago.Contains("Cheque")).Select(x => new mediosPago
                {

                    id_Medio = Convert.ToInt32(x.idformas_pago),
                    nomMedio = x.formapago,
                    id_Factura = x.idencabezado,
                    numFactura = x.numero,
                    ValorFactura = x.valor_total,
                    ValorMedio = Math.Round(Convert.ToDecimal(x.valor)),

                }).ToList();

                informe.cupo = medios_Pago.Where(x => x.formapago.Contains("Cupo")).Select(x => new mediosPago
                {

                    id_Medio = Convert.ToInt32(x.idformas_pago),
                    nomMedio = x.formapago,
                    id_Factura = x.idencabezado,
                    numFactura = x.numero,
                    ValorFactura = x.valor_total,
                    ValorMedio = Math.Round(Convert.ToDecimal(x.valor)),

                }).ToList();

                totalefectivo = informe.efectivo.Sum(d => Convert.ToDecimal(d.ValorMedio));
                totaltarjeta = informe.tarjetas.Sum(d => Convert.ToDecimal(d.ValorMedio));
                totalCheque = informe.cheque.Sum(d => Convert.ToDecimal(d.ValorMedio));
                totalcupo = informe.cupo.Sum(d => Convert.ToDecimal(d.ValorMedio));

                totaltotal = totalefectivo + totaltarjeta+ totalCheque+ totalcupo;

                informe.totalefectivo = Math.Round(totalefectivo).ToString();
                informe.totaltarjeta = Math.Round(totaltarjeta).ToString();
                informe.totalCheque = Math.Round(totalCheque).ToString();
                informe.totalcupo = Math.Round(totalcupo).ToString();

                informe.totalGeneral= Math.Round(totaltotal).ToString();

                string root = Server.MapPath("~/Pdf/");
                string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
                string path = Path.Combine(root, pdfname);
                path = Path.GetFullPath(path);

                string customSwitches = string.Format("--print-media-type --allow {0} --header-html {0} --header-spacing 5 --footer-html {1} --footer-spacing 0",
                Url.Action("CabeceraPdfCuadre", "CentralAtencion", new { area = "" }, Request.Url.Scheme), Url.Action("PiePdfCuadre", "CentralAtencion", new { area = "" }, Request.Url.Scheme));

                ViewAsPdf something = new ViewAsPdf("crearPDFCuadreDiario", informe)
                {
                    PageOrientation = Orientation.Landscape,
                    //FileName = nombre,
                    CustomSwitches = customSwitches,
                    PageSize = Size.Letter,
                    PageMargins = new Margins { Top = 40, Bottom = 20 }
                };

                return something;
            }
            return Json(0, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult CabeceraPdfCuadre()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult PiePdfCuadre()
        {
            return View();
        }

        public ActionResult crearFactura(int? id)
        {

            if (id != null)
            {
                encab_documento encab = context.encab_documento.Where(d => d.idencabezado==id && d.tipo == 3080).OrderByDescending(d => d.idencabezado).FirstOrDefault();
                tencabezaorden ot = context.tencabezaorden.Where(d => d.id == encab.orden_taller).FirstOrDefault();

                var cabeceraFactura = (from e in context.tencabezaorden
                                       join tp in context.tp_doc_registros
                                           on e.idtipodoc equals tp.tpdoc_id
                                       join b in context.bodega_concesionario
                                           on e.bodega equals b.id
                                       join t in context.icb_terceros
                                           on e.tercero equals t.tercero_id
                                       join emp in context.tablaempresa
                                           on b.concesionarioid equals emp.id
                                       where e.id == ot.id
                                       select new
                                       {
                                           id_emp = emp.id,
                                           nomemp = emp.nombre_empresa,
                                           diremp = emp.direccion,
                                           nitemp = emp.nit,
                                           tp.prefijo
                                       }).FirstOrDefault();

                var formapagoter = (from a in context.tencabezaorden
                                    join b in context.detalle_formas_pago_orden
                                        on a.id equals b.idorden
                                    join c in context.formas_pago
                                        on b.idformas_pago equals c.id
                                    where a.id == ot.id
                                    select new
                                    {
                                        c.id,
                                        c.formapago
                                    }).FirstOrDefault();

                var TipoFactura = (from a in context.tencabezaorden
                                   join b in context.tp_doc_registros
                                   on a.idtipodoc equals b.tpdoc_id
                                   where a.id == ot.id
                                   select new
                                   {
                                       b.prefijo,
                                       b.tpdoc_nombre
                                   }).FirstOrDefault();



                decimal totaldescuentosoperaciones = 0;
                decimal totaloperacionesbruto = 0;
                decimal totalivaoperaciones = 0;
                decimal totaloperaciones = 0;
                decimal totaloperacionesiniva = 0;
                decimal totaloperacionesbaseiva = 0;

                decimal totaldescuentorepuestos = 0;
                decimal repuestosbruto = 0;
                decimal totalivarepuestos = 0;
                decimal totalrepuestos = 0;
                decimal totalrepuestosiniva = 0;
                decimal totalrepuestosbaseiva = 0;
                //decimal totalflete = 0;

                decimal totaltotal = 0;

                string nombretecnico = "";
                if (ot.idtecnico != null)
                {
                    users tecnico = context.users.Where(d => d.user_id == ot.idtecnico).FirstOrDefault();
                    if (tecnico != null)
                    {
                        nombretecnico = tecnico.user_nombre + " " + tecnico.user_apellido;
                    }
                }

                grupoconsecutivos grupoint = context.grupoconsecutivos.FirstOrDefault(x => x.documento_id == ot.tp_doc_registros.tpdoc_id && x.bodega_id == ot.bodega);

                DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                var bsconcecutivo = doc.BuscarConsecutivo(grupoint.grupo);

                PdfFacturaOT1 informe = new PdfFacturaOT1
                {
                    //Empresa
                    id_Empresa = cabeceraFactura.id_emp,
                    nomEmpresa = cabeceraFactura.nomemp != null ? cabeceraFactura.nomemp : "",
                    dirEmpresa = cabeceraFactura.diremp != null ? cabeceraFactura.diremp : "",
                    nitEmpresa = cabeceraFactura.nitemp != null ? cabeceraFactura.nitemp : "",
                    bodega = ot.bodega_concesionario.bodccs_nombre,

                    //Factura
                    numFactura = ot.codigoentrada != null ? ot.codigoentrada : "",
                    formapagoFactura = formapagoter != null ? formapagoter.formapago.Contains("Cupo") ? "CREDITO" : "CONTADO" : "CREDITO",
                    prefijo = cabeceraFactura.prefijo != null ? cabeceraFactura.prefijo : "",
                    concecutivo = bsconcecutivo,
                    codigoentrada = ot.codigoentrada != null ? ot.codigoentrada : "",
                    asesor = ot.users.user_nombre + " " + ot.users.user_apellido,
                    tecnico = nombretecnico != null ? nombretecnico : "",
                    fechavenceFactura = formapagoter != null ? formapagoter.formapago.Contains("Cupo") ? DateTime.Now.AddMonths(1).ToString() : DateTime.Now.AddMonths(1).ToString() : DateTime.Now.ToString("dd/MM/yyyy", new CultureInfo("en-US")),
                    tipo_factura = TipoFactura.prefijo != null ? TipoFactura.prefijo : "",
                    //Vehiculo
                    placa = ot.placa != null ? ot.placa : "",
                    vehiculo = ot.icb_vehiculo.marca_vehiculo.marcvh_nombre != null ? ot.icb_vehiculo.marca_vehiculo.marcvh_nombre : "",
                    modelo = ot.icb_vehiculo.modelo_vehiculo.modvh_nombre != null ? ot.icb_vehiculo.modelo_vehiculo.modvh_nombre : "",
                    serie = ot.icb_vehiculo.vin != null ? ot.icb_vehiculo.vin : "",

                    //Cliente
                    txtDocumentoCliente = ot.icb_terceros.doc_tercero != null ? ot.icb_terceros.doc_tercero : "",
                    nombrecliente = !string.IsNullOrWhiteSpace(ot.icb_terceros.razon_social)
                    ? ot.icb_terceros.razon_social
                    : ot.icb_terceros.prinom_tercero + " " + ot.icb_terceros.segnom_tercero + " " +
                      ot.icb_terceros.apellido_tercero + " " + ot.icb_terceros.segapellido_tercero,
                    telefonocliente = !string.IsNullOrWhiteSpace(ot.icb_terceros.telf_tercero)
                    ? ot.icb_terceros.telf_tercero
                    : "",
                    celularcliente = !string.IsNullOrWhiteSpace(ot.icb_terceros.celular_tercero)
                    ? ot.icb_terceros.celular_tercero
                    : "",
                    correocliente = !string.IsNullOrWhiteSpace(ot.icb_terceros.email_tercero)
                    ? ot.icb_terceros.email_tercero
                    : "",
                    ciudadcliente = ot.icb_terceros.ciu_id != null
                    ? context.nom_ciudad.Where(d => d.ciu_id == ot.icb_terceros.ciu_id.Value)
                        .Select(d => d.ciu_nombre).FirstOrDefault() : "",

                    aseguradora = ot.aseguradora != null ? ot.icb_aseguradoras.nombre : "",
                    poliza = ot.aseguradora != null ? !string.IsNullOrWhiteSpace(ot.poliza) ? ot.poliza : "" : "",

                };

                
                List<vw_tot> tots = context.vw_tot.Where(d => d.numot == id).ToList();

                informe.TOT= tots.Select(d=> new TOT {

                    tot = d.id2,
                    fecha = d.fecha2,
                    proveedor = d.proveedor,
                    valor_Total = d.valor_total2,

                }).ToList();

                var total_tot = tots.Sum(d => Convert.ToDecimal(d.valor_total));

                var buscarCliente = context.tercero_cliente.Where(d => d.tercero_id == ot.tercero)
                                        .Select(d => new { d.lprecios_repuestos, d.dscto_rep }).FirstOrDefault();

                //listado de operaciones
                List<tdetallemanoobraot> operaciones = context.tdetallemanoobraot.Where(e => e.idorden == encab.orden_taller && e.estado == "1").ToList();
                informe.operacionesOT = operaciones.Select(x => new operaciones
                {
                    codigo = x.ttempario.codigo,
                    operacion = x.ttempario.operacion,
                    tecnico = x.ttecnicos.users.user_nombre + " " + x.ttecnicos.users.user_apellido,
                    tiempo = x.tiempo != null ? Math.Round(x.tiempo.Value).ToString() : "",
                    valorUnitario = Math.Round(x.valorunitario).ToString(),
                    valorBruto= Math.Round(calcularbruto(x.valorunitario, x.pordescuento, x.poriva)).ToString(),
                    valorBaseiva= Math.Round(calcularbruto(x.valorunitario, x.pordescuento, x.poriva) - calculardescuento(x.valorunitario, x.pordescuento)).ToString(),
                    porcentaje_iva = x.poriva != null ? x.poriva.Value.ToString("N2", new CultureInfo("is-IS")) : "",
                    porcentaje_descuento = x.pordescuento != null ? x.pordescuento.Value.ToString("N2", new CultureInfo("is-IS")) : "",
                    totaldescuento= Math.Round(calculardescuento(x.valorunitario, x.pordescuento)).ToString(),
                    totaliva = Math.Round(calculariva(x.valorunitario, x.pordescuento, x.poriva)).ToString(),
                    valor_total = Math.Round(Convert.ToDecimal(x.valorunitario - calculardescuento(x.valorunitario, x.pordescuento) + calculariva(x.valorunitario, x.pordescuento, x.poriva))).ToString(),

                }).ToList();

                //repuestos de la orden
                List<tsolicitudrepuestosot> listarepuestos = context.tsolicitudrepuestosot.Where(e => e.idorden == encab.orden_taller && e.recibido).ToList();
                informe.repuestos = listarepuestos.Select(e => new repuestos
                {
                    id = e.tdetallerepuestosot.id,
                    codigo = !string.IsNullOrWhiteSpace(e.reemplazo) ? e.reemplazo : e.idrepuesto,
                        nombre = !string.IsNullOrWhiteSpace(e.reemplazo)
                            ? e.icb_referencia1.ref_descripcion
                            : e.icb_referencia.ref_descripcion,
                        cantidad = e.canttraslado.ToString(),
                        recibidorep = e.recibido,
                        tarifatipo = Convert.ToInt32(e.tdetallerepuestosot.tipotarifa),
                        centro_costo = e.tdetallerepuestosot.idcentro ?? 0,
                        precio_unitario = e.tdetallerepuestosot.valorunitario.ToString("N0", new CultureInfo("is-IS")),
                        valorBruto = Math.Round(calcularbruto(e.tdetallerepuestosot.valorunitario, e.tdetallerepuestosot.pordescto, e.tdetallerepuestosot.poriva)).ToString(),
                        valorBaseiva= Math.Round(calcularbruto(e.tdetallerepuestosot.valorunitario, e.tdetallerepuestosot.pordescto, e.tdetallerepuestosot.poriva)- calculardescuentore(e.tdetallerepuestosot.valorunitario, e.canttraslado, e.tdetallerepuestosot.pordescto)).ToString(),
                        porcentaje_iva = e.tdetallerepuestosot.poriva.ToString("N2", new CultureInfo("is-IS")),
                        porcentaje_descuento = e.tdetallerepuestosot.pordescto.ToString("N2", new CultureInfo("is-IS")),
                        totaldescuento = Math.Round(calculardescuentore(e.tdetallerepuestosot.valorunitario, e.canttraslado, e.tdetallerepuestosot.pordescto)).ToString(),
                        totaliva = Math.Round(calcularivare(e.tdetallerepuestosot.valorunitario, e.canttraslado, e.tdetallerepuestosot.pordescto, e.tdetallerepuestosot.poriva)).ToString(),
                        precio_total = Math.Round(Convert.ToDecimal(e.tdetallerepuestosot.valorunitario * e.canttraslado, miCultura) -
                                Convert.ToDecimal(calculardescuentore(e.tdetallerepuestosot.valorunitario,
                                    e.canttraslado, e.tdetallerepuestosot.pordescto), miCultura) +
                                Convert.ToDecimal(calcularivare(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                                    e.tdetallerepuestosot.pordescto, e.tdetallerepuestosot.poriva), miCultura)).ToString(),
                        totalsiniva = Math.Round(Convert.ToDecimal(e.tdetallerepuestosot.valorunitario * e.canttraslado, miCultura)).ToString(),

                }).ToList();

                //totales operaciones
                totaloperaciones = informe.operacionesOT.Sum(d => Convert.ToDecimal(d.valor_total));
                totaloperacionesiniva = informe.operacionesOT.Sum(d => Convert.ToInt32(d.valorUnitario));
                totalivaoperaciones = informe.operacionesOT.Sum(d => Convert.ToInt32(d.totaliva));
                totaldescuentosoperaciones = informe.operacionesOT.Sum(d => Convert.ToInt32(d.totaldescuento));
                totaloperacionesbruto=informe.operacionesOT.Sum(d => Convert.ToInt32(d.valorBruto));
                totaloperacionesbaseiva=informe.operacionesOT.Sum(d => Convert.ToInt32(d.valorBaseiva));
                //totales repuestos
                totalrepuestos = informe.repuestos.Sum(d => Convert.ToInt32(d.precio_total));
                totalivarepuestos = informe.repuestos.Sum(d => Convert.ToInt32(d.totaliva));
                totaldescuentorepuestos = informe.repuestos.Sum(d => Convert.ToInt32(d.totaldescuento));
                totalrepuestosiniva = informe.repuestos.Sum(d => Convert.ToInt32(d.totalsiniva));
                repuestosbruto = informe.repuestos.Sum(d => Convert.ToInt32(d.valorBruto));
                totalrepuestosbaseiva = informe.repuestos.Sum(d => Convert.ToInt32(d.valorBaseiva));

                
                decimal subtotal = totaloperacionesiniva + totalrepuestosiniva;
                decimal valorBruto = totaloperacionesbruto + repuestosbruto;
                decimal totaliva = totalivaoperaciones + totalivarepuestos;
                decimal valorBaseiva = totaloperacionesbaseiva + totalrepuestosbaseiva;
                decimal totalDescuentos = totaldescuentosoperaciones + totaldescuentorepuestos;
                totaltotal = totalrepuestos + totaloperaciones+ total_tot;

                informe.subtotal = Math.Round(subtotal).ToString();
                informe.valorbruto= Math.Round(valorBruto).ToString();
                informe.totaldescuento = Math.Round(totalDescuentos).ToString();
                informe.totaliva = Math.Round(totaliva).ToString();
                informe.baseiva= Math.Round(valorBaseiva).ToString();
                informe.totalFactura = Math.Round(totaltotal).ToString();
                

                DocumentoPorBodegaController docu = new DocumentoPorBodegaController();
                var concecutivoNuevo = docu.ActualizarConsecutivo(grupoint.grupo, informe.concecutivo);

                //string nombre = "Factura_OT";
                //string nombre2 = nombre;
                //nombre = nombre + "file.pdf";

                string root = Server.MapPath("~/Pdf/");
                string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
                string path = Path.Combine(root, pdfname);
                path = Path.GetFullPath(path);

                if (ot.fecha_impresion_cotizacion == null)
                {
                    ot.fecha_impresion_cotizacion = DateTime.Now;
                    context.Entry(ot).State = EntityState.Modified;
                    context.SaveChanges();
                }
                var codigo2 = informe.codigoentrada;

                //int swclasifica = Convert.ToInt32(context.icb_sysparameter.Where(z => z.syspar_cod == "P111").Select(x => x.syspar_value).FirstOrDefault());
                int swclasifica = Convert.ToInt32(context.icb_sysparameter.Where(z => z.syspar_cod == "P129").Select(x => x.syspar_value).FirstOrDefault());

                var dicodigo = context.encab_documento.Where(x => x.orden_taller == encab.orden_taller && x.tp_doc_registros.tp_doc_sw.sw == swclasifica).Select(s => new { codigo = s.tp_doc_registros.prefijo + "-" + s.numero }).FirstOrDefault();

                string customSwitches = string.Format("--print-media-type --allow {0} --header-html {0} --header-spacing 5 --footer-html {1} --footer-spacing 0",
                Url.Action("CabeceraFacturaPDF", "CentralAtencion", new { area = "", bodega = informe.bodega, prefijo = dicodigo.codigo, formapagoFactura = informe.formapagoFactura, fechavenceFactura = informe.fechavenceFactura, tipo_factura = informe.tipo_factura }, Request.Url.Scheme), Url.Action("PieFacturaPDF", "CentralAtencion", new { area = "" }, Request.Url.Scheme));

                ViewAsPdf something = new ViewAsPdf("crearFactura", informe)
                {
                    PageOrientation = Orientation.Landscape,
                    //FileName = nombre,
                    CustomSwitches = customSwitches,
                    PageSize = Size.Letter,
                    PageMargins = new Margins { Top = 40, Bottom = 20 }
                };
                return something;
            }
            
            return Json(0, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult CabeceraFacturaPDF(string bodega, string formapagoFactura,string fechavenceFactura,string tipo_factura, string prefijo)
        {
            var recibido = Request;
            var modelo2 = new PdfFacturaOT1
            {
                bodega = bodega,
                formapagoFactura = formapagoFactura,
                fechavenceFactura= fechavenceFactura,
                tipo_factura= tipo_factura,
                prefijo=prefijo
            };

            return View(modelo2);
        }

        [AllowAnonymous]
        public ActionResult PieFacturaPDF()
        {
            return View();
        }

        public ActionResult crearPreFactura(string codigoentrada)
        {
            if (!string.IsNullOrWhiteSpace(codigoentrada))
            {
                tencabezaorden ot = context.tencabezaorden.Where(d => d.codigoentrada == codigoentrada).FirstOrDefault();

                if (ot != null)
                {



                    string nombretecnico = "";
                    if (ot.idtecnico != null)
                    {
                        users tecnico = context.users.Where(d => d.user_id == ot.idtecnico).FirstOrDefault();
                        if (tecnico != null)
                        {
                            nombretecnico = tecnico.user_nombre + " " + tecnico.user_apellido;
                        }
                    }

                    grupoconsecutivos grupoint = context.grupoconsecutivos.FirstOrDefault(x => x.documento_id == ot.tp_doc_registros.tpdoc_id && x.bodega_id == ot.bodega);

                    DocumentoPorBodegaController doc = new DocumentoPorBodegaController();
                    var concecutivo= doc.BuscarConsecutivo(grupoint.grupo);

                    var Inspeccion = (from v in context.icb_vehiculo
                                 join o in context.tencabezaorden
                                 on v.plan_mayor equals o.placa
                                 join t in context.tcitastaller
                                 on o.idcita equals t.id
                                 join p in context.tplanmantenimiento
                                 on t.id_tipo_inspeccion equals p.id
                                 where o.placa == ot.placa && o.codigoentrada == ot.codigoentrada
                                 select new
                                 {
                                     p.id,
                                     p.Descripcion,
                                     v.kilometraje
                                 }).FirstOrDefault();

                    PDFPrefactura informe = new PDFPrefactura
                    {
                        codigoentrada = ot.codigoentrada != null ? ot.codigoentrada : "",
                        placa = ot.placa != null ? ot.placa : "",
                        vehiculo = ot.icb_vehiculo.marca_vehiculo.marcvh_nombre != null ? ot.icb_vehiculo.marca_vehiculo.marcvh_nombre : "",
                        modelo = ot.icb_vehiculo.modelo_vehiculo.modvh_nombre != null ? ot.icb_vehiculo.modelo_vehiculo.modvh_nombre : "",
                        serie = ot.icb_vehiculo.vin != null ? ot.icb_vehiculo.vin : "",
                        kilometraje = ot.icb_vehiculo.kilometraje,
                        Inspeccion= Inspeccion.Descripcion != null ? Inspeccion.Descripcion: "",
                        txtDocumentoCliente = ot.icb_terceros.doc_tercero != null ? ot.icb_terceros.doc_tercero : "",
                        nombrecliente = !string.IsNullOrWhiteSpace(ot.icb_terceros.razon_social)
                        ? ot.icb_terceros.razon_social
                        : ot.icb_terceros.prinom_tercero + " " + ot.icb_terceros.segnom_tercero + " " +
                          ot.icb_terceros.apellido_tercero + " " + ot.icb_terceros.segapellido_tercero,
                        telefonocliente = !string.IsNullOrWhiteSpace(ot.icb_terceros.telf_tercero)
                        ? ot.icb_terceros.telf_tercero
                        : "",
                        celularcliente = !string.IsNullOrWhiteSpace(ot.icb_terceros.celular_tercero)
                        ? ot.icb_terceros.celular_tercero
                        : "",
                        correocliente = !string.IsNullOrWhiteSpace(ot.icb_terceros.email_tercero)
                        ? ot.icb_terceros.email_tercero
                        : "",
                        ciudadcliente = ot.icb_terceros.ciu_id != null
                        ? context.nom_ciudad.Where(d => d.ciu_id == ot.icb_terceros.ciu_id.Value)
                            .Select(d => d.ciu_nombre).FirstOrDefault()
                        : "",
                        bodega = ot.bodega_concesionario.bodccs_nombre,

                        //prefijo = ot.tp_doc_registros.prefijo != null ? ot.tp_doc_registros.prefijo : "",
                        concecutivo = concecutivo,

                        asesor = ot.users.user_nombre + " " + ot.users.user_apellido,
                        tecnico = nombretecnico != null ? nombretecnico : "",
                        aseguradora = ot.aseguradora != null ? ot.icb_aseguradoras.nombre : "",
                        poliza = ot.aseguradora != null ? !string.IsNullOrWhiteSpace(ot.poliza) ? ot.poliza : "" : ""
                    };

                    List<tdetallemanoobraot> operaciones = context.tdetallemanoobraot
                        .Where(d => d.idorden == ot.id && d.tipotarifa == 4).ToList();

                    informe.operacionesGarantia = operaciones.Select(x => new operaciones
                    {
                        codigo = x.idtempario,
                        operacion = x.ttempario.operacion,
                        tiempo = x.tiempo != null ? x.tiempo.Value.ToString() : "",
                        tecnico = x.ttecnicos.users.user_nombre + " " + x.ttecnicos.users.user_apellido,
                        valor_total = (x.tiempo != 0
                                ? x.valorunitario * (decimal)x.tiempo -
                                  x.valorunitario * x.pordescuento / 100 * (decimal)x.tiempo +
                                  (x.valorunitario * (decimal)x.tiempo -
                                   x.valorunitario * x.pordescuento / 100 * (decimal)x.tiempo) * x.poriva / 100
                                : x.valorunitario * 1 - x.valorunitario * x.pordescuento / 100 * 1 +
                                  (x.valorunitario * 1 - x.valorunitario * x.pordescuento / 100 * 1) * x.poriva / 100)
                            .ToString(),

                        valor_total2 = x.tiempo != 0
                            ? x.valorunitario * (decimal)x.tiempo -
                              x.valorunitario * x.pordescuento / 100 * (decimal)x.tiempo +
                              (x.valorunitario * (decimal)x.tiempo -
                               x.valorunitario * x.pordescuento / 100 * (decimal)x.tiempo) * x.poriva / 100
                            : x.valorunitario * 1 - x.valorunitario * x.pordescuento / 100 * 1 +
                              (x.valorunitario * 1 - x.valorunitario * x.pordescuento / 100 * 1) * x.poriva / 100
                    }).ToList();

                    informe.totaltiempooperaciones = operaciones.Sum(d => d.tiempo != null ? d.tiempo : 0).ToString();
                    informe.totalvaloroperaciones = informe.operacionesGarantia
                        .Sum(d => d.valor_total2 != null ? d.valor_total2 : 0).ToString();
                    //informe.totaldescuento = calculardescuento(x.valorunitario, x.pordescuento);
                    //    informe.totaliva = calculariva(x.valorunitario, x.pordescuento, x.poriva);


                    string nombre = "PreFactura";
                    string nombre2 = nombre;
                    nombre = nombre + "file.pdf";

                    string root = Server.MapPath("~/Pdf/");
                    string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
                    string path = Path.Combine(root, pdfname);
                    path = Path.GetFullPath(path);

                    int swclasifica = Convert.ToInt32(context.icb_sysparameter.Where(z => z.syspar_cod == "P150").Select(x => x.syspar_value).FirstOrDefault());

                    var dicodigo = context.encab_documento.Where(x => x.orden_taller == ot.id && x.tp_doc_registros.tp_doc_sw.sw == swclasifica).Select(s => new { codigo = s.tp_doc_registros.prefijo + "-" + s.numero }).FirstOrDefault();

                    DocumentoPorBodegaController docu = new DocumentoPorBodegaController();
                    var concecutivoNuevo = docu.ActualizarConsecutivo(grupoint.grupo, informe.concecutivo);

                    var codigo2 = informe.codigoentrada;

                    string customSwitches = string.Format("--print-media-type --allow {0} --header-html {0} --header-spacing 10 --footer-html {1} --footer-spacing 0",
                    Url.Action("CabeceraPDF", "CentralAtencion", new { area = "", bodega = informe.bodega, prefijo = dicodigo.codigo, concecutivo = informe.concecutivo }, Request.Url.Scheme), Url.Action("PiePDF", "CentralAtencion", new { area = "" }, Request.Url.Scheme));

                    ViewAsPdf something = new ViewAsPdf("crearPreFactura", informe)
                    {
                        PageOrientation = Orientation.Landscape,
                        FileName = nombre,
                        CustomSwitches = customSwitches,
                        PageSize = Size.Letter,
                        PageMargins = new Margins { Top = 60, Bottom = 5 }
                    };

                    return something;


                }
                return Json(0, JsonRequestBehavior.AllowGet);

            }

            return Json(0, JsonRequestBehavior.AllowGet);
        }


        [AllowAnonymous]
        public ActionResult CabeceraPDF(string bodega, string prefijo, long concecutivo)
        {
            var recibido = Request;
            var modelo2 = new PDFPrefactura
            {
                bodega = bodega,
                prefijo = prefijo,
                concecutivo = concecutivo
            };

            return View(modelo2);
        }

        [AllowAnonymous]
        public ActionResult PiePDF()
        {
            return View();
        }

        public ActionResult BrowserAveriasCA(int? menu)
        {
            IOrderedEnumerable<tallerAveria> tallerAverias = context.tallerAveria.ToList().OrderBy(x => x.nombre);
            List<SelectListItem> lisTaller = new List<SelectListItem>();
            foreach (tallerAveria item in tallerAverias)
            {
                lisTaller.Add(new SelectListItem
                {
                    Text = item.nombre,
                    Value = item.id.ToString()
                });
            }
            ViewBag.tallerAveria = lisTaller;
            BuscarFavoritos(menu);
            return View();
        }

        public void listasliquidacion(formularioliquidacionOT orden)
        {

            var tecnicos = context.ttecnicos.Where(d => d.estado && d.users.user_estado).Select(d =>
            new { id = d.users.user_id, nombre = d.users.user_nombre + " " + d.users.user_apellido }).ToList();
            ViewBag.tecnico = new SelectList(tecnicos, "id", "nombre", orden.tecnico);

            var razones = context.trazonesingreso.Where(d => d.estado).Select(d => new { d.id, nombre = d.razoz_ingreso }).ToList();

            ViewBag.razon_ingreso = new SelectList(razones, "id", "nombre", orden.razon_ingreso);
            ViewBag.razon_ingreso2 = new SelectList(razones, "id", "nombre", orden.razon_ingreso2);

            icb_sysparameter parametro1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P115").FirstOrDefault();
            int tipodocumento = parametro1 != null ? Convert.ToInt32(parametro1.syspar_value) : 28;

            icb_sysparameter parametro11 = context.icb_sysparameter.Where(d => d.syspar_cod == "P129").FirstOrDefault();
            int swiche = parametro11 != null ? Convert.ToInt32(parametro11.syspar_value) : 3;

            //parametro orden de taller prefactura
            icb_sysparameter parametro2 = context.icb_sysparameter.Where(d => d.syspar_cod == "P150").FirstOrDefault();
            int estadoprefacturada = parametro2 != null ? Convert.ToInt32(parametro2.syspar_value) : 25;

            var documentos = context.tp_doc_registros.Where(d => d.tpdoc_estado && d.tipo == tipodocumento && d.sw==swiche && d.perfil_contable_documento.Count()>0)
            .Select(d => new { id = d.tpdoc_id, nombre = d.tpdoc_nombre }).ToList();
            ViewBag.tipoDocumento = new SelectList(documentos, "id", "nombre", orden.tipoDocumento);

            ViewBag.operacionesTempario = new SelectList(context.ttempario, "codigo", "operacion");

            var buscarCentro = context.centro_costo.Where(x => x.bodega == orden.bodega).Select(x => new
            {
                id = x.centcst_id,
                nombre = x.pre_centcst + " - " + x.Tipos_Cartera.descripcion
            }).ToList();
            ViewBag.cartera = new SelectList(buscarCentro, "id", "nombre", orden.cartera);

            ViewBag.aseguradora = new SelectList(context.icb_aseguradoras, "aseg_id", "nombre", orden.aseguradora);


            //razones de ingreso


            var buscarRepuestos = (from referencia in context.icb_referencia
                                   where referencia.modulo == "R" && referencia.ref_estado
                                   select new
                                   {
                                       referencia.ref_codigo,
                                       ref_descripcion = referencia.ref_codigo + " - " + referencia.ref_descripcion,
                                       referencia.manejo_inv
                                   }).ToList();
            ViewBag.repuestos = new SelectList(buscarRepuestos, "ref_codigo", "ref_descripcion");
            ViewBag.repuestosG = new SelectList(buscarRepuestos.Where(x => x.manejo_inv == false), "ref_codigo",
                "ref_descripcion");
            ViewBag.aseguradora = new SelectList(context.icb_aseguradoras, "aseg_id", "nombre", orden.aseguradora);

            var perfiles = context.perfil_contable_documento.Take(0).Select(d => new { d.id, nombre = d.descripcion })
                .ToList();
            if (orden.tipoDocumento != null)
            {
                perfiles = context.perfil_contable_documento
                    .Where(d => d.tipo == orden.tipoDocumento.Value &&
                                d.perfil_contable_bodega.Where(e => e.idbodega == orden.bodega).Count() > 0)
                    .Select(d => new { d.id, nombre = d.descripcion }).ToList();
            }

            ViewBag.perfilContable = new SelectList(perfiles, "id", "nombre", orden.perfilContable);

            var centroCosotos = (from cc in context.centro_costo
                                 where cc.centcst_estado
                                 orderby cc.pre_centcst
                                 select new
                                 {
                                     value = cc.centcst_id,
                                     text = "(" + cc.pre_centcst + ") " + cc.centcst_nombre
                                 }).ToList();

            ViewBag.centroCostoModal = new SelectList(centroCosotos, "value", "text");
            //listado de operaciones
            ViewBag.operacionesTempario = new SelectList(context.ttempario, "codigo", "operacion");

            // formas de pago
            var tipoPagoCliente = (from c in context.tercero_cliente
                                join f in context.fpago_tercero
                                    on c.cod_pago_id equals f.fpago_id
                                where c.icb_terceros.tercero_id == orden.id_cliente
                                select f.fpago_id).FirstOrDefault();

            var formasPago = context.fpago_tercero.Select(d=>new {id=d.fpago_id,nombre=d.fpago_nombre }).ToList();
            var deshabilitados = context.fpago_tercero.Where(d => d.fpago_id != 1 && d.fpago_id != tipoPagoCliente).Select(d => d.fpago_id).ToList();
            ViewBag.forma_pago_id = new SelectList(formasPago, "id", "nombre", orden.forma_pago_id,deshabilitados);
        }

        //funcion para actualizar el precio y descuentos de operaciones
        public JsonResult actualizaroperacion(int? idorden, int? idoperacion, int? tipotarifa, int? centro_costo)
        {
            int resultado = 0;
            string error = "";
            if (idorden != null && idoperacion != null && tipotarifa != null)
            {
                //busco la orden
                tencabezaorden ordentaller = context.tencabezaorden.Where(d => d.id == idorden).FirstOrDefault();
                if (ordentaller != null)
                {
                    //busco la operación respectiva para la orden
                    tdetallemanoobraot operacion = context.tdetallemanoobraot.Where(d => d.idorden == idorden && d.id == idoperacion).FirstOrDefault();
                    var bodega = ordentaller.bodega;
                    //tarifas cliente Mano de obra
                    var mano_obra = context.ttarifastaller.Where(d => d.bodega == bodega && d.tipotarifa== tipotarifa).FirstOrDefault();

                    if (operacion != null)
                    {
                        //busco la informacion de la mano de obra en ttempario;
                        ttempario ope = context.ttempario.Where(d => d.codigo == operacion.idtempario).FirstOrDefault();
                        rparametrotipocliente parametrox = context.rparametrotipocliente.Where(d => d.idtipocliente == tipotarifa)
                            .FirstOrDefault();
                        //busco el tipo de tarifa
                        decimal precio = 0;
                        //busco el precio
                        switch (parametrox.valor)
                        {
                            case 1:
                                precio = mano_obra.total_tarifa ?? 0;
                                break;
                            case 2:
                                precio = mano_obra.total_tarifa ?? 0;
                                break;
                            default:
                                precio = mano_obra.total_tarifa ?? 0;
                                break;
                        }




                        //busco el tipo de operacion
                        switch (parametrox.operacion)
                        {
                            case 1:
                                precio = precio + precio * Convert.ToDecimal(parametrox.porcentaje, miCultura) / 100;
                                break;
                            case 2:
                                precio = precio - precio * Convert.ToDecimal(parametrox.porcentaje, miCultura) / 100;
                                break;
                        }

                        //busco el iva
                        decimal iva = ope.iva != null
                            ? ope.codigo_iva.porcentaje != null ? ope.codigo_iva.porcentaje.Value : 0
                            : 0;

                        //busco el descuento de la operacion.
                        int cliente = operacion.tencabezaorden.icb_terceros.tercero_id;
                        float descuentocliente = context.tercero_cliente.Where(d => d.tercero_id == cliente)
                            .Select(d => d.dscto_mo).FirstOrDefault();

                        //busco el tipo de tarifa
                        rtipocliente tipota = context.rtipocliente.Where(d => d.id == tipotarifa).FirstOrDefault();

                        decimal ivacliente = iva;
                        decimal descuento = Convert.ToDecimal(descuentocliente, miCultura);

                        if (tipota.aplica_iva == false)
                        {
                            ivacliente = 0;
                        }
                        else
                        {
                            ivacliente = Convert.ToDecimal(iva, miCultura);
                        }

                        if (tipota.aplica_descuento == false)
                        {
                            descuento = 0;
                        }
                        //esdta parte esta interfiriendo en la operacion es tarifa 2 me trae valores en 0
                        if (tipota.cobra_cliente == false)
                        {
                           // precio = 0;
                        }

                        if (tipota.selecciona_centro_costo == false || tipota.selecciona_centro_costo &&
                            centro_costo != null && centro_costo != 0)
                        {
                            //calculo de descuento
                            operacion.pordescuento = descuento;
                            operacion.valorunitario = precio;
                            operacion.poriva = iva;
                            //mano_obra.total_tarifa;
                            operacion.tipotarifa = tipotarifa;
                            if (centro_costo != null && centro_costo != 0)
                            {
                                operacion.idcentro = centro_costo.Value;
                            }

                            context.Entry(operacion).State = EntityState.Modified;
                            context.SaveChanges();
                            resultado = 1;
                            //calculo de iva

                            resultado = 1;
                            error = "";
                            tdetallemanoobraot e = context.tdetallemanoobraot.Where(d => d.id == operacion.id).FirstOrDefault();

                            var operaciones = new
                            {
                                e.id,
                                e.ttempario.codigo,
                                nombre = e.ttempario.operacion,
                                e.idtecnico,
                                tecnico = e.ttecnicos.users.user_nombre + " " + e.ttecnicos.users.user_apellido,
                                centro_costo = e.idcentro != null ? e.idcentro.Value : 0,
                                tiempo = e.tiempo ?? 0,
                                precio_unitario = Convert.ToDecimal(mano_obra.total_tarifa),
                                precio_string = Convert.ToDecimal(mano_obra.total_tarifa).ToString("N0", new CultureInfo("is-IS")),
                                /*porcentaje_iva = Convert.ToDecimal(mano_obra.iva) != null ? Convert.ToDecimal(mano_obra.iva) : 0,
                                porcentaje_iva_string = Convert.ToDecimal(mano_obra.iva) != null
                                    ? Convert.ToDecimal(mano_obra.iva).ToString("N2", new CultureInfo("is-IS"))
                                    : "0",*/
                                porcentaje_iva = Convert.ToDecimal(mano_obra.iva),
                                porcentaje_iva_string = Convert.ToDecimal(mano_obra.iva).ToString("N2", new CultureInfo("is-IS")),
                                porcentaje_descuento = e.pordescuento != null ? e.pordescuento : 0,
                                porcentaje_descuento_string = e.pordescuento != null
                                    ? e.pordescuento.Value.ToString("N2", new CultureInfo("is-IS"))
                                    : "0",
                                totaldescuento = calculardescuento(mano_obra.total_tarifa, e.pordescuento),
                                totaldescuento_string = calculardescuento(mano_obra.total_tarifa, e.pordescuento)
                                    .ToString("N2", new CultureInfo("is-IS")),
                                totaliva_string = calcular_iva(mano_obra.total_tarifa, e.pordescuento)
                                    .ToString("N2", new CultureInfo("is-IS")),
                                totaliva = calcular_iva(mano_obra.total_tarifa, e.pordescuento),
                                tipotarifa = e.tipotarifa ?? 0,
                                total_string =
                                    ( Convert.ToDecimal(mano_obra.total_tarifa) - calculardescuento(mano_obra.total_tarifa, e.pordescuento) +
                                     calcular_iva(mano_obra.total_tarifa, e.pordescuento))
                                    .ToString("N2", new CultureInfo("is-IS")),
                                total = Convert.ToDecimal(mano_obra.total_tarifa) - calculardescuento(mano_obra.total_tarifa, e.pordescuento) +
                                        calcular_iva(mano_obra.total_tarifa, e.pordescuento)
                            };

                            var centros = context.centro_costo.OrderBy(d => d.bodega).ThenBy(d => d.centcst_nombre)
                            .Where(d => d.centcst_estado).Select(d => new
                                {
                                id = d.centcst_id,
                                nombre = d.pre_centcst + " - " + d.centcst_nombre + " - " + (d.bodega != null
                                             ? d.bodega_concesionario.bodccs_nombre
                                             : "SIN BODEGA")
                                }).ToList();


                            var data = new
                            {
                                resultado,
                                operaciones,
                                error,
                                centros
                                };
                            return Json(data);
                        }
                        else
                        {
                            resultado = 2;
                            //selecciono los centros de costo
                            var centros = context.centro_costo.OrderBy(d => d.bodega).ThenBy(d => d.centcst_nombre)
                                .Where(d => d.centcst_estado).Select(d => new
                                {
                                    id = d.centcst_id,
                                    nombre = d.pre_centcst + " - " + d.centcst_nombre + " - " + (d.bodega != null
                                                 ? d.bodega_concesionario.bodccs_nombre
                                                 : "SIN BODEGA")
                                }).ToList();

                            error = "";
                            var data = new
                            {
                                resultado,
                                error,
                                centros
                            };
                            return Json(data);
                        }
                    }
                    else
                    {
                        resultado = 0;

                        resultado = 0;
                        error = "La operación a modificar no existe para esta orden de taller";
                        var data = new
                        {
                            resultado,
                            error
                        };
                        return Json(data);
                    }
                }
                else
                {
                    resultado = 0;
                    error = "La orden de taller introducida no es válida";
                    var data = new
                    {
                        resultado,
                        error
                    };
                    return Json(data);
                }
            }

            {
                resultado = 0;
                error = "Debe registrar todos los campos obligatorios para poder actualizar el precio de la operación";
                var data = new
                {
                    resultado,
                    error
                };
                return Json(data);
            }
        }

        //json para actualizar el perfil contable de acuerdo al tipo de documento seleccionado
        public JsonResult traerPerfiles(int? tipoDocumento, int? bodega)
        {
            if (tipoDocumento != null)
            {
                Expression<Func<perfil_contable_documento, bool>> predicado = PredicateBuilder.True<perfil_contable_documento>();
                predicado = predicado.And(d => d.tipo == tipoDocumento);
                if (bodega != null)
                {
                    predicado = predicado.And(
                        d => d.perfil_contable_bodega.Where(e => e.idbodega == bodega).Count() > 0);
                }

                var perfiles = context.perfil_contable_documento.Where(predicado)
                    .Select(d => new { d.id, nombre = d.descripcion }).ToList();
                return Json(perfiles);
            }

            return Json(0);
        }

        ////funcion para actualizar el precio y descuentos de suministros
        public JsonResult actualizarepuesto(int? idorden, int? idrepuesto, int? cantidad, int? tipotarifa,
            int? centro_costo)
        {
            int resultado = 0;
            string error = "";
            if (idorden != null && idrepuesto != null && cantidad != null && tipotarifa != null)
            {
                tdetallerepuestosot repu = context.tdetallerepuestosot.Where(d => d.idorden == idorden && d.id == idrepuesto)
                    .FirstOrDefault();
                if (repu != null)
                {
                    //busco la referencia
                    icb_referencia refer = context.icb_referencia.Where(d => d.ref_codigo == repu.idrepuesto).FirstOrDefault();
                    //busco los parametros de ese precio
                    rparametrotipocliente parametrox = context.rparametrotipocliente.Where(d => d.idtipocliente == tipotarifa)
                        .FirstOrDefault();
                    //busco el tipo de tarifa
                    decimal precio = 0;
                    //busco el precio
                    switch (parametrox.valor)
                    {
                        case 1:
                            precio = refer.costo_unitario;
                            break;
                        case 2:
                            precio = refer.precio_venta;
                            break;
                        default:
                            precio = refer.precio_venta;
                            break;
                    }

                    //busco el tipo de operacion
                    switch (parametrox.operacion)
                    {
                        case 1:
                            precio = precio + precio * Convert.ToDecimal(parametrox.porcentaje, miCultura) / 100;
                            break;
                        case 2:
                            precio = precio - precio * Convert.ToDecimal(parametrox.porcentaje, miCultura) / 100;
                            break;
                    }

                    //busco el iva
                    float iva = refer.por_iva;
                    //busco el descuento de la referencia.
                    float desc = refer.por_dscto;

                    //busco el cliente
                    int cliente = repu.tencabezaorden.icb_terceros.tercero_id;
                    float descuentocliente = context.tercero_cliente.Where(d => d.tercero_id == cliente)
                        .Select(d => d.dscto_rep).FirstOrDefault();

                    //busco el tipo de tarifa
                    rtipocliente tipota = context.rtipocliente.Where(d => d.id == tipotarifa).FirstOrDefault();

                    decimal ivacliente = 0;
                    decimal descuento = 0;

                    if (descuentocliente > desc)
                    {
                        descuento = Convert.ToDecimal(descuentocliente, miCultura);
                    }
                    else
                    {
                        descuento = Convert.ToDecimal(desc, miCultura);
                    }

                    if (tipota.aplica_iva == false)
                    {
                        ivacliente = 0;
                    }
                    else
                    {
                        ivacliente = Convert.ToDecimal(iva, miCultura);
                    }

                    if (tipota.aplica_descuento == false)
                    {
                        descuento = 0;
                    }
                    //se comenta esta parte del codigo porque esta interfiriendo con el tipo de tarifa cuando el valor es 2
                    //estra colocando los valores en 0;
                    //if (tipota.cobra_cliente == false)
                    //{
                    //    precio = 0;
                    //}

                    if (tipota.selecciona_centro_costo == false ||
                        tipota.selecciona_centro_costo && centro_costo != null && centro_costo != 0)
                    {
                        //actualizo el repuesto 
                        repu.tipotarifa = tipotarifa;
                        repu.cantidad = cantidad ?? 0;
                        if (precio==0)
                            {
                            var costo_prome = context.referencias_inven.Where(x => x.codigo == repu.idrepuesto && x.ano == DateTime.Now.Year && x.mes == DateTime.Now.Month).OrderByDescending(d=>d.ano).ThenByDescending(d=>d.mes).Select(x => x.costo_prom).FirstOrDefault();
                            precio = costo_prome;
                            }
                        repu.valorunitario = precio;
                        repu.poriva = ivacliente;
                        repu.pordescto = descuento;
                        if (centro_costo != null && centro_costo != 0)
                        {
                            repu.idcentro = centro_costo.Value;
                        }

                        context.Entry(repu).State = EntityState.Modified;

                        tsolicitudrepuestosot e = context.tsolicitudrepuestosot.Where(d => d.iddetalle == repu.id).FirstOrDefault();
                        e.canttraslado = cantidad.Value;
                        context.Entry(e).State = EntityState.Modified;
                        context.SaveChanges();

                        var repuestos = new
                        {
                            e.tdetallerepuestosot.id,
                            codigo = !string.IsNullOrWhiteSpace(e.reemplazo) ? e.reemplazo : e.idrepuesto,
                            nombre = !string.IsNullOrWhiteSpace(e.reemplazo)
                                ? e.icb_referencia1.ref_descripcion
                                : e.icb_referencia.ref_descripcion,
                            cantidad = e.canttraslado,
                            centro_costo = e.tdetallerepuestosot.idcentro ?? 0,
                            precio_unitario = e.tdetallerepuestosot.valorunitario,
                            precio_string =
                                e.tdetallerepuestosot.valorunitario.ToString("N0", new CultureInfo("is-IS")),
                            porcentaje_iva = e.tdetallerepuestosot.poriva,
                            porcentaje_iva_string =
                                e.tdetallerepuestosot.poriva.ToString("N2", new CultureInfo("is-IS")),
                            porcentaje_descuento = e.tdetallerepuestosot.pordescto,
                            porcentaje_descuento_string =
                                e.tdetallerepuestosot.pordescto.ToString("N2", new CultureInfo("is-IS")),
                            totaldescuento = calculardescuentore(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                                e.tdetallerepuestosot.pordescto),
                            totaldescuento_string =
                                calculardescuentore(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                                    e.tdetallerepuestosot.pordescto).ToString("N2", new CultureInfo("is-IS")),
                            totaliva_string =
                                calcularivare(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                                        e.tdetallerepuestosot.pordescto, e.tdetallerepuestosot.poriva)
                                    .ToString("N2", new CultureInfo("is-IS")),
                            totaliva = calcularivare(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                                e.tdetallerepuestosot.pordescto, e.tdetallerepuestosot.poriva),
                            total_string = (e.tdetallerepuestosot.valorunitario * e.canttraslado -
                                            calculardescuentore(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                                                e.tdetallerepuestosot.pordescto) +
                                            calcularivare(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                                                e.tdetallerepuestosot.pordescto, e.tdetallerepuestosot.poriva))
                                .ToString("N2", new CultureInfo("is-IS")),
                            total = Convert.ToDecimal(e.tdetallerepuestosot.valorunitario * e.canttraslado, miCultura) -
                                    Convert.ToDecimal(calculardescuentore(e.tdetallerepuestosot.valorunitario,
                                        e.canttraslado, e.tdetallerepuestosot.pordescto), miCultura) +
                                    Convert.ToDecimal(calcularivare(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                                        e.tdetallerepuestosot.pordescto, e.tdetallerepuestosot.poriva), miCultura),
                            totalsiniva = Convert.ToDecimal(e.tdetallerepuestosot.valorunitario * e.canttraslado, miCultura),
                            tipotarifa = e.tdetallerepuestosot.tipotarifa ?? 0
                        };
                        var centros = context.centro_costo.OrderBy(d => d.bodega).ThenBy(d => d.centcst_nombre)
                         .Where(d => d.centcst_estado).Select(d => new
                             {
                             id = d.centcst_id,
                             nombre = d.pre_centcst + " - " + d.centcst_nombre + " - " +
                                      (d.bodega != null ? d.bodega_concesionario.bodccs_nombre : "SIN BODEGA")
                             }).ToList();
                        resultado = 1;
                        error = "";

                        var data = new
                        {
                            resultado,
                            repuestos,
                            centros,
                            error
                        };
                        return Json(data);
                    }
                    else
                    {
                        resultado = 2;
                        //selecciono los centros de costo
                        var centros = context.centro_costo.OrderBy(d => d.bodega).ThenBy(d => d.centcst_nombre)
                            .Where(d => d.centcst_estado).Select(d => new
                            {
                                id = d.centcst_id,
                                nombre = d.pre_centcst + " - " + d.centcst_nombre + " - " +
                                         (d.bodega != null ? d.bodega_concesionario.bodccs_nombre : "SIN BODEGA")
                            }).ToList();

                        error = "";
                        var data = new
                        {
                            resultado,
                            error,
                            centros
                        };
                        return Json(data);
                    }
                }
                else
                {
                    resultado = 0;
                    error = "El repuesto no existe";
                    var data = new
                    {
                        resultado,
                        error
                    };
                    return Json(data);
                }
            }
            else
            {
                resultado = 0;
                error = "Debe registrar todos los campos obligatorios para poder actualizar el precio del repuesto";
                var data = new
                {
                    resultado,
                    error
                };
                return Json(data);
            }
        }

        public JsonResult traertotalesorden(int? idorden, int? tipoDocumento, int? perfilContable)
        {
            if (idorden != null)
            {
                tencabezaorden ordentaller = context.tencabezaorden.Where(d => d.id == idorden).FirstOrDefault();
                tercero_cliente cliente = context.tercero_cliente.Where(x => x.tercero_id == ordentaller.tercero).FirstOrDefault();

                if (ordentaller != null)
                {
                    //operaciones de la orden
                    List<tdetallemanoobraot> opera = context.tdetallemanoobraot.Where(e => e.idorden == idorden && e.estado == "1").ToList();

                    //razon de ingreso accesorios
                    icb_sysparameter accesoriosparametro =
                        context.icb_sysparameter.Where(d => d.syspar_cod == "P116").FirstOrDefault();
                    int razoningresoacce = accesoriosparametro != null
                        ? Convert.ToInt32(accesoriosparametro.syspar_value)
                        : 5;
                    int mostrarrepuestos = 0;
                    if (ordentaller.razoningreso == razoningresoacce)
                    {
                        //veo si el vehiculo asociado a la OT viene de un pedido
                        if (ordentaller.idpedido == null)
                        {
                            mostrarrepuestos = 1;
                        }
                        else
                        {
                            mostrarrepuestos = 0;
                        }
                    }
                    else
                    {
                        mostrarrepuestos = 1;
                    }
                    //tarifas cliente Mano de obra
                    //Alexis: Se coloca el uno porque ese es el id de la tarifa
                    //var mano_obra = context.ttarifastaller.Where(d => d.bodega == ordentaller.bodega && d.tipotarifa == 1).FirstOrDefault();
                    var mano_obra = context.ttarifastaller.Where(d => d.bodega == ordentaller.bodega).ToList();

                    decimal totaldescuentosoperaciones = 0;
                    decimal totalivaoperaciones = 0;
                    decimal totaloperaciones = 0;
                    decimal totaloperacionesiniva = 0;

                    decimal totaldescuentorepuestos = 0;
                    decimal totalivarepuestos = 0;
                    decimal totalrepuestos = 0;
                    decimal totalrepuestosiniva = 0;
                    decimal totalDescuentos = 0;
                    decimal totaltotal = 0;
                    decimal descliente = 0, desclientemo = 0;

                    var operaciones2 = opera.Select(e => new
                    {
                        precio_unitario = e.valorunitario,
                        e.pordescuento,
                        e.poriva,
                        e.tipotarifa,
                        totaldescuento = calculardescuento(e.valorunitario, e.pordescuento),
                        totaliva = calculariva(e.valorunitario, e.pordescuento, e.poriva),
                        total = e.valorunitario - calculardescuento(e.valorunitario, e.pordescuento) +
                                calculariva(e.valorunitario, e.pordescuento, e.poriva),
                    }).ToList();

                    List<listaoperaciones> operaciones = new List<listaoperaciones>();
                    foreach (var item in opera)
                    {
                        var valorunitarioitem = precioTarifa(mano_obra, item.tipotarifa);
                        var valorivaitem = precioIva(mano_obra, item.tipotarifa);
                        operaciones.Add(new listaoperaciones
                        {
                            id = item.id,
                            codigo = item.ttempario.codigo,
                            nombre = item.ttempario.operacion,
                            idtecnico = item.idtecnico,
                            tecnico = item.ttecnicos.users.user_nombre + " " + item.ttecnicos.users.user_apellido,
                            centro_costo = item.idcentro != null ? item.idcentro.Value : 0,
                            respuestainterna = Convert.ToBoolean(item.respuestainterno),
                            tiempo = item.tiempo ?? 0,
                            precio_unitario = valorunitarioitem,
                            valorBaseiva = Math.Round(calcular_bruto(Convert.ToDecimal(valorunitarioitem, miCultura), item.pordescuento) - calculardescuento(Convert.ToDecimal(valorunitarioitem, miCultura), item.pordescuento)).ToString(),
                            precio_string = Convert.ToDecimal(valorunitarioitem, miCultura).ToString("N0", new CultureInfo("is-IS")),
                            porcentaje_iva = valorivaitem,
                            porcentaje_iva_string = valorivaitem.ToString("N2", new CultureInfo("is-IS")),
                            porcentaje_descuento = item.pordescuento != null ? item.pordescuento.Value : 0,
                            porcentaje_descuento_string = item.pordescuento != null
                            ? item.pordescuento.Value.ToString("N2", new CultureInfo("is-IS"))
                            : "0",
                            totaldescuento = calculardescuento(valorunitarioitem, item.pordescuento),
                            totaldescuento_string = calculardescuento(valorunitarioitem, item.pordescuento)
                            .ToString("N2", new CultureInfo("is-IS")),
                            totaliva_string = calcular_iva(valorunitarioitem, valorivaitem)
                            .ToString("N2", new CultureInfo("is-IS")),
                            totaliva = calcular_iva(valorunitarioitem, valorivaitem),
                            tipotarifa = item.tipotarifa ?? 0,
                            total_string =
                            (valorunitarioitem - calculardescuento(valorunitarioitem, item.pordescuento) +
                             calcular_iva(valorunitarioitem, valorivaitem)).ToString("N2", new CultureInfo("is-IS")),
                            total = valorunitarioitem - calculardescuento(valorunitarioitem, item.pordescuento) +
                                calcular_iva(valorunitarioitem, valorivaitem),
                            //tarifas = new SelectList(ttari, "id", "nombre", item.tipotarifa),
                        });

                    }

                    totaloperaciones = operaciones.Sum(d => d.total);
                    totaloperacionesiniva = operaciones.Sum(d => d.precio_unitario);
                    totalivaoperaciones = operaciones.Sum(d => d.totaliva);
                    totaldescuentosoperaciones = operaciones.Sum(d => d.totaldescuento);
                    //repuestos de la orden
                    List<tsolicitudrepuestosot> repu = context.tsolicitudrepuestosot.Where(e => e.idorden == idorden && e.recibido).ToList();
                    var repuestos = repu.Select(e => new
                    {
                        totaldescuento = calculardescuentore(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                            e.tdetallerepuestosot.pordescto),
                        totaliva = calcularivare(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                            e.tdetallerepuestosot.pordescto, e.tdetallerepuestosot.poriva),
                        total = Convert.ToDecimal(e.tdetallerepuestosot.valorunitario * e.canttraslado, miCultura) -
                                Convert.ToDecimal(calculardescuentore(e.tdetallerepuestosot.valorunitario,
                                    e.canttraslado, e.tdetallerepuestosot.pordescto), miCultura) +
                                Convert.ToDecimal(calcularivare(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                                    e.tdetallerepuestosot.pordescto, e.tdetallerepuestosot.poriva), miCultura),
                        totalsiniva = Convert.ToDecimal(e.tdetallerepuestosot.valorunitario * e.canttraslado, miCultura)
                    }).ToList();

                    totalrepuestos = repuestos.Sum(d => d.total);
                    totalivarepuestos = repuestos.Sum(d => d.totaliva);
                    totaldescuentorepuestos = repuestos.Sum(d => d.totaldescuento);
                    totalrepuestosiniva = repuestos.Sum(d => d.totalsiniva);
                    if (ordentaller.razoningreso == razoningresoacce)
                    {
                        //veo si el vehiculo asociado a la OT viene de un pedido
                        if (ordentaller.idpedido == null)
                        {
                            mostrarrepuestos = 1;
                        }
                        else
                        {
                            mostrarrepuestos = 0;
                        }
                    }
                    else
                    {
                        mostrarrepuestos = 1;
                    }
                   
                    totaltotal = totalrepuestos + totaloperaciones;
                    decimal subtotal = totaloperacionesiniva + totalrepuestosiniva;
                    totalDescuentos = totaldescuentosoperaciones + totaldescuentorepuestos;
                    decimal totaliva = totalivaoperaciones + totalivarepuestos;
                    /*if(tipoDocumento!=null && perfilContable != null)
					{
					    var resultado = calcularetencion(tipoDocumento,ordentaller.id,subtotal,totalDescuentos,totaliva);
					    if(resultado!=0)
					}*/
                    if (mostrarrepuestos == 1)
                    {
                        if (cliente != null)
                        {
                            if (cliente.dscto_rep != 0)
                            {
                                descliente = (Convert.ToDecimal(cliente.dscto_rep, miCultura) / 100) * totalrepuestos;
                                totalrepuestos = totalrepuestos - descliente;
                                totalrepuestos = Math.Round(totalrepuestos);
                                totalDescuentos = totalDescuentos + descliente;


                            }

                        }
                    }

                    if (cliente!= null)
                    {
                        if (cliente.dscto_mo != 0)
                        {
                            desclientemo = (Convert.ToDecimal(cliente.dscto_mo, miCultura) / 100) * totaloperaciones;
                            totaloperaciones = totaloperaciones - desclientemo;
                            totaloperaciones = Math.Round(totaloperaciones);
                            totalDescuentos = totalDescuentos + desclientemo;


                        }

                    }
                    var data = new
                    {
                        totaldescuentosoperaciones =
                            totaldescuentosoperaciones.ToString("N2", new CultureInfo("is-IS")),
                        totalivaoperaciones = totalivaoperaciones.ToString("N2", new CultureInfo("is-IS")),
                        totaloperaciones = totaloperaciones.ToString("N2", new CultureInfo("is-IS")),
                        totalrepuestos = totalrepuestos.ToString("N2", new CultureInfo("is-IS")),
                        totalivarepuestos = totalivarepuestos.ToString("N2", new CultureInfo("is-IS")),
                        totaldescuentorepuestos = totaldescuentorepuestos.ToString("N2", new CultureInfo("is-IS")),
                        totaltotal = totaltotal.ToString("N2", new CultureInfo("is-IS")),
                        subtotal = subtotal.ToString("N2", new CultureInfo("is-IS")),
                        totalDescuentos = totalDescuentos.ToString("N2", new CultureInfo("is-IS")),
                        totaliva = totaliva.ToString("N2", new CultureInfo("is-IS"))
                    };
                    return Json(data);
                }

                return Json(0);
            }

            return Json(0);
        }

        public decimal precioTarifa(List<ttarifastaller> tarifas,int? tipotarifa)
        {
            decimal? resultado = 0;
            if (tarifas.Count>0)
            {
                if (tipotarifa != null)
                {
                    resultado = tarifas.Where(d => d.tipotarifa == tipotarifa).Select(d => d.total_tarifa).FirstOrDefault();
                }
                else
                {
                    resultado = tarifas.Where(d => d.tipotarifa == 1).Select(d => d.total_tarifa).FirstOrDefault();

                }
            }
            decimal resultado2 = resultado != null ? resultado.Value : 0;
            return resultado2;
        }


        public decimal precioIva(List<ttarifastaller> tarifas, int? tipotarifa)
        {
            string resultado = "0";
            if (tarifas.Count > 0)
            {
                if (tipotarifa != null)
                {
                    resultado = tarifas.Where(d => d.tipotarifa == tipotarifa).Select(d => d.iva).FirstOrDefault();
                }
                else
                {
                    resultado = tarifas.Where(d => d.tipotarifa == 1).Select(d => d.iva).FirstOrDefault();
                }
            }
            decimal resultado2 = !string.IsNullOrWhiteSpace(resultado) ? Convert.ToDecimal(resultado, miCultura) : 0;
            return resultado2;
        }
        public decimal traerTotalesporIdOrden(int? idorden)
        {
            if (idorden != null)
            {
                //valido si la orden existe
                tencabezaorden ordentaller = context.tencabezaorden.Where(d => d.id == idorden).FirstOrDefault();
                tercero_cliente cliente = context.tercero_cliente.Where(x => x.tercero_id == ordentaller.tercero).FirstOrDefault();

                var bodega = ordentaller.bodega;

                if (ordentaller != null)
                {
                    //razon de ingreso accesorios
                    icb_sysparameter accesoriosparametro =
                        context.icb_sysparameter.Where(d => d.syspar_cod == "P116").FirstOrDefault();
                    int razoningresoacce = accesoriosparametro != null
                        ? Convert.ToInt32(accesoriosparametro.syspar_value)
                        : 5;
                    int mostrarrepuestos = 0;
                    if (ordentaller.razoningreso == razoningresoacce)
                    {
                        //veo si el vehiculo asociado a la OT viene de un pedido
                        if (ordentaller.idpedido == null)
                        {
                            mostrarrepuestos = 0;
                        }
                        else
                        {
                            mostrarrepuestos = 1;
                        }
                    }
                    else
                    {
                        mostrarrepuestos = 1;
                    }

                    //listado de tipos de tarifa
                    var ttari = context.rtipocliente.Where(d => d.estado)
                        .Select(d => new { d.id, nombre = d.descripcion }).ToList();
                    //solicitudes de la orden
                    List<tsolicitudorden> solici = context.tsolicitudorden.Where(d => d.idorden == idorden).ToList();
                    //List<tsolicitudorden> solici = context.tsolicitudorden.Where(d => d.idorden == idorden).ToList();
                    var solicitudes = solici.Select(e => new
                    {
                        id = e.idsolicitud,
                        solicitudes = e.solicitud,
                        fechasolicitud = e.fecsolicitud.ToString("yyyy/MM/dd", new CultureInfo("en-Us")),
                        respuesta = !string.IsNullOrWhiteSpace(e.respuesta) ? e.respuesta : "",
                        fecharespuesta = e.fecrespuesta != null
                            ? e.fecrespuesta.Value.ToString("yyyy/MM/dd", new CultureInfo("en-Us"))
                            : ""
                    }).ToList();

                    //operaciones de la orden
                    List<tdetallemanoobraot> opera = context.tdetallemanoobraot.Where(e => e.idorden == idorden && e.estado == "1").ToList();

                    //tarifas cliente Mano de obra
                    var mano_obra = context.ttarifastaller.Where(d => d.bodega == bodega).ToList();

                    var opera2 = context.tdetallemanoobraot.Where(e => e.idorden == idorden && e.estado == "1").FirstOrDefault();

                    decimal totaldescuentosoperaciones = 0;
                    decimal totalivaoperaciones = 0;
                    decimal totaloperaciones = 0;
                    decimal total_tot = 0;
                    decimal totaloperacionesiniva = 0;
                    decimal totaloperacionesbaseiva = 0;

                    decimal totaldescuentorepuestos = 0;
                    decimal totalivarepuestos = 0;
                    decimal totalrepuestos = 0;
                    decimal totalrepuestosiniva = 0;
                    decimal totalrepuestosbaseiva = 0;
                    //decimal totalflete = 0;

                    decimal valorBaseiva = 0;
                    decimal totaltotal = 0;
                    decimal subtotal = 0;
                    decimal totalDescuentos = 0;
                    decimal totaliva = 0;
                    //operaciones

                    List<listaoperaciones> operaciones = new List<listaoperaciones>();
                    foreach (var item in opera)
                    {
                        var montounitario = precioTarifa(mano_obra,item.tipotarifa);
                        var preciounitario = montounitario * (item.tiempo??0);
                        var porcentajeiva = precioIva(mano_obra, item.tipotarifa);
                        var montodescuento = calculardescuento(preciounitario, item.pordescuento);
                        var valorbruto = preciounitario-montodescuento;
                        var valoriva = calcular_iva(valorbruto, porcentajeiva);

                        operaciones.Add(new listaoperaciones {
                            id = item.id,
                            codigo = item.ttempario.codigo,
                            nombre = item.ttempario.operacion,
                            idtecnico = item.idtecnico,
                            tecnico = item.ttecnicos.users.user_nombre + " " + item.ttecnicos.users.user_apellido,
                            centro_costo = item.idcentro != null ? item.idcentro.Value : 0,
                            respuestainterna = Convert.ToBoolean(item.respuestainterno),
                            tiempo = item.tiempo ?? 0,
                            //precio_unitario = mano_obra.total_tarifa,
                            precio_unitario = montounitario,
                            valorBaseiva = valorbruto.ToString("N2", new CultureInfo("is-IS")),
                            valorBaseiva2=valorbruto,
                        precio_string =preciounitario.ToString("N2", new CultureInfo("is-IS")),
                        porcentaje_iva = porcentajeiva,
                        porcentaje_iva_string =porcentajeiva.ToString("N2", new CultureInfo("is-IS")),
                        porcentaje_descuento = item.pordescuento != null ? item.pordescuento.Value : 0,
                        porcentaje_descuento_string = item.pordescuento != null
                            ? item.pordescuento.Value.ToString("N2", new CultureInfo("is-IS"))
                            : "0",
                            totaldescuento = montodescuento,
                            totaldescuento_string = montodescuento.ToString("N2", new CultureInfo("is-IS")),
                            totaliva_string = totaliva.ToString("N2", new CultureInfo("is-IS")),
                            totaliva =totaliva,
                            tipotarifa = item.tipotarifa ?? 0,
                            total_string =
                            (valorbruto+valoriva).ToString("N2", new CultureInfo("is-IS")),
                            total = valorbruto+valoriva,
                            tarifas = new SelectList(ttari, "id", "nombre", item.tipotarifa),
                        });

                    }
                    //si la orden viene de pedido debe listar las operaciones de la orden de taller que no tenga pedido(si la hay) y repetirla.
                    if (ordentaller.idpedido != null)
                    {
                        //busco la orden de taller con el plan mayor que no tenga pedido asociado,, y si la tiene es la orden de accesorios de exhibición
                        tencabezaorden ordenexhibicion = context.tencabezaorden
                            .Where(d => d.placa == ordentaller.placa && d.razoningreso == razoningresoacce)
                            .FirstOrDefault();
                        if (ordenexhibicion != null)
                        {
                            //veo si tiene operaciones cargadas
                            List<tdetallemanoobraot> listaordenexchibicion = context.tdetallemanoobraot
                                .Where(d => d.idorden == ordenexhibicion.id).ToList();
                            foreach (tdetallemanoobraot item in listaordenexchibicion)
                            {
                                var montounitario = precioTarifa(mano_obra, item.tipotarifa);
                                var preciounitario = montounitario * (item.tiempo ?? 0);
                                var porcentajeiva = precioIva(mano_obra, item.tipotarifa);
                                var montodescuento = calculardescuento(item.valorunitario, item.pordescuento);
                                var valorbruto = preciounitario - montodescuento;
                                var valoriva = calcular_iva(valorbruto, porcentajeiva);

                                operaciones.Add(new listaoperaciones
                                {
                                    id = item.id,
                                    codigo = item.ttempario.codigo,
                                    nombre = item.ttempario.operacion,
                                    idtecnico = item.idtecnico,
                                    tecnico = item.ttecnicos.users.user_nombre + " " +
                                              item.ttecnicos.users.user_apellido,
                                    centro_costo = item.idcentro != null ? item.idcentro.Value : 0,
                                    tiempo = item.tiempo ?? 0,
                                    precio_unitario = montounitario,
                                    precio_string = montounitario.ToString("N0", new CultureInfo("is-IS")),
                                    valorBaseiva = valorbruto.ToString("N2", new CultureInfo("is-IS")),
                                    valorBaseiva2 = valorbruto,
                                    porcentaje_iva = porcentajeiva,
                                    porcentaje_iva_string = porcentajeiva.ToString("N2", new CultureInfo("is-IS")),
                                    porcentaje_descuento = item.pordescuento != null ? item.pordescuento.Value : 0,
                                    porcentaje_descuento_string = item.pordescuento != null
                                        ? item.pordescuento.Value.ToString("N2", new CultureInfo("is-IS"))
                                        : "0",
                                    totaldescuento = montodescuento,
                                    totaldescuento_string = montodescuento.ToString("N2", new CultureInfo("is-IS")),
                                    totaliva_string = totaliva.ToString("N2", new CultureInfo("is-IS")),
                                    totaliva = totaliva,
                                    tipotarifa = item.tipotarifa ?? 0,
                                    total_string =
                                        (valorbruto+valoriva).ToString("N2", new CultureInfo("is-IS")),
                                    total = valorbruto+valoriva,
                                    tarifas = new SelectList(ttari, "id", "nombre", item.tipotarifa)
                                });
                            }
                        }
                    }


                    totaloperaciones = operaciones.Sum(d => d.total);
                    totaloperacionesiniva = operaciones.Sum(d => d.precio_unitario);
                    totalivaoperaciones = operaciones.Sum(d => d.totaliva);
                    totaldescuentosoperaciones = operaciones.Sum(d => d.totaldescuento);
                    totaloperacionesbaseiva = operaciones.Sum(d => Convert.ToInt32(d.valorBaseiva));

                    

                    //repuestos de la orden
                    List<tsolicitudrepuestosot> repu = context.tsolicitudrepuestosot.Where(e => e.idorden == idorden && e.recibido).ToList();
            
                  var repuestos = repu.Select(e => new {
                                    e.tdetallerepuestosot.id,
                                    codigo = !string.IsNullOrWhiteSpace(e.reemplazo) ? e.reemplazo : e.idrepuesto,
                                    nombre = !string.IsNullOrWhiteSpace(e.reemplazo)
                                        ? e.icb_referencia1.ref_descripcion
                                        : e.icb_referencia.ref_descripcion,
                                    cantidad = e.canttraslado,
                                    recibidorep = e.recibido,
                                    tarifatipo = e.tdetallerepuestosot.tipotarifa,
                                    centro_costo = e.tdetallerepuestosot.idcentro ?? 0,
                                    precio_unitario = e.tdetallerepuestosot.valorunitario,
                                    valorBaseiva = Math.Round(calcularbruto(e.tdetallerepuestosot.valorunitario, e.tdetallerepuestosot.pordescto, e.tdetallerepuestosot.poriva) - calculardescuentore(e.tdetallerepuestosot.valorunitario, e.canttraslado, e.tdetallerepuestosot.pordescto)).ToString(),
                                    precio_string = e.tdetallerepuestosot.valorunitario.ToString("N0", new CultureInfo("is-IS")),
                                    porcentaje_iva = e.tdetallerepuestosot.poriva,
                                    porcentaje_iva_string = e.tdetallerepuestosot.poriva.ToString("N2", new CultureInfo("is-IS")),
                                    porcentaje_descuento = e.tdetallerepuestosot.pordescto,
                                    porcentaje_descuento_string =
                                        e.tdetallerepuestosot.pordescto.ToString("N2", new CultureInfo("is-IS")),
                                    totaldescuento = calculardescuentore(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                                        e.tdetallerepuestosot.pordescto),
                                    totaldescuento_string =
                                        calculardescuentore(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                                            e.tdetallerepuestosot.pordescto).ToString("N2", new CultureInfo("is-IS")),
                                    totaliva_string =
                                        calcularivare(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                                                e.tdetallerepuestosot.pordescto, e.tdetallerepuestosot.poriva)
                                            .ToString("N2", new CultureInfo("is-IS")),
                                    totaliva = calcularivare(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                                        e.tdetallerepuestosot.pordescto, e.tdetallerepuestosot.poriva),
                                    total_string = (e.tdetallerepuestosot.valorunitario * e.canttraslado -
                                                    calculardescuentore(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                                                        e.tdetallerepuestosot.pordescto) +
                                                    calcularivare(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                                                        e.tdetallerepuestosot.pordescto, e.tdetallerepuestosot.poriva))
                                        .ToString("N2", new CultureInfo("is-IS")),
                                    total = Convert.ToDecimal(e.tdetallerepuestosot.valorunitario * e.canttraslado, miCultura) -
                                            Convert.ToDecimal(calculardescuentore(e.tdetallerepuestosot.valorunitario,
                                                e.canttraslado, e.tdetallerepuestosot.pordescto), miCultura) +
                                            Convert.ToDecimal(calcularivare(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                                                e.tdetallerepuestosot.pordescto, e.tdetallerepuestosot.poriva), miCultura),
                                    totalsiniva = Convert.ToDecimal(e.tdetallerepuestosot.valorunitario * e.canttraslado, miCultura),
                                    tipotarifa = e.tdetallerepuestosot.tipotarifa ?? 0
                                    }).ToList();

              var  tots = context.vw_tot.Where(r => r.numot == idorden).Select(x => new
                    {
                        x.fecha2,
                        x.numerofactura,
                        x.proveedor,
                        x.valor_total,
                        x.valor_total2,
                        x.id2,
                        x.numot,
                        x.codigoentrada,
                        Operaciones = context.titemstot.Where(bx => bx.idordentot == x.id && bx.Tipoitem == 1).Select(e => new { e.Descripcion }).ToList(),
                        Repuestos = context.titemstot.Where(dx => dx.idordentot == x.id && dx.Tipoitem == 2).Select(g => new { g.Descripcion }).ToList(),

                    }).ToList();

                    total_tot = Convert.ToDecimal(tots.Sum(d => d.valor_total), miCultura);
                    totalrepuestos = repuestos.Sum(d => d.total);
                    totalrepuestosiniva = repuestos.Sum(d => d.totalsiniva);
                    totalivarepuestos = repuestos.Sum(d => d.totaliva);
                    totaldescuentorepuestos = repuestos.Sum(d => d.totaldescuento);
                    totalrepuestosbaseiva = repuestos.Sum(d => Convert.ToInt32(d.valorBaseiva));

                    //descuento de repuestos por cliente
                    //descuentos del cliente para repuestos
                    if (mostrarrepuestos == 1)
                    {
                        if (cliente != null)
                        {
                            if (cliente.dscto_rep != 0)
                            {
                                var descliente = (Convert.ToDecimal(cliente.dscto_rep, miCultura) / 100) * totalrepuestos;
                                totalrepuestos = totalrepuestos - descliente;
                                totalrepuestos = Math.Round(totalrepuestos);
                                totalDescuentos = totalDescuentos + descliente;
                            }
                        }
                    }

                    if (mostrarrepuestos == 1)
                    {
                        totaltotal = totalrepuestos + totaloperaciones + total_tot;
                        valorBaseiva = totaloperacionesbaseiva + totalrepuestosbaseiva;
                    }
                    else
                    {
                        totaltotal = totaloperaciones + total_tot;
                        valorBaseiva = totaloperacionesbaseiva;
                    }


                    

                    //si la orden de taller es accesorios, en este segmento se liquidan unicamente las operaciones. Los repuestos se liquidan por otro lado.
                    if (ordentaller.razoningreso == razoningresoacce)
                    {
                        subtotal = totaloperacionesiniva;
                        totalDescuentos = totaldescuentosoperaciones;
                        totaliva = totalivaoperaciones;
                    }
                    else
                    {
                        subtotal = totaloperacionesiniva + totalrepuestosiniva;
                        totalDescuentos = totaldescuentosoperaciones + totaldescuentorepuestos;
                        totaliva = totalivaoperaciones + totalivarepuestos;
                    }


                    // Validacion para reteIVA, reteICA y retencion dependiendo del proveedor seleccionado

                    decimal totaltiempoop = Convert.ToDecimal(operaciones.Select(x => x.tiempo).Sum(), miCultura);

                    icb_sysparameter idsumo = context.icb_sysparameter.Where(d => d.syspar_cod == "P155").FirstOrDefault();
                    int insumo_id = idsumo != null ? Convert.ToInt32(idsumo.syspar_value) : 1;
                    decimal vtotalinsumo = 0;
                    var insumo = context.Tinsumo.Where(x => x.id == insumo_id).Select(a => new { a.horas_insumo, a.porcentaje }).FirstOrDefault();


                    if (insumo.horas_insumo <= totaltiempoop)
                    {

                        var insumootcount = context.tinsumooperaciones.Where(x => x.numot == ordentaller.id).Select(c => c.valort_horas).Sum();
                        if (insumootcount == null)
                        {
                            var listaophorastotales = operaciones.GroupBy(c => new { c.tipotarifa, c.tiempo }).Select(g => new CsTarifasInsumos
                            {
                                tarifa = Convert.ToInt32(g.Key.tipotarifa),
                                cantidad_horas = Convert.ToDecimal(g.Select(x => x.tiempo).Sum(), miCultura)
                            }).ToList();

                            foreach (var item in listaophorastotales)
                            {
                                var vhora = context.ttarifastaller.Where(x => x.bodega == ordentaller.bodega && x.tipotarifa == item.tarifa).Select(x => x.valorhora).FirstOrDefault();
                                decimal vrhora = Convert.ToDecimal(vhora, miCultura);
                                decimal thorasporcentaje = Convert.ToDecimal(insumo.porcentaje, miCultura) * Convert.ToDecimal(item.cantidad_horas, miCultura);
                                decimal valorhoraporcentaje = vrhora * thorasporcentaje;
                                vtotalinsumo = vtotalinsumo + valorhoraporcentaje;

                                tinsumooperaciones insumooperacion = new tinsumooperaciones();
                                insumooperacion.numot = ordentaller.id;
                                insumooperacion.codigo_insumo = insumo_id;
                                insumooperacion.tipotarifa = item.tarifa;
                                insumooperacion.porcentaje_total_ = thorasporcentaje;
                                insumooperacion.valort_horas = valorhoraporcentaje;
                                insumooperacion.valortarifa = vrhora;
                                context.tinsumooperaciones.Add(insumooperacion);
                                context.SaveChanges();

                            }
                        }
                        else
                        {
                            vtotalinsumo = Convert.ToDecimal(context.tinsumooperaciones.Where(x => x.numot == ordentaller.id).Select(x => x.valort_horas).Sum(), miCultura);
                        }


                    }

                    totaltotal = totaltotal + vtotalinsumo;
                    subtotal = subtotal + vtotalinsumo;
                    var listainsumos = context.tinsumooperaciones.Where(x => x.numot == ordentaller.id && x.valort_horas > 0).Select(e => new
                    {
                        e.id,
                        e.codigo_insumo,
                        Descripcion_insumo = e.Tinsumo.descripcion,
                        tarifa = e.ttipostarifa.Descripcion,
                        porcentaje = e.porcentaje_total_,
                        valor_tarifa = e.valortarifa,
                        e.valort_horas
                    }).ToList();


                    int roluser = Convert.ToInt32(Session["user_rolid"]);

                    var permisosinsum = (from acceso in context.rolacceso
                                         join rolPerm in context.rolpermisos
                                         on acceso.idpermiso equals rolPerm.id
                                         where acceso.idrol == roluser
                                         select new { rolPerm.codigo }).ToList();

                    var resultadoinsumo = permisosinsum.Where(x => x.codigo == "P51").Count() > 0 ? "Si" : "No";


                    var totalcombos = context.tot_combo.Where(x => x.numot == ordentaller.id && x.estado == true).Select(v => v.valortotal).Sum();

                    var combosot = context.tot_combo.Where(x => x.numot == ordentaller.id && x.estado == true).Select(c => new
                    {
                        c.id,
                        codigo = c.ttempario.codigo,
                        operacion = c.ttempario.operacion,
                        c.totalHorasCliente,
                        c.totalhorasoperario,
                        ttipostarifa = context.rtipocliente.Where(d => d.estado).Select(x => new { x.id, x.descripcion }).ToList(),
                        c.tipotarifa,
                        c.idtempario,
                        valortotal = c.valortotal != null ? c.valortotal : 0
                    }).ToList();


                    totaltotal = totaltotal + Convert.ToDecimal(totalcombos, miCultura);
                    subtotal = subtotal + Convert.ToDecimal(totalcombos, miCultura);


                    return Math.Round(totaltotal);
                }

                return 0;
            }

            return 0;
        }

        //funcion para traer la información de la orden de trabajo
        public JsonResult traerdatosporIdOrden(int? idorden)
        {
            if (idorden != null)
            {
                //valido si la orden existe
                tencabezaorden ordentaller = context.tencabezaorden.Where(d => d.id == idorden).FirstOrDefault();
                tercero_cliente cliente = context.tercero_cliente.Where(x => x.tercero_id == ordentaller.tercero).FirstOrDefault();
                var bodega = ordentaller.bodega;

                if (ordentaller != null)
                {
                    //razon de ingreso accesorios
                    icb_sysparameter accesoriosparametro =
                        context.icb_sysparameter.Where(d => d.syspar_cod == "P116").FirstOrDefault();
                    int razoningresoacce = accesoriosparametro != null
                        ? Convert.ToInt32(accesoriosparametro.syspar_value)
                        : 5;
                    int mostrarrepuestos = 0;
                    if (ordentaller.razoningreso == razoningresoacce)
                    {
                        //veo si el vehiculo asociado a la OT viene de un pedido
                        if (ordentaller.idpedido == null)
                        {
                            mostrarrepuestos = 1;
                        }
                        else
                        {
                            mostrarrepuestos = 0;
                        }
                    }
                    else
                    {
                        mostrarrepuestos = 1;
                    }

                    //listado de tipos de tarifa
                    var ttari = context.rtipocliente.Where(d => d.estado)
                        .Select(d => new { d.id, nombre = d.descripcion }).ToList();
                    //solicitudes de la orden
                    List<tsolicitudorden> solici = context.tsolicitudorden.Where(d => d.idorden == idorden).ToList();
                    //List<tsolicitudorden> solici = context.tsolicitudorden.Where(d => d.idorden == idorden).ToList();
                    var solicitudes = solici.Select(e => new
                    {
                        id = e.idsolicitud,
                        solicitudes = e.solicitud,
                        fechasolicitud = e.fecsolicitud.ToString("yyyy/MM/dd", new CultureInfo("en-Us")),
                        respuesta = !string.IsNullOrWhiteSpace(e.respuesta) ? e.respuesta : "",
                        fecharespuesta = e.fecrespuesta != null
                            ? e.fecrespuesta.Value.ToString("yyyy/MM/dd", new CultureInfo("en-Us"))
                            : ""
                    }).ToList();

                    //operaciones de la orden
                    List<tdetallemanoobraot> opera = context.tdetallemanoobraot.Where(e => e.idorden == idorden && e.estado == "1").ToList();

                    //tarifas cliente Mano de obra
                    //Alexis: Se coloca el uno porque ese es el id de la tarifa
                    var mano_obra = context.ttarifastaller.Where(d => d.bodega == bodega).ToList();

                    var opera2 = context.tdetallemanoobraot.Where(e => e.idorden == idorden && e.estado == "1").FirstOrDefault();

                    decimal totaldescuentosoperaciones = 0;
                    decimal totalivaoperaciones = 0;
                    decimal totaloperaciones = 0;
                    decimal total_tot = 0;
                    decimal totaloperacionesiniva = 0;
                    decimal totaloperacionesbaseiva = 0;

                    decimal totaldescuentorepuestos = 0;
                    decimal totalivarepuestos = 0;
                    decimal totalrepuestos = 0;
                    decimal totalrepuestosiniva = 0;
                    decimal totalrepuestosbaseiva = 0;
                    decimal totalflete = 0;

                    decimal valorBaseiva = 0;
                    decimal totaltotal = 0;
                    //operaciones
                    List<listaoperaciones> operaciones = new List<listaoperaciones>();
                    //lo hago con un foreach para poder calcular por fuera y 1 sola vez los valores unitarios, y de iva.
                    foreach (var item in opera)
                    {
                        var valorunitarioitem = precioTarifa(mano_obra, item.tipotarifa);
                        var valorivaitem = precioIva(mano_obra, item.tipotarifa);
                        operaciones.Add(new listaoperaciones
                        {
                            id = item.id,
                            codigo = item.ttempario.codigo,
                            nombre = item.ttempario.operacion,
                            idtecnico = item.idtecnico,
                            tecnico = item.ttecnicos.users.user_nombre + " " + item.ttecnicos.users.user_apellido,
                            centro_costo = item.idcentro != null ? item.idcentro.Value : 0,
                            respuestainterna = Convert.ToBoolean(item.respuestainterno),
                            tiempo = item.tiempo ?? 0,
                            precio_unitario = valorunitarioitem,
                            valorBaseiva = Math.Round(calcular_bruto(Convert.ToDecimal(valorunitarioitem, miCultura), item.pordescuento) - calculardescuento(Convert.ToDecimal(valorunitarioitem, miCultura), item.pordescuento)).ToString(),
                            precio_string = Convert.ToDecimal(valorunitarioitem, miCultura).ToString("N0", new CultureInfo("is-IS")),
                            porcentaje_iva = valorivaitem,
                            porcentaje_iva_string = valorivaitem.ToString("N2", new CultureInfo("is-IS")),
                            porcentaje_descuento = item.pordescuento != null ? item.pordescuento.Value : 0,
                            porcentaje_descuento_string = item.pordescuento != null
                            ? item.pordescuento.Value.ToString("N2", new CultureInfo("is-IS"))
                            : "0",
                            totaldescuento = calculardescuento(valorunitarioitem, item.pordescuento),
                            totaldescuento_string = calculardescuento(valorunitarioitem, item.pordescuento)
                            .ToString("N2", new CultureInfo("is-IS")),
                            totaliva_string = calcular_iva(valorunitarioitem,valorivaitem)
                            .ToString("N2", new CultureInfo("is-IS")),
                            totaliva = calcular_iva(valorunitarioitem, valorivaitem),
                            tipotarifa = item.tipotarifa ?? 0,
                            total_string =
                            (valorunitarioitem- calculardescuento(valorunitarioitem, item.pordescuento) +
                             calcular_iva(valorunitarioitem, valorivaitem)).ToString("N2", new CultureInfo("is-IS")),
                            total = valorunitarioitem - calculardescuento(valorunitarioitem, item.pordescuento) +
                                calcular_iva(valorunitarioitem, valorivaitem),
                            tarifas = new SelectList(ttari, "id", "nombre", item.tipotarifa),
                        });
                        
                    }

                    //si la orden viene de pedido debe listar las operaciones de la orden de taller que no tenga pedido(si la hay) y repetirla.
                    if (ordentaller.idpedido != null)
                    {
                        //busco la orden de taller con el plan mayor que no tenga pedido asociado,, y si la tiene es la orden de accesorios de exhibición
                        tencabezaorden ordenexhibicion = context.tencabezaorden
                            .Where(d => d.placa == ordentaller.placa && d.razoningreso == razoningresoacce)
                            .FirstOrDefault();
                        if (ordenexhibicion != null)
                        {
                            //veo si tiene operaciones cargadas
                            List<tdetallemanoobraot> listaordenexchibicion = context.tdetallemanoobraot
                                .Where(d => d.idorden == ordenexhibicion.id).ToList();
                            foreach (tdetallemanoobraot item in listaordenexchibicion)
                            {
                                var valorunitarioitem2 = precioTarifa(mano_obra, item.tipotarifa);
                                var valorivaitem2 = precioIva(mano_obra, item.tipotarifa);
                                operaciones.Add(new listaoperaciones
                                {
                                    id = item.id,
                                    codigo = item.ttempario.codigo,
                                    nombre = item.ttempario.operacion,
                                    idtecnico = item.idtecnico,
                                    tecnico = item.ttecnicos.users.user_nombre + " " +
                                              item.ttecnicos.users.user_apellido,
                                    centro_costo = item.idcentro != null ? item.idcentro.Value : 0,
                                    tiempo = item.tiempo ?? 0,
                                    precio_unitario = item.valorunitario,
                                    precio_string = item.valorunitario.ToString("N0", new CultureInfo("is-IS")),
                                    porcentaje_iva = item.poriva != null ? item.poriva.Value : 0,
                                    porcentaje_iva_string = item.poriva != null
                                        ? item.poriva.Value.ToString("N2", new CultureInfo("is-IS"))
                                        : "0",
                                    porcentaje_descuento = item.pordescuento != null ? item.pordescuento.Value : 0,
                                    porcentaje_descuento_string = item.pordescuento != null
                                        ? item.pordescuento.Value.ToString("N2", new CultureInfo("is-IS"))
                                        : "0",
                                    totaldescuento = calculardescuento(item.valorunitario, item.pordescuento),
                                    totaldescuento_string = calculardescuento(item.valorunitario, item.pordescuento)
                                        .ToString("N2", new CultureInfo("is-IS")),
                                    totaliva_string = calculariva(item.valorunitario, item.pordescuento, item.poriva)
                                        .ToString("N2", new CultureInfo("is-IS")),
                                    totaliva = calculariva(item.valorunitario, item.pordescuento, item.poriva),
                                    tipotarifa = item.tipotarifa ?? 0,
                                    total_string =
                                        (item.valorunitario - calculardescuento(item.valorunitario, item.pordescuento) +
                                         calculariva(item.valorunitario, item.pordescuento, item.poriva))
                                        .ToString("N2", new CultureInfo("is-IS")),
                                    total = item.valorunitario -
                                            calculardescuento(item.valorunitario, item.pordescuento) +
                                            calculariva(item.valorunitario, item.pordescuento, item.poriva),
                                    tarifas = new SelectList(ttari, "id", "nombre", item.tipotarifa)
                                });
                            }
                        }
                    }


                    totaloperaciones = operaciones.Sum(d => d.total);
                    totaloperacionesiniva = operaciones.Sum(d => d.precio_unitario);
                    totalivaoperaciones = operaciones.Sum(d => d.totaliva);
                    totaldescuentosoperaciones = operaciones.Sum(d => d.totaldescuento);
                    totaloperacionesbaseiva= operaciones.Sum(d => Convert.ToInt32(d.valorBaseiva));
                    //repuestos de la orden
                    List<tsolicitudrepuestosot> repu = context.tsolicitudrepuestosot.Where(e => e.idorden == idorden && e.recibido).ToList();

                    var repuestos = repu.Select(e => new
                    {
                        e.tdetallerepuestosot.id,
                        codigo = !string.IsNullOrWhiteSpace(e.reemplazo) ? e.reemplazo : e.idrepuesto,
                        nombre = !string.IsNullOrWhiteSpace(e.reemplazo)
                            ? e.icb_referencia1.ref_descripcion
                            : e.icb_referencia.ref_descripcion,
                        cantidad = e.canttraslado,
                        recibidorep = e.recibido,
                        tarifatipo = e.tdetallerepuestosot.tipotarifa,
                        centro_costo = e.tdetallerepuestosot.idcentro ?? 0,
                        precio_unitario = e.tdetallerepuestosot.valorunitario,
                        valorBaseiva = Math.Round(calcularbruto(e.tdetallerepuestosot.valorunitario, e.tdetallerepuestosot.pordescto, e.tdetallerepuestosot.poriva) - calculardescuentore(e.tdetallerepuestosot.valorunitario, e.canttraslado, e.tdetallerepuestosot.pordescto)).ToString(),
                        precio_string = e.tdetallerepuestosot.valorunitario.ToString("N0", new CultureInfo("is-IS")),
                        porcentaje_iva = e.tdetallerepuestosot.poriva,
                        porcentaje_iva_string = e.tdetallerepuestosot.poriva.ToString("N2", new CultureInfo("is-IS")),
                        porcentaje_descuento = e.tdetallerepuestosot.pordescto,
                        porcentaje_descuento_string =
                            e.tdetallerepuestosot.pordescto.ToString("N2", new CultureInfo("is-IS")),
                        totaldescuento = calculardescuentore(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                            e.tdetallerepuestosot.pordescto),
                        totaldescuento_string =
                            calculardescuentore(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                                e.tdetallerepuestosot.pordescto).ToString("N2", new CultureInfo("is-IS")),
                        totaliva_string =
                            calcularivare(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                                    e.tdetallerepuestosot.pordescto, e.tdetallerepuestosot.poriva)
                                .ToString("N2", new CultureInfo("is-IS")),
                        totaliva = calcularivare(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                            e.tdetallerepuestosot.pordescto, e.tdetallerepuestosot.poriva),
                        total_string = (e.tdetallerepuestosot.valorunitario * e.canttraslado -
                                        calculardescuentore(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                                            e.tdetallerepuestosot.pordescto) +
                                        calcularivare(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                                            e.tdetallerepuestosot.pordescto, e.tdetallerepuestosot.poriva))
                            .ToString("N2", new CultureInfo("is-IS")),
                        total = Convert.ToDecimal(e.tdetallerepuestosot.valorunitario * e.canttraslado, miCultura) -
                                Convert.ToDecimal(calculardescuentore(e.tdetallerepuestosot.valorunitario,
                                    e.canttraslado, e.tdetallerepuestosot.pordescto), miCultura) +
                                Convert.ToDecimal(calcularivare(e.tdetallerepuestosot.valorunitario, e.canttraslado,
                                    e.tdetallerepuestosot.pordescto, e.tdetallerepuestosot.poriva), miCultura),
                        totalsiniva = Convert.ToDecimal(e.tdetallerepuestosot.valorunitario * e.canttraslado, miCultura),
                        tipotarifa = e.tdetallerepuestosot.tipotarifa ?? 0
                    }).ToList();

                    var tots = context.vw_tot.Where(r => r.numot == idorden).Select(x => new
                    {
                        x.fecha2,
                        x.numerofactura,
                        x.proveedor,
                        x.valor_total,
                        x.valor_total2,
                        x.id2,
                        x.numot,
                        x.codigoentrada,
                        Operaciones = context.titemstot.Where(bx => bx.idordentot == x.id && bx.Tipoitem == 1).Select(e => new { e.Descripcion }).ToList(),
                        Repuestos = context.titemstot.Where(dx => dx.idordentot == x.id && dx.Tipoitem == 2).Select(g => new { g.Descripcion }).ToList(),

                    }).ToList();

                    total_tot = Convert.ToDecimal(tots.Sum(d=> d.valor_total), miCultura);
                    totalrepuestos = repuestos.Sum(d => d.total);
                    totalrepuestosiniva = repuestos.Sum(d => d.totalsiniva);
                    totalivarepuestos = repuestos.Sum(d => d.totaliva);
                    totaldescuentorepuestos = repuestos.Sum(d => d.totaldescuento);
                    totalrepuestosbaseiva = repuestos.Sum(d => Convert.ToInt32(d.valorBaseiva));
                    decimal totalDescuentos = 0;
                    decimal descliente = 0, desclientemo = 0;
                    if (mostrarrepuestos == 1)
                        {
                        if (cliente != null)
                            {
                            if (cliente.dscto_rep != 0)
                                {
                                descliente = (Convert.ToDecimal(cliente.dscto_rep, miCultura) / 100) * totalrepuestos;
                                totalrepuestos = totalrepuestos - descliente;
                                totalrepuestos = Math.Round(totalrepuestos);
                                totalDescuentos = totalDescuentos + descliente;


                                }

                            }
                        }

                    if (cliente != null)
                        {
                        if (cliente.dscto_mo != 0)
                            {
                            desclientemo = (Convert.ToDecimal(cliente.dscto_mo, miCultura) / 100) * totaloperaciones;
                            totaloperaciones =  totaloperaciones - desclientemo;
                            totaloperaciones = Math.Round(totaloperaciones);
                            totalDescuentos = totalDescuentos + desclientemo;
       

                            }

                        }



                    if (mostrarrepuestos == 1)
                    {
                        totaltotal = totalrepuestos + totaloperaciones + total_tot;
                        valorBaseiva = totaloperacionesbaseiva + totalrepuestosbaseiva;
                    }
                    else {
                        totaltotal =  totaloperaciones + total_tot;
                        valorBaseiva = totaloperacionesbaseiva ;
                    }
                    
                        
                    decimal subtotal = 0;
              
                    decimal totaliva = 0;
              
                    //si la orden de taller es accesorios, en este segmento se liquidan unicamente las operaciones. Los repuestos se liquidan por otro lado.
                 




                    if (mostrarrepuestos == 1)
                    {
                        subtotal = totaloperacionesiniva + totalrepuestosiniva;
                        totalDescuentos = totaldescuentosoperaciones + totaldescuentorepuestos + totalDescuentos;
                        totaliva = totalivaoperaciones + totalivarepuestos;
                    }
                    else
                    {

                        subtotal = totaloperacionesiniva;
                        totalDescuentos = totaldescuentosoperaciones+ totalDescuentos;
                        totaliva = totalivaoperaciones;

                        }







                    // Validacion para reteIVA, reteICA y retencion dependiendo del proveedor seleccionado

                    decimal totaltiempoop = Convert.ToDecimal(operaciones.Select(x => x.tiempo).Sum(), miCultura);

                    icb_sysparameter idsumo = context.icb_sysparameter.Where(d => d.syspar_cod == "P155").FirstOrDefault();
                    int insumo_id = idsumo != null ? Convert.ToInt32(idsumo.syspar_value) : 1;
                    decimal vtotalinsumo = 0;
                    decimal totalcombos = 0;
                    var insumo = context.Tinsumo.Where(x => x.id == insumo_id).Select(a => new { a.horas_insumo, a.porcentaje }).FirstOrDefault();


                    if (insumo.horas_insumo <= totaltiempoop)
                        {

                        var insumootcount = context.tinsumooperaciones.Where(x => x.numot == ordentaller.id).Select(c => c.valort_horas).Sum();
                        if (insumootcount == null)
                            {
                            var listaophorastotales = operaciones.GroupBy(c => new { c.tipotarifa, c.tiempo }).Select(g => new CsTarifasInsumos
                                {
                                tarifa = Convert.ToInt32(g.Key.tipotarifa),
                                cantidad_horas = Convert.ToDecimal(g.Select(x => x.tiempo).Sum(), miCultura)
                                }).ToList();

                            foreach (var item in listaophorastotales)
                                {
                                var vhora = context.ttarifastaller.Where(x => x.bodega == ordentaller.bodega && x.tipotarifa == item.tarifa).Select(x => x.valorhora).FirstOrDefault();
                                decimal vrhora = Convert.ToDecimal(vhora, miCultura);
                                decimal thorasporcentaje = Convert.ToDecimal(insumo.porcentaje, miCultura) * Convert.ToDecimal(item.cantidad_horas, miCultura);
                                decimal valorhoraporcentaje = vrhora * thorasporcentaje;
                                vtotalinsumo = vtotalinsumo + valorhoraporcentaje;

                                tinsumooperaciones insumooperacion = new tinsumooperaciones();
                                insumooperacion.numot = ordentaller.id;
                                insumooperacion.codigo_insumo = insumo_id;
                                insumooperacion.tipotarifa = item.tarifa;
                                insumooperacion.porcentaje_total_ = thorasporcentaje;
                                insumooperacion.valort_horas = valorhoraporcentaje;
                                insumooperacion.valortarifa = vrhora;
                                context.tinsumooperaciones.Add(insumooperacion);
                                context.SaveChanges();

                                }
                            }
                        else
                            {
                            vtotalinsumo = Convert.ToDecimal(context.tinsumooperaciones.Where(x => x.numot == ordentaller.id).Select(x => x.valort_horas).Sum(), miCultura);
                            }


                        }
                    else
                        {
                        vtotalinsumo = Convert.ToDecimal(context.tinsumooperaciones.Where(x => x.numot == ordentaller.id).Select(x => x.valort_horas).Sum(), miCultura);
                        }



                    totaltotal = totaltotal + vtotalinsumo;
                    subtotal = subtotal + vtotalinsumo;
                    var listainsumos = context.tinsumooperaciones.Where(x => x.numot == ordentaller.id && x.valort_horas > 0).Select(e => new
                        {
                        e.id,
                        e.codigo_insumo,
                        Descripcion_insumo = e.Tinsumo.descripcion,
                        tarifa = e.ttipostarifa.Descripcion,
                        porcentaje = e.porcentaje_total_,
                        valor_tarifa = e.valortarifa,
                        e.valort_horas
                        }).ToList();


                    int roluser = Convert.ToInt32(Session["user_rolid"]);

                    var permisosinsum = (from acceso in context.rolacceso
                                         join rolPerm in context.rolpermisos
                                         on acceso.idpermiso equals rolPerm.id
                                         where acceso.idrol == roluser
                                         select new { rolPerm.codigo }).ToList();

                    var resultadoinsumo = permisosinsum.Where(x => x.codigo == "P51").Count() > 0 ? "Si" : "No";


                     totalcombos = Convert.ToDecimal(context.tot_combo.Where(x => x.numot == ordentaller.id && x.estado == true).Select(v => v.valortotal).Sum(), miCultura);

                    var combosot = context.tot_combo.Where(x => x.numot == ordentaller.id && x.estado == true).Select(c => new
                        {
                        c.id,
                        codigo = c.ttempario.codigo,
                        operacion = c.ttempario.operacion,
                        c.totalHorasCliente,
                        c.totalhorasoperario,
                        ttipostarifa = context.rtipocliente.Where(d => d.estado).Select(x => new { x.id, x.descripcion }).ToList(),
                        c.tipotarifa,
                        c.idtempario,
                        valortotal = c.valortotal != null ? c.valortotal : 0
                        }).ToList();


                    totaltotal = totaltotal + Convert.ToDecimal(totalcombos, miCultura);
                    subtotal = subtotal + Convert.ToDecimal(totalcombos, miCultura); 



                    var data = new
                    {
                        //ordentaller,
                        ttari,
                        solicitudes,
                        operaciones,
                        repuestos,
                        vtotalinsumo = vtotalinsumo.ToString("N2", new CultureInfo("is-IS")),
                        resultadoinsumo,
                        totalcombos = totalcombos.ToString("N2", new CultureInfo("is-IS")),
                        combosot,
                        tots,
                        total_tot = total_tot.ToString("N2", new CultureInfo("is-IS")),
                        listainsumos,
                        totaldescuentosoperaciones =
                            totaldescuentosoperaciones.ToString("N2", new CultureInfo("is-IS")),
                        totalivaoperaciones = totalivaoperaciones.ToString("N2", new CultureInfo("is-IS")),
                        totaloperaciones = totaloperaciones.ToString("N2", new CultureInfo("is-IS")),
                        totalrepuestos = totalrepuestos.ToString("N2", new CultureInfo("is-IS")),
                        totalivarepuestos = totalivarepuestos.ToString("N2", new CultureInfo("is-IS")),
                        totaldescuentorepuestos = totaldescuentorepuestos.ToString("N2", new CultureInfo("is-IS")),
                        valorBaseiva= Math.Round(valorBaseiva),
                        totalflete=Math.Round(totalflete),
                        totaltotal = Math.Round(totaltotal),
                        //totaltotal = Math.Round(totaltotal).ToString("N2", new CultureInfo("is-IS")),
                        subtotal = Math.Round(subtotal),
                        //subtotal = Math.Round(subtotal).ToString("N2", new CultureInfo("is-IS")),
                        totalDescuentos = Math.Round(totalDescuentos),
                        //totalDescuentos = Math.Round(totalDescuentos).ToString("N2", new CultureInfo("is-IS")),
                        totaliva = Math.Round(totaliva),
                        //totaliva = Math.Round(totaliva).ToString("N2", new CultureInfo("is-IS")),
                        mostrarrepuestos
                    };
                    return Json(data);
                }

                return Json(0);
            }

            return Json(0);
        }

        public JsonResult BuscarTOTPorOrden(int? idorden) {

            if (idorden != null)
            {
                var data = context.vw_tot.Where(r => r.numot == idorden).Select(x => new
                {
                    x.fecha2,
                    x.numerofactura,
                    x.proveedor,
                    x.valor_total2,
                    x.id2,
                    x.numot,
                    x.codigoentrada,
                    Operaciones = context.titemstot.Where(bx => bx.idordentot == x.id && bx.Tipoitem == 1).Select(e => new { e.Descripcion }).ToList(),
                    Repuestos = context.titemstot.Where(dx => dx.idordentot == x.id && dx.Tipoitem == 2).Select(g => new { g.Descripcion }).ToList(),

                }).ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(0, JsonRequestBehavior.AllowGet);

        }

        public JsonResult calcularetencion(int? tipo_doc, int? idorden, string subTotal, string totalDes,
            string totalIVA)
        {
            if (tipo_doc != null && idorden != null && !string.IsNullOrWhiteSpace(subTotal) &&
                !string.IsNullOrWhiteSpace(totalDes) && !string.IsNullOrWhiteSpace(totalIVA))
            {
                bool convertir = decimal.TryParse(subTotal, out decimal subtotal2);
                bool convertir2 = decimal.TryParse(totalDes, out decimal totaldes2);

                bool convertir3 = decimal.TryParse(totalIVA, out decimal totaliva2);

                tp_doc_registros buscarTipoDocRegistro = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == tipo_doc);
                tencabezaorden orden = context.tencabezaorden.Where(d => d.id == idorden).FirstOrDefault();
                int nit = orden.tercero;
                int bodega = orden.bodega;
                //cuando se cambie el perfil tributario a tercero pues buscar en terceros.
                tercero_cliente buscarCliente = context.tercero_cliente.FirstOrDefault(x => x.tercero_id == nit);
                //var regimen_cliente = buscarCliente != null ? buscarCliente.tpregimen_id : 0;*/
                int regimen_cliente =
                    orden.icb_terceros.tpregimen_id != null ? orden.icb_terceros.tpregimen_id.Value : 0;
                //busco perfil tributario por cliente (datos encontrados en la tabla de terceros)
                icb_terceros buscarClientePerfil = context.icb_terceros.FirstOrDefault(x => x.tercero_id == nit);

                decimal retefuente = 0;
                decimal reteica = 0;
                decimal reteiva = 0;
                decimal autoRetencion = 0;
                if (buscarClientePerfil.retfuente == null)
                {
                    perfiltributario buscarPerfilTributario = context.perfiltributario.FirstOrDefault(x =>
                        x.bodega == bodega && x.sw == buscarTipoDocRegistro.sw && x.tipo_regimenid == regimen_cliente);
                    if (buscarPerfilTributario != null)
                    {
                        if (buscarPerfilTributario.retfuente == "A" &&
                            subtotal2 - totaldes2 >= buscarTipoDocRegistro.baseretencion) //retencion
                        {
                            retefuente = Math.Round((subtotal2 - totaldes2) *
                                                    Convert.ToDecimal(buscarTipoDocRegistro.retencion / 100, miCultura));
                        }

                        if (buscarPerfilTributario.retiva == "A" &&
                            totaliva2 >= buscarTipoDocRegistro.baseiva) //reteiva
                        {
                            reteiva = Math.Round(totaliva2 * Convert.ToDecimal(buscarTipoDocRegistro.retiva / 100, miCultura));
                        }

                        if (buscarPerfilTributario.autorretencion == "A") //autoretencion
                        {
                            decimal tercero_acteco = buscarCliente.acteco_tercero.autorretencion;
                            autoRetencion =
                                Math.Round((subtotal2 - totaldes2) * Convert.ToDecimal(tercero_acteco / 100, miCultura));
                        }

                        if (buscarPerfilTributario.retica == "A" &&
                            subtotal2 - totaldes2 >= buscarTipoDocRegistro.baseica) //reteica
                        {
                            terceros_bod_ica bodega_acteco = context.terceros_bod_ica.FirstOrDefault(x =>
                                x.idcodica == buscarCliente.actividadEconomica_id && x.bodega == bodega);
                            decimal tercero_acteco = buscarCliente.acteco_tercero.tarifa;
                            if (bodega_acteco != null)
                            {
                                reteica = Math.Round((subtotal2 - totaldes2) *
                                                     Convert.ToDecimal(bodega_acteco.porcentaje / 1000, miCultura));
                            }

                            if (tercero_acteco != 0)
                            {
                                reteica = Math.Round((subtotal2 - totaldes2) *
                                                     Convert.ToDecimal(buscarCliente.acteco_tercero.tarifa / 1000, miCultura));
                            }
                            else
                            {
                                reteica = Math.Round((subtotal2 - totaldes2) *
                                                     Convert.ToDecimal(buscarTipoDocRegistro.retica / 1000, miCultura));
                            }
                        }
                    }
                }
                else
                {
                    if (buscarClientePerfil.retfuente != null)
                    {
                        if (buscarClientePerfil.retfuente == "A" &&
                            subtotal2 - totaldes2 >= buscarTipoDocRegistro.baseretencion) //retencion
                        {
                            retefuente = Math.Round((subtotal2 - totaldes2) *
                                                    Convert.ToDecimal(buscarTipoDocRegistro.retencion / 100, miCultura));
                        }

                        if (buscarClientePerfil.retiva == "A" && totaliva2 >= buscarTipoDocRegistro.baseiva) //reteiva
                        {
                            reteiva = Math.Round(totaliva2 * Convert.ToDecimal(buscarTipoDocRegistro.retiva / 100, miCultura));
                        }

                        if (buscarClientePerfil.autorretencion == "A") //autoretencion
                        {
                            decimal tercero_acteco = buscarCliente.acteco_tercero.autorretencion;
                            autoRetencion =
                                Math.Round((subtotal2 - totaldes2) * Convert.ToDecimal(tercero_acteco / 100, miCultura));
                        }

                        if (buscarClientePerfil.retica == "A" &&
                            subtotal2 - totaldes2 >= buscarTipoDocRegistro.baseica) //reteica
                        {
                            terceros_bod_ica bodega_acteco = context.terceros_bod_ica.FirstOrDefault(x =>
                                x.idcodica == buscarCliente.actividadEconomica_id && x.bodega == bodega);
                            decimal tercero_acteco = buscarCliente.acteco_tercero.tarifa;
                            if (bodega_acteco != null)
                            {
                                reteica = Math.Round((subtotal2 - totaldes2) *
                                                     Convert.ToDecimal(bodega_acteco.porcentaje / 1000, miCultura));
                            }

                            if (tercero_acteco != 0)
                            {
                                reteica = Math.Round((subtotal2 - totaldes2) *
                                                     Convert.ToDecimal(buscarCliente.acteco_tercero.tarifa / 1000, miCultura));
                            }
                            else
                            {
                                reteica = Math.Round((subtotal2 - totaldes2) *
                                                     Convert.ToDecimal(buscarTipoDocRegistro.retica / 1000, miCultura));
                            }
                        }
                    }
                }

                decimal totalretenciones = reteica + retefuente + reteiva + autoRetencion;
                decimal valor_proveedor = subtotal2 - totaldes2 + totaliva2 - totalretenciones + autoRetencion;
                var data = new
                {
                    retefuente = Math.Round(retefuente),
                    //retefuente = Math.Round(retefuente).ToString("N2", new CultureInfo("is-IS")),
                    reteica = Math.Round(reteica),
                    //reteica = Math.Round(reteica).ToString("N2", new CultureInfo("is-IS")),
                    reteiva = Math.Round(reteiva),
                    //reteiva = Math.Round(reteiva).ToString("N2", new CultureInfo("is-IS")),
                    autoRetencion = Math.Round(autoRetencion),
                    //autoRetencion = Math.Round(autoRetencion).ToString("N2", new CultureInfo("is-IS")),
                    totalretenciones = Math.Round(totalretenciones),
                    //totalretenciones = Math.Round(totalretenciones).ToString("N2", new CultureInfo("is-IS")),
                    valor_proveedor = Math.Round(valor_proveedor)
                    //valor_proveedor = Math.Round(valor_proveedor).ToString("N2", new CultureInfo("is-IS"))
                };

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json(0);
        }

        public decimal calcularbruto(decimal? valor, decimal? pordescuento, decimal? poriva) {
            decimal respuesta = 0;
            if (valor != null && pordescuento != null && poriva!=null)
            {
                respuesta = valor.Value - pordescuento.Value - poriva.Value;
            }

            return respuesta;
        }

        public decimal calcular_bruto(decimal? valor, decimal? pordescuento) {

            decimal respuesta = 0;
            if (valor != null && pordescuento != null)
            {
                respuesta = valor.Value - pordescuento.Value;
            }

            return respuesta;
        }

        public decimal calculardescuento(decimal? valor, decimal? pordescuento)
        {
            decimal respuesta = 0;
            if (valor != null && pordescuento != null)
            {
                respuesta = (pordescuento.Value / 100) * valor.Value;
                }

            return respuesta;
        }
        public decimal calculariva(decimal? valor, decimal? pordescuento, decimal? poriva)
        {
            decimal respuesta = 0;
            decimal respuesta2 = 0;
            if (valor != null && poriva != null)
            {
                if (pordescuento != null)
                {
                    respuesta2 = ( pordescuento.Value / 100) * valor.Value ;
                    respuesta =  (poriva.Value / 100) * (valor.Value - respuesta2);
                }
                else
                {
                    respuesta =  ( poriva.Value / 100)* valor.Value;
                }
            }

            return respuesta;
        }

        public decimal calcular_iva(decimal? valor, decimal? pordescuento)
        {
            decimal respuesta = 0;
            decimal respuesta2 = 0;
            if (valor != null)
            {
                if (pordescuento != null)
                {
                    respuesta2 =   (pordescuento.Value / 100) * valor.Value;
                    respuesta =  respuesta2;
                }
                else
                {
                    respuesta = valor.Value;
                }
            }

            return respuesta;
        }

        public decimal calculardescuentore(decimal? valor, decimal? cantidad, decimal? pordescuento)
        {
            decimal respuesta = 0;
            if (valor != null && cantidad != null && pordescuento != null)
            {
                respuesta = valor.Value * cantidad.Value * pordescuento.Value / 100;
            }

            return respuesta;
        }

        public decimal calcularivare(decimal? valor, decimal? cantidad, decimal? pordescuento, decimal? poriva)
        {
            decimal respuesta = 0;
            decimal respuesta2 = 0;
            if (valor != null && poriva != null)
            {
                if (pordescuento != null)
                {
                    respuesta2 = valor.Value * cantidad.Value * pordescuento.Value / 100;
                    respuesta = (valor.Value * cantidad.Value - respuesta2) * poriva.Value / 100;
                }
                else
                {
                    respuesta = valor.Value * cantidad.Value * poriva.Value / 100;
                }
            }

            return respuesta;
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

        public JsonResult agregarMotivoSecundario(string codigo, int razon2)
        {
            var result = 0;
            tencabezaorden e = new tencabezaorden();

            //var data = context.tencabezaorden.Where(x => x.codigoentrada== codigo).Select(x=> x.id);

            var data = (from t in context.tencabezaorden
                        where t.codigoentrada == codigo
                        select new
                        {
                            t.id
                        }).FirstOrDefault();

            var state = context.tencabezaorden.Find(data.id);

            state.razoningreso2 = razon2;

            context.Entry(state).State = EntityState.Modified;
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

        public ActionResult VerOrdenTrabajo(int? id, int? menu)
        {
            if (Session["user_usuarioid"] != null)
            {
                int usuario = Convert.ToInt32(Session["user_usuarioid"]);
                //busco el rol del usuario
                int rol = Convert.ToInt32(Session["user_rolid"]);
                //busco la bodega del usuario
                int bodegaactual = Convert.ToInt32(Session["user_bodega"]);

                //parámetro de sistema ROL ADMIN
                icb_sysparameter roladmin = context.icb_sysparameter.Where(d => d.syspar_cod == "P109").FirstOrDefault();
                int admin = roladmin != null ? Convert.ToInt32(roladmin.syspar_value) : 1;


                //busco la orden de trabajo
                tencabezaorden orden = context.tencabezaorden.Where(d => d.id == id).FirstOrDefault();
                if (orden != null)
                {
                    int permitido = 0;
                    if (rol == admin)
                    {
                        permitido = 1;
                    }
                    else if (orden.bodega == bodegaactual)
                    {
                        permitido = 1;
                    }

                    if (permitido == 1)
                    {
                        OperacionLiquidacionForm modelo = new OperacionLiquidacionForm
                        {
                            anio = orden.icb_vehiculo.anio_vh.ToString(),
                            asesor = orden.asesor,
                            bodega = orden.bodega,
                            codigoentrada = orden.codigoentrada,
                            color = orden.icb_vehiculo.color_vehiculo.colvh_nombre,
                            correo_cliente = orden.icb_terceros.email_tercero,
                            direccion_cliente = orden.icb_terceros.terceros_direcciones.Count > 0
                                ? orden.icb_terceros.terceros_direcciones.Select(e => e.direccion).FirstOrDefault()
                                : "",
                            fecha_estimada_entrega =
                                orden.entrega.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")),
                            id = orden.id,
                            idorden = orden.id,
                            id_cliente = orden.tercero,
                            modelo = orden.icb_vehiculo.modelo_vehiculo.modvh_nombre,
                            placa = !string.IsNullOrWhiteSpace(orden.icb_vehiculo.plac_vh)
                                ? orden.icb_vehiculo.plac_vh
                                : "",
                            nombre_cliente = !string.IsNullOrWhiteSpace(orden.icb_terceros.razon_social)
                                ? orden.icb_terceros.razon_social
                                : orden.icb_terceros.razon_social + " " + orden.icb_terceros.razon_social,
                            plan_mayor = orden.icb_vehiculo.plan_mayor,
                            razon_ingreso = orden.razoningreso,
                            razon_ingreso2 = orden.razoningreso2,
                            telefono_cliente = orden.icb_terceros.telf_tercero,
                            numerocita = orden.idcita != null ? orden.idcita : null,
                            bahia = orden.idcita != null ? orden.tcitastaller.tbahias.codigo_bahia : "",
                            aseguradora = orden.aseguradora != null ? orden.aseguradora : null,
                            deducible = orden.deducible != null
                                ? orden.deducible.Value.ToString("N0", new CultureInfo("is-IS"))
                                : "",
                            fecha_soat = orden.fecha_soat != null
                                ? orden.fecha_soat.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                                : "",
                            garantia_causa = !string.IsNullOrWhiteSpace(orden.garantia_causa)
                                ? orden.garantia_causa
                                : "",
                            garantia_falla = !string.IsNullOrWhiteSpace(orden.garantia_falla)
                                ? orden.garantia_falla
                                : "",
                            garantia_solucion = !string.IsNullOrWhiteSpace(orden.garantia_solucion)
                                ? orden.garantia_solucion
                                : "",
                            minimo = orden.minimo != null
                                ? orden.minimo.Value.ToString("N0", new CultureInfo("is-IS"))
                                : "",
                            numero_soat = !string.IsNullOrWhiteSpace(orden.numero_soat) ? orden.numero_soat : "",
                            poliza = !string.IsNullOrWhiteSpace(orden.poliza) ? orden.poliza : "",
                            siniestro = !string.IsNullOrWhiteSpace(orden.siniestro) ? orden.siniestro : "",
                            kilometraje = orden.kilometraje.ToString("N0", new CultureInfo("is-IS")),
                            txtDocumentoCliente = orden.icb_terceros.doc_tercero,
                            notas = !string.IsNullOrWhiteSpace(orden.notas) ? orden.notas : "",
                            id_plan_mantenimiento = orden.id_plan_mantenimiento,
                            tecnico = orden.idtecnico
                        };

                        BuscarFavoritos(menu);

                        listasDesplegables2(modelo);
                        return View(modelo);
                    }

                    TempData["mensaje_error"] = "No tiene autorización para ver la orden de trabajo seleccionada.";
                    return RedirectToAction("Index", "CentralAtencion");
                }

                TempData["mensaje_error"] = "La orden de trabajo no existe. Por favor validar.";
                return RedirectToAction("Index", "CentralAtencion");
            }

            return RedirectToAction("Login", "Login");
        }

        public void listasDesplegables2(OperacionLiquidacionForm orden)
        {
            ViewBag.razon_ingreso = new SelectList(context.trazonesingreso, "id", "razoz_ingreso", orden.razon_ingreso);
            ViewBag.razon_ingreso2 = new SelectList(context.trazonesingreso, "id", "razoz_ingreso", orden.razon_ingreso2);
            icb_sysparameter garantiaparametro = context.icb_sysparameter.Where(d => d.syspar_cod == "P87").FirstOrDefault();
            ViewBag.razongarantia = garantiaparametro != null ? Convert.ToInt32(garantiaparametro.syspar_value) : 7;
            if (orden.razon_ingreso != null)
            {
                List<trazonesingreso> razonessec = context.trazonesingreso.Where(d => d.id != orden.razon_ingreso).ToList();
            }

            if (orden.razon_ingreso2 != null)
            {
                List<trazonesingreso> razonessec = context.trazonesingreso.Where(d => d.id != orden.razon_ingreso2).ToList();
            }

            ViewBag.id_plan_mantenimiento = new SelectList(context.tplanmantenimiento, "id", "Descripcion",
                orden.id_plan_mantenimiento);

            ViewBag.deducible =
                orden.deducible /*!=null?orden.deducible.Value.ToString("N0",new CultureInfo("is-IS")):""*/;
            ViewBag.minimoOrden =
                orden.minimo /*!= null ? orden.minimo.Value.ToString("N0", new CultureInfo("is-IS")) : ""*/;
            //ViewBag.tipooperacion = new SelectList(context.ttipooperacion, "id", "Descripcion", orden.tipooperacion);
            ViewBag.aseguradora = new SelectList(context.icb_aseguradoras, "aseg_id", "nombre", orden.aseguradora);

            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);

            //busco el documento con el tipo Orden de Taller (tabla tp_doc_registros_tipo)
            icb_sysparameter linkd = context.icb_sysparameter.Where(d => d.syspar_cod == "P90").FirstOrDefault();
            string linkderepuestos = linkd != null ? linkd.syspar_value : "https://www.google.com";
            ViewBag.linkrepuestos = linkderepuestos;
            //busco el documento con el tipo Orden de Taller (tabla tp_doc_registros_tipo)
            icb_sysparameter tipoorden = context.icb_sysparameter.Where(d => d.syspar_cod == "P76").FirstOrDefault();
            int tipodocorden = tipoorden != null ? Convert.ToInt32(tipoorden.syspar_value) : 26;

            //busco el documento con el tipo Orden de Taller (tabla tp_doc_registros_tipo)
            icb_sysparameter razin = context.icb_sysparameter.Where(d => d.syspar_cod == "87").FirstOrDefault();
            int razongarantia = razin != null ? Convert.ToInt32(razin.syspar_value) : 7;
            ViewBag.tipogarantia = razongarantia;
            if (orden.razon_ingreso == razongarantia /*|| orden.razon_secundaria == razongarantia*/)
            {
                ViewBag.camposgarantia = 1;
            }
            else
            {
                ViewBag.camposgarantia = 0;
            }

            var buscarDocumentos = (from consecutivos in context.icb_doc_consecutivos
                                    join bodega in context.bodega_concesionario
                                        on consecutivos.doccons_bodega equals bodega.id
                                    join tipoDocumento in context.tp_doc_registros
                                        on consecutivos.doccons_idtpdoc equals tipoDocumento.tpdoc_id
                                    where consecutivos.doccons_bodega == bodegaActual && tipoDocumento.tipo == tipodocorden
                                    select new
                                    {
                                        tipoDocumento.tpdoc_id,
                                        nombre = "(" + tipoDocumento.prefijo + ") " + tipoDocumento.tpdoc_nombre,
                                        bodega.bodccs_nombre,
                                        bodega.id
                                    }).ToList();

            //busco el rol que se asigna a los asesores de servicio
            icb_sysparameter aseserv1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P77").FirstOrDefault();
            int rolasesor = aseserv1 != null ? Convert.ToInt32(aseserv1.syspar_value) : 2025;
            var buscarAsesoresServicio = (from usuario in context.users
                                          join bodegaUsuario in context.bodega_usuario
                                              on usuario.user_id equals bodegaUsuario.id_usuario
                                          where bodegaUsuario.id_bodega == bodegaActual && usuario.rol_id == rolasesor
                                          select new
                                          {
                                              usuario.user_id,
                                              nombreUsuario = usuario.user_nombre + " " + usuario.user_apellido
                                          }).ToList();


            //busco el rol que se asignas a los técnicos
            icb_sysparameter asetec1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P48").FirstOrDefault();
            int roltecnico = asetec1 != null ? Convert.ToInt32(asetec1.syspar_value) : 1014;
            var buscarTecnicos = (from usuario in context.users
                                  join bodegaUsuario in context.bodega_usuario
                                      on usuario.user_id equals bodegaUsuario.id_usuario
                                  where bodegaUsuario.id_bodega == bodegaActual && usuario.rol_id == roltecnico
                                  select new
                                  {
                                      usuario.user_id,
                                      nombreUsuario = usuario.user_nombre + " " + usuario.user_apellido
                                  }).ToList();


            //ViewBag.idtipodoc = new SelectList(buscarDocumentos, "tpdoc_id", "nombre", orden.idtipodoc);
            ViewBag.asesor = new SelectList(buscarAsesoresServicio, "user_id", "nombreUsuario", orden.asesor);
            ViewBag.operacionesTempario = new SelectList(context.ttempario, "codigo", "operacion");
            //ViewBag.tipoTarifa = new SelectList(context.ttipostarifa, "id", "Descripcion");
            //ViewBag.tipoTarifaR = new SelectList(context.ttipostarifa, "id", "Descripcion");
            var tecnicos = context.ttecnicos.Where(d => d.estado && d.users.user_estado).Select(d =>
            new { id = d.users.user_id, nombre = d.users.user_nombre + " " + d.users.user_apellido }).ToList();

            ViewBag.tecnico = new SelectList(buscarTecnicos, "user_id", "nombreUsuario", orden.tecnico);
            var buscarRepuestos = (from referencia in context.icb_referencia
                                   where referencia.modulo == "R" && referencia.ref_estado
                                   select new
                                   {
                                       referencia.ref_codigo,
                                       ref_descripcion = referencia.ref_codigo + " - " + referencia.ref_descripcion,
                                       referencia.manejo_inv
                                   }).ToList();
            ViewBag.repuestos = new SelectList(buscarRepuestos, "ref_codigo", "ref_descripcion");
            ViewBag.repuestosG = new SelectList(buscarRepuestos.Where(x => x.manejo_inv == false), "ref_codigo",
                "ref_descripcion");

            var centroCosotos = (from cc in context.centro_costo
                                 where cc.centcst_estado
                                 orderby cc.pre_centcst
                                 select new
                                 {
                                     value = cc.centcst_id,
                                     text = "(" + cc.pre_centcst + ") " + cc.centcst_nombre
                                 }).ToList();

            ViewBag.centroCostoModal = new SelectList(centroCosotos, "value", "text");
        }

        public JsonResult buscarPaginados(string filtroGeneral, int[] bodega, string fechaini, string fechafin, int condicion)
        {



            //parametros de sistema de estados terminado y facturado
            icb_sysparameter estao1 = context.icb_sysparameter.Where(d => d.syspar_value == "P89").FirstOrDefault();
            int estadotermi = estao1 != null ? Convert.ToInt32(estao1.syspar_value) : 10;

            icb_sysparameter estado2 = context.icb_sysparameter.Where(d => d.syspar_value == "P111").FirstOrDefault();
            int estadofactu = estado2 != null ? Convert.ToInt32(estado2.syspar_value) : 10;


            if (Session["user_usuarioid"] != null)
            {
                string draw = Request.Form.GetValues("draw").FirstOrDefault();
                string start = Request.Form.GetValues("start").FirstOrDefault();
                string length = Request.Form.GetValues("length").FirstOrDefault();
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
                //busco el usuario
                users usuario = context.users.Where(d => d.user_id == idusuario).FirstOrDefault();
                int rol = Convert.ToInt32(Session["user_rolid"]);

                //por si necesito el rol del usuario

                icb_sysparameter admin1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P109").FirstOrDefault();
                int admin = admin1 != null ? Convert.ToInt32(admin1.syspar_value) : 1;

                Expression<Func<vw_browser_central_atencion, bool>> predicado = PredicateBuilder.True<vw_browser_central_atencion>();
                Expression<Func<vw_browser_central_atencion, bool>> predicado2 = PredicateBuilder.False<vw_browser_central_atencion>();
                Expression<Func<vw_browser_central_atencion, bool>> predicado3 = PredicateBuilder.False<vw_browser_central_atencion>();




                if (bodega.Count() > 0 && bodega[0] != 0)
                {
                    foreach (int item in bodega)
                    {
                        predicado2 = predicado2.Or(d => d.bodega == item.ToString());
                    }

                    predicado = predicado.And(predicado2);
                }
                else
                {
                    if (rol != admin)
                    {
                        //bodegasusuario
                        List<int> bode = context.bodega_usuario.Where(d => d.id_usuario == idusuario).Select(d => d.id_bodega)
                            .ToList();
                        var bodegas = context.bodega_concesionario.Where(d => d.bodccs_estado && bode.Contains(d.id))
                            .Select(d => new { d.id, nombre = d.bodccs_cod + "-" + d.bodccs_nombre }).ToList();
                        foreach (var item in bodegas)
                        {
                            predicado2 = predicado2.Or(d => d.bodega == item.id.ToString());
                        }

                        predicado = predicado.And(predicado2);
                    }
                }
                if (fechaini != "")
                {
                    DateTime fini = Convert.ToDateTime(fechaini);
                    predicado = predicado.And(d => d.fec_creacion >= fini);
                }

                if (fechafin != "")
                {
                    DateTime ffin = Convert.ToDateTime(fechafin);
                    ffin = ffin.AddDays(1);
                    predicado = predicado.And(d => d.fec_creacion <= ffin);

                }


                if (condicion==0)
                    {
                   string idestadofacturada = context.icb_sysparameter.Where(d => d.syspar_cod == "P111").Select(x=>x.syspar_value).FirstOrDefault();
                    int estadofactura = Convert.ToInt32(idestadofacturada);

                    predicado = predicado.And(d => d.idestado != estadofactura);


                    }



                if (!string.IsNullOrEmpty(filtroGeneral))
                {
                    predicado3 = predicado3.Or(d => 1 == 1 && d.codigoentrada.ToString().Contains(filtroGeneral));
                    predicado3 = predicado3.Or(d => 1 == 1 && d.fecha2.ToUpper().Contains(filtroGeneral.ToUpper()));
                    predicado3 = predicado3.Or(d => 1 == 1 && d.bodccs_nombre.Contains(filtroGeneral.ToUpper()));
                    predicado3 = predicado3.Or(d => 1 == 1 && d.placa_plan.Contains(filtroGeneral.ToUpper()));
                    predicado3 = predicado3.Or(d => 1 == 1 && d.nombrecliente.Contains(filtroGeneral.ToUpper()));
                    predicado3 = predicado3.Or(d =>
                        1 == 1 && d.doc_tercero.ToUpper().Contains(filtroGeneral.ToUpper()));
                    predicado3 =
                        predicado3.Or(d => 1 == 1 && d.modvh_nombre.ToUpper().Contains(filtroGeneral.ToUpper()));
                    predicado = predicado.And(predicado3);
                }


                int registrostotales = context.vw_browser_central_atencion.Where(predicado).Count();
                //si el ordenamiento es ascendente o descendente es distinto
                if (pageSize == -1)
                {
                    pageSize = registrostotales;
                }

                if (sortColumnDir == "asc")
                {
                    List<vw_browser_central_atencion> query2 = context.vw_browser_central_atencion.Where(predicado)
                        .OrderBy(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
                    var query = query2.Select(d => new
                    {
                        d.codigoentrada,
                        d.placa_plan,

                        d.modvh_nombre,
                        razon_ingreso = d.razon_ingreso != null ? d.razon_ingreso : "",
                        razon_ingreso2 = d.razon_ingreso2 != null ? d.razon_ingreso2 : "",
                        d.vin,
                        d.kilometraje,
                        d.bodccs_nombre,
                        d.doc_tercero,
                        d.nombrecliente,
                        d.Descripcion,
                        d.color_estado,
                        d.idestado,
                        d.fecha2,
                        d.asesor_tecnico,
                        bahia = d.bahia != null ? d.bahia : "",
                        terminada = d.fecha_fin_operacion != null ? 1 : 0,
                        facturada = d.fecha_liquidacion != null ? 1 : 0,
                        fin_operacion = d.fecha_fin_operacion != null ? 1 : 0,
                        tiempo = convertirnumeroxfecha(d.id),
                        //operaciones = context.tdetallemanoobraot.Where(e => e.idorden == d.id && !string.IsNullOrEmpty(e.estado)).Select(e => new
                        //{
                        //    idoperacion = e.id,
                        //    descripcion = e.ttempario.operacion,
                        //    tiempo = e.tiempo,
                        //}).ToList(),
                        d.id
                    }).ToList();
                    int contador = query.Count();
                    return Json(
                        new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                        JsonRequestBehavior.AllowGet);
                }
                else
                {
                    List<vw_browser_central_atencion> query2 = context.vw_browser_central_atencion.Where(predicado)
                        .OrderByDescending(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
                    var query = query2.Select(d => new
                    {
                        d.codigoentrada,
                        d.placa_plan,
                        razon_ingreso = d.razon_ingreso != null ? d.razon_ingreso : "",
                        razon_ingreso2 = d.razon_ingreso2 != null ? d.razon_ingreso2 : "",
                        d.vin,
                        d.kilometraje,
                        d.modvh_nombre,
                        d.bodccs_nombre,
                        d.doc_tercero,
                        d.nombrecliente,
                        d.Descripcion,
                        d.color_estado,
                        d.idestado,
                        d.fecha2,
                        d.asesor_tecnico,
                        bahia = d.bahia != null ? d.bahia : "",
                        terminada = d.fecha_fin_operacion != null ? 1 : 0,
                        facturada = d.fecha_liquidacion != null ? 1 : 0,
                        fin_operacion = d.fecha_fin_operacion != null ? 1 : 0,
                        tiempo = convertirnumeroxfecha(d.id),
                        //operaciones = context.tdetallemanoobraot.Where(e => e.idorden == d.id && !string.IsNullOrEmpty(e.estado)).Select(e => new
                        //{
                        //    idoperacion = e.id,
                        //    descripcion = e.ttempario.operacion,
                        //    tiempo = e.tiempo,
                        //}).ToList(),
                        d.id
                    }).ToList();
                    int contador = query.Count();
                    return Json(
                        new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                        JsonRequestBehavior.AllowGet);
                }
            }

            return Json(0);
        }

        public ActionResult DescargarExcel(string filtroGeneral, string bodegax, string fechaini, string fechafin) {
            
            string[] bodegasf = (!string.IsNullOrWhiteSpace(bodegax))?bodegax.Split('~'):string.Empty.Split('~');

            int[] bodega = new int[(bodegasf[0] == ""?0:bodegasf.Length)];
            if (bodegasf[0]!="")
            {
                for (int i = 0; i < bodegasf.Length; i++)
                {              
                    bodega[i] = Convert.ToInt32(bodegasf[i]);
                }
            }
            else
            {

            }

            CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");

            int idusuario = Convert.ToInt32(Session["user_usuarioid"]);
            //busco el usuario
            users usuario = context.users.Where(d => d.user_id == idusuario).FirstOrDefault();
            int rol = Convert.ToInt32(Session["user_rolid"]);

            //por si necesito el rol del usuario

            icb_sysparameter admin1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P109").FirstOrDefault();
            int admin = admin1 != null ? Convert.ToInt32(admin1.syspar_value) : 1;

            Expression<Func<vw_browser_central_atencion, bool>> predicado = PredicateBuilder.True<vw_browser_central_atencion>();
            Expression<Func<vw_browser_central_atencion, bool>> predicado2 = PredicateBuilder.False<vw_browser_central_atencion>();
            Expression<Func<vw_browser_central_atencion, bool>> predicado3 = PredicateBuilder.False<vw_browser_central_atencion>();




            if (bodega.Count() > 0 && bodega[0] != 0)
                {
                foreach (int item in bodega)
                    {
                    predicado2 = predicado2.Or(d => d.bodega == item.ToString());
                    }

                predicado = predicado.And(predicado2);
                }
            else
                {
                if (rol != admin)
                    {
                    //bodegasusuario
                    List<int> bode = context.bodega_usuario.Where(d => d.id_usuario == idusuario).Select(d => d.id_bodega)
                        .ToList();
                    var bodegas = context.bodega_concesionario.Where(d => d.bodccs_estado && bode.Contains(d.id))
                        .Select(d => new { d.id, nombre = d.bodccs_cod + "-" + d.bodccs_nombre }).ToList();
                    foreach (var item in bodegas)
                        {
                        predicado2 = predicado2.Or(d => d.bodega == item.id.ToString());
                        }

                    predicado = predicado.And(predicado2);
                    }
                }
            if (fechaini != "")
                {
                DateTime fini = Convert.ToDateTime(fechaini);
                predicado = predicado.And(d => d.fec_creacion >= fini);
                }

            if (fechafin != "")
                {
                DateTime ffin = Convert.ToDateTime(fechafin);
                ffin = ffin.AddDays(1);
                predicado = predicado.And(d => d.fec_creacion <= ffin);

                }



            if (!string.IsNullOrEmpty(filtroGeneral))
                {
                predicado3 = predicado3.Or(d => 1 == 1 && d.codigoentrada.ToString().Contains(filtroGeneral));
                predicado3 = predicado3.Or(d => 1 == 1 && d.fecha2.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.bodccs_nombre.Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.placa_plan.Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.nombrecliente.Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d =>
                    1 == 1 && d.doc_tercero.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 =
                    predicado3.Or(d => 1 == 1 && d.modvh_nombre.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado = predicado.And(predicado3);
                }

            List<vw_browser_central_atencion> query2 = context.vw_browser_central_atencion.Where(predicado).ToList();
            var query = query2.Select(d => new
                {
                d.codigoentrada,
                d.placa_plan,
                d.modvh_nombre,
                razon_ingreso = d.razon_ingreso != null ? d.razon_ingreso : "",
                razon_ingreso2 = d.razon_ingreso2 != null ? d.razon_ingreso2 : "",
                d.vin,
                d.kilometraje,
                d.bodccs_nombre,
                d.doc_tercero,
                d.nombrecliente,
                d.Descripcion,
                d.color_estado,
                d.idestado,
                d.fecha2,
                d.asesor_tecnico,
                bahia = d.bahia != null ? d.bahia : "",
                terminada = d.fecha_fin_operacion != null ? 1 : 0,
                facturada = d.fecha_liquidacion != null ? 1 : 0,
                fin_operacion = d.fecha_fin_operacion != null ? 1 : 0,
                tiempo = convertirnumeroxfecha(d.id),         
                d.id
                }).ToList();
            var queryarreglo = query.Select(d => new
                {
                d.codigoentrada,
                d.placa_plan,
                d.modvh_nombre,
                d.razon_ingreso,
                d.razon_ingreso2,
                d.vin,
                d.kilometraje,
                d.bodccs_nombre,
                d.doc_tercero,
                d.nombrecliente,
                d.Descripcion,            
                d.fecha2,
                d.bahia,             
                d.tiempo
                
                }).ToArray();

            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            workSheet.Cells[1, 1].Value = "Informe Orden de Trabajo";

            workSheet.Cells[3, 1].Value = "OT";
            workSheet.Cells[3, 2].Value = "Placa / Plan";
            workSheet.Cells[3, 3].Value = "Descripcion";
            workSheet.Cells[3, 4].Value = "Razon Ingreso";
            workSheet.Cells[3, 5].Value = "Razon Ingreso 2";
            workSheet.Cells[3, 6].Value = "Vin";
            workSheet.Cells[3, 7].Value = "Kilometraje";
            workSheet.Cells[3, 8].Value = "Bodega";
            workSheet.Cells[3, 9].Value = "Cedula";
            workSheet.Cells[3, 10].Value = "Nombre Cliente";
            workSheet.Cells[3, 11].Value = "Estado";           
            workSheet.Cells[3, 12].Value = "Fecha Creacion";
            workSheet.Cells[3, 13].Value = "Bahia";       
            workSheet.Cells[3, 14].Value = "Tiempo";
        

            workSheet.Cells[4, 1].LoadFromCollection(queryarreglo, false);

            using (var memoryStream = new MemoryStream())
                {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;  filename=gestionot.xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
                }

            return View();
            }

        public JsonResult buscarPaginados2(string filtros, string valorFiltros, string filtroGeneral, int? menu)
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

                Expression<Func<vw_browser_central_atencion, bool>> predicado = PredicateBuilder.True<vw_browser_central_atencion>();
                Expression<Func<vw_browser_central_atencion, bool>> predicado2 = PredicateBuilder.False<vw_browser_central_atencion>();
                Expression<Func<vw_browser_central_atencion, bool>> predicado3 = PredicateBuilder.False<vw_browser_central_atencion>();
                Expression<Func<vw_browser_central_atencion, bool>> predicado4 = PredicateBuilder.False<vw_browser_central_atencion>();
                Expression<Func<vw_browser_central_atencion, bool>> predicado5 = PredicateBuilder.False<vw_browser_central_atencion>();
                Expression<Func<vw_browser_central_atencion, bool>> predicado6 = PredicateBuilder.False<vw_browser_central_atencion>();

                string[] vectorNombresFiltro = !string.IsNullOrEmpty(filtros) ? filtros.Split(',') : new string[1];
                string[] vectorValoresFiltro = !string.IsNullOrEmpty(valorFiltros) ? valorFiltros.Split('~') : new string[1];

                //busco los valores de busqueda del menu
                string[] menus = context.menu_busqueda.Where(d => d.menu_busqueda_id_menu == menu)
                    .Select(d => d.menu_busqueda_campo).ToArray();

                List<filtrosdatatable> listavalores = new List<filtrosdatatable>();
                //lleno el vector de valores
                for (int i = 0; i < menus.Length; i++)
                {
                    listavalores.Add(new filtrosdatatable { nombre = menus[i], value = "" });
                }
                //desde cero al filtro
                for (int i = 0; i < vectorNombresFiltro.Length; i++)
                {
                    for (int j = 0; j < menus.Length; j++)
                    {
                        if (listavalores[j].nombre == vectorNombresFiltro[i] && listavalores[j].value == "")
                        {
                            listavalores[j].value = vectorValoresFiltro[i];
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(filtroGeneral))
                {

                    predicado2 = predicado2.Or(d => 1 == 1 && d.bodccs_nombre.ToUpper().Contains(filtroGeneral));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.codigoentrada.ToUpper().Contains(filtroGeneral));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.placa_plan.ToUpper().Contains(filtroGeneral));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.doc_tercero.ToUpper().Contains(filtroGeneral));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.nombrecliente.ToUpper().Contains(filtroGeneral));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.Descripcion.ToUpper().Contains(filtroGeneral));
                    predicado2 = predicado2.Or(d => 1 == 1 && d.razon_ingreso.ToUpper().Contains(filtroGeneral));

                    predicado = predicado.And(predicado2);
                }
                else
                {


                    predicado = predicado.And(predicado2);
                    List<filtrosdatatable> filtrosseleccionados = listavalores.Where(d => d.value != "").ToList();
                    foreach (filtrosdatatable item in filtrosseleccionados)
                    {
                        string nombre = item.nombre;
                        switch (nombre)
                        {
                            case "plan_mayor":
                                predicado3 = predicado.Or(d => d.placa_plan.Equals(item.value));

                                predicado = predicado.And(predicado3);
                                break;
                            case "modvh_id":
                                string modelo = item.value;
                                string[] modelo2 = modelo.Split(',');
                                foreach (string item2 in modelo2)
                                {
                                    predicado4 = predicado4.Or(d => d.modvh_id == item2);
                                }

                                predicado = predicado.And(predicado4);
                                break;
                            case "doc_tercero":
                                predicado = predicado.And(d => d.doc_tercero.Equals(item.value));
                                break;
                            case "nombrecliente":
                                predicado = predicado.And(d => d.nombrecliente.Contains(item.value));
                                break;
                            case "bodega":
                                string bodega = item.value;
                                string[] bodega2 = bodega.Split(',');
                                foreach (string item2 in bodega2)
                                {
                                    predicado4 = predicado4.Or(d => d.bodega == item2);
                                }

                                predicado = predicado.And(predicado4);
                                break;
                            case "fecha2":
                                string fechax = item.value;
                                string[] fecha2 = fechax.Split('-');
                                if (fecha2.Length > 1)
                                {
                                    DateTime fechadesde = Convert.ToDateTime(fecha2[0]);
                                    DateTime fechahasta = Convert.ToDateTime(fecha2[1]).AddDays(1);
                                    predicado = predicado.And(d => d.fec_creacion >= fechadesde);
                                    predicado = predicado.And(d => d.fec_creacion <= fechahasta);
                                }
                                else
                                {
                                    DateTime fechadesde = Convert.ToDateTime(fecha2[0]);
                                    predicado = predicado.And(d => d.fec_creacion == fechadesde);
                                }

                                break;
                            case "razoningreso":
                                string razon = item.value;
                                string[] razon2 = razon.Split(',');
                                foreach (string item3 in razon2)
                                {
                                    predicado5 = predicado5.Or(d => d.razoningreso == item3);
                                }

                                predicado = predicado.And(predicado5);
                                break;
                            case "codigoentrada":
                                predicado = predicado.And(d => d.codigoentrada.Equals(item.value));
                                break;
                        }
                    }
                }

                int registrostotales = context.vw_browser_central_atencion.Where(predicado).Count();

                if (sortColumnDir == "asc")
                {
                    List<vw_browser_central_atencion> query2 = context.vw_browser_central_atencion.Where(predicado)
                        .OrderBy(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
                    var query = query2.Select(d => new
                    {
                        d.codigoentrada,
                        d.placa_plan,
                        d.modvh_nombre,
                        d.bodccs_nombre,
                        d.doc_tercero,
                        d.nombrecliente,
                        d.Descripcion,
                        d.fecha2,
                        d.color_estado,
                        //operaciones = context.tdetallemanoobraot.Where(e => e.idorden == d.id && !string.IsNullOrEmpty(e.estado)).Select(e => new
                        //{
                        //    idoperacion = e.id,
                        //    descripcion = e.ttempario.operacion,
                        //    tiempo = e.tiempo,
                        //}).ToList(),
                        d.razon_ingreso,
                        d.id
                    }).ToList();
                    int contador = query.Count();
                    return Json(
                        new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                        JsonRequestBehavior.AllowGet);
                }
                else
                {
                    List<vw_browser_central_atencion> query2 = context.vw_browser_central_atencion.Where(predicado)
                        .OrderByDescending(GetColumnName(sortColumn).Compile()).Skip(skip).Take(pageSize).ToList();
                    var query = query2.Select(d => new
                    {
                        d.codigoentrada,
                        d.placa_plan,

                        d.modvh_nombre,
                        d.bodccs_nombre,
                        d.doc_tercero,
                        d.nombrecliente,
                        d.Descripcion,
                        d.fecha2,
                        d.color_estado,
                        //operaciones = context.tdetallemanoobraot.Where(e => e.idorden == d.id && !string.IsNullOrEmpty(e.estado)).Select(e => new
                        //{
                        //    idoperacion = e.id,
                        //    descripcion = e.ttempario.operacion,
                        //    tiempo = e.tiempo,
                        //}).ToList(),
                        d.razon_ingreso,
                        d.id
                    }).ToList();
                    int contador = query.Count();
                    return Json(
                        new { draw, recordsFiltered = registrostotales, recordsTotal = registrostotales, data = query },
                        JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { errorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
                // Exception
            }
        }

        public JsonResult infopaneles()
        {

            var info = "";
            DateTime fecha = DateTime.Now;
            DateTime fechamenos7 = DateTime.Now.AddDays(-7);
            List<tcitasestados> estados = context.tcitasestados.Where(X => X.ordenot != null).ToList();
            List<int> estadoid = estados.Select(x => x.id).ToList();
            List<tcitasestados> Coloresestados = context.tcitasestados.Where(X => X.tipoestado == "OT" || X.tipoestado == "E" || X.tipoestado == "T" || X.tipoestado == "L").ToList();
            var query = context.vw_browser_central_atencion.Where(x => x.fec_creacion >= fechamenos7 && x.fec_creacion <= fecha && estadoid.Contains(x.idestado.Value)).GroupBy(x => new { x.idestado, x.Descripcion, x.color_estado, x.ordenot }).Select(x =>
        new { id = x.Key.idestado, descripcion = x.Key.Descripcion, cantidad = x.Count(), color = x.Key.color_estado, orden = x.Key.ordenot }).ToList();

            query = query.OrderBy(x => x.orden).ToList();

            int EstadoTotal = context.vw_browser_central_atencion.Where(x => x.fec_creacion >= fechamenos7 && x.fec_creacion <= fecha && estadoid.Contains(x.idestado.Value)).Count();

            var idotcreada = context.tcitasestados.Where(X => X.tipoestado == "OT").Select(c => c.id).FirstOrDefault();


            var lista = estados.OrderBy(x => x.ordenot).Select(d => new CsPanelesColor
            {
                id = d.id,
                nombre = d.Descripcion,
                color = d.color_estado,
                order = Convert.ToInt32(d.ordenot),
                cantidad = query.Where(e => e.id == d.id).Select(x => x.cantidad).FirstOrDefault()
            }).ToList();

            if (EstadoTotal == 0)
                {
                info += "<div class='col-md-3'> <div class='' style='padding-top:2%; padding-bottom:2%; background-color:" + Coloresestados[2].color_estado + "'><div class='text-center'><h4> Total " + Coloresestados[2].Descripcion + "</h4><p style='font-size: 18px;'> 0 </p></div></div></div>";
                info += "<div class='col-md-3'> <div class='' style='padding-top:2%; padding-bottom:2%; background-color:" + Coloresestados[0].color_estado + "'><div class='text-center'><h4> Total " + Coloresestados[0].Descripcion + "</h4><p style='font-size: 18px;'> 0 </p></div></div></div>";
                info += "<div class='col-md-3'> <div class='' style='padding-top:2%; padding-bottom:2%; background-color:" + Coloresestados[1].color_estado + "'><div class='text-center'><h4> Total " + Coloresestados[1].Descripcion + "</h4><p style='font-size: 18px;'> 0 </p></div></div></div>";
                info += "<div class='col-md-3'> <div class='' style='padding-top:2%; padding-bottom:2%; background-color:" + Coloresestados[3].color_estado + "'><div class='text-center'><h4> Total " + Coloresestados[3].Descripcion + "</h4><p style='font-size: 18px;'> 0 </p></div></div></div>";

                }
            else
            {
                info += "<div class='col-md-3'> <div class='' style='padding-top:2%; padding-bottom:2%; background-color:" + Coloresestados[3].color_estado + "'><div class='text-center'><h4> Total " + Coloresestados[3].Descripcion + "s</h4><p style='font-size: 18px;'> " + EstadoTotal + "</p></div></div></div>";

                foreach (var item in lista)
                {
                    if (item.order != 1)
                    {
                        info += "<div class='col-md-3'> <div class='' style='padding-top:2%; padding-bottom:2%; background-color:" + item.color + "'><div class='text-center'><h4> Total " + item.nombre + "</h4><p style='font-size: 18px;'> " + item.cantidad + " </p></div></div></div>";

                    }


                }

            }

            return Json(info, JsonRequestBehavior.AllowGet);
        }

        public JsonResult infopanelesfiltro(int[] bodega, string fechaini, string fechafin,string filtroGeneral)
        {
            var info = "";
            int idusuario = Convert.ToInt32(Session["user_usuarioid"]);
            //busco el usuario
            users usuario = context.users.Where(d => d.user_id == idusuario).FirstOrDefault();
            int rol = Convert.ToInt32(Session["user_rolid"]);
            var predicado = PredicateBuilder.True<vw_browser_central_atencion>();
            var predicado2 = PredicateBuilder.False<vw_browser_central_atencion>();
            var predicado3 = PredicateBuilder.False<vw_browser_central_atencion>();

            //por si necesito el rol del usuario

            icb_sysparameter admin1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P109").FirstOrDefault();
            int admin = admin1 != null ? Convert.ToInt32(admin1.syspar_value) : 1;

            if (bodega.Count() > 0 && bodega[0] != 0)
            {
                foreach (int item in bodega)
                {
                    predicado2 = predicado2.Or(d => d.bodega == item.ToString());
                }

                predicado = predicado.And(predicado2);
            }
            else
            {
                if (rol != admin)
                {
                    //bodegasusuario
                    List<int> bode = context.bodega_usuario.Where(d => d.id_usuario == idusuario).Select(d => d.id_bodega)
                        .ToList();
                    var bodegas = context.bodega_concesionario.Where(d => d.bodccs_estado && bode.Contains(d.id))
                        .Select(d => new { d.id, nombre = d.bodccs_cod + "-" + d.bodccs_nombre }).ToList();
                    foreach (var item in bodegas)
                    {
                        predicado2 = predicado2.Or(d => d.bodega == item.id.ToString());
                    }

                    predicado = predicado.And(predicado2);
                }
            }
            if (fechaini != "")
            {
                DateTime fini = Convert.ToDateTime(fechaini);
                predicado = predicado.And(d => d.fec_creacion >= fini);
            }

            if (fechafin != "")
            {
                DateTime ffin = Convert.ToDateTime(fechafin);
                ffin = ffin.AddDays(1);
                predicado = predicado.And(d => d.fec_creacion <= ffin);

            }

            if (!string.IsNullOrEmpty(filtroGeneral))
            {
                predicado3 = predicado3.Or(d => 1 == 1 && d.codigoentrada.ToString().Contains(filtroGeneral));
                predicado3 = predicado3.Or(d => 1 == 1 && d.fecha2.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.bodccs_nombre.Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.placa_plan.Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d => 1 == 1 && d.nombrecliente.Contains(filtroGeneral.ToUpper()));
                predicado3 = predicado3.Or(d =>
                    1 == 1 && d.doc_tercero.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado3 =
                    predicado3.Or(d => 1 == 1 && d.modvh_nombre.ToUpper().Contains(filtroGeneral.ToUpper()));
                predicado = predicado.And(predicado3);
            }
            /*
            DateTime fini = Convert.ToDateTime(fechaini);
            DateTime ffin = Convert.ToDateTime(fechafin);
            ffin = ffin.AddDays(1);*/
            List<tcitasestados> estados = context.tcitasestados.Where(X => X.ordenot != null).ToList();
            List<int> estadoid = estados.Select(x => x.id).ToList();
            predicado = predicado.And(d => d.ordenot!=null);

            var Coloresestados = estados.OrderBy(d => d.id).GroupBy(d => d.ordenot).Select(d => new {
                orden=d.Key,
                color_estado = d.Select(e => e.color_estado).FirstOrDefault(),
                Descripcion=d.Key==2?"Ejecucion":d.Select(e=>e.Descripcion).FirstOrDefault()
            }).OrderBy(d=>d.orden).ToList();

            var query = context.vw_browser_central_atencion.Where(predicado).GroupBy(x => new { x.idestado, x.ordenot }).Select(x =>
       new { id = x.Select(d=>d.idestado).FirstOrDefault(), descripcion = x.Select(y=>y.Descripcion).FirstOrDefault(), cantidad = x.Count(), color = x.Select(y=>y.color_estado).FirstOrDefault(), orden = x.Key.ordenot }).ToList();

           /*
            var query = context.vw_browser_central_atencion.Where(x => x.fec_creacion <= ffin && x.fec_creacion >= fini && estadoid.Contains(x.idestado.Value) && bodega.Contains(x.bodega)).GroupBy(x => new { x.idestado, x.Descripcion, x.color_estado, x.ordenot }).Select(x =>
         new { id = x.Key.idestado, descripcion = x.Key.Descripcion, cantidad = x.Count(), color = x.Key.color_estado, orden = x.Key.ordenot }).ToList();
         */
            query = query.OrderBy(x => x.orden).ToList();

            //int EstadoTotal = context.vw_browser_central_atencion.Where(x => x.fec_creacion <= ffin && x.fec_creacion >= fini && estadoid.Contains(x.idestado.Value) && bodega.Contains(x.bodega)).Count();
            int EstadoTotal = context.vw_browser_central_atencion.Where(predicado).Count();

            var idotcreada = context.tcitasestados.Where(X => X.tipoestado == "OT").Select(c => c.id).FirstOrDefault();


            var lista = estados.GroupBy(d=>d.ordenot).Select(d => new CsPanelesColor
            {
                id = d.Select(e=>e.id).FirstOrDefault(),
                nombre = d.Key==2?"En Ejecucion":d.Select(e=>e.Descripcion).FirstOrDefault(),
                color = d.Select(e=>e.color_estado).FirstOrDefault(),
                order = Convert.ToInt32(d.Key),
                cantidad = query.Where(e => e.orden == d.Key).Sum(e => e.cantidad),
            }).OrderBy(d=>d.order).ToList();

            if (EstadoTotal == 0)
            {
                info += "<div class='col-md-3'> <div class='' style='padding-top:2%; padding-bottom:2%; background-color:" + Coloresestados[0].color_estado + "'><div class='text-center'><h4> Total " + Coloresestados[0].Descripcion + "</h4><p style='font-size: 18px;'> 0 </p></div></div></div>";
                info += "<div class='col-md-3'> <div class='' style='padding-top:2%; padding-bottom:2%; background-color:" + Coloresestados[1].color_estado + "'><div class='text-center'><h4> Total " + Coloresestados[1].Descripcion + "</h4><p style='font-size: 18px;'> 0 </p></div></div></div>";
                info += "<div class='col-md-3'> <div class='' style='padding-top:2%; padding-bottom:2%; background-color:" + Coloresestados[2].color_estado + "'><div class='text-center'><h4> Total " + Coloresestados[2].Descripcion + "</h4><p style='font-size: 18px;'> 0 </p></div></div></div>";
                info += "<div class='col-md-3'> <div class='' style='padding-top:2%; padding-bottom:2%; background-color:" + Coloresestados[3].color_estado + "'><div class='text-center'><h4> Total " + Coloresestados[3].Descripcion + "</h4><p style='font-size: 18px;'> 0 </p></div></div></div>";

                }
            else
            {
                //info += "<div class='col-md-3'> <div class='' style='padding-top:2%; padding-bottom:2%; background-color:" + Coloresestados[2].color_estado + "'><div class='text-center'><h4> Total " + Coloresestados[2].Descripcion + "s</h4><p style='font-size: 18px;'> " + Coloresestados[2].Descripcion + "</p></div></div></div>";

                foreach (var item in lista)
                {
                    /*if (item.order != 1)
                    {*/
                        info += "<div class='col-md-3'> <div class='' style='padding-top:2%; padding-bottom:2%; background-color:" + item.color + "'><div class='text-center'><h4> Total " + item.nombre + "</h4><p style='font-size: 18px;'> " + item.cantidad + " </p></div></div></div>";
                   // }
                }

            }

            return Json(info, JsonRequestBehavior.AllowGet);
        }

        public string convertirnumeroxfecha(int id)
        {
            string resultado = "";
            tencabezaorden orden = context.tencabezaorden.Where(d => d.id == id).FirstOrDefault();
            var totalpausas = TimeSpan.Zero;

            var pausas = context.tpausasorden.Where(x => x.idorden == id).Select(x => new { x.fecha_inicio, x.fecha_fin }).ToList();

            if (pausas != null)
            {
                foreach (var item in pausas)
                {

                    var pausa = item.fecha_fin - item.fecha_inicio;
                    totalpausas = totalpausas + (pausa!=null?TimeSpan.Parse(pausa.ToString()): TimeSpan.Zero);
                }
            }
            if (orden.fecha_inicio_operacion != null)
            {
                if (orden.fecha_fin_operacion != null)
                {
                    var tiempofin = TimeSpan.Zero;
                    if (orden.tiempototal != null)
                    {
                        tiempofin = new TimeSpan(orden.tiempototal.Value);
                        resultado = tiempofin.ToString(@"dd\.hh\:mm\:ss");
                    }
                    else
                    {
                        var tiempofind = orden.fecha_fin_operacion - orden.fecha_fin_operacion;
                        tiempofin = new TimeSpan(tiempofind.Value.Ticks);
                    }

                    resultado = tiempofin.ToString(@"dd\.hh\:mm\:ss");

                }
                else
                {
                    var tiempofin = DateTime.Now - orden.fecha_inicio_operacion;
                    TimeSpan tiempof = new TimeSpan(tiempofin.Value.Ticks);
                    resultado = tiempof.ToString(@"dd\.hh\:mm\:ss");
                }
            }
            else
            {
                resultado = "No se a Iniciado operacion";


            }

            return resultado;
        }
        public JsonResult CambiarTaller(int id, int idtaller)
        {
            icb_inspeccionvehiculos averia = context.icb_inspeccionvehiculos.Find(id);
            averia.taller_averia_id = idtaller;
            averia.encargado = Convert.ToInt32(Session["user_usuarioid"]);
            context.Entry(averia).State = EntityState.Modified;
            int result = context.SaveChanges();

            if (result > 0)
            {
                return Json(new { resultado = true, planmayor = averia.planmayor }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }

        public class Averias
        {
            public string id { get; set; }
        }


        public JsonResult solicitarAverias(string averias)
        {
            Averias[] listado = JsonConvert.DeserializeObject<Averias[]>(averias);
            int ordenTaller = 0;
            long estadoAveria = context.estadoAverias.Where(x => x.nombre.Contains("asignada")).FirstOrDefault().id;
            long estadoAveriaS = context.estadoAverias.Where(x => x.nombre.Contains("solucionada")).FirstOrDefault().id;
            long solucionAut = context.tallerAveria.Where(x => x.nombre.Contains("solucion")).FirstOrDefault().id;
            long OT = context.tallerAveria.Where(x => x.nombre.Contains("orden")).FirstOrDefault().id;
            for (int i = 0; i < listado.Length; i++)
            {
                int id = Convert.ToInt32(listado[i].id);
                icb_inspeccionvehiculos averia = context.icb_inspeccionvehiculos.Find(id);
                if (averia.taller_averia_id == solucionAut)
                {
                    averia.estado_averia_id = estadoAveriaS;
                    averia.encargado = Convert.ToInt32(Session["user_usuarioid"]);
                }
                else
                {
                    averia.estado_averia_id = estadoAveria;
                    if (averia.taller_averia_id == OT)
                    {
                        ordenTaller++;
                    }
                }
                averia.insp_solicitar = true;
                context.Entry(averia).State = EntityState.Modified;
                int resultado = context.SaveChanges();
            }
            return Json(new { resultado = true, OT = ordenTaller }, JsonRequestBehavior.AllowGet);
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

        public JsonResult Buscarpermiso() {

           int rol = Convert.ToInt32(Session["user_rolid"]);
            bool respuesta = false;
            var permiso = context.rolacceso.Where(a => a.rolpermisos.codigo == "P49" && a.idrol == rol).FirstOrDefault();
            if (permiso != null)
                {
                respuesta = true;
                }
            else {
                respuesta = false;
                }
            return Json(respuesta, JsonRequestBehavior.AllowGet);
            }


        public JsonResult ValidarDescuentoMaximo(decimal valor, int id) {
            var respuesta = false;
            tmaximodescuentomo descuento = context.tmaximodescuentomo.FirstOrDefault();

            if (descuento.valor>=valor)
                {
                tdetallemanoobraot manoobra = context.tdetallemanoobraot.Where(x => x.id == id).FirstOrDefault();
                manoobra.pordescuento = Convert.ToDecimal(valor, miCultura);
                context.Entry(manoobra).State = EntityState.Modified;
                context.SaveChanges();
                respuesta = true;
                }

            return Json(respuesta, JsonRequestBehavior.AllowGet);
            }


        public ActionResult PdfPrefacturacajaliq(int idorden)
            {
            var ot = context.tencabezaorden.Where(x => x.id == idorden).FirstOrDefault();
            int facturado = Convert.ToInt32(Session["user_usuarioid"]);
            var nombrefa = context.users.Where(x => x.user_id == facturado).Select(s => new { nombre = s.user_nombre + " " + s.user_apellido }).FirstOrDefault();
            var ciudad = context.nom_ciudad.Where(x => x.ciu_id == ot.icb_terceros.ciu_id).Select(x => x.ciu_nombre).FirstOrDefault();
            var empresa = context.tablaempresa.FirstOrDefault();

            PdfprefacLiq retiroop = new PdfprefacLiq
                {
                Aseguradoa = empresa.nombre_empresa,
                Direccion = empresa.direccion,
                Telefono = empresa.telefono,
                Documentocliente = empresa.nit,
                Ciudad = ciudad != null ? ciudad : "",
                Bodega = ot.bodega_concesionario.bodccs_nombre,
                Placa = ot.icb_vehiculo.plac_vh != null ? ot.icb_vehiculo.plac_vh : "",
                Vehiculo = ot.icb_vehiculo.modelo_vehiculo.modvh_nombre != null ? ot.icb_vehiculo.modelo_vehiculo.modvh_nombre : "",
                Serie = ot.icb_vehiculo.vin != null ? ot.icb_vehiculo.vin : "",
                Modelo = ot.icb_vehiculo.anio_vh.ToString() != null ? ot.icb_vehiculo.anio_vh.ToString() : "",
                Kilometraje = ot.kilometraje.ToString() != null ? ot.kilometraje.ToString() : "",
                OrdenT = ot.id.ToString(),
                Facturadopor = nombrefa.nombre.ToString(),
                Asesor = ot.users.user_nombre + " " + ot.users.user_apellido,
                Operaciones = context.tdetallemanoobraot.Where(x => x.idorden == idorden).ToList(),
                Repuestos = context.tdetallerepuestosot.Where(x => x.idorden == idorden).ToList()


                };

            var otplanes = context.tencabezaorden.Where(x => x.placa == ot.placa && x.tcitastaller.id_tipo_inspeccion != null).Select(q=>new { q.tcitastaller.id_tipo_inspeccion, numot = q.id, kilom = q.kilometraje}).ToList();

            var planes = context.tplanmantenimiento.Select(x => new { x.id, x.Descripcion }).ToList();

            var lista = planes.OrderBy(x => x.id).Select(d => new CsKilometraje
                {
                id = d.id,
                descripcion = d.Descripcion,
                Kilometraje = otplanes.Where(e => e.id_tipo_inspeccion == d.id).Select(f => f.kilom).FirstOrDefault().ToString(),
                numot = otplanes.Where(e => e.id_tipo_inspeccion == d.id).Select(f => f.numot).FirstOrDefault().ToString()
                }).ToList();

            retiroop.PlanMantenimiento = lista;

            int swclasifica = Convert.ToInt32(context.icb_sysparameter.Where(z => z.syspar_cod == "P150").Select(x => x.syspar_value).FirstOrDefault());

            var dicodigo = context.encab_documento.Where(x => x.orden_taller == idorden && x.tp_doc_registros.tp_doc_sw.sw == swclasifica).Select(s => new { codigo = s.tp_doc_registros.prefijo + "-" + s.numero }).FirstOrDefault();

            int Estadoprefac = Convert.ToInt32(context.icb_sysparameter.Where(z => z.syspar_cod == "P157").Select(x => x.syspar_value).FirstOrDefault());

            var encab_doc =  context.encab_documento.Where(x => x.orden_taller == idorden && x.tp_doc_registros.tp_doc_sw.sw == swclasifica).FirstOrDefault();
            if (encab_doc.feccreacionPrefac==null)
                {
                encab_doc.feccreacionPrefac = DateTime.Now;
                encab_doc.idestaddoprefac = Estadoprefac;
                } 
            context.Entry(encab_doc).State = EntityState.Modified;
            context.SaveChanges();
             
            string nombre = "PrefacturaCaja_";
            nombre = nombre + "file.pdf";
            string bodega = ot.bodega_concesionario.bodccs_nombre;
            string customSwitches = string.Format("--print-media-type --allow {0} --header-html {0} --header-spacing 5 --footer-html {1} --footer-spacing 0",
                    Url.Action("Cabeceraprefaccajaliq", "CentralAtencion", new { codigoentrada = dicodigo.codigo, aseguradoa = empresa.nombre_empresa, dir = empresa.direccion, tel = empresa.telefono, nit = empresa.nit }, Request.Url.Scheme), Url.Action("PiePDF", "ordenTaller", new { area = "" }, Request.Url.Scheme));

            ViewAsPdf something = new ViewAsPdf("PdfPrefacturacajaliq", retiroop)
                {
                PageOrientation = Orientation.Landscape,
                CustomSwitches = customSwitches,
                FileName = nombre,
                PageSize = Size.Letter,
                PageMargins = new Margins { Top = 40, Bottom = 5 }
                };



            return something;
            }

        public ActionResult Cabeceraprefaccajaliq(string codigoentrada,  string aseguradoa, string dir, string tel, string nit)
            {
            var recibido = Request;
            var fecha = DateTime.Now;
            var modelo2 = new PdfprefacLiq
                {
                CodigoRetiro = codigoentrada,
                Fechadocumento = fecha.ToString("yyyy-MM-dd"),             
                Aseguradoa = aseguradoa,
                Direccion = dir,
                Telefono = tel,
                Documentocliente = nit
                };

            return View(modelo2);
            }

        public JsonResult ActualizarMediosPago(int? cupo)
        {
            var listaFP = (from f in context.formas_pago
                           join b in context.bancos
                           on f.idbanco equals b.id into formabando
                           from fb in formabando.DefaultIfEmpty()
                           orderby fb.Descripcion
                           select new
                           {
                               f.id,
                               nombre = fb.Descripcion + " " + f.formapago + " (" + fb.numero_cuenta + ")"
                           }).ToList();
            //si la forma de pago seleccionada en el post es CONTADO
            if (cupo == 0)
            {
                listaFP = listaFP.Where(d => d.id != 7).ToList();
            }

            return Json(listaFP);
        }

        //public JsonResult 
        public class datatablex
        {
            public int id { get; set; }
            public string Columna0 { get; set; }
            public string Columna1 { get; set; }
            public string Columna2 { get; set; }
            public string Columna3 { get; set; }
            public string Columna4 { get; set; }
            public string Columna5 { get; set; }
            public string Columna6 { get; set; }
            public string Columna7 { get; set; }
            public string Columna8 { get; set; }
        }

        public class listaoperaciones
        {
            public int id { get; set; }
            public string codigo { get; set; }
            public string nombre { get; set; }
            public int idtecnico { get; set; }
            public string tecnico { get; set; }
            public int centro_costo { get; set; }
            public decimal tiempo { get; set; }
            public bool respuestainterna { get; set; }
            public decimal precio_unitario { get; set; }
            public string valorBaseiva { get; set; }
            public decimal valorBaseiva2 { get; set; }

            public string precio_string { get; set; }
            public decimal porcentaje_iva { get; set; }
            public string porcentaje_iva_string { get; set; }
            public decimal porcentaje_descuento { get; set; }
            public string porcentaje_descuento_string { get; set; }
            public decimal totaldescuento { get; set; }
            public string totaldescuento_string { get; set; }
            public decimal totaliva { get; set; }
            public string totaliva_string { get; set; }
            public int tipotarifa { get; set; }
            public string tarifa { get; set; }
            public string total_string { get; set; }
            public decimal total { get; set; }
            public SelectList tarifas { get; set; }
        }

        public class filtrosdatatable
        {
            public string nombre { get; set; }
            public string value { get; set; }
        }
        public class responsablesCaja
        {
            public int idresponsable { get; set; }
            public string responsable { get; set; }

        }
    }
}