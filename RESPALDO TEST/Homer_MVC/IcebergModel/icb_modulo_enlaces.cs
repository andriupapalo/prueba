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
    
    public partial class icb_modulo_enlaces
    {
        public int enl_id { get; set; }
        public Nullable<int> enl_licencia { get; set; }
        public int enl_modulo { get; set; }
        public int id_modulo_destino { get; set; }
        public System.DateTime enl_feccreacion { get; set; }
        public int enl_usucreacion { get; set; }
        public Nullable<System.DateTime> enl_fecactualizacion { get; set; }
        public Nullable<int> enl_usuactualizacion { get; set; }
        public bool enl_estado { get; set; }
        public string enl_razoninactivo { get; set; }
    }
}
