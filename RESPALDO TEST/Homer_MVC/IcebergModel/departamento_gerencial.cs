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
    
    public partial class departamento_gerencial
    {
        public int dpto_id { get; set; }
        public string dpto_nombre { get; set; }
        public bool dpto_estado { get; set; }
        public Nullable<int> dptoid_licencia { get; set; }
        public System.DateTime dptofec_creacion { get; set; }
        public int dptouserid_creacion { get; set; }
        public Nullable<System.DateTime> dptofec_actualizacion { get; set; }
        public Nullable<int> dptouserid_actualizacion { get; set; }
        public string dpto_razoninactivo { get; set; }
    }
}
