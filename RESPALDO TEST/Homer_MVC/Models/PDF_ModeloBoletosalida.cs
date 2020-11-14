using System.Collections.Generic;

namespace Homer_MVC.Models
{
    public class PDF_ModeloBoletosalida
    {
        //laura
        // Empresa
        public string nombreEmpre { get; set; }
        public string nitEmpre { get; set; }
        public string dirEmpre { get; set; }



        // 
        public string titulo { get; set; }
        public string fecha { get; set; }
        public string factura { get; set; }
        public string cliente { get; set; }
        public string asesor { get; set; }

        // Cargos
        public string vehiculo_frand { get; set; }
        public string vehiculo_valor { get; set; }
        //Accesorios 
        public List<AccesoriosBoleto> accesoriosboleto { get; set; }
        public string matricula_frand { get; set; }
        public string matricula_valor { get; set; }
        public string soat_frand { get; set; }
        public string soat_valor { get; set; }
        public string poliza_frand { get; set; }
        public string poliza_valor { get; set; }
        public string veh_modelo { get; set; }
        public string placa { get; set; }
        public string serVeh { get; set; }
        public int? numFactPed { get; set; }
        // tramites Usados
        public List<TramitesUsadosBoleto> tramitesusadosboleto { get; set; }
        public string obligacion_frand { get; set; }
        public string obligacion_valor { get; set; }
        // total cargos
        public string total_cargos { get; set; }

        // Abonos
        public string cuota_inicial_frand { get; set; }
        public string cuota_inicial_valor { get; set; }
        // tramites Usados
        public List<AbonosBoleto> abonosboleto { get; set; }
        public string financiera { get; set; }
        public string finanaciera_valor { get; set; }
        public string retoma { get; set; }
        public string retoma_valor { get; set; }
        // Bonos Descuentos 
        public List<BonosDescuentos> bonosdescuentos { get; set; }
        public string total_abonos { get; set; }

        public string saldo_pendiente { get; set; }

        public string saldo_devolucion { get; set; }

    }
    public class AccesoriosBoleto
    {
        public int? id_accesorio_modelo { get; set; }
        public string descripcion_accesorio { get; set; }
        public string cant_acccesorio { get; set; }
        public string val_unitario_accesorio { get; set; }
        public string val_monto_accesorio { get; set; }
        public decimal val_monto_accesoriodeci { get; set; }
    }
    public class TramitesUsadosBoleto
    {
        public string descripcion_tramite { get; set; }
        public string monto_tramite { get; set; }
    }
    public class AbonosBoleto
    {
        public string descripcion_abono { get; set; }
        public string monto_abono { get; set; }
        public decimal monto_abonodeci { get; set; }
    }
    public class BonosDescuentos
    {
        public string descripcion_descuento { get; set; }
        public string monto_descuento { get; set; }
    }


}