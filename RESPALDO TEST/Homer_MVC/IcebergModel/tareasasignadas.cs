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
    
    public partial class tareasasignadas
    {
        public int id { get; set; }
        public Nullable<int> idcotizacion { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public int idusuarioasignado { get; set; }
        public string notas { get; set; }
        public Nullable<int> idtipificacion { get; set; }
        public Nullable<int> idusuariotarea { get; set; }
        public bool esactivacion { get; set; }
        public string observaciones { get; set; }
        public Nullable<int> idtercero { get; set; }
        public Nullable<int> ordenTaller { get; set; }
    
        public virtual icb_cotizacion icb_cotizacion { get; set; }
        public virtual icb_terceros icb_terceros { get; set; }
        public virtual tencabezaorden tencabezaorden { get; set; }
        public virtual tipificaciontercero tipificaciontercero { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
        public virtual users users2 { get; set; }
        public virtual users users3 { get; set; }
    }
}
