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
    
    public partial class terceros_direcciones
    {
        public int id { get; set; }
        public int idtercero { get; set; }
        public int ciudad { get; set; }
        public Nullable<int> sector { get; set; }
        public string direccion { get; set; }
    
        public virtual icb_terceros icb_terceros { get; set; }
        public virtual nom_ciudad nom_ciudad { get; set; }
        public virtual nom_sector nom_sector { get; set; }
    }
}
