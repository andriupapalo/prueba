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
    
    public partial class tcombodetalle
    {
        public int id { get; set; }
        public Nullable<int> idtcombo { get; set; }
        public string tempario { get; set; }
        public Nullable<bool> Estado { get; set; }
    
        public virtual tcombos tcombos { get; set; }
        public virtual ttempario ttempario { get; set; }
    }
}
