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
    
    public partial class vhorarionovedad
    {
        public int id { get; set; }
        public int horarioid { get; set; }
        public System.DateTime fechaini { get; set; }
        public System.DateTime fechafin { get; set; }
        public string motivo { get; set; }
    
        public virtual parametrizacion_horario parametrizacion_horario { get; set; }
    }
}
