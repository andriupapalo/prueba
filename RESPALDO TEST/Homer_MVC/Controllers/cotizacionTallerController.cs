using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class cotizacionTallerController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        // GET: cotizacionTaller
        public ActionResult Create(int? menu)
        {
            ListasDesplegables(new tencabcotizacion());
            BuscarFavoritos(menu);
            return View(new tencabcotizacion { estado = true });
        }


        // POST: cotizacionTaller
        [HttpPost]
        public ActionResult Create(tencabcotizacion modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                long numeroConsecutivo = 0;
                ConsecutivosGestion gestionConsecutivo = new ConsecutivosGestion();
                icb_doc_consecutivos numeroConsecutivoAux = new icb_doc_consecutivos();
                tp_doc_registros buscarTipoDocRegistro = context.tp_doc_registros.FirstOrDefault(x => x.tpdoc_id == modelo.tipodoc);
                numeroConsecutivoAux =
                    gestionConsecutivo.BuscarConsecutivo(buscarTipoDocRegistro, modelo.tipodoc, modelo.bodega);

                grupoconsecutivos grupoConsecutivo = context.grupoconsecutivos.FirstOrDefault(x =>
                    x.documento_id == modelo.tipodoc && x.bodega_id == modelo.bodega);
                if (numeroConsecutivoAux != null)
                {
                    numeroConsecutivo = numeroConsecutivoAux.doccons_siguiente;
                }
                else
                {
                    TempData["mensaje_error"] = "No existe un numero consecutivo asignado para este tipo de documento";
                    BuscarFavoritos(menu);
                    return View();
                }


                modelo.userid_creacion = Convert.ToInt32(Session["user_usuarioid"]);
                modelo.fecha_creacion = DateTime.Now;
                modelo.numero = (int)numeroConsecutivo;
                context.tencabcotizacion.Add(modelo);
                int guardar = context.SaveChanges();
                if (guardar > 0)
                {
                    tencabcotizacion buscarUltimoEncabezado = context.tencabcotizacion.OrderByDescending(x => x.id).FirstOrDefault();

                    int cantidadManoObra = Convert.ToInt32(Request["cantidadesManoObra"]);
                    for (int i = 1; i <= cantidadManoObra; i++)
                    {
                        string operacion = Request["operacionManoObra" + i];
                        int cantidad = !string.IsNullOrEmpty(Request["cantidadManoObra" + i])
                            ? Convert.ToInt32(Request["cantidadManoObra" + i])
                            : 0;
                        int tiempo = !string.IsNullOrEmpty(Request["tiempoManoObra" + i])
                            ? Convert.ToInt32(Request["tiempoManoObra" + i])
                            : 0;
                        int iva = !string.IsNullOrEmpty(Request["ivaManoObra" + i])
                            ? Convert.ToInt32(Request["ivaManoObra" + i])
                            : 0;
                        int descuento = !string.IsNullOrEmpty(Request["descuentoManoObra" + i])
                            ? Convert.ToInt32(Request["descuentoManoObra" + i])
                            : 0;
                        int valor = !string.IsNullOrEmpty(Request["valorManoObra" + i])
                            ? Convert.ToInt32(Request["valorManoObra" + i])
                            : 0;
                        int tarifa = !string.IsNullOrEmpty(Request["tarifaManoObra" + i])
                            ? Convert.ToInt32(Request["tarifaManoObra" + i])
                            : 0;

                        if (operacion != null)
                        {
                            context.tcotdetallemanoobra.Add(new tcotdetallemanoobra
                            {
                                idencab = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.id : 0,
                                operacion = operacion,
                                cantidad = cantidad,
                                porcendescto = descuento,
                                porceniva = iva,
                                tarifamanoobra = tarifa,
                                tiempo = tiempo,
                                Valor = valor
                            });
                        }
                    }

                    int cantidadReferencias = Convert.ToInt32(Request["cantidadesReferencias"]);
                    for (int i = 1; i <= cantidadReferencias; i++)
                    {
                        string codigo_referencia = Request["referencia" + i];
                        int cantidad = !string.IsNullOrEmpty(Request["cantidadReferencia" + i])
                            ? Convert.ToInt32(Request["cantidadReferencia" + i])
                            : 0;
                        int valorUnitario = !string.IsNullOrEmpty(Request["valorUnitarioReferencia" + i])
                            ? Convert.ToInt32(Request["valorUnitarioReferencia" + i])
                            : 0;
                        int iva = !string.IsNullOrEmpty(Request["ivaReferencia" + i])
                            ? Convert.ToInt32(Request["ivaReferencia" + i])
                            : 0;
                        int descuento = !string.IsNullOrEmpty(Request["descuentoReferencia" + i])
                            ? Convert.ToInt32(Request["descuentoReferencia" + i])
                            : 0;
                        int costo = !string.IsNullOrEmpty(Request["costoReferencia" + i])
                            ? Convert.ToInt32(Request["costoReferencia" + i])
                            : 0;

                        if (codigo_referencia != null)
                        {
                            context.tcotdetallerepuesto.Add(new tcotdetallerepuesto
                            {
                                idencab = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.id : 0,
                                ref_codigo = codigo_referencia,
                                cantidad = cantidad,
                                valorunitario = valorUnitario,
                                porcen_iva = iva,
                                porcen_dscto = descuento,
                                costo_promedio = costo
                            });
                        }
                    }

                    int cantidadToT = Convert.ToInt32(Request["cantidadesToT"]);
                    for (int i = 1; i <= cantidadToT; i++)
                    {
                        string operacion = Request["operacionToT" + i];
                        string descripcion = Request["descripcionToT" + i];
                        int cantidad = !string.IsNullOrEmpty(Request["cantidadToT" + i])
                            ? Convert.ToInt32(Request["cantidadToT" + i])
                            : 0;
                        int tiempo = !string.IsNullOrEmpty(Request["tiempoToT" + i])
                            ? Convert.ToInt32(Request["tiempoToT" + i])
                            : 0;
                        int precio = !string.IsNullOrEmpty(Request["precioToT" + i])
                            ? Convert.ToInt32(Request["precioToT" + i])
                            : 0;
                        int costo = !string.IsNullOrEmpty(Request["costoToT" + i])
                            ? Convert.ToInt32(Request["costoToT" + i])
                            : 0;
                        int iva = !string.IsNullOrEmpty(Request["ivaToT" + i])
                            ? Convert.ToInt32(Request["ivaToT" + i])
                            : 0;
                        int descuento = !string.IsNullOrEmpty(Request["descuentoToT" + i])
                            ? Convert.ToInt32(Request["descuentoToT" + i])
                            : 0;
                        int proveedor = !string.IsNullOrEmpty(Request["proveedorToT" + i])
                            ? Convert.ToInt32(Request["proveedorToT" + i])
                            : 0;

                        if (operacion != null)
                        {
                            context.tcotdetalletot.Add(new tcotdetalletot
                            {
                                idencab = buscarUltimoEncabezado != null ? buscarUltimoEncabezado.id : 0,
                                cantidad = cantidad,
                                operacion = operacion,
                                descripcion = descripcion,
                                tiempo = tiempo,
                                precio = precio,
                                costo = costo,
                                porceniva = iva,
                                porcendescto = descuento,
                                proveedor = proveedor
                            });
                        }
                    }

                    int guardarDetalles = context.SaveChanges();
                    if (guardarDetalles > 0)
                    {
                        // Actualiza los numeros consecutivos por documento
                        int grupoId = grupoConsecutivo != null ? grupoConsecutivo.grupo : 0;
                        System.Collections.Generic.List<icb_doc_consecutivos> gruposConsecutivos = context.icb_doc_consecutivos
                            .Where(x => x.doccons_grupoconsecutivo == grupoId).ToList();
                        foreach (icb_doc_consecutivos grupo in gruposConsecutivos)
                        {
                            grupo.doccons_siguiente = grupo.doccons_siguiente + 1;
                            context.Entry(grupo).State = EntityState.Modified;
                        }

                        context.SaveChanges();
                        TempData["mensaje"] = "El registro de la nueva cotizacion de taller fue exitosa!";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                    }
                }
                else
                {
                    TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                }
            }

            ListasDesplegables(modelo);
            BuscarFavoritos(menu);
            return View();
        }


        public ActionResult Edit(int? id, int? menu)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tencabcotizacion cotizacion = context.tencabcotizacion.Find(id);
            if (cotizacion == null)
            {
                return HttpNotFound();
            }

            ListasDesplegables(cotizacion);
            ViewBag.bodega = new SelectList(context.bodega_concesionario.Where(x => x.id == cotizacion.bodega), "id",
                "bodccs_nombre", cotizacion.bodega);
            BuscarFavoritos(menu);
            return View(cotizacion);
        }


        [HttpPost]
        public ActionResult Edit(tencabcotizacion modelo, int? menu)
        {
            if (ModelState.IsValid)
            {
                modelo.user_idactualizacion = Convert.ToInt32(Session["user_usuarioid"]);
                modelo.fec_actualizacion = DateTime.Now;
                context.Entry(modelo).State = EntityState.Modified;
                int guardar = context.SaveChanges();
                if (guardar > 0)
                {
                    const string query = "DELETE FROM [dbo].[tcotdetallemanoobra] WHERE [idencab]={0}";
                    int rows = context.Database.ExecuteSqlCommand(query, modelo.id);
                    int cantidadManoObra = Convert.ToInt32(Request["cantidadesManoObra"]);
                    for (int i = 1; i <= cantidadManoObra; i++)
                    {
                        string operacion = Request["operacionManoObra" + i];
                        int cantidad = !string.IsNullOrEmpty(Request["cantidadManoObra" + i])
                            ? Convert.ToInt32(Request["cantidadManoObra" + i])
                            : 0;
                        int tiempo = !string.IsNullOrEmpty(Request["tiempoManoObra" + i])
                            ? Convert.ToInt32(Request["tiempoManoObra" + i])
                            : 0;
                        int iva = !string.IsNullOrEmpty(Request["ivaManoObra" + i])
                            ? Convert.ToInt32(Request["ivaManoObra" + i])
                            : 0;
                        int descuento = !string.IsNullOrEmpty(Request["descuentoManoObra" + i])
                            ? Convert.ToInt32(Request["descuentoManoObra" + i])
                            : 0;
                        int valor = !string.IsNullOrEmpty(Request["valorManoObra" + i])
                            ? Convert.ToInt32(Request["valorManoObra" + i])
                            : 0;
                        int tarifa = !string.IsNullOrEmpty(Request["tarifaManoObra" + i])
                            ? Convert.ToInt32(Request["tarifaManoObra" + i])
                            : 0;

                        if (operacion != null)
                        {
                            context.tcotdetallemanoobra.Add(new tcotdetallemanoobra
                            {
                                idencab = modelo.id,
                                operacion = operacion,
                                cantidad = cantidad,
                                porcendescto = descuento,
                                porceniva = iva,
                                tarifamanoobra = tarifa,
                                tiempo = tiempo,
                                Valor = valor
                            });
                        }
                    }

                    const string query2 = "DELETE FROM [dbo].[tcotdetallerepuesto] WHERE [idencab]={0}";
                    int rows2 = context.Database.ExecuteSqlCommand(query2, modelo.id);
                    int cantidadReferencias = Convert.ToInt32(Request["cantidadesReferencias"]);
                    for (int i = 1; i <= cantidadReferencias; i++)
                    {
                        string codigo_referencia = Request["referencia" + i];
                        int cantidad = !string.IsNullOrEmpty(Request["cantidadReferencia" + i])
                            ? Convert.ToInt32(Request["cantidadReferencia" + i])
                            : 0;
                        int valorUnitario = !string.IsNullOrEmpty(Request["valorUnitarioReferencia" + i])
                            ? Convert.ToInt32(Request["valorUnitarioReferencia" + i])
                            : 0;
                        int iva = !string.IsNullOrEmpty(Request["ivaReferencia" + i])
                            ? Convert.ToInt32(Request["ivaReferencia" + i])
                            : 0;
                        int descuento = !string.IsNullOrEmpty(Request["descuentoReferencia" + i])
                            ? Convert.ToInt32(Request["descuentoReferencia" + i])
                            : 0;
                        int costo = !string.IsNullOrEmpty(Request["costoReferencia" + i])
                            ? Convert.ToInt32(Request["costoReferencia" + i])
                            : 0;

                        if (codigo_referencia != null)
                        {
                            context.tcotdetallerepuesto.Add(new tcotdetallerepuesto
                            {
                                idencab = modelo.id,
                                ref_codigo = codigo_referencia,
                                cantidad = cantidad,
                                valorunitario = valorUnitario,
                                porcen_iva = iva,
                                porcen_dscto = descuento,
                                costo_promedio = costo
                            });
                        }
                    }

                    const string query3 = "DELETE FROM [dbo].[tcotdetalletot] WHERE [idencab]={0}";
                    int rows3 = context.Database.ExecuteSqlCommand(query3, modelo.id);
                    int cantidadToT = Convert.ToInt32(Request["cantidadesToT"]);
                    for (int i = 1; i <= cantidadToT; i++)
                    {
                        string operacion = Request["operacionToT" + i];
                        string descripcion = Request["descripcionToT" + i];
                        int cantidad = !string.IsNullOrEmpty(Request["cantidadToT" + i])
                            ? Convert.ToInt32(Request["cantidadToT" + i])
                            : 0;
                        int tiempo = !string.IsNullOrEmpty(Request["tiempoToT" + i])
                            ? Convert.ToInt32(Request["tiempoToT" + i])
                            : 0;
                        int precio = !string.IsNullOrEmpty(Request["precioToT" + i])
                            ? Convert.ToInt32(Request["precioToT" + i])
                            : 0;
                        int costo = !string.IsNullOrEmpty(Request["costoToT" + i])
                            ? Convert.ToInt32(Request["costoToT" + i])
                            : 0;
                        int iva = !string.IsNullOrEmpty(Request["ivaToT" + i])
                            ? Convert.ToInt32(Request["ivaToT" + i])
                            : 0;
                        int descuento = !string.IsNullOrEmpty(Request["descuentoToT" + i])
                            ? Convert.ToInt32(Request["descuentoToT" + i])
                            : 0;
                        int proveedor = !string.IsNullOrEmpty(Request["proveedorToT" + i])
                            ? Convert.ToInt32(Request["proveedorToT" + i])
                            : 0;

                        if (operacion != null)
                        {
                            context.tcotdetalletot.Add(new tcotdetalletot
                            {
                                idencab = modelo.id,
                                cantidad = cantidad,
                                operacion = operacion,
                                descripcion = descripcion,
                                tiempo = tiempo,
                                precio = precio,
                                costo = costo,
                                porceniva = iva,
                                porcendescto = descuento,
                                proveedor = proveedor
                            });
                        }
                    }

                    int guardarDetalles = context.SaveChanges();
                    if (guardarDetalles > 0)
                    {
                        TempData["mensaje"] = "La actualización de la cotizacion de taller fue exitosa!";
                    }
                    else
                    {
                        TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                    }
                }
                else
                {
                    TempData["mensaje_error"] = "Error de conexion con la base de datos, por favor valide...";
                }
            }

            ListasDesplegables(modelo);
            return RedirectToAction("Edit", "cotizacionTaller", new { modelo.id, menu });
        }


        public JsonResult ConsultarActividadTempario(string id_operacion, int? id_bodega, int? id_cliente)
        {
            var buscarTempario = (from tempario in context.ttempario
                                  join tablaIva in context.codigo_iva
                                      on tempario.iva equals tablaIva.id into iva
                                  from tablaIva in iva.DefaultIfEmpty()
                                  where tempario.codigo == id_operacion
                                  select new
                                  {
                                      tempario.tiempo,
                                      tempario.precio,
                                      tablaIva.porcentaje
                                  }).FirstOrDefault();

            //context.ttempario.FirstOrDefault(x=>x.id == id_operacion);
            decimal? tarifa = 0;
            decimal tiempo = 0;
            decimal descuentoManoObra = 0;
            decimal valorIva = 0;
            if (buscarTempario != null)
            {
                valorIva = buscarTempario.porcentaje != null ? buscarTempario.porcentaje ?? 0 : 0;

                if (buscarTempario.tiempo != null)
                {
                    var buscarCliente = (from tercero in context.icb_terceros
                                         join cliente in context.tercero_cliente
                                             on tercero.tercero_id equals cliente.tercero_id
                                         where tercero.tercero_id == id_cliente
                                         select new
                                         {
                                             cliente.cltercero_id
                                         }).FirstOrDefault();
                    int id_tercero_cliente = buscarCliente != null ? buscarCliente.cltercero_id : 0;
                    ttarifastaller buscarTarifaCliente = context.ttarifastaller.FirstOrDefault(x =>
                        x.bodega == id_bodega && x.idtercero == id_tercero_cliente);
                    if (buscarTarifaCliente != null)
                    {
                        tiempo = (decimal)buscarTempario.tiempo;
                        tarifa = Convert.ToDecimal(buscarTarifaCliente.valorhora)  * (decimal)buscarTempario.tiempo;
                    }

                    else
                    {
                        ttarifastaller buscarTarifaTaller = context.ttarifastaller.FirstOrDefault(x => x.bodega == id_bodega);
                        if (buscarTarifaTaller != null)
                        {
                            tiempo = (decimal)buscarTempario.tiempo;
                            tarifa = buscarTarifaTaller.valorhora ?? 0 * (decimal)buscarTempario.tiempo;
                        }
                    }
                }
                else
                {
                    tarifa = buscarTempario.precio ?? 0;
                }
            }

            tercero_cliente buscarTercero = context.tercero_cliente.FirstOrDefault(x => x.tercero_id == id_cliente);
            if (buscarTercero != null)
            {
                //descuentoManoObra = buscarTercero.dscto_mo != null ? (decimal)buscarTercero.dscto_mo : 0;
                descuentoManoObra = (decimal)buscarTercero.dscto_mo;

            }

            return Json(new { tarifa, tiempo, descuentoManoObra, valorIva }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult CargarElementosCotizacion(int? id)
        {
            var buscarManoObra = (from encabezado in context.tencabcotizacion
                                  join manoObra in context.tcotdetallemanoobra
                                      on encabezado.id equals manoObra.idencab
                                  join tempario in context.ttempario
                                      on manoObra.operacion equals tempario.codigo
                                  where encabezado.id == id
                                  select new
                                  {
                                      tempario.codigo,
                                      tempario.operacion,
                                      manoObra.cantidad,
                                      manoObra.tiempo,
                                      manoObra.porceniva,
                                      manoObra.porcendescto,
                                      manoObra.Valor,
                                      manoObra.tarifamanoobra
                                  }).ToList();

            var buscarReferencias = (from encabezado in context.tencabcotizacion
                                     join cotReferencia in context.tcotdetallerepuesto
                                         on encabezado.id equals cotReferencia.idencab
                                     join referencia in context.icb_referencia
                                         on cotReferencia.ref_codigo equals referencia.ref_codigo
                                     where encabezado.id == id
                                     select new
                                     {
                                         referencia.ref_codigo,
                                         referencia.ref_descripcion,
                                         cotReferencia.cantidad,
                                         cotReferencia.valorunitario,
                                         cotReferencia.porcen_iva,
                                         cotReferencia.porcen_dscto,
                                         cotReferencia.costo_promedio
                                     }).ToList();

            var buscarToTs = (from encabezado in context.tencabcotizacion
                              join cotToTs in context.tcotdetalletot
                                  on encabezado.id equals cotToTs.idencab
                              join tempario in context.ttempario
                                  on cotToTs.operacion equals tempario.codigo
                              join proveedor in context.tercero_proveedor
                                  on cotToTs.proveedor equals proveedor.prtercero_id
                              join tercero in context.icb_terceros
                                  on proveedor.tercero_id equals tercero.tercero_id
                              where encabezado.id == id
                              select new
                              {
                                  tempario.codigo,
                                  tempario.operacion,
                                  cotToTs.descripcion,
                                  cotToTs.cantidad,
                                  cotToTs.tiempo,
                                  cotToTs.porceniva,
                                  cotToTs.porcendescto,
                                  cotToTs.precio,
                                  cotToTs.costo,
                                  proveedor.prtercero_id,
                                  proveedorNombre = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " +
                                                    tercero.apellido_tercero + " " + tercero.segapellido_tercero
                              }).ToList();

            return Json(new { buscarManoObra, buscarReferencias, buscarToTs }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarCostoReferencia(string id_referencia, int? id_cliente)
        {
            var buscarReferencia = (from referencia in context.icb_referencia
                                    join promedio in context.vw_promedio
                                        on referencia.ref_codigo equals promedio.codigo
                                    join precios in context.rprecios
                                        on referencia.ref_codigo equals precios.codigo into pre
                                    from precios in pre.DefaultIfEmpty()
                                    where referencia.ref_codigo == id_referencia
                                    select new
                                    {
                                        referencia.ref_codigo,
                                        referencia.ref_descripcion,
                                        referencia.por_iva,
                                        referencia.por_dscto,
                                        referencia.por_dscto_max,
                                        promedio.Promedio,
                                        precios.precio1,
                                        precios.precio2,
                                        precios.precio3,
                                        precios.precio4,
                                        precios.precio5,
                                        precios.precio6,
                                        precios.precio7,
                                        precios.precio8,
                                        precios.precio9
                                    }).FirstOrDefault();

            var buscarPrecioCliente = (from cliente in context.tercero_cliente
                                       where cliente.tercero_id == id_cliente
                                       select new
                                       {
                                           cliente.lprecios_repuestos,
                                           cliente.dscto_rep
                                       }).FirstOrDefault();

            decimal costo = 0;
            decimal precio = 0;
            decimal iva = 0;
            decimal descuento = 0;
            if (buscarPrecioCliente != null && buscarReferencia != null)
            {
                costo = buscarReferencia.Promedio ?? 0;
                //iva = buscarReferencia.por_iva != null ? (decimal)buscarReferencia.por_iva : 0;
                iva = (decimal)buscarReferencia.por_iva;
                decimal descuento_maximo = (decimal)buscarReferencia.por_dscto_max;
                /*decimal descuento_maximo = buscarReferencia.por_dscto_max != null
                    ? (decimal)buscarReferencia.por_dscto_max
                    : 0;*/

                /*if (buscarReferencia.por_dscto == null)
                {
                    if (buscarPrecioCliente.dscto_rep != null)
                    {
                        if (descuento_maximo > (decimal)buscarPrecioCliente.dscto_rep)
                        {
                            descuento = (decimal)buscarPrecioCliente.dscto_rep;
                        }
                        else
                        {
                            descuento = descuento_maximo;
                        }
                    }
                    else
                    {
                        descuento = 0;
                    }

                    //descuento = buscarPrecioCliente.dscto_rep != null ? (decimal)buscarPrecioCliente.dscto_rep : 0;
                    //descuento = descuento_maximo < descuento ? descuento_maximo : descuento;
                }
                else
                {*/
                descuento = (decimal)buscarReferencia.por_dscto;
                //descuento = buscarReferencia.por_dscto != null ? (decimal)buscarReferencia.por_dscto : 0;
                /*decimal descuentoCliente = buscarPrecioCliente.dscto_rep != null
                        ? (decimal)buscarPrecioCliente.dscto_rep
                        : 0;*/
                decimal descuentoCliente = (decimal)buscarPrecioCliente.dscto_rep;
                descuento = descuento < descuentoCliente ? descuentoCliente : descuento;
                    if (descuento_maximo < descuento)
                    {
                        descuento = descuento_maximo;
                    }
                //}

                if (buscarPrecioCliente.lprecios_repuestos == "precio1")
                {
                    precio = buscarReferencia.precio1;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio2")
                {
                    precio = buscarReferencia.precio2;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio3")
                {
                    precio = buscarReferencia.precio3;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio4")
                {
                    precio = buscarReferencia.precio4;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio5")
                {
                    precio = buscarReferencia.precio5;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio6")
                {
                    precio = buscarReferencia.precio6;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio7")
                {
                    precio = buscarReferencia.precio7;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio8")
                {
                    precio = buscarReferencia.precio8;
                }

                if (buscarPrecioCliente.lprecios_repuestos == "precio9")
                {
                    precio = buscarReferencia.precio9;
                }

                return Json(new { costo, precio, iva, descuento }, JsonRequestBehavior.AllowGet);
            }

            costo = buscarReferencia != null ? buscarReferencia.Promedio ?? 0 : 0;

            if (buscarReferencia != null)
            {
                /*descuento = buscarReferencia.por_dscto != null ? (decimal)buscarReferencia.por_dscto : 0;
                decimal descuento_maximoAux = buscarReferencia.por_dscto_max != null
                    ? (decimal)buscarReferencia.por_dscto_max
                    : 0;*/
                descuento = (decimal)buscarReferencia.por_dscto;
                decimal descuento_maximoAux = (decimal)buscarReferencia.por_dscto_max;
                if (descuento_maximoAux < descuento)
                {
                    descuento = descuento_maximoAux;
                }
            }

            return Json(new { costo, precio, iva, descuento }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult CargarCotizacionPorNumero(int? id_cotizacion)
        {
            var buscarCotizacion = (from cotizacion in context.icb_referencia_mov
                                    join cotiza_referencias in context.icb_referencia_movdetalle
                                        on cotizacion.refmov_id equals cotiza_referencias.refmov_id
                                    join referencias in context.icb_referencia
                                        on cotiza_referencias.ref_codigo equals referencias.ref_codigo
                                    where cotizacion.refmov_id == id_cotizacion
                                    select new
                                    {
                                        cotizacion.refmov_id,
                                        referencias.ref_codigo,
                                        referencias.ref_descripcion,
                                        cotiza_referencias.refdet_cantidad,
                                        cotiza_referencias.poriva,
                                        cotiza_referencias.pordscto,
                                        cotiza_referencias.valor_unitario
                                    }).ToList();

            return Json(buscarCotizacion, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarCotizacionParaRepuestos(int? id_bodega, int? numero)
        {
            var buscarCotizacion = (from cotizacion in context.icb_referencia_mov
                                    join tercero in context.icb_terceros
                                        on cotizacion.cliente equals tercero.tercero_id
                                    join bodega in context.bodega_concesionario
                                        on cotizacion.bodega_id equals bodega.id
                                    where cotizacion.refmov_numero == numero && cotizacion.bodega_id == id_bodega
                                    select new
                                    {
                                        cotizacion.refmov_id,
                                        cotizacion.refmov_fecela,
                                        tercero = tercero.prinom_tercero + " " + tercero.segnom_tercero + " " + tercero.apellido_tercero +
                                                  " " + tercero.segapellido_tercero
                                    }).ToList();

            var data = buscarCotizacion.Select(x => new
            {
                x.refmov_id,
                cot_feccreacion = x.refmov_fecela != null ? x.refmov_fecela.ToShortDateString() : "",
                x.tercero
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonResult BuscarBodegasPorTipoDoc(int? id_tpDoc)
        {
            int idUsuarioActual = Convert.ToInt32(Session["user_usuarioid"]);
            System.Collections.Generic.List<int> bodegasUsuarioActual = (from bodegaUsuario in context.bodega_usuario
                                                                         where bodegaUsuario.id_usuario == idUsuarioActual
                                                                         select bodegaUsuario.id_bodega).ToList();
            var buscarBodegasDelDocumento = (from doc_consecutivos in context.icb_doc_consecutivos
                                             join bodega in context.bodega_concesionario
                                                 on doc_consecutivos.doccons_bodega equals bodega.id
                                             where doc_consecutivos.doccons_idtpdoc == id_tpDoc &&
                                                   bodegasUsuarioActual.Contains(doc_consecutivos.doccons_bodega) && bodega.es_taller
                                             select new
                                             {
                                                 bodega.id,
                                                 bodega.bodccs_nombre
                                             }).ToList();

            return Json(buscarBodegasDelDocumento, JsonRequestBehavior.AllowGet);
        }


        public void ListasDesplegables(tencabcotizacion cotizacion)
        {
            icb_sysparameter idCotizacionTaller = context.icb_sysparameter.FirstOrDefault(x => x.syspar_cod == "P47");
            int idCotizacion = idCotizacionTaller != null ? Convert.ToInt32(idCotizacionTaller.syspar_value) : 0;
            ViewBag.tipodoc = new SelectList(context.tp_doc_registros.Where(x => x.tipo == idCotizacion), "tpdoc_id",
                "tpdoc_nombre", cotizacion.tipodoc);

            DbSet<bodega_concesionario> bodegas = context.bodega_concesionario;
            //ViewBag.bodega = new SelectList(bodegas, "id", "bodccs_nombre",cotizacion.bodega);
            ViewBag.bodegasCotizacion = bodegas;

            var buscarClientes = (from terceros in context.icb_terceros
                                  join clientes in context.tercero_cliente
                                      on terceros.tercero_id equals clientes.tercero_id
                                  select new
                                  {
                                      terceros.tercero_id,
                                      nombre = "(" + terceros.doc_tercero + ") " + terceros.prinom_tercero + " " +
                                               terceros.segnom_tercero + " " + terceros.apellido_tercero + " " +
                                               terceros.segapellido_tercero + " " + terceros.razon_social
                                  }).ToList();
            ViewBag.cliente = new SelectList(buscarClientes, "tercero_id", "nombre", cotizacion.cliente);

            var buscarAsesores = (from usuarios in context.users
                                  where usuarios.rol_id == 4
                                  select new
                                  {
                                      usuarios.user_id,
                                      nombre = usuarios.user_nombre + " " + usuarios.user_apellido
                                  }).ToList();
            ViewBag.asesor = new SelectList(buscarAsesores, "user_id", "nombre", cotizacion.asesor);

            ViewBag.aseguradora = new SelectList(context.icb_aseguradoras, "aseg_id", "nombre", cotizacion.aseguradora);
            ViewBag.tipocotizacion =
                new SelectList(context.ttipocotizacion, "id", "descripcion", cotizacion.tipocotizacion);

            DbSet<ttempario> tempario = context.ttempario;
            ViewBag.selectOperacionManoObra = new SelectList(tempario, "id", "operacion");
            ViewBag.selectOperacionToT = new SelectList(tempario, "id", "operacion");

            ViewBag.selectReferencia = new SelectList(context.icb_referencia.Where(x => x.modulo == "R"), "ref_codigo",
                "ref_descripcion");

            var buscarProveedores = (from terceros in context.icb_terceros
                                     join proveedor in context.tercero_proveedor
                                         on terceros.tercero_id equals proveedor.tercero_id
                                     select new
                                     {
                                         proveedor.prtercero_id,
                                         nombre = terceros.prinom_tercero + " " + terceros.segnom_tercero + " " + terceros.apellido_tercero +
                                                  " " + terceros.segapellido_tercero
                                     }).ToList();
            ViewBag.selectProveedorToT = new SelectList(buscarProveedores, "prtercero_id", "nombre");
        }


        public JsonResult BuscarCotizacionesTaller()
        {
            var buscarCotizaciones = (from cotizacion in context.tencabcotizacion
                                      join bodega in context.bodega_concesionario
                                          on cotizacion.bodega equals bodega.id
                                      join cliente in context.icb_terceros
                                          on cotizacion.cliente equals cliente.tercero_id
                                      select new
                                      {
                                          cotizacion.id,
                                          cotizacion.fecha_creacion,
                                          cotizacion.numero,
                                          bodega.bodccs_nombre,
                                          cliente = cliente.prinom_tercero + " " + cliente.segnom_tercero + " " + cliente.apellido_tercero +
                                                    " " + cliente.segapellido_tercero
                                      }).ToList();

            var data = buscarCotizaciones.Select(x => new
            {
                x.id,
                x.numero,
                fecha_creacion = x.fecha_creacion.ToShortDateString(),
                x.bodccs_nombre,
                x.cliente
            });

            return Json(data, JsonRequestBehavior.AllowGet);
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
    }
}