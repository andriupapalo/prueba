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
    
    public partial class ttareainspeccionprioridad
    {
        public int id { get; set; }
        public int idencabinspeccion { get; set; }
        public int idtareainspeccion { get; set; }
        public int cantidad { get; set; }
        public decimal valor_unitario { get; set; }
        public bool autorizado { get; set; }
        public Nullable<System.DateTime> fecautorizacion { get; set; }
    
        public virtual tencabinspeccion tencabinspeccion { get; set; }
        public virtual ttareasinspeccion ttareasinspeccion { get; set; }
    }
}
