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
    
    public partial class icb_tsistemas
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public icb_tsistemas()
        {
            this.icb_tsistemas_operaciones = new HashSet<icb_tsistemas_operaciones>();
            this.icb_tsistemas_ot = new HashSet<icb_tsistemas_ot>();
        }
    
        public int tsis_id { get; set; }
        public string tsis_sistema { get; set; }
        public Nullable<bool> tsis_estado { get; set; }
        public string tsis_razoninactivo { get; set; }
        public Nullable<int> tsis_usuela { get; set; }
        public Nullable<System.DateTime> tsis_fecela { get; set; }
        public Nullable<int> tsis_usumod { get; set; }
        public Nullable<System.DateTime> tsis_fecmod { get; set; }
    
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_tsistemas_operaciones> icb_tsistemas_operaciones { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<icb_tsistemas_ot> icb_tsistemas_ot { get; set; }
    }
}