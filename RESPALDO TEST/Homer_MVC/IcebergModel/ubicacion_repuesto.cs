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
    
    public partial class ubicacion_repuesto
    {
        public int id { get; set; }
        public string codigo { get; set; }
        public int bodega { get; set; }
        public int ubicacion { get; set; }
        public float stock_minimo { get; set; }
        public float stock_maximo { get; set; }
        public Nullable<float> toma_1 { get; set; }
        public Nullable<int> usu_toma_1 { get; set; }
        public Nullable<float> toma_2 { get; set; }
        public Nullable<int> usu_toma_2 { get; set; }
        public Nullable<float> toma_3 { get; set; }
        public Nullable<int> usu_toma_3 { get; set; }
        public Nullable<System.DateTime> fecha_ultima { get; set; }
        public string tipo_abc { get; set; }
        public Nullable<int> ubicacion2 { get; set; }
        public Nullable<float> stock_seguridad { get; set; }
        public Nullable<int> id_licencia { get; set; }
        public System.DateTime fec_creacion { get; set; }
        public int userid_creacion { get; set; }
        public Nullable<System.DateTime> fec_actualizacion { get; set; }
        public Nullable<int> user_idactualizacion { get; set; }
        public bool estado { get; set; }
        public string razon_inactivo { get; set; }
        public Nullable<int> idarea { get; set; }
        public Nullable<int> conteo { get; set; }
        public int id_estanteria { get; set; }
        public string notaUbicacion { get; set; }
    
        public virtual area_bodega area_bodega { get; set; }
        public virtual bodega_concesionario bodega_concesionario { get; set; }
        public virtual estanterias estanterias { get; set; }
        public virtual icb_referencia icb_referencia { get; set; }
    }
}
