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
    
    public partial class tdetalleinspeccion
    {
        public int id { get; set; }
        public int idencabinspeccion { get; set; }
        public Nullable<int> iditeminspeccion { get; set; }
        public string respuesta { get; set; }
    
        public virtual tencabinspeccion tencabinspeccion { get; set; }
        public virtual titemsinspeccion titemsinspeccion { get; set; }
    }
}
