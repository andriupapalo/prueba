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
    
    public partial class tpcaja_vehiculo
    {
        public int tpcaj_id { get; set; }
        public Nullable<int> tpcajid_licencia { get; set; }
        public System.DateTime tpcajfec_creacion { get; set; }
        public int tpcajuserid_creacion { get; set; }
        public Nullable<System.DateTime> tpcajfec_actualizacion { get; set; }
        public Nullable<int> tpcajuserid_actualizacion { get; set; }
        public string tpcaj_nombre { get; set; }
        public bool tpcaj_estado { get; set; }
        public string tpcajrazoninactivo { get; set; }
    
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
    }
}
