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
    
    public partial class icb_tpmovimiento
    {
        public int mov_id { get; set; }
        public string mov_cod { get; set; }
        public Nullable<int> mov_consecutivo { get; set; }
        public string mov_nombre { get; set; }
        public string mov_descripcion { get; set; }
        public Nullable<System.DateTime> mov_fecela { get; set; }
        public Nullable<int> usuela { get; set; }
        public bool mov_estado { get; set; }
        public Nullable<int> licencia_id { get; set; }
        public Nullable<System.DateTime> mov_fecha_actualizacion { get; set; }
        public Nullable<int> usuario_actualizacion_id { get; set; }
        public string mov_razon_inactivo { get; set; }
    }
}