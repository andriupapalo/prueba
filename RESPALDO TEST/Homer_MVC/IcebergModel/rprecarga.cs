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
    
    public partial class rprecarga
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public rprecarga()
        {
            this.lineas_documento = new HashSet<lineas_documento>();
        }
    
        public int id { get; set; }
        public string codigo { get; set; }
        public Nullable<int> seq { get; set; }
        public Nullable<System.DateTime> fecha { get; set; }
        public decimal cant_fact { get; set; }
        public Nullable<float> poriva { get; set; }
        public decimal valor_unitario { get; set; }
        public decimal cant_ped { get; set; }
        public string documento { get; set; }
        public string pedidoint { get; set; }
        public string pedidogm { get; set; }
        public decimal valor_total { get; set; }
        public decimal valor_totalenca { get; set; }
        public bool seleccion { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public Nullable<bool> estado { get; set; }
        public string razon_inactivo { get; set; }
        public long numero { get; set; }
        public decimal cant_real { get; set; }
        public bool comprado { get; set; }
        public int difcosto { get; set; }
        public int difcantidad { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<lineas_documento> lineas_documento { get; set; }
    }
}
