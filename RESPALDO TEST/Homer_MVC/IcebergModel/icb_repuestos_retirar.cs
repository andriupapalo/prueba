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
    
    public partial class icb_repuestos_retirar
    {
        public long id_repuesto_retirar { get; set; }
        public string planmayor { get; set; }
        public int id_repuesto { get; set; }
    
        public virtual icb_vehiculo icb_vehiculo { get; set; }
        public virtual tdetallerepuestosot tdetallerepuestosot { get; set; }
    }
}
