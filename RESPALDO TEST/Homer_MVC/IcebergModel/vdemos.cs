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
    
    public partial class vdemos
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public vdemos()
        {
            this.agenda_demos = new HashSet<agenda_demos>();
            this.parametrizacion_horario = new HashSet<parametrizacion_horario>();
        }
    
        public int id { get; set; }
        public string planmayor { get; set; }
        public string serie { get; set; }
        public Nullable<int> ubicacion { get; set; }
        public string notas { get; set; }
        public string placa { get; set; }
        public bool estado { get; set; }
        public Nullable<int> user_creacion { get; set; }
        public Nullable<int> user_actualizacion { get; set; }
        public Nullable<System.DateTime> fecha_creacion { get; set; }
        public Nullable<System.DateTime> fecha_actualizacion { get; set; }
        public string razon_inactivo { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<agenda_demos> agenda_demos { get; set; }
        public virtual icb_vehiculo icb_vehiculo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<parametrizacion_horario> parametrizacion_horario { get; set; }
    }
}
