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
    
    public partial class anio_modelo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public anio_modelo()
        {
            this.pcotizaciondigital = new HashSet<pcotizaciondigital>();
            this.vlistanuevos = new HashSet<vlistanuevos>();
        }
    
        public int anio_modelo_id { get; set; }
        public string codigo_modelo { get; set; }
        public int anio { get; set; }
        public decimal costosiniva { get; set; }
        public decimal valor { get; set; }
        public string descripcion { get; set; }
        public decimal porcentaje_iva { get; set; }
        public decimal impuesto_consumo { get; set; }
        public decimal precio { get; set; }
        public decimal matricula { get; set; }
        public decimal poliza { get; set; }
        public int concesionarioid { get; set; }
        public int idporcentajeiva { get; set; }
        public int idporcentajeimpoconsumo { get; set; }
        public int idporcentajecompra { get; set; }
        public decimal porcentaje_compra { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<pcotizaciondigital> pcotizaciondigital { get; set; }
        public virtual codigo_iva codigo_iva { get; set; }
        public virtual codigo_iva codigo_iva1 { get; set; }
        public virtual codigo_iva codigo_iva2 { get; set; }
        public virtual modelo_vehiculo modelo_vehiculo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vlistanuevos> vlistanuevos { get; set; }
    }
}