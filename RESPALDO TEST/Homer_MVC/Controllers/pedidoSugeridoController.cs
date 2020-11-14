using Homer_MVC.IcebergModel;
using Homer_MVC.Models;
using Homer_MVC.ViewModels.medios;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class pedidoSugeridoController : Controller
    {
        private readonly Iceberg_Context context = new Iceberg_Context();

        public class listadosugerido
        {
            public int bod { get; set; }
            public List<string> refer { get; set; }
            public List<string> cant { get; set; }
        }

        // GET: pedidoSugerido
        public ActionResult Create(int? menu)
        {
            List<string> titulos = new List<string>
                {"Bodega", "Referencia","Descripcion", "Promedio Ventas", "Stock Actual", "Sugerencia", "Diferencia","Backorder", "Promedio Costo", "Clasificacion ABC","Seleccionar"};
            ViewBag.titulos = titulos;
            ViewBag.datos = "";
            ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
            ViewBag.referencia_codigo = new List<string>();
            ViewBag.periodo_eval = "";
            BuscarFavoritos(menu);
            return View();
        }


        [HttpPost]
        public ActionResult Create(int[] bodccs_cod, string[] referencia_codigo, int? txtMesesEvaluar, int? txtMesesSugerir, int? menu)
        {
            bool ckeckSoloVentas = true; // Solo ventas por defecto
            //bool checkVentasAjustes = false;
            bool checkSoloExistencias = false; // Sin importar stock, por defecto

            bodccs_cod = bodccs_cod != null ? bodccs_cod : new int[0];
            referencia_codigo = referencia_codigo != null ? referencia_codigo : new string[0];
            txtMesesEvaluar = txtMesesEvaluar != null ? txtMesesEvaluar : 0;
            txtMesesSugerir = txtMesesSugerir != null ? txtMesesSugerir : 1;
            int restarMeses = (txtMesesEvaluar ?? 0) * -1;
            DateTime fechaEmpezar = DateTime.Now.AddMonths(restarMeses - 1);
            DateTime fechaHoy = DateTime.Now;
            string[] mesesLetras = { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
            List<string> titulos = new List<string>();
            var des_refs = new List<string>();
            List<FechasPedidoSugerido> FechasClase = new List<FechasPedidoSugerido>();
            titulos.Add("Bodega");
            titulos.Add("Referencia");
            titulos.Add("Descripcion");
            for (int i = 1; i <= txtMesesEvaluar; i++)
            {
                titulos.Add(mesesLetras[fechaEmpezar.AddMonths(i).Month - 1] + " " + fechaEmpezar.AddMonths(i).Year);
                FechasClase.Add(new FechasPedidoSugerido
                { Anio = fechaEmpezar.AddMonths(i).Year, Mes = fechaEmpezar.AddMonths(i).Month });
            }

            titulos.Add("Promedio Ventas");
            titulos.Add("Stock Actual");
            int tiempoSugerir = txtMesesSugerir ?? 0;
            titulos.Add("Sugerencia " + (tiempoSugerir > 1 ? tiempoSugerir + " meses" : ""));
            titulos.Add("Diferencia");
            titulos.Add("Backorder");
            titulos.Add("Promedio Costo");
            titulos.Add("Clasificacion ABC");
            titulos.Add("Seleccionar");

            var allCode = "allRef";
            var allRef = referencia_codigo.Contains(allCode);
            if (allRef)
            {
                des_refs.Add(allCode + "|Todas Las Referencias");
            }
            List<PedidoSugeridoModel> modelo = new List<PedidoSugeridoModel>();

            for (int i = 0; i < bodccs_cod.Length; i++)
            {
                int bodegaid = bodccs_cod[i];
                var buscarReferencias2 = (from referencia in context.icb_referencia
                                            join promedio in context.vw_inventario_hoy
                                                on referencia.ref_codigo equals promedio.ref_codigo into pro
                                            from promedio in pro.DefaultIfEmpty()
                                            join rp in (from rc in context.rprecarga /*where !rc.seleccion*/
                                                        group rc by new { rc.numero, rc.pedidogm, rc.codigo, rc.cant_ped }
                                                             into p
                                                      select new
                                                      {
                                                          numero = p.Select(x => x.numero).FirstOrDefault(),
                                                          codigo = p.Select(x => x.codigo).FirstOrDefault(),
                                                          pedidogm = p.Select(x => x.pedidogm).FirstOrDefault(),
                                                          cant_ped = p.Select(x => x.cant_ped).FirstOrDefault(),
                                                          cant_fact = p.Select(x => x.cant_fact).Sum(),
                                                          cant_backorder = p.Select(x => x.cant_ped).FirstOrDefault() - p.Select(x => x.cant_fact).Sum(),
                                                          fec_creacion = p.Select(x => x.fec_creacion).FirstOrDefault()
                                                      })
                                             on new { Key1 = promedio.ref_codigo, Key2 = (int)promedio.ano, Key3 = (int)promedio.mes } equals new { Key1 = rp.codigo, Key2 = rp.fec_creacion.Year, Key3 = rp.fec_creacion.Month } into a
                                          from rp in a.DefaultIfEmpty()
                                          where ( (allRef) ? referencia.modulo == "R" : referencia_codigo.Contains(referencia.ref_codigo) ) && promedio.ano == fechaHoy.Year &&
                                                promedio.mes == fechaHoy.Month && promedio.bodega == bodegaid
                                            select new
                                            {
                                                clasificacion = promedio.clasificacion_ABC.ToUpper(),
                                                promedio.nombreBodega,
                                                idbodega = promedio.bodega,
                                                promedio.stock,
                                                referencia.ref_codigo,
                                                referencia.ref_descripcion,
                                                Promedio = promedio.costo_prom,
                                                sumaMes = (from inventario in context.referencias_inven
                                                           where inventario.codigo == referencia.ref_codigo && inventario.ano >= fechaEmpezar.Year &&
                                                                inventario.bodega == bodegaid
                                                            group inventario by new { inventario.ano, inventario.mes }
                                                    into g
                                                            select new
                                                            {
                                                                anio = g.Select(x => x.ano).FirstOrDefault(),
                                                                mes = g.Select(x => x.mes).FirstOrDefault(),
                                                                sumaMeses = (ckeckSoloVentas)? g.Select(x => x.can_vta).Sum() - g.Select(x => x.can_dev_vta).Sum() :
                                                                g.Select(x => x.can_vta).Sum() + g.Select(x => x.can_otr_sal).Sum() - g.Select(x => x./*inventario.*/can_dev_vta).Sum(),
                                                                cantIni = g.Select(x => x.can_ini).Sum(),
                                                                cantEntg = g.Select(x => x.can_ent).Sum(),
                                                                cantSal = g.Select(x => x.can_sal).Sum(),
                                                            }).OrderBy(x => x.anio).ThenBy(x => x.mes).ToList(),
                                                cant_ped = rp.cant_ped != null ? rp.cant_ped : 0,
                                                cant_fact = rp.cant_fact != null ? rp.cant_fact : 0,
                                                cantidad_backorder = (from pre in context.rprecarga where pre.codigo == referencia.ref_codigo select pre.codigo).FirstOrDefault() != null ?   (from pre in context.rprecarga where pre.codigo == referencia.ref_codigo select ((pre.cant_ped) - pre.cant_fact)).Sum()  : 0,
                                                //cant_backorder = rp.cant_backorder != null ? rp.cant_backorder : 0
                                            }).ToList();

                foreach (var referencia in buscarReferencias2)
                {
                    PedidoSugeridoModel nuevo = new PedidoSugeridoModel
                    {
                        Referencia = referencia.ref_descripcion ,
                        codigo = referencia.ref_codigo,
                        Clasificacion = referencia.clasificacion,
                        bodega = referencia.nombreBodega
                    };
                    decimal promedio = 0;
                    decimal stock = 0;
                    decimal backorder = 0;
                    foreach (FechasPedidoSugerido fechasSeguidas in FechasClase)
                    {
                        bool fechaEncontrada = false;
                        foreach (var meses in referencia.sumaMes)
                        {
                            if (meses.mes == fechasSeguidas.Mes && meses.anio == fechasSeguidas.Anio)
                            {
                                fechaEncontrada = true;
                                nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido { Valor = meses.sumaMeses });
                                promedio += meses.sumaMeses;
                                stock = referencia.stock;
                                backorder = Convert.ToDecimal(referencia.cantidad_backorder);
                                break;
                            }
                        }
                        if (fechaEncontrada == false)
                        {
                            nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido { Valor = 0 });
                        }
                    }

                    txtMesesEvaluar = txtMesesEvaluar != 0 ? txtMesesEvaluar : 1;
                    var promedioVentas=(promedio / txtMesesEvaluar ?? 1);//promedio de ventas
                    nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido { Valor = promedio / txtMesesEvaluar ?? 1 });//promedio de ventas
                    nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido { Valor = stock });//stock

                    if (promedioVentas > 0)
                    {
                        nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido { Valor = stock - backorder - ((promedio / txtMesesEvaluar ?? 1) * txtMesesSugerir ?? 0) }); /*sugerencia*/
                        nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido { Valor = ((promedio / txtMesesEvaluar ?? 1) * txtMesesSugerir ?? 0) - stock });//diferencia
                        nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido { Valor = backorder });
                        nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido { Valor = referencia.Promedio ?? 0 });
                        decimal cantidad = Math.Round((promedio / txtMesesEvaluar ?? 1) * txtMesesSugerir ?? 0);
                        nuevo.cantidad = stock - backorder - ((promedio / txtMesesEvaluar ?? 1) * txtMesesSugerir ?? 0);
                        var sugerencia = Math.Round(stock - backorder - ((promedio / txtMesesEvaluar ?? 1) * txtMesesSugerir ?? 0));
                        nuevo.codigo = referencia.idbodega + "," + referencia.ref_codigo + "," + sugerencia;
                        //nuevo.codigo = referencia.idbodega + "," + referencia.ref_codigo + "," + cantidad;
                    }
                    else {

                        nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido { Valor = 0 }); /*sugerencia*/
                        nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido { Valor = 0 });//diferencia
                        nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido { Valor = backorder });
                        nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido { Valor = referencia.Promedio ?? 0 });
                        decimal cantidad = Math.Round((promedio / txtMesesEvaluar ?? 1) * txtMesesSugerir ?? 0);
                        nuevo.cantidad = stock - backorder - ((promedio / txtMesesEvaluar ?? 1) * txtMesesSugerir ?? 0);
                        var sugerencia = Math.Round(stock - backorder - ((promedio / txtMesesEvaluar ?? 1) * txtMesesSugerir ?? 0));
                        nuevo.codigo = referencia.idbodega + "," + referencia.ref_codigo + "," + 0;
                        //nuevo.codigo = referencia.idbodega + "," + referencia.ref_codigo + "," + cantidad;

                    }






                    if (checkSoloExistencias)
                    {
                        if (stock != 0)
                        {
                            modelo.Add(nuevo);
                        }
                    }
                    else
                    {
                        modelo.Add(nuevo);
                    }
                    //Recuperar referencias
                    if (!allRef)
                    {
                        var tempref = referencia.ref_codigo + "|" + referencia.ref_descripcion.ToUpper();
                        if (!des_refs.Contains(tempref))
                        {
                            des_refs.Add(tempref);
                        }
                    }

                }
            }
            ViewBag.titulos = titulos;
            ViewBag.datos = modelo;
            ViewBag.bodccs_cod = context.bodega_concesionario.OrderBy(x => x.bodccs_nombre).ToList();
            ViewBag.referencia_codigo = des_refs;
            ViewBag.periodo_eval = txtMesesEvaluar;
            RetornarValoresMultiselect(bodccs_cod, referencia_codigo);
            BuscarFavoritos(menu);
            return View();
        }

        public JsonResult CrearSolicitudCompra(string referencias)
        {
            rsolicitudesrepuestos modelo = new rsolicitudesrepuestos();
            listadosugerido[] listado = JsonConvert.DeserializeObject<listadosugerido[]>(referencias);

            for (int i = 0; i < listado.Length; i++)
            {
                int guardar = 0;
                for (int h = 0; h < listado[i].cant.Count; h++)
                {
                    if (Convert.ToInt32(listado[i].cant[h]) > 0)
                    {
                        guardar = 1;
                    }
                }
                if (guardar == 1)
                {
                    modelo.bodega = listado[i].bod;
                    modelo.fecha = DateTime.Now;
                    modelo.usuario = Convert.ToInt32(Session["user_usuarioid"]);
                    modelo.Detalle = "Solicitud realizada desde pedido sugerido";
                    modelo.tiposolicitud = 3;
                    modelo.estado_solicitud = 1;
                    modelo.tipo_compra = 2;
                    modelo.cliente= Convert.ToInt32(Session["user_usuarioid"]);
                    context.rsolicitudesrepuestos.Add(modelo);
                    context.SaveChanges();

                    for (int j = 0; j < listado[i].refer.Count; j++)
                    {
                        if (Convert.ToInt32(listado[i].cant[j]) > 0)
                        {
                            string codigo = listado[i].refer[j];
                            icb_referencia buscar = context.icb_referencia.Where(x => x.modulo == "R" && x.ref_estado && x.ref_codigo == codigo).FirstOrDefault();
                            rdetallesolicitud detalle = new rdetallesolicitud
                            {
                                id_solicitud = modelo.id,
                                referencia = buscar.ref_codigo,
                                cantidad = Convert.ToInt32(listado[i].cant[j]),
                                iva = buscar.por_iva != null ? Convert.ToInt32(buscar.por_iva) : 0,
                                valor = buscar.precio_venta,
                                userid_creacion = Convert.ToInt32(Session["user_usuarioid"]),
                                fecha_creacion = DateTime.Now,
                                esta_pedido=1
                            };
                            context.rdetallesolicitud.Add(detalle);
                        }
                    }
                }
            }
            int result = context.SaveChanges();
            if (result > 0)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AgregarReferencia(string codigo, string[] demas)
        {
            if (!string.IsNullOrWhiteSpace(codigo))
            {
                if (demas.Count() == 0 || !demas.Contains(codigo))
                {
                    var data = (from r in context.icb_referencia where r.ref_codigo == codigo select new
                    {
                        id = r.ref_codigo,
                        nombre = r.ref_descripcion.ToUpper(),
                        selected = 1
                    }).FirstOrDefault();
                    return Json(data);
                }
            }
            return Json(0);
        }

        public JsonResult BuscarReferencias(string id, string[] otras)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                System.Linq.Expressions.Expression<Func<icb_referencia, bool>> predicado = PredicateBuilder.True<icb_referencia>();
                System.Linq.Expressions.Expression<Func<icb_referencia, bool>> predicado2 = PredicateBuilder.False<icb_referencia>();

                if (!string.IsNullOrWhiteSpace(otras[0]))
                {
                    //var otrasarray = otras.Split(',');
                    foreach (string item in otras)
                    {
                        predicado2 = predicado2.Or(d => d.ref_codigo == item);
                    }
                }
                //predicado = predicado.And(predicado2);

                predicado2 = predicado2.Or(d => d.ref_codigo.Contains(id));
                predicado2 = predicado2.Or(d => d.ref_descripcion.ToUpper().Contains(id.ToUpper()));
                predicado = predicado.And(predicado2);


                List<icb_referencia> listareferencias = context.icb_referencia.Where(predicado).ToList();
                var data = listareferencias.Select(d => new
                {
                    id = d.ref_codigo,
                    nombre = d.ref_descripcion.ToUpper(),
                    selected = verificarseleccionado(otras, d.ref_codigo)
                }).ToList();
                return Json(data);
            }

            return Json(0);
        }

        public int verificarseleccionado(string[] otras, string codigo)
        {
            int resultado = 0;
            List<string> codigos = new List<string>();
            if (!string.IsNullOrWhiteSpace(otras[0]))
            {
                foreach (string item in otras)
                {
                    codigos.Add(item);
                }
            }

            if (codigos.Contains(codigo))
            {
                resultado = 1;
            }

            return resultado;
        }

        public void RetornarValoresMultiselect(int[] bodccs_cod, string[] referencia_codigo)
        {
            string bodegas = "";
            bool primera = true;
            for (int i = 0; i < bodccs_cod.Length; i++)
            {
                if (primera)
                {
                    bodegas += bodccs_cod[i];
                    primera = false;
                }
                else
                {
                    bodegas += "," + bodccs_cod[i];
                }
            }

            ViewBag.bodegasSeleccionadas = bodegas;

            string referencias = "";
            primera = true;
            for (int i = 0; i < referencia_codigo.Length; i++)
            {
                if (primera)
                {
                    referencias += referencia_codigo[i];
                    primera = false;
                }
                else
                {
                    referencias += "," + referencia_codigo[i];
                }
            }

            ViewBag.referenciasSeleccionadas = referencias;
        }


        //public JsonResult FiltrarBusqueda(int[] bodegas,string[] referencias, int ? mesesEvaluar,int ? mesesSugerir, bool ventas,bool ventasAjustes) {
        //    int restarMeses = (mesesEvaluar ?? 0) * (-1);
        //    DateTime fechaEmpezar = DateTime.Now.AddMonths(restarMeses);
        //    string[] mesesLetras = new string[] {"Ene","Feb","Mar","Abr","May","Jun","Jul","Ago","Sep","Oct","Nov","Dic"};
        //    List<string> titulos = new List<string>();
        //    List<FechasPedidoSugerido> FechasClase = new List<FechasPedidoSugerido>();
        //    titulos.Add("Referencia");
        //    for (int i = 1; i <= mesesEvaluar; i++) {
        //        titulos.Add(mesesLetras[fechaEmpezar.AddMonths(i).Month-1] + " " + fechaEmpezar.AddMonths(i).Year.ToString());
        //        FechasClase.Add(new FechasPedidoSugerido() { Anio= fechaEmpezar.AddMonths(i).Year , Mes= fechaEmpezar.AddMonths(i).Month });
        //    }
        //    titulos.Add("Promedio Cantidad");
        //    titulos.Add("Stock");
        //    var tiempoSugerir = mesesSugerir ?? 0;
        //    titulos.Add("Sugerencia "+tiempoSugerir.ToString()+" meses");
        //    titulos.Add("Diferencia");
        //    titulos.Add("Promedio Costo");

        //    if (ventas)
        //    {
        //        var buscarReferencias = (from referencia in context.icb_referencia
        //                                 join promedio in context.vw_promedio
        //                                 on referencia.ref_codigo equals promedio.codigo into pro
        //                                 from promedio in pro.DefaultIfEmpty()
        //                                 where referencias.Contains(referencia.ref_codigo)
        //                                 select new
        //                                 {
        //                                     referencia.ref_codigo,
        //                                     referencia.ref_descripcion,
        //                                     promedio.Promedio,
        //                                     sumaMes = (from inventario in context.referencias_inven
        //                                                where inventario.codigo == referencia.ref_codigo && inventario.ano >= fechaEmpezar.Year && bodegas.Contains(inventario.bodega)
        //                                                group inventario by new { inventario.ano, inventario.mes } into g
        //                                                select new
        //                                                {
        //                                                    anio = g.Select(x => x.ano).FirstOrDefault(),
        //                                                    mes = g.Select(x => x.mes).FirstOrDefault(),
        //                                                    sumaMeses = g.Select(x => x.can_vta).Sum() - g.Select(x => x.can_dev_vta).Sum(),
        //                                                    cantIni = g.Select(x => x.can_ini).Sum(),
        //                                                    cantEntg = g.Select(x => x.can_ent).Sum(),
        //                                                    cantSal = g.Select(x => x.can_sal).Sum()
        //                                                }).OrderBy(x => x.anio).ThenBy(x => x.mes).ToList()
        //                                 }).ToList();

        //        List<PedidoSugeridoModel> modelo = new List<PedidoSugeridoModel>();
        //        foreach (var referencia in buscarReferencias)
        //        {
        //            PedidoSugeridoModel nuevo = new PedidoSugeridoModel();
        //            nuevo.Referencia = referencia.ref_descripcion;
        //            decimal promedio = 0;
        //            decimal stock = 0;
        //            foreach (var fechasSeguidas in FechasClase)
        //            {
        //                bool fechaEncontrada = false;
        //                foreach (var meses in referencia.sumaMes)
        //                {
        //                    if (meses.mes == fechasSeguidas.Mes && meses.anio == fechasSeguidas.Anio)
        //                    {
        //                        fechaEncontrada = true;
        //                        nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido() { Valor = meses.sumaMeses ?? 0 });
        //                        promedio += meses.sumaMeses ?? 0;
        //                        stock += meses.cantIni + meses.cantEntg - meses.cantSal;
        //                        break;
        //                    }
        //                }
        //                if (fechaEncontrada == false)
        //                {
        //                    nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido() { Valor = 0 });
        //                }
        //            }
        //            nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido() { Valor = promedio/mesesEvaluar??1 });
        //            nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido() { Valor = stock });
        //            nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido() { Valor = (promedio / mesesEvaluar ?? 1) * mesesSugerir??0 });
        //            nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido() { Valor = ((promedio / mesesEvaluar ?? 1) * mesesSugerir ?? 0) - stock });
        //            nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido() { Valor = referencia.Promedio??0 });
        //            modelo.Add(nuevo);
        //        }

        //        var armarJson = new { titulos = titulos, datos = modelo };

        //        return Json(armarJson, JsonRequestBehavior.AllowGet);


        //    }
        //    else if (ventasAjustes)
        //    {
        //        var buscarReferencias = (from referencia in context.icb_referencia
        //                                 join promedio in context.vw_promedio
        //                                 on referencia.ref_codigo equals promedio.codigo into pro
        //                                 from promedio in pro.DefaultIfEmpty()
        //                                 where referencias.Contains(referencia.ref_codigo)
        //                                 select new
        //                                 {
        //                                     referencia.ref_codigo,
        //                                     referencia.ref_descripcion,
        //                                     promedio.Promedio,
        //                                     sumaMes = (from inventario in context.referencias_inven
        //                                                where inventario.codigo == referencia.ref_codigo && inventario.ano >= fechaEmpezar.Year && bodegas.Contains(inventario.bodega)
        //                                                group inventario by new { inventario.ano, inventario.mes } into g
        //                                                select new
        //                                                {
        //                                                    anio = g.Select(x => x.ano).FirstOrDefault(),
        //                                                    mes = g.Select(x => x.mes).FirstOrDefault(),
        //                                                    sumaMeses = g.Select(x => x.can_vta).Sum() + g.Select(x => x.can_otr_sal).Sum() - g.Select(x => x.can_dev_vta).Sum(),
        //                                                    cantIni = g.Select(x => x.can_ini).Sum(),
        //                                                    cantEntg = g.Select(x => x.can_ent).Sum(),
        //                                                    cantSal = g.Select(x => x.can_sal).Sum()
        //                                                }).OrderBy(x => x.anio).ThenBy(x => x.mes).ToList()
        //                                 }).ToList();

        //        List<PedidoSugeridoModel> modelo = new List<PedidoSugeridoModel>();
        //        foreach (var referencia in buscarReferencias)
        //        {
        //            PedidoSugeridoModel nuevo = new PedidoSugeridoModel();
        //            nuevo.Referencia = referencia.ref_descripcion;
        //            decimal promedio = 0;
        //            decimal stock = 0;
        //            foreach (var fechasSeguidas in FechasClase)
        //            {
        //                bool fechaEncontrada = false;
        //                foreach (var meses in referencia.sumaMes)
        //                {
        //                    if (meses.mes == fechasSeguidas.Mes && meses.anio == fechasSeguidas.Anio)
        //                    {
        //                        fechaEncontrada = true;
        //                        nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido() { Valor = meses.sumaMeses ?? 0 });
        //                        promedio += meses.sumaMeses ?? 0;
        //                        stock += meses.cantIni + meses.cantEntg - meses.cantSal;
        //                        break;
        //                    }
        //                }
        //                if (fechaEncontrada == false)
        //                {
        //                    nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido() { Valor = 0 });
        //                }
        //            }
        //            nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido() { Valor = promedio / mesesEvaluar ?? 1 });
        //            nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido() { Valor = stock });
        //            nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido() { Valor = (promedio / mesesEvaluar ?? 1) * mesesSugerir ?? 0 });
        //            nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido() { Valor = ((promedio / mesesEvaluar ?? 1) * mesesSugerir ?? 0) - stock });
        //            nuevo.ValoresPorFecha.Add(new FechasPedidoSugerido() { Valor = referencia.Promedio ?? 0 });
        //            modelo.Add(nuevo);
        //        }

        //        var armarJson = new { titulos = titulos, datos = modelo };

        //        return Json(armarJson, JsonRequestBehavior.AllowGet);
        //    }
        //    else {
        //        return Json(true, JsonRequestBehavior.AllowGet);
        //    }


        //}


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