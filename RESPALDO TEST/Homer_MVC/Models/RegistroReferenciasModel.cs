using System;
using System.ComponentModel.DataAnnotations;

namespace Homer_MVC.Models
{
    public class RegistroReferenciasModel
    {
        public string ref_codigo { get; set; }
        public string ref_descripcion { get; set; }

        [Display(Name = " Descripción alternativa: ")]
        public string ref_alternativa { get; set; }

        public int ref_stock { get; set; }
        public bool ref_estado { get; set; }
        public int ref_usuario_creacion { get; set; }
        public DateTime ref_fecha_creacion { get; set; }
        public int? ref_usuario_actualizacion { get; set; }
        public DateTime? ref_fecha_actualizacion { get; set; }
        public string ref_valor_unitario { get; set; }
        public string grupo_id { get; set; }
        public int? clasificacion_id { get; set; }
        public int? ref_licencia { get; set; }
        public string ref_razon_inactivo { get; set; }
        public int? ref_cantidad_min { get; set; }
        public int? ref_cantidad_max { get; set; }
        public decimal ref_valor_total { get; set; }
        public string subgrupo { get; set; }
        public int? perfil { get; set; }
        public float? por_iva { get; set; }
        public float? por_iva_compra { get; set; }
        public string costo_unitario { get; set; }
        public bool manejo_inv { get; set; }
        public string unidad_medida { get; set; }
        public byte? unidad_venta { get; set; }
        public byte? unidad_compra { get; set; }
        public float? impconsumo { get; set; }
        public decimal costo_anterior { get; set; }
        public DateTime? fec_ultima_entrada { get; set; }
        public DateTime? fec_ultima_salida { get; set; }
        public float? imp_consumo_compra { get; set; }
        public float? por_consumo_compra { get; set; }
        public float? max_desc { get; set; }
        public string costo_emergencia { get; set; }
        public string clasificacion_ABC { get; set; }
        public string texto { get; set; }
        public int? proveedor_ppal { get; set; }
        public float? por_dscto { get; set; }
        public float? por_dscto_max { get; set; }
        public decimal precio_venta { get; set; }
        public string precio_alterno { get; set; }
        public string precio_garantia { get; set; }
        public string precio_diesel { get; set; }
        public int? vida_util { get; set; }
        public int? factor { get; set; }
        public int? pedidopor { get; set; }
        public string linea_id { get; set; }
        public int? tipo_id { get; set; }
        public string familia_id { get; set; }
        public string tipor { get; set; }
        public string modulo { get; set; }
        public int? idporivaventa { get; set; }
        public int? idporivacompra { get; set; }
    }
}