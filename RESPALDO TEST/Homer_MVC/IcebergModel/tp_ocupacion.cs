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
    
    public partial class tp_ocupacion
    {
        public int tpocupacion_id { get; set; }
        public string tpocupacion_nombre { get; set; }
        public bool tpocupacion_estado { get; set; }
        public Nullable<int> tpocupacionid_licencia { get; set; }
        public Nullable<System.DateTime> tpocupacionfec_creacion { get; set; }
        public Nullable<int> tpocupacionuserid_creacion { get; set; }
        public Nullable<System.DateTime> tpocupacionfec_actualizacion { get; set; }
        public Nullable<int> tpocupacionuserid_actualizacion { get; set; }
        public string tpocupacion_razoninactivo { get; set; }
    }
}
