using System;

/////////
namespace Homer_MVC.IcebergModel
{
    public class ReferenciasModel
    {
        public string ref_codigo { get; set; }
        public int? ref_stock { get; set; }
        public decimal? ref_valor_unitario { get; set; }
        public int? grupo_id { get; set; }
        public int? clasificacion_id { get; set; }
        public int? proveedor { get; set; }
        public int? refdet_cantidad { get; set; }
        public int? refmov_id { get; set; }
        public int? refmov_numero { get; set; }
        public int syspar_value { get; set; }
        public int? bodmov_id { get; set; }
        public int? tpmovimiento { get; set; }
        public int? consecutivo { get; set; }
        public string codigo_mov { get; set; }
        public int? cliente { get; set; }
        public int? vendedor { get; set; }
        public int condicion { get; set; }
        public short? dias_validez { get; set; }
        public decimal? valor_total { get; set; }
        public string notas { get; set; }
        public short? concepto { get; set; }
        public int? nit_destino { get; set; }
        public decimal? fletes { get; set; }
        public decimal? iva_fletes { get; set; }
        public int tpdocid { get; set; }
        public int bodega_id { get; set; }
        public int refmov_numpedido { get; set; }
        public int seq { get; set; }
        public int refdet_id { get; set; }
        public decimal? refdet_saldo { get; set; }
        public decimal? valor_unitario { get; set; }
        public int? id_licencia { get; set; }
        public DateTime? fec_creacion { get; set; }
        public DateTime? fec_actualizacion { get; set; }
        public int? user_idactualizacion { get; set; }
        public int? user_idcreacion { get; set; }
        public bool? estado { get; set; }
        public string razon_inactivo { get; set; }
        public float? poriva { get; set; }
        public float? pordscto { get; set; }
        public decimal? valor_total_Detalle { get; set; }
        public string descuento_pie { get; set; }
        public string valorIVA { get; set; }
        public string valorDescuento { get; set; }
    }
}