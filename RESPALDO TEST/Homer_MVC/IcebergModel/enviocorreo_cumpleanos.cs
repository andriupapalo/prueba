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
    
    public partial class enviocorreo_cumpleanos
    {
        public int id { get; set; }
        public Nullable<int> asesor_id { get; set; }
        public string asunto { get; set; }
        public Nullable<System.DateTime> fecha_envio { get; set; }
        public bool enviado { get; set; }
        public string razon_noenvio { get; set; }
    
        public virtual users users { get; set; }
    }
}