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
    
    public partial class icb_bahia_alistamiento
    {
        public int bh_als_id { get; set; }
        public Nullable<int> bahia_id { get; set; }
        public Nullable<int> ot_id { get; set; }
        public Nullable<int> id_pedido { get; set; }
        public Nullable<short> tp_movimiento { get; set; }
        public Nullable<System.DateTime> bh_als_fecha { get; set; }
        public Nullable<int> bh_als_usuela { get; set; }
        public Nullable<System.DateTime> bh_als_fecela { get; set; }
        public Nullable<int> bh_als_usumod { get; set; }
        public Nullable<System.DateTime> bh_als_fecmod { get; set; }
    
        public virtual tbahias tbahias { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
        public virtual vpedido vpedido { get; set; }
        public virtual tencabezaorden tencabezaorden { get; set; }
    }
}