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
    
    public partial class asignacion
    {
        public int id { get; set; }
        public int idProspecto { get; set; }
        public int idAsesor { get; set; }
        public bool estado { get; set; }
        public System.DateTime fechaInicio { get; set; }
        public Nullable<System.DateTime> fechaFin { get; set; }
    
        public virtual prospectos prospectos { get; set; }
        public virtual users users { get; set; }
    }
}
