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
    
    public partial class factura_modificada
    {
        public int id { get; set; }
        public int id_encabezado { get; set; }
        public string numero_anterior { get; set; }
        public string numero_nuevo { get; set; }
        public int usuario_modifica { get; set; }
        public System.DateTime fecha_modifica { get; set; }
        public string motivo { get; set; }
    
        public virtual encab_documento encab_documento { get; set; }
        public virtual users users { get; set; }
    }
}
