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
    
    public partial class configuracion_envio_correos
    {
        public int id_configuracion_correo { get; set; }
        public string nombre_remitente { get; set; }
        public string smtp_server { get; set; }
        public int puerto { get; set; }
        public string usuario { get; set; }
        public string password { get; set; }
        public string correo { get; set; }
        public bool activo { get; set; }
    }
}
