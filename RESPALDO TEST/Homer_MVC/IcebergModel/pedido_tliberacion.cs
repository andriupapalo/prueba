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
    
    public partial class pedido_tliberacion
    {
        public int id { get; set; }
        public int tpvehiculo { get; set; }
        public int dias_para_liberar { get; set; }
        public Nullable<int> user_creacion { get; set; }
        public Nullable<System.DateTime> fec_creacion { get; set; }
        public Nullable<int> user_actualizacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> licencia_id { get; set; }
        public Nullable<int> bodega_id { get; set; }
    
        public virtual bodega_concesionario bodega_concesionario { get; set; }
        public virtual tp_vehiculo tp_vehiculo { get; set; }
    }
}
