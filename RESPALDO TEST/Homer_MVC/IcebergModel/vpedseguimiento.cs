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
    
    public partial class vpedseguimiento
    {
        public int seguimiento_id { get; set; }
        public int pedidoid { get; set; }
        public int tipo { get; set; }
        public string nota { get; set; }
        public System.DateTime fecha { get; set; }
        public Nullable<int> responsable { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public int userid_creacion { get; set; }
        public string motivo { get; set; }
    
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
        public virtual vpedido vpedido { get; set; }
        public virtual vtiposeguimientocot vtiposeguimientocot { get; set; }
    }
}
