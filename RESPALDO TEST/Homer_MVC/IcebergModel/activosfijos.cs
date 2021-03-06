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
    
    public partial class activosfijos
    {
        public int id { get; set; }
        public string descripcion { get; set; }
        public string placa { get; set; }
        public int idcentro { get; set; }
        public Nullable<int> ubicacion { get; set; }
        public int idempresa { get; set; }
        public int idresponsable { get; set; }
        public int idproveedor { get; set; }
        public System.DateTime fecha_compra { get; set; }
        public bool mantenimiento { get; set; }
        public string numeroserial { get; set; }
        public Nullable<System.DateTime> fecha_vencepoliza { get; set; }
        public string numeropoliza { get; set; }
        public Nullable<int> idaseguradora { get; set; }
        public bool pignorado { get; set; }
        public Nullable<System.DateTime> fecha_vencegarantia { get; set; }
        public string detalle_garantia { get; set; }
        public string orden_compra { get; set; }
        public bool activo { get; set; }
        public int metododepreciacion { get; set; }
        public decimal valoractivo { get; set; }
        public System.DateTime fecha_activacion { get; set; }
        public int meses_depreciacion { get; set; }
        public System.DateTime fecha_findepreciacion { get; set; }
        public Nullable<decimal> valorresidual { get; set; }
        public Nullable<System.DateTime> fecha_baja { get; set; }
        public Nullable<int> capacidadlocal { get; set; }
        public bool depreciable { get; set; }
        public Nullable<int> metododeprecniff { get; set; }
        public Nullable<decimal> valoractivoniff { get; set; }
        public Nullable<System.DateTime> fecha_activacionniif { get; set; }
        public Nullable<int> meses_depreniff { get; set; }
        public Nullable<System.DateTime> fecha_findepreniff { get; set; }
        public Nullable<decimal> valorresidualniif { get; set; }
        public Nullable<System.DateTime> fecha_bajaniif { get; set; }
        public int clasificacion { get; set; }
        public Nullable<int> capacidadniif { get; set; }
        public bool depreciableniif { get; set; }
        public Nullable<int> clasificacionniff { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public Nullable<decimal> constantedepre { get; set; }
        public Nullable<decimal> constantedepreniif { get; set; }
        public Nullable<System.DateTime> fechaactualizacion { get; set; }
        public decimal valordepre { get; set; }
        public decimal valordepreniif { get; set; }
        public bool vendido { get; set; }
        public Nullable<int> motivo { get; set; }
        public bool finalizado { get; set; }
        public int mesesfaltantes { get; set; }
        public int mesesfaltantesniif { get; set; }
    
        public virtual activoclasificacion activoclasificacion { get; set; }
        public virtual activoclasificacion activoclasificacion1 { get; set; }
        public virtual activometodo activometodo { get; set; }
        public virtual activometodo activometodo1 { get; set; }
        public virtual activosfubicacion activosfubicacion { get; set; }
        public virtual centro_costo centro_costo { get; set; }
        public virtual icb_aseguradoras icb_aseguradoras { get; set; }
        public virtual motivobajaactivo motivobajaactivo { get; set; }
        public virtual tablaempresa tablaempresa { get; set; }
        public virtual tercero_proveedor tercero_proveedor { get; set; }
        public virtual users users { get; set; }
        public virtual users users1 { get; set; }
        public virtual users users2 { get; set; }
    }
}
