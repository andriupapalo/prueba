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
    
    public partial class bodega_usuario_visualizacion
    {
        public long id_usuario_visualizacion { get; set; }
        public int id_bodega { get; set; }
        public int id_usuario { get; set; }
    
        public virtual bodega_concesionario bodega_concesionario { get; set; }
        public virtual users users { get; set; }
    }
}
