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
    
    public partial class logCitastallereepprog
    {
        public int id { get; set; }
        public Nullable<int> idcita { get; set; }
        public Nullable<int> idmovreprog { get; set; }
        public Nullable<System.DateTime> fecha { get; set; }
        public Nullable<int> iduser { get; set; }
    
        public virtual tcitastaller tcitastaller { get; set; }
        public virtual tmotivosreprogcita tmotivosreprogcita { get; set; }
        public virtual users users { get; set; }
    }
}