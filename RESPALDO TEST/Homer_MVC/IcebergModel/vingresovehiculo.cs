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
    
    public partial class vingresovehiculo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public vingresovehiculo()
        {
            this.vdetalleingresovehiculo = new HashSet<vdetalleingresovehiculo>();
            this.vingresovehiculoopcion = new HashSet<vingresovehiculoopcion>();
        }
    
        public int id { get; set; }
        public bool nuevo { get; set; }
        public bool usado { get; set; }
        public string descripcion { get; set; }
        public string tiporespuesta { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public Nullable<int> tipoCheckid { get; set; }
    
        public virtual tipo_Checklist tipo_Checklist { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vdetalleingresovehiculo> vdetalleingresovehiculo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vingresovehiculoopcion> vingresovehiculoopcion { get; set; }
    }
}
