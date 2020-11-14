using System;

namespace Homer_MVC.ModeloVehiculos
{
    public class CrearVehiculoExcel
    {
        public string modvh_id { get; set; }
        public string modvh_descripcion { get; set; }
        public string colvh_id { get; set; }
        public string vin { get; set; }
        public string kmat { get; set; }
        public string nummot_vh { get; set; }
        public int anio_vh { get; set; }
        public DateTime fecfact_fabrica { get; set; }
        public string plan_mayor { get; set; }
        public int? numinv_vh { get; set; }
        public long numfactura_vh { get; set; }
        public string numManifiesto { get; set; }
        public DateTime fechaManifiesto { get; set; }
        public decimal costosiniva_vh { get; set; }
        public long numpedido_vh { get; set; }
        public decimal costototal_vh { get; set; }
        public decimal iva_vh { get; set; }
        public decimal porcentajeIva { get; set; }
        public decimal porcentajeReteIca { get; set; }
        public decimal porcentajeRetencion { get; set; }
        public decimal porcentajeReteIva { get; set; }
        public decimal impuesto_consumo { get; set; }
        public string bodegaGM { get; set; }
        public string codigoPago { get; set; }
        public string notas { get; set; }
        public bool flota { get; set; }
        public bool vehiculo_actualizar { get; set; }
        public decimal porcentaje_iva { get; set; }
    }
}