//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Homer_MVC.IcebergModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class vw_movimiento_clientes
    {
        public int tercero_id { get; set; }
        public string doc_tercero { get; set; }
        public string cliente { get; set; }
        public System.DateTime fecha { get; set; }
        public int bodega_id { get; set; }
        public string bodega { get; set; }
        public Nullable<int> tipo { get; set; }
        public string prefijo { get; set; }
        public Nullable<int> sw { get; set; }
        public int idencabezado { get; set; }
        public Nullable<int> id { get; set; }
        public int id_tipo_doc { get; set; }
        public long nro_documento { get; set; }
        public decimal valor_total { get; set; }
        public decimal valor_aplicado { get; set; }
        public decimal debito { get; set; }
        public decimal credito { get; set; }
        public decimal valor_factura { get; set; }
        public int cruzado { get; set; }
        public string referencia { get; set; }
        public string placa { get; set; }
        public Nullable<int> id_forma_pago { get; set; }
        public string forma_pago { get; set; }
        public string observacion { get; set; }
        public Nullable<int> id_tipo_cartera { get; set; }
        public string tipo_cartera { get; set; }
        public Nullable<bool> id_tipo_recibo { get; set; }
        public string tipo_recibo { get; set; }
        public Nullable<int> orden { get; set; }
        public string mediopago { get; set; }
    }
}
