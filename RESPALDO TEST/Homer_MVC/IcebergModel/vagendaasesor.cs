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
    
    public partial class vagendaasesor
    {
        public int id { get; set; }
        public Nullable<System.DateTime> fecha { get; set; }
        public Nullable<int> cotizacion { get; set; }
        public Nullable<System.TimeSpan> hora { get; set; }
        public Nullable<int> cliente { get; set; }
        public string placa { get; set; }
        public string notas { get; set; }
        public Nullable<bool> confirmado { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public Nullable<int> motivo { get; set; }
    
        public virtual icb_terceros icb_terceros { get; set; }
    }
}