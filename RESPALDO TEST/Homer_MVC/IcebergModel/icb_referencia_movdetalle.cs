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
    
    public partial class icb_referencia_movdetalle
    {
        public int refdet_id { get; set; }
        public int refmov_id { get; set; }
        public string ref_codigo { get; set; }
        public int seq { get; set; }
        public decimal refdet_cantidad { get; set; }
        public Nullable<decimal> refdet_saldo { get; set; }
        public Nullable<int> concepto { get; set; }
        public decimal valor_unitario { get; set; }
        public string notas { get; set; }
        public Nullable<int> nit_destino { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<decimal> fletes { get; set; }
        public Nullable<decimal> iva_fletes { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public Nullable<float> poriva { get; set; }
        public Nullable<float> pordscto { get; set; }
        public Nullable<decimal> valor_total { get; set; }
        public int cantidad_recibida { get; set; }
        public bool tiene_stock { get; set; }
        public bool pedido { get; set; }
        public bool traslado { get; set; }
        public Nullable<int> bodega_traslado { get; set; }
        public bool facturado { get; set; }
        public bool solicitado { get; set; }
        public Nullable<int> tipotarifa { get; set; }
        public Nullable<bool> respuestaInterna { get; set; }
        public string observacionresinterva { get; set; }
        public Nullable<int> idcentro { get; set; }
        public Nullable<int> desdepedido { get; set; }
    
        public virtual centro_costo centro_costo { get; set; }
        public virtual icb_referencia icb_referencia { get; set; }
        public virtual icb_referencia_mov icb_referencia_mov { get; set; }
        public virtual rtipocliente rtipocliente { get; set; }
    }
}
