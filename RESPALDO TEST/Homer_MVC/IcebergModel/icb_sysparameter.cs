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
    
    public partial class icb_sysparameter
    {
        public int syspar_id { get; set; }
        public string syspar_cod { get; set; }
        public string syspar_description { get; set; }
        public string syspar_nombre { get; set; }
        public string syspar_value { get; set; }
        public Nullable<int> sysparid_licencia { get; set; }
        public System.DateTime sysparfec_creacion { get; set; }
        public int sysparuserid_creacion { get; set; }
        public Nullable<System.DateTime> sysparfec_actualizacion { get; set; }
        public Nullable<int> sysparuserid_actualizacion { get; set; }
        public bool syspar_estado { get; set; }
        public string sysparrazoninactivo { get; set; }
    }
}
