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
    
    public partial class logfallosesion
    {
        public int id { get; set; }
        public int idusuario { get; set; }
        public System.DateTime fecha { get; set; }
        public int intentos { get; set; }
    
        public virtual users users { get; set; }
    }
}
