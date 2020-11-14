using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
using Rotativa;
using Rotativa.Options;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;

using System.Web.Mvc;
using static Homer_MVC.Controllers.AlmacenController;

namespace Homer_MVC.Controllers
{
    public class ordenTallerController : Controller
        {
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");

        // GET: ordenTaller
        public ActionResult Create(int? id, int? menu)
            {
            tencabezaorden encab = new tencabezaorden();
            listasDesplegables(encab);
            BuscarFavoritos(menu);
            return View(encab);
            }

        private static Expression<Func<rprecios, decimal>> GetColumnName(string property)
            {
            ParameterExpression menu = Expression.Parameter(typeof(rprecios), "rprecios");
            MemberExpression menuProperty = Expression.PropertyOrField(menu, property);
            Expression<Func<rprecios, decimal>> lambda = Expression.Lambda<Func<rprecios, decimal>>(menuProperty, menu);

            return lambda;
            }

        public ActionResult buscartipodeproceso(string referencia, string precio)
            {
            decimal preciox = context.rprecios.Where(d => d.codigo == referencia).Select(GetColumnName(precio).Compile())
                .FirstOrDefault();
            return Json(preciox, JsonRequestBehavior.AllowGet);
            }

        [HttpPost]
        public ActionResult Create(tencabezaorden orden, int? menu)
            {
            if (Session["user_usuarioid"] != null)
                {
                string tecnico = Request["tecnico"];
                int error = 1;
                int idtecnicooperacion = 0;
                orden.asesor = Convert.ToInt32(Session["user_usuarioid"]);
                bool convertir = int.TryParse(tecnico, out int idtecnico);
                if (convertir == false)
                    {
                    error = 1;
                    }
                else
                    {
                    error = 0;
                    users existetecnico = context.users.Where(d => d.user_id == idtecnico).FirstOrDefault();
                    ttecnicos existetecnicoope = context.ttecnicos.Where(d => d.idusuario == idtecnico).FirstOrDefault();

                    if (existetecnico == null || existetecnicoope == null)
                        {
                        error = 1;
                        }
                    else
                        {
                        idtecnicooperacion = existetecnicoope.id;
                        }
                    }

                if (ModelState.IsValid && error == 0)
                    {
                    using (DbContextTransaction dbTran = context.Database.BeginTransaction())
                        {
                        try
                            {
                            long numeroConsecutivo = 1;
                            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                            /*ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
							var numeroConsecutivoAux = new icb_doc_consecutivos();
							var buscarTipoDocRegistro = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == orden.idtipodoc);
							numeroConsecutivoAux = gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro, orden.idtipodoc, bodegaActual);
	
							var buscarGrupoConsecutivos = context.grupoconsecutivos.FirstOrDefault(x => x.documento_id == orden.idtipodoc && x.bodega_id == bodegaActual);
							var numeroGrupo = buscarGrupoConsecutivos != null ? buscarGrupoConsecutivos.grupo : 0;
	
							if (numeroConsecutivoAux != null)
							{
								numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
							}
							else
							{
								TempData["mensaje_error"] = "No existe un numero consecutivo asignado para este tipo de documento";
								listasDesplegables(orden);
								BuscarFavoritos(menu);
								return View(orden);
							}*/

                            icb_consecutivo_ot buscatCodigo = context.icb_consecutivo_ot.Where(x => x.otcon_bodega == bodegaActual)
                                .FirstOrDefault();
                            string codigoIOT = "";
                            if (buscatCodigo != null)
                                {
                                // La vista trae la placa pero el campo lleva el la llave primaria de icv_vehiculo que es el plan mayor
                                string buscarVehiculo = (from vehiculo in context.icb_vehiculo
                                                         where vehiculo.plac_vh == orden.placa
                                                         select vehiculo.plan_mayor).FirstOrDefault();

                                if (!string.IsNullOrEmpty(buscarVehiculo))
                                    {
                                    orden.placa = buscarVehiculo;
                                    }

                                string consecutivonum = cerosconsecutivo(buscatCodigo.otcon_consecutivo.Value);
                                string anio = DateTime.Now.Year.ToString();
                                codigoIOT = anio.Substring(anio.Length - 2) + buscatCodigo.otcon_prefijo + "-" +
                                            consecutivonum;
                                orden.codigoentrada = codigoIOT;
                                orden.fecha = DateTime.Now;
                                orden.bodega = bodegaActual;
                                orden.numero = (int)numeroConsecutivo;
                                orden.idtecnico = idtecnico;
                                string minimoOrden = Request["minimoOrden"];
                                if (!string.IsNullOrEmpty(minimoOrden))
                                    {
                                    orden.minimo = Convert.ToDecimal(minimoOrden);
                                    }

                                string deducibleOrden = Request["deducibleOrden"];
                                if (!string.IsNullOrEmpty(deducibleOrden))
                                    {
                                    orden.deducible = (float)Convert.ToDecimal(deducibleOrden);
                                    }

                                int tiempoEstimado = Convert.ToInt32(Request["txtTiempoEstimado"]);
                                string horasDias = Request["rbHorasDias"];
                                DateTime fechaHoy = DateTime.Now;
                                if (horasDias == "dias")
                                    {
                                    fechaHoy = fechaHoy.AddDays(tiempoEstimado);
                                    }
                                else
                                    {
                                    fechaHoy = fechaHoy.AddHours(tiempoEstimado);
                                    }

                                bool convertirfecha = DateTime.TryParse(Request["txtEntregaEstimado"], out fechaHoy);
                                if (convertirfecha == false)
                                    {
                                    fechaHoy = DateTime.Now;
                                    }

                                orden.entrega = fechaHoy;

                                orden.fec_creacion = DateTime.Now;
                                orden.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                                orden.asesor = Convert.ToInt32(Session["user_usuarioid"]);
                                string citaSeleccionadaOPT = Request["citaSeleccionada"];
                                icb_sysparameter estadoOT = context.icb_sysparameter.Where(d => d.syspar_cod == "P78")
                                    .FirstOrDefault();
                                int estadodeOT = estadoOT != null ? Convert.ToInt32(estadoOT.syspar_value) : 16;
                                if (!string.IsNullOrWhiteSpace(citaSeleccionadaOPT))
                                    {
                                    int citaSeleccionada = Convert.ToInt32(Request["citaSeleccionada"]);
                                    orden.idcita = citaSeleccionada;

                                    tcitastaller buscarCita = context.tcitastaller.FirstOrDefault(x => x.id == citaSeleccionada);
                                    buscarCita.estadocita = estadodeOT;
                                    context.Entry(buscarCita).State = EntityState.Modified;
                                    }

                                //le cambio el estado a la cita (crear parametro de sistema)                                    
                                orden.estadoorden = estadodeOT;
                                context.tencabezaorden.Add(orden);
                                int guardarEncabezado = context.SaveChanges();

                                if (guardarEncabezado > 0)
                                    {
                                    //var buscarUltimoEncabezado = context.tencabezaorden.OrderByDescending(x => x.id).FirstOrDefault();
                                    tencabezaorden buscarUltimoEncabezado = context.tencabezaorden.Where(x => x.id == orden.id)
                                        .FirstOrDefault();

                                    if (buscarUltimoEncabezado != null)
                                        {
                                        int numeroSolicitudes = Convert.ToInt32(Request["numeroSolicitudes"]);
                                        for (int i = 0; i < numeroSolicitudes; i++)
                                            {
                                            string solicitud = Request["solicitud" + i];
                                            if (solicitud != null && solicitud != "")
                                                {
                                                context.tsolicitudorden.Add(new tsolicitudorden
                                                    {
                                                    idorden = buscarUltimoEncabezado.id,
                                                    bodega = buscarUltimoEncabezado.bodega,
                                                    usuariosolicitud = Convert.ToInt32(Session["user_usuarioid"]),
                                                    fecsolicitud = DateTime.Now,
                                                    solicitud = solicitud
                                                    });
                                                }
                                            }

                                        int numeroOperaciones = Convert.ToInt32(Request["numeroOperaciones"]);
                                        for (int i = 0; i < numeroOperaciones; i++)
                                            {
                                            string operacion = Request["codigoOperacion" + i];
                                            //var tipoTarifa = Request["tipoTarifa" + i];
                                            int tecnico2 = idtecnicooperacion;
                                            decimal descuento = !string.IsNullOrEmpty(Request["descuento" + i])
                                                ? Convert.ToDecimal(Request["descuento" + i])
                                                : 0;
                                            decimal ivaOperacion = !string.IsNullOrEmpty(Request["iva" + i])
                                                ? Convert.ToDecimal(Request["iva" + i])
                                                : 0;
                                            decimal tiempo = !string.IsNullOrEmpty(Request["tiempo" + i])
                                                ? Convert.ToDecimal(Request["tiempo" + i])
                                                : 0;
                                            decimal precio = !string.IsNullOrEmpty(Request["precio" + i])
                                                ? Convert.ToDecimal(Request["precio" + i])
                                                : 0;
                                            if (!string.IsNullOrEmpty(operacion) && tecnico2>0)
                                                {
                                                context.tdetallemanoobraot.Add(new tdetallemanoobraot
                                                    {
                                                    idorden = buscarUltimoEncabezado.id,
                                                    idtempario = operacion,
                                                    idtecnico = tecnico2,
                                                    tiempo = tiempo,
                                                    valorunitario = precio,
                                                    poriva = ivaOperacion,
                                                    //tipotarifa = Convert.ToInt32(tipoTarifa),
                                                    pordescuento = descuento,
                                                    fecha = DateTime.Now,
                                                    estado = "1"
                                                    });
                                                }
                                            }

                                        bool convertirn = int.TryParse(Request["numeroOperacionesPlan"],
                                            out int numeroOperacionesPlan);
                                        if (numeroOperacionesPlan > 0)
                                            {
                                            for (int i = 0;
                                                i < numeroOperacionesPlan;
                                                i++)
                                                {
                                                string estaoperacion = Request["ckOperacion" + i];
                                                string operacion = Request["txtCodigoOperacion" + i];
                                                //var tipoTarifa = Request["tipoTarifa" + i];
                                                int tecnico2 = idtecnicooperacion;
                                                int descuento = 0;
                                                decimal ivaOperacion = !string.IsNullOrEmpty(Request["txtPorcetajeIVA" + i])
                                                    ? Convert.ToDecimal(Request["txtPorcetajeIVA" + i])
                                                    : 0;
                                                decimal tiempo = !string.IsNullOrEmpty(Request["txtTiempoOperacion" + i])
                                                    ? Convert.ToDecimal(Request["txtTiempoOperacion" + i])
                                                    : 0;
                                                decimal precio = !string.IsNullOrEmpty(Request["txtPrecioMatriz" + i])
                                                    ? Convert.ToDecimal(Request["txtPrecioMatriz" + i])
                                                    : 0;
                                                bool solicitado = !string.IsNullOrEmpty(estaoperacion) ? true : false;

                                                if (!string.IsNullOrEmpty(operacion) && tecnico2>0 && solicitado)
                                                    {
                                                    context.tdetallemanoobraot.Add(new tdetallemanoobraot
                                                        {
                                                        idorden = buscarUltimoEncabezado.id,
                                                        idtempario = operacion,
                                                        idtecnico = tecnico2,
                                                        tiempo = tiempo,
                                                        valorunitario = precio,
                                                        poriva = ivaOperacion,
                                                        //tipotarifa = Convert.ToInt32(tipoTarifa),
                                                        pordescuento = descuento,
                                                        fecha = DateTime.Now,
                                                        id_plan_mantenimiento =
                                                            buscarUltimoEncabezado.id_plan_mantenimiento,
                                                        estado = "1"
                                                        });
                                                    }
                                                }
                                            }

                                        int numeroRepuestos = Convert.ToInt32(Request["numeroRepuestos"]);
                                        for (int i = 0; i < numeroRepuestos; i++)
                                            {
                                            //var tpTarifa = Convert.ToInt32(Request["tipoTarifaRpt" + i]);
                                            string codigoRepuesto = Request["codigoRepuesto" + i];
                                            string cantidadRepuesto = Request["cantidadRepuesto" + i];
                                            string precioRepuesto = Request["precioRepuesto" + i];
                                            string porIvaRepuesto = Request["porIvaRepuesto" + i];
                                            string porDsctRepuesto = Request["porDsctRepuesto" + i];
                                            string solicitarRepuesto = Request["solicitarRepuesto" + i];
                                            int CantidadFinal = !string.IsNullOrEmpty(Request["cantidadRepuesto" + i])
                                                ? Convert.ToInt32(Request["cantidadRepuesto" + i])
                                                : 0;
                                            decimal PrecioFinal = !string.IsNullOrEmpty(Request["precioRepuesto" + i])
                                                ? Convert.ToDecimal(Request["precioRepuesto" + i])
                                                : 0;
                                            decimal IvaFinal = !string.IsNullOrEmpty(Request["porIvaRepuesto" + i])
                                                ? Convert.ToDecimal(Request["porIvaRepuesto" + i])
                                                : 0;
                                            decimal DescuentoFinal = !string.IsNullOrEmpty(Request["porDsctRepuesto" + i])
                                                ? Convert.ToDecimal(Request["porDsctRepuesto" + i])
                                                : 0;

                                            /*
				                            if (tpTarifa == 1)
				                            {
				                                tercero = Convert.ToInt32(Request["docTerceroBuscado"]);
				                            }
				                            if (tpTarifa == 2)
				                            {
				                                var buscarNit = context.ttarifastaller.FirstOrDefault(x => x.tipotarifa == tpTarifa).idtercero;
				                                tercero = Convert.ToInt32(buscarNit);
				                            }
				                            if (tpTarifa == 3)
				                            {
				                                var buscarNit = context.ttarifastaller.FirstOrDefault(x => x.tipotarifa == tpTarifa).idtercero;
				                                tercero = Convert.ToInt32(buscarNit);
				                            }
				                            if (tpTarifa == 4)
				                            {
				                                tercero = Convert.ToInt32(Request["docTerceroBuscado"]);
				                            }
				                            if (tpTarifa == 5)
				                            {
				                                var idAseguradora = Convert.ToInt32(Request["aseguradora"]);
				                                var busquedaAS = context.icb_aseguradoras.FirstOrDefault(x => x.aseg_id == idAseguradora).idtercero;
				                                tercero = busquedaAS;
				                            }*/

                                            if (!string.IsNullOrEmpty(codigoRepuesto))
                                                {
                                                bool solicitado = !string.IsNullOrEmpty(solicitarRepuesto)
                                                    ? solicitarRepuesto == "on" ? true : false
                                                    : false;
                                                DateTime? fecha;
                                                if (solicitado)
                                                    {
                                                    fecha = DateTime.Now;
                                                    }
                                                else
                                                    {
                                                    fecha = null;
                                                    }

                                                context.tdetallerepuestosot.Add(new tdetallerepuestosot
                                                    {
                                                    idorden = buscarUltimoEncabezado.id,
                                                    idrepuesto = codigoRepuesto,
                                                    valorunitario = PrecioFinal,
                                                    pordescto = DescuentoFinal,
                                                    poriva = IvaFinal,
                                                    //tipotarifa = tpTarifa,
                                                    idtercero = buscarUltimoEncabezado.tercero,
                                                    cantidad = CantidadFinal,
                                                    solicitado = solicitado,
                                                    fecha_solicitud = fecha
                                                    });
                                                }
                                            }

                                        bool convertir2 = int.TryParse(Request["numeroRepuestosPlan"],
                                            out int numeroRepuestosPlan);
                                        if (numeroRepuestosPlan > 0)
                                            {
                                            for (int i = 0; i < numeroRepuestosPlan; i++)
                                                {
                                                string estaoperacion = Request["ckReferencia" + i];

                                                //var tpTarifa = Convert.ToInt32(Request["tipoTarifaRpt" + i]);
                                                string codigoRepuesto = Request["txtCodigoReferencia" + i];
                                                string cantidadRepuesto = Request["txtCantidadReferencia" + i];
                                                string precioRepuesto = Request["txtValorIvaReferencia" + i];
                                                string porIvaRepuesto = Request["txtPorcentajeIvaReferencia" + i];
                                                //var porDsctRepuesto = Request["porDsctRepuestoPlan" + i];
                                                string solicitarRepuesto = "on";
                                                int CantidadFinal =
                                                    !string.IsNullOrEmpty(Request["txtCantidadReferencia" + i])
                                                        ? Convert.ToInt32(Request["txtCantidadReferencia" + i])
                                                        : 0;
                                                decimal PrecioFinal =
                                                    !string.IsNullOrEmpty(Request["txtValorTotalReferencia" + i])
                                                        ? Convert.ToDecimal(Request["txtValorTotalReferencia" + i])
                                                        : 0;
                                                decimal IvaFinal =
                                                    !string.IsNullOrEmpty(Request["txtPorcentajeIvaReferencia" + i])
                                                        ? Convert.ToDecimal(Request["txtPorcentajeIvaReferencia" + i])
                                                        : 0;
                                                bool solicitado = !string.IsNullOrEmpty(estaoperacion) ? true : false;

                                                //var DescuentoFinal = !string.IsNullOrEmpty(Request["porDsctRepuestoPlan" + i]) ? Convert.ToDecimal(Request["porDsctRepuestoPlan" + i]) : 0;
                                                decimal DescuentoFinal = 0;

                                                /*
				                                if (tpTarifa == 1)
				                                {
				                                    tercero = Convert.ToInt32(Request["docTerceroBuscado"]);
				                                }
				                                if (tpTarifa == 2)
				                                {
				                                    var buscarNit = context.ttarifastaller.FirstOrDefault(x => x.tipotarifa == tpTarifa).idtercero;
				                                    tercero = Convert.ToInt32(buscarNit);
				                                }
				                                if (tpTarifa == 3)
				                                {
				                                    var buscarNit = context.ttarifastaller.FirstOrDefault(x => x.tipotarifa == tpTarifa).idtercero;
				                                    tercero = Convert.ToInt32(buscarNit);
				                                }
				                                if (tpTarifa == 4)
				                                {
				                                    tercero = Convert.ToInt32(Request["docTerceroBuscado"]);
				                                }
				                                if (tpTarifa == 5)
				                                {
				                                    var idAseguradora = Convert.ToInt32(Request["aseguradora"]);
				                                    var busquedaAS = context.icb_aseguradoras.FirstOrDefault(x => x.aseg_id == idAseguradora).idtercero;
				                                    tercero = busquedaAS;
				                                }*/
                                                bool solicitado2 = !string.IsNullOrEmpty(solicitarRepuesto)
                                                    ? true
                                                    : false;
                                                int tecnico2 = idtecnicooperacion;

                                                if (!string.IsNullOrEmpty(codigoRepuesto) && tecnico2>0 &&
                                                    solicitado)
                                                    {
                                                    DateTime? fecha;
                                                    if (solicitado)
                                                        {
                                                        fecha = DateTime.Now;
                                                        }
                                                    else
                                                        {
                                                        fecha = null;
                                                        }

                                                    context.tdetallerepuestosot.Add(new tdetallerepuestosot
                                                        {
                                                        idorden = buscarUltimoEncabezado.id,
                                                        idrepuesto = codigoRepuesto,
                                                        valorunitario = PrecioFinal,
                                                        pordescto = DescuentoFinal,
                                                        poriva = IvaFinal,
                                                        //tipotarifa = tpTarifa,
                                                        idtercero = buscarUltimoEncabezado.tercero,
                                                        cantidad = CantidadFinal,
                                                        solicitado = solicitado,
                                                        fecha_solicitud = fecha,
                                                        id_plan_mantenimiento =
                                                            buscarUltimoEncabezado.id_plan_mantenimiento
                                                        });
                                                    }
                                                }
                                            }

                                        context.SaveChanges();
                                        //actualizo el consecutivo

                                        //le aumento el consecutivo en 1 al consecutivo de orden
                                        buscatCodigo.otcon_consecutivo = buscatCodigo.otcon_consecutivo + 1;
                                        context.Entry(buscatCodigo).State = EntityState.Modified;
                                        context.SaveChanges();
                                        }

                                    /*if (buscarGrupoConsecutivos != null)
				                    {
				                        var numerosConsecutivos = context.icb_doc_consecutivos.Where(x => x.doccons_grupoconsecutivo == numeroGrupo).ToList();
				                        foreach (var item in numerosConsecutivos)
				                        {
				                            item.doccons_siguiente = item.doccons_siguiente + 1;
				                            context.Entry(item).State = EntityState.Modified;
				                        }
				                        context.SaveChanges();
				                    }*/
                                    TempData["mensaje"] = codigoIOT;
                                    // TempData["mensaje"] = numeroConsecutivo;
                                    dbTran.Commit();
                                    return RedirectToAction("Update", new { orden.id, menu });
                                    }

                                TempData["mensaje_error"] =
                                    "La orden no se ha guardado por favor verifique su conexion...";
                                }
                            else
                                {
                                TempData["mensaje_error"] =
                                    "La orden no se ha guardado por que no se ha configurado un consecutivo para esta OT";
                                }
                            }
                        catch (DbEntityValidationException)
                            {
                            dbTran.Rollback();
                            throw;
                            }
                        }
                    }
                else
                    {
                    ViewBag.txtEntregaEstimado = DateTime.Now.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));
                    }

                listasDesplegables(orden);
                BuscarFavoritos(menu);
                return View();
                }

