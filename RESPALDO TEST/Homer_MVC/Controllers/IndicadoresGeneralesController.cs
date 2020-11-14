using Homer_MVC.IcebergModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Homer_MVC.Controllers
{
    public class IndicadoresGeneralesController : Controller
    {
        // GET: IndicadoresGenerales
        public ActionResult Index()
        {
            return View();
        }
        private readonly Iceberg_Context context = new Iceberg_Context();
        CultureInfo miCultura = new CultureInfo("is-IS");
        private CultureInfo elGR = CultureInfo.CreateSpecificCulture("el-GR");


        public JsonResult jsonIndicadoresGenerales()
            {




            var Resultado = "";
            var fecha = DateTime.Now;
            try
                {
                var ano = DateTime.Now.Year;
                var mespasado = DateTime.Now.Month - 1;
                var fechainicial = Convert.ToDateTime(DateTime.Now.Year + "-" + mespasado + "-01");
                var firstdayofmonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var ultimo_dia = firstdayofmonth.AddDays(-1);
                var fechafin = Convert.ToDateTime(ultimo_dia);


                //var Datoinvenini = context.referencias_inven.Where(x => x.ano == ano && x.mes == mespasado).Sum(s => s.cos_ini).ToString();
                var Datoinvenini = context.referencias_inven.Where(x => x.ano == ano && x.mes == mespasado).Sum(s => s.cos_ini);
                //inventario incial
                decimal Datoinveniniins = Convert.ToDecimal(Datoinvenini, miCultura);

                //var comprasGM_ingreso = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 1).Select(s => s.valor_total).DefaultIfEmpty(0).Sum().ToString();
                var comprasGM_ingreso = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 1).Select(s => s.valor_documento).DefaultIfEmpty(0).Sum();

                //compras ingresos gm
                decimal comprasGM_ingresoins = comprasGM_ingreso!=null?Convert.ToDecimal(comprasGM_ingreso.Value, miCultura):0;

                //var compras_Concesionario = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 2).Select(s => s.valor_total).DefaultIfEmpty(0).Sum().ToString();
                var compras_Concesionario = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 2).Select(s => s.valor_documento).DefaultIfEmpty(0).Sum();
                //compras concesionarios
                decimal compras_Concesionarioins = compras_Concesionario != null ? Convert.ToDecimal(compras_Concesionario.Value, miCultura):0;

                var compras_otros_rep = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 3).Select(s => s.costorepuestos).DefaultIfEmpty(0).Sum();

                // compras  otros repuestos
                decimal compras_otros_repins = compras_otros_rep!=null?Convert.ToDecimal(compras_otros_rep.Value, miCultura):0;


                //var compras_otros_acce = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 3).Select(s => s.costoaccesorios).DefaultIfEmpty(0).Sum().ToString();
                var compras_otros_acce = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 3).Select(s => s.costoaccesorios).DefaultIfEmpty(0).Sum();
                //compras otros accesorios
                decimal compras_otros_acceins = compras_otros_acce!=null?Convert.ToDecimal(compras_otros_acce.Value, miCultura):0;


                //var compras_total = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin).Select(s => s.valor_documento).DefaultIfEmpty(0).Sum().ToString();
                var compras_total = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin).Select(s => s.valor_documento).DefaultIfEmpty(0).Sum();
                //total compras 
                decimal compras_totalins = compras_total!=null?Convert.ToDecimal(compras_total.Value, miCultura):0;

                //var total_ventas = context.vw_Cantidades_Bodega.Where(x => x.fecha_creacion >= fechainicial && x.fecha_creacion <= fechafin).Select(s => s.valor_total).DefaultIfEmpty(0).Sum().ToString();
                var total_ventas = context.vw_Cantidades_Bodega.Where(x => x.fecha_creacion >= fechainicial && x.fecha_creacion <= fechafin).Select(s => s.valor_total).DefaultIfEmpty(0).Sum();

                //total ventas
                decimal total_ventasins = Convert.ToDecimal(total_ventas, miCultura);


                var ajuste_inventario = context.referencias_inven.Where(x => x.ano == ano && x.mes == mespasado).Select(s => new { costo = s.cos_ent_aju + s.cos_ini_aju, s.cos_sal_aju }).ToList();

                //ajuste inventario
                decimal ajuste_inventarioins = Convert.ToDecimal(ajuste_inventario.Sum(s => s.costo - s.cos_sal_aju), miCultura);

                //var costo_venta = context.vw_Cantidades_Bodega.Where(x => x.fecha_creacion >= fechainicial && x.fecha_creacion <= fechafin).Select(s => s.sumavalor).DefaultIfEmpty(0).Sum().ToString();
                var costo_venta = context.vw_Cantidades_Bodega.Where(x => x.fecha_creacion >= fechainicial && x.fecha_creacion <= fechafin).Select(s => s.sumavalor).DefaultIfEmpty(0).Sum();
                //costo venta
                decimal costo_ventains = costo_venta!=null?Convert.ToDecimal(costo_venta.Value, miCultura):0;


                // inventario final
                decimal Inventa_finalins = (Datoinveniniins + compras_totalins + ajuste_inventarioins) - costo_ventains;



                //var Datoinveninienero = context.referencias_inven.Where(x => x.ano == ano && x.mes == 1).Select(s => s.cos_ini).DefaultIfEmpty(0).Sum().ToString();
                var Datoinveninienero = context.referencias_inven.Where(x => x.ano == ano && x.mes == 1).Select(s => s.cos_ini).DefaultIfEmpty(0).Sum();
                //diferencia respecto a enero
                decimal diferencia = Inventa_finalins - Convert.ToDecimal(Datoinveninienero, miCultura);

                //inventario en proceso 
                decimal inv_proceso = Convert.ToDecimal(0, miCultura);

                //inv. disponible

                decimal inv_disponible = Inventa_finalins - inv_proceso;


                var anopass = ano - 1;
                var inv_anopass = context.indicadores_generales.Where(x => x.Año == anopass && x.mes == mespasado).Select(x => x.inventario_final).DefaultIfEmpty(0).Sum();
                //inventario final Ano anterior
                decimal valorinventpass = inv_anopass!=null?Convert.ToDecimal(inv_anopass.Value, miCultura):0;
                decimal variacion = Convert.ToDecimal(0, miCultura);
                if (valorinventpass==0)
                    {
                    variacion = 0;
                    }
                else
                    {
                    variacion = (Inventa_finalins / valorinventpass) - 1;
                    }
                //variacion inventario 
                



                //Rotacion
                var Costo_prom = context.vw_costo_mes_bodega.Where(x => x.año == ano && x.mes == mespasado).
      Select(f => new { f.Costo_Total }).ToList();
                decimal Costo_prom2 = Convert.ToDecimal(Costo_prom.Sum(e => e.Costo_Total));
                var valor_invent = context.vw_valor_inventario_bodega.Where(d => d.ano == ano && d.mes == mespasado).
                   Sum(a => a.Total);
                decimal valor_invent2 = Convert.ToDecimal(valor_invent, miCultura);

                               
                decimal rotacion = Convert.ToDecimal(0, miCultura);
                if (valor_invent2 != 0)
                    {
                    rotacion = (Costo_prom2 * 12) / valor_invent2;
                    }
                
                
             

                //Lealtad Bruta

                var datab = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 1).
             Select(s => s.valor_total).DefaultIfEmpty(0).Sum().ToString();
                var datatotal = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin).
             Select(s => s.valor_total).DefaultIfEmpty(0).Sum().ToString();


                decimal Lealtad_bruta = Convert.ToDecimal(0, miCultura);
                if (Convert.ToDecimal(datatotal, miCultura) != 0)
                    {
                    Lealtad_bruta = (Convert.ToDecimal(datab, miCultura) / Convert.ToDecimal(datatotal, miCultura))*100;
                    }
                   
                 
                // Lealtad  Neta

                var dataneto = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 1 && x.Clasificacion_pro == 2).
            Select(s => s.valor_total).DefaultIfEmpty(0).Sum().ToString();

                decimal Lealtad_neta = Convert.ToDecimal(0, miCultura);
                if (Convert.ToDecimal(datatotal, miCultura) != 0)
                    {
                    Lealtad_neta =( Convert.ToDecimal(dataneto, miCultura) / Convert.ToDecimal(datatotal, miCultura))*100;
                    }
                
                   //lealtad neta sin accesorios

                var datanetosin = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 1 && x.Clasificacion_pro == 2).
          Select(s => s.costorepuestos).DefaultIfEmpty(0).Sum().ToString();

                decimal lealtad_neta_sin_ac = 0;
                if (Convert.ToDecimal(datatotal, miCultura) != 0)
                    {
                    lealtad_neta_sin_ac = (Convert.ToDecimal(datanetosin, miCultura) / Convert.ToDecimal(datatotal, miCultura))*100;
                    }
                    
           

                //margen bruto sin incentivos
                decimal margbrsininc = Convert.ToDecimal(0, miCultura);

                //margen bruto con incentivos
                decimal margbrconince = Convert.ToDecimal(0, miCultura);

                //fof

                decimal valfof = Convert.ToDecimal(0, miCultura);

                //%obsolecia 
                decimal valonbsolencia = Convert.ToDecimal(0, miCultura);


                indicadores_generales indicadorgeneral = new indicadores_generales();

                indicadorgeneral.Año = Convert.ToInt16(ano);
                indicadorgeneral.mes = Convert.ToInt16(mespasado);

                indicadorgeneral.inventario_inicial = Datoinveniniins;
                indicadorgeneral.compras_gm_ingresis = comprasGM_ingresoins;
                indicadorgeneral.compras_concesionarios = compras_Concesionarioins;
                indicadorgeneral.compras_otros_repuestos = compras_otros_repins;
                indicadorgeneral.compras_otros_accesorios = compras_otros_acceins;
                indicadorgeneral.total_compras = compras_totalins;
                indicadorgeneral.total_ventas = total_ventasins;
                indicadorgeneral.ajustes_inventario = ajuste_inventarioins;
                indicadorgeneral.costo_venta = costo_ventains;
                indicadorgeneral.inventario_final = Inventa_finalins;
                indicadorgeneral.dif_inventario_enero = diferencia;
                indicadorgeneral.inv_proceso = inv_proceso;
                indicadorgeneral.inv_disponible = inv_disponible;
                indicadorgeneral.inv_final = valorinventpass;
                indicadorgeneral.variacion_inventario = variacion;
                indicadorgeneral.rotacion_inven = rotacion;
                indicadorgeneral.lealtad_bruta = Lealtad_bruta;
                indicadorgeneral.lealtad_neta_sin_acceso = lealtad_neta_sin_ac;
                indicadorgeneral.margen_bruto_sin_incent = margbrsininc;
                indicadorgeneral.margen_bruto_con_incent = margbrconince;
                indicadorgeneral.fof = valfof;
                indicadorgeneral.obsolencia = valonbsolencia;

                context.indicadores_generales.Add(indicadorgeneral);
                context.SaveChanges();
                Resultado = "Indicadores creados con exito " + fecha;
                }
            catch (Exception ex)
                {
                Resultado = fecha + "Error: " + ex.Message;         
                }

            return Json(Resultado, JsonRequestBehavior.AllowGet);
            }




        public JsonResult JsonIndicadorGenBodega() {

            var Resultado = "";
            var fecha = DateTime.Now;
            try
                {
                var ano = DateTime.Now.Year;
                var mespasado = DateTime.Now.Month - 1;
                var fechainicial =Convert.ToDateTime( DateTime.Now.Year + "-" + mespasado + "-01");
                var firstdayofmonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var ultimo_dia = firstdayofmonth.AddDays(-1);
                var fechafin =Convert.ToDateTime(ultimo_dia);
       

                List <bodega_concesionario> bodegas = context.bodega_concesionario.ToList();
                foreach (var item in bodegas)
                    {

           

                var  Datoinvenini = context.referencias_inven.Where(x => x.ano == ano && x.mes == mespasado && x.bodega == item.id ).Select(s => s.cos_ini).DefaultIfEmpty(0).Sum().ToString(); 

                //inventario incial
                decimal Datoinveniniins = Convert.ToDecimal(Datoinvenini, miCultura);

                var  comprasGM_ingreso = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro==1 && x.bodega== item.id).Select(s => s.valor_total).DefaultIfEmpty(0).Sum().ToString();
               
                //compras ingresos gm
                decimal comprasGM_ingresoins = Convert.ToDecimal(comprasGM_ingreso, miCultura);

                var compras_Concesionario = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 2 && x.bodega == item.id).Select(s => s.valor_total).DefaultIfEmpty(0).Sum().ToString();
              
                //compras concesionarios
                decimal compras_Concesionarioins = Convert.ToDecimal(compras_Concesionario, miCultura);

                var compras_otros_rep = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 3 && x.bodega == item.id).Select(s => s.costorepuestos).DefaultIfEmpty(0).Sum();
                
                // compras  otros repuestos
                decimal compras_otros_repins = compras_otros_rep!=null?Convert.ToDecimal(compras_otros_rep.Value, miCultura):0;


                var compras_otros_acce = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 3 && x.bodega == item.id).Select(s => s.costoaccesorios).DefaultIfEmpty(0).Sum();

                //compras otros accesorios
                decimal compras_otros_acceins = compras_otros_acce!=null?Convert.ToDecimal(compras_otros_acce.Value, miCultura):0;


                var compras_total = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.bodega == item.id).Select(s => s.valor_documento).DefaultIfEmpty(0).Sum().ToString();

                //total compras 
                decimal compras_totalins = Convert.ToDecimal(compras_total, miCultura);
                
                var  total_ventas = context.vw_Cantidades_Bodega.Where(x => x.fecha_creacion >= fechainicial && x.fecha_creacion <= fechafin && x.bodega == item.id).Select(s => s.valor_total).DefaultIfEmpty(0).Sum().ToString();

                //total ventas
                decimal total_ventasins = Convert.ToDecimal(total_ventas, miCultura);


                var ajuste_inventario = context.referencias_inven.Where(x => x.ano == ano && x.mes == mespasado && x.bodega == item.id).Select(s => new {  costo= s.cos_ent_aju+ s.cos_ini_aju, s.cos_sal_aju}).ToList();

                //ajuste inventario
                decimal ajuste_inventarioins = Convert.ToDecimal(ajuste_inventario.Sum(s => s.costo - s.cos_sal_aju), miCultura);
                                                
                var costo_venta = context.vw_Cantidades_Bodega.Where(x => x.fecha_creacion >= fechainicial && x.fecha_creacion <= fechafin && x.bodega == item.id).Select(s=> s.sumavalor).DefaultIfEmpty(0).Sum();
               
                //costo venta
                decimal costo_ventains = costo_venta!=null?Convert.ToDecimal(costo_venta.Value, miCultura):0;


                // inventario final
                decimal Inventa_finalins = (Datoinveniniins + compras_totalins + ajuste_inventarioins) - costo_ventains;

             

                var Datoinveninienero = context.referencias_inven.Where(x => x.ano == ano && x.mes == 1 && x.bodega == item.id).Select(s => s.cos_ini).DefaultIfEmpty(0).Sum().ToString();
              
                //diferencia respecto a enero
                decimal diferencia = Inventa_finalins - Convert.ToDecimal(Datoinveninienero, miCultura);

                //inventario en proceso 
                decimal inv_proceso = Convert.ToDecimal(0, miCultura);

                    //inv. disponible

                    decimal inv_disponible = Inventa_finalins - inv_proceso;

               
                var anopass = ano - 1;
                var  inv_anopass = context.indicadores_gen_Bodega.Where(x => x.Año == anopass && x.mes == mespasado && x.bodega == item.id).Select(x => x.inventario_final).DefaultIfEmpty(0).Sum();
                //inventario final Ano anterior
                decimal valorinventpass = inv_anopass!=null?Convert.ToDecimal(inv_anopass, miCultura):0;
                    decimal variacion = Convert.ToDecimal(0, miCultura);
                    if (valorinventpass!=0)
                        {
                        //variacion inventario 
                        variacion = (Inventa_finalins / valorinventpass) - 1;
                        }
               



                //Rotacion
                var Costo_prom = context.vw_costo_mes_bodega.Where(x => x.año == ano && x.mes == mespasado && x.bodega == item.id).
      Select(f => new { f.Costo_Total}).ToList();
                decimal Costo_prom2 = Convert.ToDecimal(Costo_prom.Sum(e => e.Costo_Total));
                var valor_invent = context.vw_valor_inventario_bodega.Where(d => d.ano == ano && d.mes == mespasado && d.bodega == item.id).
                   Sum(a =>  a.Total);
                decimal valor_invent2 = Convert.ToDecimal(valor_invent, miCultura);


                    decimal rotacion = Convert.ToDecimal(0, miCultura);
                    if (valor_invent2 != 0)
                        {
                        rotacion = (Costo_prom2 * 12) / valor_invent2;
                        }
                    
                  

                //Lealtad Bruta

                var datab = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 1 && x.bodega == item.id).
             Select(s => s.valor_total ).DefaultIfEmpty(0).Sum();
                var datatotal = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin).
             Sum(s => s.valor_total );


                    decimal Lealtad_bruta = Convert.ToDecimal(0, miCultura);
                    if (Convert.ToDecimal(datatotal, miCultura) != 0)
                        {
                        Lealtad_bruta = (Convert.ToDecimal(datab, miCultura) / Convert.ToDecimal(datatotal, miCultura))*100;
                        }
                   


                // Lealtad  Neta

                var dataneto = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin   && x.Clasificacion_pro == 1 && x.Clasificacion_pro == 2 && x.bodega == item.id).
            Select(s =>  s.valor_total ).DefaultIfEmpty(0).Sum();

                    decimal Lealtad_neta = Convert.ToDecimal(0, miCultura);
                    if (Convert.ToDecimal(datatotal, miCultura) != 0)
                        {
                        Lealtad_neta = (Convert.ToDecimal(dataneto, miCultura) / Convert.ToDecimal(datatotal, miCultura))*100;
                        }
                    
              


                //lealtad neta sin accesorios

                var datanetosin = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 1 && x.Clasificacion_pro == 2 && x.bodega == item.id).
          Sum(s => s.costorepuestos);

                    decimal lealtad_neta_sin_ac = Convert.ToDecimal(0, miCultura);
                    if (Convert.ToDecimal(datatotal, miCultura) != 0)
                        {
                        lealtad_neta_sin_ac =( Convert.ToDecimal(datanetosin, miCultura) / Convert.ToDecimal(datatotal, miCultura))*100;
                        }   
                       


                //margen bruto sin incentivos
                decimal margbrsininc = Convert.ToDecimal(0, miCultura);

                //margen bruto con incentivos
                decimal margbrconince = Convert.ToDecimal(0, miCultura);

                    //fof

                    decimal valfof = Convert.ToDecimal(0, miCultura);

                    //%obsolecia 
                    decimal valonbsolencia = Convert.ToDecimal(0, miCultura);


                    indicadores_gen_Bodega indicadorgeneral = new indicadores_gen_Bodega();

                indicadorgeneral.Año = Convert.ToInt16(ano);
                indicadorgeneral.mes = Convert.ToInt16(mespasado);
                indicadorgeneral.bodega = item.id;
                indicadorgeneral.inventario_inicial = Datoinveniniins;
                indicadorgeneral.compras_gm_ingresis = comprasGM_ingresoins;
                indicadorgeneral.compras_concesionarios = compras_Concesionarioins;
                indicadorgeneral.compras_otros_repuestos = compras_otros_repins;
                indicadorgeneral.compras_otros_accesorios =compras_otros_acceins;
                indicadorgeneral.total_compras = compras_totalins;
                indicadorgeneral.total_ventas = total_ventasins;
                indicadorgeneral.ajustes_inventario = ajuste_inventarioins;
                indicadorgeneral.costo_venta = costo_ventains;
                indicadorgeneral.inventario_final = Inventa_finalins;
                indicadorgeneral.dif_inventario_enero = diferencia;
                indicadorgeneral.inv_proceso = inv_proceso;
                indicadorgeneral.inv_disponible = inv_disponible;
                indicadorgeneral.inv_final = valorinventpass;
                indicadorgeneral.variacion_inventario = variacion;
                indicadorgeneral.rotacion_inven = rotacion;
                indicadorgeneral.lealtad_bruta = Lealtad_bruta;
                indicadorgeneral.lealtad_neta = Lealtad_neta;
                indicadorgeneral.lealtad_neta_sin_acceso = lealtad_neta_sin_ac;
                indicadorgeneral.margen_bruto_sin_incent = margbrsininc;
                indicadorgeneral.margen_bruto_con_incent = margbrconince;
                indicadorgeneral.fof =valfof;
                indicadorgeneral.obsolencia = valonbsolencia;



                    context.indicadores_gen_Bodega.Add(indicadorgeneral);
                context.SaveChanges();
                Resultado = Resultado+" Indicadores de bodega " + item.id + " creados con exito "+fecha;


                    }
                }
            catch (Exception ex)
                {
                
                Resultado = fecha + "Error: " + ex.Message;

                }

            return Json(Resultado, JsonRequestBehavior.AllowGet);

            }



        public JsonResult jsonIndicadoresGeneralesmeses()
            {
            var Resultado = "";

            for (int i = 2; i < 5; i++)
                {

                int j = 0;

     
            var fecha = DateTime.Now;
            try
                {
                var ano = DateTime.Now.Year;
                var mespasado = DateTime.Now.Month - i;
                var fechainicial = Convert.ToDateTime(DateTime.Now.Year + "-" + mespasado + "-01");
                    j = i - 1;
                      var mes_ = DateTime.Now.Month - j;
                    var firstdayofmonth = new DateTime(DateTime.Now.Year, mes_, 1);
                var ultimo_dia = firstdayofmonth.AddDays(-1);
                var fechafin = Convert.ToDateTime(ultimo_dia);


                //var Datoinvenini = context.referencias_inven.Where(x => x.ano == ano && x.mes == mespasado).Sum(s => s.cos_ini).ToString();
                var Datoinvenini = context.referencias_inven.Where(x => x.ano == ano && x.mes == mespasado).Sum(s => s.cos_ini);
                //inventario incial
                decimal Datoinveniniins = Convert.ToDecimal(Datoinvenini, miCultura);

                //var comprasGM_ingreso = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 1).Select(s => s.valor_total).DefaultIfEmpty(0).Sum().ToString();
                var comprasGM_ingreso = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 1).Select(s => s.valor_documento).DefaultIfEmpty(0).Sum();

                //compras ingresos gm
                decimal comprasGM_ingresoins = comprasGM_ingreso!=null? Convert.ToDecimal(comprasGM_ingreso.Value, miCultura):0;

                //var compras_Concesionario = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 2).Select(s => s.valor_total).DefaultIfEmpty(0).Sum().ToString();
                var compras_Concesionario = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 2).Select(s => s.valor_documento).DefaultIfEmpty(0).Sum();
                //compras concesionarios
                decimal compras_Concesionarioins = compras_Concesionario!=null?Convert.ToDecimal(compras_Concesionario.Value, miCultura):0;

                var compras_otros_rep = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 3).Select(s => s.costorepuestos).DefaultIfEmpty(0).Sum();

                // compras  otros repuestos
                decimal compras_otros_repins = compras_otros_rep!=null?Convert.ToDecimal(compras_otros_rep, miCultura):0;


                //var compras_otros_acce = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 3).Select(s => s.costoaccesorios).DefaultIfEmpty(0).Sum().ToString();
                var compras_otros_acce = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 3).Select(s => s.costoaccesorios).DefaultIfEmpty(0).Sum();
                //compras otros accesorios
                decimal compras_otros_acceins = compras_otros_acce!=null?Convert.ToDecimal(compras_otros_acce.Value, miCultura):0;


                //var compras_total = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin).Select(s => s.valor_documento).DefaultIfEmpty(0).Sum().ToString();
                var compras_total = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin).Select(s => s.valor_documento).DefaultIfEmpty(0).Sum();
                //total compras 
                decimal compras_totalins = compras_total!=null?Convert.ToDecimal(compras_total.Value, miCultura):0;

                //var total_ventas = context.vw_Cantidades_Bodega.Where(x => x.fecha_creacion >= fechainicial && x.fecha_creacion <= fechafin).Select(s => s.valor_total).DefaultIfEmpty(0).Sum().ToString();
                var total_ventas = context.vw_Cantidades_Bodega.Where(x => x.fecha_creacion >= fechainicial && x.fecha_creacion <= fechafin).Select(s => s.valor_total).DefaultIfEmpty(0).Sum();

                //total ventas
                decimal total_ventasins = Convert.ToDecimal(total_ventas, miCultura);


                var ajuste_inventario = context.referencias_inven.Where(x => x.ano == ano && x.mes == mespasado).Select(s => new { costo = s.cos_ent_aju + s.cos_ini_aju, s.cos_sal_aju }).ToList();

                //ajuste inventario
                decimal ajuste_inventarioins = Convert.ToDecimal(ajuste_inventario.Sum(s => s.costo - s.cos_sal_aju), miCultura);

                //var costo_venta = context.vw_Cantidades_Bodega.Where(x => x.fecha_creacion >= fechainicial && x.fecha_creacion <= fechafin).Select(s => s.sumavalor).DefaultIfEmpty(0).Sum().ToString();
                var costo_venta = context.vw_Cantidades_Bodega.Where(x => x.fecha_creacion >= fechainicial && x.fecha_creacion <= fechafin).Select(s => s.sumavalor).DefaultIfEmpty(0).Sum();
                //costo venta
                decimal costo_ventains = costo_venta!=null?Convert.ToDecimal(costo_venta.Value, miCultura):0;


                // inventario final
                decimal Inventa_finalins = (Datoinveniniins + compras_totalins + ajuste_inventarioins) - costo_ventains;



                //var Datoinveninienero = context.referencias_inven.Where(x => x.ano == ano && x.mes == 1).Select(s => s.cos_ini).DefaultIfEmpty(0).Sum().ToString();
                var Datoinveninienero = context.referencias_inven.Where(x => x.ano == ano && x.mes == 1).Select(s => s.cos_ini).DefaultIfEmpty(0).Sum();
                //diferencia respecto a enero
                decimal diferencia = Inventa_finalins - Convert.ToDecimal(Datoinveninienero, miCultura);

                //inventario en proceso 
                decimal inv_proceso = Convert.ToDecimal(0, miCultura);

                //inv. disponible

                decimal inv_disponible = Inventa_finalins - inv_proceso;


                var anopass = ano - 1;
                var inv_anopass = context.indicadores_generales.Where(x => x.Año == anopass && x.mes == mespasado).Select(x => x.inventario_final).DefaultIfEmpty(0).Sum();
                //inventario final Ano anterior
                decimal valorinventpass =inv_anopass!=null? Convert.ToDecimal(inv_anopass.Value, miCultura):0;
                decimal variacion = Convert.ToDecimal(0, miCultura);
                if (valorinventpass == 0)
                    {
                    variacion = 0;
                    }
                else
                    {
                    variacion = (Inventa_finalins / valorinventpass) - 1;
                    }
                //variacion inventario 




                //Rotacion
                var Costo_prom = context.vw_costo_mes_bodega.Where(x => x.año == ano && x.mes == mespasado).
      Select(f => new { f.Costo_Total }).ToList();
                decimal Costo_prom2 = Convert.ToDecimal(Costo_prom.Sum(e => e.Costo_Total));
                var valor_invent = context.vw_valor_inventario_bodega.Where(d => d.ano == ano && d.mes == mespasado).
                   Sum(a => a.Total);
                decimal valor_invent2 = Convert.ToDecimal(valor_invent, miCultura);


                decimal rotacion = Convert.ToDecimal(0, miCultura);
                if (valor_invent2 != 0)
                    {
                    rotacion = (Costo_prom2 * 12) / valor_invent2;
                    }




                //Lealtad Bruta

                var datab = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 1).
             Select(s => s.valor_total).DefaultIfEmpty(0).Sum().ToString();
                var datatotal = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin).
             Select(s => s.valor_total).DefaultIfEmpty(0).Sum().ToString();


                decimal Lealtad_bruta = Convert.ToDecimal(0, miCultura);
                if (Convert.ToDecimal(datatotal, miCultura) != 0)
                    {
                    Lealtad_bruta = (Convert.ToDecimal(datab, miCultura) / Convert.ToDecimal(datatotal, miCultura)) * 100;
                    }


                // Lealtad  Neta

                var dataneto = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 1 && x.Clasificacion_pro == 2).
            Select(s => s.valor_total).DefaultIfEmpty(0).Sum().ToString();

                decimal Lealtad_neta = Convert.ToDecimal(0, miCultura);
                if (Convert.ToDecimal(datatotal, miCultura) != 0)
                    {
                    Lealtad_neta = (Convert.ToDecimal(dataneto, miCultura) / Convert.ToDecimal(datatotal, miCultura)) * 100;
                    }

                //lealtad neta sin accesorios

                var datanetosin = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 1 && x.Clasificacion_pro == 2).
          Select(s => s.costorepuestos).DefaultIfEmpty(0).Sum().ToString();

                decimal lealtad_neta_sin_ac = 0;
                if (Convert.ToDecimal(datatotal, miCultura) != 0)
                    {
                    lealtad_neta_sin_ac = (Convert.ToDecimal(datanetosin, miCultura) / Convert.ToDecimal(datatotal, miCultura)) * 100;
                    }



                //margen bruto sin incentivos
                decimal margbrsininc = Convert.ToDecimal(0, miCultura);

                //margen bruto con incentivos
                decimal margbrconince = Convert.ToDecimal(0, miCultura);

                //fof

                decimal valfof = Convert.ToDecimal(0, miCultura);

                //%obsolecia 
                decimal valonbsolencia = Convert.ToDecimal(0, miCultura);


                indicadores_generales indicadorgeneral = new indicadores_generales();

                indicadorgeneral.Año = Convert.ToInt16(ano);
                indicadorgeneral.mes = Convert.ToInt16(mespasado);
                indicadorgeneral.inventario_inicial = Datoinveniniins;
                indicadorgeneral.compras_gm_ingresis = comprasGM_ingresoins;
                indicadorgeneral.compras_concesionarios = compras_Concesionarioins;
                indicadorgeneral.compras_otros_repuestos = compras_otros_repins;
                indicadorgeneral.compras_otros_accesorios = compras_otros_acceins;
                indicadorgeneral.total_compras = compras_totalins;
                indicadorgeneral.total_ventas = total_ventasins;
                indicadorgeneral.ajustes_inventario = ajuste_inventarioins;
                indicadorgeneral.costo_venta = costo_ventains;
                indicadorgeneral.inventario_final = Inventa_finalins;
                indicadorgeneral.dif_inventario_enero = diferencia;
                indicadorgeneral.inv_proceso = inv_proceso;
                indicadorgeneral.inv_disponible = inv_disponible;
                indicadorgeneral.inv_final = valorinventpass;
                indicadorgeneral.variacion_inventario = variacion;
                indicadorgeneral.rotacion_inven = rotacion;
                indicadorgeneral.lealtad_bruta = Lealtad_bruta;
                indicadorgeneral.lealtad_neta = Lealtad_neta;
                indicadorgeneral.lealtad_neta_sin_acceso = lealtad_neta_sin_ac;
                indicadorgeneral.margen_bruto_sin_incent = margbrsininc;
                indicadorgeneral.margen_bruto_con_incent = margbrconince;
                indicadorgeneral.fof = valfof;
                indicadorgeneral.obsolencia = valonbsolencia;

                context.indicadores_generales.Add(indicadorgeneral);
                context.SaveChanges();
                Resultado = "Indicadores creados con exito " + fecha;
                }
            catch (Exception ex)
                {

                Resultado = fecha + "Error: " + ex.Message;


                }

                }

            return Json(Resultado, JsonRequestBehavior.AllowGet);




            }


        public JsonResult JsonIndicadorGenBodegameses()
            {

            var Resultado = "";
            var fecha = DateTime.Now;

            for (int i = 2; i < 5; i++)
                {

                int j = 0;

            try
                {
                    var ano = DateTime.Now.Year;
                    var mespasado = DateTime.Now.Month - i;
                    var fechainicial = Convert.ToDateTime(DateTime.Now.Year + "-" + mespasado + "-01");
                    j = i - 1;
                    var mes_ = DateTime.Now.Month - j;
                    var firstdayofmonth = new DateTime(DateTime.Now.Year, mes_, 1);
                    var ultimo_dia = firstdayofmonth.AddDays(-1);
                    var fechafin = Convert.ToDateTime(ultimo_dia);


                    List<bodega_concesionario> bodegas = context.bodega_concesionario.ToList();
                foreach (var item in bodegas)
                    {



                    var Datoinvenini = context.referencias_inven.Where(x => x.ano == ano && x.mes == mespasado && x.bodega == item.id).Select(s => s.cos_ini).DefaultIfEmpty(0).Sum();

                    //inventario incial
                    decimal Datoinveniniins = Convert.ToDecimal(Datoinvenini, miCultura);

                    var comprasGM_ingreso = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 1 && x.bodega == item.id).Select(s => s.valor_total).DefaultIfEmpty(0).Sum();

                    //compras ingresos gm
                    decimal comprasGM_ingresoins = Convert.ToDecimal(comprasGM_ingreso, miCultura);

                    var compras_Concesionario = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 2 && x.bodega == item.id).Select(s => s.valor_total).DefaultIfEmpty(0).Sum();

                    //compras concesionarios
                    decimal compras_Concesionarioins = Convert.ToDecimal(compras_Concesionario, miCultura);

                    var compras_otros_rep = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 3 && x.bodega == item.id).Select(s => s.costorepuestos).DefaultIfEmpty(0).Sum();

                    // compras  otros repuestos
                    decimal compras_otros_repins =compras_otros_rep!=null? Convert.ToDecimal(compras_otros_rep.Value, miCultura):0;


                    var compras_otros_acce = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 3 && x.bodega == item.id).Select(s => s.costoaccesorios).DefaultIfEmpty(0).Sum();

                    //compras otros accesorios
                    decimal compras_otros_acceins =compras_otros_acce!=null? Convert.ToDecimal(compras_otros_acce.Value, miCultura):0;


                    var compras_total = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.bodega == item.id).Select(s => s.valor_documento).DefaultIfEmpty(0).Sum();

                    //total compras 
                    decimal compras_totalins =compras_total!=null? Convert.ToDecimal(compras_total.Value, miCultura):0;

                    var total_ventas = context.vw_Cantidades_Bodega.Where(x => x.fecha_creacion >= fechainicial && x.fecha_creacion <= fechafin && x.bodega == item.id).Select(s => s.valor_total).DefaultIfEmpty(0).Sum();

                    //total ventas
                    decimal total_ventasins = Convert.ToDecimal(total_ventas, miCultura);


                    var ajuste_inventario = context.referencias_inven.Where(x => x.ano == ano && x.mes == mespasado && x.bodega == item.id).Select(s => new { costo = s.cos_ent_aju + s.cos_ini_aju, s.cos_sal_aju }).ToList();

                    //ajuste inventario
                    decimal ajuste_inventarioins = Convert.ToDecimal(ajuste_inventario.Sum(s => s.costo - s.cos_sal_aju), miCultura);

                    var costo_venta = context.vw_Cantidades_Bodega.Where(x => x.fecha_creacion >= fechainicial && x.fecha_creacion <= fechafin && x.bodega == item.id).Select(s => s.sumavalor).DefaultIfEmpty(0).Sum();

                    //costo venta
                    decimal costo_ventains = costo_venta!=null?Convert.ToDecimal(costo_venta.Value, miCultura):0;


                    // inventario final
                    decimal Inventa_finalins = (Datoinveniniins + compras_totalins + ajuste_inventarioins) - costo_ventains;



                    var Datoinveninienero = context.referencias_inven.Where(x => x.ano == ano && x.mes == 1 && x.bodega == item.id).Select(s => s.cos_ini).DefaultIfEmpty(0).Sum();

                    //diferencia respecto a enero
                    decimal diferencia = Inventa_finalins - Convert.ToDecimal(Datoinveninienero, miCultura);

                    //inventario en proceso 
                    decimal inv_proceso = Convert.ToDecimal(0, miCultura);

                    //inv. disponible

                    decimal inv_disponible = Inventa_finalins - inv_proceso;


                    var anopass = ano - 1;
                    var inv_anopass = context.indicadores_gen_Bodega.Where(x => x.Año == anopass && x.mes == mespasado && x.bodega == item.id).Select(x => x.inventario_final).DefaultIfEmpty(0).Sum();
                    //inventario final Ano anterior
                    decimal valorinventpass = inv_anopass!=null?Convert.ToDecimal(inv_anopass.Value, miCultura):0;
                    decimal variacion = Convert.ToDecimal(0, miCultura);
                    if (valorinventpass != 0)
                        {
                        //variacion inventario 
                        variacion = (Inventa_finalins / valorinventpass) - 1;
                        }




                    //Rotacion
                    var Costo_prom = context.vw_costo_mes_bodega.Where(x => x.año == ano && x.mes == mespasado && x.bodega == item.id).
          Select(f => new { f.Costo_Total }).ToList();
                    decimal Costo_prom2 = Convert.ToDecimal(Costo_prom.Sum(e => e.Costo_Total));
                    var valor_invent = context.vw_valor_inventario_bodega.Where(d => d.ano == ano && d.mes == mespasado && d.bodega == item.id).
                       Sum(a => a.Total);
                    decimal valor_invent2 = Convert.ToDecimal(valor_invent, miCultura);


                    decimal rotacion = Convert.ToDecimal(0, miCultura);
                    if (valor_invent2 != 0)
                        {
                        rotacion = (Costo_prom2 * 12) / valor_invent2;
                        }



                    //Lealtad Bruta

                    var datab = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 1 && x.bodega == item.id).
                 Select(s => s.valor_total).DefaultIfEmpty(0).Sum();
                    var datatotal = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin).
                 Select(s => s.valor_total).DefaultIfEmpty(0).Sum();


                        decimal Lealtad_bruta = Convert.ToDecimal(0, miCultura);
                    if (Convert.ToDecimal(datatotal, miCultura) != 0)
                        {
                        Lealtad_bruta = (Convert.ToDecimal(datab, miCultura) / Convert.ToDecimal(datatotal, miCultura)) * 100;
                        }



                    // Lealtad  Neta

                    var dataneto = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 1 && x.Clasificacion_pro == 2 && x.bodega == item.id).
                Select(s => s.valor_total).DefaultIfEmpty(0).Sum();

                    decimal Lealtad_neta = Convert.ToDecimal(0, miCultura);
                    if (Convert.ToDecimal(datatotal, miCultura) != 0)
                        {
                        Lealtad_neta = (Convert.ToDecimal(dataneto, miCultura) / Convert.ToDecimal(datatotal, miCultura)) * 100;
                        }




                    //lealtad neta sin accesorios

                    var datanetosin = context.vw_Datos_lealtad.Where(x => x.fec_creacion >= fechainicial && x.fec_creacion <= fechafin && x.Clasificacion_pro == 1 && x.Clasificacion_pro == 2 && x.bodega == item.id).
              Sum(s => s.costorepuestos);

                    decimal lealtad_neta_sin_ac = Convert.ToDecimal(0, miCultura);
                    if (Convert.ToDecimal(datatotal, miCultura) != 0)
                        {
                        lealtad_neta_sin_ac = (Convert.ToDecimal(datanetosin, miCultura) / Convert.ToDecimal(datatotal, miCultura)) * 100;
                        }



                    //margen bruto sin incentivos
                    decimal margbrsininc = Convert.ToDecimal(0, miCultura);

                    //margen bruto con incentivos
                    decimal margbrconince = Convert.ToDecimal(0, miCultura);

                    //fof

                    decimal valfof = Convert.ToDecimal(0, miCultura);

                    //%obsolecia 
                    decimal valonbsolencia = Convert.ToDecimal(0, miCultura);


                    indicadores_gen_Bodega indicadorgeneral = new indicadores_gen_Bodega();

                    indicadorgeneral.Año = Convert.ToInt16(ano);
                    indicadorgeneral.mes = Convert.ToInt16(mespasado);
                    indicadorgeneral.bodega = item.id;
                    indicadorgeneral.inventario_inicial = Datoinveniniins ;
                    indicadorgeneral.compras_gm_ingresis = comprasGM_ingresoins;
                    indicadorgeneral.compras_concesionarios = compras_Concesionarioins;
                    indicadorgeneral.compras_otros_repuestos = compras_otros_repins;
                    indicadorgeneral.compras_otros_accesorios = compras_otros_acceins;
                    indicadorgeneral.total_compras = compras_totalins;
                    indicadorgeneral.total_ventas = total_ventasins;
                    indicadorgeneral.ajustes_inventario = ajuste_inventarioins;
                    indicadorgeneral.costo_venta = costo_ventains;
                    indicadorgeneral.inventario_final = Inventa_finalins;
                    indicadorgeneral.dif_inventario_enero = diferencia;
                    indicadorgeneral.inv_proceso = inv_proceso;
                    indicadorgeneral.inv_disponible = inv_disponible;
                    indicadorgeneral.inv_final = valorinventpass;
                    indicadorgeneral.variacion_inventario = variacion;
                    indicadorgeneral.rotacion_inven = rotacion;
                    indicadorgeneral.lealtad_bruta = Lealtad_bruta;
                    indicadorgeneral.lealtad_neta = Lealtad_neta;
                    indicadorgeneral.lealtad_neta_sin_acceso = lealtad_neta_sin_ac;
                    indicadorgeneral.margen_bruto_sin_incent = margbrsininc;
                    indicadorgeneral.margen_bruto_con_incent = margbrconince;
                    indicadorgeneral.fof = valfof;
                    indicadorgeneral.obsolencia = valonbsolencia;

                    context.indicadores_gen_Bodega.Add(indicadorgeneral);
                    context.SaveChanges();
                    Resultado = Resultado + " Indicadores de bodega " + item.id + " creados con exito " + fecha;


                    }
                }
            catch (Exception ex)
                {

                Resultado = fecha + "Error: " + ex.Message;

                }


                }



            return Json(Resultado, JsonRequestBehavior.AllowGet);

            }



        }
    }