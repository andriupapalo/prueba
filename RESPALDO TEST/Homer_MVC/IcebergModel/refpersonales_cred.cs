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
    
    public partial class refpersonales_cred
    {
        public int id { get; set; }
        public int credito_id { get; set; }
        public string nombres { get; set; }
        public string apellidos { get; set; }
        public string telefono { get; set; }
        public string celular { get; set; }
        public Nullable<System.DateTime> fec_creacion { get; set; }
        public Nullable<int> userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> userid_actualizacion { get; set; }
    
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
        public virtual v_creditos v_creditos { get; set; }
    }
}
