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
    
    public partial class contratoscomerciales
    {
        public int idcontrato { get; set; }
        public int tipocontrato { get; set; }
        public string numerocontrato { get; set; }
        public int tercero { get; set; }
        public decimal valorcontrato { get; set; }
        public Nullable<decimal> valordescontado { get; set; }
        public Nullable<int> porceniva { get; set; }
        public System.DateTime fechainicial { get; set; }
        public System.DateTime fechafinal { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public int userid_creacion { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
    
        public virtual tercero_cliente tercero_cliente { get; set; }
    }
}
