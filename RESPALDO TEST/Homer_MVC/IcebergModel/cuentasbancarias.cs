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
    
    public partial class cuentasbancarias
    {
        public int id { get; set; }
        public int idBanco { get; set; }
        public string cuenta { get; set; }
        public int numeroCuenta { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public int userid_creacion { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public Nullable<int> userid_actualizacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
    
        public virtual bancos bancos { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
    }
}
