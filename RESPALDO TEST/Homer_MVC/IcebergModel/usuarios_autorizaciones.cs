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
    
    public partial class usuarios_autorizaciones
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public int bodega_id { get; set; }
        public Nullable<System.DateTime> fecha_creacion { get; set; }
        public Nullable<int> user_creacion { get; set; }
        public Nullable<System.DateTime> fecha_actualizacion { get; set; }
        public Nullable<int> user_actualizacion { get; set; }
        public Nullable<int> tipoautorizacion { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
    }
}