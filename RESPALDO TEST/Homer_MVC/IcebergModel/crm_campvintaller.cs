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
    
    public partial class crm_campvintaller
    {
        public int id { get; set; }
        public int idcamp { get; set; }
        public string planmayor { get; set; }
        public int idtercero { get; set; }
        public Nullable<System.DateTime> fecasignacion { get; set; }
        public Nullable<System.DateTime> feccontacto { get; set; }
        public Nullable<System.DateTime> fecagendamiento { get; set; }
        public Nullable<bool> finalizada { get; set; }
        public Nullable<int> agente { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public Nullable<bool> estado { get; set; }
        public string razon_inactivo { get; set; }
    
        public virtual icb_terceros icb_terceros { get; set; }
        public virtual icb_vehiculo icb_vehiculo { get; set; }
        public virtual tcamptaller tcamptaller { get; set; }
        public virtual users users { get; set; }
    }
}