            return RedirectToAction("Login", "Login");
            }

        public JsonResult EditarRepuesto(int? repuesto)
            {
            if (repuesto != null)
                {
                //busco el repuesto
                tdetallerepuestosot repuestox = context.tdetallerepuestosot.Where(d => d.id == repuesto).FirstOrDefault();
                if (repuestox != null)
                    {
                    var buscarCliente = (from vehiculo in context.icb_vehiculo
                                         join tercero in context.icb_terceros
                                             on vehiculo.propietario equals tercero.tercero_id
                                         join cliente in context.tercero_cliente
                                             on tercero.tercero_id equals cliente.tercero_id
                                         where vehiculo.plac_vh == repuestox.tencabezaorden.icb_vehiculo.plac_vh ||
                                               vehiculo.plan_mayor == repuestox.tencabezaorden.icb_vehiculo.plac_vh
                                         select new
                                             {
                                             cliente.lprecios_repuestos,
                                             cliente.dscto_rep
                                             }).FirstOrDefault();

                    decimal valorDescuento = Math.Round(repuestox.valorunitario *
                                                    (Convert.ToDecimal(buscarCliente.dscto_rep) > repuestox.pordescto &&
                                                     Convert.ToDecimal(buscarCliente.dscto_rep) <=
                                                     Convert.ToDecimal(repuestox.icb_referencia.por_dscto_max)
                                                        ?
                                                        Convert.ToDecimal(buscarCliente.dscto_rep)
                                                        :
                                                        Convert.ToDecimal(buscarCliente.dscto_rep) <
                                                        repuestox.pordescto &&
                                                        repuestox.pordescto <=
                                                        Convert.ToDecimal(repuestox.icb_referencia.por_dscto_max)
                                                            ? repuestox.pordescto
                                                            : 0) / 100 * repuestox.cantidad);
                    decimal valorIva = Math.Round((repuestox.cantidad * repuestox.valorunitario - Math.Round(
                                                   repuestox.valorunitario *
                                                   (Convert.ToDecimal(buscarCliente.dscto_rep) > repuestox.pordescto &&
                                                    Convert.ToDecimal(buscarCliente.dscto_rep) <=
                                                    Convert.ToDecimal(repuestox.icb_referencia.por_dscto_max)
                                                       ?
                                                       Convert.ToDecimal(buscarCliente.dscto_rep)
                                                       :
                                                       Convert.ToDecimal(buscarCliente.dscto_rep) <
                                                       repuestox.pordescto &&
                                                       repuestox.pordescto <=
                                                       Convert.ToDecimal(repuestox.icb_referencia.por_dscto_max)
                                                           ? repuestox.pordescto
                                                           : 0) / 100 * repuestox.cantidad)) * repuestox.poriva / 100);
                    decimal valorTotal = Math.Round(repuestox.valorunitario * repuestox.cantidad - valorDescuento
                                                + valorIva);


                    /* var valorDescuento = Math.Round(((repuestox.valorunitario * (decimal)(repuestox.pordescto) / 100) * repuestox.cantidad)),
					     var valorIva = (Math.Round((repuestox.valorunitario * (decimal)repuestox.poriva / 100),
					     var valorTotal = Math.Round(repuestox.valorunitario * repuestox.cantidad - (Math.Round(((repuestox.valorunitario * (decimal)(repuestox.pordescto) / 100) * repuestox.cantidad)))
					             + (Math.Round((((Math.Round(((repuestox.valorunitario * (decimal)(repuestox.pordescto) / 100) * repuestox.cantidad))) * (decimal)repuestox.poriva / 100)))),*/
                    var data = new
                        {
                        porcentajeIva = repuestox.poriva,
                        porcentajeDescuento = repuestox.pordescto,
                        precio = repuestox.valorunitario,
                        repuestox.cantidad,

                        codigo = repuestox.icb_referencia.ref_codigo,
                        nombreReferencia = repuestox.icb_referencia.ref_descripcion,
                        repuestox.id,
                        valorDescuento,
                        valorIva,
                        valorTotal
                        };
                    return Json(data);
                    }

                return Json(0);
                }

            return Json(0);
            }

        public JsonResult buscarAverias(int? ot)
            {
            tencabezaorden buscarOT = context.tencabezaorden.Where(x => x.id == ot).FirstOrDefault();
            if (buscarOT != null)
                {
                long tallerOrden = context.tallerAveria.Where(x => x.nombre.Contains("orden")).FirstOrDefault().id;
                long estadoAveria = context.estadoAverias.Where(x => x.nombre.Contains("asignada")).FirstOrDefault().id;
                var buscar = context.icb_inspeccionvehiculos.Where(x => x.idcitataller == buscarOT.idcita && x.insp_solicitar == true && x.estado_averia_id == estadoAveria && x.taller_averia_id == tallerOrden && x.planmayor == buscarOT.placa)
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
            return Json(false, JsonRequestBehavior.AllowGet);
            }


        public JsonResult GuardarEdicionRepuesto(int? idrepuesto, int? cantidad)
            {
            if (idrepuesto == null || cantidad == null)
                {
                return Json(0);
                }

            //busco el repuesto
            tdetallerepuestosot repuestox = context.tdetallerepuestosot.Where(d => d.id == idrepuesto).FirstOrDefault();
            if (repuestox == null)
                {
                return Json(0);
                }

            repuestox.cantidad = cantidad.Value;
            context.Entry(repuestox).State = EntityState.Modified;
            int guardar = context.SaveChanges();
            if (guardar > 0)
                {
                return Json(1);
                }

            return Json(0);
            }

        public JsonResult agregarSolicitud(int? idorden, string solicitud)
            {
            int valor = 0;
            string respuestastring = "";
            if (Session["user_usuarioid"] != null)
                {
                if (idorden != null && !string.IsNullOrWhiteSpace(solicitud))
                    {
                    tencabezaorden existeorden = context.tencabezaorden.Where(d => d.id == idorden).FirstOrDefault();
                    if (existeorden != null)
                        {
                        //nueva solicitud
                        tsolicitudorden soli = new tsolicitudorden
                            {
                            bodega = existeorden.bodega,
                            fecsolicitud = DateTime.Now,
                            idorden = existeorden.id,
                            usuariosolicitud = Convert.ToInt32(Session["user_usuarioid"]),
                            solicitud = solicitud
                            };
                        context.tsolicitudorden.Add(soli);
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                            {
                            valor = 1;
                            }
                        else
                            {
                            respuestastring = "Error en guardado";
                            }
                        }
                    else
                        {
                        respuestastring = "Debe especificar un id de solicitud válido";
                        }
                    }
                else
                    {
                    respuestastring = "Debe especificar un id de solicitud y una solicitud";
                    }
                }
            else
                {
                respuestastring = "la sesión ha finalizado";
                }

            var data = new
                {
                valor,
                respuestastring
                };
            return Json(data);
            }

        public JsonResult actualizarSolicitud(int? idsolicitud, string solicitud, string respuesta)
            {
            int valor = 0;
            string respuestastring = "";
            if (Session["user_usuarioid"] != null)
                {
                if (idsolicitud != null && !string.IsNullOrWhiteSpace(solicitud))
                    {
                    tsolicitudorden existesolicitud = context.tsolicitudorden.Where(d => d.idsolicitud == idsolicitud)
                        .FirstOrDefault();
                    if (existesolicitud != null)
                        {
                        existesolicitud.solicitud = solicitud;
                        if (!string.IsNullOrWhiteSpace(respuesta))
                            {
                            existesolicitud.respuesta = respuesta;
                            existesolicitud.fecrespuesta = DateTime.Now;
                            existesolicitud.usuariorespuesta = Convert.ToInt32(Session["user_usuarioid"]);
                            }

                        context.Entry(existesolicitud).State = EntityState.Modified;
                        int guardar = context.SaveChanges();
                        if (guardar > 0)
                            {
                            valor = 1;
                            }
                        else
                            {
                            respuestastring = "Error en guardado";
                            }
                        }
                    else
                        {
                        respuestastring = "Debe especificar un id de solicitud válido";
                        }
                    }
                else
                    {
                    respuestastring = "Debe especificar un id de solicitud y una solicitud";
                    }
                }
            else
                {
                respuestastring = "la sesión ha finalizado";
                }

            var data = new
                {
                valor,
                respuestastring
                };
            return Json(data);
            }

        public JsonResult BuscarSolicitudesDeAgentaTaller(int id_cita)
            {
            var buscarSintomas = (from sintomas in context.tcitasintomas
                                  where sintomas.idcita == id_cita
                                  select new
                                      {
                                      sintomas.id,
                                      sintomas.sintomas
                                      }).ToList();

            var buscarOperacionSQL = (from operacion in context.tcitasoperacion
                                      join tempario in context.ttempario
                                          on operacion.operacion equals tempario.codigo
                                      join cita in context.tcitastaller
                                          on operacion.idcita equals cita.id
                                      join tipoTarifa in context.ttipostarifa
                                          on cita.tipotarifa equals tipoTarifa.id
                                      join operario in context.ttecnicos
                                          on cita.tecnico equals operario.id
                                      join usuario in context.users
                                          on operario.idusuario equals usuario.user_id
                                      join codigoIva in context.codigo_iva
                                          on tempario.iva equals codigoIva.id
                                      join centro in context.centro_costo
                                          on operacion.idcentro equals centro.centcst_id into ps
                                      from centro in ps.DefaultIfEmpty()
                                      where operacion.idcita == id_cita
                                      select new
                                          {
                                          tempario.codigo,
                                          tempario.operacion,
                                          tiempo = tempario.tiempo != null ? tempario.tiempo : 0,
                                          tempario.precio,
                                          tecnico = usuario.user_nombre + " " + usuario.user_apellido,
                                          tecnico_id = operario.id,
                                          iva = codigoIva.porcentaje,
                                          tipoTariID = tipoTarifa.id,
                                          tipoTarifa.Descripcion,
                                          operacion.idcentro,
                                          centro.centcst_nombre,
                                          operacion.inspeccion,
                                          tempario.preciomatriz,
                                          tempario.tiempomatriz
                                          }).ToList();

            var buscarDatosCita = (from cita in context.tcitastaller
                                   join tercero in context.icb_terceros
                                       on cita.cliente equals tercero.tercero_id
                                   join cliente in context.tercero_cliente
                                       on tercero.tercero_id equals cliente.tercero_id
                                   join bahia in context.tbahias
                                       on cita.bahia equals bahia.id
                                   join bodega in context.bodega_concesionario
                                       on bahia.bodega equals bodega.id
                                   where cita.id == id_cita
                                   select new
                                       {
                                       cliente.cltercero_id,
                                       bodega.id,
                                       cita.tipotarifa,
                                       cita.placa
                                       }).FirstOrDefault();

            var buscarCliente = (from vehiculo in context.icb_vehiculo
                                 join tercero in context.icb_terceros
                                     on vehiculo.propietario equals tercero.tercero_id
                                 join cliente in context.tercero_cliente
                                     on tercero.tercero_id equals cliente.tercero_id
                                 where vehiculo.plac_vh == buscarDatosCita.placa
                                 select new
                                     {
                                     cliente.lprecios_repuestos,
                                     cliente.dscto_rep
                                     }).FirstOrDefault();

            ttarifastaller buscarTarifaClienteBodega = (from tarifaCliente in context.ttarifastaller
                                                              where tarifaCliente.idtercero == buscarDatosCita.cltercero_id
                                                              select tarifaCliente).FirstOrDefault();

            ttarifastaller buscarTarifaBodega = (from tarifaTaller in context.ttarifastaller
                                                 where tarifaTaller.bodega == buscarDatosCita.id && tarifaTaller.tipotarifa == buscarDatosCita.tipotarifa
                                                 select tarifaTaller).FirstOrDefault();

            var buscarOperaciones = buscarOperacionSQL.Select(x => new
                {
                x.codigo,
                x.operacion,
                x.tiempo,
                x.tecnico,
                x.tecnico_id,
                x.tipoTariID,
                tarifa = x.Descripcion,
                x.idcentro,
                centcst_nombre = x.centcst_nombre != null ? x.centcst_nombre : "",
                //precio = x.inspeccion ? x.preciomatriz
                    //: x.precio != null ? x.precio
                    //: buscarTarifaClienteBodega != null ? buscarTarifaClienteBodega.valor
                    //: buscarTarifaBodega != null ? buscarTarifaBodega.valorhora : 0,
                ivaOperacion = x.iva != null ? x.iva : 0,
                //valorIva = x.inspeccion
                //    ? x.preciomatriz * x.iva / 100
                //    : x.precio != null
                //        ? x.precio * x.iva / 100 * (decimal)x.tiempo
                //        : buscarTarifaClienteBodega != null
                //            ? buscarTarifaClienteBodega.valor * x.iva / 100 * (decimal)x.tiempo
                //            : buscarTarifaBodega != null
                //                ? buscarTarifaBodega.valorhora * x.iva / 100 * (decimal)x.tiempo
                //                : 0,
                //valorTotal = (x.inspeccion
                //                 ? x.preciomatriz
                //                 : x.precio != null
                //                     ? x.precio * (decimal)x.tiempo
                //                     : buscarTarifaClienteBodega != null
                //                         ? (decimal)x.tiempo * buscarTarifaClienteBodega.valor
                //                         : buscarTarifaBodega != null
                //                             ? buscarTarifaBodega.valorhora * (decimal)x.tiempo
                //                             : 0)
                             //+
                             //(x.inspeccion
                             //    ? x.preciomatriz * x.iva / 100
                             //    : x.precio != null
                             //        ? x.precio * x.iva / 100 * (decimal)x.tiempo
                             //        : buscarTarifaClienteBodega != null
                             //            ? buscarTarifaClienteBodega.valor * x.iva / 100 * (decimal)x.tiempo
                             //            : buscarTarifaBodega != null
                             //                ? buscarTarifaBodega.valorhora * x.iva / 100 * (decimal)x.tiempo
                             //                : 0)
                });

            string buscarPrecioCliente = !string.IsNullOrEmpty(buscarCliente.lprecios_repuestos)
                ? buscarCliente.lprecios_repuestos
                : "precio1";
            var buscarRepuestosAgendados = (from repuestos in context.tcitarepuestos
                                            join referencia in context.icb_referencia
                                                on repuestos.idrepuesto equals referencia.ref_codigo
                                            join precios in context.rprecios
                                                on referencia.ref_codigo equals precios.codigo into ps2
                                            from precios in ps2.DefaultIfEmpty()
                                            join cita in context.tcitastaller
                                                on repuestos.idcita equals cita.id
                                            join tipoTarifa in context.ttipostarifa
                                                on cita.tipotarifa equals tipoTarifa.id
                                            join planMantenimiento in context.tplanmantenimiento
                                                on cita.id_tipo_inspeccion equals planMantenimiento.id
                                            join plan in context.tplanrepuestosmodelo
                                                on planMantenimiento.id equals plan.inspeccion
                                            where cita.id == id_cita && plan.tipo == "R"
                                            select new
                                                {
                                                referencia.ref_codigo,
                                                referencia.ref_descripcion,
                                                cantidad = plan.cantidad != null ? plan.cantidad : 0,
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
                                                referencia.por_dscto,
                                                referencia.por_dscto_max,
                                                tipoTariID = tipoTarifa.id,
                                                tipoTarifa.Descripcion
                                                }).Distinct().ToList();

            var repuestosAgendados = buscarRepuestosAgendados.Select(x => new
                {
                x.ref_codigo,
                x.ref_descripcion,
                x.precio,
                x.por_iva,
                x.cantidad,
                x.tipoTariID,
                x.Descripcion,
                valorIva = Math.Round(x.precio * (decimal)x.por_iva / 100),
                valorTotal = Math.Round((x.precio + x.precio * (decimal)x.por_iva / 100) * x.cantidad ?? 0),
                por_dscto = buscarCliente.dscto_rep > x.por_dscto && buscarCliente.dscto_rep <= x.por_dscto_max
                    ?
                    buscarCliente.dscto_rep
                    :
                    buscarCliente.dscto_rep < x.por_dscto && x.por_dscto <= x.por_dscto_max
                        ? x.por_dscto
                        : 0,
                valor_dscto = Math.Round(
                    x.precio * (decimal)(buscarCliente.dscto_rep > x.por_dscto &&
                                          buscarCliente.dscto_rep <= x.por_dscto_max ? buscarCliente.dscto_rep :
                        buscarCliente.dscto_rep < x.por_dscto && x.por_dscto <= x.por_dscto_max ? x.por_dscto : 0) /
                    100)
                });

            var buscarKilometraje = (from tei in context.tencabinspeccion
                                     where tei.idcita == id_cita
                                     select new
                                         {
                                         tei.id,
                                         tei.kilometraje
                                         }).FirstOrDefault();
            if (buscarKilometraje == null)
                {
                return Json(
                    new { buscarKilometraje, buscarSintomas, buscarOperaciones, repuestosAgendados, posicion = 4 },
                    JsonRequestBehavior.AllowGet);
                }

            var tareasInspeccion = (from t in context.ttareainspeccionprioridad
                                    join ti in context.ttareasinspeccion
                                        on t.idtareainspeccion equals ti.id
                                    where t.idencabinspeccion == buscarKilometraje.id
                                    select new
                                        {
                                        t.id,
                                        ti.descripcion,
                                        t.cantidad,
                                        t.valor_unitario,
                                        fecautorizacion = t.fecautorizacion.Value.ToString(),
                                        t.autorizado
                                        }).ToList();

            return Json(
                new
                    {
                    buscarKilometraje,
                    tareasInspeccion,
                    buscarSintomas,
                    buscarOperaciones,
                    repuestosAgendados,
                    posicion = 5
                    }, JsonRequestBehavior.AllowGet);
            }

        public JsonResult BuscarDescuentoOperacion(int tarifa, string operacion, int tercero)
            {
            float info = (from tc in context.tercero_cliente
                          where tc.tercero_id == tercero
                          select tc.dscto_mo).FirstOrDefault();

            return Json(info, JsonRequestBehavior.AllowGet);
            }

        public JsonResult BuscarNitAseguradora(int aseguradora)
            {
            int nitAseguradora = context.icb_aseguradoras.FirstOrDefault(x => x.aseg_id == aseguradora).idtercero;

            return Json(nitAseguradora, JsonRequestBehavior.AllowGet);
            }

        /*
        public JsonResult BuscarTiempoTempario(string codigoOperacion, int id_cliente, int tipo_tarifa, int tecnico, int? descuento)
        {
            var bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            var existeTarifa = true;
            var mensajeExisteTarifa = "";
            decimal? valorCalculado = 0;
            decimal? ivaCalculado = 0;
            decimal? descuentoCalculado = 0;

            if (descuento == null)
            {
                descuento = 0;
            }

            var idTecnico = (from tempario in context.ttecnicos
                             where tempario.idusuario == tecnico
                             select tempario.id).FirstOrDefault();

            var buscarTiempo = (from tempario in context.ttempario
                                join codigoIva in context.codigo_iva
                                on tempario.iva equals codigoIva.id
                                where tempario.codigo == codigoOperacionttarifastaller
                                select new
                                {
                                    tiempo = tempario.tiempo != null ? tempario.tiempo : 0,
                                    precio = tempario.precio,
                                    iva = codigoIva.porcentaje
                                }).FirstOrDefault();

            var buscarTarifaClienteBodega = (from tarifaCliente in context.ttarifastaller
                                             where tarifaCliente.idtercero == id_cliente
                                             select tarifaCliente).FirstOrDefault();

            var buscarTarifaBodega = (from tarifaTaller in context.ttarifastaller
                                      where tarifaTaller.bodega == bodegaActual && tarifaTaller.tipotarifa == tipo_tarifa
                                      select tarifaTaller).FirstOrDefault();

            if (buscarTarifaClienteBodega == null && buscarTarifaBodega == null)
            {
                existeTarifa = false;
                mensajeExisteTarifa = "La bodega no tiene tarifa asignada";
            }

            if (buscarTiempo.tiempo == 0)
            {
                descuentoCalculado = buscarTiempo.precio != null ? ((buscarTiempo.precio * descuento) / 100) * 1 :
                                     buscarTarifaClienteBodega != null ? ((buscarTarifaClienteBodega.valor) * descuento / 100) * 1 :
                                     buscarTarifaBodega != null ? ((buscarTarifaBodega.valorhora * descuento) / 100) * 1 : 0;

                ivaCalculado = buscarTiempo.precio != null ? (((buscarTiempo.precio * 1) - descuentoCalculado) * buscarTiempo.iva / 100) :
                               buscarTarifaClienteBodega != null ? (((buscarTarifaClienteBodega.valor * 1) - descuentoCalculado) * buscarTiempo.iva / 100) :
                               buscarTarifaBodega != null ? (((buscarTarifaBodega.valorhora * 1) - descuentoCalculado) * buscarTiempo.iva / 100) : 0;

                valorCalculado = (buscarTiempo.precio != null ? buscarTiempo.precio * 1 - descuentoCalculado + ivaCalculado :
                                 buscarTarifaClienteBodega != null ? 1 * buscarTarifaClienteBodega.valor - descuentoCalculado + ivaCalculado :
                                 buscarTarifaBodega != null ? buscarTarifaBodega.valorhora * 1 - descuentoCalculado + ivaCalculado : 0);

            }
            else
            {
                descuentoCalculado = buscarTiempo.precio != null ? ((buscarTiempo.precio * descuento) / 100) * (decimal)buscarTiempo.tiempo :
                                     buscarTarifaClienteBodega != null ? ((buscarTarifaClienteBodega.valor) * descuento / 100) * (decimal)buscarTiempo.tiempo :
                                     buscarTarifaBodega != null ? ((buscarTarifaBodega.valorhora * descuento) / 100) * (decimal)buscarTiempo.tiempo : 0;

                ivaCalculado = buscarTiempo.precio != null ? (((buscarTiempo.precio * (decimal)buscarTiempo.tiempo) - descuentoCalculado) * buscarTiempo.iva / 100) :
                               buscarTarifaClienteBodega != null ? (((buscarTarifaClienteBodega.valor * (decimal)buscarTiempo.tiempo) - descuentoCalculado) * buscarTiempo.iva / 100) :
                               buscarTarifaBodega != null ? (((buscarTarifaBodega.valorhora * (decimal)buscarTiempo.tiempo) - descuentoCalculado) * buscarTiempo.iva / 100) : 0;

                valorCalculado = buscarTiempo.precio != null ? buscarTiempo.precio * (decimal)buscarTiempo.tiempo - descuentoCalculado + ivaCalculado :
                                 buscarTarifaClienteBodega != null ? (decimal)buscarTiempo.tiempo * buscarTarifaClienteBodega.valor - descuentoCalculado + ivaCalculado :
                                 buscarTarifaBodega != null ? buscarTarifaBodega.valorhora * (decimal)buscarTiempo.tiempo - descuentoCalculado + ivaCalculado : 0;
            }

            var buscarOperacion = new
            {
                buscarTiempo.tiempo,
                precio = buscarTiempo.precio != null ? buscarTiempo.precio :
                         buscarTarifaClienteBodega != null ? buscarTarifaClienteBodega.valor :
                         buscarTarifaBodega != null ? buscarTarifaBodega.valorhora : 0,

                ivaOperacion = buscarTiempo.iva != null ? buscarTiempo.iva : 0,
                valorDescuento = descuentoCalculado,
                valorIva = ivaCalculado,
                valorTotal = valorCalculado,
                existeTarifa,
                idTecnico,
                mensajeExisteTarifa
            };
            return Json(buscarOperacion, JsonRequestBehavior.AllowGet);
        }*/


        public JsonResult BuscarTiempoTempario(string codigoOperacion, int id_cliente, int tecnico)
            {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            bool existeTarifa = true;
            string mensajeExisteTarifa = "";
            decimal? valorCalculado = 0;
            decimal? ivaCalculado = 0;
            decimal? descuentoCalculado = 0;


            int idTecnico = (from tempario in context.ttecnicos
                             where tempario.idusuario == tecnico
                             select tempario.id).FirstOrDefault();

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

            ttarifastaller buscarTarifaClienteBodega = (from tarifaCliente in context.ttarifastaller
                                                              where tarifaCliente.idtercero == id_cliente
                                                              select tarifaCliente).FirstOrDefault();

            if (buscarTiempo.tiempo == 0)
                {
                ivaCalculado = buscarTiempo.precio != null
                    ?
                    (buscarTiempo.precio * 1 - descuentoCalculado) * buscarTiempo.iva / 100
                    :
                    buscarTarifaClienteBodega != null
                        ? (buscarTarifaClienteBodega.valorhora * 1 - descuentoCalculado) * buscarTiempo.iva / 100
                        : 0;

                valorCalculado = buscarTiempo.precio != null
                    ?
                    buscarTiempo.precio * 1 - descuentoCalculado + ivaCalculado
                    :
                    buscarTarifaClienteBodega != null
                        ? 1 * buscarTarifaClienteBodega.valorhora - descuentoCalculado + ivaCalculado
                        : 0;
                }
            else
                {
                ivaCalculado = buscarTiempo.precio != null
                    ?
                    (buscarTiempo.precio * (decimal)buscarTiempo.tiempo - descuentoCalculado) * buscarTiempo.iva / 100
                    :
                    buscarTarifaClienteBodega != null
                        ? (buscarTarifaClienteBodega.valorhora * (decimal)buscarTiempo.tiempo - descuentoCalculado) *
                          buscarTiempo.iva / 100
                        : 0;

                valorCalculado = buscarTiempo.precio != null
                    ?
                    buscarTiempo.precio * (decimal)buscarTiempo.tiempo - descuentoCalculado + ivaCalculado
                    :
                    buscarTarifaClienteBodega != null
                        ? (decimal)buscarTiempo.tiempo * buscarTarifaClienteBodega.valorhora - descuentoCalculado +
                          ivaCalculado
                        : 0;
                }

            var buscarOperacion = new
                {
                buscarTiempo.tiempo,
                precio = buscarTiempo.precio != null ? buscarTiempo.precio :
                    buscarTarifaClienteBodega != null ? buscarTarifaClienteBodega.valorhora : 0,

                ivaOperacion = buscarTiempo.iva != null ? buscarTiempo.iva : 0,
                valorDescuento = descuentoCalculado,
                valorIva = ivaCalculado,
                valorTotal = valorCalculado,
                existeTarifa,
                idTecnico,
                mensajeExisteTarifa
                };
            return Json(buscarOperacion, JsonRequestBehavior.AllowGet);
            }

        public JsonResult agregarGarantia(string id)
            {

            var result = 0;
            icb_vehiculo v = new icb_vehiculo();

            var state = context.icb_vehiculo.Find(id);

            state.garantia = true;

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
        public JsonResult BuscarTiempoTemparioOT(int? idorden, string codigoOperacion, int id_cliente, int tecnico, string porcentajeDescuento = "0")
            {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
            bool existeTarifa = true;
            string mensajeExisteTarifa = "";
            decimal? valorCalculado = 0;
            decimal? ivaCalculado = 0;
            decimal? descuentoCalculado = 0;
            if (idorden != null)
                {
                tencabezaorden ordentrabajo = context.tencabezaorden.Where(d => d.id == idorden).FirstOrDefault();
                if (ordentrabajo != null)
                    {
                    int idTecnico = (from tempario in context.ttecnicos
                                     where tempario.idusuario == tecnico
                                     select tempario.id).FirstOrDefault();

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

                    ttarifastaller buscarTarifaClienteBodega = (from tarifaCliente in context.ttarifastaller
                                                                      where tarifaCliente.idtercero == id_cliente
                                                                      select tarifaCliente).FirstOrDefault();

                    if (buscarTiempo.tiempo == 0)
                        {
                        ivaCalculado = buscarTiempo.precio != null
                            ?
                            (buscarTiempo.precio * 1 - descuentoCalculado) * buscarTiempo.iva / 100
                            :
                            buscarTarifaClienteBodega != null
                                ? (buscarTarifaClienteBodega.valorhora * 1 - descuentoCalculado) * buscarTiempo.iva / 100
                                : 0;

                        valorCalculado = buscarTiempo.precio != null
                            ?
                            buscarTiempo.precio * 1 - descuentoCalculado + ivaCalculado
                            :
                            buscarTarifaClienteBodega != null
                                ? 1 * buscarTarifaClienteBodega.valorhora - descuentoCalculado + ivaCalculado
                                : 0;
                        }
                    else
                        {
                        ivaCalculado = buscarTiempo.precio != null
                            ?
                            (buscarTiempo.precio * (decimal)buscarTiempo.tiempo - descuentoCalculado) *
                            buscarTiempo.iva / 100
                            :
                            buscarTarifaClienteBodega != null
                                ? (buscarTarifaClienteBodega.valorhora * (decimal)buscarTiempo.tiempo -
                                   descuentoCalculado) * buscarTiempo.iva / 100
                                : 0;

                        valorCalculado = buscarTiempo.precio != null
                            ?
                            buscarTiempo.precio * (decimal)buscarTiempo.tiempo - descuentoCalculado + ivaCalculado
                            :
                            buscarTarifaClienteBodega != null
                                ? (decimal)buscarTiempo.tiempo * buscarTarifaClienteBodega.valorhora - descuentoCalculado +
                                  ivaCalculado
                                : 0;
                        }

                    //registro la operacion para dicha ot
                    tdetallemanoobraot operacionOt = new tdetallemanoobraot
                        {
                        costopromedio = 0,
                        fecha = DateTime.Now,
                        idorden = ordentrabajo.id,
                        idtecnico = idTecnico,
                        idtempario = codigoOperacion,
                        pordescuento = descuentoCalculado,
                        poriva = buscarTiempo.iva,
                        tiempo = Convert.ToDecimal(buscarTiempo.tiempo),
                        valorunitario = buscarTiempo.precio != null ? Convert.ToDecimal(buscarTiempo.precio) : 0
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
                           // precio = buscarTiempo.precio != null ? buscarTiempo.precio :
                         //       buscarTarifaClienteBodega != null ? buscarTarifaClienteBodega.valor : 0,

                            ivaOperacion = buscarTiempo.iva != null ? buscarTiempo.iva : 0,
                            valorDescuento = descuentoCalculado,
                            valorIva = ivaCalculado,
                            valorTotal = valorCalculado,
                            existeTarifa,
                            idTecnico,
                            mensajeExisteTarifa
                            };
                        return Json(buscarOperacion, JsonRequestBehavior.AllowGet);
                        }

                    return Json(0);
                    }

                return Json(0);
                }

            return Json(0);
            }


        public JsonResult BuscarCitasPorPlaca(string placa)
            {
            int bodegaActual = Convert.ToInt32(Session["user_bodega"]);

            var buscarCitasSQL = (from citas in context.tcitastaller
                                  join bahia in context.tbahias
                                      on citas.bahia equals bahia.id
                                  join bodega in context.bodega_concesionario
                                      on bahia.bodega equals bodega.id
                                  join tipoInspeccion in context.tplanmantenimiento
                                      on citas.id_tipo_inspeccion equals tipoInspeccion.id into ps
                                  from tipoInspeccion in ps.DefaultIfEmpty()
                                  where citas.placa == placa && citas.estadocita != 8 && citas.estadocita != 9 &&
                                        citas.estadocita != 10 && citas.estadocita != 11 && bodega.id == bodegaActual
                                  select new
                                      {
                                      citas.id,
                                      tipoInspeccion.Descripcion,
                                      citas.desde,
                                      citas.notas
                                      }).ToList();

            var buscarCitas = buscarCitasSQL.Select(x => new
                {
                x.id,
                Descripcion = x.Descripcion != null ? x.Descripcion : "",
                desde = x.desde.ToShortDateString(),
                horaDesde = x.desde.ToShortTimeString(),
                x.notas
                });

            return Json(buscarCitas, JsonRequestBehavior.AllowGet);
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

        public ActionResult Update(int? id, int? menu)
            {
            if (id == null)
                {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

            tencabezaorden orden = context.tencabezaorden.Find(id);
            if (orden == null)
                {
                TempData["mensaje_error"] = "La Orden de Trabajo no Existe";
                return RedirectToAction("Create", "ordenTaller", new { menu });
                }


            EncabezadoOTModel orden2 = new EncabezadoOTModel
                {
                id = orden.id,
                placa = orden.placa,
                entrega = orden.entrega.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")),
                aseguradora = orden.aseguradora,
                asesor = orden.users.user_nombre + " " + orden.users.user_apellido,
                bodega = orden.bodega,
                modelo = orden.icb_vehiculo.modelo_vehiculo.modvh_nombre,
                añovh = orden.icb_vehiculo.anio_vh,
                centrocosto = orden.centrocosto,
                codigoentrada = orden.codigoentrada,
                deducible = orden.deducible != null
                    ? orden.deducible.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "",
                minimo = orden.minimo != null ? orden.minimo.Value.ToString("N0", new CultureInfo("is-IS")) : "",
                fecha_soat = orden.fecha_soat != null
                    ? orden.fecha_soat.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : "",
                estadoorden = orden.estadoorden,
                domicilo = orden.domicilo,
                estado = orden.estado,
                garantia_causa = orden.garantia_causa,
                garantia_falla = orden.garantia_falla,
                garantia_solucion = orden.garantia_solucion,
                idcita = orden.idcita,
               
                idtipodoc = orden.idtipodoc,
                kilometraje_nuevo = orden.icb_vehiculo.kilometraje_nuevo,
                kilometraje = orden.icb_vehiculo.kilometraje,
                notas = orden.notas,
                numero = orden.numero,
                numero_soat = orden.numero_soat,
                poliza = orden.poliza,
                razoningreso = orden.razoningreso,
                razon_inactivo = orden.razon_inactivo,
                razon_secundaria = orden.razon_secundaria,
                siniestro = orden.siniestro,
                tercero = orden.tercero,
                tipooperacion = orden.tipooperacion,
                tecnico = orden.idtecnico,
                txtDocumentoCliente = orden.icb_terceros.doc_tercero,
                };

            var buscarVehiculo = (from vehiculo in context.icb_vehiculo
                                  where vehiculo.plan_mayor == orden.placa || vehiculo.plan_mayor == orden.placa
                                  select new { vehiculo.plac_vh, vehiculo.plan_mayor }).FirstOrDefault();

            var buscarContacto = (from contacto in context.icb_contacto_tercero
                                  join tercero in context.icb_terceros on contacto.tercero_id equals tercero.tercero_id
                                  where tercero.tercero_id == orden.tercero
                                  select new { contacto.con_tercero_nombre, contacto.con_tercero_telefono }).FirstOrDefault();

            orden.placa = !string.IsNullOrWhiteSpace(buscarVehiculo.plac_vh)
                ? buscarVehiculo.plac_vh
                : buscarVehiculo.plan_mayor;
            ViewBag.entrega = orden.entrega;
            ViewBag.tercero = orden.tercero;
            ViewBag.placa1 = orden.placa;

            var contacto1 = buscarContacto != null ? buscarContacto.con_tercero_nombre : "";
            var numero = buscarContacto != null ? buscarContacto.con_tercero_telefono : "";

            ViewBag.contacto = contacto1;
            ViewBag.telcontacto = numero;

            ViewBag.citaId = context.tencabezaorden.FirstOrDefault(x => x.id == id).idcita;

            listasDesplegables2(orden2);
            BuscarFavoritos(menu);
            var buscarContactos = (from contacto in context.icb_contacto_tercero
                                   join tipoContacto in context.tipocontactotercero
                                       on contacto.tipocontacto equals tipoContacto.idtipocontacto
                                   where contacto.tercero_id == orden.tercero
                                   select new
                                       {
                                       contacto.con_tercero_nombre,
                                       contacto.con_tercero_id
                                       }).ToList();
            ViewBag.recibidode =
                new SelectList(buscarContactos, "con_tercero_id", "con_tercero_nombre", orden.recibidode);

            ViewBag.insumo =
              new SelectList(context.Tinsumo.Select(x=>new { x.id, descripcion= x.codigo+ " | "+x.descripcion}), "id", "descripcion");
          
            ViewBag.tarifainsumo =
            new SelectList(context.ttipostarifa.Select(x=> new { x.Descripcion, x.id}), "id", "descripcion");

          
            icb_sysparameter idplanmparameter = context.icb_sysparameter.Where(d => d.syspar_cod == "P156").FirstOrDefault();
            int idplanm = idplanmparameter != null ? Convert.ToInt32(idplanmparameter.syspar_value) : 10;
           
            
            ViewBag.comboot = new SelectList(context.ttempario. Where(x=> x.tipooperacion== idplanm).Select(x => new { x.operacion, x.codigo }), "codigo", "operacion");

         
            ViewBag.OTcreada = 1;
            ViewBag.ejecucion = 0;
            ViewBag.liquidacion = 0;
            int rol = Convert.ToInt32(Session["user_rolid"]);

            var permisos = (from acceso in context.rolacceso
                            join rolPerm in context.rolpermisos
                            on acceso.idpermiso equals rolPerm.id
                            where acceso.idrol == rol //&& rolPerm.codigo == "P40" 
                            select new { rolPerm.codigo }).ToList();

            var resultado1 = permisos.Where(x => x.codigo == "P47").Count() > 0 ? "Si" : "No";
            var resultado2 = permisos.Where(x => x.codigo == "P45").Count() > 0 ? "Si" : "No";
            ViewBag.Permiso = resultado1;
            ViewBag.Permisomodif = resultado2;

            tcitasestados estadox = context.tcitasestados.Where(d => d.id == orden.estadoorden).FirstOrDefault();
            ViewBag.tipoEstado = estadox.tipoestado;






            //if (orden.estadoorden==)
            return View(orden2);
            }

        [HttpPost]
        public ActionResult Update(EncabezadoOTModel orden2, int? menu)
            {
            if (ModelState.IsValid)
                {
                tencabezaorden orden = context.tencabezaorden.Where(d => d.id == orden2.id).FirstOrDefault();


                orden.placa = orden2.placa;
                if (!string.IsNullOrWhiteSpace(orden2.entrega))
                    {
                    orden.entrega = Convert.ToDateTime(orden2.entrega);
                    }

                orden.aseguradora = orden2.aseguradora;
                //if (orden2.asesor != null)
                //{
                //    orden.asesor = orden2.asesor.Value;
                //}
                if (orden2.asesor != null)
                    {
                    orden.bodega = orden2.bodega.Value;
                    }

                orden.centrocosto = orden2.centrocosto;
                orden.codigoentrada = orden2.codigoentrada;
                if (!string.IsNullOrWhiteSpace(orden2.deducible))
                    {
                    orden.deducible = Convert.ToInt64(Convert.ToDecimal(orden2.deducible));
                    }

                if (!string.IsNullOrWhiteSpace(orden2.minimo))
                    {
                    orden.minimo = Convert.ToInt64(Convert.ToDecimal(orden2.minimo));
                    }

                if (!string.IsNullOrWhiteSpace(orden2.fecha_soat))
                    {
                    orden.fecha_soat = Convert.ToDateTime(orden2.fecha_soat);
                    }

                orden.estadoorden = orden2.estadoorden;
                orden.domicilo = orden2.domicilo;
                orden.estado = orden2.estado;
                orden.garantia_causa = orden2.garantia_causa;
                orden.garantia_falla = orden2.garantia_falla;
                orden.garantia_solucion = orden2.garantia_solucion;
                orden.idcita = orden2.idcita;
                if (orden2.idtipodoc != null)
                    {
                    orden.idtipodoc = orden2.idtipodoc.Value;
                    }

                orden.kilometraje = orden2.kilometraje;
                orden.notas = orden2.notas;
                if (orden2.numero != null)
                    {
                    orden.numero = orden2.numero.Value;
                    }

                orden.numero_soat = orden2.numero_soat;
                orden.poliza = orden2.poliza;
                orden.razoningreso = orden2.razoningreso;
                orden.razon_inactivo = orden2.razon_inactivo;
                orden.razon_secundaria = orden2.razon_secundaria;
                orden.siniestro = orden2.siniestro;
                if (orden2.tercero != null)
                    {
                    orden.tercero = orden2.tercero.Value;
                    }

                orden.tipooperacion = orden2.tipooperacion;
                if (orden2.tecnico != null)
                    {
                    orden.idtecnico = orden2.tecnico.Value;
                    }

                // La vista trae la placa pero el campo lleva el la llave primaria de icv_vehiculo que es el plan mayor
                string buscarVehiculo = (from vehiculo in context.icb_vehiculo
                                         where vehiculo.plac_vh == orden.placa
                                         select vehiculo.plan_mayor).FirstOrDefault();

                if (!string.IsNullOrEmpty(buscarVehiculo))
                    {
                    orden.placa = buscarVehiculo;
                    }

                string tecnico = Request["tecnico"];
                if (!string.IsNullOrEmpty(tecnico))
                    {
                    orden.idtecnico = Convert.ToInt32(tecnico);
                    }
                else
                    {
                    tencabezaorden orden23 = context.tencabezaorden.Where(d => d.id == orden.id).FirstOrDefault();
                    orden.idtecnico = orden23.idtecnico;
                    }
                int rol = Convert.ToInt32(Session["user_rolid"]);

                var permisos = (from acceso in context.rolacceso
                                join rolPerm in context.rolpermisos
                                on acceso.idpermiso equals rolPerm.id
                                where acceso.idrol == rol //&& rolPerm.codigo == "P40" 
                                select new { rolPerm.codigo }).ToList();

                var resultado1 = permisos.Where(x => x.codigo == "P47").Count() > 0 ? "Si" : "No";
                ViewBag.Permiso = resultado1;

                tcitasestados estadox = context.tcitasestados.Where(d => d.id == orden.estadoorden).FirstOrDefault();
      
                orden.fec_actualizacion = DateTime.Now;
                orden.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                context.Entry(orden).State = EntityState.Modified;
                int guardarEncabezado = 0;
                try
                    {
                    guardarEncabezado = context.SaveChanges();
                    }
                catch (Exception)
                    {
                    //throw;
                    }

                if (guardarEncabezado > 0)
                    {
                    //mostrar pestañas de cambio de estado
                    /*
					var numeroSolicitudes = Convert.ToInt32(Request["numeroSolicitudes"]);
					for (var i = 0; i < numeroSolicitudes; i++)
					{
						var id_solicitud = Request["idsolicitud" + i];
						if (!string.IsNullOrEmpty(id_solicitud))
						{
							var solicitud = Convert.ToInt32(Request["idsolicitud" + i]);
							var buscarSolicitud = context.tsolicitudorden.FirstOrDefault(x => x.idsolicitud == solicitud);
							buscarSolicitud.usuariorespuesta = Convert.ToInt32(Session["user_usuarioid"]);
							buscarSolicitud.fecrespuesta = DateTime.Now;
							buscarSolicitud.respuesta = Request["respuesta" + i];
							context.Entry(buscarSolicitud).State = EntityState.Modified;
						}
						else
						{
							var solicitudTexto = Request["solicitud" + i];
							if (solicitudTexto != null && solicitudTexto != "")
							{
								context.tsolicitudorden.Add(new tsolicitudorden()
								{
									idorden = orden.id,
									bodega = orden.bodega,
									usuariosolicitud = Convert.ToInt32(Session["user_usuarioid"]),
									fecsolicitud = DateTime.Now,
									solicitud = solicitudTexto,
									usuariorespuesta = Convert.ToInt32(Session["user_usuarioid"]),
									fecrespuesta = DateTime.Now,
									respuesta = Request["respuesta" + i]
								});
							}
						}
					}

					var cantidadOperaciones = Request["numeroOperaciones"];
					if (cantidadOperaciones != "")
					{
						var numeroOperaciones = Convert.ToInt32(Request["numeroOperaciones"]);
						for (var i = 0; i < numeroOperaciones; i++)
						{
							var codigoOperacion = Request["codigoOperacion" + i];
							var tipoTarifa = Request["tipoTarifa" + i];
							var idTecnico = Request["id_tecnico" + i];
							var tiempo = Request["tiempo" + i];
							var valorUni = Request["precio" + i];
							var porDsct = Request["descuento" + i];
							var porIVA = Request["ivaOperacion" + i];

							if (!string.IsNullOrEmpty(codigoOperacion))
							{
								var fechaHoy = DateTime.Now;
								var existe = context.tdetallemanoobraot.FirstOrDefault(x => x.idorden == orden.id && x.idtempario == codigoOperacion && x.fecha != fechaHoy);
								if (existe == null)
								{
									context.tdetallemanoobraot.Add(new tdetallemanoobraot()
									{
										idorden = orden.id,
										idtempario = codigoOperacion,
										idtecnico = Convert.ToInt32(idTecnico),
										tiempo = Convert.ToDecimal(tiempo),
										valorunitario = Convert.ToDecimal(valorUni),
										tipotarifa = Convert.ToInt32(tipoTarifa),
										pordescuento = Convert.ToDecimal(porDsct),
										poriva = Convert.ToDecimal(porIVA),
										fecha = fechaHoy
									});
								}
							}
						}
					}

					var cantidadRepuestos = Request["numeroRepuestos"];
					if (cantidadRepuestos != "")
					{
						var numeroRepuestos = Convert.ToInt32(Request["numeroRepuestos"]);
						for (var i = 0; i < numeroRepuestos; i++)
						{
							var tercero = 0;
							var tpTarifa = Convert.ToInt32(Request["tipoTarifaRpt" + i]);
							var codigoRepuesto = Request["codigoRepuesto" + i];
							var cantidadRepuesto = Request["cantidadRepuesto" + i];
							var precioRepuesto = Request["precioRepuesto" + i];
							var porIvaRepuesto = Request["porIvaRepuesto" + i];
							var porDsctRepuesto = Request["porDsctRepuesto" + i];
							var solicitarRepuesto = Request["solicitarRepuesto" + i];
							var idRepuestoDetalle = Request["idRepuesto" + i];
							var CantidadFinal = !string.IsNullOrEmpty(Request["cantidadRepuesto" + i]) ? Convert.ToInt32(Request["cantidadRepuesto" + i]) : 0;
							var PrecioFinal = !string.IsNullOrEmpty(Request["precioRepuesto" + i]) ? Convert.ToDecimal(Request["precioRepuesto" + i]) : 0;
							var IvaFinal = !string.IsNullOrEmpty(Request["porIvaRepuesto" + i]) ? Convert.ToDecimal(Request["porIvaRepuesto" + i]) : 0;
							var DescuentoFinal = !string.IsNullOrEmpty(Request["porDsctRepuesto" + i]) ? Convert.ToDecimal(Request["porDsctRepuesto" + i]) : 0;
							if (!string.IsNullOrEmpty(codigoRepuesto))
							{
								if (tpTarifa == 1)
								{
									tercero = Convert.ToInt32(Request["docTerceroBuscado"]);
								}
								if (tpTarifa == 2)
								{
									var buscarNit = context.ttarifastaller.FirstOrDefault(x => x.tipotarifa == tpTarifa).idtercero;
									tercero = Convert.ToInt32(buscarNit);
								}
								if (tpTarifa == 3)
								{
									var buscarNit = context.ttarifastaller.FirstOrDefault(x => x.tipotarifa == tpTarifa).idtercero;
									tercero = Convert.ToInt32(buscarNit);
								}
								if (tpTarifa == 4)
								{
									tercero = Convert.ToInt32(Request["docTerceroBuscado"]);
								}
								if (tpTarifa == 5)
								{
									var idAseguradora = Convert.ToInt32(Request["aseguradora"]);
									var busquedaAS = context.icb_aseguradoras.FirstOrDefault(x => x.aseg_id == idAseguradora).idtercero;
									tercero = busquedaAS;
								}

								var solicitado = !string.IsNullOrEmpty(solicitarRepuesto) ? solicitarRepuesto == "on" ? true : false : false;
								DateTime? fecha;
								if (solicitado)
								{
									fecha = DateTime.Now;
								}
								else
								{
									fecha = null;
								}
								var fechaHoy = DateTime.Now;

								if (idRepuestoDetalle == "undefined")
								{

								}
								else
								{
									context.tdetallerepuestosot.Add(new tdetallerepuestosot()
									{
										idorden = orden.id,
										idrepuesto = codigoRepuesto,
										valorunitario = PrecioFinal,
										pordescto = DescuentoFinal,
										poriva = IvaFinal,
										cantidad = CantidadFinal,
										solicitado = solicitado,
										fecha_solicitud = fecha,
										idtercero = tercero,
										tipotarifa = tpTarifa
									});
								}
							}
						}
					}*/

                    context.SaveChanges();
                    TempData["mensaje"] = orden.codigoentrada;
                    }
                else
                    {
                    TempData["mensaje_error"] = "La orden no se ha guardado por favor verifique su conexion...";
                    }
                }
            else
                {
                List<ModelErrorCollection> errors = ModelState.Select(x => x.Value.Errors)
                    .Where(y => y.Count > 0)
                    .ToList();
                }

            ViewBag.tercero = orden2.tercero;

            ViewBag.citaId = context.tencabezaorden.FirstOrDefault(x => x.id == orden2.id).idcita;
            ViewBag.entrega = orden2.entrega;
            listasDesplegables2(orden2);
            BuscarFavoritos(menu);
            var buscarContactos = (from contacto in context.icb_contacto_tercero
                                   join tipoContacto in context.tipocontactotercero
                                       on contacto.tipocontacto equals tipoContacto.idtipocontacto
                                   where contacto.tercero_id == orden2.tercero
                                   select new
                                       {
                                       contacto.con_tercero_nombre,
                                       contacto.con_tercero_id
                                       }).ToList();
            ViewBag.recibidode =
                new SelectList(buscarContactos, "con_tercero_id", "con_tercero_nombre", orden2.recibidode);



            ViewBag.insumo =
              new SelectList(context.Tinsumo.Select(x => new { x.id, descripcion = x.codigo + " | " + x.descripcion }), "id", "descripcion");

            ViewBag.tarifainsumo =
            new SelectList(context.ttipostarifa.Select(x => new { x.Descripcion, x.id }), "id", "descripcion");

            icb_sysparameter idplanmparameter = context.icb_sysparameter.Where(d => d.syspar_cod == "P156").FirstOrDefault();
            int idplanm = idplanmparameter != null ? Convert.ToInt32(idplanmparameter.syspar_value) : 8;



            ViewBag.comboot = new SelectList(context.ttempario.Where(x => x.tipooperacion == idplanm).Select(x => new { x.operacion, x.codigo }), "codigo", "operacion");


            return View(orden2);
            }

        public JsonResult SolicitarRepuestos(int idOrden, string repuesto, int cantidad, int precio, int? detalle, string estadoot = "")
            {
            tsolicitudrepuestosot existe =
                context.tsolicitudrepuestosot.FirstOrDefault(x => x.iddetalle == detalle && x.idorden == idOrden);
            if (existe == null || estadoot == "E")
                {
                //busco la ot
                tencabezaorden ordent = context.tencabezaorden.Where(d => d.id == idOrden).FirstOrDefault();
                if (ordent.fecha_solicitud_repuestos == null)
                    {
                    ordent.fecha_solicitud_repuestos = DateTime.Now;
                    context.Entry(ordent).State = EntityState.Modified;
                    context.SaveChanges();
                    }

                //estado OT creada
                icb_sysparameter estadoOT = context.icb_sysparameter.Where(d => d.syspar_cod == "P78").FirstOrDefault();
                int estadodeOT = estadoOT != null ? Convert.ToInt32(estadoOT.syspar_value) : 16;
                //estado OT solicitud Repuestos
                icb_sysparameter estadoOT1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P83").FirstOrDefault();
                int estadodeOT2 = estadoOT1 != null ? Convert.ToInt32(estadoOT1.syspar_value) : 17;

                int estado_sol = context.testado_solicitud_repuesto.Where(s => s.codigo == "ST").Select(d => d.id).FirstOrDefault();

                tsolicitudrepuestosot crearSolicitud = new tsolicitudrepuestosot
                    {
                    idorden = idOrden,
                    idrepuesto = repuesto,
                    cantidad = cantidad,
                    valor = precio,
                    solicitado = true,
                    iddetalle = detalle,
                    estado_solicitud = estado_sol,
                    fecsolicitud = DateTime.Now
                    };

                context.tsolicitudrepuestosot.Add(crearSolicitud);
                tdetallerepuestosot repo = context.tdetallerepuestosot.Where(d => d.id == detalle).FirstOrDefault();
                if (repo != null)
                    {
                    repo.solicitado = true;
                    repo.fecha_solicitud = DateTime.Now;
                    context.Entry(repo).State = EntityState.Modified;
                    }

                if (ordent.estadoorden == estadodeOT)
                    {
                    ordent.estadoorden = estadodeOT2;

                    ordent.fecha_solicitud_repuestos = DateTime.Now;
                    context.Entry(ordent).State = EntityState.Modified;
                    if (ordent.idcita != null)
                        {
                        tcitastaller cita = context.tcitastaller.Where(d => d.id == ordent.idcita).FirstOrDefault();
                        if (cita != null)
                            {
                            cita.estadocita = estadodeOT2;
                            context.Entry(cita).State = EntityState.Modified;
                            }
                        }
                    }

                int ok = context.SaveChanges();
                return Json(new { error = false, ok }, JsonRequestBehavior.AllowGet);
                }

            return Json(new { error = true, mensaje = "" }, JsonRequestBehavior.AllowGet);
            }

        public JsonResult BuscarSolicitudesPorOrden(int? id_orden)
            {
            var buscarSolicitudes = (from solicitud in context.tsolicitudorden
                                     where solicitud.idorden == id_orden
                                     select new
                                         {
                                         solicitud.idsolicitud,
                                         solicitud.solicitud,
                                         respuesta = solicitud.respuesta != null ? solicitud.respuesta : ""
                                         }).ToList();
            return Json(buscarSolicitudes, JsonRequestBehavior.AllowGet);
            }

        public JsonResult BuscarOrdenesDeTaller()
            {
            var buscarOrdenes = (from encabezadoOrden in context.tencabezaorden
                                 join tercero in context.icb_terceros
                                     on encabezadoOrden.tercero equals tercero.tercero_id
                                 join vehiculo in context.icb_vehiculo
                                     on encabezadoOrden.placa equals vehiculo.plan_mayor
                                 join modelo in context.modelo_vehiculo
                                     on vehiculo.modvh_id equals modelo.modvh_codigo
                                 select new
                                     {
                                     encabezadoOrden.numero,
                                     encabezadoOrden.id,
                                     encabezadoOrden.codigoentrada,
                                     tercero.doc_tercero,
                                     nombreTercero = tercero.razon_social + "" + tercero.prinom_tercero + " " + tercero.segnom_tercero +
                                                     " " + tercero.apellido_tercero + " " + tercero.segapellido_tercero,
                                     vehiculo.plac_vh,
                                     modelo.modvh_nombre,
                                     encabezadoOrden.fecha,
                                     estado = encabezadoOrden.tcitasestados.Descripcion,
                                     colorEstado = encabezadoOrden.tcitasestados.color_estado,
                                     }).ToList();

            var data = buscarOrdenes.Select(x => new
                {
                x.numero,
                x.id,
                x.codigoentrada,
                x.doc_tercero,
                x.nombreTercero,
                x.plac_vh,
                x.modvh_nombre,
                x.estado,
                x.colorEstado,
                fecha = x.fecha.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                });

            return Json(data, JsonRequestBehavior.AllowGet);
            }

        public JsonResult BuscarDatosPorIdOrden(int id_orden)
            {
            tencabezaorden ordendatos = context.tencabezaorden.Where(d => d.id == id_orden).FirstOrDefault();
            var buscarSolicitudes = (from solicitud in context.tsolicitudorden
                                     where solicitud.idorden == id_orden
                                     select new
                                         {
                                         solicitud.solicitud,
                                         solicitud.respuesta
                                         }).ToList();

            var buscarDatosOrden = (from cita in context.tencabezaorden
                                    join tercero in context.icb_terceros
                                        on cita.tercero equals tercero.tercero_id
                                    join cliente in context.tercero_cliente
                                        on tercero.tercero_id equals cliente.tercero_id
                                    join bodega in context.bodega_concesionario
                                        on cita.bodega equals bodega.id
                                    where cita.id == id_orden
                                    select new
                                        {
                                        cliente.cltercero_id,
                                        bodega.id,
                                        cita.placa
                                        }).FirstOrDefault();

            var buscarOperacionesSQL = (from detalle in context.tdetallemanoobraot
                                        join tempario in context.ttempario
                                            on detalle.idtempario equals tempario.codigo
                                        /*join tipoTarifa in context.ttipostarifa
                                        on detalle.tipotarifa equals tipoTarifa.id*/
                                        join operario in context.ttecnicos
                                            on detalle.idtecnico equals operario.id
                                        join usuario in context.users
                                            on operario.idusuario equals usuario.user_id
                                        where detalle.idorden == id_orden
                                        select new
                                            {
                                            detalle.id,
                                            tempario.codigo,
                                            tempario.operacion,
                                            /*tipoTarifa.id,
                                            tipoTarifa.Descripcion,*/
                                            tecnico = usuario.user_nombre + " " + usuario.user_apellido,
                                            tecnico_id = operario.id,
                                            tiempo = detalle.tiempo != null ? detalle.tiempo : 0,
                                            precio = detalle.valorunitario,
                                            detalle.pordescuento,
                                            detalle.poriva,
                                            detalle.estado,
                                            detalle.fecha_envio_sms,
                                            detalle.respuesta_sms,
                                            detalle.tipotarifa
                                            }).ToList();

            //var buscarCliente = (from vehiculo in context.icb_vehiculo
            //                     join tercero in context.icb_terceros
            //                     on vehiculo.propietario equals tercero.tercero_id
            //                     join cliente in context.tercero_cliente
            //                     on tercero.tercero_id equals cliente.tercero_id into ps2
            //                     from cliente in ps2.DefaultIfEmpty()
            //                     where vehiculo.plac_vh == buscarDatosOrden.placa || vehiculo.plan_mayor == buscarDatosOrden.placa
            //                     select new
            //                     {
            //                         lprecios_repuestos=ps2.Count()>0?cliente.lprecios_repuestos:"0",
            //                         dscto_rep= ps2.Count()>0 ? cliente.dscto_rep:0,
            //                     }).FirstOrDefault();
            var predicadovehi = PredicateBuilder.True<icb_vehiculo>();
            if (!string.IsNullOrWhiteSpace(ordendatos.icb_vehiculo.plac_vh))
                {
                predicadovehi = predicadovehi.And(d => d.plac_vh == ordendatos.icb_vehiculo.plac_vh);
                }
            else if (!string.IsNullOrWhiteSpace(ordendatos.icb_vehiculo.plan_mayor))
                {
                predicadovehi = predicadovehi.And(d => d.plan_mayor == ordendatos.icb_vehiculo.plan_mayor);
                }
            int? propietario = context.icb_vehiculo
                .Where(predicadovehi).Select(d => d.propietario)
                .FirstOrDefault();
            if (propietario == null)
                {
                propietario = ordendatos.tercero;
                }
            var buscarCliente = context.tercero_cliente.Where(d => d.tercero_id == propietario)
                .Select(d => new { d.lprecios_repuestos, d.dscto_rep }).FirstOrDefault();

            ttarifastaller buscarTarifaClienteBodega = (from tarifaCliente in context.ttarifastaller
                                                              where tarifaCliente.idtercero == ordendatos.tercero
                                                              select tarifaCliente).FirstOrDefault();

            decimal valoroperaciones = 0;
            decimal valorrepuestos = 0;
            var buscarOperaciones = buscarOperacionesSQL.Select(x => new
                {
                idoperacion = x.id,
                x.codigo,
                x.operacion,
                x.tiempo,
                x.tecnico,
                x.tecnico_id,
                /*id_tarifa = x.id,
				tarifa = x.Descripcion,*/
                //precio = x.precio != null ? x.precio : 0,
                precio = x.precio,

                ivaOperacion = x.poriva != null ? x.poriva : 0,

                descuentoOperacion = x.pordescuento != null ? x.pordescuento : 0,

                descuento = x.tiempo > 0
                    ? x.precio * x.pordescuento / 100 * (decimal)x.tiempo
                    : x.precio * x.pordescuento / 100 * 1,

                valorIva = x.tiempo != 0
                    ? (x.precio * (decimal)x.tiempo - x.precio * x.pordescuento / 100 * (decimal)x.tiempo) *
                      x.poriva / 100
                    : (x.precio * 1 - x.precio * x.pordescuento / 100 * 1) * x.poriva / 100,

                valorTotal = x.tiempo != 0
                    ? x.precio * (decimal)x.tiempo - x.precio * x.pordescuento / 100 * (decimal)x.tiempo +
                      (x.precio * (decimal)x.tiempo - x.precio * x.pordescuento / 100 * (decimal)x.tiempo) *
                      x.poriva / 100
                    : x.precio * 1 - x.precio * x.pordescuento / 100 * 1 +
                      (x.precio * 1 - x.precio * x.pordescuento / 100 * 1) * x.poriva / 100,
                estado = !string.IsNullOrWhiteSpace(x.estado) ? x.estado : "-1",
                fecha_envio_sms = x.fecha_envio_sms != null
                    ? x.fecha_envio_sms.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                respuesta = x.respuesta_sms != null ? x.respuesta_sms == true ? 1 : 0 : -1,
                boton_sms = x.id,
                fechasms = x.fecha_envio_sms,
                estadorepuesta = x.estado,
                x.tipotarifa

                });

            foreach (var item in buscarOperaciones)
                {
                valoroperaciones = valoroperaciones + (item.valorTotal != null ? item.valorTotal.Value : 0);
                }

            string buscarPrecioCliente = buscarCliente != null
                ? !string.IsNullOrEmpty(buscarCliente.lprecios_repuestos) ? buscarCliente.lprecios_repuestos : "precio1"
                : "precio1";

            var buscarRepuestosSQL = (from repuestos in context.tdetallerepuestosot
                                      join referencia in context.icb_referencia
                                          on repuestos.idrepuesto equals referencia.ref_codigo
                                      join precios in context.rprecios
                                          on referencia.ref_codigo equals precios.codigo into ps2
                                      from precios in ps2.DefaultIfEmpty()
                                      join solrespues in context.tsolicitudrepuestosot
                                      on new { repuestos.idorden, repuestos.idrepuesto } equals new { solrespues.idorden, solrespues.idrepuesto }
                                      into solestados
                                      from solrespues in solestados.DefaultIfEmpty()
                                      join estsoli in context.testado_solicitud_repuesto
                                      on solrespues.estado_solicitud equals estsoli.id into est
                                      from estsoli in est.DefaultIfEmpty()

                                          /*join orden in context.tencabezaorden
                                          on repuestos.idorden equals orden.id
                                          join cita in context.tcitastaller
                                          on orden.idcita equals cita.id into temp
                                          from cita in temp.DefaultIfEmpty()
                                          join planMantenimiento in context.tplanmantenimiento
                                          on cita.id_tipo_inspeccion equals planMantenimiento.id into ps3
                                          from planMantenimiento in ps3.DefaultIfEmpty()
                                          join plan in context.tplanrepuestosmodelo
                                          on planMantenimiento.id equals plan.inspeccion /*into ps4
                                          from plan in ps4.DefaultIfEmpty()*/
                                          /*join solicitud in context.tsolicitudrepuestosot
                                          on repuestos.id equals solicitud.iddetalle into ps5
                                          from solicitud in ps5.DefaultIfEmpty()
                                          /*join tt in context.ttipostarifa
                                          on repuestos.tipotarifa equals tt.id*/
                                      where repuestos.idorden == id_orden
                                      select new
                                          {
                                          repuestos.id,
                                          repuestos.icb_referencia.ref_codigo,
                                          repuestos.icb_referencia.ref_descripcion,
                                          repuestos.trepuestosestados.Descripcion,
                                          repuestos.solicitado,
                                          /*idTarifa = repuestos.tipotarifa,
                                          tarifaDes = tt.Descripcion,*/
                                          solicitudes = repuestos.solicitado,
                                          repuestos.cantidad,
                                          //solicitudes = solicitud.solicitado != null ? solicitud.solicitado : false,
                                          //cantidad = repuestos.cantidad != null ? repuestos.cantidad : 0,
                                          precio = repuestos.valorunitario,
                                          /*precio = planMantenimiento != null ? plan.listaprecios.Contains("precio1") ? precios != null ? precios.precio1 : 0 :
                                                   plan.listaprecios.Contains("precio2") ? precios != null ? precios.precio1 : 0 :
                                                   plan.listaprecios.Contains("precio3") ? precios != null ? precios.precio1 : 0 :
                                                   plan.listaprecios.Contains("precio4") ? precios != null ? precios.precio1 : 0 :
                                                   plan.listaprecios.Contains("precio5") ? precios != null ? precios.precio1 : 0 :
                                                   plan.listaprecios.Contains("precio6") ? precios != null ? precios.precio1 : 0 :
                                                   plan.listaprecios.Contains("precio7") ? precios != null ? precios.precio1 : 0 :
                                                   plan.listaprecios.Contains("precio8") ? precios != null ? precios.precio1 : 0 :
                                                   plan.listaprecios.Contains("precio9") ? precios != null ? precios.precio1 : 0 : 0 :

                                                   buscarPrecioCliente.Contains("precio1") ? precios != null ? precios.precio1 : 0 :
                                                   buscarPrecioCliente.Contains("precio2") ? precios != null ? precios.precio1 : 0 :
                                                   buscarPrecioCliente.Contains("precio3") ? precios != null ? precios.precio1 : 0 :
                                                   buscarPrecioCliente.Contains("precio4") ? precios != null ? precios.precio1 : 0 :
                                                   buscarPrecioCliente.Contains("precio5") ? precios != null ? precios.precio1 : 0 :
                                                   buscarPrecioCliente.Contains("precio6") ? precios != null ? precios.precio1 : 0 :
                                                   buscarPrecioCliente.Contains("precio7") ? precios != null ? precios.precio1 : 0 :
                                                   buscarPrecioCliente.Contains("precio8") ? precios != null ? precios.precio1 : 0 :
                                                   buscarPrecioCliente.Contains("precio9") ? precios != null ? precios.precio1 : 0 : 0,*/
                                          repuestos.icb_referencia.por_iva,
                                          repuestos.icb_referencia.por_dscto,
                                          repuestos.icb_referencia.por_dscto_max,
                                          estsoli.descripcion
                                          }).Distinct().ToList();
            AlmacenController metodosstock = new AlmacenController();

  
            var repuestosSoliSQL = context.tsolicitudrepuestosot.Where(d => d.idorden == id_orden).Select(d => new
                {
                d.id,
                codigo = d.tdetallerepuestosot.idrepuesto,
                descripcion = d.tdetallerepuestosot.icb_referencia.ref_descripcion,
                cantidadSolicitada = d.cantidad,
                precioUnitario = d.valor,
                fechaSolicitud = d.fecsolicitud,
                fechaRecibido = d.fecrecibidor,
                d.recibido,
                cantidadrecibido = d.canttraslado,
                estado = d.testado_solicitud_repuesto.descripcion
                }).ToList();

            var listaSolicitados = repuestosSoliSQL.Select(g => new
                {
                g.id,
                g.codigo,
                g.descripcion,
                cantidad = g.cantidadSolicitada != null
                    ? g.cantidadSolicitada.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "0",
                precio = g.precioUnitario != null
                    ? g.precioUnitario.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "0",
                fechasolicitud = g.fechaSolicitud != null
                    ? g.fechaSolicitud.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                recibido = g.recibido ? "SI" : "NO",
                fecharecibido = g.fechaRecibido != null
                    ? g.fechaRecibido.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                    : "",
                cantidadrecibido = g.recibido ? g.cantidadrecibido.ToString("N0", new CultureInfo("is-IS")) : "",
                estado = g.estado != null ? g.estado : ""
                }).ToList();

            int solicitar = 0;

            var repuestosAgendados = buscarRepuestosSQL.Select(x => new
                {
                x.id,
                x.ref_codigo,
                x.ref_descripcion,
                x.solicitado,
                x.Descripcion,
                x.solicitudes,
                /*x.idTarifa,
				x.tarifaDes,*/
                x.precio,
                x.por_iva,
                x.cantidad,
                x.por_dscto,

                //por_dscto = buscarCliente.dscto_rep > x.por_dscto && buscarCliente.dscto_rep <= x.por_dscto_max ? buscarCliente.dscto_rep :
                //				buscarCliente.dscto_rep < x.por_dscto && x.por_dscto <= x.por_dscto_max ? x.por_dscto : 0,
                valor_dscto = Math.Round(
                    x.precio * (decimal)(buscarCliente.dscto_rep > x.por_dscto &&
                                          buscarCliente.dscto_rep <= x.por_dscto_max ? buscarCliente.dscto_rep :
                        buscarCliente.dscto_rep < x.por_dscto && x.por_dscto <= x.por_dscto_max ? x.por_dscto : 0) /
                    100 * x.cantidad),
                valorIva = Math.Round((x.cantidad * x.precio - Math.Round(
                                           x.precio * (decimal)(buscarCliente.dscto_rep > x.por_dscto &&
                                                                 buscarCliente.dscto_rep <= x.por_dscto_max
                                               ?
                                               buscarCliente.dscto_rep
                                               :
                                               buscarCliente.dscto_rep < x.por_dscto && x.por_dscto <= x.por_dscto_max
                                                   ? x.por_dscto
                                                   : 0) / 100 * x.cantidad)) * (decimal)x.por_iva / 100),
                valorTotal = Math.Round(x.precio * x.cantidad - Math.Round(
                                            x.precio * (decimal)(buscarCliente.dscto_rep > x.por_dscto &&
                                                                  buscarCliente.dscto_rep <= x.por_dscto_max
                                                ?
                                                buscarCliente.dscto_rep
                                                :
                                                buscarCliente.dscto_rep < x.por_dscto && x.por_dscto <= x.por_dscto_max
                                                    ? x.por_dscto
                                                    : 0) / 100 * x.cantidad)
                                        + Math.Round((x.cantidad * x.precio - Math.Round(
                                                          x.precio *
                                                          (decimal)(buscarCliente.dscto_rep > x.por_dscto &&
                                                                     buscarCliente.dscto_rep <= x.por_dscto_max
                                                              ?
                                                              buscarCliente.dscto_rep
                                                              :
                                                              buscarCliente.dscto_rep < x.por_dscto &&
                                                              x.por_dscto <= x.por_dscto_max
                                                                  ? x.por_dscto
                                                                  : 0) / 100 * x.cantidad)) * (decimal)x.por_iva /
                                                     100)),
                stock_bodega = metodosstock.buscarStockBodega(x.ref_codigo, ordendatos.bodega),
                stock_otrasbodegas = metodosstock.buscarStockOtras(x.ref_codigo, ordendatos.bodega),
                stock_reemplazo = metodosstock.buscarStockReemplazo(x.ref_codigo, ordendatos.bodega),
                codigo_reemplazo = metodosstock.buscarStockReemplazo(x.ref_codigo, ordendatos.bodega) > 0
                    ? traerstockreemplazo2(x.ref_codigo, ordendatos.bodega, x.cantidad)
                    : "",
                Estado_sol = x.descripcion != null ? x.descripcion : ""

                });

            foreach (var item2 in repuestosAgendados)
                {
                valorrepuestos = valorrepuestos + (item2.solicitado ? item2.valorTotal : 0);
                }

            //estado
            //mostrar el estado
            tcitasestados estadox = context.tcitasestados.Where(d => d.id == ordendatos.estadoorden).FirstOrDefault();
            string estado = "";
            if (estadox != null)
                {
                estado = "<button type='button' class='btn' style='background-color:" + estadox.color_estado +
                         "'>&nbsp;" + estadox.Descripcion + "&nbsp;</buton><input type='hidden' id='estadootform' value='" + estadox.tipoestado + "'/>";
                ViewBag.tipoEstado = estadox.tipoestado;
                }
            int rol = Convert.ToInt32(Session["user_rolid"]);
            var permisos = (from acceso in context.rolacceso
                            join rolPerm in context.rolpermisos
                            on acceso.idpermiso equals rolPerm.id
                            where acceso.idrol == rol //&& rolPerm.codigo == "P40" 
                            select new { rolPerm.codigo }).ToList();

            var resultado1 = permisos.Where(x => x.codigo == "P47").Count() > 0 ? "Si" : "No";


            ViewBag.Permiso = resultado1;
            var repuestosSolicitadosSQL = (from tto in context.ttrasladosorden
                                           join r in context.icb_referencia
                                               on tto.codigo equals r.ref_codigo
                                           /*join tt in context.ttipostarifa
                                           on tto.idtipotarifa equals tt.id*/
                                           where tto.idorden == id_orden
                                           select new
                                               {
                                               traslado = tto.idtraslado,
                                               repuesto = "(" + tto.codigo + ") " + r.ref_descripcion,
                                               //tarifa = tt.Descripcion,
                                               tto.cantidad,
                                               iva = tto.poriva,
                                               descuento = tto.pordescuento,
                                               precioU = tto.preciounitario
                                               }).ToList();

            var repuestosSolicitados = repuestosSolicitadosSQL.Select(x => new
                {
                x.traslado,
                x.repuesto,
                //x.tarifa,
                x.cantidad,
                x.precioU,
                x.iva,
                x.descuento
                });

            foreach (var item in repuestosAgendados)
                {
                string codigo = item.ref_codigo;
                int hay = listaSolicitados.Where(d => d.codigo == codigo).Count();
                if (hay == 0)
                    {
                    solicitar = 1;
                    break;
                    }
                }

            string preciooperaciones = valoroperaciones.ToString("N0", new CultureInfo("is-IS"));
            string preciorepuestos = valorrepuestos.ToString("N0", new CultureInfo("is-IS"));

            //estado OT creada
            icb_sysparameter estadoOT = context.icb_sysparameter.Where(d => d.syspar_cod == "P78").FirstOrDefault();
            int otCreada = estadoOT != null ? Convert.ToInt32(estadoOT.syspar_value) : 16;
            //estado OT Inspeccion
            icb_sysparameter estadoOT3 = context.icb_sysparameter.Where(d => d.syspar_cod == "P79").FirstOrDefault();
            int otInspeccion = estadoOT3 != null ? Convert.ToInt32(estadoOT3.syspar_value) : 17;
            //estado OT solicitud Repuestos
            icb_sysparameter estadoOT1 = context.icb_sysparameter.Where(d => d.syspar_cod == "P83").FirstOrDefault();
            int otSolicitud = estadoOT1 != null ? Convert.ToInt32(estadoOT1.syspar_value) : 17;
            //estado OT Ejecucion OT
            icb_sysparameter estadoOT4 = context.icb_sysparameter.Where(d => d.syspar_cod == "P84").FirstOrDefault();
            int otEjecucion = estadoOT1 != null ? Convert.ToInt32(estadoOT1.syspar_value) : 9;

            int? estadoope = ordendatos.estadoorden;

            int botoniniciar = 0;
            int botonpausar = 0;
            int botondespausar = 0;
            int botonfinalizar = 0;
            int botoninicioinspeccion = 0;
            int botonfininspeccion = 0;
            int botonimpresionpdf = 0;
            int botonsolicitudrepuestos = 0;

            int avanza = 0;
            string tiempotranscurrido = "00:00:00";
            //para el calculo de los tiempos
            if (ordendatos.fecha_inicio_inspeccion != null)
                {
                botoninicioinspeccion = 1;
                }

            if (ordendatos.fecha_inspeccion != null)
                {
                botonfininspeccion = 1;
                }

            if (ordendatos.fecha_impresion_cotizacion != null)
                {
                botonimpresionpdf = 1;
                }

            if (ordendatos.fecha_solicitud_repuestos != null)
                {
                botonsolicitudrepuestos = 1;
                }

            if (ordendatos.fecha_inicio_operacion != null)
                {
                botoniniciar = 1;
                if (ordendatos.fecha_fin_operacion == null)
                    {
                    //veo si hay pausas
                    int pausax = context.tpausasorden.Where(d =>
                        (d.fecha_inicio != null) & (d.fecha_fin == null) && d.idorden == ordendatos.id).Count();
                    if (pausax > 0)
                        {
                        botoniniciar = 0;
                        botondespausar = 1;
                        }
                    else
                        {
                        avanza = 1;
                        botoniniciar = 0;
                        botondespausar = 0;
                        botonpausar = 1;
                        botonfinalizar = 1;
                        }
                    }
                else
                    {
                    botoniniciar = 0;
                    botondespausar = 0;
                    botonpausar = 0;
                    botonfinalizar = 0;
                    }
                }

            tiempotranscurrido = vertiempo(ordendatos.id);
            int operacioniniciada = 0;
            if (ordendatos.fecha_inicio_operacion != null)
                {
                operacioniniciada = 1;
                }
            List<ttrackingorden> tracking = context.ttrackingorden.Where(d => d.idorden == ordendatos.id).ToList();
            var datostracking = tracking.Select(d => new
                {
                orden = d.tencabezaorden.codigoentrada,
                d.descripcion,
                estado = d.tcitasestados.Descripcion,
                responsable = d.users.user_nombre + " " + d.users.user_apellido,
                fecha = d.fecha.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")),
                observaciones = context.tpausasorden.Where(x => x.idorden == d.idorden && x.fecha_inicio == d.fecha).Select(o => o.observacion_pausa).ToList()
                }).ToList();
            string entrega = ordendatos.entrega.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));

            var tots = context.vw_tot.Where(r => r.numot == id_orden).Select(x => new
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

            decimal  totaltiempoop = Convert.ToDecimal(buscarOperaciones.Where(d=>d.estado=="1").Select(x => x.tiempo).Sum(), miCultura) ;

            icb_sysparameter idsumo= context.icb_sysparameter.Where(d => d.syspar_cod == "P155").FirstOrDefault();
            int insumo_id = idsumo != null ? Convert.ToInt32(idsumo.syspar_value) : 1;
            decimal vtotalinsumo = 0;         
            var insumo = context.Tinsumo.Where(x => x.id == insumo_id).Select(a => new { a.horas_insumo, a.porcentaje }).FirstOrDefault();
            if (insumo.horas_insumo <= totaltiempoop)
                {
            
                var insumootcount = context.tinsumooperaciones.Where(x => x.numot == ordendatos.id).Select(c => c.valort_horas).Sum();
                if (insumootcount ==null)
                    {
                    var listaophorastotales = buscarOperaciones.GroupBy(c => new { c.tipotarifa, c.tiempo }).Select(g => new CsTarifasInsumos
                        {
                        tarifa = Convert.ToInt32(g.Key.tipotarifa),
                        cantidad_horas = Convert.ToDecimal(g.Select(x=>x.tiempo).Sum(), miCultura)
                        }).ToList();

                    foreach (var item in listaophorastotales)
                        {
                        var vhora = context.ttarifastaller.Where(x => x.bodega == ordendatos.bodega && x.tipotarifa == item.tarifa).Select(x => x.valorhora).FirstOrDefault();
                        decimal vrhora = Convert.ToDecimal(vhora, miCultura);
                        decimal thorasporcentaje = Convert.ToDecimal(insumo.porcentaje, miCultura) * Convert.ToDecimal(item.cantidad_horas, miCultura);
                        decimal valorhoraporcentaje = vrhora * thorasporcentaje;
                        vtotalinsumo = vtotalinsumo + valorhoraporcentaje;
                        if (vrhora != 0)
                            {
                            tinsumooperaciones insumooperacion = new tinsumooperaciones();
                            insumooperacion.numot = ordendatos.id;
                            insumooperacion.codigo_insumo = insumo_id;
                            insumooperacion.tipotarifa = item.tarifa;
                            insumooperacion.porcentaje_total_ = thorasporcentaje;
                            insumooperacion.valort_horas = valorhoraporcentaje;
                            insumooperacion.valortarifa = vrhora;
                            context.tinsumooperaciones.Add(insumooperacion);
                            context.SaveChanges();
                            }
                      

                        }
                    }
                else {
                    vtotalinsumo = Convert.ToDecimal(context.tinsumooperaciones.Where(x => x.numot == ordendatos.id).Select(x => x.valort_horas).Sum(), miCultura);
                    }        


                }


            var listainsumos = context.tinsumooperaciones.Where(x => x.numot == ordendatos.id && x.valort_horas>0).Select(e => new
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


            var totalcombos = context.tot_combo.Where(x => x.numot == ordendatos.id && x.estado == true).Select(v => v.valortotal).Sum();

            var combosot = context.tot_combo.Where(x => x.numot == ordendatos.id && x.estado == true).Select(c => new
                {
                c.id,
                codigo = c.ttempario.codigo,
                operacion = c.ttempario.operacion,
                c.totalHorasCliente,
                c.totalhorasoperario,
                ttipostarifa = context.rtipocliente.Where(d => d.estado).Select(x=>new { x.id, x.descripcion}).ToList() ,
                c.tipotarifa,
                c.idtempario,
                valortotal = c.valortotal != null? c.valortotal:0
                }).ToList();


            return Json(new
                {
                buscarSolicitudes,
                buscarOperaciones,
                repuestosAgendados,
                repuestosSolicitados,
                preciooperaciones,
                preciorepuestos,
                resultadoinsumo,
                listaSolicitados,
                vtotalinsumo,
                otCreada,
                totalcombos,
                combosot,
                otInspeccion,
                otSolicitud,
                otEjecucion,
                estadoope,
                listainsumos,
                avanza,
                botoniniciar,
                botondespausar,
                botonpausar,
                botonfinalizar,
                tiempotranscurrido,
                datostracking,
                solicitar,
                entrega,
                botoninicioinspeccion,
                botonfininspeccion,
                botonimpresionpdf,
                botonsolicitudrepuestos,
                estado,
                tots,
                operacioniniciada
                }, JsonRequestBehavior.AllowGet);
            }

        public JsonResult EliminarInsumo(int id)
            {
            tinsumooperaciones insumoopera = context.tinsumooperaciones.Where(x => x.id == id).FirstOrDefault();

            insumoopera.valort_horas = 0;
            context.Entry(insumoopera).State = EntityState.Modified;
            context.SaveChanges();

            return Json( true , JsonRequestBehavior.AllowGet);
            }

        public string observacionMensajeTexto(int id)
            {
            string observacion = context.tdetallemanoobraot.FirstOrDefault(x => x.id == id).observacion_sms;
            return observacion;
            }

        public JsonResult AgregarCombo(int idot, string idcombo) {   

            tencabezaorden ordendatos = context.tencabezaorden.Where(d => d.id == idot).FirstOrDefault();

            ttempario tempario = context.ttempario.Where(x => x.codigo == idcombo).FirstOrDefault();

            int modgeneral = ordendatos.icb_vehiculo.modelo_vehiculo.vmodelog.id;
            var tempariosptempariolan = context.tplanrepuestosmodelo.Where(x => x.modgeneral == modgeneral  && x.inspeccion == tempario.idplanm && x.listaprecios != "N/A").Select(g =>
                  new
                      {
                      g.tempario,
                      g.ttempario.HoraCliente,
                      g.ttempario.HoraOperario
                      }).ToList();


            foreach (var item in tempariosptempariolan)
                {
                if (item.tempario!=null)
                    {
                    tot_combo comboot = new tot_combo();
                    comboot.idtempario = item.tempario;
                    comboot.numot = idot;
                    comboot.estado = true;
                    comboot.fechacreacion = DateTime.Now;
                    comboot.usercreacion = Convert.ToInt32(Session["user_usuarioid"]);
                    comboot.totalHorasCliente = Convert.ToDecimal(item.HoraCliente, miCultura);
                    comboot.totalhorasoperario = item.HoraOperario;
                    context.tot_combo.Add(comboot);
                    context.SaveChanges();

                    }


                }

            return Json(true, JsonRequestBehavior.AllowGet);
            }

        public JsonResult CambiarprecioCombo(int tarifa, string horas, int idtempario, int idot) {

            tencabezaorden ordendatos = context.tencabezaorden.Where(d => d.id == idot).FirstOrDefault();

            decimal vlhora = Convert.ToDecimal( context.ttarifastaller.Where(x=>x.tipotarifa==tarifa && x.bodega== ordendatos.bodega).Select(d=>d.valorhora).FirstOrDefault(), miCultura);
            decimal horasc = Convert.ToDecimal(horas, miCultura);
            tot_combo combo = context.tot_combo.Where(x => x.id == idtempario).FirstOrDefault();

            combo.valortotal = horasc * vlhora;
            combo.tipotarifa = tarifa;
            context.Entry(combo).State = EntityState.Modified;
            context.SaveChanges();


            return Json(true, JsonRequestBehavior.AllowGet);
            }



        public JsonResult EliminarCombo(int idtempario) {

            tot_combo comboot = context.tot_combo.Where(x => x.id == idtempario).FirstOrDefault();
            comboot.estado = false;
            context.Entry(comboot).State = EntityState.Modified;
            context.SaveChanges();

            return Json(true, JsonRequestBehavior.AllowGet);
            }

        public string versms(int id, string estado, DateTime? fecha, bool? respuesta)
            {
            string resultado = "";
            double fec = fecha != null ? (DateTime.Now - fecha).Value.TotalHours : 0;
            string icon_msm = fecha != null || !string.IsNullOrWhiteSpace(estado) ? " fa-envelope" :
                fec > 0 && fec < 24 ? " fa-check" : "fa-envelope-o";
            string onclick_msm = fecha != null || fec > 0 && fec < 24
                ? " "
                : " onclick=enviomsm2(" + id + ") id=msm_envioM" + id;
            resultado = "<i style='font-size:17pt;cursor:pointer' class='fa " + icon_msm + " text-primary'" +
                        onclick_msm + "></i>";

            return resultado;
            }
        public JsonResult CalcularInsumo(int id, int codigo, decimal porcentaje, int tarifa) {

            tencabezaorden ordendatos = context.tencabezaorden.Where(d => d.id == id).FirstOrDefault();
            var operaciones = context.tdetallemanoobraot.Where(x => x.idorden == id).Select(x => x.tiempo).Sum();
            var vhora = context.ttarifastaller.Where(x => x.bodega == ordendatos.bodega && x.tipotarifa == tarifa).Select(x => x.valorhora).FirstOrDefault();
            decimal vrhora = Convert.ToDecimal(vhora, miCultura);
            decimal vtotalinsumo = 0;
            decimal thorasporcentaje = Convert.ToDecimal(porcentaje, miCultura) * Convert.ToDecimal(operaciones, miCultura);
            decimal valorhoraporcentaje = vrhora * thorasporcentaje;
            vtotalinsumo = vtotalinsumo + valorhoraporcentaje;

            tinsumooperaciones insumooperacion = new tinsumooperaciones();
            insumooperacion.numot = ordendatos.id;
            insumooperacion.codigo_insumo = codigo;
            insumooperacion.tipotarifa =tarifa;
            insumooperacion.porcentaje_total_ = thorasporcentaje;
            insumooperacion.valort_horas = valorhoraporcentaje;
            insumooperacion.valortarifa = vrhora;
            context.tinsumooperaciones.Add(insumooperacion);
            context.SaveChanges();

            return Json(true, JsonRequestBehavior.AllowGet);
            }
        
        public JsonResult Porcentajeinsumo(int codigo) {

            string porcentaje = context.Tinsumo.Where(x => x.id == codigo).Select(c => c.porcentaje).FirstOrDefault().ToString();

            return Json(porcentaje, JsonRequestBehavior.AllowGet);
            }

        public JsonResult Permisosinsumos() {
           
            int rol = Convert.ToInt32(Session["user_rolid"]);

            var permisos = (from acceso in context.rolacceso
                            join rolPerm in context.rolpermisos
                            on acceso.idpermiso equals rolPerm.id
                            where acceso.idrol == rol 
                            select new { rolPerm.codigo }).ToList();

            var resultado = permisos.Where(x => x.codigo == "P50").Count() > 0 ? "Si" : "No";
          


            return Json(resultado, JsonRequestBehavior.AllowGet);
            }

        public JsonResult PermisoImprimirOT(int idorden )
            {
            var resultado = "";
            int rol = Convert.ToInt32(Session["user_rolid"]);

            var permisos = (from acceso in context.rolacceso
                            join rolPerm in context.rolpermisos
                            on acceso.idpermiso equals rolPerm.id
                            where acceso.idrol == rol
                            select new { rolPerm.codigo }).ToList();

            resultado = permisos.Where(x => x.codigo == "P53").Count() > 0 ? "Si" : "No";

            var orden = context.tencabezaorden.Where(x => x.id == idorden).FirstOrDefault();
            if (resultado == "Si")
                {
                return Json(resultado, JsonRequestBehavior.AllowGet);
                }
            else {
                if (orden.checlimpresionot != true)
                    {
                    resultado = "Si";
                    return Json(resultado, JsonRequestBehavior.AllowGet);
                    }
                else {
                    resultado = "No";
                    return Json(resultado, JsonRequestBehavior.AllowGet);

                    }          

                }            
            }


        public JsonResult PermisoReactivarot(int idorden)
            {

            int rol = Convert.ToInt32(Session["user_rolid"]);

            var permisos = (from acceso in context.rolacceso
                            join rolPerm in context.rolpermisos
                            on acceso.idpermiso equals rolPerm.id
                            where acceso.idrol == rol
                            select new { rolPerm.codigo }).ToList();

            var resultado = permisos.Where(x => x.codigo == "P52").Count() > 0 ? "Si" : "No";

            tencabezaorden ordendatos = context.tencabezaorden.Where(d => d.id == idorden).FirstOrDefault();

            icb_sysparameter estadoOT = context.icb_sysparameter.Where(d => d.syspar_cod == "P89")
                                 .FirstOrDefault();
            int estadodeOT = estadoOT != null ? Convert.ToInt32(estadoOT.syspar_value) : 10;


            if (resultado=="Si")
                {
                if (estadodeOT!= ordendatos.estadoorden)
                    {
                    resultado = "No";
                    }


                }

            return Json(resultado, JsonRequestBehavior.AllowGet);
            }



        public JsonResult CambiarEstado(int idorden)
            {

          
            tencabezaorden ordendatos = context.tencabezaorden.Where(d => d.id == idorden).FirstOrDefault();

            icb_sysparameter estadoOT = context.icb_sysparameter.Where(d => d.syspar_cod == "P58")
                                 .FirstOrDefault();
            int estadodeOT = estadoOT != null ? Convert.ToInt32(estadoOT.syspar_value) : 9;

            ordendatos.estadoorden = estadodeOT;
            ordendatos.fecha_fin_operacion = null;
            context.Entry(ordendatos).State = EntityState.Modified;
            context.SaveChanges();

             string operacion = "";
            operacion = "La orden " + ordendatos.codigoentrada + " se ha reactivado";

            ttrackingorden track = new ttrackingorden
                {
                descripcion = operacion,
                estado = Convert.ToInt32(ordendatos.estadoorden),
                fecha = DateTime.Now,
                idorden = ordendatos.id,
                //verificar sesion cuando se haga esta consulta. deberia ser la persona logueada
                responsable = ordendatos.idtecnico != null ? ordendatos.idtecnico.Value : ordendatos.userid_creacion
                };
            context.ttrackingorden.Add(track);
            context.SaveChanges();







            return Json(true, JsonRequestBehavior.AllowGet);
            }





        public string traerstockreemplazo2(string idrepuesto, int? idbodega, int? cantidad)
            {
            if (!string.IsNullOrWhiteSpace(idrepuesto) && idbodega != null && cantidad != null)
                {
                List<tablastockreemplazo> repuestos = traerstockreemplazo(idrepuesto, idbodega, cantidad);
                if (repuestos.Count > 0)
                    {
                    return repuestos.Select(d => d.nombrereferencia).FirstOrDefault();
                    }

                return "";
                }

            return "";
            }

        public List<tablastockreemplazo> traerstockreemplazo(string idrepuesto, int? idbodega, decimal? cantidad)
            {
            if (!string.IsNullOrWhiteSpace(idrepuesto) && idbodega != null && cantidad != null)
                {
                //busco la bodega que exusta
                bodega_concesionario bod = context.bodega_concesionario.Where(d => d.id == idbodega).FirstOrDefault();
                //busco que exista la referencia
                icb_referencia refer = context.icb_referencia.Where(d => d.ref_codigo == idrepuesto).FirstOrDefault();
                if (bod != null && refer != null)
                    {
                    int encontrado = 0;
                    List<tablastockreemplazo> lista = new List<tablastockreemplazo>();

                    //busco el reemplazo de esta referencia en ESTA bodega
                    rremplazos reemplazo = context.rremplazos.Where(d => d.referencia == idrepuesto && d.alterno != idrepuesto)
                        .FirstOrDefault();
                    //si tiene reemplazo
                    if (reemplazo != null)
                        {
                        string idreemp = reemplazo.alterno;
                        //busco si tiene stock ese reemplazo
                        do
                            {
                            decimal stock = context.vw_inventario_hoy
                                .Where(e => e.ref_codigo == idreemp && e.bodega == idbodega).Select(e => e.stock)
                                .FirstOrDefault();
                            if (stock > 0 && stock >= cantidad)
                                {
                                lista.Add(context.vw_inventario_hoy
                                    .Where(e => e.ref_codigo == idreemp && e.bodega == idbodega).Select(e =>
                                        new tablastockreemplazo
                                            {
                                            stockreemplazo = e.bodega + "," + e.stock,
                                            nombrereferencia = e.ref_codigo + " (" + e.ref_descripcion + ")"
                                            }).FirstOrDefault());
                                encontrado = 1;
                                }
                            else
                                {
                                //veo si ese reemplazo tiene reemplazo y vuelvo
                                rremplazos reemplazo2 = context.rremplazos
                                    .Where(d => d.referencia == idreemp && d.alterno != idreemp).FirstOrDefault();
                                if (reemplazo2 != null)
                                    {
                                    idreemp = reemplazo2.alterno;
                                    encontrado = 0;
                                    }
                                else
                                    {
                                    encontrado = 1;
                                    }
                                }
                            } while (encontrado == 0);
                        }

                    return lista;
                    }

                return new List<tablastockreemplazo>();
                }

            return new List<tablastockreemplazo>();
            }

        public JsonResult BuscarContactosCliente(int? id_tercero, string documento)
            {
            var buscarContactos = (from contacto in context.icb_contacto_tercero
                                   join tipoContacto in context.tipocontactotercero
                                       on contacto.tipocontacto equals tipoContacto.idtipocontacto
                                   where contacto.tercero_id == id_tercero
                                   select new
                                       {
                                       contacto.con_tercero_nombre,
                                       contacto.con_tercero_id
                                       }).ToList();
            return Json(new { buscarContactos }, JsonRequestBehavior.AllowGet);
            }

        public JsonResult BuscarClientePorDocumento(int? id_tercero, string documento)
            {
            var buscarCliente = (from tercero in context.icb_terceros
                                 join cliente in context.tercero_cliente
                                     on tercero.tercero_id equals cliente.tercero_id into ps
                                 from cliente in ps.DefaultIfEmpty()
                                 where tercero.tercero_id == id_tercero || tercero.doc_tercero == documento
                                 select new
                                     {
                                     tercero.tercero_id,
                                     tercero.doc_tercero,
                                     cltercero_id = cliente != null ? cliente.cltercero_id : 0,
                                     //cltercero_id = cliente.cltercero_id != null ? cliente.cltercero_id : 0,
                                     nombres = tercero.razon_social + tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                               tercero.apellido_tercero + " " + tercero.segapellido_tercero,
                                     telf_tercero = tercero.telf_tercero != null ? tercero.telf_tercero : "",
                                     celular_tercero = tercero.celular_tercero != null ? tercero.celular_tercero : "",
                                     email_tercero = tercero.email_tercero != null ? tercero.email_tercero : ""
                                     }).FirstOrDefault();
            if (buscarCliente != null)
                {
                return Json(new { success = true, buscarCliente }, JsonRequestBehavior.AllowGet);
                }

            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

        public JsonResult BuscarVehiculoPorPlaca(string placa)
            {
            var buscarPlaca = (from vehiculo in context.icb_vehiculo
                               join color in context.color_vehiculo
                                   on vehiculo.colvh_id equals color.colvh_id
                               join aseg in context.icb_aseguradoras
                               on vehiculo.nitaseguradora equals aseg.aseg_id into ase
                               from aseg in ase.DefaultIfEmpty()
                               join ter in context.icb_terceros
                                   on aseg.idtercero equals ter.tercero_id into tero
                               from ter in tero.DefaultIfEmpty()
                               join modelo in context.modelo_vehiculo
                               on vehiculo.modvh_id equals modelo.modvh_codigo
                               join conseciona in context.concesionario
                               on modelo.concesionarioid equals conseciona.id into con
                               from conseciona in con.DefaultIfEmpty()                   
                               where vehiculo.plac_vh == placa || vehiculo.plan_mayor == placa
                               select new
                                   {
                                   vehiculo.nummot_vh,
                                   vehiculo.vin,
                                   vehiculo.kilometraje,
                                   color.colvh_nombre,
                                   vehiculo.fecha_venta,
                                   vehiculo.fecha_fin_garantia,
                                   vehiculo.propietario,
                                   vehiculo.modelo_vehiculo.modelogkit,
                                   vehiculo.fecfact_fabrica,
                                   aseguradora = ter.razon_social + " " + ter.prinom_tercero + " " +
                                           ter.segnom_tercero + " " + ter.apellido_tercero + " " + ter.segapellido_tercero,
                                   modelo.modvh_nombre,
                                   flota = vehiculo.flota == true ? "Si" : "No",
                                   concesionario = conseciona.nombre,
                          
                                   }).FirstOrDefault();
            if (buscarPlaca != null)
                {
                var buscarVehiculo = new
                    {
                    buscarPlaca.nummot_vh,
                    buscarPlaca.vin,
                    buscarPlaca.kilometraje,
                    buscarPlaca.colvh_nombre,
                    fecha_venta = buscarPlaca.fecha_venta != null
                        ? buscarPlaca.fecha_venta.Value.ToShortDateString()
                        : "",
                    fecha_fin_garantia = buscarPlaca.fecha_fin_garantia != null
                        ? buscarPlaca.fecha_fin_garantia.Value.ToShortDateString()
                        : "",
                    buscarPlaca.propietario,
                    buscarPlaca.modelogkit,
                    buscarPlaca.fecfact_fabrica,
                    buscarPlaca.aseguradora,
                    buscarPlaca.modvh_nombre,
                    buscarPlaca.flota,
                    buscarPlaca.concesionario,
         
                    };

                int bodegaActual = Convert.ToInt32(Session["user_bodega"]);
                int buscarCantidadCitas = (from citas in context.tcitastaller
                                           join bahia in context.tbahias
                                               on citas.bahia equals bahia.id
                                           join bodega in context.bodega_concesionario
                                               on bahia.bodega equals bodega.id
                                           join tipoInspeccion in context.tplanmantenimiento
                                               on citas.id_tipo_inspeccion equals tipoInspeccion.id into ps
                                           from tipoInspeccion in ps.DefaultIfEmpty()
                                           where citas.placa == placa && citas.estadocita == 1 && bodega.id == bodegaActual
                                           select new
                                               {
                                               citas.id
                                               }).Count();

                var planesMantenimiento = context.tplanrepuestosmodelo.GroupBy(d => d.inspeccion)
                    .Where(d => d.Select(e => e.modgeneral).FirstOrDefault() == buscarPlaca.modelogkit).Select(d =>
                        new { id = d.Key, nombre = d.Select(e => e.tplanmantenimiento.Descripcion).FirstOrDefault() })
                    .ToList();
                return Json(new { success = true, buscarVehiculo, buscarCantidadCitas, planesMantenimiento },
                    JsonRequestBehavior.AllowGet);
                }

            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

        public void listasDesplegables(tencabezaorden orden)
            {
            ViewBag.razoningreso =
                new SelectList(
                    context.trazonesingreso.Where(d => !d.razoz_ingreso.ToUpper().Contains("COLISION") && d.estado),
                    "id", "razoz_ingreso", orden.razoningreso);
            icb_sysparameter garantiaparametro = context.icb_sysparameter.Where(d => d.syspar_cod == "P87").FirstOrDefault();
            ViewBag.razongarantia = garantiaparametro != null ? Convert.ToInt32(garantiaparametro.syspar_value) : 7;

            /*if (orden.razoningreso != null)
                {*/
                List<trazonesingreso> razonessec = context.trazonesingreso.Where(d => d.id != orden.razoningreso).ToList();
                ViewBag.razon_secundaria = new SelectList(razonessec, "id", "razoz_ingreso", orden.razon_secundaria);
            /*    }
            else
                {
                ViewBag.razon_secundaria =
                    new SelectList(context.trazonesingreso, "id", "razoz_ingreso", orden.razon_secundaria);
                }*/

            ViewBag.deducible =
                orden.deducible /*!=null?orden.deducible.Value.ToString("N0",new CultureInfo("is-IS")):""*/;
            ViewBag.minimoOrden =
                orden.minimo /*!= null ? orden.minimo.Value.ToString("N0", new CultureInfo("is-IS")) : ""*/;
            ViewBag.tipooperacion = new SelectList(context.ttipooperacion, "id", "Descripcion", orden.tipooperacion);
            ViewBag.aseguradora = new SelectList(context.icb_aseguradoras, "aseg_id", "nombre", orden.aseguradora);
            ViewBag.id_plan_mantenimiento = new SelectList(context.tplanmantenimiento, "id", "Descripcion",
                orden.id_plan_mantenimiento);
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
            if (orden.razoningreso == razongarantia || orden.razon_secundaria == razongarantia)
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
            //lamo a los técnicos de las ba
            List<int> tecnicosbahias2 = (from bahia in context.tbahias
                                         join tecnicos in context.ttecnicos
                                             on bahia.idtecnico equals tecnicos.id
                                         where bahia.bodega == bodegaActual && tecnicos.otros_casos
                                         select
                                             tecnicos.idusuario
                ).ToList();
            List<int> tecnicosbahias = tecnicosbahias2.Distinct().ToList();

            var buscarTecnicos = (from usuario in context.users
                                  join bodegaUsuario in context.bodega_usuario
                                      on usuario.user_id equals bodegaUsuario.id_usuario
                                  join tecnicos in context.ttecnicos
                                      on usuario.user_id equals tecnicos.idusuario
                                  where bodegaUsuario.id_bodega == bodegaActual && usuario.rol_id == roltecnico
                                                                                && (context.tbahias.Where(d => d.idtecnico == tecnicos.id)
                                                                                        .Count() == 0 ||
                                                                                    tecnicosbahias.Contains(usuario.user_id))
                                  select new
                                      {
                                      usuario.user_id,
                                      nombreUsuario = usuario.user_nombre + " " + usuario.user_apellido
                                      }).ToList();

            /*var buscarTecnicos2 = (from usuario in context.users
								  join bodegaUsuario in context.bodega_usuario
								  on usuario.user_id equals bodegaUsuario.id_usuario
								  where bodegaUsuario.id_bodega == bodegaActual && usuario.rol_id == roltecnico
			                      select new
								  {
									  usuario.user_id,
									  nombreUsuario = usuario.user_nombre + " " + usuario.user_apellido
								  }).ToList();*/

            ViewBag.idtipodoc = new SelectList(buscarDocumentos, "tpdoc_id", "nombre", orden.idtipodoc);

            int asesorx = Convert.ToInt32(Session["user_usuarioid"]);
            int asesor = 0;
            users nombreasesorx = context.users.Where(d => d.user_id == asesorx).FirstOrDefault();
            if (nombreasesorx != null)
                {
                //asesor = nombreasesorx.user_nombre + " " + nombreasesorx.user_apellido;
                asesor = asesorx;
                }

            ViewBag.asesor = asesor;
            ViewBag.operacionesTempario = new SelectList(context.ttempario, "codigo", "operacion");
            //ViewBag.tipoTarifa = new SelectList(context.ttipostarifa, "id", "Descripcion");
            //ViewBag.tipoTarifaR = new SelectList(context.ttipostarifa, "id", "Descripcion");
            ViewBag.tecnico = new SelectList(buscarTecnicos, "user_id", "nombreUsuario", orden.idtecnico);
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

        public void listasDesplegables2(EncabezadoOTModel orden)
            {
            ViewBag.razoningreso = new SelectList(context.trazonesingreso, "id", "razoz_ingreso", orden.razoningreso);
            icb_sysparameter garantiaparametro = context.icb_sysparameter.Where(d => d.syspar_cod == "P87").FirstOrDefault();
            ViewBag.razongarantia = garantiaparametro != null ? Convert.ToInt32(garantiaparametro.syspar_value) : 7;

            //razon de ingreso accesorios
            icb_sysparameter accesoriosparametro = context.icb_sysparameter.Where(d => d.syspar_cod == "P116").FirstOrDefault();
            int razoningresoacce = accesoriosparametro != null ? Convert.ToInt32(accesoriosparametro.syspar_value) : 5;

            if (orden.razoningreso == razoningresoacce)
                {
                ViewBag.mostrarsignos = 0;
                }
            else
                {
                ViewBag.mostrarsignos = 1;
                }

            /*if (orden.razoningreso != null)
                {*/
                List<trazonesingreso> razonessec = context.trazonesingreso.Where(d => d.id != orden.razoningreso).ToList();
                ViewBag.razon_secundaria = new SelectList(razonessec, "id", "razoz_ingreso", orden.razon_secundaria);
             /*   }
            else
                {
                ViewBag.razon_secundaria =
                    new SelectList(context.trazonesingreso, "id", "razoz_ingreso", orden.razon_secundaria);
                }*/

            ViewBag.id_plan_mantenimiento = new SelectList(context.tplanmantenimiento, "id", "Descripcion",
                orden.id_plan_mantenimiento);

            ViewBag.deducible =
                orden.deducible /*!=null?orden.deducible.Value.ToString("N0",new CultureInfo("is-IS")):""*/;
            ViewBag.minimoOrden =
                orden.minimo /*!= null ? orden.minimo.Value.ToString("N0", new CultureInfo("is-IS")) : ""*/;
            ViewBag.tipooperacion = new SelectList(context.ttipooperacion, "id", "Descripcion", orden.tipooperacion);
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
            if (orden.razoningreso == razongarantia || orden.razon_secundaria == razongarantia)
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
            //var buscarTecnicos = (from usuario in context.users
            //                      join bodegaUsuario in context.bodega_usuario
            //                          on usuario.user_id equals bodegaUsuario.id_usuario
            //                      where bodegaUsuario.id_bodega == bodegaActual && usuario.rol_id == roltecnico
            //                      select new
            //                          {
            //                          usuario.user_id,
            //                          nombreUsuario = usuario.user_nombre + " " + usuario.user_apellido
            //                          }).ToList();

            var buscarTecnicos = (from usuario in context.users
                                  join tecnico in context.ttecnicos
                                      on usuario.user_id equals tecnico.idusuario                        
                                  select new
                                      {
                                      usuario.user_id,
                                      nombreUsuario = usuario.user_nombre + " " + usuario.user_apellido
                                      }).ToList();



            ViewBag.idtipodoc = new SelectList(buscarDocumentos, "tpdoc_id", "nombre", orden.idtipodoc);
            //ViewBag.asesor = new SelectList(buscarAsesoresServicio, "user_id", "nombreUsuario", orden.asesor);
            ViewBag.operacionesTempario = new SelectList(context.ttempario, "codigo", "operacion");
            //ViewBag.tipoTarifa = new SelectList(context.ttipostarifa, "id", "Descripcion");
            //ViewBag.tipoTarifaR = new SelectList(context.ttipostarifa, "id", "Descripcion");
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

        public JsonResult BuscarReferencia(string codigo_referencia, string documento_cliente)
            {
            var buscarCliente = (from tercero in context.icb_terceros
                                 join cliente in context.tercero_cliente
                                     on tercero.tercero_id equals cliente.tercero_id
                                 where tercero.doc_tercero == documento_cliente
                                 select new
                                     {
                                     cliente.lprecios_repuestos,
                                     cliente.dscto_rep
                                     }).FirstOrDefault();
            if (buscarCliente == null)
                {
                return Json(
                    new
                        {
                        error = true,
                        errorMessage =
                            "Debe digitar el documento del cliente para poder calcular el valor de la referencia dependiendo del tipo de cliente."
                        }, JsonRequestBehavior.AllowGet);
                }

            string buscarPrecioCliente = !string.IsNullOrEmpty(buscarCliente.lprecios_repuestos)
                ? buscarCliente.lprecios_repuestos
                : "precio1";
            var buscarReferenciaSQL = (from referencia in context.icb_referencia
                                       join precios in context.rprecios
                                           on referencia.ref_codigo equals precios.codigo into ps2
                                       from precios in ps2.DefaultIfEmpty()
                                       where referencia.ref_codigo == codigo_referencia
                                       select new
                                           {
                                           referencia.ref_codigo,
                                           referencia.ref_descripcion,
                                           precio = buscarPrecioCliente.Contains("precio1") ? precios != null ? precios.precio1 : 0 :
                                               buscarPrecioCliente.Contains("precio2") ? precios != null ? precios.precio1 : 0 :
                                               buscarPrecioCliente.Contains("precio3") ? precios != null ? precios.precio1 : 0 :
                                               buscarPrecioCliente.Contains("precio4") ? precios != null ? precios.precio1 : 0 :
                                               buscarPrecioCliente.Contains("precio5") ? precios != null ? precios.precio1 : 0 :
                                               buscarPrecioCliente.Contains("precio6") ? precios != null ? precios.precio1 : 0 :
                                               buscarPrecioCliente.Contains("precio7") ? precios != null ? precios.precio1 : 0 :
                                               buscarPrecioCliente.Contains("precio8") ? precios != null ? precios.precio1 : 0 :
                                               buscarPrecioCliente.Contains("precio9") ? precios != null ? precios.precio1 : 0 : 0,
                                           referencia.por_iva,
                                           referencia.por_dscto,
                                           referencia.por_dscto_max
                                           }).FirstOrDefault();

            var referenciaEncontrada = new
                {
                buscarReferenciaSQL.ref_codigo,
                buscarReferenciaSQL.ref_descripcion,
                buscarReferenciaSQL.precio,
                buscarReferenciaSQL.por_iva,
                //valorIva = Math.Round((buscarReferenciaSQL.precio * (decimal)buscarReferenciaSQL.por_iva) / 100),
                por_dscto = buscarCliente.dscto_rep > buscarReferenciaSQL.por_dscto &&
                            buscarCliente.dscto_rep <= buscarReferenciaSQL.por_dscto_max ? buscarCliente.dscto_rep :
                    buscarCliente.dscto_rep < buscarReferenciaSQL.por_dscto &&
                    buscarReferenciaSQL.por_dscto <= buscarReferenciaSQL.por_dscto_max ? buscarReferenciaSQL.por_dscto :
                    0
                //valor_dscto = Math.Round((buscarReferenciaSQL.precio * (decimal)(buscarCliente.dscto_rep > buscarReferenciaSQL.por_dscto && buscarCliente.dscto_rep <= buscarReferenciaSQL.por_dscto_max ? buscarCliente.dscto_rep :
                //buscarCliente.dscto_rep < buscarReferenciaSQL.por_dscto && buscarReferenciaSQL.por_dscto <= buscarReferenciaSQL.por_dscto_max ? buscarReferenciaSQL.por_dscto : 0)) / 100)
                };
            return Json(new { error = false, referenciaEncontrada }, JsonRequestBehavior.AllowGet);
            }

        public JsonResult BuscarNitTarifa(int tarifa)
            {
            int bodegaLog = Convert.ToInt32(Session["user_bodega"]);
            int? nit = context.ttarifastaller.FirstOrDefault(x => x.tipotarifa == tarifa && x.bodega == bodegaLog)
                .idtercero;
            return Json(nit, JsonRequestBehavior.AllowGet);
            }

        public JsonResult buscarTareasAsignadas(int? idCita)
            {
            if (idCita != null)
                {
                var tareasInspeccion = (from tei in context.tencabinspeccion
                                        join t in context.ttareainspeccionprioridad
                                            on tei.id equals t.idencabinspeccion
                                        join ti in context.ttareasinspeccion
                                            on t.idtareainspeccion equals ti.id
                                        where tei.idcita == idCita
                                        select new
                                            {
                                            t.id,
                                            ti.descripcion,
                                            t.cantidad,
                                            t.valor_unitario,
                                            fecautorizacion = t.fecautorizacion.Value.ToString(),
                                            t.autorizado
                                            }).ToList();

                return Json(tareasInspeccion, JsonRequestBehavior.AllowGet);
                }

            return Json(0, JsonRequestBehavior.AllowGet);
            }

        public JsonResult buscarRazonesSecundarias(int? razonprimaria)
            {
            Expression<Func<trazonesingreso, bool>> predicado = PredicateBuilder.True<trazonesingreso>();
            if (razonprimaria != null)
                {
                predicado = predicado.And(d => d.id != razonprimaria);
                }

            predicado = predicado.And(d => d.estado);

            var razones = context.trazonesingreso.Where(predicado).Select(d => new { d.id, nombre = d.razoz_ingreso })
                .ToList();
            return Json(razones);
            }


        public JsonResult autorizarTarea(int idTarea)
            {
            ttareainspeccionprioridad existe = context.ttareainspeccionprioridad.FirstOrDefault(x => x.id == idTarea);

            if (existe.autorizado == false)
                {
                //no se ha autorizado la tarea, por consiguiente la autorizamos y guardamos la fecha
                existe.autorizado = true;
                existe.fecautorizacion = DateTime.Now;
                context.Entry(existe).State = EntityState.Modified;
                }
            else
                {
                //Ya se autorizo la tarea, por consiguiente la desautorizamos y blanqueamos la fecha
                existe.autorizado = false;
                existe.fecautorizacion = null;
                context.Entry(existe).State = EntityState.Modified;
                }

            context.SaveChanges();

            return Json(JsonRequestBehavior.AllowGet);
            }

        public JsonResult agregarRepuesto(int? orden, string codigorepuesto, int? cantidad, string preciounitario,
            string porcentajeiva, string porcentajedescuento)
            {
            if (orden == null || string.IsNullOrWhiteSpace(codigorepuesto) || cantidad == null ||
                string.IsNullOrWhiteSpace(preciounitario) || string.IsNullOrWhiteSpace(porcentajeiva) ||
                string.IsNullOrWhiteSpace(porcentajedescuento))
                {
                return Json(0);
                }

            //veo la orden
            tencabezaorden datosorden = context.tencabezaorden.Where(d => d.id == orden).FirstOrDefault();
            if (datosorden != null)
                {
                //agrego el repuesto
                bool convertirpre = decimal.TryParse(preciounitario, out decimal preciore);
                bool convertiriva = decimal.TryParse(porcentajeiva, out decimal porciva);
                bool convertirdes = decimal.TryParse(porcentajedescuento, out decimal pordesc);

                tdetallerepuestosot nuevore = new tdetallerepuestosot
                    {
                    idorden = datosorden.id,
                    idrepuesto = codigorepuesto,
                    cantidad = cantidad.Value,
                    valorunitario = preciore,
                    poriva = porciva,
                    pordescto = pordesc,
                    idtercero = datosorden.tercero
                    };
                context.tdetallerepuestosot.Add(nuevore);
                int guardar = context.SaveChanges();
                if (guardar > 0)
                    {
                    var data = new { idrep = nuevore.id };
                    return Json(1);
                    }

                return Json(0);
                }

            return Json(0);
            }

        public void EliminarRepuesto(int id)
            {
            tdetallerepuestosot dato = context.tdetallerepuestosot.Find(id);
            context.Entry(dato).State = EntityState.Deleted;
            context.SaveChanges();
            }

        public void EliminarOperacion(int id)
            {
            tdetallemanoobraot dato = context.tdetallemanoobraot.Find(id);
            SignosVitalesController sv = new SignosVitalesController();
            sv.eliminarOperacionSignoVitales(id, false);
            double tiempo = Convert.ToDouble(dato.tiempo != null ? dato.tiempo.Value : 0);
            tiempo = -tiempo;

            tencabezaorden orden = context.tencabezaorden.Where(d => d.id == dato.idorden).FirstOrDefault();

            orden.entrega = orden.entrega.AddHours(tiempo);
            context.Entry(orden).State = EntityState.Modified;
            context.Entry(dato).State = EntityState.Deleted;
            context.SaveChanges();
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

        public string vertiempo(int id)
            {
            //var orden
            tencabezaorden orden = context.tencabezaorden.Where(d => d.id == id).FirstOrDefault();
            if (orden != null)
                {
                if (orden.fecha_inicio_operacion == null)
                    {
                    return "00:00:00";
                    }

                if (orden.fecha_fin_operacion == null)
                    {
                    //veo si hay pausas sin finalizar
                    int pausax = context.tpausasorden
                        .Where(d => (d.fecha_inicio != null) & (d.fecha_fin == null) && d.idorden == id).Count();
                    if (pausax > 0)
                        {
                        return "PAUSADA";
                        }

                    List<tpausasorden> pausax2 = context.tpausasorden
                        .Where(d => (d.fecha_inicio != null) & (d.fecha_fin != null) && d.idorden == id).ToList();
                    TimeSpan tiempo = new TimeSpan(0, 0, 0);
                    foreach (tpausasorden item in pausax2)
                        {
                        tiempo = tiempo + (item.fecha_fin.Value - item.fecha_inicio);
                        }

                    TimeSpan tiempoOperacion = DateTime.Now - orden.fecha_inicio_operacion.Value;
                    TimeSpan tiempototal = tiempoOperacion - tiempo;
                    return tiempototal.Hours + ":" + tiempototal.Minutes + ":" + tiempototal.Seconds;
                    }

                    {
                    List<tpausasorden> pausax2 = context.tpausasorden
                        .Where(d => (d.fecha_inicio != null) & (d.fecha_fin != null) && d.idorden == id).ToList();
                    TimeSpan tiempo = new TimeSpan(0, 0, 0);
                    foreach (tpausasorden item in pausax2)
                        {
                        tiempo = tiempo + (item.fecha_fin.Value - item.fecha_inicio);
                        }

                    TimeSpan tiempoOperacion = orden.fecha_fin_operacion.Value - orden.fecha_inicio_operacion.Value;
                    TimeSpan tiempototal = tiempoOperacion - tiempo;
                    return tiempototal.Hours + ":" + tiempototal.Minutes + ":" + tiempototal.Seconds;
                    }
                }

            return "00:00:00";
            }

        public JsonResult ComenzarEjecucion(int? id_orden, int? ope, int motivo = 0, string observacionpausa = "")
            {
            if (id_orden != null && ope != null)
                {
                string operacion = "";
                //busco la ot
                //evento OT en ejecución
                icb_sysparameter ejecu = context.icb_sysparameter.Where(d => d.syspar_cod == "P84").FirstOrDefault();
                int ejecutar = ejecu != null ? Convert.ToInt32(ejecu.syspar_value) : 9;

                icb_sysparameter fina = context.icb_sysparameter.Where(d => d.syspar_cod == "P89").FirstOrDefault();
                int finalizar = fina != null ? Convert.ToInt32(fina.syspar_value) : 10;

                tencabezaorden orden = context.tencabezaorden.Where(d => d.id == id_orden).FirstOrDefault();
                if (orden != null)
                    {
                    if (ope == 1)
                        {
                        operacion = "La orden " + orden.codigoentrada +
                                    " ha iniciado proceso de Ejecución de Operaciones";
                        orden.fecha_inicio_operacion = DateTime.Now;
                        orden.estadoorden = ejecutar;
                        orden.estado = true;
                        context.Entry(orden).State = EntityState.Modified;
                        if (orden.idcita != null)
                            {
                            //busco la cita
                            tcitastaller cita = context.tcitastaller.Where(d => d.id == orden.idcita.Value).FirstOrDefault();
                            cita.estadocita = ejecutar;
                            context.Entry(cita).State = EntityState.Modified;
                            }
                        }
                    else if (ope == 2)
                        {
                        operacion = "La orden " + orden.codigoentrada + " ha pausado Ejecución de Operaciones por motivo: "+ observacionpausa;
                        tpausasorden pausa = new tpausasorden
                            {
                            fecha_inicio = DateTime.Now,
                            idorden = orden.id,
                            razon_pausa = motivo,
                            observacion_pausa = observacionpausa
                            };
                        context.tpausasorden.Add(pausa);
                        }
                    else if (ope == 3)
                        {
                        operacion = "La orden " + orden.codigoentrada + " ha reanudado Ejecución de Operaciones";
                        tpausasorden pausa = context.tpausasorden.OrderByDescending(d => d.fecha_inicio)
                            .Where(d => d.idorden == orden.id && d.fecha_fin == null).FirstOrDefault();

                        pausa.fecha_fin = DateTime.Now;
                        context.Entry(pausa).State = EntityState.Modified;
                        }
                    else if (ope == 4)
                        {
                        try
                            {
                            TimeSpan totalpausas = TimeSpan.Zero;

                            operacion = "La orden " + orden.codigoentrada +
                                        " ha finalizado proceso de Ejecución de Operaciones";
                            orden.fecha_fin_operacion = DateTime.Now;
                            var fechafin = DateTime.Now;
                            var totaltiempo = fechafin - orden.fecha_inicio_operacion;

                            var pausas = context.tpausasorden.Where(x => x.idorden == id_orden).Select(x => new { x.fecha_inicio, x.fecha_fin }).ToList();
                            if (pausas != null)
                                {
                                foreach (var item in pausas)
                                    {

                                    var pausa = item.fecha_fin - item.fecha_inicio;
                                    totalpausas = totalpausas + TimeSpan.Parse(pausa.ToString());

                                    }
                                }

                            var tefectivo = totaltiempo - totalpausas;

                            orden.estadoorden = finalizar;
                            orden.estado = true;
                            orden.tiempototal = totaltiempo.Value.Ticks;
                            orden.tiempopausas = totalpausas.Ticks;
                            orden.tiempoefectivo = tefectivo.Value.Ticks;
                            context.Entry(orden).State = EntityState.Modified;

                            string infoorden = context.tencabezaorden.Where(x => x.id == id_orden).Select(z => z.placa)
                                .FirstOrDefault();
                            int pedidoid = context.vpedido.Where(x => x.planmayor == infoorden).Select(z => z.id)
                                .FirstOrDefault();
                            List<vpedrepuestos> vpedrepuestos = context.vpedrepuestos.Where(x => x.pedido_id == pedidoid).ToList();
                            if (vpedrepuestos != null)
                                {
                                for (int i = 0; i < vpedrepuestos.Count; i++)
                                    {
                                    vpedrepuestos[i].instalado = true;
                                    context.Entry(vpedrepuestos[i]).State = EntityState.Modified;
                                    }
                                }

                            tencabezaorden buscarOT = context.tencabezaorden.Where(x => x.id == id_orden).FirstOrDefault();
                            if (buscarOT != null)
                                {
                                long tallerOrden = context.tallerAveria.Where(x => x.nombre.Contains("orden")).FirstOrDefault().id;
                                long estadoAveria = context.estadoAverias.Where(x => x.nombre.Contains("asignada")).FirstOrDefault().id;
                                long estadoAveriaS = context.estadoAverias.Where(x => x.nombre.Contains("solucion")).FirstOrDefault().id;
                                List<icb_inspeccionvehiculos> buscar = context.icb_inspeccionvehiculos.Where(x => x.idcitataller == buscarOT.idcita && x.insp_solicitar == true
                                && x.estado_averia_id == estadoAveria && x.taller_averia_id == tallerOrden && x.planmayor == buscarOT.placa).ToList();

                                for (int i = 0; i < buscar.Count; i++)
                                    {
                                    int id = buscar[i].insp_id;
                                    icb_inspeccionvehiculos inspeccion = context.icb_inspeccionvehiculos.Find(id);
                                    inspeccion.estado_averia_id = estadoAveriaS;
                                    context.Entry(inspeccion).State = EntityState.Modified;
                                    context.SaveChanges();
                                    }

                                }

                            context.SaveChanges();
                            }
                        catch (Exception ex)
                            {
                            var mensaje = ex.Message;
                            throw;
                            }

                        }

                    //guardo cambio en tracking
                    ttrackingorden track = new ttrackingorden
                        {
                        descripcion = operacion,
                        estado = orden.estadoorden != null ? orden.estadoorden.Value : 16,
                        fecha = DateTime.Now,
                        idorden = orden.id,
                        //verificar sesion cuando se haga esta consulta. deberia ser la persona logueada
                        responsable = orden.idtecnico != null ? orden.idtecnico.Value : orden.userid_creacion
                        };
                    context.ttrackingorden.Add(track);
                    context.SaveChanges();
                    return Json(1);
                    }

                return Json(0);
                }

            return Json(0);
            }

        public ActionResult CotizacionPDF(int? id_orden_trabajo)
            {
            if (id_orden_trabajo != null)
                {
                //busco la orden de trabajo
                tencabezaorden ot = context.tencabezaorden.Where(d => d.id == id_orden_trabajo).FirstOrDefault();
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

                    //busco el técnico
                    PDFCotizacionOT informe = new PDFCotizacionOT
                        {
                        id = ot.id,
                        codigoentrada = ot.codigoentrada != null ? ot.codigoentrada : "",
                        fecha_generacion = ot.fecha.ToString("yyyy/MM/dd", new CultureInfo("is-IS")),
                        idtipodoc = ot.tp_doc_registros.tpdoc_nombre != null ? ot.tp_doc_registros.tpdoc_nombre : "",
                        asesor = ot.users.user_nombre + " " + ot.users.user_apellido,
                        tecnico = nombretecnico != null ? nombretecnico.ToUpper() : "",
                        razoningreso = ot.trazonesingreso1.razoz_ingreso != null ? ot.trazonesingreso1.razoz_ingreso : "",
                        aseguradora = ot.aseguradora != null ? ot.icb_aseguradoras.nombre : "",
                        poliza = ot.aseguradora != null ? !string.IsNullOrWhiteSpace(ot.poliza) ? ot.poliza : "" : "",
                        siniestro = ot.aseguradora != null
                            ? !string.IsNullOrWhiteSpace(ot.siniestro) ? ot.siniestro : ""
                            : "",
                        minimo = ot.aseguradora != null
                            ? ot.minimo != null ? ot.minimo.Value.ToString("N0", new CultureInfo("is-IS")) : ""
                            : "",
                        deducible = ot.aseguradora != null
                            ? ot.deducible != null ? ot.deducible.Value.ToString("N0", new CultureInfo("is-IS")) : ""
                            : "",
                        fecha_soat = ot.aseguradora != null
                            ? ot.fecha_soat != null
                                ?
                                ot.fecha_soat.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                                : ""
                            : "",
                        numero_soat = ot.aseguradora != null
                            ? !string.IsNullOrWhiteSpace(ot.numero_soat) ? ot.numero_soat : ""
                            : "",
                        garantia_falla = !string.IsNullOrWhiteSpace(ot.garantia_falla) ? ot.garantia_falla : "",
                        garantia_causa = !string.IsNullOrWhiteSpace(ot.garantia_causa) ? ot.garantia_causa : "",
                        garantia_solucion = !string.IsNullOrWhiteSpace(ot.garantia_solucion) ? ot.garantia_solucion : "",

                        //datos del vehículo
                        placa = ot.placa != null ? ot.placa : "",
                        modelo = ot.icb_vehiculo.modelo_vehiculo.modvh_nombre != null ? ot.icb_vehiculo.modelo_vehiculo.modvh_nombre : "",
                        //anio_modelo = ot.icb_vehiculo.anio_vh != null ? ot.icb_vehiculo.anio_vh : 0,
                        anio_modelo = ot.icb_vehiculo.anio_vh,
                        serie = ot.icb_vehiculo.vin != null ? ot.icb_vehiculo.vin : "",
                        numero_motor = ot.icb_vehiculo.nummot_vh != null ? ot.icb_vehiculo.nummot_vh : "",
                        color = ot.icb_vehiculo.color_vehiculo.colvh_nombre != null ? ot.icb_vehiculo.color_vehiculo.colvh_nombre : "",
                        fecha_venta = ot.icb_vehiculo.fecha_venta != null
                            ? ot.icb_vehiculo.fecha_venta.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                            : "",
                        fecha_fin_garantia = ot.icb_vehiculo.fecha_fin_garantia != null
                            ? ot.icb_vehiculo.fecha_fin_garantia.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                            : "",
                        fecha_prometida = ot.entrega != null ? ot.entrega.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                        kilometraje = ot.icb_vehiculo.kilometraje.ToString(),
                        kilometraje_actual = ot.kilometraje.ToString("N0", new CultureInfo("is-IS")),
                        bodega = ot.bodega_concesionario.bodccs_nombre,
                        //datos del propietario
                        txtDocumentoCliente = ot.icb_terceros.doc_tercero != null ? ot.icb_terceros.doc_tercero : "",
                        nombrecliente = !string.IsNullOrWhiteSpace(ot.icb_terceros.razon_social)
                            ? ot.icb_terceros.razon_social
                            : ot.icb_terceros.prinom_tercero.ToUpper() + " " + (!string.IsNullOrWhiteSpace(ot.icb_terceros.segnom_tercero)?(ot.icb_terceros.segnom_tercero.ToUpper() + " "):"") +
                              ot.icb_terceros.apellido_tercero.ToUpper() + " " + (!string.IsNullOrWhiteSpace(ot.icb_terceros.segapellido_tercero)?ot.icb_terceros.segapellido_tercero.ToUpper():""),
                        telefonocliente = !string.IsNullOrWhiteSpace(ot.icb_terceros.telf_tercero)
                            ? ot.icb_terceros.telf_tercero
                            : "",
                        celularcliente = !string.IsNullOrWhiteSpace(ot.icb_terceros.celular_tercero)
                            ? ot.icb_terceros.celular_tercero
                            : "",
                        correocliente = !string.IsNullOrWhiteSpace(ot.icb_terceros.email_tercero)
                            ? ot.icb_terceros.email_tercero.ToUpper()
                            : "",
                        ciudadcliente = ot.icb_terceros.ciu_id != null
                            ? context.nom_ciudad.Where(d => d.ciu_id == ot.icb_terceros.ciu_id.Value)
                                .Select(d => d.ciu_nombre).FirstOrDefault()
                            : "",
                        entrega = ot.entrega != null ? ot.entrega.ToString("yyyy/MM/dd", new CultureInfo("en-US")): "",
                        notas = !string.IsNullOrWhiteSpace(ot.notas) ? ot.notas : "",
                        solicitudes = context.tsolicitudorden.Where(d => d.idorden == ot.id).Select(d => new solicitudes
                            { descripcion_solicitud = d.solicitud, respuesta_taller = d.respuesta }).ToList(),
                        id_plan_mantenimiento =
                            ot.id_plan_mantenimiento != null ? ot.tplanmantenimiento.Descripcion : ""

                        /*repuestos_plan = ot.id_plan_mantenimiento != null ?
						context.tdetallerepuestosot.Where(d => d.idorden == ot.id && d.id_plan_mantenimiento == ot.id_plan_mantenimiento).Select(d => new suministrosPlan { }).ToList() : new List<suministrosPlan>(),
						*/
                        };

                    var buscarCliente = context.tercero_cliente.Where(d => d.tercero_id == ot.tercero)
                        .Select(d => new { d.lprecios_repuestos, d.dscto_rep }).FirstOrDefault();

                    if (ot.id_plan_mantenimiento != null)
                        {
                        //listado de operaciones plan
                        List<tdetallemanoobraot> operaciones_plan = context.tdetallemanoobraot.Where(d =>
                            d.idorden == ot.id && d.id_plan_mantenimiento == ot.id_plan_mantenimiento && d.estado == "1").ToList();
                        informe.operaciones_plan = operaciones_plan.Select(x => new operacionesPlan
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
                                      (x.valorunitario * 1 - x.valorunitario * x.pordescuento / 100 * 1) * x.poriva /
                                      100)
                                .ToString(),
                            valor_total2 = x.tiempo != 0
                                ? x.valorunitario * (decimal)x.tiempo -
                                  x.valorunitario * x.pordescuento / 100 * (decimal)x.tiempo +
                                  (x.valorunitario * (decimal)x.tiempo -
                                   x.valorunitario * x.pordescuento / 100 * (decimal)x.tiempo) * x.poriva / 100
                                : x.valorunitario * 1 - x.valorunitario * x.pordescuento / 100 * 1 +
                                  (x.valorunitario * 1 - x.valorunitario * x.pordescuento / 100 * 1) * x.poriva / 100
                            }).ToList();
                        informe.totaltiempooperacionesplan =
                            operaciones_plan.Sum(d => d.tiempo != null ? d.tiempo : 0).ToString();
                        informe.totalvaloroperacionesplan = informe.operaciones_plan
                            .Sum(d => d.valor_total2 != null ? d.valor_total2 : 0).ToString();
                        //listado de repuestos plan
                        List<tdetallerepuestosot> listarepuestosplan = context.tdetallerepuestosot.Where(d =>
                            d.idorden == ot.id && d.id_plan_mantenimiento == ot.id_plan_mantenimiento).ToList();
                        informe.repuestos_plan = listarepuestosplan.Select(d => new suministrosPlan
                            {
                            codigo = d.idrepuesto,
                            descripcion = d.icb_referencia.ref_descripcion,
                            cantidad = d.cantidad.ToString(),
                            precio_unitario = d.valorunitario.ToString("N0", new CultureInfo("is-IS")),
                            iva = Math.Round((d.cantidad * d.valorunitario - Math.Round(
                                                  d.valorunitario *
                                                  (Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) > d.pordescto &&
                                                   Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) <=
                                                   Convert.ToDecimal(d.icb_referencia.por_dscto_max, miCultura)
                                                      ?
                                                      Convert.ToDecimal(buscarCliente.dscto_rep, miCultura)
                                                      :
                                                      Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) < d.pordescto &&
                                                      d.pordescto <= Convert.ToDecimal(d.icb_referencia.por_dscto_max, miCultura)
                                                          ? d.pordescto
                                                          : 0) / 100 * d.cantidad)) * d.poriva / 100)
                                .ToString("N0", new CultureInfo("is-IS")),
                            precio_total = Math.Round(d.valorunitario * d.cantidad - Math.Round(
                                                          d.valorunitario *
                                                          (Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) > d.pordescto &&
                                                           Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) <=
                                                           Convert.ToDecimal(d.icb_referencia.por_dscto_max, miCultura)
                                                              ?
                                                              Convert.ToDecimal(buscarCliente.dscto_rep, miCultura)
                                                              :
                                                              Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) <
                                                              d.pordescto && d.pordescto <= d.pordescto
                                                                  ? d.pordescto
                                                                  : 0) / 100 * d.cantidad)
                                                      + Math.Round((d.cantidad * d.valorunitario - Math.Round(
                                                                        d.valorunitario *
                                                                        (Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) >
                                                                         d.pordescto &&
                                                                         Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) <=
                                                                         Convert.ToDecimal(d.icb_referencia
                                                                             .por_dscto_max, miCultura)
                                                                            ?
                                                                            Convert.ToDecimal(buscarCliente.dscto_rep, miCultura)
                                                                            :
                                                                            Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) <
                                                                            d.pordescto &&
                                                                            d.pordescto <=
                                                                            Convert.ToDecimal(d.icb_referencia
                                                                                .por_dscto_max, miCultura)
                                                                                ? d.pordescto
                                                                                : 0) / 100 * d.cantidad)) * d.poriva /
                                                                   100)).ToString("N0", new CultureInfo("is-IS")),
                            precio_total2 = Math.Round(d.valorunitario * d.cantidad - Math.Round(
                                                           d.valorunitario *
                                                           (Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) > d.pordescto &&
                                                            Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) <=
                                                            Convert.ToDecimal(d.icb_referencia.por_dscto_max, miCultura)
                                                               ?
                                                               Convert.ToDecimal(buscarCliente.dscto_rep, miCultura)
                                                               :
                                                               Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) <
                                                               d.pordescto && d.pordescto <= d.pordescto
                                                                   ? d.pordescto
                                                                   : 0) / 100 * d.cantidad)
                                                       + Math.Round((d.cantidad * d.valorunitario - Math.Round(
                                                                         d.valorunitario *
                                                                         (Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) >
                                                                          d.pordescto &&
                                                                          Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) <=
                                                                          Convert.ToDecimal(d.icb_referencia
                                                                              .por_dscto_max, miCultura)
                                                                             ?
                                                                             Convert.ToDecimal(buscarCliente.dscto_rep, miCultura)
                                                                             :
                                                                             Convert.ToDecimal(buscarCliente
                                                                                 .dscto_rep, miCultura) < d.pordescto &&
                                                                             d.pordescto <=
                                                                             Convert.ToDecimal(d.icb_referencia
                                                                                 .por_dscto_max, miCultura)
                                                                                 ? d.pordescto
                                                                                 : 0) / 100 * d.cantidad)) * d.poriva /
                                                                    100))
                            }).ToList();
                        informe.totaltcantidadrepuestosplan = listarepuestosplan.Sum(d => d.cantidad).ToString();
                        informe.totalvalorrepuestosplan = informe.repuestos_plan
                            .Sum(d => d.precio_total2 != null ? d.precio_total2 : 0).ToString();
                        }

                    //listado de operaciones
                    List<tdetallemanoobraot> operaciones = context.tdetallemanoobraot
                        .Where(d => d.idorden == ot.id /*&& d.id_plan_mantenimiento == null*/ && d.estado == "1").ToList();
                    informe.operaciones = operaciones.Select(x => new operaciones
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
                            .Value.ToString("N0", new CultureInfo("is-IS")),
                        valor_total2 = x.tiempo != 0
                            ? x.valorunitario * (decimal)x.tiempo -
                              x.valorunitario * x.pordescuento / 100 * (decimal)x.tiempo +
                              (x.valorunitario * (decimal)x.tiempo -
                               x.valorunitario * x.pordescuento / 100 * (decimal)x.tiempo) * x.poriva / 100
                            : x.valorunitario * 1 - x.valorunitario * x.pordescuento / 100 * 1 +
                              (x.valorunitario * 1 - x.valorunitario * x.pordescuento / 100 * 1) * x.poriva / 100
                        }).ToList();
                    informe.totaltiempooperaciones = operaciones.Sum(d => d.tiempo != null ? d.tiempo : 0).ToString();
                    informe.totalvaloroperaciones = informe.operaciones
                        .Sum(d => d.valor_total2 != null ? d.valor_total2 : 0).Value.ToString("N0", new CultureInfo("is-IS"));
                    //listado de repuestos
                    List<tdetallerepuestosot> listarepuestos = context.tdetallerepuestosot
                        .Where(d => d.idorden == ot.id /*&& d.id_plan_mantenimiento == null*/ && d.solicitado == true).ToList();
                    informe.repuestos = listarepuestos.Select(d => new repuestos
                        {
                        codigo = d.idrepuesto,
                        descripcion = d.icb_referencia.ref_descripcion,
                        cantidad = d.cantidad.ToString(),
                        precio_unitario = d.valorunitario.ToString("N0", new CultureInfo("is-IS")),
                        iva = Math.Round((d.cantidad * d.valorunitario - Math.Round(
                                              d.valorunitario *
                                              (Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) > d.pordescto &&
                                               Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) <=
                                               Convert.ToDecimal(d.icb_referencia.por_dscto_max, miCultura)
                                                  ?
                                                  Convert.ToDecimal(buscarCliente.dscto_rep, miCultura)
                                                  :
                                                  Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) < d.pordescto &&
                                                  d.pordescto <= Convert.ToDecimal(d.icb_referencia.por_dscto_max, miCultura)
                                                      ? d.pordescto
                                                      : 0) / 100 * d.cantidad)) * d.poriva / 100)
                            .ToString("N0", new CultureInfo("is-IS")),
                        iva2 = Math.Round((d.cantidad * d.valorunitario - Math.Round(
                                              d.valorunitario *
                                              (Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) > d.pordescto &&
                                               Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) <=
                                               Convert.ToDecimal(d.icb_referencia.por_dscto_max, miCultura)
                                                  ?
                                                  Convert.ToDecimal(buscarCliente.dscto_rep, miCultura)
                                                  :
                                                  Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) < d.pordescto &&
                                                  d.pordescto <= Convert.ToDecimal(d.icb_referencia.por_dscto_max, miCultura)
                                                      ? d.pordescto
                                                      : 0) / 100 * d.cantidad)) * d.poriva / 100),
                        descuento = Math.Round((d.cantidad * d.valorunitario *                                           
                                              (Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) > d.pordescto &&
                                               Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) <=
                                               Convert.ToDecimal(d.icb_referencia.por_dscto_max, miCultura)
                                                  ?
                                                  Convert.ToDecimal(buscarCliente.dscto_rep, miCultura)
                                                  :
                                                  Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) < d.pordescto &&
                                                  d.pordescto <= Convert.ToDecimal(d.icb_referencia.por_dscto_max, miCultura)
                                                      ? d.pordescto
                                                      : 0)/100  ))
                            .ToString("N0", new CultureInfo("is-IS")),
                        descuento2 = Math.Round((d.cantidad * d.valorunitario *                                               
                                              (Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) > d.pordescto &&
                                               Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) <=
                                               Convert.ToDecimal(d.icb_referencia.por_dscto_max, miCultura)
                                                  ?
                                                  Convert.ToDecimal(buscarCliente.dscto_rep, miCultura)
                                                  :
                                                  Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) < d.pordescto &&
                                                  d.pordescto <= Convert.ToDecimal(d.icb_referencia.por_dscto_max, miCultura)
                                                      ? d.pordescto
                                                      : 0)/100)),

                        precio_total = Math.Round(d.valorunitario * d.cantidad - Math.Round(
                                                      d.valorunitario *
                                                      (Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) > d.pordescto &&
                                                       Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) <=
                                                       Convert.ToDecimal(d.icb_referencia.por_dscto_max, miCultura)
                                                          ?
                                                          Convert.ToDecimal(buscarCliente.dscto_rep, miCultura)
                                                          :
                                                          Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) < d.pordescto &&
                                                          d.pordescto <= d.pordescto
                                                              ? d.pordescto
                                                              : 0) / 100 * d.cantidad)
                                                  + Math.Round((d.cantidad * d.valorunitario - Math.Round(
                                                                    d.valorunitario *
                                                                    (Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) >
                                                                     d.pordescto &&
                                                                     Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) <=
                                                                     Convert.ToDecimal(d.icb_referencia.por_dscto_max, miCultura)
                                                                        ?
                                                                        Convert.ToDecimal(buscarCliente.dscto_rep, miCultura)
                                                                        :
                                                                        Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) <
                                                                        d.pordescto &&
                                                                        d.pordescto <=
                                                                        Convert.ToDecimal(
                                                                            d.icb_referencia.por_dscto_max, miCultura)
                                                                            ? d.pordescto
                                                                            : 0) / 100 * d.cantidad)) * d.poriva / 100))
                            .ToString("N0", new CultureInfo("is-IS")),
                        precio_total2 = Math.Round(d.valorunitario * d.cantidad - Math.Round(
                                                       d.valorunitario *
                                                       (Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) > d.pordescto &&
                                                        Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) <=
                                                        Convert.ToDecimal(d.icb_referencia.por_dscto_max, miCultura)
                                                           ?
                                                           Convert.ToDecimal(buscarCliente.dscto_rep, miCultura)
                                                           :
                                                           Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) < d.pordescto &&
                                                           d.pordescto <= d.pordescto
                                                               ? d.pordescto
                                                               : 0) / 100 * d.cantidad)
                                                   + Math.Round((d.cantidad * d.valorunitario - Math.Round(
                                                                     d.valorunitario *
                                                                     (Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) >
                                                                      d.pordescto &&
                                                                      Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) <=
                                                                      Convert.ToDecimal(d.icb_referencia
                                                                          .por_dscto_max, miCultura)
                                                                         ?
                                                                         Convert.ToDecimal(buscarCliente.dscto_rep, miCultura)
                                                                         :
                                                                         Convert.ToDecimal(buscarCliente.dscto_rep, miCultura) <
                                                                         d.pordescto &&
                                                                         d.pordescto <=
                                                                         Convert.ToDecimal(d.icb_referencia
                                                                             .por_dscto_max, miCultura)
                                                                             ? d.pordescto
                                                                             : 0) / 100 * d.cantidad)) * d.poriva /
                                                                100))
                        }).ToList();
                    informe.totaltcantidadrepuestos = listarepuestos.Sum(d => d.cantidad).ToString();
                    informe.totalvalorrepuestos = informe.repuestos
                        .Sum(d => d.precio_total2 != null ? d.precio_total2 : 0).Value.ToString("N0", new CultureInfo("is-IS"));

                    string valorDescuento = "";
                    string valorIva = "";
                    decimal valorTotal = 0;

                    valorDescuento = informe.repuestos.Sum(d => d.descuento2).ToString("N0", new CultureInfo("is-IS"));
                    valorIva = informe.repuestos.Sum(d => d.iva2).ToString("N0", new CultureInfo("is-IS"));
                    valorTotal = Convert.ToDecimal(Convert.ToDecimal(informe.totalvaloroperaciones) + Convert.ToDecimal(informe.totalvalorrepuestos));

                    informe.valorDescuento = valorDescuento;
                    informe.valorIva = valorIva;
                    informe.valorTotal = valorTotal.ToString("N0", new CultureInfo("is-IS"));

                    string nombre = "presupuesto_";
                    string nombre2 = nombre;
                    nombre = nombre + "file.pdf";

                    string root = Server.MapPath("~/Pdf/");
                    string pdfname = string.Format("{0}.pdf", Guid.NewGuid().ToString());
                    string path = Path.Combine(root, pdfname);
                    path = Path.GetFullPath(path);
                    //var something = new Rotativa.ViewAsPdf("CotizacionPDF", informe) { /*FileName = "cv.pdf" SaveOnServerPath = path*/ };
                    if (ot.fecha_impresion_cotizacion == null)
                        {
                        ot.fecha_impresion_cotizacion = DateTime.Now;
                        context.Entry(ot).State = EntityState.Modified;
                        context.SaveChanges();
                        }

                    var codigo2 = informe.codigoentrada;
                    /* string customSwitches = string.Format("--print-media-type --allow {0} --header-html {0} --header-spacing -40 ",
                    Url.Action("CabeceraPDF", "ordenTaller", new { codigoentrada = codigo2 }, Request.Url.Scheme));*/


                    string customSwitches = string.Format("--print-media-type --allow {0} --header-html {0} --header-spacing 5 --footer-html {1} --footer-spacing 0",
                    Url.Action("CabeceraPDF", "ordenTaller", new { area = "", codigoentrada = informe.codigoentrada, fecha_generacion = informe.fecha_generacion }, Request.Url.Scheme), Url.Action("PiePDF", "ordenTaller", new { area = "" }, Request.Url.Scheme));

                    ViewAsPdf something = new ViewAsPdf("CotizacionPDF", informe)
                        {
                        PageOrientation = Orientation.Landscape,
                        FileName = nombre,
                        CustomSwitches = customSwitches,
                        PageSize = Size.Letter,
                        PageWidth = 250,
                        PageMargins = new Margins { Top = 40, Bottom = 5 }
                        };

                    return something;
                    //return View(informe);
                    }

                return Json(0, JsonRequestBehavior.AllowGet);
                }

            return Json(0, JsonRequestBehavior.AllowGet);
            //return Json(vehiculos, JsonRequestBehavior.AllowGet); 
            //return View(informe);
            }

        [AllowAnonymous]
        public ActionResult CabeceraPDF(string codigoentrada, string fecha_generacion)
            {
            var recibido = Request;
            var modelo2 = new PDFCotizacionOT
                {
                codigoentrada = codigoentrada,
                fecha_generacion = fecha_generacion,
                };

            return View(modelo2);
            }

        [AllowAnonymous]
        public ActionResult PiePDF()
            {
            return View();
            }


        public ActionResult OrdenTallerPDF(int? id)
            {

            if (id != null)
                {
                tencabezaorden ot = context.tencabezaorden.Where(d => d.id == id).FirstOrDefault();

                string nombretecnico = "";

                if (ot.idtecnico != null)
                    {
                    users tecnico = context.users.Where(d => d.user_id == ot.idtecnico).FirstOrDefault();
                    if (tecnico != null)
                        {
                        nombretecnico = tecnico.user_nombre + " " + tecnico.user_apellido;
                        }
                    }

                var contacto = context.icb_contacto_tercero.Where(d => d.tercero_id == ot.tercero).ToList();

                string contactouno = "", telcontactouno = "", contactodos = "", telcontactodos = "";
                int i = 0;
                foreach (var item in contacto)
                    {
                    switch (i) {
                        case 0:  
                            contactouno = !string.IsNullOrEmpty(item.con_tercero_nombre) ? item.con_tercero_nombre : "";
                            telcontactouno= !string.IsNullOrEmpty(item.con_tercero_telefono) ? item.con_tercero_telefono : "";
                            break;
                        
                        case 1:  
                            contactodos = !string.IsNullOrEmpty(item.con_tercero_nombre) ? item.con_tercero_nombre : "";
                            telcontactodos = !string.IsNullOrEmpty(item.con_tercero_telefono) ? item.con_tercero_telefono : "";
                            break;
                    }

                    i++;

                }


                DateTime fecha_primer_ingreseo = context.tencabezaorden.Where(x => x.placa == ot.placa).OrderBy(p => p.fec_creacion).FirstOrDefault().fec_creacion;

            PdfOrdenTaller informe = new PdfOrdenTaller
                    {

                    id = ot.id,
                    codigoentrada = ot.codigoentrada != null ? ot.codigoentrada : "",
                    idtipodoc = ot.tp_doc_registros.tpdoc_nombre != null ? ot.tp_doc_registros.tpdoc_nombre : "",
                    bodega = ot.bodega_concesionario.bodccs_nombre != null ? ot.bodega_concesionario.bodccs_nombre : "",
                    fecha_generacion = ot.fecha.ToString("yyyy/MM/dd", new CultureInfo("is-IS")),
                    asesor = ot.users.user_nombre + " " + ot.users.user_apellido,
                    tecnico = nombretecnico != null ? nombretecnico : "",
                    razoningreso = ot.trazonesingreso1.razoz_ingreso != null ? ot.trazonesingreso1.razoz_ingreso : "",
                    aseguradora = ot.aseguradora != null ? ot.icb_aseguradoras.nombre : "",
                    poliza = ot.aseguradora != null ? !string.IsNullOrWhiteSpace(ot.poliza) ? ot.poliza : "" : "",
                    siniestro = ot.aseguradora != null
                            ? !string.IsNullOrWhiteSpace(ot.siniestro) ? ot.siniestro : ""
                            : "",

                    fecha_entrada_vh = fecha_primer_ingreseo!= null ? fecha_primer_ingreseo.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                    fecha_ingreso = ot.fec_creacion != null ? ot.fec_creacion.ToString("yyyy/MM/dd HH:mm", new CultureInfo("is-IS")) : "",
                    fecha_prometida =  ot.entrega != null ? ot.entrega.ToString("yyyy/MM/dd", new CultureInfo("is-IS")) : "",
                minimo = ot.aseguradora != null
                            ? ot.minimo != null ? ot.minimo.Value.ToString("N0", new CultureInfo("is-IS")) : ""
                            : "",
                    deducible = ot.aseguradora != null
                            ? ot.deducible != null ? ot.deducible.Value.ToString("N0", new CultureInfo("is-IS")) : ""
                            : "",
                    fecha_soat = ot.aseguradora != null
                            ? ot.fecha_soat != null
                                ?
                                ot.fecha_soat.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                                : ""
                            : "",
                    numero_soat = ot.aseguradora != null
                            ? !string.IsNullOrWhiteSpace(ot.numero_soat) ? ot.numero_soat : ""
                            : "",
                    garantia_falla = !string.IsNullOrWhiteSpace(ot.garantia_falla) ? ot.garantia_falla : "",
                    garantia_causa = !string.IsNullOrWhiteSpace(ot.garantia_causa) ? ot.garantia_causa : "",
                    garantia_solucion = !string.IsNullOrWhiteSpace(ot.garantia_solucion) ? ot.garantia_solucion : "",

                   
                    contacto = contactouno != null ? contactouno : "",
                    numcontacto = telcontactouno != null?  telcontactouno : "",

                    otrocontacto = contactodos != null ? contactodos : "",
                    telotrocontacto = contacto != null ? telcontactodos : "",

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

                    placa = ot.placa != null ? ot.icb_vehiculo.plac_vh  : "",
                    serie = ot.icb_vehiculo.vin != null ? ot.icb_vehiculo.vin : "",
                    numero_motor = ot.icb_vehiculo.nummot_vh != null ? ot.icb_vehiculo.nummot_vh : "",
                    color = ot.icb_vehiculo.color_vehiculo.colvh_nombre != null ? ot.icb_vehiculo.color_vehiculo.colvh_nombre : "",
                    fecha_venta = ot.icb_vehiculo.fecha_venta != null
                            ? ot.icb_vehiculo.fecha_venta.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                            : "",
                    fecha_fin_garantia = ot.icb_vehiculo.fecha_fin_garantia != null
                            ? ot.icb_vehiculo.fecha_fin_garantia.Value.ToString("yyyy/MM/dd", new CultureInfo("is-IS"))
                            : "",
                    kilometraje = ot.icb_vehiculo.kilometraje.ToString(),
                    kilometraje_actual = ot.kilometraje.ToString("N0", new CultureInfo("is-IS")),
                    modelo = ot.icb_vehiculo.modelo_vehiculo.modvh_nombre != null ? ot.icb_vehiculo.modelo_vehiculo.modvh_nombre : "",
                    //anio_modelo = ot.icb_vehiculo.anio_vh != null ? ot.icb_vehiculo.anio_vh : 0,
                    anio_modelo = ot.icb_vehiculo.anio_vh,

                    };


                ot.checlimpresionot = true;
                ot.fechaimpresionot = DateTime.Now;
                context.Entry(ot).State = EntityState.Modified;
                context.SaveChanges();


                List<tsolicitudorden> sintomas = context.tsolicitudorden.Where(d => d.idorden == id).ToList();

                informe.sintomas = sintomas.Select(x => new sintomas
                    {
                    id_solicitud = x.idsolicitud,
                    descripcion_solicitud = x.solicitud != null ? x.solicitud : "",
                    }).ToList();

                string customSwitches = string.Format("--print-media-type --allow {0} --header-html {0} --header-spacing 5 --footer-html {1} --footer-spacing 0",
                Url.Action("CabeceraOrdenPDF", "ordenTaller", new { area = "", codigoentrada = informe.codigoentrada, fecha_generacion = informe.fecha_generacion, placa = informe.placa }, Request.Url.Scheme), Url.Action("PieOrdenPDF", "ordenTaller", new { area = "" }, Request.Url.Scheme));

                ViewAsPdf something = new ViewAsPdf("OrdenTallerPDF", informe)
                    {
                    PageOrientation = Orientation.Landscape,
                    CustomSwitches = customSwitches,
                    PageSize = Size.Letter,
                    PageWidth = 250,
                    PageMargins = new Margins { Top = 40, Bottom = 5 }
                    };

                return something;

                }
            return Json(0, JsonRequestBehavior.AllowGet);
            }



        [AllowAnonymous]
        public ActionResult CabeceraOrdenPDF(string codigoentrada, string fecha_generacion, string placa)
            {
            var recibido = Request;
            var modelo2 = new PdfOrdenTaller
                {
                codigoentrada = codigoentrada,
                fecha_generacion = fecha_generacion,
                placa = placa
                };

            return View(modelo2);
            }

        [AllowAnonymous]
        public ActionResult PieOrdenPDF()
            {
            return View();
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

        public JsonResult BuscarOperacionPorPlan(string id_vehiculo, int? id_plan)
            {
            int? buscarModeloGeneral = (from vehiculo in context.icb_vehiculo
                                        where vehiculo.plan_mayor == id_vehiculo || vehiculo.plac_vh == id_vehiculo
                                        select vehiculo.modelo_vehiculo.modelogkit).FirstOrDefault();

            var buscarOperaciones = (from plan in context.tplanrepuestosmodelo
                                     join tempario in context.ttempario
                                         on plan.tempario equals tempario.codigo into ps
                                     from tempario in ps.DefaultIfEmpty()
                                     join iva in context.codigo_iva
                                         on tempario.iva equals iva.id into ps1
                                     from iva in ps1.DefaultIfEmpty()
                                     where plan.inspeccion == id_plan && plan.modgeneral == buscarModeloGeneral && plan.tipo == "M" &&
                                           tempario != null
                                     select new
                                         {
                                         tempario.codigo,
                                         tempario.operacion,
                                         tempario.tiempomatriz,
                                         tempario.preciomatriz,
                                         iva.porcentaje
                                         }).ToList();
            float tiempo = buscarOperaciones.Sum(d => d.tiempomatriz != null ? d.tiempomatriz.Value : 0);
            decimal preciooperaciones = buscarOperaciones.Sum(d => d.preciomatriz);


            var buscarReferenciasSQL = (from plan in context.tplanrepuestosmodelo
                                        join referencia in context.icb_referencia
                                            on plan.referencia equals referencia.ref_codigo into ps
                                        from referencia in ps.DefaultIfEmpty()
                                        join precios in context.rprecios
                                            on referencia.ref_codigo equals precios.codigo into ps2
                                        from precios in ps2.DefaultIfEmpty()
                                        where plan.inspeccion == id_plan && plan.modgeneral == buscarModeloGeneral && plan.tipo == "R" &&
                                              plan.referencia != null
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
                                            cantidad = plan.cantidad != null ? plan.cantidad : 0
                                            }).ToList();

            var buscarReferencias = buscarReferenciasSQL.Select(x => new
                {
                x.ref_codigo,
                x.ref_descripcion,
                x.precio,
                x.por_iva,
                x.cantidad,
                valorIva = Math.Round(x.precio * (decimal)x.por_iva / 100),
                valorTotal = Math.Round((x.precio + x.precio * (decimal)x.por_iva / 100) * x.cantidad ?? 0)
                });
            decimal preciorepuestos = buscarReferencias.Sum(d => d.valorTotal);
            string valortotal = (preciooperaciones + preciorepuestos).ToString("N0", new CultureInfo("is-IS"));
            return Json(new { buscarOperaciones, buscarReferencias, tiempo, valortotal }, JsonRequestBehavior.AllowGet);
            }

        public ActionResult notificacionOperacion(string id)
            {

            if (id != null)
                {

                CsEncriptar descriptar = new CsEncriptar();

                string iddes = descriptar.deencriptarId(id);
                int num = Convert.ToInt32(iddes);
                List<tdetallemanoobraot> oprins = context.tdetallemanoobraot.Where(x => x.idorden == num && x.estado == null).ToList();


                return View(oprins);
                }

            return View("OperacionNoPermitida");
            }

        public JsonResult CerrarSinOperacion(int id_orden, string observacion)
            {

            tencabezaorden orden = context.tencabezaorden.Where(x => x.id == id_orden).FirstOrDefault();
            var estado = context.tcitasestados.Where(a => a.tipoestado == "C").Select(v => v.id).FirstOrDefault();
            orden.estadoorden = Convert.ToInt32(estado);
            orden.observacion_cancelacion = observacion;
            context.Entry(orden).State = EntityState.Modified;
            context.SaveChanges();
            return Json(0, JsonRequestBehavior.AllowGet);
            }

        public JsonResult envioMensajeTexto(int idins_op)
            {
            listaGeneral dat = (from v in context.tencabezaorden
                                join tr in context.icb_terceros on v.tercero equals tr.tercero_id
                                where v.id == idins_op
                                select new listaGeneral
                                    {
                                    descripcion = v.placa,
                                    codigo = tr.celular_tercero
                                    }).FirstOrDefault();

            HttpClient client = new HttpClient();
            string celu = dat.codigo;
            int resp = 0;
            string dire = Request.Url.Scheme + "://" + Request.Url.Authority;
            string urlApp = dire + "/enviosmsnotfope";
            CsEncriptar encriptar = new CsEncriptar();
            string idescript = encriptar.encriptarId(idins_op);
            string mensaje = "Iceberg le informa: se ha detectado que en el proceso de atención de su vehículo " +
                          dat.descripcion + ", encontramos  reparaciones pendientes por autorizar. Por favor ingrese a la siguiente direccion " + urlApp +
                          "?id=" + idescript;
            string msm = mensaje.Replace(" ", "%20");
            MensajesDeTexto enviar2 = new MensajesDeTexto();
            string status = enviar2.enviarmensaje(celu, msm);
            HttpStatusCode statuscode = HttpStatusCode.OK;
            if (status == "OK")
                {
                tdetallemanoobraot tinspvh = context.tdetallemanoobraot.FirstOrDefault(x => x.id == idins_op);
                if (tinspvh != null)
                    {
                    tinspvh.fecha_envio_sms = DateTime.Now;
                    context.Entry(tinspvh).State = EntityState.Modified;
                    resp = context.SaveChanges();
                    }
                }
            else
                {
                statuscode = HttpStatusCode.InternalServerError;
                }

            return Json(new { status, statuscode, resp }, JsonRequestBehavior.AllowGet);
            }

        public JsonResult confirmarRespuesto(int? txt_insp_op, int? txt_confirmar, string txt_observacion)
            {
            DateTime fec = DateTime.Now;
            //var txt = Request["txt_confirmar1"];
            bool autoriza = txt_confirmar == 1 ? true : false;
            //int insp_op = Convert.ToInt32(Request["txt_insp_op1"]);
            //string observacion = Request["txt_observacion1"];
            string estado = autoriza ? "1" : "0";
            bool error = false;
            tdetallemanoobraot insp = context.tdetallemanoobraot.FirstOrDefault(x => x.id == txt_insp_op);
            if (insp != null)
                {
                using (DbContextTransaction dbTrn = context.Database.BeginTransaction())
                    {
                    try
                        {
                        insp.observacion_sms = txt_observacion;
                        insp.respuesta_sms = autoriza;
                        insp.estado = estado;
                        insp.fecha_envio_sms = DateTime.Now;
                        context.Entry(insp).State = EntityState.Modified;
                        context.SaveChanges();
                        dbTrn.Commit();
                        }
                    catch (Exception ex)
                        {
                        Exception exp = ex;
                        dbTrn.Rollback();
                        error = true;
                        }
                    }
                }

            return Json(new { error, resl = autoriza, fec = fec.ToString(), obs = txt_observacion },
                JsonRequestBehavior.AllowGet);
            }

        public JsonResult DatosHistoricos(string placa, string fecha_desde, string fecha_hasta)
            {



            Expression<Func<tencabezaorden, bool>> predicado = PredicateBuilder.True<tencabezaorden>();
            Expression<Func<tencabezaorden, bool>> predicado2 = PredicateBuilder.False<tencabezaorden>();

            if (!string.IsNullOrWhiteSpace(fecha_desde))
                {
                DateTime fecha = DateTime.Now;
                bool convertir = DateTime.TryParse(fecha_desde, out fecha);
                if (convertir)
                    {
                    predicado = predicado.And(d => d.fec_creacion >= fecha);
                    }
                }

            if (!string.IsNullOrWhiteSpace(fecha_hasta))
                {

                DateTime fecha = DateTime.Now;
                bool convertir = DateTime.TryParse(fecha_hasta, out fecha);
                if (convertir)
                    {
                    fecha = fecha.AddDays(1);
                    predicado = predicado.And(d => d.fec_creacion <= fecha);
                    }
                }
            predicado2 = predicado2.Or(d => d.placa.ToString().Contains(placa));
            predicado = predicado.And(predicado2);


            System.Collections.Generic.List<tencabezaorden> query2 = context.tencabezaorden.OrderByDescending(x => x.fec_creacion).Where(predicado).ToList();


            var dato = query2.Select(x => new
                {
                x.kilometraje,
                fecha = x.fec_creacion != null ? x.fec_creacion.ToString("yyyy/MM/dd", new CultureInfo("en-US")) : "",
                otgenerada = x.codigoentrada != null ? x.codigoentrada : "",
                Codigo_operacion = (from detalle in context.tdetallemanoobraot
                                    join
tempario in context.ttempario on detalle.idtempario equals tempario.codigo
                                    where detalle.idorden == x.id
                                    select new
                                        {
                                        detalle.idtempario,
                                        tempario.operacion
                                        }
                                    ).ToList(),


                sintomas = context.tsolicitudorden.Where(e => e.idorden == x.id).Select(e => new
                    {
                    idsintoma = e.idsolicitud,
                    descripcion = e.solicitud != null ? e.solicitud : "",
                    respuesta = e.respuesta != null ? e.respuesta : ""
                    }).ToList(),

                repuestos = context.tdetallerepuestosot.Where(e => e.idorden == x.id).Select(e => new
                    {
                    idrepuesto = e.id,
                    codigo = e.idrepuesto,
                    descripcion = e.icb_referencia.ref_descripcion,
                    e.cantidad
                    }).ToList(),

                }).ToList();


            return Json(dato, JsonRequestBehavior.AllowGet);

            }

        public JsonResult BahiaCitaOT(int id_orden)
            {

            var dato = (from torder in context.tencabezaorden
                        join tcita in context.tcitastaller on torder.idcita equals tcita.id
                        where torder.id == id_orden
                        select
                        tcita.bahia
                        ).FirstOrDefault();
            return Json(dato, JsonRequestBehavior.AllowGet);
            }

        public JsonResult VerificarEstadoEntregaRepuestos(int id_orden)
            {
            var dato = "";
            int cantidad = 0;
            var query = context.tsolicitudrepuestosot.Where(x => x.idorden == id_orden).Select(f => new { f.recibido, f.idrepuesto }).ToList();

            foreach (var item in query)
                {
                if (item.recibido == false)
                    {
                    cantidad++;
                    }

                }
            dato = cantidad > 0 ? "No" : "Si";

            return Json(dato, JsonRequestBehavior.AllowGet);
            }


        public ActionResult retirosinternos()
            {
            return View();
            }

        public JsonResult datosautorizar()
            {
            string valortipo = context.icb_sysparameter.Where(x => x.syspar_cod == "P147").Select(a => a.syspar_value).FirstOrDefault();
            string rolAdmin = context.icb_sysparameter.Where(x => x.syspar_cod == "P109").Select(a => a.syspar_value).FirstOrDefault();
            int rol_admin = Convert.ToInt32(rolAdmin);
            int roluser = Convert.ToInt32(Session["user_rolid"]);
            int userid = Convert.ToInt32(Session["user_usuarioid"]);
            int valor = Convert.ToInt32(valortipo);
            var Autorizaciones = context.vw_autorizacionesot.Select(x => new
                {
                x.bodccs_nombre,
                x.id,
                x.numero,
                x.nombre,
                x.doc_tercero,
                x.placa,
                x.plan_mayor,
                x.origen,
                x.responsable,

                valor = x.origen == "op" ? (from detalle in context.tdetallemanoobraot
                                            join tempa in context.ttempario
                                            on detalle.idtempario equals tempa.codigo
                                            where detalle.idorden == x.id && detalle.tipotarifa == valor
                                            select
                                           tempa.costo * detalle.tiempo
                                                   ).Sum() : x.origen == "rv" ? (from refmovdet in context.icb_referencia_movdetalle
                                                                                 join refmov in context.icb_referencia_mov on
                                                                                refmovdet.refmov_id equals refmov.refmov_id
                                                                                 where refmovdet.refmov_id == x.id && x.origen == "rv"
                                                                                 && refmovdet.tipotarifa == valor
                                                                                 select refmovdet.valor_unitario * refmovdet.refdet_cantidad
                                                                               ).Sum() : (from vp in context.vpedrepuestos
                                                                                          where vp.pedido_id == x.id && x.origen == "re" &&
                                                                                          vp.tipotarifa == valor && vp.estado == true
                                                                                          select vp.vrtotal
                                                             ).Sum()


                }).ToList();


            var dato = Autorizaciones.GroupBy(x => new {  x.id, x.numero, x.bodccs_nombre, x.doc_tercero, x.nombre, x.placa, x.plan_mayor, x.valor, x.origen, x.responsable }).
                Select(x => new
                    {
                    bodega = x.Key.bodccs_nombre,
                    ot = x.Key.id,
                    numero = x.Key.numero,
                    x.Key.nombre,
                    cedula = x.Key.doc_tercero,
                    planm = x.Key.plan_mayor != null ? x.Key.plan_mayor : "",
                    placa = x.Key.placa != null ? x.Key.placa : "",
                    valor = x.Key.valor != null ? x.Key.valor : 0,
                    x.Key.origen,
                    operacion = context.tdetallemanoobraot.Where(s => s.idorden == x.Key.id && s.tipotarifa == valor && x.Key.origen == "op").Select(d => d.ttempario.operacion).ToList(),
                    repuesto = (from vp in context.vpedrepuestos
                                join re in context.icb_referencia
                                on vp.referencia equals re.ref_codigo
                                where vp.pedido_id == x.Key.id && x.Key.origen == "re"
                                && vp.tipotarifa == valor && vp.estado == true
                                group re.ref_descripcion by new { re.ref_descripcion } into descrip
                                select
                              descrip
                            ).ToList(),
                    repuestoven = (from refmevdet in context.icb_referencia_movdetalle
                                   join re in context.icb_referencia
                                   on refmevdet.ref_codigo equals re.ref_codigo
                                   where refmevdet.refmov_id == x.Key.id && x.Key.origen == "rv" &&
                                   refmevdet.tipotarifa == valor
                                   select
                               re.ref_descripcion
                               ).ToList(),
                    x.Key.responsable
                    });

            if (roluser != rol_admin)
                {
                dato = dato.Where(x => x.responsable == userid);
                }

            return Json(dato, JsonRequestBehavior.AllowGet);
            }

        public JsonResult datosdeot(int id_orden)
            {

            try
                {
                string rolAdmin = context.icb_sysparameter.Where(x => x.syspar_cod == "P109").Select(a => a.syspar_value).FirstOrDefault();
                int rol_admin = Convert.ToInt32(rolAdmin);
                int roluser = Convert.ToInt32(Session["user_rolid"]);
                int userid = Convert.ToInt32(Session["user_usuarioid"]);

                var datos = (from a in context.tencabezaorden
                             join b in context.bodega_concesionario on a.bodega equals b.id
                             join c in context.icb_terceros on a.tercero equals c.tercero_id
                             join d in context.icb_vehiculo on a.placa equals d.plan_mayor
                             where a.numero == id_orden
                             select new
                                 {
                                 b.bodccs_nombre,
                                 a.id,
                                 a.numero,
                                 c.doc_tercero,
                                 nombre = c.prinom_tercero + " " + c.segnom_tercero + " " + c.apellido_tercero + " " + c.segapellido_tercero,
                                 a.placa,
                                 d.plan_mayor
                                 }
                       ).FirstOrDefault();
                string valortipo = context.icb_sysparameter.Where(x => x.syspar_cod == "P147").Select(a => a.syspar_value).FirstOrDefault();
                int valor = Convert.ToInt32(valortipo);
                var operaciones = (from d in context.tdetallemanoobraot
                                   join e in context.ttempario on d.idtempario equals e.codigo
                                   join f in context.centro_costo on d.idcentro equals f.centcst_id
                                   where d.idorden == datos.id && d.tipotarifa == valor && d.respuestainterno == null
                                   select new
                                       {
                                       e.operacion,
                                       d.id,
                                       valor_total = d.tiempo * e.costo,
                                       f.idresponsdable
                                       }).ToList();
               var  operacion = operaciones.Select(x => new
                    {
                    x.operacion,
                    x.id,
                    x.idresponsdable,
                    valor_total = x.valor_total != null ? x.valor_total : 0
                    }).ToList();


                if (roluser != rol_admin)
                    {
                    var opera = operacion.Where(x => x.idresponsdable == userid).Count();
                    if (opera == 0)
                        {
                        return Json("", JsonRequestBehavior.AllowGet);
                        }


                    }


                return Json(new { datos, operacion }, JsonRequestBehavior.AllowGet);
                }
            catch (Exception ex)
                {

                throw ex;
                }

            }

        public JsonResult datosdeped(int id)
            {

            try
                {

                string rolAdmin = context.icb_sysparameter.Where(x => x.syspar_cod == "P109").Select(a => a.syspar_value).FirstOrDefault();
                int rol_admin = Convert.ToInt32(rolAdmin);
                int roluser = Convert.ToInt32(Session["user_rolid"]);
                int userid = Convert.ToInt32(Session["user_usuarioid"]);
                var datos = (from a in context.vpedido
                             join b in context.bodega_concesionario on a.bodega equals b.id
                             join c in context.icb_terceros on a.nit equals c.tercero_id
                             join d in context.icb_vehiculo on a.planmayor equals d.plan_mayor
                             where a.numero == id
                             select new
                                 {
                                 b.bodccs_nombre,
                                 a.id,
                                 a.numero,
                                 c.doc_tercero,
                                 nombre = c.prinom_tercero + " " + c.segnom_tercero + " " + c.apellido_tercero + " " + c.segapellido_tercero,
                                 placa = d.plac_vh != null ? d.plac_vh : "",
                                 d.plan_mayor
                                 }
                       ).FirstOrDefault();

                int tipotarifa = Convert.ToInt32(context.icb_sysparameter.Where(s => s.syspar_cod == "P147").Select(z => z.syspar_value).FirstOrDefault());

                var repuestos = (from d in context.vpedrepuestos
                                 join e in context.icb_referencia on d.referencia equals e.ref_codigo
                                 join f in context.centro_costo on d.idcentro equals f.centcst_id
                                 where d.pedido_id == datos.id && d.tipotarifa == tipotarifa && d.respuestaInterna == null
                                 && d.estado == true
                                 select new
                                     {
                                     e.ref_descripcion,
                                     d.id,
                                     cantidad = d.cantidad,
                                     valor_total = d.vrtotal,
                                     f.idresponsdable,
                                     d.descuento,
                                     d.iva,
                                     d.vrunitario
                                     }).ToList();

                if (roluser != rol_admin)
                    {
                    var repues = repuestos.Where(x => x.idresponsdable == userid).Count();
                    if (repues == 0)
                        {
                        return Json("", JsonRequestBehavior.AllowGet);
                        }
                    }




                return Json(new { datos, repuestos }, JsonRequestBehavior.AllowGet);
                }
            catch (Exception ex)
                {

                throw ex;
                }

            }


        public JsonResult datosdepedreferencia(int id)
            {

            try
                {
                string rolAdmin = context.icb_sysparameter.Where(x => x.syspar_cod == "P109").Select(a => a.syspar_value).FirstOrDefault();
                int rol_admin = Convert.ToInt32(rolAdmin);
                int roluser = Convert.ToInt32(Session["user_rolid"]);
                int userid = Convert.ToInt32(Session["user_usuarioid"]);
                var datos = (from a in context.icb_referencia_mov
                             join b in context.bodega_concesionario on a.bodega_id equals b.id
                             join c in context.icb_terceros on a.cliente equals c.tercero_id
                             where a.refmov_numero == id
                             select new
                                 {
                                 b.bodccs_nombre,
                                 id = a.refmov_id,
                                 numero  = a.refmov_numero,
                                 c.doc_tercero,
                                 nombre = c.prinom_tercero + " " + c.segnom_tercero + " " + c.apellido_tercero + " " + c.segapellido_tercero,
                                 placa = "",
                                 plan_mayor = ""
                                 }
                       ).FirstOrDefault();

                int tipotarifa = Convert.ToInt32(context.icb_sysparameter.Where(s => s.syspar_cod == "P147").Select(z => z.syspar_value).FirstOrDefault());

                var repuestos = (from d in context.icb_referencia_movdetalle
                                 join e in context.icb_referencia on d.ref_codigo equals e.ref_codigo
                                 join f in context.centro_costo on d.idcentro equals f.centcst_id
                                 where d.refmov_id == datos.id && d.tipotarifa == tipotarifa && d.respuestaInterna == null
                                 && d.estado == true
                                 select new
                                     {
                                     e.ref_descripcion,
                                     id = d.refdet_id,
                                     cantidad = d.refdet_cantidad,
                                     valor_total = d.refdet_cantidad * d.valor_unitario,
                                     f.idresponsdable
                                     }).ToList();
                if (roluser != rol_admin)
                    {
                    var repues = repuestos.Where(x => x.idresponsdable == userid).Count();
                    if (repues == 0)
                        {
                        return Json("", JsonRequestBehavior.AllowGet);
                        }
                    }



                return Json(new { datos, repuestos }, JsonRequestBehavior.AllowGet);
                }
            catch (Exception ex)
                {

                throw ex;
                }

            }

        public JsonResult Respuestaautoopera(int idordendt, int respuesta, string onservacion)
            {
            tdetallemanoobraot detalle = context.tdetallemanoobraot.Where(x => x.id == idordendt).FirstOrDefault();

            var valorres = respuesta == 1 ? true : false;
            if (valorres)
                {
                detalle.respuestainterno = valorres;


                }
            else
                {
                detalle.respuestainterno = valorres;
                detalle.observacioninterva = onservacion;


                }
            context.Entry(detalle).State = EntityState.Modified;
            context.SaveChanges();

            return Json("Operacion autorizada", JsonRequestBehavior.AllowGet);
            }

        public JsonResult Respuestaautorep(int id, int respuesta, string onservacion)
            {
            vpedrepuestos detalle = context.vpedrepuestos.Where(x => x.id == id).FirstOrDefault();

            var valorres = respuesta == 1 ? true : false;
            if (valorres)
                {
                detalle.respuestaInterna = valorres;


                }
            else
                {
                detalle.respuestaInterna = valorres;
                detalle.observacionresinterva = onservacion;


                }
            context.Entry(detalle).State = EntityState.Modified;
            context.SaveChanges();

            return Json("Operacion autorizada", JsonRequestBehavior.AllowGet);
            }

        public JsonResult Respuestaautorepvent(int id, int respuesta, string onservacion)
            {
            icb_referencia_movdetalle detalle = context.icb_referencia_movdetalle.Where(x => x.refdet_id == id).FirstOrDefault();

            var valorres = respuesta == 1 ? true : false;
            if (valorres)
                {
                detalle.respuestaInterna = valorres;


                }
            else
                {
                detalle.respuestaInterna = valorres;
                detalle.observacionresinterva = onservacion;


                }
            context.Entry(detalle).State = EntityState.Modified;
            context.SaveChanges();

            return Json("Operacion autorizada", JsonRequestBehavior.AllowGet);
            }


        public JsonResult autorizacionescreadas()
            {
            string valortipo = context.icb_sysparameter.Where(x => x.syspar_cod == "P147").Select(a => a.syspar_value).FirstOrDefault();
            int valor = Convert.ToInt32(valortipo);
            var Autorizaciones = context.vw_autorizacionesotver.Select(x => new
                {
                x.bodccs_nombre,
                x.id,
                x.doc_tercero,
                x.nombre,
                x.placa,
                x.plan_mayor,
                operacion = (from detalle in context.tdetallemanoobraot
                             join tempa in context.ttempario
                             on detalle.idtempario equals tempa.codigo
                             where detalle.idorden == x.id && detalle.tipotarifa == valor
                             select
                             tempa.operacion).ToList(),
                repuestos = (from rep in context.tdetallerepuestosot
                             where rep.idorden == x.id && rep.tipotarifa == valor
                             select rep).Count(),

                valor = (from detalle in context.tdetallemanoobraot
                         join tempa in context.ttempario
                         on detalle.idtempario equals tempa.codigo
                         where detalle.idorden == x.id && detalle.tipotarifa == valor
                         select
                        tempa.costo * detalle.tiempo).Sum(),
                repuesto = (from vp in context.vpedrepuestos
                            join re in context.icb_referencia
                            on vp.referencia equals re.ref_codigo
                            where vp.pedido_id == x.id && x.origen == "re"
                            select
                            re.ref_descripcion
                            ).ToList(),
                repuestoicb = (from vp in context.lineas_documento
                               join re in context.icb_referencia
                               on vp.codigo equals re.ref_codigo
                               where vp.id_encabezado == x.id && x.origen == "rv"
                               select
                               re.ref_descripcion
                            ).ToList(),
                x.origen
                }).ToList();


            var dato = Autorizaciones.GroupBy(x => new { x.bodccs_nombre, x.id, x.operacion, x.doc_tercero, x.nombre, x.placa, x.plan_mayor, x.valor, x.repuestos, x.repuesto, x.repuestoicb, x.origen }).
                Select(x => new
                    {
                    bodega = x.Key.bodccs_nombre,
                    ot = x.Key.id,
                    cedula = x.Key.doc_tercero,
                    x.Key.nombre,
                    planm = x.Key.plan_mayor != null ? x.Key.plan_mayor : "",
                    x.Key.operacion,
                    x.Key.repuestos,
                    x.Key.placa,
                    valor = x.Key.valor != null ? x.Key.valor : 0,
                    x.Key.repuesto,
                    x.Key.repuestoicb,
                    x.Key.origen
                    });

            return Json(dato, JsonRequestBehavior.AllowGet);
            }


        public ActionResult Pdfautorizacionot(int idorden)
            {
            var ot = context.tencabezaorden.Where(x => x.id == idorden).FirstOrDefault();
            int facturado = Convert.ToInt32(Session["user_usuarioid"]);
            var nombrefa = context.users.Where(x => x.user_id == facturado).Select(s => new { nombre = s.user_nombre + " " + s.user_apellido }).FirstOrDefault();
            var ciudad = context.nom_ciudad.Where(x => x.ciu_id == ot.icb_terceros.ciu_id).Select(x => x.ciu_nombre).FirstOrDefault();
            var empresa = context.tablaempresa.FirstOrDefault();

            PDFRetiroInterno retiroop = new PDFRetiroInterno
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
                Asesor =  ot.users.user_nombre + " " + ot.users.user_apellido,
                Operaciones = context.tdetallemanoobraot.Where(x => x.idorden == idorden).ToList(),
                Repuestos = context.tdetallerepuestosot.Where(x => x.idorden == idorden).ToList()


                };
            int swclasifica = Convert.ToInt32(context.icb_sysparameter.Where(z => z.syspar_cod == "P148").Select(x => x.syspar_value).FirstOrDefault());

            var dicodigo = context.encab_documento.Where(x => x.orden_taller == idorden && x.tp_doc_registros.tp_doc_sw.sw == swclasifica).Select(s => new { codigo = s.tp_doc_registros.prefijo + "-" + s.numero }).FirstOrDefault();

            string nombre = "RetiroInternoDI_";
            nombre = nombre + "file.pdf";
            string bodega = ot.bodega_concesionario.bodccs_nombre;
            string customSwitches = string.Format("--print-media-type --allow {0} --header-html {0} --header-spacing 5 --footer-html {1} --footer-spacing 0",
                    Url.Action("CabeceraRetiroindiPDF", "ordenTaller", new { codigoentrada = dicodigo.codigo, bodega = bodega }, Request.Url.Scheme), Url.Action("PiePDF", "ordenTaller", new { area = "" }, Request.Url.Scheme));

            ViewAsPdf something = new ViewAsPdf("RetiroInternoPDF", retiroop)
                {
                PageOrientation = Orientation.Landscape,
                CustomSwitches = customSwitches,
                FileName = nombre,
                PageSize = Size.Letter,
                PageMargins = new Margins { Top = 40, Bottom = 5 }
                };



            return something;
            }


        public ActionResult CabeceraRetiroindiPDF(string codigoentrada, string bodega)
            {
            var recibido = Request;
            var fecha = DateTime.Now;
            var modelo2 = new PDFRetiroInterno
                {
                CodigoRetiro = codigoentrada,
                Fechadocumento = fecha.ToString("yyyy-MM-dd"),
                Bodega = bodega

                };

            return View(modelo2);
            }


        public ActionResult pdfrepuestosretiro(int idorden)
            {

            var ot = context.tencabezaorden.Where(x => x.id == idorden).FirstOrDefault();
            int facturado = Convert.ToInt32(Session["user_usuarioid"]);
            var nombrefa = context.users.Where(x => x.user_id == facturado).Select(s => new { nombre = s.user_nombre + " " + s.user_apellido }).FirstOrDefault();
            var ciudad = context.nom_ciudad.Where(x => x.ciu_id == ot.icb_terceros.ciu_id).Select(x => x.ciu_nombre).FirstOrDefault();
            var empresa = context.tablaempresa.FirstOrDefault();
            PDFRetiroInterno retiroRep = new PDFRetiroInterno
                {
                Aseguradoa = empresa.nombre_empresa,
                Direccion = empresa.direccion,
                Telefono = empresa.telefono,
                Documentocliente = empresa.nit,
                Ciudad = "",
                Bodega = ot.bodega_concesionario.bodccs_nombre,
                Placa = ot.icb_vehiculo.plac_vh != null ? ot.icb_vehiculo.plac_vh : "",
                Vehiculo = ot.icb_vehiculo.modelo_vehiculo.modvh_nombre != null ? ot.icb_vehiculo.modelo_vehiculo.modvh_nombre : "",
                Serie = ot.icb_vehiculo.vin != null ? ot.icb_vehiculo.vin : "",
                Modelo = ot.icb_vehiculo.anio_vh.ToString() != null ? ot.icb_vehiculo.anio_vh.ToString() : "",
                Kilometraje = ot.kilometraje.ToString() != null ? ot.kilometraje.ToString() : "",
                OrdenT = ot.id.ToString(),
                Facturadopor = nombrefa.nombre.ToString(),
                //Asesor = ot.asesor != null ? ot.users.user_nombre + " " + ot.users.user_apellido : "",
                Asesor = ot.users.user_nombre + " " + ot.users.user_apellido,
                Operaciones = context.tdetallemanoobraot.Where(x => x.idorden == idorden).ToList(),
                Repuestos = context.tdetallerepuestosot.Where(x => x.idorden == idorden).ToList()


                };
            int swclasifica = Convert.ToInt32(context.icb_sysparameter.Where(z => z.syspar_cod == "P149").Select(x => x.syspar_value).FirstOrDefault());

            var dicodigo = context.encab_documento.Where(x => x.orden_taller == idorden && x.tp_doc_registros.tp_doc_sw.sw == swclasifica).Select(s => new { codigo = s.tp_doc_registros.prefijo + "-" + s.numero }).FirstOrDefault();

            string nombre = "RetiroInternoRI_";
            nombre = nombre + "file.pdf";
            string bodega = ot.bodega_concesionario.bodccs_nombre;
            string customSwitches = string.Format("--print-media-type --allow {0} --header-html {0} --header-spacing 5 --footer-html {1} --footer-spacing 0",
                    Url.Action("CabeceraRetiroRIPDF", "ordenTaller", new { codigoentrada = dicodigo.codigo, bodega = bodega, aseguradoa = empresa.nombre_empresa, dir = empresa.direccion, tel = empresa.telefono, nit = empresa.nit }, Request.Url.Scheme), Url.Action("PiePDF", "ordenTaller", new { area = "" }, Request.Url.Scheme));

            ViewAsPdf something = new ViewAsPdf("RetiroInternoRIPDF", retiroRep)
                {
                PageOrientation = Orientation.Landscape,
                CustomSwitches = customSwitches,
                FileName = nombre,
                PageSize = Size.Letter,
                PageMargins = new Margins { Top = 40, Bottom = 5 }
                };



            return something;
            }

        public ActionResult CabeceraRetiroRIPDF(string codigoentrada, string bodega, string aseguradoa, string dir, string tel, string nit)
            {
            var recibido = Request;
            var fecha = DateTime.Now;
            var modelo2 = new PDFRetiroInterno
                {
                CodigoRetiro = codigoentrada,
                Fechadocumento = fecha.ToString("yyyy-MM-dd"),
                Bodega = bodega,
                Aseguradoa = aseguradoa,
                Direccion = dir,
                Telefono = tel,
                Documentocliente = nit
                };

            return View(modelo2);
            }


        public ActionResult pdfrepuestosRIped(int id)
            {

            var ot = context.vpedido.Where(x => x.id == id).FirstOrDefault();
            int facturado = Convert.ToInt32(Session["user_usuarioid"]);
            var nombrefa = context.users.Where(x => x.user_id == facturado).Select(s => new { nombre = s.user_nombre + " " + s.user_apellido }).FirstOrDefault();
            var ciudad = context.nom_ciudad.Where(x => x.ciu_id == ot.icb_terceros.ciu_id).Select(x => x.ciu_nombre).FirstOrDefault();
            var empresa = context.tablaempresa.FirstOrDefault();
            PDFRetiroInterno_ped retiroRep = new PDFRetiroInterno_ped
                {
                Aseguradoa = empresa.nombre_empresa,
                Direccion = empresa.direccion,
                Telefono = empresa.telefono,
                Documentocliente = empresa.nit,
                Ciudad = "",
                Bodega = ot.bodega_concesionario.bodccs_nombre,
                Placa = ot.icb_vehiculo.plac_vh != null ? ot.icb_vehiculo.plac_vh : "",
                Vehiculo = ot.icb_vehiculo.modelo_vehiculo.modvh_nombre != null ? ot.icb_vehiculo.modelo_vehiculo.modvh_nombre : "",
                Serie = ot.icb_vehiculo.vin != null ? ot.icb_vehiculo.vin : "",
                Modelo = ot.icb_vehiculo.anio_vh.ToString() != null ? ot.icb_vehiculo.anio_vh.ToString() : "",
                Kilometraje = ot.icb_vehiculo.kilometraje.ToString() != null ? ot.icb_vehiculo.kilometraje.ToString() : "",
                OrdenT = ot.id.ToString(),
                Facturadopor = nombrefa.nombre.ToString(),
                Asesor = ot.vendedor != null ? ot.users.user_nombre + " " + ot.users.user_apellido : "",
                Repuestos = context.vpedrepuestos.Where(x => x.pedido_id == id).ToList()


                };
            int swclasifica = Convert.ToInt32(context.icb_sysparameter.Where(z => z.syspar_cod == "P149").Select(x => x.syspar_value).FirstOrDefault());

            var dicodigo = context.encab_documento.Where(x => x.id_pedido_vehiculo == id && x.tp_doc_registros.tp_doc_sw.sw == swclasifica).Select(s => new { codigo = s.tp_doc_registros.prefijo + "-" + s.numero }).FirstOrDefault();

            string nombre = "RetiroInternoRI_";
            nombre = nombre + "file.pdf";
            string bodega = ot.bodega_concesionario.bodccs_nombre;
            string customSwitches = string.Format("--print-media-type --allow {0} --header-html {0} --header-spacing 5 --footer-html {1} --footer-spacing 0",
                    Url.Action("CabeceraRetiroRIPDF", "ordenTaller", new { codigoentrada = dicodigo.codigo, bodega = bodega, aseguradoa = empresa.nombre_empresa, dir = empresa.direccion, tel = empresa.telefono, nit = empresa.nit }, Request.Url.Scheme), Url.Action("PiePDF", "ordenTaller", new { area = "" }, Request.Url.Scheme));

            ViewAsPdf something = new ViewAsPdf("RetiroInternoRIPDF_ped", retiroRep)
                {
                PageOrientation = Orientation.Landscape,
                CustomSwitches = customSwitches,
                FileName = nombre,
                PageSize = Size.Letter,
                PageMargins = new Margins { Top = 40, Bottom = 5 }
                };



            return something;
            }


        public ActionResult CreacionTOT(int? id = 0)
            {

            tencabezaorden orderot = context.tencabezaorden.Where(x => x.id == id).FirstOrDefault();



            string valortpdoc = context.icb_sysparameter.Where(x => x.syspar_cod == "P154").Select(a => a.syspar_value).FirstOrDefault();
            int valordoc = Convert.ToInt32(valortpdoc);

            var tpdoc = context.tp_doc_registros.Where(x => x.tp_doc_sw.sw == valordoc).Select(x => new { x.tpdoc_id, x.tpdoc_nombre }).ToList();

            ViewBag.tipo = new SelectList(tpdoc, "tpdoc_id", "tpdoc_nombre");

            int idUsuario = Convert.ToInt32(Session["user_usuarioid"]);
            var bodegasUsuario = (from bodegaUsuario in context.bodega_usuario
                                  join bodega in context.bodega_concesionario
                                      on bodegaUsuario.id_bodega equals bodega.id
                                  where bodegaUsuario.id_usuario == idUsuario
                                  select new { bodega.id, bodega.bodccs_nombre }).ToList();



            string valortipo = context.icb_sysparameter.Where(x => x.syspar_cod == "P111").Select(a => a.syspar_value).FirstOrDefault();
            int valor = Convert.ToInt32(valortipo);

            var ot = context.tencabezaorden.Where(x => x.estadoorden != valor).Select(c => new { c.id, c.codigoentrada }).ToList();

            if (orderot != null)
                {
                ViewBag.numot = new SelectList(ot, "id", "codigoentrada", orderot.id);
                ViewBag.Bodega = new SelectList(bodegasUsuario, "id", "bodccs_nombre", orderot.bodega);

                }
            else
                {
                ViewBag.numot = new SelectList(ot, "id", "codigoentrada");
                ViewBag.Bodega = new SelectList(bodegasUsuario, "id", "bodccs_nombre");

                }

            var provedores = from pro in context.tercero_proveedor
                             join ter in context.icb_terceros
                                 on pro.tercero_id equals ter.tercero_id
                             select new
                                 {
                                 idTercero = pro.prtercero_id,
                                 nombreTErcero = ter.prinom_tercero,
                                 apellidosTercero = ter.apellido_tercero,
                                 razonSocial = ter.razon_social
                                 };
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in provedores)
                {
                string nombre = item.nombreTErcero + " " + item.apellidosTercero + " " + item.razonSocial;
                items.Add(new SelectListItem { Text = nombre, Value = item.idTercero.ToString() });
                }
            int user_id = Convert.ToInt32(Session["user_usuarioid"]);

            ViewBag.nombreusuario = context.users.Where(x => x.user_id == user_id).Select(x => x.user_nombre + " " + x.user_apellido).FirstOrDefault();


            var fpago_d = context.fpago_tercero.Select(x => new { x.fpago_id, x.fpago_nombre }).ToList();
            ViewBag.fpago = new SelectList(fpago_d, "fpago_id", "fpago_nombre");
            ViewBag.proveedor = items;


            ViewBag.tipotarifa = new SelectList(context.ttarifatot.Where(x => x.estado == true).Select(x => new { x.id, x.nombre_tarifa }), "id", "nombre_tarifa");
            return View();
            }


        public JsonResult Creartot(int tipo, int Bodega, int perfil, int fpago, int numerofactura, DateTime fecha, int numot, int tipotarifa, int proveedor,DateTime fechafactura)
            {
            int user_id = Convert.ToInt32(Session["user_usuarioid"]);

            tordentot ordentot = new tordentot();
            ordentot.tipo = tipo;
            ordentot.bodega = Bodega;
            ordentot.fecha = fecha;
            ordentot.tipo = tipo;
            ordentot.numerofactura = numerofactura;
            ordentot.condicionpago = fpago;
            ordentot.numot = numot;
            ordentot.tarifa = tipotarifa;
            ordentot.usuariocreacion = user_id;
            ordentot.fechacreacion = DateTime.Now;
            ordentot.idproveedor = proveedor;
            ordentot.perfil = perfil;
            ordentot.fecfactura = fechafactura;
            context.tordentot.Add(ordentot);
            context.SaveChanges();

            return Json(ordentot.id, JsonRequestBehavior.AllowGet);
            }


        public JsonResult Crearoperaciontot(int idtot, string operacion, decimal vrunitarioop, decimal descuentoop = 0, decimal iva = 0)
            {

            titemstot operaciontot = new titemstot();
            operaciontot.idordentot = idtot;
            operaciontot.Descripcion = operacion;
            operaciontot.Valorunitario = Convert.ToDecimal(vrunitarioop, miCultura);
            operaciontot.descuento = descuentoop;
            operaciontot.iva = iva;
            operaciontot.estado = true;
            operaciontot.Tipoitem = 1;
            context.titemstot.Add(operaciontot);
            context.SaveChanges();

            var listaoperaciones = context.titemstot.Where(x => x.idordentot == idtot && x.estado == true && x.Tipoitem == 1).Select(x =>
               new
                   {
                   id = x.id,
                   vliva = x.iva,
                   vrunitario = x.Valorunitario,
                   descripcion = x.Descripcion,
                   vrdescuento = x.descuento

                   }
            ).ToList();

            var datos = listaoperaciones.Select(x => new
                {
                x.id,
                x.descripcion,
                x.vliva,
                x.vrdescuento,
                vrunitario = x.vrunitario.Value.ToString("N2", new CultureInfo("is-IS")),
                vlunitario = x.vrunitario

                }).ToList();

            return Json(datos, JsonRequestBehavior.AllowGet);
            }


        public JsonResult Calculartot(int idtot)
            {

            decimal subtotal = 0, total = 0, totaliva = 0, subsiniva = 0, iva = 0, descuento = 0, totaldesc = 0, valorADD = 0, valorunitario = 0, valor = 0, valordes = 0;
            var idtotdetalles = context.titemstot.Where(x => x.idordentot == idtot && x.estado == true).ToList();

            foreach (var item in idtotdetalles)
                {
                if (item.Tipoitem == 1)
                    {
                    if (item.iva > 0)
                        {
                        valor = (Convert.ToDecimal(item.iva, miCultura) / 100) + 1;
                        valorADD = Convert.ToDecimal(item.Valorunitario, miCultura) / valor;
                        iva = (Convert.ToDecimal(item.iva, miCultura) / 100) * valorADD;
                        }
                    else
                        {
                        valorADD = Convert.ToDecimal(item.Valorunitario, miCultura);
                        iva = 0;
                        }
                    if (item.descuento > 0)
                        {
                        valordes = (Convert.ToDecimal(item.descuento, miCultura) / 100);
                        valordes = 1 - valordes;
                        valorunitario = valorADD / valordes;
                        descuento = (Convert.ToDecimal(item.descuento, miCultura) / 100) * valorunitario;
                        }
                    else
                        {
                        descuento = 0;
                        valorunitario = valorADD;
                        }
                    subsiniva = (valorADD - descuento);
                    subtotal = subtotal + subsiniva;
                    totaliva = totaliva + iva;
                    totaldesc = totaldesc + descuento;
                    total = total + Convert.ToDecimal(item.Valorunitario, miCultura);

                    }
                else
                    {
                    valorunitario = Convert.ToDecimal(item.Valorunitario, miCultura) * Convert.ToDecimal(item.cantidad, miCultura);
                    if (item.descuento > 0)
                        {
                        valordes = (Convert.ToDecimal(item.descuento, miCultura) / 100) * valorunitario;
                        descuento = valordes;
                        valordes = valorunitario - valordes;

                        }
                    else
                        {
                        descuento = 0;
                        valordes = valorunitario;
                        }
                    if (item.iva > 0)
                        {
                        iva = valordes * (Convert.ToDecimal(item.iva, miCultura) / 100);
                        }
                    else
                        {
                        iva = 0;
                        }
                    subtotal = subtotal + valordes;
                    totaliva = totaliva + iva;
                    totaldesc = totaldesc + descuento;
                    total = total + valordes + iva;

                    }
                }
            var tot = context.tordentot.Where(x => x.id == idtot).FirstOrDefault();
            var tarifatot = context.ttarifatot.Where(x => x.id == tot.tarifa).FirstOrDefault();
            decimal ganancia = (Convert.ToDecimal(tarifatot.valor, miCultura) / 100) * total;
            decimal totalconganancia = total + ganancia;
            subtotal = Math.Round(subtotal);
            totaliva = Math.Round(totaliva);
            total = Math.Round(total);
            totaldesc = Math.Round(totaldesc);
            totalconganancia = Math.Round(totalconganancia);
            string subtotals = subtotal.ToString("N2", new CultureInfo("is-IS"));
            string totalivas = totaliva.ToString("N2", new CultureInfo("is-IS"));
            string totals = total.ToString("N2", new CultureInfo("is-IS"));
            string totaldescs = totaldesc.ToString("N2", new CultureInfo("is-IS"));
            string totalconganancias = totalconganancia.ToString("N2", new CultureInfo("is-IS"));

            return Json(new { subtotals, totalivas, totals, totaldescs, totalconganancias }, JsonRequestBehavior.AllowGet);
            }


        public JsonResult Crearrepuestostot(int idtot, string repuesto, decimal vrunitario, int cantidad, decimal descuento = 0, decimal iva = 0)
            {

            titemstot repuestotot = new titemstot();
            repuestotot.idordentot = idtot;
            repuestotot.Descripcion = repuesto;
            repuestotot.Valorunitario = Convert.ToDecimal(vrunitario, miCultura);
            repuestotot.descuento = descuento;
            repuestotot.cantidad = cantidad;
            repuestotot.estado = true;
            repuestotot.iva = iva;
            repuestotot.Tipoitem = 2;
            context.titemstot.Add(repuestotot);
            context.SaveChanges();

            var listarepuestos = context.titemstot.Where(x => x.idordentot == idtot && x.Tipoitem == 2 && x.estado == true).Select(x =>
             new
                 {
                 id = x.id,
                 canti = x.cantidad,
                 descripcion = x.Descripcion,
                 vrunitario = x.Valorunitario,
                 vrdesc = x.descuento,
                 vriva = x.iva,
                 total = x.cantidad * x.Valorunitario

                 }
            ).ToList();


            var datos = listarepuestos.Select(x => new
                {
                x.id,
                x.descripcion,
                x.canti,
                x.vriva,
                x.vrdesc,
                vrunitario = x.vrunitario.Value.ToString("N2", new CultureInfo("is-IS")),
                vlunitario = x.vrunitario,
                total = x.total.Value.ToString("N2", new CultureInfo("is-IS"))

                }).ToList();


            return Json(datos, JsonRequestBehavior.AllowGet);
            }

        public JsonResult Eliminaritem(int idtotdet, int tipo)
            {

            titemstot item = context.titemstot.Where(x => x.id == idtotdet).FirstOrDefault();
            item.estado = false;
            int id = Convert.ToInt32(item.idordentot);
            context.Entry(item).State = EntityState.Modified;
            context.SaveChanges();
            var listaitems = context.titemstot.Where(x => x.idordentot == id && x.estado == true && x.Tipoitem == tipo).Select(x =>
              new
                  {
                  id = x.id,
                  cant = x.cantidad,
                  vrunitario = x.Valorunitario,
                  vliva = x.iva,
                  vrdesc = x.descuento,
                  descripcion = x.Descripcion,
                  total = x.cantidad != null ? x.cantidad * x.Valorunitario : 0
                  }
            ).ToList();


            var datos = listaitems.Select(x => new
                {
                x.id,
                x.descripcion,
                x.cant,
                x.vliva,
                x.vrdesc,
                vrunitario = x.vrunitario.Value.ToString("N2", new CultureInfo("is-IS")),
                vlunitario = x.vrunitario,
                total = x.total.Value.ToString("N2", new CultureInfo("is-IS"))

                }).ToList();


            return Json(datos, JsonRequestBehavior.AllowGet);
            }


        public ActionResult ttarifastot()
            {

            return View();
            }


        public JsonResult GuardarValoresEncab(int idtot)
            {

            tordentot odentot = context.tordentot.Where(x => x.id == idtot).FirstOrDefault();

            decimal subtotal = 0, total = 0, totaliva = 0, subsiniva = 0, iva = 0, descuento = 0, totaldesc = 0, valorADD = 0, valorunitario = 0, valor = 0, valordes = 0;
            var idtotdetalles = context.titemstot.Where(x => x.idordentot == idtot && x.estado == true).ToList();

            foreach (var item in idtotdetalles)
                {
                if (item.Tipoitem == 1)
                    {
                    if (item.iva > 0)
                        {
                        valor = (Convert.ToDecimal(item.iva, miCultura) / 100) + 1;
                        valorADD = Convert.ToDecimal(item.Valorunitario, miCultura) / valor;
                        iva = (Convert.ToDecimal(item.iva, miCultura) / 100) * valorADD;
                        }
                    else
                        {
                        valorADD = Convert.ToDecimal(item.Valorunitario, miCultura);
                        iva = 0;
                        }
                    if (item.descuento > 0)
                        {
                        valordes = (Convert.ToDecimal(item.descuento, miCultura) / 100);
                        valordes = 1 - valordes;
                        valorunitario = valorADD / valordes;
                        descuento = (Convert.ToDecimal(item.descuento, miCultura) / 100) * valorunitario;
                        }
                    else
                        {
                        descuento = 0;
                        valorunitario = valorADD;
                        }
                    subsiniva = (valorADD - descuento);
                    subtotal = subtotal + subsiniva;
                    totaliva = totaliva + iva;
                    totaldesc = totaldesc + descuento;
                    total = total + Convert.ToDecimal(item.Valorunitario, miCultura);

                    }
                else
                    {
                    valorunitario = Convert.ToDecimal(item.Valorunitario, miCultura) * Convert.ToDecimal(item.cantidad, miCultura);
                    if (item.descuento > 0)
                        {
                        valordes = (Convert.ToDecimal(item.descuento, miCultura) / 100) * valorunitario;
                        descuento = valordes;
                        valordes = valorunitario - valordes;

                        }
                    else
                        {
                        descuento = 0;
                        valordes = valorunitario;
                        }
                    if (item.iva > 0)
                        {
                        iva = valordes * (Convert.ToDecimal(item.iva, miCultura) / 100);
                        }
                    else
                        {
                        iva = 0;
                        }
                    subtotal = subtotal + valordes;
                    totaliva = totaliva + iva;
                    totaldesc = totaldesc + descuento;
                    total = total + valordes + iva;

                    }
                }
            var tot = context.tordentot.Where(x => x.id == idtot).FirstOrDefault();
            var tarifatot = context.ttarifatot.Where(x => x.id == tot.tarifa).FirstOrDefault();
            decimal ganancia = (Convert.ToDecimal(tarifatot.valor, miCultura) / 100) * total;
            decimal totalconganancia = total + ganancia;
            subtotal = Math.Round(subtotal);
            totaliva = Math.Round(totaliva);
            total = Math.Round(total);
            totaldesc = Math.Round(totaldesc);
            totalconganancia = Math.Round(totalconganancia);


            odentot.subtotal = Convert.ToDecimal(subtotal, miCultura);
            odentot.totaliva = Convert.ToDecimal(totaliva, miCultura);
            odentot.totaldescuento = Convert.ToDecimal(totaldesc, miCultura);
            odentot.totalsinganancia = Convert.ToDecimal(total, miCultura);
            odentot.totalganancia = Convert.ToDecimal(totalconganancia, miCultura);
            context.Entry(odentot).State = EntityState.Modified;
            context.SaveChanges();

            return Json(idtot, JsonRequestBehavior.AllowGet);
            }

        [HttpPost]
        public ActionResult ttarifastot(Formtarifatot model)
            {
            try
                {
                ttarifatot tarifa = new ttarifatot();

                tarifa.nombre_tarifa = model.Nombretarifa;
                tarifa.valor = Convert.ToDecimal(model.valor, miCultura);
                tarifa.estado = model.Estado;
                context.ttarifatot.Add(tarifa);
                context.SaveChanges();
                TempData["mensaje"] = "Tarifa creada con exito";
                }
            catch (Exception E)
                {


                TempData["mensaje_error"] = "Error al crear la tarifa. " + E.Message;
                throw E;
                }


            return View();
            }

        public JsonResult ConsultartarifasTOT()
            {

            var tarifas = context.ttarifatot.Select(z =>
            new
                {
                z.id,
                z.nombre_tarifa,
                z.valor,
                z.estado
                }).ToList();

            var datos = tarifas.Select(x => new
                {
                id = x.id,
                tarifa = x.nombre_tarifa,
                valor = x.valor.Value.ToString("N2", new CultureInfo("is-IS")),
                estado = x.estado != false ? "Activo" : "Inactivo"
                }).ToList();

            return Json(datos, JsonRequestBehavior.AllowGet);
            }

        public ActionResult UpdateTarifaTOT(int id)
            {

            ttarifatot tarifa = context.ttarifatot.Where(x => x.id == id).FirstOrDefault();

            Formtarifatot modelo = new Formtarifatot();
            modelo.id = tarifa.id;
            modelo.Nombretarifa = tarifa.nombre_tarifa;
            modelo.valor = Convert.ToDecimal(tarifa.valor, miCultura);
            modelo.Estado = tarifa.estado!=null?tarifa.estado.Value: false;

            return View(modelo);
            }
        [HttpPost]
        public ActionResult UpdateTarifaTOT(Formtarifatot tarifa)
            {
            try
                {

                ttarifatot modelo = context.ttarifatot.Where(x => x.id == tarifa.id).FirstOrDefault();
                modelo.nombre_tarifa = tarifa.Nombretarifa;
                modelo.valor = tarifa.valor;
                modelo.estado = tarifa.Estado;
                context.Entry(modelo).State = EntityState.Modified;
                context.SaveChanges();
                TempData["mensaje"] = "Tarifa actualizada con exito";
                }
            catch (Exception E)
                {

                TempData["mensaje_error"] = "Error al crear la tarifa. " + E.Message;
                throw E;
                }



            return View();
            }

        public JsonResult ConsultarToT()
            {


            var tot = context.vw_tot.Select(a =>
                       new
                           {
                           a.id2,
                           a.numerofactura,
                           a.proveedor,
                           a.codigoentrada,
                           Operaciones = context.titemstot.Where(x => x.idordentot == a.id && x.Tipoitem == 1).Select(e => new { e.Descripcion }).ToList(),
                           Repuestos = context.titemstot.Where(x => x.idordentot == a.id && x.Tipoitem == 2).Select(g => new { g.Descripcion }).ToList(),
                           fecha = a.fecha2,
                           numidot = a.numot2,
                           Valor_total = a.valor_total2,
                           }).ToList();

            var datos = tot.Select(x => new
                {
                x.id2,
                x.numerofactura,
                x.Operaciones,
                x.Repuestos,
                x.fecha,
                x.numidot,
                x.codigoentrada,
                x.proveedor,
                x.Valor_total

                }).ToList();

            return Json(datos, JsonRequestBehavior.AllowGet);
            }


        public JsonResult ConsultarFiltroTot(string cadena, DateTime? fechaini, DateTime? fechafin)
            {

            Expression<Func<vw_tot, bool>> predicado = PredicateBuilder.True<vw_tot>();
            Expression<Func<vw_tot, bool>> predicado2 = PredicateBuilder.False<vw_tot>();

            if (!string.IsNullOrWhiteSpace(cadena))
                {
                predicado2 = predicado2.Or(d => d.id2 == cadena);
                predicado2 = predicado2.Or(d => d.numerofactura2 == cadena);
                predicado2 = predicado2.Or(d => d.numot2 == cadena);
                predicado2 = predicado2.Or(d => d.proveedor == cadena);
                predicado2 = predicado2.Or(d => d.codigoentrada == cadena);
                predicado2 = predicado2.Or(d => d.valor_total2 == cadena);
                predicado = predicado.And(predicado2);
                }
            if (fechaini != null)
                {
                predicado = predicado.And(d => d.fecha >= fechaini);
                }
            if (fechafin != null)
                {
                predicado = predicado.And(d => d.fecha <= fechafin);
                }


            var tots = context.vw_tot.Where(predicado).Select(x => new
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

            var datos = tots.Select(d => new
                {
                d.id2,
                d.numerofactura,
                d.proveedor,
                d.numot,
                d.codigoentrada,
                d.Operaciones,
                d.Repuestos,
                d.fecha2,
                valor_total = d.valor_total2 != null ? d.valor_total2 : ""
                }).ToList();


            return Json(datos, JsonRequestBehavior.AllowGet);
            }


        public JsonResult ConsultarFiltroTotliq(string idot, string cadena, DateTime? fechaini, DateTime? fechafin)
            {

            Expression<Func<vw_tot, bool>> predicado = PredicateBuilder.True<vw_tot>();
            Expression<Func<vw_tot, bool>> predicado2 = PredicateBuilder.False<vw_tot>();

            if (!string.IsNullOrWhiteSpace(cadena))
                {
                predicado2 = predicado2.Or(d => d.id2 == cadena);
                predicado2 = predicado2.Or(d => d.numerofactura2 == cadena);
                predicado2 = predicado2.Or(d => d.proveedor == cadena);
                predicado2 = predicado2.Or(d => d.codigoentrada == cadena);
                predicado2 = predicado2.Or(d => d.valor_total2 == cadena);
                predicado = predicado.And(predicado2);
                }
            if (!string.IsNullOrWhiteSpace(idot))
                {
                predicado = predicado.And(d => d.numot2 == idot);
                }
            if (fechaini != null)
                {
                predicado = predicado.And(d => d.fecha >= fechaini);
                }
            if (fechafin != null)
                {
                predicado = predicado.And(d => d.fecha <= fechafin);
                }


            var tots = context.vw_tot.Where(predicado).Select(x => new
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

            var datos = tots.Select(d => new
                {
                d.id2,
                d.numerofactura,
                d.proveedor,
                d.numot,
                d.codigoentrada,
                d.Operaciones,
                d.Repuestos,
                d.fecha2,
                valor_total = d.valor_total2 != null ? d.valor_total2 : ""
                }).ToList();


            return Json(datos, JsonRequestBehavior.AllowGet);
            }


        public JsonResult buscarFormaPago(int id)
            {
            var data = (from c in context.tercero_proveedor
                        join f in context.fpago_tercero
                            on c.fpago_id equals f.fpago_id
                        where c.prtercero_id == id
                        select new
                            {
                            id = f.fpago_id,
                            fpago = f.fpago_nombre
                            }).FirstOrDefault();
            if (data == null)
                {
                return Json(new { info = false }, JsonRequestBehavior.AllowGet);
                }

            return Json(new { info = true, data }, JsonRequestBehavior.AllowGet);
            }


        public ActionResult EditarTOT(int idtot)
            {


            tordentot ordentot = context.tordentot.Where(x => x.id == idtot).FirstOrDefault();



            string valortpdoc = context.icb_sysparameter.Where(x => x.syspar_cod == "P154").Select(a => a.syspar_value).FirstOrDefault();
            int valordoc = Convert.ToInt32(valortpdoc);

            var tpdoc = context.tp_doc_registros.Where(x => x.tp_doc_sw.sw == valordoc).Select(x => new { x.tpdoc_id, x.tpdoc_nombre }).ToList();

            ViewBag.tipo = new SelectList(tpdoc, "tpdoc_id", "tpdoc_nombre", ordentot.tipo);

            int idUsuario = Convert.ToInt32(Session["user_usuarioid"]);
            var bodegasUsuario = (from bodegaUsuario in context.bodega_usuario
                                  join bodega in context.bodega_concesionario
                                      on bodegaUsuario.id_bodega equals bodega.id
                                  where bodegaUsuario.id_usuario == idUsuario
                                  select new { bodega.id, bodega.bodccs_nombre }).ToList();



            string valortipo = context.icb_sysparameter.Where(x => x.syspar_cod == "P111").Select(a => a.syspar_value).FirstOrDefault();
            int valor = Convert.ToInt32(valortipo);

            var ot = context.tencabezaorden.Where(x => x.estadoorden != valor).Select(c => new { c.id, c.codigoentrada }).ToList();


            ViewBag.numot = new SelectList(ot, "id", "codigoentrada");
            ViewBag.Bodega = new SelectList(bodegasUsuario, "id", "bodccs_nombre");


            var provedores = from pro in context.tercero_proveedor
                             join ter in context.icb_terceros
                                 on pro.tercero_id equals ter.tercero_id
                             select new
                                 {
                                 idTercero = pro.prtercero_id,
                                 nombreTErcero = ter.prinom_tercero,
                                 apellidosTercero = ter.apellido_tercero,
                                 razonSocial = ter.razon_social
                                 };
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in provedores)
                {
                string nombre = item.nombreTErcero + " " + item.apellidosTercero + " " + item.razonSocial;
                items.Add(new SelectListItem { Text = nombre, Value = item.idTercero.ToString() });
                }
            int user_id = Convert.ToInt32(Session["user_usuarioid"]);

            ViewBag.nombreusuario = context.users.Where(x => x.user_id == user_id).Select(x => x.user_nombre + " " + x.user_apellido).FirstOrDefault();


            var fpago_d = context.fpago_tercero.Select(x => new { x.fpago_id, x.fpago_nombre }).ToList();
            ViewBag.fpago = new SelectList(fpago_d, "fpago_id", "fpago_nombre");
            ViewBag.proveedor = items;

            Expression<Func<perfil_contable_documento, bool>> predicado = PredicateBuilder.True<perfil_contable_documento>();
            predicado = predicado.And(d => d.tipo == ordentot.tipo);
            predicado = predicado.And(
                    d => d.perfil_contable_bodega.Where(e => e.idbodega == ordentot.bodega).Count() > 0);

            var perfiles = context.perfil_contable_documento.Where(predicado)
                .Select(d => new { d.id, nombre = d.descripcion }).ToList();

            ViewBag.perfil = new SelectList(perfiles, "id", "nombre");

            ViewBag.tipotarifa = new SelectList(context.ttarifatot.Where(x => x.estado == true).Select(x => new { x.id, x.nombre_tarifa }), "id", "nombre_tarifa");

            TOTformulario tot = new TOTformulario();
            tot.id = Convert.ToInt32(ordentot.id);
            tot.numerofactura = Convert.ToInt32(ordentot.numerofactura);
            tot.fecha = Convert.ToDateTime(ordentot.fecha);
            tot.fpago = Convert.ToInt32(ordentot.condicionpago);
            tot.tipo = Convert.ToInt32(ordentot.tipo);
            tot.proveedor = Convert.ToInt32(ordentot.idproveedor);
            tot.tipotarifa = Convert.ToInt32(ordentot.tarifa);
            tot.Bodega = Convert.ToInt32(ordentot.bodega);
            tot.numot = Convert.ToInt32(ordentot.numot);
            tot.perfil = Convert.ToInt32(ordentot.perfil);

            return View(tot);
            }

        public JsonResult CargarItemsTot(int idtot)
            {

            var listaoperaciones = context.titemstot.Where(x => x.idordentot == idtot && x.estado == true && x.Tipoitem == 1).Select(x =>
        new
            {
            id = x.id,
            vliva = x.iva,
            vrunitario = x.Valorunitario,
            descripcion = x.Descripcion,
            vrdescuento = x.descuento

            }
     ).ToList();

            var datos = listaoperaciones.Select(x => new
                {
                x.id,
                x.descripcion,
                x.vliva,
                x.vrdescuento,
                vrunitario = x.vrunitario.Value.ToString("N2", new CultureInfo("is-IS")),
                vlunitario = x.vrunitario

                }).ToList();

            var listarepuestos = context.titemstot.Where(x => x.idordentot == idtot && x.Tipoitem == 2 && x.estado == true).Select(x =>
           new
               {
               id = x.id,
               canti = x.cantidad,
               descripcion = x.Descripcion,
               vrunitario = x.Valorunitario,
               vrdesc = x.descuento,
               vriva = x.iva,
               total = x.cantidad * x.Valorunitario

               }
          ).ToList();


            var datos2 = listarepuestos.Select(x => new
                {
                x.id,
                x.descripcion,
                x.canti,
                x.vriva,
                x.vrdesc,
                vrunitario = x.vrunitario.Value.ToString("N2", new CultureInfo("is-IS")),
                vlunitario = x.vrunitario,
                total = x.total.Value.ToString("N2", new CultureInfo("is-IS"))

                }).ToList();




            return Json(new { datos, datos2 }, JsonRequestBehavior.AllowGet);
            }

        public JsonResult actualizartarifatot(int idtot, int tarifa)
            {


            tordentot ordentot = context.tordentot.Where(x => x.id == idtot).FirstOrDefault();
            ordentot.tarifa = tarifa;
            context.Entry(ordentot).State = EntityState.Modified;
            context.SaveChanges();

            return Json(true, JsonRequestBehavior.AllowGet);
            }
        public JsonResult GuardarValorestot(int idtot, int tipo, int Bodega, int perfil, int fpago, int numerofactura, DateTime fecha, int numot, int tipotarifa, int proveedor)
            {
            try
                {



                int user_id = Convert.ToInt32(Session["user_usuarioid"]);

                tordentot tot = context.tordentot.Where(x => x.id == idtot).FirstOrDefault();
                tot.tipo = tipo;
                tot.bodega = Bodega;
                tot.fecha = fecha;
                tot.tipo = tipo;
                tot.numerofactura = numerofactura;
                tot.condicionpago = fpago;
                tot.numot = numot;
                tot.tarifa = tipotarifa;
                tot.usuariocreacion = user_id;
                tot.fechacreacion = DateTime.Now;
                tot.idproveedor = proveedor;
                tot.perfil = perfil;
                decimal subtotal = 0, total = 0, totaliva = 0, subsiniva = 0, iva = 0, descuento = 0, totaldesc = 0, valorADD = 0, valorunitario = 0, valor = 0, valordes = 0;
                var idtotdetalles = context.titemstot.Where(x => x.idordentot == idtot && x.estado == true).ToList();

                foreach (var item in idtotdetalles)
                    {
                    if (item.Tipoitem == 1)
                        {
                        if (item.iva > 0)
                            {
                            valor = (Convert.ToDecimal(item.iva, miCultura) / 100) + 1;
                            valorADD = Convert.ToDecimal(item.Valorunitario, miCultura) / valor;
                            iva = (Convert.ToDecimal(item.iva, miCultura) / 100) * valorADD;
                            }
                        else
                            {
                            valorADD = Convert.ToDecimal(item.Valorunitario, miCultura);
                            iva = 0;
                            }
                        if (item.descuento > 0)
                            {
                            valordes = (Convert.ToDecimal(item.descuento, miCultura) / 100);
                            valordes = 1 - valordes;
                            valorunitario = valorADD / valordes;
                            descuento = (Convert.ToDecimal(item.descuento, miCultura) / 100) * valorunitario;
                            }
                        else
                            {
                            descuento = 0;
                            valorunitario = valorADD;
                            }
                        subsiniva = (valorADD - descuento);
                        subtotal = subtotal + subsiniva;
                        totaliva = totaliva + iva;
                        totaldesc = totaldesc + descuento;
                        total = total + Convert.ToDecimal(item.Valorunitario, miCultura);

                        }
                    else
                        {
                        valorunitario = Convert.ToDecimal(item.Valorunitario, miCultura) * Convert.ToDecimal(item.cantidad, miCultura);
                        if (item.descuento > 0)
                            {
                            valordes = (Convert.ToDecimal(item.descuento, miCultura) / 100) * valorunitario;
                            descuento = valordes;
                            valordes = valorunitario - valordes;

                            }
                        else
                            {
                            descuento = 0;
                            valordes = valorunitario;
                            }
                        if (item.iva > 0)
                            {
                            iva = valordes * (Convert.ToDecimal(item.iva, miCultura) / 100);
                            }
                        else
                            {
                            iva = 0;
                            }
                        subtotal = subtotal + valordes;
                        totaliva = totaliva + iva;
                        totaldesc = totaldesc + descuento;
                        total = total + valordes + iva;

                        }
                    }

                var tarifatot = context.ttarifatot.Where(x => x.id == tot.tarifa).FirstOrDefault();
                decimal ganancia = (Convert.ToDecimal(tarifatot.valor, miCultura) / 100) * total;
                decimal totalconganancia = total + ganancia;
                subtotal = Math.Round(subtotal);
                totaliva = Math.Round(totaliva);
                total = Math.Round(total);
                totaldesc = Math.Round(totaldesc);
                totalconganancia = Math.Round(totalconganancia);


                tot.subtotal = Convert.ToDecimal(subtotal, miCultura);
                tot.totaliva = Convert.ToDecimal(totaliva, miCultura);
                tot.totaldescuento = Convert.ToDecimal(totaldesc, miCultura);
                tot.totalsinganancia = Convert.ToDecimal(total, miCultura);
                tot.totalganancia = Convert.ToDecimal(totalconganancia, miCultura);




                context.Entry(tot).State = EntityState.Modified;
                context.SaveChanges();
                return Json(tot.id, JsonRequestBehavior.AllowGet);
                }
            catch (Exception e)
                {

                throw e;
                }

            }

        public JsonResult FechaEntrega(int idorden)
            {

            tencabezaorden orden = context.tencabezaorden.Where(x => x.id == idorden).FirstOrDefault();
            string fecha = orden.entrega.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"));

            return Json(fecha, JsonRequestBehavior.AllowGet);
            }

        public JsonResult CambiarFechaentrega(int idorden, DateTime fecha, string observacion)
            {
            tencabezaorden orden = context.tencabezaorden.Where(x => x.id == idorden).FirstOrDefault();

            orden.entrega = fecha;
            orden.ObservacionCamfecha = observacion;
            context.Entry(orden).State = EntityState.Modified;
            context.SaveChanges();

            return Json(orden.id, JsonRequestBehavior.AllowGet);
            }


        public ActionResult motivospausaot()
            {

            return View();
            }

        [HttpPost]
        public ActionResult motivospausaot(Formmotivopausaot modelo)
            {

            tmotivospausa motivo = new tmotivospausa();
            motivo.motivo = modelo.descripcion;
            motivo.estado = modelo.estado;
            motivo.razon_inactivo = modelo.razoninactivo;
            motivo.fecha_creacion = DateTime.Now;
            motivo.usuario_creacion = Convert.ToInt32(Session["user_usuarioid"]);

            context.tmotivospausa.Add(motivo);
            TempData["mensaje"] = "Motivo de pausa creado con exito";
            context.SaveChanges();
            modelo = null;
            return View(modelo);
            }


        public JsonResult BuscarMotivosPausa()
            {

            var datos = context.tmotivospausa.Select(x => new
                {
                x.motivo,
                estado = x.estado != false ? "Activo" : "inactivo",
                x.id
                }).ToList();

            return Json(datos, JsonRequestBehavior.AllowGet);
            }
        public JsonResult BuscarMotivosPausaSelect()
            {

            var datos = context.tmotivospausa.Where(x => x.estado == true).Select(x => new
                {
                x.motivo,
                x.id
                }).ToList();

            return Json(datos, JsonRequestBehavior.AllowGet);
            }

        public ActionResult motivospausaEditar(int id)
            {

            tmotivospausa tmotivospausa = context.tmotivospausa.Where(x => x.id == id).FirstOrDefault();
            Formmotivopausaot modelo = new Formmotivopausaot();
            modelo.descripcion = tmotivospausa.motivo;
            modelo.estado = tmotivospausa.estado;
            modelo.razoninactivo = tmotivospausa.razon_inactivo;

            return View(modelo);
            }
        [HttpPost]
        public ActionResult motivospausaEditar(Formmotivopausaot modelo)
            {

            tmotivospausa tmotivospausa = context.tmotivospausa.Where(x => x.id == modelo.id).FirstOrDefault();
            ViewBag.EstadoDependencia = new SelectList(context.tEstadosOT2.Where(x => x.estado == true).Select(x => new { x.descripcion, x.id }).ToList(), "id", "descripcion");

            tmotivospausa.motivo = modelo.descripcion;
            tmotivospausa.estado = modelo.estado;
            tmotivospausa.razon_inactivo = modelo.razoninactivo;
            tmotivospausa.fecha_actualizacion = DateTime.Now;
            tmotivospausa.usuario_actualizacion = Convert.ToInt32(Session["user_usuarioid"]);
            context.Entry(tmotivospausa).State = EntityState.Modified;
            context.SaveChanges();
            TempData["mensaje"] = "Motivo de pausa actualizado con exito";
            return View(modelo);
            }

        public JsonResult ClaveSeguridadTecnico(int idtecnico, string clave)
            {
            var resultado = false;
            ttecnicos tecnico = context.ttecnicos.Where(x => x.idusuario == idtecnico && x.claveSeguridad == clave).FirstOrDefault();
            if (tecnico != null)
                {
                resultado = true;
                }
            else
                {
                resultado = false;
                }
            return Json(resultado, JsonRequestBehavior.AllowGet);
            }
        public ActionResult EstadosGarantias()
            {
            ViewBag.EstadoDependencia = new SelectList(context.tEstadosOT2.Where(x => x.estado == true).Select(x => new { x.descripcion, x.id }).ToList(), "id", "descripcion");

            return View();
            }
        [HttpPost]
        public ActionResult EstadosGarantias(Formestadogarantia modelo)
            {
            tEstadosOT2 tEstadosOT2 = new tEstadosOT2();

            ViewBag.EstadoDependencia = new SelectList(context.tEstadosOT2.Where(x => x.estado == true).Select(x => new { x.descripcion, x.id }).ToList(), "id", "descripcion", tEstadosOT2.Estadodepende);

            tEstadosOT2.descripcion = modelo.descripcion;

            tEstadosOT2.estado = modelo.estado;                   

            if (modelo.EstadoDependencia>0) {
                tEstadosOT2.Estadodepende = modelo.EstadoDependencia;
                }


            tEstadosOT2.fecha_creaciomn = DateTime.Now;
            tEstadosOT2.user_creacion = Convert.ToInt32(Session["user_usuarioid"]);
            tEstadosOT2.razoninactivo = modelo.razoninactivo;
            context.tEstadosOT2.Add(tEstadosOT2);
            context.SaveChanges();
            TempData["mensaje"] = "Estado Garantia creado con exito";
            return View();
            }

        public JsonResult BuscarEstadosGarantiaSelect()
            {

            var datos = context.tEstadosOT2.Where(x => x.estado == true).Select(x => new
                {
                x.descripcion,
                x.id
                }).ToList();

            return Json(datos, JsonRequestBehavior.AllowGet);
            }

        public JsonResult BuscarEstadosGarantia()
            {

            var datos = context.tEstadosOT2.Select(x => new
                {
                x.descripcion,
                x.id,
                estado = x.estado != false ? "Activo" : "inactivo",
                }).ToList();

            return Json(datos, JsonRequestBehavior.AllowGet);
            }
        public ActionResult EstadosGarantiasEditar(int id)
            {
            Formestadogarantia modelo = new Formestadogarantia();
            tEstadosOT2 tEstadosOT2 = context.tEstadosOT2.Where(x => x.id == id).FirstOrDefault();
            modelo.id = tEstadosOT2.id;
            modelo.descripcion = tEstadosOT2.descripcion;
            modelo.estado = Convert.ToBoolean(tEstadosOT2.estado);
            modelo.razoninactivo = tEstadosOT2.razoninactivo;
            if (tEstadosOT2.Estadodepende>0)
                {
                modelo.EstadoDependencia = Convert.ToInt32(tEstadosOT2.Estadodepende);
                }
    
            ViewBag.EstadoDependencia = new SelectList(context.tEstadosOT2.Where(x => x.estado == true).Select(x => new { x.descripcion, x.id }).ToList(), "id", "descripcion", tEstadosOT2.Estadodepende);

            return View(modelo);
            }


        [HttpPost]
        public ActionResult EstadosGarantiasEditar(Formestadogarantia modelo)
            {
   
            tEstadosOT2 tEstadosOT2 = context.tEstadosOT2.Where(x => x.id == modelo.id).FirstOrDefault();
    
            tEstadosOT2.descripcion= modelo.descripcion;
            tEstadosOT2.estado = Convert.ToBoolean(modelo.estado);
           tEstadosOT2.razoninactivo = modelo.razoninactivo;
            if (modelo.EstadoDependencia > 0)
                {
           tEstadosOT2.Estadodepende =  modelo.EstadoDependencia;
                }
            context.Entry(tEstadosOT2).State = EntityState.Modified;
            context.SaveChanges();
            ViewBag.EstadoDependencia = new SelectList(context.tEstadosOT2.Where(x => x.estado == true).Select(x => new { x.descripcion, x.id }).ToList(), "id", "descripcion", tEstadosOT2.Estadodepende);
            TempData["mensaje"] = "Estado actualizado con exito";
            return View(modelo);
            }

        public ActionResult AdmonGarantias() {
            return View();
            }

        public JsonResult BuscarAdmonGarantias() {
            var datos = context.vw_admonot_garantia.Select(x => new
                {
                x.id,
                fecha = x.fecha2,
                x.codigoentrada,
                x.cedula,
               cliente = x.Nombre_cliente,
                x.placa,
                x.vin,
                x.nomEstado,
                x.estadodos,
                x.bodega
                }).ToList();

            return Json(datos, JsonRequestBehavior.AllowGet);
            }
        public JsonResult BuscarAdmonGarantiasFiltro(string placa, string cliente, string ot, DateTime fechaini, DateTime fechafin)
            {

            Expression<Func<vw_admonot_garantia, bool>> predicado = PredicateBuilder.True<vw_admonot_garantia>();
            Expression<Func<vw_admonot_garantia, bool>> predicado2 = PredicateBuilder.False<vw_admonot_garantia>();

            if (!string.IsNullOrWhiteSpace(placa))
                {
                predicado2 = predicado2.Or(e=> e.placa==placa);
              
                predicado = predicado.And(predicado2);
                }
            if (!string.IsNullOrWhiteSpace(cliente))
                {
                predicado2 = predicado2.Or(e => e.cedula == cliente);
                predicado2 = predicado2.Or(e => e.Nombre_cliente == cliente);
                predicado = predicado.And(predicado2);
                }
            if (!string.IsNullOrWhiteSpace(ot))
                {
                predicado2 = predicado2.Or(e => e.codigoentrada == ot);           
                predicado = predicado.And(predicado2);
                }


            if (fechaini != null)
                {
                predicado = predicado.And(d => d.fecha >= fechaini);
                }
            if (fechafin != null)
                {
                predicado = predicado.And(d => d.fecha <= fechafin);
                }




            var datos = context.vw_admonot_garantia.Where(predicado).Select(x => new
                {
                x.id,
                fecha = x.fecha2,
                x.codigoentrada,
                x.cedula,
                cliente = x.Nombre_cliente,
                x.placa,
                x.vin,
                x.nomEstado,
                x.estadodos,
                x.bodega
                }).ToList();

            return Json(datos, JsonRequestBehavior.AllowGet);
            }
        public JsonResult GuardarSeguimiento(int ot, DateTime fecha, string observacion) {

            tseguimientoot seguimiento = new tseguimientoot();
            seguimiento.FechaSeguimiento = fecha;
            seguimiento.numot = ot;
            seguimiento.observacion = observacion;
            seguimiento.usuario_creacion = Convert.ToInt32(Session["user_usuarioid"]);
            context.tseguimientoot.Add(seguimiento);
            context.SaveChanges();

         

            return Json(true, JsonRequestBehavior.AllowGet);
            }
        
        public JsonResult consultasegot(int ot) { 

            var dato = context.tseguimientoot.Where(x => x.numot == ot).Select(c => new
                {
                c.observacion
               

                }).ToList();

            return Json(dato, JsonRequestBehavior.AllowGet);
            }

        public ActionResult Garantiaot(int id) {

            tencabezaorden orden = context.tencabezaorden.Find(id);

            EncabezadoOTModel orden2 = new EncabezadoOTModel
                {
                id = orden.id,
                placa = orden.placa,
                entrega = orden.entrega.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US")),
                aseguradora = orden.aseguradora,
                asesor = orden.users.user_nombre + " " + orden.users.user_apellido,
                bodega = orden.bodega,
                modelo = orden.icb_vehiculo.modelo_vehiculo.modvh_nombre,
                añovh = orden.icb_vehiculo.anio_vh,
                centrocosto = orden.centrocosto,
                codigoentrada = orden.codigoentrada,
                deducible = orden.deducible != null
                    ? orden.deducible.Value.ToString("N0", new CultureInfo("is-IS"))
                    : "",
                minimo = orden.minimo != null ? orden.minimo.Value.ToString("N0", new CultureInfo("is-IS")) : "",
                fecha_soat = orden.fecha_soat != null
                    ? orden.fecha_soat.Value.ToString("yyyy/MM/dd", new CultureInfo("en-US"))
                    : "",
                estadoorden = orden.estadoorden,
                domicilo = orden.domicilo,
                estado = orden.estado,
                garantia_causa = orden.garantia_causa,
                garantia_falla = orden.garantia_falla,
                garantia_solucion = orden.garantia_solucion,
                idcita = orden.idcita,
                //idtecnico=orden.idtecnico,
                idtipodoc = orden.idtipodoc,
                kilometraje_nuevo = orden.icb_vehiculo.kilometraje_nuevo,
                kilometraje = orden.icb_vehiculo.kilometraje,
                notas = orden.notas,
                numero = orden.numero,
                numero_soat = orden.numero_soat,
                poliza = orden.poliza,
                razoningreso = orden.razoningreso,
                razon_inactivo = orden.razon_inactivo,
                razon_secundaria = orden.razon_secundaria,
                siniestro = orden.siniestro,
                tercero = orden.tercero,
                tipooperacion = orden.tipooperacion,
                tecnico = orden.idtecnico,
                txtDocumentoCliente = orden.icb_terceros.doc_tercero,
                Estadodos =  orden.estadoorden_dos 
                };


            icb_sysparameter tipoorden = context.icb_sysparameter.Where(d => d.syspar_cod == "P76").FirstOrDefault();
            int tipodocorden = tipoorden != null ? Convert.ToInt32(tipoorden.syspar_value) : 26;
            
            var buscarVehiculo = (from vehiculo in context.icb_vehiculo
                                  where vehiculo.plan_mayor == orden.placa || vehiculo.plan_mayor == orden.placa
                                  select new { vehiculo.plac_vh, vehiculo.plan_mayor }).FirstOrDefault();


            orden.placa = !string.IsNullOrWhiteSpace(buscarVehiculo.plac_vh)
               ? buscarVehiculo.plac_vh
               : buscarVehiculo.plan_mayor;
            ViewBag.entrega = orden.entrega;
            ViewBag.tercero = orden.tercero;
            ViewBag.placa1 = orden.placa;


            ViewBag.citaId = context.tencabezaorden.FirstOrDefault(x => x.id == id).idcita;
            var buscarContacto = (from contacto in context.icb_contacto_tercero
                                  join tercero in context.icb_terceros on contacto.tercero_id equals tercero.tercero_id
                                  where tercero.tercero_id == orden.tercero
                                  select new { contacto.con_tercero_nombre, contacto.con_tercero_telefono }).FirstOrDefault();




            var contacto1 = buscarContacto != null ? buscarContacto.con_tercero_nombre : "";
            var numero = buscarContacto != null ? buscarContacto.con_tercero_telefono : "";

            ViewBag.contacto = contacto1;
            ViewBag.telcontacto = numero;


            var datos = context.tEstadosOT2.Where(x => x.estado == true).Select(x => new
                {
                x.descripcion,
                x.id
                }).ToList();

            ViewBag.Estadodos = new SelectList(datos, "id", "descripcion", orden.estadoorden_dos);

            

            listasDesplegables2(orden2);


            return View(orden2);
            }
        
        public JsonResult Consultaopeyrepgaran(int idorden) {


            AlmacenController metodosstock = new AlmacenController();

            tencabezaorden ordendatos = context.tencabezaorden.Where(d => d.id == idorden).FirstOrDefault();

            var buscarOperacionesSQL = (from detalle in context.tdetallemanoobraot
                                        join tempario in context.ttempario
                                            on detalle.idtempario equals tempario.codigo
                                        join operario in context.ttecnicos
                                            on detalle.idtecnico equals operario.id
                                        join usuario in context.users
                                            on operario.idusuario equals usuario.user_id
                                        where detalle.idorden == idorden 
                                        select new
                                            {
                                            detalle.id,
                                            tempario.codigo,
                                            tempario.operacion,
                                           tecnico = usuario.user_nombre + " " + usuario.user_apellido,
                                            tecnico_id = operario.id,
                                            tiempo = detalle.tiempo != null ? detalle.tiempo : 0,
                                            precio = detalle.valorunitario,
                                            detalle.pordescuento,
                                            detalle.poriva,
                                            detalle.estado,
                                            detalle.fecha_envio_sms,
                                            detalle.respuesta_sms
                                            }).ToList();
            var buscarOperaciones = buscarOperacionesSQL.Select(x => new
                {
                idoperacion = x.id,
                x.codigo,
                x.operacion,
                x.tiempo,
                x.tecnico,
                x.tecnico_id,          
                //precio = x.precio != null ? x.precio : 0,
                precio = x.precio,

                ivaOperacion = x.poriva != null ? x.poriva : 0,

                descuentoOperacion = x.pordescuento != null ? x.pordescuento : 0,

                descuento = x.tiempo > 0
                ? x.precio * x.pordescuento / 100 * (decimal)x.tiempo
                : x.precio * x.pordescuento / 100 * 1,

                valorIva = x.tiempo != 0
                ? (x.precio * (decimal)x.tiempo - x.precio * x.pordescuento / 100 * (decimal)x.tiempo) *
                  x.poriva / 100
                : (x.precio * 1 - x.precio * x.pordescuento / 100 * 1) * x.poriva / 100,

                valorTotal = x.tiempo != 0
                ? x.precio * (decimal)x.tiempo - x.precio * x.pordescuento / 100 * (decimal)x.tiempo +
                  (x.precio * (decimal)x.tiempo - x.precio * x.pordescuento / 100 * (decimal)x.tiempo) *
                  x.poriva / 100
                : x.precio * 1 - x.precio * x.pordescuento / 100 * 1 +
                  (x.precio * 1 - x.precio * x.pordescuento / 100 * 1) * x.poriva / 100,
                estado = !string.IsNullOrWhiteSpace(x.estado) ? x.estado : "-1",
                fecha_envio_sms = x.fecha_envio_sms != null
                ? x.fecha_envio_sms.Value.ToString("yyyy/MM/dd HH:mm", new CultureInfo("en-US"))
                : "",
                respuesta = x.respuesta_sms != null ? x.respuesta_sms == true ? 1 : 0 : -1,
                boton_sms = x.id,
                fechasms = x.fecha_envio_sms,
                estadorepuesta = x.estado
                });

            decimal valoroperaciones = 0;
            decimal valorrepuestos = 0;
            foreach (var item in buscarOperaciones)
                {
                valoroperaciones = valoroperaciones + (item.valorTotal != null ? item.valorTotal.Value : 0);
                }

            var buscarRepuestosSQL = (from repuestos in context.tdetallerepuestosot
                                      join referencia in context.icb_referencia
                                          on repuestos.idrepuesto equals referencia.ref_codigo
                                      join precios in context.rprecios
                                          on referencia.ref_codigo equals precios.codigo into ps2
                                      from precios in ps2.DefaultIfEmpty()                                   
                                      where repuestos.idorden == idorden 
                                      select new
                                          {
                                          repuestos.id,
                                          repuestos.icb_referencia.ref_codigo,
                                          repuestos.icb_referencia.ref_descripcion,
                                          repuestos.solicitado,                                    
                                          solicitudes = repuestos.solicitado,
                                          repuestos.cantidad,                                     
                                          precio = repuestos.valorunitario,
                                           repuestos.icb_referencia.por_iva,
                                          repuestos.icb_referencia.por_dscto,
                                          repuestos.icb_referencia.por_dscto_max                                        
                                          }).Distinct().ToList();

            

                  var buscarCliente = context.tercero_cliente.Where(d => d.tercero_id == ordendatos.tercero)
          .Select(d => new { d.lprecios_repuestos, d.dscto_rep }).FirstOrDefault();

            var repuestosAgendados = buscarRepuestosSQL.Select(x => new
                {
                x.id,
                x.ref_codigo,
                x.ref_descripcion,
                x.solicitado,
                x.solicitudes,       
                x.precio,
                x.por_iva,
                x.cantidad,
                x.por_dscto,

                valor_dscto = Math.Round(
        x.precio * (decimal)(buscarCliente.dscto_rep > x.por_dscto &&
                              buscarCliente.dscto_rep <= x.por_dscto_max ? buscarCliente.dscto_rep :
            buscarCliente.dscto_rep < x.por_dscto && x.por_dscto <= x.por_dscto_max ? x.por_dscto : 0) /
        100 * x.cantidad),
                valorIva = Math.Round((x.cantidad * x.precio - Math.Round(
                               x.precio * (decimal)(buscarCliente.dscto_rep > x.por_dscto &&
                                                     buscarCliente.dscto_rep <= x.por_dscto_max
                                   ?
                                   buscarCliente.dscto_rep
                                   :
                                   buscarCliente.dscto_rep < x.por_dscto && x.por_dscto <= x.por_dscto_max
                                       ? x.por_dscto
                                       : 0) / 100 * x.cantidad)) * (decimal)x.por_iva / 100),
                valorTotal = Math.Round(x.precio * x.cantidad - Math.Round(
                                x.precio * (decimal)(buscarCliente.dscto_rep > x.por_dscto &&
                                                      buscarCliente.dscto_rep <= x.por_dscto_max
                                    ?
                                    buscarCliente.dscto_rep
                                    :
                                    buscarCliente.dscto_rep < x.por_dscto && x.por_dscto <= x.por_dscto_max
                                        ? x.por_dscto
                                        : 0) / 100 * x.cantidad)
                            + Math.Round((x.cantidad * x.precio - Math.Round(
                                              x.precio *
                                              (decimal)(buscarCliente.dscto_rep > x.por_dscto &&
                                                         buscarCliente.dscto_rep <= x.por_dscto_max
                                                  ?
                                                  buscarCliente.dscto_rep
                                                  :
                                                  buscarCliente.dscto_rep < x.por_dscto &&
                                                  x.por_dscto <= x.por_dscto_max
                                                      ? x.por_dscto
                                                      : 0) / 100 * x.cantidad)) * (decimal)x.por_iva /
                                         100)),
                stock_bodega = metodosstock.buscarStockBodega(x.ref_codigo, ordendatos.bodega),
                stock_otrasbodegas = metodosstock.buscarStockOtras(x.ref_codigo, ordendatos.bodega),
                stock_reemplazo = metodosstock.buscarStockReemplazo(x.ref_codigo, ordendatos.bodega),
                codigo_reemplazo = metodosstock.buscarStockReemplazo(x.ref_codigo, ordendatos.bodega) > 0
        ? traerstockreemplazo2(x.ref_codigo, ordendatos.bodega, x.cantidad)
        : "",
           

                });

            foreach (var item2 in repuestosAgendados)
                {
                valorrepuestos = valorrepuestos + (item2.solicitado ? item2.valorTotal : 0);
                }


            return Json(new { buscarOperaciones, valoroperaciones , repuestosAgendados, valorrepuestos }, JsonRequestBehavior.AllowGet);
            }


        public JsonResult GetObservacionespuasa(int idorden) {
            var Datos = context.tpausasorden.Where(x => x.idorden == idorden).OrderByDescending(e => e.fecha_fin).Select(u => u.observacion_pausa).ToList();

            return Json(Datos, JsonRequestBehavior.AllowGet);
            }

        public JsonResult ConsultarAverias(int idot)
            {
            var resultado = false;
            tencabezaorden ordendatos = context.tencabezaorden.Where(d => d.id == idot).FirstOrDefault();

            var listaverias = context.icb_vehiculo_eventos.Where(x => x.planmayor == ordendatos.placa).Select(d => new { d.planmayor, d.evento_id }).ToList();

            if (listaverias.Count>0)
                {
                resultado = true;
                }


            return Json(resultado, JsonRequestBehavior.AllowGet);
            }

        public JsonResult validarInspeccion(int idorden) {

            var  orden = context.tencabezaorden.Where(x => x.id == idorden).Select(l=>new { l.fecha_inicio_inspeccion, l.fecha_inspeccion }).FirstOrDefault();
            
            return Json(orden, JsonRequestBehavior.AllowGet);
            }




        }


    }