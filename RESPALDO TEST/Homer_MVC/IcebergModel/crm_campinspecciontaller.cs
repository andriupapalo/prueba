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
    
    public partial class crm_campinspecciontaller
    {
        public int id { get; set; }
        public int idencabinspeccion { get; set; }
        public int idtarea { get; set; }
        public int cantidad { get; set; }
        public decimal preciounitario { get; set; }
        public System.DateTime feccontacto { get; set; }
        public bool agendado { get; set; }
        public Nullable<System.DateTime> fecagendado { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
    
        public virtual tencabinspeccion tencabinspeccion { get; set; }
        public virtual ttareasinspeccion ttareasinspeccion { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
    }
}