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
    
    public partial class archivo_compra
    {
        public int id_archivo_compra { get; set; }
        public int tipo_compra { get; set; }
        public int bodega { get; set; }
        public System.DateTime fecha { get; set; }
        public string archivo { get; set; }
        public bool vigente { get; set; }
        public int numero_compra { get; set; }
        public string numero_gm { get; set; }
    
        public virtual icb_sedes icb_sedes { get; set; }
        public virtual rtipocompra rtipocompra { get; set; }
    }
}